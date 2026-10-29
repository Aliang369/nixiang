#!/usr/bin/env python3
# 失落城堡2 · il2cpp 静态取证工具（名字↔RVA / 反汇编 / 调用方）
#
# 为什么需要它：本作（易盾 NIS 4.x + 乐变壳）把 libil2cpp.so 的节表毁掉、dynsym 全随机名，
# Il2CppDumper/Cpp2IL 自动模式拿不到 CodeRegistration（`CodeRegistration : 0`）。
# 但 metadata 已从内存取到明文，且进程 bss 里有 per-module 的 methodPointers 数组 ⇒
# 用「锚点反查法」可重建整张 name→RVA 表（本机实测 210,024 条，LC2.Core 52,162 条全覆盖）。
#
# 依赖的本地产物（不入库，按 10_docs/失落城堡2 文档的 dump 流程产出）：
#   09_test/失落城堡2/meta/global-metadata.dat   明文 metadata（v31）
#   09_test/失落城堡2/meta_mem.bin                metadata 映射原件（含 0x150 字节前缀）
#   09_test/失落城堡2/bss1.bin   VA 0x729a91f000  libil2cpp [anon:.bss]（含 codeGenModules）
#   09_test/失落城堡2/bss2.bin   VA 0x729b3cc000  libil2cpp [anon:.bss]（含 methodPointers 数组）
#   09_test/失落城堡2/libil2cpp_dumped.so        内存重建 so（offset == vaddr）
#
# 用法：
#   python3 04_scripts/失落城堡2/il2cpp_static.py map                     # 重建 rva_map.tsv
#   python3 04_scripts/失落城堡2/il2cpp_static.py find  <名字子串>         # 名字 → RVA
#   python3 04_scripts/失落城堡2/il2cpp_static.py disasm <rva> [条数]      # 反汇编（自动注解 bl/b 目标）
#   python3 04_scripts/失落城堡2/il2cpp_static.py callers <rva> [<rva>…]   # 扫全 .text 找 BL/B 调用方
import bisect
import json
import os
import struct
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
G = os.path.join(ROOT, "09_test", "失落城堡2")
META = os.path.join(G, "meta", "global-metadata.dat")
SO = os.path.join(G, "libil2cpp_dumped.so")
BSS1, BSS1_VA = os.path.join(G, "bss1.bin"), 0x729A91F000
BSS2, BSS2_VA = os.path.join(G, "bss2.bin"), 0x729B3CC000
OUT = os.path.join(G, "analysis")
MAP = os.path.join(OUT, "rva_map.tsv")
BASE = 0x72876B8000          # 该 dump 的加载基址（VA = BASE + RVA）
TEXT = (0x728DDC4000, 0x729A91C000)

# metadata header 字段顺序（v31）
HDR_FIELDS = [
    "stringLiteral", "stringLiteralData", "string", "events", "properties", "methods",
    "parameterDefaultValues", "fieldDefaultValues", "fieldAndParameterDefaultValueData",
    "fieldMarshaledSizes", "parameters", "fields", "genericParameters",
    "genericParameterConstraints", "genericContainers", "nestedTypes", "interfaces",
    "vtableMethods", "interfaceOffsets", "typeDefinitions", "images", "assemblies",
    "fieldRefs", "referencedAssemblies", "attributeData", "attributeDataRange",
    "unresolvedVirtualCallParameterTypes", "unresolvedVirtualCallParameterRanges",
    "windowsRuntimeTypeNames", "windowsRuntimeStrings", "exportedTypeDefinitions",
]


def _md():
    """解析 metadata → (bytes, header dict, 取字符串函数)"""
    m = open(META, "rb").read()
    hdr = {n: struct.unpack_from("<ii", m, 8 + i * 8) for i, n in enumerate(HDR_FIELDS)}
    soff = hdr["string"][0]

    def s(idx):
        e = m.index(b"\0", soff + idx)
        return m[soff + idx:e].decode("utf-8", "replace")

    return m, hdr, s


def _images(m, hdr, s):
    """metadata images[165]（40B/条）→ [(name, typeStart, typeCount, methodCount)]"""
    ioff, ibytes = hdr["images"]
    toff = hdr["typeDefinitions"][0]
    tsz = 88

    def mcount(ti):
        return struct.unpack_from("<H", m, toff + ti * tsz + 64)[0]

    out = []
    for k in range(ibytes // 40):
        name_idx, _asm, tstart, tcnt = struct.unpack_from("<4i", m, ioff + k * 40)
        out.append((s(name_idx), tstart, tcnt, sum(mcount(tstart + j) for j in range(tcnt))))
    return out


def _module_arrays():
    """在 bss 里找每张 image 的 methodPointers 数组 → {imageIndex: (count, arrayVA)}"""
    d2 = open(BSS2, "rb").read()
    cands = []
    for path, va in ((BSS1, BSS1_VA), (BSS2, BSS2_VA)):
        d = open(path, "rb").read()
        for off in range(0, len(d) - 0x30, 4):
            cnt = struct.unpack_from("<I", d, off)[0]
            if not 1 <= cnt <= 60000:
                continue
            mp = struct.unpack_from("<Q", d, off + 8)[0]
            if not BSS2_VA <= mp < BSS2_VA + 0x8B7000:
                continue
            if struct.unpack_from("<I", d, off + 0x10)[0] > 60000:
                continue
            step = max(1, cnt // 64)
            good = tot = 0
            for k in range(0, cnt, step):
                o = mp - BSS2_VA + k * 8
                if 0 <= o + 8 <= len(d2) and TEXT[0] <= struct.unpack_from("<Q", d2, o)[0] < TEXT[1]:
                    good += 1
                tot += 1
            if tot and good / tot >= 0.5:
                cands.append((mp, cnt))
    uniq = {}
    for mp, cnt in cands:
        uniq.setdefault(mp, (mp, cnt))
    cands = sorted(uniq.values(), key=lambda c: c[0])
    return d2, cands


def cmd_map():
    m, hdr, s = _md()
    imgs = _images(m, hdr, s)
    os.makedirs(OUT, exist_ok=True)
    json.dump([list(x) for x in imgs], open(os.path.join(OUT, "images.json"), "w"))
    d2, cands = _module_arrays()
    counts = [i[3] for i in imgs]
    byname = {}
    for c in cands:
        byname.setdefault(c[1], []).append(c)
    used, modules = set(), {}
    for i, ct in enumerate(counts):
        for c in byname.get(ct, []):
            if c[0] in used:
                continue
            used.add(c[0])
            modules[i] = c
            break
    print(f"images={len(imgs)} 数组候选={len(cands)} 对齐成功={len(modules)}")

    # 自检：LC2.Core 的 IsAppearanceItemUnlocked 必须解析成 0x11944af8
    anchor = None
    if 0 in modules:
        mp, cnt = modules[0]
        o = mp - BSS2_VA + 0x7C7D * 8
        if 0 <= o + 8 <= len(d2):
            anchor = struct.unpack_from("<Q", d2, o)[0] - BASE
    print(f"anchor IsAppearanceItemUnlocked = {anchor:#x} (期望 0x11944af8)" if anchor else "anchor 缺失")

    # 遍历 typeDefs → methods，按 rid-1 取指针
    toff, tbytes = hdr["typeDefinitions"]
    tsz = 88
    imgidx = {imgs[i][0]: i for i in range(len(imgs))}
    out = open(MAP, "w")
    nok = nbad = 0
    for k in range(tbytes // tsz):
        b = toff + k * tsz
        name_idx, ns_idx = struct.unpack_from("<2i", m, b)
        mstart = struct.unpack_from("<i", m, b + 36)[0]
        mcount = struct.unpack_from("<H", m, b + 64)[0]
        full = s(name_idx)
        ns = s(ns_idx)
        if ns:
            full = f"{ns}.{full}"
        for j in range(mcount):
            mo = hdr["methods"][0] + (mstart + j) * 36
            mname_idx = struct.unpack_from("<i", m, mo)[0]
            argc = struct.unpack_from("<H", m, mo + 34)[0]   # v31: slot@32 / parameterCount@34
            token = struct.unpack_from("<I", m, mo + 24)[0]
            rid = token & 0xFFFFFF
            img = None
            for i, im in enumerate(imgs):
                if im[1] <= k < im[1] + im[2]:
                    img = i
                    break
            mod = modules.get(img) if img is not None else None
            if not mod or not (1 <= rid <= mod[1]):
                nbad += 1
                continue
            o = mod[0] - BSS2_VA + (rid - 1) * 8
            if not (0 <= o + 8 <= len(d2)):
                nbad += 1
                continue
            v = struct.unpack_from("<Q", d2, o)[0]
            if v < BASE:
                nbad += 1
                continue
            out.write(f"{imgs[img][0]}\t{full}\t{s(mname_idx)}\t{argc}\t{token:#010x}\t{v - BASE:#x}\n")
            nok += 1
    out.close()
    print(f"mapped={nok} skipped={nbad} → {MAP}")


def _names():
    n = {}
    if os.path.exists(MAP):
        for ln in open(MAP, encoding="utf-8"):
            p = ln.rstrip("\n").split("\t")
            if len(p) == 6:
                n.setdefault(int(p[5], 16), f"{p[1]}.{p[2]}")
    return n


def _read(off, n):
    with open(SO, "rb") as f:
        f.seek(off)
        return f.read(n)


def cmd_disasm(rva, maxn=200):
    import capstone
    md = capstone.Cs(capstone.CS_ARCH_ARM64, capstone.CS_MODE_ARM)
    N = _names()
    for ins in md.disasm(_read(rva, maxn * 4), rva):
        ann = ""
        if ins.mnemonic in ("bl", "b", "blr", "br") and ins.op_str.startswith("#"):
            try:
                t = int(ins.op_str[1:], 16)
            except ValueError:
                t = None
            if t is not None:
                ann = f"   ; -> {N.get(t) or ''} ({t:#x})"
        print(f"{ins.address:#010x}  {ins.bytes.hex():<8} {ins.mnemonic:<8} {ins.op_str}{ann}")
        if ins.mnemonic == "ret":
            break


def cmd_find(q):
    for k, v in sorted(_names().items()):
        if q.lower() in v.lower():
            print(f"{k:#x}\t{v}")


def cmd_callers(targets):
    import numpy as np
    buf = np.frombuffer(_read(0, 0x13264000), dtype="<u4")
    names = _names()
    ks = sorted(names)
    for t in targets:
        print(f"=== target {t:#x} {names.get(t, '')} ===")
        op = buf >> 26
        for opline, opname in ((0x25, "BL"), (0x05, "B")):
            idx = np.nonzero(op == opline)[0]
            if idx.size == 0:
                continue
            imms = (buf[idx] & 0x03FFFFFF).astype(np.int64)
            imms = np.where(imms >= 0x02000000, imms - 0x04000000, imms)
            dest = idx.astype(np.int64) * 4 + imms * 4
            for h in idx[dest == t][:40]:
                rva = int(h) * 4
                i = bisect.bisect_right(ks, rva) - 1
                fs = ks[i] if i >= 0 else None
                print(f"  {opname} at {rva:#x}  in {fs:#x} {names.get(fs, '?') if fs else '?'}")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(2)
    cmd = sys.argv[1]
    if cmd == "map":
        cmd_map()
    elif cmd == "find":
        cmd_find(sys.argv[2])
    elif cmd == "disasm":
        cmd_disasm(int(sys.argv[2].replace("0x", ""), 16), int(sys.argv[3]) if len(sys.argv) > 3 else 200)
    elif cmd == "callers":
        cmd_callers([int(a.replace("0x", ""), 16) for a in sys.argv[2:]])
    else:
        print(__doc__)
        sys.exit(2)
