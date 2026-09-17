#!/usr/bin/env python3
"""Carve intact managed PE images from a live-process heap dump.

The source dump is a raw memory range. Hot-update assemblies are found by
locating BSJB metadata signatures and matching the nearest earlier PE header.
This script only reads the dump and writes derived analysis artifacts.
"""
from __future__ import annotations

import argparse
import hashlib
import struct
from pathlib import Path

import pefile


def find_pe_start(data: bytes, bsjb: int, max_back: int) -> int | None:
    start = max(0, bsjb - max_back)
    pos = data.rfind(b"MZ", start, bsjb + 1)
    while pos >= 0:
        if pos + 0x40 <= len(data):
            lfanew = struct.unpack_from("<I", data, pos + 0x3C)[0]
            if 0 < lfanew <= 0x10000 and pos + lfanew + 4 <= len(data):
                if data[pos + lfanew:pos + lfanew + 4] == b"PE\0\0":
                    return pos
        pos = data.rfind(b"MZ", start, pos)
    return None


def pe_image_size(pe: pefile.PE) -> int:
    end = pe.OPTIONAL_HEADER.SizeOfHeaders
    for section in pe.sections:
        end = max(end, section.PointerToRawData + section.SizeOfRawData)
    return end


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("dump", type=Path)
    parser.add_argument("outdir", type=Path)
    parser.add_argument("--max-back", type=int, default=4 * 1024 * 1024)
    args = parser.parse_args()

    data = args.dump.read_bytes()
    args.outdir.mkdir(parents=True, exist_ok=True)

    seen: set[int] = set()
    index = []
    bsjb = data.find(b"BSJB")
    while bsjb >= 0:
        pe_start = find_pe_start(data, bsjb, args.max_back)
        if pe_start is not None and pe_start not in seen:
            seen.add(pe_start)
            available = data[pe_start:]
            try:
                pe = pefile.PE(data=available, fast_load=True)
                size = pe_image_size(pe)
                if size <= 0 or size > len(available):
                    raise ValueError(f"invalid image size {size:#x}")
                image = available[:size]
                # Parse once more with the exact carved range.
                pefile.PE(data=image, fast_load=True)
            except Exception as exc:
                print(f"skip bsjb={bsjb:#x} pe={pe_start:#x}: {exc}")
            else:
                digest = hashlib.sha256(image).hexdigest()
                name = f"dll_{pe_start:08x}_{size:08x}_{digest[:12]}.dll"
                path = args.outdir / name
                path.write_bytes(image)
                index.append((pe_start, bsjb, size, digest, path.name))
                print(f"{path.name} size={size} bsjb={bsjb:#x}")
        bsjb = data.find(b"BSJB", bsjb + 1)

    manifest = args.outdir / "carved_manifest.tsv"
    with manifest.open("w", encoding="utf-8") as fh:
        fh.write("pe_offset\tbsjb_offset\tsize\tsha256\tfile\n")
        for row in index:
            fh.write(f"{row[0]:#x}\t{row[1]:#x}\t{row[2]}\t{row[3]}\t{row[4]}\n")
    print(f"carved={len(index)} manifest={manifest}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
