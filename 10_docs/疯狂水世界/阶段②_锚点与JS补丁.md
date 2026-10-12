# 疯狂水世界 1.11.6 —— 阶段② 锚点与 JS 补丁（无敌 / 伤害倍率 / 战斗加速）

> 建立：2026-10-13 ｜ 依据：对**已解密运行时 JS** 的静态确认（`09_test/疯狂水世界/jsdump/`，见 §1）
> 结论：三功能全部落在 **JS 层运行时补丁**，native 仍 **0 hook**。

## 一、为什么只能走运行时 JS 补丁（两条死路已排除）

| 死路 | 实测证据 |
|---|---|
| 静态改包内 JS | 包内 `assets/**` 394 个 `.js` **全是 NHPK 密文**（`assets/src/cocos-js/cc.js`、`chunks/bundle.js`、`settings.json` 同族）；塞明文 JS 进去会被按密文再解 ⇒ 不可行 |
| 按名挂 native `evalString` | `libcocos.so` 30.4 MB **dynsym 全混淆**（`llvm-nm -D` 3720 个符号名全是垃圾串，逐字节检索 `evalString`/`_ZN2se12ScriptEngine10evalString` 命中 0）⇒ 无符号可挂 |

⇒ 走先例（仙逆 `Games/XnbzMod.cpp`）同款通道：native 启线程 → `com.starry.magic.cocos.FkssjHooks.pump(js)` → `CocosHelper.runOnGameThreadAtForeground` → `com.cocos.lib.CocosJavascriptJavaBridge.evalString(js)` → 补丁在 SystemJS 模块层打锚点。

## 二、怎么拿到游戏本体代码（决定性的取证）

- `assets/src/settings.json`（解密后）：**`scripting.scriptPackages = ["../chunks/bundle.js"]`** ⇒ 全游戏只有**一个**脚本包。
- 但包内 `bundle.js` 1,548,191 B **只有 95 个模块、全是第三方库**（crypto-js / protobuf / lz4 / jsbi / matter / fx…），中文串 0 ⇒ **游戏本体不在这里**。
- 真相：引擎优先从**热更目录**解析
  `fullpath bundle=/data/user/0/com.gx.sw.qa.fkssj004.esj/files/gx_game_hotupdate/src/chunks/bundle.js`，
  且 `jsb.fileUtils.getStringFromFile()` **返回的已是解密明文**。
- 于是用 SystemJS 注册表抓全量模块（幂等 dump JS，`Games/FkssjMod.cpp` §⑨ `kDumpJs`）：
  **1912 个模块**（`chunks:///_virtual/*.ts` 1877 + `virtual:///prerequisite-imports/*` 35）⇒ 游戏本体 **1746 个 `.ts` 模块只在运行时注册表里**。
- 取证产物（本地，`09_test/` 已 gitignore）：`fkssj_dump.js` 7,955,684 B + 按段拆好的 `jsdump/mods/*.js`（1912 个，文件名 = 模块名把非字母数字换成 `_`，例 `chunks_____virtual_BattleUtils.ts.js`）。
- 重抓开关：`Games/FkssjMod.cpp` 的 `#define FKSSJ_JS_DUMP 1` 重编 → 装机启动 → `bash 04_scripts/疯狂水世界/dump_pull.sh`。

## 三、三项锚点（全部经解密 JS 实证）

### 3.1 战斗是**客户端模拟**（不是服务端下发录像）
`BattleAPI.createBattle(e,t)`：按 `t.attackRoles`/`t.defenseRoles` 的 `attr/hp/skillLevels/exBuff` 建单位 → `a.setSeed(t.randomSeed)` → 跑 `GameBattle`。
`GameBattle.checkWin()` 只看**敌方** `AIUnit` 全死；`checkFail()` 只看**我方**；`checkBattleFinish()` 在 `fightTime>=maxFightTime || frame>2e5` 判负。
⚠️ **演出是客户端跑，但「结算归属」分模式**：奖励/进度/关卡推进来自服务端响应，胜负弹窗则只有主线读服务端字段 —— 见 §五。

**阵营常量（`BattleDefine.ts`）**：`ATTACK_CAMP=0` / `MY_CAMP=0`（我方）；`DEFEND_CAMP=1` / `ENEMY_CAMP=1`（敌方）。
**`EUnitProperty`（`BattleDefine.ts`）**：`None=0,Hp=1,MaxHp=2,Mp=3,MaxMp=4,Def=5,DefRd=6,AD=7,AP=8,AtkRange=9,AtkSpeed=10,…`
（`BattleUnitData` 用 `nowProps[]/initProps[]/limitProps[]` 存，读写走 `setValue/addValue/subValue/getValue`。）

### 3.2 锚点①：伤害唯一汇聚点 `BattleUtils.applyDamage`（→ 无敌 + 伤害倍率）
```
static applyDamage(e,t,r,s=!1){
  let n=t.data.getValue(l.Hp).asInt(), i=n-r;
  i<0&&(i=0),
  i>t.data.getValue(l.MaxLimitHp).asInt()&&(i=t.data.getValue(l.MaxLimitHp).asInt()),
  n-=i, t.data.setValue(l.Hp,i);   // …后续掉血→回蓝逻辑…
  return n                          // 返回值 = 实际掉血量
}
```
- 生产环境**只有 `DamageEffect.ts` 一处**调用：`i=m.applyDamage(this.caster,t,i,this.isGetMp)`，其中 `i` 来自 `calculateShield(...)` 的 `floorInt()`。
- ⇒ 覆盖 `applyDamage` 即可**同时**实现：目标 `camp===0`（我方）且无敌 ⇒ `r=0`；目标 `camp===1`（敌方）⇒ `r=Math.floor(r*倍率)`。
- ⚠️ 关键收益：`PerLimitDmg`（单次伤害上限）是在 `DamageEffect` 内**调用之前**用 `Math.min` 钳的 ⇒ **在 `applyDamage` 里乘倍率可以绕过该上限**。
- `r=0` 时 HP 不变且直接 return 0（连受击事件都不派发）⇒ 无敌不会有「掉血闪一下」。
- 游戏自带 `EffectType.INVINCIBLE`（`DamageInmunityEffect` + `statusCheck(INVINCIBLE)`）也能免伤，但要求目标身上真的挂上该 buff、且要走效果表配置 ⇒ 不采用。

### 3.3 锚点②：死亡判定 `AIUnit.checkDeath`（→ 无敌双保险）
```
checkDeath(){ return this.statusCheck(g.DEATH) || m.lessEqual(this.data.getValue(v.Hp),0) }
```
覆盖为：`F.cfg.inv && this.camp===0 → false`。
我方恒不死 ⇒ 由于 `checkWin()` 只看敌方、`checkFail()` 只看我方，**只会赢不会输**，而敌方仍会被正常打死（伤害链未动）。

### 3.4 锚点③：推帧 `BattleInterfaceBase.prototype.update`（→ 战斗加速）
```
update(t){
  if(this.battle&&this.battle.isStart&&!this.battle.isEnd&&!this.pause)
    for(0==this.battle.frame ? this._delayTime=this.FRAME_TIME/this.playSpeed : this._delayTime+=t;
        this._delayTime>=this.FRAME_TIME/this.playSpeed && this.battle && !this.battle.isEnd;)
      this._delayTime-=this.FRAME_TIME/this.playSpeed, this.battle.update(), this.synchUIHP(),
      …dispatchEvent(ON_UPDATE_BATTLE_FRAME…);
  this.updateDrawOrder()
}
```
- 帧推进与播放走**同一个循环** ⇒ 放大传入 `dt` 即整体加速（动画/结算同步）。
- ⚠️ **不要改 `playSpeed`**：它在构造时被 `a.clamp(defaultBattleSpeed(),1,1.5)` **钳死 ≤1.5**，且 `defaultBattleSpeed()`（`BattleViewUtil.ts`）读私有 cookie `BATTLE_SPEED`（UI 只有 1×/2×）。⇒ 只放大 `dt`。

### 3.5 模块取用方式
`System.get("chunks:///_virtual/<Name>.ts")` → 命名空间对象；`namespace.<ClassName>` 即类对象，改其静态方法 / `.prototype` 即全局生效（其它模块的 setter 捕获的是同一个类对象引用）。
涉及模块（`jsdump/mods/` 内均有）：`BattleUtils.ts` 16,528 B、`BattleDefine.ts` 16,494 B、`DamageEffect.ts` 9,865 B、`BattleInterfaceBase.ts` 14,478 B、`AIUnit.ts` 22,878 B、`BattleAPI.ts` 12,469 B、`GameBattle.ts` 2,527 B、`BattleUnitData.ts` 2,961 B。

## 四、补丁落地（`Games/FkssjMod.cpp` §⑨-b）

- `static const char* kPatchJs = R"JS(...)JS"`：**幂等**（每处用 `F.hookBU`/`F.hookAI`/`F.hookBIB`/`F.hookBIBE` 存**包装后的函数**做守卫 —— 用函数身份而非类对象身份，防场景重载把方法还原后误判「已挂」而静默丢补丁），可无限重复 eval。
- **锚点④ = 战斗结束取证（只读，不改行为）**：包 `BattleInterfaceBase.prototype._onBattleEnd`，每场结束回传
  `battleEnd type=<模式名> clientWin=<0/1> serverWin=<0/1/-1> popup=<WIN/FAIL/?> frame=… fightTime=…/maxFightTime=… inv=… dmg=…`
  ⇒ 这是判定「补丁在哪个模式真的改到了结算」的**唯一机器证据**（`serverWin=-1` = 该模式响应里没有结果字段）。
- `kCfgJsFmt`（`snprintf` 三个 `%d` = inv/dmg/spd）写进 `globalThis.__fkssj.cfg`。
- `FkssjBridgeThread`：**常驻 pthread，2s 一轮** —— ① cfg 只在数值变化时推送（菜单拨动 ≤2s 生效）② 每轮重跑补丁 ⇒ **场景切换 / System 重载后自动重挂**。不依赖框架 Tick（Tick 只在 `session_live()` 授权通过后才跑）。
- 回传通道：`jsb.reflection.callStaticMethod("com/starry/magic/cocos/FkssjHooks","logStatus","(Ljava/lang/String;)V", …)` → `adb logcat -s StarryTrace`，前缀 `fkssj-js`。
- 阈值：伤害倍率 1..100（UI 上限一致）、战斗加速 1..10；`1` = 关闭。
- 取值：`Fkssj_Tick(features, values)` 用 `values[FKSSJ_FEAT_DMG]` / `values[FKSSJ_FEAT_SPD]`（惯例同 `BlfxMod.cpp`/`CzbhzMod.cpp`）。

### 装机实证（2026-10-13 00:31）
```
[Fkssj] lib ready (cocos jsb, js-patch armed)
[Fkssj] js bridge thread start (interval=2s)
StarryTrace: pump#1 act=com.cocos.game.AppActivity safe=true
[Fkssj] js cfg push inv=1 dmg=100 spd=10
StarryTrace: fkssj-js anchor#1 applyDamage hooked
StarryTrace: fkssj-js anchor#2 checkDeath hooked
StarryTrace: fkssj-js anchor#3 battle speed hooked
StarryTrace: fkssj-js all 3 anchors armed
StarryTrace: fkssj-js stat#2 inv=0 dmg=0 spd=1
```
⇒ 三个锚点**全部挂载成功**；`inv`/`dmg` 计数在**进入战斗后**才会增长（`stat#N inv=… dmg=…` 增长 = 战斗确实走过伤害汇聚点，是机器侧可判证据）。

## 五、「打赢了却显示战斗失败」的机制 —— 结果判定归属（分模式）

客户端手里有**两份结果**，必须分开看：
- **演出结果**：客户端自己跑的那场模拟（`GameBattle.victorCamp`）。
- **结算结果**：服务端响应里的字段（`ChallengeStageResponse.stats.isLeftWin` / `ArenaChallengeResponse.isWin` / `*Response.reward`），**奖励与进度只看这份**。

### 5.1 胜负弹窗读谁 —— 分模式（全部实测于解密 JS）

| 模式 | 弹窗依据 | 证据 |
|---|---|---|
| **主线推图** BT_MAINLINE | **服务端** | `StageMainLineBattleInterface._showBattleResult()`：`const t=this.serverData, e=t.stats.isLeftWin; e?show(STAGE_BATTLE_WIN_POP,t):show(BATTLE_FAILED_POP,…)` |
| 英雄副本 BT_HERO_RAID | 客户端 | `HeroRaidBattleInterface._onBattleEnd`：`e.victorCamp==MY_CAMP?show(HERO_RAID_WIN_POP,a):show(BATTLE_FAILED_POP,…)` |
| 试炼副本 BT_TRIAL_RAID | 客户端 | `TrialRaidBattleInterface`：`e.victorCamp==MY_CAMP?show(TRIAL_RAID_WIN_POP,a):…` |
| PvE / 世界BOSS / 海盗 / 资源田 / 雷达 | 客户端 | `PvEBattleInterface`：`e?show(PVE_BATTLE_RESULT_POP,…):show(BATTLE_FAILED_POP,…)` |
| 竞技场 / 联赛 / 锦标赛 / 推荐 / 城战 / 远征 / Ultra / 徽章战 | 客户端 | `PvpBattleInterface`/`LeagueBattleInterface`/`TournamentBattleInterface`/`RecommendBattleInterface`/`CityWarBattleInterface`/`ExpeditionBattleInterface`/`UltraBattleInterface` 均为 `const e=t[0].victorCamp==MY_CAMP` |

`BattleInterfaceBase._onBattleEnd` 本体：`this.isEnd=!0;let e=t[0];e&&this.victory(e.victorCamp),p.event.dispatchEvent(ON_BATTLE_END,…)`。

### 5.2 主线为什么是服务端说了算（原文证据）
```js
// StageUtil.stageFight / PvEBattleView._onBattleFightAgain 的 MainLine 分支
const l = newMsg("ChallengeStageRequest", {stage:i});
NetworkManager.instance.sendMessage(l, t => {          // ① 服务端先回「结果+奖励+敌方阵容」
    if (t.stats && t.stats.isLeftWin) model.setMainlineStage(i+1);   // ② 关卡推进此刻已定
    s.level = i; s.setServerData(t); s.enterBattle(t.initialData, …); // ③ 然后才开始播演出
});
```
⇒ **演出还没开始，主线输赢就已经写死**；`serverData` 里还有 `stats`（`BattleStats{isLeftWin,leftHeroDamages,rightHeroDamages,leftHp…}`）与 `reward`，弹窗直接展示它。
同类：`HeroRaidChallenge` 回调 `t.reward && model.activity.updateRaid(raidId,stageId)`（没 reward 就不推进）。

**协议层旁证**：613 个 `*Request` 中**0 个**携带战果字段（请求只带标识符：`ChallengeStageRequest{stage}`、`LeagueFightRequest{playerId}`、`UltraFightRequest{lv,affixId}` …）；而**所有**战斗响应都自带 `isWin`/`stats`/`reward`。传输层是真 WebSocket（`WebsocketTransport`：ping/pong + reconnect + auth），非本地假服务器。⇒ 客户端**无法上报战果**。

### 5.3 等级校验（有，两处）
1. **关卡开放等级（客户端硬拦，请求都不发）** `StageUtil`：
   ```js
   static stageOpenLevel(t){ return ConfigManager.get("StageCfg","stage",t).playerLevel }
   static stageFight(t){ const e=c.stageOpenLevel(t);
     if (s.model.playerInfo.level < e) return void o.showFormatNotice(r.formatString(transTS("game_ui_shops_ShopView_auto_1"), e)); … }
   ```
   `StageBattleWinPop` 也用同一函数决定「自动战斗」按钮态：`this._c_state.selectedIndex = model.playerInfo.level>=StageUtil.stageOpenLevel(nextStage)?0:1`。
2. **服务端侧校验**：静态不可见（逻辑不在包里）。敌方阵容/种子/奖励都由它给，必然用服务端角色数据自算一遍。

### 5.4 因此三功能的**可达语义**
- **战斗加速**：**真有效**（全模式；只改演出速度）。旁证：`GameBattle.onBattleUpdate()` 里 `fightTime` 由 `_accumulateTime.add(FRAME_DELTA)` 累加（≈帧计数，非墙钟）⇒ 放大 `dt` **不改变 `fightTime`**，既不引入超时风险，也不能靠加速规避超时。
- **无敌 / 伤害倍率**：
  - **主线**：仅演出层 —— 弹窗与关卡推进都听服务端，**不能把服务端判负的关变成胜**。
  - **其余模式**：弹窗判的是客户端 `victorCamp`，**会跟着变成「胜利」**；但**奖励/进度仍由服务端响应决定**（胜利弹窗里的奖励数据取自 `serverData`，服务端判负时可能为空）。
- **副作用（须实测关注）**：服务端判负 + 我方无敌 ⇒ 演出无法经 `checkFail()` 收场，只能跑满 `maxFightTime`（`frame>2e5` 兜底）才结束 ⇒ 该场演出会变长；把战斗加速调大即可缓解。锚点④ 的 `fightTime=x/y` 字段可直接看出是否撞上超时。

## 六、已知风险 / 待实测判定
1. **服务端判定不可翻盘**（见 5.4）：若用户要「主线也能真赢」，客户端改包做不到；只能改**请求**（例如自动挑能打的 `stage`），那已不是「无敌」。
2. **倍率过高**：`dmg=100` + 我方无敌是极端值，首测建议先小倍率（如 3）。
3. **战斗加速上限**：×10 时单帧推进 10×，若某场战斗有强制动画/时序（QTE）可能表现异常。
4. `BattleInterfaceBase.update(dt)` 的 `dt` 是否被别的子系统复用（同一 `dt` 也驱动 UI）—— 实测看是否有非战斗界面被加速。
5. **编译期无 JS 语法校验** ⇒ 已加门禁 `04_scripts/疯狂水世界/fkssj_check_js.py`（抽 `R"JS(...)JS"` 全块跑 `node --check`，已挂进 `fkssj_build_so.sh`）：写错一个括号编译照样过、装机后补丁静默失效。

## 七、L3 实测取证工具

| 脚本 | 用途 |
|---|---|
| `04_scripts/疯狂水世界/fkssj_redeploy.sh` | 一键重出包并装机：编 so → surgical → sign → device_ready → install（自动处理「产物指纹变化」确认点，不跳过任何硬校验），末尾启动游戏 |
| `04_scripts/疯狂水世界/fkssj_battle_report.sh` | **战斗取证汇总**：清缓冲 → 请用户打 1~2 场 → 抽 `battleEnd` 行成表（模式 / 客户端判 / 服务端判 / 弹窗 / 帧 / fightTime），并对主线自动标注「服务端判负而演出判胜」的场次与「frame>1800（被无敌拖到超时）」的场次；产出 `09_test/疯狂水世界/battle_<ts>.log` |
| `04_scripts/_common/flight_recorder.sh` | 全程飞行记录（用户喊「标记」时跑其 `mark` 子命令） |
| `04_scripts/_common/post_install_precheck.sh` | 装机后上手前的机器预检（菜单加载/弹出/授权/空转崩溃） |

用法：`bash 04_scripts/疯狂水世界/fkssj_battle_report.sh`（默认清缓冲并等回车；`--dump` 只抓现有缓冲）。


