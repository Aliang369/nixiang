#!/usr/bin/env python3.11
# 标准 PE .NET 程序集的 IL 反汇编（base_* 补充程序集为标准 PE，非 ZTG）
# 用法:
#   python3.11 il_disasm_pe.py <dll> list <类名关键字>
#   python3.11 il_disasm_pe.py <dll> il   <类型全名正则> <方法名正则>
import sys, re, struct
sys.path.insert(0, __file__.rsplit("/", 1)[0])
import dnfile
from ztg_dump import IL1, IL2


def resolve(pe, tok):
    t, rid = tok >> 24, tok & 0xFFFFFF
    try:
        if t == 0x70:  # UserString
            us = pe.net.user_strings.get(rid)
            return f'"{us.value}"' if us else f"us[{rid}]"
        if t == 0x0A:  # MemberRef
            mr = pe.net.mdtables.MemberRef.rows[rid - 1]
            cls = mr.Class.row
            cn = str(getattr(cls, "TypeName", getattr(cls, "Name", "?")))
            ns = str(getattr(cls, "TypeNamespace", ""))
            return f"{ns + '.' if ns else ''}{cn}::{mr.Name}"
        if t == 0x06:  # MethodDef
            md = pe.net.mdtables.MethodDef.rows[rid - 1]
            return f"method {md.Name}"
        if t == 0x04:
            return f"fld {pe.net.mdtables.Field.rows[rid - 1].Name}"
        if t == 0x02:
            td = pe.net.mdtables.TypeDef.rows[rid - 1]
            return f"{td.TypeNamespace}.{td.TypeName}"
        if t == 0x01:
            tr = pe.net.mdtables.TypeRef.rows[rid - 1]
            return f"{tr.TypeNamespace}.{tr.TypeName}"
        if t == 0x1B:
            return f"typespec[{rid}]"
        if t == 0x11:
            return f"sig[{rid}]"
        if t == 0x2B:
            return f"methodspec[{rid}]"
    except Exception as e:
        return f"tok{tok:#x}(err {e})"
    return f"tok{tok:#x}"


def rva2data(pe, rva):
    off = pe.get_offset_from_rva(rva)
    return off


def read_body(pe, rva):
    off = rva2data(pe, rva)
    b = pe.__data__
    h0 = b[off]
    if h0 & 3 == 2:
        size = h0 >> 2
        return b[off + 1:off + 1 + size], "tiny"
    hdr = (b[off + 1] >> 4) * 4
    code_size = struct.unpack_from("<I", b, off + 4)[0]
    return b[off + hdr:off + hdr + code_size], "fat"


def disasm(pe, il):
    out = []
    i = 0
    n = len(il)
    while i < n:
        pos = i
        op = il[i]; i += 1
        if op == 0xFE:
            op2 = il[i]; i += 1
            name, olen = IL2.get(op2, (f"fe{op2:02x}", 0))
        else:
            name, olen = IL1.get(op, (f"op{op:02x}", 0))
        txt = f"  IL_{pos:04x}: {name}"
        if olen == -1:  # switch
            cnt = struct.unpack_from("<I", il, i)[0]; i += 4
            targets = struct.unpack_from(f"<{cnt}I", il, i); i += 4 * cnt
            txt += f" ({cnt}) {list(targets)}"
        elif olen:
            v = il[i:i + olen]; i += olen
            if olen == 4 and name in ("call", "callvirt", "newobj", "ldflda", "ldsflda",
                                      "ldftn", "newarr", "ldtoken", "initobj", "cpobj",
                                      "ldobj", "stobj", "box", "unbox", "unbox.any",
                                      "castclass", "isinst", "ldelem", "stelem",
                                      "constrained.", "sizeof"):
                tok = struct.unpack("<I", v)[0]
                txt += f" {resolve(pe, tok)}"
            elif olen == 4:
                txt += f" {struct.unpack('<i', v)[0]} ({struct.unpack('<f', v)[0]:g})" if name == "ldc.r4" else f" {struct.unpack('<I', v)[0]:#x}"
            elif olen == 8:
                txt += f" {struct.unpack('<q', v)[0]}"
            elif olen == 2:
                txt += f" {struct.unpack('<h', v)[0]}"
            elif olen == 1:
                txt += f" {v[0]}"
        out.append(txt)
    return out


def main():
    path, cmd = sys.argv[1], sys.argv[2]
    pe = dnfile.dnPE(path)
    if cmd == "list":
        kw = sys.argv[3]
        for td in pe.net.mdtables.TypeDef:
            nm = str(td.TypeName)
            if kw.lower() in nm.lower():
                print(f"=== {td.TypeNamespace}.{nm} ===")
                for m in td.MethodList:
                    md = m.row
                    body = ""
                    if md.Rva:
                        try:
                            il, kind = read_body(pe, md.Rva)
                            body = f" il={len(il)}B {kind}"
                        except Exception:
                            body = " il=?"
                    print(f"   [{md.Name}] rva={md.Rva:#x}{body}")
    elif cmd == "il":
        tpat, mpat = sys.argv[3], sys.argv[4]
        rt, rm = re.compile(tpat, re.I), re.compile(mpat)
        for td in pe.net.mdtables.TypeDef:
            full = f"{td.TypeNamespace}.{td.TypeName}"
            if not rt.search(full):
                continue
            for m in td.MethodList:
                md = m.row
                if not rm.search(str(md.Name)) or not md.Rva:
                    continue
                il, kind = read_body(pe, md.Rva)
                print(f"\n=== {full}::{md.Name} ({len(il)}B {kind}) ===")
                print("\n".join(disasm(pe, il)))


if __name__ == "__main__":
    main()
