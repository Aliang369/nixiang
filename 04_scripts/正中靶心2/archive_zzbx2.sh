#!/usr/bin/env bash
# 正中靶心2：Git 固化。只提交本作相关路径，不触碰其他会话/游戏改动。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$NIXIANG_ROOT"

git config core.quotepath false

[[ -f "04_scripts/正中靶心2/cleanup_summary.json" ]] || { echo "错误: 缺少 cleanup_summary.json，先跑清理阶段" >&2; exit 1; }
[[ -f "10_docs/正中靶心2/收尾与最小复现清单_1.10_20261124.md" ]] || { echo "错误: 缺少本作收尾清单" >&2; exit 1; }

git add -- \
  "04_scripts/正中靶心2" \
  "10_docs/正中靶心2" \
  "10_docs/_state/games/正中靶心2.md" \
  "10_docs/_state/current.md" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/StarryInitProvider.java" \
  "mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/StarryAppComponentFactory.java" \
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
    04_scripts/正中靶心2/*|10_docs/正中靶心2/*|\
    10_docs/_state/games/正中靶心2.md|10_docs/_state/current.md|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/StarryInitProvider.java|\
    mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/StarryAppComponentFactory.java|\
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

# ── 提交信息一律从事实取值（禁硬编码版本号/指纹）─────────────────────────────
# 事故（2026-11-24 提交 870aa85a）：消息硬编码「1.9」而本轮是 1.10，且收尾用裸
# `git push git@… main`，违反 AGENTS.md §5「push 一律经 push_main」。此后：
#   版本 ← official_apk.json 的 version_name/version_code
#   正式包 md5 ← 08_output/正中靶心2/ 下唯一 *.apk
# 任一取不到即报错退出（**不退回硬编码**）；已入库的历史提交不改写。
META_FILE="$SCRIPT_DIR/official_apk.json"
OUT_DIR="$NIXIANG_ROOT/08_output/正中靶心2"

_zzbx2_meta_get() {  # $1 = official_apk.json 的键；取不到返回非 0
  python3 - "$META_FILE" "$1" <<'PY'
import json
import sys

path, key = sys.argv[1], sys.argv[2]
try:
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
except (OSError, ValueError) as e:
    raise SystemExit("读取 official_apk.json 失败: %s" % e)
val = data.get(key)
if val in (None, ""):
    raise SystemExit("official_apk.json 缺少键或值为空: " + key)
sys.stdout.write(str(val))
PY
}

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 ${META_FILE}，无法取版本（不退回硬编码）" >&2; exit 1; }
VER="$(_zzbx2_meta_get version_name)" || { echo "错误: 无法从 official_apk.json 取 version_name，拒绝硬编码版本" >&2; exit 1; }
VC="$(_zzbx2_meta_get version_code)" || { echo "错误: 无法从 official_apk.json 取 version_code，拒绝硬编码" >&2; exit 1; }
[[ -n "$VER" && -n "$VC" ]] || { echo "错误: official_apk.json 的 version_name/version_code 为空" >&2; exit 1; }

# 正式包：目录下必须**有且仅有** 1 个 APK；多个即不确定，报错而不猜。
shopt -s nullglob
APKS=("$OUT_DIR"/*.apk)
shopt -u nullglob
if [[ ${#APKS[@]} -ne 1 ]]; then
  echo "错误: $OUT_DIR 下应恰好有 1 个 APK，实际 ${#APKS[@]} 个，无法确定正式包 md5" >&2
  [[ ${#APKS[@]} -gt 0 ]] && printf '  %s\n' "${APKS[@]}" >&2
  exit 1
fi
# md5 取值要跨环境：GUI/精简 PATH 下没有 /usr/bin 之外的 md5，故按 BSD→GNU→openssl 回退。
_zzbx2_md5() {  # $1 = 文件；输出 32 位小写十六进制，取不到则输出空
  local f="$1" out=""
  if command -v md5 >/dev/null 2>&1; then out="$(md5 -q "$f" 2>/dev/null || true)"
  elif [[ -x /sbin/md5 ]]; then out="$(/sbin/md5 -q "$f" 2>/dev/null || true)"
  elif command -v md5sum >/dev/null 2>&1; then out="$(md5sum "$f" 2>/dev/null | awk '{print $1}' || true)"
  elif command -v openssl >/dev/null 2>&1; then out="$(openssl md5 "$f" 2>/dev/null | awk '{print $NF}' || true)"
  fi
  printf '%s' "$out"
}
MD5="$(_zzbx2_md5 "${APKS[0]}")"
[[ -n "$MD5" ]] || { echo "错误: 计算正式包 md5 失败（无 md5/md5sum/openssl）: ${APKS[0]}" >&2; exit 1; }
[[ "$MD5" =~ ^[0-9a-f]{32}$ ]] || { echo "错误: 正式包 md5 非法: $MD5" >&2; exit 1; }

COMMIT_MSG="正中靶心2 ${VER} 星光版复刻收官（底包 vc${VC}，正式包 md5 ${MD5}）"
printf '%s\n' "$STAGED"
git commit -m "$COMMIT_MSG"

# ── 推送一律经 push_main（AGENTS.md §5：禁裸 git push、禁 --force）──────────────
# push_main rc：0=已推送 1=已提交未推送 3=远端分叉（需 fetch/rebase）
# shellcheck source=/dev/null
source "$NIXIANG_ROOT/04_scripts/_common/git_push_main.sh"
PUSH_RC=0
push_main main || PUSH_RC=$?
case "$PUSH_RC" in
  0) echo "==> 已推送 origin/main" ;;
  1) echo "错误: 未推送（push_main rc=1）：提交已保留在本地 $(git rev-parse --short HEAD)" >&2
     echo "      本地领先 origin/main $(git rev-list --count origin/main..main 2>/dev/null || echo '?') 个提交；请重跑推送，禁 --force" >&2
     exit 1 ;;
  3) echo "错误: 远端与本地历史分叉（push_main rc=3）：需人工 fetch/rebase 后重推，禁 --force" >&2
     echo "      远端 main tip=$(git ls-remote origin main 2>/dev/null | awk '{print $1}') 本地 tip=$(git rev-parse --short HEAD)" >&2
     exit 3 ;;
  *) echo "错误: push_main 异常返回 rc=${PUSH_RC}（提交已在本地 $(git rev-parse --short HEAD)）" >&2
     exit "$PUSH_RC" ;;
esac

echo "==> 本作相关状态"
git status --short -- 04_scripts/正中靶心2 06_dumped/正中靶心2 10_docs/正中靶心2 10_docs/_state/games/正中靶心2.md 10_docs/_state/current.md mod菜单/starry-mod-menu
echo "==> 全仓未处理项（应仅见其他会话/游戏）"
git status --short
