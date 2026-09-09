#!/usr/bin/env python3
"""libil2cpp.so icall 表定位: 通过 icall 名字符串 VA 反查 {name*, fn*} 注册表
用法: python3 find_icall_table.py <libil2cpp.so> <substring> [context_entries]
"""
import struct, sys

def main():
    path, needle, ctx = sys.argv[1], sys.argv[2].encode(), int(sys.argv[3]) if len(sys.argv) > 3 else 8
    data = open(path, "rb").read()

    # ELF64 program headers: file offset -> VA 映射
    e_phoff = struct.unpack_from("<Q", data, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", data, 0x36)[0]
    e_phnum = struct.unpack_from("<H", data, 0x38)[0]
    segs = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type, p_flags = struct.unpack_from("<II", data, off)
        p_offset, p_vaddr, _p_paddr, p_filesz = struct.unpack_from("<QQQQ", data, off + 8)
        if p_type == 1 and p_filesz > 0:
            segs.append((p_offset, p_vaddr, p_filesz))

    def off2va(o):
        for po, va, sz in segs:
            if po <= o < po + sz:
                return va + (o - po)
        return None

    def va2off(v):
        for po, va, sz in segs:
            if va <= v < va + sz:
                return po + (v - va)
        return None

    # 1) 找字符串 (全部出现)
    pos, hits = 0, []
    while True:
        i = data.find(needle, pos)
        if i < 0:
            break
        # 校验以 \0 结尾的独立字符串
        end = data.find(b"\0", i)
        s = data[i:end].decode("utf-8", "replace")
        va = off2va(i)
        hits.append((i, va, s))
        pos = i + 1
    if not hits:
        print("string not found")
        return 1
    for off, va, s in hits:
        print(f"string@off=0x{off:x} va=0x{va:x}: {s[:100]}")

    # 2) 反查: 先建重定位表 (PIE: 指针存于 .rela.dyn R_AARCH64_RELATIVE 的 addend)
    e_shoff = struct.unpack_from("<Q", data, 0x28)[0]
    e_shentsize = struct.unpack_from("<H", data, 0x3A)[0]
    e_shnum = struct.unpack_from("<H", data, 0x3C)[0]
    e_shstrndx = struct.unpack_from("<H", data, 0x3E)[0]
    shstr_off = None
    sections = []
    for i in range(e_shnum):
        off = e_shoff + i * e_shentsize
        sh_name, sh_type, sh_flags, sh_addr, sh_offset, sh_size, sh_link, sh_info, sh_align, sh_entsize = struct.unpack_from("<IIQQQQIIQQ", data, off)
        sections.append((sh_name, sh_type, sh_addr, sh_offset, sh_size, sh_entsize))
        if i == e_shstrndx:
            shstr_off = sh_offset
    rela_map = {}
    for sh_name, sh_type, sh_addr, sh_offset, sh_size, sh_entsize in sections:
        if sh_type == 4:  # SHT_RELA
            n = sh_size // 24
            for i in range(n):
                o = sh_offset + i * 24
                r_offset, r_info, r_addend = struct.unpack_from("<QQq", data, o)
                if (r_info & 0xFFFFFFFF) == 1027:  # R_AARCH64_RELATIVE
                    rela_map[r_offset] = r_addend
    print(f"rela entries: {len(rela_map)}")

    # 3) 反查 VA 指针 → 注册表项 (查内联 + 查 rela)
    for off, va, s in hits:
        if va is None:
            continue
        vb = struct.pack("<Q", va)
        refs = []
        ppos = 0
        while True:
            j = data.find(vb, ppos)
            if j < 0:
                break
            refs.append(("inline", j))
            ppos = j + 1
        for r_off, addend in rela_map.items():
            if addend == va:
                refs.append(("rela", r_off))
        print(f"\n== refs to va=0x{va:x}: {len(refs)}")
        for kind, r in refs[:8]:
            if kind == "inline":
                fn = struct.unpack_from("<Q", data, r + 8)[0]
                foff = va2off(fn)
                print(f"  inline@off=0x{r:x}: fn=0x{fn:x} (fileoff={'0x%x' % foff if foff else '??'})")
            else:
                fn = rela_map.get(r + 8)
                foff = va2off(fn) if fn else None
                print(f"  rela@off=0x{r:x}: fn=0x{fn:x} (fileoff={'0x%x' % foff if foff else '??'})")
            # 打印周围表项 (经 rela)
            for k in range(-ctx, ctx + 1):
                eo = r + k * 16
                nv = rela_map.get(eo)
                fv = rela_map.get(eo + 8)
                if nv is None or fv is None:
                    continue
                no = va2off(nv)
                if no is None:
                    continue
                try:
                    e = data.find(b"\0", no)
                    nm = data[no:e].decode("utf-8", "replace")
                    if not nm or len(nm) > 120:
                        continue
                    print(f"    [{k:+d}] {nm} -> 0x{fv:x}")
                except Exception:
                    continue
    return 0

if __name__ == "__main__":
    sys.exit(main())
