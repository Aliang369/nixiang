#!/usr/bin/env bash
# 物质与魔法 — 只换 libstarry_magic.so（底包须已是 surgical-nativeMenu 成品）
# 用法: STARRY_INSTALL=1 bash scripts/wzym_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/物质与魔法"
BASE="$OUT/物质与魔法_6.03_surgical-nativeMenu-releaseSigned.apk"
SIGNED="$OUT/物质与魔法_6.03_surgical-nativeMenu-releaseSigned.apk"
PKG="com.huanshits.materialMagic"
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$BASE" ]] || { echo "ERROR: missing $BASE (先跑 surgical_wzym_6.03.sh)"; exit 1; }
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_WZYM' "$MK" \
  || { echo "ERROR: Android.mk not WZYM"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/wzym_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/3] ndk-build"
APP="$STARRY_SRC/app"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)"

SO=""
for cand in \
  "$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so" \
  "$APP/build/intermediates/ndk/obj/local/arm64-v8a/libstarry_magic.so" \
  "$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
do
  if [[ -f "$cand" ]] && grep -a -q '\[Wzym\]' "$cand"; then
    SO="$cand"; break
  fi
done
[[ -n "$SO" ]] || { echo "ERROR: no WZYM libstarry_magic.so"; exit 1; }
echo "  so=$SO"

echo "[2/3] zip -u so + uber resign (debug，与底包同签)"
cp "$BASE" "$WORK/base.apk"
(
  cd "$WORK"
  mkdir -p lib/arm64-v8a
  cp "$SO" lib/arm64-v8a/libstarry_magic.so
  zip -u base.apk lib/arm64-v8a/libstarry_magic.so
)
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/base.apk" "$ALIGNED"
else
  cp "$WORK/base.apk" "$ALIGNED"
fi
mkdir -p "$WORK/signed"
UBER_ARGS=(-a "$ALIGNED" --allowResign --out "$WORK/signed")
[[ -n "$BT" ]] && UBER_ARGS+=(--zipAlignPath "$BT/zipalign")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
SIGNED_SRC="$(ls -t "$WORK/signed"/*Signed*.apk 2>/dev/null | head -1)"
cp -f "$SIGNED_SRC" "$SIGNED"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE=/data/local/tmp/wzym_so_only.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell su -c "pm install -r -d --user 0 $REMOTE"
  adb shell rm -f "$REMOTE" 2>/dev/null || true
  adb shell appops set "$PKG" SYSTEM_ALERT_WINDOW allow 2>/dev/null || true
  echo "installed: $PKG"
fi
echo "DONE: $SIGNED"
