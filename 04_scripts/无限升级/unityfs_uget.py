#!/usr/bin/env python3
"""Extract full decompressed data section from a UnityFS bundle (handles padding + LZ4)."""
import struct, sys, os
import unityfs

def load(path):
    b = open(path, "rb").read()
    o = 0
    sig, o = unityfs.read_cstr(b, o); o += 4
    u, o = unityfs.read_cstr(b, o)
    r, o = unityfs.read_cstr(b, o)
    o += 8
    cbis = struct.unpack_from(">I", b, o)[0]; o += 4
    ubis = struct.unpack_from(">I", b, o)[0]; o += 4
    flags = struct.unpack_from(">I", b, o)[0]; o += 4
    if flags & 0x200:
        o = (o + 15) & ~15
    bi = unityfs.decompress(flags & 0x3F, b[o:o + cbis], ubis)
    o += cbis
    if flags & 0x200:
        o = (o + 15) & ~15
    p = 16
    n = struct.unpack_from(">I", bi, p)[0]; p += 4
    blocks = [struct.unpack_from(">IIH", bi, p + 10 * i) for i in range(n)]
    p += 10 * n
    nn = struct.unpack_from(">I", bi, p)[0]; p += 4
    nodes = []
    for _ in range(nn):
        off, sz, fl = struct.unpack_from(">qqI", bi, p); p += 20
        nm, p = unityfs.read_cstr(bi, p)
        nodes.append((off, sz, fl, nm))
    pos, chunks, bad = o, [], []
    for idx, (uu, c, fl) in enumerate(blocks):
        try:
            chunks.append(unityfs.lz4_decompress(b[pos:pos + c], uu))
        except Exception:
            chunks.append(b"\x00" * uu); bad.append(idx)
        pos += c
    return nodes, b"".join(chunks), bad

if __name__ == "__main__":
    nodes, data, bad = load(sys.argv[1])
    print(f"nodes={len(nodes)} data={len(data)} badblocks={bad}")
    for (off, sz, fl, nm) in nodes:
        blob = data[off:off + sz]
        print(f"  {nm} off={off} size={sz} fl=0x{fl:x} MZ={blob.find(b'MZ')}")
