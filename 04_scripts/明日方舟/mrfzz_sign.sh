#!/usr/bin/env bash
# 明日方舟 2.7.71 - 签名阶段（pipeline sign）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="明日方舟"
VERSION="2.7.71"
PKG="com.hypergryph.arknights"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
ORIG_APK="$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_taptap_official_from_device.apk"
ORIG_MD5="56a905c17c34aa58dd1db5be74b6c58e"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILD="$NIXIANG/mod菜单/starry-mod-menu/libs_mrfzz/arm64-v8a/libstarry_magic.so"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED (run surgical first)" >&2; exit 1; }
[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$SO_BUILD" ]] || { echo "ERROR: missing built so $SO_BUILD" >&2; exit 1; }
ACTUAL_ORIG_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACTUAL_ORIG_MD5" == "$ORIG_MD5" ]] \
  || { echo "ERROR: official apk md5 mismatch: $ACTUAL_ORIG_MD5" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done

WORK="$(mktemp -d /tmp/mrfzz_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  rm -f "$SIGNED"
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --verbose --print-certs "$SIGNED" 2>&1 | sed -n '1,12p'
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
if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" verify "$SIGNED" >/dev/null \
    || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

unzip -l "$SIGNED" > "$WORK/entries.txt"
for n in classes.dex classes2.dex classes3.dex classes4.dex classes5.dex \
         classes6.dex resources.arsc; do
  grep -q " $n$" "$WORK/entries.txt" || { echo "ERROR: $n missing" >&2; exit 1; }
done
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" \
  || { echo "ERROR: starry so missing" >&2; exit 1; }

SO_MD5="$(md5 -q "$SO_BUILD")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }

unzip -p "$SIGNED" classes6.dex > "$WORK/classes6.dex"
LC_ALL=C grep -aqF "com.hypergryph.arknights" "$WORK/classes6.dex" \
  || { echo "ERROR: classes6.dex missing HyperGryph package" >&2; exit 1; }
LC_ALL=C grep -aqF "MIICMzCCAZyg" "$WORK/classes6.dex" \
  || { echo "ERROR: classes6.dex missing HyperGryph certificate" >&2; exit 1; }

python3 - "$ORIG_APK" "$SIGNED" <<'PY'
import hashlib
import sys
import zipfile

orig, signed = sys.argv[1], sys.argv[2]
GUARD = [
    "classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes5.dex",
    "resources.arsc", "lib/arm64-v8a/libil2cpp.so",
    "lib/arm64-v8a/libunity.so", "lib/arm64-v8a/libtapsdkcore.so",
    "lib/arm64-v8a/libmain.so", "lib/arm64-v8a/libthemis.so",
    "lib/arm64-v8a/libtprt.so",
    "assets/bin/Data/Managed/Metadata/global-metadata.dat",
]
with zipfile.ZipFile(orig) as a, zipfile.ZipFile(signed) as b:
    for name in GUARD:
        ha = hashlib.sha256(a.read(name)).hexdigest()
        hb = hashlib.sha256(b.read(name)).hexdigest()
        if ha != hb:
            raise SystemExit(f"protected entry changed: {name}")
        print(f"  {name} sha256={hb[:16]}... ok")
print("  official protected entries unchanged")
PY

if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 \
    | grep -q "CN=StarryMagic" \
    || { echo "ERROR: unexpected signer certificate" >&2; exit 1; }
fi

echo "[sign] done -> $SIGNED"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
echo "[sign] sha256 = $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
