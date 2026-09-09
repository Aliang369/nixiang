#!/usr/bin/env python3
"""checker hunt：函数边界 + 调用图 + PLT→符号名解析，解剖文件读取助手的 7 个调用方。"""
import struct
from pathlib import Path
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

BASE = Path("/Users/aliang/Documents/nixiang/03_decompiled/雷霆战机/reanalysis_20260905")
b = (BASE / "libtersafe2.so").read_bytes()

# ---------- ELF：段表 + 动态符号 + .rela.plt ----------
shoff = struct.unpack_from("<Q", b, 0x28)[0]
shentsize, shnum, shstrndx = struct.unpack_from("<HHH", b, 0x3A)
secs = {}
shstr = None
raw_secs = []
for i in range(shnum):
    o = shoff + i * shentsize
    name, typ, flags, addr, off, size, link, info, align, entsz = struct.unpack_from("<IIQQQQIIQQ", b, o)
    raw_secs.append((name, typ, addr, off, size, link, entsz))
strtab_off = raw_secs[shstrndx][3]
def secname(n):
    e = b.index(b"\0", strtab_off + n)
    return b[strtab_off + n:e].decode()
S = {}
for name, typ, addr, off, size, link, entsz in raw_secs:
    S[secname(name)] = (typ, addr, off, size, link, entsz)

# dynsym → name（含未定义导入）
_, _, ds_off, ds_size, ds_link, ds_ent = S[".dynsym"]
_, _, st_off, _, _, _ = S[".dynstr"]
symtab = []
for i in range(ds_size // ds_ent):
    o = ds_off + i * ds_ent
    st_name, st_info, st_other, st_shndx, st_value, st_size = struct.unpack_from("<IBBHQQ", b, o)
    e = b.index(b"\0", st_off + st_name)
    symtab.append(b[st_off + st_name:e].decode(errors="replace"))

# .rela.plt → GOT 槽 → 符号名
_, _, rp_off, rp_size, rp_link, rp_ent = S[".rela.plt"]
got2sym = {}
for i in range(rp_size // rp_ent):
    o = rp_off + i * rp_ent
    r_offset, r_info, r_addend = struct.unpack_from("<QQq", b, o)
    symidx = r_info >> 32
    got2sym[r_offset] = symtab[symidx] if symidx < len(symtab) else f"sym{symidx}"

# .plt：plt0=32B，其后每 16B 一项，对应 .rela.plt 顺序（ADRPGOT 式，按序对应）
_, plt_addr, plt_off, plt_size, _, _ = S[".plt"]
plt_map = {}  # plt entry VA → symbol name
n_reloc = rp_size // rp_ent
for i in range(min((plt_size - 32) // 16, n_reloc)):
    plt_map[plt_addr + 32 + i * 16] = got2sym.get(rp_off + i * rp_ent and struct.unpack_from("<QQq", b, rp_off + i * rp_ent)[0], f"?{i}")
# 上面太绕，重算：第 i 个 plt entry 对应 .rela.plt 第 i 项
plt_map = {}
for i in range(min((plt_size - 32) // 16, n_reloc)):
    o = rp_off + i * rp_ent
    r_offset, r_info, _ = struct.unpack_from("<QQq", b, o)
    symidx = r_info >> 32
    plt_map[plt_addr + 32 + i * 16] = symtab[symidx] if symidx < len(symtab) else f"?{i}"

# ---------- 反汇编 + 函数边界 ----------
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

bl_targets = set()
for a, m, o in insns:
    if m == "bl":
        try:
            bl_targets.add(int(o.replace("#", ""), 16))
        except ValueError:
            pass
starts = sorted(bl_targets)

def containing(addr):
    lo, hi = 0, len(starts)
    best = 0
    for s in starts:
        if s <= addr:
            best = s
        else:
            break
    return best

def func_end(start):
    i = 0
    for j, s in enumerate(starts):
        if s == start:
            i = j
            break
    return starts[i + 1] if i + 1 < len(starts) else 0x55A32C

# ---------- 解剖 7 个调用方 ----------
CALLERS = [0x29E674, 0x3483D0, 0x36D030, 0x36F480, 0x374D2C, 0x376638, 0x37BB10]
import_names = plt_map  # plt VA -> name
report = []
for c in CALLERS:
    fs = containing(c)
    fe = func_end(fs)
    body = [(a, m, o) for a, m, o in insns if fs <= a < fe]
    bls = []
    loops = 0
    svc_in = []
    for a, m, o in body:
        if m == "bl":
            try:
                t = int(o.replace("#", ""), 16)
                bls.append((hex(a), import_names.get(t, hex(t))))
            except ValueError:
                pass
        if m.startswith("b.") or m == "b":
            try:
                t = int(o.replace("#", ""), 16)
                if fs <= t < a:
                    loops += 1
            except ValueError:
                pass
    # 统计关键导入命中
    hits = {}
    for _, nm in bls:
        for k in ("nanosleep", "usleep", "sleep", "signal", "sigaction", "kill", "ptrace", "abort", "exit", "mmap", "open", "fopen", "fread"):
            if nm == k or nm.startswith(k):
                hits[nm] = hits.get(nm, 0) + 1
    print(f"\n=== caller {c:#x} → 函数 {fs:#x}-{fe:#x} (size {fe-fs:#x}, insns {len(body)}, 回边 {loops}) ===")
    print(f"  导入命中: {hits}")
    top = {}
    for _, nm in bls:
        top[nm] = top.get(nm, 0) + 1
    print(f"  调用 top: {sorted(top.items(), key=lambda kv: -kv[1])[:10]}")
