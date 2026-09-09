#!/usr/bin/env bash
# 生存33天 — 仅换 libstarry_magic.so（底包须已是可用外科成品）
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NX="$(cd "$ROOT/../.." && pwd)"
PKG=com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive
OUT_DIR="$NX/08_output/生存33天"
BASE_APK="${SC33_BASE_APK:-$OUT_DIR/生存33天_1.0.23_surgical-releaseSigned.apk}"
NDK="${NDK:-$(ls -d "$HOME/Library/Android/sdk/ndk/"* 2>/dev/null | sort -V | tail -1)}"

if [[ ! -f "$BASE_APK" ]]; then
  echo "ERROR: 缺少底包 $BASE_APK — 先跑 surgical_sc33_1.0.23.sh" >&2
  exit 1
fi

echo "[1] ndk-build STARRY_GAME_SC33 (STARRY_TRACE=${STARRY_TRACE:-0})"
grep -q 'STARRY_GAME_SC33' "$ROOT/app/src/main/jni/Android.mk" || {
  echo "ERROR: Android.mk 未启用 STARRY_GAME_SC33" >&2
  exit 1
}
NDK_FLAGS=(NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk -j8)
if [[ "${STARRY_TRACE:-0}" == "1" ]]; then
  NDK_FLAGS+=(STARRY_TRACE=1)
else
  NDK_FLAGS+=(SC33_SPLICE_ENABLED=1)
fi
if [[ "${SC33_SPLICE_ENABLED:-0}" == "1" ]]; then
  NDK_FLAGS+=(SC33_SPLICE_ENABLED=1)
fi
"$NDK/ndk-build" -C "$ROOT/app/src/main/jni" "${NDK_FLAGS[@]}"

SO="$ROOT/app/src/libs/arm64-v8a/libstarry_magic.so"
cp -f "$SO" "$ROOT/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
if ! strings "$SO" | grep -q '\[Sc33\]'; then
  echo "ERROR: so 自检失败，未见 [Sc33]" >&2
  exit 1
fi

WORKDIR=$(mktemp -d)
trap 'rm -rf "$WORKDIR"' EXIT
cp -f "$BASE_APK" "$WORKDIR/base.apk"
(
  cd "$WORKDIR"
  mkdir -p lib/arm64-v8a
  cp -f "$SO" lib/arm64-v8a/libstarry_magic.so
  zip -q -u base.apk lib/arm64-v8a/libstarry_magic.so
)

if [[ "${STARRY_UPDATE_DEX:-1}" == "1" ]]; then
  echo "[2b] update classes11.dex (StarryApp→JMYBT + getAtkMul)"
  DEX_WORK=$(mktemp -d)
  bash "$ROOT/scripts/sc33_update_classes11.sh" "$DEX_WORK/classes11.dex"
  cp -f "$DEX_WORK/classes11.dex" "$WORKDIR/classes11.dex"
  (cd "$WORKDIR" && zip -q -u base.apk classes11.dex)
  rm -rf "$DEX_WORK"
fi

UNSIGNED="$WORKDIR/base.apk"
SIGNED="$OUT_DIR/生存33天_1.0.23_surgical-releaseSigned.apk"
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
  export ANDROID_SERIAL="${ANDROID_SERIAL:-f9a116a2}"
  adb push "$SIGNED" /data/local/tmp/sc33_so.apk
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell su -c "pm install -r -d --user 0 /data/local/tmp/sc33_so.apk"
  adb shell rm -f /data/local/tmp/sc33_so.apk 2>/dev/null || true
  echo "installed; check: adb logcat -s StarryApp:V StarryMagic:V Sc33:V"
fi
