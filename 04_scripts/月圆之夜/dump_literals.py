#!/usr/bin/env python3.11
# 导出全部 stringLiterals 并搜关键词; 建 RVA->方法 索引
import struct
import re

MD = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
DUMPCS = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/dump_out3/dump.cs"
d = open(MD, "rb").read()
SL, SLD = 0x100, 0x228C8
n = 17657
lits = []
for i in range(n):
    ln, di = struct.unpack_from("<Ii", d, SL + i * 8)
    if ln:
        s = d[SLD + di: SLD + di + ln].decode("utf-8", "replace")
    else:
        s = ""
    lits.append(s)

print("字面量总数:", len(lits))
import re as _re
pats = ["yyzy", "Decrypt", "decrypt", "password", "Password", "secret", "Secret",
        "hotfix", "HotUpdate", "Key", "key=", "xxtea", "XXTEA", "crypt"]
seen = set()
for i, s in enumerate(lits):
    for p in pats:
        if p in s and s not in seen and len(s) < 120:
            seen.add(s)
            print("  [%5d] %r" % (i, s))
            break

# RVA 索引
print("\n== RVA 索引 ==")
txt = open(DUMPCS).read()
rva_map = {}
cur = []
for line in txt.splitlines():
    ls = line.strip()
    if ls.startswith("// Namespace:"):
        cur = []
    elif "class " in ls or "struct " in ls or "interface " in ls or "enum " in ls:
        cur.append(ls.split("// TypeDefIndex")[0].strip())
    m = re.search(r"// RVA: (0x[0-9A-Fa-f]+)", ls)
    if m:
        mname = re.search(r"(\S+)\s*\{$", ls) or re.search(r"public|private", ls)
        rva_map[int(m.group(1), 16)] = (cur[-1] if cur else "?", ls.strip()[:80])
import pickle
pickle.dump(rva_map, open("/Users/aliang/Documents/nixiang/04_scripts/月圆之夜/rva_map.pkl", "wb"))
print("索引条目:", len(rva_map))
for a in (0x14fc968, 0x14fca34, 0x14fca08, 0x26e38fc):
    print("  0x%x -> %s" % (a, rva_map.get(a, "未收录")))
