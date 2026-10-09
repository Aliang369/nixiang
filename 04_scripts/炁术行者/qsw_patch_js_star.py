# -*- coding: utf-8 -*-
# 炁术行者（com.h5wrapper.app）MOD 补丁生成器 —— 星光版（主 JS 侧）
#
# 作用：把服务端下发的原始玩法主 JS（/assets/main/index.js）改成本地版，
#       由包内 MainActivity$2.shouldInterceptRequest 拦截替换后喂给 WebView。
#
# 职责边界（星光框架接入后）
#   · 本文件只负责「机制」：在 3 个伤害/受击口径上挂 window.__QSW_DMG，
#     并提供 window.__QSW_APPLY(k, drPct) 供框架侧补丁投值。
#   · 默认值 = 原版（k=1, dr=0）：**未授权时游戏与官方完全一致**。
#   · 授权门控与取值在 mod菜单/starry-mod-menu/app/src/main/assets/starry/qsxz_patch.js，
#     由 h5box.StarryH5 注入并把菜单状态投给 __QSW_APPLY。
#   · 无任何外部服务：无远程配置 XHR、无回传 beacon。
#
# 补丁主体（①~⑪）与该游戏已验证通过的 v8 版一致。
#
# 锚点版本：2026-10-08 复刻。服务端 /assets/main/index.js 被重新压缩
#   （5,414,360 B，md5 67df56a68137b11aef65f1229bdcef70），压缩器改名/改写导致
#   ⑥⑦b⑧⑪b⑨ 五处锚点命中 0 次，已按新源码逐处重导；①②③④⑦⑩ 原锚点仍唯一命中。
#   重导原则：只改「锚点字面量」，不改补丁语义；每处仍必须命中恰好 1 次。
#   将来再次失配时脚本会打印「命中 N 次（期望 1）」并 exit 1 —— 按该处上下文重导锚点，
#   **不要把断言放宽成 ≥1**（放宽会把补丁落到未经验证的位置）。
#
# 用法：
#   python3 qsw_patch_js_star.py --src <原始 main/index.js> --dst <输出 main.js>

import argparse
import hashlib
import re
import sys

HEADER = r'''/*QSW-MOD v10s 星光版*/
!function(){
var w=("undefined"!=typeof window)?window:this;
if(w.__QSW_STAR_INSTALLED)return;
w.__QSW_STAR_INSTALLED=!0;

/* ===== 默认 = 原版：只有框架侧补丁投值后才会偏离官方口径 ===== */
w.__QSWK=1;          /* 伤害倍率（1 = 原版） */
w.__QSW_DR=0;        /* 减伤比例（0 = 原版；受击只吃 1-dr） */
w.__QSW_INV=!1;      /* 真无敌：关闭（服务端 9988 封死，不提供开关） */
w.__QSW_HR=0;
w.__QSW_PS_K=0;      /* 战力放大：恒不启用 */
w.__QSW_DN=0;
w.__QSW_HN=0;
w.__QSW_PSN=0;
w.__QSW_LOG=function(){};            /* 无回传、无外部服务 */
w.__QSW_PSF=function(v){return v};   /* 战力恒等（战力完全不动） */
w.__QSW_NETSKIP={9998:1,9999:1,9997:1};  /* 静默的 netcode（账号被标后反复弹「参数传输遇到问题」） */
w.__QSW_NETGUARD=function(e){return !0===w.__QSW_NETSKIP[e]};
w.__QSW_APPLY=function(k,drPct){
  k=(k|0); if(k<1)k=1; else if(k>5)k=5;
  drPct=(drPct|0); if(drPct<0)drPct=0; else if(drPct>50)drPct=50;
  w.__QSWK=k; w.__QSW_DR=drPct/100;
};

w.__QSW_DMG=function(v,c){
  if(1===c){ if(w.__QSW_DR>0)return Math.max(1,Math.round(v*(1-w.__QSW_DR))); return v; }
  w.__QSW_DN++;
  return v*w.__QSWK;
};

try{console.log("QSW_MOD_LOADED v10s")}catch(e){}
}();
'''


def sub_once(text, old, new, tag):
    n = text.count(old)
    if n != 1:
        print('!! %s 命中 %d 次（期望 1）' % (tag, n))
        sys.exit(1)
    print('   %-30s OK (1)' % tag)
    return text.replace(old, new)


def re_once(text, pat, rep, tag):
    m = list(re.finditer(pat, text))
    if len(m) != 1:
        print('!! %s 命中 %d 次（期望 1）' % (tag, len(m)))
        sys.exit(1)
    print('   %-30s OK (1)' % tag)
    return text[:m[0].start()] + re.sub(pat, rep, text[m[0].start():m[0].end()]) + text[m[0].end():]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--src', required=True, help='原始 /assets/main/index.js')
    ap.add_argument('--dst', required=True, help='输出 main.js（进包 assets/qsw/main.js）')
    a = ap.parse_args()

    src = open(a.src, encoding='utf-8').read()
    orig_len = len(src)

    print('=== 应用补丁 v10s（星光版）===')
    # ① 无敌（受 __QSW_INV 控制；本版恒 false ⇒ 不生效，仅保留已验证代码不引入新变量）
    src = sub_once(src,
        'setHeroHp=function(e){this.realDie||(',
        'setHeroHp=function(e){window.__QSW_INV&&1===this.camp&&e<1&&(e=1,++window.__QSW_HN%10===0&&window.__QSW_LOG({ev:"hp_clamp",n:window.__QSW_HN,camp:this.camp,hid:this.heroId})),this.realDie||(',
        '① setHeroHp 无敌')

    # ②③④ 伤害倍率 / 减伤（受 __QSWK / __QSW_DR 实时控制）
    src = sub_once(src,
        'r-=this.life.value*this.damageReduce2.value),Math.max(0,s)+Math.max(0,r)',
        'r-=this.life.value*this.damageReduce2.value),window.__QSW_DMG(Math.max(0,s)+Math.max(0,r),this.camp)',
        '② getBaseReduceHp 倍率')
    src = re_once(src,
        r'(getExtraRealReduceHp=function\(e,t\)\{return)([^{}]*?)(\})',
        r'\1 window.__QSW_DMG((\2),this.camp)\3',
        '③ getExtraRealReduceHp')
    src = re_once(src,
        r'(getRealReduceHp=function\(e,t\)\{return)([^{}]*?)(\})',
        r'\1 window.__QSW_DMG((\2),this.camp)\3',
        '④ getRealReduceHp')

    # ⑥ 结算回执取证（本版 __QSW_LOG 为空实现 ⇒ 等价 no-op，仅保留结构）
    #   锚点：2026-10-08 重压缩后三元式取代 if/else、局部变量 v→b；
    #        2026-10-09 再次重压缩后 b→y（y=t.DEBUG 常量，生产恒假）、resolve 回调 a(t)→e(t)
    src = sub_once(src,
        '!t.result.isReject||y?(r.applyFightResultData(t),e(t)):',
        'try{window.__QSW_LOG({ev:"result",isReject:t.result.isReject,hurtRate:t.result.hurtRate,'
        'hn:window.__QSW_HN||0,dn:window.__QSW_DN||0,'
        'hasBarrier:!!t.result.barrierData,hasTower:!!t.result.towerData,keys:Object.keys(t.result||{})})}catch(e){};'
        '!t.result.isReject||y?(r.applyFightResultData(t),e(t)):',
        '⑥ 结算取证')

    # ⑦ neterr
    src = sub_once(src,
        'this._networkTips&&this._networkTips.responseErrorCode(i.code,r),_.rspObject.rejectback&&(',
        'window.__QSW_LOG({ev:"neterr",cb:i.callback,code:i.code,d:String(i.data).slice(0,300)}),'
        'this._networkTips&&this._networkTips.responseErrorCode(i.code,r),_.rspObject.rejectback&&(',
        '⑦ neterr 信标')

    # ⑦b bfe_catch（锚点：2026-10-08 重压缩后局部变量 e→t；且 `}catch(t){this.onClose()}`
    #     在全 JS 共出现 3 次 ⇒ 必须带 UIFightOver 结算弹窗前缀才唯一）
    src = sub_once(src,
        'kt.battleType=this.battleType,kt.title=s,et.gui.open(it.UIFightOver,kt,ot.getPopCommonEffect())}}catch(t){this.onClose()}',
        'kt.battleType=this.battleType,kt.title=s,et.gui.open(it.UIFightOver,kt,ot.getPopCommonEffect())}}catch(t){try{window.__QSW_LOG({ev:"bfe_catch",err:String(t&&t.message||t).slice(0,200)})}catch(_e){};this.onClose()}',
        '⑦b bfe_catch 信标')

    # ⑧ bfe 入口（锚点：2026-10-08 重压缩后模块作用域变量 g→p）
    src = sub_once(src,
        'barrierFightEnd=function(t){var r=this;return p.hurtRate&&(t.hurtRate=p.hurtRate),new Promise(',
        'barrierFightEnd=function(t){var r=this;try{var _fp=null;try{_fp=JSON.parse(t.fightpro)}catch(_e){};'
        'window.__QSW_LOG({ev:"bfe",win:t.win,time:t.time,'
        'keys:Object.keys(t),powers:t.powers,th:_fp&&_fp.th,tbh:_fp&&_fp.tbh,hurtRate:p.hurtRate||0,'
        'pw:typeof t.powers})}catch(e){};return p.hurtRate&&(t.hurtRate=p.hurtRate),new Promise(',
        '⑧ bfe 入口信标')

    # ⑪ 上报战力放大（__QSW_PSF 本版恒等 ⇒ 战力完全不动）
    src = sub_once(src,
        'w?o.powers[P][B]=w.power:x&&(o.powers[P][B]=x.power)',
        'w?o.powers[P][B]=window.__QSW_PSF(w.power):x&&(o.powers[P][B]=window.__QSW_PSF(x.power))',
        '⑪ powers 恒等-普通关卡')
    #  锚点：2026-10-08 重压缩后树洞/跨服分支局部变量整体重排（A/D/O → O/A/D）
    src = sub_once(src,
        'O?o.powers[1][A]=O.power:D&&(o.powers.pet1[A]=D.power)',
        'O?o.powers[1][A]=window.__QSW_PSF(O.power):D&&(o.powers.pet1[A]=window.__QSW_PSF(D.power))',
        '⑪ powers 恒等-树洞/跨服竞技')

    # ⑨ hurtCheck 中和（★实质功能项：阻断客户端把 hurtRate 上报作弊校验）
    #   锚点：2026-10-08 重压缩后 http 句柄 l→f、apipath/dbid 所在模块 g→p、getChannel/restartGame 所在 s→l
    #   2026-10-09 改法：原替换保留了 m.alert("netcode_-10"…)+restartGame()，被标记账号每场必弹
    #   「系统检测到游戏客户端异常，请点击确定重新启动」并重启。现改为「按已接受口径 resolve」
    #   —— 与真值分支 `r.applyFightResultData(t),e(t)` 完全同形：不弹窗、不重启、结算不挂起。
    #   （不改 reject：reject 会以 await 抛异常 → 结算响应异常弹窗，比原行为更糟。）
    src = sub_once(src,
        'f.http.post(p.apipath+"c=cheat&a=hurtCheck",{uid:p.dbid,channelId:l.getChannel(),hurtRate:t.result.hurtRate},(function(t){m.alert("netcode_-10",(function(){l.restartGame()}))}))',
        '(window.__QSW_LOG({ev:"hurtcheck_blocked",hurtRate:t.result.hurtRate}),r.applyFightResultData(t),e(t))',
        '⑨ hurtCheck 中和')

    # ⑩ hurtCheckPro 中和
    src = sub_once(src,
        'c.http.post(g.apipath+"c=cheat&a=hurtCheckPro",{uid:g.dbid,channelId:a.getChannel(),code:e},(function(t){"restart"==t?r.alert("netcode_"+e,(function(){a.restartGame()})):r.alert("netcode_"+e)}))',
        '(window.__QSW_LOG({ev:"cheat_code",code:e}),r.alert("netcode_"+e))',
        '⑩ hurtCheckPro 中和')

    # ⑫ netcode 弹窗静默（★用户诉求：账号被标后每场反复弹「参数传输遇到问题，点击确定重试」）
    #   锚点 = NetGameTips.responseErrorCode 的最后 else 分支（全 JS 唯一）。
    #   只静默 __QSW_NETSKIP 白名单（9998/9999/9997），其余错误码照旧弹窗，绝不吞掉真实错误。
    #   刻意不动 `<0 || 9995` 致命分支（账号异地登录 / 客户端不是最新版本）——那里必须 alert+restartGame。
    src = sub_once(src,
        '196==e?r.toast("netcode_"+e):r.alert("netcode_"+e)},e}())',
        '196==e?r.toast("netcode_"+e):window.__QSW_NETGUARD(e)?window.__QSW_LOG({ev:"netalert_suppressed",code:e}):r.alert("netcode_"+e)},e}())',
        '⑫ netcode 弹窗静默')

    # ⑬ 结算上报回流原版口径（★「屏蔽标记」的实质实现）
    #   机制：服务端拿客户端上报的结算数据与「原版期望值」比对，容差约 +50%；k 倍放大后
    #   th/hl/pl 是原版的 k 倍、tbh 因减伤低于原版 → 双向都被判异常 → 账号被标记。
    #   改法：只对「返回值」做折算，构造新对象，不动累加器本身（保证多次调用幂等、不叠加）。
    #   fightpro/worldScale=md5str(seedstr+fightpro+herotimes+pettimes) 都基于本返回值序列化，
    #   故折算后自洽，不会触发反篡改校验。
    #   折算口径与 __QSW_DMG 严格对应：camp==1（玩家受击）走 (1-dr)，其余走 k。
    src = sub_once(src,
        'n.createHurtLog=function(){return{pl:t.instance.petFontLog,hl:t.instance.heroFontLog,th:t.instance.totalHurt,tbh:t.instance.totalBeHurt}}',
        'n.createHurtLog=function(){var _k=window.__QSWK||1,_d=window.__QSW_DR||0,_p={},_h={},_i;if(_k>1){for(_i in t.instance.petFontLog)_p[_i]=Math.round(t.instance.petFontLog[_i]/_k);for(_i in t.instance.heroFontLog)_h[_i]=Math.round(t.instance.heroFontLog[_i]/_k);}else{_p=t.instance.petFontLog;_h=t.instance.heroFontLog;}return{pl:_p,hl:_h,th:_k>1?Math.round(t.instance.totalHurt/_k):t.instance.totalHurt,tbh:_d>0?Math.round(t.instance.totalBeHurt/(1-_d)):t.instance.totalBeHurt}}',
        '⑬ 结算上报回流原版口径')

    out = HEADER + src
    with open(a.dst, 'w', encoding='utf-8') as f:
        f.write(out)
    print('=== 写出 %s ===' % a.dst)
    print('  原始 %d B -> 补丁后 %d B (delta %+d)' % (orig_len, len(out), len(out) - orig_len))
    print('  md5 %s' % hashlib.md5(out.encode('utf-8')).hexdigest())


if __name__ == '__main__':
    main()
