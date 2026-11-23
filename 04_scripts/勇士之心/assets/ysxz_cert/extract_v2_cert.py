#!/usr/bin/env python3
"""勇士之心 1.0.6 官方签名证书导出（APK Signing Block v2/v3 -> X.509 DER）。

为什么需要这个脚本：本包是**纯 v2 签名**（包内没有 META-INF/*.RSA），
unzip 取 META-INF 里的 .RSA 拿不到证书；apksigner --print-certs 只给指纹不给 DER。
所以直接解析 APK Signing Block 里 v2（0x7109871a）或 v3（0xf05368c0）pair 的证书序列。

用法：
  python3 extract_v2_cert.py --apk 02_apks/勇士之心_1.0.6_official.apk \
      --out-dir 04_scripts/勇士之心/assets/ysxz_cert \
      --expect-sha256 de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a

产物（写进 --out-dir）：
  ysxz_official_cert.der           X.509 DER（877 B）
  ysxz_official_cert.b64           单行 base64
  ysxz_official_cert.b64.wrapped   每 64 字符换行（smali 字面量形态）

退出码：0 = 抽出且指纹相符；2 = 指纹不符；1 = 结构错误。
"""
import argparse
import base64
import hashlib
import struct
import sys
from pathlib import Path

MAGIC = b"APK Sig Block 42"
ID_V2 = 0x7109871A
ID_V3 = 0xF05368C0


def lp(buf, off=0):
    """读一个 uint32 长度前缀的切片，返回 (value, next_off)。"""
    if len(buf) < off + 4:
        raise SystemExit("ERROR: 长度前缀越界 (off=%d len=%d)" % (off, len(buf)))
    n = struct.unpack("<I", buf[off:off + 4])[0]
    return buf[off + 4:off + 4 + n], off + 4 + n


def read_pairs(data):
    i = data.rfind(MAGIC)
    if i <= 0:
        raise SystemExit("ERROR: 未找到 APK Signing Block（不是 v2+ 签名包）")
    size = struct.unpack("<Q", data[i - 8:i])[0]
    start = i + 16 - size - 8
    block = data[start:i + 16]
    pos, end, pairs = 8, len(block) - 24, {}
    while pos < end:
        plen = struct.unpack("<Q", block[pos:pos + 8])[0]
        pid = struct.unpack("<I", block[pos + 8:pos + 12])[0]
        pairs[pid] = block[pos + 12:pos + 8 + plen]
        pos += 8 + plen
    return pairs


def first_cert(pair):
    """v2/v3: signers -> signer -> signed_data -> (digests, certificates) -> 第一张 DER。"""
    signers, _ = lp(pair)
    signer, _ = lp(signers)
    signed_data, off = lp(signer)
    _, off = lp(signed_data, off)          # digests（跳过）
    certs_seq, _ = lp(signed_data, off)
    der, _ = lp(certs_seq)
    return der


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--apk", required=True)
    ap.add_argument("--out-dir", required=True)
    ap.add_argument("--expect-sha256", default="")
    a = ap.parse_args()

    data = Path(a.apk).read_bytes()
    pairs = read_pairs(data)
    use_v2 = pairs.get(ID_V2)
    pair = use_v2 if use_v2 else pairs.get(ID_V3)
    if not pair:
        raise SystemExit("ERROR: 无 v2/v3 签名 pair，实际有 %s" % [hex(k) for k in pairs])
    der = first_cert(pair)
    sha256 = hashlib.sha256(der).hexdigest()
    md5 = hashlib.md5(der).hexdigest()
    sha1 = hashlib.sha1(der).hexdigest()

    out = Path(a.out_dir)
    out.mkdir(parents=True, exist_ok=True)
    (out / "ysxz_official_cert.der").write_bytes(der)
    b64 = base64.b64encode(der).decode()
    (out / "ysxz_official_cert.b64").write_text(b64 + "\n", encoding="utf-8")
    wrapped = "\n".join(b64[j:j + 64] for j in range(0, len(b64), 64)) + "\n"
    (out / "ysxz_official_cert.b64.wrapped").write_text(wrapped, encoding="utf-8")

    print("  chain       = %s" % ("v2" if use_v2 else "v3"))
    print("  cert DER    = %d B" % len(der))
    print("  SHA-256     = %s" % sha256)
    print("  SHA-1       = %s" % sha1)
    print("  MD5         = %s" % md5)
    if a.expect_sha256 and sha256 != a.expect_sha256:
        print("ERROR: 指纹不符，期望 %s" % a.expect_sha256, file=sys.stderr)
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
