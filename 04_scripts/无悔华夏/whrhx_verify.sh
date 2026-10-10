#!/usr/bin/env bash
# 无悔华夏 3.8.280 —— 一键验收器（P0-①）
#
# 为什么要有它：本轮四次「看着像成功其实没生效」全部靠人肉 grep 才拆穿
# （`rva miss` 噪音 221 条把 `policy panel gate incomplete` ×71 埋掉；
#  ANR 根因要靠 /data/anr 栈才看得出来）。判据固化成脚本，收尾不许凭印象。
#
# 用法：
#   bash 04_scripts/无悔华夏/whrhx_verify.sh                     # 抓真机日志并全查 4 项
#   bash 04_scripts/无悔华夏/whrhx_verify.sh --feat 10,11        # 只查指定功能
#   bash 04_scripts/无悔华夏/whrhx_verify.sh --log /tmp/x.log    # 分析已有日志（离线）
#   bash 04_scripts/无悔华夏/whrhx_verify.sh --selftest          # 只查「免登录落点自检」
#   bash 04_scripts/无悔华夏/whrhx_verify.sh --serial f9a116a2 --pkg com.yhhy.whrhx.aligames
#
# 判据（全部来自真机实锤日志）：
#   注：日志里 %lX 打的是大写十六进制 ⇒ 所有 hex 匹配都必须带 A-F（本文件已踩过）。
#   身份链   `identity api ready mgrOff=0x30 countryOff=0xE0` + 心跳 `idApi=1`（mgr 非 0）
#   自检     `selftest ok cid=<n> … policies=<n> ministers=<n>`（P2-⑨，**不需要卡密**）
#   装钩方式 `hooked … (il2cpp-name)` / `(rva)` 计数（按名是稳定路径）
#   #10      `noCD-policy finish … no active remain=0.000`（失败时给 `policy panel gate incomplete` 提示）
#   #11      `minister favor pinned=<n>` 或 `minister favorite id=`
#   #12      `noCD-minister finish id=<n> …`
#   #13      `army maxSend type=0 A→B (+N%)` / `army cap target=<n> A→B`（N>1 才算真生效）
#
# 退出码：0 = 请求的检查全 PASS；3 = 有 FAIL/NOTSEEN；2 = 用法错误；1 = 环境错误。
set -uo pipefail

SERIAL="${DEV_SERIAL:-f9a116a2}"
PKG="${DEV_PKG:-com.yhhy.whrhx.aligames}"
FEATS="10,11,12,13"
LOGFILE=""
SELFTEST_ONLY=0
TMPLOG=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --feat) FEATS="${2:-}"; shift 2 ;;
    --log) LOGFILE="${2:-}"; shift 2 ;;
    --serial) SERIAL="${2:-}"; shift 2 ;;
    --pkg) PKG="${2:-}"; shift 2 ;;
    --selftest) SELFTEST_ONLY=1; shift ;;
    -h|--help) sed -n '2,30p' "$0"; exit 0 ;;
    *) echo "ERROR: unknown arg $1" >&2; exit 2 ;;
  esac
done

if [[ -n "$LOGFILE" ]]; then
  [[ -f "$LOGFILE" ]] || { echo "ERROR: no such log: $LOGFILE" >&2; exit 1; }
  LOG="$LOGFILE"
else
  command -v adb >/dev/null || { echo "ERROR: adb not found" >&2; exit 1; }
  TMPLOG="/tmp/whrhx_verify_$(date +%Y%m%d_%H%M%S).log"
  echo "[verify] adb -s $SERIAL logcat -d  ->  $TMPLOG"
  adb -s "$SERIAL" logcat -d -v time > "$TMPLOG" 2>/dev/null || true
  LOG="$TMPLOG"
fi
LINES="$(wc -l < "$LOG" | tr -d ' ')"
echo "[verify] log=$LOG lines=$LINES pkg=$PKG serial=$SERIAL"

# ---- 小工具 ----
PASS_N=0; FAIL_N=0; MISS_N=0
c_grep() { LC_ALL=C grep -a -c -- "$1" "$LOG" 2>/dev/null | tr -d ' '; }
c_first() { LC_ALL=C grep -a -m1 -o -- "$1" "$LOG" 2>/dev/null | head -1; }
report() { # name status evidence
  printf 'CHECK %-22s %-8s %s\n' "$1" "$2" "$3"
  case "$2" in PASS) PASS_N=$((PASS_N+1));; FAIL) FAIL_N=$((FAIL_N+1));; *) MISS_N=$((MISS_N+1));; esac
}

# ---- 1) 身份链（四项功能的公共前置）----
ID_READY="$(c_first 'identity api ready mgrOff=0x[0-9a-fA-F]* countryOff=0x[0-9a-fA-F]*')"
ID_TICK="$(c_first 'tick=[0-9]* api=1 idApi=1 mgr=0x[0-9a-fA-F]*')"
if [[ -n "$ID_READY" && -n "$ID_TICK" ]]; then
  report identity PASS "$ID_READY | $ID_TICK"
else
  EV="${ID_READY:-no-identity-api-ready}"
  [[ -z "$ID_TICK" ]] && EV="$EV | no idApi=1 heartbeat"
  [[ -z "$ID_READY" ]] && EV="$EV | hint: 类表未解出 -> grep 'identity miss' / 'class .* not found'"
  report identity FAIL "$EV"
fi

# ---- 2) 免登录落点自检（P2-⑨）----
SELF_LINE="$(c_first 'selftest ok cid=[0-9]* .*policies=-?[0-9]* ministers=-?[0-9]*')"
SELF_REACH="$(c_first "selftest reach mgr=.* dicKlass=Dictionary\`2 .*count=[0-9-]*")"
SELF_WAIT="$(c_first 'selftest wait: .*')"
if [[ -n "$SELF_LINE" ]]; then
  if LC_ALL=C grep -a -q 'policies=\(-1\|0\) ' <<<"$SELF_LINE"; then
    report selftest-no-login FAIL "$SELF_LINE (政策表不可读/为空)"
  else
    report selftest-no-login PASS "$SELF_LINE"
  fi
elif [[ -n "$SELF_REACH" ]]; then
  # 停在标题界面/未登录时国度表必然为空（count=0），此路只证「身份链可达」：
  # CountryDataMgr 按名解析成功 + m_countryDataDic(0x10) 处确实是 Dictionary`2。
  report selftest-no-login PASS "reach-only: $SELF_REACH (进局后复跑可得 selftest ok)"
elif [[ -n "$SELF_WAIT" ]]; then
  report selftest-no-login NOTSEEN "$SELF_WAIT"
else
  report selftest-no-login NOTSEEN "无 selftest 日志（需含 P2-⑨ 的新 so；正常玩一局即可触发）"
fi

# ---- 3) 装钩方式统计（按名 vs RVA）----
BY_NAME="$(c_grep '(il2cpp-name)')"
BY_RVA="$(c_grep '(rva)')"
RVA_MISS="$(c_grep 'rva miss')"
if [[ "${BY_NAME:-0}" -ge 1 ]]; then
  report hook-mode PASS "il2cpp-name=${BY_NAME} rva=${BY_RVA} rva-miss=${RVA_MISS}"
elif [[ "${BY_RVA:-0}" -ge 1 ]]; then
  report hook-mode PASS "il2cpp-name=0 rva=${BY_RVA} rva-miss=${RVA_MISS} (仅 RVA 命中：热更后罕见，注意复查)"
else
  report hook-mode FAIL "没有任何 hooked 行（装钩未发生）rva-miss=${RVA_MISS}"
fi

want() { [[ ",$FEATS," == *",$1,"* ]]; }

if [[ $SELFTEST_ONLY -eq 0 ]]; then
  # ---- #10 政策技能无CD ----
  if want 10; then
    F10="$(c_first 'noCD-policy finish id=[0-9]* no active remain=0\.000')"
    if [[ -n "$F10" ]]; then
      report feat10-policy-nocd PASS "$F10"
    else
      PANEL="$(c_first 'policy panel gate incomplete[^"]*')"
      SWEEP="$(c_first 'policy noCD sweep cleared=[0-9]*')"
      if [[ -n "$PANEL" ]]; then
        report feat10-policy-nocd FAIL "面板门未装齐：$PANEL"
      elif [[ -n "$SWEEP" ]]; then
        report feat10-policy-nocd NOTSEEN "${SWEEP}（只清到 CD 值，未见面板连点证据）"
      else
        report feat10-policy-nocd NOTSEEN "无 #10 证据（菜单需打开 #10；且需一次政策点击）"
      fi
    fi
  fi

  # ---- #11 名臣好感度 ----
  if want 11; then
    F11="$(c_first 'minister favor pinned=[1-9][0-9]*')"
    [[ -z "$F11" ]] && F11="$(c_first 'minister favorite id=[0-9]* [0-9]*→[0-9]*')"
    [[ -z "$F11" ]] && F11="$(c_first 'minister add favor [0-9]*→[0-9]*')"
    if [[ -n "$F11" ]]; then
      report feat11-minister-favor PASS "$F11"
    else
      H11="$(c_first 'minister hooks=[0-9]*[^"]*')"
      report feat11-minister-favor NOTSEEN "${H11:-未装钩（菜单需打开 #11）}"
    fi
  fi

  # ---- #12 名臣技能无CD ----
  if want 12; then
    F12="$(c_first 'noCD-minister finish id=[0-9]*[^"]*')"
    if [[ -n "$F12" ]]; then
      report feat12-minister-nocd PASS "$F12"
    else
      report feat12-minister-nocd NOTSEEN "无 #12 证据（菜单打开 #12 后点一次名臣技能）"
    fi
  fi

  # ---- #13 军队规模加成（滑块 1-100）----
  if want 13; then
    F13="$(c_first 'army maxSend type=[0-9]* [0-9]*→[0-9]* (+[0-9]*%)')"
    [[ -z "$F13" ]] && F13="$(c_first 'army cap target=[0-9]* [0-9]*→[0-9]*')"
    if [[ -n "$F13" ]]; then
      PCT="$(LC_ALL=C sed -n 's/.*(+\([0-9]\{1,\}\)%).*/\1/p' <<<"$F13" | head -1)"
      if [[ -z "$PCT" || "$PCT" -gt 1 ]]; then
        report feat13-army-scale PASS "$F13"
      else
        report feat13-army-scale FAIL "滑块仍在 1%（=关）：$F13"
      fi
    else
      R13="$(c_first 'army scale hooks[^"]*')"
      report feat13-army-scale NOTSEEN "${R13:-未装钩（菜单需打开 #13 并拖滑块）}"
    fi
  fi
fi

# ---- 4) 授权/会话状态（信息行，不计入判定）----
AUTH="$(c_first '\[auth\] store verdict=[a-z-]*')"
REJ="$(c_grep '拒绝 #[0-9]*（未授权）')"
echo "INFO  auth: ${AUTH:-none}  |  未授权拒绝次数=${REJ:-0}"

echo "----"
echo "SUMMARY pass=$PASS_N fail=$FAIL_N notseen=$MISS_N log=$LOG"
if [[ $((FAIL_N + MISS_N)) -eq 0 ]]; then
  echo "VERIFY_RESULT=ALL_PASS"
  exit 0
fi
echo "VERIFY_RESULT=INCOMPLETE（NOTSEEN 多为「功能没开/没点」，FAIL 才是缺陷）"
exit 3
