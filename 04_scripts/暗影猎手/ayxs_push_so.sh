#!/usr/bin/env bash
# 暗影猎手 OTA 快速迭代：把新 so 推到 starry_ota 目录，绕过 1GB 重装（约 30 秒一轮）
# 路径规则（PlayOtaPaths）：<filesDir>/starry_ota/<game_id>/libstarry_magic.so
# 用法: bash 04_scripts/暗影猎手/ayxs_push_so.sh
set -euo pipefail
NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
SO="$NIXIANG/mod菜单/starry-mod-menu/libs_ayxs/arm64-v8a/libstarry_magic.so"
PKG="com.enigma.shadowhunter.m4399"
GID="ayxssldsj"
SER="${1:-f9a116a2}"
ADB="/opt/homebrew/bin/adb"

[ -f "$SO" ] || { echo "ERROR: 缺 ${SO}（先跑 ayxs_build_so.sh）"; exit 1; }
LC_ALL=C grep -aq '\[Ayxs\]' "$SO" || { echo "ERROR: so 无 [Ayxs] 标签"; exit 1; }
H="$(shasum -a 256 "$SO" | cut -c1-16)"
echo "[ota] so sha256=$H  $(stat -f %z "$SO") B"

"$ADB" -s "$SER" push "$SO" /data/local/tmp/libstarry_magic.so >/dev/null
UID_LINE="$("$ADB" -s "$SER" shell "su -c 'stat -c %u /data/data/$PKG'" | tr -d '\r')"
"$ADB" -s "$SER" shell "su -c 'mkdir -p /data/data/$PKG/files/starry_ota/$GID && cp -f /data/local/tmp/libstarry_magic.so /data/data/$PKG/files/starry_ota/$GID/libstarry_magic.so && chown $UID_LINE:$UID_LINE /data/data/$PKG/files/starry_ota/$GID/libstarry_magic.so && chmod 644 /data/data/$PKG/files/starry_ota/$GID/libstarry_magic.so && restorecon -R /data/data/$PKG/files/starry_ota 2>/dev/null; ls -l /data/data/$PKG/files/starry_ota/$GID/'"
echo "[ota] 已落盘；重启游戏生效"
"$ADB" -s "$SER" shell am force-stop "$PKG"
"$ADB" -s "$SER" shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" >/dev/null 2>&1
sleep 12
"$ADB" -s "$SER" logcat -d 2>/dev/null | grep -E "loaded OTA so|resolveOtaSo|lib ready|install pass" | tail -5
