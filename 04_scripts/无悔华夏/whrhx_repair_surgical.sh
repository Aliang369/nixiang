#!/usr/bin/env bash
# 无悔华夏 3.8.280 九游星空包 - 2026-09-12 资源功能热更修复
# 只替换当前星空底包内的 lib/arm64-v8a/libstarry_magic.so，再以同一星空证书签名。
# 不安装、不卸载、不清数据。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
BASE_APK="${WHRHX_BASE_APK:-$NIXIANG/02_apks/无悔华夏/无悔华夏_3.8.280_九游星空_20260912.apk}"
SO="${WHRHX_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_whrhx/arm64-v8a/libstarry_magic.so}"
OUT_DIR="$NIXIANG/08_output/无悔华夏"
OUT_APK="$OUT_DIR/无悔华夏_3.8.280_资源热更修复_20260912-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
EXPECTED_BASE_SHA="d6e3e8d21e96a0ace74357b3852d81322d705c7d6fe3481bcc37965b57000997"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing mod so: $SO" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing signing key: $KS" >&2; exit 1; }

BASE_SHA="$(shasum -a 256 "$BASE_APK" | awk '{print $1}')"
[[ "$BASE_SHA" == "$EXPECTED_BASE_SHA" ]] || {
  echo "ERROR: unexpected base apk sha256=$BASE_SHA" >&2
  echo "expected=$EXPECTED_BASE_SHA" >&2
  exit 1
}

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: apksigner not found" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx_repair.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE_APK" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"

(
  cd "$WORK"
  zip -q -u base.apk lib/arm64-v8a/libstarry_magic.so
)

python3.11 - "$BASE_APK" "$WORK/base.apk" <<'PY'
import sys, zipfile
src, dst = sys.argv[1], sys.argv[2]
target = "lib/arm64-v8a/libstarry_magic.so"
with zipfile.ZipFile(src) as a, zipfile.ZipFile(dst) as b:
    names_a = a.namelist()
    names_b = b.namelist()
    if names_a != names_b:
        raise SystemExit("ERROR: zip entry list changed")
    changed = []
    for name in names_a:
        ia, ib = a.getinfo(name), b.getinfo(name)
        if name == target:
            if ia.CRC == ib.CRC:
                raise SystemExit("ERROR: target so CRC did not change")
            changed.append(name)
            continue
        if ia.CRC != ib.CRC or ia.file_size != ib.file_size:
            raise SystemExit(f"ERROR: unrelated entry changed: {name}")
    if changed != [target]:
        raise SystemExit(f"ERROR: changed entries unexpected: {changed}")
print("zip entry audit: only libstarry_magic.so changed")
PY

"$BT/zipalign" -f -p 4 "$WORK/base.apk" "$WORK/aligned.apk"
mkdir -p "$OUT_DIR"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT_APK" "$WORK/aligned.apk"
"$BT/apksigner" verify --verbose --print-certs "$OUT_APK" > "$WORK/verify.txt"

LC_ALL=C grep -q "CN=StarryMagic" "$WORK/verify.txt" || {
  echo "ERROR: signed apk does not use StarryMagic certificate" >&2
  cat "$WORK/verify.txt" >&2
  exit 1
}
LC_ALL=C grep -q "SHA-1 digest: 821c92e404ab5fdac8dbf157c2e467e71440917d" "$WORK/verify.txt" || {
  echo "ERROR: unexpected signer SHA-1" >&2
  cat "$WORK/verify.txt" >&2
  exit 1
}
unzip -p "$OUT_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
cmp -s "$SO" "$WORK/final.so" || { echo "ERROR: packaged so differs" >&2; exit 1; }

echo "[surgical] done -> $OUT_APK"
echo "[surgical] apk sha256 = $(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
echo "[surgical] apk md5    = $(md5 -q "$OUT_APK")"
echo "[surgical] so  sha256 = $(shasum -a 256 "$SO" | awk '{print $1}')"
