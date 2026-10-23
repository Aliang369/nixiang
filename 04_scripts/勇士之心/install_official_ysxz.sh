#!/usr/bin/env bash
# 勇士之心：把「官方底包」（只读重抓的 base.apk）原样安装到指定设备，用于基线取证。
#
# 用途：阶段 A 在无 root 机上验证「官方包本身能否启动」，以区分
#   ① 壳自检因环境（root）变脏 ⇒ dirty=1 ⇒ 跳过 Application.onCreate ⇒ AppInfo.parameters NPE
#   ② 包自身问题（与设备是否 root 无关）
#
# 只在「目标包未安装」或「同签覆盖」时使用；不卸载、不清数据、不改存档。
# 同签不同签由调用方先核对：本脚本只做安装与装后回读，不做任何数据操作。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PKG="${PKG:-com.fightingstars.games.warrior}"
GAME="勇士之心"
ADB_BIN="${ADB_BIN:-adb}"
SERIAL="${SERIAL:?必须显式指定 SERIAL（例如 SERIAL=96f0d883）}"
APK="${APK:-$NIXIANG_ROOT/02_apks/$GAME/${GAME}_1.0.6_taptap_official.apk}"

[[ -f "$APK" ]] || { echo "错误: 官方底包不存在: $APK" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

LOCAL_MD5="$(md5 -q "$APK")"
LOCAL_SHA="$(shasum -a 256 "$APK" | awk '{print $1}')"
LOCAL_SIZE="$(wc -c <"$APK" | tr -d ' ')"
echo "serial=$SERIAL"
echo "package=$PKG"
echo "apk=$APK"
echo "local_md5=$LOCAL_MD5"
echo "local_sha256=$LOCAL_SHA"
echo "local_size=$LOCAL_SIZE"

BEFORE_VERSION="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | sed -n 's/^[[:space:]]*versionName=//p' | head -1 | tr -d '\r')"
echo "installed_version_before=${BEFORE_VERSION:-<未安装>}"

REMOTE_APK="/data/local/tmp/ysxz_official.apk"
"$ADB_BIN" -s "$SERIAL" push "$APK" "$REMOTE_APK" >/dev/null
# -r 覆盖（保数据）；-d 允许降级；不传 -k（不清数据）
if [[ -n "$BEFORE_VERSION" ]]; then
  "$ADB_BIN" -s "$SERIAL" shell pm install -r -d "$REMOTE_APK"
else
  "$ADB_BIN" -s "$SERIAL" shell pm install "$REMOTE_APK"
fi
"$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE_APK" >/dev/null 2>&1 || true

AFTER="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
echo "installed_version_after=$(printf '%s\n' "$AFTER" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
REMOTE_BASE="$(printf '%s\n' "$AFTER" | sed -n 's/^[[:space:]]*codePath=//p' | head -1)/base.apk"
DEV_MD5="$("$ADB_BIN" -s "$SERIAL" shell su -c "md5sum '$REMOTE_BASE'" 2>/dev/null | awk '{print $1}' | tr -d '\r')"
if [[ -z "$DEV_MD5" ]]; then
  echo "device_base_md5=<无法读取（无 root/su 不可用）>"
else
  echo "device_base_md5=$DEV_MD5"
  [[ "$DEV_MD5" == "$LOCAL_MD5" ]] || { echo "错误: 设备 base.apk md5 与产物不一致" >&2; exit 3; }
  echo "✓ 装机一致"
fi
