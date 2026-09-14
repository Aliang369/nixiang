#!/usr/bin/env bash
# 追迹迷城：安装已签名的星空外科包并校验设备 base.apk 指纹。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
UNINSTALL_MARK="$SCRIPT_DIR/uninstall_done.json"
APK="$NIXIANG_ROOT/08_output/追迹迷城/追迹迷城_1.0.20260912_surgical-releaseSigned.apk"
PKG="com.TeaMelo.DungeonChasers"
REMOTE="/data/local/tmp/zjmc_surgical.apk"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件" >&2; exit 1; }
[[ -f "$UNINSTALL_MARK" ]] || { echo "错误: 缺少用户授权后的卸载记录 $UNINSTALL_MARK" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "错误: 星空 APK 不存在: $APK" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

echo "==> 设备余量"
"$ADB_BIN" -s "$SERIAL" shell df -h /data | sed -n '1,4p'

echo "==> push $APK"
"$ADB_BIN" -s "$SERIAL" push "$APK" "$REMOTE"

echo "==> pm install"
if ! "$ADB_BIN" -s "$SERIAL" shell "pm install -r -d --user 0 '$REMOTE'"; then
  "$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE" || true
  echo "错误: 安装失败；未执行额外清数据或卸载其他内容" >&2
  exit 1
fi
"$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE" || true

echo "==> 验证设备包指纹"
bash "$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "==> 安装身份"
"$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r' | \
  grep -E 'versionName=|versionCode=|primaryCpuAbi=|lastUpdateTime=' | head -10

cat <<EOF
serial=$SERIAL
package=$PKG
apk=$APK
apk_md5=$(md5 -q "$APK")
install=ok
EOF
