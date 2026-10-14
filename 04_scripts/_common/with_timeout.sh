#!/usr/bin/env bash
# ================================================================
#  with_timeout.sh — 超时垫片（macOS 无 GNU `timeout`）
#
#  为什么要有（2026-10-14 潮汐守望者会话复盘 P0-3）:
#    macOS 自带没有 `timeout`（GNU coreutils 才有），`lint_shell_traps.py` R5 一直在
#    警告「别用 timeout」，但「那用什么」没有落点 —— 于是每轮都现编后台+轮询，
#    09_test 门禁还因此踩过一次「命令不存在」。
#
#  用法:
#    bash 04_scripts/_common/with_timeout.sh <秒> <命令…>
#    bash 04_scripts/_common/with_timeout.sh 30 adb -s 96f0d883 shell pm list packages
#
#  语义: 到期后先 TERM 再 KILL **直接子进程**（不级联子进程树，见下「已知边界」）。
#  退出码: 0..123 = 命令自身的退出码；124 = 超时被杀；125 = 用法错
#
#  已知边界: 只杀直接子进程；`with_timeout 5 sh -c 'sleep 100 & wait'` 这类孙子进程
#           可能残留（macOS 无进程组杀一的便携写法）。需要级联时让被包命令自己收尾。
# ================================================================
set -uo pipefail

if [ "$#" -lt 2 ]; then
  sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'
  exit 125
fi

SECS="$1"; shift
case "$SECS" in
  ''|*[!0-9]*) echo "[with_timeout] 秒数必须是正整数: $SECS" >&2; exit 125 ;;
esac
[ "$SECS" -gt 0 ] || { echo "[with_timeout] 秒数必须 > 0" >&2; exit 125; }

"$@" &
pid=$!

# 轮询间隔 0.1 s ⇒ 最多 SECS*10 次（bash 3.2 兼容，不用 SECONDS 内建以外的花招）
max=$((SECS * 10))
n=0
while kill -0 "$pid" 2>/dev/null; do
  if [ "$n" -ge "$max" ]; then
    echo "[with_timeout] 超时 ${SECS}s ⇒ 终止 pid=${pid}: $*" >&2
    # 收尾期间静音 stderr：bash 会为被信号杀死的后台任务打印 "Terminated: 15" 噪声，
    # 那是宿主 shell 的作业通知，不是被包命令的输出，混进日志会干扰 grep。
    exec 3>&2
    exec 2>/dev/null
    kill -TERM "$pid" 2>/dev/null || true
    sleep 1
    kill -KILL "$pid" 2>/dev/null || true
    wait "$pid" 2>/dev/null || true
    exec 2>&3
    exec 3>&-
    exit 124
  fi
  sleep 0.1
  n=$((n + 1))
done

wait "$pid"
rc=$?
exit "$rc"
