#!/usr/bin/env bash
# ================================================================
#  workspace_health.sh — 工作区一屏体检（月度/收尾时跑一次）
#
#  用法:
#    bash 04_scripts/_common/workspace_health.sh          # 摘要（默认）
#    bash 04_scripts/_common/workspace_health.sh --full   # 含明细
#
#  退出码: 0=无硬问题；1=有需要处理的红项
#
#  为什么需要（2026-11-10 用户指出「工作区杂乱无章」后建立）：
#    此前每项健康度都散在不同脚本里，没人会把它们一起跑 —— 于是
#    「规则冲突」「文档不连贯」「脚本荒废」只能靠出问题才发现。
#    本脚本只做**汇总与红线计数**，不改任何文件；判据全部复用既有门禁，
#    不在这里另写一套标准。
#
#  红线（任一即 rc=1）：
#    ① 冲突标记残留  ② _index.md 过期  ③ 脚本索引过期
#    ④ 规则门禁映射有错  ⑤ AGENTS.md 超硬闸
#  提示项（不挡关）：
#    ⑥ 文档覆盖率  ⑦ 摘要陈旧  ⑧ 未跟踪/脏文件  ⑨ 产物体积
# ================================================================
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT" || exit 2

FULL=0
[ "${1:-}" = "--full" ] && FULL=1

red=0
say() { printf '%s\n' "$*"; }
hdr() { printf '\n\033[1m── %s ──\033[0m\n' "$*"; }
ok()  { printf '  \033[32m✓\033[0m %s\n' "$*"; }
bad() { printf '  \033[31m✗\033[0m %s\n' "$*"; red=$((red+1)); }
warn(){ printf '  \033[33m·\033[0m %s\n' "$*"; }

py() {
  for c in python3.11 python3 python py; do
    command -v "$c" >/dev/null 2>&1 && { echo "$c"; return 0; }
  done
  return 1
}
PY=$(py) || PY=""

say "════════ 工作区体检 $(date +%Y-%m-%d\ %H:%M) ════════"

# ---------- ① 冲突标记 ----------
hdr "1. 冲突标记"
if [ -f 04_scripts/_common/check_merge_markers.sh ]; then
  if out=$(bash 04_scripts/_common/check_merge_markers.sh 2>&1); then
    ok "0 处残留"
  else
    bad "存在 Git 冲突标记（会污染开局注入）"
    [ "$FULL" = "1" ] && printf '%s\n' "$out" | grep -v '^✓' | sed 's/^/      /' | head -10
  fi
fi

# ---------- ② 游戏文档索引 ----------
hdr "2. 游戏文档索引（10_docs/{游戏}/_index.md）"
if [ -n "$PY" ] && [ -f 04_scripts/_common/gen_game_doc_index.py ]; then
  if out=$(PYTHONUTF8=1 "$PY" 04_scripts/_common/gen_game_doc_index.py --check 2>&1); then
    ok "$out"
  else
    bad "有目录索引缺失/过期"
    printf '%s\n' "$out" | sed 's/^/      /' | head -8
  fi
fi

# ---------- ③ 脚本索引 ----------
hdr "3. 公共脚本索引（04_scripts/_common/scripts_index.tsv）"
if [ -n "$PY" ] && [ -f 04_scripts/_common/gen_scripts_index.py ]; then
  if out=$(PYTHONUTF8=1 "$PY" 04_scripts/_common/gen_scripts_index.py --check 2>&1); then
    ok "$out"
  else
    bad "脚本索引过期"
    printf '%s\n' "$out" | sed 's/^/      /' | head -5
  fi
  if [ -f 04_scripts/_common/scripts_index.tsv ]; then
    orphans=$(awk -F'\t' 'NR>1 && $5=="orphan"' 04_scripts/_common/scripts_index.tsv | wc -l | tr -d ' ')
    [ "$orphans" -gt 0 ] && warn "$orphans 个孤儿脚本（无任何引用，考虑补路由或迁 legacy_tools/）" || ok "无孤儿脚本"
  fi
fi

# ---------- ④ 规则门禁映射 ----------
hdr "4. 规则 ↔ 门禁映射"
if [ -n "$PY" ] && [ -f 04_scripts/_common/audit_rule_gate_map.py ]; then
  if out=$(PYTHONUTF8=1 "$PY" 04_scripts/_common/audit_rule_gate_map.py check 2>&1); then
    ok "$out"
  else
    bad "映射表有错（引用了不存在的正文/脚本）"
    printf '%s\n' "$out" | sed 's/^/      /' | head -8
  fi
fi

# ---------- ⑤ 入口体量 ----------
hdr "5. 入口体量（AGENTS.md）"
if [ -f AGENTS.md ]; then
  sz=$(wc -c < AGENTS.md | tr -d ' ')
  if [ "$sz" -gt 32768 ]; then bad "AGENTS.md ${sz} B > 32768 硬闸"
  elif [ "$sz" -gt 30720 ]; then warn "AGENTS.md ${sz} B > 30720 预警线"
  else ok "AGENTS.md ${sz} B（硬闸 32768）"; fi
fi

# ---------- ⑥ 文档覆盖率 ----------
hdr "6. 单游戏文档覆盖率"
if [ -n "$PY" ] && [ -f 04_scripts/_common/audit_game_docs_coverage.py ]; then
  out=$(PYTHONUTF8=1 "$PY" 04_scripts/_common/audit_game_docs_coverage.py scan 2>&1 | tail -20)
  tot=$(printf '%s\n' "$out" | grep -c '^| `')
  hz=$(grep -c '^\- 有 `硬约束.md`' 2>/dev/null || true)
  warn "覆盖清单：$(sed -n 's/^- 游戏目录总数：\*\*\([0-9]*\)\*\*/\1/p' 10_docs/_state/游戏文档覆盖清单.md 2>/dev/null) 个目录 / 硬约束 $(sed -n 's/^- 有 `硬约束.md`：\*\*\([0-9]*\)\*\*/\1/p' 10_docs/_state/游戏文档覆盖清单.md 2>/dev/null) / 路线锁定 $(sed -n 's/^- 有 `路线锁定.md`：\*\*\([0-9]*\)\*\*/\1/p' 10_docs/_state/游戏文档覆盖清单.md 2>/dev/null)"
  warn "缺硬约束的游戏下次接手时只能靠摘要+记忆（不阻断，但接老游戏前先补）"
fi

# ---------- ⑦ 摘要陈旧度 ----------
hdr "7. 状态摘要新鲜度"
if [ -d 10_docs/_state/games ]; then
  n=$(ls 10_docs/_state/games/*.md 2>/dev/null | wc -l | tr -d ' ')
  big=$(find 10_docs/_state/games -name '*.md' -size +12k 2>/dev/null | wc -l | tr -d ' ')
  ok "$n 份摘要"
  [ "$big" -gt 0 ] && warn "$big 份超过 12 KB（应把旧轮次下沉 history/）" || ok "无超 12 KB 的摘要"
fi

# ---------- ⑧ Git 工作区 ----------
hdr "8. Git 工作区"
untracked=$(git status --porcelain 2>/dev/null | grep -c '^??' || true)
modified=$(git status --porcelain 2>/dev/null | grep -c '^ M' || true)
branch=$(git status -sb 2>/dev/null | head -1)
say "  $branch"
[ "${untracked:-0}" -gt 50 ] && warn "$untracked 个未跟踪项（根目录散落？用 trash_paths.sh 归位）" || ok "未跟踪 ${untracked:-0} 个"
[ "${modified:-0}" -gt 0 ] && warn "$modified 个已修改未提交" || ok "无未提交改动"

# ---------- ⑨ 产物体积 ----------
hdr "9. 产物体积（可清理项）"
for d in 09_test 13_scratch 06_dumped 07_patched 08_output; do
  [ -d "$d" ] && printf '  %-12s %s\n' "$d/" "$(du -sh "$d" 2>/dev/null | cut -f1)"
done
say "  说明：这些都可再生；保留规则见 _state/规范变更日志.md §归档正文 D"

# ---------- 汇总 ----------
printf '\n════════ 结论 ════════\n'
if [ "$red" -gt 0 ]; then
  printf '\033[31m%s 项红线未过\033[0m —— 见上列 ✗ 行\n' "$red"
  exit 1
fi
printf '\033[32m无红线问题\033[0m（提示项见上列 · 行，不阻断）\n'
exit 0
