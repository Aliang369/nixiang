#!/usr/bin/env bash
# 暗影格斗3 1.45.3：收官 Git 固化。
# 只提交本作相关路径，不触碰其他会话/游戏的改动；08_output 与 02_apks 由用户手动管理，不入库。
# 说明：本脚本只做本地提交，不 push（本工作区惯例：推送由用户决定）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$NIXIANG_ROOT"

git config core.quotepath false

[[ -f "10_docs/暗影格斗3/收尾与最小复现清单_1.45.3_20260919.md" ]] \
  || { echo "错误: 缺少本作收尾清单，先写文档" >&2; exit 1; }
[[ -f "10_docs/暗影格斗3/正式包裁剪清单_1.45.3_20260919.json" ]] \
  || { echo "错误: 缺少正式包裁剪清单，先跑 prune_output_final.sh --apply" >&2; exit 1; }
[[ -f "08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk" ]] \
  || { echo "错误: 缺少唯一正式包（08_output 未裁剪或包被移走）" >&2; exit 1; }

# 08_output 只允许留一个 apk（唯一正式包）；多包=裁剪没做完，拒绝归档
APK_COUNT="$(find 08_output/暗影格斗3 -maxdepth 1 -name '*.apk' | wc -l | tr -d ' ')"
[[ "$APK_COUNT" == "1" ]] || { echo "错误: 08_output/暗影格斗3 下有 ${APK_COUNT} 个 apk，应只剩唯一正式包" >&2; exit 1; }

# 01_tools/02_apks/07_patched/08_output 均被 .gitignore 排除，此处只白名单本作源码/脚本/文档
git add -- \
  "04_scripts/暗影格斗3" \
  "10_docs/暗影格斗3" \
  "10_docs/_state/games/暗影格斗3.md" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/bridge/ModBridge.java" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android_sf3.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/MainModern.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Entry.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.h"

# 06_dumped 只进文本（dump.cs/stringliteral.json/扫描 txt）；大二进制由 .gitignore 兜底
git add -A -- "06_dumped/暗影格斗3"

if git diff --cached --name-only -- "06_dumped/暗影格斗3" \
   | grep -E '\.(apk|ap_|so|o|jar|bin|dat|dex|i64|idb)$' >/dev/null; then
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
    04_scripts/暗影格斗3/*|10_docs/暗影格斗3/*|\
    10_docs/_state/games/暗影格斗3.md|10_docs/_state/current.md|\
    06_dumped/暗影格斗3/*|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/bridge/ModBridge.java|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java|\
    mod菜单/starry-mod-menu/app/src/main/jni/Android_sf3.mk|\
    mod菜单/starry-mod-menu/app/src/main/jni/MainModern.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Entry.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.h)
      ;;
    *)
      echo "错误: 暂存区出现非本作路径，拒绝提交: $path" >&2
      exit 1
      ;;
  esac
done <<< "$STAGED"

printf '%s\n' "$STAGED"
git commit -m "暗影格斗3 1.45.3 收尾清理：06_dumped 大二进制删除 + 08_output 裁剪 + 文档补录"

echo "==> 本作相关状态"
git status --short -- 04_scripts/暗影格斗3 06_dumped/暗影格斗3 10_docs/暗影格斗3 10_docs/_state/games/暗影格斗3.md 10_docs/_state/current.md mod菜单/starry-mod-menu
echo "==> 全仓未处理项（应仅见其他会话/游戏）"
git status --short
