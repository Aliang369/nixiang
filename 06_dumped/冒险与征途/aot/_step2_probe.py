import sys, os, struct, re
P = r'E:\nixiang\06_dumped\冒险与征途\aot\hot1.unity3d'
b = open(P, 'rb').read()
print('filesize', len(b))
BASE = 13
print('prefix  =', b[:BASE].hex(), repr(b[:BASE]))

def cstr(off):
    e = b.index(b'\0', off)
    return b[off:e].decode('utf-8', 'replace'), e + 1

off = BASE
sig, off = cstr(off)
ver = struct.unpack_from('>I', b, off)[0]; off += 4
uv, off = cstr(off)
ur, off = cstr(off)
size = struct.unpack_from('>q', b, off)[0]; off += 8
cbs = struct.unpack_from('>I', b, off)[0]; off += 4
ubs = struct.unpack_from('>I', b, off)[0]; off += 4
fl = struct.unpack_from('>I', b, off)[0]; off += 4
print('sig=%r ver=%d uver=%r urev=%r size=%d cbs=%d ubs=%d flags=0x%X hdr_end=0x%X' % (sig, ver, uv, ur, size, cbs, ubs, fl, off))
print('size check: size+13 == filesize ?', size + 13 == len(b))

print()
print('--- BSJB occurrences ---')
for m in re.finditer(re.escape(b'BSJB'), b):
    o = m.start()
    try:
        verstr = b[o+12:o+12+16].decode('latin-1')
    except Exception:
        verstr = '?'
    print('  BSJB @0x%X  verstr=%r' % (o, verstr))

print()
print('--- MZ occurrences (first 40) ---')
n = 0
for m in re.finditer(re.escape(b'MZ'), b):
    o = m.start()
    if o + 0x40 > len(b):
        continue
    peoff = struct.unpack_from('<I', b, o + 0x3C)[0]
    ok = (0 < peoff < 0x400) and (o + peoff + 4 <= len(b)) and b[o+peoff:o+peoff+4] == b'PE\0\0'
    if ok:
        mach = struct.unpack_from('<H', b, o + peoff + 4)[0]
        print('  MZ @0x%X VALID PE machine=0x%X' % (o, mach))
    n += 1
    if n <= 40:
        print('  MZ @0x%X peoff=0x%X valid=%s' % (o, peoff, ok))
print('total MZ =', n)
