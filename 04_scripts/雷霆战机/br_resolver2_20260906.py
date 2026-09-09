#!/usr/bin/env python3
"""BR 跳表解析器 v2：向后数据流求解（块内局部，避免跨块污染）。"""
import struct
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
b = (BASE / "libtersafe2.so").read_bytes()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

insns = []
pos, code = 0, b[:0x55A32C]
while pos < len(code):
    got = False
    for ins in md.disasm(code[pos:], pos):
        insns.append((ins.address, ins.mnemonic, ins.op_str))
        pos += ins.size
        got = True
    if not got:
        pos += 4
print(f"指令总数 {len(insns)}", file=sys.stderr)

def parse_imm(s):
    s = s.strip().lstrip("#")
    try:
        return int(s, 0) & 0xFFFFFFFFFFFFFFFF
    except ValueError:
        return None

def value_of(reg, j, depth=0, budget0=48):
    """求 insns[j] 之前寄存器 reg 的常量值：向后找最近定义，递归折叠。depth 防循环。"""
    if depth > 6 or j < 0:
        return None
    k = j - 1
    budget = budget0
    while k >= 0 and budget > 0:
        a, m, o = insns[k]
        parts = [p.strip() for p in o.split(",")]
        dst = parts[0] if parts else ""
        budget -= 1
        if m == "mov" and dst == reg:
            v = parse_imm(parts[1])
            if v is not None:
                return v
            if parts[1] in ("x29", "sp"):
                return None
            return value_of(parts[1], k, depth + 1)
        if m == "movz" and dst == reg:
            return parse_imm(parts[1])
        if m == "movk" and dst == reg:
            v = parse_imm(parts[1].split("lsl")[0])
            sh = 0
            if "lsl" in parts[1]:
                sh = parse_imm(parts[1].split("lsl")[1]) or 0
            prev = value_of(reg, k, depth + 1)
            if prev is None or v is None:
                return None
            return (prev & 0xFFFF) | (v << sh) & 0xFFFFFFFFFFFFFFFF
        if m == "orr" and dst == reg:
            v2 = parse_imm(parts[2])
            v1 = value_of(parts[1], k, depth + 1) if not parts[1].startswith("#") else parse_imm(parts[1])
            if v1 is None or v2 is None:
                return None
            return v1 | v2
        if m == "eor" and dst == reg:
            v1 = value_of(parts[1], k, depth + 1) if not parts[1].startswith("#") else parse_imm(parts[1])
            v2 = value_of(parts[2], k, depth + 1) if not parts[2].startswith("#") else parse_imm(parts[2])
            if v1 is None or v2 is None:
                return None
            return v1 ^ v2
        if m in ("add", "sub") and dst == reg:
            v2 = parse_imm(parts[2])
            v1 = value_of(parts[1], k, depth + 1) if not parts[1].startswith("#") else parse_imm(parts[1])
            if m == "add" and len(parts) >= 4:
                v2b = parse_imm(parts[3]) if parts[3].startswith("#") else None
                if v1 is not None and v2 is not None and v2b is not None:
                    return v1 + v2 + v2b
                return None
            if v1 is None or v2 is None:
                return None
            return (v1 + v2) if m == "add" else (v1 - v2)
        if m == "adrp" and dst == reg:
            return parse_imm(parts[1])
        if dst == reg and m in ("ldr", "ldur") and "[" in o:
            inner = o[o.index("[") + 1:o.index("]")]
            ip = [p.strip() for p in inner.split(",")]
            if len(ip) == 2 and ip[0] in ("sp", "x29"):
                try:
                    off = parse_imm(ip[1])
                except Exception:
                    off = None
                if off is not None:
                    # 向后继续扫最近 64 条找写该槽的 str/stp
                    val = slot_value(ip[0], off, k)
                    if val is not None:
                        return val
            return None
        if dst == reg and m in ("ldp", "ldrsw", "blr", "bl"):
            return None
        k -= 1
    return None

def slot_value(base_reg, off, j, depth=0):
    """在 insns[j] 之前找写 [base_reg+off] 的 str/stp 并折叠值。stp 存相邻双槽。"""
    if depth > 3:
        return None
    k = j
    budget = 64
    while k >= 0 and budget > 0:
        a, m, o = insns[k]
        budget -= 1
        if m in ("str", "stur", "stp") and "[" in o and "]" in o:
            val_part = o[:o.index("[")].strip().rstrip(",")
            inner = o[o.index("[") + 1:o.index("]")]
            ip = [p.strip() for p in inner.split(",")]
            if ip[0] == base_reg and len(ip) >= 2:
                soff = parse_imm(ip[1])
                if soff is not None:
                    regs = [p.strip() for p in val_part.split(",")]
                    if m == "stp" and len(regs) == 2:
                        if soff == off:
                            return value_of(regs[0], k, depth + 1)
                        if soff + 4 == off:
                            return value_of(regs[1], k, depth + 1)
                    elif len(regs) >= 1 and soff == off:
                        return value_of(regs[0], k, depth + 1)
        if m == "bl" and k < j:
            return None
        k -= 1
    return None


def resolve_br(idx):
    a, m, o = insns[idx]
    # 1) 找 br 的 ldr：紧邻的 ldr Xn,[Xb,Wm,uxtw#3]（通常在 br 前 1-2 条）
    tgt_reg = base_reg = idx_reg = None
    for j in range(idx - 1, max(-1, idx - 6), -1):
        a2, m2, o2 = insns[j]
        if m2 == "ldr" and "[" in o2 and ("uxtw #3" in o2 or "sxtw #3" in o2 or "lsl #3" in o2):
            inner = o2[o2.index("[") + 1:o2.index("]")]
            ip = [p.strip() for p in inner.split(",")]
            tgt_reg = o2.split(",")[0].strip()
            base_reg = ip[0]
            idx_reg = ip[1] if len(ip) > 1 else None
            break
    if not idx_reg:
        return None
    # 2) base：从 ldr 位置向后找 base_reg 定义
    ldr_j = next(j for j in range(idx - 1, idx - 6, -1)
                 if insns[j][1] == "ldr" and "[" in insns[j][2] and ("uxtw #3" in insns[j][2] or "sxtw #3" in insns[j][2] or "lsl #3" in insns[j][2]))
    base = value_of(base_reg, ldr_j, budget0=4096)
    if base is None:
        return ("nobase",)
    # 3) idx：找定义 idx_reg 的 csel（ldr 之前最近）
    csel_j = None
    for j in range(ldr_j - 1, max(-1, ldr_j - 30), -1):
        a3, m3, o3 = insns[j]
        if m3 == "csel":
            parts = [p.strip() for p in o3.split(",")]
            if parts[0] == idx_reg:
                csel_j = j
                break
        if o3.split(",")[0].strip() == idx_reg and insns[j][1] in ("mov", "movk", "orr", "eor", "add"):
            break
    if csel_j is None:
        return ("nocsel", base)
    parts = [p.strip() for p in insns[csel_j][2].split(",")]
    cond = parts[3]
    s1, s2 = parts[1], parts[2]
    v1 = 0 if s1 == "wzr" else value_of(s1, csel_j)
    v2 = 0 if s2 == "wzr" else value_of(s2, csel_j)
    if v1 is None or v2 is None:
        return ("noimm", base, cond, hex(insns[csel_j][0]))
    def rd(base_, ix):
        off = base_ + ix * 8
        if 0 <= off <= len(b) - 8:
            return struct.unpack_from("<Q", b, off)[0]
        return None
    return ("ok", base, v1, v2, cond, hex(insns[csel_j][0]), rd(base, v1), rd(base, v2))

def main():
    out = []
    ok = 0
    total = 0
    for i, (a, m, o) in enumerate(insns):
        if m != "br":
            continue
        total += 1
        r = resolve_br(i)
        ah = f"{a:X}"
        if not r:
            out.append(f"{ah} BR ?")
        elif r[0] == "ok":
            _, base, v1, v2, cond, cj, t1, t2 = r
            s1h = f"{t1:X}" if isinstance(t1, int) else "?"
            s2h = f"{t2:X}" if isinstance(t2, int) else "?"
            out.append(f"{ah} BR cond={cond} idx={v1}/{v2} → {s1h} | {s2h} (base={base:X} csel@{cj})")
            ok += 1
        else:
            out.append(f"{ah} BR {r[0]} {r[1:] if len(r)>1 else ''}")
    (BASE / "br_resolved2.txt").write_text("\n".join(out))
    print(f"解析成功 {ok}/{total} → br_resolved2.txt", file=sys.stderr)

if __name__ == "__main__":
    main()
