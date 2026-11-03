#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""修仙大陆模拟器 (com.sky.xxdl.dlt 1.0.5) —— index.jsc 离线解密
XXTEA(key) -> 明文（本作不套 gzip，解出来直接是 System.register 模块源码）

key 来源（2026-11-04 实测）：libcocos.so 的 _ZL8xxteaKey（.bss，vaddr 0x16d7b70）
  运行时是 std::string SSO：首字节 = size<<1 = 0x20 => 长度 16
  真机读取：基址 = /proc/<pid>/maps 里 libcocos.so 首行起址，读基址+0x16d7b70 32 字节
  实测值：6aMz7p1tlGtr6MeR（换版本必须重取；静态推导不可行，jsb_set_xxtea_key 无静态调用点）

用法：python3 xxdl_decrypt_jsc.py [apk路径]
产物：13_scratch/xxdl_js/index.dec.js
"""
import struct
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
APK = Path(sys.argv[1]) if len(sys.argv) > 1 else ROOT / "02_apks" / "原版_TapTap_20261103" / "sky_xxdl_dlt.apk"
OUT = ROOT / "13_scratch" / "xxdl_js"
KEY = b"6aMz7p1tlGtr6MeR"
TARGETS = {
    "assets/assets/main/index.jsc": "index.dec.js",
    "assets/assets/game/index.jsc": "game.dec.js",
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
        if src not in z.namelist():
            print("skip (missing):", src)
            continue
        d = z.read(src)
        if len(d) % 4:
            d += b"\x00" * (4 - len(d) % 4)
        out = xxtea_decrypt(d, KEY)
        (OUT / dst).write_bytes(out)
        print("%s -> %s (%d bytes) head=%s" % (src, dst, len(out), out[:12]))


if __name__ == "__main__":
    main()
