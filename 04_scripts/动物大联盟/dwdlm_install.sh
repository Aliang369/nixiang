#!/usr/bin/env bash
# 动物大联盟 1.0.23 — 真机安装验证入口。
# 官方包为 CN=shan，星光包为 CN=StarryMagic，签名不同；覆盖安装会被系统拒绝。
# 只有显式设置 DWDLM_ALLOW_UNINSTALL=1 才允许卸载官方包（会清应用数据）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
APK="$NIXIANG/08_output/动物大联盟/动物大联盟_1.0.23_starry-releaseSigned.apk"
PKG="com.fightingstars.games.animal"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "ERROR: missing $STATE_FILE" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }

if "$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1; then
  echo "INFO: $PKG 已安装，先尝试同签名增量覆盖（保留应用数据）"
  set +e
  INSTALL_OUT="$("$ADB_BIN" -s "$SERIAL" install -r "$APK" 2>&1)"
  INSTALL_RC=$?
  set -e
  printf '%s\n' "$INSTALL_OUT"
  if [[ $INSTALL_RC -ne 0 ]]; then
    if [[ "${DWDLM_ALLOW_UNINSTALL:-0}" != "1" ]]; then
      echo "BLOCK: $PKG 已安装且签名不同。"
      echo "如需安装星光包，将卸载官方包并清除应用数据；确认后设置 DWDLM_ALLOW_UNINSTALL=1 重试。"
      exit 2
    fi
    echo "WARN: 覆盖安装失败；已获显式卸载授权，卸载 $PKG"
    "$ADB_BIN" -s "$SERIAL" uninstall "$PKG"
    "$ADB_BIN" -s "$SERIAL" install -r "$APK"
  fi
else
  "$ADB_BIN" -s "$SERIAL" install -r "$APK"
fi
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
echo "[install] verified $APK"
