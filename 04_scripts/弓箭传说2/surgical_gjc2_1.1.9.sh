#!/usr/bin/env bash
# 弓箭传说2 1.1.9 (com.xq.archeroii.ua) TapTap — 星光外科组装
#
# 官包零改动：classes.dex/classes2.dex / resources.arsc / assets/** / lib/*.so（除新增）字节不变。
# 改动：
#   - AndroidManifest：Application → com.starry.magic.StarryApp（其 smali 已改 .super 为 HabbyApplication）
#     + appComponentFactory → com.starry.magic.sign.SignSpoofAppComponentFactory（冷启动即回灌官方签名）
#     + StarryInitProvider + MenuService + FOREGROUND_SERVICE 权限
#   - 改 classes3.dex：TapSignKit.getSign 直接返回官方签名 MD5（b298339c…）
#   - 新增 classes4.dex（星光 smali：StarryApp / MenuService / SignSpoof / TapLoginForceWeb / OTA）
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
#
# 产物：08_output/弓箭传说2/弓箭传说2_1.1.9_surgical-unsigned.apk（未签名）
#       签名由 04_scripts/弓箭传说2/gjc2_sign.sh（pipeline sign 阶段）负责
#       07_patched/弓箭传说2/surgical/{AndroidManifest.xml,classes4.dex}（留证 / pre_install_gate 门禁用）
#
# 用法: bash 04_scripts/弓箭传说2/surgical_gjc2_1.1.9.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="弓箭传说2"
VERSION="1.1.9"
PKG="com.xq.archeroii.ua"
ORIG_APK="${GJC2_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_taptap_official_from_device.apk}"
ORIG_MD5="${GJC2_BASE_MD5:-ad0d6735f97ff702908b31f64faa54c9}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-gnu/libc++_shared.so"
[[ -f "$NDK_CXX" ]] || NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
AUTHORITY="$PKG.starryinit"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
# Habby 官方证书 MD5（TapSignKit.getSign 返回的口径）
ORIG_CERT_MD5="b298339c438d6645a4b649b984360c26"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN" >&2; exit 1; }
SO_PREBUILT="${GJC2_SO:-$STARRY_SRC/libs_gjc2/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so ${SO_PREBUILT}（先跑 gjc2_build_so.sh）" >&2; exit 1; }
echo "[check] modern so = $SO_PREBUILT ($(md5 -q "$SO_PREBUILT"))"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 ${ACTUAL_MD5}（期望 ${ORIG_MD5}）" >&2; exit 1; }

LOCK_DIR="/tmp/gjc2_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/gjc2_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle assembleRelease (starry → smali + certs)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] apktool d starry → com/starry smali → classes4.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for need in \
  "com/starry/magic/StarryApp.smali" \
  "com/starry/magic/StarryInitProvider.smali" \
  "com/starry/magic/menu/MenuService.smali" \
  "com/starry/magic/sign/SignSpoof.smali" \
  "com/starry/magic/sign/SignSpoofAppComponentFactory.smali" \
  "com/starry/magic/sign/TapLoginForceWeb.smali"; do
  [[ -f "$SMALI_ROOT/$need" ]] || { echo "ERROR: $need missing（Java 桥没编进 starry app）" >&2; exit 1; }
done

# StarryApp 继承 HabbyApplication（保留官方 SDK 初始化链）
python3 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/habby/sdkhubv2/HabbyApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/habby/sdkhubv2/HabbyApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/habby/sdkhubv2/HabbyApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/habby/sdkhubv2/HabbyApplication;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if "Lcom/habby/sdkhubv2/HabbyApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali 未指向 HabbyApplication")
# 只校验「继承与 super 调用」已换到 HabbyApplication；StarryApp 里对
# Landroid/app/Application; 的静态调用（如 getProcessName）属正常用法，不拦。
if ".super Lcom/habby/sdkhubv2/HabbyApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali .super 未改为 HabbyApplication")
for bad in (
    "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
    "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
    "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
):
    if bad in t:
        raise SystemExit(f"ERROR: StarryApp.smali 仍残留旧 super 调用: {bad}")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp → HabbyApplication ok ({changed} edits)")
PY

"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
ls -la "$WORK/classes4.dex"

# dex↔Java JNI 声明交叉校验
MB_JAVA="$STARRY_SRC/app/src/main/java/com/starry/magic/bridge/ModBridge.java"
[[ -f "$MB_JAVA" ]] || { echo "ERROR: missing $MB_JAVA" >&2; exit 1; }
NATIVE_TOTAL=0
NATIVE_MISS=0
for m in $(LC_ALL=C grep -oE '\bnative[A-Za-z0-9_]+' "$MB_JAVA" | sort -u); do
  NATIVE_TOTAL=$((NATIVE_TOTAL + 1))
  LC_ALL=C grep -aqF "$m" "$WORK/classes4.dex" || {
    echo "ERROR: classes4.dex 缺 native 声明 $m" >&2
    NATIVE_MISS=$((NATIVE_MISS + 1))
  }
done
[[ "$NATIVE_MISS" -eq 0 ]] \
  || { echo "ERROR: classes4.dex 与 ModBridge.java 声明不一致（$NATIVE_MISS/$NATIVE_TOTAL 缺失）" >&2; exit 1; }
echo "  dex↔ModBridge native 声明校验通过（$NATIVE_TOTAL 项）"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"

echo "[3/6] patch classes3.dex: TapSignKit.getSign → 官方 MD5"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
python3 - "$WORK/c3smali" "$ORIG_CERT_MD5" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
md5 = sys.argv[2]
tap = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
if not tap.is_file():
    raise SystemExit(f"missing {tap}")

NEW = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .registers 2

    # starry_sign_bypass（Habby 官方证书 MD5，原包 TapSignKit.getSign 口径）
    const-string v0, "{md5}"

    return-object v0
.end method"""
ts = tap.read_text(encoding="utf-8")
ts2, n = re.subn(
    r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
    NEW,
    ts,
    count=1,
)
if n != 1:
    raise SystemExit("TapSignKit.getSign patch failed")
tap.write_text(ts2, encoding="utf-8")
print(f"  TapSignKit.getSign → {md5}")
PY
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"
ls -la "$WORK/classes3.dex"

# 留证：装机前门禁（pre_install_gate --dex）与复查都要用这个 dex
STAGE_DIR="$NIXIANG/07_patched/$GAME/surgical"
mkdir -p "$STAGE_DIR"
cp -f "$WORK/classes4.dex" "$STAGE_DIR/classes4.dex"
cp -f "$WORK/classes3.dex" "$STAGE_DIR/classes3.dex"
echo "  classes4.dex → $STAGE_DIR/classes4.dex ($(md5 -q "$STAGE_DIR/classes4.dex"))"
echo "  classes3.dex → $STAGE_DIR/classes3.dex ($(md5 -q "$STAGE_DIR/classes3.dex"))"

echo "[4/6] binary AndroidManifest（Application→StarryApp + factory/provider/service/权限）"
if [[ -n "${GJC2_BINARY_MANIFEST:-}" && -f "${GJC2_BINARY_MANIFEST:-}" ]]; then
  cp "$GJC2_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $GJC2_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once (apktool d -s --no-assets)"
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" "$AUTHORITY" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
pkg = sys.argv[2]
authority = sys.argv[3]
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

# Application → StarryApp（StarryApp.smali 已改 .super 为 HabbyApplication）
old_app = 'android:name="com.habby.sdkhubv2.HabbyApplication"'
new_app = 'android:name="com.starry.magic.StarryApp"'
if new_app not in t:
    if old_app not in t:
        raise SystemExit("expected HabbyApplication not found in manifest")
    t = t.replace(old_app, new_app, 1)

old_factory = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
new_factory = 'android:appComponentFactory="com.starry.magic.sign.SignSpoofAppComponentFactory"'
if new_factory not in t:
    if old_factory not in t:
        raise SystemExit("expected CoreComponentFactory not found")
    t = t.replace(old_factory, new_factory, 1)

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % authority
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
print("  text Manifest patched")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4b/6] binary Manifest 回读校验"
if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  for need in \
    "com.starry.magic.StarryApp" \
    "com.starry.magic.StarryInitProvider" \
    "com.starry.magic.menu.MenuService" \
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE" \
    "com.starry.magic.sign.SignSpoofAppComponentFactory"; do
    grep -q "$need" "$WORK/tree_new.txt" \
      || { echo "ERROR: 新 manifest 缺 $need" >&2; exit 1; }
  done
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  python3 - "$WORK/o.norm" "$WORK/n.norm" <<'PY'
import sys
from collections import Counter
from pathlib import Path


def canon(path):
    stack, out = [], []
    for ln in Path(path).read_text(encoding='utf-8').splitlines():
        if not ln.strip():
            continue
        ind = len(ln) - len(ln.lstrip(' '))
        core = ln.strip()
        if core.startswith('N:'):
            continue
        if core.startswith('E:'):
            lv = (ind + 2) // 4
            stack = stack[:lv - 1]
            stack.append(core[2:].strip().split(' ')[0])
            out.append('/'.join(stack))
        elif core.startswith('A:') and stack:
            out.append('/'.join(stack[:ind // 4]) + '|' + core)
    return Counter(out)


o, n = canon(sys.argv[1]), canon(sys.argv[2])
dropped = o - n
added = sum((n - o).values())
def allowed_lost(k):
    # 旧 factory 与旧 Application 是有意替换掉的；其余任何丢失都算重建事故
    return ('androidx.core.app.CoreComponentFactory' in k) \
        or ('com.habby.sdkhubv2.HabbyApplication' in k)

unexpected = {k: v for k, v in dropped.items() if not allowed_lost(k)}
if unexpected:
    for k, v in list(unexpected.items())[:10]:
        print(f'  LOST x{v}: {k}', file=sys.stderr)
    raise SystemExit(f'manifest 重建丢失官方节点（{sum(unexpected.values())} 项）')
if not (12 <= added <= 30):
    raise SystemExit(f'manifest 新增节点数异常（{added} 项）')
print(f'  manifest ok (新增 {added} 项 / 丢弃 {sum(dropped.values())} 项=旧 factory+Application，顺序无关)')
PY
else
  echo "  WARN: aapt2 不在 ${AAPT2}，跳过 manifest 回读校验" >&2
fi

echo "[5/6] surgical zip merge（原 dex/arsc/assets/lib 字节不变）"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "$NDK_CXX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx_path = Path(sys.argv[5])
cxx = cxx_path.read_bytes() if cxx_path.is_file() else b""
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes3 = (work / 'classes3.dex').read_bytes()
classes4 = (work / 'classes4.dex').read_bytes()

replace = {'AndroidManifest.xml': manifest, 'classes3.dex': classes3}
extras = {
    'classes4.dex': classes4,
    'lib/arm64-v8a/libstarry_magic.so': so,
}
if cxx:
    extras['lib/arm64-v8a/libc++_shared.so'] = cxx

# 官包游戏本体硬校验基线（sha256，来自 02_apks 原包）
GUARD = {
    'classes.dex', 'classes2.dex', 'resources.arsc',
    'lib/arm64-v8a/libil2cpp.so', 'lib/arm64-v8a/libtapsdkcore.so',
    'lib/arm64-v8a/libunity.so',
    'assets/bin/Data/Managed/Metadata/global-metadata.dat',
}

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name.startswith('META-INF/') and (name == 'META-INF/MANIFEST.MF'
                                             or name.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if name in extras:
            continue
        data = replace.get(name)
        if data is None:
            data = zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == 'resources.arsc' else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
    for name, data in extras.items():
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_STORED if name.startswith('lib/') and name.endswith('.so') else zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'r') as zout:
    out_names = set(zout.namelist())
    changed = 0
    for n in original_names:
        if n.startswith('META-INF/') and (n == 'META-INF/MANIFEST.MF'
                                          or n.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
        changed += 1
    for n in GUARD:
        if n not in out_names:
            raise SystemExit(f'{n} missing from output')
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'protected entry changed: {n}')
    if 'classes4.dex' not in out_names:
        raise SystemExit('classes4.dex missing')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print(f'surgical zip verified: {changed} 个原始条目字节不变 + classes4.dex + starry so')

# 官包游戏 dex / so 指纹硬校验（与原始 02_apks 基线逐项对照）
import subprocess
base = Path('/Users/aliang/Documents/nixiang/02_apks/弓箭传说2/弓箭传说2_1.1.9_taptap_official_from_device.apk')
def entry_md5(apk, name):
    with zipfile.ZipFile(apk) as z:
        return hashlib.md5(z.read(name)).hexdigest()
for n in ('lib/arm64-v8a/libil2cpp.so', 'assets/bin/Data/Managed/Metadata/global-metadata.dat'):
    m = entry_md5(dst, n)
    print(f'  {n} md5={m}')

# 2GB 以上包的 zip64 → 经典记录归一化（照搬已实证方案）
def normalize_zip(path):
    import struct
    f = open(path, "r+b")
    f.seek(0, 2); size = f.tell()
    f.seek(max(0, size - 65536)); tail = f.read()
    e = tail.rfind(b"PK\x05\x06")
    assert e >= 0, "EOCD not found"
    _, _, _, _, n2, cdsize, cdoff, clen = struct.unpack("<IHHHHIIH", tail[e:e+22])
    assert clen == 0, "unexpected EOCD comment"
    if cdoff == 0xFFFFFFFF or cdsize == 0xFFFFFFFF or n2 == 0xFFFF:
        e64 = e - 76
        assert e64 >= 0 and tail[e64:e64+4] == b"PK\x06\x06", "expected EOCD64 before EOCD"
        count64, = struct.unpack("<Q", tail[e64+32:e64+40])
        cdsize64, cdoff64 = struct.unpack("<QQ", tail[e64+40:e64+56])
        n2, cdsize, cdoff = count64, cdsize64, cdoff64
    f.seek(cdoff); cd = f.read(cdsize)
    out = bytearray(); pos = fixed = 0
    while pos < len(cd):
        assert cd[pos:pos+4] == b"PK\x01\x02", f"bad CD sig @{pos}"
        nlen, elen, clen2 = struct.unpack_from("<HHH", cd, pos+28)
        hoff = struct.unpack_from("<I", cd, pos+42)[0]
        vneed = struct.unpack_from("<H", cd, pos+6)[0]
        total = 46 + nlen + elen + clen2
        rec = bytearray(cd[pos:pos+total])
        if hoff == 0xFFFFFFFF:
            name = cd[pos+46:pos+46+nlen]
            extra = cd[pos+46+nlen:pos+46+nlen+elen]
            comment = cd[pos+46+nlen+elen:pos+total]
            real = None; blocks = []; q = 0
            while q + 4 <= len(extra):
                bid, bsz = struct.unpack_from("<HH", extra, q)
                payload = extra[q+4:q+4+bsz]
                if bid == 1:
                    real = payload
                else:
                    blocks.append((bid, payload))
                q += 4 + bsz
            assert real is not None and len(real) == 8, "unexpected zip64 payload"
            real_off = struct.unpack("<Q", real)[0]
            assert real_off < 0xFFFFFFFF, "entry offset exceeds classic uint32"
            f.seek(real_off + 4); lver = struct.unpack("<H", f.read(2))[0]
            new_extra = b"".join(struct.pack("<HH", bid, len(pl)) + pl for bid, pl in blocks)
            rec = bytearray(cd[pos:pos+46])
            struct.pack_into("<H", rec, 6, lver)
            struct.pack_into("<H", rec, 30, len(new_extra))
            struct.pack_into("<I", rec, 42, real_off)
            out += rec + name + new_extra + comment
            fixed += 1
        elif vneed == 45:
            f.seek(hoff + 4); lver = struct.unpack("<H", f.read(2))[0]
            struct.pack_into("<H", rec, 6, lver)
            out += rec
            fixed += 1
        else:
            out += rec
        pos += total
    new_cdsize = len(out)
    f.seek(cdoff); f.write(out)
    f.seek(cdoff + new_cdsize)
    f.write(struct.pack("<IHHHHIIH", 0x06054b50, 0, 0, n2, n2, new_cdsize, cdoff, 0))
    f.truncate()
    f.close()
    print(f'  normalized: zip64 extras stripped/vneed fixed={fixed}, classic EOCD ok')


normalize_zip(str(dst))
PY

echo "[6/6] unsigned output → $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_size=$(stat -f '%z' "$UNSIGNED")"
