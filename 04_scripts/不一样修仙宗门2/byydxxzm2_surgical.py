#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""不一样的修仙宗门2 (com.gamebl.byydxxzm.xxt, TapTap/阔游 CL 渠道)
星光外科组装：官方 dex/arsc/so/assets 全部保持逐字节不变，只做四件事
  1. AndroidManifest.xml：保留官方 CLApplication + CoreComponentFactory，仅新增
     StarryInitProvider / MenuService / 两条前台服务权限
  2. 新增 classes6.dex（星光 smali：菜单/Auth/Byydxxzm2Hooks）
  3. 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
  4. 去掉官方 META-INF 签名条目
然后用 apksigner 签名。

用法：python 04_scripts/不一样修仙宗门2/byydxxzm2_surgical.py [--skip-sign]
"""
import os, sys, shutil, subprocess, zipfile, hashlib
from pathlib import Path

ROOT = Path(r"E:\nixiang")
GAME = "不一样的修仙宗门2"
PKG = "com.gamebl.byydxxzm.xxt"
ORIG = ROOT / "02_apks" / GAME / "byydxxzm_tap_original.apk"
ORIG_MD5 = "ac2ad76a4ad7244678f22601f4c2c284"
STARRY_APK = ROOT / "_starry_build" / "app" / "build" / "outputs" / "apk" / "release" / "app-release.apk"
SO_DIR = ROOT / "mod菜单" / "starry-mod-menu" / "libs_byydxxzm2" / "arm64-v8a"
OUT = ROOT / "08_output" / GAME
WORK = ROOT / "13_scratch" / "byydxxzm2_surgical"
SMALI = ROOT / "01_tools" / "smali" / "smali"
APKTOOL = ROOT / "01_tools" / "apktool" / "apktool.jar"
KS = ROOT / "01_tools" / "starry_release.jks"
BT = Path(r"C:\Users\Administrator\AppData\Local\Android\Sdk\build-tools\36.1.0")
NEW_DEX = "classes6.dex"
JAVA = Path(r"C:\Program Files\Microsoft\jdk-17.0.20.101-hotspot\bin\java.exe")


def run(cmd, **kw):
    print("  $", " ".join(str(c) for c in cmd[:6]))
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True, encoding="utf-8", errors="replace", **kw)
    if r.returncode != 0:
        print(r.stdout[-3000:])
        print(r.stderr[-3000:])
        raise SystemExit("FAILED: %s" % cmd[0])
    return r.stdout


def main():
    if not ORIG.exists():
        raise SystemExit("missing orig apk")
    md5 = hashlib.md5(ORIG.read_bytes()).hexdigest()
    if md5 != ORIG_MD5:
        raise SystemExit("orig md5 mismatch: %s" % md5)
    if not STARRY_APK.exists():
        raise SystemExit("missing starry apk (先跑 gradlew assembleRelease)")
    so = SO_DIR / "libstarry_magic.so"
    if not so.exists():
        raise SystemExit("missing libstarry_magic.so")
    if b"byydxxzm2" not in so.read_bytes():
        raise SystemExit("so 缺 byydxxzm2 标签")

    if WORK.exists():
        shutil.rmtree(WORK)
    WORK.mkdir(parents=True)
    OUT.mkdir(parents=True, exist_ok=True)

    # 版本号
    ver = "1.1.8"
    yml = ROOT / "03_decompiled" / GAME / "apktool" / "apktool.yml"
    if yml.exists():
        for line in yml.read_text(encoding="utf-8", errors="ignore").splitlines():
            if line.strip().startswith("versionName:"):
                ver = line.split(":", 1)[1].strip().strip("'\"") or ver
    unsigned = OUT / ("%s_%s_byydxxzm2-unsigned.apk" % (GAME, ver))
    signed = OUT / ("%s_%s_byydxxzm2-releaseSigned.apk" % (GAME, ver))

    # 1) starry apk -> com/starry smali -> classes6.dex
    print("[1/5] starry apk -> smali ->", NEW_DEX)
    dec_starry = WORK / "starry_dec"
    run([JAVA, "-jar", APKTOOL, "d", "-f", "-o", dec_starry, STARRY_APK])
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
    hooks = smali_root / "com" / "starry" / "magic" / "cocos" / "Byydxxzm2Hooks.smali"
    if not hooks.exists():
        raise SystemExit("Byydxxzm2Hooks.smali missing（Java 没编进 starry apk）")
    print("   starry smali dirs:", n)
    run([root_bash(), SMALI, "assemble", "-o", WORK / NEW_DEX, smali_root])

    # 2) manifest patch
    print("[2/5] AndroidManifest patch")
    dec_orig = WORK / "orig_dec"
    run([JAVA, "-jar", APKTOOL, "d", "-f", "-s", "--no-assets", "-o", dec_orig, ORIG])
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
    run([JAVA, "-jar", APKTOOL, "b", "-f", dec_orig, "-o", WORK / "mf_only.apk"])
    with zipfile.ZipFile(WORK / "mf_only.apk") as z:
        new_mf = z.read("AndroidManifest.xml")

    # 3) zip surgical merge
    print("[3/5] zip merge ->", unsigned.name)
    jsc = ROOT / "13_scratch" / "byydxxzm2_js" / "project.jsc"
    if not jsc.exists():
        raise SystemExit("missing patched project.jsc (先跑 byydxxzm2_patch_js.py)")
    extras = {"assets/src/project.jsc": jsc.read_bytes()}
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
        info = zipfile.ZipInfo(name, (2026, 11, 2, 0, 0, 0))
        info.compress_type = zipfile.ZIP_DEFLATED
        info.external_attr = 0o644 << 16
        info.create_system = 3
        zout.writestr(info, data)
    info = zipfile.ZipInfo(NEW_DEX, (2026, 11, 2, 0, 0, 0))
    info.compress_type = zipfile.ZIP_DEFLATED
    info.external_attr = 0o644 << 16
    info.create_system = 3
    zout.writestr(info, (WORK / NEW_DEX).read_bytes())
    zout.close()
    zin.close()

    # 4) 断言官方字节不变
    print("[4/5] assert untouched entries")
    zo = zipfile.ZipFile(ORIG)
    zn = zipfile.ZipFile(unsigned)
    must_same = ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes5.dex",
                 "resources.arsc", "assets/src/settings.jsc",
                 "lib/arm64-v8a/libcocos2djs.so"]
    for m in must_same:
        if hashlib.sha256(zo.read(m)).digest() != hashlib.sha256(zn.read(m)).digest():
            raise SystemExit("官方条目被改动: %s" % m)
    print("   官方 dex/arsc/jsc/so 全部逐字节一致 ✓")

    if "--skip-sign" in sys.argv:
        print("skip sign ->", unsigned)
        return

    # 5) sign
    print("[5/5] zipalign + apksigner")
    aligned = WORK / "aligned.apk"
    run([BT / "zipalign.exe", "-f", "-p", "4", unsigned, aligned])
    run([BT / "apksigner.bat", "sign", "--ks", KS, "--ks-key-alias", "starry",
         "--ks-pass", "pass:starry2026", "--key-pass", "pass:starry2026",
         "--out", signed, aligned])
    r = subprocess.run([str(BT / "apksigner.bat"), "verify", "--print-certs", str(signed)],
                       capture_output=True, text=True, encoding="utf-8", errors="replace")
    print("\n".join(r.stdout.splitlines()[:4]))
    print("SIGNED:", signed, os.path.getsize(signed), hashlib.md5(signed.read_bytes()).hexdigest())


def root_bash():
    for p in (r"C:\Program Files\Git\bin\bash.exe", "bash"):
        if p == "bash" or os.path.exists(p):
            return p
    return "bash"


if __name__ == "__main__":
    main()
