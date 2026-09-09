#!/usr/bin/env python3.11
"""山河旅探 1.4.38 — 把 ILRuntime HotFix_Project.dll 的购买/关卡判定改成恒真。

不 hook IL 解释器。输入官方 UnityFS bundle，输出 patched bundle + 更新后的
YooAsset PackageManifest（md5 / crc32 / size / manifest.hash）。
"""
from __future__ import annotations

import hashlib
import struct
import sys
import zlib
from pathlib import Path

import UnityPy
import dnfile


JOBS = [
    ("DefaultPurchasedManager", "IsChapterPurchased", "bool_true"),
    ("DefaultPurchasedManager", "IsSkinPurchased", "tuple_tt"),
    ("DefaultPurchasedManager", "IsActivityLimitedSkin", "bool_false"),
    ("SaveManager", "CheckChapterStageIsUnlocked", "bool_true"),
    ("AchievementManager", "GetAchievementUnLock", "bool_true"),
]
def _find_tuple_newobj(buf: bytearray, body: int) -> bytes:
    """IsSkinPurchased 里出现次数最多的 `newobj`（MemberRef）即 ValueTuple<bool,bool>::.ctor。"""
    blob = bytes(buf[body : body + 700])
    counts: dict[bytes, int] = {}
    for i in range(len(blob) - 4):
        if blob[i] == 0x73 and blob[i + 4] == 0x0A:
            tok = blob[i : i + 5]
            counts[tok] = counts.get(tok, 0) + 1
    if not counts:
        raise SystemExit("no newobj token in IsSkinPurchased")
    return max(counts, key=counts.get)


def _getstr(pe, idx) -> str:
    item = pe.net.strings.get(idx)
    if item is None:
        return ""
    v = getattr(item, "value", item)
    return v.decode() if isinstance(v, bytes) else str(v)


def _rva_to_off(pe, rva: int):
    for sec in pe.sections:
        va = sec.VirtualAddress
        sz = max(sec.Misc_VirtualSize, sec.SizeOfRawData)
        if va <= rva < va + sz:
            return sec.PointerToRawData + (rva - va)
    return None


def _body_off(buf: bytearray, method_off: int) -> int:
    hdr = buf[method_off]
    if (hdr & 3) == 2:
        return method_off + 1
    flags = struct.unpack_from("<H", buf, method_off)[0]
    return method_off + ((flags >> 12) & 0xF) * 4


def patch_dll_bytes(raw: bytes) -> bytes:
    tmp = Path("/tmp/shlt_hotfix_in.dll")
    tmp.write_bytes(raw)
    pe = dnfile.dnPE(str(tmp))
    buf = bytearray(raw)
    done = set()
    for td in pe.net.mdtables.TypeDef.rows:
        tname = _getstr(pe, td.struct.TypeName_StringIndex)
        for idx in td.MethodList or []:
            md = idx.row
            name = _getstr(pe, md.struct.Name_StringIndex)
            for tn, mn, kind in JOBS:
                if tname != tn or name != mn:
                    continue
                off = _rva_to_off(pe, md.Rva)
                if off is None:
                    raise SystemExit(f"no body {tn}.{mn}")
                body = _body_off(buf, off)
                if kind == "bool_true":
                    patch = bytes([0x17, 0x2A])
                elif kind == "bool_false":
                    patch = bytes([0x16, 0x2A])
                elif kind == "tuple_tt":
                    newobj = _find_tuple_newobj(buf, body)
                    patch = bytes([0x17, 0x17]) + newobj + bytes([0x2A])
                    print(f"tuple newobj {newobj.hex()}")
                else:
                    raise SystemExit(kind)
                buf[body : body + len(patch)] = patch
                done.add((tn, mn))
    missing = {(a, b) for a, b, _ in JOBS} - done
    if missing:
        raise SystemExit(f"missing methods: {missing}")
    return bytes(buf)


def extract_dll_from_bundle(bundle: bytes) -> tuple[object, object, str]:
    env = UnityPy.load(bundle)
    for obj in env.objects:
        if obj.type.name == "TextAsset":
            ta = obj.read()
            return env, ta, ta.m_Script
    raise SystemExit("TextAsset missing in bundle")


def rewrite_bundle(old_bundle: bytes, patched_dll: bytes) -> bytes:
    env, ta, _ = extract_dll_from_bundle(old_bundle)
    ta.m_Script = patched_dll.decode("utf-8", "surrogateescape")
    ta.save()
    return env.file.save()


def patch_yoo_manifest(man: bytes, old_bundle: bytes, new_bundle: bytes) -> bytes:
    old_md5 = hashlib.md5(old_bundle).hexdigest().encode("ascii")
    new_md5 = hashlib.md5(new_bundle).hexdigest().encode("ascii")
    old_crc_hex = struct.pack("<I", zlib.crc32(old_bundle) & 0xFFFFFFFF).hex().encode("ascii")
    new_crc_hex = struct.pack("<I", zlib.crc32(new_bundle) & 0xFFFFFFFF).hex().encode("ascii")
    old_size = struct.pack("<Q", len(old_bundle))
    new_size = struct.pack("<Q", len(new_bundle))
    name = b"hotfixdll_assets_hotfixdll_hotfix_project_dll.bundle"
    idx = man.find(name)
    if idx < 0:
        raise SystemExit("bundle name missing in manifest")
    pos = idx + len(name)
    # uint32 UnityCRC — 改包后对不上，置 0 让 LoadFromFile 跳过 CRC
    man = man[:pos] + b"\x00\x00\x00\x00" + man[pos + 4 :]
    out = man.replace(old_md5, new_md5, 1)
    if out == man:
        raise SystemExit("manifest md5 not found")
    out2 = out.replace(old_crc_hex, new_crc_hex, 1)
    if out2 == out:
        raise SystemExit("manifest crc hex not found")
    out3 = out2.replace(old_size, new_size, 1)
    if out3 == out2:
        raise SystemExit("manifest size not found")
    return out3


def main() -> int:
    if len(sys.argv) < 4:
        print(
            "usage: shlt_patch_hotfix.py <in.bundle> <in.manifest.bytes> <out_dir>",
            file=sys.stderr,
        )
        return 2
    in_bundle = Path(sys.argv[1]).read_bytes()
    in_man = Path(sys.argv[2]).read_bytes()
    out_dir = Path(sys.argv[3])
    out_dir.mkdir(parents=True, exist_ok=True)

    env, ta, script = extract_dll_from_bundle(in_bundle)
    raw = script.encode("utf-8", "surrogateescape")
    patched = patch_dll_bytes(raw)
    (out_dir / "HotFix_Project.dll").write_bytes(patched)
    new_bundle = rewrite_bundle(in_bundle, patched)
    new_man = patch_yoo_manifest(in_man, in_bundle, new_bundle)
    (out_dir / "hotfixdll_assets_hotfixdll_hotfix_project_dll.bundle").write_bytes(new_bundle)
    (out_dir / "PackageManifest_HotfixDll_1.4.38.bytes").write_bytes(new_man)
    (out_dir / "PackageManifest_HotfixDll_1.4.38.hash").write_text(
        hashlib.md5(new_man).hexdigest(), encoding="ascii"
    )
    print(f"dll {len(patched)} bundle {len(in_bundle)} -> {len(new_bundle)}")
    print(f"manifest hash {hashlib.md5(new_man).hexdigest()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
