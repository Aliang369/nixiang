#!/usr/bin/env bash
# 黑暗领域 — 仅换 libstarry_magic.so（底包须已是可用外科成品）
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NX="$(cd "$ROOT/../.." && pwd)"
PKG=com.haly.tap
OUT_DIR="$NX/08_output/黑暗领域"
BASE_APK="${HALY_BASE_APK:-$OUT_DIR/黑暗领域_1.0_starry.apk}"
if [[ ! -f "$BASE_APK" && -f "$OUT_DIR/黑暗领域_1.0_surgical-releaseSigned.apk" ]]; then
  BASE_APK="$OUT_DIR/黑暗领域_1.0_surgical-releaseSigned.apk"
fi
NDK="${NDK:-$(ls -d "$HOME/Library/Android/sdk/ndk/"* 2>/dev/null | sort -V | tail -1)}"

if [[ ! -f "$BASE_APK" ]]; then
  echo "ERROR: 缺少底包 $BASE_APK — 先跑 scripts/surgical_haly_1.0.sh" >&2
  exit 1
fi

echo "[1] ndk-build STARRY_GAME_HALY"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_HALY' \
  "$ROOT/app/src/main/jni/Android.mk" || {
  echo "ERROR: Android.mk 未启用 STARRY_GAME_HALY" >&2
  exit 1
}
"$NDK/ndk-build" -C "$ROOT/app/src/main/jni" \
  NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk -j8

SO="$ROOT/app/src/libs/arm64-v8a/libstarry_magic.so"
for dest in \
  "$ROOT/app/src/main/jniLibs/arm64-v8a" \
  "$ROOT/app/src/main/libs/arm64-v8a" \
  "$ROOT/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO" "$dest/libstarry_magic.so"
done
if ! grep -a -F -q '[Haly]' "$SO"; then
  echo "ERROR: so 自检失败，未见 [Haly]" >&2
  exit 1
fi
if grep -a -qE '\[Gjc2\]|\[Dwct\]|\[Shhj\]|gjc2intl' "$SO"; then
  echo "ERROR: so 仍含其它游戏标签" >&2
  exit 1
fi

WORKDIR=$(mktemp -d)
trap 'rm -rf "$WORKDIR"' EXIT
cp -f "$BASE_APK" "$WORKDIR/base.apk"
(
  cd "$WORKDIR"
  mkdir -p lib/arm64-v8a
  cp -f "$SO" lib/arm64-v8a/libstarry_magic.so
  zip -u base.apk lib/arm64-v8a/libstarry_magic.so
)

UNSIGNED="$WORKDIR/base.apk"
SIGNED="$OUT_DIR/黑暗领域_1.0_starry.apk"
mkdir -p "$OUT_DIR"
KS="$NX/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
if [[ -n "$BT" && -f "$KS" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORKDIR/aligned.apk"
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$WORKDIR/aligned.apk"
else
  java -jar "$NX/01_tools/uber-apk-signer.jar" -a "$UNSIGNED" --out "$OUT_DIR" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT_DIR"/*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
rm -f "$OUT_DIR/"*.idsig 2>/dev/null || true
echo "signed → $SIGNED"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  adb push "$SIGNED" /data/local/tmp/haly_so.apk
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb install -r -d "$SIGNED" 2>/dev/null || adb shell pm install -r -d --user 0 /data/local/tmp/haly_so.apk 2>/dev/null || true
  adb shell rm -f /data/local/tmp/haly_so.apk 2>/dev/null || true
  echo "installed; check: adb logcat -s StarryApp:V StarryMagic:V | grep -E 'haly|Haly'"
fi
