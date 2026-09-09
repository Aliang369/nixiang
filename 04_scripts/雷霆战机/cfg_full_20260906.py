#!/usr/bin/env python3
"""全图重建 + 死亡走廊完整遍历：从 br_graph.json 构图，从指定块出发 BFS，逐块 dump，找 fault 触发。"""
import json
import re
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
B = (BASE / "libtersafe2.so").read_bytes()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
G = json.loads((BASE / "br_graph.json").read_text())
graph = {int(k, 16): v for k, v in G.items()}

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
addr_idx = {a: i for i, (a, m, o) in enumerate(INSNS)}

def block(start):
    """从块首反汇编到 br/ret，返回 (end, lines)。"""
    i = addr_idx.get(start)
    if i is None:
        return None, [f"  {start:X} <无指令>"]
    lines = []
    while i < len(INSNS):
        a, m, o = INSNS[i]
        lines.append(f"  {a:08X}  {m:<8} {o}")
        if m in ("br", "ret") or m.startswith("b.") or m == "b":
            return a, lines
        if m == "bl" and i + 1 < len(INSNS) and INSNS[i + 1][1] in ("br", "ret", "b", "b.") and False:
            pass
        i += 1
    return None, lines

def walk(entries, max_blocks=800, depth_arg=None):
    visited = []
    vis = set()
    queue = list(entries)
    out = []
    while queue and len(vis) < max_blocks:
        cur = queue.pop(0)
        if cur in vis:
            continue
        vis.add(cur)
        visited.append(cur)
        endbr, lines = block(cur)
        note = ""
        if endbr is not None and endbr in graph:
            g = graph[endbr]
            note = f"  ⟹ cond={g['cond']} → {', '.join(g['targets'])}"
            for t in g["targets"]:
                ti = int(t, 16)
                if ti not in vis:
                    queue.append(ti)
        elif endbr is None:
            note = "  (未解析边)"
        out.append(f"\n──── {cur:X} ────{note}")
        out.extend(lines)
    return "\n".join(out), vis, visited

if __name__ == "__main__":
    entries = [int(x, 16) for x in sys.argv[1:]]
    txt, vis, order = walk(entries)
    print(f"覆盖 {len(vis)} 块", file=sys.stderr)
    outp = BASE / ("walkfull_" + "_".join(f"{e:X}" for e in entries[:3]) + ".txt")
    outp.write_text(txt)
    print(outp, file=sys.stderr)
