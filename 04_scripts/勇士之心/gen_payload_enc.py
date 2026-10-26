# -*- coding: utf-8 -*-
"""把功能载荷（Games/YsxzPatchJs.h 里的明文 JS）加密成密文头（P2-2 第一层）。

设计要点：
  · K_build 由主密钥 + 载荷内容 sha256 派生 ⇒ 同一个载荷永远同一把钥（可复现），
    但**不写进头文件**（只写 ct/mac/len），钥存到 ~/.starry-keys/payload/<sha>.key。
  · 运行时用票里的 kx 还原 K_build（kx = K_build XOR sha256("sgpay"||sig)），
    这一步在服务器 v3 票里；本脚本只负责出密文 + 生成钥。
  · 默认不改变现有构建：产物头需显式 -DSG_PAYLOAD_ENC 才被 YsxzMod.cpp 使用。

用法：
  python gen_payload_enc.py            # 生成 Includes/sg_payload_enc.h + 钥文件
  python gen_payload_enc.py --check     # 校验现有头与当前载荷是否一致（打包门禁用）
"""
import argparse
import hashlib
import hmac
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
JNI = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni")
# 必须与构建脚本用的**同一份**（ysxz_build_so_103.sh 里 MENU_DIR 指向它；
# 2026-10-27 踩过：mod菜单 下同名文件内容不同（113,060 vs 120,057 B），加密错源头
# ⇒ 注入的是旧载荷，部分功能失效）
SRC = os.path.join(ROOT, "04_scripts", "勇士之心", "ysxz_1.0.3_menu", "YsxzPatchJs.h")
OUT = os.path.join(JNI, "Includes", "sg_payload_enc.h")
MASTER = os.path.join(os.path.expanduser("~"), ".starry-keys", "payload_master.txt")
KEYDIR = os.path.join(os.path.expanduser("~"), ".starry-keys", "payload")


def load_master(create=True):
    if os.path.exists(MASTER):
        txt = io.open(MASTER, "r", encoding="ascii").read().strip()
        if len(txt) >= 32:
            return bytes.fromhex(txt[:64])
    if not create:
        raise SystemExit("主密钥缺失：%s" % MASTER)
    os.makedirs(os.path.dirname(MASTER), exist_ok=True)
    k = os.urandom(32)
    io.open(MASTER, "w", encoding="ascii", newline="\n").write(k.hex() + "\n")
    print("已生成主密钥（务必与服务器共用同一份）：%s" % MASTER)
    return k


def extract_payload():
    s = io.open(SRC, "r", encoding="utf-8", errors="surrogateescape").read()
    m = re.search(r'R"YSXZJS\((.*)\)YSXZJS"', s, re.S)
    if not m:
        raise SystemExit("载荷头里没找到 R\"YSXZJS(...)YSXZJS\" 段")
    return m.group(1).encode("utf-8", "surrogateescape")


def build_key(master, payload):
    digest = hashlib.sha256(payload).digest()
    return hmac.new(master, b"sgpay-build|" + digest, hashlib.sha256).digest(), digest


def keystream_xor(key, data):
    out = bytearray(data)
    ctr = 0
    for off in range(0, len(out), 32):
        blk = hmac.new(key, b"sgp1" + ctr.to_bytes(4, "big"), hashlib.sha256).digest()
        m = min(32, len(out) - off)
        for i in range(m):
            out[off + i] ^= blk[i]
        ctr += 1
    return bytes(out)


def enc(key, plain):
    ct = keystream_xor(key, plain)
    mac = hmac.new(key, b"sgm1" + ct, hashlib.sha256).digest()
    return ct, mac


def emit(ct, mac, plain_sha):
    lines = [
        "#pragma once",
        "/* 自动生成，勿手改：python 04_scripts/勇士之心/gen_payload_enc.py */",
        "/* 明文载荷 sha256 = %s，密文 %d B（K_build 不在此文件内）*/" % (plain_sha, len(ct)),
        "#include <stdint.h>",
        "static const uint32_t kYsxzPayloadLen = %du;" % len(ct),
        "static const uint8_t kYsxzPayloadMac[32] = {%s};" % ",".join("%d" % b for b in mac),
        "static const uint8_t kYsxzPayloadCt[%d] = {" % len(ct),
    ]
    for i in range(0, len(ct), 16):
        lines.append("    " + ",".join("%d" % b for b in ct[i:i + 16]) + ",")
    lines.append("};")
    lines.append('static const char* kYsxzPayloadSha = "%s";' % plain_sha)
    io.open(OUT, "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true")
    a = ap.parse_args()

    payload = extract_payload()
    master = load_master(create=not a.check)
    key, digest = build_key(master, payload)
    sha = digest.hex()

    if a.check:
        if not os.path.exists(OUT):
            print("FAIL 缺少 sg_payload_enc.h（未跑 gen_payload_enc.py）")
            return 1
        head = io.open(OUT, "r", encoding="utf-8").read()
        if sha not in head:
            print("FAIL 密文头与当前载荷不一致（sha %s 不在头里）—— 载荷改过但没重新加密" % sha[:16])
            return 1
        print("PASS 载荷密文与当前明文一致 sha=%s…" % sha[:16])
        return 0

    ct, mac = enc(key, payload)
    os.makedirs(KEYDIR, exist_ok=True)
    kf = os.path.join(KEYDIR, sha + ".key")
    io.open(kf, "w", encoding="ascii", newline="\n").write(key.hex() + "\n")
    emit(ct, mac, sha)
    print("明文 %d B  →  密文 %d B" % (len(payload), len(ct)))
    print("载荷 sha256 = %s" % sha)
    print("K_build 已写: %s（服务器签发 v3 票要用它算 kx）" % kf)
    print("密文头已写: %s" % OUT)
    return 0


if __name__ == "__main__":
    sys.exit(main())
