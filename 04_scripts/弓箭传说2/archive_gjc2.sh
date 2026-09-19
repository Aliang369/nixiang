#!/usr/bin/env bash
# 弓箭传说2 1.1.9：收官 Git 固化。
# 只提交本作相关路径，不触碰其他会话/游戏的改动；08_output 与 02_apks 由用户手动管理，不入库。
# 说明：本脚本只做本地提交，不 push（本工作区惯例：推送由用户决定）。
#
# 与 stages.md 收官清单的对应关系：
#   1 文档收尾 / 2 正式包 / 4 滚动物清理已由收尾流程完成，本脚本只做校验 + 第 6 步 git 固化。
#   校验失败即拒绝提交，避免"没裁干净/没写文档"却标记收官。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$NIXIANG_ROOT"

git config core.quotepath false

SIGNED="08_output/弓箭传说2/弓箭传说2_1.1.9_surgical-releaseSigned.apk"
DOC="10_docs/弓箭传说2/星空落地_1.1.9_20260919.md"
MANIFEST="10_docs/弓箭传说2/正式包裁剪清单_1.1.9_20260919.json"

[[ -f "$SIGNED" ]] || { echo "错误: 缺少唯一正式包（08_output 未裁剪或包被移走）" >&2; exit 1; }
[[ -f "$DOC" ]] || { echo "错误: 缺少落地文档 $DOC" >&2; exit 1; }
[[ -f "$MANIFEST" ]] || { echo "错误: 缺少正式包裁剪清单（先跑 prune_output_final.sh --apply）" >&2; exit 1; }

# 08_output 只允许留一个 apk（唯一正式包）；多包=裁剪没做完，拒绝归档
APK_COUNT="$(find 08_output/弓箭传说2 -maxdepth 1 -name '*.apk' | wc -l | tr -d ' ')"
[[ "$APK_COUNT" == "1" ]] || { echo "错误: 08_output/弓箭传说2 下有 ${APK_COUNT} 个 apk，应只剩唯一正式包" >&2; exit 1; }

# 收尾应清掉的中间产物：留着说明清理没做完
for stale in \
  "08_output/弓箭传说2/弓箭传说2_1.1.9_surgical-unsigned.apk" \
  "08_output/弓箭传说2/弓箭传说2_1.1.9_surgical-releaseSigned.apk.idsig"; do
  [[ -e "$stale" ]] && { echo "错误: 收尾中间产物未清理: $stale" >&2; exit 1; }
done

# 01_tools/02_apks/07_patched/08_output 均被 .gitignore 排除，此处只白名单本作源码/脚本/文档
git add -- \
  "04_scripts/弓箭传说2" \
  "10_docs/弓箭传说2" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android_gjc2.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Gjc2Entry.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Archero2Mod.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/Archero2Mod.h"

STAGED="$(git diff --cached --name-only)"
if [[ -z "$STAGED" ]]; then
  echo "没有待提交的本作改动"
  exit 0
fi

# 白名单之外一律拒绝，防止把其他会话/游戏的改动卷进本作提交
while IFS= read -r path; do
  [[ -n "$path" ]] || continue
  case "$path" in
    04_scripts/弓箭传说2/*|10_docs/弓箭传说2/*|\
    10_docs/_state/current.md|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java|\
    mod菜单/starry-mod-menu/app/src/main/jni/Android_gjc2.mk|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Gjc2Entry.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Archero2Mod.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/Archero2Mod.h)
      ;;
    *)
      echo "错误: 暂存区出现非本作路径，拒绝提交: $path" >&2
      exit 1
      ;;
  esac
done <<< "$STAGED"

printf '%s\n' "$STAGED"
git commit -m "弓箭传说2 1.1.9 状态机收口：archive 完成（9/9 阶段 done）"

echo "==> 本作相关状态"
git status --short -- 04_scripts/弓箭传说2 10_docs/弓箭传说2 10_docs/_state/current.md mod菜单/starry-mod-menu
echo "==> 全仓未处理项（应仅见其他会话/游戏）"
git status --short
