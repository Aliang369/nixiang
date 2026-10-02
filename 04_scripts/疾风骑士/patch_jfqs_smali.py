#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
疾风骑士 60.0.1 (com.emay.wxsy.topon) — 星光外科组装 smali 补丁器。

两个模式：
  patch_jfqs_smali.py tap <dec_root> <orig_md5>
      classes14.dex 解码根目录。补丁：
        com/taptap/sdk/kit/internal/utils/TapSignKit.smali
          getSign(Landroid/content/Context;)Ljava/lang/String;  → 固定返回原厂证书 MD5
        com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali
          isClientLogin(Landroid/app/Activity;...Actions;)Z    → 恒 false（回落 WebLoginFragment）

  patch_jfqs_smali.py activity <dec_root>
      classes9.dex 解码根目录。补丁 com/mar/sdk/MarMainActivity.smali：
        onResume()V           → 追加 MenuService;->notifyHostResumed(Landroid/app/Activity;)V
        onUserLeaveHint()V    → 新增，调 MenuService;->notifyHostLeaving()V

设计原则（沿用 patch_zjmc_smali.py）：
  - 幂等：命中标记已存在即跳过（重复跑不重复注入）
  - 精确：每个替换都断言命中数 == 1，否则 SystemExit（宁可失败也不产半成品）
  - 只增不改：不触碰其它 dex / resources.arsc
"""
import re
import sys
from pathlib import Path

# ---------------------------------------------------------------- 常量

TAP_SIGN_REL = "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
CLIENT_LOGIN_REL = "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
CLIENT_LOGIN_ALT = "com/taptap/sdk/login/internal/util/ClientLoginUtils.smali"
MAR_ACTIVITY_REL = "com/mar/sdk/MarMainActivity.smali"

SIGN_MARK = "# starry_sign_bypass"
WEB_MARK = "# starry: force WebLogin"
RESUME_MARK = "# starry: notifyHostResumed"
LEAVE_MARK = "# starry: notifyHostLeaving"

MENU_RESUMED = ("invoke-static {p0}, "
                "Lcom/starry/magic/menu/MenuService;->"
                "notifyHostResumed(Landroid/app/Activity;)V")
MENU_LEAVING = ("invoke-static {}, "
                "Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V")

# ---------------------------------------------------------------- 工具


def die(msg: str) -> None:
    raise SystemExit(msg)


def read(p: Path) -> str:
    return p.read_text(encoding="utf-8", errors="surrogateescape")


def write(p: Path, s: str) -> None:
    p.write_text(s, encoding="utf-8", errors="surrogateescape")


def find(root: Path, rel: str) -> Path:
    p = root / rel
    if p.is_file():
        return p
    hits = list(root.rglob(Path(rel).name))
    if not hits:
        die("smali not found: %s (under %s)" % (rel, root))
    return hits[0]


def method_block(text: str, sig: str) -> tuple:
    """返回 (start, body_start, body_end, end_of_block)，body 为方法体不含 .method/.end method 行。"""
    m = re.search(r"^\.method[^\n]*" + re.escape(sig) + r"\n", text, re.M)
    if not m:
        die("method not found: %s" % sig)
    end = text.find("\n.end method", m.end())
    if end < 0:
        die("malformed method (no .end method): %s" % sig)
    return m.start(), m.end(), end, end + len("\n.end method")


def replace_body(text: str, sig: str, new_body: str) -> str:
    s, bs, be, _ = method_block(text, sig)
    return text[:bs] + new_body + text[be:]

# ---------------------------------------------------------------- tap


def patch_tap(root: Path, orig_md5: str) -> None:
    md5 = orig_md5.strip().lower()
    if len(md5) != 32 or any(c not in "0123456789abcdef" for c in md5):
        die("bad orig_md5: %r" % orig_md5)

    # --- 1) TapSignKit.getSign → 固定 MD5
    f = find(root, TAP_SIGN_REL)
    t = read(f)
    if SIGN_MARK in t:
        print("[tap] TapSignKit 已补丁，跳过")
    else:
        sig = "getSign(Landroid/content/Context;)Ljava/lang/String;"
        new = ("    .locals 1\n"
               "\n"
               "    %s\n"
               "    const-string v0, \"%s\"\n"
               "\n"
               "    return-object v0\n" % (SIGN_MARK, md5))
        t = replace_body(t, sig, new)
        n = t.count(md5)
        if n != 1:
            die("TapSignKit patch failed: md5 occurrences=%d" % n)
        write(f, t)
        print("[tap] TapSignKit.getSign → %s  OK" % md5)

    # --- 2) ClientLoginUtils.isClientLogin → false（回落 WebLoginFragment）
    p = root / CLIENT_LOGIN_REL
    if not p.is_file():
        p = root / CLIENT_LOGIN_ALT
    if not p.is_file():
        die("ClientLoginUtils.smali not found (tried %s / %s)"
            % (CLIENT_LOGIN_REL, CLIENT_LOGIN_ALT))
    t = read(p)
    if WEB_MARK in t:
        print("[tap] ClientLoginUtils 已补丁，跳过")
    else:
        sig2 = ("isClientLogin(Landroid/app/Activity;"
                "Lcom/taptap/sdk/login/impl/data/model/Actions;)Z")
        sig1 = "isClientLogin(Landroid/app/Activity;)Z"
        sig = sig2 if sig2 in t else sig1
        new = ("    .locals 1\n"
               "\n"
               "    %s\n"
               "    const/4 v0, 0x0\n"
               "\n"
               "    return v0\n" % WEB_MARK)
        t = replace_body(t, sig, new)
        if WEB_MARK not in t:
            die("ClientLoginUtils patch failed")
        write(p, t)
        print("[tap] ClientLoginUtils.isClientLogin(%s) → false  OK"
              % ("2-arg" if sig == sig2 else "1-arg"))

# ---------------------------------------------------------------- activity


def patch_activity(root: Path) -> None:
    f = find(root, MAR_ACTIVITY_REL)
    t = read(f)
    changed = 0

    # --- onResume：在 invoke-super ...UnityPlayerActivity;->onResume()V 之后注入
    if RESUME_MARK in t:
        print("[activity] onResume 已补丁，跳过")
    else:
        anchor = "invoke-super {p0}, Lcom/unity3d/player/UnityPlayerActivity;->onResume()V\n"
        if t.count(anchor) != 1:
            die("onResume anchor count=%d (want 1)" % t.count(anchor))
        inj = ("    %s\n"
               "    %s\n" % (RESUME_MARK, MENU_RESUMED))
        t = t.replace(anchor, anchor + inj, 1)
        # .registers 3 已足够（p0 = v2），无需扩容
        changed += 1
        print("[activity] onResume → notifyHostResumed  OK")

    # --- onUserLeaveHint：新增
    if LEAVE_MARK in t:
        print("[activity] onUserLeaveHint 已补丁，跳过")
    else:
        new_method = (
            "\n.method public onUserLeaveHint()V\n"
            "    .registers 1\n"
            "\n"
            "    %s\n"
            "    %s\n"
            "\n"
            "    return-void\n"
            ".end method\n" % (LEAVE_MARK, MENU_LEAVING))
        t = t.rstrip("\n") + "\n" + new_method
        changed += 1
        print("[activity] onUserLeaveHint → notifyHostLeaving  OK")

    if changed:
        write(f, t)

# ---------------------------------------------------------------- main


def main() -> None:
    if len(sys.argv) < 3:
        die("usage: %s tap <dec_root> <orig_md5>\n"
            "       %s activity <dec_root>" % (sys.argv[0], sys.argv[0]))
    mode = sys.argv[1]
    root = Path(sys.argv[2])
    if not root.is_dir():
        die("dec root not a directory: %s" % root)

    if mode == "tap":
        if len(sys.argv) < 4:
            die("usage: %s tap <dec_root> <orig_md5>" % sys.argv[0])
        patch_tap(root, sys.argv[3])
    elif mode == "activity":
        patch_activity(root)
    else:
        die("unknown mode: %s" % mode)

    print("[jfqs] %s patch done: %s" % (mode, root))


if __name__ == "__main__":
    main()
