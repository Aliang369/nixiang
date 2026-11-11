#!/usr/bin/env bash
# 追迹迷城：Git 固化。只提交本作相关路径，不触碰其他会话/游戏改动。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$NIXIANG_ROOT"

git config core.quotepath false

[[ -f "04_scripts/追迹迷城/cleanup_summary.json" ]] || { echo "错误: 缺少 cleanup_summary.json，先跑清理阶段" >&2; exit 1; }
[[ -f "10_docs/追迹迷城/收尾清单_1.0.20260912_20260914.md" ]] || { echo "错误: 缺少本作收尾清单" >&2; exit 1; }

git add -- \
  "04_scripts/追迹迷城" \
  "10_docs/追迹迷城" \
  "10_docs/_state/games/追迹迷城.md" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Main.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/ZjmcMod.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/ZjmcMod.h" \
  "mod菜单/starry-mod-menu/scripts/surgical_zjmc_1.0.20260912.sh"

git add -f -- \
  "mod菜单/starry-mod-menu/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so" \
  "mod菜单/starry-mod-menu/app/src/main/libs/arm64-v8a/libstarry_magic.so" \
  "06_dumped/追迹迷城/1.0.20260912_il2cpp/dump.cs" \
  "06_dumped/追迹迷城/1.0.20260912_il2cpp/stringliteral.json" \
  "06_dumped/追迹迷城/1.0.20260912_il2cpp/rva_targets.json"

STAGED="$(git diff --cached --name-only)"
if [[ -z "$STAGED" ]]; then
  echo "没有待提交的本作改动"
  exit 0
fi

while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  case "$path" in
    04_scripts/追迹迷城/*|10_docs/追迹迷城/*|10_docs/_state/games/追迹迷城.md|10_docs/_state/current.md|\
    mod菜单/starry-mod-menu/app/src/main/jni/Android.mk|\
    mod菜单/starry-mod-menu/app/src/main/jni/Main.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/ZjmcMod.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/ZjmcMod.h|\
    mod菜单/starry-mod-menu/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so|\
    mod菜单/starry-mod-menu/app/src/main/libs/arm64-v8a/libstarry_magic.so|\
    mod菜单/starry-mod-menu/scripts/surgical_zjmc_1.0.20260912.sh|\
    06_dumped/追迹迷城/1.0.20260912_il2cpp/dump.cs|\
    06_dumped/追迹迷城/1.0.20260912_il2cpp/stringliteral.json|\
    06_dumped/追迹迷城/1.0.20260912_il2cpp/rva_targets.json)
      ;;
    *)
      echo "错误: 暂存区出现非本作路径，拒绝提交: $path" >&2
      exit 1
      ;;
  esac
done <<< "$STAGED"

printf '%s\n' "$STAGED"
git commit -m "追迹迷城 1.0.20260912 星空包交付与收官"
git push origin main

echo "==> 本作相关状态"
git status --short -- 04_scripts/追迹迷城 06_dumped/追迹迷城 10_docs/追迹迷城 10_docs/_state/games/追迹迷城.md 10_docs/_state/current.md mod菜单/starry-mod-menu
echo "==> 全仓未处理项（应仅见其他会话/游戏）"
git status --short
