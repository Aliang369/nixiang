#!/usr/bin/env python3.11
# 月圆之夜 metadata 头部重建: token 扫描定位全部表 -> 生成 ver24.5 标准头部
# 已知: 字面量表@0x100 (17658条), 字面量数据@0xA0000 (0x183000), 名字区@0x23000 (0x7C000)
import struct, sys

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)
U = struct.Struct("<I")

def find_token_table(token_hi, first_rid, stride, lo=0, hi=FS):
    """找 token 序列表: 连续 rid, 每条 stride 字节, token 位于结构内某处
    返回 (table_start, count, token_offset_in_struct)"""
    # 先搜 first_rid 的 token 值位置
    tok = (token_hi << 24) | first_rid
    pat = struct.pack("<I", tok)
    results = []
    start = lo
    while True:
        i = d.find(pat, start, hi)
        if i < 0:
            break
        # 验证序列: i, i+stride, i+2*stride 处 token 递增
        ok = 0
        for k in range(1, 6):
            t = struct.unpack_from("<I", d, i + k * stride)[0] if i + k * stride + 4 <= FS else 0
            if (t >> 24) == token_hi and (t & 0xFFFFFF) == first_rid + k:
                ok += 1
        if ok >= 4:
            results.append(i)
        start = i + 4
    return results

def count_forward(off, stride, token_hi, first_rid):
    """off = 首个 token 的绝对位置, 向后数连续同表 token"""
    n = 0
    while off + (n + 1) * stride + 4 <= FS:
        t = struct.unpack_from("<I", d, off + (n + 1) * stride)[0]
        if (t >> 24) == token_hi and (t & 0xFFFFFF) == first_rid + n + 1:
            n += 1
        else:
            break
    return n

def count_backward(off, stride, token_hi, first_rid):
    n = 0
    while off - (n + 1) * stride >= 0:
        t = struct.unpack_from("<I", d, off - (n + 1) * stride)[0]
        if (t >> 24) == token_hi and (t & 0xFFFFFF) == first_rid - n - 1:
            n += 1
        else:
            break
    return n

# ---- token 表特征: (token_hi, 最小rid, 结构stride列表候选) ----
# ver24.5: events 24B(token@+20), properties 20B(token@+16),
#          methods 36B(token@+24), fields 12B(token@+8), parameters 12B(token@+4),
#          typeDefinitions 92B?(token@最后4)
tables = {}
for name, thi, stride, tok_off in [
    ("events", 0x14, 24, 20),
    ("properties", 0x17, 20, 16),
    ("methods", 0x06, 36, 24),
    ("fields", 0x04, 12, 8),
    ("parameters", 0x08, 12, 4),
    ("typeDefs", 0x02, 92, 88),
    ("typeDefs88", 0x02, 88, 84),
]:
    hits = find_token_table(thi, 1, stride)
    best = None
    for h in hits:
        fwd = count_forward(h, stride, thi, 1)
        bwd = count_backward(h, stride, thi, 1)
        tok_start = h - bwd * stride          # token 位置
        start = tok_start - tok_off           # 结构起点
        cnt = fwd + bwd + 1
        if best is None or cnt > best[1]:
            best = (start, cnt)
    if best:
        tables[name] = (best[0], best[1], stride, tok_off)
        print(f"{name:12s}: start={best[0]:#x} count={best[1]} stride={stride}")

# 附加: methods 可能 stride 不同, 多试几档
if "methods" not in tables:
    print("methods 36B 未命中, 尝试其他 stride")
    for stride in (32, 40, 44, 48):
        hits = find_token_table(0x06, 1, stride)
        for h in hits[:3]:
            fwd = count_forward(h, stride, 0x06, 1)
            bwd = count_backward(h, stride, 0x06, 1)
            print(f"  stride={stride}: tok@{h:#x} start={h - bwd*stride - (stride-4):#x} count={fwd+bwd+1}")
EOF_MARK = None
