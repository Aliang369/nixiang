#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""不一样西游 A 轨：把 LoginBlock.onViewCreated 的 SSO 入口改成 SDK 自带 web 登录。

为什么改这里（2026-11-02 两次实测失败后的定案）：
  ① 在 LoginClient.sendLoginRequest 里提前 return ⇒ 跳过 cloudGameHandler 的 bind 记账，
     销毁时 unbindService 抛 IllegalArgumentException: Service not registered ⇒ 点开始游戏闪退。
  ② 把 TapTapActivity 的 new LoginBlock 换成 new WebBlock ⇒ 绕过 WebViewHandler.authorize 里的
     setLoginCallback + startBlock ⇒ 登录页永远转圈（WebBlock 收不到回调）。
  正解：仍从 LoginBlock 进入，但把「建 LoginClient 并发 SSO 请求」换成
  「建 WebViewHandler 并 authorize(request)」，即 SDK 自己的完整 web 路径，
  LoginClient 整个不被创建 ⇒ 无 bind/unbind 记账问题。

用法：patch_loginblock.py <LoginBlock.smali 路径>   （幂等：已打过则跳过）
"""
import sys
from pathlib import Path

PAIRS = [
    # ① 连「建 LoginClient + 存进 client 字段」整块替换：字段类型是 LoginClient，
    #    若把 WebViewHandler 存进去 verifier 会直接拒绝，必须一起删掉。
    #    （<init> 里那条 `iput-object v0 …->client` 不同块，不能误删 ⇒ 用整块锚点。）
    ("""    invoke-direct {v0, v1}, Lcom/taptap/sdk/ui/LoginClient;-><init>(Lcom/taptap/sdk/ui/ActivityDelegate;)V

    iput-object v0, p0, Lcom/taptap/sdk/ui/LoginBlock;->client:Lcom/taptap/sdk/ui/LoginClient;

""",
     """    invoke-direct {v0, v1}, Lcom/taptap/sdk/ui/WebViewHandler;-><init>(Lcom/taptap/sdk/ui/ActivityDelegate;)V

"""),
    # ② SSO 客户端类 → web handler
    ("new-instance v0, Lcom/taptap/sdk/ui/LoginClient;",
     "new-instance v0, Lcom/taptap/sdk/ui/WebViewHandler;"),
    # ③ SSO 请求 → web authorize（SDK 自带 callback + startBlock）
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
    print("  patched LoginBlock.onViewCreated: LoginClient(SSO) → WebViewHandler.authorize(web)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
