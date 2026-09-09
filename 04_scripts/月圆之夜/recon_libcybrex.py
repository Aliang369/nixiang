#!/usr/bin/env python3.11
# 月圆之夜 MHP 3.5.0 侦察：libcybrex.so 结构/熵/密码学常量/导出 + hp_* 家族画像
# 用法: python3.11 recon_libcybrex.py <libcybrex.so> [hp_cfg hp_refs ...]
import struct, sys, math
from collections import Counter

CRYPTO_CONSTS = {
    "AES_SBOX": bytes([0x63,0x7c,0x77,0x7b,0xf2,0x6b,0x6f,0xc5,0x30,0x01,0x67,0x2b,0xfe,0xd7,0xab,0x76]),
    "AES_INV_SBOX": bytes([0x52,0x09,0x6a,0xd5,0x30,0x36,0xa5,0x38,0xbf,0x40,0xa3,0x9e,0x81,0xf3,0xd7,0xfb]),
    "AES_RCON": bytes([0x01,0x02,0x04,0x08,0x10,0x20,0x40,0x80,0x1b,0x36]),
    "SM4_FK": bytes.fromhex("a3b1bac6"),
    "SM4_CK0": bytes.fromhex("00070e15"),
    "CHACHA_SIG": b"expand 32-byte k",
    "SALSA20_SIG": b"expand 16-byte k",
    "TEA_DELTA_LE": struct.pack("<I", 0x9E3779B9),
    "TEA_DELTA_BE": struct.pack(">I", 0x9E3779B9),
    "XXTEA_MX": struct.pack("<I", 0x9E3779B9),  # 同 delta
    "BLOWFISH_P0": bytes.fromhex("243f6a88"),
    "MD5_K0": bytes.fromhex("78a46ad7"),        # 0xd76aa478 LE
    "SHA256_H0": bytes.fromhex("6a09e667"),
    "SHA1_H0": bytes.fromhex("67452301"),
    "CRC32_TAB0": struct.pack("<I", 0x00000000) + struct.pack("<I", 0x77073096)[:4],
    "RC4_IDEASET": bytes([0x07,0x0c,0x11,0x16,0x1b,0x20,0x25,0x2a]),  # 无常量算法，仅占位
    "DES_PC1_0": bytes.fromhex("3a32312e"),  # DES IP 表常见形态
    "GCM_H0": bytes.fromhex("e1000000"),
    "ZUC_D": bytes.fromhex("3ada0a1a"),
    "RC5_P": struct.pack("<I", 0xB7E15163),
    "RC6_P": struct.pack("<I", 0xB7E15163),
    "SEED_KC0": bytes.fromhex("9e3779b9"),
    "SKIPJACK": bytes([0xa3,0x3d,0x94,0x78]),
}

def entropy(data: bytes) -> float:
    if not data:
        return 0.0
    c = Counter(data)
    n = len(data)
    return -sum((v / n) * math.log2(v / n) for v in c.values())

def parse_elf64(path):
    d = open(path, "rb").read()
    assert d[:4] == b"\x7fELF", "not ELF"
    ei_class, ei_data = d[4], d[5]
    if ei_class == 2:
        e_shoff, = struct.unpack_from("<Q", d, 0x28)
        e_shentsize, e_shnum, e_shstrndx = struct.unpack_from("<HHH", d, 0x3a)
    else:
        e_shoff, = struct.unpack_from("<I", d, 0x20)
        e_shentsize, e_shnum, e_shstrndx = struct.unpack_from("<HHH", d, 0x2e)
    secs = []
    if e_shoff and e_shnum:
        for i in range(e_shnum):
            off = e_shoff + i * e_shentsize
            if ei_class == 2:
                name, typ, flags, addr, offset, size, link, info, align, entsize = struct.unpack_from("<IIQQQQIIQQ", d, off)
            else:
                name, typ, flags, addr, offset, size, link, info, align, entsize = struct.unpack_from("<IIIIIIIIII", d, off)
            secs.append({"name": name, "type": typ, "addr": addr, "offset": offset, "size": size})
        shstr = secs[e_shstrndx]
        for s in secs:
            end = d.find(b"\x00", shstr["offset"] + s["name"])
            s["name_str"] = d[shstr["offset"] + s["name"]:end].decode(errors="replace")
    return d, secs

def dynsym_exports(d, secs):
    out = []
    symtab = next((s for s in secs if s["name_str"] == ".dynsym"), None)
    strtab = next((s for s in secs if s["name_str"] == ".dynstr"), None)
    if not symtab or not strtab:
        return out, False
    is64 = len(d) > 5 and d[4] == 2
    esz = 24 if is64 else 16
    n = symtab["size"] // esz
    base = strtab["offset"]
    for i in range(n):
        off = symtab["offset"] + i * esz
        if is64:
            st_name, st_info, st_other, st_shndx, st_value, st_size = struct.unpack_from("<IBBHQQ", d, off)
        else:
            st_name, st_value, st_size, st_info, st_other, st_shndx = struct.unpack_from("<IIIBBH", d, off)
        if st_name == 0:
            continue
        end = d.find(b"\x00", base + st_name)
        name = d[base + st_name:end].decode(errors="replace")
        bind = st_info >> 4
        if bind in (1, 2) and st_value != 0:  # GLOBAL/WEAK defined
            out.append((name, st_value, st_size))
    return out, True

PT_NAMES = {1: "LOAD", 2: "DYNAMIC", 3: "INTERP", 4: "NOTE", 6: "PHDR", 7: "TLS",
            0x6474e550: "GNU_EH_FRAME", 0x6474e551: "GNU_STACK", 0x6474e552: "GNU_RELRO",
            0x6474e553: "GNU_PROPERTY"}
DT_NAMES = {0: "NULL", 1: "NEEDED", 5: "STRTAB", 6: "SYMTAB", 4: "HASH", 10: "STRSZ",
            11: "SYMENT", 0x6ffffef5: "GNU_HASH", 12: "REL", 13: "RELSZ", 20: "PLTGOT",
            23: "JMPREL", 2: "PLTRELSZ", 3: "PLTGOTsz", 0x6ffffff0: "VERSYM", 25: "INIT_ARRAY",
            27: "FINI_ARRAY", 24: "INIT_ARRAYSZ", 7: "RELA", 8: "RELASZ", 0x6ffffff9: "RELACOUNT",
            0x6ffffffe: "VERNEED", 0x6fffffff: "VERNEEDNUM", 26: "SONAME", 14: "RPATH", 15: "RUNPATH", 29: "FLAGS_1"}

def parse_phdr(d):
    is64 = d[4] == 2
    if is64:
        e_phoff, = struct.unpack_from("<Q", d, 0x20)
        e_phentsize, e_phnum = struct.unpack_from("<HH", d, 0x36)
    else:
        e_phoff, = struct.unpack_from("<I", d, 0x1c)
        e_phentsize, e_phnum = struct.unpack_from("<HH", d, 0x2c)
    segs, dyn = [], []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        if is64:
            p_type, p_flags, p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align = struct.unpack_from("<IIQQQQQQ", d, off)
        else:
            p_type, p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_flags, p_align = struct.unpack_from("<IIIIIIII", d, off)
        segs.append(dict(type=p_type, flags=p_flags, offset=p_offset, vaddr=p_vaddr,
                         filesz=p_filesz, memsz=p_memsz, align=p_align))
        if p_type == 2:
            esz = 16 if is64 else 8
            for j in range(p_filesz // esz):
                doff = p_offset + j * esz
                tag, val = struct.unpack_from("<Qq", d, doff) if is64 else struct.unpack_from("<Ii", d, doff)
                if tag == 0:
                    break
                dyn.append((tag, val))
    return segs, dyn

def va2off(segs, va):
    for s in segs:
        if s["type"] == 1 and s["vaddr"] <= va < s["vaddr"] + s["filesz"]:
            return s["offset"] + (va - s["vaddr"])
    return None

def dyn_exports_via_dynamic(d, segs, dyn):
    dt = {t: v for t, v in dyn}
    symtab_va = dt.get(6)
    strtab_va = dt.get(5)
    is64 = d[4] == 2
    if not symtab_va or not strtab_va:
        return [], "no DT_SYMTAB/DT_STRTAB"
    sym_off = va2off(segs, symtab_va)
    str_off = va2off(segs, strtab_va)
    if sym_off is None or str_off is None:
        return [], "symtab/strtab VA 不在任何可加载段"
    # 符号数：优先 DT_HASH nchain
    n = None
    if dt.get(4):
        hoff = va2off(segs, dt[4])
        if hoff is not None:
            nbucket, nchain = struct.unpack_from("<II", d, hoff)
            n = nchain
    if n is None and dt.get(0x6ffffef5):
        n = 4096  # GNU_HASH 无直接计数，给上限再裁剪
    out = []
    esz = 24 if is64 else 16
    for i in range(n if n else 0):
        off = sym_off + i * esz
        if off + esz > len(d):
            break
        if is64:
            st_name, st_info, st_other, st_shndx, st_value, st_size = struct.unpack_from("<IBBHQQ", d, off)
        else:
            st_name, st_value, st_size, st_info, st_other, st_shndx = struct.unpack_from("<IIIBBH", d, off)
        if st_name == 0 or st_value == 0:
            continue
        end = d.find(b"\x00", str_off + st_name)
        name = d[str_off + st_name:end].decode(errors="replace")
        if not name:
            continue
        out.append((name, st_value, st_size, st_info >> 4, st_shndx))
        if len(out) > 2000:
            break
    return out, f"DT_HASH nchain={n}"

def main():
    path = sys.argv[1]
    d, secs = parse_elf64(path)
    print(f"== {path} ==")
    print(f"文件大小: {len(d)}  ELF class={'64' if d[4]==2 else '32'}  machine={struct.unpack_from('<H', d, 0x12)[0]}")
    e_entry, = struct.unpack_from("<Q", d, 0x18)
    print(f"entry=0x{e_entry:x}")
    segs, dyn = parse_phdr(d)
    print("\n-- program headers --")
    for s in segs:
        fl = "".join(c for c, b in zip("RWX", (4, 2, 1)) if s["flags"] & b)
        print(f"{PT_NAMES.get(s['type'], hex(s['type'])):<14} off=0x{s['offset']:<8x} va=0x{s['vaddr']:<9x} filesz=0x{s['filesz']:<8x} memsz=0x{s['memsz']:<8x} {fl}")
    print("\n-- .dynamic 关键项 --")
    for t, v in dyn:
        if t in DT_NAMES and DT_NAMES[t] != "NULL":
            print(f"  {DT_NAMES[t]:<12} = 0x{v:x}" + (f" ({v})" if t == 10 else ""))
    print("\n-- 经 PT_DYNAMIC 恢复的导出表 --")
    exps, note = dyn_exports_via_dynamic(d, segs, dyn)
    print(f"共 {len(exps)} 个 ({note})")
    for name, val, size, bind, shndx in exps[:120]:
        print(f"  va=0x{val:<9x} size={size:<7} bind={bind} shndx={shndx} {name}")
    print("\n-- sections (name / addr / off / size / entropy) --")
    for s in secs:
        if s["size"] == 0:
            continue
        ent = entropy(d[s["offset"]:s["offset"] + s["size"]][:262144])
        print(f"{s['name_str']:<22} addr=0x{s['addr']:x} off=0x{s['offset']:<8x} size=0x{s['size']:<8x} ent={ent:.2f}")
    print("\n-- 密码学常量扫描 --")
    hits = 0
    for name, pat in CRYPTO_CONSTS.items():
        idx = d.find(pat)
        if idx >= 0:
            # 多次出现只报前3
            positions = []
            p = idx
            while p >= 0 and len(positions) < 3:
                positions.append(p)
                p = d.find(pat, p + 1)
            print(f"HIT {name}: file_off={[hex(x) for x in positions]}")
            hits += 1
    if not hits:
        print("(无命中)")
    print("\n-- 导出符号 (GLOBAL/WEAK defined) --")
    exps, ok = dynsym_exports(d, secs)
    print(f"共 {len(exps)} 个")
    for name, val, size in exps[:80]:
        print(f"  0x{val:<10x} size={size:<7} {name}")
    # 粗粒度熵图（4KB 块，>7.5 标密文区）
    print("\n-- 高熵块图 (4KB 块熵>7.5，前 60 段合并) --")
    runs = []
    BLK = 4096
    cur = None
    for i in range(0, len(d), BLK):
        e = entropy(d[i:i + BLK])
        if e > 7.5:
            if cur and i <= cur[1] + BLK:
                cur = (cur[0], i + BLK)
            else:
                if cur: runs.append(cur)
                cur = (i, i + BLK)
        else:
            if cur and i > cur[1] + BLK:
                runs.append(cur); cur = None
    if cur: runs.append(cur)
    for a, b in runs[:60]:
        print(f"  0x{a:x}-0x{b:x} ({b-a}B)")

if __name__ == "__main__":
    main()
