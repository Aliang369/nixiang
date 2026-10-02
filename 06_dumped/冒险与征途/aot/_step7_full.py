"""Step 7: correct data_start, decompress all 173 blocks, parse inner directory + SerializedFile."""
import struct, hashlib, os
P = r'E:\nixiang\06_dumped\冒险与征途\aot\hot1.unity3d'
OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
b = open(P, 'rb').read()
BI_OFF, CBS, UBS, DATA_START = 0x4D, 862, 1811, 0x3AD


def lz4_dec(src, limit=64 << 20):
    out = bytearray(); i, n = 0, len(src)
    while i < n:
        token = src[i]; i += 1
        ll = token >> 4
        if ll == 15:
            while True:
                c = src[i]; i += 1; ll += c
                if c != 255: break
        out += src[i:i + ll]; i += ll
        if i >= n: break
        off = src[i] | (src[i + 1] << 8); i += 2
        ml = token & 0xF
        if ml == 15:
            while True:
                c = src[i]; i += 1; ml += c
                if c != 255: break
        ml += 4
        st = len(out) - off
        if off <= 0 or st < 0: raise ValueError('bad-off %d' % off)
        if len(out) + ml > limit: raise ValueError('limit')
        for k in range(ml): out.append(out[st + k])
    return bytes(out)


bi = lz4_dec(b[BI_OFF:BI_OFF + CBS])
assert len(bi) == UBS
o = 16
nblocks = struct.unpack_from('>I', bi, o)[0]; o += 4
blocks = [struct.unpack_from('>IIH', bi, o + i * 10) for i in range(nblocks)]
o += nblocks * 10
nnodes = struct.unpack_from('>I', bi, o)[0]; o += 4
nodes = []
for i in range(nnodes):
    f_off, f_size, f_flags = struct.unpack_from('>qII', bi, o); o += 16
    e = bi.index(b'\0', o); path = bi[o:e].decode(); o = e + 1
    nodes.append((f_off, f_size, f_flags, path))
print('nblocks=%d nnodes=%d nodes=%s' % (nblocks, nnodes, nodes))

data = bytearray()
pos = DATA_START
errs = 0
for i, (u, c, f) in enumerate(blocks):
    blob = b[pos:pos + c]; pos += c
    cf = f & 0x3F
    if cf == 0: d = blob
    elif cf in (2, 3): d = lz4_dec(blob)
    else: raise ValueError('codec %d' % cf)
    if len(d) != u:
        errs += 1
        if errs < 5: print('  blk%d size mismatch %d != %d' % (i, len(d), u))
    data += d
print('decompressed %d bytes, size-mismatch blocks: %d' % (len(data), errs))
open(os.path.join(OUT, '_bundle_data.bin'), 'wb').write(data)

print()
print('--- first 64 bytes of data ---')
print(data[:64].hex())
for i in range(0, 96, 16):
    ch = data[i:i + 16]
    print('%08X  %-48s %s' % (i, ' '.join('%02x' % c for c in ch),
                              ''.join(chr(c) if 32 <= c < 127 else '.' for c in ch)))

# SerializedFile header
print()
ver = struct.unpack_from('>I', data, 4)[0]
doff = struct.unpack_from('>I', data, 8)[0]
print('metadata size(=0 expected)=%d  format_version=%d  data_offset=%d' % (struct.unpack_from('>I', data, 0)[0], ver, doff))
