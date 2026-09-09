import struct
import os
import lz4.block
import lz4.frame

RAWDIR = "/tmp/wxsj/rawfile_apk"


def read_cstr(b, o):
    e = b.index(b"\0", o)
    return b[o:e].decode("utf8", "replace"), e + 1


def parse(path):
    data = open(path, "rb").read()
    sig, o = read_cstr(data, 0)
    ver = struct.unpack_from(">I", data, o)[0]
    o += 4
    vp, o = read_cstr(data, o)
    ve, o = read_cstr(data, o)
    size, csz, usz, flags = struct.unpack_from(">qIII", data, o)
    o += 20
    start = (o + 15) & ~15
    ublob = lz4.block.decompress(data[start:start + csz], uncompressed_size=usz)
    p = 16
    cnt = struct.unpack_from(">I", ublob, p)[0]
    p += 4
    blocks = []
    for _ in range(cnt):
        us, cs = struct.unpack_from(">II", ublob, p)
        p += 8
        bf = struct.unpack_from(">H", ublob, p)[0]
        p += 2
        blocks.append((us, cs, bf))
    nn = struct.unpack_from(">I", ublob, p)[0]
    p += 4
    nodes = []
    for _ in range(nn):
        noff, nsz = struct.unpack_from(">qq", ublob, p)
        p += 16
        nf = struct.unpack_from(">I", ublob, p)[0]
        p += 4
        npth, p = read_cstr(ublob, p)
        nodes.append((npth, noff, nsz, nf))
    dpos = ((start + csz) + 15) & ~15
    return data, start, dpos, blocks, nodes


def ratio_printable(b):
    if not b:
        return 0.0
    return sum(1 for x in b if 32 <= x < 127 or x in (9, 10, 13)) / len(b)


def main():
    targets = [
        "ed4df249da0680cf9db3dcbae580da48.rawfile",
        "8d1b50e6edecd73a788e3c52c9c3632c.rawfile",
        "283207f9b6d43e659dc8b5caf4faee2b.rawfile",
    ]
    for fn in targets:
        p = os.path.join(RAWDIR, fn)
        data, start, dpos, blocks, nodes = parse(p)
        print("=" * 100)
        print(fn)
        print(" start=%d dpos=%d" % (start, dpos))
        for npth, noff, nsz, nf in nodes:
            print(" node: path=%r off=%d size=%d" % (npth, noff, nsz))
        dr = data[dpos:]
        print(" data region: %d B, whole printable ratio=%.4f" % (len(dr), ratio_printable(dr)))
        off = 0
        for i, (us, cs, bf) in enumerate(blocks[:5]):
            seg = dr[off:off + cs]
            print("  block%d us=%d cs=%d bflags=0x%x printable=%.4f head=%s" % (
                i, us, cs, bf, ratio_printable(seg[:8192]), seg[:32].hex(" ")))
            off += cs
        for kw in (b"targetfuntional", b"mscorlib", b"assembly", b"Mono", b"Unity", b"BSJB", b'":'):
            f = dr.find(kw)
            print("  find %-16r count=%d firstoff=%s" % (kw, dr.count(kw), f))
        try:
            r = lz4.frame.decompress(dr[:blocks[0][1]])
            print("  lz4.frame OK len=%d head=%s" % (len(r), r[:16].hex(" ")))
        except Exception as e:
            print("  lz4.frame fail: %s" % type(e).__name__)
        for key in (b"gamemen2026", b"c698rish2210"):
            x = bytes(a ^ b for a, b in zip(dr[:blocks[0][1]], (key * 65536)[:blocks[0][1]]))
            try:
                r = lz4.block.decompress(x, uncompressed_size=blocks[0][0])
                print("  xor(%s)+lz4 OK len=%d head=%s" % (key.decode(), len(r), r[:16].hex(" ")))
            except Exception:
                print("  xor(%s)+lz4 fail" % key.decode())
        try:
            import UnityPy
            env = UnityPy.load(p)
            objs = list(env.objects)
            print("  UnityPy objects=%d" % len(objs))
            for ob in objs[:5]:
                try:
                    print("    obj type=%s" % ob.type.name)
                except Exception:
                    pass
        except Exception as e:
            print("  UnityPy fail: %s: %s" % (type(e).__name__, e))


main()
