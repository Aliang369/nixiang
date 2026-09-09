#!/usr/bin/env python3.11
"""雷霆战机斌哥底包：保留 Killer+libsdk，注入星光 Menu / LtzjHooks / 免广告。"""
from __future__ import annotations

import re
import sys
from pathlib import Path


def main() -> None:
    if len(sys.argv) < 2:
        raise SystemExit("usage: _patch_ltzj_binge_smali.py <PATCH_DIR>")
    patch = Path(sys.argv[1])

    # Manifest：悬浮窗权限 + MenuService（不改 Application，保留 Killer 链）
    manifest = patch / "AndroidManifest.xml"
    t = manifest.read_text(encoding="utf-8")
    if "com.cocos.game.CloverApplication" not in t:
        print("WARN: CloverApplication missing")
    ace = next(patch.glob("smali*/com/ace/gshell/AceApplication.smali"), None)
    if ace is None or "KillerApplication193" not in ace.read_text(encoding="utf-8"):
        raise SystemExit("ERROR: binge Ace must extend KillerApplication193")
    print("ace: Killer superclass ok")
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

    act = next(patch.glob("smali*/com/cocos/game/AppActivity.smali"), None)
    if act is None:
        raise SystemExit("AppActivity.smali missing")
    s = act.read_text(encoding="utf-8")

    # onCreate：跳过斌哥 Main 菜单；Cocos 起来后再 load starry + MenuService
    if "starry_magic" not in s.split(".method protected onCreate")[1].split(".end method")[0]:
        binge_main = (
            "    invoke-static {p0}, Lcom/android/support/Main;->StartWithoutPermission"
            "(Landroid/content/Context;)V\n\n"
        )
        if binge_main in s:
            s = s.replace(
                binge_main,
                "    # starry: skip binge Main (NativeUI/Dobby 与星光冲突)\n\n",
                1,
            )
            print("hook: skip binge Main.StartWithoutPermission")
        needle = (
            "    invoke-super {p0, p1}, Lcom/cocos/lib/CocosActivity;->onCreate(Landroid/os/Bundle;)V\n"
        )
        if needle not in s:
            raise SystemExit("AppActivity.onCreate Cocos super missing")
        s = s.replace(
            needle,
            needle
            + '\n    const-string v0, "starry_magic"\n\n'
            + "    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n\n"
            + "    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start"
            "(Landroid/content/Context;)V\n",
            1,
        )
        s2, n = re.subn(
            r"(\.method protected onCreate\(Landroid/os/Bundle;\)V\n    )\.locals (\d+)",
            lambda m: m.group(1) + ".locals " + str(max(int(m.group(2)), 2)),
            s,
            count=1,
        )
        if n == 1:
            s = s2
        print("hook: onCreate ← loadLibrary starry_magic + MenuService after Cocos")
    else:
        print("hook: onCreate starry already")

    # 隐私页跳过（否则卡在协议弹窗）
    priv = next(patch.glob("smali*/com/xxx/sdk/GamePrivacyActivity.smali"), None)
    if priv is None:
        print("WARN: GamePrivacyActivity missing")
    else:
        p = priv.read_text(encoding="utf-8")
        if "starry skip privacy dialog" in p:
            print("privacy: already skipped")
        else:
            old = (
                "    new-instance v0, Lcom/xxx/sdk/GamePrivacyActivity$2;\n\n"
                "    invoke-direct {v0, p0}, Lcom/xxx/sdk/GamePrivacyActivity$2;-><init>"
                "(Lcom/xxx/sdk/GamePrivacyActivity;)V\n\n"
                "    invoke-static {p0, v2, v1, p1, v0}, "
                "Lcom/xxx/sdk/privacy/PrivacyDialog;->showDialog("
                "Landroid/app/Activity;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;"
                "Lcom/xxx/sdk/privacy/IProtocolListener;)V\n\n"
                "    return-void\n.end method"
            )
            new = (
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
                print("WARN: privacy showDialog pattern missing")
            else:
                priv.write_text(p.replace(old, new, 1), encoding="utf-8")
                print("privacy: skip dialog")

    if "MenuService;->notifyHostResumed" not in s:
        # 尽量挂 onResume 尾
        if ".method protected onResume()V" in s:
            s2, n = re.subn(
                r"(\.method protected onResume\(\)V\n[\s\S]*?)(\n    return-void\n.end method)",
                r"\1\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;"
                r"->notifyHostResumed(Landroid/app/Activity;)V\n\2",
                s,
                count=1,
            )
            if n == 1:
                s = s2
                print("hook: onResume → notifyHostResumed")
    if "MenuService;->notifyHostLeaving" not in s and ".method protected onPause()V" in s:
        s2, n = re.subn(
            r"(\.method protected onPause\(\)V\n[\s\S]*?)(\n    return-void\n.end method)",
            r"\1\n    invoke-static {}, Lcom/starry/magic/menu/MenuService;"
            r"->notifyHostLeaving()V\n\2",
            s,
            count=1,
        )
        if n == 1:
            s = s2
            print("hook: onPause → notifyHostLeaving")

    # get_ret 1..3 → LtzjHooks（覆盖斌哥 prefs）
    if "LtzjHooks;->getRet" not in s:
        new_get = """.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/LtzjHooks;->getRet(I)Z

    move-result v0

    return v0
.end method
"""
        s2, n = re.subn(
            r"\.method public static get_ret\(I\)Z[\s\S]*?\.end method",
            new_get,
            s,
            count=1,
        )
        if n != 1:
            raise SystemExit("get_ret replace failed")
        s = s2
        print("hook: get_ret → LtzjHooks")
    else:
        print("hook: get_ret already LtzjHooks")

    if "LtzjHooks;->setRet" not in s:
        s += """
.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/LtzjHooks;->setRet(IZ)V

    return-void
.end method
"""
        print("hook: set_ret → LtzjHooks")

    act.write_text(s, encoding="utf-8")

    # TopOn 免广告（若存在）
    bridge = next(patch.glob("smali*/com/anythink/cocosjs/ATRewardedVideoJSBridge.smali"), None)
    if bridge is None:
        print("WARN: ATRewardedVideoJSBridge missing — 免广告跳过")
    elif "LtzjHooks;->shouldSkipAd" in bridge.read_text(encoding="utf-8"):
        print("ad: already patched")
    else:
        bs = bridge.read_text(encoding="utf-8")
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
            print("WARN: TopOn show patch failed")
        else:
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
                print("WARN: TopOn isAdReady patch failed")
            else:
                bridge.write_text(bs2, encoding="utf-8")
                print("ad: TopOn → LtzjHooks")

    print("binge+starry hooks done")


if __name__ == "__main__":
    main()
