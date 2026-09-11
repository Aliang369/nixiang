#!/usr/bin/env bash
# 异世界勇者 8.1.1 — 外科组装（官包字节尽量不变：classes2.dex patch + classes3.dex 新增）
# 玩法：YsjYzMod se::evalString splice（JS 补丁块，14 项功能）
#
# 产物：08_output/异世界勇者/异世界勇者_8.1.1_surgical-unsigned.apk（未签名）
#       签名由 ysjyz_sign.sh 负责（pipeline sign 阶段）
#
# 用法: bash scripts/surgical_ysjyz_8.1.1.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
ORIG_APK="$NIXIANG/02_apks/异世界勇者/异世界勇者_8.1.1_official.apk"
ORIG_MD5="710d1cfe9a86f837d5c697b376e73767"
OUT="$NIXIANG/08_output/异世界勇者"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.mudamuda.idlewow"
UNSIGNED="$OUT/异世界勇者_8.1.1_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/异世界勇者/surgical/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }
SO_PREBUILT="${YSJYZ_SO:-$STARRY_SRC/libs_ysjyz/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so $SO_PREBUILT（先跑 ysjyz_build_so.sh）"; exit 1; }
echo "[check] modern so = $SO_PREBUILT"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 $ACTUAL_MD5"; exit 1; }

LOCK_DIR="/tmp/ysjyz_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/ysjyz_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/5] gradle assembleRelease (starry → smali + certs)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_ysjyz/' --exclude 'libs_ysjyz/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/5] apktool d starry → com/starry smali → classes3.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
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
[[ -f "$SMALI_ROOT/com/starry/magic/cocos/YsjYzHooks.smali" ]] \
  || { echo "ERROR: YsjYzHooks.smali missing（Java 桥没编进 starry app）"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$SMALI_ROOT"
ls -la "$WORK/classes3.dex"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/5] patch classes2: LocalApplication.onCreate + AppActivity 生命周期 + get_ret/get_val"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
python3.11 - "$WORK/c2smali" <<'PY'
from pathlib import Path
import sys
root = Path(sys.argv[1])


def inject_before_final_return(text: str, method_sig: str, inject: str, tag: str) -> str:
    start = text.find(method_sig)
    if start < 0:
        raise SystemExit(f"{tag}: method {method_sig} not found")
    end = text.find(".end method", start)
    block = text[start:end]
    pos = block.rfind("    return-void")
    if pos < 0:
        raise SystemExit(f"{tag}: no return-void in {method_sig}")
    return text[:start] + block[:pos] + inject + block[pos:] + text[end:]


# ---- 1) LocalApplication.onCreate：SignSpoof + loadLibrary（早于 XiPuSDKApplication.onCreate）
app = root / "org/cocos2dx/javascript/LocalApplication.smali"
if not app.is_file():
    raise SystemExit("LocalApplication.smali missing")
t = app.read_text(encoding="utf-8")
if "SignSpoof;->install" in t and "starry_magic" in t:
    print("  LocalApplication already patched")
else:
    needle = "    invoke-super {p0}, Lcom/xipu/msdk/XiPuSDKApplication;->onCreate()V\n"
    if needle not in t:
        raise SystemExit("LocalApplication.onCreate super not found")
    inject = ("""    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

""" + needle)
    t = t.replace(needle, inject, 1)
    # .registers 1 → 2（新增 v0）
    head = t.find(".method public onCreate()V")
    end = t.find(".end method", head)
    block = t[head:end]
    if ".registers 1" not in block:
        raise SystemExit("LocalApplication.onCreate .registers 1 not found")
    t = t[:head] + block.replace(".registers 1", ".registers 2", 1) + t[end:]
    app.write_text(t, encoding="utf-8")
    print("  LocalApplication ← SignSpoof + loadLibrary")

# ---- 2) AppActivity：MenuService.start + 生命周期通知 + get_ret/get_val/set_ret
act = root / "org/cocos2dx/javascript/AppActivity.smali"
if not act.is_file():
    raise SystemExit("AppActivity.smali missing")
s = act.read_text(encoding="utf-8")

if "MenuService;->start" not in s:
    s = inject_before_final_return(
        s, ".method protected onCreate(Landroid/os/Bundle;)V",
        "    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V\n\n",
        "menu/MenuService onCreate")
    print("  onCreate → MenuService.start")
else:
    print("  MenuService.start already present")

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

if "YsjYzHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .registers 2

    invoke-static {p0}, Lcom/starry/magic/cocos/YsjYzHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static get_val(I)I
    .registers 2

    invoke-static {p0}, Lcom/starry/magic/cocos/YsjYzHooks;->getVal(I)I

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .registers 2

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/YsjYzHooks;->setRet(IZ)V

    return-void
.end method

.method public static set_val(II)V
    .registers 2

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/YsjYzHooks;->setVal(II)V

    return-void
.end method
"""
    print("  +get_ret/get_val/set_ret/set_val → YsjYzHooks")
else:
    print("  get_ret already present")

act.write_text(s, encoding="utf-8")
print("  classes2 patches done")
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"
ls -la "$WORK/classes2.dex"

echo "[4/5] binary AndroidManifest（保留 LocalApplication，加菜单权限/服务）"
if [[ -n "${YSJYZ_BINARY_MANIFEST:-}" && -f "$YSJYZ_BINARY_MANIFEST" ]]; then
  cp "$YSJYZ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once (apktool d --no-assets + b extract)"
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
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
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 保留原 Application（org.cocos2dx.javascript.LocalApplication），不换成 StarryApp
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
print("  text Manifest patched (keep original Application)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  mkdir -p "$WORK/orig_dec/assets"
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/5] zip surgical merge → $UNSIGNED"
python3.11 - "$ORIG_APK" "$WORK" "$UNSIGNED" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c2 = (work / "classes2.dex").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
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
untouched = ["classes.dex", "resources.arsc",
             "assets/assets/main/index.jsc", "assets/src/settings.jsc"]

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
for n in replace:
    assert n in names, f"orig apk missing {n}"
assert "classes3.dex" not in names, "orig apk already has classes3.dex — 需改用 classes4.dex"
for n in untouched:
    assert n in names, f"orig apk missing {n}"

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

info = zipfile.ZipInfo("classes3.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c3)
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
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
assert "classes3.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
c2o = zin2.read("classes2.dex"); c2n = zout2.read("classes2.dex")
assert hashlib.sha256(c2o).hexdigest() != hashlib.sha256(c2n).hexdigest(), "classes2 not patched"
print(f"  merged ok; entries≈{n}; c2={len(c2)} c3={len(c3)}; untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

# ---- so 闭环校验：包内 so 必须与 build_so 产物字节一致（防旧 so 混包） ----
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
if [[ "$IN_SO_MD5" != "$SRC_SO_MD5" ]]; then
  echo "ERROR: 包内 so ($IN_SO_MD5) != 构建产物 ($SRC_SO_MD5)——旧 so 混包，禁止交付" >&2
  exit 1
fi
echo "[so-verify] 包内 so md5 == 构建产物 ($IN_SO_MD5) ✓"

ls -lh "$UNSIGNED"
echo "DONE(unsigned): $UNSIGNED"
echo "NOTE: 未签名；由 ysjyz_sign.sh（pipeline sign 阶段）签名。"
