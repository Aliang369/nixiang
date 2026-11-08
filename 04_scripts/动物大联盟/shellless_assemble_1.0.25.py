
#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 · 无壳包外科 zip 组装（官方包逐条透传）
依据 10_docs/动物大联盟/脱壳与无壳包方案_1.0.25_20261105.md §3
用法: python3 shellless_assemble_1.0.25.py <official.apk> <new_AndroidManifest.xml> <dex1> <dex2> <dex3> <libcocos2djs.so> <out_unsigned.apk> [extradex ...]
      extradex（可选，1.0.25 SL2 起）：按顺序接排为 classes4.dex/classes5.dex…；不传则与 SL1 完全一致。
环境变量 SHELLLESS_EXTRA_SO（可选，冒号分隔）：额外 so 落 lib/arm64-v8a/<basename>；不设则与 SL2 逐字节一致。
"""
import sys, re, zipfile, hashlib, json

official, newmf, d1, d2, d3, so, out = sys.argv[1:8]
EXTRA = sys.argv[8:]          # 可选补丁 dex：接排 classes4.dex/classes5.dex…
DEFS = [("classes.dex", d1), ("classes2.dex", d2), ("classes3.dex", d3)]
DEFS += [("classes%d.dex" % (4 + i), p) for i, p in enumerate(EXTRA)]

DROP_EXACT = {
    "classes.dex",                                   # 壳 dex（1,538,536B / 1152 类），整个删、不占号
    "assets/apksecurefile", "assets/apksecuresolist",
    "lib/x86/libsecret.so",
}
for abi in ("arm64-v8a", "armeabi-v7a"):
    for n in ("libsecret.so", "libEncryptorP.so", "libzeus_direct_dex.so", "libterrain.so", "libluster.so"):
        DROP_EXACT.add("lib/%s/%s" % (abi, n))

def drop(name):
    if name in DROP_EXACT:
        return True
    if name.startswith("assets/apksecuresolib/"):
        return True
    if re.match(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", name, re.I):
        return True
    if name.endswith("/"):
        return True
    return False

mf = open(newmf, "rb").read()
zin = zipfile.ZipFile(official, "r")
zout = zipfile.ZipFile(out, "w")
placed_dex = False
dropped, added = [], []

def put(name, data, compress, dt=(2026, 11, 5, 0, 0, 0), attr=0o100644 << 16):
    info = zipfile.ZipInfo(name, dt)
    info.compress_type = compress
    info.external_attr = attr
    info.create_system = 3
    zout.writestr(info, data)

for item in zin.infolist():
    name = item.filename
    if name == "classes.dex":
        # 在原 classes.dex 位置连续放 真 dex（+可选补丁 dex），编号不跳号
        for dn, dp in DEFS:
            put(dn, open(dp, "rb").read(), zipfile.ZIP_DEFLATED)
            added.append((dn, len(open(dp, "rb").read())))
        placed_dex = True
        dropped.append(name)
        continue
    if drop(name):
        dropped.append(name); continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    if name == "AndroidManifest.xml":
        added.append((name, len(mf)))

assert placed_dex, "官方包内未找到 classes.dex，无法定位 dex 插入点"
# 新增 libcocos2djs.so：与官方 .so 同压缩方式（官方实测 DEFLATED=8）
put("lib/arm64-v8a/libcocos2djs.so", open(so, "rb").read(), zipfile.ZIP_DEFLATED)
added.append(("lib/arm64-v8a/libcocos2djs.so", len(open(so, "rb").read())))

# ---- 可选：追加本游戏私有 so（默认空 ⇒ 与 SL2 逐字节一致）------------------
# SHELLLESS_EXTRA_SO 用冒号分隔的绝对路径列表；落 lib/arm64-v8a/<basename>，DEFLATED。
# 用途：菜单 so 与其依赖（libc++_shared.so，官方包与无壳包都不含）必须一起进包，
#       否则 dlopen libstarry_magic.so 直接 UnsatisfiedLinkError(llvm-readelf NEEDED 实测)。
import os as _os
for _p in [x for x in _os.environ.get("SHELLLESS_EXTRA_SO", "").split(":") if x]:
    _name = "lib/arm64-v8a/" + _os.path.basename(_p)
    assert _name not in zout.namelist(), "重复 so 条目: " + _name
    _data = open(_p, "rb").read()
    put(_name, _data, zipfile.ZIP_DEFLATED)
    added.append((_name, len(_data)))
zout.close(); zin.close()

# ---- 透传校验 ----
zi = zipfile.ZipFile(official); zo = zipfile.ZipFile(out)
touched = {n for n, _ in added} | set(DROP_EXACT) | {"AndroidManifest.xml"}
same = checked = 0
for it in zi.infolist():
    n = it.filename
    if n.endswith("/") or n in touched or n.startswith("META-INF/") or n.startswith("assets/apksecuresolib/"):
        continue
    checked += 1
    if hashlib.sha256(zi.read(n)).digest() == hashlib.sha256(zo.read(n)).digest():
        same += 1
assert same == checked, "透传字节不一致 %d/%d" % (same, checked)
assert hashlib.sha256(zo.read("resources.arsc")).digest() == hashlib.sha256(zi.read("resources.arsc")).digest()
assert zo.getinfo("resources.arsc").compress_type == zi.getinfo("resources.arsc").compress_type

print(json.dumps({
    "ok": True, "out": out,
    "dropped": sorted(dropped),
    "added": [{"name": n, "size": s} for n, s in added],
    "passthrough_same": "%d/%d" % (same, checked),
    "zip_entries": len(zo.namelist()),
}, ensure_ascii=False, indent=2))
