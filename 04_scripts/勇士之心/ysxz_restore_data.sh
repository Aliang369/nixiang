#!/usr/bin/env bash
# 勇士之心：星光包安装后还原 ysxz_backup_data.sh 的数据。
# 必须显式 ALLOW_RESTORE=1。步骤：停进程 -> 解包 -> 修正属主与 SELinux 标签 -> 校验。
#
# 用法：ALLOW_RESTORE=1 TAG=20260921 bash ysxz_restore_data.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
PKG="${PKG:-com.fightingstars.games.warrior}"
TAG="${TAG:-20260921}"
ADB_BIN="${ADB_BIN:-adb}"
STATE_FILE="$SCRIPT_DIR/.device_serial"
LOCAL_TAR="$NIXIANG/09_test/勇士之心/backup_$TAG/data_$TAG.tar"
REMOTE_TAR="/data/local/tmp/ysxz_backup_$TAG.tar"
REMOTE_DIR="/data/data/$PKG"

[[ "${ALLOW_RESTORE:-0}" == "1" ]] || { echo "错误: 未设置 ALLOW_RESTORE=1" >&2; exit 2; }
[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少 .device_serial" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"

echo "[1/5] 确认目标包已安装（星光包）"
"$ADB_BIN" -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' | tee "$NIXIANG/09_test/勇士之心/backup_$TAG/restore_pm_path.txt"

echo "[2/5] 停进程"
"$ADB_BIN" -s "$SERIAL" shell "am force-stop $PKG" >/dev/null 2>&1 || true

echo "[3/5] 准备设备端 tar"
if [[ -f "$LOCAL_TAR" ]]; then
  "$ADB_BIN" -s "$SERIAL" push "$LOCAL_TAR" "$REMOTE_TAR" >/dev/null
fi
"$ADB_BIN" -s "$SERIAL" shell "su -c 'ls -la \"$REMOTE_TAR\"'" | tr -d '\r'

echo "[4/5] 解包并修正属主/标签"
"$ADB_BIN" -s "$SERIAL" shell "su -c '
set -e
UID_GID=\$(stat -c %u:%g \"$REMOTE_DIR\")
cd \"$REMOTE_DIR\"
tar -xf \"$REMOTE_TAR\"
chown -R \$UID_GID \"$REMOTE_DIR/files\" \"$REMOTE_DIR/shared_prefs\" \"$REMOTE_DIR/databases\" 2>/dev/null || true
restorecon -R \"$REMOTE_DIR\" 2>/dev/null || true
echo \"uid_gid=\$UID_GID\"
du -sh \"$REMOTE_DIR/files\" \"$REMOTE_DIR/shared_prefs\" \"$REMOTE_DIR/databases\" 2>/dev/null
ls \"$REMOTE_DIR/shared_prefs\" | wc -l
'" | tr -d '\r' | tee "$NIXIANG/09_test/勇士之心/backup_$TAG/restore_result.txt"

echo "[5/5] 完成；请冷启动游戏核对登录状态与热更资源是否直接可用"
echo "restore_ok=true"
