#!/usr/bin/env python3.11
# 全表定位 (token 扫描 + 衔接验证) -> 输出表地图
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)

def longest_run(token_hi, stride, min_count=10):
    """全文件找 (token_hi<<24|rid) 的最长等距递增链
    返回 (tok_pos_of_rid1, count, rid_min) 或 None"""
    rid = 1
    best = None
    # 找所有 rid=N 起点: 用每个命中的 token 值做锚
    pos = 0
    hits = []
    pat = None
    # 直接扫: 找所有形如 0xTT000001 的位置
    pat = struct.pack("<I", (token_hi << 24) | 1)
    p = 0
    while True:
        i = d.find(pat, p)
        if i < 0:
            break
        # 验证序列
        n = 1
        while True:
            t = struct.unpack_from("<I", d, i + n * stride)[0] if i + (n + 1) * stride <= FS else 0
            if (t >> 24) == token_hi and (t & 0xFFFFFF) == n + 1:
                n += 1
            else:
                break
        b = 0
        while i - (b + 1) * stride >= 0:
            t = struct.unpack_from("<I", d, i - (b + 1) * stride)[0]
            if (t >> 24) == token_hi and (t & 0xFFFFFF) == 1 - (b + 1) == 0:
                b += 1  # rid 不会小于1, 除非表从0开始
            else:
                break
        if best is None or n > best[0]:
            best = (n, i)
        p = i + 4
    if best is None:
        return None
    n, i = best
    return i, n, stride

print("== 最长 token 链 (按候选 stride) ==")
for name, thi, strides in [
    ("events", 0x14, [24]),
    ("properties", 0x17, [20]),
    ("methods", 0x06, [32, 36]),
    ("fields", 0x04, [12]),
    ("parameters", 0x08, [12]),
    ("typeDefs", 0x02, [88, 92]),
    ("fieldRefs?", 0x0A, [8, 12]),
]:
    for stride in strides:
        r = longest_run(thi, stride)
        if r:
            i, n, s = r
            # 结构起点 = token位置 - (stride - 4) 若 token 在结构尾
            print(f"{name:10s} stride={s}: tok@{i:#x} rid1, count={n}, 若token在尾: 表起点={i-(s-4):#x}")
        else:
            print(f"{name:10s} stride={stride}: 未命中")
