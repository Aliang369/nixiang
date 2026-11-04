# -*- coding: utf-8 -*-
"""星签 so-only 重打包：底包逐条搬运，只替换 lib/arm64-v8a/libil2cpp.so，去掉旧签名后重签。"""
import sys, zipfile, os

src_apk, new_so, out_apk = sys.argv[1], sys.argv[2], sys.argv[3]
SIG = (".SF", ".RSA", ".DSA", ".EC", "MANIFEST.MF")
blob = open(new_so, "rb").read()
zi_src = zipfile.ZipFile(src_apk)
old = zi_src.getinfo("lib/arm64-v8a/libil2cpp.so")
print("原条目 csize=%d usize=%d compress=%d" % (old.compress_size, old.file_size, old.compress_type))
os.makedirs(os.path.dirname(out_apk), exist_ok=True)
dst = zipfile.ZipFile(out_apk, "w", zipfile.ZIP_DEFLATED, allowZip64=True)
kept = dropped = 0
for zi in zi_src.infolist():
    n = zi.filename
    if n.startswith("META-INF/") and n.upper().endswith(SIG):
        dropped += 1
        continue
    if n == "lib/arm64-v8a/libil2cpp.so":
        ni = zipfile.ZipInfo(n, date_time=zi.date_time)
        ni.compress_type = zipfile.ZIP_DEFLATED
        ni.external_attr = zi.external_attr
        dst.writestr(ni, blob)
        continue
    with zi_src.open(zi) as fp:
        dst.writestr(zi, fp.read())
    kept += 1
dst.close()
print("kept", kept, "dropped-sig", dropped, "out", os.path.getsize(out_apk))
print("检查:", zipfile.ZipFile(out_apk).getinfo("lib/arm64-v8a/libil2cpp.so").file_size)
