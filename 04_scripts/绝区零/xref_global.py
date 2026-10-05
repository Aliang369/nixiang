#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""xref_global.py — 在 so 的可执行段里找「目标全局地址」的引用点（ADRP + 页内偏移）。

用法:
  python xref_global.py SO 0x1d97fe38 [--window 8]
输出：命中指令地址 + 反汇编窗口（用于区分 ldr/ldrb/str 及目标寄存器）。
"""
import argparse
import struct

from capstone import CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN, Cs

ADRP_MASK = 0x9F000000
ADRP_VAL = 0x90000000


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
    ap.add_argument("target", type=lambda x: int(x, 0))
    ap.add_argument("--window", type=int, default=8)
    ap.add_argument("--max", type=int, default=400)
    a = ap.parse_args()

    d = open(a.so, "rb").read()
    want_page = a.target & ~0xFFF
    want_off = a.target & 0xFFF
    md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

    hits = []
    for p_off, p_vaddr, p_filesz in elf_exec_segments(d):
        seg = d[p_off:p_off + p_filesz]
        n = p_filesz // 4
        for i in range(n):
            (w,) = struct.unpack_from("<I", seg, i * 4)
            if (w & ADRP_MASK) != ADRP_VAL:
                continue
            rd = w & 0x1F
            immlo = (w >> 29) & 3
            immhi = (w >> 5) & 0x7FFFF
            imm = ((immhi << 2) | immlo) << 12
            pc = p_vaddr + i * 4
            tgt = (pc & ~0xFFF) + imm
            if tgt != want_page:
                continue
            hits.append(pc)

    try:
        import numpy as np
    except ImportError:
        np = None

    hits = []
    for p_off, p_vaddr, p_filesz in elf_exec_segments(d):
        seg = d[p_off:p_off + p_filesz]
        if np is not None:
            words = np.frombuffer(seg[: p_filesz // 4 * 4], dtype='<u4').astype(np.uint64)
            cand = np.nonzero((words & ADRP_MASK) == ADRP_VAL)[0]
            immlo = (words[cand] >> np.uint64(29)) & np.uint64(3)
            immhi = (words[cand] >> np.uint64(5)) & np.uint64(0x7FFFF)
            imm = ((immhi << np.uint64(2)) | immlo) << np.uint64(12)
            pc = (p_vaddr + cand * 4) & np.uint64(0xFFFFFFFFFFFFF000)
            tgt = pc + imm
            sel = cand[tgt == np.uint64(want_page)]
            hits.extend(int(p_vaddr + i * 4) for i in sel)
            continue
        n = p_filesz // 4
        for i in range(n):
            (w,) = struct.unpack_from("<I", seg, i * 4)
            if (w & ADRP_MASK) != ADRP_VAL:
                continue
            immlo = (w >> 29) & 3
            immhi = (w >> 5) & 0x7FFFF
            imm = ((immhi << 2) | immlo) << 12
            pc = p_vaddr + i * 4
            tgt = (pc & ~0xFFF) + imm
            if tgt != want_page:
                continue
            hits.append(pc)

    print("ADRP-to-page sites: %d" % len(hits))
    shown = 0
    for pc in hits:
        start = pc
        seg = None
        for p_off, p_vaddr, p_filesz in elf_exec_segments(d):
            if p_vaddr <= start < p_vaddr + p_filesz:
                seg = (p_off, p_vaddr, p_filesz)
                break
        if not seg:
            continue
        p_off, p_vaddr, p_filesz = seg
        fo = p_off + (start - p_vaddr)
        chunk = d[fo:fo + 4 * a.window]
        txt = []
        matched_off = False
        for ins in md.disasm(chunk, start):
            txt.append("    %#x  %-7s %s" % (ins.address, ins.mnemonic, ins.op_str))
            if f"#0x{want_off:x}]" in ins.op_str:
                matched_off = True
        if not matched_off:
            continue
        shown += 1
        print("== hit @%#x" % pc)
        print("\n".join(txt))
        if shown >= a.max:
            break
    print("shown: %d" % shown)


if __name__ == "__main__":
    main()
