#!/usr/bin/env bash
# ================================================================
#  幻想武将模拟器 1.0.8 (com.qxjc.hxwjmlq) — 分体安装脚本
#  分体安装（base + split）
#
#  签名语义：本包用 01_tools/starry_release.jks 签名（CN=StarryMagic），
#  与官方签名（CN=sy，SHA-1 8a31d12c…）不同。
#  故「同签名覆盖安装」对官方原版必然是 INSTALL_FAILED_UPDATE_INCOMPATIBLE
#  —— 需要先卸载设备上那一版。
#
#  授权语义（本项目政策：不默认卸载、不默认清数据）：
#    默认      = 只做同签名覆盖安装（pm install -r -d）；失败即报告并 exit 1。
#    HXWJ_ALLOW_UNINSTALL=1
#                = 用户已明确授权「卸载设备上现装的那一版（丢本地数据/存档）+ 全新安装」。
#                  回退包：02_apks/幻想武将模拟器/幻想武将模拟器_1.0.8_taptap_original.apk
#
#  装机后闭环：分别比对设备 base.apk / split_starry.apk 与本地产物 md5。
#
#  用法:
#    bash 04_scripts/幻想武将模拟器/hxwj_install.sh                          # 覆盖（签名冲突会失败并提示授权）
#    HXWJ_ALLOW_UNINSTALL=1 bash 04_scripts/幻想武将模拟器/hxwj_install.sh   # 卸载重装
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="幻想武将模拟器"
VERSION="1.0.8"
PKG="com.qxjc.hxwjmlq"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
BASE_APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_base-resign.apk"
SPLIT_APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_split-starry.apk"
REMOTE_BASE="/data/local/tmp/hxwj_base_resign.apk"
REMOTE_SPLIT="/data/local/tmp/hxwj_split_starry.apk"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"
BT="${BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing ${SERIAL_FILE}（先跑 device_ready_hxwj.sh）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$BASE_APK" ]] || { echo "ERROR: missing ${BASE_APK}（先跑 hxwj_build_split.sh）" >&2; exit 1; }
[[ -f "$SPLIT_APK" ]] || { echo "ERROR: missing ${SPLIT_APK}（先跑 hxwj_build_split.sh）" >&2; exit 1; }
[[ -x "$BT/apksigner" ]] || { echo "ERROR: missing $BT/apksigner" >&2; exit 1; }

rm -f "$INSTALL_OK"

BASE_MD5="$(md5 -q "$BASE_APK")"
SPLIT_MD5="$(md5 -q "$SPLIT_APK")"
BASE_SIZE="$(stat -f%z "$BASE_APK")"
SPLIT_SIZE="$(stat -f%z "$SPLIT_APK")"
echo "[install] base  md5=$BASE_MD5 size=$BASE_SIZE"
echo "[install] split md5=$SPLIT_MD5 size=$SPLIT_SIZE"
"$BT/apksigner" verify "$BASE_APK" >/dev/null
"$BT/apksigner" verify "$SPLIT_APK" >/dev/null
unzip -p "$SPLIT_APK" classes.dex | LC_ALL=C grep -aF 'FOREGROUND_SERVICE not granted' >/dev/null \
  || { echo "ERROR: split dex 不是本轮 MenuService 降级版" >&2; exit 1; }
unzip -p "$SPLIT_APK" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -aF '[hxwj]' >/dev/null \
  || { echo "ERROR: split so 缺 [hxwj]" >&2; exit 1; }
echo "[install] 签名 / dex / so 自检 ok"
adb -s "$SERIAL" get-state >/dev/null

# 设备可用空间预检（本包 168 MB，安装峰值约 500 MB）
AVAIL_KB="$(adb -s "$SERIAL" shell "df -k /data | tail -1" | tr -d '\r' | awk '{print $4}')"
if [[ -n "$AVAIL_KB" ]] && [[ "$AVAIL_KB" =~ ^[0-9]+$ ]] && [[ "$AVAIL_KB" -lt 800000 ]]; then
  echo "ERROR: /data 可用空间仅 $((AVAIL_KB/1024)) MB，本包安装需 ≥800 MB" >&2
  exit 1
fi

DEV_BASE_PATH="$(adb -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' | sed -n 's/^package://p' | grep '/base.apk$' | head -1 || true)"
DEV_SPLIT_PATH="$(adb -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' | sed -n 's/^package://p' | grep '/split_starry.apk$' | head -1 || true)"
DEV_BASE_MD5=""
DEV_SPLIT_MD5=""
if [[ -n "$DEV_BASE_PATH" ]]; then
  DEV_BASE_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_BASE_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
fi
if [[ -n "$DEV_SPLIT_PATH" ]]; then
  DEV_SPLIT_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_SPLIT_PATH'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
fi
echo "[install] 设备现存 base=${DEV_BASE_MD5:-none} split=${DEV_SPLIT_MD5:-none}"
case "$DEV_BASE_MD5" in
  68ed2632579329c3700559447aba77f0) echo "[install] 设备 base = 官方 TapTap 原版（CN=sy，覆盖必冲突）" ;;
  "$BASE_MD5")                        echo "[install] 设备 base = 本星光 base-resign" ;;
  "")                                 echo "[install] 设备未安装 ${PKG}（将全新安装）" ;;
  *)                                  echo "[install] 设备 base = 未知版本" ;;
esac

adb -s "$SERIAL" shell am force-stop "$PKG" || true

echo "[install] push base + split"
adb -s "$SERIAL" push "$BASE_APK" "$REMOTE_BASE" >/dev/null
adb -s "$SERIAL" push "$SPLIT_APK" "$REMOTE_SPLIT" >/dev/null
# EXIT trap 必须显式 exit ${rc}：否则 trap 内末条命令成功会吞掉脚本真实退出码。
trap 'rc=$?; adb -s "$SERIAL" shell rm -f "$REMOTE_BASE" "$REMOTE_SPLIT" >/dev/null 2>&1 || true; exit $rc' EXIT

install_session() {
  local create_out session_id
  if ! create_out="$(adb -s "$SERIAL" shell pm install-create "$@" -p "$PKG" 2>&1)"; then
    printf '%s\n' "$create_out" >&2
    return 1
  fi
  session_id="$(printf '%s\n' "$create_out" | tr -d '\r' | sed -n 's/.*\[\([0-9][0-9]*\)\].*/\1/p' | tail -1)"
  [[ -n "$session_id" ]] || { printf '%s\n' "$create_out" >&2; return 1; }

  adb -s "$SERIAL" shell pm install-write -S "$BASE_SIZE" "$session_id" base "$REMOTE_BASE" >/dev/null
  adb -s "$SERIAL" shell pm install-write -S "$SPLIT_SIZE" "$session_id" split_starry "$REMOTE_SPLIT" >/dev/null
  adb -s "$SERIAL" shell pm install-commit "$session_id"
}

echo "[install] 分体覆盖安装（保留数据）"
if ! INSTALL_OUT="$(install_session -r 2>&1)"; then
  printf '%s\n' "$INSTALL_OUT" >&2
  if [[ "${HXWJ_ALLOW_UNINSTALL:-0}" != "1" ]]; then
    cat >&2 <<'EOM'
ERROR: 分体覆盖安装失败（通常是签名冲突）。本脚本不会自动卸载。

用户确认「可以卸载设备上现装的那一版、接受本地数据/存档丢失」后重跑：
    HXWJ_ALLOW_UNINSTALL=1 bash 04_scripts/幻想武将模拟器/hxwj_install.sh

回退包（不可再生输入，勿删）：
  官方原版  02_apks/幻想武将模拟器/幻想武将模拟器_1.0.8_taptap_original.apk
EOM
    exit 1
  fi
  echo "[install] 已授权卸载：pm uninstall --user 0 ${PKG}（本地数据将被清除）"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  INSTALL_OUT="$(install_session 2>&1)"
fi
printf '%s\n' "$INSTALL_OUT"

echo "[install] 闭环校验：设备 base/split == 本地产物"
adb -s "$SERIAL" shell "pm path $PKG" | tr -d '\r' > /tmp/hxwj_install_paths.txt
grep -q '/base.apk$' /tmp/hxwj_install_paths.txt || { echo "ERROR: 设备缺 base.apk" >&2; exit 1; }
grep -q '/split_starry.apk$' /tmp/hxwj_install_paths.txt || { echo "ERROR: 设备缺 split_starry.apk" >&2; exit 1; }
DEV_BASE_PATH="$(sed -n 's/^package://p' /tmp/hxwj_install_paths.txt | grep '/base.apk$' | head -1)"
DEV_SPLIT_PATH="$(sed -n 's/^package://p' /tmp/hxwj_install_paths.txt | grep '/split_starry.apk$' | head -1)"
DEV_BASE_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_BASE_PATH'" | tr -d '\r\n' | awk '{print $1}')"
DEV_SPLIT_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEV_SPLIT_PATH'" | tr -d '\r\n' | awk '{print $1}')"
[[ "$DEV_BASE_MD5" == "$BASE_MD5" ]] || { echo "ERROR: device base md5=$DEV_BASE_MD5 != $BASE_MD5" >&2; exit 1; }
[[ "$DEV_SPLIT_MD5" == "$SPLIT_MD5" ]] || { echo "ERROR: device split md5=$DEV_SPLIT_MD5 != $SPLIT_MD5" >&2; exit 1; }

printf '%s %s %s\n' "$BASE_MD5" "$SPLIT_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"

cat <<'EOM'
[install] done: com.qxjc.hxwjmlq (base + split_starry)

首次启动确认清单：
  1. 悬浮球菜单可拉起，三项 = 秒杀 / 百分百掉落 / 免广告
  2. 实时日志：adb logcat -s hxwj Mod_Menu StarryMagic MenuService
  3. 去签名（SignSpoof 回灌官方 CN=sy 证书）+ 强制网页登录（TapLoginForceWeb）随星光 dex 生效
EOM
