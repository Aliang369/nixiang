#!/usr/bin/env bash
# 明日方舟：终末地 1.7.2（versionCode 71）— 外科组装（禁止 apktool b 整包游戏）
#
# 复刻依据（权威输入，冲突时序号小者优先）：
#   1. 10_docs/终末地/交付_1.7.2_20261126.md            §二 路线（dex 零改动 / classes7 / killer / SPI / 桩识别）
#   2. 10_docs/终末地/交付_1.7.2_20261126.md            §四 出包复现（断言集）/ §八 代码位置与行号
#   3. 10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md
#   4. 底本脚本：mod菜单/starry-mod-menu/scripts/surgical_endfield_1.7.2.sh 同族 1.6.4 版（git 历史）
#
# 1.7.2 与 1.6.4 的差异（逐项已实测，见输入 2）：
#   · dex 布局同构：classes.dex~classes6.dex（classes6 = 720B 占位）→ 空闲下标 7
#   · ACE so 尺寸逐字节同：libtersafe2 5,609,568 / libtprt 1,879,688（引擎未升级）
#   · Application 仍 com.u8.sdk.U8Application（super = com.ace.gshell.AceApplication）
#   · global-metadata.dat 头仍 94 43 72 12（同套混淆，65,247,548 B）；Unity 仍 2021.3.34f5
#   · 官方证书 DER md5 1cf808541a17bf218d3dd5a30787e485（与 1.6.4 同值）
#   · 官方包内**没有** libc++_shared.so ⇒ 我方 so 的 DT_NEEDED 必须随包注入
#
# 1.7.2 相对 1.6.4 的两处**实质**变化（不是改名）：
#   ① 签名/登录从「运行时签名伪装」改为 **A 轨静态改 dex**（验签路线 §4.1）：
#      classes2 TapSignKit.getSign 回灌官方证书 md5、classes5 门闩 + 掐弹窗、
#      classes3 登录 action 等长改写。详见 endfield_tap_dexpatch_1.7.2.py（含逐条断言）。
#      旧运行时伪装类已整体删除，**禁止复活**（AGENTS.md §7 + skill/starry-mod）。
#   ② 构建戳随 OTA 下线退役（2026-11-21）⇒ 不再 touch Includes/Utils.cpp，也不断言 STARRYSTAMP1。
#
# 非 1.6.4 的两处现代化（2026-10-15 m00007 铁律，不碰共享文件）：
#   · 构建走私有 Android_mrfzzmd.mk（modern 注册表），不再切公共 Android.mk
#   · StarryInitProvider 插到**首个 provider 之前**（skill/starry-mod 入口铁律：
#     Provider 顺序＝平台实例化顺序，须先回灌官方证书再让 TapSDK 读）
#
# - 保留官方 classes.dex / classes4.dex / classes6.dex / resources.arsc / ACE so 字节
# - A 轨改的是 classes2.dex / classes3.dex / classes5.dex（assemble 重组，头部校验自洽）
# - 新增 classes{N}.dex = com/starry（框架 dex）；新增 libstarry_magic.so + libc++_shared.so
# - StarryApp .super → U8Application；【不替换 Application】（ACE 校验类名，换即毒丸黑屏）
# - 产物：08_output/终末地/终末地_1.7.2_surgical-releaseSigned.apk
#
# 用法:
#   bash mod菜单/starry-mod-menu/scripts/surgical_endfield_1.7.2.sh
#   ENDFIELD_BASE_APK=/tmp/x/终末地_1.7.2_taptap_official.apk bash ...   # 显式指定底包
#   STARRY_TRACE=1 bash ...                                            # 带 StarryTrace
#   STARRY_INSTALL=1 STARRY_CLEAN_INSTALL=1 SERIAL=f9a116a2 bash ...    # 异签名：卸载后装
  ENDFIELD_SKIP_DEXPATCH=1 ENDFIELD_SIG_KILLER=1 bash ...            # C 轨：官方 dex 零改动 + 运行时证书回灌
#   STARRY_INSTALL_ONLY=1 bash ...                                     # 只验证+装机，不重建
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

VERSION="1.7.2"
VC="71"
# 底包身份（10_docs/终末地/交付_1.7.2_20261126.md §一 实测），不符即停，避免打错版本
BASELINE_MD5="8abd4b431401a984f34006e858bcae38"
TAG_MK="Android_mrfzzmd.mk"
TAG_MK_TAG="mrfzzmd"
SO_LABEL="[Endfield]"
AUTH_ID="mrfzzmd"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
DEXPATCH_PY="$SCRIPT_DIR/endfield_tap_dexpatch_1.7.2.py"
ORIG_APK="${ENDFIELD_BASE_APK:-}"
# 底包默认路径：1.7.2 现存货名是 …_taptap_official.apk（不是 …_base.apk），按实际留存位回退
if [ -z "$ORIG_APK" ]; then
  for _cand in "$NIXIANG/02_apks/终末地/终末地_${VERSION}_taptap_official.apk" \
               "$NIXIANG/02_apks/终末地/终末地_${VERSION}_base.apk" \
               "$NIXIANG/09_test/终末地/base/终末地_${VERSION}_base.apk"; do
    if [ -f "$_cand" ]; then ORIG_APK="$_cand"; break; fi
  done
fi
if [ -z "$ORIG_APK" ]; then
  echo "ERROR: 找不到官方底包。请用 ENDFIELD_BASE_APK=<绝对路径> 指定；已尝试：" >&2
  echo "       $NIXIANG/02_apks/终末地/终末地_${VERSION}_taptap_official.apk" >&2
  echo "       $NIXIANG/02_apks/终末地/终末地_${VERSION}_base.apk" >&2
  echo "       $NIXIANG/09_test/终末地/base/终末地_${VERSION}_base.apk" >&2
  exit 1
fi

md5_of() { if command -v md5 >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | awk '{print $1}'; fi; }

BASE_MD5="$(md5_of "$ORIG_APK")"
echo "[0/7] 底包：$ORIG_APK"
echo "      size：$(stat -f %z "$ORIG_APK" 2>/dev/null || stat -c %s "$ORIG_APK")  md5：$BASE_MD5"
if [ "$BASE_MD5" != "$BASELINE_MD5" ]; then
  if [ "${ENDFIELD_ALLOW_ANY_BASE:-0}" = "1" ]; then
    echo "      [WARN] 底包 md5 与 1.7.2 基线不符（ENDFIELD_ALLOW_ANY_BASE=1，按用户显式授权继续）"
  else
    echo "ERROR: 底包 md5=$BASE_MD5 ≠ 基线 ${BASELINE_MD5}（版本/渠道不对？真要继续置 ENDFIELD_ALLOW_ANY_BASE=1）" >&2
    exit 1
  fi
fi

# ENDFIELD_OUT 可把产物指到临时区做 A/B（默认 08_output/{游戏}，避免验证性构建误覆盖正式包）
# ENDFIELD_SKIP_DEXPATCH=1：「官方 dex 一律不改」单变量（A/B 二分 V1）
#   置 1 ⇒ 跳过 [3b/7] 的 patch 与收尾的 verify，组装时 classes2/3/5 直接取官方原样；
#   其余步骤（classes7 框架 dex / libstarry_magic.so / libc++_shared.so / Manifest / 签名）全部不变。
SKIP_DEXPATCH="${ENDFIELD_SKIP_DEXPATCH:-0}"
# ENDFIELD_REASSEMBLE_DEX5=1：V2 二分用 —— classes5.dex 只做 baksmali→smali 重组、零语义改动后替换回包；
#   classes2/3/6 保持官方原字节（配合 ENDFIELD_SKIP_DEXPATCH=1 使用）。门禁：dex 头自洽 + 类数不变 + 反汇编回读一致。
REASSEMBLE_DEX5="${ENDFIELD_REASSEMBLE_DEX5:-0}"
# ENDFIELD_SIG_KILLER=1：C 轨证书回灌（官方 dex 零改动）—— 把烘焙好的 killer 本体并进 classes7。
#   触发点是既有 Provider 路径：StarryInitProvider.onCreate 内的 SignatureKillerBootstrap.install()
#   → Class.forName("bin.mt.signature.KillerApplication193")，安装动作全在该类 <clinit>。
#   一般与 ENDFIELD_SKIP_DEXPATCH=1 同用；烘焙脚本 scripts/endfield_killer_dex_1.7.2.sh；
#   方案与依据：10_docs/终末地/交付_1.7.2_20261126.md §二 第 3 项。
SIG_KILLER="${ENDFIELD_SIG_KILLER:-0}"
# ENDFIELD_NO_MENU_SVC=1：诊断用 —— Manifest **不注入** com.starry.magic.menu.MenuService（浮窗/前台服务），
#   其余注入照旧（StarryInitProvider / killer / so）。用于隔离「MenuService 反复重建/停止」这条嫌疑。
NO_MENU_SVC="${ENDFIELD_NO_MENU_SVC:-0}"
KILLER_SCRIPT="$SCRIPT_DIR/endfield_killer_dex_1.7.2.sh"
OUT="${ENDFIELD_OUT:-$NIXIANG/08_output/终末地}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.hypergryph.endfield"
OFFICIAL_APP="Lcom/u8/sdk/U8Application;"
SIGNED="$OUT/终末地_${VERSION}_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/终末地_${VERSION}_surgical-debugSigned.apk"
[[ "${STARRY_TRACE:-0}" == "1" ]] && SIGNED="$OUT/终末地_${VERSION}_surgical-trace-releaseSigned.apk"
# 1.7.2 首次出包：缓存目录现场生成（07_patched/终末地/surgical_1.7.2/）
MF_CACHE="$NIXIANG/07_patched/终末地/surgical_${VERSION}/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: no build-tools found"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK (set ENDFIELD_BASE_APK)"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS"; exit 1; }
[[ -f "$DEXPATCH_PY" ]] || { echo "ERROR: missing $DEXPATCH_PY"; exit 1; }

# ---- dex 目标自动探测（禁止硬编码 classesN：换代会改 dex 数量与布局）----
DEX_N="$(python3 "$NIXIANG/04_scripts/_common/next_free_dex.py" "$ORIG_APK")"
case "$DEX_N" in ''|*[!0-9]*) echo "ERROR: bad dex index '$DEX_N'"; exit 1;; esac
DEX_NAME="classes${DEX_N}.dex"

# ---- 私有 mk 自检（现代入口合同）----
MK="$STARRY_SRC/app/src/main/jni/$TAG_MK"
[[ -f "$MK" ]] || { echo "ERROR: missing $MK"; exit 1; }
grep -E "STARRY_GAME_ENDFIELD" "$MK" >/dev/null \
  || { echo "ERROR: $TAG_MK must define STARRY_GAME_ENDFIELD"; exit 1; }
grep -E "Games/EndfieldMod\.cpp" "$MK" >/dev/null \
  || { echo "ERROR: $TAG_MK must compile Games/EndfieldMod.cpp"; exit 1; }
grep -E "Games/EndfieldEntry\.cpp" "$MK" >/dev/null \
  || { echo "ERROR: $TAG_MK must compile Games/EndfieldEntry.cpp"; exit 1; }
echo "[check] $TAG_MK target=endfield dex=$DEX_NAME vc=$VC"

LOCK_DIR="/tmp/endfield_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/endfield_surgical.XXXXXX)"
trap 'if [[ "${ENDFIELD_KEEP_WORK:-0}" == "1" ]]; then echo "KEEP WORK=$WORK"; else rm -rf "$WORK"; fi; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# ------------------------------------------------------------------
# 装机段：构建路径与「只装机」路径共用同一份逻辑（避免两处漂移）
# 本轮（2026-11-25）任务口径＝只出包不装机 ⇒ 默认不进入本函数
# ------------------------------------------------------------------
endfield_install() {
    SERIAL_ARG=()
    [[ -n "${SERIAL:-}" ]] && SERIAL_ARG=(-s "$SERIAL")
    REMOTE="/data/local/tmp/endfield_${VERSION//./}_surgical.apk"
    APPEXT="/sdcard/Android/data/${PKG}"
    RESERVE="/sdcard/Android/data/${PKG}.nxreserve"
    adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" push "$SIGNED" "$REMOTE"

    if [[ "${STARRY_CLEAN_INSTALL:-0}" == "1" ]]; then
      echo "[install] CLEAN uninstall（异签名必须；已获用户授权卸载重装才可走）"
      adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell am force-stop "$PKG" 2>/dev/null || true

      # ---- 外置资源保护（1.6.4 实测 /sdcard/Android/data/com.hypergryph.endfield/files/VFS = 29,467 MB）----
      # 卸载可能连带删 /sdcard/Android/data/<pkg>；同卷 rename 瞬时完成、零额外空间。
      # **先落备胎并核对非空才继续**：备胎失败即中止，绝不裸卸（数十 GB 重下代价过高）。
      echo "[install] 保护外置资源目录 → $RESERVE"
      adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell \
        "su -c 'rm -rf \"$RESERVE\"; if [ -d \"$APPEXT\" ]; then mv \"$APPEXT\" \"$RESERVE\"; fi; ls -d \"$RESERVE\" 2>/dev/null'" \
        > "$WORK/reserve_check.txt" 2>&1 || true
      cat "$WORK/reserve_check.txt"
      if grep -q "$RESERVE" "$WORK/reserve_check.txt"; then
        echo "[install] 备胎就位（mv 成功）"
      else
        echo "[WARN] 外置资源目录不存在或 mv 失败 → 卸载会丢数十 GB 热更资源"
        if [[ "${ENDFIELD_ALLOW_RESOURCE_LOSS:-0}" == "1" ]]; then
          echo "[WARN] ENDFIELD_ALLOW_RESOURCE_LOSS=1 → 按用户显式授权继续"
        else
          echo "       已中止卸载。确认可丢资源时置 ENDFIELD_ALLOW_RESOURCE_LOSS=1 重跑"
          exit 1
        fi
      fi

      adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell "su -c 'pm uninstall $PKG'" || true
    else
      echo "[install] cover install -r -d (no uninstall)"
      adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell am force-stop "$PKG" 2>/dev/null || true
    fi

    adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\" && rm -f \"$REMOTE\"'" \
      || adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" install -r -d "$SIGNED"

    # ---- 资源归位：重装后 app uid 可能变，必须 chown 到新 uid，否则 app 读不到自己的外置目录 ----
    if [[ "${STARRY_CLEAN_INSTALL:-0}" == "1" ]]; then
      NEW_UID="$(adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell "su -c 'stat -c %u /data/data/$PKG 2>/dev/null'" 2>/dev/null | tr -d '\r' | head -1)"
      case "$NEW_UID" in ''|*[!0-9]*) echo "[WARN] 取不到新 uid（'$NEW_UID'）→ 跳过归位，需人工处理"; NEW_UID="";; esac
      if [[ -n "$NEW_UID" ]]; then
        echo "[install] 资源归位 → $APPEXT (uid=$NEW_UID)"
        adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell \
          "su -c 'R=\"$RESERVE\"; T=\"$APPEXT\"; if [ ! -d \"\$R\" ]; then echo NO_RESERVE; elif [ -d \"\$T/files/VFS\" ]; then echo TARGET_ALREADY_POPULATED; else rm -rf \"\$T\"; mv \"\$R\" \"\$T\"; chown -R $NEW_UID:$NEW_UID \"\$T\"; restorecon -R \"\$T\" 2>/dev/null; du -sm \"\$T\" 2>/dev/null; echo RESTORED; fi'"
      fi
    fi

    bash "$NIXIANG/04_scripts/_common/ensure_starry_device_id_seed.sh" || true
    echo "log: adb logcat -s StarryApp:V StarryMagic:V StarryTrace:V Mod_Menu:V MenuService:V TapInitializer:V"
}

# ---- 只装机模式：复用现有正式包（不重建）----
# 约定见 skill/starry-mod/SKILL.md「存量游戏快速重打」：STARRY_INSTALL_ONLY=1
# 语义 = **只做验证 + 安装**；同时也避免重建覆盖已取证的正式包（重签会改 md5）。
if [[ "${STARRY_INSTALL_ONLY:-0}" == "1" ]]; then
  [[ -f "$SIGNED" ]] || { echo "ERROR: STARRY_INSTALL_ONLY=1 但正式包不存在：$SIGNED"; exit 1; }
  echo "[install-only] 复用正式包（不重建）：$SIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" > "$WORK/signer.txt" 2>&1 || true
  grep -q "CN=StarryMagic" "$WORK/signer.txt" || { echo "ERROR: 正式包签名身份不是 StarryMagic"; cat "$WORK/signer.txt"; exit 1; }
  unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/io.so"
  grep -a -F -q "$SO_LABEL" "$WORK/io.so" || { echo "ERROR: 包内 so 不是 ENDFIELD"; exit 1; }
  grep -a -F -q "$AUTH_ID" "$WORK/io.so" || { echo "ERROR: 包内 so 缺 game_id"; exit 1; }
  echo "  so md5=$(md5_of "$WORK/io.so")  apk md5=$(md5_of "$SIGNED")  size=$(stat -f %z "$SIGNED")"
  endfield_install
  echo "DONE(install-only): $SIGNED"
  exit 0
fi

echo "[1/7] ndk-build ($TAG_MK) -> libs_${TAG_MK_TAG} (STARRY_TRACE=${STARRY_TRACE:-0})"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
NDK_EXTRA=()
if [[ "${STARRY_TRACE:-0}" == "1" ]]; then
  NDK_EXTRA+=(STARRY_TRACE=1)
fi
# 诊断构建：崩溃落盘 handler（ENDFIELD_CRASHLOG=1 才编进 so，默认不启用）
if [[ "${ENDFIELD_CRASHLOG:-0}" == "1" ]]; then NDK_EXTRA+=(STARRY_ENDFIELD_CRASHLOG=1); fi
OBJ_OUT="$STARRY_SRC/obj_${TAG_MK_TAG}"
LIBS_OUT="$STARRY_SRC/libs_${TAG_MK_TAG}"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./"$TAG_MK" NDK_APPLICATION_MK=./Application.mk \
  NDK_OUT="$OBJ_OUT" NDK_LIBS_OUT="$LIBS_OUT" \
  "${NDK_EXTRA[@]+"${NDK_EXTRA[@]}"}" -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$LIBS_OUT/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -F -q "$SO_LABEL" "$SO_BUILT" || ! grep -a -F -q "$AUTH_ID" "$SO_BUILT"; then
  echo "ERROR: so 不是 ENDFIELD"
  strings "$SO_BUILT" | grep -E 'Endfield|mrfzzmd|Wggls|\[Wggls\]|gjc2' | sort -u | head -20
  exit 1
fi
# 标签一律按**字面量**比（-F）：写成 grep '[Wggls]' 是字符类，会匹配任意单个 W/g/l/s
# → 正查假通过、反查假失败（2026-10-16 实测踩到）
for bad in '[Wggls]' '[Gjc2]' '[Dwct]' '[Cstjjs]' '[Hzsj]' '[Wxsj]'; do
  if grep -a -F -q -- "$bad" "$SO_BUILT"; then
    echo "ERROR: so 仍含上一款标签 $bad"; exit 1
  fi
done
# 菜单串字节自检（中文必须按字节搜：strings 默认 ASCII 不认 UTF-8）—— 4 项，不多不少
for menu in '百倍伤害' '无敌' '无限跳跃' '无限技能'; do
  if ! LC_ALL=C grep -aq "$menu" "$SO_BUILT"; then
    echo "ERROR: so 缺菜单串 $menu"; exit 1
  fi
done
# 构建戳已随 OTA 下线退役（2026-11-21）：so 内不再要求构建戳，也不 touch Includes/Utils.cpp
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ($SO_LABEL/$AUTH_ID) md5=$(md5_of "$SO_BUILT")"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -F -q "$SO_LABEL" "$WORK/check.so" || { echo "ERROR: gradle APK so 不是 ENDFIELD"; exit 1; }

echo "[2/7] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] $DEX_NAME = com/starry (StarryApp → U8Application)"
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
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/u8/sdk/U8Application;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/u8/sdk/U8Application;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/u8/sdk/U8Application;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/u8/sdk/U8Application;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if "Lcom/u8/sdk/U8Application;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing U8Application")
supers = [ln for ln in t.splitlines() if ln.startswith(".super ")]
if not supers or supers[0].strip() != ".super Lcom/u8/sdk/U8Application;":
    raise SystemExit("ERROR: bad .super: %s" % supers[:3])
p.write_text(t, encoding="utf-8")
print("  StarryApp → U8Application ok (%d edits)" % changed)
PY
# C 轨（opt-in）：killer 只进我们自己的 $DEX_NAME；官方 classes*.dex 一个字节都不动。
# 合并形态有先例：mod菜单/starry-mod-menu/scripts/surgical_bysxy_1.0.40.sh（killer smali 并进框架 dex）。
if [[ "$SIG_KILLER" == "1" ]]; then
  echo "[3c/7] C 轨证书回灌：killer smali 并入 ${DEX_NAME}（官方 dex 零改动）"
  bash "$KILLER_SCRIPT" --base-apk "$ORIG_APK" --merge-into "$SMALI_ROOT" \
    --out "$WORK/endfield_signature_killer.dex"
fi
"$SMALI_BIN" assemble -o "$WORK/$DEX_NAME" "$SMALI_ROOT"
ls -la "$WORK/$DEX_NAME"

# TapTap 签名/登录 ＝ A 轨静态改 dex（2026-11-25 定案，验签路线 §4.1）：
#   classes2 TapSignKit.getSign → 官方证书 md5；classes5 门闩 + 掐弹窗；classes3 登录 action 等长改写。
# 不再有任何运行时签名伪装（该类已删除，禁止复活）。补丁脚本自带四条断言与 dex 头自校验。
DEXPATCH_OUT="$WORK/dexpatch"
if [[ "$SKIP_DEXPATCH" == "1" ]]; then
  echo "[3b/7] SKIP：ENDFIELD_SKIP_DEXPATCH=1 —— 不跑 dex patch（classes2/3/5 保持官方原样）"
else
  echo "[3b/7] A 轨 dex 补丁（签名回灌 / 门闩 / 掐弹窗 / 登录入口）"
  DEXPATCH_EXTRA=()
  if [[ "${ENDFIELD_TAP_NO_SIGN:-0}" == "1" ]]; then DEXPATCH_EXTRA+=(--no-sign); fi
  python3.11 "$DEXPATCH_PY" patch --base-apk "$ORIG_APK" --out-dir "$DEXPATCH_OUT" \
    --workdir "$WORK/dexpatch_work" "${DEXPATCH_EXTRA[@]+"${DEXPATCH_EXTRA[@]}"}"
fi

if [[ "$REASSEMBLE_DEX5" == "1" ]]; then
  echo "[3c/7] V2：classes5.dex 重组（baksmali→smali，零语义改动）"
  REASM="$WORK/reasm5"
  rm -rf "$REASM"; mkdir -p "$REASM"
  python3.11 - "$ORIG_APK" "$REASM/classes5.orig.dex" <<'PY'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as z, open(sys.argv[2], "wb") as f:
    f.write(z.read("classes5.dex"))
print("  extracted official classes5.dex -> %s" % sys.argv[2])
PY
  DEX_VER="$(python3.11 -c 'import sys;print(open(sys.argv[1],"rb").read(8)[4:7].decode())' "$REASM/classes5.orig.dex")"
  # dexlib2 的 api→dex version 映射：035→23 / 037→24 / 038→26 / 039→28（版本必须与原 dex 一致）
  case "$DEX_VER" in 035) DV_API=23;; 037) DV_API=24;; 038) DV_API=26;; 039) DV_API=28;; *) DV_API=24;; esac
  echo "  dex version=$DEX_VER → smali --api=$DV_API"
  "$NIXIANG/01_tools/smali/baksmali" disassemble "$REASM/classes5.orig.dex" -o "$REASM/smali" >/dev/null
  "$SMALI_BIN" assemble -a "$DV_API" -o "$REASM/classes5.dex" "$REASM/smali"
  python3.11 - "$REASM/classes5.orig.dex" "$REASM/classes5.dex" <<'PY'
import sys, struct, zlib, hashlib
a = open(sys.argv[1], "rb").read(); b = open(sys.argv[2], "rb").read()
def hdr(d):
    if d[:4] != b"dex\n": raise SystemExit("ERROR: bad dex magic")
    ver = d[4:7].decode()
    checksum = struct.unpack_from("<I", d, 8)[0]
    sig = d[12:32]
    file_size = struct.unpack_from("<I", d, 32)[0]
    class_defs_size = struct.unpack_from("<I", d, 96)[0]
    if file_size != len(d): raise SystemExit("ERROR: file_size %d != 实际 %d" % (file_size, len(d)))
    if hashlib.sha1(d[32:]).digest() != sig: raise SystemExit("ERROR: sha1 签名区不自洽")
    if (zlib.adler32(d[12:]) & 0xFFFFFFFF) != checksum: raise SystemExit("ERROR: adler32 checksum 不自洽")
    return ver, class_defs_size, len(d)
va, ca, la = hdr(a); vb, cb, lb = hdr(b)
print("  orig : ver=%s classes=%d bytes=%d" % (va, ca, la))
print("  reasm: ver=%s classes=%d bytes=%d" % (vb, cb, lb))
if va != vb: raise SystemExit("ERROR: dex version 变化 %s→%s" % (va, vb))
if ca != cb: raise SystemExit("ERROR: class 数变化 %d→%d" % (ca, cb))
print("  [ok] dex 头 checksum/adler32/sha1 自洽，类数不变，字节 %+d" % (lb - la))
PY
  if ! "$BT/dexdump" -f "$REASM/classes5.dex" > "$REASM/dexdump.txt" 2>&1; then
    echo "ERROR: dexdump 无法解析重组 dex"; exit 1; fi
  grep -q "class_defs_size" "$REASM/dexdump.txt" || { echo "ERROR: dexdump 输出缺 class_defs_size"; exit 1; }
  echo "  [ok] dexdump -f 可解析"
  "$NIXIANG/01_tools/smali/baksmali" disassemble "$REASM/classes5.dex" -o "$REASM/smali_rt" >/dev/null
  if ! diff -r "$REASM/smali" "$REASM/smali_rt" > "$REASM/smali_diff.txt" 2>&1; then
    echo "ERROR: 重组 dex 反汇编与原始反汇编不一致（存在结构/语义差异）"; sed -n '1,20p' "$REASM/smali_diff.txt"; exit 1; fi
  echo "  [ok] 反汇编回读逐文件一致（零语义改动）; md5=$(md5_of "$REASM/classes5.dex")"
fi

echo "[4/7] 注入 so（libstarry_magic.so + libc++_shared.so）"
SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
# 官方包无 libc++_shared.so（1.7.2 实测 grep 计数 0），我方 so 的 DT_NEEDED 需要它 ⇒ 必须注入
[[ -f "$NDK_CXX" ]] || { echo "ERROR: 缺 libc++_shared.so（${NDK_CXX}）；不允许静默跳过"; exit 1; }
cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
echo "  libc++_shared.so <- $NDK_CXX"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[5/7] binary AndroidManifest (mini apktool，不对原包 apktool b)"
if [[ -n "${ENDFIELD_BINARY_MANIFEST:-}" && -f "${ENDFIELD_BINARY_MANIFEST:-}" ]]; then
  cp "${ENDFIELD_BINARY_MANIFEST}" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" && "${NO_MENU_SVC:-0}" != "1" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once via mini zip"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src)
zout = zipfile.ZipFile(dst, "w")
for name in keep:
    info = zin.getinfo(name)
    data = zin.read(name)
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, data)
zin.close(); zout.close()
print("  mini apk %d bytes" % dst.stat().st_size)
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import os, sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 替换官方 Application 会叠加第二个 android:name；且 20260902 实测终末地 ACE
# initialize 校验 Manifest Application 类名，换成 StarryApp 会 native 信号量毒丸挂起
# （黑屏 Splash）。因此【不替换 Application】，改由 StarryInitProvider 自动初始化。
if 'com.starry.magic.StarryInitProvider' not in t:
    prov = '''<provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.hypergryph.endfield.starryinit"
            android:exported="false" />
        '''
    # 入口铁律：Provider 顺序＝平台实例化顺序，必须排在其它 Provider 之前
    # （TapTapKitInitProvider 等要先拿到回灌的官方证书）。取首个 <provider 之前插入。
    idx = t.find("<provider")
    if idx > 0:
        t = t[:idx] + prov + t[idx:]
    else:
        t = t.replace("</application>", prov + "\n    </application>", 1)
if os.environ.get("ENDFIELD_NO_MENU_SVC", "0") != "1" and "com.starry.magic.menu.MenuService" not in t:
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
print("  text Manifest patched (StarryInitProvider first%s, Application 保留)" %
      ("，MenuService SKIP(ENDFIELD_NO_MENU_SVC=1)" if os.environ.get("ENDFIELD_NO_MENU_SVC", "0") == "1" else " + MenuService"))
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  # NO_MENU_SVC=1 时**不写缓存**：否则会把"无 MenuService"的 Manifest 缓存下来污染后续正常构建
  if [[ "${NO_MENU_SVC:-0}" == "1" ]]; then
    echo "  NO_MENU_SVC=1：跳过写缓存（保持 $MF_CACHE 原样）"
  else
    cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
    echo "  cached → $MF_CACHE"
  fi
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[6/7] zip surgical merge（官方 dex 除 A 轨三份外逐字节保留，arsc/ACE so 不动）"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$DEX_NAME" "$DEXPATCH_OUT" <<'PY'
import sys, os, zipfile, hashlib, re
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
dex_name = sys.argv[4]
patchdir = Path(sys.argv[5])
mf = (work / "AndroidManifest.xml").read_bytes()
c7 = (work / dex_name).read_bytes()

# A 轨改写的三份 dex（补丁脚本产出，必须齐）；SKIP_DEXPATCH=1 时一律用官方原样
skip_dex = os.environ.get("ENDFIELD_SKIP_DEXPATCH", "0") == "1"
patched = {}
if skip_dex:
    print("  [skip-dexpatch] classes2/3/5 取官方原样（dex 零改动 → 落到 protected 面 sha256 校验）")
else:
    for n in ("classes2.dex", "classes3.dex", "classes5.dex"):
        p = patchdir / n
        if not p.is_file():
            raise SystemExit("ERROR: dexpatch 产物缺失 %s（%s）" % (n, patchdir))
        patched[n] = p.read_bytes()
# V2：重组 classes5 覆盖进 patched（仍走同一落地断言）
if os.environ.get("ENDFIELD_REASSEMBLE_DEX5", "0") == "1":
    r5 = work / "reasm5" / "classes5.dex"
    if not r5.is_file():
        raise SystemExit("ERROR: 重组 classes5 缺失 %s" % r5)
    patched["classes5.dex"] = r5.read_bytes()
    print("  [reasm-dex5] classes5.dex 用重组产物（官方字节仅 classes2/3/6）")

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

def is_official_dex(name):
    """官方 classes*.dex：A 轨三份之外一律逐字节保留（ACE 面）"""
    if name == dex_name:
        return False
    if name in patched:
        return False
    if name == "classes.dex":
        return True
    return bool(re.fullmatch(r"classes\d+\.dex", name))

def is_protected(name):
    if name == "resources.arsc":
        return True
    if name.startswith("lib/arm64-v8a/"):
        return True          # 官方 so（含 ACE tersafe2/tprt）逐字节不动
    return is_official_dex(name)

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
replaced = []
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name == dex_name:
        continue
    if name == "AndroidManifest.xml":
        data = mf
    elif name in patched:
        data = patched[name]
        replaced.append(name)
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

info = zipfile.ZipInfo(dex_name)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c7)

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

zin.close()
zout.close()

if sorted(replaced) != sorted(patched):
    raise SystemExit("ERROR: A 轨 dex 未全部落地 replaced=%s" % replaced)

# 保护面复读（从成品包重新读）
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
out_names = set(zout2.namelist())
checked = 0
for name in zin2.namelist():
    if is_protected(name):
        if name not in out_names:
            raise SystemExit("PROTECTED MISSING: %s" % name)
        h1 = hashlib.sha256(zin2.read(name)).hexdigest()
        h2 = hashlib.sha256(zout2.read(name)).hexdigest()
        if h1 != h2:
            raise SystemExit("PROTECTED MUTATED: %s" % name)
        checked += 1
print("  merged ok; entries≈%d; %s=%d bytes; A轨替换=%s; protected keep=%d sha256 match"
      % (n, dex_name, len(c7), ",".join(replaced), checked))
zin2.close(); zout2.close()
PY

# Manifest 复读门禁（黑屏三件事之二）：必须从**二进制** Manifest 复读。
# 禁止 aapt2 ... | grep -q：grep -q 命中即退出 → aapt2 吃 SIGPIPE(141)，
# 在 set -o pipefail 下整条管道判失败（2026-10-16 实测假失败）。先落盘再 grep。
"$BT/aapt2" dump xmltree --file AndroidManifest.xml "$WORK/unsigned.apk" > "$WORK/mf_dump.txt" 2>/dev/null || true
grep -q "com.starry.magic.StarryInitProvider" "$WORK/mf_dump.txt" \
  || { echo "ERROR: StarryInitProvider 未写进 Manifest"; exit 1; }
if [[ "$NO_MENU_SVC" == "1" ]]; then
  grep -q "com.starry.magic.menu.MenuService" "$WORK/mf_dump.txt" \
    && { echo "ERROR: ENDFIELD_NO_MENU_SVC=1 但 MenuService 仍被写进 Manifest"; exit 1; }
  echo "  Manifest 复读 ok: StarryInitProvider 在包内；MenuService 已按开关剔除"
else
  grep -q "com.starry.magic.menu.MenuService" "$WORK/mf_dump.txt" \
    || { echo "ERROR: MenuService 未写进 Manifest"; exit 1; }
  echo "  Manifest 复读 ok: StarryInitProvider + MenuService 均在包内"
fi

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true

# ---- 成品自检（从成品包重新解出，不信中间产物）----
echo "== 成品自检 =="
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
grep -a -F -q "$SO_LABEL" "$WORK/final.so" || { echo "ERROR: final so 标签错"; exit 1; }
grep -a -F -q "$AUTH_ID" "$WORK/final.so" || { echo "ERROR: final so 缺 game_id"; exit 1; }
unzip -p "$SIGNED" "$DEX_NAME" > "$WORK/final.dex"
python3.11 - "$WORK/final.dex" <<'PY'
import sys
from pathlib import Path
d = Path(sys.argv[1]).read_bytes()
need = ["Lcom/starry/magic/StarryInitProvider;", "Lcom/starry/magic/menu/MenuService;"]
miss = [n for n in need if n.encode() not in d]
if miss:
    raise SystemExit("ERROR: final dex 缺类: " + ", ".join(miss))
print("  final dex 类自检 ok (StarryInitProvider / MenuService)")
PY

# C 轨验收：成品包里的 $DEX_NAME 必须含 killer，且包名/证书逐项正确（反汇编回读，不信中间产物）
if [[ "$SIG_KILLER" == "1" ]]; then
  bash "$KILLER_SCRIPT" --verify "$WORK/final.dex"
fi

# 包内 so 清单（官方 31 个 + 我方 2 个；官方 so 已由 merge 逐字节断言）
echo "-- 包内 lib/arm64-v8a 清单 --"
"$BT/aapt2" dump badging "$SIGNED" > "$WORK/badging.txt" 2>/dev/null || true
grep -E "^package" "$WORK/badging.txt" || true
python3.11 - "$SIGNED" <<'PY'
import sys, zipfile, hashlib
z = zipfile.ZipFile(sys.argv[1])
for i in z.infolist():
    if i.filename.startswith("lib/arm64-v8a/") and i.filename.endswith(".so"):
        d = z.read(i)
        print("  %-46s %10d B md5=%s" % (i.filename, len(d), hashlib.md5(d).hexdigest()))
PY

# Manifest provider 顺序（入口铁律：StarryInitProvider 必须在首个 provider 之前）
"$BT/aapt2" dump xmltree --file AndroidManifest.xml "$SIGNED" > "$WORK/mf_final.txt" 2>/dev/null || true
python3.11 - "$WORK/mf_final.txt" <<'PY'
import sys
t = open(sys.argv[1], encoding="utf-8", errors="ignore").read()
i_starry = t.find("com.starry.magic.StarryInitProvider")
i_first = t.find("com.hypergryph.gameupdate.core.GameUpdateApkProvider")
nprov = t.count("E: provider ")
print("  provider 总数=%d  StarryInitProvider@%d  官方首 provider@%d" % (nprov, i_starry, i_first))
if i_starry < 0:
    raise SystemExit("ERROR: 成品 Manifest 里没有 StarryInitProvider")
if i_first >= 0 and i_starry > i_first:
    raise SystemExit("ERROR: StarryInitProvider 不在首个 provider 之前（顺序铁律）")
print("  [ok] StarryInitProvider 位于首个 provider 之前")
PY

# A 轨验收（逐字节对比 + 反汇编回读 + 注入面）；SKIP_DEXPATCH=1 时不适用
if [[ "$SKIP_DEXPATCH" == "1" ]]; then
  echo "  [skip-dexpatch] 跳过 A 轨 verify（classes2/3/5 已由 merge protected sha256 校验兜住）"
else
  python3.11 "$DEXPATCH_PY" verify --apk "$SIGNED" --base-apk "$ORIG_APK"
fi

echo "  so md5=$(md5_of "$WORK/final.so")  $DEX_NAME md5=$(md5_of "$WORK/final.dex")"
ls -lh "$SIGNED"
md5_of "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  endfield_install
fi

echo "DONE: $SIGNED"
