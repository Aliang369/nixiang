#!/usr/bin/env python3
"""Encrypt the first 0x100 bytes of a decrypted Bszz System.Network DLL."""
from pathlib import Path
import hashlib
import sys

KEY = bytes((0x04, 0x10, 0x05, 0x1B, 0x0A, 0x01))


def main() -> int:
    if len(sys.argv) != 3:
        print("usage: pack_network_dll.py <plain.dll> <output.dll.bytes>", file=sys.stderr)
        return 2
    src = Path(sys.argv[1])
    dst = Path(sys.argv[2])
    data = bytearray(src.read_bytes())
    for i in range(min(0x100, len(data))):
        data[i] ^= KEY[i % len(KEY)]
    dst.write_bytes(data)
    print(f"output={dst}")
    print(f"size={len(data)}")
    print(f"md5={hashlib.md5(data).hexdigest()}")
    print(f"sha256={hashlib.sha256(data).hexdigest()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
