#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
疾风骑士 (Jfqs) 60.0.1 — 二进制 AndroidManifest.xml 文本层补丁

用法：
    python3.11 patch_jfqs_manifest.py <apktool_decoded_dir>

作用（对 apktool d -s 解出的文本 AndroidManifest.xml 做三处只增/最小改）：
  1. 补齐 3 条权限（官方无）：SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE
  2. application@name : com.mar.sdk.MARApplication → com.starry.magic.StarryApp
     （route (c)：StarryApp extends MARApplication，见 surgical 脚本 [3/6] 的 .super 改写）
  3. 注入 <service com.starry.magic.menu.MenuService>（specialUse 前台服务 + FGS subtype property）

之后由调用方 `apktool b` 重打包并抽取二进制 AndroidManifest.xml 缓存。
"""
import re
import sys
from pathlib import Path

PKG = "com.emay.wxsy.topon"
APP_OLD = "com.mar.sdk.MARApplication"
APP_NEW = "com.starry.magic.StarryApp"

PERMS = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]

MENU_SERVICE = """        <service android:name="com.starry.magic.menu.MenuService" android:enabled="true" android:exported="false" android:foregroundServiceType="specialUse" android:stopWithTask="false">
            <property android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE" android:value="MOD menu overlay" />
        </service>
"""


def die(msg):
    raise SystemExit("[jfqs-manifest] ERROR: " + msg)


def main():
    if len(sys.argv) != 2:
        die("usage: patch_jfqs_manifest.py <apktool_decoded_dir>")
    dec = Path(sys.argv[1])
    mf = dec / "AndroidManifest.xml"
    if not mf.is_file():
        die("AndroidManifest.xml not found in %s" % dec)

    t = mf.read_text(encoding="utf-8", errors="surrogateescape")

    # --- 0. 幂等：已打过就跳过 -------------------------------------------------
    if "com.starry.magic.menu.MenuService" in t:
        print("[jfqs-manifest] already patched, skip")
        return

    # --- 1. 权限 --------------------------------------------------------------
    add = []
    for p in PERMS:
        if ('android:name="%s"' % p) in t:
            continue
        add.append('    <uses-permission android:name="%s"/>' % p)
    if add:
        m = re.search(r'^[ \t]*<uses-permission\b', t, re.M)
        if not m:
            die("no <uses-permission> anchor found")
        t = t[:m.start()] + "\n".join(add) + "\n" + t[m.start():]
        print("[jfqs-manifest] +permissions %d" % len(add))
    else:
        print("[jfqs-manifest] permissions already present")

    # --- 2. application@name --------------------------------------------------
    if ('android:name="%s"' % APP_NEW) in t:
        print("[jfqs-manifest] application already StarryApp")
    else:
        n = t.count('android:name="%s"' % APP_OLD)
        if n != 1:
            die('application@name="%s" count=%d (want 1)' % (APP_OLD, n))
        t = t.replace('android:name="%s"' % APP_OLD, 'android:name="%s"' % APP_NEW, 1)
        print("[jfqs-manifest] application %s -> %s" % (APP_OLD, APP_NEW))

    # --- 3. MenuService -------------------------------------------------------
    n = t.count("</application>")
    if n != 1:
        die("</application> count=%d (want 1)" % n)
    t = t.replace("</application>", MENU_SERVICE + "    </application>", 1)
    print("[jfqs-manifest] +MenuService")

    mf.write_text(t, encoding="utf-8", errors="surrogateescape")

    # --- 自检 -----------------------------------------------------------------
    chk = mf.read_text(encoding="utf-8", errors="surrogateescape")
    for p in PERMS:
        if ('android:name="%s"' % p) not in chk:
            die("selfcheck: permission %s missing" % p)
    if ('android:name="%s"' % APP_NEW) not in chk:
        die("selfcheck: application not StarryApp")
    if "com.starry.magic.menu.MenuService" not in chk:
        die("selfcheck: MenuService missing")
    if "foregroundServiceType=\"specialUse\"" not in chk:
        die("selfcheck: specialUse missing")
    if APP_OLD in chk:
        die("selfcheck: residual %s" % APP_OLD)
    print("[jfqs-manifest] OK -> %s" % mf)


if __name__ == "__main__":
    main()
