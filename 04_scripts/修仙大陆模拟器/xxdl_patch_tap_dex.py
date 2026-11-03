#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""修仙大陆模拟器 —— TapTap A 轨（改包法）可复现重打 classes_patched.dex

现行双轨制（skill/re-analyze/login/taptap-web-login.md 顶部口径）：
  A 改包法 = 登录方式/渠道分支：ClientLoginUtils.isClientLogin(...)Z 恒 false ⇒ SDK 回落 WebView 登录
  B 运行时 = 签名/完整性：classes9.dex 的 bin.mt.signature.KillerApplication193（见 build_killer_dex_xxdlmnq.sh）

本脚本只做 A：解官方 classes.dex → baksmali → 改写一个方法 → smali 重组 → 回读自检。
产物：13_scratch/xxdl_taptap/classes_patched.dex（xxdl_surgical.py 作为 classes.dex 注入）

用法：python3 04_scripts/修仙大陆模拟器/xxdl_patch_tap_dex.py
"""
import hashlib
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
APK = ROOT / "02_apks" / "原版_TapTap_20261103" / "sky_xxdl_dlt.apk"
WORK = ROOT / "13_scratch" / "xxdl_taptap"
SMALI = ROOT / "01_tools" / "smali" / "smali"
BAKSMALI = ROOT / "01_tools" / "smali" / "baksmali"
TARGET = "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
SIG = ".method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z"


def run(cmd):
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True)
    if r.returncode != 0:
        raise SystemExit("FAILED: %s\n%s\n%s" % (" ".join(str(c) for c in cmd), r.stdout[-1500:], r.stderr[-1500:]))
    return r.stdout


def main():
    WORK.mkdir(parents=True, exist_ok=True)
    dex = WORK / "classes.dex"
    with zipfile.ZipFile(APK) as z:
        dex.write_bytes(z.read("classes.dex"))

    dec = WORK / "dec1"
    if dec.exists():
        shutil.rmtree(dec)
    run([BAKSMALI, "d", "-o", dec, dex])

    p = dec / TARGET
    text = p.read_text(encoding="utf-8")
    if SIG not in text:
        raise SystemExit("找不到 isClientLogin 签名（SDK 版本变了？重新抓锚点）")
    i = text.index(SIG)
    j = text.index(".end method", i) + len(".end method")
    text = text[:i] + SIG + "\n    .registers 4\n\n    const/4 p1, 0x0\n\n    return p1\n.end method" + text[j:]
    p.write_text(text, encoding="utf-8")

    out = WORK / "classes_patched.dex"
    run([SMALI, "assemble", "-o", out, dec])

    # 回读自检：方法体必须只剩 const/4 + return
    ver = WORK / "verify_patched"
    if ver.exists():
        shutil.rmtree(ver)
    run([BAKSMALI, "d", "-o", ver, out])
    body = (ver / TARGET).read_text(encoding="utf-8")
    k = body.index(SIG)
    seg = body[k:body.index(".end method", k)]
    if "const/4 p1, 0x0" not in seg or "return p1" not in seg or "resolveActivity" in seg:
        raise SystemExit("回读自检失败：补丁未生效")
    shutil.rmtree(dec)
    shutil.rmtree(ver)

    md5 = hashlib.md5(out.read_bytes()).hexdigest()
    print("classes_patched.dex -> %s (%d B, md5 %s)" % (out, out.stat().st_size, md5))


if __name__ == "__main__":
    main()
