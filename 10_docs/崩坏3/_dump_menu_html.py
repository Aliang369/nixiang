#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import os

WC = "/tmp/bh3_recon/wvcache/wvcache"
OUT = "/tmp/bh3_recon/menu_out"
os.makedirs(OUT, exist_ok=True)

for f in sorted(os.listdir(WC)):
    p = os.path.join(WC, f)
    if not os.path.isfile(p):
        continue
    d = open(p, "rb").read()
    i = d.find(b"<!doctype")
    if i < 0:
        i = d.find(b"<!DOCTYPE")
    if i < 0:
        continue
    s = d[i:].decode("utf-8", "replace")
    with open(os.path.join(OUT, f + ".html"), "w", encoding="utf-8") as w:
        w.write(s)
    print("wrote %s  html_len=%d" % (f, len(s)))
