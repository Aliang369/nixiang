#!/usr/bin/env bash
# fkssj_battle_report.sh — 疯狂水世界 战斗取证汇总（L3 人工实测的机器侧收口）
#
# 为什么需要它：
#   本作胜负「分模式」判定（主线听服务端 stats.isLeftWin，其余模式听客户端 victorCamp），
#   所以「用户说好不好用」必须先变成一张表：哪个模式、客户端判什么、服务端判什么、
#   弹的是哪个窗、打了几帧/几秒。锚点#4（BattleInterfaceBase._onBattleEnd）每场回传一行
#   `battleEnd type=<模式> clientWin=<0/1> serverWin=<0/1/-1> popup=<WIN/FAIL/?> frame=…
#    fightTime=…/maxFightTime=…`，本脚本把它抽成表并给出结论提示。
#
# 用法：
#   bash 04_scripts/疯狂水世界/fkssj_battle_report.sh            # 用 .device_serial，清缓冲后等回车再抓
#   bash 04_scripts/疯狂水世界/fkssj_battle_report.sh --dump     # 只抓现有缓冲（不清、不等）
#   bash 04_scripts/疯狂水世界/fkssj_battle_report.sh --serial 96f0d883 --dump
#
# 产出：09_test/疯狂水世界/battle_<时间戳>.log（原始日志）+ 终端表格

set -u

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
GAME="疯狂水世界"
PKG="com.gx.sw.qa.fkssj004.esj"
SERIAL=""
DUMP_ONLY=0

while [ $# -gt 0 ]; do
  case "$1" in
    --serial) SERIAL="${2:-}"; shift 2 ;;
    --dump)   DUMP_ONLY=1; shift ;;
    -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

if [ -z "${SERIAL}" ]; then
  SER=""
  if [ -f "${NIXIANG}/04_scripts/${GAME}/.device_serial" ]; then
    SER="$(grep -m1 '^serial=' "${NIXIANG}/04_scripts/${GAME}/.device_serial" | cut -d= -f2 | tr -d ' \r')"
  fi
  if [ -z "${SER}" ]; then
    SER="$(adb devices | awk 'NR>1 && $2=="device"{print $1; exit}')"
  fi
  SERIAL="${SER}"
fi

if [ -z "${SERIAL}" ]; then
  echo "ERROR: 未找到设备（未设 ANDROID_SERIAL、无 .device_serial、adb devices 也无 device）" >&2
  exit 1
fi

echo "════ 疯狂水世界 战斗取证 ════"
echo "serial = ${SERIAL}"

if [ "${DUMP_ONLY}" = "0" ]; then
  echo "▶ 清空 logcat 缓冲（保留后续取证）"
  adb -s "${SERIAL}" logcat -c || true
  echo ""
  echo "【操作请求卡】"
  echo "  请你做什么：进游戏打 2~3 场战斗（建议 ① 主线推图一场 ② 英雄副本/试炼/竞技场任一场）"
  echo "              打完回到主城即可，不需要截图"
  echo "  成功标志：每场结束都能看到胜负弹窗（赢或输都算成功，我们要的就是结果）"
  echo "  我接着看什么：本脚本抽出的 battleEnd 表（模式 / 客户端判 / 服务端判 / 弹窗 / 时长）"
  echo ""
  printf "  打完后按回车继续抓取…"
  read -r _ || true
fi

TAGS="StarryTrace:I Mod_Menu:I NativeMenu:I MenuService:I ModBridge:I Debug:I AndroidRuntime:E"
OUTDIR="${NIXIANG}/09_test/${GAME}"
mkdir -p "${OUTDIR}"
TS="$(date +%Y%m%d_%H%M%S)"
OUT="${OUTDIR}/battle_${TS}.log"

adb -s "${SERIAL}" logcat -d -v time -s ${TAGS} > "${OUT}" 2>&1 || true

TOTAL_LINES="$(wc -l < "${OUT}" | tr -d ' ')"
BE_LINES="$(grep -c 'battleEnd' "${OUT}" 2>/dev/null || true)"
[ -z "${BE_LINES}" ] && BE_LINES=0

echo ""
echo "▶ 原始日志 ${TOTAL_LINES} 行 → ${OUT}"
echo "▶ battleEnd 行数：${BE_LINES}"

# ---- 补丁挂载自检 ----
echo ""
echo "── 补丁挂载自检 ──"
for k in "anchor#1 applyDamage hooked" "anchor#2 checkDeath hooked" "anchor#3 battle speed hooked" "anchor#4 battle-end forensics hooked" "all 3 anchors armed"; do
  n="$(grep -c -F "${k}" "${OUT}" 2>/dev/null || true)"
  [ -z "${n}" ] && n=0
  if [ "${n}" -gt 0 ]; then echo "  ✓ ${k}"; else echo "  ✗ ${k}（未命中）"; fi
done
echo "  · cfg 推送：$(grep -o 'js cfg push inv=[0-9]* dmg=[0-9]* spd=[0-9]*' "${OUT}" 2>/dev/null | tail -1)"

if [ "${BE_LINES}" = "0" ]; then
  echo ""
  echo "⚠ 本次没有抓到任何 battleEnd —— 说明这几场战斗没走到 _onBattleEnd，或补丁未挂/被打断。"
  echo "  排查：① 确认包内 so 是最新（fkssj_redeploy.sh 闭环 md5）② 确认打过一场完整战斗到出弹窗"
  echo "  ③ 菜单若战斗加速=1、无敌关、倍率=1 也要照样出 battleEnd（它是只读取证，与开关无关）"
  exit 0
fi

# ---- 解析成表 ----
echo ""
echo "── battleEnd 明细（模式 / 客户端判 / 服务端判 / 弹窗 / 帧 / 时长） ──"
printf '%-10s %-9s %-10s %-7s %-12s %s\n' "模式" "客户端" "服务端" "弹窗" "帧" "fightTime/max"

awk '
/battleEnd/ {
  line = $0
  type=""; cw=""; sw=""; pop=""; fr=""; ftt=""
  if (match(line, /type=[A-Za-z0-9_?]+/))    { type = substr(line, RSTART+5, RLENGTH-5) }
  if (match(line, /clientWin=[0-9-]+/))      { cw   = substr(line, RSTART+10, RLENGTH-10) }
  if (match(line, /serverWin=-?[0-9]+/))     { sw   = substr(line, RSTART+10, RLENGTH-10) }
  if (match(line, /popup=[A-Z?]+/))          { pop  = substr(line, RSTART+6, RLENGTH-6) }
  if (match(line, /frame=[0-9]+/))           { fr   = substr(line, RSTART+6, RLENGTH-6) }
  if (match(line, /fightTime=[0-9]+\/[0-9]+/)){
      t = substr(line, RSTART+10, RLENGTH-10)
      split(t, a, "/"); ftt = a[1] "/" a[2]
  }
  cwt = (cw=="1") ? "胜" : ((cw=="0") ? "负" : "-")
  swt = (sw=="1") ? "胜" : ((sw=="0") ? "负" : ((sw=="-1") ? "无字段" : "-"))
  if (pop=="WIN") popt="WIN"; else if (pop=="FAIL") popt="FAIL"; else popt=pop
  printf "%-10s %-9s %-10s %-7s %-12s %s\n", type, cwt, swt, popt, fr, ftt
  n[type]++
  if (type=="MAINLINE") { m++
    if (sw=="0" && cw=="1") mismatch++          # 服务端判负、客户端演出判胜 ⇒ 用户看到的「打赢了却失败」
    if (sw=="1") swWin++
    if (fr+0 > 1800) longrun++                  # 帧数异常大 = 无敌把演出拖到超时兜底
  }
}
END {
  print ""
  print "── 每个模式场次 ──"
  for (t in n) printf "  %-10s %d 场\n", t, n[t]
  if (m > 0) {
    print ""
    print "── 主线结论 ──"
    printf "  主线共 %d 场：服务端判胜 %d 场，服务端判负而演出判胜 %d 场（即「打赢了却显示失败」）\n", m, swWin+0, mismatch+0
    if (longrun+0 > 0) printf "  其中 %d 场 frame>1800（无敌把演出拖到超时兜底，建议调大战斗加速）\n", longrun+0
  }
}
' "${OUT}"
