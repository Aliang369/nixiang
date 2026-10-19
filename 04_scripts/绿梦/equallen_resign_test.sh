#!/usr/bin/env bash
# ============================================================================
#  equallen_resign_test.sh — 绿梦「等长重签」单变量实验：装机 + 冷启 + 观测 35s 闸门
#
#  假设：若 35s 干净退出是「APK 尺寸」触发，等长包应能活过 35s；
#        若仍是 ~30s 干净退出 ⇒ 尺寸不是变量（指向签名内容/其它）。
#
#  用法： bash 04_scripts/绿梦/equallen_resign_test.sh [--apk 包] [--serial S] [--wait 45]
#  退出码：0=已装机并完成观测（结论看输出）；1=装机失败；2=用法/环境错误
#  ⚠ 会 force-stop 游戏并冷启，打断当前对局。
# ============================================================================
set -euo pipefail
ROOT="${NIXIANG_ROOT:-$(cd "$(dirname "$0")/../.." && pwd -P)}"
APK="$ROOT/07_patched/绿梦/fg_test/绿梦_equallen_resign.apk"
PKG="com.zhuziplay.lvmengnew.op"
ACT="com.bamboo.game.SplashActivity"
SERIAL=""
WAIT=45
while [ $# -gt 0 ]; do
  case "$1" in
    --apk) APK="$2"; shift 2 ;;
    --serial) SERIAL="$2"; shift 2 ;;
    --wait) WAIT="$2"; shift 2 ;;
    -h|--help) sed -n '2,14p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done
ADB=(adb); [ -n "$SERIAL" ] && ADB=(adb -s "$SERIAL")
[ -f "$APK" ] || { echo "缺包: $APK" >&2; exit 2; }
OUTDIR="$ROOT/09_test/绿梦/fg_forensics"; mkdir -p "$OUTDIR"
TS=$(date +%m%d_%H%M%S)
LOG="$OUTDIR/equallen_${TS}.txt"

echo "== 装机（覆盖，不清数据）=="
echo "   apk: $APK  size=$(stat -f%z "$APK")  md5=$(md5 -q "$APK")"
"${ADB[@]}" install -r "$APK" || { echo "装机失败" >&2; exit 1; }
echo "   已装机: $("${ADB[@]}" shell dumpsys package "$PKG" | grep -m1 lastUpdateTime)"

echo "== 冷启 + 观测 ${WAIT}s =="
"${ADB[@]}" shell am force-stop "$PKG" || true
"${ADB[@]}" logcat -c || true
"${ADB[@]}" logcat -v time ActivityManager:I Zygote:I DEBUG:I libc:I AndroidRuntime:E '*:S' > "$LOG" &
LPID=$!
sleep 2
"${ADB[@]}" shell am start -W -n "$PKG/$ACT" 2>&1 | head -4 || true
sleep "${WAIT}"
kill "$LPID" 2>/dev/null || true
wait "$LPID" 2>/dev/null || true

echo "== 判定（日志 ${LOG}）=="
grep -E "Start proc|Death received|exited cleanly|has died|FATAL|signal" "$LOG" | head -12 || echo "   （无匹配）"
if grep -q "exited cleanly" "$LOG"; then
  echo "   >>> 结论：进程在 ${WAIT}s 窗口内【干净退出】"
elif grep -q "has died" "$LOG"; then
  echo "   >>> 结论：进程在 ${WAIT}s 窗口内【非干净死亡】"
else
  echo "   >>> 结论：${WAIT}s 窗口内【仍存活】"
fi
