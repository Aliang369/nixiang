#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
敢达争锋对决 (gdzfdj) — 外科 zip 组装（不 apktool 整包）

替换/新增条目：
  AndroidManifest.xml                    ← 打好的二进制 Manifest（patch_gdzfdj_manifest.py）
  classes3.dex                           ← javac+d8 出的星光菜单 dex（官包只有 classes.dex + classes2.dex）
  lib/arm64-v8a/libstarry_magic.so        ← ndk-build 出的 mod so
其余条目按原样搬（保留 compress_type / external_attr / create_system），
跳过 META-INF/（旧签名，稍后 apksigner 重签），resources.arsc 强制 ZIP_STORED。

用法:
  python gdzfdj_surgical.py --base <orig apk> --out <out apk> \
      [--manifest <AndroidManifest.patched.xml>] [--dex <classes3.dex>] [--so <libstarry_magic.so>]
输出后由调用方执行： zipalign -f -p 4 in out && apksigner sign
"""
import argparse
import hashlib
import os
import zipfile

DEX_SLOT = "classes3.dex"
SO_SLOT = "lib/arm64-v8a/libstarry_magic.so"
SKIP_PREFIX = ("META-INF/",)
# 必须保持未压缩（且 4 字节对齐）的条目
FORCE_STORED = ("resources.arsc",)


def sha256(p):
    h = hashlib.sha256()
    with open(p, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--manifest", default=None)
    ap.add_argument("--dex", default=None)
    ap.add_argument("--so", default=None)
    args = ap.parse_args()

    repl = {}
    if args.manifest:
        repl["AndroidManifest.xml"] = args.manifest
    if args.dex:
        repl[DEX_SLOT] = args.dex
    if args.so:
        repl[SO_SLOT] = args.so

    zin = zipfile.ZipFile(args.base, "r")
    asserted = {}
    with zipfile.ZipFile(args.out, "w", zipfile.ZIP_DEFLATED, allowZip64=True) as zout:
        for info in zin.infolist():
            name = info.filename
            if name.startswith(SKIP_PREFIX):
                continue
            if name in ("AndroidManifest.xml", "resources.arsc"):
                asserted[name] = hashlib.sha256(zin.read(name)).hexdigest()
            src = repl.pop(name, None)
            data = open(src, "rb").read() if src else zin.read(name)
            ct = info.compress_type
            if name in FORCE_STORED or (src and (name.endswith(".so") or name.endswith(".dex"))):
                ct = zipfile.ZIP_STORED
            elif name == "AndroidManifest.xml":
                ct = zipfile.ZIP_STORED
            zi = zipfile.ZipInfo(name, date_time=info.date_time)
            zi.compress_type = ct
            zi.external_attr = info.external_attr
            zi.create_system = info.create_system
            zi.internal_attr = info.internal_attr
            if ct == zipfile.ZIP_STORED:
                zi.compress_type = zipfile.ZIP_STORED
            zout.writestr(zi, data)
            if src:
                print("[gdzfdj-surgical] replaced %-40s <- %s (%d B, ctype=%d)" % (
                    name, os.path.basename(src), len(data), ct))
        # 新增尚未存在的条目
        for name, src in repl.items():
            data = open(src, "rb").read()
            ct = zipfile.ZIP_STORED
            zi = zipfile.ZipInfo(name)
            zi.compress_type = ct
            zi.external_attr = 0o100644 << 16
            zi.create_system = 3
            zout.writestr(zi, data)
            print("[gdzfdj-surgical] added    %-40s <- %s (%d B, STORED)" % (
                name, os.path.basename(src), len(data)))

    names = zipfile.ZipFile(args.out).namelist()
    for need in repl_names(args):
        if need not in names:
            raise SystemExit("[gdzfdj-surgical] ERROR: missing %s in output" % need)
    print("[gdzfdj-surgical] out=%s (%d B, sha256=%s, entries=%d)" % (
        args.out, os.path.getsize(args.out), sha256(args.out)[:16], len(names)))
    print("[gdzfdj-surgical] resources.arsc sha256(orig)=%s  (须与新包一致)" % asserted.get("resources.arsc", "-"))


def repl_names(args):
    out = ["AndroidManifest.xml"]
    if args.dex:
        out.append(DEX_SLOT)
    if args.so:
        out.append(SO_SLOT)
    return out


if __name__ == "__main__":
    main()
