#!/usr/bin/env bash
# 动物大联盟：确认唯一在线真机，并记录 root 可用性（只读；不安装/不清数据/不改设备）。
# 阶段1口径：本阶段全部为只读动作，root 不是前置条件 —— 只记录，不因此失败
# （与 1.0.23 旧版 device_ready_dwdlm.sh 唯一差异：旧版 root 不可用即 exit 1）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="${SCRIPT_DIR}/.device_serial"
OUT_TXT="${SCRIPT_DIR}/device_ready.txt"
ADB_BIN="${ADB_BIN:-adb}"
REQUESTED_SERIAL="${SERIAL:-}"
PKG="${PKG:-com.fightingstars.games.animal}"

command -v "${ADB_BIN}" >/dev/null 2>&1 || { echo "错误: adb 不可用" >&2; exit 1; }

if [ -n "${REQUESTED_SERIAL}" ]; then
  SERIALS="${REQUESTED_SERIAL}"
else
  SERIALS="$("${ADB_BIN}" devices | awk 'NR > 1 && $2 == "device" {print $1}')"
fi

COUNT="$(printf '%s\n' "${SERIALS}" | sed '/^$/d' | wc -l | tr -d ' ')"
if [ "${COUNT}" -ne 1 ]; then
  echo "错误: 需要唯一在线设备，当前识别到 ${COUNT} 台" >&2
  printf '%s\n' "${SERIALS}" >&2
  exit 1
fi

SERIAL="$(printf '%s\n' "${SERIALS}" | sed -n '1p')"
"${ADB_BIN}" -s "${SERIAL}" get-state >/dev/null

ROOT_ID="$("${ADB_BIN}" -s "${SERIAL}" shell "su -c id" 2>/dev/null | tr -d '\r' || true)"
ROOT_STATE="unavailable"
if [ -n "${ROOT_ID}" ]; then
  case "${ROOT_ID}" in
    *uid=0*) ROOT_STATE="available" ;;
  esac
fi
PM_PRESENT="$("${ADB_BIN}" -s "${SERIAL}" shell pm list packages "${PKG}" 2>/dev/null | tr -d '\r' | sed -n 's/^package://p' | head -1 || true)"

printf '%s\n' "${SERIAL}" > "${STATE_FILE}"
{
  printf 'serial=%s\n' "${SERIAL}"
  printf 'root=%s\n' "${ROOT_STATE}"
  printf 'root_id=%s\n' "${ROOT_ID:-<empty>}"
  printf 'pkg=%s\n' "${PKG}"
  printf 'pkg_installed=%s\n' "${PM_PRESENT:-no}"
  printf 'state_file=%s\n' "${STATE_FILE}"
} | tee "${OUT_TXT}"

echo "[device_ready] done → ${OUT_TXT}"
