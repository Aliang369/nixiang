#!/usr/bin/env bash
# ================================================================
#  疯狂水世界 1.11.6 (com.gx.sw.qa.fkssj004.esj) — 管线 install 后端
#
#  签名语义：星光包用 01_tools/starry_release.jks（CN=StarryMagic）签名，
#  官方包签名 CN=changwan ⇒「同签名覆盖安装」必定失败
#    INSTALL_FAILED_UPDATE_INCOMPATIBLE / signature mismatch，
#  必须先卸载设备上那一版。
#
#  授权语义（本项目政策：不默认卸载、不默认清数据）：
#    默认      = 只做同签名覆盖安装（pm install -r -d）；失败即报告并 exit 1。
#    FKSJ_ALLOW_UNINSTALL=1
#                = 用户已明确授权「卸载设备上现装的那一版（丢本地数据/存档）+ 全新安装」。
#                  官方底包可再从 TapTap 重下，或从设备只读拉取（见 底包再获取台账.md）。
#
#  悬浮窗：MenuService 需要 SYSTEM_ALERT_WINDOW。
#  ★本机小米真机 **无 root**（硬约束 H6）⇒ appops 走三级回退，三级都不成才告警，
#    并给出用户手动授权指引，**不 exit 1**（剩下的是用户侧一次手动操作）。
#
#  装机后闭环：verify_installed_apk.sh 比对设备 base.apk 与本地产物 md5。
#  装机后预检：post_install_precheck.sh（机器关），过完才请用户上手。
#
#  用法:
#    bash 04_scripts/疯狂水世界/fkssj_install.sh                          # 覆盖（签名冲突会失败并提示授权）
#    FKSJ_ALLOW_UNINSTALL=1 bash 04_scripts/疯狂水世界/fkssj_install.sh   # 卸载重装
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="疯狂水世界"
VERSION="1.11.6"
PKG="com.gx.sw.qa.fkssj004.esj"
SCRIPT_DIR="$NIXIANG/04_scripts/$GAME"
SERIAL_FILE="$SCRIPT_DIR/.device_serial"
APK="${FKSJ_SIGNED:-$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk}"
REMOTE="/data/local/tmp/fkssj_surgical.apk"
INSTALL_OK="$SCRIPT_DIR/.install_ok"

# 设备上官方原版 1.11.6 的 base.apk 指纹（只读拉取所得，见 底包再获取台账.md）
OFFICIAL_MD5="f8ed1b954a3c5ce91dcecb4e721f3e9b"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing ${SERIAL_FILE}（先跑 device_ready_fkssj.sh）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing ${APK}（先跑 surgical + fkssj_sign.sh）" >&2; exit 1; }

rm -f "$INSTALL_OK"

APK_MD5="$(md5 -q "$APK")"
APK_SIZE="$(stat -f%z "$APK")"
echo "[install] local apk md5=$APK_MD5 size=$APK_SIZE"
adb -s "$SERIAL" get-state >/dev/null

# 设备可用空间预检（本包约 520 MB，安装峰值约 1.6 GB）
AVAIL_KB="$(adb -s "$SERIAL" shell "df -k /data | tail -1" | tr -d '\r' | awk '{print $4}')"
if [[ -n "$AVAIL_KB" ]] && [[ "$AVAIL_KB" =~ ^[0-9]+$ ]] && [[ "$AVAIL_KB" -lt 2600000 ]]; then
  echo "ERROR: /data 可用空间仅 $((AVAIL_KB/1024)) MB，本包安装需 ≥2.6 GB" >&2
  exit 1
fi

# 设备现存包身份判定（无 root 时 md5sum 读不到 /data/app，DEV_MD5 为空属正常）
DEV_PATH="$(adb -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
DEV_MD5=""
if [[ -n "$DEV_PATH" ]]; then
  DEV_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
  if [[ -z "$DEV_MD5" ]]; then
    DEV_MD5="$(adb -s "$SERIAL" shell "md5sum $DEV_PATH" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
  fi
fi
case "$DEV_MD5" in
  "$OFFICIAL_MD5") echo "[install] 设备现存 = 官方原版 1.11.6（需授权卸载才能换成星光包）" ;;
  "$APK_MD5")      echo "[install] 设备现存 = 本星光包（同包重装，不会丢数据）" ;;
  "")              echo "[install] 设备未安装 ${PKG}，或读不到 md5（无 root 时正常）" ;;
  *)               echo "[install] 设备现存 md5=${DEV_MD5}（未知版本）" ;;
esac

adb -s "$SERIAL" shell am force-stop "$PKG" || true

echo "[install] push $APK → $REMOTE"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
# EXIT trap 必须显式 exit ${rc}：否则 trap 内末条命令成功会吞掉脚本真实退出码。
trap 'rc=$?; adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true; exit $rc' EXIT

echo "[install] 同签名覆盖安装（保留数据）"
if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE"; then
  if [[ "${FKSJ_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    cat >&2 <<EOM
ERROR: 覆盖安装失败（签名冲突）——设备上现装的包与本星光包签名不同，无法覆盖。

本脚本不会自动卸载：卸载会清掉游戏本地数据/存档，属破坏性操作，需要用户明确授权。

设备上现装的是：
  · 官方原版  ${PKG} 1.11.6 / versionCode 480（签名 CN=changwan，TapTap 渠道）
    md5 ${OFFICIAL_MD5} / 519,961,058 B

用户确认「可以卸载设备上现装的那一版、接受本地数据/存档丢失」后重跑：
    FKSJ_ALLOW_UNINSTALL=1 bash 04_scripts/疯狂水世界/fkssj_install.sh

回退路径（官方 1.11.6 底包）：
  · TapTap 渠道重下；或设备 /data/app 只读拉取；或 09_test/${GAME}/base/base.apk
EOM
    exit 1
  fi
  echo "[install] 已授权卸载：pm uninstall --user 0 ${PKG}（本地数据将被清除）"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  adb -s "$SERIAL" shell pm install --user 0 "$REMOTE"
fi

echo "[install] 悬浮窗权限（MenuService 必需）：appops SYSTEM_ALERT_WINDOW allow"
echo "[install]   本机为小米真机、无 root ⇒ 三级回退：探测已 allow → su -c → 直设"
if adb -s "$SERIAL" shell appops get "$PKG" SYSTEM_ALERT_WINDOW 2>/dev/null | grep -q allow; then
  echo "[install]   ① 已 allow（跳过）"
elif adb -s "$SERIAL" shell su -c "appops set $PKG SYSTEM_ALERT_WINDOW allow" >/dev/null 2>&1 \
     && adb -s "$SERIAL" shell appops get "$PKG" SYSTEM_ALERT_WINDOW 2>/dev/null | grep -q allow; then
  echo "[install]   ② su -c appops set 成功"
elif adb -s "$SERIAL" shell appops set "$PKG" SYSTEM_ALERT_WINDOW allow >/dev/null 2>&1 \
     && adb -s "$SERIAL" shell appops get "$PKG" SYSTEM_ALERT_WINDOW 2>/dev/null | grep -q allow; then
  echo "[install]   ③ 非 root 直设成功（无先例，值得记录）"
else
  echo "[install]   ⚠️ 三级回退均未生效 —— 悬浮菜单不会显示，需要你在手机上手动授权一次："
  echo "              设置 → 应用设置 → 应用管理 → 疯狂水世界 → 权限管理 → 显示悬浮窗 → 允许"
  echo "            （或 adb -s $SERIAL shell appops set $PKG SYSTEM_ALERT_WINDOW allow，若你有 root）"
  echo "            这只是权限缺口，不是装机失败，继续往下走预检。"
fi

echo "[install] 闭环校验：设备 base.apk == 本地产物"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK"

printf '%s %s\n' "$APK_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"

cat <<EOM
[install] done: ${PKG}

下一步（不要跳过）：装机后机器预检
    bash 04_scripts/_common/post_install_precheck.sh --game ${GAME}

首次启动确认清单（本阶段＝过环境，功能项全为惰性、只写日志）：
  1. 悬浮球菜单可拉起，菜单项 = 战斗[无敌 / 伤害倍率 / 战斗加速]
  2. 实时日志：
       adb -s ${SERIAL} logcat -s NativeMenu:V MenuService:V ModBridge:V StarryMagic:V StarrySignSpoof:V
     - \`[Fkssj] lib ready (cocos jsb, zero-hook env build)\`  ⇐ native 侧已就绪（0 hook 是本阶段的设计，不是遗漏）
     - \`auth game_id synced from native: fkssj\`               ⇐ 注册表 game_id 已同步
     - \`StarrySignSpoof: installed pkgs=[...com.gx.sw.qa.fkssj004.esj...]\` ⇐ 原厂证书已回灌
  3. 点开关只应有日志、无副作用（阶段② 才做真功能，须等用户明确放行）
  4. TapTap 相关：本作底包内**没有** com.taptap.sdk.login.* / TapSignKit，
     登录由 cn.ewan.supersdk（易玩）+ com.cw.platform.core（创玩）承载 ⇒
     本阶段**不做**强制网页登录补丁，登录按官方渠道原样走。
  5. 渠道号：签名块自定义 ID-value 对 0x64707763="259425" 已由 fkssj_sign.sh 回注，
     复核命令：python3 04_scripts/疯狂水世界/fkssj_idblock.py check "$APK" --expect 259425
EOM
