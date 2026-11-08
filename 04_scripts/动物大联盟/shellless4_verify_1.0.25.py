#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 · shellless4 静态自检（只读，不装机）。

比对对象：
  APK          待检包（shellless4-releaseSigned.apk）
  BASE_APK     基座 shellless3-releaseSigned.apk
  OFFICIAL_APK 官方包
  DEX_FILE     新 classes4.dex

检查项（全部必须 PASS）：
  V1  arm64 so 共 19 个（基座 18 + libsecret.so）
  V2  lib/arm64-v8a/libsecret.so 的 sha256 与官方包内一致
  V3  classes4.dex 含 <clinit>、含字符串 "secret"、含 loadLibrary 调用
  V4  AndroidManifest.xml 与基座逐字节一致；application/launcher 未变
  V5  除 classes4.dex 与新增 libsecret.so 外，所有条目与基座逐字节一致
  V6  签名 DN 含 StarryMagic
用法: 以环境变量传参（见 shellless4_build_1.0.25.sh 第 4 步）。
"""
import os, re, sys, json, hashlib, zipfile, subprocess

APK = os.environ["APK"]
BASE = os.environ["BASE_APK"]
OFFICIAL = os.environ["OFFICIAL_APK"]
DEXF = os.environ["DEX_FILE"]
BT = os.environ["BT_DIR"]
OUT = os.environ["OUT_JSON"]
SECRET = "lib/arm64-v8a/libsecret.so"

def sha(b):
    return hashlib.sha256(b).hexdigest()

def sh(*args):
    return subprocess.run(args, capture_output=True, text=True).stdout

def sig_re(name):
    return bool(re.match(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", name, re.I))

chks = []
def chk(cid, title, ok, detail):
    chks.append({"id": cid, "title": title, "ok": bool(ok), "detail": detail})
    print(("  ✔ %-3s %s" % (cid, title)) if ok else ("  ✘ %-3s %s" % (cid, title)))
    print("        " + str(detail))

za, zb, zo = zipfile.ZipFile(APK), zipfile.ZipFile(BASE), zipfile.ZipFile(OFFICIAL)
na, nb, no = za.namelist(), zb.namelist(), zo.namelist()

# V1 arm64 so 数量
so_a = sorted(n for n in na if n.startswith("lib/arm64-v8a/") and n.endswith(".so"))
so_b = sorted(n for n in nb if n.startswith("lib/arm64-v8a/") and n.endswith(".so"))
chk("V1", "arm64 so 共 19 个（基座 18 + libsecret.so）",
    len(so_a) == 19 and len(so_b) == 18 and SECRET in so_a,
    "before=%d after=%d secret=%s" % (len(so_b), len(so_a), SECRET in so_a))

# V2 libsecret.so 与官方一致
sec_a = sha(za.read(SECRET)) if SECRET in na else None
sec_o = sha(zo.read(SECRET)) if SECRET in no else None
secb_a = za.getinfo(SECRET).compress_type if SECRET in na else -1
secb_o = zo.getinfo(SECRET).compress_type if SECRET in no else -2
chk("V2", "libsecret.so sha256 与官方包一致 + 压缩方式一致",
    sec_a is not None and sec_a == sec_o and secb_a == secb_o,
    "apk=%s official=%s size=%d ct=%d(官方%d)" % (sec_a, sec_o,
        za.getinfo(SECRET).file_size if SECRET in na else -1, secb_a, secb_o))

# V3 classes4.dex 内容
dex = za.read("classes4.dex") if "classes4.dex" in na else b""
dump = sh(os.path.join(BT, "dexdump"), "-d", DEXF)  # -d 才会反汇编方法体（loadLibrary 在代码段）
has_clinit = "name          : '<clinit>'" in dump
has_secret = b"secret\x00" in dex
has_load = "System;.loadLibrary" in dump
has_gate = "Ldev/mars/secure/ShellGate;" in dump
dexes = sorted([n for n in na if re.match(r"^classes(\d*)\.dex$", n)])
chk("V3", "classes4.dex 带 <clinit> + loadLibrary(\"secret\") + 仍只有 ShellGate",
    has_clinit and has_secret and has_load and has_gate
    and dexes == ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex"]
    and sha(dex) == sha(open(DEXF, "rb").read()),
    "clinit=%s secret_str=%s loadLibrary=%s dex链=%s size=%d sha256=%s" %
    (has_clinit, has_secret, has_load, ",".join(dexes), len(dex), sha(dex)[:16] + "…"))

# V4 Manifest 未变 + application/launcher 未变
mf_a = sha(za.read("AndroidManifest.xml"))
mf_b = sha(zb.read("AndroidManifest.xml"))
aapt2 = os.path.join(BT, "aapt2")
bad_a = sh(aapt2, "dump", "badging", APK)
bad_b = sh(aapt2, "dump", "badging", BASE)
def pick(txt, key):
    for line in txt.splitlines():
        if line.startswith(key):
            return line.strip()
    return ""
keys = ("package:", "launchable-activity:", "sdkVersion:", "targetSdkVersion:")
same_badging = all(pick(bad_a, k) == pick(bad_b, k) for k in keys)
xmltree = sh(aapt2, "dump", "xmltree", "--file", "AndroidManifest.xml", APK)
has_app = 'com.bingo.cocos.bridge.BingoApplication' in xmltree
has_act = 'org.cocos2dx.js' not in xmltree and 'org.cocos2dx.javascript.AppActivity' in xmltree
no_shell = 'dev.mars.secure' not in xmltree
chk("V4", "Manifest 逐字节未变 + Application/launcher 未变",
    mf_a == mf_b and same_badging and has_app and has_act and no_shell,
    "manifest sha=%s vs base %s | app=%s launcher=%s 无壳类=%s | %s" %
    (mf_a[:16] + "…", mf_b[:16] + "…", has_app, has_act, no_shell, pick(bad_a, "launchable-activity:")))

# V5 透传一致性
touched = {"classes4.dex", SECRET} | {n for n in nb if sig_re(n)}
mismatch, checked = [], 0
for it in zb.infolist():
    n = it.filename
    if n.endswith("/") or n in touched:
        continue
    checked += 1
    if n not in na or sha(zb.read(n)) != sha(za.read(n)):
        mismatch.append(n)
extra = sorted(set(na) - set(nb) - {SECRET})
missing = sorted(set(nb) - set(na) - {n for n in nb if sig_re(n)})
chk("V5", "其余条目逐字节透传（唯一差异＝classes4.dex + libsecret.so）",
    not mismatch and not extra and not missing,
    "透传 %d/%d 一致 | 多出=%r | 缺失=%r | META-INF 签名条目重签 %d 个" %
    (checked - len(mismatch), checked, extra[:3], missing[:3], len(touched) - 2))

# V6 签名
certs = sh(os.path.join(BT, "apksigner"), "verify", "--print-certs", APK)
dn = ""
for line in certs.splitlines():
    if "certificate DN:" in line:
        dn = line.split("certificate DN:", 1)[1].strip()
        break
cert_sha = ""
for line in certs.splitlines():
    if "certificate SHA-256 digest:" in line:
        cert_sha = line.split("digest:", 1)[1].strip()
        break
chk("V6", "签名 DN 含 StarryMagic", "StarryMagic" in dn,
    "DN=%s | cert_sha256=%s" % (dn, cert_sha[:16] + "…"))

# zip 对齐（-p 4）——只做记录，不判失败
zalign = subprocess.run([os.path.join(BT, "zipalign"), "-c", "-p", "4", APK],
                        capture_output=True, text=True)

report = {
    "apk": APK, "apk_size": os.path.getsize(APK), "apk_sha256": sha(open(APK, "rb").read()),
    "base_shellless3_sha256": sha(open(BASE, "rb").read()),
    "official_sha256": sha(open(OFFICIAL, "rb").read()),
    "secret_so_sha256": sec_a, "secret_so_size": za.getinfo(SECRET).file_size if SECRET in na else None,
    "secret_so_compress_type": secb_a,
    "classes4_dex_sha256": sha(dex), "classes4_dex_size": len(dex),
    "classes4_has_clinit": has_clinit,
    "arm64_so_count": len(so_a), "zip_entries": len(na),
    "signer_dn": dn, "signer_cert_sha256": cert_sha,
    "zipalign_p4_ok": zalign.returncode == 0,
    "checks": chks,
    "all_pass": all(c["ok"] for c in chks),
}
json.dump(report, open(OUT, "w", encoding="utf-8"), ensure_ascii=False, indent=2)

print("  ---- 汇总: %d/%d PASS ----" % (sum(1 for c in chks if c["ok"]), len(chks)))
print("  apk  size=%d sha256=%s" % (report["apk_size"], report["apk_sha256"]))
print("  report -> " + OUT)
sys.exit(0 if report["all_pass"] else 1)
