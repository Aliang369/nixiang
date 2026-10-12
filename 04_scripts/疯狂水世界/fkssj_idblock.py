#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""疯狂水世界：APK Signing Block 自定义 ID-value 对（渠道块）保留 / 回注 / 校验。

背景（2026-10-13 实测底包 base.apk，md5 f8ed1b954a3c5ce91dcecb4e721f3e9b）：
底包 APK Signing Block 里除标准两对外还有一个**渠道自有块**：

    id = 0x64707763   value = b"259425"   （6 字节 ASCII，渠道号 / 分包号）

`apksigner sign` 只会写 `0x7109871a`（v2 签名）与 `0x42726577`（verity padding），
**重签必然丢掉 0x64707763**。而底包内三条读取路径都盯着它：

  · `com.ewrisk.sdk.util.as.L(Context,String)`   —— 逐对扫签名块取目标 id 的 UTF-8 值
  · `com.ewrisk.sdk.util.dolly.a.c`（IdValueReader）—— `getByteValueById(File,int)`
  · `com.cw.platform.core.util.dolly.common.*` / `com.xcsp.sdk.util.dolly.*`（同源三份副本）

丢块的后果是渠道号取空 —— 轻则渠道上报变成默认值，重则 SDK 初始化直接抛错。

## 为什么「签名后再塞」不破签
APK Signature Scheme v2 的摘要覆盖范围是：
  ① 各 ZIP entry 的原始数据（文件头 → 签名块起始处）
  ② ZIP Central Directory
  ③ EOCD（**其中「中央目录偏移」字段按规范替换为「签名块起始偏移」**）
签名块**自身内容不在摘要里**，且签名块起始偏移（= entry 数据末尾）在尾部插入时不变。
因此在签名块里追加一个未知 ID-value 对：
  · ① 不变（插入点在签名块内部，位于 entry 数据之后）
  · ② 不变
  · ③ 只有 EOCD 的「中央目录偏移」字段要 +delta，而校验方算摘要时会把这个字段
      替换回签名块起始偏移 ⇒ 摘要输入逐字节不变 ⇒ **v2 签名依然有效**。
这正是美团 Walle 多渠道打包的原理；`apksigner verify` 与 Android PackageManager
都只挑自己认识的 ID（0x7109871a），其余对忽略。

用法：
  校验：python3 04_scripts/疯狂水世界/fkssj_idblock.py check  <apk> [--id 0x64707763] [--expect 259425]
  回注：python3 04_scripts/疯狂水世界/fkssj_idblock.py inject <apk> [--out <新apk>] \
            [--id 0x64707763] [--value 259425]
  注：inject **原地重写**时先写 .tmp 再替换；--out 则写到新文件。
"""

import argparse
import os
import struct
import sys

MAGIC = b"APK Sig Block 42"
EOCD_SIG = b"PK\x05\x06"
ZIP64_EOCD_SIG = b"PK\x06\x06"
ZIP64_LOCATOR_SIG = b"PK\x06\x07"
DEFAULT_ID = 0x64707763
DEFAULT_VALUE = b"259425"


def _fail(msg):
    print("ERROR: %s" % msg, file=sys.stderr)
    sys.exit(1)


class SigningBlock(object):
    __slots__ = ("blk_start", "blk_end", "pairs", "cd_off", "eocd_off", "cd_size",
                 "entries_count", "zip64_eocd_off", "_data")

    def __init__(self):
        self.blk_start = 0
        self.blk_end = 0
        self.pairs = []          # list of (id:int, value:bytes), in file order
        self.cd_off = 0
        self.cd_size = 0
        self.eocd_off = 0
        self.entries_count = 0
        self.zip64_eocd_off = None


def parse(path):
    with open(path, "rb") as f:
        data = f.read()
    n = len(data)
    if n < 32:
        _fail("%s 太小，不像 APK" % path)

    # ---- 1) 找 EOCD（comment 最长 65535，从尾部回扫）----
    lo = max(0, n - 22 - 65535)
    eocd = -1
    pos = data.rfind(EOCD_SIG, lo)
    while pos != -1:
        if pos + 22 <= n:
            f = struct.unpack_from("<IHHHHIIH", data, pos)
            if pos + 22 + f[7] == n:      # f = sig,dn,cddisk,cnt,cdnum,cdsize,cdoff,clen
                eocd = pos
                break
        pos = data.rfind(EOCD_SIG, lo, pos)
    if eocd == -1:
        _fail("找不到 EOCD")
    f = struct.unpack_from("<IHHHHIIH", data, eocd)
    cnt, cdsize, cdoff, clen = f[3], f[5], f[6], f[7]

    sb = SigningBlock()
    sb.eocd_off = eocd
    sb.entries_count = cnt
    sb.cd_size = cdsize
    sb.cd_off = cdoff

    # ---- ZIP64：cd 偏移可能记在 ZIP64 EOCD ----
    loc = data.rfind(ZIP64_LOCATOR_SIG, max(0, eocd - 64), eocd)
    if loc != -1 and data[cdoff:cdoff + 4] != b"PK\x01\x02":
        z64 = struct.unpack_from("<Q", data, loc + 8)[0]
        sb.zip64_eocd_off = z64
        if data[z64:z64 + 4] == ZIP64_EOCD_SIG:
            cdoff = struct.unpack_from("<Q", data, z64 + 48)[0]
            cdsize = struct.unpack_from("<Q", data, z64 + 40)[0]
            sb.cd_off = cdoff
            sb.cd_size = cdsize

    if data[cdoff:cdoff + 4] != b"PK\x01\x02":
        _fail("中央目录偏移 %d 处不是 PK\\x01\\x02（EOCD 偏移 %d）" % (cdoff, eocd))

    # ---- 2) 签名块紧贴在 cd_off 之前 ----
    if data[cdoff - 16:cdoff] != MAGIC:
        _fail("中央目录之前 16 字节不是 %r ⇒ 该 APK 无 APK Signing Block（v2 未启用？）" % MAGIC)
    size2 = struct.unpack_from("<Q", data, cdoff - 24)[0]
    blk_start = cdoff - size2 - 8          # size2 不含自身 8 字节
    size1 = struct.unpack_from("<Q", data, blk_start)[0]
    if size1 != size2:
        _fail("签名块首尾 size 不一致（%d vs %d）" % (size1, size2))
    if blk_start < 0:
        _fail("签名块起始偏移为负（size=%d）" % size2)

    sb.blk_start = blk_start
    sb.blk_end = cdoff

    # ---- 3) 拆 ID-value 对 ----
    p = blk_start + 8
    end = cdoff - 24
    while p < end:
        if p + 12 > end:
            _fail("签名块 ID-value 对结构损坏（偏移 %d 剩 %d 字节）" % (p, end - p))
        plen = struct.unpack_from("<Q", data, p)[0]
        if plen < 4 or p + 8 + plen > end:
            _fail("签名块对长度非法：%d（偏移 %d）" % (plen, p))
        pid = struct.unpack_from("<I", data, p + 8)[0]
        sb.pairs.append((pid, data[p + 12:p + 8 + plen]))
        p += 8 + plen
    if p != end:
        _fail("签名块未对齐到结尾（%d != %d）" % (p, end))
    sb._data = data
    return sb


def _fmt_pair(pid, value):
    body = struct.pack("<I", pid) + value
    return struct.pack("<Q", len(body)) + body


def cmd_check(args):
    sb = parse(args.apk)
    print("[idblock] %s" % args.apk)
    print("  签名块 : [%d, %d)  共 %d 字节" % (sb.blk_start, sb.blk_end, sb.blk_end - sb.blk_start))
    print("  中央目录: off=%d size=%d  EOCD@%d  entries=%d" % (sb.cd_off, sb.cd_size,
                                                               sb.eocd_off, sb.entries_count))
    for pid, value in sb.pairs:
        tag = ""
        if pid == 0x7109871a:
            tag = "  <- v2 签名"
        elif pid == 0x42726577:
            tag = "  <- verity padding"
        elif pid == args.id:
            tag = "  <- 本作渠道块"
        printable = value if all(32 <= c < 127 for c in value) else None
        shown = printable.decode("ascii") if printable else ("<%d B 二进制>" % len(value))
        print("  id=0x%08x (%s)  len=%d  value=%s%s"
              % (pid, "".join(chr((pid >> s) & 0xFF) for s in (0, 8, 16, 24)),
                 len(value), shown, tag))

    found = [v for i, v in sb.pairs if i == args.id]
    if not found:
        print("RESULT: MISSING  未找到 id=0x%08x" % args.id)
        return 1
    got = found[0]
    if args.expect is not None:
        want = args.expect.encode("utf-8") if isinstance(args.expect, str) else args.expect
        if got != want:
            print("RESULT: MISMATCH  id=0x%08x 值 %r != 期望 %r" % (args.id, got, want))
            return 1
        print("RESULT: OK  0x%08x = %r" % (args.id, got))
        return 0
    print("RESULT: OK  0x%08x = %r（未指定 --expect）" % (args.id, got))
    return 0


def cmd_inject(args):
    sb = parse(args.apk)
    data = sb._data
    pid = args.id
    value = args.value.encode("utf-8")

    for i, v in sb.pairs:
        if i == pid:
            if v == value:
                print("[idblock] 已存在 id=0x%08x 值匹配 %r ⇒ 无需回注" % (pid, value))
                return 0
            _fail("已存在 id=0x%08x 但值不同（现有 %r，期望 %r）—— 拒绝覆盖" % (pid, v, value))

    new_pair = _fmt_pair(pid, value)
    delta = len(new_pair)
    new_size2 = (sb.blk_end - sb.blk_start - 8) + delta

    head = data[:sb.blk_start]
    blk_body = data[sb.blk_start + 8:sb.blk_end - 24]
    tail = data[sb.blk_end:]

    new_blk = struct.pack("<Q", new_size2) + blk_body + new_pair \
        + struct.pack("<Q", new_size2) + MAGIC

    # EOCD 的「中央目录偏移」字段 +delta（签名校验会把这个字段替换回签名块起始偏移，
    # 故改动它不影响 v2 摘要；但不改的话解压器会找不到中央目录）
    tail = bytearray(tail)
    eocd_rel = sb.eocd_off - sb.blk_end
    old = struct.unpack_from("<I", tail, eocd_rel + 16)[0]
    if old != sb.cd_off:
        _fail("EOCD 中央目录偏移字段(%d) 与解析值(%d) 不一致 —— 解析可疑，拒绝改写" % (old, sb.cd_off))
    struct.pack_into("<I", tail, eocd_rel + 16, old + delta)
    if sb.zip64_eocd_off is not None:
        z_rel = sb.zip64_eocd_off - sb.blk_end
        if z_rel >= 0:
            z_old = struct.unpack_from("<Q", tail, z_rel + 48)[0]
            if z_old == sb.cd_off:
                struct.pack_into("<Q", tail, z_rel + 48, z_old + delta)

    out = args.out
    tmp = (out or args.apk) + ".idblock.tmp"
    with open(tmp, "wb") as f:
        f.write(head)
        f.write(new_blk)
        f.write(bytes(tail))
    os.replace(tmp, out or args.apk)

    print("[idblock] 回注完成 id=0x%08x value=%r  (+%d 字节)" % (pid, value, delta))
    print("  签名块 [%d,%d) size=%d→%d   中央目录 off=%d→%d"
          % (sb.blk_start, sb.blk_end, sb.blk_end - sb.blk_start - 8,
             new_size2, sb.cd_off, sb.cd_off + delta))
    print("  输出: %s" % (out or args.apk))
    return 0


def main():
    ap = argparse.ArgumentParser(add_help=True, description="APK Signing Block 自定义 ID-value 对 保留/回注/校验")
    sub = ap.add_subparsers(dest="cmd")

    c = sub.add_parser("check", help="列出签名块所有 ID-value 对并校验目标 id")
    c.add_argument("apk")
    c.add_argument("--id", type=lambda s: int(s, 0), default=DEFAULT_ID)
    c.add_argument("--expect", default=None)
    c.set_defaults(func=cmd_check)

    i = sub.add_parser("inject", help="追加缺失的 ID-value 对（不破 v2 签名）")
    i.add_argument("apk")
    i.add_argument("--out", default=None, help="输出到新文件；省略则原地重写")
    i.add_argument("--id", type=lambda s: int(s, 0), default=DEFAULT_ID)
    i.add_argument("--value", default=DEFAULT_VALUE.decode("ascii"))
    i.set_defaults(func=cmd_inject)

    args = ap.parse_args()
    if not args.cmd:
        ap.print_help()
        return 2
    return args.func(args)


if __name__ == "__main__":
    sys.exit(main())
