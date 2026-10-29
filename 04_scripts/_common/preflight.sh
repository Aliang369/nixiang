#!/usr/bin/env bash
# ================================================================
#  preflight.sh — 文件变更检查关（改完任何文件后必跑；全部会话/所有模型一体适用）
#
#  用法:
#    bash 04_scripts/_common/preflight.sh <文件1> [文件2 ...]
#    bash 04_scripts/_common/preflight.sh                        # 无参数 = 全仓 bash 3.2 扫描（见下）
#    bash 04_scripts/_common/preflight.sh --sweep                # 同上，显式写法
#    bash 04_scripts/_common/preflight.sh --env <文件...>        # 顺带跑环境体检
#    bash 04_scripts/_common/preflight.sh --env                 # 只跑环境体检
#
#  无参 / --sweep（2026-10-12 霓虹深渊2 会话复盘 P8 收尾）：把
#  `04_scripts/*/*.sh` + `04_scripts/_common/*.sh` + `mod菜单/*/scripts/*.sh`
#  + `mod菜单/*/scripts/_lib/*.sh`（2026-10-15 保卫向日葵复盘补：原 glob 漏了 `_lib/`，
#  既有的 `dex_target.sh` 与新加的 `check_shared_tables.sh` 都一直不在扫描范围内）
#  全量过一遍 bash32_check.sh。**为什么需要**：bash32 门禁本来是**按文件传参**的，
#  于是"脚本存在 ≠ 门禁生效"—— 2026-10-12 实测全仓 633 个脚本里 63 个带 ✗ 硬错误，
#  没人全仓跑过它。本模式让新脚本进来时自己被抓，不再靠"有人想起来扫一遍"。
#  列表上限由 PF_SWEEP_LIST 控制（默认 20 个文件）。
#
#  --env：额外执行 `04_scripts/_common/env_deps.sh` —— 检查 bash 版本、宿主判定、
#    NIXIANG_* 解析、python/Pillow、python3.11（win 侧垫片）、apksigner/zipalign/
#    adb/java、cygpath/sips/md5/stat 的可用性与 mac 优先序。用于"动真格前"确认机器，
#    环境有 FAIL 时整体 rc=1（与文件检查同样挡关）。
#
#  按扩展名自动路由检查器：
#    .sh   → bash -n + bash 3.2 兼容门禁（macOS 自带 bash 就是 3.2；见下 "bash 3.2 门禁"）
#            + shell 环境坑位 lint（lint_shell_traps.py：rg 不存在 / 设备侧 $((0x…)) /
#              BRE 方括号 grep / 大包直接哈希 / timeout / grep -c 进 &&；ERROR 级挡关）
#  bash 3.2 门禁（2026-09-30 起，伊莫复盘 P1-3）：本仓库一律在 macOS bash 3.2 下运行，
#    bash 4+ 的语法会**整套脚本直接跑不起来**，`set -u` + 空数组/多字节变量名会**静默少做事**。
#    检查项：① `$VAR` 紧跟非 ASCII 字符（bash 会把中文并进变量名 ⇒ unbound variable；
#            实测 `$HAS_CXXSHARED（` 就是这么炸的）② 空数组展开 `${arr[@]}`/`${#arr[@]}`
#            （提示改 `${arr[@]+"${arr[@]}"}` 或改计数器）③ bash 4+ 语法
#            （`mapfile`/`readarray`/`declare -A`/`local -n`/`${v^^}`/`;;&`/`&>>`/`[[ -v x ]]`）。
#    .py   → compile() 内存检查（不落 pyc 副作用）
#    .js   → node --check     （node 缺失则 SKIP 提示）
#    .json → json.tool 校验
#    .cpp/.cc/.c/.h/.hpp/.mk → ⚠ 无独立语法检查器：必须重跑管线构建阶段验证
#    .md（仅 AGENTS.md）→ 入口体量：>30720 B REMIND（30 KB 工作上限）/ >32768 B FAIL（D6 闸门）
#            2026-10-30 补：D6 原先只在 twin_status 查，等于"撞线才清"；这里提前预警，
#            配合 AGENTS.md §1「入口增量纪律」把清理成本摊到每次改动。
#    其他  → PASS（跳过）
#
#  退出码: 0=全部通过/SKIP/REMIND；1=存在 FAIL（必须修复后再继续）
#  铁律: 这是"改完文件"的机器关——多一个符号、少个括号在这里抓，
#        不要等到装机/进游戏才发现返工。
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

fail=0; pass=0; skip=0; remind=0; cpp_remind=0; sweep_files=0

check_one() {
  local f="$1"
  if [ ! -f "$f" ]; then
    echo "✗ FAIL  [缺失] $f"
    fail=$((fail+1)); return
  fi
  local ext="${f##*.}"
  case "$ext" in
    sh)
      if bash -n "$f" 2>/tmp/pf_err_$$; then
        echo "✓ PASS  [bash语法] $f"; pass=$((pass+1))
      else
        echo "✗ FAIL  [bash语法] $f"
        sed 's/^/       /' /tmp/pf_err_$$
        fail=$((fail+1))
      fi
      rm -f /tmp/pf_err_$$
      # ---- bash 3.2 兼容门禁（macOS 默认 bash；bash 4+ 语法会整套脚本跑不起来）----
      compat_out=$(bash "$SCRIPT_DIR/bash32_check.sh" "$f" 2>&1) || {
        echo "$compat_out" | sed 's/^/       /'
        case "$compat_out" in
          *"✗"*) fail=$((fail+1)) ;;
          *)     remind=$((remind+1)) ;;
        esac
      }
      # ---- shell 环境坑位 lint（2026-10-10 复盘 P2）：rg 不存在 / 设备侧 $((0x…)) /
      #      BRE 方括号 grep / 大包直接哈希 / timeout / grep -c 进 && —— 这些坑本会话各炸过一次
      if [ -f "$SCRIPT_DIR/lint_shell_traps.py" ]; then
        lint_out=$(python3 "$SCRIPT_DIR/lint_shell_traps.py" "$f" 2>&1); lint_rc=$?
        if [ -n "$lint_out" ]; then echo "$lint_out" | sed 's/^/       /'; fi
        if [ "$lint_rc" != "0" ]; then
          echo "       ✗ FAIL  [坑位lint] ${f}（ERROR 级：本机会静默失败，必须先修）"
          fail=$((fail+1))
        elif [ -n "$lint_out" ]; then
          remind=$((remind+1))
        fi
      fi
      ;;
    py)
      if python3 -c 'import sys; compile(open(sys.argv[1], encoding="utf-8").read(), sys.argv[1], "exec")' "$f" 2>/tmp/pf_err_$$; then
        echo "✓ PASS  [python语法] $f"; pass=$((pass+1))
      else
        echo "✗ FAIL  [python语法] $f"
        sed 's/^/       /' /tmp/pf_err_$$
        fail=$((fail+1))
      fi
      rm -f /tmp/pf_err_$$
      ;;
    js)
      if command -v node >/dev/null 2>&1; then
        if node --check "$f" 2>/tmp/pf_err_$$; then
          echo "✓ PASS  [js语法] $f"; pass=$((pass+1))
        else
          echo "✗ FAIL  [js语法] $f"
          sed 's/^/       /' /tmp/pf_err_$$
          fail=$((fail+1))
        fi
        rm -f /tmp/pf_err_$$
      else
        echo "- SKIP  [js语法] ${f}（本机无 node，跳过检查）"; skip=$((skip+1))
      fi
      ;;
    json)
      if python3 -m json.tool "$f" >/dev/null 2>/tmp/pf_err_$$; then
        echo "✓ PASS  [json] $f"; pass=$((pass+1))
      else
        echo "✗ FAIL  [json格式] $f"
        sed 's/^/       /' /tmp/pf_err_$$
        fail=$((fail+1))
      fi
      rm -f /tmp/pf_err_$$
      ;;
    cpp|cc|c|h|hpp|mk)
      echo "⚠ REMIND [C++/mk] $f —— 无独立语法检查器：该改动必须以重跑管线构建阶段验证（bash 04_scripts/pipeline.sh <游戏> run build_so 或对应阶段）"
      remind=$((remind+1)); cpp_remind=$((cpp_remind+1))
      # ★FW-22（2026-10-16）：Games/*Entry.cpp 的 usleep 字段（initDelayMs / initRetryIntervalMs /
      # tickIntervalMs）单位是**微秒**，按毫秒写小值会让延迟与 tick 静默失真（实测 32 个 entry 命中；
      # 终末地因此 tick 跑到 ~2300 次/秒、退避节拍失效）。判定阈值 <10000 ⇒ 视为误写。
      # 全量修正器：`python3 04_scripts/_common/fix_entry_units.py [--apply|--check]`。
      if [[ "$f" == *"/Games/"*"Entry.cpp" ]]; then
        _bad="$(grep -E '\.(initDelayMs|initRetryIntervalMs|tickIntervalMs)[[:space:]]*=[[:space:]]*[0-9]+' "$f" 2>/dev/null | awk -F= '{v=$2; gsub(/[^0-9]/,"",v); if (v+0>0 && v+0<10000) print}')"
        if [ -n "$_bad" ]; then
          echo "⚠ REMIND [FW-22] $f 的 usleep 字段可能按毫秒误写（单位应为微秒，见 Games/ModEntry.h）："
          printf '%s\n' "$_bad" | sed 's/^/       /' | head -4
          remind=$((remind+1))
        fi
      fi
      ;;
    md)
      # 入口体量关（2026-10-30）：只查 AGENTS.md —— 入口文件边界要求它只放通用规则与路由，
      # 单游戏/日志/渠道正文不属于这里。30 KB 是工作上限（预警），32 KB 是 D6 硬闸（与
      # twin_status.sh 同源同值），两者都在改完当场给出，不等到双机体检。
      case "$f" in
        *AGENTS.md|AGENTS.md)
          _esz=$(wc -c < "$f" | tr -d ' ')
          if [ "${_esz:-0}" -gt 32768 ]; then
            echo "✗ FAIL  [入口体量] ${f} ${_esz} B > 32768（D6 闸门）：按 §1「入口文件边界」把单游戏/渠道/日志正文迁出，只留指针"
            fail=$((fail+1))
          elif [ "${_esz:-0}" -gt 30720 ]; then
            echo "⚠ REMIND [入口体量] ${f} ${_esz} B > 30720（30 KB 工作上限；D6 硬闸 32768）：新增前先迁出等量旧正文（§1 入口增量纪律）"
            remind=$((remind+1))
          else
            echo "✓ PASS  [入口体量] ${f} ${_esz} B <= 30720（距 D6 硬闸还有 $((32768 - _esz)) B）"
            pass=$((pass+1))
          fi
          ;;
        *)
          echo "✓ PASS  [跳过] ${f}（md 只对入口文件查体量）"; pass=$((pass+1))
          ;;
      esac
      ;;
    *)
      echo "✓ PASS  [跳过] ${f}（类型无检查器）"; pass=$((pass+1))
      ;;
  esac
}

# ---------------------------------------------------------------
# 全仓 bash 3.2 扫描（无参模式 / --sweep）
#   判据 100% 复用 bash32_check.sh（单一真源，不在这里重写正则）
# ---------------------------------------------------------------
sweep_bash32() {
  local list="${PF_SWEEP_LIST:-20}"
  local tmp; tmp=$(mktemp -t pf_sweep) || { echo "✗ FAIL  [全仓bash32] mktemp 失败"; fail=$((fail+1)); return; }
  for f in 04_scripts/*/*.sh 04_scripts/_common/*.sh mod菜单/*/scripts/*.sh mod菜单/*/scripts/_lib/*.sh; do
    [ -f "$f" ] || continue
    printf '%s\0' "$f" >> "$tmp"
    sweep_files=$((sweep_files+1))
  done
  if [ "$sweep_files" -eq 0 ]; then
    echo "⚠ REMIND [全仓bash32] 扫描目标 0 个 —— 请在仓库根运行（04_scripts/**、mod菜单/**/scripts/** 都不存在？）"
    rm -f "$tmp"; remind=$((remind+1)); return
  fi
  echo "── 全仓 bash 3.2 门禁扫描：$sweep_files 个脚本（04_scripts/*/*.sh + _common/*.sh + mod菜单/*/scripts/*.sh + mod菜单/*/scripts/_lib/*.sh）──"
  local out
  out=$(xargs -0 bash "$SCRIPT_DIR/bash32_check.sh" < "$tmp" 2>&1)
  rm -f "$tmp"
  # 逐文件提取：bash32_check 的行格式是 `  [<文件>] ✗ …`；汇总行/缺文件行不含方括号前缀，先剔掉
  local hits
  hits=$(echo "$out" | grep '✗' | grep -v 'bash 3.2 门禁' | grep -v '缺文件')
  local bad
  bad=$(echo "$hits" | sed 's/^  \[\(.*\)\] .*/\1/' | sort -u | grep -c .)
  if [ "${bad:-0}" -gt 0 ]; then
    echo "✗ FAIL  [全仓bash32] $bad 个脚本仍有硬错误（共扫 $sweep_files 个）："
    echo "$hits" | head -n "$list" | sed 's/^/       /'
    if [ "$bad" -gt "$list" ]; then
      echo "       …（只列前 $list 个文件；要全量就用 PF_SWEEP_LIST=0 或直接 bash $SCRIPT_DIR/bash32_check.sh <文件…>）"
    fi
    echo "       修法（判据与门禁一致，只改 \$VAR → \${VAR}）："
    echo "         python3 04_scripts/_common/fix_bash32_var_brace.py <该文件…>            # 先看 diff"
    echo "         python3 04_scripts/_common/fix_bash32_var_brace.py --apply <该文件…>    # 写回"
    fail=$((fail+bad))
  else
    echo "✓ PASS  [全仓bash32] $sweep_files 个脚本，0 处硬错误"
    pass=$((pass+1))
  fi
}



# --- 参数解析（bash 3.2 安全）---
# --env：额外跑一遍环境依赖体检（env_deps.sh）。用于"动真格前先确认机器没问题"
#        —— 收官链在 Win 上曾被 apksigner/python3.11/stat 三个环境问题分三次打断。
# --sweep：全仓 bash 3.2 扫描（无参数时也走这条 —— 见文件头"无参 / --sweep"）
ENV_CHECK=0
SWEEP_CHECK=0
FILE_ARGS=0
for _a in "$@"; do
  case "$_a" in
    --env)      ENV_CHECK=1 ;;
    --sweep)    SWEEP_CHECK=1 ;;
    -h|--help)  echo "用法: bash 04_scripts/_common/preflight.sh [--sweep] [--env] <文件...>（不给文件 = 全仓 bash 3.2 扫描）"; exit 0 ;;
    --*)        echo "✗ FAIL  [参数] 未知选项 $_a"; exit 2 ;;
    *)          FILE_ARGS=$((FILE_ARGS+1)) ;;
  esac
done
# 无文件、也无 --env ⇒ 默认做全仓扫描（而不是打 usage 退出：2026-10-12 复盘 P8 收尾）
if [ "$ENV_CHECK" = "0" ] && [ "$FILE_ARGS" -lt 1 ]; then
  SWEEP_CHECK=1
fi

if [ "$SWEEP_CHECK" = "1" ]; then
  sweep_bash32
  [ "$FILE_ARGS" -ge 1 ] && echo
fi

for f in "$@"; do
  case "$f" in --*) continue ;; esac
  check_one "$f"
done

env_rc=0
if [ "$ENV_CHECK" = "1" ]; then
  echo
  if [ -f "$SCRIPT_DIR/env_deps.sh" ]; then
    bash "$SCRIPT_DIR/env_deps.sh" || env_rc=$?
  else
    echo "⚠ 未找到 $SCRIPT_DIR/env_deps.sh，跳过环境体检"
  fi
fi

echo "────────────────────────────────"
echo "汇总: PASS=$pass  FAIL=$fail  REMIND=$remind  SKIP=$skip"
if [ "$fail" -gt 0 ]; then
  echo "❌ 存在 FAIL：必须修复后再继续下一步（这就是你改完代码的机器关）。"
  exit 1
fi
if [ "$env_rc" != "0" ]; then
  echo "❌ 环境体检未通过（env_deps.sh rc=${env_rc}）：先修环境再继续。"
  exit 1
fi
if [ "$cpp_remind" -gt 0 ]; then
  echo "⚠ 有 C++/mk 改动：进入下一步前必须重跑对应管线构建阶段。"
fi
if [ "$remind" -gt "$cpp_remind" ]; then
  echo "⚠ 存在 REMIND 提示（见上，非 C++/mk）：确认无风险后再继续。"
fi
echo "✅ 机器关通过。"
exit 0
