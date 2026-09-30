#!/usr/bin/env bash
# ================================================================
#  preflight.sh — 机器检查关（弱模型改完任何文件后必跑）
#
#  用法:
#    bash 04_scripts/_common/preflight.sh <文件1> [文件2 ...]
#
#  按扩展名自动路由检查器：
#    .sh   → bash -n + bash 3.2 兼容门禁（macOS 自带 bash 就是 3.2；见下 "bash 3.2 门禁"）
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
#    其他  → PASS（跳过）
#
#  退出码: 0=全部通过/SKIP/REMIND；1=存在 FAIL（必须修复后再继续）
#  铁律: 这是"改完文件"的机器关——多一个符号、少个括号在这里抓，
#        不要等到装机/进游戏才发现返工。
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

fail=0; pass=0; skip=0; remind=0

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
      remind=$((remind+1))
      ;;
    *)
      echo "✓ PASS  [跳过] ${f}（类型无检查器）"; pass=$((pass+1))
      ;;
  esac
}

[ $# -ge 1 ] || { echo "用法: bash 04_scripts/_common/preflight.sh <文件...>（改过什么文件就传什么）"; exit 1; }

for f in "$@"; do
  check_one "$f"
done

echo "────────────────────────────────"
echo "汇总: PASS=$pass  FAIL=$fail  REMIND=$remind  SKIP=$skip"
if [ "$fail" -gt 0 ]; then
  echo "❌ 存在 FAIL：必须修复后再继续下一步（这就是你改完代码的机器关）。"
  exit 1
fi
if [ "$remind" -gt 0 ]; then
  echo "⚠ 有 C++/mk 改动：进入下一步前必须重跑对应管线构建阶段。"
fi
echo "✅ 机器关通过。"
exit 0
