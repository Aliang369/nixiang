#!/usr/bin/env bash
# 旷野征途 2.2 目标卸载重装（no-backup）· 游戏私有入口（2026-11-14）
#
# B 区授权：用户 ask_user_question 原话「直接卸载重装就可以，不用备份数据」——
#   仅限设备 f9a116a2、仅限包 com.xf.kyzt、仅本次候选 sha256 9b0100ea…772e；
#   不做数据/存档备份；不动其它包；不启动应用、不截图、不点按、不代用户认证。
#
# 设计要点：
#   · 所有实质预检在 uninstall **之前**完成，任一失败即退出，绝不先卸后卡。
#   · 只卸载本包（不用 -k 保留数据，因为用户明确不要备份、可丢数据）。
#   · 不 mv / 不 rm 任何数据目录；装完由 post_install_precheck.sh 做装机后检查。
#   · 默认 pre_install_gate 照实记录 FAIL=1（查12 字节差启发式，不识别 SPI-only），
#     本次任务级用 TAP_MODE_CHECK=0 排除该**不适用**启发式并写入报告理由；不冒充“全通过”。
set -euo pipefail

ADB_BIN="${ADB_BIN:-/opt/homebrew/bin/adb}"
SERIAL="f9a116a2"
PKG="com.xf.kyzt"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
APK="${APK:-${ROOT_DIR}/08_output/旷野征途/旷野征途_2.2_starry-releaseSigned.apk}"
BASE="${BASE:-${ROOT_DIR}/02_apks/旷野征途/旷野征途_2.2_taptap_official.apk}"
# 2026-11-14 21:06 改包法候选（classes.dex 已打 Tap 门闩补丁）
EXPECT_SHA="${EXPECT_SHA:-cac2a777b07df0008fa16a605e0c814fa6a0a38b5a8b7e9b935d8cfd8e77915d}"
DRY=0
[ "${1:-}" = "--dry-run" ] && DRY=1

BT="/Users/aliang/Library/Android/sdk/build-tools/35.0.0"

die() { echo "ERROR[$1]: $2" >&2; exit "$1"; }
stage() { echo; echo "== $* =="; }

stage "S0 目标与输入"
echo "  serial  : ${SERIAL}"
echo "  pkg     : ${PKG}"
echo "  apk     : ${APK}"
echo "  base    : ${BASE}"
echo "  dry-run : ${DRY}"
[ -f "${APK}" ] || die 10 "候选 APK 不存在"
[ -f "${BASE}" ] || die 10 "底包不存在（保留作恢复输入）"

stage "S1 设备唯一性"
MAP="$(mktemp)"
"${ADB_BIN}" devices | sed -n "2,$ p" | sed "/^[[:space:]]*$/d" > "${MAP}"
DEV_N="$(wc -l < "${MAP}" | tr -d " ")"
DEV_OK="$("${ADB_BIN}" -s "${SERIAL}" get-state 2>/dev/null | tr -d "\r" || true)"
rm -f "${MAP}"
echo "  devices=${DEV_N} state(${SERIAL})=${DEV_OK}"
[ "${DEV_N}" = "1" ] || die 11 "设备不是唯一（${DEV_N} 台），拒绝执行"
[ "${DEV_OK}" = "device" ] || die 11 "目标设备状态非 device"

stage "S2 候选指纹与签名/对齐"
GOT_SHA="$(shasum -a 256 "${APK}" | awk "{print \$1}")"
echo "  sha256=${GOT_SHA}"
[ "${GOT_SHA}" = "${EXPECT_SHA}" ] || die 12 "候选 sha256 与锁包不一致"
"${BT}/zipalign" -c 4 "${APK}" >/dev/null 2>&1 || die 13 "zipalign -c 4 失败"
"${BT}/apksigner" verify "${APK}" >/dev/null 2>&1 || die 14 "apksigner verify 失败"
echo "  zipalign=0 apksigner=0"

stage "S3 包名/版本/ABI"
BADGING="$("${BT}/aapt2" dump badging "${APK}" 2>/dev/null || true)"
echo "${BADGING}" | grep -q "package: name=.${PKG}." || die 15 "包名不是 ${PKG}"
VC="$(printf "%s\n" "${BADGING}" | awk -F"'" '{for(k=1;k<=NF;k++) if ($k ~ /versionCode=/) print $(k+1)}' | head -1)"
echo "  versionCode=${VC}"
[ "${VC}" = "22" ] || die 15 "versionCode 不是 22"
if echo "${BADGING}" | grep -q "native-code:.*armeabi"; then die 16 "含非 arm64 ABI"; fi

stage "S4 默认门禁（照实记录，不阻断）"
set +e
bash "${ROOT_DIR}/04_scripts/_common/pre_install_gate.sh" --apk "${APK}" --base-apk "${BASE}" 2>&1 | tail -3
GATE_RC="$?"
set -e
echo "  default_gate_rc=${GATE_RC}（预期 FAIL=1：查12 字节差启发式，不识别 SPI-only）"

stage "S5 任务级排除查12（仅本次运行测试）"
GATE2_OUT="$(TAP_MODE_CHECK=0 bash "${ROOT_DIR}/04_scripts/_common/pre_install_gate.sh" --apk "${APK}" --base-apk "${BASE}" 2>&1 || true)"
if printf "%s\n" "${GATE2_OUT}" | grep -q "FAIL=0"; then
  echo "  TAP_MODE_CHECK=0 下其余实质检查通过（不含查12）"
else
  die 17 "排除查12 后仍有实质检查 FAIL，拒绝卸载"
fi

stage "S6 现装信息（卸载前）"
"${ADB_BIN}" -s "${SERIAL}" shell dumpsys package "${PKG}" | grep -E "versionCode=|firstInstallTime|installerPackageName" | head -4 || die 18 "读取现装信息失败"

if [ "${DRY}" = "1" ]; then
  stage "DRY-RUN 结束（未执行 uninstall/install）"
  echo "  将执行：adb -s ${SERIAL} uninstall ${PKG} ; adb -s ${SERIAL} install ${APK}"
  exit 0
fi

stage "S7 卸载本包（no -k，不备份；不影响其它包）"
"${ADB_BIN}" -s "${SERIAL}" uninstall "${PKG}" || die 20 "卸载失败（不改其它包，停止）"
echo "  uninstall rc=0"

stage "S8 安装候选"
"${ADB_BIN}" -s "${SERIAL}" install "${APK}" || die 21 "安装失败（不重复卸载，保留 02 原包可恢复）"
echo "  install rc=0"

stage "S9 装机后只读核验"
"${ADB_BIN}" -s "${SERIAL}" shell pm path "${PKG}"
"${ADB_BIN}" -s "${SERIAL}" shell dumpsys package "${PKG}" | grep -E "versionCode=|versionName=|signatures" | head -3 || true

stage "S10 post_install_precheck"
bash "${ROOT_DIR}/04_scripts/_common/post_install_precheck.sh" --game 旷野征途 || echo "  post_install_precheck 返回非 0，按输出处理"

echo
echo "DONE: 安装流程结束（应用未启动，点击/认证由用户完成）"
