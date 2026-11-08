#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 外科 zip 组装（环境候选；不装机、不 adb）
#
# 与 1.0.23 旧脚本（f9e8f36b^:surgical_dwdlm_1.0.23.sh）的差异，全部是本轮用户点名口径：
#   1) 保留官方 Application「dev.mars.secure.ProxyApplication」，不做 DwdlmApplication 替换
#      （APKSecure 壳自持 ProxyApplication；换 Application = 壳链断）。注入只走 ContentProvider。
#   2) 注入 = 「com.starry.magic.StarryInitProvider」，追加在 </application> 之前，
#      android:initOrder="2147483647"（照 失落城堡2 / 境界守望者先例）。
#   3) so 只从本游戏 libs_dwdlm/<profile>/arm64-v8a/libstarry_magic.so 取，明确路径，绝不从
#      共享 jniLibs 或别的游戏 libs_* 拿；组装期只写 /tmp 工作副本，不改共享 jniLibs。
#   4) 无 install / 无 force-stop / 无 adb 调用（本轮严禁装机、启动、卸载、清数据）。
#   5) 菜单 dex 只注入 classes2.dex（官方仅 classes.dex）；成品禁止出现 SignSpoof 字符串。
#   6) DWDLM_SIGSPOOF=1（2026-11-05 第五轮，单变量＝加「运行时签名替换」）：
#      把 assets/dwdlm_signature_killer.dex（build_killer_dex_dwdlm.sh 烘焙的 MT
#      ApkSignatureKillerEx 形态）反编译后**并入同一个注入 dex**（路线甲：不新增 dex 编号、
#      不动 classes2 断言）。依据：纯重签零注入同样崩 ⇒ 根因是壳拿不到官方签名
#      （libsecret: key-unwrap / GET_SIGNATURES(fallback) / cert cross mismatch）。
#
# 用法：
#   bash 04_scripts/动物大联盟/surgical_dwdlm_1.0.25.sh --dry-run    # 只做静态预检，零写
#   DWDLM_SO_PROFILE=env bash 04_scripts/动物大联盟/surgical_dwdlm_1.0.25.sh
#   DWDLM_MENU_DEX=/path/classes2.dex ...                            # 复用已编好的菜单 dex（跳过 gradle）
set -euo pipefail

MODE="build"
for a in "$@"; do
  case "$a" in
    --dry-run) MODE="dry" ;;
    -h|--help) sed -n '2,22p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $a" >&2; exit 2 ;;
  esac
done

# ---- 单变量对照开关（2026-11-05 用户授权本轮）--------------------------------
#   factory（默认）= 现状：Manifest 挂 android:appComponentFactory=私有工厂 + StarryInitProvider
#   provider-only  = 只留 Provider 注入，**不挂** appComponentFactory；dex 必须复用同源候选
#                    （DWDLM_DEX_REUSE_APK）保证字节不变，so 不重编、六项不启用。
# 目的：把「工厂类静态初始化 → 进程极早期载入 so / 起 watchdog」从启动链上摘掉，
#       只保留官方 ProxyApplication + Provider，验证首次冷启 NPE 是否随之消失。
FACTORY_MODE="${DWDLM_FACTORY_MODE:-factory}"
case "$FACTORY_MODE" in
  factory|provider-only) ;;
  *) echo "ERROR: DWDLM_FACTORY_MODE 只支持 factory|provider-only（收到 ${FACTORY_MODE}）" >&2; exit 2 ;;
esac
SIGSPOOF="${DWDLM_SIGSPOOF:-0}"
case "$SIGSPOOF" in
  0|1) ;;
  *) echo "ERROR: DWDLM_SIGSPOOF 只支持 0|1（收到 ${SIGSPOOF}）" >&2; exit 2 ;;
esac
# 签名杀手 dex（烘焙产物；smali 源在 assets/dwdlm_killer_smali，dex 不入库由脚本重建）
KILLER_MD5=""
REUSE_DEX_SHA=""
FDIR=""
FACTORY_FROM_MENU="n/a"
DEX_SOURCE=""

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="动物大联盟"
VER="1.0.25"
PKG="com.fightingstars.games.animal"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
SMALI_BAK="$NIXIANG/01_tools/smali/baksmali"
APKTOOL_JAR="$NIXIANG/01_tools/apktool/apktool.jar"
# 签名杀手 dex 路径（DWDLM_SIGSPOOF=1 时并入注入 dex；由 build_killer_dex_dwdlm.sh 生成）
KILLER_DEX="$SCRIPT_DIR/assets/dwdlm_signature_killer.dex"
# 私有工厂（本游戏专属，不进共享菜单产物）：源码 → javac → d8 → baksmali → 并入注入 dex。
# 编译产物只落本游戏私有 build 目录（04_scripts/动物大联盟/build/），
# 绝不写 mod菜单/starry-mod-menu/app/build 之类共享产物。
FACTORY_CLASS="com.starry.magic.dwdlm.DwdlmAppComponentFactory"
FACTORY_SRC="$STARRY_SRC/app/src/main/java/com/starry/magic/dwdlm/DwdlmAppComponentFactory.java"
SIGN_SRC="$(dirname "$FACTORY_SRC")/DwdlmSignBridge.java"
FACTORY_REL="com/starry/magic/dwdlm/DwdlmAppComponentFactory.smali"
DEX_BUILD="$SCRIPT_DIR/build/factory_dex"
KS="$NIXIANG/01_tools/starry_release.jks"
ORIG_APK="$NIXIANG/02_apks/$GAME/${GAME}_${VER}_official.apk"
EXPECT_SHA256="2c9e85451b41b3bcb38148e488e28c85bcfba005db9c37743c926c0363c4f539"
EXPECT_SIZE=81191854
SO_PROFILE="${DWDLM_SO_PROFILE:-env}"
SO_PREBUILT="$STARRY_SRC/libs_dwdlm/$SO_PROFILE/arm64-v8a/libstarry_magic.so"
OUT_DIR="$NIXIANG/08_output/$GAME"
SIGSPOOF_SUFFIX=""
REPORT_SUFFIX=""
if [[ "$SIGSPOOF" == "1" ]]; then SIGSPOOF_SUFFIX="-sigspoof"; REPORT_SUFFIX="_sigspoof"; fi
if [[ "$FACTORY_MODE" == "provider-only" ]]; then
  TAG_NAME="${GAME}_${VER}_starry-$SO_PROFILE-provideronly${SIGSPOOF_SUFFIX}"
  REPORT="$SCRIPT_DIR/assemble_${SO_PROFILE}_provideronly${REPORT_SUFFIX}.json"
else
  TAG_NAME="${GAME}_${VER}_starry-$SO_PROFILE${SIGSPOOF_SUFFIX}"
  REPORT="$SCRIPT_DIR/assemble_${SO_PROFILE}${REPORT_SUFFIX}.json"
fi
UNSIGNED="$OUT_DIR/${TAG_NAME}-unsigned.apk"
SIGNED="$OUT_DIR/${TAG_NAME}-releaseSigned.apk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
NDK_CXX=""
for cand in "$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so" \
            "$NDK/toolchains/llvm/prebuilt/darwin-aarch64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"; do
  if [[ -f "$cand" ]]; then NDK_CXX="$cand"; break; fi
done

say() { printf '%s\n' "$*"; }
die() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }

# ---- 0. 静态预检（dry-run 到此为止，零写）----------------------------------
[[ -f "$ORIG_APK" ]] || die "缺底包 ${ORIG_APK}（本轮禁止重拉，请先确认 02_apks 保全）"
ACT_SIZE="$(stat -f '%z' "$ORIG_APK")"
ACT_SHA="$(shasum -a 256 "$ORIG_APK" | awk '{print $1}')"
[[ "$ACT_SIZE" == "$EXPECT_SIZE" ]] || die "底包 size 与登记值不符（底包被替换？）"
[[ "$ACT_SHA" == "$EXPECT_SHA256" ]] || die "底包 sha256 与登记值不符（底包被替换？）"
say "[check] 底包身份 ok: size=$ACT_SIZE sha256=$(printf '%s' "$ACT_SHA" | cut -c1-16)..."

[[ -x "$SMALI_BIN" ]] || die "缺 smali: $SMALI_BIN"
[[ -x "$SMALI_BAK" ]] || die "缺 baksmali: $SMALI_BAK"
[[ -f "$APKTOOL_JAR" ]] || die "缺 apktool: $APKTOOL_JAR"
[[ -f "$KS" ]] || die "缺签名库: $KS"
[[ -n "$BT" ]] || die "缺 build-tools（zipalign/apksigner/d8）"
[[ -x "$BT/d8" ]] || die "缺 d8: $BT/d8"
[[ -n "$NDK_CXX" ]] || die "缺 NDK libc++_shared.so（底包不含，必须随包注入）"
command -v javac >/dev/null 2>&1 || die "缺 javac（JAVA_HOME=${JAVA_HOME}）"
ANDROID_JAR=""
for api in 36 35 34 33; do
  if [[ -f "$HOME/Library/Android/sdk/platforms/android-$api/android.jar" ]]; then
    ANDROID_JAR="$HOME/Library/Android/sdk/platforms/android-$api/android.jar"; break
  fi
done
[[ -n "$ANDROID_JAR" ]] || die "缺 platforms/android-<api>/android.jar（javac/d8 需要）"
[[ -f "$FACTORY_SRC" ]] || die "缺私有工厂源码: ${FACTORY_SRC}（source 侧未集成完，不能出包）"
APKTOOL="java -jar $APKTOOL_JAR"
say "[check] smali/baksmali/apktool/ks/build-tools/d8/javac/NDK libc++ 均就位"
say "[check] android.jar=$ANDROID_JAR"

[[ -f "$SO_PREBUILT" ]] || die "缺 $SO_PREBUILT —— 先跑 bash 04_scripts/动物大联盟/dwdlm_build_so.sh（profile=${SO_PROFILE}）"
SO_MD5="$(md5 -q "$SO_PREBUILT")"
LC_ALL=C grep -aq "dwdlm" "$SO_PREBUILT" || die "so 无 dwdlm 标签（不是本游戏产物）"
if [[ "$SO_PROFILE" == "env" ]]; then
  if LC_ALL=C grep -aq "starry_dwdlm_v1" "$SO_PREBUILT"; then die "env so 内含玩法 JS 补丁标记（应已编译期剔除）"; fi
  LC_ALL=C grep -aq "DWDLM_ENV_ONLY" "$SO_PREBUILT" || die "env so 缺 DWDLM_ENV_ONLY 横幅（宏没生效？）"
fi
say "[check] so ok: profile=$SO_PROFILE md5=$SO_MD5 size=$(stat -f '%z' "$SO_PREBUILT")"

INJECT_NUM="$(python3 "$NIXIANG/04_scripts/_common/next_free_dex.py" "$ORIG_APK")"
INJECT_DEX="classes${INJECT_NUM}.dex"
say "[check] 官方 dex 最大序号 → 注入 $INJECT_DEX"

MANIFEST_APP="$(python3 - "$ORIG_APK" <<'PY'
import sys, zipfile
data = zipfile.ZipFile(sys.argv[1]).read("AndroidManifest.xml")
text = data.decode("utf-16-le", "ignore")
print("dev.mars.secure.ProxyApplication" if "dev.mars.secure.ProxyApplication" in text else "UNKNOWN")
PY
)"
[[ "$MANIFEST_APP" == "dev.mars.secure.ProxyApplication" ]] || die "官方 Application 不是 ProxyApplication（口径变了，先人工核对）"
say "[check] 官方 Application = ${MANIFEST_APP}（本脚本保留它，不替换）"

if [[ "$MODE" == "dry" ]]; then
  say ""
  say "[dry-run] 无任何写入。若执行将产出："
  say "  unsigned : $UNSIGNED"
  say "  signed   : $SIGNED"
  say "  report   : $REPORT"
  say "  menu dex : ${INJECT_DEX}（gradle assembleRelease → apktool d → baksmali+smali 合并）"
  say "  factory  : ${FACTORY_CLASS}（javac+d8 → $DEX_BUILD → 并入 ${INJECT_DEX}）"
  say "  sigspoof : DWDLM_SIGSPOOF=${SIGSPOOF}（1 ⇒ baksmali $KILLER_DEX 并入 ${INJECT_DEX}；不新增 dex）"
  say "  so       : ${SO_PREBUILT}（md5=${SO_MD5}）→ lib/arm64-v8a/libstarry_magic.so"
  say "  cxx      : $NDK_CXX → lib/arm64-v8a/libc++_shared.so（底包不含）"
  say "  factory  : mode=${FACTORY_MODE}（provider-only ⇒ <application> **不挂** appComponentFactory；需 DWDLM_DEX_REUSE_APK）"
  say "  manifest : application@name 保持 ProxyApplication；appComponentFactory=${FACTORY_CLASS}（仅 factory 模式）"
  say "             追加 StarryInitProvider(authority=${PKG}.starryinit, initOrder=2147483647) + MenuService + 3 权限"
  say "[dry-run] 静态预检全过"
  exit 0
fi

# ---- 1. 共享输入只读性核对 -------------------------------------------------
SHARED_JNILIBS="$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
SHARED_MD5_BEFORE="$(md5 -q "$SHARED_JNILIBS" 2>/dev/null || echo missing)"
say "[1/7] 共享 jniLibs 现状 md5=${SHARED_MD5_BEFORE}（组装全程不写它，结束复核）"

LOCK="/tmp/dwdlm_surgical.lockdir"
mkdir "$LOCK" 2>/dev/null || die "lock held: $LOCK"
echo $$ > "$LOCK/pid"
WORK="$(mktemp -d /tmp/dwdlm_surgical.XXXXXX)"
cleanup() { rm -rf "$WORK"; rm -rf "$LOCK"; }
trap cleanup EXIT
mkdir -p "$WORK"

# ---- 2/3. 注入 dex ---------------------------------------------------------
# provider-only 模式：**必须**复用同源候选包里的注入 dex（不重编、不 baksmali/smali 往返），
# 这样「只 Manifest 不同」才是可证的单变量；工厂类留在 dex 但 Manifest 不再引用 ⇒
# 类不会被初始化 ⇒ 静态块里的 loadLibrary/nativeEnvStart 不会在进程早期执行。
if [[ "$FACTORY_MODE" == "provider-only" ]]; then
  REUSE_APK="${DWDLM_DEX_REUSE_APK:-}"
  [[ -f "$REUSE_APK" ]] || die "provider-only 模式必须显式给 DWDLM_DEX_REUSE_APK=<上一份同源候选 apk>（保证 ${INJECT_DEX} 字节相同）"
  unzip -p "$REUSE_APK" "$INJECT_DEX" > "$WORK/$INJECT_DEX" 2>/dev/null || true
  [[ -s "$WORK/$INJECT_DEX" ]] || die "从 ${REUSE_APK} 取不到 ${INJECT_DEX}"
  REUSE_DEX_SHA="$(shasum -a 256 "$WORK/$INJECT_DEX" | awk '{print $1}')"
  DEX_SOURCE="复用 ${REUSE_APK} 的 ${INJECT_DEX}（未重编）"
  say "[2/7] provider-only：复用 ${REUSE_APK} 内的 ${INJECT_DEX}，sha256=$(printf '%s' "$REUSE_DEX_SHA" | cut -c1-16)…"
  say "[3/7] 跳过 gradle/javac/d8（DEX 字节不变；工厂类留包未被引用）"
  LC_ALL=C grep -aq "starry" "$WORK/$INJECT_DEX" || die "复用 dex 无 starry 标记"
  LC_ALL=C grep -aq "Lcom/starry/magic/dwdlm/DwdlmAppComponentFactory;" "$WORK/$INJECT_DEX" \
    || die "复用 dex 缺 ${FACTORY_CLASS}（不是同一份同源候选？）"
  LC_ALL=C grep -aq "nativeEnvStart" "$WORK/$INJECT_DEX" || die "复用 dex 缺 nativeEnvStart 声明"
  if LC_ALL=C grep -aq "SignSpoof" "$WORK/$INJECT_DEX"; then die "复用 dex 含 SignSpoof（禁止复活）"; fi
  say "  ${INJECT_DEX} size=$(stat -f "%z" "$WORK/$INJECT_DEX")B sha256=$REUSE_DEX_SHA"
else

# ---- 2. 菜单 dex 基底（gradle /tmp 隔离副本，或复用 DWDLM_MENU_DEX）--------
BASE_MENU="$WORK/base_menu.dex"
if [[ -n "${DWDLM_MENU_DEX:-}" ]]; then
  [[ -f "$DWDLM_MENU_DEX" ]] || die "DWDLM_MENU_DEX 不存在: $DWDLM_MENU_DEX"
  cp -f "$DWDLM_MENU_DEX" "$BASE_MENU"
  say "[2/7] 复用 DWDLM_MENU_DEX 作基底 dex（跳过 gradle）"
else
  say "[2/7] gradle assembleRelease（/tmp 隔离副本；共享 jniLibs / 共享 build 产物不动）"
  rsync -a --delete \
    --exclude ".gradle/" --exclude "app/build/" --exclude "build/" \
    --exclude "auth-server/.venv/" --exclude "auth-server/data/" --exclude ".idea/" \
    --exclude "obj_*/" --exclude "libs_*/" \
    "$STARRY_SRC/" "$WORK/starry/"
  mkdir -p "$WORK/starry/app/src/main/jniLibs/arm64-v8a"
  cp -f "$SO_PREBUILT" "$WORK/starry/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
  cp -f "$NDK_CXX" "$WORK/starry/app/src/main/jniLibs/arm64-v8a/libc++_shared.so"
  ( cd "$WORK/starry" && ./gradlew assembleRelease --console=plain -q )
  STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
  [[ -f "$STARRY_APK" ]] || die "gradle 未产出 $STARRY_APK"
  unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
  [[ "$(md5 -q "$WORK/check.so")" == "$SO_MD5" ]] || die "gradle APK 内 so 与 libs_dwdlm 产物 md5 不一致（旧 so 混包）"
  say "  gradle APK so 校验 ok"
  $APKTOOL d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
  SMALI_BASE="$WORK/smali_base"; mkdir -p "$SMALI_BASE"
  shopt -s nullglob
  for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
    if [[ -d "$d/com/starry" ]]; then rsync -a "$d/com/starry" "$SMALI_BASE/com/"; fi
  done
  shopt -u nullglob
  [[ -f "$SMALI_BASE/com/starry/magic/StarryInitProvider.smali" ]] || die "smali 缺 StarryInitProvider（注入点不存在）"
  [[ -f "$SMALI_BASE/com/starry/magic/cocos/DwdlmHooks.smali" ]] || die "smali 缺 DwdlmHooks（本游戏 Java 桥没进包）"
  if [[ -e "$SMALI_BASE/com/starry/magic/sign/SignSpoof.smali" ]]; then die "smali 竟含 SignSpoof（2026-10-28 已整体删除，禁止复活）"; fi
  "$SMALI_BIN" assemble -o "$BASE_MENU" "$SMALI_BASE"
fi
[[ -f "$BASE_MENU" ]] || die "基底菜单 dex 未产出"
LC_ALL=C grep -aq "starry" "$BASE_MENU" || die "基底菜单 dex 无 starry 标记"
say "  基底 dex size=$(stat -f "%z" "$BASE_MENU")B"

# ---- 3. 私有工厂 dex（本游戏隔离 build）+ 合并 → 注入 dex -------------------
# 为什么不用 gradle 的现成产物：菜单工程是共享的，其 build/ 目录里的 class 可能停在旧日期
# （模板过期事故根因）。私有工厂必须由**当前源码**现编，产物落本游戏私有目录，
# 三类输入进同一份 dex：菜单 smali（基底） + 本次 javac/d8 的工厂 smali。
FACTORY_SHA8="$(shasum -a 256 "$FACTORY_SRC" | cut -c1-8)"
FDIR="$DEX_BUILD/$FACTORY_SHA8"
mkdir -p "$FDIR/classes" "$FDIR/dex" "$FDIR/smali"
say "[3/7] javac 私有工厂 → d8 → baksmali（隔离目录 ${FDIR}）"
javac -source 8 -target 8 -bootclasspath "$ANDROID_JAR" -encoding UTF-8 -nowarn \
  -d "$FDIR/classes" "$FACTORY_SRC" "$SIGN_SRC"
[[ -f "$FDIR/classes/com/starry/magic/dwdlm/DwdlmAppComponentFactory.class" ]] \
  || die "javac 未产出 DwdlmAppComponentFactory.class"
# d8 只吃 .class 文件（传目录会报 Unsupported source file type，2026-11-05 实测）；
# 输出目录必须先存在。用 find -print0 | xargs -0 传参，路径含空格也不会被词分割撕开。
CLASS0="$FDIR/classes0.txt"
find "$FDIR/classes" -name "*.class" -type f -print0 > "$CLASS0"
[[ -s "$CLASS0" ]] || die "javac 未产出任何 .class"
xargs -0 "$BT/d8" --min-api 21 --lib "$ANDROID_JAR" --output "$FDIR/dex" < "$CLASS0"
[[ -f "$FDIR/dex/classes.dex" ]] || die "d8 未产出工厂 dex"
[[ -f "$FDIR/dex/classes.dex" ]] || die "d8 未产出工厂 dex"
"$SMALI_BAK" d "$FDIR/dex/classes.dex" -o "$FDIR/smali"
[[ -f "$FDIR/smali/$FACTORY_REL" ]] || die "baksmali 未产出 $FACTORY_REL"

SMALI_ROOT="$WORK/smali_merge"
"$SMALI_BAK" d "$BASE_MENU" -o "$SMALI_ROOT"
FACTORY_FROM_MENU=no
if [[ -f "$SMALI_ROOT/$FACTORY_REL" ]]; then FACTORY_FROM_MENU=yes; fi
rsync -a "$FDIR/smali/" "$SMALI_ROOT/"
[[ -f "$SMALI_ROOT/$FACTORY_REL" ]] || die "合并树里没有工厂 smali"
# ---- 3b. 签名杀手并入同一注入 dex（路线甲：不新增 dex 编号，classes2.dex 断言不动）----
if [[ "$SIGSPOOF" == "1" ]]; then
  [[ -f "$KILLER_DEX" ]] || die "缺签名杀手 dex: ${KILLER_DEX}（先跑 bash 04_scripts/动物大联盟/build_killer_dex_dwdlm.sh）"
  KILLER_MD5="$(md5 -q "$KILLER_DEX")"
  say "[3b/7] 签名杀手：baksmali ${KILLER_DEX}（md5=${KILLER_MD5}, $(stat -f '%z' "$KILLER_DEX") B）→ 并入 ${INJECT_DEX}"
  "$SMALI_BAK" d "$KILLER_DEX" -o "$WORK/killer_smali"
  [[ -f "$WORK/killer_smali/bin/mt/signature/KillerApplication193.smali" ]] \
    || die "killer dex 里没有 bin/mt/signature/KillerApplication193"
  if grep -rq '@@' "$WORK/killer_smali/bin/mt/signature/"; then die "killer smali 残留 @@ 占位符（没烘焙）"; fi
  rsync -a "$WORK/killer_smali/bin" "$SMALI_ROOT/"
  [[ -f "$SMALI_ROOT/bin/mt/signature/KillerApplication193.smali" ]] || die "合并树里没有 killer smali"
fi
if [[ -e "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" ]]; then die "合并树含 SignSpoof（禁止复活）"; fi
"$SMALI_BIN" assemble -o "$WORK/$INJECT_DEX" "$SMALI_ROOT"
[[ -f "$WORK/$INJECT_DEX" ]] || die "注入 dex 未产出"
LC_ALL=C grep -aq "starry" "$WORK/$INJECT_DEX" || die "注入 dex 无 starry 标记"
LC_ALL=C grep -aq "Lcom/starry/magic/dwdlm/DwdlmAppComponentFactory;" "$WORK/$INJECT_DEX" \
  || die "注入 dex 缺 ${FACTORY_CLASS}（工厂没进 dex）"
LC_ALL=C grep -aq "nativeEnvStart" "$WORK/$INJECT_DEX" || die "注入 dex 缺 nativeEnvStart 声明"
if LC_ALL=C grep -aq "SignSpoof" "$WORK/$INJECT_DEX"; then die "注入 dex 含 SignSpoof（禁止复活）"; fi
if [[ "$SIGSPOOF" == "1" ]]; then
  LC_ALL=C grep -aq "Lbin/mt/signature/KillerApplication193;" "$WORK/$INJECT_DEX" \
    || die "注入 dex 缺 killer 类 bin/mt/signature/KillerApplication193（签名杀手没进包）"
  say "  ${INJECT_DEX} 含 bin/mt/signature/KillerApplication193 ✓（源 dex md5=${KILLER_MD5}）"
fi
say "  $INJECT_DEX size=$(stat -f "%z" "$WORK/$INJECT_DEX")B  factory=${FACTORY_CLASS}（来源：本次 javac；菜单自带同名副本=${FACTORY_FROM_MENU}）"
  DEX_SOURCE="本次 javac/d8 + 菜单 smali 现编"
fi

# ---- 4. Manifest（保留 ProxyApplication + appComponentFactory + Provider）---
say "[4/7] binary Manifest：ProxyApplication 不动 + appComponentFactory=私有工厂 + Provider(initOrder=MAX)"
python3 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(dst, "w")
for name in keep:
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, zin.read(name))
zin.close(); zout.close()
print("  mini apk %d B" % dst.stat().st_size)
PY
$APKTOOL d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" "$FACTORY_CLASS" "$FACTORY_MODE" <<'PY'
import re, sys
from pathlib import Path
p, pkg, factory, fmode = Path(sys.argv[1]), sys.argv[2], sys.argv[3], sys.argv[4]
t = p.read_text(encoding="utf-8")
OFFICIAL_APP = "dev.mars.secure.ProxyApplication"
PROVIDER = "com.starry.magic.StarryInitProvider"
AUTH = pkg + ".starryinit"
assert OFFICIAL_APP in t, "官方 Application 不是 ProxyApplication，口径变了"
assert "com.starry.magic.dwdlm.DwdlmApplication" not in t, "不得替换为 DwdlmApplication"
assert PROVIDER not in t, "Manifest 已含 Provider，重复注入"
assert "SignSpoof" not in t, "Manifest 含 SignSpoof（2026-10-28 已删，禁止复活）"
perms = []
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    if "android.permission." + perm not in t:
        perms.append('    <uses-permission android:name="android.permission.%s"/>' % perm)
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + chr(10).join(perms) + chr(10) + t[i:]
# application@name 保持官方 ProxyApplication。
#   factory 模式       → **新增** appComponentFactory（API 28+ 生效）
#   provider-only 模式 → **不加**该属性（本轮单变量对照：只留 Provider）
if fmode == "provider-only":
    assert "android:appComponentFactory" not in t, "底包已含 appComponentFactory，provider-only 对照不成立"
else:
    if "android:appComponentFactory" not in t:
        t, n = re.subn(r"<application\b", '<application android:appComponentFactory="%s"' % factory, t, count=1)
        assert n == 1, "没找到 <application 起始标签"
prov = chr(10) + '''        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s"
            android:exported="false"
            android:initOrder="2147483647" />
''' % AUTH
svc = '''        <service
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
t = t.replace("</application>", prov + svc + chr(10) + "    </application>", 1)

# ---- 硬断言（不是打印就算过）----
app_m = re.search(r"<application\b[^>]*", t)
assert app_m, "Manifest 无 <application 标签"
app_tag = app_m.group(0)
assert 'android:name="%s"' % OFFICIAL_APP in app_tag, "application@name 不再是官方 ProxyApplication"
if fmode == "provider-only":
    assert "android:appComponentFactory" not in app_tag, "provider-only 但 <application> 仍挂 appComponentFactory（单变量被破坏）"
    assert t.count("android:appComponentFactory") == 0, "provider-only 但 Manifest 仍有 appComponentFactory"
else:
    assert 'android:appComponentFactory="%s"' % factory in app_tag, "application@appComponentFactory 不是私有工厂"
    assert t.count("android:appComponentFactory") == 1, "appComponentFactory 出现多次"
assert t.count(PROVIDER) == 1, "StarryInitProvider 出现次数 != 1"
pm = re.search(r"<provider\b[^>]*android:name=\"%s\"[^>]*" % re.escape(PROVIDER), t)
assert pm, "找不到注入的 provider 标签"
pt = pm.group(0)
assert 'android:authorities="%s"' % AUTH in pt, "provider authority 不是 %s" % AUTH
assert 'android:initOrder="2147483647"' in pt, "provider initOrder 不是 2147483647"
assert 'android:exported="false"' in pt, "provider 未 exported=false"
assert len(re.findall(r'android:authorities="[^"]*"', t)) == len(set(re.findall(r'android:authorities="([^"]*)"', t))), "存在重复 authority"
p.write_text(t, encoding="utf-8")
print("  Manifest ok: mode=%s application@name=ProxyApplication, appComponentFactory=%s," % (
    fmode, factory if fmode == "factory" else "（不挂：provider-only 对照）"))
print("               provider=%s authority=%s initOrder=2147483647 perms=+%d" % (PROVIDER, AUTH, len(perms)))
PY
$APKTOOL b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
[[ -s "$WORK/AndroidManifest.xml" ]] || die "Manifest 重建失败"
# 二进制 AXML 复核（解码 utf-16-le 取字符串池）：三条字符串必须都在，且不得出现 SignSpoof
python3 - "$WORK/AndroidManifest.xml" "$PKG" "$FACTORY_CLASS" "$FACTORY_MODE" <<'PY'
import sys
from pathlib import Path
data = Path(sys.argv[1]).read_bytes(); pkg = sys.argv[2]; factory = sys.argv[3]; fmode = sys.argv[4]
assert data[:4] == b"\x03\x00\x08\x00", "不是真 AXML（头应为 03000800）"
text = data.decode("utf-16-le", "ignore")
need = [("com.starry.magic.StarryInitProvider", "注入 Provider"),
        ("dev.mars.secure.ProxyApplication", "官方 Application"),
        (pkg + ".starryinit", "provider authority")]
if fmode == "provider-only":
    assert factory not in text, "provider-only 但二进制 Manifest 仍含工厂类名（属性没去掉）"
else:
    need = [(factory, "appComponentFactory 类名")] + need
for needle, what in need:
    assert needle in text, "二进制 Manifest 缺 " + what + "（" + needle + "）"
assert "SignSpoof" not in text, "二进制 Manifest 含 SignSpoof"
print("  二进制 Manifest 复核 ok（AXML 头 03000800，mode=%s，关键字符串齐全）" % fmode)
PY

# ---- 5. 外科 zip 合并 ------------------------------------------------------
mkdir -p "$OUT_DIR"
say "[5/7] zip 外科合并（官方条目逐字节透传）"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$INJECT_DEX" "$SO_PREBUILT" "$NDK_CXX" "${DWDLM_SEC_SO:-}" "${DWDLM_LUSTER_SO:-}" <<'PY'
import hashlib, re, sys, zipfile
from pathlib import Path
src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
inject, so_path, cxx_path = sys.argv[4], Path(sys.argv[5]), Path(sys.argv[6])
sec_path = sys.argv[7] if len(sys.argv) > 7 else ""
luster_path = sys.argv[8] if len(sys.argv) > 8 else ""
mf = (work / "AndroidManifest.xml").read_bytes()
extras = {
    "lib/arm64-v8a/libstarry_magic.so": so_path.read_bytes(),
    "lib/arm64-v8a/libc++_shared.so": cxx_path.read_bytes(),
}
if sec_path:
    extras["lib/arm64-v8a/libsecret.so"] = Path(sec_path).read_bytes()
    print("  sec.so 开关补丁版已并入: %s" % sec_path)
if luster_path:
    extras["lib/arm64-v8a/libluster.so"] = Path(luster_path).read_bytes()
    print("  luster.so JNI_OnLoad 去注册版已并入: %s" % luster_path)
SIG_RE = re.compile(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", re.I)
dropped = []
zin = zipfile.ZipFile(src, "r"); zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if SIG_RE.match(name) or name.endswith("/"):
        dropped.append(name); continue
    if name in extras or name == inject:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
info = zipfile.ZipInfo(inject, (2026, 11, 5, 0, 0, 0)); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
zout.writestr(info, (work / inject).read_bytes())
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name, (2026, 11, 5, 0, 0, 0)); info.compress_type = zipfile.ZIP_STORED; info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()
print("  dropped v1 signatures: %s" % dropped)

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
assert hashlib.sha256(zin2.read("resources.arsc")).hexdigest() == hashlib.sha256(zout2.read("resources.arsc")).hexdigest(), "resources.arsc 被改"
assert zout2.getinfo("resources.arsc").compress_type == zipfile.ZIP_STORED, "resources.arsc 未 STORED"
assert hashlib.sha256(zin2.read("classes.dex")).hexdigest() == hashlib.sha256(zout2.read("classes.dex")).hexdigest(), "官方 classes.dex 被改"
for must in ("assets/apksecurefile", "lib/arm64-v8a/libEncryptorP.so"):
    assert hashlib.sha256(zin2.read(must)).hexdigest() == hashlib.sha256(zout2.read(must)).hexdigest(), must + " 被改"
assert inject in zout2.namelist()
inj = set(extras) | {inject, "AndroidManifest.xml"}
n_same = n_checked = 0
for item in zin2.infolist():
    name = item.filename
    if SIG_RE.match(name) or name.endswith("/") or name in inj:
        continue
    n_checked += 1
    if hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest():
        n_same += 1
print("  官方条目透传 %d/%d; inject=%s" % (n_same, n_checked, sorted(inj)))
if n_same != n_checked:
    raise SystemExit("ERROR: 官方条目 hash 不一致（外科透传被破坏）")
print("  so in apk md5 == %s" % hashlib.md5(zout2.read("lib/arm64-v8a/libstarry_magic.so")).hexdigest())
zin2.close(); zout2.close()
PY

# ---- 6. zipalign + 签名 ----------------------------------------------------
say "[6/7] zipalign + apksigner → $SIGNED"
mkdir -p "$OUT_DIR"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "${STARRY_KS_ALIAS:-starry}" \
  --ks-pass "pass:${STARRY_KS_PASS:-starry2026}" --key-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -6

# ---- 7. 交付前自检（三方一致：so .dynsym ↔ dex 类/方法 ↔ 二进制 Manifest）---
say "[7/7] 自检：so / dex / manifest 三方一致"
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
unzip -p "$SIGNED" "$INJECT_DEX" > "$WORK/final_inject.dex"
[[ "$(md5 -q "$WORK/final.so")" == "$SO_MD5" ]] || die "成品包内 so 与构建产物 md5 不一致"
[[ "$(md5 -q "$WORK/final_inject.dex")" == "$(md5 -q "$WORK/$INJECT_DEX")" ]] || die "成品包内注入 dex 与构建产物 md5 不一致"

LLVM_NM=""
for cand in "$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm" \
            "$NDK/toolchains/llvm/prebuilt/darwin-aarch64/bin/llvm-nm"; do
  if [[ -x "$cand" ]]; then LLVM_NM="$cand"; break; fi
done
[[ -n "$LLVM_NM" ]] || die "缺 llvm-nm（无法校验 so 导出符号）"
JNI_SYM="Java_com_starry_magic_dwdlm_DwdlmAppComponentFactory_nativeEnvStart"
"$LLVM_NM" -D --defined-only "$WORK/final.so" | LC_ALL=C grep -q " T ${JNI_SYM}$" \
  || die "成品 so 的 .dynsym 缺 ${JNI_SYM}（native 侧没编入新工厂入口）"
N_JNI="$("$LLVM_NM" -D --defined-only "$WORK/final.so" | LC_ALL=C grep -c "^.* T Java_" || true)"
say "  [1/3] so 导出 ${JNI_SYM} ✓（Java_* 导出共 ${N_JNI} 个）"

python3 - "$SIGNED" "$INJECT_DEX" "$FACTORY_CLASS" "$PKG" "$FACTORY_MODE" "$SIGSPOOF" <<'PYEOF'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
dex, factory, pkg, fmode = sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5]
sigspoof = sys.argv[6]
names = z.namelist()
for must in (dex, "lib/arm64-v8a/libstarry_magic.so", "AndroidManifest.xml", "classes.dex",
             "assets/apksecurefile", "lib/arm64-v8a/libsecret.so"):
    if must not in names: raise SystemExit("ERROR: 成品包缺 " + must)
d = z.read(dex)
desc = "L" + factory.replace(".", "/") + ";"
if desc.encode() not in d: raise SystemExit("ERROR: " + dex + " 缺类型 " + desc)
if b"nativeEnvStart" not in d: raise SystemExit("ERROR: " + dex + " 缺 nativeEnvStart 方法名")
if b"SignSpoof" in d: raise SystemExit("ERROR: " + dex + " 含 SignSpoof")
if sigspoof == "1" and b"Lbin/mt/signature/KillerApplication193;" not in d:
    raise SystemExit("ERROR: " + dex + " 缺签名杀手 bin/mt/signature/KillerApplication193")
mf = z.read("AndroidManifest.xml").decode("utf-16-le", "ignore")
need = ["dev.mars.secure.ProxyApplication",
        "com.starry.magic.StarryInitProvider", pkg + ".starryinit"]
if fmode == "provider-only":
    if factory in mf: raise SystemExit("ERROR: provider-only 但成品 Manifest 仍含 " + factory)
else:
    need = [factory] + need
for needle in need:
    if needle not in mf: raise SystemExit("ERROR: 成品 Manifest 缺 " + needle)
if "SignSpoof" in mf: raise SystemExit("ERROR: 成品 Manifest 含 SignSpoof")
print("  [2/3] dex %s 含 %s + nativeEnvStart ✓" % (dex, desc))
if sigspoof == "1":
    print("  [2b/3] dex %s 含 bin/mt/signature/KillerApplication193（运行时签名替换）✓" % dex)
print("  [3/3] 成品 Manifest: mode=%s ProxyApplication + %s + Provider ✓" % (
    fmode, "appComponentFactory=" + factory if fmode == "factory" else "无 appComponentFactory（provider-only）"))
PYEOF
if LC_ALL=C grep -aq "SignSpoof" "$SIGNED"; then die "成品包含 SignSpoof 字符串（禁止复活）"; fi
CERT_SHA="$("$BT/apksigner" verify --print-certs "$SIGNED" | sed -n "s/^Signer #1 certificate SHA-256 digest: //p" | head -1)"
say "  签名证书 SHA-256=${CERT_SHA:-unknown}"
SHARED_MD5_AFTER="$(md5 -q "$SHARED_JNILIBS" 2>/dev/null || echo missing)"
[[ "$SHARED_MD5_AFTER" == "$SHARED_MD5_BEFORE" ]] || die "共享 jniLibs 被改动（$SHARED_MD5_BEFORE → ${SHARED_MD5_AFTER}）"
say "  共享 jniLibs 未被改动（md5=${SHARED_MD5_AFTER}）"

SIGNED_SHA="$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
SIGNED_SIZE="$(stat -f '%z' "$SIGNED")"
INJECT_DEX_SHA="$(shasum -a 256 "$WORK/$INJECT_DEX" | awk '{print $1}')"
if [[ "$FACTORY_MODE" == "provider-only" ]]; then
  APP_FACTORY_NOTE="（无：provider-only 单变量对照，<application> 不挂 appComponentFactory；工厂类留在 dex 未被引用）"
  FACTORY_SRC_NOTE="未重编（复用同源候选 dex，见 dex_source）"
else
  APP_FACTORY_NOTE="$FACTORY_CLASS"
  FACTORY_SRC_NOTE="本次 javac+d8（非共享菜单 build 产物）；菜单自带同名副本=$FACTORY_FROM_MENU"
fi
cat > "$REPORT" <<JSON
{
  "game": "$GAME",
  "version": "$VER",
  "profile": "$SO_PROFILE",
  "base_apk": "$ORIG_APK",
  "base_sha256": "$EXPECT_SHA256",
  "so_md5": "$SO_MD5",
  "injected_dex": "$INJECT_DEX",
  "application": "dev.mars.secure.ProxyApplication（保留，未替换）",
  "factory_mode": "$FACTORY_MODE",
  "application_factory": "$APP_FACTORY_NOTE",
  "factory_build_dir": "$FDIR",
  "factory_injected_from": "$FACTORY_SRC_NOTE",
  "inject_dex_sha256": "$INJECT_DEX_SHA",
  "dex_source": "$DEX_SOURCE",
  "sigspoof": "$SIGSPOOF",
  "sigspoof_dex": "${KILLER_DEX}",
  "sigspoof_dex_md5": "${KILLER_MD5:-（未启用）}",
  "sigspoof_build": "04_scripts/动物大联盟/build_killer_dex_dwdlm.sh（官方证书 DER 877B / SHA-256 de8a75d3…3e0a）",
  "injection": "com.starry.magic.StarryInitProvider authority=$PKG.starryinit initOrder=2147483647 + MenuService",
  "cert_sha256": "$CERT_SHA",
  "tri_check": "so .dynsym nativeEnvStart / dex 类型+nativeEnvStart / 二进制 Manifest 三方一致",
  "unsigned": "$UNSIGNED",
  "signed": "$SIGNED",
  "signed_sha256": "$SIGNED_SHA",
  "signed_size": $SIGNED_SIZE,
  "installed": false,
  "installable_verified": false,
  "built_at": "$(date '+%Y-%m-%d %H:%M:%S')",
  "note": "factory_mode=${FACTORY_MODE}；provider-only 为单变量对照候选（仅 Manifest 不挂 appComponentFactory，其余条目应逐字节相同）。so 未重编、六项未启用"
}
JSON

say ""
say "[done] $SIGNED"
say "  sha256=$SIGNED_SHA size=$SIGNED_SIZE"
say "  report=$REPORT"
say "  * 未装机：本轮严禁 install/启动/卸载/清数据；装机需用户当轮授权"