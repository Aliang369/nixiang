#!/usr/bin/env bash
# 蛋壳战机私服 1.2.1 (vc35) — 星光签名阶段。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="蛋壳战机"
VERSION="1.2.1"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_私服_${VERSION}_starry-unsigned.apk"
SIGNED="$OUT/${GAME}_私服_${VERSION}_starry-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILD="$NIXIANG/mod菜单/starry-mod-menu/libs_dkzjsf/arm64-v8a/libstarry_magic.so"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED （先跑 surgical）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/dkzj_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  rm -f "$SIGNED"
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
if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" verify "$SIGNED" >/dev/null || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

unzip -l "$SIGNED" > "$WORK/entries.txt"
for n in classes.dex classes2.dex classes3.dex classes4.dex classes5.dex classes6.dex classes7.dex classes8.dex resources.arsc; do
  grep -q " $n$" "$WORK/entries.txt" || { echo "ERROR: $n missing" >&2; exit 1; }
done
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" \
  || { echo "ERROR: starry so missing" >&2; exit 1; }

SO_MD5="$(md5 -q "$SO_BUILD" 2>/dev/null || echo none)"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }

ORIG_APK="$NIXIANG/02_apks/弹壳战机.apk"
[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
python3 - "$ORIG_APK" "$SIGNED" <<'PY'
import hashlib, sys, zipfile
orig, signed = sys.argv[1], sys.argv[2]
GUARD = [
    'classes.dex', 'classes2.dex', 'classes3.dex', 'classes4.dex',
    'classes5.dex', 'classes6.dex', 'classes7.dex',
    'resources.arsc', 'lib/arm64-v8a/libil2cpp.so',
    'lib/arm64-v8a/libunity.so', 'lib/arm64-v8a/libmain.so',
    'lib/arm64-v8a/liblgsgift.so',
    'assets/bin/Data/Managed/Metadata/global-metadata.dat',
]
with zipfile.ZipFile(orig) as a, zipfile.ZipFile(signed) as b:
    for n in GUARD:
        ha = hashlib.sha256(a.read(n)).hexdigest()
        hb = hashlib.sha256(b.read(n)).hexdigest()
        if ha != hb:
            raise SystemExit(f'protected entry changed: {n}')
        print(f'  {n} sha256={hb[:16]}… ok')
print('  原包游戏本体零改动校验通过')
PY

# 私服无签名校验/非 TapTap，不需 SignSpoof 包名/证书映射；只校验包内 so 含 game_id dkzjsf
LC_ALL=C grep -aq "dkzjsf" <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so) \
  || { echo "ERROR: 包内 so 缺 game_id dkzjsf" >&2; exit 1; }

echo "[sign] done → $SIGNED"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
echo "[sign] sha256 = $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
