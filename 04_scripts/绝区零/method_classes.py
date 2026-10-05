#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""method_classes.py — 从运行期堆 dump 里解析 MethodInfo 表并附上所属类名。

记录布局（实测于绝区零 3.2.0 内存堆，步长 0x40）：
  +0x00  const char* name_ptr   -> 真实方法名在 name_ptr+0x18
  +0x08  void*       methodPointer
  +0x10  void*       klass       -> 类名串指针在 klass+0x20
  +0x18  uint64      id

用法:
  python method_classes.py DUMP --base 0x5f80001000 --libbase 0x400023000000 \
      --libhi 0x400043000000 --out methods_cls.tsv [--cls-kw Monster,Skill] [--kw ultimate]
"""
import argparse
import re
import struct

NAME_PREFIX = 0x18
NAME_RE = re.compile(rb"[A-Za-z_<>.$][A-Za-z0-9_<>.:`\[\]$@]{1,200}\x00")
CLS_RE = re.compile(rb"[A-Za-z_<>.$][A-Za-z0-9_<>.:`\[\]$@]{1,200}\x00")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("dump")
    ap.add_argument("--base", type=lambda x: int(x, 0), default=0x5f80001000)
    ap.add_argument("--libbase", type=lambda x: int(x, 0), default=0x400023000000)
    ap.add_argument("--libhi", type=lambda x: int(x, 0), default=0x400043000000)
    ap.add_argument("--out", required=True)
    ap.add_argument("--cls-kw", default=None, help="按类名过滤（逗号分隔，子串匹配）")
    ap.add_argument("--kw", default=None, help="按方法名过滤（逗号分隔，子串匹配）")
    ap.add_argument("--limit", type=int, default=400)
    a = ap.parse_args()

    d = open(a.dump, "rb").read()
    size = len(d)
    lo = a.base
    hi = a.base + size

    def cstr(va, rx, maxlen):
        off = va - lo
        if off < 0 or off + 2 >= size:
            return None
        m = rx.match(d, off)
        if not m:
            return None
        try:
            return m.group(0)[:-1].decode("utf-8")
        except UnicodeDecodeError:
            return None

    out = []
    seen = set()
    klass_cache = {}
    for off in range(0, size - 0x40, 8):
        nameptr, code, klass = struct.unpack_from("<QQQ", d, off)
        if not (lo <= nameptr < hi and lo <= klass < hi and a.libbase <= code < a.libhi):
            continue
        name = cstr(nameptr + NAME_PREFIX, NAME_RE, 220)
        if name is None:
            continue
        cname = klass_cache.get(klass, False)
        if cname is False:
            koff = klass - lo
            if koff < 0 or koff + 0x28 > size:
                cname = None
            else:
                cptr = struct.unpack_from("<Q", d, koff + 0x20)[0]
                cname = cstr(cptr, CLS_RE, 220) if lo <= cptr < hi else None
            klass_cache[klass] = cname
        rva = code - a.libbase
        key = (cname, name, rva)
        if key in seen:
            continue
        seen.add(key)
        out.append((cname or "", name, rva, a.base + off))

    with open(a.out, "w", encoding="utf-8") as f:
        for cname, name, rva, va in out:
            f.write("%s\t%s\t0x%x\t0x%x\n" % (cname, name, rva, va))
    classes = sorted(set(c for c, _, _, _ in out if c))
    print("entries=%d classes=%d -> %s" % (len(out), len(classes), a.out))

    def report(tag, pool):
        print("== %s : %d" % (tag, len(pool)))
        for row in pool[:a.limit]:
            print("%-46s %-52s RVA 0x%x" % (row[0][:46], row[1][:52], row[2]))

    if a.cls_kw:
        kws = [k.strip().lower() for k in a.cls_kw.split(",") if k.strip()]
        report("class~" + ",".join(kws),
               [r for r in out if any(k in (r[0] or "").lower() for k in kws)])
    if a.kw:
        kws = [k.strip().lower() for k in a.kw.split(",") if k.strip()]
        report("name~" + ",".join(kws),
               [r for r in out if any(k in (r[1] or "").lower() for k in kws)])


if __name__ == "__main__":
    main()
