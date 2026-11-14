#!/usr/bin/env python3
"""战斗合同检查：① 源码常量/字段名/地址口径；② 纯计算边界 fixture。

注意边界：①是静态文本约束（防"改一处忘一处"），②是规则级 fixture；
两者都**不能**替代运行时验证（真机未测）。
"""
import math
import pathlib
import sys

CPP = pathlib.Path("mod菜单/starry-mod-menu/app/src/main/jni/Games/旷野征途Mod.cpp")
t = CPP.read_text(encoding="utf-8")

checks = [
  ("param 索引常量", "constexpr int kIdxAtker = 0;" in t and "constexpr int kIdxDmg   = 2;" in t),
  ("槽位常量", "constexpr int kSlotDmg  = 24;" in t and "constexpr int kSlotThis = 0;" in t),
  ("调用 get_param 用常量", "p_mgetParam(mi, kIdxAtker)" in t and "p_mgetParam(mi, kIdxDmg)" in t),
  ("读槽用常量", "(char*)argBase + kSlotDmg" in t and "(char*)argBase + kSlotThis" in t),
  ("无裸索引残留", "p_mgetParam(mi, 1)" not in t and "p_mgetParam(mi, 3)" not in t),
  ("player backing field", "<player>k__BackingField" in t),
  ("Instance 静态字段", 'class_get_field(mgr, "Instance")' in t),
  ("hook 用 file offset", "0x19a5004u" in t and "0x19a9004u" not in t),
  ("审计/特权未调用", "CombatDamageAudit::" not in t and "PrivilegeService::" not in t and "get_isGod" not in t),
]

CLAMP = -2.1474835e9

def apply_x100(d):
    """镜像 cpp：仅当 d<0 时乘 100，结果 < CLAMP 则钳位；NaN/0/正数原样返回。"""
    if not (d < 0.0):
        return d
    m = d * 100.0
    return CLAMP if m < CLAMP else m

FIXTURES = [
    (-1.0, -100.0),
    (-0.5, -50.0),
    (-1.0e8, CLAMP),
    (-2.0e7, -2.0e9),   # -2e7*100 = -2e9 > CLAMP ⇒ 不钳位（边界外）
    (50.0, 50.0),
    (0.0, 0.0),
    (-0.0, -0.0),
]

def main():
    bad = [n for n, ok in checks if not ok]
    for n, ok in checks:
        print(("PASS " if ok else "FAIL ") + n)
    bad_fx = []
    for src, want in FIXTURES:
        got = apply_x100(src)
        if got != want:
            bad_fx.append((src, got, want))
    nan_out = apply_x100(float("nan"))
    if not math.isnan(nan_out):
        bad_fx.append(("NaN", nan_out, "NaN"))
    print(("PASS " if not bad_fx else "FAIL ") + "FIXTURE x100/clamp boundary (%d cases)" % (len(FIXTURES) + 1))
    if bad_fx:
        print("   mismatches:", bad_fx)
    ok = (not bad) and (not bad_fx)
    print("结论：" + ("一致" if ok else "不一致"))
    return 0 if ok else 1

if __name__ == "__main__":
    sys.exit(main())
