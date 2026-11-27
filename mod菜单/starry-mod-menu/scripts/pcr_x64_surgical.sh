#!/usr/bin/env bash
# 公主连结 11.7.2 — 雷电（x86_64）适配出包：一份 APK 同时带 arm64-v8a + x86_64 的 libstarry_magic.so
#
# 根因（2026-10-31 实测）：官方 PCR 11.7.2 的 APK 自带 lib/arm64-v8a + lib/armeabi-v7a + lib/x86_64
# 三套 so；雷电模拟器 primaryCpuAbi=x86_64 ⇒ 只解出 lib/x86_64，而星光包只注入了 arm64 的
# libstarry_magic.so ⇒ 进程内 dlopen("libstarry_magic.so") 恒失败
# （logcat: StarryNativeLoad: java.lang.UnsatisfiedLinkError: dlopen failed: library "libstarry_magic.so" not found）
# ⇒ Java 菜单照弹、bridge 无 native ⇒ 菜单显示「暂无功能列表（native 未就绪）」。
# 修法＝按 x86_64 编一份同源 so 一起打进包（不是为模拟器单独出包：同一个包两端通用）。
#
# 底包：设备上现装的星光包（只读拉取），受保护条目（classes0–12 / resources.arsc / Manifest）
# 逐字节保留，只替换 classes13.dex + lib/*/libstarry_magic.so + lib/*/libc++_shared.so。
#
# 用法：
#   bash scripts/pcr_x64_surgical.sh                  # 出包
#   STARRY_INSTALL=1 bash scripts/pcr_x64_surgical.sh # 出包 + 覆盖安装（禁 uninstall，热更约 10GB）
#   STARRY_BASE_APK=<path>  覆盖底包
#   APP_ABIS="arm64-v8a x86_64"（默认两 ABI 全出）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../../.." && pwd)}"
PKG="com.bilibili.priconne"
BASE="${STARRY_BASE_APK:-$NIXIANG/02_apks/公主连结/公主连结_11.7.2_device_starry_20261031.apk}"
OUT="$NIXIANG/08_output/公主连结"
SIGNED="$OUT/公主连结_11.7.2_双ABI_20261031-releaseSigned.apk"
# 可选：二进制补丁 Manifest（外科合并时用它替换底包那条；受保护断言改为「产物==提供文件」，
# classes*.dex / resources.arsc 仍必须与底包逐字节相等）。留空＝完全沿用旧行为。
PCR_BINARY_MANIFEST="${PCR_BINARY_MANIFEST:-}"
APP_ABIS="${APP_ABIS:-}"        # 留空＝按底包实况自动判定（04_scripts/_common/detect_abis.py）
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

# shellcheck disable=SC1091
. "$NIXIANG/04_scripts/_common/env.sh"

m() { cygpath -m "$1" 2>/dev/null || printf '%s' "$1"; }   # 给 Windows 程序（make / java）用的混合路径

LOCAL_APP_DATA="${LOCALAPPDATA:-C:\\Users\\Administrator\\AppData\\Local}"
SDK_HOME="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-$LOCAL_APP_DATA\\Android\\Sdk}}"
NDK="${ANDROID_NDK_HOME:-$SDK_HOME\\ndk\\27.0.12077973}"
NDK_BIN="$(m "$NDK")/ndk-build.cmd"
ZIPALIGN="${NIXIANG_ZIPALIGN:-}"
APKSIGNER="${NIXIANG_APKSIGNER:-}"
PY="${NIXIANG_PYTHON:-python3}"
APKTOOL="$NIXIANG/01_tools/apktool/apktool.jar"
SMALI_JAR="$NIXIANG/01_tools/smali/smali-fat.jar"   # 必须 fat-jar：smali.jar 无 Main-Class


[[ -f "$BASE" ]] || { echo "ERROR: 缺底包 $BASE"; exit 1; }
[[ -f "$NDK_BIN" ]] || { echo "ERROR: 缺 NDK ndk-build.cmd ($NDK_BIN)"; exit 1; }
[[ -f "$APKTOOL" ]] || { echo "ERROR: 缺 apktool.jar"; exit 1; }
[[ -f "$SMALI_JAR" ]] || { echo "ERROR: 缺 smali.jar"; exit 1; }
if [ -n "$PCR_BINARY_MANIFEST" ]; then
  [[ -f "$PCR_BINARY_MANIFEST" ]] || { echo "ERROR: PCR_BINARY_MANIFEST 指向的文件不存在: $PCR_BINARY_MANIFEST"; exit 1; }
fi
[[ -n "$ZIPALIGN" && -n "$APKSIGNER" ]] || { echo "ERROR: 缺 zipalign/apksigner（env.sh 未解析）"; exit 1; }

# 按底包实况决定出哪几套 so：底包带 lib/x86_64 就多出一份（雷电原生 x86_64 直跑），
# 只有 arm64 就只出 arm64（雷电靠 houdini 转译，另见 skill/starry-mod/surgical-build.md「ABI」节）。
# 覆盖：APP_ABIS="arm64-v8a" 或 APP_ABIS="arm64-v8a x86_64"
if [ -z "$APP_ABIS" ]; then
  APP_ABIS="$("$PY" "$NIXIANG/04_scripts/_common/detect_abis.py" "$BASE" 2>/dev/null | tr -d '\r')"
fi
[ -n "$APP_ABIS" ] || { echo "ERROR: 底包没有框架可产出的 ABI（用 APP_ABIS=... 显式指定）"; exit 1; }

# dex 序号必须按底包实况算：ART 的 DexPathList 只按 classes.dex→classes2.dex→… 连续扫描，
# 遇到第一个缺号即停；写死 classes13.dex 在只有 4 个 dex 的底包上会被断层在 5 而永不加载。
DEX_IDX="$("$PY" "$NIXIANG/04_scripts/_common/next_free_dex.py" "$BASE" 2>/dev/null | tr -d '\r')"
case "$DEX_IDX" in ''|*[!0-9]*) echo "ERROR: next_free_dex.py 未给出有效序号（'$DEX_IDX'）"; exit 1;; esac
DEX_NAME="classes${DEX_IDX}.dex"
echo "DEX_NAME=${DEX_NAME}（next_free_dex.py 按底包实况算出）"
export JAVA_HOME="${NIXIANG_JAVA17:-${JAVA_HOME:-}}"
export PATH="$JAVA_HOME/bin:$PATH"
export ANDROID_HOME="$(m "$SDK_HOME")"
export ANDROID_SDK_ROOT="$ANDROID_HOME"

# 工作目录放在仓内 13_scratch（Windows 程序看不到 MSYS 的 /tmp），并统一用混合路径
mkdir -p "$NIXIANG/13_scratch" "$OUT"
WORK="$(m "$(mktemp -d "$NIXIANG/13_scratch/pcr_x64.XXXXXX")")"
trap 'rm -rf "$WORK"' EXIT
echo "WORK=$WORK"

echo "[1/6] ndk-build（APP_ABI=${APP_ABIS}，源码 Android_pcr.mk）"
( cd "$STARRY_SRC/app/src/main/jni" && "$NDK_BIN" \
    NDK_PROJECT_PATH="$(m "$STARRY_SRC/app")" \
    APP_BUILD_SCRIPT=./Android_pcr.mk \
    NDK_APPLICATION_MK=./Application.mk \
    APP_ABI="$APP_ABIS" \
    NDK_OUT="$WORK/obj" NDK_LIBS_OUT="$WORK/libs" -j8 )

mkdir -p "$WORK/lib"
for abi in $APP_ABIS; do
  SO="$WORK/libs/$abi/libstarry_magic.so"
  [[ -f "$SO" ]] || { echo "ERROR: ndk-build 未产出 $abi so"; exit 1; }
  # 用 grep -a 而不是 strings：Git Bash 里没有 strings 命令
  grep -a -q '\[Pcr\]' "$SO" || { echo "ERROR: $abi so 缺 [Pcr] 标签"; exit 1; }
  if grep -a -qE '\[Gjc2\]|\[Dwct\]|\[Ltzj\]|\[Zhmr\]' "$SO"; then
    echo "ERROR: $abi so 仍含其他游戏标签"; exit 1
  fi
  # 构建戳已随 OTA 下线退役（2026-11-21），不再断言 STARRYSTAMP1
  mkdir -p "$WORK/lib/$abi"
  cp -f "$SO" "$WORK/lib/$abi/libstarry_magic.so"
  CXX="$WORK/libs/$abi/libc++_shared.so"
  [[ -f "$CXX" ]] || { echo "ERROR: 缺 $abi libc++_shared.so"; exit 1; }
  cp -f "$CXX" "$WORK/lib/$abi/libc++_shared.so"
  echo "  $abi so=$(wc -c <"$SO")B cxx=$(wc -c <"$CXX")B"
done

echo "[2/6] gradle assembleRelease（菜单 smali 源）"
( cd "$STARRY_SRC" && ./gradlew :app:assembleRelease --console=plain --offline \
    -x lintVitalReportRelease -x lintVitalAnalyzeRelease >/dev/null )
STARRY_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: 缺 $STARRY_APK"; exit 1; }

echo "[3/6] apktool d → com/starry smali → $DEX_NAME"
java -jar "$(m "$APKTOOL")" d -f -o "$WORK/starry_dec" "$(m "$STARRY_APK")" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT/com"
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  cp -R "$d/com/starry" "$SMALI_ROOT/com/"
done
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] || { echo "ERROR: 缺 StarryApp.smali"; exit 1; }

# 运行时必须继承官方 App，否则 PermissionActivity/GSC 未 init 秒崩
"$PY" - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/bilibili/priconne/App;"),
    ("invoke-direct {p0}, Landroid/app/Application;-><init>()V",
     "invoke-direct {p0}, Lcom/bilibili/priconne/App;-><init>()V"),
    ("invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
     "invoke-super {p0, p1}, Lcom/bilibili/priconne/App;->attachBaseContext(Landroid/content/Context;)V"),
    ("invoke-super {p0}, Landroid/app/Application;->onCreate()V",
     "invoke-super {p0}, Lcom/bilibili/priconne/App;->onCreate()V"),
]
n = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b); n += 1
if ".super Lcom/bilibili/priconne/App;" not in t:
    raise SystemExit("ERROR: StarryApp.smali .super 未接上官方 App")
# 只禁「父类调用仍落在 Application」；普通 invoke-virtual 调 Application 的公开方法
# （registerActivityLifecycleCallbacks 等）改父类后依然合法，不算漏改。
for bad in ("invoke-direct {p0}, Landroid/app/Application;-><init>()V",
            "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext",
            "invoke-super {p0}, Landroid/app/Application;->onCreate()V"):
    if bad in t:
        raise SystemExit(f"ERROR: StarryApp.smali 仍有父类调用未改: {bad}")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp.smali → com/bilibili/priconne/App ({n} edits)")
PY

java -jar "$(m "$SMALI_JAR")" assemble -o "$WORK/$DEX_NAME" "$SMALI_ROOT" >/dev/null
ls -l "$WORK/$DEX_NAME"
if [ -d "$WORK/starry_dec/assets/certs" ]; then
  mkdir -p "$WORK/assets"
  cp -R "$WORK/starry_dec/assets/certs" "$WORK/assets/certs"
fi

echo "[4/6] zip 外科合并（受保护 dex / resources.arsc / Manifest 逐字节保留）"
"$PY" - "$BASE" "$WORK" "$WORK/unsigned.apk" "${PCR_BINARY_MANIFEST:-}" "$DEX_NAME" <<'PY'
import sys, re, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
dex_name = sys.argv[5]      # 由 next_free_dex.py 算出的 classesN.dex（禁写死）
mf = Path(sys.argv[4]) if len(sys.argv) > 4 and sys.argv[4].strip() else None   # 可选：补丁 Manifest
mf_bytes = mf.read_bytes() if mf is not None else b""
mf_sha = hashlib.sha256(mf_bytes).hexdigest() if mf is not None else ""
c13 = (work / dex_name).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

def is_protected(name):
    if name == "resources.arsc":
        return True
    if name == "AndroidManifest.xml":
        return mf is None      # 提供补丁 Manifest 时改判「产物 == 提供文件」，不参与底包等比
    # 底包所有 classes*.dex 一律逐字节保留（不再只认 0/2..12：序号范围随底包变化）
    m = re.fullmatch(r"classes(\d*)\.dex", name)
    if m:
        mid = m.group(1)
        return mid == "" or int(mid) >= 2
    return False

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name == dex_name or name in extras:
        continue
    data = mf_bytes if (name == "AndroidManifest.xml" and mf is not None) else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

info = zipfile.ZipInfo(dex_name)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c13)

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zi, zo = zipfile.ZipFile(src, "r"), zipfile.ZipFile(dst, "r")
for name in zi.namelist():
    if is_protected(name):
        assert hashlib.sha256(zi.read(name)).hexdigest() == \
               hashlib.sha256(zo.read(name)).hexdigest(), f"PROTECTED MUTATED: {name}"
if mf is not None:
    assert "AndroidManifest.xml" in zo.namelist(), "MISSING: AndroidManifest.xml"
    assert hashlib.sha256(zo.read("AndroidManifest.xml")).hexdigest() == mf_sha, \
           "MANIFEST != PROVIDED: " + mf_sha
zn = set(zo.namelist())
for k in [dex_name] + list(extras):
    assert k in zn, f"MISSING: {k}"
# 新增条目白名单：只允许 classesN.dex + extras（lib/*、assets/certs/*、Manifest override 不算新增）
allowed_new = {dex_name} | set(extras)
surplus = set(zn) - set(zi.namelist()) - allowed_new
assert not surplus, f"UNEXPECTED NEW ENTRIES: {sorted(surplus)}"
prot = "classes*.dex + resources.arsc" + ("" if mf is not None else " + AndroidManifest.xml")
print(f"  合并完成 entries≈{n} {dex_name}={len(c13)}B 新增={len(extras)} 受保护全等（{prot}）")
if mf is not None:
    print(f"  Manifest 取补丁文件 {mf} sha256={mf_sha}（已跳过底包 Manifest）")
for k in sorted(extras):
    print(f"    + {k} {len(extras[k])}B")
zi.close(); zo.close()
PY

echo "[5/6] zipalign"
"$ZIPALIGN" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"

echo "[6/6] apksigner 签名 → $SIGNED"
"$APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$APKSIGNER" verify --print-certs "$SIGNED" | head -5
ls -lh "$SIGNED"

# Manifest 校验行：产物 zip 内 AndroidManifest.xml 是否等于提供的补丁文件
if [ -n "$PCR_BINARY_MANIFEST" ]; then
  "$PY" - "$SIGNED" "$PCR_BINARY_MANIFEST" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
apk, mfp = Path(sys.argv[1]), Path(sys.argv[2])
exp = hashlib.sha256(mfp.read_bytes()).hexdigest()
with zipfile.ZipFile(apk, "r") as z:
    got = hashlib.sha256(z.read("AndroidManifest.xml")).hexdigest()
eq = "YES" if got == exp else "NO"
print(f"[manifest] 来源={mfp} 提供sha256={exp} 产物sha256={got} 与提供文件相等={eq}")
if got != exp:
    raise SystemExit("ERROR: 产物 AndroidManifest.xml 与提供的补丁文件不一致")
PY
else
  echo "[manifest] 未设 PCR_BINARY_MANIFEST，AndroidManifest.xml 沿用底包（逐字节保留）"
fi

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  SERIAL="${SERIAL:-emulator-5554}"
  echo "[install] $SERIAL 覆盖安装（禁 uninstall：热更约 10GB）"
  adb -s "$SERIAL" shell "am force-stop $PKG" 2>/dev/null || true
  adb -s "$SERIAL" push "$SIGNED" /data/local/tmp/pcr_x64.apk >/dev/null
  if ! adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 /data/local/tmp/pcr_x64.apk'"; then
    echo "[install] su 分支失败，回退 adb install 流式安装"
    adb -s "$SERIAL" install -r -d "$SIGNED"
  fi
  adb -s "$SERIAL" shell "su -c 'rm -f /data/local/tmp/pcr_x64.apk'" 2>/dev/null || true
  echo "log: adb -s $SERIAL logcat -s StarryApp:V StarryNativeLoad:V StarryMagic:V Pcr:V MenuService:V"
fi

if [ -n "$PCR_BINARY_MANIFEST" ]; then
  PROT_DESC="底包全部 classes*.dex + resources.arsc"
  NEW_DESC="$DEX_NAME + lib/*/libstarry_magic.so|libc++_shared.so + AndroidManifest.xml(override)"
else
  PROT_DESC="底包全部 classes*.dex + resources.arsc + AndroidManifest.xml"
  NEW_DESC="$DEX_NAME + lib/*/libstarry_magic.so|libc++_shared.so"
fi
echo "[dex] DEX_NAME=$DEX_NAME 受保护断言=全等（$PROT_DESC 逐字节相等）；新增=$NEW_DESC"

echo "DONE: $SIGNED"
