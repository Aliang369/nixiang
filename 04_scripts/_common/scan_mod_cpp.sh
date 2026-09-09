#!/usr/bin/env bash
# 扫描 Games/*Mod.cpp：ast-grep 规则 + 文本级硬禁项
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SG_CONFIG="$ROOT/11_proj_tools/ast-grep/sgconfig.yml"
TARGET="$ROOT/mod菜单/starry-mod-menu/app/src/main/jni/Games"

fail=0

if ! command -v ast-grep >/dev/null 2>&1; then
  echo "[scan_mod_cpp] ast-grep 未安装。macOS: brew install ast-grep" >&2
  exit 2
fi

if [[ ! -d "$TARGET" ]]; then
  echo "[scan_mod_cpp] 目录不存在: $TARGET" >&2
  exit 2
fi

echo "[scan_mod_cpp] config=$SG_CONFIG"
echo "[scan_mod_cpp] target=$TARGET"
echo

# --- ast-grep：warning/info 仅报告；error 级规则命中则 fail ---
if ! ast-grep scan --config "$SG_CONFIG" "$TARGET"; then
  fail=1
fi

echo
echo "[scan_mod_cpp] --- 文本硬禁项 ---"

# Hzsj 反面教材：禁止其它 Mod 引用/复制（HzsjMod.cpp 本身保留作教材）
hzsj_hits=$(grep -rn 'HzsjMod\|from Hzsj\|复制.*Hzsj' "$TARGET" --include='*Mod.cpp' 2>/dev/null \
  | grep -v '/HzsjMod.cpp:' || true)
if [[ -n "$hzsj_hits" ]]; then
  echo "$hzsj_hits"
  echo "[scan_mod_cpp] ERROR: 非 HzsjMod 文件引用了 Hzsj 反面教材" >&2
  fail=1
else
  echo "[scan_mod_cpp] OK: 无跨文件 Hzsj 复制引用"
fi

# 共享 invoke 入口 hook（注释行除外）
invoke_hits=$(grep -rn 'DobbyHook' "$TARGET" --include='*Mod.cpp' 2>/dev/null \
  | grep -E 'il2cpp_runtime_invoke|ScriptingInvocation|HybridCLR|Interpreter.*Execute' \
  | grep -v '^[^:]*:[^:]*:.*//' || true)
if [[ -n "$invoke_hits" ]]; then
  echo "$invoke_hits"
  echo "[scan_mod_cpp] ERROR: 疑似 hook 共享 invoke 入口（只 hook 具体方法）" >&2
  fail=1
else
  echo "[scan_mod_cpp] OK: 无共享 invoke DobbyHook"
fi

echo
if [[ $fail -eq 0 ]]; then
  echo "[scan_mod_cpp] PASS — error 级无命中（warning/info 请人工确认）"
else
  echo "[scan_mod_cpp] FAIL — 修复 error 后再装机" >&2
fi
exit "$fail"
