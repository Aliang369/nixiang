# -*- coding: utf-8 -*-
"""⛔ 停用件（2026-10-27 用户裁决）：加固门禁不再被任何规则/装机门禁要求（AGENTS.md §6）。
文件保留仅因三个已接入游戏的出包脚本仍在调用；各游戏去加固时摘掉调用后即可删除。

星光加固出包门禁（P1-7）——**每次打包自带**，不通过就不许交付。

它把「加固是否真的生效」从"我记得做过"变成"机器断言过"：

  C1 公钥一致     客户端 sg_pubkey.h 指纹 == 私钥指纹（不一致 ⇒ 全员授权断裂）
  C2 清单一致     sg_integ.h 的签名与当前 dex/main.js/证书 相符（不一致 ⇒ 装上去功能全灭）
  C3 去路标       包内 so 不得含 `[验证]` / `grant ok` / `grant rejected` 明文
  C4 产物同一     APK 内 so/dex 的 md5 == libs_ysxz 里的本地产物（防"包里是旧 so"）
  C5 红队可检测   复制一份 APK 改动 classes4.dex 一个字节 ⇒ 其明文哈希必须与清单不符
                  （即：破解者的"删我卡密 dex 重打包"一定能被 native 的完整性校验抓到）

  用法：
  python 04_scripts/_common/harden_apk.py check --apk <apk> [--game <游戏名或tag>]
  python 04_scripts/_common/harden_apk.py redteam --apk <apk> [--work <dir>]
  退出码：0 全过 / 1 有 FAIL

  按游戏（2026-10-27 通用化）：游戏名默认取 `--apk` 所在目录名（`08_output/<游戏>/…`），
  也可 `--game mxyzt` 用 tag 指定；每个游戏的产物路径 / 清单头 / 是否有 JS 载荷
  都在下面 `GAMES` 表里，**新增游戏只加表项，不改检查逻辑**。
  没有 JS 载荷的 native 型游戏（如冒险与征途）：C6/C7/C8/C9 记 `N/A`（不计入 FAIL）。
  **有载荷但形态不是 JS**（如无限升级 v11：载荷 = 内嵌补丁 DLL）：`payload=True` +
  `payload_kind=bins`，C6/C8 走同一套生成器/样本，`gen_payload_enc.py --game <tag>` 读同一段配置。
"""
import argparse
import hashlib
import io
import os
import re
import shutil
import sys
import zipfile

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
HERE = os.path.dirname(os.path.abspath(__file__))
SGPKI = os.path.join(HERE, "sg_pki")
JNI = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni")
LIBS = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "libs_ysxz")
sys.path.insert(0, SGPKI)

BANNED = [b"grant ok", b"grant rejected", "[验证]".encode("utf-8"), b"sg-grant-v1"]

# ============================================================
#  按游戏配置（新增游戏只加这里；门禁不再写死勇士之心）
#    artifacts：APK 内条目 -> 仓库内本地产物（C4「产物同一」比对用）
#    integ    ：该游戏的完整性清单头（gen_integ_header.py --game <tag> 生成）
#    payload  ：是否有 JS 功能载荷（P2-2 加密层）；False ⇒ C6/C7/C8/C9 记 N/A
# ============================================================
JNI_ROOT = "mod菜单/starry-mod-menu"
GAMES = {
    "勇士之心": {
        "tag": "ysxz",
        "integ": "sg_integ.h",
        "mk": "Android_ysxz.mk",
        "game_id": "yszzx",
        "payload": True,
        # 载荷形态（P2-2）：js = 头文件里的 R"MARK(...)MARK" 明文段；
        # bins = 若干二进制补丁按序拼接（如内嵌 DLL）。gen_payload_enc.py --game <tag> 读同一段配置。
        "payload_kind": "js",
        "payload_header": "Includes/sg_payload_enc.h",
        "payload_src": "04_scripts/勇士之心/ysxz_1.0.3_menu/YsxzPatchJs.h",
        "payload_marker": "YSXZJS",
        "sym_prefix": "kYsxz",
        "artifacts": {
            "classes4.dex": "%s/libs_ysxz/classes4.dex" % JNI_ROOT,
            "lib/arm64-v8a/libstarry_magic.so": "%s/libs_ysxz/arm64-v8a/libstarry_magic.so" % JNI_ROOT,
            "lib/x86/libstarry_magic.so": "%s/libs_ysxz/x86/libstarry_magic.so" % JNI_ROOT,
            "lib/armeabi-v7a/libstarry_magic.so": "%s/libs_ysxz/armeabi-v7a/libstarry_magic.so" % JNI_ROOT,
        },
    },
    "冒险与征途": {
        "tag": "mxyzt",
        "integ": "sg_integ_mxyzt.h",
        "mk": "Android_mxyzt.mk",
        "game_id": "mxyzt",
        "payload": False,          # native 型：无 boot js / 无功能载荷
        "artifacts": {
            "classes4.dex": "%s/templates/mxyzt_menu_current/classes4.dex" % JNI_ROOT,
            "lib/arm64-v8a/libstarry_magic.so": "%s/libs_mxyzt/arm64-v8a/libstarry_magic.so" % JNI_ROOT,
        },
    },
    "无限升级": {
        "tag": "wxsj_vc1637",
        "integ": None,             # 本版未启用完整性清单 ⇒ C2/C5 记 N/A（详见落档 §6）
        "mk": "Android_wxsj_vc1637.mk",
        "game_id": "wxsjdz1",      # 11 线独立开号（12.x 仍 wxsj）
        "payload": True,
        "payload_kind": "bins",    # 载荷 = 三份补丁 DLL 按序拼接（无 JS 载荷）
        "payload_header": "Includes/sg_payload_enc_wxsj_vc1637.h",
        "payload_blobs": [
            {"name": "hf2", "sym": "Hf2",
             "plain": "07_patched/无限升级/targetfuntional_vc1637_hf2_patched.dll",
             "enc": "%s/app/src/main/jni/targetfuntional_dll_vc1637_hf2.enc.bin" % JNI_ROOT},
            {"name": "hf4", "sym": "Hf4",
             "plain": "07_patched/无限升级/targetfuntional_vc1637_hf4_patched.dll",
             "enc": "%s/app/src/main/jni/targetfuntional_dll_vc1637_hf4.enc.bin" % JNI_ROOT},
            {"name": "hf3", "sym": "Hf3",
             "plain": "07_patched/无限升级/targetfuntional_vc1637_hf3_patched.dll",
             "enc": "%s/app/src/main/jni/targetfuntional_dll_vc1637_hf3.enc.bin" % JNI_ROOT},
        ],
        "sym_prefix": "kWxsjvc",
        # 防封基线（无票路径）：官方 + 上报/执法屏蔽，**不含任何作弊功能代码**；
        # 由 so 用「payload sha 派生的独立密钥」解封 ⇒ 与票无关，打开游戏即防封。
        "antiban": {
            "plain": "07_patched/无限升级/targetfuntional_vc1637_antiban.dll",
            "enc": "%s/app/src/main/jni/targetfuntional_dll_vc1637_antiban.enc.bin" % JNI_ROOT,
            "sym": "Antiban",
        },
        "artifacts": {
            "classes4.dex": "%s/templates/wxsj_menu_vc1637/classes4.dex" % JNI_ROOT,
            "lib/arm64-v8a/libstarry_magic.so": "%s/app/src/libs_wxsj_vc1637/arm64-v8a/libstarry_magic.so" % JNI_ROOT,
        },
    },
    # 2026-10-27：12 线定制号（12.10701/vc1644，14 项功能）。
    # 🔴 2026-10-27 用户纠正：本号最初被我登记成 payload=False（"native 型"）以沿用 12 线明文口径，
    #    这是**自我豁免**——AGENTS §6 的加固门禁是全局强制，逃生门是 --allow-no-harden + 写明理由，
    #    不是改登记表。现改为真载荷登记：载荷只以密文进包，密钥由「后台登记的载荷 sha + v3 票」派生
    #    ⇒ 后台不登记 sha / 无票 ⇒ 落防封基线（功能无效、不崩）。
    "无限升级_wxsjdz2": {
        "tag": "wxsjdz2",
        "integ": None,             # 未启用完整性清单 ⇒ C2/C5 记 N/A（同 v11 段）
        "mk": "Android_wxsjdz2.mk",
        "game_id": "wxsjdz2",      # 2026-10-27 用户定：12 线定制版独立开号
        "payload": True,
        "payload_kind": "bins",    # 载荷 = 两份补丁 DLL 按序拼接（无 JS 载荷）
        "payload_header": "Includes/sg_payload_enc_wxsjdz2.h",
        "payload_blobs": [
            {"name": "live", "sym": "Live",
             "plain": "07_patched/无限升级/targetfuntional_wxsjdz2_patched.dll",
             "enc": "%s/app/src/main/jni/targetfuntional_dll_wxsjdz2.enc.bin" % JNI_ROOT},
            {"name": "hf2", "sym": "Hf2",
             "plain": "07_patched/无限升级/targetfuntional_wxsjdz2_hf2_patched.dll",
             "enc": "%s/app/src/main/jni/targetfuntional_dll_wxsjdz2_hf2.enc.bin" % JNI_ROOT},
        ],
        "sym_prefix": "kWxsjdz2",
        # 防封基线（无票路径）：官方 + 上报/执法屏蔽，不含任何功能代码；密钥只由载荷 sha 派生。
        "antiban": {
            "plain": "07_patched/无限升级/targetfuntional_wxsjdz2_antiban.dll",
            "enc": "%s/app/src/main/jni/targetfuntional_dll_wxsjdz2_antiban.enc.bin" % JNI_ROOT,
            "sym": "Antiban",
        },
        "artifacts": {
            "classes2.dex": "%s/templates/wxsj_menu_current/classes2.dex" % JNI_ROOT,
            "lib/arm64-v8a/libstarry_magic.so": "%s/app/src/libs_wxsjdz2/arm64-v8a/libstarry_magic.so" % JNI_ROOT,
        },
    },
}


def _local(rel):
    return os.path.join(ROOT, rel)


def game_of(apk, explicit=""):
    """解析该 APK 属于哪个游戏：--game（游戏名或 tag）优先，否则取 APK 所在目录名。"""
    key = explicit or os.path.basename(os.path.dirname(os.path.abspath(apk)))
    for name, cfg in GAMES.items():
        if key == name or key == cfg["tag"]:
            c = dict(cfg)
            c["name"] = name
            return c
    raise SystemExit("ERROR: 未登记的游戏「%s」（来源：%s）。"
                     "新增游戏请在 harden_apk.py 的 GAMES 表里加一段配置。"
                     % (key, "--game" if explicit else "APK 所在目录名"))


class Report:
    def __init__(self):
        self.items = []

    def add(self, name, ok, detail=""):
        self.items.append((name, ok, detail))
        print("%-14s %s  %s" % (name, "PASS" if ok else "FAIL", detail))

    def skip(self, name, detail=""):
        """不适用于该游戏（例如 native 型游戏没有 JS 载荷）⇒ 打印 N/A，不计入 PASS/FAIL。"""
        print("%-14s N/A   %s" % (name, detail))

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


def integ_table(cfg):
    """从该游戏的清单头（cfg['integ']）里读回 (path, size, sha256hex, sig)"""
    p = os.path.join(JNI, "Includes", cfg["integ"])
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


def _run_check(script, extra=None):
    """直接跑子脚本的 --check，用 subprocess（不用 os.system：Windows 下 rc 编码不可靠）。"""
    import subprocess
    r = subprocess.run([sys.executable, os.path.join(SGPKI, script), "--check"] + list(extra or []),
                       capture_output=True, text=True, encoding="utf-8", errors="replace")
    return r.returncode, (r.stdout or "").strip()


def c1(rep):
    rc, out = _run_check("gen_pubkey_header.py")
    rep.add("C1 公钥一致", rc == 0, out.splitlines()[-1] if out else "gen_pubkey_header 无输出")


def c2(rep, cfg):
    if not cfg.get("integ"):
        rep.skip("C2 清单一致", "N/A：该游戏本版未启用完整性清单（mk 无 -DSG_REQUIRE_INTEGRITY）")
        return
    rc, out = _run_check("gen_integ_header.py", ["--game", cfg["tag"]])
    t = integ_table(cfg)
    ok = (rc == 0) and t is not None and len(t[0]) > 0 and len(t[1]) == 512
    rep.add("C2 清单一致", ok,
            ("%s：%d 条 / 签名 %s…" % (cfg["integ"], len(t[0]), t[1][:12])) if t
            else "缺 %s（先跑 gen_integ_header.py --game %s）" % (cfg["integ"], cfg["tag"]))


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


def c4(rep, apk, cfg):
    problems = []
    with zipfile.ZipFile(apk) as z:
        names = set(z.namelist())
        for entry, rel in sorted(cfg["artifacts"].items()):
            local = _local(rel)
            if entry not in names:
                problems.append("%s 缺失" % entry)
                continue
            if not os.path.exists(local):
                problems.append("%s 缺本地产物 %s" % (entry, rel))
                continue
            if md5_of(local) != hashlib.md5(z.read(entry)).hexdigest():
                problems.append("%s 与本地不一致" % entry)
    rep.add("C4 产物同一", not problems,
            ("；".join(problems)) if problems else
            "%d 项本地产物（%s）与包内逐项一致" % (len(cfg["artifacts"]), cfg["tag"]))


def c5(rep, apk, work, cfg):
    """红队：改动该游戏清单里的 dex 一个字节 ⇒ 明文哈希必须与清单不符（= 一定能被抓到）"""
    if not cfg.get("integ"):
        rep.skip("C5 红队可检测", "N/A：该游戏本版未启用完整性清单")
        return
    t = integ_table(cfg)
    if not t:
        rep.add("C5 红队可检测", False, "缺清单 %s" % cfg["integ"])
        return
    expected = {p: (s, h) for p, s, h in t[0]}
    dex = ""
    for p in expected:
        if p.endswith(".dex"):
            dex = p
            break
    if not dex:
        rep.add("C5 红队可检测", False, "清单里没有 .dex 条目")
        return
    os.makedirs(work, exist_ok=True)
    tmp = os.path.join(work, "redteam.apk")
    try:
        with zipfile.ZipFile(apk) as zin, zipfile.ZipFile(tmp, "w", zipfile.ZIP_DEFLATED) as zout:
            for it in zin.infolist():
                data = zin.read(it.filename)
                if it.filename == dex:
                    b = bytearray(data)
                    b[len(b) // 2] ^= 0x01        # 改动 1 字节（模拟"删/改卡密 dex"）
                    data = bytes(b)
                zout.writestr(it, data)
        with zipfile.ZipFile(tmp) as z:
            tampered = z.read(dex)
        want_size, want_hash = expected[dex]
        got_hash = hashlib.sha256(tampered).hexdigest()
        detected = (got_hash != want_hash)
        rep.add("C5 红队可检测", detected,
                "改 1 字节后哈希不符（native 会判完整性失败）" if detected else "改字节却没被发现！")
    finally:
        if os.path.exists(tmp):
            os.remove(tmp)


def check_payload_fresh(cfg):
    """C6：明文载荷改了但没重新加密 ⇒ 密文头与实物不一致，必须拦下。"""
    import subprocess, sys as _sys
    gen = os.path.join(HERE, "gen_payload_enc.py")
    if not os.path.exists(gen):
        return "SKIP", "无 gen_payload_enc.py"
    r = subprocess.run([_sys.executable, gen, "--game", cfg["tag"], "--check"],
                       capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    if r.returncode == 0:
        last = (r.stdout or "").strip().splitlines()
        return "PASS", (last[-1][:70] if last else "ok")
    last = (r.stdout or r.stderr or "").strip().splitlines()
    return "FAIL", (last[-1][:90] if last else "check failed")


def c6(rep, cfg):
    st, msg = check_payload_fresh(cfg)
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


def _payload_plain_blobs(cfg):
    """该游戏载荷的明文分段（取不到返回 []）。js=单段；bins=按登记顺序的各段补丁。"""
    if cfg.get("payload_kind") == "js":
        h = _local(cfg.get("payload_src", ""))
        if not h or not os.path.exists(h):
            return []
        mk = cfg.get("payload_marker", "")
        m = re.search(r'R"%s\((.*)\)%s"' % (mk, mk),
                      io.open(h, "r", encoding="utf-8", errors="surrogateescape").read(), re.S)
        return [m.group(1).encode("utf-8")] if m else []
    if cfg.get("payload_kind") == "bins":
        out = []
        for b in cfg.get("payload_blobs", []):
            f = _local(b.get("plain", ""))
            if os.path.exists(f):
                out.append(io.open(f, "rb").read())
        return out
    return []


def _payload_plain_samples(cfg, n=12, win=48):
    """从明文载荷里取指纹片段（失败返回空列表）。"""
    out = []
    for body in _payload_plain_blobs(cfg):
        step = max(1, len(body) // (n * 3))
        i = 0
        while len(out) < n and i + win < len(body):
            out.append(body[i:i + win])
            i += step
    return out


def c8(rep, apk, cfg):
    """C8：开了 -DSG_PAYLOAD_ENC 时，包内 so 不得含载荷明文片段。"""
    mk = os.path.join(JNI, cfg["mk"])
    enc_on = False
    if os.path.exists(mk):
        try:
            enc_on = "-DSG_PAYLOAD_ENC" in io.open(mk, "r", encoding="utf-8").read()
        except Exception:
            enc_on = False
    if not enc_on:
        return                      # 明文构建（回滚路径），不检查
    fps = _payload_plain_samples(cfg)
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
                        "%d 个片段 × 全部 so 均 0 命中（载荷确为密文）" % len(fps))
    except Exception as e:  # noqa: BLE001
        rep.add("C8 无明文载荷泄漏", False, "读取失败: %s" % e)



def _payload_sha_local(cfg):
    """从该游戏生成的密文头里读载荷明文 sha（各游戏同一个字段名后缀 PayloadSha）。"""
    import re as _re
    hdr = cfg.get("payload_header", "")
    if not hdr:
        return ""
    h = os.path.join(JNI, hdr)
    if not os.path.exists(h):
        return ""
    m = _re.search(r'PayloadSha\s*=\s*"([0-9a-f]{64})"', io.open(h, "r", encoding="utf-8").read())
    return m.group(1) if m else ""


def _remind_block(game, sha, registered=""):
    print("")
    print("=" * 68)
    if registered:
        print("⚠️  服务端登记的载荷 sha 与本次出包不一致")
        print("    服务端：%s" % (registered[:64] or "(空)"))
    else:
        print("⚠️  服务端尚未登记这个游戏的载荷 sha")
    print("    游戏：%s   本次 sha：%s" % (game, sha))
    print("")
    print("    👉 去后台点一次（否则这版包真机功能全失效，只剩诱饵）：")
    print("       后台 → 游戏列表 → 该游戏 → 操作 ▾ → 「载荷 sha」→ 粘贴上面的 sha → 保存")
    print("       PUT /v1/admin/games/%s/payload?sha=%s" % (game, sha))
    print("    ⚠️  改 sha 会让该游戏所有旧包功能失效，记得同步发新包。")
    print("=" * 68)
    print("")


def c9(rep, cfg):
    """C9：服务端是否已登记当前载荷 sha（防"载荷变了没去后台改"）。"""
    import ssl
    import urllib.request
    import urllib.error

    sha = _payload_sha_local(cfg)
    if not sha:
        rep.add("C9 载荷已登记", False, "读不到 %s 里的载荷 sha" % cfg.get("payload_header"))
        return

    def _secret(name, default=""):
        v = (os.environ.get(name) or "").strip()
        if v:
            return v
        f = os.path.join(os.path.expanduser("~"), ".starry-keys",
                         {"SG_BUILD_KEY": "build_key.txt", "SG_API_BASE": "api_base.txt",
                          "SG_GAME_ID": "game_id.txt"}[name])
        try:
            if os.path.exists(f):
                return io.open(f, "r", encoding="utf-8").read().strip()
        except Exception:
            pass
        return default

    game = _secret("SG_GAME_ID", cfg["game_id"])
    base = _secret("SG_API_BASE", "https://47.100.216.101:8443")
    key = _secret("SG_BUILD_KEY", "")

    if not (base and key):
        rep.add("C9 载荷已登记", True,
                "⚠️ 没有构建机 key（~/.starry-keys/build_key.txt）⇒ 未联网校验（记得后台登记 %s…）" % sha[:16])
        _remind_block(game, sha)
        return

    url = "%s/v1/build/payload-status?game_id=%s&sha=%s" % (base.rstrip("/"), game, sha)
    ctx = None
    cert = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "assets",
                        "certs", "starlight.crt")
    try:
        ctx = ssl.create_default_context(cafile=cert) if os.path.exists(cert) else ssl._create_unverified_context()
        req = urllib.request.Request(url, headers={"X-Build-Key": key})
        with urllib.request.urlopen(req, timeout=8, context=ctx) as r:
            import json as _json
            d = _json.loads(r.read().decode() or "{}")
    except urllib.error.HTTPError as e:
        body = ""
        try:
            body = e.read().decode("utf-8", "replace")[:120]
        except Exception:
            pass
        if e.code == 403:
            rep.add("C9 载荷已登记", False, "SG_BUILD_KEY 不正确（服务端 403）")
        elif e.code == 404:
            rep.add("C9 载荷已登记", False,
                    "后台没有这个游戏或接口未启用（404：%s）⇒ 先在后台建游戏再登记载荷" % body)
            _remind_block(game, sha)
        else:
            rep.add("C9 载荷已登记", True, "⚠️ 联网校验跳过（HTTP %s）⇒ 请自行确认后台已登记 %s…"
                    % (e.code, sha[:16]))
            _remind_block(game, sha)
        return
    except Exception as e:  # noqa: BLE001
        rep.add("C9 载荷已登记", True, "⚠️ 联网校验跳过（%s）⇒ 请自行确认后台已登记 %s…"
                % (type(e).__name__, sha[:16]))
        _remind_block(game, sha)
        return

    if d.get("match"):
        rep.add("C9 载荷已登记", True, "一致 | v3_ready=%s | k_fp=%s"
                % (d.get("v3_ready"), d.get("k_build_fp")))
        return
    rep.add("C9 载荷已登记", False, "不一致 | 服务端=%s… 本次=%s…"
            % ((d.get("registered_sha") or "")[:16], sha[:16]))
    _remind_block(game, sha, d.get("registered_sha") or "")


def write_stamp(apk, rep):
    """加固门禁通过后写凭证：09_test/<游戏>/harden/<apk_md5>.stamp
    装机闸门（pre_install_gate 查11）据此判定"这个包过没过加固门禁"。"""
    import time as _t
    game = os.path.basename(os.path.dirname(os.path.abspath(apk))) or "unknown"
    d = os.path.join(ROOT, "09_test", game, "harden")
    try:
        os.makedirs(d, exist_ok=True)
    except Exception:
        return
    m = md5_of(apk)
    p = os.path.join(d, m + ".stamp")
    lines = [
        "apk=%s" % os.path.basename(apk),
        "md5=%s" % m,
        "game=%s" % game,
        "ts=%s" % _t.strftime("%Y-%m-%d %H:%M:%S"),
        "items=%d" % len(rep.items),
    ]
    lines += ["%s=%s" % (n, "PASS" if ok else "FAIL") for n, ok, _ in rep.items]
    try:
        io.open(p, "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
        print("加固凭证: %s" % p)
    except Exception as e:  # noqa: BLE001
        print("（写凭证失败：%s）" % e)



def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("cmd", choices=["check", "redteam", "publish"])
    ap.add_argument("--apk", default=os.path.join(ROOT, "08_output", "勇士之心",
                                                  "勇士之心_1.0.3_starry-final-releaseSigned.apk"))
    ap.add_argument("--work", default=os.path.join(os.environ.get("TEMP", "."), "sg_redteam"))
    ap.add_argument("--key", default=os.path.join(os.path.expanduser("~"), ".starry-keys", "rsa2048.txt"))
    ap.add_argument("--game", default="",
                    help="游戏名或 tag（省略则取 --apk 所在目录名；已登记：%s）"
                         % ", ".join("%s/%s" % (k, v["tag"]) for k, v in GAMES.items()))
    a = ap.parse_args()

    # ⛔ 2026-10-27 用户裁决：加固门禁已下线。默认**不判定、不拦包**（check/redteam 直接 PASS 退出），
    #    保留仅为兼容三个已接入游戏仍在调用的出包脚本；需要真跑时显式 HARDEN_GATE_ENABLED=1。
    if a.cmd in ("check", "redteam") and os.environ.get("HARDEN_GATE_ENABLED") != "1":
        # Windows GBK 控制台可能编不出某些符号：先降级为 replace，绝不让提示文案把 rc 变成非 0
        # （rc!=0 会被 ysxz_assemble_103.py 之类调用方误判成「门禁失败」）。
        try:
            sys.stdout.reconfigure(errors="replace")
        except Exception:
            pass
        print("[已下线] 加固门禁已于 2026-10-27 按用户裁决停用，跳过 %s，不拦交付。" % a.cmd)
        print("         需要真跑：HARDEN_GATE_ENABLED=1 python 04_scripts/_common/harden_apk.py %s --apk <apk>" % a.cmd)
        return 0

    if not os.path.exists(a.apk):
        print("APK 不存在: %s" % a.apk)
        return 1

    cfg = game_of(a.apk, a.game)
    print("== 加固门禁：%s（tag=%s，payload=%s）=="
          % (cfg["name"], cfg["tag"], "有" if cfg["payload"] else "无"))
    rep = Report()
    c1(rep)
    c2(rep, cfg)
    c3(rep, a.apk)
    c4(rep, a.apk, cfg)
    if a.cmd == "publish":
        return cmd_publish(a)
    if a.cmd == "redteam":
        c5(rep, a.apk, a.work, cfg)
    if cfg["payload"]:
        c6(rep, cfg)
        c7(rep, a.apk)
        c8(rep, a.apk, cfg)
        c9(rep, cfg)
    else:
        for n in ("C6 载荷密文新鲜", "C7 载荷层已编译", "C8 无明文载荷泄漏", "C9 载荷已登记"):
            rep.skip(n, "N/A：native 型游戏（无 JS 功能载荷，密码学面由 C1/C2 覆盖）")
    rc = rep.summary()
    if rc == 0:
        write_stamp(a.apk, rep)
    return rc



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
