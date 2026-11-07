#!/usr/bin/env bash
# 暗影格斗3 国际服 1.46.1 — so 构建（私有 Android_sf3g.mk，modern 注册表）
# 产物 = mod菜单/starry-mod-menu/libs_sf3g/arm64-v8a/libstarry_magic.so
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_sf3g"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
OBJ="$STARRY_SRC/obj_sf3g"
STAMP="$OBJ/.build_stamp"

[ -x "$NDK/ndk-build" ] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[ -f "$JNI/Android_sf3g.mk" ] || { echo "ERROR: missing Android_sf3g.mk" >&2; exit 1; }

CFG_HASH="$( (cat "$JNI/Android_sf3g.mk" "$JNI/Application.mk"; printf 'TRACE=%s\n' "${STARRY_TRACE:-}") | shasum -a 256 | awk '{print $1}')"
if [ -f "$STAMP" ] && [ "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]; then
  echo "[stamp] 构建配置变化 → 清 obj_sf3g 全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_sf3g.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[ "$BUILD_RC" -eq 0 ] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[ -f "$SO" ] || { echo "ERROR: build produced no $SO" >&2; exit 1; }
if [ -n "$NEW_O" ] && [ "$(md5 -q "$SO")" = "$SO_MD5_BEFORE" ]; then
  echo "ERROR: 产生了新 .o 但 so 未变化（旧 so 顶包）" >&2; exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

TAG_HIT=$(LC_ALL=C grep -ac "sf3g" "$SO" || true)
[ "$TAG_HIT" -ge 1 ] || { echo "ERROR: so 不含 sf3g 标签" >&2; exit 1; }
while IFS= read -r s; do
  HIT=$(LC_ALL=C grep -ac "$s" "$SO" || true)
  [ "$HIT" -ge 1 ] || { echo "ERROR: so 不含菜单串 $s" >&2; exit 1; }
done <<'STRINGS'
Category_功能
Slider_伤害倍率:1:100:1
Toggle_上帝模式
STRINGS
for bad in "Slider_战斗速度" "Toggle_定怪" "com.wanda.sf3.wd"; do
  HIT=$(LC_ALL=C grep -ac "$bad" "$SO" || true)
  [ "$HIT" -eq 0 ] || { echo "ERROR: so 含非本包串 $bad" >&2; exit 1; }
done

echo "[build_so] done → $SO ($(md5 -q "$SO"))"
echo "[build_so] size = $(stat -f '%z' "$SO")"
