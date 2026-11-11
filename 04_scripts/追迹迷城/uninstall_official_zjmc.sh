#!/usr/bin/env bash
# 追迹迷城：按用户明确授权卸载官方包（不保留数据）。
set -euo pipefail

# ── 本轮卸载确认门（2026-11-11 修复 F4）────────────────────────────────────────
# 卸载属破坏性操作（连带清 app 私有数据 + /sdcard/Android/data），依据 AGENTS §4 必须用户当轮授权。
# 本脚本默认**在任何设备写动作之前非零退出**；只有显式传入本轮唯一令牌才允许继续：
#   --confirm zjmc-uninstall:1.0.20261111:com.TeaMelo.DungeonChasers:f9a116a2
#   （或同名环境变量 ZJMC_UNINSTALL_CONFIRM；不含 serial/版本/包名的令牌一律拒绝）
# --dry-run 只打印将执行的设备命令，不调用任何设备工具、不写卸载标记（离线自检用）。
# 不要为此新增「自动允许」环境变量，也不要用旧日期标记绕过。
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="com.TeaMelo.DungeonChasers"
ADB_BIN="${ADB_BIN:-adb}"
VERSION="1.0.20261111"
SERIAL_EXPECT="${ZJMC_SERIAL:-f9a116a2}"
DRY_RUN=0
CONFIRM="${ZJMC_UNINSTALL_CONFIRM:-}"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --confirm) CONFIRM="${2:-}"; shift 2 ;;
    --confirm=*) CONFIRM="${1#*=}"; shift ;;
    --dry-run) DRY_RUN=1; shift ;;
    *) echo "错误: 未知参数 $1（支持 --confirm <token> / --dry-run）" >&2; exit 2 ;;
  esac
done
TOKEN_EXPECT="zjmc-uninstall:${VERSION}:${PKG}:${SERIAL_EXPECT}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }
if [[ "$SERIAL" != "$SERIAL_EXPECT" ]]; then
  echo "拒绝: 当前设备 $SERIAL 不是本轮授权设备 $SERIAL_EXPECT" >&2
  exit 3
fi

if [[ "$DRY_RUN" == "1" ]]; then
  echo "[dry-run] 未调用任何设备工具、未写 uninstall_done.json"
  echo "[dry-run] 设备: $SERIAL / 包: $PKG / 版本: $VERSION"
  echo "[dry-run] 令牌格式: $TOKEN_EXPECT"
  echo "[dry-run] 将执行: adb -s $SERIAL shell am force-stop $PKG"
  echo "[dry-run] 将执行: adb -s $SERIAL shell pm uninstall --user 0 $PKG"
  exit 0
fi

if [[ -z "$CONFIRM" ]]; then
  echo "拒绝: 未提供本轮卸载确认令牌（当前无用户卸载授权）" >&2
  echo "  需要: --confirm $TOKEN_EXPECT" >&2
  echo "  依据: AGENTS §4 破坏性操作须用户当轮明确确认；本脚本不含任何自动放行开关" >&2
  exit 3
fi
if [[ "$CONFIRM" != "$TOKEN_EXPECT" ]]; then
  echo "拒绝: 确认令牌与「本轮/设备 $SERIAL_EXPECT/版本 $VERSION/包 ${PKG}」不匹配" >&2
  exit 3
fi

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

printf '{"serial":"%s","package":"%s","version":"%s","action":"uninstall_no_keep_data","confirm":"%s","at":"%s"}\n' \
  "$SERIAL" "$PKG" "$VERSION" "$CONFIRM" "$(date '+%Y-%m-%d %H:%M:%S')" > "$SCRIPT_DIR/uninstall_done.json"

cat <<EOF
serial=$SERIAL
package=$PKG
uninstall=ok
keep_data=no
EOF
