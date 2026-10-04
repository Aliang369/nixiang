#!/usr/bin/env bash
# 星光魔法 → 战魂铭人：构建 + 注入改包树 + 重打包 + 签名
# 用法:
#   bash scripts/inject_zhmr.sh                           # 默认 release（R8，上线）
#   STARRY_BUILD_TYPE=debug bash scripts/inject_zhmr.sh   # 仅联调
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
ROOT="$(cd "$STARRY/../../.." && pwd 2>/dev/null || true)"
# 若相对路径不对，用绝对锚定
if [[ ! -d "${NIXIANG}/07_patched/战魂铭人/apktool_3.4.0_modmenu" ]]; then
  echo "ERROR: 找不到改包工作树 07_patched/战魂铭人/apktool_3.4.0_modmenu"
  exit 1
fi
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
PATCH="$NIXIANG/07_patched/战魂铭人/apktool_3.4.0_modmenu"
OUT="$NIXIANG/08_output/战魂铭人"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK_CXX="$HOME/Library/Android/sdk/ndk/27.0.12077973/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
JAVA_HOME_DEFAULT="${JAVA_HOME_DEFAULT:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
export JAVA_HOME="${JAVA_HOME:-$JAVA_HOME_DEFAULT}"

TMP="$(mktemp -d /tmp/starry_zhmr_inject.XXXXXX)"
trap 'rm -rf "$TMP"' EXIT

mkdir -p "$OUT"
UNSIGNED="$OUT/战魂铭人_3.4.0_starry_unsigned.apk"
SIGNED_HINT="$OUT/战魂铭人_3.4.0_starry_unsigned-aligned-debugSigned.apk"

echo "[1/6] build starry $BUILD_TYPE apk via $GRADLE_TASK (R8 on if release)"
(cd "$STARRY" && ./gradlew "$GRADLE_TASK" --console=plain)

APK="$STARRY/app/build/outputs/apk/$BUILD_TYPE/app-$BUILD_TYPE.apk"
if [[ ! -f "$APK" ]]; then
  echo "ERROR: missing $APK"
  exit 1
fi
echo "using: $APK"

echo "[2/6] decode starry apk → extract so / assets / smali"
apktool d -f -o "$TMP/starry_dec" "$APK" >/dev/null

echo "[3/6] inject into patched tree"
# native
mkdir -p "$PATCH/lib/arm64-v8a"
cp "$TMP/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$PATCH/lib/arm64-v8a/libstarry_magic.so"
if [[ -f "$NDK_CXX" ]]; then
  cp "$NDK_CXX" "$PATCH/lib/arm64-v8a/libc++_shared.so"
elif [[ -f "$TMP/starry_dec/lib/arm64-v8a/libc++_shared.so" ]]; then
  cp "$TMP/starry_dec/lib/arm64-v8a/libc++_shared.so" "$PATCH/lib/arm64-v8a/libc++_shared.so"
else
  echo "WARN: libc++_shared.so not found — 设备上可能加载失败"
fi
# 旧 LGL so 可留着但不启动；删除避免误加载体积
rm -f "$PATCH/lib/arm64-v8a/libjzgame.so"

# web assets + auth cert (HTTPS 证书固定依赖 assets/certs)
# 原生菜单：不再拷贝 assets/web；清掉旧包残留 HTML
rm -rf "$PATCH/assets/web" 2>/dev/null || true
if [[ -d "$TMP/starry_dec/assets/certs" ]]; then
  mkdir -p "$PATCH/assets/certs"
  cp -R "$TMP/starry_dec/assets/certs/." "$PATCH/assets/certs/"
  echo "assets: certs injected"
else
  echo "WARN: assets/certs missing in starry apk — HTTPS 验证会失败"
fi

# smali → 新 dex 目录，避免 classes4 与 LGL 混在一起
rm -rf "$PATCH/smali_classes5/com/starry"
mkdir -p "$PATCH/smali_classes5/com"
# apktool 可能把类放在 smali 或 smali_classesN
if [[ -d "$TMP/starry_dec/smali/com/starry" ]]; then
  cp -R "$TMP/starry_dec/smali/com/starry" "$PATCH/smali_classes5/com/"
fi
for d in "$TMP/starry_dec"/smali_classes*/com/starry; do
  [[ -d "$d" ]] || continue
  mkdir -p "$PATCH/smali_classes5/com"
  cp -R "$d" "$PATCH/smali_classes5/com/"
done
if [[ ! -d "$PATCH/smali_classes5/com/starry/magic" ]]; then
  echo "ERROR: starry smali not found in decoded apk"
  find "$TMP/starry_dec" -type d -name 'starry' 2>/dev/null | head
  exit 1
fi

echo "[4/6] patch Manifest + privacy Activity hook"
MANIFEST="$PATCH/AndroidManifest.xml"
# 权限
if ! grep -q 'FOREGROUND_SERVICE_SPECIAL_USE' "$MANIFEST"; then
  # 插在 SYSTEM_ALERT_WINDOW 后
  if grep -q 'SYSTEM_ALERT_WINDOW' "$MANIFEST"; then
    python3 - <<'PY'
from pathlib import Path
p = Path("${NIXIANG}/07_patched/战魂铭人/apktool_3.4.0_modmenu/AndroidManifest.xml")
t = p.read_text(encoding="utf-8")
needle = '<uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>'
extra = needle + '''
    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>
    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>'''
if needle in t and "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    t = t.replace(needle, extra, 1)
    p.write_text(t, encoding="utf-8")
    print("manifest: added FGS permissions")
else:
    print("manifest: FGS perms skip")
PY
  fi
fi
# MenuService
if ! grep -q 'com.starry.magic.menu.MenuService' "$MANIFEST"; then
  python3 - <<'PY'
from pathlib import Path
p = Path("${NIXIANG}/07_patched/战魂铭人/apktool_3.4.0_modmenu/AndroidManifest.xml")
t = p.read_text(encoding="utf-8")
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
# 插在旧 Launcher service 之前或 </application> 前
marker = 'android:name="com.android.support.Launcher"'
if marker in t:
    # 在 <service ... Launcher 整块前插入
    idx = t.find("<service")
    # find Launcher service block start
    i = t.find('android:name="com.android.support.Launcher"')
    start = t.rfind("<service", 0, i)
    t = t[:start] + svc + t[start:]
else:
    t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("manifest: MenuService added")
PY
fi

# 替换隐私页启动 LGL → starry
PRIV="$PATCH/smali/com/chillyroomsdk/sdkbridge/privacy/BasePrivacyActivity.smali"
if [[ -f "$PRIV" ]]; then
  python3 - <<'PY'
from pathlib import Path
p = Path("${NIXIANG}/07_patched/战魂铭人/apktool_3.4.0_modmenu/smali/com/chillyroomsdk/sdkbridge/privacy/BasePrivacyActivity.smali")
t = p.read_text(encoding="utf-8")
old = "    invoke-static {p0}, Lcom/android/support/Main;->Start(Landroid/content/Context;)V"
new = """    const-string v0, \"starry_magic\"
    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V"""
if old in t:
    t = t.replace(old, new, 1)
    p.write_text(t, encoding="utf-8")
    print("hook: BasePrivacyActivity → MenuService.start")
elif "MenuService;->start" in t:
    print("hook: BasePrivacyActivity already patched")
else:
    print("WARN: Start() call not found in BasePrivacyActivity.smali")
PY
else
  echo "WARN: BasePrivacyActivity.smali missing"
fi

# 主游戏 Activity 也启动（跳过隐私页时仍能出菜单）
PLAYER="$PATCH/smali/com/chillyroomsdk/sdkbridge/BasePlayerActivity.smali"
if [[ -f "$PLAYER" ]]; then
  python3 - <<'PY'
from pathlib import Path
p = Path("${NIXIANG}/07_patched/战魂铭人/apktool_3.4.0_modmenu/smali/com/chillyroomsdk/sdkbridge/BasePlayerActivity.smali")
t = p.read_text(encoding="utf-8")
if "MenuService;->start" in t:
    print("hook: BasePlayerActivity already patched")
else:
    needle = "    invoke-direct {p0}, Lcom/chillyroomsdk/sdkbridge/BasePlayerActivity;->setAndroidIDByFirst()V\n\n    return-void"
    insert = """    invoke-direct {p0}, Lcom/chillyroomsdk/sdkbridge/BasePlayerActivity;->setAndroidIDByFirst()V

    const-string v0, \"starry_magic\"
    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void"""
    if needle in t:
        t = t.replace(needle, insert, 1)
        p.write_text(t, encoding="utf-8")
        print("hook: BasePlayerActivity → MenuService.start")
    else:
        print("WARN: BasePlayerActivity insert point not found")

# onUserLeaveHint：上划/Home/进浮窗前尽早藏全屏 overlay，避免 ColorOS 警告
t = p.read_text(encoding="utf-8")
if "MenuService;->notifyHostLeaving" not in t:
    leave = """.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method protected onPause()V"
    if marker in t:
        t = t.replace(marker, leave + marker, 1)
        p.write_text(t, encoding="utf-8")
        print("hook: BasePlayerActivity.onUserLeaveHint → MenuService.notifyHostLeaving")
    else:
        print("WARN: onPause marker missing for onUserLeaveHint")
else:
    print("hook: onUserLeaveHint already present")
PY
fi

echo "[5/6] apktool rebuild"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[6/6] sign + root silent install"
java -jar "$SIGNER" -a "$UNSIGNED" --overwrite >/dev/null
PKG="com.chillyroom.zhmr.yw"
SIGNED="$OUT/战魂铭人_3.4.0_starry-debugSigned.apk"
# uber 可能写出 *-aligned-debugSigned.apk，统一拷一份固定名
ALIGNED="$OUT/战魂铭人_3.4.0_starry_unsigned-aligned-debugSigned.apk"
if [[ -f "$ALIGNED" ]]; then
  cp -f "$ALIGNED" "$SIGNED"
elif [[ -f "$UNSIGNED" ]]; then
  cp -f "$UNSIGNED" "$SIGNED"
fi
ls -lh "$SIGNED" 2>/dev/null || ls -lh "$OUT"/战魂铭人_3.4.0_starry* 2>/dev/null || true

REMOTE_APK="/data/local/tmp/starry_zhmr_install.apk"
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
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done"
echo "log: adb logcat -s StarryMagic Zhmr Mod_Menu MenuService"
