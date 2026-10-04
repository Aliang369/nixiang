#!/usr/bin/env bash
# 鬼谷八荒 1.1.329：锁定真机、确认 root，并只读核对目标包已安装。
# 引擎：Unity IL2CPP（单机）；底包 = 官方 TapTap 原版 1.1.329（vc329，1,678,372,111 B /
# md5 f6cc1a4eb063bc0505cb93b68020d714），装机阶段由 starry_release.jks 重签（与官方 O=guigugame 不同），
# 故此处包存在性检查对「官方版 / 星光版 / 斌哥改包」均接受（只读识别，不改设备）。
# 注意：底包默认路径 $NIXIANG/02_apks/鬼谷八荒/鬼谷八荒_1.1.329_taptap_official_from_device.apk
# 当前不存在；本机实测底包在 /tmp/ggbh_stock/ggbh_1.1.329_official_from_device.apk，
# 跑 surgical 时用 GGBH_BASE_APK=... 指定（本脚本不读底包）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"
PKG="com.guigugame.guigubahuang"
EXPECTED_SERIAL="${EXPECTED_SERIAL:-f9a116a2}"
OFFICIAL_MD5="f6cc1a4eb063bc0505cb93b68020d714"   # 官方 TapTap 原版 1.1.329（1,678,372,111 B）

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
DEVICES="$("$ADB_BIN" devices | tr -d '\r' | awk 'NR > 1 && $2 == "device" {print $1}')"
COUNT="$(printf '%s\n' "$DEVICES" | sed '/^$/d' | wc -l | tr -d ' ')"
[[ "$COUNT" -eq 1 ]] || {
  echo "错误: 需要恰好一台在线设备，当前为 $COUNT" >&2
  "$ADB_BIN" devices -l >&2
  exit 1
}

SERIAL="$(printf '%s\n' "$DEVICES" | sed -n '1p')"
if [[ -n "$EXPECTED_SERIAL" && "$SERIAL" != "$EXPECTED_SERIAL" ]]; then
  echo "错误: 在线设备为 ${SERIAL}，期望 ${EXPECTED_SERIAL}；拒绝误操作其他设备" >&2
  exit 1
fi

MODEL="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
ANDROID="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.build.version.release | tr -d '\r')"
ABI="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.cpu.abi | tr -d '\r')"
PACKAGE_PATH="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"

ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su -c id' 2>/dev/null | tr -d '\r' || true)"
if ! printf '%s\n' "$ROOT_ID" | grep -q 'uid=0'; then
  ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su 0 id' 2>/dev/null | tr -d '\r' || true)"
fi
printf '%s\n' "$ROOT_ID" | grep -q 'uid=0' || { echo "错误: 设备未取得 root shell" >&2; exit 1; }

DEV_APK_MD5=""
DEV_APK_SIZE=""
if [[ -n "$PACKAGE_PATH" ]]; then
  DEV_APK_MD5="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'md5sum $PACKAGE_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
  DEV_APK_SIZE="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'stat -c %s $PACKAGE_PATH'" 2>/dev/null | tr -d '\r' || true)"
fi

case "$DEV_APK_MD5" in
  "$OFFICIAL_MD5") DEV_KIND="官方 TapTap 原版 1.1.329（未改动的原始底包）" ;;
  "")              DEV_KIND="未安装 / 读不到 md5" ;;
  *)               DEV_KIND="其它（可能是本星光包、斌哥改包或别的版本）" ;;
esac

printf '%s\n' "$SERIAL" > "$STATE_FILE"
cat <<EOF
serial=$SERIAL
model=$MODEL
android=$ANDROID
abi=$ABI
package=$PKG
package_path=${PACKAGE_PATH:-<未安装>}
device_apk_md5=$DEV_APK_MD5
device_apk_size=$DEV_APK_SIZE
device_kind=$DEV_KIND
root=$ROOT_ID
state_file=$STATE_FILE
EOF
