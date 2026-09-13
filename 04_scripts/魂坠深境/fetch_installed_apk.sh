#!/usr/bin/env bash
# 只读提取真机已安装的《魂坠深境》星空包单一 base APK。
# 用途：对照分析。只读设备，不卸载、不清数据、不安装，拒绝覆盖已有产物。
set -euo pipefail

if [[ $# -ne 3 ]]; then
  echo "用法: $0 <serial> <package> <output.apk>" >&2
  exit 1
fi

SERIAL="$1"
PKG="$2"
OUT="$3"
TMP="${OUT}.part"
AAPT2="${AAPT2:-aapt2}"
APKSIGNER="${APKSIGNER:-apksigner}"

if [[ -e "$OUT" ]]; then
  echo "错误: 目标 APK 已存在，拒绝覆盖: $OUT" >&2
  exit 1
fi

if ! adb -s "$SERIAL" get-state >/dev/null 2>&1; then
  echo "错误: 设备不可用: $SERIAL" >&2
  exit 1
fi

DUMP="$(adb -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
PACKAGE_PATHS="$(adb -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"

[[ -n "$VERSION_NAME" ]] || { echo "错误: 无法读取 versionName: $PKG" >&2; exit 1; }
[[ -n "$VERSION_CODE" ]] || { echo "错误: 无法读取 versionCode: $PKG" >&2; exit 1; }
[[ -n "$PACKAGE_PATHS" ]] || { echo "错误: 设备未安装包: $PKG" >&2; exit 1; }

PATH_COUNT="$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')"
if [[ "$PATH_COUNT" -ne 1 ]]; then
  echo "错误: 检测到 $PATH_COUNT 个 APK 路径；当前只允许完整单 base APK，拒绝漏拉 split。" >&2
  printf '%s\n' "$PACKAGE_PATHS" >&2
  exit 1
fi

REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"
case "$REMOTE_APK" in
  /data/app/*) ;;
  *) echo "错误: 非预期安装路径: $REMOTE_APK" >&2; exit 1 ;;
esac

mkdir -p "$(dirname "$OUT")"
rm -f "$TMP"
trap 'rm -f "$TMP"' EXIT

adb -s "$SERIAL" pull "$REMOTE_APK" "$TMP"
unzip -tq "$TMP" >/dev/null

APK_PACKAGE=""
APK_VERSION_NAME=""
APK_VERSION_CODE=""
if command -v "$AAPT2" >/dev/null 2>&1; then
  BADGING="$("$AAPT2" dump badging "$TMP")"
  APK_PACKAGE="$(printf '%s\n' "$BADGING" | sed -n "s/^package: name='\([^']*\)'.*/\1/p" | head -1)"
  APK_VERSION_NAME="$(printf '%s\n' "$BADGING" | sed -n "s/.*versionName='\([^']*\)'.*/\1/p" | head -1)"
  APK_VERSION_CODE="$(printf '%s\n' "$BADGING" | sed -n "s/.*versionCode='\([^']*\)'.*/\1/p" | head -1)"
  [[ "$APK_PACKAGE" == "$PKG" ]] || { echo "错误: 拉取包名不匹配: device=$PKG apk=$APK_PACKAGE" >&2; exit 1; }
  [[ "$APK_VERSION_NAME" == "$VERSION_NAME" && "$APK_VERSION_CODE" == "$VERSION_CODE" ]] || {
    echo "错误: 拉取包版本不匹配: device=$VERSION_NAME/$VERSION_CODE apk=$APK_VERSION_NAME/$APK_VERSION_CODE" >&2
    exit 1
  }
else
  echo "警告: 未找到 AAPT2 ($AAPT2)，跳过包名/版本交叉校验" >&2
fi

SIG_STATUS="skipped"
SIG_OUT=""
if command -v "$APKSIGNER" >/dev/null 2>&1; then
  if SIG_OUT="$("$APKSIGNER" verify --print-certs "$TMP" 2>&1)"; then
    SIG_STATUS="ok"
  else
    SIG_STATUS="failed"
  fi
fi

STARRY_SO="$(unzip -Z1 "$TMP" 2>/dev/null | grep -E '(^|/)libstarry_magic\.so$' | head -1 || true)"
[[ -n "$STARRY_SO" ]] || STARRY_SO="NOT_FOUND"

mv "$TMP" "$OUT"

echo "package=$PKG"
echo "versionName=$VERSION_NAME"
echo "versionCode=$VERSION_CODE"
echo "installer=$INSTALLER"
echo "primaryCpuAbi=$PRIMARY_ABI"
echo "serial=$SERIAL"
echo "remote=$REMOTE_APK"
echo "output=$OUT"
echo "size=$(stat -f '%z' "$OUT")"
echo "sha256=$(shasum -a 256 "$OUT" | awk '{print $1}')"
echo "md5=$(md5 -q "$OUT")"
echo "signature_verified=$SIG_STATUS"
echo "starry_magic_so=$STARRY_SO"
printf '%s\n' "$SIG_OUT" | sed -n '1,24p'
