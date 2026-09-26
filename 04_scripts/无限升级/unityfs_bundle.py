#!/usr/bin/env python3
"""Minimal UnityFS bundle extractor (no UnityPy dependency).
Usage: unityfs.py <bundle> [--list] [--extract NAME OUT]
"""
import struct, sys, os

def read_cstr(b, o):
    e = b.index(b"\x00", o)
    return b[o:e].decode("utf-8", "replace"), e + 1

def lz4_decompress(src, dst_size):
    dst = bytearray()
    i, n = 0, len(src)
    while i < n:
        token = src[i]; i += 1
        lit = token >> 4
        if lit == 15:
            while True:
                b = src[i]; i += 1
                lit += b
                if b != 255: break
        if lit:
            dst += src[i:i+lit]; i += lit
        if i >= n: break
        off = src[i] | (src[i+1] << 8); i += 2
        mlen = token & 0x0F
        if mlen == 15:
            while True:
                b = src[i]; i += 1
                mlen += b
                if b != 255: break
        mlen += 4
        start = len(dst) - off
        if start < 0: raise ValueError("bad offset")
        for k in range(mlen):
            dst.append(dst[start + k])
    if dst_size and len(dst) != dst_size:
        print(f"  [warn] lz4 out {len(dst)} != expected {dst_size}")
    return bytes(dst)

def decompress(ctype, data, unc_size):
    if ctype == 0:
        return data[:unc_size] if unc_size else data
    if ctype in (2, 3):  # LZ4 / LZ4HC
        return lz4_decompress(data, unc_size)
    if ctype == 1:  # LZMA
        import lzma
        return lzma.decompress(data)
    raise ValueError(f"unsupported compression {ctype}")

def parse(path):
    b = open(path, "rb").read()
    o = 0
    sig, o = read_cstr(b, o)
    assert sig == "UnityFS", sig
    version = struct.unpack_from(">I", b, o)[0]; o += 4
    uver, o = read_cstr(b, o)
    urev, o = read_cstr(b, o)
    size = struct.unpack_from(">q", b, o)[0]; o += 8
    cbis = struct.unpack_from(">I", b, o)[0]; o += 4
    ubis = struct.unpack_from(">I", b, o)[0]; o += 4
    flags = struct.unpack_from(">I", b, o)[0]; o += 4
    ctype = flags & 0x3F
    print(f"version={version} unity={uver} rev={urev} size={size} cbis={cbis} ubis={ubis} flags=0x{flags:x} ctype={ctype}")
    if flags & 0x200:  # block info need padding at start
        o = (o + 15) & ~15
    bi = decompress(ctype, b[o:o+cbis], ubis)
    o += cbis
    print(f"blocksInfo raw at 0x{o-cbis:x} decompressed={len(bi)}")
    for hashlen in (16, 0):
        try:
            p = hashlen
            nblocks = struct.unpack_from(">I", bi, p)[0]; p += 4
            if not (0 < nblocks < 200): continue
            blocks = []
            ok = True
            for _ in range(nblocks):
                u, c, fl = struct.unpack_from(">IIH", bi, p); p += 10
                if u > 200_000_000 or c > 200_000_000: ok = False; break
                blocks.append((u, c, fl))
            if not ok: continue
            nnodes = struct.unpack_from(">I", bi, p)[0]; p += 4
            if not (0 < nnodes < 5000): continue
            nodes = []
            for _ in range(nnodes):
                off, sz, fl = struct.unpack_from(">qqI", bi, p); p += 20
                nm, p = read_cstr(bi, p)
                nodes.append((off, sz, fl, nm))
            print(f"  hashlen={hashlen} nblocks={nblocks} nnodes={nnodes}")
            for (u, c, fl) in blocks: print(f"    block unc={u} comp={c} fl=0x{fl:x}")
            for nd in nodes: print(f"    node off={nd[0]} size={nd[1]} fl=0x{nd[2]:x} {nd[3]}")
            return b, o, ctype, blocks, nodes
        except Exception as e:
            print(f"  hashlen={hashlen} parse failed: {e}")
    raise SystemExit("cannot parse blocksInfo")

if __name__ == "__main__":
    parse(sys.argv[1])
