#!/usr/bin/env bash
# ============================================================================
#  surgical_abssnmxt_2.36.sh — 阿比斯少年冒险团（com.wks.abyss 2.36 / vc236 / TapTap）
#                              外科 zip 组装 + 星光菜单 + TapTap 改包法，八段
#
#  底包  02_apks/阿比斯少年冒险团/阿比斯少年冒险团_2.36_taptap_original.apk
#        1,133,253,810 B  md5 8040D8A84B46BDA58365582A9909EB17
#        包 com.wks.abyss v2.36/vc236  Unity 2022.3.62f2 IL2CPP
#        dex: classes.dex(1,826,036) + classes2.dex(8,632,396)，**无 classes3** ⇒ 菜单 dex = classes3.dex
#        lib: arm64-v8a + armeabi-v7a（无 x86）**无 libc++_shared.so** ⇒ 必须自带
#
#  产物  07_patched/阿比斯少年冒险团/surgical_2.36/{classes2.dex,classes3.dex,AndroidManifest.xml,
#        audit_stage1.json,audit_stage2.json,stage1.apk,stage2_unsigned.apk}
#        08_output/阿比斯少年冒险团/阿比斯少年冒险团_2.36_starry_signed.apk
#
#  段（每段失败即 die；ABC_STEPS=1,2,3 只跑指定段，段间幂等可重跑）：
#    1 底包校验（md5 + size + zip 结构 + dex 数量 + ABI + 无壳）
#    2 tap 补丁（abssnmxt_tap_patch.py 就地改 classes2 的 6 个类）→ classes2.dex
#    3 菜单 dex（gradle javac → obfuscate_menu_dex.sh 混淆 → classesN.dex，N 实测）
#    4 二进制 AndroidManifest（StarryInitProvider 首位 + MenuService + 权限）
#    5 外科 zip 置换（raw-span 复制，未改动条目逐字节相同断言）
#    6 加 so（lib/arm64-v8a/libstarry_magic.so + libc++_shared.so；**不动 libil2cpp.so**）
#    7 zipalign + apksigner（01_tools/starry_release.jks）+ 证书钉归一化比对
#    8 门禁（tap verify / framework_entry_audit / check_release_apk / pre_install_gate / 框架隔离）
#
#  ⚠️ 本机（Windows / Git Bash）三条硬经验：
#   a) ndk-build 的 NDK_OUT/NDK_LIBS_OUT **不能**放中文路径：GNU Make 4.3(Win32) 把
#      "mod菜单" 转成 mod??，编译期报 "unable to open output file"。本脚本统一用
#      13_scratch/abssnmxt_ndk/{obj,libs}（纯 ASCII），编完再拷回中文路径 jniLibs。
#   b) 本机 NDK 只有 ndk-build.cmd，$(shell git log) 取不到 epoch ⇒ 显式传
#      STARRY_BUILD_EPOCH=<git log -1 --format=%ct>（否则 pre_install_gate 查8 会拦）。
#   c) java 工具（baksmali/smali/aapt2）一律在 ASCII 暂存目录跑，产物再拷回中文路径。
#
#  用法：
#    bash 04_scripts/阿比斯少年冒险团/surgical_abssnmxt_2.36.sh
#    ABC_STEPS=1,2 bash 04_scripts/阿比斯少年冒险团/surgical_abssnmxt_2.36.sh
#    ABC_SKIP_GRADLE=1 ...   # 复用上次 javac 产物（仅调试；正式出包禁止）
# ============================================================================
set -euo pipefail

_here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../_common/env.sh
. "$_here/../_common/env.sh"

ROOT="$NIXIANG_ROOT"
GAME="阿比斯少年冒险团"
PKG="com.wks.abyss"
VER="2.36"
VC="236"
BASE="$ROOT/02_apks/$GAME/${GAME}_2.36_taptap_original.apk"
BASE_SIZE=1133253810
BASE_MD5="8040d8a84b46bda58365582a9909eb17"
STARRY="$ROOT/mod菜单/starry-mod-menu"
SCRIPTS="$ROOT/04_scripts/$GAME"
TAP_PATCH="$SCRIPTS/abssnmxt_tap_patch.py"
MF_PATCH="$SCRIPTS/abssnmxt_manifest_patch.py"
ZIP_SURG="$SCRIPTS/abssnmxt_zip_surgery.py"
AUTHORITY="$PKG.starryinit"

STAGE="$ROOT/07_patched/$GAME/surgical_2.36"
OUTDIR="$ROOT/08_output/$GAME"
# 成品命名：功能版 abssnmxt_2.36_v1.1.apk / 探针版 abssnmxt_2.36_v1.1_probe.apk（v1.1 = 解析层重写）
if [ "${ABC_SO_VARIANT:-normal}" = "probe" ]; then
  OUTAPK="$OUTDIR/abssnmxt_2.36_${ABC_VER:-v1.1}_probe.apk"
else
  OUTAPK="$OUTDIR/abssnmxt_2.36_${ABC_VER:-v1.1}.apk"
fi
JT="$ROOT/13_scratch/abssnmxt_work"
# so 变体：normal = 功能版；probe = 探针版（-DSTARRY_PROBE=1，只解析+打日志不写内存）
# 用法：ABC_SO_VARIANT=probe bash surgical_abssnmxt_2.36.sh（产物名带 _probe，不覆盖正式包）
SO_VARIANT="${ABC_SO_VARIANT:-normal}"
if [ "$SO_VARIANT" = "probe" ]; then
  NDKOBJ="$ROOT/13_scratch/abssnmxt_ndk/obj_probe"
  NDKLIB="$ROOT/13_scratch/abssnmxt_ndk/libs_probe"
else
  NDKOBJ="$ROOT/13_scratch/abssnmxt_ndk/obj"
  NDKLIB="$ROOT/13_scratch/abssnmxt_ndk/libs"
fi
SHIM_SDK="$ROOT/13_scratch/sdk_shim"

PY="${NIXIANG_PYTHON:-python}"
JAVA17="${NIXIANG_JAVA17:-}"
BT="${NIXIANG_BUILD_TOOLS:-}"
ZIPALIGN="${NIXIANG_ZIPALIGN:-}"
APKSIGNER="${NIXIANG_APKSIGNER:-}"
AAPT2="$BT/aapt2"
[ -f "$AAPT2" ] || AAPT2="$BT/aapt2.exe"
BAKSMALI="$ROOT/01_tools/smali/baksmali"
SMALI="$ROOT/01_tools/smali/smali"
KEYS="$ROOT/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
ANDROID_SDK_HOST="${ANDROID_HOME:-${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk}"

STEPS="${ABC_STEPS:-1,2,3,4,5,6,7,8}"

die()  { echo "[FAIL] $*" >&2; exit 1; }
say()  { echo "$*"; }
hr()   { echo "------------------------------------------------------------"; }
want() { case ",$STEPS," in *",$1,"*) return 0 ;; *) return 1 ;; esac; }

md5of() { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
size_of() { wc -c < "$1" | tr -d ' '; }
sha_of()  { if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}'; else shasum -a 256 "$1" | awk '{print $1}'; fi; }

[ -n "$JAVA17" ] || die "找不到 JDK17（env.sh 的 NIXIANG_JAVA17 为空）"
export JAVA_HOME="$JAVA17"
export PATH="$JAVA_HOME/bin:$PATH"
[ -f "$APKSIGNER" ] || die "找不到 apksigner（NIXIANG_APKSIGNER=${APKSIGNER}）"
[ -f "$ZIPALIGN" ] || die "找不到 zipalign（NIXIANG_ZIPALIGN=${ZIPALIGN}）"

mkdir -p "$STAGE" "$OUTDIR" "$JT" "$NDKOBJ" "$NDKLIB"
say "=== 阿比斯少年冒险团 $VER (vc$VC) 外科组装 8 段 | steps=$STEPS ==="
say "host=$NIXIANG_HOST java17=$JAVA17"
say "build-tools=$BT"

# ===========================================================================
# 段1 底包校验
# ===========================================================================
step1() {
  hr; say "[1/8] 底包校验"
  [ -f "$BASE" ] || die "缺底包 $BASE"
  local sz md5
  sz="$(size_of "$BASE")"; md5="$(md5of "$BASE")"
  [ "$sz" = "$BASE_SIZE" ] || die "底包大小不符 got=$sz want=$BASE_SIZE"
  [ "$md5" = "$BASE_MD5" ] || die "底包 md5 不符 got=$md5 want=$BASE_MD5"
  "$PY" - "$BASE" <<'PY' || exit 1
import sys, zipfile
apk = sys.argv[1]
z = zipfile.ZipFile(apk)
names = z.namelist()
infos = z.infolist()
dex = [n for n in names if n.startswith("classes") and n.endswith(".dex")]
if dex != ["classes.dex", "classes2.dex"]:
    raise SystemExit("ERROR: dex 布局不符: %r（期望 classes.dex/classes2.dex，无 classes3）" % dex)
abis = sorted({n.split("/")[1] for n in names if n.startswith("lib/")})
if abis != ["arm64-v8a", "armeabi-v7a"]:
    raise SystemExit("ERROR: ABI 不符: %r" % abis)
if any("libtprt.so" in n or "libDexHelper" in n or "/libjiagu" in n for n in names):
    raise SystemExit("ERROR: 检出加固库，本脚本按「无壳」前提写的")
if "lib/arm64-v8a/libil2cpp.so" not in names:
    raise SystemExit("ERROR: 缺 libil2cpp.so（不是 Unity IL2CPP 包？）")
print("  entries=%d dex=%s abis=%s STORED=%d" % (
    len(infos), dex, abis, sum(1 for i in infos if i.compress_type == 0)))
print("  [ok] zip 结构 / dex 数量 / ABI / 无壳")
PY
  say "[ok] 底包 size=$sz md5=$md5"
}

# ===========================================================================
# 段2 TapTap 改包法：classes2.dex 就地改 6 个类 → 重新 assemble
# ===========================================================================
step2() {
  hr; say "[2/8] TapTap 改包法（P1–P6）→ classes2.dex"
  [ -f "$TAP_PATCH" ] || die "缺 $TAP_PATCH"
  rm -rf "$JT/c2_smali" "$JT/c2_smali_tap_backup"
  "$PY" - "$BASE" "$JT" <<'PY' || exit 1
import sys, zipfile
apk, jt = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
d = z.read("classes2.dex")
open(jt + "/classes2.dex", "wb").write(d)
print("  classes2.dex %d B 取出" % len(d))
PY
  "$BAKSMALI" d "$JT/classes2.dex" -o "$JT/c2_smali" >/dev/null || die "baksmali 失败"
  say "  baksmali: $(find "$JT/c2_smali" -name '*.smali' | wc -l | tr -d ' ') 个 smali"
  "$PY" "$TAP_PATCH" patch --dec-dir "$JT/c2_smali" --base-apk "$BASE" || die "tap patch 失败"
  "$SMALI" assemble -o "$JT/classes2_patched.dex" "$JT/c2_smali" || die "smali assemble 失败"
  "$PY" - "$JT/classes2.dex" "$JT/classes2_patched.dex" <<'PY' || exit 1
import struct, sys
def hdr(p):
    b = open(p, "rb").read(0x70)
    if b[:4] != b"dex\x0a":
        raise SystemExit("ERROR: %s 不是合法 dex" % p)
    return {k: struct.unpack_from("<I", b, o)[0] for k, o in
            (("strings", 0x38), ("types", 0x40), ("protos", 0x48),
             ("fields", 0x50), ("methods", 0x58))}
a, b = hdr(sys.argv[1]), hdr(sys.argv[2])
print("  orig", a); print("  new ", b)
if b["methods"] >= 65536:
    raise SystemExit("ERROR: method_ids %d 超 65536" % b["methods"])
if b["methods"] < a["methods"]:
    raise SystemExit("ERROR: method_ids 变少（%d -> %d）" % (a["methods"], b["methods"]))
if b["methods"] - a["methods"] > 32:
    raise SystemExit("ERROR: method_ids 增长异常（+%d）" % (b["methods"] - a["methods"]))
print("  [ok] classes2 dex 头部/method 预算正常")
PY
  cp -f "$JT/classes2_patched.dex" "$STAGE/classes2.dex"
  "$PY" "$TAP_PATCH" verify --dec-dir "$JT/c2_smali" || die "tap 补丁断言失败"
  say "[ok] $STAGE/classes2.dex $(size_of "$STAGE/classes2.dex") B md5=$(md5of "$STAGE/classes2.dex")"
}

# ===========================================================================
# 段3 菜单 dex：gradle javac → obfuscate_menu_dex.sh（R8）→ classesN.dex
# ===========================================================================
step3() {
  hr; say "[3/8] 菜单 dex（R8 混淆）"
  local n
  n="$("$PY" "$ROOT/04_scripts/_common/next_free_dex.py" "$BASE")" || die "next_free_dex 失败"
  [ "$n" = "3" ] || die "最小空闲 dex 下标实测=${n}，与预期 3 不符（底包 dex 布局变了，先复核）"
  say "  next_free_dex → classes$n.dex"

  local CLS="$STARRY/app/build/intermediates/javac/release/compileReleaseJavaWithJavac/classes"
  if [ "${ABC_SKIP_GRADLE:-0}" = "1" ] && [ -d "$CLS" ]; then
    say "  [warn] ABC_SKIP_GRADLE=1：复用上次 javac 产物（仅调试）"
  else
    export ANDROID_HOME="$ANDROID_SDK_HOST"
    if [ "$NIXIANG_HOST" = "win" ]; then
      ( cd "$STARRY" && ./gradlew.bat :app:compileReleaseJavaWithJavac --offline --console=plain ) \
        || die "gradle compileReleaseJavaWithJavac 失败"
    else
      ( cd "$STARRY" && ./gradlew :app:compileReleaseJavaWithJavac --offline --console=plain ) \
        || die "gradle compileReleaseJavaWithJavac 失败"
    fi
  fi
  [ -d "$CLS" ] || die "缺 javac 产物目录 $CLS"
  say "  javac classes: $(find "$CLS" -name '*.class' | wc -l | tr -d ' ') 个 .class"

  # obfuscate_menu_dex.sh 要 cmdline-tools 的 r8.jar + platforms/android-*/android.jar；
  # 本机 SDK（01_tools/android-sdk）两者都没有 ⇒ 建 shim：
  #   cmdline-tools/latest/lib/r8.jar ← build-tools/android-15/lib/d8.jar（同一 R8 引擎 8.6.2）
  #   platforms → junction 到 %LOCALAPPDATA%\Android\Sdk\platforms（真 android.jar）
  mkdir -p "$SHIM_SDK/cmdline-tools/latest/lib"
  local d8="$ROOT/01_tools/android-sdk/build-tools/android-15/lib/d8.jar"
  [ -f "$SHIM_SDK/cmdline-tools/latest/lib/r8.jar" ] \
    || cp -f "$d8" "$SHIM_SDK/cmdline-tools/latest/lib/r8.jar"
  if [ ! -d "$SHIM_SDK/platforms" ]; then
    if [ "$NIXIANG_HOST" = "win" ]; then
      cmd //c mklink /J "$(nixiang_win_path "$SHIM_SDK/platforms")" \
        "$(nixiang_win_path "$ANDROID_SDK_HOST/platforms")" >/dev/null || die "建 platforms junction 失败"
    else
      ln -s "$ANDROID_SDK_HOST/platforms" "$SHIM_SDK/platforms"
    fi
  fi
  [ -f "$SHIM_SDK/platforms/android-35/android.jar" ] || die "shim 里找不到 android.jar"
  say "  R8 shim: r8.jar=d8.jar 同引擎 / android.jar=android-35"

  ANDROID_SDK_ROOT="$SHIM_SDK" NIXIANG_JAVA17="$JAVA17" \
    bash "$ROOT/04_scripts/_common/obfuscate_menu_dex.sh" "$CLS" "$STAGE/classes$n.dex" \
    || die "obfuscate_menu_dex 失败"

  "$PY" - "$STAGE/classes$n.dex" <<'PY' || exit 1
import struct, sys
p = sys.argv[1]
d = open(p, "rb").read()
if d[:4] != b"dex\x0a":
    raise SystemExit("ERROR: 菜单 dex 头不合法")
methods = struct.unpack_from("<I", d, 0x58)[0]
if methods >= 65536:
    raise SystemExit("ERROR: 菜单 method_ids %d 超限" % methods)
for bad in (b"Lcom/starry/magic/sign/SignSpoof;", b"SignSpoofAppComponentFactory"):
    if bad in d:
        raise SystemExit("ERROR: 菜单 dex 含已删类 %r" % bad)
for need in (b"StarryInitProvider", b"MenuService", b"ModBridge"):
    if need not in d:
        raise SystemExit("ERROR: 菜单 dex 缺 %r" % need)
print("  [ok] 菜单 dex %d B / methods=%d / 无 SignSpoof / 含 StarryInitProvider+MenuService+ModBridge"
      % (len(d), methods))
PY
  say "[ok] $STAGE/classes$n.dex md5=$(md5of "$STAGE/classes$n.dex")"
}

# ===========================================================================
# 段4 二进制 AndroidManifest（provider 首位 + MenuService + 权限）
# ===========================================================================
step4() {
  hr; say "[4/8] 二进制 AndroidManifest"
  [ -f "$MF_PATCH" ] || die "缺 $MF_PATCH"
  "$PY" - "$BASE" "$JT/manifest_orig.xml" <<'PY' || exit 1
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
open(sys.argv[2], "wb").write(z.read("AndroidManifest.xml"))
print("  AndroidManifest.xml 取出（binary AXML）")
PY
  "$PY" "$MF_PATCH" "$JT/manifest_orig.xml" "$JT/manifest_new.xml" --authority "$AUTHORITY" \
    || die "manifest patch 失败"
  "$PY" - "$JT/manifest_new.xml" "$JT/mf_probe.apk" <<'PY' || exit 1
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$JT/mf_probe.apk" > "$JT/tree_new.txt" 2>/dev/null \
    || die "aapt2 无法解析新 manifest"
  "$PY" - "$JT/tree_new.txt" "$AUTHORITY" <<'PY' || exit 1
import sys
t = open(sys.argv[1], encoding="utf-8", errors="replace").read()
auth = sys.argv[2]
def pos(needle):
    i = t.find(needle)
    if i < 0:
        raise SystemExit("ERROR: manifest 回读缺 %r" % needle)
    return i
p_starry = pos("com.starry.magic.StarryInitProvider")
p_tap = pos("com.taptap.sdk.kit.internal.TapTapKitInitProvider")
p_themis = pos("com.taptap.sdk.themis.lite.ThemisProvider")
if not (p_starry < p_tap and p_starry < p_themis):
    raise SystemExit("ERROR: 入口 provider 没排在 TapTap provider 之前")
for need in (auth, "com.starry.magic.menu.MenuService",
             "android.permission.SYSTEM_ALERT_WINDOW",
             "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
             "androidx.core.app.CoreComponentFactory",
             "com.u8.sdk.U8Application",
             "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"):
    if need not in t:
        raise SystemExit("ERROR: manifest 回读缺 %s" % need)
print("  [ok] provider 顺序 / MenuService / 权限 / 官方 application+appComponentFactory 均在")
PY
  cp -f "$JT/manifest_new.xml" "$STAGE/AndroidManifest.xml"
  say "[ok] $STAGE/AndroidManifest.xml $(size_of "$STAGE/AndroidManifest.xml") B"
}

# ===========================================================================
# 段5 外科 zip 置换（raw-span 复制，未改动条目逐字节相同）
# ===========================================================================
step5() {
  hr; say "[5/8] 外科 zip 置换（Manifest + classes2.dex + classes3.dex）"
  [ -f "$ZIP_SURG" ] || die "缺 $ZIP_SURG"
  local menu="$STAGE/classes3.dex"
  [ -f "$STAGE/classes2.dex" ] || die "缺 $STAGE/classes2.dex（先跑段2）"
  [ -f "$STAGE/AndroidManifest.xml" ] || die "缺 $STAGE/AndroidManifest.xml（先跑段4）"
  [ -f "$menu" ] || die "缺 ${menu}（先跑段3）"
  local t0 elapsed
  t0=$(date +%s)
  "$PY" "$ZIP_SURG" --base "$BASE" --out "$STAGE/stage1.apk" \
     --replace "AndroidManifest.xml=$STAGE/AndroidManifest.xml" \
     --replace "classes2.dex=$STAGE/classes2.dex" \
     --add "classes3.dex=$menu" \
     --drop-signatures --audit "$STAGE/audit_stage1.json" --verify-unchanged \
     || die "外科组装（stage1）失败"
  elapsed=$(( $(date +%s) - t0 ))
  say "  stage1: $(size_of "$STAGE/stage1.apk") B  ${elapsed}s"
  "$PY" - "$STAGE/audit_stage1.json" <<'PY' || exit 1
import json, sys
a = json.load(open(sys.argv[1], encoding="utf-8"))
acts = {}
for e in a["audit"]:
    acts[e["action"]] = acts.get(e["action"], 0) + 1
print("  audit: %r" % acts)
if acts.get("replace") != 2 or acts.get("add") != 1:
    raise SystemExit("ERROR: 置换/新增条目数异常 %r" % acts)
if "classes.dex" not in [e["name"] for e in a["audit"] if e["action"] == "copy"]:
    raise SystemExit("ERROR: classes.dex 未按原样复制")
PY
}

# ===========================================================================
# 段6 加 so（静态链 STL：libstarry_magic.so 单文件，不再依赖 libc++_shared.so）
#   APP_STL 在 Application.mk 里是 app 级设置（c++_shared），本游戏不改共享文件 ⇒
#   用 13_scratch 下的私有 app mk 覆盖成 c++_static（NDK_APPLICATION_MK 指绝对路径）。
#   静态链后：llvm-readelf -d 的 NEEDED 只剩 liblog/libandroid/libc/libm/libdl，
#   包内不需要 libc++_shared.so（底包本来也没有），杜绝 dlopen 找不到 STL 的风险。
#   不动 libil2cpp.so（直装包只换能换的）。
# ===========================================================================
step6() {
  hr; say "[6/8] 加 native so（c++_static 静态链）"
  local NDK="$ROOT/01_tools/android-sdk/ndk/android-ndk-r27c"
  local SO_OBJ="$NDKLIB/arm64-v8a/libstarry_magic.so"
  local EPOCH
  EPOCH="$(cd "$ROOT" && git log -1 --format=%ct 2>/dev/null | tr -cd '0-9')"
  [ -n "$EPOCH" ] || die "取不到 git epoch（STARRY_BUILD_EPOCH 必需；pre_install_gate 查8）"
  say "  STARRY_BUILD_EPOCH=$EPOCH"

  # 私有 app mk：STL 静态链（共享的 Application.mk 不改）
  local APPMK="$ROOT/13_scratch/app_abssnmxt_static.mk"
  mkdir -p "$(dirname "$APPMK")"
  {
    echo "APP_ABI := arm64-v8a"
    echo "APP_PLATFORM := android-21"
    echo "# 静态链 STL：so 单文件，NEEDED 里不再有 libc++_shared.so"
    echo "APP_STL := c++_static"
    echo "APP_CPPFLAGS := -std=c++17 -fexceptions -fpermissive -DSTARRY_BUILD_EPOCH=$EPOCH"
  } > "$APPMK"
  say "  app mk: ${APPMK}（APP_STL=c++_static）"
  # ★换 STL 必须清 obj：ndk-build 不跟踪编译开关，旧 .o 里仍是 c++_shared 引用
  #   ⇒ 链接后 NEEDED 又出现 libc++_shared.so（本段末尾的断言会拦，但先清更省一轮）
  rm -rf "$NDKOBJ" "$NDKLIB"
  say "  ndk-build: APP_BUILD_SCRIPT=./Android_abssnmxt.mk NDK_OUT=$NDKOBJ NDK_LIBS_OUT=$NDKLIB"
  local PROBE_ARG=""
  [ "$SO_VARIANT" = "probe" ] && PROBE_ARG="STARRY_PROBE=1"
  # 额外 make 变量（例：ABC_MAKE_EXTRA=STARRY_SPEED_ONLY=1 出移速实验版）
  local EXTRA_ARG="${ABC_MAKE_EXTRA:-}"
  ( cd "$STARRY/app/src/main/jni" && \
    if [ "$NIXIANG_HOST" = "win" ]; then
      "$NDK/ndk-build.cmd" NDK_PROJECT_PATH="$STARRY/app" \
        APP_BUILD_SCRIPT=./Android_abssnmxt.mk NDK_APPLICATION_MK="$APPMK" \
        NDK_OUT="$NDKOBJ" NDK_LIBS_OUT="$NDKLIB" $PROBE_ARG $EXTRA_ARG -j4
    else
      "$NDK/ndk-build" NDK_PROJECT_PATH="$STARRY/app" \
        APP_BUILD_SCRIPT=./Android_abssnmxt.mk NDK_APPLICATION_MK="$APPMK" \
        NDK_OUT="$NDKOBJ" NDK_LIBS_OUT="$NDKLIB" $PROBE_ARG -j4
    fi ) || die "ndk-build 失败"
  [ -f "$SO_OBJ" ] || die "缺构建产物 $SO_OBJ"
  if [ -f "$NDKLIB/arm64-v8a/libc++_shared.so" ]; then
    die "产物目录出现 libc++_shared.so ⇒ APP_STL 没被覆盖成 c++_static"
  fi

  # 标签/变体/STL 依赖三查
  "$PY" - "$SO_OBJ" <<'PY' || exit 1
import sys
d = open(sys.argv[1], "rb").read()
for need in (b"[Abssnmxt]", b"abssnmxt"):
    if need not in d:
        raise SystemExit("ERROR: so 内缺 %r（旧 so / 编错 mk）" % need)
for bad in (b"[Hkld]", b"hkld", b"[Neon]", b"[Tlgd]", b"[Sc33]", b"[Nhsy2]"):
    if bad in d:
        raise SystemExit("ERROR: so 内含上一款标签 %r" % bad)
if b"STARRYSTAMP1:EPOCH=" not in d:
    raise SystemExit("ERROR: so 内无 OTA 构建戳 STARRYSTAMP1:EPOCH=")
import os
want_probe = os.environ.get("ABC_SO_VARIANT", "normal") == "probe"
has_probe = b"PROBE_BUILD=1" in d
if want_probe != has_probe:
    raise SystemExit("ERROR: so 变体不符（期望 %s，实际 PROBE_BUILD=%s）"
                     % ("probe" if want_probe else "normal", has_probe))
print("  [ok] so 标签 [Abssnmxt] 在、旧标签不在、含 OTA 戳、变体=%s"
      % ("probe" if has_probe else "normal"))
PY
  local RD="$NDK/toolchains/llvm/prebuilt/windows-x86_64/bin/llvm-readelf.exe"
  [ -f "$RD" ] || RD="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-readelf"
  if [ -f "$RD" ]; then
    local NEEDED
    NEEDED="$("$RD" -d "$SO_OBJ" | awk '/NEEDED/ {gsub(/[\[\]]/,"",$NF); print $NF}' | tr '\n' ' ')"
    say "  NEEDED: $NEEDED"
    case " $NEEDED " in
      *" libc++_shared.so "*) die "so 仍动态依赖 libc++_shared.so（静态链没生效）" ;;
    esac
    say "  [ok] 无 libc++_shared.so 依赖（静态链 STL）"
  else
    say "  [warn] 无 llvm-readelf，跳过 NEEDED 检查"
  fi
  local NM="$NDK/toolchains/llvm/prebuilt/windows-x86_64/bin/llvm-nm.exe"
  [ -f "$NM" ] || NM="$NDK/toolchains/llvm/prebuilt/windows-x86_64/bin/llvm-nm"
  if [ -f "$NM" ]; then
    if "$NM" -D --undefined-only "$SO_OBJ" | grep -q emutls; then
      die "so 依赖 __emutls_get_address（缺 -fno-emulated-tls）"
    fi
    say "  [ok] 无 emutls 未定义符号"
  fi
  cp -f "$SO_OBJ" "$STARRY/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
  say "  jniLibs 同步：libstarry_magic.so $(size_of "$SO_OBJ") B（单文件，无 STL so）"

  local t0 elapsed
  t0=$(date +%s)
  "$PY" "$ZIP_SURG" --base "$STAGE/stage1.apk" --out "$STAGE/stage2_unsigned.apk" \
     --add "lib/arm64-v8a/libstarry_magic.so=$SO_OBJ" \
     --audit "$STAGE/audit_stage2.json" --verify-unchanged \
     || die "外科组装（stage2：加 so）失败"
  elapsed=$(( $(date +%s) - t0 ))
  say "  stage2: $(size_of "$STAGE/stage2_unsigned.apk") B  ${elapsed}s"

  "$PY" - "$STAGE/stage2_unsigned.apk" "$BASE" "$SO_OBJ" <<'PY' || exit 1
import sys, zipfile, hashlib
out, base, so = sys.argv[1], sys.argv[2], sys.argv[3]
zo, zb = zipfile.ZipFile(out), zipfile.ZipFile(base)
def h(z, n):
    return hashlib.md5(z.read(n)).hexdigest()
have = hashlib.md5(open(so, "rb").read()).hexdigest()
inapk = h(zo, "lib/arm64-v8a/libstarry_magic.so")
if inapk != have:
    raise SystemExit("ERROR: 包内 so %s != 构建产物 %s" % (inapk, have))
for n in ("lib/arm64-v8a/libil2cpp.so", "lib/armeabi-v7a/libil2cpp.so",
          "lib/arm64-v8a/libunity.so", "classes.dex"):
    if h(zo, n) != h(zb, n):
        raise SystemExit("ERROR: %s 被改动（直装包不得只换 libil2cpp）" % n)
if "lib/x86_64/libstarry_magic.so" in zo.namelist():
    raise SystemExit("ERROR: 误塞 x86_64 so（底包无 x86）")
if "lib/arm64-v8a/libc++_shared.so" in zo.namelist():
    raise SystemExit("ERROR: 包内不该有 libc++_shared.so（静态链后不需要）")
print("  [ok] 包内 so==构建产物；libil2cpp/libunity/classes.dex 与底包逐字节相同；无 STL so")
PY
}
# ===========================================================================
# 段7 zipalign + apksigner + 证书钉归一化比对
# ===========================================================================
step7() {
  hr; say "[7/8] zipalign + apksigner"
  [ -f "$KEYS" ] || die "缺签名库 $KEYS"
  local aligned="$STAGE/stage2_aligned.apk"
  "$ZIPALIGN" -f -p 4 "$STAGE/stage2_unsigned.apk" "$aligned" || die "zipalign 失败"
  say "  zipalign ok: $(size_of "$aligned") B"
  "$APKSIGNER" sign --ks "$KEYS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
    --out "$OUTAPK" "$aligned" || die "apksigner sign 失败"
  "$APKSIGNER" verify --print-certs "$OUTAPK" > "$JT/certs.txt" 2>&1 || die "apksigner verify 失败"
  local got want
  got="$("$PY" - "$JT/certs.txt" <<'PY'
import re, sys
t = open(sys.argv[1], encoding="utf-8", errors="replace").read()
m = re.search(r"certificate MD5 digest:\s*([0-9a-fA-F]{32})", t)
print(m.group(1).lower() if m else "")
PY
)"
  # 注：JDK17 的 keytool -list -v 不再打印 MD5（只有 SHA1/SHA256）⇒ 用 exportcert 拿 DER 自己算 MD5，
  # 与 apksigner 的 "certificate MD5 digest" 同源（都是证书 DER 的 MD5），再做归一化比对。
  local kt="$JAVA_HOME/bin/keytool"
  [ -f "$kt" ] || kt="$JAVA_HOME/bin/keytool.exe"
  want="$("$kt" -exportcert -alias "$KS_ALIAS" -keystore "$KEYS" -storepass "$KS_PASS" 2>/dev/null \
    | "$PY" -c "import sys,hashlib;d=sys.stdin.buffer.read();print(hashlib.md5(d).hexdigest() if d else '')")"
  [ -n "$got" ] || die "取不到成品证书 MD5"
  [ -n "$want" ] || die "取不到 keystore 证书 MD5"
  [ "$got" = "$want" ] || die "证书钉不符 got=$got want=$want"
  say "  [ok] 证书钉：成品=$got == starry_release.jks（归一化后比对）"
  "$AAPT2" dump badging "$OUTAPK" > "$JT/badging.txt" 2>/dev/null || die "aapt2 badging 失败"
  "$PY" - "$JT/badging.txt" "$PKG" "$VC" <<'PY' || exit 1
import sys
t = open(sys.argv[1], encoding="utf-8", errors="replace").read()
pkg, vc = sys.argv[2], sys.argv[3]
if ("package: name='%s'" % pkg) not in t:
    raise SystemExit("ERROR: 包名不对（应 %s）" % pkg)
if ("versionCode='%s'" % vc) not in t:
    raise SystemExit("ERROR: versionCode 不是 %s" % vc)
print("  [ok] badging: 包名/versionCode 与底包一致")
PY
  cp -f "$OUTAPK" "$ROOT/07_patched/$GAME/$(basename "$OUTAPK")"
  say "  [ok] 同步副本 → 07_patched/$GAME/$(basename "$OUTAPK")"
  say "[ok] 成品 $OUTAPK"
  say "     size=$(size_of "$OUTAPK") B  md5=$(md5of "$OUTAPK")"
  say "     sha256=$(sha_of "$OUTAPK")"
}

# ===========================================================================
# 段8 门禁
# ===========================================================================
step8() {
  hr; say "[8/8] 门禁"
  [ -f "$OUTAPK" ] || die "缺成品 ${OUTAPK}（先跑段7）"
  local rc=0
  say "-- 8.1 tap verify"
  "$PY" "$TAP_PATCH" verify --apk "$OUTAPK" || rc=1
  say "-- 8.2 framework_entry_audit（走豁免包装器：modern 注册表/私有 mk 两条为已知无害偏离）"
  bash "$ROOT/04_scripts/_common/audit_with_waivers.sh" --game "$GAME" --pkg "$PKG" --apk "$OUTAPK" || rc=1
  say "-- 8.3 check_release_apk"
  bash "$ROOT/04_scripts/_common/check_release_apk.sh" --apk "$OUTAPK" --game "$GAME" --expect-vc "$VC" || rc=1
  say "-- 8.4 pre_install_gate"
  bash "$ROOT/04_scripts/_common/pre_install_gate.sh" --apk "$OUTAPK" --base-apk "$BASE" \
    --menu-dex "$OUTAPK" --menu-dex-name classes3.dex --game "$GAME" || rc=1
  say "-- 8.5 check_framework_isolation"
  bash "$ROOT/04_scripts/_common/check_framework_isolation.sh" || rc=1
  say "-- 8.6 menu_tpl_gate（javac 管线无 dex 构建戳 ⇒ WARN 放行，非 --require-stamp）"
  bash "$ROOT/04_scripts/_common/menu_tpl_gate.sh" --apk "$OUTAPK" --dex-name classes3.dex || rc=1
  [ "$rc" = "0" ] || die "门禁未全过（见上）"
  say "[ok] 门禁全过"
}

# ===========================================================================
for i in 1 2 3 4 5 6 7 8; do
  want "$i" || continue
  "step$i"
done
hr
say "DONE. steps=$STEPS"
if [ -f "$OUTAPK" ]; then
  say "产物: $OUTAPK"
  say "      $(size_of "$OUTAPK") B  md5=$(md5of "$OUTAPK")"
fi