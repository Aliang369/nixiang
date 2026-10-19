#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
fg_unpack.py — 绿梦 FairGuard 离线解包器（阶段 1 骨架 / 诊断版）

现状（2026-10-20 §19 定案）：**"加密"其实就是 zlib/deflate 压缩**——磁盘 0x458 起是一段
标准 zlib 流（78 9C），膨胀出 0x400B78 B，正好是虚址 0x458..0x400FD0 的镜像；壳通过
DT_NEEDED 的 libz.so 调 inflate 就地解开，**没有密钥**。本工具据此直接产出镜像，并报告与
运行期镜像的逐字节一致率与差异分类（差异来自壳运行期清零 .dynstr + 重定位）。

用法:
  python3 fg_unpack.py --disk 06_dumped/绿梦_20261016/libFairGuard.so \
                       --image 06_dumped/绿梦_20261016/fg_runtime/libFairGuard_rwxp.bin \
                       [--out 06_dumped/绿梦_20261016/fg_unpacked]

退出码: 0 = 诊断完成（不代表解包成功）；3 = 未提供镜像（仅打印磁盘区域模型）
"""
import argparse
import collections
import hashlib
import json
import math
import os
import struct
import sys
import zlib

# ---------------- 区域模型（Stage 0/3 实测） ----------------
REGIONS = [
    ("elf_head",   0x000000, 0x000400, "明文 ELF 头（磁盘=镜像前 64B 相同）"),
    ("blob_head",  0x000400, 0x000458, "FairGuard blob 头：+0x0c magic 0x612EB115、+0x10 crc、+0x1c usize、+0x20 csize、+0x28 end_va"),
    ("zlib_stream", 0x000458, 0x1F452E, "**标准 zlib(deflate) 流**，78 9C 起；膨胀出 usize=0x400B78 B，覆盖 va 0x458..0x400FD0"),
    ("stream_tail", 0x1F0000, 0x1F452E, "压缩流尾部（盘上唯一的高熵块 17.6 KB 就是压缩流尾巴，不是独立 blob）"),
    ("zero_fill",  0x1F452E, 0x400000, "磁盘全零；运行期内容 = 同一条流的膨胀结果（va 0x1F452E..0x400FD0 段）"),
    ("shell_code", 0x413AB0, 0x431350, "磁盘明文壳代码（121 KB，含 mini-libc + svc 桩）"),
    ("rodata_tbl", 0x431350, 0x43172C, "988 B 表（80 个不同字节、602 个 0，**不是** 256B 置换/S-box）"),
]

STUB_PATTERNS = {
    "exit_group(0x5E)": "c80b80d2010000d4",
    "openat(0x38)":     "080780d2010000d4",
}

def entropy(b):
    if not b:
        return 0.0
    c = collections.Counter(b)
    n = len(b)
    return -sum(v / n * math.log2(v / n) for v in c.values())

def elf_header(b):
    if b[:4] != b"\x7fELF":
        return None
    f = struct.unpack_from("<16sHHIQQQIHHHHHH", b, 0)
    return {"type": f[1], "machine": f[2], "entry": f[4], "phoff": f[5],
            "shoff": f[6], "phnum": f[10]}

def py_argv_pad():
    return None

def count_pattern(buf, hexpat):
    pat = bytes.fromhex(hexpat)
    n = 0
    i = buf.find(pat)
    while i >= 0:
        n += 1
        i = buf.find(pat, i + 1)
    return n

def anchor_check(disk, image):
    """Stage 3 的锚点：镜像里的 svc 桩区与磁盘壳桩区有部分逐字节相同。"""
    out = []
    for (io, do, n, tag) in ((0x35B100, 0x42EB00, 0x400, "桩区 0x35B100/0x42EB00"),
                             (0x35B000, 0x42E000, 0x1000, "桩区 0x35B000/0x42E000"),
                             (0x000000, 0x000000, 0x1000, "ELF 头页")):
        a = image[io:io + n]
        b = disk[do:do + n]
        if not a or not b:
            continue
        eq = sum(1 for x, y in zip(a, b) if x == y)
        out.append({"tag": tag, "img": io, "disk": do, "n": n,
                    "equal_ratio": round(eq / float(n), 4)})
    return out

def block_shift_scan(disk, image, blocks=(0x0, 0x10000, 0x100000, 0x200000, 0x300000, 0x35B000),
                     step=0x800, sample=0x400):
    """每个 64K 镜像块，找"最像"的磁盘平移量。用来判断是否存在分段整体复制。"""
    res = []
    for io in blocks:
        if io + 0x10000 > len(image):
            continue
        best = (None, -1)
        for do in range(0, len(disk) - 0x2000, step):
            s = 0
            for k in range(4):
                so = io + k * 0x4000
                a = image[so:so + sample]
                b = disk[do + k * 0x4000:do + k * 0x4000 + sample]
                if len(a) < sample or len(b) < sample:
                    continue
                s += sum(1 for x, y in zip(a, b) if x == y)
            if s > best[1]:
                best = (do - io, s)
        res.append({"img_block": io, "best_shift": None if best[0] is None else hex(best[0]),
                    "equal_bytes": best[1], "sampled_bytes": 4 * sample})
    return res

BLOB_HDR = 0x400
STREAM   = 0x458

def decrypt(disk):
    """
    ==== 算法（2026-10-20 §19 定案：不是密码，是 zlib/deflate） ====
    磁盘 0x458 起是一段**标准 zlib 流**（78 9C，DEFLATE），膨胀出
      usize = blob_head+0x1c = 0x400B78 = 4,197,240 B，
    正好是虚址 0x458..0x400FD0 的线性镜像；流在 0x1F452E 结束（csize = blob_head+0x20 = 0x1F40D6）。
    运行期镜像与该膨胀结果 A 区 99.40% / B 区 99.50% 逐字节相同（差异见 main 里的分类）。
    因此"解密"= 壳经 **libz.so（DT_NEEDED 第 3 项）的 inflate** 就地解开，无密钥。

    返回 (start_va, out, meta) ；失败返回 None。
    """
    magic, _z0, _z1, _z2, crc, _z3, _z4, usize = struct.unpack_from("<8I", disk, BLOB_HDR)
    csize, _u24, end_va, _u2c, _u30 = struct.unpack_from("<5I", disk, BLOB_HDR + 0x20)
    d = zlib.decompressobj(15)
    try:
        out = d.decompress(disk[STREAM:])
        out += d.flush()
    except zlib.error as exc:
        print("  [decrypt] zlib 失败: %s" % exc)
        return None
    consumed = len(disk) - STREAM - len(d.unused_data)
    meta = {"magic": hex(magic), "hdr_crc": hex(crc), "usize_hdr": usize, "usize_got": len(out),
            "csize_hdr": csize, "csize_got": consumed, "end_va_hdr": hex(end_va),
            "end_va_got": hex(STREAM + len(out)), "eof": d.eof,
            "stream_end": hex(STREAM + consumed)}
    ok = (len(out) == usize and consumed == csize and STREAM + len(out) == end_va and d.eof)
    print("  [decrypt] zlib: stream %#x..%#x (csize %#x/%#x) -> %d B (usize %#x/%#x) eof=%s 头字段全对=%s"
          % (STREAM, STREAM + consumed, consumed, csize, len(out), usize, usize, d.eof, ok))
    return STREAM, out, meta

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--disk", default="06_dumped/绿梦_20261016/libFairGuard.so")
    ap.add_argument("--image", default="06_dumped/绿梦_20261016/fg_runtime/libFairGuard_rwxp.bin")
    ap.add_argument("--out", default="06_dumped/绿梦_20261016/fg_unpacked")
    a = ap.parse_args()

    disk = open(a.disk, "rb").read()
    print("[disk] %s  %d B  md5=%s" % (a.disk, len(disk), hashlib.md5(disk).hexdigest()))
    eh = elf_header(disk)
    print("[disk] ELF %s" % eh)

    print("\n[regions]")
    for name, lo, hi, note in REGIONS:
        seg = disk[lo:min(hi, len(disk))]
        print("  %-12s %#08x..%#08x  %8d B  entropy=%.2f  %s"
              % (name, lo, hi, max(0, len(seg)), entropy(seg), note))

    status = {"disk_md5": hashlib.md5(disk).hexdigest(), "algorithm_recovered": False}

    if not os.path.exists(a.image):
        print("\n[image] 缺运行期镜像，仅完成磁盘侧建模（退出码 3）")
        return 3
    image = open(a.image, "rb").read()
    print("\n[image] %s  %d B  md5=%s" % (a.image, len(image), hashlib.md5(image).hexdigest()))
    print("[image] 头字段与磁盘一致: %s" % (elf_header(image) == eh))

    print("\n[entropy]")
    for tag, lo, hi in (("payload_enc(disk)", 0x400, 0x1F0000),
                        ("payload(disk->img)", 0x400, 0x1F0000),
                        ("zero_fill(disk)", 0x1F0000, 0x400000),
                        ("image_全段", 0, 0x400000)):
        src = disk if "disk" in tag else image
        print("  %-20s entropy=%.2f" % (tag, entropy(src[lo:hi])))

    print("\n[stub patterns in image]")
    for name, pat in STUB_PATTERNS.items():
        print("  %-18s x%d" % (name, count_pattern(image, pat)))

    print("\n[anchor] （Stage 3 锚点：部分逐字节相同）")
    anchors = anchor_check(disk, image)
    for x in anchors:
        print("  %-24s equal=%.2f%% (%d B 窗口)" % (x["tag"], x["equal_ratio"] * 100, x["n"]))

    print("\n[block shift scan] （是否存在分段整体复制）")
    shifts = block_shift_scan(disk, image)
    for x in shifts:
        print("  img %#08x  best_shift=%-9s equal=%d/%d"
              % (x["img_block"], x["best_shift"], x["equal_bytes"], x["sampled_bytes"]))

    res = decrypt(disk)
    if res is None:
        print("\n[decrypt] 失败 —— 未产出镜像")
        status["algorithm_recovered"] = False
    else:
        start_va, out, meta = res
        os.makedirs(a.out, exist_ok=True)
        total = max(len(image), start_va + len(out))   # 膨胀结果覆盖到 va 0x400FD0，可能超出镜像 dump 的 4 MB
        got = bytearray(total)
        got[0:start_va] = disk[0:start_va]
        got[start_va:start_va + len(out)] = out
        p = os.path.join(a.out, "libFairGuard_unpacked.bin")
        open(p, "wb").write(got)
        n = min(len(got), len(image))
        same = sum(1 for i in range(n) if got[i] == image[i])
        print("\n[decrypt] 写出 %s（覆盖 va 0..%#x）" % (p, total))
        print("  与运行期镜像逐字节相同: %d/%d = %.2f%%（差额=壳运行期把 .dynstr 等字符串清零 + 重定位）"
              % (same, n, 100.0 * same / n))
        runs = []
        cur = None
        for i in range(n):
            if got[i] != image[i]:
                if cur and i == cur[1] + 1:
                    cur[1] = i
                else:
                    if cur: runs.append(tuple(cur))
                    cur = [i, i]
        if cur: runs.append(tuple(cur))
        zeroed = sum(1 for x, y in runs if image[x:y + 1] == bytes(y - x + 1))
        print("  差异段 %d 段，其中「运行期为全 0」(被壳清零) %d 段" % (len(runs), zeroed))
        big = max((y - x + 1 for x, y in runs), default=0)
        print("  最大差异段 %#x B；首段 %s" % (big, hex(runs[0][0]) if runs else "-"))
        status["algorithm"] = "zlib/deflate inflate from 0x458 (libz.so DT_NEEDED)"
        status["algorithm_recovered"] = True
        status["byte_identical_ratio"] = round(100.0 * same / n, 4)
        status["diff_runs"] = len(runs)
        status["diff_runs_zeroed"] = zeroed
        status["inflate"] = meta

    os.makedirs(a.out, exist_ok=True)
    status["anchors"] = anchors
    status["block_shifts"] = shifts
    with open(os.path.join(a.out, "fg_unpack_status.json"), "w") as f:
        json.dump(status, f, ensure_ascii=False, indent=1)
    print("\n[status] → %s/fg_unpack_status.json" % a.out)
    return 0

if __name__ == "__main__":
    sys.exit(main())
