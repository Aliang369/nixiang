#!/usr/bin/env python3.11
"""Static regression checks for generated YsxzPatchJs.h.

用法: python3.11 verify_ysxz_patch_js.py [YsxzPatchJs.h 路径]
默认校验 1.0.4 主线那份(mod菜单/.../jni/Games/YsxzPatchJs.h)。
1.0.3 专用那份要显式传: 04_scripts/勇士之心/ysxz_1.0.3_menu/YsxzPatchJs.h
"""
from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DEFAULT_HEADER = ROOT / "mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzPatchJs.h"
HEADER = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else DEFAULT_HEADER
START = 'R"YSXZJS('
END = ')YSXZJS";'


def main() -> int:
    print(f"header: {HEADER}")
    text = HEADER.read_text(encoding="utf-8")
    start = text.find(START)
    end = text.rfind(END)
    if start < 0 or end < 0 or end <= start:
        raise SystemExit("raw JS payload not found")
    js = text[start + len(START):end]
    if js.count("MOD.feature4") != 1:
        raise SystemExit("feature4 occurrences changed")
    if "if (MOD.speed > 1) {" not in js:
        raise SystemExit("movement speed does not use slider value")
    if "if (MOD.feature5)" not in js or "AUTO_BATTLE" not in js:
        raise SystemExit("auto-battle privilege hook missing")
    # 2026-09-21 热更修复：自动战斗指纹不得再写死被重命名的 buff 管理器方法名
    if 'indexOf("bVs(")' in js:
        raise SystemExit("auto-battle fingerprint still hardcodes the renamed bVs()")
    if 'indexOf("BuffTableEnum")' not in js:
        raise SystemExit("auto-battle plaintext fingerprint BuffTableEnum missing")
    if "auto-battle candidate" not in js:
        raise SystemExit("auto-battle reject diagnostic missing")
    if "restoreCosts" not in js:
        raise SystemExit("skill-cost exception fallback missing")
    if "try { touched[j][0].hTimeScale" not in js:
        raise SystemExit("movement restore exception guard missing")
    # feature6 伤害倍率：滑块值经 MOD.damage 读取，且不得吃掉秒杀开关的独立把关
    if 'Object.defineProperty(M, "damage"' not in js:
        raise SystemExit("damage-multiplier shim getter missing")
    if "if (_dm > 1) d = d * _dm;" not in js:
        raise SystemExit("damage-multiplier not applied to final damage formula")
    if "if (_f1) d = Math.ceil(d * 100);" not in js:
        raise SystemExit("kill switch no longer gated by MOD.feature1")
    if js.count("MOD.feature1") != 2:
        raise SystemExit("MOD.feature1 occurrences changed (hook + report expected)")
    # 菜单下标一致性（2026-09-30 换位后新增）：SHIM 里的数字序号 = 菜单 featureList 下标，
    # 序号与控件类型任一错位都会让功能串号（JS 按序号取值），必须在生成期机器拦。
    entry = ROOT / "mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzEntry.cpp"
    cpp = entry.read_text(encoding="utf-8")
    m = re.search(r"kYsxz_Features\[\]\s*=\s*\{(.*?)\};", cpp, re.S)
    if not m:
        raise SystemExit("kYsxz_Features[] not found in YsxzEntry.cpp")
    feats = re.findall(r'"([^"]+)"', m.group(1))

    def menu_index(keyword: str) -> int:
        for i, name in enumerate(feats):
            if keyword in name:
                return i
        raise SystemExit(f"menu entry containing {keyword!r} missing in kYsxz_Features")

    shim = {
        name: (fn, int(num))
        for name, fn, num in re.findall(
            r'Object\.defineProperty\(M, "([A-Za-z0-9_]+)", \{ get: function \(\) \{ return ([RV])\((\d+)\); \} \}\)',
            js,
        )
    }
    expected = {
        "feature1": ("R", menu_index("秒杀")),
        "feature2": ("R", menu_index("无敌")),
        "feature3": ("R", menu_index("无限技能")),
        "feature5": ("R", menu_index("自动战斗")),
        "speed": ("V", menu_index("移动速度")),
        "damage": ("V", menu_index("伤害倍率")),
    }
    for prop, want in expected.items():
        got = shim.get(prop)
        if got != want:
            raise SystemExit(f"shim {prop} -> {got}, menu index/type {want} (换位后序号未同步)")
    if shim.get("feature4") != ("R", 0):
        raise SystemExit("feature4 placeholder (R(0)) changed")
    for prop, (fn, _num) in shim.items():
        if prop in ("speed", "damage") and fn != "V":
            raise SystemExit(f"{prop} 是滑块却用 {fn} 读取（滑块必须 getVal）")
        if prop.startswith("feature") and fn != "R":
            raise SystemExit(f"{prop} 是开关却用 {fn} 读取（开关必须 getRet）")
    proc = subprocess.run(
        ["node", "--check", "/dev/stdin"],
        input=js,
        text=True,
        capture_output=True,
    )
    if proc.returncode != 0:
        raise SystemExit("node --check failed:\n" + proc.stderr)
    if not re.search(r"function tryAuto\(Cls\)", js):
        raise SystemExit("tryAuto implementation missing")
    print("verify_ysxz_patch_js: OK")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
