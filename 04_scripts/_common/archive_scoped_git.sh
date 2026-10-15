#!/usr/bin/env bash
# 通用收官 Git 固化：只暂存显式白名单路径，禁止把其他会话改动带入提交。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

# P0-5：传输自适应推送（SSH 不可达时自动 HTTPS+gh token，见 git_push_main.sh）
source "$(dirname "$0")/git_push_main.sh"

MESSAGE=""
# 默认提交后推送（用户口令「git」= 提交 + 推送连体）；--no-push 才关。
PUSH=1
STAGED_ONLY=0
DRY_RUN=0
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
  - --dry-run: 只校验**当前暂存区**的范围与内容并打印将提交的清单，不提交不推送（隐含 --staged-only）。
               真提交前先跑一次，避免「提交完才发现范围不对」。
  - --require-clean-paths: 白名单路径仍有未暂存改动时直接停止；用于防止混有其它会话改动的共享文件被路径级 add 误带。
  - 共享文件硬门禁: 下列多会话共享文件永远不做路径级 git add（只能内容级暂存后 --staged-only）：
      mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java
      10_docs/_state/current.md
      10_docs/_state/底包再获取台账.md
    这些文件出现在**暂存区**时自动放行，**不需要**额外传 --path；
    它们也不参与 --require-clean-paths（别的写者留下的未暂存改动不算本任务的问题）。
    内容级暂存工具: python3 04_scripts/_common/stage_shared_file.py --path <file> --drop-pattern ...
    被拒时报错会打印「本次 --path 清单 + 自动放行清单 + 两条处置命令」，照做即可。
  - 目录白名单顺带纳入的新增文件会逐条打印提醒（确认是否属于本任务，例如同目录下别的会话的新文件）。
  - 默认提交后推送 origin/main（用户口令「git」= 提交 + 推送连体）。
    只有明确不想推送时才加 --no-push，并在汇报里说明未推送。
  - push 失败/冲突则保留本地提交并报错，不自动强推。
  - 退出码（2026-10-16 起，调用方应按失败处理）：0 = 已提交且（按需）推送成功，或 --no-push；
    **1 = 提交成功但未推送**（网络/通道问题，提交在本地、汇报须写明 ahead）；
    **3 = 远端与本地历史分叉**（需 fetch/rebase，绝不 force）。
    仅「提交本身失败」（白名单越界、空白门禁等）沿用各自原有的非 0 码。
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
    --dry-run)
      DRY_RUN=1
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

# --dry-run：只校验**当前暂存区**的范围与内容并打印将提交的清单；不改暂存区、不提交、不推送。
# 用于「先确认范围对不对」再真提交（尤其是共享文件内容级暂存之后）。
if [[ "$DRY_RUN" == "1" ]]; then
  STAGED_ONLY=1
fi

git config core.quotepath false

is_allowed() {
  local path="$1" allowed
  # 共享文件自动放行（无需 --path）：它们只可能经 stage_shared_file.py 内容级暂存进来，
  # 路径级 add 已被下方共享门禁拦住，故放行不会带入别会话内容。
  # 2026-10-02 实际摩擦：调用方忘了额外传 --path 10_docs/_state/current.md，
  # 内容级暂存明明成功，却在此处被拒，且报错没说清「它其实是允许的」。
  is_shared_file "$path" && return 0
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
      {
        echo "错误: 暂存区出现白名单外路径，拒绝提交: $path"
        echo "       本次 --path 白名单只有:"
        printf '         %s\n' "${PATHS[@]}"
        echo "       自动放行的共享文件（不需要 --path）:"
        printf '         %s\n' "${SHARED_FILES[@]}"
        echo "       处置:"
        echo "         · $path 确属本任务 → 追加 --path '$path' 重跑"
        echo "         · $path 属于其它工作 → git restore --staged -- '$path' 后再提交"
      } >&2
      exit 1
    fi
  done < <(git diff --cached --name-only)
}

check_clean_allowed_paths() {
  local path
  while IFS= read -r path; do
    [[ -n "$path" ]] || continue
    # 共享文件不走路径级 add（见上方共享门禁），故不参与本检查；
    # 否则会话期间别的写者留下的 current.md 未暂存改动会让本检查永远失败。
    is_shared_file "$path" && continue
    if is_allowed "$path"; then
      echo "错误: 白名单路径仍有未暂存改动: $path" >&2
      echo "       该文件可能混有其它会话改动；请先做 hunk 级暂存，再改用 --staged-only" >&2
      exit 1
    fi
  done < <(git diff --name-only)
}

# 如果调用前已有其他会话的暂存内容，直接拒绝，避免误提交。
check_staged_scope

# ---------- 共享文件「逐行确认归属」硬门禁（2026-10-16 P0-1） ----------
# 背景：复盘会话实测——共享文件内容级暂存时把并行会话的行一起写进索引，提交后才发现
# （current.md 的「冒险与征途 v48」行随 d1905e3b 入库）。stage_shared_file.py --own-pattern
# 做逐行归属校验；此处再加一道「提交前必须看过逐行差异」的闸门：只要共享文件在暂存区，
# 就完整打印 git diff --cached，并要求 ACK_SHARED=1 才放行（--dry-run 只读，不拦）。
DRY_CALL=0
case " $* " in *" --dry-run "*) DRY_CALL=1 ;; esac
if [[ "$DRY_CALL" == "0" ]]; then
  for path in "${SHARED_FILES[@]}"; do
    if [[ -n "$(git diff --cached --name-only -- "$path")" ]]; then
      echo "== 共享文件在暂存区：${path}（逐行确认归属；改动行必须全属本轮）==" >&2
      git diff --cached -- "$path" >&2
      if [[ "${ACK_SHARED:-0}" != "1" ]]; then
        echo "错误: 提交含共享文件需显式确认（已打印逐行差异）。确认无误后加 ACK_SHARED=1 重跑：" >&2
        echo "       ACK_SHARED=1 bash $0 --staged-only --message \"…\" --path ${path}" >&2
        echo "       差异里若有别会话行：先 python3 04_scripts/_common/stage_shared_file.py --path ${path} --own-pattern '<只属本轮的串>' 重新内容级暂存。" >&2
        exit 1
      fi
    fi
  done
fi

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
  ':(exclude,glob)10_docs/伊莫/binge_lua_mod_*.lua' \
  `# 同上，勇士之心：从第三方包/出货 so 提取的原始载荷，以及逐字节比对用的生成物` \
  ':(exclude,glob)**/*_原始.js' \
  ':(exclude,glob)**/*from_shipped_so.js' \
  ':(exclude,glob)04_scripts/勇士之心/ysxz_anticheat/patch_v2_ac.js' \
  `# 2026-10-14 蛋壳战机 1.1.8 收官实测：用户/外部「收到的」第三方原始证据（面板日志、他人回传文本）` \
  `# 常带 EOF 多余空行或行尾空白，归一化即改动原始字节 ⇒ 整目录豁免空白门禁，只做入库不校验空白` \
  ':(exclude,glob)10_docs/**/收到_*/**'

printf '%s\n' "$STAGED"
echo "--- 即将提交的改动量（共享文件请逐行核对归属，禁止只用 grep 标记串核验）---"
git diff --cached --stat

if [[ "$DRY_RUN" == "1" ]]; then
  echo "(dry-run: 未提交、未推送)"
  exit 0
fi

git commit -m "$MESSAGE"

# 推送结果必须经退出码传出去（2026-10-16 修）：旧写法 `push_main main || echo "…"`
# 把失败吞掉后仍以 0 结束，于是**调用方只看 rc 就会把「已提交但没推上去」当成功**——
# `finalize_round.sh` 里那句 `rc=$?; [ "$rc" -eq 0 ] || step_fail "$rc"` 因此一直是死代码，
# pipeline 的 archive 阶段同理。现在：本地提交照旧保留、提示照旧打印，但 rc = push_main 的 rc。
PUSH_RC=0
if [[ "$PUSH" == "1" ]]; then
  push_main main || PUSH_RC=$?
  if [[ "$PUSH_RC" != "0" ]]; then
    echo "[push] 未推送（push_main rc=${PUSH_RC}）：本地提交已保留（$(git rev-parse --short HEAD)），汇报里写明 ahead"
  fi
fi

echo "==> 提交完成: $(git log -1 --oneline)"

if [[ "$PUSH_RC" != "0" ]]; then
  # 1 = 未推送/网络问题（提交在本地）；3 = 远端分叉（需 fetch/rebase，绝不 force）。
  echo "==> 退出码 ${PUSH_RC}：提交已在本地，**远端未同步**，调用方请按失败处理" >&2
fi
exit "$PUSH_RC"
