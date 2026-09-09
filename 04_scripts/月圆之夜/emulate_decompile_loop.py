#!/usr/bin/env python3.11
# 复刻 Il2CppDumper decompiler 类型循环, 定位 image 级异常的越界字段
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = open(P, "rb").read()
u32 = lambda o: struct.unpack_from("<I", d, o)[0]
i32 = lambda o: struct.unpack_from("<i", d, o)[0]
u16 = lambda o: struct.unpack_from("<H", d, o)[0]

TD, NTYPES = 0x8AFD34, 12626
IM = 0x9BF164
NTYPE_IDX = 49223                    # typesCount (binary MR)
N_IFACE = 21624 // 4                 # interfaceIndices 表项数
N_NESTED = 18228 // 4                # nestedTypes (u32 数组)
N_IFO = 103528 // 8                  # interfaceOffsets (8B pair)
N_GCONT = 58704 // 12                # genericContainers 12B

fails = []
for i in range(NTYPES):
    b = TD + i * 88
    parent = i32(b + 16)
    ifaceS = i32(b + 52)
    iCnt = u16(b + 76)
    nestedS = i32(b + 48)
    nCnt = u16(b + 72)
    ifoS = i32(b + 60)
    ioCnt = u16(b + 78)
    gcont = i32(b + 24)
    errs = []
    if not (0 <= parent < NTYPE_IDX):
        errs.append("parent=%d" % parent)
    if iCnt and not (0 <= ifaceS and ifaceS + iCnt <= N_IFACE):
        errs.append("iface[%d+%d>%d]" % (ifaceS, iCnt, N_IFACE))
    elif iCnt:
        for k in range(iCnt):
            v = u32(0x80F7F4 + (ifaceS + k) * 4)
            if v >= NTYPE_IDX:
                errs.append("ifaceVal[%d]=%d" % (k, v))
                break
    if nCnt and not (0 <= nestedS and nestedS + nCnt <= N_NESTED):
        errs.append("nested[%d+%d>%d]" % (nestedS, nCnt, N_NESTED))
    elif nCnt:
        for k in range(nCnt):
            v = u32(0x80B0C0 + (nestedS + k) * 4)
            if v >= NTYPES:
                errs.append("nestedVal=%d" % v)
                break
    if ioCnt and not (0 <= ifoS and ifoS + ioCnt <= N_IFO):
        errs.append("ifo[%d+%d>%d]" % (ifoS, ioCnt, N_IFO))
    if gcont != -1 and not (0 <= gcont < N_GCONT):
        errs.append("gcont=%d" % gcont)
    if errs:
        fails.append((i, errs))

print("越界类型总数:", len(fails))
for i, e in fails[:40]:
    print(i, e)
# 按 image 归组统计
img_fail = {}
for i, e in fails:
    for k in range(102):
        ts = u32(IM + k * 40 + 8)
        tc = u32(IM + k * 40 + 12)
        if ts <= i < ts + tc:
            img_fail[k] = img_fail.get(k, 0) + 1
            break
print("受影响 image 数:", len(img_fail))
print(sorted(img_fail.items())[:50])
