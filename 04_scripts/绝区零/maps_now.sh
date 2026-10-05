#!/usr/bin/env bash
# Fresh maps snapshot + analysis of mod-lib mappings and anonymous neighbors.
S=96f0d883
W=/c/Users/Administrator/AppData/Local/Temp/nx_zzz_work_20261005
PID=$(adb -s $S shell pidof com.miHoYo.Nap | tr -d '\r' | awk '{print $1}')
echo "PID=$PID"
adb -s $S shell "run-as com.miHoYo.Nap cat /proc/$PID/maps" > $W/nap_maps_now.txt 2>&1
echo "lines: $(wc -l < $W/nap_maps_now.txt)"
echo "=== all game-path mappings ==="
grep -F "com.miHoYo.Nap" $W/nap_maps_now.txt | grep -vE '/(lib|oat|dalvik)' | head -20
echo "=== mod libs now ==="
grep -E 'libChiy|libcymod|libxhook|libmain' $W/nap_maps_now.txt
echo "=== anonymous exec mappings (possible injected payload) ==="
grep -E 'r-xp 00000000 00:00 0' $W/nap_maps_now.txt | head -20
echo "=== memfd/ashmem/tmp mappings ==="
grep -E 'memfd|ashmem|/data/local/tmp|deleted' $W/nap_maps_now.txt | head -20
