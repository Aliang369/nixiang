#!/usr/bin/env bash
# 蛋壳战机私服 1.2.1 (vc35) 星空候选包安装后端。
#
# 策略（对齐 AGENTS 分发红线）：
#   1) 设备上已安装同包时，只尝试同签覆盖安装（pm install -r -d --user 0）；
#      失败即停，不自动卸载、不清数据（异签先报告）。
#   2) 设备上未安装时，直接全新安装（本次 2026-10-26 实测设备无 com.wxdmxga.dkzj）。
#   3) 不建数据备份目录（AGENTS：数据备份默认不做）。
#   4) 安装后必须跑 verify_installed_apk.sh；失败即退出。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="蛋壳战机"
VERSION="1.2.1"
PKG="com.wxdmxga.dkzj"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_私服_${VERSION}_starry-releaseSigned.apk"
EXPECT_MD5="${DKZJ_SF_EXPECT_MD5:-}"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"
REMOTE_APK="/data/local/tmp/dkzj_sf_starry_1.2.1.apk"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }

LOCAL_MD5="$(md5 -q "$APK")"
if [[ -n "$EXPECT_MD5" && "$LOCAL_MD5" != "$EXPECT_MD5" ]]; then
  echo "ERROR: candidate md5 mismatch: $LOCAL_MD5 != $EXPECT_MD5" >&2
  exit 1
fi

rm -f "$INSTALL_OK"
adb -s "$SERIAL" get-state >/dev/null

INSTALLED=0
if adb -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | grep -q '^package:'; then
  INSTALLED=1
fi

adb -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true
echo "[install] push $APK ($LOCAL_MD5) installed=$INSTALLED"
adb -s "$SERIAL" push "$APK" "$REMOTE_APK" >/dev/null

cleanup() {
  adb -s "$SERIAL" shell rm -f "$REMOTE_APK" >/dev/null 2>&1 || true
}
trap 'rc=$?; cleanup; exit $rc' EXIT

if [[ "$INSTALLED" -eq 1 ]]; then
  echo "[install] try same-signature cover install"
  if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK"; then
    cat >&2 <<'EOM'

ERROR: 覆盖安装失败（常见原因：设备上包为异签）。本脚本不会自动卸载/清数据。
如需异签替换，请先向用户说明「卸载会清除本地数据」并取得明确授权后再处理。
EOM
    exit 1
  fi
else
  echo "[install] fresh install (package not present)"
  if ! adb -s "$SERIAL" shell pm install --user 0 "$REMOTE_APK"; then
    echo "ERROR: fresh install failed" >&2
    exit 1
  fi
fi

echo "[install] verify device base.apk == local candidate"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
printf '%s %s\n' "$LOCAL_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"
echo "[install] done: $PKG ($LOCAL_MD5)"
