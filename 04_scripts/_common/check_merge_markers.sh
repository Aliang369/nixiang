#!/usr/bin/env bash
# ================================================================
#  check_merge_markers.sh — Git 冲突标记残留检查
#
#  用法:
#    bash 04_scripts/_common/check_merge_markers.sh            # 扫全部 git 跟踪文件
#    bash 04_scripts/_common/check_merge_markers.sh <文件...>   # 只扫指定文件
#
#  退出码: 0=无残留；1=存在残留
#
#  为什么需要（2026-11-10 实测事故）：
#    `10_docs/_state/current.md`（每次开局注入的状态索引）里带着 `<<<<<<< HEAD` /
#    `=======` / `>>>>>>> <hash>` 三行，且已经 commit 进 main。成因是双机协作时
#    `git pull --rebase --autostash` 把生成物卷进冲突，生成器当时没有覆盖该区块。
#    后果是每次开会话，AI 读到的第一份全局状态里就夹着三行垃圾 —— 这正是
#    「开局注入内容会误导判断」的具体实例，而且没有任何门禁能拦住它：
#    `preflight --check` 只验 frontmatter，不验冲突标记。
#
#  设计取舍：
#    * 只扫 `git ls-files` 跟踪的文件 —— 未跟踪的临时产物带标记不算事故（也不是我们的输出）。
#    * `=======` 单独出现不算（Markdown 表格分隔行、setext 标题下划线都可能长这样）；
#      只认「行首 7 个 < 或 7 个 >」，这是 Git 冲突标记的唯一可靠特征。
#    * 跳过 .git/ 与 node_modules/：前者本来就有标记、后者是第三方代码。
# ================================================================
set -uo pipefail

cd "$(dirname "$0")/../.." || exit 2

PAT='^(<<<<<<< |>>>>>>> )'

# 有参数就只查参数（配合"改完任何文件必跑"的按文件模式）；无参数则全仓
if [ "$#" -gt 0 ]; then
  targets=()
  for f in "$@"; do
    [ -f "$f" ] || continue
    case "$f" in
      *.md|*.json|*.sh|*.py|*.tsv|*.yaml|*.yml|*.java|*.cpp|*.h|*.mk|*.txt|*.properties|*.xml) targets+=("$f") ;;
    esac
  done
  [ "${#targets[@]}" -eq 0 ] && { echo "✓ PASS  [冲突标记] 无适用类型的待查文件"; exit 0; }
  hits=$(printf '%s\0' "${targets[@]}" | xargs -0 grep -lE "$PAT" 2>/dev/null)
else
  # git ls-files 只列跟踪文件；node_modules 若被跟踪也应排除（第三方代码的标记不是我们的）
  hits=$(git ls-files -z -- '*.md' '*.json' '*.sh' '*.py' '*.tsv' '*.yaml' '*.yml' \
                              '*.java' '*.cpp' '*.h' '*.mk' '*.txt' '*.properties' '*.xml' 2>/dev/null \
         | xargs -0 grep -lE "$PAT" 2>/dev/null)
fi

if [ -n "$hits" ]; then
  echo "✗ FAIL  [冲突标记] 以下文件残留 Git 冲突标记（<<<<<<< / >>>>>>>）："
  printf '%s\n' "$hits" | sed 's/^/       /'
  echo "       修法：能重新生成的（如 _state/current.md）直接跑它的生成器；"
  echo "             不能生成的逐处人工合并，保留的一侧删掉三条标记行，然后 preflight 复跑。"
  exit 1
fi

echo "✓ PASS  [冲突标记] 0 处残留"
exit 0
