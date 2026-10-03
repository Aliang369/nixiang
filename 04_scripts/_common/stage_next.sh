#!/usr/bin/env bash
# ================================================================
#  stage_next.sh — 机械管线的「包装器 / 内循环驱动」（2026-10-03 新增，P4/P5）
#
#  它与 pipeline.sh 的关系（铁律）：
#    * 它 **不替代** pipeline.sh，也 **不修改** pipeline.sh / pipeline.lock
#      （后者启动即校验 sha256 指纹；两个文件都走 git 同步、lock 被 Syncthing
#        忽略 ⇒ 真要改必须同一提交改 pipeline.sh + 刷新 lock 并 push，
#        另一台 `git pull` 后才一致，否则那台会 die「指纹不匹配」）。
#    * 所有机械动作仍由 `bash 04_scripts/pipeline.sh <游戏> <动词>` 执行；
#      本脚本只做三件事：① 转发动词 ② 回显日志尾 40 行 ③ 装机前查轮次门禁。
#
#  用法:
#    bash 04_scripts/_common/stage_next.sh <游戏> status        # 开工第一步：钉硬约束 + 管线状态 + 台账摘要
#    bash 04_scripts/_common/stage_next.sh <游戏> next          # 执行下一阶段（装机阶段前自动过轮次门禁）
#    bash 04_scripts/_common/stage_next.sh <游戏> auto [N]      # 连跑最多 N 个 next（默认 5），失败即停
#    bash 04_scripts/_common/stage_next.sh <游戏> log [N]       # 只回显 pipeline.log 尾 N 行（默认 40）
#    bash 04_scripts/_common/stage_next.sh <游戏> rounds        # 打假设矩阵（被门禁拦住时照它填）
#    bash 04_scripts/_common/stage_next.sh <游戏> gate <阶段id> # 只跑装机前轮次门禁，不执行阶段
#    bash 04_scripts/_common/stage_next.sh <游戏> round <功能#> <有效|无效|未测> [改动摘要]
#                                                              # 记一轮「改代码→装机→用户实测」结果
#
#  轮次门禁（P4）：同一功能「改→装→实测」连续第 3 轮仍无效 ⇒ 拒绝再装机。
#    台账: 10_docs/<文档游戏名>/轮次台账.tsv   （列: 日期 功能# 轮次 改动摘要 装机结果 用户结果）
#    覆盖: 仅当用户当轮明确要求「再试一次」时，用 STAGE_NEXT_FORCE=1 显式放行（会大声打印原因）。
#
#  bash 3.2 兼容（macOS 自带 bash 3.2 也须能跑）：不用关联数组 / mapfile / ${v,,} / &>>
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/../.." && pwd)"
PIPELINE="${ROOT_DIR}/04_scripts/pipeline.sh"
TAIL_LINES="${STAGE_NEXT_TAIL:-40}"

die() { echo "❌ $*" >&2; exit 1; }
info() { echo "▶ $*"; }

usage() {
  awk 'NR>1{ if (/^#/) { sub(/^# ?/, ""); print } else { exit } }' "$0"
}

# ---------- 参数与路径 ----------
GAME="${1:-}"
ACTION="${2:-status}"
[ -n "$GAME" ] || { usage; exit 1; }

GAME_DIR="${ROOT_DIR}/04_scripts/${GAME}"
PIPELINE_JSON="${GAME_DIR}/pipeline.json"
PIPELINE_LOG="${GAME_DIR}/pipeline.log"

# 文档目录名可能与管线游戏键不同名：优先同名，其次唯一模糊命中
DOC_DIR="${ROOT_DIR}/10_docs/${GAME}"
if [ ! -d "$DOC_DIR" ]; then
  cand="$(ls -d "${ROOT_DIR}/10_docs"/*"${GAME}"* 2>/dev/null | grep -v -e '^.*/_[^/]*$' | head -1)"
  [ -n "${cand:-}" ] && [ -d "$cand" ] && DOC_DIR="$cand"
fi
CONSTRAINTS="${DOC_DIR}/硬约束.md"
LEDGER="${DOC_DIR}/轮次台账.tsv"
# 允许覆盖（跨目录文档 / 自检用）：STAGE_NEXT_DOC_DIR、STAGE_NEXT_LEDGER
[ -n "${STAGE_NEXT_DOC_DIR:-}" ] && { DOC_DIR="$STAGE_NEXT_DOC_DIR"; CONSTRAINTS="${DOC_DIR}/硬约束.md"; LEDGER="${DOC_DIR}/轮次台账.tsv"; }
[ -n "${STAGE_NEXT_LEDGER:-}" ] && LEDGER="$STAGE_NEXT_LEDGER"

[ -f "$PIPELINE" ] || die "找不到 ${PIPELINE}"

# ---------- Windows 编码垫片（2026-10-03 实测） ----------
# pipeline.sh 的状态输出含 ⬜/✅ 等字符；Windows 侧 python3 默认 cp936 stdout 会在
# 打印第一行状态时直接 UnicodeEncodeError 崩掉（`'gbk' codec can't encode '\u2b1c'`）。
# pipeline.sh 有指纹锁禁改 ⇒ 改由本包装器注入 UTF-8 环境（macOS 侧无副作用）。
export PYTHONIOENCODING="${PYTHONIOENCODING:-utf-8}"
export PYTHONUTF8="${PYTHONUTF8:-1}"

if [ ! -f "$PIPELINE_JSON" ]; then
  echo "已注册游戏: $(ls "${ROOT_DIR}/04_scripts"/*/pipeline.json 2>/dev/null | awk -F/ '{print $(NF-1)}' | tr '\n' ' ')" >&2
  die "游戏未注册: ${GAME}（缺 ${PIPELINE_JSON}）。新游戏管线由强 AI 生成（见 _common/stages.md）。"
fi

json_get() { python3 - "$PIPELINE_JSON" "$1" <<'PYEOF'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
s = [x for x in d.get("stages", []) if x.get("status") == "pending"]
print(s[0]["id"] if s else "")
PYEOF
}

tail_log() {
  local n="$1"
  [ -f "$PIPELINE_LOG" ] || { echo "(无日志: ${PIPELINE_LOG})"; return 0; }
  echo "──── pipeline.log 尾 ${n} 行 ────"
  tail -n "$n" "$PIPELINE_LOG"
}

# ---------- 硬约束钉行（P1） ----------
pin_constraints() {
  if [ ! -f "$CONSTRAINTS" ]; then
    echo "⚠  缺 ${CONSTRAINTS}（用户下达的「必须/禁止/照 XX 做」未落盘；模板 10_docs/_templates/硬约束.md）"
    return 0
  fi
  echo "──── 硬约束（生效条目，原文见 ${CONSTRAINTS}）────"
  grep -n '生效' "$CONSTRAINTS" | grep -v '已失效' | head -8 | while IFS= read -r line; do
    printf '  %s\n' "$line"
  done
  echo "  （该文件共 $(grep -c '生效' "$CONSTRAINTS" 2>/dev/null | head -1) 行含状态；改功能代码前、装机前各回读一次）"
}

# ---------- 轮次台账（P4） ----------
ensure_ledger() {
  [ -f "$LEDGER" ] && return 0
  [ -d "$DOC_DIR" ] || die "文档目录不存在: ${DOC_DIR}（无法记轮次台账）"
  {
    echo "# 轮次台账 — 同一功能「改代码 → 装机 → 用户实测」逐轮结果（stage_next.sh round 追加）"
    echo "# 门禁规则：同一功能连续 3 轮「无效」⇒ 拒绝装机，先填假设矩阵（stage_next.sh rounds）"
    printf '日期\t功能#\t轮次\t改动摘要\t装机结果\t用户结果\n'
  } > "$LEDGER"
  echo "▶ 已新建台账: ${LEDGER}"
}

# 用法: ledger_round <功能#> <有效|无效|未测> [摘要]
ledger_round() {
  local feat="$1" verdict="$2" note="${3:-}"
  case "$verdict" in 有效|无效|未测) ;; *) die "结果只能填 有效 / 无效 / 未测（收到: ${verdict}）";; esac
  ensure_ledger
  local n
  n="$(awk -F'\t' -v f="$feat" 'NR>3 && $2==f {c++} END{print (c+0)+1}' "$LEDGER")"
  printf '%s\t%s\t%s\t%s\t%s\t%s\n' \
    "$(date '+%Y-%m-%d %H:%M')" "$feat" "$n" "$note" "$(basename "${PIPELINE_LOG}")" "$verdict" >> "$LEDGER"
  echo "✅ 已记：功能#${feat} 第 ${n} 轮 → ${verdict}${note:+（${note}）}"
  if ledger_block_check "$feat"; then
    echo "⛔ 功能#${feat} 已连续 3 轮无效 —— 下次装机将被拒绝。先跑 ${0##*/} ${GAME} rounds 填假设矩阵并贴给用户。"
  fi
  return 0
}

# 返回 0 = 该功能已被门禁盯上（连续 >=3 轮无效）
# 返回 1 = 未触发
ledger_block_check() {
  local feat="$1" streak
  [ -f "$LEDGER" ] || return 1
  streak="$(awk -F'\t' -v f="$feat" '
    NR>3 && $2==f {v[++n]=$6}
    END{
      c=0
      for(i=n;i>=1;i--){ if(v[i]=="无效") c++; else break }
      print c
    }' "$LEDGER")"
  [ "${streak:-0}" -ge 3 ] && return 0
  return 1
}

ledger_summary() {
  if [ ! -f "$LEDGER" ]; then
    echo "──── 轮次台账：尚未建立（当前管线轮次无记录 ⇒ 门禁不会拦）────"
    return 0
  fi
  echo "──── 轮次台账 ${LEDGER} ────"
  awk -F'\t' 'NR>3' "$LEDGER" | tail -n 12 | while IFS= read -r l; do
    printf '  %s\n' "$(printf '%s' "$l" | tr '\t' ' ')"
  done
  echo "  连续无效轮次统计:"
  awk -F'\t' '
    NR>3 {f[$2]=1; v[$2"|"++n[$2]]=$6}
    END{
      for (k in f) {
        c=0; for(i=n[k];i>=1;i--){ if(v[k"|"i]=="无效") c++; else break }
        printf "    功能#%-4s 第 %d 轮后连续无效 %d 轮\n", k, n[k], c
      }
    }' "$LEDGER" | sort
}

print_matrix() {
  echo ""
  echo "════════ 假设矩阵（必须先填完再谈继续装机；由 stage_next.sh rounds 生成）════════"
  ledger_summary
  echo ""
  echo "【已排除的假设】假设 / 反证证据行（文件:行 或 logcat 行）"
  echo "  1. "
  echo "  2. "
  echo "【待验证的假设】假设 / 验证方法（命令） / 代价（分钟）"
  echo "  1. "
  echo "  2. "
  echo "【已确定的事实】事实 / 证据行"
  echo "  1. "
  echo "════════ 填完贴给用户确认方向；确认前禁止再装机（第 3 轮起）════════"
}

# ---------- 装机前门禁 ----------
gate_before_stage() {
  local sid="$1"
  case "$sid" in
    *install*|*Install*) ;;
    *) return 0 ;;
  esac
  [ -f "$LEDGER" ] || return 0
  local blocked="" feat
  for feat in $(awk -F'\t' 'NR>3 {print $2}' "$LEDGER" | sort -u); do
    if ledger_block_check "$feat"; then blocked="${blocked} ${feat}"; fi
  done
  [ -n "$blocked" ] || return 0
  echo ""
  echo "⛔ 轮次门禁：功能#${blocked# } 已连续 3 轮「改动→装机→仍无效」—— 拒绝执行装机阶段 [${sid}]。" >&2
  echo "   绕圈不是勤快，是烧用户的测试时间（AGENTS.md「执行纪律补充」P4）。" >&2
  print_matrix >&2
  if [ "${STAGE_NEXT_FORCE:-0}" = "1" ]; then
    echo "" >&2
    echo "⚠  STAGE_NEXT_FORCE=1：用户当轮明确要求再试一次 ⇒ 放行本次装机（已在日志与汇报中留痕）。" >&2
    return 0
  fi
  echo "   若用户当轮明确要求「再试一次」，用 STAGE_NEXT_FORCE=1 显式放行。" >&2
  exit 3
}

# ---------- 动作 ----------
do_next() {
  local sid rc
  sid="$(json_get)"
  if [ -z "$sid" ]; then
    echo "无待执行阶段（全部完成/失败）。查看状态: bash 04_scripts/_common/stage_next.sh ${GAME} status"
    return 0
  fi
  info "下一阶段: ${sid}"
  gate_before_stage "$sid"
  bash "$PIPELINE" "$GAME" next
  rc=$?
  tail_log "$TAIL_LINES"
  return "$rc"
}

case "$ACTION" in
  status)
    pin_constraints
    echo ""
    info "管线状态（内部调用 pipeline.sh）"
    bash "$PIPELINE" "$GAME" status
    echo ""
    ledger_summary
    ;;
  next)
    do_next
    ;;
  auto)
    n="${3:-5}"
    i=0
    while [ "$i" -lt "$n" ]; do
      i=$((i + 1))
      echo ""
      info "auto ${i}/${n}"
      sid="$(json_get)"
      [ -n "$sid" ] || { echo "无待执行阶段，停止。"; break; }
      do_next || { echo "❌ 阶段失败即停（auto 中断于第 ${i} 次）。读日志定位原因，修复后重跑；禁止换命令绕过。"; exit 1; }
    done
    ;;
  log)
    tail_log "${3:-$TAIL_LINES}"
    ;;
  rounds)
    print_matrix
    ;;
  gate)
    [ $# -ge 3 ] || die "用法: stage_next.sh ${GAME} gate <stage_id>  # 只跑装机前门禁（不执行阶段）"
    gate_before_stage "$3"
    echo "✅ 轮次门禁：阶段 [${3}] 未被拦（无功能连续 3 轮无效）"
    ;;
  round)
    [ $# -ge 4 ] || die "用法: stage_next.sh ${GAME} round <功能#> <有效|无效|未测> [改动摘要]"
    ledger_round "$3" "$4" "${5:-}"
    ;;
  *)
    usage; die "未知动作: ${ACTION}"
    ;;
esac
