#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""造 mini APK（只含 AndroidManifest.xml + resources.arsc，ZIP_STORED）用于 aapt2 快速校验 manifest 合法性。

用法: mkmini_apk.py <AndroidManifest.xml> <resources.arsc> <out.apk>
"""
import sys
import zipfile

manifest, arsc, out = sys.argv[1], sys.argv[2], sys.argv[3]
with zipfile.ZipFile(out, "w") as z:
    for name, path in (("AndroidManifest.xml", manifest), ("resources.arsc", arsc)):
        zi = zipfile.ZipInfo(name, (2026, 1, 1, 0, 0, 0))
        zi.compress_type = zipfile.ZIP_STORED
        zi.external_attr = 0o644 << 16
        z.writestr(zi, open(path, "rb").read())
print("[mkmini] wrote %s" % out)
