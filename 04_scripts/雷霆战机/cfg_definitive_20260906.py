#!/usr/bin/env python3
"""终极 CFG 构建：线性扫描建块地图 + br_graph 边 + b.cond 边 → 全 .text 控制流图。
输出：blocks.json（块→[指令]）、cfg_edges.json、以及任意地址→块的查询。
"""
import json
import struct
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
B = (BASE / "libtersafe2.so").read_bytes()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

INSNS = []
pos, code = 0, B[:0x55A32C]
while pos < len(code):
    got = False
    for ins in md.disasm(code[pos:], pos):
        INSNS.append((ins.address, ins.mnemonic, ins.op_str))
        pos += ins.size
        got = True
    if not got:
        pos += 4
N = len(INSNS)
addr_idx = {a: i for i, (a, m, o) in enumerate(INSNS)}

G = json.loads((BASE / "br_graph.json").read_text())
GRAPH = {int(k, 16): v for k, v in G.items()}

def tgt_of(o):
    try:
        return int(o.replace("#", ""), 16)
    except ValueError:
        return None

# 1) 线性扫描分块
blocks = []  # (start_idx, end_idx_exclusive, terminator_kind, terminator_addr)
start_i = 0
TERMINATORS = {"br", "ret", "b"}
COND_B = ("b.eq", "b.ne", "b.lt", "b.le", "b.gt", "b.ge", "b.hs", "b.ls", "b.hi", "b.lo",
          "b.ge", "b.gt", "cbz", "cbnz", "tbz", "tbnz")
for i, (a, m, o) in enumerate(INSNS):
    if m in TERMINATORS:
        blocks.append((start_i, i + 1, m, a))
        start_i = i + 1
    elif m in COND_B:
        blocks.append((start_i, i + 1, m, a))
        start_i = i + 1
if start_i < N:
    blocks.append((start_i, N, "eof", None))
print(f"块数 {len(blocks)}", file=sys.stderr)

block_of_addr = {}
block_starts = set()
for bi, (s, e, term, ta) in enumerate(blocks):
    bs = INSNS[s][0]
    block_starts.add(bs)
    for j in range(s, e):
        block_of_addr[INSNS[j][0]] = bi

# 2) 边
edges = {}  # block_start -> list[(target, kind)]
for bi, (s, e, term, ta) in enumerate(blocks):
    bs = INSNS[s][0]
    lst = []
    if term in TERMINATORS:
        if term == "br":
            g = GRAPH.get(ta)
            if g:
                for t in g["targets"]:
                    lst.append((int(t, 16), f"br:{g['cond']}:{g['csel']}"))
            else:
                lst.append((None, "br:未解析"))
        elif term == "ret":
            lst.append((None, "ret"))
        else:  # b
            t = tgt_of(INSNS[e-1][2])
            lst.append((t, "b"))
    elif term in COND_B:
        t = tgt_of(INSNS[e-1][2])
        lst.append((t, term))
        if e < N:
            lst.append((INSNS[e][0], "fallthrough"))
    if e < N and term not in ("ret",):
        # br/ret/b 不顺落
        if term in COND_B:
            pass  # 已加 fallthrough
    edges[bs] = lst

# 3) 查询函数：地址 → 块
def query_block(addr):
    bi = block_of_addr.get(addr)
    if bi is None:
        return None
    s, e, term, ta = blocks[bi]
    return {
        "start": INSNS[s][0], "end_term": ta, "term": term,
        "text": "\n".join(f"  {INSNS[j][0]:08X}  {INSNS[j][1]:<8} {INSNS[j][2]}" for j in range(s, e)),
        "edges": edges.get(INSNS[s][0], []),
    }

def walk(start_addr, max_blocks=500):
    out = []
    vis = set()
    queue = [start_addr]
    while queue and len(vis) < max_blocks:
        cur = queue.pop(0)
        if cur in vis:
            continue
        vis.add(cur)
        q = query_block(cur)
        if q is None:
            out.append(f"\n──── {cur:X} ──── <无块>")
            continue
        enote = " | ".join(f"{t:X}({k})" if t else f"({k})" for t, k in q["edges"])
        out.append(f"\n──── {q['start']:X} .. {q['end_term'] if q['end_term'] else '?':X} [{q['term']}] ──── ⟹ {enote}")
        out.append(q["text"])
        for t, k in q["edges"]:
            if t and t not in vis and t >= 0xA9AF0 and t < 0x43113C:
                queue.append(t)
    return "\n".join(out), vis

if __name__ == "__main__":
    for a in sys.argv[1:]:
        addr = int(a, 16)
        txt, vis = walk(addr)
        outp = BASE / f"blk_{addr:X}.txt"
        outp.write_text(txt)
        print(f"{addr:#x}: 覆盖 {len(vis)} 块 → {outp.name}", file=sys.stderr)
