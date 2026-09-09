#!/system/bin/sh
# P0-1 A/B 第二轮：独立 tracing 实例 + do_filp_open kprobe，抓「谁读被改的 libcocos」
T=/sys/kernel/tracing
I=$T/instances/ltzj_p01
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

# 0) 翻转末字节（上轮已恢复）
SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo "flipped: $(dd if=$LIB bs=1 skip=$((SZ-1)) count=1 2>/dev/null | od -An -tu1)"

# 1) 独立实例
mkdir -p $I 2>/dev/null
echo 0 > $I/tracing_on
echo > $I/trace
echo 4096 > $I/buffer_size_kb

# 2) kprobe（全局注册一次；重复注册先删）
grep -q ltzjopen $T/kprobe_events 2>/dev/null && echo '-:ltzjopen' > $T/kprobe_events 2>/dev/null
echo 'p:ltzjopen do_filp_open fname=+0(+0(%x1)):string' > $T/kprobe_events && echo KPROBE_OK
echo 'fname ~ *ltzj*' > $I/events/kprobes/ltzjopen/filter 2>/dev/null && echo KFILTER_OK
echo 1 > $I/events/kprobes/ltzjopen/enable && echo KENABLE_OK

# 3) 信号事件（本实例干净）
echo 1 > $I/events/signal/signal_generate/enable
echo 1 > $I/events/signal/signal_deliver/enable
echo 1 > $I/tracing_on

# 4) 采集
cat $I/trace_pipe > /data/local/tmp/ltzj_p01_ab2.log 2>/dev/null &
CAT=$!
logcat -b events -b crash -v time > /data/local/tmp/ltzj_p01_logcat2.log 2>/dev/null &
LOG=$!

sleep 1
monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
sleep 3
echo "PID=$(pidof com.a5game.ltzj.jj)"

sleep 75
echo 0 > $I/tracing_on
kill $CAT $LOG 2>/dev/null

echo "--- 结束后进程 ---"
ps -A | grep ltzj || echo "全部已死"
echo "--- trace 行数 ---"
wc -l /data/local/tmp/ltzj_p01_ab2.log
