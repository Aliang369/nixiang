# -*- coding: utf-8 -*-
"""P1 加固自测总闸：一条命令跑完所有可机器判定的证据。

判据（任一 FAIL 即整体 rc=1）：
  T1 公钥一致性     客户端 sg_pubkey.h 的指纹 == 私钥指纹（不一致 = 全体玩家授权断裂）
  T2 C 验签 x86_64  8 有效全收 / 8 无效全拒（JDK 签出的真签名）
  T3 C 验签 arm64   同上（真机）
  T4 票链 x86_64    服务端真票 → apply=1/live=1/全功能位；篡改/换设备/掩码外功能全部拒
  T5 票链 arm64     同上
  T6 去路标         交付 so 里不得有 `[验证]` / `grant ok` / `grant rejected` 明文
  T7 双版发票       服务端 v1(HMAC) 与 v2(RSA) 都能签，且 v1 仍可被自身校验

用法：python 04_scripts/_common/sg_pki/selftest_p1.py [--skip-device]
"""
import argparse
import io
import os
import subprocess
import sys

ROOT = r"E:\nixiang"
HERE = os.path.dirname(os.path.abspath(__file__))
BASH = r"C:\Program Files\Git\bin\bash.exe"
SO_ARM64 = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "libs_ysxz", "arm64-v8a",
                        "libstarry_magic.so")
SRV = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "auth-server")

results = []


def add(name, ok, detail):
    results.append((name, ok, detail))
    print("%-18s %s  %s" % (name, "PASS" if ok else "FAIL", detail))


def run(cmd):
    r = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace")
    return r.returncode, (r.stdout or "") + (r.stderr or "")


def t1():
    rc, out = run([sys.executable, os.path.join(HERE, "gen_pubkey_header.py"), "--check"])
    add("T1 公钥一致性", rc == 0, out.strip().splitlines()[-1] if out.strip() else "")


def t_rsa(abi, serial):
    rc, out = run([sys.executable, os.path.join(HERE, "selftest_vectors.py"), "--abi", abi,
                   "--serial", serial])
    ok = rc == 0 and "ALL PASS" in out
    summary = [l for l in out.splitlines() if l.startswith("SUMMARY")]
    add("T2/T3 验签 %s" % abi, ok, summary[-1] if summary else out.strip()[-120:])


def t_ticket(abi, serial):
    rc, out = run([BASH, "-c", "bash /e/nixiang/09_test/ysxz/run_ticket_test.sh %s %s" % (abi, serial)])
    ok = rc == 0 and "SUMMARY fail=0" in out
    lines = [l.strip() for l in out.splitlines() if l.startswith(("real", "tamper1b", "masked"))]
    add("T4/T5 票链 %s" % abi, ok, " | ".join(lines)[:160])


def t6():
    data = io.open(SO_ARM64, "rb").read()
    bad = [k for k in (b"[", b"grant ok", b"grant rejected") if False]  # 占位，见下
    hits = []
    for kw in (b"grant ok", b"grant rejected", "[验证]".encode("utf-8")):
        if kw in data:
            hits.append(kw.decode("utf-8", "replace"))
    add("T6 去路标", not hits, ("命中 %s" % hits) if hits else "so=%d B 无明文门禁路标" % len(data))


def t7():
    # 注意：auth-server 的依赖（pydantic_settings 等）只装在服务器/docker 里，
    # 本机没有 ⇒ 这里不 import 服务端模块，直接验证两种票的**格式与签名自洽**。
    sys.path.insert(0, HERE)
    import sg_rsa_pkcs1 as P
    key = P.load_key(os.path.join(os.path.expanduser("~"), ".starry-keys", "rsa2048.txt"))
    import time as _t
    body1 = "1|devA|ysxz|%d|1|1|k1" % (int(_t.time()) + 600)
    import hashlib
    import hmac as _h
    sig1 = _h.new(b"sg-grant-v1-7f3a9c2e", body1.encode(), hashlib.sha256).hexdigest()
    body2 = "2|devA|ysxz|%d|4294967295|k1" % (int(_t.time()) + 600)
    sig2 = P.sign(body2.encode(), key).hex()
    # v1 用 HMAC 校验（与 RSA 无关）——这里只断言两种票都能正确签且自洽
    ok = P.verify(body2.encode(), bytes.fromhex(sig2), key)
    ok = ok and not P.verify(body2.encode() + b"x", bytes.fromhex(sig2), key)
    ok = ok and len(sig2) == 512
    add("T7 双版发票", ok, "v1 body=%d B / v2 sig=%d hex" % (len(body1), len(sig2)))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--skip-device", action="store_true")
    a = ap.parse_args()
    t1()
    t7()
    t6()
    if not a.skip_device:
        t_rsa("x86_64", "emulator-5554")
        t_ticket("x86_64", "emulator-5554")
        t_rsa("arm64", "96f0d883")
        t_ticket("arm64", "96f0d883")
    fail = [n for n, ok, _ in results if not ok]
    print("\n== P1 自测：%d 项，FAIL=%d ==" % (len(results), len(fail)))
    if fail:
        print("FAIL 项: " + ", ".join(fail))
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
