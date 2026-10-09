#!/usr/bin/env bash
# 实测判读（明日方舟/星光）：从 flight_recorder 采集的 logcat 里抽出 [mrfzz] 关键行，
# 按七个功能汇总「hook 是否装上 / 是否真的被游戏调用」，并列出所有失败行。
#
# 用法:
#   bash 04_scripts/明日方舟/实测判读_mrfzz.sh [recorder_dir]
#   不传目录时取 09_test/_recorder/mrfzz_* 中最新的一目录。
#
# 退出码: 0 有正常输出；2 找不到日志；3 七项全无 hooked 行（可能用户还没点开关）
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
echo "== 判读对象 =="
echo "  目录: ${DIR}"
echo "  日志: ${LOG} (${SIZE} bytes)"
echo

g() { LC_ALL=C grep -acF -- "$1" "${LOG}" 2>/dev/null || true; }

echo "== 初始化 =="
LC_ALL=C grep -aE "\[mrfzz\] (lib ready|ext api|初始化成功)|MOD 已就绪|MOD 菜单初始化|attached ACTIVITY" "${LOG}" | tail -6
echo

echo "== 七功能逐项 =="
printf '  %-16s %-16s %-8s %s\n' "日志 id" "hooked/hit" "标签" "判定"
TOTAL_HOOKED=0
for row in \
  "attack_speed|超级攻速" \
  "invincible|超级无敌" \
  "base_lock|基地锁血" \
  "damage10|十倍伤害" \
  "deploy_anywhere|全图放置" \
  "infinite_sp|无限技能(sp)" \
  "infinite_cnt|无限技能(次数)" \
  "infinite_usedup|无限技能(用尽)" \
  "zero_cost_card|零部署费用(Card)" \
  "zero_cost_token|零部署费用(TokenCard)"
do
  ID="${row%%|*}"
  LABEL="${row#*|}"
  H=$(g "[mrfzz] hooked ${ID} ")
  T=$(g "[mrfzz] hit ${ID}")
  if [[ "${H}" -gt 0 && "${T}" -gt 0 ]]; then
    V="已生效（游戏真实调用 ${T} 次）"
  elif [[ "${H}" -gt 0 ]]; then
    V="已装上，未触发（战斗里没用上？）"
  else
    V="未装上（查下方 miss / give up）"
  fi
  [[ "${H}" -gt 0 ]] && TOTAL_HOOKED=$((TOTAL_HOOKED + 1))
  printf '  %-16s %-16s %-8s %s\n' "${ID}" "${H}/${T}" "${LABEL}" "${V}"
done
echo

echo "== hooked 明细（成功挂上的目标地址）=="
LC_ALL=C grep -aF "[mrfzz] hooked " "${LOG}" | tail -15
echo

echo "== 失败行（应尽量为空）=="
FAILN=0
for pat in "[mrfzz] method miss" "[mrfzz] nested miss" "[mrfzz] dobby fail" "[mrfzz] install give up" "[Il2Cpp] core api missing" "[Il2Cpp] miss" "[Il2Cpp] backfill" "[Il2Cpp] xdl_open" "[Il2Cpp] dynsym locate failed" "[Il2Cpp] asm open miss"; do
  N=$(g "${pat}")
  if [[ "${N}" -gt 0 ]]; then
    FAILN=$((FAILN + 1))
    echo "  ⚠ ${pat} × ${N}"
    LC_ALL=C grep -aF -- "${pat}" "${LOG}" | tail -3
  fi
done
[[ "${FAILN}" -eq 0 ]] && echo "  （以上失败行一条都没有）"
echo

if [[ "${TOTAL_HOOKED}" -eq 0 ]]; then
  echo "结论：还没有任何功能被装上 —— 用户可能尚未摇出菜单或尚未打开开关。"
  exit 3
fi
echo "结论：${TOTAL_HOOKED} 个 hook 已装上（详见上表判定列）。"
