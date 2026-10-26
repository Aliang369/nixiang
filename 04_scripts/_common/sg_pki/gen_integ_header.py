# -*- coding: utf-8 -*-
"""P1-6：生成「完整性清单」C 头 `jni/Includes/sg_integ.h`（含私钥签名）。

为什么清单在**打包前**生成（而不是打包后）：
    清单要编进 so，so 又要打进 APK —— 打包后生成就成了死循环。
    解法：清单只覆盖**明文内容**（classes4.dex / assets/main.js / assets/certs/starlight.crt），
    这三样在 so 编译前就已经定稿；native 运行时从 APK 里解压出明文再比对。
    所以清单**不覆盖 so 自身**（自指不可能），但覆盖了「删我卡密 dex / 换 boot js / 换证书」这三条主路。

为什么要签名：
    清单若只是明文表，破解者改表即可自我放行。清单用**私钥签名**后，native 先验签名
    再比对哈希 ⇒ 没有私钥改不动这张表（与授权票同一把钥匙、同一套 RSA 验签）。

用法：
  python 04_scripts/_common/sg_pki/gen_integ_header.py \
      --dex <classes4.dex> --base-apk <底包.apk> --certs <starlight.crt> \
      [--key <私钥>] [--out <sg_integ.h>] [--check]
"""
import argparse
import hashlib
import io
import os
import sys
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(ROOT, "mod菜单", "starry-mod-menu", "auth-server"))
import sg_rsa_pkcs1 as P  # noqa: E402

DEFAULT_KEY = os.path.join(os.path.expanduser("~"), ".starry-keys", "rsa2048.txt")
DEFAULT_OUT = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni",
                           "Includes", "sg_integ.h")


def collect(dex, base_apk, certs):
    """返回 [(apk 内路径, 明文字节)] —— 顺序固定，改动要同步 C 侧注释。"""
    items = []
    with io.open(dex, "rb") as f:
        items.append(("classes4.dex", f.read()))
    with zipfile.ZipFile(base_apk) as z:
        items.append(("assets/main.js", z.read("assets/main.js")))
    with io.open(certs, "rb") as f:
        items.append(("assets/certs/starlight.crt", f.read()))
    return items


def canonical(items):
    """规范化串：SGI1|<path>=<len>:<sha256hex>|...（签的就是它）"""
    parts = ["SGI1"]
    for path, data in items:
        parts.append("%s=%d:%s" % (path, len(data), hashlib.sha256(data).hexdigest()))
    return "|".join(parts).encode("utf-8")


def render(items, key):
    body = canonical(items)
    sig = P.sign(body, key) if key else None
    n = len(items)
    out = []
    out.append("#ifndef SG_INTEG_H")
    out.append("#define SG_INTEG_H")
    out.append("")
    out.append("// ============================================================")
    out.append("//  sg_integ.h — 完整性清单（**自动生成，勿手改**）")
    out.append("//  生成：04_scripts/_common/sg_pki/gen_integ_header.py")
    out.append("//  覆盖：APK 内条目明文哈希；清单由私钥签名，C 侧先验签名再比对。")
    out.append("//  改这张表没有私钥 ⇒ 验签失败 ⇒ 会话不成立（fail-closed）。")
    out.append("// ============================================================")
    out.append("")
    out.append("#include <stdint.h>")
    out.append("")
    out.append("typedef struct {")
    out.append("    const char* path;        // APK 内路径")
    out.append("    uint32_t    size;        // 明文长度")
    out.append("    uint8_t     sha256[32];  // 明文 sha256")
    out.append("} sg_integ_entry;")
    out.append("")
    out.append("static const sg_integ_entry kSgInteg[%d] = {" % n)
    for path, data in items:
        h = hashlib.sha256(data).digest()
        hx = ", ".join("0x%02x" % b for b in h)
        out.append('    { "%s", %d, { %s } },' % (path, len(data), hx))
    out.append("};")
    out.append("static const int kSgIntegCount = %d;" % n)
    out.append("")
    out.append('static const char kSgIntegSigHex[] = "%s";' % (sig.hex() if sig else ""))
    out.append("")
    out.append("#endif  // SG_INTEG_H")
    return "\n".join(out) + "\n", body, sig


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dex", default=os.path.join(ROOT, "mod菜单", "starry-mod-menu",
                                                 "libs_ysxz", "classes4.dex"))
    ap.add_argument("--base-apk", default=os.path.join(ROOT, "02_apks", "勇士之心.apk"))
    ap.add_argument("--certs", default=os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app",
                                                   "src", "main", "assets", "certs", "starlight.crt"))
    ap.add_argument("--key", default=DEFAULT_KEY)
    ap.add_argument("--out", default=DEFAULT_OUT)
    ap.add_argument("--check", action="store_true")
    a = ap.parse_args()

    items = collect(a.dex, a.base_apk, a.certs)
    key = P.load_key(a.key) if os.path.exists(a.key) else None
    if key is None:
        print("ERROR: 找不到私钥 %s（清单必须签名，否则破解者改表即放行）" % a.key)
        return 1
    body, sig = render(items, key)[1], render(items, key)[2]
    text = render(items, key)[0]

    if a.check:
        if not os.path.exists(a.out):
            print("MISSING %s" % a.out)
            return 1
        cur = io.open(a.out, "r", encoding="utf-8").read()
        cur_sig = ""
        for line in cur.splitlines():
            if "kSgIntegSigHex" in line and '"' in line:
                cur_sig = line.split('"')[1]
        ok = cur_sig == sig.hex()
        print(("OK 清单一致 sig=%s…" % sig.hex()[:16]) if ok else "MISMATCH 清单与当前产物不符")
        return 0 if ok else 1

    os.makedirs(os.path.dirname(a.out), exist_ok=True)
    io.open(a.out, "w", encoding="utf-8", newline="\n").write(text)
    print("wrote %s entries=%d body=%d B sig=%s…" % (a.out, len(items), len(body), sig.hex()[:16]))
    for path, data in items:
        print("  %-28s %8d B  %s" % (path, len(data), hashlib.sha256(data).hexdigest()[:16]))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
