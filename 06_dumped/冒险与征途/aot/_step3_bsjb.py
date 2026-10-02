import struct, re
P = r'E:\nixiang\06_dumped\冒险与征途\aot\hot1.unity3d'
b = open(P, 'rb').read()

def hd(o, n=64, label=''):
    print('--- 0x%X %s ---' % (o, label))
    for i in range(o, o + n, 16):
        chunk = b[i:i+16]
        print('%08X  %-48s %s' % (i, ' '.join('%02x' % c for c in chunk),
                                  ''.join(chr(c) if 32 <= c < 127 else '.' for c in chunk)))

hd(0x3FC300, 96, 'around BSJB')

print()
print('--- stream name occurrences ---')
for name in [b'#~', b'#Strings', b'#Blob', b'#US', b'#GUID', b'#-']:
    offs = [m.start() for m in re.finditer(re.escape(name), b)][:12]
    print('%-10s %d hits, first: %s' % (name.decode(), len([m for m in re.finditer(re.escape(name), b)]),
                                        ['0x%X' % x for x in offs]))

print()
print('--- "Assembly-CSharp" / "HybridCLR" occurrences ---')
for name in [b'Assembly-CSharp', b'HybridCLR', b'mscoree.dll', b'CAB-']:
    ms = [m.start() for m in re.finditer(re.escape(name), b)]
    print('%-18s %d hits: %s' % (name.decode(), len(ms), ['0x%X' % x for x in ms[:20]]))

print()
print('--- hex around 0x772 ---')
hd(0x740, 112, 'CAB name area')
