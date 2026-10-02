import struct
P = r'E:\nixiang\06_dumped\冒险与征途\aot\hot1.unity3d'
b = open(P, 'rb').read()

def lz4_dec(src):
    out = bytearray(); i, n = 0, len(src)
    while i < n:
        token = src[i]; i += 1
        ll = token >> 4
        if ll == 15:
            while True:
                c = src[i]; i += 1; ll += c
                if c != 255: break
        out += src[i:i+ll]; i += ll
        if i >= n: break
        off = src[i] | (src[i+1] << 8); i += 2
        ml = token & 0xF
        if ml == 15:
            while True:
                c = src[i]; i += 1; ml += c
                if c != 255: break
        ml += 4
        st = len(out) - off
        if off <= 0 or st < 0: raise ValueError('bad-off %d' % off)
        for k in range(ml): out.append(out[st+k])
    return bytes(out)

bi = lz4_dec(b[0x4D:0x4D+862])
print('bi len', len(bi))
off = 16
nb = struct.unpack_from('>I', bi, off)[0]; off += 4
print('nblocks', nb, 'entry_end', 16+4+nb*10)
print('--- tail hex from 1740 ---')
for i in range(1740, len(bi), 16):
    ch = bi[i:i+16]
    print('%5d  %-48s %s' % (i, ' '.join('%02x'%c for c in ch), ''.join(chr(c) if 32<=c<127 else '.' for c in ch)))

# try candidate node counts
for cand in range(1740, 1790):
    v = struct.unpack_from('>I', bi, cand)[0]
    if 1 <= v <= 64:
        print('cand nnodes@%d = %d' % (cand, v))
