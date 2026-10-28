#!/usr/bin/env bash
# check_deleted_class_injection.sh — 已删除类"注入/硬依赖"巡检（2026-10-28 建）
#
# 背景：用户 2026-10-28 指令把运行时签名伪装整体删除
#   （sign/SignSpoof.java、sign/SignSpoofAppComponentFactory.java 已从源码移除）。
#   删类之后，任何仍然
#     ① 注入 `invoke-static … SignSpoof;->install()V`，或
#     ② 组装 dex 时硬要求 `sign/SignSpoof.smali` 存在 / 断言其含包名·证书
#   的脚本，重跑都会产出「引用不存在类」的包（smali 汇编不校验类型存在性 ⇒ 不报错，
#   装机后才 NoClassDefFoundError 或功能静默失效）。
#
# 判级：
#   · 04_scripts/**（现行管线）出现任一命中 ⇒ FAIL（rc=1）：现行管线不得再依赖已删类。
#   · mod菜单/starry-mod-menu/scripts/**（历史一次性注入脚本）⇒ WARN + 清单：保留作历史，
#     但禁止重跑；重跑前须先按改包法重写。
#
# 用法：bash 04_scripts/_common/check_deleted_class_injection.sh [--root <仓库根>]
# 退出码：0=通过（可能含 WARN）；1=现行管线存在依赖；2=用法错误
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --root) ROOT="$2"; shift 2 ;;
    -h|--help) sed -n '1,25p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

DELETED_CLASSES=(
  "SignSpoof"
  "SignSpoofAppComponentFactory"
)

fail=0
warn=0

hits_in() {  # $1=目录  ⇒ 打印命中行
  local dir="$1" cls pat
  [[ -d "$dir" ]] || return 0
  for cls in "${DELETED_CLASSES[@]}"; do
    # 注入站点：invoke-static … Class;->install()V 视为"会生成调用"，
    # 纯注释/兜底断言（行首带 # 或位于 python 字符串里的 needle 变量）不在此列判定为注入，
    # 由下方的"硬依赖"模式单独统计。
    grep -rn --include=*.sh --include=*.py -E "invoke-static.*${cls}->install" "$dir" 2>/dev/null \
      | sed 's/^/    [注入] /'
  done
}

deps_in() {  # $1=目录 ⇒ 打印"硬依赖已删 smali"的行（排除注释行）
  local dir="$1" cls
  [[ -d "$dir" ]] || return 0
  for cls in "${DELETED_CLASSES[@]}"; do
    grep -rn --include=*.sh --include=*.py -F "sign/${cls}.smali" "$dir" 2>/dev/null \
      | grep -vE ':[0-9]+:[[:space:]]*#' \
      | sed 's/^/    [硬依赖] /'
  done
}

echo "== 已删类注入/硬依赖巡检（SignSpoof 已于 2026-10-28 删除）=="

echo "-- 1/2 现行管线 04_scripts（命中即 FAIL）"
w_out="$(deps_in "$ROOT/04_scripts")"
i_out="$(hits_in "$ROOT/04_scripts")"
if [[ -n "$w_out" || -n "$i_out" ]]; then
  [[ -n "$i_out" ]] && printf '%s\n' "$i_out"
  [[ -n "$w_out" ]] && printf '%s\n' "$w_out"
  echo "  ✗ 现行管线仍有脚本依赖/注入已删除的类 ⇒ 必须改成 StarryAppComponentFactory 或删除该断言"
  fail=1
else
  echo "  ✓ 04_scripts 无对已删类的注入或硬依赖"
fi

echo "-- 2/2 历史注入脚本 mod菜单/starry-mod-menu/scripts（WARN，禁止重跑）"
hist="$( { hits_in "$ROOT/mod菜单/starry-mod-menu/scripts"; deps_in "$ROOT/mod菜单/starry-mod-menu/scripts"; } | sort -u )"
if [[ -n "$hist" ]]; then
  printf '%s\n' "$hist" | sed 's|'"$ROOT"'/||' | head -40
  n_hit="$(printf '%s\n' "$hist" | wc -l | tr -d ' ')"
  echo "  ⚠ ${n_hit} 处历史注入脚本仍写入已删除的类：这些脚本是历史轮次的一次性脚本，"
  echo "     **禁止重跑**（会生成引用不存在类的 dex）。需要重做该版本时，按改包法重写。"
  warn=$((warn + 1))
else
  echo "  ✓ 无历史注入脚本残留"
fi

echo
if [[ "$fail" != 0 ]]; then
  echo "== 结论：✗ FAIL（现行管线依赖已删类）=="
  exit 1
fi
echo "== 结论：✓ PASS（WARN ${warn} 项）=="
exit 0
