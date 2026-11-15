#!/usr/bin/env bash
# 无限升级 断网版 星空探针 —— 装机 + 冷启 + 抓日志 + DIAG 落盘拉回
# 用法：bash 04_scripts/无限升级_断网版/install_probe.sh [等待秒数=45]
# 前置：设备 f9a116a2 已连、已 root（su -c 可用）；用户已授权卸载重装（异签名才卸）
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "${SCRIPT_DIR}/../.." && pwd)"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.xingzuodaoshi.infinityOffline"
ACT="local.privategame.infinity.GateActivity"
APK="${NIXIANG}/07_patched/无限升级_断网版/无限升级_12.10701_断网版_星空探针.apk"
OUT="${NIXIANG}/09_test/无限升级_断网版"
WAIT="${1:-45}"
TS="$(date +%Y%m%d_%H%M%S)"
LOG="${OUT}/logcat_${TS}.txt"

A(){ adb -s "${SERIAL}" "$@"; }
mkdir -p "${OUT}"
[[ -f "${APK}" ]] || { echo "ERROR: missing ${APK}" >&2; exit 1; }

echo "[1/6] 装机（同签覆盖；异签名先卸 —— 已获用户授权）"
A install -r -d "${APK}" 2>&1 | tail -3

echo "[2/6] 清日志 + force-stop"
A logcat -c || true
A shell am force-stop "${PKG}"
sleep 1

echo "[3/6] 起 logcat 捕获（后台，全量落盘）"
( A logcat -v threadtime > "${LOG}" 2>&1 ) &
LPID=$!
sleep 2

echo "[4/6] 冷启 ${PKG}/${ACT}"
A shell am start -W -n "${PKG}/${ACT}" 2>&1 | tail -5

echo "[5/6] 等待 ${WAIT}s（不截图不点屏）"
sleep "${WAIT}"

echo "[6/6] 收日志"
kill "$LPID" 2>/dev/null || true
wait "$LPID" 2>/dev/null || true
A logcat -d > "${OUT}/logcat_ring_${TS}.txt" 2>&1 || true
wc -l "${LOG}" || true

echo "--- 进程状态"
A shell pidof "${PKG}" || echo "  (no pid: 进程不在)"

echo "--- 关键字命中统计"
for k in 'hook installed' 'Exec call len=' 'patched dll injected' 'PASSTHROUGH' 'MISMATCH' 'StarryMagic' 'MenuService' 'RegisterNatives' 'FATAL' 'DIAG dumped' 'Wxsj'; do
  n=$(grep -c -- "$k" "${LOG}" 2>/dev/null || true)
  printf '  %-24s %s\n' "$k" "${n:-0}"
done

echo "--- DIAG 落盘文件"
A shell "su -c 'ls -l /data/user/0/${PKG}/files/ 2>/dev/null'" || true
echo "LOG=${LOG}"
