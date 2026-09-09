#!/usr/bin/env python3
"""Locate HybridCLR InterpFrameGroup entry trio in a stripped libil2cpp.so.

Structural identification (no old so needed):
  1. Candidate function starts: `str x30,[sp,#-N]!` / `sub sp,sp,#N` prologues.
  2. Execute = the huge interp switch loop (largest function in scan region).
  3. EnterFrameFromNative / EnterFrameFromInterpreter = adjacent functions whose
     extent reaches Execute, sizes ~0x100-0x2000, one heavily BL-referenced
     (native thunks -> FromNative), the other sparsely referenced (interp loop
     -> FromInterpreter).
  4. Emit candidates + BL ref counts; final eyeball via llvm-objdump windows.
"""
from __future__ import annotations

import argparse
import struct
import sys
from pathlib import Path

import numpy as np


def read_text_section(so: bytes):
    """Return (va_base, file_off, data) of the executable PT_LOAD."""
    e_phoff = struct.unpack_from("<Q", so, 32)[0]
    e_phentsize = struct.unpack_from("<H", so, 54)[0]
    e_phnum = struct.unpack_from("<H", so, 56)[0]
    execs = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type = struct.unpack_from("<I", so, off)[0]
        p_flags = struct.unpack_from("<I", so, off + 4)[0]
        if p_type != 1 or not (p_flags & 1):
            continue
        p_offset, p_vaddr = struct.unpack_from("<QQ", so, off + 8)
        p_filesz = struct.unpack_from("<Q", so, off + 32)[0]
        execs.append((p_offset, p_vaddr, p_filesz))
    if not execs:
        raise SystemExit("no executable PT_LOAD")
    # main .text = largest exec load
    off, va, sz = max(execs, key=lambda t: t[2])
    return va, off, so[off : off + sz], execs


def va_to_file(va: int, execs):
    for off, v, sz in execs:
        if v <= va < v + sz:
            return off + (va - v)
    return None


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("so", help="libil2cpp.so path")
    ap.add_argument("--region", default="0x2400000:0x2a00000",
                    help="VA region to scan for the trio (start:stop)")
    ap.add_argument("--top", type=int, default=12)
    args = ap.parse_args()

    so = Path(args.so).read_bytes()
    text_va, text_off, text, execs = read_text_section(so)
    words = np.frombuffer(text, dtype="<u4")
    n = len(words)
    print(f"text: va=0x{text_va:x} file_off=0x{text_off:x} size=0x{len(text):x} words={n}")

    reg_lo, reg_hi = (int(x, 16) for x in args.region.split(":"))

    # --- candidate function starts ------------------------------------
    w = words
    prologue_str = ((w & np.uint32(0xFFE00FFF)) == np.uint32(0xF8000FFE))  # str x30,[sp,#-N]!
    prologue_sub = ((w & np.uint32(0xFF8003FF)) == np.uint32(0xD10003FF))  # sub sp,sp,#N
    cand = np.flatnonzero(prologue_str | prologue_sub)
    cand_va = text_va + cand * 4
    inreg = (cand_va >= reg_lo) & (cand_va < reg_hi)
    cand_idx, cand_va = cand[inreg], cand_va[inreg]
    print(f"candidates in region: {len(cand_idx)}")

    # extents: to next candidate
    nxt = np.empty(len(cand_idx), dtype=np.int64)
    nxt[:-1] = cand_idx[1:] - cand_idx[:-1]
    nxt[-1] = n - cand_idx[-1]

    # --- BL reference graph --------------------------------------------
    bl = ((w >> np.uint32(26)) == np.uint32(0x25))
    bl_idx = np.flatnonzero(bl)
    bl_w = w[bl_idx]
    imm = (bl_w & np.uint32(0x03FFFFFF)).astype(np.int64)
    imm = np.where(imm & (1 << 25), imm - (1 << 26), imm) * 4
    targets = text_va + bl_idx * 4 + imm
    order = np.argsort(targets)
    t_sorted, src_sorted = targets[order], bl_idx[order]

    def refcount(target_va: int) -> int:
        lo = np.searchsorted(t_sorted, target_va, side="left")
        hi = np.searchsorted(t_sorted, target_va, side="right")
        return int(hi - lo)

    def refsrcs(target_va: int, k=6):
        lo = np.searchsorted(t_sorted, target_va, side="left")
        hi = np.searchsorted(t_sorted, target_va, side="right")
        return [hex(text_va + int(s) * 4) for s in src_sorted[lo:hi][:k]], int(hi - lo)

    # --- Execute = largest function in region --------------------------
    sizes = nxt * 4
    big_order = np.argsort(sizes)[::-1][:args.top]
    print("\n== largest functions in region ==")
    for i in big_order:
        srcs, rc = refsrcs(int(cand_va[i]))
        print(f"  va=0x{cand_va[i]:x} size=0x{sizes[i]:x} bl_refs={rc} first_srcs={srcs[:3]}")

    exec_i = big_order[0]
    exec_va = int(cand_va[exec_i])
    print(f"\nExecute guess: 0x{exec_va:x} (size 0x{sizes[exec_i]:x})")

    # --- adjacent pair before Execute ----------------------------------
    print("\n== adjacent predecessors of Execute ==")
    for j in range(max(0, exec_i - 4), exec_i):
        va, sz = int(cand_va[j]), int(sizes[j])
        if va + sz < reg_lo or sz > 0x4000:
            continue
        # does this function's extent reach Execute (adjacent)?
        adjacent = (va + sz) == exec_va
        rc = refcount(va)
        print(f"  va=0x{va:x} size=0x{sz:x} adjacent_to_exec={adjacent} bl_refs={rc}")
        if adjacent:
            # who calls it?
            srcs, rc2 = refsrcs(va)
            print(f"    refs={rc2} srcs={srcs}")

    # dump pair candidates with refs from inside Execute
    print("\n== BL targets from inside Execute (first 30) ==")
    lo = np.searchsorted(t_sorted, exec_va, side="left")
    hi = np.searchsorted(t_sorted, exec_va + int(sizes[exec_i]), side="right")
    seen = {}
    for t in t_sorted[lo:hi]:
        seen[int(t)] = seen.get(int(t), 0) + 1
    for t, c in list(seen.items())[:30]:
        f = va_to_file(t, execs)
        print(f"  bl -> 0x{t:x} x{c} file_off=0x{f:x}" if f else f"  bl -> 0x{t:x} x{c}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
