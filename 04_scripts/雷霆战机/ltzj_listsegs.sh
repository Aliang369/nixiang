#!/system/bin/sh
PID=$(pidof com.a5game.ltzj.jj)
echo PID=$PID > /data/local/tmp/lc_pid.txt
rm -f /data/local/tmp/lc_segs.txt
grep -E "rw-p" /proc/$PID/maps | while read line; do
  R=$(echo "$line" | cut -d" " -f1)
  A=$(echo $R | cut -d- -f1)
  B=$(echo $R | cut -d- -f2)
  SZ=$(( 0x$B - 0x$A ))
  if [ $SZ -le 0x4000000 ] && [ $SZ -gt 0x1000 ]; then
    echo "$A $B $line" >> /data/local/tmp/lc_segs.txt
  fi
done
wc -l < /data/local/tmp/lc_segs.txt
