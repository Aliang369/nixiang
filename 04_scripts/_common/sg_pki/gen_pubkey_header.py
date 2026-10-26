# -*- coding: utf-8 -*-
"""从私钥文件生成 C 侧公钥头 `jni/Includes/sg_pubkey.h`（只有模数，属公开材料，可入库）。

出包门禁靠 `kSgPubFp`（sha256(n) 前 8 字节 hex）核对「客户端公钥 == 服务器公钥」，
两边不一致时所有人都登录不了，所以这一条必须在出包阶段挡住。

用法：
  python 04_scripts/_common/sg_pki/gen_pubkey_header.py [--key <私钥文件>] [--out <头文件>] [--check]
  --check  ：只比对现有头文件与私钥是否一致（出包门禁用），不一致 rc=1
"""
import argparse
import hashlib
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
sys.path.insert(0, HERE)
import sg_rsa_pkcs1 as P  # noqa: E402

DEFAULT_KEY = os.path.join(os.path.expanduser("~"), ".starry-keys", "rsa2048.txt")
DEFAULT_OUT = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni",
                           "Includes", "sg_pubkey.h")


def fp_of(n_int, nbytes):
    return hashlib.sha256(n_int.to_bytes(nbytes, "big")).hexdigest()[:16]


def render(key):
    n = key["n"]
    k = (n.bit_length() + 7) // 8
    nb = n.to_bytes(k, "big")
    lines = []
    lines.append("#ifndef SG_PUBKEY_H")
    lines.append("#define SG_PUBKEY_H")
    lines.append("")
    lines.append("// ============================================================")
    lines.append("//  sg_pubkey.h — 星光授权票验签公钥（**自动生成，勿手改**）")
    lines.append("//  生成：04_scripts/_common/sg_pki/gen_pubkey_header.py")
    lines.append("//  私钥在服务器 / 出包机 keys 目录，**不入库**；这里只有模数（公开材料）。")
    lines.append("//  kSgPubFp = sha256(模数) 前 16 个 hex 字符，供出包门禁核对两端一致。")
    lines.append("// ============================================================")
    lines.append("")
    lines.append("#include <stdint.h>")
    lines.append("")
    lines.append("static const uint8_t kSgPubN[%d] = {" % k)
    for i in range(0, k, 12):
        chunk = ", ".join("0x%02x" % b for b in nb[i:i + 12])
        lines.append("    " + chunk + ",")
    lines.append("};")
    lines.append("")
    lines.append('static const char* kSgPubFp = "%s";' % fp_of(n, k))
    lines.append("")
    lines.append("#endif  // SG_PUBKEY_H")
    return "\n".join(lines) + "\n", fp_of(n, k)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--key", default=DEFAULT_KEY)
    ap.add_argument("--out", default=DEFAULT_OUT)
    ap.add_argument("--check", action="store_true")
    args = ap.parse_args()

    key = P.load_key(args.key)
    body, fp = render(key)

    if args.check:
        if not os.path.exists(args.out):
            print("MISSING %s" % args.out)
            return 1
        cur = open(args.out, "r", encoding="utf-8").read()
        cur_fp = ""
        for line in cur.splitlines():
            if "kSgPubFp" in line and '"' in line:
                cur_fp = line.split('"')[1]
        if cur_fp == fp:
            print("OK 公钥一致 fp=%s" % fp)
            return 0
        print("MISMATCH 头文件 fp=%s / 私钥 fp=%s" % (cur_fp or "?", fp))
        return 1

    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    with open(args.out, "w", encoding="utf-8", newline="\n") as f:
        f.write(body)
    print("wrote %s fp=%s" % (args.out, fp))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
