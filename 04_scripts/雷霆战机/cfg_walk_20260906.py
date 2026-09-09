#!/usr/bin/env python3
"""CFG 游走器：用 br_resolved2 的边遍历 checker 状态机，逐块输出可读反汇编。"""
import struct
import re
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
b = (BASE / "libtersafe2.so").read_bytes()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

insns = []
pos, code = 0, b[:0x55A32C]
while pos < len(code):
    got = False
    for ins in md.disasm(code[pos:], pos):
        insns.append((ins.address, ins.mnemonic, ins.op_str))
        pos += ins.size
        got = True
    if not got:
        pos += 4
addr_map = {a: i for i, (a, m, o) in enumerate(insns)}

# 解析 br_resolved2
edges = {}  # br_addr -> [(tgt, cond_note)]
for line in (BASE / "br_resolved2.txt").read_text().splitlines():
    m = re.match(r"^([0-9A-F]+) BR cond=(\w+) idx=(-?\d+)/(-?\d+) → ([0-9A-F?]+) \| ([0-9A-F?]+)", line)
    if m:
        br_a = int(m.group(1), 16)
        cond = m.group(2)
        t1, t2 = m.group(5), m.group(6)
        edges[br_a] = (cond, t1, t2)

def block_dump(start, visited):
    """从 start 反汇编到最近的 br（含），返回 (end_br, text)。"""
    i = addr_map.get(start)
    if i is None:
        return None, f"{start:X} <无指令>\n"
    lines = []
    while i < len(insns):
        a, m, o = insns[i]
        lines.append(f"  {a:08X}  {m:<8} {o}")
        if m == "br":
            return a, "\n".join(lines)
        if m == "ret":
            return a, "\n".join(lines)
        i += 1
    return None, "\n".join(lines)

def walk(entry, max_blocks=400):
    visited = set()
    queue = [entry]
    out = []
    while queue and len(visited) < max_blocks:
        cur = queue.pop(0)
        if cur in visited:
            continue
        visited.add(cur)
        endbr, text = block_dump(cur, visited)
        edge_note = ""
        if endbr and endbr in edges:
            cond, t1, t2 = edges[endbr]
            edge_note = f"  ==> cond={cond}: {t1} / {t2}"
            for t in (t1, t2):
                if t != "?" and int(t, 16) not in visited:
                    queue.append(int(t, 16))
        out.append(f"\n──── BLOCK {cur:X} ────{edge_note}")
        out.append(text)
    return "\n".join(out), visited

# 入口：checker 循环块的已知成员（perf 热点 + 解析到的目标）
entries = [0xBFA3C, 0xBF8D8, 0xC0174, 0xC036C, 0xC04A4, 0xC04AC, 0xC0444, 0xC04C4]
all_visited = set()
graph_txt = []
for e in entries:
    txt, vis = walk(e)
    newv = vis - all_visited
    all_visited |= vis
    graph_txt.append(f"\n\n########## 从 {e:X} 可达 {len(vis)} 块（新增 {len(newv)}）##########")
    graph_txt.append(txt)
(BASE / "checker_cfg.txt").write_text("\n".join(graph_txt))
print(f"遍历完成，覆盖 {len(all_visited)} 块 → checker_cfg.txt", file=sys.stderr)
