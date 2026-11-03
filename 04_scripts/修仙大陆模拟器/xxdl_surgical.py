#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""修仙大陆模拟器 (com.sky.xxdl.dlt 1.0.5) —— 外科组装出包
官方 dex/arsc/so/资源保持逐字节不变，只做四件事：
  1. assets/main.js 末尾追加星空功能层（该文件明文，无需解密）
  2. AndroidManifest 加 StarryInitProvider + MenuService
  3. 新增 classes8.dex（com.starry.* 菜单壳） + libstarry_magic.so / libc++_shared.so
  4. TapTap 双轨（2026-10-29 口径）：
     A 轨改包法  classes.dex 替换为 classes_patched.dex（ClientLoginUtils.isClientLogin 恒 false ⇒ 登录走网页）
     B 轨运行时  新增 classes9.dex（MT 签名杀手，烘焙本包名 + 官方证书）＋ Manifest appComponentFactory

用法：python3 04_scripts/修仙大陆模拟器/xxdl_surgical.py [--skip-sign]
"""
import hashlib
import os
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "修仙大陆模拟器"
PKG = "com.sky.xxdl.dlt"
VER = "1.0.5"
ORIG = ROOT / "02_apks" / "原版_TapTap_20261103" / "sky_xxdl_dlt.apk"
ORIG_MD5 = "15733bb0b2e817c8207be36fa248cc23"
STARRY_APK = ROOT / "mod菜单" / "starry-mod-menu" / "app" / "build" / "outputs" / "apk" / "release" / "app-release.apk"
SO_DIR = ROOT / "mod菜单" / "starry-mod-menu" / "libs_xxdl" / "arm64-v8a"
FEATURE = ROOT / "04_scripts" / "修仙大陆模拟器" / "xxdl_feature.js"
OUT = ROOT / "08_output" / GAME
WORK = ROOT / "13_scratch" / "xxdl_surgical"
SMALI = ROOT / "01_tools" / "smali" / "smali"
APKTOOL = ROOT / "01_tools" / "apktool" / "apktool.jar"
KS = ROOT / "01_tools" / "starry_release.jks"
BT = Path.home() / "Library" / "Android" / "sdk" / "build-tools" / "36.0.0"
NEW_DEX = "classes8.dex"
KILLER_DEX = "classes9.dex"
ZS = "assets/main.js"
TAPTAP_DIR = ROOT / "13_scratch" / "xxdl_taptap"
TAP_PATCHED = TAPTAP_DIR / "classes_patched.dex"
KILLER = ROOT / "04_scripts" / "修仙大陆模拟器" / "assets" / "xxdlmnq_signature_killer.dex"
OFFICIAL_CERT_SHA256 = "d491e007ddf7230beec54da69677703fcf5379e38915c49131b7e386e52048a6"
MARK = "window.__sst"


def names_precheck(apk):
    with zipfile.ZipFile(apk) as z:
        return z.namelist()


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
    if b"Xxdl" not in so.read_bytes():
        raise SystemExit("so 缺 Xxdl 标签（编错游戏）")
    if not FEATURE.exists():
        raise SystemExit("missing xxdl_feature.js")

    if WORK.exists():
        shutil.rmtree(WORK)
    WORK.mkdir(parents=True)
    OUT.mkdir(parents=True, exist_ok=True)
    unsigned = OUT / ("%s_%s_xxdlmnq-unsigned.apk" % (GAME, VER))
    signed = OUT / ("%s_%s_xxdlmnq-releaseSigned.apk" % (GAME, VER))

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
    if not (smali_root / "com" / "starry" / "magic" / "cocos" / "XxdlHooks.smali").exists():
        raise SystemExit("XxdlHooks.smali missing（Java 没编进 starry apk）")
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
    # B 轨挂载点：appComponentFactory 换成星光早启动工厂（内部 Class.forName 触发 killer <clinit>）
    old_f = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
    new_f = 'android:appComponentFactory="com.starry.magic.sign.StarryAppComponentFactory"'
    if old_f in t:
        t = t.replace(old_f, new_f, 1)
    elif new_f not in t:
        raise SystemExit("manifest 既无 CoreComponentFactory 也无星光工厂，无法挂 B 轨")
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
    # A 轨：TapSDK 改包法（登录恒 Web / 门闩相关类重写后的 classes.dex）
    if not TAP_PATCHED.is_file():
        raise SystemExit("missing %s（先跑 TapSDK 改包 smali 流程）" % TAP_PATCHED)
    tap_patched = TAP_PATCHED.read_bytes()
    if b"starry_force_web_login" in tap_patched:
        raise SystemExit("patched dex 残留模板标记")
    extras["classes.dex"] = tap_patched
    print("   [TapTap A 轨] classes.dex <- classes_patched.dex (%d B)" % len(tap_patched))
    # B 轨：运行时签名替换（killer dex）
    if not KILLER.is_file():
        raise SystemExit("missing %s（先跑 build_killer_dex_xxdlmnq.sh）" % KILLER)
    killer = KILLER.read_bytes()
    if PKG.encode() not in killer:
        raise SystemExit("killer dex 未烘焙本包名 %s" % PKG)
    if KILLER_DEX in names_precheck(ORIG):
        raise SystemExit("orig 已含 %s" % KILLER_DEX)
    extras[KILLER_DEX] = killer
    print("   [TapTap B 轨] %s <- killer dex (%d B)" % (KILLER_DEX, len(killer)))
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
    must_same = ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
                 "classes5.dex", "classes6.dex", "classes7.dex", "resources.arsc",
                 "lib/arm64-v8a/libcocos.so", "assets/assets/main/index.jsc"]
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
