# -*- coding: utf-8 -*-
"""在 YooAsset 清单里检索热更 dll 相关条目。"""
import glob
import os
import re
import sys

d0 = sys.argv[1] if len(sys.argv) > 1 else "_cs104/tmp/manifests/cs104"
files = glob.glob(os.path.join(d0, "*v1.0.4.5.bytes")) or glob.glob(os.path.join(d0, "*.bytes"))
f = sorted(files)[0]
d = open(f, "rb").read()
print("file", os.path.basename(f), "size", len(d), "head", d[:12])
for k in [b"dll", b"Code", b"HotUpdate", b"hotfix", b"Assembly"]:
    print(f"  {k!r}: {d.count(k)}")
txt = re.findall(rb"[ -~]{5,}", d)
hits = [t for t in txt if b"dll" in t.lower() or b"code" in t.lower() or b"hot" in t.lower()]
print("== 命中片段（前 40）==")
for t in hits[:40]:
    print("   ", t.decode("ascii", "replace"))
print("== 全部条目名样本（前 20）==")
for t in txt[:20]:
    print("   ", t.decode("ascii", "replace"))
