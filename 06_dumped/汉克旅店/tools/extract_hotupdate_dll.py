#!/usr/bin/env python3
"""从 YooAsset(UnityFS/LZ4HC) 离线包中取出明文热更 DLL。
用法: python3 extract_hotupdate_dll.py <bundle> <out.dll> [--dump-raw raw.bin]
背景: 汉克旅店 1.1.0 的热更程序集 = assets/yoo/DefaultPackage/951c5741e0f5ee1380228dd746768640.bundle
      内含 TextAsset 'Assets/GameRes/HotUpdateDlls/Assembly-CSharp.dll.bytes'（明文 PE）。
"""
import struct, sys

def lz4_block(src, out_size):
    out = bytearray(); i = 0; n = len(src)
    while i < n:
        token = src[i]; i += 1
        lit = token >> 4
        if lit == 15:
            while True:
                b = src[i]; i += 1; lit += b
                if b != 255: break
        out += src[i:i+lit]; i += lit
        if i >= n: break
        offset = src[i] | (src[i+1] << 8); i += 2
        ml = token & 0xF
        if ml == 15:
            while True:
                b = src[i]; i += 1; ml += b
                if b != 255: break
        ml += 4
        start = len(out) - offset
        for k in range(ml): out.append(out[start + k])
    if out_size and len(out) != out_size:
        raise ValueError('LZ4 size mismatch %d != %d' % (len(out), out_size))
    return bytes(out)

def cstr(buf, p):
    e = buf.index(0, p)
    return buf[p:e].decode('utf-8', 'replace'), e + 1

def main():
    bundle, out = sys.argv[1], sys.argv[2]
    f = open(bundle, 'rb').read()
    assert f[:8] == b'UnityFS' + bytes([0]), 'not UnityFS'
    p = 8
    ver = struct.unpack('>I', f[p:p+4])[0]; p += 4
    _, p = cstr(f, p); _, p = cstr(f, p)          # playerVersion, unityVersion
    p += 8                                        # total size int64 BE
    csize = struct.unpack('>I', f[p:p+4])[0]; p += 4
    usize = struct.unpack('>I', f[p:p+4])[0]; p += 4
    flags = struct.unpack('>I', f[p:p+4])[0]; p += 4
    comp = flags & 0x3F
    # 0x200 = blocksInfo 需 16B 对齐；本包 blocksInfo 紧跟头部并对齐到 16
    p = (p + 15) // 16 * 16
    bi = lz4_block(f[p:p+csize], usize) if comp in (2, 3) else f[p:p+csize]
    assert bi[16:20] or True
    q = 16
    nblk = struct.unpack('>I', bi[q:q+4])[0]; q += 4
    blocks = [struct.unpack('>IIH', bi[q+10*k:q+10*k+10]) for k in range(nblk)]
    data = (p + csize + 15) // 16 * 16
    buf = bytearray(); off = data
    for (u, c, fl) in blocks:
        chunk = f[off:off+c]; off += c
        buf += lz4_block(chunk, u) if (fl & 0x3F) in (2, 3) else chunk
    d = bytes(buf)
    if len(sys.argv) > 4 and sys.argv[3] == '--dump-raw':
        open(sys.argv[4], 'wb').write(d)
    i = d.find(b'MZ')
    assert i >= 0, 'no PE found'
    lf = struct.unpack('<I', d[i+0x3c:i+0x40])[0]
    assert d[i+lf:i+lf+4] == b'PE' + bytes([0, 0]), 'bad PE'
    nsec = struct.unpack('<H', d[i+lf+6:i+lf+8])[0]
    optsz = struct.unpack('<H', d[i+lf+20:i+lf+22])[0]
    sec = i + lf + 24 + optsz
    end = 0
    for k in range(nsec):
        s = d[sec+40*k:sec+40*(k+1)]
        rsize, rptr = struct.unpack('<I', s[16:20])[0], struct.unpack('<I', s[20:24])[0]
        end = max(end, rptr + rsize)
    open(out, 'wb').write(d[i:i+end])
    print('wrote %s  (%d bytes, PE offset %d)' % (out, end, i))

main()
