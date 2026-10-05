#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""extract_methods_v2.py — 从运行期堆 dump 里解析 MethodInfo 表，输出「方法名→RVA」。

记录布局（实测于绝区零 3.2.0 内存堆，步长 0x40）：
  +0x00  const char* name_ptr   -> 名字串，真实串在 +0x16 偏移处，前 22 字节为哈希前缀
  +0x08  void*       methodPointer
  +0x10  void*       klass
  +0x18  uint64      id
  +0x20  uint32      0xffffffff
  +0x28  void*       invoker
  +0x30  uint32      metadata token (低 4 字节) / flags

用法:
  python extract_methods_v2.py DUMP --base 0x5f80001000 --libbase 0x400023000000 \
      --libhi 0x400043000000 --out methods.tsv [--kw energy,time,freeze]
"""
import argparse
import re
import struct

NAME_PREFIX = 0x18
NAME_RE = re.compile(rb"[A-Za-z_<>.$][A-Za-z0-9_<>.:`\[\]$@]{1,200}\x00")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("dump")
    ap.add_argument("--base", type=lambda x: int(x, 0), default=0x5f80001000,
                    help="dump 文件起点对应的虚拟地址")
    ap.add_argument("--libbase", type=lambda x: int(x, 0), default=0x400023000000)
    ap.add_argument("--libhi", type=lambda x: int(x, 0), default=0x400043000000)
    ap.add_argument("--out", required=True)
    ap.add_argument("--kw", default=None)
    ap.add_argument("--limit", type=int, default=200)
    a = ap.parse_args()

    d = open(a.dump, "rb").read()
    size = len(d)
    lo = a.base
    hi = a.base + size

    out = []
    seen = set()
    diag = {"scan": 0, "ptr_ok": 0, "name_ok": 0}
    for off in range(0, size - 0x40, 8):
        diag["scan"] += 1
        nameptr, code, klass = struct.unpack_from("<QQQ", d, off)
        if not (lo <= nameptr < hi and lo <= klass < hi and a.libbase <= code < a.libhi):
            continue
        diag["ptr_ok"] += 1
        pos = nameptr - lo + NAME_PREFIX
        if pos < 0 or pos + 2 >= size:
            continue
        m = NAME_RE.match(d, pos)
        if not m:
            continue
        diag["name_ok"] += 1
        name = m.group(0)[:-1].decode("ascii")
        rva = code - a.libbase
        key = (name, rva)
        if key in seen:
            continue
        seen.add(key)
        out.append((name, rva, a.base + off))
    print("diag", diag)

    with open(a.out, "w", encoding="utf-8") as f:
        for name, rva, va in out:
            f.write("%s\t0x%x\t0x%x\n" % (name, rva, va))
    print("methods=%d unique_names=%d -> %s" % (
        len(out), len(set(n for n, _, _ in out)), a.out))

    if a.kw:
        kws = [k.strip().lower() for k in a.kw.split(",") if k.strip()]
        for k in kws:
            hits = [r for r in out if k in r[0].lower()]
            print("== %s: %d" % (k, len(hits)))
            for name, rva, va in hits[:a.limit]:
                print("   %-64s RVA 0x%x" % (name, rva))


if __name__ == "__main__":
    main()
