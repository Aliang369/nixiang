#!/usr/bin/env python3
"""v7.16 无限升级：哈希门常数交叉验证。
用法: wxsj_hash_check.py <WxsjMod.cpp> <官方 targetfuntional.dll> [补丁 targetfuntional_dll.bin]
校验 WxsjMod.cpp 里的三个常数是否与实际文件一致：
  kOrigDllLen    = 官方底包长度（装载数组长度门）
  kOrigDllHash   = 官方底包整包 FNV-1a 64（注入门）
  kPatchedBufHash= 「原地注入后缓冲区」整包 FNV-1a 64 = FNV(补丁 dll + 尾零补到官方长度)
                   用途：识别同一数组被二次装载（v7.16 防「第二次登录就被禁止进入」）
常数写错的后果：装载门永远不匹配 → 要么静默不注入，要么把正常二次装载误判成「官方换版」封入口。
补丁 dll 缺省取 <WxsjMod.cpp 所在目录>/../targetfuntional_dll.bin。"""
import os, re, sys

def fnv1a64(data: bytes) -> int:
    h = 0xcbf29ce484222325
    for b in data:
        h ^= b
        h = (h * 0x100000001b3) & 0xFFFFFFFFFFFFFFFF
    return h

def pick(txt, name):
    m = re.search(name + r'\s*=\s*(0x[0-9a-fA-F]+|\d+)', txt)
    if not m:
        print("ERROR: 无法从源码解析 %s" % name)
        sys.exit(1)
    v = m.group(1)
    return int(v, 16) if v.startswith('0x') else int(v)

src, dll = sys.argv[1], sys.argv[2]
patch = sys.argv[3] if len(sys.argv) > 3 else os.path.join(os.path.dirname(os.path.abspath(src)),
                                                          '..', 'targetfuntional_dll.bin')
txt = open(src, encoding='utf-8', errors='replace').read()
raw = open(dll, 'rb').read()

claimed_hash = pick(txt, 'kOrigDllHash')
claimed_len = pick(txt, 'kOrigDllLen')
claimed_pbuf = pick(txt, 'kPatchedBufHash')

real_hash = fnv1a64(raw)
real_len = len(raw)
print("[wxsj-so] 哈希门: kOrigDllLen  code=%d official=%d" % (claimed_len, real_len))
print("[wxsj-so] 哈希门: kOrigDllHash code=0x%016x official=0x%016x  (%s)"
      % (claimed_hash, real_hash, os.path.basename(dll)))

bad = False
if claimed_len != real_len:
    print("ERROR: kOrigDllLen=%d ≠ 官方底包长度 %d" % (claimed_len, real_len)); bad = True
if claimed_hash != real_hash:
    print("ERROR: kOrigDllHash 与官方底包整包 FNV-1a 64 不一致 → 运行时不会注入补丁"); bad = True

if not os.path.isfile(patch):
    print("WARN: 补丁 dll 不存在，跳过 kPatchedBufHash 校验: %s" % patch)
else:
    pb = open(patch, 'rb').read()
    if len(pb) > real_len:
        print("ERROR: 补丁 dll %d > 官方底包 %d" % (len(pb), real_len)); bad = True
    else:
        buf = bytearray(pb) + bytes(real_len - len(pb))   # 原地注入：尾零补满，长度不变
        real_pbuf = fnv1a64(bytes(buf))
        print("[wxsj-so] 哈希门: kPatchedBufHash code=0x%016x real=0x%016x  (补丁 %d + 尾零 %d)"
              % (claimed_pbuf, real_pbuf, len(pb), real_len - len(pb)))
        if claimed_pbuf != real_pbuf:
            print("ERROR: kPatchedBufHash 与「原地注入后缓冲区」不一致 → 二次装载会被误判为官方换版（封入口）")
            bad = True

if bad:
    sys.exit(1)
print("[wxsj-so] 哈希门常数交叉验证 ✓")
