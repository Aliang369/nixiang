#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""追迹迷城：按「命名空间.类.方法」在 Il2CppDumper 的 dump.cs 里定位 RVA（＝文件偏移）。

用途：版本位移时把 ZjmcMod.cpp 的 17 个 RVA 常量重新取值，语义原样复用（硬约束：禁止自创挂点）。

用法:
  python3 find_rva_zjmc.py --dump 06_dumped/追迹迷城/1.0.20261030_il2cpp/dump.cs \
                           [--json 06_dumped/追迹迷城/1.0.20261030_il2cpp/rva_targets.json]

输出：每个目标的 RVA/Offset/VA + 类内相对间距核对（与上一版全等＝同源代码线性位移）。
"""
import argparse
import json
import re
import sys

# (常量名, 命名空间, 类名, 方法名, 期望参数类型片段, 角色) —— 唯一来源＝锚点_五功能 文档
TARGETS = [
    ("RVA_BM_UPDATE", "Teamelo.ActionEngine", "BattleManager", "FixedUpdate", [], "AOE hook(BattleManager$$FixedUpdate)"),
    ("RVA_BM_GET_ALIVE", "Teamelo.ActionEngine", "BattleManager", "GetCurWaveAliveEnemyList", [], "AOE helper"),
    ("RVA_ATK_CAN_USE", "Teamelo.ActionEngine", "AttackBattleData", "CheckCurrentIsCanUse", [], "infinite skill"),
    ("RVA_ATK_CUR_SP", "Teamelo.ActionEngine", "AttackBattleData", "CheckCurSp", [], "infinite skill"),
    ("RVA_PARTNER_CAN_USE", "Teamelo.ActionEngine", "MonsterPartnerSkillBattleData", "CheckCurrentIsCanUse", [], "infinite skill"),
    ("RVA_PARTNER_CUR_SP", "Teamelo.ActionEngine", "MonsterPartnerSkillBattleData", "CheckCurSp", [], "infinite skill"),
    ("RVA_LP_CHECK_1", "Teamelo.ActionEngine", "LocalPlayer", "CheckSkillUse_1", [], "infinite skill"),
    ("RVA_LP_CHECK_2", "Teamelo.ActionEngine", "LocalPlayer", "CheckSkillUse_2", [], "infinite skill"),
    ("RVA_LP_CHECK_DODGE", "Teamelo.ActionEngine", "LocalPlayer", "CheckSkillUse_Dodge", [], "infinite skill optional"),
    ("RVA_LP_PROCESS_DMG", "Teamelo.ActionEngine", "LocalPlayer", "ProcessDamageOrHeal", ["RoleUnit", "CombatResultData"], "invincible"),
    ("RVA_LP_CHANGE_HP", "Teamelo.ActionEngine", "LocalPlayer", "ChangeHP", ["float", "bool"], "invincible fallback"),
    ("RVA_LP_CHECK_ACTION", "Teamelo.ActionEngine", "LocalPlayer", "CheckSkillUse_ActionState", [], "infinite skill"),
    ("RVA_MONSTER_PROCESS_DMG", "Teamelo.ActionEngine", "Monster", "ProcessDamageOrHeal", ["RoleUnit", "CombatResultData"], "one-hit kill fallback"),
    ("RVA_MONSTER_CHANGE_HP", "Teamelo.ActionEngine", "Monster", "ChangeHP", ["float", "bool"], "one-hit kill"),
    ("RVA_GET_FREE_SKIP_ADS", "Teamelo.ProjectAT", "ATGameManager", "GetFreeSkipAdsRemainingCount", [], "no ad"),
    ("RVA_GET_FREE_SKIP_ADS_USED", "Teamelo.ProjectAT", "ATGameManager", "GetFreeSkipAdsUsedCount", [], "no ad"),
    ("RVA_USE_FREE_SKIP_ADS", "Teamelo.ProjectAT", "ATGameManager", "UseFreeSkipAds", [], "no ad"),
]

# 类内相对间距核对
# STABLE：0912 与 1009 两版实测**逐条相等**的间距（同源代码线性位移的判据）
# INFO：仅作回显记录，版本间会变（1009→1030 实测：LP Check1→Check2 0xA8→0x2C，
#       Monster PDH→HP 0xA20→0x2444 ⇒ 更新改变了函数体长度，不是定位错误）
STABLE = [
    ("LP_DODGE-LP_CHECK_2", "RVA_LP_CHECK_2", "RVA_LP_CHECK_DODGE", 0x2C),
    ("ATK_CUR_SP-ATK_CAN_USE", "RVA_ATK_CAN_USE", "RVA_ATK_CUR_SP", 0x1C),
    ("PARTNER_CUR_SP-PARTNER_CAN_USE", "RVA_PARTNER_CAN_USE", "RVA_PARTNER_CUR_SP", 0x34),
    ("ADS_USED-ADS_REMAINING", "RVA_GET_FREE_SKIP_ADS", "RVA_GET_FREE_SKIP_ADS_USED", 0x17C),
    ("ADS_USE-ADS_USED", "RVA_GET_FREE_SKIP_ADS_USED", "RVA_USE_FREE_SKIP_ADS", 0x24),
]
INFO = [
    ("LP_CHECK_2-LP_CHECK_1", "RVA_LP_CHECK_1", "RVA_LP_CHECK_2"),
    ("MONSTER_HP-MONSTER_PDH", "RVA_MONSTER_PROCESS_DMG", "RVA_MONSTER_CHANGE_HP"),
    ("LP_HP-LP_PDH", "RVA_LP_PROCESS_DMG", "RVA_LP_CHANGE_HP"),
    ("BM_ALIVE-BM_UPDATE", "RVA_BM_UPDATE", "RVA_BM_GET_ALIVE"),
]

RE_METHOD = re.compile(r"^\s*//\s*RVA:\s*(0x[0-9A-Fa-f]+)\s+Offset:\s*(0x[0-9A-Fa-f]+)\s+VA:\s*(0x[0-9A-Fa-f]+)")
RE_NS = re.compile(r"^//\s*Namespace:\s*(\S*)")
RE_TYPE = re.compile(r"^\s*(?:public|private|internal|protected|sealed|abstract|\s)*\s*(?:class|struct|interface|enum)\s+([A-Za-z_]\w*)")


def parse(dump_path):
    """返回 {(ns, class, method): [(rva, offset, va, lineno, param_text)]}"""
    out = {}
    ns = ""
    cls = ""
    pending = None
    with open(dump_path, "r", encoding="utf-8", errors="replace") as f:
        for lineno, line in enumerate(f, 1):
            m = RE_NS.match(line)
            if m:
                ns = m.group(1)
                continue
            m = RE_TYPE.match(line)
            if m and not line.lstrip().startswith("//"):
                cls = m.group(1)
                pending = None
                continue
            m = RE_METHOD.match(line)
            if m:
                pending = (m.group(1), m.group(2), m.group(3), lineno)
                continue
            if pending is not None:
                s = line.strip()
                if s and not s.startswith("//"):
                    # 形如：public float ChangeHP(float hp, bool showText) { }
                    sig = s.split("{")[0].strip()
                    name = None
                    paren = sig.find("(")
                    if paren > 0:
                        head = sig[:paren].split()
                        if head:
                            name = head[-1]
                    if name:
                        key = (ns, cls, name)
                        out.setdefault(key, []).append(
                            (pending[0], pending[1], pending[2], pending[3], sig)
                        )
                    pending = None
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dump", required=True)
    ap.add_argument("--json", default="")
    ap.add_argument("--libil2cpp-md5", default="")
    ap.add_argument("--metadata-md5", default="")
    ap.add_argument("--version", default="")
    ap.add_argument("--vc", default="")
    a = ap.parse_args()

    table = parse(a.dump)
    found = {}
    problems = []
    print("%-30s %-12s %-12s %-12s %s" % ("constant", "RVA(file)", "Offset", "VA", "signature"))
    for const, ns, cls, meth, params, role in TARGETS:
        hits = table.get((ns, cls, meth))
        if not hits:
            problems.append("未命中 %s.%s.%s" % (ns, cls, meth))
            continue
        if params:
            filt = [h for h in hits if all(p in h[4] for p in params)]
            if filt:
                hits = filt
        if len(hits) > 1:
            problems.append("重载歧义 %s.%s.%s: %d 个候选" % (ns, cls, meth, len(hits)))
        rva, off, va, lineno, sig = hits[0]
        found[const] = {"name": "%s.%s.%s" % (ns, cls, meth), "rva": rva,
                        "offset": off, "va": va, "signature": sig,
                        "line": lineno, "role": role}
        print("%-30s %-12s %-12s %-12s %s" % (const, rva, off, va, sig[:60]))

    # ELF bias 核对：VA - RVA 应为 RX LOAD 的 vaddr-file 差（历史 0x4000）
    biases = set()
    for v in found.values():
        biases.add(int(v["va"], 16) - int(v["rva"], 16))
    print("\nVA-RVA 差值集合: %s" % (", ".join("0x%X" % b for b in sorted(biases))))

    print("\n类内相对间距核对（STABLE=跨版本不变，必须相等；INFO=仅记录）:")
    for label, ca, cb, expect in STABLE:
        if ca in found and cb in found:
            d = int(found[cb]["rva"], 16) - int(found[ca]["rva"], 16)
            ok = "OK" if d == expect else "DIFF"
            print("  %-32s = 0x%-6X 期望 0x%-6X %s" % (label, d, expect, ok))
            if d != expect:
                problems.append("间距漂移 %s" % label)
    for label, ca, cb in INFO:
        if ca in found and cb in found:
            d = int(found[cb]["rva"], 16) - int(found[ca]["rva"], 16)
            print("  %-32s = 0x%-6X (参考)" % (label, d))

    if a.json:
        payload = {
            "game": "追迹迷城",
            "version_name": a.version,
            "version_code": int(a.vc) if a.vc else None,
            "package": "com.TeaMelo.DungeonChasers",
            "libil2cpp_md5": a.libil2cpp_md5,
            "global_metadata_md5": a.metadata_md5,
            "elf_bias": "0x4000",
            "source": a.dump,
            "targets": [
                {"constant": c, **v} for c, v in found.items()
            ],
            "problems": problems,
        }
        with open(a.json, "w", encoding="utf-8") as f:
            json.dump(payload, f, ensure_ascii=False, indent=2)
        print("\n已写入 %s" % a.json)

    if problems:
        print("\n问题:")
        for p in problems:
            print("  - %s" % p)
        return 1
    print("\n全部 17 个目标命中且间距与上版一致")
    return 0


if __name__ == "__main__":
    sys.exit(main())
