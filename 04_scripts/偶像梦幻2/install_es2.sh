#!/usr/bin/env bash
# 偶像梦幻祭2 install 阶段：默认硬拒绝；只有用户明确授权卸载官方包后才能运行。
# 授权标志必须由用户确认后单独创建，脚本不会自行创建。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="偶像梦幻2"
VERSION="4.5.8861"
PKG="com.happyelements.es2"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
AUTH_FILE="$NIXIANG/04_scripts/$GAME/UNINSTALL_AUTHORIZED"
REMOTE="/data/local/tmp/es2_starry.apk"

[[ -f "$AUTH_FILE" ]] || {
  echo "ERROR: 未取得卸载官方包/不保留数据的具体授权；拒绝 install。" >&2
  echo "需要用户明确确认后，再创建授权标志: $AUTH_FILE" >&2
  exit 1
}
[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing serial file" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -f "$APK" ]] || { echo "ERROR: missing APK: $APK" >&2; exit 1; }

adb -s "$SERIAL" get-state >/dev/null
adb -s "$SERIAL" shell am force-stop "$PKG" || true
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
trap 'adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true' EXIT

if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE"; then
  echo "[install] 官方签名与星光签名不兼容；按用户明确授权卸载官方包并安装。"
  echo "[install] 警告：该操作会删除官方包应用数据，不执行清数据命令。"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  adb -s "$SERIAL" shell pm install --user 0 "$REMOTE"
fi

bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
printf 'installed=true\nserial=%s\npackage=%s\napk=%s\nmd5=%s\n' \
  "$SERIAL" "$PKG" "$APK" "$(md5 -q "$APK")" > "$NIXIANG/04_scripts/$GAME/install_result.txt"
echo "[install] done: $PKG"
