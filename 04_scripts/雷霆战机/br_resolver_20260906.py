#!/usr/bin/env python3
"""BR 跳表解析器：还原 tersafe2 平坦化控制流的真实 CFG。
模式：... mov/movk/orr/eor(常量折叠) ... csel Wm,Wa,Wb,cond ... ldr Xn,[Xbase,Wm,UXTW#3] ... br Xn
Xbase 由 adrp+add 定义。输出每个 br 站点的 (条件, 索引A→目标A, 索引B→目标B)。
"""
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

def rd_reg(op):
    return op.split(",")[0].strip()

def parse_imm(s):
    s = s.strip().lstrip("#")
    try:
        return int(s, 0)
    except ValueError:
        return None

class Lifter:
    """向后小窗口常量折叠：寄存器 → 具体值 or None（未知/依赖内存）。"""
    def __init__(self, insns):
        self.insns = insns

def resolve_br(idx, addr_index_map):
    """从 br 指令 idx 向后最多 60 条做常量折叠，返回 (base, [idx...], cond) or None。"""
    regs = {}      # w寄存器(32位视图) / x寄存器 → 值
    base_reg = None
    idx_reg = None
    cond = None
    window = insns[max(0, idx - 120):idx]
    # 先找 br 的索引寄存器与表基址寄存器
    for j in range(len(window) - 1, -1, -1):
        a, m, o = window[j]
        if m == "ldr" and ("uxtw#3" in o.replace(" ", "") or "lsl#3" in o.replace(" ", "")):
            parts = [p.strip() for p in o.split(",")]
            if len(parts) >= 3 and "[" in o:
                idx_reg = parts[1]
                inner = o[o.index("[") + 1:o.index("]")]
                ip = [p.strip() for p in inner.split(",")]
                base_reg = ip[0]
                break
    if not idx_reg or not base_reg:
        return None
    # 正向折叠（简化：从头跑 window，只追踪 mov/movk/adrp/add/orr/eor/csel/sub/movk）
    r32 = {}
    r64 = {}
    for a, m, o in window:
        parts = [p.strip() for p in o.split(",")]
        dst = parts[0] if parts else None
        if m == "mov" and len(parts) == 2:
            v = parse_imm(parts[1])
            if dst and dst.startswith("w"):
                r32[dst] = v if v is not None else None
                r64.pop("x" + dst[1:], None)
            elif dst and dst.startswith("x"):
                r64[dst] = v
        elif m == "movk" and len(parts) == 2:
            v = parse_imm(parts[1].split("lsl")[0])
            if dst and dst.startswith("w") and r32.get(dst) is not None and v is not None:
                sh = 0
                if "lsl" in parts[1]:
                    try:
                        sh = int(parts[1].split("lsl")[1].strip().lstrip("#"), 0)
                    except ValueError:
                        sh = 0
                r32[dst] = (r32[dst] & 0xFFFF) | (v << sh)
        elif m == "orr" and len(parts) == 3:
            v = parse_imm(parts[2])
            s1v = 0 if parts[1] == 'wzr' else (r32.get(parts[1]) if not parts[1].startswith("#") else parse_imm(parts[1]))
            if dst.startswith("w") and v is not None and s1v is not None:
                r32[dst] = s1v | v
        elif m == "eor" and len(parts) == 3:
            d, s1, s2 = parts
            if d.startswith("w"):
                v1 = r32.get(s1)
                v2 = r32.get(s2) if not s2.startswith("#") else parse_imm(s2)
                if v1 is not None and v2 is not None:
                    r32[d] = v1 ^ v2
        elif m == "add" and len(parts) == 3 and parts[1] == dst and dst.startswith(("w", "x")):
            v = parse_imm(parts[2])
            reg = r32 if dst.startswith("w") else r64
            if v is not None and reg.get(parts[1]) is not None:
                reg[dst] = (reg[parts[1]] + v) & (0xFFFFFFFF if dst.startswith("w") else 0xFFFFFFFFFFFFFFFF)
        elif m == "adrp":
            if dst and dst.startswith("x"):
                page = parse_imm(o.split("#")[1]) if "#" in o else None
                # capstone adrp op_str = "x20, #0x566000"
                r64[dst] = page
        elif m == "csel":
            if len(parts) == 4:
                d, s1, s2, c = parts
                if d.startswith("w"):
                    r32[d] = None  # 运行时决定
                    pending = (r32.get(s1), r32.get(s2), c)
        elif m in ("str", "stur", "stp", "bl", "blr", "cbz", "cbnz", "tbz", "tbnz", "cmp", "ccmp", "ldr", "ldur", "ldp", "mrs", "movi"):
            # 干扰寄存器追踪的指令：非目标寄存器覆盖一般不影响（简化处理）
            pass
    ia = r32.get(idx_reg)
    # 索引寄存器是 csel 结果 → 未知；转而收集 window 里 csel 的两个源值
    csel_pair = None
    for a, m, o in window:
        if m == "csel":
            parts = [p.strip() for p in o.split(",")]
            if len(parts) == 4 and parts[0].startswith("w"):
                s1v = 0 if parts[1] == 'wzr' else (r32.get(parts[1]) if not parts[1].startswith("#") else parse_imm(parts[1]))
                s2v = 0 if parts[2] == 'wzr' else (r32.get(parts[2]) if not parts[2].startswith("#") else parse_imm(parts[2]))
                if s1v is not None and s2v is not None:
                    csel_pair = (s1v, s2v, parts[3], a)
    base = r64.get(base_reg)
    return (base, csel_pair, idx, hex(window[0][0]))

def read_table(base, index):
    off = base + index * 8
    if 0 <= off <= len(b) - 8:
        return struct.unpack_from("<Q", b, off)[0]
    return None

def main():
    br_sites = []
    addr_index_map = {a: i for i, (a, m, o) in enumerate(insns)}
    for i, (a, m, o) in enumerate(insns):
        if m == "br":
            br_sites.append(i)
    print(f"BR 站点总数 {len(br_sites)}", file=sys.stderr)
    out = []
    ok = 0
    for i in br_sites:
        r = resolve_br(i, addr_index_map)
        a = insns[i][0]
        if not r:
            out.append(f"{a:X} BR ?(unresolved)")
            continue
        base, pair, _, w0 = r
        if base is None or pair is None:
            out.append(f"{a:X} BR base={hex(base) if base else '?'} pair={pair}")
            continue
        ia, ib, cond, csel_at = pair
        ta = read_table(base, ia)
        tb = read_table(base, ib)
        out.append(f"{a:X} BR cond={cond} idx={ia}/{ib} → {hex(ta) if ta else '?'}/{hex(tb) if tb else '?'} (csel@{csel_at:X})")
        ok += 1
    (BASE / "br_resolved.txt").write_text("\n".join(out))
    print(f"解析成功 {ok}/{len(br_sites)} → br_resolved.txt", file=sys.stderr)
    # 聚焦热区
    hot = [l for l in out if l.startswith(("b2", "b3", "b4", "b5", "b6", "b7")) or any(l.startswith(h) for h in ("19a", "19b", "19c", "19d"))]
    (BASE / "br_resolved_hot.txt").write_text("\n".join(hot))
    print(f"热区条目 {len(hot)} → br_resolved_hot.txt", file=sys.stderr)

if __name__ == "__main__":
    main()
