#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""阿比斯少年冒险团（com.wks.abyss 2.36）— 外科 zip 组装器（纯 stdlib，raw-span 复制）

为什么自己写而不是 zipfile.writestr 逐条重压：
  底包 1,133,253,810 B / 1607 条，其中 STORED 1331 条（AssetBundles .ab 全是 store）。
  逐条解压再压缩要把 1.4 GB 再过一遍 deflate（慢且无意义），而本工具对**未改动条目**
  直接按「源文件字节跨度」原样搬运 —— 连 data descriptor（bit3，273 条）都一并保留，
  因此未改动条目在产物里与底包**逐字节相同**（可用 sha256 断言）。

能力：
  --replace NAME=PATH   置换已有条目（AndroidManifest.xml / classes2.dex）
  --add     NAME=PATH   追加新条目（classes3.dex / lib/arm64-v8a/libstarry_magic.so）
  --drop-signatures     丢弃 META-INF/*.SF|*.RSA|*.DSA|*.EC 与 META-INF/MANIFEST.MF
  --audit PATH          写出逐条目审计 json（sha256_raw / crc / size / 动作）
  --verify-unchanged    写完后回读：所有未改动条目 raw sha256 与底包相等，否则 exit 1

用法：
  python abssnmxt_zip_surgery.py --base <底包.apk> --out <产物.apk> \
      --replace AndroidManifest.xml=... --replace classes2.dex=... \
      --add classes3.dex=... --drop-signatures --audit audit.json --verify-unchanged
"""
import argparse
import hashlib
import json
import os
import struct
import sys
import zlib

SIG_LOCAL = 0x04034B50
SIG_CEN = 0x02014B50
SIG_EOCD = 0x06054B50


def die(msg):
    print("[FAIL] " + msg, file=sys.stderr)
    raise SystemExit(1)


def sha256(b):
    return hashlib.sha256(b).hexdigest()


class Entry:
    __slots__ = ("name", "raw_cen", "hdr_off", "csize", "usize", "crc", "method",
                 "flags", "span_off", "span_len", "extra_len_cen", "cen_extra",
                 "name_bytes", "ver_made", "ver_need", "ext_attr", "iattr")


def read_central(buf):
    """解析中央目录；返回 (entries, cd_offset, cd_size)。"""
    # EOCD 从尾部回扫（comment 最长 65535）
    i = len(buf) - 22
    eocd = -1
    while i >= 0:
        if struct.unpack_from("<I", buf, i)[0] == SIG_EOCD:
            eocd = i
            break
        i -= 1
    if eocd < 0:
        die("找不到 EOCD（不是合法 zip）")
    (_sig, disk, cd_disk, n_disk, n_total, cd_size, cd_off,
     clen) = struct.unpack_from("<IHHHHIIH", buf, eocd)
    if disk or cd_disk or n_disk != n_total:
        die("多卷 zip 不支持")
    if cd_off == 0xFFFFFFFF or n_total == 0xFFFF:
        die("zip64 不支持（本包实测无 zip64）")
    out = []
    p = cd_off
    for _ in range(n_total):
        if struct.unpack_from("<I", buf, p)[0] != SIG_CEN:
            die("中央目录签名错位 @%d" % p)
        # 中央目录头布局：sig(4) ver_made(2) ver_need(2) flags(2) method(2) time(2)
        #   date(2) crc(4) csize(4) usize(4) nlen(2) elen(2) clen(2) disk(2) iattr(2)
        #   eattr(4) offset(4) ⇒ p+4 是 ver_made/ver_need，p+8 起是 flags
        ver_made, ver_need = struct.unpack_from("<HH", buf, p + 4)
        (flags, method, mtime, mdate, crc, csize, usize, nlen, elen,
         clen2, _dstart, _iattr, _eattr,
         hdr_off) = struct.unpack_from("<HHHHIIIHHHHHII", buf, p + 8)
        name = buf[p + 46:p + 46 + nlen].decode("utf-8", "replace")
        e = Entry()
        e.name = name
        e.ver_made = ver_made
        e.ver_need = ver_need
        e.ext_attr = _eattr
        e.iattr = _iattr
        e.raw_cen = bytes(buf[p:p + 46 + nlen + elen + clen2])
        e.extra_len_cen = elen
        # ★中央目录的 extra 必须原样保留：局部头里的 extra 是原样搬的，
        #   若这里丢掉 central extra，zip 工具会报 "header mismatch"（zipalign 刷屏告警）
        e.cen_extra = bytes(buf[p + 46 + nlen:p + 46 + nlen + elen])
        e.name_bytes = bytes(buf[p + 46:p + 46 + nlen])
        e.hdr_off = hdr_off
        e.csize = csize
        e.usize = usize
        e.crc = crc
        e.method = method
        e.flags = flags
        out.append(e)
        p += 46 + nlen + elen + clen2
    return out, cd_off, cd_size


def local_span(buf, e, next_off):
    """条目在源文件里的原始跨度 [hdr_off, next_off)（含 local header + data + data descriptor）。"""
    if struct.unpack_from("<I", buf, e.hdr_off)[0] != SIG_LOCAL:
        die("local header 签名错位: %s @%d" % (e.name, e.hdr_off))
    nlen, elen = struct.unpack_from("<HH", buf, e.hdr_off + 26)
    data_off = e.hdr_off + 30 + nlen + elen
    end = data_off + e.csize
    # bit3 ⇒ 后面跟 data descriptor（12 或 16 字节）：用「到下一个条目/中央目录」的跨度兜住
    if e.flags & 0x8:
        end = next_off
    return e.hdr_off, end - e.hdr_off


def build_local(name, data, method=8, mtime=0x5000, mdate=0x5A21):
    nb = name.encode("utf-8")
    if method == 0:
        comp = data
    else:
        co = zlib.compressobj(9, zlib.DEFLATED, -15)
        comp = co.compress(data) + co.flush()
    crc = zlib.crc32(data) & 0xFFFFFFFF
    hdr = struct.pack("<IHHHHHIIIHH", SIG_LOCAL, 20, 0, method, mtime, mdate,
                      crc, len(comp), len(data), len(nb), 0)
    return hdr + nb + comp, method, crc, len(comp), len(data)


def build_cen(name, method, crc, csize, usize, off, flags=0, mtime=0x5000, mdate=0x5A21,
              extra=b"", ext_attr=0o644 << 16, ver_made=20, ver_need=20, iattr=0):
    nb = name.encode("utf-8")
    return struct.pack("<IHHHHHHIIIHHHHHII", SIG_CEN, ver_made, ver_need, flags, method,
                       mtime, mdate, crc, csize, usize, len(nb), len(extra), 0, 0,
                       iattr, ext_attr, off) + nb + extra


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--replace", action="append", default=[])
    ap.add_argument("--add", action="append", default=[])
    ap.add_argument("--drop-signatures", action="store_true")
    ap.add_argument("--audit", default=None)
    ap.add_argument("--verify-unchanged", action="store_true")
    a = ap.parse_args()

    def kv(lst, what):
        d = {}
        for s in lst:
            if "=" not in s:
                die("%s 需要 NAME=PATH 形式: %s" % (what, s))
            k, v = s.split("=", 1)
            if not os.path.isfile(v):
                die("%s 源文件不存在: %s" % (what, v))
            d[k] = v
        return d

    rep = kv(a.replace, "--replace")
    add = kv(a.add, "--add")

    with open(a.base, "rb") as f:
        buf = f.read()
    entries, cd_off, cd_size = read_central(buf)
    names = [e.name for e in entries]
    if len(set(names)) != len(names):
        die("底包有重名条目")
    for n in rep:
        if n not in names:
            die("--replace 目标不在底包里: %s" % n)
    for n in add:
        if n in names:
            die("--add 目标已存在于底包（会重复）: %s" % n)

    order = sorted(entries, key=lambda e: e.hdr_off)
    next_of = {}
    for k, e in enumerate(order):
        next_of[e.name] = order[k + 1].hdr_off if k + 1 < len(order) else cd_off

    def is_sig(n):
        if not n.startswith("META-INF/"):
            return False
        if n == "META-INF/MANIFEST.MF":
            return True
        return n.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

    audit = []
    out = open(a.out, "wb")
    cen = []
    off = 0
    written = {}

    def emit_local(name, blob, method, crc, csize, usize, flags=0, extra=b"",
                   ext_attr=0o644 << 16, ver_made=20, ver_need=20, iattr=0,
                   raw_cen=None):
        nonlocal off
        out.write(blob)
        if raw_cen is not None:
            # ★原样沿用源中央目录记录，只补丁「相对本文件偏移」字段（第 42..46 字节）：
            #   自己重建 ver_made/iattr/extra 会与局部头不一致 ⇒ zipalign 每条都报
            #   "WARNING: header mismatch"（实测 1334 条），且丢 comment/extra 语义。
            c = bytearray(raw_cen)
            struct.pack_into("<I", c, 42, off)
            cen.append(bytes(c))
        else:
            cen.append(build_cen(name, method, crc, csize, usize, off, flags, extra=extra,
                                 ext_attr=ext_attr, ver_made=ver_made, ver_need=ver_need,
                                 iattr=iattr))
        written[name] = (off, len(blob))
        off += len(blob)

    for e in order:
        if a.drop_signatures and is_sig(e.name):
            audit.append({"name": e.name, "action": "drop-signature"})
            continue
        if e.name in rep:
            data = open(rep[e.name], "rb").read()
            blob, method, crc, csize, usize = build_local(e.name, data)
            emit_local(e.name, blob, method, crc, csize, usize)
            audit.append({"name": e.name, "action": "replace",
                          "new_sha256": sha256(data), "new_size": len(data),
                          "old_sha256": None, "old_crc": e.crc})
            continue
        s_off, s_len = local_span(buf, e, next_of[e.name])
        raw = buf[s_off:s_off + s_len]
        # ★必须沿用源条目的 flags：bit3（data descriptor）在中央目录里同样置位，
        #   否则回读时 span 长度算短 12/16 字节 ⇒ 假「不一致」
        # ★中央目录必须原样沿用源条目的 ver_made/ver_need/ext_attr/ext_extra，
        #   否则 zipalign 会对每个 STORED 条目报 "WARNING: header mismatch"（实测 1331 条）
        emit_local(e.name, raw, e.method, e.crc, e.csize, e.usize, e.flags,
                   raw_cen=e.raw_cen)
        audit.append({"name": e.name, "action": "copy",
                      "raw_sha256": sha256(raw), "csize": e.csize, "usize": e.usize,
                      "crc": e.crc})

    for n in sorted(add):
        data = open(add[n], "rb").read()
        blob, method, crc, csize, usize = build_local(n, data)
        emit_local(n, blob, method, crc, csize, usize)
        audit.append({"name": n, "action": "add", "new_sha256": sha256(data),
                      "new_size": len(data)})

    cd_start = off
    for c in cen:
        out.write(c)
        off += len(c)
    cd_len = off - cd_start
    out.write(struct.pack("<IHHHHIIH", SIG_EOCD, 0, 0, len(cen), len(cen), cd_len,
                          cd_start, 0))
    out.close()

    h = hashlib.sha256()
    with open(a.out, "rb") as f:
        for chunk in iter(lambda: f.read(8 << 20), b""):
            h.update(chunk)

    if a.audit:
        with open(a.audit, "w", encoding="utf-8") as f:
            json.dump({"base": a.base, "out": a.out,
                       "base_sha256": sha256(buf), "out_sha256": h.hexdigest(),
                       "entries": len(cen), "audit": audit}, f,
                      ensure_ascii=False, indent=1)

    print("  entries: base=%d out=%d (copy=%d replace=%d add=%d drop=%d)"
          % (len(entries), len(cen),
             sum(1 for x in audit if x["action"] == "copy"),
             sum(1 for x in audit if x["action"] == "replace"),
             sum(1 for x in audit if x["action"] == "add"),
             sum(1 for x in audit if x["action"] == "drop-signature")))

    if a.verify_unchanged:
        with open(a.out, "rb") as f:
            ob = f.read()
        o_entries, _o_cd, _o_sz = read_central(ob)
        o_map = {e.name: e for e in o_entries}
        o_order = sorted(o_entries, key=lambda e: e.hdr_off)
        o_next = {}
        for k, e in enumerate(o_order):
            o_next[e.name] = o_order[k + 1].hdr_off if k + 1 < len(o_order) else _o_cd
        bad = []
        checked = 0
        for e in order:
            if e.name in rep or (a.drop_signatures and is_sig(e.name)):
                continue
            if e.name not in o_map:
                bad.append((e.name, "missing"))
                continue
            ob_e = o_map[e.name]
            s_off, s_len = local_span(buf, e, next_of[e.name])
            n_off, n_len = local_span(ob, ob_e, o_next[e.name])
            hs = sha256(buf[s_off:s_off + s_len])
            hn = sha256(ob[n_off:n_off + n_len])
            checked += 1
            if hs != hn:
                bad.append((e.name, "differs"))
        for n in add:
            if n not in o_map:
                bad.append((n, "add-missing"))
        if bad:
            for n, why in bad[:10]:
                print("   ! %s %s" % (n, why), file=sys.stderr)
            die("未改动条目未逐字节一致（%d 处）" % len(bad))
        print("  verify: %d 个未改动条目 raw sha256 与底包逐一相同 ✓" % checked)

    print("  out: %s  %.1f MB  sha256=%s" % (a.out, os.path.getsize(a.out) / 1e6,
                                             h.hexdigest()[:16]))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())