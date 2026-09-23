#!/usr/bin/env bash
# 无悔华夏 3.8.280 九游 - 私有构建（Android_whrhx.mk）
# 产物：mod菜单/starry-mod-menu/libs_whrhx/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_whrhx"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
OBJ="$STARRY_SRC/obj_whrhx"
STAMP="$OBJ/.build_stamp"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_whrhx.mk" ]] || { echo "ERROR: missing Android_whrhx.mk" >&2; exit 1; }

CFG_HASH="$( (cat "$JNI/Android_whrhx.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] build config changed -> clean obj_whrhx"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_whrhx.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: new objects but so unchanged - stale output suspected" >&2
  exit 1
fi

mkdir -p "$OBJ"
echo "$CFG_HASH" > "$STAMP"

chk() {
  local pattern="$1"
  LC_ALL=C grep -a -q "$pattern" "$SO" || {
    echo "ERROR: so missing [$pattern]" >&2
    exit 1
  }
}
chk "[Whrhx]"
chk "whhx"
chk "GetCountryDataById"
chk "GetCityData"
chk "identity api ready"
chk "research hooks ready"
chk "policy panel gate ready"
chk "PolicyDataMgr.GetPolicyItemShowState"
chk "科研立即完成"

echo "[build_so] done -> $SO"
echo "[build_so] md5 = $(md5 -q "$SO")"
