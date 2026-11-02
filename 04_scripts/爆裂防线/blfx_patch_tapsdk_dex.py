#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
爆裂防线 1.02.01 — TapTap 改包法（静态 dex 补丁，2026-10-28 定案口径）

本作 TapSDK 走**新版路径**（LoginTypes / DefaultTapLoginFragmentProvider /
LoginRequest.loginType / embedded_web），包内无旧版 com.taptap.sdk.ui.TapTapActivity。
锚点全部按**当前底包现取**（2026-11-02 反编译 classes.dex 核对）：

  ① com/taptap/sdk/initializer/repository/GatekeeperRepository
       isGatekeeperLoadedSuccessfully()Z  → 恒 true（门闩放行；最底层，链路上涌）
  ② 同上 showErrorToast(TapNetworkException)V → 方法体清空 return-void（掐验签弹窗）
  ③ com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider
       provide(LoginRequest)Fragment：`aget v2,v3,v2` 后插 `const/4 v2, 0x3`
       ⇒ WhenMappings 命中 WEB 分支 ⇒ WebLoginFragment（不动 instance-of / 不改 .registers）
  ④ com/taptap/sdk/login/impl/utils/ClientLoginUtils
       isClientLogin(Activity,Actions)Z → 恒 false
       ⇒ LoginTypes$Companion.from(Activity,Actions) 不再返回 CLIENT（覆盖另一条路由）

同一链条只改包，不叠运行时伪装（TapLoginForceWeb 白名单对本包不再承担登录分流）。

用法：
  python 04_scripts/爆裂防线/blfx_patch_tapsdk_dex.py \
      [--apk <原包>] [--out <classes.dex>] [--work <.workbuddy/scratch/blfx_tapdex>]
    --keep-work  保留反编译目录（排查用）
"""
import argparse
import re
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
JAVA = None


def die(msg):
    print("ERROR: " + msg, file=sys.stderr)
    sys.exit(1)


def java_bin():
    import os
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


def replace_method(text, sig_regex, new_body, label):
    pat = re.compile(r"(\.method " + sig_regex + r"\n).*?\n\.end method\n", re.S)
    m = pat.search(text)
    if not m:
        die("锚点未命中：%s（底包结构可能已变）" % label)
    out = text[:m.start()] + new_body + text[m.end():]
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--apk", default=str(REPO / "02_apks/爆裂防线/爆裂防线_1.02.01_original_from_device.apk"))
    ap.add_argument("--out", default=str(REPO / "07_patched/爆裂防线/surgical/classes.dex"))
    ap.add_argument("--work", default=str(REPO / ".workbuddy/scratch/blfx_tapdex"))
    ap.add_argument("--keep-work", action="store_true")
    a = ap.parse_args()

    apk = Path(a.apk)
    out = Path(a.out)
    work = Path(a.work)
    if not apk.is_file():
        die("底包不存在: %s" % apk)

    java = java_bin()
    baksmali = REPO / "01_tools/smali/baksmali-fat.jar"
    smali = REPO / "01_tools/smali/smali-fat.jar"
    for j in (baksmali, smali):
        if not j.is_file():
            die("缺工具: %s" % j)

    dec = work / "dec"
    if work.exists():
        shutil.rmtree(work)
    work.mkdir(parents=True, exist_ok=True)
    dec.mkdir(parents=True, exist_ok=True)

    print("[1/6] 提取 classes.dex")
    with zipfile.ZipFile(apk) as z:
        raw = z.read("classes.dex")
    src_dex = work / "classes_orig.dex"
    src_dex.write_bytes(raw)
    print("      %d B" % len(raw))

    print("[2/6] baksmali 反编译 → %s" % dec)
    r = subprocess.run([java, "-jar", str(baksmali), "d", str(src_dex), "-o", str(dec)])
    if r.returncode != 0:
        die("baksmali rc=%d" % r.returncode)

    # ---- ① 门闩恒 true ----
    p1 = dec / "com/taptap/sdk/initializer/repository/GatekeeperRepository.smali"
    t = p1.read_text(encoding="utf-8")
    body1 = (".method public final isGatekeeperLoadedSuccessfully()Z\n"
             "    .registers 2\n\n"
             "    const/4 v0, 0x1\n\n"
             "    return v0\n"
             ".end method\n")
    t = replace_method(t, r"public final isGatekeeperLoadedSuccessfully\(\)Z", body1,
                       "GatekeeperRepository.isGatekeeperLoadedSuccessfully")
    # ---- ② 弹窗掐掉 ----
    body2 = (".method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V\n"
             "    .registers 5\n\n"
             "    return-void\n"
             ".end method\n")
    t = replace_method(t, r"private final showErrorToast\(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;\)V",
                       body2, "GatekeeperRepository.showErrorToast")
    p1.write_text(t, encoding="utf-8")
    print("[3/6] GatekeeperRepository 补丁完成（门闩恒 true + 弹窗 no-op）")

    # ---- ③ provide 强制 WEB 分支 ----
    p2 = dec / "com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali"
    t = p2.read_text(encoding="utf-8")
    anchor = "    aget v2, v3, v2\n"
    n = t.count(anchor)
    if n != 1:
        die("provide 的 aget 锚点命中 %d 次（期望 1）" % n)
    t = t.replace(anchor, anchor + "\n    const/4 v2, 0x3\n", 1)
    p2.write_text(t, encoding="utf-8")
    print("[4/6] DefaultTapLoginFragmentProvider.provide → 恒 WEB 分支")

    # ---- ④ isClientLogin 恒 false ----
    p3 = dec / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
    t = p3.read_text(encoding="utf-8")
    body3 = (".method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z\n"
             "    .registers 4\n\n"
             "    const/4 v0, 0x0\n\n"
             "    return v0\n"
             ".end method\n")
    t = replace_method(t, r"public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z",
                       body3, "ClientLoginUtils.isClientLogin")
    p3.write_text(t, encoding="utf-8")
    print("[5/6] ClientLoginUtils.isClientLogin → 恒 false")

    out.parent.mkdir(parents=True, exist_ok=True)
    tmp_dex = work / "classes_patched.dex"
    print("[6/6] smali 汇编 → %s" % tmp_dex)
    r = subprocess.run([java, "-jar", str(smali), "assemble", str(dec), "-o", str(tmp_dex)])
    if r.returncode != 0:
        die("smali assemble rc=%d" % r.returncode)

    data = tmp_dex.read_bytes()
    # 字节级断言：门闩 true 的 smali 特征不可直接搜（已编译），改搜源码残留
    for f, key in ((p1, "const/4 v0, 0x1"), (p2, "const/4 v2, 0x3"), (p3, "const/4 v0, 0x0")):
        if key not in f.read_text(encoding="utf-8"):
            die("补丁回读失败: %s 缺 %s" % (f, key))
    if len(data) < 1000:
        die("产物 dex 异常小: %d B" % len(data))

    out.write_bytes(data)
    print("[ok] %s (%d B)" % (out, len(data)))
    if not a.keep_work:
        shutil.rmtree(work, ignore_errors=True)


if __name__ == "__main__":
    main()
