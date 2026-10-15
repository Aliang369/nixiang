#!/usr/bin/env bash
# ================================================================
#  pipeline.sh — 机械操作统一入口（状态机管线）
#
#  铁律（详见 _common/stages.md）：
#    1. 机械操作（拆/打/注/移/签/装/清理）唯一合法入口=本脚本；
#       禁止直接敲 adb/apktool/zip/ndk-build 等命令序列。
#    2. 本文件与 pipeline.lock 的变更纪律（所有会话一体适用）：禁随手改、禁只改一边；
#       启动即校验 sha256 指纹。
#       唯一合法改法＝同一提交内改本文件 + 刷新 _common/pipeline.lock 的
#       `shasum -a 256` 并 push（另一台 git pull 后即一致；只改不推会让另一台
#       经 Syncthing 先拿到新脚本而 lock 仍旧 ⇒ 那台所有命令 die「指纹不匹配」）。
#       详见 _common/stages.md「真要改 pipeline.sh 时的跨机姿势」。
#    3. 阶段失败即停：读日志修原因，禁止换命令绕过。
#    4. 合法参数修改走 04_scripts/{游戏}/pipeline.json 的 params。
#
#  用法：
#    bash 04_scripts/pipeline.sh <游戏> status          # 查看状态（开工第一步；同时钉硬约束 + 台账）
#    bash 04_scripts/pipeline.sh <游戏> status --brief  # 精简状态（跳过 说明/前置 与 硬约束+台账 回显，≈1/3 输出）
#    bash 04_scripts/pipeline.sh <游戏> next            # 执行下一阶段
#    bash 04_scripts/pipeline.sh <游戏> next --loop 4   # 连跑最多 4 个阶段，逐次回显日志尾 40 行（失败即停）
#    bash 04_scripts/pipeline.sh <游戏> run <stage_id> # 显式执行某阶段
#    bash 04_scripts/pipeline.sh <游戏> reset <stage_id># 状态回退 pending
#    bash 04_scripts/pipeline.sh <游戏> verify <stage_id> [--set]  # 只校验产物不执行；--set 校验通过即置 done
#    bash 04_scripts/pipeline.sh <游戏> log [N]         # 只回显 pipeline.log 尾 N 行（默认 40）
#    bash 04_scripts/pipeline.sh <游戏> round <功能#> <有效|无效|未测> [摘要]   # 记一轮实测结果
#    bash 04_scripts/pipeline.sh <游戏> rounds          # 打假设矩阵（被轮次门禁拦住时照它填）
#    bash 04_scripts/pipeline.sh <游戏> gate <stage_id> # 只跑装机前轮次门禁，不执行阶段
# ================================================================
set -uo pipefail

# ---------- 输出编码（2026-10-03） ----------
# Windows 侧 python3 默认 cp936 stdout：打印状态符号 ⬜/✅ 时会直接
# UnicodeEncodeError 崩掉（'gbk' codec can't encode '\u2b1c'）⇒ 统一强制 UTF-8
# 输出（macOS/Linux 无副作用；已有显式设置时不覆盖）。
export PYTHONIOENCODING="${PYTHONIOENCODING:-utf-8}"
export PYTHONUTF8="${PYTHONUTF8:-1}"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
COMMON_DIR="${SCRIPT_DIR}/_common"
LOCK_FILE="${COMMON_DIR}/pipeline.lock"
LOG_SEP="────────────────────────────────────────────"

die() { echo "❌ $*" >&2; exit 1; }

# ---------- 内循环 / 轮次门禁 / 硬约束 公共设施（2026-10-03，P1/P4/P5） ----------
# 逻辑原生放在本文件；04_scripts/_common/stage_next.sh 只做同义转发（兼容旧命令）。
#   P1 硬约束钉行：status 时把用户「必须/禁止/照 XX 做」的生效条目钉在屏幕上。
#   P4 轮次门禁：同一功能「改代码→装机→用户实测」连续 3 轮仍「无效」⇒ 拒绝装机阶段（exit 3）。
#   P5 内循环：next --loop N（逐次只回显日志尾 TAIL_LINES 行）+ log 动词。
# 覆盖开关：STAGE_NEXT_FORCE=1（仅用户当轮明确要求「再试一次」时用，会大声留痕）。
# 覆盖路径：STAGE_NEXT_DOC_DIR / STAGE_NEXT_LEDGER / STAGE_NEXT_TAIL（自检与跨目录文档用）。
TAIL_LINES="${STAGE_NEXT_TAIL:-40}"

doc_dir_of() {  # 文档目录名可能与管线游戏键不同名：优先同名，其次唯一模糊命中
  local game="$1" d cand
  d="$(cd "${SCRIPT_DIR}/.." && pwd)/10_docs/${game}"
  if [ ! -d "$d" ]; then
    cand="$(ls -d "${SCRIPT_DIR}/../10_docs/"*"${game}"* 2>/dev/null | grep -v -e '^.*/_[^/]*$' | head -1)"
    [ -n "${cand:-}" ] && [ -d "$cand" ] && d="$cand"
  fi
  if [ -n "${STAGE_NEXT_DOC_DIR:-}" ] && [ -d "${STAGE_NEXT_DOC_DIR}" ]; then d="$STAGE_NEXT_DOC_DIR"; fi
  echo "$d"
}

constraints_path() { echo "$(doc_dir_of "$1")/硬约束.md"; }

ledger_path() {
  if [ -n "${STAGE_NEXT_LEDGER:-}" ]; then
    echo "$STAGE_NEXT_LEDGER"
  else
    echo "$(doc_dir_of "$1")/轮次台账.tsv"
  fi
}

pin_constraints() {  # P1：开工第一步 / 装机前把用户硬约束钉在屏幕上
  local game="$1" f
  f="$(constraints_path "$game")"
  if [ ! -f "$f" ]; then
    echo "⚠  缺 ${f}（用户下达的「必须/禁止/照 XX 做」未落盘；模板 10_docs/_templates/硬约束.md）"
    return 0
  fi
  echo "──── 硬约束（生效条目，原文见 ${f}）────"
  grep -n '生效' "$f" | grep -v '已失效' | head -8
  echo "  （改功能代码前、装机前各回读一次；该文件永不删行，失效只改状态）"
}

ensure_ledger() {
  local game="$1" l
  l="$(ledger_path "$game")"
  [ -f "$l" ] && return 0
  [ -d "$(dirname "$l")" ] || die "文档目录不存在: $(dirname "$l")（无法记轮次台账）"
  {
    echo "# 轮次台账 — 同一功能「改代码 → 装机 → 用户实测」逐轮结果（pipeline.sh <游戏> round 追加）"
    echo "# 门禁规则：同一功能连续 3 轮「无效」⇒ 拒绝装机，先填假设矩阵（pipeline.sh <游戏> rounds）"
    printf '日期\t功能#\t轮次\t改动摘要\t装机结果\t用户结果\n'
  } > "$l"
  echo "▶ 已新建台账: ${l}"
}

ledger_round() {  # ledger_round <游戏> <功能#> <有效|无效|未测> [改动摘要]
  local game="$1" feat="$2" verdict="$3" note="${4:-}" l n
  case "$verdict" in 有效|无效|未测) ;; *) die "结果只能填 有效 / 无效 / 未测（收到: ${verdict}）";; esac
  l="$(ledger_path "$game")"
  ensure_ledger "$game"
  n="$(awk -F'\t' -v f="$feat" 'NR>3 && $2==f {c++} END{print (c+0)+1}' "$l")"
  printf '%s\t%s\t%s\t%s\t%s\t%s\n' \
    "$(date '+%Y-%m-%d %H:%M')" "$feat" "$n" "$note" "pipeline.log" "$verdict" >> "$l"
  echo "✅ 已记：功能#${feat} 第 ${n} 轮 → ${verdict}${note:+（${note}）}"
  if ledger_block_check "$game" "$feat"; then
    echo "⛔ 功能#${feat} 已连续 3 轮无效 —— 下次装机将被拒绝。先跑 pipeline.sh ${game} rounds 填假设矩阵并贴给用户。"
  fi
  return 0
}

ledger_block_check() {  # 0 = 已被门禁盯上（连续 >=3 轮无效）；1 = 未触发
  local game="$1" feat="$2" l streak
  l="$(ledger_path "$game")"
  [ -f "$l" ] || return 1
  streak="$(awk -F'\t' -v f="$feat" '
    NR>3 && $2==f {v[++n]=$6}
    END{ c=0; for(i=n;i>=1;i--){ if(v[i]=="无效") c++; else break } print c }' "$l")"
  [ "${streak:-0}" -ge 3 ] && return 0
  return 1
}

ledger_summary() {
  local game="$1" l
  l="$(ledger_path "$game")"
  if [ ! -f "$l" ]; then
    echo "──── 轮次台账：尚未建立（当前管线轮次无记录 ⇒ 门禁不会拦）────"
    return 0
  fi
  echo "──── 轮次台账 ${l} ────"
  awk -F'\t' 'NR>3' "$l" | tail -n 12 | while IFS= read -r line; do
    printf '  %s\n' "$(printf '%s' "$line" | tr '\t' ' ')"
  done
  echo "  连续无效轮次统计:"
  awk -F'\t' '
    NR>3 {f[$2]=1; v[$2"|"++n[$2]]=$6}
    END{ for (k in f) { c=0
           for(i=n[k];i>=1;i--){ if(v[k"|"i]=="无效") c++; else break }
           printf "    功能#%-4s 第 %d 轮后连续无效 %d 轮\n", k, n[k], c } }' "$l" | sort
}

print_matrix() {  # P4：被门禁拦住时照它填，填完贴给用户确认方向
  local game="$1"
  echo ""
  echo "════════ 假设矩阵（必须先填完再谈继续装机；pipeline.sh <游戏> rounds 生成）════════"
  ledger_summary "$game"
  echo ""
  echo "【已排除的假设】假设 / 反证证据行（文件:行 或 logcat 行）"
  echo "  1. "
  echo "  2. "
  echo "【待验证的假设】假设 / 验证方法（命令） / 代价（分钟）"
  echo "  1. "
  echo "  2. "
  echo "【已确定的事实】事实 / 证据行"
  echo "  1. "
  echo "════════ 填完贴给用户确认方向；确认前禁止再装机（第 3 轮起）════════"
}

gate_before_stage() {  # P4：只拦「装机」阶段；被拦即 exit 3（STAGE_NEXT_FORCE=1 放行）
  local game="$1" sid="$2" l blocked="" feat
  case "$sid" in
    *install*|*Install*) ;;
    *) return 0 ;;
  esac
  l="$(ledger_path "$game")"
  [ -f "$l" ] || return 0
  for feat in $(awk -F'\t' 'NR>3 {print $2}' "$l" | sort -u); do
    if ledger_block_check "$game" "$feat"; then blocked="${blocked} ${feat}"; fi
  done
  [ -n "$blocked" ] || return 0
  echo "" >&2
  echo "⛔ 轮次门禁：功能#${blocked# } 已连续 3 轮「改动→装机→仍无效」—— 拒绝执行装机阶段 [${sid}]。" >&2
  echo "   绕圈不是勤快，是烧用户的测试时间（AGENTS.md「执行纪律补充」P4）。" >&2
  print_matrix "$game" >&2
  if [ "${STAGE_NEXT_FORCE:-0}" = "1" ]; then
    echo "" >&2
    echo "⚠  STAGE_NEXT_FORCE=1：用户当轮明确要求再试一次 ⇒ 放行本次装机（已在日志与汇报中留痕）。" >&2
    return 0
  fi
  echo "   若用户当轮明确要求「再试一次」，用 STAGE_NEXT_FORCE=1 显式放行。" >&2
  exit 3
}

# ---------- 0. 指纹自检（防改脚本） ----------
self_check() {
  [ -f "$LOCK_FILE" ] || die "指纹锁缺失: ${LOCK_FILE}（请按 _common/stages.md「真要改 pipeline.sh 时的跨机姿势」重新生成，勿手工创建）"
  local expected actual
  expected="$(sed -n 's/^pipeline_sh_sha256=//p' "$LOCK_FILE" | tr -d '[:space:]')"
  actual="$(shasum -a 256 "$0" 2>/dev/null | awk '{print $1}')"
  [ -n "$expected" ] && [ "$expected" = "$actual" ] || die "pipeline.sh 已被改动（指纹不匹配）。
  机械管线禁止随手修改——请还原（git checkout）或按 _common/stages.md「真要改 pipeline.sh 时的跨机姿势」修复（同一提交改脚本 + 刷新 lock + push）。
  期望指纹: ${expected:-<lock缺失>}
  实际指纹: ${actual:-<计算失败>}"
}

# ---------- 用法与游戏发现 ----------
list_games() {
  local d
  for d in "${SCRIPT_DIR}"/*/; do
    [ -f "${d}pipeline.json" ] && basename "$d"
  done
}

usage() {
  cat <<EOF
用法: bash 04_scripts/pipeline.sh <游戏> <动作> [参数]

动作:
  status [--brief]              查看状态（开工第一步；默认同时钉出硬约束生效条目与轮次台账；--brief 只回显阶段行 + 下一步）
  next [--loop N]               # 执行下一阶段；--loop N 连跑最多 N 个阶段（逐次回显日志尾 ${TAIL_LINES} 行）
  run <stage_id>                # 显式执行某阶段
  reset <stage_id>              # 状态回退 pending
  verify <stage_id> [--set]     # 只校验产物不执行；--set＝校验通过即置 done（用于「产物已在、状态需补」的收尾）
  log [N]                       # 只回显 pipeline.log 尾 N 行（默认 ${TAIL_LINES}）
  round <功能#> <有效|无效|未测> [摘要]   # 记一轮「改代码→装机→用户实测」结果（写 10_docs/<游戏>/轮次台账.tsv）
  rounds                        # 打假设矩阵（被轮次门禁拦住时照它填，填完贴给用户确认方向）
  gate <stage_id>               # 只跑装机前轮次门禁（不执行阶段；被拦时退出码 3）

已注册游戏（04_scripts/*/pipeline.json）:
$(list_games | sed 's/^/  - /')

规则见 ${COMMON_DIR}/stages.md（操作二分 / 阶段表 / 失败即停）

轮次门禁（P4，2026-10-03）:
  - 同一功能「改代码→装机→用户实测」连续 3 轮仍「无效」时，含 install 的阶段直接拒绝
    （退出码 3）并打印假设矩阵；仅当用户当轮明确要求「再试一次」时用 STAGE_NEXT_FORCE=1 放行。
  - 结果由 AI 用 `round` 动词逐轮登记，别靠记忆（AGENTS.md「执行纪律补充」P4）。

防旧产物直装机制（自动生效，无需记忆）:
  - 上游阶段产物变化后，仍处 done 的下游阶段自动回退 pending（next 连跑即刷新）
  - 上游链路过期时，本阶段拒绝执行（改了源码没重编、换了 so 没重打、都会拦）
  - install 阶段后端应接 _common/verify_installed_apk.sh 校验设备包 hash
EOF
}

# ---------- python3 json 助手 ----------
json_py() {  # json_py <json路径> <python代码>；游戏目录作为 P4Game 注入
  local jf="$1"; shift
  python3 - "$jf" "$@" <<'PYEOF'
import json, sys, os, time, hashlib
jf = sys.argv[1]
data = json.load(open(jf, encoding="utf-8"))

def out(x):
    print(x)

cmd = sys.argv[2] if len(sys.argv) > 2 else ""
gdir = os.path.dirname(jf)
workdir_root = data.get("workdir", gdir)
stages = data.get("stages", [])
by_id = {s["id"]: s for s in stages}

# ---------- 陈旧度机制公共设施（2026-09-10 实战教训固化） ----------
# 背景：reset build_so 后忘重跑下游 → 旧 so/旧包照常"装机成功"。产物指纹只防
# "产物被人改过"，防不了"上游换了新产物、本阶段没重跑"。故引入 inputs 快照：
# 阶段完成时记录其上游产物的 md5；之后任一环节发现快照≠现状 = 本阶段过期。
# 排除两类"活文件"：pipeline.json（状态机自身，每次写都会变）与 *.md（分析
# 文档，迭代期持续演进，不参与机器新鲜度判定）。
def _is_state_file(p):
    return os.path.basename(p) == "pipeline.json"

def _is_doc_file(p):
    return p.endswith(".md")

def _resolve(p):
    return p if os.path.isabs(p) else os.path.normpath(os.path.join(workdir_root, p))

def _md5_of(p):
    try:
        return hashlib.md5(open(p, "rb").read()).hexdigest()
    except Exception:
        return None

def _stage_inputs_of(s):
    """本阶段上游产物快照：{path: md5}（剔除活文档/状态文件）"""
    inp = {}
    for r in s.get("requires", []):
        for p in (by_id.get(r, {}) or {}).get("artifacts", {}):
            if _is_state_file(p) or _is_doc_file(p):
                continue
            h = _md5_of(_resolve(p))
            if h:
                inp[p] = h
    return inp

def _stale_inputs(s):
    """inputs 快照 vs 上游产物现状；非空 = 本阶段在上游变化后未重跑"""
    bad = []
    for p, rec_h in (s.get("inputs") or {}).items():
        if _md5_of(_resolve(p)) != rec_h:
            bad.append(p)
    return bad

def _transitive_ancestors(sid):
    seen, stack = set(), list((by_id.get(sid, {}) or {}).get("requires", []))
    while stack:
        r = stack.pop()
        if r in seen:
            continue
        seen.add(r)
        stack.extend((by_id.get(r, {}) or {}).get("requires", []))
    return seen

def _save():
    json.dump(data, open(jf, "w", encoding="utf-8"), ensure_ascii=False, indent=2)

if cmd == "status":
    brief = "--brief" in sys.argv
    gname = os.path.basename(gdir)
    stages = data.get("stages", [])
    cur = next((i for i, s in enumerate(stages) if s.get("status") == "pending"), len(stages))
    print(f"游戏: {data.get('game')}  版本: {data.get('version','')}  阶段: {cur}/{len(stages)}")
    if not brief:
        print(f"说明: {data.get('notes','')}")
    print("-" * 68)
    for i, s in enumerate(stages):
        mark = {"pending": "⬜", "done": "✅", "failed": "❌", "skipped": "⏭"}.get(s.get("status"), "❓")
        req = ",".join(s.get("requires", [])) or "-"
        arts = s.get("artifacts")
        art_str = ""
        if arts:
            changed = [k for k, v in arts.items() if isinstance(v, dict) and v.get("changed")]
            art_str = f"  [产物指纹{'⚠️已变化' if changed else '✓'}]" if arts else ""
        if s.get("status") == "done" and s.get("inputs") and _stale_inputs(s):
            art_str += "  [⚠️上游已更新，本阶段过期（需重跑）]"
        if brief:
            print(f" {mark} {i+1}. {s['id']:<12} {s.get('status'):<8}{art_str}")
            if s.get("status") == "failed" and s.get("error"):
                print(f"      错误: {s['error']}")
            continue
        print(f" {mark} {i+1}. {s['id']:<12} {s.get('name','')}")
        print(f"      前置: {req}   状态: {s.get('status')}{art_str}")
        if s.get("status") == "failed" and s.get("error"):
            print(f"      错误: {s['error']}")
    print("-" * 68)
    nxt = stages[cur] if cur < len(stages) else None
    if nxt:
        print(f"下一步: bash 04_scripts/pipeline.sh {gname} next   （将执行: {nxt['id']} {nxt.get('name','')}）")
    else:
        print("全部阶段完成或无待执行阶段。")
    if brief:
        print("（--brief：已省略 说明 / 阶段名 / 前置 与「硬约束 + 台账」回显；要全量用不带 --brief 的 status）")

elif cmd == "get_next":
    stages = data.get("stages", [])
    for s in stages:
        if s.get("status") == "pending":
            out(s["id"]); sys.exit(0)
    sys.exit(1)

elif cmd == "stage_info":
    sid = sys.argv[3]
    for s in data.get("stages", []):
        if s["id"] == sid:
            out(json.dumps(s, ensure_ascii=False)); sys.exit(0)
    print(f"stage 不存在: {sid}", file=sys.stderr); sys.exit(1)

elif cmd == "check_prereq":
    sid = sys.argv[3]
    stages = {s["id"]: s for s in data.get("stages", [])}
    missing = [r for r in stages.get(sid, {}).get("requires", []) if stages.get(r, {}).get("status") != "done"]
    if missing:
        print("前置未满足: " + ", ".join(missing)); sys.exit(1)
    sys.exit(0)

elif cmd == "verify":
    sid = sys.argv[3]
    stage = next((s for s in data.get("stages", []) if s["id"] == sid), None)
    if not stage:
        print(f"stage 不存在: {sid}", file=sys.stderr); sys.exit(1)
    workdir = data.get("workdir", gdir)
    rules = stage.get("verify", [])
    artifacts = stage.get("artifacts", {})
    ok_all, report = True, []
    for r in rules:
        p = os.path.normpath(os.path.join(workdir, r["path"])) if not os.path.isabs(r["path"]) else r["path"]
        t = r["type"]
        if t == "file_exists":
            ok = os.path.isfile(p)
            msg = f"存在: {p}" if ok else f"缺失: {p}"
        elif t == "not_exists":
            ok = not os.path.exists(p)
            msg = "已清理" if ok else f"仍存在: {p}"
        elif t == "file_contains":
            try:
                raw = open(p, "rb").read()
                ok = r["pattern"].encode() in raw
            except Exception:
                ok = False
            msg = f"含标签[{r['pattern']}]: {'✓' if ok else '✗'}"
        else:
            ok, msg = True, f"未知校验类型(跳过): {t}"
        report.append(("  " + ("✓ " if ok else "✗ ") + msg))
        ok_all = ok_all and ok
    # 产物指纹比对（检测"改错文件"）；跳过 pipeline.json 状态文件与 .md 活文档
    # （前者随状态机写入而变、后者随迭代演进——指纹化只会永久假告警）
    changed = []
    for path, rec in artifacts.items():
        if _is_state_file(path) or _is_doc_file(path):
            rec.pop("changed", None)
            continue
        ap = os.path.normpath(os.path.join(workdir, path)) if not os.path.isabs(path) else path
        try:
            h = hashlib.md5(open(ap, "rb").read()).hexdigest()
            if h != rec.get("md5"):
                changed.append(path)
                rec["changed"] = True
            else:
                rec.pop("changed", None)
        except Exception:
            changed.append(f"{path}(读取失败)")
            rec["changed"] = True
    if changed:
        report.append("  ⚠ 产物指纹变化: " + ", ".join(changed) + "（可能被改动过，请核对）")
        ok_all = False  # 产物被动过=硬失败，不给"警告即通过"的错觉
    if artifacts:  # 无论变化与否都写回（还原后要清掉 changed 标志）
        json.dump(data, open(jf, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
    if ok_all:
        print("校验通过:"); [print(x) for x in report]; sys.exit(0)
    else:
        print("校验失败:"); [print(x) for x in report]; sys.exit(1)

elif cmd == "check_stale":
    # 上游链路陈旧度门禁：任一祖先阶段的 inputs 快照与上游产物现状不符 →
    # 拒绝执行（防"改了源码没重编 so、换了 so 没重打 APK，拿旧产物继续"）。
    # 只看 inputs 快照、不比祖先自身产物指纹：源码文件本身就是上游阶段的
    # "产物"（impl_mod→HzsjMod.cpp），改源码后 reset 重编是正常调试循环，
    # 若比自身指纹会把合法循环全部误杀。自身指纹归 verify 显式校验管。
    sid = sys.argv[3]
    reasons = []
    for aid in sorted(_transitive_ancestors(sid)):
        a = by_id.get(aid)
        if not a:
            continue
        for p, rec_h in (a.get("inputs") or {}).items():
            if _md5_of(_resolve(p)) != rec_h:
                reasons.append(f"{aid}: 上游产物已变化 {p}")
    if reasons:
        for r in reasons:
            out("  " + r)
        out(f"阶段 {sid} 的上游链路已过期——继续执行=旧产物直装风险。")
        sys.exit(1)
    sys.exit(0)

elif cmd == "set_status":
    sid, st = sys.argv[3], sys.argv[4]
    err = sys.argv[5] if len(sys.argv) > 5 else ""
    for s in data.get("stages", []):
        if s["id"] == sid:
            s["status"] = st
            if st == "done":
                s["done_at"] = time.strftime("%Y-%m-%d %H:%M:%S")
                # 记录产物指纹（verify 声明的 file_exists 产物；剔除 pipeline.json
                # 状态文件——它随状态机写入而变，指纹化只会产生永久假告警）
                arts = {}
                for r in s.get("verify", []):
                    if r["type"] == "file_exists" and not _is_state_file(r["path"]):
                        h = _md5_of(_resolve(r["path"]))
                        if h:
                            arts[r["path"]] = {"md5": h}
                s["artifacts"] = arts
                # 上游产物快照（陈旧度机制的锚点）
                inp = _stage_inputs_of(s)
                if inp:
                    s["inputs"] = inp
                else:
                    s.pop("inputs", None)
            elif st == "failed":
                s["failed_at"] = time.strftime("%Y-%m-%d %H:%M:%S")
                s["error"] = err[:300]
            elif st == "pending":
                s.pop("done_at", None); s.pop("failed_at", None); s.pop("error", None)
                s.pop("artifacts", None); s.pop("inputs", None)
            break
    _save()
    # 一致性扫掠：上游产物已变化而仍处 done 的阶段自动回退 pending。
    # 这就是"reset 上游忘重跑下游 → 旧包直装"事故的结构性根治：
    # 上游一完成，过期的下游立刻被打回 pending，next 连跑即刷新。
    if st == "done":
        pended = []
        for d in data.get("stages", []):
            if d.get("status") != "done" or d["id"] == sid:
                continue
            if _stale_inputs(d):
                d["status"] = "pending"
                for k in ("done_at", "failed_at", "error", "artifacts", "inputs"):
                    d.pop(k, None)
                pended.append(d["id"])
        if pended:
            _save()
            for pid in pended:
                out(f"⚠ 自动回退 [{pid}] → pending（上游产物已变化，本阶段产物过期；连跑 next 刷新）")

elif cmd == "export_env":
    for k, v in data.get("env", {}).items():
        print(f"{k}\t{v}")

elif cmd == "workdir":
    out(data.get("workdir", gdir))
PYEOF
}

# ---------- 阶段执行 ----------
run_stage() {
  local game="$1" sid="$2"
  local jf="${SCRIPT_DIR}/${game}/pipeline.json"
  local log="${SCRIPT_DIR}/${game}/pipeline.log"

  # 装机前轮次门禁（P4）：同一功能连续 3 轮无效 ⇒ 拒绝装机阶段（exit 3，除非 STAGE_NEXT_FORCE=1）
  gate_before_stage "$game" "$sid"

  # 前置检查
  if ! json_py "$jf" check_prereq "$sid"; then
    die "阶段 ${sid} 前置未满足（顺序执行是铁律；确需重跑请先 reset 前置阶段）"
  fi

  # 上游链路陈旧度门禁（防旧产物直装）：任一祖先阶段的产物/其上游快照与现状
  # 不符（如改了 Mod 源码没重编 so、换了 so 没重打 APK），本阶段拒绝执行
  if ! json_py "$jf" check_stale "$sid"; then
    die "阶段 ${sid} 上游链路已过期：旧产物直装风险。先重跑上游（reset <上游阶段> 后 next 连跑），不要绕过。"
  fi

  local stage_json workdir
  stage_json="$(json_py "$jf" stage_info "$sid")" || die "stage_info 失败"
  workdir="$(json_py "$jf" workdir)"
  [ -d "$workdir" ] || die "workdir 不存在: ${workdir}"

  local command
  command="$(printf '%s' "$stage_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("command","true"))')"

  # env 展开（来自 json 的 env 字段）
  while IFS=$'\t' read -r k v; do
    [ -n "$k" ] && export "$k=$v"
  done <<ENV
$(json_py "$jf" export_env)
ENV

  # preflight 机器关（阶段可声明 preflight 文件列表：执行命令前先过语法/格式检查）
  local pf_files pf_one
  pf_files="$(printf '%s' "$stage_json" | python3 -c 'import json,sys
files = json.load(sys.stdin).get("preflight", [])
print("\n".join(files))')"
  if [ -n "$pf_files" ]; then
    echo "▶ preflight 机器关（阶段声明文件，执行前先检查）"
    local pf_args=()
    while IFS= read -r pf_one; do
      [ -n "$pf_one" ] || continue
      case "$pf_one" in
        /*) pf_args+=("$pf_one") ;;
        *)  pf_args+=("${workdir}/${pf_one}") ;;
      esac
    done <<PFEOF
$pf_files
PFEOF
    if ! bash "${COMMON_DIR}/preflight.sh" "${pf_args[@]}"; then
      json_py "$jf" set_status "$sid" failed "preflight 机器关未通过（语法/格式错误，修复后重跑）"
      die "阶段 ${sid} preflight 未通过：改动的脚本/配置有语法或格式错误，先修复再执行。"
    fi
  fi

  {
    echo ""
    echo "${LOG_SEP}"
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] [${game}] stage=${sid} 开始"
    echo "命令: ${command}"
    echo "目录: ${workdir}"
  } >> "$log"

  echo "▶ 执行阶段 [${sid}]（日志: ${log}）"

  # 执行（stdout/stderr 同步入日志与终端）
  if ( cd "$workdir" && bash -c "$command" ) >> "$log" 2>&1; then
    echo "  命令执行成功，开始产物校验…"
  else
    local rc=$?
    json_py "$jf" set_status "$sid" failed "命令退出码 ${rc}（详见日志尾部）"
    {
      echo "[$(date '+%Y-%m-%d %H:%M:%S')] [${game}] stage=${sid} 命令失败 rc=${rc}"
      echo "${LOG_SEP}"
    } >> "$log"
    tail -15 "$log"
    die "阶段 ${sid} 命令失败（rc=${rc}）。失败即停：读日志定位原因，修复后 reset 重跑；禁止换命令绕过。"
  fi

  # 产物校验
  if json_py "$jf" verify "$sid"; then
    json_py "$jf" set_status "$sid" done
    {
      echo "[$(date '+%Y-%m-%d %H:%M:%S')] [${game}] stage=${sid} 完成（校验通过）"
      echo "${LOG_SEP}"
    } >> "$log"
    echo "✅ 阶段 [${sid}] 完成"
    echo "下一步: bash 04_scripts/pipeline.sh ${game} next"
  else
    json_py "$jf" set_status "$sid" failed "产物校验未通过（详见上方校验输出）"
    {
      echo "[$(date '+%Y-%m-%d %H:%M:%S')] [${game}] stage=${sid} 校验失败"
      echo "${LOG_SEP}"
    } >> "$log"
    die "阶段 ${sid} 产物校验失败。失败即停：核对产物路径/标签是否正确。"
  fi
}

# ---------- 主流程 ----------
main() {
  [ $# -ge 2 ] || { usage; exit 1; }
  self_check

  local game="$1" action="$2"
  local jf="${SCRIPT_DIR}/${game}/pipeline.json"
  [ -f "$jf" ] || die "游戏未注册: ${game}（缺 ${jf}）。已注册: $(list_games | tr '\n' ' ')。新游戏管线按 _common/stages.md 的阶段定义生成。"

  case "$action" in
    status)
      local brief=0
      [ "${3:-}" = "--brief" ] && brief=1
      if [ "${brief}" = 0 ]; then
        pin_constraints "$game"
        echo ""
        json_py "$jf" status
        echo ""
        ledger_summary "$game"
      else
        json_py "$jf" status --brief
      fi
      ;;
    next)
      local sid i=0 loop=1
      case "${3:-}" in
        --loop|-n) loop="${4:-0}" ;;
        "") ;;
        *) die "用法: pipeline.sh ${game} next [--loop N]" ;;
      esac
      case "$loop" in ''|*[!0-9]*) die "用法: pipeline.sh ${game} next [--loop N]（N 为正整数）" ;; esac
      [ "$loop" -ge 1 ] || die "用法: pipeline.sh ${game} next [--loop N]（N 为正整数）"
      while [ "$i" -lt "$loop" ]; do
        i=$((i + 1))
        sid="$(json_py "$jf" get_next)" || { echo "无待执行阶段（全部完成/失败）。查看状态: pipeline.sh ${game} status"; break; }
        if [ "$loop" -gt 1 ]; then
          echo ""
          echo "──── next --loop ${i}/${loop} ────"
        fi
        echo "下一阶段: ${sid}"
        run_stage "$game" "$sid"
        if [ "$loop" -gt 1 ]; then
          echo "──── pipeline.log 尾 ${TAIL_LINES} 行 ────"
          tail -n "$TAIL_LINES" "${SCRIPT_DIR}/${game}/pipeline.log"
        fi
      done
      ;;
    run)
      [ $# -ge 3 ] || die "用法: pipeline.sh ${game} run <stage_id>"
      run_stage "$game" "$3"
      ;;
    reset)
      [ $# -ge 3 ] || die "用法: pipeline.sh ${game} reset <stage_id>"
      json_py "$jf" set_status "$3" pending
      echo "已回退 [${3}] → pending"
      ;;
    verify)
      [ $# -ge 3 ] || die "用法: pipeline.sh ${game} verify <stage_id> [--set]"
      if json_py "$jf" verify "$3"; then
        if [ "${4:-}" = "--set" ]; then
          json_py "$jf" set_status "$3" done
          echo "✅ 已按校验结果置 [${3}] → done（--set；同时刷新产物指纹与上游快照）"
        fi
      else
        die "阶段 ${3} 产物校验未通过（--set 未生效）"
      fi
      ;;
    log)
      local logf="${SCRIPT_DIR}/${game}/pipeline.log" ln
      ln="${3:-$TAIL_LINES}"
      case "$ln" in ''|*[!0-9]*) die "用法: pipeline.sh ${game} log [N]" ;; esac
      [ -f "$logf" ] || die "无日志: ${logf}（本游戏尚无阶段执行记录）"
      echo "──── pipeline.log 尾 ${ln} 行 ────"
      tail -n "$ln" "$logf"
      ;;
    rounds)
      print_matrix "$game"
      ;;
    round)
      [ $# -ge 4 ] || die "用法: pipeline.sh ${game} round <功能#> <有效|无效|未测> [改动摘要]"
      ledger_round "$game" "$3" "$4" "${5:-}"
      ;;
    gate)
      [ $# -ge 3 ] || die "用法: pipeline.sh ${game} gate <stage_id>  # 只跑装机前轮次门禁，不执行阶段"
      gate_before_stage "$game" "$3"
      echo "✅ 轮次门禁：阶段 [${3}] 未被拦（无功能连续 3 轮无效）"
      ;;
    *)
      usage; die "未知动作: ${action}"
      ;;
  esac
}

main "$@"
