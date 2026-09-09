#!/system/bin/sh
# E4：kprobe 信号类 syscall，抓 ~10s 自杀的 raise 现行
T=/sys/kernel/tracing
I=$T/instances/ltzj_p01
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo flipped

# kprobes（先清旧的）
for p in sigq tsq tgk kil exg tk4; do echo "-:$p" > $T/kprobe_events 2>/dev/null; done
echo 'p:sigq __arm64_sys_rt_sigqueueinfo tgid=%x0 sig=%x1 code=+8(%x2):s32 addr=+16(%x2):x64' > $T/kprobe_events && echo P1
echo 'p:tsq __arm64_sys_rt_tgsigqueueinfo tgid=%x0 tid=%x1 sig=%x2 code=+8(%x3):s32' > $T/kprobe_events && echo P2
echo 'p:tgk __arm64_sys_tgkill tgid=%x0 tid=%x1 sig=%x2' > $T/kprobe_events && echo P3
echo 'p:kil __arm64_sys_kill pid=%x0 sig=%x1' > $T/kprobe_events && echo P4
echo 'p:tk4 __arm64_sys_tkill tid=%x0 sig=%x1' > $T/kprobe_events && echo P5
echo 'p:exg __arm64_sys_exit_group code=%x0' > $T/kprobe_events && echo P6

mkdir -p $I 2>/dev/null
echo 0 > $I/tracing_on; echo > $I/trace
echo 1 > $I/events/kprobes/sigq/enable
echo 1 > $I/events/kprobes/tsq/enable
echo 1 > $I/events/kprobes/tgk/enable
echo 1 > $I/events/kprobes/kil/enable
echo 1 > $I/events/kprobes/tk4/enable
echo 1 > $I/events/kprobes/exg/enable
echo 1 > $I/events/signal/signal_generate/enable
echo 1 > $I/events/signal/signal_deliver/enable
echo 1 > $I/tracing_on

cat $I/trace_pipe > /data/local/tmp/ltzj_p01_e4.log 2>/dev/null &
CAT=$!

monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
sleep 1
echo "PID=$(pidof com.a5game.ltzj.jj)"
sleep 50
echo 0 > $I/tracing_on
kill $CAT 2>/dev/null
echo "final main=$(pidof com.a5game.ltzj.jj)"
cp /data/local/tmp/libcocos_backup.so $LIB
md5sum $LIB | cut -d' ' -f1
