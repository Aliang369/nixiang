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
KNOWN_DOC_ISSUES=0   # 2026-10-27 第五轮：28 处既有断链已清零，任何新 issue 即失败
DOCS_STRICT=0
[ "${1:-}" = "--docs-strict" ] && DOCS_STRICT=1

command -v python3 >/dev/null 2>&1 || { echo "FAIL 找不到 python3"; exit 2; }

rc=0

echo "== 1/7 单元测试（硬约束结构 / 经验卡 / 覆盖类） =="
python3 -B -m unittest test_audit_workspace_docs test_audit_experience_registry test_audit_coverage || rc=1

echo
echo "== 2/7 硬约束结构审计（--strict，28 份游戏硬约束） =="
python3 -B audit_workspace_docs.py --root "$ROOT" constraints --strict || rc=1

echo
echo "== 3/7 经验卡元数据审计 =="
python3 -B audit_experience_registry.py --root "$ROOT" || rc=1

echo
echo "== 4/7 全仓文档审计（链接 / 指针长度 / 状态索引体量） =="
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
echo "== 5/7 加固门禁覆盖审计 =="
python3 -B harden_coverage.py --root "$ROOT" check || rc=1

echo
echo "== 6/7 游戏文档覆盖（95 目录） + 规则↔门禁映射 =="
python3 -B audit_game_docs_coverage.py --root "$ROOT" check || rc=1
python3 -B audit_rule_gate_map.py --root "$ROOT" check || rc=1

echo
echo "== 7/7 补丁卫生：git diff --check + 脚本/配置 preflight =="
git -C "$ROOT" diff --check || { echo "FAIL git diff --check"; rc=1; }
bash "$HERE/preflight.sh" \
  "$HERE/audit_workspace_docs.py" "$HERE/audit_experience_registry.py" \
  "$HERE/test_audit_workspace_docs.py" "$HERE/test_audit_experience_registry.py" \
  "$HERE/run_audit_tests.sh" "$HERE/harden_coverage.py" \
  "$HERE/audit_game_docs_coverage.py" "$HERE/audit_rule_gate_map.py" \
  "$HERE/test_audit_coverage.py" | tail -3 || rc=1

echo
if [ "$rc" -eq 0 ]; then echo "✅ 全部通过"; else echo "❌ 有失败项（见上）"; fi
exit "$rc"
