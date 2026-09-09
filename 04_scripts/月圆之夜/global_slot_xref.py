#!/usr/bin/env python3.11
# 全局 ADRP+ADD(+ADD)+LDR xref 扫描指定槽 -> 函数归属
import struct
import pickle
import re

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
META = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
DUMPCS = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/dump_out3/dump.cs"
d = open(SO, "rb").read()
md = open(META, "rb").read()
SL, LD, NLIT = 0x100, 0x228C8, 17657

def lit(i):
    if not (0 <= i < NLIT):
        return None
    a, b = struct.unpack_from("<ii", md, SL + i * 8)
    if a <= 0 or a > 4096:
        return None
    return md[LD + b: LD + b + a].decode("utf-8", "replace")

SLOTS = {0x5162108: 2429, 0x514ff68: 2430, 0x5118e08: 5562, 0x516d910: 5570,
         0x5140d80: 17197, 0x513db20: 17198, 0x511a5d0: 17199, 0x5139188: 17200}

# 函数边界: 从 dump.cs 收集所有 (RVA, 类名, 签名行)
funcs = []
cls = ""
for line in open(DUMPCS):
    ls = line.strip()
    if ls.startswith("// Namespace:"):
        cls = ""
    elif "class " in ls and "// TypeDefIndex:" in ls:
        cls = ls.split("// TypeDefIndex")[0].strip()
    m = re.match(r"// RVA: (0x[0-9A-Fa-f]+)", ls)
    if m:
        funcs.append((int(m.group(1), 16), cls, ls[8:].strip()))
funcs.sort()
starts = [f[0] for f in funcs]
import bisect
def owner(addr):
    i = bisect.bisect_right(starts, addr) - 1
    if i >= 0:
        return funcs[i]
    return None

# 反汇编整个 .text，跟踪 adrp+add+ldr
from capstone import *
from capstone.arm64 import ARM64_OP_IMM, ARM64_OP_MEM, ARM64_REG_X0, ARM64_REG_W0
cs = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
cs.detail = True

TEXT_LO, TEXT_HI = 0x100000, 0x5100000   # 代码段粗界（覆盖全部方法 RVA）
targets = set(SLOTS.keys())
hits = {}
# 扫描步进: 只反汇编命中槽地址附近? 不行——adrp 覆盖 4KB 页, 必须顺序流。
# 简化: 反汇编全部 .text 太慢; 改为块扫描——adrp 页 = slot & ~0xFFF, 目标 add 立即数已知。
# 每个 slot 的 (page, disp) 已知 -> 在 .text 中搜索指令模式:
#   adrp Xn, page  => 编码可枚举; 更简单: 扫所有 adrp, 若 imm==page 记录, 再看后续 0x10 条内 add/ldr。
# 为效率: 先线性 capstone 扫全部 .text 一次, 记录所有 adrp(imm==page) 位置及其后 16 条的 add/ldr 链。
pages = {s & ~0xFFF: [] for s in targets}
for s in targets:
    pages[s & ~0xFFF].append(s)

import time
t0 = time.time()
code_range = d[TEXT_LO:TEXT_HI]
adrp_hits = []
for i in cs.disasm(code_range, TEXT_LO):
    if i.mnemonic == "adrp":
        op = i.operands[1]
        tv = getattr(op, "imm", getattr(op, "value", 0))
        if isinstance(tv, bytes):
            tv = int.from_bytes(tv, "little")
        tv = int(tv) & 0xFFFFFFFFFFFFFFFF
        if tv in pages:
            adrp_hits.append((i.address, i.operands[0].reg, tv))
print("adrp 命中页: %d 处, 用时 %.1fs" % (len(adrp_hits), time.time() - t0))

# 二次扫描: 对每个 adrp 命中, 反汇编其后 24 条找 add #disp / ldr [Xn, #disp]
for addr, reg, page in adrp_hits:
    for j in cs.disasm(d[addr:addr + 24 * 8], addr):
        if j.address <= addr:
            continue
        if j.mnemonic == "add" and j.operands[1].type == ARM64_OP_IMM and j.operands[0].reg == reg and j.operands[1].reg == reg:
            va = page + j.operands[1].imm
            if va in targets:
                hits.setdefault(va, set()).add(j.address)
        elif j.mnemonic == "ldr" and j.operands[1].type == ARM64_OP_MEM and j.operands[1].mem.base == reg:
            va = page + j.operands[1].mem.disp
            if va in targets:
                hits.setdefault(va, set()).add(j.address)
                break
        if j.address > addr + 200:
            break

print("\n== 槽 -> 引用指令 -> 函数 ==")
for va in sorted(hits):
    idx = SLOTS[va]
    for ia in sorted(hits[va]):
        f = owner(ia)
        print("  slot 0x%X [%d] %r" % (va, idx, lit(idx)))
        print("     insn@0x%X  func=%s  rva=0x%X" % (ia, (f[1] if f else "?"), ia))
