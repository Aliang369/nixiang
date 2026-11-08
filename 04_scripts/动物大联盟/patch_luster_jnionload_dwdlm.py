#!/usr/bin/env python3
"""动物大联盟 1.0.25 · libluster.so JNI_OnLoad 去注册补丁

现象：libluster.so 的 JNI_OnLoad 里 FindClass("com/bytedance/ads/convert/broadcast/common/
EncryptionTools") 返回 null（该 so 由 native 侧 dlopen 时，classloader 退化为系统 loader），
随后 RegisterNatives 拿到 null class → JNI abort（SIGABRT, java_class == null）。

做法：把 JNI_OnLoad 里 "bl 0x198c"（内部 FindClass+RegisterNatives）改成 nop，
函数直接按已取到的 GetVersion 结果返回（0x10006）。Java 侧认为加载成功；
若真有代码调用 EncryptionTools 的 native 方法才会 UnsatisfiedLinkError（主流程不碰）。
"""
import hashlib, struct, sys
from pathlib import Path

NOP = 0x1F2003D5
TARGET_VA = 0x5FEC          # JNI_OnLoad+0x58: bl 0x198c
EXPECT_INSNS = {0x5FE4: 0x94000000, 0x5FEC: 0x94000000}  # bl 族（高 6 位 100101）


def va_to_off(d, va):
    e_phoff = struct.unpack_from("<Q", d, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", d, 0x36)[0]
    e_phnum = struct.unpack_from("<H", d, 0x38)[0]
    for i in range(e_phnum):
        o = e_phoff + i * e_phentsize
        p_type, p_flags = struct.unpack_from("<II", d, o)
        p_offset, p_vaddr, p_paddr, p_filesz, p_memsz = struct.unpack_from("<QQQQQ", d, o + 8)
        if p_type == 1 and p_vaddr <= va < p_vaddr + p_filesz:
            return p_offset + (va - p_vaddr)
    return None


def main(argv):
    if len(argv) < 3:
        print("usage: patch_luster_jnionload_dwdlm.py <in.so> <out.so>")
        return 2
    src, dst = Path(argv[1]), Path(argv[2])
    d = bytearray(src.read_bytes())
    print("in  : %s %d B" % (src, len(d)))
    off = va_to_off(d, TARGET_VA)
    if off is None:
        print("FAIL: 0x%x 不在任何 PT_LOAD 内" % TARGET_VA)
        return 1
    insn = struct.unpack_from("<I", d, off)[0]
    print("target va=0x%x off=0x%x insn=0x%08x" % (TARGET_VA, off, insn))
    if (insn >> 26) != 0x25:          # bl = 100101
        print("FAIL: 该处不是 bl（0x%08x），拒绝盲改" % insn)
        return 1
    struct.pack_into("<I", d, off, NOP)
    dst.write_bytes(bytes(d))
    chk = bytearray(dst.read_bytes())
    assert struct.unpack_from("<I", chk, off)[0] == NOP
    print("out : %s sha256=%s" % (dst, hashlib.sha256(bytes(d)).hexdigest()))
    print("OK: JNI_OnLoad 的 RegisterNatives 调用已 nop（跳过加密工具类注册）")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
