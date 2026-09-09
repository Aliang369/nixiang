#!/system/bin/sh
# E13：出生即停 + 中和主哈希引擎 0x35AA60（verdict 锁存前置零）
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so
TGT_OFF=3517024
EXP=ff8307d1ef3b146ded33156deb2b166d
PATCH=/data/local/tmp/patch_e13.bin

restore() {
  cp /data/local/tmp/libcocos_backup.so $LIB 2>/dev/null
  echo "restore_md5=$(md5sum $LIB | cut -d' ' -f1)"
}
killby() {
  P=$(pidof com.a5game.ltzj.jj)
  [ -n "$P" ] && kill -STOP $P 2>/dev/null
}

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo "flipped"

printf '\000\000\200\322\300\003\137\326' > $PATCH

monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 &
# 出生即停：5ms 轮询
i=0; PID=""
while [ $i -lt 2000 ]; do
  PID=$(pidof com.a5game.ltzj.jj)
  if [ -n "$PID" ]; then
    kill -STOP $PID 2>/dev/null
    P2=$(pidof com.a5game.ltzj.jj)
    if [ -n "$P2" ]; then break; fi
  fi
  sleep 0.005
  i=$((i+1))
done
[ -z "$PID" ] && { echo ABORT_NO_PID; restore; exit 1; }
echo "PID=$PID stopped early"

LINE=$(grep -m1 "r-xp.*libtersafe2.so" /proc/$PID/maps 2>/dev/null)
BASE_HEX=$(echo "$LINE" | sed 's/-.*//')
[ -z "$BASE_HEX" ] && { echo ABORT_NO_BASE; kill -CONT $PID; restore; exit 1; }
echo "base=$BASE_HEX"
BASE_DEC=$(awk -v h="$BASE_HEX" 'BEGIN{n=0; for(i=1;i<=length(h);i++){c=tolower(substr(h,i,1)); d=index("0123456789abcdef",c)-1; n=n*16+d} printf "%.0f", n}')
TGT_DEC=$(awk -v b="$BASE_DEC" 'BEGIN{printf "%.0f", b+3517024}')
CUR=$(dd if=/proc/$PID/mem bs=1 skip=$TGT_DEC count=16 2>/dev/null | od -An -tx1 | tr -d ' \n')
echo "cur=$CUR"
if [ "$CUR" != "$EXP" ]; then echo ABORT_GUARD; kill -CONT $PID; restore; exit 1; fi
dd if=$PATCH of=/proc/$PID/mem bs=1 seek=$TGT_DEC conv=notrunc 2>/dev/null && echo PATCHED
AFTER=$(dd if=/proc/$PID/mem bs=1 skip=$TGT_DEC count=8 2>/dev/null | od -An -tx1 | tr -d ' \n')
echo "after=$AFTER"
kill -CONT $PID
echo "resumed at $(date +%s)"

n=0
while [ $n -lt 14 ]; do
  sleep 5
  n=$((n+1))
  M=$(pidof com.a5game.ltzj.jj)
  echo "t+$((n*5))s main=${M:-DEAD}"
  [ -z "$M" ] && break
done
echo "VERDICT: $(pidof com.a5game.ltzj.jj >/dev/null 2>&1 && echo SURVIVED_70s || echo DIED)"
restore
