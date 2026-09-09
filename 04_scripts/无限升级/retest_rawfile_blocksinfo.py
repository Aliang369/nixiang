import struct
import os
import lz4.block

RAWDIR = "/tmp/wxsj/rawfile_apk"


def read_cstr(b, o):
    e = b.index(b"\0", o)
    return b[o:e].decode("utf8", "replace"), e + 1


def dec(src, usize, comp):
    if comp == 0:
        return bytes(src)
    if comp in (2, 3):
        return lz4.block.decompress(src, uncompressed_size=usize)
    if comp == 1:
        import lzma
        return lzma.LZMADecompressor(format=lzma.FORMAT_ALONE).decompress(src)
    raise ValueError("comp=%d" % comp)


def a16(x):
    return (x + 15) & ~15


def try_blocks(ublob, skip, en):
    try:
        p = skip
        cnt = struct.unpack_from(en + "I", ublob, p)[0]
        p += 4
        if not (0 < cnt < 10000):
            return None
        blocks = []
        for _ in range(cnt):
            us, cs = struct.unpack_from(en + "II", ublob, p)
            p += 8
            bf = struct.unpack_from(en + "H", ublob, p)[0]
            p += 2
            if us == 0 or us > (1 << 30) or cs == 0 or cs > (1 << 30):
                return None
            blocks.append((us, cs, bf))
        nn = struct.unpack_from(en + "I", ublob, p)[0]
        p += 4
        if not (0 <= nn < 1000):
            return None
        nodes = []
        for _ in range(nn):
            noff, nsz = struct.unpack_from(en + "qq", ublob, p)
            p += 16
            nf = struct.unpack_from(en + "I", ublob, p)[0]
            p += 4
            npth, p = read_cstr(ublob, p)
            if nsz < 0 or nsz > (1 << 30):
                return None
            nodes.append((npth, noff, nsz, nf))
        return cnt, blocks, nodes, p
    except Exception:
        return None


def sniff(out):
    tags = []
    if out[:2] == b"MZ":
        tags.append("MZ@0")
    i = out.find(b"MZ")
    if i > 0:
        tags.append("MZ@%d" % i)
    if b"BSJB" in out:
        tags.append("BSJB")
    if b"targetfuntional" in out:
        tags.append("STR:targetfuntional")
    if b"mscorlib" in out:
        tags.append("STR:mscorlib")
    if b"YooAssetAesCrypto" in out:
        tags.append("STR:YooAssetAesCrypto")
    return ",".join(tags) or "plain"


def main():
    try:
        from UnityPy.enums import ArchiveFlags
        print("ArchiveFlags enum:", [(m.name, m.value) for m in ArchiveFlags])
    except Exception as e:
        print("enum print fail:", e)
    print()
    files = sorted(f for f in os.listdir(RAWDIR) if f.endswith(".rawfile"))
    n_ok = 0
    for fn in files:
        p = os.path.join(RAWDIR, fn)
        data = open(p, "rb").read()
        sig, o = read_cstr(data, 0)
        ver = struct.unpack_from(">I", data, o)[0]
        o += 4
        vp, o = read_cstr(data, o)
        ve, o = read_cstr(data, o)
        size, csz, usz, flags = struct.unpack_from(">qIII", data, o)
        o += 20
        if ver >= 7:
            o = a16(o)
        start = o
        blob = data[start:start + csz]
        try:
            ublob = dec(blob, usz, flags & 0x3F)
        except Exception as e:
            print("=" * 108)
            print("%s sig=%s ver=%d ve=%s size=%d cbi=%d ubi=%d flags=0x%x" % (
                fn, sig, ver, ve, size, csz, usz, flags))
            print("  !! blocksInfo decompress fail: %s (possible encryption / header mismatch)" % e)
            continue
        cands = {}
        for skip in (0, 16):
            for en in (">", "<"):
                r = try_blocks(ublob, skip, en)
                if r:
                    key = "skip=%d,endian=%s" % (skip, "BE" if en == ">" else "LE")
                    cands[key] = r
        print("=" * 108)
        print("%s sig=%s ver=%d ve=%s size=%d cbi=%d ubi=%d flags=0x%x blob=%dB" % (
            fn, sig, ver, ve, size, csz, usz, flags, len(ublob)))
        for k in ("skip=0,endian=BE", "skip=0,endian=LE", "skip=16,endian=BE", "skip=16,endian=LE"):
            if k not in cands:
                print("  [%s] -> parse fail" % k)
        for k, (cnt, blocks, nodes, consumed) in cands.items():
            s_usz = sum(b[0] for b in blocks)
            s_csz = sum(b[1] for b in blocks)
            print("  [%s] blockCount=%d blocks=%dB(us)/%dB(cs) nodes=%d consumed=%d/%d %s" % (
                k, cnt, s_usz, s_csz, len(nodes), consumed, len(ublob),
                "OK" if s_csz <= len(data) else "BAD"))
        best = None
        for key in ("skip=16,endian=BE", "skip=16,endian=LE", "skip=0,endian=BE", "skip=0,endian=LE"):
            if key in cands:
                best = cands[key]
                print("  >> use %s" % key)
                break
        if not best:
            print("  !! no sane parse")
            continue
        cnt, blocks, nodes, consumed = best
        s_usz = sum(b[0] for b in blocks)
        out = None
        for dpos in (start + csz, a16(start + csz)):
            try:
                buf = bytearray()
                q = dpos
                for us, cs, bf in blocks:
                    chunk = data[q:q + cs]
                    q += cs
                    buf += dec(chunk, us, bf & 0x3F)
                if len(buf) == s_usz:
                    out = bytes(buf)
                    print("  data@%d decompress=%dB == sum(us) OK" % (dpos, len(out)))
                    break
                print("  data@%d len=%d != sum(us)=%d" % (dpos, len(buf), s_usz))
            except Exception as e:
                print("  data@%d fail: %s" % (dpos, e))
        if out is None:
            print("  !! data decompress failed (possible encryption)")
            continue
        for npth, noff, nsz, nf in nodes:
            print("  node: path=%s off=%d size=%d" % (npth, noff, nsz))
        print("  head=%s sniff=[%s]" % (out[:16].hex(), sniff(out)))
        n_ok += 1
    print()
    print("RESULT: %d/%d rawfile parsed as standard unencrypted UnityFS" % (n_ok, len(files)))


main()
