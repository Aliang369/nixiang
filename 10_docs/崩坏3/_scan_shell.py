#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import re, struct, sys

SO = "/tmp/bh3_recon/shell/libSDKloading.so"
data = open(SO, "rb").read()
print("=== libSDKloading.so size=%d ===" % len(data))

# ---- ELF header ----
print("magic:", data[:4].hex(), "class:", data[4], "machine:", struct.unpack_from("<H", data, 0x12)[0])
e_shoff = struct.unpack_from("<Q", data, 0x28)[0]
e_shentsize = struct.unpack_from("<H", data, 0x3a)[0]
e_shnum = struct.unpack_from("<H", data, 0x3c)[0]
e_shstrndx = struct.unpack_from("<H", data, 0x3e)[0]
print("e_shoff=0x%x e_shentsize=%d e_shnum=%d e_shstrndx=%d" % (e_shoff, e_shentsize, e_shnum, e_shstrndx))

def sh(i):
    o = e_shoff + i * e_shentsize
    return struct.unpack_from("<IIQQQQIIQQ", data, o)

sections = [sh(i) for i in range(e_shnum)]
shstr = sections[e_shstrndx]
def sname(off):
    end = data.index(b"\x00", shstr[4] + off)
    return data[shstr[4] + off:end].decode("latin1")

print("--- sections ---")
for i, s in enumerate(sections):
    nm = sname(s[0]) if s[0] < shstr[5] else "?"
    if s[1] != 8:  # skip NOBITS print? keep all
        print("[%2d] %-18s type=%2d addr=0x%08x off=0x%08x size=0x%x" % (i, nm, s[1], s[3], s[4], s[5]))

# ---- hex offset strings like 0x5F16228 ----
print("--- hex-address-like ASCII strings ---")
hexpat = re.compile(rb"0[xX][0-9a-fA-F]{4,12}")
seen = set()
for m in hexpat.finditer(data):
    s = m.group().decode()
    if s.lower() not in seen:
        seen.add(s.lower())
        print("0x%x: %s" % (m.start(), s))
print("total distinct hex strings:", len(seen))

# ---- feature keyword strings ----
print("--- feature keyword ASCII strings ---")
kw = [b"kill", b"Kill", b"damage", b"Damage", b"HP", b"Crit", b"crit", b"invinc", b"Immortal",
      b"get_ret", b"featNum", b"feat_num", b"menu", b"Menu", b"il2cpp", b"libil2cpp",
      b"Method", b"method", b"Offset", b"offset", b"patch", b"Patch", b"patchByte", b"Toggle",
      b"Category", b"Switch", b"BUFF", b"Buff", b"buff"]
for k in kw:
    idxs = [m.start() for m in re.finditer(re.escape(k), data)]
    if idxs:
        print("%-12s x%d  first@0x%x" % (k.decode(), len(idxs), idxs[0]))
        for i in idxs[:4]:
            ctx = data[max(0, i - 24):i + 40]
            print("    0x%x: %r" % (i, ctx))

# ---- CJK strings (UTF-8) ----
print("--- CJK UTF-8 strings ---")
cjk = re.compile(rb"(?:[\xe4-\xe9][\x80-\xbf][\x80-\xbf]){2,}")
cnt = 0
for m in cjk.finditer(data):
    try:
        s = m.group().decode("utf-8")
    except Exception:
        continue
    if any(ch in s for ch in "秒杀无敌血量暴击伤害开挂功能菜单开启关闭"):
        print("0x%x: %s" % (m.start(), s))
        cnt += 1
        if cnt > 60:
            break

# ---- 'image/class/method' style bindings ----
print("--- binding-ish strings (contain '.dll' or 'Assembly' or '::') ---")
for pat in [rb"[\x20-\x7e]{3,40}\.dll", rb"[\x20-\x7e]{3,60}::[\x20-\x7e]{1,40}",
            rb"Assembly-CSharp[\x20-\x7e]{0,30}"]:
    for m in re.finditer(pat, data):
        print("0x%x: %r" % (m.start(), m.group()))
