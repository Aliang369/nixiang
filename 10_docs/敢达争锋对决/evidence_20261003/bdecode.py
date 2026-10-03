import capstone, sys
sys.stdout.reconfigure(encoding='utf-8')
md = capstone.Cs(capstone.CS_ARCH_ARM64, capstone.CS_MODE_LITTLE_ENDIAN)
md.detail = True
# guest base of libil2cpp (LOAD1 va=0) — 由 hook1 落点 0x4000213e76b4 反推
BASE = 0x400025000000
P = [
    ('hook1', 0x3d9cf54, 0xF81D0FFE, 0x161929D8),
    ('hook2', 0x3db99cc, 0xFF8303D1, 0x2EB71816),
    ('hook3', 0x67be180, 0xFE4FBFA9, 0xA0030016),
    ('hook4', 0x67be458, 0xE80F1EFC, 0xED020016),
]
for name, rva, disk, mem in P:
    va = BASE + rva
    print('%s RVA=0x%x VA=0x%x' % (name, rva, va))
    for tag, w in (('disk', disk), ('mem ', mem)):
        b = w.to_bytes(4, 'little')
        for ins in md.disasm(b, va):
            tgt = ''
            if ins.mnemonic == 'b' and ins.operands:
                tgt = '  -> 0x%x' % ins.operands[0].imm
            print('   %s %s\t%s%s' % (tag, ins.mnemonic, ins.op_str, tgt))
print()
print('stubs:')
for a, b in ((0x4000237bf000, bytes.fromhex('51e1fe9031321c9120021fd6')), (0x4000237bf00c, bytes.fromhex('51e1fe9031321d9120021fd6'))):
    for ins in md.disasm(b, a):
        print('   0x%x %s %s' % (ins.address, ins.mnemonic, ins.op_str))
