#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""勇士之心 1.0.3 星光外科组装（加固包自动脱壳 + 多 ABI）。

为什么用 Python 而不是 bash：
  * 底包/仓库路径含中文，bash 侧 `cygpath`/引号/GBK 控制台反复踩坑；
  * 多 ABI（arm64-v8a / x86 / armeabi-v7a）与「下一个空闲 classesN.dex」都要算。

流程：
  1. 底包若有 fancy/origin.apk（加固壳）→ 取出真实游戏 APK 当底包（脱壳）；
  2. apktool 解 AndroidManifest → 追加星光 provider/service/FGS 权限 → 回编 → 只取 manifest；
  3. zip 外科合并：换 manifest + 新增 classesN.dex + 每个 ABI 的 libstarry_magic.so /
     libc++_shared.so + assets/certs/starlight.crt；其余条目逐字节保留（回读校验）；
  4. zipalign -p + apksigner 同钥匙重签 + 证书指纹校验。

用法：
  python3 04_scripts/勇士之心/ysxz_assemble_103.py [--abis "arm64-v8a x86"] [--out DIR]

依赖：JAVA_HOME、ANDROID_HOME（build-tools 里含 zipalign/apksigner/aapt2）、NDK sysroot 的 libc++_shared。
"""
from __future__ import annotations

import argparse
import hashlib
import io
import os
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path

GAME = "勇士之心"
VERSION = "1.0.3"
PKG = "com.fightingstars.games.warrior.tt1"
WANT_CERT = "83C5CC0BE218D046B3B7AF0D2A9F8236297FE228171AF2974B71BAC130736F1D"
CXX_DIR = {
    "arm64-v8a": "aarch64-linux-android",
    "x86": "i686-linux-android",
    "armeabi-v7a": "arm-linux-androideabi",
    "x86_64": "x86_64-linux-android",
}
# so 侧必须带的串（1.0.6 全集菜单 + 新定位 + 自举兜底）
SO_TOKENS = [
    "Slider_伤害倍率:1:50:1",
    "Slider_移动速度:1:10:1",
    "Slider_战斗加速:1:5:1",
    "Toggle_自动战斗",
    "Toggle_解锁金卡",
    "Toggle_自动穿戴",
    "evalString resolved by dynsym",
    "readable-map fallback",
]


def die(msg: str) -> "None":
    print("ERROR: " + msg, file=sys.stderr)
    raise SystemExit(1)


def run(cmd, **kw):
    p = subprocess.run(cmd, capture_output=True, text=True, **kw)
    if p.returncode != 0:
        die("命令失败(%d): %s\n%s\n%s" % (p.returncode, " ".join(map(str, cmd)), p.stdout[-2000:], p.stderr[-2000:]))
    return p


def first_existing(paths, what):
    for p in paths:
        if p and Path(p).exists():
            return Path(p)
    die("找不到 %s（试过: %s）" % (what, ", ".join(str(p) for p in paths if p)))


def main() -> int:
    here = Path(__file__).resolve().parent
    root = Path(os.environ.get("NIXIANG") or here.parents[1])

    ap = argparse.ArgumentParser()
    ap.add_argument("--orig-apk", default=str(root / "02_apks" / (GAME + ".apk")))
    ap.add_argument("--dex", default=str(root / "mod菜单/starry-mod-menu/libs_ysxz/classes4.dex"))
    ap.add_argument("--libroot", default=str(root / "mod菜单/starry-mod-menu/libs_ysxz"))
    ap.add_argument("--certs", default=str(root / "mod菜单/starry-mod-menu/app/src/main/assets/certs/starlight.crt"))
    ap.add_argument("--abis", default="")
    ap.add_argument("--out", default=str(root / "08_output" / GAME))
    ap.add_argument("--keystore", default=str(root / "01_tools/starry_release.jks"))
    ap.add_argument("--ks-pass", default="starry2026")
    ap.add_argument("--ks-alias", default="starry")
    a = ap.parse_args()

    java_home = os.environ.get("JAVA_HOME", "")
    java = Path(java_home) / "bin" / ("java.exe" if os.name == "nt" else "java")
    if not java.exists():
        die("找不到 java（设 JAVA_HOME 到 JDK 17）")
    sdk = os.environ.get("ANDROID_HOME") or os.environ.get("ANDROID_SDK_ROOT") or ""
    if not sdk:
        die("找不到 Android SDK（设 ANDROID_HOME）")
    bt = sorted((Path(sdk) / "build-tools").glob("*"), key=lambda p: p.name)[-1]
    exe = ".exe" if os.name == "nt" else ""
    zipalign = bt / ("zipalign" + exe)
    aapt2 = bt / ("aapt2" + exe)
    apksigner_jar = bt / "lib" / "apksigner.jar"
    print("[0/8] build-tools = %s" % bt.name)

    ndk = os.environ.get("ANDROID_NDK_HOME") or str(Path(sdk) / "ndk" / "27.0.12077973")
    prebuilt = None
    for name in ("windows-x86_64", "darwin-x86_64", "linux-x86_64"):
        cand = Path(ndk) / "toolchains" / "llvm" / "prebuilt" / name / "sysroot" / "usr" / "lib"
        if cand.is_dir():
            prebuilt = cand
            break
    if prebuilt is None:
        die("找不到 NDK sysroot: %s" % ndk)

    orig = Path(a.orig_apk)
    dex = Path(a.dex)
    libroot = Path(a.libroot)
    cert = Path(a.certs)
    ks = Path(a.keystore)
    apktool = root / "01_tools" / "apktool" / "apktool.jar"
    for p, what in ((orig, "底包"), (dex, "菜单 dex"), (cert, "授权证书"), (ks, "签名钥匙"), (apktool, "apktool.jar")):
        if not p.exists():
            die("缺%s: %s" % (what, p))

    work = Path(tempfile.mkdtemp(prefix="ysxz_asm103_"))
    try:
        # ---------- 1. 脱壳 ----------
        base = orig
        if "fancy/origin.apk" in zipfile.ZipFile(orig).namelist():
            print("[1/8] 检测到加固壳 fancy/ ⇒ 取出真实游戏底包（脱壳）")
            data = zipfile.ZipFile(orig).read("fancy/origin.apk")
            base = work / "base_origin.apk"
            base.write_bytes(data)
            zb = zipfile.ZipFile(base)
            if "AndroidManifest.xml" not in zb.namelist() or "classes.dex" not in zb.namelist():
                die("fancy/origin.apk 不是合法 APK")
            print("  origin.apk ok: %d entries, %d B" % (len(zb.namelist()), len(data)))
        else:
            print("[1/8] 明文底包（无 fancy/ 加固壳）")

        zb = zipfile.ZipFile(base)
        names = set(zb.namelist())

        # ---------- 2. dex 名 + ABI ----------
        mx = 0
        for n in names:
            m = re.fullmatch(r"classes(\d*)\.dex", n)
            if m:
                mx = max(mx, int(m.group(1) or 1))
        new_dex = "classes%d.dex" % (mx + 1)
        base_abis = {n.split("/")[1] for n in names
                     if n.startswith("lib/") and n.count("/") == 2 and n.endswith("/libcocos2djs.so")}
        ours = {d.name for d in libroot.iterdir() if (d / "libstarry_magic.so").is_file()} if libroot.is_dir() else set()
        want = [x for x in (a.abis.split() or ["arm64-v8a", "x86", "armeabi-v7a", "x86_64"]) if x]
        abis = [x for x in want if x in base_abis and x in ours]
        if not abis:
            die("没有可用 ABI（底包库=%s / libs_ysxz=%s）" % (sorted(base_abis), sorted(ours)))
        print("[2/8] 星光 dex = %s ; ABI = %s" % (new_dex, " ".join(abis)))

        # ---------- 3. 断言 ----------
        for abi in abis:
            so = libroot / abi / "libstarry_magic.so"
            blob = so.read_bytes()
            for tok in SO_TOKENS:
                if tok.encode() not in blob:
                    die("%s so 缺 [%s]" % (abi, tok))
            if "Slider_移动速度:1:5:1".encode() in blob:
                die("%s so 是旧菜单" % abi)
            if not (prebuilt / CXX_DIR[abi] / "libc++_shared.so").exists():
                die("缺 %s libc++_shared.so（%s）" % (abi, prebuilt))
        dexblob = dex.read_bytes()
        if b"MagicRes$PanelBgDrawable" not in dexblob:
            die("dex 无 PanelBgDrawable 外观串（旧外观）")
        if b"YsxzAppComponentFactory" in dexblob:
            die("dex 混进 1.0.4 过检测类")

        # ---------- 4. Manifest ----------
        print("[3/8] 解/改/回编 AndroidManifest（保留官方 Application）")
        dec = work / "orig_dec"
        run([str(java), "-jar", str(apktool), "d", "-f", "-s", "--no-assets", "-o", str(dec), str(base)])
        mf_path = dec / "AndroidManifest.xml"
        t = mf_path.read_text(encoding="utf-8")
        perms = []
        if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
            perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
        if 'FOREGROUND_SERVICE"' not in t:
            perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
        if perms:
            i = t.find("<uses-permission")
            if i < 0:
                die("manifest 里没有 <uses-permission>")
            t = t[:i] + "\n".join(perms) + "\n" + t[i:]
        if "com.starry.magic.StarryInitProvider" not in t:
            prov = (
                '\n        <provider\n'
                '            android:name="com.starry.magic.StarryInitProvider"\n'
                '            android:authorities="%s.starryinit"\n'
                '            android:enabled="true"\n'
                '            android:exported="false"\n'
                '            android:initOrder="100" />\n' % PKG
            )
            t = t.replace("</application>", prov + "\n    </application>", 1)
        if "com.starry.magic.menu.MenuService" not in t:
            svc = (
                '\n        <service\n'
                '            android:name="com.starry.magic.menu.MenuService"\n'
                '            android:enabled="true"\n'
                '            android:exported="false"\n'
                '            android:foregroundServiceType="specialUse"\n'
                '            android:stopWithTask="false">\n'
                '            <property\n'
                '                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"\n'
                '                android:value="MOD menu overlay" />\n'
                '        </service>\n'
            )
            t = t.replace("</application>", svc + "\n    </application>", 1)
        mf_path.write_text(t, encoding="utf-8")
        mf_apk = work / "mf_only.apk"
        run([str(java), "-jar", str(apktool), "b", "-f", str(dec), "-o", str(mf_apk)])
        with zipfile.ZipFile(mf_apk) as zm:
            new_mf = zm.read("AndroidManifest.xml")
        if not new_mf:
            die("manifest 回编产物为空")

        # aapt2 复核
        def xmltree(src: Path) -> str:
            p = subprocess.run([str(aapt2), "dump", "xmltree", str(src), "--file", "AndroidManifest.xml"],
                               capture_output=True, text=True)
            return p.stdout if p.returncode == 0 else ""

        check = work / "mf_check.apk"
        with zipfile.ZipFile(check, "w") as zc:
            zc.writestr("AndroidManifest.xml", new_mf)
        tree_new = xmltree(check)
        tree_orig = xmltree(base)
        for need in ("com.starry.magic.StarryInitProvider", "com.starry.magic.menu.MenuService",
                     "FOREGROUND_SERVICE_SPECIAL_USE"):
            if need not in tree_new:
                die("新 manifest 缺 %s" % need)
        if not re.search(r"WarriorApplication|org\.cocos2dx", tree_new):
            die("新 manifest 丢了官方 Application")
        if tree_orig:
            o = re.sub(r"\(line=\d+\)", "", tree_orig).splitlines()
            n_ = re.sub(r"\(line=\d+\)", "", tree_new).splitlines()
            import difflib
            lost = [l for l in difflib.unified_diff(o, n_, lineterm="") if l.startswith("-") and not l.startswith("---")]
            if lost:
                die("manifest 丢失官方节点 %d 行：%s" % (len(lost), lost[:3]))
            print("  ✓ manifest 官方节点零丢失")

        # ---------- 5. zip 合并 ----------
        print("[4/8] zip 外科合并（其余条目逐字节保留）")
        adds = {new_dex: dexblob}
        for abi in abis:
            adds["lib/%s/libstarry_magic.so" % abi] = (libroot / abi / "libstarry_magic.so").read_bytes()
            adds["lib/%s/libc++_shared.so" % abi] = (prebuilt / CXX_DIR[abi] / "libc++_shared.so").read_bytes()
        adds["assets/certs/starlight.crt"] = cert.read_bytes()
        for k in adds:
            if k in names:
                die("底包已有条目 %s（不该覆盖）" % k)

        def is_sig(name: str) -> bool:
            up = name.upper()
            if not up.startswith("META-INF/"):
                return False
            return up.endswith((".SF", ".RSA", ".DSA", ".EC")) or up.endswith("MANIFEST.MF")

        unsigned = work / ("%s_%s_starry-final-unsigned.apk" % (GAME, VERSION))
        untouched = []
        with zipfile.ZipFile(base) as zin, zipfile.ZipFile(unsigned, "w", zipfile.ZIP_DEFLATED, allowZip64=True) as zout:
            for it in zin.infolist():
                n = it.filename
                if is_sig(n) or n in adds:
                    continue
                data = new_mf if n == "AndroidManifest.xml" else zin.read(it)
                zi = zipfile.ZipInfo(n, date_time=it.date_time)
                zi.compress_type = (zipfile.ZIP_STORED if n == "resources.arsc"
                                    else zipfile.ZIP_DEFLATED if n == "AndroidManifest.xml"
                                    else it.compress_type)
                zi.external_attr = it.external_attr
                zi.create_system = it.create_system
                zout.writestr(zi, data)
                if n != "AndroidManifest.xml":
                    untouched.append(n)
            for n, data in sorted(adds.items()):
                zi = zipfile.ZipInfo(n)
                zi.compress_type = zipfile.ZIP_DEFLATED
                zi.create_system = 3
                zout.writestr(zi, data)

        # 回读校验
        bad = []
        with zipfile.ZipFile(base) as z1, zipfile.ZipFile(unsigned) as z2:
            out_names = set(z2.namelist())
            for n in untouched:
                if hashlib.sha256(z1.read(n)).digest() != hashlib.sha256(z2.read(n)).digest():
                    bad.append(n)
            for n in adds:
                if n not in out_names:
                    bad.append("missing:" + n)
            if any(is_sig(x) for x in out_names):
                bad.append("sig leaked")
        if bad:
            die("完整性校验失败: %s" % bad[:8])
        print("  entries 保留 %d 条，逐字节校验通过" % len(untouched))

        # ---------- 6. zipalign + 签名 ----------
        print("[5/8] zipalign + 重签名")
        aligned = work / "aligned.apk"
        p = subprocess.run([str(zipalign), "-p", "-f", "4", str(unsigned), str(aligned)], capture_output=True)
        if p.returncode != 0:
            run([str(zipalign), "-f", "4", str(unsigned), str(aligned)])
        run([str(zipalign), "-c", "-v", "4", str(aligned)])

        out_dir = Path(a.out)
        out_dir.mkdir(parents=True, exist_ok=True)
        signed = out_dir / ("%s_%s_starry-final-releaseSigned.apk" % (GAME, VERSION))
        run([str(java), "-jar", str(apksigner_jar), "sign",
             "--ks", str(ks), "--ks-pass", "pass:" + a.ks_pass,
             "--ks-key-alias", a.ks_alias, "--key-pass", "pass:" + a.ks_pass,
             "--v1-signing-enabled", "true", "--v2-signing-enabled", "true", "--v3-signing-enabled", "true",
             "--out", str(signed), str(aligned)])

        # ---------- 7. 校验 ----------
        print("[6/8] 校验")
        v = subprocess.run([str(java), "-jar", str(apksigner_jar), "verify", "--print-certs", str(signed)],
                           capture_output=True, text=True)
        m = re.search(r"SHA-256 digest:\s*([0-9a-f:]+)", v.stdout, re.I)
        got = m.group(1).replace(":", "").upper() if m else "(none)"
        if got != WANT_CERT:
            die("证书指纹不一致 got=%s" % got)
        print("  ✓ 证书 SHA-256 %s" % got)

        with zipfile.ZipFile(signed) as zs, zipfile.ZipFile(base) as zo:
            def md5(b):
                return hashlib.md5(b).hexdigest()
            ok = True
            def rep(label, g, e):
                nonlocal ok
                good = g == e
                ok = ok and good
                print("    %-40s %s  %s" % (label, g, "OK" if good else "MISMATCH " + e))
            rep(new_dex, md5(zs.read(new_dex)), md5(dexblob))
            for abi in abis:
                rep("so[%s]" % abi, md5(zs.read("lib/%s/libstarry_magic.so" % abi)),
                    md5((libroot / abi / "libstarry_magic.so").read_bytes()))
            for n in ("classes.dex", "resources.arsc", "assets/main.js"):
                if n in zo.namelist():
                    rep(n, md5(zs.read(n)), md5(zo.read(n)))
            if not ok:
                die("条目 md5 校验失败")

        # ---------- 8. 报告 ----------
        size = signed.stat().st_size
        print("[7/8] 产物")
        print("  底包 : %s" % base)
        print("  ABI  : %s" % " ".join(abis))
        print("  包   : %s" % signed)
        print("  体积 : %d B" % size)
        print("  md5  : %s" % hashlib.md5(signed.read_bytes()).hexdigest())
        print("  sha  : %s" % hashlib.sha256(signed.read_bytes()).hexdigest())
        print("  dex  : %s" % hashlib.md5(dexblob).hexdigest())
        for abi in abis:
            print("  so[%s] : %s" % (abi, hashlib.md5((libroot / abi / "libstarry_magic.so").read_bytes()).hexdigest()))
        print("[8/8] done")
        return 0
    finally:
        shutil.rmtree(work, ignore_errors=True)


if __name__ == "__main__":
    sys.exit(main())
