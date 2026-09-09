import struct
import os
import lz4.block

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
    dpos = ((start + csz) + 15) & ~15
    return data, dpos, blocks


def lz4_decode_tolerant(src, dst_budget):
    out = bytearray()
    i = 0
    n = len(src)
    while i < n:
        if len(out) >= dst_budget:
            return bytes(out), i, "budget"
        token = src[i]
        i += 1
        lit = token >> 4
        if lit == 15:
            while True:
                if i >= n:
                    return bytes(out), i, "lit-len-eof"
                b = src[i]
                i += 1
                lit += b
                if b != 255:
                    break
        if i + lit > n:
            return bytes(out), i, "lit-range-eof(need%d have%d)" % (lit, n - i)
        out += src[i:i + lit]
        i += lit
        if i >= n:
            return bytes(out), i, "end-after-lit(normal stream end)"
        if len(out) >= dst_budget:
            return bytes(out), i, "budget-after-lit"
        if i + 2 > n:
            return bytes(out), i, "offset-eof"
        off = src[i] | (src[i + 1] << 8)
        i += 2
        if off == 0:
            return bytes(out), i, "offset-0(corrupt)"
        ml = token & 15
        if ml == 15:
            while True:
                if i >= n:
                    return bytes(out), i, "matchlen-eof"
                b = src[i]
                i += 1
                ml += b
                if b != 255:
                    break
        ml += 4
        pos = len(out) - off
        if pos < 0:
            return bytes(out), i, "offset-before-start(off=%d out=%d)" % (off, len(out))
        for j in range(ml):
            out.append(out[pos + j])
    return bytes(out), i, "input-consumed"


def main():
    for fn in (
        "ed4df249da0680cf9db3dcbae580da48.rawfile",
        "8d1b50e6edecd73a788e3c52c9c3632c.rawfile",
        "283207f9b6d43e659dc8b5caf4faee2b.rawfile",
    ):
        p = os.path.join(RAWDIR, fn)
        data, dpos, blocks = parse(p)
        dr = data[dpos:]
        print("=" * 100)
        print(fn)
        b0 = blocks[0]
        seg0 = dr[:b0[1]]
        out, ipos, why = lz4_decode_tolerant(seg0, b0[0])
        print(" block0 standalone manual decode: out=%dB/%dB input=%d/%d stop=%r" % (
            len(out), b0[0], ipos, len(seg0), why))
        print("  out head: %s" % out[:48].hex(" "))
        printable = out[:64]
        print("  out ascii: %r" % bytes(32 <= c < 127 and c or 46 for c in printable))
        tail = max(0, ipos - 16)
        print("  in tail@%d: %s" % (ipos, seg0[tail:ipos + 8].hex(" ")))
        total_us = sum(b[0] for b in blocks)
        try:
            r = lz4.block.decompress(dr, uncompressed_size=total_us)
            print(" WHOLE-REGION single lz4 stream: OK out=%dB (expect %d) head=%s" % (
                len(r), total_us, r[:48].hex(" ")))
            print("  ascii: %r" % bytes(32 <= c < 127 and c or 46 for c in r[:64]))
            cabs = r.find(b"CAB-")
            print("  CAB- at %d, Unity ver str at %s" % (cabs, r.find(b"2022.")))
            open("/tmp/wxsj/%s.wholestream.bin" % fn[:8], "wb").write(r)
        except Exception as e:
            print(" WHOLE-REGION single stream fail: %s" % e)


main()
