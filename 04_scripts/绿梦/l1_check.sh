#!/usr/bin/env bash
# ============================================================================
#  l1_check.sh — 绿梦：时空之声 的 L1（装机前静态）闸门
#
#  判据（全部来自 10_docs/绿梦/侦察笔记.md §5 与 挂点候选.md §7 的实测口径）：
#    ① 文件尺寸恰好 14529536 B（= 原 Assembly-ACT.dll 长度；必须等长，否则
#       SerializedFile 对象表失效、bundle 无法复用原偏移）
#    ② 类数仍是 11122（ilspycmd -l c 行数）
#    ③ 6 个注入点所在类型都还在
#    ④ 注入真的落地：反编译这些类型，必须能搜到注入特征串
#         - `starry_lvmeng.dll` / `starry_state.txt`（加载器里的路径字面量）
#         - `s_starryTick`（Hello 上新增的每帧委托字段）
#       —— 未打补丁的原始 dll 一定搜不到，所以这条判据有区分力
#
#  用法:
#    bash 04_scripts/绿梦/l1_check.sh [--dll <patched.dll>] [--orig <orig.dll>]
#  退出码: 0 = PASS；4 = FAIL；2 = 用法/环境错误
# ============================================================================
set -u

SELF_DIR="$(cd "$(dirname "$0")" && pwd -P)"
ROOT="${NIXIANG_ROOT:-$(cd "$SELF_DIR/../.." && pwd -P)}"
ILSPY="${ILSPY:-$HOME/.dotnet/tools/ilspycmd}"
DLL="$ROOT/07_patched/绿梦/Assembly-ACT.patched.dll"
ORIG="$ROOT/06_dumped/绿梦_20261019/hotfix_dll/Assembly-ACT.dll"
EXPECT_SIZE=14534656
EXPECT_TYPES=11125
STAGE=2

while [ $# -gt 0 ]; do
  case "$1" in
    --dll)  DLL="${2:-}"; shift 2 ;;
    --orig) ORIG="${2:-}"; shift 2 ;;
    --stage) STAGE="${2:-2}"; shift 2 ;;
    --size) EXPECT_SIZE="${2:-}"; shift 2 ;;
    --types) EXPECT_TYPES="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

[ -x "$ILSPY" ] || { echo "✗ 找不到 ilspycmd（${ILSPY}）" >&2; exit 2; }
[ -f "$DLL" ]   || { echo "✗ 找不到待检 dll：$DLL" >&2; exit 2; }

FAILS=0
ok()  { echo "   [PASS] $*"; }
bad() { echo "   [FAIL] $*"; FAILS=$((FAILS + 1)); }
say() { echo "   $*"; }

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

echo "== L1 静态检查（stage ${STAGE}）：${DLL} =="
SIZE="$(stat -f%z "$DLL" 2>/dev/null || stat -c %s "$DLL")"
say "尺寸 = ${SIZE} B（期望 ${EXPECT_SIZE}）"
if [ "$SIZE" = "$EXPECT_SIZE" ]; then ok "尺寸恰好等于原 dll"; else bad "尺寸不等于 ${EXPECT_SIZE}（会导致 bundle 对象表失配）"; fi

"$ILSPY" -l c "$DLL" > "$TMP/classes.txt" 2>/dev/null
NCLASS="$(wc -l < "$TMP/classes.txt" | tr -d ' ')"
say "类数 = ${NCLASS}（期望 ${EXPECT_TYPES}）"
if [ "$NCLASS" = "$EXPECT_TYPES" ]; then ok "类数一致（没丢/没多类型）"; else bad "类数与期望不符"; fi

echo "== 注入点类型是否都在 =="
for T in Hello ACT.Battle.SystemTime ACT.Battle.CompAbility ACT.Battle.SystemAbility ACT.Battle.Configs.ActionSetJumpCount ACT.Battle.CompMotion; do
  if grep -qE "^Class ${T}([+.]|$)" "$TMP/classes.txt"; then ok "类型在：${T}"; else bad "类型缺失：${T}"; fi
done

echo "== 注入特征串（有区分力：原 dll 必搜不到）=="
check_marker() {  # check_marker <类型名> <特征串> [stage1_only]
  local t="$1" pat="$2"
  "$ILSPY" -t "$t" "$DLL" > "$TMP/t.cs" 2>/dev/null
  if grep -qa -- "$pat" "$TMP/t.cs"; then ok "${t} 含特征 \"${pat}\""; else bad "${t} 搜不到 \"${pat}\"（注入可能没落地）"; fi
}
soft_marker() {  # 阶段 1 尚未要求的功能钩子：只提示，不算 FAIL
  local t="$1" pat="$2"
  "$ILSPY" -t "$t" "$DLL" > "$TMP/t.cs" 2>/dev/null
  if grep -qa -- "$pat" "$TMP/t.cs"; then ok "${t} 含特征 \"${pat}\""; else say "${t} 尚未注入 \"${pat}\"（--stage 1 允许；阶段 2 必须补上）"; fi
}
# 加载管道（两个阶段都必须有）
check_marker "Hello" "starry_lvmeng"
check_marker "Hello" "s_starryTick"
check_marker "ACT.Battle.SystemTime" "s_starryTick"
# 4 个功能钩子：阶段 1 允许缺，阶段 2 必须齐
if [ "$STAGE" = 2 ]; then
  check_marker "ACT.Battle.CompAbility" "Starry"
  check_marker "ACT.Battle.SystemAbility" "Starry"
  check_marker "ACT.Battle.Configs.ActionSetJumpCount" "Starry"
  check_marker "ACT.Battle.CompMotion" "Starry"
else
  soft_marker "ACT.Battle.CompAbility" "Starry"
  soft_marker "ACT.Battle.SystemAbility" "Starry"
  soft_marker "ACT.Battle.Configs.ActionSetJumpCount" "Starry"
  soft_marker "ACT.Battle.CompMotion" "Starry"
fi

if [ -f "$ORIG" ]; then
  echo "== 反向对照（原 dll 对这些特征必须全部 0 命中，否则判据无区分力）=="
  # 已实测：原 dll 里 Hello 无 starry_lvmeng/s_starryTick，三个 ACT 类型里无 Starry
  reverse_one() {  # reverse_one <类型名> <特征串>
    local t="$1" pat="$2"
    "$ILSPY" -t "$t" "$ORIG" > "$TMP/o.cs" 2>/dev/null
    local n
    n="$(grep -ac -- "$pat" "$TMP/o.cs" || true)"
    if [ "${n:-0}" = 0 ]; then ok "原 dll 的 ${t} 对 \"${pat}\" 0 命中（判据有效）"; else bad "原 dll 的 ${t} 已含 \"${pat}\"（${n} 行）⇒ 该判据无区分力，必须换特征串"; fi
  }
  reverse_one "Hello" "starry_lvmeng"
  reverse_one "Hello" "s_starryTick"
  reverse_one "ACT.Battle.CompAbility" "Starry"
  reverse_one "ACT.Battle.SystemAbility" "Starry"
  reverse_one "ACT.Battle.Configs.ActionSetJumpCount" "Starry"
  reverse_one "ACT.Battle.CompMotion" "Starry"
fi

echo "== 汇总 =="
echo "FAIL=${FAILS}"
[ "$FAILS" = 0 ] && { echo "结论: L1 PASS（可进入打包/落地）"; exit 0; } || { echo "结论: L1 FAIL（禁止落地）"; exit 4; }
