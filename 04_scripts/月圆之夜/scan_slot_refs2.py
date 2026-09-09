#!/usr/bin/env python3.11
# 修正版: 正确跟踪 ADRP 基址寄存器, 匹配 ADD #disp 与 LDR [reg,#disp] 两种引用
import struct, re

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
DUMP = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/dump_out3/dump.cs"
d = open(SO, "rb").read()

TARGETS = {0x5139188: "yyzy{0}yyzy0", 0x511A5D0: "yyzy_{0}.log"}
DISPS = {va & 0xFFF: va for va in TARGETS}


def adrp_target(insn, pc):
    if (insn & 0x9F000000) != 0x90000000:
        return None
    rd = insn & 0x1F
    immlo = (insn >> 29) & 3
    immhi = (insn >> 5) & 0x7FFFF
    imm = (immhi << 2) | immlo
    if imm & (1 << 20):
        imm -= 1 << 21
    return rd, ((pc & ~0xFFF) + (imm << 12)) & 0xFFFFFFFFFFFFFFFF


def decode_add_imm(insn):
    # ADD (immediate) 64-bit: 1 0 0 100010 0 imm12 Rn Rd
    if (insn & 0xFF800000) != 0x91000000:
        return None
    imm12 = (insn >> 10) & 0xFFF
    rn = (insn >> 5) & 0x1F
    rd = insn & 0x1F
    return rd, rn, imm12


def decode_ldr_imm(insn):
    # LDR/STR unsigned imm: bit30=1 -> 64bit(F9..) scale 8; bit30=0 -> 32bit(B9..) scale 4
    if (insn & 0xFFC00000) == 0xF9400000:  # 64-bit LDR
        return ((insn >> 5) & 0x1F, ((insn >> 10) & 0xFFF) * 8)
    if (insn & 0xBF800000) == 0xB9400000:  # 32-bit LDR
        return ((insn >> 5) & 0x1F, ((insn >> 10) & 0xFFF) * 4)
    return None


# 限定在 IL2CPP 游戏代码区域 (排除元数据初始化器 0x14fcxxx 区):
# 扫整个文件, 但对每个 hit 反查上下文 10 条内是否有 bl 0x14fc964 判定为 init 区
cs_ranges = [(0x1000, len(d) - 4)]
hits = {va: [] for va in TARGETS}
base = {}  # pc -> (rd, page) 最近 adrp
WINDOW = 64
adrp_hist = []  # (pc, rd, page)
for pc in range(0, len(d) - 4, 4):
    insn = struct.unpack_from("<I", d, pc)[0]
    a = adrp_target(insn, pc)
    if a:
        adrp_hist.append((pc, a[0], a[1]))
        if len(adrp_hist) > 8:
            adrp_hist.pop(0)
        continue
    disp_val = None
    kind = None
    ad = decode_add_imm(insn)
    if ad:
        rd, rn, imm12 = ad
        disp_val = imm12
        kind = "add"
    else:
        li = decode_ldr_imm(insn)
        if li:
            rn, disp_val = li
            kind = "ldr"
    if disp_val in DISPS:
        # 找最近的同寄存器 adrp 页匹配
        rn = ad[1] if ad else li[0]
        for p, r, pg in reversed(adrp_hist):
            if r == rn and p >= pc - WINDOW:
                va = DISPS[disp_val]
                if pg == (va & ~0xFFF):
                    hits[va].append((pc, kind))
                break

for va, name in TARGETS.items():
    print(f"\n0x{va:X} {name}: {len(hits[va])} refs")
    for pc, kind in hits[va]:
        # 判定是否元数据初始化区: 附近有 bl 0x14fc964
        region = d[pc - 0x80: pc + 0x40]
        is_init = False
        for i in range(0, len(region) - 4, 4):
            ins = struct.unpack_from("<I", region, i)[0]
            if (ins & 0xFC000000) == 0x94000000:
                imm = ins & 0x3FFFFFF
                if imm & (1 << 25):
                    imm -= 1 << 26
                t = pc - 0x80 + i + imm * 4
                if t == 0x14FC964:
                    is_init = True
                    break
        print(f"   {kind}@0x{pc:X} {'[init区]' if is_init else '[代码]'}")
