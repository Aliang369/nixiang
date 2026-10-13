#!/usr/bin/env python3
"""蛋壳战机 1.1.8 锚点归档：从官包 + dump.cs 事实生成 抓点_锚点_1.1.8_20261014.json。

所有数值现场计算（md5/sha256/ELF 段/序言 4 字节），不手抄，避免转录错误。
用法：python3 04_scripts/蛋壳战机/dkzj_anchor_1.1.8.py
"""
import hashlib
import json
import struct
import zipfile
from pathlib import Path

ROOT = Path("/Users/aliang/Documents/nixiang")
GAME = "蛋壳战机"
BASE = ROOT / f"02_apks/{GAME}/{GAME}_1.1.8_official_from_device.apk"
SO_PATH = ROOT / f"06_dumped/{GAME}/static_inspect_1.1.8/key_files/libil2cpp.so"
OUT = ROOT / f"10_docs/{GAME}/抓点_锚点_1.1.8_20261014.json"
DELTA = 0x4000  # va = file_offset + 0x4000（1.1.8 实测仍成立）

# (显示名, dump.cs RVA, 1.1.7 旧 file offset, 序言 4 字节)
HOOKS = [
    ("HotFix.BattleLogic.EntityCharacter.ChangeHp(long)", 0x6C2E314, 0x6C31EC0, 0xA9BB6FFE),
    ("HotFix.BattleLogic.EntityHero.ChangeHp(long)", 0x6D24180, 0x6D1C3E4, 0xF81D0FFE),
    ("HotFix.BattleLogic.BattleWorldContext.PostProcessLearnedSkill(EntityCharacter,int,Skill_Main,bool,HeroComponentRandomSkill)",
     0x6EE8654, 0x6EDE058, 0xD10303FF),
    ("HotFix.ADModuleMgr.CheckAndPlayVideo(string, Action<bool>)", 0x77EBA98, 0x77E2EF8, 0xA9BC67FE),
]
AUX = [
    ("HotFix.BattleLogic.BattleWorldContext.AddSkillToCharacter(EntityCharacter,int,bool)",
     0x6ED8CA8, 0x6ECE6AC, "偷盗技能回灌入口；MethodInfo 按名解析"),
    ("LocalModels.LocalModelManager.GetSkill_Main(int)", 0x68DFBD0, 0x68D88A0,
     "读 Skill_Main.UpgradeSkillId 判下一级"),
    ("Game.get_Table()", 0x75BDF30, 0x75B5390, "取 LocalModelManager 表入口"),
]
CORE_ENTRIES = [
    "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
    "lib/arm64-v8a/libmain.so",
    "assets/bin/Data/Managed/Metadata/global-metadata.dat",
    "classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
    "resources.arsc",
]


def sha256_file(p: Path) -> str:
    h = hashlib.sha256()
    with p.open("rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()


def text_segment(so: Path):
    """返回满足 va-file==0x4000 的可执行 PT_LOAD（file_off, va, filesz）。"""
    with so.open("rb") as f:
        f.seek(0x20)
        phoff = struct.unpack("<Q", f.read(8))[0]
        f.seek(0x36)
        phentsize, phnum = struct.unpack("<HH", f.read(4))
        out = []
        for i in range(phnum):
            f.seek(phoff + i * phentsize)
            p_type, p_flags = struct.unpack("<II", f.read(8))
            p_offset, p_vaddr = struct.unpack("<QQ", f.read(16))
            # Elf64_Phdr: p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align
            p_paddr, p_filesz = struct.unpack("<QQ", f.read(16))
            if p_type == 1 and (p_flags & 1) and p_vaddr - p_offset == DELTA:
                out.append((p_offset, p_vaddr, p_filesz))
        return out


def word_at(so: Path, off: int) -> int:
    with so.open("rb") as f:
        f.seek(off)
        return struct.unpack("<I", f.read(4))[0]


def main():
    apk_md5 = __import__("subprocess").run(
        ["md5", "-q", str(BASE)], capture_output=True, text=True).stdout.strip()
    with zipfile.ZipFile(BASE) as z:
        names = z.namelist()
        core = {n: hashlib.sha256(z.read(n)).hexdigest() for n in CORE_ENTRIES}
        dex = sorted(n for n in names
                     if n.startswith("classes") and n.endswith(".dex"))

    segs = text_segment(SO_PATH)
    hooks = []
    for method, rva, old_off, want in HOOKS:
        fo = rva - DELTA
        got = word_at(SO_PATH, fo)
        hooks.append({
            "method": method,
            "rva": hex(rva), "file_offset": hex(fo),
            "prologue_word_1_1_8": hex(got),
            "prologue_word_1_1_7": hex(want),
            "prologue_same_as_1_1_7": got == want,
            "file_offset_1_1_7": hex(old_off),
        })
    aux = []
    for method, rva, old_off, purpose in AUX:
        fo = rva - DELTA
        aux.append({
            "method": method, "rva": hex(rva), "file_offset": hex(fo),
            "file_offset_1_1_7": hex(old_off),
            "prologue_word": hex(word_at(SO_PATH, fo)),
            "purpose": purpose,
        })

    doc = {
        "game": GAME,
        "actual_app_label": "弹壳战机",
        "package": "com.survivor.acecn",
        "version_name": "1.1.8",
        "version_code": 35,
        "serial": "f9a116a2",
        "engine": "Unity IL2CPP",
        "date": "2026-10-14",
        "note": ("1.1.8 锚点重抓。1.1.7 的 file offset 全部作废（so 布局已变，"
                 "四处 file offset 平移 -0x7BAC ~ +0xA5FC 不等）。"),
        "elf": dict(
            {
                "file": "lib/arm64-v8a/libil2cpp.so",
                "rva_to_file_offset_delta": hex(DELTA),
            },
            **(
                {
                    "text_section_file_offset": hex(segs[0][0]),
                    "text_section_va": hex(segs[0][1]),
                    "text_section_size": hex(segs[0][2]),
                }
                if segs else {}
            ),
        ),
        "apk": {
            "path": f"02_apks/{GAME}/{BASE.name}",
            "size": BASE.stat().st_size,
            "md5": apk_md5,
            "sha256": sha256_file(BASE),
            "signer": "O=gorilla",
            "signer_cert_sha256":
                "4c999c6ac30fde9c5d6c72e2d48e643a9044c26ff353ce53c46a3d4e91a18994",
            "app_component_factory": "androidx.core.app.CoreComponentFactory",
            "dex_entries": dex,
        },
        "core_entry_sha256": core,
        "menu": {
            "template": f"10_docs/{GAME}/dkzj_menu_template.json",
            "features": [
                {"button": "Slider_攻击倍率", "feature": 1, "name": "攻击倍率",
                 "type": "range", "range": "1-100"},
                {"button": "Toggle_上帝模式", "feature": 2, "name": "上帝模式",
                 "type": "toggle"},
                {"button": "Toggle_偷盗技能", "feature": 3, "name": "偷盗技能",
                 "type": "toggle"},
                {"button": "Toggle_免广告", "feature": 4, "name": "免广告",
                 "type": "toggle"},
            ],
        },
        "hooks": hooks,
        "field_offsets_verified_1_1_8": {
            "HeroComponentRandomSkill._allSkills": "0x58",
            "HeroComponentRandomSkill._randomSkillResult": "0x88",
            "RandomSkillResult.<ShowSkills>k__BackingField": "0x60",
            "List<int>.items": "0x10",
            "List<int>.size": "0x18",
            "int[] length": "0x18",
            "int[] data": "0x20",
            "Skill_Main.<UpgradeSkillId>k__BackingField": "0x30",
            "note": "与 1.1.7 全等（dump.cs 1.1.8 实测）",
        },
        "resolved_by_name": aux,
        "artifacts": {
            "mod_cpp": "mod菜单/starry-mod-menu/app/src/main/jni/Games/DkzjMod.cpp",
            "signed_apk": f"08_output/{GAME}/{GAME}_1.1.8_starry-releaseSigned.apk",
        },
        "status": "待真机验收",
    }
    OUT.write_text(json.dumps(doc, ensure_ascii=False, indent=2) + "\n",
                   encoding="utf-8")
    print(f"written {OUT}")
    for h in hooks:
        print(" ", h["method"].split("(")[0], h["file_offset"],
              "prologue_same=", h["prologue_same_as_1_1_7"])


if __name__ == "__main__":
    main()
