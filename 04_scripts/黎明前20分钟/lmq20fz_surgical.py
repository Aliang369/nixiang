#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""黎明前20分钟 (com.nanostudios.games.twenty.minutes 1.0.106) —— 外科组装出包

官方 dex/arsc/so/资源保持逐字节不变，只做四件事：
  1. AndroidManifest：加 StarryInitProvider + MenuService；appComponentFactory 换成
     com.starry.magic.sign.StarryAppComponentFactory（它负责挂签名杀手 B 轨，
     本包 <application> 无 android:name ⇒ 走「形态三：appComponentFactory 挂载」）
  2. 新增 classes7.dex（com.starry.* 菜单壳，供 Provider/Service 用）
  3. 新增 classes8.dex（bin.mt.signature 签名杀手，A 轨登录补丁见 lmq20fz_taptap_patch.py）
  4. 用 13_scratch/lmq20fz_taptap/classes5.dex 替换官方 classes5.dex（TapTap 登录恒网页）
  5. 加 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so

用法：python3 04_scripts/黎明前20分钟/lmq20fz_surgical.py [--skip-sign]
"""
import hashlib
import os
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "黎明前20分钟"
PKG = "com.nanostudios.games.twenty.minutes"
VER = "1.0.106"
ORIG = ROOT / "02_apks" / "原版_TapTap_20261103" / "twenty_minutes.apk"
ORIG_MD5 = "5c35c88a6f5eb3cc0f1fb158f35c3bd3"
STARRY_APK = (ROOT / "mod菜单" / "starry-mod-menu" / "app" / "build" / "outputs"
              / "apk" / "release" / "app-release.apk")
SO_DIR = ROOT / "mod菜单" / "starry-mod-menu" / "libs_lmq20fz" / "arm64-v8a"
KILLER = ROOT / "04_scripts" / GAME / "assets" / "lmq20fz_signature_killer.dex"
TAPTAP_DEX = ROOT / "13_scratch" / "lmq20fz_taptap" / "classes5.dex"
OUT = ROOT / "08_output" / GAME
WORK = ROOT / "13_scratch" / "lmq20fz_surgical"
SMALI = ROOT / "01_tools" / "smali" / "smali"
APKTOOL = ROOT / "01_tools" / "apktool" / "apktool.jar"
KS = ROOT / "01_tools" / "starry_release.jks"
BT = Path.home() / "Library" / "Android" / "sdk" / "build-tools" / "36.0.0"
NEW_DEX = "classes7.dex"
KILLER_DEX = "classes8.dex"
TAP_DEX = "classes5.dex"
FACTORY = "com.starry.magic.sign.StarryAppComponentFactory"


def run(cmd):
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True)
    if r.returncode != 0:
        raise SystemExit("FAILED: %s\n%s\n%s" % (" ".join(str(c) for c in cmd),
                                                 r.stdout[-2000:], r.stderr[-2000:]))
    return r.stdout


def main():
    for need, what in ((ORIG, "官方底包"), (STARRY_APK, "星光菜单 apk（先 gradlew assembleRelease）"),
                       (KILLER, "签名杀手 dex（先 build_killer_dex_lmq20fz.sh）"),
                       (TAPTAP_DEX, "TapTap 补丁 dex（先 lmq20fz_taptap_patch.py）")):
        if not need.exists():
            raise SystemExit("missing %s: %s" % (what, need))
    md5 = hashlib.md5(ORIG.read_bytes()).hexdigest()
    if md5 != ORIG_MD5:
        raise SystemExit("orig md5 mismatch: %s" % md5)
    so = SO_DIR / "libstarry_magic.so"
    if not so.exists():
        raise SystemExit("missing libstarry_magic.so（先 ndk-build）")
    if b"lmq20fz" not in so.read_bytes():
        raise SystemExit("so 缺 lmq20fz 标签（编错游戏）")

    if WORK.exists():
        shutil.rmtree(WORK)
    WORK.mkdir(parents=True)
    OUT.mkdir(parents=True, exist_ok=True)
    unsigned = OUT / ("%s_%s_lmq20fz-unsigned.apk" % (GAME, VER))
    signed = OUT / ("%s_%s_lmq20fz-releaseSigned.apk" % (GAME, VER))

    print("[1/6] starry apk -> smali ->", NEW_DEX)
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
    for must in ("com/starry/magic/StarryInitProvider.smali",
                 "com/starry/magic/sign/StarryAppComponentFactory.smali",
                 "com/starry/magic/menu/MenuService.smali"):
        if not (smali_root / must).exists():
            raise SystemExit("starry smali 缺 %s（Java 没编进 starry apk）" % must)
    print("   starry smali dirs:", n)
    run([SMALI, "assemble", "-o", WORK / NEW_DEX, smali_root])

    print("[2/6] AndroidManifest patch")
    dec_orig = WORK / "orig_dec"
    run(["java", "-jar", APKTOOL, "d", "-f", "-s", "--no-assets", "-o", dec_orig, ORIG])
    mf = dec_orig / "AndroidManifest.xml"
    t = mf.read_text(encoding="utf-8")
    if "com.starry.magic.StarryInitProvider" not in t:
        perms = []
        for perm in ("FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
            if perm not in t:
                perms.append('    <uses-permission android:name="android.permission.%s"/>\n' % perm)
        if perms:
            i = t.find("<uses-permission")
            t = t[:i] + "".join(perms) + t[i:]
        prov = ('''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % PKG)
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
    if FACTORY not in t:
        old = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
        if old not in t:
            raise SystemExit("底包 appComponentFactory 形态变了，按实际现取锚点")
        t = t.replace(old, 'android:appComponentFactory="%s"' % FACTORY, 1)
    mf.write_text(t, encoding="utf-8")
    run(["java", "-jar", APKTOOL, "b", "-f", dec_orig, "-o", WORK / "mf_only.apk"])
    with zipfile.ZipFile(WORK / "mf_only.apk") as z:
        new_mf = z.read("AndroidManifest.xml")
    # apktool 编出的 AXML 字符串池实测是 UTF-16LE，两种编码都认
    if not any(FACTORY.encode(enc) in new_mf for enc in ("utf-8", "utf-16-le")):
        raise SystemExit("manifest 回读未见 appComponentFactory")

    print("[3/6] zip merge ->", unsigned.name)
    zin = zipfile.ZipFile(ORIG)
    extras = {}
    extras[TAP_DEX] = TAPTAP_DEX.read_bytes()
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
    for nd in (NEW_DEX, KILLER_DEX):
        if nd in names:
            raise SystemExit("orig 已含 %s" % nd)
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
        info = zipfile.ZipInfo(name, (2026, 11, 4, 0, 0, 0))
        info.compress_type = zipfile.ZIP_DEFLATED
        info.external_attr = 0o644 << 16
        info.create_system = 3
        zout.writestr(info, data)
    for nd, src in ((NEW_DEX, WORK / NEW_DEX), (KILLER_DEX, KILLER)):
        info = zipfile.ZipInfo(nd, (2026, 11, 4, 0, 0, 0))
        info.compress_type = zipfile.ZIP_DEFLATED
        info.external_attr = 0o644 << 16
        info.create_system = 3
        zout.writestr(info, src.read_bytes())
    zout.close()

    print("[4/6] assert untouched entries")
    zo = zipfile.ZipFile(ORIG)
    zn = zipfile.ZipFile(unsigned)
    must_same = ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes6.dex",
                 "resources.arsc", "lib/arm64-v8a/libil2cpp.so",
                 "assets/bin/Data/Managed/Metadata/global-metadata.dat"]
    for m in must_same:
        if m not in zo.namelist():
            continue
        if hashlib.sha256(zo.read(m)).digest() != hashlib.sha256(zn.read(m)).digest():
            raise SystemExit("官方条目被改动: %s" % m)
    if hashlib.sha256(zn.read(TAP_DEX)).digest() != hashlib.sha256(TAPTAP_DEX.read_bytes()).digest():
        raise SystemExit("classes5.dex 未按补丁版替换")
    for nd in (NEW_DEX, KILLER_DEX):
        if nd not in zn.namelist():
            raise SystemExit("%s 未进包" % nd)
    if hashlib.sha256(zn.read(NEW_DEX)).digest() != hashlib.sha256((WORK / NEW_DEX).read_bytes()).digest():
        raise SystemExit("%s 回读不一致" % NEW_DEX)
    print("   官方 dex/arsc/so/元数据逐字节一致 OK；classes5 已换补丁版；7/8 已入包")

    if "--skip-sign" in sys.argv:
        print("skip sign ->", unsigned)
        return

    print("[5/6] zipalign + apksigner")
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
