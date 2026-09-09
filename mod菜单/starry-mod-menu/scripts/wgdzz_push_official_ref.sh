#!/usr/bin/env bash
# 推送官方 base.apk 到设备，供 kadp 验签 IO 重定向（一次性，约 509MB）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
OFFICIAL="${OFFICIAL_APK:-$NIXIANG/02_apks/王国大作战前线/base_1.1.7646.apk}"
PKG="com.xr.wgdzztap"
REMOTE_DIR="/data/user/0/$PKG/files/starry_ref"
REMOTE="$REMOTE_DIR/base.apk"

[[ -f "$OFFICIAL" ]] || { echo "ERROR: missing $OFFICIAL"; exit 1; }

echo "[push] $OFFICIAL -> device tmp"
adb push "$OFFICIAL" /data/local/tmp/wgdzz_official_ref.apk

if adb shell "su -c 'id'" >/dev/null 2>&1; then
  adb shell "su -c 'UID=\$(cmd package list packages -U $PKG | sed \"s/.*uid://\"); mkdir -p \"$REMOTE_DIR\"; cp /data/local/tmp/wgdzz_official_ref.apk \"$REMOTE\"; chown -R \$UID:\$UID /data/user/0/$PKG/files; chmod 700 \"$REMOTE_DIR\"; chmod 600 \"$REMOTE\"; ls -la \"$REMOTE\"'"
else
  adb shell "run-as $PKG mkdir -p files/starry_ref"
  adb push "$OFFICIAL" "$REMOTE"
  adb shell "ls -la '$REMOTE'"
fi

echo "DONE: official ref ready for Wgdzz IO redirect"
