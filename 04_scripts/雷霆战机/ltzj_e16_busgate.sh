#!/system/bin/sh
# E16：总线断闸 —— tersafe2+0xB1980（消息发送+重试）入口 → mov w0,#0; ret
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so
TGT_OFF=727424
EXP=ffc303d1fb4b00f9
PATCH=/data/local/tmp/patch_e15.bin

restore() {
  cp /data/local/tmp/libcocos_backup.so $LIB 2>/dev/null
  echo "restore_md5=$(md5sum $LIB | cut -d' ' -f1)"
}

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo "flipped=$(dd if=$LIB bs=1 skip=$((SZ-1)) count=1 2>/dev/null | od -An -tu1 | tr -d ' ')"

printf '\000\000\200\322\300\003\137\326' > $PATCH
od -An -tx1 $PATCH

logcat -c 2>/dev/null
logcat -b events -b crash -v time > /data/local/tmp/ltzj_e15_lc.log 2>/dev/null &
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

LINE=$(grep -m1 "r-xp.*libtersafe2.so" /proc/$PID/maps 2>/dev/null)
BASE_HEX=$(echo "$LINE" | sed 's/-.*//')
[ -z "$BASE_HEX" ] && { echo ABORT_NO_BASE; kill $LOGPID 2>/dev/null; restore; exit 1; }
echo "base=$BASE_HEX polls=$i"

BASE_DEC=$(awk -v h="$BASE_HEX" 'BEGIN{n=0; for(i=1;i<=length(h);i++){c=tolower(substr(h,i,1)); d=index("0123456789abcdef",c)-1; n=n*16+d} printf "%.0f", n}')
TGT_DEC=$(awk -v b="$BASE_DEC" -v o=727424 'BEGIN{printf "%.0f", b+o}')

CUR=$(dd if=/proc/$PID/mem bs=1 skip=$TGT_DEC count=8 2>/dev/null | od -An -tx1 | tr -d ' \n')
echo "cur=$CUR"
if [ "$CUR" != "$EXP" ]; then echo ABORT_GUARD_MISMATCH; kill $LOGPID 2>/dev/null; restore; exit 1; fi

dd if=$PATCH of=/proc/$PID/mem bs=1 seek=$TGT_DEC conv=notrunc 2>/dev/null && echo PATCHED
AFTER=$(dd if=/proc/$PID/mem bs=1 skip=$TGT_DEC count=8 2>/dev/null | od -An -tx1 | tr -d ' \n')
echo "after=$AFTER (期望 000080d2c0035fd6)"

n=0
while [ $n -lt 14 ]; do
  sleep 5
  n=$((n+1))
  M=$(pidof com.a5game.ltzj.jj)
  C=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}' | tr '\n' ',')
  echo "t+$((n*5))s main=${M:-DEAD} core=${C:-none}"
  [ -z "$M" ] && break
done
echo "VERDICT: $(pidof com.a5game.ltzj.jj >/dev/null 2>&1 && echo SURVIVED_70s || echo DIED)"
kill $LOGPID 2>/dev/null
grep -E "am_proc_died|am_kill" /data/local/tmp/ltzj_e15_lc.log | grep ltzj | head -4
restore
