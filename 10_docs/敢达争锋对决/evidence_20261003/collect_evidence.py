# -*- coding: utf-8 -*-
"""敢达争锋对决 斌哥壳抓点取证：handler/stub 反汇编 + flag 页槽位 + 字符串表
产物: evidence/handler_disasm.txt, evidence/shell_rw_strings.txt
"""
import capstone, struct, sys, re, os
sys.stdout.reconfigure(encoding='utf-8')

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)

md = capstone.Cs(capstone.CS_ARCH_ARM64, capstone.CS_MODE_LITTLE_ENDIAN)
md.detail = True

def dis(blob, va, limit=24):
    out = []
    for ins in md.disasm(blob, va):
        out.append((ins.address, ins.mnemonic, ins.op_str, ins.size))
        if len(out) >= limit:
            break
        if ins.mnemonic in ('ret',) or (ins.mnemonic in ('b', 'br') ) or \
           (ins.mnemonic in ('ldp',) and 'x29' in ins.op_str):
            pass
    return out

def fmt(rows, base):
    lines = []
    for addr, m, o, sz in rows:
        tgt = ''
        lines.append('  +0x%03x  0x%x  %-8s %s%s' % (addr - base, addr, m, o, tgt))
    return '\n'.join(lines)

report = []
report.append('== 1. shell handler 反汇编（源 shell_code.bin，VA 基址 0x40002111c000）==')
SHELL_BASE = 0x40002111c000
code = open(os.path.join(ROOT, 'shell_code.bin'), 'rb').read()
handlers = [
    ('H_CalcDamage   ', 0x2cb684, 0x4000213e7684),
    ('H_UpdateAI     ', 0x2cb6b4, 0x4000213e76b4),
    ('H_get_deltaTime', 0x2cb70c, 0x4000213e770c),
    ('H_set_timeScale', 0x2cb74c, 0x4000213e774c),
]
for name, off, va in handlers:
    assert SHELL_BASE + off == va, (hex(SHELL_BASE + off), hex(va))
    # 反汇编到下一个 handler 起点或 0x80 字节
    nxt = min([o for _, o, _ in handlers if o > off] + [off + 0x80])
    blob = code[off:nxt]
    rows = dis(blob, va, 32)
    size = rows[-1][0] + rows[-1][3] - va
    report.append('\n%s va=0x%x file_off_in_shell=0x%x size=%d(0x%x)' % (name, va, off, size, size))
    report.append(fmt(rows, va))
    # 语义注解
    for addr, m, o, sz in rows:
        pass

report.append('\n== 2. B-stub 页 0x4000237bf000（live dump stub237bf000.bin，4096B）==')
stub = open(os.path.join(HERE, 'stub237bf000.bin'), 'rb').read()
SB = 0x4000237bf000
for entry in (0x0, 0xc):
    rows = dis(stub[entry:entry + 0x10], SB + entry, 6)
    tgt = None
    for addr, m, o, sz in rows:
        if m == 'br' and 'x17' in o:
            pass
    report.append('stub[%d] @0x%x' % (entry // 0xc, SB + entry))
    report.append(fmt(rows, SB))
# 解析 adrp+add 求目标
for entry in (0x0, 0xc):
    rows = dis(stub[entry:entry + 0x10], SB + entry, 6)
    page = None; addoff = 0
    for addr, m, o, sz in rows:
        if m == 'adrp':
            page = int(o.split('#')[-1], 16)
        if m == 'add' and 'x17' in o:
            addoff = int(o.split('#')[-1], 16)
    if page is not None:
        report.append('  stub[0x%x] -> 0x%x' % (entry, page + addoff))

report.append('\n== 3. flag 页 0x4000227b8000（live dump flagpage_227b8000.bin，233472B）==')
fp = open(os.path.join(HERE, 'flagpage_227b8000.bin'), 'rb').read()
def u32(o): return struct.unpack_from('<I', fp, o)[0]
def u64(o): return struct.unpack_from('<Q', fp, o)[0]
report.append('  +0x0000 u32 = %d (menu 总开关/状态?)' % u32(0x0))
report.append('  +0x0004 u32 = %d (伤害加值 int)' % u32(0x4))
report.append('  +0x0008 u32 = %d (加速倍率 int)' % u32(0x8))
report.append('  +0x232c u8  = %d (超级定怪 开关字节)' % fp[0x232c])
for slot, note in ((0x390, 'CalcDamage 原函数跳板'), (0x398, 'UpdateAI 原函数跳板'),
                   (0x3a0, 'Time.set_timeScale 原函数跳板'), (0x3a8, 'Time.get_deltaTime 原函数跳板')):
    report.append('  +0x%03x qword = 0x%x  (%s)' % (slot, u64(slot), note))

report.append('\n== 4. flag 页字符串表（>=6 可打印，UTF-8/ASCII）==')
strs = []
for m in re.finditer(rb'[\x20-\x7e]{6,}', fp):
    strs.append((m.start(), m.group().decode('ascii')))
outp = os.path.join(HERE, 'shell_rw_strings.txt')
with open(outp, 'w', encoding='utf-8') as f:
    for off, s in strs:
        f.write('+0x%06x  %s\n' % (off, s))
report.append('  抽到 %d 条 ASCII 串 -> evidence/shell_rw_strings.txt' % len(strs))
report.append('  --- 0x24000-0x26000 区间（菜单/hook 描述区）---')
for off, s in strs:
    if 0x24000 <= off < 0x26000:
        report.append('  +0x%06x  %s' % (off, s))

txt = '\n'.join(report)
open(os.path.join(HERE, 'handler_disasm.txt'), 'w', encoding='utf-8').write(txt + '\n')
print(txt)
