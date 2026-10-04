#!/usr/bin/env bash
# 山海幻境 3.8 TapTap — 外科组装
# - StarryApp 继承 GameApplication（SignSpoof + so）
# - classes.dex：TapSignKit MD5 + ClientLoginUtils/Handler 强制网页登录
# - AppActivity：get_ret → ShhjHooks + MenuService 生命周期
# - 新增 classes4.dex（星光）+ libstarry_magic.so；保留 classes2/3 + arsc
#
# 产物：08_output/山海幻境/山海幻境_3.8_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_shhj_3.8.sh
#   STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1 bash scripts/surgical_shhj_3.8.sh
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
VERSION="${SHHJ_VERSION:-3.8}"
ORIG_APK="${SHHJ_ORIG_APK:-$NIXIANG/02_apks/山海幻境/山海幻境_${VERSION}_taptap_official.apk}"
OUT="$NIXIANG/08_output/山海幻境"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.chx.shhj"
SIGNED="$OUT/山海幻境_${VERSION}_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/山海幻境_${VERSION}_surgical-debugSigned.apk"
if [[ -n "${SHHJ_OUTPUT_NAME:-}" ]]; then
  SIGNED="$OUT/${SHHJ_OUTPUT_NAME}"
fi
ALIAS_APK="$OUT/山海幻境_${VERSION}_starry.apk"
MF_CACHE="${SHHJ_MF_CACHE:-$NIXIANG/07_patched/山海幻境/surgical_${VERSION}/AndroidManifest.xml}"
# TapTap 官方证书 MD5（TapSignKit.getSign）
ORIG_MD5="86d1959875555021642fcf3b0d9c161d"

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
if [[ -z "${SHHJ_PREBUILT_SO:-}" ]]; then
  grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_SHHJ' "$MK" \
    || { echo "ERROR: Android.mk must enable STARRY_GAME_SHHJ"; exit 1; }
  echo "[check] Android.mk target=shhj"
else
  echo "[check] prebuilt SHHJ so: $SHHJ_PREBUILT_SO"
fi

LOCK_DIR="/tmp/shhj_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/shhj_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] ndk-build -> jniLibs (Gradle does not build native; packs jniLibs so only)"
if [[ -n "${SHHJ_PREBUILT_SO:-}" ]]; then
  SO_BUILT="$SHHJ_PREBUILT_SO"
else
  NDK_BUILD="$NDK/ndk-build"
  [[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
  (cd "$STARRY_SRC/app/src/main/jni" && \
    "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
  SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
fi
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q 'shhj' "$SO_BUILT" || ! grep -a -q '\[Shhj\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 SHHJ（可能仍是上一款游戏）。strings 检查失败。"
  strings "$SO_BUILT" | grep -E 'shhj|gyws|\[Shhj\]|\[Gyws\]|\[Dwct\]' | sort -u | head -20
  exit 1
fi
if grep -a -q '\[Gyws\]' "$SO_BUILT" || grep -a -q '\[Dwct\]' "$SO_BUILT" || grep -a -q '\[Zmxy\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款游戏标签，jniLibs 未刷新"
  exit 1
fi

echo "[1b/6] gradle ${GRADLE_TASK} (pack refreshed jniLibs so)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
mkdir -p "$WORK/starry/app/src/main/jniLibs/arm64-v8a"
cp -f "$SO_BUILT" "$WORK/starry/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
echo "  so -> temp jniLibs ok (shhj verified)"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Shhj\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 SHHJ —— 菜单会错成上一款游戏"
  strings "$WORK/check.so" | grep -E '\[Shhj\]|\[Gyws\]|\[Dwct\]|shhj|gyws' | sort -u | head -20
  exit 1
fi
echo "  gradle APK so verified [Shhj]"

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes4.dex (StarryApp → GameApplication)"
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
    (".super Landroid/app/Application;", ".super Lcom/chx/shhj/GameApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/chx/shhj/GameApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/chx/shhj/GameApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/chx/shhj/GameApplication;->onCreate()V",
    ),
    # 2026-08-27 框架改版新增：isMainProcess 走 Application.getProcessName()
    (
        "Landroid/app/Application;->getProcessName()Ljava/lang/String;",
        "Lcom/chx/shhj/GameApplication;->getProcessName()Ljava/lang/String;",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
        print(f"  patch: {a[:52]}…")
# 兜底：R8 会把继承方法调用重写到定义类（registerActivityLifecycleCallbacks、
# getProcessName 等），剩余 Landroid/app/Application;-> 引用统一改指 GameApplication。
if "Landroid/app/Application;->" in t:
    n2 = t.count("Landroid/app/Application;->")
    t = t.replace("Landroid/app/Application;->",
                  "Lcom/chx/shhj/GameApplication;->")
    changed += 1
    print(f"  patch: catch-all ×{n2} inherited Application calls → GameApplication")
if "Lcom/chx/shhj/GameApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing GameApplication")
if "Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali still references Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp → GameApplication ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
ls -la "$WORK/classes4.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3b/6] patch classes.dex: TapSign + force WebLogin + AppActivity get_ret/MenuService"
unzip -p "$ORIG_APK" classes.dex > "$WORK/classes_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c1smali" "$WORK/classes_orig.dex"
python3.11 - "$WORK/c1smali" "$ORIG_MD5" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
md5 = sys.argv[2]

tap = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
if not tap.is_file():
    raise SystemExit(f"missing {tap}")
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

# 本包 ClientLoginUtils.isClientLogin(Activity)Z（无 Actions 参数）
clu = root / "com/taptap/sdk/login/internal/util/ClientLoginUtils.smali"
if not clu.is_file():
    raise SystemExit(f"missing {clu}")
s = clu.read_text(encoding="utf-8")
if "force WebLogin" not in s:
    new_icl = """.method public final isClientLogin(Landroid/app/Activity;)Z
    .locals 1

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO（改签后必失败）
    const/4 v0, 0x0

    return v0
.end method"""
    s2, n = re.subn(
        r"\.method public final isClientLogin\(Landroid/app/Activity;\)Z[\s\S]*?\.end method",
        new_icl,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("ClientLoginUtils.isClientLogin patch failed")
    clu.write_text(s2, encoding="utf-8")
    print("  ClientLoginUtils.isClientLogin → false (force WebLogin)")
else:
    print("  ClientLoginUtils already forced")

# 双保险：ClientLoginHandler.canHandle → false
ch = root / "com/taptap/sdk/login/internal/handlers/client/ClientLoginHandler.smali"
if ch.is_file():
    s = ch.read_text(encoding="utf-8")
    if "force WebLogin" not in s:
        new_ch = """.method public canHandle(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
        s2, n = re.subn(
            r"\.method public canHandle\(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;\)Z[\s\S]*?\.end method",
            new_ch,
            s,
            count=1,
        )
        if n == 1:
            ch.write_text(s2, encoding="utf-8")
            print("  ClientLoginHandler.canHandle → false")
        else:
            print("  WARN: ClientLoginHandler.canHandle pattern miss (ok if Utils works)")
    else:
        print("  ClientLoginHandler already forced")

act = root / "com/chx/shhj/AppActivity.smali"
if not act.is_file():
    raise SystemExit(f"missing {act}")
s = act.read_text(encoding="utf-8")

if "MenuService;->notifyHostResumed" not in s:
    s2, n = re.subn(
        r"(\.method protected onResume\(\)V[\s\S]*?)(    return-void\n\.end method)",
        r"\1    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n\2",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("AppActivity.onResume patch failed")
    s = s2
    print("  onResume → notifyHostResumed")
else:
    print("  onResume already patched")

if "MenuService;->notifyHostLeaving" not in s:
    s2, n = re.subn(
        r"(\.method protected onPause\(\)V[\s\S]*?)(    return-void\n\.end method)",
        r"\1    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n\2",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("AppActivity.onPause patch failed")
    s = s2
    print("  onPause → notifyHostLeaving")
else:
    print("  onPause already patched")

if "onUserLeaveHint" not in s:
    leave = """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method protected onPause()V"
    if marker not in s:
        raise SystemExit("onPause marker missing")
    s = s.replace(marker, leave + marker, 1)
    print("  +onUserLeaveHint")

if "ShhjHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/ShhjHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/ShhjHooks;->setRet(IZ)V

    return-void
.end method
"""
    print("  +get_ret/set_ret → ShhjHooks")
else:
    print("  get_ret already present")

act.write_text(s, encoding="utf-8")
PY
"$SMALI_BIN" assemble -o "$WORK/classes.dex" "$WORK/c1smali"
ls -la "$WORK/classes.dex"

if [[ "${SHHJ_SPOOF_PACKAGE_SIGN:-0}" == "1" ]]; then
  echo "[3c/6] patch classes3.dex: RSSDK.getMD5 -> official $ORIG_MD5"
  unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
  "$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
  python3.11 - "$WORK/c3smali" "$ORIG_MD5" <<'PY'
from pathlib import Path
import re
import sys

root = Path(sys.argv[1])
md5 = sys.argv[2]
rssdk = root / "org/cocos2dx/javascript/service/RSSDK.smali"
if not rssdk.is_file():
    raise SystemExit(f"missing {rssdk}")
method = f""".method public static getMD5()Ljava/lang/String;
    .registers 1

    # starry_package_sign_spoof
    const-string v0, "{md5}"

    return-object v0
.end method"""
text = rssdk.read_text(encoding="utf-8")
patched, count = re.subn(
    r"\.method public static getMD5\(\)Ljava/lang/String;[\s\S]*?\.end method",
    method,
    text,
    count=1,
)
if count != 1:
    raise SystemExit("RSSDK.getMD5 patch failed")
rssdk.write_text(patched, encoding="utf-8")
print(f"  RSSDK.getMD5 -> {md5}")
PY
  "$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"
  ls -la "$WORK/classes3.dex"
fi

echo "[4/6] binary AndroidManifest (GameApplication → StarryApp + MenuService)"
if [[ -n "${SHHJ_BINARY_MANIFEST:-}" && -f "$SHHJ_BINARY_MANIFEST" ]]; then
  cp "$SHHJ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
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
t = t.replace(
    'android:name="com.chx.shhj.GameApplication"',
    'android:name="com.starry.magic.StarryApp"',
)
if 'android:name="com.starry.magic.StarryApp"' not in t:
    raise SystemExit("ERROR: failed to set StarryApp as application")
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
c1 = (work / "classes.dex").read_bytes()
c4 = (work / "classes4.dex").read_bytes()
c3_path = work / "classes3.dex"
c3 = c3_path.read_bytes() if c3_path.is_file() else None
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

replace = {
    "AndroidManifest.xml": mf,
    "classes.dex": c1,
}
if c3 is not None:
    replace["classes3.dex"] = c3

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name == "classes4.dex":
        continue
    if name in replace:
        data = replace[name]
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

info = zipfile.ZipInfo("classes4.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c4)
n += 1

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in ("resources.arsc", "classes2.dex"):
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
if c3 is None:
    assert hashlib.sha256(zin2.read("classes3.dex")).hexdigest() == hashlib.sha256(zout2.read("classes3.dex")).hexdigest(), "classes3.dex"
else:
    assert hashlib.sha256(zout2.read("classes3.dex")).hexdigest() != hashlib.sha256(zin2.read("classes3.dex")).hexdigest(), "classes3.dex"
assert hashlib.sha256(zout2.read("classes.dex")).hexdigest() != hashlib.sha256(zin2.read("classes.dex")).hexdigest()
assert "classes4.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
print(f"  merged ok; entries≈{n}; classes={len(c1)} classes4={len(c4)}")
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
  REMOTE="/data/local/tmp/shhj_surgical.apk"
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
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Shhj:V MenuService:V StarrySignSpoof:V"
fi

echo "DONE: $SIGNED"
echo "NOTE: 默认未装机。真机已装官方时需 STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1。"
