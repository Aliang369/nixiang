#!/usr/bin/env bash
# ================================================================
#  l3_round.sh — 明日方舟 L3 真人实测「两段制」（2026-10-10 复盘 P1-2）
#
#  为什么两段制：一轮 L3 让用户「七项全开进战斗看效果」，一旦中间发现菜单卡/闪退，
#  前面几项白看、用户白测一轮（本会话就白耗了 2 轮）。改成：
#    段1（≈1 分钟）：冷启 → 摇出菜单 → 逐项开关一次 → **只看菜单卡不卡 / 有无闪退**
#                    判据全在日志里（hooked 数、开关→装钩延迟、ANR/崩溃），不需要看画面。
#    段2（战斗实测）：七项全开进战斗，逐项看画面效果；段1 不过就别进段2。
#  两段各自出证据文件 + 判定，段2 直接闭合轮次台账（有效/无效）。
#
#  用法:
#    bash 04_scripts/明日方舟/l3_round.sh seg1        # 段1：冷启 + 菜单 + 逐项开关（用户手动，回车收尾）
#    bash 04_scripts/明日方舟/l3_round.sh seg2        # 段2：进战斗实测七项（用户手动，回车收尾）
#    bash 04_scripts/明日方舟/l3_round.sh status      # 看最近一次记录仪与 L3 报告
#    选项: --no-launch（段1 不自动冷启）/ --no-ledger（段2 不写轮次台账）/ --dir <记录仪目录>
#
#  判读口径（与 实测判读_mrfzz.sh 的 10 行表对齐；7 个功能对应 10 个挂点）:
#    #1 超级攻速 attack_speed | #2 超级无敌 invincible | #3 基地锁血 base_lock
#    #4 十倍伤害 damage10      | #5 全图放置 deploy_anywhere
#    #6 无限技能 = infinite_sp + infinite_cnt + infinite_usedup
#    #7 零部署费用 = zero_cost_card + zero_cost_token
#  延迟判据用 `ModBridge: Toggle:` → 紧随的 `[mrfzz] hooked`（这两串在交付包里永久保留；
#  `[mrfzz] feature #N` 属诊断层，STARRY_DIAG=0 时会被剥掉，不能当判据）。
#
#  硬规则：脚本只允许 force-stop / monkey 启动与读日志；**不做截图、不做 input tap**，
#          屏幕上的操作一律由用户完成（AGENTS 硬约束）。
#  bash 3.2 兼容
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
JUDGE="${SCRIPT_DIR}/实测判读_mrfzz.sh"
CLASSIFY="${ROOT}/04_scripts/_common/classify_proc_gone.sh"
RECORDER="${ROOT}/04_scripts/_common/flight_recorder.sh"
PIPELINE="${ROOT}/04_scripts/pipeline.sh"
APK_JSON="${SCRIPT_DIR}/official_apk.json"
DEV_JSON="${SCRIPT_DIR}/device_info.json"
GAME="明日方舟"
PKG="com.hypergryph.arknights"; SERIAL="f9a116a2"
if [ -f "$APK_JSON" ]; then
  PKG="$(python3 -c "import json;d=json.load(open('$APK_JSON'));print(d.get('package','') or '$PKG')" 2>/dev/null || echo "$PKG")"
  SERIAL="$(python3 -c "import json;d=json.load(open('$APK_JSON'));print(d.get('serial','') or '$SERIAL')" 2>/dev/null || echo "$SERIAL")"
fi
[ -n "$SERIAL" ] || SERIAL="f9a116a2"

SEG="${1:-}"; shift 2>/dev/null || true
NO_LAUNCH=0; NO_LEDGER=0; FORCE_DIR=""
while [ $# -gt 0 ]; do
  case "$1" in
    --no-launch) NO_LAUNCH=1; shift ;;
    --no-ledger) NO_LEDGER=1; shift ;;
    --dir)       FORCE_DIR="${2:-}"; shift 2 ;;
    -h|--help)   sed -n '2,27p' "$0"; exit 0 ;;
    *) echo "✗ 未知参数: $1" >&2; exit 2 ;;
  esac
done

# 判读表的 10 个日志 id（顺序即判读脚本表序）
FEAT_IDS="attack_speed invincible base_lock damage10 deploy_anywhere infinite_sp infinite_cnt infinite_usedup zero_cost_card zero_cost_token"
# 7 个功能的展示名:其挂点 id（空格分隔，供汇总）
FEAT_ROWS='超级攻速:attack_speed
超级无敌:invincible
基地锁血:base_lock
十倍伤害:damage10
全图放置:deploy_anywhere
无限技能:infinite_sp infinite_cnt infinite_usedup
零部署费用:zero_cost_card zero_cost_token'

LAST_PID_OF() {  # 最后一次 MOD 会话的 pid（与判读脚本同口径：取最后一条 [mrfzz] 行的 pid）
  LC_ALL=C grep -aF '[mrfzz]' "$1" 2>/dev/null | tail -1 | LC_ALL=C awk '{print $3}'
}
COUNT_PID() {  # COUNT_PID <日志> <ERE> <pid> —— 只数指定 pid 的行
  [ -n "$3" ] || { echo 0; return 0; }
  LC_ALL=C grep -aE "$2" "$1" 2>/dev/null | LC_ALL=C awk -v p="$3" '$3 == p' | wc -l | tr -d ' '
}
FIRST_PID() {  # 指定 pid 的首个命中行（给报告留证据）
  [ -n "$3" ] || return 0
  LC_ALL=C grep -aE "$2" "$1" 2>/dev/null | LC_ALL=C awk -v p="$3" '$3 == p { print; exit }'
}
PID_OF_APP() {  # 设备上游戏的 pid（pidof 优先，ps 兜底）
  _p="$(adb -s "$SERIAL" shell pidof -s "$PKG" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
  if [ -z "$_p" ]; then
    _p="$(adb -s "$SERIAL" shell "ps -A | grep ${PKG}" 2>/dev/null | tr -d '\r' | LC_ALL=C awk '{print $2}' | head -1)"
  fi
  echo "$_p"
}
WAIT_PID() {  # 等游戏进程起来（最多 ~40s），打印 pid（可能为空）
  _n=0; _p=""
  while [ "$_n" -lt 20 ]; do
    _p="$(PID_OF_APP)"
    [ -n "$_p" ] && break
    _n=$((_n + 1)); sleep 2
  done
  echo "$_p"
}
PID_SET_FOR_SEG() {  # 本段相关 pid 集合（启动时 pid + 最后 MOD 会话 pid，去重）
  _out=""
  for _p in "$1" "$2"; do
    [ -n "$_p" ] || continue
    case " $_out " in *" $_p "*) ;; *) _out="${_out} ${_p}" ;; esac
  done
  echo "$_out"
}
SUM_OVER_PIDS() {  # SUM_OVER_PIDS <日志> <ERE> <pid 集合> —— 按 pid 求和
  _log="$1"; _ere="$2"; shift 2
  _s=0
  for _p in $*; do
    _c="$(COUNT_PID "$_log" "$_ere" "$_p")"
    [ -n "$_c" ] || _c=0
    _s=$((_s + _c))
  done
  echo "$_s"
}
FIRST_OVER_PIDS() {  # FIRST_OVER_PIDS <日志> <ERE> <pid 集合> —— 首个命中行
  _log="$1"; _ere="$2"; shift 2
  for _p in $*; do
    _l="$(FIRST_PID "$_log" "$_ere" "$_p")"
    [ -n "$_l" ] && { echo "$_l"; return 0; }
  done
}
COUNT_SESS() {  # COUNT_SESS <日志> <字面串> <pid> —— 只看最后一次会话（同 pid）
  _log="$1"; _needle="$2"; _pid="$3"
  [ -n "$_pid" ] || { echo 0; return 0; }
  LC_ALL=C grep -aF -- "$_needle" "$_log" 2>/dev/null | LC_ALL=C awk -v p="$_pid" '$3 == p' | wc -l | tr -d ' '
}
FEAT_HITS() {  # FEAT_HITS <日志> <pid> <功能序号> —— 该功能的 hit 次数（多挂点求和）
  _log="$1"; _pid="$2"; _n="$3"
  _row="$(printf '%s\n' "$FEAT_ROWS" | sed -n "${_n}p")"
  _ids="${_row#*:}"
  _sum=0
  for _id in $_ids; do
    _c="$(COUNT_SESS "$_log" "[mrfzz] hit ${_id}" "$_pid")"
    [ -n "$_c" ] || _c=0
    _sum=$((_sum + _c))
  done
  echo "$_sum"
}

rec_dir_of() {  # stdout 只输出目录（消息一律走 stderr）
  if [ -n "$FORCE_DIR" ]; then echo "$FORCE_DIR"; return 0; fi
  for m in $(ls -1dt "${ROOT}"/09_test/_recorder/mrfzz_* 2>/dev/null); do
    if [ -f "$m/recorder.meta" ]; then
      sup="$(sed -n 's/^SUP=\([0-9]\{1,\}\).*/\1/p' "$m/recorder.meta" | head -1)"
      if [ -n "$sup" ] && kill -0 "$sup" 2>/dev/null; then echo "$m"; return 0; fi
    fi
  done
  d="${ROOT}/09_test/_recorder/mrfzz_$(date '+%Y%m%d_%H%M')_l3${SEG}"
  n=1
  while [ -d "$d" ]; do d="${ROOT}/09_test/_recorder/mrfzz_$(date '+%Y%m%d_%H%M')_l3${SEG}_${n}"; n=$((n + 1)); done
  echo "$d"
}

ensure_recorder() {  # stdout 只输出目录；失败 return 1（消息走 stderr）
  d="$(rec_dir_of)"
  # 1) 显式 --dir：直接用（已停也允许，只告警 —— 用于复盘与干跑）
  if [ -n "$FORCE_DIR" ]; then
    [ -f "$d/logcat.txt" ] || { echo "✗ --dir 指定的目录里没有 logcat.txt: $d" >&2; return 1; }
    echo "[记录仪] --dir 指定：${d}（已停也按原样判读）" >&2
    echo "$d"; return 0
  fi
  # 2) 已有在录的记录仪 → 复用
  if [ -f "$d/logcat.txt" ]; then
    sup="$(sed -n 's/^SUP=\([0-9]\{1,\}\).*/\1/p' "$d/recorder.meta" 2>/dev/null | head -1)"
    if [ -n "$sup" ] && kill -0 "$sup" 2>/dev/null; then echo "$d"; return 0; fi
  fi
  # 3) 新开录
  mkdir -p "$d"
  echo "▶ 启动飞行记录仪 → $d" >&2
  if ! bash "$RECORDER" start --pkg "$PKG" --serial "$SERIAL" --out "$d" >/dev/null 2>&1; then
    echo "✗ 记录仪启动失败（$RECORDER start --pkg $PKG --serial $SERIAL --out ${d}）" >&2
    return 1
  fi
  sleep 2
  echo "$d"
}

TOGGLE_LAG_MS() {  # 开关→装钩 最大延迟（同会话；窗口 30s）
  python3 - "$1" <<'PY'
import re, sys
try:
    lines = open(sys.argv[1], encoding='utf-8', errors='replace').read().splitlines()
except Exception:
    print('-'); raise SystemExit
pid = None
for ln in lines:
    if '[mrfzz]' in ln:
        f = ln.split()
        if len(f) > 2: pid = f[2]
def t(s):
    m = re.match(r'(\d\d)-(\d\d) (\d\d):(\d\d):(\d\d)\.(\d\d\d)', s)
    return (int(m.group(3))*3600 + int(m.group(4))*60 + int(m.group(5)))*1000 + int(m.group(6)) if m else None
tog = None; maxd = 0; seen = 0
for l in lines:
    f = l.split()
    if pid and f[2:3] != [pid]:
        continue
    if 'ModBridge: Toggle:' in l:
        tog = t(l)
    elif '[mrfzz] hooked ' in l and tog is not None:
        cur = t(l)
        if cur is not None:
            d = cur - tog
            if 0 <= d <= 30000:
                if d > maxd: maxd = d
                seen += 1
        tog = None
print(maxd if seen else '-')
PY
}

judge_all() {  # judge_all <dir> <报告文件>
  d="$1"; rep="$2"
  {
    echo "== L3 段判读 =="
    echo "时间: $(date '+%Y-%m-%d %H:%M:%S')   记录仪: $d   包: $PKG   设备: $SERIAL"
    echo
    echo "---- 装钩/命中（实测判读，按 pid 切会话）----"
    bash "$JUDGE" "$d" 2>&1
    echo
    echo "---- 进程消失定性（ANR / native 崩溃 / 主动停止）----"
    echo "（注意：这里是**整个记录仪目录**的次数与定性；「本段」口径见段落末尾的判定块）"
    bash "$CLASSIFY" "$d" --pkg "$PKG" 2>&1
  } > "$rep" 2>&1
  cat "$rep"
}

# ---------------- status ----------------
if [ "$SEG" = "status" ]; then
  echo "== 记录仪 =="
  bash "$RECORDER" status 2>&1 | tail -6 || true
  echo
  echo "== 最近 L3 报告 =="
  if ls -1t "${ROOT}"/09_test/明日方舟/l3_*.txt >/dev/null 2>&1; then
    ls -1t "${ROOT}"/09_test/明日方舟/l3_*.txt | head -3
  else
    echo "（还没有 L3 报告）"
  fi
  exit 0
fi

[ "$SEG" = "seg1" ] || [ "$SEG" = "seg2" ] || { echo "用法: l3_round.sh seg1|seg2|status（--help 看说明）" >&2; exit 2; }
[ "$(adb -s "$SERIAL" get-state 2>/dev/null || true)" = "device" ] || { echo "✗ 设备 ${SERIAL} 不在线" >&2; exit 1; }
mkdir -p "${ROOT}/09_test/明日方舟"
REC="$(ensure_recorder)" || exit 1
[ -n "$REC" ] && [ -d "$REC" ] || { echo "✗ 拿不到可用的记录仪目录（原因见上）" >&2; exit 1; }
TS="$(date '+%Y%m%d_%H%M')"
REP="${ROOT}/09_test/明日方舟/l3_${SEG}_${TS}.txt"
LOG="$REC/logcat.txt"

if [ "$SEG" = "seg1" ]; then
  if [ "$NO_LAUNCH" = "0" ]; then
    echo "▶ 冷启游戏（force-stop + monkey）"
    adb -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true
    sleep 3
    adb -s "$SERIAL" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
  fi
  cat <<'EOF'

════════ 段1 请用户操作（约 1 分钟，只看菜单，不用进战斗）════════
 1. 等游戏进到主界面（可停在公告/主页任意界面）
 2. 摇一摇手机唤出 MOD 菜单
 3. 把七个开关**逐个**打开（每开一个停 1~2 秒，看菜单是否卡住/无响应）
 4. 再逐个关掉（同样留意卡顿）
 5. 全程不要强退游戏
════════════════════════════════════════════════════════════
EOF
  if [ "$NO_LAUNCH" = "0" ]; then P0="$(WAIT_PID)"; else P0="$(PID_OF_APP)"; fi
  echo "[设备] 启动时游戏 pid = ${P0:-（没等到进程，可能还没起来）}"
  printf '完成后回车（输入 q 放弃本次记录）: '; read -r ans
  case "$ans" in q|Q) echo "已放弃（记录仪继续在录：${REC}）"; exit 0 ;; esac

  judge_all "$REC" "$REP"

  PID="$(LAST_PID_OF "$LOG")"
  PIDS="$(PID_SET_FOR_SEG "$P0" "$PID")"
  HOOKED="$(COUNT_SESS "$LOG" '[mrfzz] hooked ' "$PID")"
  MENU="$(COUNT_SESS "$LOG" 'MenuService: overlayVisible=true' "$PID")"
  DOBBY="$(COUNT_SESS "$LOG" 'dobby fail' "$PID")"
  # 只数本段相关 pid 的行 —— 记录仪跨会话累加，全量计数会把历次的 ANR/崩溃算进来
  ANR="$(SUM_OVER_PIDS "$LOG" 'ANR in |Input dispatching timed out' $PIDS)"
  CRASH="$(SUM_OVER_PIDS "$LOG" 'Fatal signal|tombstone' $PIDS)"
  ANR_EV="$(FIRST_OVER_PIDS "$LOG" 'ANR in |Input dispatching timed out' $PIDS)"
  CRASH_EV="$(FIRST_OVER_PIDS "$LOG" 'Fatal signal|tombstone' $PIDS)"
  LAG="$(TOGGLE_LAG_MS "$LOG")"
  RESTART=""
  # 重启判据只在「本段是我们自己冷启的」时成立（--no-launch / --dir 复盘时 P0 是当前无关进程）
  if [ "$NO_LAUNCH" = "0" ] && [ -n "$P0" ] && [ -n "$PID" ] && [ "$P0" != "$PID" ]; then
    RESTART="是（启动 pid ${P0} ≠ 最后 MOD 会话 pid ${PID} ⇒ 期间进程重启过，等于死过一次）"
  fi

  {
    echo
    echo "---- 段1 判定（菜单卡顿 / 闪退 只看这里）----"
    echo "  本段相关 pid                  : ${P0:-?} / ${PID:-（无 [mrfzz] 行 ⇒ MOD 根本没起）}"
    echo "  检测到进程重启                : ${RESTART:-否}"
    echo "  本会话 hooked 行数            : ${HOOKED}（十处挂点期望 10；get_cost 占两条）"
    echo "  菜单弹出 overlayVisible=true  : $MENU"
    echo "  dobby fail 行数               : $DOBBY"
    echo "  开关→装钩最大延迟(ms)         : ${LAG}（>1500ms 视为卡顿风险）"
    echo "  本段 ANR 行数                 : $ANR"
    echo "  本段 native 崩溃行数          : $CRASH"
    [ -n "$ANR_EV" ] && echo "    ANR 证据: $ANR_EV"
    [ -n "$CRASH_EV" ] && echo "    崩溃证据: $CRASH_EV"
    if [ "$ANR" -eq 0 ] && [ "$CRASH" -eq 0 ] && [ -z "$RESTART" ] && [ "$HOOKED" -ge 1 ] && [ "$MENU" -ge 1 ] && { [ "$LAG" = "-" ] || [ "$LAG" -le 1500 ]; }; then
      echo "  结论: ✅ 段1 通过 —— 可以进段2：bash 04_scripts/明日方舟/l3_round.sh seg2"
    else
      echo "  结论: ❌ 段1 未通过 —— 先修，别让用户进战斗"
      [ -n "$RESTART" ] && echo "        · 进程重启 ⇒ 游戏死过一次（看上面 classify_proc_gone 对该 pid 的定性）"
      [ "$MENU" -lt 1 ] && echo "        · 菜单没弹出 ⇒ 用户没摇到菜单，或 MenuService 未在前台（先看 post_install_precheck）"
      [ "$HOOKED" -lt 1 ] && echo "        · hooked=0 ⇒ 装钩整层没跑（看报告里的失败行与 lib ready）"
      [ "$ANR" -gt 0 ] && echo "        · ANR ⇒ 装钩阻塞了 UI 线程（Dobby 安装必须留在 worker）"
      [ "$CRASH" -gt 0 ] && echo "        · 崩溃 ⇒ 看上面 classify_proc_gone 的定性首行"
      [ "$LAG" != "-" ] && [ "$LAG" -gt 1500 ] && echo "        · 延迟 ${LAG}ms ⇒ 开关回调里同步装钩（应只置标志、由 worker 装）"
    fi
  } | tee -a "$REP"
  echo
  echo "报告: $REP"
  exit 0
fi

# ---------------- seg2 ----------------
cat <<'EOF'

════════ 段2 请用户操作（战斗实测七项）════════
 1. 进一局能出怪的关卡（随便普通关；能放干员、能开技能）
 2. 七项开关全部保持「开」
 3. 逐项看效果：攻速↑ / 干员无敌 / 基地不掉血 / 伤害×10 / 任意位置放人 / 技能无限 /
    部署费用 0（放人后费用不减少）
 4. 期间不要强退游戏；有异常（卡住/闪退/数值不对）记下是哪个功能
════════════════════════════════════════════
EOF
P0="$(PID_OF_APP)"
printf '实测完成后回车（输入 q 放弃本次记录）: '; read -r ans
case "$ans" in q|Q) echo "已放弃（记录仪继续在录：${REC}）"; exit 0 ;; esac

judge_all "$REC" "$REP"

PID="$(LAST_PID_OF "$LOG")"
PIDS="$(PID_SET_FOR_SEG "$P0" "$PID")"
S2_ANR="$(SUM_OVER_PIDS "$LOG" 'ANR in |Input dispatching timed out' $PIDS)"
S2_CRASH="$(SUM_OVER_PIDS "$LOG" 'Fatal signal|tombstone' $PIDS)"
S2_ANR_EV="$(FIRST_OVER_PIDS "$LOG" 'ANR in |Input dispatching timed out' $PIDS)"
S2_CRASH_EV="$(FIRST_OVER_PIDS "$LOG" 'Fatal signal|tombstone' $PIDS)"
echo
echo "---- 段2 本段稳定性（相关 pid:${PIDS}）----"
echo "  ANR 行数: ${S2_ANR}    native 崩溃行数: ${S2_CRASH}"
[ -n "$S2_ANR_EV" ] && echo "    ANR 证据: ${S2_ANR_EV}"
[ -n "$S2_CRASH_EV" ] && echo "    崩溃证据: ${S2_CRASH_EV}"
echo "  （一个能放干员、能开技能的本，每项都该有 hit；hit=0 说明该局没用到，不是「装上但无效」的铁证）"
{
  echo
  echo "---- 段2 本段稳定性（相关 pid:${PIDS}）----"
  echo "  ANR 行数: ${S2_ANR}    native 崩溃行数: ${S2_CRASH}"
  [ -n "$S2_ANR_EV" ] && echo "    ANR 证据: ${S2_ANR_EV}"
  [ -n "$S2_CRASH_EV" ] && echo "    崩溃证据: ${S2_CRASH_EV}"
} >> "$REP"
echo
echo "---- 段2 逐项命中情况（判读表 10 行；pid=${PID:-无}）----"
for k in $FEAT_IDS; do
  line="$(LC_ALL=C grep -aE "^[[:space:]]+${k}[[:space:]]" "$REP" 2>/dev/null | head -1)"
  [ -n "$line" ] || line="（判读未输出该行）"
  printf '  %-18s %s\n' "$k" "$line"
done

echo
echo "---- 七功能汇总（hit = 游戏侧真实调用次数）----"
_i=0
printf '%s\n' "$FEAT_ROWS" | while IFS= read -r row; do
  [ -n "$row" ] || continue
  _i=$((_i + 1))
  name="${row%%:*}"
  hits="$(FEAT_HITS "$LOG" "$PID" "$_i")"
  if [ "$hits" -gt 0 ]; then printf '  #%d %-10s ✅ 已触发 %d 次\n' "$_i" "$name" "$hits"
  else printf '  #%d %-10s ⚠ 本会话未触发（该局没用上，或函数没被调用到）\n' "$_i" "$name"; fi
done

echo
printf '七项是否都在画面上生效？(y=全部有效 / n=有无效项 / s=跳过不登记): '
read -r verdict
NOTE="L3 段2 实测（两段制）；报告 ${REP##*/}；$(LC_ALL=C grep -a '本会话 hooked 行数' "$REP" | head -1 | sed 's/^[[:space:]]*//')"
case "$verdict" in
  y|Y)
    { echo; echo "---- 用户判定：七项全部有效（$(date '+%Y-%m-%d %H:%M:%S')）----"; } >> "$REP"
    if [ "$NO_LEDGER" = "0" ]; then
      for i in 1 2 3 4 5 6 7; do
        bash "$PIPELINE" "$GAME" round "$i" 有效 "$NOTE" 2>&1 | sed 's/^/  /'
      done
    else
      echo "  （--no-ledger：未写轮次台账）"
    fi
    echo "✅ 段2 通过并已登记轮次台账（有效）。"
    ;;
  n|N)
    { echo; echo "---- 用户判定：存在无效项（$(date '+%Y-%m-%d %H:%M:%S')）----"; } >> "$REP"
    printf '哪几项无效？（填功能序号，空格分隔；直接回车＝七项全记无效）: '
    read -r bad
    [ -n "$bad" ] && echo "无效项序号: ${bad}" >> "$REP"
    if [ "$NO_LEDGER" = "0" ]; then
      for i in 1 2 3 4 5 6 7; do
        v="有效"
        if [ -z "$bad" ]; then v="无效"
        else
          for b in $bad; do [ "$b" = "$i" ] && v="无效"; done
        fi
        bash "$PIPELINE" "$GAME" round "$i" "$v" "$NOTE" 2>&1 | sed 's/^/  /'
      done
    fi
    echo "⚠ 段2 未全过：已按上面的判定登记。连着 2 轮无效会被 pre_install_gate 查6 拦住（需先写根因定案）。"
    echo "   参考: bash 04_scripts/_common/rootcause_note.sh --game ${GAME} --feature <N> --round <R> --cause '<根因>' --evidence '<证据行>'"
    exit 1
    ;;
  *)
    echo "已跳过登记（报告仍保留：${REP}）"
    ;;
esac
echo "报告: $REP"
exit 0
