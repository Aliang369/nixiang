#!/usr/bin/env python3.11
"""山海幻境：离线复刻 C++ 拼接规则，核对主 JS 锚点是否仍然命中。

用法：
    python3.11 04_scripts/山海幻境/check_shhj_js_anchors.py <解密后的 assets/main/index.js> \
        [--out <拼接后 JS 路径>] [--quiet]

规则与 `mod菜单/starry-mod-menu/app/src/main/jni/Games/ShhjMod.cpp::SpliceCombatJs`
逐字对应（顺序、ReplaceAll 语义、各步骤的进入条件都照抄），因此本脚本的命中结论
＝ 装机后 `[Shhj] spliced …` 日志的静态预判。任一必需锚点缺失 ⇒ rc=3（禁止出包）。
"""
from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path

ACT = "com.chx.shhj.AppActivity"
BOSS_GUARD = (
    "(_.default.ActivityWorldBoss||_.default.guildBoss||_.default.daoyun||_.default.waterFireBoss)"
)
BOSS_GUARD_SP = (
    "(_.default.ActivityWorldBoss || _.default.guildBoss || _.default.daoyun || "
    "_.default.waterFireBoss)"
)

PREAMBLE = (
    "/*starry_shhj_gxr*/window.__hack_ret=window.__hack_ret||{};"
    "window.__gxc=window.__gxc||{t:{},v:{}};"
    "window.GXR=function(n){if(window.__hack_ret[n])return!0;"
    "var c=window.__gxc,m=Date.now();"
    "if(!c.t[n]||m-c.t[n]>250){c.t[n]=m;try{c.v[n]=!!jsb.reflection."
    f'callStaticMethod("{ACT}","get_ret","(I)Z",n)}}catch(e){{c.v[n]=!1}}}}'
    "return c.v[n];};"
    "window.GXD=function(){return window.GXR(3)?100:1;};"
    "window.GX5=function(m){if(!window.GXR(4)||!m)return 1;"
    "var b=m.ActivityWorldBoss||m.guildBoss||m.daoyun||m.waterFireBoss;"
    "return window.GXR(3)&&!b?1:5;};\n"
)

GET_HURT_FROM = "e.prototype.getHurt=function(t,e,i){"
GET_HURT_TO = (
    "e.prototype.getHurt=function(t,e,i){if(t&&t[2]<0&&t[3]&&"
    "t[3].camp==d.default.SELFCAMP&&this.camp!=d.default.SELFCAMP){"
    "var _m=window.GX5(E.default);_m>1&&(t[2]=Math.floor(_m*t[2]));}"
)
GET_HURT_FROM_SP = "e.prototype.getHurt = function(t, e, i) {"
GET_HURT_TO_SP = (
    "e.prototype.getHurt = function(t, e, i) {if (t && t[2] < 0 && t[3] && "
    "t[3].camp == d.default.SELFCAMP && this.camp != d.default.SELFCAMP) { "
    "var _m = window.GX5(E.default); _m > 1 && "
    "(t[2] = Math.floor(_m * t[2])); }"
)

CUR_HP = (
    "this.curHp+=(f<0&&this.view.camp!=a.default.SELFCAMP&&!"
    + BOSS_GUARD
    + "&&window.GXD()>1?Math.floor(window.GXD()*f):f);"
)
CUR_HP_SP = (
    "this.curHp += (f < 0 && this.view.camp != a.default.SELFCAMP && !"
    + BOSS_GUARD_SP
    + " && window.GXD() > 1 ? Math.floor(window.GXD() * f) : f);"
)

CREATE = (
    "createNum(t[0],t[1],t[2]<0&&!(p.default.ActivityWorldBoss||p.default.guildBoss||"
    "p.default.daoyun||p.default.waterFireBoss)&&window.GXD()>1?Math.floor(window.GXD()*"
    "t[2]):t[2],t[4]"
)
CREATE_SP = (
    "createNum(t[0], t[1], t[2] < 0 && !(p.default.ActivityWorldBoss || "
    "p.default.guildBoss || p.default.daoyun || p.default.waterFireBoss) && "
    "window.GXD() > 1 ? Math.floor(window.GXD() * t[2]) : t[2], t[4]"
)

INV_FROM = "if(this._hasEffectType[r.default.SKILLEFFECTSUBTYPE77]){"
INV_TO = (
    "if(this._hasEffectType[r.default.SKILLEFFECTSUBTYPE77]||"
    "(this.view.camp==a.default.SELFCAMP&&window.GXR(2))){"
)
INV_FROM_SP = "if (this._hasEffectType[r.default.SKILLEFFECTSUBTYPE77]) {"
INV_TO_SP = (
    "if (this._hasEffectType[r.default.SKILLEFFECTSUBTYPE77] || "
    "(this.view.camp == a.default.SELFCAMP && window.GXR(2))) {"
)

BOSS_FROM = "this.curHp+f<=0&&this.view.isBoss"
BOSS_TO = "this.curHp+f*(!" + BOSS_GUARD + "?window.GXD():1)<=0&&this.view.isBoss"
BOSS_FROM_SP = "this.curHp + f <= 0 && this.view.isBoss"
BOSS_TO_SP = (
    "this.curHp + f * (!(_.default.ActivityWorldBoss || _.default.guildBoss || "
    "_.default.daoyun || _.default.waterFireBoss) ? window.GXD() : 1) <= 0 && "
    "this.view.isBoss"
)


def replace_all(s: str, frm: str, to: str) -> tuple[str, int]:
    n = s.count(frm)
    if n:
        s = s.replace(frm, to)
    return s, n


def occurrences(s: str, needles: list[str]) -> dict[str, int]:
    return {n: s.count(n) for n in needles}


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("js", type=Path)
    ap.add_argument("--out", type=Path, default=None)
    ap.add_argument("--quiet", action="store_true")
    args = ap.parse_args()

    if not args.js.is_file():
        print(f"ERROR: JS 不存在: {args.js}", file=sys.stderr)
        return 2
    s = args.js.read_text(encoding="utf-8", errors="replace")
    orig_len = len(s)
    hits: list[str] = []

    # 锚点在原文中的位置（对齐 4.2 文档的「锚点 / 行 / 命中」表）
    lines = s.splitlines()
    for label, needle in (
        ("RoleBase.getHurt 定义", GET_HURT_FROM_SP),
        ("RoleBase.getHurt 定义(紧凑)", GET_HURT_FROM),
        ("curHp += f;", "this.curHp += f;"),
        ("curHp+=f;", "this.curHp+=f;"),
        ("createNum(...)", "createNum(t[0], t[1], t[2], t[4]"),
        ("SKILLEFFECTSUBTYPE77 免伤块", INV_FROM_SP),
        ("world.isBoss 斩杀判定", "this.view.isBoss"),
        ("window.__require", "window.__require"),
    ):
        cnt = s.count(needle)
        first = next((i + 1 for i, ln in enumerate(lines) if needle in ln), 0)
        if not args.quiet:
            print(f"  [锚点] {label}: 命中 {cnt} 处" + (f"，首行 {first}" if first else ""))

    # 1) 主包头注入 GXR/GXD/GX5
    if "starry_shhj_gxr" not in s and "window.__require" in s:
        p = s.find("window.__require")
        s = s[:p] + PREAMBLE + s[p:]
        hits.append("spliced GXR/GXD/GX5 preamble")

    # 2) RoleBase.getHurt 五倍
    if "window.GX5(" not in s:
        s, n = replace_all(s, GET_HURT_FROM, GET_HURT_TO)
        if n:
            hits.append(f"spliced getHurt dmg5 (compact) ×{n}")
        s, n = replace_all(s, GET_HURT_FROM_SP, GET_HURT_TO_SP)
        if n:
            hits.append(f"spliced getHurt dmg5 (spaced) ×{n}")

    # 3) 扣血 curHp
    s, n = replace_all(s, "this.curHp+=f;", CUR_HP)
    if n:
        hits.append(f"spliced curHp (compact) ×{n}")
    s, n = replace_all(s, "this.curHp += f;", CUR_HP_SP)
    if n:
        hits.append(f"spliced curHp (spaced) ×{n}")

    # 4) 跳字 createNum
    s, n = replace_all(s, "createNum(t[0],t[1],t[2],t[4]", CREATE)
    if n:
        hits.append(f"spliced createNum (compact) ×{n}")
    s, n = replace_all(s, "createNum(t[0], t[1], t[2], t[4]", CREATE_SP)
    if n:
        hits.append(f"spliced createNum (spaced) ×{n}")

    # 5) 无敌 tHurt
    if "window.GXR(2)" not in s:
        s, n = replace_all(s, INV_FROM, INV_TO)
        if n:
            hits.append(f"spliced tHurt inv ×{n}")
        s, n = replace_all(s, INV_FROM_SP, INV_TO_SP)
        if n:
            hits.append(f"spliced tHurt inv (spaced) ×{n}")

    # 6) Boss 斩杀辅助倍率（与 C++ 同条件：仅当尚未出现 window.GXD() 时才做）
    if "this.view.isBoss" in s and "window.GXD()" not in s:
        s, n = replace_all(s, BOSS_FROM, BOSS_TO)
        if n:
            hits.append(f"spliced boss kill check ×{n}")
        s, n = replace_all(s, BOSS_FROM_SP, BOSS_TO_SP)
        if n:
            hits.append(f"spliced boss kill check (spaced) ×{n}")

    print(f"  原文 {orig_len} B → 拼接后 {len(s)} B（Δ{len(s) - orig_len:+d}）")
    for h in hits:
        print(f"  HIT  {h}")

    required = {
        "GXR/GXD/GX5 preamble": any("preamble" in h for h in hits),
        "getHurt dmg5": any("getHurt dmg5" in h for h in hits),
        "curHp": any(h.startswith("spliced curHp") for h in hits),
        "tHurt inv": any("tHurt inv" in h for h in hits),
    }
    # 3 个功能：① 无敌(GXR2/tHurt) ② 百倍(GXD/curHp) ③ 五倍(BOSS)(GX5/getHurt)
    missing = [k for k, v in required.items() if not v]
    if missing:
        print(f"  MISS 必需锚点缺失: {', '.join(missing)}", file=sys.stderr)
        return 3

    if args.out:
        args.out.parent.mkdir(parents=True, exist_ok=True)
        args.out.write_text(s, encoding="utf-8")
        print(f"  拼接结果 → {args.out}")
        node = subprocess.run(
            ["node", "--check", str(args.out)], capture_output=True, text=True
        )
        if node.returncode != 0:
            print(f"  node --check FAIL: {node.stderr.strip()[:400]}", file=sys.stderr)
            return 4
        print("  node --check PASS")

    # 运行时判据（装机后 logcat 应出现的行）
    print("  期望运行时日志：")
    for h in hits:
        print(f"    Mod_Menu: [Shhj] {h.split(' ×')[0]}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
