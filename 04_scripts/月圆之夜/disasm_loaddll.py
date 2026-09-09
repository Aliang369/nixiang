#!/usr/bin/env python3.11
# 反汇编 LoadDLL.LoadHotfix / DecompressBytes, 解析 ADRP/LDR 常量与 BL 调用目标
import struct, re, sys
from capstone import *
from capstone.arm64 import ARM64_OP_MEM, ARM64_REG_X0

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
DUMP = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/dump_out3/dump.cs"
d = open(SO, "rb").read()

# 1) 从 dump.cs 建 RVA→(类.方法) 索引
rva2name = {}
cur_cls = None
for line in open(DUMP, encoding="utf-8", errors="replace"):
    ms = re.search(r"^(?:public |private |internal |protected |static |sealed |abstract |readonly |override |virtual |extern )*(?:class|struct|enum|interface) ([^\s:{]+)", line)
    if ms:
        cur_cls = ms.group(1)
        continue
    mr = re.search(r"// RVA: 0x([0-9A-Fa-f]+)", line)
    if mr:
        nm = line.strip().split(" // RVA")[0].strip()
        ns = ""
        rva = int(mr.group(1), 16)
        rva2name[rva] = f"{cur_cls}::{nm}"

FUNCS = {
    "LoadDLL.LoadHotfix": (0x1D9F134, 0x1D9F2C4),
    "LoadDLL.DecompressBytes": (0x1D9F454, 0x1D9F990),
    "LoadDLL.LoadAssemble": (0x1D9F990, 0x1DA0F00),
}
cs = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
cs.detail = True

for fn, (a, b) in FUNCS.items():
    if len(sys.argv) > 1 and sys.argv[1] not in fn:
        continue
    print(f"\n===== {fn}  0x{a:X}-0x{b:X} ({b-a} B) =====")
    adrp = {}
    for i in cs.disasm(d[a:b], a):
        note = ""
        ops = i.op_str
        m = i.mnemonic
        if m == "adrp":
            try:
                imm = i.operands[1].imm & 0xFFFFFFFFFFFFFFFF
                adrp[i.operands[0].reg] = imm
                note = f"; page=0x{imm:X}"
            except Exception:
                pass
        elif m in ("ldr", "ldrb", "ldrsb", "str") and i.operands[-1].type == ARM64_OP_MEM:
            base = i.operands[-1].mem.base
            if base in adrp:
                va = (adrp[base] + i.operands[-1].mem.disp) & 0xFFFFFFFFFFFFFFFF
                note = f"; @0x{va:X}"
                if va + 8 <= len(d):
                    q = struct.unpack_from("<Q", d, va)[0]
                    note += f" =0x{q:X}"
                    if q in rva2name:
                        note += f" ->{rva2name[q]}"
        elif m in ("bl", "b") and i.operands[0].type == 2:  # imm
            tgt = i.operands[0].imm
            if tgt in rva2name:
                note = f"; -> {rva2name[tgt]}"
            else:
                note = f"; -> 0x{tgt:X}"
        # movz/movk 常量拼接太啰嗦, 只标 movz 大数
        elif m == "movz" and i.operands[1].type == 2 and abs(i.operands[1].imm) > 0xFF:
            note = f"; imm=0x{i.operands[1].imm & 0xFFFF:X}"
        print(f"  0x{i.address:X}: {m:<8} {ops} {note}")
