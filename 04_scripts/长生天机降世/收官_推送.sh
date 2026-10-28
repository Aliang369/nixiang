#!/usr/bin/env bash
# 收官推送：远端已前进（非快进）⇒ 先 pull --rebase --autostash，再经 push_main 推送
set -uo pipefail
cd /e/nixiang || exit 1
export PYTHONUTF8=1 PYTHONIOENCODING=utf-8
echo "== before =="
git status -sb | head -4

echo "== pull --rebase --autostash =="
git pull --rebase --autostash 2>&1 | tail -12
echo "pull rc=$?"

echo "== status after pull =="
git status -sb | head -6
git log --oneline -3

echo "== push =="
source 04_scripts/_common/git_push_main.sh
push_main main 2>&1 | tail -8
echo "push rc=$?"
echo "== final =="
git status -sb | head -3
