#!/usr/bin/env bash
# ================================================================
#  pipeline.sh — 机械操作统一入口（状态机管线）
#
#  铁律（详见 _common/stages.md）：
#    1. 机械操作（拆/打/注/移/签/装/清理）唯一合法入口=本脚本；
#       禁止直接敲 adb/apktool/zip/ndk-build 等命令序列。
#    2. 本文件与 pipeline.lock 禁止修改：启动即校验 sha256 指纹。
#    3. 阶段失败即停：读日志修原因，禁止换命令绕过。
#    4. 合法参数修改走 04_scripts/{游戏}/pipeline.json 的 params。
#
#  用法：
#    bash 04_scripts/pipeline.sh <游戏> status          # 查看状态（开工第一步）
#    bash 04_scripts/pipeline.sh <游戏> next            # 执行下一阶段
#    bash 04_scripts/pipeline.sh <游戏> run <stage_id> # 显式执行某阶段
#    bash 04_scripts/pipeline.sh <游戏> reset <stage_id># 状态回退 pending
#    bash 04_scripts/pipeline.sh <游戏> verify <stage_id># 只校验产物不执行
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
COMMON_DIR="${SCRIPT_DIR}/_common"
LOCK_FILE="${COMMON_DIR}/pipeline.lock"
LOG_SEP="────────────────────────────────────────────"

die() { echo "❌ $*" >&2; exit 1; }

# ---------- 0. 指纹自检（防改脚本） ----------
self_check() {
  [ -f "$LOCK_FILE" ] || die "指纹锁缺失: ${LOCK_FILE}（请用强 AI 重新生成，勿手工创建）"
  local expected actual
  expected="$(sed -n 's/^pipeline_sh_sha256=//p' "$LOCK_FILE" | tr -d '[:space:]')"
  actual="$(shasum -a 256 "$0" 2>/dev/null | awk '{print $1}')"
  [ -n "$expected" ] && [ "$expected" = "$actual" ] || die "pipeline.sh 已被改动（指纹不匹配）。
  机械管线禁止修改——请还原（git checkout）或请强 AI 修复。
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
用法: bash 04_scripts/pipeline.sh <游戏> <status|next|run|reset|verify> [stage_id]

已注册游戏（04_scripts/*/pipeline.json）:
$(list_games | sed 's/^/  - /')

规则见 ${COMMON_DIR}/stages.md（操作二分 / 阶段表 / 失败即停）
EOF
}

# ---------- python3 json 助手 ----------
json_py() {  # json_py <json路径> <python代码>；游戏目录作为 P4Game 注入
  local jf="$1"; shift
  python3 - "$jf" "$@" <<'PYEOF'
import json, sys, os, time
jf = sys.argv[1]
data = json.load(open(jf, encoding="utf-8"))

def out(x):
    print(x)

cmd = sys.argv[2] if len(sys.argv) > 2 else ""
gdir = os.path.dirname(jf)

if cmd == "status":
    gname = os.path.basename(gdir)
    stages = data.get("stages", [])
    cur = next((i for i, s in enumerate(stages) if s.get("status") == "pending"), len(stages))
    print(f"游戏: {data.get('game')}  版本: {data.get('version','')}  阶段: {cur}/{len(stages)}")
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
    # 产物指纹比对（检测"改错文件"）
    changed = []
    for path, rec in artifacts.items():
        ap = os.path.normpath(os.path.join(workdir, path)) if not os.path.isabs(path) else path
        try:
            import hashlib
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
        ok_all = False  # 产物被动过=硬失败，不给弱模型"警告即通过"的错觉
    if artifacts:  # 无论变化与否都写回（还原后要清掉 changed 标志）
        json.dump(data, open(jf, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
    if ok_all:
        print("校验通过:"); [print(x) for x in report]; sys.exit(0)
    else:
        print("校验失败:"); [print(x) for x in report]; sys.exit(1)

elif cmd == "set_status":
    sid, st = sys.argv[3], sys.argv[4]
    err = sys.argv[5] if len(sys.argv) > 5 else ""
    for s in data.get("stages", []):
        if s["id"] == sid:
            s["status"] = st
            if st == "done":
                s["done_at"] = time.strftime("%Y-%m-%d %H:%M:%S")
                # 记录产物指纹（verify 声明的 file_exists 产物）
                workdir = data.get("workdir", gdir)
                import hashlib
                arts = {}
                for r in s.get("verify", []):
                    if r["type"] == "file_exists":
                        p = os.path.normpath(os.path.join(workdir, r["path"])) if not os.path.isabs(r["path"]) else r["path"]
                        try:
                            arts[r["path"]] = {"md5": hashlib.md5(open(p, "rb").read()).hexdigest()}
                        except Exception:
                            pass
                if arts:
                    s["artifacts"] = arts
            if st == "failed":
                s["failed_at"] = time.strftime("%Y-%m-%d %H:%M:%S")
                s["error"] = err[:300]
            if st == "pending":
                s.pop("done_at", None); s.pop("failed_at", None); s.pop("error", None); s.pop("artifacts", None)
            break
    json.dump(data, open(jf, "w", encoding="utf-8"), ensure_ascii=False, indent=2)

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

  # 前置检查
  if ! json_py "$jf" check_prereq "$sid"; then
    die "阶段 ${sid} 前置未满足（顺序执行是铁律；确需重跑请先 reset 前置阶段）"
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
  [ -f "$jf" ] || die "游戏未注册: ${game}（缺 ${jf}）。已注册: $(list_games | tr '\n' ' ')。新游戏管线由强 AI 生成（见 _common/stages.md）。"

  case "$action" in
    status)
      json_py "$jf" status
      ;;
    next)
      local sid
      sid="$(json_py "$jf" get_next)" || die "无待执行阶段（全部完成/失败）。查看状态: pipeline.sh ${game} status"
      echo "下一阶段: ${sid}"
      run_stage "$game" "$sid"
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
      [ $# -ge 3 ] || die "用法: pipeline.sh ${game} verify <stage_id>"
      json_py "$jf" verify "$3"
      ;;
    *)
      usage; die "未知动作: ${action}"
      ;;
  esac
}

main "$@"
