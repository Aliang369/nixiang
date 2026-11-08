#!/usr/bin/env python3
"""把引擎里两处"从 .bss 全局 std::string 取 res xxtea key"的代码改成"直接用 .rodata 字面量"。

背景：无壳包里没人给那个 std::string 赋值（原本由壳 libsecret.so 写），
      于是 key_len=0，所有 .jsc 解密失败，启动卡在「数据检查」。
      字面量 "d03398ba-c326-44" 就在 .rodata 0x19419ab（engine 自带，任何签名下都一样）。

补丁点（原指令序列 7 条 → 4 条 + 3 nop）：
    ldrb w9,[x8] / ldr x10,[x8,#0x10] / ldr w11,[x8,#8] / lsr x12,x9,#1
    tst w9,#1 / csinc x2,x10,x8,ne / csel w3,w12,w11,eq
  ⇒ adrp x2,0x1941000 / add x2,x2,#0x9ab / movz w3,#16 / nop x3
x2 = key ptr, w3 = key len（后面紧跟 add x4,sp,#..; mov x0,x20; bl decrypt）
"""
import struct, sys

SITES = [0x2FF760, 0x2FFD0C]
INSNS = [0xD000B202, 0x9126AC42, 0x52800203, 0xD503201F, 0xD503201F, 0xD503201F, 0xD503201F]


def patch(path, out):
    d = bytearray(open(path, "rb").read())
    for site in SITES:
        old = [struct.unpack_from("<I", d, site + 4 * i)[0] for i in range(7)]
        print("0x%x old: %s" % (site, " ".join("%08x" % w for w in old)))
        for i, w in enumerate(INSNS):
            struct.pack_into("<I", d, site + 4 * i, w)
        new = [struct.unpack_from("<I", d, site + 4 * i)[0] for i in range(4)]
        print("0x%x new: %s" % (site, " ".join("%08x" % w for w in new)))
    open(out, "wb").write(d)
    print("wrote", out, len(d))


if __name__ == "__main__":
    patch(sys.argv[1], sys.argv[2])
