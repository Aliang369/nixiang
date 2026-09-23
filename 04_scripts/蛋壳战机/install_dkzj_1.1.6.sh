#!/usr/bin/env bash
# 弹壳战机 1.1.6 (vc27) 星光候选包安装后端。
#
# 默认只尝试同签覆盖，覆盖失败即停，不卸载、不清数据。
# 官方包签名 O=gorilla，星光包签名 CN=StarryMagic，异签无法覆盖，
# 因此替换必须经过「卸载官方包 → 安装星光包」。
#
# 破坏性操作授权开关（默认关闭）：
#   DKZJ_ALLOW_UNINSTALL=1
# 打开后脚本会：
#   1) root tar 备份 /data/data/com.survivor.acecn 到 09_test/蛋壳战机/backup_*/
#   2) pm uninstall -k --user 0   （-k = 保留数据目录，不删存档）
#   3) pm install --user 0 星光包
#   4) 安装失败时自动回滚安装官方包
#   5) 用 root 把数据目录属主/上下文修正到新 uid（-k 保留数据后 uid 可能变化）
#
# 回退包（不可再生输入，务必保留）：
#   02_apks/蛋壳战机/蛋壳战机_1.1.6_official_from_device.apk
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="蛋壳战机"
VERSION="1.1.6"
PKG="com.survivor.acecn"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_starry-releaseSigned.apk"
OFFICIAL="$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_official_from_device.apk"
EXPECT_MD5="c8944f4579afa12e267f72e6d4164d0a"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"
REMOTE_APK="/data/local/tmp/dkzj_starry_1.1.6.apk"
REMOTE_BACKUP="/data/local/tmp/dkzj_private_backup_1.1.6.tgz"
AUTHORIZED_UNINSTALL="${DKZJ_ALLOW_UNINSTALL:-0}"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }
[[ -f "$OFFICIAL" ]] || { echo "ERROR: missing rollback APK $OFFICIAL" >&2; exit 1; }

LOCAL_MD5="$(md5 -q "$APK")"
[[ "$LOCAL_MD5" == "$EXPECT_MD5" ]] \
  || { echo "ERROR: candidate md5 mismatch: $LOCAL_MD5 != $EXPECT_MD5" >&2; exit 1; }

rm -f "$INSTALL_OK"
adb -s "$SERIAL" get-state >/dev/null
adb -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1 \
  || { echo "ERROR: device package not installed: $PKG" >&2; exit 1; }

echo "[install] device current package:"
adb -s "$SERIAL" shell dumpsys package "$PKG" | grep -E "versionName=|versionCode=|userId=" | sed 's/^/  /'
adb -s "$SERIAL" shell su -c "du -sh /data/data/$PKG 2>/dev/null" | sed 's/^/  data: /' || true

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

替换必须卸载官方包，属于破坏性操作，需要明确授权。
授权后运行（会用 -k 保留存档，并先做 root tar 备份）：
    DKZJ_ALLOW_UNINSTALL=1 bash 04_scripts/蛋壳战机/install_dkzj_1.1.6.sh
回退包：02_apks/蛋壳战机/蛋壳战机_1.1.6_official_from_device.apk
EOM
    exit 1
  fi

  echo "[install] authorized replacement: backup private data first"
  BACKUP_DIR="$NIXIANG/09_test/$GAME/backup_${VERSION}_$(date +%Y%m%d_%H%M%S)"
  mkdir -p "$BACKUP_DIR"
  adb -s "$SERIAL" shell su -c "tar -C /data/data -czf '$REMOTE_BACKUP' '$PKG'" \
    || { echo "ERROR: private-data backup failed; no uninstall performed" >&2; exit 1; }
  adb -s "$SERIAL" shell su -c "chmod 644 '$REMOTE_BACKUP'" >/dev/null 2>&1 || true
  adb -s "$SERIAL" pull "$REMOTE_BACKUP" "$BACKUP_DIR/private_data.tgz" >/dev/null
  shasum -a 256 "$BACKUP_DIR/private_data.tgz" > "$BACKUP_DIR/private_data.tgz.sha256"
  echo "[install] backup saved: $BACKUP_DIR/private_data.tgz"

  OLD_UID="$(adb -s "$SERIAL" shell dumpsys package "$PKG" | sed -n 's/.*userId=\([0-9]*\).*/\1/p' | head -1 | tr -d '\r')"
  # DKZJ_KEEP_DATA=0 → 彻底卸载（连存档一起清），用户明确选择的「最干净」路径。
  if [[ "${DKZJ_KEEP_DATA:-1}" == "0" ]]; then
    echo "[install] uninstall official package (full, app data will be cleared), old uid=${OLD_UID:-unknown}"
    adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  else
    echo "[install] uninstall official package (-k keeps app data), old uid=${OLD_UID:-unknown}"
    adb -s "$SERIAL" shell pm uninstall -k --user 0 "$PKG" >/dev/null
  fi

  if ! adb -s "$SERIAL" shell pm install --user 0 "$REMOTE_APK"; then
    echo "[install] candidate install failed; restoring official package" >&2
    adb -s "$SERIAL" push "$OFFICIAL" "$REMOTE_APK" >/dev/null
    adb -s "$SERIAL" shell pm install --user 0 "$REMOTE_APK" >/dev/null \
      || echo "[install] WARNING: official rollback install also failed" >&2
    exit 1
  fi

  NEW_UID="$(adb -s "$SERIAL" shell dumpsys package "$PKG" | sed -n 's/.*userId=\([0-9]*\).*/\1/p' | head -1 | tr -d '\r')"
  if [[ -n "${NEW_UID:-}" ]]; then
    echo "[install] fix data dir ownership for new uid=$NEW_UID"
    adb -s "$SERIAL" shell su -c "chown -R $NEW_UID:$NEW_UID /data/data/$PKG" >/dev/null 2>&1 || true
    adb -s "$SERIAL" shell su -c "restorecon -RF /data/data/$PKG" >/dev/null 2>&1 || true
  fi
fi

echo "[install] verify device base.apk == local candidate"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
printf '%s %s\n' "$LOCAL_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"
echo "[install] done: $PKG"
