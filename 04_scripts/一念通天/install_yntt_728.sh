#!/usr/bin/env bash
# 一念通天 728 管线 install 阶段。
#
# 默认只做同签覆盖安装，覆盖失败即停，不自动卸载。
# 仅在用户明确授权后，设置 YNTT_AUTHORIZED_UNINSTALL=1 才允许：
#   备份 /data/data/com.qinglv.Immortal -> 卸载旧包 -> 安装星光包。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="一念通天"
VERSION="728"
PKG="com.qinglv.Immortal"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/yntt_surgical_728.apk"
REMOTE_BACKUP="/data/local/tmp/yntt_data_backup_728.tgz"
AUTHORIZED_UNINSTALL="${YNTT_AUTHORIZED_UNINSTALL:-0}"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }

adb -s "$SERIAL" get-state >/dev/null
adb -s "$SERIAL" shell am force-stop "$PKG" || true

echo "[install] push $APK"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
trap 'adb -s "$SERIAL" shell rm -f "$REMOTE" "$REMOTE_BACKUP" >/dev/null 2>&1 || true' EXIT

BACKUP_DIR="$NIXIANG/09_test/$GAME/backup_${VERSION}_$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

echo "[install] backup private data before replacement"
if ! adb -s "$SERIAL" shell su -c "tar -C /data/data -czf '$REMOTE_BACKUP' '$PKG'"; then
  echo "[install] BLOCKED: private-data backup failed; no uninstall was performed."
  exit 1
fi
adb -s "$SERIAL" shell su -c "chmod 644 '$REMOTE_BACKUP'" >/dev/null 2>&1 || true
adb -s "$SERIAL" pull "$REMOTE_BACKUP" "$BACKUP_DIR/private_data.tgz" >/dev/null
shasum -a 256 "$BACKUP_DIR/private_data.tgz" > "$BACKUP_DIR/private_data.tgz.sha256"
echo "[install] backup saved: $BACKUP_DIR/private_data.tgz"

echo "[install] try cover install"
if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE"; then
  echo "[install] cover install failed (signature mismatch or downgrade)."
  if [[ "$AUTHORIZED_UNINSTALL" != "1" ]]; then
    echo "[install] BLOCKED: no uninstall was performed; local data is untouched."
    echo "[install] To replace after explicit authorization, set YNTT_AUTHORIZED_UNINSTALL=1."
    exit 1
  fi
  echo "[install] authorized uninstall+reinstall"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  adb -s "$SERIAL" shell pm install --user 0 "$REMOTE"
fi

echo "[install] verify device base.apk == local"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "[install] done: $PKG"
