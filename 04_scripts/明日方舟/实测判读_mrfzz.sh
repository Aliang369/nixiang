#!/usr/bin/env bash
# 实测判读（明日方舟/星光）：从 flight_recorder 采集的 logcat 里抽出 [mrfzz] 关键行，
# 按七个功能汇总「hook 是否装上 / 是否真的被游戏调用」，并列出所有失败行。
#
# 用法:
#   bash 04_scripts/明日方舟/实测判读_mrfzz.sh [recorder_dir]
#   不传目录时取 09_test/_recorder/mrfzz_* 中最新的一目录。
#
# 统计口径（v2，2026-10-10 修）：
#   - 只统计「最后一次 MOD 初始化」那个 pid 的行（同一份 logcat 跨多次装机/会话，
#     全量计数会把历次装机的 10 条 hooked 累加成 30，误判为「本轮全装上了」）。
#   - hooked 行打的是**方法名**（`[mrfzz] hooked get_atk argc=0 target=…`），
#     不是功能键名；功能键名只出现在 `[mrfzz] hit <key>`。本表按方法名映射功能。
#   - `zero_cost` 两项共用两条 `get_cost`（Deck.Card 与 Deck.TokenCard），按条数区分。
#
# 退出码: 0 有正常输出；2 找不到日志；3 本轮（该 pid）七项全无 hooked 行
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

DIR="${1:-}"
if [[ -z "${DIR}" ]]; then
  DIR=$(ls -dt "${ROOT}"/09_test/_recorder/mrfzz_* 2>/dev/null | head -1)
fi
if [[ -z "${DIR}" || ! -d "${DIR}" ]]; then
  echo "[判读] 找不到记录仪目录（09_test/_recorder/mrfzz_*）" >&2
  exit 2
fi
LOG="${DIR}/logcat.txt"
if [[ ! -f "${LOG}" ]]; then
  echo "[判读] 找不到 ${LOG}" >&2
  exit 2
fi

SIZE=$(wc -c < "${LOG}" | tr -d ' ')
# logcat 行格式：`MM-DD HH:MM:SS.mmm PID TID L TAG: msg` ⇒ pid = $3
LASTPID=$(LC_ALL=C awk '/\[mrfzz\]/ { p = $3 } END { print p }' "${LOG}" 2>/dev/null || true)
[[ -z "${LASTPID}" ]] && LASTPID="0"

echo "== 判读对象 =="
echo "  目录: ${DIR}"
echo "  日志: ${LOG} (${SIZE} bytes)"
echo "  统计 pid: ${LASTPID}（该 pid 的最后一次 MOD 会话；历次装机已排除）"
echo

# g <字面串>  —— 只看本次会话（同 pid）
g() { LC_ALL=C grep -aF -- "$1" "${LOG}" 2>/dev/null | LC_ALL=C awk -v p="${LASTPID}" '$3 == p' | wc -l | tr -d ' '; }
# ga <字面串> —— 全量（跨会话），用于「曾经装上过」的旁证
ga() { LC_ALL=C grep -acF -- "$1" "${LOG}" 2>/dev/null || true; }

echo "== 初始化 =="
LC_ALL=C grep -aE "\[mrfzz\] (lib ready|ext api|初始化成功)|MOD 已就绪|MOD 菜单初始化|attached ACTIVITY" "${LOG}" | tail -6
echo

echo "== 七功能逐项（本会话 pid=${LASTPID}）=="
printf '  %-16s %-11s %-12s %s\n' "日志 id" "hooked/hit" "标签" "判定"
TOTAL_HOOKED=0
TOTAL_HIT=0
while IFS='|' read -r ID LABEL MINH PATTERN; do
  [[ -z "${ID}" ]] && continue
  H=0
  for pat in ${PATTERN}; do
    N=$(g "[mrfzz] hooked ${pat} ")
    H=$((H + N))
  done
  T=$(g "[mrfzz] hit ${ID}")
  if [[ "${H}" -ge "${MINH}" && "${T}" -gt 0 ]]; then
    V="已生效（游戏真实调用 ${T} 次）"
  elif [[ "${H}" -ge "${MINH}" ]]; then
    V="已装上，本会话未触发（该局没用上？）"
  else
    V="未装上（查下方失败行）"
  fi
  [[ "${H}" -ge "${MINH}" ]] && TOTAL_HOOKED=$((TOTAL_HOOKED + 1))
  [[ "${T}" -gt 0 ]] && TOTAL_HIT=$((TOTAL_HIT + 1))
  printf '  %-16s %-11s %-12s %s\n' "${ID}" "${H}/${T}" "${LABEL}" "${V}"
done <<'ROWS'
attack_speed|超级攻速|1|CalculateAttackInterval
invincible|超级无敌|1|get_isInvincible
base_lock|基地锁血|1|get_lifePointReduce
damage10|十倍伤害|1|get_atk
deploy_anywhere|全图放置|1|get_buildableType
infinite_sp|无限技能.sp|1|get_sp
infinite_cnt|无限技能.次数|1|get_availableCnt
infinite_usedup|无限技能.用尽|1|get_isUsedUp
zero_cost_card|零部署费用.Card|1|get_cost
zero_cost_token|零部署费用.Token|2|get_cost
ROWS
echo

HOOKLINE=$(g "[mrfzz] hooked ")
echo "本会话 hooked 行数 = ${HOOKLINE}（十处挂点期望 10；get_cost 占两条）"
echo "本会话 hit 行数    = $(g "[mrfzz] hit ")   ← 只统计「功能已开 且 钩子被游戏调用」"
echo

echo "== hit 明细（游戏侧真实调用的时间点）=="
HITLINES=$(LC_ALL=C grep -aF "[mrfzz] hit " "${LOG}" 2>/dev/null | LC_ALL=C awk -v p="${LASTPID}" '$3 == p' | tail -12)
if [[ -n "${HITLINES}" ]]; then echo "${HITLINES}"; else echo "  （本会话还没有：功能已开但游戏还没调用到这些钩子 ⇒ 尚未进战斗）"; fi
echo

echo "== hooked 明细（本会话，成功挂上的目标地址）=="
LC_ALL=C grep -aF "[mrfzz] hooked " "${LOG}" | LC_ALL=C awk -v p="${LASTPID}" '$3 == p' | tail -15
echo

echo "== 失败行（应尽量为空）=="
FAILN=0
for pat in "[mrfzz] method miss" "[mrfzz] nested miss" "[mrfzz] dobby fail" "[mrfzz] install give up" "[Il2Cpp] core api missing" "[Il2Cpp] miss" "[Il2Cpp] backfill" "[Il2Cpp] xdl_open" "[Il2Cpp] dynsym locate failed" "[Il2Cpp] asm open miss"; do
  N=$(g "${pat}")
  if [[ "${N}" -gt 0 ]]; then
    FAILN=$((FAILN + 1))
    echo "  ⚠ ${pat} × ${N}"
    LC_ALL=C grep -aF -- "${pat}" "${LOG}" | LC_ALL=C awk -v p="${LASTPID}" '$3 == p' | tail -3
  fi
done
[[ "${FAILN}" -eq 0 ]] && echo "  （以上失败行一条都没有）"
echo

echo "== 稳定性（本会话）=="
for pat in "ANR in com.hypergryph" "Input dispatching timed out" "tombstone" "FATAL EXCEPTION" "signal 9 (Killed)"; do
  printf '  %-32s %s\n' "${pat}" "$(g "${pat}")"
done
echo

if [[ "${TOTAL_HOOKED}" -eq 0 ]]; then
  echo "结论：本轮没有任何功能被装上 —— 用户可能尚未摇出菜单、尚未打开开关，或本次会话未包含 MOD 初始化。"
  exit 3
fi
if [[ "${TOTAL_HIT}" -eq 0 ]]; then
  echo "结论：${TOTAL_HOOKED}/10 处挂点已装上（hooked=${HOOKLINE}），但**本会话还没有任何 hit** ⇒ 尚未进战斗打到触发点（装钩层 OK，效果层待实测）。"
  exit 0
fi
echo "结论：${TOTAL_HOOKED}/10 处挂点（7 功能）已装上，其中 ${TOTAL_HIT} 项已被游戏真实调用（详见上表判定列）。"
echo "     hit 只证明「钩子被调用」；数值/视觉是否真的变了，仍需人工看画面（必要时下一包给该功能加『原值→改后值』首调用日志）。"

