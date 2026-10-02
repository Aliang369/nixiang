import struct, os, sys, io

BASE = r"E:\nixiang\06_dumped\冒险与征途"
p = os.path.join(BASE, r"apk_extract\lib\arm64-v8a\libSDKloading.so")
d = open(p, 'rb').read()

def sections(d):
    e_shoff = struct.unpack_from('<Q', d, 0x28)[0]
    e_shentsize = struct.unpack_from('<H', d, 0x3A)[0]
    e_shnum = struct.unpack_from('<H', d, 0x3C)[0]
    e_shstrndx = struct.unpack_from('<H', d, 0x3E)[0]
    raw = []
    for i in range(e_shnum):
        o = e_shoff + i * e_shentsize
        name, typ, flags, addr, offset, size, link, info, align, entsize = struct.unpack_from('<IIQQQQIIQQ', d, o)
        raw.append(dict(name=name, type=typ, flags=flags, addr=addr, off=offset, size=size,
                        link=link, info=info, entsize=entsize))
    return raw

secs = sections(d)
# need shstrtab
e_shoff = struct.unpack_from('<Q', d, 0x28)[0]
e_shentsize = struct.unpack_from('<H', d, 0x3A)[0]
e_shstrndx = struct.unpack_from('<H', d, 0x3E)[0]
o = e_shoff + e_shstrndx * e_shentsize
shstr_off, shstr_size = struct.unpack_from('<QQ', d, o + 0x18)
shstr = d[shstr_off:shstr_off + shstr_size]
for s in secs:
    s['sname'] = shstr[s['name']:shstr.find(b'\0', s['name'])].decode('latin1')

byname = {s['sname']: s for s in secs}
dynsym = byname['.dynsym']
dynstr = byname['.dynstr']
strtab = d[dynstr['off']:dynstr['off'] + dynstr['size']]
n = dynsym['size'] // 24
print("dynsym count:", n)
syms = []
for i in range(n):
    o = dynsym['off'] + i * 24
    st_name, st_info, st_other, st_shndx, st_value, st_size = struct.unpack_from('<IBBHQQ', d, o)
    nm = strtab[st_name:strtab.find(b'\0', st_name)].decode('latin1')
    syms.append((st_value, st_size, st_info >> 4, st_info & 0xf, st_shndx, nm))

# typed = has a section index (defined), STT_FUNC=2
funcs = [s for s in syms if s[2] == 2]
objs = [s for s in syms if s[2] == 1]
print("FUNC:", len(funcs), "OBJECT:", len(objs))

with open(os.path.join(BASE, "sdkloading_dynsym.txt"), 'w', encoding='utf-8') as f:
    for v, sz, t, b, sh, nm in sorted(syms, key=lambda s: s[0]):
        f.write(f"{v:08x} {sz:6d} t={t} shndx={sh} {nm}\n")

def show(title, pred, limit=200):
    hit = [s for s in syms if pred(s)]
    print(f"--- {title}: {len(hit)}")
    for v, sz, t, b, sh, nm in hit[:limit]:
        print(f"  {v:08x} sz={sz:6d} t={t} {nm}")

import re
show("non-ascii / chinese names", lambda s: any(ord(c) > 127 for c in s[5]), 300)
show("keyword names", lambda s: re.search(r'(?i)hook|patch|god|invinc|dead|kill|hp|damage|speed|cheat|mod|flag|switch|binge|kaiguan', s[5]) and not s[5].startswith('_ZN'), 300)
