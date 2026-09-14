#!/usr/bin/env bash
# 无悔华夏 4399：按用户明确授权卸载官方包并安装无壳星空包。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SERIAL="${SERIAL:-f9a116a2}"
PKG="${PKG:-com.yhhy.whrhx.m4399}"
ADB_BIN="${ADB_BIN:-adb}"
APK="$NIXIANG_ROOT/08_output/无悔华夏4399/无悔华夏4399_3.8.280_无壳七功能-releaseSigned.apk"
VERIFY="$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh"

[[ -f "$APK" ]] || { echo "ERROR: missing APK: $APK" >&2; exit 1; }
[[ -f "$VERIFY" ]] || { echo "ERROR: missing verify script: $VERIFY" >&2; exit 1; }

echo "==> 卸载官方包（用户已明确授权）"
"$ADB_BIN" -s "$SERIAL" uninstall "$PKG"

echo "==> 安装无壳星空包（1.5G，等待 adb 返回）"
"$ADB_BIN" -s "$SERIAL" install --no-incremental "$APK"

echo "==> 核对设备 APK 指纹"
bash "$VERIFY" "$SERIAL" "$PKG" "$APK" --su

echo "INSTALL_DONE $APK"
