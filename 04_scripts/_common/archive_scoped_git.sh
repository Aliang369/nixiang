#!/usr/bin/env bash
# 通用收官 Git 固化：只暂存显式白名单路径，禁止把其他会话改动带入提交。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

MESSAGE=""
# 默认提交后推送（用户口令「git」= 提交 + 推送连体）；--no-push 才关。
PUSH=1
STAGED_ONLY=0
PATHS=()
REQUIRE_CLEAN_PATHS=0

# 多会话共享文件：多个会话会在同一会话周期内并行改这些文件（当前写者不止本任务）。
# 路径级 `git add` 会把别的会话未提交的改动一起带走（2026-09-30 实际发生过一次：
# 提交 9aed48b9 混入伊莫会话 17 行证书常量），故这里做硬门禁，只允许内容级暂存。
SHARED_FILES=(
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java"
  "10_docs/_state/current.md"
  "10_docs/_state/底包再获取台账.md"
)

is_shared_file() {
  local path="$1" shared
  for shared in "${SHARED_FILES[@]}"; do
    [[ "$path" == "$shared" ]] && return 0
  done
  return 1
}

usage() {
  cat >&2 <<'EOF'
用法:
  bash 04_scripts/_common/archive_scoped_git.sh \
    --message "提交说明" \
    [--no-push] \
    [--staged-only] \
    [--require-clean-paths] \
    --path "04_scripts/{游戏}" \
    --path "10_docs/{游戏}" \
    [--path "..."]

说明:
  - 所有 path 都会用 `git add -A --` 暂存。
  - 路径下若已有 `git rm --cached` 退库的文件：照常 add 整条路径，再把被加回的退库文件
    复原为删除；**不会**像旧版那样跳过整条路径（旧版会连带漏掉同目录的合法改动）。
  - 暂存区出现白名单外路径时拒绝提交。
  - --staged-only: 不执行 git add，只校验已有暂存区、提交并推送。
  - --require-clean-paths: 白名单路径仍有未暂存改动时直接停止；用于防止混有其它会话改动的共享文件被路径级 add 误带。
  - 共享文件硬门禁: 下列多会话共享文件永远不做路径级 git add（只能内容级暂存后 --staged-only）：
      mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java
      10_docs/_state/current.md
      10_docs/_state/底包再获取台账.md
    内容级暂存工具: python3 04_scripts/_common/stage_shared_file.py --path <file> --drop-pattern ...
  - 目录白名单顺带纳入的新增文件会逐条打印提醒（确认是否属于本任务，例如同目录下别的会话的新文件）。
  - 默认提交后推送 origin/main（用户口令「git」= 提交 + 推送连体）。
    只有明确不想推送时才加 --no-push，并在汇报里说明未推送。
  - push 失败/冲突则保留本地提交并报错，不自动强推。
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
    --no-push)
      PUSH=0
      shift
      ;;
    --staged-only)
      STAGED_ONLY=1
      shift
      ;;
    --require-clean-paths)
      REQUIRE_CLEAN_PATHS=1
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

check_clean_allowed_paths() {
  local path
  while IFS= read -r path; do
    [[ -n "$path" ]] || continue
    if is_allowed "$path"; then
      echo "错误: 白名单路径仍有未暂存改动: $path" >&2
      echo "       该文件可能混有其它会话改动；请先做 hunk 级暂存，再改用 --staged-only" >&2
      exit 1
    fi
  done < <(git diff --name-only)
}

# 如果调用前已有其他会话的暂存内容，直接拒绝，避免误提交。
check_staged_scope

# 共享文件门禁：路径级 add 前先看它是否混有其它会话的在写改动。
SHARED_DIRTY=()
for path in "${PATHS[@]}"; do
  if is_shared_file "$path" && [[ -n "$(git diff --name-only -- "$path")" ]]; then
    SHARED_DIRTY+=("$path")
  fi
done
if [[ ${#SHARED_DIRTY[@]} -gt 0 ]]; then
  echo "注意: 以下共享文件仍有未暂存改动（可能含其它会话在写的内容）:" >&2
  printf '        %s\n' "${SHARED_DIRTY[@]}" >&2
  if [[ "$STAGED_ONLY" == "0" ]]; then
    echo "错误: 拒绝用路径级 git add 提交共享文件，否则会带走其它会话的改动。" >&2
    echo "       请改用内容级暂存：" >&2
    echo "         python3 04_scripts/_common/stage_shared_file.py --path <file> --drop-pattern '<他会话标记>' ..." >&2
    echo "         bash 04_scripts/_common/archive_scoped_git.sh --staged-only --message \"…\" --path <file>" >&2
    exit 1
  fi
  echo "       （--staged-only 不会再做 add，仅提醒：本次提交内容取自当前暂存区）" >&2
fi

if [[ "$REQUIRE_CLEAN_PATHS" == "1" ]]; then
  check_clean_allowed_paths
fi

if [[ "$STAGED_ONLY" == "0" ]]; then
  for path in "${PATHS[@]}"; do
    if [[ ! -e "$path" ]] && [[ -z "$(git ls-files -- "$path")" ]]; then
      echo "错误: 白名单路径不存在且未被 Git 跟踪: $path" >&2
      exit 1
    fi

    # 本次 add 之前该路径下已有的 staged deletion（例如 git rm --cached 退库的 ignored 文件）。
    # 不能像旧写法那样"看到删除就整条路径 continue"：那会连带跳过同一目录下其它合法改动。
    # 2026-10-01 实际发生：--path 04_scripts 因 26 个退库删除，静默漏掉了
    # 04_scripts/_common/env.sh 的修改（提交后复查才发现）。
    # 正确做法：照常 add，再把被重新加回的退库文件复原为删除。
    # 注意 bash 3.2 兼容（mac 侧是 3.2.57）：只用 while read + here-string，不用 bash 4+ 内建。
    KEEP_DELETED=""
    while IFS= read -r _f; do
      [[ -n "$_f" ]] || continue
      KEEP_DELETED="${KEEP_DELETED}${_f}"$'\n'
    done < <(git diff --cached --name-only --diff-filter=D -- "$path")

    git add -A -- "$path"

    if [[ -n "$KEEP_DELETED" ]]; then
      while IFS= read -r _f; do
        [[ -n "$_f" ]] || continue
        # 目标：add 之前已是 staged deletion 的文件，add 之后必须仍是 staged deletion。
        # 不用 --diff-filter=A 判断：文件若与 HEAD 内容相同，重新加回后相对 HEAD 是"无变化"，
        # 不会显示为 A（实测踩过），只能反过来查"删除是否还在"。
        if [[ -z "$(git diff --cached --name-only --diff-filter=D -- "$_f")" ]]; then
          if ! git rm --cached -q -- "$_f"; then
            echo "错误: 无法复原 $_f 的退库状态，请手工检查暂存区" >&2
            exit 1
          fi
          echo "注意: $_f 此前已 git rm --cached 退库，路径级 add 后已复原为删除（本地文件保留）" >&2
        fi
      done <<< "$KEEP_DELETED"
    fi
    unset KEEP_DELETED
  done

  # 目录白名单顺带纳入的新增文件：逐条提醒核对归属（同目录可能有别的会话的新文件）。
  while IFS= read -r path; do
    [[ -n "$path" ]] || continue
    if is_shared_file "$path"; then
      echo "错误: 共享文件被路径级 add 带出，请改用内容级暂存: $path" >&2
      exit 1
    fi
    literal=0
    for allowed in "${PATHS[@]}"; do
      [[ "$path" == "$allowed" ]] && literal=1
    done
    if [[ "$literal" == "0" ]]; then
      echo "提醒: 目录白名单顺带纳入的新增文件，请确认属于本任务: $path" >&2
    fi
  done < <(git diff --cached --diff-filter=A --name-only)
fi

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
  ':(exclude,glob)**/*.so' \
  `# 提取类原始载荷：保留字节原貌（含作者原有的行尾空白），不允许为了过空白门禁改写` \
  ':(exclude,glob)**/*_raw.lua' \
  ':(exclude,glob)10_docs/伊莫/binge_lua_mod_*.lua'

printf '%s\n' "$STAGED"
echo "--- 即将提交的改动量（共享文件请逐行核对归属，禁止只用 grep 标记串核验）---"
git diff --cached --stat
git commit -m "$MESSAGE"

if [[ "$PUSH" == "1" ]]; then
  git push origin main
fi

echo "==> 提交完成: $(git log -1 --oneline)"
