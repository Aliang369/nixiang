#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""extract_methods_v3.py — 全量解析运行期 MethodInfo 表（按 0x40 连续段），输出 类名/方法名/RVA。

背景：绝区零 3.2.0 的堆里，MethodInfo 记录按「类」分成若干连续段，步长 0x40：
  +0x00 name_ptr（真实名字串在 name_ptr+prefix，prefix 随记录版本不同，实测多为 0x18）
  +0x08 methodPointer（il2cpp 代码 VA）
  +0x10 klass（类名串指针在 klass+0x20）
本脚本先找连续段，再逐条解析，自动尝试 prefix ∈ {0x18,0x10,0x20,0x8,0x0}。

用法:
  python extract_methods_v3.py DUMP --base 0x5f80001000 --libbase 0x400023000000 \
      --libhi 0x400043000000 --out all_methods.tsv
"""
import argparse
import re
import struct

PREFIXES = (0x18, 0x10, 0x20, 0x8, 0x0)
NAME_RE = re.compile(rb"[A-Za-z_<>.$][A-Za-z0-9_<>.:`\[\]$@]{1,200}\x00")
CLS_RE = re.compile(rb"[A-Za-z_<>.$][A-Za-z0-9_<>.:`\[\]$@]{1,200}\x00")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("dump")
    ap.add_argument("--base", type=lambda x: int(x, 0), default=0x5f80001000)
    ap.add_argument("--libbase", type=lambda x: int(x, 0), default=0x400023000000)
    ap.add_argument("--libhi", type=lambda x: int(x, 0), default=0x400043000000)
    ap.add_argument("--minrun", type=int, default=4)
    ap.add_argument("--out", required=True)
    ap.add_argument("--cls-kw", default=None)
    ap.add_argument("--limit", type=int, default=400)
    a = ap.parse_args()

    d = open(a.dump, "rb").read()
    size = len(d)
    lo = a.base
    hi = a.base + size

    def valid(o):
        if o + 0x40 > size:
            return False
        np_, code, kl = struct.unpack_from("<QQQ", d, o)
        return lo <= np_ < hi and a.libbase <= code < a.libhi and lo <= kl < hi

    def cstr(va, rx, maxlen=220):
        o = va - lo
        if o < 0 or o + 2 >= size:
            return None
        m = rx.match(d, o)
        if not m:
            return None
        try:
            return m.group(0)[:-1].decode("utf-8")
        except UnicodeDecodeError:
            return None

    # 1) 找连续段
    runs = []
    off = 0
    while off < size - 0x40:
        if valid(off):
            n = 1
            while valid(off + n * 0x40):
                n += 1
            if n >= a.minrun:
                runs.append((off, n))
            off += n * 0x40
        else:
            off += 8

    # 2) 逐条解析（带 name_ptr / klass 缓存）
    name_cache = {}
    klass_cache = {}
    rows = []
    seen = set()
    unresolved = 0
    for roff, n in runs:
        for i in range(n):
            o = roff + i * 0x40
            np_, code, kl = struct.unpack_from("<QQQ", d, o)
            name = name_cache.get(np_, False)
            if name is False:
                name = None
                for pre in PREFIXES:
                    s = cstr(np_ + pre, NAME_RE)
                    if s:
                        name = s
                        break
                name_cache[np_] = name
            if name is None:
                unresolved += 1
                continue
            cn = klass_cache.get(kl, False)
            if cn is False:
                cn = None
                ko = kl - lo
                if 0 <= ko + 0x28 <= size:
                    cptr = struct.unpack_from("<Q", d, ko + 0x20)[0]
                    if lo <= cptr < hi:
                        cn = cstr(cptr, CLS_RE)
                klass_cache[kl] = cn
            rva = code - a.libbase
            key = (cn or "", name, rva)
            if key in seen:
                continue
            seen.add(key)
            rows.append((cn or "", name, rva, lo + o))

    with open(a.out, "w", encoding="utf-8") as f:
        for cn, name, rva, va in rows:
            f.write("%s\t%s\t0x%x\t0x%x\n" % (cn, name, rva, va))
    print("runs=%d records=%d unresolved=%d classes=%d -> %s" % (
        len(runs), len(rows), unresolved, len(set(r[0] for r in rows if r[0])), a.out))

    if a.cls_kw:
        kws = [k.strip().lower() for k in a.cls_kw.split(",") if k.strip()]
        hits = [r for r in rows if any(k in r[0].lower() for k in kws)]
        print("== class~%s : %d" % (",".join(kws), len(hits)))
        for r in hits[:a.limit]:
            print("%-46s %-52s RVA 0x%x" % (r[0][:46], r[1][:52], r[2]))


if __name__ == "__main__":
    main()
