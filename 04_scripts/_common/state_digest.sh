#!/usr/bin/env bash
# ================================================================
#  state_digest.sh — 大状态文档「分片速读」摘要器
#
#  为什么要有这个脚本（2026-10-12 霓虹深渊2 会话复盘 P0-5）
#  ----------------------------------------------------------------
#  实测：那次会话把状态文档**整读**了 10 轮以上 —— 台账.md 10 次 / 76 KB、
#  games/ 摘要 9 次 / 34 KB、TOOLBOOK.md 10 次 / 29 KB、路线锁定.md 6 次 / 43 KB，
#  单 read 工具输出 1.12 MB 里 59% 花在同一个 .cpp、状态文档合计 ~500 KB。
#  这些文档都是 append-only（只追加不删行，见 10_docs/_templates/台账.md），
#  所以**不能靠删内容瘦身**；正确做法是「先看一行摘要，需要细节再按行号读段」。
#
#  本脚本把 10_docs/<游戏>/ 下的大文档压成一屏：
#    · 每个 §小节 → 行号区间
#    · 每个表格行 → 行号 + 首列（截断），带「归属/判据」线索
#  输出 ≤ MAXLINES 行（默认 120），可直接喂给 AI 当开工上下文。
#
#  用法
#  ----------------------------------------------------------------
#    bash 04_scripts/_common/state_digest.sh <游戏>              # 打到 stdout
#    bash 04_scripts/_common/state_digest.sh <游戏> --write      # 写 10_docs/<游戏>/_digest.md
#    bash 04_scripts/_common/state_digest.sh <游戏> --check      # 只看是否需要重生成（rc=1 需重生成）
#    MAXLINES=60 bash 04_scripts/_common/state_digest.sh <游戏>
#
#  读完之后怎么用（**这就是纪律**）：不要 `read 台账.md`（无 offset），改成
#    sed -n '206,240p' 10_docs/<游戏>/台账.md      # 只取需要的区间
#    bash 04_scripts/_common/code_locate.sh 10_docs/<游戏>/台账.md '闪退' --ctx 3
#
#  退出码：0 = 正常；1 = --check 判定「摘要缺失或过期」；2 = 用法错
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"

GAME=""; MODE=out
while [ $# -gt 0 ]; do
  case "$1" in
    --write) MODE=write; shift ;;
    --check) MODE=check; shift ;;
    -h|--help) sed -n '1,40p' "$0"; exit 0 ;;
    -*) echo "未知参数: $1" >&2; exit 2 ;;
    *)  GAME="$1"; shift ;;
  esac
done
[ -n "$GAME" ] || { echo "用法: bash 04_scripts/_common/state_digest.sh <游戏> [--write|--check]" >&2; exit 2; }

DOC="$ROOT/10_docs/$GAME"
[ -d "$DOC" ] || { echo "✗ 文档目录不存在: $DOC" >&2; exit 2; }
MAXLINES="${MAXLINES:-120}"
OUT="$DOC/_digest.md"

digest() {
  echo "# ${GAME} 状态速读摘要（state_digest.sh 生成，勿手改；需要原文按行号读段）"
  echo
  echo "> 纪律：**不要整读**下列原文。先在本摘要里定位行号，再 \`sed -n 'A,Bp'\` 或"
  echo "> \`bash 04_scripts/_common/code_locate.sh <文件> '<关键词>' --ctx 3\`。"
  echo
  local f bytes lines
  for f in "台账.md" "硬约束.md" "路线锁定.md" "current.md" "决策台账.md"; do
    [ -f "$DOC/$f" ] || continue
    bytes=$(wc -c < "$DOC/$f" | tr -d ' ')
    lines=$(wc -l < "$DOC/$f" | tr -d ' ')
    echo "## ${f}（${bytes} B / ${lines} 行）"
    awk -F'\t' '
      /^#{2,3}[^#]/ { printf "  :%-5d %s\n", NR, $0; next }
      /^\|/ && NR>2 {
        line=$0
        gsub(/^\|[[:space:]]*/,"",line)
        n=split(line,c,"|")
        key=c[1]; gsub(/[[:space:]]+$/,"",key)
        if (key=="") next
        if (key ~ /^-+$/) next
        if (length(key)>110) key=substr(key,1,108)"…"
        printf "  :%-5d | %s\n", NR, key
      }
    ' "$DOC/$f" | iconv -f UTF-8 -t UTF-8 -c   # -c：丢掉截断处的半个多字节字符，保证摘要可 grep
    echo
  done
}

D="$(digest)"

if [ "$MODE" = "check" ]; then
  if [ ! -f "$OUT" ]; then echo "⚠ 无摘要：${OUT}（开工前先 --write）"; exit 1; fi
  newest="$(ls -t "$DOC"/*.md 2>/dev/null | head -1)"
  if [ -n "$newest" ] && [ "$newest" -nt "$OUT" ]; then
    echo "⚠ 摘要过期：$newest 比 $OUT 新 ⇒ 重新生成"; exit 1
  fi
  echo "✓ 摘要新鲜：$OUT"; exit 0
fi

if [ "$MODE" = "write" ]; then
  printf '%s\n' "$D" > "$OUT"
  echo "▶ 已写 ${OUT}（$(printf '%s\n' "$D" | wc -l | tr -d ' ') 行）"
  exit 0
fi

printf '%s\n' "$D" | head -n "$MAXLINES"
TOT=$(printf '%s\n' "$D" | wc -l | tr -d ' ')
[ "$TOT" -gt "$MAXLINES" ] && echo "…（共 $TOT 行，已截到 ${MAXLINES}；MAXLINES=0 可看全）"
exit 0
