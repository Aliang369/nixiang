import os, struct, collections

BASE = r"E:\nixiang\06_dumped\冒险与征途"
DUMP_BASE = 0x400023200000
disk = open(os.path.join(BASE, r"apk_extract\lib\arm64-v8a\libil2cpp.so"), 'rb').read()
raw = bytearray()
for i in range(1, 7):
    raw += open(os.path.join(BASE, "mem_il2cpp", f"il2cpp_mem_{i}.bin"), 'rb').read()

MAPS = [
    (0x400023200000, 0x400024F74000, 0x00000000),
    (0x400024F74000, 0x40002527D000, 0x01D73000),
    (0x40002527D000, 0x40002527E000, 0x0207C000),
    (0x40002527E000, 0x400027F00000, 0x0207D000),
    (0x400027F00000, 0x400028248000, 0x04CFE000),
]
mem = bytearray(b'\x00' * len(disk))
cov = bytearray(len(disk))
for vs, ve, fo in MAPS:
    size = ve - vs
    src = vs - DUMP_BASE
    size = min(size, len(raw) - src)
    mem[fo:fo + size] = raw[src:src + size]
    cov[fo:fo + size] = b'\x01' * size

# executable LOAD segment of libil2cpp.so (from ELF phdrs)
EXEC_OFF, EXEC_END = 0x01D7308C, 0x04CFE290
print(f"exec range file 0x{EXEC_OFF:X}..0x{EXEC_END:X}  ({(EXEC_END-EXEC_OFF)/1048576:.1f} MB)")

diffs = []
i = EXEC_OFF
while i < EXEC_END:
    if not cov[i] or mem[i] == disk[i]:
        i += 1; continue
    j = i
    while j < EXEC_END and cov[j] and mem[j] != disk[j]:
        j += 1
    diffs.append((i, j - i)); i = j
print("exec diff runs:", len(diffs), "bytes:", sum(d[1] for d in diffs))

merged = []
for off, ln in diffs:
    if merged and off - (merged[-1][0] + merged[-1][1]) <= 8:
        o, l = merged[-1]; merged[-1] = (o, off + ln - o)
    else:
        merged.append((off, ln))
print("merged exec regions (gap<=8):", len(merged))
with open(os.path.join(BASE, "exec_diff_regions.txt"), 'w') as f:
    for o, l in merged:
        f.write(f"0x{o:08X} len=0x{l:X} ({l})\n")

def disasm_hex(b):
    return ' '.join(f"{struct.unpack_from('<I', b, k)[0]:08x}" for k in range(0, len(b) - 3, 4))

print("\n--- first 60 merged regions ---")
for o, l in merged[:60]:
    al = o % 4
    print(f"0x{o:08X} len={l} align%4={al}")
    print(f"   disk: {disasm_hex(disk[o - al:o - al + min(32, 32)])}")
    print(f"   mem : {disasm_hex(mem[o - al:o - al + min(32, 32)])}")

# classify lengths
c = collections.Counter()
for o, l in merged:
    c[(l // 4) * 4 if l >= 4 else l] += 1
print("\n--- length histogram (top 20) ---")
for k, v in c.most_common(20):
    print(f"  len={k}: {v}")
