#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""修仙王朝模拟器 (com.sky.xxwc.xxt 1.0.4) — TapTap 改包法：强制网页登录 + 验签门闩放行

背景：官方包走 TapSDK 客户端快速认证（SSO），改签后必然 signature not match；
     本包 TapSDK 为**新 SDK**（classes.dex 含 ClientLoginUtils / GatekeeperRepository）。

三处 patch（均为「保留 .registers、整块方法体替换」）：
  1. com/taptap/sdk/login/impl/utils/ClientLoginUtils.isClientLogin(Activity,Actions)Z
       → 恒 false ⇒ SDK fallback 到 WebLoginFragment（内嵌 WebView 网页授权）
  2. com/taptap/sdk/initializer/repository/GatekeeperRepository.isGatekeeperLoadedSuccessfully()Z
       → 恒 true ⇒ 完整性/签名门闩放行（上层 TapTapSdkInitializer / InitializerServiceImpl 全部透传）
  3. 同类 showGatekeeperError()V → return-void（掐掉"签名校验失败"弹窗）

用法：python3 04_scripts/修仙王朝模拟器/xxwc_patch_taptap_login.py
产物：13_scratch/xxwc_taptap/classes.dex（唯一被改的官方 dex）
"""
import re, shutil, subprocess, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PKG = "com.sky.xxwc.xxt"
ORIG = ROOT / "02_apks" / ("修仙王朝模拟器_官方_%s.apk" % PKG)
WORK = ROOT / "13_scratch" / "xxwc_taptap"
BAKSMALI = ROOT / "01_tools" / "smali" / "baksmali"
SMALI = ROOT / "01_tools" / "smali" / "smali"

PATCHES = [
    {
        "file": "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali",
        "sig": ".method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z",
        "body": "    .registers 4\n\n    const/4 v0, 0x0\n\n    return v0\n",
        "why": "isClientLogin=false → Web 登录",
    },
    {
        "file": "com/taptap/sdk/initializer/repository/GatekeeperRepository.smali",
        "sig": ".method public final isGatekeeperLoadedSuccessfully()Z",
        "body": "    .registers 2\n\n    const/4 v0, 0x1\n\n    return v0\n",
        "why": "Gatekeeper 恒成功",
    },
    {
        "file": "com/taptap/sdk/initializer/repository/GatekeeperRepository.smali",
        "sig": ".method public final showGatekeeperError()V",
        "body": "    .registers 5\n\n    return-void\n",
        "why": "掐掉验签失败弹窗",
    },
    {
        "file": "com/taptap/sdk/compliance/internal/TapComplianceInternal.smali",
        "sig": ".method private final goTapFastAuth(Landroid/app/Activity;)V",
        "body": "    .registers 6\n\nconst/4 v0, 0x1\n\nsput-boolean v0, Lcom/taptap/sdk/compliance/internal/TapComplianceInternal;->canPlay:Z\n\nsput-boolean v0, Lcom/taptap/sdk/compliance/internal/TapComplianceInternal;->noticeGameCanPlay:Z\n\ninvoke-direct {p0}, Lcom/taptap/sdk/compliance/internal/TapComplianceInternal;->noticeGamePlayable()V\n\nreturn-void\n",
        "why": "不弹「是否使用快速认证」（实名已过，走游戏自有流程）",
    },
    {
        "file": "com/taptap/sdk/initializer/repository/GatekeeperRepository.smali",
        "sig": ".method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V",
        "body": "    .registers 5\n\n    return-void\n",
        "why": "初始化失败时不弹「签名/Gatekeeper」错误 Toast",
    },
]


def run(cmd):
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True)
    if r.returncode != 0:
        print(r.stdout[-2000:]); print(r.stderr[-2000:])
        raise SystemExit("FAILED: %s" % cmd[0])
    return r.stdout


def replace_method(text, sig, body):
    """把 .method <sig> ... .end method 整块替换（保留 .annotation 之前的原样）"""
    lines = text.split("\n")
    out, i, done = [], 0, False
    while i < len(lines):
        if lines[i].startswith(sig) and not done:
            # 回退：方法名行前的 .annotation 块属于方法注解，smali 中注解在 .method 之后
            out.append(lines[i])
            i += 1
            # 跳过原 body 直到 .end method
            while i < len(lines) and lines[i].strip() != ".end method":
                i += 1
            out.append(body.rstrip("\n"))
            out.append(".end method")
            i += 1
            done = True
            continue
        out.append(lines[i]); i += 1
    return "\n".join(out), done


def main():
    if not ORIG.exists():
        raise SystemExit("missing orig apk")
    WORK.mkdir(parents=True, exist_ok=True)
    dex = WORK / "classes.dex"
    if not dex.exists():
        run(["unzip", "-o", "-q", "-j", ORIG, "classes.dex", "-d", WORK])
    smali_dir = WORK / "smali"
    if smali_dir.exists():
        shutil.rmtree(smali_dir)
    print("[1/4] baksmali classes.dex ->", smali_dir)
    run([BAKSMALI, "d", dex, "-o", smali_dir])

    print("[2/4] patch 3 methods")
    for p in PATCHES:
        f = smali_dir / p["file"]
        t = f.read_text(encoding="utf-8")
        t2, ok = replace_method(t, p["sig"], p["body"])
        if not ok:
            raise SystemExit("锚点未命中: %s" % p["sig"])
        f.write_text(t2, encoding="utf-8")
        print("   ✓", p["why"], "->", p["file"].split("/")[-1])

    print("[3/4] smali assemble ->", dex)
    out_dex = WORK / "classes.patched.dex"
    if out_dex.exists():
        out_dex.unlink()
    run([SMALI, "assemble", smali_dir, "-o", out_dex])

    print("[4/4] 回读自检")
    chk = WORK / "smali_check"
    if chk.exists():
        shutil.rmtree(chk)
    run([BAKSMALI, "d", out_dex, "-o", chk])
    c1 = (chk / PATCHES[0]["file"]).read_text(encoding="utf-8")
    seg = c1.split(PATCHES[0]["sig"], 1)[1].split(".end method", 1)[0]
    if "const/4 v0, 0x0" not in seg or "resolveActivity" in seg:
        raise SystemExit("isClientLogin 回读不符")
    c2 = (chk / PATCHES[1]["file"]).read_text(encoding="utf-8")
    seg2 = c2.split(PATCHES[1]["sig"], 1)[1].split(".end method", 1)[0]
    if "const/4 v0, 0x1" not in seg2:
        raise SystemExit("isGatekeeperLoadedSuccessfully 回读不符")
    seg3 = c2.split(PATCHES[2]["sig"], 1)[1].split(".end method", 1)[0]
    if "return-void" not in seg3 or "showToast" in seg3:
        raise SystemExit("showGatekeeperError 回读不符")
    import hashlib
    print("   ✓ 三处回读全部命中；classes.patched.dex",
          out_dex.stat().st_size, hashlib.sha256(out_dex.read_bytes()).hexdigest()[:16])


if __name__ == "__main__":
    main()
