#!/usr/bin/env bash
# 一念通天：768 最终 Git 固化。只提交本作相关路径，不触碰其他会话/游戏改动。
#
# 注意：10_docs/_state/current.md 是多会话共享索引，本脚本不整文件 add，
# 只接受「已按行暂存、且暂存改动全部属于本作」的状态；否则中止。
# 历史事故（2026-09-30）：路径级 add 曾把别会话的索引行与 TapLoginForceWeb.java 一起提交。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$NIXIANG_ROOT"

git config core.quotepath false

[[ -f "04_scripts/一念通天/cleanup_summary.json" ]] || { echo "错误: 缺少 cleanup_summary.json，先跑清理阶段" >&2; exit 1; }
[[ -f "10_docs/一念通天/收尾与最小复现清单_768_20261030.md" ]] || { echo "错误: 缺少 768 收尾清单" >&2; exit 1; }

git add -- \
  "04_scripts/一念通天" \
  "10_docs/一念通天" \
  "10_docs/_state/games/一念通天.md" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android_yntt.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttMod.cpp" \
  "mod菜单/starry-mod-menu/scripts/surgical_yntt_768.sh"

git add -A -- "06_dumped/一念通天"
if git diff --cached --name-only -- "06_dumped/一念通天" | grep -E '\.(apk|ap_|so|o|jar|bin|dat|dex|i64|idb)$' >/dev/null; then
  echo "错误: 暂存区出现 06_dumped 大二进制，拒绝提交" >&2
  exit 1
fi

# 共享索引 current.md：一次只提交本作那一行；若暂存改动出现非本作行 → 中止。
# 注意（2026-10-12 踩坑）：grep 无匹配返回 1，配合 set -o pipefail 会让赋值语句
# 直接失败且无任何输出（表现为 rc=1 静默死）。故此处必须 `|| true`。
CUR_DIFF="$(git diff --cached -- "10_docs/_state/current.md" | grep -E '^[+-]' | grep -vE '^(\+\+\+|---)' || true)"
if [[ -n "$CUR_DIFF" ]] && printf '%s\n' "$CUR_DIFF" | grep -qv '一念通天'; then
  echo "错误: current.md 暂存区含非本作行，拒绝提交。请先 git reset 后按行暂存：" >&2
  printf '%s\n' "$CUR_DIFF" >&2
  exit 1
fi

STAGED="$(git diff --cached --name-only)"
if [[ -z "$STAGED" ]]; then
  echo "没有待提交的本作改动"
  exit 0
fi

while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  case "$path" in
    04_scripts/一念通天/*|10_docs/一念通天/*|10_docs/_state/games/一念通天.md|10_docs/_state/current.md|\
    mod菜单/starry-mod-menu/app/src/main/jni/Android_yntt.mk|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttMod.cpp|\
    mod菜单/starry-mod-menu/scripts/surgical_yntt_768.sh|\
    06_dumped/一念通天/*)
      ;;
    *)
      echo "错误: 暂存区出现非本作路径，拒绝提交: $path" >&2
      exit 1
      ;;
  esac
done <<< "$STAGED"

printf '%s\n' "$STAGED"
git commit -m "一念通天 768 星空包交付与收官"
# push 一律经 push_main（AGENTS §5：禁裸 git push；四通道自愈）
# shellcheck source=/dev/null
source "$(git rev-parse --show-toplevel)/04_scripts/_common/git_push_main.sh"
push_main main

echo "==> 本作相关状态"
git status --short -- 04_scripts/一念通天 06_dumped/一念通天 10_docs/一念通天 10_docs/_state/games/一念通天.md 10_docs/_state/current.md mod菜单/starry-mod-menu
echo "==> 全仓未处理项（应仅见其他会话/游戏）"
git status --short
