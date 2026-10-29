# -*- coding: utf-8 -*-
"""绿梦：时空之声 —— 外科换条目 + zipalign + apksigner 重打（跨平台）

用途
----
热更（Dll bundle / 菜单 dex）后只替换包内**指定条目**，其余条目逐字节不动，
再对齐 + 重签。禁止手工 `zip -d` / `jarsigner`（AGENTS.md §3「机械操作走脚本」）。

用法
----
  python 04_scripts/绿梦/repack_apk_payload.py \
      --src <底包.apk> --out <新包.apk> \
      --replace classes4.dex=<新 classes4.dex> \
      --replace assets/lvmeng/__data=<新 __data> \
      --replace assets/lvmeng/__info=<新 __info> \
      [--ks 01_tools/starry_release.jks] [--alias starry] [--ks-pass starry2026] \
      [--sdk-root <Android Sdk 根>] [--dry-run]

约束
----
* `--out` 必须不等于 `--src`（原包留作逐条比对基线）。
* 除 `--replace` 点名的条目与 `META-INF/*` 外，任何条目的 sha256 变化即 FAIL。
* 退出码：0=成功；1=校验/重打/比对失败；2=用法错。
"""
import argparse
import hashlib
import os
import shutil
import subprocess
import sys
import tempfile
import zipfile

EXIT_USAGE = 2
EXIT_FAIL = 1


def die(msg):
    sys.stderr.write("ERROR: %s\n" % msg)
    sys.exit(EXIT_FAIL)


def find_build_tools(sdk_root=None):
    cands = []
    if sdk_root:
        cands.append(os.path.join(sdk_root, "build-tools"))
    env_sdk = os.environ.get("ANDROID_SDK_ROOT") or os.environ.get("ANDROID_HOME")
    if env_sdk:
        cands.append(os.path.join(env_sdk, "build-tools"))
    local = os.environ.get("LOCALAPPDATA")
    if local:
        cands.append(os.path.join(local, "Android", "Sdk", "build-tools"))
    home = os.path.expanduser("~")
    cands.append(os.path.join(home, "Library", "Android", "sdk", "build-tools"))
    cands.append(os.path.join(home, "Android", "Sdk", "build-tools"))
    for base in cands:
        if not os.path.isdir(base):
            continue
        for ver in sorted(os.listdir(base), reverse=True):
            d = os.path.join(base, ver)
            zl = os.path.join(d, "zipalign.exe" if os.name == "nt" else "zipalign")
            pas = os.path.join(d, "apksigner.bat" if os.name == "nt" else "apksigner")
            if os.path.isfile(zl) and os.path.isfile(pas):
                return zl, pas
    die("找不到 build-tools 里的 zipalign/apksigner（可用 --sdk-root 指定）")


def run(cmd, quiet=False):
    p = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    out = p.stdout.decode("utf-8", "replace")
    if p.returncode != 0 and not quiet:
        sys.stderr.write(out)
    return p.returncode, out


def entry_sha(z, name):
    h = hashlib.sha256()
    with z.open(name) as f:
        while True:
            b = f.read(1 << 20)
            if not b:
                break
            h.update(b)
    return h.hexdigest()


def file_sha(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        while True:
            b = f.read(1 << 20)
            if not b:
                break
            h.update(b)
    return h.hexdigest()


def rebuild(src, out, repl):
    zin = zipfile.ZipFile(src)
    names = zin.namelist()
    missing = [n for n in repl if n not in names]
    if missing:
        die("底包里没有这些条目：%s" % missing)
    with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED, allowZip64=True) as zout:
        for info in zin.infolist():
            n = info.filename
            if n.startswith("META-INF/"):
                continue  # 旧签名整体摘掉
            zi = zipfile.ZipInfo(n, date_time=info.date_time)
            zi.compress_type = info.compress_type
            zi.external_attr = info.external_attr
            zi.create_system = info.create_system
            if n in repl:
                with open(repl[n], "rb") as f:
                    data = f.read()
                zout.writestr(zi, data)
                sys.stdout.write("  [swap] %-40s %8d → %8d B\n" % (n, info.file_size, len(data)))
            else:
                zi.file_size = info.file_size  # STORED 条目必须显式给长度
                with zin.open(n) as fin, zout.open(zi, "w") as fout:
                    shutil.copyfileobj(fin, fout, 1 << 20)
    zin.close()


def verify(src, out, repl):
    za, zb = zipfile.ZipFile(src), zipfile.ZipFile(out)
    na = {n for n in za.namelist() if not n.startswith("META-INF/")}
    nb = {n for n in zb.namelist() if not n.startswith("META-INF/")}
    missing, extra = sorted(na - nb), sorted(nb - na)
    if missing:
        die("新包丢了条目：%s" % missing[:8])
    if extra:
        die("新包多了条目：%s" % extra[:8])
    changed = []
    for n in sorted(na):
        a = file_sha_of_entry(za, n)
        b = file_sha_of_entry(zb, n)
        if a != b:
            changed.append(n)
    unexpected = [n for n in changed if n not in repl]
    if unexpected:
        die("以下条目内容变了但不该变：%s" % unexpected[:8])
    unchanged = [n for n in na if n not in repl]
    if sorted(changed) != sorted(repl.keys()):
        die("点名要换的条目没全换掉：changed=%s" % sorted(changed))
    sys.stdout.write("  entries=%d，除 %s 外全部字节一致（校验 %d 条）\n"
                     % (len(na), sorted(repl), len(unchanged)))
    for n in ("lib/arm64-v8a/libstarry_magic.so", "AndroidManifest.xml",
              "assets/lvmeng/fg_orig.apk", "assets/lvmeng/starry_lvmeng.dll"):
        if n in nb:
            sys.stdout.write("   · %-42s %10d B  sha256 %s\n" % (n, zb.getinfo(n).file_size,
                                                                file_sha_of_entry(zb, n)[:16]))
    for n in sorted(repl):
        sys.stdout.write("   · %-42s %10d B  sha256 %s\n" % (n, zb.getinfo(n).file_size,
                                                            file_sha_of_entry(zb, n)[:16]))


def file_sha_of_entry(z, name):
    h = hashlib.sha256()
    with z.open(name) as f:
        while True:
            b = f.read(1 << 20)
            if not b:
                break
            h.update(b)
    return h.hexdigest()


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--src", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--replace", action="append", default=[],
                    help="条目名=新文件路径，可重复")
    ap.add_argument("--ks", default="")
    ap.add_argument("--alias", default=os.environ.get("STARRY_KS_ALIAS", "starry"))
    ap.add_argument("--ks-pass", default=os.environ.get("STARRY_KS_PASS", "starry2026"))
    ap.add_argument("--sdk-root", default="")
    ap.add_argument("--dry-run", action="store_true")
    a = ap.parse_args()

    repl = {}
    for item in a.replace:
        if "=" not in item:
            sys.stderr.write("ERROR: --replace 需要 条目=路径，实得 %r\n" % item)
            sys.exit(EXIT_USAGE)
        k, v = item.split("=", 1)
        if not os.path.isfile(v):
            die("替换源不存在：%s" % v)
        repl[k] = v
    if not repl:
        sys.stderr.write("ERROR: 至少要一个 --replace\n")
        sys.exit(EXIT_USAGE)
    src = os.path.abspath(a.src)
    out = os.path.abspath(a.out)
    if src == out:
        die("--out 不得等于 --src（底包要留作逐条比对基线）")
    if not os.path.isfile(src):
        die("底包不存在：%s" % src)

    root = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
    ks = a.ks or os.path.join(root, "01_tools", "starry_release.jks")
    if not os.path.isfile(ks):
        die("缺签名库：%s" % ks)

    zl, pas = find_build_tools(a.sdk_root or None)
    sys.stdout.write("== repack_apk_payload ==\n  src = %s\n  out = %s\n  zipalign = %s\n"
                     % (src, out, zl))
    for k in sorted(repl):
        sys.stdout.write("  swap %-40s ← %s (%d B)\n"
                         % (k, repl[k], os.path.getsize(repl[k])))
    if a.dry_run:
        sys.stdout.write("  [dry-run] 未执行\n")
        return 0

    os.makedirs(os.path.dirname(out), exist_ok=True)
    tmpd = tempfile.mkdtemp(prefix="lvmeng_repack_")
    try:
        unsigned = os.path.join(tmpd, "unsigned.apk")
        aligned = os.path.join(tmpd, "aligned.apk")
        sys.stdout.write("== 1/4 换条目 ==\n")
        rebuild(src, unsigned, repl)
        sys.stdout.write("== 2/4 zipalign ==\n")
        rc, o = run([zl, "-f", "-p", "4", unsigned, aligned])
        if rc != 0:
            die("zipalign 失败：%s" % o)
        sys.stdout.write("== 3/4 签名 ==\n")
        rc, o = run([pas, "sign", "--ks", ks, "--ks-key-alias", a.alias,
                     "--ks-pass", "pass:%s" % a.ks_pass,
                     "--key-pass", "pass:%s" % a.ks_pass,
                     "--out", out, aligned])
        if rc != 0:
            die("apksigner 失败：%s" % o)
        sys.stdout.write("== 4/4 校验 ==\n")
        rc, o = run([zl, "-c", "-p", "4", out])
        if rc != 0:
            die("zipalign -c 校验失败：%s" % o)
        sys.stdout.write("  zipalign -c -p 4 : OK\n")
        rc, o = run([pas, "verify", "--print-certs", out])
        for line in o.splitlines()[:4]:
            sys.stdout.write("  " + line + "\n")
        if rc != 0:
            die("apksigner verify 失败")
        verify(src, out, repl)
    finally:
        shutil.rmtree(tmpd, ignore_errors=True)

    sys.stdout.write("-- 产物 --\n")
    sys.stdout.write("  size   = %d B\n" % os.path.getsize(out))
    sys.stdout.write("  sha256 = %s\n" % file_sha(out))
    h = hashlib.md5()
    with open(out, "rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    sys.stdout.write("  md5    = %s\n" % h.hexdigest())
    return 0


if __name__ == "__main__":
    sys.exit(main())
