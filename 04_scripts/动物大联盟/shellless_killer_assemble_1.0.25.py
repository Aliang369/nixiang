#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 · 无壳包 + 运行时签名替换（唯一变量＝加 killer dex + appComponentFactory）
用法: python3 shellless_killer_assemble_1.0.25.py <base_shellless2_signed.apk> <new_manifest.xml> <killer_dex> <factory_dex> <out_unsigned.apk>
基座内 classes.dex..classes4.dex 逐字节透传；新增 classes5.dex=killer、classes6.dex=工厂。
除 AndroidManifest.xml 与新增 dex 外，所有 zip 条目按原 date_time/compress_type/external_attr 逐字节透传并断言。
"""
import sys, re, zipfile, hashlib, json

base, mfpath, killer, factory, out = sys.argv[1:6]
NEW_DEX = [("classes5.dex", killer), ("classes6.dex", factory)]

def is_sig(name):
    return bool(re.match(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", name, re.I))

mf = open(mfpath, "rb").read()
assert mf[:4] == b"\x03\x00\x08\x00", "新 Manifest 不是真 AXML"
mftxt = mf.decode("utf-16-le", "ignore")
for needle, what in [("com.starry.magic.dwdlm.DwdlmKillerFactory", "工厂类名"),
                     ("com.bingo.cocos.bridge.BingoApplication", "无壳 Application"),
                     ("org.cocos2dx.javascript.AppActivity", "启动 Activity"),
                     ("android.intent.category.LAUNCHER", "LAUNCHER")]:
    assert needle in mftxt, "新 Manifest 缺 " + what
for bad, what in [("dev.mars.secure", "壳类"), ("SignSpoof", "签名伪装")]:
    assert bad not in mftxt, "新 Manifest 含 " + what

zin = zipfile.ZipFile(base, "r")
zout = zipfile.ZipFile(out, "w")
added, dropped = [], []

def put(name, data, compress, dt, attr):
    info = zipfile.ZipInfo(name, dt)
    info.compress_type = compress
    info.external_attr = attr
    info.create_system = 3
    zout.writestr(info, data)

dex_names = [n for n in zin.namelist() if re.match(r"^classes(\d*)\.dex$", n)]
assert dex_names == ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex"], \
    "基座 dex 序列与预期不符: %r" % dex_names

for item in zin.infolist():
    name = item.filename
    if name.endswith("/"):
        continue
    if is_sig(name):
        dropped.append(name)
        continue
    if name == "AndroidManifest.xml":
        put(name, mf, item.compress_type, item.date_time, item.external_attr)
        added.append({"name": name, "size": len(mf), "from": "edited"})
        continue
    put(name, zin.read(item), item.compress_type, item.date_time, item.external_attr)
    if name == "classes4.dex":
        for dn, dp in NEW_DEX:
            data = open(dp, "rb").read()
            put(dn, data, zipfile.ZIP_DEFLATED, (2026, 11, 6, 0, 0, 0), 0o100644 << 16)
            added.append({"name": dn, "size": len(data),
                          "sha256": hashlib.sha256(data).hexdigest()})
zout.close(); zin.close()

# ---- 断言 ----------------------------------------------------------------
zi = zipfile.ZipFile(base); zo = zipfile.ZipFile(out)
touched = {"AndroidManifest.xml", "classes5.dex", "classes6.dex"} | set(dropped)
same = checked = 0
mismatch = []
for it in zi.infolist():
    n = it.filename
    if n.endswith("/") or n in touched:
        continue
    checked += 1
    if hashlib.sha256(zi.read(n)).digest() == hashlib.sha256(zo.read(n)).digest():
        same += 1
    else:
        mismatch.append(n)
assert not mismatch, "透传字节不一致: %r" % mismatch[:5]
for n in ("resources.arsc", "lib/arm64-v8a/libcocos2djs.so"):
    assert zo.getinfo(n).compress_type == zi.getinfo(n).compress_type, n + " 压缩方式被改"
new_dexes = sorted([n for n in zo.namelist() if re.match(r"^classes(\d*)\.dex$", n)])
assert new_dexes == ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
                     "classes5.dex", "classes6.dex"], "dex 编号不连续: %r" % new_dexes

print(json.dumps({"ok": True, "out": out, "dex_chain": new_dexes,
                  "added": added, "dropped_sig": dropped,
                  "passthrough_same": "%d/%d" % (same, checked),
                  "zip_entries": len(zo.namelist())}, ensure_ascii=False, indent=2))
