#!/usr/bin/env bash
# 造梦无双 1.34（vc77，TapTap 渠道官方原包）— 外科组装
#   官方 dex 字节尽量不变：仅 patch classes5.dex（FN4399HzApplication + AppActivity）+ 新增 classes27.dex（starry）
#   玩法：ZmwsMod se::evalString splice（get_ret 1..4 + 反举报对齐斌哥）
#
# 与 1.33 版差异（2026-10-27 复刻）：
#   1) 底包 = 造梦无双_1.34_taptap_official.apk（真机 pull，CN=4399 / vc77）
#   2) FN4399HzApplication.attachBaseContext 的 super 目标由 FNSDKLIbApplication 变为 android/app/Application
#      → 两代 needle 都试，命中哪代记日志
#   3) 1.34 包内已有 classes7..classes26.dex（其中 classes7~10 多为 140 B 空壳）⇒ starry dex 改名 classes27.dex，
#      禁止与原包同名（同级 zip 重名会让 ART 读到空壳 dex，菜单类直接 ClassNotFound）
#   4) Manifest 首次生成用 apktool d -s --no-assets（1.9 GB 包别整包 decode）
#   5) 装机段默认不装；装机统一走 04_scripts/_common/install_starry_apk.sh（异签名需卸载重装）
#
# 用法:
#   bash scripts/surgical_zmws_1.34.sh                  # 只出包
#   SERIAL=<序列号> STARRY_INSTALL=1 bash ... 1.34.sh   # 出包后覆盖安装（同签名才可能成功）
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE 只支持 release/debug" >&2
  exit 2
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
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
VER="1.34"
VER_SHORT="1.34"
ORIG_APK="$NIXIANG/02_apks/造梦无双/造梦无双_1.34_taptap_official.apk"
ORIG_MD5="71108bd6b79e0afcdf03db83777188ef"
OUT="$NIXIANG/08_output/造梦无双"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.xm4399.zmws.m4399"
SERIAL="${SERIAL:-$(adb get-serialno 2>/dev/null || true)}"
STARRY_DEX_NAME="${STARRY_DEX_NAME:-classes27.dex}"
SIGNED="$OUT/造梦无双_${VER_SHORT}_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/造梦无双_${VER_SHORT}_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/造梦无双/surgical_${VER_SHORT}/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
AAPT2="${BT:+$BT/aapt2}"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }

# modern so 定位：优先显式 ZMWS_SO，其次管线 build_so 产物（libs_zmws），再次 app/src/libs_zmws
SO_PREBUILT="${ZMWS_SO:-}"
if [[ -z "$SO_PREBUILT" ]]; then
  for c in "$STARRY_SRC/libs_zmws/arm64-v8a/libstarry_magic.so" \
           "$STARRY_SRC/app/src/libs_zmws/arm64-v8a/libstarry_magic.so"; do
    [[ -f "$c" ]] && SO_PREBUILT="$c" && break
  done
fi
[[ -n "$SO_PREBUILT" && -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so（先跑 04_scripts/造梦无双/zmws_build_so.sh）"; exit 1; }
echo "[check] modern so = $SO_PREBUILT ($(md5 -q "$SO_PREBUILT"))"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 $ACTUAL_MD5 != $ORIG_MD5"; exit 1; }
echo "[check] 官包 md5 = $ACTUAL_MD5 ✓"

# 预检：目标 dex 名不得与原包同名
if unzip -l "$ORIG_APK" | grep -q " $STARRY_DEX_NAME$"; then
  echo "ERROR: 原包已存在 ${STARRY_DEX_NAME}，换 STARRY_DEX_NAME（重名会让 ART 读到空壳 dex）" >&2
  exit 1
fi

LOCK_DIR="/tmp/zmws_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zmws_surgical.XXXXXX)"
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

echo "[3/6] collect com/starry smali → $STARRY_DEX_NAME"
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
"$SMALI_BIN" assemble -o "$WORK/$STARRY_DEX_NAME" "$SMALI_ROOT"
ls -la "$WORK/$STARRY_DEX_NAME"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3b/6] patch classes5: FN4399HzApplication.attachBaseContext → SignSpoof + loadLibrary；AppActivity → 菜单"
unzip -p "$ORIG_APK" classes5.dex > "$WORK/classes5_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c5smali" "$WORK/classes5_orig.dex"
python3.11 - "$WORK/c5smali" <<'PY'
from pathlib import Path
import sys
root = Path(sys.argv[1])

# ---- 1) FN4399HzApplication.attachBaseContext：super 后接 SignSpoof + loadLibrary
app = root / "com/ssjj/fnsdk/platform/FN4399HzApplication.smali"
if not app.is_file():
    raise SystemExit("FN4399HzApplication.smali missing")
t = app.read_text(encoding="utf-8")
if "SignSpoof;->install" in t and "starry_magic" in t:
    print("  FN4399HzApplication already patched")
else:
    # 1.33: super 指 FNSDKLIbApplication；1.34: super 指 android/app/Application（两代都认）
    cands = [
        "    invoke-super {p0, p1}, Lcom/ssjj/fnsdk/core/FNSDKLIbApplication;->attachBaseContext(Landroid/content/Context;)V\n",
        "    invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V\n",
    ]
    needle = next((c for c in cands if c in t), None)
    if needle is None:
        raise SystemExit("FN4399HzApplication.attachBaseContext super 两代 needle 都未命中")
    inject = needle + """
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
"""
    t = t.replace(needle, inject, 1)
    app.write_text(t, encoding="utf-8")
    print("  FN4399HzApplication ← SignSpoof + loadLibrary (super=%s)" % needle.strip().split(",")[1].strip())

# ---- 2) AppActivity：onCreate 早载 so + MenuService.start；onResume/onPause 通知；onUserLeaveHint；get_ret
act = root / "com/xm4399/zmws/AppActivity.smali"
if not act.is_file():
    raise SystemExit("AppActivity.smali missing")
s = act.read_text(encoding="utf-8")

if "MenuService;->start" not in s:
    super_on = "    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V\n"
    early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V
"""
    if super_on not in s:
        raise SystemExit("AppActivity.onCreate super not found")
    oncreate = s.split(".method protected onCreate")[1].split(".end method")[0]
    if "starry_magic" not in oncreate:
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
    print("  onCreate → loadLibrary + MenuService.start")
else:
    print("  MenuService.start already present")

def inject_before_final_return(text: str, method_sig: str, inject: str, tag: str) -> str:
    """在指定方法块最后一个 return-void 前插入 invoke（cond 标签无关）。"""
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

if "ZmwsHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/ZmwsHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/ZmwsHooks;->setRet(IZ)V

    return-void
.end method
"""
    print("  +get_ret/set_ret → ZmwsHooks")
else:
    print("  get_ret already present")

act.write_text(s, encoding="utf-8")
print("  classes5 patches done")
PY
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$WORK/c5smali"
ls -la "$WORK/classes5.dex"

echo "[4/6] binary AndroidManifest（保留官方 FN4399HzApplication，加菜单权限/服务）"
if [[ -n "${ZMWS_BINARY_MANIFEST:-}" && -f "$ZMWS_BINARY_MANIFEST" ]]; then
  cp "$ZMWS_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  首次生成 binary Manifest（apktool d -s --no-assets：跳过 1.9GB assets）"
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
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 保留官方 Application（FN4399HzApplication），不换成 StarryApp
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
print("  text Manifest patched (keep FN4399HzApplication)")
PY
  # apktool d 会把 <uses-sdk> 抽到 apktool.yml 的 sdkInfo；缺失时 b 出来就没有 uses-sdk
  # ⇒ Android 按 minSdk=1/targetSdk=1 兼容模式跑（2026-10-27 首跑实测丢 3 行）。这里显式回填。
  python3.11 - "$WORK/orig_dec/apktool.yml" "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
import re, sys
from pathlib import Path
yml, mf = Path(sys.argv[1]), Path(sys.argv[2])
t = mf.read_text(encoding="utf-8")
minv = re.search(r'minSdkVersion="(\d+)"', t)
tgtv = re.search(r'targetSdkVersion="(\d+)"', t)
minv = minv.group(1) if minv else "16"
tgtv = tgtv.group(1) if tgtv else "30"
y = yml.read_text(encoding="utf-8")
if "sdkInfo:" not in y:
    y = y.rstrip("\n") + "\n" + (
        "sdkInfo:\n"
        f"  minSdkVersion: '{minv}'\n"
        f"  targetSdkVersion: '{tgtv}'\n")
    yml.write_text(y, encoding="utf-8")
    print(f"  apktool.yml ← sdkInfo(min={minv}, target={tgtv})")
else:
    print("  apktool.yml 已含 sdkInfo")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
echo "  AndroidManifest.xml = $(stat -f '%z' "$WORK/AndroidManifest.xml") B"

# ---- Manifest 回读校验（节点在 + 官方节点不丢）
if [[ -n "$AAPT2" && -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  for k in "com.starry.magic.menu.MenuService" \
           "android.permission.FOREGROUND_SERVICE_SPECIAL_USE" \
           "android.permission.SYSTEM_ALERT_WINDOW" \
           "com.ssjj.fnsdk.platform.FN4399HzApplication" \
           "com.xm4399.zmws.AppActivity"; do
    grep -q "$k" "$WORK/tree_new.txt" || { echo "ERROR: 新 manifest 缺 $k" >&2; exit 1; }
  done
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  # 按「行多重集」比对，不做位置 diff：apktool 会把 <uses-sdk> 从文档尾部挪到开头
  # （2026-10-27 实测：位置 diff 把这次合法搬家误报成「丢失 3 行」）。要求＝官方每一行都能在新树里找到。
  python3.11 - "$WORK/o.norm" "$WORK/n.norm" <<'PY'
import sys
from collections import Counter
from pathlib import Path
o = Counter(Path(sys.argv[1]).read_text(encoding="utf-8").splitlines())
n = Counter(Path(sys.argv[2]).read_text(encoding="utf-8").splitlines())
missing = o - n
added = sum((n - o).values())
if missing:
    print("ERROR: manifest 重建丢失官方节点（%d 行）：" % sum(missing.values()))
    for line, c in list(missing.items())[:10]:
        print("   -%dx %s" % (c, line.strip()))
    raise SystemExit(1)
print("  manifest ok（官方节点 0 丢失；新增 %d 行）" % added)
PY
  # uses-sdk 值必须与官方一致（丢了会让系统按 minSdk=1/targetSdk=1 兼容模式跑，Android 14+ 甚至直接拒装）
  grep -q "minSdkVersion(0x0101020c)=16" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest minSdk 不对" >&2; exit 1; }
  grep -q "targetSdkVersion(0x01010270)=30" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest targetSdk 不对" >&2; exit 1; }
  echo "  uses-sdk ok（min=16 / target=30）✓"
else
  echo "  WARN: aapt2 不可用，跳过 manifest 回读校验"
fi

echo "[5/6] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$STARRY_DEX_NAME" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst, starry_dex = (
    Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4])
mf = (work / "AndroidManifest.xml").read_bytes()
c5 = (work / "classes5.dex").read_bytes()
c7 = (work / starry_dex).read_bytes()
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
    "classes5.dex": c5,
}
untouched = ["classes.dex", "classes2.dex", "classes3.dex",
             "classes4.dex", "classes6.dex", "resources.arsc"]

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert starry_dex not in names, f"{starry_dex} 与原包重名，禁止（会覆盖空壳 dex）"
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name == starry_dex:
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

info = zipfile.ZipInfo(starry_dex)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c7)
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
    assert hashlib.sha256(zin2.read(name)).hexdigest() == \
           hashlib.sha256(zout2.read(name)).hexdigest(), f"changed: {name}"
out_names = zout2.namelist()
assert len(out_names) == len(set(out_names)), "zip 内存在重名条目（会读到错 dex）"
assert starry_dex in out_names
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
c5o = zin2.read("classes5.dex"); c5n = zout2.read("classes5.dex")
assert hashlib.sha256(c5o).hexdigest() != hashlib.sha256(c5n).hexdigest(), "classes5 not patched"
print(f"  merged ok; entries≈{n}; c5={len(c5)} {starry_dex}={len(c7)}; untouched 6/6 verified；无重名")
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

# ---- so 闭环校验：包内 so 必须与 build_so 产物字节一致
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
if [[ "$IN_SO_MD5" != "$SRC_SO_MD5" ]]; then
  echo "ERROR: 包内 so ($IN_SO_MD5) != 构建产物 ($SRC_SO_MD5)——旧 so 混包，禁止交付" >&2
  exit 1
fi
echo "[so-verify] 包内 so md5 == 构建产物 ($IN_SO_MD5) ✓"

# ---- dex 闭环校验：菜单类真的在包里
CK_DEX="$(mktemp -d /tmp/zmws_ckdex.XXXXXX)"
unzip -p "$SIGNED" "$STARRY_DEX_NAME" > "$CK_DEX/$STARRY_DEX_NAME"
"$BAKSMALI_BIN" d -o "$CK_DEX/out" "$CK_DEX/$STARRY_DEX_NAME" >/dev/null
[[ -f "$CK_DEX/out/com/starry/magic/cocos/ZmwsHooks.smali" ]] \
  || { echo "ERROR: 包内 $STARRY_DEX_NAME 缺 ZmwsHooks" >&2; rm -rf "$CK_DEX"; exit 1; }
rm -rf "$CK_DEX"
echo "[dex-verify] 包内 $STARRY_DEX_NAME 含 ZmwsHooks ✓"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  [[ -n "$SERIAL" ]] || { echo "ERROR: 无设备（SERIAL 为空）" >&2; exit 1; }
  REMOTE="/data/local/tmp/zmws_surgical.apk"
  adb -s "$SERIAL" push "$SIGNED" "$REMOTE"
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  # 大资源游戏：仅覆盖安装；异签名请改走 install_starry_apk.sh --clean（需人工确认卸载）
  if ! adb -s "$SERIAL" shell "pm install -r -d --user 0 \"$REMOTE\"" 2>/dev/null; then
    echo "[install] 覆盖安装失败 —— 不自动卸载；异签名需人工确认后走 install_starry_apk.sh --clean"
    exit 1
  fi
  adb -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb -s $SERIAL logcat -s StarryMagic:V Zmws:V MenuService:V StarrySignSpoof:V"
fi

echo "DONE: $SIGNED"
echo "NOTE: 默认未装机。异签名（官方 CN=4399 vs 星空 CN=StarryMagic）必须卸载重装，先与用户确认。"
