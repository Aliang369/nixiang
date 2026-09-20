#!/usr/bin/env bash
# 蛋壳战机 1.1.5：收官 Git 固化。
# 只提交本作相关路径，不触碰其他会话/游戏的改动；08_output、02_apks 与设备备份不入库。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$NIXIANG_ROOT"

git config core.quotepath false

SIGNED="08_output/蛋壳战机/蛋壳战机_1.1.5_starry-releaseSigned.apk"
DOC="10_docs/蛋壳战机/星空落地_1.1.5_20260920.md"
FINAL_DOC="10_docs/蛋壳战机/收尾与最小复现清单_1.1.5_20260920.md"
MANIFEST="10_docs/蛋壳战机/正式包裁剪清单_1.1.5_20260920.json"
CLEANUP="04_scripts/蛋壳战机/cleanup_summary.json"

for required in "$SIGNED" "$DOC" "$FINAL_DOC" "$MANIFEST" "$CLEANUP"; do
  [[ -f "$required" ]] || { echo "错误: 缺少收尾产物: $required" >&2; exit 1; }
done

APK_COUNT="$(find 08_output/蛋壳战机 -maxdepth 1 -name '*.apk' | wc -l | tr -d ' ')"
[[ "$APK_COUNT" == "1" ]] || {
  echo "错误: 08_output/蛋壳战机 下有 ${APK_COUNT} 个 apk，应只剩唯一正式包" >&2
  exit 1
}

for stale in \
  "08_output/蛋壳战机/蛋壳战机_1.1.5_starry-unsigned.apk" \
  "08_output/蛋壳战机/蛋壳战机_1.1.5_starry-releaseSigned.apk.idsig" \
  "06_dumped/蛋壳战机/il2cpp_dump_1.1.5/script.json" \
  "06_dumped/蛋壳战机/il2cpp_dump_1.1.5/DummyDll" \
  "06_dumped/蛋壳战机/il2cpp_dump_1.1.5/il2cpp.h" \
  "06_dumped/蛋壳战机/static_inspect/key_files" \
  "07_patched/蛋壳战机" \
  "mod菜单/starry-mod-menu/libs_dkzj" \
  "mod菜单/starry-mod-menu/obj_dkzj"; do
  [[ -e "$stale" ]] && { echo "错误: 收尾中间产物未清理: $stale" >&2; exit 1; }
done

git add -- \
  "04_scripts/蛋壳战机" \
  "10_docs/蛋壳战机" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android_dkzj.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjEntry.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjMod.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjMod.h"

git add -A -- "06_dumped/蛋壳战机"
if git diff --cached --name-only -- "06_dumped/蛋壳战机" \
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
    04_scripts/蛋壳战机/*|10_docs/蛋壳战机/*|\
    10_docs/_state/current.md|06_dumped/蛋壳战机/*|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java|\
    mod菜单/starry-mod-menu/app/src/main/jni/Android_dkzj.mk|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjEntry.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjMod.cpp|\
    mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjMod.h)
      ;;
    *)
      echo "错误: 暂存区出现非本作路径，拒绝提交: $path" >&2
      exit 1
      ;;
  esac
done <<< "$STAGED"

# 原始 logcat / dump.cs 行尾常带空格，属于生成证据；空白门禁只覆盖手写代码、JSON 和 Markdown。
git diff --cached --check -- \
  "04_scripts/蛋壳战机/*.sh" \
  "04_scripts/蛋壳战机/*.json" \
  "10_docs/蛋壳战机/*.md" \
  "10_docs/蛋壳战机/*.json" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Android_dkzj.mk" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjEntry.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjMod.cpp" \
  "mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjMod.h"
printf '%s\n' "$STAGED"
git commit -m "蛋壳战机 1.1.5 星空包交付与收官"
git push origin main

echo "==> 本作相关状态"
git status --short -- 04_scripts/蛋壳战机 06_dumped/蛋壳战机 10_docs/蛋壳战机 10_docs/_state/current.md mod菜单/starry-mod-menu
echo "==> 全仓未处理项（应仅见其他会话/游戏）"
git status --short
