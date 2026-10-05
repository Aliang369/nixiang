#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""xref_bl.py — 统计 so 里对某个目标地址的 BL 调用点（ARM64）。

用法:
  python xref_bl.py SO 0x19f51f68 [--max 50]
"""
import argparse
import struct

BL_MASK = 0xFC000000
BL_VAL = 0x94000000


def elf_exec_segments(d):
    e_phoff = struct.unpack_from("<Q", d, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", d, 0x36)[0]
    e_phnum = struct.unpack_from("<H", d, 0x38)[0]
    segs = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type, p_flags = struct.unpack_from("<II", d, off)
        p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align = struct.unpack_from(
            "<QQQQQQ", d, off + 8)
        if p_type == 1 and (p_flags & 1) and p_filesz:
            segs.append((p_offset, p_vaddr, p_filesz))
    return segs


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("so")
    ap.add_argument("targets", nargs="+", type=lambda x: int(x, 0))
    ap.add_argument("--max", type=int, default=40)
    a = ap.parse_args()

    d = open(a.so, "rb").read()
    segs = elf_exec_segments(d)
    wanted = set(a.targets)
    hits = {t: [] for t in a.targets}

    for p_off, p_vaddr, p_filesz in segs:
        seg = d[p_off:p_off + p_filesz]
        n = p_filesz // 4
        for i in range(n):
            (w,) = struct.unpack_from("<I", seg, i * 4)
            if (w & BL_MASK) != BL_VAL:
                continue
            imm = w & 0x03FFFFFF
            if imm & 0x02000000:
                imm -= 0x04000000
            pc = p_vaddr + i * 4
            tgt = pc + imm * 4
            if tgt in wanted:
                hits[tgt].append(pc)

    for t in a.targets:
        h = hits[t]
        print("target %#x  callers=%d" % (t, len(h)))
        for pc in h[:a.max]:
            print("    bl from %#x" % pc)


if __name__ == "__main__":
    main()
