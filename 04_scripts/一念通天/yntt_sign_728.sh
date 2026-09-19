#!/usr/bin/env bash
# 一念通天 728 — 签名阶段（pipeline sign）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="一念通天"
VERSION="728"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/yntt_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p' || true
fi

if [[ ! -f "$SIGNED" ]]; then
  [[ -n "$BT" ]] || cp "$UNSIGNED" "$ALIGNED"
  java -jar "$SIGNER" -a "$ALIGNED" --out "$WORK/signed" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" >/dev/null
  CAND="$(ls -t "$WORK"/signed/*Signed*.apk 2>/dev/null | sed -n '1p' || true)"
  [[ -n "${CAND:-}" ]] || { echo "ERROR: uber-apk-signer produced nothing" >&2; exit 1; }
  cp -f "$CAND" "$SIGNED"
fi

[[ -f "$SIGNED" ]] || { echo "ERROR: sign failed" >&2; exit 1; }
if [[ -n "$BT" ]]; then
  "$BT/apksigner" verify "$SIGNED" >/dev/null || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

unzip -l "$SIGNED" > "$WORK/entries.txt"
for e in classes.dex classes2.dex classes3.dex \
         lib/arm64-v8a/libstarry_magic.so resources.arsc; do
  grep -q " $e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing in signed apk" >&2; exit 1; }
done

SO_MD5="$(md5 -q "$NIXIANG/mod菜单/starry-mod-menu/libs_yntt/arm64-v8a/libstarry_magic.so")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] || { echo "ERROR: signed so mismatch" >&2; exit 1; }

echo "[sign] done → $SIGNED"
echo "[sign] size=$(stat -f '%z' "$SIGNED")"
echo "[sign] md5=$(md5 -q "$SIGNED")"
echo "[sign] sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "[sign] so_md5=$IN_SO_MD5"
