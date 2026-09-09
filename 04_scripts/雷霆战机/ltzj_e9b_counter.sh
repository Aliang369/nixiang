#!/system/bin/sh
# E8：中和 tersafe2 直连 read 包装器（0x315710 → mov w0,#0; ret）
# 假设：期望哈希与校验读取同源时，两边同时变空 → 判定通过
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so
TGT_OFF=917700           # 0xE00C4 计数上限 0x0a->0xfe
EXP=df2a007108018a1aa8da68f8fa270129
PATCH=/data/local/tmp/patch_e9.bin

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

printf '\337\372\003\161' > $PATCH

monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
i=0; PID=""
while [ $i -lt 100 ]; do
  PID=$(pidof com.a5game.ltzj.jj)
  [ -n "$PID" ] && break
  sleep 0.1; i=$((i+1))
done
[ -z "$PID" ] && { echo ABORT_NO_PID; restore; exit 1; }
echo "PID=$PID"

i=0; BASE_HEX=""
while [ $i -lt 200 ]; do
  LINE=$(grep -m1 "r-xp.*libtersafe2.so" /proc/$PID/maps 2>/dev/null)
  if [ -n "$LINE" ]; then BASE_HEX=$(echo "$LINE" | sed 's/-.*//'); break; fi
  sleep 0.05; i=$((i+1))
done
[ -z "$BASE_HEX" ] && { echo ABORT_NO_BASE; restore; exit 1; }
echo "base=$BASE_HEX polls=$i"

BASE_DEC=$(awk -v h="$BASE_HEX" 'BEGIN{n=0; for(i=1;i<=length(h);i++){c=tolower(substr(h,i,1)); d=index("0123456789abcdef",c)-1; n=n*16+d} printf "%.0f", n}')
TGT_DEC=$(awk -v b="$BASE_DEC" 'BEGIN{printf "%.0f", b+917700}')

CUR=$(dd if=/proc/$PID/mem bs=1 skip=$TGT_DEC count=16 2>/dev/null | od -An -tx1 | tr -d ' \n')
echo "cur=$CUR"
if [ "$CUR" != "$EXP" ]; then echo "ABORT_GUARD_MISMATCH"; restore; exit 1; fi

dd if=$PATCH of=/proc/$PID/mem bs=1 seek=$TGT_DEC conv=notrunc 2>/dev/null && echo PATCHED
AFTER=$(dd if=/proc/$PID/mem bs=1 skip=$TGT_DEC count=16 2>/dev/null | od -An -tx1 | tr -d ' \n')
echo "after=$AFTER"

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
restore
