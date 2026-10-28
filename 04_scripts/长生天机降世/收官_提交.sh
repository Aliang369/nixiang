#!/usr/bin/env bash
# 收官提交：暂存区已就绪（含 current.md 内容级暂存），走 archive_scoped_git --staged-only
# 注：脚本要求至少有 1 个 --path（第 125 行），--staged-only 下只做范围校验不 add。
set -uo pipefail
cd /e/nixiang || exit 1
export PYTHONUTF8=1 PYTHONIOENCODING=utf-8 ACK_SHARED=1
MSG='长生天机降世 1.0.4 收官（正式包裁剪唯一APK+滚动物隔离清理+文档定稿+快照升1.0.4）；绿梦完结（E盘_build 迁入工作区缩容、08 只留 v25/v32、盘外散件归位）'
bash 04_scripts/_common/archive_scoped_git.sh --staged-only --message "$MSG" \
  --path '04_scripts/长生天机降世' \
  --path '04_scripts/绿梦/收官清理_20261028.sh' \
  --path '04_scripts/绿梦/probes_20261028' \
  --path '04_scripts/绿梦/lvmeng_mod' \
  --path '04_scripts/绿梦/ilpatch/ilpatch.win.csproj' \
  --path '04_scripts/绿梦/fg_cage.c' \
  --path '10_docs/长生天机降世' \
  --path '10_docs/绿梦' \
  --path '10_docs/_state/games/长生天机降世.md' \
  --path '10_docs/_state/games/绿梦.md' \
  --path '10_docs/tools/07_本机与真机坑位_H5助手与Hook.md' \
  --path '06_dumped/长生天机降世/1.0.4/README.md' > /tmp/_gitout.txt 2>&1
rc=$?
echo "rc=$rc"
grep -vE 'trailing whitespace|new blank line|^\+|^@@|^ ' /tmp/_gitout.txt | tail -20
echo "=== log ==="
git log --oneline -2
echo "=== status ==="
git status -sb | head -3
