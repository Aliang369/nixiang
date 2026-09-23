#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""WhrhxMod 逆向辅助：把 Il2CppDumper 的 dump.cs 变成 RVA→符号表，
再对 il2cpp.so 的指定地址区间做反汇编，并把 bl/b 目标标注成方法名。

用法:
  python3 whrhx_anno.py map   <dump.cs>                    # 只建表，输出指定符号的 RVA
  python3 whrhx_anno.py dis   <dump.cs> <so> <hex_start> [hex_end]
  python3 whrhx_anno.py who   <dump.cs> <hex_rva> [...]    # RVA 反查方法名(含附近)

约定：本 so 的 dump RVA == 文件偏移 == objdump 地址（已实测）。
"""
import re, subprocess, sys, bisect

RVA_RE = re.compile(r'//\s*RVA:\s*(0x[0-9A-Fa-f]+)')
SIG_RE = re.compile(r'^\s*(?:public|private|protected|internal|static|virtual|override|sealed|readonly|extern|\s)+[^\n;{}]*?([A-Za-z_<>][\w<>`\.\[\], ]*?)\s*\(([^)]*)\)')


def build_map(dump_path):
    """RVA -> (class, signature) —— 用最近的 class 行做归属。"""
    m = {}
    cls = ''
    last_rva = None
    with open(dump_path, 'r', encoding='utf-8', errors='replace') as f:
        for line in f:
            if line[:1] not in ('\t', ' ', '\n', '/', '{', '}'):
                c = re.match(r'[^\n]*?\b(?:class|struct|enum|interface)\s+([\w<>`\.]+)', line)
                if c:
                    cls = c.group(1)
            r = RVA_RE.search(line)
            if r:
                last_rva = int(r.group(1), 16)
                continue
            if last_rva is not None and '(' in line and line.strip().endswith('{ }'):
                s = line.strip()
                m.setdefault(last_rva, (cls, s))
                last_rva = None
    return m


def annotate(cmd_out, m):
    lines = []
    for line in cmd_out.splitlines():
        mm = re.search(r'\b(bl|b)\s+0x([0-9a-f]+)', line)
        if mm:
            tgt = int(mm.group(2), 16)
            hit = m.get(tgt)
            tag = ''
            if hit:
                tag = '   ; %s.%s' % hit
            else:
                # 落在某方法内部 1..0xFF 字节处 -> 标出宿主
                near = [k for k in m if 0 < tgt - k <= 0x100]
                if near:
                    k = max(near)
                    tag = '   ; %s+0x%X' % (m[k], tgt - k)
            line = line + tag
        lines.append(line)
    return '\n'.join(lines)


def main():
    mode = sys.argv[1]
    dump = sys.argv[2]
    m = build_map(dump)
    sys.stderr.write('[anno] %d methods indexed\n' % len(m))

    if mode == 'map':
        pat = sys.argv[3].lower()
        for rva in sorted(m):
            c, s = m[rva]
            if pat in s.lower() or pat in c.lower():
                print('0x%08X  %s.%s' % (rva, c, s))
        return

    if mode == 'who':
        keys = sorted(m)
        for a in sys.argv[3:]:
            tgt = int(a, 16)
            i = bisect.bisect_right(keys, tgt) - 1
            if i >= 0:
                print('%s: %s.%s  (+0x%X)' % (a, m[keys[i]][0], m[keys[i]][1], tgt - keys[i]))
        return

    if mode == 'dis':
        so = sys.argv[3]
        start = int(sys.argv[4], 16)
        end = int(sys.argv[5], 16) if len(sys.argv) > 5 else start + 0x200
        out = subprocess.run(['objdump', '-d', '--start-address=%d' % start,
                              '--stop-address=%d' % end, so],
                             capture_output=True, text=True).stdout
        print(annotate(out, m))
        return

    sys.stderr.write(__doc__)


if __name__ == '__main__':
    main()
