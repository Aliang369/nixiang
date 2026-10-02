#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""消消英雄2 定制 Lua 5.3 字节码 反汇编器（诊断用）

格式（实测）:
  magic  "\x1bLua" + version 0x53 + format 0x01 + LUAC_DATA(6) +
         sizes(4 bytes: 04 04 08 08) + LUAC_INT(8) + LUAC_NUM(8) = 32 字节头
  之后为标准 Lua 5.3 结构：sizeupvalues(1) + source(字符串) + 函数体
字符串长度：1 字节，0xFF 表示后面跟 8 字节长度（与 5.3 一致）
"""
import sys, struct

OPNAMES = """MOVE LOADK LOADKX LOADBOOL LOADNIL GETUPVAL GETTABUP GETTABLE SETTABUP SETUPVAL
SETTABLE NEWTABLE SELF ADD SUB MUL MOD POW DIV IDIV BAND BOR BXOR SHL SHR UNM BNOT NOT LEN CONCAT
JMP EQ LT LE TEST TESTSET CALL TAILCALL RETURN FORLOOP FORPREP TFORCALL TFORLOOP SETLIST
CLOSURE VARARG EXTRAARG""".split()

IABC, IABX, IASBX, IAX = range(4)
OPMODE = {
    "MOVE": IABC, "LOADK": IABX, "LOADKX": IABX, "LOADBOOL": IABC, "LOADNIL": IABC,
    "GETUPVAL": IABC, "GETTABUP": IABC, "GETTABLE": IABC, "SETTABUP": IABC, "SETUPVAL": IABC,
    "SETTABLE": IABC, "NEWTABLE": IABC, "SELF": IABC, "ADD": IABC, "SUB": IABC, "MUL": IABC,
    "MOD": IABC, "POW": IABC, "DIV": IABC, "IDIV": IABC, "BAND": IABC, "BOR": IABC,
    "BXOR": IABC, "SHL": IABC, "SHR": IABC, "BNOT": IABC, "NOT": IABC, "LEN": IABC,
    "CONCAT": IABC, "JMP": IASBX, "EQ": IABC, "LT": IABC, "LE": IABC, "TEST": IABC,
    "TESTSET": IABC, "CALL": IABC, "TAILCALL": IABC, "RETURN": IABC, "FORLOOP": IASBX,
    "FORPREP": IASBX, "TFORCALL": IABC, "TFORLOOP": IASBX, "SETLIST": IABC, "CLOSURE": IABX,
    "VARARG": IABC, "EXTRAARG": IAX,
}
# 哪些 A 操作数是 "寄存器局部变量" —— 简化: 全部显示为 R(n)
KIND_NONE = {"NONE": 0, "K": 1, "RK": 2, "U": 3, "R": 4}
# 每个 opcode 的 A/B/C/x 语义（Lua 5.3 lopcodes.c luaP_opmodes / lopcodes.h 宏）
# 简化为: 打印时把 RK 类操作数解析为常量


class Reader:
    def __init__(self, b, pos=0):
        self.b, self.p = b, pos

    def byte(self):
        v = self.b[self.p]
        self.p += 1
        return v

    def u32(self):
        v = struct.unpack_from("<I", self.b, self.p)[0]
        self.p += 4
        return v

    def i32(self):
        v = struct.unpack_from("<i", self.b, self.p)[0]
        self.p += 4
        return v

    def u64(self):
        v = struct.unpack_from("<Q", self.b, self.p)[0]
        self.p += 8
        return v

    def dbl(self):
        v = struct.unpack_from("<d", self.b, self.p)[0]
        self.p += 8
        return v

    def string(self):
        n = self.byte()
        if n == 0xFF:
            n = self.u64()
        if n == 0:
            return None
        s = self.b[self.p:self.p + n - 1]
        self.p += n - 1
        return s.decode("utf-8", "replace")


class Proto:
    pass


def load_proto(r):
    p = Proto()
    p.source = r.string()
    p.linedefined = r.i32()
    p.lastlinedefined = r.i32()
    p.numparams = r.byte()
    p.is_vararg = r.byte()
    p.maxstacksize = r.byte()
    n = r.i32()
    p.code = [r.u32() for _ in range(n)]
    nk = r.i32()
    p.k = []
    for _ in range(nk):
        t = r.byte()
        if t == 0:
            p.k.append(None)
        elif t == 1:
            p.k.append(bool(r.byte()))
        elif t == 3:
            p.k.append(r.dbl())
        elif t == 19:
            p.k.append(r.u64())
        elif t == 4:
            p.k.append(r.string())
        elif t == 20:
            p.k.append(r.string())
        else:
            raise ValueError("bad constant tag %d @%x" % (t, r.p))
    nu = r.i32()
    p.upvalues = [(r.byte(), r.byte()) for _ in range(nu)]
    np = r.i32()
    p.protos = [load_proto(r) for _ in range(np)]
    # debug
    nl = r.i32()
    p.lineinfo = [r.i32() for _ in range(nl)]
    nlv = r.i32()
    for _ in range(nlv):
        r.string(); r.i32(); r.i32()
    nup = r.i32()
    for _ in range(nup):
        r.string()
    return p


def parse(path):
    b = open(path, "rb").read()
    r = Reader(b, 0)
    assert b[:4] == b"\x1bLua", "not lua chunk"
    r.p = 4
    ver = r.byte()
    fmt = r.byte()
    r.p += 6  # LUAC_DATA
    sizes = list(b[12:16])
    r.p = 16
    luac_int = r.u64()
    luac_num = r.dbl()
    nups = r.byte()  # sizeupvalues of main chunk
    p = load_proto(r)
    p.main_nups = nups
    p.consumed, p.total, p.sizes = r.p, len(b), sizes
    return p


def const_str(p, idx):
    if idx < len(p.k):
        v = p.k[idx]
        if isinstance(v, str):
            return '"%s"' % v
        return repr(v)
    return "K%d?" % idx


def rk(p, x, fields=None):
    return const_str(p, x & 0xFF) if x & 0x100 else "R%d" % x


def disasm(p, depth=0, out=None, parent=None):
    if out is None:
        out = []
    ind = "  " * depth
    hdr = "proto #%d" % (parent.count) if parent else "main"
    out.append("%sfunction %s @%s:%d-%d params=%d vararg=%d stack=%d nups=%d" %
               (ind, hdr, p.source, p.linedefined, p.lastlinedefined,
                p.numparams, p.is_vararg, p.maxstacksize, len(p.upvalues)))
    for i, ins in enumerate(p.code):
        op = ins & 0x3F
        name = OPNAMES[op] if op < len(OPNAMES) else "OP%d" % op
        line = p.lineinfo[i] if i < len(p.lineinfo) else -1
        if name == "LOADK":
            ax, bx = (ins >> 6) & 0xFF, (ins >> 14) & 0x3FFFF
            out.append("%s  [%3d] %-9s %-4s %-4s %s" % (ind, i, name, "R%d" % ax, "", const_str(p, bx)))
        elif name in ("LOADBOOL", "LOADNIL", "GETUPVAL", "SETUPVAL", "GETTABUP", "SETTABUP",
                      "GETTABLE", "SETTABLE", "NEWTABLE", "SELF", "ADD", "SUB", "MUL", "MOD",
                      "POW", "DIV", "IDIV", "BAND", "BOR", "BXOR", "SHL", "SHR", "BNOT", "NOT",
                      "LEN", "CONCAT", "EQ", "LT", "LE", "TEST", "TESTSET", "CALL", "TAILCALL",
                      "RETURN", "TFORCALL", "MOVE", "VARARG", "SETLIST"):
            a = (ins >> 6) & 0xFF
            c = (ins >> 14) & 0x1FF
            bb = (ins >> 23) & 0x1FF
            extra = ""
            if name == "GETTABUP":
                extra = "U%d %s" % (bb, rk(p, c))
            elif name == "SETTABUP":
                extra = "U%d %s %s" % (a, rk(p, bb), rk(p, c))
            elif name in ("GETUPVAL", "SETUPVAL"):
                extra = "U%d" % bb
            elif name in ("GETTABLE", "SELF"):
                extra = "%s %s" % ("R%d" % bb, rk(p, c))
            elif name == "SETTABLE":
                extra = "%s %s %s" % ("R%d" % a, rk(p, bb), rk(p, c))
            elif name in ("ADD", "SUB", "MUL", "MOD", "POW", "DIV", "IDIV", "BAND", "BOR", "BXOR", "SHL", "SHR"):
                extra = "%s %s" % (rk(p, bb), rk(p, c))
            else:
                extra = "%d %d" % (bb, c)
            out.append("%s  [%3d] %-9s %-4s %s" % (ind, i, name, "R%d" % a, extra))
        elif name == "CLOSURE":
            ax, bx = (ins >> 6) & 0xFF, (ins >> 14) & 0x3FFFF
            hint = ""
            for j in range(max(0, i - 4), i):
                w = p.code[j]
                if (w & 0x3F) == 1:  # LOADK
                    kx = (w >> 14) & 0x3FFFF
                    if kx < len(p.k) and isinstance(p.k[kx], str):
                        hint = " (name=%s)" % p.k[kx]
                        break
            out.append("%s  [%3d] %-9s %-4s -> proto#%d %s%s" % (ind, i, name, "R%d" % ax, bx,
                       p.protos[bx].source if bx < len(p.protos) else "?", hint))
        else:
            s = (ins >> 14) & 0x3FFFF
            s -= 131071  # GETARG_sBx = Bx - MAXARG_sBx
            a = (ins >> 6) & 0xFF
            out.append("%s  [%3d] %-9s %-4s %+d -> %d" % (ind, i, name, "R%d" % a, s, i + 1 + s))
    for j, sp in enumerate(p.protos):
        out.append("%s-- 子函数 %d (%s @%d)" % (ind, j, sp.source, sp.linedefined))
        disasm(sp, depth + 1, out, type("P", (), {"count": j}))
    return out


if __name__ == "__main__":
    p = parse(sys.argv[1])
    print("consumed=%d total=%d sizes=%s nups=%d k=%d code=%d protos=%d" %
          (p.consumed, p.total, p.sizes, p.main_nups, len(p.k), len(p.code), len(p.protos)))
    for line in disasm(p):
        print(line)
