#!/usr/bin/env python3
"""第四遍：完整反汇编 tersafe2 ptrace 簇1（0x124900–0x125200）+ farm 加宽 xref + tprt 间接 svc。
输出: cluster1_disasm.txt + 控制台摘要
"""
import json
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
b = (BASE / "libtersafe2.so").read_bytes()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

def disasm_range(lo, hi):
    out, pos = [], lo
    while pos < hi:
        got = False
        for ins in md.disasm(b[pos:hi], pos):
            out.append(f"  {pos:#08x}  {ins.mnemonic:<8} {ins.op_str}")
            pos += ins.size
            got = True
        if not got:
            out.append(f"  {pos:#08x}  (bad)")
            pos += 4
    return "\n".join(out)

lines = ["=== ptrace 簇1 0x124900-0x125200 ===", disasm_range(0x124900, 0x125200)]
lines += ["", "=== 簇1 外围 0x125200-0x125500 ===", disasm_range(0x125200, 0x125500)]
(BASE / "cluster1_disasm.txt").write_text("\n".join(lines))
print("cluster1_disasm.txt 写出")

# farm 加宽 xref：bl/b 目标在 svc-0x60..svc；回看 16 条找 w0/x0 立即数
sites = json.loads((BASE / "libtersafe2.so.svc_enum.json").read_text())["sites"]
farm = sorted(int(s["off"], 16) for s in sites if 0x3f8300 <= int(s["off"], 16) <= 0x3f9200)
insns = []
pos, code = 0, b[:0x55a32c]
while pos < len(code):
    got = False
    for ins in md.disasm(code[pos:], pos):
        insns.append((ins.address, ins.mnemonic, ins.op_str))
        pos += ins.size
        got = True
    if not got:
        pos += 4
calls = []
for i, (a, m, o) in enumerate(insns):
    if m not in ("bl", "b"):
        continue
    try:
        tgt = int(o.replace("#", ""), 16)
    except ValueError:
        continue
    for svc_a in farm:
        if 0 < svc_a - tgt <= 0x60:
            nr = None
            for j in range(i - 1, max(-1, i - 17), -1):
                _, mo, mops = insns[j]
                if mo == "mov" and mops.split(",")[0].strip() in ("w0", "x0") and "#" in mops:
                    try:
                        nr = int(mops.split("#")[1].split(",")[0], 0)
                    except ValueError:
                        nr = -1
                    break
                if mo in ("bl", "b", "ret", "br", "blr"):
                    break
            calls.append((hex(a), m, hex(svc_a), nr))
            break
print(f"\n加宽后 farm 调用点: {len(calls)}")
hist = {}
for c in calls:
    hist.setdefault(c[3], []).append(c)
for nr, lst in sorted(hist.items(), key=lambda kv: (kv[0] is None, kv[0] if kv[0] is not None else 0)):
    print(f"  nr={nr}: {len(lst)} 处  例: {lst[0][0]}->{lst[0][2]}")

# tprt 间接 svc 上下文
bt = (BASE / "libtprt.so").read_bytes()
lines2 = ["=== tprt 间接 svc @0x146bfc ±0x80 ===", disasm_range(0x146bfc - 0x80, 0x146bfc + 0x40)]
(BASE / "tprt_indirect_svc.txt").write_text("\n".join(lines2))
print("tprt_indirect_svc.txt 写出")
