#!/usr/bin/env bash
# 黑暗领域 v2：私有 mk 构建，独立 obj_haly/libs_haly，避免共享 Android.mk 串游戏。
set -euo pipefail

trap 'rc=$?; echo "ERROR: haly_build_so.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"

# ---- 2026-11-25 Windows 可移植层（照 04_scripts/999秒幸存者/mxcz999_build_so.sh:62-123 标准实现）----
hash256() { if command -v sha256sum >/dev/null 2>&1; then sha256sum | awk '{print $1}'; else shasum -a 256 | awk '{print $1}'; fi; }
md5f()    { if command -v md5sum   >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
ncpu()    { if command -v nproc >/dev/null 2>&1; then nproc; else sysctl -n hw.ncpu 2>/dev/null || echo 8; fi; }
nonascii() { if [ "$(printf '%s' "$1" | LC_ALL=C tr -d '\000-\177' | wc -c | tr -d ' ')" != "0" ]; then echo 1; else echo 0; fi; }
winpath() { if command -v cygpath >/dev/null 2>&1; then cygpath -m -- "$1"; else echo "$1"; fi; }
ndk_ok() {
  [ -x "$1/ndk-build" ] && return 0
  [ -x "$1/prebuilt/windows-x86_64/bin/make.exe" ] && [ -f "$1/build/core/build-local.mk" ] && return 0
  return 1
}
resolve_ndk() {
  local d
  if [ -n "${ANDROID_NDK_HOME:-}" ] && ndk_ok "$ANDROID_NDK_HOME"; then echo "$ANDROID_NDK_HOME"; return 0; fi
  for d in "$NIXIANG"/01_tools/android-sdk/ndk/*; do
    if [ -d "$d" ] && ndk_ok "$d"; then echo "$d"; return 0; fi
  done
  for d in "$HOME"/Library/Android/sdk/ndk/*; do
    if [ -d "$d" ] && ndk_ok "$d"; then echo "$d"; return 0; fi
  done
  return 1
}
NDK="$(resolve_ndk || true)"
ndk_build() {
  if [ -x "$NDK/ndk-build" ]; then
    "$NDK/ndk-build" "$@"
  else
    "$NDK/prebuilt/windows-x86_64/bin/make.exe" -O \
      -f "$NDK/build/core/build-local.mk" SHELL=cmd "$@"
  fi
}
PYBIN="$(command -v python3.11 || command -v python3 || command -v python || true)"
OBJ="$STARRY_SRC/obj_haly"
LIBS="$STARRY_SRC/libs_haly"
SO="$LIBS/arm64-v8a/libstarry_magic.so"
HEADER="$JNI/Games/HalyPatchJs.h"

[[ -n "$NDK" ]] || { echo "ERROR: 找不到可用 NDK（试过 $ANDROID_NDK_HOME 与 01_tools/android-sdk/ndk）" >&2; exit 1; }
[[ -n "$PYBIN" ]] || { echo "ERROR: 找不到 python（gen_haly_patch_header.py 需要）" >&2; exit 1; }
[[ -f "$JNI/Android_haly.mk" ]] || { echo "ERROR: missing Android_haly.mk" >&2; exit 1; }
[[ -f "$JNI/Games/HalyPatch.js" ]] || { echo "ERROR: missing HalyPatch.js" >&2; exit 1; }

"$PYBIN" "$NIXIANG/04_scripts/黑暗领域/gen_haly_patch_header.py"
node --check "$JNI/Games/HalyPatch.js"
node "$NIXIANG/04_scripts/黑暗领域/test_haly_patch.js"

CFG_HASH="$( (cat "$JNI/Android_haly.mk" "$JNI/Application.mk" \
  "$JNI/Games/HalyMod.cpp" "$JNI/Games/HalyMod.h" "$JNI/Games/HalyPatch.js" \
  "$HEADER"; printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | hash256)"
STAMP="$OBJ/.build_stamp"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] build inputs changed -> clean obj_haly"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$([ -f "$SO" ] && md5f "$SO" || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
# GNU Make 4.3(Win32) 会把 "mod菜单" 转成 mod?? → "unable to open output file"。
# 解法（与 surgical_abssnmxt_2.36.sh:27-31 既有经验一致）：输出目录必须纯 ASCII，
# 且放在**工作区内** 13_scratch/黑暗领域_ndk/，编完拷回 libs_haly；禁止用工作区外目录。
# 实测边界（2026-11-25，三次隔离试验）：ndk-build 读含中文的源码路径没问题；
# 只有**编译输出绝对路径含中文**时 clang 报 unable to open output file。
# 所以只需把 NDK_OUT/NDK_LIBS_OUT 指到纯 ASCII 目录（工作区内 13_scratch/haly_ndk），
# 不需要复制 jni、更不需要用工作区外目录。
BUILD_JNI="$JNI"; BUILD_OUT="$OBJ"; BUILD_LIBS="$LIBS"; STAGED=0
if [ "$(nonascii "$JNI")" = "1" ]; then
  STAGE_ROOT="$NIXIANG/13_scratch/haly_ndk"
  BUILD_OUT="$STAGE_ROOT/obj"; BUILD_LIBS="$STAGE_ROOT/libs"
  STAGED=1
  mkdir -p "$BUILD_OUT" "$BUILD_LIBS"
  echo "[build_so] 输出目录改走纯 ASCII: ${STAGE_ROOT}（源码路径不变）"
fi
ndk_build -C "$BUILD_JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_haly.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$(winpath "$BUILD_OUT")" \
  NDK_LIBS_OUT="$(winpath "$BUILD_LIBS")" \
  -j"$(ncpu)" || BUILD_RC=$?
if [ "$STAGED" = "1" ] && [ -f "$BUILD_LIBS/arm64-v8a/libstarry_magic.so" ]; then
  mkdir -p "$LIBS/arm64-v8a"
  cp -f "$BUILD_LIBS/arm64-v8a/libstarry_magic.so" "$SO"
fi
NEW_O="$(find "$BUILD_OUT" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

if [ -n "$NEW_O" ] && [ "$(md5f "$SO")" = "$SO_MD5_BEFORE" ]; then
  echo "ERROR: new object files but so did not change" >&2
  exit 1
fi
mkdir -p "$OBJ" && printf '%s\n' "$CFG_HASH" > "$STAMP"

for token in "starry_haly_v2" "[Haly2]" "skill_summon_request" "skill_passive_trigger" "SdkRewardAdGuard" "__starryHalyV2"; do
  LC_ALL=C grep -aq "$token" "$SO" || { echo "ERROR: so missing token: $token" >&2; exit 1; }
 done

mkdir -p "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a"
cp -f "$SO" "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"

printf '[build_so] so=%s\n[build_so] md5=%s\n[build_so] sha256=%s\n' \
  "$SO" "$(md5f "$SO")" "$(hash256 < "$SO")"
