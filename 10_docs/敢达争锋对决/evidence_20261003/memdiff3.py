import struct, sys, os, zipfile
sys.stdout.reconfigure(encoding='utf-8')
BASE = r'E:\nixiang\07_patched\敢达争锋对决'
mem = open(os.path.join(BASE, 'memdump2', 'il2_mem_image.bin'), 'rb').read()
with zipfile.ZipFile(os.path.join(BASE, 'apk', 'base.apk')) as z:
    so = z.read('lib/arm64-v8a/libil2cpp.so')
LOADS = [(0x0, 0x0, 0x26e4c44, 4), (0x26e4c44, 0x26e8c44, 0x43c1edc, 5), (0x6aa6b20, 0x6aaeb20, 0x462380, 6), (0x6f08ea0, 0x6f14ea0, 0x4b2288, 6)]

def f2v(f):
    for o, v, fs, fl in LOADS:
        if o <= f < o+fs: return v + (f-o)
    return None

def decode(w, addr):
    """very small arm64 decoder for hook patterns"""
    if (w & 0xFF000000) == 0x58000000:  # LDR Xt, literal
        rt = w & 0x1F
        imm = (w >> 5) & 0x7FFFF
        return 'LDR X%d, #0x%x' % (rt, imm*4)
    if (w & 0xFFFFFC1F) == 0xD61F0000:  # BR Xn
        return 'BR X%d' % ((w >> 5) & 0x1F)
    if (w & 0xFFFFFC1F) == 0xD63F0000:
        return 'BLR X%d' % ((w >> 5) & 0x1F)
    opc = (w >> 26) & 0x3F
    if opc == 0x05: return 'B %+#x' % (((w & 0x3FFFFFF) - (1 << 26) if w & (1 << 25) else (w & 0x3FFFFFF)) * 4)
    if opc == 0x25: return 'BL %+#x' % (((w & 0x3FFFFFF) - (1 << 26) if w & (1 << 25) else (w & 0x3FFFFFF)) * 4)
    if (w & 0x9F000000) == 0x90000000:  # ADRP
        return 'ADRP X%d, #%+#x' % (w & 0x1F, ((((w >> 29) & 3) | ((w >> 3) & 0x1FFFFC)) << 12))
    if (w & 0x7F800000) == 0x11000000:
        return 'ADD/SUB imm X%d' % (w & 0x1F)
    if (w & 0x9F000000) == 0x10000000:
        return 'ADR X%d' % (w & 0x1F)
    if (w & 0xFFFFFC1F) == 0xD65F0000:
        return 'RET'
    return ''

def diff_range(lo, hi, label, maxprint=60):
    regs = []
    i = lo
    while i + 4 <= hi:
        if so[i:i+4] != mem[i:i+4]:
            j = i
            while j + 4 <= hi and so[j:j+4] != mem[j:j+4]:
                j += 4
            regs.append((i, j))
            i = j
        else:
            i += 4
    print('\n===== %s file[0x%x,0x%x) diff regions=%d' % (label, lo, hi, len(regs)))
    for (a, b) in regs[:maxprint]:
        va = f2v(a)
        wf = struct.unpack_from('<I', so, a)[0]
        wm = struct.unpack_from('<I', mem, a)[0]
        s = '  off=0x%08x va=0x%08x len=%3d  file: %-28s  mem: %-28s' % (
            a, va, b-a, ('%08x %s' % (wf, decode(wf, va)))[:28], ('%08x %s' % (wm, decode(wm, va)))[:28])
        if b-a >= 8:
            wm2 = struct.unpack_from('<I', mem, a+4)[0]
            s += ' | 2nd=%s' % decode(wm2, va+4)
            if b - a >= 16:
                tgt = struct.unpack_from('<Q', mem, a+8)[0]
                s += ' tgt64=0x%x' % tgt
        print(s)
    return regs

r1 = diff_range(0, 0x26e4c44, 'LOAD1 (R)')
r2 = diff_range(0x26e4c44, 0x6aa6b20, 'LOAD2 (RX)')
