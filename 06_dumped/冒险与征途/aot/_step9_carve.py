"""Step 9: carve Assembly-CSharp.dll from decompressed bundle data, validate PE + .NET metadata."""
import struct, hashlib, os, re
OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
d = open(os.path.join(OUT, '_bundle_data.bin'), 'rb').read()

# locate the directory entry "Assembly-CSharp.dll"
pos = d.index(b'Assembly-CSharp.dll\x00')
print('path string at 0xA34 =', hex(pos))
# entry layout (observed): [.. 12 bytes ..] u32 pathlen, path, null, u32 size, <data>
size_field_off = pos + 19 + 1          # skip "Assembly-CSharp.dll" + null
dll_off = size_field_off + 4
size_field = struct.unpack_from('<I', d, size_field_off)[0]
print('size field @0x%X = %d' % (size_field_off, size_field))
print('dll_off = 0x%X  file starts: %r' % (dll_off, d[dll_off:dll_off+2]))

# --- PE parse ---
mz = dll_off
pe = mz + struct.unpack_from('<I', d, mz + 0x3C)[0]
assert d[pe:pe+4] == b'PE\0\0'
coff = struct.unpack_from('<H', d, pe + 4)[0]
mach = struct.unpack_from('<H', d, pe + 4)[0]
nsec = struct.unpack_from('<H', d, pe + 6)[0]
ts = struct.unpack_from('<I', d, pe + 8)[0]
opt_size = struct.unpack_from('<H', d, pe + 20)[0]
chars = struct.unpack_from('<H', d, pe + 22)[0]
opt = pe + 24
magic = struct.unpack_from('<H', d, opt)[0]
print('DOS e_lfanew=0x%X  PE@0x%X  machine=0x%X sections=%d timestamp=%d optmagic=0x%X' % (pe - mz, pe, coff, nsec, ts, magic))
size_img = struct.unpack_from('<I', d, opt + 56)[0]
size_hdr = struct.unpack_from('<I', d, opt + 60)[0]
checksum = struct.unpack_from('<I', d, opt + 64)[0]
nrva = struct.unpack_from('<I', d, opt + 92)[0]
print('SizeOfImage=%d SizeOfHeaders=%d CheckSum=0x%X NumRvaAndSizes=%d' % (size_img, size_hdr, checksum, nrva))
sects = []
raw_end = 0
print('sections:')
for i in range(nsec):
    s = opt + opt_size + i * 40
    name = d[s:s+8].rstrip(b'\0').decode('latin-1')
    vsize, vaddr, rsize, raddr = struct.unpack_from('<IIII', d, s + 8)
    sects.append((name, vsize, vaddr, rsize, raddr))
    raw_end = max(raw_end, raddr + rsize)
    print('   %-8s vsize=0x%-8X vaddr=0x%-8X rawsize=0x%-8X rawaddr=0x%-8X' % (name, vsize, vaddr, rsize, raddr))
total = max(raw_end, size_hdr)
print('computed raw file size = %d (size field said %d)' % (total, size_field))

# PE must sit at a 0x200-aligned boundary relative to the DLL start
clen = size_field if size_field else total
dll = d[dll_off:dll_off + clen]
# self-check: every section raw range inside, and section raw offs 0x200-aligned
ok = True
for name, vsize, vaddr, rsize, raddr in sects:
    if raddr + rsize > len(dll):
        print('  !! section %s exceeds file' % name); ok = False
print('all sections inside file:', ok)

# .NET metadata
clr = None
for i in range(nrva):
    rva, sz = struct.unpack_from('<II', d, opt + 96 + i * 8)
    if i == 14:
        clr = (rva, sz)
print('CLR header dir (index14):', clr)
if clr and clr[0]:
    def r2o(rva):
        for name, vsize, vaddr, rsize, raddr in sects:
            if vaddr <= rva < vaddr + max(vsize, rsize):
                return raddr + (rva - vaddr)
        return None
    co = r2o(clr[0])
    cbsz, cbmaj, cbmin, mrva, msz = struct.unpack_from('<IHHII', dll, co)
    print('COR20: cb=0x%X runtime=%d.%d MetaDataRVA=0x%X MetaDataSize=0x%X' % (cbsz, cbmaj, cbmin, mrva, msz))
    mo = r2o(mrva)
    print('metadata offset in dll = 0x%X ; bytes = %r' % (mo, dll[mo:mo+4]))
    assert dll[mo:mo+4] == b'BSJB'
    verlen = struct.unpack_from('<I', dll, mo + 12)[0]
    ver = dll[mo+16:mo+16+verlen].rstrip(b'\0').decode('latin-1')
    flags_, nstreams = struct.unpack_from('<HH', dll, mo + 16 + ((verlen + 3)//4)*4)
    print('metadata version=%r streams=%d' % (ver, nstreams))
    sp = mo + 16 + ((verlen + 3)//4)*4 + 4
    streams = {}
    for i in range(nstreams):
        so, ss = struct.unpack_from('<II', dll, sp); sp += 8
        e = dll.index(b'\0', sp)
        nm = dll[sp:e].decode('latin-1'); sp = e + 1
        sp = (sp + 3) & ~3
        streams[nm] = (so, ss)
        print('  stream %-10s off=0x%-8X size=%d' % (nm, so, ss))
    with open(os.path.join(OUT, 'Assembly-CSharp.dll'), 'wb') as f:
        f.write(dll)
    h = hashlib.sha256(dll).hexdigest()
    print()
    print('WROTE Assembly-CSharp.dll  size=%d  sha256=%s' % (len(dll), h))
    open(os.path.join(OUT, '_dll_sha256.txt'), 'w').write('%s  %d bytes\n' % (h, len(dll)))
