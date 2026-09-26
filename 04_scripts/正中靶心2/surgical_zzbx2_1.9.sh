#!/usr/bin/env bash
# 正中靶心2 1.9 — 星空外科组装。
# 支持两种组装底包：
#   1. 官方 1.9 原包（本作唯一合法底包）；
#   2. 真机上一版星空 1.9（其官方 classes/assets/main.js 未变），
#      重建时替换旧 classes5.dex、旧 libstarry_magic.so 与旧 Manifest 外挂项。
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug"; exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
VER="${ZZBX2_VER:-1.9}"
ORIG_APK="${ZZBX2_BASE_APK:-$NIXIANG/02_apks/正中靶心2/正中靶心2_${VER}_official.apk}"
ORIG_MD5="${ZZBX2_BASE_MD5:-3b126e42a931d712ceef85b0c9b8ec05}"
OUT_DIR="$NIXIANG/07_patched/正中靶心2"
UNSIGNED="$OUT_DIR/正中靶心2_${VER}_starry-unsigned.apk"
PATCH_JS="$SCRIPT_DIR/patch_main.js"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.openew.monster"
SO_PREBUILT="${ZZBX2_SO:-$STARRY_SRC/libs_zzbx2/arm64-v8a/libstarry_magic.so}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK $ORIG_APK" >&2; exit 1; }
[[ -f "$PATCH_JS" ]] || { echo "ERROR: missing $PATCH_JS" >&2; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing" >&2; exit 1; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing starry so $SO_PREBUILT" >&2; exit 1; }
[[ -f "$NDK_CXX" ]] || { echo "ERROR: missing libc++_shared at $NDK_CXX" >&2; exit 1; }

ACTUAL_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: base APK fingerprint mismatch: $ACTUAL_MD5" >&2; exit 1; }

# 官方底包自检：不得已含星空外挂物（避免在改包上二次组装）
python3 - "$ORIG_APK" <<'PYCHK'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as z:
    names = set(z.namelist())
    bad = [n for n in ('classes5.dex', 'lib/arm64-v8a/libstarry_magic.so') if n in names]
if bad:
    raise SystemExit('ERROR: base APK is not official (already contains %s)' % bad)
print('  base APK is official (no starry extras)')
PYCHK

LOCK="/tmp/zzbx2_surgical.lockdir"
if ! mkdir "$LOCK" 2>/dev/null; then echo "ERROR: lock held: $LOCK" >&2; exit 1; fi
echo $$ > "$LOCK/pid"
WORK="$(mktemp -d /tmp/zzbx2_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK"' EXIT
mkdir -p "$OUT_DIR" "$WORK"

echo "[1/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_zzbx2/' --exclude 'libs_zzbx2/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: starry apk missing" >&2; exit 1; }

echo "[2/7] collect starry smali -> classes5.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] || { echo "ERROR: StarryApp missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/cocos/Zzbx2Hooks.smali" ]] || { echo "ERROR: Zzbx2Hooks missing from Gradle build" >&2; exit 1; }

# API 24-27 会忽略 manifest 的 appComponentFactory，必须再把入口改为
# StarryApp，并在 OpenewApplication.attachBaseContext 前安装 SignSpoof。
python3 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/openew/game/OpenewApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/openew/game/OpenewApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/openew/game/OpenewApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/openew/game/OpenewApplication;->onCreate()V",
    ),
]
changed = 0
for old, new in repls:
    if old in t:
        t = t.replace(old, new)
        changed += 1
if ".super Lcom/openew/game/OpenewApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali did not switch super to OpenewApplication")
for bad in (
    "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
    "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
    "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
):
    if bad in t:
        raise SystemExit(f"ERROR: StarryApp.smali still has old super call: {bad}")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp -> OpenewApplication ok ({changed} edits)")
PY

"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
ls -lh "$WORK/classes5.dex"

echo "[3/7] patch TapSignKit.getSign -> official Openew MD5"
TAP_PATCHED_DEX="$WORK/taptap_patched.dex"
TAP_DEX_NAME="$(python3 - "$ORIG_APK" "$WORK" "$BAKSMALI_BIN" "$SMALI_BIN" "$TAP_PATCHED_DEX" <<'PY'
from pathlib import Path
import re
import shutil
import subprocess
import sys
import zipfile

apk = Path(sys.argv[1])
work = Path(sys.argv[2])
baksmali = sys.argv[3]
smali = sys.argv[4]
output = Path(sys.argv[5])
cert_md5 = "b9c07db608080d40b04254d32ac3fc5c"
marker = b"Lcom/taptap/sdk/kit/internal/utils/TapSignKit;"

with zipfile.ZipFile(apk) as z:
    candidates = [
        name
        for name in z.namelist()
        if re.fullmatch(r"classes(?:\d+)?\.dex", name)
        and marker in z.read(name)
    ]
if len(candidates) != 1:
    raise SystemExit(f"expected exactly one TapSignKit dex, got {candidates}")

dex_name = candidates[0]
source_dex = work / f"original_{dex_name}"
with zipfile.ZipFile(apk) as z:
    source_dex.write_bytes(z.read(dex_name))

unpacked = work / "taptap_smali"
if unpacked.exists():
    shutil.rmtree(unpacked)
subprocess.run([baksmali, "d", "-o", str(unpacked), str(source_dex)], check=True)
target = unpacked / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
if not target.is_file():
    raise SystemExit(f"missing {target}")

text = target.read_text(encoding="utf-8")
pattern = re.compile(
    r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;"
    r".*?\.end method",
    re.S,
)
replacement = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1

    const-string v0, "{cert_md5}"

    return-object v0
.end method"""
text, count = pattern.subn(replacement, text, count=1)
if count != 1:
    raise SystemExit("TapSignKit.getSign(Context) method not found")
target.write_text(text, encoding="utf-8")

subprocess.run([smali, "assemble", "-o", str(output), str(unpacked)], check=True)
print(dex_name)
PY
)"
[[ -n "$TAP_DEX_NAME" && -f "$TAP_PATCHED_DEX" ]] \
  || { echo "ERROR: TapSignKit dex patch failed" >&2; exit 1; }
echo "  $TAP_DEX_NAME TapSignKit.getSign -> b9c07db608080d40b04254d32ac3fc5c"

echo "[4/7] create Manifest patch"
apktool d -f -s -o "$WORK/base_dec" "$ORIG_APK" >/dev/null
python3 - "$WORK/base_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no uses-permission node")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

old_app = 'android:name="com.openew.game.OpenewApplication"'
new_app = 'android:name="com.starry.magic.StarryApp"'
if new_app not in t:
    if old_app not in t:
        raise SystemExit("expected OpenewApplication not found in manifest")
    t = t.replace(old_app, new_app, 1)

old_factory = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
new_factory = 'android:appComponentFactory="com.starry.magic.sign.SignSpoofAppComponentFactory"'
if new_factory not in t:
    if old_factory not in t:
        raise SystemExit("expected CoreComponentFactory not found")
    t = t.replace(old_factory, new_factory, 1)

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

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.openew.monster.starryinit"
            android:exported="false"
            android:initOrder="100" />
'''
    t = t.replace("</application>", prov + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("manifest patched: provider + MenuService")
PY
rm -rf "$WORK/base_dec"/smali* 2>/dev/null || true
apktool b -f "$WORK/base_dec" -o "$WORK/manifest_only.apk" >/dev/null
unzip -p "$WORK/manifest_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

echo "[5/7] surgical zip merge（原 assets/main.js 保持字节不变）"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "$NDK_CXX" \
  "$TAP_DEX_NAME" "$TAP_PATCHED_DEX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx = Path(sys.argv[5]).read_bytes()
tap_dex_name = sys.argv[6]
tap_patched_dex = Path(sys.argv[7]).read_bytes()
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes5 = (work / 'classes5.dex').read_bytes()

replace = {
    'AndroidManifest.xml': manifest,
    tap_dex_name: tap_patched_dex,
}
extras = {
    'classes5.dex': classes5,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name == 'META-INF/MANIFEST.MF' or (name.startswith('META-INF/') and name.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
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
    for n in original_names:
        if n == 'META-INF/MANIFEST.MF' or (n.startswith('META-INF/') and n.endswith(('.SF', '.RSA', '.DSA', '.EC'))) or n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
    if 'classes5.dex' not in out_names:
        raise SystemExit('classes5.dex missing')
    if hashlib.sha256(zin.read('assets/main.js')).digest() != hashlib.sha256(zout.read('assets/main.js')).digest():
        raise SystemExit('assets/main.js changed unexpectedly')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print('surgical zip verified: original assets unchanged + classes5 + starry so')
PY

echo "[6/7] verify patched official dex"
unzip -p "$UNSIGNED" "$TAP_DEX_NAME" > "$WORK/verify_taptap.dex"
"$BAKSMALI_BIN" d -o "$WORK/verify_taptap_smali" "$WORK/verify_taptap.dex"
python3 - "$WORK/verify_taptap_smali/com/taptap/sdk/kit/internal/utils/TapSignKit.smali" \
  "b9c07db608080d40b04254d32ac3fc5c" <<'PY'
from pathlib import Path
import sys

text = Path(sys.argv[1]).read_text(encoding="utf-8")
md5 = sys.argv[2]
if f'const-string v0, "{md5}"' not in text:
    raise SystemExit("TapSignKit.getSign verification failed")
print("  packaged TapSignKit.getSign returns official MD5")
PY

echo "[7/7] unsigned output → $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_size=$(stat -f '%z' "$UNSIGNED")"
