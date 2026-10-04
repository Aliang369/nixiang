#!/usr/bin/env bash
# 地城狩猎 1.0.77409 - surgical assembly + signing.
#
# Original game DEX files and resources.arsc are preserved byte-for-byte.
# Added: classes5.dex (Starry framework + Conch bridge), libstarry_magic.so,
# assets/starry/dcsl_patch.js, and a Manifest with two startup providers.
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
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
ORIG_APK="$NIXIANG/02_apks/地城狩猎/地城狩猎_1.0.77409_from_device.apk"
OUT="$NIXIANG/08_output/地城狩猎"
UNSIGNED="$NIXIANG/07_patched/地城狩猎/dcsl_1.0.77409_unsigned.apk"
SIGNED="$OUT/地城狩猎_1.0.77409_无限技能-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.dcslad4.ad1b"
ORIG_MD5="3183a4a7d79764d43c6fa2001f4c835f"
SO_PREBUILT="${DCSL_SO:-$STARRY_SRC/libs_dcsl/arm64-v8a/libstarry_magic.so}"
MF_CACHE="$NIXIANG/07_patched/地城狩猎/surgical_1.0.77409/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: no zipalign in build-tools"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing $SO_PREBUILT (run build_so first)"; exit 1; }
[[ -f "$SIGNER" || -x "$BT/apksigner" ]] || { echo "ERROR: no signer"; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing $KS"; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] \
  || { echo "ERROR: original APK md5 mismatch"; exit 1; }
if [[ -e "$SIGNED" && "${STARRY_REPLACE:-0}" != "1" ]]; then
  echo "ERROR: output already exists: $SIGNED (set STARRY_REPLACE=1 to replace)"; exit 1
fi
if [[ "${STARRY_REPLACE:-0}" == "1" ]]; then
  rm -f "$SIGNED" "$UNSIGNED"
fi

LOCK_DIR="/tmp/dcsl_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/dcsl_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$(dirname "$UNSIGNED")" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_dcsl/' --exclude 'libs_dcsl/' --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/6] collect starry smali -> classes5.dex"
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
  || { echo "ERROR: StarryInitProvider.smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/conch/StarryConch.smali" ]] \
  || { echo "ERROR: StarryConch.smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/conch/DcslConchProvider.smali" ]] \
  || { echo "ERROR: DcslConchProvider.smali missing"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
ls -la "$WORK/classes5.dex"

echo "[3/6] stage new files"
mkdir -p "$WORK/lib/arm64-v8a" "$WORK/assets/starry"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
if [[ -f "$NDK_CXX" ]]; then
  cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
else
  echo "WARN: libc++_shared.so not found at $NDK_CXX"
fi
PATCH_SRC="$WORK/starry_dec/assets/starry/dcsl_patch.js"
[[ -f "$PATCH_SRC" ]] || PATCH_SRC="$STARRY_SRC/app/src/main/assets/starry/dcsl_patch.js"
[[ -f "$PATCH_SRC" ]] || { echo "ERROR: dcsl_patch.js missing"; exit 1; }
cp "$PATCH_SRC" "$WORK/assets/starry/dcsl_patch.js"
echo "  patch bytes = $(wc -c < "$WORK/assets/starry/dcsl_patch.js" | tr -d ' ')"

echo "[4/6] binary Manifest patch"
if [[ -n "${DCSL_BINARY_MANIFEST:-}" && -f "$DCSL_BINARY_MANIFEST" ]]; then
  cp "$DCSL_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
else
  apktool d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
pkg = "com.dcslad4.ad1b"

perms = []
for perm in (
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
):
    if perm not in t:
        perms.append('    <uses-permission android:name="%s"/>' % perm)
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
    print("  +permissions: %d" % len(perms))

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
    print("  +MenuService")

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:exported="false"
            android:initOrder="100" />
''' % pkg
    t = t.replace("</application>", prov + "\n    </application>", 1)
    print("  +StarryInitProvider")

if "com.starry.magic.conch.DcslConchProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.conch.DcslConchProvider"
            android:authorities="%s.dcslconch"
            android:exported="false"
            android:initOrder="101" />
''' % pkg
    t = t.replace("</application>", prov + "\n    </application>", 1)
    print("  +DcslConchProvider")

p.write_text(t, encoding="utf-8")
print("  text Manifest patched; original Application preserved")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached -> $MF_CACHE"
fi

echo "[5/6] surgical zip merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import hashlib
import shutil
import sys
import zipfile
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c5 = (work / "classes5.dex").read_bytes()

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p
base = work / "assets" / "starry"
for p in base.rglob("*"):
    if p.is_file():
        extras["assets/starry/" + str(p.relative_to(base)).replace("\\", "/")] = p

replace = {"AndroidManifest.xml": mf}
zin = zipfile.ZipFile(src, "r")
dex_names = sorted(n for n in zin.namelist()
                   if n.startswith("classes") and n.endswith(".dex"))
untouched = dex_names + ["resources.arsc"]

zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    info = zipfile.ZipInfo(name, item.date_time)
    info.external_attr = item.external_attr
    info.create_system = 3
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    if name in replace:
        zout.writestr(info, replace[name])
    else:
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)

info = zipfile.ZipInfo("classes5.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c5)

for name in sorted(extras):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, extras[name].read_bytes())
zin.close()
zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    assert hashlib.sha256(zin2.read(name)).hexdigest() == \
           hashlib.sha256(zout2.read(name)).hexdigest(), "changed: " + name
for need in ("classes5.dex", "lib/arm64-v8a/libstarry_magic.so",
             "assets/starry/dcsl_patch.js", "AndroidManifest.xml"):
    assert need in zout2.namelist(), "missing: " + need
print("  merged; original dex count=%d; classes5=%d B" % (len(dex_names), len(c5)))
zin2.close()
zout2.close()
PY
cp "$WORK/unsigned.apk" "$UNSIGNED"

echo "[6/6] sign and verify"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"

unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/extracted.so"
SO_BUILD_MD5="$(md5 -q "$SO_PREBUILT")"
SO_PKG_MD5="$(md5 -q "$WORK/extracted.so")"
[[ "$SO_BUILD_MD5" == "$SO_PKG_MD5" ]] \
  || { echo "ERROR: packaged so != build so ($SO_BUILD_MD5 vs $SO_PKG_MD5)"; exit 1; }
unzip -l "$SIGNED" | grep -E "classes5.dex|assets/starry/dcsl_patch.js|libstarry_magic.so" || true
"$BT/apksigner" verify --print-certs "$SIGNED" 2>/dev/null | head -5 || true

echo "OK: $SIGNED"
echo "md5 = $(md5 -q "$SIGNED")"
echo "unsigned = $UNSIGNED"
