#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 无壳包 · 菜单注入点（ContentProvider）文本改造

在 shellless_manifest_edit_1.0.25.py 之后跑，就地改写同一个 dec/AndroidManifest.xml。
只做三件事（照仓内多游戏先例：伊莫 / 无限升级 / 失落城堡2 / 挂机打怪兽）：
  1) 补 3 个 uses-permission（SYSTEM_ALERT_WINDOW 悬浮窗、FOREGROUND_SERVICE、
     FOREGROUND_SERVICE_SPECIAL_USE）；MenuService 的 overlay 与前台通知都要它们；
  2) 追加 provider com.starry.magic.StarryInitProvider（authorities=<pkg>.starryinit,
     exported=false, initOrder=2147483647）—— 唯一启动注入点（ContentProvider.onCreate
     早于 Activity.onCreate）；
  3) 追加 service com.starry.magic.menu.MenuService（MenuService.start 走 startService，
     Manifest 不声明则 ServiceNotFoundException -> 菜单永远出不来）。

**不加 appComponentFactory**：无壳包的 Application 是 com.bingo.cocos.bridge.BingoApplication，
挂工厂会断启动链（2026-11-05 对照实测）。

用法: python3 shellless_menu_manifest_edit_1.0.25.py <dec/AndroidManifest.xml> [pkg]
"""
import re, sys, json

p = sys.argv[1]
PKG = sys.argv[2] if len(sys.argv) > 2 else "com.fightingstars.games.animal"
APP = "com.bingo.cocos.bridge.BingoApplication"
ACT = "org.cocos2dx.javascript.AppActivity"
PROVIDER = "com.starry.magic.StarryInitProvider"
SERVICE = "com.starry.magic.menu.MenuService"
AUTH = PKG + ".starryinit"
PERMS = ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE")

t = open(p, encoding="utf-8").read()
log = []

# ---- 前置：无壳改造必须已完成 ----
assert APP in t, "Manifest 里没有 %s（先跑 shellless_manifest_edit_1.0.25.py）" % APP
assert ACT in t, "Manifest 里没有 %s" % ACT
assert "dev.mars.secure" not in t, "Manifest 仍含 dev.mars.secure（无壳改造没做完）"
assert PROVIDER not in t, "Manifest 已含 %s（重复注入）" % PROVIDER
assert "appComponentFactory" not in t, "底包已含 appComponentFactory（本方案不挂，先核对口径）"
assert "SignSpoof" not in t, "Manifest 含 SignSpoof（2026-10-28 已删，禁止复活）"

# ---- 1) 补权限（缺哪个补哪个）----
addp = []
for perm in PERMS:
    if "android.permission." + perm not in t:
        addp.append('    <uses-permission android:name="android.permission.%s"/>' % perm)
if addp:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    assert i > 0, "找不到 uses-permission/application 插入点"
    t = t[:i] + chr(10).join(addp) + chr(10) + t[i:]
log.append("补权限 +%d: %s" % (len(addp), [x.split('"')[1].split(".")[-1] for x in addp]))

# ---- 2) + 3) provider / service ----
prov = chr(10) + '''        <provider
            android:name="%s"
            android:authorities="%s"
            android:exported="false"
            android:initOrder="2147483647" />
''' % (PROVIDER, AUTH)
svc = '''        <service
            android:name="%s"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
''' % SERVICE
assert t.count("</application>") == 1, "application 闭合标签数 != 1"
t = t.replace("</application>", prov + svc + chr(10) + "    </application>", 1)
log.append("加 provider %s（authority=%s, initOrder=2147483647）" % (PROVIDER, AUTH))
log.append("加 service %s（specialUse FGS）" % SERVICE)

# ---- 硬断言 ----
app_i = t.find("<application")
assert app_i > 0, "Manifest 无 <application 标签"
app_tag = t[app_i:t.find(">", app_i) + 1]
assert 'android:name="%s"' % APP in app_tag, "application@name 不是 %s" % APP
assert "appComponentFactory" not in t, "Manifest 出现 appComponentFactory（会断无壳包启动链）"
assert t.count(PROVIDER) == 1, "%s 出现次数 != 1" % PROVIDER
pi = t.find('<provider', t.find('android:name="%s"' % PROVIDER) - 400)
pt = t[t.rfind("<provider", 0, t.find('android:name="%s"' % PROVIDER)):]
pt = pt[:pt.find("/>") + 2]
assert 'android:authorities="%s"' % AUTH in pt, "provider authority 不是 %s" % AUTH
assert 'android:initOrder="2147483647"' in pt, "provider initOrder 不是 2147483647"
assert 'android:exported="false"' in pt, "provider 未 exported=false"
auths = re.findall('android:authorities="([^"]*)"', t)
assert len(auths) == len(set(auths)), "authorities 有重复（%d 条）" % len(auths)
assert SERVICE in t and "<service" in t, "MenuService 未声明"
assert ACT in t, "AppActivity 不在 Manifest"

open(p, "w", encoding="utf-8").write(t)
print(json.dumps({"ok": True, "file": p, "pkg": PKG, "authorities_total": len(auths), "steps": log},
                 ensure_ascii=False, indent=2))
