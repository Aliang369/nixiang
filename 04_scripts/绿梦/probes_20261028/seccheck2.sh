#!/system/bin/sh
p=$(pidof com.zhuziplay.lvmengnew.op)
echo "pid=$p"
grep -E '^(Seccomp|Seccomp_filters|NoNewPrivs):' /proc/$p/status
echo "--- per-thread uniq Seccomp_filters ---"
for t in /proc/$p/task/*; do grep -h '^Seccomp_filters:' $t/status 2>/dev/null; done | sort | uniq -c
echo "--- uniq Seccomp mode ---"
for t in /proc/$p/task/*; do grep -h '^Seccomp:' $t/status 2>/dev/null; done | sort | uniq -c
