#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""异世界当欧皇 — TapTap 登录改包法（双轨制的「登录方式」轨）
问题：改签后 TapSDK 3.29 客户端 SSO 返回 signature not match
      （com.taptap.sdk.ui.TapTapActivity → 客户端授权失败）。
做法：本包是老 SDK（Block/Handler 结构，无 ClientLoginUtils）。
      LoginClient.sendLoginRequest() 先 try ActivityHandler.authorize()，
      catch(Exception) 后依次 handlePhoneVerify → handlePhoneVerify2 → webViewHandler.authorize()。
      ⇒ 让 ActivityHandler.authorize() 恒抛 ActivityNotFoundException，
        即走 SDK 自带的 WebView 网页登录（WebBlock）。
锚点：com/taptap/sdk/ui/ActivityHandler.smali  .method public authorize(Lcom/taptap/sdk/LoginRequest;)V

用法：python3 04_scripts/异世界当欧皇/ysjdoh_patch_taptap_login.py
产物：13_scratch/ysjdoh_taptap/classes2.dex（由 ysjdoh_surgical.py 塞回 APK）
"""
import hashlib, re, shutil, subprocess, sys, zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
APK = ROOT / "02_apks" / "异世界当欧皇_官方_com.cxgame.ysjdoh.apk"
DEX = "classes2.dex"
WORK = ROOT / "13_scratch" / "ysjdoh_taptap"
SMALI = ROOT / "01_tools" / "smali" / "smali"
BAKSMALI = ROOT / "01_tools" / "smali" / "baksmali"
OUT_DEX = WORK / "classes2.dex"

TARGET = "com/taptap/sdk/ui/ActivityHandler.smali"
NEW_METHOD = '''.method public authorize(Lcom/taptap/sdk/LoginRequest;)V
    .registers 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    new-instance v0, Landroid/content/ActivityNotFoundException;

    const-string v1, "starry_force_web_login"

    invoke-direct {v0, v1}, Landroid/content/ActivityNotFoundException;-><init>(Ljava/lang/String;)V

    throw v0
.end method
'''


def run(cmd):
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    if r.returncode != 0:
        print(r.stdout[-2000:]); print(r.stderr[-2000:])
        raise SystemExit("FAILED: %s" % cmd[0])
    return r.stdout


def main():
    if WORK.exists():
        shutil.rmtree(WORK)
    WORK.mkdir(parents=True)
    src = WORK / DEX
    with zipfile.ZipFile(APK) as z:
        src.write_bytes(z.read(DEX))
    print("classes2.dex sha256 =", hashlib.sha256(src.read_bytes()).hexdigest()[:16])

    tree = WORK / "smali"
    run([BAKSMALI, "d", src, "-o", tree])
    f = tree / TARGET
    if not f.exists():
        raise SystemExit("锚点文件不存在: %s" % TARGET)
    t = f.read_text(encoding="utf-8")
    pat = re.compile(r"\.method public authorize\(Lcom/taptap/sdk/LoginRequest;\)V.*?\n\.end method\n", re.S)
    m = pat.search(t)
    if not m:
        raise SystemExit("未找到 authorize 方法块")
    if m.group(0).count(".method") != 1:
        raise SystemExit("authorize 方法块解析异常")
    t = t[:m.start()] + NEW_METHOD + t[m.end():]
    f.write_text(t, encoding="utf-8")
    print("patched", TARGET)

    run([SMALI, "assemble", tree, "-o", OUT_DEX])
    print("out:", OUT_DEX, OUT_DEX.stat().st_size, "B  sha256",
          hashlib.sha256(OUT_DEX.read_bytes()).hexdigest()[:16])

    # 自检：反编译产物，确认 authorize 只剩抛异常
    chk = WORK / "smali_chk"
    run([BAKSMALI, "d", OUT_DEX, "-o", chk])
    body = (chk / TARGET).read_text(encoding="utf-8")
    i = body.find(".method public authorize(Lcom/taptap/sdk/LoginRequest;)V")
    j = body.find(".end method", i)
    blk = body[i:j]
    if "starry_force_web_login" not in blk or "ActivityDelegate" in blk:
        raise SystemExit("产物自检失败：authorize 未被替换干净")
    print("产物自检 OK（authorize 恒抛 starry_force_web_login）")


if __name__ == "__main__":
    main()
