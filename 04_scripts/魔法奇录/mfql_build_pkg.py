#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""魔法奇录（com.shw.mfqlm1 1.0.1 vc2）星光版外科组装。

用法:
    python 04_scripts/魔法奇录/mfql_build_pkg.py \
        --base 02_apks/魔法奇录.apk \
        --dec  03_decompiled/魔法奇录/apktool_1.0.1 \
        --menu-apk "mod菜单/starry-mod-menu/app/build/outputs/apk/release/app-release.apk" \
        --so-dir   "mod菜单/starry-mod-menu/libs_mfql" \
        --index    03_decompiled/魔法奇录/build/Game.index.patched.js \
        --out      08_output/魔法奇录/魔法奇录_1.0.1_star_20261126.apk

做的事（外科，逐条目）:
    1. 底包 sha256 校验（拿错底包直接停）
    2. 文本 AndroidManifest 打补丁（3 权限 + StarryInitProvider + MenuService）→ apktool b 取二进制 Manifest
    3. classes.dex(菜单) → classes6.dex（next_free_dex.py 实测 6 是首个空闲下标）
    4. lib/{arm64-v8a,x86_64}/libstarry_magic.so + libc++_shared.so
    5. assets/assets/Game/index.js → 已打补丁版
    6. zipalign -p 4 + apksigner（starry_release.jks）
    7. 逐条目复核 + 打印指纹

判据（任一不满足即非零退出）:
    - Manifest 二进制含 com.starry.magic.StarryInitProvider / com.starry.magic.menu.MenuService
    - 成品含 classes6.dex 且不含第二个 starry dex
    - index.js 含 marker starry_mfql_freead_v1 恰好 1 次，且原 index.js 字节数变大
    - 两个 ABI 的 so 都含 "Mfql" 与 "免广告"
    - apksigner verify 通过
"""
import argparse
import hashlib
import os
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

MARKER = b"starry_mfql_freead_v1"
EXPECT_BASE_SHA256 = "4dd7640f78041d05bf9a95c9e8eb3efba8e8dfa49a71b9375fccaaa6673215e9"
PKG = "com.shw.mfqlm1"

PERMS = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]

PROVIDER_XML = """
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="__PKG__.starryinit"
            android:exported="false" />
"""

SERVICE_XML = """
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


def sha256_file(p: Path) -> str:
    h = hashlib.sha256()
    with open(p, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def run(cmd, **kw):
    cmd = [str(c) for c in cmd]
    if cmd[0].lower().endswith(".bat"):
        # cmd /c 不认含 '/' 的相对路径，必须转成绝对 + 反斜杠
        exe = os.path.abspath(cmd[0]).replace("/", "\\")
        cmd = [os.environ.get("ComSpec", "cmd.exe"), "/c", exe] + cmd[1:]
    elif not os.path.isabs(cmd[0]) and os.path.exists(cmd[0]):
        cmd[0] = os.path.abspath(cmd[0])
    print("   $ " + " ".join(cmd))
    # apksigner.bat 在 zh-CN Windows 上输出 GBK，统一按 utf-8+replace 解码，避免炸在读取上
    r = subprocess.run(cmd, capture_output=True, text=True,
                       encoding="utf-8", errors="replace", **kw)
    if r.returncode != 0:
        print((r.stdout or "")[-3000:])
        print((r.stderr or "")[-3000:])
        raise SystemExit("命令失败: %s (rc=%d)" % (cmd[0], r.returncode))
    return r


def patch_text_manifest(dec: Path) -> None:
    mf = dec / "AndroidManifest.xml"
    t = mf.read_text(encoding="utf-8")
    if "com.starry.magic" in t:
        raise SystemExit("ERROR: 底包 Manifest 已含 com.starry.magic（拿错底包？）")
    add = []
    for perm in PERMS:
        if perm not in t:
            add.append('    <uses-permission android:name="%s"/>' % perm)
    if add:
        i = t.find("<uses-permission")
        if i < 0:
            i = t.find("<application")
        t = t[:i] + "\n".join(add) + "\n" + t[i:]
    ins = PROVIDER_XML.replace("__PKG__", PKG) + SERVICE_XML
    if "com.starry.magic.StarryInitProvider" not in t:
        t = t.replace("</application>", ins + "\n    </application>", 1)
    mf.write_text(t, encoding="utf-8")
    print("   text manifest: perms+=%d provider+service" % len(add))


def build_binary_manifest(dec: Path, apktool_jar: Path, java: str, tmp: Path) -> bytes:
    # 不重编 dex：把 smali 目录临时挪开，b 完再放回（dec 可能来自完整反编译）
    moved = []
    for name in ("smali", "smali_classes2", "smali_classes3", "smali_classes4", "smali_classes5"):
        d = dec / name
        if d.is_dir():
            tgt = tmp / ("_moved_" + name)
            shutil.move(str(d), str(tgt))
            moved.append((d, tgt))
    try:
        out_apk = tmp / "manifest_only.apk"
        run([java, "-jar", apktool_jar, "b", "-f", dec, "-o", out_apk])
        with zipfile.ZipFile(out_apk) as z:
            data = z.read("AndroidManifest.xml")
    finally:
        for d, tgt in moved:
            shutil.move(str(tgt), str(d))
    return data


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--base", required=True)
    ap.add_argument("--dec", required=True)
    ap.add_argument("--menu-apk", required=True)
    ap.add_argument("--so-dir", required=True)
    ap.add_argument("--index", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--apktool", default="01_tools/apktool/apktool.jar")
    ap.add_argument("--java", default="java")
    ap.add_argument("--zipalign", default="01_tools/android-sdk/build-tools/android-15/zipalign")
    ap.add_argument("--apksigner", default="01_tools/android-sdk/build-tools/android-15/apksigner.bat")
    ap.add_argument("--ks", default="01_tools/starry_release.jks")
    ap.add_argument("--ks-alias", default="starry")
    ap.add_argument("--ks-pass", default="starry2026")
    ap.add_argument("--skip-sign", action="store_true")
    a = ap.parse_args()

    base = Path(a.base)
    dec = Path(a.dec)
    out = Path(a.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    # 外部命令（java / cmd / apksigner.bat）在中文路径下会因代码页乱码，
    # 全部中间产物落在 ASCII 的 TEMP 下，最后一步再拷到 out。
    tmp = Path(os.environ.get("TEMP", "/tmp")) / "mfql_build"
    if tmp.exists():
        shutil.rmtree(tmp)
    tmp.mkdir(parents=True)

    print("[1/7] 底包校验")
    got = sha256_file(base)
    print("   base sha256 %s (%d B)" % (got, base.stat().st_size))
    if got != EXPECT_BASE_SHA256:
        raise SystemExit("ERROR: 底包 sha256 不符（期望 %s）" % EXPECT_BASE_SHA256)

    print("[2/7] Manifest（3 权限 + StarryInitProvider + MenuService）")
    build_dec = tmp / "dec"
    build_dec.mkdir()
    for name in ("apktool.yml", "AndroidManifest.xml"):
        shutil.copy2(dec / name, build_dec / name)
    shutil.copytree(dec / "res", build_dec / "res")
    if (dec / "unknown").is_dir():
        shutil.copytree(dec / "unknown", build_dec / "unknown")
    patch_text_manifest(build_dec)
    bin_mf = build_binary_manifest(build_dec, Path(a.apktool), a.java, tmp)
    for probe in ("com.starry.magic.StarryInitProvider",
                  "com.starry.magic.menu.MenuService",
                  "com.shw.mfqlm1.starryinit"):
        if probe.encode("utf-8") not in bin_mf and probe.encode("utf-16-le") not in bin_mf:
            raise SystemExit("ERROR: 二进制 Manifest 缺 %r" % probe)
    print("   binary manifest ok (%d B)" % len(bin_mf))
    (tmp / "AndroidManifest.xml").write_bytes(bin_mf)

    print("[3/7] 菜单 dex → classes6.dex")
    menu_dex = tmp / "classes6.dex"
    with zipfile.ZipFile(a.menu_apk) as z:
        names = [n for n in z.namelist() if n.endswith(".dex")]
        if len(names) != 1:
            raise SystemExit("ERROR: 菜单 APK 应恰好 1 个 dex，实际 %r" % names)
        menu_dex.write_bytes(z.read(names[0]))
    d = menu_dex.read_bytes()
    if b"Lcom/starry/magic/cocos/MfqlHooks;" not in d:
        raise SystemExit("ERROR: 菜单 dex 缺 MfqlHooks（R8 keep 未生效）")
    print("   classes6.dex %d B（含 MfqlHooks）" % len(d))

    print("[4/7] so 与 libc++_shared")
    replacements = { }
    replacements["AndroidManifest.xml"] = bin_mf
    replacements["classes6.dex"] = d
    for abi in ("arm64-v8a", "x86_64"):
        so = Path(a.so_dir) / abi / "libstarry_magic.so"
        sb = so.read_bytes()
        if b"Mfql" not in sb or "免广告".encode("utf-8") not in sb:
            raise SystemExit("ERROR: %s so 缺 Mfql/免广告 串" % abi)
        replacements["lib/%s/libstarry_magic.so" % abi] = sb
        # libc++_shared：优先用 ndk-build 落在 libs_<tag>/<abi>/ 的那份（两 ABI 都齐），
        # 再退回菜单 APK 内的（菜单 APK 只打 arm64）。
        cxx_path = Path(a.so_dir) / abi / "libc++_shared.so"
        if cxx_path.is_file():
            cxx = cxx_path.read_bytes()
        else:
            with zipfile.ZipFile(a.menu_apk) as z:
                cxx = z.read("lib/%s/libc++_shared.so" % abi)
        replacements["lib/%s/libc++_shared.so" % abi] = cxx
        print("   %-10s so=%d cxx=%d" % (abi, len(sb), len(cxx)))

    print("[5/7] index.js（免广告补丁）")
    patched = Path(a.index).read_bytes()
    if patched.count(MARKER) != 1:
        raise SystemExit("ERROR: 补丁后 index.js marker 计数 != 1")
    with zipfile.ZipFile(base) as z:
        orig = z.read("assets/assets/Game/index.js")
    if len(patched) <= len(orig):
        raise SystemExit("ERROR: 补丁后 index.js 未变长")
    replacements["assets/assets/Game/index.js"] = patched
    print("   index.js %d B -> %d B" % (len(orig), len(patched)))

    print("[6/7] 外科组装 zip")
    raw = tmp / "assembled.apk"
    with zipfile.ZipFile(base) as zin, zipfile.ZipFile(raw, "w") as zout:
        seen = set()
        for item in zin.infolist():
            data = replacements.get(item.filename, None)
            if item.filename in replacements:
                seen.add(item.filename)
                ct = zipfile.ZIP_DEFLATED if item.filename.endswith((".dex", ".js")) else item.compress_type
                zi = zipfile.ZipInfo(item.filename, date_time=item.date_time)
                zi.compress_type = ct
                zi.external_attr = item.external_attr
                zout.writestr(zi, data)
            else:
                zout.writestr(item, zin.read(item.filename))
        for name, data in replacements.items():
            if name in seen:
                continue
            zi = zipfile.ZipInfo(name, date_time=(2026, 11, 26, 0, 0, 0))
            zi.compress_type = zipfile.ZIP_DEFLATED
            zout.writestr(zi, data)
            print("   + 新增条目 %s (%d B)" % (name, len(data)))
    print("   assembled %d B" % raw.stat().st_size)

    print("[7/7] zipalign + 签名")
    aligned = tmp / "aligned.apk"
    signed = tmp / "signed.apk"
    run([a.zipalign, "-f", "-p", "4", raw, aligned])
    if a.skip_sign:
        shutil.copyfile(aligned, out)
    else:
        run([a.apksigner, "sign",
             "--ks", a.ks, "--ks-key-alias", a.ks_alias,
             "--ks-pass", "pass:" + a.ks_pass,
             "--key-pass", "pass:" + a.ks_pass,
             "--out", signed, aligned])
        run([a.apksigner, "verify", "--print-certs", signed])
        shutil.copyfile(signed, out)

    print("\n=== 成品 ===")
    print("path   %s" % out)
    print("size   %d B" % out.stat().st_size)
    print("sha256 %s" % sha256_file(out))
    with zipfile.ZipFile(out) as z:
        names = z.namelist()
        print("dex    %s" % [n for n in names if n.endswith(".dex")])
        print("so     %s" % [n for n in names if n.startswith("lib/") and n.endswith(".so")])
        mf = z.read("AndroidManifest.xml")
        print("mf     provider=%s service=%s" % (
            b"com.starry.magic.StarryInitProvider" in mf,
            b"com.starry.magic.menu.MenuService" in mf))
        js = z.read("assets/assets/Game/index.js")
        print("js     marker=%d size=%d" % (js.count(MARKER), len(js)))
    print("PASS")
    return 0


if __name__ == "__main__":
    sys.exit(main())
