#!/usr/bin/env python3.11
# 解剖 typeDefs 尾部 12156-12625（dump.cs 缺失段）: 字段范围 + names 解密
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = open(P, "rb").read()
FS = len(d)
u32 = lambda o: struct.unpack_from("<I", d, o)[0]
i32 = lambda o: struct.unpack_from("<i", d, o)[0]
u16 = lambda o: struct.unpack_from("<H", d, o)[0]

TD, NTYPES = 0x8AFD34, 12626
IM = 0x9BF164
NB, NAMES_SZ = 0xA23B8, 0x2227DC - 0xA23B8
ME, MESZ = 0x278A30, 98644 * 32
FD, FDSZ = 0x740AE8, 55037 * 12
NVTM = 531552 // 8          # vtableMethods count (gapB)
NIF = 21624 // 4            # interfaces
NIO = 103528 // 8           # interfaceOffsets

def name_at(off):
    if not (0 <= off < NAMES_SZ):
        return "<OOB:0x%x>" % off
    e = d.index(b"\x00", NB + off)
    raw = d[NB + off:e]
    k = (off & 0xFF) | 0x80
    dec = bytes((b ^ k) & 0xFF for b in raw)
    # 文件里已解密过的 OUT 是解密后版本; 若 OUT 本身已解密则直接读
    return raw, dec

def td(i):
    b = TD + i * 88
    return dict(
        name=i32(b), ns=i32(b + 4), byval=i32(b + 8),
        decl=i32(b + 12), parent=i32(b + 16), elem=i32(b + 20),
        gcontainer=i32(b + 24), flags=u32(b + 28),
        fieldStart=i32(b + 32), methodStart=i32(b + 36),
        eventStart=i32(b + 40), propStart=i32(b + 44),
        nestedStart=i32(b + 48), ifaceStart=i32(b + 52),
        vtStart=i32(b + 56), ifoStart=i32(b + 60),
        m_cnt=u16(b + 64), p_cnt=u16(b + 66), f_cnt=u16(b + 68), e_cnt=u16(b + 70),
        n_cnt=u16(b + 72), v_cnt=u16(b + 74), i_cnt=u16(b + 76), io_cnt=u16(b + 78),
        bitfield=u32(b + 80), token=u32(b + 84))

# imageDef 尾部 3 个
print("== imageDefs[99..101] ==")
for k in range(99, 102):
    b = IM + k * 40
    print(k, dict(name=u32(b), asm=u32(b + 4), typeStart=u32(b + 8), typeCount=u32(b + 12),
                  expStart=u32(b + 16), expCnt=u32(b + 20), entry=u32(b + 24),
                  token=u32(b + 28), caStart=u32(b + 32), caCnt=u32(b + 36)))

# names 解密: OUT 文件 names 区是否已解密? 先试解密, 再试原文
raw102, dec102 = name_at(u32(TD + 10155 * 88))     # type 12155 tk2dSystem 名字(已知好)
print("\n对照 type12155 name raw=%r dec=%r" % (raw102[:20], dec102[:20]))

def getname(off):
    if not (0 <= off < NAMES_SZ):
        return "<OOB>"
    e = NB + off
    while e < NB + NAMES_SZ and d[e] != 0:
        e += 1
    raw = d[NB + off:e]
    k = (off & 0xFF) | 0x80
    dec = bytes((b ^ k) & 0xFF for b in raw)
    printable = sum(1 for c in dec if 32 <= c < 127) / max(len(dec), 1)
    # 若解密结果不可读而原文可读, 则 OUT 的 names 已解密
    praw = sum(1 for c in raw if 32 <= c < 127) / max(len(raw), 1)
    return (dec if printable >= praw else raw).decode("utf-8", "replace")

print("\n== 对照样本 type12150-12155 ==")
for i in range(12150, 12156):
    t = td(i)
    print(i, hex(t["flags"]), "mStart=%d mCnt=%d fCnt=%d vCnt=%d" % (t["methodStart"], t["m_cnt"], t["f_cnt"], t["v_cnt"]), repr(getname(t["name"])))

print("\n== 尾部 typeDefs 12156-12625 ==")
bad = 0
samples = []
for i in range(12156, NTYPES):
    t = td(i)
    ok = (0 <= t["name"] < NAMES_SZ and
          0 <= t["methodStart"] + t["m_cnt"] <= 98644 and
          0 <= t["fieldStart"] + t["f_cnt"] <= 55037 and
          0 <= t["vtStart"] + t["v_cnt"] <= NVTM and
          0 <= t["ifaceStart"] + t["i_cnt"] <= NIF)
    if not ok:
        bad += 1
        if len(samples) < 10:
            samples.append((i, t))
    elif len(samples) < 40 and i < 12200 or (i % 500 == 0):
        samples.append((i, t, getname(t["name"])))
print("字段越界类型数: %d / %d" % (bad, NTYPES - 12156))
for s in samples:
    if len(s) == 3:
        i, t, nm = s
        print(i, "flags=%08x" % t["flags"], "mS=%d mC=%d fC=%d vC=%d ns=%d" % (t["methodStart"], t["m_cnt"], t["f_cnt"], t["v_cnt"], t["ns"]), repr(nm))
    else:
        i, t = s
        print(i, "BAD", {k: t[k] for k in ("name", "ns", "byval", "parent", "flags", "methodStart", "m_cnt", "f_cnt", "vtStart", "v_cnt", "token")})
