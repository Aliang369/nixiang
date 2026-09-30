#!/usr/bin/env bash
# 无限升级 11.266018 / vc1637 — 星空版外科组装（禁止对游戏 apktool b 整包）
#
# 方案：官方 base.apk 逐字节透传 + 外科 zip 追加/替换：
#   AndroidManifest.xml（仅加 provider/service/权限）
#   lib/arm64-v8a/libstarry_magic.so（vc1637 专用，内嵌 vc1637 补丁 DLL）
#   lib/arm64-v8a/libc++_shared.so
#   assets/certs/starlight.crt
#   classes4.dex（com/starry 菜单，官方 classes1-3 保持原样）
# 注入方式：ContentProvider（com.starry.magic.StarryInitProvider），不替换 Application。
#   官方 application 无 android:name（默认 android.app.Application），
#   且全包不含 com/netease/nis/wrapper/MyApplication ⇒ 菜单 dex 的 StarryApp
#   必须 super Landroid/app/Application;（见 wxsj_vc1637_menu_tpl.sh）。
#
# 与 12.x 完全隔离：输出名/中间目录/so/菜单模板/DLL 全部独立，不覆盖 12.x 产物。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY="$NIXIANG/mod菜单/starry-mod-menu"
ORIG_APK="${WXSJ_ORIG_APK:-$NIXIANG/07_patched/无限升级/installed_official_11.266018_vc1637_base.apk}"
FINAL_SO="${WXSJ_SO:-$STARRY/app/src/libs_wxsj_vc1637/arm64-v8a/libstarry_magic.so}"
MENU_TPL_DIR="${WXSJ_MENU_TPL:-$STARRY/templates/wxsj_menu_vc1637}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
OUT="${WXSJ_OUT:-$NIXIANG/08_output/无限升级/无限升级_11.266018_vc1637_starry-releaseSigned.apk}"
STARRY_DEX="classes4.dex"

for f in "$ORIG_APK" "$FINAL_SO" "$KS" "$MENU_TPL_DIR/$STARRY_DEX" \
         "$MENU_TPL_DIR/libc++_shared.so" "$MENU_TPL_DIR/starlight.crt"; do
  [[ -f "$f" ]] || { echo "ERROR: missing $f"; exit 1; }
done

# ---- so 门禁：vc1637 标签 + 商店广告无限 + 无他游戏标签 + 无 DIAG ----
LC_ALL=C grep -aqF '[WxsjVC1637]' "$FINAL_SO" || { echo "ERROR: so 缺 [WxsjVC1637]"; exit 1; }
LC_ALL=C grep -aqF 'Toggle_商店广告无限' "$FINAL_SO" || { echo "ERROR: so 缺 Toggle_商店广告无限"; exit 1; }
for bad in '[Cwjh]' '[Endfield]' '[Zhmr]' '[Wxsj]' 'Toggle_宝箱全拿'; do
  if LC_ALL=C grep -aqF -- "$bad" "$FINAL_SO"; then
    echo "ERROR: so 含禁用串 $bad"; exit 1
  fi
done
if LC_ALL=C grep -aqF 'DIAG dumped' "$FINAL_SO"; then
  echo "ERROR: so 是 DIAG 取证版，禁出正式包"; exit 1
fi

# ---- 菜单 dex 门禁：不得引用易盾壳类 ----
if LC_ALL=C grep -aq 'com/netease/nis/wrapper/MyApplication' "$MENU_TPL_DIR/$STARRY_DEX"; then
  echo "ERROR: $STARRY_DEX 引用易盾壳类（vc1637 无此类，必崩）"; exit 1
fi
LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$MENU_TPL_DIR/$STARRY_DEX" \
  || { echo "ERROR: $STARRY_DEX 缺 StarryInitProvider"; exit 1; }

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: Android build-tools not found"; exit 1; }

WORK="$(mktemp -d /tmp/wxsj_vc1637_build.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK/stage/lib/arm64-v8a" "$WORK/stage/assets/certs"

echo "[1/6] decode official manifest ($(basename "$ORIG_APK"))"
apktool d -f -s -o "$WORK/base_dec" "$ORIG_APK" >/dev/null

echo "[2/6] patch decoded manifest（加 provider/service/权限，不动 application@name）"
python3 - "$WORK/base_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
import xml.etree.ElementTree as ET

path = Path(sys.argv[1])
ET.register_namespace("android", "http://schemas.android.com/apk/res/android")
A = "http://schemas.android.com/apk/res/android"
q = lambda n: f"{{{A}}}{n}"
tree = ET.parse(path)
root = tree.getroot()
assert root.tag == "manifest", root.tag

existing = {e.get(q("name")) for e in root.findall("uses-permission")}
app = root.find("application")
assert app is not None
for name in (
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
):
    if name not in existing:
        elem = ET.Element("uses-permission", {q("name"): name})
        root.insert(list(root).index(app), elem)

app_name = app.get(q("name"))
if app_name is not None:
    print(f"  WARN: 官方 application@name = {app_name}（vc1637 预期无此属性）")

providers = {e.get(q("name")): e for e in app.findall("provider")}
sp = providers.get("com.starry.magic.StarryInitProvider")
if sp is None:
    sp = ET.SubElement(app, "provider", {
        q("name"): "com.starry.magic.StarryInitProvider",
        q("authorities"): "com.xingzuodaoshi.infinityUpgrade.starryinit",
        q("exported"): "false",
    })
# 【2026-09-30】initOrder=MAX：让 SignSpoof.install() 早于 TapTap provider/TapSDK init，
# 否则 TapSDK 的 TapInitializer 会读到真实签名并误报「包名/签名未在 TapTap 开发者中心正确配置」。
sp.set(q("initOrder"), "2147483647")

services = {e.get(q("name")) for e in app.findall("service")}
if "com.starry.magic.menu.MenuService" not in services:
    svc = ET.SubElement(app, "service", {
        q("name"): "com.starry.magic.menu.MenuService",
        q("enabled"): "true",
        q("exported"): "false",
        q("foregroundServiceType"): "specialUse",
        q("stopWithTask"): "true",
    })
    ET.SubElement(svc, "property", {
        q("name"): "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE",
        q("value"): "MOD menu overlay service",
    })

tree.write(path, encoding="utf-8", xml_declaration=True)
print(f"  manifest: application@name={app_name} 保持不变 + StarryInitProvider + MenuService")
PY

echo "[3/6] rebuild resources/manifest only"
apktool b "$WORK/base_dec" -o "$WORK/rebuilt.apk" >/dev/null
unzip -p "$WORK/rebuilt.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

echo "[4/6] stage starry so/dex/assets"
cp -f "$WORK/AndroidManifest.xml" "$WORK/stage/AndroidManifest.xml"
cp -f "$FINAL_SO" "$WORK/stage/lib/arm64-v8a/libstarry_magic.so"
cp -f "$MENU_TPL_DIR/libc++_shared.so" "$WORK/stage/lib/arm64-v8a/libc++_shared.so"
cp -f "$MENU_TPL_DIR/starlight.crt" "$WORK/stage/assets/certs/starlight.crt"
cp -f "$MENU_TPL_DIR/$STARRY_DEX" "$WORK/stage/$STARRY_DEX"

cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
(cd "$WORK/stage" && zip -u "$WORK/unsigned.apk" \
  AndroidManifest.xml \
  lib/arm64-v8a/libstarry_magic.so \
  lib/arm64-v8a/libc++_shared.so \
  assets/certs/starlight.crt \
  "$STARRY_DEX") >/dev/null

echo "[5/6] verify merged structure"
"$BT/aapt2" dump xmltree "$WORK/rebuilt.apk" --file AndroidManifest.xml > "$WORK/mf_tree.txt" 2>&1
grep -q 'com.starry.magic.StarryInitProvider' "$WORK/mf_tree.txt" || { echo "ERROR: StarryInitProvider missing"; exit 1; }
grep -q 'android:initOrder(0x0101001a)=2147483647' "$WORK/mf_tree.txt" \
  || { echo "ERROR: StarryInitProvider 缺 android:initOrder=2147483647（签名伪装将晚于 TapSDK init）"; exit 1; }
grep -q 'com.starry.magic.menu.MenuService' "$WORK/mf_tree.txt" || { echo "ERROR: MenuService missing"; exit 1; }
if grep -q 'com.starry.magic.StarryApp' "$WORK/mf_tree.txt"; then
  echo "ERROR: manifest 把 StarryApp 当 application@name（本方案禁）"; exit 1
fi
python3 - "$WORK/unsigned.apk" "$ORIG_APK" "$STARRY_DEX" "$FINAL_SO" <<'PY'
import hashlib, sys, zipfile
new, old = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
starry_dex, final_so = sys.argv[3], sys.argv[4]
old_names = set(old.namelist())
checked = [n for n in old_names if n in ("lib/arm64-v8a/libil2cpp.so", "resources.arsc")
           or (n.startswith("classes") and n.endswith(".dex"))]
for name in sorted(checked):
    assert hashlib.sha256(new.read(name)).digest() == hashlib.sha256(old.read(name)).digest(), name
assert "lib/arm64-v8a/libil2cpp.so" in checked, "libil2cpp.so not found in official apk"
so = new.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[WxsjVC1637]" in so, "starry so missing [WxsjVC1637]"
assert b"Toggle_\xe5\x95\x86\xe5\xba\x97\xe5\xb9\xbf\xe5\x91\x8a\xe6\x97\xa0\xe9\x99\x90" in so, "so missing Toggle_商店广告无限"
assert hashlib.sha256(so).digest() == hashlib.sha256(open(final_so, "rb").read()).digest(), "so hash mismatch"
assert starry_dex in new.namelist(), f"{starry_dex} missing"
dx = new.read(starry_dex)
assert b"com/starry/magic/StarryInitProvider" in dx, "StarryInitProvider not in starry dex"
assert b"com/netease/nis/wrapper/MyApplication" not in dx, "starry dex references 易盾 shell class"
print(f"  official {len(checked)} files intact; {starry_dex}(com/starry) + so present")
PY

echo "[6/6] zipalign + sign"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
mkdir -p "$(dirname "$OUT")"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$OUT" | head -4

echo
echo "[verify] versionCode/versionName + 包内指纹"
"$BT/aapt2" dump xmltree "$OUT" --file AndroidManifest.xml > "$WORK/out_mf.txt" 2>/dev/null
grep -E 'versionCode|versionName' "$WORK/out_mf.txt" | head -2
python3 - "$OUT" <<'PY'
import hashlib, sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
so = z.read("lib/arm64-v8a/libstarry_magic.so")
dx = z.read("classes4.dex")
print("  so   %d B sha256=%s" % (len(so), hashlib.sha256(so).hexdigest()[:32]))
print("  dex4 %d B sha256=%s" % (len(dx), hashlib.sha256(dx).hexdigest()[:32]))
PY
ls -lh "$OUT"
shasum -a 256 "$OUT"
