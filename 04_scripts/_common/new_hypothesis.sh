#!/usr/bin/env bash
# ================================================================
#  new_hypothesis.sh — 「假设预注册」脚手架（改动前先写下「怎么算对」）
#
#  为什么要有（2026-10-14 潮汐守望者会话复盘 P0-5）:
#    本轮 e1–e9 共约 7 轮是「先改代码、装包、请用户打一把、看日志」，
#    每轮都没有事先写清「这一轮判什么、什么算否证」⇒ 日志到手后仍要临时决定
#    「这算不算有效果」，同一现象（`layout: ok=0`）被判读两次、两次结论不同。
#    已有经验卡 `10_docs/_templates/经验_功能不对先Trace证实挂点偏移.md` 讲的是同一课，
#    但它是**叙述**，没有落盘位置和字段 ⇒ 这里把它做成「动手前必须落一个文件」。
#
#  用法:
#    bash 04_scripts/_common/new_hypothesis.sh --game 潮汐守望者 [--id cxswz] \
#         [--round e8] [--hypothesis "HPChangeComponent 在非引擎程序集里能按短名解析到"] \
#         [--verdict "class HPChangeComponent.*-> 0x"] \
#         [--reject "抓不到 short index 行，或 ok=0 连续 2 轮同因"] \
#         [--evidence "09_logs/…grep 命中"] [--out DIR]
#    缺省字段留 TODO 占位（脚本不猜）；编号自动取 09_test/<game>/hypothesis_*.md 的下一个
#
#  产出: 09_test/<game>/hypothesis_<n>.md （字段: 假设 / 判定行 / 否决条件 / 证据 / 结论=未验证）
#  退出码: 0=已创建；2=用法错；3=同名文件已存在（并发）
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"

GAME=""; ID=""; ROUND=""; HYP=""; VERDICT=""; REJECT=""; EVID=""; OUT=""
while [ $# -gt 0 ]; do
  case "$1" in
    --game) GAME="${2:-}"; shift 2 ;;
    --id) ID="${2:-}"; shift 2 ;;
    --round) ROUND="${2:-}"; shift 2 ;;
    --hypothesis) HYP="${2:-}"; shift 2 ;;
    --verdict) VERDICT="${2:-}"; shift 2 ;;
    --reject) REJECT="${2:-}"; shift 2 ;;
    --evidence) EVID="${2:-}"; shift 2 ;;
    --out) OUT="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,28p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

[ -n "$GAME" ] || { echo "[hypo] 必须给 --game" >&2; exit 2; }
[ -n "$OUT" ] || OUT="$ROOT/09_test/$GAME"
mkdir -p "$OUT"

# 下一个可用编号（bash 3.2：无 mapfile/无关联数组）
N=1
while [ -e "$OUT/hypothesis_$N.md" ]; do N=$((N + 1)); done
FILE="$OUT/hypothesis_$N.md"
[ -e "$FILE" ] && { echo "[hypo] 已存在: $FILE" >&2; exit 3; }

put() { [ -n "$2" ] && printf '%s' "$2" || printf 'TODO'; }

{
  echo "# 假设 $N${ROUND:+ · 轮次 $ROUND}${GAME:+ · $GAME}"
  echo
  echo "- 日期: $(date '+%Y-%m-%d %H:%M')"
  echo "- 游戏/标识: $GAME${ID:+ ($ID)}${ROUND:+ · $ROUND}"
  echo "- 状态: **未验证**"
  echo
  echo "## 假设（动手前写死，改动后再看它是否成立）"
  echo
  printf '%s\n' "$(put H "$HYP")"
  echo
  echo "## 判定行（日志/命令里必须出现的可 grep 字符串）"
  echo
  echo '```'
  printf '%s\n' "$(put V "$VERDICT")"
  echo '```'
  echo
  echo "## 否决条件（出现这些就判否，不许事后改口径）"
  echo
  printf '%s\n' "$(put R "$REJECT")"
  echo
  echo "## 证据落点（跑完后回填）"
  echo
  printf '%s\n' "$(put E "$EVID")"
  echo
  echo "## 结论（跑完后回填：成立 / 否证 / 无效轮次及原因）"
  echo
  echo "TODO"
  echo
  echo "---"
  echo "关联: \`10_docs/_templates/经验_功能不对先Trace证实挂点偏移.md\`（先取证再改代码）"
} >"$FILE"

echo "[hypo] 已创建 $FILE"
echo "[hypo] 下一步: 先补齐 TODO 字段，再动代码；跑完回填「结论」并 grep 判定行:"
echo "[hypo]   grep -E '<判定行>' 09_logs/<游戏>/<日志>"
exit 0
