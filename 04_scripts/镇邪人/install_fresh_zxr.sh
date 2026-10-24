#!/usr/bin/env bash
# 镇邪人 星空包「干净安装」：卸载设备在装的包 → 全新安装 → 装机完整性校验。
#
# 与 install_zxr.sh（同签覆盖安装）的区别：本脚本用于**签名变更**场景
# （官方原版证书 8fa00ae1… → StarryMagic 证书 83c5cc0b…），无法 -r 覆盖。
# 卸载是破坏性操作：会清 app 私有数据与 /sdcard/Android/data/<pkg>；
# 只在用户明确确认后运行（2026-10-24 用户已确认）。
#
# 用法：
#   SERIAL=f9a116a2 bash 04_scripts/镇邪人/install_fresh_zxr.sh <apk>
#   CONFIRM_UNINSTALL=1 必须显式给出，否则只做 dry-run 打印。
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"
APK="${1:-$ROOT/08_output/镇邪人/镇邪人_1.0.3_cb9dc_star-releaseSigned.apk}"
REMOTE="/data/local/tmp/zxr_star_fresh.apk"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }
adb -s "$SERIAL" get-state >/dev/null 2>&1 || { echo "ERROR: device $SERIAL not online" >&2; exit 1; }

echo "[1/5] 当前设备包状态"
adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d '\r' \
  | grep -E 'versionName=|versionCode=|firstInstallTime=|lastUpdateTime=|signatures=' | head -8 || true

if [[ "${CONFIRM_UNINSTALL:-0}" != "1" ]]; then
  echo "DRY-RUN：未设置 CONFIRM_UNINSTALL=1，不动设备。"
  echo "将执行：pm uninstall ${PKG} → push+pm install ${APK}"
  exit 0
fi

echo "[2/5] 卸载官方原版（破坏性：清 app 数据）"
adb -s "$SERIAL" shell "su -c 'pm uninstall ${PKG}'" 2>&1 | tr -d '\r' || true
if adb -s "$SERIAL" shell pm list packages | tr -d '\r' | grep -Fq "package:${PKG}"; then
  echo "ERROR: 卸载失败，包仍在" >&2
  exit 1
fi
echo "  已卸载"

echo "[3/5] 全新安装星空包"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
INSTALL_OUT="$(adb -s "$SERIAL" shell "su -c 'pm install --user 0 ${REMOTE}'" 2>&1 | tr -d '\r')"
printf '%s\n' "$INSTALL_OUT"
adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true
printf '%s\n' "$INSTALL_OUT" | grep -Fq 'Success' || { echo "ERROR: pm install 失败" >&2; exit 1; }

echo "[4/5] 装机完整性校验（设备包 md5 == 本地 APK）"
bash "$ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "[5/5] 安装后状态"
adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d '\r' \
  | grep -E 'versionName=|versionCode=|firstInstallTime=|lastUpdateTime=|signatures=' | head -8 || true
echo "install_fresh=ok  ($APK)"
