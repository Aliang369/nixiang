#!/usr/bin/env python3.11
# .ZTG 符号名提取: ZTGame 定制 HybridCLR 预解析镜像内含长度前缀的明文标识符
# 用法: python3.11 extract_ztg_symbols.py  ->  06_dumped/月圆之夜/hotfix_dlls/symbols_*.txt
import re, glob, os

OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/hotfix_dlls"
IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_`<>\+\.\[\]=\-/]*$")


def extract_idents(b: bytes):
    """扫描 [len][ascii] 对: 1字节或7bit长度前缀, 名字为标识符字符集"""
    res = set()
    n = len(b)
    for i in range(n - 2):
        L = b[i]
        if 6 <= L <= 220 and i + 1 + L <= n:
            s = b[i + 1:i + 1 + L]
            if IDENT.match(s) and (b"." in s or b"+" in s or (b[i] < 0x80 and s[:1].isalpha())):
                res.add(s.decode())
        # 7-bit 两字节长度 (0x80|lo, hi): 名字 >=128
        if b[i] & 0x80 and i + 2 <= n:
            L = ((b[i] & 0x7F) | (b[i + 1] << 7))
            if 6 <= L <= 220 and i + 2 + L <= n:
                s = b[i + 2:i + 2 + L]
                if IDENT.match(s):
                    res.add(s.decode())
    return res


for f in sorted(glob.glob(f"{OUT}/hotfix_*.dll")):
    b = open(f, "rb").read()
    idents = extract_idents(b)
    name = os.path.basename(f)[len("hotfix_"):-len(".dll")]
    outp = f"{OUT}/symbols_{name}.txt"
    open(outp, "w").write("\n".join(sorted(idents)))
    print(f"{name}: {len(idents)} 标识符 -> {os.path.basename(outp)}")
