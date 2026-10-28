#!/system/bin/sh
p=$(pidof com.zhuziplay.lvmengnew.op)
echo "pid=$p"
total=0; filtered=0
for t in /proc/$p/task/*; do
  total=$((total+1))
  s=$(grep -m1 '^Seccomp:' $t/status 2>/dev/null | tr -s ' ' | cut -d' ' -f2)
  c=$(cat $t/comm 2>/dev/null)
  if [ "$s" = "2" ]; then filtered=$((filtered+1)); fi
  echo "$s $c"
done
echo "TOTAL=$total FILTERED=$filtered"
