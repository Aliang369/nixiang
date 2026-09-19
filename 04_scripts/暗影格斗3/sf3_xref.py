#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""sf3_xref.py — IL2CPP libil2cpp.so 的 BL/B 直接调用交叉引用扫描器。

用途：给定若干目标 RVA（来自 Il2CppDumper 的 dump.cs），列出 .so 中所有
直接 `bl` / `b` 到这些地址的指令位置，并把调用点归位到「所属类 + 方法」。

不依赖 capstone：AArch64 的 BL/B 是固定 32 位编码，直接扫描 4 字节对齐的字即可。

用法：
    python3 sf3_xref.py --so libil2cpp.so --dump dump.cs 0x6016F04 0x6026A28
    python3 sf3_xref.py --so libil2cpp.so --dump dump.cs --json out.json 0x6016F04

输出：按目标分组的调用点列表，形如
    0x6016F04  KLFPIPINPKN(int)   [JKJBPEKEBLN]
      0x5B642C4  <- SetCurrencyType(int)  [CurrencyPresenter]  (b)
"""

import argparse
import json
import os
import re
import struct
import sys

# AArch64 顶层操作码：B = 000101, BL = 100101
_OP_B = 0b000101
_OP_BL = 0b100101

_RVA_RE = re.compile(r"//\s*RVA:\s*0x([0-9A-Fa-f]+)\s+Offset:\s*0x([0-9A-Fa-f]+)")
_CLASS_RE = re.compile(
    r"^\s*(?:public|internal|private|protected)\s+"
    r"(?:static\s+|sealed\s+|abstract\s+|readonly\s+)*"
    r"(class|struct|enum|interface)\s+([A-Za-z_<>][\w.<>]*)"
)


def parse_elf_loads(path):
    """返回 [(offset, vaddr, filesz, flags), ...]，只保留 PT_LOAD。"""
    with open(path, "rb") as fh:
        head = fh.read(64)
    if head[:4] != b"\x7fELF":
        raise SystemExit("not an ELF file: %s" % path)
    is64 = head[4] == 2
    little = head[5] == 1
    if not is64 or not little:
        raise SystemExit("only 64-bit little-endian ELF supported")
    e_phoff = struct.unpack_from("<Q", head, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", head, 0x36)[0]
    e_phnum = struct.unpack_from("<H", head, 0x38)[0]
    loads = []
    with open(path, "rb") as fh:
        fh.seek(e_phoff)
        for _ in range(e_phnum):
            ph = fh.read(e_phentsize)
            if len(ph) < 56:
                break
            p_type = struct.unpack_from("<I", ph, 0)[0]
            if p_type != 1:  # PT_LOAD
                continue
            p_offset = struct.unpack_from("<Q", ph, 0x08)[0]
            p_vaddr = struct.unpack_from("<Q", ph, 0x10)[0]
            p_filesz = struct.unpack_from("<Q", ph, 0x20)[0]
            p_flags = struct.unpack_from("<I", ph, 0x04)[0]
            loads.append((p_offset, p_vaddr, p_filesz, p_flags))
    loads.sort()
    return loads


def call_sites(so_path, loads, targets):
    """扫描 .so，返回 {target: [(caller_vaddr, op), ...]}。"""
    hits = {t: [] for t in targets}
    with open(so_path, "rb") as fh:
        data = fh.read()
    for (off, vaddr, filesz, flags) in loads:
        if not (flags & 0x1):  # PF_X
            continue
        start = off
        end = min(off + filesz, len(data))
        # 保证 4 字节对齐（LOAD 段本身按 4 对齐）
        start += (-start) % 4
        for pos in range(start, end - 3, 4):
            word = struct.unpack_from("<I", data, pos)[0]
            op = word >> 26
            if op != _OP_B and op != _OP_BL:
                continue
            imm = word & 0x03FFFFFF
            if imm & 0x02000000:
                imm -= 0x04000000
            pc = vaddr + (pos - off)
            target = pc + (imm << 2)
            bucket = hits.get(target)
            if bucket is not None:
                bucket.append((pc, "bl" if op == _OP_BL else "b"))
    for t in hits:
        hits[t].sort()
    return hits


def parse_dump(dump_path):
    """返回 (methods, owners)：
    methods = [(rva, signature, line_no), ...] 已排序
    owners  = [(line_no, "class X"), ...] 已排序（用于归属）
    """
    methods = []
    owners = []
    pending_rva = None
    with open(dump_path, "r", encoding="utf-8", errors="replace") as fh:
        for lineno, line in enumerate(fh, 1):
            if pending_rva is not None:
                sig = line.strip()
                if sig:
                    methods.append((pending_rva, sig, lineno - 1))
                pending_rva = None
                # 继续往下走，这一行也可能是类声明（不会同时成立）
            m = _RVA_RE.search(line)
            if m:
                pending_rva = int(m.group(1), 16)
                continue
            c = _CLASS_RE.match(line)
            if c:
                owners.append((lineno, "%s %s" % (c.group(1), c.group(2))))
    methods.sort()
    owners.sort()
    return methods, owners


def owner_of(owners, lineno):
    import bisect
    idx = bisect.bisect_right(owners, (lineno, "\uffff")) - 1
    if idx < 0:
        return "?"
    return owners[idx][1]


def method_of(methods, addr):
    import bisect
    idx = bisect.bisect_right(methods, (addr, "\uffff", 0)) - 1
    if idx < 0:
        return None
    rva, sig, lineno = methods[idx]
    if rva > addr:
        return None
    return {
        "rva": rva,
        "sig": sig,
        "line": lineno,
        "delta": addr - rva,
    }


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--so", required=True)
    ap.add_argument("--dump", required=True)
    ap.add_argument("--json")
    ap.add_argument("--only-methods", action="store_true",
                    help="只显示命中方法起始地址的调用点")
    ap.add_argument("targets", nargs="+", help="目标 RVA，如 0x6016F04")
    args = ap.parse_args()

    targets = []
    for t in args.targets:
        targets.append(int(t, 16) & 0xFFFFFFFFFFFFFFFF)

    loads = parse_elf_loads(args.so)
    hits = call_sites(args.so, loads, targets)
    methods, owners = parse_dump(args.dump)

    report = {}
    for t in targets:
        tgt = method_of(methods, t)
        print("=" * 78)
        if tgt:
            print("TARGET 0x%X %s  [%s]  (dump.cs:%d)"
                  % (t, tgt["sig"], owner_of(owners, tgt["line"]), tgt["line"]))
        else:
            print("TARGET 0x%X  (dump.cs 中无对应方法)" % t)
        rows = []
        for (caller, op) in hits[t]:
            m = method_of(methods, caller)
            if args.only_methods and m and m["delta"] != 0:
                continue
            if m:
                who = "%s  [%s]  +0x%X" % (m["sig"], owner_of(owners, m["line"]), m["delta"])
                rows.append((caller, op, who))
            else:
                rows.append((caller, op, "(未知函数)"))
        if not rows:
            print("  (无直接调用)")
        else:
            print("  命中 %d 处：" % len(rows))
            for (caller, op, who) in rows:
                print("    0x%08X  %-3s <- %s" % (caller, op, who))
        report["0x%X" % t] = [
            {"caller": "0x%X" % c, "op": o, "who": w} for (c, o, w) in rows
        ]

    if args.json:
        with open(args.json, "w", encoding="utf-8") as fh:
            json.dump(report, fh, ensure_ascii=False, indent=2)
        print("\n已写：%s" % args.json)
    return 0


if __name__ == "__main__":
    sys.exit(main())
