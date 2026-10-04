#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""勇士之心 1.0.3 外科重打包：把新编 libstarry_magic.so / classes4.dex 换进底包。

设计原则（2026-10-04 定稿）：
  * 只动两个条目，其余条目**逐字节保留**（清空签名条目后重签名）；
  * 逐条保留原 compress_type / external_attr / create_system / date_time，
    resources.arsc 这类不能重压的条目不会被二次压缩；
  * 完成后回读校验：所有未列入替换清单的条目 sha256 必须与原包一致。

用法:
  python3 ysxz_repack_103.py <src.apk> <new_libstarry_magic.so> <dst.apk> [new_classes4.dex]
"""
import hashlib
import sys
import zipfile

SO_ENTRY = "lib/arm64-v8a/libstarry_magic.so"
DEX_ENTRY = "classes4.dex"
SIG_SUFFIXES = (".RSA", ".SF", ".DSA", ".EC")


def is_sig(name: str) -> bool:
    up = name.upper()
    if not up.startswith("META-INF/"):
        return False
    return up.endswith(SIG_SUFFIXES) or up.endswith("MANIFEST.MF")


def main() -> int:
    if len(sys.argv) < 4:
        print(__doc__)
        return 2
    src, so_path, dst = sys.argv[1], sys.argv[2], sys.argv[3]
    dex_path = sys.argv[4] if len(sys.argv) > 4 else None

    new_so = open(so_path, "rb").read()
    new_dex = open(dex_path, "rb").read() if dex_path else None

    zin = zipfile.ZipFile(src, "r")
    names = set(zin.namelist())
    for must in (SO_ENTRY, DEX_ENTRY):
        if must not in names:
            print("ERROR: 底包缺少 %s" % must, file=sys.stderr)
            return 1
    so_info = zin.getinfo(SO_ENTRY)
    dex_info = zin.getinfo(DEX_ENTRY)
    print("src      : %s (%d entries)" % (src, len(names)))
    print("so       : compress=%d size=%d -> %d" % (so_info.compress_type, so_info.file_size, len(new_so)))
    print("dex      : compress=%d size=%d -> %d" % (dex_info.compress_type, dex_info.file_size, len(new_dex or b"")))

    replaced = {"so": 0, "dex": 0}
    dropped = []
    accounted = set()
    with zipfile.ZipFile(dst, "w", zipfile.ZIP_DEFLATED, allowZip64=True) as zout:
        for it in zin.infolist():
            n = it.filename
            if is_sig(n):
                dropped.append(n)
                continue
            data = zin.read(n)
            if n == SO_ENTRY:
                data = new_so
                replaced["so"] += 1
            elif n == DEX_ENTRY and new_dex is not None:
                data = new_dex
                replaced["dex"] += 1
            else:
                accounted.add(n)
            zi = zipfile.ZipInfo(n, date_time=it.date_time)
            zi.compress_type = it.compress_type
            zi.external_attr = it.external_attr
            zi.internal_attr = it.internal_attr
            zi.create_system = it.create_system
            zout.writestr(zi, data)
    zin.close()

    # ---- 回读校验：未替换条目必须逐字节一致 ----
    z1 = zipfile.ZipFile(src, "r")
    z2 = zipfile.ZipFile(dst, "r")
    bad = []
    for n in sorted(accounted):
        if hashlib.sha256(z1.read(n)).digest() != hashlib.sha256(z2.read(n)).digest():
            bad.append(n)
    out_names = set(z2.namelist())
    for n in (SO_ENTRY, DEX_ENTRY):
        if n not in out_names:
            bad.append("missing:" + n)
    if any(is_sig(x) for x in out_names):
        bad.append("signature entries leaked")
    z1.close()
    z2.close()

    print("replaced : so=%d dex=%d" % (replaced["so"], replaced["dex"]))
    print("dropped  : %s" % (dropped or "(none)"))
    print("preserved: %d entries byte-identical" % len(accounted))
    if bad:
        print("ERROR: 完整性校验失败: %s" % bad[:10], file=sys.stderr)
        return 1
    print("dst      : %s" % dst)
    return 0


if __name__ == "__main__":
    sys.exit(main())
