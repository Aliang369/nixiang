#!/usr/bin/env bash
# ================================================================
#  gate_note.sh — 门禁即时记账（P7）
#
#  背景（2026-10-10 邂逅在迷宫会话复盘 P7）：
#  本会话门禁结论（G1 静态 / G2 机器 / G3 人验）全部只活在对话里，收尾时
#  才靠回忆补写成表 —— 于是"某门禁到底过没过、证据是哪一行日志"反复被重问，
#  还会把"未验证"当成"已通过"。对策：**过一门禁当轮记一行**，只追加不改写。
#
#  用法:
#    bash 04_scripts/_common/gate_note.sh <游戏> <门禁> <结论> [证据] [--by ai|human] [--show]
#  示例:
#    bash 04_scripts/_common/gate_note.sh 邂逅在迷宫 G2 通过 "gradle_cycle 出包+apksigner verify 通过"
#    bash 04_scripts/_common/gate_note.sh 邂逅在迷宫 G3 通过 "PASS_TO_HUMAN 空转3分钟无崩溃 09_test/.../precheck_20261010_044339"
#    bash 04_scripts/_common/gate_note.sh 邂逅在迷宫 G1 未验证 "隔离空目录重建未做"
#    bash 04_scripts/_common/gate_note.sh 邂逅在迷宫 --show
#
#  门禁命名: G1=静态(L1 人读代码/产物) G2=机器(L2 实机/脚本) G3=人验(L3 用户实测最终)
#            也允许自由文本（如 LV1/装机/收尾）
#  落点: 10_docs/<游戏>/门禁台账.tsv（TSV，一游戏一份，只追加）
#  退出码: 0=已记账；2=用法错
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

GAME=""; GATE=""; RESULT=""; EVIDENCE=""; BY="ai"; SHOW=0
positional=0

while [ $# -gt 0 ]; do
  case "$1" in
    --by)   BY="$2"; shift 2 ;;
    --show) SHOW=1; shift ;;
    -h|--help) sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    -*) echo "未知参数: $1" >&2; exit 2 ;;
    *)
      positional=$((positional+1))
      case "$positional" in
        1) GAME="$1" ;;
        2) GATE="$1" ;;
        3) RESULT="$1" ;;
        4) EVIDENCE="$1" ;;
        *) ;;
      esac
      shift ;;
  esac
done

[ -n "$GAME" ] || { sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'; exit 2; }
LEDGER="$NIXIANG_ROOT/10_docs/$GAME/门禁台账.tsv"
mkdir -p "$(dirname "$LEDGER")"

if [ "$SHOW" = "1" ] || { [ -z "$GATE" ] && [ -f "$LEDGER" ]; }; then
  echo "== $GAME 门禁台账（${LEDGER}）=="
  column -t -s "$(printf '\t')" "$LEDGER" 2>/dev/null | sed 's/^/  /' || cat "$LEDGER"
  exit 0
fi

[ -n "$GATE" ]   || { echo "✗ 缺门禁名（G1/G2/G3 或自由文本）" >&2; exit 2; }
[ -n "$RESULT" ] || { echo "✗ 缺结论（通过/失败/未验证）" >&2; exit 2; }

if [ ! -f "$LEDGER" ]; then
  printf '时间\t游戏\t门禁\t结论\t证据\t记入者\n' > "$LEDGER"
fi

TS="$(date '+%Y-%m-%d %H:%M')"
# 证据里的制表符/换行会破坏 TSV，替换成空格
EVIDENCE="$(printf '%s' "$EVIDENCE" | tr '\t\n' '  ')"
printf '%s\t%s\t%s\t%s\t%s\t%s\n' "$TS" "$GAME" "$GATE" "$RESULT" "$EVIDENCE" "$BY" >> "$LEDGER"
echo "✓ 已记账：$GAME  $GATE=$RESULT  ($TS, by $BY)"
echo "  → $LEDGER"
exit 0
