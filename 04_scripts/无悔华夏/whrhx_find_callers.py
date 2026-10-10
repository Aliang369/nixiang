#!/usr/bin/env python3
"""whrhx_find_callers.py —— 在游戏 so 里找「谁调用了它」（ARM64 直接 bl）

背景（2026-10-11）：dump.cs 的方法体是空的，静态追调用链只有两条路
  ① 反汇编看某个函数里 bl 了谁            → llvm-objdump + whrhx_addr2name.sh
  ② 反汇编找「谁 bl 了某地址」（反向）    → 本脚本
本脚本按 ELF 程序头取出**可执行段**逐条解码 bl（0x94000000 家族），
按 pc + imm26*4 算目标，因此不受「无符号表」影响；再用 --owner-map
（whrhx_addr2name.sh 生成的 地址→类.方法 表）把调用点回填到「所属函数」。
⚠ 局限：只找**直接 bl**；虚调用/委托/invoke 桩（blr xN、ILFixDynamicMethodWrapper）
看不见，需另配 Trace 或按类名扫实例。

用法:
  whrhx_find_callers.py <so> 0x7016B98 [更多目标...] [--owner-map /tmp/whrhx_addr_map.txt]
  whrhx_find_callers.py <so> --from-stdin        # 目标地址从 stdin 读（每行一个）
输出: 调用点地址  目标地址  所属函数(可选)  —— 按调用点排序
"""
import struct
import sys


def exec_segments(path):
    with open(path, "rb") as f:
        data = f.read()
    if data[:4] != b"\x7fELF" or data[4] != 2:
        raise SystemExit("不是 64 位 ELF: %s" % path)
    e_phoff = struct.unpack_from("<Q", data, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", data, 0x36)[0]
    e_phnum = struct.unpack_from("<H", data, 0x38)[0]
    segs = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type = struct.unpack_from("<I", data, off)[0]
        p_flags = struct.unpack_from("<I", data, off + 4)[0]
        p_offset = struct.unpack_from("<Q", data, off + 8)[0]
        p_vaddr = struct.unpack_from("<Q", data, off + 16)[0]
        # 注意: off+24 是 p_paddr，p_filesz 在 off+32（漏掉 p_paddr 会让整段扫成 0 字节）
        p_filesz = struct.unpack_from("<Q", data, off + 32)[0]
        if p_type == 1 and (p_flags & 1) and p_filesz:
            segs.append((p_vaddr, p_offset, p_filesz))
    return data, segs


def main(argv):
    if len(argv) < 3:
        print(__doc__)
        return 2
    path = argv[1]
    want = set()
    owner_map = None
    i = 2
    while i < len(argv):
        a = argv[i]
        if a == "--owner-map":
            owner_map = argv[i + 1]
            i += 2
            continue
        if a == "--from-stdin":
            for line in sys.stdin:
                line = line.strip()
                if line.startswith("0x") or line.startswith("0X"):
                    want.add(int(line, 16))
            i += 1
            continue
        want.add(int(a, 16) if a.lower().startswith("0x") else int(a))
        i += 1
    if not want:
        raise SystemExit("没有目标地址")

    owners = None
    if owner_map:
        owners = []
        with open(owner_map) as f:
            for line in f:
                p = line.rstrip("\n").split("\t")
                if len(p) == 2 and p[0].startswith("0x"):
                    owners.append((int(p[0], 16), p[1]))
        owners.sort()
        owners_addrs = [o[0] for o in owners]

    import bisect

    data, segs = exec_segments(path)
    hits = 0
    for p_vaddr, p_offset, p_filesz in segs:
        n = p_filesz // 4
        words = struct.unpack_from("<%dI" % n, data, p_offset)
        for idx, w in enumerate(words):
            if (w & 0xFC000000) != 0x94000000:
                continue
            imm = w & 0x03FFFFFF
            if imm & 0x02000000:
                imm -= 0x04000000
            pc = p_vaddr + idx * 4
            tgt = pc + (imm << 2)
            if tgt in want:
                owner = ""
                if owners:
                    j = bisect.bisect_right(owners_addrs, pc) - 1
                    if j >= 0:
                        base, name = owners[j]
                        owner = name if pc - base < 0x8000 else "%s?+0x%X" % (name, pc - base)
                print("0x%X\t0x%X\t%s" % (pc, tgt, owner))
                hits += 1
    print("[find_callers] %d call site(s) / %d target(s)" % (hits, len(want)), file=sys.stderr)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
