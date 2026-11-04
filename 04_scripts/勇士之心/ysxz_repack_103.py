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
  python3 ysxz_repack_103.py <src.apk> <libs_dir> <dst.apk> [new_classes4.dex]
      ↑ 第二参给目录时，按目录下 <abi>/libstarry_magic.so 替换包内 lib/<abi>/ 同名条目
        （2026-11-04 加固下线轮：三 ABI 一起换，避免包内残留旧 so）

2026-11-04：新增「加固残留」门禁 —— 任何被换进去的 so 若仍含 "sg-grant-v1"
（v1 票 pepper，加固层标记）直接拒绝出包。
"""
import hashlib
import os
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

    # 替换表：{包内条目名: 新字节}。第二参既可以是单个 .so，也可以是多 ABI 目录
    targets = {}
    if os.path.isdir(so_path):
        for abi in sorted(os.listdir(so_path)):
            f = os.path.join(so_path, abi, "libstarry_magic.so")
            if not os.path.isfile(f):
                continue
            target = "lib/%s/libstarry_magic.so" % abi
            data = open(f, "rb").read()
            if b"sg-grant-v1" in data:
                print("ERROR: %s 仍含 v1 pepper（加固未去干净）" % f, file=sys.stderr)
                return 1
            targets[target] = data
        if not targets:
            print("ERROR: %s 下没有 <abi>/libstarry_magic.so" % so_path, file=sys.stderr)
            return 1
    else:
        data = open(so_path, "rb").read()
        if b"sg-grant-v1" in data:
            print("ERROR: %s 仍含 v1 pepper（加固未去干净）" % so_path, file=sys.stderr)
            return 1
        targets[SO_ENTRY] = data

    new_dex = open(dex_path, "rb").read() if dex_path else None

    zin = zipfile.ZipFile(src, "r")
    names = set(zin.namelist())
    missing = [t for t in targets if t not in names]
    if missing:
        print("ERROR: 底包缺少条目 %s" % missing, file=sys.stderr)
        return 1
    if DEX_ENTRY not in names:
        print("ERROR: 底包缺少 %s" % DEX_ENTRY, file=sys.stderr)
        return 1
    for t, d in targets.items():
        print("so[%s] : -> %d B" % (t, len(d)))
    so_info = zin.getinfo(SO_ENTRY)
    dex_info = zin.getinfo(DEX_ENTRY)
    print("src      : %s (%d entries)" % (src, len(names)))
    print("so       : compress=%d size=%d -> %d" % (so_info.compress_type, so_info.file_size, sum(len(d) for d in targets.values())))
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
            if n in targets:
                data = targets[n]
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
    for n in list(targets) + [DEX_ENTRY]:
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
