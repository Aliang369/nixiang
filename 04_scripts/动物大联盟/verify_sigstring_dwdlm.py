#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dwdlm 1.0.25 — 用壳 .bss 拿到的 KEY/IV 反推「签名串」，离线判定其真实格式。

原理（D 报告实测公式）：
    KEY[i] = SHA256(签名串 + 包名 + "K")[i] ^ const32[i]
    IV[i]  = SHA256(签名串 + 包名 + "I")[i] ^ const16[i]
所以 SHA256(签名串 + 包名 + "K") = KEY ^ const32 是已知的 32 字节目标值。
把 APK v2 签名块里的候选串（DER / hex / base64 / 整块 ...）逐个拼接求哈希，
命中即为「签名串」的真实格式。

只读：只读本地文件。
"""
import base64
import hashlib
import json
import struct
import sys

PKG = "com.fightingstars.games.animal"
CONST32 = bytes.fromhex("96d17f230113b18b48ae3399c791efaac0e273d6df8dcc3ee61dd50d92fe1f03")
CONST16 = bytes.fromhex("87aede97d12b8292437ee20312cfbd81")
KEY_HEX = "3deec1195ff469b2a17ed6272d2733b24fcc377b790b24a3b34f5e79dcfc580b"
IV_HEX = "7979900dd0d65c9576ab0f2a16f2da69"


def xor(a, b):
    return bytes(x ^ y for x, y in zip(a, b))


def lp_items(buf, off=0):
    """解析 length-prefixed (uint32 LE) 序列，返回 (list_of_bytes, 新偏移)"""
    n = struct.unpack_from("<I", buf, off)[0]
    off += 4
    end = off + n
    out = []
    while off < end:
        ln = struct.unpack_from("<I", buf, off)[0]
        off += 4
        out.append(buf[off:off + ln])
        off += ln
    return out, end


def find_signing_block(apk):
    data = open(apk, "rb").read()
    i = data.rfind(b"PK\x05\x06")
    if i < 0:
        raise SystemExit("找不到 EOCD")
    cd_off = struct.unpack_from("<I", data, i + 16)[0]
    magic = data[cd_off - 16:cd_off]
    if magic != b"APK Sig Block 42":
        raise SystemExit("CD 前没有 APK Signing Block（可能只有 v1 签名）")
    size = struct.unpack_from("<Q", data, cd_off - 24)[0]
    start = cd_off - size - 8
    block = data[start:cd_off]
    pairs = {}
    off = 8
    while off < len(block) - 24:
        ln = struct.unpack_from("<Q", block, off)[0]
        pid = struct.unpack_from("<I", block, off + 8)[0]
        val = block[off + 12:off + 8 + ln]
        pairs[pid] = val
        off += 8 + ln
    return data, block, pairs


def parse_v2(block):
    """返回 (第一张证书 DER, 全部证书列表, 签名者数量)"""
    signers, _ = lp_items(block, 0)
    certs_all = []
    for sg in signers:
        signed, _ = lp_items(sg, 0)          # signed data
        digests, _ = lp_items(signed[0], 0)  # digests
        certs, _ = lp_items(signed[1], 0)    # certificates
        certs_all.extend(certs)
    return (certs_all[0] if certs_all else None), certs_all, len(signers)


def main():
    apk = sys.argv[1] if len(sys.argv) > 1 else "installed_base.apk"
    key = bytes.fromhex(KEY_HEX)
    iv = bytes.fromhex(IV_HEX)
    hk = xor(key, CONST32)   # 目标 = SHA256(签名串 + pkg + "K")
    hi = xor(iv, CONST16)

    data, block, pairs = find_signing_block(apk)
    print("APK          : %s (%d bytes)" % (apk, len(data)))
    print("签名块 ids   : %s" % ", ".join("0x%08x(%dB)" % (k, len(v)) for k, v in sorted(pairs.items())))
    v2 = pairs.get(0x7109871A)
    if v2 is None:
        raise SystemExit("无 v2 签名块 (0x7109871a)")
    cert, certs, nsigners = parse_v2(v2)
    print("signers      : %d, 证书 %d 张, 首张 DER %d 字节" % (nsigners, len(certs), len(cert)))
    print("首证书 SHA256: %s" % hashlib.sha256(cert).hexdigest())
    print("目标 SHA256(sig+pkg+K) = %s" % hk.hex())
    print("目标 SHA256(sig+pkg+I) = %s" % hi.hex())
    print()

    # ---- 候选串 ----
    cands = {}
    cands["证书DER原字节"] = cert
    cands["证书DER的hex(lower)"] = cert.hex().encode()
    cands["证书DER的hex(UPPER)"] = cert.hex().upper().encode()
    cands["证书DER的base64"] = base64.b64encode(cert)
    cands["v2整块原字节"] = v2
    cands["v2整块的hex"] = v2.hex().encode()
    cands["签名块(SigningBlock)原字节"] = block
    cands["证书DER的SHA256(hex)"] = hashlib.sha256(cert).hexdigest().encode()
    cands["证书DER的SHA256(原字节)"] = hashlib.sha256(cert).digest()
    for idx, c in enumerate(certs):
        if idx:
            cands["证书%d DER" % idx] = c
            cands["证书%d DER hex" % idx] = c.hex().encode()
            cands["证书%d DER base64" % idx] = base64.b64encode(c)

    hits = []
    rows = []
    for name, val in cands.items():
        for suf in (b"K", b"I", b"k", b"i"):
            for pkg_mode in ("bin", "txt"):
                pkg_b = PKG.encode() if pkg_mode == "bin" else PKG.encode()
                msg = val + pkg_b + suf
                d = hashlib.sha256(msg).digest()
                tag = "%s + pkg + %r" % (name, suf.decode())
                ok_k = (d == hk)
                ok_i = (d == hi)
                rows.append((tag, len(val), d.hex()[:16], ok_k, ok_i))
                if ok_k or ok_i:
                    hits.append((tag, len(val), ok_k, ok_i, val))
    print("%-42s %8s %-18s %s %s" % ("候选", "长度", "SHA256前16", "=K", "=I"))
    for tag, ln, d16, ok_k, ok_i in rows:
        if ok_k or ok_i:
            print(">>> HIT  %-40s %8d %-18s %s %s" % (tag, ln, d16, ok_k, ok_i))
    if hits:
        print("\n命中 %d 条" % len(hits))
    else:
        print("(无命中，前 12 行示例)")
        for tag, ln, d16, ok_k, ok_i in rows[:12]:
            print("    %-42s %8d %-18s %s %s" % (tag, ln, d16, ok_k, ok_i))

    out = {"apk": apk, "apk_sha256": hashlib.sha256(data).hexdigest(), "pkg": PKG,
           "key": KEY_HEX, "iv": IV_HEX, "target_K": hk.hex(), "target_I": hi.hex(),
           "signers": nsigners, "certs": len(certs), "cert0_len": len(cert),
           "cert0_sha256": hashlib.sha256(cert).hexdigest(),
           "hits": [{"cand": t, "len": l, "K": k, "I": i} for t, l, k, i, _ in hits],
           "rows": [{"cand": t, "len": l, "sha16": d, "K": k, "I": i} for t, l, d, k, i in rows]}
    with open("sigstring_verify.json", "w", encoding="utf-8") as f:
        json.dump(out, f, ensure_ascii=False, indent=2)
    print("\n写出 sigstring_verify.json")


if __name__ == "__main__":
    main()
