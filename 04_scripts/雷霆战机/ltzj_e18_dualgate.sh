#!/system/bin/sh
# E18：双闸 —— 总线 0xB1980 + 路由 0xB1C8C 同时断闸（+1.5s 应用）
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so
OFF1=727424      # 0xB1980 总线发送
EXP1=ffc303d1fb4b00f9
OFF2=728204      # 0xB1C8C 路由器
EXP2=ff8302d1fa6705a9
PATCH=/data/local/tmp/patch_e18.bin

restore() {
  cp /data/local/tmp/libcocos_backup.so $LIB 2>/dev/null
  echo "restore_md5=$(md5sum $LIB | cut -d' ' -f1)"
}
am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1
SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo flipped
printf '\000\000\200\322\300\003\137\326' > $PATCH

logcat -c 2>/dev/null
logcat -b events -v time > /data/local/tmp/ltzj_e18_lc.log 2>/dev/null &
LOGPID=$!

monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
i=0; PID=""
while [ $i -lt 100 ]; do
  PID=$(pidof com.a5game.ltzj.jj)
  [ -n "$PID" ] && break
  sleep 0.1; i=$((i+1))
done
[ -z "$PID" ] && { echo ABORT_NO_PID; kill $LOGPID 2>/dev/null; restore; exit 1; }
echo "PID=$PID"
sleep 1.5
LINE=$(grep -m1 "r-xp.*libtersafe2.so" /proc/$PID/maps 2>/dev/null)
BASE_HEX=$(echo "$LINE" | sed 's/-.*//')
if [ -z "$BASE_HEX" ]; then echo ABORT_NO_BASE; kill $LOGPID 2>/dev/null; restore; exit 1; fi
echo "base=$BASE_HEX"
BASE_DEC=$(awk -v h="$BASE_HEX" 'BEGIN{n=0; for(i=1;i<=length(h);i++){c=tolower(substr(h,i,1)); d=index("0123456789abcdef",c)-1; n=n*16+d} printf "%.0f", n}')

for PAIR in "$OFF1 $EXP1 gate1" "$OFF2 $EXP2 gate2"; do
  OFF=$(echo $PAIR | cut -d' ' -f1)
  EX=$(echo $PAIR | cut -d' ' -f2)
  NM=$(echo $PAIR | cut -d' ' -f3)
  TD=$(awk -v b="$BASE_DEC" -v o=$OFF 'BEGIN{printf "%.0f", b+o}')
  C=$(dd if=/proc/$PID/mem bs=1 skip=$TD count=8 2>/dev/null | od -An -tx1 | tr -d ' \n')
  if [ "$C" = "$EX" ]; then
    dd if=$PATCH of=/proc/$PID/mem bs=1 seek=$TD conv=notrunc 2>/dev/null && echo "$NM PATCHED"
  else
    echo "$NM MISMATCH cur=$C"
  fi
done

n=0
while [ $n -lt 16 ]; do
  sleep 5
  n=$((n+1))
  M=$(pidof com.a5game.ltzj.jj)
  echo "t+$((n*5))s main=${M:-DEAD}"
  [ -z "$M" ] && break
done
echo "VERDICT: $(pidof com.a5game.ltzj.jj >/dev/null 2>&1 && echo SURVIVED_80s || echo DIED)"
kill $LOGPID 2>/dev/null
restore
