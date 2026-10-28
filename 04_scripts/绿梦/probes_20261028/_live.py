import struct
d = open(r"E:\_build\lvmeng\fg_live.bin","rb").read()
def word(img_off):
    return struct.unpack_from("<I", d, img_off)[0]
print("live dump size", len(d))
for name, off in [("exit svc site", 0x35c404), ("0x35be04", 0x35be04), ("0x35c904", 0x35c904), ("0x35cf04", 0x35cf04)]:
    w = word(off)
    prev = word(off - 4)
    print("%-14s img+%#08x = %#010x  prev=%#010x" % (name, off, w, prev))
# 统计活镜像里还剩多少 svc
import re
cnt = 0
svcs = []
for i in range(0, len(d) - 4, 4):
    if struct.unpack_from("<I", d, i)[0] == 0xD4000001:
        cnt += 1
        if i % 0x100 == 4:
            svcs.append(i)
print("剩余 svc #0 计数:", cnt, " 其中 0x..0404 对齐的:", len(svcs), svcs[:12])
