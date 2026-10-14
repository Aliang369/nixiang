#!/usr/bin/env bash
# ================================================================
#  finalize_round.sh — 收官编排（把「收尾+正式包+清理+git」那串口令固化成一条命令）
#
#  为什么要有（2026-10-14 潮汐守望者会话复盘 P0-4）:
#    收官口令每轮都要人工按 `stages.md:226` 展开 5～7 步：裁正式包 → 查干净 →
#    列清理候选 → 白名单提交推送 → 回填「提交号/是否推送/远端是否同步」。
#    步骤本身都不难，难在**漏一步**（漏 prune 就有旧包残留、漏 push 复核就不知道
#    远端是否同步）；而且顺序有依赖（必须先裁包再提交，否则清单与实物不符）。
#
#  用法:
#    bash 04_scripts/_common/finalize_round.sh --game 潮汐守望者 \
#         --apk 08_output/潮汐守望者/xxx-releaseSigned.apk [--message "…"] [--path …]…
#         [--id cxswz] [--clean-tag Cxswz] [--so <libstarry_magic.so>] [--apply] [--keep-going]
#    --apply   真正执行（默认 dry-run：只打印每步命令）
#    --message git 提交信息（--apply 时必需）
#    --path    白名单路径，可重复；不给则只做裁剪+候选，不提交
#
#  步骤: 1 裁剪唯一正式包+同步文档片段  2 干净正式包门禁  3 清理候选草案
#        4 白名单提交+推送 origin/main  5 复核 HEAD==origin/main 并打印收官素材
#  退出码: 0=全部通过；1=某步失败；2=用法错
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"

GAME=""; APK=""; MSG=""; ID=""; CLEAN_TAG=""; SO=""
APPLY=0; KEEP_GOING=0
PATHS=""
while [ $# -gt 0 ]; do
  case "$1" in
    --game) GAME="${2:-}"; shift 2 ;;
    --apk) APK="${2:-}"; shift 2 ;;
    --message) MSG="${2:-}"; shift 2 ;;
    --path) PATHS="$PATHS $2"; shift 2 ;;
    --id) ID="${2:-}"; shift 2 ;;
    --clean-tag) CLEAN_TAG="${2:-}"; shift 2 ;;
    --so) SO="${2:-}"; shift 2 ;;
    --apply) APPLY=1; shift ;;
    --dry-run) APPLY=0; shift ;;
    --keep-going) KEEP_GOING=1; shift ;;
    -h|--help) sed -n '2,24p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

[ -n "$GAME" ] || { echo "[finalize] 必须给 --game" >&2; exit 2; }
[ -n "$APK" ] || { echo "[finalize] 必须给 --apk" >&2; exit 2; }
if [ "$APPLY" = "1" ] && [ -n "$PATHS" ] && [ -z "$MSG" ]; then
  echo "[finalize] --apply 且给了 --path 时必须给 --message" >&2; exit 2
fi

run() { # run <说明> <命令…>
  desc="$1"; shift
  if [ "$APPLY" = "1" ]; then
    echo "[finalize] ▶ $desc"
    "$@"
    return $?
  fi
  echo "[finalize] (dry-run) $desc"
  printf '[finalize]   cmd: %s\n' "$*"
  return 0
}

STEP=0; RCFAIL=0
step_begin() { STEP=$((STEP + 1)); echo; echo "[finalize] ==== 步骤 $STEP: $1 ===="; }
step_fail() { echo "[finalize] ✗ 步骤 $STEP 失败(rc=$1)"; RCFAIL=$((RCFAIL + 1)); [ "$KEEP_GOING" = "1" ] || STEP_ABORT=1; }
STEP_ABORT=0

# ---------- 步骤 1: 裁唯一正式包 + 打印文档同步片段 ----------
step_begin "裁剪唯一正式包 + --sync-doc 片段"
run "finalize_game_delivery --prune --sync-doc" \
  bash "$SCRIPT_DIR/finalize_game_delivery.sh" --game "$GAME" --apk "$APK" --prune --sync-doc $( [ "$APPLY" = "1" ] && echo --apply )
rc=$?; [ "$rc" -eq 0 ] || step_fail "$rc"

# ---------- 步骤 2: 干净正式包门禁 ----------
if [ "$STEP_ABORT" = "0" ]; then
  step_begin "干净正式包门禁"
  if [ -n "$CLEAN_TAG" ]; then
    if [ -n "$SO" ]; then
      run "check_release_clean --tag $CLEAN_TAG --so $SO" \
        bash "$SCRIPT_DIR/check_release_clean.sh" --tag "$CLEAN_TAG" --so "$SO"
    else
      run "check_release_clean --tag $CLEAN_TAG" bash "$SCRIPT_DIR/check_release_clean.sh" --tag "$CLEAN_TAG"
    fi
    rc=$?; [ "$rc" -eq 0 ] || step_fail "$rc"
  else
    echo "[finalize] 跳过（未给 --clean-tag）"
  fi
fi

# ---------- 步骤 3: 清理候选草案（只打印，不删）----------
if [ "$STEP_ABORT" = "0" ]; then
  step_begin "清理候选草案（trash_paths --suggest，只打印）"
  if [ -n "$ID" ]; then
    bash "$SCRIPT_DIR/trash_paths.sh" --suggest --game "$GAME" --id "$ID" || true
    echo "[finalize] 选中候选后手动执行: bash 04_scripts/_common/trash_paths.sh --apply <路径…>"
  else
    echo "[finalize] 跳过（未给 --id）"
  fi
fi

# ---------- 步骤 4: 白名单提交 + 推送 ----------
if [ "$STEP_ABORT" = "0" ]; then
  step_begin "白名单提交 + 推送 origin/main"
  if [ -z "$PATHS" ]; then
    echo "[finalize] 跳过（未给 --path）"
  else
    echo "[finalize] 提交前 git status:"
    git -C "$ROOT" status -sb | head -20
    ARGS=""
    for p in $PATHS; do ARGS="$ARGS --path $p"; done
    if [ "$APPLY" = "1" ]; then
      echo "[finalize] ▶ archive_scoped_git --message … $ARGS"
      # shellcheck disable=SC2086
      bash "$SCRIPT_DIR/archive_scoped_git.sh" --message "$MSG" $ARGS
      rc=$?; [ "$rc" -eq 0 ] || step_fail "$rc"
    else
      echo "[finalize] (dry-run) cmd: archive_scoped_git.sh --message \"$MSG\"$ARGS"
    fi
  fi
fi

# ---------- 步骤 5: 复核 + 收官素材 ----------
if [ "$STEP_ABORT" = "0" ]; then
  step_begin "复核 HEAD / origin / 工作区，并打印收官素材"
  echo "[finalize] git status -sb:"; git -C "$ROOT" status -sb | head -10
  HEAD_LOCAL="$(git -C "$ROOT" rev-parse --short HEAD 2>/dev/null || echo '?')"
  HEAD_REMOTE="$(git -C "$ROOT" rev-parse --short origin/main 2>/dev/null || echo '?')"
  echo "[finalize] HEAD=$HEAD_LOCAL origin/main=$HEAD_REMOTE"
  if [ "$HEAD_LOCAL" = "$HEAD_REMOTE" ] && [ "$HEAD_LOCAL" != "?" ]; then
    echo "[finalize] ⇒ 远端已同步"
  else
    echo "[finalize] ⇒ ⚠ 本地与 origin/main 不一致或未推送，请复核"
  fi
  echo "[finalize] 收官汇报必须写出: 提交号($HEAD_LOCAL) / 是否已推送 / 远端是否同步($( [ "$HEAD_LOCAL" = "$HEAD_REMOTE" ] && echo 是 || echo 否 ))"
fi

echo
if [ "$RCFAIL" -eq 0 ]; then
  [ "$APPLY" = "1" ] || echo "[finalize] (dry-run 完成，加 --apply 真正执行)"
  echo "[finalize] RESULT=OK"
  exit 0
fi
echo "[finalize] RESULT=FAIL steps_failed=$RCFAIL"
exit 1
