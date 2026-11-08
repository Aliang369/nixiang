#!/usr/bin/env python3
"""dex_probe_strings.py — 在运行时取到的 DEX/VDEX 里只查「环境登录所需的 6 个精确标识串」。

范围铁律（本文件结构性保证）：
  · 只查下面 NEEDLES 里写死的 6 条；不做类/方法枚举，不列 metadata，不查 ABI，不查 RVA。
  · 不做 Cocos 玩法 / JS 解密分析。
  · 不改任何输入文件；只读 + 打印。

命中语义：命中的是**原始字节**（DEX string_data 区里的 MUTF-8 原文）。
  · 『未见』≠『类不存在』—— 只在本次取到的文件范围内成立（见报告「取证范围」段）。
"""
from __future__ import annotations

import argparse
import hashlib
import sys
from pathlib import Path

# 只有这 6 条（任务书原文）。禁止扩充。
NEEDLES = [
    "GatekeeperRepository",
    "isGatekeeperLoadedSuccessfully",
    "IntentHelper",
    "LoginActivityImpl",
    "WebHandler",
    "dev.mars.secure",
]

MAGICS = {
    b"dex\n": "DEX",
    b"vdex": "VDEX",
    b"\x7fELF": "ELF",
    b"PK\x03\x04": "ZIP",
}


def magic_of(data: bytes) -> str:
    for m, name in MAGICS.items():
        if data.startswith(m):
            return name
    if data[:4] == b"dex\n":
        return "DEX"
    return "UNKNOWN"


def sha256_of(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def probe(path: Path, limit: int) -> int:
    data = path.read_bytes()
    kind = magic_of(data)
    print(f"\n== {path.name}")
    print(f"   size={len(data)} sha256={sha256_of(path)[:16]}… magic={kind}")
    if kind in ("VDEX", "ZIP"):
        # 容器内嵌 DEX 计数（只数魔数，不解析、不枚举）
        n_dex = data.count(b"dex\n035") + data.count(b"dex\n036") + data.count(b"dex\n037")
        print(f"   embedded_dex_magic={n_dex}")
    hits = 0
    for s in NEEDLES:
        b8 = s.encode()
        b16 = s.encode("utf-16le")
        c8 = data.count(b8)
        c16 = data.count(b16)
        if c8 or c16:
            offs8 = []
            i = data.find(b8)
            while i >= 0 and len(offs8) < 3:
                offs8.append(hex(i))
                i = data.find(b8, i + 1)
            offs16 = []
            j = data.find(b16)
            while j >= 0 and len(offs16) < 3:
                offs16.append(hex(j))
                j = data.find(b16, j + 1)
            print(f"   HIT  {s}  utf8={c8}@{','.join(offs8) or '-'}  utf16le={c16}@{','.join(offs16) or '-'}")
            hits += 1
        else:
            print(f"   miss {s}")
    print(f"   -> needles_hit={hits}/{len(NEEDLES)}")
    return hits


def main() -> int:
    ap = argparse.ArgumentParser(add_help=True)
    ap.add_argument("files", nargs="+", help="取到的 DEX/VDEX/ELF 文件")
    ap.add_argument("--limit", type=int, default=0, help="保留参数（当前不做字节扫描上限）")
    a = ap.parse_args()
    total = 0
    for f in a.files:
        p = Path(f)
        if not p.is_file():
            print(f"✗ 缺文件 {p}", file=sys.stderr)
            return 2
        total += probe(p, a.limit)
    print(f"\nTOTAL needles_hit={total}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
