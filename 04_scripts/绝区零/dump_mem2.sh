#!/usr/bin/env bash
# Dump a contiguous memory range from the game process via run-as + /proc/pid/mem.
# usage: dump_mem2.sh <start-hex> <end-hex> <local-out> [serial]
S=${4:-96f0d883}
START=$(( $1 ))
END=$(( $2 ))
OUT=$3
PID=$(adb -s $S shell pidof com.miHoYo.Nap | tr -d '\r' | awk '{print $1}')
PAGES=$(( (END - START) / 4096 ))
TMP=/.nx_dump.bin
SKIP=$((START/4096))
echo "pid=$PID start=$(printf 0x%x $START) end=$(printf 0x%x $END) pages=$PAGES"
adb -s $S shell "run-as com.miHoYo.Nap sh -c 'dd if=/proc/$PID/mem of=/data/data/com.miHoYo.Nap/files$TMP bs=4096 skip=$SKIP count=$PAGES 2>/dev/null; ls -l /data/data/com.miHoYo.Nap/files$TMP'" 2>&1
MSYS2_ARG_CONV_EXCL='*' adb -s $S exec-out run-as com.miHoYo.Nap cat /data/data/com.miHoYo.Nap/files$TMP > "$OUT" 2>/dev/null
echo "local: $(stat -c %s "$OUT") bytes -> $OUT"
