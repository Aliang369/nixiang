#!/usr/bin/env bash
# 动物餐厅 12.38 — 外科组装（官方 classes.dex / resources.arsc 字节不变）
#
# 改动：
#   - 新增 classes4.dex（星光 smali）
#   - 改 classes3.dex（AppActivity / DeviceHelper / TapSignKit / 广告 / wxPay）
#   - 改 AndroidManifest（StarryApp + MenuService + 悬浮窗权限）
#   - 加 libstarry_magic.so / libc++_shared.so / assets/certs
#   - 改 assets/assets/Script/index.jsc（__starryNoIap 门控）
#
# 用法:
#   bash scripts/surgical_dwct_12.38.sh
#   STARRY_INSTALL=1 bash scripts/surgical_dwct_12.38.sh
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
ORIG_APK="$NIXIANG/02_apks/动物餐厅/动物餐厅_12.38_official.apk"
OUT="$NIXIANG/08_output/动物餐厅"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="inland.twgame.restaurant"
SIGNED="$OUT/动物餐厅_12.38_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/动物餐厅_12.38_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/动物餐厅/surgical_12.38/AndroidManifest.xml"
ORIG_SHA1="94F80FC5161E41BAD58C6541E739920425BAE53D"
ORIG_MD5="3169d253f38147f0f7cb6ba1672e6255"

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
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_DWCT' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_DWCT"; exit 1; }
echo "[check] Android.mk target=dwct"

LOCK_DIR="/tmp/dwct_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/dwct_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] gradle $GRADLE_TASK + ndk (starry) — clean rebuild (防残留 hzsj so)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew clean "$GRADLE_TASK" --console=plain --rerun-tasks)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
# 切游戏硬校验：so 必须带 dwct，禁止再打出 hzsj 鉴权 id
SO_CHK="$(find "$WORK/starry/app/build" -name libstarry_magic.so -path '*/arm64-v8a/*' | head -1)"
[[ -f "$SO_CHK" ]] || SO_CHK="$(find "$WORK/starry/app/build" -name libstarry_magic.so | head -1)"
[[ -f "$SO_CHK" ]] || { echo "ERROR: libstarry_magic.so not found after build"; exit 1; }
if ! strings -a "$SO_CHK" | tr '\0' '\n' | grep -qx 'dwct'; then
  echo "ERROR: so missing standalone game_id dwct — Android.mk 未真正切到 DWCT？"; exit 1
fi
if strings -a "$SO_CHK" | tr '\0' '\n' | grep -qx 'hzsj'; then
  echo "ERROR: so still contains standalone hzsj — NDK 缓存污染，拒绝出包"; exit 1
fi
echo "[check] so game_id=dwct ok ($SO_CHK)"

echo "[2/7] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] collect com/starry smali → classes4.dex"
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
# 官方无自定义 Application，保持继承 Landroid/app/Application;
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

echo "[4/7] patch classes3: AppActivity / DeviceHelper / TapSignKit / ad / wxPay"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
python3.11 - "$WORK/c3smali" "$ORIG_SHA1" "$ORIG_MD5" <<'PY'
from pathlib import Path
import re, sys

root = Path(sys.argv[1])
sha1, md5 = sys.argv[2], sys.argv[3]
act = root / "org/cocos2dx/javascript/AppActivity.smali"
dh = root / "org/cocos2dx/javascript/util/DeviceHelper.smali"
tap = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
for p in (act, dh, tap):
    if not p.is_file():
        raise SystemExit(f"missing {p}")

# --- DeviceHelper.getSignature ---
s = dh.read_text(encoding="utf-8")
if sha1 not in s:
    s2, n = re.subn(
        r"\.method public static getSignature\(Ljava/lang/String;\)Ljava/lang/String;[\s\S]*?\.end method",
        f""".method public static getSignature(Ljava/lang/String;)Ljava/lang/String;
    .registers 1

    const-string p0, "{sha1}"

    return-object p0
.end method""",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("DeviceHelper.getSignature patch failed")
    dh.write_text(s2, encoding="utf-8")
    print(f"  DeviceHelper.getSignature → {sha1}")
else:
    print("  DeviceHelper ok")

# --- TapSignKit.getSign ---
s = tap.read_text(encoding="utf-8")
if md5 not in s:
    s2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .registers 2
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    const-string p1, "{md5}"

    return-object p1
.end method""",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("TapSignKit.getSign patch failed")
    tap.write_text(s2, encoding="utf-8")
    print(f"  TapSignKit.getSign → {md5}")
else:
    print("  TapSignKit ok")

s = act.read_text(encoding="utf-8")

# --- isRewardVideoADReady ---
ready_new = """.method public static isRewardVideoADReady()Z
    .registers 1

    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->shouldSkipAd()Z

    move-result v0

    if-eqz v0, :cond_starry_ad

    const/4 v0, 0x1

    return v0

    :cond_starry_ad
    sget-boolean v0, Lorg/cocos2dx/javascript/tapAd/tapAdManager;->isInited:Z

    if-eqz v0, :cond_c

    invoke-static {}, Lorg/cocos2dx/javascript/tapAd/tapAdManager;->isVideoReady()Z

    move-result v0

    if-eqz v0, :cond_c

    const/4 v0, 0x1

    goto :goto_d

    :cond_c
    const/4 v0, 0x0

    :goto_d
    return v0
.end method"""
if "DwctHooks;->shouldSkipAd" not in s.split("isRewardVideoADReady")[1][:400]:
    s2, n = re.subn(
        r"\.method public static isRewardVideoADReady\(\)Z[\s\S]*?\.end method",
        ready_new,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("isRewardVideoADReady patch failed")
    s = s2
    print("  ad: isRewardVideoADReady hooked")
else:
    print("  ad: isRewardVideoADReady ok")

# --- showRewardAd ---
show_new = """.method public static showRewardAd()V
    .registers 2

    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->shouldSkipAd()Z

    move-result v0

    if-eqz v0, :cond_starry_show

    const-string v0, "reward"

    const/4 v1, 0x0

    invoke-static {v0, v1}, Lorg/cocos2dx/javascript/AppActivity;->callToJs(Ljava/lang/String;I)V

    return-void

    :cond_starry_show
    sget-object v0, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    new-instance v1, Lorg/cocos2dx/javascript/AppActivity$a;

    invoke-direct {v1}, Lorg/cocos2dx/javascript/AppActivity$a;-><init>()V

    invoke-virtual {v0, v1}, Landroid/app/Activity;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method"""
chunk = s.split("showRewardAd", 1)[1][:500] if "showRewardAd" in s else ""
if 'const-string v0, "reward"' not in chunk:
    s2, n = re.subn(
        r"\.method public static showRewardAd\(\)V[\s\S]*?\.end method",
        show_new,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("showRewardAd patch failed")
    s = s2
    print("  ad: showRewardAd hooked")
else:
    print("  ad: showRewardAd ok")

# --- wxPay Java fallback ---
wx_new = """.method public static wxPay(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .registers 7

    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->shouldSkipIap()Z

    move-result v0

    if-eqz v0, :cond_starry_wx

    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->fakeWxPaySuccess()V

    return-void

    :cond_starry_wx
    sget-object v0, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    invoke-virtual {v0}, Lorg/cocos2dx/javascript/AppActivity;->isWxHas()Z

    move-result v0

    if-nez v0, :cond_f

    const/4 p0, 0x5

    const-string p1, "false"

    invoke-static {p0, p1}, Linland/twgame/restaurant/wxapi/WXPayEntryActivity;->b(ILjava/lang/String;)V

    return-void

    :cond_f
    new-instance v0, Lcom/tencent/mm/opensdk/modelpay/PayReq;

    invoke-direct {v0}, Lcom/tencent/mm/opensdk/modelpay/PayReq;-><init>()V

    const-string v1, "wx4297cf03e4daa5fa"

    iput-object v1, v0, Lcom/tencent/mm/opensdk/modelpay/PayReq;->appId:Ljava/lang/String;

    const-string v1, "Sign=WXPay"

    iput-object v1, v0, Lcom/tencent/mm/opensdk/modelpay/PayReq;->packageValue:Ljava/lang/String;

    iput-object p0, v0, Lcom/tencent/mm/opensdk/modelpay/PayReq;->partnerId:Ljava/lang/String;

    iput-object p1, v0, Lcom/tencent/mm/opensdk/modelpay/PayReq;->prepayId:Ljava/lang/String;

    iput-object p2, v0, Lcom/tencent/mm/opensdk/modelpay/PayReq;->nonceStr:Ljava/lang/String;

    iput-object p3, v0, Lcom/tencent/mm/opensdk/modelpay/PayReq;->timeStamp:Ljava/lang/String;

    iput-object p4, v0, Lcom/tencent/mm/opensdk/modelpay/PayReq;->sign:Ljava/lang/String;

    sget-object p0, Lorg/cocos2dx/javascript/AppActivity;->wxapi:Lcom/tencent/mm/opensdk/openapi/IWXAPI;

    invoke-interface {p0, v0}, Lcom/tencent/mm/opensdk/openapi/IWXAPI;->sendReq(Lcom/tencent/mm/opensdk/modelbase/BaseReq;)Z

    return-void
.end method"""
if "shouldSkipIap" not in s:
    s2, n = re.subn(
        r"\.method public static wxPay\(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;\)V[\s\S]*?\.end method",
        wx_new,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("wxPay patch failed")
    s = s2
    print("  iap: wxPay hooked")
else:
    print("  iap: wxPay ok")

# --- onCreate: loadLibrary + MenuService（StarryApp 也会起菜单；双保险）---
if "MenuService;->start" not in s:
    old = """.method protected onCreate(Landroid/os/Bundle;)V
    .registers 2

    .line 1
    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V

    .line 2
    sput-object p0, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    .line 3
    sput-object p0, Lorg/cocos2dx/javascript/AppActivity;->sContext:Landroid/content/Context;

    .line 4
    invoke-virtual {p0}, Landroid/app/Activity;->isTaskRoot()Z

    move-result p1

    if-nez p1, :cond_e

    return-void

    .line 5
    :cond_e
    sget-object p1, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    invoke-static {p1}, Lorg/cocos2dx/javascript/JsbHelper;->Init(Lorg/cocos2dx/javascript/AppActivity;)V

    .line 6
    invoke-static {}, Lorg/cocos2dx/javascript/SDKWrapper;->getInstance()Lorg/cocos2dx/javascript/SDKWrapper;

    move-result-object p1

    invoke-virtual {p1, p0}, Lorg/cocos2dx/javascript/SDKWrapper;->init(Landroid/content/Context;)V

    return-void
.end method"""
    new = """.method protected onCreate(Landroid/os/Bundle;)V
    .registers 2

    .line 1
    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V

    .line 2
    sput-object p0, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    .line 3
    sput-object p0, Lorg/cocos2dx/javascript/AppActivity;->sContext:Landroid/content/Context;

    .line 4
    invoke-virtual {p0}, Landroid/app/Activity;->isTaskRoot()Z

    move-result p1

    if-nez p1, :cond_e

    return-void

    .line 5
    :cond_e
    sget-object p1, Lorg/cocos2dx/javascript/AppActivity;->app:Lorg/cocos2dx/javascript/AppActivity;

    invoke-static {p1}, Lorg/cocos2dx/javascript/JsbHelper;->Init(Lorg/cocos2dx/javascript/AppActivity;)V

    .line 6
    invoke-static {}, Lorg/cocos2dx/javascript/SDKWrapper;->getInstance()Lorg/cocos2dx/javascript/SDKWrapper;

    move-result-object p1

    invoke-virtual {p1, p0}, Lorg/cocos2dx/javascript/SDKWrapper;->init(Landroid/content/Context;)V

    const-string p1, "starry_magic"

    invoke-static {p1}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void
.end method"""
    if old not in s:
        raise SystemExit("onCreate pattern not found (label/registers changed?)")
    s = s.replace(old, new, 1)
    print("  hook: AppActivity.onCreate → loadLibrary + MenuService")
else:
    print("  hook: AppActivity already has MenuService")

# --- onUserLeaveHint ---
if "MenuService;->notifyHostLeaving" not in s:
    leave = """.method protected onUserLeaveHint()V
    .registers 1

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method protected onPause()V"
    if marker not in s:
        raise SystemExit("onPause marker missing for onUserLeaveHint insert")
    s = s.replace(marker, leave + marker, 1)
    print("  hook: onUserLeaveHint → notifyHostLeaving")
else:
    print("  hook: onUserLeaveHint ok")

# --- onResume notify ---
if "MenuService;->notifyHostResumed" not in s:
    resume = """.method protected onResume()V
    .registers 1

    invoke-super {p0}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onResume()V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V

    return-void
.end method

"""
    # append before last .end class if no onResume override, else leave alone
    if ".method protected onResume()V" not in s:
        s = s.replace(".end class", resume + ".end class", 1)
        print("  hook: +onResume → notifyHostResumed")
    else:
        print("  hook: onResume exists (skip append)")
else:
    print("  hook: onResume ok")

act.write_text(s, encoding="utf-8")
print("  AppActivity written")
PY
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"
ls -la "$WORK/classes3.dex"

echo "[5/7] patch Script/index.jsc (__starryNoIap)"
mkdir -p "$WORK/assets/assets/Script"
unzip -p "$ORIG_APK" "assets/assets/Script/index.jsc" > "$WORK/assets/assets/Script/index.jsc"
node "$STARRY_SRC/scripts/patch_dwct_iap_gate.js" "$WORK/assets/assets"
# 明文门控不会出现在加密 jsc；用解密校验
node -e '
const fs=require("fs");
const pako=require("/Users/aliang/Documents/nixiang/11_proj_tools/动物餐厅/cocos_jsc_decrypt/node_modules/pako");
const xxtea=require("/Users/aliang/Documents/nixiang/11_proj_tools/动物餐厅/cocos_jsc_decrypt/node_modules/xxtea-node");
const KEY="431FEWJOTe-gecTW";
const buf=fs.readFileSync(process.argv[1]);
const d=xxtea.decrypt(buf, xxtea.toBytes(KEY));
const js=Buffer.from(pako.ungzip(new Uint8Array(d))).toString("utf8");
const n=(js.match(/__starryNoIap/g)||[]).length;
if(n<10){console.error("iap gate verify fail, gates="+n); process.exit(1);}
console.log("  iap gate verified, gates="+n);
' "$WORK/assets/assets/Script/index.jsc"

echo "[6/7] binary AndroidManifest"
if [[ -n "${DWCT_BINARY_MANIFEST:-}" && -f "$DWCT_BINARY_MANIFEST" ]]; then
  cp "$DWCT_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
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
    t = t.replace("<application ", '<application android:name="com.starry.magic.StarryApp" ', 1)
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

echo "[7/7] zip surgical merge + sign"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
c4 = (work / "classes4.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
jsc = work / "assets" / "assets" / "Script" / "index.jsc"
extras["assets/assets/Script/index.jsc"] = jsc.read_bytes()

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
skip = set(extras) | {"classes3.dex", "classes4.dex"}
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in skip:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
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

for name, data in (
    ("classes3.dex", c3),
    ("classes4.dex", c4),
):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in ("classes.dex", "classes2.dex", "resources.arsc"):
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "classes3.dex" in zout2.namelist() and "classes4.dex" in zout2.namelist()
assert "assets/assets/Script/index.jsc" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
print(f"  merged ok; entries≈{n}; classes3={len(c3)} classes4={len(c4)} jsc={len(extras['assets/assets/Script/index.jsc'])}")
zin2.close(); zout2.close()
PY

ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT/.prev"
for old in "$OUT"/动物餐厅_12.38_surgical*Signed.apk; do
  [[ -f "$old" ]] || continue
  mv -f "$old" "$OUT/.prev/$(basename "$old").$(date +%Y%m%d%H%M%S)" 2>/dev/null || true
done
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
  REMOTE="/data/local/tmp/dwct_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  # 官方原版 → 星光签名：需卸载再装；用户已确认真机是原版且可覆盖换签
  if ! adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/tmp/dwct_pm_err.txt; then
    echo "[install] cover failed (likely signature mismatch). Need uninstall+reinstall for first starry install over official."
    cat /tmp/dwct_pm_err.txt 2>/dev/null || true
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      adb shell "pm uninstall $PKG" 2>/dev/null || adb uninstall "$PKG" || true
      adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
    else
      echo "ERROR: set STARRY_ALLOW_UNINSTALL=1 to replace official signature (animal restaurant is not a 10GB OBB game)."
      exit 1
    fi
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  # shellcheck source=/dev/null
  source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
  starry_post_install "$PKG" || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Dwct:V MenuService:V"
fi

echo "DONE: $SIGNED"
