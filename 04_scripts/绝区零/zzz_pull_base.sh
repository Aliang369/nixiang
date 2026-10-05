#!/usr/bin/env bash
# 绝区零 官方底包只读拉取（MuMu 127.0.0.1:16384）
#   用途：星空版外科组装输入（README/硬约束 #7：只读，不改动 MuMu 上的官方包）
#   产物：$TEMP/nx_zzz_starry_20261005/base_official.apk（用完即删，只在台账登记 md5）
# 用法：bash 04_scripts/绝区零/zzz_pull_base.sh [serial]
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
SERIAL="${1:-${SERIAL:-127.0.0.1:16384}}"
PKG="${PKG:-com.miHoYo.Nap}"
OUT_DIR="${ZZZ_BASE_DIR:-${TEMP:-/tmp}/nx_zzz_starry_20261005}"
OUT_APK="$OUT_DIR/base_official.apk"
ADB="${ADB:-adb}"

mkdir -p "$OUT_DIR"

echo "[1/4] 设备检查：$SERIAL"
"$ADB" -s "$SERIAL" get-state >/dev/null

echo "[2/4] pm path $PKG"
APK_PATH="$("$ADB" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | head -1 | sed 's/^package://')"
[ -n "$APK_PATH" ] || { echo "ERROR: $PKG 未安装或 pm path 为空" >&2; exit 1; }
echo "  $APK_PATH"

echo "[3/4] adb pull → $OUT_APK"
"$ADB" -s "$SERIAL" pull "$APK_PATH" "$OUT_APK" >/dev/null

echo "[4/4] 校验"
SIZE="$(stat -f %z "$OUT_APK" 2>/dev/null || stat -c %s "$OUT_APK")"
MD5="$(md5 -q "$OUT_APK" 2>/dev/null || md5sum "$OUT_APK" | awk '{print $1}')"
echo "  size=$SIZE"
echo "  md5=$MD5"
echo "  路径=$OUT_APK"
