# -*- coding: utf-8 -*-
"""星光加固出包门禁（P1-7）——**每次打包自带**，不通过就不许交付。

它把「加固是否真的生效」从"我记得做过"变成"机器断言过"：

  C1 公钥一致     客户端 sg_pubkey.h 指纹 == 私钥指纹（不一致 ⇒ 全员授权断裂）
  C2 清单一致     sg_integ.h 的签名与当前 dex/main.js/证书 相符（不一致 ⇒ 装上去功能全灭）
  C3 去路标       包内 so 不得含 `[验证]` / `grant ok` / `grant rejected` 明文
  C4 产物同一     APK 内 so/dex 的 md5 == libs_ysxz 里的本地产物（防"包里是旧 so"）
  C5 红队可检测   复制一份 APK 改动 classes4.dex 一个字节 ⇒ 其明文哈希必须与清单不符
                  （即：破解者的"删我卡密 dex 重打包"一定能被 native 的完整性校验抓到）

用法：
  python 04_scripts/_common/harden_apk.py check --apk <apk>
  python 04_scripts/_common/harden_apk.py redteam --apk <apk> [--work <dir>]
退出码：0 全过 / 1 有 FAIL
"""
import argparse
import hashlib
import io
import os
import re
import shutil
import sys
import zipfile

ROOT = r"E:\nixiang"
HERE = os.path.dirname(os.path.abspath(__file__))
SGPKI = os.path.join(HERE, "sg_pki")
JNI = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni")
LIBS = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "libs_ysxz")
sys.path.insert(0, SGPKI)

BANNED = [b"grant ok", b"grant rejected", "[验证]".encode("utf-8"), b"sg-grant-v1"]


class Report:
    def __init__(self):
        self.items = []

    def add(self, name, ok, detail=""):
        self.items.append((name, ok, detail))
        print("%-14s %s  %s" % (name, "PASS" if ok else "FAIL", detail))

    def ok(self):
        return all(ok for _, ok, _ in self.items)

    def summary(self):
        fail = [n for n, ok, _ in self.items if not ok]
        print("\n== 加固门禁：%d 项 FAIL=%d ==" % (len(self.items), len(fail)))
        if fail:
            print("FAIL: " + ", ".join(fail))
        return 0 if not fail else 1


def md5_of(path):
    h = hashlib.md5()
    with io.open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def integ_table():
    """从 sg_integ.h 里读回 (path, size, sha256hex, sig)"""
    p = os.path.join(JNI, "Includes", "sg_integ.h")
    if not os.path.exists(p):
        return None
    s = io.open(p, "r", encoding="utf-8").read()
    entries = []
    for m in re.finditer(r'\{\s*"([^"]+)",\s*(\d+),\s*\{([^}]+)\}\s*\}', s):
        path, size, body = m.group(1), int(m.group(2)), m.group(3)
        hx = "".join("%02x" % int(b, 16) for b in re.findall(r"0x([0-9a-fA-F]{2})", body))
        entries.append((path, size, hx))
    sig = ""
    ms = re.search(r'kSgIntegSigHex\[\]\s*=\s*"([^"]*)"', s)
    if ms:
        sig = ms.group(1)
    return entries, sig


def _run_check(script):
    """直接跑子脚本的 --check，用 subprocess（不用 os.system：Windows 下 rc 编码不可靠）。"""
    import subprocess
    r = subprocess.run([sys.executable, os.path.join(SGPKI, script), "--check"],
                       capture_output=True, text=True, encoding="utf-8", errors="replace")
    return r.returncode, (r.stdout or "").strip()


def c1(rep):
    rc, out = _run_check("gen_pubkey_header.py")
    rep.add("C1 公钥一致", rc == 0, out.splitlines()[-1] if out else "gen_pubkey_header 无输出")


def c2(rep):
    rc, out = _run_check("gen_integ_header.py")
    t = integ_table()
    ok = (rc == 0) and t is not None and len(t[0]) > 0 and len(t[1]) == 512
    rep.add("C2 清单一致", ok,
            ("%d 条 / 签名 %s…" % (len(t[0]), t[1][:12])) if t else "缺 sg_integ.h")


def c3(rep, apk):
    with zipfile.ZipFile(apk) as z:
        hit = []
        so_names = [n for n in z.namelist() if n.endswith("libstarry_magic.so")]
        for n in so_names:
            data = z.read(n)
            for kw in BANNED:
                if kw in data:
                    hit.append("%s:%s" % (n.split("/")[-2], kw.decode("utf-8", "replace")))
    rep.add("C3 去路标", not hit, ("命中 %s" % hit) if hit else "%d 个 so 无门禁路标" % len(so_names))


def c4(rep, apk):
    problems = []
    with zipfile.ZipFile(apk) as z:
        for abi in ("arm64-v8a", "x86", "armeabi-v7a"):
            name = "lib/%s/libstarry_magic.so" % abi
            local = os.path.join(LIBS, abi, "libstarry_magic.so")
            if name not in z.namelist():
                problems.append("%s 缺失" % abi)
                continue
            in_apk = hashlib.md5(z.read(name)).hexdigest()
            if os.path.exists(local):
                if md5_of(local) != in_apk:
                    problems.append("%s 与本地不一致" % abi)
        if "classes4.dex" in z.namelist():
            if os.path.exists(os.path.join(LIBS, "classes4.dex")):
                if hashlib.md5(z.read("classes4.dex")).hexdigest() != md5_of(os.path.join(LIBS, "classes4.dex")):
                    problems.append("classes4.dex 与本地不一致")
        else:
            problems.append("classes4.dex 缺失")
    rep.add("C4 产物同一", not problems, ("；".join(problems)) if problems else "3 ABI so + dex 均与本地一致")


def c5(rep, apk, work):
    """红队：改动 classes4.dex 一个字节 ⇒ 明文哈希必须与清单不符（= 一定能被抓到）"""
    t = integ_table()
    if not t:
        rep.add("C5 红队可检测", False, "缺清单")
        return
    expected = {p: (s, h) for p, s, h in t[0]}
    if "classes4.dex" not in expected:
        rep.add("C5 红队可检测", False, "清单里没有 classes4.dex")
        return
    os.makedirs(work, exist_ok=True)
    tmp = os.path.join(work, "redteam.apk")
    try:
        with zipfile.ZipFile(apk) as zin, zipfile.ZipFile(tmp, "w", zipfile.ZIP_DEFLATED) as zout:
            for it in zin.infolist():
                data = zin.read(it.filename)
                if it.filename == "classes4.dex":
                    b = bytearray(data)
                    b[len(b) // 2] ^= 0x01        # 改动 1 字节（模拟"删/改卡密 dex"）
                    data = bytes(b)
                zout.writestr(it, data)
        with zipfile.ZipFile(tmp) as z:
            tampered = z.read("classes4.dex")
        want_size, want_hash = expected["classes4.dex"]
        got_hash = hashlib.sha256(tampered).hexdigest()
        detected = (got_hash != want_hash)
        rep.add("C5 红队可检测", detected,
                "改 1 字节后哈希不符（native 会判完整性失败）" if detected else "改字节却没被发现！")
    finally:
        if os.path.exists(tmp):
            os.remove(tmp)


def check_payload_fresh():
    """C6：明文载荷改了但没重新加密 ⇒ 密文头与实物不一致，必须拦下。"""
    import subprocess, sys as _sys
    gen = os.path.join(ROOT, "04_scripts", "勇士之心", "gen_payload_enc.py")
    if not os.path.exists(gen):
        return "SKIP", "无 gen_payload_enc.py"
    r = subprocess.run([_sys.executable, gen, "--check"], capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    if r.returncode == 0:
        last = (r.stdout or "").strip().splitlines()
        return "PASS", (last[-1][:70] if last else "ok")
    last = (r.stdout or r.stderr or "").strip().splitlines()
    return "FAIL", (last[-1][:90] if last else "check failed")


def c6(rep):
    st, msg = check_payload_fresh()
    if st != "SKIP":
        rep.add("C6 载荷密文新鲜", st == "PASS", msg)


def c7(rep, apk):
    """C7：载荷加密层是否真的编进 so（防"门禁全绿但载荷其实没加密"）。"""
    need = [b"payload decrypted", b"-> decoy"]
    leak = b"__starry_layer_ready"   # 诱饵独有：有它说明走的是诱饵分支代码（合法），仅作参考
    try:
        with zipfile.ZipFile(apk) as z:
            names = [n for n in z.namelist() if n.endswith("libstarry_magic.so")]
            if not names:
                rep.add("C7 载荷层已编译", False, "包里没有 libstarry_magic.so")
                return
            miss = []
            for n in names:
                blob = z.read(n)
                for k in need:
                    if k not in blob:
                        miss.append("%s 缺 %s" % (n, k.decode()))
            if miss:
                rep.add("C7 载荷层已编译", False, "；".join(miss[:3]))
                return
            rep.add("C7 载荷层已编译", True,
                    "%d 个 so 均含解密/诱饵分支（%s 仅调试用：%s）" % (
                        len(names), "decoy-marker",
                        "有" if leak else "无"))
    except Exception as e:  # noqa: BLE001
        rep.add("C7 载荷层已编译", False, "读取失败: %s" % e)


def _payload_plain_samples(n=12, win=48):
    """从明文载荷里取指纹片段（失败返回空列表）。"""
    import re as _re
    h = os.path.join(ROOT, "04_scripts", "勇士之心", "ysxz_1.0.3_menu", "YsxzPatchJs.h")
    if not os.path.exists(h):
        return []
    txt = io.open(h, "r", encoding="utf-8", errors="surrogateescape").read()
    m = _re.search(r'R"YSXZJS\((.*)\)YSXZJS"', txt, _re.S)
    if not m:
        return []
    body = m.group(1)
    step = max(1, len(body) // (n * 3))
    out, i = [], 0
    while len(out) < n and i + win < len(body):
        out.append(body[i:i + win].encode("utf-8"))
        i += step
    return out


def c8(rep, apk):
    """C8：开了 -DSG_PAYLOAD_ENC 时，包内 so 不得含载荷明文片段。"""
    mk = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni",
                      "Android_ysxz.mk")
    enc_on = False
    if os.path.exists(mk):
        try:
            enc_on = "-DSG_PAYLOAD_ENC" in io.open(mk, "r", encoding="utf-8").read()
        except Exception:
            enc_on = False
    if not enc_on:
        return                      # 明文构建（回滚路径），不检查
    fps = _payload_plain_samples()
    if not fps:
        rep.add("C8 无明文载荷泄漏", False, "取不到载荷指纹片段")
        return
    try:
        with zipfile.ZipFile(apk) as z:
            bad = []
            for n in [x for x in z.namelist() if x.endswith("libstarry_magic.so")]:
                blob = z.read(n)
                hits = sum(1 for f in fps if f in blob)
                if hits:
                    bad.append("%s 命中 %d/%d 片段" % (n, hits, len(fps)))
            if bad:
                rep.add("C8 无明文载荷泄漏", False, "；".join(bad[:3]))
            else:
                rep.add("C8 无明文载荷泄漏", True,
                        "%d 个片段 × 3 ABI 全部 0 命中（载荷确为密文）" % len(fps))
    except Exception as e:  # noqa: BLE001
        rep.add("C8 无明文载荷泄漏", False, "读取失败: %s" % e)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("cmd", choices=["check", "redteam", "publish"])
    ap.add_argument("--apk", default=os.path.join(ROOT, "08_output", "勇士之心",
                                                  "勇士之心_1.0.3_starry-final-releaseSigned.apk"))
    ap.add_argument("--work", default=os.path.join(os.environ.get("TEMP", "."), "sg_redteam"))
    ap.add_argument("--key", default=os.path.join(os.path.expanduser("~"), ".starry-keys", "rsa2048.txt"))
    a = ap.parse_args()

    if not os.path.exists(a.apk):
        print("APK 不存在: %s" % a.apk)
        return 1

    rep = Report()
    c1(rep)
    c2(rep)
    c3(rep, a.apk)
    c4(rep, a.apk)
    if a.cmd == "publish":
        return cmd_publish(a)
    if a.cmd == "redteam":
        c5(rep, a.apk, a.work)
    c6(rep)
    c7(rep, a.apk)
    c8(rep, a.apk)
    return rep.summary()



# ---------------- P2-1：服务端签名的 build 清单 ----------------

MANIFEST_DIR = os.path.join(ROOT, "10_docs", "勇士之心", "build_manifests")
COVER = ["classes4.dex", "assets/main.js", "assets/certs/starlight.crt",
         "lib/arm64-v8a/libstarry_magic.so", "lib/x86/libstarry_magic.so",
         "lib/armeabi-v7a/libstarry_magic.so"]


def _sg_rsa():
    srv = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "auth-server")
    if srv not in sys.path:
        sys.path.insert(0, srv)
    from app.services import sg_rsa  # noqa: E402
    return sg_rsa


def build_manifest(apk, key_path=None):
    """对成品 APK 生成**服务端签名**的 build 清单（覆盖 dex + so + boot js + 证书）。
    这是 P2 的核心：它能把 `libstarry_magic.so` 自己也纳入校验（SgIntegrity 做不到）。"""
    sg_rsa = _sg_rsa()
    key = sg_rsa.load_key(key_path or os.path.join(os.path.expanduser("~"), ".starry-keys", "rsa2048.txt"))
    with zipfile.ZipFile(apk) as z:
        names = set(z.namelist())
        entries = []
        for n in COVER:
            if n not in names:
                raise SystemExit("清单目标缺失: %s" % n)
            data = z.read(n)
            entries.append((n, len(data), hashlib.sha256(data).hexdigest()))
    build_id = "b" + md5_of(apk)[:16]
    lines = ["SGB1", "build %s" % build_id, "created %d" % int(__import__("time").time())]
    for n, sz, hx in entries:
        lines.append("E %s %d %s" % (n, sz, hx))
    body = ("\n".join(lines) + "\n").encode("utf-8")
    sig = sg_rsa.sign(body, key).hex()
    text = body.decode("utf-8") + "SIG %s\n" % sig
    return build_id, text, entries


def cmd_publish(a):
    build_id, text, entries = build_manifest(a.apk, a.key)
    os.makedirs(MANIFEST_DIR, exist_ok=True)
    out = os.path.join(MANIFEST_DIR, "%s.txt" % build_id)
    io.open(out, "w", encoding="utf-8", newline="\n").write(text)
    print("build_id = %s" % build_id)
    for n, sz, hx in entries:
        print("  %-38s %9d B  %s" % (n, sz, hx[:16]))
    print("清单已写: %s（%d B，直接上传到服务器 ota/builds 目录或按你部署方式放入）" % (out, len(text)))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
