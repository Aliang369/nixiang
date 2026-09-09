#!/usr/bin/env python3.11
# 路线 A 外科 zip 组装：原包逐条目原样复制，仅替换 2 个 entry，最后 uber 签名
#
# 用法: python3.11 routeA_build_apk.py <原包.apk>
# 产物: 07_patched/月圆之夜/routeA/*_routeA_unsigned.apk
#       (随后用 uber-apk-signer 签名输出 08_output/月圆之夜/)
import re, sys, os, zipfile, shutil, subprocess

GAME = "月圆之夜"
ROUTEA = f"/Users/aliang/Documents/nixiang/07_patched/{GAME}/routeA"
NEW_RAW = f"{ROUTEA}/final.rawfile"
MF_ENTRY = "assets/yoo/DefaultPackage/PackageManifest_DefaultPackage_1.0.055.bytes"
HASH_ENTRY = "assets/yoo/DefaultPackage/PackageManifest_DefaultPackage_1.0.055.hash"
PL_ENTRY = "assets/yoo/PackageList.json"
NEW_MANIFEST = None                                       # 路线B2: 不改 manifest
NEW_DEX = "/tmp/classes_new.dex"                          # apktool 重编译的 classes.dex (含 starryRun)
DEX_ENTRY = "classes.dex"
UBER = "/Users/aliang/Documents/nixiang/01_tools/uber-apk-signer.jar"
OUT_DIR = f"/Users/aliang/Documents/nixiang/08_output/{GAME}"


def build(src_apk: str):
    assert os.path.exists(NEW_RAW), "缺少补丁 rawfile"
    new_raw = open(NEW_RAW, "rb").read()
    new_mf = open(f"{ROUTEA}/PackageManifest_DefaultPackage_1.0.055.bytes", "rb").read()
    new_hash = open(f"{ROUTEA}/PackageManifest_DefaultPackage_1.0.055.hash").read().strip()
    new_dex = open(NEW_DEX, "rb").read()
    import hashlib
    n_md5 = hashlib.md5(new_raw).hexdigest()
    new_raw_entry = f"assets/yoo/DefaultPackage/{n_md5}.rawfile"
    base = os.path.splitext(os.path.basename(src_apk))[0]
    dst = f"{ROUTEA}/{base}_routeB_unsigned.apk"

    zin = zipfile.ZipFile(src_apk)
    names = zin.namelist()
    for need in (MF_ENTRY, HASH_ENTRY, PL_ENTRY, DEX_ENTRY):
        assert need in names, f"原包缺少条目 {need}"

    pl = zin.read(PL_ENTRY)
    pl_new, n = re.subn(rb'("NeedCheckPatch"\s*:\s*)true', rb"\g<1>false", pl)
    assert n >= 1, f"PackageList.json 未命中 NeedCheckPatch ({n})"
    print(f"[PackageList] NeedCheckPatch true->false ×{n}")

    zout = zipfile.ZipFile(dst, "w")
    copied = added = replaced = 0
    for info in zin.infolist():
        nm = info.filename
        if nm.startswith("META-INF/"):
            continue
        if nm == "assets/yoo/DefaultPackage/ccdda85485ad592a3c5deac4b801609c.rawfile":
            data, comp, nm = new_raw, zipfile.ZIP_STORED, new_raw_entry  # 仅替换热更容器, STORED
            replaced += 1
        elif nm == MF_ENTRY:
            data, comp = new_mf, info.compress_type
            replaced += 1
        elif nm == HASH_ENTRY:
            data, comp = new_hash.encode(), info.compress_type
            replaced += 1
        elif nm == PL_ENTRY:
            data, comp = pl_new, info.compress_type
            replaced += 1
        elif nm == DEX_ENTRY:
            data, comp = new_dex, info.compress_type
            replaced += 1
        else:
            data = zin.read(nm)
            comp = info.compress_type
            copied += 1
        ni = zipfile.ZipInfo(nm, date_time=info.date_time)
        ni.compress_type = comp
        ni.external_attr = info.external_attr
        ni.internal_attr = info.internal_attr
        ni.create_system = info.create_system
        zout.writestr(ni, data)
    zout.close()
    zin.close()
    print(f"[assemble] {dst} ({os.path.getsize(dst):,}B): 复制 {copied}, 替换 {replaced}")
    assert replaced == 5
    return dst


def sign(unsigned_apk: str):
    os.makedirs(OUT_DIR, exist_ok=True)
    print("==> uber-apk-signer (zipalign + v1/v2/v3)")
    r = subprocess.run(
        ["java", "-jar", UBER, "-a", unsigned_apk, "-o", OUT_DIR, "--allowResign"],
        capture_output=True, text=True)
    tail = "\n".join((r.stdout + r.stderr).splitlines()[-12:])
    print(tail)
    signed = [f for f in os.listdir(OUT_DIR)
              if f.endswith("Signed.apk") and base in f]
    if not signed:
        signed = [f for f in sorted(os.listdir(OUT_DIR)) if f.endswith(".apk")]
    assert signed, "未找到签名产物:\n" + tail
    out = os.path.join(OUT_DIR, signed[0])
    print(f"[sign] {out}")
    return out


if __name__ == "__main__":
    src = sys.argv[1]
    assert os.path.exists(src), f"原包不存在: {src}"
    base = os.path.splitext(os.path.basename(src))[0]
    u = build(src)
    sign(u)
