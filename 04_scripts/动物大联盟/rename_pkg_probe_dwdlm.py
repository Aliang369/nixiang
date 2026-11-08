#!/usr/bin/env python3
"""把无壳包改成等长旁路包名（com.fightingstars.games.animal -> ...animax），用于不碰原安装的验证装机。
只改 AndroidManifest.xml 里的 UTF-16LE 包名字符串（等长，字符串池偏移不变），其余条目原样透传。
"""
import zipfile, sys, shutil, os

SRC = sys.argv[1]
DST = sys.argv[2]
OLD = "com.fightingstars.games.animal"
NEW = "com.fightingstars.games.animax"
assert len(OLD) == len(NEW)

zin = zipfile.ZipFile(SRC)
out = zipfile.ZipFile(DST, "w")
n_patch = 0
for it in zin.infolist():
    name = it.filename
    if name.startswith("META-INF/") and (name.endswith(".SF") or name.endswith(".RSA") or name.endswith(".DSA") or name.endswith("MANIFEST.MF")):
        continue
    data = zin.read(name)
    if name in ("AndroidManifest.xml", "resources.arsc"):
        b = bytearray(data)
        o = OLD.encode("utf-16-le")
        n = b.count(o)
        nb = NEW.encode("utf-16-le")
        i = 0
        while True:
            j = b.find(o, i)
            if j < 0:
                break
            b[j:j+len(o)] = nb
            i = j + len(o)
            n_patch += 1
        data = bytes(b)
        print("%-18s 替换 %d 处" % (name, n))
    zi = zipfile.ZipInfo(name, date_time=it.date_time)
    zi.compress_type = it.compress_type
    zi.external_attr = it.external_attr
    zi.internal_attr = it.internal_attr
    zi.create_system = it.create_system
    out.writestr(zi, data)
out.close()
print("wrote", DST, os.path.getsize(DST), "entries", len(zin.namelist()), "patched", n_patch)
