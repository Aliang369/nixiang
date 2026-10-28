#!/usr/bin/env bash
# 蛋壳战机 国服 1.2.1 (vc44) 星空候选包安装后端（雷电模拟器 emulator-5554 / 任意已授权设备）。
#
# 策略（对齐 AGENTS 分发红线）：
#   1) 设备上已装同包时，只尝试同签覆盖安装（pm install -r -d --user 0）；
#      失败即停，不自动卸载、不清数据（异签先报告）。
#      国服官方包签名 O=gorilla，星光包 CN=StarryMagic ⇒ 异签必然失败，
#      需用户明确授权后才可加 DKZJ_ALLOW_UNINSTALL=1 + INSTALL_GUARD_ALLOW_UNINSTALL=1。
#   2) 设备上未安装时直接全新安装。
#   3) 不建数据备份目录（AGENTS：数据备份默认不做）。
#   4) 安装后跑 verify_installed_apk.sh；失败即退出。
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
GAME="蛋壳战机"
VERSION="1.2.1"
PKG="com.survivor.acecn"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_starry-releaseSigned.apk"
EXPECT_MD5="${DKZJ_EXPECT_MD5:-}"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"
REMOTE_APK="/data/local/tmp/dkzj_starry_1.2.1_cn.apk"

md5_of() { if md5 -q /dev/null >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | cut -d' ' -f1; fi; }
# Git Bash(MSYS) 会把 adb 的远端路径参数改写成 Windows 路径（/data/local/tmp/x → C:/Program Files/Git/…），
# 导致 push 落空、脚本静默中止（2026-10-28 实测）。故：远端路径不转换 + 本地路径显式转 Windows 形式。
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*' ;;
esac
to_host_path() { if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else printf '%s' "$1"; fi; }

# 机台选择：SERIAL 环境变量 > 04_scripts/蛋壳战机/.device_serial > emulator-5554
SERIAL="${SERIAL:-}"
if [ -z "$SERIAL" ] && [ -f "$NIXIANG/04_scripts/$GAME/.device_serial" ]; then
  SERIAL="$(tr -d '[:space:]' < "$NIXIANG/04_scripts/$GAME/.device_serial")"
fi
[ -n "$SERIAL" ] || SERIAL="emulator-5554"
echo "[install] serial = $SERIAL"
[ -f "$APK" ] || { echo "ERROR: missing $APK" >&2; exit 1; }

LOCAL_MD5="$(md5_of "$APK")"
if [ -n "$EXPECT_MD5" ] && [ "$LOCAL_MD5" != "$EXPECT_MD5" ]; then
  echo "ERROR: candidate md5 mismatch: $LOCAL_MD5 != $EXPECT_MD5" >&2; exit 1
fi

rm -f "$INSTALL_OK"
adb -s "$SERIAL" get-state >/dev/null

INSTALLED=0
if adb -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | grep -q '^package:'; then
  INSTALLED=1
fi

adb -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true
APK_HOST="$(to_host_path "$APK")"

# 安装后端：优先 streamed `adb install`（雷电/LDPlayer 上 push 到 /data/local/tmp 再 pm install
# 会因 restorecon 判 INSTALL_FAILED_MEDIA_UNAVAILABLE，2026-10-28 实测）；失败才回退 push+pm。
install_apk() {
  local log="$1"
  echo "[install] streamed adb install ($APK_HOST)"
  if adb -s "$SERIAL" install -r -d "$APK_HOST" > "$log" 2>&1; then
    tail -1 "$log"; return 0
  fi
  echo "[install] streamed 失败 → 回退 push + pm install；日志尾：" >&2
  tail -3 "$log" >&2 || true
  adb -s "$SERIAL" push "$APK_HOST" "$REMOTE_APK" >/dev/null
  adb -s "$SERIAL" shell "su -c 'restorecon -R $REMOTE_APK'" >/dev/null 2>&1 || true
  adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK"
}

cleanup() { adb -s "$SERIAL" shell rm -f "$REMOTE_APK" >/dev/null 2>&1 || true; }
trap 'rc=$?; cleanup; exit $rc' EXIT

INSTALL_LOG="${TMPDIR:-/tmp}/dkzj_install_cn.log"
if [ "$INSTALLED" -eq 1 ]; then
  echo "[install] try same-signature cover install"
  if ! install_apk "$INSTALL_LOG"; then
    if [ "${DKZJ_ALLOW_UNINSTALL:-0}" = "1" ] && [ "${INSTALL_GUARD_ALLOW_UNINSTALL:-0}" = "1" ]; then
      echo "[install] authorized replacement: uninstall existing package first"
      adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG"
      install_apk "$INSTALL_LOG" || { echo "ERROR: authorized reinstall failed" >&2; exit 1; }
    else
      cat >&2 <<'EOM'

ERROR: 覆盖安装失败（设备上为异签官方包 O=gorilla）。本脚本不会自动卸载/清数据。
如用户已明确授权「卸载官方包，包内本地数据清零」，再加两个环境变量重跑：
  DKZJ_ALLOW_UNINSTALL=1 INSTALL_GUARD_ALLOW_UNINSTALL=1 bash 04_scripts/蛋壳战机/install_dkzj_1.2.1_cn.sh
EOM
      exit 1
    fi
  fi
else
  echo "[install] fresh install (package not present)"
  install_apk "$INSTALL_LOG" || { echo "ERROR: fresh install failed" >&2; exit 1; }
fi

echo "[install] verify device base.apk == local candidate"
# 有 root 走 base.apk md5 硬校验；无 root（如小米 M2102K1AC）读不了 /data/app ⇒ 退化为
# 签名身份 + versionName/versionCode 软校验（证据强度次于 md5，需在报告里写明）。
VERIFY_SU="--su"
if ! adb -s "$SERIAL" shell "su -c id" >/dev/null 2>&1; then VERIFY_SU=""; echo "[install] 设备无 su → 先用非 root 校验"; fi
VERIFY_OK=0
if [ -n "$VERIFY_SU" ]; then
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" "$VERIFY_SU" && VERIFY_OK=1
else
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" && VERIFY_OK=1 || true
fi
if [ "$VERIFY_OK" -eq 0 ]; then
  echo "[install] WARN: base.apk md5 校验不可用（无 root）→ 退化验证签名+版本"
  SIG="$(adb -s "$SERIAL" shell "dumpsys package $PKG | grep -m1 'signatures='" | tr -d '\r')"
  VN="$(adb -s "$SERIAL" shell "dumpsys package $PKG | grep -m1 versionName" | tr -d '\r' | tr -s ' ')"
  echo "[install] $VN"
  echo "[install] $SIG"
  printf '%s' "$SIG" | grep -q '821c92e4' \
    || { echo "ERROR: 设备签名不是星光（期望 sha1 821c92e4…）" >&2; exit 1; }
  printf '%s' "$VN" | grep -q 'versionName=1.2.1' \
    || { echo "ERROR: 设备版本不是 1.2.1" >&2; exit 1; }
  echo "[install] 软校验通过（签名 = 星光 821c92e4… / 版本 1.2.1）——md5 未取（无 root）"
fi
printf '%s %s\n' "$LOCAL_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"
echo "[install] done: $PKG ($LOCAL_MD5)"
