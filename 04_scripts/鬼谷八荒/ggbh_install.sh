#!/usr/bin/env bash
# ================================================================
#  鬼谷八荒 1.1.329 (com.guigugame.guigubahuang) — 管线 install 后端
#  引擎：Unity IL2CPP（单机；菜单 = GM 菜单 + 全局加速两项）
#
#  签名语义：本包用 01_tools/starry_release.jks 签名（CN=StarryMagic），
#  官方包签名 O=guigugame ⇒「同签名覆盖安装」失败即 INSTALL_FAILED_UPDATE_INCOMPATIBLE，
#  需要先卸载设备上那一版。
#
#  授权语义（本项目政策：不默认卸载、不默认清数据）：
#    默认      = 只做同签名覆盖安装（pm install -r -d）；失败即报告并 exit 1。
#    GGBH_ALLOW_UNINSTALL=1
#                = 用户已明确授权「卸载设备上现装的那一版（丢本地数据/存档）+ 全新安装」。
#                  官方底包可再从 TapTap 重下或从设备只读拉取（见 底包再获取台账.md）。
#
#  底包期望（官方 TapTap 原版 1.1.329）：md5 f6cc1a4eb063bc0505cb93b68020d714 / 1,678,372,111 B
#    默认路径 $NIXIANG/02_apks/鬼谷八荒/鬼谷八荒_1.1.329_taptap_official_from_device.apk
#    （该默认路径当前不存在；本机实测底包在
#      /tmp/ggbh_stock/ggbh_1.1.329_official_from_device.apk —— 只在跑 surgical 时用
#      GGBH_BASE_APK=/tmp/ggbh_stock/ggbh_1.1.329_official_from_device.apk 指过去，本脚本不读它）
#  产物路径：signed 08_output/鬼谷八荒/鬼谷八荒_1.1.329_surgical-releaseSigned.apk
#          留证    07_patched/鬼谷八荒/surgical/{AndroidManifest.xml,classes2.dex}
#
#  装机后闭环：verify_installed_apk.sh 比对设备 base.apk 与本地产物 md5。
#  悬浮窗：MenuService 需要 SYSTEM_ALERT_WINDOW（appops allow，装机后自动执行）。
#
#  用法:
#    bash 04_scripts/鬼谷八荒/ggbh_install.sh                          # 覆盖（签名冲突会失败并提示授权）
#    GGBH_ALLOW_UNINSTALL=1 bash 04_scripts/鬼谷八荒/ggbh_install.sh   # 卸载重装
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="鬼谷八荒"
VERSION="1.1.329"
PKG="com.guigugame.guigubahuang"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/ggbh_surgical.apk"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing ${SERIAL_FILE}（先跑 04_scripts/鬼谷八荒/device_ready_ggbh.sh）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing ${APK}（先跑 surgical + sign）" >&2; exit 1; }

rm -f "$INSTALL_OK"

APK_MD5="$(md5 -q "$APK")"
APK_SIZE="$(stat -f%z "$APK")"
echo "[install] local apk md5=$APK_MD5 size=$APK_SIZE"
adb -s "$SERIAL" get-state >/dev/null

# 设备可用空间预检（本包约 1.6 GB，安装峰值约 4 GB；Unity 大资源包装机峰值远高于包体）
AVAIL_KB="$(adb -s "$SERIAL" shell "df -k /data | tail -1" | tr -d '\r' | awk '{print $4}')"
if [[ -n "$AVAIL_KB" ]] && [[ "$AVAIL_KB" =~ ^[0-9]+$ ]] && [[ "$AVAIL_KB" -lt 5000000 ]]; then
  echo "ERROR: /data 可用空间仅 $((AVAIL_KB/1024)) MB，本包安装需 ≥5 GB" >&2
  exit 1
fi

DEV_PATH="$(adb -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
DEV_MD5=""
if [[ -n "$DEV_PATH" ]]; then
  DEV_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
fi
case "$DEV_MD5" in
  f6cc1a4eb063bc0505cb93b68020d714) echo "[install] 设备现存 = 官方 TapTap 原版 1.1.329（需授权卸载才能换成星光包）" ;;
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
  if [[ "${GGBH_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    cat >&2 <<'EOM'
ERROR: 覆盖安装失败（签名冲突）——设备上现装的包与本星光包签名不同，无法覆盖。

本脚本不会自动卸载：卸载会清掉游戏本地数据/存档，属破坏性操作，需要用户明确授权。

设备上现装的是：
  · 官方原版  com.guigugame.guigubahuang 1.1.329（签名 O=guigugame，TapTap 渠道）
    md5 f6cc1a4eb063bc0505cb93b68020d714 / 1,678,372,111 B

用户确认「可以卸载设备上现装的那一版、接受本地数据/存档丢失」后重跑：
    GGBH_ALLOW_UNINSTALL=1 bash 04_scripts/鬼谷八荒/ggbh_install.sh

回退路径（官方 1.1.329 底包）：
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
[install] done: com.guigugame.guigubahuang

首次启动确认清单：
  1. 悬浮球菜单可拉起，菜单项 = 鬼谷功能[GM 菜单(Toggle) / 加速倍率(SeekBar 1..20，1 = 关)]
  2. 框架日志：adb logcat -s Mod_Menu:V StarryMagic:V StarrySignSpoof:V
     - `MOD 菜单已加载 (MainModern tag=Ggbh)` / `MOD 已就绪 tag=Ggbh ready=1`（MenuService overlayVisible=true）
     - `[Ggbh] lib ready (FileOffToVa 双路径, lazy hooks, N slots)` / `auth game_id=ggbh`
     - `StarrySignSpoof spoof hit pkg=com.guigugame.guigubahuang md5=ba61fbe7f5fbb708a43cc4527f2a2a51`
       ⇐ 回灌官方证书（O=guigugame / serial 183982C3 / 有效期 2024-08-14→2074-08-02），
         否则 TapSDK 与渠道侧签名校验报 signature not match
  3. 功能取证日志（进游戏、勾上开关/拖动滑块后回看）：
     - 基址判据：`[Ggbh] SELFCHECK base=0x… rd=… hook=… interp=… path=… maps=… page=… verity=…`
     - GM 菜单：`[Ggbh] PROBE gmCheat hit=<N> on=1`（读侧恒真；hit 稳定增长 = 命中挂点）
     - 全局加速：`[Ggbh] PROBE deltaTime hit=<N> mult=<倍率> pin=<0|1>`
                 / `[Ggbh] PROBE setTimeScale hit=<N> value=<v>`
     - 汇总：`[Ggbh] HITS gm=… deltaTime=… setTimeScale=… (gm=… mult=…)`
  4. 登录：本作 TapSDK 走旧版路径 ⇒ 星光 dex（classes2.dex）内 TapLoginForceWeb 白名单 +
     YnttLoginRequest 生效，登录应出现网页登录而非跳 TapTap 客户端。
  5. 功能边界：本作是单机（用户口径：CTF 题目逆向单机游戏），两项功能本地生效即为最终效果；
     功能集只有用户点名的两项（10_docs/鬼谷八荒/硬约束.md #1），禁止加第三项。
  6. 回退路径：官方 1.1.329（md5 f6cc1a4eb063bc0505cb93b68020d714 / 1,678,372,111 B）
     —— TapTap 渠道重下；或设备 /data/app 只读拉取。
EOM
