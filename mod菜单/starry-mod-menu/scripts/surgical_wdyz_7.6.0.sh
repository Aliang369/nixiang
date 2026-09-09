#!/usr/bin/env bash
# 我的勇者 7.6.0 (com.rsg.wdyz, vc 202) — 外科 zip 组装（首包，官方原包无星光注入）
# - 注入方式：ContentProvider（StarryInitProvider），【不替换 Application】
#   （官方 Application=com.netease.nis.wrapper.MyApplication 易盾壳，替换会触发校验）
# - classes2.dex：com/starry smali（StarryApp/StarryInitProvider/MenuService…）
# - lib/arm64-v8a：libstarry_magic.so（WDYZ RVA 钩：秒杀 MonsterSettle ×1000 / 无敌 HeroSettle=0）+ libc++_shared.so
# - 官方 classes.dex / resources.arsc 原样保留（易盾壳主 dex 不动）
#
# 产物：08_output/我的勇者/我的勇者_7.6.0_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_wdyz_7.6.0.sh
#   STARRY_INSTALL=1 bash scripts/surgical_wdyz_7.6.0.sh       （装机）
#   STARRY_ALLOW_UNINSTALL=1 ...                               （签名不一致需卸载重装时显式给）
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
NIXIANG="/Users/aliang/Documents/nixiang"
ORIG_APK="$NIXIANG/02_apks/我的勇者/我的勇者_7.6.0_base.apk"
[[ -f "$ORIG_APK" ]] || ORIG_APK="/tmp/game_id/wdyz.apk"
OUT="$NIXIANG/08_output/我的勇者"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.rsg.wdyz"
OFFICIAL_APP="Lcom/netease/nis/wrapper/MyApplication;"
SIGNED="$OUT/我的勇者_7.6.0_surgical-${BUILD_TYPE}Signed.apk"
ALIAS_APK="$OUT/我的勇者_7.6.0_starry.apk"
MF_CACHE="$NIXIANG/07_patched/我的勇者/surgical_7.6.0/AndroidManifest.xml"

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
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_WDYZ' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_WDYZ"; exit 1; }
echo "[check] Android.mk target=wdyz"

LOCK_DIR="/tmp/wdyz_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wdyz_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/5] ndk-build -> jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  APP_ABI=arm64-v8a -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/main/jni/../../libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Wdyz\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 WDYZ（可能仍是上一款游戏）。strings 检查失败。"
  exit 1
fi
if grep -a -q '\[Cwjh\]' "$SO_BUILT" || grep -a -q '\[Wxsj\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款游戏标签"
  exit 1
fi
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok (wdyz verified)"

echo "[1b/5] gradle ${GRADLE_TASK}"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Wdyz\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 WDYZ"
  exit 1
fi
echo "  gradle APK so verified [Wdyz]"

echo "[2/5] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/5] 收集 com/starry smali → classes2.dex（StarryApp.super → 官方 MyApplication，Manifest 不走它）"
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
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" "$OFFICIAL_APP" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
app = sys.argv[2]
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", f".super {app}"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        f"invoke-direct {{p0}}, {app}-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        f"invoke-super {{p0, p1}}, {app}->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        f"invoke-super {{p0}}, {app}->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if app not in t:
    raise SystemExit(f"ERROR: StarryApp.smali missing {app}")
if ".super Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali still .super Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp.super -> {app} ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$SMALI_ROOT"
ls -la "$WORK/classes2.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/5] binary AndroidManifest（ContentProvider 注入；不替换 Application）"
if [[ -f "$MF_CACHE" ]]; then
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
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, zin.read(name))
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
if "FOREGROUND_SERVICE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 官方 Application=MyApplication（易盾 wrapper），【不替换】；改由 StarryInitProvider 自动初始化
if 'com.starry.magic.StarryInitProvider' not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.rsg.wdyz.starryinit"
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
print("  text Manifest patched (StarryInitProvider + MenuService)")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/5] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c2 = (work / "classes2.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
seen = set()
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name == "classes2.dex":
        continue
    if name in ("classes3.dex", "classes4.dex", "classes5.dex", "classes6.dex", "classes7.dex", "classes8.dex", "classes9.dex"):
        continue  # 官方只有 classes.dex；防御性跳过不存在项
    if name == "AndroidManifest.xml":
        data = mf
    else:
        data = zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)

info = zipfile.ZipInfo("classes2.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c2)

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_STORED if name.endswith(".so") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
assert hashlib.sha256(zin2.read("resources.arsc")).hexdigest() == hashlib.sha256(zout2.read("resources.arsc")).hexdigest()
assert hashlib.sha256(zin2.read("classes.dex")).hexdigest() == hashlib.sha256(zout2.read("classes.dex")).hexdigest()
assert "classes2.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
print("  merged ok; arsc & classes.dex 原样（hash 一致）")
zin2.close(); zout2.close()
PY

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
cp -f "$SIGNED" "$ALIAS_APK"
ls -lh "$SIGNED" "$ALIAS_APK"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/wdyz_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  else
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      echo "[install] signature mismatch → uninstall + reinstall (ALLOW_UNINSTALL=1)"
      adb shell "pm uninstall $PKG" 2>/dev/null || adb uninstall "$PKG" || true
      adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
    else
      echo "ERROR: 签名与真机原版不一致，覆盖安装失败。"
      echo "  需卸载原版才能装星光包。确认后设 STARRY_ALLOW_UNINSTALL=1 再跑。"
      exit 2
    fi
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Wdyz:V MenuService:V"
fi
echo "[done] $SIGNED"