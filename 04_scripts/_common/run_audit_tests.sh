#!/usr/bin/env bash
# 一条命令复跑「规则文档」相关的全部机器检查与单元测试（可在任意干净环境执行）。
# 用法：bash 04_scripts/_common/run_audit_tests.sh
# 退出码：0 全过；1 有失败；2 环境缺 python3。
set -uo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
cd "$HERE" || exit 2

command -v python3 >/dev/null 2>&1 || { echo "FAIL 找不到 python3"; exit 2; }

rc=0
echo "== 单元测试（审计器） =="
python3 -B -m unittest test_audit_workspace_docs test_audit_experience_registry || rc=1

echo
echo "== 硬约束结构审计（--strict） =="
python3 -B audit_workspace_docs.py --root "$ROOT" constraints --strict || rc=1

echo
echo "== 经验卡元数据审计 =="
python3 -B audit_experience_registry.py --root "$ROOT" || rc=1

echo
if [ "$rc" -eq 0 ]; then echo "✅ 全部通过"; else echo "❌ 有失败项（见上）"; fi
exit "$rc"
