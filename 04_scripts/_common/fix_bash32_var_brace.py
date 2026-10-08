#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""fix_bash32_var_brace.py — 机械修 `$VAR` 紧跟非 ASCII（bash 3.2 硬错误 ⓵）

背景（2026-10-08 我本布衣世家录预检复盘）：
  `echo "（限 $PKG）"` 这类写法在 **UTF-8 locale** 下会被 macOS bash 3.2 把中文首字节
  并进变量名 ⇒ `PKG\ufffd: unbound variable`，脚本中断；`LC_ALL=C` 反而不炸，
  所以平时「看着没事」，一到真机预检才炸（post_install_precheck.sh 就这样漏了半年）。
  修法唯一且无副作用：`$VAR` → `${VAR}`。

判据与 04_scripts/_common/bash32_check.sh 的 ⓵ **逐行一致**（含 heredoc / 行注释 /
块注释处理），故「门禁报什么就修什么」，不会多改也不会漏改。

用法:
  python3 04_scripts/_common/fix_bash32_var_brace.py <文件...>          # dry-run，只打印
  python3 04_scripts/_common/fix_bash32_var_brace.py --apply <文件...>  # 写回

典型：门禁报错后
  python3 04_scripts/_common/fix_bash32_var_brace.py --apply $(git diff --name-only | grep '\\.sh$')
  然后 bash 04_scripts/_common/bash32_check.sh <同上>   # 应报 0 硬错误
"""
import re
import sys


def hits_of(path):
    """返回 [(行号, 变量名)]，判据同 bash32_check.sh 的 ⓵。"""
    lines = open(path, encoding="utf-8", errors="replace").read().splitlines()
    out = []
    in_comment_block = False
    heredoc_tag = None
    heredoc_quoted = False
    for i, line in enumerate(lines, 1):
        if heredoc_tag is not None:
            if line.strip() == heredoc_tag or line == heredoc_tag:
                heredoc_tag = None
                continue
            if heredoc_quoted:
                continue                    # <<'TAG' 字面量：$VAR 不展开，也不该改
            code = line
        else:
            code = line
            code = re.sub(r'(?<!:)#.*$', '', code)
            if in_comment_block:
                if '*/' in code:
                    code = code.split('*/', 1)[1]
                    in_comment_block = False
                else:
                    continue
            if re.match(r'^\s*/\*(?!\))', code) and '*/' not in code:
                code = ''
                in_comment_block = True
            m = re.search(r"<<-?\s*(['\"]?)([A-Za-z_][A-Za-z0-9_]*)\1\s*$", code)
            if m:
                heredoc_tag = m.group(2)
                heredoc_quoted = (m.group(1) != '')
        if not code.strip():
            continue
        for m in re.finditer(r'(?<!\\)\$([A-Za-z_][A-Za-z_0-9]*)([^\x00-\x7F])', code):
            out.append((i, m.group(1)))
    return out, lines


def main():
    apply = '--apply' in sys.argv
    files = [a for a in sys.argv[1:] if a != '--apply']
    if not files:
        print(__doc__)
        return 2
    total = 0
    touched = []
    for f in files:
        hs, lines = hits_of(f)
        if not hs:
            print("  [跳过] %s 无命中" % f)
            continue
        for ln, var in hs:
            old = lines[ln - 1]
            new = re.sub(r'(?<!\\)\$%s(?=[^\x00-\x7F])' % re.escape(var),
                         '${%s}' % var, old)
            print("  %s:%d  $%s\n      - %s\n      + %s"
                  % (f, ln, var, old.strip()[:110], new.strip()[:110]))
            lines[ln - 1] = new
            total += 1
        if apply:
            open(f, 'w', encoding='utf-8').write('\n'.join(lines) + '\n')
            touched.append(f)
    print("命中 %d 处%s" % (total, "（已写回 %d 个文件）" % len(touched) if apply else "（dry-run，未写回）"))
    return 0


if __name__ == '__main__':
    sys.exit(main())
