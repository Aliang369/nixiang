#!/usr/bin/env bash
# 我的技能无限触发 1.6.3 (com.SmallMill.EnMaster, vc163) — 外科组装（直装版，禁 apktool b 整包）
#
# 原包零改动：classes.dex / classes2.dex / resources.arsc / lib/**.so / global-metadata.dat 逐字节不变。
# 改动只有三项：
#   1. AndroidManifest：新增 StarryInitProvider + MenuService + FOREGROUND_SERVICE(-SPECIAL_USE)；
#      官方 application 无 android:name、appComponentFactory 为 androidx CoreComponentFactory，均保持原样
#      （环境阶段不挂 killer；TapTap 双轨制在登录关单独做）
#   2. 新增 classes3.dex（星光 smali：StarryInitProvider / MenuService / 菜单 UI / auth）
#   3. 新增 lib/arm64-v8a/libstarry_magic.so（Wdjnwcf 版）+ libc++_shared.so
#   ※ 移除官方签名条目 META-INF/*.SF|RSA（重签必然）
#
# 产物：08_output/我的技能无限触发/我的技能无限触发_1.6.3_surgical-unsigned.apk
#
# 用法: bash 04_scripts/我的技能无限触发/surgical_wdjnwcf_1.6.3.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="我的技能无限触发"
VERSION="1.6.3"
PKG="com.SmallMill.EnMaster"
ORIG_APK="$NIXIANG/02_apks/我的技能无限触发_官方_com.SmallMill.EnMaster.apk"
ORIG_MD5="2c62f08035a4a63c83e96866ed148a58"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
NEW_DEX="classes3.dex"
# ---- TapTap 双轨制（2026-10-29 口径）----
# B 轨 签名/完整性＝运行时签名替换：MT KillerApplication193（已烘焙本包名 + 官方证书 DER），
#      由 com.starry.magic.sign.StarryAppComponentFactory 在 instantiateApplication 里 mount。
# A 轨 登录方式＝改包法：官方 classes.dex 的 LoginBlock.onViewCreated 由 SSO 改走 SDK 自带 web 登录。
KILLER_DEX="${WDJNWCF_KILLER_DEX:-$NIXIANG/04_scripts/我的技能无限触发/assets/wdjnwcf_signature_killer.dex}"
KILLER_MD5="${WDJNWCF_KILLER_MD5:-b078f98afad45d70e1b4f357dc60ca3b}"
PATCHED_C1="${WDJNWCF_PATCHED_CLASSES:-$NIXIANG/07_patched/我的技能无限触发/classes_patched.dex}"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
SO_PREBUILT="${WDJNWCF_SO:-$STARRY_SRC/libs_wdjnwcf/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing so ${SO_PREBUILT}（先跑 wdjnwcf_build_so.sh）" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || { echo "ERROR: 原包指纹不符" >&2; exit 1; }
LC_ALL=C grep -aq 'Wdjnwcf' "$SO_PREBUILT" || { echo "ERROR: so 缺 Wdjnwcf 标签" >&2; exit 1; }
[[ -f "$KILLER_DEX" ]] || { echo "ERROR: missing killer dex ${KILLER_DEX}（先跑 build_killer_dex_wdjnwcf.sh）" >&2; exit 1; }
[[ "$(md5 -q "$KILLER_DEX")" == "$KILLER_MD5" ]] || { echo "ERROR: killer dex md5 与记录不符" >&2; exit 1; }
[[ -f "$PATCHED_C1" ]] || { echo "ERROR: missing ${PATCHED_C1}（先跑 patch_taptap_login_wdjnwcf.sh）" >&2; exit 1; }
if LC_ALL=C grep -aq 'Bysxy\]\|Byyxx2\]\|Nhsy2\]\|Zjmc\]' "$SO_PREBUILT"; then
  echo "ERROR: so 残留上一款游戏标签" >&2; exit 1
fi

LOCK_DIR="/tmp/wdjnwcf_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held" >&2; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wdjnwcf_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/6] gradle assembleRelease (starry → smali)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] apktool d starry → com/starry smali → $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" ]] \
  || { echo "ERROR: StarryInitProvider.smali missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing" >&2; exit 1; }
# B 轨：killer smali 装进同一个新 dex（StarryAppComponentFactory 需能 Class.forName 到）
"$BAKSMALI_BIN" d -o "$WORK/killer_smali" "$KILLER_DEX" >/dev/null
rsync -a "$WORK/killer_smali/bin" "$SMALI_ROOT/"
[[ -f "$SMALI_ROOT/bin/mt/signature/KillerApplication193.smali" ]] \
  || { echo "ERROR: killer smali missing after unpack" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/sign/StarryAppComponentFactory.smali" ]] \
  || { echo "ERROR: StarryAppComponentFactory.smali missing" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

echo "[2b/6] A 轨：官方 classes.dex → 补丁版（LoginBlock: SSO → SDK web 登录）"
cp -f "$PATCHED_C1" "$WORK/classes_patched.dex"
[[ -s "$WORK/classes_patched.dex" ]] || { echo "ERROR: 补丁 classes.dex 为空" >&2; exit 1; }

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"; cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/6] AndroidManifest（官方 application/appComponentFactory 保持原样）"
apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
for perm in ("FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    if perm not in t:
        perms.append('    <uses-permission android:name="android.permission.%s"/>' % perm)
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
# B 轨挂载点：appComponentFactory → 星光入口（instantiateApplication 里 mount killer，
# 早于真实 Application.attachBaseContext/onCreate 与全部 ContentProvider）
import re as _re
t, n_fac = _re.subn(r'android:appComponentFactory="[^"]*"',
                    'android:appComponentFactory="com.starry.magic.sign.StarryAppComponentFactory"', t)
if n_fac != 1:
    raise SystemExit("appComponentFactory 替换次数异常: %d" % n_fac)
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
apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt"
  python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
  "$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || { echo "ERROR: provider missing" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" || { echo "ERROR: FGS permission missing" >&2; exit 1; }
  grep -q "com.starry.magic.sign.StarryAppComponentFactory" "$WORK/tree_new.txt" || { echo "ERROR: appComponentFactory 未换成星光入口" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep '^<' | grep -vc 'appComponentFactory' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 丢失/改动官方节点 $DIFF_D 行" >&2; exit 1; }
  echo "  manifest 闭环 ✓（官方节点零丢失）"
fi

echo "[4/6] zip surgical merge → $UNSIGNED"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
src, work, dst, new_dex = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
patched1 = (work / "classes_patched.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
SIG_NAMES = {"META-INF/MANIFEST.MF"}
untouched = ["classes2.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
             "lib/arm64-v8a/libmain.so",
             "assets/bin/Data/Managed/Metadata/global-metadata.dat"]
zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names
assert new_dex not in names, f"orig 已有 {new_dex}"
for n in untouched:
    assert n in names, f"orig apk missing {n}"
def is_sig(name):
    if name in SIG_NAMES: return True
    if not name.startswith("META-INF/"): return False
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))
kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in extras:
        continue
    if name == "AndroidManifest.xml":
        data = mf
    elif name == "classes.dex":
        data = patched1      # A 轨：LoginBlock SSO → SDK web 登录
    else:
        data = zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else (zipfile.ZIP_DEFLATED if name == "AndroidManifest.xml" else item.compress_type)
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data); n += 1
info = zipfile.ZipInfo(new_dex); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
zout.writestr(info, cnew); n += 1
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
    zout.writestr(info, data); n += 1
zin.close(); zout.close()
zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
out_names = set(zout2.namelist())
for name in kept_meta: assert name in out_names, f"lost {name}"
assert new_dex in out_names
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
assert not any(x.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for x in out_names)
assert "META-INF/MANIFEST.MF" not in out_names
d1 = zout2.read("classes.dex")
assert d1 == patched1, "classes.dex 未按补丁版写入"
assert b"bin/mt/signature/KillerApplication193" in cnew, "classes3.dex 缺 killer（B 轨未注入）"
print(f"  merged ok; entries≈{n}; {new_dex}={len(cnew)}(含 killer); classes.dex=patched({len(d1)}); untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

echo "[5/6] 闭环校验"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
[[ "$IN_SO_MD5" == "$SRC_SO_MD5" ]] || { echo "ERROR: so md5 mismatch" >&2; exit 1; }
IN_DEX_MD5="$(unzip -p "$UNSIGNED" "$NEW_DEX" | md5 -q)"
[[ "$IN_DEX_MD5" == "$(md5 -q "$WORK/$NEW_DEX")" ]] || { echo "ERROR: $NEW_DEX md5 mismatch" >&2; exit 1; }
echo "  so md5=$IN_SO_MD5  $NEW_DEX md5=$IN_DEX_MD5"

echo "[6/6] 完成（未签名）: $UNSIGNED"
ls -l "$UNSIGNED"
