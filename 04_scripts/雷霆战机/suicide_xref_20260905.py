#!/usr/bin/env python3
"""第五遍：xref 自杀机器入口（簇1 编排 0x124940 等 + 簇2 成员），找触发方。"""
import json
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
b = (BASE / "libtersafe2.so").read_bytes()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

TARGETS = {
    0x124940: "簇1编排(enum TID→SIG_DFL→worker→_exit127)",
    0x12498c: "TID枚举",
    0x124a5c: "信号重置(SIG_DFL×7+SIGCHLD+SIGHUP)",
    0x124b14: "per-TID循环",
    0x124e50: "worker-A(ptrace)",
    0x125058: "worker-B(ptrace)",
    0x1251a8: "worker-C(ptrace)",
    0x1252a0: "簇2-1",
    0x12b580: "簇2-2",
    0x12b9fc: "簇2-3",
    0x12ba60: "簇2-4",
    0x12bcbc: "簇2-5",
    0x12d7fc: "簇2-6",
    0x141320: "簇2-7",
    0x125384: "kill@plt调用1(STOP)",
    0x1253b8: "kill@plt调用2(CONT)",
    0x3d7110: "ACE线程入口(§10.1)",
}

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

print(f"反汇编指令数: {len(insns)}")
xrefs = {t: [] for t in TARGETS}
for i, (a, m, o) in enumerate(insns):
    if m not in ("bl", "b"):
        continue
    try:
        tgt = int(o.replace("#", ""), 16)
    except ValueError:
        continue
    if tgt in xrefs:
        xrefs[tgt].append((hex(a), m))

for t, name in sorted(TARGETS.items()):
    xs = xrefs[t]
    print(f"\n▶ {t:#x} {name}: {len(xs)} 个直接调用方")
    for a, m in xs[:12]:
        print(f"    {m.upper()} ← {a}")

# 附加：adrp+add 对 0x124940 地址常量引用（函数指针/注册表形式）
print("\n--- 地址常量引用扫描（movz/movk 组合或字面量池）---")
import struct
for t in (0x124940, 0x124e50, 0x125058, 0x12b580, 0x3d7110):
    pat = struct.pack("<Q", t)
    hits = []
    off = 0
    while True:
        i = b.find(pat, off)
        if i < 0:
            break
        hits.append(hex(i))
        off = i + 1
    # 也找高低半拆（movk 组合）在代码里
    print(f"  {t:#x}: 8字节字面量 {len(hits)} 处 {hits[:6]}")
