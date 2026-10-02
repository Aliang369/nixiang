"""Step 5: full UnityFS decode -> node list -> carve Assembly-CSharp.dll."""
import struct, hashlib, os, sys
P = r'E:\nixiang\06_dumped\冒险与征途\aot\hot1.unity3d'
OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
b = open(P, 'rb').read()

HDR_BASE = 0x0D          # real UnityFS header start (13-byte junk prefix before it)
BI_OFF = 0x4D            # LZ4 blocksInfo stream start
CBS, UBS = 862, 1811


def lz4_dec(src, limit=64 * 1024 * 1024):
    out = bytearray()
    i, n = 0, len(src)
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
        if off <= 0 or st < 0:
            raise ValueError('bad offset %d' % off)
        if len(out) + ml > limit:
            raise ValueError('output limit')
        for k in range(ml):
            out.append(out[st + k])
    return bytes(out)


bi = lz4_dec(b[BI_OFF:BI_OFF + CBS])
assert len(bi) == UBS, len(bi)
print('blocksInfo decoded:', len(bi))

off = 0
hash16 = bi[off:off + 16]; off += 16
nblocks = struct.unpack_from('>I', bi, off)[0]; off += 4
print('hash    =', hash16.hex())
print('nblocks =', nblocks)
blocks = []
for i in range(nblocks):
    u, c, f = struct.unpack_from('>IIH', bi, off); off += 10
    blocks.append((u, c, f))
nnodes = struct.unpack_from('>I', bi, off)[0]; off += 4
print('nnodes  =', nnodes)
nodes = []
for i in range(nnodes):
    o, s, f = struct.unpack_from('>qII', bi, off); off += 16
    e = bi.index(b'\0', off)
    path = bi[off:e].decode('utf-8', 'replace')
    off = e + 1
    nodes.append((o, s, f, path))
print('blocksInfo consumed %d / %d bytes' % (off, len(bi)))

DATA_START = BI_OFF + CBS
print('data start = 0x%X' % DATA_START)

# verify block sizes sum
tot_u = sum(x[0] for x in blocks); tot_c = sum(x[1] for x in blocks)
print('sum uncompressed blocks =', tot_u, ' sum compressed =', tot_c, ' file rest =', len(b) - DATA_START)

# decompress blocks
data = bytearray()
pos = DATA_START
bstat = {}
for i, (u, c, f) in enumerate(blocks):
    cf = f & 0x3F
    blob = b[pos:pos + c]; pos += c
    try:
        if cf == 0:
            d = blob
        elif cf in (2, 3):
            d = lz4_dec(blob)
        elif cf == 1:
            import lzma
            d = lzma.decompress(blob)
        else:
            raise ValueError('codec %d' % cf)
        ok = (len(d) == u)
    except Exception as ex:
        d, ok = b'', 'ERR:%s' % ex
    bstat[cf] = bstat.get(cf, 0) + 1
    data += d
    if i < 5 or not ok:
        print('  blk%3d u=%-8d c=%-8d cf=%d -> %s %s' % (i, u, c, cf, len(d), 'ok' if ok is True else ok))
print('codec histogram:', bstat)
print('decompressed total =', len(data))

with open(os.path.join(OUT, '_bundle_data.bin'), 'wb') as fh:
    fh.write(data)
with open(os.path.join(OUT, '_nodes.txt'), 'w', encoding='utf-8') as fh:
    for o, s, f, p in nodes:
        fh.write('%12d %12d 0x%08X %s\n' % (o, s, f, p))
print('nodes dumped')
for o, s, f, p in nodes:
    print('   %10d %10d %s' % (o, s, p))
