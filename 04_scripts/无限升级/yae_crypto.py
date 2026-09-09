#!/usr/bin/env python3
# 无限升级 · YooAssetAesCrypto (YAES) 解密/加密工具
# 还原来源：2026-09-05 真机 memdump libil2cpp 运行时镜像 → 反汇编
#   CreateAesKey/DecryptBytes/CreateDeterministicIv/ComputeSha256（地址见进度文档 §YAES）
# 参数（全部经 live 内存 + 真实 bundle 验证或代码实锤）：
#   key  = PBKDF2-HMAC-SHA1(password="gamemen2026", salt="c698rish2210", iter=10000, dkLen=32)
#          （AesKey live 实测吻合；代码硬编码 0x2710=10000，metadata 里 20000 未用）
#   格式 = [4B "YAES"][1B version=1][16B IV][AES-CBC 密文]，PKCS7（set_Padding(2)）
#   CDN 侧 bundle 用此格式；APK 内置 rawfile 不是 YAES（头是明文 UnityFS，爱加密另有数据块混淆）
import hashlib, struct, sys

PASSWORD = b"gamemen2026"
SALT = b"c698rish2210"
ITERATIONS = 10000
MAGIC = b"YAES"
VERSION = 1

def aes_key() -> bytes:
    return hashlib.pbkdf2_hmac("sha1", PASSWORD, SALT, ITERATIONS, 32)

def _pkcs7_pad(d: bytes) -> bytes:
    n = 16 - len(d) % 16
    return d + bytes([n]) * n

def _pkcs7_unpad(d: bytes) -> bytes:
    n = d[-1]
    if 1 <= n <= 16 and d[-n:] == bytes([n]) * n:
        return d[:-n]
    return d  # 无填充（长度恰整块）

def decrypt(data: bytes) -> bytes:
    from Crypto.Cipher import AES
    assert data[:4] == MAGIC, f"not YAES: {data[:8].hex()}"
    ver = data[4]
    assert ver == VERSION, f"version {ver} != {VERSION}"
    iv = data[5:21]
    return _pkcs7_unpad(AES.new(aes_key(), AES.MODE_CBC, iv).decrypt(data[21:]))

def encrypt(plain: bytes) -> bytes:
    from Crypto.Cipher import AES
    iv = _deterministic_iv_for_repack(plain)
    ct = AES.new(aes_key(), AES.MODE_CBC, iv).encrypt(_pkcs7_pad(plain))
    return MAGIC + bytes([VERSION]) + iv + ct

def _deterministic_iv_for_repack(plain: bytes) -> bytes:
    # 游戏加密侧 IV = SHA256(nameBytes ‖ SHA256(?) ‖ saltBytes)[:16]（CreateDeterministicIv）。
    # 解密不依赖 IV 来源（IV 内嵌在 payload），重封时任意 IV 均可被游戏解出；
    # 如需与官方 CDN 包逐字节一致，再逆 EncryptBytes 补此函数。
    return hashlib.sha256(b"wxsj-repack").digest()[:16]

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("usage: yae_crypto.py decrypt <in> [out] | encrypt <in> [out] | key")
        sys.exit(1)
    k = aes_key()
    print("AesKey:", k.hex())
    if sys.argv[1] == "key":
        sys.exit(0)
    data = open(sys.argv[2], "rb").read()
    out = decrypt(data) if sys.argv[1] == "decrypt" else encrypt(data)
    dst = sys.argv[3] if len(sys.argv) > 3 else sys.argv[2] + "." + sys.argv[1]
    open(dst, "wb").write(out)
    print("wrote", dst, len(out), "bytes")
