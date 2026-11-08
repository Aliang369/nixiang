#!/usr/bin/env python3
"""动物大联盟 1.0.25 —— res JS 补丁器（在解密后的 JS 上做定点插桩，再回封成 .jsc）

设计原则:
  - 只做「最小插入」，不改原语句语义；开关全部挂在 window.__nx 上，默认关闭 ⇒ 未注入时行为与官方一致
  - 每个补丁有唯一锚点串，命中数必须 == 1，否则整批放弃（防串改）
  - 输出前做 node --check 语法校验（若本机有 node）+ 回封 roundtrip 校验

用法:
  python3 patch_js_dwdlm.py <src_decrypted_dir> <out_dir>          # 打补丁（只改文件，不加密）
  python3 patch_js_dwdlm.py --enc <out_dir> <jsc_out_dir>          # 把打好的 .js 回封成 .jsc
"""
import os, sys, shutil, subprocess, json

BATTLE = "assets/battleScript/index.js"

# window.__nx 的默认值注入（放在 battleScript 顶部，模块表之前）
NX_PRELUDE = """// ==== NX patch prelude (动物大联盟 mod) ====
window.__nx = window.__nx || { atkMul: 1, god: false, infSkill: false };
// ==== end prelude ====
"""

PATCHES = [
    dict(
        name="atkMul 攻击倍率",
        file=BATTLE,
        anchor="te = Math.ceil(St * _e * Mt) + Math.ceil(Kt * _e);",
        replace=("te = Math.ceil(St * _e * Mt) + Math.ceil(Kt * _e);\n"
                 "if (window.__nx && window.__nx.atkMul > 1 && te > 0) te = Math.ceil(te * window.__nx.atkMul);"),
    ),
    dict(
        name="god 上帝模式（本地玩家受击伤害归零）",
        file=BATTLE,
        anchor="te = Math.ceil(St * _e * Mt) + Math.ceil(Kt * _e);\n"
               "if (window.__nx && window.__nx.atkMul > 1 && te > 0) te = Math.ceil(te * window.__nx.atkMul);",
        replace=("te = Math.ceil(St * _e * Mt) + Math.ceil(Kt * _e);\n"
                 "if (window.__nx && window.__nx.atkMul > 1 && te > 0) te = Math.ceil(te * window.__nx.atkMul);\n"
                 "if (window.__nx && window.__nx.god && bt && bt._Va === g) te = 0;"),
    ),
    dict(
        name="infSkill 无限技能（跳过 CD 门控）",
        file=BATTLE,
        anchor="if (Pt[n._gm._Vr] < 1) return \"continue\";",
        replace="if (Pt[n._gm._Vr] < 1 && !(window.__nx && window.__nx.infSkill)) return \"continue\";",
    ),
]


def apply_patches(src_dir, out_dir):
    if os.path.exists(out_dir):
        shutil.rmtree(out_dir)
    shutil.copytree(src_dir, out_dir)
    report = []
    stats = {}
    for p in PATCHES:
        path = os.path.join(out_dir, p["file"])
        txt = open(path, encoding="utf-8").read()
        cnt = txt.count(p["anchor"])
        if cnt != 1:
            report.append("FAIL %-42s anchor 命中 %d 次（应为 1）" % (p["name"], cnt))
            stats[p["name"]] = False
            continue
        txt = txt.replace(p["anchor"], p["replace"])
        open(path, "w", encoding="utf-8").write(txt)
        report.append("OK   %-42s 已插入" % p["name"])
        stats[p["name"]] = True
    # prelude
    bp = os.path.join(out_dir, BATTLE)
    txt = open(bp, encoding="utf-8").read()
    if "NX patch prelude" not in txt:
        open(bp, "w", encoding="utf-8").write(NX_PRELUDE + txt)
        report.append("OK   prelude 已插入 %s" % BATTLE)
    # 语法校验
    for rel in (BATTLE,):
        f = os.path.join(out_dir, rel)
        try:
            r = subprocess.run(["node", "--check", f], capture_output=True, text=True, timeout=120)
            report.append("%s node --check %s" % ("OK  " if r.returncode == 0 else "FAIL", rel))
            if r.returncode != 0:
                report.append(r.stderr.strip()[:400])
        except FileNotFoundError:
            report.append("SKIP node 不可用，跳过语法校验")
        except Exception as e:
            report.append("SKIP node 校验异常 %s" % e)
    print("\n".join(report))
    return all(stats.values())


def main(argv):
    if len(argv) >= 2 and argv[1] == "--enc":
        sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
        from jsc_codec_dwdlm import encrypt
        src, dst = argv[2], argv[3]
        for root, _d, files in os.walk(src):
            for f in files:
                if not f.endswith(".js"):
                    continue
                p = os.path.join(root, f)
                rel = os.path.relpath(p, src)
                out = os.path.join(dst, rel[:-3] + ".jsc")
                os.makedirs(os.path.dirname(out), exist_ok=True)
                enc = encrypt(open(p, "rb").read())
                open(out, "wb").write(enc)
                print("ENC %-46s %d B" % (rel, len(enc)))
        return 0
    if len(argv) != 3:
        print(__doc__)
        return 2
    ok = apply_patches(argv[1], argv[2])
    print("ALL_OK" if ok else "SOME_FAILED")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv))
