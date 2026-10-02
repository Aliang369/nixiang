# -*- coding: utf-8 -*-
"""Scan the executable PT_LOAD of libil2cpp.so for ARM64 instructions that reference
the exact addresses of the strings "AllocFrame" / "AllocStackSlot".

Self-written AArch64 decoder (no capstone). Handles:
  ADRP                -> page target
  ADD  Xd, Xd, #imm12 -> 0x91000000 / 0x91400000 (+ shift 12)
  LDR  Xd, [Xd, #imm] -> 0xF9400000 (unsigned offset, 64-bit) / 0xF8400000
  LDRSW/LDR literal   -> reported separately
Also builds a function-prologue index (stp x29,x30,[sp,#-N]! / pacibsp / sub sp,sp,#N)
so each hit can be attributed to the function that contains it.
Read-only. mmap + array. No third-party deps.
"""
import mmap, os, json, struct, array, sys

SO   = r"E:\nixiang\06_dumped\冒险与征途\apk_extract\lib\arm64-v8a\libil2cpp.so"
META = r"E:\nixiang\06_dumped\冒险与征途\_elf_meta.json"
OUT  = r"E:\nixiang\06_dumped\冒险与征途\interp_entry_xrefs.txt"

meta  = json.load(open(META))
loads = [p for p in meta["phdrs"] if p["type"] == 1]

def va2off(va):
    for p in loads:
        if p["vaddr"] <= va < p["vaddr"] + p["filesz"]:
            return va - (p["vaddr"] - p["off"]), p
    return None, None
def off2va(off):
    for p in loads:
        if p["off"] <= off < p["off"] + p["filesz"]:
            return off + (p["vaddr"] - p["off"]), p
    return None, None

# ---- executable range: PT_LOAD idx2 (R-X) ----
execp = [p for p in loads if p["flags"] & 1][0]
EXEC_OFF, EXEC_VA, EXEC_SZ = execp["off"], execp["vaddr"], execp["filesz"]
CODE_DELTA = EXEC_VA - EXEC_OFF
print("EXEC segment: off=%#x va=%#x size=%#x delta=%#x" % (EXEC_OFF, EXEC_VA, EXEC_SZ, CODE_DELTA))

# ---- section table for context ----
sects = meta["shdrs"]

f = open(SO, "rb")
mm = mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ)

# ---------------- target strings ----------------
TARGETS = {}
for name, want in (("AllocFrame", 0xd1ca18), ("AllocStackSlot", 0xcf2929)):
    o, p = va2off(want)
    assert o is not None, want
    b = mm[o:o+len(name)]
    assert b.decode() == name, (name, b)
    TARGETS[name] = dict(va=want, off=o, page=want & ~0xFFF, pageoff=want & 0xFFF)
    print("%s: va=%#x off=%#x page=%#x pageoff=%#x" % (name, want, o, want & ~0xFFF, want & 0xFFF))

# ---------------- load code ----------------
code = mm[EXEC_OFF:EXEC_OFF+EXEC_SZ]
n = len(code) // 4
ins = array.array("I")
ins.frombytes(code[:n*4])
print("instructions: %d" % n)

def sex(v, bits):
    m = 1 << (bits-1)
    return (v ^ m) - m

def dec_adrp(w, pc):
    rd = w & 0x1F
    immlo = (w >> 29) & 3
    immhi = (w >> 5) & 0x7FFFF
    imm = sex((immhi << 2) | immlo, 21) << 12
    return rd, (pc & ~0xFFF) + imm

def dec_add_imm(w, rn_val):
    if (w >> 23) & 1:            # sh
        pass
    shift = 12 if ((w >> 22) & 1) else 0
    imm12 = (w >> 10) & 0xFFF
    return imm12 << shift

# ---------------- pass: prologues ----------------
def is_prologue(w):
    # stp x29,x30,[sp,#-imm]!   (pre-index, 64-bit, store, L=0)
    if (w & 0xFFC00000) == 0xA9800000 and (w & 0x1F) == 29 and ((w >> 5) & 0x1F) == 31 and ((w >> 10) & 0x1F) == 30:
        return "stp x29,x30,[sp,#-N]!"
    if w == 0xD503237F:
        return "pacibsp"
    # stp x29,x30,[sp,#N]  (signed offset)  -> 0xA9000000 + imm
    if (w & 0xFFC00000) == 0xA9000000 and (w & 0x1F) == 29 and ((w >> 5) & 0x1F) == 31 and ((w >> 10) & 0x1F) == 30:
        return "stp x29,x30,[sp,#N]"
    # sub sp, sp, #imm  (0xD1000000 | imm<<10 | 31<<5 | 31)
    if (w & 0xFF8003FF) == 0xD10003FF:
        return "sub sp,sp,#N"
    return None

prologues = []
for i in range(n):
    kind = is_prologue(ins[i])
    if kind:
        prologues.append(EXEC_VA + i*4)
prologues.sort()
import bisect
def func_start(va):
    k = bisect.bisect_right(prologues, va) - 1
    return prologues[k] if k >= 0 else None

print("prologues found: %d" % len(prologues))

# ---------------- pass: ADRP xrefs ----------------
hits = []
adrp_hits = []      # bare ADRP to target page
for i in range(n):
    w = ins[i]
    if (w & 0x9F000000) != 0x90000000:
        continue
    pc = EXEC_VA + i*4
    rd, page = dec_adrp(w, pc)
    tgt_pages = {t["page"]: t for t in TARGETS.values()}
    if page not in tgt_pages:
        continue
    t = tgt_pages[page]
    adrp_hits.append((pc, rd, page, t["va"]))
    # look ahead for ADD Xd,Xd,#imm or LDR Xd,[Xd,#imm]
    for j in range(i+1, min(i+14, n)):
        w2 = ins[j]
        if (w2 & 0x1F) != rd:
            continue
        if (w2 & 0x7F800000) == 0x11000000:        # ADD (imm) 32/64
            imm = dec_add_imm(w2, None)
            rn = (w2 >> 5) & 0x1F
            if rn != rd:
                continue
            final = page + imm
            hits.append(dict(kind="ADRP+ADD", at=pc, va=EXEC_VA+j*4, j=j, rd=rd,
                             target=final, base=page, imm=imm, w_pc=w, w_at=w2))
        elif (w2 & 0xFFC00000) == 0xF9400000:      # LDR (imm, 64-bit unsigned)
            imm = ((w2 >> 10) & 0xFFF) * 8
            rn = (w2 >> 5) & 0x1F
            if rn != rd:
                continue
            final = page + imm
            hits.append(dict(kind="ADRP+LDR", at=pc, va=EXEC_VA+j*4, j=j, rd=rd,
                             target=final, base=page, imm=imm, w_pc=w, w_at=w2))
        elif (w2 & 0xFFC00000) == 0xF8400000:      # LDR (imm, 64-bit post/pre-index)
            hits.append(dict(kind="ADRP+LDRp", at=pc, va=EXEC_VA+j*4, j=j, rd=rd,
                             target=None, base=page, imm=None, w_pc=w, w_at=w2))
print("ADRP to target pages: %d ; matched ADD/LDR: %d" % (len(adrp_hits), len(hits)))

# ---------------- pass: ADR / ADRP+ADD via other regs (broad) ----------------
# Also: direct 32-bit literal loads are impossible for 64-bit VA; skip.

# ---------------- render ----------------
def hexdump_words(va, count, before=0):
    i0 = (va - EXEC_VA)//4 - before
    lines = []
    for k in range(count):
        idx = i0 + k
        if idx < 0 or idx >= n:
            continue
        a = EXEC_VA + idx*4
        lines.append("  %#010x (%#010x): %08x" % (a, a - CODE_DELTA if a >= EXEC_VA else a, ins[idx]))
    return lines

def cstr_at(va):
    o, p = va2off(va)
    if o is None:
        return None
    b = o
    end = mm.find(b"\0", o)
    if end < 0 or end - o > 120:
        return None
    try:
        return mm[o:end].decode("utf-8")
    except Exception:
        return None

def name_of(va):
    for k, t in TARGETS.items():
        if t["va"] == va:
            return k
    return None

exact = [h for h in hits if h["target"] is not None and name_of(h["target"])]
near  = [h for h in hits if h["target"] is not None and not name_of(h["target"])]

out = []
out.append("# 字符串 xref 命中点 — libil2cpp.so")
out.append("")
out.append("目标字符串:")
for k, t in TARGETS.items():
    out.append("- `%s`  VA = file_off = %#x  (.rodata, PT_LOAD0 delta=0)" % (k, t["va"]))
out.append("")
out.append("代码段: PT_LOAD2 R-X  file=%#x..%#x  va=%#x..%#x  delta=%#x  (%d 条指令)" %
           (EXEC_OFF, EXEC_OFF+EXEC_SZ, EXEC_VA, EXEC_VA+EXEC_SZ, CODE_DELTA, n))
out.append("")
out.append("解码器: 自写 AArch64 子集 (ADRP / ADD-imm / LDR-imm / STP-prologue)。")
out.append("扫描策略: 先找 ADRP 落在目标页的指令, 再向后最多 13 条指令匹配相同 Rd 的 ADD/LDR 立即数。")
out.append("")
out.append("## 裸 ADRP 命中目标页 — %d 条" % len(adrp_hits))
out.append("")
out.append("| ADRP RVA | Rd | page | 该页目标串 |")
out.append("|---|---|---|---|")
for pc, rd, page, sva in sorted(adrp_hits):
    out.append("| %#x | x%d | %#x | %s @%#x |" % (pc, rd, page, name_of(sva), sva))
out.append("")
out.append("## 精确 xref: ADRP+ADD/LDR 落到**串首地址** — %d 条" % len(exact))
out.append("")
if not exact:
    out.append("**0 条。**  .text / il2cpp AOT 段内不存在任何把 `AllocFrame` (%#x) 或 `AllocStackSlot` (%#x) 的"
               "**串首地址**装进寄存器的指令序列。" % (TARGETS["AllocFrame"]["va"], TARGETS["AllocStackSlot"]["va"]))
else:
    for h in sorted(exact, key=lambda x: x["at"]):
        nm = name_of(h["target"])
        fs = func_start(h["at"])
        out.append("### `%s` 被 %s 引用  RVA %#x" % (nm, h["kind"], h["at"]))
        out.append("- ADRP: RVA %#x word=%08x -> x%d = page %#x" % (h["at"], h["w_pc"], h["rd"], h["base"]))
        out.append("- 落地: RVA %#x word=%08x imm=%#x -> %#x" % (h["va"], h["w_at"], h["imm"], h["target"]))
        out.append("- 所属函数(prologue 推断): %s" % ("%#x" % fs if fs else "unknown"))
        out.append("- 上下文:")
        out.extend(hexdump_words(h["at"], 12))
        out.append("")
out.append("## 页内但非串首的落地 — %d 条 (说明这些页被大量其它串共用)" % len(near))
out.append("")
out.append("| 落点 VA | 相对 AllocFrame/AllocStackSlot | 落点处 C 串 | 指令 RVA | 类型 |")
out.append("|---|---|---|---|---|")
for h in sorted(near, key=lambda x: (x["at"])):
    s = cstr_at(h["target"])
    base = None
    for k, t in TARGETS.items():
        if t["page"] == h["base"]:
            base = t
            nm = k
            break
    rel = h["target"] - base["va"]
    out.append("| %#x | %s%+#x | %s | %#x | %s |" % (h["target"], nm, rel,
               ("`%s`" % s[:60]) if s else "(no NUL/utf8)", h["at"], h["kind"]))
out.append("")

open(OUT, "w", encoding="utf-8").write("\n".join(out) + "\n")
print("wrote", OUT)
print("exact=%d near=%d" % (len(exact), len(near)))
print("\n".join(out[:30]))
