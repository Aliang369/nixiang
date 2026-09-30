#!/usr/bin/env bash
# 勇士之心：已授权卸载原版并安装星光包。
# 该脚本只有显式 ALLOW_UNINSTALL=1 才执行卸载，防止误操作。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"
PKG="${PKG:-com.fightingstars.games.warrior}"
APK="${APK:-$NIXIANG_ROOT/08_output/勇士之心/勇士之心_${VERSION:-1.0.4}_surgical-releaseSigned.apk}"
REMOTE_APK="/data/local/tmp/ysxz_starry_signed.apk"

[[ "${ALLOW_UNINSTALL:-0}" == "1" ]] || {
  echo "错误: 未设置 ALLOW_UNINSTALL=1，拒绝执行卸载安装" >&2
  exit 2
}
[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少 .device_serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "错误: 星光包不存在: $APK" >&2; exit 1; }
# SERIAL 可由环境变量覆盖（例如临时装到另一台测试机，不改 .device_serial）。
SERIAL="${SERIAL:-$(tr -d '[:space:]' < "$STATE_FILE")}"
[[ -n "$SERIAL" ]] || { echo "错误: serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

LOCAL_MD5="$(md5 -q "$APK")"
echo "authorized_uninstall=1"
echo "package=$PKG"
echo "serial=$SERIAL"
echo "apk=$APK"
echo "local_md5=$LOCAL_MD5"

"$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true

echo "==> push 星光包"
"$ADB_BIN" -s "$SERIAL" push "$APK" "$REMOTE_APK" >/dev/null

if "$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1; then
  echo "==> 先尝试同签名覆盖安装（保留当前应用数据）"
  if ! "$ADB_BIN" -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK"; then
    [[ "${ALLOW_UNINSTALL:-0}" == "1" ]] || {
      echo "错误: 覆盖安装失败且未授权卸载" >&2
      exit 1
    }
    echo "==> 覆盖失败，按已授权卸载后重装"
    "$ADB_BIN" -s "$SERIAL" shell pm uninstall --user 0 "$PKG"
    "$ADB_BIN" -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK"
  fi
else
  echo "==> 未检测到已安装包，直接安装"
  "$ADB_BIN" -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK"
fi
"$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE_APK" >/dev/null 2>&1 || true

echo "==> 设备包指纹校验"
if [[ "${VERIFY_MODE:-md5}" == "sig" ]]; then
  # 非 root 设备读不到 /data/app：改用 dumpsys 里只有星光包才有的组件做指纹
  # （官方包没有 StarryInitProvider / MenuService），并打印实际签名供比对。
  DUMP="$("$ADB_BIN" -s "$SERIAL" shell "dumpsys package $PKG" 2>/dev/null | tr -d '\r')"
  echo "$DUMP" | grep -m1 -E "versionName=" || true
  echo "$DUMP" | grep -i -m1 -E "signatures=\[" || true
  if echo "$DUMP" | grep -q -E "com\.starry\.magic\.(StarryInitProvider|menu\.MenuService)"; then
    echo "[install-verify] 设备已注册星光组件 (StarryInitProvider/MenuService) ✓ 非 md5 模式"
  else
    echo "ERROR: dumpsys 未看到星光组件 —— 装上的可能不是星光包" >&2
    exit 1
  fi
else
  bash "$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
fi

echo "install_ok=true"
echo "device_md5=$LOCAL_MD5"
