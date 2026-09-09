#!/usr/bin/env python3.11
# YooAsset 1.5.2 定制二进制清单补丁：
#   Bundle 记录 = u16len(长名) + u16len(FileHash=MD5 hex32) + u16len(FileCRC=crc32 LE字节 hex8) + i64(FileSize) + i32(refCount) + u16(tagCount)
#   APK 内 rawfile 文件名 = FileHash + ".rawfile"（ccdda854… 即原文件 MD5）
# 改动: FileHash/FileCRC/FileSize 三字段原位等长替换 + .hash 文件(=清单 bytes 的 md5) 同步替换
import zipfile, zlib, hashlib, struct, sys, os

APK = "/Users/aliang/Documents/nixiang/02_apks/月圆之夜/月圆之夜_1.6.30.1_base.apk"
OUT = "/Users/aliang/Documents/nixiang/07_patched/月圆之夜/routeA"
NEW_RAW = f"{OUT}/ccdda85485ad592a3c5deac4b801609c.rawfile"
MF_ENTRY = "assets/yoo/DefaultPackage/PackageManifest_DefaultPackage_1.0.055.bytes"
HASH_ENTRY = "assets/yoo/DefaultPackage/PackageManifest_DefaultPackage_1.0.055.hash"
OLD_MD5 = "ccdda85485ad592a3c5deac4b801609c"


def u16s(b, p):
    n = int.from_bytes(b[p:p + 2], "little")
    return b[p + 2:p + 2 + n], p + 2 + n


def main():
    new_raw = open(NEW_RAW, "rb").read()
    n_md5 = hashlib.md5(new_raw).hexdigest()
    n_crc = (zlib.crc32(new_raw) & 0xFFFFFFFF).to_bytes(4, "little").hex()
    n_size = len(new_raw)
    print(f"[new] md5={n_md5} crc={n_crc} size={n_size}")

    z = zipfile.ZipFile(APK)
    mf = bytearray(z.read(MF_ENTRY))

    # 定位唯一 hotfix bundle 记录: u16len(32) + 旧md5
    pat = struct.pack("<H", 32) + OLD_MD5.encode()
    locs = [m.start() for m in __import__("re").finditer(__import__("re").escape(pat), bytes(mf))]
    assert len(locs) == 1, f"旧 FileHash 出现 {len(locs)} 次"
    p = locs[0]
    # 校验整条记录自洽（实测布局: FileHash(u16s@p) + FileCRC(u16s@p+34) + i64(FileSize@p+44)）
    crc_str, p_size = u16s(mf, p + 2 + 32)   # FileCRC
    assert len(crc_str) == 8, f"CRC 字段长度异常 {len(crc_str)}"
    size = int.from_bytes(mf[p_size:p_size + 8], "little")
    assert size == 7177784, f"清单 size={size} != 原 rawfile 7177784"
    old_crc_raw = z.read(f"assets/yoo/DefaultPackage/{OLD_MD5}.rawfile")
    assert crc_str.decode() == (zlib.crc32(old_crc_raw) & 0xFFFFFFFF).to_bytes(4, "little").hex(), f"CRC 字段不自洽 {crc_str}"
    print(f"[locate] @{p:#x} FileHash={OLD_MD5} crc={crc_str.decode()} size={size} ✓ 自洽")

    # 原位等长替换 FileHash / FileCRC / FileSize
    mf[p + 2:p + 34] = n_md5.encode()
    mf[p + 36:p_size] = n_crc.encode()
    mf[p_size:p_size + 8] = struct.pack("<q", n_size)
    assert len(mf) == len(z.read(MF_ENTRY)), "清单长度变了"

    # .hash = 清单 bytes 的 md5（先验证语义）
    old_hash_file = z.read(HASH_ENTRY).decode().strip()
    assert hashlib.md5(bytes(mf)).hexdigest() != old_hash_file  # 已改
    assert hashlib.md5(z.read(MF_ENTRY)).hexdigest() == old_hash_file, ".hash ≠ 清单 md5?"
    new_mf_md5 = hashlib.md5(bytes(mf)).hexdigest()

    os.makedirs(OUT, exist_ok=True)
    open(f"{OUT}/PackageManifest_DefaultPackage_1.0.055.bytes", "wb").write(mf)
    open(f"{OUT}/PackageManifest_DefaultPackage_1.0.055.hash", "w").write(new_mf_md5)
    open(f"{OUT}/_entry_names.txt", "w").write(
        f"{MF_ENTRY}\n{HASH_ENTRY}\nassets/yoo/DefaultPackage/{n_md5}.rawfile")
    print(f"[patch] 清单三字段已改, .hash: {old_hash_file} -> {new_mf_md5}")
    print(f"[rename] APK 内 rawfile 应改名: {OLD_MD5}.rawfile -> {n_md5}.rawfile")


if __name__ == "__main__":
    main()
