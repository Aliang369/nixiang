# -*- coding: utf-8 -*-
"""P1-6：生成「完整性清单」C 头（含私钥签名）—— **按游戏**输出 `sg_integ_<tag>.h`。

为什么清单在**打包前**生成（而不是打包后）：
    清单要编进 so，so 又要打进 APK —— 打包后生成就成了死循环。
    解法：清单只覆盖**明文内容**（菜单 dex / boot js / 证书），这些在 so 编译前就已定稿；
    native 运行时从 APK 里解压出明文再比对。所以清单**不覆盖 so 自身**（自指不可能），
    但覆盖了「删我卡密 dex / 换 boot js / 换证书」这三条主路。

为什么要签名：
    清单若只是明文表，破解者改表即可自我放行。清单用**私钥签名**后，native 先验签名
    再比对哈希 ⇒ 没有私钥改不动这张表（与授权票同一把钥匙、同一套 RSA 验签）。

按游戏（2026-10-27 通用化，`10_docs/防御加固_通用路线图.md` §1 耦合点 2.2）：
    每个游戏一份条目表 + 一份输出头，头文件名带游戏 tag，编译期由 mk 用
    `-DSG_INTEG_HEADER_FILE='"sg_integ_<tag>.h"'` 指过去（见 `Includes/SgIntegrity.h`）。
    新增游戏 = 在下面 `GAMES` 里加一段配置，不改本文件逻辑。

用法：
  python 04_scripts/_common/sg_pki/gen_integ_header.py --game mxyzt            # 生成
  python 04_scripts/_common/sg_pki/gen_integ_header.py --game mxyzt --check    # 校验（出包门禁 C2 用）
  python 04_scripts/_common/sg_pki/gen_integ_header.py --check                 # 不带 --game ⇒ 默认 ysxz
"""
import argparse
import hashlib
import io
import os
import sys
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(ROOT, "mod菜单", "starry-mod-menu", "auth-server"))
import sg_rsa_pkcs1 as P  # noqa: E402

DEFAULT_KEY = os.path.join(os.path.expanduser("~"), ".starry-keys", "rsa2048.txt")
JNI_INCLUDES = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni",
                            "Includes")

# ============================================================
#  按游戏的清单条目（新增游戏只加这里）
#    · entries 顺序即签名串顺序，**改动要同步 C 侧注释**（SgIntegrity.h 的 canonical 串）
#    · src       = 仓库内相对路径（本地产物，打包前已定稿）
#    · from_base = 从该游戏 base_apk 里取同名条目（游戏官方资源，例如 boot js）
# ============================================================
GAMES = {
    "ysxz": {
        "name": "勇士之心",
        # 历史文件名：勇士之心 so 已按 `sg_integ.h` 编译，改名会让旧包/旧构建链断掉 ⇒ 保留
        "out": "sg_integ.h",
        "base_apk": "02_apks/勇士之心.apk",
        "entries": [
            {"apk": "classes4.dex",
             "src": "mod菜单/starry-mod-menu/libs_ysxz/classes4.dex"},
            {"apk": "assets/main.js", "from_base": True},
            {"apk": "assets/certs/starlight.crt",
             "src": "mod菜单/starry-mod-menu/app/src/main/assets/certs/starlight.crt"},
        ],
    },
    "mxyzt": {
        "name": "冒险与征途",
        "out": "sg_integ_mxyzt.h",
        "entries": [
            # 本作是 native 型（无 boot js）：菜单 dex 由外科 zip 注入成 classes4.dex
            {"apk": "classes4.dex",
             "src": "mod菜单/starry-mod-menu/templates/mxyzt_menu_current/classes4.dex"},
            {"apk": "assets/certs/starlight.crt",
             "src": "mod菜单/starry-mod-menu/app/src/main/assets/certs/starlight.crt"},
        ],
    },
}


def cfg_of(tag):
    cfg = GAMES.get(tag)
    if not cfg:
        raise SystemExit("ERROR: 未知游戏 tag=%s（可选：%s）。新增游戏请在 GAMES 里加配置。"
                         % (tag, ", ".join(sorted(GAMES))))
    return cfg


def collect(cfg):
    """返回 [(apk 内路径, 明文字节)] —— 顺序固定（= 签名串顺序）。"""
    items = []
    base_apk = os.path.join(ROOT, cfg["base_apk"]) if cfg.get("base_apk") else ""
    for e in cfg["entries"]:
        if e.get("from_base"):
            if not base_apk or not os.path.exists(base_apk):
                raise SystemExit("ERROR: 缺底包 %s（条目 %s 需要从底包取明文）"
                                 % (base_apk or "(未配置 base_apk)", e["apk"]))
            with zipfile.ZipFile(base_apk) as z:
                items.append((e["apk"], z.read(e["apk"])))
            continue
        p = os.path.join(ROOT, e["src"])
        if not os.path.exists(p):
            raise SystemExit("ERROR: 缺产物 %s（条目 %s）⇒ 先编出该游戏的 dex/资源"
                             % (p, e["apk"]))
        with io.open(p, "rb") as f:
            items.append((e["apk"], f.read()))
    return items


def canonical(items):
    """规范化串：SGI1|<path>=<len>:<sha256hex>|...（签的就是它）"""
    parts = ["SGI1"]
    for path, data in items:
        parts.append("%s=%d:%s" % (path, len(data), hashlib.sha256(data).hexdigest()))
    return "|".join(parts).encode("utf-8")


def render(items, key, tag, game_name, title=""):
    body = canonical(items)
    sig = P.sign(body, key) if key else None
    n = len(items)
    out = []
    out.append("#ifndef SG_INTEG_H")
    out.append("#define SG_INTEG_H")
    out.append("")
    out.append("// ============================================================")
    out.append("//  %s — %s 完整性清单（**自动生成，勿手改**）"
               % (title or ("sg_integ_%s.h" % tag), game_name))
    out.append("//  生成：04_scripts/_common/sg_pki/gen_integ_header.py --game %s" % tag)
    out.append("//  覆盖：APK 内条目明文哈希；清单由私钥签名，C 侧先验签名再比对。")
    out.append("//  改这张表没有私钥 ⇒ 验签失败 ⇒ 会话不成立（fail-closed）。")
    out.append("// ============================================================")
    out.append("")
    out.append("#include <stdint.h>")
    out.append("")
    out.append("typedef struct {")
    out.append("    const char* path;        // APK 内路径")
    out.append("    uint32_t    size;        // 明文长度")
    out.append("    uint8_t     sha256[32];  // 明文 sha256")
    out.append("} sg_integ_entry;")
    out.append("")
    out.append("static const sg_integ_entry kSgInteg[%d] = {" % n)
    for path, data in items:
        h = hashlib.sha256(data).digest()
        hx = ", ".join("0x%02x" % b for b in h)
        out.append('    { "%s", %d, { %s } },' % (path, len(data), hx))
    out.append("};")
    out.append("static const int kSgIntegCount = %d;" % n)
    out.append("")
    out.append('static const char kSgIntegSigHex[] = "%s";' % (sig.hex() if sig else ""))
    out.append("")
    out.append("#endif  // SG_INTEG_H")
    return "\n".join(out) + "\n", body, sig


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--game", default="ysxz",
                    help="游戏 tag（%s；默认 ysxz 保持历史行为）" % ", ".join(sorted(GAMES)))
    ap.add_argument("--key", default=DEFAULT_KEY)
    ap.add_argument("--out", default="", help="覆盖输出路径（默认 jni/Includes/<该游戏 out>）")
    ap.add_argument("--check", action="store_true")
    a = ap.parse_args()

    cfg = cfg_of(a.game)
    out = a.out or os.path.join(JNI_INCLUDES, cfg["out"])

    items = collect(cfg)
    key = P.load_key(a.key) if os.path.exists(a.key) else None
    if key is None:
        print("ERROR: 找不到私钥 %s（清单必须签名，否则破解者改表即放行）" % a.key)
        return 1
    text, body, sig = render(items, key, a.game, cfg.get("name", a.game),
                             os.path.basename(out))

    if a.check:
        if not os.path.exists(out):
            print("MISSING %s（先跑 gen_integ_header.py --game %s）" % (out, a.game))
            return 1
        cur = io.open(out, "r", encoding="utf-8").read()
        cur_sig = ""
        for line in cur.splitlines():
            if "kSgIntegSigHex" in line and '"' in line:
                cur_sig = line.split('"')[1]
        ok = cur_sig == sig.hex()
        print(("OK 清单一致（%s）sig=%s…" % (a.game, sig.hex()[:16])) if ok
              else "MISMATCH 清单与当前产物不符（%s）⇒ 重跑 --game %s" % (a.game, a.game))
        return 0 if ok else 1

    os.makedirs(os.path.dirname(out), exist_ok=True)
    io.open(out, "w", encoding="utf-8", newline="\n").write(text)
    print("wrote %s entries=%d body=%d B sig=%s…" % (out, len(items), len(body), sig.hex()[:16]))
    for path, data in items:
        print("  %-28s %8d B  %s" % (path, len(data), hashlib.sha256(data).hexdigest()[:16]))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
