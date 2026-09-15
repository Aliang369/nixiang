#!/usr/bin/env bash
# 偶像梦幻祭2 4.5.8861 — 签名阶段。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="偶像梦幻2"
VERSION="4.5.8861"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done

WORK="$(mktemp -d /tmp/es2_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED"
else
  java -jar "$SIGNER" -a "$UNSIGNED" --out "$WORK/signed" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" >/dev/null
  CAND="$(ls -t "$WORK"/signed/*Signed*.apk 2>/dev/null | sed -n '1p' || true)"
  [[ -n "${CAND:-}" ]] || { echo "ERROR: signer produced nothing" >&2; exit 1; }
  cp -f "$CAND" "$SIGNED"
fi

[[ -f "$SIGNED" ]] || { echo "ERROR: sign failed" >&2; exit 1; }
if [[ -n "$BT" ]]; then "$BT/apksigner" verify "$SIGNED" >/dev/null; fi
unzip -l "$SIGNED" > "$WORK/entries.txt"
for e in AndroidManifest.xml resources.arsc classes.dex classes2.dex classes3.dex lib/arm64-v8a/libil2cpp.so lib/arm64-v8a/libstarry_magic.so; do
  grep -q " $e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing in signed APK" >&2; exit 1; }
done
SO_MD5="$(md5 -q "$NIXIANG/mod菜单/starry-mod-menu/libs_es2/arm64-v8a/libstarry_magic.so")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] || { echo "ERROR: signed APK so mismatch" >&2; exit 1; }

cat <<EOF
[sign] done -> $SIGNED
[sign] size=$(stat -f '%z' "$SIGNED")
[sign] md5=$(md5 -q "$SIGNED")
[sign] sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')
[sign] so_md5=$IN_SO_MD5
EOF
