#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
绿梦：时空之声 — 热更 Dll bundle（UnityFS）解包 / 单对象替换 / 重打包

为什么需要它：游戏的业务代码在 HybridCLR 热更程序集 Assembly-ACT.dll 里，该 dll 由 YooAsset
以 TextAsset 形式装在 Dll 包的 bundle 内（`files/yoo/Dll/BundleFiles/<xx>/<guid>/__data`）。
要改游戏逻辑只能换掉 bundle 里那段 PE 字节。

## 实测格式（2026-10-16，游戏 1.0.5/vc12035）
- 头：`UnityFS\\0` + u32BE ver(8) + cstr "5.x.x" + cstr "2022.3.62f2" + i64BE totalSize
      + u32BE compressedBlocksInfoSize + u32BE uncompressedBlocksInfoSize + u32BE flags
      本例 totalSize=13165811 cbi=1078 ubi=2321 flags=0x243（低 6 位 3=LZ4HC，0x40=BlocksAndDirCombined）
- 头结束后 **align(16) → 64**，其处取 cbi 字节，`lz4.block.decompress(raw, ubi)` 得 blocksInfo：
      16B uncompressedDataHash + i32BE nblocks + nblocks*(u32BE u, u32BE c, u16BE flags)
      + i32BE nnodes + nnodes*(i64BE off, i64BE size, u32BE flags, cstr path)
- 数据区起点 = align(64+cbi, 16)。每块 `lz4.block.decompress(raw, u)`。

## 必须保留的坑
索引 0 那块（文件偏移 1152..62348，61196 B）**不是合法 LZ4 流**（liblz4 返回 -14），
索引 1..223 全部正常。游戏自己能跑 ⇒ 索引 0 被某种方式变换过（未查明）。
故本工具把**索引 0 的原始字节当作不透明数据原样保留**（内容、位置、compressedSize 都不动），
只解码/重编码索引 1..223。替换目标 Assembly-ACT.dll 位于解压流偏移 649120、长度 14529536，
完全落在索引 >=1 的块内，所以不受该坑影响。

用法：
  inspect        <bundle>
  extract-stream <bundle> <out.bin>
  locate-pe      <bundle>                     # 枚举解压流里所有 PE 程序集(名/偏移/长度)
  replace        <bundle> <in.bin> <out.bundle> --offset N --size S
  write-info     <data文件> <__info输出路径>
  selftest       <bundle>
"""
import argparse
import struct
import sys
import zlib

import lz4.block

OPAQUE_BLOCK = 0          # 索引 0：不可解码，原样保留
HASH_LEN = 16


def _rstr(d, o):
    e = d.index(b"\0", o)
    return d[o:e].decode("utf-8", "replace"), e + 1


def parse_header(d):
    if d[:8] != b"UnityFS\0":
        raise SystemExit("不是 UnityFS bundle（头签名不符）")
    off = 8
    ver = struct.unpack(">I", d[off:off + 4])[0]
    off += 4
    uv, off = _rstr(d, off)
    ur, off = _rstr(d, off)
    total = struct.unpack(">q", d[off:off + 8])[0]
    off += 8
    cbi, ubi, flags = struct.unpack(">III", d[off:off + 12])
    off += 12
    return dict(ver=ver, uv=uv, ur=ur, total=total, cbi=cbi, ubi=ubi, flags=flags, hdr_end=off)


def parse_blocks(d, h):
    bioff = (h["hdr_end"] + 15) & ~15
    bi = lz4.block.decompress(d[bioff:bioff + h["cbi"]], h["ubi"])
    p = 0
    hash16 = bi[p:p + HASH_LEN]
    p += HASH_LEN
    nb = struct.unpack(">i", bi[p:p + 4])[0]
    p += 4
    blocks = []
    for _ in range(nb):
        u, c, f = struct.unpack(">IIH", bi[p:p + 10])
        p += 10
        blocks.append([u, c, f])
    nn = struct.unpack(">i", bi[p:p + 4])[0]
    p += 4
    nodes = []
    for _ in range(nn):
        o, s, fl = struct.unpack(">qqI", bi[p:p + 20])
        p += 20
        e = bi.index(b"\0", p)
        nodes.append((o, s, fl, bi[p:e].decode("utf-8", "replace")))
        p = e + 1
    return dict(bioff=bioff, hash16=hash16, blocks=blocks, nodes=nodes,
                data_start=(bioff + h["cbi"] + 15) & ~15, bi_tail=bi[p:])


def decompress_block(raw, u, f, index):
    """索引 0 原样返回（不可解码）；其余按 flags 低 6 位解压。"""
    if index == OPAQUE_BLOCK:
        return raw
    cf = f & 0x3F
    if cf == 3:
        return lz4.block.decompress(raw, u)
    if cf == 2:
        return lz4.block.decompress(raw, u)
    if cf == 1:
        import lzma
        return lzma.decompress(raw)
    if cf == 0:
        return raw
    raise SystemExit("未知压缩标志 flags=%#x (block %d)" % (f, index))


def split_raw(d, h, bi):
    """按块表把数据区切成 list[raw bytes]（不做解压）。"""
    pos = bi["data_start"]
    out = []
    for u, c, f in bi["blocks"]:
        out.append(d[pos:pos + c])
        pos += c
    return out, pos


def decode_stream(raws, blocks):
    """拼解压流。索引 0 的解压内容不可得，用零填充占位以保持后续块偏移正确；
    重编码时索引 0 会换回原始不透明字节（见 encode_bundle），故占位内容永不被使用。"""
    parts = []
    for i, (u, c, f) in enumerate(blocks):
        if i == OPAQUE_BLOCK:
            parts.append(raws[i].ljust(u, b"\0"))
            continue
        parts.append(decompress_block(raws[i], u, f, i))
    return b"".join(parts)


def build_blocksinfo(blocks, nodes, hash16):
    out = bytearray(hash16)
    out += struct.pack(">i", len(blocks))
    for u, c, f in blocks:
        out += struct.pack(">IIH", u, c, f)
    out += struct.pack(">i", len(nodes))
    for o, s, fl, path in nodes:
        out += struct.pack(">qqI", o, s, fl)
        out += path.encode("utf-8") + b"\0"
    return bytes(out)


def encode_bundle(h, blocks, nodes, hash16, raws):
    """raws[i] = 第 i 块的**未压缩**内容（索引 0 为原始不透明字节）。"""
    new_blocks = []
    for i, (u, c, f) in enumerate(blocks):
        if i == OPAQUE_BLOCK:
            # 不透明块：内容与 compressedSize 都保持原样
            new_blocks.append([u, len(raws[i]), f])
        else:
            comp = lz4.block.compress(raws[i], mode="high_compression", store_size=False)
            new_blocks.append([u, len(comp), f & 0x3F | (f & 0xC0)])
    bi = build_blocksinfo(new_blocks, nodes, hash16)
    bi_c = lz4.block.compress(bi, mode="high_compression", store_size=False)
    hdr_end = 8 + 4 + len(h["uv"].encode()) + 1 + len(h["ur"].encode()) + 1 + 8 + 12
    bioff = (hdr_end + 15) & ~15
    data_start = (bioff + len(bi_c) + 15) & ~15
    body = bytearray()
    for i, (u, c, f) in enumerate(new_blocks):
        if i == OPAQUE_BLOCK:
            body += raws[i]
        else:
            body += lz4.block.compress(raws[i], mode="high_compression", store_size=False)
    total = data_start + len(body)
    head = bytearray()
    head += b"UnityFS\0"
    head += struct.pack(">I", h["ver"])
    head += h["uv"].encode() + b"\0"
    head += h["ur"].encode() + b"\0"
    head += struct.pack(">q", total)
    head += struct.pack(">III", len(bi_c), len(bi), h["flags"])
    out = bytearray(head)
    out += b"\0" * (bioff - len(out))
    out += bi_c
    out += b"\0" * (data_start - len(out))
    out += body
    assert len(out) == total, (len(out), total)
    return bytes(out), new_blocks, len(bi), len(bi_c)


def load(path):
    with open(path, "rb") as f:
        d = f.read()
    h = parse_header(d)
    bi = parse_blocks(d, h)
    raws, end = split_raw(d, h, bi)
    return d, h, bi, raws, end


def cmd_inspect(a):
    d, h, bi, raws, end = load(a.bundle)
    print("header:", h)
    print("node:", bi["nodes"])
    print("bioff=%d data_start=%d 块数=%d 数据区结束=%d 文件长=%d" %
          (bi["bioff"], bi["data_start"], len(bi["blocks"]), end, len(d)))
    bad = 0
    for i, (u, c, f) in enumerate(bi["blocks"]):
        tag = ""
        if i == OPAQUE_BLOCK:
            # 索引 0 不可解是**已知且预期**的（原样保留、不参与重编码）
            bad += 1
            tag = "  <不可解(预期)>"
            print("  block %3d: u=%d c=%d flags=%#x  [opaque 原样保留]%s" % (i, u, c, f, tag))
            continue
        try:
            got = len(decompress_block(raws[i], u, f, i))
            if got != u:
                tag = "  <长度不符 %d!=%d>" % (got, u)
        except Exception as e:
            tag = "  <解压失败 %s>" % type(e).__name__
            bad += 1
        if i < 4 or tag or i == len(bi["blocks"]) - 1:
            print("  block %3d: u=%d c=%d flags=%#x%s%s" %
                  (i, u, c, f, "  [opaque 原样保留]" if i == OPAQUE_BLOCK else "", tag))
    print("解压失败块数 = %d（索引 0 预期不可解 ⇒ 正常应为 1）" % bad)
    return 0


def cmd_extract_stream(a):
    d, h, bi, raws, end = load(a.bundle)
    stream = decode_stream(raws, bi["blocks"])
    with open(a.out, "wb") as f:
        f.write(stream)
    print("解压流 %d 字节 → %s" % (len(stream), a.out))
    print("注意：偏移 0..%d 为索引 0 原始字节（不可信，未解压）" % len(raws[OPAQUE_BLOCK]),
          file=sys.stderr)
    print("node 声明 size = %d，一致=%s" % (bi["nodes"][0][1], len(stream) == bi["nodes"][0][1]))
    return 0


def cmd_locate_pe(a):
    d, h, bi, raws, end = load(a.bundle)
    stream = decode_stream(raws, bi["blocks"])
    import tempfile
    import os
    try:
        import dnfile
    except ImportError:
        dnfile = None
    n = 0
    for mz in _find_mz(stream):
        total = _pe_total(stream, mz)
        if not total:
            continue
        name = "?"
        if dnfile is not None:
            tf = tempfile.NamedTemporaryFile(suffix=".dll", delete=False)
            tf.write(stream[mz:mz + total])
            tf.close()
            try:
                pe = dnfile.dnPE(tf.name)
                md = pe.net.mdtables
                if md and md.Assembly and md.Assembly.rows:
                    name = str(md.Assembly.rows[0].Name)
            except Exception:
                name = "ERR"
            os.unlink(tf.name)
        print("off=%9d size=%9d %s" % (mz, total, name))
        n += 1
    print("-- %d 个 PE" % n)
    return 0


def _find_mz(d):
    i = 0
    while True:
        j = d.find(b"MZ", i)
        if j < 0:
            return
        i = j + 1
        yield j


def _pe_total(d, mz):
    import struct as _s
    try:
        e = _s.unpack("<I", d[mz + 0x3C:mz + 0x40])[0]
        if not (0x40 <= e <= 0x1000):
            return None
        pe = mz + e
        if d[pe:pe + 4] != b"PE\0\0":
            return None
        nsec = _s.unpack("<H", d[pe + 6:pe + 8])[0]
        if not (1 <= nsec <= 16):
            return None
        optsz = _s.unpack("<H", d[pe + 20:pe + 22])[0]
        magic = _s.unpack("<H", d[pe + 24:pe + 26])[0]
        if magic not in (0x10B, 0x20B):
            return None
        secoff = pe + 24 + optsz
        end = 0
        for s in range(nsec):
            so = secoff + 40 * s
            praw = _s.unpack("<I", d[so + 20:so + 24])[0]
            sraw = _s.unpack("<I", d[so + 16:so + 20])[0]
            end = max(end, praw + sraw)
        return end if 1024 < end < 64 * 1024 * 1024 else None
    except Exception:
        return None


def cmd_replace(a):
    d, h, bi, raws, end = load(a.bundle)
    with open(a.infile, "rb") as f:
        repl = f.read()
    if len(repl) != a.size:
        raise SystemExit("替换内容长度 %d != --size %d（必须等长，否则 SerializedFile 对象表失效）"
                         % (len(repl), a.size))
    stream = bytearray(decode_stream(raws, bi["blocks"]))
    if a.offset + a.size > len(stream):
        raise SystemExit("替换区间越界")
    # 不允许改到不透明块（索引 0）所在区间（按未压缩长度算边界）
    opaque_len = bi["blocks"][OPAQUE_BLOCK][0]
    if a.offset < opaque_len:
        raise SystemExit("替换区间落在不透明块 [0,%d) 内，禁止" % opaque_len)
    stream[a.offset:a.offset + a.size] = repl
    # 重新切块：索引 0 原样，其余按块表长度切
    new_raws = []
    p = 0
    for i, (u, c, f) in enumerate(bi["blocks"]):
        if i == OPAQUE_BLOCK:
            new_raws.append(raws[i])
        else:
            new_raws.append(bytes(stream[p:p + u]))
        p += u
    assert p == len(stream), (p, len(stream))
    out, nb, ubi2, cbi2 = encode_bundle(h, bi["blocks"], bi["nodes"], bi["hash16"], new_raws)
    with open(a.out, "wb") as f:
        f.write(out)
    print("旧 bundle %d B → 新 bundle %d B（cbi %d→%d, ubi %d→%d）" %
          (len(d), len(out), h["cbi"], cbi2, h["ubi"], ubi2))
    print("CRC 字符串: 旧=%s 新=%s" % (crc_str(d), crc_str(out)))
    return 0


def crc_str(data):
    return struct.pack("<I", zlib.crc32(data) & 0xFFFFFFFF).hex()


def cmd_write_info(a):
    with open(a.data, "rb") as f:
        d = f.read()
    c = crc_str(d).encode("ascii")
    blob = struct.pack("<H", len(c)) + c + struct.pack("<q", len(d))
    with open(a.out, "wb") as f:
        f.write(blob)
    print("__info = %d B  crc=%s size=%d → %s" % (len(blob), c.decode(), len(d), a.out))
    return 0


def cmd_selftest(a):
    d, h, bi, raws, end = load(a.bundle)
    stream = decode_stream(raws, bi["blocks"])
    print("① 解码 OK：流 %d B，node size %d" % (len(stream), bi["nodes"][0][1]))
    assert len(stream) == bi["nodes"][0][1]
    new_raws = [raws[OPAQUE_BLOCK]]
    p = bi["blocks"][OPAQUE_BLOCK][0]
    for i in range(1, len(bi["blocks"])):
        u = bi["blocks"][i][0]
        new_raws.append(bytes(stream[p:p + u]))
        p += u
    assert p == len(stream)
    out, nb, ubi2, cbi2 = encode_bundle(h, bi["blocks"], bi["nodes"], bi["hash16"], new_raws)
    print("② 重打包 OK：%d B → %d B（cbi %d→%d）" % (len(d), len(out), h["cbi"], cbi2))
    d2, h2, bi2, raws2, end2 = load_from_bytes(out)
    stream2 = decode_stream(raws2, bi2["blocks"])
    print("③ 再解码 OK：流2 %d B" % len(stream2))
    assert stream2 == stream, "解压流不一致！"
    assert raws2[OPAQUE_BLOCK] == raws[OPAQUE_BLOCK], "不透明块字节变了！"
    assert len(bi2["blocks"]) == len(bi["blocks"])
    for i, (u, c, f) in enumerate(bi["blocks"]):
        if i == OPAQUE_BLOCK:
            assert bi2["blocks"][i][1] == c, "不透明块 compressedSize 变了"
    print("④ 逐字节断言全过：解压流一致、不透明块原样、块数 %d 一致" % len(bi["blocks"]))
    print("   CRC: %s → %s" % (crc_str(d), crc_str(out)))
    return 0


def load_from_bytes(d):
    h = parse_header(d)
    bi = parse_blocks(d, h)
    raws, end = split_raw(d, h, bi)
    return d, h, bi, raws, end


def main():
    ap = argparse.ArgumentParser(description="绿梦 Dll bundle 解包/替换/重打包")
    sub = ap.add_subparsers(dest="cmd", required=True)

    p = sub.add_parser("inspect")
    p.add_argument("bundle")
    p.set_defaults(fn=cmd_inspect)

    p = sub.add_parser("extract-stream")
    p.add_argument("bundle")
    p.add_argument("out")
    p.set_defaults(fn=cmd_extract_stream)

    p = sub.add_parser("locate-pe")
    p.add_argument("bundle")
    p.set_defaults(fn=cmd_locate_pe)

    p = sub.add_parser("replace")
    p.add_argument("bundle")
    p.add_argument("infile")
    p.add_argument("out")
    p.add_argument("--offset", type=int, required=True)
    p.add_argument("--size", type=int, required=True)
    p.set_defaults(fn=cmd_replace)

    p = sub.add_parser("write-info")
    p.add_argument("data")
    p.add_argument("out")
    p.set_defaults(fn=cmd_write_info)

    p = sub.add_parser("selftest")
    p.add_argument("bundle")
    p.set_defaults(fn=cmd_selftest)

    a = ap.parse_args()
    return a.fn(a)


if __name__ == "__main__":
    sys.exit(main())
