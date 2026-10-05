#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""class_methods.py — 按「类名」在运行期 dump 里定位 Il2CppClass，并列出该类的方法（RVA）。

背景（绝区零 3.2.0 实测布局）：
  Il2CppClass:  +0x20 = name 串指针, +0x40 = 0x7f38.. 区指针（元数据映射）
  MethodInfo:   +0x00 name_obj, +0x08 methodPointer(0x400023000000+RVA), +0x10 klass

用法:
  python class_methods.py --dump DIR_OR_FILE [--dump ...] --class NAME [--class NAME2 ...]

dump 支持多个文件；-默认读 %TEMP%\\nx_zzz_a_20261005 下的 region_*.bin / r*.bin。
每段 dump 需给出基址：region_5f80_v2.bin=0x5f80001000，其余用 --bases 覆盖。
"""
import argparse
import glob
import os
import re
import struct
import sys

LIBBASE_DEFAULT = 0x400023000000
LIBHI_DEFAULT = 0x400043000000

DEFAULT_DUMPS = {
    "region_5f80_v2.bin": 0x5F80001000,
    "region_7800.bin": 0x7800000000,
    "r45.bin": 0x5FA9800000,
    "r5fa9.bin": 0x5FA9800000,
    "r5fa5.bin": 0x5FA5800000,
}


def load_dumps(paths, bases):
    out = []
    for p in paths:
        files = [p] if os.path.isfile(p) else sorted(glob.glob(os.path.join(p, "*.bin")))
        for f in files:
            base = bases.get(os.path.basename(f))
            if base is None:
                print("skip %s (no base)" % f, file=sys.stderr)
                continue
            out.append((f, base, open(f, "rb").read()))
    return out


def cstr_at(base, d, va, rx, maxlen=240):
    if not (base <= va < base + len(d)):
        return None
    m = rx.match(d, va - base)
    if not m:
        return None
    try:
        return m.group(0)[:-1].decode("utf-8")
    except UnicodeDecodeError:
        return None


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dump", action="append", default=None)
    ap.add_argument("--bases", default="", help="name=base,name2=base2 覆盖默认基址")
    ap.add_argument("--class", dest="classes", action="append", required=True)
    ap.add_argument("--libbase", type=lambda x: int(x, 0), default=LIBBASE_DEFAULT)
    ap.add_argument("--libhi", type=lambda x: int(x, 0), default=LIBHI_DEFAULT)
    ap.add_argument("--limit", type=int, default=200)
    a = ap.parse_args()

    bases = dict(DEFAULT_DUMPS)
    for kv in a.bases.split(","):
        if "=" in kv:
            k, v = kv.split("=", 1)
            bases[k.strip()] = int(v, 0)
    paths = a.dump or [os.path.join(os.environ.get("TEMP", "/tmp"), "nx_zzz_a_20261005")]
    dumps = load_dumps(paths, bases)
    print("dumps=%d total=%.1f MB" % (len(dumps), sum(len(d) for _, _, d in dumps) / 1e6))

    ident = re.compile(rb"[A-Za-z_][A-Za-z0-9_]{1,240}\x00")
    for cls in a.classes:
        needle = cls.encode() + b"\x00"
        print("\n===== class %s" % cls)
        for fname, base, d in dumps:
            for m in re.finditer(re.escape(needle), d):
                sva = base + m.start()
                pat = struct.pack("<Q", sva)
                for fo, fbase, fd in dumps:
                    pos = 0
                    while True:
                        i = fd.find(pat, pos)
                        if i < 0:
                            break
                        pos = i + 1
                        klass_va = fbase + i - 0x20
                        ko = i - 0x20
                        if ko < 0 or ko + 0x48 > len(fd):
                            continue
                        p40 = struct.unpack_from("<Q", fd, ko + 0x40)[0]
                        if not (0x7F0000000000 <= p40 < 0x800000000000):
                            continue
                        print("  klass %#x (%s)" % (klass_va, os.path.basename(fo)))
                        # 扫 MethodInfo
                        n = 0
                        for j, jbase, jd in dumps:
                            k = 0
                            while True:
                                k = jd.find(struct.pack("<Q", klass_va), k)
                                if k < 0:
                                    break
                                ro = k - 0x10
                                k += 1
                                if ro < 0 or ro + 0x40 > len(jd):
                                    continue
                                np_ = struct.unpack_from("<Q", jd, ro)[0]
                                code = struct.unpack_from("<Q", jd, ro + 8)[0]
                                if not (jbase <= np_ < jbase + len(jd)):
                                    continue
                                if not (a.libbase <= code < a.libhi):
                                    continue
                                nm = None
                                for pre in (0x18, 0x10, 0x20, 0x8, 0x0):
                                    nm = cstr_at(jbase, jd, np_ + pre, ident)
                                    if nm:
                                        break
                                n += 1
                                if n <= a.limit:
                                    print("     RVA 0x%-10x %-46s rec=%#x" % (
                                        code - a.libbase, nm or "(enc)", jbase + ro))
                        print("     methods=%d" % n)


if __name__ == "__main__":
    main()
