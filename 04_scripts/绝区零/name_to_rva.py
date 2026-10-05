#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""name_to_rva.py — 用名字字符串反查运行期 MethodInfo 记录，输出「类名 :: 方法名 → RVA」。

原理：方法记录 +0x00 是 name_ptr，真实名字串在 name_ptr+0x18（实测 3.2.0 堆布局）。
给定名字串 VA，就能在 dump 里找指向 name_ptr 的 qword，再校验 +0x08 的
methodPointer（应落在 libil2cpp 区间）与 +0x10 的 klass（应落在堆区间）。

用法:
  python name_to_rva.py DUMP --base 0x5f80001000 --libbase 0x400023000000 \
      --libhi 0x400043000000 --rx "EntityTimeScale|SetTimeScale|MonsterAI"
"""
import argparse
import re
import struct


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("dump")
    ap.add_argument("--base", type=lambda x: int(x, 0), default=0x5f80001000)
    ap.add_argument("--libbase", type=lambda x: int(x, 0), default=0x400023000000)
    ap.add_argument("--libhi", type=lambda x: int(x, 0), default=0x400043000000)
    ap.add_argument("--rx", required=True, help="名字串正则（大小写敏感）")
    ap.add_argument("--minlen", type=int, default=3)
    ap.add_argument("--limit", type=int, default=400)
    ap.add_argument("--show-orphan", action="store_true", help="也打印没有方法记录的串")
    a = ap.parse_args()

    d = open(a.dump, "rb").read()
    size = len(d)
    lo = a.base
    hi = a.base + size
    rx = re.compile(a.rx.encode())

    # 1) 收集符合正则的 C 风格名字串（必须以 \0 结束，且前缀符合标识符）
    names = []
    for m in re.finditer(rb"[\x20-\x7e]{3,200}\x00", d):
        s = m.group(0)[:-1]
        if len(s) < a.minlen or not rx.search(s):
            continue
        names.append((m.start(), s))
    print("matched strings=%d" % len(names))

    # 2) 对每个串，找指向 (stringVA-0x18) 的 qword
    rows = []
    orphan = []
    for off, s in names:
        sva = lo + off
        np_ = sva - 0x18
        pat = struct.pack("<Q", np_)
        pos = 0
        found = False
        while True:
            pos = d.find(pat, pos)
            if pos < 0:
                break
            if pos + 0x40 > size:
                pos += 1
                continue
            code = struct.unpack_from("<Q", d, pos + 0x8)[0]
            klass = struct.unpack_from("<Q", d, pos + 0x10)[0]
            if a.libbase <= code < a.libhi and lo <= klass < hi:
                cname = ""
                ko = klass - lo
                if 0 <= ko + 0x28 <= size:
                    cptr = struct.unpack_from("<Q", d, ko + 0x20)[0]
                    if lo <= cptr < hi:
                        co = cptr - lo
                        e = d.find(b"\x00", co, co + 200)
                        if e > co:
                            try:
                                cname = d[co:e].decode("utf-8")
                            except UnicodeDecodeError:
                                cname = ""
                rows.append((cname, s.decode("ascii", "replace"), code - a.libbase, lo + pos))
                found = True
            pos += 1
        if not found and a.show_orphan:
            orphan.append((off, s))

    rows.sort(key=lambda r: (r[0], r[1], r[2]))
    print("records=%d" % len(rows))
    for cname, name, rva, recva in rows[:a.limit]:
        print("%-44s %-52s RVA 0x%-10x rec=%#x" % (cname[:44], name[:52], rva, recva))
    if a.show_orphan:
        print("orphan strings=%d" % len(orphan))
        for off, s in orphan[:a.limit]:
            print("   %-70s off=%#x" % (s[:70], off))


if __name__ == "__main__":
    main()
