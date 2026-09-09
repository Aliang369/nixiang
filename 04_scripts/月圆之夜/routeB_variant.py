#!/usr/bin/env python3.11
# 路线 B 二分工具: 构造补丁变体容器 + 生成沙箱注入物 (rawfile/__info/manifest 补丁/.hash)
# 用法: python3.11 routeB_variant.py <none|updategold|bools_fixed|all_fixed>
import sys, os, struct, hashlib, zlib, zipfile, lzma

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from ztg_dump import Meta
from decrypt_hotfix_stage1 import xxtea_encrypt, xxtea_decrypt

APK = "/Users/aliang/Documents/nixiang/02_apks/月圆之夜/月圆之夜_1.6.30.1_base.apk"
OUT = "/Users/aliang/Documents/nixiang/07_patched/月圆之夜/routeB"
KEY = b"yyzy_hotfix"
OLD_MD5 = "ccdda85485ad592a3c5deac4b801609c"
MF_OFF = 0x18D  # 清单内 hotfix bundle FileHash 记录偏移 (u16 len @MF_OFF, 内容 @MF_OFF+2)


def read7bit(d, p):
    r = 0; s = 0
    while True:
        b = d[p]; p += 1; r |= (b & 0x7F) << s
        if not b & 0x80:
            return r, p
        s += 7


def parse_container(dec: bytes):
    p = 0
    cnt = int.from_bytes(dec[p:p + 4], "little"); p += 4
    items = []
    for _ in range(cnt):
        n, p = read7bit(dec, p)
        name = dec[p:p + n].decode(); p += n
        dlen = int.from_bytes(dec[p:p + 4], "little"); p += 4
        items.append((name, p, dlen))
        p += dlen
    assert p == len(dec)
    return items


def get_orig_rawfile():
    z = zipfile.ZipFile(APK)
    return z.read(f"assets/yoo/DefaultPackage/{OLD_MD5}.rawfile"), z.read(
        "assets/yoo/DefaultPackage/PackageManifest_DefaultPackage_1.0.055.bytes")


def patch_dll(m: Meta, variant: str):
    b = bytearray(m.b)

    def rva2off(rva):
        for vaddr, fo, sz in m.secs:
            if vaddr <= rva < vaddr + sz:
                return fo + (rva - vaddr)
        raise ValueError(f"rva {rva:#x} 不在节内")

    def expect(off, old, what):
        cur = bytes(b[off:off + len(old)])
        assert cur == old, f"{what}: 原字节不符 {cur.hex()} != {old.hex()}"

    if variant in ("updategold", "all_fixed"):
        md = m.methoddef(33736)
        assert md["name"] == "UpdateGold"
        base = rva2off(md["rva"]) + 12
        for rel in (0x41, 0x79):
            expect(base + rel, b"\x03", f"UpdateGold+{rel:#x}")
            b[base + rel] = 0x16
        print("[patch] UpdateGold ldarg.1->ldc.i4.0 x2")

    if variant in ("bools_fixed", "all_fixed"):
        for rid, want in ((3152, "get_isUnlockAll"), (9339, "IsUnLockCard")):
            md = m.methoddef(rid)
            assert md["name"] == want
            o = rva2off(md["rva"])
            h0 = b[o]
            if h0 & 3 == 2:      # tiny: 头 1B, 代码 @ o+1
                code = o + 1
                assert h0 >> 2 >= 2, f"{want}: tiny 原体过小"
                kind = "tiny"
            elif h0 & 3 == 3:    # fat: 头 12B, 代码 @ o+12
                code = o + 12
                assert int.from_bytes(b[o + 4:o + 8], "little") >= 2, f"{want}: fat 原体过小"
                kind = "fat"
            else:
                raise AssertionError(f"{want}: 非法体头 {h0:#x}")
            orig2 = bytes(b[code:code + 2])
            b[code:code + 2] = b"\x17\x2A"   # ldc.i4.1; ret —— 头保持原样
            print(f"[patch] {want} [{kind}] 代码前2B {orig2.hex()} -> 172a (保头)")
    return bytes(b)


def main():
    variant = sys.argv[1] if len(sys.argv) > 1 else "all_fixed"
    os.makedirs(OUT, exist_ok=True)
    orig_raw, orig_mf = get_orig_rawfile()
    orig_lz = xxtea_decrypt(orig_raw, KEY)
    props = orig_lz[:5]
    dec = lzma.LZMADecompressor(format=lzma.FORMAT_ALONE).decompress(orig_lz)
    items = parse_container(dec)
    name, off, dlen = [it for it in items if it[0] == "Assembly-CSharp.dll"][0]

    dll_path = f"{OUT}/orig_Assembly-CSharp.dll"
    if not os.path.exists(dll_path):
        open(dll_path, "wb").write(dec[off:off + dlen])
    patched = patch_dll(Meta(dll_path), variant)
    assert len(patched) == dlen
    diff = [i for i in range(dlen) if dec[off + i] != patched[i]]
    print(f"[patch] 变体 {variant}: 改动 {len(diff)}B @ {[hex(x) for x in diff]}")

    new_dec = dec[:off] + patched + dec[off + dlen:]
    dict_size = struct.unpack("<I", props[1:5])[0]
    filt = [{"id": lzma.FILTER_LZMA1, "dict_size": dict_size, "lc": 3, "lp": 0, "pb": 2}]
    new_lz = lzma.compress(new_dec, format=lzma.FORMAT_ALONE, filters=filt)
    assert new_lz[:5] == props, f"props 漂移"
    raw = xxtea_encrypt(new_lz, KEY)

    # 回环自验证
    back = xxtea_decrypt(raw, KEY)
    back_dec = lzma.LZMADecompressor(format=lzma.FORMAT_ALONE).decompress(back)
    assert back_dec == new_dec, "回环容器不一致"
    print(f"[repack] 容器 {len(new_dec)}B -> LZMA {len(new_lz)}B -> 密文 {len(raw)}B, 回环 PASS")

    md5 = hashlib.md5(raw).hexdigest()
    crc = (zlib.crc32(raw) & 0xFFFFFFFF).to_bytes(4, "little").hex()
    info = struct.pack("<H", 8) + crc.encode() + struct.pack("<q", len(raw))
    open(f"{OUT}/{variant}.rawfile", "wb").write(raw)
    open(f"{OUT}/{variant}.info", "wb").write(info)

    # 沙箱清单原位等长替换 FileHash/CRC/Size
    mf = bytearray(orig_mf)
    p = MF_OFF
    assert int.from_bytes(mf[p:p + 2], "little") == 32
    mf[p + 2:p + 34] = md5.encode()
    p_crc = p + 34
    assert int.from_bytes(mf[p_crc:p_crc + 2], "little") == 8
    mf[p_crc + 2:p_crc + 10] = crc.encode()
    mf[p_crc + 10:p_crc + 18] = struct.pack("<q", len(raw))
    assert len(mf) == len(orig_mf)
    mf_hash = hashlib.md5(bytes(mf)).hexdigest()
    open(f"{OUT}/{variant}.manifest.bytes", "wb").write(mf)
    open(f"{OUT}/{variant}.manifest.hash", "w").write(mf_hash)
    print(f"[manifest] md5={md5} crc={crc} size={len(raw)} mf_hash={mf_hash}")


if __name__ == "__main__":
    main()
