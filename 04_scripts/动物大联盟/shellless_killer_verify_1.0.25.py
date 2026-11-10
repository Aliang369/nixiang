#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 · shellless-killer 静态自检
用法: python3 shellless_killer_verify_1.0.25.py <signed.apk> [base_shellless2.apk]
产出: 04_scripts/动物大联盟/shellless_killer_verify.json
检查：dex 连续到 classes6 / classes5 含 KillerApplication193+$a..$l / classes6 含 DwdlmKillerFactory
      且只重写 instantiateApplication / manifest 有 appComponentFactory 且 application+launcher 未变
      / 条目逐字节透传 / 签名 DN 含 StarryMagic
"""
import sys, re, os, json, zipfile, hashlib, subprocess

HERE = os.path.dirname(os.path.abspath(__file__))
apk = os.path.abspath(sys.argv[1])
base = os.path.abspath(sys.argv[2]) if len(sys.argv) > 2 else os.path.abspath(os.path.join(
    HERE, "..", "..", "08_output", "动物大联盟", "动物大联盟_1.0.25_shellless2-releaseSigned.apk"))
BT = os.path.expanduser("~/Library/Android/sdk/build-tools/36.0.0")
FACTORY = "com.starry.magic.dwdlm.DwdlmKillerFactory"
KILLER = "bin/mt/signature/KillerApplication193"

def sha(p):
    h = hashlib.sha256()
    with open(p, "rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()

def xmltree(p):
    out = subprocess.run([os.path.join(BT, "aapt2"), "dump", "xmltree",
                          "--file", "AndroidManifest.xml", p],
                         capture_output=True, text=True, check=True).stdout
    return [re.sub(r" \(line=\d+\)$", "", ln).rstrip() for ln in out.splitlines()]

res = {"apk": apk, "apk_sha256": sha(apk), "apk_size": os.path.getsize(apk),
       "base": base, "base_sha256": sha(base), "checks": {}, "raw": {}}
z = zipfile.ZipFile(apk); zb = zipfile.ZipFile(base)
names = z.namelist()

# 1) dex 序列
dex = [n for n in names if re.match(r"^classes(\d*)\.dex$", n)]
dex.sort(key=lambda n: 1 if n == "classes.dex" else int(re.sub(r"\D", "", n)))
res["raw"]["dex_chain"] = dex
res["checks"]["dex_chain_to_6"] = dex == ["classes.dex", "classes2.dex", "classes3.dex",
                                          "classes4.dex", "classes5.dex", "classes6.dex"]

# 2) classes5 = killer dex
d5 = z.read("classes5.dex")
sub = [KILLER] + [KILLER + "$" + chr(ord("a") + i) for i in range(12)]
missing = [s for s in sub if s.encode() not in d5]
res["raw"]["classes5_size"] = len(d5)
res["raw"]["classes5_missing"] = missing
res["raw"]["classes5_has_official_pkg"] = b"com.fightingstars.games.animal" in d5
res["checks"]["classes5_has_killer_and_inner"] = not missing

# 3) classes6 = 工厂 dex
d6 = z.read("classes6.dex")
own = [b"Lcom/starry/magic/dwdlm/DwdlmKillerFactory;", b"instantiateApplication", b"killer mounted"]
res["raw"]["classes6_size"] = len(d6)
res["checks"]["classes6_has_factory"] = all(s in d6 for s in own)
others = [b"instantiateActivity", b"instantiateProvider",
          b"instantiateReceiver", b"instantiateService"]
res["raw"]["classes6_other_overrides"] = [s.decode() for s in others if s in d6]
res["checks"]["classes6_only_instantiateApplication"] = not res["raw"]["classes6_other_overrides"]

# 4) manifest：有 factory、application 与 launcher 未变
tn, tb = xmltree(apk), xmltree(base)
added = [ln.strip() for ln in tn if ln not in tb]
res["raw"]["manifest_added_lines"] = added
res["checks"]["manifest_identical_except_factory"] = ([ln for ln in tn if "appComponentFactory" not in ln] == tb)
res["checks"]["manifest_has_factory"] = (len(added) == 1 and "appComponentFactory" in added[0]
                                         and FACTORY in added[0])
joined = "\n".join(tn)
m = re.search(r'android:name\(0x01010003\)="([^"]*BingoApplication)"', joined)
res["raw"]["application_name"] = m.group(1) if m else None
res["checks"]["application_unchanged"] = res["raw"]["application_name"] == "com.bingo.cocos.bridge.BingoApplication"
res["checks"]["launcher_unchanged"] = ("org.cocos2dx.javascript.AppActivity" in joined and
    tn.count("category(0x01010001)") == tb.count("category(0x01010001)") and
    tn.count("android.intent.category.LAUNCHER") == tb.count("android.intent.category.LAUNCHER"))
res["checks"]["no_shell_classes"] = "dev.mars.secure" not in joined
res["checks"]["no_signspoof"] = "SignSpoof" not in joined
res["checks"]["no_starry_provider"] = "StarryInitProvider" not in joined

# 5) 条目透传
touched = {"AndroidManifest.xml", "classes5.dex", "classes6.dex"} | {
    n for n in names if re.match(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", n, re.I)}
same = checked = 0; bad = []
for it in zb.infolist():
    n = it.filename
    if n.endswith("/") or n in touched:
        continue
    checked += 1
    if hashlib.sha256(zb.read(n)).digest() == hashlib.sha256(z.read(n)).digest():
        same += 1
    else:
        bad.append(n)
res["raw"]["passthrough_same"] = "%d/%d" % (same, checked)
res["raw"]["passthrough_bad"] = bad[:5]
res["raw"]["removed_entries"] = sorted(touched - set(names))
res["checks"]["passthrough_identical"] = (not bad) and checked > 2000

# 6) 签名
out = subprocess.run([os.path.join(BT, "apksigner"), "verify", "--print-certs", apk],
                     capture_output=True, text=True).stdout
dn = re.search(r"certificate DN: (.*)", out)
res["raw"]["cert_dn"] = dn.group(1).strip() if dn else None
res["checks"]["signed_dn_starry"] = bool(dn and "StarryMagic" in dn.group(1))
res["checks"]["apk_verify_ok"] = subprocess.run([os.path.join(BT, "apksigner"), "verify", apk],
                                                capture_output=True, text=True).returncode == 0
res["ok"] = all(res["checks"].values())
res["failed"] = [k for k, v in res["checks"].items() if not v]
json.dump(res, open(os.path.join(HERE, "shellless_killer_verify.json"), "w", encoding="utf-8"),
          ensure_ascii=False, indent=2)
print(json.dumps({"ok": res["ok"], "failed": res["failed"], "checks": res["checks"],
                  "passthrough": res["raw"]["passthrough_same"], "dex": res["raw"]["dex_chain"],
                  "cert_dn": res["raw"]["cert_dn"], "manifest_added": res["raw"]["manifest_added_lines"]},
                 ensure_ascii=False, indent=2))
sys.exit(0 if res["ok"] else 1)
