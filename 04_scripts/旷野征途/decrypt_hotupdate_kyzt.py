#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""旷野征途 (kyzt) — HotUpdate.dll 封装解包（RSA+AES）

用途
    3.2 起 HotUpdate.dll 不再是裸 DLL / 不再是旧式单层封装，而改为「RSA + AES」封装：
        偏移 0        1 B   版本字节
        偏移 1        16 B  AES-CBC IV
        偏移 17       N B   密文（AES-256-CBC / PKCS#7）
        末尾          384 B RSA-3072 签名（本脚本不验证，只跳过）
    密钥不是外挂文件，而是 IL2CPP metadata 里的字面量（3.2 = 字面量 #6155）。
    本脚本解密出明文 DLL，供 ApplyHp 反汇编 / 战斗合同核对使用。

3.2 实测基准（2026-11-24 真机 96f0d883 轮）
    AES-256 key : 3144ee1cf6b89a8267831e6098ab5cc73fba86abe698aa12b78aa98f0b0300bf
    明文         : 3,583,488 B，sha256 812e6563…d551b0
    ApplyHp      : argc=4，body 1594 字节，槽位 this/atker/atkSeatNum/dmgInfo/OnDie = +0/+8/+16/+24/+32

前置条件
    pip 包 cryptography（cryptography.hazmat）—— 本机实测 50.0.2
    加密包来源：解包后的 assets 里 HotUpdate 容器文件（3.2 实测由 13_scratch/kyzt32_native/hotupdate/ 下取得）

来源
    3.2 首版在 13_scratch/kyzt32_native/hd_decrypt.py 内跑通（硬编码绝对路径），本条提升入库并参数化。
    密钥怎么来的（一次性穷举探针，故意**不入库**）：13_scratch/kyzt32_native/crack_key.py +
    hd_key_brute*.py —— 从 metadata 候选字面量穷举 AES key，属一次性探测。

用法
    python3 04_scripts/旷野征途/decrypt_hotupdate_kyzt.py --pkg <加密包> --out <明文.dll>
    python3 04_scripts/旷野征途/decrypt_hotupdate_kyzt.py --pkg <加密包> --header-only
    python3 04_scripts/旷野征途/decrypt_hotupdate_kyzt.py --pkg a.bin --out b.dll --key <hex32>
"""

import argparse
import hashlib
import struct
import sys
from pathlib import Path

KEY_32 = "3144ee1cf6b89a8267831e6098ab5cc73fba86abe698aa12b78aa98f0b0300bf"
SIZE_32 = 3583488
SHA256_32_PREFIX = "812e6563"
SHA256_32_SUFFIX = "d551b0"
SIG_LEN = 384  # RSA-3072


def parse_pkg(blob: bytes):
    """拆封装：返回 (version, iv, ciphertext, signature)。"""
    if len(blob) < 1 + 16 + 16 + SIG_LEN:
        raise SystemExit("ERROR: 封装长度异常 %d B" % len(blob))
    ver = blob[0]
    iv = blob[1:17]
    ct = blob[17:len(blob) - SIG_LEN]
    sig = blob[len(blob) - SIG_LEN:]
    if len(ct) % 16:
        raise SystemExit("ERROR: 密文长度 %d 不是 16 的倍数（封装布局与本脚本假设不符）" % len(ct))
    return ver, iv, ct, sig


def pe_report(pt: bytes) -> None:
    if pt[:2] != b"MZ":
        print("  PE: 明文不是 MZ 头（前 16 B: %s）" % pt[:16].hex())
        return
    e_lfanew = struct.unpack_from("<i", pt, 0x3C)[0]
    sig = pt[e_lfanew:e_lfanew + 4]
    machine, nsec = struct.unpack_from("<HH", pt, e_lfanew + 4)
    print("  PE: e_lfanew=0x%x sig=%r machine=0x%x sections=%d" % (e_lfanew, sig, machine, nsec))


def main(argv=None):
    ap = argparse.ArgumentParser(
        description="旷野征途 HotUpdate.dll 封装解包（RSA+AES，3.2 起）",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    ap.add_argument("--pkg", required=True, help="加密的 HotUpdate 容器文件")
    ap.add_argument("--out", help="明文 DLL 输出路径（省略则只在内存里核对指纹）")
    ap.add_argument("--key", default=KEY_32, help="AES-256 密钥 hex（默认 3.2 实测值）")
    ap.add_argument("--expect-size", type=int, default=SIZE_32,
                    help="期望明文字节数（默认 3.2 = %d，传 0 关闭）" % SIZE_32)
    ap.add_argument("--expect-sha256", default="",
                    help="期望明文 sha256 全值（可选硬门禁）")
    ap.add_argument("--header-only", action="store_true", help="只打印封装头部字段，不解密")
    args = ap.parse_args(argv)

    try:
        from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
    except Exception as exc:  # noqa: BLE001
        raise SystemExit("ERROR: 需要 pip 包 cryptography：%s" % exc)

    pkg = Path(args.pkg)
    if not pkg.is_file():
        raise SystemExit("ERROR: 找不到加密包 %s" % pkg)
    blob = pkg.read_bytes()
    ver, iv, ct, sig = parse_pkg(blob)

    print("封装: %s" % pkg)
    print("  总长 %d B / 版本字节 %d / IV %s / 密文 %d B / 签名 %d B %s"
          % (len(blob), ver, iv.hex(), len(ct), len(sig), "（RSA-3072，本脚本不验签）"))
    if args.header_only:
        return 0

    key = bytes.fromhex(args.key)
    if len(key) not in (16, 24, 32):
        raise SystemExit("ERROR: --key 长度必须是 16/24/32 B（32 B = AES-256）")

    dec = Cipher(algorithms.AES(key), modes.CBC(iv)).decryptor()
    raw = dec.update(ct) + dec.finalize()
    pad = raw[-1]
    if pad < 1 or pad > 16 or raw[-pad:] != bytes([pad]) * pad:
        raise SystemExit("ERROR: PKCS#7 去填充失败（key 或布局不对；末字节 %d）" % pad)
    pt = raw[:-pad]

    sha = hashlib.sha256(pt).hexdigest()
    print("  明文 %d B  sha256 %s" % (len(pt), sha))
    pe_report(pt)

    ok = True
    if args.expect_size and len(pt) != args.expect_size:
        print("  [FAIL] 明文长度 %d != 期望 %d" % (len(pt), args.expect_size))
        ok = False
    if args.expect_sha256:
        if sha != args.expect_sha256.lower():
            print("  [FAIL] sha256 不符，期望 %s" % args.expect_sha256)
            ok = False
        else:
            print("  [OK] sha256 与期望一致")
    elif args.expect_size == SIZE_32:
        # 3.2 基准只记录了首尾片段，做软提示不做硬门禁
        pref = "命中" if sha.startswith(SHA256_32_PREFIX) and sha.endswith(SHA256_32_SUFFIX) else "不符（可能是别的版本/别次构建）"
        print("  [info] 对照 3.2 基准 %s…%s：%s" % (SHA256_32_PREFIX, SHA256_32_SUFFIX, pref))

    if args.out:
        out = Path(args.out)
        out.parent.mkdir(parents=True, exist_ok=True)
        out.write_bytes(pt)
        print("  已写出 %s" % out)
    else:
        print("  （未给 --out，明文只做校验未落盘）")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
