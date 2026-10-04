#!/usr/bin/env bash
# 王国大作战 B 路线：签后补全官方渠道 SigningBlock（0x42726577 1600B）并重签 v2/v3
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/wgdzz_channel_patch.sh
#   NO_RESIGN=1 STARRY_INSTALL=1 bash scripts/wgdzz_channel_patch.sh   # 签后扩展，禁止 apksigner 二次重签（会截回 920B）
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/王国大作战前线"
DONOR="${WGDZZ_DONOR_APK:-$NIXIANG/02_apks/王国大作战前线/base_1.1.7646.apk}"
BASE_APK="${BASE_APK:-$OUT/王国大作战前线_1.1.7646_surgical-kwK1-1sSigned.apk}"
SIGNED="$OUT/王国大作战前线_1.1.7646_surgical-kwK1-channelSigned.apk"
PATCH_PY="$NIXIANG/work/wgdzz_b_route/preserve_channel_signblock.py"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PKG="com.xr.wgdzztap"

[[ -f "$PATCH_PY" ]] || { echo "ERROR: missing $PATCH_PY"; exit 1; }
[[ -f "$DONOR" ]] || { echo "ERROR: missing donor $DONOR"; exit 1; }
[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base $BASE_APK"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]] && BT="$HOME/Library/Android/sdk/build-tools/$v" && break
done
[[ -n "$BT" ]] || { echo "ERROR: apksigner not found"; exit 1; }

WORK="$(mktemp -d /tmp/wgdzz_channel.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/3] extend channel block (official 1600B)"
python3.11 "$PATCH_PY" --donor "$DONOR" -i "$BASE_APK" -o "$WORK/extended.apk"

echo "[2/3] ${NO_RESIGN:+skip re-sign (NO_RESIGN=1)}"
if [[ "${NO_RESIGN:-0}" != "1" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$WORK/extended.apk"
  echo "WARN: apksigner re-sign truncates channel 1600→920; use NO_RESIGN=1 for full channel test"
else
  cp -f "$WORK/extended.apk" "$SIGNED"
fi

echo "[3/3] verify channel + v2"
python3.11 "$PATCH_PY" --donor "$DONOR" -i "$SIGNED" -o /dev/null --report-only 2>/dev/null || \
  python3.11 "$PATCH_PY" --donor "$DONOR" -i "$SIGNED" -o "$WORK/dummy.apk" 2>&1 | tail -8
"$BT/apksigner" verify "$SIGNED" 2>&1 | head -3 || true
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb install -r -d "$SIGNED"; then
    echo "[install] cover ok"
    echo "[install] pm clear jiagu cache"
    adb shell pm clear "$PKG" 2>/dev/null || true
    echo "log: adb logcat -c && adb logcat -s StarryTrace StarryMagic:V WgdzzAppComponentFact:V"
  else
    echo "ERROR: install failed"; exit 2
  fi
fi
echo "DONE $SIGNED"
