import sys, zipfile, hashlib, shutil, os
from pathlib import Path

src = Path(r"E:\nixiang\02_apks\绿梦\绿梦_1.0.5_official.apk")
work = Path(r"E:\_build\lvmeng")
dst = work / "unsigned.apk"

mfonly = zipfile.ZipFile(work / "mf_only.apk")
mf = mfonly.read("AndroidManifest.xml")
mfonly.close()
c4 = (work / "classes4.dex").read_bytes()
c2p = work / "classes2_patched.dex"   # 2026-10-28 [lvmeng] TapTap 登录改走 Web


extras = {}
libdir = work / "lib"
if libdir.is_dir():
    for p in libdir.rglob("*"):
        if p.is_file():
            extras["lib/" + str(p.relative_to(libdir)).replace("\\", "/")] = p
for sub in ("lvmeng", "certs"):
    base = work / "assets" / sub
    if base.is_dir():
        for p in base.rglob("*"):
            if p.is_file():
                extras["assets/%s/%s" % (sub, str(p.relative_to(base)).replace("\\", "/"))] = p
print("extras:", sorted(extras))

untouched = ["classes.dex", "classes3.dex", "resources.arsc"]
if not c2p.is_file():
    untouched.insert(1, "classes2.dex")
zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name == "classes2.dex" and c2p.is_file():
        continue
    info = zipfile.ZipInfo(name, item.date_time)
    info.external_attr = item.external_attr
    info.create_system = 3
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    if name == "AndroidManifest.xml":
        zout.writestr(info, mf)
    else:
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    n += 1

info = zipfile.ZipInfo("classes4.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c4)
n += 1

if c2p.is_file():
    info2 = zipfile.ZipInfo("classes2.dex")
    info2.compress_type = zipfile.ZIP_DEFLATED
    info2.create_system = 3
    zout.writestr(info2, c2p.read_bytes())
    n += 1

for name in sorted(extras):
    info = zipfile.ZipInfo(name)
    # fg_orig.apk 必须 STORED（native 侧按本地头+数据偏移直接 pread，不解压）
    info.compress_type = zipfile.ZIP_STORED if name.endswith("assets/lvmeng/fg_orig.apk") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    if name.endswith("assets/lvmeng/fg_orig.apk"):
        with open(extras[name], "rb") as fi, zout.open(info, "w") as fo:
            import shutil as _sh
            _sh.copyfileobj(fi, fo, 1 << 20)
    else:
        zout.writestr(info, extras[name].read_bytes())
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src); zout2 = zipfile.ZipFile(dst)
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, "changed: " + name
names = zout2.namelist()
for need in ["classes4.dex", "lib/arm64-v8a/libstarry_magic.so", "AndroidManifest.xml", "assets/lvmeng/fg_orig.apk"]:
    assert need in names, "missing: " + need
assert hashlib.sha256(zin2.read("AndroidManifest.xml")).hexdigest() != hashlib.sha256(zout2.read("AndroidManifest.xml")).hexdigest()
print("merged ok entries=%d classes4=%d B classes2_patched=%s untouched %d/%d byte-identical" % (n, len(c4), c2p.is_file(), len(untouched), len(untouched)))
zin2.close(); zout2.close()
