#!/usr/bin/env bash
# 宝石战争 6.0.012：把设备切回 TapTap 官方原版（用于与星光包对照）。
# 官方包与星光包签名不同，切换必须卸载后再装；需要显式 BSZZ_ALLOW_UNINSTALL=1。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="com.acing.gow.adv"
OFFICIAL="$NIXIANG_ROOT/02_apks/宝石战争/宝石战争_6.0.012_official_from_device.apk"
EXPECT_MD5="f207fc9d5b62b1a956a72c35c6a56def"
RESULT="$SCRIPT_DIR/install_official_result.txt"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "ERROR: missing device serial" >&2; exit 1; }
[[ -f "$OFFICIAL" ]] || { echo "ERROR: missing official apk: $OFFICIAL" >&2; exit 1; }
[[ "${BSZZ_ALLOW_UNINSTALL:-0}" == "1" ]] || {
  echo "ERROR: 切回官方版需卸载当前星光包；请显式设置 BSZZ_ALLOW_UNINSTALL=1" >&2
  exit 1
}

SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
LOCAL_MD5="$(md5 -q "$OFFICIAL")"
[[ "$LOCAL_MD5" == "$EXPECT_MD5" ]] || {
  echo "ERROR: 官方原包 md5 不符（$LOCAL_MD5 != $EXPECT_MD5），拒绝安装" >&2
  exit 1
}

{
  echo "time=$(date '+%Y-%m-%d %H:%M:%S %z')"
  echo "serial=$SERIAL"
  echo "apk=$OFFICIAL"
  echo "local_md5=$LOCAL_MD5"
  echo "mode=restore_official_authorized_uninstall"
} > "$RESULT"

"$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true
set +e
UNINSTALL_OUT="$("$ADB_BIN" -s "$SERIAL" uninstall "$PKG" 2>&1)"
UNINSTALL_RC=$?
set -e
printf '%s\n' "$UNINSTALL_OUT" >> "$RESULT"
echo "uninstall_rc=$UNINSTALL_RC" >> "$RESULT"
if [[ $UNINSTALL_RC -ne 0 ]]; then
  cat "$RESULT"
  exit $UNINSTALL_RC
fi

set +e
INSTALL_OUT="$("$ADB_BIN" -s "$SERIAL" install "$OFFICIAL" 2>&1)"
RC=$?
set -e
printf '%s\n' "$INSTALL_OUT" >> "$RESULT"
echo "install_rc=$RC" >> "$RESULT"
if [[ $RC -ne 0 ]]; then
  echo "官方原版安装失败：星光包已卸载，设备当前无该游戏。" >> "$RESULT"
  cat "$RESULT"
  exit $RC
fi

bash "$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$OFFICIAL" --su | tee -a "$RESULT"
echo "已切回官方原版（卸载安装模式：原星光包本地数据已清除）。" >> "$RESULT"
cat "$RESULT"
