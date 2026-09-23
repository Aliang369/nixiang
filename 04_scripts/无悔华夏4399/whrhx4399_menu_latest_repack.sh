#!/usr/bin/env bash
# ================================================================
# 无悔华夏 3.8.280 4399（m4399）— 「新版菜单 + 本轮 so」外科合并
#
# 语义（九游 whrhx_menu_latest_repack.sh 的 4399 孪生版）：
#   底包 = 真机现装的 4399 无壳星空包（用户授权从设备拉取；
#          md5 e146f03c… == 管线 so_only 阶段产物，Manifest 已是
#          Application=StarryApp + MenuService 的注入态，本轮不动 Manifest）
#   只改两个 zip 条目：
#     classes4.dex                     ← 07_patched/无悔华夏4399/menu_latest_20260923/classes4.dex
#                                        （仓库当前菜单源码重建 + StarryApp
#                                          .super 回改 Lcom/excelliance/open/LBApplication;）
#     lib/arm64-v8a/libstarry_magic.so ← 本轮 4399 私有构建（whhx4399 + 科研真门
#                                          PolicyDataMgr.GetPolicyItemShowState）
#   其余条目（classes.dex/classes2.dex/classes3.dex / libil2cpp.so / resources.arsc /
#   libc++_shared.so / certs / Manifest …）逐字节不动。
#   → zipalign → 星空证书重签 → 四查审计。不安装、不卸载、不清数据。
#
# 注意：同名产物若已存在请先删除（磁盘峰值 ≈ 底包 ×3）。
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
BASE_APK="${WHRHX4399_BASE_APK:-$NIXIANG/06_dumped/无悔华夏4399/current_20260923/pkg/base_installed_20260923.apk}"
BASE_SHA_EXPECT="${WHRHX4399_BASE_SHA:-6b51ee7f0e107d8fc3ccd1078d0ace4fc8776d2efe519fc0dc63724037a7c9c7}"
BASE_MD5_EXPECT="${WHRHX4399_BASE_MD5:-e146f03c9c034d6f9dadf51d708b4aa0}"
SO="${WHRHX4399_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_whrhx4399/arm64-v8a/libstarry_magic.so}"
DEX="${WHRHX4399_MENU_DEX:-$NIXIANG/07_patched/无悔华夏4399/menu_latest_20260923/classes4.dex}"
DEX_SHA_EXPECT="${WHRHX4399_MENU_DEX_SHA:-817c9b693d34f7d1c11854b2b886682548231d5cff58f87da87fb153d9faece1}"
OUT_DIR="$NIXIANG/08_output/无悔华夏4399"
OUT_APK="$OUT_DIR/无悔华夏4399_3.8.280_新版菜单_科研修复_20260923-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_ENTRY="lib/arm64-v8a/libstarry_magic.so"
DEX_ENTRY="classes4.dex"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing mod so: $SO" >&2; exit 1; }
[[ -f "$DEX" ]] || { echo "ERROR: missing menu dex: $DEX" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing signing key: $KS" >&2; exit 1; }
[[ ! -e "$OUT_APK" ]] || { echo "ERROR: output exists, remove first (disk): $OUT_APK" >&2; exit 1; }

BASE_SHA="$(shasum -a 256 "$BASE_APK" | awk '{print $1}')"
[[ "$BASE_SHA" == "$BASE_SHA_EXPECT" ]] || {
  echo "ERROR: unexpected base apk sha256=$BASE_SHA (want $BASE_SHA_EXPECT)" >&2
  exit 1
}
BASE_MD5="$(md5 -q "$BASE_APK")"
[[ "$BASE_MD5" == "$BASE_MD5_EXPECT" ]] || {
  echo "ERROR: unexpected base apk md5=$BASE_MD5 (want $BASE_MD5_EXPECT)" >&2
  exit 1
}

# --- so 标签门禁：4399 私有构建标记 + 本轮科研真门 hook ---
for tag in "[Whrhx]" "whhx4399" "target=m4399" "policy panel gate ready" \
           "PolicyDataMgr.GetPolicyItemShowState" "科研立即完成" "research hooks ready"; do
  LC_ALL=C grep -a -q "$tag" "$SO" || { echo "ERROR: mod so missing tag: $tag" >&2; exit 1; }
done

# --- 菜单 dex 门禁：指纹 + 4399 壳父类补丁 + 入口类 ---
DEX_SHA="$(shasum -a 256 "$DEX" | awk '{print $1}')"
[[ "$DEX_SHA" == "$DEX_SHA_EXPECT" ]] || {
  echo "ERROR: unexpected menu dex sha256=$DEX_SHA (want $DEX_SHA_EXPECT)" >&2
  exit 1
}
LC_ALL=C grep -a -q "Lcom/excelliance/open/LBApplication;" "$DEX" || {
  echo "ERROR: menu dex missing 4399-shell StarryApp super patch" >&2; exit 1; }
LC_ALL=C grep -a -q "Lcom/netease/nis/wrapper/MyApplication;" "$DEX" && {
  echo "ERROR: menu dex still carries NIS MyApplication reference" >&2; exit 1; }
for cls in "Lcom/starry/magic/StarryApp;" "Lcom/starry/magic/menu/MenuService;" \
           "Lcom/starry/magic/menu/NativeMenuController;" "Lcom/starry/magic/menu/MagicRes;" \
           "Lcom/starry/magic/bridge/ModBridge;"; do
  LC_ALL=C grep -a -q "$cls" "$DEX" || { echo "ERROR: menu dex missing class: $cls" >&2; exit 1; }
done

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: apksigner not found" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx4399_menulatest.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[4399_menu_latest] base   sha256 = $BASE_SHA  md5 = $BASE_MD5"
echo "[4399_menu_latest] so     sha256 = $(shasum -a 256 "$SO" | awk '{print $1}')  md5=$(md5 -q "$SO")  size=$(stat -f %z "$SO")"
echo "[4399_menu_latest] menudex sha256 = $DEX_SHA  size=$(stat -f %z "$DEX")"
echo "[4399_menu_latest] base 内 旧 dex sha256 = $(unzip -p "$BASE_APK" "$DEX_ENTRY" | shasum -a 256 | awk '{print $1}')"
echo "[4399_menu_latest] base 内 旧 so  sha256 = $(unzip -p "$BASE_APK" "$SO_ENTRY" | shasum -a 256 | awk '{print $1}')"

cp -f "$BASE_APK" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
cp -f "$DEX" "$WORK/classes4.dex"
(
  cd "$WORK"
  zip -q -u base.apk "$SO_ENTRY" "$DEX_ENTRY"
)

python3.11 - "$WORK/base.apk" <<'PY'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as b:
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
targets = {"lib/arm64-v8a/libstarry_magic.so", "classes4.dex"}
with zipfile.ZipFile(src) as a, zipfile.ZipFile(dst) as b:
    names_a, names_b = a.namelist(), b.namelist()
    if names_a != names_b:
        raise SystemExit("ERROR: zip entry list/order changed")
    changed, meta = [], []
    for name in names_a:
        ia, ib = a.getinfo(name), b.getinfo(name)
        if ia.CRC != ib.CRC or ia.file_size != ib.file_size:
            changed.append(name)
        if ia.compress_type != ib.compress_type:
            meta.append(name)
    if set(changed) != targets:
        raise SystemExit(f"ERROR: changed entries unexpected: {changed}")
    if meta:
        raise SystemExit(f"ERROR: compress method drifted: {meta}")
print("zip entry audit: only classes4.dex + lib/arm64-v8a/libstarry_magic.so changed")
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
  echo "ERROR: signed apk does not use StarryMagic certificate" >&2; cat "$WORK/verify.txt" >&2; exit 1; }
LC_ALL=C grep -q "SHA-1 digest: 821c92e404ab5fdac8dbf157c2e467e71440917d" "$WORK/verify.txt" || {
  echo "ERROR: unexpected signer SHA-1" >&2; cat "$WORK/verify.txt" >&2; exit 1; }
LC_ALL=C grep -q "Verifies" "$WORK/verify.txt" || {
  echo "ERROR: apksigner verify failed" >&2; cat "$WORK/verify.txt" >&2; exit 1; }

# --- 产物复核：两个目标条目 == 输入；其余关键条目与底包逐字节一致 ---
unzip -p "$OUT_APK" "$SO_ENTRY" > "$WORK/final.so"
cmp -s "$SO" "$WORK/final.so" || { echo "ERROR: packaged so differs from build output" >&2; exit 1; }
unzip -p "$OUT_APK" "$DEX_ENTRY" > "$WORK/final.dex"
cmp -s "$DEX" "$WORK/final.dex" || { echo "ERROR: packaged dex differs from menu dex" >&2; exit 1; }
for d in classes.dex classes2.dex classes3.dex resources.arsc AndroidManifest.xml \
         lib/arm64-v8a/libil2cpp.so lib/arm64-v8a/libc++_shared.so assets/certs/starlight.crt; do
  unzip -p "$BASE_APK" "$d" > "$WORK/base_only" 2>/dev/null || { echo "ERROR: base missing $d" >&2; exit 1; }
  unzip -p "$OUT_APK"  "$d" > "$WORK/out_only"  2>/dev/null || { echo "ERROR: out missing $d" >&2; exit 1; }
  cmp -s "$WORK/base_only" "$WORK/out_only" || { echo "ERROR: $d changed" >&2; exit 1; }
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

# --- dex 内嵌复核（baksmali 侧确认补丁真的进了包）---
if [[ -x "$NIXIANG/01_tools/smali/baksmali" ]]; then
  "$NIXIANG/01_tools/smali/baksmali" list classes "$WORK/final.dex" > "$WORK/final_classes.txt"
  echo "[4399_menu_latest] 包内 dex 类数 = $(wc -l < "$WORK/final_classes.txt" | tr -d ' ')"
  for cls in "Lcom/starry/magic/menu/MagicRes;" "Lcom/starry/magic/StarryApp;"; do
    LC_ALL=C grep -q "$cls" "$WORK/final_classes.txt" || {
      echo "ERROR: packaged dex lost $cls" >&2; exit 1; }
  done
fi

echo "[4399_menu_latest] done -> $OUT_APK"
echo "[4399_menu_latest] apk size   = $(stat -f %z "$OUT_APK")"
echo "[4399_menu_latest] apk sha256 = $(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
echo "[4399_menu_latest] apk md5    = $(md5 -q "$OUT_APK")"
