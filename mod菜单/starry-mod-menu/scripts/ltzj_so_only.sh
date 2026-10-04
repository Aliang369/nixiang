#!/usr/bin/env bash
# 雷霆战机：只重编 so，zip 替换 assets/starry_native/libstarry_magic.so（禁止 apktool b）
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/ltzj_so_only.sh
#   STARRY_BASE_APK=... bash scripts/ltzj_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/雷霆战机"
PKG="com.a5game.ltzj.jj"
BASE="${STARRY_BASE_APK:-$OUT/雷霆战机_1.25.31_starry-zipAssets-menu-releaseSigned.apk}"
SIGNED="$OUT/雷霆战机_1.25.31_starry-zipAssets-menu-soOnly-releaseSigned.apk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -q 'STARRY_GAME_LTZJ' "$MK" || { echo "ERROR: enable STARRY_GAME_LTZJ in Android.mk"; exit 1; }
[[ -f "$BASE" ]] || { echo "ERROR: missing base apk: $BASE"; exit 1; }

APP="$STARRY_SRC/app"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8

SO="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: ndk-build did not produce $SO"; exit 1; }

WORK="$(mktemp -d /tmp/ltzj_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE" "$WORK/in.apk"
mkdir -p "$WORK/assets/starry_native"
cp -f "$SO" "$WORK/assets/starry_native/libstarry_magic.so"
(cd "$WORK" && zip -u in.apk assets/starry_native/libstarry_magic.so)

KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
java -jar "$SIGNER" -a "$WORK/in.apk" --overwrite --allowResign \
  --ks "$KS" --ksAlias starry --ksPass starry2026 --ksKeyPass starry2026

mkdir -p "$OUT"
if [[ -f "$WORK/in-aligned-releaseSigned.apk" ]]; then
  cp -f "$WORK/in-aligned-releaseSigned.apk" "$SIGNED"
elif [[ -f "$WORK/in-releaseSigned.apk" ]]; then
  cp -f "$WORK/in-releaseSigned.apk" "$SIGNED"
else
  cp -f "$WORK/in.apk" "$SIGNED"
fi
ls -lh "$SIGNED" "$SO"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  adb push "$SIGNED" /data/local/tmp/starry_ltzj_so.apk
  adb shell "su -c \"pm install -r -d --user 0 /data/local/tmp/starry_ltzj_so.apk\""
fi
echo "done → $SIGNED (base=$(basename "$BASE"); NO apktool)"
