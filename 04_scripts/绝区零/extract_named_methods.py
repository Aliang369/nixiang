#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""extract_named_methods.py — 从内存 dump 里抽取 {ptrA, code, ptrB, name} 记录，输出「名→代码RVA」表。

背景：绝区零 libil2cpp 的 metadata 磁盘加密，但运行期堆里存在
      [ptr(0x5f8...)] [code ptr 0x4000...] [ptr(0x5f8...)] [NUL 结尾名字] 的记录；
      code 指针落在 libil2cpp 映射区间时，可换算成 RVA（libil2cpp 基址默认 0x400023000000）。

用法:
  python extract_named_methods.py DUMP --liblo 0x400023000000 --libhi 0x400043000000 \
      --out names.tsv [--pattern] [--kw timeScale,energy,ultimate]
"""
import argparse
import re
import struct


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("dump")
    ap.add_argument("--liblo", type=lambda x: int(x, 0), default=0x400023000000)
    ap.add_argument("--libhi", type=lambda x: int(x, 0), default=0x400043000000)
    ap.add_argument("--heaplo", type=lambda x: int(x, 0), default=0x5f80000000)
    ap.add_argument("--hephi", type=lambda x: int(x, 0), default=0x5f90000000)
    ap.add_argument("--base", type=lambda x: int(x, 0), default=0x400023000000,
                    help="RVA 基准（libil2cpp 装载基址）")
    ap.add_argument("--out", default="names.tsv")
    ap.add_argument("--kw", default=None)
    a = ap.parse_args()

    d = open(a.dump, "rb").read()
    name_re = re.compile(rb"[A-Za-z_<>.][A-Za-z0-9_<>.:`\[\]]{2,120}\x00")
    out = []
    seen = set()
    # 记录起点按 8 字节对齐扫描
    for off in range(0, len(d) - 32, 8):
        p1, code, p2 = struct.unpack_from("<QQQ", d, off)
        if not (a.heaplo <= p1 < a.hephi):
            continue
        if not (a.liblo <= code < a.libhi):
            continue
        if not (a.heaplo <= p2 < a.hephi):
            continue
        m = name_re.match(d, off + 24)
        if not m:
            continue
        name = m.group(0)[:-1].decode("ascii")
        rva = code - a.base
        key = (name, rva)
        if key in seen:
            continue
        seen.add(key)
        out.append((name, rva, off))

    with open(a.out, "w", encoding="utf-8") as f:
        for name, rva, off in out:
            f.write("%s\t0x%x\t0x%x\n" % (name, rva, off))
    print("records=%d -> %s" % (len(out), a.out))

    if a.kw:
        kws = [k.lower() for k in a.kw.split(",")]
        for name, rva, off in out:
            low = name.lower()
            if any(k in low for k in kws):
                print("  %-60s RVA 0x%x (dump 0x%x)" % (name, rva, off))


if __name__ == "__main__":
    main()
