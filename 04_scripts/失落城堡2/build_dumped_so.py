#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""失落城堡2：把运行时内存镜像拼成 dump 风格 ELF，供 Il2CppDumper 解析。

背景（与失落城堡 1 同源，网易易盾 NIS 4.23）：
  1) libil2cpp.so 磁盘件代码段整段密文，装载期在内存原地解密；
  2) 段表被搅乱（节名乱序、`.text` 只有 0x10 字节）、dynsym 名字全部随机化
     ⇒ Il2CppDumper 的 SymbolSearch 必死；
  3) 本作 **metadata v31**，模块名字符串只存在于 metadata（so 里搜不到
     `mscorlib.dll`）⇒ FindCodeRegistrationExec 的字符串特征法也死，
     只能靠 FindCodeRegistrationData（在 data 段里找指针链）。
     为此本脚本把「代码段 / 代码段 bss / 小数据段 / .data」拆成 4 个 PT_LOAD，
     并让指向数据的段带 PF_W，使 Il2CppDumper 的 data/bss 段集合正确。

做法：
  - file offset == vaddr（恒等映射），后续把 dump 基址填 `0x72876b8000`
    （Elf64.FixedProgramSegment 会给 p_vaddr 加上 ImageBase）；
  - 去掉节表（e_shoff=0/e_shnum=0）⇒ 触发 "dump file" 分支。

输入（均在 09_test/失落城堡2/ 下）：
  apk_x/lib/arm64-v8a/libil2cpp.so   磁盘件（只取 ELF 头 + 非 LOAD 程序头）
  dump_A/il2cpp_rx_1.bin             VA 0x670c000 起 12 KB
  dump_A/il2cpp_rx_2.bin             VA 0x670f000 起 ~205 MB（代码）
  bss1.bin                           VA 0x13267000 起 10.7 MB
  bss2.bin                           VA 0x13d14000 起  9.1 MB
  seg3_mem.bin                       VA 0x145cc000 起 741 KB 段（此处 dump 偏大，截断用）
  data_mem.bin                       VA 0x14686000 起 40.0 MB
输出：libil2cpp_dumped.so（稀疏文件，约 382 MB 逻辑大小）
"""
import os
import struct
import sys

BASE_IMAGE = 0x72876B8000
END = 0x16CC2C58  # 最后一段 memsz 末尾

ROOT = os.path.dirname(os.path.abspath(__file__))
GAME = os.path.join(ROOT, "..", "..", "09_test", "失落城堡2")

DISK = os.path.join(GAME, "apk_x/lib/arm64-v8a/libil2cpp.so")
OUT = os.path.join(GAME, "libil2cpp_dumped.so")

SEGMENTS = [
    # (vaddr, 段大小, flags, 来源文件相对 GAME, 源文件内起点 vaddr)
    (0x0000000, 0x13264000, 5, None, 0),                       # 代码（下方按块填）
    (0x13264000, 0x145CC000 - 0x13264000, 6, None, 0),          # 代码段 bss
    (0x145CC000, 0x00B5000, 6, "seg3_mem.bin", 0x145CC000),
    (0x14686000, END - 0x14686000, 6, "data_mem.bin", 0x14686000),
]

CODE_BLOCKS = [
    ("dump_A/il2cpp_rx_1.bin", 0x0670C000),
    ("dump_A/il2cpp_rx_2.bin", 0x0670F000),
]
BSS_BLOCKS = [
    ("bss1.bin", 0x13267000),
    ("bss2.bin", 0x13D14000),
]


def main():
    if not os.path.exists(DISK):
        sys.exit("缺磁盘件: %s" % DISK)
    disk = open(DISK, "rb").read()
    e_phoff, = struct.unpack_from("<Q", disk, 0x20)
    e_phentsize, e_phnum = struct.unpack_from("<HH", disk, 0x36)
    orig = [disk[e_phoff + i * e_phentsize: e_phoff + (i + 1) * e_phentsize] for i in range(e_phnum)]
    dyn, note, ehframe = orig[4], orig[5], orig[6]

    data = bytearray(END)
    data[0:0x40] = disk[0:0x40]

    ph = b""
    for va, size, flags, _src, _sva in SEGMENTS:
        ph += struct.pack("<IIQQQQQQ", 1, flags, va, va, va, size, size, 0x1000)
    ph += dyn + note + ehframe + b"\x00" * 56
    assert len(ph) % e_phentsize == 0, "程序头长度必须是 e_phentsize 的整数倍"
    data[e_phoff:e_phoff + len(ph)] = ph
    struct.pack_into("<H", data, 0x38, len(ph) // e_phentsize)  # e_phnum
    struct.pack_into("<Q", data, 0x28, 0)                      # e_shoff = 0
    struct.pack_into("<H", data, 0x3C, 0)                      # e_shnum = 0

    def fill(rel, va):
        p = os.path.join(GAME, rel)
        if not os.path.exists(p):
            print("[skip] 缺 %s" % rel)
            return
        b = open(p, "rb").read()
        end = min(END, va + len(b))
        data[va:end] = b[:end - va]
        print("[fill] %-26s %10d B -> VA %#x" % (rel, len(b), va))

    for rel, va in CODE_BLOCKS:
        fill(rel, va)
    for rel, va in BSS_BLOCKS:
        fill(rel, va)
    for _va, _sz, _fl, rel, sva in SEGMENTS:
        if rel:
            fill(rel, sva)

    open(OUT, "wb").write(bytes(data))
    print("[out] %s  逻辑大小 %d B" % (OUT, len(data)))
    print("[hint] Il2CppDumper dump 地址请填: %x" % BASE_IMAGE)


if __name__ == "__main__":
    main()
