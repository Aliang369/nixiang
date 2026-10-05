#!/usr/bin/env python3
"""Convert between ELF VA/RVA and file offset via PT_LOAD segments.

usage: va_conv.py <so> va 0x<addr> | va_conv.py <so> off 0x<offset>
"""
import sys

from elftools.elf.elffile import ELFFile

path = sys.argv[1]
mode = sys.argv[2]
x = int(sys.argv[3], 16)
elf = ELFFile(open(path, "rb"))
segs = [(ph["p_vaddr"], ph["p_offset"], ph["p_filesz"])
        for ph in elf.iter_segments() if ph["p_type"] == "PT_LOAD"]

if mode == "va":
    for v, o, sz in segs:
        if v <= x < v + sz:
            print(f"va {x:#x} -> file_off {o + (x - v):#x} (seg vaddr={v:#x} off={o:#x})")
            break
    else:
        print(f"va {x:#x} -> NOT IN PT_LOAD")
elif mode == "off":
    for v, o, sz in segs:
        if o <= x < o + sz:
            print(f"file_off {x:#x} -> va {v + (x - o):#x} (seg vaddr={v:#x} off={o:#x})")
            break
    else:
        print(f"file_off {x:#x} -> NOT IN PT_LOAD")
else:
    sys.exit("mode must be va|off")
