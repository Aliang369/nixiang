#!/usr/bin/env bash
# 不一样西游 1.0.40 (com.wingjoy.toWest, vc622) — 外科组装（直装版，禁 apktool b 整包）
#
# 原包零改动：classes/classes2/classes3.dex / resources.arsc / lib/**.so / global-metadata.dat 字节不变。
# 改动只有四项：
#   1. AndroidManifest：保留官方 application(androidx.multidex.MultiDexApplication) 与
#      appComponentFactory(androidx.core.app.CoreComponentFactory)，只新增
#      StarryInitProvider + MenuService + FOREGROUND_SERVICE(-SPECIAL_USE) 两条权限
#   2. 新增 classes4.dex（星光 smali：StarryInitProvider / MenuService / 菜单 UI / auth）
#      —— 官方 3 个 dex 全部保持逐字节不变（classes3.dex 余量 45190，但零改动优先级更高）
#   3. 新增 lib/arm64-v8a/libstarry_magic.so（Bysxy 版）+ libc++_shared.so
#   4. 移除官方签名条目（含 pangle 的 PANGLEDY.SF/.RSA），保留其余 META-INF 条目
#
# 产物：08_output/不一样西游/不一样西游_1.4.135_surgical-unsigned.apk
#
# 用法:
#   bash mod菜单/starry-mod-menu/scripts/surgical_bysxy_1.0.40.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
GAME="不一样西游"
VERSION="1.0.40"
PKG="com.wingjoy.toWest"
ORIG_APK="$NIXIANG/02_apks/不一样西游_官方_com.wingjoy.toWest.apk"
ORIG_MD5="0f5053a9c551ec643ab30eb9d8b55576"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
NEW_DEX="classes4.dex"
# ---- TapTap 双轨制（2026-10-29 口径）----
# B 轨 签名/完整性＝运行时签名替换：MT KillerApplication193（已烘焙本包名 + 官方证书 DER）
# A 轨 登录方式＝改包法：把官方 classes2.dex 的 LoginClient.sendLoginRequest 改成直接走 web 兜底
KILLER_DEX="$NIXIANG/04_scripts/不一样西游/assets/byyxy_signature_killer.dex"
KILLER_MD5="f8b243e4d711c6765a521b3c55f71e69"
PATCHED_C2="${BYYXY_PATCHED_CLASSES2:-$NIXIANG/07_patched/不一样西游/classes2_patched.dex}"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing ${SMALI_BIN}（01_tools/smali）" >&2; exit 1; }
SO_PREBUILT="${BYYXX2_SO:-$STARRY_SRC/libs_bysxy/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing modern so ${SO_PREBUILT}（先跑 bysxy_build_so.sh）" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || { echo "ERROR: 原包指纹不符" >&2; exit 1; }
if LC_ALL=C grep -aq 'Dkzj\]\|Zzz\]\|Nhsy2\]\|Gjc2\]\|Byyxx2\]' "$SO_PREBUILT"; then
  echo "ERROR: so 残留上一款游戏标签" >&2; exit 1
fi
LC_ALL=C grep -aq 'Bysxy' "$SO_PREBUILT" || { echo "ERROR: so 缺 Bysxy 标签" >&2; exit 1; }

[[ -f "$KILLER_DEX" ]] || { echo "ERROR: missing killer dex ${KILLER_DEX}（先跑 build_killer_dex_byyxy.sh）" >&2; exit 1; }
[[ "$(md5 -q "$KILLER_DEX")" == "$KILLER_MD5" ]] || { echo "ERROR: killer dex md5 与记录不符" >&2; exit 1; }
[[ -f "$PATCHED_C2" ]] || { echo "ERROR: missing patched ${PATCHED_C2}（先跑 patch_taptap_login_byyxy.sh）" >&2; exit 1; }

LOCK_DIR="/tmp/bysxy_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/bysxy_surgical.XXXXXX)"
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
  || { echo "ERROR: StarryInitProvider.smali missing（Java 桥没编进 starry app）" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing" >&2; exit 1; }
# B 轨：killer smali 一起装进同一个新 dex（StarryAppComponentFactory 需能 Class.forName 到）
"$BAKSMALI_BIN" d -o "$WORK/killer_smali" "$KILLER_DEX" >/dev/null
rsync -a "$WORK/killer_smali/bin" "$SMALI_ROOT/"
[[ -f "$SMALI_ROOT/bin/mt/signature/KillerApplication193.smali" ]] \
  || { echo "ERROR: killer smali missing after unpack" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/sign/StarryAppComponentFactory.smali" ]] \
  || { echo "ERROR: StarryAppComponentFactory.smali missing" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

# A 轨默认关闭（2026-11-02 实测两次失败后改口径）：
#   ① 在 LoginClient.sendLoginRequest 里提前 return ⇒ 跳过 cloudGameHandler 的 bind 记账，
#      销毁时 unbindService 抛 IllegalArgumentException: Service not registered ⇒ 点开始游戏闪退；
#   ② 把 TapTapActivity 的 LoginBlock 换成 WebBlock ⇒ 绕过了 WebViewHandler.authorize 里
#      `setLoginCallback` + `startBlock` 这两步，页面拿不到回调 ⇒ 登录页永远转圈。
#   ⇒ 正解：SSO 走官方原路（B 轨 killer 已把签名换成官方证书，TapSignKit 在本进程内算出的
#      sign 就是官方值）；即便 SSO 失败，SDK 自带兜底 onLoginFailed → WebViewHandler.authorize
#      是**完整**的 web 路径（含 callback/startBlock），比手工拼装可靠。
#   ⇒ 2026-11-02 复测：只留 B 轨时 SSO 仍失败、且无任何 web 兜底被触发（用户：我怎么登录？）
#     ⇒ A 轨恢复为**默认开启**，但改走 SDK 完整 web 路径（LoginBlock → WebViewHandler.authorize）。
#     要临时关掉对比：BYYXY_TAPTAP_A=0
TAPTAP_A="${BYYXY_TAPTAP_A:-1}"
if [[ "$TAPTAP_A" == "1" ]]; then
  cp -f "$PATCHED_C2" "$WORK/classes2_patched.dex"
else
  echo "  [A轨] 关闭：classes2.dex 保持官方逐字节不变（登录走官方 SSO + SDK 自带 web 兜底）"
fi

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/6] AndroidManifest（保留官方 application/CoreComponentFactory）"
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
# B 轨挂载点：appComponentFactory → 星光入口（它在 instantiateApplication 里 mount killer，
# 早于真实 Application.attachBaseContext/onCreate 与全部 ContentProvider）
import re as _re
t, n_fac = _re.subn(r'android:appComponentFactory="[^"]*"',
                    'android:appComponentFactory="com.starry.magic.sign.StarryAppComponentFactory"', t)
if n_fac != 1:
    raise SystemExit("appComponentFactory 替换次数异常: %d" % n_fac)
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
  "$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null || {
    echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || { echo "ERROR: provider missing" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" || { echo "ERROR: FGS permission missing" >&2; exit 1; }
  grep -q "com.starry.magic.sign.StarryAppComponentFactory" "$WORK/tree_new.txt" || { echo "ERROR: appComponentFactory 未换成星光入口" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  # 官方侧只允许「appComponentFactory 值」这一行发生变化，其余节点零丢失
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep '^<' | grep -vc 'appComponentFactory' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 丢失/改动官方节点 $DIFF_D 行" >&2; exit 1; }
  grep -q 'appComponentFactory.*CoreComponentFactory' "$WORK/o.norm" || { echo "ERROR: 官方原 appComponentFactory 未在原始树中（基线错误）" >&2; exit 1; }
  echo "  manifest 闭环 ✓（唯一变化＝appComponentFactory → StarryAppComponentFactory）"
fi

echo "[4/6] zip surgical merge → $UNSIGNED"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
src, work, dst, new_dex = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
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
p2 = work / "classes2_patched.dex"
patched2 = p2.read_bytes() if p2.exists() else None
untouched = ["classes.dex", "classes3.dex",
             "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
             "lib/arm64-v8a/libmain.so",
             "assets/bin/Data/Managed/Metadata/global-metadata.dat"]
if patched2 is None:
    untouched.insert(1, "classes2.dex")   # A 轨关闭时官方 classes2.dex 同样必须逐字节不变
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
    elif name == "classes2.dex" and patched2 is not None:
        data = patched2      # A 轨：TapTapActivity → WebBlock（默认关闭）
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
d2 = zout2.read("classes2.dex")
if patched2 is not None:
    assert d2 == patched2, "classes2.dex 未按补丁版写入"
else:
    assert d2 == zin2.read("classes2.dex"), "classes2.dex 被意外改动"
assert b"bin/mt/signature/KillerApplication193" in cnew, "classes4.dex 缺 killer（B 轨未注入）"
print(f"  merged ok; entries≈{n}; {new_dex}={len(cnew)}(含 killer); classes2.dex=patched({len(d2)}); untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

echo "[5/6] 闭环校验"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
[[ "$IN_SO_MD5" == "$SRC_SO_MD5" ]] || { echo "ERROR: so md5 mismatch" >&2; exit 1; }
IN_SO_TAG="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -ac 'byyxy' || true)"
[[ "$IN_SO_TAG" -ge 1 ]] || { echo "ERROR: so tag byyxy missing" >&2; exit 1; }
IN_DEX_MD5="$(unzip -p "$UNSIGNED" "$NEW_DEX" | md5 -q)"
[[ "$IN_DEX_MD5" == "$(md5 -q "$WORK/$NEW_DEX")" ]] || { echo "ERROR: $NEW_DEX md5 mismatch" >&2; exit 1; }
echo "  so md5=$IN_SO_MD5  $NEW_DEX md5=$IN_DEX_MD5"

echo "[6/6] 完成（未签名）: $UNSIGNED"
ls -l "$UNSIGNED"
