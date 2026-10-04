#!/usr/bin/env bash
# 山河旅探：只重编 so，zip 进已验外科包（禁止 apktool b）
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/shlt_so_only.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/山河旅探"
PKG="com.itaotuo.shanhelvtan"
SIGNED="$OUT/山河旅探_1.4.38.apk"
BASE="${STARRY_BASE_APK:-$SIGNED}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_SHLT' "$MK" \
  || { echo "ERROR: enable STARRY_GAME_SHLT in Android.mk"; exit 1; }
grep -E 'Games/ShltMod\.cpp' "$MK" || { echo "ERROR: Android.mk must compile ShltMod.cpp"; exit 1; }
[[ -f "$BASE" ]] || { echo "ERROR: missing base surgical apk: $BASE"; exit 1; }

APP="$STARRY_SRC/app"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)"

SO="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
if [[ ! -f "$SO" ]]; then
  SO="$APP/src/libs/arm64-v8a/libstarry_magic.so"
fi
[[ -f "$SO" ]] || { echo "ERROR: ndk-build did not produce libstarry_magic.so"; exit 1; }

mkdir -p "$APP/src/main/jniLibs/arm64-v8a" "$APP/src/main/libs/arm64-v8a" \
         "$APP/src/libs/arm64-v8a" "$APP/libs/arm64-v8a"
cp -f "$SO" "$APP/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
cp -f "$SO" "$APP/src/main/libs/arm64-v8a/libstarry_magic.so"
cp -f "$SO" "$APP/src/libs/arm64-v8a/libstarry_magic.so"
cp -f "$SO" "$APP/libs/arm64-v8a/libstarry_magic.so"

if ! grep -a -q '\[Shlt\]' "$SO" || ! grep -a -q 'LoadAssembly' "$SO"; then
  echo "ERROR: so missing [Shlt] / LoadAssembly"
  strings "$SO" | grep -E 'Shlt|shlt|LoadAssembly|Ltzj|Dwct|Gjc2' | sort -u | head -30
  exit 1
fi
if grep -a -qE '\[Ltzj\]|\[Dwct\]|\[Gjc2\]|\[Pcr\]' "$SO"; then
  echo "ERROR: so still contains other-game tag"; exit 1
fi

WORK="$(mktemp -d /tmp/shlt_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE" "$WORK/in.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -u in.apk lib/arm64-v8a/libstarry_magic.so)

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
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] cover install (no uninstall)"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb push "$SIGNED" /data/local/tmp/starry_shlt_so.apk
  adb shell "su -c \"pm install -r -d --user 0 /data/local/tmp/starry_shlt_so.apk\""
  adb shell "su -c \"rm -f /data/local/tmp/starry_shlt_so.apk\"" 2>/dev/null || true
fi
echo "done → $SIGNED (NO apktool)"
