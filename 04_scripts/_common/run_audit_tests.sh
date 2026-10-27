#!/usr/bin/env bash
# 一条命令复跑「规则文档」相关的全部机器检查与单元测试（可在任意干净环境执行）。
# 用法：bash 04_scripts/_common/run_audit_tests.sh [--docs-strict]
#   --docs-strict：全仓文档审计一旦有任何 issue 就算失败（默认只与已知基线比较）
# 退出码：0 全过；1 有失败项；2 环境缺 python3。
set -uo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
cd "$HERE" || exit 2

# 已知基线：全仓文档审计里既有的 missing-link 数（普查 C 段记录；本脚本只在「新增」时失败）
KNOWN_DOC_ISSUES=28
DOCS_STRICT=0
[ "${1:-}" = "--docs-strict" ] && DOCS_STRICT=1

command -v python3 >/dev/null 2>&1 || { echo "FAIL 找不到 python3"; exit 2; }

rc=0

echo "== 1/5 单元测试（硬约束结构审计 + 经验卡审计） =="
python3 -B -m unittest test_audit_workspace_docs test_audit_experience_registry || rc=1

echo
echo "== 2/5 硬约束结构审计（--strict，28 份游戏硬约束） =="
python3 -B audit_workspace_docs.py --root "$ROOT" constraints --strict || rc=1

echo
echo "== 3/5 经验卡元数据审计 =="
python3 -B audit_experience_registry.py --root "$ROOT" || rc=1

echo
echo "== 4/5 全仓文档审计（链接 / 指针长度 / 状态索引体量） =="
docs_out="$(python3 -B audit_workspace_docs.py --root "$ROOT" audit 2>&1)"
docs_rc=$?
printf '%s\n' "$docs_out" | tail -3
issues="$(printf '%s\n' "$docs_out" | sed -n 's/.*Audited [0-9]* files; \([0-9]*\) issue(s).*/\1/p')"
if [ -z "$issues" ]; then
  echo "FAIL 无法解析文档审计结果"; rc=1
elif [ "${docs_rc}" -eq 0 ]; then
  echo "文档审计: 0 issue"
elif [ "${DOCS_STRICT}" -eq 1 ]; then
  echo "FAIL 文档审计 issue=${issues}（--docs-strict）"; rc=1
elif [ "$issues" -le "$KNOWN_DOC_ISSUES" ]; then
  echo "WARN 文档审计 issue=${issues} ≤ 已知基线 ${KNOWN_DOC_ISSUES}（既有债务，见 规范普查 §C/§九；未新增）"
else
  echo "FAIL 文档审计 issue=${issues} > 已知基线 ${KNOWN_DOC_ISSUES}（新增断链，必须修）"; rc=1
fi

echo
echo "== 5/5 补丁卫生：git diff --check + 脚本/配置 preflight =="
git -C "$ROOT" diff --check || { echo "FAIL git diff --check"; rc=1; }
bash "$HERE/preflight.sh" \
  "$HERE/audit_workspace_docs.py" "$HERE/audit_experience_registry.py" \
  "$HERE/test_audit_workspace_docs.py" "$HERE/test_audit_experience_registry.py" \
  "$HERE/run_audit_tests.sh" | tail -3 || rc=1

echo
if [ "$rc" -eq 0 ]; then echo "✅ 全部通过"; else echo "❌ 有失败项（见上）"; fi
exit "$rc"
