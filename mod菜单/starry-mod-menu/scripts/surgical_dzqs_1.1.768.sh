#!/usr/bin/env bash
# 独奏骑士 1.1.768 — 外科组装（官方 resources.arsc 字节不变）
#
# 产物：08_output/独奏骑士/独奏骑士_1.1.768_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_dzqs_1.1.768.sh
#   STARRY_INSTALL=1 bash scripts/surgical_dzqs_1.1.768.sh   # 仅覆盖安装，禁止卸载
#   STARRY_INSTALL=1 STARRY_REINSTALL=1 bash scripts/surgical_dzqs_1.1.768.sh
#       # 仅在用户明确授权后：完整卸载（不保留数据）→ 安装星光包
#
# 依赖：Android.mk 已启用 STARRY_GAME_DZQS；JDK 17；官方底包 ~551MB
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
ORIG_APK="${DZQS_ORIG_APK:-$NIXIANG/09_test/独奏骑士/base_1.1.768/base.apk}"
OUT="$NIXIANG/08_output/独奏骑士"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
ORIG_CERT_MD5="6870e083bf4136acf4d0d4b132d5dccd"
# 1.1.768 官方 crcCode：classes.dex CRC 和 + arm64 原生库内容 CRC 的 MD5。
OFFICIAL_CRC_CODE="34259c1947fde1c2e7fe5f3373a1054d"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.Shimmer.SoloKnight.cn"
SIGNED="$OUT/独奏骑士_1.1.768_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/独奏骑士_1.1.768_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/独奏骑士/surgical_1.1.768/AndroidManifest.xml"
SERIAL="${STARRY_SERIAL:-f9a116a2}"

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
grep -q 'STARRY_GAME_DZQS' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_DZQS"; exit 1; }
echo "[check] Android.mk target=dzqs"

LOCK_DIR="/tmp/dzqs_surgical_1768.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/dzqs_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] ndk-build -> jniLibs (Gradle does not build native)"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  STARRY_GAME_DZQS=1 "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q 'dzqs' "$SO_BUILT" || ! grep -a -q '\[Dzqs\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 DZQS（可能仍是上一款游戏）。strings 检查失败。"
  strings "$SO_BUILT" | grep -E 'dwct|tlgd|dzqs|gjc2|\[Dwct\]|\[Tlgd\]|\[Dzqs\]|\[Gjc2\]' | sort -u | head -20
  exit 1
fi
if grep -a -q '\[Tlgd\]' "$SO_BUILT" || grep -a -q '\[Dwct\]' "$SO_BUILT"; then
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
echo "  so -> jniLibs ok (dzqs verified)"

echo "[1b/6] gradle ${GRADLE_TASK} (pack refreshed jniLibs so)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Dzqs\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 DZQS —— 菜单会错成上一款游戏"
  strings "$WORK/check.so" | grep -E '\[Dwct\]|\[Tlgd\]|\[Dzqs\]|dwct|tlgd|dzqs' | sort -u | head -20
  exit 1
fi
echo "  gradle APK so verified [Dzqs]"

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes2.dex (StarryApp → SGGlobalApplication)"
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
    (".super Landroid/app/Application;", ".super Lcom/sgsdk/core/SGGlobalApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/sgsdk/core/SGGlobalApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/sgsdk/core/SGGlobalApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/sgsdk/core/SGGlobalApplication;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if "Lcom/sgsdk/core/SGGlobalApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing SGGlobalApplication")
# Keep intentional calls/type references such as Application.getProcessName() and
# registerActivityLifecycleCallbacks(); only superclass construction/dispatch must change.
bad = [
    marker for marker in (
        ".super Landroid/app/Application;",
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
    ) if marker in t
]
if bad:
    raise SystemExit("ERROR: StarryApp.smali still has Application super dispatch: " + "; ".join(bad))
p.write_text(t, encoding="utf-8")
print(f"  StarryApp → SGGlobalApplication ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$SMALI_ROOT"
ls -la "$WORK/classes2.dex"

echo "[3b/6] patch official classes.dex: FileCheck crcCode + TapSign + WebLogin"
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }
unzip -p "$ORIG_APK" classes.dex > "$WORK/classes_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c1smali" "$WORK/classes_orig.dex"
python3.11 - "$WORK/c1smali" "$ORIG_CERT_MD5" "$OFFICIAL_CRC_CODE" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
md5, crc = sys.argv[2], sys.argv[3]

# 1) FileCheck.findLibrary → 固定官方 crcCode（避免 classes3 / libstarry 改变校验）
fc = root / "com/gm/util/FileCheck.smali"
if not fc.exists():
    raise SystemExit("FileCheck.smali missing")
ft = fc.read_text(encoding="utf-8")
new_fl = f""".method public static findLibrary(Landroid/content/Context;Ljava/lang/String;)Ljava/lang/String;
    .locals 1

    # starry: freeze official ForceUpdate crcCode
    const-string v0, "{crc}"

    return-object v0
.end method"""
ft2, n = re.subn(
    r"\.method public static findLibrary\(Landroid/content/Context;Ljava/lang/String;\)Ljava/lang/String;[\s\S]*?\.end method",
    new_fl, ft, count=1,
)
if n != 1:
    raise SystemExit("FileCheck.findLibrary patch failed")
fc.write_text(ft2, encoding="utf-8")
print(f"  FileCheck.findLibrary → {crc}")

# 2) TapSignKit.getSign → 官方 cert MD5
tap = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
if not tap.exists():
    raise SystemExit("TapSignKit.smali missing")
ts = tap.read_text(encoding="utf-8")
TAP_GETSIGN = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "{md5}"

    return-object v0
.end method"""
ts2, n = re.subn(
    r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
    TAP_GETSIGN, ts, count=1,
)
if n != 1:
    raise SystemExit("TapSignKit.getSign patch failed")
tap.write_text(ts2, encoding="utf-8")
print(f"  TapSignKit.getSign → {md5}")

# 3) ClientLoginUtils.isClientLogin → false（强制网页登录）
clu = root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
if not clu.exists():
    raise SystemExit("ClientLoginUtils.smali missing")
cs = clu.read_text(encoding="utf-8")
new_icl = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
cs2, n = re.subn(
    r"\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z[\s\S]*?\.end method",
    new_icl, cs, count=1,
)
if n != 1:
    raise SystemExit("ClientLoginUtils.isClientLogin patch failed")
clu.write_text(cs2, encoding="utf-8")
print("  ClientLoginUtils.isClientLogin → false")
PY
"$SMALI_BIN" assemble -o "$WORK/classes.dex" "$WORK/c1smali"
ls -la "$WORK/classes.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/6] binary AndroidManifest (mini apktool，不对大包 apktool b)"
if [[ -n "${DZQS_BINARY_MANIFEST:-}" && -f "$DZQS_BINARY_MANIFEST" ]]; then
  cp "$DZQS_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
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
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
t = t.replace(
    'android:name="com.sgsdk.core.SGGlobalApplication"',
    'android:name="com.starry.magic.StarryApp"',
)
if 'android:name="com.starry.magic.StarryApp"' not in t:
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
print("  text Manifest patched")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/6] zip surgical merge (keep official resources.arsc；替换 patched classes.dex)"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c1 = (work / "classes.dex").read_bytes()
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

def is_protected(name: str) -> bool:
    # classes.dex 已打 FileCheck/Tap 补丁，不再与原包比字节
    return name == "resources.arsc"

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name in ("classes.dex", "classes2.dex"):
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
    n += 1

for name, data in (("classes.dex", c1), ("classes2.dex", c2)):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in zin2.namelist():
    if is_protected(name):
        assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "classes.dex" in zout2.namelist() and "classes2.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
print(f"  merged ok; entries≈{n}; classes={len(c1)} classes2={len(c2)}")
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
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/dzqs_surgical.apk"
  adb -s "$SERIAL" push "$SIGNED" "$REMOTE"
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  if [[ "${STARRY_REINSTALL:-0}" == "1" ]]; then
    echo "[install] explicit reinstall: pm uninstall --user 0 $PKG (data will be removed)"
    if ! adb -s "$SERIAL" shell "su -c 'pm uninstall --user 0 \"$PKG\"'"; then
      echo "ERROR: 卸载失败；未继续安装，请检查设备与用户范围。"
      adb -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true
      exit 2
    fi
    if [[ -n "$(adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r')" ]]; then
      echo "ERROR: $PKG 仍在设备上，停止安装以免误覆盖。"
      adb -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true
      exit 2
    fi
    echo "[install] uninstall ok; package data removed"
  fi
  if adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  else
    echo "ERROR: 星光包安装失败；官方底包仍保留在 09_test/。"
    adb -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true
    exit 2
  fi
  adb -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true
  bash "$NIXIANG/04_scripts/_common/ensure_starry_device_id_seed.sh" || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Dzqs:V MenuService:V StarrySignSpoof:V"
fi

echo "DONE: $SIGNED"
