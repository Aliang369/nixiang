#!/usr/bin/env bash
# 通用收官 Git 固化：只暂存显式白名单路径，禁止把其他会话改动带入提交。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

MESSAGE=""
PUSH=0
PATHS=()

usage() {
  cat >&2 <<'EOF'
用法:
  bash 04_scripts/_common/archive_scoped_git.sh \
    --message "提交说明" \
    [--push] \
    --path "04_scripts/{游戏}" \
    --path "10_docs/{游戏}" \
    [--path "..."]

说明:
  - 所有 path 都会用 `git add -A --` 暂存。
  - 暂存区出现白名单外路径时拒绝提交。
  - 默认只提交；显式 --push 才推送 origin/main。
  - 空白检查自动排除 pipeline.log、06_dumped、evidence 原始日志和二进制。
EOF
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --message)
      [[ $# -ge 2 ]] || usage
      MESSAGE="$2"
      shift 2
      ;;
    --push)
      PUSH=1
      shift
      ;;
    --path)
      [[ $# -ge 2 ]] || usage
      case "$2" in
        /*) echo "错误: --path 只接受仓库相对路径: $2" >&2; exit 2 ;;
      esac
      PATHS+=("$2")
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

[[ -n "$MESSAGE" ]] || usage
[[ ${#PATHS[@]} -gt 0 ]] || usage

git config core.quotepath false

is_allowed() {
  local path="$1" allowed
  for allowed in "${PATHS[@]}"; do
    case "$path" in
      "$allowed"|"$allowed"/*) return 0 ;;
    esac
  done
  return 1
}

check_staged_scope() {
  local path
  while IFS= read -r path; do
    [[ -n "$path" ]] || continue
    if ! is_allowed "$path"; then
      echo "错误: 暂存区出现白名单外路径，拒绝提交: $path" >&2
      exit 1
    fi
  done < <(git diff --cached --name-only)
}

# 如果调用前已有其他会话的暂存内容，直接拒绝，避免误提交。
check_staged_scope

for path in "${PATHS[@]}"; do
  # 已通过 git rm --cached 退库的 ignored 文件：保留 staged deletion，不再次 add。
  if [[ -n "$(git diff --cached --name-only --diff-filter=D -- "$path")" ]]; then
    continue
  fi
  if [[ ! -e "$path" ]] && [[ -z "$(git ls-files -- "$path")" ]]; then
    echo "错误: 白名单路径不存在且未被 Git 跟踪: $path" >&2
    exit 1
  fi
  git add -A -- "$path"
done

STAGED="$(git diff --cached --name-only)"
if [[ -z "$STAGED" ]]; then
  echo "没有待提交的改动"
  exit 0
fi

check_staged_scope

git diff --cached --check -- . \
  ':(exclude,glob)**/*.log' \
  ':(exclude,glob)06_dumped/**' \
  ':(exclude,glob)**/evidence_*/**' \
  ':(exclude,glob)**/*.bin' \
  ':(exclude,glob)**/*.dat' \
  ':(exclude,glob)**/*.dex' \
  ':(exclude,glob)**/*.so'

printf '%s\n' "$STAGED"
git commit -m "$MESSAGE"

if [[ "$PUSH" == "1" ]]; then
  git push origin main
fi

echo "==> 提交完成: $(git log -1 --oneline)"
