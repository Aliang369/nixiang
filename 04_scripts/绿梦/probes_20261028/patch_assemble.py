import re
p = r"E:\_build\lvmeng\assemble.py"
s = open(p, encoding="utf-8").read()
old = '''    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, extras[name].read_bytes())
    n += 1'''
new = '''    info = zipfile.ZipInfo(name)
    # fg_orig.apk 必须 STORED（native 侧按本地头+数据偏移直接 pread，不解压）
    info.compress_type = zipfile.ZIP_STORED if name.endswith("assets/lvmeng/fg_orig.apk") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    if name.endswith("assets/lvmeng/fg_orig.apk"):
        with open(extras[name], "rb") as fi, zout.open(info, "w") as fo:
            import shutil as _sh
            _sh.copyfileobj(fi, fo, 1 << 20)
    else:
        zout.writestr(info, extras[name].read_bytes())
    n += 1'''
assert s.count(old) == 1
s = s.replace(old, new)
old2 = '''for need in ["classes4.dex", "lib/arm64-v8a/libstarry_magic.so", "AndroidManifest.xml"]:'''
new2 = '''for need in ["classes4.dex", "lib/arm64-v8a/libstarry_magic.so", "AndroidManifest.xml", "assets/lvmeng/fg_orig.apk"]:'''
assert s.count(old2) == 1
s = s.replace(old2, new2)
open(p, "w", encoding="utf-8").write(s)
print("assemble.py patched")
