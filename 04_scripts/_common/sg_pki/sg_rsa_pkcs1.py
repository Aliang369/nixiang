# -*- coding: utf-8 -*-
"""星光 PKI —— 票签名（PKCS#1 v1.5 + SHA-256）参考实现，纯标准库，无第三方依赖。

用途：
  1) 服务端 `auth-server` 用它给授权票签名（私钥只在服务器 / 出包机）；
  2) 出包机 `harden_apk.py` 用它给完整性清单签名；
  3) 作为 C 侧 `sg_rsa_verify.c` 的对拍 oracle（同一份算法，两份独立实现）。

签名口径（与 C 侧严格一致，勿改）：
    EM = 0x00 || 0x01 || 0xFF * PS || 0x00 || T
    T  = DigestInfo(SHA-256) || SHA256(msg)
    sig = pow(int(EM), d, n)  → 定长 k 字节（大端）
验签口径：
    m = pow(int(sig), e, n) → 还原 EM → **逐字节 memcmp 期望的 EM**（不做宽松解析，
    规避 Bleichenbacher 型伪造）

密钥文件格式（key=hex，每行一个）：n, e, d, p, q, dp, dq, qinv
"""
import hashlib

# DigestInfo prefix for SHA-256 (RFC 8017 §9.2, 19 bytes)
SHA256_DIGESTINFO = bytes.fromhex("3031300d060960864801650304020105000420")


def load_key(path):
    kv = {}
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or "=" not in line:
                continue
            k, v = line.split("=", 1)
            kv[k.strip()] = int(v.strip(), 16)
    return kv


def _em(msg, k):
    t = SHA256_DIGESTINFO + hashlib.sha256(msg).digest()
    if k < len(t) + 11:
        raise ValueError("key too short for PKCS#1 v1.5")
    ps = b"\xff" * (k - len(t) - 3)
    return b"\x00\x01" + ps + b"\x00" + t


def sign(msg, key):
    n, d = key["n"], key["d"]
    k = (n.bit_length() + 7) // 8
    em = _em(msg, k)
    s = pow(int.from_bytes(em, "big"), d, n)
    return s.to_bytes(k, "big")


def verify(msg, sig, key):
    n, e = key["n"], key["e"]
    k = (n.bit_length() + 7) // 8
    if len(sig) != k:
        return False
    m = pow(int.from_bytes(sig, "big"), e, n)
    got = m.to_bytes(k, "big")
    return got == _em(msg, k)


if __name__ == "__main__":
    import sys
    key = load_key(sys.argv[1])
    # 与 JDK 交叉对拍：PKCS#1 v1.5 是确定性签名，两边应逐字节相同
    jdk_sig = bytes.fromhex(open(sys.argv[1] + ".jdk_sig", "r", encoding="utf-8").read().strip())
    mine = sign(b"STARRY-SELFTEST", key)
    print("python==jdk :", mine == jdk_sig)
    print("verify(self) :", verify(b"STARRY-SELFTEST", mine, key))
    print("tamper(msg)  :", verify(b"STARRY-SELFTESX", mine, key))
    bad = bytearray(mine)
    bad[100] ^= 0x01
    print("tamper(sig)  :", verify(b"STARRY-SELFTEST", bytes(bad), key))
    print("k_bytes      :", (key["n"].bit_length() + 7) // 8)
