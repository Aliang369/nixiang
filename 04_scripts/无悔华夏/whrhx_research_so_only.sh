#!/usr/bin/env bash
# ================================================================
# 无悔华夏 3.8.280 九游 — 科研立即完成「热更 RVA 重校」so-only 外科
#
# 语义（stages.md「调试内循环」：so_only = 已有底包只换 so 重签，不碰原包）：
#   底包 = 真机现装的星空包（用户授权从设备拉取，已含 ModBridge 菜单 dex，
#          其 native 声明 nativeBridgeRegistered/nativeGetPinnedCert 上一轮已补齐，
#          故本轮无需 smali 配对，只替换 lib/arm64-v8a/libstarry_magic.so）。
#   只改一个 zip 条目 → zipalign → 星空证书重签 → 四查审计。
#   不安装、不卸载、不清数据；输出独立文件名，不覆盖历史修复包。
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
BASE_APK="${WHRHX_BASE_APK:-$NIXIANG/06_dumped/无悔华夏/current_20260923/pkg/base_installed_20260923.apk}"
BASE_SHA_EXPECT="${WHRHX_BASE_SHA:-0b6c4772e64a6753b3a3169c9aea3a2e9af435c6361746c83b13516ba3f03167}"
SO="${WHRHX_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_whrhx/arm64-v8a/libstarry_magic.so}"
OUT_DIR="$NIXIANG/08_output/无悔华夏"
OUT_APK="$OUT_DIR/无悔华夏_3.8.280_科研立即完成热更修复_20260923-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
TARGET_ENTRY="lib/arm64-v8a/libstarry_magic.so"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing mod so: $SO" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing signing key: $KS" >&2; exit 1; }

BASE_SHA="$(shasum -a 256 "$BASE_APK" | awk '{print $1}')"
[[ "$BASE_SHA" == "$BASE_SHA_EXPECT" ]] || {
  echo "ERROR: unexpected base apk sha256=$BASE_SHA (want $BASE_SHA_EXPECT)" >&2
  exit 1
}

# --- so 标签门禁：本轮科研 hook + 名臣入口标记必须存在 ---
for tag in "[Whrhx]" "research hooks ready" "科研立即完成" "minister policy ui execute" "extra minister policy ui execute"; do
  LC_ALL=C grep -a -q "$tag" "$SO" || { echo "ERROR: mod so missing tag: $tag" >&2; exit 1; }
done

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: apksigner not found" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx_soonly.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[so_only] base  sha256 = $BASE_SHA"
echo "[so_only] so    md5    = $(md5 -q "$SO")"
echo "[so_only] so    sha256 = $(shasum -a 256 "$SO" | awk '{print $1}')"
echo "[so_only] old so in base = $(unzip -p "$BASE_APK" "$TARGET_ENTRY" | md5 -q)"

cp -f "$BASE_APK" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(
  cd "$WORK"
  zip -q -u base.apk "$TARGET_ENTRY"
)

python3.11 - "$WORK/base.apk" <<'PY'
import sys, zipfile
dst = sys.argv[1]
with zipfile.ZipFile(dst) as b:
    infos = b.infolist()
    names = [i.filename for i in infos]
    if len(names) != len(set(names)):
        raise SystemExit("ERROR: duplicate zip entries after update")
    bad = [i.filename for i in infos if i.compress_type not in (0, 8)]
    if bad:
        raise SystemExit(f"ERROR: unexpected compress methods: {bad[:5]}")
print(f"zip entries after update: {len(names)}")
PY

python3.11 - "$BASE_APK" "$WORK/base.apk" <<'PY'
import sys, zipfile
src, dst = sys.argv[1], sys.argv[2]
target = "lib/arm64-v8a/libstarry_magic.so"
with zipfile.ZipFile(src) as a, zipfile.ZipFile(dst) as b:
    names_a, names_b = a.namelist(), b.namelist()
    if names_a != names_b:
        raise SystemExit("ERROR: zip entry list/order changed")
    def method_map(z):
        return {i.filename: i.compress_type for i in z.infolist()}
    ma, mb = method_map(a), method_map(b)
    changed, meta = [], []
    for name in names_a:
        ia, ib = a.getinfo(name), b.getinfo(name)
        if ia.CRC != ib.CRC or ia.file_size != ib.file_size:
            changed.append(name)
        if ia.compress_type != ib.compress_type:
            meta.append(name)
    if set(changed) != {target}:
        raise SystemExit(f"ERROR: changed entries unexpected: {changed}")
    if meta:
        raise SystemExit(f"ERROR: compress method drifted: {meta}")
print("zip entry audit: only lib/arm64-v8a/libstarry_magic.so changed")
PY

"$BT/zipalign" -f -p 4 "$WORK/base.apk" "$WORK/aligned.apk"
rm -f "$WORK/base.apk"

mkdir -p "$OUT_DIR"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT_APK" "$WORK/aligned.apk"
rm -f "$WORK/aligned.apk"

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

# --- 产物复核：包内 so == 构建产物；包内 dex 与底包逐字节一致 ---
unzip -p "$OUT_APK" "$TARGET_ENTRY" > "$WORK/final.so"
cmp -s "$SO" "$WORK/final.so" || { echo "ERROR: packaged so differs from build output" >&2; exit 1; }
for d in classes.dex classes2.dex; do
  unzip -p "$BASE_APK" "$d" > "$WORK/base_$d"
  unzip -p "$OUT_APK" "$d" > "$WORK/out_$d"
  cmp -s "$WORK/base_$d" "$WORK/out_$d" || { echo "ERROR: $d changed" >&2; exit 1; }
done
python3.11 - "$BASE_APK" "$OUT_APK" <<'PY'
import sys, zipfile
def sig(n):
    u = n.upper()
    return n.startswith("META-INF/") and u.endswith((".SF", ".RSA", ".DSA", ".MF"))
with zipfile.ZipFile(sys.argv[1]) as a, zipfile.ZipFile(sys.argv[2]) as b:
    na, nb = a.namelist(), b.namelist()
    if [n for n in na if not sig(n)] != [n for n in nb if not sig(n)]:
        raise SystemExit("ERROR: final apk non-signature entry list changed")
    delta = sorted(set(nb) - set(na)), sorted(set(na) - set(nb))
    print(f"final apk entries ok; META-INF signature delta: +{delta[0]} -{delta[1]}")
PY

echo "[so_only] done -> $OUT_APK"
echo "[so_only] apk size   = $(stat -f %z "$OUT_APK")"
echo "[so_only] apk sha256 = $(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
echo "[so_only] apk md5    = $(md5 -q "$OUT_APK")"
echo "[so_only] so  sha256 = $(shasum -a 256 "$SO" | awk '{print $1}')"
