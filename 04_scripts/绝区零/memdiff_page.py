#!/usr/bin/env python3
"""对比一页运行时内存与 libil2cpp.so 文件字节，输出差分区间 + VA + 反汇编。

用法:
  python3 04_scripts/绝区零/memdiff_page.py --lib <libil2cpp.so> \
      --mem <page.bin> --offset 0xadfc000

--offset 是 /proc/<pid>/maps 里该段显示的 file offset（内存页对应文件位置）。
依赖: capstone（本机已安装）。
"""
import argparse
import mmap
import struct
import sys

try:
    from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM
except ImportError:
    print("ERROR: 需要 capstone（pip install capstone）", file=sys.stderr)
    sys.exit(2)


def parse_load_segments(mm):
    if mm[:4] != b"\x7fELF":
        raise SystemExit("ERROR: not an ELF file")
    e_phoff = struct.unpack_from("<Q", mm, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", mm, 0x36)[0]
    e_phnum = struct.unpack_from("<H", mm, 0x38)[0]
    segs = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type = struct.unpack_from("<I", mm, off)[0]
        if p_type != 1:  # PT_LOAD
            continue
        p_offset = struct.unpack_from("<Q", mm, off + 0x08)[0]
        p_vaddr = struct.unpack_from("<Q", mm, off + 0x10)[0]
        p_filesz = struct.unpack_from("<Q", mm, off + 0x20)[0]
        segs.append((p_offset, p_vaddr, p_filesz))
    return segs


def foff_to_va(segs, foff):
    for p_off, p_va, p_sz in segs:
        if p_off <= foff < p_off + p_sz:
            return p_va + (foff - p_off)
    return None


def diff_runs(ref, mem):
    runs = []
    i = 0
    n = min(len(ref), len(mem))
    while i < n:
        if ref[i] != mem[i]:
            j = i
            while j < n and ref[j] != mem[j]:
                j += 1
            runs.append((i, j))
            i = j
        else:
            i += 1
    return runs


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--lib", required=True)
    ap.add_argument("--mem", required=True)
    ap.add_argument("--offset", required=True, help="hex file offset of the page")
    ap.add_argument("--disasm", type=int, default=12, help="instructions before/after first diff")
    args = ap.parse_args()

    foff = int(args.offset, 0)
    mem = open(args.mem, "rb").read()
    with open(args.lib, "rb") as f:
        mm = mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ)
        ref = mm[foff:foff + len(mem)]
        segs = parse_load_segments(mm)

    va = foff_to_va(segs, foff)
    seg = next(((a, b, c) for a, b, c in segs if a <= foff < a + c), None)
    print(f"page file_off=0x{foff:x} size={len(mem)}")
    print(f"elf PT_LOAD: off=0x{seg[0]:x} va=0x{seg[1]:x} filesz=0x{seg[2]:x}" if seg else "no PT_LOAD?")
    print(f"page VA=0x{va:x}" if va is not None else "page VA=unknown")

    runs = diff_runs(ref, mem)
    print(f"diff runs: {len(runs)}  total diff bytes: {sum(b - a for a, b in runs)}")
    for a, b in runs[:24]:
        fa = foff_to_va(segs, foff + a)
        pa = foff_to_va(segs, foff + b - 1)
        print(f"  +0x{a:03x}..0x{b - 1:03x} ({b - a}B) VA 0x{fa:x}..0x{pa:x}")
        print(f"    file: {ref[a:b].hex()}")
        print(f"    mem : {mem[a:b].hex()}")
    if len(runs) > 24:
        print(f"  ... {len(runs) - 24} more runs")

    if runs and args.disasm:
        md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
        start = max(0, (runs[0][0] // 4) * 4 - args.disasm * 4)
        stop = min(len(mem), runs[-1][1] + args.disasm * 4)
        base_va = va if va is not None else 0
        print(f"--- disasm (mem) 0x{base_va + start:x}..0x{base_va + stop:x} ---")
        for insn in md.disasm(mem[start:stop], base_va + start):
            mark = "*" if ref[insn.address - base_va] != mem[insn.address - base_va] else " "
            print(f"{mark} 0x{insn.address:x}: {insn.mnemonic}\t{insn.op_str}")


if __name__ == "__main__":
    main()
