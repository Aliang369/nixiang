import struct
P = r'E:\nixiang\06_dumped\冒险与征途\aot\hot1.unity3d'
b = open(P, 'rb').read()
CBS, UBS, HDR_END = 862, 1811, 0x3F


def lz4_dec(src, max_out=None, prefix=b''):
    """LZ4 block decompressor. prefix = history bytes available before src (for warm start)."""
    out = bytearray(prefix)
    base = len(prefix)
    i = 0
    n = len(src)
    while i < n:
        token = src[i]; i += 1
        litlen = token >> 4
        if litlen == 15:
            while True:
                if i >= n: raise ValueError('eof-litlen')
                c = src[i]; i += 1
                litlen += c
                if c != 255: break
        if i + litlen > n:
            raise ValueError('eof-literal')
        out += src[i:i + litlen]
        i += litlen
        if i >= n:
            break
        if i + 2 > n: raise ValueError('eof-offset')
        off = src[i] | (src[i + 1] << 8); i += 2
        mlen = token & 0xF
        if mlen == 15:
            while True:
                if i >= n: raise ValueError('eof-mlen')
                c = src[i]; i += 1
                mlen += c
                if c != 255: break
        mlen += 4
        start = len(out) - off
        if off == 0 or start < 0:
            raise ValueError('bad-offset %d at out=%d' % (off, len(out)))
        if max_out and len(out) + mlen > max_out:
            raise ValueError('overflow')
        for k in range(mlen):
            out.append(out[start + k])
    return bytes(out[base:])


print('=== brute force LZ4 start offsets for blocksInfo ===')
for s in range(0x39, 0x90):
    chunk = b[s:s + CBS]
    for mode, pref in (('cold', b''), ('warm', b[s - 0x00:s])):
        pass
    try:
        r = lz4_dec(chunk)
        tag = 'OK len=%d' % len(r)
        if b'Assembly' in r:
            tag += '  *** contains Assembly'
        if len(r) == UBS:
            tag += '  <== exact UBS match'
        print('  0x%X cold: %s  %r' % (s, tag, r[:24]))
    except Exception as e:
        print('  0x%X cold: ERR %s' % (s, e))

print()
print('=== raw region hex 0x39..0xA0 ===')
for i in range(0x30, 0xA0, 16):
    print('%08X  %-48s %s' % (i, ' '.join('%02x' % c for c in b[i:i+16]),
                              ''.join(chr(c) if 32 <= c < 127 else '.' for c in b[i:i+16])))
