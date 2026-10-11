#!/usr/bin/env bash
# ================================================================
#  rootcause_note.sh — 「根因定案」登记与闸门（2026-10-10 复盘 P1-3）
#
#  背景：同一功能连续多轮「无效」时，最容易发生的事是**继续瞎改一个自变量再装一次包**
#        （本轮明日方舟因此白耗 2 轮 + 用户 2 次实测）。定下来的规矩：
#          · 某功能连续 ≥2 轮「无效」⇒ 装机前必须有一条**根因定案**（本脚本登记），
#            定案轮次必须 ≥ 最后一次「无效」的轮次（即定案要在那轮之后做出）；
#          · 没有定案就装机 ⇒ 由 `pre_install_gate.sh --game <游戏>` 的「查6 轮次根因关」拦下；
#          · 强行绕过只允许 `STAGE_NEXT_FORCE=1`，且必须在交班/台账里写明为何绕过。
#        定案必须指向**具体判据/代码行**（不是「可能是 XX 问题」）；诊断与修复可同包，但
#        定案一句话 + 它的证据行必须落盘，让下一轮不必重走。
#
#  用法:
#    # 登记定案（--feature 可重复或用 all；--evidence 可重复）
#    bash 04_scripts/_common/rootcause_note.sh --game 明日方舟 --feature all --round 2 \
#         --cause "ImageOf 第二道守卫 detail::IsReadableRange 不清 0xb4 顶字节标签，静默返回 nullptr" \
#         --evidence "Unity/Il2CppApi.cpp:1002" \
#         --evidence "logcat: calib: guard2 raw=0 untag=1" \
#         --verify "calib 标定行：anchor heap=1 read=1 live=1" [--dry-run]
#    bash ... --list                 # 打印现有定案
#    bash ... --check --game 明日方舟 # 只检查闸门（pre_install_gate 查6 复用；exit 1=被拦）
#
#  参数: --game <游戏>  --feature <N|all>  --round <N>  --cause <一句话>  --evidence <行>
#        --verify <方法>  [--threshold N=2]  [--dry-run]  [--list]  [--check]
#  退出码: 0=通过/已登记；1=闸门拦截或参数/文件错
#  bash 3.2 兼容
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
ROOT="${RCS_ROOT:-$ROOT}"   # 仅供 --selfcheck 夹具用（正常调用不要设）

GAME=""; FEATURES=""; ROUND=""; CAUSE=""; EVIDENCE=""; VERIFY=""; THRESHOLD=2
MODE="note"

while [ $# -gt 0 ]; do
  case "$1" in
    --game)      GAME="${2:-}"; shift 2 ;;
    --feature)   FEATURES="$FEATURES ${2:-}"; shift 2 ;;
    --round)     ROUND="${2:-}"; shift 2 ;;
    --cause)     CAUSE="${2:-}"; shift 2 ;;
    --evidence)  EVIDENCE="${EVIDENCE}
${2:-}"; shift 2 ;;
    --verify)    VERIFY="${2:-}"; shift 2 ;;
    --threshold) THRESHOLD="${2:-2}"; shift 2 ;;
    --dry-run)   MODE="dry"; shift ;;
    --list)      MODE="list"; shift ;;
    --check)     MODE="check"; shift ;;
    --selfcheck) MODE="selfcheck"; GAME="${GAME:-（自检）}"; shift ;;
    -h|--help)   sed -n '2,32p' "$0"; exit 0 ;;
    *) echo "✗ 未知参数: $1（--help 看用法）" >&2; exit 1 ;;
  esac
done

# ================================================================
#  --selfcheck：闸门自身的夹具自检（三组：连续无效无定案 → 拦；有定案 → 放；无效后有效 → 放）
#    口径错一次就会「假通过」或「白拦」，所以闸门必须自带夹具。
# ================================================================
if [ "$MODE" = "selfcheck" ]; then
  SC="$(mktemp -d /tmp/rootcause_selfcheck.XXXXXX)" || exit 1
  SC_FAIL=0
  mkfix() {  # mkfix <目录名> <轮次/结果序列，如 "1:无效 2:无效">
    d="$SC/10_docs/$1"; mkdir -p "$d"
    {
      echo "# 轮次台账（夹具）"; echo "# 门禁规则（夹具）"
      printf '日期\t功能#\t轮次\t改动摘要\t装机结果\t用户结果\n'
      i=0
      for pair in $2; do
        i=$((i + 1))
        printf '2026-10-10 10:0%d\t%s\t%s\t夹具\tpipeline.log\t%s\n' "$i" "$3" "${pair%%:*}" "${pair##*:}"
      done
    } > "$d/轮次台账.tsv"
  }
  # 组1 连续 2 轮无效、无定案 ⇒ 期望被拦（rc=1）
  mkfix 拦  "1:无效 2:无效" 1
  RCS_ROOT="$SC" bash "$0" --check --game 拦 > "$SC/o1" 2>&1; RC1=$?
  [ "$RC1" = "1" ] || { echo "✗ 自检失败[组1]：连续 2 轮无效无定案应被拦（rc=1）实为 $RC1"; SC_FAIL=1; }
  [ "$SC_FAIL" = "0" ] && echo "✓ 自检[组1] 连续 2 轮无效 + 无定案 ⇒ 拦截"
  # 组2 同上但补一条定案（轮次 3 ≥ 最后无效轮 2）⇒ 期望放行（rc=0）
  mkfix 放  "1:无效 2:无效" 1
  {
    echo "# 根因定案（夹具）"; echo "# 规则（夹具）"
    printf '日期\t功能#\t定案轮次\t根因\t证据行\t验证方法\t记录人\n'
    printf '2026-10-10 11:00\t1\t3\t夹具根因\t夹具:1\t夹具命令\tAI\n'
  } > "$SC/10_docs/放/根因定案.tsv"
  RCS_ROOT="$SC" bash "$0" --check --game 放 > "$SC/o2" 2>&1; RC2=$?
  [ "$RC2" = "0" ] || { echo "✗ 自检失败[组2]：有定案应放行（rc=0）实为 $RC2"; sed 's/^/    /' "$SC/o2"; SC_FAIL=1; }
  [ "$SC_FAIL" = "0" ] && echo "✓ 自检[组2] 连续 2 轮无效 + 有定案（轮次 3）⇒ 放行"
  # 组3 无效→有效（尾部连续无效 1 < 2）⇒ 期望放行
  mkfix 转 "1:无效 2:有效" 1
  RCS_ROOT="$SC" bash "$0" --check --game 转 > "$SC/o3" 2>&1; RC3=$?
  [ "$RC3" = "0" ] || { echo "✗ 自检失败[组3]：尾部连续无效 1 轮应放行（rc=0）实为 $RC3"; SC_FAIL=1; }
  [ "$SC_FAIL" = "0" ] && echo "✓ 自检[组3] 无效→有效（尾部连续无效 1）⇒ 放行"
  rm -rf "$SC"
  echo "────────────────────────────────"
  [ "$SC_FAIL" -gt 0 ] && { echo "❌ 轮次根因闸门自检失败。"; exit 1; }
  echo "✅ 轮次根因闸门自检通过（拦/放/转 三组）。"
  exit 0
fi

[ -n "$GAME" ] || { echo "✗ 必须给 --game（如 --game 明日方舟）" >&2; exit 1; }

DOC="$ROOT/10_docs/$GAME"
LEDGER="$DOC/轮次台账.tsv"
NOTE="$DOC/根因定案.tsv"
if [ "$MODE" != "check" ] && [ ! -d "$DOC" ]; then
  echo "✗ 文档目录不存在: $DOC" >&2; exit 1
fi
case "$THRESHOLD" in ''|*[!0-9]*) THRESHOLD=2 ;; esac

ensure_note() {
  [ -f "$NOTE" ] && return 0
  {
    echo "# 根因定案 — 被轮次闸门拦住的功能必须先在这里留下「定案」（rootcause_note.sh 追加）"
    echo "# 规则：某功能连续 ${THRESHOLD} 轮「无效」后，必须有一条「定案轮次 ≥ 最后一次无效轮次」的记录，否则 pre_install_gate 查6 拦截（STAGE_NEXT_FORCE=1 可强过，须说明）"
    printf '日期\t功能#\t定案轮次\t根因（一句话，指向具体判据/代码行）\t证据行\t验证方法\t记录人\n'
  } > "$NOTE"
  echo "▶ 已新建定案台账: ${NOTE}"
}

# ── 台账格式识别 + 判定归一化（2026-10-12 P0-2：**这是「同一个功能改十几遍」的作案机制**）──
#  旧实现直接取 $6 与字面量 "无效" 比较。但仓库里同时存在三种台账表头：
#    · 六列（pipeline.sh 生成）：日期 功能# 轮次 改动摘要 装机结果 用户结果      → 判定在第 6 列
#    · 七列（P22 模板，霓虹深渊2/邂逅在迷宫/炁术行者手工写）：时间 功能 轮次 改动 结果 判据 状态
#      → 判定本应在第 5 列、且取值是「通过/失败」（**从无 "无效" 字样**）
#    · 自由格式（绝区零）
#  ⇒ 七列表台账下 `$6=="无效"` **永远为假**，门禁数学上不可能触发，却一直打印「✓ PASS」。
#  现在：先按表头定位判定列，再对取值做归一化（无效|失败|未通过→无效；有效|通过→有效；其余→未测）。
LEDGER_VERDICT_COL="6"
ledger_verdict_col() {  # 输出判定列号；表头识别不了时输出 0（调用方须告警，不许静默 PASS）
  [ -f "$LEDGER" ] || { echo 0; return 0; }
  awk -F'\t' '
    NR==3{ for (i=1;i<=NF;i++) if ($i=="用户结果") { print i; exit }
           for (i=1;i<=NF;i++) if ($i=="结果")   { print i; exit }
           print 6; exit }
    END{ if (NR<3) print 0 }' "$LEDGER"
}
# 该功能「连续无效」的轮次与最后一轮轮次：输出 "<streak> <last_round>"
streak_of() {  # streak_of <功能#>
  [ -f "$LEDGER" ] || { echo "0 0"; return 0; }
  awk -F'\t' -v f="$1" -v vc="${LEDGER_VERDICT_COL:-6}" '
    function nv(s,   t){
      t=s; gsub(/[[:space:]]/,"",t); gsub(/（推）/,"",t); gsub(/\(推\)/,"",t)
      if (t ~ /无效|失败|未通过|已否证/) return "无效"
      if (t ~ /未测|未验证|待验证|待用户|进行中|已作废/) return "未测"
      if (t ~ /有效|通过|已成立|已完成/) return "有效"
      return "未测"
    }
    NR>3 && $2==f && $2!="功能#" && $2!="功能" && $2!="日期" && $2!="时间" { v[++n]=nv($(vc)); r[n]=$3 }
    END{ c=0
         for (i=n; i>=1; i--) { if (v[i]=="无效") { c++; if ((r[i]+0) > (last+0)) last=r[i] } else break }
         print c" "(last+0) }' "$LEDGER"
}
# 台账里有多少行带「有效/无效」判定（=0 说明只有装机登记、没有任何实测判定 ⇒ 门禁没有计数依据）
judged_rows() {
  [ -f "$LEDGER" ] || { echo 0; return 0; }
  awk -F'\t' -v vc="${LEDGER_VERDICT_COL:-6}" '
    function nv(s,   t){
      t=s; gsub(/[[:space:]]/,"",t); gsub(/（推）/,"",t); gsub(/\(推\)/,"",t)
      if (t ~ /无效|失败|未通过|已否证/) return 1
      if (t ~ /有效|通过|已成立|已完成/) return 1
      return 0
    }
    NR>3 && $2!="" && $2!="功能#" && $2!="功能" && $2!="日期" && $2!="时间" { if (nv($(vc))) c++ } END{ print c+0 }' "$LEDGER"
}
# 历史累计无效轮数与最近一次无效轮次：输出 "<count> <last_invalid_round>"
# （用于提示「改十几遍但从没写过根因定案」——尾部已转有效不算拦，但必须看得见，否则下次从头再试）
invalid_total_of() {  # invalid_total_of <功能#>
  [ -f "$LEDGER" ] || { echo "0 0"; return 0; }
  awk -F'\t' -v f="$1" -v vc="${LEDGER_VERDICT_COL:-6}" '
    function nv(s,   t){
      t=s; gsub(/[[:space:]]/,"",t); gsub(/（推）/,"",t); gsub(/\(推\)/,"",t)
      if (t ~ /无效|失败|未通过|已否证/) return "无效"
      if (t ~ /未测|未验证|待验证|待用户|进行中|已作废/) return "未测"
      if (t ~ /有效|通过|已成立|已完成/) return "有效"
      return "未测"
    }
    NR>3 && $2==f { if (nv($(vc))=="无效") { c++; last=$3 } }
    END{ print c+0" "(last+0) }' "$LEDGER"
}
# 是否有定案：定案轮次 ≥ 给定轮次 → 输出 "1"
has_note() {  # has_note <功能#> <需要的轮次>
  [ -f "$NOTE" ] || { echo "0"; return 0; }
  awk -F'\t' -v f="$1" -v need="$2" '
    NR>3 && $2==f { if (($3+0) >= (need+0)) hit=1 }
    END{ print (hit?"1":"0") }' "$NOTE"
}

if [ "$MODE" = "list" ]; then
  if [ ! -f "$NOTE" ]; then echo "（尚无定案台账: ${NOTE}）"; exit 0; fi
  echo "──── 根因定案 ${NOTE} ────"
  awk -F'\t' 'NR>3 { printf "  [功能#%s / 轮次%s] %s\n        证据: %s\n        验证: %s\n", $2,$3,$4,$5,$6 }' "$NOTE"
  exit 0
fi

if [ "$MODE" = "check" ]; then
  if [ ! -f "$LEDGER" ]; then
    echo "✓ PASS  [查6 轮次根因] 无轮次台账（${LEDGER}）⇒ 不拦"
    exit 0
  fi
  LEDGER_VERDICT_COL="$(ledger_verdict_col)"
  if [ "${LEDGER_VERDICT_COL:-0}" = "0" ]; then
    echo "⚠ WARN  [查6 轮次根因] 台账 ${LEDGER##*/} 第 3 行不是列名行 ⇒ **判定列识别不了，本节对本游戏失明**"
    echo "        ⇒ 表头请用： 日期 功能# 轮次 改动摘要 装机结果 用户结果 [判据 状态 功能名]"
    echo "        ⇒ 迁移参考： 霓虹深渊2/轮次台账.tsv（2026-10-12 由 P22 七列迁为九列超集）"
    exit 0
  fi
  JUDGED="$(judged_rows)"
  if [ "${JUDGED:-0}" = "0" ]; then
    echo "⚠ WARN  [查6 轮次根因] 台账 ${LEDGER##*/} 里 **没有任何「有效/无效」判定**（列 ${LEDGER_VERDICT_COL} 全是未测/装机登记）"
    echo "        ⇒ 门禁没有计数依据、拦不住连续返工；用户每次 L3 回执后必须回写一行："
    echo "           bash 04_scripts/pipeline.sh ${GAME} round <功能#> <有效|无效> '<一句摘要>（用户 mXXXXX）'"
    exit 0
  fi
  echo "──── 查6 轮次根因：${LEDGER##*/} 判定列=${LEDGER_VERDICT_COL}，带判定行数=${JUDGED}，阈值=${THRESHOLD} 轮 ────"
  BAD=0; CHECKED=0; WARNED=0
  FEATS="$(awk -F'\t' -v vc="$LEDGER_VERDICT_COL" 'NR>3 && $2!="" && $2!="功能#" && $2!="功能" && $2!="日期" && $2!="时间" && $(vc)!="" {print $2}' "$LEDGER" | sort -u -n)"
  while IFS= read -r f; do
    [ -n "$f" ] || continue
    SET="$(streak_of "$f")"
    ST="${SET% *}"; LR="${SET#* }"
    if [ "${ST:-0}" -lt "$THRESHOLD" ]; then
      IT="$(invalid_total_of "$f")"; ITN="${IT% *}"; ITR="${IT#* }"
      if [ "${ITN:-0}" -ge "$THRESHOLD" ] && [ "$(has_note "$f" "$ITR")" != "1" ]; then
        WARNED=$((WARNED + 1))
        echo "⚠ WARN  [查6 轮次根因] 功能#${f} 历史累计无效 ${ITN} 轮（最近无效第 ${ITR} 轮，末次判定已非无效）**且无根因定案**"
        echo "        ⇒ 这是「同一个功能改十几遍」的痕迹（本轮不拦，但下次不许从头再试）；补一笔定案："
        echo "           bash 04_scripts/_common/rootcause_note.sh --game ${GAME} --feature ${f} --round $((ITR+1)) --cause '…' --evidence '…' --verify '…'"
      fi
      continue
    fi
    CHECKED=$((CHECKED + 1))
    if [ "$(has_note "$f" "$LR")" = "1" ]; then
      echo "✓ PASS  [查6 轮次根因] 功能#${f} 连续无效 ${ST} 轮（最后第 ${LR} 轮）已有根因定案"
    else
      echo "✗ FAIL  [查6 轮次根因] 功能#${f} 连续无效 ${ST} 轮（最后第 ${LR} 轮）**没有根因定案**"
      echo "        ⇒ 先跑： bash 04_scripts/_common/rootcause_note.sh --game ${GAME} --feature ${f} --round $((LR+1)) \\"
      echo "                 --cause '<一句话根因，指向具体判据/代码行>' --evidence '<文件:行 或 logcat 行>' --verify '<验证命令>'"
      echo "        ⇒ 或显式强过（须在台账写明理由）： STAGE_NEXT_FORCE=1"
      BAD=$((BAD + 1))
    fi
  done <<EOF
$FEATS
EOF
  if [ "$BAD" -gt 0 ]; then
    echo "❌ 查6 未过：${BAD} 个功能缺根因定案（连续无效 ≥${THRESHOLD} 轮）。"
    echo "   ⚠ 这正是「同一个功能改十几遍」的闸门：不许再换个自变量装同一轮包，先出假设矩阵（pipeline.sh ${GAME} rounds）。"
    exit 1
  fi
  echo "✓ PASS  [查6 轮次根因] 无「连续 ${THRESHOLD} 轮无效且无定案」的功能（检查 ${CHECKED} 个达标功能；历史累计告警 ${WARNED} 个）"
  exit 0
fi

# ---------- 登记模式 ----------
[ -n "$FEATURES" ] || { echo "✗ 必须给 --feature（数字或 all；可重复）" >&2; exit 1; }
[ -n "$ROUND" ]     || { echo "✗ 必须给 --round（该定案针对第几轮，通常＝最后一次无效轮次+1）" >&2; exit 1; }
[ -n "$CAUSE" ]     || { echo "✗ 必须给 --cause（一句话根因，指向具体判据/代码行；不接受「可能是」）" >&2; exit 1; }
case "$ROUND" in ''|*[!0-9]*) echo "✗ --round 必须是数字" >&2; exit 1 ;; esac
if [ -z "$EVIDENCE" ]; then
  echo "⚠ 未给 --evidence：定案没有证据行，下一轮会被要求重走。强烈建议补上（文件:行 或 logcat 行）。"
fi
EV_ONE="$(printf '%s' "$EVIDENCE" | tr '\n' ';' | sed 's/^;*//; s/;[[:space:]]*$//; s/;[[:space:]]*;/; /g')"
[ -n "$EV_ONE" ] || EV_ONE="（未填）"
[ -n "$VERIFY" ] || VERIFY="（未填）"

TODAY="$(date '+%Y-%m-%d %H:%M')"
if [ "$MODE" = "dry" ]; then
  echo "▶ [dry-run] 将登记到 ${NOTE}："
  echo "   日期=${TODAY}  功能#=${FEATURES# }  定案轮次=${ROUND}"
  echo "   根因=${CAUSE}"
  echo "   证据=${EV_ONE}"
  echo "   验证=${VERIFY}"
  echo "▶ [dry-run] 未写盘。"
  exit 0
fi

ensure_note
printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\n' \
  "$TODAY" "${FEATURES# }" "$ROUND" "$CAUSE" "$EV_ONE" "$VERIFY" "AI" >> "$NOTE"
echo "✅ 已登记根因定案：功能#${FEATURES# } 轮次 ${ROUND} → ${NOTE}"

# 登记后即时自检：闸门是否已放行
if bash "$0" --check --game "$GAME" --threshold "$THRESHOLD"; then
  echo "✅ 查6 轮次根因关：已放行（本次定案满足闸门）。"
else
  echo "⚠ 仍有功能缺定案（见上）。装机前把其它被拦功能也补上，或用 STAGE_NEXT_FORCE=1 显式强过。"
  exit 1
fi
exit 0
