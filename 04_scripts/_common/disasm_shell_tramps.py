#!/usr/bin/env python3
"""反汇编斌哥壳 trampoline 区（libSDKloading r-xp 内存镜像）。

用法: disasm_shell_tramps.py SDK_RX_BIN SDK_RX_VA OUT_TXT [--sites VA:NAME,...]
输出: 每个 trampoline 起点的完整反汇编 + BSS 数据引用汇总。
依赖: capstone
"""
import sys, argparse
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM
from capstone.arm64 import ARM64_OP_MEM

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('bin'); ap.add_argument('va', type=lambda x: int(x, 0))
    ap.add_argument('out')
    ap.add_argument('--sites', required=True,
                    help='逗号分隔 VA:NAME')
    ap.add_argument('--span', type=lambda x: int(x, 0), default=0x140)
    ap.add_argument('--bss-lo', type=lambda x: int(x, 0), default=0x782d700000)
    ap.add_argument('--bss-hi', type=lambda x: int(x, 0), default=0x782d7a0000)
    a = ap.parse_args()

    data = open(a.bin, 'rb').read()
    base = a.va
    md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
    md.detail = True
    sites = []
    for tok in a.sites.split(','):
        va, name = tok.split(':', 1)
        sites.append((int(va, 0), name))
    sites.sort()

    out = open(a.out, 'w')
    for va, name in sites:
        ins = list(md.disasm(data[va - base: va - base + a.span], va))
        out.write('\n===== trampoline 0x%08X  %s  (+0x%X)\n' % (va, name, va - base))
        refs = []
        last = {}
        for i in ins:
            if i.mnemonic in ('adrp',):
                last[i.operands[0].reg] = i.operands[1].imm
            elif i.mnemonic in ('add', 'ldr', 'ldrb', 'ldrh', 'ldrsw', 'str', 'strb', 'strh') and len(i.operands) >= 2:
                m = i.operands[1]
                if m.type == ARM64_OP_MEM and m.mem.base in last:
                    tgt = last[m.mem.base] + m.mem.disp
                    if a.bss_lo <= tgt < a.bss_hi:
                        refs.append((i.address, tgt, i.mnemonic, i.op_str))
            # rstrip：mnemonic 用 %-8s 填充 + op_str 为空（如 ret）会产生行尾空白，
            # 会让 archive_scoped_git.sh 的 git diff --cached --check 失败并中断提交。
            out.write(('  0x%08X  %-8s %s' % (i.address, i.mnemonic, i.op_str)).rstrip() + '\n')
        out.write('  -- BSS 引用:')
        if not refs:
            out.write(' (无)\n')
        else:
            out.write('\n')
            for ad, tgt, mn, ops in refs:
                out.write('     0x%08X -> 0x%08X  %s %s\n' % (ad, tgt, mn, ops))
    out.close()
    print('wrote', a.out)

if __name__ == '__main__':
    main()
