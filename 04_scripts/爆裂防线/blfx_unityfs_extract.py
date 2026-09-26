#!/usr/bin/env python3
# 爆裂防线 1.01.06 UnityFS 解包器（纯 Python，含手写 LZ4）
# 来源：2026-09-26 会话中为解 assets/assetbundle/hotfixdlls/android/scriptdll/scriptdll.ab 临时编写
#       （当时 PyPI 断网，装不了 UnityPy）；原版脚本曾在 /tmp/blfx_unityfs.py。
# 用法：python3 blfx_unityfs_extract.py <bundle 文件> [输出文件]

import struct, sys, os, json

def lz4_decompress(src, dest_size=None):
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
        off = src[i] | (src[i+1] << 8); i += 2
        ml = token & 0x0f
        if ml == 15:
            while True:
                b = src[i]; i += 1; ml += b
                if b != 255: break
        ml += 4
        start = len(out) - off
        if start < 0: raise ValueError('bad offset')
        for j in range(ml):
            out.append(out[start+j])
    return bytes(out)

def rdstr(d, o):
    e = d.index(b'\0', o); return d[o:e].decode('utf-8', 'replace'), e+1

def parse(path, strip=16, verbose=True):
    raw = open(path, 'rb').read()
    p = raw.find(b'UnityFS'); assert p >= 0
    d = raw[p:]; base = p
    o = 8
    ver = struct.unpack('>I', d[o:o+4])[0]; o += 4
    uv, o = rdstr(d, o); ur, o = rdstr(d, o)
    size = struct.unpack('>q', d[o:o+8])[0]; o += 8
    cbi = struct.unpack('>I', d[o:o+4])[0]; o += 4
    ubi = struct.unpack('>I', d[o:o+4])[0]; o += 4
    flags = struct.unpack('>I', d[o:o+4])[0]; o += 4
    comp = flags & 0x3f; dirend = bool(flags & 0x80); pad = bool(flags & 0x200)
    if verbose:
        print(f'[hdr] ver={ver} uv={uv} ur={ur} size={size} cbi={cbi} ubi={ubi} flags=0x{flags:x} comp={comp} dir_end={dirend} pad={pad}')
    if pad:
        o = (o + 15) & ~15
    if dirend:
        bi_raw = d[len(d)-cbi:]; data_off = o
    else:
        bi_raw = d[o:o+cbi]; data_off = (o + cbi + 15) & ~15 if pad else o + cbi
    if len(bi_raw) == ubi:
        bi = bi_raw
    elif comp == 0:
        bi = bi_raw
    elif comp == 2 or comp == 3:
        bi = lz4_decompress(bi_raw, ubi)
    elif comp == 1:
        import lzma; bi = lzma.LZMADecompressor().decompress(bi_raw)
    else:
        raise ValueError('comp %d' % comp)
    assert len(bi) == ubi, (len(bi), ubi)
    b = bi; q = 16  # hash128
    nb = struct.unpack('>I', b[q:q+4])[0]; q += 4
    blocks = []
    for _ in range(nb):
        usz, csz, bf = struct.unpack('>IIH', b[q:q+10]); q += 10
        blocks.append((usz, csz, bf))
    nn = struct.unpack('>I', b[q:q+4])[0]; q += 4
    nodes = []
    for _ in range(nn):
        off, sz, nf = struct.unpack('>qqI', b[q:q+20]); q += 20
        nm, q = rdstr(b, q)
        q = (q + 3) & ~3
        nodes.append((off, sz, nf, nm))
    if verbose:
        print(f'[info] blocks={nb} nodes={nn} data_off=0x{data_off:x}')
        for i,(usz,csz,bf) in enumerate(blocks): print(f'  block[{i}] u={usz} c={csz} flags=0x{bf:x}')
        for (off,sz,nf,nm) in nodes: print(f'  node off={off} size={sz} flags=0x{nf:x} name={nm}')
    # decompress data blocks
    chunks = []; pos = data_off
    for (usz, csz, bf) in blocks:
        c = d[pos:pos+csz]; pos += csz
        bcomp = bf & 0x3f
        if bcomp == 0: chunks.append(c)
        elif bcomp in (2,3): chunks.append(lz4_decompress(c, usz))
        elif bcomp == 1:
            import lzma; chunks.append(lzma.LZMADecompressor().decompress(c, usz))
        else: raise ValueError('block comp %d' % bcomp)
    data = b''.join(chunks)
    if verbose: print(f'[data] total={len(data)} (expect {size if size else "?"})')
    return data, nodes, dict(ver=ver, uv=uv, ur=ur, flags=flags)

if __name__ == '__main__':
    path = sys.argv[1]; outp = sys.argv[2] if len(sys.argv) > 2 else None
    data, nodes, meta = parse(path)
    if outp:
        open(outp, 'wb').write(data)
        print('wrote', outp, len(data))
