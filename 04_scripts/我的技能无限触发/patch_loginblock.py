#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""我的技能无限触发 A 轨：把 LoginBlock.onViewCreated 的 SSO 入口改成 SDK 自带 web 登录。

同一套 TapTap SDK 形态（com.taptap.sdk.ui.LoginBlock / LoginClient / WebViewHandler），
方法与 04_scripts/不一样西游/patch_loginblock.py 一致；锚点本轮已按本包 classes.dex 现取核对：
  LoginBlock.smali 中 LoginClient 的 new-instance / <init>(ActivityDelegate) / iput client 字段
  / sendLoginRequest(LoginRequest) 各命中 1 次。

为什么改这里（不一样西游 2026-11-02 两次实测失败的定案，适用同一 SDK 形态）：
  ① 在 LoginClient.sendLoginRequest 里提前 return ⇒ 跳过 cloudGameHandler 的 bind 记账，
     销毁时 unbindService 抛 IllegalArgumentException: Service not registered ⇒ 点开始游戏闪退。
  ② 把 TapTapActivity 的 new LoginBlock 换成 new WebBlock ⇒ 绕过 WebViewHandler.authorize 里
     setLoginCallback + startBlock ⇒ 登录页永远转圈。
  正解：仍从 LoginBlock 进入，把「建 LoginClient 并发 SSO」换成「建 WebViewHandler + authorize(request)」。

用法：patch_loginblock.py <LoginBlock.smali 路径>   （幂等：已打过则跳过）
"""
import sys
from pathlib import Path

PAIRS = [
    ("""    invoke-direct {v0, v1}, Lcom/taptap/sdk/ui/LoginClient;-><init>(Lcom/taptap/sdk/ui/ActivityDelegate;)V

    iput-object v0, p0, Lcom/taptap/sdk/ui/LoginBlock;->client:Lcom/taptap/sdk/ui/LoginClient;

""",
     """    invoke-direct {v0, v1}, Lcom/taptap/sdk/ui/WebViewHandler;-><init>(Lcom/taptap/sdk/ui/ActivityDelegate;)V

"""),
    ("new-instance v0, Lcom/taptap/sdk/ui/LoginClient;",
     "new-instance v0, Lcom/taptap/sdk/ui/WebViewHandler;"),
    ("invoke-virtual {v0, p1}, Lcom/taptap/sdk/ui/LoginClient;->sendLoginRequest(Lcom/taptap/sdk/LoginRequest;)V",
     "invoke-virtual {v0, p1}, Lcom/taptap/sdk/ui/WebViewHandler;->authorize(Lcom/taptap/sdk/LoginRequest;)V"),
]


def main() -> int:
    path = Path(sys.argv[1])
    text = path.read_text(encoding="utf-8")

    if "WebViewHandler;->authorize(Lcom/taptap/sdk/LoginRequest;)V" in text:
        print("  [skip] 已打过补丁（幂等）")
        return 0

    for old, new in PAIRS:
        n = text.count(old)
        if n != 1:
            raise SystemExit("ERROR: 锚点命中数异常(%d)：%s" % (n, old.strip()))
        text = text.replace(old, new, 1)

    path.write_text(text, encoding="utf-8")
    print("  patched LoginBlock.onViewCreated: LoginClient(SSO) -> WebViewHandler.authorize(web)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
