#!/usr/bin/env python3
"""Cocos 2.4 JSC codec for 动物大联盟 1.0.23.

Format: XXTEA(gzip(plain) + uint32_le(gzip_len) + 0x00, pad-to-4)
The same key is used by the runtime script bundle loader.
"""
import gzip
import struct
import sys
import zlib
from pathlib import Path

KEY = b"d03398ba-c326-44"
BOOT_KEY = b"237GaogniB!@#$1sdf546685"
DELTA = 0x9E3779B9
MASK = 0xFFFFFFFF


def _mx(sum_, y, z, p, e, k):
    return ((((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^
            ((sum_ ^ y) + (k[(p & 3) ^ e] ^ z))) & MASK


def _key_words(key):
    key = key.ljust(16, b"\0")[:16]
    return list(struct.unpack("<4I", key))


def xxtea_decrypt(data, key):
    if len(data) < 8:
        return b""
    n = len(data) // 4
    v = list(struct.unpack("<%dI" % n, data[: n * 4]))
    k = _key_words(key)
    q = 6 + 52 // n
    sum_ = (q * DELTA) & MASK
    y = v[0]
    while sum_ != 0:
        e = (sum_ >> 2) & 3
        for p in range(n - 1, 0, -1):
            z = v[p - 1]
            v[p] = (v[p] - _mx(sum_, y, z, p, e, k)) & MASK
            y = v[p]
        z = v[n - 1]
        v[0] = (v[0] - _mx(sum_, y, z, 0, e, k)) & MASK
        y = v[0]
        sum_ = (sum_ - DELTA) & MASK
    return struct.pack("<%dI" % n, *v)


def xxtea_encrypt(data, key):
    if not data:
        return b""
    if len(data) % 4:
        data = data + b"\0" * (4 - len(data) % 4)
    n = len(data) // 4
    v = list(struct.unpack("<%dI" % n, data))
    k = _key_words(key)
    q = 6 + 52 // n
    sum_ = 0
    z = v[n - 1]
    for _ in range(q):
        sum_ = (sum_ + DELTA) & MASK
        e = (sum_ >> 2) & 3
        for p in range(n - 1):
            y = v[p + 1]
            v[p] = (v[p] + _mx(sum_, y, z, p, e, k)) & MASK
            z = v[p]
        y = v[0]
        v[n - 1] = (v[n - 1] + _mx(sum_, y, z, n - 1, e, k)) & MASK
        z = v[n - 1]
    return struct.pack("<%dI" % n, *v)


def decrypt(raw):
    if len(raw) % 4:
        raw = raw + b"\0" * (4 - len(raw) % 4)
    dec = xxtea_decrypt(raw, KEY)
    stream = zlib.decompressobj(31)
    plain = stream.decompress(dec)
    plain += stream.flush()
    if not stream.eof:
        raise ValueError("gzip stream incomplete")
    return plain


def encrypt(plain):
    packed = gzip.compress(plain, mtime=0)
    blob = packed + struct.pack("<I", len(packed)) + b"\0"
    if len(blob) % 4:
        blob += b"\0" * (4 - len(blob) % 4)
    return xxtea_encrypt(blob, KEY)


def decrypt_ze(raw):
    if not raw.startswith(b"*ze*\0"):
        raise ValueError("not a *ze* container")
    dec = xxtea_decrypt(raw[5:], BOOT_KEY)
    return zlib.decompress(dec, 15)


def encrypt_ze(plain):
    packed = zlib.compress(plain, 6)
    return b"*ze*\0" + xxtea_encrypt(packed, BOOT_KEY)


def main():
    modes = ("decrypt", "encrypt", "decrypt-ze", "encrypt-ze")
    if len(sys.argv) != 4 or sys.argv[1] not in modes:
        raise SystemExit("usage: dwdlm_jsc_tool.py " + "|".join(modes) + " <in> <out>")
    mode, src, dst = sys.argv[1], Path(sys.argv[2]), Path(sys.argv[3])
    raw = src.read_bytes()
    if mode == "decrypt":
        out = decrypt(raw)
    elif mode == "encrypt":
        out = encrypt(raw)
    elif mode == "decrypt-ze":
        out = decrypt_ze(raw)
    else:
        out = encrypt_ze(raw)
    dst.parent.mkdir(parents=True, exist_ok=True)
    dst.write_bytes(out)
    print(f"{mode}: {src} -> {dst} ({len(raw)} -> {len(out)} bytes)")


if __name__ == "__main__":
    main()
