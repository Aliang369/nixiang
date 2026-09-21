# -*- coding: utf-8 -*-
"""离线复刻 CwjhMod.cpp SpliceCombatJs（20260921 版）用于 node --check 预检"""
import sys, re
RET = '"org.cocos2dx.javascript.AppActivity","get_ret","(I)Z",'
def G(i): return 'jsb.reflection.callStaticMethod(%s%d)' % (RET, i)
HP_MULT, SP_MULT = 4, 4
CHC, DODGE = ".6", ".35"

def replace_all(s, frm, to):
    n = s.count(frm)
    return s.replace(frm, to), n

def splice(s):
    log = []
    # ① timer
    a = "_timer() {\nthis.recHp();\nthis.recSp();\n}"
    if a in s:
        rep = "_timer() {\n"
        for _ in range(HP_MULT - 1): rep += "if(%s&&this.sideType==1)this.recHp();\n" % G(0)
        rep += "this.recHp();\n"
        for _ in range(SP_MULT - 1): rep += "if(%s&&this.sideType==1)this.recSp();\n" % G(1)
        rep += "this.recSp();\n}"
        s, n = replace_all(s, a, rep); log.append(("timer", n, len(rep) - len(a)))
    # ② CHC
    a = 'u = t ? this.getPropVal("CHC") + (this.getPropVal("SK_CHC", t.skId.toString()) || 0) + (this.getPropVal("SK_CHC", "0") || 0) : this.getPropVal("CHC");'
    if a in s:
        rep = a + '\nif(%s&&this.sideType==1)u+=%s;' % (G(2), CHC)
        s, n = replace_all(s, a, rep); log.append(("chcA", n, len(rep) - len(a)))
    a = 'let i = e ? e._dmgChc || 0 : this.getPropVal("CHC") || 0;'
    if a in s:
        rep = a + '\nif(%s&&this.sideType==1)i+=%s;' % (G(2), CHC)
        s, n = replace_all(s, a, rep); log.append(("chcB", n, len(rep) - len(a)))
    # ③ CHD
    a = 'm *= t ? Math.max(1, this.getPropVal("CHD") + (this.getPropVal("SK_CHD", t.skId.toString()) || 0) + (this.getPropVal("SK_CHD", "0") || 0) + e) : Math.max(1, this.getPropVal("CHD") + e);'
    if a in s:
        rep = ('m *= (jsb.reflection.callStaticMethod(%s3)&&this.sideType==1?2:1) * (t ? Math.max(1, this.getPropVal("CHD") + '
               '(this.getPropVal("SK_CHD", t.skId.toString()) || 0) + (this.getPropVal("SK_CHD", "0") || 0) + e) : '
               'Math.max(1, this.getPropVal("CHD") + e));') % RET
        s, n = replace_all(s, a, rep); log.append(("chdA", n, len(rep) - len(a)))
    for src, var in (('c *= Math.max(1, i + t);', 'c'), ('o *= Math.max(1, i + t);', 'o')):
        if src in s:
            rep = '%s *= (jsb.reflection.callStaticMethod(%s3)&&this.sideType==1?2:1) * Math.max(1, i + t);' % (var, RET)
            s, n = replace_all(s, src, rep); log.append(("chd%s" % var, n, len(rep) - len(src)))
    # ④ AMR
    for src, var, tgt in (('r = Math.min(Math.max(r, -1), .9);', 'r', 'e'), ('r = Math.min(Math.max(r, -1), .5);', 'r', 'i')):
        if src in s:
            rep = src + '\nif(%s&&%s.sideType==1){%s+=.15;%s=Math.min(%s,.95);}' % (G(4), tgt, var, var, var)
            s, n = replace_all(s, src, rep); log.append(("amr", n, len(rep) - len(src)))
    # ⑤ RES
    for src, tgt in (('a = Math.min(Math.max(a, -1), .9);', 'e'), ('a = Math.min(Math.max(a, -1), .5);', 'i')):
        if src in s:
            rep = src + '\nif(%s&&%s.sideType==1){a*=2;a=Math.min(a,.95);}' % (G(5), tgt)
            s, n = replace_all(s, src, rep); log.append(("res", n, len(rep) - len(src)))
    # ⑥ DODGE
    for src, tgt, var in (('s += e.trigger(skill.TriggerTiming.PreDodge, i, e, s);', 'e', 's'),
                          ('n += i.trigger(skill.TriggerTiming.PreDodge, s, i, n);', 'i', 'n')):
        if src in s:
            rep = src + '\nif(%s&&%s.sideType==1)%s+=%s;' % (G(6), tgt, var, DODGE)
            s, n = replace_all(s, src, rep); log.append(("ddg", n, len(rep) - len(src)))
    # ⑦ ads
    a = 'async adsPlay(t, e, i = "") {'
    if a in s:
        rep = a + ('\nif(jsb.reflection.callStaticMethod(%s7)){this._isPlaying=!0;'
                   'try{let __r=await network.rpcCall(pb.MessageID.C2S_WATCH_ADS_END,'
                   'pb.WatchAdsArg.encode({Type:t,Data:e}));'
                   'if(0==__r.errcode){let __w=pb.WatchAdsReply.decode(__r.payload);'
                   'return {succ:!0,rdata:__w};}}finally{this._isPlaying=!1;}}') % RET
        s, n = replace_all(s, a, rep); log.append(("ads", n, len(rep) - len(a)))
    # ⑧⑨ doDmg
    sigs = [("doDmg(r, a, o, c, h, l, _, d, u, m = !0, g = !1, p = i.Normal, f = !1) {", "r", "o"),
            ("doDmg(r, a, o, c, h, l, _, d, m, u = !0, g = !1, p = i.Normal, f = !1) {", "r", "o"),
            ("doDmg(e, r, a, o, c, h, l, _, d, m = !0, u = !1, g = i.Normal, p = !1) {", "e", "a"),
            ("doDmg(e, n, r, a, o, c, h, l, _, d = !0, m = !1, u = i.Normal) {", "e", "r")]
    for sig, dmg, atk in sigs:
        if sig not in s: continue
        rep = sig
        rep += '\nif(jsb.reflection.callStaticMethod(%s9)&&this.sideType==t.SideType.MY_SIDE){return 0;}' % RET
        rep += ('\nif(jsb.reflection.callStaticMethod(%s8)&&%s&&this.sideType!=%s.sideType&&%s.sideType==t.SideType.MY_SIDE)'
                '{%s=this.getPropVal("CUR_HP")+1;}' % (RET, atk, atk, atk, dmg))
        s, n = replace_all(s, sig, rep); log.append(("doDmg[%s]" % dmg, n, len(rep) - len(sig)))
        break
    return s, log

src = open(sys.argv[1], encoding='utf-8').read()
out, log = splice(src)
for name, n, d in log: print("  %-14s hits=%d delta=%+d" % (name, n, d))
print("total delta = %+d" % (len(out) - len(src)))
open(sys.argv[2], 'w', encoding='utf-8').write(out)
