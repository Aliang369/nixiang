#!/usr/bin/env bash
#  fg_pagewatch.sh — clear_refs + pagemap + kpageflags 观测 base.apk 映射页是否被访问
#  用法: WAIT=6 bash 04_scripts/绿梦/fg_pagewatch.sh
set -uo pipefail
PKG="com.zhuziplay.lvmengnew.op"
ACT="com.bamboo.game.SplashActivity"
TARGET_OFF=0xAB3C000
MAP_OFF=0xAB36000
adb shell am force-stop "$PKG" >/dev/null 2>&1 || true
sleep 2
adb shell am start -n "$PKG/$ACT" >/dev/null 2>&1
sleep "${WAIT:-45}"
PID=$(adb shell pidof "$PKG" | tr -d '\r' | awk '{print $1}')
[ -n "$PID" ] || { echo "no pid（进程已死）"; exit 1; }
echo "pid=$PID"
LINE=$(adb shell "su -c \"grep 'base.apk' /proc/$PID/maps\"" | tr -d '\r' | awk -v o="$(printf %08x $MAP_OFF)" '$3==o {print; exit}')
echo "map: $LINE"
START=$(echo "$LINE" | cut -d- -f1)
VA=$(printf '0x%x' $(( 0x$START + TARGET_OFF - MAP_OFF )))
echo "VA=$VA"
probe() {
  local tag="$1"
  adb shell "su -c \"E=\$(dd if=/proc/$PID/pagemap bs=8 skip=$(( VA / 4096 )) count=1 2>/dev/null | od -An -tu8 | tr -d ' '); echo entry=\$E; PFN=\$(( E & 0x7fffffffffffff )); F=\$(dd if=/proc/kpageflags bs=8 skip=\$PFN count=1 2>/dev/null | od -An -tu8 | tr -d ' '); echo kpageflags=\$F ref=\$(( (F>>2) & 1 )) active=\$(( (F>>5) & 1 ))\"" 2>&1 | tr -d '\r' | sed "s/^/  [$tag] /"
}
probe "t0"
adb shell "su -c 'echo 1 > /proc/$PID/clear_refs'" >/dev/null 2>&1
sleep 2
probe "t0+2s(after clear_refs)"
