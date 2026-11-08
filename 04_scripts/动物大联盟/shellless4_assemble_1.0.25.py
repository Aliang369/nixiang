#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 · shellless4 外科 zip 组装（唯一变量＝加 libsecret.so + 换 classes4.dex）。

基座 = shellless3（已实测能进 Cocos、过数据检查，随后弹「安全提示·不安全的设备环境」）。
改动只有两处：
  ① lib/arm64-v8a/libsecret.so  从官方包原样取（字节 + 压缩方式都照官方 = STORED/未压缩）
  ② classes4.dex                 换成带 <clinit>: System.loadLibrary("secret") 的新桩
其余条目（含 AndroidManifest.xml / resources.arsc / 全部 so / classes.dex..classes3.dex）
逐字节透传，并在这里断言 sha256 一致。

用法: python3 shellless4_assemble_1.0.25.py <base_shellless3_signed.apk> <official.apk> <classes4.dex> <out_unsigned.apk>
"""
import sys, re, zipfile, hashlib, json

base, official, new_dex, out = sys.argv[1:5]
SECRET = "lib/arm64-v8a/libsecret.so"
# 插入位置：紧跟 lib/arm64-v8a/libpangleflipped.so（与官方包内 libsecret 的相对次序一致）
ANCHOR = "lib/arm64-v8a/libpangleflipped.so"

def is_sig(name):
    return bool(re.match(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", name, re.I))

def sha(b):
    return hashlib.sha256(b).hexdigest()

zo_src = zipfile.ZipFile(official, "r")
if SECRET not in zo_src.namelist():
    raise SystemExit("ERROR: 官方包无 " + SECRET)
sec_info = zo_src.getinfo(SECRET)
sec_data = zo_src.read(SECRET)
sec_ct = sec_info.compress_type

dex_data = open(new_dex, "rb").read()
if dex_data[:4] != b"dex\n":
    raise SystemExit("ERROR: 新 classes4.dex 不是合法 dex")

zin = zipfile.ZipFile(base, "r")
zout = zipfile.ZipFile(out, "w")
added, dropped = [], []

def put(name, data, compress, dt, attr):
    info = zipfile.ZipInfo(name, dt)
    info.compress_type = compress
    info.external_attr = attr
    info.create_system = 3
    zout.writestr(info, data)

names = [n for n in zin.namelist() if not n.endswith("/")]
if SECRET in names:
    raise SystemExit("ERROR: 基座已含 " + SECRET + "（基座不正确）")
if ANCHOR not in names:
    raise SystemExit("ERROR: 基座缺锚点 " + ANCHOR)

for item in zin.infolist():
    name = item.filename
    if name.endswith("/"):
        continue
    if is_sig(name):
        dropped.append(name)
        continue
    if name == "classes4.dex":
        put(name, dex_data, item.compress_type, item.date_time, item.external_attr)
        added.append({"name": name, "size": len(dex_data), "sha256": sha(dex_data), "from": "rebuilt"})
    else:
        put(name, zin.read(item), item.compress_type, item.date_time, item.external_attr)
    if name == ANCHOR:
        put(SECRET, sec_data, sec_ct, sec_info.date_time, sec_info.external_attr)
        added.append({"name": SECRET, "size": len(sec_data), "sha256": sha(sec_data),
                      "compress_type": sec_ct, "from": "official"})
zout.close(); zin.close(); zo_src.close()

# ---- 断言 ----------------------------------------------------------------
zi = zipfile.ZipFile(base); zo = zipfile.ZipFile(out)
touched = {"classes4.dex", SECRET} | set(dropped)
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
for n in ("AndroidManifest.xml", "resources.arsc", "lib/arm64-v8a/libcocos2djs.so"):
    assert zo.getinfo(n).compress_type == zi.getinfo(n).compress_type, n + " 压缩方式被改"
assert zo.getinfo(SECRET).compress_type == sec_ct, "libsecret.so 压缩方式被改（应与官方一致）"
new_dexes = sorted([n for n in zo.namelist() if re.match(r"^classes(\d*)\.dex$", n)])
assert new_dexes == ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex"], \
    "dex 序列变了: %r" % new_dexes
assert sha(zo.read(SECRET)) == sha(sec_data), "libsecret.so 字节与官方不一致"
assert sha(zo.read("classes4.dex")) == sha(dex_data), "classes4.dex 未写入"
so_names = [n for n in zo.namelist() if n.startswith("lib/arm64-v8a/") and n.endswith(".so")]
base_so = [n for n in zi.namelist() if n.startswith("lib/arm64-v8a/") and n.endswith(".so")]
assert len(so_names) == len(base_so) + 1, "arm64 so 数量异常: %d -> %d" % (len(base_so), len(so_names))

print(json.dumps({"ok": True, "out": out, "dex_chain": new_dexes,
                  "added": added, "dropped_sig": dropped,
                  "passthrough_same": "%d/%d" % (same, checked),
                  "arm64_so_before": len(base_so), "arm64_so_after": len(so_names),
                  "zip_entries": len(zo.namelist())}, ensure_ascii=False, indent=2))
