#!/usr/bin/env bash
# ================================================================
#  暗影格斗3 1.45.3 (com.wanda.sf3.wd) — 管线 install 后端
#
#  授权语义（本项目政策：不默认卸载、不默认清数据）：
#    默认      = 只做"同签名覆盖安装"（pm install -r -d）。
#                设备上若仍是官方签名的包，会 INSTALL_FAILED_UPDATE_INCOMPATIBLE，
#                本脚本报告后 exit 1，不做任何卸载/清数据动作。
#    SF3_ALLOW_UNINSTALL=1
#                = 用户已明确授权"卸载官方包（丢存档/数据）+ 全新安装"时使用。
#
#  装机后闭环：verify_installed_apk.sh 比对设备 base.apk 与本地产物 md5。
#
#  用法:
#    bash 04_scripts/暗影格斗3/sf3_install.sh                      # 覆盖
#    SF3_ALLOW_UNINSTALL=1 bash 04_scripts/暗影格斗3/sf3_install.sh # 卸载重装
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="暗影格斗3"
VERSION="1.45.3"
PKG="com.wanda.sf3.wd"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/sf3_surgical.apk"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing ${APK}（先跑 surgical + sign）" >&2; exit 1; }

# 本轮装机成功戳：先清旧戳，只有闭环校验通过才重新写入。
# 管线 install 阶段 verify 依赖它，防"脚本崩了但退出码 0"被误标 done。
rm -f "$INSTALL_OK"

APK_MD5="$(md5 -q "$APK")"
echo "[install] local apk md5=$APK_MD5"
adb -s "$SERIAL" get-state >/dev/null
adb -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1 \
  || { echo "ERROR: 设备上未安装 ${PKG}（原包已卸载？先确认状态再装）" >&2; exit 1; }

adb -s "$SERIAL" shell am force-stop "$PKG" || true

echo "[install] push $APK → $REMOTE"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
# EXIT trap 必须显式 exit $rc：否则 trap 内末条命令成功会吞掉脚本真实退出码，
# 管线会把失败的装机误标 done（2026-09-19 暗影格斗3 实测踩坑）。
trap 'rc=$?; adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true; exit $rc' EXIT

echo "[install] 同签名覆盖安装（保留数据）"
if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE"; then
  if [[ "${SF3_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    cat >&2 <<'EOM'
ERROR: 覆盖安装失败（设备上多半还是官方签名的包）。

本脚本不会自动卸载：卸载会清掉游戏数据/存档，属破坏性操作，需要用户明确授权。
用户确认「可以卸载官方包、接受数据丢失」后，用下面任一条重跑：
    SF3_ALLOW_UNINSTALL=1 bash 04_scripts/暗影格斗3/sf3_install.sh
回退路径：官方原包仍在 02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk
EOM
    exit 1
  fi
  echo "[install] 已授权卸载：pm uninstall --user 0 ${PKG}（数据将被清除）"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  adb -s "$SERIAL" shell pm install --user 0 "$REMOTE"
fi

echo "[install] 闭环校验：设备 base.apk == 本地产物"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

printf '%s %s\n' "$APK_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"

echo "[install] done: ${PKG}（首次启动请确认星光悬浮球与菜单）"
