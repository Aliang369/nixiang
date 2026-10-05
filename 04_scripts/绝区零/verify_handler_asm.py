#!/usr/bin/env python
# 自检：Asm_ZzzMoveApply 裸汇编 handler 的两处 adrp/add 重定位是否落到真实地址
#   期望序列（handler 尾部）：str s8,[x0,#0xf8] / str s8,[x0,#0x100] / strb w8,[x0,#0x104]
#   用法：python 04_scripts/绝区零/verify_handler_asm.py [so]
import sys

SO = sys.argv[1] if len(sys.argv) > 1 else \
    r"E:\nixiang\mod菜单\starry-mod-menu\libs_zzz\arm64-v8a\libstarry_magic.so"

b = open(SO, 'rb').read()
# fmul s8,s8,s9 (0x1E290908) + str s8,[x0,#0xf8] (0xBD00F808)
pat = bytes.fromhex('0809291e08f800bd')
i = b.find(pat)
print('so            :', SO)
print('handler hit   :', hex(i) if i >= 0 else 'NOT FOUND')
if i < 0:
    sys.exit(1)

lo, hi = max(0, i - 48), min(len(b), i + 48)
seg = b[lo:hi]
print('bytes[%#x:%#x]: %s' % (lo, hi, seg.hex()))


def insns(buf, base):
    out = []
    for k in range(0, len(buf) - 3, 4):
        w = int.from_bytes(buf[k:k + 4], 'little')
        out.append((base + k, w))
    return out


def adrp_pages(addr, w):
    immhi = (w >> 5) & 0x7FFFF
    immlo = (w >> 29) & 0x3
    imm = (immhi << 2) | immlo
    if imm & (1 << 20):
        imm -= 1 << 21
    return (addr & ~0xFFF) + (imm << 12)


def adr_target(addr, w):
    # ADR 的 PC = 指令自身地址（不是页基址）；ADRP 才是页基址
    immhi = (w >> 5) & 0x7FFFF
    immlo = (w >> 29) & 0x3
    imm = (immhi << 2) | immlo
    if imm & (1 << 20):
        imm -= 1 << 21
    return addr + imm


print('--- decode window ---')
for addr, w in insns(seg, lo):
    note = ''
    if (w & 0x9F000000) == 0x90000000:
        note = 'ADRP x%d, page %#x' % (w & 0x1F, adrp_pages(addr, w))
    elif (w & 0x9F000000) == 0x10000000:
        note = 'ADR  x%d, %#x' % (w & 0x1F, adr_target(addr, w))
    elif (w & 0xFF800000) == 0x91000000:
        note = 'ADD  x%d, x%d, #%#x' % (w & 0x1F, (w >> 5) & 0x1F, (w >> 10) & 0xFFF)
    elif w == 0xD61F0200:
        note = 'BR   x16'
    print('  %#010x  %08x  %s' % (addr, w, note))
