#!/system/bin/sh
# E17：守门人 —— 周期性清零投诉标志 [单例+0x72]，验证 janitor 概念
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so
GSLOT=5984904   # 0x5B5288 单例指针槽

restore() {
  cp /data/local/tmp/libcocos_backup.so $LIB 2>/dev/null
  echo "restore_md5=$(md5sum $LIB | cut -d' ' -f1)"
}

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1
SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo "flipped"

monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
i=0; PID=""
while [ $i -lt 100 ]; do
  PID=$(pidof com.a5game.ltzj.jj)
  [ -n "$PID" ] && break
  sleep 0.1; i=$((i+1))
done
[ -z "$PID" ] && { echo ABORT_NO_PID; restore; exit 1; }
echo "PID=$PID"
LINE=$(grep -m1 "r-xp.*libtersafe2.so" /proc/$PID/maps 2>/dev/null)
BASE_HEX=$(echo "$LINE" | sed 's/-.*//')
BASE_DEC=$(awk -v h="$BASE_HEX" 'BEGIN{n=0; for(i=1;i<=length(h);i++){c=tolower(substr(h,i,1)); d=index("0123456789abcdef",c)-1; n=n*16+d} printf "%.0f", n}')
GDEC=$(awk -v b="$BASE_DEC" 'BEGIN{printf "%.0f", b+5982344}')
printf '\000' > /data/local/tmp/zero1.bin

# 守门循环：+0.3s 起，每 0.4s 清一次 [单例+0x72]，共 40 次（~16s）
n=0
while [ $n -lt 40 ]; do
  PHEX=$(dd if=/proc/$PID/mem bs=1 skip=$GDEC count=8 2>/dev/null | od -An -tx1 | tr -d ' \n')
  if [ "$PHEX" != "0000000000000000" ] && [ -n "$PHEX" ]; then
PDEC=$(awk -v h="$PHEX" 'BEGIN{s=""; for(i=15;i>=1;i-=2){s=s substr(h,i,2)}; n=0; for(k=1;k<=length(s);k++){c=tolower(substr(s,k,1)); d=index("0123456789abcdef",c)-1; n=n*16+d}; printf "%.0f", n}')
    # awk 大端转小端64位：直接在 awk 内做
    T2=$(awk -v p="$PDEC" 'BEGIN{printf "%.0f", p+114}')
    dd if=/data/local/tmp/zero1.bin of=/proc/$PID/mem bs=1 seek=$T2 conv=notrunc 2>/dev/null
    echo "t+$n: flag cleared (obj=$PHEX)"
  fi
  M=$(pidof com.a5game.ltzj.jj)
  [ -z "$M" ] && { echo "MAIN DEAD at tick $n"; break; }
  sleep 0.4
  n=$((n+1))
done

sleep 45
echo "VERDICT: $(pidof com.a5game.ltzj.jj >/dev/null 2>&1 && echo SURVIVED || echo DIED)"
restore
