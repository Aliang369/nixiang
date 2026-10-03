#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把「敢达争锋对决」官包签名证书写进 SignSpoof.java（幂等，字节级替换，UTF-8 其余内容原样保留）。

用法: patch_signspoof.py <SignSpoof.java> <cert_b64.txt>
"""
import hashlib
import io
import sys

SRC, CERTB64 = sys.argv[1], sys.argv[2]

PKG = "com.kingnet.gundam.mg.normal"
CONST_NAME = "GDZFDJ"
MARK = "// 敢达争锋对决（%s）官包签名证书" % PKG

b64 = open(CERTB64, "r", encoding="ascii").read().strip()
b64 = "".join(b64.split())


def java_literal(s, width=118, indent="            "):
    parts = [s[i:i + width] for i in range(0, len(s), width)]
    out = [indent + '"%s"' % parts[0]]
    for p in parts[1:]:
        out.append(indent + "        + \"%s\"" % p)
    return "\n".join(out)


const_block = (
    "\n"
    "    %s\n" % MARK +
    "    // 取自官包 base.apk META-INF（keytool -printcert -rfc -jarfile）\n"
    "    // subject/issuer: C=zh, ST=sh, L=sh, O=kingnet, OU=qmqj, CN=qmqj\n"
    "    // MD5:     28185fbfd55a46a024bd75bd1569fd72\n"
    "    // SHA-1:   767e4adea7709285ff828e83bd93dc8a87301cca\n"
    "    // SHA-256: c1ace7c113c98d8ebfe370d0e1e75da8c504d98b853504751a8cb5ed8bc23185\n"
    "    // 有效期: 2015-01-16 → 2069-10-19；DER 852 B / RSA-2048 / SHA256withRSA\n"
    "    // 用途: 模组包自签后回灌官签，喂给游戏的完整性/渠道校验\n"
    "    private static final String %s_PKG = \"%s\";\n" % (CONST_NAME, PKG) +
    "    private static final String %s_CERT_B64 =\n" % CONST_NAME +
    java_literal(b64) + ";\n"
)

src = open(SRC, "rb").read()
txt = src.decode("utf-8")

if MARK in txt:
    print("[signspoof] already patched, no change")
    sys.exit(0)

anchor1 = "    public static void install() {"
anchor2 = "                fakes.put(JFQS_PKG, new Signature(Base64.decode(JFQS_CERT_B64, Base64.DEFAULT)));\n"
assert txt.count(anchor1) == 1, "anchor1 hits=%d" % txt.count(anchor1)
assert txt.count(anchor2) == 1, "anchor2 hits=%d" % txt.count(anchor2)

txt = txt.replace(anchor1, const_block + "\n" + anchor1, 1)
txt = txt.replace(
    anchor2,
    anchor2 + "                fakes.put(%s_PKG, new Signature(Base64.decode(%s_CERT_B64, Base64.DEFAULT)));\n"
    % (CONST_NAME, CONST_NAME),
    1,
)

data = txt.encode("utf-8")
open(SRC, "wb").write(data)
print("[signspoof] patched %s (%d bytes)" % (SRC, len(data)))
print("[signspoof] decoded cert md5 = %s" % hashlib.md5(
    __import__("base64").b64decode(b64)).hexdigest())
