#!/usr/bin/env bash
# 无悔华夏 - 从真机拉取当前已安装 base.apk（用户授权：真机现装包即本轮外科底包）
# 只读操作：不安装、不卸载、不清数据、不改设备文件。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
SERIAL="${SERIAL:-f9a116a2}"
PKG="${WHRHX_PKG:-com.yhhy.whrhx.aligames}"
OUT_DIR="$NIXIANG/06_dumped/无悔华夏/current_20260923/pkg"
OUT_APK="${WHRHX_OUT_APK:-$OUT_DIR/base_installed_20260923.apk}"
EXPECT_SHA="${WHRHX_EXPECT_SHA:-}"

command -v adb >/dev/null 2>&1 || { echo "ERROR: adb not found in PATH" >&2; exit 1; }

mkdir -p "$OUT_DIR"

DEV_PATH="$(adb -s "$SERIAL" shell "su -c 'pm path $PKG'" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
[[ -n "$DEV_PATH" ]] || { echo "ERROR: cannot resolve installed apk path for $PKG" >&2; exit 1; }
echo "[pull] device path: $DEV_PATH"

DEV_SIZE="$(adb -s "$SERIAL" shell "su -c 'stat -c %s $DEV_PATH'" | tr -d '\r' | tr -dc '0-9')"
[[ -n "$DEV_SIZE" ]] || { echo "ERROR: cannot stat device apk" >&2; exit 1; }
echo "[pull] device size: $DEV_SIZE"

adb -s "$SERIAL" exec-out "su -c 'cat $DEV_PATH'" > "$OUT_APK"

LOCAL_SIZE="$(stat -f %z "$OUT_APK")"
[[ "$LOCAL_SIZE" == "$DEV_SIZE" ]] || {
  echo "ERROR: size mismatch local=$LOCAL_SIZE device=$DEV_SIZE" >&2
  exit 1
}

SHA="$(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
echo "[pull] local   = $OUT_APK"
echo "[pull] size    = $LOCAL_SIZE"
echo "[pull] sha256  = $SHA"

if [[ -n "$EXPECT_SHA" && "$SHA" != "$EXPECT_SHA" ]]; then
  echo "ERROR: unexpected sha256 want=$EXPECT_SHA got=$SHA" >&2
  exit 1
fi
echo "[pull] done"
