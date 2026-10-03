#!/usr/bin/env bash
# ================================================================
#  stage_next.sh — 兼容转发器（薄转发，2026-10-03）
#
#  2026-10-03 起，P1/P4/P5 的能力（硬约束钉行 / 轮次门禁 / 内循环 / 日志尾回显）
#  已**原生并入 `04_scripts/pipeline.sh`**（用户当轮口令「改吧」授权；改法见
#  _common/stages.md「真要改 pipeline.sh 时的跨机姿势」：同一提交改脚本 + 刷新
#  _common/pipeline.lock 的 sha256 + push）。本脚本保留旧命令形态，只做同义转发，
#  不再实现任何逻辑；新会话直接记 pipeline.sh 的原生动词即可。
#
#  用法（新旧对照）:
#    bash 04_scripts/_common/stage_next.sh <游戏> status              → pipeline.sh <游戏> status
#    bash 04_scripts/_common/stage_next.sh <游戏> next                → pipeline.sh <游戏> next
#    bash 04_scripts/_common/stage_next.sh <游戏> auto [N]            → pipeline.sh <游戏> next --loop N（默认 5）
#    bash 04_scripts/_common/stage_next.sh <游戏> log [N]             → pipeline.sh <游戏> log N（默认 40）
#    bash 04_scripts/_common/stage_next.sh <游戏> rounds              → pipeline.sh <游戏> rounds
#    bash 04_scripts/_common/stage_next.sh <游戏> round <功能#> <有效|无效|未测> [摘要]
#    bash 04_scripts/_common/stage_next.sh <游戏> gate <阶段id>
#
#  轮次门禁（P4）：同一功能「改→装→实测」连续 3 轮仍无效 ⇒ 含 install 的阶段被拒
#  （退出码 3），除非用户当轮明确要求再试一次并用 STAGE_NEXT_FORCE=1 显式放行。
#  台账：10_docs/<文档游戏名>/轮次台账.tsv（覆盖路径可用 STAGE_NEXT_DOC_DIR /
#  STAGE_NEXT_LEDGER / STAGE_NEXT_TAIL 调整，自检与跨目录文档用）。
#
#  bash 3.2 兼容（macOS 自带 bash 3.2 也须能跑）：不用关联数组 / mapfile / ${v,,} / &>>
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/../.." && pwd)"
PIPELINE="${ROOT_DIR}/04_scripts/pipeline.sh"

die() { echo "❌ $*" >&2; exit 1; }

usage() {
  awk 'NR>1{ if (/^#/) { sub(/^# ?/, ""); print } else { exit } }' "$0"
}

GAME="${1:-}"
ACTION="${2:-status}"
[ -n "$GAME" ] || { usage; exit 1; }
[ -f "$PIPELINE" ] || die "找不到 ${PIPELINE}"

# exec 转发：保留 pipeline.sh 的退出码（含轮次门禁的 exit 3）
case "$ACTION" in
  status) exec bash "$PIPELINE" "$GAME" status ;;
  next)   exec bash "$PIPELINE" "$GAME" next ;;
  auto)   exec bash "$PIPELINE" "$GAME" next --loop "${3:-5}" ;;
  log)    exec bash "$PIPELINE" "$GAME" log "${3:-${STAGE_NEXT_TAIL:-40}}" ;;
  rounds) exec bash "$PIPELINE" "$GAME" rounds ;;
  round)
    [ $# -ge 4 ] || die "用法: stage_next.sh ${GAME} round <功能#> <有效|无效|未测> [改动摘要]"
    exec bash "$PIPELINE" "$GAME" round "$3" "$4" "${5:-}"
    ;;
  gate)
    [ $# -ge 3 ] || die "用法: stage_next.sh ${GAME} gate <stage_id>"
    exec bash "$PIPELINE" "$GAME" gate "$3"
    ;;
  *) usage; die "未知动作: ${ACTION}（见上方用法）" ;;
esac
