#!/usr/bin/env python3
"""fg_so_swap.py — 外科替换 APK 内单个 .so（保持其余字节不变，重建尾部偏移）

用法:
  python3 fg_so_swap.py --apk 官方.apk --entry lib/arm64-v8a/libFairGuard.so \
      --so 新.so --out 输出.apk
原理:
  只重写目标 entry 的 local header+压缩数据；其余 entry 的本地数据原样搬移；
  再按累计位移修正中央目录里受影响 entry 的 relative offset 与目标 entry 的
  CRC/压缩后大小；EOCD 只改中央目录偏移。其余字节逐字节保留。
"""
import argparse, struct, zlib, sys

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--apk', required=True)
    ap.add_argument('--entry', required=True)
    ap.add_argument('--so', required=True)
    ap.add_argument('--out', required=True)
    a = ap.parse_args()

    d = open(a.apk, 'rb').read()
    new_so = open(a.so, 'rb').read()

    eocd = d.rfind(b'PK\x05\x06')
    if eocd < 0:
        sys.exit('no EOCD')
    n_ent, cd_size, cd_off = struct.unpack_from('<HII', d, eocd + 10)

    # walk central directory
    p = cd_off
    recs = []
    for _ in range(n_ent):
        if d[p:p + 4] != b'PK\x01\x02':
            sys.exit('bad CD sig at %#x' % p)
        nlen, elen, clen = struct.unpack_from('<HHH', d, p + 28)
        csize, crc = struct.unpack_from('<II', d, p + 20)[0], struct.unpack_from('<I', d, p + 16)[0]
        rel = struct.unpack_from('<I', d, p + 42)[0]
        name = d[p + 46:p + 46 + nlen].decode('utf-8')
        recs.append(dict(pos=p, nlen=nlen, elen=elen, clen=clen, csize=csize, crc=crc, rel=rel, name=name))
        p += 46 + nlen + elen + clen
    tgt = [r for r in recs if r['name'] == a.entry]
    if len(tgt) != 1:
        sys.exit('entry not found/duplicated')
    t = tgt[0]

    h = t['rel']
    nlen, elen = struct.unpack_from('<HH', d, h + 26)
    doff = h + 30 + nlen + elen
    old = d[doff:doff + t['csize']]
    if d[h + 8:h + 10] != b'\x08\x00':
        sys.exit('entry not deflated (method!=8)')
    raw_old = zlib.decompressobj(-15).decompress(old)
    if len(raw_old) != len(new_so):
        print('note: 未压缩长度变化 %d -> %d（允许，CD/偏移会自动修正）' % (len(raw_old), len(new_so)))

    co = zlib.compressobj(9, zlib.DEFLATED, -15, 9)
    new_c = co.compress(new_so) + co.flush()
    delta = len(new_c) - t['csize']
    crc = zlib.crc32(new_so) & 0xffffffff
    print('old_csize=%d new_csize=%d delta=%d crc=%08x' % (t['csize'], len(new_c), delta, crc))

    head = d[:h]
    lh = bytearray(d[h:h + 30 + nlen + elen])
    struct.pack_into('<I', lh, 14, crc)
    struct.pack_into('<I', lh, 18, len(new_c))
    struct.pack_into('<I', lh, 22, len(new_so))
    tail = d[doff + t['csize']:cd_off]          # 其余 entry 的本地数据原样
    new_cd_off = cd_off + delta

    cd = bytearray(d[cd_off:cd_off + cd_size])
    for r in recs:
        off = r['pos'] - cd_off
        if r['name'] == a.entry:
            struct.pack_into('<I', cd, off + 16, crc)
            struct.pack_into('<I', cd, off + 20, len(new_c))
            struct.pack_into('<I', cd, off + 24, len(new_so))
        if r['rel'] > h:
            struct.pack_into('<I', cd, off + 42, r['rel'] + delta)

    eo = bytearray(d[eocd:eocd + 22])
    struct.pack_into('<I', eo, 16, new_cd_off)
    out = bytes(head) + bytes(lh) + new_c + tail + bytes(cd) + bytes(eo)
    open(a.out, 'wb').write(out)
    print('written %s size=%d' % (a.out, len(out)))

if __name__ == '__main__':
    main()
