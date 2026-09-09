#!/usr/bin/env bash
# es2 补充 dump 宿主端。Usage: dump_es2_ext.sh PACKAGE OUTDIR （用 ANDROID_SERIAL 选设备）
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PACKAGE="${1:?PACKAGE}"
OUTDIR="${2:?OUTDIR}"
mkdir -p "$OUTDIR/raw"
DEV_DIR=/data/local/tmp/es2_extdump
PID=$(adb shell pidof "$PACKAGE" | tr -d '\r' | awk '{print $1}')
[[ -n "$PID" ]] || { echo "ERROR: no pid"; exit 2; }
echo "PACKAGE=$PACKAGE PID=$PID OUTDIR=$OUTDIR (serial=${ANDROID_SERIAL:-default})"

adb push "$SCRIPT_DIR/dump_es2_ext_device.sh" /data/local/tmp/dump_es2_ext_device.sh >/dev/null
adb shell "su -c 'chmod 755 /data/local/tmp/dump_es2_ext_device.sh'"
adb shell "su -c 'cat /proc/$PID/maps'" > "$OUTDIR/maps.txt"
adb shell "su -c '/data/local/tmp/dump_es2_ext_device.sh $PID $DEV_DIR'" | tee "$OUTDIR/dump_log.txt"

while IFS= read -r f; do
  f=$(echo "$f" | tr -d '\r')
  [[ -n "$f" ]] || continue
  adb pull "$DEV_DIR/$f" "$OUTDIR/raw/$f" 2>/dev/null || \
    adb shell "su -c 'cat $DEV_DIR/$f'" > "$OUTDIR/raw/$f"
done < <(adb shell "su -c 'ls -1 $DEV_DIR'" | tr -d '\r')
cp -f "$OUTDIR/raw/"* "$OUTDIR/" 2>/dev/null || true
ls -lah "$OUTDIR" | tail -5
echo "OK pid=$PID -> $OUTDIR"
