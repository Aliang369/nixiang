#!/usr/bin/env bash
# ============================================================
#  bash32_check.sh — macOS bash 3.2 兼容门禁（伊莫 1.0.1 会话复盘 P1-3）
#
#  用法: bash 04_scripts/_common/bash32_check.sh <脚本.sh> [...]
#  退出码: 0 = 通过（可能含 ⚠ 提示）；1 = 有 ✗ 硬错误
#
#  为什么：本仓库所有 `04_scripts/**`、`mod菜单/**/scripts/**` 都在 macOS 自带
#  bash 3.2 下跑。bash 4+ 的语法不是"降级"，是**整套脚本直接起不来**；
#  而 `set -u` 与空数组/多字节变量名的组合会**静默少做一件事**（最难查）。
#
#  三类检查：
#    ✗ ① `$VAR` 紧跟非 ASCII（bash 把中文并进变量名 ⇒ unbound variable。
#         实测 `$HAS_CXXSHARED（` ：行 91 unbound variable。修法：`${VAR}（`）
#    ✗ ② bash 4+ 语法：mapfile / readarray / declare -A / local -n / ${v^^} / ${v,,}
#         / ;;& / &>> / [[ -v x ]] / &>  （bash 4 才有）
#    ⚠ ③ `set -u` 下的裸空数组展开 `"${arr[@]}"`、`${#arr[@]}`：
#         数组为空即 unbound variable。修法 `${arr[@]+"${arr[@]}"}` 或用显式计数器。
#         （⚠ 不判 FAIL：数组确定非空时是合法写法，需要人看一眼）
# ============================================================
set -uo pipefail

[ $# -ge 1 ] || { echo "用法: bash 04_scripts/_common/bash32_check.sh <脚本.sh>"; exit 1; }

hard=0; soft=0
for f in "$@"; do
  [ -f "$f" ] || { echo "✗ 缺文件 $f"; hard=$((hard+1)); continue; }
  # PYTHONIOENCODING=utf-8：Windows 控制台是 GBK，python 打印 '✗' 会抛
  # UnicodeEncodeError 并只吐半截输出 —— 那会让本脚本**误报通过**（见下方空 counts 兜底）
  out=$(PYTHONIOENCODING=utf-8 python3 - "$f" <<'PY'
import re, sys
p = sys.argv[1]
lines = open(p, encoding="utf-8", errors="replace").read().splitlines()
hard, soft = [], []
in_comment_block = False
bc_line = 0          # `/*` 起始行（未闭合时用于报「其后未检查」）
# heredoc 状态: <<TAG 启动后, 到顶格 TAG 行之前的内容是字面/待展开文本 ——
# ① 其中的 `#` 不是注释(2026-10-04 实测: 心跳 heredoc 里 `# 机器心跳: $HOST（`
#   被注释剔除放过, 真跑 unbound variable); ② unquoted heredoc 里 $VAR 会展开, 必须检查
heredoc_tag = None
heredoc_quoted = False
for i, line in enumerate(lines, 1):
    if heredoc_tag is not None:
        if line.strip() == heredoc_tag or line == heredoc_tag:
            heredoc_tag = None
            continue
        if heredoc_quoted:
            continue                      # <<'TAG' 字面量, 不展开, 无风险
        code = line                       # 待展开内容: 不剔注释, 整行检查
    else:
        code = line
        # 粗略剔除注释（够用：本仓库脚本无复杂嵌套）
        # 行注释**先**剔：否则注释里的 `/*`（例：`# ... 10_docs/<游戏>/*_menu_template.json`）
        # 会被下面的块注释判定吞掉，连带其后整份文件被跳过。
        code = re.sub(r'(?<!:)#.*$', '', code)          # 行注释（跳过 ${#x} 里的 # 由 (?<!:) 兜）
        if in_comment_block:
            if '*/' in code: code = code.split('*/', 1)[1]; in_comment_block = False
            else: continue
        # 块注释起始**只认**「行首（去空白）就是 `/*` 且行内无 `*/`」的多行 C 注释。
        # 2026-10-08 实测：旧判据「行内出现 `/*`」把三种**代码**误判成注释开头 ⇒
        # 其后所有行被 continue 跳过 ⇒ **门禁静默假 PASS**（post_install_precheck.sh
        # 的 `$PKG）` 硬错误就是这样漏掉的，装机预检才炸）。被误判的三种写法：
        #   ① 路径 glob：`10_docs/$GAME/*_menu_template.json`
        #   ② 绝对路径判定：`[[ "$APK" = /* ]]` / `!= /*` / `== /*`
        #   ③ case 分支：`/*) pf_args+=("$pf_one") ;;`
        # 故加 `(?!\))` 排除 case 分支，并要求 `/*` 前只有空白。
        if re.match(r'^\s*/\*(?!\))', code) and '*/' not in code:
            code = ''; in_comment_block = True; bc_line = i
        m = re.search(r"<<-?\s*(['\"]?)([A-Za-z_][A-Za-z0-9_]*)\1\s*$", code)
        if m:
            heredoc_tag = m.group(2); heredoc_quoted = (m.group(1) != '')
    if not code.strip(): continue
    # ⓵ $VAR 紧跟非 ASCII
    # (?<!\\) ：`\$VAR（` 是转义的字面量，不展开，不报错
    for m in re.finditer(r'(?<!\\)\$([A-Za-z_][A-Za-z_0-9]*)([^\x00-\x7F])', code):
        hard.append("L%d: `$%s%s` —— bash 会把 %r 并进变量名 ⇒ 未定义变量报错；改成 `${%s}`"
                    % (i, m.group(1), m.group(2), m.group(2), m.group(1)))
    # ⓶ bash 4+ 语法
    for pat, why in [(r'\bmapfile\b', 'mapfile 是 bash 4 才有'),
                     (r'\breadarray\b', 'readarray 是 bash 4 才有'),
                     (r'\bdeclare\s+-A\b', '关联数组是 bash 4 才有'),
                     (r'\blocal\s+-n\b', 'nameref 是 bash 4.3 才有'),
                     (r'\$\{[A-Za-z_][A-Za-z_0-9]*(\^\^|,,)\}', '${v^^}/${v,,} 是 bash 4 才有'),
                     (r';;&', '`;;&` 是 bash 4 才有'),
                     (r'&>>', '`&>>` 是 bash 4 才有'),
                     (r'&>\s', '`&>` 是 bash 4 才有'),
                     (r'\[\[\s+-v\s+', '`[[ -v ]]` 是 bash 4.2 才有'),
                     (r'\bdeclare\s+-n\b', 'declaure -n 是 bash 4 才有')]:
        if re.search(pat, code):
            hard.append("L%d: %s —— %s" % (i, code.strip()[:70], why))
    # ⓷ 裸空数组展开（仅在脚本有 set -u 时提示）
    if re.search(r'(?<!\+)\$\{[A-Za-z_][A-Za-z_0-9]*\[@\]\}', code) and '+${' not in code and '[@]+' not in code:
        soft.append("L%d: 裸 `${arr[@]}` 展开 —— 若数组可能为空，`set -u` 下会 unbound；"
                    "改 `${arr[@]+\"${arr[@]}\"}` 或用计数器  (%s)" % (i, code.strip()[:60]))
    if re.search(r'\$\{#[A-Za-z_][A-Za-z_0-9]*\[@\]\}', code):
        soft.append("L%d: `${#arr[@]}` —— 空数组 + `set -u` 会 unbound；改用计数器  (%s)"
                    % (i, code.strip()[:60]))
# 块注释未闭合 ⇒ 其后所有行都没被检查过。宁可误报 FAIL，也绝不静默放过。
if in_comment_block:
    hard.append("L%d 起 `/*` 块注释未闭合（全文找不到 `*/`）—— 其后 %d 行**未被检查**；"
                "请补 `*/`，或把 `/*` 改成行注释/加空格（防门禁假 PASS）"
                % (bc_line, len(lines) - bc_line))
has_u = any(re.search(r'^\s*set\s+-[a-z]*u', l) for l in lines)
for h in hard: print("✗ " + h)
if has_u:
    for s in soft: print("⚠ " + s)
elif soft:
    print("⚠ 本脚本未 `set -u`，空数组展开风险降低（仍建议统一写法）")
print("__COUNTS__ %d %d" % (len(hard), len(soft)))
PY
)
  counts=$(echo "$out" | grep '^__COUNTS__' | awk '{print $2, $3}')
  # 没拿到 __COUNTS__ ⇒ 探测进程崩了（如 GBK 控制台 UnicodeEncodeError 只吐半截），
  # 此时 h/s 都取默认 0，会**静默误报通过**。宁可误报 FAIL，也不放过。
  if [ -z "$counts" ]; then
    echo "  [$f] ✗ 兼容性探测未产出结果（python 探测进程异常退出）——按 FAIL 处理，请人工检查该文件"
    echo "$out" | sed "s|^|  [$f] 探测输出: |"
    hard=$((hard + 1))
    continue
  fi
  body=$(echo "$out" | grep -v '^__COUNTS__')
  [ -n "$body" ] && echo "$body" | sed "s|^|  [$f] |"
  h=$(echo "$counts" | awk '{print $1}'); s=$(echo "$counts" | awk '{print $2}')
  hard=$((hard + ${h:-0})); soft=$((soft + ${s:-0}))
done

if [ "$hard" -gt 0 ]; then
  echo "✗ bash 3.2 门禁：$hard 处硬错误（必须修）$( [ "$soft" -gt 0 ] && echo "，另有 $soft 处提示")"
  echo "  机械修法（判据与本门禁 ⓵ 一致，只改 \$VAR → \${VAR}）:"
  echo "    python3 04_scripts/_common/fix_bash32_var_brace.py <文件...>           # 先 dry-run 预览"
  echo "    python3 04_scripts/_common/fix_bash32_var_brace.py --apply <文件...>   # 写回"
  exit 1
fi
if [ "$soft" -gt 0 ]; then
  echo "⚠ bash 3.2 门禁：无硬错误，$soft 处空数组展开提示（确认数组非空即可）"
fi
exit 0
