#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 装机脚本（**默认 dry-run，零写**）
#
# 设计口径（2026-11-05，本轮用户约束）：
#   * 不带参数 = dry-run：不调用任何 adb 写命令、不推包、不安装、不启动、不卸载、不清数据、不截图。
#     真装机必须显式 --apply，且需用户当轮授权（本轮未授权）。
#   * 覆盖安装优先（adb install -r -d）；异签**不自动卸载** —— 直接停下报告，卸载需用户当轮确认。
#   * 内建装机前产物门禁 pre_install_gate.sh（--apk/--base-apk/--cpp/--dex/--menu-dex 全上下文），
#     FAIL 即 exit 1 阻止装机；要跳过必须显式 SKIP_PRE_INSTALL_GATE=1 并在报告写明理由。
#   * 查10 菜单新鲜度：本作 dex 由 surgical 脚本在**本次构建内**现编（gradle→smali + javac+d8 工厂），
#     不是模板产物、不带 starry-menu 戳 ⇒ 用 REQUIRE_STAMP=0（门禁自身支持的"无戳但现编"模式，
#     输出 WARN 而非 SKIP），并以 assemble_<profile>.json 里的构建输入 sha 作为替代新鲜度证据。
#
# 用法：
#   bash 04_scripts/动物大联盟/install_dwdlm.sh                 # dry-run（默认，零写）
#   bash 04_scripts/动物大联盟/install_dwdlm.sh --apply          # 真装机（需用户当轮授权）
#   SERIAL=f9a116a2 bash 04_scripts/动物大联盟/install_dwdlm.sh --apply
set -euo pipefail

MODE="dry"
for a in "$@"; do
  case "$a" in
    --dry-run) MODE="dry" ;;
    --apply)   MODE="apply" ;;
    -h|--help) sed -n "2,19p" "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $a" >&2; exit 2 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="动物大联盟"
PKG="com.fightingstars.games.animal"
SERIAL="${SERIAL:-f9a116a2}"
PROFILE="${DWDLM_SO_PROFILE:-env}"
APK="${DWDLM_APK:-$NIXIANG/08_output/$GAME/${GAME}_1.0.25_starry-${PROFILE}-releaseSigned.apk}"
BASE="$NIXIANG/02_apks/$GAME/${GAME}_1.0.25_official.apk"
JNI="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni"
REMOTE="/data/local/tmp/dwdlm_install.apk"

say() { printf '%s\n' "$*"; }
die() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }

[[ -f "$APK" ]]  || die "缺成品包: ${APK}（先跑 pipeline assemble_env）"
[[ -f "$BASE" ]] || die "缺底包: $BASE"

APK_SHA="$(shasum -a 256 "$APK" | awk "{print \$1}")"
APK_SIZE="$(stat -f "%z" "$APK")"
say "[check] APK   : $APK"
say "[check] size  : $APK_SIZE"
say "[check] sha256: $APK_SHA"
say "[check] base  : $BASE"

# ---- 1. 装机前产物门禁（真跑，参数一次配齐；见 gate_dwdlm.sh 头注释）------
if [[ "${SKIP_PRE_INSTALL_GATE:-0}" == "1" ]]; then
  say "[gate] 显式跳过 pre_install_gate（SKIP_PRE_INSTALL_GATE=1）—— 必须在报告里写明理由"
else
  set +e
  DWDLM_APK="$APK" bash "$SCRIPT_DIR/gate_dwdlm.sh"
  GATE_RC=$?
  set -e
  if [[ $GATE_RC -ne 0 ]]; then
    die "pre_install_gate FAIL（rc=${GATE_RC}）=> 禁止装机；逐项原因见上"
  fi
fi

# ---- 2. dry-run（默认）：零写，只打印计划 ---------------------------------
if [[ "$MODE" == "dry" ]]; then
  say ""
  say "[dry-run] 本模式**不调用 adb**，零写（不推包/不安装/不启动/不卸载/不清数据/不截图）。"
  say "[dry-run] 加 --apply 时将执行："
  say "  1) adb -s $SERIAL get-state                                            # 只读：确认设备在线"
  say "  2) adb -s $SERIAL shell dumpsys package $PKG | grep versionName/versionCode  # 只读：现装版本"
  say "  3) 签名比对：异签 => **停下报告，不自动卸载**（卸载需用户当轮确认）"
  say "  4) adb -s $SERIAL push $APK $REMOTE"
  say "  5) adb -s $SERIAL shell su -c pm install -r -d --user 0 $REMOTE"
  say "  6) adb -s $SERIAL shell rm -f $REMOTE"
  say "  7) bash 04_scripts/_common/verify_installed_apk.sh $SERIAL $PKG <apk> --su"
  say "  8) bash 04_scripts/_common/post_install_precheck.sh --game $GAME"
  say "[dry-run] 明确不做：force-stop / am start / pm uninstall / pm clear / screencap / input tap"
  say "[dry-run] 结论：可审阅、零写；真装机需 --apply + 用户当轮授权"
  exit 0
fi

# ---- 3. 真装机（--apply；本轮未授权，仅授权后使用）------------------------
command -v adb >/dev/null 2>&1 || die "缺 adb"
adb -s "$SERIAL" get-state >/dev/null || die "设备 $SERIAL 不在线"
say "[apply] 现装版本（只读）："
adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d "\r" \
  | grep -E "versionName=|versionCode=|firstInstallTime=|lastUpdateTime=" | head -8 || true
INSTALLED_CERT="$(adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d "\r" \
  | grep -oE "signatures=\[[^]]*\]" | head -1 || true)"
say "[apply] 现装签名: ${INSTALLED_CERT:-（读不到；异签前先人工确认）}"
bash "$NIXIANG/04_scripts/_common/install_guard.sh" "$GAME" --cmd "adb -s $SERIAL install -r -d $APK" || \
  die "install_guard 拒绝该装机命令"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null || die "push 失败"
set +e
OUT="$(adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 $REMOTE'" 2>&1 | tr -d "\r")"
RC=$?
set -e
printf "%s\n" "$OUT"
adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true
if [[ $RC -ne 0 ]] || ! printf "%s\n" "$OUT" | grep -Fq "Success"; then
  die "pm install 失败 rc=${RC}；**未自动卸载、未清数据**（异签请先向用户确认）"
fi
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
say "[apply] 装机完成（未启动、未卸载、未清数据）"
say "[apply] 下一步由用户手动进游戏；机器侧只读取证 post_install_precheck.sh --game $GAME"
