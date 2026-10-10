#!/usr/bin/env bash
# ================================================================
#  classify_proc_gone.sh — 把「进程消失了」定性：ANR / native 崩溃 / 系统杀 / 我方 force-stop / 无痕
#
#  背景（2026-10-10 明日方舟复盘 P2）：
#    飞行记录仪 `events.log` 里的 `PROC_GONE pid=N` 只说明进程没了，不说明为什么。
#    本会话第一轮把 ANR（`Input dispatching timed out` → 系统杀）读成「游戏闪退」，据此误判了修复方向。
#    铁律：进程消失必须回查原文再定性 ——
#      `ANR in <pkg>` / `Input dispatching timed out`            → ANR 被杀（**不是** native 崩溃）
#      `Fatal signal` / `tombstone` / `signal 6|7|11`           → native 崩溃（通常有 tombstone）
#      `Killing <pid>:<proc> (adj N): <reason>`                 → 系统/用户请求杀（reason 是关键）
#      `Force stopping <pkg>` / `am_kill` / `am force-stop`     → 包管理器停止（装机/人工）
#      `Death received, pid = <pid>` / `(pid <pid>) has died`   → 有据的终止（无崩溃证据）
#    注意：`su -c am force-stop` **不会**留下 `Force stopping` 行（那条只在 PM 侧操作时出现），
#    只有 `Death received` / `has died` / `libprocessgroup: Removed cgroup …pid_<pid>`。
#
#  用法:
#    bash 04_scripts/_common/classify_proc_gone.sh <recorder_dir | logcat.txt> \
#         [--pkg <包名>] [--window <秒，默认 8>] [--wide <秒，默认 30>] [--pid <只看某 pid>]
#
#  退出码: 0=无 ANR/崩溃证据（多为我方 force-stop 或正常终止）
#          1=发现 ANR 或 native 崩溃（**先按此定性，禁止笼统写「游戏闪退」**）
#          2=用法/文件错
#  bash 3.2 兼容（不用 mapfile/关联数组）
# ================================================================
set -uo pipefail

TARGET="${1:-}"; shift 2>/dev/null || true
PKG=""; WINDOW=8; WIDE=30; ONLY_PID=""
while [ $# -gt 0 ]; do
  case "$1" in
    --pkg)    PKG="${2:-}"; shift 2 ;;
    --window) WINDOW="${2:-8}"; shift 2 ;;
    --wide)   WIDE="${2:-30}"; shift 2 ;;
    --pid)    ONLY_PID="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,27p' "$0"; exit 0 ;;
    *) echo "✗ 未知参数: $1" >&2; exit 2 ;;
  esac
done
[ -n "$TARGET" ] || { echo "用法: classify_proc_gone.sh <recorder_dir|logcat.txt> [--pkg P] [--window N] [--pid N]" >&2; exit 2; }

if [ -d "$TARGET" ]; then
  DIR="$TARGET"; LOG="$TARGET/logcat.txt"; EV="$TARGET/events.log"
elif [ -f "$TARGET" ]; then
  LOG="$TARGET"; DIR="$(dirname "$TARGET")"; EV="$DIR/events.log"
else
  echo "✗ 找不到 $TARGET" >&2; exit 2
fi
[ -f "$LOG" ] || { echo "✗ 找不到 logcat: $LOG" >&2; exit 2; }
[ -f "$EV" ]  || { echo "✗ 找不到 events.log: ${EV}（本脚本靠它的 PROC_GONE 时间点定位窗口）" >&2; exit 2; }

case "$WINDOW" in ''|*[!0-9]*) WINDOW=8 ;; esac
case "$WIDE"   in ''|*[!0-9]*) WIDE=30 ;; esac
[ -n "$PKG" ] || PKG="$(LC_ALL=C awk -F'pkg=' '/REC_START/ { split($2,a," "); print a[1]; exit }' "$EV" 2>/dev/null || true)"

echo "== 进程消失定性 =="
echo "  记录仪: $DIR"
echo "  包     : ${PKG:-未知}"
echo "  窗口   : ±${WINDOW}s（无痕时放宽到 ±${WIDE}s）"
echo

# 时间窗（logcat 行首 `MM-DD HH:MM:SS.mmm`；events.log 是 `YYYY-MM-DD HH:MM:SS`）
# 跨日滚动交 python3，避免 bash 3.2 的日期算术坑。
win_of() {  # win_of <ts> <sec> → 两行 MM-DD HH:MM:SS
  python3 - "$1" "$2" <<'PY'
import datetime, sys
t = datetime.datetime.strptime(sys.argv[1].strip(), "%Y-%m-%d %H:%M:%S")
w = int(sys.argv[2])
print((t - datetime.timedelta(seconds=w)).strftime("%m-%d %H:%M:%S"))
print((t + datetime.timedelta(seconds=w)).strftime("%m-%d %H:%M:%S"))
PY
}
seg_of() {  # seg_of <wstart> <wend> → 该窗口内的 logcat 行
  LC_ALL=C awk -v s="$1" -v e="$2" '{ ts = substr($0, 1, 17); if (ts >= s && ts <= e) print }' "$LOG" 2>/dev/null
}
cnt() {     # cnt <seg> <egrep 模式>
  printf '%s\n' "$1" | LC_ALL=C grep -acE "$2" 2>/dev/null || true
}
pick() {    # pick <seg> <egrep 模式> <行数>
  printf '%s\n' "$1" | LC_ALL=C grep -aE "$2" 2>/dev/null | head -"$3"
}

CRASH=0; EVENTS=0

while IFS='|' read -r EPOCH TS REST; do
  case "$REST" in PROC_GONE*) ;; *) continue ;; esac
  PID="$(printf '%s' "$REST" | sed -n 's/.*pid=\([0-9]\{1,\}\).*/\1/p')"
  [ -n "$PID" ] || continue
  if [ -n "$ONLY_PID" ] && [ "$PID" != "$ONLY_PID" ]; then continue; fi
  EVENTS=$((EVENTS + 1))

  W="$(win_of "$TS" "$WINDOW")"; SEG="$(seg_of "$(printf '%s\n' "$W" | sed -n 1p)" "$(printf '%s\n' "$W" | sed -n 2p)")"
  VERDICT=""; WHY=""; WIDENED=""

  rank() {   # rank <seg> <是否放宽标记> → 设置 VERDICT/WHY（优先级从上到下）
    s="$1"; mark="$2"
    n="$(cnt "$s" 'ANR in |Input dispatching timed out')"
    case "$n" in ''|*[!0-9]*) n=0 ;; esac
    if [ "$n" -gt 0 ]; then
      VERDICT="ANR（输入分发超时 → 系统杀；**不是 native 崩溃**）${mark}"
      WHY="$(pick "$s" 'ANR in |Input dispatching timed out' 2)"; return 0
    fi
    n="$(cnt "$s" 'Fatal signal|tombstone|signal 6 \(Aborted\)|signal 11 \(SIGSEGV\)|signal 7 \(SIGBUS\)')"
    case "$n" in ''|*[!0-9]*) n=0 ;; esac
    if [ "$n" -gt 0 ]; then
      VERDICT="native 崩溃（有 tombstone / 致命信号）${mark}"
      WHY="$(pick "$s" 'Fatal signal|tombstone|signal 6 \(|signal 11 \(|signal 7 \(' 2)"; return 0
    fi
    n="$(cnt "$s" "Killing ${PID}:")"
    case "$n" in ''|*[!0-9]*) n=0 ;; esac
    if [ "$n" -gt 0 ]; then
      VERDICT="系统/用户请求杀（Killing ${PID}: … reason 见下）${mark}"
      WHY="$(pick "$s" "Killing ${PID}:" 2)"; return 0
    fi
    n="$(cnt "$s" 'Force stopping|am_kill|am force-stop')"
    case "$n" in ''|*[!0-9]*) n=0 ;; esac
    if [ "$n" -gt 0 ]; then
      VERDICT="包管理器/我方主动停止（Force stopping / am_kill）${mark}"
      WHY="$(pick "$s" 'Force stopping|am_kill' 2)"; return 0
    fi
    n="$(cnt "$s" "Death received, pid = ${PID}|\(pid ${PID}\) has died|pid_${PID}[^0-9]|Process ${PID} exited")"
    case "$n" in ''|*[!0-9]*) n=0 ;; esac
    if [ "$n" -gt 0 ]; then
      VERDICT="正常终止/被停止（有 pid 匹配的死亡行，无 ANR 与崩溃证据）${mark}"
      WHY="$(pick "$s" "Death received, pid = ${PID}|\(pid ${PID}\) has died|pid_${PID}[^0-9]|Process ${PID} exited" 2)"; return 0
    fi
    return 1
  }

  rank "$SEG" "" || {   # 紧窗口无痕 → 放宽窗口再判（force-stop 常在几秒前）
    W2="$(win_of "$TS" "$WIDE")"
    SEG2="$(seg_of "$(printf '%s\n' "$W2" | sed -n 1p)" "$(printf '%s\n' "$W2" | sed -n 2p)")"
    if rank "$SEG2" "（放宽到 ±${WIDE}s 后判定）" ; then WIDENED="1"; else
      VERDICT="窗口内无任何死亡/崩溃/ANR 证据（±${WIDE}s 也无）"
    fi
  }

  case "$VERDICT" in *ANR*|*native\ 崩溃*|*"user request after error"*) CRASH=1 ;; esac

  echo "── ${TS}  pid=${PID}"
  echo "   判定: ${VERDICT}"
  [ -n "$WHY" ] && printf '%s\n' "$WHY" | sed 's/^/        /'
done < "$EV"

echo
echo "────────────────────────────────"
if [ "$EVENTS" -eq 0 ]; then
  echo "（events.log 里没有 PROC_GONE 记录）"
  exit 0
fi
if [ "$CRASH" -gt 0 ]; then
  echo "❌ ${EVENTS} 次进程消失中有 ANR 或 native 崩溃：**按上面定性写结论，禁止笼统写「游戏闪退」**。"
  exit 1
fi
echo "✅ ${EVENTS} 次进程消失，均无 ANR/崩溃证据（多为装机/force-stop 主动停止或正常终止）。"
exit 0
