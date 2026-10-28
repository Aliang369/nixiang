#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把 wlvcode: 日志的 hex 用 llvm-objdump 反汇编（capstone 在本机 arm64 不可用）。

做法：hex -> binary -> llvm-objcopy 包成 elf64-littleaarch64 的 .text ->
      llvm-objdump -d --adjust-vma=<方法地址> -> 逐行解析并注释 bl 目标。

用法: python dis_wlv2.py <wlvcode.txt> [方法名过滤]
"""
import os
import re
import subprocess
import sys
import tempfile

NDK = os.path.expandvars(r"%LOCALAPPDATA%\Android\Sdk\ndk\27.0.12077973")
BIN = os.path.join(NDK, "toolchains", "llvm", "prebuilt", "windows-x86_64", "bin")
OBJCOPY = os.path.join(BIN, "llvm-objcopy.exe")
OBJDUMP = os.path.join(BIN, "llvm-objdump.exe")

LINE = re.compile(r"wlvcode:\s+(\S+?)\.(\w+)\((\d+)\)\s+code=0x([0-9a-fA-F]+)\s+([0-9a-fA-F]+)")
INSN = re.compile(r"^\s*([0-9a-f]+):\s+(?:[0-9a-f]{8}\s+)?(\S+)\s*(.*)$")


def parse(path):
    out = []
    with open(path, "r", encoding="utf-8", errors="replace") as f:
        for ln in f:
            m = LINE.search(ln)
            if m:
                out.append((m.group(1), m.group(2), int(m.group(3)),
                            int(m.group(4), 16), m.group(5)))
    return out


def to_bytes(hexstr):
    """日志里是 read32 的数值 ⇒ 文本序是大端；按 4 字节小组翻转回真实内存序。"""
    out = bytearray()
    for i in range(0, len(hexstr) - 7, 8):
        chunk = hexstr[i:i + 8]
        out += bytes.fromhex(chunk)[::-1]
    return bytes(out)


def disasm(blob_hex, vma, tmpdir):
    raw = os.path.join(tmpdir, "t.bin")
    obj = os.path.join(tmpdir, "t.o")
    with open(raw, "wb") as f:
        f.write(to_bytes(blob_hex))
    subprocess.run([OBJCOPY, "-I", "binary", "-O", "elf64-littleaarch64",
                    "--rename-section", ".data=.text,alloc,code,load,readonly", raw, obj],
                   check=True, capture_output=True)
    r = subprocess.run([OBJDUMP, "-d", "--adjust-vma=0x%x" % vma, obj],
                       capture_output=True, text=True)
    return r.stdout


def main():
    path = sys.argv[1]
    flt = sys.argv[2] if len(sys.argv) > 2 else None
    funcs = parse(path)
    sym = {code: "%s.%s(%d)" % (c, n, a) for c, n, a, code, _ in funcs}
    with tempfile.TemporaryDirectory() as td:
        for cls, name, argc, code, blob in funcs:
            if flt and flt not in name:
                continue
            print("\n===== %s.%s(argc=%d) code=0x%x =====" % (cls, name, argc, code))
            txt = disasm(blob, code, td)
            for ln in txt.splitlines():
                m = INSN.match(ln)
                if not m:
                    continue
                addr = int(m.group(1), 16)
                mn, ops = m.group(2), m.group(3).strip()
                note = ""
                if mn.startswith("b") and "#0x" in ops:
                    try:
                        tgt = int(ops.split("#")[-1], 16)
                    except ValueError:
                        tgt = None
                    if tgt in sym:
                        note = "   ; -> %s" % sym[tgt]
                    elif tgt:
                        note = "   ; -> 0x%x" % tgt
                print("  %08x  %-7s %s%s" % (addr, mn, ops, note))


if __name__ == "__main__":
    main()
