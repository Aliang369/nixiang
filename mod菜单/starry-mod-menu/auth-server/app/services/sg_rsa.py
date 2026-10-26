# -*- coding: utf-8 -*-
"""星光 PKI —— 授权票 / 完整性清单签名（RSA-2048 PKCS#1 v1.5 + SHA-256）。

**唯一实现**：出包机脚本（`04_scripts/_common/sg_pki/`）与服务端都从这里取逻辑，
避免两份拷贝漂移（历史教训：pepper 在 Main.cpp / MainModern.cpp 各一份）。

为什么不是 HMAC：
    HMAC 的密钥必须同时存在于服务端与客户端 so 里 ⇒ 谁剥出 so 里的 pepper，
    谁就能给任意设备签票（"铸币权"）。改成非对称后客户端只有公钥模数，
    私钥只在服务端 / 出包机，破解者剥不动、签不出。

为什么是 RSA 而不是 Ed25519：
    出包环境无网络（取不到可审计的 curve25519 实现）、本机无 Windows C 编译器；
    RSA 验签实现面小（bignum 乘+模幂+逐字节 memcmp），C 侧已用 JDK 签名
    逐条对拍通过（x86_64 模拟器 + arm64 真机，16/16）。

签名口径（与 C 侧 `jni/Includes/sg_rsa_verify.h` 严格一致，勿改）：
    EM = 0x00 || 0x01 || 0xFF*PS || 0x00 || DigestInfo(SHA-256) || SHA256(msg)
    sig = pow(int(EM), d, n) → 定长 k 字节大端
验签口径：还原 EM 后**逐字节比较**，不做宽松解析（防 Bleichenbacher 型伪造）。

私钥文件格式（`key=hex` 每行一个）：n, e, d, p, q, dp, dq, qinv
—— 只在服务器 / 出包机，**绝不入库、绝不进 APK**。
"""
from __future__ import annotations

import hashlib
import os
import threading

SHA256_DIGESTINFO = bytes.fromhex("3031300d060960864801650304020105000420")

_lock = threading.Lock()
_cache: dict[str, dict] = {}


def load_key(path: str) -> dict:
    """读私钥文件（key=hex）。带进程内缓存，避免每次发票都读盘。"""
    if not path:
        raise ValueError("key path 为空")
    ap = os.path.abspath(path)
    with _lock:
        if ap in _cache:
            return _cache[ap]
        kv: dict[str, int] = {}
        with open(ap, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if not line or line.startswith("#") or "=" not in line:
                    continue
                k, v = line.split("=", 1)
                kv[k.strip()] = int(v.strip(), 16)
        for need in ("n", "e", "d"):
            if need not in kv:
                raise ValueError("私钥文件缺字段 %s" % need)
        _cache[ap] = kv
        return kv


def key_fingerprint(key: dict) -> str:
    """公钥指纹 = sha256(模数) 前 16 hex，用来核对「客户端公钥 == 服务端公钥」。"""
    k = key_bytes(key)
    return hashlib.sha256(key["n"].to_bytes(k, "big")).hexdigest()[:16]


def key_bytes(key: dict) -> int:
    return (key["n"].bit_length() + 7) // 8


def _em(msg: bytes, k: int) -> bytes:
    t = SHA256_DIGESTINFO + hashlib.sha256(msg).digest()
    if k < len(t) + 11:
        raise ValueError("key too short for PKCS#1 v1.5")
    ps = b"\xff" * (k - len(t) - 3)
    return b"\x00\x01" + ps + b"\x00" + t


def sign(msg: bytes, key: dict) -> bytes:
    n, d = key["n"], key["d"]
    k = key_bytes(key)
    em = _em(msg, k)
    return pow(int.from_bytes(em, "big"), d, n).to_bytes(k, "big")


def verify(msg: bytes, sig: bytes, key: dict) -> bool:
    n, e = key["n"], key["e"]
    k = key_bytes(key)
    if len(sig) != k:
        return False
    got = pow(int.from_bytes(sig, "big"), e, n).to_bytes(k, "big")
    return got == _em(msg, k)


def sign_hex(msg: bytes, key: dict) -> str:
    return sign(msg, key).hex()


def verify_hex(msg: bytes, sig_hex: str, key: dict) -> bool:
    try:
        sig = bytes.fromhex(sig_hex)
    except ValueError:
        return False
    return verify(msg, sig, key)


if __name__ == "__main__":
    import sys
    key = load_key(sys.argv[1])
    print("k_bytes =", key_bytes(key), "fp =", key_fingerprint(key))
    m = b"STARRY-SELFTEST"
    s = sign(m, key)
    print("verify  =", verify(m, s, key))
    print("tamper  =", verify(m + b"x", s, key))
