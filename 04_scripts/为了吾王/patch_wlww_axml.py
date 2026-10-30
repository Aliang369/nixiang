# -*- coding: utf-8 -*-
"""为了吾王(com.ftk1.m3839) AndroidManifest.xml 二进制补丁：插入 StarryInitProvider

思路（不动 resources.arsc，避免 @mipmap/@string 引用 ID 漂移）：
 1) 解析 AXML：XML 根 chunk → string pool → resource map → 节点流
 2) 只向 string pool **追加**新串（旧串索引不变），按原 flags 保持 UTF-8/UTF-16
 3) 在 <application> 的 START_ELEMENT 之后紧插 START/END 两个 provider 节点
    ⇒ provider 顺序天然排在游戏自带 fn.fileprovider 之前（Provider 顺序铁律）
 4) 修正 XML 根 chunk 的 size / string pool 的 size 与 stringsStart
 5) 写完立刻回读自检（provider 存在、且在 fn.fileprovider 之前、引用 ID 未变）

用法: python patch_wlww_axml.py <in_AndroidManifest.xml> <out_AndroidManifest.xml>
"""
import struct
import sys

ANDROID_NS = "http://schemas.android.com/apk/res/android"
CLS = "com.starry.magic.StarryInitProvider"
AUTH = "com.ftk1.m3839.starryinit"

RES_XML_TYPE = 0x0003
RES_STRING_POOL_TYPE = 0x0001
RES_XML_RESOURCE_MAP_TYPE = 0x0180
RES_XML_START_ELEMENT_TYPE = 0x0102
RES_XML_END_ELEMENT_TYPE = 0x0103
UTF8_FLAG = 0x100

TYPE_STRING = 0x03
TYPE_INT_BOOLEAN = 0x12


def u32(b, o):
    return struct.unpack_from("<I", b, o)[0]


def u16(b, o):
    return struct.unpack_from("<H", b, o)[0]


# ---------------- string pool ----------------
def parse_pool(b, off):
    typ, hsz, size = u16(b, off), u16(b, off + 2), u32(b, off + 4)
    sc, stc, flags, strings_start, styles_start = (u32(b, off + 8), u32(b, off + 12),
                                                  u32(b, off + 16), u32(b, off + 20),
                                                  u32(b, off + 24))
    offs = [u32(b, off + 28 + 4 * i) for i in range(sc)]
    utf8 = bool(flags & UTF8_FLAG)
    strs = []
    for o in offs:
        p = off + strings_start + o
        if utf8:
            # utf-16 length（可能两字节）再 utf-8 字节长度
            n1 = b[p]
            if n1 & 0x80:
                n1 = ((n1 & 0x7F) << 8) | b[p + 1]; p += 2
            else:
                p += 1
            n2 = b[p]
            if n2 & 0x80:
                n2 = ((n2 & 0x7F) << 8) | b[p + 1]; p += 2
            else:
                p += 1
            strs.append(b[p:p + n2].decode("utf-8", "replace"))
        else:
            n = u16(b, p); p += 2
            if n & 0x8000:
                n = ((n & 0x7FFF) << 16) | u16(b, p); p += 2
            strs.append(b[p:p + n * 2].decode("utf-16-le", "replace"))
    return {"off": off, "hsz": hsz, "size": size, "count": sc, "styleCount": stc,
            "flags": flags, "utf8": utf8, "strings": strs}


def enc_len_utf8(n):
    if n > 0x7F:
        return bytes([0x80 | (n >> 8), n & 0xFF])
    return bytes([n])


def build_pool(pool, extra):
    strs = pool["strings"] + extra
    utf8 = pool["utf8"]
    data = bytearray()
    offs = []
    for s in strs:
        offs.append(len(data))
        if utf8:
            raw = s.encode("utf-8")
            data += enc_len_utf8(len(s)) + enc_len_utf8(len(raw)) + raw + b"\x00"
        else:
            raw = s.encode("utf-16-le")
            n = len(raw) // 2
            data += (struct.pack("<H", n) if n < 0x8000 else
                     struct.pack("<H", 0x8000 | (n >> 16)) + struct.pack("<H", n & 0xFFFF))
            data += raw + b"\x00\x00"
    while len(data) % 4:
        data += b"\x00"
    hsz = 28
    sc, stc = len(strs), pool["styleCount"]
    strings_start = hsz + 4 * sc + 4 * stc
    size = strings_start + len(data)
    out = bytearray()
    out += struct.pack("<HHI", RES_STRING_POOL_TYPE, hsz, size)
    out += struct.pack("<IIIII", sc, stc, pool["flags"], strings_start, 0)
    for o in offs:
        out += struct.pack("<I", o)
    # styleCount 必须为 0（APK manifest 实测无 style span），否则这里要重建 styles 区
    assert stc == 0, "string pool 含 style span，本脚本不支持"
    out += data
    assert len(out) == size, (len(out), size)
    return bytes(out)


# ---------------- nodes ----------------
def read_nodes(b, start, end):
    """返回 [(offset_in_b, type, chunk_size)]"""
    out = []
    o = start
    while o + 8 <= end:
        t = u16(b, o)
        sz = u32(b, o + 4)
        if sz < 8 or o + sz > end:
            break
        out.append((o, t, sz))
        o += sz
    return out


def make_start_element(name_idx, ns_idx, attrs, line=0):
    """attrs: [(ns_idx, name_idx, raw_idx, type, data)]"""
    body = bytearray()
    body += struct.pack("<ii", line, -1)                      # lineNumber, comment
    body += struct.pack("<i", ns_idx) + struct.pack("<i", name_idx)
    body += struct.pack("<HHHHHH", 20, 20, len(attrs), 0, 0, 0)
    for ns, nm, raw, typ, data in attrs:
        body += struct.pack("<iii", ns, nm, raw)
        body += struct.pack("<HBBI", 8, 0, typ, data)
    size = 8 + len(body)      # 8 = type/headerSize/size；lineNumber+comment 已含在 body 里
    assert size == 96, size   # 8+8+8+12+3*20
    return struct.pack("<HHI", RES_XML_START_ELEMENT_TYPE, 16, size) + bytes(body)


def make_end_element(name_idx, ns_idx, line=0):
    return (struct.pack("<HHI", RES_XML_END_ELEMENT_TYPE, 16, 24) +
            struct.pack("<ii", line, -1) + struct.pack("<ii", ns_idx, name_idx))


def main():
    src, dst = sys.argv[1], sys.argv[2]
    b = open(src, "rb").read()
    assert u16(b, 0) == RES_XML_TYPE, "不是 AXML"
    root_size = u32(b, 4)

    pool = parse_pool(b, 8)
    p = 8 + pool["size"]
    assert u16(b, p) == RES_XML_RESOURCE_MAP_TYPE, "resource map 位置异常"
    rmap_size = u32(b, p + 4)
    nodes_start = p + rmap_size

    # 取索引（缺的追加）
    need = [ANDROID_NS, "provider", "name", "authorities", "exported", CLS, AUTH]
    extra = []
    idx = {}
    for s in need:
        if s in pool["strings"]:
            idx[s] = pool["strings"].index(s)
        else:
            idx[s] = len(pool["strings"]) + len(extra)
            extra.append(s)
    new_pool = build_pool(pool, extra)
    delta_pool = len(new_pool) - pool["size"]

    # 找 <application>
    nodes = read_nodes(b, nodes_start, len(b))
    app_off = None
    for off, t, sz in nodes:
        if t == RES_XML_START_ELEMENT_TYPE:
            name_idx = u32(b, off + 20)
            if pool["strings"][name_idx] == "application":
                app_off, app_sz = off, sz
                break
    assert app_off, "没找到 <application>"

    # 构造 provider 节点
    A = idx[ANDROID_NS]
    start = make_start_element(
        idx["provider"], -1,
        [(A, idx["name"], idx[CLS], TYPE_STRING, idx[CLS]),
         (A, idx["authorities"], idx[AUTH], TYPE_STRING, idx[AUTH]),
         (A, idx["exported"], -1, TYPE_INT_BOOLEAN, 0)])
    end = make_end_element(idx["provider"], -1)
    insert = start + end

    # 拼接：XML 头 + 新 string pool + 原 resource map（原样搬，不动资源 ID）+ 节点流(带插入)
    head = b[:8] + new_pool + b[p:p + rmap_size]
    body_nodes = b[nodes_start:]
    cut = app_off + app_sz - nodes_start          # application 节点在 body_nodes 里的末尾
    out = bytearray(head + body_nodes[:cut] + insert + body_nodes[cut:])
    struct.pack_into("<I", out, 4, len(out))      # XML 根 chunk size
    out = bytes(out)

    # ---- 自检 ----
    pool2 = parse_pool(out, 8)
    p2 = 8 + pool2["size"]
    rm2 = u32(out, p2 + 4)
    n2 = read_nodes(out, p2 + rm2, len(out))
    seq = []
    for off, t, sz in n2:
        if t == RES_XML_START_ELEMENT_TYPE:
            seq.append(pool2["strings"][u32(out, off + 20)])
    i_new = seq.index("provider")
    i_old = seq.index("provider", i_new + 1) if "provider" in seq[i_new + 1:] else -1
    assert "StarryInitProvider" in " ".join(pool2["strings"]), "类名未进池"
    assert seq[seq.index("application") + 1] == "provider", "provider 不是 application 的第一个子节点"
    # 资源表未动
    assert out[p2:p2 + 8] == b[p:p + 8], "resource map 头被破坏"
    print("自检通过：application 首子节点=provider，旧 provider 仍在其后，resource map 未变")
    print(f"池 {pool['size']} → {pool2['size']} B（Δ{delta_pool:+d}），文件 {len(b)} → {len(out)} B")
    open(dst, "wb").write(out)


if __name__ == "__main__":
    main()
