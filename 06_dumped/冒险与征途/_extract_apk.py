import zipfile, os, hashlib, sys, json

apk = r"E:\nixiang\02_apks\冒险与征途\冒险与征途_1.5.9_binge_base.apk"
out = r"E:\nixiang\06_dumped\冒险与征途\apk_extract"
os.makedirs(out, exist_ok=True)

z = zipfile.ZipFile(apk)

want_exact = {'AndroidManifest.xml'}
want_prefix = ('lib/', 'assets/bin/')
want_assets_shallow = 1

names = []
for i in z.infolist():
    n = i.filename
    if n.endswith('/'):
        continue
    if n in want_exact:
        names.append(n); continue
    if n.startswith('lib/'):
        names.append(n); continue
    if n.startswith('assets/bin/'):
        names.append(n); continue
    if n.startswith('classes') and n.endswith('.dex'):
        names.append(n); continue
    if n.startswith('assets/') and not n.startswith('assets/dataroot/') and n.count('/') <= 2:
        names.append(n); continue

report = []
for n in names:
    dest = os.path.join(out, n.replace('/', os.sep))
    os.makedirs(os.path.dirname(dest), exist_ok=True)
    with z.open(n) as src, open(dest, 'wb') as dst:
        while True:
            b = src.read(1 << 20)
            if not b:
                break
            dst.write(b)
    h = hashlib.md5(open(dest, 'rb').read()).hexdigest()
    sz = os.path.getsize(dest)
    report.append((sz, h, n))
    print(f"{sz:12d}  {h}  {n}")

with open(os.path.join(out, '_md5.txt'), 'w', encoding='utf-8') as f:
    for sz, h, n in sorted(report, key=lambda r: r[2]):
        f.write(f"{h}  {sz:12d}  {n}\n")
print("WROTE", out)
