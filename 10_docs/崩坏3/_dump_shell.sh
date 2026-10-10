#!/system/bin/sh
# Dump libSDKloading + anon:.bss segments of PID (<=16MB each) for binding-table scan.
PID="$1"
D="$2"
[ -n "$PID" ] && [ -n "$D" ] || { echo "usage: dump_shell.sh PID DIR"; exit 2; }
rm -rf "$D"
mkdir -p "$D"
: > "$D/index.txt"
i=0
grep -E 'libSDKloading.so|anon:\.bss' /proc/$PID/maps | while read -r line; do
  range=$(echo "$line" | awk '{print $1}')
  perm=$(echo "$line" | awk '{print $2}')
  s=${range%-*}
  e=${range#*-}
  start=$(printf '%d' "0x$s")
  end=$(printf '%d' "0x$e")
  size=$((end - start))
  if [ "$size" -gt 16777216 ]; then
    echo "SKIP $range $perm size=$size" >> "$D/index.txt"
    continue
  fi
  i=$((i + 1))
  name="seg_${i}_${perm}_${s}.bin"
  dd if=/proc/$PID/mem of="$D/$name" bs=1048576 iflag=skip_bytes,count_bytes \
    skip=$start count=$size status=none 2>/dev/null
  echo "$name | $line" >> "$D/index.txt"
done
ls -la "$D"
chmod -R a+r "$D" 2>/dev/null
echo DONE
