#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""黎明前20分钟 —— TapTap 改包法（A 轨：登录方式/渠道分支）

按 AGENTS.md §3 的现行双轨制：
  · 签名/完整性（结果类校验）→ B 轨运行时替换，见 build_killer_dex_lmq20fz.sh
  · 登录方式/渠道分支（流程类）→ 本脚本，改包法，只动这一个分支点
  **同一链条禁双修**：本脚本不碰 InitializerServiceImpl / GatekeeperRepository。

改点（本轮 smali 实读，TapSDK 4.x + tap-login_release）：
  com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.provide(LoginRequest)
    WhenMappings 映射后 aget v2,v3,v2 → 1=ClientLoginFragment / 2=CloudGameFragment / 3=WebLoginFragment
  在 aget 之后插入 const/4 v2, 0x3 ⇒ 恒走 WebLoginFragment（网页登录），
  与蛋壳战机 1.2.1 的既有实测配方一致。

产物：13_scratch/lmq20fz_taptap/classes5.dex（surgical 脚本按同名替换包内 classes5.dex）
"""
import hashlib
import shutil
import subprocess
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ORIG = ROOT / "02_apks" / "原版_TapTap_20261103" / "twenty_minutes.apk"
ORIG_MD5 = "5c35c88a6f5eb3cc0f1fb158f35c3bd3"
WORK = ROOT / "13_scratch" / "lmq20fz_taptap"
SMALI = ROOT / "01_tools" / "smali" / "smali"
BAKSMALI = ROOT / "01_tools" / "smali" / "baksmali"

TARGET_DEX = "classes5.dex"
TARGET_SMALI = ("com/taptap/sdk/login/impl/ui/fragment/"
                "DefaultTapLoginFragmentProvider.smali")
NEEDLE = "    aget v2, v3, v2\n\n    const/4 v3, 0x1"
PATCHED = "    aget v2, v3, v2\n\n    const/4 v2, 0x3\n\n    const/4 v3, 0x1"


def run(cmd):
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True)
    if r.returncode != 0:
        raise SystemExit("FAILED: %s\n%s\n%s" % (" ".join(str(c) for c in cmd),
                                                 r.stdout[-2000:], r.stderr[-2000:]))
    return r.stdout


def main():
    if not ORIG.exists():
        raise SystemExit("missing orig apk: %s" % ORIG)
    md5 = hashlib.md5(ORIG.read_bytes()).hexdigest()
    if md5 != ORIG_MD5:
        raise SystemExit("orig md5 mismatch: %s" % md5)

    if WORK.exists():
        shutil.rmtree(WORK)
    WORK.mkdir(parents=True)

    with zipfile.ZipFile(ORIG) as z:
        if TARGET_DEX not in z.namelist():
            raise SystemExit("%s 不在底包里" % TARGET_DEX)
        (WORK / TARGET_DEX).write_bytes(z.read(TARGET_DEX))

    print("[1/4] baksmali", TARGET_DEX)
    dec = WORK / "smali_in"
    run([BAKSMALI, "d", "-o", dec, WORK / TARGET_DEX])

    smali_file = dec / TARGET_SMALI
    if not smali_file.exists():
        raise SystemExit("missing %s" % smali_file)
    text = smali_file.read_text(encoding="utf-8")
    if PATCHED in text:
        raise SystemExit("already patched（重复打补丁会让 dex 漂移）")
    hits = text.count(NEEDLE)
    if hits != 1:
        raise SystemExit("锚点命中 %d 次（期望 1）—— TapSDK 版本变了，按版本现取锚点" % hits)

    print("[2/4] patch provide() -> 恒 WebLoginFragment")
    smali_file.write_text(text.replace(NEEDLE, PATCHED), encoding="utf-8")

    print("[3/4] smali assemble ->", TARGET_DEX)
    out_dex = WORK / TARGET_DEX
    out_dex.unlink()
    run([SMALI, "assemble", "-o", out_dex, dec])

    print("[4/4] 回读校验")
    verify = WORK / "verify"
    run([BAKSMALI, "d", "-o", verify, out_dex])
    vtext = (verify / TARGET_SMALI).read_text(encoding="utf-8")
    if PATCHED not in vtext:
        raise SystemExit("回读未见补丁")
    for must in ("WebLoginFragment$Companion;->newInstance",
                 "ClientLoginFragment$Companion;->newInstance"):
        if must not in vtext:
            raise SystemExit("分支代码缺失: %s" % must)
    print("   ok  classes5.dex %d B  md5 %s"
          % (out_dex.stat().st_size, hashlib.md5(out_dex.read_bytes()).hexdigest()))


if __name__ == "__main__":
    main()
