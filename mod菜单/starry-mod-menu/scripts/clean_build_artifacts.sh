#!/usr/bin/env bash
# ============================================================
#  构建产物统一清理（mod菜单/starry-mod-menu）
#
#  为什么需要：ndk-build 产物有三种落点，且 .gitignore 全忽略 —— git status
#  看不见、脚本只写不删，每做一个游戏就堆一套，长期累积到数百 MB。
#
#    A. 工程根      obj_<tag>/  libs_<tag>/      ← 主力写法，新脚本请用这个
#    B. app/src 下  obj_<tag>/  libs_<tag>/      ← 部分脚本写死，打包脚本会从这里读 so
#    C. app/build/  intermediates / outputs / ndk
#
#  用法:
#    bash scripts/clean_build_artifacts.sh                    # dry-run：只列清单与大小
#    bash scripts/clean_build_artifacts.sh --apply            # 真删
#    bash scripts/clean_build_artifacts.sh --tag mxzz         # 只清 mxzz 一套
#    bash scripts/clean_build_artifacts.sh --apply --with-venv # 连 auth-server/.venv
#
#  安全边界:
#    · 只处理上面三类显式落点；不碰 app/src/main/jniLibs（Gradle 打包源）
#      与 app/src/main/libs（同步副本），二者删了会影响出包
#    · 候选路径若含 git tracked 文件 ⇒ 拒绝并跳过（历史上清理清单压到 tracked
#      文件的教训）
#    · 默认 dry-run；--apply 会再打印一次清单再删
#    · 删除走显式路径 + rm -rf，不用通配符拼路径
# ============================================================
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MODE="dry-run"
TAG=""
WITH_VENV=0

usage() {
  sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --apply)      MODE="apply"; shift ;;
    --dry-run)    MODE="dry-run"; shift ;;
    --with-venv)  WITH_VENV=1; shift ;;
    --tag)
      [[ $# -ge 2 ]] || usage
      TAG="$2"; shift 2 ;;
    -h|--help)    usage ;;
    *) echo "ERROR: 未知参数 $1" >&2; usage ;;
  esac
done

[[ -d "$ROOT/app" ]] || { echo "ERROR: 不是 starry-mod-menu 工程根: $ROOT" >&2; exit 1; }

cands=()
if [[ -n "$TAG" ]]; then
  cands+=("$ROOT/obj_$TAG" "$ROOT/libs_$TAG" "$ROOT/app/src/obj_$TAG" "$ROOT/app/src/libs_$TAG")
else
  while IFS= read -r d; do cands+=("$d"); done < <(find "$ROOT" -maxdepth 1 -mindepth 1 -type d \( -name 'obj_*' -o -name 'libs_*' \) 2>/dev/null | LC_ALL=C sort)
  while IFS= read -r d; do cands+=("$d"); done < <(find "$ROOT/app/src" -maxdepth 1 -mindepth 1 -type d \( -name 'obj_*' -o -name 'libs_*' \) 2>/dev/null | LC_ALL=C sort)
  cands+=("$ROOT/app/build" "$ROOT/_dbgobj" "$ROOT/_dbglibs" "$ROOT/.gradle")
fi
[[ "$WITH_VENV" -eq 1 ]] && cands+=("$ROOT/auth-server/.venv")

total_kb=0
count=0
skipped=0
echo "[$MODE] 工程根: $ROOT"
echo

for p in "${cands[@]}"; do
  [[ -e "$p" ]] || continue
  rel="${p#$ROOT/}"
  if [[ -n "$(git -C "$ROOT" ls-files -- "$rel" 2>/dev/null | head -1)" ]]; then
    echo "  SKIP  (含 tracked 文件) $rel"
    skipped=$((skipped+1))
    continue
  fi
  kb="$(du -sk "$p" 2>/dev/null | awk '{print $1}')"
  kb="${kb:-0}"
  total_kb=$((total_kb + kb))
  count=$((count+1))
  printf "  %8.1f MB  %s\n" "$(awk -v k="$kb" 'BEGIN{print k/1024}')" "$rel"
  if [[ "$MODE" == "apply" ]]; then
    rm -rf -- "$p"
  fi
done

echo
printf "合计 %d 项 / %.1f MB" "$count" "$(awk -v k="$total_kb" 'BEGIN{print k/1024}')"
[[ "$skipped" -gt 0 ]] && printf "（跳过 %d 项含 tracked 文件）" "$skipped"
echo

if [[ "$MODE" == "dry-run" ]]; then
  echo "未删除任何文件。确认后加 --apply。"
else
  echo "已删除。下次出包会按需重编（单游戏约几十秒）；auth-server 需 pip install -r requirements.txt 重建 .venv。"
fi
