#!/usr/bin/env bash
# 无限升级 (wxsj) — 仅重编 libstarry_magic.so（不打包、不装机）→ app/src/libs/arm64-v8a/
# 用法:
#   WXSJ_DIAG=1 bash scripts/wxsj_so_only.sh   # DIAG 取证 so（只 dump 官方运行时 dll，不替换不崩）
#   bash scripts/wxsj_so_only.sh               # 正式 so（Android.mk 必须已去掉 -DWXSJ_DIAG_DUMP=1）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SO_OUT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
# 教训(2026-09-10)：NDK_OUT/NDK_LIBS_OUT 必须是绝对路径，相对路径被 -C 的 cwd 解析到 app/ 下 → 假成功
OBJ="$STARRY_SRC/app/src/libs_wxsj_obj"
LIBS="$STARRY_SRC/app/src/libs_wxsj_libs"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
[[ -f "$JNI/Android.mk" ]] || { echo "ERROR: missing $JNI/Android.mk"; exit 1; }

grep -qE '^[[:space:]]*LOCAL_CFLAGS[[:space:]]*\+=[[:space:]]*.*-DSTARRY_GAME_WXSJ' "$JNI/Android.mk" \
  || { echo "ERROR: Android.mk 未激活 -DSTARRY_GAME_WXSJ（当前会编成别的游戏）"; exit 1; }
grep -qE '^[[:space:]]*LOCAL_SRC_FILES[[:space:]]*:=.*WxsjMod\.cpp' "$JNI/Android.mk" \
  || { echo "ERROR: Android.mk LOCAL_SRC_FILES 未含 Games/WxsjMod.cpp"; exit 1; }

DIAG="0"
grep -qE '^[[:space:]]*LOCAL_CFLAGS[[:space:]]*\+=[[:space:]]*.*-DWXSJ_DIAG_DUMP=1' "$JNI/Android.mk" && DIAG="1"
if [[ "${WXSJ_DIAG:-0}" == "1" && "$DIAG" != "1" ]]; then
  echo "ERROR: 要编 DIAG so，但 Android.mk 缺 -DWXSJ_DIAG_DUMP=1"; exit 1
fi
if [[ "${WXSJ_DIAG:-0}" != "1" && "$DIAG" == "1" ]]; then
  echo "ERROR: Android.mk 仍带 -DWXSJ_DIAG_DUMP=1 —— 正式 so 禁用 DIAG（先去掉该 flag）"; exit 1
fi

# obj 缓存按配置指纹失效（教训：make 只看 mtime，开关变化会漏旧 .o 进新 so）
CFG_HASH=$( { cat "$JNI/Android.mk" "$JNI/Application.mk" 2>/dev/null; printf 'WXSJ_DIAG=%s\n' "$DIAG"; } | shasum -a 256 | awk '{print $1}' )
mkdir -p "$OBJ" "$LIBS"
if [[ ! -f "$OBJ/.build_stamp" || "$(cat "$OBJ/.build_stamp" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[wxsj-so] 配置指纹变化 → 清 obj/libs 全量重编"
  rm -rf "$OBJ" "$LIBS"
  mkdir -p "$OBJ" "$LIBS"
  printf '%s\n' "$CFG_HASH" > "$OBJ/.build_stamp"
fi

PRE_MD5="$(md5 -q "$SO_OUT" 2>/dev/null || echo none)"

# 🩸 血案(2026-09-23 二次热更)：`jni/Games/WxsjPatchedDll.a` 是**手工 objcopy 产物**，不随
# `jni/targetfuntional_dll.bin` 自动更新 → bin 换了新 dll 但 .a 还是旧版，ndk-build 照旧链旧数据，
# so 里内嵌的仍是旧 dll（且体积/校验全看不出），装机后「用旧补丁 dll 覆盖新官方 dll」= 灾难。
# ⇒ 本脚本从此**每次强制重建 .a**（objcopy 必须在 jni 下用相对文件名，符号名才叫
#   `_binary_targetfuntional_dll_bin_*`），并断言 .a 大小 = bin + 552（.data 偏移 296 + 4 个节头 256）。
BIN_SRC="$JNI/targetfuntional_dll.bin"
[[ -f "$BIN_SRC" ]] || { echo "ERROR: 缺内嵌 dll $BIN_SRC"; exit 1; }
BIN_NOW="$(stat -f%z "$BIN_SRC" 2>/dev/null || stat -c%s "$BIN_SRC")"
OBJCOPY="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-objcopy"
[[ -x "$OBJCOPY" ]] || { echo "ERROR: llvm-objcopy missing at $OBJCOPY"; exit 1; }
echo "[wxsj-so 0/3] objcopy 重建内嵌 dll 静态库（$BIN_NOW B）"
( cd "$JNI" && "$OBJCOPY" -I binary -O elf64-littleaarch64 -B aarch64 \
    targetfuntional_dll.bin Games/WxsjPatchedDll.a )
ARCHIVE="$JNI/Games/WxsjPatchedDll.a"
A_SIZE="$(stat -f%z "$ARCHIVE" 2>/dev/null || stat -c%s "$ARCHIVE")"
if (( A_SIZE != BIN_NOW + 552 )); then
  echo "ERROR: $ARCHIVE 大小 $A_SIZE ≠ 内嵌 dll $BIN_NOW + 552 —— objcopy 未生效（链的会是旧 dll）"
  exit 1
fi
echo "[wxsj-so] 静态库校验: $A_SIZE = $BIN_NOW + 552 ✓"

echo "[wxsj-so 1/3] ndk-build arm64-v8a (DIAG=$DIAG)"
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="$STARRY_SRC/app" \
  APP_BUILD_SCRIPT="$JNI/Android.mk" \
  NDK_APPLICATION_MK="$JNI/Application.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$LIBS" \
  -j8

BUILT="$LIBS/arm64-v8a/libstarry_magic.so"
[[ -f "$BUILT" ]] || { echo "ERROR: missing $BUILT"; exit 1; }

echo "[wxsj-so 2/3] 防呆断言"
LC_ALL=C grep -aq '\[Wxsj\]' "$BUILT" || { echo "ERROR: so 缺 [Wxsj] 日志串"; exit 1; }
# 中文 UTF-8 必须 LC_ALL=C grep -a 按字节搜（strings 默认 ASCII 会漏报）
LC_ALL=C grep -aq 'Toggle_秒杀' "$BUILT" || { echo "ERROR: so 缺菜单串 Toggle_秒杀（Main.cpp 未编入？）"; exit 1; }
LC_ALL=C grep -aq 'targetfuntional' "$BUILT" || { echo "ERROR: so 缺内嵌 dll 名 targetfuntional"; exit 1; }
# 血案防呆(2026-09-23)：内嵌 patched dll 必须 ≤ 官方热更数组长度 kOrigDllLen，
# 否则运行时 HookExec 里 pLen > len → 「embedded dll size ... > array ..., skip」静默跳过
# → 实际跑官方 dll（防封/全部功能失效、封号弹窗裸奔，且菜单开关看不出异常）。
OFFICIAL_LEN="$(grep -oE 'kOrigDllLen[[:space:]]*=[[:space:]]*[0-9]+' "$JNI/Games/WxsjMod.cpp" | grep -oE '[0-9]+$' | head -1)"
[[ -n "$OFFICIAL_LEN" ]] || { echo "ERROR: 无法从 Games/WxsjMod.cpp 解析 kOrigDllLen"; exit 1; }
BIN="$JNI/targetfuntional_dll.bin"
[[ -f "$BIN" ]] || { echo "ERROR: 缺内嵌 dll $BIN"; exit 1; }
BIN_SIZE="$(stat -f%z "$BIN" 2>/dev/null || stat -c%s "$BIN")"
if (( BIN_SIZE > OFFICIAL_LEN )); then
  echo "ERROR: 内嵌 dll $BIN_SIZE B > 官方热更数组 $OFFICIAL_LEN B（超 $((BIN_SIZE - OFFICIAL_LEN)) B）→ 注入必然被跳过，先给补丁瘦身"
  exit 1
fi
echo "[wxsj-so] 内嵌 dll 尺寸校验: $BIN_SIZE ≤ $OFFICIAL_LEN (余量 $((OFFICIAL_LEN - BIN_SIZE)) B)"
if [[ "$DIAG" == "1" ]]; then
  LC_ALL=C grep -aq 'DIAG dumped' "$BUILT" || { echo "ERROR: DIAG so 缺 dump 日志串"; exit 1; }
else
  if LC_ALL=C grep -aq 'DIAG dumped' "$BUILT"; then echo "ERROR: 正式 so 含 DIAG dumped（取证代码未剥离）"; exit 1; fi
fi
POST_MD5="$(md5 -q "$BUILT")"
# 🩸 终局断言：编译产物里必须**逐字节**含当前内嵌 dll（防 .a 过期 / 链接缓存复用旧数据）
python3 - "$BUILT" "$BIN_SRC" <<'PY' || { echo "ERROR: so 内未找到内嵌 dll 的完整字节（.a 过期？）"; exit 1; }
import sys
so = open(sys.argv[1], 'rb').read()
dll = open(sys.argv[2], 'rb').read()
sys.exit(0 if so.find(dll) >= 0 else 1)
PY
echo "[wxsj-so] 内嵌 dll 逐字节存在性校验 ✓"
if [[ "$POST_MD5" == "$PRE_MD5" ]]; then
  echo "WARN: so md5 与构建前相同（$PRE_MD5）—— 确认是否真的重链（无新 .o 时正常）"
fi

echo "[wxsj-so 3/3] 发布 → $SO_OUT"
mkdir -p "$(dirname "$SO_OUT")"
cp -f "$BUILT" "$SO_OUT"
ls -lh "$SO_OUT"
shasum -a 256 "$SO_OUT"
echo "done (DIAG=$DIAG)"
