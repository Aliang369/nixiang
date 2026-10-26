#!/usr/bin/env python3.11
"""勇士之心：离线复现星光 JS 注入体的“指纹发现”逻辑。

用途：热更/换版本后，在重建 so 之前先确认 MOD_boot 能否在离线解密产物里
发现全部钩子。复现范围与 10_docs/勇士之心/斌哥注入脚本_提取_999999_20260915.js
（以及 gen_ysxz_patch_header.py 的 SHIM/TAIL 变体）保持一致：

  1) harvestA / harvestB 的明文锚点正则
  2) 目标模块筛选：maxApDefRate | lockMinHP | hitPauseTimeScale | AUTO_BATTLE
  3) tryLogic：伤害公式 / 技能消耗 / 技能门 / 死亡例程
  4) tryEntity：唯一扣血口
  5) trySystem：移动系统 update
  6) tryAuto：AUTO_BATTLE 金卡特权

用法：
  python3.11 simulate_ysxz_discovery.py <decrypted_bundle_dir> [--auto-fp OLD|NEW]
"""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

MOD_SPLIT = re.compile(r"\n([A-Za-z_$][\w$]*): \[ function\((\w+), (\w+), (\w+)\) \{")
ANCHORS = ("maxApDefRate", "lockMinHP", "hitPauseTimeScale", "AUTO_BATTLE")
# 2026-10-26：采集阶段的“战斗明文”锚点集（只用于排序，不用于筛掉模块）
HANCHORS = ("maxApDefRate", "baseCritDamage", "atkEntity", "lockMinHP",
            "hitPauseTimeScale", "isPlayerCamp")
RE_APPLY = re.compile(r"(\w+)\.(\w+)\s*=\s*Math\.max\(\1\.\2\s*-\s*\w+,\s*\1\.lockMinHP\)")
RE_ISPLAYER = re.compile(r'case\s*["\']isPlayer["\']\s*:\s*return\s+\w+\.(\w+)\.(\w+)\s*[;,}]')
RE_CAMP = re.compile(r"\w+\.(\w+)\.(\w+)\s*===\s*\w+\s*!==\s*\w+\.isPlayerCamp")
RE_CTX = re.compile(r"this\.context\.(\w+)\.(\w+)\[0\]\.(\w+)")
RE_HP = re.compile(r"(\w+)\.(\w+)\s*=\s*Math\.max\(\1\.\2\s*-\s*\w+,\s*\1\.lockMinHP\)")
RE_SKILL_CHARGE = re.compile(r"\w+\.\w+\s*>\s*0\s*\)\s*\w+\.\w+\s*-=\s*1")
RE_CD = re.compile(r"\w+\.(\w+)\s*=\s*\w+\.maxCdTime")
RE_DA = re.compile(r"if\s*\(\s*\w+\.(\w+)\s*\)\s*\{\s*\w+\.\w+\s*=\s*Math\.max\(0,")
# 2026-10-06: 游戏包属性初始化方法 .bJV 也含 maxApDefRate+baseCritDamage(AttributeParamTable),
# 只按前两个明文会被它抢占 -> 加 atkEntity(本作明文字段, 真公式 .GD 必含)。
RE_KILL_FP = lambda s: ("maxApDefRate" in s and "baseCritDamage" in s and "atkEntity" in s)
RE_AUTO_OLD = lambda s: "AUTO_BATTLE" in s and "bVs(" in s and len(s) < 600
RE_AUTO_NEW = lambda s: ("AUTO_BATTLE" in s and "BuffTableEnum" in s and "return" in s
                         and len(s) < 600)


def extract_modules(text: str) -> list[tuple[str, str]]:
    """按 `<id>: [ function(a,b,c) {` 切出模块工厂函数源码（花括号配平、跳过字符串/注释）。"""
    mods: list[tuple[str, str]] = []
    for m in MOD_SPLIT.finditer(text):
        name = m.group(1)
        start = text.index("{", m.end() - 1)
        i = start
        depth = 0
        n = len(text)
        while i < n:
            ch = text[i]
            if ch == '"' or ch == "'":
                q = ch
                i += 1
                while i < n:
                    if text[i] == "\\":
                        i += 2
                        continue
                    if text[i] == q:
                        break
                    i += 1
            elif ch == "/" and i + 1 < n and text[i + 1] == "/":
                j = text.find("\n", i)
                i = n if j < 0 else j
            elif ch == "/" and i + 1 < n and text[i + 1] == "*":
                j = text.find("*/", i + 2)
                i = n if j < 0 else j + 1
            elif ch == "{":
                depth += 1
            elif ch == "}":
                depth -= 1
                if depth == 0:
                    break
            i += 1
        mods.append((name, text[start:i + 1]))
    return mods


def find_fn(obj_src: str, test) -> bool:
    """obj_src 是模块源码；这里只判断该模块里是否存在满足指纹的函数。"""
    return test(obj_src)


def simulate(root: Path, auto_mode: str = "NEW") -> dict:
    bundles = ["assets/battleLogic/index.js", "assets/game/index.js",
               "assets/battleRender/index.js", "assets/main/index.js"]
    auto_test = RE_AUTO_NEW if auto_mode == "NEW" else RE_AUTO_OLD
    out: dict = {"root": str(root), "auto_fp": auto_mode, "bundles": {}}
    # ★ 2026-10-26（res 1.1.10.0 实机取证）：运行时 MOD_boot 的 N 是 **per-bundle** 的
    #   （每个包调一次 MOD_boot 就新建一份），且采集范围已从“只含伤害公式三指纹的模块”放宽到
    #   “本包全部模块，含战斗明文的排前面”。这里必须照着做，否则又会得出
    #   “camp/hp/ctx/world 全 undefined ⇒ isMine 恒 false” 的假结论（四项功能因此被判死）。
    mods_by_bundle: dict[str, list[tuple[str, str]]] = {}
    for rel in bundles:
        pth = root / rel
        if not pth.exists():
            out["bundles"][rel] = {"error": "missing"}
            continue
        mods = extract_modules(pth.read_text(encoding="utf-8", errors="replace"))
        out["bundles"][rel] = {"modules": len(mods)}
        mods_by_bundle[rel] = mods

    def harvest(mods: list[tuple[str, str]]) -> dict:
        n: dict = {}
        anchored = [s for _, s in mods if any(a in s for a in HANCHORS)]
        rest = [s for _, s in mods if not any(a in s for a in HANCHORS)]
        for src in anchored + rest:
            if "hx" not in n:
                m = RE_ISPLAYER.search(src)
                if m:
                    n.update(hx=m.group(1), isPlayer=m.group(2))
            if "hD" not in n:
                m = RE_CAMP.search(src)
                if m:
                    n.update(hD=m.group(1), camp=m.group(2))
            if "ctxKey" not in n:
                m = RE_CTX.search(src)
                if m:
                    n.update(ctxKey=m.group(1), teams=m.group(2), teamCamp=m.group(3))
            if "hp" not in n:
                m = RE_HP.search(src)
                if m:
                    n.update(hp=m.group(2))
        if "ctxKey" in n and "world" not in n:
            for src in anchored + rest:
                for pat in (re.compile(r"this\.([A-Za-z_$][\w$]*)\." + n["ctxKey"] + r"\."),
                            re.compile(r"[\w$)\]]\.([A-Za-z_$][\w$]*)\." + n["ctxKey"] + r"\.")):
                    for m in pat.finditer(src):
                        if m.group(1) != "context":
                            n["world"] = m.group(1)
                            break
                    if "world" in n:
                        break
                if "world" in n:
                    break
        return n

    # 运行时“发现名”的取值者 = 战斗包那一次 MOD_boot（钩子全在这里装）
    names = harvest(mods_by_bundle.get(bundles[0], []))
    out["names_scope"] = bundles[0]
    for rel in bundles:
        if rel in mods_by_bundle:
            out["bundles"][rel]["names"] = harvest(mods_by_bundle[rel])
    out["names"] = names

    # 2) 目标模块（运行时 = 源码含 maxApDefRate|lockMinHP|hitPauseTimeScale|AUTO_BATTLE）
    #    与 3) 指纹发现，都在**战斗包自己**的模块集合里做（与运行时共用同一份 N）。
    targets = [(f"{bundles[0]}::{k}", s) for k, s in mods_by_bundle.get(bundles[0], [])
               if any(a in s for a in ANCHORS)]
    out["target_modules"] = [k for k, _ in targets]

    found: dict = {}
    # 运行时每个包各自 scan 自己的 exports，用自己那份 N；这里同样逐包跑，再合并结果
    all_targets: list[tuple[str, str, dict]] = []
    for rel in bundles:
        if rel not in mods_by_bundle:
            continue
        bn = harvest(mods_by_bundle[rel])
        for k, s in mods_by_bundle[rel]:
            if any(a in s for a in ANCHORS):
                all_targets.append((f"{rel}::{k}", s, bn))
    out["target_modules"] = [k for k, _, _ in all_targets]
    for key, src, names in all_targets:
        bodies = extract_function_bodies(src)
        if "kill_formula" not in found and any(RE_KILL_FP(b) for b in bodies):
            found["kill_formula"] = key
        if "skill_cost" not in found:
            for b in bodies:
                if "maxCdTime" in b and RE_SKILL_CHARGE.search(b):
                    m_cd = RE_CD.search(b)
                    m_da = RE_DA.search(b)
                    found["skill_cost"] = {"mod": key, "cd": m_cd.group(1) if m_cd else None,
                                           "Da": m_da.group(1) if m_da else None}
                    break
        if "skill_gate" not in found and isinstance(found.get("skill_cost"), dict):
            cd = found["skill_cost"].get("cd")
            if cd:
                re_gate = re.compile(r"\w+\.\w+\s*>\s*0\s*\?\s*\w+\.\w+\s*<=\s*0\s*:\s*\w+\." + cd)
                for b in bodies:
                    if re_gate.search(b):
                        m_dc = re.search(r"null\s*==\s*\w+\s*\|\|\s*!\w+\.(\w+)", b)
                        found["skill_gate"] = {"mod": key, "unlocked": m_dc.group(1) if m_dc else None}
                        break
        if "death_route" not in found and names.get("hp"):
            re_hp0 = re.compile(r"\.\s*" + names["hp"] + r"\s*=\s*0\s*;")
            for b in bodies:
                if "hitDirType" in b and re_hp0.search(b):
                    found["death_route"] = key
                    break
        if "damage_apply" not in found:
            for b in bodies:
                if "finalHurtValue" in b and RE_APPLY.search(b):
                    found["damage_apply"] = key
                    break
        if "move_system" not in found:
            for b in bodies:
                if ("worldTimeScale" in b and "hitPauseTimeScale" in b and "hTimeScale" in b):
                    found["move_system"] = key
                    break
        if "auto" not in found and any(auto_test(b) for b in bodies):
            found["auto"] = key
    out["found"] = found
    need = ["kill_formula", "skill_cost", "skill_gate", "death_route", "damage_apply",
            "move_system", "auto"]
    out["missing"] = [k for k in need if k not in found]
    out["verdict"] = "OK" if not out["missing"] else "MISSING:" + ",".join(out["missing"])
    return out


def extract_function_bodies(src: str) -> list[str]:
    """粗粒度抽出模块内的所有 function(...) {...} 体（含 prototype 方法）。

    注意：这是模块源码里的函数体集合，是“模块导出函数”的超集，只用于回答
    “指纹依赖的代码结构是否还在”，不等价于运行时 findFn(exports) 的结果；
    运行时判定仍以设备 logcat 的 [MODMOD] REPORT 行为准。
    """
    out = []
    for m in re.finditer(r"function\s*\([^)]*\)\s*\{", src):
        start = src.index("{", m.end() - 1)
        i, depth, n = start, 0, len(src)
        while i < n:
            ch = src[i]
            if ch == '"' or ch == "'":
                q = ch
                i += 1
                while i < n:
                    if src[i] == "\\":
                        i += 2
                        continue
                    if src[i] == q:
                        break
                    i += 1
            elif ch == "/" and i + 1 < n and src[i + 1] == "/":
                j = src.find("\n", i)
                i = n if j < 0 else j
            elif ch == "/" and i + 1 < n and src[i + 1] == "*":
                j = src.find("*/", i + 2)
                i = n if j < 0 else j + 1
            elif ch == "{":
                depth += 1
            elif ch == "}":
                depth -= 1
                if depth == 0:
                    break
            i += 1
        out.append(src[start:i + 1])
    return out


def fingerprint_stats(root: Path, auto_mode: str = "NEW") -> dict:
    """按指纹统计“命中函数体”的数量，用于和上一版抓点做对照。"""
    auto_test = RE_AUTO_NEW if auto_mode == "NEW" else RE_AUTO_OLD
    tests = {
        "kill_formula": RE_KILL_FP,
        "skill_cost": lambda s: "maxCdTime" in s and RE_SKILL_CHARGE.search(s) is not None,
        "skill_gate_hint": lambda s: "maxCdTime" in s and "? " in s,
        "death_route": lambda s: "hitDirType" in s and "= 0 ;" in s.replace("= 0;", "= 0 ;"),
        "damage_apply": lambda s: "finalHurtValue" in s and RE_APPLY.search(s) is not None,
        "move_system": lambda s: ("worldTimeScale" in s and "hitPauseTimeScale" in s
                                  and "hTimeScale" in s),
        "auto_battle": auto_test,
    }
    report: dict = {"root": str(root), "auto_fp": auto_mode, "bundles": {}}
    for rel in ("assets/battleLogic/index.js", "assets/game/index.js",
                "assets/battleRender/index.js", "assets/main/index.js"):
        p = root / rel
        if not p.exists():
            continue
        text = p.read_text(encoding="utf-8", errors="replace")
        bodies = []
        for _, mod_src in extract_modules(text):
            bodies.extend(extract_function_bodies(mod_src))
        hits = {}
        for name, test in tests.items():
            n = sum(1 for b in bodies if test(b))
            hits[name] = n
        report["bundles"][rel] = {"function_bodies": len(bodies), "hits": hits}
    return report


def main() -> int:
    if len(sys.argv) < 2:
        raise SystemExit(__doc__)
    root = Path(sys.argv[1])
    auto_mode = "NEW"
    if "--auto-fp" in sys.argv:
        auto_mode = sys.argv[sys.argv.index("--auto-fp") + 1].upper()
    rep = simulate(root, auto_mode)
    if "--fingerprints" in sys.argv:
        rep["fingerprint_stats"] = fingerprint_stats(root, auto_mode)
    print(json.dumps(rep, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
