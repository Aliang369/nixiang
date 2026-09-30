#!/usr/bin/env python3
"""从 Lua 5.4 字节码反汇编清单按导出名提取单个函数（依赖 luac -l -l 调试信息）。
用法: python3 lua_fn.py <file.luac> [--list | 名字...]"""
import re, os, sys, subprocess

LUAC = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'lua54', 'luac')
HEAD = re.compile(r'^(main|function) <(.*?)> \((.*?) at (0x[0-9a-f]+)\)')
CLO  = re.compile(r'CLOSURE\s+(\d+)\s+\d+\s+; (0x[0-9a-f]+)')
SET  = re.compile(r'(SETTABUP|SETFIELD|SETTABLE)\s+(\d+)\s+(\d+)\s+(\d+)\s+; (?:[A-Za-z_]+ )?"([A-Za-z_][A-Za-z0-9_]*)"')
SRC  = re.compile(r'/root/project/Temp/_BuildAssets_/Lua/Lua/[0-9a-f]+')

def listing(p):
    return subprocess.run([LUAC,'-l','-l',p],capture_output=True,text=True).stdout

def split_blocks(txt):
    blocks, order, cur, buf = {}, [], None, []
    for line in txt.splitlines():
        m = HEAD.match(line)
        if m:
            if cur: blocks[cur] = '\n'.join(buf)
            cur = m.group(4); buf=[line]; order.append(cur)
        elif cur is not None:
            buf.append(line)
    if cur: blocks[cur] = '\n'.join(buf)
    return blocks, order

def name_map(blocks, order):
    m = {}
    for addr in order:
        regs = {}
        for line in blocks[addr].splitlines():
            c = CLO.search(line)
            if c:
                regs[c.group(1)] = c.group(2); continue
            s = SET.search(line)
            if s and s.group(4) in regs:
                m.setdefault(s.group(5), []).append(regs[s.group(4)])
    return m

def main():
    path = sys.argv[1]; want = sys.argv[2:]
    txt = listing(path)
    blocks, order = split_blocks(txt)
    nm = name_map(blocks, order)
    if not want or want == ['--list']:
        print('== 绑定名 %d ==' % len(nm)); print(' '.join(sorted(nm))); return
    for w in want:
        addrs = nm.get(w, [])
        if not addrs:
            pat = re.compile(r'; (?:[A-Za-z_]+ )?"%s"' % re.escape(w))
            addrs = [a for a in order if pat.search(blocks[a])]
        if not addrs:
            print('!! %s not found' % w); continue
        for a in addrs[:1]:
            print('===== %s @ %s =====' % (w, a))
            print(SRC.sub('MOD', blocks[a]))
main()
