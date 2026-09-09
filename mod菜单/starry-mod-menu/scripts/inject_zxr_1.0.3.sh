#!/usr/bin/env bash
# [DEPRECATED 2026-08-25] 本脚本对游戏包执行 apktool b，已违反仓规。
# 请改用：scripts/surgical_zxr_1.0.3.sh（zip 级换新版原生菜单，保留百度 Stub）。
#
# 星光魔法 → 镇邪人 1.0.3 原包：构建 + 注入 + 签名 + 可选安装（历史脚本，勿再跑）
#
# 注意：Gradle/apktool/aapt2 在含中文的路径下会把目录弄成 ?? 并失败。
# 本脚本把 native/gradle/apktool 都放到 /tmp ASCII 路径完成，再拷回 08_output。
#
# 用法（已废弃）:
#   bash scripts/surgical_zxr_1.0.3.sh
set -euo pipefail
echo "ERROR: inject_zxr_1.0.3.sh 已废弃（apktool b 整包）。请用: bash scripts/surgical_zxr_1.0.3.sh" >&2
exit 1
# --- dead code below (kept for history) ---
false && {
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
SRC_DECODE="$NIXIANG/03_decompiled/镇邪人/apktool_1.0.3_orig"
FINAL_PATCH="$NIXIANG/07_patched/镇邪人/apktool_1.0.3_modmenu"
OUT="$NIXIANG/08_output/镇邪人"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
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

PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"
SIGNED="$OUT/镇邪人_1.0.3_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/镇邪人_1.0.3_starry-debugSigned.apk"

WORK="$(mktemp -d /tmp/zxr_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$WORK/apktool_modmenu"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/zxr_starry_unsigned.apk"
trap 'rm -rf "$WORK"' EXIT

[[ -d "$SRC_DECODE" ]] || { echo "ERROR: missing decode $SRC_DECODE"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
mkdir -p "$OUT" "$OUT_ASCII"

echo "[0/7] rsync starry → ASCII work + copy decode"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$STARRY_BUILD/"
cp -a "$SRC_DECODE" "$PATCH"
# 复用上次已打 TapSign 的嵌套插件，避免每次 apktool 63MB
shopt -s nullglob
for prev in "$FINAL_PATCH"/assets/sq_plugin_*.apk; do
  if grep -a -q 'starry_sign_bypass' "$prev" 2>/dev/null || grep -a -q '8fa00ae1397609e5e09ff34cc43030e3' "$prev" 2>/dev/null; then
    cp -f "$prev" "$PATCH/assets/$(basename "$prev")"
    echo "[0/7] reuse patched plugin $(basename "$prev")"
    break
  fi
done
shopt -u nullglob

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
rm -rf "$PATCH/smali_classes8/com/starry"
mkdir -p "$PATCH/smali_classes8/com"
[[ -d "$WORK/starry_dec/smali/com/starry" ]] && cp -R "$WORK/starry_dec/smali/com/starry" "$PATCH/smali_classes8/com/"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali_classes*/com/starry; do
  cp -R "$d" "$PATCH/smali_classes8/com/"
done
[[ -d "$PATCH/smali_classes8/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }

echo "[4/7] patch Manifest / JMYBTActivity / SdkShowAdReward / TapSign(sq_plugin)"
python3.11 <<PY
from pathlib import Path
import re

PATCH = Path(r"$PATCH")
# 37WAN 原包 cert（META-INF/37WAN.RSA → MD5(cert DER) / SHA1）
ORIG_MD5 = "8fa00ae1397609e5e09ff34cc43030e3"
ORIG_SHA1 = "B79FCF14AFF40F25C876BD327CCB3766B3E7E1EC"
PATCHED_SIG = []

# --- Manifest: 保留百度 StubApplication，仅加权限 + MenuService ---
manifest = PATCH / "AndroidManifest.xml"
t = manifest.read_text(encoding="utf-8")
if "com.sagittarius.v6.StubApplication" not in t:
    print("WARN: StubApplication missing — 百度壳可能被改动")
if "com.starry.magic.StarryApplication" in t:
    raise SystemExit("ERROR: 勿替换 Application 为 StarryApplication（需保留百度 Stub）")
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

# --- Application.attachBaseContext：尽早 loadLibrary（抢 .jsc eval）---
app_smali = PATCH / "smali_classes7/com/sanqi/jmybt/game37/JMYBTApplication.smali"
if app_smali.is_file():
    apps = app_smali.read_text(encoding="utf-8")
    if "starry_magic" in apps and "attachBaseContext" in apps:
        print("hook: JMYBTApplication.attachBaseContext ok")
    else:
        apps += """
.method protected attachBaseContext(Landroid/content/Context;)V
    .locals 1
    .param p1, "base"    # Landroid/content/Context;

    invoke-super {p0, p1}, Lcom/sqwan/msdk/SQApplication;->attachBaseContext(Landroid/content/Context;)V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    return-void
.end method
"""
        app_smali.write_text(apps, encoding="utf-8")
        print("hook: JMYBTApplication.attachBaseContext → loadLibrary")
else:
    print("hook: WARN JMYBTApplication.smali missing")

act = PATCH / "smali_classes7/com/sanqi/jmybt/game37/JMYBTActivity.smali"
s = act.read_text(encoding="utf-8")

# --- onCreate：loadLibrary 移到 super 之前 + MenuService ---
if "MenuService;->start" not in s:
    # 在 invoke-super onCreate 前插入 loadLibrary
    super_on = """    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"""
    early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"""
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
    print("hook: JMYBTActivity.onCreate → early loadLibrary + MenuService")
elif "starry_magic" in s and "MenuService;->start" in s:
    # 已有旧版（尾部 loadLibrary）：尽量挪到 super 前
    if "invoke-static {v0}, Ljava/lang/System;->loadLibrary" in s.split("invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate")[0]:
        print("hook: onCreate already early loadLibrary")
    else:
        # 去掉尾部重复 loadLibrary，改插到 super 前
        s = s.replace(
            """
    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
""",
            """
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
""",
            1,
        )
        super_on = """    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"""
        early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"""
        if super_on in s:
            s = s.replace(super_on, early_load, 1)
            print("hook: moved loadLibrary before Cocos2dxActivity.onCreate")
        else:
            print("hook: onCreate already has MenuService (load order unchanged)")
else:
    print("hook: onCreate already has MenuService")

# --- onResume → MenuService.notifyHostResumed ---
if "MenuService;->notifyHostResumed" not in s:
    s2, n = re.subn(
        r"(\.method protected onResume\(\)V[\s\S]*?)(    :cond_0\n    return-void\n\.end method)",
        r"\1    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n\2",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("JMYBTActivity.onResume patch failed")
    s = s2
    print("hook: JMYBTActivity.onResume → notifyHostResumed")
else:
    print("hook: onResume already patched")

# --- onPause → MenuService.notifyHostLeaving ---
if "MenuService;->notifyHostLeaving" not in s:
    s2, n = re.subn(
        r"(\.method protected onPause\(\)V[\s\S]*?)(    :cond_0\n    return-void\n\.end method)",
        r"\1    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n\2",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("JMYBTActivity.onPause patch failed")
    s = s2
    print("hook: JMYBTActivity.onPause → notifyHostLeaving")
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
        raise SystemExit("onPause marker missing for onUserLeaveHint insert")
    s = s.replace(marker, leave + marker, 1)
    print("hook: JMYBTActivity.onUserLeaveHint → notifyHostLeaving")

# --- JMYBTActivity.get_ret / set_ret（对齐斌哥 JS jsb.reflection）---
if "ZxrHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/ZxrHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/ZxrHooks;->setRet(IZ)V

    return-void
.end method
"""
    print("hook: JMYBTActivity.get_ret/set_ret → ZxrHooks")
else:
    print("hook: get_ret already present")

act.write_text(s, encoding="utf-8")

# --- SdkManager.SdkShowAdReward：免广告直接 success（对齐动物餐厅 showRewardAd）---
sdk = PATCH / "smali_classes7/com/sanqi/jmybt/game37/sdk/SdkManager.smali"
if sdk.is_file():
    ss = sdk.read_text(encoding="utf-8")
    if "ZxrHooks;->shouldSkipAd" in ss:
        print("ad: SdkShowAdReward ok")
    else:
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

    invoke-static {}, Lcom/starry/magic/cocos/ZxrHooks;->shouldSkipAd()Z

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
    const-string v0, "[SdkManager](SdkShowAdReward)\\u8c03\\u7528\\u5c55\\u793a\\u6fc0\\u52b1\\u5e7f\\u544a"

    invoke-static {v0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->logi(Ljava/lang/String;)V

    invoke-static {p1}, Lcom/alibaba/fastjson/JSONObject;->parseObject(Ljava/lang/String;)Lcom/alibaba/fastjson/JSONObject;

    move-result-object p1

    const-string v0, "content"

    invoke-virtual {p1, v0}, Lcom/alibaba/fastjson/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object p1

    invoke-static {}, Lcom/sqwan/msdk/SQwanCore;->getInstance()Lcom/sqwan/msdk/SQwanCore;

    move-result-object v0

    iget-object v1, p0, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->mContext:Landroid/content/Context;

    new-instance v2, Lcom/sanqi/jmybt/game37/sdk/SdkManager\$13;

    invoke-direct {v2, p0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager\$13;-><init>(Lcom/sanqi/jmybt/game37/sdk/SdkManager;)V

    invoke-virtual {v0, v1, p1, v2}, Lcom/sqwan/msdk/SQwanCore;->showAdReward(Landroid/content/Context;Ljava/lang/String;Lcom/sqwan/msdk/api/SQResultListener;)V

    return-void
.end method"""
        # 原文用真实中文 unicode，上面 \\u 会被写成字面反斜杠；改用原文常量
        ad_new = ad_new.replace(
            'const-string v0, "[SdkManager](SdkShowAdReward)\\u8c03\\u7528\\u5c55\\u793a\\u6fc0\\u52b1\\u5e7f\\u544a"',
            'const-string v0, "[SdkManager](SdkShowAdReward)\u8c03\u7528\u5c55\u793a\u6fc0\u52b1\u5e7f\u544a"',
        )
        s2, n = re.subn(
            r"\.method public SdkShowAdReward\(Ljava/lang/String;\)V[\s\S]*?\.end method",
            ad_new,
            ss,
            count=1,
        )
        if n != 1:
            raise SystemExit("SdkShowAdReward patch failed")
        sdk.write_text(s2, encoding="utf-8")
        print("ad: SdkShowAdReward → ZxrHooks.shouldSkipAd")
else:
    print("ad: skip SdkManager (missing)")

def patch_getsign_smali(path: Path, method_pattern: str, new_method: str, label: str):
    if not path.is_file():
        print(f"sign: skip {label} (missing {path.relative_to(PATCH)})")
        return
    s = path.read_text(encoding="utf-8")
    if ORIG_MD5 in s and "starry_sign_bypass" in s:
        print(f"sign: {label} ok")
        PATCHED_SIG.append(str(path.relative_to(PATCH)))
        return
    s2, n = re.subn(method_pattern, new_method, s, count=1)
    if n != 1:
        raise SystemExit(f"{label} patch failed ({path})")
    path.write_text(s2, encoding="utf-8")
    PATCHED_SIG.append(str(path.relative_to(PATCH)))
    print(f"sign: {label} fixed MD5")

TAP_GETSIGN = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass (动物餐厅同款：固定原版 cert MD5)
    const-string v0, "{ORIG_MD5}"

    return-object v0
.end method"""

# --- TapSignKit：主机 smali（通常没有）+ 嵌套 sq_plugin（真正的 TapTap 渠道校验）---
for tap in PATCH.glob("smali*/com/taptap/sdk/kit/internal/utils/TapSignKit.smali"):
    patch_getsign_smali(
        tap,
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        TAP_GETSIGN,
        "TapSignKit.getSign(host)",
    )

import shutil
import subprocess
import tempfile
import zipfile

plugin_paths = sorted(PATCH.glob("assets/sq_plugin_*.apk"))
if not plugin_paths:
    print("sign: WARN no assets/sq_plugin_*.apk — TapTap 校验可能仍弹窗")
elif ORIG_MD5.encode() in plugin_paths[0].read_bytes() and b"starry_sign_bypass" in plugin_paths[0].read_bytes():
    print(f"sign: plugin already has ORIG_MD5 bypass, skip apktool ({plugin_paths[0].name})")
    PATCHED_SIG.append(f"plugin:{plugin_paths[0].name}:cached")
else:
    plugin = plugin_paths[0]
    work = Path(tempfile.mkdtemp(prefix="zxr_plugin_"))
    try:
        plug_dec = work / "dec"
        plug_out = work / "out.apk"
        # apktool 拆插件（含 classes4 TapSignKit）
        r = subprocess.run(
            ["apktool", "d", "-f", "-o", str(plug_dec), str(plugin)],
            capture_output=True, text=True,
        )
        if r.returncode != 0:
            print("sign: apktool d plugin FAILED:", (r.stderr or r.stdout)[-400:])
            raise SystemExit("sq_plugin decode failed")
        taps = list(plug_dec.glob("smali*/com/taptap/sdk/kit/internal/utils/TapSignKit.smali"))
        if not taps:
            # 有的版本 dex 未拆成 smali 名；尝试全树搜索
            taps = list(plug_dec.rglob("TapSignKit.smali"))
        if not taps:
            print("sign: WARN TapSignKit.smali not in plugin after apktool d")
        else:
            for tap in taps:
                ts = tap.read_text(encoding="utf-8")
                if ORIG_MD5 in ts and "starry_sign_bypass" in ts:
                    print(f"sign: plugin TapSignKit ok ({tap.relative_to(plug_dec)})")
                    continue
                ts2, n = re.subn(
                    r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
                    TAP_GETSIGN,
                    ts,
                    count=1,
                )
                if n != 1:
                    # Kotlin 可能是 getSign\$... 或不同访问标志
                    ts2, n = re.subn(
                        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
                        TAP_GETSIGN,
                        ts,
                        count=1,
                        flags=re.M,
                    )
                if n != 1:
                    raise SystemExit(f"plugin TapSignKit.getSign patch failed ({tap})")
                tap.write_text(ts2, encoding="utf-8")
                PATCHED_SIG.append(f"plugin:{tap.relative_to(plug_dec)}")
                print(f"sign: plugin TapSignKit.getSign fixed ({tap.relative_to(plug_dec)})")
        r = subprocess.run(
            ["apktool", "b", "-f", str(plug_dec), "-o", str(plug_out)],
            capture_output=True, text=True,
        )
        if r.returncode != 0:
            print("sign: apktool b plugin FAILED:", (r.stderr or r.stdout)[-400:])
            raise SystemExit("sq_plugin rebuild failed")
        # 保留原插件压缩方式：用 zip 替换 classes*.dex 更稳；此处整包替换
        shutil.copy2(plug_out, plugin)
        print(f"sign: replaced {plugin.name} ({plugin.stat().st_size} bytes)")
    finally:
        shutil.rmtree(work, ignore_errors=True)

# --- Nirvana AppUtils.getSign → 原版 MD5 ---
app_utils = PATCH / "smali_classes2/com/nirvana/tools/core/AppUtils.smali"
patch_getsign_smali(
    app_utils,
    r"\.method public static declared-synchronized getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
    f""".method public static declared-synchronized getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1

    # starry_sign_bypass
    const-string v0, "{ORIG_MD5}"

    return-object v0
.end method""",
    "AppUtils.getSign",
)

# --- Ali 一键登录 PackageUtils.getSign（native → smali stub）---
pkg_utils = PATCH / "smali_classes2/com/mobile/auth/gatewayauth/utils/security/PackageUtils.smali"
if pkg_utils.is_file():
    s = pkg_utils.read_text(encoding="utf-8")
    if ORIG_MD5 in s and "starry_sign_bypass" in s:
        print("sign: PackageUtils.getSign ok")
        PATCHED_SIG.append(str(pkg_utils.relative_to(PATCH)))
    else:
        s2, n = re.subn(
            r"\.method public static synchronized native getSign\(Landroid/content/Context;\)Ljava/lang/String;\s*\n\.end method",
            f""".method public static synchronized getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1

    # starry_sign_bypass (was native)
    const-string v0, "{ORIG_MD5}"

    return-object v0
.end method""",
            s,
            count=1,
        )
        if n != 1:
            raise SystemExit("PackageUtils.getSign patch failed")
        pkg_utils.write_text(s2, encoding="utf-8")
        PATCHED_SIG.append(str(pkg_utils.relative_to(PATCH)))
        print("sign: PackageUtils.getSign fixed MD5 (native stub)")
else:
    print("sign: skip PackageUtils.getSign (missing)")

print(f"sign: ORIG_MD5={ORIG_MD5.upper()} ORIG_SHA1={ORIG_SHA1}")
print(f"sign: patched {len(PATCHED_SIG)} → {', '.join(PATCHED_SIG) or '(none)'}")
print("hooks done")
PY

echo "[5/7] (combat = evalString splice in ZxrMod; TapSign in sq_plugin)"
echo "[6/7] apktool rebuild (ASCII)"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + optional install"
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

# 原生 UI：不再校验 APK 内 mod-menu-v2.html

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (set STARRY_INSTALL=1 to adb install — 勿覆盖用户官方原包)"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_zxr_install.apk"
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
echo "log: adb logcat -s StarryMagic Zxr Mod_Menu MenuService"
} # end false && { historical body}
