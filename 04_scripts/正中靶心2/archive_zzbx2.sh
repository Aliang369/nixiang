#!/usr/bin/env bash
# 正中靶心2：Git 固化。只提交本作相关路径，不触碰其他会话/游戏改动。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$NIXIANG_ROOT"

git config core.quotepath false

[[ -f "04_scripts/正中靶心2/cleanup_summary.json" ]] || { echo "错误: 缺少 cleanup_summary.json，先跑清理阶段" >&2; exit 1; }
[[ -f "10_docs/正中靶心2/收尾与最小复现清单_1.7_20260915.md" ]] || { echo "错误: 缺少本作收尾清单" >&2; exit 1; }

git add -- \
  "04_scripts/正中靶心2" \
  "10_docs/正中靶心2" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/StarryInitProvider.java" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/cocos/Zzbx2Hooks.java" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android_zzbx2.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Zzbx2Entry.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Zzbx2Mod.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Zzbx2Mod.h"

# 文本白名单可能由清理阶段重新写入 index；不把 dex/so/bin 引入。
git add -A -- "06_dumped/正中靶心2"

if git diff --cached --name-only -- "06_dumped/正中靶心2" | grep -E '\.(apk|ap_|so|o|jar|bin|dat|dex|i64|idb)$' >/dev/null; then
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
    04_scripts/正中靶心2/*|10_docs/正中靶心2/*|10_docs/_state/current.md|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/StarryInitProvider.java|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/cocos/Zzbx2Hooks.java|\
    mod菜单/starry-mod-menu/app/src/main/jni/Android_zzbx2.mk|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Zzbx2Entry.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Zzbx2Mod.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Zzbx2Mod.h|\
    06_dumped/正中靶心2/*)
      ;;
    *)
      echo "错误: 暂存区出现非本作路径，拒绝提交: $path" >&2
      exit 1
      ;;
  esac
done <<< "$STAGED"

printf '%s\n' "$STAGED"
git commit -m "正中靶心2 1.7 星空包交付与收官"
git push origin main

echo "==> 本作相关状态"
git status --short -- 04_scripts/正中靶心2 06_dumped/正中靶心2 10_docs/正中靶心2 10_docs/_state/current.md mod菜单/starry-mod-menu
echo "==> 全仓未处理项（应仅见其他会话/游戏）"
git status --short
