#!/usr/bin/env bash
# 弹壳战机 1.1.5 星光候选包安装后端。
#
# 默认只尝试同签覆盖，覆盖失败即停，不卸载、不清数据。
# 仅当用户明确授权替换官方异签包时，设置：
#   DKZJ_ALLOW_UNINSTALL=1
# 脚本会先备份 /data/data/com.survivor.acecn，再卸载并安装星光包。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="蛋壳战机"
VERSION="1.1.5"
PKG="com.survivor.acecn"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_starry-releaseSigned.apk"
OFFICIAL="$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_official_from_device.apk"
EXPECT_MD5="55457ccbc564504e42f57d566a2333f6"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"
REMOTE_APK="/data/local/tmp/dkzj_starry_1.1.5.apk"
REMOTE_BACKUP="/data/local/tmp/dkzj_private_backup_1.1.5.tgz"
AUTHORIZED_UNINSTALL="${DKZJ_ALLOW_UNINSTALL:-0}"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }

LOCAL_MD5="$(md5 -q "$APK")"
[[ "$LOCAL_MD5" == "$EXPECT_MD5" ]] \
  || { echo "ERROR: candidate md5 mismatch: $LOCAL_MD5 != $EXPECT_MD5" >&2; exit 1; }

rm -f "$INSTALL_OK"
adb -s "$SERIAL" get-state >/dev/null
adb -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1 \
  || { echo "ERROR: device package not installed: $PKG" >&2; exit 1; }

adb -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true
echo "[install] push $APK"
adb -s "$SERIAL" push "$APK" "$REMOTE_APK" >/dev/null

cleanup() {
  adb -s "$SERIAL" shell rm -f "$REMOTE_APK" "$REMOTE_BACKUP" >/dev/null 2>&1 || true
}
trap 'rc=$?; cleanup; exit $rc' EXIT

echo "[install] try same-signature cover install"
if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK"; then
  if [[ "$AUTHORIZED_UNINSTALL" != "1" ]]; then
    cat >&2 <<'EOM'
ERROR: 覆盖安装失败（官方 gorilla 与星光 StarryMagic 异签）。

卸载会清除游戏本地数据，属于破坏性操作，必须先取得明确授权。
用户确认“允许备份并卸载官方包，再安装星光包”后，运行：
    DKZJ_ALLOW_UNINSTALL=1 bash 04_scripts/蛋壳战机/install_dkzj.sh
回退包：02_apks/蛋壳战机/蛋壳战机_1.1.5_official_from_device.apk
EOM
    exit 1
  fi

  echo "[install] authorized replacement: backup private data before uninstall"
  BACKUP_DIR="$NIXIANG/09_test/$GAME/backup_${VERSION}_$(date +%Y%m%d_%H%M%S)"
  mkdir -p "$BACKUP_DIR"
  adb -s "$SERIAL" shell su -c "tar -C /data/data -czf '$REMOTE_BACKUP' '$PKG'" \
    || { echo "ERROR: private-data backup failed; no uninstall performed" >&2; exit 1; }
  adb -s "$SERIAL" shell su -c "chmod 644 '$REMOTE_BACKUP'" >/dev/null 2>&1 || true
  adb -s "$SERIAL" pull "$REMOTE_BACKUP" "$BACKUP_DIR/private_data.tgz" >/dev/null
  shasum -a 256 "$BACKUP_DIR/private_data.tgz" > "$BACKUP_DIR/private_data.tgz.sha256"
  echo "[install] backup saved: $BACKUP_DIR/private_data.tgz"

  echo "[install] uninstall official package (local app data will be cleared)"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  if ! adb -s "$SERIAL" shell pm install --user 0 "$REMOTE_APK"; then
    echo "[install] candidate install failed; restoring official package" >&2
    if [[ -f "$OFFICIAL" ]]; then
      adb -s "$SERIAL" shell pm install --user 0 "$OFFICIAL" >/dev/null \
        || echo "[install] WARNING: official rollback install also failed" >&2
    else
      echo "[install] WARNING: official rollback APK missing: $OFFICIAL" >&2
    fi
    exit 1
  fi
fi

echo "[install] verify device base.apk == local candidate"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
printf '%s %s\n' "$LOCAL_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"
echo "[install] done: $PKG"
