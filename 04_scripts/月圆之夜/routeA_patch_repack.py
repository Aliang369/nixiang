#!/usr/bin/env python3.11
# 路线 A：.ZTG 静态 IL patch → 容器原位拼接 → LZMA+XXTEA 重加密 → 回环自验证
#
# 补丁集（2026-09-08 定稿，全部为 1:1 / 缩短原位替换，容器条目长度不变）：
#   1. UserInfo::UpdateGold [33736] 金币锁定：
#      代码 0x41、0x79 两处 ldarg.1(03) -> ldc.i4.0(16)
#      （检查式 curgold<-delta 变 curgold<-0 永不过；更新变 curgold+=0；增减全冻结）
#   2. EditorSettingExtension::get_isUnlockAll [3152]：tiny 体覆盖 0A 17 2A（ldc.i4.1; ret）
#   3. AchieveConfig::IsUnLockCard [9339]：同上覆盖（卡牌图鉴显示层）
#
# 硬约束：ZTG 解释器 opcode 表经重映射（0x65=neg / 0x6B=conv.r4 / 0x5A=mul 实证），
#         补丁只允许使用已证实为原值的 opcode：ldc/ldarg/ret/分支/ldfld/stfld/call(virt)。
import struct, sys, os, lzma

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from ztg_dump import Meta
from decrypt_hotfix_stage1 import xxtea_encrypt, xxtea_decrypt

DLL = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/hotfix_dlls/hotfix_Assembly-CSharp.dll"
CONTAINER = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/hotfix_dlls/hotfix_container.bin"
OUT_DIR = "/Users/aliang/Documents/nixiang/07_patched/月圆之夜/routeA"
KEY = b"yyzy_hotfix"


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


def patch_dll() -> bytes:
    m = Meta(DLL)
    b = bytearray(m.b)

    def rva2off(rva):
        for vaddr, fo, sz in m.secs:
            if vaddr <= rva < vaddr + sz:
                return fo + (rva - vaddr)
        raise ValueError(f"rva {rva:#x} 不在节内")

    def expect(off, old, what):
        cur = bytes(b[off:off + len(old)])
        assert cur == old, f"{what}: 原字节不符 {cur.hex()} != {old.hex()}"

    # --- 1. UpdateGold [33736] 两处 ldarg.1 -> ldc.i4.0 ---
    md = m.methoddef(33736)
    assert md["name"] == "UpdateGold"
    base = rva2off(md["rva"]) + 12          # fat 头 12B，代码起始
    for rel in (0x41, 0x79):                # 检查式 / 更新式的 ldarg.1
        expect(base + rel, b"\x03", f"UpdateGold+{rel:#x}")
        b[base + rel] = 0x16                # ldc.i4.0

    # --- 2/3. bool getter 体覆盖: tiny(2B) = 0A 17 2A ---
    TINY_TRUE = bytes([0x0A, 0x17, 0x2A])   # header(2B)+ldc.i4.1+ret
    for rid, want in ((3152, "get_isUnlockAll"), (9339, "IsUnLockCard")):
        md = m.methoddef(rid)
        assert md["name"] == want
        o = rva2off(md["rva"])
        h0 = b[o]
        assert (h0 & 3) in (2, 3), f"{want}: 非法体头 {h0:#x}"
        assert h0 >> 2 >= 2 or (h0 & 3) == 3, f"{want}: 原体过小"
        b[o:o + 3] = TINY_TRUE

    out = os.path.join(OUT_DIR, "hotfix_Assembly-CSharp.patched.dll")
    open(out, "wb").write(b)
    print(f"[patch] DLL 补丁完成 -> {out} ({len(b)}B, 与原文件等长={len(b)==os.path.getsize(DLL)})")
    return bytes(b)


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    patched = patch_dll()

    orig_dll = open(DLL, "rb").read()
    diff = [i for i in range(len(orig_dll)) if orig_dll[i] != patched[i]]
    print(f"[patch] 改动字节数: {len(diff)} @ {', '.join(hex(x) for x in diff)}")

    # --- 容器原位拼接（hotfix_container.bin 为已解密明文容器）---
    dec = open(CONTAINER, "rb").read()
    items = parse_container(dec)
    tgt = [it for it in items if it[0] == "Assembly-CSharp.dll"]
    assert len(tgt) == 1, f"容器内未唯一命中 Assembly-CSharp.dll: {[it[0] for it in items]}"
    name, off, dlen = tgt[0]
    assert dlen == len(patched) == len(orig_dll), f"条目长度不符 {dlen}"
    new_dec = dec[:off] + patched + dec[off + dlen:]
    print(f"[container] '{name}' @ {off:#x} len {dlen} 已拼接")

    # --- LZMA alone（沿用原流 props: lc3 lp0 pb2 + 原 dict）+ XXTEA ---
    props = open("/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/hotfix_dlls/hotfix_lzma.bin", "rb").read()[:5]
    dict_size = struct.unpack("<I", props[1:5])[0]
    filt = [{"id": lzma.FILTER_LZMA1, "dict_size": dict_size, "lc": 3, "lp": 0, "pb": 2}]
    new_lz = lzma.compress(new_dec, format=lzma.FORMAT_ALONE, filters=filt)
    assert new_lz[:5] == props, f"props 漂移 {new_lz[:5].hex()} != {props.hex()}"
    new_raw = xxtea_encrypt(new_lz, KEY)
    out_raw = os.path.join(OUT_DIR, "ccdda85485ad592a3c5deac4b801609c.rawfile")
    open(out_raw, "wb").write(new_raw)
    print(f"[repack] LZMA(dict={dict_size:#x}) {len(new_dec)}B->{len(new_lz)}B ; XXTEA -> {out_raw} ({len(new_raw)}B, 原 rawfile 7177784B)")

    # --- 回环验证: 重解并逐字节比对 ---
    back_lz = xxtea_decrypt(new_raw, KEY)
    assert back_lz is not None, "回环 XXTEA 失败"
    back_dec = lzma.LZMADecompressor(format=lzma.FORMAT_ALONE).decompress(back_lz)
    assert back_dec == new_dec, "回环容器不一致!"
    items2 = parse_container(back_dec)
    n2, o2, d2 = [it for it in items2 if it[0] == "Assembly-CSharp.dll"][0]
    assert back_dec[o2:o2 + d2] == patched, "回环 DLL 不一致!"
    untouched = all(back_dec[it[1]:it[1] + it[2]] == dec[it[1]:it[1] + it[2]]
                    for it in items if it[0] != "Assembly-CSharp.dll")
    print(f"[verify] 回环 PASS: {len(items2)} 项, 其余 {sum(1 for it in items if it[0]!='Assembly-CSharp.dll')} 项字节一致={untouched}")

    # --- 补丁后 DLL 重新解析验证 ---
    open("/tmp/routeA_verify.dll", "wb").write(patched)
    mv = Meta("/tmp/routeA_verify.dll")
    for rid, want in ((33736, "UpdateGold"), (3152, "get_isUnlockAll"), (9339, "IsUnLockCard")):
        md = mv.methoddef(rid)
        assert md["name"] == want
        print(f"[verify] [{rid}] {want} rva={md['rva']:#x} 可定位 ✓")
    print("[done] routeA 产物就绪，待原 APK 组装")


if __name__ == "__main__":
    main()
