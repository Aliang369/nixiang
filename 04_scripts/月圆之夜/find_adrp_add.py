#!/usr/bin/env python3
"""扫 ADRP+ADD 对计算指定 VA 的代码点 (icall 注册处), 打印周边指令
用法: python3 find_adrp_add.py <libil2cpp.so> <va_hex> [--ctx N]
"""
import struct, sys

def load(path):
    data = open(path, "rb").read()
    e_phoff = struct.unpack_from("<Q", data, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", data, 0x36)[0]
    e_phnum = struct.unpack_from("<H", data, 0x38)[0]
    segs = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type = struct.unpack_from("<I", data, off)[0]
        p_offset, p_vaddr, _pa, p_filesz, _m = struct.unpack_from("<QQQQQ", data, off + 8)[:5]
        if p_type == 1:
            segs.append((p_offset, p_vaddr, p_filesz))
    return data, segs

def va2off(segs, v):
    for po, va, sz in segs:
        if va <= v < va + sz:
            return po + (v - va)
    return None

def disasm_rough(insn, pc):
    """极简 arm64 反汇编: 只认 adrp/add/movz/bl/b"""
    if (insn & 0x9F000000) == 0x90000000:
        rd = insn & 31
        immlo = (insn >> 29) & 3
        immhi = (insn >> 5) & 0x7FFFF
        imm = (immhi << 2) | immlo
        if imm & (1 << 20):
            imm -= (1 << 21)
        tgt = (pc & ~0xFFF) + (imm << 12)
        return f"adrp x{rd}, 0x{tgt:x}"
    if (insn & 0xFF800000) == 0x91000000:
        rd = insn & 31
        rn = (insn >> 5) & 31
        imm12 = (insn >> 10) & 0xFFF
        sh = (insn >> 22) & 3
        imm = imm12 << 12 if sh == 1 else imm12
        return f"add x{rd}, x{rn}, #0x{imm:x}"
    if (insn & 0xFFE00000) == 0x94000000 or (insn & 0xFFE00000) == 0x14000000:
        imm26 = insn & 0x3FFFFFF
        if imm26 & (1 << 25):
            imm26 -= (1 << 26)
        return f"{('b' if (insn & 0xFC000000)==0x14000000 else 'bl')} 0x{pc + imm26*4:x}"
    if (insn & 0xFFE00001) == 0xF9400000:
        rt = insn & 31
        rn = (insn >> 5) & 31
        imm12 = (insn >> 10) & 0xFFF
        return f"ldr x{rt}, [x{rn}, #{imm12*8}]"
    return None

def main():
    path = sys.argv[1]
    target = int(sys.argv[2], 16)
    ctx = int(sys.argv[3]) if len(sys.argv) > 3 else 12
    data, segs = load(path)

    text = None
    e_shoff = struct.unpack_from("<Q", data, 0x28)[0]
    e_shentsize = struct.unpack_from("<H", data, 0x3A)[0]
    e_shnum = struct.unpack_from("<H", data, 0x3C)[0]
    for i in range(e_shnum):
        sh = struct.unpack_from("<IIQQQQIIQQ", data, e_shoff + i * e_shentsize)
        if sh[1] == 6:  # SHT_PROGBITS named .text — 简化: 取 addr&1==0 最大的 PROGBITS ALLOC EXEC
            if sh[3] and (sh[2] & 0x4):
                text = sh
    # 保险: 用名为 .text 的节
    e_shstrndx = struct.unpack_from("<H", data, 0x3E)[0]
    shstr = secs = None
    for i in range(e_shnum):
        sh = struct.unpack_from("<IIQQQQIIQQ", data, e_shoff + i * e_shentsize)
        if i == e_shstrndx:
            shstr = sh
    for i in range(e_shnum):
        sh = struct.unpack_from("<IIQQQQIIQQ", data, e_shoff + i * e_shentsize)
        nm = data[shstr[4] + sh[0]:data.find(b"\0", shstr[4] + sh[0])].decode()
        if nm == ".text":
            text = sh
            break
    _, _, sh_addr, sh_offset, sh_size = (None, None, None, None, None)
    if text:
        _, _, sh_addr, sh_offset, sh_size = text[0], text[1], text[3], text[4], text[5]
        print(f".text va=0x{sh_addr:x} off=0x{sh_offset:x} size=0x{sh_size:x}")
    else:
        print("(无 .text 节, 用程序头扫描)")

    page = target & ~0xFFF
    low12 = target & 0xFFF
    refs = []
    # 扫所有可执行 LOAD 段 (MHP 打包后节表不可信)
    for po, va, sz in segs:
        # PF_X = 0x1 需查 flags, 这里直接全段扫 (data 段 ADRP 引用无效, 但不会误报 ADD 对)
        pass
    # 重建 segs with flags
    e_phoff = struct.unpack_from("<Q", data, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", data, 0x36)[0]
    e_phnum = struct.unpack_from("<H", data, 0x38)[0]
    xsegs = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type, p_flags = struct.unpack_from("<II", data, off)
        p_offset, p_vaddr, _pa, p_filesz = struct.unpack_from("<QQQQ", data, off + 8)
        if p_type == 1 and (p_flags & 1):
            xsegs.append((p_offset, p_vaddr, p_filesz))
    print("exec segs: " + ", ".join(f"va=0x{v:x} sz=0x{s:x}" for _, v, s in xsegs))
    for po, va, sz in xsegs:
        n = sz // 4
        buf = data[po: po + sz]
        for i in range(n - 1):
            insn = struct.unpack_from("<I", buf, i * 4)[0]
            if (insn & 0x9F000000) != 0x90000000:
                continue
            rd = insn & 31
            immlo = (insn >> 29) & 3
            immhi = (insn >> 5) & 0x7FFFF
            imm = (immhi << 2) | immlo
            if imm & (1 << 20):
                imm -= (1 << 21)
            if (va + i * 4 & ~0xFFF) + (imm << 12) != page:
                continue
            insn2 = struct.unpack_from("<I", buf, (i + 1) * 4)[0]
            if (insn2 & 0xFF800000) != 0x91000000:
                continue
            if insn2 & 31 != rd or (insn2 >> 5) & 31 != rd:
                continue
            imm2 = (insn2 >> 10) & 0xFFF
            if (insn2 >> 22) & 3 == 1:
                continue
            if imm2 == low12:
                refs.append(va + i * 4)
        if refs:
            break
    print(f"refs to 0x{target:x}: {len(refs)} -> {[hex(x) for x in refs[:10]]}")

    for r in refs[:6]:
        ro = va2off(segs, r)
        print(f"\n== context @0x{r:x}")
        for k in range(-ctx, ctx + 1):
            o = ro + k * 4
            if o < 0 or o + 4 > len(data):
                continue
            insn = struct.unpack_from("<I", data, o)[0]
            d = disasm_rough(insn, r + k * 4)
            mark = " <<<<" if k == 0 else ""
            if d:
                print(f"  {r + k*4:#x}: {d}{mark}")
            else:
                raw = struct.unpack_from("<I", data, o)[0]
                print(f"  {r + k*4:#x}: ({raw:#010x}){mark}")

if __name__ == "__main__":
    main()
