#!/usr/bin/env python3
"""Apply the reviewed ZXR battle patch to decrypted Cocos JS and re-encrypt it."""
from __future__ import annotations

import gzip
import hashlib
import json
import sys
import zlib
from pathlib import Path

import xxtea


def pack_native_xxtea_payload(data: bytes) -> bytes:
    """Match libcocos2djs XXTEA: data, zero padding, then a length word."""
    if len(data) > 0xFFFFFFFF:
        raise ValueError("gzip payload too large for Cocos XXTEA length word")
    return data + b"\x00" * (-len(data) % 4) + len(data).to_bytes(4, "little")


def unpack_native_xxtea_payload(packed: bytes) -> bytes:
    if len(packed) < 4 or len(packed) % 4:
        raise ValueError(f"invalid Cocos XXTEA payload size: {len(packed)}")
    length = int.from_bytes(packed[-4:], "little")
    if length > len(packed) - 4:
        raise ValueError(f"invalid Cocos XXTEA length word: {length}/{len(packed)}")
    return packed[:length]


def apply_patch(source: str, spec: dict) -> tuple[str, list[dict]]:
    marker = spec["marker"]
    if marker in source:
        raise ValueError(f"source already contains marker {marker}")

    patched = source + "\n" + spec["prefix"]
    records: list[dict] = []
    for rep in spec["replacements"]:
        name = rep["name"]
        anchor = rep["anchor"]
        replacement = anchor + rep["insert"]
        count = patched.count(anchor)
        if count != 1:
            raise ValueError(f"{name}: expected exactly one anchor, found {count}: {anchor!r}")
        patched = patched.replace(anchor, replacement, 1)
        records.append({"name": name, "anchor": anchor, "count": count})
    return patched, records


def encrypt_jsc(js: bytes, key: bytes) -> bytes:
    packed = gzip.compress(js, compresslevel=9, mtime=0)
    packed = pack_native_xxtea_payload(packed)
    encrypted = xxtea.encrypt(packed, key, padding=False)
    if not encrypted:
        raise ValueError("xxtea encrypt returned empty")
    return encrypted


def decrypt_jsc(encrypted: bytes, key: bytes) -> bytes:
    packed = xxtea.decrypt(encrypted, key, padding=False)
    packed = unpack_native_xxtea_payload(packed)
    dec = zlib.decompressobj(16 + zlib.MAX_WBITS)
    js = dec.decompress(packed) + dec.flush()
    if not dec.eof:
        raise ValueError("gzip stream did not terminate")
    return js


def main() -> int:
    if len(sys.argv) != 5:
        print(
            "usage: patch_jsc_zxr.py <decrypted.js> <patch_spec.json> <out.js> <out.jsc>",
            file=sys.stderr,
        )
        return 2
    src_path = Path(sys.argv[1])
    spec_path = Path(sys.argv[2])
    out_js_path = Path(sys.argv[3])
    out_jsc_path = Path(sys.argv[4])
    key = b"c25d728a-a9d9-4c"

    source = src_path.read_text(encoding="utf-8")
    spec = json.loads(spec_path.read_text(encoding="utf-8"))
    patched, records = apply_patch(source, spec)

    out_js_path.parent.mkdir(parents=True, exist_ok=True)
    out_jsc_path.parent.mkdir(parents=True, exist_ok=True)
    out_js_path.write_text(patched, encoding="utf-8", newline="")
    encrypted = encrypt_jsc(patched.encode("utf-8"), key)
    plain = decrypt_jsc(encrypted, key)
    if plain != patched.encode("utf-8"):
        raise ValueError("encrypt/decrypt round-trip mismatch")
    out_jsc_path.write_bytes(encrypted)

    summary = {
        "source": str(src_path),
        "source_sha256": hashlib.sha256(source.encode("utf-8")).hexdigest(),
        "patched_js": str(out_js_path),
        "patched_js_sha256": hashlib.sha256(patched.encode("utf-8")).hexdigest(),
        "patched_jsc": str(out_jsc_path),
        "patched_jsc_sha256": hashlib.sha256(encrypted).hexdigest(),
        "prefix_size": len(spec["prefix"].encode("utf-8")),
        "patched_size": len(patched.encode("utf-8")),
        "jsc_size": len(encrypted),
        "replacements": records,
    }
    summary_path = out_jsc_path.with_suffix(".json")
    summary_path.write_text(
        json.dumps(summary, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(
        f"patched={summary['patched_size']} jsc={summary['jsc_size']} "
        f"sha256={summary['patched_jsc_sha256']}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
