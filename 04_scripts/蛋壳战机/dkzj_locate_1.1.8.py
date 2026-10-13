#!/usr/bin/env python3
"""蛋壳战机 1.1.8 锚点定位：从 Il2CppDumper dump.cs 抽取方法 RVA/Offset 与字段偏移。

用法：python3 dkzj_locate_1.1.8.py [dump.cs] [--fields]
只按目标类名做窗口扫描（dump.cs 57MB，禁整读）。
"""
import re
import sys
from pathlib import Path

DUMP = Path(sys.argv[1] if len(sys.argv) > 1
            else "06_dumped/蛋壳战机/il2cpp_dump_1.1.8/dump.cs")

# (namespace, class, [methods...])  —— 类名按 dump.cs 里 `class <Name>` 精确匹配
CLASSES = [
    ("HotFix.BattleLogic", "EntityCharacter", ["ChangeHp"]),
    ("HotFix.BattleLogic", "EntityHero", ["ChangeHp"]),
    ("HotFix.BattleLogic", "BattleWorldContext",
     ["PostProcessLearnedSkill", "AddSkillToCharacter"]),
    ("HotFix", "ADModuleMgr", ["CheckAndPlayVideo"]),
    ("", "LocalModelManager", ["GetSkill_Main"]),
]
# 字段偏移目标：类 -> [字段名前缀]
FIELDS = [
    ("HeroComponentRandomSkill", ["_allSkills", "RandomSkillResult", "randomSkillResult"]),
    ("RandomSkillResult", ["ShowSkills", "showSkills"]),
    ("Skill_Main", ["UpgradeSkillId", "upgradeSkillId"]),
]

CLASS_RE = re.compile(r"^(?:public |internal |sealed )*(?:class|struct|interface|enum) (\w+)")
NS_RE = re.compile(r"^// Namespace: (.*)$")
RVA_RE = re.compile(r"//\s*RVA:\s*(0x[0-9A-Fa-f]+)\s+Offset:\s*(0x[0-9A-Fa-f]+)")


def iter_class_blocks(lines, want_class, want_ns=None):
    """yield (start, end, ns, class) for each matching class header."""
    hits = []
    cur_ns = ""
    cur = None
    for i, line in enumerate(lines):
        m = NS_RE.match(line)
        if m:
            cur_ns = m.group(1).strip()
            continue
        m = CLASS_RE.match(line)
        if m:
            if cur is not None:
                cur[1] = i
                if cur[3] == want_class and (want_ns is None or cur[2] == want_ns):
                    hits.append(tuple(cur))
            cur = [i, len(lines), cur_ns, m.group(1)]
    if cur is not None:
        cur[1] = len(lines)
        if cur[3] == want_class and (want_ns is None or cur[2] == want_ns):
            hits.append(tuple(cur))
    return hits


def main():
    text = DUMP.read_text(encoding="utf-8", errors="replace")
    lines = text.splitlines()
    print(f"# dump={DUMP} lines={len(lines)}")

    for ns, cls, methods in CLASSES:
        blocks = iter_class_blocks(lines, cls, ns if ns else None)
        if not blocks:
            blocks = iter_class_blocks(lines, cls)
        print(f"\n## {ns}.{cls}  (blocks={len(blocks)})")
        for start, end, bns, bcls in blocks:
            print(f"   header line={start+1} ns='{bns}'")
            for m in methods:
                pat = re.compile(r"\b" + re.escape(m) + r"\s*\(")
                found = 0
                for i in range(start, min(end, start + 4000)):
                    if pat.search(lines[i]) and "RVA" not in lines[i]:
                        # RVA 注释在方法声明前一行（或前两行内含 Attribute）
                        for back in (1, 2, 3):
                            rva = RVA_RE.search(lines[i - back]) if i - back >= 0 else None
                            if rva:
                                off = int(rva.group(2), 16)
                                rva_v = int(rva.group(1), 16)
                                print(f"   {m}: line={i+1} RVA={rva.group(1)} "
                                      f"Offset={rva.group(2)} file_delta=0x{rva_v - off:X}")
                                print(f"        decl: {lines[i].strip()[:180]}")
                                found += 1
                                break
                        if found >= 3:
                            break

    print("\n## fields")
    for cls, prefixes in FIELDS:
        blocks = iter_class_blocks(lines, cls)
        print(f"\n### {cls} (blocks={len(blocks)})")
        for start, end, bns, bcls in blocks:
            print(f"   header line={start+1} ns='{bns}'")
            for i in range(start, min(end, start + 800)):
                line = lines[i]
                if "// 0x" not in line:
                    continue
                for p in prefixes:
                    if re.search(r"\b" + re.escape(p), line):
                        print(f"      {line.strip()[:160]}")
                        break


if __name__ == "__main__":
    main()
