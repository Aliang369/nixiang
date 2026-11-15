#!/usr/bin/env bash
# 无限升级 断网版 wxsjdz1 —— 装机 + 冷启 + 抓日志（不截图、不点屏）
# 用法：bash 04_scripts/无限升级_断网版/install_dz1.sh [等待秒数=45] [apk路径]
set -uo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "${SCRIPT_DIR}/../.." && pwd)"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.xingzuodaoshi.infinityOffline"
ACT="local.privategame.infinity.GateActivity"
OUT="${NIXIANG}/09_test/无限升级_断网版"
WAIT="${1:-45}"
APK="${2:-${NIXIANG}/07_patched/无限升级_断网版/无限升级_12.10701_断网版_星空v1.apk}"
TS="$(date +%Y%m%d_%H%M%S)"
LOG="${OUT}/logcat_dz1_${TS}.txt"
A(){ adb -s "${SERIAL}" "$@"; }
mkdir -p "${OUT}"
[[ -f "${APK}" ]] || { echo "ERROR: missing ${APK}" >&2; exit 1; }

echo "[1/5] 装机：${APK}"
A install -r -d "${APK}" 2>&1 | tail -3
echo "[2/5] 清日志 + force-stop"
A logcat -c || true
A shell am force-stop "${PKG}"
sleep 1
echo "[3/5] 冷启 ${PKG}/${ACT}（后台抓 logcat）"
( A logcat -v threadtime > "${LOG}" 2>&1 ) &
LPID=$!
sleep 2
A shell am start -W -n "${PKG}/${ACT}" 2>&1 | tail -4
echo "[4/5] 等待 ${WAIT}s（不截图不点屏）"
sleep "${WAIT}"
echo "[5/5] 收日志"
kill "${LPID}" 2>/dev/null || true
wait "${LPID}" 2>/dev/null || true
A logcat -d > "${OUT}/logcat_ring_dz1_${TS}.txt" 2>&1 || true
KEY="${OUT}/keylines_dz1_${TS}.txt"
grep -aE 'Wxsjdz1|wxsjdz1|StarryMagic|MenuService|StarryInitProvider|had|FATAL|AndroidRuntime' "${LOG}" > "${KEY}" 2>/dev/null || true
echo "--- 进程"; A shell pidof "${PKG}" || echo "  (no pid)"
echo "--- 关键字"
for k in 'hook installed' 'Exec call len=' 'patched dll injected' 'PASSTHROUGH' 'MISMATCH' 'auth game_id' 'MenuService created' 'il2cpp api' 'FATAL'; do
  n=$(grep -ac -- "$k" "${LOG}" 2>/dev/null || true); printf '  %-24s %s\n' "$k" "${n:-0}"
done
echo "LOG=${LOG}"; echo "KEY=${KEY}"
