#!/usr/bin/env bash
# 我嘎嘎乱杀 — 仅换 libstarry_magic.so（底包须已是可用外科成品）
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NX="$(cd "$ROOT/../.." && pwd)"
PKG=com.sixag.gaga
OUT_DIR="$NX/08_output/我嘎嘎乱杀"
BASE_APK="${WGGLS_BASE_APK:-$OUT_DIR/我嘎嘎乱杀_1.3.6_surgical-releaseSigned.apk}"
NDK="${NDK:-$(ls -d "$HOME/Library/Android/sdk/ndk/"* 2>/dev/null | sort -V | tail -1)}"

[[ -f "$BASE_APK" ]] || { echo "ERROR: 缺少底包 $BASE_APK"; exit 1; }
grep -E 'STARRY_GAME_WGGLS' "$ROOT/app/src/main/jni/Android.mk" >/dev/null \
  || { echo "ERROR: Android.mk 未启用 STARRY_GAME_WGGLS"; exit 1; }

echo "[1] ndk-build STARRY_GAME_WGGLS"
"$NDK/ndk-build" -C "$ROOT/app/src/main/jni" \
  NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)"

SO="$ROOT/app/src/libs/arm64-v8a/libstarry_magic.so"
mkdir -p "$ROOT/app/src/main/jniLibs/arm64-v8a"
cp -f "$SO" "$ROOT/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
grep -a -q '\[Wggls\]' "$SO" || { echo "ERROR: so 未见 [Wggls]"; exit 1; }
grep -a -q 'HandleSessionSec' "$SO" || { echo "ERROR: so 未见 HandleSessionSec"; exit 1; }
grep -a -q 'CanSkipAdWithoutWatching force true' "$SO" || { echo "ERROR: so 未见免广告 CanSkip"; exit 1; }
grep -a -q 'D0A3F9B20BAD9BE5' "$SO" || { echo "ERROR: so 未见正确官方 CodeHash"; exit 1; }
grep -a -q 'spoof CodeHash' "$SO" || { echo "ERROR: so 未见 spoof CodeHash"; exit 1; }

WORKDIR=$(mktemp -d)
trap 'rm -rf "$WORKDIR"' EXIT
cp -f "$BASE_APK" "$WORKDIR/base.apk"
(
  cd "$WORKDIR"
  mkdir -p lib/arm64-v8a
  cp -f "$SO" lib/arm64-v8a/libstarry_magic.so
  zip -u base.apk lib/arm64-v8a/libstarry_magic.so
)

SIGNED="$OUT_DIR/我嘎嘎乱杀_1.3.6_surgical-releaseSigned.apk"
KS="$NX/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
"$BT/zipalign" -f -p 4 "$WORKDIR/base.apk" "$WORKDIR/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORKDIR/aligned.apk"
echo "signed → $SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  adb install -r -d -i com.taptap "$SIGNED"
  echo "installed; log: adb logcat -s StarryMagic:V StarryTrace:V Mod_Menu:V"
fi
