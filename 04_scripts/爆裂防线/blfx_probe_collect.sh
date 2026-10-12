#!/usr/bin/env bash
# 爆裂防线 探针采集（阶段 A 专用，可重复跑）
#
#  启动游戏 → 等探针 6 轮（3/8/15/25/40/70 秒）跑完 → 用 root 拉
#  /data/data/com.muyegame.blfx/files/blfx_probe.txt  +  logcat（Mod_Menu / [blfx]）
#
#  产物：09_test/爆裂防线/blfx_probe_<时间戳>.txt
#        09_test/爆裂防线/logcat_<时间戳>.txt
#
#  用法: bash 04_scripts/爆裂防线/blfx_probe_collect.sh [等待秒数=100]
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="爆裂防线"
PKG="com.muyegame.blfx"
ACT="$PKG/com.unity3d.player.MyMainActivity"
DUMP="/data/data/$PKG/files/blfx_probe.txt"
SERIAL_FILE="$SCRIPT_DIR/.device_serial"
WAIT_SEC="${1:-100}"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing ${SERIAL_FILE}（先跑 device_ready_blfx.sh）" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }

OUTDIR="$NIXIANG/09_test/$GAME"
mkdir -p "$OUTDIR"
TS="$(date +%Y%m%d_%H%M%S)"
PROBE_OUT="$OUTDIR/blfx_probe_$TS.txt"
LOG_OUT="$OUTDIR/logcat_$TS.txt"

echo "[probe] serial=$SERIAL 等待 ${WAIT_SEC}s（探针轮次 3/8/15/25/40/70s）"
adb -s "$SERIAL" shell am force-stop "$PKG" || true
adb -s "$SERIAL" logcat -c || true
sleep 2
echo "[probe] am start $ACT"
adb -s "$SERIAL" shell am start -n "$ACT" >/dev/null
sleep "$WAIT_SEC"

echo "[probe] logcat → $LOG_OUT"
adb -s "$SERIAL" logcat -d 2>/dev/null | tr -d '\r' > "$LOG_OUT" || true

echo "[probe] pull $DUMP → $PROBE_OUT"
if adb -s "$SERIAL" shell "su -c 'cat $DUMP'" 2>/dev/null | tr -d '\r' > "$PROBE_OUT"; then
  :
fi
# 探针是否真的落盘：首行应为 "lib ready: ..."，否则说明 so 没跑起来
if grep -q 'lib ready' "$PROBE_OUT" 2>/dev/null; then
  echo "[probe] OK  探针文件 $(wc -l < "$PROBE_OUT" | tr -d ' ') 行"
else
  echo "[probe] WARN 探针文件里没有 'lib ready' —— so 可能没被加载（看 logcat）" >&2
fi

echo "[probe] 进程存活检查"
adb -s "$SERIAL" shell "pidof $PKG" | tr -d '\r' || true

echo "[probe] logcat 关键行（Mod_Menu / blfx）:"
grep -aE 'Mod_Menu|\[blfx\]|StarryMagic|libstarry_magic' "$LOG_OUT" | tail -40 || true

echo "[probe] 崩溃痕迹:"
grep -aE 'FATAL EXCEPTION|DEBUG   |signal [0-9]+|SIGSEGV|SIGABRT|tombstone' "$LOG_OUT" | tail -15 || true

echo "[probe] done: $PROBE_OUT"
