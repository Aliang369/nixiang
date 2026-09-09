#!/usr/bin/env python3.11
"""雷霆战机 inject 用：Manifest / CloverApplication / AppActivity / TopOn 旁路。"""
from __future__ import annotations

import re
import sys
from pathlib import Path


def main() -> None:
    if len(sys.argv) < 3:
        raise SystemExit("usage: _patch_ltzj_smali.py <PATCH_DIR> <ORIG_MD5>")
    patch = Path(sys.argv[1])
    orig_md5 = sys.argv[2]

    manifest = patch / "AndroidManifest.xml"
    t = manifest.read_text(encoding="utf-8")
    if "com.cocos.game.CloverApplication" not in t:
        print("WARN: CloverApplication missing in manifest")
    if "com.starry.magic.StarryApplication" in t:
        raise SystemExit("ERROR: 勿替换 Application 为 StarryApplication")
    perms = []
    if "SYSTEM_ALERT_WINDOW" not in t:
        perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
    if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
        perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
    if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
        perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
    if perms:
        i = t.find("<uses-permission")
        t = t[:i] + "\n".join(perms) + "\n" + t[i:]
        print("manifest: perms")
    if "com.starry.magic.menu.MenuService" not in t:
        svc = """
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
"""
        t = t.replace("</application>", svc + "\n    </application>", 1)
        print("manifest: MenuService")
    manifest.write_text(t, encoding="utf-8")

    # ACE：干净机 resign-only 可活；Android 16 上 SignSpoof 多项 hiddenapi denied。
    # STARRY_SKIP_SIGNSPOOF=1 时不插 SignSpoof（对照探针）。默认仍安装。
    # 勿在 Application 里 loadLibrary（tersafe 抢跑易黑屏），so 只在 AppActivity.onCreate 加载。
    import os

    skip_signspoof = os.environ.get("STARRY_SKIP_SIGNSPOOF", "0") == "1"
    app_smali = next(patch.glob("smali*/com/cocos/game/CloverApplication.smali"), None)
    if app_smali is None:
        raise SystemExit("CloverApplication.smali missing")
    s = app_smali.read_text(encoding="utf-8")
    ace_super = (
        "    invoke-super {p0, p1}, "
        "Lcom/ace/gshell/AceApplication;->attachBaseContext(Landroid/content/Context;)V\n"
    )
    # 清理旧版：Ace super 之后的 SignSpoof / loadLibrary；以及可选整段 SignSpoof
    bad = (
        ace_super
        + "\n    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n\n"
        + '    const-string v0, "starry_magic"\n\n'
        + "    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n"
    )
    if bad in s:
        s = s.replace(bad, ace_super, 1)
    bad2 = ace_super + "\n    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n"
    if bad2 in s:
        s = s.replace(bad2, ace_super, 1)
    spoof_before = "    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n\n" + ace_super
    if skip_signspoof:
        if spoof_before in s:
            s = s.replace(spoof_before, ace_super, 1)
        print("sign/hook: SKIP SignSpoof in CloverApplication (STARRY_SKIP_SIGNSPOOF=1)")
    else:
        blk_key = ".method protected attachBaseContext"
        if blk_key not in s:
            raise SystemExit("attachBaseContext missing")
        blk = s.split(blk_key)[1].split(".end method")[0]
        if "SignSpoof;->install" in blk:
            if blk.find("SignSpoof;->install") < blk.find("AceApplication;->attachBaseContext"):
                print("sign/hook: CloverApplication SignSpoof-before-ACE ok")
            else:
                raise SystemExit("SignSpoof still after Ace super")
        else:
            if ace_super not in s:
                raise SystemExit("Ace super not found")
            s = s.replace(ace_super, spoof_before, 1)
            s2, n = re.subn(
                r"(\.method protected attachBaseContext\(Landroid/content/Context;\)V\n    )\.locals [01]",
                r"\1.locals 1",
                s,
                count=1,
            )
            if n == 1:
                s = s2
            print("sign/hook: CloverApplication ← SignSpoof BEFORE AceApplication")
    # onCreate：保留 TP2Sdk（干净机仅重签可玩；跳过 initEx 无必要）
    if "starry: skip TP2Sdk" in s:
        print("WARN: CloverApplication still has starry TP2Sdk skip — re-decode from official")
    else:
        print("ace: CloverApplication.onCreate keep TP2Sdk (no skip)")
    app_smali.write_text(s, encoding="utf-8")

    # AceApplication：可选 clinit SignSpoof；**保留** attachBaseContextShell→initialize
    # （nop shell 会在 loadLibrary(cocos) 里 libtprt sem_wait 死锁黑屏，干净机已复现）
    ace = next(patch.glob("smali*/com/ace/gshell/AceApplication.smali"), None)
    if ace is None:
        print("WARN: AceApplication.smali missing")
    else:
        a = ace.read_text(encoding="utf-8")
        clinit = a.split(".method static constructor <clinit>")[1].split(".end method")[0]
        spoof_tersafe = (
            "    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n\n"
            '    const-string v0, "tersafe2"\n\n'
            "    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V"
        )
        plain_tersafe = (
            '    const-string v0, "tersafe2"\n\n'
            "    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V"
        )
        if skip_signspoof:
            if spoof_tersafe in a:
                a = a.replace(spoof_tersafe, plain_tersafe, 1)
            print("ace: SKIP SignSpoof in AceApplication.<clinit>")
        elif "SignSpoof;->install" not in clinit:
            a = a.replace(plain_tersafe, spoof_tersafe, 1)
            print("ace: AceApplication.<clinit> ← SignSpoof before tersafe2")
        else:
            print("ace: AceApplication.<clinit> SignSpoof ok")
        if "starry: skip ACE gshell initialize" in a:
            raise SystemExit(
                "AceApplication.attachBaseContextShell was nop'd in source decode — "
                "delete 03_decompiled/.../apktool_1.25.31_orig and re-decode from official APK"
            )
        print("ace: keep AceApplication.attachBaseContextShell (initialize)")
        ace.write_text(a, encoding="utf-8")

    # 隐私页：重签后 PrivacyDialog 常黑屏卡住 → 直接进下一页
    priv = next(patch.glob("smali*/com/xxx/sdk/GamePrivacyActivity.smali"), None)
    if priv is None:
        print("WARN: GamePrivacyActivity.smali missing")
    else:
        p = priv.read_text(encoding="utf-8")
        if "starry skip privacy dialog" in p:
            print("privacy: already skipped")
        else:
            old = (
                "    new-instance v0, Lcom/xxx/sdk/GamePrivacyActivity$2;\n\n"
                "    invoke-direct {v0, p0}, Lcom/xxx/sdk/GamePrivacyActivity$2;-><init>(Lcom/xxx/sdk/GamePrivacyActivity;)V\n\n"
                "    invoke-static {p0, v2, v1, p1, v0}, "
                "Lcom/xxx/sdk/privacy/PrivacyDialog;->showDialog("
                "Landroid/app/Activity;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;"
                "Lcom/xxx/sdk/privacy/IProtocolListener;)V\n\n"
                "    return-void\n.end method"
            )
            new = (
                "    # starry: skip privacy dialog (black screen after resign)\n"
                '    const-string v0, "XSDK"\n\n'
                '    const-string v1, "starry skip privacy dialog"\n\n'
                "    invoke-static {v0, v1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I\n\n"
                '    const-string v0, "ux_privacy_dialog_showed"\n\n'
                "    const/4 v1, 0x1\n\n"
                "    invoke-static {p0, v0, v1}, "
                "Lcom/game/sdk/common/utils/StoreUtils;->putBoolean("
                "Landroid/content/Context;Ljava/lang/String;Z)V\n\n"
                "    invoke-direct {p0}, Lcom/xxx/sdk/GamePrivacyActivity;->startNextActivity()V\n\n"
                "    return-void\n.end method"
            )
            if old not in p:
                raise SystemExit("GamePrivacyActivity showDialog pattern missing")
            priv.write_text(p.replace(old, new, 1), encoding="utf-8")
            print("privacy: GamePrivacyActivity → skip dialog + startNextActivity")

    act = next(patch.glob("smali*/com/cocos/game/AppActivity.smali"), None)
    if act is None:
        raise SystemExit("AppActivity.smali missing")
    s = act.read_text(encoding="utf-8")

    super_on = "    invoke-super {p0, p1}, Lcom/cocos/lib/CocosActivity;->onCreate(Landroid/os/Bundle;)V\n"
    early_load = """    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-super {p0, p1}, Lcom/cocos/lib/CocosActivity;->onCreate(Landroid/os/Bundle;)V
"""
    if "MenuService;->start" not in s:
        if super_on not in s:
            raise SystemExit("AppActivity.onCreate super not found")
        oncreate = s.split(".method protected onCreate")[1].split(".end method")[0]
        if "starry_magic" not in oncreate:
            s = s.replace(super_on, early_load, 1)
        marker = """    invoke-virtual {p1, v0}, Landroid/view/Window;->addFlags(I)V

    return-void
.end method"""
        inject = """    invoke-virtual {p1, v0}, Landroid/view/Window;->addFlags(I)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V

    return-void
.end method"""
        if marker not in s:
            raise SystemExit("AppActivity.onCreate tail not found")
        s = s.replace(marker, inject, 1)
        print("hook: AppActivity.onCreate → early loadLibrary + MenuService")
    else:
        print("hook: AppActivity.onCreate already patched")

    if "MenuService;->notifyHostResumed" not in s:
        needle = "    invoke-virtual {v0, p0}, Lcom/game/sdk/XPlatform;->onResume(Landroid/app/Activity;)V\n"
        if needle not in s:
            raise SystemExit("AppActivity.onResume XPlatform not found")
        s = s.replace(
            needle,
            needle + "\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n",
            1,
        )
        print("hook: AppActivity.onResume → notifyHostResumed")
    else:
        print("hook: onResume already patched")

    if "MenuService;->notifyHostLeaving" not in s:
        needle = "    invoke-virtual {v0, p0}, Lcom/game/sdk/XPlatform;->onPause(Landroid/app/Activity;)V\n"
        if needle not in s:
            raise SystemExit("AppActivity.onPause XPlatform not found")
        s = s.replace(
            needle,
            needle + "\n    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n",
            1,
        )
        print("hook: AppActivity.onPause → notifyHostLeaving")
    else:
        print("hook: onPause already patched")

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
            raise SystemExit("onPause marker missing for onUserLeaveHint")
        s = s.replace(marker, leave + marker, 1)
        print("hook: AppActivity.onUserLeaveHint")

    if "LtzjHooks;->getRet" not in s:
        s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/LtzjHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/LtzjHooks;->setRet(IZ)V

    return-void
.end method
"""
        print("hook: AppActivity.get_ret/set_ret → LtzjHooks")
    else:
        print("hook: get_ret already present")

    act.write_text(s, encoding="utf-8")

    bridge = next(patch.glob("smali*/com/anythink/cocosjs/ATRewardedVideoJSBridge.smali"), None)
    if bridge is None:
        print("WARN: ATRewardedVideoJSBridge missing — 免广告 smali 跳过")
    else:
        bs = bridge.read_text(encoding="utf-8")
        if "LtzjHooks;->shouldSkipAd" in bs:
            print("ad: ATRewardedVideoJSBridge already patched")
        else:
            new_show = """.method public static show(Ljava/lang/String;Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/starry/magic/cocos/LtzjHooks;->shouldSkipAd()Z

    move-result v0

    if-eqz v0, :cond_starry_ad

    invoke-static {p0}, Lcom/starry/magic/cocos/LtzjHooks;->fakeRewardShow(Ljava/lang/String;)V

    return-void

    :cond_starry_ad
    invoke-static {p0}, Lcom/anythink/cocosjs/ATRewardedVideoJSBridge;->getHelper(Ljava/lang/String;)Lcom/anythink/cocosjs/rewardvideo/RewardVideoHelper;

    move-result-object p0

    if-eqz p0, :cond_0

    invoke-virtual {p0, p1}, Lcom/anythink/cocosjs/rewardvideo/RewardVideoHelper;->showVideo(Ljava/lang/String;)V

    :cond_0
    return-void
.end method"""
            bs2, n = re.subn(
                r"\.method public static show\(Ljava/lang/String;Ljava/lang/String;\)V[\s\S]*?\.end method",
                new_show,
                bs,
                count=1,
            )
            if n != 1:
                raise SystemExit("ATRewardedVideoJSBridge.show patch failed")
            new_ready = """.method public static isAdReady(Ljava/lang/String;)Z
    .locals 1

    invoke-static {}, Lcom/starry/magic/cocos/LtzjHooks;->shouldSkipAd()Z

    move-result v0

    if-eqz v0, :cond_starry_ready

    const/4 p0, 0x1

    return p0

    :cond_starry_ready
    invoke-static {p0}, Lcom/anythink/cocosjs/ATRewardedVideoJSBridge;->getHelper(Ljava/lang/String;)Lcom/anythink/cocosjs/rewardvideo/RewardVideoHelper;

    move-result-object p0

    if-eqz p0, :cond_0

    invoke-virtual {p0}, Lcom/anythink/cocosjs/rewardvideo/RewardVideoHelper;->isAdReady()Z

    move-result p0

    return p0

    :cond_0
    const/4 p0, 0x0

    return p0
.end method"""
            bs2, n2 = re.subn(
                r"\.method public static isAdReady\(Ljava/lang/String;\)Z[\s\S]*?\.end method",
                new_ready,
                bs2,
                count=1,
            )
            if n2 != 1:
                raise SystemExit("ATRewardedVideoJSBridge.isAdReady patch failed")
            bridge.write_text(bs2, encoding="utf-8")
            print("ad: ATRewardedVideoJSBridge show/isAdReady → LtzjHooks")

    print(f"sign: ORIG_MD5={orig_md5} (SignSpoof)")
    print("hooks done")


if __name__ == "__main__":
    main()
