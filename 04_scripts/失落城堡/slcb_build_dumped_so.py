#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""失落城堡：把「运行时内存镜像」拼成一个 dump 风格的可解析 ELF。

背景（为什么需要它）：
  易盾(NIS)对 libil2cpp.so 做了两层破坏——
    1) 磁盘上 [0, 0x3993000) 整段密文（含整片 40MB 假零区），装载时在内存里原地解密；
       实测内存 vs 磁盘差异 51,583,232 B（69.9%）。
    2) 段表(section header table)被搅乱：没有名叫 .text 的节，
       e_shstrndx 指向垃圾 → Il2CppDumper 的 CheckSection() 必失败；
       同时 dynsym 名字全部被改写成随机短名（n5 / d85J_2 / MP …），
       2698 个动态符号里 0 个 il2cpp_* → 按名查找（dlsym / xdl_sym / SymbolSearch）全死。
  另外内存里第 0 页被壳换成了自己的结构（不是 ELF header）。

  于是：以运行时加载基址为准，按 **vaddr 布局**（file offset == vaddr）重建镜像，
  并把程序头的 p_offset 改写成 p_vaddr，使 Il2CppDumper 的 MapVATR 成为恒等映射。
  这样链接期 vaddr 形式的数据指针可以直接被解析（配 dump 地址输入 0 使用）。

输入：
  09_test/失落城堡/orig/elfcheck/lib/arm64-v8a/libil2cpp.so   （磁盘件，取 ELF 头 + 程序头）
  09_test/失落城堡/mem/slcb_il2cpp_mem/seg_*.bin               （slcb_dump_il2cpp_mem.sh 落地的各段）

输出：
  09_test/失落城堡/mem/libil2cpp_dumped.so
  同目录 libil2cpp_dumped.so.meta.txt（各段 vaddr 布局 + base）
"""
import os
import re
import struct
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
DISK = os.path.join(ROOT, "09_test/失落城堡/orig/elfcheck/lib/arm64-v8a/libil2cpp.so")
SEGDIR = os.path.join(ROOT, "09_test/失落城堡/mem/slcb_il2cpp_mem")
OUT = os.path.join(ROOT, "09_test/失落城堡/mem/libil2cpp_dumped.so")
META = OUT + ".meta.txt"

PT_LOAD = 1


def main():
    disk = open(DISK, "rb").read()
    if disk[:4] != b"\x7fELF":
        sys.exit("磁盘件不是 ELF")

    e_phoff, = struct.unpack_from("<Q", disk, 0x20)
    e_phentsize, e_phnum = struct.unpack_from("<HH", disk, 0x36)
    phdrs = []
    for i in range(e_phnum):
        o = e_phoff + i * e_phentsize
        phdrs.append(list(struct.unpack_from("<IIQQQQQQ", disk, o)))

    loads = [p for p in phdrs if p[0] == PT_LOAD]
    top = max(p[3] + p[6] for p in loads)  # p_vaddr + p_memsz
    print("PT_LOAD 段: " + ", ".join(
        "off=0x%x va=0x%x filesz=0x%x memsz=0x%x flags=%d" % (p[2], p[3], p[5], p[6], p[1]) for p in loads))
    print("镜像总高 = 0x%x (%.1f MB)" % (top, top / 1048576))

    # 段文件名形如 seg_00_r-xp_00000000_73f9ce9000.bin：第 4 段是运行时地址
    segs = []
    for fn in sorted(os.listdir(SEGDIR)):
        m = re.match(r"seg_(\d+)_(.+)_([0-9a-f]{8})_([0-9a-f]+)\.bin$", fn)
        if not m:
            continue
        segs.append((int(m.group(1)), os.path.join(SEGDIR, fn), int(m.group(4), 16)))
    if not segs:
        sys.exit("没有找到 seg_*.bin，先跑 slcb_dump_il2cpp_mem.sh")
    segs.sort()
    base = min(a for _, _, a in segs)
    print("运行时加载基址 base = 0x%x" % base)

    img = bytearray(top)
    lines = ["base=0x%x" % base, "image_size=0x%x" % top]
    for idx, path, addr in segs:
        data = open(path, "rb").read()
        va = addr - base
        end = va + len(data)
        if end > top:
            print("  ! seg_%02d 超出镜像高度，截断" % idx)
            data = data[:top - va]
            end = top
        img[va:end] = data
        lines.append("seg_%02d va=0x%x len=0x%x src=%s" % (idx, va, len(data), os.path.basename(path)))
        print("  seg_%02d -> vaddr 0x%-10x len 0x%-9x (%.2f MB)" % (idx, va, len(data), len(data) / 1048576))

    # 第 0 页在运行时被壳占用，用磁盘件的 ELF 头 + 程序头补回
    img[0:0x1000] = disk[0:0x1000]

    # 程序头改成 dump 布局：p_offset = p_vaddr
    for i, p in enumerate(phdrs):
        o = e_phoff + i * e_phentsize
        struct.pack_into("<Q", img, o + 8, p[3])  # p_offset := p_vaddr

    open(OUT, "wb").write(bytes(img))
    lines.append("output=%s size=%d" % (OUT, len(img)))
    open(META, "w").write("\n".join(lines) + "\n")
    print("写出 %s (%d B, %.1f MB)" % (OUT, len(img), len(img) / 1048576))
    print("下一步：printf '0\\n' | dotnet 01_tools/Il2CppDumper/Il2CppDumper.dll <该 so> <解密 metadata> <输出目录>")


if __name__ == "__main__":
    main()
