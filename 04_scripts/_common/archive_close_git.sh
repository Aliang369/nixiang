#!/usr/bin/env bash
# archive_close：生成机器交付 manifest，并把 manifest 与管线状态一起收口。
# archive 阶段的源码/文档应先由 archive_scoped_git.sh 提交；本脚本拒绝遗漏的游戏范围脏改动。
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
MANIFEST="10_docs/$GAME/delivery_manifest.json"
[[ -f "$STATE" ]] || { echo "错误: 缺少管线状态文件: $STATE" >&2; exit 1; }

# 未暂存修改和暂存修改都拒绝（STATE/MANIFEST 仅允许本脚本稍后生成/暂存）。
while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  echo "错误: archive_close 前交付范围仍有未暂存改动: $path" >&2
  echo "       先用 archive_scoped_git.sh 完成源码/文档白名单提交，再执行 archive_close。" >&2
  exit 1
done < <(git diff --name-only -- "10_docs/$GAME" "04_scripts/$GAME")
while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  echo "错误: archive_close 前交付范围仍有未提交的暂存改动: $path" >&2
  echo "       archive_close 仅能提交本脚本生成的管线状态与 manifest。" >&2
  exit 1
done < <(git diff --cached --name-only -- "10_docs/$GAME" "04_scripts/$GAME")
while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  [[ "$path" == "$MANIFEST" ]] && continue
  echo "错误: archive_close 前存在未跟踪的交付文件: $path" >&2
  echo "       请先确认归属并通过 archive_scoped_git.sh 纳入源码/文档提交。" >&2
  exit 1
done < <(git ls-files --others --exclude-standard -- "10_docs/$GAME" "04_scripts/$GAME")

git config core.quotepath false

while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  [[ "$path" == "$STATE" || "$path" == "$MANIFEST" ]] || {
    echo "错误: archive_close 前已有其他暂存改动，拒绝提交: $path" >&2
    exit 1
  }
done < <(git diff --cached --name-only)

# manifest 读取归档提交的源码/文档事实；不要求在生成时预测自己的 commit hash。
python3 04_scripts/_common/delivery_manifest.py --game "$GAME" --output "$MANIFEST"
git add -- "$STATE" "$MANIFEST"

# 生成/暂存之后再复核一次：提交前的最终暂存区仍只能包含 STATE 与 MANIFEST。
# 这是第二道门，避免并发暂存或未来脚本改动让非允许文件漏进 commit。
while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  [[ "$path" == "$STATE" || "$path" == "$MANIFEST" ]] || {
    echo "错误: 提交前暂存区含非收口文件，拒绝提交: $path" >&2
    git diff --cached --name-only >&2
    exit 1
  }
done < <(git diff --cached --name-only)

if git diff --cached --quiet -- "$STATE" "$MANIFEST"; then
  echo "管线状态与 manifest 无变化，无需补提交"
  exit 0
fi

git diff --cached --check -- "$STATE" "$MANIFEST"
[[ -n "$MESSAGE" ]] || MESSAGE="$GAME 管线状态收口"
git commit -m "$MESSAGE"

if [[ "$PUSH" == "1" ]]; then
  git push origin main
fi

echo "==> 状态提交完成: $(git log -1 --oneline)"
echo "==> 交付 manifest: $MANIFEST"
python3 04_scripts/_common/delivery_manifest.py --game "$GAME" --output "$MANIFEST" --check
