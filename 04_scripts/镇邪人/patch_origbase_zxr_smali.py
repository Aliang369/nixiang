#!/usr/bin/env python3
"""镇邪人 1.0.3(vc11 taptap 原版) 原版底包外科 smali 补丁：菜单入口 + 免广告 + TapSign 签名固定。

只做定点文本替换，不做整包 apktool b：
  application  com/sanqi/jmybt/game37/JMYBTApplication  → attachBaseContext 里早期 loadLibrary
  activity     com/sanqi/jmybt/game37/JMYBTActivity      → onCreate 早期 load + MenuService.start，
                                                          onResume/onPause/onUserLeaveHint 生命周期通知，
                                                          get_ret/set_ret 静态桥（对齐 ZxrHooks）
  sdkmanager   com/sanqi/jmybt/game37/sdk/SdkManager     → SdkShowAdReward 前置 shouldSkipAd 分支
  tapsign      com/taptap/sdk/kit/internal/utils/TapSignKit → getSign 固定原版证书 MD5
"""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

STARRY_ACT = "Lcom/starry/magic/menu/MenuService;"
MAGIC_LIB = "starry_magic"


def _method_block(text: str, signature_regex: str, label: str) -> tuple[int, int]:
    m = re.search(signature_regex, text)
    if not m:
        raise SystemExit(f"ERROR: {label}: method not found ({signature_regex})")
    start = m.start()
    end = text.find("\n.end method", start)
    if end < 0:
        raise SystemExit(f"ERROR: {label}: .end method not found")
    return start, end + len("\n.end method")


OPCODE_START = re.compile(r"^[a-z][a-z0-9/\-_]*(\s|$)")


def _insert_before_first_instruction(block: str, snippet: str) -> str:
    """在方法体第一条指令前插入片段（跳过 .registers/.annotation/.line/.local 等指令行）。"""
    lines = block.split("\n")
    in_annotation = False
    for i, line in enumerate(lines):
        stripped = line.strip()
        if not stripped or stripped.startswith("#"):
            continue
        if stripped.startswith(".annotation"):
            in_annotation = True
            continue
        if stripped.startswith(".end annotation"):
            in_annotation = False
            continue
        if in_annotation or stripped.startswith("."):
            continue
        if OPCODE_START.match(stripped):
            lines.insert(i, snippet.rstrip("\n"))
            return "\n".join(lines)
    raise SystemExit("ERROR: cannot locate first instruction in method body")


def patch_application(path: Path) -> None:
    s = path.read_text(encoding="utf-8")
    if "starry_magic" in s and "attachBaseContext" in s:
        print("  app: attachBaseContext already patched")
        return
    if ".method protected attachBaseContext(Landroid/content/Context;)V" in s:
        raise SystemExit("ERROR: app: attachBaseContext exists without starry load")
    method = """
.method protected attachBaseContext(Landroid/content/Context;)V
    .locals 1
    .param p1, "base"    # Landroid/content/Context;

    invoke-super {p0, p1}, Lcom/sqwan/msdk/SQApplication;->attachBaseContext(Landroid/content/Context;)V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    return-void
.end method
"""
    s = s.rstrip("\n") + "\n" + method
    path.write_text(s, encoding="utf-8")
    print("  app: attachBaseContext → loadLibrary(starry_magic)")


def patch_activity(path: Path) -> None:
    s = path.read_text(encoding="utf-8")
    if "MenuService;->start" in s:
        print("  act: MenuService.start already present")
    else:
        start, end = _method_block(
            s, r"\.method protected onCreate\(Landroid/os/Bundle;\)V", "onCreate"
        )
        block = s[start:end]
        super_on = "invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;->onCreate(Landroid/os/Bundle;)V"
        if block.count(super_on) != 1:
            raise SystemExit("ERROR: act: onCreate super call not unique")
        early = (
            '    const-string v0, "starry_magic"\n\n'
            "    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n\n"
            "    " + super_on
        )
        block = block.replace("    " + super_on, early, 1)
        tail_idx = block.rfind("    return-void")
        if tail_idx < 0:
            raise SystemExit("ERROR: act: onCreate return-void missing")
        start_call = (
            "    invoke-static {p0}, "
            + STARRY_ACT
            + "->start(Landroid/content/Context;)V\n\n"
        )
        block = block[:tail_idx] + start_call + block[tail_idx:]
        s = s[:start] + block + s[end:]
        print("  act: onCreate → early loadLibrary + MenuService.start")

    for label, sig, call, marker in (
        (
            "onResume",
            r"\.method protected onResume\(\)V",
            "    invoke-static {p0}, " + STARRY_ACT + "->notifyHostResumed(Landroid/app/Activity;)V\n\n",
            "MenuService;->notifyHostResumed",
        ),
        (
            "onPause",
            r"\.method protected onPause\(\)V",
            "    invoke-static {}, " + STARRY_ACT + "->notifyHostLeaving()V\n\n",
            "MenuService;->notifyHostLeaving",
        ),
    ):
        start, end = _method_block(s, sig, label)
        block = s[start:end]
        if marker in block:
            print(f"  act: {label} already patched")
            continue
        tail_idx = block.rfind("    return-void")
        if tail_idx < 0:
            raise SystemExit(f"ERROR: act: {label} return-void missing")
        block = block[:tail_idx] + call + block[tail_idx:]
        s = s[:start] + block + s[end:]
        print(f"  act: {label} → notify")

    if "onUserLeaveHint" not in s:
        leave = """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
        marker = ".method protected onPause()V"
        if marker not in s:
            raise SystemExit("ERROR: act: onPause marker missing for onUserLeaveHint insert")
        s = s.replace(marker, leave + marker, 1)
        print("  act: onUserLeaveHint → notifyHostLeaving")

    if "ZxrHooks;->getRet" not in s:
        s = s.rstrip("\n") + """

.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/ZxrHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/ZxrHooks;->setRet(IZ)V

    return-void
.end method
"""
        print("  act: get_ret/set_ret → ZxrHooks")
    path.write_text(s, encoding="utf-8")


def patch_sdkmanager(path: Path) -> None:
    s = path.read_text(encoding="utf-8")
    if "ZxrHooks;->shouldSkipAd" in s:
        print("  sdk: SdkShowAdReward already patched")
        return
    start, end = _method_block(
        s, r"\.method public SdkShowAdReward\(Ljava/lang/String;\)V", "SdkShowAdReward"
    )
    block = s[start:end]
    preamble = """    invoke-static {}, Lcom/starry/magic/cocos/ZxrHooks;->shouldSkipAd()Z

    move-result v0

    if-eqz v0, :cond_zxr_ad

    const-string v0, "[SdkManager](SdkShowAdReward) starry skip ad -> success"

    invoke-static {v0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->logi(Ljava/lang/String;)V

    new-instance v0, Lcom/alibaba/fastjson/JSONObject;

    invoke-direct {v0}, Lcom/alibaba/fastjson/JSONObject;-><init>()V

    const/4 v1, 0x1

    invoke-static {v1}, Ljava/lang/Boolean;->valueOf(Z)Ljava/lang/Boolean;

    move-result-object v2

    const-string v1, "result"

    invoke-virtual {v0, v1, v2}, Lcom/alibaba/fastjson/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Ljava/lang/Object;

    const/4 v1, 0x1

    invoke-static {v1}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v2

    const-string v1, "state"

    invoke-virtual {v0, v1, v2}, Lcom/alibaba/fastjson/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Ljava/lang/Object;

    const-string v1, "SdkShowAdReward"

    invoke-virtual {p0, v1, v0}, Lcom/sanqi/jmybt/game37/sdk/SdkManager;->call(Ljava/lang/String;Lcom/alibaba/fastjson/JSONObject;)V

    return-void

    :cond_zxr_ad
"""
    block = _insert_before_first_instruction(block, preamble)
    s = s[:start] + block + s[end:]
    path.write_text(s, encoding="utf-8")
    print("  sdk: SdkShowAdReward → shouldSkipAd 前置分支")


def patch_tapsign(path: Path, orig_md5: str) -> None:
    s = path.read_text(encoding="utf-8")
    if orig_md5 in s and "starry_sign_bypass" in s:
        print("  tap: getSign already fixed")
        return
    new = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;

    # starry_sign_bypass: 固定原版证书 MD5
    const-string v0, "{orig_md5}"

    return-object v0
.end method"""
    s2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        new,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("ERROR: tap: TapSignKit.getSign patch failed")
    path.write_text(s2, encoding="utf-8")
    print("  tap: TapSignKit.getSign → 原版证书 MD5")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--application", type=Path)
    ap.add_argument("--activity", type=Path)
    ap.add_argument("--sdkmanager", type=Path)
    ap.add_argument("--tapsignkit", type=Path)
    ap.add_argument("--orig-md5", default="")
    a = ap.parse_args()
    if a.application:
        patch_application(a.application)
    if a.activity:
        patch_activity(a.activity)
    if a.sdkmanager:
        patch_sdkmanager(a.sdkmanager)
    if a.tapsignkit:
        if not a.orig_md5:
            raise SystemExit("ERROR: --orig-md5 required with --tapsignkit")
        patch_tapsign(a.tapsignkit, a.orig_md5)
    return 0


if __name__ == "__main__":
    sys.exit(main())
