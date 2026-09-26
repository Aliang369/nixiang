#!/usr/bin/env bash
# ================================================================
#  光阴之外 1.0.3 (com.shbq.gyzw.ppyx) — 管线 install 后端
#
#  签名语义：本包用 01_tools/starry_release.jks 签名，与官方签名（CN=binqi）
#  以及设备上现装的斌哥包（CN=binge2.keystore）都不同。
#  故「同签名覆盖安装」必然是 INSTALL_FAILED_UPDATE_INCOMPATIBLE
#  —— 需要先卸载设备上那一版。
#
#  授权语义（本项目政策：不默认卸载、不默认清数据）：
#    默认      = 只做同签名覆盖安装（pm install -r -d）；失败即报告并 exit 1。
#    GYZW_ALLOW_UNINSTALL=1
#                = 用户已明确授权「卸载设备上现装的那一版（丢本地数据/存档）+ 全新安装」。
#                  回退包：02_apks/光阴之外/光阴之外_1.0.3_taptap_original.apk（官方原版）
#                          02_apks/光阴之外/光阴之外_1.0.3_binge2.apk（斌哥改包，对照物）
#
#  装机后闭环：verify_installed_apk.sh 比对设备 base.apk 与本地产物 md5。
#
#  用法:
#    bash 04_scripts/光阴之外/gyzw_install.sh                          # 覆盖（签名冲突会失败并提示授权）
#    GYZW_ALLOW_UNINSTALL=1 bash 04_scripts/光阴之外/gyzw_install.sh   # 卸载重装
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="光阴之外"
VERSION="1.0.3"
PKG="com.shbq.gyzw.ppyx"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/gyzw_surgical.apk"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing ${SERIAL_FILE}（先跑 device_ready_gyzw.sh）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing ${APK}（先跑 surgical + sign）" >&2; exit 1; }

rm -f "$INSTALL_OK"

APK_MD5="$(md5 -q "$APK")"
APK_SIZE="$(stat -f%z "$APK")"
echo "[install] local apk md5=$APK_MD5 size=$APK_SIZE"
adb -s "$SERIAL" get-state >/dev/null

# 设备可用空间预检（本包 560 MB，安装峰值约 1.2 GB）
AVAIL_KB="$(adb -s "$SERIAL" shell "df -k /data | tail -1" | tr -d '\r' | awk '{print $4}')"
if [[ -n "$AVAIL_KB" ]] && [[ "$AVAIL_KB" =~ ^[0-9]+$ ]] && [[ "$AVAIL_KB" -lt 2500000 ]]; then
  echo "ERROR: /data 可用空间仅 $((AVAIL_KB/1024)) MB，本包安装需 ≥2.5 GB" >&2
  exit 1
fi

DEV_PATH="$(adb -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
DEV_MD5=""
if [[ -n "$DEV_PATH" ]]; then
  DEV_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
fi
case "$DEV_MD5" in
  ebab193edc592b94e5c1e109ca93e36d) echo "[install] 设备现存 = 斌哥改包 1.0.3" ;;
  363ac0e6926b3b7a1ea71f804500d1fd) echo "[install] 设备现存 = 官方 TapTap 原版 1.0.3" ;;
  "$APK_MD5")                        echo "[install] 设备现存 = 本星光包（同包重装，不会丢数据）" ;;
  "")                                echo "[install] 设备未安装 ${PKG}（将全新安装）" ;;
  *)                                 echo "[install] 设备现存 md5=${DEV_MD5}（未知版本）" ;;
esac

adb -s "$SERIAL" shell am force-stop "$PKG" || true

echo "[install] push $APK → $REMOTE"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
# EXIT trap 必须显式 exit ${rc}：否则 trap 内末条命令成功会吞掉脚本真实退出码。
trap 'rc=$?; adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true; exit $rc' EXIT

echo "[install] 同签名覆盖安装（保留数据）"
if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE"; then
  if [[ "${GYZW_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    cat >&2 <<'EOM'
ERROR: 覆盖安装失败（签名冲突）——设备上现装的包与本星光包签名不同，无法覆盖。

本脚本不会自动卸载：卸载会清掉游戏本地数据/存档，属破坏性操作，需要用户明确授权。

设备上可能现装的是：
  · 斌哥改包  com.shbq.gyzw.ppyx 1.0.3（签名 CN=binge2.keystore）← 本轮拉取时的状态
  · 官方原版  com.shbq.gyzw.ppyx 1.0.3（签名 CN=binqi，TapTap 渠道）

用户确认「可以卸载设备上现装的那一版、接受本地数据/存档丢失」后重跑：
    GYZW_ALLOW_UNINSTALL=1 bash 04_scripts/光阴之外/gyzw_install.sh

回退路径（不可再生输入，勿删）：
  斌哥改包  02_apks/光阴之外/光阴之外_1.0.3_binge2.apk
  官方原版  02_apks/光阴之外/光阴之外_1.0.3_taptap_original.apk
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

cat <<'EOM'
[install] done: com.shbq.gyzw.ppyx

首次启动确认清单：
  1. 悬浮球菜单可拉起，六项 = 战斗[百倍伤害/超级无敌/战斗加速滑块1-10] + 广告[免广告]
  2. 实时日志：adb logcat -s Gyzw StarryMagic MenuService
  3. 三项功能对应斌哥改包基线；免广告为本次新增（斌哥包无此项）
EOM
