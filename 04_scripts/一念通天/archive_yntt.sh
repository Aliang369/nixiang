#!/usr/bin/env bash
# 一念通天：最终 Git 固化。只提交本作相关路径，不触碰其他会话/游戏改动。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$NIXIANG_ROOT"

git config core.quotepath false

[[ -f "04_scripts/一念通天/cleanup_summary.json" ]] || { echo "错误: 缺少 cleanup_summary.json，先跑清理阶段" >&2; exit 1; }
[[ -f "10_docs/一念通天/收尾与最小复现清单_728_20260919.md" ]] || { echo "错误: 缺少 728 收尾清单" >&2; exit 1; }

git add -- \
  "04_scripts/一念通天" \
  "10_docs/一念通天" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android_yntt.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttEntry.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttMod.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttMod.h" \
  "mod菜单/starry-mod-menu/scripts/surgical_yntt_720.sh" \
  "mod菜单/starry-mod-menu/scripts/surgical_yntt_728.sh" \
  "mod菜单/starry-mod-menu/yntt_smali"

git add -A -- "06_dumped/一念通天"
if git diff --cached --name-only -- "06_dumped/一念通天" | grep -E '\.(apk|ap_|so|o|jar|bin|dat|dex|i64|idb)$' >/dev/null; then
  echo "错误: 暂存区出现 06_dumped 大二进制，拒绝提交" >&2
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
    04_scripts/一念通天/*|10_docs/一念通天/*|10_docs/_state/current.md|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java|\
    mod菜单/starry-mod-menu/app/src/main/jni/Android_yntt.mk|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttEntry.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttMod.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/YnttMod.h|\
    mod菜单/starry-mod-menu/scripts/surgical_yntt_720.sh|\
    mod菜单/starry-mod-menu/scripts/surgical_yntt_728.sh|\
    mod菜单/starry-mod-menu/yntt_smali/*|06_dumped/一念通天/*)
      ;;
    *)
      echo "错误: 暂存区出现非本作路径，拒绝提交: $path" >&2
      exit 1
      ;;
  esac
done <<< "$STAGED"

printf '%s\n' "$STAGED"
git commit -m "一念通天 728 星空包交付与收官"
git push git@github.com:Aliang369/nixiang.git main

echo "==> 本作相关状态"
git status --short -- 04_scripts/一念通天 06_dumped/一念通天 10_docs/一念通天 10_docs/_state/current.md mod菜单/starry-mod-menu
echo "==> 全仓未处理项（应仅见其他会话/游戏）"
git status --short
