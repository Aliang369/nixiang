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
[ -n "$DEV_MD5" ] || { echo "ERROR: 设备侧 md5sum 失败（${BASE}）；root 设备尝试加 --su" >&2; exit 1; }

LOCAL_MD5="$( if command -v md5 >/dev/null 2>&1; then md5 -q "$LOCAL"; else md5sum "$LOCAL" | awk '{print $1}'; fi )"
if [ "$DEV_MD5" != "$LOCAL_MD5" ]; then
  echo "ERROR: 设备包($DEV_MD5) != 本地产物($LOCAL_MD5) —— 装的是旧包/没装上，禁止宣称装机完成" >&2
  exit 1
fi
echo "[install-verify] 设备 base.apk md5 == 本地产物 ($LOCAL_MD5) ✓"

# ---------- 第二查：运行中的 so == 包内 so（2026-10-16；终末地/小米 byron 事故） ----------
# extractNativeLibs=true 的包，运行时加载的是解出到 /data/app/~~x/<pkg>-y/lib/<abi>/ 的那份 so，
# 它可被 root 热替换（04_scripts/_common/so_hotswap.sh 就是干这个）、或残留旧 OTA so 顶替
# ⇒ **只比 base.apk md5 会出现「核对全绿、跑的是别的 so」**（实测：崩溃帧 so build-id 与包内不同，
# base.apk 却完全正确）。故补第二查，不等即判「热替换态/异 so」并给出恢复命令。
TMP_SO="$(mktemp "${TMPDIR:-/tmp}/install_verify_so.XXXXXX")"
trap 'rm -f "${TMP_SO:-}"' EXIT
if unzip -l "$LOCAL" 2>/dev/null | grep -q 'lib/arm64-v8a/libstarry_magic.so'; then
  PKG_LIB_DIR="$(dirname "$BASE")/lib/arm64"
  if [ "$USE_SU" = "--su" ]; then
    RUN_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $PKG_LIB_DIR/libstarry_magic.so'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}')"
  else
    RUN_MD5="$(adb -s "$SERIAL" shell "md5sum $PKG_LIB_DIR/libstarry_magic.so" 2>/dev/null | tr -d '\r\n' | awk '{print $1}')"
  fi
  if [ -z "$RUN_MD5" ]; then
    echo "- SKIP  [运行 so] 读不到 $PKG_LIB_DIR/libstarry_magic.so（extractNativeLibs=false / 需 --su / 路径形态不同）"
  else
    unzip -p "$LOCAL" lib/arm64-v8a/libstarry_magic.so > "$TMP_SO" 2>/dev/null || true
    APK_SO_MD5="$( if command -v md5 >/dev/null 2>&1; then md5 -q "$TMP_SO"; else md5sum "$TMP_SO" | awk '{print $1}'; fi )"
    if [ "$RUN_MD5" != "$APK_SO_MD5" ]; then
      echo "ERROR: 运行中的 so($RUN_MD5) != 包内 so($APK_SO_MD5) —— 处于「热替换态/异 so」：" >&2
      echo "       base.apk 是新的，但实际被加载的是另一份 so（旧 OTA so 残留或 root 热替换），" >&2
      echo "       症状通常是「一开就崩」或「行为像旧版」，而 base.apk 核对完全正常。" >&2
      echo "       恢复：bash 04_scripts/_common/so_hotswap.sh --serial $SERIAL --pkg $PKG --restore-from $LOCAL --restart" >&2
      exit 1
    fi
    echo "[install-verify] 运行 so md5 == 包内 so ($APK_SO_MD5) ✓"
  fi
fi
