#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import os, re, base64, zlib, gzip, io

WC = "/tmp/bh3_recon/wvcache"
KW = ["秒杀", "无敌", "暴击", "血量", "伤害", "超级", "攻击", "暴率", "冷却", "攻速",
      "Category", "Toggle", "Slider", "feat", "FEATURE", "menu", "game/38", "38.html"]

def try_decompress(b):
    for fn in (lambda x: gzip.decompress(x),
               lambda x: zlib.decompress(x),
               lambda x: zlib.decompress(x, -15)):
        try:
            return fn(b)
        except Exception:
            pass
    return None

def cjk_strings(b, minlen=2):
    out = []
    cur = bytearray()
    i = 0
    while i < len(b):
        c = b[i]
        if c >= 0xe0:
            # try utf8 3-byte
            try:
                ch = b[i:i+3].decode("utf-8")
                if "\u4e00" <= ch <= "\u9fff":
                    cur += b[i:i+3]; i += 3; continue
            except Exception:
                pass
        if cur:
            if len(cur) // 3 >= minlen:
                out.append(cur.decode("utf-8", "ignore"))
            cur = bytearray()
        i += 1
    if len(cur) // 3 >= minlen:
        out.append(cur.decode("utf-8", "ignore"))
    return out

print("=== decode base64 blob A1ujRv8f7osKp6WBzBQAAAAASUVORK5CYII= ===")
blob = base64.b64decode("A1ujRv8f7osKp6WBzBQAAAAASUVORK5CYII=")
print("len=%d hex=%s" % (len(blob), blob.hex()))
print("ascii=%r" % blob)

print()
print("=== wvcache scan ===")
for root, _, files in os.walk(WC):
    for f in files:
        p = os.path.join(root, f)
        raw = open(p, "rb").read()
        body = raw
        dec = try_decompress(raw)
        if dec:
            body = dec
        hits = []
        for k in KW:
            kb = k.encode("utf-8")
            n = body.count(kb)
            if n:
                hits.append("%s x%d" % (k, n))
        cj = cjk_strings(body)
        cj = [s for s in cj if len(s) >= 2]
        tag = " [gzip]" if dec else ""
        if hits or cj:
            print("\n-- %s size=%d%s" % (f, len(raw), tag))
            if hits:
                print("   KW: " + ", ".join(hits))
            if cj:
                print("   CJK: " + " | ".join(dict.fromkeys(cj))[:600])
