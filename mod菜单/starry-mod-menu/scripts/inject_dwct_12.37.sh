#!/usr/bin/env bash
# 星光魔法 → 动物餐厅 12.37：构建 + 注入 + 签名绕过 + 广告/内购钩子 + 正式包
# 用法:
#   bash scripts/inject_dwct_12.37.sh                           # 默认 release（R8，上线）
#   STARRY_BUILD_TYPE=debug bash scripts/inject_dwct_12.37.sh   # 仅联调
#   STARRY_SKIP_INSTALL=1 bash scripts/inject_dwct_12.37.sh     # 只出包不装机
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
PATCH="$NIXIANG/07_patched/动物餐厅/apktool_12.37_modmenu"
OUT="$NIXIANG/08_output/动物餐厅"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK_CXX="$HOME/Library/Android/sdk/ndk/27.0.12077973/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"
    break
  fi
done
export JAVA_HOME="${JAVA_HOME:-$HOME/Library/Java/JavaVirtualMachines/jdk-17.0.2.jdk/Contents/Home}"
export PATH="$JAVA_HOME/bin:$PATH"

PKG="inland.twgame.restaurant"
UNSIGNED="$OUT/动物餐厅_12.37_starry_unsigned.apk"
SIGNED="$OUT/动物餐厅_12.37_starry-${BUILD_TYPE}Signed.apk"
TMP="$(mktemp -d /tmp/starry_dwct37_inject.XXXXXX)"
trap 'rm -rf "$TMP"' EXIT

[[ -d "$PATCH" ]] || { echo "ERROR: missing $PATCH"; exit 1; }
mkdir -p "$OUT"

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

rm -rf "$PATCH/smali_classes4/com/starry"
mkdir -p "$PATCH/smali_classes4/com"
[[ -d "$TMP/starry_dec/smali/com/starry" ]] && cp -R "$TMP/starry_dec/smali/com/starry" "$PATCH/smali_classes4/com/"
for d in "$TMP/starry_dec"/smali_classes*/com/starry; do
  [[ -d "$d" ]] || continue
  cp -R "$d" "$PATCH/smali_classes4/com/"
done
[[ -d "$PATCH/smali_classes4/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }

echo "[4/7] patch Manifest / AppActivity / signature / ad hooks"
python3 <<'PY'
from pathlib import Path

PATCH = Path("${NIXIANG}/07_patched/动物餐厅/apktool_12.37_modmenu")

# --- Manifest ---
manifest = PATCH / "AndroidManifest.xml"
t = manifest.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'android.permission.FOREGROUND_SERVICE"' not in t and "FOREGROUND_SERVICE/>" not in t:
    # avoid matching SPECIAL_USE
    if "android.permission.FOREGROUND_SERVICE\"" not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
        if 'name="android.permission.FOREGROUND_SERVICE"' not in t:
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

# --- DeviceHelper.getSignature → 原版 SHA1 ---
dh = PATCH / "smali_classes3/org/cocos2dx/javascript/util/DeviceHelper.smali"
s = dh.read_text(encoding="utf-8")
if "94F80FC5161E41BAD58C6541E739920425BAE53D" not in s:
    old = """.method public static getSignature(Ljava/lang/String;)Ljava/lang/String;
    .locals 3

    .line 1
    :try_start_0
    sget-object v0, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;"""
    # replace whole method body with fixed return
    import re
    s2, n = re.subn(
        r"\.method public static getSignature\(Ljava/lang/String;\)Ljava/lang/String;[\s\S]*?\.end method",
        """.method public static getSignature(Ljava/lang/String;)Ljava/lang/String;
    .locals 1

    const-string v0, "94F80FC5161E41BAD58C6541E739920425BAE53D"

    return-object v0
.end method""",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("DeviceHelper.getSignature patch failed")
    dh.write_text(s2, encoding="utf-8")
    print("sig: DeviceHelper.getSignature fixed SHA1")
else:
    print("sig: DeviceHelper ok")

# --- TapSignKit.getSign → 原版 MD5 ---
ts = PATCH / "smali_classes3/com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
s = ts.read_text(encoding="utf-8")
if "3169d253f38147f0f7cb6ba1672e6255" not in s:
    import re
    s2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        """.method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    const-string v0, "3169d253f38147f0f7cb6ba1672e6255"

    return-object v0
.end method""",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("TapSignKit.getSign patch failed")
    ts.write_text(s2, encoding="utf-8")
    print("sig: TapSignKit.getSign fixed MD5")
else:
    print("sig: TapSignKit ok")

# --- AppActivity ad hooks + MenuService start ---
act = PATCH / "smali_classes3/org/cocos2dx/javascript/AppActivity.smali"
s = act.read_text(encoding="utf-8")

# isRewardVideoADReady
if "DwctHooks;->shouldSkipAd" not in s or "isRewardVideoADReady" in s:
    import re
    ready_new = """.method public static isRewardVideoADReady()Z
    .locals 1

    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->shouldSkipAd()Z

    move-result v0

    if-eqz v0, :cond_starry

    const/4 v0, 0x1

    return v0

    :cond_starry
    sget-boolean v0, Lorg/cocos2dx/javascript/tapAd/tapAdManager;->isInited:Z

    if-eqz v0, :cond_0

    invoke-static {}, Lorg/cocos2dx/javascript/tapAd/tapAdManager;->isVideoReady()Z

    move-result v0

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    goto :goto_0

    :cond_0
    const/4 v0, 0x0

    :goto_0
    return v0
.end method"""
    s2, n = re.subn(
        r"\.method public static isRewardVideoADReady\(\)Z[\s\S]*?\.end method",
        ready_new,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("isRewardVideoADReady patch failed")
    s = s2
    print("ad: isRewardVideoADReady hooked")

# showRewardAd
show_new = """.method public static showRewardAd()V
    .locals 2

    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->shouldSkipAd()Z

    move-result v0

    if-eqz v0, :cond_starry

    const-string v0, "reward"

    const/4 v1, 0x0

    invoke-static {v0, v1}, Lorg/cocos2dx/javascript/AppActivity;->callToJs(Ljava/lang/String;I)V

    return-void

    :cond_starry
    sget-object v0, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    new-instance v1, Lorg/cocos2dx/javascript/AppActivity$a;

    invoke-direct {v1}, Lorg/cocos2dx/javascript/AppActivity$a;-><init>()V

    invoke-virtual {v0, v1}, Landroid/app/Activity;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method"""
if "showRewardAd" in s:
    import re
    # only replace if not already hooked with callToJs reward skip
    if 'const-string v0, "reward"' not in s.split("showRewardAd")[1][:500]:
        s2, n = re.subn(
            r"\.method public static showRewardAd\(\)V[\s\S]*?\.end method",
            show_new,
            s,
            count=1,
        )
        if n != 1:
            raise SystemExit("showRewardAd patch failed")
        s = s2
        print("ad: showRewardAd hooked")
    else:
        print("ad: showRewardAd ok")

# onCreate → MenuService
if "MenuService;->start" not in s:
    old = """.method protected onCreate(Landroid/os/Bundle;)V
    .locals 0

    .line 1
    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V

    .line 2
    sput-object p0, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    .line 3
    sput-object p0, Lorg/cocos2dx/javascript/AppActivity;->sContext:Landroid/content/Context;

    .line 4
    invoke-virtual {p0}, Landroid/app/Activity;->isTaskRoot()Z

    move-result p1

    if-nez p1, :cond_0

    return-void

    .line 5
    :cond_0
    sget-object p1, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    invoke-static {p1}, Lorg/cocos2dx/javascript/JsbHelper;->Init(Lorg/cocos2dx/javascript/AppActivity;)V

    .line 6
    invoke-static {}, Lorg/cocos2dx/javascript/SDKWrapper;->getInstance()Lorg/cocos2dx/javascript/SDKWrapper;

    move-result-object p1

    invoke-virtual {p1, p0}, Lorg/cocos2dx/javascript/SDKWrapper;->init(Landroid/content/Context;)V

    return-void
.end method"""
    new = old.replace(".locals 0", ".locals 1", 1).replace(
        """    invoke-virtual {p1, p0}, Lorg/cocos2dx/javascript/SDKWrapper;->init(Landroid/content/Context;)V

    return-void
.end method""",
        """    invoke-virtual {p1, p0}, Lorg/cocos2dx/javascript/SDKWrapper;->init(Landroid/content/Context;)V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void
.end method""",
    )
    if old not in s:
        raise SystemExit("onCreate pattern not found")
    s = s.replace(old, new, 1)
    print("hook: AppActivity.onCreate → MenuService")
else:
    print("hook: AppActivity already has MenuService")

act.write_text(s, encoding="utf-8")
print("AppActivity written")

# --- onUserLeaveHint：上划/Home 尽早藏悬浮窗（早于 onPause）---
s = act.read_text(encoding="utf-8")
if "MenuService;->notifyHostLeaving" not in s:
    leave = """.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method protected onPause()V"
    if marker not in s:
        raise SystemExit("onPause marker missing for onUserLeaveHint insert")
    s = s.replace(marker, leave + marker, 1)
    act.write_text(s, encoding="utf-8")
    print("hook: AppActivity.onUserLeaveHint → MenuService.notifyHostLeaving")
else:
    print("hook: onUserLeaveHint already present")
PY

echo "[5/7] jsc 免内购门控 (__starryNoIap)"
SCRIPT_JSC="$PATCH/assets/assets/Script/index.jsc"
if LC_ALL=C grep -aq '__starryNoIap' "$SCRIPT_JSC" 2>/dev/null; then
  echo "iap gate: already present in Script/index.jsc"
else
  node "$STARRY/scripts/patch_dwct_iap_gate.js" "$PATCH/assets/assets"
fi

echo "[6/7] apktool rebuild"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign (+ optional install)"
UBER_ARGS=(-a "$UNSIGNED" --overwrite --allowResign)
if [[ -n "$BT_ZIPALIGN" ]]; then
  UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
fi
# 先挪走旧的 *Signed.apk，避免误把历史包当新品（曾导致装上 8/7 旧菜单）
mkdir -p "$OUT/.prev"
for old in "$OUT"/动物餐厅_12.37_starry*Signed.apk "$OUT"/动物餐厅_12.37_starry_unsigned-aligned-debugSigned.apk; do
  [[ -f "$old" ]] || continue
  mv -f "$old" "$OUT/.prev/$(basename "$old").$(date +%Y%m%d%H%M%S)" 2>/dev/null || true
done

java -jar "$SIGNER" "${UBER_ARGS[@]}"
# --overwrite：签名写回 UNSIGNED 本身，必须用它作为正式包来源
[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing signed unsigned apk"; exit 1; }
cp -f "$UNSIGNED" "$SIGNED"
# 兼容旧文件名（内容与 release 相同，便于习惯路径）
cp -f "$SIGNED" "$OUT/动物餐厅_12.37_starry-debugSigned.apk"
# 校验菜单 HTML 已是当前源（防再装旧包）
# 原生 UI：不再校验 APK 内 mod-menu-v2.html
ls -lh "$SIGNED"

if [[ "${STARRY_SKIP_INSTALL:-0}" == "1" ]]; then
  echo "skip install (STARRY_SKIP_INSTALL=1)"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_dwct_install.apk"
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
# monkey 在部分机型返回非 0，勿令整脚本失败
set +e
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
set -e
echo "done → $SIGNED"
echo "log: adb logcat -s StarryMagic Dwct Mod_Menu MenuService"
exit 0
