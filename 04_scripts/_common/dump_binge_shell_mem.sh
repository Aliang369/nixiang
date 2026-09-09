#!/usr/bin/env bash
# Dump binge libSDKloading + nearby BSS/exec + libil2cpp r-xp via KernelSU dd.
# Usage: dump_binge_shell_mem.sh PACKAGE OUTDIR
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PACKAGE="${1:-}"
OUTDIR="${2:-}"
if [[ -z "$PACKAGE" || -z "$OUTDIR" ]]; then
  echo "usage: $0 PACKAGE OUTDIR" >&2
  exit 2
fi
mkdir -p "$OUTDIR/raw"
DEV_DIR=/data/local/tmp/binge_memdump
PID=$(adb shell pidof "$PACKAGE" | tr -d '\r' | awk '{print $1}')
if [[ -z "$PID" ]]; then
  echo "ERROR: no pid for $PACKAGE" >&2
  exit 1
fi
echo "PACKAGE=$PACKAGE PID=$PID OUTDIR=$OUTDIR"

adb push "$SCRIPT_DIR/dump_binge_shell_mem_device.sh" /data/local/tmp/dump_binge_shell_mem_device.sh >/dev/null
adb shell "su -c 'chmod 755 /data/local/tmp/dump_binge_shell_mem_device.sh'"
adb shell "su -c 'cat /proc/$PID/maps'" > "$OUTDIR/maps.txt"
adb shell "su -c '/data/local/tmp/dump_binge_shell_mem_device.sh $PID $DEV_DIR'" | tee "$OUTDIR/dump_log.txt"

# Pull all artifacts
while IFS= read -r f; do
  f=$(echo "$f" | tr -d '\r')
  [[ -n "$f" ]] || continue
  adb pull "$DEV_DIR/$f" "$OUTDIR/raw/$f" 2>/dev/null || \
    adb shell "su -c 'cat $DEV_DIR/$f'" > "$OUTDIR/raw/$f"
done < <(adb shell "su -c 'ls -1 $DEV_DIR'" | tr -d '\r')

cp -f "$OUTDIR/raw/"* "$OUTDIR/" 2>/dev/null || true
ls -lah "$OUTDIR" | tee "$OUTDIR/host_listing.txt"
echo "OK pid=$PID -> $OUTDIR"
