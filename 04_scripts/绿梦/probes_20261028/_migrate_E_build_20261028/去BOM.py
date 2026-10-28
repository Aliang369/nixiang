# -*- coding: utf-8 -*-
"""去掉 .py 文件开头的 UTF-8 BOM（BOM 会让 `python file.py` 直接 SyntaxError）。

来源：绿梦迁移进来的探针脚本早期由 PowerShell 写出，带 EF BB BF 头。
只去掉 3 字节 BOM，不改任何其它内容；改完逐个 py_compile 自检。
"""
import glob
import py_compile
import sys

pats = ["04_scripts/绿梦/probes_20261028/*.py",
        "04_scripts/长生天机降世/*.py",
        "04_scripts/绿梦/probes_20261028/_migrate_E_build_20261028/*.py"]
fixed, skipped, bad = [], [], []
for p in pats:
    for f in sorted(glob.glob(p)):
        raw = open(f, "rb").read()
        if raw.startswith(b"\xef\xbb\xbf"):
            open(f, "wb").write(raw[3:])
            fixed.append(f)
        else:
            skipped.append(f)
for f in fixed + skipped:
    try:
        py_compile.compile(f, doraise=True, cfile="/tmp/_pyc_check.pyc")
    except Exception as e:
        bad.append((f, str(e)[:120]))
print("fixed:", len(fixed))
for f in fixed:
    print("  -", f)
print("skipped(no BOM):", len(skipped))
print("compile bad:", len(bad))
for f, e in bad:
    print("  !", f, e)
