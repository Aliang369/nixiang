#!/usr/bin/env python3
"""生存33天 — 拼接锚点离线复现器（逐条对齐 Sc33Mod.cpp 结构派生判据）

复刻 `mod菜单/starry-mod-menu/app/src/main/jni/Games/Sc33Mod.cpp` 的四段拼接，对
**任意版本的运行时明文 JS** 做离线预览，用于热更后先验锚点、再谈出包：

    fight    Sc33DeriveFightVars + BuildFightHook
    baseDmg  Sc33DeriveBaseDmgVars + BuildBaseDmgHook
    atk      TryInsertAtkHook（kMulRead + kMulApply 替换锚点）
    viewAtk  TryInsertViewAtkHook（结构保留；历史已知假阳性，不要求命中）

用法（CLI 形状固定，不依赖任何固定目录/临时目录）:
    python3 sim_sc33_splice.py <明文.js> <preview.js>
    node --check <preview.js>

输出行与设备 logcat 的 `[Sc33] ...` 一一对应，便于静态/动态对照：
    [Sc33] fight hook auto em=%s neg=%s opts=%s extra=%s @%zu
    [Sc33] baseDmg hook auto tgt=%s caster=%s @%zu
    [Sc33] atk hook in CalcFinalAtkSpeedValue @%zu
    [Sc33] atk hook in GetViewFightTimeRate @%zu
末尾给 `delta_bytes=` 与 `preview_sha256=`（可跨机比对的复现证据）。

判据（2026-11-27 与 C++ v214 逐条对齐，改 C++ 必须同步改这里）:
  * fight：头 `prototype.FightChangeEntityHp = function(`；形参 5 个且都 ≤3 字符 ident；
    16384 B 窗口内找 `&& <X>.force, ` 且其后紧跟 `<id> = <X> && <X>.trueDmg` → opts=X；
    `= <第3形参> < 0,` 前那个 ident → neg；插入点 = 该 var 语句首个 `;` 之后；
    窗口内首个 `.EntityManager.Ins` 前的 ident → em；extra=第5形参。
  * baseDmg：锚点 `t = u.baseDmg;`，向前 rfind `DamageCommonCalcFinal = function(`，
    距离 ≤20000，形参 9 个，caster=第5、tgt=第6，tgt≠caster。
  * atk：锚点 `e = Math.min(e, i);\nreturn Math.max(e, .01);`，其前 900 B 内须有
    `CalcFinalAtkSpeedValue`；以 kMulRead+kMulApply 替换（44 B → 520 B，+476）。
  * viewAtk：锚点 `i = this.CalcFinalAtkSpeedValue(o, i);\nreturn this.CalcFightFloat(i);`
    （v214 原文已变 `(n, i)` 故 miss —— 历史已知假阳性，仅报告不阻断）。

说明：本脚本**只跑结构派生**，不复刻 C++ 的变体表兜底——变体表是现场兜底，
离线器要回答的是「结构路径还成不成立」；这里 MISS 就等于 C++ 会退化到变体表。
"""
import hashlib
import sys

# ident 判定与 Sc33IsIdentChar / Sc33IsIdent 一致：std::isalnum + `_` + `$`，长度 1~3
IDCHARS = frozenset("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$")
WS_BACK = " \t\n"  # C++ 里 neg 回扫只剥这三个（不含 \r）


def is_ident_char(c):
    return c in IDCHARS


def is_ident(t):
    return 1 <= len(t) <= 3 and all(c in IDCHARS for c in t)


def parse_params(s, open_paren):
    """对齐 Sc33ParseParams：open_paren 指向 `(`；形参无默认值/解构。"""
    if open_paren >= len(s) or s[open_paren] != "(":
        return None
    close = s.find(")", open_paren)
    if close < 0 or close - open_paren > 160:
        return None
    body = s[open_paren + 1:close]
    out = []
    start = 0
    while True:
        c = body.find(",", start)
        t = body[start:] if c < 0 else body[start:c]
        tn = t.strip(" \t\r\n")
        if not tn:
            return None
        out.append(tn)
        if c < 0:
            break
        start = c + 1
    return out


# ---------------- 结构派生：FightChangeEntityHp ----------------

FIGHT_HEAD = "prototype.FightChangeEntityHp = function("
FIGHT_WIN = 16384


def derive_fight(js):
    """对齐 Sc33DeriveFightVars：返回 (insert_pos, em, neg, opts, extra) 或 None。"""
    h = js.find(FIGHT_HEAD)
    if h < 0:
        return None
    ps = parse_params(js, h + len(FIGHT_HEAD) - 1)
    if not ps or len(ps) != 5:
        return None
    for p in ps:
        if not is_ident(p):
            return None
    extra = ps[4]
    dmg = ps[2]          # 第3形参 = 伤害值变量（neg 的 `= <dmg> < 0,` 用它，不写死）

    win = js[h:min(len(js), h + FIGHT_WIN)]

    # 1) opts：`&& <X>.force, ` 后紧接 `<id> = <X> && <X>.trueDmg`
    vp = -1
    opts = ""
    fp = win.find("&& ")
    while fp >= 0:
        b = fp + 3
        e2 = b
        while e2 < len(win) and is_ident_char(win[e2]):
            e2 += 1
        cand = win[b:e2]
        nxt = win.find("&& ", fp + 1)
        if not cand or not is_ident(cand):
            fp = nxt
            continue
        self_pat = cand + ".force, "
        if win[fp + 3:fp + 3 + len(self_pat)] != self_pat:
            fp = nxt
            continue
        after = fp + 3 + len(self_pat)
        id_end = after
        while id_end < len(win) and is_ident_char(win[id_end]):
            id_end += 1
        if id_end == after:
            fp = nxt
            continue
        tail = " = " + cand + " && " + cand + ".trueDmg"
        if win[id_end:id_end + len(tail)] != tail:
            fp = nxt
            continue
        vp = fp
        opts = cand
        break
    if vp < 0:
        return None

    # 2) neg：`= <dmg> < 0,` 之前那个 ident
    np_ = win.find("= " + dmg + " < 0,", vp)
    if np_ < 0:
        return None
    e3 = np_
    while e3 > 0 and win[e3 - 1] in WS_BACK:
        e3 -= 1
    b2 = e3
    while b2 > 0 and is_ident_char(win[b2 - 1]):
        b2 -= 1
    neg = win[b2:e3]
    if not is_ident(neg):
        return None

    # 3) 插入点 = 该 var 语句结束的 `;` 之后（不依赖尾项是哪一个 `= 0`）
    semi = win.find(";", np_)
    if semi < 0:
        return None

    # 4) em：窗口内第一个 `<X>.EntityManager.Ins` 的 X
    e = win.find(".EntityManager.Ins")
    if e < 0:
        return None
    b3 = e
    while b3 > 0 and is_ident_char(win[b3 - 1]):
        b3 -= 1
    em = win[b3:e]
    if not is_ident(em):
        return None

    return (h + semi + 1, em, neg, opts, extra)


def build_fight_hook(em, neg, opts, extra):
    """与 BuildFightHook 逐字节一致（v214 em=h neg=m opts=n extra=o → 937 B）。"""
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

BASEDMG_ANCHOR = "t = u.baseDmg;"
BASEDMG_HEAD = "DamageCommonCalcFinal = function("


def derive_basedmg(js):
    """对齐 Sc33DeriveBaseDmgVars：返回 (insert_pos, tgt, caster) 或 None。"""
    a = js.find(BASEDMG_ANCHOR)
    if a < 0:
        return None
    h = js.rfind(BASEDMG_HEAD, 0, a)
    if h < 0 or a - h > 20000:
        return None
    ps = parse_params(js, h + len(BASEDMG_HEAD) - 1)
    if not ps or len(ps) != 9:
        return None
    for p in ps:
        if not is_ident(p):
            return None
    caster = ps[4]   # 第5形参 = casterEntity
    tgt = ps[5]      # 第6形参 = targetEntity
    if tgt == caster:
        return None
    return (a + len(BASEDMG_ANCHOR), tgt, caster)


def build_basedmg_hook(tgt, caster):
    """与 BuildBaseDmgHook 逐字节一致（v214 tgt=a caster=o → 332 B）。"""
    return (
        "var isChecked_RET_dmg100=jsb.reflection.callStaticMethod("
        '"com.sanqi.jmybt.game37.JMYBTActivity","get_ret","(I)Z",1);'
        "if(isChecked_RET_dmg100&&" + caster + "&&" + caster + ".heroCfg&&" + tgt + "&&" + tgt +
        ".monsterCfg)t*=100;"
        "var isChecked_RET_dmg0=jsb.reflection.callStaticMethod("
        '"com.sanqi.jmybt.game37.JMYBTActivity","get_ret","(I)Z",2);'
        "if(isChecked_RET_dmg0&&" + tgt + "&&" + tgt + ".heroCfg)t=0;"
    )


# ---------------- atk：CalcFinalAtkSpeedValue ----------------

ATK_ANCHOR = "e = Math.min(e, i);\nreturn Math.max(e, .01);"
ATK_MUL_READ = (
    "var _sc33Mul=1;try{"
    "var _g=typeof globalThis!=='undefined'?globalThis:window;"
    "if(_g.__sc33AtkMul>1)_sc33Mul=_g.__sc33AtkMul;"
    "if(typeof cc!=='undefined'&&cc.sys&&cc.sys.localStorage){"
    "var _v=parseInt(cc.sys.localStorage.getItem('sc33_atk_mul'),10);"
    "if(_v>1)_sc33Mul=_v;}"
    "try{var _j=jsb.reflection.callStaticMethod("
    '"com.starry.magic.cocos.Sc33Hooks","getAtkMul","()I");'
    "if(_j>1)_sc33Mul=_j;}catch(_e){}"
    "}catch(_e){}"
)
ATK_MUL_APPLY = (
    "if(_sc33Mul>1&&t&&!t.CheckEntityHaveStatus("
    "l.EnumEntityStatus.bossFakeControl))e*=_sc33Mul;"
)
ATK_HOOK = ATK_MUL_READ + ATK_MUL_APPLY + "return Math.max(e, .01);"


def splice_atk(s):
    """对齐 TryInsertAtkHook。返回 (new_s, info)；info=("hit"|"skip"|"miss", pos, note)。"""
    if "sc33_atk_mul" in s and "CalcFinalAtkSpeedValue" in s:
        return (s, ("skip", -1, "already spliced"))
    pos = s.find(ATK_ANCHOR)
    if pos < 0:
        return (s, ("miss", -1, "anchor min/max not found"))
    fn = s.rfind("CalcFinalAtkSpeedValue", 0, pos)
    if fn < 0 or pos - fn > 900:
        return (s, ("miss", pos, "min/max not in CalcFinalAtkSpeedValue"))
    return (s[:pos] + ATK_HOOK + s[pos + len(ATK_ANCHOR):],
            ("hit", pos, "%d B -> %d B (%+d)" % (len(ATK_ANCHOR), len(ATK_HOOK),
                                                len(ATK_HOOK) - len(ATK_ANCHOR))))


# ---------------- viewAtk：GetViewFightTimeRate ----------------

VIEWATK_ANCHOR = ("i = this.CalcFinalAtkSpeedValue(o, i);\n"
                 "return this.CalcFightFloat(i);")
VIEWATK_HOOK = (
    "i = this.CalcFinalAtkSpeedValue(o, i);"
    "var _sc33Mul=1;try{"
    "if(typeof cc!=='undefined'&&cc.sys&&cc.sys.localStorage){"
    "var _v=parseInt(cc.sys.localStorage.getItem('sc33_atk_mul'),10);"
    "if(_v>1)_sc33Mul=_v;}"
    "try{var _j=jsb.reflection.callStaticMethod("
    '"com.starry.magic.cocos.Sc33Hooks","getAtkMul","()I");'
    "if(_j>1)_sc33Mul=_j;}catch(_e){}"
    "}catch(_e){}"
    "if(_sc33Mul>1&&o)i*=_sc33Mul;"
    "return this.CalcFightFloat(i);"
)


def splice_viewatk(s):
    """对齐 TryInsertViewAtkHook。返回 (new_s, info)；info 同 splice_atk。"""
    if "sc33_atk_mul" in s:
        p = s.find("GetViewFightTimeRate")
        if p >= 0 and s.find("sc33_atk_mul", p) >= 0:
            return (s, ("skip", -1,
                        "already spliced (sc33_atk_mul after GetViewFightTimeRate@%d; "
                        "same as C++ guard)" % p))
    pos = s.find(VIEWATK_ANCHOR)
    if pos < 0:
        return (s, ("miss", -1, "anchor not found (params renamed upstream; known benign "
                       "false-positive)"))
    return (s[:pos] + VIEWATK_HOOK + s[pos + len(VIEWATK_ANCHOR):],
            ("hit", pos, "%d B -> %d B (%+d)" % (len(VIEWATK_ANCHOR), len(VIEWATK_HOOK),
                                                len(VIEWATK_HOOK) - len(VIEWATK_ANCHOR))))


def read_bytes_as_text(path):
    """原文按字节读入，再 latin-1 逐字节映射成 str。

    必须这样做：C++ 侧所有 find/rfind/@%zu 都是**字节偏移**，Python 若按 utf-8 解码，
    索引与 len() 都会变成字符数（v214 明文差 131969），静态偏移就无法与 logcat 对照。
    latin-1 是 1 char = 1 byte 的双射，扫描语义与 C++ 完全一致，且可无损还原字节。
    """
    with open(path, "rb") as f:
        return f.read().decode("latin-1")


def write_utf8_no_translate(path, text):
    """按字节原样落盘：先还原 latin-1 字节，再交给 utf-8 写出（LF 不转 CRLF）。

    原文非法 UTF-8 时退化为 latin-1 写出，保证字节不变（不因编解码改动明文）。
    """
    data = text.encode("latin-1")
    try:
        plain = data.decode("utf-8")
    except UnicodeDecodeError:
        with open(path, "w", encoding="latin-1", newline="") as f:
            f.write(data.decode("latin-1"))
        return
    with open(path, "w", encoding="utf-8", newline="") as f:
        f.write(plain)


def main():
    if len(sys.argv) != 3:
        raise SystemExit(__doc__)
    src_path, out_path = sys.argv[1], sys.argv[2]
    src = read_bytes_as_text(src_path)
    s = src

    # 1) fight（守卫与 C++ 同：已拼过就不重复插）
    if "isChecked_RET_aoeAll" in s:
        print("[fight] skip (already spliced)")
    else:
        f_info = derive_fight(s)
        if not f_info:
            print("[fight] STRUCTURAL MISS (C++ falls back to variant table)")
        else:
            pos, em, neg, opts, extra = f_info
            hook = build_fight_hook(em, neg, opts, extra)
            print("[fight] auto em=%s neg=%s opts=%s extra=%s @%d (hook %d B)"
                  % (em, neg, opts, extra, pos, len(hook)))
            s = s[:pos] + hook + s[pos:]

    # 2) baseDmg
    if ".heroCfg)t=0;" in s:
        print("[baseDmg] skip (already spliced)")
    else:
        b_info = derive_basedmg(s)
        if not b_info:
            print("[baseDmg] STRUCTURAL MISS (C++ falls back to variant table)")
        else:
            pos, tgt, caster = b_info
            hook = build_basedmg_hook(tgt, caster)
            print("[baseDmg] auto tgt=%s caster=%s @%d (hook %d B)"
                  % (tgt, caster, pos, len(hook)))
            s = s[:pos] + hook + s[pos:]

    # 3) atk
    s, (kind, pos, note) = splice_atk(s)
    print("[atk] %s%s %s" % (kind, (" @%d" % pos) if pos >= 0 else "", note))

    # 4) viewAtk（历史已知假阳性；miss 不阻断）
    s, (kind, pos, note) = splice_viewatk(s)
    print("[viewAtk] %s%s %s" % (kind, (" @%d" % pos) if pos >= 0 else "", note))

    write_utf8_no_translate(out_path, s)

    print("delta_bytes=%d" % (len(s) - len(src)))
    print("preview_sha256=%s" % hashlib.sha256(s.encode("latin-1")).hexdigest())
    print("len %d -> %d (%+d)" % (len(src), len(s), len(s) - len(src)))


if __name__ == "__main__":
    main()
