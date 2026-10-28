import struct
blob = open(r"E:\nixiang\06_dumped\绿梦_20261027\fg_inflated.bin","rb").read()
BASE = 0x458
def w(va):
    o = va - BASE
    if o < 0 or o + 4 > len(blob): return None
    return struct.unpack_from("<I", blob, o)[0]
def decode_bl(va):
    x = w(va)
    if x is None: return None
    if (x & 0xFC000000) == 0x94000000:
        imm = x & 0x03FFFFFF
        if imm & 0x02000000: imm -= 0x04000000
        return ("bl", va + (imm << 2))
    if x == 0xD63F0000 or (x & 0xFFFFFC1F) == 0xD63F0000:
        return ("blr", None)
    if (x & 0xFFFFFC1F) == 0xD61F0000:
        return ("br", None)
    return None
for va in (0x2783a8, 0x27de88, 0x35c408, 0x2a3d4):
    print("=== ret 0x%x ===" % va)
    for back in range(1, 33):
        site = va - back * 4
        d = decode_bl(site)
        if d:
            print("   call at 0x%x -> %s %s (距 ret %d 条)" % (site, d[0], ("0x%x" % d[1]) if d[1] else "", back))
    # 附近是否有 svc
    hits = [ (va + k*4) for k in range(-40, 40) if w(va + k*4) == 0xD4000001 ]
    print("   svc within ±40:", ["0x%x" % h for h in hits])
