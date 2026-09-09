#!/system/bin/sh
# E14：忙等抢停（测时）+ 双引擎中和（master 0x35AA60 + cluster2 0x12C194）+ logcat
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so
P1=3517024        # 0x35AA60
E1=ff8307d1ef3b146ded33156deb2b166d
P2=1229204        # 0x12C194
E2=f50f1df8f44f01a9fd7b02a9fd830091
PATCH=/data/local/tmp/patch_e14.bin

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
logcat -b main -b events -v time > /data/local/tmp/ltzj_e14_lc.log 2>/dev/null &
LOGPID=$!

echo "T0=$(date +%s%N)"
monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 &
# 忙等抢停
PID=""
while :; do
  PID=$(pidof com.a5game.ltzj.jj)
  if [ -n "$PID" ]; then
    kill -STOP $PID 2>/dev/null
    P2C=$(pidof com.a5game.ltzj.jj)
    if [ -n "$P2C" ]; then break; fi
  fi
done
echo "T1=$(date +%s%N) PID=$PID"

LINE=$(grep -m1 "r-xp.*libtersafe2.so" /proc/$PID/maps 2>/dev/null)
BASE_HEX=$(echo "$LINE" | sed 's/-.*//')
BASE_DEC=$(awk -v h="$BASE_HEX" 'BEGIN{n=0; for(i=1;i<=length(h);i++){c=tolower(substr(h,i,1)); d=index("0123456789abcdef",c)-1; n=n*16+d} printf "%.0f", n}')

# patch1
T1D=$(awk -v b="$BASE_DEC" -v o=$P1 'BEGIN{printf "%.0f", b+o}')
C1=$(dd if=/proc/$PID/mem bs=1 skip=$T1D count=16 2>/dev/null | od -An -tx1 | tr -d ' \n')
if [ "$C1" = "$E1" ]; then
  dd if=$PATCH of=/proc/$PID/mem bs=1 seek=$T1D conv=notrunc 2>/dev/null && echo "P1 PATCHED"
else
  echo "P1 GUARD MISMATCH $C1"
fi
# patch2
T2D=$(awk -v b="$BASE_DEC" -v o=$P2 'BEGIN{printf "%.0f", b+o}')
C2=$(dd if=/proc/$PID/mem bs=1 skip=$T2D count=16 2>/dev/null | od -An -tx1 | tr -d ' \n')
if [ "$C2" = "$E2" ]; then
  dd if=$PATCH of=/proc/$PID/mem bs=1 seek=$T2D conv=notrunc 2>/dev/null && echo "P2 PATCHED"
else
  echo "P2 GUARD MISMATCH $C2"
fi

kill -CONT $PID
echo "T2=$(date +%s%N) resumed"

n=0
while [ $n -lt 14 ]; do
  sleep 5
  n=$((n+1))
  M=$(pidof com.a5game.ltzj.jj)
  echo "t+$((n*5))s main=${M:-DEAD}"
  [ -z "$M" ] && break
done
echo "VERDICT: $(pidof com.a5game.ltzj.jj >/dev/null 2>&1 && echo SURVIVED_70s || echo DIED)"
kill $LOGPID 2>/dev/null
grep -E "nativeloader.*tersafe2|AppActivity" /data/local/tmp/ltzj_e14_lc.log | head -4
restore
