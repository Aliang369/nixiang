#!/usr/bin/env bash
# 无限升级 11.266020 / vc1641 — 外科组装（禁止对游戏 apktool b 整包）
# 方案：官方 base.apk + starry dex/so/assets 外科 zip 追加；不改官方 dex/libil2cpp。
# 产物：08_output/无限升级/无限升级_11.266020_surgical-final-releaseSigned.apk
#
# 前置：
#   - 官方底包：07_patched/无限升级/installed_orig_20260921/base.apk
#   - starry 模板 APK：mod菜单/starry-mod-menu/app/build/outputs/apk/debug/app-debug.apk
#   - 正式 so：app/src/libs/arm64-v8a/libstarry_magic.so（本脚本会先与 jniLibs 同步）
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
ORIG_APK="$NIXIANG/07_patched/无限升级/installed_orig_20260921/base.apk"
STARRY_APK="$NIXIANG/mod菜单/starry-mod-menu/app/build/outputs/apk/debug/app-debug.apk"
FINAL_SO="$NIXIANG/mod菜单/starry-mod-menu/app/src/libs/arm64-v8a/libstarry_magic.so"
JNILIBS_SO="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
OUT="$NIXIANG/08_output/无限升级/无限升级_11.266020_surgical-final-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

for f in "$ORIG_APK" "$STARRY_APK" "$FINAL_SO" "$KS"; do
  [[ -f "$f" ]] || { echo "ERROR: missing $f"; exit 1; }
done

# 以正式 so 为准同步构建副本，避免下次 Gradle 打包误用旧/诊断 so。
cp -f "$FINAL_SO" "$JNILIBS_SO"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: Android build-tools not found"; exit 1; }

WORK="$(mktemp -d /tmp/wxsj_build.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK/stage/lib/arm64-v8a" "$WORK/stage/assets"

echo "[1/7] decode official manifest/resources"
apktool d -f -s -o "$WORK/base_dec" "$ORIG_APK" >/dev/null

echo "[2/7] patch decoded manifest"
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

# Starry 所需权限（缺失才加，避免重复）
existing = {e.get(q("name")) for e in root.findall("uses-permission")}
for name in (
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
):
    if name not in existing:
        # uses-permission must precede application for Android manifest parsing.
        elem = ET.Element("uses-permission", {q("name"): name})
        root.insert(list(root).index(root.find("application")), elem)

app = root.find("application")
assert app is not None
app.set(q("name"), "com.starry.magic.StarryApp")

# MenuService：Application 已负责 nativeInit；Service 负责悬浮菜单。
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
print("  manifest: StarryApp + MenuService + 3 permissions")
PY

echo "[3/7] rebuild resources/manifest only"
apktool b "$WORK/base_dec" -o "$WORK/rebuilt.apk" >/dev/null
unzip -p "$WORK/rebuilt.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

echo "[4/7] stage starry dex/so/assets"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
cp -f "$WORK/AndroidManifest.xml" "$WORK/stage/AndroidManifest.xml"
cp -f "$FINAL_SO" "$WORK/stage/lib/arm64-v8a/libstarry_magic.so"
unzip -p "$STARRY_APK" lib/arm64-v8a/libc++_shared.so > "$WORK/stage/lib/arm64-v8a/libc++_shared.so"

# 官方 dex 为 classes.dex..classes4.dex；starry 模板从 classes5.dex 起追加。
idx=5
for dex in $(unzip -Z1 "$STARRY_APK" | grep -E '^classes[0-9]*\.dex$' | sort -V); do
  unzip -p "$STARRY_APK" "$dex" > "$WORK/stage/classes${idx}.dex"
  idx=$((idx + 1))
done
unzip -p "$STARRY_APK" assets/certs/starlight.crt > "$WORK/stage/assets/starlight.crt"
[[ -s "$WORK/stage/assets/starlight.crt" ]] || {
  echo "ERROR: starry cert missing"; exit 1;
}
mkdir -p "$WORK/stage/assets/certs"
mv "$WORK/stage/assets/starlight.crt" "$WORK/stage/assets/certs/starlight.crt"

(cd "$WORK/stage" && zip -u "$WORK/unsigned.apk" \
  AndroidManifest.xml \
  lib/arm64-v8a/libstarry_magic.so \
  lib/arm64-v8a/libc++_shared.so \
  assets/certs/starlight.crt \
  classes5.dex classes6.dex classes7.dex classes8.dex classes9.dex \
  classes10.dex classes11.dex classes12.dex classes13.dex classes14.dex) >/dev/null

echo "[5/7] verify merged structure"
"$BT/aapt2" dump xmltree "$WORK/rebuilt.apk" --file AndroidManifest.xml > "$WORK/mf_tree.txt" 2>&1
grep -q 'com.starry.magic.StarryApp' "$WORK/mf_tree.txt" || { echo "ERROR: StarryApp missing"; exit 1; }
grep -q 'com.starry.magic.menu.MenuService' "$WORK/mf_tree.txt" || { echo "ERROR: MenuService missing"; exit 1; }
python3 - "$WORK/unsigned.apk" "$ORIG_APK" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
new, old = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
for name in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "lib/arm64-v8a/libil2cpp.so"):
    assert hashlib.sha256(new.read(name)).digest() == hashlib.sha256(old.read(name)).digest(), name
so = new.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Wxsj]" in so and b"DIAG dumped" not in so
for i in range(5, 15):
    assert f"classes{i}.dex" in new.namelist(), i
print("  official dex/libil2cpp intact; starry classes5-14 + final so present")
PY

echo "[6/7] zipalign + sign"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
mkdir -p "$(dirname "$OUT")"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$OUT" >/dev/null

echo "[7/7] done"
ls -lh "$OUT"
shasum -a 256 "$OUT"
