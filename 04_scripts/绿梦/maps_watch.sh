#!/usr/bin/env bash
# ============================================================================
#  maps_watch.sh — 绿梦：冷启后在 25..34 s 窗口高频抓 /proc/<pid>/maps
#  用途：验证「open(base.apk) 之后是否新出现 r--s 的 base.apk mmap」
#  用法： bash 04_scripts/绿梦/maps_watch.sh [--start 25] [--n 18] [--gap 0.3]
#  产物：09_test/绿梦/fg_forensics/maps_<ts>/maps_NN.txt
# ============================================================================
set -euo pipefail
ROOT="${NIXIANG_ROOT:-$(cd "$(dirname "$0")/../.." && pwd -P)}"
PKG="com.zhuziplay.lvmengnew.op"
ACT="com.bamboo.game.SplashActivity"
START=25; N=18; GAP=0.3
while [ $# -gt 0 ]; do
  case "$1" in
    --start) START="$2"; shift 2 ;;
    --n) N="$2"; shift 2 ;;
    --gap) GAP="$2"; shift 2 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done
D="$ROOT/09_test/绿梦/fg_forensics/maps_$(date +%m%d_%H%M%S)"
mkdir -p "$D"
echo "==> 目录 $D"
adb shell am force-stop "$PKG" || true
adb shell am start -n "$PKG/$ACT" >/dev/null
echo "==> 先等 ${START}s（让启动期噪声过去）"
sleep "${START}"
for i in $(seq 1 "${N}"); do
  PID=$(adb shell pidof "$PKG" 2>/dev/null | tr -d '\r' | awk '{print $1}')
  if [ -z "$PID" ]; then echo "   [$i] 进程已消失"; break; fi
  adb shell "su -c \"cat /proc/$PID/maps\"" > "$D/maps_$(printf %02d "$i").txt" 2>/dev/null || true
  echo "   [$i] pid=$PID lines=$(wc -l < "$D/maps_$(printf %02d "$i").txt" | tr -d ' ')"
  sleep "${GAP}"
done
echo "==> 采样结束"
