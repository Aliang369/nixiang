#!/usr/bin/env bash
# 独奏骑士 1.1.768 —— 官方底包 → 星光外科包（Windows / Git Bash 版）
#
# 蓝本：scripts/surgical_dzqs_1.1.768.sh（mac 版 461 行）。本文件只做平台适配，
# 原脚本的断言、门禁、逐条目 zip 审计一条不减少；平台差异统一用 "WIN:" 注释标出。
#
# 产物：08_output/独奏骑士/独奏骑士_1.1.768_surgical-releaseSigned.apk
# 中间产物：13_scratch/dzqs_build/（work/ 每次重建，日志留在同一目录）
# 不装机：本脚本只出包；STARRY_INSTALL 一律拒绝（装机是独立步骤、需用户确认）。
#
# 用法：bash scripts/surgical_dzqs_1.1.768_win.sh
set -euo pipefail

if [[ "${STARRY_INSTALL:-0}" != "0" ]]; then
  echo "ERROR: 本脚本只出包不装机（不支持 STARRY_INSTALL=1）"; exit 1
fi

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
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../../.." && pwd)}"
ORIG_APK="${DZQS_ORIG_APK:-$NIXIANG/02_apks/独奏骑士_1.1.768.apk}"
OUT="$NIXIANG/08_output/独奏骑士"
WORK="$NIXIANG/13_scratch/dzqs_build"
B="$WORK/work"

# WIN: 官方底包固定在 02_apks（mac 版取 09_test/独奏骑士/base_1.1.768/base.apk）
ORIG_MD5="ccf1b28bf7b4b9d9a950bc7a86fbaf9e"
ORIG_ENTRIES=1907
ORIG_CLASSES_MD5="087d3b21fbd88fe3bf6bdf08068caa38"
ORIG_IL2CPP_MD5="26a4e1174bacf2fd836bd4e833f9572b"
ORIG_CERT_MD5="6870e083bf4136acf4d0d4b132d5dccd"
# 1.1.768 官方 crcCode：classes.dex CRC 和 + arm64 原生库内容 CRC 的 MD5。
OFFICIAL_CRC_CODE="34259c1947fde1c2e7fe5f3373a1054d"
EXPECTED_CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

SMALI_JAR="$NIXIANG/01_tools/smali/smali-fat.jar"
BAKSMALI_JAR="$NIXIANG/01_tools/smali/baksmali-fat.jar"
APKTOOL_JAR="$NIXIANG/01_tools/apktool/apktool.jar"
SMALI() { java -jar "$SMALI_JAR" "$@"; }
BAKSMALI() { java -jar "$BAKSMALI_JAR" "$@"; }
APKTOOL() { java -jar "$APKTOOL_JAR" "$@"; }
KS="$NIXIANG/01_tools/starry_release.jks"

# WIN: 工具链定点到本机实测可用版本（不用 /usr/libexec/java_home、不用 mac SDK 路径）
# WIN: 环境里可能已有 Windows 形态的 JAVA_HOME（C:\\...\\hotspot\\），统一转成 MSYS 形态
if [[ -n "${JAVA_HOME:-}" ]]; then
  JAVA_HOME="$(cygpath -u "$JAVA_HOME")"
else
  JAVA_HOME="/c/Program Files/Microsoft/jdk-17.0.20.101-hotspot"
fi
export JAVA_HOME
export PATH="$JAVA_HOME/bin:$PATH"
NDK="${ANDROID_NDK_HOME:-/c/Users/Administrator/AppData/Local/Android/Sdk/ndk/27.0.12077973}"
NDK_BUILD="$NDK/build/ndk-build"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
# WIN: 本机 PATH 无 make，ndk-build 用 NDK 自带 make.exe
GNUMAKE="${GNUMAKE:-$NDK/prebuilt/windows-x86_64/bin/make.exe}"
export GNUMAKE
BT="${STARRY_BUILD_TOOLS:-/c/Users/Administrator/AppData/Local/Android/Sdk/build-tools/35.0.0}"
SDK_DIR="/c/Users/Administrator/AppData/Local/Android/Sdk"

PKG="com.Shimmer.SoloKnight.cn"
SIGNED="$OUT/独奏骑士_1.1.768_surgical-releaseSigned.apk"
if [[ "$BUILD_TYPE" == "debug" ]]; then SIGNED="$OUT/独奏骑士_1.1.768_surgical-debugSigned.apk"; fi
MF_CACHE="$NIXIANG/07_patched/独奏骑士/surgical_1.1.768/AndroidManifest.xml"
# WIN: 签名别名/口令沿用 mac 版 apksigner 行的取值（starry / starry2026）
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

PASS() { echo "  PASS  $*"; }
FAIL() { echo "  FAIL  $*" >&2; exit 1; }
STEP() { echo; echo "== $* =="; }
md5f() { md5sum "$1" | cut -d' ' -f1; }

# WIN: apksigner 只有 .bat；.bat 需要 Windows 形态 JAVA_HOME，用 subshell 局部覆盖
apksigner_bat() {
  local jh; jh="$(cygpath -w "$JAVA_HOME")"
  ( JAVA_HOME="$jh"; "$BT/apksigner.bat" "$@" )
}

STEP "[0/9] 预检（底包指纹 + 工具链）"
[[ -f "$ORIG_APK" ]] || FAIL "missing $ORIG_APK"
[[ -f "$SMALI_JAR" && -f "$BAKSMALI_JAR" && -f "$APKTOOL_JAR" ]] || FAIL "smali/baksmali/apktool jar 缺失"
[[ -x "$BT/zipalign.exe" && -f "$BT/apksigner.bat" ]] || FAIL "zipalign/apksigner.bat 缺失（${BT}）"
[[ -x "$NDK_BUILD" || -f "$NDK/ndk-build.cmd" ]] || FAIL "missing ndk-build / ndk-build.cmd"
[[ -f "$KS" ]] || FAIL "missing $KS"
got="$(md5f "$ORIG_APK")"
[[ "$got" == "$ORIG_MD5" ]] || FAIL "底包 md5 $got != $ORIG_MD5"
PASS "底包 md5 = $ORIG_MD5 ($(stat -c %s "$ORIG_APK") B)"
python3.11 - "$ORIG_APK" "$ORIG_ENTRIES" "$ORIG_CLASSES_MD5" "$ORIG_IL2CPP_MD5" <<'PY'
import hashlib, sys, zipfile
apk, entries, c_md5, so_md5 = sys.argv[1], int(sys.argv[2]), sys.argv[3], sys.argv[4]
z = zipfile.ZipFile(apk)
names = z.namelist()
assert len(names) == entries, f"entries {len(names)} != {entries}"
assert len(set(names)) == len(names), "duplicate zip entries"
bad = [n for n in names if not n.isascii()]
assert not bad, f"non-ascii names: {bad[:5]}"
assert sum(1 for n in names if n.endswith(".dex")) == 1, "底包应只有 classes.dex"
h = lambda n: hashlib.md5(z.read(n)).hexdigest()
assert h("classes.dex") == c_md5, "classes.dex md5 不符"
assert h("lib/arm64-v8a/libil2cpp.so") == so_md5, "libil2cpp.so md5 不符"
print(f"  PASS  条目 {len(names)} / 全 ASCII / 单 dex / classes.dex+libil2cpp 指纹一致")
PY

mkdir -p "$WORK" "$OUT" "$(dirname "$MF_CACHE")"
LOCK_DIR="$WORK/.lock"
mkdir "$LOCK_DIR" 2>/dev/null || FAIL "lock $LOCK_DIR held"
echo $$ >"$LOCK_DIR/pid"
trap 'rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$B"
# WIN: 只清本轮输出物，保留 work/project(含 obj) 与 work/starry 的增量构建目录 ——
# 单次命令有时长上限，重跑必须仍是「完整跑一遍断言」而不是跳过步骤，所以不动增量目录。
rm -rf "$B/project/jni" "$B/project_jni" "$B/c1smali" "$B/smali2" "$B/starry_dec" \
       "$B/mini_dec" "$B/classes.dex" "$B/classes2.dex" "$B/classes_orig.dex" \
       "$B/unsigned.apk" "$B/aligned.apk" "$B/signed.apk" "$B/AndroidManifest.xml" \
       "$B/check.so" "$B/mf_only.apk"

STEP "[1/9] ndk-build → libstarry_magic.so（STARRY_GAME_DZQS=1）"
# WIN: 工程在中文路径下，ndk-build 会产出乱码目录 → 先把 jni 树复制到 ASCII 的 $B/project
cp -a "$STARRY_SRC/app/src/main/jni" "$B/project_jni"
mkdir -p "$B/project"
mv "$B/project_jni" "$B/project/jni"
[[ -f "$B/project/jni/Application.mk" ]] || FAIL "Application.mk 缺失"
# WIN: 公共 Android.mk 声明的预编译库旧名（Games/WxsjPatchedDll_hf2.a|hf3.a）在本机不存在，
# 真实文件是 Games/WxsjVc1637Hf2Dll.a / WxsjVc1637Hf3Dll.a。**只改临时副本的 Android.mk 两行**，
# 仓库源文件不动（AGENTS §7：公共 Android.mk 禁改）。
if grep -q 'Games/WxsjPatchedDll_hf[23]\.a' "$B/project/jni/Android.mk"; then
  sed -i 's#Games/WxsjPatchedDll_hf2\.a#Games/WxsjVc1637Hf2Dll.a#; s#Games/WxsjPatchedDll_hf3\.a#Games/WxsjVc1637Hf3Dll.a#' \
    "$B/project/jni/Android.mk"
  diff -u "$STARRY_SRC/app/src/main/jni/Android.mk" "$B/project/jni/Android.mk" >"$B/android_mk_copy.diff" || true
  python3.11 - "$B/android_mk_copy.diff" <<'PY'
from pathlib import Path
import sys
lines = Path(sys.argv[1]).read_text(encoding="utf-8", errors="replace").splitlines()
chg = [l for l in lines if (l.startswith("+") or l.startswith("-")) and not l.startswith(("+++", "---"))]
minus = [l for l in chg if l.startswith("-")]
plus = [l for l in chg if l.startswith("+")]
assert len(minus) == 2 and len(plus) == 2, f"副本 Android.mk 应只改 2 行，实际 -{len(minus)}/+{len(plus)}"
assert "WxsjPatchedDll_hf2.a" in minus[0] and "WxsjVc1637Hf2Dll.a" in plus[0], chg
assert "WxsjPatchedDll_hf3.a" in minus[1] and "WxsjVc1637Hf3Dll.a" in plus[1], chg
print("  PASS  副本 Android.mk 仅 2 行改动（仓库源文件未改）：")
for l in chg:
    print("   ", l)
PY
else
  if grep -q 'filter-out -DSTARRY_GAME_%' "$B/project/jni/Android.mk"; then
    echo "  PASS  仓库 Android.mk 已是通配 filter-out（-DSTARRY_GAME_%），无 wxsj 旧名 → 副本无需改动"
  else
    echo "  WARN  仓库 Android.mk 既无 wxsj 旧名、也无通配 filter-out，需人工确认激活段"
  fi
fi
NPROC="$(nproc 2>/dev/null || echo 4)"
# WIN: Git Bash 下 sh 版 ndk-build 无 make/HOST_NULL 语义，走官方 Windows 封装 ndk-build.cmd
NDK_BUILD_WIN="$NDK/ndk-build.cmd"
if [[ -f "$NDK_BUILD_WIN" ]]; then NDK_BUILD_BIN="$NDK_BUILD_WIN"; else NDK_BUILD_BIN="$NDK_BUILD"; fi
( cd "$B/project" && STARRY_GAME_DZQS=1 "$NDK_BUILD_BIN" \
    NDK_PROJECT_PATH=. APP_BUILD_SCRIPT=./jni/Android.mk NDK_APPLICATION_MK=./jni/Application.mk \
    NDK_OUT=./obj NDK_LIBS_OUT=./libs -j"$NPROC" ) >"$B/ndk.log" 2>&1 \
  || { tail -40 "$B/ndk.log"; FAIL "ndk-build 失败（log: $B/ndk.log）"; }
tail -3 "$B/ndk.log"
SO_BUILT="$B/project/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || FAIL "missing $SO_BUILT"
grep -a -q '\[Dzqs\]' "$SO_BUILT" || FAIL "so 不含 [Dzqs]（可能仍是上一款游戏）"
grep -a -qi 'dzqs' "$SO_BUILT" || FAIL "so 不含 dzqs 标识"
if grep -a -q '\[Tlgd\]' "$SO_BUILT"; then FAIL "so 仍含 [Tlgd]"; fi
if grep -a -q '\[Dwct\]' "$SO_BUILT"; then FAIL "so 仍含 [Dwct]"; fi
# 回归门禁（2026-11-27 定）：鉴权 game_id 必须落到 dzqs。曾因 Android.mk 291 行只 filter-out WDYZ，
# 公共段激活的 -DSTARRY_GAME_ZHMR 混进 DZQS 构建 ⇒ GameConfig.h 先命中 ZHMR ⇒ auth game_id=zhmr。
python3.11 - "$SO_BUILT" <<'PY'
import sys
b = open(sys.argv[1], "rb").read()
c = {k.decode(): b.count(k) for k in (b"dzqs", b"zhmr", b"[Dzqs]", b"Toggle_")}
print("  so 字节计数: dzqs=%d zhmr=%d [Dzqs]=%d Toggle_=%d" % (c["dzqs"], c["zhmr"], c["[Dzqs]"], c["Toggle_"]))
if b"resolve enter-interp failed" not in b:
    raise SystemExit("ERROR: so 缺 'resolve enter-interp failed' 日志串（DzqsMod 不是同一份实现）")
if c["dzqs"] < 1:
    raise SystemExit("ERROR: so 缺 dzqs（鉴权 game_id 会错成别的游戏）")
if c["zhmr"] != 0:
    raise SystemExit("ERROR: so 残留 zhmr ⇒ Android.mk 未滤掉 -DSTARRY_GAME_ZHMR")
if c["[Dzqs]"] < 1:
    raise SystemExit("ERROR: so 缺 [Dzqs] 标签")
if c["Toggle_"] != 3:
    raise SystemExit(f"ERROR: so Toggle_ 数量 {c['Toggle_']} != 3（菜单项会缺）")
PY
PASS "so 指纹：$(stat -c %s "$SO_BUILT") B md5=$(md5f "$SO_BUILT")；含 [Dzqs]、game_id=dzqs、无 zhmr"

STEP "[2/9] gradle ${GRADLE_TASK}（离线优先）"
mkdir -p "$B/starry"
python3.11 - "$STARRY_SRC" "$B/starry" <<'PY'
import shutil, sys
src, dst = sys.argv[1], sys.argv[2]
ignore = shutil.ignore_patterns(".git", ".gradle", "build", "obj_*", "libs_*",
                                "_dbglibs", "_dbgobj", ".idea", "*.iml")
shutil.copytree(src, dst, ignore=ignore, dirs_exist_ok=True)
print("  copied starry source → ASCII work tree")
PY
# WIN: 仓库 local.properties 指向 mac SDK，只改写副本（不碰仓库文件）
printf 'sdk.dir=%s\n' "$(cygpath -w "$SDK_DIR" | sed 's/\\/\\\\/g')" > "$B/starry/local.properties"
for dest in "$B/starry/app/src/main/jniLibs/arm64-v8a" \
            "$B/starry/app/src/main/libs/arm64-v8a" \
            "$B/starry/app/libs/arm64-v8a"; do
  mkdir -p "$dest"; cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
NET_RETRY=0
if ! ( cd "$B/starry" && ./gradlew "$GRADLE_TASK" --console=plain --offline ) >"$B/gradle.log" 2>&1; then
  echo "  WARN  离线构建失败，允许联网重试一次（记录：NET_RETRY=1）"
  NET_RETRY=1
  ( cd "$B/starry" && ./gradlew "$GRADLE_TASK" --console=plain ) >"$B/gradle.log" 2>&1 \
    || { tail -40 "$B/gradle.log"; FAIL "gradle $GRADLE_TASK 失败（log: $B/gradle.log）"; }
fi
tail -3 "$B/gradle.log"
STARRY_APK="$B/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || FAIL "missing $STARRY_APK"
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$B/check.so"
grep -a -q '\[Dzqs\]' "$B/check.so" || FAIL "gradle APK 内 so 不是 DZQS —— 菜单会错成上一款游戏"
PASS "菜单 APK $(stat -c %s "$STARRY_APK") B；包内 so [Dzqs] 已核（NET_RETRY=${NET_RETRY}）"

STEP "[3/9] apktool d 菜单 APK → com/starry smali → classes2.dex（StarryApp → SGGlobalApplication）"
APKTOOL d -f -o "$B/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$B/smali2"
mkdir -p "$SMALI_ROOT/com"
shopt -s nullglob
for d in "$B/starry_dec"/smali "$B/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  cp -R "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] || FAIL "StarryApp.smali missing"
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/sgsdk/core/SGGlobalApplication;"),
    ("invoke-direct {p0}, Landroid/app/Application;-><init>()V",
     "invoke-direct {p0}, Lcom/sgsdk/core/SGGlobalApplication;-><init>()V"),
    ("invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
     "invoke-super {p0, p1}, Lcom/sgsdk/core/SGGlobalApplication;->attachBaseContext(Landroid/content/Context;)V"),
    ("invoke-super {p0}, Landroid/app/Application;->onCreate()V",
     "invoke-super {p0}, Lcom/sgsdk/core/SGGlobalApplication;->onCreate()V"),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if changed != 4:
    raise SystemExit(f"ERROR: StarryApp.smali 只命中 {changed}/4 处 Application 引用")
if "Lcom/sgsdk/core/SGGlobalApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing SGGlobalApplication")
p.write_text(t, encoding="utf-8")
print("  StarryApp → SGGlobalApplication ok (4/4 edits)")
PY
SMALI assemble -o "$B/classes2.dex" "$SMALI_ROOT"
[[ -s "$B/classes2.dex" ]] || FAIL "classes2.dex 未生成"
python3.11 - "$B/classes2.dex" <<'PY'
import sys
b = open(sys.argv[1], "rb").read()
assert b[:4] == b"dex\n", "不是 dex"
for n in (b"Lcom/starry/magic/StarryApp;", b"Lcom/sgsdk/core/SGGlobalApplication;",
          b"Lcom/starry/magic/menu/MenuService;"):
    assert n in b, f"classes2.dex 缺 {n!r}"
bad = [x for x in (b"Lcom/itaotuo/shanhelvtan/AomiApplication;", b"[Shlt]", b"[Tlgd]", b"[Dwct]") if x in b]
assert not bad, f"classes2.dex 残留其他游戏引用: {bad}"
print(f"  PASS  classes2.dex {len(b)} B：StarryApp→SGGlobalApplication，含 MenuService，无他游戏残留")
PY

STEP "[4/9] 官方 classes.dex → baksmali → 三处补丁 → classes.dex"
unzip -p "$ORIG_APK" classes.dex > "$B/classes_orig.dex"
c1_md5="$(md5f "$B/classes_orig.dex")"
[[ "$c1_md5" == "$ORIG_CLASSES_MD5" ]] || FAIL "抽出的 classes.dex md5 不符（${c1_md5}）"
BAKSMALI d -o "$B/c1smali" "$B/classes_orig.dex"
python3.11 - "$B/c1smali" "$ORIG_CERT_MD5" "$OFFICIAL_CRC_CODE" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1]); md5, crc = sys.argv[2], sys.argv[3]

fc = root / "com/gm/util/FileCheck.smali"
if not fc.exists(): raise SystemExit("FileCheck.smali missing")
ft = fc.read_text(encoding="utf-8")
new_fl = f""".method public static findLibrary(Landroid/content/Context;Ljava/lang/String;)Ljava/lang/String;
    .locals 1

    # starry: freeze official ForceUpdate crcCode
    const-string v0, "{crc}"

    return-object v0
.end method"""
ft2, n = re.subn(r"\.method public static findLibrary\(Landroid/content/Context;Ljava/lang/String;\)Ljava/lang/String;[\s\S]*?\.end method", new_fl, ft, count=1)
if n != 1: raise SystemExit("FileCheck.findLibrary patch failed")
fc.write_text(ft2, encoding="utf-8")
print(f"  PASS  FileCheck.findLibrary → {crc} (count=1)")

tap = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
if not tap.exists(): raise SystemExit("TapSignKit.smali missing")
ts = tap.read_text(encoding="utf-8")
TAP_GETSIGN = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "{md5}"

    return-object v0
.end method"""
ts2, n = re.subn(r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method", TAP_GETSIGN, ts, count=1)
if n != 1: raise SystemExit("TapSignKit.getSign patch failed")
tap.write_text(ts2, encoding="utf-8")
print(f"  PASS  TapSignKit.getSign → {md5} (count=1)")

clu = root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
if not clu.exists(): raise SystemExit("ClientLoginUtils.smali missing")
cs = clu.read_text(encoding="utf-8")
new_icl = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
cs2, n = re.subn(r"\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z[\s\S]*?\.end method", new_icl, cs, count=1)
if n != 1: raise SystemExit("ClientLoginUtils.isClientLogin patch failed")
clu.write_text(cs2, encoding="utf-8")
print("  PASS  ClientLoginUtils.isClientLogin → false (count=1)")
PY
SMALI assemble -o "$B/classes.dex" "$B/c1smali"
python3.11 - "$B/classes.dex" <<'PY'
import sys
b = open(sys.argv[1], "rb").read()
assert b[:4] == b"dex\n", "不是 dex"
for n in (b"34259c1947fde1c2e7fe5f3373a1054d", b"6870e083bf4136acf4d0d4b132d5dccd"):
    assert n in b, f"classes.dex 缺补丁常量 {n!r}"
print(f"  PASS  classes.dex {len(b)} B：crcCode + TapSign 常量已在，WebLogin 补丁已落")
PY

STEP "[5/9] 二进制 AndroidManifest（三级策略：env → 缓存 → 迷你 zip 生成）"
if [[ -n "${DZQS_BINARY_MANIFEST:-}" && -f "${DZQS_BINARY_MANIFEST:-}" ]]; then
  cp "$DZQS_BINARY_MANIFEST" "$B/AndroidManifest.xml"
  echo "  from DZQS_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$B/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once via mini zip（不对 578MB 整包 apktool b）"
  python3.11 - "$ORIG_APK" "$B/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
z = zipfile.ZipFile(src)
out = zipfile.ZipFile(dst, "w")
n = 0
for i in z.infolist():
    name = i.filename
    if name not in keep and not name.startswith("res/"):
        continue
    zi = zipfile.ZipInfo(name, i.date_time)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    out.writestr(zi, z.read(i))
    n += 1
z.close(); out.close()
print(f"  mini apk {dst.stat().st_size} B, {n} entries")
PY
  APKTOOL d -f -s -o "$B/mini_dec" "$B/mini.apk" >/dev/null
  python3.11 - "$B/mini_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); t = p.read_text(encoding="utf-8")
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
t = t.replace('android:name="com.sgsdk.core.SGGlobalApplication"',
              'android:name="com.starry.magic.StarryApp"')
if 'android:name="com.starry.magic.StarryApp"' not in t:
    t = t.replace("<application ", '<application android:name="com.starry.magic.StarryApp" ', 1)
assert 'android:name="com.starry.magic.StarryApp"' in t, "StarryApp 未写入 Manifest"
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
assert "com.starry.magic.menu.MenuService" in t, "MenuService 未写入 Manifest"
p.write_text(t, encoding="utf-8")
print("  text Manifest patched（perms + StarryApp + MenuService）")
PY
  if ! APKTOOL b -f "$B/mini_dec" -o "$B/mf_only.apk" >"$B/apktool_b.log" 2>&1; then
    echo "  WARN  apktool b 首次失败，修第三方资源占位项后重试（$B/apktool_b.log）"
    python3.11 - "$B/mini_dec" <<'PY'
import re, sys
from pathlib import Path
root = Path(sys.argv[1]); res = root / "res"
files = {p.name.rsplit(".", 1)[0] for p in res.glob("*/*") if p.is_file()}
FALLBACK = {
    "layout": '<item type="layout" name="{n}">@layout/abc_action_bar_title_item</item>',
    "anim": '<item type="anim" name="{n}">@anim/abc_fade_in</item>',
    "mipmap": '<item type="mipmap" name="{n}">@mipmap/app_icon</item>',
    "drawable": '<drawable name="{n}">#00000000</drawable>',
    "color": '<color name="{n}">#00000000</color>',
}
pat = re.compile(r'<item type="(layout|anim|mipmap|drawable|color)" name="([^"]+)">false</item>')
n = 0
for p in sorted(res.glob("values*/*.xml")):
    if p.name == "public.xml": continue
    t = p.read_text(encoding="utf-8")
    if ">false</item>" not in t: continue
    def sub(m):
        global n
        typ, name = m.group(1), m.group(2)
        n += 1
        if typ != "color" and name in files: return ""
        return FALLBACK[typ].format(n=name)
    p.write_text(pat.sub(sub, t), encoding="utf-8")
print(f"  placeholder res entries fixed: {n}")
PY
    APKTOOL b -f "$B/mini_dec" -o "$B/mf_only.apk" >"$B/apktool_b.log" 2>&1 \
      || { tail -30 "$B/apktool_b.log"; FAIL "apktool b 生成二进制 Manifest 失败"; }
  fi
  unzip -p "$B/mf_only.apk" AndroidManifest.xml > "$B/AndroidManifest.xml"
  mkdir -p "$(dirname "$MF_CACHE")"
  cp "$B/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
[[ -s "$B/AndroidManifest.xml" ]] || FAIL "AndroidManifest.xml 为空"
python3.11 - "$B/AndroidManifest.xml" <<'PY'
import sys
b = open(sys.argv[1], "rb").read()
assert b[:2] == b"\x03\x00", "不是二进制 AXML"
assert "com.starry.magic.StarryApp".encode("utf-16-le") in b, "AXML 缺 StarryApp"
assert "com.starry.magic.menu.MenuService".encode("utf-16-le") in b, "AXML 缺 MenuService"
print(f"  PASS  二进制 Manifest {len(b)} B：StarryApp + MenuService 已核（UTF-16LE 字节断言）")
PY

STEP "[6/9] zip 外科组装（1907 条目逐条审计；resources.arsc 原样 STORED）"
mkdir -p "$B/lib/arm64-v8a"
cp -f "$B/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$B/lib/arm64-v8a/"
if [[ -f "$NDK_CXX" ]]; then
  cp "$NDK_CXX" "$B/lib/arm64-v8a/libc++_shared.so"
else
  echo "  WARN  NDK libc++_shared.so 缺失（${NDK_CXX}），跳过"
fi
if [[ -d "$B/starry_dec/assets/certs" ]]; then
  mkdir -p "$B/assets/certs"; cp -R "$B/starry_dec/assets/certs/." "$B/assets/certs/"
  echo "  assets/certs 一并打入"
else
  echo "  WARN  菜单 APK 无 assets/certs（框架未随包分发证书），跳过"
fi
python3.11 - "$ORIG_APK" "$B" "$B/unsigned.apk" "$WORK/zip_audit.txt" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst, report = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), Path(sys.argv[4])
mf = (work / "AndroidManifest.xml").read_bytes()
c1 = (work / "classes.dex").read_bytes()
c2 = (work / "classes2.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name in ("classes.dex", "classes2.dex", "AndroidManifest.xml"):
        continue
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, zin.read(item))
    n += 1
for name, data in (("AndroidManifest.xml", mf), ("classes.dex", c1), ("classes2.dex", c2)):
    info = zipfile.ZipInfo(name, (2026, 1, 1, 0, 0, 0))
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name, (2026, 1, 1, 0, 0, 0))
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

za = zipfile.ZipFile(src, "r"); zb = zipfile.ZipFile(dst, "r")
a = {i.filename: i for i in za.infolist()}
b = {i.filename: i for i in zb.infolist()}
touched = set(extras) | {"AndroidManifest.xml", "classes.dex", "classes2.dex"}
removed = sorted(k for k in a if k not in b)
added = sorted(k for k in b if k not in a)
changed = sorted(k for k in a if k in b and a[k].CRC != b[k].CRC)
meta_removed = {k for k in a if k.startswith("META-INF/")}
assert set(removed) <= meta_removed, f"误删非 META-INF: {sorted(set(removed) - meta_removed)}"
assert set(changed) <= {"AndroidManifest.xml", "classes.dex"}, f"越界改动: {sorted(set(changed) - {'AndroidManifest.xml','classes.dex'})}"
assert set(changed) == {"AndroidManifest.xml", "classes.dex"}, f"预期改动未生效: {sorted({'AndroidManifest.xml','classes.dex'} - set(changed))}"
expect_added = {"classes2.dex"} | set(extras)
assert set(added) == expect_added, f"新增条目不符: 多{sorted(set(added)-expect_added)} 缺{sorted(expect_added-set(added))}"
bad = [k for k in b if k in a and k not in touched and a[k].CRC != b[k].CRC]
assert not bad, f"未触碰条目 CRC 变化: {bad[:5]}"
assert b["resources.arsc"].compress_type == 0, "resources.arsc 不是 STORED"
assert a["resources.arsc"].CRC == b["resources.arsc"].CRC
arsc_sha = hashlib.sha256(zb.read("resources.arsc")).hexdigest()
assert hashlib.sha256(za.read("resources.arsc")).hexdigest() == arsc_sha
lines = [
    f"官方条目 {len(a)} / 产物条目 {len(b)} / 原样保留 {len(set(b) & set(a)) - len(changed)}",
    f"removed: {len(removed)} 个 META-INF/*（签名前清除）",
    f"changed(CRC): {', '.join(changed)}",
    f"added: {', '.join(added)}",
    f"resources.arsc STORED=1 CRC={a['resources.arsc'].CRC:#010x} sha256={arsc_sha}",
    "未触碰条目 CRC 全等：通过",
]
za.close(); zb.close()
report.write_text("\n".join(lines) + "\n", encoding="utf-8")
print("  PASS  " + lines[0])
print("  PASS  " + lines[2])
print("  PASS  " + lines[3])
print("  PASS  resources.arsc STORED 且 sha256 与官方逐字节一致")
PY

STEP "[7/9] zipalign -p 4 + apksigner 签名（ASCII 路径，避免中文路径进 .bat）"
# WIN: .bat/.exe 一律接 Windows 形态路径，避免 MSYS 路径转换猜测
"$BT/zipalign.exe" -f -p 4 "$(cygpath -w "$B/unsigned.apk")" "$(cygpath -w "$B/aligned.apk")"
"$BT/zipalign.exe" -c -p 4 "$(cygpath -w "$B/aligned.apk")" || FAIL "zipalign 校验失败"
PASS "zipalign -p 4 校验通过"
apksigner_bat sign --ks "$(cygpath -w "$KS")" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$(cygpath -w "$B/signed.apk")" "$(cygpath -w "$B/aligned.apk")" 2>"$B/apksigner.log" \
  || { tail -20 "$B/apksigner.log"; FAIL "apksigner sign 失败"; }
apksigner_bat verify --print-certs "$(cygpath -w "$B/signed.apk")" >"$B/signer.txt" 2>&1 || FAIL "apksigner verify 失败"
head -4 "$B/signer.txt"
cert="$(grep -o 'certificate SHA-256 digest: [0-9a-f]*' "$B/signer.txt" | head -1 | awk '{print $NF}')"
[[ "$cert" == "$EXPECTED_CERT_SHA256" ]] || FAIL "证书 SHA-256 $cert != $EXPECTED_CERT_SHA256"
PASS "签名证书 SHA-256 = $EXPECTED_CERT_SHA256"
mkdir -p "$OUT"
cp -f "$B/signed.apk" "$SIGNED"
md_out="$(md5f "$SIGNED")"; md_src="$(md5f "$B/signed.apk")"
[[ "$md_out" == "$md_src" ]] || FAIL "产物拷贝后 md5 不一致"

STEP "[8/9] 产物审计（条目表 + 哈希表）"
python3.11 - "$SIGNED" "$ORIG_APK" "$WORK/report.txt" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
apk, orig, report = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
z = zipfile.ZipFile(apk); zo = zipfile.ZipFile(orig)
rows = []
for name in ("classes.dex", "classes2.dex", "lib/arm64-v8a/libstarry_magic.so",
             "lib/arm64-v8a/libc++_shared.so", "AndroidManifest.xml"):
    if name not in z.namelist():
        continue
    data = z.read(name)
    rows.append(f"{name:40s} {len(data):>10d}  md5={hashlib.md5(data).hexdigest()}  sha256={hashlib.sha256(data).hexdigest()}")
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Dzqs]" in so and b"[Tlgd]" not in so and b"[Dwct]" not in so
assert z.getinfo("resources.arsc").compress_type == 0
assert hashlib.sha256(zo.read("resources.arsc")).hexdigest() == hashlib.sha256(z.read("resources.arsc")).hexdigest()
names = z.namelist()
assert "classes2.dex" in names and "lib/arm64-v8a/libstarry_magic.so" in names
out = [f"APK    {apk}", f"size   {apk.stat().st_size} B",
       f"md5    {hashlib.md5(apk.read_bytes()).hexdigest()}",
       f"sha256 {hashlib.sha256(apk.read_bytes()).hexdigest()}",
       f"entries {len(names)}（官方 {len(zo.namelist())}，META-INF 清除 {len([n for n in zo.namelist() if n.startswith('META-INF/')])}，新增 {len(names) - len([n for n in zo.namelist() if not n.startswith('META-INF/')])}）",
       "", "== 关键条目哈希 =="] + rows
report.write_text("\n".join(out) + "\n", encoding="utf-8")
print("\n".join(out))
PY

STEP "[9/9] 结果"
echo "  产物: $SIGNED"
echo "  审计: $WORK/zip_audit.txt"
echo "  报告: $WORK/report.txt"
printf '  md5    %s\n' "$(md5f "$SIGNED")"
printf '  sha256 %s\n' "$(sha256sum "$SIGNED" | cut -d' ' -f1)"
echo "DONE: $SIGNED"
