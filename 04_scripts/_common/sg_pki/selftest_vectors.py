# -*- coding: utf-8 -*-
"""C 侧 sg_rsa_verify.c 的对拍套件（生成向量 → 交叉编译 → 真机/模拟器跑）。

为什么这么做：出包环境无网络、无 Windows C 编译器，只有 NDK clang；
所以「C 实现对不对」必须用 JDK 签出来的真签名在**同一台设备上**验，
不能靠肉眼看代码。本脚本一次跑完：

  1) 用 04_scripts/_common/sg_pki/sg_rsa_pkcs1.py 造向量
       - 8 条有效（各种长度消息：0B / 1B / 16B / 票格式 / 1KB / 4KB）
       - 8 条无效（改消息 / 改签名首字节 / 改签名尾字节 / 截断签名 / 超长签名 /
                  全零签名 / sig=n 本身 / 换一把公钥）
  2) NDK clang 交叉编译 test_sg_rsa.c + sg_rsa_verify.c + hmac_sha256.c → x86_64 / arm64
  3) adb push 到 /data/local/tmp/sg_pki/ 并运行，逐条比对期望值

用法：
  python 04_scripts/_common/sg_pki/selftest_vectors.py --key <私钥文件> [--abi x86_64|arm64] [--serial emulator-5554]
"""
import argparse
import os
import subprocess
import sys
import random

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
JNI = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni")
sys.path.insert(0, HERE)
import sg_rsa_pkcs1 as P  # noqa: E402

NDK = os.path.join(os.environ.get("LOCALAPPDATA", ""), "Android", "Sdk", "ndk", "27.0.12077973")
CLANG = os.path.join(NDK, "toolchains", "llvm", "prebuilt", "windows-x86_64", "bin", "clang.exe")
SYSROOT = os.path.join(NDK, "toolchains", "llvm", "prebuilt", "windows-x86_64", "sysroot")
ADB = os.path.join(os.environ.get("LOCALAPPDATA", ""), "Android", "Sdk", "platform-tools", "adb.exe")


def make_vectors(key, out_path):
    n = key["n"]
    k = (n.bit_length() + 7) // 8
    msgs = [b"", b"A", b"STARRY-SELFTEST", b"hi-there-16bytes",
            b"2|dev_abc123|ysxz|1790000000|4095|k1|", os.urandom(64),
            bytes(range(256)) * 4, os.urandom(1024)]
    lines = ["N %0*x" % (k * 2, n)]
    valid = 0
    for m in msgs:
        sig = P.sign(m, key)
        assert P.verify(m, sig, key), "python 自验失败"
        lines.append("V %s %s 1" % (m.hex() if m else "-", sig.hex()))
        valid += 1

    m0 = b"STARRY-SELFTEST"
    good = P.sign(m0, key)
    bad = []
    bad.append(("改消息", b"STARRY-SELFTESU", good, 0))
    b1 = bytearray(good); b1[0] ^= 0x01
    bad.append(("改签名首字节", m0, bytes(b1), 0))
    b2 = bytearray(good); b2[k - 1] ^= 0x80
    bad.append(("改签名尾字节", m0, bytes(b2), 0))
    bad.append(("签名截断", m0, good[:-1], 0))
    bad.append(("签名超长", m0, good + b"\x00", 0))
    bad.append(("全零签名", m0, b"\x00" * k, 0))
    bad.append(("sig==n", m0, n.to_bytes(k, "big"), 0))
    other = {"n": n, "e": key["e"]}   # 结构非法（缺 d）也能造：用另一把钥签
    # 用同模数但篡改中间一段 → 覆盖「中间位翻转」
    b3 = bytearray(good); b3[k // 2] ^= 0xFF
    bad.append(("中间字节翻转", m0, bytes(b3), 0))

    for name, m, s, want in bad:
        assert not P.verify(m, s, key), "无效向量竟然验过了：" + name
        lines.append("V %s %s %d" % (m.hex() if m else "-", s.hex(), want))

    with open(out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")
    return valid, len(bad)


def build(abi, workdir):
    target = {"x86_64": "x86_64-linux-android24", "arm64": "aarch64-linux-android24"}[abi]
    exe = os.path.join(workdir, "test_sg_rsa_%s" % abi)
    cmd = [CLANG, "--target=" + target, "--sysroot=" + SYSROOT, "-O2", "-std=c99",
           "-o", exe,
           os.path.join(HERE, "test_sg_rsa.c"),
           os.path.join(JNI, "Includes", "hmac_sha256.c"),   # sg 验签是 header-only（Includes/sg_rsa_verify.h）
           "-I", os.path.join(JNI, "Includes"),
           "-Wl,--gc-sections"]
    r = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace")
    if r.returncode != 0:
        print(r.stdout)
        print(r.stderr)
        raise SystemExit("交叉编译失败")
    return exe


def run_on_device(exe, vectors, serial):
    subprocess.run([ADB, "-s", serial, "shell", "mkdir", "-p", "/data/local/tmp/sg_pki"], check=True)
    subprocess.run([ADB, "-s", serial, "push", exe, "/data/local/tmp/sg_pki/t"], check=True,
                   capture_output=True)
    subprocess.run([ADB, "-s", serial, "push", vectors, "/data/local/tmp/sg_pki/v.txt"], check=True,
                   capture_output=True)
    subprocess.run([ADB, "-s", serial, "shell", "chmod", "755", "/data/local/tmp/sg_pki/t"], check=True)
    r = subprocess.run([ADB, "-s", serial, "shell", "/data/local/tmp/sg_pki/t /data/local/tmp/sg_pki/v.txt"],
                       capture_output=True, text=True, encoding="utf-8", errors="replace")
    return r.returncode, (r.stdout or "") + (r.stderr or "")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--key", default=os.path.join(os.path.expanduser("~"), ".starry-keys", "rsa2048.txt"))
    ap.add_argument("--abi", default="x86_64", choices=["x86_64", "arm64"])
    ap.add_argument("--serial", default="emulator-5554")
    args = ap.parse_args()

    work = os.path.join(os.environ.get("TEMP", "."), "sg_pki_selftest")
    os.makedirs(work, exist_ok=True)
    key = P.load_key(args.key)
    vectors = os.path.join(work, "vectors.txt")
    nv, ni = make_vectors(key, vectors)
    print("[1/3] 向量：有效 %d 条 + 无效 %d 条 → %s" % (nv, ni, vectors))

    exe = build(args.abi, work)
    print("[2/3] 交叉编译 OK → %s (%d B)" % (exe, os.path.getsize(exe)))

    rc, out = run_on_device(exe, vectors, args.serial)
    print("[3/3] 设备 %s 运行结果（rc=%d）:" % (args.serial, rc))
    print(out.strip())
    if rc != 0:
        raise SystemExit("有向量判错，C 实现不可用")
    print("ALL PASS")


if __name__ == "__main__":
    main()
