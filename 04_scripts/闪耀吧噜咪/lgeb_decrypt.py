#!/usr/bin/env python3
"""Decrypt "LGEB" bundles used by 闪耀吧噜咪 1.5.3 (Addressables EncryptedBundleProvider).

Format (confirmed against the live process on 2026-09-17):

    offset 0x00 : b"LGEB"
    offset 0x04 : uint32 little endian "version"
    offset 0x08 : AES-128-CBC ciphertext, PKCS#7 padded

Key material:

    salt    = BundleCipherFormat.BuildSalt()          # deterministic, see build_salt()
    name    = bundle key (the content-hash part of the cached file name,
              e.g. "2517e568e31ff963bd6e37787662622e")
    sha     = SHA256(salt + UTF8(f"{name}:{version}"))
    key, iv = sha[0:16], sha[16:32]

Only reads the input file and writes the decrypted bundle.
"""
from __future__ import annotations

import argparse
import hashlib
from pathlib import Path

from Crypto.Cipher import AES

MAGIC = b"LGEB"
MASK32 = 0xFFFFFFFF


def build_salt(length: int = 16) -> bytes:
    """Reproduce BundleCipherFormat.BuildSalt() (arm64 at RVA 0x1df7198)."""
    acc = 0x9D3FA658
    prime = 0x01000193
    step = 0x85EBCA6B
    xor = 0
    shift = 0
    out = bytearray()
    for _ in range(length):
        acc = (acc * prime) & MASK32
        acc ^= xor
        out.append((acc >> (shift & 0x18)) & 0xFF)
        shift = (shift + 8) & MASK32
        xor = (xor + step) & MASK32
    return bytes(out)


def derive_key_iv(name: str, version: int) -> tuple[bytes, bytes]:
    salt = build_salt()
    password = f"{name}:{version}".encode()
    digest = hashlib.sha256(salt + password).digest()
    return digest[0:16], digest[16:32]


def decrypt(data: bytes, name: str) -> tuple[bytes, int]:
    if data[:4] != MAGIC:
        raise ValueError("not an LGEB bundle")
    version = int.from_bytes(data[4:8], "little")
    key, iv = derive_key_iv(name, version)
    plain = AES.new(key, AES.MODE_CBC, iv).decrypt(data[8:])
    if not plain.startswith(b"UnityFS"):
        raise ValueError("decrypted payload is not a UnityFS bundle")
    return plain, version


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("input", type=Path)
    ap.add_argument("bundle_name", help="content hash part, e.g. 2517e568e31ff963bd6e37787662622e")
    ap.add_argument("output", type=Path)
    args = ap.parse_args()

    plain, version = decrypt(args.input.read_bytes(), args.bundle_name)
    args.output.write_bytes(plain)
    print(f"version={version} size={len(plain)} -> {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
