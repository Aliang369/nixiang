#!/usr/bin/env bash
# 王国大作战 B 路线：K3（NOP _exit 调用点）快速换 libKwProtectSDK.so
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/wgdzz_kwK3_so_only.sh
#   BASE_APK=08_output/.../surgical-traceSigned.apk STARRY_INSTALL=1 bash scripts/wgdzz_kwK3_so_only.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
PATCH_PY="$NIXIANG/work/wgdzz_b_route/kwprotect_k3_patch.py"
K1="$NIXIANG/07_patched/王国大作战前线/libKwProtectSDK_sigkill_k1.so"
K5="$NIXIANG/07_patched/王国大作战前线/libKwProtectSDK_exit_k5.so"
OUT="$NIXIANG/08_output/王国大作战前线"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BASE_APK="${BASE_APK:-$OUT/王国大作战前线_1.1.7646_surgical-traceSigned.apk}"
SIGNED="$OUT/王国大作战前线_1.1.7646_surgical-kwK5Signed.apk"

[[ -f "$PATCH_PY" ]] || { echo "ERROR: missing $PATCH_PY"; exit 1; }
[[ -f "$K1" ]] || { echo "ERROR: missing K1 $K1"; exit 1; }
[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk $BASE_APK"; exit 1; }

python3.11 "$PATCH_PY" -i "$K1" -o "$K5"

WORK="$(mktemp -d /tmp/wgdzz_k3.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE_APK" "$WORK/unsigned.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$K5" "$WORK/lib/arm64-v8a/libKwProtectSDK.so"
(cd "$WORK" && zip -q -u unsigned.apk lib/arm64-v8a/libKwProtectSDK.so)

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: zipalign not found"; exit 1; }

"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
if [[ -f "$KS" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$WORK/aligned.apk"
else
  java -jar "$SIGNER" -a "$WORK/aligned.apk" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT"/*kwK3*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
rm -f "$OUT/"*.idsig 2>/dev/null || true
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/wgdzz_kwK3.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop com.xr.wgdzztap 2>/dev/null || true
  if adb install -r -d "$SIGNED" 2>/dev/null; then
    echo "[install] cover ok"
  elif adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok (su)"
  else
    echo "ERROR: 覆盖安装失败"; exit 2
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
fi
echo "DONE $SIGNED"
