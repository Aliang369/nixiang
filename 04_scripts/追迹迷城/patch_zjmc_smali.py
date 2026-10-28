#!/usr/bin/env python3
# 追迹迷城 smali 结构化补丁：Activity 注入与 TapTap 登录/签名绕过。
import re
import sys
from pathlib import Path


def inject_activity(path: Path) -> None:
    if not path.is_file():
        print(f"  skip {path}")
        return
    text = path.read_text(encoding="utf-8")
    if "MenuService;->start" not in text:
        match = re.search(
            r"(\.method protected onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(\n    return-void\n\.end method)",
            text,
        )
        if not match:
            match = re.search(
                r"(\.method public onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(\n    return-void\n\.end method)",
                text,
            )
        if not match:
            raise SystemExit(f"onCreate not found: {path}")
        head = match.group(1)
        head2, _ = re.subn(
            r"(\.locals )(\d+)",
            lambda m: m.group(1) + str(max(int(m.group(2)), 1)),
            head,
            count=1,
        )
        # 2026-10-28：SignSpoof 已从源码删除 ⇒ 不再注入 install()（注入会引用不存在的类，dex 运行期 NoClassDefFoundError）。
        # 验签/登录改走改包法（TapSDK 门闩恒 true + 登录入口恒 Web）。
        inject = "\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V\n"
        text = text[:match.start(1)] + head2 + inject + text[match.start(2):]
        print(f"  hook {path.name}.onCreate")

    if "MenuService;->notifyHostLeaving" not in text:
        text += """
.method protected onUserLeaveHint()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V
    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V
    return-void
.end method
"""
        print(f"  hook {path.name}.onUserLeaveHint")

    if "MenuService;->notifyHostResumed" not in text:
        if ".method protected onResume()V" not in text and ".method public onResume()V" not in text:
            text += """
.method protected onResume()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onResume()V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V
    return-void
.end method
"""
            print(f"  hook {path.name}.onResume")

    path.write_text(text, encoding="utf-8")


def patch_tap(root: Path, orig_md5: str) -> None:
    taps = list(root.glob("com/taptap/sdk/kit/internal/utils/TapSignKit.smali"))
    if not taps:
        raise SystemExit("TapSignKit.smali not found")
    tap_method = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "{orig_md5}"

    return-object v0
.end method"""
    for tap in taps:
        text = tap.read_text(encoding="utf-8")
        text, count = re.subn(
            r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
            tap_method,
            text,
            count=1,
        )
        if count != 1:
            raise SystemExit(f"TapSignKit patch failed: {tap}")
        tap.write_text(text, encoding="utf-8")
        print("  TapSignKit.getSign -> original cert")

    login = root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
    if not login.is_file():
        login = root / "com/taptap/sdk/login/internal/util/ClientLoginUtils.smali"
    if not login.is_file():
        raise SystemExit("ClientLoginUtils.smali not found")
    text = login.read_text(encoding="utf-8")
    new_method = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
    text, count = re.subn(
        r"\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z[\s\S]*?\.end method",
        new_method,
        text,
        count=1,
    )
    if count != 1:
        old_method = """.method public final isClientLogin(Landroid/app/Activity;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
        text, count = re.subn(
            r"\.method public final isClientLogin\(Landroid/app/Activity;\)Z[\s\S]*?\.end method",
            old_method,
            text,
            count=1,
        )
    if count != 1:
        raise SystemExit("ClientLoginUtils.isClientLogin patch failed")
    login.write_text(text, encoding="utf-8")
    print("  ClientLoginUtils -> force WebLogin")


def main() -> None:
    if len(sys.argv) < 3:
        raise SystemExit("usage: patch_zjmc_smali.py activity <root> | tap <root> <orig_md5>")
    mode = sys.argv[1]
    root = Path(sys.argv[2])
    if mode == "activity":
        privacy = next(root.rglob("PrivacyActivity.smali"), None)
        if privacy is None:
            raise SystemExit("PrivacyActivity.smali not found")
        inject_activity(privacy)
        unity = list(root.rglob("UnityPlayerActivity.smali"))
        if not unity:
            raise SystemExit("UnityPlayerActivity.smali not found")
        for path in unity:
            inject_activity(path)
        return
    if mode == "tap":
        if len(sys.argv) != 4:
            raise SystemExit("tap mode requires original cert MD5")
        patch_tap(root, sys.argv[3])
        return
    raise SystemExit(f"unknown mode: {mode}")


if __name__ == "__main__":
    main()
