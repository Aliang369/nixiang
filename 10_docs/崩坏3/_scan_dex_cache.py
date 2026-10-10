#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import re, glob, os

WORDS = ["秒杀", "无敌", "暴击", "超级血量", "血量", "不死", "无限", "秒", "伤害",
         "Toggle", "Category", "get_ret", "featNum", "initGame", "exp", "Slider",
         "menu", "Menu", "BUFF", "Buff", "BG", "bg888", "SDKloading"]

def scan_file(path, label):
    try:
        d = open(path, "rb").read()
    except Exception as e:
        print("ERR", path, e); return
    hits = {}
    for w in WORDS:
        wb = w.encode("utf-8")
        idxs = [m.start() for m in re.finditer(re.escape(wb), d)]
        if idxs:
            hits[w] = idxs
    if hits:
        print("[%s] size=%d" % (label, len(d)))
        for w, idxs in hits.items():
            print("   %-12s x%d  first@0x%x" % (w, len(idxs), idxs[0]))
            for i in idxs[:2]:
                ctx = d[max(0, i - 40):i + 60]
                print("        %r" % ctx)

print("===== DEX =====")
for f in sorted(glob.glob("/tmp/bh3_recon/dex/*.dex")):
    scan_file(f, os.path.basename(f))

print("\n===== wvcache =====")
for f in sorted(glob.glob("/tmp/bh3_recon/wvcache/wvcache/*")):
    if os.path.isfile(f):
        scan_file(f, os.path.basename(f))

print("\n===== APK assets html/js/json =====")
import zipfile
z = zipfile.ZipFile("/tmp/bh3_recon/NGHSoD_base.apk")
names = [n for n in z.namelist() if n.lower().endswith((".html", ".js", ".json", ".xml")) and "assets/" in n.lower()]
print("candidate asset files:", len(names))
for n in names[:60]:
    print("   ", n)
