#!/usr/bin/env bash
# 明日方舟：终末地 1.4.3 — 外科组装（禁止 apktool b 整包游戏）
#
# - 保留官方 classes.dex~classes6.dex / resources.arsc / ACE so 字节
# - 新增 classes7.dex = com/starry；StarryApp .super → U8Application
# - Manifest：Application → com.starry.magic.StarryApp + MenuService
# - 产物：08_output/终末地/终末地_1.4.3_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_endfield_1.4.3.sh
#   STARRY_TRACE=1 bash scripts/surgical_endfield_1.4.3.sh   # 带 StarryTrace
#   STARRY_INSTALL=1 bash scripts/surgical_endfield_1.4.3.sh # 覆盖安装
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
ORIG_APK="${ENDFIELD_BASE_APK:-$NIXIANG/02_apks/终末地/endfield_official_1.4.3.apk}"
OUT="$NIXIANG/08_output/终末地"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.hypergryph.endfield"
OFFICIAL_APP="Lcom/u8/sdk/U8Application;"
SIGNED="$OUT/终末地_1.4.3_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/终末地_1.4.3_surgical-debugSigned.apk"
[[ "${STARRY_TRACE:-0}" == "1" ]] && SIGNED="$OUT/终末地_1.4.3_surgical-trace-releaseSigned.apk"
MF_CACHE="$NIXIANG/07_patched/终末地/surgical_1.4.3/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_ENDFIELD' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_ENDFIELD"; exit 1; }
grep -E 'Games/EndfieldMod\.cpp' "$MK" \
  || { echo "ERROR: Android.mk must compile EndfieldMod.cpp"; exit 1; }
echo "[check] Android.mk target=endfield"

LOCK_DIR="/tmp/endfield_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/endfield_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] ndk-build -> jniLibs (STARRY_TRACE=${STARRY_TRACE:-0})"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
NDK_EXTRA=()
if [[ "${STARRY_TRACE:-0}" == "1" ]]; then
  NDK_EXTRA+=(STARRY_TRACE=1)
fi
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  "${NDK_EXTRA[@]}" -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Endfield\]' "$SO_BUILT" || ! grep -a -q 'mrfzzmd' "$SO_BUILT"; then
  echo "ERROR: so 不是 ENDFIELD"
  strings "$SO_BUILT" | grep -E 'Endfield|mrfzzmd|Wggls|\[Wggls\]|gjc2' | sort -u | head -20
  exit 1
fi
if grep -a -q '\[Wggls\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含 [Wggls]"; exit 1
fi
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Endfield]/mrfzzmd)"

echo "[1b/6] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q '\[Endfield\]' "$WORK/check.so" || { echo "ERROR: gradle APK so 不是 ENDFIELD"; exit 1; }

echo "[2/6] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] classes7.dex = com/starry (StarryApp → U8Application)"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/u8/sdk/U8Application;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/u8/sdk/U8Application;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/u8/sdk/U8Application;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/u8/sdk/U8Application;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if "Lcom/u8/sdk/U8Application;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing U8Application")
# Application 仍可能出现在 getProcessName 等，允许；但 .super 必须是 U8
if not t.splitlines()[0:30].__iter__():
    pass
supers = [ln for ln in t.splitlines() if ln.startswith(".super ")]
if not supers or supers[0].strip() != ".super Lcom/u8/sdk/U8Application;":
    raise SystemExit(f"ERROR: bad .super: {supers[:3]}")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp → U8Application ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes7.dex" "$SMALI_ROOT"
ls -la "$WORK/classes7.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/6] binary AndroidManifest (mini apktool，不对原包 apktool b)"
if [[ -n "${ENDFIELD_BINARY_MANIFEST:-}" && -f "$ENDFIELD_BINARY_MANIFEST" ]]; then
  cp "$ENDFIELD_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once via mini zip"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src)
zout = zipfile.ZipFile(dst, "w")
for name in keep:
    info = zin.getinfo(name)
    data = zin.read(name)
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, data)
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 替换官方 Application，勿叠加第二个 android:name
# 20260902 实测：终末地 ACE initialize 校验 Manifest Application 类名，
# 替换为 StarryApp 会导致 native 信号量毒丸挂起（黑屏 Splash）。
# 因此【不替换 Application】，改由 StarryInitProvider 自动初始化。
if 'com.starry.magic.StarryInitProvider' not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.hypergryph.endfield.starryinit"
            android:exported="false" />
'''
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
print("  text Manifest patched (StarryApp + MenuService)")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/6] zip surgical merge (keep official dex0-6 + arsc)"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c7 = (work / "classes7.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

def is_protected_dex(name: str) -> bool:
    if name == "classes.dex":
        return True
    if name.startswith("classes") and name.endswith(".dex"):
        mid = name[len("classes"):-len(".dex")]
        if mid.isdigit() and 2 <= int(mid) <= 6:
            return True
    return False

def is_protected(name: str) -> bool:
    return name == "resources.arsc" or is_protected_dex(name)

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
seen = set()
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name == "classes7.dex":
        continue
    if name == "AndroidManifest.xml":
        data = mf
    else:
        data = zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    seen.add(name)
    n += 1

info = zipfile.ZipInfo("classes7.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c7)

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

zin.close()
zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in zin2.namelist():
    if is_protected(name):
        h1 = hashlib.sha256(zin2.read(name)).hexdigest()
        h2 = hashlib.sha256(zout2.read(name)).hexdigest()
        assert h1 == h2, f"PROTECTED MUTATED: {name}"
print(f"  merged ok; entries≈{n}; classes7={len(c7)} bytes; protected sha256 match")
zin2.close(); zout2.close()
PY

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
# so 标签再验
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | strings | grep -E '\[Endfield\]|mrfzzmd' | head -5
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/endfield_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  if [[ "${STARRY_CLEAN_INSTALL:-0}" == "1" ]]; then
    echo "[install] CLEAN uninstall (STARRY_CLEAN_INSTALL=1)"
    adb shell "pm uninstall $PKG" 2>/dev/null || true
  else
    echo "[install] cover install -r -d (no uninstall)"
    adb shell am force-stop "$PKG" 2>/dev/null || true
  fi
  adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\" && rm -f \"$REMOTE\"'" \
    || adb install -r -d "$SIGNED"
  bash "$NIXIANG/04_scripts/_common/ensure_starry_device_id_seed.sh" || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V StarryTrace:V Endfield:V MenuService:V"
fi

echo "DONE: $SIGNED"
