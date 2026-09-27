#!/usr/bin/env bash
# 幻想武将模拟器单包静默覆盖安装 + 40s 存活核验；不点击屏幕。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
GAME="幻想武将模拟器"
VERSION="1.0.8"
PKG="com.qxjc.hxwjmlq"
SERIAL="${SERIAL:-$(cat "$ROOT/04_scripts/$GAME/.device_serial" 2>/dev/null || true)}"
APK="${1:-$ROOT/08_output/$GAME/${GAME}_${VERSION}.apk}"
BT="${BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"

[[ -n "$SERIAL" ]] || { echo "ERROR: 缺设备 serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: 缺 APK $APK" >&2; exit 1; }
"$BT/apksigner" verify "$APK" >/dev/null || { echo "ERROR: APK 签名校验失败" >&2; exit 1; }

adb -s "$SERIAL" push "$APK" /data/local/tmp/hxwj_single.apk >/dev/null
adb -s "$SERIAL" shell su -c "/system/bin/pm install -r -d /data/local/tmp/hxwj_single.apk"
adb -s "$SERIAL" shell am force-stop "$PKG"
adb -s "$SERIAL" logcat -c
adb -s "$SERIAL" shell am start -n "$PKG/com.unity3d.player.UnityPlayerActivity" >/dev/null

for i in $(seq 1 20); do
  sleep 2
  pid="$(adb -s "$SERIAL" shell pidof "$PKG" | tr -d '\r')"
  [[ -n "$pid" ]] || { echo "ERROR: 进程在约 $((i*2))s 退出" >&2; exit 1; }
  [[ $((i % 5)) -eq 0 ]] && echo "alive $((i*2))s pid=$pid"
done

LOG="$(adb -s "$SERIAL" logcat -d 2>/dev/null || true)"
printf '%s\n' "$LOG" | grep -aE "RegisterNatives 19/19|StarryBridgeTest.*PASS=|MOD 已就绪|StarryInitProvider|MenuService: overlayVisible=true|CheckSignature" | tail -20
printf '%s\n' "$LOG" | grep -aq "RegisterNatives 19/19" || { echo "ERROR: native 注册未通过" >&2; exit 1; }
printf '%s\n' "$LOG" | grep -aq "MOD 已就绪 tag=hxwj ready=1" || { echo "ERROR: MOD 未就绪" >&2; exit 1; }
echo "[install_single] PASS"
