#!/usr/bin/env python3
"""蛋壳战机 国服 1.2.1 (vc44, taptap) 锚点归档。

从 02_apks/蛋壳战机/蛋壳战机_1.2.1_official_vc44_from_emulator.apk 与
06_dumped/蛋壳战机/il2cpp_dump_1.2.1_cn/dump.cs 现场计算哈希、ELF 段与 hook 序言，
生成 10_docs/蛋壳战机/抓点_锚点_1.2.1_CN_20261028.json。

用法：python 04_scripts/蛋壳战机/dkzj_anchor_1.2.1_cn.py
"""
import hashlib
import json
import struct
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "蛋壳战机"
APK = ROOT / "02_apks" / GAME / "蛋壳战机_1.2.1_official_vc44_from_emulator.apk"
STATIC = ROOT / "06_dumped" / GAME / "static_inspect_1.2.1_cn" / "key_files"
SO = STATIC / "libil2cpp.so"
META = STATIC / "global-metadata.dat"
DUMP = ROOT / "06_dumped" / GAME / "il2cpp_dump_1.2.1_cn" / "dump.cs"
OUT = ROOT / "10_docs" / GAME / "抓点_锚点_1.2.1_CN_20261028.json"
DELTA = 0x4000

# (显示名, RVA, 序言 4 字节, 用途, 来源)
HOOKS = [
    ("HotFix.BattleLogic.EntityCharacter.ChangeHp(long)", 0x6D1F118, 0xA9BB6FFE,
     "攻击倍率旧层兜底 / 上帝模式 ChangeHp 路径", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.BattleLogic.EntityHero.ChangeHp(long)", 0x6E17D30, 0xF81D0FFE,
     "上帝模式：英雄扣血入口（t_heroDepth 标记）", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.BattleLogic.HurtBase.FinalizeAttackResult(FP)", 0x6E32404, 0xF81D0FFE,
     "攻击倍率主挂点：FP 16.16 攻击结算 ×N，被击方为 Hero/Follower 时跳过", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.BattleLogic.EntityHero.set_CurrentHp(long)", 0x6E1A390, 0xF81D0FFE,
     "上帝模式补刀：拒绝向下写 CurrentHp；字段 _currentHp=0x1A0", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.BattleLogic.BattleWorldContext.PostProcessLearnedSkill(EntityCharacter,int,Skill_Main,bool,HeroComponentRandomSkill)",
     0x6FDBA08, 0xD10303FF, "偷盗技能：三选一后补授其余候选", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.ADModuleMgr.CheckAndPlayVideo(string,Action<bool>)", 0x78E5148, 0xA9BC67FE,
     "免广告旧层兜底：直接回调 onClose(true)", "1.2.1(vc44) dump.cs 重定位"),
    ("Framework.GameAdsManager.CheckAndPlayVideo(Action<bool>,Action,Action<string>,Func<string>)",
     0x445B058, 0xD10183FF, "免广告框架层：不播广告，反射调 onShow/onClose(true)", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.BattleLogic.RandomSkillResult.set_ShowSkills(List<int>)", 0x6DFB784, 0xF81D0FFE,
     "偷盗技能候选快照：普通三选一", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.AngleViewData.set_ShowSkills(List<int>)", 0x7A55BD8, 0xF81D0FFE,
     "偷盗技能候选快照：角度选择", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.ContractDevilData.set_ShowSkills(List<int>)", 0x7B3568C, 0xF81D0FFE,
     "偷盗技能候选快照：契约恶魔", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.PunchboardViewData.set_ShowSkills(List<int>)", 0x7FBBB1C, 0xF81D0FFE,
     "偷盗技能候选快照：弹珠板", "1.2.1(vc44) dump.cs 重定位"),
    ("HotFix.BattleRecordManager.IsNeedUploadSaveData()", 0x7627018, 0xA9BE57FE,
     "拦截上报常开：返回 false，走 SkippedNotNeeded=2", "1.2.1(vc44) dump.cs 重定位"),
]
AUX = [
    ("HotFix.BattleLogic.BattleWorldContext.AddSkillToCharacter(EntityCharacter,int,bool)",
     0x6FCBBDC, "偷盗技能回灌入口（file offset 直取）"),
    ("LocalModels.LocalModelManager.GetSkill_Main(int)", 0x69C631C,
     "读 Skill_Main 升级链（file offset 直取）"),
    ("HotFix.Game.get_Table()", 0x76FDB58,
     "取 LocalModelManager 表入口（file offset 直取）"),
]
MENU = [
    {"button": "Slider_攻击倍率", "feature": 1, "name": "攻击倍率", "type": "range", "range": "1-100"},
    {"button": "Toggle_上帝模式", "feature": 2, "name": "上帝模式", "type": "toggle"},
    {"button": "Toggle_偷盗技能", "feature": 3, "name": "偷盗技能", "type": "toggle"},
    {"button": "Toggle_免广告", "feature": 4, "name": "免广告", "type": "toggle"},
]


def sha256_file(p: Path) -> str:
    h = hashlib.sha256()
    with p.open("rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()


def md5_file(p: Path) -> str:
    h = hashlib.md5()
    with p.open("rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()


def word_at(so: Path, off: int) -> int:
    with so.open("rb") as f:
        f.seek(off)
        return struct.unpack("<I", f.read(4))[0]


def text_segment(so: Path):
    with so.open("rb") as f:
        f.seek(0x20)
        phoff = struct.unpack("<Q", f.read(8))[0]
        f.seek(0x36)
        phentsize, phnum = struct.unpack("<HH", f.read(4))
        for _ in range(phnum):
            f.seek(phoff)
            p_type, p_flags = struct.unpack("<II", f.read(8))
            p_offset, p_vaddr = struct.unpack("<QQ", f.read(16))
            p_paddr, p_filesz = struct.unpack("<QQ", f.read(16))
            if p_type == 1 and (p_flags & 1) and p_vaddr - p_offset == DELTA:
                return {"file_offset": hex(p_offset), "va": hex(p_vaddr), "size": hex(p_filesz)}
            phoff += phentsize
    return {}


def main():
    with zipfile.ZipFile(APK) as z:
        names = z.namelist()
        core_hashes = {}
        for n in ["lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
                  "lib/arm64-v8a/libmain.so", "assets/bin/Data/Managed/Metadata/global-metadata.dat",
                  "classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "resources.arsc"]:
            core_hashes[n] = hashlib.sha256(z.read(n)).hexdigest()
        dex_entries = sorted(n for n in names if n.startswith("classes") and n.endswith(".dex"))

    hooks = []
    for method, rva, want, purpose, source in HOOKS:
        fo = rva - DELTA
        got = word_at(SO, fo)
        hooks.append({
            "method": method, "rva": hex(rva), "file_offset": hex(fo),
            "prologue_word": hex(got), "expected_prologue": hex(want),
            "prologue_ok": got == want, "purpose": purpose, "source": source,
        })
    aux = []
    for method, rva, purpose in AUX:
        fo = rva - DELTA
        aux.append({"method": method, "rva": hex(rva), "file_offset": hex(fo),
                    "prologue_word": hex(word_at(SO, fo)), "purpose": purpose})

    doc = {
        "game": GAME,
        "actual_app_label": "蛋壳战机",
        "package": "com.survivor.acecn",
        "channel": "taptap",
        "version_name": "1.2.1",
        "version_code": 44,
        "engine": "Unity IL2CPP",
        "date": "2026-10-28",
        "device": "emulator-5554 (雷电 LDPlayer，x86_64 + libhoudini arm64 转译)",
        "note": "国服 1.2.1/vc44 底包（自模拟器 /data/app 只读拉取）锚点全量重定位；"
                "1.1.8 的 file offset 全部作废。实测 12 hook RVA 与 3 aux RVA 同私服 1.2.1/vc35 逐位一致。",
        "apk": {
            "path": "02_apks/蛋壳战机/蛋壳战机_1.2.1_official_vc44_from_emulator.apk",
            "size": APK.stat().st_size,
            "md5": md5_file(APK),
            "sha256": sha256_file(APK),
            "signer": "O=gorilla",
            "signature_sha1_prefix": "89279fb3",
            "app_component_factory": "androidx.core.app.CoreComponentFactory",
            "dex_entries": dex_entries,
            "free_dex_slot": "classes5.dex",
        },
        "elf": {
            "file": "lib/arm64-v8a/libil2cpp.so",
            "sha256": core_hashes["lib/arm64-v8a/libil2cpp.so"],
            "rva_to_file_offset_delta": hex(DELTA),
            "text_segment": text_segment(SO),
        },
        "metadata": {"sha256": core_hashes["assets/bin/Data/Managed/Metadata/global-metadata.dat"],
                     "version": 31},
        "core_entry_sha256": core_hashes,
        "resolved_by_name": aux,
        "field_offsets_verified_1_2_1_cn": {
            "HurtBase.Entity": "0x60",
            "EntityCharacter._currentHp": "0x1A0",
            "HeroComponentRandomSkill._allSkills": "0x58",
            "HeroComponentRandomSkill._randomSkillResult": "0x88",
            "RandomSkillResult.<ShowSkills>k__BackingField": "0x60",
            "Skill_Main.<IsUpgrade>k__BackingField": "0x28",
            "Skill_Main.<UpgradeSkillId>k__BackingField": "0x30",
            "Skill_Main.<MaxStar>k__BackingField": "0x40",
            "Skill_Main.<CoverSkill>k__BackingField": "0x80",
            "Skill_Main.<NextSkill>k__BackingField": "0x84",
            "Skill_Main.<IsUpgrade_B>k__BackingField": "0x14C",
            "Skill_Main.<UpgradeSkillId_B>k__BackingField": "0x150",
            "Skill_Main.<UnlockUpgradeId>k__BackingField": "0x158",
            "Skill_Main.<RepeatSkills>k__BackingField": "0x168",
        },
        "menu": {"template": "10_docs/蛋壳战机/dkzj_menu_template.json", "features": MENU},
        "hooks": hooks,
        "dump": {"dir": "06_dumped/蛋壳战机/il2cpp_dump_1.2.1_cn",
                 "dump_cs": "06_dumped/蛋壳战机/il2cpp_dump_1.2.1_cn/dump.cs",
                 "dump_cs_bytes": DUMP.stat().st_size if DUMP.is_file() else None,
                 "metadata_version": 31},
        "build": {"mk": "Android_dkzj121.mk", "define": "-DDKZJ_TARGET_1_2_1",
                  "game_id": "dkzj"},
        "status": "锚点序言 12/12 静态核验通过；构建/组装/签名/装机详见 星空落地_1.2.1_CN_20261028.md",
    }
    OUT.write_text(json.dumps(doc, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"written {OUT}")
    bad = [h for h in hooks if not h["prologue_ok"]]
    print(f"hooks={len(hooks)} prologue_ok={len(hooks)-len(bad)} bad={len(bad)}")
    for h in hooks:
        print(" ", h["method"].split("(")[0], h["file_offset"], h["prologue_word"],
              "OK" if h["prologue_ok"] else "BAD")
    return 0 if not bad else 1


if __name__ == "__main__":
    sys.exit(main())
