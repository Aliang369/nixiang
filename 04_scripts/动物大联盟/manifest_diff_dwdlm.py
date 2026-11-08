#!/usr/bin/env python3
# 动物大联盟 —— 两个 APK 的解剖级单变量比对（本游戏私有，只读，不写包）
#
# 用途：本轮「移除 android:appComponentFactory」对照必须证明：除了 AndroidManifest.xml
# 与签名相关条目（META-INF/*），其余条目（classes.dex / classes2.dex / so / assets / resources.arsc）
# **逐字节相同**。本脚本按 zip 条目逐条比 sha256，并给出二进制 Manifest 的字符串级事实
# （是否含 appComponentFactory、Provider、ProxyApplication、com.starry.magic 出现次数）。
#
# 用法：
#   python3 04_scripts/动物大联盟/manifest_diff_dwdlm.py A.apk B.apk [--json out.json]
# 退出码：0 = 只差 Manifest/签名；1 = 出现其它差异（对照不成立）；2 = 用法/文件错误
import hashlib
import json
import re
import sys
import zipfile
from pathlib import Path

SIG_RE = re.compile(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", re.I)
EXPECTED_DIFF = {"AndroidManifest.xml"}

def facts(apk):
    z = zipfile.ZipFile(apk)
    mf = z.read("AndroidManifest.xml")
    t = mf.decode("utf-16-le", "ignore")
    z.close()
    return {
        "apk": str(apk),
        "size": Path(apk).stat().st_size,
        "sha256": hashlib.sha256(Path(apk).read_bytes()).hexdigest(),
        "manifest_sha256": hashlib.sha256(mf).hexdigest(),
        "manifest_len": len(mf),
        "manifest_facts": {
            # 注意：AXML 里 framework 属性以资源 id 存储，"android:appComponentFactory" 字面量恒为 0；
            # 该属性是否存在以 aapt2 dump xmltree 为准（本工具只报类名/Provider 等字符串事实）。
            "appComponentFactory_literal": t.count("android:appComponentFactory"),
            "factory_class": t.count("com.starry.magic.dwdlm.DwdlmAppComponentFactory"),
            "starry_init_provider": t.count("com.starry.magic.StarryInitProvider"),
            "proxy_application": t.count("dev.mars.secure.ProxyApplication"),
            "menu_service": t.count("com.starry.magic.menu.MenuService"),
            "signspoof": t.count("SignSpoof"),
            "dwdlm_application": t.count("com.starry.magic.dwdlm.DwdlmApplication"),
        },
    }

def entries(apk):
    z = zipfile.ZipFile(apk)
    out = {}
    for i in z.infolist():
        if i.filename.endswith("/"):
            continue
        out[i.filename] = hashlib.sha256(z.read(i)).hexdigest()
    z.close()
    return out

def main(argv):
    args = [a for a in argv[1:] if not a.startswith("--")]
    out_json = None
    if "--json" in argv:
        out_json = Path(argv[argv.index("--json") + 1])
        args = [a for a in args if a != str(out_json)]
    if len(args) != 2:
        print("用法: manifest_diff_dwdlm.py A.apk B.apk [--json out.json]", file=sys.stderr)
        return 2
    a, b = Path(args[0]), Path(args[1])
    for p in (a, b):
        if not p.is_file():
            print("缺文件: %s" % p, file=sys.stderr)
            return 2
    fa, fb = facts(a), facts(b)
    ea, eb = entries(a), entries(b)
    only_a = sorted(set(ea) - set(eb))
    only_b = sorted(set(eb) - set(ea))
    diff = sorted(n for n in set(ea) & set(eb) if ea[n] != eb[n])
    diff_sig = [n for n in diff if SIG_RE.match(n)]
    diff_other = [n for n in diff if not SIG_RE.match(n) and n not in EXPECTED_DIFF]
    same = len([n for n in set(ea) & set(eb) if ea[n] == eb[n]])
    print("[A] %s  %d B  sha256=%s" % (a.name, fa["size"], fa["sha256"][:16]))
    print("[B] %s  %d B  sha256=%s" % (b.name, fb["size"], fb["sha256"][:16]))
    print("[A] manifest: %s" % json.dumps(fa["manifest_facts"], ensure_ascii=False))
    print("[B] manifest: %s" % json.dumps(fb["manifest_facts"], ensure_ascii=False))
    print("[diff] 相同条目=%d  仅A有=%s  仅B有=%s" % (same, only_a or "无", only_b or "无"))
    print("[diff] 内容不同=%s" % (diff or "无"))
    print("[diff] 其中签名条目=%d 条: %s" % (len(diff_sig), diff_sig))
    print("[diff] 其中非 Manifest/签名差异=%s" % (diff_other or "无"))
    verdict = "ONLY_MANIFEST_AND_SIGNATURE" if not diff_other and not only_a and not only_b else "OTHER_DIFF"
    print("[verdict] %s" % verdict)
    if out_json:
        out_json.write_text(json.dumps({
            "a": fa, "b": fb,
            "same_entries": same,
            "only_in_a": only_a, "only_in_b": only_b,
            "diff_entries": diff, "diff_signature": diff_sig, "diff_other": diff_other,
            "verdict": verdict,
        }, ensure_ascii=False, indent=1) + chr(10), encoding="utf-8")
        print("[json] %s" % out_json)
    return 0 if verdict == "ONLY_MANIFEST_AND_SIGNATURE" else 1

if __name__ == "__main__":
    sys.exit(main(sys.argv))
