#!/usr/bin/env python3
"""BR 跳表解析器 v4：
- 向后数据流 + 常量折叠（literal pool、寄存器按位运算、movk 链、csel 链展开）
- 候选值集合（csel 展开为 {v1,v2}，上游 csel 递归展开）
- 全二进制解析 + 跳表结构输出（graph JSON）
"""
import struct
import sys
import json
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
B = (BASE / "libtersafe2.so").read_bytes()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

INSNS = []
pos, code = 0, B[:0x55A32C]
while pos < len(code):
    got = False
    for ins in md.disasm(code[pos:], pos):
        INSNS.append((ins.address, ins.mnemonic, ins.op_str))
        pos += ins.size
        got = True
    if not got:
        pos += 4
N = len(INSNS)
print(f"指令 {N}", file=sys.stderr)

def pimm(s):
    s = s.strip().lstrip("#")
    if not s:
        return None
    try:
        return int(s, 0) & 0xFFFFFFFFFFFFFFFF
    except ValueError:
        return None

RW_VA = 0x55BCC0  # RW 段 VA 下界；VA≥此值时 file = VA - 0x1000
def va2file(va):
    return va - 0x1000 if va >= RW_VA else va

def lit_read(va, size=4):
    """literal pool 读：按程序头做 VA→file 映射"""
    fo = va2file(va)
    if 0 <= fo <= len(B) - size:
        return int.from_bytes(B[fo:fo + size], "little")
    return None

def tbl_read(base, idx):
    fo = va2file(base + idx * 8)
    if 0 <= fo <= len(B) - 8:
        return struct.unpack_from("<Q", B, fo)[0]
    return None

class Ctx:
    """折叠上下文：值集合语义（每个寄存器 = 候选值集合）"""
    def __init__(self, depth=0):
        self.depth = depth

REG_DEF_OPS = {"mov","movz","movk","orr","eor","and","bic","add","sub","adrp","ldr","ldur",
               "mvn","neg","negs","lsl","lsr","asr","ror","rev","rev16","rev32","sxtw","sxtb","uxtw","uxtb",
               "cset","csinc","csneg","csinv","adr","adds","subs","ands","orns","eon","bif","bit","bsl"}
MEM_STORE_OPS = {"str","stur","stp","strb","sturb","strh","sturb","stnp"}

def values_of(reg, j, depth=0, budget=64, seen=None):
    """求 insns[j] 之前 reg 的候选值集合（set of int）。无法确定 → 空集。"""
    if seen is None:
        seen = set()
    key = (reg, j)
    if key in seen or depth > 5:
        return set()
    seen.add(key)
    k = j - 1
    bud = budget
    while k >= 0 and bud > 0:
        a, m, o = INSNS[k]
        bud -= 1
        parts = [p.strip() for p in o.split(",")]
        dst = parts[0] if parts else ""
        if m in MEM_STORE_OPS or m not in REG_DEF_OPS:
            k -= 1
            continue
        if dst != reg:
            k -= 1
            continue
        if m == "mov":
            v = pimm(parts[1])
            if v is not None:
                return {v}
            if parts[1] in ("xzr", "wzr"):
                return {0}
            if parts[1].startswith(("x", "w")):
                return values_of(parts[1], k, depth + 1, budget, set(seen))
            return set()
        if m in ("movz", "adr", "adrp"):
            v = pimm(parts[1])
            return {v} if v is not None else set()
        if m == "movk":
            v = pimm(parts[1].split("lsl")[0])
            sh = pimm(parts[2].split("lsl")[1]) if len(parts) > 2 and "lsl" in parts[2] else 0
            if v is None:
                return set()
            out = set()
            for prev in values_of(reg, k, depth + 1, budget, set(seen)):
                out.add(((prev & 0xFFFF) | (v << (sh or 0))) & 0xFFFFFFFFFFFFFFFF)
            return out
        if m in ("orr", "eor", "and", "add", "sub", "eon", "orn", "bic") and len(parts) >= 3:
            s1, s2 = parts[1], parts[2]
            def val(x):
                if x in ("xzr", "wzr"):
                    return {0}
                v = pimm(x)
                return {v} if v is not None else values_of(x, k, depth + 1, budget, set(seen))
            out = set()
            for v1 in val(s1):
                for v2 in val(s2):
                    if m in ("orr", "orn"):
                        out.add((v1 | v2) if m == "orr" else (~(v1 | v2)) & 0xFFFFFFFF)
                    elif m in ("eor", "eon"):
                        out.add((v1 ^ v2) if m == "eor" else (~(v1 ^ v2)) & 0xFFFFFFFF)
                    elif m in ("and", "bic", "ands"):
                        out.add((v1 & v2) if m != "bic" else (v1 & ~v2) & 0xFFFFFFFF)
                    elif m == "add" or m == "adds":
                        out.add((v1 + v2) & 0xFFFFFFFF)
                    else:
                        out.add((v1 - v2) & 0xFFFFFFFF)
            return out
        if m in ("lsl", "lsr", "asr", "ror") and len(parts) == 3:
            v2 = pimm(parts[2])
            out = set()
            for v1 in values_of(parts[1], k, depth + 1, budget, set(seen)):
                if v2 is None:
                    continue
                if m == "lsl":
                    out.add((v1 << v2) & 0xFFFFFFFF)
                elif m == "lsr":
                    out.add(v1 >> v2)
                elif m == "asr":
                    out.add(v1 >> v2)
                else:
                    sh = v2 & 31
                    out.add(((v1 >> sh) | (v1 << (32 - sh))) & 0xFFFFFFFF if sh else v1)
            return out
        if m == "mvn" and len(parts) == 2:
            out = set()
            for v1 in values_of(parts[1], k, depth + 1, budget, set(seen)):
                out.add((~v1) & 0xFFFFFFFF)
            return out
        if m == "cset":
            return {0, 1}
        if m in ("adrp",):
            v = pimm(parts[1])
            return {v} if v is not None else set()
        if m in ("ldr", "ldur") and "[" in o and "]" in o:
            inner = o[o.index("[") + 1:o.index("]")]
            ip = [p.strip() for p in inner.split(",")]
            if len(ip) == 1:
                return set()
            if ip[0] in ("sp", "x29"):
                off = pimm(ip[1])
                if off is not None:
                    return slot_values(ip[0], off, k, depth + 1)
                return set()
            bases = values_of(ip[0], k, depth + 1, budget, set(seen))
            off = pimm(ip[1]) if len(ip) > 1 else None
            out = set()
            for bv in bases:
                if off is not None:
                    v = lit_read(bv + off)
                    if v is not None:
                        out.add(v)
            return out
        # 其余定义类指令（ldp/ldrsw/csel/复杂寻址 ldr）→ 未知
        return set()
    return set()

def slot_values(base_reg, off, j, depth=0):
    """找写 [base+off] 的 str/stp，返回值集合。"""
    if depth > 4:
        return set()
    k = j
    bud = 96
    out = set()
    while k >= 0 and bud > 0:
        a, m, o = INSNS[k]
        bud -= 1
        if m in ("str", "stur", "stp") and "[" in o and "]" in o:
            val_part = o[:o.index("[")].strip().rstrip(",")
            inner = o[o.index("[") + 1:o.index("]")]
            ip = [p.strip() for p in inner.split(",")]
            if ip and ip[0] == base_reg and len(ip) >= 2:
                soff = pimm(ip[1])
                if soff is not None:
                    regs = [p.strip() for p in val_part.split(",")]
                    if m == "stp" and len(regs) == 2:
                        if soff == off:
                            out |= values_of(regs[0], k, depth + 1)
                        if soff + 4 == off:
                            out |= values_of(regs[1], k, depth + 1)
                    elif regs and soff == off:
                        out |= values_of(regs[0], k, depth + 1)
        if m == "bl":
            break
        k -= 1
    return out

def resolve_br(idx):
    a, m, o = INSNS[idx]
    tgt_reg = base_reg = idx_reg = None
    ldr_j = None
    for j in range(idx - 1, max(-1, idx - 24), -1):
        a2, m2, o2 = INSNS[j]
        if m2 in ("ldr", "ldur") and "[" in o2 and "]" in o2 and ("uxtw #3" in o2 or "sxtw #3" in o2 or "lsl #3" in o2):
            inner = o2[o2.index("[") + 1:o2.index("]")]
            ip = [p.strip() for p in inner.split(",")]
            tgt_reg = o2.split(",")[0].strip()
            base_reg = ip[0]
            idx_reg = ip[1] if len(ip) > 1 else None
            ldr_j = j
            break
    if not idx_reg:
        return None
    bases = values_of(base_reg, ldr_j, budget=4096)
    if not bases:
        return ("nobase",)
    # idx 寄存器定义：找最近的 csel 或其它定义
    csel_j = None
    for j in range(ldr_j - 1, max(-1, ldr_j - 40), -1):
        a3, m3, o3 = INSNS[j]
        if o3.split(",")[0].strip() == idx_reg:
            if m3 == "csel":
                csel_j = j
            break
    if csel_j is None:
        # 非csel定义（mov/ldr）→ 单值
        vs = values_of(idx_reg, ldr_j)
        if not vs:
            return ("noimm", tuple(bases))
        edges = []
        for b0 in bases:
            for v in vs:
                edges.append((v, tbl_read(b0, v)))
        return ("ok", tuple(bases), [ (v, v) for v in vs ], "?", edges, hex(a))
    parts = [p.strip() for p in INSNS[csel_j][2].split(",")]
    cond = parts[3]
    s1, s2 = parts[1], parts[2]
    v1s = {0} if s1 == "wzr" else values_of(s1, csel_j)
    v2s = {0} if s2 == "wzr" else values_of(s2, csel_j)
    if not v1s or not v2s:
        return ("noimm", tuple(bases), cond, hex(a))
    edges = []
    pairs = []
    for b0 in bases:
        for v1 in v1s:
            for v2 in v2s:
                pairs.append((v1, v2))
                edges.append((v1, tbl_read(b0, v1)))
                edges.append((v2, tbl_read(b0, v2)))
    return ("ok", tuple(bases), pairs, cond, edges, hex(INSNS[csel_j][0]))

def main():
    addr_of = {}
    out_lines = []
    graph = {}
    stats = {"ok": 0, "nobase": 0, "noimm": 0, "skip": 0, "total": 0}
    for i, (a, m, o) in enumerate(INSNS):
        if m != "br":
            continue
        if not (0xA9AF0 <= a < 0x43113C):  # 排除 .plt
            continue
        stats["total"] += 1
        ah = f"{a:X}"
        r = resolve_br(i)
        if r is None:
            stats["skip"] += 1
            continue
        if r[0] == "ok":
            _, bases, pairs, cond, edges, cj = r
            ok_e = [(v, t) for v, t in edges if t is not None and 0 <= t < 0x55A32C]
            bad = len(edges) - len(ok_e)
            stats["ok"] += 1
            tg = sorted({t for _, t in ok_e})
            graph[ah] = {"cond": cond, "cand": sorted({v for pr in pairs for v in pr}), "targets": [f"{t:X}" for t in tg], "csel": cj}
            out_lines.append(f"{ah} BR cond={cond} → {', '.join(f'{t:X}' for t in tg)}" + (f"  (+{bad} 越界)" if bad else ""))
        elif r[0] == "nobase":
            stats["nobase"] += 1
            out_lines.append(f"{ah} BR nobase")
        else:
            stats["noimm"] += 1
            out_lines.append(f"{ah} BR noimm cond={r[2] if len(r)>2 else ''}")
    (BASE / "br_resolved4.txt").write_text("\n".join(out_lines))
    (BASE / "br_graph.json").write_text(json.dumps(graph, indent=0))
    print(f"{stats}", file=sys.stderr)

if __name__ == "__main__":
    main()
