#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 · shellless6 外科 zip 组装（唯一变量＝换 classes4.dex）。

基座 = shellless5（19 个 so，已含官方 libsecret.so；SL5 口径 ShellGate 无 loadLibrary）。
本轮唯一改动：classes4.dex → SL6 口径（<clinit> 直接 loadLibrary("secret")、require/apiTable native）。
其余条目（AndroidManifest.xml / resources.arsc / 全部 so / classes.dex..classes3.dex）逐字节透传并断言 sha256 一致。

用法: python3 shellless6_assemble_1.0.25.py <base_shellless5_signed.apk> <classes4.dex> <out_unsigned.apk>
"""
import sys, re, zipfile, hashlib, json

base, new_dex, out = sys.argv[1:4]
SECRET = "lib/arm64-v8a/libsecret.so"

def is_sig(name):
    return bool(re.match(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", name, re.I))

def sha(b):
    return hashlib.sha256(b).hexdigest()

dex_data = open(new_dex, "rb").read()
if dex_data[:4] != b"dex\n":
    raise SystemExit("ERROR: 新 classes4.dex 不是合法 dex")

zin = zipfile.ZipFile(base, "r")
zout = zipfile.ZipFile(out, "w")
dropped = []

def put(name, data, compress, dt, attr):
    info = zipfile.ZipInfo(name, dt)
    info.compress_type = compress
    info.external_attr = attr
    info.create_system = 3
    zout.writestr(info, data)

names = [n for n in zin.namelist() if not n.endswith("/")]
if SECRET not in names:
    raise SystemExit("ERROR: 基座缺 " + SECRET + "（shellless5 才含 libsecret）")
if "classes4.dex" not in names:
    raise SystemExit("ERROR: 基座缺 classes4.dex")
secret_sha_base = sha(zin.read(SECRET))

for item in zin.infolist():
    name = item.filename
    if name.endswith("/"):
        continue
    if is_sig(name):
        dropped.append(name)
        continue
    if name == "classes4.dex":
        put(name, dex_data, item.compress_type, item.date_time, item.external_attr)
    else:
        put(name, zin.read(item), item.compress_type, item.date_time, item.external_attr)
zout.close(); zin.close()

# ---- 断言 ----------------------------------------------------------------
zi = zipfile.ZipFile(base); zo = zipfile.ZipFile(out)
touched = {"classes4.dex"} | set(dropped)
same = checked = 0
mismatch = []
for it in zi.infolist():
    n = it.filename
    if n.endswith("/") or n in touched:
        continue
    checked += 1
    if sha(zi.read(n)) == sha(zo.read(n)):
        same += 1
    else:
        mismatch.append(n)
assert not mismatch, "透传字节不一致: %r" % mismatch[:5]
for n in ("AndroidManifest.xml", "resources.arsc", "lib/arm64-v8a/libsecret.so", "lib/arm64-v8a/libcocos2djs.so"):
    assert zo.getinfo(n).compress_type == zi.getinfo(n).compress_type, n + " 压缩方式被改"
assert sha(zo.read(SECRET)) == secret_sha_base, "libsecret.so 字节被动过"
assert sha(zo.read("classes4.dex")) == sha(dex_data), "classes4.dex 未写入"
assert zi.getinfo("classes4.dex").compress_type == zo.getinfo("classes4.dex").compress_type, "classes4.dex 压缩方式被改"
new_dexes = sorted([n for n in zo.namelist() if re.match(r"^classes(\d*)\.dex$", n)])
assert new_dexes == ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex"], "dex 序列变了: %r" % new_dexes
so_names = [n for n in zo.namelist() if n.startswith("lib/arm64-v8a/") and n.endswith(".so")]
assert len(so_names) == 19, "arm64 so 数量异常: %d" % len(so_names)

print(json.dumps({"ok": True, "out": out, "dex_chain": new_dexes,
                  "classes4_sha256": sha(dex_data), "classes4_size": len(dex_data),
                  "libsecret_sha256": secret_sha_base,
                  "dropped_sig": dropped, "passthrough_same": "%d/%d" % (same, checked),
                  "arm64_so": len(so_names), "zip_entries": len(zo.namelist())},
                 ensure_ascii=False, indent=2))
