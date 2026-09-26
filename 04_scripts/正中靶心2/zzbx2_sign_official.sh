#!/usr/bin/env bash
# 正中靶心2 1.9 — 签名阶段。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
VER="${ZZBX2_VER:-1.9}"
UNSIGNED="${ZZBX2_UNSIGNED:-$NIXIANG/07_patched/正中靶心2/正中靶心2_${VER}_starry-unsigned.apk}"
OUT="$NIXIANG/08_output/正中靶心2"
SIGNED="$OUT/正中靶心2_${VER}_starry-releaseSigned.apk"
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

WORK="$(mktemp -d /tmp/zzbx2_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"
rm -f "$SIGNED"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify "$SIGNED" >/dev/null

unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/packed_starry.so"
LC_ALL=C grep -aq "starry_zzbx2_v1" "$WORK/packed_starry.so" \
  || { echo "ERROR: signed apk missing evalString patch marker in so" >&2; exit 1; }
unzip -l "$SIGNED" > "$WORK/entries.txt"
grep -q "classes5.dex" "$WORK/entries.txt" || { echo "ERROR: classes5.dex missing" >&2; exit 1; }
LC_ALL=C grep -aq "Toggle_解锁月卡特权" "$WORK/packed_starry.so" \
  || { echo "ERROR: signed apk so missing Toggle_解锁月卡特权" >&2; exit 1; }
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" || { echo "ERROR: starry so missing" >&2; exit 1; }

echo "[sign] done → $SIGNED"
echo "[sign] md5=$(md5 -q "$SIGNED")"
echo "[sign] sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
