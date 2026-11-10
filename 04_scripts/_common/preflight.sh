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
#    .md（AGENTS.md）→ 入口体量：>30720 B REMIND（30 KB 工作上限）/ >32768 B FAIL（D6 闸门）
#    .md（_state/current.md）→ §6 状态索引：>15360 B REMIND（15 KB 上限）/ >24576 B FAIL（应急闸）
#            ＋单行 >1200 B（≈400 中文字符）REMIND。2026-11-09 起纳入机检。
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

# 解释器探测：沿用 prune_output_final.sh 的既有顺序，mac 走 brew python3.11 / 系统 python3，
# Win 走 win_shims 的 python3.11 垫片或 py。两者都没有时返回 1，调用方降级为 WARN 而非 FAIL。
idx_python() {
  local c
  for c in python3.11 python3 python py; do
    if command -v "$c" >/dev/null 2>&1; then echo "$c"; return 0; fi
  done
  return 1
}

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
      # 索引一致性关（2026-11-09）：current.md/archive_index.md 是 games/*.md frontmatter 的**投影**，
      # 改任一侧都要求同步重生成，否则索引与实际状态漂移（旧的「手工加一行」模式已废除）。
      case "$f" in
        *10_docs/_state/games/*.md|*_state/current.md|*_state/archive_index.md)
          # --check 一次扫全部 games/*.md，结果与文件名无关；本进程内只跑一次（否则 101 个游戏
          # 文件就是 101 × 138 ms ≈ 14 s）。
          if [ "${_IDX_CHECKED:-0}" = "1" ]; then
            :
          elif [ -f "$SCRIPT_DIR/gen_state_index.py" ] && _idx_py=$(idx_python); then
            _IDX_CHECKED=1
            # PYTHONUTF8=1：Win 端默认 cp936，print 中文会炸（与 stage_shared_file.py 同一处理）
            if _idx_out=$(PYTHONUTF8=1 "$_idx_py" "$SCRIPT_DIR/gen_state_index.py" --check 2>&1); then
              echo "✓ PASS  [状态索引] $f 索引与真源一致"
              # 生成器 --check 通过时仍会吐「⚠ 挂起 / 超限」提示：这些行不在 current 里但不是错误，
              # 若只 echo PASS 就被吞掉了（工作集不全没人知道）。透传前 2 行并计一次 REMIND。
              _idx_notes=$(printf '%s\n' "$_idx_out" | grep '^⚠' | head -2)
              if [ -n "$_idx_notes" ]; then
                printf '%s\n' "$_idx_notes" | sed 's/^/       /'
                remind=$((remind+1))
              fi
              pass=$((pass+1))
            else
              echo "✗ FAIL  [状态索引] $f 与索引不一致 ——"
              printf '%s\n' "$_idx_out" | sed 's/^/       /' | head -3
              fail=$((fail+1))
            fi
          else
            # 没有解释器时不能静默跳过：否则 Win 端这一关悄悄失效，谁都发现不了。
            echo "⚠ REMIND [状态索引] 本机无可用 python（python3.11/python3/py 全缺），索引一致性关未生效"
            echo "        Win 端请自行确认 win_shims 在 PATH 上（本关不代写 Windows 适配）"
            remind=$((remind+1))
            _IDX_CHECKED=1
          fi
          ;;
      esac
      # 入口体量关（2026-10-30）：只查 AGENTS.md —— 入口文件边界要求它只放通用规则与路由，
      # 单游戏/日志/渠道正文不属于这里。30 KB 是工作上限（预警），32 KB 是 D6 硬闸（与
      # twin_status.sh 同源同值），两者都在改完当场给出，不等到双机体检。
      case "$f" in
        *_state/current.md)
          # §6「状态索引」（2026-11-09）：此前只有 AGENTS.md 有体量关，current.md 从 18 KB 一路涨到
          # 21 KB 全程无人拦（preflight 全绿）。现按 §6 原文补机检：整文件 <15 KB、单行 ≈≤400 字符。
          # 超出时先做去重 / 把 hash·包大小·挂点下沉到 games/{游戏}.md，再考虑改结构（分片或生成式索引）。
          _csz=$(wc -c < "$f" | tr -d ' ')
          _clong=$(LC_ALL=C awk 'length($0) > 1200 { printf "L%d(%dB) ", NR, length($0) }' "$f")
          if [ "${_csz:-0}" -gt 24576 ]; then
            echo "✗ FAIL  [状态索引] ${f} ${_csz} B > 24576（应急闸）：先把不活跃游戏行迁 _state/history/ 或改结构再提交"
            fail=$((fail+1))
          elif [ "${_csz:-0}" -gt 15360 ]; then
            echo "⚠ REMIND [状态索引] ${f} ${_csz} B > 15360（§6 上限）：新增前先迁出等量旧行（去重、hash/包大小下沉 games/）"
            remind=$((remind+1))
          else
            echo "✓ PASS  [状态索引] ${f} ${_csz} B <= 15360（距上限还有 $((15360 - _csz)) B）"
            pass=$((pass+1))
          fi
          if [ -n "$_clong" ]; then
            echo "⚠ REMIND [状态索引] 单行超 1200 B（≈400 中文字符）：${_clong}"
            remind=$((remind+1))
          fi
          ;;
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
ENV_QUICK_OFF=0   # --no-env-quick：临时跳过自动工具链体检（本机没装也不耽误改文档）
SWEEP_CHECK=0
FILE_ARGS=0
for _a in "$@"; do
  case "$_a" in
    --env)      ENV_CHECK=1 ;;
    --no-env-quick) ENV_QUICK_OFF=1 ;;
    --sweep)    SWEEP_CHECK=1 ;;
    -h|--help)  echo "用法: bash 04_scripts/_common/preflight.sh [--sweep] [--env] [--no-env-quick] <文件...>（不给文件 = 全仓 bash 3.2 扫描）"; exit 0 ;;
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

# ---------------------------------------------------------------
# 自动闸门 ①：分析工具链体检（2026-11-04）
#   每次都跑，不缓存：--quick 实测 1.3 s，换来"永远是最新状态"——
#   刚装完 apktool 下一次 preflight 就能看见，不用等、不用记着加 --env。
#   输出压成一行：全齐只打「✓ PASS [分析工具链] N 项就绪」，有缺才展开明细。
#   要临时关：--no-env-quick
# ---------------------------------------------------------------
if [ "$FILE_ARGS" -ge 1 ] && [ "$ENV_CHECK" = "0" ] && [ "$ENV_QUICK_OFF" = "0" ] && [ -f "$SCRIPT_DIR/env_deps.sh" ]; then
  _qout=$(bash "$SCRIPT_DIR/env_deps.sh" --quick 2>&1)
  _qok=$(printf '%s\n' "$_qout" | grep -c '✓' || true)
  _qwarn=$(printf '%s\n' "$_qout" | grep -c '⚠' || true)
  _qbad=$(printf '%s\n' "$_qout" | grep -c '✗' || true)
  if [ "${_qbad:-0}" -gt 0 ] || [ "${_qwarn:-0}" -gt 0 ]; then
    echo
    echo "── 分析工具链：${_qok:-0} 就绪 / ${_qwarn:-0} 按需缺 / ${_qbad:-0} 必需缺 ──"
    printf '%s\n' "$_qout" | sed -n '/\[6\/7\]/,/\[7\/7\]/p' | sed '1d;$d'
    if [ "${_qbad:-0}" -gt 0 ]; then
      echo "⚠ REMIND [分析工具链] ${_qbad} 项必需工具缺失（apktool / jadx，拆包会直接断）。"
      remind=$((remind+1))
    else
      echo "· 按需项缺失只在用到时才补（IDA / IL2CPP / ndk 等）。"
    fi
  else
    echo "✓ PASS  [分析工具链] ${_qok:-0} 项就绪"
    pass=$((pass+1))
  fi
fi

# ---------------------------------------------------------------
# 自动闸门 ②：验收卡空转检查（2026-11-04）
#   只在本轮改动里出现 *验收卡*.md 时触发 —— 改了卡却没填 §3「已实证」等于卡开了一半。
#   REMIND 级（卡是活文档，进行中本就可能空），但每轮都会冒出来，不会像以前那样"开了没人看见"。
# ---------------------------------------------------------------
for f in "$@"; do
  case "$f" in *验收卡*.md) ;; *) continue ;; esac
  [ -f "$f" ] || continue
  _card_rows=$(awk '
    /^##[[:space:]]*3\./ { in3=1; next }
    /^##[[:space:]]*4\./ { in3=0 }
    in3 && /^\|/ {
      if ($0 ~ /^\|[[:space:]]*-/) next
      if ($0 ~ /结论[[:space:]]*\|/) next
      if ($0 ~ /示例/) next
      if ($0 ~ /待填/) next
      n++
    }
    END { print n+0 }
  ' "$f" 2>/dev/null) || _card_rows=0
  if [ "${_card_rows:-0}" -eq 0 ]; then
    echo "⚠ REMIND [验收卡] ${f} 的 §3「已实证」为空 —— 收尾前补证据，或写进 §4 NOT PROVEN 说明缺什么。"
    remind=$((remind+1))
  else
    echo "✓ PASS  [验收卡] ${f}（§3 已实证 ${_card_rows} 行）"
    pass=$((pass+1))
  fi
done

# ---------------------------------------------------------------
# 自动闸门 ③：Git 冲突标记残留（2026-11-10）
#   事故：`10_docs/_state/current.md`（**每次开局注入**的状态索引）带着
#   `<<<<<<< HEAD` / `=======` / `>>>>>>> <hash>` 三行被 commit 进 main，
#   而 preflight 全绿 —— 因为当时只有 frontmatter 关，没有标记扫描关。
#   后果是每开会话 AI 读到的第一份全局状态就夹着垃圾。
#   只扫 git 跟踪文件（未跟踪的临时产物不是我们的输出）；有参数时只扫本次改动文件。
# ---------------------------------------------------------------
if [ -f "$SCRIPT_DIR/check_merge_markers.sh" ]; then
  if [ "$FILE_ARGS" -ge 1 ]; then
    _mm_args=()
    for f in "$@"; do case "$f" in --*) continue;; esac; [ -f "$f" ] && _mm_args+=("$f"); done
    if [ "${#_mm_args[@]}" -gt 0 ]; then
      _mm_out=$(bash "$SCRIPT_DIR/check_merge_markers.sh" "${_mm_args[@]}" 2>&1); _mm_rc=$?
    else
      _mm_out=$(bash "$SCRIPT_DIR/check_merge_markers.sh" 2>&1); _mm_rc=$?
    fi
  else
    _mm_out=$(bash "$SCRIPT_DIR/check_merge_markers.sh" 2>&1); _mm_rc=$?
  fi
  if [ "$_mm_rc" != "0" ]; then
    printf '%s\n' "$_mm_out" | sed 's/^/       /'
    fail=$((fail+1))
  fi
fi

# ---------------------------------------------------------------
# 自动闸门 ④：游戏文档索引新鲜度（2026-11-10）
#   每个 `10_docs/{游戏}/` 有生成物 `_index.md`，列出本目录文件并标出
#   【当前】/【历史】。它解决「50 个文件平铺、AI 不知道哪 5 个是当前的」。
#   只在「改动了游戏目录文件」或「改动了 _index.md 本身」时触发 ——
#   否则每轮都跑一遍全库比对纯属浪费（全库 114 个目录约 0.5 s，触发时也不贵，
#   但无谓地把无关轮次的输出撑长）。
# ---------------------------------------------------------------
if [ -f "$SCRIPT_DIR/gen_game_doc_index.py" ] && [ "$FILE_ARGS" -ge 1 ]; then
  _gi_need=0
  for f in "$@"; do
    case "$f" in --*) continue;; esac
    case "$f" in
      */10_docs/_state/*|*/10_docs/_templates/*|*/10_docs/tools/*) ;;
      */10_docs/*) _gi_need=1 ;;
      *_index.md) _gi_need=1 ;;
    esac
  done
  if [ "$_gi_need" = "1" ]; then
    _gi_py=$(idx_python) || _gi_py=""
    if [ -n "$_gi_py" ]; then
      if _gi_out=$(PYTHONUTF8=1 "$_gi_py" "$SCRIPT_DIR/gen_game_doc_index.py" --check 2>&1); then
        echo "✓ PASS  [游戏文档索引] _index.md 与目录内容一致"
        pass=$((pass+1))
      else
        echo "✗ FAIL  [游戏文档索引] 有目录的 _index.md 缺失或过期 ——"
        printf '%s\n' "$_gi_out" | sed 's/^/       /' | head -6
        fail=$((fail+1))
      fi
    fi
  fi

# ---------------------------------------------------------------
# 自动闸门 ⑤：公共脚本索引新鲜度（2026-11-10）
#   `04_scripts/_common/scripts_index.tsv` 列出 105 个公共脚本的一句话用途与是否孤儿。
#   它治的是「新会话不知道有这个脚本 ⇒ 重造」——实测 47 个脚本此前没有任何规则文档
#   提到过（AGENTS §3.3 明令禁止重造）。只在改动 `04_scripts/_common/**` 时触发。
# ---------------------------------------------------------------
if [ -f "$SCRIPT_DIR/gen_scripts_index.py" ] && [ "$FILE_ARGS" -ge 1 ]; then
  _si_need=0
  for f in "$@"; do
    case "$f" in --*) continue;; esac
    case "$f" in
      */04_scripts/_common/*|*_common/*) _si_need=1 ;;
    esac
  done
  if [ "$_si_need" = "1" ]; then
    _si_py=$(idx_python) || _si_py=""
    if [ -n "$_si_py" ]; then
      if _si_out=$(PYTHONUTF8=1 "$_si_py" "$SCRIPT_DIR/gen_scripts_index.py" --check 2>&1); then
        echo "✓ PASS  [脚本索引] scripts_index.tsv 与 _common/ 一致"
        pass=$((pass+1))
      else
        echo "✗ FAIL  [脚本索引] scripts_index.tsv 已过期 ——"
        printf '%s\n' "$_si_out" | sed 's/^/       /' | head -4
        fail=$((fail+1))
      fi
    fi
  fi
fi
fi

env_rc=0
# ---- 框架隔离门禁（2026-11-11 起，每次 preflight 都跑）----
#   判据唯一正文：mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/core/GameHook.java
#   为什么放在这里而不是"改到才跑"：框架泄漏是**慢性**的 —— 某款游戏改一行共用类，
#   一年后没人记得。全量跑的成本只有一次 find+grep，值得。
if [ -f "$SCRIPT_DIR/check_framework_isolation.sh" ]; then
  echo
  echo "── 框架隔离门禁 ──"
  iso_out=$(bash "$SCRIPT_DIR/check_framework_isolation.sh" 2>&1); iso_rc=$?
  if [ "$iso_rc" -eq 0 ]; then
    echo "✓ PASS  [框架隔离] 框架层无单游戏逻辑"
    pass=$((pass+1))
  else
    printf '%s\n' "$iso_out" | sed 's/^/       /'
    fail=$((fail+1))
  fi
fi

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
