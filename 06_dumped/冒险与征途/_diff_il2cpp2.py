import os, hashlib

BASE = r"E:\nixiang\06_dumped\冒险与征途"
disk_path = os.path.join(BASE, r"apk_extract\lib\arm64-v8a\libil2cpp.so")
DUMP_BASE = 0x400023200000
mem_files = [os.path.join(BASE, "mem_il2cpp", f"il2cpp_mem_{i}.bin") for i in range(1, 7)]

raw = bytearray()
for p in mem_files:
    with open(p, 'rb') as f:
        raw += f.read()

disk = open(disk_path, 'rb').read()
print("disk size:", hex(len(disk)), "raw dump:", hex(len(raw)))

# (vaddr_start, vaddr_end, file_off)   -- taken verbatim from /proc/PID/maps
MAPS = [
    (0x400023200000, 0x400024F74000, 0x00000000),
    (0x400024F74000, 0x40002527D000, 0x01D73000),
    (0x40002527D000, 0x40002527E000, 0x0207C000),
    (0x40002527E000, 0x400027F00000, 0x0207D000),
    (0x400027F00000, 0x400028248000, 0x04CFE000),
    (0x400028248000, 0x4000284FF000, 0x05045000),
]

mem = bytearray(b'\x00' * len(disk))
covered = bytearray(len(disk))
for vs, ve, fo in MAPS:
    size = ve - vs
    src = vs - DUMP_BASE
    if src + size > len(raw):
        size = len(raw) - src          # rw-p .data tail was not dumped (not needed for text hooks)
    if size <= 0:
        continue
    mem[fo:fo + size] = raw[src:src + size]
    covered[fo:fo + size] = b'\x01' * size
print("covered bytes:", sum(covered), "of", len(disk))

diffs = []
n = len(disk)
i = 0
while i < n:
    if not covered[i] or mem[i] == disk[i]:
        i += 1
        continue
    j = i
    while j < n and covered[j] and mem[j] != disk[j]:
        j += 1
    diffs.append((i, j - i))
    i = j
print("diff runs:", len(diffs), "total diff bytes:", sum(d[1] for d in diffs))

merged = []
for off, ln in diffs:
    if merged and off - (merged[-1][0] + merged[-1][1]) <= 16:
        o, l = merged[-1]
        merged[-1] = (o, off + ln - o)
    else:
        merged.append((off, ln))
print("merged regions (gap<=16):", len(merged))
with open(os.path.join(BASE, "diff_regions.txt"), 'w') as f:
    for o, l in merged:
        f.write(f"0x{o:08X} len=0x{l:X} ({l})\n")
for o, l in merged[:120]:
    print(f"0x{o:08X} len={l}  disk={disk[o:o+16].hex()} mem={mem[o:o+16].hex()}")
