import struct
d = open(r"E:\_build\lvmeng\fg_live.bin","rb").read()
base = 0x400022800000
addrs = []
for i in range(0, len(d) - 4, 4):
    if struct.unpack_from("<I", d, i)[0] == 0xD4000001:
        addrs.append(base + i)
print("count", len(addrs))
open(r"E:\_build\lvmeng\svc_addrs.txt","w").write("\n".join("0x%x" % a for a in addrs))
print(addrs[:5], "...", addrs[-3:])
