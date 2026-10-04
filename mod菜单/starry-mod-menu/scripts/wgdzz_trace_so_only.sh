#!/usr/bin/env bash
# 王国大作战 B 路线：仅重编 STARRY_TRACE so 并 zip 进 trace 外科包
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/wgdzz_trace_so_only.sh
#   BASE_APK=08_output/.../surgical-traceSigned.apk STARRY_INSTALL=1 bash scripts/wgdzz_trace_so_only.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/王国大作战前线"
BASE_APK="${BASE_APK:-$OUT/王国大作战前线_1.1.7646_surgical-traceSigned.apk}"
SIGNED="$OUT/王国大作战前线_1.1.7646_surgical-traceSigned.apk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -q 'STARRY_GAME_WGDZZ' "$MK" || { echo "ERROR: enable STARRY_GAME_WGDZZ in Android.mk"; exit 1; }
[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk $BASE_APK"; exit 1; }

echo "[1/3] ndk-build STARRY_TRACE=1"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  STARRY_TRACE=1 -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")

SO="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: missing $SO"; exit 1; }
strings "$SO" | grep -q '\[Wgdzz\]' || { echo "ERROR: so missing [Wgdzz] tag"; exit 1; }
strings "$SO" | grep -q 'System.exit' || { echo "ERROR: so missing Java exit trace"; exit 1; }

for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done

echo "[2/3] zip replace libstarry_magic.so"
WORK="$(mktemp -d /tmp/wgdzz_trace_so.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE_APK" "$WORK/in.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -q -u in.apk lib/arm64-v8a/libstarry_magic.so)

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]] && BT="$HOME/Library/Android/sdk/build-tools/$v" && break
done
[[ -n "$BT" ]] || { echo "ERROR: zipalign not found"; exit 1; }

"$BT/zipalign" -f -p 4 "$WORK/in.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
rm -f "$OUT/"*.idsig 2>/dev/null || true
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  adb shell am force-stop com.xr.wgdzztap 2>/dev/null || true
  adb install -r -d "$SIGNED"
  echo "[install] cover ok"
  echo "log: adb logcat -c && adb logcat -s StarryTrace StarryMagic:V WgdzzAppComponentFact:V"
fi
echo "DONE $SIGNED"
