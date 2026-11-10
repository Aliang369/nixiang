#!/usr/bin/env bash
# 一念通天 pipeline 阶段 archive —— 本作 Git 固化（版本无关）。
#
# 只提交本作路径，不触碰其他会话/游戏改动（AGENTS.md §5：禁 git add -A）。
# 10_docs/_state/current.md 是多会话共享索引：只接受「已按行暂存、且暂存改动全部属于本作」的状态。
# 历史事故（2026-09-30）：路径级 add 曾把别会话的索引行与 TapLoginForceWeb.java 一起提交。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/yntt_common.sh"

cd "${NIXIANG_ROOT}"
git config core.quotepath false

yntt_need_file "${SCRIPT_DIR}/cleanup_summary.json"

CLOSE_DOC="$(ls -1t "10_docs/${GAME}"/收尾与最小复现清单_"${YNTT_VER}"_*.md 2>/dev/null | sed -n '1p' || true)"
[ -n "${CLOSE_DOC}" ] || yntt_die "缺少本版收尾清单 10_docs/${GAME}/收尾与最小复现清单_${YNTT_VER}_*.md"
yntt_log "收尾清单=${CLOSE_DOC}"

git add -- \
  "04_scripts/${GAME}" \
  "10_docs/${GAME}" \
  "10_docs/_state/games/${GAME}.md" \
  "10_docs/_state/current.md" \
  "10_docs/_state/机器心跳/mac.md" \
  "10_docs/双机协作/本轮归属.tsv" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android_yntt.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttMod.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttEntry.cpp"

git add -A -- "06_dumped/${GAME}"
if git diff --cached --name-only -- "06_dumped/${GAME}" | grep -E '\.(apk|ap_|so|o|jar|bin|dat|dex|i64|idb)$' >/dev/null; then
  yntt_die "暂存区出现 06_dumped 大二进制，拒绝提交"
fi

# 共享索引 current.md：一次只提交本作那一行；出现非本作行即中止。
# grep 无匹配返回 1，配合 pipefail 会静默死 ⇒ 必须 || true。
CUR_DIFF="$(git diff --cached -- "10_docs/_state/current.md" | grep -E '^[+-]' | grep -vE '^(\+\+\+|---)' || true)"
if [ -n "${CUR_DIFF}" ] && printf '%s\n' "${CUR_DIFF}" | grep -qv "${GAME}"; then
  printf '[yntt][ERROR] current.md 暂存区含非本作行，拒绝提交：\n%s\n' "${CUR_DIFF}" >&2
  exit 1
fi

STAGED="$(git diff --cached --name-only)"
if [ -z "${STAGED}" ]; then
  yntt_log "没有待提交的本作改动"
  exit 0
fi

while IFS= read -r path; do
  [ -n "${path}" ] || continue
  case "${path}" in
    "04_scripts/${GAME}"/*|"10_docs/${GAME}"/*|"10_docs/_state/games/${GAME}.md"|"10_docs/_state/current.md"|\
    "10_docs/_state/机器心跳/mac.md"|"10_docs/双机协作/本轮归属.tsv"|\
    "mod菜单/starry-mod-menu/app/src/main/jni/Android_yntt.mk"|\
    "mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttMod.cpp"|\
    "mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttEntry.cpp"|\
    "06_dumped/${GAME}"/*)
      ;;
    *)
      yntt_die "暂存区出现非本作路径，拒绝提交: ${path}"
      ;;
  esac
done <<EOF
${STAGED}
EOF

printf '%s\n' "${STAGED}"
git commit -m "一念通天 ${YNTT_VER} 星空包交付与收官"
# push 一律经 push_main（AGENTS §5：禁裸 git push）
# shellcheck source=/dev/null
source "${NIXIANG_ROOT}/04_scripts/_common/git_push_main.sh"
push_main main

yntt_log "本作相关状态"
git status --short -- "04_scripts/${GAME}" "06_dumped/${GAME}" "10_docs/${GAME}" "10_docs/_state/games/${GAME}.md" "10_docs/_state/current.md" mod菜单/starry-mod-menu
yntt_log "done"
