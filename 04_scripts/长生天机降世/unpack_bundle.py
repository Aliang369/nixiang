# -*- coding: utf-8 -*-
"""从 UnityFS bundle 里取出所有 TextAsset（用法: python unpack_bundle.py <bundle> <outdir>）。"""
import os
import sys

import UnityPy

src = sys.argv[1]
outdir = sys.argv[2]
os.makedirs(outdir, exist_ok=True)

env = UnityPy.load(src)
print("objects:", len(env.objects))
for obj in env.objects:
    if obj.type.name != "TextAsset":
        continue
    d = obj.read()
    name = getattr(d, "m_Name", "") or "unnamed"
    raw = getattr(d, "m_Script", b"")
    if isinstance(raw, str):
        raw = raw.encode("utf-8", "surrogateescape")
    path = os.path.join(outdir, name)
    open(path, "wb").write(raw)
    print(f"  {name}: {len(raw)} B -> {path}")
