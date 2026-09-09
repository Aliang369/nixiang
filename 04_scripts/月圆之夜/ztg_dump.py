#!/usr/bin/env python3.11
# .ZTG (ZTGame 定制 HybridCLR 预解析镜像) 元数据解析器 v2
#
# 布局:
#   0x00 ".ZTG"
#   0x08 u32 0x18   0x0C u32 3(节数)   0x10 u32 0x3C(条目区)  0x14 u32 5(流数)
#   0x18 节表 3 组 (u32 rva, u32 fileoff, u32 size)   <- 方法体池在节1
#   0x3C 条目 5 组 (u16 kind, i32 off, i32 size)
#     kind: 1=#~ 2=#Strings 3=#US 4=#GUID 5=#Blob
#   #~ 为标准 ECMA-335 tables stream(压缩布局, 标准表编号)
import struct, sys, os, re

KIND_STREAM = {1: "#~", 2: "#Strings", 3: "#US", 4: "#GUID", 5: "#Blob"}

# 标准表号
MODULE, TYPEREF, TYPEDEF, FIELD, METHODDEF, PARAM = 0x00, 0x01, 0x02, 0x04, 0x06, 0x08
INTERFACEIMPL, MEMBERREF, CONSTANT, CUSTOMATTR, FIELDMARSHAL, DECLSEC = 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E
CLASSLAYOUT, STANDALONESIG, EVENTMAP, EVENT, PROPERTYMAP, PROPERTY = 0x0F, 0x11, 0x12, 0x14, 0x15, 0x17
METHODSEMANTICS, METHODIMPL, MODULEREF, TYPESPEC, IMPLMAP, FIELDRVA = 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D

T_TYPEDEFORREF = [TYPEDEF, TYPEREF, TYPESPEC]                    # 2bit
T_HASCONST = [FIELD, PARAM, PROPERTY]                            # 2bit
T_HASCA = [TYPEDEF, METHODDEF, FIELD, PARAM, EVENT, PROPERTY, MODULEREF,
           DECLSEC, INTERFACEIMPL, MEMBERREF, MODULE, PARAM - 1, TYPESPEC,
           ASSEMBLY := 0x20, ASSEMBLYREF := 0x23, FILE := 0x26, EXPORTEDTYPE := 0x27,
           MANIFESTRESOURCE := 0x28, NESTEDCLASS := 0x29, GENERICPARAM := 0x2A,
           METHODSPEC := 0x2B, 0x17, 0x14, 0x0E, 0x02, 0x06, 0x04, 0x08]  # 5bit
T_HASDECLSEC = [TYPEDEF, METHODDEF, 0x20]                        # 2bit
T_MEMBERREFPARENT = [TYPEDEF, TYPEREF, MODULEREF, METHODDEF, TYPESPEC]  # 3bit
T_HASSEMANTICS = [EVENT, PROPERTY]                               # 1bit
T_METHODDEFORREF = [METHODDEF, MEMBERREF]                        # 1bit
T_MEMBERFORWARDED = [FIELD, METHODDEF]                           # 1bit
T_CUSTOMATTRTYPE = [METHODDEF, MEMBERREF]                        # 3bit (tag2,3有效)
T_RESOLUTIONSCOPE = [MODULE, MODULEREF, 0x23, TYPEREF]           # 2bit
T_TYPEORMETHODDEF = [TYPEDEF, METHODDEF]                         # 2bit
T_IMPLEMENTATION = [0x26, 0x23, 0x27]                            # 2bit


def rc4(data: bytes, key: bytes) -> bytes:
    S = list(range(256))
    j = 0
    for i in range(256):
        j = (j + S[i] + key[i % len(key)]) & 0xFF
        S[i], S[j] = S[j], S[i]
    out = bytearray(len(data))
    i = j = 0
    for n, c in enumerate(data):
        i = (i + 1) & 0xFF
        j = (j + S[i]) & 0xFF
        S[i], S[j] = S[j], S[i]
        out[n] = c ^ S[(S[i] + S[j]) & 0xFF]
    return bytes(out)


def parse(path):
    b = open(path, "rb").read()
    assert b[:4] == b".ZTG", "魔数不符"
    nsec = struct.unpack_from("<I", b, 0x0C)[0]
    secs = [struct.unpack_from("<III", b, 0x18 + i * 12) for i in range(nsec)]
    cnt = struct.unpack_from("<i", b, 0x14)[0]
    streams = {}
    for i in range(cnt):
        kind, off, size = struct.unpack_from("<Hii", b, 0x3C + i * 10)
        streams[KIND_STREAM[kind]] = (off, size)
    return b, secs, streams


class Meta:
    def __init__(self, path):
        self.path = path
        self.b, self.secs, self.st = parse(path)
        # #Strings: RC4, key = ZTG 文件头前 24 字节; #US: XOR 0x9E; #Blob: 明文
        self.key = self.b[:24]
        so, ss = self.st["#Strings"]
        self.STR = rc4(self.b[so:so + ss], self.key)
        uo, us_ = self.st["#US"]
        self.USX = bytes(c ^ 0x9E for c in self.b[uo:uo + us_])
        t_off, t_size = self.st["#~"]
        b = self.b
        self.hsz = b[t_off + 6]
        self.valid, = struct.unpack_from("<Q", b, t_off + 8)   # 掩码 8 字节, 含 ≥0x20 表
        p = t_off + 24
        self.rows = {}
        for i in range(64):
            if self.valid >> i & 1:
                self.rows[i] = struct.unpack_from("<I", b, p)[0]
                p += 4
        self.tstart = p
        self._layout()

    def rva2off(self, rva):
        for vaddr, fo, sz in self.secs:
            if vaddr <= rva < vaddr + sz:
                return fo + (rva - vaddr)
        return None

    # ---- 索引宽度 ----
    def idx(self, table):
        return 4 if self.rows.get(table, 0) > 0xFFFF else 2

    def cidx(self, tables, bits):
        m = max((self.rows.get(t, 0) for t in tables), default=0)
        return 4 if (m << bits) > 0xFFFF else 2

    def _layout(self):
        S = self._S = 4 if self.hsz & 1 else 2       # #Strings
        G = self._G = 4 if self.hsz & 2 else 2       # #GUID
        B = self._B = 4 if self.hsz & 4 else 2       # #Blob
        c2_tdr = self.cidx(T_TYPEDEFORREF, 2)
        c2_hc = self.cidx(T_HASCONST, 2)
        c5_hca = self.cidx(T_HASCA, 5)
        c1_hfm = self.cidx(T_MEMBERFORWARDED, 1)  # FieldMarshal 与 MemberForwarded 同编码域
        c2_hds = self.cidx(T_HASDECLSEC, 2)
        c3_mrp = self.cidx(T_MEMBERREFPARENT, 3)
        c1_hs = self.cidx(T_HASSEMANTICS, 1)
        c1_mdr = self.cidx(T_METHODDEFORREF, 1)
        c1_mf = self.cidx(T_MEMBERFORWARDED, 1)
        c3_cat = self.cidx(T_CUSTOMATTRTYPE, 3)
        c2_rs = self.cidx(T_RESOLUTIONSCOPE, 2)
        c2_impl = self.cidx(T_IMPLEMENTATION, 2)
        c2_tom = self.cidx(T_TYPEORMETHODDEF, 2)     # GenericParam.Owner
        i_td, i_f, i_md, i_p = self.idx(2), self.idx(4), self.idx(6), self.idx(8)
        i_ev, i_pr = self.idx(0x14), self.idx(0x17)
        i_mr = self.idx(0x0A)
        i_gp = self.idx(0x2A)                        # GenericParam
        R = {
            0x00: 2 + S + G * 3,                       # Module
            0x01: c2_rs + S + S,                       # TypeRef
            0x02: 4 + S + S + c2_tdr + i_f + i_md,     # TypeDef
            0x04: 2 + S + B,                           # Field
            0x06: 4 + 2 + 2 + S + B + i_p,             # MethodDef
            0x08: 2 + 2 + S,                           # Param
            0x09: i_td + c2_tdr,                       # InterfaceImpl
            0x0A: c3_mrp + S + B,                      # MemberRef
            0x0B: 1 + 1 + c2_hc + B,                   # Constant
            0x0C: c5_hca + c3_cat + B,                 # CustomAttribute
            0x0D: c1_hfm + B,                          # FieldMarshal
            0x0E: 2 + c2_hds + B,                      # DeclSecurity
            0x0F: 2 + 4 + i_td,                        # ClassLayout
            0x11: B,                                   # StandAloneSig
            0x12: i_td + i_ev,                         # EventMap
            0x14: 2 + S + c2_tdr,                      # Event
            0x15: i_td + i_pr,                         # PropertyMap
            0x17: 2 + S + B,                           # Property
            0x18: 2 + i_md + c1_hs,                    # MethodSemantics
            0x19: i_td + c1_mdr + c1_mdr,              # MethodImpl
            0x1A: S,                                   # ModuleRef
            0x1B: B,                                   # TypeSpec
            0x1C: 2 + c1_mf + S + i_mr,                # ImplMap
            0x1D: 4 + i_f,                             # FieldRVA
            0x20: 4 + 2 + 2 + 2 + 2 + 4 + B + S + S,   # Assembly
            0x23: 2 * 4 + 4 + B + S + S + B,           # AssemblyRef
            0x29: i_td + i_td,                         # NestedClass
            0x2A: 2 + 2 + c2_tom + S,                  # GenericParam
            0x2B: c1_mdr + B,                          # MethodSpec
            0x2C: i_gp + c2_tdr,                       # GenericParamConstraint
        }
        self.tab = {}
        p = self.tstart
        for t in sorted(self.rows):
            if t not in R:
                raise ValueError(f"未实现的表 0x{t:x}, 行数 {self.rows[t]}")
            self.tab[t] = (p, R[t])
            p += R[t] * self.rows[t]
        self.tend = p

    # ---- 堆读取 ----
    def strings(self, i):
        e = self.STR.index(b"\x00", i)
        return self.STR[i:e].decode("utf-8", "replace")

    def us(self, i):
        p = i
        L = self.USX[p]; p += 1
        if L & 0xC0 == 0x80:
            L = ((L & 0x3F) << 8) | self.USX[p]; p += 1
        elif L & 0xC0 == 0xC0:
            L = ((L & 0x1F) << 24) | (self.USX[p] << 16) | (self.USX[p + 1] << 8) | self.USX[p + 2]; p += 3
        return self.USX[p:p + max(L - 1, 0)].decode("utf-16-le", "replace")

    def blob(self, i):
        off, _ = self.st["#Blob"]
        p = off + i
        L = self.b[p]; p += 1
        if L & 0xC0 == 0x80:
            L = ((L & 0x3F) << 8) | self.b[p]; p += 1
        elif L & 0xC0 == 0xC0:
            L = ((L & 0x1F) << 24) | (self.b[p] << 16) | (self.b[p + 1] << 8) | self.b[p + 2]; p += 3
        return self.b[p:p + L]

    def rows_of(self, t):
        off, rs = self.tab[t]
        for r in range(self.rows[t]):
            yield r + 1, off + r * rs  # rid 从 1 起

    # ---- 记录读取 ----
    def typedef(self, rid):
        off, rs = self.tab[TYPEDEF]
        p = off + (rid - 1) * rs
        S = self._S
        c2 = self.cidx(T_TYPEDEFORREF, 2)
        i_f, i_md = self.idx(4), self.idx(6)
        flags = struct.unpack_from("<I", self.b, p)[0]
        ni = int.from_bytes(self.b[p + 4:p + 4 + S], "little")
        nsi = int.from_bytes(self.b[p + 4 + S:p + 4 + 2 * S], "little")
        q = p + 4 + 2 * S
        ext_raw = int.from_bytes(self.b[q:q + c2], "little")
        ext = (ext_raw >> 2) | ((ext_raw & 3) << 24)      # TypeDefOrRef: tag 低 2bit + rid
        fl = int.from_bytes(self.b[q + c2:q + c2 + i_f], "little")
        ml = int.from_bytes(self.b[q + c2 + i_f:q + c2 + i_f + i_md], "little")
        return dict(rid=rid, flags=flags, name=self.strings(ni), ns=self.strings(nsi),
                    extends=ext, field_list=fl, method_list=ml, _off=p, _rs=rs)

    def methoddef(self, rid):
        off, rs = self.tab[METHODDEF]
        p = off + (rid - 1) * rs
        stored, implf, flags = struct.unpack_from("<IHH", self.b, p)
        S, B, i_p = self._S, self._B, self.idx(8)
        ni = int.from_bytes(self.b[p + 8:p + 8 + S], "little")
        si = int.from_bytes(self.b[p + 8 + S:p + 8 + S + B], "little")
        pl = int.from_bytes(self.b[p + 8 + S + B:p + 8 + S + B + i_p], "little")
        # RVA 解混淆 (libil2cpp 0x1499170 实证, 4 文件全量零异常):
        #   real = stored ^ (rid*0x01000193 + name_idx + 0x0A000FBE)
        # real==0 表示无 IL 体 (extern/abstract); 体定位 = fileoff + (real - sec.rva)
        rva = (stored ^ ((rid * 0x01000193 + ni + 0x0A000FBE) & 0xFFFFFFFF)) & 0xFFFFFFFF
        return dict(rid=rid, rva=rva, stored_rva=stored, impl_flags=implf, flags=flags,
                    name=self.strings(ni), sig=self.blob(si), param_list=pl, _off=p)

    # ---- 方法体 ----
    def body(self, rva):
        """返回 (il bytes, maxstack, locals_sig_token, flags)"""
        o = self.rva2off(rva)
        if o is None:
            return None
        b = self.b
        h0 = b[o]
        if h0 & 3 == 2:  # tiny: size<<2|2
            size = h0 >> 2
            return b[o + 1:o + 1 + size], 8, 0, "tiny"
        # fat: byte1 高 4bit = 头长(dword), [2:4]=maxstack, [4:8]=CodeSize, [8:12]=localVarSigTok
        hdr = (b[o + 1] >> 4) * 4
        maxstack = struct.unpack_from("<H", b, o + 2)[0]
        code_size = struct.unpack_from("<I", b, o + 4)[0]
        ltok = struct.unpack_from("<I", b, o + 8)[0]
        return b[o + hdr:o + hdr + code_size], maxstack, ltok, "fat"


# ---------------- dump 功能 ----------------
def full_name(td):
    return f"{td['ns']}.{td['name']}" if td["ns"] else td["name"]


def dump_types(m: Meta, out_path):
    tds = {rid: m.typedef(rid) for rid, _ in m.rows_of(TYPEDEF)}
    n_m = m.rows[METHODDEF]
    lines = []
    # 按 method_list 推 method 区间
    rids = sorted(tds)
    for i, rid in enumerate(rids):
        td = tds[rid]
        start = td["method_list"]
        end = tds[rids[i + 1]]["method_list"] if i + 1 < len(rids) else n_m + 1
        lines.append(f"\n=== {full_name(td)} (extends rid {td['extends']}) ===")
        for mr in range(start, min(end, n_m + 1)):
            if mr < 1:
                continue
            md = m.methoddef(mr)
            lines.append(f"  [{md['rid']}] {md['name']}  rva={md['rva']:#x} flags={md['flags']:#x}")
    open(out_path, "w").write("\n".join(lines))
    return tds


def resolve_token(m: Meta, tok):
    t, rid = tok >> 24, tok & 0xFFFFFF
    try:
        if t == 0x70:
            return f'ldstr "{m.us(rid)}"'
        if t == 0x0A:  # MemberRef
            off, rs = m.tab[MEMBERREF]
            p = off + (rid - 1) * rs
            w3 = m.cidx(T_MEMBERREFPARENT, 3)
            S = 4 if m.hsz & 1 else 2
            cls = int.from_bytes(m.b[p:p + w3], "little")
            ni = int.from_bytes(m.b[p + w3:p + w3 + S], "little")
            par = resolve_token(m, (cls & 7) << 24 | (cls >> 3)) if False else None
            return f"{memberref_parent(m, cls)}.{m.strings(ni)}"
        if t == 0x06:
            return f"method {m.methoddef(rid)['name']}"
        if t == 0x02 or t == 0x01 or t == 0x1B:
            if t == 0x02:
                td = m.typedef(rid)
                return full_name(td)
            if t == 0x01:
                off, rs = m.tab[TYPEREF]
                w2 = m.cidx(T_TYPEDEFORREF if False else T_RESOLUTIONSCOPE, 2)
                S = 4 if m.hsz & 1 else 2
                ni, nsi = struct.unpack_from("<II" if S == 4 else "<HH", m.b, off + (rid - 1) * rs + w2)
                return f"{m.strings(nsi)}.{m.strings(ni)}"
            return f"typespec[{rid}]"
        if t == 0x04:
            off, rs = m.tab[FIELD]
            S = 4 if m.hsz & 1 else 2
            ni = int.from_bytes(m.b[off + (rid - 1) * rs + 2:off + (rid - 1) * rs + 2 + S], "little")
            return f"fld {m.strings(ni)}"
        if t == 0x11:
            return f"sig[{rid}]"
    except Exception as e:
        return f"tok{tok:#x}(err {e})"
    return f"tok{tok:#x}"


def memberref_parent(m, cls):
    tag = cls & 7
    rid = cls >> 3
    try:
        if tag == 0:
            return full_name(m.typedef(rid))
        if tag == 1:
            off, rs = m.tab[TYPEREF]
            w2 = m.cidx(T_RESOLUTIONSCOPE, 2)
            S = 4 if m.hsz & 1 else 2
            ni, nsi = struct.unpack_from("<II" if S == 4 else "<HH", m.b, off + (rid - 1) * rs + w2)
            return f"{m.strings(nsi)}.{m.strings(ni)}"
        if tag == 4:
            return f"typespec[{rid}]"
    except Exception:
        pass
    return f"cls({tag},{rid})"


IL_SIZE = {}  # 简表: opcode -> (operand bytes, name)


def il_disasm(m: Meta, il: bytes):
    """极简 IL 反汇编, 仅解析操作数长度与 token"""
    out = []
    i = 0
    n = len(il)
    while i < n:
        op = il[i]; i += 1
        if op == 0xFE:
            op2 = il[i]; i += 1
            name, olen = IL2.get(op2, (f"fe{op2:02x}", 0))
        else:
            name, olen = IL1.get(op, (f"op{op:02x}", 0))
        txt = f"  {i:04x} {name}"
        if olen:
            v = il[i:i + olen]
            i += olen
            if olen == 4:
                tok = struct.unpack("<I", v)[0]
                if name in ("call", "callvirt", "newobj", "ldflda", "ldsflda",
                            "ldftn", "newarr", "ldtoken", "initobj", "cpobj",
                            "ldobj", "stobj", "box", "unbox", "unbox.any",
                            "castclass", "isinst", "sizeof", "calli", "ldc.i4"):
                    if name in ("ldc.i4",):
                        txt += f" {tok:#x}"
                    elif name == "calli":
                        txt += f" sig[{tok & 0xFFFFFF}]"
                    else:
                        txt += f" {resolve_token(m, tok)}"
                else:
                    txt += f" {tok:#x}"
            elif olen == 1:
                txt += f" {v[0]}"
            elif olen == 2:
                txt += f" {struct.unpack('<h', v)[0]}"
            elif olen == 8:
                txt += f" {struct.unpack('<q', v)[0]}"
        out.append(txt)
    return out


# 单字节 opcode: (名, 操作数字节数)
IL1 = {
    0x00: ("nop", 0), 0x01: ("break", 0), 0x02: ("ldarg.0", 0), 0x03: ("ldarg.1", 0),
    0x04: ("ldarg.2", 0), 0x05: ("ldarg.3", 0), 0x06: ("ldloc.0", 0), 0x07: ("ldloc.1", 0),
    0x08: ("ldloc.2", 0), 0x09: ("ldloc.3", 0), 0x0A: ("stloc.0", 0), 0x0B: ("stloc.1", 0),
    0x0C: ("stloc.2", 0), 0x0D: ("stloc.3", 0), 0x0E: ("ldarg.s", 1), 0x0F: ("ldarga.s", 1),
    0x10: ("starg.s", 1), 0x11: ("ldloc.s", 1), 0x12: ("ldloca.s", 1), 0x13: ("stloc.s", 1),
    0x14: ("ldnull", 0), 0x15: ("ldc.i4.m1", 0), 0x16: ("ldc.i4.0", 0), 0x17: ("ldc.i4.1", 0),
    0x18: ("ldc.i4.2", 0), 0x19: ("ldc.i4.3", 0), 0x1A: ("ldc.i4.4", 0), 0x1B: ("ldc.i4.5", 0),
    0x1C: ("ldc.i4.6", 0), 0x1D: ("ldc.i4.7", 0), 0x1E: ("ldc.i4.8", 0), 0x1F: ("ldc.i4.s", 1),
    0x20: ("ldc.i4", 4), 0x21: ("ldc.i8", 8), 0x22: ("ldc.r4", 4), 0x23: ("ldc.r8", 8),
    0x25: ("dup", 0), 0x26: ("pop", 0), 0x27: ("jmp", 4), 0x28: ("call", 4),
    0x29: ("calli", 4), 0x2A: ("ret", 0), 0x2B: ("br.s", 1), 0x2C: ("brfalse.s", 1),
    0x2D: ("brtrue.s", 1), 0x2E: ("beq.s", 1), 0x2F: ("bge.s", 1), 0x30: ("bgt.s", 1),
    0x31: ("ble.s", 1), 0x32: ("blt.s", 1), 0x33: ("bne.un.s", 1), 0x34: ("bge.un.s", 1),
    0x35: ("bgt.un.s", 1), 0x36: ("ble.un.s", 1), 0x37: ("blt.un.s", 1), 0x38: ("br", 4),
    0x39: ("brfalse", 4), 0x3A: ("brtrue", 4), 0x3B: ("beq", 4), 0x3C: ("bge", 4),
    0x3D: ("bgt", 4), 0x3E: ("ble", 4), 0x3F: ("blt", 4), 0x40: ("bne.un", 4),
    0x41: ("bge.un", 4), 0x42: ("bgt.un", 4), 0x43: ("ble.un", 4), 0x44: ("blt.un", 4),
    0x45: ("switch", -1), 0x46: ("ldind.i1", 0), 0x47: ("ldind.u1", 0), 0x48: ("ldind.i2", 0),
    0x49: ("ldind.u2", 0), 0x4A: ("ldind.i4", 0), 0x4B: ("ldind.u4", 0), 0x4C: ("ldind.i8", 0),
    0x4D: ("ldind.i", 0), 0x4E: ("ldind.r4", 0), 0x4F: ("ldind.r8", 0), 0x50: ("ldind.ref", 0),
    0x51: ("stind.ref", 0), 0x52: ("stind.i1", 0), 0x53: ("stind.i2", 0), 0x54: ("stind.i4", 0),
    0x55: ("stind.i8", 0), 0x56: ("stind.r4", 0), 0x57: ("stind.r8", 0),
}
for op in range(0x58, 0xC2):  # 算术/比较, 全部 0 操作数
    IL1.setdefault(op, (f"op{op:02x}", 0))
IL1.update({
    0x6F: ("callvirt", 4), 0x70: ("cpobj", 4), 0x71: ("ldobj", 4), 0x72: ("ldstr", 4),
    0x73: ("newobj", 4), 0x74: ("castclass", 4), 0x75: ("isinst", 4), 0x76: ("conv.r.un", 0),
    0x79: ("unbox", 4), 0x7A: ("throw", 0), 0x7B: ("ldfld", 4), 0x7C: ("ldflda", 4),
    0x7D: ("stfld", 4), 0x7E: ("ldsfld", 4), 0x7F: ("ldsflda", 4), 0x80: ("stsfld", 4),
    0x81: ("stobj", 4), 0x8C: ("box", 4), 0x8D: ("newarr", 4), 0x8E: ("ldlen", 0),
    0x8F: ("ldelema", 4), 0x91: ("ldelem.i1", 0), 0x92: ("ldelem.u1", 0), 0x93: ("ldelem.i2", 0),
    0x94: ("ldelem.u2", 0), 0x95: ("ldelem.i4", 0), 0x96: ("ldelem.u4", 0), 0x97: ("ldelem.i8", 0),
    0x98: ("ldelem.i", 0), 0x99: ("ldelem.r4", 0), 0x9A: ("ldelem.r8", 0), 0x9B: ("ldelem.ref", 0),
    0x9C: ("stelem.i", 0), 0x9D: ("stelem.i1", 0), 0x9E: ("stelem.i2", 0), 0x9F: ("stelem.i4", 0),
    0xA0: ("stelem.i8", 0), 0xA1: ("stelem.r4", 0), 0xA2: ("stelem.r8", 0), 0xA3: ("stelem.ref", 0),
    0xA4: ("ldelem", 4), 0xA5: ("stelem", 4), 0xA6: ("unbox.any", 4), 0xC2: ("refanyval", 4),
    0xC6: ("ckfinite", 0), 0xD0: ("ldtoken", 4), 0xD1: ("conv.u2", 0), 0xD2: ("conv.u1", 0),
    0xD3: ("conv.i", 0), 0xD6: ("add.ovf", 0), 0xD8: ("mul.ovf", 0), 0xDA: ("sub.ovf", 0),
    0xDC: ("endfinally", 0), 0xDD: ("leave", 4), 0xDE: ("leave.s", 1), 0xDF: ("stind.i", 0),
    0xE0: ("conv.u", 0),
})
IL2 = {
    0x00: ("arglist", 0), 0x01: ("ceq", 0), 0x02: ("cgt", 0), 0x03: ("cgt.un", 0),
    0x04: ("clt", 0), 0x05: ("clt.un", 0), 0x06: ("ldftn", 4), 0x07: ("ldvirtftn", 4),
    0x09: ("ldarg", 2), 0x0A: ("ldarga", 2), 0x0B: ("starg", 2), 0x0C: ("ldloc", 2),
    0x0D: ("ldloca", 2), 0x0E: ("stloc", 2), 0x0F: ("localloc", 0), 0x11: ("endfilter", 0),
    0x12: ("unaligned.", 1), 0x13: ("volatile.", 0), 0x14: ("tail.", 0), 0x15: ("initobj", 4),
    0x16: ("constrained.", 4), 0x17: ("cpblk", 0), 0x18: ("initblk", 0), 0x1A: ("rethrow", 0),
    0x1C: ("sizeof", 4), 0x1D: ("refanytype", 0), 0x1E: ("readonly.", 0),
}


def search(m: Meta, kw: str, ignorecase=True):
    hits = []
    for rid, _ in m.rows_of(TYPEDEF):
        td = m.typedef(rid)
        fn = full_name(td)
        if kw.lower() in fn.lower():
            hits.append(("typedef", fn))
    return hits


if __name__ == "__main__":
    DEFAULT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/hotfix_dlls/hotfix_Assembly-CSharp.dll"
    cmd = sys.argv[1] if len(sys.argv) > 1 else "info"
    path = sys.argv[2] if len(sys.argv) > 2 else DEFAULT
    m = Meta(path)
    if cmd == "info":
        print("流:", {k: (hex(a), hex(b)) for k, (a, b) in m.st.items()})
        print("表区域:", hex(m.tstart), "->", hex(m.tend), " #~ 大小:", hex(m.st['#~'][1]))
        print("表:", {hex(k): v for k, v in sorted(m.rows.items())})
        # TypeDef 前 10 个
        for rid, _ in list(m.rows_of(TYPEDEF))[:10]:
            td = m.typedef(rid)
            print(" ", rid, full_name(td))
        # MethodDef 前 10 个
        for rid, _ in list(m.rows_of(METHODDEF))[:10]:
            md = m.methoddef(rid)
            print(f"  M[{md['rid']}] {md['name']} rva={md['rva']:#x} -> off={m.rva2off(md['rva']) and hex(m.rva2off(md['rva']))}")
    elif cmd == "types":  # 全量类型+方法索引
        out = sys.argv[3] if len(sys.argv) > 3 else "/tmp/ztg_types.txt"
        dump_types(m, out)
        print("written", out)
    elif cmd == "il":  # il <pattern> 反汇编
        pat = sys.argv[3]
        r = re.compile(pat, re.I)
        n_m = m.rows[METHODDEF]
        tds = {rid: m.typedef(rid) for rid, _ in m.rows_of(TYPEDEF)}
        rids = sorted(tds)
        shown = 0
        for i, rid in enumerate(rids):
            td = tds[rid]
            start = td["method_list"]
            end = tds[rids[i + 1]]["method_list"] if i + 1 < len(rids) else n_m + 1
            for mr in range(start, min(end, n_m + 1)):
                if mr < 1:
                    continue
                md = m.methoddef(mr)
                if not r.search(md["name"]) and not r.search(full_name(td)):
                    continue
                il, ms, ltok, kind = m.body(md["rva"]) or (b"", 0, 0, "?")
                print(f"\n### {full_name(td)}::{md['name']} [{md['rid']}] rva={md['rva']:#x} {kind} {len(il)}B maxstack={ms}")
                if ltok:
                    print(f"    locals tok={ltok:#x}")
                print("\n".join(il_disasm(m, il)))
                shown += 1
                if shown >= 5:
                    sys.exit(0)
        print(f"(匹配 {shown} 个)")
    elif cmd == "find":  # find <keyword> 在类型名/方法名中搜
        kw = sys.argv[3]
        n_m = m.rows[METHODDEF]
        tds = {rid: m.typedef(rid) for rid, _ in m.rows_of(TYPEDEF)}
        rids = sorted(tds)
        for i, rid in enumerate(rids):
            td = tds[rid]
            fn = full_name(td)
            ms_ = []
            start = td["method_list"]
            end = tds[rids[i + 1]]["method_list"] if i + 1 < len(rids) else n_m + 1
            for mr in range(start, min(end, n_m + 1)):
                if mr >= 1 and kw.lower() in m.methoddef(mr)["name"].lower():
                    ms_.append(m.methoddef(mr)["name"])
            if kw.lower() in fn.lower() or ms_:
                print(f"{fn}  [{rid}]")
                for x in ms_[:20]:
                    print("   .", x)
