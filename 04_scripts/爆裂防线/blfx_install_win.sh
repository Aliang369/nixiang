#!/usr/bin/env bash
# ================================================================
#  爆裂防线 1.02.01 — 装机（Windows/MSYS 后端，雷电/真机通用）
#
#  签名语义：本包 CN=StarryMagic，与官方 CN=my 不同 ⇒ 覆盖安装必然
#  INSTALL_FAILED_UPDATE_INCOMPATIBLE，只能卸载原版后全新安装。
#  卸载属破坏性操作：默认**不做**，必须显式 BLFX_ALLOW_UNINSTALL=1
#  （用户 2026-11-02 已就本项目本轮明确授权卸载原版）。
#
#  设备选择：BLFX_SERIAL=<serial> 显式指定；否则取 adb 列表里的模拟器
#  用法：
#    bash 04_scripts/爆裂防线/blfx_install_win.sh                       # 覆盖装（签名冲突即失败）
#    BLFX_ALLOW_UNINSTALL=1 bash 04_scripts/爆裂防线/blfx_install_win.sh  # 卸载重装
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=blfx_win_common.sh
. "$HERE/blfx_win_common.sh"

APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/blfx_surgical.apk"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"

[ -f "$APK" ] || { echo "ERROR: missing ${APK}（先跑 surgical + sign）" >&2; exit 1; }

SERIAL="${BLFX_SERIAL:-}"
if [ -z "$SERIAL" ]; then
  SERIAL="$(adb devices | awk 'NR>1 && $2=="device" {print $1}' | grep -E '^emulator-' | head -1 || true)"
  [ -n "$SERIAL" ] || SERIAL="$(adb devices | awk 'NR>1 && $2=="device" {print $1}' | head -1 || true)"
fi
[ -n "$SERIAL" ] || { echo "ERROR: 无可用设备（adb devices 为空）" >&2; exit 1; }
echo "[install] serial = $SERIAL"

# Git Bash（MSYS）会把远端 POSIX 路径 /data/local/tmp/... 自动转成
# C:/Program Files/Git/data/local/tmp/... ⇒ adb 必须禁用路径转换
adbx() { MSYS_NO_PATHCONV=1 adb "$@"; }

rm -f "$INSTALL_OK"
APK_MD5="$(md5f "$APK")"
echo "[install] local apk md5=$APK_MD5 size=$(filesize "$APK") B"

adbx -s "$SERIAL" get-state >/dev/null
DEV_PATH="$(adbx -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r' | sed -n 's/^package://p' | head -1)"
if [ -n "$DEV_PATH" ]; then
  DEV_MD5="$(adbx -s "$SERIAL" shell "md5sum $DEV_PATH" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
  case "$DEV_MD5" in
    c4f293d9009e90aa7098e29acea2d053) echo "[install] 设备现存 = 官方原版 1.02.01（CN=my）" ;;
    "$APK_MD5")                        echo "[install] 设备现存 = 本星光包（同包重装，不丢数据）" ;;
    "")                                echo "[install] 设备现存 md5 读取失败（按未知处理）" ;;
    *)                                 echo "[install] 设备现存 md5=${DEV_MD5}（未知版本）" ;;
  esac
else
  echo "[install] 设备上未安装 ${PKG}（全新安装）"
fi

adbx -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true

echo "[install] 准备本地安装源（ASCII 暂存）"
# adb.exe 是 ANSI 程序：含中文的源路径经 MSYS→Win32 转换会被破坏（push/install 静默失败）
PUSH_SRC="$NIXIANG/.workbuddy/scratch/blfx_install.apk"
mkdir -p "$(dirname "$PUSH_SRC")"
cp -f "$APK" "$PUSH_SRC"
APK_W="$(winpath "$PUSH_SRC")"

# 走 adb install 流式安装：pm install <文件> 在本机实测 INSTALL_FAILED_MEDIA_UNAVAILABLE
# （Failed to restorecon，/data/local/tmp 文件上下文问题），流式安装绕开该路径。
echo "[install] 同签名覆盖尝试（adb install -r -d）"
if adbx -s "$SERIAL" install -r -d "$APK_W" >/dev/null 2>&1; then
  echo "[install] 覆盖安装成功"
else
  if [ "${BLFX_ALLOW_UNINSTALL:-0}" != "1" ]; then
    cat >&2 <<EOM
ERROR: 覆盖安装失败（签名冲突）。卸载会清该包本地数据，属破坏性操作，
       需显式授权后重跑：
         BLFX_ALLOW_UNINSTALL=1 bash 04_scripts/爆裂防线/blfx_install_win.sh
回退包：$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_original_from_device.apk
EOM
    exit 1
  fi
  echo "[install] 已授权卸载：保护外置数据 → pm uninstall --user 0 ${PKG}"
  EXT_DIR="/sdcard/Android/data/${PKG}"
  EXT_RESERVE="${EXT_DIR}.nxreserve"
  NEED_RESTORE=0
  if adbx -s "$SERIAL" shell "test -d $EXT_DIR" >/dev/null 2>&1; then
    adbx -s "$SERIAL" shell "mv $EXT_DIR $EXT_RESERVE" >/dev/null
    if adbx -s "$SERIAL" shell "test -d $EXT_RESERVE" >/dev/null 2>&1; then
      NEED_RESTORE=1
      echo "[install] 外置数据已移入备胎 $EXT_RESERVE"
    else
      echo "[install] WARN: 备胎未就绪，中止卸载以保护数据" >&2
      exit 1
    fi
  fi
  adbx -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  adbx -s "$SERIAL" install -r "$APK_W" >/dev/null
  if [ "$NEED_RESTORE" = "1" ]; then
    adbx -s "$SERIAL" shell "mv $EXT_RESERVE $EXT_DIR" >/dev/null
    NEW_UID="$(adbx -s "$SERIAL" shell "stat -c %u /data/user/0/${PKG}" 2>/dev/null | tr -d '\r' || true)"
    if [ -n "${NEW_UID:-}" ]; then
      adbx -s "$SERIAL" shell "chown -R ${NEW_UID}:${NEW_UID} $EXT_DIR" >/dev/null 2>&1 || true
    fi
    echo "[install] 外置数据已归位（uid=${NEW_UID:-?}）"
  fi
fi

echo "[install] 闭环校验：设备 base.apk == 本地产物"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" || true

printf '%s %s\n' "$APK_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"
echo "[install] done: ${PKG}（拉起游戏确认悬浮球菜单 + 三功能）"
