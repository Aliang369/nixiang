#!/usr/bin/env bash
# 地城狩猎 build_so pipeline backend.
# No native game hook: this only builds the Conch bridge/menu state library.
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_dcsl"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_dcsl.mk" ]] || { echo "ERROR: missing $JNI/Android_dcsl.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_dcsl"
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_dcsl.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] build config changed -> clean obj_dcsl"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_dcsl.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: new object files but so bytes unchanged; refusing stale-so build" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

chk() {
  local n
  n=$(LC_ALL=C grep -ac "$1" "$SO" || true)
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO missing '$1' ($2)" >&2; exit 1; }
}
chk "dcsl"               "game tag missing"
chk "Category_基础功能"   "category missing"
chk "Toggle_无限技能"     "skill feature missing"

echo "[build_so] done -> $SO"
echo "[build_so] md5 = $(md5 -q "$SO")  size = $(wc -c < "$SO" | tr -d ' ')"
