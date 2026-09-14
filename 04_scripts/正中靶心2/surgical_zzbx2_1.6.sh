#!/usr/bin/env bash
# 正中靶心2 1.6 — 星空外科组装。
# 当前按用户指示以真机斌哥改包为组装底包；只替换 Manifest、追加 classes9.dex、
# 注入 libstarry_magic.so，并在明文 assets/main.js 末尾追加星光启动补丁。
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
ORIG_APK="${ZZBX2_BASE_APK:-$NIXIANG/02_apks/正中靶心2/正中靶心2_1.6_binge_mod.apk}"
ORIG_MD5="${ZZBX2_BASE_MD5:-7b99987150a68d5ed84fa202e4220b03}"
OUT_DIR="$NIXIANG/07_patched/正中靶心2"
UNSIGNED="$OUT_DIR/正中靶心2_1.6_starry-unsigned.apk"
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

LOCK="/tmp/zzbx2_surgical.lockdir"
if ! mkdir "$LOCK" 2>/dev/null; then echo "ERROR: lock held: $LOCK" >&2; exit 1; fi
echo $$ > "$LOCK/pid"
WORK="$(mktemp -d /tmp/zzbx2_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK"' EXIT
mkdir -p "$OUT_DIR" "$WORK"

echo "[1/6] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_zzbx2/' --exclude 'libs_zzbx2/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: starry apk missing" >&2; exit 1; }

echo "[2/6] collect starry smali -> classes9.dex"
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
"$SMALI_BIN" assemble -o "$WORK/classes9.dex" "$SMALI_ROOT"
ls -lh "$WORK/classes9.dex"

echo "[3/6] create Manifest patch"
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

echo "[4/6] patch assets/main.js"
python3 - "$ORIG_APK" "$PATCH_JS" "$WORK/main.js" <<'PY'
import sys, zipfile
from pathlib import Path
apk, patch_path, out = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
patch = patch_path.read_text(encoding="utf-8")
with zipfile.ZipFile(apk) as z:
    names = [n for n in z.namelist() if n == "assets/main.js"]
    if not names:
        raise SystemExit("assets/main.js not found")
    main = z.read("assets/main.js").decode("utf-8")
if "starry_zzbx2_v1" not in main:
    main = main.rstrip() + "\n\n" + patch + "\n"
    print("main.js patched")
else:
    print("main.js already contains starry patch")
out.write_text(main, encoding="utf-8")
PY

echo "[5/6] surgical zip merge"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "$NDK_CXX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx = Path(sys.argv[5]).read_bytes()
main_js = (work / 'main.js').read_bytes()
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes9 = (work / 'classes9.dex').read_bytes()

replace = {'AndroidManifest.xml': manifest, 'assets/main.js': main_js}
extras = {
    'classes9.dex': classes9,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name.startswith('META-INF/'):
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
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'r') as zout:
    out_names = set(zout.namelist())
    for n in original_names:
        if n.startswith('META-INF/') or n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
    if 'classes9.dex' not in out_names:
        raise SystemExit('classes9.dex missing')
    pkg_main = zout.read('assets/main.js')
    if b'starry_zzbx2_v1' not in pkg_main:
        raise SystemExit('main.js patch marker missing')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print('surgical zip verified: untouched entries + main.js + classes9 + starry so')
PY

echo "[6/6] unsigned output → $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_size=$(stat -f '%z' "$UNSIGNED")"
