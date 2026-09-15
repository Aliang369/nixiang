#!/usr/bin/env bash
# 黑暗领域 v2：私有 mk 构建，独立 obj_haly/libs_haly，避免共享 Android.mk 串游戏。
set -euo pipefail

trap 'rc=$?; echo "ERROR: haly_build_so.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OBJ="$STARRY_SRC/obj_haly"
LIBS="$STARRY_SRC/libs_haly"
SO="$LIBS/arm64-v8a/libstarry_magic.so"
HEADER="$JNI/Games/HalyPatchJs.h"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_haly.mk" ]] || { echo "ERROR: missing Android_haly.mk" >&2; exit 1; }
[[ -f "$JNI/Games/HalyPatch.js" ]] || { echo "ERROR: missing HalyPatch.js" >&2; exit 1; }

python3.11 "$NIXIANG/04_scripts/黑暗领域/gen_haly_patch_header.py"
node --check "$JNI/Games/HalyPatch.js"
node "$NIXIANG/04_scripts/黑暗领域/test_haly_patch.js"

CFG_HASH="$( (cat "$JNI/Android_haly.mk" "$JNI/Application.mk" \
  "$JNI/Games/HalyMod.cpp" "$JNI/Games/HalyMod.h" "$JNI/Games/HalyPatch.js" \
  "$HEADER"; printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
STAMP="$OBJ/.build_stamp"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] build inputs changed -> clean obj_haly"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_haly.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
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
  "$SO" "$(md5 -q "$SO")" "$(shasum -a 256 "$SO" | awk '{print $1}')"
