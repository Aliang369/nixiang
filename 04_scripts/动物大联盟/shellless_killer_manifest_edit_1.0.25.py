#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 · 无壳包 Manifest 追加 appComponentFactory（唯一变量）
用法: python3 shellless_killer_manifest_edit_1.0.25.py <AndroidManifest.xml> <package>
只做一件事：<application> 增加 android:appComponentFactory="com.starry.magic.dwdlm.DwdlmKillerFactory"。
其余（application/@android:name、launcher activity、权限、meta-data）一律不动，改完断言。
"""
import sys, re, json

path, pkg = sys.argv[1], sys.argv[2]
FACTORY = "com.starry.magic.dwdlm.DwdlmKillerFactory"
src = open(path, encoding="utf-8").read()

m = re.search(r"<application\b[^>]*>", src)
assert m, "未找到 <application> 标签"
tag = m.group(0)
assert "appComponentFactory" not in src, "已存在 appComponentFactory，拒绝二次添加"

new_tag = tag[:-1].rstrip() + ' android:appComponentFactory="%s">' % FACTORY
out = src[:m.start()] + new_tag + src[m.end():]

# ---- 断言：application/launcher 未变 -------------------------------------
def get(t, attr):
    mm = re.search(attr + r'="([^"]*)"', t)
    return mm.group(1) if mm else None

checks = {
    "package_present": pkg in out,
    "app_name_before": get(tag, r"android:name"),
    "app_name_after": get(new_tag, r"android:name"),
    "factory_after": get(new_tag, r"android:appComponentFactory"),
    "launcher_activity": "org.cocos2dx.javascript.AppActivity" in out,
    "launcher_category": "android.intent.category.LAUNCHER" in out,
    "provider_absent": "StarryInitProvider" not in out,
    "shell_absent": "dev.mars.secure" not in out,
}
assert checks["app_name_before"] == checks["app_name_after"] == "com.bingo.cocos.bridge.BingoApplication",     "Application 名被改动: %r -> %r" % (checks["app_name_before"], checks["app_name_after"])
assert checks["factory_after"] == FACTORY, "工厂属性写入失败"
assert checks["launcher_activity"] and checks["launcher_category"], "launcher 被动过"
assert checks["provider_absent"], "本实验不应出现 StarryInitProvider"
assert checks["shell_absent"], "Manifest 含壳类"

open(path, "w", encoding="utf-8").write(out)
print(json.dumps({"ok": True, "added": {"android:appComponentFactory": FACTORY},
                  "application_name": checks["app_name_after"]}, ensure_ascii=False, indent=2))
