#!/usr/bin/env bash
# ================================================================
#  crash_forensics.sh — 崩溃取证一条命令（真机闪退 / SIGABRT / SIGSEGV 后立刻跑）
#
#  为什么要有（2026-10-14 潮汐守望者会话复盘 P0-2）:
#    那一轮为了定位「启动即崩」「打怪闪退」绕了 3 次死路：
#      · `adb shell cmd dropbox --print`  → 该子命令不存在
#      · `adb shell dumpsys dropbox --print` → 只有 Java 层 data_app_crash，没有 native tombstone
#      · `/data/tombstones/tombstone_00` 是 `-rw-------` → 非 root 读不到
#    最后能读的落点是 **App 自己写的** `/sdcard/Android/media/<pkg>/files/crash_logs/crash_*.txt`，
#    以及 `dumpsys activity exit-info <pkg>`（ApplicationExitInfo，免 root，含 reason/subreason/时间）。
#    这三条知识此前散在会话里，这次固化成一条命令。
#
#  用法:
#    bash 04_scripts/_common/crash_forensics.sh --pkg com.gf.cxswz [--serial 96f0d883]
#         [--game 潮汐守望者] [--out DIR] [--main-tail-lines 20000]
#    --game   落盘目录用 09_test/<game>/crash_<时间戳>/（缺省用包名）
#    --out    显式指定输出目录
#
#  产出（$OUT/）: crash_logs/ 最近 5 份 App 崩溃报告 · logcat_crash.txt · logcat_signals.txt
#                 dropbox.txt · exit_info.txt · tombstones.txt · SUMMARY.txt
#  退出码: 0=找到崩溃证据；1=未找到；2=用法错/无设备
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"

SERIAL=""; PKG=""; GAME=""; OUT=""; MAIN_LINES=20000
while [ $# -gt 0 ]; do
  case "$1" in
    --serial) SERIAL="${2:-}"; shift 2 ;;
    --pkg) PKG="${2:-}"; shift 2 ;;
    --game) GAME="${2:-}"; shift 2 ;;
    --out) OUT="${2:-}"; shift 2 ;;
    --main-tail-lines) MAIN_LINES="${2:-20000}"; shift 2 ;;
    -h|--help) sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

[ -n "$PKG" ] || { echo "[crash] 必须给 --pkg <包名>" >&2; exit 2; }
command -v adb >/dev/null 2>&1 || { echo "[crash] 无 adb" >&2; exit 2; }
if [ -z "$SERIAL" ]; then
  SERIAL="$(adb devices | awk 'NR>1 && $2=="device"{print $1; exit}')"
fi
[ -n "$SERIAL" ] || { echo "[crash] 无在线设备" >&2; exit 2; }

TS="$(date +%Y%m%d_%H%M%S)"
if [ -z "$OUT" ]; then
  NAME="${GAME:-$PKG}"
  OUT="$ROOT/09_test/$NAME/crash_$TS"
fi
mkdir -p "$OUT/crash_logs"

# LC_ALL=C: macOS 的 tr 在 UTF-8 locale 下碰到非法字节会打印 "Illegal byte sequence"
sh() { adb -s "$SERIAL" shell "$@" 2>/dev/null | LC_ALL=C tr -d '\r'; }

# 崩溃关键字（与 post_install_precheck.sh:108 的 RE_CRASH 对齐并加 native 形态）
NEEDLES='FATAL EXCEPTION|Fatal signal|SIGSEGV|SIGABRT|tombstone|signal 6|signal 11|exited due to signal|has died|ANR in|beginning of crash|java\.lang\.'

echo "[crash] serial=$SERIAL pkg=$PKG out=$OUT"

# ---------- 1. App 自带崩溃报告（免 root，最可靠）----------
DEV_DIR="/sdcard/Android/media/$PKG/files/crash_logs"
ls_out="$(sh ls -t "$DEV_DIR" 2>/dev/null | head -5)"
if [ -n "$ls_out" ]; then
  n=0
  for f in $ls_out; do
    n=$((n + 1))
    adb -s "$SERIAL" pull "$DEV_DIR/$f" "$OUT/crash_logs/$f" >/dev/null 2>&1 || true
  done
  echo "[crash] App crash_logs: 拉取 $n 份 ($DEV_DIR)"
else
  echo "[crash] App crash_logs: 无（${DEV_DIR}）"
fi

# ---------- 2. ApplicationExitInfo（免 root，含 reason/subreason）----------
sh dumpsys activity exit-info "$PKG" >"$OUT/exit_info.txt" 2>/dev/null || true

# ---------- 3. logcat crash 缓冲区 ----------
adb -s "$SERIAL" logcat -b crash -d -t 3000 2>/dev/null | LC_ALL=C tr -d '\r' >"$OUT/logcat_crash.txt" || true

# ---------- 4. 主缓冲区里按关键字切片 ----------
adb -s "$SERIAL" logcat -d -t "$MAIN_LINES" 2>/dev/null | LC_ALL=C tr -d '\r' | grep -E "$NEEDLES" >"$OUT/logcat_signals.txt" 2>/dev/null || true

# ---------- 5. dropbox（注意：只有 Java 层 data_app_crash，native 走 tombstone）----------
sh dumpsys dropbox --print 2>/dev/null | grep -E 'data_app_crash|Package:|Process:|Cmdline:|signal [0-9]+|Cause:|^====' | tail -120 >"$OUT/dropbox.txt" 2>/dev/null || true

# ---------- 6. tombstone 可读性（非 root 通常读不到，记录事实即可）----------
{
  echo "## ls -l /data/tombstones/ (as shell)"
  sh ls -l /data/tombstones/ 2>&1 | head -10
  echo
  echo "## ls -l /data/tombstones/ (as root, if available)"
  sh su -c 'ls -l /data/tombstones/' 2>&1 | head -10
} >"$OUT/tombstones.txt" 2>/dev/null || true

# ---------- 7. 统计 ----------
count_in() { # $1=文件 $2=ERE
  [ -f "$1" ] || { echo 0; return; }
  grep -cE "$2" "$1" 2>/dev/null || true
}
C_SIG=$(count_in "$OUT/logcat_signals.txt" 'Fatal signal|signal 6|signal 11|SIGSEGV|SIGABRT')
C_FATAL=$(count_in "$OUT/logcat_signals.txt" 'FATAL EXCEPTION|beginning of crash')
# `has died` 在主缓冲区里也出现在正常回收（非崩溃）⇒ 只作线索，不当定论；
# 真正定论看 fatal_signal / FATAL EXCEPTION / App 崩溃报告 / dropbox 四类。
C_DIED=$(count_in "$OUT/logcat_signals.txt" 'has died|exited due to signal')
C_DIED_PKG=$(count_in "$OUT/logcat_signals.txt" "($PKG.*has died)|(has died.*$PKG)")
C_CRASHBUF=$(count_in "$OUT/logcat_crash.txt" '.')
C_DROPBOX=$(count_in "$OUT/dropbox.txt" 'data_app_crash')
C_APPRPT=$(ls "$OUT/crash_logs" 2>/dev/null | wc -l | tr -d ' ')
EXIT_REASON="$(grep -m3 -E 'reason=|Reason:|description=' "$OUT/exit_info.txt" 2>/dev/null | tr '\n' ' ')"

SUMM=""
line() { SUMM="${SUMM}$1
"; }
line "[crash] serial=$SERIAL pkg=$PKG ts=$TS"
line "[crash] app_crash_logs_pulled=$C_APPRPT"
line "[crash] logcat_signals: fatal_signal=$C_SIG fatal_exception=$C_FATAL died(line)=⚠$C_DIED"
line "[crash] logcat_signals: died_of_$PKG=$C_DIED_PKG (线索，非定论)"
line "[crash] logcat_crash_buffer_lines=$C_CRASHBUF"
line "[crash] dropbox_data_app_crash=$C_DROPBOX"
line "[crash] exit_info: ${EXIT_REASON:-（空）}"
line "[crash] 证据目录=$OUT"

printf '%s' "$SUMM" | tee "$OUT/SUMMARY.txt"

DECISIVE=$((C_SIG + C_FATAL + C_CRASHBUF + C_APPRPT + C_DROPBOX))
if [ "$DECISIVE" -gt 0 ]; then
  echo "[crash] ⇒ 找到崩溃证据"
  echo "[crash] 下一步: bash 04_scripts/_common/evidence.sh --source $OUT/logcat_signals.txt --needle 'Fatal signal|FATAL EXCEPTION|signal 11'"
  echo "[crash]          bash 04_scripts/_common/rootcause_note.sh --game <游戏> --feature <功能> --round <轮次> --cause <根因> --evidence <上面这些行> --verify <验证方式>"
  exit 0
fi
if [ "$C_DIED_PKG" -gt 0 ]; then
  echo "[crash] ⇒ 未找到**定论性**崩溃证据，但有 $C_DIED_PKG 条「$PKG has died」线索（看 logcat_signals.txt）"
  echo "[crash]    若那是闪退，通常同一窗口里还有 signal 11/6 或 SIGSEGV ⇒ 用更早的 logcat 重跑，或看 App 自身 crash_logs"
  exit 1
fi
echo "[crash] ⇒ 未找到崩溃证据（可能：设备已重启/缓冲区被冲掉/App 未写报告；重放场景后再跑）"
exit 1
