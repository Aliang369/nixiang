#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""enum_classes.py — 从运行期内存 dump 反向枚举全部 Il2CppClass 名。

判据（2026-10-05 实测布局）: klass+0x20 -> name 字符串; klass+0x40 -> 0x7f00.. 区。
输出: klass_va\tname
用法: python enum_classes.py out.txt [dumpdir]
"""
import glob
import os
import re
import struct
import sys

DEFAULT_DUMPS = {
    "region_5f80_v2.bin": 0x5F80001000,
    "region_7800.bin": 0x7800000000,
    "r45.bin": 0x5FA9800000,
    "r5fa9.bin": 0x5FA9800000,
    "r5fa5.bin": 0x5FA5800000,
}

rx_name = re.compile(rb"[A-Za-z_][A-Za-z0-9_`<>\[\].,/+]{2,160}\x00")


def main():
    out_path = sys.argv[1]
    d = sys.argv[2] if len(sys.argv) > 2 else os.path.join(
        os.environ.get("TEMP", "/tmp"), "nx_zzz_a_20261005")
    dumps = []
    for f in sorted(glob.glob(os.path.join(d, "*.bin"))):
        base = DEFAULT_DUMPS.get(os.path.basename(f))
        if base is None:
            continue
        dumps.append((f, base, open(f, "rb").read()))
    print("dumps=%d total=%.0f MB" % (len(dumps), sum(len(x[2]) for x in dumps) / 1e6),
          file=sys.stderr)

    def cstr_va(va):
        for f, base, data in dumps:
            if base <= va < base + len(data):
                m = rx_name.match(data, va - base)
                if m:
                    try:
                        return m.group(0)[:-1].decode("utf-8")
                    except UnicodeDecodeError:
                        return None
        return None

    def u64_va(va):
        for f, base, data in dumps:
            if base <= va < base + len(data) - 7:
                return struct.unpack_from("<Q", data, va - base)[0]
        return None

    seen = {}
    for f, base, data in dumps:
        n = len(data) - 8
        for i in range(0, n, 8):
            if i < 0x40:
                continue
            val = struct.unpack_from("<Q", data, i)[0]
            if not (0x5F00000000 <= val < 0x8000000000):
                continue
            name = cstr_va(val)
            if not name or name.startswith("_") or len(name) < 3:
                continue
            klass_va = base + i - 0x20
            p40 = u64_va(klass_va + 0x40)
            if p40 is None or not (0x7F0000000000 <= p40 < 0x800000000000):
                continue
            seen[klass_va] = name
    with open(out_path, "w", encoding="utf-8") as fh:
        for k in sorted(seen):
            fh.write("%#x\t%s\n" % (k, seen[k]))
    print("classes=%d -> %s" % (len(seen), out_path), file=sys.stderr)


if __name__ == "__main__":
    main()
