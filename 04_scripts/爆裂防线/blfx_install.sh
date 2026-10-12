#!/usr/bin/env bash
# ================================================================
#  爆裂防线 1.01.06 (com.muyegame.blfx) — 管线 install 后端
#
#  签名语义：本包用 01_tools/starry_release.jks 签名，与官方签名（以及设备上
#  现装的斌哥包 CN=binge2.keystore）都不同。
#  故「同签名覆盖安装」必然是 INSTALL_FAILED_UPDATE_INCOMPATIBLE
#  —— 需要先卸载设备上那一版（官方 或 斌哥改包）。
#
#  授权语义（本项目政策：不默认卸载、不默认清数据）：
#    默认      = 只做同签名覆盖安装（pm install -r -d）；失败即报告并 exit 1。
#    BLFX_ALLOW_UNINSTALL=1
#                = 用户已明确授权「卸载设备上现装的那一版（丢本地数据/存档）+ 全新安装」。
#                  回退包：02_apks/爆裂防线/爆裂防线_1.01.06_original_from_device.apk
#
#  ⚠ 设备上若装的是斌哥改包：卸载它不会丢失原作进度之外的任何东西，
#    但仍然会清掉该包名下的本地存档，故同样必须先取得用户明确授权。
#
#  装机后闭环：verify_installed_apk.sh 比对设备 base.apk 与本地产物 md5。
#
#  用法:
#    bash 04_scripts/爆裂防线/blfx_install.sh                          # 覆盖（签名冲突会失败并提示授权）
#    BLFX_ALLOW_UNINSTALL=1 bash 04_scripts/爆裂防线/blfx_install.sh   # 卸载重装
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="爆裂防线"
VERSION="1.01.06"
PKG="com.muyegame.blfx"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/blfx_surgical.apk"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing ${SERIAL_FILE}（先跑 device_ready_blfx.sh）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing ${APK}（先跑 surgical + sign）" >&2; exit 1; }

rm -f "$INSTALL_OK"

APK_MD5="$(md5 -q "$APK")"
echo "[install] local apk md5=$APK_MD5"
adb -s "$SERIAL" get-state >/dev/null
adb -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1 \
  || { echo "ERROR: 设备上未安装 ${PKG}（先装官方原版/斌哥包，或直接 pm install 全新安装）" >&2; exit 1; }

# 装机前记录设备上现装的是哪一版（只读；卸载后无法再查）
DEV_PATH="$(adb -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
DEV_MD5=""
if [[ -n "$DEV_PATH" ]]; then
  DEV_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
fi
case "$DEV_MD5" in
  1a57be4509b55582f4aae5475aeaf6c6) echo "[install] 设备现存 = 官方原版 1.01.06" ;;
  dda75d4f8466b2b2992ddd2c3e928901) echo "[install] 设备现存 = 斌哥改包 1.01.06" ;;
  "$APK_MD5")                        echo "[install] 设备现存 = 本星光包（同包重装，不会丢数据）" ;;
  "")                                echo "[install] 设备现存 md5 读取失败（将按未知处理）" ;;
  *)                                 echo "[install] 设备现存 md5=${DEV_MD5}（未知版本）" ;;
esac

adb -s "$SERIAL" shell am force-stop "$PKG" || true

echo "[install] push $APK → $REMOTE"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
# EXIT trap 必须显式 exit $rc：否则 trap 内末条命令成功会吞掉脚本真实退出码。
trap 'rc=$?; adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true; exit $rc' EXIT

echo "[install] 同签名覆盖安装（保留数据）"
if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE"; then
  if [[ "${BLFX_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    cat >&2 <<'EOM'
ERROR: 覆盖安装失败（签名冲突）——设备上现装的包与本星光包签名不同，无法覆盖。

本脚本不会自动卸载：卸载会清掉游戏本地数据/存档，属破坏性操作，需要用户明确授权。

设备上可能现装的是：
  · 官方原版  com.muyegame.blfx 1.01.06（签名 CN=binge2.keystore 之外的原厂签名）
  · 斌哥改包  com.muyegame.blfx 1.01.06（签名 CN=binge2.keystore）← 本轮拉取时的状态

用户确认「可以卸载设备上现装的那一版、接受本地数据/存档丢失」后重跑：
    BLFX_ALLOW_UNINSTALL=1 bash 04_scripts/爆裂防线/blfx_install.sh

回退路径（不可再生输入，勿删）：
  官方原包  02_apks/爆裂防线/爆裂防线_1.01.06_original_from_device.apk
  斌哥改包  02_apks/爆裂防线/爆裂防线_1.01.06_binge_from_device.apk
EOM
    exit 1
  fi
  echo "[install] 已授权卸载：pm uninstall --user 0 ${PKG}（本地数据将被清除）"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  adb -s "$SERIAL" shell pm install --user 0 "$REMOTE"
fi

echo "[install] 闭环校验：设备 base.apk == 本地产物"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

printf '%s %s\n' "$APK_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"

echo "[install] done: ${PKG}（首次启动确认：悬浮球菜单可拉起 + 三项功能生效）"
