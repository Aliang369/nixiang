#!/usr/bin/env bash
# 生存33天 1.0.23 TapTap — 外科 zip 组装（禁止对游戏 apktool b 整包）
#
# - 原包 classes.dex~classes9.dex / resources.arsc 字节保留
# - 仅替换 classes10.dex（JMYBTActivity/SdkManager 补丁）
# - 新增 classes11.dex（com/starry）+ libstarry_magic.so + assets/certs
# - AndroidManifest：JMYBTApplication → StarryApp + MenuService
# - assets/sq_plugin_*.apk：仅嵌套插件 apktool 改 TapSign（非游戏本体）
#
# 产物：08_output/生存33天/生存33天_1.0.23_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_sc33_1.0.23.sh
#   STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1 bash scripts/surgical_sc33_1.0.23.sh
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
ORIG_APK="$NIXIANG/02_apks/生存33天/生存33天_1.0.23_taptap_official.apk"
OUT="$NIXIANG/08_output/生存33天"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive"
SIGNED="$OUT/生存33天_1.0.23_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/生存33天_1.0.23_surgical-debugSigned.apk"
ALIAS_APK="$OUT/生存33天_1.0.23_starry.apk"
MF_CACHE="$NIXIANG/07_patched/生存33天/surgical_1.0.23/AndroidManifest.xml"
ORIG_MD5="8fa00ae1397609e5e09ff34cc43030e3"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_SC33' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_SC33"; exit 1; }
echo "[check] Android.mk target=sc33"

LOCK_DIR="/tmp/sc33_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/sc33_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] ndk-build -> jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Sc33\]' "$SO_BUILT" || ! grep -a -q 'sc33' "$SO_BUILT"; then
  echo "ERROR: so 不是 SC33"
  strings "$SO_BUILT" | grep -E 'Sc33|sc33|\[Haly\]|\[Shhj\]' | sort -u | head -20
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
echo "  so -> jniLibs ok ([Sc33] verified)"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q '\[Sc33\]' "$WORK/check.so" || { echo "ERROR: gradle APK so 不是 SC33"; exit 1; }

echo "[2/7] apktool d starry template（小模板 APK，非游戏包）"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] classes11.dex = com/starry（StarryApp → JMYBTApplication）"
SMALI_ROOT="$WORK/smali_starry"
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
import re, sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/sanqi/jmybt/game37/JMYBTApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/sanqi/jmybt/game37/JMYBTApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->onCreate()V",
    ),
]
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
if "Lcom/sanqi/jmybt/game37/JMYBTApplication;" not in t:
    raise SystemExit("ERROR: StarryApp missing JMYBTApplication super")
if ".method protected attachBaseContext" not in t:
    t = t.rstrip() + """

.method protected attachBaseContext(Landroid/content/Context;)V
    .locals 1
    .param p1, "base"    # Landroid/content/Context;

    invoke-super {p0, p1}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->attachBaseContext(Landroid/content/Context;)V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    return-void
.end method
"""
    print("  StarryApp +attachBaseContext loadLibrary")
p.write_text(t, encoding="utf-8")
print("  StarryApp → JMYBTApplication ok")
PY
"$SMALI_BIN" assemble -o "$WORK/classes11.dex" "$SMALI_ROOT"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] patch classes10.dex（JMYBTActivity + SdkManager）"
unzip -p "$ORIG_APK" classes10.dex > "$WORK/classes10_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c10smali" "$WORK/classes10_orig.dex"
python3.11 - "$WORK/c10smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])

act = root / "com/sanqi/jmybt/game37/JMYBTActivity.smali"
if not act.is_file():
    raise SystemExit("missing JMYBTActivity.smali")
s = act.read_text(encoding="utf-8")

super_on = "    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"
early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"""
if "MenuService;->start" not in s:
    if super_on not in s:
        raise SystemExit("JMYBTActivity.onCreate super not found")
    s = s.replace(super_on, early_load, 1)
    marker = """    invoke-virtual {p1, v0}, Landroid/view/Window;->addFlags(I)V

    return-void
.end method"""
    inject = """    invoke-virtual {p1, v0}, Landroid/view/Window;->addFlags(I)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void
.end method"""
    if marker not in s:
        raise SystemExit("JMYBTActivity.onCreate tail not found")
    s = s.replace(marker, inject, 1)
    print("  JMYBTActivity.onCreate early loadLibrary + MenuService")
else:
    print("  JMYBTActivity.onCreate already patched")

def inject_lifecycle(method: str, leaving: bool):
    global s
    if leaving:
        needle = "MenuService;->notifyHostLeaving"
        call = "    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n"
    else:
        needle = "MenuService;->notifyHostResumed"
        call = "    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n"
    if needle in s:
        return
    pat = rf"(\.method protected {method}\(\)V[\s\S]*?)(    return-void\n\.end method)"
    s2, n = re.subn(pat, r"\1" + call + r"\2", s, count=1)
    if n != 1:
        raise SystemExit(f"JMYBTActivity.{method} patch failed")
    s = s2
    print(f"  JMYBTActivity.{method} → {needle.split(';')[1]}")

inject_lifecycle("onResume", False)
inject_lifecycle("onPause", True)

if "onUserLeaveHint" not in s:
    leave = """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    s = s.replace(".method protected onPause()V", leave + ".method protected onPause()V", 1)
    print("  +onUserLeaveHint")

if "Sc33Hooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/Sc33Hooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/Sc33Hooks;->setRet(IZ)V

    return-void
.end method
"""
    print("  +get_ret/set_ret → Sc33Hooks")
act.write_text(s, encoding="utf-8")

sdk = root / "com/sanqi/jmybt/game37/sdk/SdkManager.smali"
if not sdk.is_file():
    raise SystemExit("missing SdkManager.smali")
ss = sdk.read_text(encoding="utf-8")
if "Sc33Hooks;->shouldSkipAd" not in ss:
    ad_new = """.method public SdkShowAdReward(Ljava/lang/String;)V
    .locals 3
    .annotation system Ldalvik/annotation/MethodParameters;
        accessFlags = {
            0x0
        }
        names = {
            "json"
        }
    .end annotation

    invoke-static {}, Lcom/starry/magic/cocos/Sc33Hooks;->shouldSkipAd()Z

    move-result v0

    if-eqz v0, :cond_starry_ad

    const-string v0, "[SdkManager](SdkShowAdReward) starry skip ad → success"

    invoke-static {v0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->logi(Ljava/lang/String;)V

    new-instance v0, Lcom/alibaba/fastjson/JSONObject;

    invoke-direct {v0}, Lcom/alibaba/fastjson/JSONObject;-><init>()V

    const/4 v1, 0x1

    invoke-static {v1}, Ljava/lang/Boolean;->valueOf(Z)Ljava/lang/Boolean;

    move-result-object v2

    const-string v1, "result"

    invoke-virtual {v0, v1, v2}, Lcom/alibaba/fastjson/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Ljava/lang/Object;

    const/4 v1, 0x1

    invoke-static {v1}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v2

    const-string v1, "state"

    invoke-virtual {v0, v1, v2}, Lcom/alibaba/fastjson/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Ljava/lang/Object;

    const-string v1, "SdkShowAdReward"

    invoke-virtual {p0, v1, v0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->call(Ljava/lang/String;Lcom/alibaba/fastjson/JSONObject;)V

    return-void

    :cond_starry_ad
    const-string v0, "[SdkManager](SdkShowAdReward)\u8c03\u7528\u5c55\u793a\u6fc0\u52b1\u5e7f\u544a"

    invoke-static {v0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->logi(Ljava/lang/String;)V

    invoke-static {p1}, Lcom/alibaba/fastjson/JSONObject;->parseObject(Ljava/lang/String;)Lcom/alibaba/fastjson/JSONObject;

    move-result-object p1

    const-string v0, "content"

    invoke-virtual {p1, v0}, Lcom/alibaba/fastjson/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object p1

    invoke-static {}, Lcom/sqwan/msdk/SQwanCore;->getInstance()Lcom/sqwan/msdk/SQwanCore;

    move-result-object v0

    iget-object v1, p0, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->mContext:Landroid/content/Context;

    new-instance v2, Lcom/sanqi/jmybt/game37/sdk/SdkManager$13;

    invoke-direct {v2, p0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager$13;-><init>(Lcom/sanqi/jmybt/game37/sdk/SdkManager;)V

    invoke-virtual {v0, v1, p1, v2}, Lcom/sqwan/msdk/SQwanCore;->showAdReward(Landroid/content/Context;Ljava/lang/String;Lcom/sqwan/msdk/api/SQResultListener;)V

    return-void
.end method"""
    ss2, n = re.subn(
        r"\.method public SdkShowAdReward\(Ljava/lang/String;\)V[\s\S]*?\.end method",
        ad_new,
        ss,
        count=1,
    )
    if n != 1:
        raise SystemExit("SdkShowAdReward patch failed")
    sdk.write_text(ss2, encoding="utf-8")
    print("  SdkShowAdReward → Sc33Hooks.shouldSkipAd")
else:
    print("  SdkShowAdReward ok")
PY
"$SMALI_BIN" assemble -o "$WORK/classes10.dex" "$WORK/c10smali"

echo "[4b/7] patch sq_plugin（嵌套插件，非游戏 apktool b）"
PLUGIN_NAME=$(unzip -Z1 "$ORIG_APK" 'assets/sq_plugin_*.apk' | head -1)
[[ -n "$PLUGIN_NAME" ]] || { echo "ERROR: sq_plugin missing in orig"; exit 1; }
unzip -p "$ORIG_APK" "$PLUGIN_NAME" > "$WORK/plugin_orig.apk"
apktool d -f -o "$WORK/plug_dec" "$WORK/plugin_orig.apk" >/dev/null
python3.11 - "$WORK/plug_dec" "$ORIG_MD5" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
md5 = sys.argv[2]
taps = list(root.rglob("TapSignKit.smali"))
if not taps:
    raise SystemExit("TapSignKit missing in sq_plugin")
TAP = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "{md5}"

    return-object v0
.end method"""
for tap in taps:
    ts = tap.read_text(encoding="utf-8")
    if md5 in ts and "starry_sign_bypass" in ts:
        print(f"  TapSignKit ok ({tap.relative_to(root)})")
        continue
    ts2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        TAP, ts, count=1,
    )
    if n != 1:
        raise SystemExit(f"TapSignKit patch failed: {tap}")
    tap.write_text(ts2, encoding="utf-8")
    print(f"  TapSignKit.getSign → {md5} ({tap.relative_to(root)})")
PY
apktool b -f "$WORK/plug_dec" -o "$WORK/plugin_patched.apk" >/dev/null
cp -f "$WORK/plugin_patched.apk" "$WORK/assets/$(basename "$PLUGIN_NAME")"

echo "[5/7] binary AndroidManifest（StarryApp + MenuService）"
if [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached Manifest"
else
  apktool d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
t = t.replace(
    'android:name="com.sanqi.jmybt.game37.JMYBTApplication"',
    'android:name="com.starry.magic.StarryApp"',
)
if 'android:name="com.starry.magic.StarryApp"' not in t:
    raise SystemExit("ERROR: failed to set StarryApp")
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

echo "[6/7] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$PLUGIN_NAME" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, work, dst, plugin_name = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
c10 = (work / "classes10.dex").read_bytes()
c11 = (work / "classes11.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
plug = work / "assets" / Path(plugin_name).name
if plug.is_file():
    extras[plugin_name] = plug.read_bytes()

replace = {"AndroidManifest.xml": mf, "classes10.dex": c10}
skip_new = set(extras) | {"classes11.dex"}

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
orig_hashes = {}
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in skip_new:
        continue
    if name in replace:
        data = replace[name]
    else:
        data = zin.read(name)
    orig_hashes[name] = hashlib.sha256(data).hexdigest()
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

info = zipfile.ZipInfo("classes11.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c11)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name, h in orig_hashes.items():
    if name in replace:
        assert hashlib.sha256(zout2.read(name)).hexdigest() != h, f"{name} should change"
        continue
    assert hashlib.sha256(zout2.read(name)).hexdigest() == h, f"byte drift: {name}"
assert hashlib.sha256(zout2.read("resources.arsc")).hexdigest() == orig_hashes["resources.arsc"]
assert "classes11.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
print(f"  merge ok; classes10 patched; classes11 added; arsc unchanged")
zin2.close(); zout2.close()
PY

echo "[7/7] zipalign + sign → $SIGNED"
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
else
  java -jar "$NIXIANG/01_tools/uber-apk-signer.jar" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" 2>/dev/null || \
  java -jar "$NIXIANG/01_tools/uber-apk-signer.jar" -a "$ALIGNED" --overwrite --allowResign
  CAND=$(ls -t "$OUT"/*surgical*Signed*.apk "$OUT"/*releaseSigned*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
cp -f "$SIGNED" "$ALIAS_APK"
ls -lh "$SIGNED" "$ALIAS_APK"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/sc33_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  elif [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
    adb uninstall "$PKG" 2>/dev/null || true
    adb install --no-incremental "$SIGNED"
  else
    echo "ERROR: install failed; set STARRY_ALLOW_UNINSTALL=1"; exit 2
  fi
  _NIXIANG="$(cd "$SCRIPT_DIR/../../.." && pwd)"
  # shellcheck source=/dev/null
  [[ -f "$_NIXIANG/04_scripts/_common/starry_post_install.sh" ]] && \
    source "$_NIXIANG/04_scripts/_common/starry_post_install.sh" && starry_post_install "$PKG" || true
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb logcat -s StarryApp StarryMagic Sc33 MenuService"
fi

echo "DONE: $SIGNED"
