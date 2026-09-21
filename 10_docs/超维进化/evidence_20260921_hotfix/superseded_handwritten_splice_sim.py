# -*- coding: utf-8 -*-
"""CwjhMod.cpp::SpliceCombatJs 的离线镜像（与 2026-09-21 二次热更 <0.3.5> 版 C++ 同步）
用途：任何热更档先跑本脚本 → node --check + 比对运行时 splice delta，双闭环后再出包。
用法：python3 splice_sim.py <game.js> <out.js>
"""
import sys

RET = '"org.cocos2dx.javascript.AppActivity","get_ret","(I)Z",'
def G(i): return 'jsb.reflection.callStaticMethod(%s%d)' % (RET, i)

HP_MULT, SP_MULT = 4, 4      # CWJH_HP/SP_TICK_MULT
CHC, DODGE = ".6", ".35"     # CWJH_CHC_BONUS / CWJH_DODGE_BONUS

def rep_all(s, frm, to, log, tag):
    n = s.count(frm)
    if n:
        s = s.replace(frm, to)
        log.append((tag, n, len(to) - len(frm)))
    return s

def splice(s):
    log = []
    # ① 回血/回蓝 ×N（每 tick 己方多跳 N-1 次）
    a = "_timer() {\nthis.recHp();\nthis.recSp();\n}"
    if a in s:
        to = "_timer() {\n"
        for _ in range(HP_MULT - 1): to += "if(%s&&this.sideType==1)this.recHp();\n" % G(0)
        to += "this.recHp();\n"
        for _ in range(SP_MULT - 1): to += "if(%s&&this.sideType==1)this.recSp();\n" % G(1)
        to += "this.recSp();\n}"
        s = rep_all(s, a, to, log, "timer")
    # ② 暴击率 +CHC pp（形态 A/B/C）
    a = ('u = t ? this.getPropVal("CHC") + (this.getPropVal("SK_CHC", t.skId.toString()) || 0) + '
         '(this.getPropVal("SK_CHC", "0") || 0) : this.getPropVal("CHC");')
    s = rep_all(s, a, a + '\nif(%s&&this.sideType==1)u+=%s;' % (G(2), CHC), log, "chcA")
    a = 'let i = e ? e._dmgChc || 0 : this.getPropVal("CHC") || 0;'
    s = rep_all(s, a, a + '\nif(%s&&this.sideType==1)i+=%s;' % (G(2), CHC), log, "chcB")
    # ③ 暴伤 ×2（形态 A / B1 / B2）
    a = ('m *= t ? Math.max(1, this.getPropVal("CHD") + (this.getPropVal("SK_CHD", t.skId.toString()) || 0) + '
         '(this.getPropVal("SK_CHD", "0") || 0) + e) : Math.max(1, this.getPropVal("CHD") + e);')
    s = rep_all(s, a, 'm *= (%s3&&this.sideType==1?2:1) * (t ? Math.max(1, this.getPropVal("CHD") + '
                      '(this.getPropVal("SK_CHD", t.skId.toString()) || 0) + (this.getPropVal("SK_CHD", "0") || 0) + e) : '
                      'Math.max(1, this.getPropVal("CHD") + e));' % RET, log, "chdA")
    s = rep_all(s, 'c *= Math.max(1, i + t);',
                'c *= (%s3&&this.sideType==1?2:1) * Math.max(1, i + t);' % RET, log, "chdB1")
    s = rep_all(s, 'o *= Math.max(1, i + t);',
                'o *= (%s3&&this.sideType==1?2:1) * Math.max(1, i + t);' % RET, log, "chdB2")
    # ④ AMR +15pp（形态 C=0.3.5+ / A / B）
    a = "o = Math.min(Math.max(o, -1), t.StatusDef.getMinEdr(this));"
    s = rep_all(s, a, a + '\nif(%s&&i.sideType==1){o+=.15;o=Math.min(o,t.StatusDef.getMinEdr(this));}' % G(4), log, "amrC")
    for shape, tgt in (("r = Math.min(Math.max(r, -1), .9);", "e"), ("r = Math.min(Math.max(r, -1), .5);", "i")):
        s = rep_all(s, shape, shape + '\nif(%s&&%s.sideType==1){r+=.15;r=Math.min(r,.95);}' % (G(4), tgt), log, "amr")
    # ⑤ EDR ×2（形态 C / A / B）
    a = "c = Math.min(Math.max(c, -1), t.StatusDef.getMinEdr(this));"
    s = rep_all(s, a, a + '\nif(%s&&i.sideType==1){c*=2;c=Math.min(c,t.StatusDef.getMinEdr(this));}' % G(5), log, "resC")
    for shape, tgt in (("a = Math.min(Math.max(a, -1), .9);", "e"), ("a = Math.min(Math.max(a, -1), .5);", "i")):
        s = rep_all(s, shape, shape + '\nif(%s&&%s.sideType==1){a*=2;a=Math.min(a,.95);}' % (G(5), tgt), log, "res")
    # ⑥ 闪避 +DODGE pp（形态 A / B）
    a = "s += e.trigger(skill.TriggerTiming.PreDodge, i, e, s);"
    s = rep_all(s, a, a + '\nif(%s&&e.sideType==1)s+=%s;' % (G(6), DODGE), log, "ddgA")
    a = "n += i.trigger(skill.TriggerTiming.PreDodge, s, i, n);"
    s = rep_all(s, a, a + '\nif(%s&&i.sideType==1)n+=%s;' % (G(6), DODGE), log, "ddgB")
    # ⑦ 免广告
    a = 'async adsPlay(t, e, i = "") {'
    if a in s:
        to = a + ('\nif(%s7){this._isPlaying=!0;try{let __r=await network.rpcCall(pb.MessageID.C2S_WATCH_ADS_END,'
                  'pb.WatchAdsArg.encode({Type:t,Data:e}));if(0==__r.errcode){let __w=pb.WatchAdsReply.decode(__r.payload);'
                  'return {succ:!0,rdata:__w};}}finally{this._isPlaying=!1;}}') % RET
        s = rep_all(s, a, to, log, "ads")
    # ⑧⑨ doDmg 入口：前缀 + 下一个 ") {"
    PREFIX = "doDmg(r, a, o, c, h, l, _, d, u, m = !0"
    legacy = [("doDmg(r, a, o, c, h, l, _, d, m, u = !0, g = !1, p = i.Normal, f = !1) {", "r", "o"),
              ("doDmg(e, r, a, o, c, h, l, _, d, m = !0, u = !1, g = i.Normal, p = !1) {", "e", "a"),
              ("doDmg(e, n, r, a, o, c, h, l, _, d = !0, m = !1, u = i.Normal) {", "e", "r")]
    def guard(dmg, atk):
        # 必须与 C++ 的 "if(jsb.reflection.callStaticMethod(" CWJH_RET "9)&&…" 逐字节一致：
        # 漏掉 ")(" 包裹时 node --check 仍会通过（`if("a","b",9&&x){…}` 是合法逗号表达式），
        # 语义却退化成「无敌无条件常开」——20260921 实测踩过，靠 byte-delta 闭环才暴露。
        return ('\nif(jsb.reflection.callStaticMethod(%s9)&&this.sideType==t.SideType.MY_SIDE){return 0;}' % RET +
                '\nif(jsb.reflection.callStaticMethod(%s8)&&%s&&this.sideType!=%s.sideType&&%s.sideType==t.SideType.MY_SIDE){%s=this.getPropVal("CUR_HP")+1;}'
                % (RET, atk, atk, atk, dmg))
    p = s.find(PREFIX)
    if p >= 0 and (s.find(") {", p) - p) < 220:
        e = s.find(") {", p)
        s = rep_all(s, s[p:e + 3], s[p:e + 3] + guard("r", "o"), log, "doDmg[prefix]")
    else:
        for anchor, dmg, atk in legacy:
            if anchor in s:
                s = rep_all(s, anchor, anchor + guard(dmg, atk), log, "doDmg[%s]" % dmg)
                break
    return s, log

src_path, out_path = sys.argv[1], sys.argv[2]
raw = open(src_path, 'rb').read()
src = raw.decode('utf-8')
out, log = splice(src)
for name, n, d in log:
    print("  %-16s hits=%d delta=%+d" % (name, n, d))
print("total delta = %+d (bytes %+d)" % (len(out) - len(src), len(out.encode('utf-8')) - len(raw)))
open(out_path, 'w', encoding='utf-8').write(out)
