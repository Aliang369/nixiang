#!/usr/bin/env bash
# 迷雾大陆 14.0.0 — 外科组装（官方 dex/arsc 字节尽量不变）
# 玩法：MwdlMod se::evalString splice（get_ret 1..4 + 反作弊）
#
# 产物：08_output/迷雾大陆/迷雾大陆_14.0.0_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_mwdl_14.0.0.sh              # 只出包，不装机
#   STARRY_INSTALL=1 bash scripts/surgical_mwdl_14.0.0.sh
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
ORIG_APK="$NIXIANG/02_apks/迷雾大陆/迷雾大陆_14.0.0_official.apk"
OUT="$NIXIANG/08_output/迷雾大陆"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.hortor.mwdl"
SIGNED="$OUT/迷雾大陆_14.0.0_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/迷雾大陆_14.0.0_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/迷雾大陆/surgical/AndroidManifest.xml"

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
SO_PREBUILT="${MWDL_SO:-$STARRY_SRC/app/src/libs_mwdl/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so $SO_PREBUILT（先跑 Android_mwdl.mk 构建）"; exit 1; }
echo "[check] modern so = $SO_PREBUILT"

LOCK_DIR="/tmp/mwdl_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/mwdl_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle $GRADLE_TASK + ndk (starry)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes5.dex"
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
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
ls -la "$WORK/classes5.dex"

SO_SRC="$SO_PREBUILT"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3b/6] patch classes3: GameApplication.attachBaseContext → SignSpoof + loadLibrary"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
python3.11 - "$WORK/c3smali" <<'PY'
from pathlib import Path
import sys
app = Path(sys.argv[1]) / "com/hortorgames/gamesdk/GameApplication.smali"
t = app.read_text(encoding="utf-8")
if "SignSpoof;->install" in t and "starry_magic" in t:
    print("  GameApplication already patched")
else:
    inject = """
.method protected attachBaseContext(Landroid/content/Context;)V
    .locals 1
    .param p1, "base"    # Landroid/content/Context;

    invoke-super {p0, p1}, Landroidx/multidex/MultiDexApplication;->attachBaseContext(Landroid/content/Context;)V

    # starry：先 loadLibrary → JNI_OnLoad 做 hidden-api 豁免，再装 SignSpoof（清缓存才有豁免可用）
    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    return-void
.end method

"""
    marker = ".method public onCreate()V"
    if marker not in t:
        raise SystemExit("GameApplication.onCreate missing")
    t = t.replace(marker, inject + marker, 1)
    app.write_text(t, encoding="utf-8")
    print("  +attachBaseContext → SignSpoof + loadLibrary")
PY
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"
ls -la "$WORK/classes3.dex"

echo "[3c/6] patch classes4: AppActivity + force TapTap WebLogin + get_ret"
unzip -p "$ORIG_APK" classes4.dex > "$WORK/classes4_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c4smali" "$WORK/classes4_orig.dex"
python3.11 - "$WORK/c4smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])

# 旧版 Tap SDK：ActivityHandler.authorize 会跳转 com.taptap 客户端 SSO（改签必失败）
# 抛 ActivityNotFoundException → LoginClient.sendLoginRequest catch 走 WebViewHandler
ah = root / "com/taptap/sdk/ui/ActivityHandler.smali"
if not ah.is_file():
    raise SystemExit(f"missing {ah}")
ah_t = ah.read_text(encoding="utf-8")
FORCE_WEB = """.method public authorize(Lcom/taptap/sdk/LoginRequest;)V
    .locals 1
    .param p1, "request"    # Lcom/taptap/sdk/LoginRequest;

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO
    new-instance v0, Landroid/content/ActivityNotFoundException;

    const-string p1, "starry_force_web_login"

    invoke-direct {v0, p1}, Landroid/content/ActivityNotFoundException;-><init>(Ljava/lang/String;)V

    throw v0
.end method"""
ah2, n = re.subn(
    r"\.method public authorize\(Lcom/taptap/sdk/LoginRequest;\)V[\s\S]*?\.end method",
    FORCE_WEB,
    ah_t,
    count=1,
)
if n != 1:
    raise SystemExit("ActivityHandler.authorize patch failed")
ah.write_text(ah2, encoding="utf-8")
print("  ActivityHandler.authorize → force WebLogin")

act = root / "org/cocos2dx/javascript/AppActivity.smali"
s = act.read_text(encoding="utf-8")

# onCreate 尾部启动 MenuService（在 return-void 前）
if "MenuService;->start" not in s:
    marker = """    invoke-virtual {v0, p1}, Lcom/hortorgames/gamesdk/common/AppSDK;->onCreate(Landroid/os/Bundle;)V

    return-void
.end method"""
    inject = """    invoke-virtual {v0, p1}, Lcom/hortorgames/gamesdk/common/AppSDK;->onCreate(Landroid/os/Bundle;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void
.end method"""
    if marker not in s:
        raise SystemExit("AppActivity.onCreate tail not found")
    s = s.replace(marker, inject, 1)
    print("  onCreate → MenuService.start")
else:
    print("  MenuService.start already present")

if "MenuService;->notifyHostResumed" not in s:
    s2, n = re.subn(
        r"(\.method protected onResume\(\)V[\s\S]*?)(    return-void\n\.end method)",
        r"\1    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n\2",
        s, count=1)
    if n != 1:
        raise SystemExit("AppActivity.onResume patch failed")
    s = s2
    print("  onResume → notifyHostResumed")
else:
    print("  onResume already patched")

if "MenuService;->notifyHostLeaving" not in s:
    # onPause 末尾
    s2, n = re.subn(
        r"(\.method protected onPause\(\)V[\s\S]*?)(    return-void\n\.end method)",
        r"\1    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n\2",
        s, count=1)
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

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method protected onPause()V"
    if marker not in s:
        raise SystemExit("onPause marker missing")
    s = s.replace(marker, leave + marker, 1)
    print("  +onUserLeaveHint")

if "MwdlHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/MwdlHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/MwdlHooks;->setRet(IZ)V

    return-void
.end method
"""
    print("  +get_ret/set_ret → MwdlHooks")
else:
    print("  get_ret already present")

if "MwdlHooks;->getVal" not in s:
    s += """
.method public static get_val(I)F
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/MwdlHooks;->getVal(I)F

    move-result v0

    return v0
.end method

.method public static set_val(IF)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "v"    # F

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/MwdlHooks;->setVal(IF)V

    return-void
.end method
"""
    print("  +get_val/set_val → MwdlHooks")
else:
    print("  get_val already present")

act.write_text(s, encoding="utf-8")
PY
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$WORK/c4smali"
ls -la "$WORK/classes4.dex"

echo "[4/6] binary AndroidManifest（保留 GameApplication，加菜单权限/服务）"
if [[ -n "${MWDL_BINARY_MANIFEST:-}" && -f "$MWDL_BINARY_MANIFEST" ]]; then
  cp "$MWDL_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
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
# 保留 com.hortorgames.gamesdk.GameApplication，不换成 StarryApp
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
print("  text Manifest patched (keep GameApplication)")
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
c3 = (work / "classes3.dex").read_bytes()
c4 = (work / "classes4.dex").read_bytes()
c5 = (work / "classes5.dex").read_bytes()
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
    "classes3.dex": c3,
    "classes4.dex": c4,
    "classes5.dex": c5,
}

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name in replace and name != "classes5.dex":
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

# 新增 classes5（原包无）
info = zipfile.ZipInfo("classes5.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c5)
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
for name in ("resources.arsc", "classes.dex", "classes2.dex"):
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "classes5.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
print(f"  merged ok; entries≈{n}; c3={len(c3)} c4={len(c4)} c5={len(c5)}")
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
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/mwdl_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  # 大资源游戏：仅覆盖安装，禁止默认卸载
  if ! adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover-install failed — 不自动卸载；请人工处理签名冲突后重试"
    exit 1
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Mwdl:V MenuService:V StarrySignSpoof:V"
fi

echo "DONE: $SIGNED"
echo "NOTE: 默认未装机。需要覆盖装机时再 STARRY_INSTALL=1 并先在聊天确认。"
