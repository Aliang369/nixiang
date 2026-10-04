#!/usr/bin/env bash
# ================================================================
#  仙逆：本尊 0.0.14 (com.qqsy.xnbz) — 管线 install 后端
#
#  签名语义：本包用 01_tools/starry_release.jks 签名（CN=StarryMagic），
#  官方包签名 CN=qqian ⇒「同签名覆盖安装」失败即 INSTALL_FAILED_UPDATE_INCOMPATIBLE，
#  需要先卸载设备上那一版。
#
#  授权语义（本项目政策：不默认卸载、不默认清数据）：
#    默认      = 只做同签名覆盖安装（pm install -r -d）；失败即报告并 exit 1。
#    XNBZ_ALLOW_UNINSTALL=1
#                = 用户已明确授权「卸载设备上现装的那一版（丢本地数据/存档）+ 全新安装」。
#                  官方底包可再从 TapTap 重下或从设备只读拉取（见 底包再获取台账.md）。
#
#  装机后闭环：verify_installed_apk.sh 比对设备 base.apk 与本地产物 md5。
#  悬浮窗：MenuService 需要 SYSTEM_ALERT_WINDOW（appops allow，装机后自动执行）。
#
#  用法:
#    bash 04_scripts/仙逆/xnbz_install.sh                          # 覆盖（签名冲突会失败并提示授权）
#    XNBZ_ALLOW_UNINSTALL=1 bash 04_scripts/仙逆/xnbz_install.sh   # 卸载重装
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="仙逆"
VERSION="0.0.14"
PKG="com.qqsy.xnbz"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/xnbz_surgical.apk"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing ${SERIAL_FILE}（先跑 device_ready_xnbz.sh）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing ${APK}（先跑 surgical + sign）" >&2; exit 1; }

rm -f "$INSTALL_OK"

APK_MD5="$(md5 -q "$APK")"
APK_SIZE="$(stat -f%z "$APK")"
echo "[install] local apk md5=$APK_MD5 size=$APK_SIZE"
adb -s "$SERIAL" get-state >/dev/null

# 设备可用空间预检（本包约 375 MB，安装峰值约 1.2 GB）
AVAIL_KB="$(adb -s "$SERIAL" shell "df -k /data | tail -1" | tr -d '\r' | awk '{print $4}')"
if [[ -n "$AVAIL_KB" ]] && [[ "$AVAIL_KB" =~ ^[0-9]+$ ]] && [[ "$AVAIL_KB" -lt 2000000 ]]; then
  echo "ERROR: /data 可用空间仅 $((AVAIL_KB/1024)) MB，本包安装需 ≥2 GB" >&2
  exit 1
fi

DEV_PATH="$(adb -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
DEV_MD5=""
if [[ -n "$DEV_PATH" ]]; then
  DEV_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
fi
case "$DEV_MD5" in
  756df4d27ded491e00a865687571fc53) echo "[install] 设备现存 = 官方 TapTap 原版 0.0.14（需授权卸载才能换成星光包）" ;;
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
  if [[ "${XNBZ_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    cat >&2 <<'EOM'
ERROR: 覆盖安装失败（签名冲突）——设备上现装的包与本星光包签名不同，无法覆盖。

本脚本不会自动卸载：卸载会清掉游戏本地数据/存档，属破坏性操作，需要用户明确授权。

设备上现装的是：
  · 官方原版  com.qqsy.xnbz 0.0.14（签名 CN=qqian，TapTap 渠道）
    md5 756df4d27ded491e00a865687571fc53 / 375,615,779 B

用户确认「可以卸载设备上现装的那一版、接受本地数据/存档丢失」后重跑：
    XNBZ_ALLOW_UNINSTALL=1 bash 04_scripts/仙逆/xnbz_install.sh

回退路径（官方 0.0.14 底包）：
  · TapTap 渠道重下；或设备 /data/app 只读拉取
EOM
    exit 1
  fi
  echo "[install] 已授权卸载：pm uninstall --user 0 ${PKG}（本地数据将被清除）"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  adb -s "$SERIAL" shell pm install --user 0 "$REMOTE"
fi

echo "[install] 悬浮窗权限（MenuService 必需）：appops SYSTEM_ALERT_WINDOW allow"
# Android 14/16 上 shell(uid 2000) 无 MANAGE_APP_OPS_MODES ⇒ 直接 appops 会 SecurityException；
# 本机已 root（KernelSU），必须走 su。
if adb -s "$SERIAL" shell appops get "$PKG" SYSTEM_ALERT_WINDOW 2>/dev/null | grep -q allow; then
  echo "[install]   已 allow（跳过）"
elif adb -s "$SERIAL" shell su -c "appops set $PKG SYSTEM_ALERT_WINDOW allow" >/dev/null 2>&1; then
  echo "[install]   su -c appops set → $(adb -s "$SERIAL" shell appops get "$PKG" SYSTEM_ALERT_WINDOW 2>/dev/null | tr -d '\r')"
elif adb -s "$SERIAL" shell appops set "$PKG" SYSTEM_ALERT_WINDOW allow >/dev/null 2>&1; then
  echo "[install]   非 root 直设成功"
else
  echo "[install] ⚠️ 自动授权失败：请手动执行（否则悬浮菜单不显示）"
  echo "            adb -s $SERIAL shell su -c 'appops set $PKG SYSTEM_ALERT_WINDOW allow'"
fi

echo "[install] 闭环校验：设备 base.apk == 本地产物"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

printf '%s %s\n' "$APK_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"

cat <<'EOM'
[install] done: com.qqsy.xnbz

首次启动确认清单：
  1. 悬浮球菜单可拉起，菜单项 = 战斗[无敌 / 伤害倍率滑块 / 重试判胜]
  2. 实时日志：adb logcat -s Mod_Menu:V StarryMagic:V StarryTrace:V StarrySignSpoof:V
     - `[Xnbz] lib ready (cocos jsb, js-bridge armed, patch armed)` / `auth game_id=xnbz`
     - `StarryTrace: xnbz-js patch v4 installed (hp-ledger + inv + dmg-mul + force-win + retry-win)` ⇐ JS 补丁已挂上
  3. 战斗内取证日志：
     - 无敌：`xnbz-js inv blocked dmg=`（我方扣血被吞）
     - 倍率：`xnbz-js replay try#N win=? actions=?` / `xnbz-js enemy hp -<差值> x<倍率> -> <本地血量>`
     - 结算：`xnbz-js force win #N (server result.isWin -> true)`
     - 重试判胜：`xnbz-js server judged LOSS, auto-retry #N/3` / `xnbz-js server judged WIN after retry #N -> 奖励应由服务端正常下发` / `retry cap hit (3)`
  4. TapTap 登录：点登录应出现 WebView 网页登录页，而不是跳 TapTap 客户端
  5. 功能边界（服务端权威）：
     - 无敌 / 伤害倍率 = 改**客户端回放**（我方不掉血、敌方按倍率掉血、本地判胜 + 胜利分支），改不了服务端账目
     - 奖励（changes.items）由服务端在 `game.fightbegin` 时定稿的那份数据包决定 ⇒ 客户端改 `isWin` 只能改显示
     - 想拿真奖励 = 只能让服务端自己判胜 ⇒【重试判胜】重开本场战斗，让服务端重算（消耗体力等资源，留意）
EOM
