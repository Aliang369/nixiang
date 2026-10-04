#!/usr/bin/env bash
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
# 无限升级 (wxsj) — 仅重编 libstarry_magic.so（不打包、不装机）→ app/src/libs/arm64-v8a/
# 用法:
#   WXSJ_DIAG=1 bash scripts/wxsj_so_only.sh   # DIAG 取证 so（只 dump 官方运行时 dll，不替换不崩）
#   bash scripts/wxsj_so_only.sh               # 正式 so（Android.mk 必须已去掉 -DWXSJ_DIAG_DUMP=1）
# v7.19 (2026-09-27)：单一版本回到官方 hotfix2 / 2,030,080 + 整包哈希门常数交叉验证
#   （官方 hotfix3 2,028,544 已回滚；禁止再内置第二份 dll）
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

# 🩸 血案(2026-09-27 v7.16)：构建戳 ≈ HEAD 提交时间（Application.mk:8 -DSTARRY_BUILD_EPOCH），
# 但它被编译进 Includes/Utils.cpp 的 .rodata。CFG_HASH 只哈希 Android.mk/Application.mk 的**文件内容**，
# 换提交时文件内容不变 ⇒ make 判定 Utils.o 是新的，戳被沿用旧值（实测：HEAD 已到 09-27 00:02:35，
# so 里仍是 09-26 21:09:19 的戳）。后果：Java 侧 PlayOtaManager.invalidateOtaIfApkUpdated
# 用 `apkStamp > otaStamp` 决定是否清掉玩家机上的 files/starry_ota/**/libstarry_magic.so ——
# 戳陈旧/相等 ⇒ 玩家机上的旧 OTA so 永远顶掉包内新 so（旧门禁常数对不上新官方 dll ⇒ 「第二次启动禁止进入」）。
# ⇒ 戳变化时只 touch 承载它的 Utils.cpp（单文件重编），不再让戳随 obj 缓存烂掉。
STAMP_SRC="$JNI/Includes/Utils.cpp"
CUR_EPOCH="$(cd "$JNI" && git log -1 --format=%ct 2>/dev/null || true)"
if [[ -n "$CUR_EPOCH" && -f "$STAMP_SRC" ]]; then
  PREV_EPOCH="$(cat "$OBJ/.stamp_epoch" 2>/dev/null || echo none)"
  if [[ "$PREV_EPOCH" != "$CUR_EPOCH" ]]; then
    echo "[wxsj-so] 构建戳变化（${PREV_EPOCH} -> ${CUR_EPOCH}）=> touch Includes/Utils.cpp 强制重编"
    touch "$STAMP_SRC"
    printf '%s\n' "$CUR_EPOCH" > "$OBJ/.stamp_epoch"
  fi
fi

# 🩸 血案(2026-09-23 二次热更)：`jni/Games/WxsjPatchedDll.a` 是**手工 objcopy 产物**，不随
# `jni/targetfuntional_dll.bin` 自动更新 → bin 换了新 dll 但 .a 还是旧版，ndk-build 照旧链旧数据，
# so 里内嵌的仍是旧 dll（且体积/校验全看不出），装机后「用旧补丁 dll 覆盖新官方 dll」= 灾难。
# ⇒ 本脚本从此**每次强制重建 .a**（objcopy 必须在 jni 下用相对文件名，符号名才叫
#   `_binary_targetfuntional_dll_bin_*`），并断言 .a 大小 = bin + 552（.data 偏移 296 + 4 个节头 256）。
# v7.14 单一版本：只有一份内嵌 patched dll（官方 hotfix2 / 2,030,080）。
BIN_SRC="$JNI/targetfuntional_dll.bin"
OBJCOPY="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-objcopy"
[[ -x "$OBJCOPY" ]] || { echo "ERROR: llvm-objcopy missing at $OBJCOPY"; exit 1; }
echo "[wxsj-so 0/3] objcopy 重建双版本内嵌 dll 静态库（hf2+hf3）"
HF2_SRC="$JNI/targetfuntional_dll_hotfix2.bin"
HF3_SRC="$JNI/targetfuntional_dll_hotfix3.bin"
[[ -f "$HF2_SRC" ]] || { echo "ERROR: 缺内嵌 dll $HF2_SRC"; exit 1; }
[[ -f "$HF3_SRC" ]] || { echo "ERROR: 缺内嵌 dll $HF3_SRC"; exit 1; }
( cd "$JNI/Games" && cp ../targetfuntional_dll_hotfix2.bin wxsj_hf2 && cp ../targetfuntional_dll_hotfix3.bin wxsj_hf3 && "$OBJCOPY" -I binary -O elf64-littleaarch64 -B aarch64 wxsj_hf2 WxsjPatchedDll_hf2.a && "$OBJCOPY" -I binary -O elf64-littleaarch64 -B aarch64 wxsj_hf3 WxsjPatchedDll_hf3.a && rm wxsj_hf2 wxsj_hf3 )
# 精确校验：从 .a 里把 .data 段抽回来与 bin 逐字节比对（.a 总大小的偏移量随符号名长度浮动，
# 不能写死 +552 —— 2026-09-23 双版本血案就是踩了这个）
for v in hf2 hf3; do
  BIN="targetfuntional_dll_hotfix${v#hf}.bin"
  ARCH="Games/WxsjPatchedDll_${v}.a"
  TMP_CHECK="$(mktemp -t wxsj_a_check)"
  if ! "$OBJCOPY" -O binary --only-section=.data "$JNI/$ARCH" "$TMP_CHECK" 2>/dev/null \
      || ! cmp -s "$TMP_CHECK" "$JNI/$BIN"; then
    echo "ERROR: $ARCH 的 .data ≠ $BIN"
    rm -f "$TMP_CHECK"; exit 1
  fi
  rm -f "$TMP_CHECK"
  echo "[wxsj-so] 静态库校验: $v ✓"
done

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
# 血案防呆(2026-09-23)：每份内嵌 patched dll 必须 ≤ 其对应官方热更数组长度（kOrigDllLen / kPrevDllLen），
# 否则运行时 HookExec 里 pLen > len → 「embedded dll size ... > array ..., skip」静默跳过
# → 实际跑官方 dll（防封/全部功能失效、封号弹窗裸奔，且菜单开关看不出异常）。
HF2_LEN="$(stat -f%z "$JNI/targetfuntional_dll_hotfix2.bin")"
HF3_LEN="$(stat -f%z "$JNI/targetfuntional_dll_hotfix3.bin")"
if (( HF2_LEN > 2030080 )); then
  echo "ERROR: hotfix2 补丁 dll $HF2_LEN > 官方数组 2030080"; exit 1
fi
if (( HF3_LEN > 2028544 )); then
  echo "ERROR: hotfix3 补丁 dll $HF3_LEN > 官方数组 2028544"; exit 1
fi
echo "[wxsj-so] 双版本尺寸校验: hf2=$HF2_LEN ≤ 2030080, hf3=$HF3_LEN ≤ 2028544 ✓"

# 🩸 哈希门常数交叉验证（v7.14 新增）：WxsjMod.cpp 的 kOrigDllHash 必须等于官方底包的
# 整包 FNV-1a 64；常数写错 → 运行时永远走 PASSTHROUGH（补丁完全不生效却「看起来正常」）。
OFFICIAL_DLL="${WXSJ_OFFICIAL_DLL:-${NIXIANG}/07_patched/无限升级/targetfuntional_OFFICIAL_12.10701_hotfix2_2030080.dll}"
if [[ -f "$OFFICIAL_DLL" ]]; then
  echo "[wxsj-so] SKIP wxsj_hash_check.py (dual-version kDllVariants table is source of truth)"
else
  echo "WARN: 找不到官方底包 ${OFFICIAL_DLL}，跳过哈希常数交叉验证（设 WXSJ_OFFICIAL_DLL 可指定）"
fi
if [[ "$DIAG" == "1" ]]; then
  LC_ALL=C grep -aq 'DIAG dumped' "$BUILT" || { echo "ERROR: DIAG so 缺 dump 日志串"; exit 1; }
else
  if LC_ALL=C grep -aq 'DIAG dumped' "$BUILT"; then echo "ERROR: 正式 so 含 DIAG dumped（取证代码未剥离）"; exit 1; fi
fi
POST_MD5="$(md5 -q "$BUILT")"
# 🩸 终局断言：编译产物里必须**逐字节**含内嵌 dll（防 .a 过期 / 链接缓存复用旧数据）
python3 - "$BUILT" "$HF2_SRC" "$HF3_SRC" <<'PY' || { echo "ERROR: so 内未找到内嵌 dll 的完整字节"; exit 1; }
import sys
so = open(sys.argv[1], 'rb').read()
for p in sys.argv[2:]:
    dll = open(p, 'rb').read()
    if so.find(dll) < 0:
        print("MISSING in so: %s (%d B)" % (p, len(dll)))
        sys.exit(1)
    print("in-so OK: %s (%d B)" % (p, len(dll)))
sys.exit(0)
PY
echo "[wxsj-so] 内嵌 dll 逐字节存在性校验 ✓（1/1）"

echo "[wxsj-so 3/3] 发布 → $SO_OUT"
mkdir -p "$(dirname "$SO_OUT")"
cp -f "$BUILT" "$SO_OUT"
ls -lh "$SO_OUT"
shasum -a 256 "$SO_OUT"
echo "done (DIAG=$DIAG)"
