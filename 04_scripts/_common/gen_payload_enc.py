# -*- coding: utf-8 -*-
"""星光加固 · 载荷加密（P2-2 第一层）——**跨游戏唯一实现**。

以前只有勇士之心一份（`04_scripts/勇士之心/gen_payload_enc.py`，路径/符号名全硬编码），
第二个游戏接入时必然要复制粘贴 ⇒ 走样。本文件按 `harden_apk.py::GAMES` 登记表工作：
`--game <tag>` 决定「载荷从哪读、密文写哪、符号叫什么」，算法一条不改。

机制（与 `jni/Includes/SgPayload.h` 严格对称）：
    K_build = HMAC-SHA256(master, "sgpay-build|" + sha256(载荷))
    ct      = 载荷 XOR keystream(K_build)          keystream = HMAC(K_build,"sgp1"||ctr_be32)×32B
    mac     = HMAC-SHA256(K_build, "sgm1" | ct)    ← Encrypt-then-MAC
    K_build **不写进任何头文件**：出包机存 `~/.starry-keys/payload/<sha>.key`，
    服务器用同一份 master 算同一个 K_build，再由 v3 票里的 kx 交给客户端。

两种载荷形态：
    kind=js    载荷 = 头文件里的 `R"MARK(...)MARK"` 明文 JS（勇士之心）
    kind=bins  载荷 = 若干二进制补丁按登记顺序拼接（无限升级 v11 = 三份补丁 DLL）；
               每段单独出密文文件（构建脚本 objcopy 成 .a 内嵌），MAC/长度进同一张头

用法：
    python 04_scripts/_common/gen_payload_enc.py --game <tag>          # 出密文 + 钥
    python 04_scripts/_common/gen_payload_enc.py --game <tag> --check   # 新鲜度校验（门禁 C6）
"""
import argparse
import hashlib
import hmac
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
sys.path.insert(0, HERE)
import harden_apk as GATE  # noqa: E402
# ↑ 登记表**只有一张**：出包门禁 `harden_apk.py::GAMES`（2026-10-27 与 Windows 端合并口径）。
#   载荷形态写在表项的 payload_kind / payload_src|payload_blobs / payload_header 里，
#   本脚本按 --game <tag> 读同一段配置，避免两份表各自漂移。

JNI = GATE.JNI

# 历史生成物的注释行必须逐字节保持不变（勇士之心已交付包的密文头不能因为重构而改变）
LEGACY_GEN_CMD = {"ysxz": "04_scripts/勇士之心/gen_payload_enc.py"}
GEN_CMD = {}       # tag -> 写进生成物注释的命令行（main 里填）

MASTER = os.path.join(os.path.expanduser("~"), ".starry-keys", "payload_master.txt")
KEYDIR = os.path.join(os.path.expanduser("~"), ".starry-keys", "payload")


def load_master(create=True):
    if os.path.exists(MASTER):
        txt = io.open(MASTER, "r", encoding="ascii").read().strip()
        if len(txt) >= 32:
            return bytes.fromhex(txt[:64])
    if not create:
        raise SystemExit("主密钥缺失：%s" % MASTER)
    os.makedirs(os.path.dirname(MASTER), exist_ok=True)
    k = os.urandom(32)
    io.open(MASTER, "w", encoding="ascii", newline="\n").write(k.hex() + "\n")
    print("已生成主密钥（务必与服务器共用同一份）：%s" % MASTER)
    return k


def build_key(master, digest):
    return hmac.new(master, b"sgpay-build|" + digest, hashlib.sha256).digest()


def keystream_xor(key, data):
    out = bytearray(data)
    ctr = 0
    for off in range(0, len(out), 32):
        blk = hmac.new(key, b"sgp1" + ctr.to_bytes(4, "big"), hashlib.sha256).digest()
        m = min(32, len(out) - off)
        for i in range(m):
            out[off + i] ^= blk[i]
        ctr += 1
    return bytes(out)


def enc(key, plain):
    ct = keystream_xor(key, plain)
    mac = hmac.new(key, b"sgm1" + ct, hashlib.sha256).digest()
    return ct, mac


# ---------------------------------------------------------------- 载荷读取

def cfg_of(tag):
    """从门禁登记表取该游戏的载荷配置（--game 可给 tag，也可给中文游戏名）。"""
    for name, c in GATE.GAMES.items():
        if tag == c.get("tag") or tag == name:
            g = dict(c)
            g["name"] = name
            g.setdefault("tag", c.get("tag"))
            return g
    raise SystemExit("未知游戏 tag=%s（已登记：%s）"
                     % (tag, ", ".join("%s/%s" % (n, c.get("tag")) for n, c in GATE.GAMES.items())))


def rel(p):
    return os.path.join(ROOT, p) if p else ""


def load_unit(g):
    """返回 (载荷明文整体, 分段列表)。分段 = [{sym,name,plain,enc,data}]"""
    kind = g.get("payload_kind")
    if kind == "js":
        src = rel(g["payload_src"])
        s = io.open(src, "r", encoding="utf-8", errors="surrogateescape").read()
        mk = g["payload_marker"]
        m = re.search(r'R"%s\((.*)\)%s"' % (mk, mk), s, re.S)
        if not m:
            raise SystemExit('载荷头 %s 里没找到 R"%s(...)%s" 段' % (g["payload_src"], mk, mk))
        body = m.group(1).encode("utf-8", "surrogateescape")
        return body, [dict(sym=None, name="js", data=body)]
    if kind == "bins":
        parts, whole = [], bytearray()
        for b in g.get("payload_blobs", []):
            path = rel(b["plain"])
            if not os.path.exists(path):
                raise SystemExit("载荷明文缺失：%s（先跑该游戏的补丁环节）" % b["plain"])
            data = io.open(path, "rb").read()
            parts.append(dict(sym=b["sym"], name=b["name"], data=data,
                              enc=b["enc"], plain=b["plain"]))
            whole += data
        return bytes(whole), parts
    raise SystemExit("该游戏没有可加密的载荷（payload_kind=%s）" % kind)


# ---------------------------------------------------------------- 生成物

def emit_js(g, ct, mac, sha):
    pre = g["sym_prefix"]
    lines = [
        "#pragma once",
        "/* 自动生成，勿手改：python %s */" % GEN_CMD[g["tag"]],
        "/* 明文载荷 sha256 = %s，密文 %d B（K_build 不在此文件内）*/" % (sha, len(ct)),
        "#include <stdint.h>",
        "static const uint32_t %sPayloadLen = %du;" % (pre, len(ct)),
        "static const uint8_t %sPayloadMac[32] = {%s};" % (pre, ",".join("%d" % b for b in mac)),
        "static const uint8_t %sPayloadCt[%d] = {" % (pre, len(ct)),
    ]
    for i in range(0, len(ct), 16):
        lines.append("    " + ",".join("%d" % b for b in ct[i:i + 16]) + ",")
    lines.append("};")
    lines.append('static const char* %sPayloadSha = "%s";' % (pre, sha))
    return "\n".join(lines) + "\n"


def antiban_key(sha_hex):
    """防封基线密钥 = sha256("starry-antiban-v1|" || payload_sha_hex)。
    与票无关（so 侧同名派生），故意的：基线不含功能代码，只要求「不以明文进包」。"""
    return hashlib.sha256(b"starry-antiban-v1|" + sha_hex.encode("ascii")).digest()


def emit_bins(g, encs, sha, total, antiban=None):
    pre = g["sym_prefix"]
    lines = [
        "#pragma once",
        "/* 自动生成，勿手改：python %s */" % GEN_CMD[g["tag"]],
        "/* 明文载荷 sha256 = %s（%d 段拼接，共 %d B），各段密文分别内嵌（K_build 不在此文件内）*/"
        % (sha, len(encs), total),
        "#include <stdint.h>",
        'static const char* %sPayloadSha = "%s";' % (pre, sha),
        "static const int %sPayloadSegs = %d;" % (pre, len(encs)),
    ]
    for e in encs:
        sym = "%s%s" % (pre, e["sym"])
        lines.append("/* %s：%s  %d B */" % (e["name"], e["plain"], e["len"]))
        lines.append("static const uint32_t %sPayloadLen = %du;" % (sym, e["len"]))
        lines.append("static const uint8_t %sPayloadMac[32] = {%s};"
                     % (sym, ",".join("%d" % b for b in e["mac"])))
        lines.append('static const char* %sPlainSha = "%s";' % (sym, e["sha"]))
    if antiban:
        # 防封基线（无票路径）：官方 + 上报/执法屏蔽，不含任何功能代码；密钥由 payload sha 派生
        lines.append("")
        lines.append("/* 防封基线（无票也生效，不含任何作弊功能代码）：%s  %d B */" % (antiban["plain"], antiban["len"]))
        lines.append("static const uint32_t %sAntibanLen = %du;" % (pre, antiban["len"]))
        lines.append("static const uint8_t %sAntibanMac[32] = {%s};"
                     % (pre, ",".join("%d" % b for b in antiban["mac"])))
        lines.append('static const char* %sAntibanSha = "%s";' % (pre, antiban["sha"]))
    return "\n".join(lines) + "\n"


def write_key(sha, key):
    os.makedirs(KEYDIR, exist_ok=True)
    kf = os.path.join(KEYDIR, sha + ".key")
    io.open(kf, "w", encoding="ascii", newline="\n").write(key.hex() + "\n")
    return kf


def remind_register(g, sha, write=True):
    """载荷 sha 变了/新游戏时，打一段**必须处理**的提醒（出包机不看就发版 ⇒ 功能全灭）。"""
    last = os.path.join(KEYDIR, g["game_id"] + ".last")
    old = ""
    if os.path.exists(last):
        try:
            old = io.open(last, "r", encoding="ascii").read().strip().lower()
        except Exception:  # noqa: BLE001
            old = ""
    if old == sha:
        return 0
    line = "=" * 68
    kind = ("新游戏（首次加密，服务端还没有这条登记）" if not old
            else "载荷变了（当前出包 sha 与服务端上次登记的不同）")
    print("")
    print(line)
    print("⚠️  %s" % kind)
    print("    游戏：%s（game_id=%s）" % (g["name"], g["game_id"]))
    print("    新 sha：%s" % sha)
    if old:
        print("    旧 sha：%s" % old)
    print("")
    print("    👉 必须去后台点一次，否则这版包在真机上功能全失效（只剩诱饵）：")
    print("       后台 → 游戏列表 → %s → 操作 ▾ → 「载荷 sha」→ 粘贴上面的 sha → 保存" % g["name"])
    print("       （或选密文文件让后台自动算；等价接口）")
    print("       PUT /v1/admin/games/%s/payload?sha=%s" % (g["game_id"], sha))
    print("")
    print("    ⚠️  改 sha 会让该游戏**所有旧包**功能失效，记得同步发新包。")
    print(line)
    print("")
    if write:
        try:
            io.open(last, "w", encoding="ascii", newline="\n").write(sha + "\n")
        except Exception as e:  # noqa: BLE001
            print("（写 %s 失败：%s）" % (last, e))
    return 0


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument("--game", required=True, help="harden_apk.py::GAMES 里的 tag（或游戏名）")
    ap.add_argument("--check", action="store_true")
    a = ap.parse_args(argv)

    g = cfg_of(a.game)
    GEN_CMD[g["tag"]] = LEGACY_GEN_CMD.get(
        g["tag"], "04_scripts/_common/gen_payload_enc.py --game %s" % g["tag"])
    out = os.path.join(JNI, g["payload_header"])
    payload, parts = load_unit(g)
    digest = hashlib.sha256(payload).digest()
    sha = digest.hex()
    master = load_master(create=not a.check)
    key = build_key(master, digest)

    if a.check:
        if not os.path.exists(out):
            print("FAIL 缺少 %s（未跑 gen_payload_enc.py --game %s）" % (g["payload_header"], a.game))
            return 1
        head = io.open(out, "r", encoding="utf-8").read()
        if sha not in head:
            print("FAIL 密文头与当前载荷不一致（sha %s 不在头里）——载荷改过但没重新加密" % sha[:16])
            return 1
        if g.get("payload_kind") == "bins":
            # 逐段自解：密文文件必须存在、长度一致、且解回来与明文逐字节相同
            for p in parts:
                ep = rel(p["enc"])
                if not os.path.exists(ep):
                    print("FAIL 密文段缺失：%s" % p["enc"])
                    return 1
                ct = io.open(ep, "rb").read()
                if len(ct) != len(p["data"]):
                    print("FAIL %s 密文长度 %d != 明文 %d" % (p["name"], len(ct), len(p["data"])))
                    return 1
                back = keystream_xor(key, ct)
                if back != p["data"]:
                    print("FAIL %s 解回来的明文与当前载荷不一致（换过 master 或旧密文）" % p["name"])
                    return 1
        spec = g.get("antiban")
        if spec:
            abp = rel(spec["plain"])
            abep = rel(spec["enc"])
            if not (os.path.exists(abp) and os.path.exists(abep)):
                print("FAIL 防封基线产物缺失：%s / %s" % (spec["plain"], spec["enc"]))
                return 1
            abd = io.open(abp, "rb").read()
            if keystream_xor(antiban_key(sha), io.open(abep, "rb").read()) != abd:
                print("FAIL 防封基线密文与当前基线明文不一致（重跑生成器）")
                return 1
            print("PASS 防封基线密文与明文一致 %d B" % len(abd))
        print("PASS 载荷密文与当前明文一致 sha=%s…" % sha[:16])
        remind_register(g, sha, write=False)
        return 0

    if g.get("payload_kind") == "js":
        ct, mac = enc(key, payload)
        os.makedirs(os.path.dirname(out), exist_ok=True)
        io.open(out, "w", encoding="utf-8", newline="\n").write(emit_js(g, ct, mac, sha))
        total = len(ct)
        detail = "明文 %d B  →  密文 %d B" % (len(payload), total)
    else:
        encs, total = [], 0
        for p in parts:
            ct, mac = enc(key, p["data"])
            ep = rel(p["enc"])
            os.makedirs(os.path.dirname(ep), exist_ok=True)
            io.open(ep, "wb").write(ct)
            encs.append(dict(sym=p["sym"], name=p["name"], plain=p["plain"], len=len(ct),
                             mac=mac, sha=hashlib.sha256(p["data"]).hexdigest()))
            total += len(ct)
        ab = None
        spec = g.get("antiban")
        if spec:
            abp = rel(spec["plain"])
            if not os.path.exists(abp):
                raise SystemExit("防封基线明文缺失：%s（先跑 WXSJ_ANTIBAN_ONLY=1 的 ilpatch / ilpatch_delta）"
                                 % spec["plain"])
            abd = io.open(abp, "rb").read()
            abct, abmac = enc(antiban_key(sha), abd)
            abep = rel(spec["enc"])
            os.makedirs(os.path.dirname(abep), exist_ok=True)
            io.open(abep, "wb").write(abct)
            ab = dict(plain=spec["plain"], len=len(abct), mac=abmac,
                      sha=hashlib.sha256(abd).hexdigest())
            print("  防封基线 %-58s %8d B（独立密钥，无票也解）" % (spec["plain"], ab["len"]))
        os.makedirs(os.path.dirname(out), exist_ok=True)
        io.open(out, "w", encoding="utf-8", newline="\n").write(emit_bins(g, encs, sha, total, ab))
        detail = "%d 段明文共 %d B  →  密文共 %d B" % (len(encs), len(payload), total)
        for e in encs:
            print("  %-4s %-58s %8d B" % (e["name"], e["plain"], e["len"]))

    kf = write_key(sha, key)
    print("%s" % detail)
    print("载荷 sha256 = %s" % sha)
    print("K_build 已写: %s（服务器签发 v3 票要用它算 kx）" % kf)
    print("密文头已写: %s" % os.path.relpath(out, ROOT))
    remind_register(g, sha, write=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
