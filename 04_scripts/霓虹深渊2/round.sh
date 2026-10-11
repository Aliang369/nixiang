#!/usr/bin/env bash
# ================================================================
#  霓虹深渊2 一键轮次环（2026-10-12 会话效率复盘 P1-10）
#
#  用法:
#    bash 04_scripts/霓虹深渊2/round.sh <标签> [--loop N] [--dry-run]
#      <标签>      本轮标签，如 r18 / r18b（写进 pipeline.json 的 params.version）
#      --loop N    连跑最多 N 个阶段（默认 9 = 全部；失败即停）
#      --dry-run   只改 params 并打印将执行的命令，不真跑
#
#  为什么要有它（那一轮的实测代价）:
#    霓虹深渊2 会话里「改码 → 编 so → 签名 → 装机 → 校验」全靠手敲，
#    单轮链路里 ndk-build 43 次、python 外科 74 次、apksigner 13 次、zipalign 4 次、
#    手敲 adb install 3 次，而 pipeline.sh 的阶段机只跑了 2 次 gradle_cycle
#    ⇒ 单轮 130+ 条命令、26 轮里 10 轮在做同一件事的返工。
#
#  铁律（pipeline.sh 头部 #1/#4）:
#    · 机械操作（拆/打/注/移/签/装/清理）唯一合法入口 = 04_scripts/pipeline.sh；
#    · 合法参数修改走 04_scripts/{游戏}/pipeline.json 的 params。
#    本脚本**只做**：① 改 params.version 标签 ② 转发 pipeline.sh ③ 打印回写命令。
#    它自己**不碰** adb / apksigner / ndk-build（那会绕过闸门）。
#
#  退出码: 0=全链成功；其它=转发 pipeline.sh 的退出码（3=被轮次门禁拦，属正常拦截）
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
GAME="霓虹深渊2"
PJ="$SCRIPT_DIR/pipeline.json"
PIPE="$ROOT/04_scripts/pipeline.sh"

TAG=""; LOOP=9; DRY=0
while [ $# -gt 0 ]; do
  case "$1" in
    --loop) LOOP="${2:-9}"; shift 2 ;;
    --dry-run) DRY=1; shift ;;
    -h|--help) sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    -*) echo "未知参数: $1" >&2; exit 2 ;;
    *) TAG="$1"; shift ;;
  esac
done
[ -n "$TAG" ] || { echo "用法: bash 04_scripts/霓虹深渊2/round.sh <标签> [--loop N] [--dry-run]" >&2; exit 2; }
[ -f "$PJ" ] || { echo "✗ 缺 ${PJ}（轮次门禁与装机策略都靠它）" >&2; exit 2; }

case "$TAG" in
  *[!A-Za-z0-9._-]*) echo "✗ 标签只允许字母数字._- : $TAG" >&2; exit 2 ;;
esac

echo "▶ 轮次标签：$TAG"

# ---------- ① 改 params（唯一允许的参数修改面）----------
python3 - "$PJ" "$TAG" <<'PY'
import json,sys,collections
pj,tag=sys.argv[1],sys.argv[2]
j=json.load(open(pj,encoding='utf-8'),object_pairs_hook=collections.OrderedDict)
old=j.setdefault('params',{}).get('version','(无)')
j['params']['version']=tag
# 维护 version_code：保留原尾部源码指纹（末段 ≥8 位），version 段换成新标签
vc=j['params'].get('version_code','')
parts=vc.split('.')
tail = parts[-1] if len(parts) >= 3 and len(parts[-1]) >= 8 else ''
j['params']['version_code'] = f"{tag}.{tail}" if tail else tag
json.dump(j,open(pj,'w',encoding='utf-8'),ensure_ascii=False,indent=2)
print(f"  params.version: {old} → {tag}")
PY

# ---------- ② 转发 pipeline.sh（机械操作唯一入口）----------
CMD="bash $PIPE $GAME next --loop $LOOP"
echo "▶ 执行：$CMD"
if [ "$DRY" = "1" ]; then
  echo "  （--dry-run：不真跑）"
  exit 0
fi
bash "$PIPE" "$GAME" next --loop "$LOOP"
RC=$?

# ---------- ③ 出口：把「用户实测后必须回写」的命令摆出来 ----------
echo
echo "----------------------------------------------------------------"
if [ "$RC" = "3" ]; then
  echo "⛔ rc=3：被轮次门禁拦住 —— 说明同一功能已连续 3 轮无效。"
  echo "   现在**不许**再换一个自变量装同一轮包，先出假设矩阵："
  echo "     bash $PIPE $GAME rounds"
  echo "   填完贴给用户确认方向；仅当用户当轮明确说「再试一次」才可用 STAGE_NEXT_FORCE=1。"
else
  echo "✅ 链路 rc=${RC}。装机后立刻跑装机层预检（P10）："
  echo "     bash $ROOT/04_scripts/_common/post_install_precheck.sh --game $GAME --markers $ROOT/10_docs/$GAME/预检标记.txt"
  echo "   用户 L3 实测回执之后，**必须**回写一行（不回写 ⇒ 轮次门禁永久失明）："
  echo "     bash $PIPE $GAME round <功能#> <有效|无效> '<一句话摘要>（用户 mXXXXX）'"
  echo "   功能# 编号见 10_docs/$GAME/功能总表_合并版.md §0.1。"
fi
echo "----------------------------------------------------------------"
exit "$RC"
