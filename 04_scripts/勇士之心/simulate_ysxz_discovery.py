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
    names: dict = {}
    order: list[tuple[str, str]] = []
    for rel in bundles:
        p = root / rel
        if not p.exists():
            out["bundles"][rel] = {"error": "missing"}
            continue
        mods = extract_modules(p.read_text(encoding="utf-8", errors="replace"))
        out["bundles"][rel] = {"modules": len(mods)}
        for name, src in mods:
            order.append((f"{rel}::{name}", src))

    # 1) harvestA / harvestB（按注册顺序，命中即锁定）
    for key, src in order:
        if "hx" not in names:
            m = RE_ISPLAYER.search(src)
            if m:
                names.update(hx=m.group(1), isPlayer=m.group(2), hx_mod=key)
        if "hD" not in names:
            m = RE_CAMP.search(src)
            if m:
                names.update(hD=m.group(1), camp=m.group(2), hD_mod=key)
        if "ctxKey" not in names:
            m = RE_CTX.search(src)
            if m:
                names.update(ctxKey=m.group(1), teams=m.group(2), teamCamp=m.group(3), ctx_mod=key)
        if "hp" not in names:
            m = RE_HP.search(src)
            if m:
                names.update(hp=m.group(2), hp_mod=key)
    if "ctxKey" in names and "world" not in names:
        for key, src in order:
            if "lockMinHP" not in src:
                continue
            re_world = re.compile(r"this\.([A-Za-z_$][\w$]*)\." + names["ctxKey"] + r"\.")
            for m in re_world.finditer(src):
                if m.group(1) != "context":
                    names.update(world=m.group(1), world_mod=key)
                    break
            if "world" in names:
                break
    out["names"] = names

    # 2) 目标模块 = 源码含 4 个锚点之一的模块
    targets = [(k, s) for k, s in order if any(a in s for a in ANCHORS)]
    out["target_modules"] = [k for k, _ in targets]

    # 3) tryLogic / tryEntity / trySystem：运行时对目标模块的“导出函数”逐个 findFn。
    #    离线拿不到 exports，用模块内函数体集合（超集）近似，粒度仍是函数级。
    found: dict = {}
    for key, src in targets:
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
