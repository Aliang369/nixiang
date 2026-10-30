# -*- coding: utf-8 -*-
"""为了吾王 星光包外科组装：底包 + classes10.dex + libstarry_magic.so(+libc++_shared.so) + 补丁后 manifest

规则：
 - 新增/替换：AndroidManifest.xml、classes10.dex、lib/arm64-v8a/libstarry_magic.so、
   lib/arm64-v8a/libc++_shared.so（2026-10-30 补：官方包只带 libstdc++.so，无 libc++_shared.so，
   而我们的 so NEEDED libc++_shared.so ⇒ 不随包必 dlopen 失败、菜单静默降级。
   先例：skill/re-extract/xlua-from-scratch.md「官方包缺 libc++_shared.so」+ 爆裂防线 surgical 脚本）
 - 删除旧签名文件 META-INF/*.SF|*.RSA|*.DSA|MANIFEST.MF（重签前必须）
 - 其余条目原样搬运（保持压缩方式与顺序）
 - 压缩方式跟底包一致：底包 33 个 lib/*.so 全是 DEFLATED（manifest extractNativeLibs=true），
   故新增 so 也用 DEFLATED（不再用 STORED —— 混入 STORED 会要求 4K 对齐且与包风格不符）
"""
import os
import subprocess
import sys
import tempfile
import zipfile

# NDK 里的 libc++_shared.so（随包，strip 后约 1.3 MB）
NDK_CXX_CANDIDATES = [
    os.environ.get("ANDROID_NDK_HOME", ""),
    r"C:\Users\Administrator\AppData\Local\Android\Sdk\ndk\27.0.12077973",
    r"C:\Users\Administrator\AppData\Local\Android\Sdk\ndk\29.0.14033849",
]
CXX_REL = r"toolchains\llvm\prebuilt\windows-x86_64\sysroot\usr\lib\aarch64-linux-android\libc++_shared.so"
STRIP_REL = r"toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-strip.exe"


def build_cxx_shared():
    for ndk in NDK_CXX_CANDIDATES:
        if not ndk:
            continue
        src = os.path.join(ndk, CXX_REL)
        if os.path.isfile(src):
            strip = os.path.join(ndk, STRIP_REL)
            tmp = os.path.join(tempfile.gettempdir(), "wlww_libcxx_shared.so")
            with open(src, "rb") as f:
                data = f.read()
            with open(tmp, "wb") as f:
                f.write(data)
            if os.path.isfile(strip):
                subprocess.run([strip, "--strip-unneeded", tmp], check=True,
                               stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            with open(tmp, "rb") as f:
                out = f.read()
            print(f"libc++_shared.so: {src}\n  {len(data)} → {len(out)} B（strip 后）")
            return out
    raise SystemExit("找不到 NDK 的 libc++_shared.so，检查 ANDROID_NDK_HOME")

BASE = r"E:\nixiang\13_scratch\ftk_orig4399\base.apk"
MANIFEST = r"E:\nixiang\13_scratch\mani\patched_manifest.bin"
MENU_APK = r"E:\nixiang\mod菜单\starry-mod-menu\app\build\outputs\apk\release\app-release.apk"
SO = r"E:\nixiang\mod菜单\starry-mod-menu\libs_wlww\arm64-v8a\libstarry_magic.so"
OUT = r"E:\nixiang\08_output\为了吾王\为了吾王_3.0.4_wlww_unsigned.apk"

DROP = ("META-INF/", )
SIG_EXT = (".SF", ".RSA", ".DSA", ".EC")
NEW_DEX = "classes10.dex"
SO_ENTRY = "lib/arm64-v8a/libstarry_magic.so"

CXX_ENTRY = "lib/arm64-v8a/libc++_shared.so"

menu = zipfile.ZipFile(MENU_APK)
dex_bytes = menu.read("classes.dex")
menu.close()
so_bytes = open(SO, "rb").read()
man_bytes = open(MANIFEST, "rb").read()
cxx_bytes = build_cxx_shared()
print(f"dex {len(dex_bytes)} B, so {len(so_bytes)} B, cxx {len(cxx_bytes)} B, manifest {len(man_bytes)} B")

src = zipfile.ZipFile(BASE)
names = src.namelist()
assert NEW_DEX not in names, f"{NEW_DEX} 已存在，重算最小空闲下标"
assert SO_ENTRY not in names, "底包已含同名 so"
assert CXX_ENTRY not in names, "底包已含 libc++_shared.so（那就直接复用底包的，不该重复加）"

import os
os.makedirs(os.path.dirname(OUT), exist_ok=True)
dst = zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED, allowZip64=True)

skipped, kept, replaced = 0, 0, 0
for zi in src.infolist():
    n = zi.filename
    if n.startswith("META-INF/") and (n.upper().endswith(SIG_EXT) or n.upper().endswith("MANIFEST.MF")):
        skipped += 1
        continue
    if n == "AndroidManifest.xml":
        dst.writestr(zi, man_bytes)
        replaced += 1
        continue
    with src.open(zi) as fp:
        dst.writestr(zi, fp.read())
    kept += 1

# 新增三项
dst.writestr(zipfile.ZipInfo(NEW_DEX, date_time=(2026, 10, 30, 0, 0, 0)), dex_bytes,
             compress_type=zipfile.ZIP_DEFLATED)
for name, blob in ((SO_ENTRY, so_bytes), (CXX_ENTRY, cxx_bytes)):
    zi = zipfile.ZipInfo(name, date_time=(2026, 10, 30, 0, 0, 0))
    zi.compress_type = zipfile.ZIP_DEFLATED
    zi.external_attr = 0o644 << 16
    dst.writestr(zi, blob)
dst.close()
src.close()

# 自检
z = zipfile.ZipFile(OUT)
nm = z.namelist()
assert "AndroidManifest.xml" in nm and NEW_DEX in nm and SO_ENTRY in nm and CXX_ENTRY in nm
assert not any(n.startswith("META-INF/") and n.upper().endswith(SIG_EXT) for n in nm)
bad = [n for n in nm if n.upper().endswith((".SF", ".RSA", ".DSA"))]
print(f"完成: {OUT}")
print(f"  条目 {len(nm)}（搬运 {kept}、换 manifest {replaced}、删签名 {skipped}）")
print(f"  新增: {NEW_DEX} / {SO_ENTRY} / {CXX_ENTRY}")
print(f"  残留签名文件: {bad}")
print(f"  大小 {os.path.getsize(OUT)/1024/1024:.1f} MB")
z.close()
