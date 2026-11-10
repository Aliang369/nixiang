#!/usr/bin/env bash
# 一念通天 pipeline 阶段 install —— 装机 + 设备包身份校验（版本无关）。
#
# 默认只做同签覆盖安装，失败即停，**绝不自动卸载**（AGENTS.md §4 / 硬约束 A#4）。
# 仅当用户当轮明确授权卸载重装时，才允许设置 YNTT_AUTHORIZED_UNINSTALL=1：
#   卸载旧包 → 安装星光包。注意：卸载会删除 /data/data/com.qinglv.Immortal 本地数据；
#   按「数据备份默认不做」，本脚本不自动备份，需保留存档须另行授权备份。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/yntt_common.sh"

SERIAL_FILE="${SCRIPT_DIR}/.device_serial"
APK="${OUT_DIR}/${GAME}_${YNTT_VER}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/yntt_surgical_${YNTT_VER}.apk"
ADB_BIN="${ADB_BIN:-adb}"
AUTHORIZED_UNINSTALL="${YNTT_AUTHORIZED_UNINSTALL:-0}"

yntt_need_file "${SERIAL_FILE}"
SERIAL="$(tr -d '[:space:]' < "${SERIAL_FILE}")"
[ -n "${SERIAL}" ] || yntt_die "设备 serial 为空（重跑 device_ready 阶段）"
yntt_need_file "${APK}"
command -v "${ADB_BIN}" >/dev/null || yntt_die "adb 不可用"

"${ADB_BIN}" -s "${SERIAL}" get-state >/dev/null
"${ADB_BIN}" -s "${SERIAL}" shell am force-stop "${PKG}" || true

yntt_log "push ${APK}"
"${ADB_BIN}" -s "${SERIAL}" push "${APK}" "${REMOTE}" >/dev/null
trap '"${ADB_BIN}" -s "${SERIAL}" shell rm -f "${REMOTE}" >/dev/null 2>&1 || true' EXIT

yntt_log "尝试覆盖安装"
if ! "${ADB_BIN}" -s "${SERIAL}" shell pm install -r -d --user 0 "${REMOTE}"; then
  yntt_log "覆盖安装失败（异签或降级）。"
  if [ "${AUTHORIZED_UNINSTALL}" != "1" ]; then
    yntt_log "BLOCKED：未执行卸载，本地数据保持原样。"
    yntt_log "如需替换，须用户当轮明确授权后设 YNTT_AUTHORIZED_UNINSTALL=1 重跑。"
    exit 1
  fi
  yntt_log "已授权卸载重装（本地数据将被删除）"
  "${ADB_BIN}" -s "${SERIAL}" shell pm uninstall --user 0 "${PKG}" >/dev/null
  "${ADB_BIN}" -s "${SERIAL}" shell pm install --user 0 "${REMOTE}"
fi

yntt_log "校验设备 base.apk == 本地产物"
bash "${NIXIANG_ROOT}/04_scripts/_common/verify_installed_apk.sh" "${SERIAL}" "${PKG}" "${APK}" --su

yntt_log "done: ${PKG} ${YNTT_VER}"
