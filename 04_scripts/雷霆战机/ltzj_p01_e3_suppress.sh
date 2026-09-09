#!/system/bin/sh
# E3：持续压制 :core 60s，观察主进程死活（ftrace 信号同步观测）
T=/sys/kernel/tracing
I=$T/instances/ltzj_p01
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo "flipped"

# ftrace 实例（信号事件）
mkdir -p $I 2>/dev/null
echo 0 > $I/tracing_on; echo > $I/trace
echo 1 > $I/events/signal/signal_generate/enable
echo 1 > $I/events/signal/signal_deliver/enable
echo 1 > $I/tracing_on
cat $I/trace_pipe > /data/local/tmp/ltzj_p01_e3.log 2>/dev/null &
CAT=$!
logcat -b events -b crash -v time > /data/local/tmp/ltzj_p01_e3_lc.log 2>/dev/null &

monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
sleep 1
PID=$(pidof com.a5game.ltzj.jj)
echo "PID=$PID"

# 持续压制 :core 60s（每秒检查并杀）
( i=0
  while [ $i -lt 60 ]; do
    C=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}')
    if [ -n "$C" ]; then
      kill -9 $C 2>/dev/null && echo "killed core $C at $(date +%s)"
    fi
    M=$(pidof com.a5game.ltzj.jj)
    if [ -z "$M" ]; then
      echo "MAIN DEAD at $(date +%s)"
      break
    fi
    sleep 1
    i=$((i+1))
  done ) &
SUP=$!

sleep 65
kill $SUP $CAT 2>/dev/null
echo 0 > $I/tracing_on
echo "final main=$(pidof com.a5game.ltzj.jj) core=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}')"

cp /data/local/tmp/libcocos_backup.so $LIB
md5sum $LIB | cut -d' ' -f1
