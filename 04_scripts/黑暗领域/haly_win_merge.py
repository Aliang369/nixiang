
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import hashlib, sys

import sys
# 用法: python haly_win_merge.py <base.apk> <out.apk> <manifest> <classes2.dex> <classes4.dex> <so> <libcxx.so>
base = Path(sys.argv[1])
out = Path(sys.argv[2])
repl = {
    'AndroidManifest.xml': Path(sys.argv[3]).read_bytes(),
    'classes2.dex': Path(sys.argv[4]).read_bytes(),
    'classes4.dex': Path(sys.argv[5]).read_bytes(),
    'lib/arm64-v8a/libstarry_magic.so': Path(sys.argv[6]).read_bytes(),
    'lib/arm64-v8a/libc++_shared.so': Path(sys.argv[7]).read_bytes(),
}
if out.exists():
    out.unlink()
seen = set()
with ZipFile(base, 'r') as zin, ZipFile(out, 'w', ZIP_DEFLATED) as zout:
    for info in zin.infolist():
        n = info.filename
        if n.startswith('META-INF/'):
            continue
        data = repl.get(n)
        if data is not None:
            seen.add(n)
            zout.writestr(info, data)
        else:
            zout.writestr(info, zin.read(n))
    for name, data in repl.items():
        if name not in seen:
            zout.writestr(name, data)
            print('added', name)
print('replaced', sorted(seen))
print('out_bytes', out.stat().st_size)
print('out_md5', hashlib.md5(out.read_bytes()).hexdigest())

z = ZipFile(out)
zo = ZipFile(base)
for n in ('resources.arsc', 'classes.dex', 'classes3.dex'):
    a = hashlib.sha256(zo.read(n)).hexdigest()
    b = hashlib.sha256(z.read(n)).hexdigest()
    print(n, 'intact' if a == b else 'CHANGED!')
so = z.read('lib/arm64-v8a/libstarry_magic.so')
print('so_has_haly', b'[Haly]' in so)
print('classes4_md5', hashlib.md5(z.read('classes4.dex')).hexdigest())
print('MERGE_DONE')
