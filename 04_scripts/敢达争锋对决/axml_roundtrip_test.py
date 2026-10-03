#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""AXML 序列化器往返自检：parse -> build（不改任何东西）-> 与原文件逐字节比对 + 报告首处差异。"""
import importlib.util
import sys

PATCHER = r"E:\nixiang\04_scripts\敢达争锋对决\patch_gdzfdj_manifest.py"


def load():
    spec = importlib.util.spec_from_file_location("gdzfdj_axml", PATCHER)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def main():
    src, dst = sys.argv[1], sys.argv[2]
    orig = open(src, "rb").read()
    m = load()
    doc = m.Doc(orig)
    out = doc.build()
    open(dst, "wb").write(out)
    print("[roundtrip] orig=%d out=%d diff=%d" % (len(orig), len(out), len(out) - len(orig)))
    n = min(len(orig), len(out))
    first = None
    for i in range(n):
        if orig[i] != out[i]:
            first = i
            break
    if first is None:
        print("[roundtrip] no byte difference in common prefix (files differ only by length)")
    else:
        print("[roundtrip] first diff @0x%X: orig=%s out=%s" % (
            first, orig[first:first + 16].hex(" "), out[first:first + 16].hex(" ")))
        print("[roundtrip] context orig=0x%X..0x%X" % (first - 8, first + 24))
    # chunk 布局对比
    def chunks(b):
        o = 0
        res = []
        while o + 8 <= len(b):
            t, hs, sz = __import__("struct").unpack_from("<HHI", b, o)
            res.append((o, t, hs, sz))
            if sz <= 0:
                break
            o += sz
        return res
    print("[roundtrip] orig chunks head:", chunks(orig)[:4], "count", len(chunks(orig)))
    print("[roundtrip] out  chunks head:", chunks(out)[:4], "count", len(chunks(out)))


if __name__ == "__main__":
    main()
