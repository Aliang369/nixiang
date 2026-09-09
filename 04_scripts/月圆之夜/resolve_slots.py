#!/usr/bin/env python3.11
# 解析 LoadDLL.* 函数的 metadata usage 槽 (内存镜像运行时值 → 类名/字符串), v2
import struct
from capstone import *
from capstone.arm64 import ARM64_OP_MEM, ARM64_OP_IMM

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
MEM = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_mem_full.bin"
BASE = 0x774A046000
d = open(SO, "rb").read()
m = open(MEM, "rb").read()
MSZ = len(m)


def off(addr):
    o = addr - BASE
    return o if 0 <= o <= MSZ - 8 else None


def u64a(addr):
    o = off(addr)
    return struct.unpack_from("<Q", m, o)[0] if o is not None else 0


def u64v(va):
    """模块内 VA 直接读文件 (dump 文件偏移 == 模块 VA)"""
    return struct.unpack_from("<Q", m, va)[0] if 0 <= va <= MSZ - 8 else 0


def cstr_at(addr, maxn=80):
    o = off(addr)
    if o is None:
        return None
    e = m.find(b"\x00", o, o + maxn)
    if e < 0:
        return None
    try:
        s = m[o:e].decode("ascii")
    except Exception:
        return None
    return s if s and all(32 <= ord(c) < 127 for c in s) else None


def ustr_at(addr):
    o = off(addr)
    if o is None:
        return None
    for loff in (0x10, 0x8):
        p = o + loff
        if p + 4 > MSZ:
            continue
        ln = struct.unpack_from("<i", m, p)[0]
        if 0 < ln < 512 and p + 4 + ln * 2 <= MSZ:
            try:
                s = m[p + 4:p + 4 + ln * 2].decode("utf-16-le")
                if s and all(32 <= ord(c) < 0xFFEF for c in s):
                    return (loff, s)
            except Exception:
                pass
    return None


md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
md.detail = True
FUNCS = {"LoadHotfix": (0x1D9F134, 0x1D9F2C4), "DecompressBytes": (0x1D9F454, 0x1D9F990),
         "LoadAssemble": (0x1D9F990, 0x1DA0F00)}


def imm_of(op):
    tv = getattr(op, "imm", None)
    if tv is None:
        tv = getattr(op, "value", None)
    if isinstance(tv, bytes):
        tv = int.from_bytes(tv, "little")
    return int(tv) & 0xFFFFFFFFFFFFFFFF


slots = {}
for fn, (a, b) in FUNCS.items():
    adrp_val = {}
    for i in md.disasm(d[a:b], a):
        dst = i.operands[0].reg if i.operands else None
        if i.mnemonic == "adrp":
            adrp_val[i.operands[0].reg] = imm_of(i.operands[1])
            continue
        if (i.mnemonic.startswith("ldr") or i.mnemonic.startswith("ldp") or i.mnemonic.startswith("adr")
                or i.mnemonic in ("mov", "movz", "add", "bl")):
            pass
        if i.mnemonic == "ldr" and len(i.operands) == 2 and i.operands[1].type == ARM64_OP_MEM:
            base = i.operands[1].mem.base
            if base in adrp_val:
                slots.setdefault(adrp_val[base] + i.operands[1].mem.disp, set()).add(fn)
                if base == dst:
                    del adrp_val[base]          # 目的寄存器覆盖 → 基址失效
                    continue
        # 任何写寄存器的指令都使该寄存器的 adrp 基址失效
        if dst is not None and dst in adrp_val and i.mnemonic != "adrp":
            del adrp_val[dst]

print("== usage 槽运行时解析 (va=槽地址, mem=运行时值) ==")
for va in sorted(slots):
    fns = ",".join(sorted(slots[va]))
    v_mem = u64v(va)
    note = ""
    if off(v_mem) is not None:
        # ① 槽值即 Il2CppString*/Il2CppClass*
        cs = cstr_at(v_mem) or cstr_at(u64a(v_mem + 0x0))
        us = ustr_at(v_mem)
        cls = None
        for noff in (0x0, 0x10):
            p = u64a(v_mem + noff)
            s = cstr_at(p)
            if s:
                cls = (noff, s)
                break
        parts = []
        if cs:
            parts.append("cstr=%r" % cs)
        if us:
            parts.append("ustr@%s=%r" % (hex(us[0]), us[1]))
        if cls:
            parts.append("cls[+%x].name=%r" % cls)
        note = " :: " + " | ".join(parts) if parts else " :: (ptr 0x%X)" % v_mem
    print("  0x%-8X (%s) mem=0x%012X%s" % (va, fns, v_mem, note))
