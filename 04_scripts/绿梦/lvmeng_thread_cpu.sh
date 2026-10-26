#!/usr/bin/env bash
# ============================================================================
#  lvmeng_thread_cpu.sh — 绿梦：免注入的「线程 CPU 归因」采样
#  为什么有它：Phase 1 证明 il2cpp 影副本在 t≈12–17 s 有一轮滚动变换；静态镜像里
#  搜不到 0xcb8 立即数 ⇒ 想知道「谁在变换」，先看**哪个线程**在那几秒烧 CPU
#  （零注入：只读 /proc/<pid>/task/<tid>/{comm,stat}，不 attach、不改 TracerPid）。
#  用法：bash 04_scripts/绿梦/lvmeng_thread_cpu.sh [--dur 22] [--iv 0.3] [--t0 6]
# ============================================================================
set -uo pipefail
SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.zhuziplay.lvmengnew.op"
ACT="com.bamboo.game.SplashActivity"
OUT="/tmp/lvcpu"
DUR=22; IV=0.3
while [ $# -gt 0 ]; do
  case "$1" in
    --dur) DUR="$2"; shift 2 ;;
    --iv)  IV="$2"; shift 2 ;;
    --serial) SERIAL="$2"; shift 2 ;;
    -h|--help) sed -n '2,12p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done
A() { adb -s "${SERIAL}" "$@"; }
S() { adb -s "${SERIAL}" shell "$@"; }
rm -rf "${OUT}"; mkdir -p "${OUT}"
S am force-stop "${PKG}" >/dev/null 2>&1
S am start -W -n "${PKG}/${ACT}" > "${OUT}/launch.txt" 2>&1
T0=$(date +%s)
echo "[cpu] t0=${T0}"
while : ; do
  NOW=$(date +%s); T=$(( NOW - T0 ))
  [ "${T}" -ge "${DUR}" ] && break
  PID=$(S pidof "${PKG}" 2>/dev/null | tr -d '\r' | awk '{print $1}')
  [ -z "${PID}" ] && { echo "[cpu] t=${T}s 进程已死"; break; }
  S "su -c 'for d in /proc/${PID}/task/*; do t=\$(basename \$d); c=\$(cat \$d/comm 2>/dev/null); s=\$(sed \"s/.*) //\" \$d/stat 2>/dev/null | awk \"{print \\\$12, \\\$13}\"); echo \$t \$c \$s; done'" 2>/dev/null | tr -d '\r' > "${OUT}/t_${T}.txt"
  sleep "${IV}"
done
echo "[cpu] 采样 $(ls "${OUT}"/t_*.txt 2>/dev/null | wc -l | tr -d ' ') 拍 -> ${OUT}"
python3 - "${OUT}" <<'PY'
import glob, os, re, sys
d = sys.argv[1]
fs = sorted(glob.glob(os.path.join(d, 't_*.txt')), key=lambda p: int(re.search(r't_(\d+)', p).group(1)))
series = []
for p in fs:
    t = int(re.search(r't_(\d+)', p).group(1))
    snap = {}
    for ln in open(p, errors='replace'):
        parts = ln.split()
        if len(parts) < 4: continue
        tid, comm, u, s = parts[0], parts[1], parts[2], parts[3]
        try: snap[tid] = (comm, int(u) + int(s))
        except ValueError: pass
    series.append((t, snap))
if len(series) < 2:
    print('样本不足'); sys.exit(1)
acc = {}
for i in range(1, len(series)):
    t0, a = series[i-1]; t1, b = series[i]
    for tid, (comm, v) in b.items():
        if tid in a:
            dv = v - a[tid][1]
            if dv > 0:
                acc.setdefault(tid, [comm, 0])
                acc[tid][1] += dv
print('窗口 %ds..%ds 内 CPU 增量 top10（100 tick/s）:' % (series[0][0], series[-1][0]))
for tid, (comm, dv) in sorted(acc.items(), key=lambda kv: -kv[1][1])[:10]:
    print('  tid=%-7s %-22s +%4d ticks (~%d.%02ds)' % (tid, comm, dv, dv//100, dv%100))
PY
