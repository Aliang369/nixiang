import os, sys, hashlib

BASE = r"E:\nixiang\06_dumped\冒险与征途"
disk_path = os.path.join(BASE, r"apk_extract\lib\arm64-v8a\libil2cpp.so")
mem_files = [os.path.join(BASE, "mem_il2cpp", f"il2cpp_mem_{i}.bin") for i in range(1, 7)]

BLOCK = 1 << 20
mem = bytearray()
for p in mem_files:
    with open(p, 'rb') as f:
        mem += f.read()
print("mem image size:", len(mem), hex(len(mem)))

disk = open(disk_path, 'rb').read()
print("disk size:", len(disk), hex(len(disk)))
print("disk md5:", hashlib.md5(disk).hexdigest())

n = min(len(mem), len(disk))
diffs = []
i = 0
while i < n:
    if mem[i] == disk[i]:
        i += 1
        continue
    j = i
    while j < n and mem[j] != disk[j]:
        j += 1
    diffs.append((i, j - i))
    i = j
print("diff runs:", len(diffs), "total diff bytes:", sum(d[1] for d in diffs))

# merge runs separated by <=16 equal bytes (trampoline = 12 bytes, small gaps possible)
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
for o, l in merged[:80]:
    print(f"0x{o:08X} len={l}  disk={disk[o:o+16].hex()} mem={mem[o:o+16].hex()}")
