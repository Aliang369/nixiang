#!/usr/bin/env bash
# 境界守望者（com.ap.watcherofrealms 国际服）管线 build_so 阶段后端：私有 Android_wor.mk 直编
# 产物 = mod菜单/starry-mod-menu/libs_wor/arm64-v8a/libstarry_magic.so
#
# 母本：04_scripts/潮汐守望者/cxswz_build_so.sh
#
# 阶段（2026-10-03）：e2 功能阶段（WOR_ENV_STAGE=0）= 无敌 + 伤害倍率，真机验收要看到
#   `[Wor] lib ready (liblogic.so, feature stage) hooked=N tag=wor-e2-20261003`。
# 历史：e1 环境阶段（WOR_ENV_STAGE=1）= kSlots 空表 = 0 hook，仅壳 + 菜单 + 去签名 + 官证回灌；
#   本脚本的阶段自检按 so 内容自动翻转（见文末），历史标签 wor-e1-20261003 已不在自检清单内。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC_REAL="$NIXIANG/mod菜单/starry-mod-menu"

# ---------- ASCII 路径垫片（Windows 上 ndk/make 处理非 ASCII 路径会乱码） ----------
# 本仓库的 mod菜单/ 含中文；ndk-build 的 make 在 Windows 下会把非 ASCII 路径打乱，
# 导致 clang 收到错路径或 make 报 "The system cannot find the file specified."。
# 解决：建一个纯 ASCII 的目录 junction（starry_link）指向真实项目，构建全程走 junction。
STARRY_SRC="$STARRY_SRC_REAL"
case "$STARRY_SRC_REAL" in
  *[!\ -~]*)
    LINK="$NIXIANG/starry_link"
    if [[ ! -d "$LINK" ]]; then
      MSYS2_ARG_CONV_EXCL='*' cmd //c mklink /J "$(cygpath -w "$LINK")" "$(cygpath -w "$STARRY_SRC_REAL")" >/dev/null 2>&1 || true
    fi
    if [[ -d "$LINK" ]]; then
      STARRY_SRC="$LINK"
      echo "[ascii] 走 ASCII junction: $LINK -> $STARRY_SRC_REAL"
    else
      echo "WARN: 无法创建 ASCII junction，继续用中文路径（Windows 上 ndk-build 可能失败）" >&2
    fi
    ;;
esac
JNI="$STARRY_SRC/app/src/main/jni"

# ---------- NDK 探测（macOS / Linux / Windows Git Bash 三平台） ----------
# 判定标准：该目录下必须有 ndk-build 或 ndk-build.cmd
_ndk_has_build() { [[ -x "$1/ndk-build" || -f "$1/ndk-build.cmd" ]]; }
_ndk_resolve() {  # $1 = 可能是 sdk 根 / ndk 目录 / 具体版本目录
  local base="$1"
  _ndk_has_build "$base" && { echo "$base"; return; }
  # 具体版本优先 27.0.12077973（与母本一致），否则取第一个可用的
  local v
  for v in 27.0.12077973; do
    _ndk_has_build "$base/$v" && { echo "$base/$v"; return; }
    _ndk_has_build "$base/ndk/$v" && { echo "$base/ndk/$v"; return; }
  done
  local c
  for c in "$base"/ndk/* "$base"/*; do
    _ndk_has_build "$c" && { echo "$c"; return; }
  done
}

WUSER="${USER:-${USERNAME:-Administrator}}"
NDK=""
for cand in "${ANDROID_NDK_HOME:-}" "${ANDROID_NDK_ROOT:-}" \
            "${ANDROID_SDK_ROOT:-}" "${ANDROID_HOME:-}" \
            "$HOME/Library/Android/sdk" "$HOME/Android/Sdk" \
            "/c/Users/$WUSER/AppData/Local/Android/Sdk" \
            "/mnt/c/Users/$WUSER/AppData/Local/Android/Sdk"; do
  [[ -n "$cand" && -d "$cand" ]] || continue
  NDK="$(_ndk_resolve "$cand")"
  [[ -n "$NDK" ]] && break
done

if _ndk_has_build "${NDK:-/nonexistent}"; then
  if [[ -x "$NDK/ndk-build" ]]; then NDK_BUILD="$NDK/ndk-build"
  else NDK_BUILD="$NDK/ndk-build.cmd"; fi
else
  echo "ERROR: ndk-build 未找到（设 ANDROID_NDK_HOME=/path/to/ndk）" >&2
  exit 1
fi
echo "[ndk] $NDK_BUILD"
[[ -f "$JNI/Android_wor.mk" ]] || { echo "ERROR: missing $JNI/Android_wor.mk" >&2; exit 1; }

OUT_LIBS="$STARRY_SRC/libs_wor"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

# ---------- 可移植垫片 ----------
md5_of() {
  if command -v md5 >/dev/null 2>&1; then md5 -q "$1"
  else md5sum "$1" | awk '{print $1}'; fi
}
size_of() {
  if stat -c%s "$1" >/dev/null 2>&1; then stat -c%s "$1"
  else stat -f%z "$1"; fi
}
sha256_of_stdin() {
  if command -v sha256sum >/dev/null 2>&1; then sha256sum | awk '{print $1}'
  else shasum -a 256 | awk '{print $1}'; fi
}
ncpu() {
  if command -v nproc >/dev/null 2>&1; then nproc
  elif command -v sysctl >/dev/null 2>&1; then sysctl -n hw.ncpu 2>/dev/null || echo 8
  else echo "${NUMBER_OF_PROCESSORS:-8}"; fi
}

OBJ="$STARRY_SRC/obj_wor"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / trace 开关变化 → 清 obj 强制全量重编 ----
CFG_HASH="$( (cat "$JNI/Android_wor.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | sha256_of_stdin)"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_wor 强制全量重编"
  rm -rf "$OBJ"
fi

# NDK_OUT/NDK_LIBS_OUT 必须绝对路径：相对路径会被 -C 的 cwd 解析到 app/ 下（假成功教训）
SO_MD5_BEFORE="$(md5_of "$SO" 2>/dev/null || echo none)"
# ---- STARRY_BUILD_EPOCH 必须在命令行强制传入纯数字 ----
# Application.mk 里 `STARRY_BUILD_EPOCH := $(shell git log -1 --format=%ct 2>/dev/null || date +%s ...)`
# 在 Windows（ndk-build 用 cmd.exe 当 shell）会退化成整个 git commit 文本（含空格 / <email> / 中文），
# 使 -DSTARRY_BUILD_EPOCH=... 裂成多参，其中 `...@users.noreply.github.com>` 里的 > 被 cmd 当重定向，
# 编译直接报 "The system cannot find the file specified."。macOS 上 sh 正常所以母本没暴露。
EPOCH="$(git -C "$NIXIANG" log -1 --format=%ct 2>/dev/null || true)"
[[ "$EPOCH" =~ ^[0-9]+$ ]] || EPOCH="$(date +%s 2>/dev/null || echo 1790184000)"
[[ "$EPOCH" =~ ^[0-9]+$ ]] || EPOCH=1790184000
echo "[epoch] STARRY_BUILD_EPOCH=$EPOCH"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK_BUILD" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_wor.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  STARRY_BUILD_EPOCH="$EPOCH" \
  -j"$(ncpu)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1 || true)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# ---- 链接活动保险：有新 .o 但 so 字节没变 → 旧 so 顶包嫌疑 ----
if [[ -n "$NEW_O" && "$(md5_of "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 归属防呆：本作串必须在、别家标签必须不在（跨会话共享目录）----
# 中文 UTF-8 多字节，strings 默认 ASCII 会漏报 ⇒ LC_ALL=C grep -a 按字节搜
for s in "Category_战斗" "Toggle_无敌" "Slider_伤害倍率:1:10:1" "[Wor]" "liblogic.so" "wor-e2-20261003"; do
  n="$(LC_ALL=C grep -acF -- "$s" "$SO" || true)"
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含本作串 ${s}（旧 so 混入或源码未编译）" >&2; exit 1; }
done
for s in "[Cxswz]" "[Jjdm]" "[Gyzw]" "[Hxwj]" "[Blfx]" "[Sjzz]" "[WbjY]" "[Wjtxd]"; do
  # -F：标签里的 [] 是字面字符，否则会被 grep 当字符类（曾假报含别家标签）
  if LC_ALL=C grep -aqF -- "$s" "$SO"; then
    echo "ERROR: $SO 含别家标签 ${s}（libs_wor 被并行会话覆盖）" >&2
    exit 1
  fi
done

# ---- 阶段自检（环境阶段 / 功能阶段二选一，按源码实际状态自动判定）----
# 判据用 "ENV stage e1"（TryInit 里一定会执行到的日志）。
# 不要用 InstallBattleHooks 里那句 "env stage 0-hook: kSlots disabled"：
#   环境阶段该函数从不被调用，-ffunction-sections + --gc-sections 会把它整段连同
#   它的字符串字面量一起回收，so 里查不到（实测 confirmed）。
SO_ENV_N="$(LC_ALL=C grep -acF -- "ENV stage e1" "$SO" || true)"
SO_FEAT_N="$(LC_ALL=C grep -acF -- "feature stage" "$SO" || true)"
if [[ "$SO_ENV_N" -ge 1 ]]; then
  echo "[stage] 环境阶段 so（WOR_ENV_STAGE=1，0 hook）"
  [[ "$SO_FEAT_N" == "0" ]] \
    || { echo "ERROR: 环境阶段 so 里混进了功能阶段标记（feature stage）" >&2; exit 1; }
  for s in "DMG x%d" "INV e=%p"; do
    LC_ALL=C grep -aqF -- "$s" "$SO" \
      && { echo "ERROR: 环境阶段 so 里混进了功能阶段日志（${s}）" >&2; exit 1; }
  done
  if LC_ALL=C grep -aqF -- "Slider_伤害倍率:1:100:1" "$SO"; then
    echo "ERROR: $SO 仍含旧 1:100 滑条" >&2; exit 1
  fi
else
  echo "[stage] 功能阶段 so"
  LC_ALL=C grep -aqF -- "feature stage" "$SO" \
    || { echo "ERROR: $SO 未见功能阶段标记（源码未切到功能阶段）" >&2; exit 1; }
  LC_ALL=C grep -aqF -- "DMG x%d" "$SO" \
    || { echo "ERROR: $SO 未见伤害倍率改写日志（功能未编译进去）" >&2; exit 1; }
  LC_ALL=C grep -aqF -- "INV e=%p" "$SO" \
    || { echo "ERROR: $SO 未见无敌改写日志（功能未编译进去）" >&2; exit 1; }
  LC_ALL=C grep -aqF -- "Slider_伤害倍率:1:10:1" "$SO" \
    || { echo "ERROR: $SO 伤害倍率滑条不是 1:10（菜单串与需求不一致）" >&2; exit 1; }
fi

# ---- 同步到 gradle 实际取用的 jniLibs（共享路径，必须字节一致）----
SO_MD5="$(md5_of "$SO")"
for d in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
         "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
         "$STARRY_SRC/app/libs/arm64-v8a"; do
  [[ -d "$d" ]] || continue
  cp -f "$SO" "$d/libstarry_magic.so"
done
for d in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
         "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
         "$STARRY_SRC/app/libs/arm64-v8a"; do
  [[ -f "$d/libstarry_magic.so" ]] || continue
  m="$(md5_of "$d/libstarry_magic.so")"
  [[ "$m" == "$SO_MD5" ]] || { echo "ERROR: $d 同步失败（$m != ${SO_MD5}）" >&2; exit 1; }
done

echo "[build_so] done → $SO ($SO_MD5, $(size_of "$SO") B)；jniLibs 已同步"
