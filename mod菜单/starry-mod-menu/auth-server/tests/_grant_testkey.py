"""本地自测专用：临时 RSA-2048 签名密钥（每次运行现生成，随临时目录丢弃）。

只用于离线回归，**不得**指向 ~/.starry-keys 或任何真实私钥；内容不打印、不入库。
"""
from __future__ import annotations

import os


def make_test_key(directory: str) -> str:
    """在 directory 下生成一把临时私钥文件（sg_rsa 的 key=hex 格式），返回路径。"""
    from cryptography.hazmat.primitives.asymmetric import rsa

    key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    nums = key.private_numbers()
    pub = nums.public_numbers
    p, q, d = nums.p, nums.q, nums.d
    path = os.path.join(directory, "grant_test_key.txt")
    lines = [
        "n=%x" % pub.n,
        "e=%x" % pub.e,
        "d=%x" % d,
        "p=%x" % p,
        "q=%x" % q,
        "dp=%x" % (d % (p - 1)),
        "dq=%x" % (d % (q - 1)),
        "qinv=%x" % pow(q, -1, p),
    ]
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
    try:
        os.chmod(path, 0o600)
    except OSError:
        pass
    return path
