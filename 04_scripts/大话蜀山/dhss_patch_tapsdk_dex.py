#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""大话蜀山 2.7 (com.cylj.rogue.tp479) —— TapTap A 轨（改包法）重打 classes2.dex

口径：skill/re-analyze/login/taptap-web-login.md 顶部「2026-10-29 双轨制」
  A 改包法 = 登录方式/渠道分支（本脚本）
  B 运行时 = 签名/完整性（本作不做 B；禁止 SignSpoof / TapLoginForceWeb 复活）

底包现取锚点（2.7 / vc13，TapSDK 4.10.7，均落在 classes2.dex）：
  1 com/taptap/sdk/initializer/repository/GatekeeperRepository
      isGatekeeperLoadedSuccessfully()Z -> 恒 true（门闩放行）
  2 同文件 showErrorToast(TapNetworkException)V -> return-void（保留 .registers 5，掐验签弹窗）
  3 com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider
      provide(LoginRequest)Fragment：aget v2,v3,v2 后插 const/4 v2, 0x3 => WebLoginFragment
  4 com/taptap/sdk/login/impl/utils/ClientLoginUtils
      isClientLogin(Activity,Actions)Z -> 恒 false

产物：07_patched/大话蜀山/surgical/classes2.dex
用法：python3 04_scripts/大话蜀山/dhss_patch_tapsdk_dex.py [--apk X] [--out Y] [--keep-work]
"""
import argparse
import hashlib
import re
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
DEX_NAME = "classes2.dex"


def die(msg):
    print("ERROR: " + msg, file=sys.stderr)
    sys.exit(1)


def replace_method(text, sig, new_body, label):
    pat = re.compile(r"\.method " + re.escape(sig) + r"\n.*?\n\.end method\n", re.S)
    m = pat.search(text)
    if not m:
        die("锚点未命中：%s（底包结构可能已变，须按当前底包重新抓点）" % label)
    return text[:m.start()] + new_body + text[m.end():]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--apk", default=str(REPO / "02_apks/大话蜀山/大话蜀山_2.7_taptap_official.apk"))
    ap.add_argument("--out", default=str(REPO / "07_patched/大话蜀山/surgical" / DEX_NAME))
    ap.add_argument("--work", default=str(REPO / "13_scratch/dhss27/tapdex"))
    ap.add_argument("--keep-work", action="store_true")
    a = ap.parse_args()

    apk, out, work = Path(a.apk), Path(a.out), Path(a.work)
    if not apk.is_file():
        die("底包不存在: %s" % apk)
    baksmali = REPO / "01_tools/smali/baksmali"
    smali = REPO / "01_tools/smali/smali"
    for t in (baksmali, smali):
        if not t.is_file():
            die("缺工具: %s" % t)

    if work.exists():
        shutil.rmtree(work)
    work.mkdir(parents=True, exist_ok=True)

    print("[1/6] 提取 %s" % DEX_NAME)
    with zipfile.ZipFile(apk) as z:
        raw = z.read(DEX_NAME)
    src_dex = work / DEX_NAME
    src_dex.write_bytes(raw)
    print("      %d B md5=%s" % (len(raw), hashlib.md5(raw).hexdigest()))

    dec = work / "dec"
    print("[2/6] baksmali -> %s" % dec)
    r = subprocess.run([str(baksmali), "d", "-o", str(dec), str(src_dex)],
                       capture_output=True, text=True)
    if r.returncode != 0:
        die("baksmali rc=%d\n%s\n%s" % (r.returncode, r.stdout[-800:], r.stderr[-800:]))

    p1 = dec / "com/taptap/sdk/initializer/repository/GatekeeperRepository.smali"
    t = p1.read_text(encoding="utf-8")
    b1 = (".method public final isGatekeeperLoadedSuccessfully()Z\n"
          "    .registers 2\n\n    const/4 v0, 0x1\n\n    return v0\n.end method\n")
    t = replace_method(t, "public final isGatekeeperLoadedSuccessfully()Z", b1,
                       "GatekeeperRepository.isGatekeeperLoadedSuccessfully")
    b2 = (".method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V\n"
          "    .registers 5\n\n    return-void\n.end method\n")
    t = replace_method(t,
                       "private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V",
                       b2, "GatekeeperRepository.showErrorToast")
    p1.write_text(t, encoding="utf-8")
    print("[3/6] GatekeeperRepository 补丁完成（门闩恒 true + 弹窗 no-op）")

    p2 = dec / "com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali"
    t = p2.read_text(encoding="utf-8")
    anchor = "    aget v2, v3, v2\n"
    n = t.count(anchor)
    if n != 1:
        die("provide 的 aget 锚点命中 %d 次（期望 1）" % n)
    t = t.replace(anchor, anchor + "\n    const/4 v2, 0x3\n", 1)
    p2.write_text(t, encoding="utf-8")
    print("[4/6] DefaultTapLoginFragmentProvider.provide -> 恒 WEB 分支")

    p3 = dec / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
    t = p3.read_text(encoding="utf-8")
    b3 = (".method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z\n"
          "    .registers 4\n\n    const/4 v0, 0x0\n\n    return v0\n.end method\n")
    t = replace_method(t,
                       "public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z",
                       b3, "ClientLoginUtils.isClientLogin")
    p3.write_text(t, encoding="utf-8")
    print("[5/6] ClientLoginUtils.isClientLogin -> 恒 false")

    tmp = work / "classes2_patched.dex"
    print("[6/6] smali assemble -> %s" % tmp)
    r = subprocess.run([str(smali), "assemble", "-o", str(tmp), str(dec)],
                       capture_output=True, text=True)
    if r.returncode != 0:
        die("smali assemble rc=%d\n%s\n%s" % (r.returncode, r.stdout[-1500:], r.stderr[-1500:]))

    data = tmp.read_bytes()
    for f, key in ((p1, "const/4 v0, 0x1"), (p2, "const/4 v2, 0x3"), (p3, "const/4 v0, 0x0")):
        if key not in f.read_text(encoding="utf-8"):
            die("补丁回读失败: %s 缺 %s" % (f.name, key))
    if len(data) < 1000:
        die("产物 dex 异常小: %d B" % len(data))
    if hashlib.md5(data).hexdigest() == hashlib.md5(raw).hexdigest():
        die("产物 dex 与底包逐字节相同 -> 补丁未生效（pre_install_gate 查12 会 FAIL）")

    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_bytes(data)
    print("[ok] %s (%d B md5=%s)" % (out, len(data), hashlib.md5(data).hexdigest()))
    if not a.keep_work:
        shutil.rmtree(work, ignore_errors=True)


if __name__ == "__main__":
    main()
