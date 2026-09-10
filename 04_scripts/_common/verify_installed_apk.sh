#!/usr/bin/env bash
# ================================================================
#  verify_installed_apk.sh — 装机完整性校验（防"装了旧包"假成功）
#
#  用法:
#    bash verify_installed_apk.sh <serial> <包名> <本地APK> [--su]
#
#  逻辑: pm path 取设备上 base.apk 路径 → 设备侧 md5sum → 与本地
#  产物 md5 比对。不一致 = 设备上还是旧包（构建/打包/装机某环没换新），
#  直接 exit 1，由管线 install 阶段置 failed。
#
#  --su: 设备读 /data/app 需要 root 时加（KSU 真机）；MuMu 等 adb 直
#  root 的设备不用加。
#
#  背景（2026-09-10 魂坠深境实战教训）：build_so 假成功/忘 reset 下游
#  → 旧 so 旧包照常"装机成功"，用户看到旧菜单。装机后必须闭环校验。
# ================================================================
set -euo pipefail

usage() { echo "用法: bash verify_installed_apk.sh <serial> <包名> <本地APK> [--su]" >&2; exit 2; }
[ $# -ge 3 ] || usage
SERIAL="$1"; PKG="$2"; LOCAL="$3"; USE_SU="${4:-}"
[ -f "$LOCAL" ] || { echo "ERROR: 本地 APK 不存在: $LOCAL" >&2; exit 1; }

BASE="$(adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null \
  | tr -d '\r' | grep '^package:' | grep 'base\.apk' | head -1 | sed 's/^package://')"
[ -n "$BASE" ] || { echo "ERROR: pm path 未找到 $PKG 的 base.apk（未安装？）" >&2; exit 1; }

if [ "$USE_SU" = "--su" ]; then
  DEV_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $BASE'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}')"
else
  DEV_MD5="$(adb -s "$SERIAL" shell "md5sum $BASE" 2>/dev/null | tr -d '\r\n' | awk '{print $1}')"
fi
[ -n "$DEV_MD5" ] || { echo "ERROR: 设备侧 md5sum 失败（$BASE）；root 设备尝试加 --su" >&2; exit 1; }

LOCAL_MD5="$(md5 -q "$LOCAL")"
if [ "$DEV_MD5" != "$LOCAL_MD5" ]; then
  echo "ERROR: 设备包($DEV_MD5) != 本地产物($LOCAL_MD5) —— 装的是旧包/没装上，禁止宣称装机完成" >&2
  exit 1
fi
echo "[install-verify] 设备 base.apk md5 == 本地产物 ($LOCAL_MD5) ✓"
