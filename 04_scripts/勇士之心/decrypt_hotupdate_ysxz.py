#!/usr/bin/env python3.11
"""勇士之心：解密运行时热更 JS（*ze* + XXTEA + zlib）。

用法：
  python3.11 decrypt_hotupdate_ysxz.py [IN_ROOT] [OUT_ROOT]
默认 IN_ROOT = 09_test/勇士之心/regrab_20260921，OUT_ROOT = 06_dumped/勇士之心/hotupdate_20260921
"""
from __future__ import annotations

import json
import struct
import sys
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
IN_ROOT = Path(sys.argv[1]) if len(sys.argv) > 1 else ROOT / "09_test/勇士之心/regrab_20260921"
OUT_ROOT = Path(sys.argv[2]) if len(sys.argv) > 2 else ROOT / "06_dumped/勇士之心/hotupdate_20260921"
KEY = b"237GaogniB!@#$1sdf546685"
MAGIC = b"*ze*"


def xxtea_decrypt_cocos(data: bytes, key: bytes) -> bytes | None:
    if len(data) % 4:
        return None
    n = len(data) // 4
    if n < 2:
        return None
    v = list(struct.unpack(f"<{n}I", data))
    key = key[:16].ljust(16, b"\0")
    k = list(struct.unpack("<4I", key))

    q = 6 + 52 // n
    total = (q * 0x9E3779B9) & 0xFFFFFFFF
    z = v[n - 1]
    y = v[0]

    while total:
        e = (total >> 2) & 3
        for p in range(n - 1, 0, -1):
            z = v[p - 1]
            mx = ((((z >> 5) ^ (y << 2)) & 0xFFFFFFFF) + (((y >> 3) ^ (z << 4)) & 0xFFFFFFFF)) & 0xFFFFFFFF
            mx = (mx ^ (((total ^ y) & 0xFFFFFFFF) + (k[(p & 3) ^ e] ^ z))) & 0xFFFFFFFF
            y = (v[p] - mx) & 0xFFFFFFFF
            v[p] = y

        z = v[n - 1]
        mx = ((((z >> 5) ^ (y << 2)) & 0xFFFFFFFF) + (((y >> 3) ^ (z << 4)) & 0xFFFFFFFF)) & 0xFFFFFFFF
        mx = (mx ^ (((total ^ y) & 0xFFFFFFFF) + (k[e] ^ z))) & 0xFFFFFFFF
        y = (v[0] - mx) & 0xFFFFFFFF
        v[0] = y
        total = (total - 0x9E3779B9) & 0xFFFFFFFF

    block_len = n * 4
    plain_len = v[-1]
    if not (block_len - 7 <= plain_len <= block_len - 4):
        return None
    return struct.pack(f"<{n}I", *v)[:plain_len]


def decrypt_file(src: Path, dst: Path) -> dict:
    raw = src.read_bytes()
    if not raw.startswith(MAGIC):
        raise ValueError(f"bad magic: {src}")
    payload = raw[5:]
    payload = payload[: len(payload) // 4 * 4]
    inner = xxtea_decrypt_cocos(payload, KEY)
    if inner is None:
        raise ValueError(f"xxtea failed: {src}")
    plain = zlib.decompress(inner)
    dst.parent.mkdir(parents=True, exist_ok=True)
    dst.write_bytes(plain)
    return {
        "src": str(src.relative_to(ROOT)),
        "dst": str(dst.relative_to(ROOT)),
        "encrypted_size": len(raw),
        "xxtea_size": len(inner),
        "plain_size": len(plain),
    }


def main() -> int:
    files = sorted(p for p in IN_ROOT.rglob("*.js") if p.read_bytes()[:4] == MAGIC)
    if not files:
        raise SystemExit(f"no encrypted js under {IN_ROOT}")
    records = []
    for src in files:
        dst = OUT_ROOT / src.relative_to(IN_ROOT)
        records.append(decrypt_file(src, dst))
    OUT_ROOT.mkdir(parents=True, exist_ok=True)
    summary = {
        "in_root": str(IN_ROOT),
        "out_root": str(OUT_ROOT),
        "key_full": KEY.decode(),
        "key_effective": KEY[:16].decode(),
        "count": len(records),
        "files": records,
    }
    (OUT_ROOT / "decrypt_summary.json").write_text(
        json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps({"count": len(records),
                      "sizes": {r["dst"].split("hotupdate_20260921/")[-1]: r["plain_size"] for r in records}},
                     ensure_ascii=False, indent=2)[:3000])
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
