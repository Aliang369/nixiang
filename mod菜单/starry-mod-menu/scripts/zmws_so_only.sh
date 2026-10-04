#!/usr/bin/env bash
# 造梦无双：只重编 so → zip 进已有星光包 → 重签覆盖安装
# 用法: STARRY_INSTALL=1 bash scripts/zmws_so_only.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/造梦无双"
SIGNED="$OUT/造梦无双_1.33_starry-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
PKG="com.xm4399.zmws.m4399"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$SIGNED" ]] || { echo "ERROR: missing $SIGNED (先跑 inject_zmws_1.33.sh)"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing"; exit 1; }
grep -q 'STARRY_GAME_ZMWS' "$STARRY_SRC/app/src/main/jni/Android.mk" \
  || { echo "ERROR: Android.mk not ZMWS"; exit 1; }

WORK="$(mktemp -d /tmp/zmws_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' \
  "$STARRY_SRC/" "$WORK/starry/"
APP="$WORK/starry/app"

echo "[so-only 1/4] ndk-build arm64"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8

SO="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: missing $SO"; exit 1; }
ls -lh "$SO"

echo "[so-only 2/4] replace so inside APK"
cp -f "$SIGNED" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -u -q base.apk "lib/arm64-v8a/libstarry_magic.so")

echo "[so-only 3/4] resign"
java -jar "$SIGNER" -a "$WORK/base.apk" --overwrite --allowResign >/dev/null
SIGNED_SRC=""
for c in \
  "$WORK/base-aligned-debugSigned.apk" \
  "$WORK/base-aligned-releaseSigned.apk" \
  "$WORK/base-debugSigned.apk" \
  "$WORK/base-releaseSigned.apk" \
  "$WORK/base.apk"; do
  if [[ -f "$c" ]]; then SIGNED_SRC="$c"; break; fi
done
cp -f "$SIGNED_SRC" "$SIGNED"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (STARRY_INSTALL=1 to install)"
  echo "done → $SIGNED"
  exit 0
fi

echo "[so-only 4/4] adb install"
adb push "$SIGNED" /data/local/tmp/starry_zmws_so.apk
if ! adb shell su -c "pm install -r -d --user 0 /data/local/tmp/starry_zmws_so.apk"; then
  adb install -r --no-incremental "$SIGNED"
fi
adb shell am force-stop "$PKG" 2>/dev/null || true
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done → $SIGNED"
echo "log: adb logcat -s StarryMagic:V Zmws:V Mod_Menu:V"
