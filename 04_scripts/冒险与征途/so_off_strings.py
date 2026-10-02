#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""反查 .so 故障偏移属于哪个函数：解析 objdump 反汇编里的 adrp/add 对，读出引用的字符串。

用法：python so_off_strings.py <so> <起始偏移> <结束偏移> [<起始> <结束> ...]
"""
import os
import re
import struct
import subprocess
import sys

LLVM_OBJDUMP = (r"C:\Users\Administrator\AppData\Local\Android\Sdk\ndk\27.0.12077973\toolchains"
                r"\llvm\prebuilt\windows-x86_64\bin\llvm-objdump.exe")


def sections(path):
    with open(path, "rb") as f:
        data = f.read()
    (e_shoff,) = struct.unpack_from("<Q", data, 0x28)
    (e_shentsize, e_shnum, e_shstrndx) = struct.unpack_from("<HHH", data, 0x3a)
    secs = []
    for i in range(e_shnum):
        off = e_shoff + i * e_shentsize
        (nameoff, typ) = struct.unpack_from("<II", data, off)
        (flags, addr, offset, size) = struct.unpack_from("<QQQQ", data, off + 8)
        secs.append([nameoff, typ, flags, addr, offset, size])
    st = secs[e_shstrndx][4]
    for s in secs:
        end = data.index(b"\0", st + s[0])
        s.append(data[st + s[0]:end].decode("latin1"))
    return data, secs


def read_cstr(data, secs, vaddr):
    for s in secs:
        _, typ, flags, addr, offset, size, name = s
        if addr <= vaddr < addr + size and typ != 8:
            fo = offset + (vaddr - addr)
            end = data.index(b"\0", fo)
            if end - fo > 200:
                return None
            raw = data[fo:end]
            try:
                return raw.decode("utf-8")
            except UnicodeDecodeError:
                return None
    return None


def main():
    so = sys.argv[1]
    pairs = [(int(sys.argv[i], 16), int(sys.argv[i + 1], 16)) for i in range(2, len(sys.argv), 2)]
    data, secs = sections(so)
    for lo, hi in pairs:
        out = subprocess.run([LLVM_OBJDUMP, "-d", "--start-address=0x%x" % lo,
                              "--stop-address=0x%x" % hi, so],
                             capture_output=True, text=True).stdout
        adrp = {}
        hits = []
        for line in out.splitlines():
            m = re.match(r"\s*([0-9a-f]+):\s+([0-9a-f ]+?)\s{2,}(\S+)(.*)$", line)
            if not m:
                continue
            addr = int(m.group(1), 16)
            ins = m.group(3)
            rest = m.group(4).strip()
            if ins == "adrp":
                mm = re.search(r"(x\d+), 0x([0-9a-f]+)", rest)
                if mm:
                    adrp[mm.group(1)] = (addr & ~0xfff) + (int(mm.group(2), 16) - (int(mm.group(2), 16) & ~0xfff)) if False else int(mm.group(2), 16)
            elif ins in ("add", "ldr") and rest.startswith(tuple(adrp)):
                mm = re.search(r"^(x\d+), (?:x\d+, )?#0x([0-9a-f]+)", rest)
                if mm and mm.group(1) in adrp:
                    tgt = adrp[mm.group(1)] + int(mm.group(2), 16)
                    s = read_cstr(data, secs, tgt)
                    if s and len(s) >= 4:
                        hits.append((addr, ins, tgt, s))
        print("\n=== so 0x%x-0x%x: %d 条字符串引用 ===" % (lo, hi, len(hits)))
        for addr, ins, tgt, s in hits:
            print("  0x%x %-4s -> 0x%x  %r" % (addr, ins, tgt, s))


if __name__ == "__main__":
    main()
