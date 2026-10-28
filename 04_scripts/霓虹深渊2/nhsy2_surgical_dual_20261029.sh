#!/usr/bin/env bash
# ============================================================================
# 霓虹深渊2 r31「双轨」改包（签名=B 运行时 killer；登录=A 静态改包）— 本机(Windows)可用版
#
#   背景：AGENTS.md 2026-10-29 起 TapTap 走双轨制；经验卡＝
#         10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md §2026-10-29 增补
#   分工：签名真伪 → B 轨（classes6.dex 的 MT KillerApplication193，改父类挂载）
#         登录路由 → A 轨（classes3.dex 的 ClientLoginUtils.isClientLogin 恒 false）
#         ★旧的 TapSignKit.getSign 回灌补丁**已删**（同一链条禁止双修）
#
#   打包基座 = 已交付的 r29 成品（星光签名，已含 manifest 的 provider/service/权限注入与
#              assets/certs/starlight.crt）；只是把 dex/so/manifest 换成新的一套。
#              ⇒ 证书不变 ⇒ 装机是「同签覆盖」，不需要重新注入 manifest。
#   官方底包只用于：① 取官方证书 DER（喂给 killer）② 门禁查12 的逐字节比对基准。
#
#   用法:
#     NHSY2_ORIG_APK=/path/official_2026.10.26.apk \
#     NHSY2_MENU_APK=/path/app-release.apk \
#     NHSY2_KILLER_SMALI=/path/killer_smali \
#     bash 04_scripts/霓虹深渊2/nhsy2_surgical_dual_20261029.sh
#
#   可选: NHSY2_SO / NHSY2_WORK / NHSY2_OUT / NHSY2_SIGNED
#
#   三个输入怎么来（本机已实测）:
#     ① 官方底包：设备装着官方原版时 `adb shell pm path com.veewo.neonabyss2` + `adb pull`
#        （核对 md5 54347472319d6fc120d2d7d0faa13a4a）；已换星光包则先重下官方。
#     ② 菜单 APK：项目路径含中文会让 AGP 在 Windows 直接拒绝，先把工程拷到纯 ASCII 路径再编：
#        robocopy mod菜单/starry-mod-menu E:\starry_build\starry-mod-menu /E
#        ANDROID_HOME=<sdk> bash ./gradlew :app:assembleRelease   →  app/build/outputs/apk/release/app-release.apk
#     ③ killer smali：从斌哥包（含 MT ApkSignatureKillerEx）提取 classes8.dex 再反汇编：
#        unzip -o 02_apks/暗影格斗3/暗影格斗3_1.45.3_binge_*.apk classes8.dex
#        java -jar 01_tools/smali/baksmali-fat.jar d classes8.dex -o killer_smali
#        （脚本会自己改父类/包名/证书，别手工改原件）
# ============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="霓虹深渊2"
VER_SHORT="2026.10.26"
PKG="com.veewo.neonabyss2"
KILLER_APP="bin.mt.signature.SgKillerApp"

R29="${NHSY2_R29:-$NIXIANG/08_output/$GAME/${GAME}_${VER_SHORT}_surgical-releaseSigned.apk}"
ORIG="${NHSY2_ORIG_APK:-}"
MENU_APK="${NHSY2_MENU_APK:-}"
KILLER_SMALI="${NHSY2_KILLER_SMALI:-}"
SO_SRC="${NHSY2_SO:-$NIXIANG/mod菜单/starry-mod-menu/app/src/libs_nhsy2/arm64-v8a/libstarry_magic.so}"
WORK="${NHSY2_WORK:-$NIXIANG/07_tmp/nhsy2_dual}"
OUT="${NHSY2_OUT:-$NIXIANG/08_output/$GAME/${GAME}_${VER_SHORT}_dual-unsigned.apk}"
SIGNED="${NHSY2_SIGNED:-$NIXIANG/08_output/$GAME/${GAME}_${VER_SHORT}_dual-releaseSigned.apk}"

JAVA_BIN="${JAVA_BIN:-java}"
SMALI_JAR="$NIXIANG/01_tools/smali/smali-fat.jar"
BAKSMALI_JAR="$NIXIANG/01_tools/smali/baksmali-fat.jar"
APKTOOL_JAR="$NIXIANG/01_tools/apktool/apktool.jar"
BT="${ANDROID_BUILD_TOOLS:-/c/Users/Administrator/AppData/Local/Android/Sdk/build-tools/36.1.0}"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

die() { echo "✗ FAIL  $*" >&2; exit 1; }
step() { echo; echo "== $* =="; }

[[ -f "$R29" ]]        || die "缺 r29 打底包: $R29"
[[ -n "$ORIG" && -f "$ORIG" ]] || die "必须传 NHSY2_ORIG_APK（官方 2026.10.26 底包，用于证书与门禁比对）"
[[ -n "$MENU_APK" && -f "$MENU_APK" ]] || die "必须传 NHSY2_MENU_APK（星光菜单 APK，gradle assembleRelease 产物）"
[[ -n "$KILLER_SMALI" && -d "$KILLER_SMALI" ]] || die "必须传 NHSY2_KILLER_SMALI（MT killer smali 目录）"
[[ -f "$SO_SRC" ]]     || die "缺 libstarry_magic.so: $SO_SRC"

mkdir -p "$WORK" "$(dirname "$OUT")"

step "0/7 输入核对"
echo "  r29 基座 : $R29 ($(stat -c%s "$R29") B)"
echo "  官方底包 : $ORIG ($(stat -c%s "$ORIG") B)"
echo "  菜单 APK : $MENU_APK"
echo "  killer   : $KILLER_SMALI"
echo "  so       : $SO_SRC ($(stat -c%s "$SO_SRC") B)"

# --------------------------------------------------------------- 1/7 官方证书
step "1/7 从官方底包取证书 DER（喂 killer）"
python3 - "$ORIG" "$WORK" <<'PY'
import sys, zipfile, os
apk, work = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
cand = [n for n in z.namelist() if n.startswith("META-INF/") and n.upper().endswith(".RSA")]
assert len(cand) == 1, f"META-INF *.RSA 数量异常: {cand}"
open(os.path.join(work, "ANDROID-.RSA"), "wb").write(z.read(cand[0]))
print("  RSA 包:", cand[0], len(z.read(cand[0])), "B")
PY
openssl pkcs7 -inform DER -in "$WORK/ANDROID-.RSA" -print_certs -out "$WORK/cert.pem" 2>/dev/null \
  || die "openssl 解析 PKCS7 失败"
openssl x509 -in "$WORK/cert.pem" -outform DER -out "$WORK/official.der"
FPR="$(openssl x509 -in "$WORK/cert.pem" -noout -fingerprint -md5 | sed 's/.*=//; s/://g' | tr 'A-Z' 'a-z')"
echo "  官方证书 MD5: $FPR  (期望 bbe1da8584fb2fb1017a512b2e99ff21)"
[[ "$FPR" = "bbe1da8584fb2fb1017a512b2e99ff21" ]] || die "官方证书指纹不符（底包不对）"

# ------------------------------------------------------------- 2/7 classes3.dex
step "2/7 classes3.dex：只打 A 轨登录补丁（isClientLogin 恒 false）"
mkdir -p "$WORK/c3"
unzip -o -q "$ORIG" classes3.dex -d "$WORK/c3"
rm -rf "$WORK/c3_smali"
"$JAVA_BIN" -jar "$BAKSMALI_JAR" d "$WORK/c3/classes3.dex" -o "$WORK/c3_smali" >/dev/null
python3 - "$WORK/c3_smali" <<'PY'
import io, os, sys
root = sys.argv[1]
HEAD = ".method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z"
NEW = HEAD + """
    .registers 3

    # starry_force_web_login
    const/4 p0, 0x0

    return p0
.end method
"""
p = os.path.join(root, "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali")
t = io.open(p, encoding="utf-8").read()
if "starry_force_web_login" in t:
    print("  已打过补丁（幂等）")
    sys.exit(0)
i = t.find(HEAD)
assert i >= 0, "isClientLogin 锚点未命中"
j = t.find(".end method", i)
assert j > i
io.open(p, "w", encoding="utf-8", newline="\n").write(t[:i] + NEW + t[j + len(".end method"):])
print("  classes3.dex 补丁完成（仅登录侧）")
PY
"$JAVA_BIN" -jar "$SMALI_JAR" assemble -o "$WORK/classes3.dex" "$WORK/c3_smali"
echo "  classes3.dex(new) = $(stat -c%s "$WORK/classes3.dex") B"
grep -c "starry_force_web_login" "$WORK/c3_smali/com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali" >/dev/null \
  || die "回读缺 starry_force_web_login"
if grep -q "starry_sign_bypass" "$WORK/c3_smali/com/taptap/sdk/kit/internal/utils/TapSignKit.smali" 2>/dev/null; then
  die "TapSignKit 仍带旧签名回灌补丁（双修违规）"
fi
echo "  ✓ 登录侧补丁在、旧签名补丁不在（双轨不双修）"

# -------------------------------------------------------- 3/7 killer classes6.dex
step "3/7 killer dex（B 轨）：改包名/证书/父类 → classes6.dex"
rm -rf "$WORK/killer_smali"
cp -r "$KILLER_SMALI" "$WORK/killer_smali"
python3 - "$WORK/killer_smali" "$WORK/official.der" "$PKG" <<'PY'
import base64, io, os, sys
root, der_path, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
der = open(der_path, "rb").read()
b64 = base64.b64encode(der).decode()
wrapped = "\\n".join(b64[i:i+76] for i in range(0, len(b64), 76))
p = os.path.join(root, "bin/mt/signature/KillerApplication193.smali")
t = io.open(p, encoding="utf-8").read()
pairs = [
    (".super Lcom/wanda/sf3/wd/PkjNluikmwxGUJv;", ".super Landroid/app/Application;"),
    ("invoke-direct {p0}, Lcom/wanda/sf3/wd/PkjNluikmwxGUJv;-><init>()V",
     "invoke-direct {p0}, Landroid/app/Application;-><init>()V"),
]
for old, new in pairs:
    assert t.count(old) == 1, f"锚点 {old!r} 命中 {t.count(old)} 次"
    t = t.replace(old, new, 1)
# 包名
import re
t, n = re.subn(r'const-string v1, "[^"]*"', 'const-string v1, "%s"' % pkg, t, count=1)
assert n == 1, "包名锚点未命中"
# 证书 base64（<clinit> 里以 MIID/MIIC 开头那条）
i = t.find('const-string v0, "MI')
assert i >= 0, "证书锚点未命中"
j = t.find('"', i + len('const-string v0, '))
k = t.find('"', j + 1)
t = t[:i] + 'const-string v0, "%s"' % wrapped + t[k+1:]
io.open(p, "w", encoding="utf-8", newline="\n").write(t)
print("  killer: 父类→Application、包名=%s、证书 base64 %d 字符" % (pkg, len(b64)))
PY
# 挂载子类（manifest android:name 指向它）
cat > "$WORK/killer_smali/bin/mt/signature/SgKillerApp.smali" <<'SMALI'
.class public Lbin/mt/signature/SgKillerApp;
.super Lbin/mt/signature/KillerApplication193;

.method public constructor <init>()V
    .registers 1
    invoke-direct {p0}, Lbin/mt/signature/KillerApplication193;-><init>()V
    return-void
.end method
SMALI
"$JAVA_BIN" -jar "$SMALI_JAR" assemble -o "$WORK/classes6.dex" "$WORK/killer_smali"
rm -rf "$WORK/c6_verify"
"$JAVA_BIN" -jar "$BAKSMALI_JAR" d "$WORK/classes6.dex" -o "$WORK/c6_verify" >/dev/null
grep -q '^\.super Landroid/app/Application;' "$WORK/c6_verify/bin/mt/signature/KillerApplication193.smali" \
  || die "killer 父类未改成 Application"
grep -q "const-string v1, \"$PKG\"" "$WORK/c6_verify/bin/mt/signature/KillerApplication193.smali" \
  || die "killer 包名未改"
grep -q '^\.super Lbin/mt/signature/KillerApplication193;' "$WORK/c6_verify/bin/mt/signature/SgKillerApp.smali" \
  || die "SgKillerApp 挂载类缺失"
echo "  classes6.dex = $(stat -c%s "$WORK/classes6.dex") B ✓"

# ------------------------------------------------------------- 4/7 菜单 dex
step "4/7 星光菜单 dex（重打后不含 SignSpoof）"
unzip -o -q "$MENU_APK" classes.dex -d "$WORK/menu"
mv -f "$WORK/menu/classes.dex" "$WORK/classes5.dex"
python3 - "$WORK/classes5.dex" <<'PY'
import sys
d = open(sys.argv[1], "rb").read()
for bad in (b"Lcom/starry/magic/sign/SignSpoof;", b"SignSpoofAppComponentFactory"):
    assert bad not in d, f"菜单 dex 仍含已删类: {bad!r}"
for need in (b"StarryInitProvider", b"MenuService"):
    assert need in d, f"菜单 dex 缺 {need!r}"
print("  classes5.dex 校验通过（无 SignSpoof，含 StarryInitProvider/MenuService）")
PY

# ------------------------------------------------------------- 5/7 manifest
step "5/7 AndroidManifest：<application> 注入 android:name（B 轨挂载点）"
rm -rf "$WORK/r29man" "$WORK/mf" "$WORK/mf_only.apk"
"$JAVA_BIN" -jar "$APKTOOL_JAR" d -f -s --no-assets -o "$WORK/r29man" "$R29" >/dev/null
grep -q "android:appComponentFactory" "$WORK/r29man/AndroidManifest.xml" \
  || die "manifest 缺 appComponentFactory（基座结构漂移）"
python3 - "$WORK/r29man/AndroidManifest.xml" "$KILLER_APP" <<'PY'
import io, sys
p, name = sys.argv[1], sys.argv[2]
t = io.open(p, encoding="utf-8").read()
if 'android:name="%s"' % name in t:
    print("  已注入（幂等）"); sys.exit(0)
anchor = "<application "
assert t.count(anchor) == 1, "application 锚点异常"
t = t.replace(anchor, '<application android:name="%s" ' % name, 1)
io.open(p, "w", encoding="utf-8", newline="").write(t)
print("  manifest 注入 android:name=%s" % name)
PY
"$JAVA_BIN" -jar "$APKTOOL_JAR" b -f "$WORK/r29man" -o "$WORK/mf_only.apk" >/dev/null
mkdir -p "$WORK/mf"
unzip -o -q "$WORK/mf_only.apk" AndroidManifest.xml -d "$WORK/mf"
cp -f "$WORK/mf/AndroidManifest.xml" "$WORK/AndroidManifest.xml"
echo "  binary manifest = $(stat -c%s "$WORK/AndroidManifest.xml") B"

# ------------------------------------------------------------- 6/7 合并打包
step "6/7 zip 合并 → $OUT"
python3 - "$R29" "$WORK" "$OUT" "$SO_SRC" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, work, dst, so = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), Path(sys.argv[4])
mf = (work / "AndroidManifest.xml").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
c5 = (work / "classes5.dex").read_bytes()
c6 = (work / "classes6.dex").read_bytes()
so_bytes = so.read_bytes()

def is_sig(name):
    if not name.startswith("META-INF/"):
        return False
    if name == "META-INF/MANIFEST.MF":
        return True
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

REPLACE = {"AndroidManifest.xml": mf, "classes3.dex": c3, "classes5.dex": c5,
           "lib/arm64-v8a/libstarry_magic.so": so_bytes}
ADD = {"classes6.dex": c6}
UNTOUCHED = ["classes.dex", "classes2.dex", "classes4.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so"]

zin = zipfile.ZipFile(src)
names = set(zin.namelist())
for n in ["AndroidManifest.xml", "classes3.dex", "classes5.dex", "lib/arm64-v8a/libstarry_magic.so"]:
    assert n in names, f"基座缺 {n}"
assert "classes6.dex" not in names, "基座已含 classes6.dex"
for n in UNTOUCHED:
    assert n in names, f"基座缺 {n}"

zout = zipfile.ZipFile(dst, "w")
cnt = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name):
        continue
    data = REPLACE.get(name)
    if data is not None:
        ctype = zipfile.ZIP_DEFLATED
    else:
        data, ctype = zin.read(item), item.compress_type
        if name == "resources.arsc":
            ctype = zipfile.ZIP_STORED
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = ctype
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    cnt += 1
for name, data in ADD.items():
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    cnt += 1
zin.close(); zout.close()

# 闭环核对
zi, zo = zipfile.ZipFile(src), zipfile.ZipFile(dst)
for n in UNTOUCHED:
    assert hashlib.sha256(zi.read(n)).hexdigest() == hashlib.sha256(zo.read(n)).hexdigest(), f"被改动: {n}"
for n in ("classes3.dex", "classes5.dex", "classes6.dex",
          "lib/arm64-v8a/libstarry_magic.so", "assets/certs/starlight.crt",
          "META-INF/services/java.security.Provider"):
    assert n in zo.namelist(), f"缺 {n}"
assert zo.read("classes6.dex") == c6
assert zo.read("AndroidManifest.xml") == mf
assert zo.read("lib/arm64-v8a/libstarry_magic.so") == so_bytes
assert not any(x.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for x in zo.namelist()), "签名残留"
print(f"  条目 {cnt}；未触碰 6 项 sha256 逐字节一致；classes6/manifest/so 回读一致；无签名残留 ✓")
PY

# ------------------------------------------------------------- 7/7 对齐+签名
step "7/7 zipalign + apksigner（星光证书）"
# Windows 的 build-tools 里 apksigner 是 .bat（bash 直接调不了），优先取 .bat
APKSIGNER="$BT/apksigner"
[[ -x "$APKSIGNER" ]] || APKSIGNER="$BT/apksigner.bat"
[[ -f "$APKSIGNER" ]] || die "找不到 apksigner（${BT}）"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$OUT" "$ALIGNED"
cmd //c "$(cygpath -w "$APKSIGNER")" sign --ks "$(cygpath -w "$KS")" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --out "$(cygpath -w "$WORK/signed.apk")" "$(cygpath -w "$ALIGNED")"
cmd //c "$(cygpath -w "$APKSIGNER")" verify --print-certs "$(cygpath -w "$WORK/signed.apk")" | head -6
mv -f "$WORK/signed.apk" "$WORK/final.apk"
# ★签名产物落独立文件名（与 unsigned 分开；Windows 上就地覆盖会因文件被占用失败）
cp -f "$WORK/final.apk" "$SIGNED"
echo
echo "✓ 双轨改包完成: $SIGNED ($(stat -c%s "$SIGNED") B)"
echo "  未签名件（便于复核）: $OUT"
echo "  下一关: pre_install_gate.sh --apk $SIGNED --base-apk $ORIG"
