#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 原版包只读运行期结构取证（不启动应用、不注入、不改设备状态）
#
# 允许做什么（本轮用户授权）：
#   * 读目标包 pid（pidof），pid 不存在则如实记录并退出，不启动应用；
#   * 读 /proc/<pid>/maps 的行级元数据，只筛与本包运行时相关的库：
#     libsecret / libEncryptorP / libzeus_direct_dex / libcocos2djs / apksecuresolib / base.apk；
#   * 汇总壳映像数与运行时 so 真实路径、是否已出现 libstarry_magic（本作装机版才会出现）。
#
# 明确禁止（脚本层面无任何此类命令）：
#   * am start / monkey / input（不启动、不点屏）；pm install/uninstall/clear（不改包状态）；
#   * frida / gadget / ptrace 注入；/proc/<pid>/mem 读取；存档与账号数据读取；
#   * 不装载任何反 Frida / 反调试服务。
#
# 用法：bash 04_scripts/动物大联盟/probe_official_maps_dwdlm.sh
# 产物：04_scripts/动物大联盟/official_runtime_maps.txt / official_runtime_maps.json
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PKG="${PKG:-com.fightingstars.games.animal}"
ADB_BIN="${ADB_BIN:-adb}"
STATE_FILE="$SCRIPT_DIR/.device_serial"
OUT_TXT="$SCRIPT_DIR/official_runtime_maps.txt"
OUT_JSON="$SCRIPT_DIR/official_runtime_maps.json"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺 .device_serial（先跑 device_ready）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: serial 为空" >&2; exit 1; }

NOW="$(date '+%Y-%m-%d %H:%M:%S')"
PID="$(${ADB_BIN:-adb} -s "$SERIAL" shell "pidof $PKG" 2>/dev/null | tr -d '\r' | tr -d '[:space:]' || true)"

if [[ -z "$PID" ]]; then
  {
    printf 'package=%s\n' "$PKG"
    printf 'serial=%s\n' "$SERIAL"
    printf 'probed_at=%s\n' "$NOW"
    printf 'pid=<none>\n'
    printf 'conclusion=目标包当前未运行：本轮不启动应用、不改设备状态，记录后等待后续必要交互\n'
  } > "$OUT_TXT"
  cat > "$OUT_JSON" <<JSON
{
  "package": "$PKG",
  "serial": "$SERIAL",
  "probed_at": "$NOW",
  "pid": null,
  "running": false,
  "conclusion": "目标包当前未运行；未启动应用、未注入、未改设备状态",
  "read_only": true
}
JSON
  echo "[probe] pid 不存在 → 已记录，未启动应用"
  cat "$OUT_TXT"
  exit 0
fi

MAPS_RAW="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'cat /proc/$PID/maps'" 2>/dev/null | tr -d '\r' || true)"
if [[ -z "$MAPS_RAW" ]]; then
  MAPS_RAW="$("$ADB_BIN" -s "$SERIAL" shell "cat /proc/$PID/maps" 2>/dev/null | tr -d '\r' || true)"
fi
[[ -n "$MAPS_RAW" ]] || { echo "错误: 读不到 /proc/$PID/maps（root 或进程已退）" >&2; exit 1; }

FILTERED="$(printf '%s\n' "$MAPS_RAW" | LC_ALL=C grep -E 'libsecret|libEncryptorP|libzeus_direct_dex|libcocos2djs|apksecuresolib|libstarry_magic|base\.apk' || true)"
TOTAL_LINES="$(printf '%s\n' "$MAPS_RAW" | sed '/^$/d' | wc -l | tr -d ' ')"
HIT_LINES="$(printf '%s\n' "$FILTERED" | sed '/^$/d' | wc -l | tr -d ' ')"
SECRET_MAPS="$(printf '%s\n' "$FILTERED" | LC_ALL=C grep -c 'libsecret\.so' || true)"
COCOS_MAPS="$(printf '%s\n' "$FILTERED" | LC_ALL=C grep -c 'libcocos2djs\.so' || true)"
STARRY_MAPS="$(printf '%s\n' "$FILTERED" | LC_ALL=C grep -c 'libstarry_magic\.so' || true)"
: "${SECRET_MAPS:=0}"; : "${COCOS_MAPS:=0}"; : "${STARRY_MAPS:=0}"
SECRET_MAPS="${SECRET_MAPS}"; COCOS_MAPS="${COCOS_MAPS}"; STARRY_MAPS="${STARRY_MAPS}"

{
  printf 'package=%s\n' "$PKG"
  printf 'serial=%s\n' "$SERIAL"
  printf 'probed_at=%s\n' "$NOW"
  printf 'pid=%s\n' "$PID"
  printf 'maps_total_lines=%s\n' "$TOTAL_LINES"
  printf 'filtered_lines=%s\n' "$HIT_LINES"
  printf 'libsecret_map_lines=%s\n' "$SECRET_MAPS"
  printf 'libcocos2djs_map_lines=%s\n' "$COCOS_MAPS"
  printf 'libstarry_magic_map_lines=%s\n' "$STARRY_MAPS"
  printf -- '---- filtered maps ----\n'
  printf '%s\n' "$FILTERED"
} > "$OUT_TXT"

cat > "$OUT_JSON" <<JSON
{
  "package": "$PKG",
  "serial": "$SERIAL",
  "probed_at": "$NOW",
  "pid": $PID,
  "running": true,
  "maps_total_lines": $TOTAL_LINES,
  "filtered_lines": $HIT_LINES,
  "libsecret_map_lines": $SECRET_MAPS,
  "libcocos2djs_map_lines": $COCOS_MAPS,
  "libstarry_magic_map_lines": $STARRY_MAPS,
  "txt": "$OUT_TXT",
  "read_only": true,
  "note": "只读 /proc/pid/maps 行级元数据；未读 mem、未注入、未启动、未改设备状态"
}
JSON

echo "[probe] pid=$PID maps=$TOTAL_LINES 行，命中 $HIT_LINES 行 → $OUT_TXT"
printf '%s\n' "$FILTERED" | head -20
