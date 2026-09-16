#!/usr/bin/env bash
# 山海幻境：跨签名替换前卸载官方包（原包已备份；该操作会清空游戏数据）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
MARK_FILE="$SCRIPT_DIR/uninstall_done.json"
PKG="com.chx.shhj"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }

BEFORE="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"
if [[ -z "$BEFORE" ]]; then
  echo "设备未安装 $PKG，无需卸载"
else
  echo "官方包: $BEFORE"
  "$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  "$ADB_BIN" -s "$SERIAL" shell pm uninstall --user 0 "$PKG"
fi

AFTER="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"
[[ -z "$AFTER" ]] || { echo "错误: 卸载后仍存在 $PKG: $AFTER" >&2; exit 1; }

python3 - "$MARK_FILE" "$SERIAL" <<'PY'
import json
import sys
from pathlib import Path

path, serial = sys.argv[1:]
data = {
    "serial": serial,
    "package": "com.chx.shhj",
    "action": "uninstall_official_for_cross_signature_replace",
    "official_apk_backup": "/Users/aliang/Documents/nixiang/02_apks/山海幻境/山海幻境_4_official_from_device.apk",
    "keep_data": False,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

echo "uninstall=ok package=$PKG keep_data=no"
