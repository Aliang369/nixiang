#!/usr/bin/env bash
# 星光魔法 → 动物餐厅：还原免广告/免内购烘焙补丁 + 注入菜单 + 重打包签名安装
set -euo pipefail

STARRY="$(cd "$(dirname "$0")/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
PATCH="$NIXIANG/07_patched/动物餐厅/apktool_12.35_modmenu"
OUT="$NIXIANG/08_output/动物餐厅"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK_CXX="$HOME/Library/Android/sdk/ndk/27.0.12077973/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
JAVA_HOME_DEFAULT="/Users/aliang/Library/Java/JavaVirtualMachines/jdk-17.0.2.jdk/Contents/Home"
export JAVA_HOME="${JAVA_HOME:-$JAVA_HOME_DEFAULT}"
export PATH="$JAVA_HOME/bin:$PATH"

PKG="inland.twgame.restaurant"
UNSIGNED="$OUT/动物餐厅_12.35_starry_unsigned.apk"
TMP="$(mktemp -d /tmp/starry_dwct_inject.XXXXXX)"
trap 'rm -rf "$TMP"' EXIT

if [[ ! -d "$PATCH" ]]; then
  echo "ERROR: missing apktool tree $PATCH"
  exit 1
fi
mkdir -p "$OUT"

echo "[1/7] unpatch baked 免广告/免内购 in jsc"
node "$STARRY/scripts/unpatch_dwct_jsc.js" "$PATCH/assets/assets"

echo "[2/7] build starry debug apk"
(cd "$STARRY" && ./gradlew assembleDebug --console=plain -q)
APK="$STARRY/app/build/outputs/apk/debug/app-debug.apk"
[[ -f "$APK" ]] || { echo "ERROR: missing $APK"; exit 1; }

echo "[3/7] decode starry apk"
apktool d -f -o "$TMP/starry_dec" "$APK" >/dev/null

echo "[4/7] inject so / assets / smali"
mkdir -p "$PATCH/lib/arm64-v8a"
cp "$TMP/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$PATCH/lib/arm64-v8a/libstarry_magic.so"
if [[ -f "$NDK_CXX" ]]; then
  cp "$NDK_CXX" "$PATCH/lib/arm64-v8a/libc++_shared.so"
elif [[ -f "$TMP/starry_dec/lib/arm64-v8a/libc++_shared.so" ]]; then
  cp "$TMP/starry_dec/lib/arm64-v8a/libc++_shared.so" "$PATCH/lib/arm64-v8a/libc++_shared.so"
fi

# 原生菜单：不再拷贝 assets/web；清掉旧包残留 HTML
rm -rf "$PATCH/assets/web" 2>/dev/null || true
if [[ -d "$TMP/starry_dec/assets/certs" ]]; then
  mkdir -p "$PATCH/assets/certs"
  cp -R "$TMP/starry_dec/assets/certs/." "$PATCH/assets/certs/"
fi

rm -rf "$PATCH/smali_classes4/com/starry"
mkdir -p "$PATCH/smali_classes4/com"
if [[ -d "$TMP/starry_dec/smali/com/starry" ]]; then
  cp -R "$TMP/starry_dec/smali/com/starry" "$PATCH/smali_classes4/com/"
fi
for d in "$TMP/starry_dec"/smali_classes*/com/starry; do
  [[ -d "$d" ]] || continue
  mkdir -p "$PATCH/smali_classes4/com"
  cp -R "$d" "$PATCH/smali_classes4/com/"
done
[[ -d "$PATCH/smali_classes4/com/starry/magic" ]] || {
  echo "ERROR: starry smali missing"; find "$TMP/starry_dec" -type d -name starry; exit 1;
}

echo "[5/7] Manifest + AppActivity hook"
python3 - <<'PY'
from pathlib import Path
manifest = Path("/Users/aliang/Documents/nixiang/07_patched/动物餐厅/apktool_12.35_modmenu/AndroidManifest.xml")
t = manifest.read_text(encoding="utf-8")

perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "FOREGROUND_SERVICE\"" not in t and "FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    # 插在第一个 uses-permission 前
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
    print("manifest: permissions added")
else:
    print("manifest: permissions ok")

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
    print("manifest: MenuService added")
else:
    print("manifest: MenuService ok")

manifest.write_text(t, encoding="utf-8")

act = Path("/Users/aliang/Documents/nixiang/07_patched/动物餐厅/apktool_12.35_modmenu/smali_classes3/org/cocos2dx/javascript/AppActivity.smali")
s = act.read_text(encoding="utf-8")
if "MenuService;->start" in s:
    print("hook: AppActivity already patched")
else:
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
    new = """.method protected onCreate(Landroid/os/Bundle;)V
    .locals 1

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

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void
.end method"""
    if old not in s:
        raise SystemExit("ERROR: AppActivity.onCreate pattern not found")
    act.write_text(s.replace(old, new, 1), encoding="utf-8")
    print("hook: AppActivity.onCreate → MenuService.start")
PY

echo "[6/7] apktool rebuild"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + install"
java -jar "$SIGNER" -a "$UNSIGNED" --overwrite >/dev/null

# uber-apk-signer 默认输出 *-aligned-debugSigned.apk
SIGNED="$(ls -1 "$OUT"/动物餐厅_12.35_starry_unsigned-aligned-debugSigned.apk 2>/dev/null || true)"
if [[ -z "$SIGNED" ]]; then
  SIGNED="$(ls -1t "$OUT"/动物餐厅_12.35_starry*.apk | head -1)"
fi
echo "signed: $SIGNED"
ls -lh "$SIGNED"

adb uninstall "$PKG" 2>/dev/null || true
adb install --no-incremental "$SIGNED"
# 全游戏卡密种子 + 悬浮窗（勿删；换游戏 403 多因此）
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
  echo "WARN: 请手动开启悬浮窗权限"

adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done. package=$PKG"
echo "log: adb logcat -s StarryMagic Dwct CocosJsBridge MenuService"
