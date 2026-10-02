import os, struct, collections

B = r"E:\nixiang\06_dumped\冒险与征途"
apk = open(os.path.join(B, r"apk_extract\lib\arm64-v8a\libil2cpp.so"), 'rb').read()
raw = b"".join(open(os.path.join(B, "mem_il2cpp", f"il2cpp_mem_{i}.bin"), 'rb').read() for i in range(1, 7))
extra = open(os.path.join(B, "mem_il2cpp", "il2cpp_data.bin"), 'rb').read()
BASE = 0x400023200000
EXTRA_START = 0x400028248000
LOADS = [(0x0, 0x0, 0x1D7308C), (0x1D7308C, 0x1D7408C, 0x2F8B204),
         (0x4CFE290, 0x4D00290, 0x347388), (0x5045618, 0x5048618, 0x2B5AB0)]

def v2f(v):
    for off, va, sz in LOADS:
        if va <= v < va + sz:
            return v + (off - va)
    return None

def memword(vaddr):
    roff = vaddr
    if roff + 8 <= len(raw):
        return raw[roff:roff + 8], 'chunk'
    o2 = vaddr + BASE - EXTRA_START
    if 0 <= o2 and o2 + 8 <= len(extra):
        return extra[o2:o2 + 8], 'extra'
    return None, None

# --- parse .rela.dyn ---
REL_OFF, REL_SZ = 0x35E68, 0xC9A398
n = REL_SZ // 24
rel_relative = {}
rel_other = set()
for i in range(n):
    o = REL_OFF + i * 24
    r_off, r_info, r_add = struct.unpack_from('<QQq', apk, o)
    t = r_info & 0xFFFFFFFF
    if t == 1027:
        rel_relative[r_off] = r_add
    else:
        rel_other.add(r_off)
print(f"rela.dyn entries={n}  RELATIVE={len(rel_relative)} other={len(rel_other)}")

# --- scan all 8-byte aligned words in RW LOADs ---
targets = [(0x4CFE290 - 0x1000, LOADS[2][2]), (0x5045618 - 0x3000, LOADS[3][2])]
report = collections.defaultdict(list)
for va_start_file, sz in [("LOAD3", 0), ("LOAD4", 0)]:
    pass

for li in (2, 3):
    off, va, sz = LOADS[li]
    name = f"LOAD{li+1}({'rx?'})"
    seg_v = va
    cnt_rel = cnt_other = cnt_free = 0
    bad_rel = []
    free_diff = []
    for v in range(va, va + sz - 7, 8):
        mw, src = memword(v)
        if mw is None:
            continue
        fo = v2f(v)
        if fo is None or fo + 8 > len(apk):
            continue
        dw = apk[fo:fo + 8]
        m = struct.unpack('<Q', mw)[0]
        d = struct.unpack('<Q', dw)[0]
        if v in rel_relative:
            cnt_rel += 1
            exp = (BASE + rel_relative[v]) & 0xFFFFFFFFFFFFFFFF
            if m != exp:
                bad_rel.append((v, d, m, exp))
        elif v in rel_other:
            cnt_other += 1
        else:
            cnt_free += 1
            if m != d:
                free_diff.append((v, d, m))
    print(f"\n=== {name} va=0x{va:X} size=0x{sz:X} ===")
    print(f"  words: reloc-relative={cnt_rel} reloc-other={cnt_other} nonreloc={cnt_free}")
    print(f"  RELATIVE-slot mismatches (=shell rewrote a relocated slot?): {len(bad_rel)}")
    for v, d, m, exp in bad_rel[:25]:
        print(f"    vaddr=0x{v:X} file=0x{v2f(v):X} diskadd=0x{d:X} mem=0x{m:X} exp=0x{exp:X}")
    print(f"  non-reloc words differing: {len(free_diff)}")
    for v, d, m in free_diff[:40]:
        print(f"    vaddr=0x{v:X} file=0x{v2f(v):X} disk=0x{d:X} mem=0x{m:X}")

# --- special: which diffs point into libSDKloading / anon? ---
print("\n=== pointer classification of non-reloc diffs in LOAD3/4 ===")
buckets = collections.Counter()
suspicious = []
for li in (2, 3):
    off, va, sz = LOADS[li]
    for v in range(va, va + sz - 7, 8):
        if v in rel_relative or v in rel_other:
            continue
        mw, src = memword(v)
        if mw is None: continue
        fo = v2f(v)
        if fo is None or fo + 8 > len(apk): continue
        m = struct.unpack('<Q', mw)[0]
        d = struct.unpack('<Q', apk[fo:fo + 8])[0]
        if m == d: continue
        if 0x400020800000 <= m < 0x400020D02000:
            buckets['->libSDKloading'] += 1; suspicious.append((v, d, m, 'SDK'))
        elif 0x400060000000 <= m < 0x400090000000:
            buckets['->anon/translator'] += 1; suspicious.append((v, d, m, 'ANON'))
        elif 0x400023200000 <= m < 0x400028500000:
            buckets['->libil2cpp'] += 1; suspicious.append((v, d, m, 'IL2CPP'))
        elif 0x400021A00000 <= m < 0x400023200000:
            buckets['->libunity'] += 1; suspicious.append((v, d, m, 'UNITY'))
        elif m == 0:
            buckets['->zero'] += 1
        else:
            buckets['->other 0x%X' % (m >> 40)] += 1
print(dict(buckets))
print("\n--- suspicious (mem points outside libil2cpp) ---")
for v, d, m, k in suspicious[:60]:
    print(f"  {k} vaddr=0x{v:X} file=0x{v2f(v):X} disk=0x{d:X} mem=0x{m:X}")
print("total suspicious:", len(suspicious))
