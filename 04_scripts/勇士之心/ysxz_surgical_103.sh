#!/usr/bin/env bash
# =============================================================================
#  勇士之心 1.0.3（TapTap .tt1 干净底包）星光外科组装 —— 可移植版（Windows Git-Bash / macOS）
#
#  与 scripts/surgical_ysxz_1.0.3.sh 的区别（那份只适用于旧的 mac 侧底包）：
#    * 底包换成 02_apks/勇士之心.apk（com.fightingstars.games.warrior.tt1 / vc3 / arm64-v8a）
#    * 该底包官方自带 classes.dex..classes6.dex ⇒ 星光 dex 落 classes7.dex
#    * 官方 so 是重编版（evalString 在 .dynsym），原生定位由 YsxzMod.cpp 的
#      候选符号优先 + RVA 兜底负责，本脚本不碰 libcocos2djs.so
#    * 工具链走 JAVA_HOME / ANDROID_HOME / NDK，不写死 mac 路径
#
#  只做三件事：换 AndroidManifest（追加星光组件）→ 加 classes7.dex / libstarry_magic.so /
#  libc++_shared.so / assets/certs → 其余条目逐字节保留；然后 zipalign + 同钥匙重签 + 校验。
#
#  用法：
#    bash 04_scripts/勇士之心/ysxz_surgical_103.sh
#    ORIG_APK=/path/to/base.apk bash 04_scripts/勇士之心/ysxz_surgical_103.sh
#  环境：YSXZ_SO / YSXZ_DEX 可覆盖输入产物；OUT 可覆盖输出目录。
# =============================================================================
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${NIXIANG:-$(cd "$HERE/../.." && pwd)}"
GAME="勇士之心"
VERSION="1.0.3"
PKG="com.fightingstars.games.warrior.tt1"
NEW_DEX="classes7.dex"          # 官方已有 classes.dex..classes6.dex

ORIG_APK="${ORIG_APK:-$ROOT/02_apks/${GAME}.apk}"
SO="${YSXZ_SO:-$ROOT/mod菜单/starry-mod-menu/libs_ysxz/arm64-v8a/libstarry_magic.so}"
DEX="${YSXZ_DEX:-$ROOT/mod菜单/starry-mod-menu/libs_ysxz/classes4.dex}"
CERT_CRT="$ROOT/mod菜单/starry-mod-menu/app/src/main/assets/certs/starlight.crt"
OUT="${OUT:-$ROOT/08_output/$GAME}"
APKNAME="${GAME}_${VERSION}_starry-final-releaseSigned.apk"

KS="${KS:-$ROOT/01_tools/starry_release.jks}"
KS_PASS="${KS_PASS:-starry2026}"
KS_ALIAS="${KS_ALIAS:-starry}"
WANT_CERT="83C5CC0BE218D046B3B7AF0D2A9F8236297FE228171AF2974B71BAC130736F1D"

APKTOOL_JAR="$ROOT/01_tools/apktool/apktool.jar"

say() { printf '%s\n' "$*" >&2; }
die() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }

# ---------------- 0. 工具链 ----------------
JAVA="${JAVA_HOME:+$JAVA_HOME/bin/}java"
command -v "$JAVA" >/dev/null 2>&1 || die "找不到 java（设 JAVA_HOME 到 JDK 17）"
SDK="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-}}"
[[ -n "$SDK" && -d "$SDK" ]] || die "找不到 Android SDK（设 ANDROID_HOME）"
BT="$(ls -d "$SDK"/build-tools/*/ 2>/dev/null | sort -V | tail -1 || true)"
BT="${BT%/}"
[[ -n "$BT" ]] || die "找不到 build-tools"
ZL="$BT/zipalign"; [[ -x "$ZL.exe" ]] && ZL="$ZL.exe"
AAPT2="$BT/aapt2"; [[ -x "$AAPT2.exe" ]] && AAPT2="$AAPT2.exe"
AS_JAR="$BT/lib/apksigner.jar"
[[ -f "$AS_JAR" ]] || die "缺 apksigner.jar: $AS_JAR"
NDK="${ANDROID_NDK_HOME:-$SDK/ndk/27.0.12077973}"
CXX_SO="$NDK/toolchains/llvm/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"

# ---------------- 1. 输入断言 ----------------
[[ -f "$ORIG_APK" ]] || die "缺底包 $ORIG_APK"
[[ -f "$SO" ]] || die "缺 so $SO"
[[ -f "$DEX" ]] || die "缺菜单 dex $DEX"
[[ -f "$CERT_CRT" ]] || die "缺授权证书 $CERT_CRT"
[[ -f "$APKTOOL_JAR" ]] || die "缺 $APKTOOL_JAR"
[[ -f "$CXX_SO" ]] || die "缺 libc++_shared.so（NDK: ${NDK}）"

# so 侧：必须是 1.0.6 全集菜单（含战斗加速），且带新版 evalString 定位
for tok in "Slider_伤害倍率:1:50:1" "Slider_移动速度:1:10:1" "Slider_战斗加速:1:5:1" "Toggle_自动战斗" \
           "Toggle_解锁金卡" "Toggle_自动穿戴" "evalString resolved by dynsym" "readable-map fallback"; do
  LC_ALL=C grep -aqF -- "$tok" "$SO" || die "so 缺 [$tok]"
done
LC_ALL=C grep -aqF -- "Slider_移动速度:1:5:1" "$SO" && die "so 是旧菜单（含 Slider_移动速度:1:5:1）"
# dex 侧：新外观 + 不含 1.0.4 过检测类
LC_ALL=C grep -aqF -- 'MagicRes$PanelBgDrawable' "$DEX" || die "dex 无 PanelBgDrawable 外观串（旧外观，先跑 ysxz_build_menu_dex.sh）"
LC_ALL=C grep -aqF -- 'YsxzAppComponentFactory' "$DEX" && die "dex 混进 1.0.4 过检测类"

WORK="$(mktemp -d "${TMPDIR:-/tmp}/ysxz_surg103.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT"

# ---------------- 2. AndroidManifest：保留官方 Application，只追加星光组件 ----------
say "[1/6] 解 AndroidManifest（保留官方 Application）"
"$JAVA" -jar "$(cygpath -w "$APKTOOL_JAR" 2>/dev/null || echo "$APKTOOL_JAR")" d -f -s --no-assets \
  -o "$(cygpath -w "$WORK/orig_dec")" "$(cygpath -w "$ORIG_APK")" >/dev/null

python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if "FOREGROUND_SERVICE\"" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg
    t = t.replace("</application>", prov + "\n    </application>", 1)
if "com.starry.magic.menu.MenuService" not in t:
    svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  manifest patched: provider + service + foreground permissions")
PY

"$JAVA" -jar "$(cygpath -w "$APKTOOL_JAR" 2>/dev/null || echo "$APKTOOL_JAR")" b -f \
  "$(cygpath -w "$WORK/orig_dec")" -o "$(cygpath -w "$WORK/mf_only.apk")" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
[[ -s "$WORK/AndroidManifest.xml" ]] || die "manifest 重建失败"

# aapt2 复核：星光组件在、官方 Application 在、官方节点一个不少
"$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt" 2>/dev/null || true
python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
"$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null \
  || die "aapt2 解析不了新 manifest"
grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || die "provider 缺失"
grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" || die "service 缺失"
grep -q "FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" || die "FGS 权限缺失"
grep -qE "WarriorApplication|org\.cocos2dx" "$WORK/tree_new.txt" || die "官方 Application 丢了"
if [[ -s "$WORK/tree_orig.txt" ]]; then
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$D" == "0" ]] || die "manifest 丢失官方节点 $D 行"
  say "  ✓ manifest 官方节点零丢失"
fi

# ---------------- 3. zip 外科合并 ----------------
say "[2/6] zip 外科合并（其余条目逐字节保留）"
UNSIGNED="$WORK/${GAME}_${VERSION}_starry-final-unsigned.apk"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" "$SO" "$DEX" "$CERT_CRT" "$CXX_SO" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
src, work, dst, new_dex, so_p, dex_p, crt_p, cxx_p = sys.argv[1:9]
src, work, dst = Path(src), Path(work), Path(dst)
mf = (work / "AndroidManifest.xml").read_bytes()
adds = {
    new_dex: Path(dex_p).read_bytes(),
    "lib/arm64-v8a/libstarry_magic.so": Path(so_p).read_bytes(),
    "lib/arm64-v8a/libc++_shared.so": Path(cxx_p).read_bytes(),
    "assets/certs/starlight.crt": Path(crt_p).read_bytes(),
}
untouched = ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes5.dex",
             "classes6.dex", "resources.arsc", "lib/arm64-v8a/libcocos2djs.so",
             "lib/arm64-v8a/libsecsdk.so", "assets/main.js", "assets/src/cocos2d-jsb.js",
             "assets/src/settings.js", "assets/assets/main/index.js"]

def is_sig(name):
    up = name.upper()
    if not up.startswith("META-INF/"):
        return False
    return up.endswith((".SF", ".RSA", ".DSA", ".EC")) or up.endswith("MANIFEST.MF")

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
for n in untouched:
    assert n in names, "orig apk missing %s" % n
assert new_dex not in names, "already has %s" % new_dex
kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]

zout = zipfile.ZipFile(dst, "w", zipfile.ZIP_DEFLATED, allowZip64=True)
n = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in adds:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = item.create_system
    zout.writestr(info, data)
    n += 1
for name, data in adds.items():
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

z1 = zipfile.ZipFile(src, "r"); z2 = zipfile.ZipFile(dst, "r")
bad = []
for name in untouched:
    if hashlib.sha256(z1.read(name)).digest() != hashlib.sha256(z2.read(name)).digest():
        bad.append(name)
out_names = set(z2.namelist())
for name in kept_meta:
    if name not in out_names:
        bad.append("lost:" + name)
for name in adds:
    if name not in out_names:
        bad.append("missing:" + name)
if any(is_sig(x) for x in out_names):
    bad.append("signature entries leaked")
z1.close(); z2.close()
print("  entries=%d untouched=%d/%d verified" % (n, len(untouched), len(untouched)))
if bad:
    print("  ERROR:", bad[:8])
    sys.exit(1)
PY

# ---------------- 4. 对齐 + 签名 ----------------
say "[3/6] zipalign"
ALIGNED="$WORK/aligned.apk"
"$ZL" -p -f 4 "$UNSIGNED" "$ALIGNED" 2>/dev/null || "$ZL" -f 4 "$UNSIGNED" "$ALIGNED" >/dev/null
"$ZL" -c -v 4 "$ALIGNED" >/dev/null 2>&1 || die "zipalign 校验失败"

say "[4/6] 重签名（星光同一把钥匙）"
[[ -f "$KS" ]] || die "缺钥匙 $KS"
SIGNED="$OUT/$APKNAME"
"$JAVA" -jar "$AS_JAR" sign \
  --ks "$KS" --ks-pass "pass:$KS_PASS" --ks-key-alias "$KS_ALIAS" --key-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$ALIGNED"

# ---------------- 5. 校验 ----------------
say "[5/6] 校验"
CERT_OUT="$("$JAVA" -jar "$AS_JAR" verify --print-certs "$SIGNED" 2>&1 || true)"
GOT="$(printf '%s\n' "$CERT_OUT" | grep -i 'SHA-256 digest' | head -1 | grep -oiE '[0-9a-f:]{64,95}' | tr -d ':' | tr 'a-f' 'A-F' || true)"
[[ "$GOT" == "$WANT_CERT" ]] || die "证书指纹不一致 got=[$GOT]"
python3 - "$SIGNED" "$SO" "$DEX" "$NEW_DEX" "$ORIG_APK" <<'PY'
import hashlib, sys, zipfile
apk, so, dex, new_dex, orig = sys.argv[1:6]
z = zipfile.ZipFile(apk); zo = zipfile.ZipFile(orig)
def md5b(b): return hashlib.md5(b).hexdigest()
def md5f(p): return hashlib.md5(open(p, "rb").read()).hexdigest()
ok = True
def rep(label, got, exp=None):
    global ok
    good = exp is None or got == exp
    ok = ok and good
    print("    %-32s %s%s" % (label, got, "" if exp is None else ("  OK" if good else "  MISMATCH " + exp)))
rep("classes7.dex(md5)", md5b(z.read(new_dex)), md5f(dex))
rep("libstarry_magic.so(md5)", md5b(z.read("lib/arm64-v8a/libstarry_magic.so")), md5f(so))
for n in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes5.dex",
          "classes6.dex", "resources.arsc", "lib/arm64-v8a/libcocos2djs.so", "assets/main.js"):
    same = md5b(z.read(n)) == md5b(zo.read(n))
    rep(n, md5b(z.read(n)), md5b(zo.read(n)))
sys.exit(0 if ok else 1)
PY

# ---------------- 6. 交付信息 ----------------
say "[6/6] 产物"
SZ="$(wc -c <"$SIGNED" | tr -d ' ')"
say "  包   : $SIGNED"
say "  体积 : $SZ B"
say "  md5  : $(md5sum "$SIGNED" | awk '{print $1}')"
say "  sha  : $(sha256sum "$SIGNED" | awk '{print $1}')"
say "  so   : $(md5sum "$SO" | awk '{print $1}')"
say "  dex  : $(md5sum "$DEX" | awk '{print $1}')"
say "  done"
