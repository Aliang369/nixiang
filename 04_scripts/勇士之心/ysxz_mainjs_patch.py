#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""勇士之心 1.0.3 —— 生成「自举版」引导脚本 main.js（加密态）+ 供 so 自愈用的 C 头。

背景（2026-10-05）：
  * 引擎真正加载的是运行时 files/res/main.js（APK 内 assets/main.js 只在版本不一致时拷贝）；
  * main.js **不在** 服务端 version_update_md5 校验名单里（名单只有
    assets/{battleLogic,battleRender,game,main}/index.js 四个 bundle），所以可以改；
  * 模拟器上原生 evalString 钩子不生效 ⇒ 把自举补丁追加到 main.js 尾部即可，
    加密格式与游戏一致（b"*ze*" + 1B 标志 + XXTEA(zlib(plain))，前 16B 密钥；
    标志字节从底包继承，实测必须是 0x00 —— 0x01 会被引擎拒收并回退明文解析）。

本脚本一次产出（全部可复现）：
  1) <out>        加密后的 main.js（追加了自举补丁）
  2) <header>     C 头：kYsxzBootPristine / kYsxzBootPatched 两个字节数组
                  （so 启动时比对并自愈写回，见 YsxzMod.cpp ::YsxzBootSelfHeal）
  3) 控制台报告   pristine/patched 的 md5、明文长度、追加的补丁字节数（纯 ASCII）

用法：
  python3.11 ysxz_mainjs_patch.py --apk 02_apks/勇士之心_1.0.3.apk \
      --boot-js <libs_ysxz/ysxz_boot.js> --out <main.patched.enc.js> --header <YsxzBootBlob.h>
  # 也可以 --pristine FILE 直接给加密态 main.js
"""
from __future__ import annotations

import argparse
import hashlib
import struct
import zipfile
import zlib
from pathlib import Path

KEY = b"237GaogniB!@#$1sdf546685"
MAGIC = b"*ze*"
# 标志字节必须与底包一致：底包 = 0x00。实测 0x01 会被引擎拒绝，
# 回退按明文解析 ⇒ `Uncaught SyntaxError: Unexpected token '*', location: main.js:0:0`
# （2026-10-05 MuMu 实测）。下面 encrypt_main 从 pristine 继承该字节，不写死。
FLAG = b"\x00"
MARK = "/*starry_ysxz_boot_v1*/"
DELTA = 0x9E3779B9
MASK = 0xFFFFFFFF


# ---------------- XXTEA（与游戏一致，见 decrypt_hotupdate_ysxz.py） ----------------
def _mx(y, z, p, e, total, k):
    a = (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) & MASK
    return (a ^ (((total ^ y) & MASK) + (k[(p & 3) ^ e] ^ z))) & MASK


def xxtea_decrypt(data: bytes, key: bytes):
    if len(data) % 4 or len(data) < 8:
        return None
    n = len(data) // 4
    v = list(struct.unpack("<%dI" % n, data))
    k = list(struct.unpack("<4I", key[:16].ljust(16, b"\0")))
    q = 6 + 52 // n
    total = (q * DELTA) & MASK
    y = v[0]
    z = v[n - 1]
    while total:
        e = (total >> 2) & 3
        for p in range(n - 1, 0, -1):
            z = v[p - 1]
            y = (v[p] - _mx(y, z, p, e, total, k)) & MASK
            v[p] = y
        z = v[n - 1]
        y = (v[0] - _mx(y, z, 0, e, total, k)) & MASK
        v[0] = y
        total = (total - DELTA) & MASK
    plain_len = v[-1]
    if not (n * 4 - 7 <= plain_len <= n * 4 - 4):
        return None
    return struct.pack("<%dI" % n, *v)[:plain_len]


def xxtea_encrypt(data: bytes, key: bytes) -> bytes:
    n = (len(data) + 4 + 3) // 4
    v = list(struct.unpack("<%dI" % n, data.ljust(n * 4, b"\0")))
    v[-1] = len(data)
    k = list(struct.unpack("<4I", key[:16].ljust(16, b"\0")))
    q = 6 + 52 // n
    total = 0
    z = v[n - 1]
    for _ in range(q):
        total = (total + DELTA) & MASK
        e = (total >> 2) & 3
        for p in range(n - 1):
            y = v[p + 1]
            z = v[p] = (v[p] + _mx(y, z, p, e, total, k)) & MASK
        y = v[0]
        z = v[n - 1] = (v[n - 1] + _mx(y, z, n - 1, e, total, k)) & MASK
    return struct.pack("<%dI" % n, *v)


def decrypt_main(raw: bytes) -> str:
    if not raw.startswith(MAGIC):
        raise SystemExit("ERROR: main.js is not encrypted (*ze*): %r" % raw[:8])
    payload = raw[5:]
    payload = payload[: len(payload) // 4 * 4]
    inner = xxtea_decrypt(payload, KEY)
    if inner is None:
        raise SystemExit("ERROR: XXTEA decrypt failed")
    return zlib.decompress(inner).decode("utf-8", "surrogateescape")


def encrypt_main(plain: str, flag: bytes = FLAG) -> bytes:
    inner = zlib.compress(plain.encode("utf-8", "surrogateescape"), 9)
    return MAGIC + flag + xxtea_encrypt(inner, KEY)


def read_pristine(args) -> bytes:
    if args.pristine:
        return Path(args.pristine).read_bytes()
    with zipfile.ZipFile(args.apk) as z:
        return z.read("assets/main.js")


def c_array(name: str, data: bytes) -> str:
    lines = ["static const unsigned char %s[] = {" % name]
    for i in range(0, len(data), 16):
        chunk = data[i:i + 16]
        lines.append("    " + " ".join("0x%02x," % b for b in chunk))
    lines.append("};")
    lines.append("static const unsigned int %s_len = %d;" % (name, len(data)))
    return "\n".join(lines)


def main() -> int:
    ap = argparse.ArgumentParser()
    src = ap.add_mutually_exclusive_group(required=True)
    src.add_argument("--apk", help="base APK (read assets/main.js)")
    src.add_argument("--pristine", help="encrypted main.js file")
    ap.add_argument("--boot-js", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--header", required=True)
    ap.add_argument("--sym", default="YsxzBoot")
    a = ap.parse_args()

    raw = read_pristine(a)
    plain = decrypt_main(raw)
    boot = Path(a.boot_js).read_text(encoding="utf-8")

    if MARK in plain:  # 幂等：先剥掉上一次追加
        plain = plain[: plain.index(MARK)].rstrip()
    if "window.__starry_capture" in boot:
        raise SystemExit("ERROR: boot-js contains the native capture section -- wrong file?")

    patched_plain = plain.rstrip() + "\n\n" + MARK + "\n" + boot
    patched = encrypt_main(patched_plain)

    # 标志字节必须与底包逐字节一致（底包实测 0x00；写成 0x01 时引擎不认，直接回退明文解析）
    flag = raw[4:5]
    if len(flag) != 1 or flag != FLAG:
        raise SystemExit("ERROR: 底包 main.js 标志字节异常 %r（期望 %r）" % (flag, FLAG))
    patched = encrypt_main(patched_plain, flag)

    # 回读校验：解密必须是原文 + 补丁，且补丁标记只出现一次
    back = decrypt_main(patched)
    if back.count(MARK) != 1 or not back.startswith(plain.rstrip()):
        raise SystemExit("ERROR: round-trip check failed (mark=%d)" % back.count(MARK))

    out = Path(a.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_bytes(patched)

    hdr = Path(a.header)
    hdr.parent.mkdir(parents=True, exist_ok=True)
    hdr.write_text(
        "// AUTO-GENERATED by ysxz_mainjs_patch.py -- do not edit.\n"
        "// pristine = base APK assets/main.js (encrypted)\n"
        "// patched  = same plain JS + appended bootstrap patch, re-encrypted\n"
        "// so compares bytes at startup: pristine or missing => write patched.\n"
        "#pragma once\n\n"
        "static const char k%sMarkJs[] = \"%s\";\n\n%s\n\n%s\n"
        % (a.sym, MARK, c_array("k" + a.sym + "Pristine", raw),
           c_array("k" + a.sym + "Patched", patched)),
        encoding="utf-8",
    )

    def md5(b):
        return hashlib.md5(b).hexdigest()

    print("main.js pristine : %6d B  md5 %s" % (len(raw), md5(raw)))
    print("main.js patched  : %6d B  md5 %s" % (len(patched), md5(patched)))
    print("  plain          : %6d -> %6d B (+%d, boot-js %d B)"
          % (len(plain), len(patched_plain), len(patched_plain) - len(plain), len(boot)))
    print("  out            : %s" % out)
    print("  header         : %s" % hdr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
