#!/usr/bin/env bash
# 无尽探险队 26.1.5 — 外科组装（官方包字节尽量不变：patch classes2 + 新增 classes5/so/manifest）
# 玩法：WjtxdMod JNI evalString splice（超级无敌 get_ret1 / 超级移速 get_ret2，对齐斌哥）
#
# 产物：08_output/无尽探险队/无尽探险队_26.1.5_surgical-releaseSigned.apk
# 用法:
#   bash scripts/surgical_wjtxd_26.1.5.sh              # 只出包，不装机
#   STARRY_INSTALL=1 bash scripts/surgical_wjtxd_26.1.5.sh   # MuMu 覆盖装机
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
ORIG_APK="$NIXIANG/02_apks/无尽探险队/official_base.apk"
ORIG_MD5="95f3ef57617b2e48ec526268687de7c5"
OUT="$NIXIANG/08_output/无尽探险队"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.wjtxdtt.zy"
SERIAL="${SERIAL:-127.0.0.1:16384}"
SIGNED="$OUT/无尽探险队_26.1.5_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/无尽探险队_26.1.5_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/无尽探险队/surgical/AndroidManifest.xml"

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
SO_PREBUILT="${WJTXD_SO:-$STARRY_SRC/libs_wjtxd/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so ${SO_PREBUILT}（先跑 Android_wjtxd.mk 构建）"; exit 1; }
echo "[check] modern so = $SO_PREBUILT"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 $ACTUAL_MD5"; exit 1; }

LOCK_DIR="/tmp/wjtxd_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wjtxd_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle $GRADLE_TASK (starry → smali + certs)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/6] apktool d starry → smali + certs"
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
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3b/6] patch classes2: GameApplication.onCreate → SignSpoof + loadLibrary; GameActivity → MenuService + get_ret"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
python3.11 - "$WORK/c2smali" <<'PY'
from pathlib import Path
import sys
root = Path(sys.argv[1])

# ---- 1) GameApplication.onCreate：super 后接 SignSpoof + loadLibrary ----
app = root / "cn/play800/game/GameApplication.smali"
if not app.is_file():
    raise SystemExit("GameApplication.smali missing")
t = app.read_text(encoding="utf-8")
if "SignSpoof;->install" in t and "starry_magic" in t:
    print("  GameApplication already patched")
else:
    needle = "    invoke-super {p0}, Lcom/wx/platform/WXApplication;->onCreate()V\n"
    inject = needle + """
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
"""
    if needle not in t:
        raise SystemExit("GameApplication.onCreate super not found")
    if "starry_magic" not in t:
        t = t.replace(needle, inject, 1)
    app.write_text(t, encoding="utf-8")
    print("  GameApplication ← SignSpoof + loadLibrary")

# ---- 2) GameActivity：onCreate 早载 so + MenuService.start；onResume/onPause/onUserLeaveHint；get_ret ----
act = root / "cn/play800/game/GameActivity.smali"
if not act.is_file():
    raise SystemExit("GameActivity.smali missing")
s = act.read_text(encoding="utf-8")

if "MenuService;->start" not in s:
    super_on = "    invoke-super {p0, p1}, Lcom/cocos/lib/CocosActivity;->onCreate(Landroid/os/Bundle;)V\n"
    early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lcom/cocos/lib/CocosActivity;->onCreate(Landroid/os/Bundle;)V
"""
    if super_on not in s:
        raise SystemExit("GameActivity.onCreate super not found")
    oncreate = s.split(".method protected onCreate", 1)[1].split(".end method", 1)[0]
    if "starry_magic" not in oncreate:
        s = s.replace(super_on, early_load, 1)
    marker = "    invoke-virtual {p1, p0, v0}, Lcom/wx/platform/WXCommPlatform;->onCreate(Landroid/app/Activity;Lcom/wx/platform/callback/WXInitListener;)V\n\n    return-void\n.end method"
    inject = "    invoke-virtual {p1, p0, v0}, Lcom/wx/platform/WXCommPlatform;->onCreate(Landroid/app/Activity;Lcom/wx/platform/callback/WXInitListener;)V\n\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V\n\n    return-void\n.end method"
    if marker not in s:
        raise SystemExit("GameActivity.onCreate tail not found")
    s = s.replace(marker, inject, 1)
    print("  onCreate → loadLibrary + MenuService.start")
else:
    print("  MenuService.start already present")

def inject_before_final_return(text: str, method_sig: str, inject: str, tag: str) -> str:
    start = text.find(method_sig)
    if start < 0:
        raise SystemExit(f"{tag}: method {method_sig} not found")
    end = text.find(".end method", start)
    block = text[start:end]
    pos = block.rfind("    return-void")
    if pos < 0:
        raise SystemExit(f"{tag}: no return-void in {method_sig}")
    newblock = block[:pos] + inject + block[pos:]
    return text[:start] + newblock + text[end:]

if "MenuService;->notifyHostResumed" not in s:
    s = inject_before_final_return(
        s, ".method protected onResume()V",
        "    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n",
        "menu/MenuService onResume")
    print("  onResume → notifyHostResumed")

if "MenuService;->notifyHostLeaving" not in s:
    s = inject_before_final_return(
        s, ".method protected onPause()V",
        "    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n",
        "menu/MenuService onPause")
    print("  onPause → notifyHostLeaving")

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

if "WjtxdHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/WjtxdHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/WjtxdHooks;->setRet(IZ)V

    return-void
.end method
"""
    print("  +get_ret/set_ret → WjtxdHooks")
else:
    print("  get_ret already present")

act.write_text(s, encoding="utf-8")
print("  classes2 patches done")
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"
ls -la "$WORK/classes2.dex"

echo "[4/6] binary AndroidManifest（加菜单权限/服务，保留原 Application）"
if [[ -n "${WJTXD_BINARY_MANIFEST:-}" && -f "$WJTXD_BINARY_MANIFEST" ]]; then
  cp "$WJTXD_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
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
# 注册 StarryInitProvider（无 Application 替换注入：SignSpoof + Tap 强制网页 + MenuService 心跳）
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.wjtxdtt.zy.starryinit"
            android:exported="false"
            android:initOrder="100" />
'''
    t = t.replace("</application>", prov + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  text Manifest patched (keep original Application)")
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
c2 = (work / "classes2.dex").read_bytes()
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
    "classes2.dex": c2,
}
untouched = ["classes.dex", "classes3.dex", "classes4.dex", "resources.arsc"]

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name in replace:
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

# 新增 classes5（starry 代码；官方包 classes~classes4 无同名冲突）
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

# 校验：未触碰 dex/arsc 字节不变，patch 与新增到位
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    assert hashlib.sha256(zin2.read(name)).hexdigest() == \
           hashlib.sha256(zout2.read(name)).hexdigest(), f"changed: {name}"
assert "classes5.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
c2o = zin2.read("classes2.dex"); c2n = zout2.read("classes2.dex")
assert hashlib.sha256(c2o).hexdigest() != hashlib.sha256(c2n).hexdigest(), "classes2 not patched"
print(f"  merged ok; entries≈{n}; c2={len(c2)} c5={len(c5)}; untouched 4/4 verified")
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
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
    --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED"
else
  java -jar "$SIGNER" -a "$ALIGNED" --ks "$KS" --ksAlias "$KS_ALIAS" --ksKeyPass "$KS_PASS" \
    --ksPass "$KS_PASS"
  mv "$OUT/$(basename "$ALIGNED")" "$SIGNED" 2>/dev/null || true
fi
MD5_SIGNED=$(md5 -q "$SIGNED")
echo "[6/6] done → $SIGNED (md5 $MD5_SIGNED)"

echo "[verify] 包内 so md5 闭环"
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/extracted.so"
SO_BUILD_MD5=$(md5 -q "$SO_SRC")
SO_PKG_MD5=$(md5 -q "$WORK/extracted.so")
[[ "$SO_BUILD_MD5" == "$SO_PKG_MD5" ]] \
  || { echo "ERROR: 包内 so != 构建产物 md5 ($SO_BUILD_MD5 vs $SO_PKG_MD5)"; exit 1; }
echo "[verify] so md5 ok = $SO_BUILD_MD5"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] push + pm install -r → $SERIAL"
  adb -s "$SERIAL" connect "$SERIAL" >/dev/null 2>&1 || true
  adb -s "$SERIAL" push "$SIGNED" /data/local/tmp/wjtxd_surgical.apk >/dev/null
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$SERIAL" shell "pm install -r -d --user 0 /data/local/tmp/wjtxd_surgical.apk" \
    || { echo "ERROR: 覆盖安装失败（异签名须先卸载并获确认）"; exit 1; }
  adb -s "$SERIAL" shell rm -f /data/local/tmp/wjtxd_surgical.apk
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" \
    || { echo "ERROR: 装机指纹校验失败"; exit 1; }
  echo "[install] done"
fi