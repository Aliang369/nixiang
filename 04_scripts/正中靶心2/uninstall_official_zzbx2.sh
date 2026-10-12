#!/usr/bin/env bash
# 正中靶心2：按用户明确授权卸载官方 1.9（不保留本机数据）。
# 授权来源：2026-09-26 用户原话「授权卸载安装」。
# 触发条件：AGENTS.md 要求异签名无法覆盖时先报告、不自动转卸载；
#          已报告并获得授权，故本阶段单独存在、且必须显式带上确认变量。
set -euo pipefail

: "${ZZBX2_CONFIRM_UNINSTALL:=}"   # 必须是 YES，防止误跑
if [[ "$ZZBX2_CONFIRM_UNINSTALL" != "YES" ]]; then
  echo "错误: 未确认卸载。需显式 ZZBX2_CONFIRM_UNINSTALL=YES（用户授权见 2026-09-26「授权卸载安装」）" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="com.openew.monster"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

# 卸载前留证：版本 + 签名者指纹，便于回溯「卸掉的是官方还是星空」
echo "[uninstall] 卸载前现场:"
"$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d '\r' | \
  grep -E 'versionName=|versionCode=|firstInstallTime=|lastUpdateTime=' | head -n 6 || true

BEFORE="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"
if [[ -z "$BEFORE" ]]; then
  echo "设备未安装 ${PKG}，无需卸载"
else
  echo "[uninstall] 官方包路径: $BEFORE"
  "$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  "$ADB_BIN" -s "$SERIAL" shell pm uninstall --user 0 "$PKG"
fi

AFTER="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"
[[ -z "$AFTER" ]] || { echo "错误: 卸载后仍能查到 $PKG: $AFTER" >&2; exit 1; }

printf '{"serial":"%s","package":"%s","action":"uninstall_no_keep_data","authorized_by":"用户 2026-09-26「授权卸载安装」","at":"%s"}\n' \
  "$SERIAL" "$PKG" "$(date '+%Y-%m-%d %H:%M:%S')" > "$SCRIPT_DIR/uninstall_done.json"

cat <<EOF
serial=$SERIAL
package=$PKG
uninstall=ok
keep_data=no
EOF
