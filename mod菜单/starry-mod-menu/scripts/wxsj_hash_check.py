#!/usr/bin/env python3
"""v7.14 无限升级：哈希门常数交叉验证。
用法: wxsj_hash_check.py <WxsjMod.cpp> <官方 targetfuntional.dll>
校验 WxsjMod.cpp 里的 kOrigDllHash / kOrigDllLen 是否与官方底包一致。
常数写错的后果：运行时哈希门永远不匹配 → 静默 PASSTHROUGH（补丁完全不生效却看不出来）。"""
import re, sys

def fnv1a64(data: bytes) -> int:
    h = 0xcbf29ce484222325
    for b in data:
        h ^= b
        h = (h * 0x100000001b3) & 0xFFFFFFFFFFFFFFFF
    return h

src, dll = sys.argv[1], sys.argv[2]
txt = open(src, encoding='utf-8', errors='replace').read()
raw = open(dll, 'rb').read()

m = re.search(r'kOrigDllHash\s*=\s*0x([0-9a-fA-F]+)', txt)
if not m:
    print("ERROR: 无法从 %s 解析 kOrigDllHash" % src); sys.exit(1)
claimed_hash = int(m.group(1), 16)
m = re.search(r'kOrigDllLen\s*=\s*(\d+)', txt)
if not m:
    print("ERROR: 无法从 %s 解析 kOrigDllLen" % src); sys.exit(1)
claimed_len = int(m.group(1))

real_hash = fnv1a64(raw)
real_len = len(raw)
print("[wxsj-so] 哈希门: kOrigDllLen  code=%d official=%d" % (claimed_len, real_len))
print("[wxsj-so] 哈希门: kOrigDllHash code=0x%016x official=0x%016x  (%s)" % (claimed_hash, real_hash, dll.split('/')[-1]))

bad = False
if claimed_len != real_len:
    print("ERROR: kOrigDllLen=%d ≠ 官方底包长度 %d" % (claimed_len, real_len)); bad = True
if claimed_hash != real_hash:
    print("ERROR: kOrigDllHash 与官方底包整包 FNV-1a 64 不一致 → 运行时不会注入补丁"); bad = True
if bad:
    sys.exit(1)
print("[wxsj-so] 哈希门常数交叉验证 ✓")
