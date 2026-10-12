#!/usr/bin/env bash
# 追迹迷城：按用户明确授权卸载官方包（不保留数据）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="com.TeaMelo.DungeonChasers"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

BEFORE="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"
if [[ -z "$BEFORE" ]]; then
  echo "设备未安装 ${PKG}，无需卸载"
else
  echo "官方包路径: $BEFORE"
  "$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  "$ADB_BIN" -s "$SERIAL" shell pm uninstall --user 0 "$PKG"
fi

AFTER="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"
[[ -z "$AFTER" ]] || { echo "错误: 卸载后仍能查到 $PKG: $AFTER" >&2; exit 1; }

printf '{"serial":"%s","package":"%s","action":"uninstall_no_keep_data","at":"%s"}\n' \
  "$SERIAL" "$PKG" "$(date '+%Y-%m-%d %H:%M:%S')" > "$SCRIPT_DIR/uninstall_done.json"

cat <<EOF
serial=$SERIAL
package=$PKG
uninstall=ok
keep_data=no
EOF
