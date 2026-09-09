#!/system/bin/sh
# E5：单变量内存中和 N1（tersafe2 簇1编排器 0x124940 → mov x0,#0; ret）
# 流程：翻 libcocos 末字节 → 冷启 → tersafe2 映射即 patch（版本守卫）→ 监控 60s
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so
TGT_OFF=1198400          # 0x124940
EXP=f403002ae0330091827f8052e1031f2a
PATCH=/data/local/tmp/patch_n1.bin

restore() {
  cp /data/local/tmp/libcocos_backup.so $LIB 2>/dev/null
  echo "restore_md5=$(md5sum $LIB | cut -d' ' -f1)"
}

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

# 1) 翻转末字节
SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo "flipped=$(dd if=$LIB bs=1 skip=$((SZ-1)) count=1 2>/dev/null | od -An -tu1 | tr -d ' ')"

# 2) 补丁文件：mov x0,#0 (d2800000) ; ret (d65f03c0)
printf '\000\000\200\322\300\003\137\326' > $PATCH
echo "patch=$(od -An -tx1 $PATCH | tr -d ' \n')"

# 3) 冷启
monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
i=0; PID=""
while [ $i -lt 100 ]; do
  PID=$(pidof com.a5game.ltzj.jj)
  [ -n "$PID" ] && break
  sleep 0.1; i=$((i+1))
done
[ -z "$PID" ] && { echo ABORT_NO_PID; restore; exit 1; }
echo "PID=$PID"

# 4) 等 tersafe2 映射（≤10s）
i=0; BASE_HEX=""
while [ $i -lt 200 ]; do
  LINE=$(grep -m1 "libtersafe2.so" /proc/$PID/maps 2>/dev/null)
  if [ -n "$LINE" ]; then
    BASE_HEX=$(echo "$LINE" | sed 's/-.*//')
    break
  fi
  sleep 0.05; i=$((i+1))
done
[ -z "$BASE_HEX" ] && { echo ABORT_NO_BASE; restore; exit 1; }
echo "base=$BASE_HEX polls=$i"

# 5) 地址换算（纯 awk，64 位精确）
BASE_DEC=$(awk -v h="$BASE_HEX" 'BEGIN{n=0; for(i=1;i<=length(h);i++){c=tolower(substr(h,i,1)); d=index("0123456789abcdef",c)-1; if(d<0){printf "-1"; exit} n=n*16+d} printf "%.0f", n}')
TGT_DEC=$(awk -v b="$BASE_DEC" 'BEGIN{printf "%.0f", b+1198400}')
echo "tgt_dec=$TGT_DEC"

# 6) 版本守卫：读 16 字节比对
CUR=$(dd if=/proc/$PID/mem bs=1 skip=$TGT_DEC count=16 2>/dev/null | od -An -tx1 | tr -d ' \n')
echo "cur=$CUR"
if [ "$CUR" != "$EXP" ]; then echo "ABORT_GUARD_MISMATCH"; restore; exit 1; fi

# 7) patch + 回读验证
dd if=$PATCH of=/proc/$PID/mem bs=1 seek=$TGT_DEC conv=notrunc 2>/dev/null && echo PATCHED
AFTER=$(dd if=/proc/$PID/mem bs=1 skip=$TGT_DEC count=8 2>/dev/null | od -An -tx1 | tr -d ' \n')
echo "after=$AFTER"

# 8) 监控 60s（行刑点 ~+9-10s）
START=$(date +%s)
n=0
while [ $n -lt 12 ]; do
  sleep 5
  n=$((n+1))
  M=$(pidof com.a5game.ltzj.jj)
  C=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}' | tr '\n' ',')
  echo "t+$((n*5))s main=${M:-DEAD} core=${C:-none}"
  [ -z "$M" ] && break
done
echo "VERDICT: $(pidof com.a5game.ltzj.jj >/dev/null 2>&1 && echo SURVIVED_60s || echo DIED)"

restore
