#!/usr/bin/env bash
# ============================================================================
#  il2cpp_win_snap.sh — 绿梦：libil2cpp rwxp 解密影副本「窗口时序快照」取证
#
#  目的：判定 0x3635xxx–0x364axxx 这段非法指令是「启动解密就坏」还是「运行期被写坏」，
#        并给出翻转时刻 + 翻转 offset 表（相对影副本基址）。
#  做法：冷启 → 轮询 pidof → 每 interval 秒用 memdump64(pread /proc/<pid>/mem，不 attach、
#        不改 TracerPid) 抓一次 [base+off, base+off+len) → 拉回 /tmp/lvwin/ →
#        python(capstone) 逐 4 字节判「能否解码成合法 ARM64 指令」。
#  用法：bash 04_scripts/绿梦/il2cpp_win_snap.sh [--dur 45] [--iv 2] [--off 0x3630000] [--len 0x20000]
#  退出码：0=已出结论；1=没抓到；2=用法/环境错
# ============================================================================
set -uo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.zhuziplay.lvmengnew.op"
ACT="com.bamboo.game.SplashActivity"
DUMPER="$NIXIANG/04_scripts/_common/memdump64"
OUT="/tmp/lvwin"
OFF=0x3630000
LEN=0x20000
DUR=45
IV=2

while [ $# -gt 0 ]; do
  case "$1" in
    --off) OFF="$2"; shift 2 ;;
    --len) LEN="$2"; shift 2 ;;
    --dur) DUR="$2"; shift 2 ;;
    --iv)  IV="$2";  shift 2 ;;
    --serial) SERIAL="$2"; shift 2 ;;
    -h|--help) sed -n '2,16p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

A() { adb -s "${SERIAL}" "$@"; }
S() { adb -s "${SERIAL}" shell "$@"; }

[ -f "${DUMPER}" ] || { echo "缺 memdump64: ${DUMPER}" >&2; exit 2; }
rm -rf "${OUT}"; mkdir -p "${OUT}"
A push "${DUMPER}" /data/local/tmp/memdump64 >/dev/null 2>&1 || { echo "push memdump64 失败" >&2; exit 2; }
S "chmod 755 /data/local/tmp/memdump64" >/dev/null 2>&1

S am force-stop "${PKG}" >/dev/null 2>&1
A logcat -b all -c >/dev/null 2>&1
A logcat -b main -v time > "${OUT}/main.log" 2>&1 &
LOGCAT_PID=$!

S am start -W -n "${PKG}/${ACT}" > "${OUT}/launch.txt" 2>&1
T0=$(date +%s)
echo "[snap] launch t0=${T0} off=${OFF} len=${LEN} dur=${DUR}s iv=${IV}s -> ${OUT}"

BASE=""
while : ; do
  NOW=$(date +%s); T=$(( NOW - T0 ))
  [ "${T}" -ge "${DUR}" ] && { echo "[snap] 到 ${DUR}s 结束"; break; }
  PID=$(S pidof "${PKG}" 2>/dev/null | tr -d '\r' | awk '{print $1}')
  if [ -z "${PID}" ]; then echo "[snap] t=${T}s 进程已不在（死亡时刻约 ${T}s）"; echo "${T}" > "${OUT}/death_at.txt"; break; fi
  if [ -z "${BASE}" ]; then
    LINE=$(S "su -c \"grep libil2cpp.so /proc/${PID}/maps | grep rwxp | head -1\"" 2>/dev/null | tr -d '\r')
    if [ -n "${LINE}" ]; then
      BASE="0x$(echo "${LINE}" | cut -d- -f1)"
      echo "[snap] pid=${PID} il2cpp rwxp base=${BASE}   $(echo "${LINE}" | awk '{print $1, $2, $6}')"
      echo "${PID} ${BASE}" > "${OUT}/proc.txt"
    fi
  fi
  if [ -n "${BASE}" ]; then
    START=$(python3 -c "print(hex(${BASE} + ${OFF}))")
    END=$(python3 -c "print(hex(${BASE} + ${OFF} + ${LEN}))")
    S "su -c '/data/local/tmp/memdump64 ${PID} ${START} ${END} /data/local/tmp/lvw_${T}.bin'" >/dev/null 2>&1
    echo "[snap] t=${T}s dumped ${START}..${END}"
  fi
  sleep "${IV}"
done

kill "${LOGCAT_PID}" 2>/dev/null || true
for f in $(S "ls /data/local/tmp/lvw_*.bin" 2>/dev/null | tr -d '\r'); do
  A pull "$f" "${OUT}/" >/dev/null 2>&1 || true
  S "su -c 'rm -f $f'" >/dev/null 2>&1 || true
done
echo "[snap] 拉回 $(ls "${OUT}"/lvw_*.bin 2>/dev/null | wc -l | tr -d ' ') 拍"
grep -a 'get signal' "${OUT}/main.log" 2>/dev/null | head -3

python3 - "${OUT}" <<'PY'
import glob, os, re, sys
import capstone
d = sys.argv[1]
files = sorted(glob.glob(os.path.join(d, 'lvw_*.bin')),
               key=lambda p: int(re.search(r'lvw_(\d+)', p).group(1)))
if not files:
    print('NO SNAPSHOTS'); sys.exit(1)
md = capstone.Cs(capstone.CS_ARCH_ARM64, capstone.CS_MODE_ARM)
def bad_offsets(buf):
    bad = []
    for i in range(0, len(buf) - 3, 4):
        w = int.from_bytes(buf[i:i+4], 'little')
        ok = False
        for _ in md.disasm(w.to_bytes(4, 'little'), 0):
            ok = True; break
        if not ok:
            bad.append(i)
    return bad
prev = None
for p in files:
    t = int(re.search(r'lvw_(\d+)', p).group(1))
    buf = open(p, 'rb').read()
    bad = bad_offsets(buf)
    zero = sum(1 for i in range(0, len(buf)-3, 4) if buf[i:i+4] == b'\x00\x00\x00\x00')
    line = 't=%3ds size=%6d bad=%5d zero=%5d' % (t, len(buf), len(bad), zero)
    if bad:
        line += '  bad_head=%s' % ','.join('0x%x' % o for o in bad[:6])
    if prev is not None:
        pb, pt = prev
        if pb == buf:
            line += '  [与上拍相同]'
        else:
            diff = [i for i in range(min(len(pb), len(buf))) if pb[i] != buf[i]]
            line += '  [较 t=%ds 变 %d 字节, 首个 offset=0x%x]' % (pt, len(diff), diff[0] if diff else -1)
    print(line)
    prev = (buf, t)
PY
