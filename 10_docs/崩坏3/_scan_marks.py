import os, re, sys

DIR = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell"
marks = [b"libil2cpp.so", b"Toggle_000", b"game/38.html", b"fate.AGTool",
         b"0x5F16228", b"5F16228", b"5F156A8", b"5F16828",
         b"Category_1", b"BUFF", b"file.myqcloud.com", b"c8d531894261daf1fbd49a738932a702",
         b"IconWebViewData", b"IsGameLibLoaded", b"GetFeatureList", b"getGoHomeUrl",
         b"/data/local/tmp/fha", b"SpeedUpCCH", b"tersafe"]

files = sorted(f for f in os.listdir(DIR) if f.startswith("seg_") and f.endswith(".bin"))
print("scanned files:", len(files))
for f in files:
    p = os.path.join(DIR, f)
    try:
        data = open(p, "rb").read()
    except Exception as e:
        continue
    hits = []
    for m in marks:
        c = data.count(m)
        if c:
            hits.append((m.decode("latin1"), c))
    if hits:
        print(f"{f} ({len(data)}):", hits)
