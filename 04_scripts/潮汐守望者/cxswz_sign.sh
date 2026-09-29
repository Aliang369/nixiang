#!/usr/bin/env bash
# 潮汐守望者 1.2.56.375.3 — 星光包签名阶段（starry_release.jks，与其他星光包同签）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="潮汐守望者"
VERSION="1.2.56.375.3"
UNSIGNED="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_starry-unsigned.apk"
OUT="$NIXIANG/08_output/$GAME"
SIGNED="$OUT/${GAME}_${VERSION}_starry-releaseSigned.apk"
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
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" && -x "$BT/apksigner" ]] || { echo "ERROR: apksigner missing" >&2; exit 1; }

WORK="$(mktemp -d /tmp/cxswz_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"
rm -f "$SIGNED"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify "$SIGNED" >/dev/null

unzip -l "$SIGNED" > "$WORK/entries.txt"
grep -q "classes16.dex" "$WORK/entries.txt" || { echo "ERROR: classes16.dex missing" >&2; exit 1; }
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" || { echo "ERROR: starry so missing" >&2; exit 1; }
SIGNED_TAG_N="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF -- "[Cxswz]" || true)"
[[ "$SIGNED_TAG_N" -ge 1 ]] || { echo "ERROR: signed apk starry so missing [Cxswz] tag" >&2; exit 1; }

echo "[sign] done → $SIGNED"
echo "[sign] md5=$(md5 -q "$SIGNED")"
echo "[sign] sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>/dev/null | grep -E 'certificate (DN|SHA-256|MD5)' | head -3 || true
ls -lh "$SIGNED"
