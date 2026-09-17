# 闪耀吧噜咪 1.5.3 — 热更 DLL 解密、战斗权威判定与四功能可行性

> 日期：2026-09-17
> 设备：`f9a116a2`（OPD2515，Android 16 / arm64-v8a，KernelSU root）
> 样本：`com.bilibili.lumi.cn` 1.5.3 / vc42（TapTap 渠道，原版）
> 前置文档：[加固与可行性初判_1.5.3_20260917.md](加固与可行性初判_1.5.3_20260917.md)
> 本轮性质：**只读分析**（运行态内存导出 + 离线解密 + 反编译）；未改包、未安装、未卸载、未清数据、未碰上报与存档。
> 收尾状态：**2026-09-17 已按用户口令收尾清理**（滚动物已删，只留本目录文档与复现脚本；详见第九节）。文中出现的 `03/06/09` 路径均为分析期记录，复查需按「复现」重新生成。

---

## 一、结论摘要

1. **热更 DLL 已完整拿到并反编译。** 自定义 `LGEB` 容器已离线解密，产出 `Game.dll`（21,086,720 B）与另外 18 个程序集；`Game.dll` 含全部游戏逻辑（10,419 个类型）。
2. **战斗是服务端权威（server-authoritative）**：伤害/血量/技能可用性全部由战斗服计算并下发，客户端只做表现与发包。**攻击倍率、上帝模式、无限技能在客户端改数值没有真实效果。**
3. **捕捉是本作里唯一「客户端上传操作、服务端据此结算」的链路**：`Cmd_Adventure_EventCatchLumi_Catch_Req.GestureResultList`（cmd 3053）由客户端填充；而游戏自带的月卡自动冒险路径会**把全部音符填成 Perfect**（`_FillPerfectOperations`）。这是「有操作空间」的确切位置。
4. **客户端校验点已定位**：`CheatChecker` 四项时间/加速校验（命中即弹框退出）；网易易盾（HTProtect）token 每 30 分钟上报一次（cmd 4150）并在进游戏时上报角色信息。二者**本轮不触碰**，仅记录。
5. 客户端内置整套 GM/调试面板（含战斗 GM 指令，如 `GM_BATTLE_NO_MANA`），指令由服务端权限判定，不属可用路径。

---

## 二、LGEB 热更包解密（已实锤，可复现）

`aa_enc` 目录下的热更包是自定义容器：

```
偏移 0x00  4 字节   魔数 "LGEB"
偏移 0x04  uint32LE  版本（本版 = 34）
偏移 0x08  ...       AES-128-CBC 密文（PKCS#7 填充）
```

密钥派生（由 `BundleCipherFormat.BuildSalt` + `EncryptedBundleProvider.DeriveKeyIv` 反汇编核出，并用离线解密命中 `UnityFS` 验证）：

```
salt      = BuildSalt()                              # 确定性 16 字节，见脚本
password  = UTF8("<bundle_hash>:<version>")          # bundle_hash 例：2517e568e31ff963bd6e37787662622e
sha       = SHA256(salt || password)
key, iv   = sha[0:16], sha[16:32]
明文      = AES-128-CBC-Decrypt(key, iv, data[8:])
```

- 运行态静态字段实证：`s_Key = "LumiGoKey1234567"`、`s_IV = "LumiGoIV12345678"`（cctor 用的字面量，仅用于旧路径；本版实际生效的是上面的派生密钥）。
- 复现脚本：`04_scripts/闪耀吧噜咪/lgeb_decrypt.py`
  ```bash
  /usr/bin/python3 04_scripts/闪耀吧噜咪/lgeb_decrypt.py \
    06_dumped/闪耀吧噜咪/1.5.3/runtime_bundles/e2ef324cf561f8d65c38bceaff962afb_2517e568e31ff963bd6e37787662622e.bundle \
    2517e568e31ff963bd6e37787662622e \
    06_dumped/闪耀吧噜咪/1.5.3/runtime_bundles/dlls_assets_all.dec.bundle
  ```
- 产物：`06_dumped/闪耀吧噜咪/1.5.3/hotfix_dlls_real/`（19 个 TextAsset，含 `Game.dll`）。

> 说明：容器是**加密但未压缩、未签名**；只要知道 bundle_hash 就能重加密，这一点对后续「热更 DLL 替换」路线很关键。

---

## 三、战斗架构判定（决定三项功能能不能做）

### 证据（均在 `Game.dll` 内）

| 位置 | 事实 |
|---|---|
| `BattleServerInvoker.BattleAttack/BattleSkill` | 客户端只发「攻击 / 用哪个技能（SkillId）」，**不上传伤害值** |
| `BattleServerInvoker.BattleLogin/BattlePing/SnapShot` | 独立战斗服会话；客户端上报渲染帧号，服务端下发状态 |
| `BattleClientExecer` cmd 11501/11502/11503/11504/11510 | 服务端下发 `BattleActionEvent`、`DamageHit`、`BattleStateSnapshot` |
| `BattleHeroProperty.StartProperty/UpdateProperty(LumiState)` | 血量、血量上限、死亡、技能CD/充能/可用**全部来自服务端下发** |
| `GamePlayBattle.OnNotifyDamageHit` | 客户端只负责飘伤害数字 + 受击表现（`BattleDamageNumManager`） |
| `BattlePlayerSkillState` / `BattleHero._OnHPUpdated` | 纯播放状态机/视图刷新 |

### 由此得到的结论

- **攻击倍率**：客户端没有伤害计算入口，乘任何系数只改飘字，服务端血量照旧 ⇒ ❌ 无真实效果。
- **上帝模式**：血量与死亡判定由服务端快照驱动，本地锁血只会立刻被覆盖/表现不一致 ⇒ ❌ 无真实效果。
- **无限技能**：`Skill1CanUse / Skill1Charge / Skill1Cost` 来自服务端，本地门禁只是「不让你乱点」；服务端仍会按帧校验 ⇒ ❌ 无真实效果。
- 可行的替代方向只有**自动化**（自动攻击/自动放技能，抢占可用窗口），它不改变任何数值，属于「手感层」收益。

---

## 四、捕捉链路（唯一有真实杠杆的一条）

### 4.1 公式（客户端 UI 用它显示，服务端按同一套配置结算）

`AdventureCalcFormulaUtils.CalcCatchRate`：

```
rate = (baseCatchRate + opCatchRate) * 0.01
     * (foodRate * 0.0001) * (catchItemRate * 0.0001) * (runawayRate * 0.0001)
     * (1 + styleCatchBuffRate * 0.0001)
```

- `baseCatchRate`：`TbLumiCatch.CatchRate`（配置）
- `opCatchRate`：**敲击操作分** = `Σ tapRate[level]`，每级 = `CatchTapNoteRate.GetCatchRate / 音符总数`
- `foodRate`：`10000 + Σ 每类食物最高 CatchRate`（喂食，cmd 3059）
- `catchItemRate`：道具加成；`runawayRate`：`TbEscapeCatchMul`（已逃跑次数越多越高）；`styleCatchBuffRate`：风格加成

### 4.2 请求/应答

```
客户端 → 服务端  cmd 3053 Cmd_Adventure_EventCatchLumi_Catch_Req
                 field1 EventId
                 field2 CostItem(CmdItemShow)
                 field3 GestureResultList : repeated int32   ← 每次敲击的判定等级
服务端 → 客户端  Cmd_Adventure_EventCatchLumi_Catch_Ack
                 Result(0=抓到) / LumiUid / SuccWeight / ShowInfo
```

判定等级枚举：`CatchTapNoteLevel { General=0, Good=1, Perfect=2, Miss=3 }`。

**关键点：`GestureResultList` 完全由客户端填。** 服务端返回值 `SuccWeight`（万分比）就是它算出来的成功率，客户端拿它做摇晃动画。

### 4.3 游戏自己就有「全 Perfect」通路（重要）

自动敲击的判定点在伴奏控制器里，而且**不只月卡用户生效**：

```csharp
private bool _NeedAutoPerfectCatch()
{
    return (bool)Entity<Player>.Instance.IsCanAutoAdventure || Entity<Player>.Instance.AdvAccState != AdvAccEnum.Normal;
}
// _DelayShowMusic()：命中该条件时，在每个音符的 Perfect 时点自动触发 _OnFingerDown(0, null)
```

即：**只要处于「自动冒险」状态，游戏就会在每个音符的完美时点自动点击 ⇒ 这一次捕捉的 GestureResultList 天然全是 Perfect**（服务端必须接受，否则自动冒险用户全部报错）。月卡则更进一步：直接 `_FillPerfectOperations()` 跳过小游戏。低一级的自动冒险开关 `_isAutoAdventure` 是本地 `ReceiveDataBool`，`SetAutoAdventure(true)` 只向服务端发一个 `AdventureAutoSettingReq(true)` 设置同步。

```csharp
// AdventureEventCatchLumi 系列 _PreCatch()
if (Entity<Player>.Instance.AdvAccState != AdvAccEnum.Normal) {
    _FillPerfectOperations();               // 把 TapNoteList 里每个音符都记成 Perfect
    _UpdateCatchRate(immediately: false);
    _model.SetState(BattleCatchState.Finish);// 跳过节奏小游戏直接结算
} else {
    _musicGameController.StartGame();        // 手打
}
```

- `AdvAccEnum { Normal = 1, MonthCard }`，`AdvAccState` 就是**月卡「自动冒险/加速」开关**，本地持久化在 `GamePrefs("AdvAccState")`，`LoadAdvAccState()` 只在**账号没有月卡 BenefitsType13** 时回落到 `Normal`。
- 同一开关还带来：`GapTime => 0`、跳过等待、`_IsCanAutoClick() = true`、`TimeScale = 月卡加速倍率`。
- 也就是说：**「每次操作提高概率」的正解就是让每次操作都是 Perfect**；服务端接收到的會是月卡用户合法产生的同一种载荷。

### 4.4 其它加成链路（都是服务端写、客户端只发请求）

| 链路 | cmd | 说明 |
|---|---|---|
| 喂食 | 3059 `Adventure_EventCatchLumi_Feed_Req` | 提升 `foodRate`，`_CanFeed` 校验同类型食物等级/剩余次数/道具数量 |
| 逃跑后加成 | 3055 `FinishRunAway` | `AlreadyRunawayTimes` → `TbEscapeCatchMul`，失败次数越多概率越高 |
| 战斗后捕捉 | `AdventureEventCatchLumiFightReq` | `AlreadyFightTimes != 0` 时换手势/另一套 TapRate |

---

## 五、校验面与风控（本轮只读记录，不修改）

### 5.1 客户端校验：`CheatChecker`

| 编号 | 判据 | 触发结果 |
|---|---|---|
| Cheat1 | `TimeScaleManager.TimeScale - Player.AdvTimeScale > 0.001` | 弹 `SystemTips.System4107` 确认框并锁关闭按钮 → `ExitApp()` |
| Cheat2 | 每 2 秒：`Σ游戏时间 / Σ真实时间 > 1.5` | 同上 |
| Cheat3 | 启动至今 `游戏时间 / 真实时间 > 1.5` | 同上 |
| Cheat4 | 服务器时间 27~33 秒区间内，UTC/游戏时间比值 > 1.5（连续 3 次） | 同上 |

⇒ **任何全局时间缩放都会被抓**，除非同时抬高 `Player.AdvTimeScale`（`TimeScaleManager.CheatCheck` 就是拿它当上限）。`IsDebugAdvAccMode()`（`_debugAdvAcc`，默认 false）可让冒险内时间倍率直接等于 2f —— 这是客户端自带的调试口。

### 5.2 网易易盾 / HTProtect 上报链

- `LoginClientExecer`：登录后取号 `YiDunManager.GetTokenAsync` → `SDKDataCenter.YiDunToken` → `LoginServerInvoker.SendYiDunReportToken(token)`（**cmd 4150**，ack 4151），并每 `1800s` 刷新重报。
- `_OnEnterGameAck` 里调用 `YiDunManager.SetRoleInfo(角色ID, 昵称, UID, 服名, 服ID)` —— 反作弊侧拿到完整角色身份。
- 客户端还带一个「易盾」调试分类（初始化 / 本地库检测 / 取号 / 上报角色 / 查看缓存 Token / 手动上报），说明这条链是正式功能而非残留。
- 本轮**不改动**该链路（用户明确要求）。

### 5.3 其它

- 内置 GM/调试面板（`GameDebugLogController`）含大量指令，其中战斗类如 `GM_BATTLE_NO_MANA`、`GM_BATTLE_UPDATE_LUMI_HEALTH`、`GM_BATTLE_CRIT` 等；全部走服务端 GM 权限校验，普通账号不可用（且属服务端写操作，不在授权范围）。
- 云存档 / 请求签名 / Themis 在前置文档已记录（Java 层 `ArchiveManager`、GSC `ParamsSignInterceptors`、`libthemis.so`），本轮未动。

---

## 六、四项功能逐项结论

| 功能 | 判定 | 依据 | 若要做，可做的形态 |
|---|---|---|---|
| 攻击倍率 | ❌ 无真实效果 | 伤害由战斗服计算并下发（`Notify_DamageHit`） | 只改飘字（不建议）；或自动攻击提高出手覆盖率 |
| 上帝模式 | ❌ 无真实效果 | 血量/死亡为服务端 `LumiState` 快照 | 只改血条（不建议） |
| 无限技能 | ❌ 无真实效果 | `SkillXCanUse/Charge/Cost` 服务端下发 | 自动放技能（到点就发请求） |
| 百分百捕捉 | ✅ 有真实杠杆 | `GestureResultList` 客户端上传；游戏自带全 Perfect 通路 | 让每次捕捉都走「全 Perfect 操作」+ 喂食/逃跑加成 |

---

## 七、下一步可选路线（待用户决定）

- **路线 A（推荐，真实收益）**：只做捕捉 —— 让捕捉流程始终走 `_FillPerfectOperations` 分支（等价于月卡自动冒险的结算载荷），必要时叠加喂食。风险面最小，因为载荷与服务端已支持的模式一致。
- **路线 B（手感层）**：自动攻击 / 自动技能 / 自动捕捉，不改任何数值；等价于「自动玩」。
- **路线 C（不建议）**：改飘字伤害、锁血条显示 —— 与真实状态不一致，容易被玩家自身察觉，且无收益。
- **实现载体**（三选一，需先定路线）：
  1. 热更 DLL 注入：改 `Game.dll` IL → 用 `lgeb_decrypt.py` 反向重加密 → 替换设备 `aa_enc` 缓存包（需先验证 Addressables 是否校验包哈希）；
  2. 星光菜单：Java 菜单 + native so 运行态改字段/挂钩（HybridCLR 解释执行，挂钩点需另定）；
  3. 仅运行时内存改字段（分析验证用，不做交付）。

---

## 八、待验证 / 风险

- 服务端是否校验 `GestureResultList` 的**长度与内容合法性**（例如必须等于音符数、是否允许全 Perfect）。这是捕捉路线唯一的关键未知项，可通过单次低风险实测确认（同一 event 内对比操作分与返回 `SuccWeight`）。
- Addressables 是否对 `aa_enc` 缓存包做哈希校验（决定「替换缓存包」这条实现路线是否可行）。
- 月卡权益 `BenefitsType13` 是否被服务端用于校验自动冒险状态（`LoadAdvAccState` 只在本地判定，但 `AdventureAutoSettingReq` 会向服务端同步开关值）。
- 易盾 token 与行为画像的关联强度未知；本轮不触碰该链路。
- 未做任何装机/出包动作；出包与装机需用户另行明确授权。

## 九、证据与产物

> **收尾（2026-09-17，用户口令「收尾+清理+git，只保留 10 的文档」）**：本作滚动物已清，本节路径均为**当时记录**，如需复查须按「复现」列重新生成。

- 已删（可再生）：`03_decompiled/闪耀吧噜咪`（104M，jadx）、`06_dumped/闪耀吧噜咪`（66M，热更 DLL/解密 bundle/内存 carve）、`09_test/闪耀吧噜咪`（1.0G，运行态 dump）。同一口径实测释放约 1.17G。
- 保留：本目录文档、`04_scripts/闪耀吧噜咪/lgeb_decrypt.py`（自写复现工具，工作区规则规定随复现链永留）。
- 未动：`02_apks/闪耀吧噜咪/闪耀吧噜咪_1.5.3_taptap_official_from_device.apk`（192M，工作区规则：原包用户手动管理）。

- 热更程序集（解密后）：`06_dumped/闪耀吧噜咪/1.5.3/hotfix_dlls_real/Game.dll.596d1a96abb4.bin` 等 19 个
- 解密后 bundle：`06_dumped/闪耀吧噜咪/1.5.3/runtime_bundles/dlls_assets_all_2517e568e31ff963bd6e37787662622e.dec.bundle`
- 解密脚本：`04_scripts/闪耀吧噜咪/lgeb_decrypt.py`
- 运行态数据（登录后）：`09_test/闪耀吧噜咪/runtime_login_20260917/`（maps / il2cpp_bss / heap dumps）
- 全量反编译（临时，可再生）：`/tmp/lumi_cs/Game.dll.596d1a96abb4.decompiled.cs`（1,478,183 行）
  复现：`~/.dotnet/tools/ilspycmd --nested-directories -o <out> <Game.dll>`
