#!/usr/bin/env bash
# 动物大联盟：只读拉取真机当前安装的 TapTap 官方 base APK（单一 base，若有 split 直接拒绝）。
# 设备侧只读：pm path / dumpsys package / adb pull。
# root 中转默认关闭（ALLOW_ROOT_TRANSIT=0）：pull 失败即报错并保留证据，不自动提权复制。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
STATE_FILE="${SCRIPT_DIR}/.device_serial"
META_FILE="${SCRIPT_DIR}/official_apk.json"
TXT_FILE="${SCRIPT_DIR}/official_pull.txt"
PKG="${PKG:-com.fightingstars.games.animal}"
GAME="动物大联盟"
EXPECTED_VERSION="${EXPECTED_VERSION:-1.0.25}"
EXPECTED_VERSION_CODE="${EXPECTED_VERSION_CODE:-25}"
ADB_BIN="${ADB_BIN:-adb}"
ALLOW_ROOT_TRANSIT="${ALLOW_ROOT_TRANSIT:-0}"
REMOTE_TMP="/data/local/tmp/dwdlm_official_base.apk"

[ -f "${STATE_FILE}" ] || { echo "错误: 缺少设备状态文件（先跑 device_ready）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "${STATE_FILE}")"
[ -n "${SERIAL}" ] || { echo "错误: serial 为空" >&2; exit 1; }

DUMP="$("${ADB_BIN}" -s "${SERIAL}" shell dumpsys package "${PKG}" | tr -d '\r')"
[ -n "${DUMP}" ] || { echo "错误: dumpsys 无输出（包未安装？）" >&2; exit 1; }

VERSION_NAME="$(printf '%s\n' "${DUMP}" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "${DUMP}" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "${DUMP}" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "${DUMP}" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
FIRST_INSTALL="$(printf '%s\n' "${DUMP}" | sed -n 's/^[[:space:]]*firstInstallTime=//p' | head -1)"
LAST_UPDATE="$(printf '%s\n' "${DUMP}" | sed -n 's/^[[:space:]]*lastUpdateTime=//p' | head -1)"
SPLITS_FIELD="$(printf '%s\n' "${DUMP}" | sed -n 's/^[[:space:]]*splits=\[//p' | head -1 | sed 's/\].*$//')"
APP_FLAGS="$(printf '%s\n' "${DUMP}" | sed -n 's/^[[:space:]]*flags=\[//p' | head -1 | sed 's/\].*$//')"

PACKAGE_PATHS="$("${ADB_BIN}" -s "${SERIAL}" shell pm path "${PKG}" | tr -d '\r' | sed -n 's/^package://p')"
[ -n "${PACKAGE_PATHS}" ] || { echo "错误: pm path 无结果" >&2; exit 1; }
PATH_COUNT="$(printf '%s\n' "${PACKAGE_PATHS}" | sed '/^$/d' | wc -l | tr -d ' ')"
if [ "${PATH_COUNT}" -ne 1 ]; then
  echo "错误: 检测到 ${PATH_COUNT} 个 APK 路径（预期单一 base APK；有 split 需先确认口径）" >&2
  printf '%s\n' "${PACKAGE_PATHS}" >&2
  exit 1
fi
REMOTE_APK="$(printf '%s\n' "${PACKAGE_PATHS}" | sed -n '1p')"
case "${REMOTE_APK}" in
  /data/app/*) ;;
  *) echo "错误: 非预期远端路径 ${REMOTE_APK}" >&2; exit 1 ;;
esac

if [ -z "${VERSION_NAME}" ] || [ "${VERSION_NAME}" != "${EXPECTED_VERSION}" ]; then
  echo "错误: 真机 versionName=${VERSION_NAME:-<空>}，预期 ${EXPECTED_VERSION}" >&2
  exit 1
fi
if [ -z "${VERSION_CODE}" ] || [ "${VERSION_CODE}" != "${EXPECTED_VERSION_CODE}" ]; then
  echo "错误: 真机 versionCode=${VERSION_CODE:-<空>}，预期 ${EXPECTED_VERSION_CODE}" >&2
  exit 1
fi

OUT_DIR="${NIXIANG_ROOT}/02_apks/${GAME}"
OUT="${OUT_DIR}/${GAME}_${VERSION_NAME}_official.apk"
TMP="${OUT}.part"
if [ -e "${OUT}" ]; then
  echo "错误: 已存在，拒绝覆盖原输入: ${OUT}" >&2
  exit 1
fi
mkdir -p "${OUT_DIR}"
rm -f "${TMP}"
trap 'rm -f "${TMP}"' EXIT

PULL_MODE="direct"
if ! "${ADB_BIN}" -s "${SERIAL}" pull "${REMOTE_APK}" "${TMP}"; then
  if [ "${ALLOW_ROOT_TRANSIT}" != "1" ]; then
    echo "错误: adb pull 失败（root 中转默认关闭）；远端=${REMOTE_APK}" >&2
    echo "如需 root 中转: ALLOW_ROOT_TRANSIT=1 重跑，并在报告中说明原因与设备侧残留清理" >&2
    exit 1
  fi
  PULL_MODE="root_transit"
  "${ADB_BIN}" -s "${SERIAL}" shell "su -c 'cp \"${REMOTE_APK}\" \"${REMOTE_TMP}\" && chmod 644 \"${REMOTE_TMP}\"'"
  "${ADB_BIN}" -s "${SERIAL}" pull "${REMOTE_TMP}" "${TMP}"
  "${ADB_BIN}" -s "${SERIAL}" shell "su -c 'rm -f \"${REMOTE_TMP}\"'" || true
fi
[ -s "${TMP}" ] || { echo "错误: 拉取结果为空" >&2; exit 1; }
unzip -tq "${TMP}" >/dev/null || { echo "错误: 拉取到的 APK 未通过 zip 完整性检查" >&2; exit 1; }

mv "${TMP}" "${OUT}"
SIZE="$(stat -f '%z' "${OUT}")"
SHA256="$(shasum -a 256 "${OUT}" | awk '{print $1}')"
MD5="$(md5 -q "${OUT}")"

python3 - "${META_FILE}" "${TXT_FILE}" "${OUT}" "${VERSION_NAME}" "${VERSION_CODE}" \
  "${INSTALLER}" "${PRIMARY_ABI}" "${SERIAL}" "${REMOTE_APK}" "${SIZE}" "${SHA256}" "${MD5}" \
  "${FIRST_INSTALL}" "${LAST_UPDATE}" "${SPLITS_FIELD}" "${APP_FLAGS}" "${PULL_MODE}" "${PATH_COUNT}" <<'PY'
import json, sys
from pathlib import Path
(meta, txt, apk, vn, vc, installer, abi, serial, remote, size, sha256, md5,
 first_install, last_update, splits, flags, pull_mode, path_count) = sys.argv[1:]
data = {
    'game': '动物大联盟',
    'package': 'com.fightingstars.games.animal',
    'kind': 'official',
    'channel': 'taptap',
    'channel_evidence': {
        'installer_package_name': installer or None,
        'first_install_time': first_install or None,
        'last_update_time': last_update or None,
        'note': 'installer 为空=手动/商店侧无记录；渠道以 installer + 包内 TapSDK/壳资源 markers 交叉判定（见 official_check.json）',
    },
    'apk_path': apk,
    'version_name': vn,
    'version_code': int(vc) if vc.isdigit() else vc,
    'primary_cpu_abi': abi or None,
    'split': {
        'pm_path_count': int(path_count),
        'splits_field': splits.split(',') if splits.strip() else [],
        'splits_normalized': [s.strip() for s in splits.split(',')
                              if s.strip().lower() not in ('', 'base')],
        # dumpsys 对非 split 安装同样打印 splits=[base]；判单 base 必须先剔除 base 占位符
        'is_single_base': int(path_count) == 1 and not [
            s for s in splits.split(',') if s.strip().lower() not in ('', 'base')],
    },
    'serial': serial,
    'remote_path': remote,
    'pull_mode': pull_mode,
    'size': int(size),
    'sha256': sha256,
    'md5': md5,
    'app_flags': flags or None,
    'pulled_at': __import__('datetime').datetime.now().strftime('%Y-%m-%d %H:%M:%S'),
}
Path(meta).write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
lines = [
    'package=' + data['package'],
    'versionName=' + str(data['version_name']),
    'versionCode=' + str(data['version_code']),
    'installer=' + str(installer or '<none>'),
    'primaryCpuAbi=' + str(abi or '<none>'),
    'firstInstallTime=' + str(first_install or '<none>'),
    'lastUpdateTime=' + str(last_update or '<none>'),
    'split_pm_path_count=' + str(path_count),
    'split_splits_field=' + str(splits or '<none>'),
    'serial=' + serial,
    'remote_path=' + remote,
    'pull_mode=' + pull_mode,
    'apk=' + apk,
    'size=' + str(data['size']),
    'sha256=' + sha256,
    'md5=' + md5,
]
Path(txt).write_text('\n'.join(lines) + '\n', encoding='utf-8')
print('\n'.join(lines))
PY

echo "[pull_official] done → ${OUT}"
