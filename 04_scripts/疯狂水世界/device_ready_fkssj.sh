#!/usr/bin/env bash
# ================================================================
#  疯狂水世界：锁定真机、记录设备状态，并只读核对目标包。
#
#  底包 = 官方 TapTap 原版 1.11.6（vc480）；装机阶段由 starry_release.jks 重签
#  （与官方 CN=changwan 不同），故此处包存在性检查对「官方版 / 星光版」均接受。
#
#  ★与仙逆先例的差异：本机为**小米真机、无 root**（10_docs/疯狂水世界/硬约束.md H6）。
#    因此 root 不是硬门槛 —— 取不到 root 只记 root=<无> 并继续；
#    设备侧 base.apk 的 md5/size 读不到时留空（/data/app 无 root 读不了），
#    绝不当成失败退出。
#
#  用法: bash 04_scripts/疯狂水世界/device_ready_fkssj.sh
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"
PKG="com.gx.sw.qa.fkssj004.esj"
EXPECTED_SERIAL="${EXPECTED_SERIAL:-96f0d883}"
# 官方 TapTap 原版 1.11.6（519,961,058 B，设备只读拉取所得）
OFFICIAL_MD5="f8ed1b954a3c5ce91dcecb4e721f3e9b"

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
SDK="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.build.version.sdk | tr -d '\r')"
ABI="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.cpu.abi | tr -d '\r')"
PACKAGE_PATH="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"

# root 是「加分项」不是门槛：无 root 时后续取证改用 logcat / /proc/<pid> / frida
ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su -c id' 2>/dev/null | tr -d '\r' || true)"
if ! printf '%s\n' "$ROOT_ID" | grep -q 'uid=0'; then
  ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su 0 id' 2>/dev/null | tr -d '\r' || true)"
fi
if printf '%s\n' "$ROOT_ID" | grep -q 'uid=0'; then
  ROOT_KIND="有（${ROOT_ID%% *}）"
else
  ROOT_KIND="无"
  echo "[device_ready] 提示: 设备无 root —— 按硬约束 H6 继续；取证走 logcat / /proc/<pid> / frida"
fi

DEV_APK_MD5=""
DEV_APK_SIZE=""
if [[ -n "$PACKAGE_PATH" ]]; then
  DEV_APK_MD5="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'md5sum $PACKAGE_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
  DEV_APK_SIZE="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'stat -c %s $PACKAGE_PATH'" 2>/dev/null | tr -d '\r' || true)"
  if [[ -z "$DEV_APK_MD5" ]]; then
    # 无 root：/data/app 读不到，属预期情况，留空
    DEV_APK_MD5="$("$ADB_BIN" -s "$SERIAL" shell "md5sum $PACKAGE_PATH" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
  fi
fi

case "$DEV_APK_MD5" in
  "$OFFICIAL_MD5") DEV_KIND="官方 TapTap 原版 1.11.6（去 taptap 签名 + 注入菜单的底包状态）" ;;
  "")              DEV_KIND="未安装 / 无 root 读不到 md5（本机属预期）" ;;
  *)               DEV_KIND="其它（可能是本星光包或别的版本）" ;;
esac

printf '%s\n' "$SERIAL" > "$STATE_FILE"
cat <<EOF
serial=$SERIAL
model=$MODEL
android=$ANDROID
sdk=$SDK
abi=$ABI
package=$PKG
package_path=${PACKAGE_PATH:-<未安装>}
device_apk_md5=${DEV_APK_MD5:-<读不到>}
device_apk_size=${DEV_APK_SIZE:-<读不到>}
device_kind=$DEV_KIND
root=$ROOT_KIND
state_file=$STATE_FILE
EOF
