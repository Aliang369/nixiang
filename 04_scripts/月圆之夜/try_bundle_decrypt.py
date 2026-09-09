#!/usr/bin/env python3.11
# 尝试解密 YooAsset bundle 头：XXTEA / XOR 候选密钥
import struct

B1 = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_dump_20260907/yyzy_il2cpp/assets/yoo/DefaultPackage/ccdda85485ad592a3c5deac4b801609c.rawfile"
B2 = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_dump_20260907/yyzy_il2cpp/assets/yoo/DefaultPackage/ed5e2ed0517719a5a5d0e2ed998557d9.rawfile"

def xxtea_decrypt(data, key):
    """标准 XXTEA ( wikipedia ), data 长度须为 4 的倍数, key 16B"""
    n = len(data) // 4
    v = list(struct.unpack("<%dI" % n, data))
    k = list(struct.unpack("<4I", key))
    delta = 0x9E3779B9
    rounds = 6 + 52 // n
    s = (rounds * delta) & 0xFFFFFFFF
    while s != 0:
        e = (s >> 2) & 3
        for p in range(n - 1, 0, -1):
            z = v[p - 1]
            y = v[(p + 1) % n]
            mx = (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((s ^ y) + (k[(p & 3) ^ e] ^ z))
            v[p] = (v[p] - mx) & 0xFFFFFFFF
        z = v[n - 1]
        y = v[0]
        mx = (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((s ^ y) + (k[0 ^ e] ^ z))
        v[0] = (v[0] - mx) & 0xFFFFFFFF
        s = (s - delta) & 0xFFFFFFFF
    return struct.pack("<%dI" % n, *v)

keys = [b"yyzy10000yyzy0\x00\x00", b"yyzy{0}yyzy0\x00\x00\x00", b"yyzy10000yyzy0\x00\x00"[:16],
        b"yyzy10000yyzy00", b"yyzy10000yyzy0yy"]
for path in (B1, B2):
    head = open(path, "rb").read(256)
    print("==", path.split("/")[-1], "==")
    print("  raw head:", head[:16].hex(" "))
    for ki, key in enumerate(keys):
        try:
            dec = xxtea_decrypt(head[:64], key)
        except Exception as e:
            print("  key%d %r err %s" % (ki, key, e)); continue
        mark = "★★★" if dec[:7] == b"UnityFS" else ""
        print("  key%d %r xxtea: %s %s" % (ki, key, dec[:16].hex(" "), mark))
    # XOR 循环
    for ki, key in enumerate(keys):
        dec = bytes(c ^ key[i % len(key)] for i, c in enumerate(head[:16]))
        if dec[:7] == b"UnityFS":
            print("  key%d XOR ★★★: %s" % (ki, dec[:16].hex(" ")))
    # 单字节 XOR
    for k in range(256):
        if bytes(c ^ k for c in head[:7]) == b"UnityFS":
            print("  单字节 XOR key=0x%02x ★★★" % k)
