import struct, re, os
D = r'E:\nixiang\06_dumped\冒险与征途\aot\_bundle_data.bin'
d = open(D, 'rb').read()
print('decompressed size', len(d))
for name in [b'MZ', b'BSJB', b'Assembly-CSharp', b'CAB-', b'HybridCLR', b'\x00\x00\x00\x00']:
    ms = [m.start() for m in re.finditer(re.escape(name), d)]
    print('%-18s %d hits  first: %s' % (name.decode('latin-1'), len(ms), ['0x%X' % x for x in ms[:12]]))

print()
print('--- valid PE scan ---')
cnt = 0
for m in re.finditer(re.escape(b'MZ'), d):
    o = m.start()
    if o + 0x40 > len(d): continue
    pe = struct.unpack_from('<I', d, o + 0x3C)[0]
    if 0 < pe < 0x400 and o + pe + 24 <= len(d) and d[o+pe:o+pe+4] == b'PE\0\0':
        mach = struct.unpack_from('<H', d, o + pe + 4)[0]
        nsec = struct.unpack_from('<H', d, o + pe + 6)[0]
        print('  PE @0x%X machine=0x%X sections=%d' % (o, mach, nsec))
        cnt += 1
print('valid PE count =', cnt)

print()
print('--- around first BSJB ---')
m = re.search(re.escape(b'BSJB'), d)
if m:
    o = m.start()
    for i in range(max(0, o - 64), o + 96, 16):
        ch = d[i:i+16]
        print('%08X  %-48s %s' % (i, ' '.join('%02x' % c for c in ch),
                                  ''.join(chr(c) if 32 <= c < 127 else '.' for c in ch)))

print()
print('--- around first Assembly-CSharp ---')
m = re.search(re.escape(b'Assembly-CSharp'), d)
if m:
    o = m.start()
    for i in range(max(0, o - 96), o + 96, 16):
        ch = d[i:i+16]
        print('%08X  %-48s %s' % (i, ' '.join('%02x' % c for c in ch),
                                  ''.join(chr(c) if 32 <= c < 127 else '.' for c in ch)))
