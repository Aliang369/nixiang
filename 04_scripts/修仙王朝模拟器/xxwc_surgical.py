#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""修仙王朝模拟器 (com.sky.xxwc.xxt 1.0.4 / vc5) — 星光外科组装（mac）

官方 dex/arsc/so/assets 全部逐字节不变，只做三件事：
  1. AndroidManifest.xml：新增 StarryInitProvider / MenuService / 两条前台服务权限
  2. 新增 classes7.dex（星光 smali：菜单/Auth/CocosJsBridge）
  3. 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
本包功能全部走运行期 JS 注入（Games/XxwcPatch.js），**不改任何 assets**。

用法：python3 04_scripts/修仙王朝模拟器/xxwc_surgical.py [--skip-sign]
产物：08_output/修仙王朝模拟器/修仙王朝模拟器_1.0.4_xxwc-releaseSigned.apk
"""
import hashlib, os, shutil, subprocess, sys, zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "修仙王朝模拟器"
PKG = "com.sky.xxwc.xxt"
VER = "1.0.4"
ORIG = ROOT / "02_apks" / ("%s_官方_%s.apk" % (GAME, PKG))
ORIG_MD5 = "fb6c72eaf25804a4fde0c14faa0ad4f2"
STARRY_APK = ROOT / "mod菜单" / "starry-mod-menu" / "app" / "build" / "outputs" / "apk" / "release" / "app-release.apk"
SO_DIR = ROOT / "mod菜单" / "starry-mod-menu" / "libs_xxwc" / "arm64-v8a"
OUT = ROOT / "08_output" / GAME
WORK = ROOT / "13_scratch" / "xxwc_surgical"
SMALI = ROOT / "01_tools" / "smali" / "smali"
APKTOOL = ROOT / "01_tools" / "apktool" / "apktool.jar"
KS = ROOT / "01_tools" / "starry_release.jks"
BT = Path.home() / "Library" / "Android" / "sdk" / "build-tools" / "36.0.0"
DEX_TP = ROOT / "13_scratch" / "xxwc_taptap" / "classes.patched.dex"  # TapTap 改包法产物
NEW_DEX = "classes7.dex"


def run(cmd):
    print("  $", " ".join(str(c) for c in cmd[:6]))
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    if r.returncode != 0:
        print(r.stdout[-3000:]); print(r.stderr[-3000:])
        raise SystemExit("FAILED: %s" % cmd[0])
    return r.stdout


def main():
    if not ORIG.exists():
        raise SystemExit("missing orig apk")
    md5 = hashlib.md5(ORIG.read_bytes()).hexdigest()
    if md5 != ORIG_MD5:
        raise SystemExit("orig md5 mismatch: %s（版本变了，先重抓锚点）" % md5)
    if not STARRY_APK.exists():
        raise SystemExit("missing starry apk (先 gradlew assembleRelease)")
    so = SO_DIR / "libstarry_magic.so"
    if not so.exists():
        raise SystemExit("missing libstarry_magic.so")
    if b"xxwc" not in so.read_bytes():
        raise SystemExit("so 缺 xxwc 标签")

    if WORK.exists():
        shutil.rmtree(WORK)
    WORK.mkdir(parents=True)
    OUT.mkdir(parents=True, exist_ok=True)
    unsigned = OUT / ("%s_%s_xxwc-unsigned.apk" % (GAME, VER))
    signed = OUT / ("%s_%s_xxwc-releaseSigned.apk" % (GAME, VER))

    print("[1/5] starry apk -> smali ->", NEW_DEX)
    dec_starry = WORK / "starry_dec"
    run(["java", "-jar", APKTOOL, "d", "-f", "-o", dec_starry, STARRY_APK])
    smali_root = WORK / "smali_in"
    (smali_root / "com").mkdir(parents=True, exist_ok=True)
    n = 0
    for d in list(dec_starry.glob("smali*")):
        src = d / "com" / "starry"
        if src.is_dir():
            shutil.copytree(src, smali_root / "com" / "starry", dirs_exist_ok=True)
            n += 1
    if not (smali_root / "com" / "starry" / "magic" / "StarryInitProvider.smali").exists():
        raise SystemExit("StarryInitProvider.smali missing")
    if not (smali_root / "com" / "starry" / "magic" / "cocos" / "CocosJsBridge.smali").exists():
        raise SystemExit("CocosJsBridge.smali missing（框架 JS 桥没编进 starry apk）")
    print("   starry smali dirs:", n)
    run([SMALI, "assemble", "-o", WORK / NEW_DEX, smali_root])

    print("[2/5] AndroidManifest patch")
    dec_orig = WORK / "orig_dec"
    run(["java", "-jar", APKTOOL, "d", "-f", "-s", "--no-assets", "-o", dec_orig, ORIG])
    mf = dec_orig / "AndroidManifest.xml"
    t = mf.read_text(encoding="utf-8")
    if "com.starry.magic.StarryInitProvider" not in t:
        perms = []
        for perm in ("FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
            if perm not in t:
                perms.append('    <uses-permission android:name="android.permission.%s"/>' % perm)
        if perms:
            i = t.find("<uses-permission")
            t = t[:i] + "\n".join(perms) + "\n" + t[i:]
        prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % PKG
        t = t.replace("</application>", prov + "\n    </application>", 1)
    if "com.starry.magic.menu.MenuService" not in t:
        svc = '''
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
'''
        t = t.replace("</application>", svc + "\n    </application>", 1)
    mf.write_text(t, encoding="utf-8")
    run(["java", "-jar", APKTOOL, "b", "-f", dec_orig, "-o", WORK / "mf_only.apk"])
    with zipfile.ZipFile(WORK / "mf_only.apk") as z:
        new_mf = z.read("AndroidManifest.xml")

    print("[3/5] zip merge ->", unsigned.name)
    extras = {}
    if DEX_TP.exists():
        extras["classes.dex"] = DEX_TP.read_bytes()
        print("   [TapTap 改包法] classes.dex = Web 登录 + Gatekeeper 放行版")
    else:
        print("   [警告] 无 TapTap 补丁 dex，classes.dex 保持官方原样")
    for f in SO_DIR.iterdir():
        if f.is_file() and f.suffix == ".so":
            extras["lib/arm64-v8a/" + f.name] = f.read_bytes()

    def is_sig(name):
        if name == "META-INF/MANIFEST.MF":
            return True
        if not name.startswith("META-INF/"):
            return False
        return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

    zin = zipfile.ZipFile(ORIG)
    names = zin.namelist()
    if NEW_DEX in names:
        raise SystemExit("orig 已含 %s" % NEW_DEX)
    zout = zipfile.ZipFile(unsigned, "w")
    for item in zin.infolist():
        name = item.filename
        if is_sig(name) or name in extras:
            continue
        data = new_mf if name == "AndroidManifest.xml" else zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.external_attr = item.external_attr
        info.create_system = 3
        if name == "resources.arsc":
            info.compress_type = zipfile.ZIP_STORED
        elif name == "AndroidManifest.xml":
            info.compress_type = zipfile.ZIP_DEFLATED
        else:
            info.compress_type = item.compress_type
        zout.writestr(info, data)
    for name, data in extras.items():
        info = zipfile.ZipInfo(name, (2026, 11, 3, 0, 0, 0))
        info.compress_type = zipfile.ZIP_DEFLATED
        info.external_attr = 0o644 << 16
        info.create_system = 3
        zout.writestr(info, data)
    info = zipfile.ZipInfo(NEW_DEX, (2026, 11, 3, 0, 0, 0))
    info.compress_type = zipfile.ZIP_DEFLATED
    info.external_attr = 0o644 << 16
    info.create_system = 3
    zout.writestr(info, (WORK / NEW_DEX).read_bytes())
    zout.close()
    zin.close()

    print("[4/5] assert untouched entries")
    zo = zipfile.ZipFile(ORIG)
    zn = zipfile.ZipFile(unsigned)
    must_same = ["classes2.dex", "classes3.dex", "classes4.dex",
                 "classes5.dex", "classes6.dex", "resources.arsc",
                 "lib/arm64-v8a/libcocos.so",
                 "assets/assets/script/index.jsc"]
    for m in must_same:
        if hashlib.sha256(zo.read(m)).digest() != hashlib.sha256(zn.read(m)).digest():
            raise SystemExit("官方条目被改动: %s" % m)
    if b"CocosJsBridge" not in zn.read(NEW_DEX):
        raise SystemExit("%s 缺 CocosJsBridge（JS 桥未打包）" % NEW_DEX)
    if DEX_TP.exists():
        if hashlib.sha256(zo.read("classes.dex")).digest() == hashlib.sha256(zn.read("classes.dex")).digest():
            raise SystemExit("classes.dex 未生效（仍是官方字节）")
        print("   classes.dex = TapTap 改包法版（唯一被改的官方 dex）")
    print("   官方 dex/arsc/so/jsc 全部逐字节一致 OK")

    if "--skip-sign" in sys.argv:
        print("skip sign ->", unsigned)
        return

    print("[5/5] zipalign + apksigner")
    aligned = WORK / "aligned.apk"
    run([BT / "zipalign", "-f", "-p", "4", unsigned, aligned])
    run([BT / "apksigner", "sign", "--ks", KS, "--ks-key-alias", "starry",
         "--ks-pass", "pass:starry2026", "--key-pass", "pass:starry2026",
         "--out", signed, aligned])
    out = run([BT / "apksigner", "verify", "--print-certs", signed])
    print("\n".join(out.splitlines()[:4]))
    print("SIGNED:", signed, os.path.getsize(signed), hashlib.md5(signed.read_bytes()).hexdigest())


if __name__ == "__main__":
    main()
