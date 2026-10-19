#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""fg_repack.py — 绿梦 FairGuard：明文 patch → re-deflate → 回填 blob 的往返验证器

背景（见 10_docs/绿梦/FairGuard逆壳_阶段1_解包器.md §19）：
  磁盘 0x458 起是标准 zlib 流，膨胀出 4,197,240 B = 虚址 0x458..0x400FD0 的线性镜像。
  所以"静态改包"= 改膨胀后的明文 → 重新 deflate → 回填 0x458 → 改 blob 头 +0x20(csize)。

用法:
  python3 fg_repack.py --patch 0x35D440=1f2003d5 --out /tmp/libFairGuard_patched.so
  python3 fg_repack.py --verify-only            # 只做原样往返，证明 de/inflate 可逆且头自洽

退出码: 0 通过；4 往返不一致；5 头字段不自洽；6 patch 越界
"""
import argparse, os, struct, sys, zlib

HDR, STREAM = 0x400, 0x458
SLACK_END = 0x403190   # 0x1F452E..0x403190 实测全零，可安全占用

def parse_hdr(disk):
    magic, _0, _1, _2, crc, _3, _4, usize = struct.unpack_from("<8I", disk, HDR)
    csize, _u24, end_va, _u2c, _u30 = struct.unpack_from("<5I", disk, HDR + 0x20)
    return dict(magic=magic, crc=crc, usize=usize, csize=csize, end_va=end_va)

def inflate(disk):
    d = zlib.decompressobj(15)
    out = d.decompress(disk[STREAM:]) + d.flush()
    used = len(disk) - STREAM - len(d.unused_data)
    return out, used, d.eof

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--disk", default="06_dumped/绿梦_20261016/libFairGuard.so")
    ap.add_argument("--patch", default="", help='逗号分隔 VA=HEX，如 0x35D440=1f2003d5')
    ap.add_argument("--out", default="/tmp/libFairGuard_patched.so")
    ap.add_argument("--level", type=int, default=9)
    ap.add_argument("--verify-only", action="store_true")
    a = ap.parse_args()

    disk = open(a.disk, "rb").read()
    h = parse_hdr(disk)
    print("[hdr] magic=%#x crc=%#x usize=%#x csize=%#x end_va=%#x"
          % (h["magic"], h["crc"], h["usize"], h["csize"], h["end_va"]))
    out, used, eof = inflate(disk)
    ok_hdr = (len(out) == h["usize"] and used == h["csize"]
              and STREAM + len(out) == h["end_va"] and eof)
    print("[inflate] %d B (usize %#x) stream %#x..%#x (csize %#x) eof=%s 头自洽=%s"
          % (len(out), h["usize"], STREAM, STREAM + used, used, eof, ok_hdr))
    if not ok_hdr:
        print("[FAIL] 原文件头字段不自洽，拒绝继续")
        return 5

    patched = bytearray(out)
    if not a.verify_only:
        for spec in [s for s in a.patch.split(",") if s]:
            va_s, hex_s = spec.split("=")
            va = int(va_s, 16)
            blob = bytes.fromhex(hex_s)
            i = va - STREAM
            if not (0 <= i and i + len(blob) <= len(patched)):
                print("[FAIL] patch %s 越界（合法 va 范围 %#x..%#x）"
                      % (spec, STREAM, STREAM + len(patched)))
                return 6
            old = bytes(patched[i:i + len(blob)])
            patched[i:i + len(blob)] = blob
            print("[patch] va %#x: %s -> %s" % (va, old.hex(), blob.hex()))

    comp = zlib.compressobj(a.level, zlib.DEFLATED, 15)
    new_stream = comp.compress(bytes(patched)) + comp.flush()
    print("[deflate] level=%d -> %d B（原 %d B，差 %+d）" % (a.level, len(new_stream), h["csize"], len(new_stream) - h["csize"]))
    if STREAM + len(new_stream) > SLACK_END:
        print("[FAIL] 新流越界（%#x > %#x）" % (STREAM + len(new_stream), SLACK_END))
        return 6

    new = bytearray(disk)
    end_old = STREAM + h["csize"]
    new[STREAM:max(end_old, STREAM + len(new_stream))] = bytes(max(end_old - STREAM, len(new_stream)))
    new[STREAM:STREAM + len(new_stream)] = new_stream
    struct.pack_into("<I", new, HDR + 0x20, len(new_stream))   # csize
    open(a.out, "wb").write(bytes(new))
    print("[write] %s  %d B  (原 %d B)" % (a.out, len(new), len(disk)))

    # ---- 往返验证：把写好的文件重新 inflate，必须逐字节等于 patched ----
    back, used2, eof2 = inflate(bytes(new))
    same = (back == bytes(patched))
    print("[roundtrip] 再 inflate: %d B eof=%s 逐字节等于 patched 明文=%s" % (len(back), eof2, same))
    if same:
        n = sum(1 for x, y in zip(back, out) if x != y)
        print("[roundtrip] 相对原始明文改动 %d 字节（应等于 patch 字节数）" % n)
        print("[ok] 静态 patch 链成立：明文 patch -> re-deflate -> 回填 0x%x -> inflate 完全还原" % STREAM)
        return 0
    print("[FAIL] 往返不一致")
    return 4

if __name__ == "__main__":
    sys.exit(main())
