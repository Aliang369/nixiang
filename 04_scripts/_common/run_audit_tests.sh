#!/usr/bin/env bash
# 一条命令复跑「规则文档」相关的全部机器检查与单元测试（可在任意干净环境执行）。
# 用法：bash 04_scripts/_common/run_audit_tests.sh [--docs-strict] [--harden-strict]
#   --docs-strict  ：全仓文档审计一旦有任何 issue 就算失败（默认只与已知基线比较）
#   --harden-strict：加固覆盖「严格验收」——任何正式 APK 入口未接入门禁即失败。
#                    ⚠️ 默认模式只保证**结构与回归**通过，不代表全局加固已落地；
#                    当前仍有大量正式 APK 入口未接线，默认模式会打印真实缺口数字。
# 退出码：0 全过；1 有失败项；2 环境缺 python3。
set -uo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
cd "$HERE" || exit 2

# 已知基线：全仓文档审计里既有的 missing-link 数（普查 C 段记录；本脚本只在「新增」时失败）
KNOWN_DOC_ISSUES=0   # 2026-10-27 第五轮：28 处既有断链已清零，任何新 issue 即失败
DOCS_STRICT=0
HARDEN_STRICT=0
for a in "$@"; do
  case "$a" in
    --docs-strict) DOCS_STRICT=1 ;;
    --harden-strict) HARDEN_STRICT=1 ;;
  esac
done

command -v python3 >/dev/null 2>&1 || { echo "FAIL 找不到 python3"; exit 2; }

rc=0

echo "== 1/8 单元测试（硬约束 / 经验卡 / 覆盖类 / 逃生门理由 / 双机脚本） =="
python3 -B -m unittest test_audit_workspace_docs test_audit_experience_registry test_audit_coverage test_harden_reason || rc=1
python3 -B -m unittest discover -s "$ROOT/04_scripts/双机协作" -p 'test_*.py' || rc=1

echo
echo "== 2/8 硬约束结构审计（--strict，28 份游戏硬约束） =="
python3 -B audit_workspace_docs.py --root "$ROOT" constraints --strict || rc=1

echo
echo "== 3/8 经验卡元数据审计 =="
python3 -B audit_experience_registry.py --root "$ROOT" || rc=1

echo
echo "== 4/8 全仓文档审计（链接 / 指针长度 / 状态索引体量） =="
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
echo "== 5/8 加固门禁覆盖审计（默认模式：只报缺口，不据此宣称全局加固完成） =="
harden_out="$(python3 -B harden_coverage.py --root "$ROOT" check 2>&1)"
harden_rc=$?
printf '%s\n' "$harden_out" | grep -v '^WARN .*未接入加固门禁' | tail -3
gaps="$(printf '%s\n' "$harden_out" | grep -c '^WARN .*未接入加固门禁' || true)"
echo "  —— 真实缺口：仍有 ${gaps} 个正式 APK 入口未接入加固门禁（详见 10_docs/防御加固_覆盖登记.md）"
[ "$harden_rc" -eq 0 ] || rc=1
if [ "$HARDEN_STRICT" = "1" ]; then
  echo "== 5b/8 加固覆盖严格验收（--harden-strict） =="
  python3 -B harden_coverage.py --root "$ROOT" check --strict >/tmp/harden_strict.$$ 2>&1
  if [ $? -eq 0 ]; then
    echo "  ✅ 严格验收通过：所有正式 APK 入口均受门禁保护"
  else
    echo "  ❌ 严格验收未通过：$(grep -c '^FAIL' /tmp/harden_strict.$$) 个正式 APK 入口未受保护"
    grep '^FAIL' /tmp/harden_strict.$$ | head -5
    rc=1
  fi
  rm -f /tmp/harden_strict.$$
else
  echo "  （未启用 --harden-strict：本次只做回归，**不得**据此报告「全局加固已完成」）
  echo "  —— 单游戏限定验收：python3 -B harden_coverage.py check --strict --game <游戏名|tag>""
fi

echo
echo "== 6/8 游戏文档覆盖（95 目录） + 规则↔门禁映射 =="
python3 -B audit_game_docs_coverage.py --root "$ROOT" check || rc=1
python3 -B audit_rule_gate_map.py --root "$ROOT" check || rc=1

echo
echo "== 8/8 补丁卫生：git diff --check + 脚本/配置 preflight =="
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
