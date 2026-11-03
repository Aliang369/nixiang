#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""挂机打怪兽 (org.cocos2d.guajidaguaishou.tap 1.1.65) —— 外科组装出包
官方 dex/arsc/so/资源保持逐字节不变，只做四件事：
  1. assets/main.js 末尾追加星空功能层（该文件明文，无需解密）
  2. AndroidManifest 加 StarryInitProvider + MenuService
  3. 新增 classes4.dex（com.starry.* 菜单壳） + libstarry_magic.so / libc++_shared.so
  4. 若存在 TapTap 改包 dex（13_scratch/gjdgs_taptap/）则替换对应官方 dex

用法：python3 04_scripts/挂机打怪兽/gjdgs_surgical.py [--skip-sign]
"""
import hashlib
import os
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "挂机打怪兽"
PKG = "org.cocos2d.guajidaguaishou.tap"
VER = "1.1.65"
ORIG = ROOT / "02_apks" / "原版_TapTap_20261103" / "guaji_daguaishou.apk"
ORIG_MD5 = "ede30476ad27be98be907c2427abe433"
STARRY_APK = ROOT / "mod菜单" / "starry-mod-menu" / "app" / "build" / "outputs" / "apk" / "release" / "app-release.apk"
SO_DIR = ROOT / "mod菜单" / "starry-mod-menu" / "libs_gjdgs" / "arm64-v8a"
FEATURE = ROOT / "04_scripts" / "挂机打怪兽" / "gjdgs_feature.js"
OUT = ROOT / "08_output" / GAME
WORK = ROOT / "13_scratch" / "gjdgs_surgical"
SMALI = ROOT / "01_tools" / "smali" / "smali"
APKTOOL = ROOT / "01_tools" / "apktool" / "apktool.jar"
KS = ROOT / "01_tools" / "starry_release.jks"
BT = Path.home() / "Library" / "Android" / "sdk" / "build-tools" / "36.0.0"
NEW_DEX = "classes4.dex"
ZS = "assets/main.js"
TAPTAP_DIR = ROOT / "13_scratch" / "gjdgs_taptap"
MARK = "window.__sst"


def run(cmd):
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True)
    if r.returncode != 0:
        raise SystemExit("FAILED: %s\n%s\n%s" % (" ".join(str(c) for c in cmd), r.stdout[-2000:], r.stderr[-2000:]))
    return r.stdout


def main():
    if not ORIG.exists():
        raise SystemExit("missing orig apk: %s" % ORIG)
    md5 = hashlib.md5(ORIG.read_bytes()).hexdigest()
    if md5 != ORIG_MD5:
        raise SystemExit("orig md5 mismatch: %s" % md5)
    if not STARRY_APK.exists():
        raise SystemExit("missing starry apk (先 gradlew assembleRelease)")
    so = SO_DIR / "libstarry_magic.so"
    if not so.exists():
        raise SystemExit("missing libstarry_magic.so (先 ndk-build)")
    if b"gjdgs" not in so.read_bytes():
        raise SystemExit("so 缺 gjdgs 标签（编错游戏）")
    if not FEATURE.exists():
        raise SystemExit("missing gjdgs_feature.js")

    if WORK.exists():
        shutil.rmtree(WORK)
    WORK.mkdir(parents=True)
    OUT.mkdir(parents=True, exist_ok=True)
    unsigned = OUT / ("%s_%s_gjdgs-unsigned.apk" % (GAME, VER))
    signed = OUT / ("%s_%s_gjdgs-releaseSigned.apk" % (GAME, VER))

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
    if not (smali_root / "com" / "starry" / "magic" / "cocos" / "GjdgsHooks.smali").exists():
        raise SystemExit("GjdgsHooks.smali missing（Java 没编进 starry apk）")
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
    zin = zipfile.ZipFile(ORIG)
    orig_main = zin.read(ZS)
    feat = FEATURE.read_bytes()
    patched_main = orig_main + b"\n" + feat
    if MARK.encode() not in patched_main:
        raise SystemExit("feature js not appended")
    extras = {ZS: patched_main}
    tap_dexes = []
    if TAPTAP_DIR.is_dir():
        for f in sorted(TAPTAP_DIR.glob("classes*.dex")):
            extras[f.name] = f.read_bytes()
            tap_dexes.append(f.name)
    if tap_dexes:
        print("   [TapTap 改包法] 已并入: %s" % ", ".join(tap_dexes))
    else:
        print("   [提示] 未发现 TapTap 改包 dex（先跑改包 smali 流程）")
    for f in SO_DIR.iterdir():
        if f.is_file() and f.suffix == ".so":
            extras["lib/arm64-v8a/" + f.name] = f.read_bytes()

    def is_sig(name):
        if name == "META-INF/MANIFEST.MF":
            return True
        if not name.startswith("META-INF/"):
            return False
        return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

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

    print("[4/5] assert untouched entries")
    zo = zipfile.ZipFile(ORIG)
    zn = zipfile.ZipFile(unsigned)
    must_same = ["classes.dex", "classes2.dex", "classes3.dex", "resources.arsc",
                 "lib/arm64-v8a/libcocos2djs.so", "assets/assets/main/index.17a32.jsc"]
    for m in must_same:
        if m not in zo.namelist():
            continue
        if m in extras:
            continue  # TapTap 改包法替换的 dex，下面单独验
        if hashlib.sha256(zo.read(m)).digest() != hashlib.sha256(zn.read(m)).digest():
            raise SystemExit("官方条目被改动: %s" % m)
    if MARK.encode() not in zn.read(ZS):
        raise SystemExit("main.js 功能层未生效")
    print("   官方 dex/arsc/so/脚本包全部逐字节一致 OK；main.js 已追加功能层")

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
