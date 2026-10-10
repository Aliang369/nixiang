#!/usr/bin/env bash
# ================================================================
#  session_start.sh — 开工 preflight（长会话/弱模型的第一道强制关口）
#
#  背景（2026-10-10 邂逅在迷宫会话复盘 P0）：
#  该会话 4.4k 条消息、9 版助手包、11.4 GB 死路产物，根因不是"文档不够"，
#  而是**既有纪律文件被整体旁路**——本游戏既没有 10_docs/<游戏>/硬约束.md，
#  也没有 04_scripts/<游戏>/pipeline.json，于是用户硬指令、已否证结论、
#  脚本坑位全靠对话记忆维持，prune 一来就稀释。对策不是"记得更牢"，
#  而是**每次开工强制回读一批可审计的文件**。
#
#  用法:
#    bash 04_scripts/_common/session_start.sh <游戏> [--serial <S>] [--pkg <包名>] [--quiet]
#  示例:
#    bash 04_scripts/_common/session_start.sh 邂逅在迷宫 --serial f9a116a2 --pkg cn.alltune.endless
#
#  它做什么（六查，全部只读，除"缺失即生成骨架"）:
#    查1 硬约束    ：10_docs/<游戏>/硬约束.md 是否存在、是否已抄用户原话（占位符「……」=FAIL）
#    查2 路线锁定  ：10_docs/<游戏>/路线锁定.md 未确认偏离计数（有对照包时必需）
#    查3 台账      ：10_docs/<游戏>/台账.md 的「已否证」区 = **禁止重试清单**，逐条打印
#    查4 管线出口  ：04_scripts/<游戏>/pipeline.json 状态；缺失时提醒"机械动作无管线出口"
#    查5 坑位回读  ：TOOLBOOK.md「本机 / 真机坑位」节的行数与位置（踩过即落一行）
#    查6 设备/预算 ：adb 设备状态 + 目标包 versionName + 会话预算常量（P6）+ 纪律卡
#
#  退出码: 0=就绪；1=有 FAIL（缺硬约束/硬约束未抄原话/缺台账）；2=用法错
#  铁律: **未跑本脚本，不得改功能代码、不得装机。** 骨架文件生成后仍是 FAIL，
#        因为"生成空表"不等于"抄了用户原话"——原话只能由当轮的人写进去。
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

TODAY="$(date +%Y%m%d)"
GAME=""; SERIAL=""; PKG=""; QUIET=0

usage() {
  cat <<'EOF'
用法: bash 04_scripts/_common/session_start.sh <游戏> [--serial <S>] [--pkg <包名>] [--quiet]
  <游戏>        10_docs/<游戏>/ 与 04_scripts/<游戏>/ 的目录名（如 邂逅在迷宫）
  --serial S    指定 adb 设备（多设备必传）
  --pkg P       目标游戏包名（用于打印 versionName；也可省略）
  --quiet       只打印 FAIL/WARN 与禁止重试清单（省 token）
退出码: 0=就绪 / 1=有 FAIL / 2=用法错
EOF
}

while [ $# -gt 0 ]; do
  case "$1" in
    --serial) SERIAL="$2"; shift 2 ;;
    --pkg)    PKG="$2";    shift 2 ;;
    --quiet)  QUIET=1;     shift ;;
    -h|--help) usage; exit 0 ;;
    -*) echo "未知参数: $1"; usage; exit 2 ;;
    *) GAME="$1"; shift ;;
  esac
done

if [ -z "$GAME" ]; then usage; exit 2; fi

DOC_DIR="$NIXIANG_ROOT/10_docs/$GAME"
SCRIPT_GAME_DIR="$NIXIANG_ROOT/04_scripts/$GAME"
TPL_DIR="$NIXIANG_ROOT/10_docs/_templates"
fail=0; warn=0

ok()   { echo "  ✓ $*"; }
bad()  { echo "  ✗ $*"; fail=$((fail+1)); }
note() { echo "  ⚠ $*"; warn=$((warn+1)); }
head_() { [ "$QUIET" = "1" ] || echo; [ "$QUIET" = "1" ] || echo "── $* ──"; }
kv()   { [ "$QUIET" = "1" ] && return 0; echo "  · $*"; }

echo "================================================================"
echo " 开工 preflight：$GAME   （${TODAY}，host=${NIXIANG_HOST}）"
echo "================================================================"

# ---------- 查1 硬约束 ----------
head_ "查1 硬约束（用户原话）"
CONSTRAINT="$DOC_DIR/硬约束.md"
if [ ! -f "$CONSTRAINT" ]; then
  mkdir -p "$DOC_DIR"
  cat > "$CONSTRAINT" <<EOF
# $GAME 硬约束（$TODAY 起）

- 游戏 / 版本 / 建档日期：$GAME / / $TODAY
- 约束来源：用户聊天原话；每条必须带消息号。**逐字引用**若为概括则标 \`（意译）\`。

| # | 原话（逐字，含消息号） | 类型 | 可执行化（我该怎么遵守） | 违反一次的实例 | 状态 |
|---|---|---|---|---|---|
| 1 | 「……」(m00000) | 必须 | … | … | 生效 |

## 落地检查（每条约束在动代码/装机前必须能回答）

1. 我这轮要做/已做的动作，有哪一条**能被这条约束判定为违反**？
2. 我认为必须偏离时，是否已把「偏离内容 + 理由 + 对照证据」写成一条，并**取得用户当轮确认**？
3. 「照 XX 做」类：对照方的机制是否已逐项抄进 \`路线锁定.md\`？**没抄完 = 没读**。
4. 状态为「已失效」的约束，是否写清了失效依据（用户新指令 / 版本变化）与日期？

## 变更记录

| 日期 | 变更 | 依据（消息号） |
|---|---|---|
| $TODAY | 骨架生成（待抄用户原话） | session_start.sh |
EOF
  bad "硬约束.md 缺失 ⇒ 已从模板生成骨架：$CONSTRAINT"
  bad "  请当轮把用户约束（逐字 + 消息号）抄进去后再开工；占位符「……」未清 = 未抄"
else
  rows=$(grep -c '^| [0-9]' "$CONSTRAINT" 2>/dev/null || echo 0)
  if grep -q '「……」' "$CONSTRAINT" 2>/dev/null; then
    bad "硬约束.md 仍有占位符「……」⇒ 用户原话未抄入（${CONSTRAINT}）"
  elif [ "$rows" = "0" ]; then
    bad "硬约束.md 无有效约束行（表格为空）：$CONSTRAINT"
  else
    ok "硬约束.md 在位，约束行 $rows 条"
    grep -n '^| [0-9]' "$CONSTRAINT" | sed -n '1,40p' | while IFS= read -r l; do kv "  $l"; done
  fi
fi

# ---------- 查2 路线锁定 ----------
head_ "查2 路线锁定（移植/照抄对照）"
ROUTE="$DOC_DIR/路线锁定.md"
if [ -f "$ROUTE" ]; then
  unconfirmed=$(grep -c '未确认' "$ROUTE" 2>/dev/null || echo 0)
  if [ "$unconfirmed" -gt 0 ] 2>/dev/null; then
    note "路线锁定.md 有 $unconfirmed 处「未确认」偏离 ⇒ 偏离未获用户确认时不得装机：$ROUTE"
  else
    ok "路线锁定.md 在位，无未确认偏离"
  fi
else
  note "路线锁定.md 不存在（仅当本轮存在「照某包做」的对照任务时才必需）：$ROUTE"
fi

# ---------- 查3 台账（已否证 = 禁止重试清单） ----------
head_ "查3 台账：已否证 = 禁止重试清单"
LEDGER="$DOC_DIR/台账.md"
if [ ! -f "$LEDGER" ]; then
  mkdir -p "$DOC_DIR"
  if [ -f "$TPL_DIR/台账.md" ]; then
    {
      echo "# 台账（${GAME}，append-only）"
      echo
      echo "> 由 session_start.sh 于 $TODAY 从 10_docs/_templates/台账.md 生成骨架；只追加、不删行。"
      echo "> 列含义与三条纪律见模板：10_docs/_templates/台账.md"
      echo
      sed -n '/^## 已成立/,$p' "$TPL_DIR/台账.md"
    } > "$LEDGER"
  else
    printf '# 台账（%s，append-only）\n\n## 已成立\n\n| 结论 | 决定性证据 | 状态 | 归属 |\n|---|---|---|---|\n\n## 已否证（禁止重试区）\n\n| 曾经的假设 | 否证证据 | 状态 | 归属 |\n|---|---|---|---|\n\n## 未验证（等什么）\n\n| 待验证的结论 | 需要的实验 / 证据 | 状态 | 归属 |\n|---|---|---|---|\n' "$GAME" > "$LEDGER"
  fi
  bad "台账.md 缺失 ⇒ 已生成骨架：${LEDGER}（本会话结论要当轮落行）"
else
  denied=$(grep -c '已否证' "$LEDGER" 2>/dev/null || echo 0)
  ok "台账.md 在位（含「已否证」标记 $denied 行）"
  if [ "$denied" -gt 0 ] 2>/dev/null; then
    echo "  ★ 禁止重试清单（不得再试，除非写出新证据推翻它）："
    grep '已否证' "$LEDGER" | grep '^|' | awk -F'|' '{print $2}' | sed 's/^ *//;s/ *$//' | grep -v '^曾经的假设$' | nl -w2 -s'. ' | sed 's/^/     /'
  fi
  # 未验证区也打印（避免把"还没验"当成"已验"）
  if [ "$QUIET" != "1" ]; then
    pend=$(awk '/^## 未验证/{f=1} f' "$LEDGER" | grep '^|' | awk -F'|' '{print $2}' | sed 's/^ *//;s/ *$//' | grep -v '^待验证的结论$' | grep -v '^---')
    if [ -n "$pend" ]; then echo "  · 未验证条目："; echo "$pend" | nl -w2 -s'. ' | sed 's/^/     /'; fi
  fi
fi

# ---------- 查4 管线出口 ----------
head_ "查4 机械动作的管线出口"
if [ -f "$SCRIPT_GAME_DIR/pipeline.json" ]; then
  ok "pipeline.json 在位：$SCRIPT_GAME_DIR/pipeline.json"
  [ "$QUIET" = "1" ] || grep -o '"name"[^,]*' "$SCRIPT_GAME_DIR/pipeline.json" 2>/dev/null | head -20 | sed 's/^/     /'
else
  note "无 04_scripts/$GAME/pipeline.json ⇒ 本游戏的机械动作没有登记出口；改包/重打/装机前先写清「本次出口命令」或补 pipeline"
fi

# ---------- 查5 坑位回读 ----------
head_ "查5 TOOLBOOK 坑位（踩过即落一行）"
TB="$NIXIANG_ROOT/TOOLBOOK.md"
if [ -f "$TB" ]; then
  ln_start=$(grep -n '^## 二-b、本机 / 真机坑位' "$TB" | head -1 | cut -d: -f1)
  if [ -n "$ln_start" ]; then
    ln_next=$(awk -v s="$ln_start" 'NR>s && /^## /{print NR; exit}' "$TB")
    [ -n "$ln_next" ] || ln_next=$((ln_start+400))
    cnt=$((ln_next - ln_start - 1))
    ok "坑位节在位：TOOLBOOK.md:${ln_start}（约 $cnt 行）"
    [ "$QUIET" = "1" ] || sed -n "${ln_start},$((ln_next-1))p" "$TB" | grep '^|' | sed -n '1,60p' | sed 's/^/     /'
  else
    note "TOOLBOOK.md 无「## 二-b、本机 / 真机坑位」节 ⇒ 本轮踩到的坑无处沉淀"
  fi
else
  note "TOOLBOOK.md 不存在"
fi

# ---------- 查6 设备 / 预算 / 纪律 ----------
head_ "查6 设备、会话预算、纪律卡"
ADB="${ADB:-adb}"
if command -v "$ADB" >/dev/null 2>&1; then
  if [ -n "$SERIAL" ]; then ST="$("$ADB" -s "$SERIAL" get-state 2>/dev/null || true)"; else ST="$("$ADB" get-state 2>/dev/null || true)"; fi
  if [ "$ST" != "device" ]; then
    note "adb 设备不可用（get-state=${ST:-空}）；多设备请传 --serial"
  else
    if [ -n "$SERIAL" ]; then ADBX="$ADB -s $SERIAL"; else ADBX="$ADB"; fi
    MODEL="$($ADBX shell getprop ro.product.model 2>/dev/null | tr -d '\r')"
    ok "设备在线：${SERIAL:-（唯一设备）} model=$MODEL"
    if [ -n "$PKG" ]; then
      VN="$($ADBX shell dumpsys package "$PKG" 2>/dev/null | grep -m1 'versionName=' | tr -d '\r' | sed 's/.*versionName=//')"
      if [ -n "$VN" ]; then kv "目标包 $PKG versionName=$VN"; else note "目标包 $PKG 未安装或查不到 versionName"; fi
    fi
  fi
else
  note "adb 不在 PATH（设 ADB=/abs/path/adb 或 export PATH）"
fi
kv "会话预算（AGENTS.md P6）：工具调用 ≈600 / 装机 ≈40 / 阴影 token ≈150K —— 超了就按复盘格式记账"
kv "纪律卡：① 禁止 input tap/swipe（m01899：屏幕由用户操作）② 出网/改外部机器/改交付形态 ⇒ 先出 10_docs/_templates/方案选型卡.md 等用户选定（m03714）③ 装机后跑 post_install_precheck.sh ④ 卸载/清数据后此前结论全部作废（状态归零纪律）"

# ---------- 出包前提示（2026-10-10 复盘 P0-1 / P2-1；只追加提示，不改退出码与既有结构） ----------
kv "框架入口自检（入口类清单）：grep -rln --include='*.java' -E 'class [A-Za-z0-9_]+ extends (Application|ContentProvider|AppComponentFactory)' mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/"
kv "出包前跑审计（任一 ✗ 禁止出包）：bash 04_scripts/_common/framework_entry_audit.sh --game <游戏> --pkg <包名> --apk <apk>；入口取舍见 skill/starry-mod/SKILL.md「入口点决策表」"
MK_ENTRY_AUDIT="${NIXIANG_ROOT}/mod菜单/starry-mod-menu/app/src/main/jni/Android.mk"
if [ -f "$MK_ENTRY_AUDIT" ] && grep -qE '^[[:space:]]*LOCAL_CFLAGS.*-DENABLE_UNITY_HOOK.*-DSTARRY_GAME_' "$MK_ENTRY_AUDIT"; then
  kv "该游戏 Android.mk 激活分支属 libil2cpp 基址解析类 ⇒ 按「设备画像先行」可跑 bash 04_scripts/_common/device_profile.sh --serial <S> --pkg <P> --rva <符号>=0x<HEX>"
fi

# ---------- preflight 凭证（2026-10-10 第二轮复盘 P10 硬化） ----------
# 为什么要有凭证：P10 一直只是「说给人听的纪律」。第二轮复盘发现它**又被整段旁路**
# （本会话没跑 preflight 就改了 v3.6~v3.9 四版代码、装了 4 次机，其中 2 版白做）。
# 纪律要能生效就得有闸门 ⇒ 这里在通过时落一份签章凭证（含硬约束台账 md5），
# gradle_cycle.sh --install 只认它：凭证缺失 / 超过 12h / 硬约束台账被改过 ⇒ 拒绝装机。
STAMP_DIR="${NIXIANG_ROOT}/10_docs/_state/机器心跳"
STAMP="${STAMP_DIR}/preflight_${GAME}.stamp"
HC_MD="${NIXIANG_ROOT}/10_docs/${GAME}/硬约束.md"
hc_md5="-"; hc_rows=0
if [ -f "$HC_MD" ]; then
  hc_md5="$(md5 -q "$HC_MD" 2>/dev/null || md5sum "$HC_MD" | awk '{print $1}')"
  hc_rows="$(grep -cE '^\| [0-9]+ \|' "$HC_MD" 2>/dev/null || true)"
  [ -n "$hc_rows" ] || hc_rows=0
fi
if [ "$fail" -gt 0 ]; then
  kv "preflight 未通过 ⇒ 不落凭证（${STAMP}）；此时 gradle_cycle.sh --install 会被拒"
else
  mkdir -p "$STAMP_DIR"
  {
    echo "t=$(date '+%Y-%m-%d %H:%M:%S')"
    echo "epoch=$(date +%s)"
    echo "host=${NIXIANG_HOST}"
    echo "game=${GAME}"
    echo "serial=${SERIAL:-（唯一设备）}"
    echo "pkg=${PKG:-（未传 --pkg）}"
    echo "hard_constraints_md5=${hc_md5}"
    echo "hard_constraints_rows=${hc_rows}"
    echo "warn=${warn}"
    echo "by=session_start.sh"
  } > "$STAMP"
  ok "preflight 凭证已落：${STAMP}（硬约束 md5=${hc_md5} 行数=${hc_rows}；12h 内有效）"
fi

echo "================================================================"
if [ "$fail" -gt 0 ]; then
  echo "结论：✗ FAIL $fail 项 / ⚠ WARN $warn 项 ⇒ 补齐后再改功能代码或装机"
  echo "================================================================"
  exit 1
fi
echo "结论：✓ 就绪（WARN $warn 项）"
echo "================================================================"
exit 0
