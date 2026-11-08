# 动物大联盟 1.0.25 —— res JS 结构 & 六项功能挂点初勘

> 2026-11-06 第十五轮之三。源码来自 09_test/动物大联盟/res_js_20261106/（由 jsc_codec_dwdlm.py 解出）。
> 标「推断」的未验证，其余为读码实测。

## 1. bundle 结构（Cocos 2.4 标准 window.__require 模块表）

| bundle | 模块数 | 体积 | 说明 |
|---|---|---|---|
| assets/scripts/index.js | 238 | 2,321,617 B | 主干：ApiManager、AccountLoginView、Battle*View，广告/反作弊在混淆模块 _2/_4U/_5F/_7A |
| assets/subScripts/index.js | 706 | 4,096,747 B | 全部 UI 面板（ArenaView/BagView/PanelCfgSkill…） |
| assets/battleScript/index.js | 111 | 568,930 B | 战斗模拟，模块名全混淆（_20/_4D/_W…） |
| assets/main/index.js | 2 | 11,562 B | StartScene + UpdateManager（脚本热更，读 localStorage.updateFileUrl） |

命名规律：混淆模块成员统一 _Vxx 前缀；**原生桥回调名保留可读**（如 _onRewardedAdClose），可作定位锚点。

## 2. 伤害链（battleScript 2758–2800）

    Mt = It / (It + kt)                               // 防御减免，It = 200 * 防御系数
    _e = Zt * At * Ct * Dt * Nt * jt * Xt * Tt        // 攻击方倍率链
    te = Math.ceil(St * _e * Mt) + Math.ceil(Kt * _e) // 最终伤害

St = K[n._gm._VK]（攻击）；At/Ct/Tt 是 +1 的百分比加成；Nt/jt 是防御方减伤；Xt = Ht.damageFactor（技能修正）。
n._gm 是属性枚举表（_VK/_Vi/_Vb/_VG/_VP/_Vm/_Vf/_Vh/_Vx/_Vz…），**属性名待定**（下一步 1）。

## 3. 反作弊（重要，先于功能设计）

- 阈值服务端下发：scripts 27558 `DynamicData.cheatDatas = [0, 5e5, 2e4, 300, 4, 5e3, 0, 4e3, 6400, 0, 6, 1500]`。
- **cheat5（逐次命中触发）**：battleScript 2797 —— St > cheatDatas[2]=20000、或 Zt>300、或 Ct/At/Tt>4、或 Dt>1、或 Mt>1、或 Kt>1500 即上报。
- 周期检查：scripts 27904 起 setTimeout(5e3*rand + cheatDatas[0])；cheatDatas[0]==0 ⇒ 该块默认关闭（服务端可开）。
- 其他埋点：cheat2/3/4/7/8/10/12/100/101（scripts 42308/42319、27558 附近；battleScript 15441）。
- 上报入口：DynamicData._Vx._VL(name, flag)（_Vx 为单例，实现待定；scripts 27317/27813 是静态 getter）。

⇒ 六项功能不能直接堆数值：攻击倍率一旦让面板攻击 > 20000（或加成 > 4 倍），命中的那一刻就触发 cheat5。
设计上要么把倍率压在阈值内，要么同时把 _VL 上报入口 patch 成空实现（JS 能改也能回封，这条路可行）。

## 4. 六项功能候选挂点

| 功能 | 候选挂点 | 状态 |
|---|---|---|
| 免广告 | scripts 模块 _7A＝广告管理器（preloadVideoAd/_onRewardedAdLoad/_onRewardedAdClose/initBannerInfo）；_5F 是「免广告卡」流程（50709/50855/50882 三条文案） | 锚点已找到，具体函数待定 |
| 自动战斗 | battleScript _VxC(t) 切 UI 文案；_Vmb() 里 checkIsUnlockByBtn(EUnLockBtnType.AutoBattle) ⇒ 服务端解锁项 | 解锁判断点已定位 |
| 攻击倍率 | 伤害链 St=K[n._gm._VK] 或 _e 倍率链（battleScript 2758–2800） | 公式已定位，需避 cheat5 |
| 上帝模式 | 同上：M.entityHP = Math.max(0, C - F)（受击扣血）＋防御方 Pt[n._gm._Vf] 减伤链 | 公式已定位 |
| 无限技能 | 技能门控 Pt[n._gm._Vr] < 1 与 Pt[n._gm._Va]（battleScript 2775 附近）；UI 侧 BattleBtnOptionView/TopNodeItem | 门控点已定位 |
| 移速 | battleScript 未直接命中；scripts _5D（速度）与 HorseRacingPrepareItem | 待定 |

## 5. 下一步

1. 定 n._gm 属性表（_VK/_Vi/_Vb/_Vf/_Vr 对上攻击/攻速/暴击/生命/技能CD）。
2. 定 _VL 上报入口实现，评估 patch 成空实现的副作用。
3. 热更校验待验：assets/scripts/index.jsc 已换成「只多一行 console.log([NX_PROBE] res-patch alive)」的版本（原件备份 /data/local/tmp/nx_backup_scripts_index.jsc），下次冷启看 logcat 是否出现该行、文件是否被覆盖。
