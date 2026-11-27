#!/usr/bin/env bash
# ============================================================================
# 生存33天 1.0.24 TapTap — Windows(Git Bash) 首包外科组装
#   移植自 scripts/sc33_first_pkg_modern_20261104.sh（mac 侧脚本保持原样不改）
#   只出包，不装机（不 adb install）
# 产物：08_output/生存33天/生存33天_1.0.24_hotfix1_v214_win-releaseSigned.apk
#
# 与 mac 版的差异（全部是平台适配，流程/补丁内容一致）：
#   - NDK 用 ndk-build.cmd（Windows NDK 无 bash 版 ndk-build），经 cmd.exe //c 调用
#   - 默认 SKIP_BUILD=1：直接复用 app/src/libs_sc33/arm64-v8a/libstarry_magic.so
#     （已验收 sha256，源码零改动），不在 Windows 重编 native
#   - gradle 走 scripts/win_build.cmd（它负责改写/还原 local.properties 的 sdk.dir）
#   - strings -> llvm-nm/grep -a；md5sum；stat -c%s；cp -a 代替 rsync
#   - apktool / smali / baksmali 走 01_tools 内的 fat-jar（PATH 里没有）
#   - 签名只用 build-tools/35.0.0 的 zipalign.exe + apksigner.bat（无 uber-apk-signer）
#   - lib/arm64-v8a/libc++_shared.so 不覆盖底包自带（底包已有；已核对 so 的 20 个
#     C++ 未定义符号全部由底包 libc++_shared.so 覆盖，零漂移优先）
#   - 打进包里的 libstarry_magic.so 取本地已验收副本，保证与本地 sha256 一致
#
# 用法:
#   bash scripts/sc33_first_pkg_win_20261127.sh                  # 复用已编好的 so（默认）
#   SKIP_BUILD=0 bash scripts/sc33_first_pkg_win_20261127.sh     # 真跑 ndk-build.cmd
# ============================================================================
set -euo pipefail
export PYTHONUTF8=1

# ---------------------------------------------------------------- 0) 环境 ---
NDK="${NDK:-/c/Users/Administrator/AppData/Local/Android/Sdk/ndk/27.0.12077973}"
BT="${BT:-/c/Users/Administrator/AppData/Local/Android/Sdk/build-tools/35.0.0}"
export JAVA_HOME="${JAVA_HOME:-/c/Program Files/Microsoft/jdk-17.0.20.101-hotspot}"
# WIN: 环境里可能已是 Windows 形态（C:\...\hotspot\），统一转 MSYS 形态；.bat 用时再 cygpath -w 转回
JAVA_HOME="$(cygpath -u "$JAVA_HOME")"
export JAVA_HOME
export PATH="$JAVA_HOME/bin:/usr/bin:/bin:$PATH"
PY="${PY:-python3.11}"
SKIP_BUILD="${SKIP_BUILD:-1}"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../../.." && pwd)}"

ORIG_APK="$NIXIANG/02_apks/生存33天/生存33天_1.0.24_taptap_official_20261127.apk"
ORIG_MD5="36a792f9afa349dcc9f4d6498349d4cd"
# 官方签名证书 MD5（= 历史脚本的 ORIG_MD5）：TapSignKit.getSign 必须返回它，不能返回 APK 文件 MD5。
# 2026-11-27 实测：写成 APK MD5 会让 TapTap SDK 校验失败并弹「授权失败/当前网络不可用」Toast。
ORIG_SIGN_MD5="8fa00ae1397609e5e09ff34cc43030e3"
ORIG_SIZE=1178230291
OUT="$NIXIANG/08_output/生存33天"
SIGNED="$OUT/生存33天_1.0.24_hotfix1_v214_win-releaseSigned.apk"

APKTOOL_JAR="$NIXIANG/01_tools/apktool/apktool.jar"
SMALI_JAR="$NIXIANG/01_tools/smali/smali-fat.jar"
BAKSMALI_JAR="$NIXIANG/01_tools/smali/baksmali-fat.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PKG="com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive"

SO_LOCAL="$STARRY_SRC/app/src/libs_sc33/arm64-v8a/libstarry_magic.so"
SO_SHA="e09efc85075d3a4456abe01ee8ede300965789a0a6df39c8f2c9916984a876f6"

PATCH_DEX="classes8.dex"
NEW_DEX="classes9.dex"
C7_DEX="classes7.dex"

# Manifest 缓存默认放 WORK（Windows 侧不留中间产物）；需要跨轮复用就 STARRY_MF_CACHE=<path>
MF_CACHE="${STARRY_MF_CACHE:-}"
# smali/apktool 重建 classes8/9/sq_plugin 与旧包内容等价但字节不定（同一输入两次 assemble 的
# sha256 就不同），要只变 classes7 就得以外科基线打包：MERGE_BASE=<已验收 signed apk>
MERGE_BASE="${MERGE_BASE:-}"

JNI_DIR="$STARRY_SRC/app/src/main/jni"
APP_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"

die() { echo "ERROR: $*" >&2; exit 1; }
need_dir() { [[ -d "$1" ]] || die "缺少目录: $1"; }
need_file() { [[ -f "$1" ]] || die "缺少文件: $1"; }
winp() { cygpath -w "$1"; }
md5_of() { md5sum "$1" | cut -d' ' -f1; }
sha_of() { sha256sum "$1" | cut -d' ' -f1; }
# WIN: apksigner 只有 .bat；.bat 里的 java 需要 Windows 形态 JAVA_HOME（subshell 局部覆盖）
apksigner_bat() { local jh; jh="$(cygpath -w "$JAVA_HOME")"; ( JAVA_HOME="$jh"; "$BT/apksigner.bat" "$@" ); }
EXPECTED_CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

echo "[0/7] 环境断言（NDK / build-tools / JDK / python / apktool / keystore / 底包）"
need_dir "$NDK";                              echo "   NDK       = $NDK"
need_file "$NDK/ndk-build.cmd";               echo "   ndk-build = $NDK/ndk-build.cmd (cmd 版)"
need_dir "$BT"
need_file "$BT/zipalign.exe"; need_file "$BT/apksigner.bat"; need_file "$BT/aapt2.exe"
echo "   BT        = $BT"
need_file "$JAVA_HOME/bin/java"; need_file "$JAVA_HOME/bin/keytool"
echo "   JAVA_HOME = $JAVA_HOME"
command -v "$PY" >/dev/null || die "找不到 python: $PY"
echo "   PY        = $(command -v "$PY")"
need_file "$APKTOOL_JAR"; need_file "$SMALI_JAR"; need_file "$BAKSMALI_JAR"
need_file "$KS"
"$JAVA_HOME/bin/keytool" -list -alias "$KS_ALIAS" -keystore "$KS" -storepass "$KS_PASS" >/dev/null 2>&1 \
  || die "keystore 里没有 alias=$KS_ALIAS: $KS"
echo "   KS        = $KS (alias=$KS_ALIAS ok)"

need_file "$ORIG_APK"
[[ "$(stat -c%s "$ORIG_APK")" == "$ORIG_SIZE" ]] \
  || die "底包大小不是 ${ORIG_SIZE}（实际 $(stat -c%s "$ORIG_APK")）: $ORIG_APK"
_om="$(md5_of "$ORIG_APK")"
[[ "$_om" == "$ORIG_MD5" ]] || die "底包 md5 不是 ${ORIG_MD5}（实际 ${_om}）: $ORIG_APK"
echo "   底包      = $(basename "$ORIG_APK")  size=$ORIG_SIZE md5=$_om"

need_file "$SO_LOCAL"
_ss="$(sha_of "$SO_LOCAL")"
[[ "$_ss" == "$SO_SHA" ]] || die "本地 so sha256 不是 ${SO_SHA}（实际 ${_ss}）"
grep -a -q '\[Sc33\]' "$SO_LOCAL" || die "本地 so 不含 [Sc33] 标记"
echo "   so        = $SO_LOCAL  sha256=$_ss ([Sc33] ok)"

LOCK_DIR="$NIXIANG/13_scratch/sc33_win.lockdir"
mkdir -p "$NIXIANG/13_scratch"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  die "锁被占用: ${LOCK_DIR}（如无进程在跑，删掉它）"
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d "$NIXIANG/13_scratch/sc33_win.XXXXXX")"
cleanup() { rm -rf "$WORK"; rm -rf "$LOCK_DIR"; }
trap cleanup EXIT
mkdir -p "$OUT" "$WORK"
[[ -n "$MF_CACHE" ]] || MF_CACHE="$WORK/AndroidManifest.xml"
echo "   WORK      = $WORK"
# 底包条目表只取一次（pipefail 下不能用 unzip|grep -q：grep 提前退出会让 unzip 吃 SIGPIPE）
unzip -Z1 "$ORIG_APK" > "$WORK/base_entries.txt"
_BE="$(wc -l < "$WORK/base_entries.txt")"
_BX="$(grep -cE '^classes[0-9]*\.dex$' "$WORK/base_entries.txt")"
[[ "$_BE" == "24044" ]] || die "底包条目数不是 24044（实际 ${_BE}）"
[[ "$_BX" == "8" ]] || die "底包 dex 数不是 8（实际 ${_BX}）"
echo "   条目      = $_BE  dex=$_BX"

# ------------------------------------------------------- 1) so（ndk-build）---
if [[ "$SKIP_BUILD" == "1" ]]; then
  echo "[1/7] 复用已编好的 so（SKIP_BUILD=1，不重编 native）"
  SO_USE="$SO_LOCAL"
  SO_EXPECT_SHA="$SO_SHA"
else
  echo "[1/7] ndk-build.cmd Android_sc33.mk"
  JOBS="${JOBS:-$(nproc 2>/dev/null || echo 4)}"
  # WIN: 仓库在中文路径下，ndk-build 会写出乱码目录 → jni 树先复制到 ASCII 的 $WORK/project
  export GNUMAKE="${GNUMAKE:-$NDK/prebuilt/windows-x86_64/bin/make.exe}"
  [[ -f "$GNUMAKE" ]] || die "缺少 make.exe: $GNUMAKE"
  rm -rf "$WORK/project"; mkdir -p "$WORK/project"
  cp -a "$JNI_DIR" "$WORK/project/jni"
  [[ -f "$WORK/project/jni/Android_sc33.mk" ]] || die "Android_sc33.mk 缺失"
  ( cd "$WORK/project" && "$NDK/ndk-build.cmd" \
      NDK_PROJECT_PATH=. APP_BUILD_SCRIPT=./jni/Android_sc33.mk NDK_APPLICATION_MK=./jni/Application.mk \
      NDK_OUT=./obj_sc33 NDK_LIBS_OUT=./libs_sc33 -j"$JOBS" SC33_SPLICE_ENABLED=1 ) >"$WORK/ndk.log" 2>&1 \
    || { tail -30 "$WORK/ndk.log"; die "ndk-build.cmd 失败（log: $WORK/ndk.log）"; }
  tail -3 "$WORK/ndk.log"
  SO_USE="$WORK/project/libs_sc33/arm64-v8a/libstarry_magic.so"
  need_file "$SO_USE"
  grep -a -q '\[Sc33\]' "$SO_USE" || die "重编后的 so 不含 [Sc33]"
  SO_EXPECT_SHA="$(sha_of "$SO_USE")"
  echo "   重编 so sha256=$SO_EXPECT_SHA"
fi

NM="$NDK/toolchains/llvm/prebuilt/windows-x86_64/bin/llvm-nm.exe"
if [[ -f "$NM" ]]; then
  "$NM" -D --defined-only "$SO_USE" 2>/dev/null > "$WORK/so_dynsym.txt" || true
  grep -q ' T JNI_OnLoad' "$WORK/so_dynsym.txt" || die "so 未导出 JNI_OnLoad"
  "$NM" -D --undefined-only "$SO_USE" 2>/dev/null > "$WORK/so_undef.txt" || true
  grep -q 'emutls' "$WORK/so_undef.txt" && die "so 含 __emutls_* 未定义符号"
  echo "   dynsym ok: JNI_OnLoad exported, no emutls"
fi

for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_USE" "$dest/libstarry_magic.so"
done
echo "   so -> jniLibs/libs ok"

# ------------------------------------------------------------ 1b) gradle ---
echo "[1b/7] gradle assembleRelease（scripts/win_build.cmd，内部改写并还原 local.properties）"
[[ -f "$SCRIPT_DIR/win_build.cmd" ]] || die "缺少 $SCRIPT_DIR/win_build.cmd"
( cd "$SCRIPT_DIR" && cmd.exe //c "win_build.cmd assembleRelease" ) || die "win_build.cmd/gradle 失败"
need_file "$APP_APK"
echo "   模板 APK  = $APP_APK  size=$(stat -c%s "$APP_APK")"
unzip -p "$APP_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so" \
  || die "模板 APK 里没有 libstarry_magic.so"
grep -a -q '\[Sc33\]' "$WORK/check.so" || die "模板 APK 里的 so 不是 SC33"

# ------------------------------------------------- 2) apktool d 模板 APK ---
echo "[2/7] apktool d starry template（小模板 APK，非游戏包）"
"$JAVA_HOME/bin/java" -jar "$APKTOOL_JAR" d -f -o "$WORK/starry_dec" "$APP_APK" >/dev/null \
  || die "apktool d 模板失败"

# ------------------------------------------------------- 3) 打 starry dex ---
echo "[3/7] $NEW_DEX = com/starry（StarryApp → JMYBTApplication）"
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT/com"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  cp -R "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || die "StarryApp.smali missing"
"$PY" - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/sanqi/jmybt/game37/JMYBTApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/sanqi/jmybt/game37/JMYBTApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->onCreate()V",
    ),
]
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
if "Lcom/sanqi/jmybt/game37/JMYBTApplication;" not in t:
    raise SystemExit("ERROR: StarryApp missing JMYBTApplication super")
if ".method protected attachBaseContext" not in t:
    t = t.rstrip() + """

.method protected attachBaseContext(Landroid/content/Context;)V
    .locals 1
    .param p1, "base"    # Landroid/content/Context;

    invoke-super {p0, p1}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->attachBaseContext(Landroid/content/Context;)V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    return-void
.end method
"""
    print("  StarryApp +attachBaseContext loadLibrary")
p.write_text(t, encoding="utf-8")
print("  StarryApp → JMYBTApplication ok")
PY
"$JAVA_HOME/bin/java" -jar "$SMALI_JAR" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT" \
  || die "smali assemble $NEW_DEX 失败"
# 按名引用的类必须在模板 dex 里真实存在（proguard 缺 -keep 时 R8 会删/改名，这里当场拦）
"$PY" - "$WORK/$NEW_DEX" <<'PY'
import sys
b = open(sys.argv[1], "rb").read()
for s in (b"Lcom/starry/magic/cocos/Sc33Hooks;", b"Lcom/starry/magic/menu/MenuService;",
          b"Lcom/starry/magic/StarryApp;"):
    assert s in b, f"模板 dex 缺 {s!r}: app/proguard-rules.pro + inject-dex-rules.pro 需加 -keep"
print("  classes9 内容 ok: StarryApp / MenuService / Sc33Hooks")
PY

mkdir -p "$WORK/lib/arm64-v8a"
# 用本地已验收的 so（不用 gradle 侧可能被 strip 的副本），保证包内 sha256 == 本地 sha256
cp -f "$SO_USE" "$WORK/lib/arm64-v8a/libstarry_magic.so"
# libc++_shared.so：底包已自带 → 不覆盖（零漂移）。底包若没有才补 NDK 的。
if grep -qx 'lib/arm64-v8a/libc++_shared.so' "$WORK/base_entries.txt"; then
  echo "   libc++_shared.so 底包已有 → 保持原字节不覆盖"
else
  NDK_CXX="$NDK/toolchains/llvm/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
  [[ -f "$NDK_CXX" ]] || die "底包无 libc++_shared.so 且 NDK 副本缺失: $NDK_CXX"
  cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
  echo "   libc++_shared.so 取自 NDK"
fi
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
  echo "   assets/certs 已带出"
fi

# --------------------------------------------- 4) patch 游戏 classesN.dex ---
echo "[4/7] patch ${PATCH_DEX}（JMYBTActivity + SdkManager）"
unzip -p "$ORIG_APK" "$PATCH_DEX" > "$WORK/patch_orig.dex" || die "底包取 $PATCH_DEX 失败"
"$JAVA_HOME/bin/java" -jar "$BAKSMALI_JAR" d -o "$WORK/c10smali" "$WORK/patch_orig.dex" \
  || die "baksmali $PATCH_DEX 失败"
"$PY" - "$WORK/c10smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])

act = root / "com/sanqi/jmybt/game37/JMYBTActivity.smali"
if not act.is_file():
    raise SystemExit("missing JMYBTActivity.smali")
s = act.read_text(encoding="utf-8")

super_on = "    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"
early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"""
if "MenuService;->start" not in s:
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
    print("  JMYBTActivity.onCreate early loadLibrary + MenuService")
else:
    print("  JMYBTActivity.onCreate already patched")

def inject_lifecycle(method: str, leaving: bool):
    global s
    if leaving:
        needle = "MenuService;->notifyHostLeaving"
        call = "    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n"
    else:
        needle = "MenuService;->notifyHostResumed"
        call = "    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n"
    if needle in s:
        return
    pat = rf"(\.method protected {method}\(\)V[\s\S]*?)(    return-void\n\.end method)"
    s2, n = re.subn(pat, r"\1" + call + r"\2", s, count=1)
    if n != 1:
        raise SystemExit(f"JMYBTActivity.{method} patch failed")
    s = s2
    print(f"  JMYBTActivity.{method} → {needle.split(';')[1]}")

inject_lifecycle("onResume", False)
inject_lifecycle("onPause", True)

if "onUserLeaveHint" not in s:
    leave = """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    s = s.replace(".method protected onPause()V", leave + ".method protected onPause()V", 1)
    print("  +onUserLeaveHint")

if "Sc33Hooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/Sc33Hooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/Sc33Hooks;->setRet(IZ)V

    return-void
.end method
"""
    print("  +get_ret/set_ret → Sc33Hooks")
act.write_text(s, encoding="utf-8")

sdk = root / "com/sanqi/jmybt/game37/sdk/SdkManager.smali"
if not sdk.is_file():
    raise SystemExit("missing SdkManager.smali")
ss = sdk.read_text(encoding="utf-8")
if "Sc33Hooks;->shouldSkipAd" not in ss:
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

    invoke-static {}, Lcom/starry/magic/cocos/Sc33Hooks;->shouldSkipAd()Z

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
    const-string v0, "[SdkManager](SdkShowAdReward)\u8c03\u7528\u5c55\u793a\u6fc0\u52b1\u5e7f\u544a"

    invoke-static {v0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->logi(Ljava/lang/String;)V

    invoke-static {p1}, Lcom/alibaba/fastjson/JSONObject;->parseObject(Ljava/lang/String;)Lcom/alibaba/fastjson/JSONObject;

    move-result-object p1

    const-string v0, "content"

    invoke-virtual {p1, v0}, Lcom/alibaba/fastjson/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object p1

    invoke-static {}, Lcom/sqwan/msdk/SQwanCore;->getInstance()Lcom/sqwan/msdk/SQwanCore;

    move-result-object v0

    iget-object v1, p0, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->mContext:Landroid/content/Context;

    new-instance v2, Lcom/sanqi/jmybt/game37/sdk/SdkManager$13;

    invoke-direct {v2, p0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager$13;-><init>(Lcom/sanqi/jmybt/game37/sdk/SdkManager;)V

    invoke-virtual {v0, v1, p1, v2}, Lcom/sqwan/msdk/SQwanCore;->showAdReward(Landroid/content/Context;Ljava/lang/String;Lcom/sqwan/msdk/api/SQResultListener;)V

    return-void
.end method"""
    ss2, n = re.subn(
        r"\.method public SdkShowAdReward\(Ljava/lang/String;\)V[\s\S]*?\.end method",
        ad_new,
        ss,
        count=1,
    )
    if n != 1:
        raise SystemExit("SdkShowAdReward patch failed")
    sdk.write_text(ss2, encoding="utf-8")
    print("  SdkShowAdReward → Sc33Hooks.shouldSkipAd")
else:
    print("  SdkShowAdReward ok")
PY
"$JAVA_HOME/bin/java" -jar "$SMALI_JAR" assemble -o "$WORK/$PATCH_DEX" "$WORK/c10smali" \
  || die "smali assemble $PATCH_DEX 失败"
rm -rf "$WORK/c10smali" "$WORK/patch_orig.dex"
echo "   新 $PATCH_DEX size=$(stat -c%s "$WORK/$PATCH_DEX")  新 $NEW_DEX size=$(stat -c%s "$WORK/$NEW_DEX")"

# ------------------------------------------------ 4b) patch sq_plugin.apk ---
echo "[4b/7] patch sq_plugin（嵌套插件，非游戏 apktool b 整包）"
PLUGIN_NAME="$(unzip -Z1 "$ORIG_APK" 2>/dev/null | grep -E '^assets/sq_plugin_.*[.]apk$' | head -1 || true)"
[[ -n "$PLUGIN_NAME" ]] || die "底包缺 assets/sq_plugin_*.apk"
echo "   plugin = $PLUGIN_NAME"
unzip -p "$ORIG_APK" "$PLUGIN_NAME" > "$WORK/plugin_orig.apk" || die "取 plugin 失败"
"$JAVA_HOME/bin/java" -jar "$APKTOOL_JAR" d -f -o "$WORK/plug_dec" "$WORK/plugin_orig.apk" >/dev/null \
  || die "apktool d plugin 失败"
"$PY" - "$WORK/plug_dec" "$ORIG_SIGN_MD5" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
md5 = sys.argv[2]
taps = list(root.rglob("TapSignKit.smali"))
if not taps:
    raise SystemExit("TapSignKit missing in sq_plugin")
TAP = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "{md5}"

    return-object v0
.end method"""
for tap in taps:
    ts = tap.read_text(encoding="utf-8")
    if md5 in ts and "starry_sign_bypass" in ts:
        print(f"  TapSignKit ok ({tap.relative_to(root)})")
        continue
    ts2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        TAP, ts, count=1,
    )
    if n != 1:
        raise SystemExit(f"TapSignKit patch failed: {tap}")
    tap.write_text(ts2, encoding="utf-8")
    print(f"  TapSignKit.getSign → {md5} ({tap.relative_to(root)})")
PY
"$JAVA_HOME/bin/java" -jar "$APKTOOL_JAR" b -f "$WORK/plug_dec" -o "$WORK/plugin_patched.apk" >/dev/null \
  || die "apktool b plugin 失败"
mkdir -p "$WORK/assets"
cp -f "$WORK/plugin_patched.apk" "$WORK/assets/$(basename "$PLUGIN_NAME")"
rm -rf "$WORK/plug_dec" "$WORK/plugin_orig.apk" "$WORK/plugin_patched.apk"

# --------------------------------- 4c) classes7.dex（保持官方原版）---
# 2026-11-27：此处曾尝试改 BaseWebAuthorizeActivity.showNetworkErrorDialog 来屏蔽「授权失败」
# 提示，实测无效 —— 那条提示是 2 秒 Toast，真实根因是 TapSignKit.getSign 返回了 APK MD5 而不是
# 签名证书 MD5（见 10_docs/生存33天/锚点与验证.md 的「签名校验」一节）。正式包一律用底包原版。
echo "[4c/7] ${C7_DEX} 保持官方原版（不做弹窗屏蔽改动）"
unzip -p "$ORIG_APK" "$C7_DEX" > "$WORK/$C7_DEX" || die "底包取 $C7_DEX 失败"

# ------------------------------------------- 5) binary AndroidManifest.xml ---
echo "[5/7] binary AndroidManifest（StarryApp + MenuService）"
if [[ -f "$MF_CACHE" ]]; then
  cp -f "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached Manifest ($MF_CACHE)"
else
  echo "  building stub apk (manifest+arsc+res only)"
  rm -rf "$WORK/stub_src"; mkdir -p "$WORK/stub_src"
  # WIN: unzip 对 1.17GB 包的 res/* 通配匹配失败（filename not matched）→ 用 python 精确抽取
  "$PY" - "$ORIG_APK" "$WORK/stub_src" <<'PY'
import sys, zipfile
from pathlib import Path
apk, out = sys.argv[1], Path(sys.argv[2])
z = zipfile.ZipFile(apk)
names = [n for n in z.namelist()
         if n in ("AndroidManifest.xml", "resources.arsc") or n.startswith("res/")]
for n in names:
    p = out / n
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_bytes(z.read(n))
print(f"  stub src: {len(names)} entries (manifest+arsc+res/)")
PY
  ( cd "$WORK/stub_src" && zip -q -r -X "$WORK/stub.apk" AndroidManifest.xml resources.arsc res ) \
    || die "打 stub 失败"
  "$JAVA_HOME/bin/java" -jar "$APKTOOL_JAR" d -f -s -o "$WORK/stub_dec" "$WORK/stub.apk" >/dev/null \
    || die "apktool d stub 失败"
  "$PY" - "$WORK/stub_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if 'android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
t = t.replace(
    'android:name="com.sanqi.jmybt.game37.JMYBTApplication"',
    'android:name="com.starry.magic.StarryApp"',
)
if 'android:name="com.starry.magic.StarryApp"' not in t:
    raise SystemExit("ERROR: failed to set StarryApp")
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
  "$JAVA_HOME/bin/java" -jar "$APKTOOL_JAR" b -f "$WORK/stub_dec" -o "$WORK/stub_patched.apk" >/dev/null \
    || die "apktool b stub 失败"
  unzip -p "$WORK/stub_patched.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml" \
    || die "取回 stub manifest 失败"
  # 资源 ID 保持自检：重建前后 manifest 的 @0x… 引用必须完全一致
  if [[ -f "$BT/aapt2.exe" ]]; then
    "$BT/aapt2.exe" dump xmltree --file AndroidManifest.xml "$(winp "$ORIG_APK")" 2>/dev/null \
      | grep -oE "@0x[0-9a-f]+" | sort -u > "$WORK/ref_orig.txt" || true
    "$BT/aapt2.exe" dump xmltree --file AndroidManifest.xml "$(winp "$WORK/stub_patched.apk")" 2>/dev/null \
      | grep -oE "@0x[0-9a-f]+" | sort -u > "$WORK/ref_new.txt" || true
    if [[ -s "$WORK/ref_orig.txt" && -s "$WORK/ref_new.txt" ]]; then
      diff -q "$WORK/ref_orig.txt" "$WORK/ref_new.txt" >/dev/null \
        || { echo "ERROR: 重建 manifest 资源引用漂移"; diff "$WORK/ref_orig.txt" "$WORK/ref_new.txt"; exit 1; }
      echo "  res-id check ok ($(wc -l < "$WORK/ref_orig.txt") refs)"
    else
      echo "  WARN: res-id check skipped"
    fi
  fi
  rm -rf "$WORK/stub_src" "$WORK/stub.apk" "$WORK/stub_dec" "$WORK/stub_patched.apk"
  if [[ "$MF_CACHE" != "$WORK/AndroidManifest.xml" ]]; then
    mkdir -p "$(dirname "$MF_CACHE")"; cp -f "$WORK/AndroidManifest.xml" "$MF_CACHE"
    echo "  cached → $MF_CACHE"
  fi
fi

# ------------------------------------------------------- 6) zip 外科合并 ---
echo "[6/7] zip surgical merge"
"$PY" - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$PLUGIN_NAME" "$PATCH_DEX" "$NEW_DEX" "$C7_DEX" "$MERGE_BASE" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, work, dst, plugin_name, patch_dex, new_dex, c7_dex = (
    Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4], sys.argv[5], sys.argv[6],
    sys.argv[7],
)
merge_base = sys.argv[8].strip() if len(sys.argv) > 8 else ""
if merge_base:
    src = Path(merge_base)
mf = (work / "AndroidManifest.xml").read_bytes()
c_patch = (work / patch_dex).read_bytes()
c_new = (work / new_dex).read_bytes()
c_c7 = (work / c7_dex).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
plug = work / "assets" / Path(plugin_name).name
if plug.is_file():
    extras[plugin_name] = plug.read_bytes()

if merge_base:
    # 基线模式：基线里的 classes8/classes9/Manifest/sq_plugin/so 已验收，本轮只外科替换 classes7.dex
    replace = {c7_dex: c_c7}
    skip_new = set()
else:
    replace = {"AndroidManifest.xml": mf, patch_dex: c_patch, c7_dex: c_c7}
    skip_new = set(extras) | {new_dex}

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
orig_hashes = {}
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in skip_new:
        continue
    if name in replace:
        data = replace[name]
    else:
        data = zin.read(name)
    # 零漂移基线一律取「原包字节」，与被替换项无关
    orig_hashes[name] = hashlib.sha256(zin.read(name)).hexdigest()
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

if not merge_base:
    info = zipfile.ZipInfo(new_dex)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, c_new)
    for name, data in sorted(extras.items()):
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name, h in orig_hashes.items():
    if name in replace:
        assert hashlib.sha256(zout2.read(name)).hexdigest() != h, f"{name} should change"
        continue
    assert hashlib.sha256(zout2.read(name)).hexdigest() == h, f"byte drift: {name}"
assert hashlib.sha256(zout2.read("resources.arsc")).hexdigest() == orig_hashes["resources.arsc"]
assert new_dex in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
if merge_base:
    print(f"  merge ok (baseline={Path(merge_base).name}); only {c7_dex} replaced; arsc unchanged")
else:
    print(f"  merge ok; {c7_dex} + {patch_dex} patched; {new_dex} added; arsc unchanged")
print(f"  entries {len(zin2.namelist())} -> {len(zout2.namelist())}")
print("  jsc baseline assets/assets/main/index.jsc sha256=" + orig_hashes.get("assets/assets/main/index.jsc", "n/a"))
zin2.close(); zout2.close()
PY

# ------------------------------------------------------- 7) align + sign ---
echo "[7/7] zipalign -p 4 + apksigner（签在 ASCII 工作路径，最后拷到中文产物路径）"
ALIGNED="$WORK/aligned.apk"
SIGNED_TMP="$WORK/signed.apk"
"$BT/zipalign.exe" -f -p 4 "$(winp "$WORK/unsigned.apk")" "$(winp "$ALIGNED")" || die "zipalign 失败"
"$BT/zipalign.exe" -c -p 4 "$(winp "$ALIGNED")" || die "zipalign -c 校验失败"
echo "   zipalign -p 4 校验通过"
rm -f "$WORK/unsigned.apk"
apksigner_bat sign --ks "$(winp "$KS")" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v4-signing-enabled false \
  --out "$(winp "$SIGNED_TMP")" "$(winp "$ALIGNED")" 2>"$WORK/apksigner.log" \
  || { tail -20 "$WORK/apksigner.log"; die "apksigner sign 失败"; }
rm -f "$ALIGNED"
apksigner_bat verify --print-certs "$(winp "$SIGNED_TMP")" >"$WORK/verify.txt" 2>&1 || die "apksigner verify 失败"
CERT_SHA="$(grep -o 'certificate SHA-256 digest: [0-9a-f]*' "$WORK/verify.txt" | head -1 | awk '{print $NF}')"
[[ "$CERT_SHA" == "$EXPECTED_CERT_SHA256" ]] || die "证书 SHA-256=$CERT_SHA != $EXPECTED_CERT_SHA256"
echo "   证书 SHA-256 = $CERT_SHA"
mkdir -p "$OUT"; rm -f "$SIGNED" "$SIGNED.idsig"
cp -f "$SIGNED_TMP" "$SIGNED"; rm -f "$SIGNED_TMP"

# --------------------------------------------------------------- 自检 ---
need_file "$SIGNED"
echo "[self-check] 产物指纹"
APK_SIZE="$(stat -c%s "$SIGNED")"
APK_MD5="$(md5_of "$SIGNED")"
APK_SHA="$(sha_of "$SIGNED")"
echo "  path   = $SIGNED"
echo "  size   = $APK_SIZE"
echo "  md5    = $APK_MD5"
echo "  sha256 = $APK_SHA"

echo "  cert   = $CERT_SHA";

unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/inapk.so" || die "包内无 libstarry_magic.so"
INAPK_SHA="$(sha_of "$WORK/inapk.so")"
[[ "$INAPK_SHA" == "$SO_EXPECT_SHA" ]] || die "包内 so sha256=$INAPK_SHA != 本地 $SO_EXPECT_SHA"
echo "  so in apk sha256 = $INAPK_SHA (== 本地 ok)"

DEX_LIST="$(unzip -Z1 "$SIGNED" | grep -E '^classes[0-9]*\.dex$' | sort -V | tr '\n' ' ')"
echo "  dex    = $DEX_LIST"
echo "$DEX_LIST" | grep -q "classes9.dex" || die "包内缺 classes9.dex"
unzip -p "$SIGNED" classes9.dex > "$WORK/c9.dex"
unzip -p "$SIGNED" classes8.dex > "$WORK/c8.dex"
"$PY" - "$WORK/c9.dex" "$WORK/c8.dex" <<'PY'
import sys
c9 = open(sys.argv[1], "rb").read()
c8 = open(sys.argv[2], "rb").read()
n = c9.count(b"Lcom/starry/magic/cocos/Sc33Hooks;")
assert n >= 1, f"classes9.dex 内 Sc33Hooks 命中 {n}（应 >=1）"
assert b"Sc33Hooks" in c9
# dex 里类型描述与方法名是字符串池里的两条独立记录，不能拼成 "Sc33Hooks;->getRet" 找
assert c8.count(b"Lcom/starry/magic/cocos/Sc33Hooks;") >= 1, "classes8.dex 无 Sc33Hooks 类型引用"
for meth in (b"getRet", b"setRet", b"shouldSkipAd"):
    assert meth in c8, f"classes8.dex 缺方法名 {meth!r}"
print(f"  dex 交叉引用 ok: classes9 Sc33Hooks x{n}; classes8 type ref x{c8.count(b'Lcom/starry/magic/cocos/Sc33Hooks;')} + getRet/setRet/shouldSkipAd")
PY

unzip -p "$SIGNED" "$C7_DEX" > "$WORK/c7_inapk.dex" || die "包内无 $C7_DEX"
unzip -p "$ORIG_APK" "$C7_DEX" > "$WORK/c7_orig_ref.dex" || die "底包无 $C7_DEX"
cmp -s "$WORK/c7_inapk.dex" "$WORK/c7_orig_ref.dex" || die "包内 ${C7_DEX} 与底包不一致（正式包必须保持官方原版）"
echo "  包内 ${C7_DEX} == 底包原版"

unzip -p "$SIGNED" AndroidManifest.xml > "$WORK/mf_inapk.xml" || die "包内无 AndroidManifest.xml"
"$PY" - "$WORK/mf_inapk.xml" <<'PY'
import sys
b = open(sys.argv[1], "rb").read()
assert b[:2] == b"\x03\x00", "不是二进制 AXML"
for s in ("com.starry.magic.StarryApp", "com.starry.magic.menu.MenuService"):
    assert s.encode("utf-16-le") in b, f"AXML 缺 {s}"
print(f"  manifest {len(b)} B: StarryApp ok / MenuService ok（UTF-16LE 字节断言）")
PY

BASE_JSC="$(unzip -p "$ORIG_APK" 'assets/assets/main/index.jsc' | sha256sum | cut -d' ' -f1)"
NEW_JSC="$(unzip -p "$SIGNED" 'assets/assets/main/index.jsc' | sha256sum | cut -d' ' -f1)"
[[ "$BASE_JSC" == "$NEW_JSC" ]] || die "assets/assets/main/index.jsc 被改动 ($BASE_JSC -> $NEW_JSC)"
echo "  index.jsc sha256 = $NEW_JSC (== 底包 ok)"

echo "  libc++_shared.so in apk = $(unzip -Z1 "$SIGNED" | grep -cx 'lib/arm64-v8a/libc++_shared.so' || true)"
echo "  entries = $(unzip -Z1 "$SIGNED" | grep -c .)"

echo "DONE: $SIGNED"
