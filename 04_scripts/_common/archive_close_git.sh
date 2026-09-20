#!/usr/bin/env bash
# archive 后端 commit 后，pipeline.sh 会再把阶段写为 done。
# 本脚本只提交该状态变化，消除“补一次手工提交”的重复步骤。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

GAME=""
# 默认提交后推送；--no-push 才关。
PUSH=1
MESSAGE=""

usage() {
  echo "用法: bash 04_scripts/_common/archive_close_git.sh --game <游戏> [--no-push] [--message <说明>]" >&2
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --game)
      [[ $# -ge 2 ]] || usage
      GAME="$2"
      shift 2
      ;;
    --push)
      PUSH=1
      shift
      ;;
    --no-push)
      PUSH=0
      shift
      ;;
    --message)
      [[ $# -ge 2 ]] || usage
      MESSAGE="$2"
      shift 2
      ;;
    -h|--help)
      usage
      ;;
    *)
      echo "错误: 未知参数 $1" >&2
      usage
      ;;
  esac
done

[[ -n "$GAME" ]] || usage
STATE="04_scripts/$GAME/pipeline.json"
[[ -f "$STATE" ]] || { echo "错误: 缺少管线状态文件: $STATE" >&2; exit 1; }

git config core.quotepath false

while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  [[ "$path" == "$STATE" ]] || {
    echo "错误: archive_close 前已有其他暂存改动，拒绝提交: $path" >&2
    exit 1
  }
done < <(git diff --cached --name-only)

git add -- "$STATE"
if git diff --cached --quiet -- "$STATE"; then
  echo "管线状态无变化，无需补提交"
  exit 0
fi

git diff --cached --check -- "$STATE"
[[ -n "$MESSAGE" ]] || MESSAGE="$GAME 管线状态收口"
git commit -m "$MESSAGE"

if [[ "$PUSH" == "1" ]]; then
  git push origin main
fi

echo "==> 状态提交完成: $(git log -1 --oneline)"
