#!/system/bin/sh
# P0-1 A/B 取证：libcocos 末字节已翻转状态下冷启，抓信号/openat/线程堆栈
T=/sys/kernel/tracing
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

# 1) tracing 复位 + kprobe + 事件
echo 0 > $T/events/enable
echo 0 > $T/tracing_on
echo > $T/trace
echo 'p:ltzjopen do_sys_openat2 fname=+0(%x1):ustring' > $T/kprobe_events 2>/dev/null && echo KPROBE_SET
echo 'fname ~ *ltzj*' > $T/events/kprobes/ltzjopen/filter 2>/dev/null
echo 1 > $T/events/kprobes/ltzjopen/enable 2>/dev/null
echo '(target_comm ~ *ltzj*) || (comm ~ *ltzj*)' > $T/events/signal/signal_generate/filter 2>/dev/null && echo GEN_FILTER_OK
echo '(target_comm ~ *ltzj*) || (comm ~ *ltzj*)' > $T/events/signal/signal_deliver/filter 2>/dev/null
echo 1 > $T/events/signal/signal_generate/enable
echo 1 > $T/events/signal/signal_deliver/enable
echo 1 > $T/tracing_on

# 2) 后台采集
cat $T/trace_pipe > /data/local/tmp/ltzj_p01_ab.log 2>/dev/null &
CAT=$!
logcat -b crash -b events -v time > /data/local/tmp/ltzj_p01_logcat.log 2>/dev/null &
LOG=$!

sleep 1
# 3) 冷启 + 第9秒 kill -3
monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
sleep 3
PID=$(pidof com.a5game.ltzj.jj)
PIDCORE=$(ps -A | grep 'ltzj.jj:core' | awk '{print $2}')
echo "PID=$PID CORE=$PIDCORE"
( sleep 6; kill -3 $PID 2>/dev/null; echo "kill3 sent at $(date +%s)" ) &

# 4) 窗口 75s（覆盖 ~10-15s 预期死亡窗）
sleep 75
echo 0 > $T/tracing_on
kill $CAT $LOG 2>/dev/null

echo "--- 进程状态（结束后）---"
ps -A | grep ltzj
echo "--- trace 行数 ---"
wc -l /data/local/tmp/ltzj_p01_ab.log
echo "--- 装机 libcocos 末字节 ---"
SZ=$(stat -c %s $LIB)
dd if=$LIB bs=1 skip=$((SZ-1)) count=1 2>/dev/null | od -An -tu1
