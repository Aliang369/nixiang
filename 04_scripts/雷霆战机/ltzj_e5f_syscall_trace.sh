#!/system/bin/sh
# E5-F：raw_syscalls 抓 kill 族(93/94/129/130/131)全系统调用 → 定位组杀通道与发送线程
T=/sys/kernel/tracing
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so

restore() {
  cp /data/local/tmp/libcocos_backup.so $LIB 2>/dev/null
  echo "restore_md5=$(md5sum $LIB | cut -d' ' -f1)"
}

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

# 0) 清残留 probe/实例状态
for p in sigq tsq tgk kil tk4 exg; do
  echo 0 > $T/events/kprobes/$p/enable 2>/dev/null
done
sleep 1
for p in sigq tsq tgk kil tk4 exg; do
  echo "-:$p" > $T/kprobe_events 2>/dev/null
done

# 1) 翻字节
SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo flipped

# 2) ftrace：kill 族 syscall + 信号事件
echo 0 > $T/tracing_on
echo > $T/trace
echo 'id == 93 || id == 94 || id == 129 || id == 130 || id == 131' > $T/events/raw_syscalls/sys_enter/filter 2>/dev/null && echo FILTER_OK
echo 0 > $T/events/enable
echo 1 > $T/events/raw_syscalls/sys_enter/enable && echo SYS_ENTER_OK
echo 1 > $T/events/signal/signal_generate/enable
echo 1 > $T/events/signal/signal_deliver/enable
echo 32768 > $T/buffer_size_kb
echo 1 > $T/tracing_on

cat $T/trace_pipe > /data/local/tmp/ltzj_e5f.log 2>/dev/null &
CAT=$!

# 3) 冷启
monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
sleep 1
echo "PID=$(pidof com.a5game.ltzj.jj)"

# 4) 45s 窗口
sleep 45
echo 0 > $T/tracing_on
kill $CAT 2>/dev/null
echo "final=$(pidof com.a5game.ltzj.jj)"

restore
echo "--- kill 族事件 ---"
grep -E "sys_enter: .*nr=(93|94|129|130|131)" /data/local/tmp/ltzj_e5f.log | head -30
echo "--- 行数 ---"
wc -l /data/local/tmp/ltzj_e5f.log
