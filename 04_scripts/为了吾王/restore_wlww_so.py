# -*- coding: utf-8 -*-
"""把 R2e 的 5 处静态补丁还原成官方原指令（功能改由菜单 hook 控制）。"""
import struct, sys
SITES = [
    ("B1", 0x1F62074, 0x528847E9, 0x5290D3E9),
    ("B2", 0x1F62078, 0x72A001E9, 0x72A00029),
    ("C",  0x1F6903C, 0x52800020, 0x1A9FB7E0),
    ("D",  0x1DBD5B4, 0x52800020, 0x1A9FD7E0),
    ("E",  0x2059C14, 0x52800020, 0x1A9F87E0),
    ("F",  0x1F5CA80, 0x149F0960, 0x2B1502C8),
]
CAVE_FILE=0x44E6000; CAVE_LEN=0x40
def main():
    src,dst = sys.argv[1], sys.argv[2]
    data=bytearray(open(src,"rb").read()); n=len(data)
    for k,off,exp,orig in SITES:
        got=struct.unpack_from("<I",data,off)[0]
        assert got==exp, f"{k}@{off:#x} 期望 {exp:#010x} 实得 {got:#010x}"
        struct.pack_into("<I",data,off,orig); print(f"  还原 {k}@{off:#x} -> {orig:#010x}")
    data[CAVE_FILE:CAVE_FILE+CAVE_LEN]=bytes(CAVE_LEN)
    print("  cave 清零")
    open(dst,"wb").write(bytes(data)); print("输出",dst,len(data))
main()
