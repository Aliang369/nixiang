
import re, hashlib, binascii
cands=set()
def add(b):
    if isinstance(b,str): b=b.encode()
    if 6 <= len(b) <= 96: cands.add(b)
for path in ['/tmp/shell/libsecret.so',
             '/Users/aliang/Documents/nixiang/09_test/动物大联盟/unpack_20261105/pull/no_backup/mlib/arm64-v8a/libcocos2djs.so']:
    d=open(path,'rb').read()
    for m in re.finditer(rb'[ -~]{6,96}', d):
        add(m.group())
cert_hex='de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a'
for s in [cert_hex, cert_hex.upper(), binascii.unhexlify(cert_hex),
          binascii.unhexlify('01e45f2b5ae835fe015cb07c8dc8bf7d3c09e421ba6f67c24450b437d341ce61'),
          b'01e45f2b5ae835fe015cb07c8dc8bf7d3c09e421ba6f67c24450b437d341ce61',
          b'73f80704c19fffe0d03ab8d7f7becc2d',
          binascii.unhexlify('73f80704c19fffe0d03ab8d7f7becc2d'),
          b'com.fightingstars.games.animal', b'fightingstars', b'bingo', b'dwdlm',
          b'9E3779B9', b'dwres']:
    add(s)
out=sorted(cands)
with open('/tmp/keys.txt','wb') as f:
    for c in out:
        f.write(c+b'\n')
print('candidates', len(out))
