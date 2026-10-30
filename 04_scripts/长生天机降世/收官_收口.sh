#!/usr/bin/env bash
# 最后收口：提交收官_提交.sh（本机一次性包装脚本），跑会话度量，输出体积汇总
set -uo pipefail
cd /e/nixiang || exit 1
export PYTHONUTF8=1 PYTHONIOENCODING=utf-8 ACK_SHARED=1
bash 04_scripts/_common/archive_scoped_git.sh --message '长生天机降世收官：白名单提交包装脚本（记录本次交付范围）' \
  --path '04_scripts/长生天机降世/收官_提交.sh' 2>&1 | tail -6
echo
echo "=== 最终状态 ==="
git log --oneline -3
git status -sb | head -4
echo
echo "=== 体积 ==="
du -sh 08_output/长生天机降世 08_output/绿梦 04_scripts/绿梦/build_inputs_20261028 04_scripts/绿梦/probes_20261028 01_tools/dotnet 06_dumped/长生天机降世 99_archive/_trash_20261028 2>/dev/null
echo "工作区总计: $(du -sh . 2>/dev/null | cut -f1)"
echo
echo "=== 会话度量 ==="
D:/Python313/python.exe 04_scripts/_common/session_metrics.py --session-id current --gate 2>&1 | tail -12
