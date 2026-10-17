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
# v14s（2026-10-10）：加速上限 10 → 3。用户实测：滑块 5、10 均过不了关（服务端 9998 拒判），
#   3 可稳定过关（依据用户 m00572「战斗加速5就可以…10不行」+ m00579「5好像也不行了，只能是3」）。
#   只改钳制上限与相关注释，机制（⑭ GAME_SPEED 取值器 / ⑮ TweenSpeed 纠偏）与其它锚点零改动。
# v11s（2026-10-11）：新增「战斗加速」两处锚点（⑭⑮），机制见下方 §battle-speed。
# v13s（2026-10-10）：只加「证据」，不改任何行为。新增 ⑰ 发送埋点（QSWREQ），并把旧 ⑦ 的 neterr
#   信标（原走 __QSW_LOG 空实现 = 生产包看不见）升级为 QSWERR 证据行 —— 两者都落 logcat tag
#   StarryJs_qsxz（rspCmd + 协议 callback + 截断 body + 服务端 data），
#   并把 __QSW_AUDIT 从「只记 netcode_」扩到「任何 alert 文案」，被吞的额外记一条 SKIPPED。
#   依据：用户 2026-10-10 报「弹窗虽静默，但点开始战斗点击不了」——必须知道是哪个 RPC 返回了什么码。
# v12s（2026-10-10）：新增 ⑯ 弹窗总闸——把 netcode 静默从「NetGameTips 最后 else 一处」
#   扩到「所有 alert 调用方」（每个别名绑的都是 GameUtil 类对象），并加 __QSW_AUDIT 证据行。
#
# §battle-speed（战斗加速）机制 —— 为什么不用「自己 new 一个 timeScale 就行」
#   ① 本作自带两档战斗速度，速度常量是 Const.ts 的枚举
#      E("GAME_SPEED", ...) ⇒ speed1=1.2 / speed2=1.6；战斗开始前 initSpeed() 调
#      getScheduler().setTimeScale(R.speed1|R.speed2)，状态存 localStorage doubleRate。
#   ② 引擎侧（cocos-js/_virtual_cc-*.js）：Scheduler.update(dt) 内部再乘一次自身
#      _timeScale ⇒ schedule 回调/tween 拿到 dt×T²；组件 update 拿到原始 dt。
#      但游戏自带 TweenSpeed 组件把 director.tick 包了一层（dt 先乘一次 timeScale），
#      于是组件 update 也变成 dt×T，2 倍速才真正加速战斗。
#   ③ 4 份技能 CD 用 `t/=("speed1"==...?E.speed1:E.speed2)` 把调度器给的 dt×T²
#      除回 dt×T ⇒ 只有「timeScale」与「这个常量」同步改，技能节奏才与原版一致。
#   ⇒ 因此补丁只做两件事：⑭ 把 GAME_SPEED 改成实时取值器（一处改动同时覆盖
#      2 个战斗 UI 的起步/切换/结算回落 + 4 份技能 CD，共 6 个消费方）；
#      ⑮ 抓 Scheduler 并在每帧只对 >1 的 timeScale 纠偏（场外 timeScale=1 绝不干预）。
#   语义：滑块 1 = 原版完全不干预（游戏自带 1x/2x 仍可用）；2..3 = 强制该倍速
#   （上限 3 是实测天花板：5/10 会被服务端以 9998 拒判整场，见硬约束 #7）。
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

HEADER = r'''/*QSW-MOD v15s 星光版*/
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
w.__QSW_NETSKIP={9998:!0,9999:!0,9997:!0};  /* 静默的 netcode（账号被标后反复弹「参数传输遇到问题」）
   ★ v12s 修正：旧值写成 1，而守卫用 `!0===白名单[e]` 严格比较 ⇒ 1!==true，白名单恒不命中、
      上轮 ⑫ 静默实为空操作（用户 2026-10-10 又见弹窗的真因）。此处必须是布尔 !0。 */
w.__QSW_NETGUARD=function(e){return !0===w.__QSW_NETSKIP[e]};

/* ===== 弹窗总闸（v12s 新增）：任何路径（NetGameTips / NetChatTips / HTTP 回调）弹 =====
   「参数传输遇到问题，点击确定重试」(netcode_9998) 都静默；其余错误码一律照旧弹窗。 */
w.__QSW_ALERTSKIP=function(s){
  if("string"!=typeof s)return !1;
  if(s.indexOf("参数传输遇到问题")>=0)return !0;
  if(0!==s.indexOf("netcode_"))return !1;
  return !0===w.__QSW_NETSKIP[parseInt(s.slice(8),10)];
};
/* 截断助手：日志只留前 n 字符，绝不让 logcat 被大 body 刷爆 */
w.__QSW_TRUNC=function(v,n){try{var s=(null==v)?(""+v):(("string"==typeof v)?v:JSON.stringify(v));
  if(null==s)s=""+v; return s.length>n?s.slice(0,n)+"~":s}catch(QT1){return "?"}};
/* 证据行（v13s 扩）：任何 alert 文案都先落一条日志（StarryH5.log -> logcat StarryJs_qsxz） */
w.__QSW_AUDIT=function(k){try{if(w.StarryH5&&w.StarryH5.log)w.StarryH5.log("QSWALERT "+w.__QSW_TRUNC(k,110))}catch(QA1){}
  try{w.__QSW_LOG({ev:"netalert_key",key:""+w.__QSW_TRUNC(k,110)})}catch(QA2){}};
/* ===== 网络埋点（v13s 新增，只记日志、不改行为）：取「哪个 RPC 返回了什么码」的地面真值 ===== */
w.__QSW_NETREQ=function(cmd,obj){try{var cb=(obj&&obj.callback)?(""+obj.callback):(""+cmd);
  if(cb.indexOf("heart")>=0)return;
  w.__QSW_AUDIT("QSWREQ cmd="+cmd+" cb="+cb+" body="+w.__QSW_TRUNC(obj,220))}catch(QR1){}};
w.__QSW_NETERR=function(code,cmd,cb,data){try{
  w.__QSW_AUDIT("QSWERR code="+code+" cmd="+cmd+" cb="+cb+" data="+w.__QSW_TRUNC(data,220))}catch(QE1){}};

/* ===== 战斗加速（v11s 新增）：滑块值 = 目标倍速；1 = 原版完全不干预 ===== */
w.__QSW_SPD=1;      /* 菜单滑块值 1..10（1 = 不干预，游戏自带 1x/2x 照旧可用；上限 10 = 用户定制 1-10） */
w.__QSW_SCHED=null; /* 运行时抓到的 cocos Scheduler（在 TweenSpeed.start 里捕获） */
w.__QSW_SPDVAL=function(d){var q=w.__QSW_SPD|0;return q>=2?q:d};  /* GAME_SPEED 枚举取值器 */

w.__QSW_APPLY=function(k,drPct,spd){
  k=(k|0); if(k<1)k=1; else if(k>100)k=100;
  drPct=(drPct|0); if(drPct<0)drPct=0; else if(drPct>100)drPct=100;   /* 下限 0=原版哨兵（未授权回落 0）；上限 50→100（用户定制） */
  spd=(spd|0); if(spd<1)spd=1; else if(spd>10)spd=10;   /* 2026-10-18：上限 3→10（用户定制 1-10） */
  w.__QSWK=k; w.__QSW_DR=drPct/100;
  var ch=(w.__QSW_SPD!==spd);
  w.__QSW_SPD=spd;
  /* 滑块变了且正在战斗（timeScale>1）→ 立即生效；场外 timeScale=1 一律不碰 */
  if(ch&&w.__QSW_SCHED){
    try{
      var c=w.__QSW_SCHED.getTimeScale();
      if(spd>=2&&c>1&&c!==spd)w.__QSW_SCHED.setTimeScale(spd);
      w.__QSW_LOG({ev:"speed_apply",q:spd,c:c});
    }catch(e){}
  }
};

w.__QSW_DMG=function(v,c){
  if(1===c){ if(w.__QSW_DR>0)return Math.max(1,Math.round(v*(1-w.__QSW_DR))); return v; }
  w.__QSW_DN++;
  return v*w.__QSWK;
};

try{console.log("QSW_MOD_LOADED v15s")}catch(e){}
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

    print('=== 应用补丁 v15s（星光版）===')
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
        'window.__QSW_NETERR&&window.__QSW_NETERR(i.code,r,i.callback,i.data),'
        'window.__QSW_LOG({ev:"neterr",cb:i.callback,code:i.code,d:String(i.data).slice(0,300)}),'
        'this._networkTips&&this._networkTips.responseErrorCode(i.code,r),_.rspObject.rejectback&&(',
        '⑦ neterr 信标（v13s 起兼作 QSWERR 证据行）')

    # ⑦b bfe_catch（锚点：2026-10-08 重压缩后局部变量 e→t；且 `}catch(t){this.onClose()}`
    #     在全 JS 共出现 3 次 ⇒ 必须带 UIFightOver 结算弹窗前缀才唯一）
    src = sub_once(src,
        'kt.battleType=this.battleType,kt.title=s,et.gui.open(it.UIFightOver,kt,ot.getPopCommonEffect())}}catch(t){this.onClose()}',
        'kt.battleType=this.battleType,kt.title=s,et.gui.open(it.UIFightOver,kt,ot.getPopCommonEffect())}}catch(t){try{window.__QSW_LOG({ev:"bfe_catch",err:String(t&&t.message||t).slice(0,200)})}catch(_e){};this.onClose()}',
        '⑦b bfe_catch 信标')

    # ⑧ bfe 入口（锚点：2026-10-08 重压缩后模块作用域变量 g→p）
    src = sub_once(src,
        'barrierFightEnd=function(t){var r=this;return m.hurtRate&&(t.hurtRate=m.hurtRate),new Promise(',
        'barrierFightEnd=function(t){var r=this;try{var _fp=null;try{_fp=JSON.parse(t.fightpro)}catch(_e){};'
        'window.__QSW_LOG({ev:"bfe",win:t.win,time:t.time,'
        'keys:Object.keys(t),powers:t.powers,th:_fp&&_fp.th,tbh:_fp&&_fp.tbh,hurtRate:m.hurtRate||0,'
        'pw:typeof t.powers})}catch(e){};return m.hurtRate&&(t.hurtRate=m.hurtRate),new Promise(',
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
        'f.http.post(m.apipath+"c=cheat&a=hurtCheck",{uid:m.dbid,channelId:l.getChannel(),hurtRate:t.result.hurtRate},(function(t){p.alert("netcode_-10",(function(){l.restartGame()}))}))',
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
        'i.createHurtLog=function(){return{pl:t.instance.petFontLog,hl:t.instance.heroFontLog,th:t.instance.totalHurt,tbh:t.instance.totalBeHurt}}',
        'i.createHurtLog=function(){var _k=window.__QSWK||1,_d=window.__QSW_DR||0,_p={},_h={},_i;if(_k>1){for(_i in t.instance.petFontLog)_p[_i]=Math.round(t.instance.petFontLog[_i]/_k);for(_i in t.instance.heroFontLog)_h[_i]=Math.round(t.instance.heroFontLog[_i]/_k);}else{_p=t.instance.petFontLog;_h=t.instance.heroFontLog;}return{pl:_p,hl:_h,th:_k>1?Math.round(t.instance.totalHurt/_k):t.instance.totalHurt,tbh:_d>0&&_d<1?Math.round(t.instance.totalBeHurt/(1-_d)):t.instance.totalBeHurt}}',
        '⑬ 结算上报回流原版口径')

    # ⑭ 战斗加速①：GAME_SPEED 枚举改成「实时取值器」
    #   Const.ts 的常量枚举由 SystemJS _export 导出同一个对象引用（不冻结不拷贝），
    #   定义只在模块执行期跑一次 ⇒ 直接改数值会被冻成常量，必须装 getter 才能随滑块变。
    #   一处改动覆盖全部 6 个消费方：两套战斗 UI 的 initSpeed/onClickSpeed/结算回落（timeScale）
    #   + 4 份技能 CD 的除速分子（t/=speed），二者必须同值，技能节奏才与原版按游戏时间一致。
    src = sub_once(src,
        'E("GAME_SPEED",function(E){return E[E.speed1=1.2]="speed1",E[E.speed2=1.6]="speed2",E}({}))',
        'E("GAME_SPEED",function(Q){Q[Q.speed1=1.2]="speed1",Q[Q.speed2=1.6]="speed2";'
        'try{Object.defineProperty(Q,"speed1",{get:function(){return window.__QSW_SPDVAL(1.2)},enumerable:!0,configurable:!0}),'
        'Object.defineProperty(Q,"speed2",{get:function(){return window.__QSW_SPDVAL(1.6)},enumerable:!0,configurable:!0})}'
        'catch(QSXZE){};return Q}({}))',
        '⑭ 战斗加速·速度枚举')

    # ⑮ 战斗加速②：抓 Scheduler + 每帧「只提速不降速」纠偏
    #   游戏自带 TweenSpeed 组件把 director.tick 包了一层（dt 先乘 timeScale），
    #   本补丁复用这一层：顺手捕获 getScheduler()，并在滑块开启时把 >1 的 timeScale 拉到目标值。
    #   门槛刻意用 c>1：场外 resetGameSpeed 置 1 ⇒ 绝不干预（城里/菜单不加速），
    #   结算慢镜 0.5 也保留；滑块中途改动在下一次注入（1s/5s）内即生效，不必等到下一场。
    src = sub_once(src,
        'n.start=function(){var e=this;r.tick=function(t){e.oldTick.call(r,t*r.getScheduler().getTimeScale())}}',
        'n.start=function(){var e=this;window.__QSW_SCHED=r.getScheduler(),r.tick=function(t){'
        'var q=window.__QSW_SPD|0;if(q>=2){var c=window.__QSW_SCHED.getTimeScale();'
        'c>1&&c!==q&&(window.__QSW_SCHED.setTimeScale(q),window.__QSW_LOG({ev:"speed_force",q:q,c:c}))}'
        'e.oldTick.call(r,t*window.__QSW_SCHED.getTimeScale())}}',
        '⑮ 战斗加速·每帧纠偏')

    # ⑯ 弹窗总闸（v12s）：把 netcode 静默从「NetGameTips 最后 else 一处」扩到「全部 alert 调用方」
    #   实测依据：全 JS 每个 alert 别名都在 setters 里绑定 GameUtil **类对象本身**
    #   （NetGameTips r=e.GameUtil / NetChatTips s=t.GameUtil / SDKPlatform m=e.GameUtil /
    #    HomeManager g=e.GameUtil / HotUpdateComp S=e.GameUtil / XipuInviteView C=t.GameUtil …
    #    全文这类绑定 400+ 处，无一例绑 alert 函数值），故包装这一个静态方法即可覆盖 9998 的所有
    #    可能来源：NetChatTips 的无过滤分支、`alert(t.data…"netcode_-2")` 的 HTTP 错误回调、
    #    HotUpdateComp / SDKPlatform 的登录与热更失败提示。
    #   只静默 __QSW_NETSKIP 白名单（9998/9999/9997）+ 该文案原文；
    #   -8/-9/-10/-12/-13 致命码与 9986~9989「战斗验证失败」族一律照旧弹（绝不吞真实错误）。
    #   __QSW_AUDIT 先落证据行，装机后可从 logcat 反查是哪条路径弹的（也能证伪本锚点是否覆盖到）。
    src = sub_once(src,
        'e.alert=function(e,n,o,i){var t={content:e,okWord:i||"UI_STANDARD_1",autoClose:!0,okFunc:function(){n&&n()},needCancel:!1};',
        'e.alert=function(e,n,o,i){try{if("string"==typeof e&&window.__QSW_AUDIT)window.__QSW_AUDIT(e)}catch(QA0){}'
        'try{if(window.__QSW_ALERTSKIP&&window.__QSW_ALERTSKIP(e)){window.__QSW_AUDIT&&window.__QSW_AUDIT("SKIPPED "+e);return}}catch(QSWAE){}'
        'var t={content:e,okWord:i||"UI_STANDARD_1",autoClose:!0,okFunc:function(){n&&n()},needCancel:!1};',
        '⑯ 弹窗总闸·GameUtil.alert')

    # ⑰ 发送埋点（v13s，只记日志）：base_request 里 _requests.push 之前记一条 QSWREQ
    #   （cmd=rspCmd、body=协议对象 e 截断 220 字符；心跳 callback 含 "heart" 直接跳过以免刷屏）
    src = sub_once(src,
        'o||(this._requests.push({reqProtocol:e,rspCmd:t,rspObject:i,showTips:s})',
        'o||(window.__QSW_NETREQ&&window.__QSW_NETREQ(t,e),this._requests.push({reqProtocol:e,rspCmd:t,rspObject:i,showTips:s})',
        '⑰ 发送埋点·QSWREQ')

    # （v13s）服务端错误码埋点不另开锚点：旧的 ⑦ neterr 信标锚点就落在同一个派发点
    #   （_.rspCmd==r){…,l?(this._networkTips&&…responseErrorCode(i.code,r)…），
    #   已在上方把它的注入体升级为「先 __QSW_NETERR 落 QSWERR 证据行」⇒ 一个锚点覆盖两件事。

    out = HEADER + src
    with open(a.dst, 'w', encoding='utf-8') as f:
        f.write(out)
    print('=== 写出 %s ===' % a.dst)
    print('  原始 %d B -> 补丁后 %d B (delta %+d)' % (orig_len, len(out), len(out) - orig_len))
    print('  md5 %s' % hashlib.md5(out.encode('utf-8')).hexdigest())


if __name__ == '__main__':
    main()
