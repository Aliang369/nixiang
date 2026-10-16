#!/usr/bin/env python3
"""生存33天 — 拼接锚点离线复现器（结构派生优先 + 变体表兜底）

复刻 `mod菜单/starry-mod-menu/app/src/main/jni/Games/Sc33Mod.cpp` 里
`Sc33DeriveFightVars` / `Sc33DeriveBaseDmgVars` 的判定逻辑，对**运行时明文 JS**
做拼接预览，用于热更后重定锚点（先离线 OK，再谈出包）。

用法:
    python3 sim_sc33_splice.py <runtime_main.js> <patched_preview.js>
    node --check <patched_preview.js>          # 语法关（31MB <1s）

明文 JS 从哪来（2026-10-17 实测可行，frida 在本机不可用）：
    1. 启动游戏，`adb logcat | grep -m1 "big eval len=.*combat=1"` 命中即
       `adb shell su -c 'kill -STOP <pid>'`（此刻原始 31MB 缓冲一定在内存里）；
    2. 按 `/proc/<pid>/maps` 逐区 `dd if=/proc/<pid>/mem` 拉 rw-p 匿名区（≥16MB），
       本地 grep `prototype.FightChangeEntityHp`，命中区从偏移 0 起
       `len=31206123` 字节即整份原文（该轮实测三份副本按 32MB 对齐）；
    3. `kill -CONT <pid>` 收尾。

2026-10-17 复现结论（热更版 len=31206123）:
    fight : em=h neg=f opts=n extra=o   @9173929
    baseDmg: tgt=a caster=o             @23575647
    +1269 B；node --check SYNTAX OK
"""
import re
import sys


# ---------------- 结构派生：FightChangeEntityHp ----------------

def derive_fight(js):
    """返回 (insert_pos, em, neg, opts, extra) 或 None"""
    head = "prototype.FightChangeEntityHp = function("
    h = js.find(head)
    if h < 0:
        return None
    ps = h + len(head)
    pe = js.find(")", ps)
    if pe < 0 or pe - ps > 120:
        return None
    params = [p.strip() for p in js[ps:pe].split(",")]
    if len(params) != 5 or any(not re.fullmatch(r"[\w$]{1,3}", p) for p in params):
        return None
    opts, extra = params[3], params[4]

    # 函数体内 `var p = X && X.force, d = X && X.trueDmg, Y = i < 0, _ = i > 0, m = 0;`
    body = js[pe:pe + 4000]
    m = re.search(
        r"var p = ([\w$]+) && \1\.force, d = \1 && \1\.trueDmg, ([\w$]+) = i < 0, _ = i > 0, m = 0;",
        body)
    if not m:
        return None
    if m.group(1) != opts:
        return None
    neg = m.group(2)
    anchor_end = pe + m.end()

    # EntityManager 模块别名：函数体内 `<id>.EntityManager.Inst()`
    em = None
    for mm in re.finditer(r"([\w$]+)\.EntityManager\.Inst\(\)", body):
        em = mm.group(1)
        break
    if not em:
        return None
    return (anchor_end, em, neg, opts, extra)


def build_fight_hook(em, neg, opts, extra):
    return (
        "var isChecked_RET_aoeAll=jsb.reflection.callStaticMethod("
        '"com.sanqi.jmybt.game37.JMYBTActivity","get_ret","(I)Z",3);'
        "if(isChecked_RET_aoeAll&&!this._aoeAllEnemiesRunning&&" + neg + "&&t&&t.monsterCfg){"
        "var aoeCaster=" + em + ".EntityManager.Inst().GetEntity(e);"
        "if(aoeCaster&&aoeCaster.heroCfg){this._aoeAllEnemiesRunning=!0;try{"
        "var aoeMap=" + em + ".EntityManager.Inst().entityMap;"
        "aoeMap&&aoeMap.forEach(function(aoeEnt){"
        "if(aoeEnt&&aoeEnt!==t&&!aoeEnt.IsDead&&aoeEnt.monsterCfg)"
        "this.FightChangeEntityHp(aoeEnt,e,i," + opts + "," + extra + ");},this);"
        "}finally{this._aoeAllEnemiesRunning=!1;}}}"
        "var isChecked_RET_dmg100=jsb.reflection.callStaticMethod("
        '"com.sanqi.jmybt.game37.JMYBTActivity","get_ret","(I)Z",1);'
        "var isChecked_RET_dmg0=jsb.reflection.callStaticMethod("
        '"com.sanqi.jmybt.game37.JMYBTActivity","get_ret","(I)Z",2);'
        "var _sc33Fc=" + em + ".EntityManager.Inst().GetEntity(e);"
        "if(isChecked_RET_dmg100&&_sc33Fc&&_sc33Fc.heroCfg&&" + neg + "&&t&&t.monsterCfg)i*=100;"
        "if(isChecked_RET_dmg0&&t&&t.heroCfg&&" + neg + ")i=0;"
    )


# ---------------- 结构派生：DamageCommonCalcFinal ----------------

def derive_basedmg(js):
    """返回 (anchor_end, tgt, caster) 或 None"""
    anchor = "t = u.baseDmg;"
    a = js.find(anchor)
    if a < 0:
        return None
    head = "DamageCommonCalcFinal = function("
    h = js.rfind(head, 0, a)
    if h < 0 or a - h > 20000:
        return None
    ps = h + len(head)
    pe = js.find(")", ps)
    if pe < 0 or pe - ps > 160:
        return None
    params = [p.strip() for p in js[ps:pe].split(",")]
    if len(params) != 9 or any(not re.fullmatch(r"[\w$]{1,3}", p) for p in params):
        return None
    caster, tgt = params[4], params[5]
    return (a + len(anchor), tgt, caster)


def build_basedmg_hook(tgt, caster):
    return (
        "var isChecked_RET_dmg100=jsb.reflection.callStaticMethod("
        '"com.sanqi.jmybt.game37.JMYBTActivity","get_ret","(I)Z",1);'
        "if(isChecked_RET_dmg100&&" + caster + "&&" + caster + ".heroCfg&&" + tgt + "&&" + tgt + ".monsterCfg)t*=100;"
        "var isChecked_RET_dmg0=jsb.reflection.callStaticMethod("
        '"com.sanqi.jmybt.game37.JMYBTActivity","get_ret","(I)Z",2);'
        "if(isChecked_RET_dmg0&&" + tgt + "&&" + tgt + ".heroCfg)t=0;"
    )


def main():
    if len(sys.argv) != 3:
        raise SystemExit(__doc__)
    src = open(sys.argv[1], encoding="utf-8").read()
    out = sys.argv[2]
    s = src

    f = derive_fight(s)
    if f:
        pos, em, neg, opts, extra = f
        hook = build_fight_hook(em, neg, opts, extra)
        print("[fight] auto em=%s neg=%s opts=%s extra=%s @%d (hook %d B)"
              % (em, neg, opts, extra, pos, len(hook)))
        s = s[:pos] + hook + s[pos:]
    else:
        print("[fight] STRUCTURAL MISS")

    b = derive_basedmg(s)
    if b:
        pos, tgt, caster = b
        hook = build_basedmg_hook(tgt, caster)
        print("[baseDmg] auto tgt=%s caster=%s @%d (hook %d B)"
              % (tgt, caster, pos, len(hook)))
        s = s[:pos] + hook + s[pos:]
    else:
        print("[baseDmg] STRUCTURAL MISS")

    open(out, "w", encoding="utf-8").write(s)
    print("len %d -> %d (+%d)" % (len(src), len(s), len(s) - len(src)))


if __name__ == "__main__":
    main()
