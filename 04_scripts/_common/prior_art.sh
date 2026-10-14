#!/usr/bin/env bash
# ================================================================
#  prior_art.sh — 动手前 3 分钟先例检索（P11）
#
#  背景（2026-10-10 邂逅在迷宫会话复盘 P2）：
#  本会话踩的坑里，至少 4 类是**仓库里早有记录**的（FairGuard 判定走协作式退出见
#  蜀汉传 AB 实验、iptables -m string 不命中、adb install 绝对路径当本地路径、
#  apksigner 断言字节串端序）—— 都是"没检索就重推"。检索成本 3 分钟，
#  重推成本半天到两轮装机。
#
#  用法:
#    bash 04_scripts/_common/prior_art.sh <关键词1> [关键词2 ...] [--dirs 'a b c'] [--max 12] [--quiet]
#  示例:
#    bash 04_scripts/_common/prior_art.sh 加固 自退出 bind-mount
#    bash 04_scripts/_common/prior_art.sh 'libFairGuard' 'exit_group' --max 20
#
#  搜索面（默认，**只管仓内**）: 10_docs/ skill/ 04_scripts/ TOOLBOOK.md AGENTS.md
#  输出：每个关键词命中条数 + 前 N 条 `文件:行:内容`（自动截断长行，便于扫读）
#  退出码: 0=至少一个关键词有命中；1=仓内全无命中 ⇒ **触发一次外部检索**（≠ 可以放心自己推）
#  纪律①：命中先例后**先读先例**再动手；先例结论与本轮现象冲突时，先解释冲突再推翻它。
#  纪律②（2026-10-15 新增，用户本会话「按照你说的方案进行全量修改」）：本脚本只覆盖仓内，
#    仓内 rc=1 时按下方「外部检索」再查一次 —— 依据是三次已查实的失效：
#    境界刀鸣 2026-09-23 记 `web_search` 不可用；霓虹深渊2 2026-10-11 会话 2393 次调用里
#    web_search=0 次、web_fetch 2 次全失败 ⇒「去外面查」这个动作长期消失，全靠自研硬解。
#    预算：≤3 次调用，命中即止；未命中把关键词写进台账「空结果」栏，不再重查。
#    通道（curl 直连，2026-10-15 实测全 200）：DuckDuckGo html 端点 / GitHub API / 看雪 bbs.kanxue.com /
#      吾爱 52pojie.cn / MT 论坛 bbs.binmt.cc。⚠ curl 走 Bing 实测返回无关结果（不可用）；
#      `web_search` 工具本机额度/API key 常年不可用，**不要依赖它**。
#    关键词模板与证据分级见 skill/diagnosis-index.md「外部检索协议」；结果落
#      10_docs/_state/外部方案台账.md（查一次双机复用，含「已评估·不接入」清单防重复评估）。
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

DIRS="10_docs skill 04_scripts TOOLBOOK.md AGENTS.md"
MAX=12; QUIET=0; KWS=""

while [ $# -gt 0 ]; do
  case "$1" in
    --dirs)  DIRS="$2"; shift 2 ;;
    --max)   MAX="$2"; shift 2 ;;
    --quiet) QUIET=1; shift ;;
    -h|--help) sed -n '2,30p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    -*) echo "未知参数: $1" >&2; exit 2 ;;
    *) KWS="$KWS $1"; shift ;;
  esac
done

[ -n "$KWS" ] || { echo "用法: bash 04_scripts/_common/prior_art.sh <关键词...> [--max 12]"; exit 2; }

cd "$NIXIANG_ROOT" || exit 2
hit_any=0

for kw in $KWS; do
  files=""
  for d in $DIRS; do
    [ -e "$d" ] || continue
    files="$files $d"
  done
  # shellcheck disable=SC2086
  out="$(grep -rn -I -F --include='*.md' --include='*.sh' --include='*.py' --include='*.json' --include='*.txt' \
        --  "$kw" $files 2>/dev/null | grep -v '^10_docs/_state/规范优化_20261010' | head -"$MAX")"
  cnt="$(printf '%s' "$out" | grep -c . || true)"
  echo "══ 关键词「${kw}」命中 $cnt 条（显示前 ${MAX}）══"
  if [ "$cnt" = "0" ]; then
    echo "   （仓内无先例 ⇒ 触发一次外部检索；通道与关键词模板见 skill/diagnosis-index.md「外部检索协议」）"
    echo "      结果落 10_docs/_state/外部方案台账.md（未命中也要写进「空结果」栏，避免重复烧 step）"
  else
    hit_any=1
    printf '%s\n' "$out" | while IFS= read -r line; do
      [ -n "$line" ] || continue
      printf '   %s\n' "$(printf '%s' "$line" | cut -c1-180)"
    done
  fi
  [ "$QUIET" = "1" ] || echo
done

echo "提示：命中先例后先读先例再动手；推论冲突时，先解释冲突再推翻先例结论。"
[ "$hit_any" = "1" ] && exit 0 || exit 1
