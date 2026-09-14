#!/usr/bin/env bash
# 无悔华夏 4399：无壳外科重建 + 星光功能注入。
#
# 输入：
#   - 真机运行时 base.odex.apk（3.8.295 热更整包，含更新资源与同源 libil2cpp）
#   - Frida 精确 dump 的 3 个真实 InMemoryDexFile
#   - 构建好的 4399 专属 libstarry_magic.so
#
# 输出：
#   08_output/无悔华夏4399/无悔华夏4399_3.8.280_无壳七功能-releaseSigned.apk
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
OUT="$NIXIANG/08_output/无悔华夏4399"
ORIG_APK="${WHRHX4399_ORIG_APK:-$NIXIANG/06_dumped/无悔华夏4399/runtime_20260914/base.odex.apk}"
DEX_DIR="${WHRHX4399_DEX_DIR:-$NIXIANG/06_dumped/无悔华夏4399/runtime_20260914/inmemory_dex}"
SO_BUILT="${WHRHX4399_SO:-$STARRY_SRC/libs_whrhx4399/arm64-v8a/libstarry_magic.so}"
DEX1="$DEX_DIR/inmemory_L4_e1_8621488.dex"
DEX2="$DEX_DIR/inmemory_L4_e2_1902428.dex"
DEX3="$DEX_DIR/inmemory_L4_e3_1108844.dex"

SMALI_BIN="$NIXIANG/01_tools/smali/smali"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.yhhy.whrhx.m4399"
SIGNED="$OUT/无悔华夏4399_3.8.280_无壳七功能-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/无悔华夏4399_3.8.280_无壳七功能-debugSigned.apk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK: $ORIG_APK"; exit 1; }
[[ -f "$DEX1" && -f "$DEX2" && -f "$DEX3" ]] || { echo "ERROR: missing runtime DEX trio"; exit 1; }
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing 4399 so: $SO_BUILT"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing smali: $SMALI_BIN"; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing signing key: $KS"; exit 1; }
LC_ALL=C grep -a -q 'target=m4399' "$SO_BUILT" || { echo "ERROR: so target is not m4399"; exit 1; }

WORK="$(mktemp -d /tmp/whrhx4399_surgical.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/6] 构建星光 Java 模板 APK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
for dest in \
  "$WORK/starry/app/src/main/jniLibs/arm64-v8a" \
  "$WORK/starry/app/src/main/libs/arm64-v8a" \
  "$WORK/starry/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing starry APK"; exit 1; }

echo "[2/6] 组装 classes4.dex（StarryApp -> LBApplication）"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] || {
  echo "ERROR: StarryApp.smali missing"; exit 1;
}
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/excelliance/open/LBApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/excelliance/open/LBApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/excelliance/open/LBApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/excelliance/open/LBApplication;->onCreate()V",
    ),
]
for old, new in repls:
    t = t.replace(old, new)
if "Lcom/excelliance/open/LBApplication;" not in t:
    raise SystemExit("ERROR: StarryApp missing LBApplication super")
p.write_text(t, encoding="utf-8")
print("  StarryApp -> LBApplication")
PY
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
ls -lh "$WORK/classes4.dex"

echo "[3/6] 生成 patched AndroidManifest.xml"
python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
with zipfile.ZipFile(src) as zin, zipfile.ZipFile(dst, "w") as zout:
    for name in keep:
        item = zin.getinfo(name)
        data = zin.read(name)
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
        zout.writestr(info, data)
PY
apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "android.permission.SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "android.permission.FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    anchor = t.find("<application")
    if anchor < 0:
        raise SystemExit("ERROR: <application> missing in decoded Manifest")
    t = t[:anchor] + "\n".join(perms) + "\n" + t[anchor:]

old_apps = [
    'android:name="com.netease.nis.wrapper.MyApplication"',
    'android:name="com.excelliance.open.LBApplication"',
]
if 'android:name="com.starry.magic.StarryApp"' not in t:
    replaced = False
    for old in old_apps:
        if old in t:
            t = t.replace(old, 'android:name="com.starry.magic.StarryApp"', 1)
            replaced = True
            break
    if not replaced:
        t = t.replace("<application ", '<application android:name="com.starry.magic.StarryApp" ', 1)
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
print("  Manifest -> StarryApp / LBApplication + MenuService")
PY
apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
ls -lh "$WORK/AndroidManifest.xml"

echo "[4/6] 外科 zip 合并（保留官方 resources/so，替换真实 DEX）"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/libc++_shared.so"
python3.11 - "$ORIG_APK" "$WORK" "$DEX1" "$DEX2" "$DEX3" "$SO_BUILT" "$WORK/unsigned.apk" <<'PY'
import hashlib
import sys
import zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dex1 = Path(sys.argv[3]).read_bytes()
dex2 = Path(sys.argv[4]).read_bytes()
dex3 = Path(sys.argv[5]).read_bytes()
so = Path(sys.argv[6]).read_bytes()
dst = Path(sys.argv[7])
manifest = (work / "AndroidManifest.xml").read_bytes()
libcxx = work / "libc++_shared.so"

extras = {
    "classes.dex": dex1,
    "classes2.dex": dex2,
    "classes3.dex": dex3,
    "classes4.dex": (work / "classes4.dex").read_bytes(),
    "lib/arm64-v8a/libstarry_magic.so": so,
}
if libcxx.is_file():
    extras["lib/arm64-v8a/libc++_shared.so"] = libcxx.read_bytes()
certs = work / "starry_dec" / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

with zipfile.ZipFile(src, "r") as zin, zipfile.ZipFile(dst, "w") as zout:
    n = 0
    for item in zin.infolist():
        name = item.filename
        if name.startswith("META-INF/"):
            continue
        if name in extras:
            continue
        if name == "AndroidManifest.xml":
            data = manifest
        else:
            data = zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
        n += 1
    for name, data in sorted(extras.items()):
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)
        n += 1

with zipfile.ZipFile(src, "r") as a, zipfile.ZipFile(dst, "r") as b:
    for name in ("resources.arsc", "assets/bin/Data/Managed/Metadata/global-metadata.dat",
                 "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so"):
        if name in a.namelist() and name in b.namelist():
            assert hashlib.sha256(a.read(name)).hexdigest() == hashlib.sha256(b.read(name)).hexdigest(), name
    for name in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
                 "lib/arm64-v8a/libstarry_magic.so"):
        assert name in b.namelist(), name
    print(f"  merged entries≈{n}")
PY

echo "[5/6] zipalign"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi

echo "[6/6] 签名 -> $SIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --verbose --print-certs "$SIGNED" | sed -n '1,12p'
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
fi

ls -lh "$SIGNED"
shasum -a 256 "$SIGNED"
md5 -q "$SIGNED"
echo "DONE $SIGNED"
