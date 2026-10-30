#!/usr/bin/env bash
# 补提交：本轮最后生成的两个辅助脚本（收官_推送.sh、清行尾空白.py）
set -uo pipefail
cd /e/nixiang || exit 1
export PYTHONUTF8=1 PYTHONIOENCODING=utf-8 ACK_SHARED=1
bash 04_scripts/_common/archive_scoped_git.sh \
  --message '长生天机降世收官补：推送/清空白两个辅助脚本入库（构建输入 20M 二进制按规范不入库）' \
  --path '04_scripts/长生天机降世/收官_推送.sh' \
  --path '04_scripts/绿梦/probes_20261028/_migrate_E_build_20261028/清行尾空白.py' 2>&1 | tail -12
echo "=== final ==="
git log --oneline -1
git status -sb | head -5
