#!/usr/bin/env bash
# 星光魔法 → 堡垒之战 1.0.30：构建 + 注入 + 签名绕过 + 正式包
# 用法:
#   bash scripts/inject_blzz_1.0.30.sh                           # 默认 release
#   STARRY_BUILD_TYPE=debug bash scripts/inject_blzz_1.0.30.sh
#   STARRY_SKIP_INSTALL=1 bash scripts/inject_blzz_1.0.30.sh
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug (got: $BUILD_TYPE)"
  exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"
else
  GRADLE_TASK="assembleDebug"
fi

STARRY="$(cd "$(dirname "$0")/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
SRC_DECODE="$NIXIANG/03_decompiled/堡垒之战/apktool_1.0.30"
PATCH="$NIXIANG/07_patched/堡垒之战/apktool_1.0.30_modmenu"
OUT="$NIXIANG/08_output/堡垒之战"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK_CXX="$HOME/Library/Android/sdk/ndk/27.0.12077973/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"
    break
  fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

PKG="com.ultralisk.game304"
UNSIGNED="$OUT/堡垒之战_1.0.30_starry_unsigned.apk"
SIGNED="$OUT/堡垒之战_1.0.30_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/堡垒之战_1.0.30_starry-debugSigned.apk"
TMP="$(mktemp -d /tmp/starry_blzz30_inject.XXXXXX)"
trap 'rm -rf "$TMP"' EXIT

[[ -d "$SRC_DECODE" ]] || { echo "ERROR: missing decode $SRC_DECODE"; exit 1; }
mkdir -p "$OUT" "$(dirname "$PATCH")"

echo "[0/7] refresh 07_patched from decode"
rm -rf "$PATCH"
cp -a "$SRC_DECODE" "$PATCH"

echo "[1/7] build starry $BUILD_TYPE via $GRADLE_TASK"
(cd "$STARRY" && ./gradlew "$GRADLE_TASK" --console=plain)
APK="$STARRY/app/build/outputs/apk/$BUILD_TYPE/app-$BUILD_TYPE.apk"
[[ -f "$APK" ]] || { echo "ERROR: missing $APK"; exit 1; }
echo "using: $APK"

echo "[2/7] decode starry"
apktool d -f -o "$TMP/starry_dec" "$APK" >/dev/null

echo "[3/7] inject so/assets/smali"
mkdir -p "$PATCH/lib/arm64-v8a"
cp "$TMP/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$PATCH/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$PATCH/lib/arm64-v8a/libc++_shared.so"

# 原生菜单：不再拷贝 assets/web；清掉旧包残留 HTML
rm -rf "$PATCH/assets/web" 2>/dev/null || true
if [[ -d "$TMP/starry_dec/assets/certs" ]]; then
  mkdir -p "$PATCH/assets/certs"
  cp -R "$TMP/starry_dec/assets/certs/." "$PATCH/assets/certs/"
fi

# 游戏仅有 smali / smali_classes2 → 新建 smali_classes3 放星光
rm -rf "$PATCH/smali_classes3/com/starry"
mkdir -p "$PATCH/smali_classes3/com"
[[ -d "$TMP/starry_dec/smali/com/starry" ]] && cp -R "$TMP/starry_dec/smali/com/starry" "$PATCH/smali_classes3/com/"
for d in "$TMP/starry_dec"/smali_classes*/com/starry; do
  [[ -d "$d" ]] || continue
  cp -R "$d" "$PATCH/smali_classes3/com/"
done
[[ -d "$PATCH/smali_classes3/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }

echo "[4/7] patch Manifest / AppActivity / signature"
python3 <<'PY'
from pathlib import Path
import re

PATCH = Path("${NIXIANG}/07_patched/堡垒之战/apktool_1.0.30_modmenu")
ORIG_MD5 = "f5aef50d3b3f626a8512bc1a87cad012"

# --- Manifest ---
manifest = PATCH / "AndroidManifest.xml"
t = manifest.read_text(encoding="utf-8")
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
                android:value="MOD 菜单悬浮窗服务" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
    print("manifest: MenuService")
manifest.write_text(t, encoding="utf-8")

# --- TapSignKit.getSign → 原版 MD5 ---
tap = PATCH / "smali_classes2/com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
s = tap.read_text(encoding="utf-8")
if ORIG_MD5 not in s:
    new_get = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1

    const-string v0, "{ORIG_MD5}"

    return-object v0
.end method"""
    s2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        new_get,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("TapSignKit.getSign patch failed")
    tap.write_text(s2, encoding="utf-8")
    print("sign: TapSignKit.getSign fixed")
else:
    print("sign: TapSignKit ok")

# --- ClientLoginUtils.isClientLogin → 始终 false（强制 Web 登录）---
# TapTap App SSO 会用 PackageManager 真实签名校验，改签后必然 Fail to login；
# WebLogin 走我们已 patch 的 TapSignKit.getSign。
clu = PATCH / "smali_classes2/com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
s = clu.read_text(encoding="utf-8")
if "force WebLogin" not in s:
    new_icl = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    # starry: force WebLogin — TapTap 客户端 SSO 会校验真实签名导致 Fail to login
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
    print("sign: ClientLoginUtils → force WebLogin")
else:
    print("sign: ClientLoginUtils ok")

# --- ULTool.y → 始终 true ---
ul = PATCH / "smali/cn/ulsdk/utils/ULTool.smali"
s = ul.read_text(encoding="utf-8")
if "starry_sign_bypass" not in s:
    new_y = """.method public static y(Landroid/content/Context;)Z
    .locals 1

    # starry_sign_bypass
    const/4 v0, 0x1

    return v0
.end method"""
    s2, n = re.subn(
        r"\.method public static y\(Landroid/content/Context;\)Z[\s\S]*?\.end method",
        new_y,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("ULTool.y patch failed")
    ul.write_text(s2, encoding="utf-8")
    print("sign: ULTool.y bypass")
else:
    print("sign: ULTool.y ok")

# --- AppActivity.onCreate → loadLibrary + MenuService ---
act = PATCH / "smali/com/cocos/game/AppActivity.smali"
s = act.read_text(encoding="utf-8")
if "MenuService;->start" not in s:
    old_ret = """    invoke-virtual {p1, p0, v0, v1}, Lcn/ulsdk/reflecter/ULSdkReflecter;->onCreate(Landroid/app/Activity;Lcn/ulsdk/reflecter/ULSdkListener;[Ljava/lang/Object;)V

    return-void
.end method"""
    new_ret = """    invoke-virtual {p1, p0, v0, v1}, Lcn/ulsdk/reflecter/ULSdkReflecter;->onCreate(Landroid/app/Activity;Lcn/ulsdk/reflecter/ULSdkListener;[Ljava/lang/Object;)V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void
.end method"""
    if old_ret not in s:
        raise SystemExit("AppActivity.onCreate pattern not found")
    s = s.replace(old_ret, new_ret, 1)
    print("hook: AppActivity.onCreate → MenuService")
else:
    print("hook: AppActivity already has MenuService")

if "MenuService;->notifyHostLeaving" not in s:
    leave = """.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method public onPause()V"
    if marker not in s:
        raise SystemExit("onPause marker missing")
    s = s.replace(marker, leave + marker, 1)
    print("hook: onUserLeaveHint")
else:
    print("hook: onUserLeaveHint ok")

if "MenuService;->notifyHostResumed" not in s:
    old_resume = """.method public onResume()V
    .locals 1

    invoke-super {p0}, Lcom/cocos/lib/CocosActivity;->onResume()V

    invoke-static {}, Lcom/cocos/service/SDKWrapper;->shared()Lcom/cocos/service/SDKWrapper;

    move-result-object v0

    invoke-virtual {v0}, Lcom/cocos/service/SDKWrapper;->onResume()V

    invoke-static {}, Lcn/ulsdk/reflecter/ULSdkReflecter;->getInstance()Lcn/ulsdk/reflecter/ULSdkReflecter;

    move-result-object v0

    invoke-virtual {v0}, Lcn/ulsdk/reflecter/ULSdkReflecter;->onResume()V

    return-void
.end method"""
    new_resume = """.method public onResume()V
    .locals 1

    invoke-super {p0}, Lcom/cocos/lib/CocosActivity;->onResume()V

    invoke-static {}, Lcom/cocos/service/SDKWrapper;->shared()Lcom/cocos/service/SDKWrapper;

    move-result-object v0

    invoke-virtual {v0}, Lcom/cocos/service/SDKWrapper;->onResume()V

    invoke-static {}, Lcn/ulsdk/reflecter/ULSdkReflecter;->getInstance()Lcn/ulsdk/reflecter/ULSdkReflecter;

    move-result-object v0

    invoke-virtual {v0}, Lcn/ulsdk/reflecter/ULSdkReflecter;->onResume()V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V

    return-void
.end method"""
    if old_resume not in s:
        raise SystemExit("AppActivity.onResume pattern not found for notifyHostResumed")
    s = s.replace(old_resume, new_resume, 1)
    print("hook: onResume → notifyHostResumed")
else:
    print("hook: onResume notifyHostResumed ok")

act.write_text(s, encoding="utf-8")
print("AppActivity written")
PY

echo "[5/7] (no jsc gate for blzz)"

echo "[6/7] apktool rebuild"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign (+ optional install)"
UBER_ARGS=(-a "$UNSIGNED" --overwrite --allowResign)
if [[ -n "$BT_ZIPALIGN" ]]; then
  UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
fi
mkdir -p "$OUT/.prev"
for old in "$OUT"/堡垒之战_1.0.30_starry*Signed.apk; do
  [[ -f "$old" ]] || continue
  mv -f "$old" "$OUT/.prev/$(basename "$old").$(date +%Y%m%d%H%M%S)" 2>/dev/null || true
done

java -jar "$SIGNER" "${UBER_ARGS[@]}"
[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing signed apk"; exit 1; }
cp -f "$UNSIGNED" "$SIGNED"
# 原生 UI：不再校验 APK 内 mod-menu-v2.html
ls -lh "$SIGNED"

if [[ "${STARRY_SKIP_INSTALL:-0}" == "1" ]]; then
  echo "skip install (STARRY_SKIP_INSTALL=1)"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_blzz_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
if adb shell su -c "pm install -r -d --user 0 '$REMOTE_APK'" 2>/tmp/starry_pm_err.txt; then
  echo "root pm install ok"
else
  echo "root pm install failed, fallback adb install:"
  cat /tmp/starry_pm_err.txt 2>/dev/null || true
  adb uninstall "$PKG" 2>/dev/null || true
  adb install --no-incremental "$SIGNED"
fi
# 全游戏卡密种子 + 悬浮窗（勿删；换游戏 403 多因此）
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG" || true
adb shell am force-stop "$PKG" 2>/dev/null || true
set +e
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
set -e
echo "done → $SIGNED"
echo "log: adb logcat -s StarryMagic Blzz Mod_Menu MenuService CocosJsBridge"
exit 0
