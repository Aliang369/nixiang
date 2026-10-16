#!/usr/bin/env bash
# 生存33天 — 热更后取「运行时明文 JS」冻结抓取器（frida 在本机不可用时用这条）
#
# 原理：mod 的 hooked_seEval 只在自己的 splice 期间存在原始明文缓冲；
#   命中游戏侧 `big eval` 日志的瞬间 `kill -STOP` 冻结进程，就能把
#   原始 31MB 明文（以及 mod 的 src/patched 两份拷贝）留在内存里，
#   再从 /proc/<pid>/mem 按 maps 逐区捞、本地 grep 定位，最后 CONTINUE。
#
# 2026-10-17 实测：命中区 108MB 内同一段明文 3 份、按 32MB 对齐；
#   从区偏移 0 起 31206123 字节即整份 index.jsc 明文。
#
# 用法:
#   bash 04_scripts/生存33天/freeze_grab_eval_js.sh \
#       --pkg com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive \
#       --out /tmp/sc33_eval_main.js \
#       [--activity <pkg>/<Activity>] [--trigger 'big eval len=.*combat=1'] \
#       [--needle 'prototype.FightChangeEntityHp'] [--min-mb 16] [--timeout 120]
#
# 产出：命中区整块 + stdout 打印「区起始 / 命中偏移 / 建议切片长度」。
#       明文长度可用 `[Sc33] big eval len=` 日志核对（2026-10-17 = 31206123）。
set -uo pipefail

PKG=""
OUT=""
ACTIVITY=""
TRIGGER='big eval len=.*combat=1'
NEEDLE='prototype.FightChangeEntityHp'
MIN_MB=16
TIMEOUT=120
while [[ $# -gt 0 ]]; do
  case "$1" in
    --pkg) PKG="${2:?}"; shift 2 ;;
    --out) OUT="${2:?}"; shift 2 ;;
    --activity) ACTIVITY="${2:?}"; shift 2 ;;
    --trigger) TRIGGER="${2:?}"; shift 2 ;;
    --needle) NEEDLE="${2:?}"; shift 2 ;;
    --min-mb) MIN_MB="${2:?}"; shift 2 ;;
    --timeout) TIMEOUT="${2:?}"; shift 2 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done
[[ -n "$PKG" && -n "$OUT" ]] || { echo "用法见文件头（--pkg/--out 必填）" >&2; exit 2; }

WORK="$(mktemp -d /tmp/sc33_freeze.XXXXXX)"
PID=""
cleanup() {
  if [[ -n "$PID" ]]; then
    adb shell "su -c 'kill -CONT $PID'" >/dev/null 2>&1 || true
  fi
  rm -rf "$WORK"
}
trap cleanup EXIT

echo "[1] 冷启游戏"
adb logcat -c >/dev/null 2>&1 || true
adb shell am force-stop "$PKG"
sleep 1
if [[ -n "$ACTIVITY" ]]; then
  adb shell am start -n "$ACTIVITY" >/dev/null 2>&1
else
  adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
fi

echo "[2] 等触发日志: $TRIGGER"
( adb logcat 2>/dev/null | grep --line-buffered -m1 -aE "$TRIGGER" > "$WORK/trig.txt" ) &
W=$!
for _ in $(seq 1 $((TIMEOUT * 4))); do
  [[ -s "$WORK/trig.txt" ]] && break
  sleep 0.25
done
kill "$W" 2>/dev/null
[[ -s "$WORK/trig.txt" ]] || { echo "ERROR: ${TIMEOUT}s 内没等到触发日志" >&2; exit 1; }
echo "    $(cat "$WORK/trig.txt")"

PID="$(adb shell pidof "$PKG" | tr -d '\r')"
[[ -n "$PID" ]] || { echo "ERROR: 拿不到 pid" >&2; exit 1; }
echo "[3] SIGSTOP pid=${PID}（此刻原始明文缓冲一定在内存）"
adb shell "su -c 'kill -STOP $PID'"

adb shell "su -c 'cat /proc/$PID/maps'" > "$WORK/maps.txt" 2>/dev/null
echo "[4] 扫 rw-p 匿名区（>= ${MIN_MB}MB）找 '$NEEDLE'"
python3.11 - "$WORK/maps.txt" "$MIN_MB" > "$WORK/regions.txt" <<'PY'
import sys
rows = []
for ln in open(sys.argv[1]):
    parts = ln.strip().split(None, 5)
    if len(parts) < 5:
        continue
    s_e, perm = parts[0], parts[1]
    path = parts[5] if len(parts) > 5 else ''
    if perm != 'rw-p' or path.startswith('/'):
        continue
    try:
        a, b = s_e.split('-')
        s, e = int(a, 16), int(b, 16)
    except ValueError:
        continue
    if e - s < int(sys.argv[2]) * 1024 * 1024:
        continue
    rows.append((e - s, s))
rows.sort(reverse=True)
for sz, s in rows:
    print(s // 4096, (sz + 4095) // 4096, sz, hex(s))
PY
cat "$WORK/regions.txt"

FOUND=""
while read -r sk ct sz hx; do
  echo "    dump $hx ($((sz / 1048576))MB) ..."
  adb exec-out "su -c 'dd if=/proc/$PID/mem bs=4096 skip=$sk count=$ct 2>/dev/null'" > "$WORK/reg_$hx.bin"
  if grep -a -q "$NEEDLE" "$WORK/reg_$hx.bin"; then
    FOUND="$WORK/reg_$hx.bin"
    break
  fi
  rm -f "$WORK/reg_$hx.bin"
done < "$WORK/regions.txt"

if [[ -z "$FOUND" ]]; then
  echo "ERROR: 在 >=${MIN_MB}MB 的 rw-p 匿名区里没找到 '$NEEDLE'（可加大 --timeout / 调小 --min-mb）" >&2
  exit 1
fi

cp -f "$FOUND" "$OUT"
echo "[5] 命中区已落盘: $OUT ($(stat -f%z "$OUT") B)"
python3.11 - "$OUT" "$NEEDLE" <<'PY'
import sys
d = open(sys.argv[1], 'rb').read()
off = d.find(sys.argv[2].encode())
print("    区起始=文件偏移 0；命中 '%s' @%d" % (sys.argv[2], off))
print("    参考：2026-10-17 轮明文 = 31206123 B，命中点 @9202005（同区 3 份按 32MB 对齐）")
print("    切片：若该区偏移 0 处即明文起点，取 [0:%d] 即整份 index.jsc 明文" % 31206123)
PY
echo "[6] kill -CONT $PID"
adb shell "su -c 'kill -CONT $PID'" || true
PID=""
echo "DONE"
