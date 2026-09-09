#!/usr/bin/env python3.11
# XXTEA 解密 v3: xxtea-c 约定(长度在最后一个uint) + 多偏移(0/32/64) + 双候选key
# 依据: HybridClrXlua_XXTeaDecrypt@0x14d2f24 = xxtea-c 标准实现, key零填充到16B
import struct

DELTA = 0x9E3779B9


def mx(s, y, z, p, e, k):
    return (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((s ^ y) + (k[(p & 3) ^ e] ^ z))


def dec_uints(v, k):
    n = len(v)
    if n < 2:
        return v
    rounds = 6 + 52 // n
    s = (rounds * DELTA) & 0xFFFFFFFF
    y = v[0]
    while s != 0:
        e = (s >> 2) & 3
        for p in range(n - 1, 0, -1):
            z = v[p - 1]
            y = v[p] = (v[p] - mx(s, y, z, p, e, k)) & 0xFFFFFFFF
        z = v[n - 1]
        y = v[0] = (v[0] - mx(s, y, z, 0, e, k)) & 0xFFFFFFFF
        s = (s - DELTA) & 0xFFFFFFFF
    return v


def enc_uints(v, k):
    n = len(v)
    if n < 2:
        return v
    rounds = 6 + 52 // n
    s = 0
    z = v[n - 1]
    e = 0
    for _ in range(rounds):
        s = (s + DELTA) & 0xFFFFFFFF
        e = (s >> 2) & 3
        for p in range(n - 1):
            y = v[p + 1]
            z = v[p] = (v[p] + mx(s, y, z, p, e, k)) & 0xFFFFFFFF
        y = v[0]
        z = v[n - 1] = (v[n - 1] + mx(s, y, z, n - 1, e, k)) & 0xFFFFFFFF
    return v


def fix_key(key: bytes) -> bytes:
    return key[:16] if len(key) > 16 else key + b"\x00" * (16 - len(key))


def xxtea_decrypt(data: bytes, key: bytes):
    """xxtea-c 约定: 明文真实长度存于最后一个 uint"""
    n = len(data) // 4
    v = list(struct.unpack("<%dI" % n, data[: n * 4]))
    k = list(struct.unpack("<4I", fix_key(key)))
    v = dec_uints(v, k)
    outlen = v[-1]
    if not ((n - 1) * 4 - 3 <= outlen <= (n - 1) * 4):
        return None
    return struct.pack("<%dI" % (n - 1), *v[:-1])[:outlen]


def raw_decrypt(data: bytes, key: bytes) -> bytes:
    """不做长度校验, 返回全部解密字节(前n-1个uint), 供魔数探测"""
    n = len(data) // 4
    v = list(struct.unpack("<%dI" % n, data[: n * 4]))
    k = list(struct.unpack("<4I", fix_key(key)))
    v = dec_uints(v, k)
    return struct.pack("<%dI" % (n - 1), *v[:-1])


def xxtea_encrypt(data: bytes, key: bytes):
    n_words = (len(data) + 3) // 4 + 1  # includeLength, 长度存最后一个uint
    pad = data + b"\x00" * ((n_words - 1) * 4 - len(data))
    v = list(struct.unpack("<%dI" % (n_words - 1), pad)) + [len(data)]
    k = list(struct.unpack("<4I", fix_key(key)))
    v = enc_uints(v, k)
    return struct.pack("<%dI" % n_words, *v)


# 自校验(末uint约定)
msg = b"Hello xxtea self-test 1234567890"
ct = xxtea_encrypt(msg, b"yyzy10000yyzy0")
pt = xxtea_decrypt(ct, b"yyzy10000yyzy0")
print("自校验:", "PASS" if pt == msg else f"FAIL {pt!r}")

SRC = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_dump_20260907/yyzy_il2cpp/assets/yoo/DefaultPackage"
KEYS = [b"yyzy1.0.055yyzy0", b"yyzy10000yyzy0", b"yyzy1055yyzy0"]

import lzma


def lzma_decompress(data: bytes):
    """游戏 Utility 用 SevenZip LZMA(裸流, 5B props + 8B size 或纯流), 两种都试"""
    try:
        # 带 props 头 (5B props + 8B uncompressed size)
        return lzma.LZMADecompressor(format=lzma.FORMAT_ALONE).decompress(data)
    except Exception:
        pass
    try:
        return lzma.decompress(data)
    except Exception:
        return None


def parse_container(out: bytes):
    """BinaryReader 格式: int32 count + (string name, int32 len, byte[] data)*count"""
    try:
        pos = 0
        cnt = int.from_bytes(out[pos:pos + 4], "little"); pos += 4
        if not (0 < cnt < 64):
            return None
        items = []
        for _ in range(cnt):
            nlen = int.from_bytes(out[pos:pos + 4], "little"); pos += 4  # BinaryWriter 7-bit? 先按 length-prefixed
            if not (0 < nlen < 256):
                return None
            name = out[pos:pos + nlen].decode("utf-8", "replace"); pos += nlen
            dlen = int.from_bytes(out[pos:pos + 4], "little"); pos += 4
            if not (0 < dlen <= len(out)):
                return None
            items.append((name, dlen))
            pos += dlen
        return items
    except Exception:
        return None


# ============================================================
# 最终解密链 (2026-09-08 实证通过):
#   rawfile 原样(无XOR/无跳头) -> XXTEA(delta=0x9E3779B9, key=UTF8(password)零填充16B,
#   includeLength约定) -> LZMA alone流(5B props + 8B size) -> 容器
#   容器 = BinaryWriter: int32 count + (7bitLen string name + int32 len + byte[] data)*count
#   password: hotfix包='yyzy_hotfix'  base包='yyzy_base'
#   证据链: LoadDLL.DecompressBytes@0x1D9F454(bl 0x1D9F520->XXTEA.Decrypt@0x1D9FB0C);
#   password槽0x5107e78运行时->record 0x5316358->StrLit idx17198='yyzy_hotfix'(内存dump实锤);
#   托管delta实锤: Decrypt(uint[],uint[])@0x1DB9188 内 mov #0x8647/movk #0x61c8 = 0x61C88647 = -DELTA
#   dump.cs 'delta=14713' 系混淆损坏常量, 以反汇编为准
import lzma, os

OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/hotfix_dlls"
SRC = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_dump_20260907/yyzy_il2cpp/assets/yoo/DefaultPackage"


def read7bit(d, p):
    r = 0; s = 0
    while True:
        b = d[p]; p += 1; r |= (b & 0x7F) << s
        if not b & 0x80:
            return r, p
        s += 7


def parse_container(dec: bytes):
    """int32 count + (7bitLen name + int32 len + bytes)*count; 返回[(name, offset, len)]"""
    p = 0
    cnt = int.from_bytes(dec[p:p + 4], "little"); p += 4
    if not (0 < cnt < 64):
        raise ValueError(f"count 异常 {cnt}")
    items = []
    for _ in range(cnt):
        n, p = read7bit(dec, p)
        name = dec[p:p + n].decode("utf-8", "replace"); p += n
        dlen = int.from_bytes(dec[p:p + 4], "little"); p += 4
        items.append((name, p, dlen))
        p += dlen
    assert p == len(dec), f"容器尾差 {p} != {len(dec)}"
    return items


def extract(tag: str, fid: str, key: bytes):
    data = open(f"{SRC}/{fid}.rawfile", "rb").read()
    out = xxtea_decrypt(data, key)          # 含长度校验, 命中即可信
    assert out is not None, f"{tag} XXTEA 失败"
    dec = lzma.LZMADecompressor(format=lzma.FORMAT_ALONE).decompress(out)
    items = parse_container(dec)
    print(f"== {tag}: {len(data)}B -> xxtea {len(out)}B -> lzma {len(dec)}B, {len(items)} 项")
    for name, off, dlen in items:
        blob = dec[off:off + dlen]
        safe = name.replace("/", "_")
        open(f"{OUT}/{tag}_{safe}", "wb").write(blob)
        print(f"   {name}: {dlen}B magic={blob[:4].hex()}")
    return items


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    extract("hotfix", "ccdda85485ad592a3c5deac4b801609c", b"yyzy_hotfix")
    extract("base", "ed5e2ed0517719a5a5d0e2ed998557d9", b"yyzy_base")
