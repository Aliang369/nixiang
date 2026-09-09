#!/usr/bin/env python3.11
# 解析任意 metadata usage 槽: slot VA -> RELA addend -> record -> 字面量/类型
import struct, subprocess, re, sys

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
META = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = open(SO, "rb").read()
md = open(META, "rb").read()
SL, LD = 0x100, 0x228C8
NLIT = 141256 // 8

# 字面量表
lits = []
for i in range(NLIT):
    a, b = struct.unpack_from("<ii", md, SL + i * 8)
    if a <= 0 or a > 4096:
        lits.append(None)
    else:
        lits.append(md[LD + b:LD + b + a].decode("utf-8", "replace"))

out = subprocess.run(["/opt/homebrew/opt/llvm/bin/llvm-readelf", "-r", SO],
                     capture_output=True, text=True).stdout
relmap = {}
pat = re.compile(r"^([0-9a-f]+)\s+[0-9a-f]+\s+R_AARCH64_RELATIVE\s+([0-9a-f]+)\s*$")
for line in out.splitlines():
    mo = pat.match(line.strip())
    if mo:
        relmap[int(mo.group(1), 16)] = int(mo.group(2), 16)

TY = {0: "Type", 1: "MethodDef?", 2: "FieldInfo?", 3: "Str", 4: "MethodSpec", 5: "StrLit", 6: "??", 7: "??"}


def decode_record(rec_va):
    t = struct.unpack_from("<Q", d, rec_va)[0]
    ty = (t >> 29) & 7
    idx = (t & 0x1FFFFFFE) >> 1
    ridx = t & 1
    return ty, idx, ridx, t


for arg in sys.argv[1:]:
    slot = int(arg, 16)
    add = relmap.get(slot)
    if add is None:
        print(f"slot 0x{slot:X}: 无 RELA (file 值 0x{struct.unpack_from('<Q', d, slot)[0]:X})")
        continue
    ty, idx, ridx, raw = decode_record(add)
    s = lits[idx] if ty == 5 and 0 <= idx < NLIT else None
    print(f"slot 0x{slot:X} -> rec@0x{add:X} raw=0x{raw:X} type={ty}({TY.get(ty,'?')}) idx={idx} ridx={ridx}" + (f" lit={s!r}" if s else ""))
