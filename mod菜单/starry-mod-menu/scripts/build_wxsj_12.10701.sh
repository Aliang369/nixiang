#!/usr/bin/env bash
# 无限升级 12.10701 / vc1644 — 外科组装（禁止对游戏 apktool b 整包）
# 方案：官方 base.apk + starry classes2.dex/so/assets 外科 zip 追加；官方 dex/resources.arsc/libil2cpp 逐字节透传。
# 注入方式：ContentProvider（com.starry.magic.StarryInitProvider），【禁止替换 Application】
#   官方 Application = com.netease.nis.wrapper.MyApplication（网易易盾壳，vc1644 起引入）。
#   替换 Application 会绕过壳的运行时 dex 解密 → handleBindApplication 早期崩溃：
#   "Unable to get provider com.taptap.sdk.themis.lite.ThemisProvider: ClassNotFoundException"（2026-09-22 实测）。
#   starry dex 只装 com/starry（smali 组装），StarryApp.super → Lcom/netease/nis/wrapper/MyApplication;（Manifest 不走它）。
# 阶段（WXSJ_PHASE）：
#   diag    → 08_output/无限升级/无限升级_12.10701_diag.apk          （so 必须含 DIAG dumped）
#   release → 08_output/无限升级/无限升级_12.10701_surgical-final-releaseSigned.apk
#
# 前置：
#   - 官方底包：07_patched/无限升级/installed_orig_20260922/base.apk（vc1644，不可再生）
#   - starry 模板 APK：mod菜单/starry-mod-menu/app/build/outputs/apk/debug/app-debug.apk
#   - so：app/src/libs/arm64-v8a/libstarry_magic.so（scripts/wxsj_so_only.sh 产出）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
ORIG_APK="$NIXIANG/07_patched/无限升级/installed_orig_20260922/base.apk"
STARRY_APK="$NIXIANG/mod菜单/starry-mod-menu/app/build/outputs/apk/debug/app-debug.apk"
FINAL_SO="$NIXIANG/mod菜单/starry-mod-menu/app/src/libs/arm64-v8a/libstarry_magic.so"
JNILIBS_SO="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

PHASE="${WXSJ_PHASE:-release}"
case "$PHASE" in
  diag)    OUT="$NIXIANG/08_output/无限升级/无限升级_12.10701_diag.apk" ;;
  release) OUT="$NIXIANG/08_output/无限升级/无限升级_12.10701_surgical-final-releaseSigned.apk" ;;
  *) echo "ERROR: WXSJ_PHASE 只接受 diag|release（当前 '$PHASE'）"; exit 1 ;;
esac

for f in "$ORIG_APK" "$STARRY_APK" "$FINAL_SO" "$KS"; do
  [[ -f "$f" ]] || { echo "ERROR: missing $f"; exit 1; }
done

# so 与阶段一致性（防「DIAG so 出正式包」/「正式 so 出取证包」）
if [[ "$PHASE" == "diag" ]]; then
  LC_ALL=C grep -aq 'DIAG dumped' "$FINAL_SO" || { echo "ERROR: DIAG 阶段但 so 无 DIAG dumped（先跑 WXSJ_DIAG=1 scripts/wxsj_so_only.sh）"; exit 1; }
else
  if LC_ALL=C grep -aq 'DIAG dumped' "$FINAL_SO"; then echo "ERROR: release 阶段但 so 仍含 DIAG dumped（取证版 so，禁出正式包）"; exit 1; fi
fi
LC_ALL=C grep -aq '\[Wxsj\]' "$FINAL_SO" || { echo "ERROR: so 缺 [Wxsj] 日志串"; exit 1; }

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
mkdir -p "$WORK/stage/lib/arm64-v8a" "$WORK/stage/assets/certs"

echo "[1/7] decode official manifest/resources ($PHASE)"
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
# 官方 Application = com.netease.nis.wrapper.MyApplication（网易易盾壳）：【禁止改 android:name】
existing_app = app.get(q("name"))
if existing_app != "com.netease.nis.wrapper.MyApplication":
    print(f"  WARN: 官方 application@name = {existing_app}（预期 易盾 MyApplication）")

providers = {e.get(q("name")): e for e in app.findall("provider")}
sp = providers.get("com.starry.magic.StarryInitProvider")
if sp is None:
    sp = ET.SubElement(app, "provider", {
        q("name"): "com.starry.magic.StarryInitProvider",
        q("authorities"): "com.xingzuodaoshi.infinityUpgrade.starryinit",
        q("exported"): "false",
    })
# 【2026-09-30 修复 tapsdk 包名/签名误报】initOrder=MAX：声明位置保持最后一位，但由系统最先初始化，
# 使 StarryInitProvider.onCreate 里的 SignSpoof.install() 早于所有 TapTap provider（TapTapKitInitProvider/
# ThemisProvider）与 TapSDK init；TapSDK 的 TapInitializer 读 PackageManager 签名校验应用配置，
# 伪装未生效时打「应用的包名、签名未在 TapTap 开发者中心正确配置」⇒ 二次启动无法进入游戏。
# 禁用替代方案：把 provider 声明挪到第 1 位会被易盾壳静默跳过（整个 onCreate 不执行，2026-09-30 实测）。
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
print(f"  manifest: application@name={existing_app} 保持 + StarryInitProvider(initOrder=2147483647) + MenuService + 3 permissions")
PY

echo "[3/7] rebuild resources/manifest only"
apktool b "$WORK/base_dec" -o "$WORK/rebuilt.apk" >/dev/null
unzip -p "$WORK/rebuilt.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

echo "[4/7] stage starry so/assets + starry classes2.dex（smali 组装：只装 com/starry）"
# starry dex 起始编号由官方 dex 数决定（vc1644 官方仅 classes.dex；编号必须连续，
# 缺号会让 ART 在第一个断号处停止加载后续 dex）。
OFFICIAL_DEX_NAMES="$(unzip -Z1 "$ORIG_APK" | grep -E '^classes[0-9]*\.dex$' | sort -V || true)"
OFFICIAL_DEX_COUNT="$(printf '%s\n' "$OFFICIAL_DEX_NAMES" | grep -c . || true)"
[[ "$OFFICIAL_DEX_COUNT" -ge 1 ]] || { echo "ERROR: 官方包无 classes*.dex"; exit 1; }
STARRY_DEX="classes$((OFFICIAL_DEX_COUNT + 1)).dex"
echo "  official dex count = $OFFICIAL_DEX_COUNT ($(printf '%s ' $OFFICIAL_DEX_NAMES)) → starry $STARRY_DEX"

# 菜单 dex 来源：优先用仓库内「已定型菜单模板」（= 装机在用那套 com/starry）。
# 2026-09-26 已把模板按当前源码重建（scripts/wxsj_refresh_menu_tpl.sh，外科 zip 组装）：
#   旧模板 wxsj_menu_20260922 落后于 so/Java → dex 未声明 nativeGetBuildStamp()
#   → RegisterNatives NoSuchMethodError → JNI_OnLoad 失败 → 整条注入链不启动。
#   新模板 wxsj_menu_20260926 相对旧模板是**严格超集**：242→244 类，0 删除，
#   新增 GyzwHooks / OtaSoStamp；MagicRes$*Drawable 绘制类全在 ⇒ 无外观回归。
#   （09-23 那次「重建就变样」的根因是**当时源码本身缺**这些绘制类，现源码已补齐。）
# 模板目录内 classes2.dex 已含 StarryInitProvider 且 StarryApp.super 已改易盾壳类，故走模板时
# 跳过 apktool/smali 组装与相关断言；libc++_shared.so / starlight.crt 也取自同一模板。
MENU_TPL_DIR="${WXSJ_MENU_TPL:-$NIXIANG/mod菜单/starry-mod-menu/templates/wxsj_menu_20260926}"
MENU_FROM_TPL=0
if [[ -s "$MENU_TPL_DIR/$STARRY_DEX" ]]; then
  MENU_FROM_TPL=1
  cp -f "$MENU_TPL_DIR/$STARRY_DEX" "$WORK/stage/$STARRY_DEX"
  echo "  menu dex: 仓库模板 $MENU_TPL_DIR/$STARRY_DEX ($(stat -f %z "$WORK/stage/$STARRY_DEX") B，跳过 apktool/smali 组装)"
else

# 只装 com/starry：易盾壳主 dex 不动，避免模板整包 10 dex 干扰壳的 dex 校验/加载序。
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT/com/starry"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry/" "$SMALI_ROOT/com/starry/"
done
shopt -u nullglob
for f in StarryApp StarryInitProvider; do
  [[ -f "$SMALI_ROOT/com/starry/magic/$f.smali" ]] || { echo "ERROR: $f.smali missing in starry template"; exit 1; }
done
python3 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
APP = "Lcom/netease/nis/wrapper/MyApplication;"
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", f".super {APP}"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        f"invoke-direct {{p0}}, {APP}-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        f"invoke-super {{p0, p1}}, {APP}->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        f"invoke-super {{p0}}, {APP}->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if APP not in t:
    raise SystemExit(f"ERROR: StarryApp.smali missing {APP}")
if ".super Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali still .super Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp.super -> 易盾 {APP} ok ({changed} edits)")
PY
"$NIXIANG/01_tools/smali/smali" assemble -o "$WORK/stage/$STARRY_DEX" "$SMALI_ROOT" >/dev/null
[[ -s "$WORK/stage/$STARRY_DEX" ]] || { echo "ERROR: $STARRY_DEX 组装失败"; exit 1; }
echo "  assembled $STARRY_DEX ($(stat -f %z "$WORK/stage/$STARRY_DEX") B)"
fi

cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
cp -f "$WORK/AndroidManifest.xml" "$WORK/stage/AndroidManifest.xml"
cp -f "$FINAL_SO" "$WORK/stage/lib/arm64-v8a/libstarry_magic.so"
if [[ "$MENU_FROM_TPL" == 1 ]]; then
  cp -f "$MENU_TPL_DIR/libc++_shared.so" "$WORK/stage/lib/arm64-v8a/libc++_shared.so"
  cp -f "$MENU_TPL_DIR/starlight.crt" "$WORK/stage/assets/certs/starlight.crt"
else
  unzip -p "$STARRY_APK" lib/arm64-v8a/libc++_shared.so > "$WORK/stage/lib/arm64-v8a/libc++_shared.so"
  unzip -p "$STARRY_APK" assets/certs/starlight.crt > "$WORK/stage/assets/certs/starlight.crt"
fi
[[ -s "$WORK/stage/assets/certs/starlight.crt" ]] || { echo "ERROR: starry cert missing"; exit 1; }
LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$WORK/stage/$STARRY_DEX" \
  || { echo "ERROR: $STARRY_DEX 内缺 StarryInitProvider"; exit 1; }

(cd "$WORK/stage" && zip -u "$WORK/unsigned.apk" \
  AndroidManifest.xml \
  lib/arm64-v8a/libstarry_magic.so \
  lib/arm64-v8a/libc++_shared.so \
  assets/certs/starlight.crt \
  "$STARRY_DEX") >/dev/null

echo "[5/7] verify merged structure"
"$BT/aapt2" dump xmltree "$WORK/rebuilt.apk" --file AndroidManifest.xml > "$WORK/mf_tree.txt" 2>&1
grep -q 'com.starry.magic.StarryInitProvider' "$WORK/mf_tree.txt" || { echo "ERROR: StarryInitProvider missing"; exit 1; }
# 【2026-09-30】必须带 initOrder=2147483647：否则星空层晚于 TapTap provider 初始化，TapSDK 会误报包名/签名错误
grep -q 'android:initOrder(0x0101001a)=2147483647' "$WORK/mf_tree.txt" \
  || { echo "ERROR: StarryInitProvider 缺 android:initOrder=2147483647（签名伪装将晚于 TapSDK init）"; exit 1; }
grep -q 'com.starry.magic.menu.MenuService' "$WORK/mf_tree.txt" || { echo "ERROR: MenuService missing"; exit 1; }
grep -q 'com.netease.nis.wrapper.MyApplication' "$WORK/mf_tree.txt" || { echo "ERROR: 易盾 Application 丢失（禁）"; exit 1; }
if grep -q 'com.starry.magic.StarryApp' "$WORK/mf_tree.txt"; then
  echo "ERROR: manifest 仍把 StarryApp 当 application@name（禁：会绕过易盾壳）"; exit 1
fi
python3 - "$WORK/unsigned.apk" "$ORIG_APK" "$PHASE" "$STARRY_DEX" <<'PY'
import hashlib, sys, zipfile
new, old = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
phase, starry_dex = sys.argv[3], sys.argv[4]
old_names = set(old.namelist())
# 官方 dex / resources.arsc / libil2cpp.so 必须逐字节原样（只追加，不改官方文件）
checked = [n for n in old_names if n in ("lib/arm64-v8a/libil2cpp.so", "resources.arsc")
           or (n.startswith("classes") and n.endswith(".dex"))]
for name in sorted(checked):
    assert hashlib.sha256(new.read(name)).digest() == hashlib.sha256(old.read(name)).digest(), name
assert "lib/arm64-v8a/libil2cpp.so" in checked, "libil2cpp.so not found in official apk"
so = new.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Wxsj]" in so, "starry so missing [Wxsj]"
if phase == "diag":
    assert b"DIAG dumped" in so, "diag phase but so lacks DIAG dumped"
else:
    assert b"DIAG dumped" not in so, "release phase but so carries DIAG dumped"
assert starry_dex in new.namelist(), f"{starry_dex} missing"
assert b"com/starry/magic/StarryInitProvider" in new.read(starry_dex), "StarryInitProvider not in starry dex"
print(f"  official {len(checked)} files intact; {starry_dex}(com/starry) + so present ({phase})")
PY

echo "[6/7] zipalign + sign"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
mkdir -p "$(dirname "$OUT")"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$OUT" >/dev/null

echo "[7/7] done ($PHASE)"
ls -lh "$OUT"
shasum -a 256 "$OUT"
