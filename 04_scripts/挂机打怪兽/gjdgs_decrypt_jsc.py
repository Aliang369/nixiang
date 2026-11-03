#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""挂机打怪兽 (org.cocos2d.guajidaguaishou.tap 1.1.65) —— jsc 解密
XXTEA(key="74f38ae4-b95f-42") -> gunzip
key 来源：运行时读 libcocos2djs.so 的 jsb_set_xxtea_key 目标全局字符串
  （符号 0x77b270，写入 0x1b4fe98 的 std::string；1.1.65 实测值 74f38ae4-b95f-42）
用法：python3 gjdgs_decrypt_jsc.py [apk路径]
产物：13_scratch/gjdgs_js/*.dec.js
"""
import gzip, io, struct, sys, zipfile, zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
APK = Path(sys.argv[1]) if len(sys.argv) > 1 else ROOT / "02_apks" / "原版_TapTap_20261103" / "guaji_daguaishou.apk"
OUT = ROOT / "13_scratch" / "gjdgs_js"
KEY = b"74f38ae4-b95f-42"
TARGETS = {
    "assets/assets/main/index.17a32.jsc": "index.17a32.dec.js",
    "assets/src/settings.79641.jsc": "settings.dec.js",
}


def xxtea_decrypt(data: bytes, key: bytes) -> bytes:
    n = len(data) // 4
    v = list(struct.unpack("<%dI" % n, data))
    k = list(struct.unpack("<%dI" % (len(key) // 4), key))
    delta = 0x9E3779B9
    q = 6 + 52 // n
    sm = (q * delta) & 0xFFFFFFFF
    y = v[0]
    while sm:
        e = (sm >> 2) & 3
        for p in range(n - 1, -1, -1):
            z = v[p - 1] if p > 0 else v[n - 1]
            mx = ((((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((sm ^ y) + (k[(p & 3) ^ e] ^ z))) & 0xFFFFFFFF
            v[p] = (v[p] - mx) & 0xFFFFFFFF
            y = v[p]
        sm = (sm - delta) & 0xFFFFFFFF
    return struct.pack("<%dI" % n, *v)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    z = zipfile.ZipFile(APK)
    for src, dst in TARGETS.items():
        d = z.read(src)
        if len(d) % 4:
            d += b"\x00" * (4 - len(d) % 4)
        dec = xxtea_decrypt(d, KEY)
        try:
            dec = zlib.decompressobj(16 + zlib.MAX_WBITS).decompress(dec)
        except Exception as e:
            print("gunzip fail:", e)
        (OUT / dst).write_bytes(dec)
        print("%s -> %s (%d bytes)" % (src, dst, len(dec)))


if __name__ == "__main__":
    main()
