#!/usr/bin/env bash
# 动物大联盟 1.0.23 — 签名阶段。
set -euo pipefail
NIXIANG="/Users/aliang/Documents/nixiang"
UNSIGNED="$NIXIANG/07_patched/动物大联盟/动物大联盟_1.0.23_starry-unsigned.apk"
OUT="$NIXIANG/08_output/动物大联盟"
SIGNED="$OUT/动物大联盟_1.0.23_starry-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"
[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
mkdir -p "$OUT"
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" && -x "$BT/apksigner" ]] || { echo "ERROR: build-tools missing" >&2; exit 1; }
WORK="$(mktemp -d /tmp/dwdlm_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
rm -f "$SIGNED"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify "$SIGNED" >/dev/null
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/packed_starry.so"
LC_ALL=C grep -aq 'starry_dwdlm_v1' "$WORK/packed_starry.so" || { echo "ERROR: signed apk missing JS patch marker" >&2; exit 1; }
unzip -l "$SIGNED" > "$WORK/entries.txt"
grep -q 'classes2.dex' "$WORK/entries.txt" || { echo "ERROR: classes2.dex missing" >&2; exit 1; }
grep -q 'lib/arm64-v8a/libstarry_magic.so' "$WORK/entries.txt" || { echo "ERROR: starry so missing" >&2; exit 1; }
echo "[sign] done -> $SIGNED"
echo "[sign] md5=$(md5 -q "$SIGNED")"
echo "[sign] sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
