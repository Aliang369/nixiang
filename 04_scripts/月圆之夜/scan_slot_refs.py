#!/usr/bin/env python3.11
# 全 so 扫描: ADRP(page=0x5139000) + 后续指令用 disp=0x188 的引用点 (密码槽 0x5139188)
# 同时扫日志槽 0x511A5D0 (yyzy_{0}.log) 作对照
import struct, bisect

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
d = open(SO, "rb").read()

# 代码范围: 用 PT_LOAD 的可执行段近似, 直接扫整个文件对齐区(文件=VA)
TARGETS = {0x5139188: "yyzy{0}yyzy0", 0x511A5D0: "yyzy_{0}.log", 0x513DB20: "yyzy_hotfix"}


def adrp_target(insn, pc):
    # ADRP: 1[1]0010000 immlo(2) immhi(19) Rd(5)  -> 1x0 1000000
    if (insn & 0x9F000000) != 0x90000000:
        return None
    rd = insn & 0x1F
    immlo = (insn >> 29) & 3
    immhi = (insn >> 5) & 0x7FFFF
    imm = (immhi << 2) | immlo
    if imm & (1 << 20):
        imm -= 1 << 21
    page = ((pc & ~0xFFF) + (imm << 12)) & 0xFFFFFFFFFFFFFFFF
    return rd, page


def is_ldr_disp(insn):
    # LDR (imm, unsigned) 64bit: 11 111 0 01 01 imm12 Rn Rt ; 32bit: 10 111...
    if (insn & 0xBF800000) != 0xB9400000 and (insn & 0xFFC00000) != 0xF9400000:
        return None
    disp = ((insn >> 10) & 0xFFF) * 4 if (insn & 0x40000000) else ((insn >> 10) & 0xFFF) * 8
    rn = (insn >> 5) & 0x1F
    return rn, disp


hits = {va: [] for va in TARGETS}
# 每个字长 4B 对齐扫描, 记录 ADRP, 只向前看 16 条指令窗口
N = len(d)
adrp_live = []  # (pc, rd, page)
STEP = 4
for pc in range(0, N - 4, STEP):
    insn = struct.unpack_from("<I", d, pc)[0]
    a = adrp_target(insn, pc)
    if a:
        rd, page = a
        # 只关心指向相关页的 adrp
        for va, name in TARGETS.items():
            if (va & ~0xFFF) == page:
                adrp_live.append((pc, rd, page, name))
        continue
    # 检查 ldr/add disp
    if adrp_live:
        lr = is_ldr_disp(insn)
        if lr:
            rn, disp = lr
            for va, name in TARGETS.items():
                if disp == (va & 0xFFF):
                    for j in range(len(adrp_live) - 1, -1, -1):
                        p, r, pg, nm = adrp_live[j]
                        if r == rn and (va & ~0xFFF) == pg and p >= pc - 64:
                            hits[va].append(pc)
                            break

for va, name in TARGETS.items():
    print(f"0x{va:X} {name}: {len(hits[va])} hits")
    for pc in hits[va][:20]:
        print(f"   ref@0x{pc:X}")
