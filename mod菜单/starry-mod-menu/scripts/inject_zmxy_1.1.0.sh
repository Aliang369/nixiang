#!/usr/bin/env bash
# 星光魔法 → 造梦西游·黎尤浩劫 1.1.0 官方包：构建 + 注入 + SignSpoof + 签名装机
#
# 注意：Gradle/apktool/aapt2 在含中文的路径下会失败；native/gradle/apktool 均在 /tmp ASCII 完成。
#
# 用法:
#   bash scripts/inject_zmxy_1.1.0.sh
#   STARRY_BUILD_TYPE=debug bash scripts/inject_zmxy_1.1.0.sh
#   STARRY_SKIP_INSTALL=1 bash scripts/inject_zmxy_1.1.0.sh
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug (got: $BUILD_TYPE)"
  exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"
  APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"
  APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
ORIG_APK="$NIXIANG/02_apks/造梦西游/造梦西游_黎尤浩劫_orig_from_4399.apk"
SRC_DECODE="$NIXIANG/03_decompiled/造梦西游/apktool_1.1.0_orig"
FINAL_PATCH="$NIXIANG/07_patched/造梦西游/apktool_1.1.0_modmenu"
OUT="$NIXIANG/08_output/造梦西游"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
ORIG_MD5="c07390da1d165f6d004588f7d3847d77"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"
    break
  fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

PKG="com.xm4399.zmxy6hj"
SIGNED="$OUT/造梦西游_1.1.0_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/造梦西游_1.1.0_starry-debugSigned.apk"

WORK="$(mktemp -d /tmp/zmxy_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$WORK/apktool_modmenu"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/zmxy_starry_unsigned.apk"
trap 'rm -rf "$WORK"' EXIT

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
MK_SRC="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -q 'STARRY_GAME_ZMXY' "$MK_SRC" || { echo "ERROR: Android.mk missing STARRY_GAME_ZMXY"; exit 1; }
grep -E 'STARRY_GAME_ZMXY' "$MK_SRC" \
  || { echo "ERROR: Android.mk Android.mk must enable STARRY_GAME_ZMXY"; exit 1; }
echo "[check] Android.mk target=zmxy"
mkdir -p "$OUT" "$OUT_ASCII" "$(dirname "$SRC_DECODE")"

if [[ ! -f "$SRC_DECODE/AndroidManifest.xml" ]]; then
  echo "[0a] apktool decode orig → ASCII then move"
  DEC_TMP="$(mktemp -d /tmp/zmxy_dec.XXXXXX)"
  apktool d -f -o "$DEC_TMP/apktool_1.1.0_orig" "$ORIG_APK"
  rm -rf "$SRC_DECODE"
  mv "$DEC_TMP/apktool_1.1.0_orig" "$SRC_DECODE"
  rmdir "$DEC_TMP" 2>/dev/null || true
fi

echo "[0/7] rsync starry → ASCII work + copy decode"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$STARRY_BUILD/"
cp -a "$SRC_DECODE" "$PATCH"

APP="$STARRY_BUILD/app"
echo "[1/7] ndk-build + assemble ($GRADLE_TASK) in $STARRY_BUILD"
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
    t,
    count=1,
)
p.write_text(t2)
PY
(cd "$STARRY_BUILD" && ./gradlew "$GRADLE_TASK" --console=plain)
APK="$STARRY_BUILD/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$APK" ]] || { echo "ERROR: missing $APK"; exit 1; }
echo "using: $APK"

echo "[2/7] decode starry"
apktool d -f -o "$WORK/starry_dec" "$APK" >/dev/null

echo "[3/7] inject so/assets/smali"
mkdir -p "$PATCH/lib/arm64-v8a"
cp "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$PATCH/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$PATCH/lib/arm64-v8a/libc++_shared.so"
# 原生菜单：不再拷贝 assets/web；清掉旧包残留 HTML
rm -rf "$PATCH/assets/web" 2>/dev/null || true
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$PATCH/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$PATCH/assets/certs/"
fi
# 新 dex：原包到 smali_classes6
rm -rf "$PATCH/smali_classes7/com/starry"
mkdir -p "$PATCH/smali_classes7/com"
[[ -d "$WORK/starry_dec/smali/com/starry" ]] && cp -R "$WORK/starry_dec/smali/com/starry" "$PATCH/smali_classes7/com/"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali_classes*/com/starry; do
  cp -R "$d" "$PATCH/smali_classes7/com/"
done
[[ -d "$PATCH/smali_classes7/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }

echo "[4/7] patch Manifest / FNApplication SignSpoof / AppActivity get_ret+MenuService"
python3.11 <<PY
from pathlib import Path
import re

PATCH = Path(r"$PATCH")
ORIG_MD5 = "$ORIG_MD5"

manifest = PATCH / "AndroidManifest.xml"
t = manifest.read_text(encoding="utf-8")
if "com.ssjj.fnsdk.tool.privacy_policy.FNApplication" not in t:
    print("WARN: FNApplication missing in manifest")
if "com.starry.magic.StarryApplication" in t:
    raise SystemExit("ERROR: 勿替换 Application 为 StarryApplication（需保留 FNApplication）")
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

# FNApplication：尽早 SignSpoof + loadLibrary
app_smali = next(PATCH.glob("smali*/com/ssjj/fnsdk/tool/privacy_policy/FNApplication.smali"), None)
if app_smali is None:
    raise SystemExit("FNApplication.smali missing")
s = app_smali.read_text(encoding="utf-8")
if "SignSpoof;->install" in s and "starry_magic" in s:
    print(f"sign/hook: FNApplication ok ({app_smali.relative_to(PATCH)})")
else:
    needle = "    invoke-super {p0, p1}, Lcom/ssjj/fnsdk/platform/FN4399HzApplication;->attachBaseContext(Landroid/content/Context;)V\n"
    inject = needle + """
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
"""
    if needle not in s:
        raise SystemExit("FNApplication.attachBaseContext super not found")
    # .locals 3 → 需要 v0；原文已有 v0/v1/v2，可复用 v0（随后会被覆盖）
    s = s.replace(needle, inject, 1)
    app_smali.write_text(s, encoding="utf-8")
    print(f"sign/hook: FNApplication ← SignSpoof + loadLibrary ({app_smali.relative_to(PATCH)})")

act = next(PATCH.glob("smali*/com/xm4399/zm6/AppActivity.smali"), None)
if act is None:
    raise SystemExit("AppActivity.smali missing")
s = act.read_text(encoding="utf-8")

# onCreate：super 前 loadLibrary + 尾部 MenuService
super_on = "    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V\n"
early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V
"""
if "MenuService;->start" not in s:
    if super_on not in s:
        raise SystemExit("AppActivity.onCreate super not found")
    # 避免重复 early load
    if "starry_magic" not in s.split(".method protected onCreate")[1].split(".end method")[0]:
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
    print("hook: AppActivity.onCreate → early loadLibrary + MenuService")
elif "starry_magic" in s:
    print("hook: AppActivity.onCreate already patched")
else:
    print("hook: AppActivity MenuService present (load order check)")

# onResume / onPause
if "MenuService;->notifyHostResumed" not in s:
    s2, n = re.subn(
        r"(\.method protected onResume\(\)V[\s\S]*?)(    :cond_0\n    return-void\n\.end method)",
        r"\1    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n\2",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("AppActivity.onResume patch failed")
    s = s2
    print("hook: AppActivity.onResume → notifyHostResumed")
else:
    print("hook: onResume already patched")

if "MenuService;->notifyHostLeaving" not in s:
    s2, n = re.subn(
        r"(\.method protected onPause\(\)V[\s\S]*?)(    :cond_0\n    return-void\n\.end method)",
        r"\1    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n\2",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("AppActivity.onPause patch failed")
    s = s2
    print("hook: AppActivity.onPause → notifyHostLeaving")
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
        raise SystemExit("onPause marker missing for onUserLeaveHint")
    s = s.replace(marker, leave + marker, 1)
    print("hook: AppActivity.onUserLeaveHint")

# get_ret / set_ret → ZmxyHooks（对齐斌哥 JS）
if "ZmxyHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/ZmxyHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/ZmxyHooks;->setRet(IZ)V

    return-void
.end method
"""
    print("hook: AppActivity.get_ret/set_ret → ZmxyHooks")
else:
    print("hook: get_ret already present")

act.write_text(s, encoding="utf-8")
print(f"sign: ORIG_MD5={ORIG_MD5} (SignSpoof)")
print("hooks done")
PY

echo "[5/7] (combat = evalString splice in ZmxyMod)"
echo "[6/7] apktool rebuild (ASCII) — large APK, wait"
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
  echo "skip install"
  echo "done → $SIGNED"
  exit 0
fi
if [[ "${STARRY_INSTALL:-1}" != "1" ]]; then
  echo "STARRY_INSTALL!=1 skip install"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_zmxy_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
if ! adb shell su -c "pm install -r -d --user 0 '$REMOTE_APK'"; then
  echo "signature mismatch? uninstall + reinstall"
  adb uninstall "$PKG" 2>/dev/null || true
  adb install --no-incremental "$SIGNED"
fi
# 全游戏卡密种子 + 悬浮窗（勿删；换游戏 403 多因此）
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
  || true
adb shell am force-stop "$PKG" 2>/dev/null || true
set +e
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
set -e
echo "done → $SIGNED"
echo "log: adb logcat -s StarrySignSpoof:V StarryMagic:V Zmxy:V Mod_Menu:V"
