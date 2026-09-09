#!/usr/bin/env python3.11
# .ZTG 方法体池平铺器: 标准 ECMA tiny/fat + MoreSects 子节, 验证 body↔MethodDef 映射
import struct, sys, re

sys.path.insert(0, "/Users/aliang/Documents/nixiang/04_scripts/月圆之夜")
import ztg_dump as z


def body_end(b, o, fsz):
    """返回 (end_offset, kind, code_size) 或 None

    实证格式 (0xb08 样本验证):
      small 节: Kind(1, 0x01=EHTable|0x80=MoreSects) + DataSize(1, 总字节数!) + pad(2)
      small 子句 12B: Flags(2) TryOff(2) TryLen(1) HandOff(2) HandLen(1) Token(4)
      fat 节: u32 = DataSize低24位 | MoreSects<<31, Kind|0x40 在低字节
    """
    h0 = b[o]
    if h0 & 3 == 2:                       # tiny
        sz = h0 >> 2
        if sz == 0 or o + 1 + sz > fsz:
            return None
        return o + 1 + sz, "tiny", sz
    if h0 & 3 == 3:                       # fat
        flags = h0 | (b[o + 1] << 8)
        hdr = (b[o + 1] >> 4) * 4
        if not (12 <= hdr <= 64):
            return None
        code = struct.unpack_from("<I", b, o + 4)[0]
        e = o + hdr + code
        if e > fsz:
            return None
        if flags & 0x08:                  # MoreSects
            e = (e + 3) & ~3
            while True:
                if e + 4 > fsz:
                    return None
                b0 = b[e]
                if b0 & 0x40:             # fat 节
                    v = struct.unpack_from("<I", b, e)[0]
                    sz = v & 0x00FFFFFF
                    more = (v >> 31) & 1
                else:                     # small 节: DataSize = 第2字节(总字节数)
                    sz = b[e + 1]
                    more = b0 & 0x80
                if sz < 4 or e + sz > fsz:
                    return None
                e += sz
                if not more:
                    break
                e = (e + 3) & ~3
        return e, "fat", code
    return None


def tile(b, start, end, fsz, maxn=None):
    o = start
    offs = []
    while o < end:
        if maxn and len(offs) >= maxn:
            break
        r = body_end(b, o, fsz)
        if r is None:
            break
        e, kind, sz = r
        if e > end:
            break
        offs.append((o, kind, sz))
        o = (e + 3) & ~3                  # 下一 body 4 字节对齐
    return offs, o


def main():
    path = sys.argv[1]
    m = z.Meta(path)
    b = m.b
    fsz = len(b)
    pool_end = m.st["#~"][0]
    print(f"{path.split('/')[-1]}: 文件 {hex(fsz)} 池尾(=#~) {hex(pool_end)}")
    # 找起始: 尝试若干起点, 取平铺最长
    best = (0, None, None)
    for start in range(0x60, 0x2000, 2):
        offs, o = tile(b, start, pool_end, fsz)
        if len(offs) > best[0]:
            best = (len(offs), start, o)
    print(f"最长平铺: {best[0]} 体, 起点 {hex(best[1])}, 止于 {hex(best[2])}")
    offs, o = tile(b, best[1], pool_end, fsz)
    if offs:
        print("前5:", [(hex(a), k, s) for a, k, s in offs[:5]])
        print("断点字节:", b[best[2]:best[2] + 16].hex(" "))
        # MethodDef rva!=0 的数量
        off, rs = m.tab[0x06]
        n = m.rows[0x06]
        storeds = [struct.unpack_from("<I", b, off + r * rs)[0] for r in range(n)]
        nz = [s for s in storeds if s & 0xFFFFFF]
        print(f"MethodDef {n}, rva非零 {len(nz)}, 平铺体 {len(offs)}")
        # 按 rid 顺序配对: body[k] <-> 第k个 rva!=0 的方法
        if len(nz) == len(offs):
            print("数量相等! 配对 diff(low24-bodyoff):")
            diffs = [(l & 0xFFFFFF) - a for l, (a, _, _) in zip(nz, offs)]
            print("  前12:", [hex(x) for x in diffs[:12]])
            import collections
            c = collections.Counter(diffs)
            print("  diff 唯一数:", len(c), " top3:", c.most_common(3))


if __name__ == "__main__":
    main()
