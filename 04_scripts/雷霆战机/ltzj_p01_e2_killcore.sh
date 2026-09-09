#!/system/bin/sh
# E2：验证 :core 是否为执行者 —— 翻字节冷启后第2秒杀 :core，观察主进程死活与 :core 重生
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo "flipped=$(dd if=$LIB bs=1 skip=$((SZ-1)) count=1 2>/dev/null | od -An -tu1 | tr -d ' ')"

monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
sleep 1
PID=$(pidof com.a5game.ltzj.jj)
CORE=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}')
echo "PID=$PID CORE=$CORE t=$(date +%s)"

# +2s 杀 :core；+6s 再杀一次（防重生后行刑）
sleep 2
CORE2=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}')
kill -9 $CORE2 2>/dev/null && echo "core killed#1 pid=$CORE2 t=$(date +%s)"
sleep 4
CORE3=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}')
[ -n "$CORE3" ] && kill -9 $CORE3 2>/dev/null && echo "core killed#2 pid=$CORE3 t=$(date +%s)"
echo "core_alive_now=$(ps -A | grep -c 'ltzj.jj:core')"

# 观察 60s（覆盖原 ~10s 行刑点）
for i in 1 2 3 4 5 6; do
  sleep 10
  echo "t+$(($i*10+6))s main_alive=$(pidof com.a5game.ltzj.jj) core_alive=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}' | tr '\n' ',')"
done

echo "--- 恢复 ---"
cp /data/local/tmp/libcocos_backup.so $LIB
md5sum $LIB
