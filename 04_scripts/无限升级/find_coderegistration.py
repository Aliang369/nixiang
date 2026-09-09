import struct

SO_PATH = "/Users/aliang/Documents/nixiang/06_dumped/无限升级/libil2cpp.so"


def parse_elf_segments(data):
    assert data[:4] == b"\x7fELF", "not ELF"
    assert data[4] == 2, "not ELF64"
    e_phoff, = struct.unpack_from("<Q", data, 0x20)
    e_phentsize, e_phnum = struct.unpack_from("<HH", data, 0x36)
    segs = []
    for i in range(e_phnum):
        o = e_phoff + i * e_phentsize
        p_type, p_flags = struct.unpack_from("<II", data, o)
        p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align = struct.unpack_from("<QQQQQQ", data, o + 8)
        if p_type == 1:
            segs.append((p_offset, p_vaddr, p_filesz))
    return segs


def main():
    so = open(SO_PATH, "rb").read()
    n = len(so)
    segs = parse_elf_segments(so)
    print("PT_LOAD: %d" % len(segs))
    for p_offset, p_vaddr, p_filesz in segs:
        print("  off=0x%x va=0x%x size=0x%x" % (p_offset, p_vaddr, p_filesz))

    def off2va(o):
        for p_offset, p_vaddr, p_filesz in segs:
            if p_offset <= o < p_offset + p_filesz:
                return p_vaddr + (o - p_offset)
        return None

    def va2off(v):
        for p_offset, p_vaddr, p_filesz in segs:
            if p_vaddr <= v < p_vaddr + p_filesz:
                return p_offset + (v - p_vaddr)
        return None

    sbox = bytes.fromhex("637c777bf26b6fc53001672bfed7ab76")
    print("AES sbox occurrences=%d" % so.count(sbox))

    results = {}
    for needle in (b"targetfuntional.dll\x00", b"mscorlib.dll\x00", b"Assembly-CSharp.dll\x00"):
        results[needle] = []
        pos = so.find(needle)
        while pos != -1:
            va = off2va(pos)
            if va:
                results[needle].append((pos, va))
            pos = so.find(needle, pos + 1)
    for k, v in results.items():
        print("string %r -> %s" % (k, [(hex(a), hex(b)) for a, b in v]))

    strva = results[b"targetfuntional.dll\x00"][0][1] if results[b"targetfuntional.dll\x00"] else None
    if strva is None:
        print("!! no module name string in so -> so may be packed/encrypted")
        return

    pat = struct.pack("<Q", strva)
    struct_positions = [p for p in range(0, n - 8, 8) if so[p:p + 8] == pat]
    struct_vas = [off2va(p) for p in struct_positions]
    print("module struct candidates: %s" % [hex(v) for v in struct_vas if v])

    array_slots = set()
    for sv in struct_vas:
        if not sv:
            continue
        q = so.find(struct.pack("<Q", sv))
        while q != -1:
            if q % 8 == 0:
                array_slots.add(q)
            q = so.find(struct.pack("<Q", sv), q + 1)
    slots = sorted(array_slots)
    groups = []
    cur = []
    for s in slots:
        if cur and s - cur[-1] == 8:
            cur.append(s)
        else:
            if cur:
                groups.append(cur)
            cur = [s]
    if cur:
        groups.append(cur)
    groups.sort(key=len, reverse=True)
    print("pointer groups top5 len: %s" % [len(g) for g in groups[:5]])
    if not groups or len(groups[0]) < 5:
        print("!! no codeGenModules array")
        return
    best = groups[0]
    arr_off = best[0]
    arr_va = off2va(arr_off)
    mod_count = len(best)
    print("codeGenModules array: off=0x%x va=0x%x count=%d" % (arr_off, arr_va, mod_count))

    cr_cands = []
    q = so.find(struct.pack("<Q", arr_va))
    while q != -1:
        if q % 8 == 0:
            cnt_before, = struct.unpack_from("<Q", so, q - 8)
            if cnt_before == mod_count:
                cr_cands.append(q - 16)
        q = so.find(struct.pack("<Q", arr_va), q + 1)
    print("CodeRegistration candidates: %s" % [hex(off2va(c)) if off2va(c) else hex(c) for c in cr_cands])

    mr_cands = []
    vals_seen = set()
    positions = []
    from array import array
    qw = array("Q")
    qw.frombytes(so[: (n // 8) * 8])
    m = len(qw)
    for i in range(m - 2):
        v0 = qw[i]
        if 1000 <= v0 <= 200000:
            vptr = qw[i + 1]
            if 0x1000 <= vptr < 0x800000000000 and qw[i + 2] == v0:
                o = va2off(vptr)
                if o is not None and 0 < o < n:
                    positions.append((i * 8, v0))
                    vals_seen.add(v0)
    print("generic [V,ptr,V] hits: %s" % [(hex(off2va(i)) if off2va(i) else hex(i), v) for i, v in positions[:20]])
    print("V candidates: %s" % sorted(vals_seen))
    for i, v0 in positions[:20]:
        for back in range(0, 200, 8):
            base = i - back
            bva = off2va(base)
            if bva:
                mr_cands.append((bva, back, v0))
    print("MetadataRegistration candidate bases: %s" % [(hex(a), b, v) for a, b, v in mr_cands[:30]])


main()
