#!/usr/bin/env bash
# ================================================================
#  check_cleanup_residue.sh — 七处目录「收尾零保留」机器检查
#
#  用法:
#    bash 04_scripts/_common/check_cleanup_residue.sh               # 查全部七处
#    bash 04_scripts/_common/check_cleanup_residue.sh --game 霓虹深渊
#
#  退出码: 0=无红线（可能有 WARN）；1=存在已交付游戏的残留
#
#  为什么需要（2026-11-27 实测漏项）:
#    `10_docs/tools/05`「收尾清理」写明七处零保留，检查者一栏写的是
#    「无机器检查，靠人工审查」。于是 11-26 霓虹深渊装机轮（不是收尾口令轮）
#    留下的 07_patched/霓虹深渊/surgical_3.2.860 与 09_test/霓虹深渊/precheck_*
#    一直躺到次日；终末地的 doc_prune 清单同理留在 09_test。规范变更日志
#    line 850 已记过同型事故（07_tmp 被旧脚本 mkdir 复活），当时结论就是
#    「无检查者 ⇒ 回归无人拦」。本脚本把那条人工清单变成可执行判据。
#
#  判据（唯一依据，不另立标准）:
#    * 七处 = 03_decompiled 05_ida 06_dumped 07_patched 09_logs 09_test 13_scratch
#    * 每处除 README.md / .DS_Store / _index.md 外的一级条目即「残留」
#    * 残留归属游戏 = 一级条目名；用 10_docs/_state/games/{游戏}.md 的
#      frontmatter `status` 判断是否已收尾：
#        delivered          → FAIL（已交付仍有产物，违反收尾零保留）
#        active / blocked   → WARN（在研，正常，收尾时才清）
#        无状态文件/未登记   → WARN（无法判定，需人工确认是否游戏目录）
#    * 一级条目是文件（根下散落）→ WARN
#
#  设计取舍:
#    * 只报不改：本脚本不动任何文件；清理仍走 trash_paths.sh（显式路径 + dry-run 复核）。
#    * 不把「在研」判红：在研游戏本来就有 03/06/09 内容，判红会让日常体检天天报错，
#      反而训练出「红灯可以忽略」的习惯。红线只留给人能立刻处理的情形。
#    * 不按 mtime 判活跃：mtime 会被 Finder/.DS_Store 触碰改写（2026-11-27 实测
#      07_patched 被浏览改成 19:14），状态文件 frontmatter 才是权威。
# ================================================================
set -uo pipefail

cd "$(dirname "$0")/../.." || exit 2

PLACES="03_decompiled 05_ida 06_dumped 07_patched 09_logs 09_test 13_scratch"
ONLY_GAME=""
if [ "${1:-}" = "--game" ]; then
  ONLY_GAME="${2:?--game 需要游戏名}"
fi

fail=0
warn=0
checked=0

status_of() {
  local f="10_docs/_state/games/$1.md"
  [ -f "$f" ] || { printf 'UNREGISTERED'; return 0; }
  awk 'NR==1 && $0!="---"{exit} /^---$/{n++; if(n==2) exit; next} n==1 && /^status:/{sub(/^status:[[:space:]]*/,""); gsub(/"/,""); print; exit}' "$f"
}

printf '════ 七处零保留检查 ════\n'
for d in $PLACES; do
  [ -d "$d" ] || continue
  while IFS= read -r entry; do
    [ -n "$entry" ] || continue
    base=$(basename "$entry")
    case "$base" in README.md|.DS_Store|_index.md) continue ;; esac
    kind=dir
    name="$base"
    if [ ! -d "$entry" ]; then
      kind=file
      name="(根下散落)"
    fi
    if [ -n "$ONLY_GAME" ] && { [ "$kind" = "file" ] || [ "$name" != "$ONLY_GAME" ]; }; then
      continue
    fi
    checked=$((checked+1))
    size=$(du -sh "$entry" 2>/dev/null | cut -f1)
    if [ "$kind" = "file" ]; then
      printf '  · WARN  %-46s %6s  根下散落文件，不属于任何游戏目录\n' "$entry" "$size"
      warn=$((warn+1))
      continue
    fi
    st=$(status_of "$name")
    case "$st" in
      delivered)
        printf '  ✗ FAIL  %-46s %6s  「%s」status=delivered，已交付仍有残留\n' "$entry" "$size" "$name"
        fail=$((fail+1))
        ;;
      active|blocked)
        printf '  · WARN  %-46s %6s  「%s」status=%s（在研，收尾时清）\n' "$entry" "$size" "$name" "$st"
        warn=$((warn+1))
        ;;
      *)
        printf '  · WARN  %-46s %6s  「%s」无 games 状态文件，无法判定是否已收尾\n' "$entry" "$size" "$name"
        warn=$((warn+1))
        ;;
    esac
  done < <(find "$d" -mindepth 1 -maxdepth 1 2>/dev/null | sort)
done

if [ "$checked" -eq 0 ]; then
  printf '  ✓ 七处零残留（README 除外）\n'
fi
printf '结论：检查 %d 项 · FAIL %d · WARN %d\n' "$checked" "$fail" "$warn"
[ "$fail" -gt 0 ] && exit 1
exit 0
