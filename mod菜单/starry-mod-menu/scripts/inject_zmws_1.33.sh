#!/usr/bin/env bash
# 星光魔法 → 造梦无双 1.33 官方包：构建 + 注入 + SignSpoof + 签名装机
# 玩法：ZmwsMod se::evalString splice（对齐斌哥 get_ret 1..4）
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/inject_zmws_1.33.sh
#   STARRY_SKIP_INSTALL=1 bash scripts/inject_zmws_1.33.sh
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
ORIG_APK="$NIXIANG/02_apks/造梦无双/造梦无双_1.33_official_4399.apk"
SRC_DECODE="$NIXIANG/03_decompiled/造梦无双/apktool_1.33_official"
FINAL_PATCH="$NIXIANG/07_patched/造梦无双/apktool_1.33_modmenu"
OUT="$NIXIANG/08_output/造梦无双"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
ORIG_MD5="41065dcf3c36e7dd37a04612965144a1"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"; break
  fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

PKG="com.xm4399.zmws.m4399"
SIGNED="$OUT/造梦无双_1.33_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/造梦无双_1.33_starry-debugSigned.apk"

WORK="$(mktemp -d /tmp/zmws_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$WORK/apktool_modmenu"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/zmws_starry_unsigned.apk"
trap 'rm -rf "$WORK"' EXIT

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_ZMWS' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_ZMWS"; exit 1; }
echo "[check] Android.mk target=zmws"
mkdir -p "$OUT" "$OUT_ASCII" "$(dirname "$SRC_DECODE")"

if [[ ! -f "$SRC_DECODE/AndroidManifest.xml" ]]; then
  echo "[0a] apktool decode official"
  DEC_TMP="$(mktemp -d /tmp/zmws_dec.XXXXXX)"
  apktool d -f -o "$DEC_TMP/apktool_1.33_official" "$ORIG_APK"
  rm -rf "$SRC_DECODE"
  mv "$DEC_TMP/apktool_1.33_official" "$SRC_DECODE"
  rmdir "$DEC_TMP" 2>/dev/null || true
fi

echo "[0/7] rsync starry → ASCII work + copy decode"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$STARRY_BUILD/"
cp -a "$SRC_DECODE" "$PATCH"

APP="$STARRY_BUILD/app"
echo "[1/7] ndk-build + assemble ($GRADLE_TASK)"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8
mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$APP/build/intermediates/ndk/libs/arm64-v8a/"*.so "$APP/src/main/jniLibs/arm64-v8a/"
python3.11 - <<PY
from pathlib import Path
import re
p = Path("$APP/build.gradle")
t = p.read_text()
t2 = re.sub(
    r"\n    externalNativeBuild \{[\s\S]*?\n    \}\n",
    "\n    // externalNativeBuild disabled (ASCII CI): so from jniLibs\n",
    t, count=1)
p.write_text(t2)
PY
(cd "$STARRY_BUILD" && ./gradlew "$GRADLE_TASK" --console=plain)
APK="$STARRY_BUILD/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$APK" ]] || { echo "ERROR: missing $APK"; exit 1; }

echo "[2/7] decode starry"
apktool d -f -o "$WORK/starry_dec" "$APK" >/dev/null

echo "[3/7] inject so/assets/smali"
mkdir -p "$PATCH/lib/arm64-v8a"
cp "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$PATCH/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$PATCH/lib/arm64-v8a/libc++_shared.so"
rm -rf "$PATCH/assets/web" 2>/dev/null || true
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$PATCH/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$PATCH/assets/certs/"
fi
rm -rf "$PATCH/smali_classes7/com/starry"
mkdir -p "$PATCH/smali_classes7/com"
[[ -d "$WORK/starry_dec/smali/com/starry" ]] && cp -R "$WORK/starry_dec/smali/com/starry" "$PATCH/smali_classes7/com/"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali_classes*/com/starry; do
  cp -R "$d" "$PATCH/smali_classes7/com/"
done
[[ -d "$PATCH/smali_classes7/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }

echo "[4/7] patch Manifest / FN4399HzApplication / AppActivity"
python3.11 <<PY
from pathlib import Path
import re

PATCH = Path(r"$PATCH")
ORIG_MD5 = "$ORIG_MD5"

manifest = PATCH / "AndroidManifest.xml"
t = manifest.read_text(encoding="utf-8")
if "com.starry.magic.StarryApplication" in t or "com.starry.magic.StarryApp" in t and 'android:name="com.starry.magic.StarryApp"' in t:
    raise SystemExit("ERROR: do not replace Application with StarryApp")
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
    print("manifest: perms")
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
    print("manifest: MenuService")
manifest.write_text(t, encoding="utf-8")

app_smali = next(PATCH.glob("smali*/com/ssjj/fnsdk/platform/FN4399HzApplication.smali"), None)
if app_smali is None:
    raise SystemExit("FN4399HzApplication.smali missing")
s = app_smali.read_text(encoding="utf-8")
if "SignSpoof;->install" in s and "starry_magic" in s:
    print(f"sign/hook: FN4399HzApplication ok")
else:
    needle = "    invoke-super {p0, p1}, Lcom/ssjj/fnsdk/core/FNSDKLIbApplication;->attachBaseContext(Landroid/content/Context;)V\n"
    inject = needle + """
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
"""
    if needle not in s:
        raise SystemExit("FN4399HzApplication.attachBaseContext super not found")
    s = s.replace(needle, inject, 1)
    app_smali.write_text(s, encoding="utf-8")
    print("sign/hook: FN4399HzApplication ← SignSpoof + loadLibrary")

act = next(PATCH.glob("smali*/com/xm4399/zmws/AppActivity.smali"), None)
if act is None:
    raise SystemExit("AppActivity.smali missing")
s = act.read_text(encoding="utf-8")

super_on = "    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V\n"
early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V
"""
if "MenuService;->start" not in s:
    if super_on not in s:
        raise SystemExit("AppActivity.onCreate super not found")
    oncreate = s.split(".method protected onCreate")[1].split(".end method")[0]
    if "starry_magic" not in oncreate:
        s = s.replace(super_on, early_load, 1)
    marker = """    invoke-virtual {p1, v0}, Landroid/view/Window;->addFlags(I)V

    return-void
.end method"""
    inject = """    invoke-virtual {p1, v0}, Landroid/view/Window;->addFlags(I)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void
.end method"""
    if marker not in s:
        raise SystemExit("AppActivity.onCreate tail not found")
    s = s.replace(marker, inject, 1)
    print("hook: AppActivity.onCreate → loadLibrary + MenuService")
else:
    print("hook: AppActivity.onCreate already patched")

if "MenuService;->notifyHostResumed" not in s:
    s2, n = re.subn(
        r"(\.method protected onResume\(\)V[\s\S]*?)(    :cond_0\n    return-void\n\.end method)",
        r"\1    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n\2",
        s, count=1)
    if n != 1:
        raise SystemExit("AppActivity.onResume patch failed")
    s = s2
    print("hook: AppActivity.onResume")
else:
    print("hook: onResume already patched")

if "MenuService;->notifyHostLeaving" not in s:
    s2, n = re.subn(
        r"(\.method protected onPause\(\)V[\s\S]*?)(    :cond_0\n    return-void\n\.end method)",
        r"\1    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n\2",
        s, count=1)
    if n != 1:
        raise SystemExit("AppActivity.onPause patch failed")
    s = s2
    print("hook: AppActivity.onPause")
else:
    print("hook: onPause already patched")

if "onUserLeaveHint" not in s:
    leave = """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method protected onPause()V"
    if marker not in s:
        raise SystemExit("onPause marker missing")
    s = s.replace(marker, leave + marker, 1)
    print("hook: AppActivity.onUserLeaveHint")

if "ZmwsHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/ZmwsHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/ZmwsHooks;->setRet(IZ)V

    return-void
.end method
"""
    print("hook: AppActivity.get_ret/set_ret → ZmwsHooks")
else:
    print("hook: get_ret already present")

act.write_text(s, encoding="utf-8")
print(f"sign: ORIG_MD5={ORIG_MD5} (SignSpoof ZMWS cert)")
print("hooks done")
PY

echo "[5/7] combat = ZmwsMod evalString splice"
echo "[6/7] apktool rebuild — large, wait"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + install"
UBER_ARGS=(-a "$UNSIGNED" --overwrite --allowResign)
[[ -n "$BT_ZIPALIGN" ]] && UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
mkdir -p "$OUT"
SIGNED_SRC=""
for c in \
  "${UNSIGNED%.apk}-aligned-debugSigned.apk" \
  "${UNSIGNED%.apk}-aligned-releaseSigned.apk" \
  "${UNSIGNED%.apk}-debugSigned.apk" \
  "${UNSIGNED%.apk}-releaseSigned.apk" \
  "$UNSIGNED"; do
  if [[ -f "$c" ]]; then SIGNED_SRC="$c"; break; fi
done
cp -f "$SIGNED_SRC" "$SIGNED"
rm -rf "$FINAL_PATCH"
mkdir -p "$(dirname "$FINAL_PATCH")"
cp -a "$PATCH" "$FINAL_PATCH"
ls -lh "$SIGNED"

if [[ "${STARRY_SKIP_INSTALL:-0}" == "1" ]]; then
  echo "skip install"; echo "done → $SIGNED"; exit 0
fi
if [[ "${STARRY_INSTALL:-1}" != "1" ]]; then
  echo "STARRY_INSTALL!=1 skip install"; echo "done → $SIGNED"; exit 0
fi

REMOTE_APK="/data/local/tmp/starry_zmws_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
if ! adb shell su -c "pm install -r -d --user 0 '$REMOTE_APK'"; then
  echo "signature mismatch → uninstall + reinstall (assets in APK; not PCR-scale)"
  adb uninstall "$PKG" 2>/dev/null || true
  adb install --no-incremental "$SIGNED"
fi
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG" || true
adb shell am force-stop "$PKG" 2>/dev/null || true
set +e
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
set -e
echo "done → $SIGNED"
echo "log: adb logcat -s StarrySignSpoof:V StarryMagic:V Zmws:V MenuService:V"
