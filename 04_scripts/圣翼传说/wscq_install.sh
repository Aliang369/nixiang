#!/usr/bin/env bash
# ================================================================
#  圣翼传说 7.6.06+ (com.aiyue.wscq) — 管线 install 后端
#
#  签名语义：官方包为 O=tianyu，星光包为 CN=StarryMagic。设备已切成星光签名后，
#  后续构建只做同包名、同星光签名的覆盖安装，保留账号与本机数据。
#  若设备仍是官方签名，覆盖会报 INSTALL_FAILED_UPDATE_INCOMPATIBLE；此时默认停止。
#
#  授权语义（本项目政策：不默认卸载、不默认清数据）：
#    默认      = 只做同签名覆盖安装（pm install -r -d）；失败即报告并 exit 1。
#    WSCQ_ALLOW_UNINSTALL=1
#                = 用户已明确授权「卸载官方包（丢本地数据/存档）+ 全新安装」。
#                  当前回退底包：09_test/圣翼传说/base_20261026/base.apk
#
#  装机后闭环：verify_installed_apk.sh 比对设备 base.apk 与本地产物 md5。
#
#  用法:
#    WSCQ_VERSION=7.6.06 bash 04_scripts/圣翼传说/wscq_install.sh
#    WSCQ_VERSION=7.6.06 WSCQ_ALLOW_UNINSTALL=1 bash 04_scripts/圣翼传说/wscq_install.sh
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="圣翼传说"
VERSION="${WSCQ_VERSION:-7.5.10}"
PKG="com.aiyue.wscq"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="${WSCQ_SIGNED:-$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk}"
REMOTE="/data/local/tmp/wscq_${VERSION}_surgical.apk"
INSTALL_OK="${WSCQ_INSTALL_OK:-$NIXIANG/04_scripts/$GAME/.install_ok}"
OFFICIAL_BASE="${WSCQ_BASE_APK:-$NIXIANG/09_test/$GAME/base_20261026/base.apk}"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing ${SERIAL_FILE}（先跑 device_ready_wscq.sh）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing ${APK}（先跑 surgical + sign）" >&2; exit 1; }

if [[ "${WSCQ_ALLOW_UNINSTALL:-0}" == "1" ]]; then
  INSTALL_GUARD_ALLOW_UNINSTALL=1 bash "$NIXIANG/04_scripts/_common/install_guard.sh" \
    "$GAME" --allow-uninstall --cmd "adb -s $SERIAL shell pm uninstall --user 0 $PKG; adb -s $SERIAL shell pm install --user 0 $REMOTE" || exit $?
else
  bash "$NIXIANG/04_scripts/_common/install_guard.sh" \
    "$GAME" --cmd "adb -s $SERIAL shell pm install -r -d --user 0 $REMOTE" || exit $?
fi

rm -f "$INSTALL_OK"

APK_MD5="$(md5 -q "$APK")"
echo "[install] local apk md5=$APK_MD5"
adb -s "$SERIAL" get-state >/dev/null
adb -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1 \
  || { echo "ERROR: 设备上未安装 ${PKG}（先装官方原版或直接 --fresh）" >&2; exit 1; }

adb -s "$SERIAL" shell am force-stop "$PKG" || true

echo "[install] push $APK → $REMOTE"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
# EXIT trap 必须显式 exit $rc：否则 trap 内末条命令成功会吞掉脚本真实退出码。
trap 'rc=$?; adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true; exit $rc' EXIT

echo "[install] 同签名覆盖安装（保留数据）"
if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE"; then
  if [[ "${WSCQ_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    cat >&2 <<EOM
ERROR: 覆盖安装失败（设备上是官方签名 O=tianyu 的包，与本星光包签名冲突）。

本脚本不会自动卸载：卸载会清掉游戏本地数据，属破坏性操作，需要用户明确授权。
用户确认「可以卸载官方包、接受本地数据丢失」后重跑：
    WSCQ_ALLOW_UNINSTALL=1 bash 04_scripts/圣翼传说/wscq_install.sh
回退路径：官方原包仍在 ${OFFICIAL_BASE}
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

echo "[install] done: ${PKG}（首次启动确认：TapTap 走包内网页登录 + 星光悬浮球菜单）"
