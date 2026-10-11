#!/usr/bin/env bash
# ================================================================
#  code_locate.sh — 定行读文件的省事入口（AGENTS.md P18 读文件纪律的配套工具）
#
#  背景（2026-10-10 邂逅在迷宫第二轮复盘 P18）：
#  同一个 2xx 行的源码文件在一个会话里被**整文件读 4 次**，每次几十 K token，
#  压缩后还各留一个摘要块 —— 白烧上下文，结论却仍靠回忆。
#  纪律是「>200 行禁止整读」，但纪律要能被执行，就得让**正确路径比整读更省事**：
#  本脚本一条命令给出「命中行 + 上下文 + 可直接复制的定行读参数」。
#
#  用法:
#    bash 04_scripts/_common/code_locate.sh <文件> '<正则>' [--ctx N] [--max M] [--files]
#    --ctx N   上下文行数（默认 2）
#    --max M   最多打印多少行（默认 60，防一次刷屏）
#    --files   把 <文件> 当目录，先列出含该正则的文件（等价 grep -rln）
#
#  示例:
#    bash 04_scripts/_common/code_locate.sh \
#      'mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/menu/net/FakeCdn.java' \
#      'isCdnDest|OVERRIDES' --ctx 3
#
#  退出码: 0=有命中；1=无命中/文件不存在；2=用法错
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

usage() {
  sed -n '2,24p' "$0" | sed 's/^# \{0,1\}//'
  exit 2
}

TARGET=""; PAT=""; CTX=2; MAX=60; FILES=0
while [ $# -gt 0 ]; do
  case "$1" in
    --ctx)   CTX="${2:-2}"; shift 2 ;;
    --max)   MAX="${2:-60}"; shift 2 ;;
    --files) FILES=1; shift ;;
    -h|--help) usage ;;
    -*) echo "未知参数: $1" >&2; usage ;;
    *)
      if [ -z "$TARGET" ]; then TARGET="$1"; elif [ -z "$PAT" ]; then PAT="$1"; fi
      shift ;;
  esac
done
[ -n "$TARGET" ] && [ -n "$PAT" ] || usage

cd "$NIXIANG_ROOT" 2>/dev/null || true

if [ "$FILES" = "1" ]; then
  [ -d "$TARGET" ] || { echo "✗ 不是目录: $TARGET" >&2; exit 1; }
  LIST="$(grep -rlE --binary-files=without-match "$PAT" "$TARGET" 2>/dev/null || true)"
  if [ -z "$LIST" ]; then echo "✗ 无命中：$TARGET  ← 「${PAT}」"; exit 1; fi
  echo "== 含「${PAT}」的文件 =="
  printf '%s\n' "$LIST" | sed 's/^/  /'
  exit 0
fi

[ -f "$TARGET" ] || { echo "✗ 文件不存在: $TARGET" >&2; exit 1; }
TOTAL_LINES="$(wc -l < "$TARGET" | tr -d ' ')"
HITS="$(grep -nE "$PAT" "$TARGET" 2>/dev/null | cut -d: -f1 | tr '\n' ' ')"
if [ -z "${HITS// /}" ]; then
  echo "✗ 无命中：$TARGET  ← 「${PAT}」"
  echo "  （该文件共 ${TOTAL_LINES} 行；换个正则，或确认文件名/大小写）"
  exit 1
fi

HIT_N=$(printf '%s\n' $HITS | grep -c . )
echo "== ${TARGET}（共 ${TOTAL_LINES} 行，命中 ${HIT_N} 处）=="
printed=0; prev=0; first_shown=""; shown_lines=0
for n in $HITS; do
  start=$((n - CTX)); [ "$start" -lt 1 ] && start=1
  end=$((n + CTX))
  [ "$start" -le "$prev" ] && start=$((prev + 1))
  [ "$start" -gt "$end" ] && continue
  if [ "$printed" -ge "$MAX" ]; then
    echo "  …（已达 --max ${MAX}，省略后续；用 --ctx/--max 调整）"
    break
  fi
  awk -v s="$start" -v e="$end" -v fn="$TARGET" -v hs="$HITS" '
    NR>=s && NR<=e {
      mark=" ";
      if (index(" " hs " ", " " NR " ") > 0) mark="*";
      printf "%s %s:%d\t%s\n", mark, fn, NR, $0
    }' "$TARGET"
  shown_lines=$((shown_lines + end - start + 1))
  printed=$((printed + end - start + 1))
  prev=$end
  [ -z "$first_shown" ] && first_shown=$start
done

echo
echo "→ 定行读（不要整文件读）：read file_path=\"${TARGET}\" offset=${first_shown} limit=${shown_lines}"
echo "→ 读完把这行结论写进 10_docs/<游戏>/侦察笔记.md（文件:行 → 结论），下次查笔记不重读"
echo "→ 改这里时的纪律：同轮同文件只用一种工具（read 拿读戳后只用 edit；要用 bash 重写就整轮都别用 edit）"
echo "  ↳ 混用的代价：本仓库实测 29 次 \`file changed since it was read\`（skill/re-implement/hook-rules.md §32）"
exit 0
