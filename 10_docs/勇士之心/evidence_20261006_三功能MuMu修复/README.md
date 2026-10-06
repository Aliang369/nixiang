# 勇士之心 1.0.3 — 秒杀 / 伤害倍率 / 无限技能 三功能 MuMu 失效修复（2026-10-06）

状态：**已修复并双侧用户实测通过**（MuMu 模拟器 + 真机 96f0d883，同一份包）。
判定：CLOSED（用户原话「mumu模拟器没问题了，手机也没问题了」）。

## 根因（一句话）

模拟器侧走「自举 JS」路线。旧自举把类的 `prototype` 方法也并入逻辑钩子的扫描面，
游戏包里的属性初始化方法 `.bJV`（含 `maxApDefRate`+`baseCritDamage`，**无** `atkEntity`）
抢先命中并置 `logicDone=1`，真正的伤害公式 `battleLogic .GD`（**静态**方法，含 `atkEntity`）
再也挂不上。`秒杀/伤害倍率/无限技能` 三个咽喉**同在 $KC 这一个类**上（离线复现：
`kill_formula/skill_cost/skill_gate/death_route` 全在 `battleLogic::$KC`）⇒ 三个一起失效；
无敌/移速/自动战斗在别的类上，不受影响。

第二个必要条件：覆盖安装（保数据）后残留的是**上一版**自举 `main.js`，
不重写就继续跑旧 driver ⇒ 新增 `*ze*` 且非 pristine 即按「上一版自举」重写。

## 修法

1. `04_scripts/勇士之心/gen_ysxz_boot_js.py`：自举扫描去掉 `shim()`，`tryLogic` 只看类的
   **静态**方法（与真机原生路线同语义），禁止再合并 prototype。
2. `04_scripts/勇士之心/ysxz_1.0.3_menu/YsxzPatchJs.h`：伤害公式指纹加 `atkEntity`
   （`maxApDefRate`+`baseCritDamage`+`atkEntity`），并保留 `.bJV rejected by fingerprint` 诊断。
3. `04_scripts/勇士之心/ysxz_1.0.3_menu/YsxzMod.cpp`：`main.js` 自愈——`*ze*` 且非 pristine
   一律重写为本版 patched（覆盖安装后旧自举不再残留）。
4. `04_scripts/勇士之心/simulate_ysxz_discovery.py`：离线指纹同步加 `atkEntity`。

## 决定性证据（`decisive_lines.txt`，MuMu emulator-5554，pid 6138）

```
21:52:32  auto-battle privilege hooked = .prototype.fDj
21:52:35  REPORT  formula=undefined ... movesys=NO auto=yes     <- 进战斗前（$KC 未加载）
21:53:07  resolve: 36/36 模块就绪                               <- 进入战斗，战斗逻辑包加载
21:53:08  battle-logic class hit: damage formula = .GD          <- 真公式命中（旧版是 .bJV）
21:53:08  feature1 hooked                                       <- 秒杀
21:53:08  skill cost = .FL  fields: ...                         <- 无限技能 消耗
21:53:08  skill gate = .FK  unlockedField=Dv                    <- 无限技能 门
21:53:08  feature3 hooked (gate=true cost=true)                 <- 无限技能
21:53:08  scanned all 35 modules ... hooks: logic=1 ent=1 sys=1 auto=1
21:53:11  REPORT  formula=GD apply=hY gate=FK cost=FL death=Hs movesys=yes auto=yes
21:53:11  REPORT  MOD=... dmg=28                                <- 用户当时已把伤害倍率拉到 28
```

对照：同一份包在**进战斗前**（$KC 未加载）`REPORT formula=undefined`，
且第一趟扫描 `hooks: logic=0`（旧版此处会误挂 `.bJV`）。

## 离线复现（最短）

```bash
python 04_scripts/勇士之心/simulate_ysxz_discovery.py _codex_tmp/plainroot
# => verdict OK；kill_formula/skill_cost/skill_gate/death_route 均落在 battleLogic::$KC
# game 包 atkEntity 计数 = 0（.bJV 不可能匹配新指纹）；battleLogic 唯一命中
```

## 产物

- so md5 `12821a23cf75b62a817cff0960472179`（160848 B）
- 正式包 `勇士之心_1.0.3_starry-final-releaseSigned.apk`
  md5 `9a158ae5dd7e31cfcfbca774efdb59ea`，sha256 `c512364d…`，64,679,723 B
- 双侧同包：MuMu `emulator-5554` + 真机 `96f0d883`（设备 base.apk md5 == 本地产物）

## NOT PROVEN / 边界

- 真机侧本轮未单独抓 logcat（走原生 splice 路线，功能点未改；用户口述「手机也没问题了」）。
- 1.0.4 未适配（硬约束 #8：只改 1.0.3）。
