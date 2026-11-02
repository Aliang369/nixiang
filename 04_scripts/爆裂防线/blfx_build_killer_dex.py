#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
爆裂防线 — 生成 MT 形态签名杀手 dex（bin.mt.signature.KillerApplication193）

背景（2026-11-02 实测）：本作 TapSDK 的「包名、签名错误」判定在 **native 层**读取
签名，Java 侧改包（门闩/登录）拦不住；框架的 `StarryAppComponentFactory` 会在
冷启动时 `Class.forName("bin.mt.signature.KillerApplication193", true, cl)`，
killer 的 `<clinit>` 做两件事：
    killPM(pkg, sigB64)  → 替换 PackageInfo.CREATOR + 清 Parcel/PM 反射缓存
                          （只对 pkg 这个包名生效，其余包原样返回）
    killOpen(pkg, p1,p2) → base.apk 读取重定向（本作不启用，参数留空）

模板取自 04_scripts/爆裂防线/assets/killer_template.dex（13,884 B，纯 killer 类
13 个 smali，来源：冒险与征途包内 classes4.dex，md5 6aaed994a860d66e84b713c0b42e7970）。
本脚本只改 `<clinit>` 的两条常量，其余字节逻辑不动：
    v0 = 官方签名证书 DER 的 Base64（openSSL 从底包 META-INF/*.RSA 现取）
    v1 = 本作包名 com.muyegame.blfx

用法：
  python 04_scripts/爆裂防线/blfx_build_killer_dex.py \
      [--apk <底包>] [--pkg com.muyegame.blfx] \
      [--out 07_patched/爆裂防线/surgical/classes4.dex]
"""
import argparse
import base64
import os
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
KILLER_CLS = "bin/mt/signature/KillerApplication193.smali"


def die(msg):
    print("ERROR: " + msg, file=sys.stderr)
    sys.exit(1)


def java_bin():
    cands = []
    if os.environ.get("NIXIANG_JAVA17"):
        cands.append(Path(os.environ["NIXIANG_JAVA17"]) / "bin" / "java.exe")
        cands.append(Path(os.environ["NIXIANG_JAVA17"]) / "bin" / "java")
    la = os.environ.get("LOCALAPPDATA")
    if la:
        cands.append(Path(la) / "Programs" / "Microsoft" / "jdk-17.0.20.101-hotspot" / "bin" / "java.exe")
    cands.append(Path("C:/Program Files/Microsoft/jdk-17.0.20.101-hotspot/bin/java.exe"))
    for c in cands:
        if c.is_file():
            return str(c)
    return "java"


def openssl_bin():
    """Git Bash 的 openssl 不在 Windows PATH 上，逐个候选解析"""
    if os.environ.get("OPENSSL") and Path(os.environ["OPENSSL"]).is_file():
        return os.environ["OPENSSL"]
    cands = [
        Path("C:/Program Files/Git/mingw64/bin/openssl.exe"),
        Path("C:/Program Files/Git/usr/bin/openssl.exe"),
        Path("/usr/bin/openssl"),
    ]
    for c in cands:
        if c.is_file():
            return str(c)
    return "openssl"


def cert_b64(apk: Path) -> str:
    """从底包 META-INF/*.RSA 提 PKCS#7 → X.509 DER → Base64（单行）"""
    with zipfile.ZipFile(apk) as z:
        cand = [n for n in z.namelist()
                if n.startswith("META-INF/") and n.upper().endswith((".RSA", ".DSA", ".EC"))]
        if not cand:
            die("底包无 META-INF/*.RSA（v1 签名缺失，无法取官方证书）")
        rsa = z.read(sorted(cand)[0])

    ssl = openssl_bin()
    tmp = Path(tempfile.mkdtemp(prefix="blfx_killer_"))
    try:
        (tmp / "cert.p7").write_bytes(rsa)
        r = subprocess.run([ssl, "pkcs7", "-inform", "DER", "-in", str(tmp / "cert.p7"),
                            "-print_certs", "-out", str(tmp / "cert.pem")],
                           capture_output=True, text=True)
        if r.returncode != 0:
            die("openssl pkcs7 失败: " + (r.stderr or "").strip())
        r = subprocess.run([ssl, "x509", "-in", str(tmp / "cert.pem"),
                            "-outform", "DER"], capture_output=True)
        if r.returncode != 0 or not r.stdout:
            die("openssl x509 失败")
        der = r.stdout
    finally:
        shutil.rmtree(tmp, ignore_errors=True)
    b64 = base64.b64encode(der).decode()
    print("[killer] 官方证书 DER %d B → Base64 %d 字符" % (len(der), len(b64)))
    return b64


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--apk", default=str(REPO / "02_apks/爆裂防线/爆裂防线_1.02.01_original_from_device.apk"))
    ap.add_argument("--pkg", default="com.muyegame.blfx")
    ap.add_argument("--template", default=str(REPO / "04_scripts/爆裂防线/assets/killer_template.dex"))
    ap.add_argument("--out", default=str(REPO / "07_patched/爆裂防线/surgical/classes4.dex"))
    a = ap.parse_args()

    apk = Path(a.apk)
    tpl = Path(a.template)
    out = Path(a.out)
    for p in (apk, tpl):
        if not p.is_file():
            die("缺文件: %s" % p)

    java = java_bin()
    baksmali = REPO / "01_tools/smali/baksmali-fat.jar"
    smali = REPO / "01_tools/smali/smali-fat.jar"

    sig = cert_b64(apk)

    work = Path(tempfile.mkdtemp(prefix="blfx_killer_dec_"))
    dec = work / "dec"
    try:
        print("[1/4] baksmali 模板 → %s" % dec)
        r = subprocess.run([java, "-jar", str(baksmali), "d", str(tpl), "-o", str(dec)])
        if r.returncode != 0:
            die("baksmali rc=%d" % r.returncode)

        f = dec / KILLER_CLS
        if not f.is_file():
            die("模板内无 %s" % KILLER_CLS)
        t = f.read_text(encoding="utf-8")

        # ① 签名常量：<clinit> 里第一条 const-string v0, "MIID..."（证书 Base64）
        m = re.search(r'const-string v0, "MI[A-Za-z0-9+/=\\n]{40,}"', t)
        if not m:
            die("未定位到签名常量（模板结构变化）")
        t = t[:m.start()] + 'const-string v0, "%s"' % sig + t[m.end():]

        # ② 包名常量：const-string v1, "包名"
        m2 = re.search(r'const-string v1, "[^"]+"', t)
        if not m2:
            die("未定位到包名常量")
        t = t[:m2.start()] + 'const-string v1, "%s"' % a.pkg + t[m2.end():]

        # ③ 父类换成本作包里一定存在的 Application
        #    模板来自冒险与征途包，`.super` 指向该包自己的桥接类
        #    （com/mxyzt/gzmy/wuJvNQvtpQZhnd）——本包没有 ⇒ Class.forName 抛
        #    ClassNotFoundException（首次注入实测：StarryAppFactory: SignatureKiller absent）
        m3 = re.search(r'^\.super Lcom/mxyzt/gzmy/[^;]+;', t, re.M)
        if not m3:
            die("未定位到模板父类（模板结构变化）")
        t = t[:m3.start()] + ".super Landroid/app/Application;" + t[m3.end():]
        t = re.sub(r'invoke-direct \{p0\}, Lcom/mxyzt/gzmy/[^;]+;-><init>\(\)V',
                   'invoke-direct {p0}, Landroid/app/Application;-><init>()V', t)
        if "mxyzt" in t:
            die("<clinit>/父类改写后仍有 mxyzt 残留引用")
        f.write_text(t, encoding="utf-8")
        print("[2/4] 常量与父类已替换：pkg=%s / 证书 Base64 / super=Application" % a.pkg)

        tmp_dex = work / "killer.dex"
        print("[3/4] smali 汇编")
        r = subprocess.run([java, "-jar", str(smali), "assemble", str(dec), "-o", str(tmp_dex)])
        if r.returncode != 0:
            die("smali assemble rc=%d" % r.returncode)

        data = tmp_dex.read_bytes()
        assert a.pkg.encode() in data, "产物 dex 缺包名"
        assert sig[:40].encode() in data, "产物 dex 缺签名 Base64 前缀"
        assert b"KillerApplication193" in data, "产物 dex 缺 KillerApplication193"
        assert b"Landroid/app/Application;" in data, "产物 dex 未改父类"
        assert b"mxyzt" not in data, "产物 dex 仍残留原包类引用"
        out.parent.mkdir(parents=True, exist_ok=True)
        out.write_bytes(data)
        print("[4/4] %s (%d B)" % (out, len(data)))
    finally:
        shutil.rmtree(work, ignore_errors=True)


if __name__ == "__main__":
    main()
