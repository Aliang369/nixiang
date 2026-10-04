#!/usr/bin/env bash
# 伯吉的温馨厨房 1.0.1 — 外科组装（官方 classes.dex/classes4.dex/arsc 字节不变）
#
# 产物：08_output/伯吉的温馨厨房/伯吉的温馨厨房_1.0.1_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_bjdw_1.0.1.sh
#   STARRY_INSTALL=1 bash scripts/surgical_bjdw_1.0.1.sh
#
# 依赖：Android.mk 已启用 STARRY_GAME_BJDW；JDK 17；原包 02_apks/伯吉的温馨厨房/base.apk
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
ORIG_APK="$NIXIANG/02_apks/伯吉的温馨厨房/base.apk"
OUT="$NIXIANG/08_output/伯吉的温馨厨房"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.hnyg.bjdwxcf.topon"
SIGNED="$OUT/伯吉的温馨厨房_1.0.1_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/伯吉的温馨厨房_1.0.1_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/伯吉的温馨厨房/surgical_1.0.1/AndroidManifest.xml"

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
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_BJDW' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_BJDW"; exit 1; }
echo "[check] Android.mk target=bjdw"

LOCK_DIR="/tmp/bjdw_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/bjdw_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] ndk-build -> jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q 'bjdw' "$SO_BUILT" || ! grep -a -q '\[Bjdw\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 BJDW"
  strings "$SO_BUILT" | grep -E 'bjdw|\[Bjdw\]' | sort -u | head -20
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
echo "  so -> jniLibs ok (bjdw verified)"

echo "[1b/6] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Bjdw\]' "$WORK/check.so" || ! grep -a -q 'bjdw' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 BJDW"
  exit 1
fi
echo "  gradle APK so verified [Bjdw]"

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes5.dex（官方 classes4=kotlin 不动）"
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
    (".super Landroid/app/Application;", ".super Lcom/mar/sdk/MARApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/mar/sdk/MARApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/mar/sdk/MARApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/mar/sdk/MARApplication;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
        print(f"  patch: {a[:48]}…")
if "Lcom/mar/sdk/MARApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing MARApplication references")
if "Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali still references Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp.smali MARApplication wiring ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
ls -la "$WORK/classes5.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

ORIG_MD5="e28a5f390ab1a7590a032f05b3fde72a"
echo "[3b/6] patch classes3: TapSign MD5 + force Tap WebLogin"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
python3.11 - "$WORK/c3smali" "$ORIG_MD5" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
md5 = sys.argv[2]
tap = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
clu = root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
if not tap.is_file():
    raise SystemExit(f"missing {tap}")
if not clu.is_file():
    raise SystemExit(f"missing {clu}")

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
ts = tap.read_text(encoding="utf-8")
ts2, n = re.subn(
    r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
    TAP_GETSIGN,
    ts,
    count=1,
)
if n != 1:
    raise SystemExit("TapSignKit.getSign patch failed")
tap.write_text(ts2, encoding="utf-8")
print(f"  TapSignKit.getSign → {md5}")

s = clu.read_text(encoding="utf-8")
new_icl = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO（改签后必失败）
    const/4 v0, 0x0

    return v0
.end method"""
s2, n = re.subn(
    r"\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z[\s\S]*?\.end method",
    new_icl,
    s,
    count=1,
)
if n != 1:
    raise SystemExit("ClientLoginUtils.isClientLogin patch failed")
clu.write_text(s2, encoding="utf-8")
print("  ClientLoginUtils.isClientLogin → false (force WebLogin)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"
ls -la "$WORK/classes3.dex"

echo "[3c/6] patch classes2: MarMainActivity onResume/onUserLeaveHint → MenuService"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
python3.11 - "$WORK/c2smali" <<'PY'
from pathlib import Path
import re, sys
main = Path(sys.argv[1]) / "com/mar/sdk/MarMainActivity.smali"
t = main.read_text(encoding="utf-8")
if "notifyHostResumed" not in t:
    t2, n = re.subn(
        r"(\.method public onResume\(\)V[\s\S]*?"
        r"invoke-super \{p0\}, Lcom/unity3d/player/UnityPlayerActivity;->onResume\(\)V)\n\n    return-void",
        r"\1\n\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n    return-void",
        t,
        count=1,
    )
    if n != 1:
        raise SystemExit("MarMainActivity.onResume patch failed")
    t = t2
    print("  onResume → notifyHostResumed")
if "notifyHostLeaving" not in t:
    t += """
.method protected onUserLeaveHint()V
    .registers 1
    invoke-super {p0}, Lcom/unity3d/player/UnityPlayerActivity;->onUserLeaveHint()V
    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V
    return-void
.end method
"""
    print("  +onUserLeaveHint → notifyHostLeaving")
main.write_text(t)
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"
ls -la "$WORK/classes2.dex"

echo "[4/6] binary AndroidManifest"
if [[ -n "${BJDW_BINARY_MANIFEST:-}" && -f "$BJDW_BINARY_MANIFEST" ]]; then
  cp "$BJDW_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once (apktool d -s + b extract)"
  apktool d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
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
if 'android:name="com.starry.magic.StarryApp"' not in t:
    t = t.replace(
        'android:name="com.mar.sdk.MARApplication"',
        'android:name="com.starry.magic.StarryApp"',
        1,
    )
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

echo "[5/6] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c2 = (work / "classes2.dex").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
c5 = (work / "classes5.dex").read_bytes()
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
    # classes.dex + classes4.dex + arsc 保持原包字节；2/3 改过；5 新增
    return name in ("classes.dex", "classes4.dex")

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name in ("classes2.dex", "classes3.dex", "classes5.dex"):
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

for dex_name, dex_data in (("classes2.dex", c2), ("classes3.dex", c3), ("classes5.dex", c5)):
    info = zipfile.ZipInfo(dex_name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, dex_data)
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
    if name == "resources.arsc" or is_protected(name):
        assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "classes2.dex" in zout2.namelist()
assert "classes3.dex" in zout2.namelist()
assert "classes5.dex" in zout2.namelist()
assert b"SignSpoof" in zout2.read("classes5.dex")
assert b"notifyHostResumed" in zout2.read("classes2.dex")
print(f"  merged ok; entries≈{n}; classes2={len(c2)} classes3={len(c3)} classes5={len(c5)}")
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
  REMOTE="/data/local/tmp/bjdw_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  else
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" != "1" ]]; then
      echo "ERROR: 签名不一致无法覆盖安装。确认后重跑: STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1 $0"
      exit 1
    fi
    echo "[install] signature mismatch → uninstall + reinstall (STARRY_ALLOW_UNINSTALL=1)"
    adb shell "pm uninstall $PKG" 2>/dev/null || adb uninstall "$PKG" || true
    adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Bjdw:V MenuService:V StarrySignSpoof:V"
fi

echo "DONE: $SIGNED"
