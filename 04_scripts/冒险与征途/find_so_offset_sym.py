#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把 libstarry_magic.so 的故障偏移反查回「哪个 .o 的哪个函数」。
原理：链接后 .so 的函数体字节与中间 .o 的 .text 字节基本一致（只有 adrp/bl 等重定位差异），
     故在 .o 的 .text 里搜索 .so 故障点附近 16 字节窗口即可定位源对象与函数符号。
用法：python find_so_offset_sym.py <so文件> <偏移1> [偏移2 ...]
"""
import os
import struct
import sys
import subprocess

OBJ_DIR = os.path.join(os.environ.get("LOCALAPPDATA", ""), "Temp", "mxyzt_build", "obj", "local",
                       "arm64-v8a")
LLVM_NM = (r"C:\Users\Administrator\AppData\Local\Android\Sdk\ndk\27.0.12077973\toolchains"
           r"\llvm\prebuilt\windows-x86_64\bin\llvm-nm.exe")


def elf_sections(path):
    """返回 [(name, vaddr, offset, size)]（64 位小端）。"""
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
    strtab_off = secs[e_shstrndx][4]
    for s in secs:
        end = data.index(b"\0", strtab_off + s[0])
        s.append(data[strtab_off + s[0]:end].decode("latin1"))
    return data, secs


def vaddr_to_off(secs, vaddr, prefer_exec=True):
    for s in secs:
        _, typ, flags, addr, offset, size, name = s
        if addr <= vaddr < addr + size and typ != 8:
            return offset + (vaddr - addr), name
    return None, None


def main():
    so_path = sys.argv[1]
    offs = [int(x, 16) for x in sys.argv[2:]]
    sdata, ssecs = elf_sections(so_path)
    print("so sections (exec):")
    for s in ssecs:
        if s[6] in (".text", ".rodata"):
            print("   %-10s vaddr=0x%x off=0x%x size=0x%x" % (s[6], s[3], s[4], s[5]))
    objs = []
    for root, _dirs, files in os.walk(OBJ_DIR):
        for f in files:
            if f.endswith(".o"):
                objs.append(os.path.join(root, f))
    print("objects: %d" % len(objs))
    for voff in offs:
        foff, secname = vaddr_to_off(ssecs, voff)
        if foff is None:
            print("\n[0x%x] 不在任何节里" % voff)
            continue
        win = sdata[foff:foff + 32]
        print("\n[so vaddr 0x%x] 节=%s 文件偏移=0x%x bytes=%s" % (voff, secname, foff, win.hex()))
        found = False
        for o in objs:
            try:
                odata, osecs = elf_sections(o)
            except Exception:
                continue
            for s in osecs:
                if s[6] != ".text":
                    continue
                text = odata[s[4]:s[4] + s[5]]
                for start in range(0, 17):
                    pat = win[start:start + 16]
                    if len(pat) < 16:
                        break
                    idx = text.find(pat)
                    if idx >= 0:
                        print("  命中 -> %s  .text+0x%x (窗口起 %d)" % (o, idx, start))
                        found = True
                        break
                if found:
                    break
            if found:
                break
        if not found:
            print("  未命中（可能该处含重定位）")


if __name__ == "__main__":
    main()
