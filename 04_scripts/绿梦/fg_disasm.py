#!/usr/bin/env python3
"""fg_disasm.py — capstone 线性反汇编运行期镜像（只读）

用法: python3 fg_disasm.py <image.bin> [start_hex] [end_hex]
输出: 代码分布 / svc 桩 / bl 目标 / 解密循环候选 / 桩表调用者
"""
import sys, collections
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM

img = open(sys.argv[1], 'rb').read()
start = int(sys.argv[2], 16) if len(sys.argv) > 2 else 0
end = int(sys.argv[3], 16) if len(sys.argv) > 3 else min(len(img), 0x400000)
md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)

bl_t = collections.Counter()
svc = []
loops = []
blocks = collections.Counter()
insn_n = 0
CH = 0x1000

for base in range(start, end, CH):
    chunk = img[base:base + CH]
    off = 0
    win = []
    while off <= len(chunk) - 4:
        got = list(md.disasm(chunk[off:off + 4], base + off))
        if not got:
            off += 4
            win = []
            continue
        i = got[0]
        insn_n += 1
        blocks[i.address >> 16] += 1
        if i.mnemonic == 'svc':
            svc.append(i.address)
        elif i.mnemonic == 'bl':
            try: bl_t[i.operands[0].imm] += 1
            except Exception: pass
        win.append(i.mnemonic)
        if len(win) > 6: win.pop(0)
        if i.mnemonic == 'strb' and 'eor' in win and ('ldrb' in win or 'ldr' in win):
            loops.append(i.address)
        off += 4

print('instructions: %d' % insn_n)
print('svc sites: %d -> %s' % (len(svc), [hex(a) for a in svc[:24]]))
print('decrypt-loop candidates (ldr/ldrb+eor+strb): %d' % len(loops))
for a in loops[:12]: print('   @%#x' % a)
print('top bl targets:')
for t, c in bl_t.most_common(15):
    tag = ' <payload-stub-table>' if 0x35b000 <= t < 0x35c000 else ''
    print('   %#x x%d%s' % (t, c, tag))
print('bl -> payload stub table: %s' % sorted((t, c) for t, c in bl_t.items() if 0x35b000 <= t < 0x35c000))
print('code per 64K (top 8): %s' % [(hex(k << 16), v) for k, v in sorted(blocks.items(), key=lambda kv: -kv[1])[:8]])
print('covered 64K blocks: %d' % len(blocks))