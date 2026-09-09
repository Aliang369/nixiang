# 王国大作战：前线 — 逆向分析（初判）

> 版本：**1.1.7646**（versionCode 17646）  
> 包名：`com.xr.wgdzztap`  
> 来源：真机 pull `base.apk`（2026-08-27）  
> 状态：**静态初判完成**；玩法 Hook 点须 HybridCLR/Lua 运行时 Trace 实锤

---

## 1. 基本信息

| 项 | 值 |
|---|---|
| 显示名 | 王国大作战：前线 |
| 包名 | `com.xr.wgdzztap` |
| 版本 | 1.1.7646 / 17646 |
| APK 体积 | ~509 MB |
| 发行 | TapTap（快捷登录）+ Q1 平台 SDK（APPID **2170**） |
| 玩法 | 肉鸽塔防：局内召唤/合成英雄、领主微操、波次防守、双人合作 |

---

## 2. 引擎与代码分布

| 特征 | 判定 |
|---|---|
| `libil2cpp.so` + `global-metadata.dat` | Unity **IL2CPP** |
| `ScriptingAssemblies.json` 含 `HybridCLR.Runtime.dll` | **HybridCLR 热更**（局内核心逻辑大概率在热更程序集） |
| `libtolua.so` + `assets/BinaryAssets/Lua.zip`（魔数 `rkt`） | **xLua/ToLua**，Lua 脚本自定义加密包 |
| `RocketClient.dll` / `BaseCore.dll` / `rkt_*` 命名 | 冰川/火箭引擎系（与多款 Q1 发行游戏同栈） |

**含义：** Il2CppDumper 只能稳定看到 **C# 桥接层 + Unity 引擎**；真正的伤害/经济/波次逻辑在 **HybridCLR 热更 DLL + Lua** 里，静态 dump 不完整（Dumper 报 `This file may be protected`，DummyDll 生成失败）。

---

## 3. 保护与登录（能不能改）

### 3.1 保护栈

| 组件 | 说明 | 风险 |
|---|---|---|
| `com.kiwivm.security.StubApplication` | **Kiwi VM** 壳，DEX 运行时加载 | 改 Java/smali 需继承/兼容壳，apktool 整包 decode Manifest 失败 |
| `libKwProtectSDK.so` | KwProtect | native 保护 |
| `libzxprotect.so` | 额外保护 | native |
| `libthemis.so` + TapTap Themis Provider | TapTap 侧检测 | 非腾讯 ACE，但仍可能杀调试/改包 |
| `libemulator_check.so` | 模拟器检测 | 真机优先 |

**不是**腾讯 ACE/TSS（无 `libtersafe2` / `AceApplication`）。  
→ **可以尝试星光外科组装**，但 Application 必须正确处理 Kiwi VM（`StarryApp` 继承链待运行时验证，⚠️ 待验证）。

### 3.2 登录与支付

- **TapTap SDK**：`TapTapLoginActivity`、`libtapsdkcore.so`
- **Q1 SDK**：Manifest 大量 `com.q1.sdk.*`；C# 侧 `OHPlatformSDK` / `NiPlatformSDK`
- 支付：支付宝 + Q1 `PayInitializer`；Il2Cpp 有 `PurchaseRecorder` 本地购单缓存

---

## 4. 已识别的玩法 Hook 面（来自 dump.cs / stringliteral）

以下为 **Lua↔C# 桥接 API 与事件名**，impl 在 HybridCLR/Lua，需 Trace 找真实实现。

### 4.1 战斗 / 伤害（塔防核心）

| 事件/API | 可能功能 | 可行性 | 备注 |
|---|---|---|---|
| `OnEvent_CalcDamageBefore` / `CalcDamageAfter` | 改 outgoing damage | ⚠️ 中 | 塔防「秒杀」主路径 |
| `OnEvent_BeforeReduceHp` / `SetChangeHp` | 扣血前拦截 / 改血量 | ⚠️ 中 | 「无敌」主路径 |
| `OnEvent_DeadWithInvincible` | 无敌死亡相关 | ⚠️ 中 | 需敌我/领主 vs 怪物过滤 |
| `OnEvent_FinalDamageReduction` / `CollectFinalDamage` | 减伤/伤害汇总 | ⚠️ 中 | 与秒杀配合 |
| `OnEvent_ShareDamage` | 分担伤害 | 低优先 | |
| `BattleValidateBridge.ValidateDamage` | **战斗伤害校验** | ❌ 硬门槛 | 存在 `SetDamageParamBase/Att`、`ResetValidateState` — 疑似**本地重放/服务端校验** |
| `BattleRecordBridge` | 战斗录像 | ❌ | 联机/结算可能对照录像 |
| `OnEvent_WaveFinish` | 波次结束 | 低 | 通关相关 |

### 4.2 技能 / 操作

| API | 可能功能 | 可行性 |
|---|---|---|
| `ReleaseSkill` / `ReleaseUltimateSkill` | 技能释放 | ⚠️ 中（零 CD 需找 CD 检查点） |
| `CheckUltimateSkill` | 大招条件 | ⚠️ 中 |
| `BreakSkill` / `CanTriggerSubSkill` | 打断/子技能 | 低~中 |
| `SkillStartAttackEvent` / `SkillStageFinishEvent` | 技能阶段 | 辅助 Trace |

### 4.3 局内实体 / 合成（玩法特色）

| API | 可能功能 | 可行性 |
|---|---|---|
| `AddEntity` / `GridAddEntity` | 召唤单位 | ⚠️ 中（可能仅客户端，联机高风险） |
| `MergeNewPetEvent` | 合成升阶 | ⚠️ 中 |
| `AddPropFormulaEffectEvent` | 属性公式 | 中~高（改攻击/攻速等） |

### 4.4 工具 / 低风控

| API | 可能功能 | 可行性 | 备注 |
|---|---|---|---|
| `TimeScaleManager.ChangeTimeScale` / `set_TimeScale` | **游戏加速** | ✅ 较高 | 客户端 TimeScale，优先验证项 |
| `TimeScaleManager.Pause` | 暂停 | ✅ 较高 | |
| `GameUtil.timeScale` / `set_timeScale` | 另一路加速 | ✅ 较高 | 与上可能重复 |
| `GameUtil.NoHitEffect` | 关闭受击特效 | ✅ 高 | 纯表现 |

### 4.5 经济 / 广告 / 内购

| API | 可能功能 | 可行性 | 备注 |
|---|---|---|---|
| `GameUtil.AddDiamond` | 加钻石 | ❌ 低 | 名称像 **编辑器/GM** 接口 |
| `OHPlatformSDK.Payment` / `SdkPPP` / `NiPlatformSDK.SdkShowAd` | 内购/广告 | ❌~⚠️ | 订单 `OrderSign` 服务端验；广告可走 callback hook 但可能二次校验 |
| `OnWatchAdCallback` | 广告完成回调 | ⚠️ 中 | 「免广告领奖励」候选 |
| `PurchaseRecorder` | 本地购单记录 | 低 | 不能替代服务端发货 |

### 4.6 局外养成（英雄、符文、商城）

逻辑在 **HybridCLR + 加密 Lua + 服务端存档**，静态未见可靠客户端-only 金币/钻石写入点。  
→ **局外资源修改：不建议作为首目标**（⚠️ 待抓包/存档结构验证）。

---

## 5. 功能优先级建议（按星光菜单落地难度）

| 优先级 | 功能 | 理由 |
|---|---|---|
| P0 试探 | 游戏加速（2x/3x） | `TimeScaleManager` 明确，改动面小，易 Trace 验证 |
| P1 | 局内无敌 / 秒杀 | 有完整伤害事件链，但 **`ValidateDamage` 可能拦结算** |
| P1 | 零 CD / 无限大招 | 技能事件齐全，需 HybridCLR 里找 CD 字段 |
| P2 | 局内银币/召唤加速 | 塔防局内经济，可能纯客户端 |
| P2 | 免广告奖励 | Hook `SdkShowAd` 或 `OnWatchAdCallback` |
| P3 | 内购免费 | Q1 验单，成功率低 |
| 不建议首做 | 联机合作、局外钻石/英雄全解锁 | `BattleValidate` + 服务端存档 |

---

## 6. 推荐技术路线

1. **静态**（已完成）：APK → lib 列表 / Manifest / Il2CppDumper partial dump  
2. **动态（必须）**：`STARRY_TRACE=1` 自注入，进 **单人普通局** 打 1~2 波  
   - 观察 `CalcDamageBefore`、`BeforeReduceHp`、`TimeScale` 是否命中  
   - 结算是否报数据异常 / 失败（验证 `ValidateDamage` 影响）  
3. **HybridCLR**：运行时 `/proc/pid/mem` 或 Trace 找 `Assembly-CSharp` 热更 PE（`MZ` 头）  
4. **Lua**：`Lua.zip` 为 `rkt` 自定义格式，需引擎解密流程或运行时 dump  
5. **构建**：外科 zip 组装；Application 兼容 Kiwi VM（⚠️ 待首包试验）  
6. **禁止**：默认长驻 Frida（壳 + Themis）；游戏包 `apktool b` 整包重建

---

## 7. 产物路径

| 路径 | 内容 |
|---|---|
| `02_apks/王国大作战前线/base_1.1.7646.apk` | 真机原包 |
| `03_decompiled/王国大作战前线/jadx_1.1.7646/` | jadx（有 86 error，Manifest 可读） |
| `03_decompiled/王国大作战前线/il2cpp_dump_1.1.7646/` | dump.cs / script.json / stringliteral.json |

---

## 8. 结论（初判）

**可以改，但属于「HybridCLR + Lua + 战斗校验」的中高难度项目，不是纯 Il2CPP 直 Hook 型。**

- **最有把握先做**：加速、纯客户端表现类  
- **核心价值但需验证**：局内无敌/秒杀/零 CD（取决于 `BattleValidateBridge` 是否强制校验联机/结算）  
- **短期不建议承诺**：局外货币、内购、联机作弊

下一步：开 Trace 单人局验证伤害链与 TimeScale；确认 Kiwi VM 下星光首包能否正常启动。

## 动态分析进度（2026-08-27，**暂停**）

> **阶段总结**：[B路线_阶段总结_暂停_20260827.md](./B路线_阶段总结_暂停_20260827.md)

| 项 | 状态 |
|---|---|
| Trace so + WgdzzMod.cpp | ✅ dlopen/open/exit hook + early-so |
| SignSpoof | ✅ `com.xr.wgdzztap` + TapTap 原版证书 |
| 外科 zip 组装 | ✅ `scripts/surgical_wgdzz_1.1.7646.sh` |
| Kiwi 注入 | ✅ ACF + classes5；K1 底包 **1s 延迟** loadLibrary |
| KwProtect K1 | ✅ 绕过 signal 9；推荐 `surgical-kwK1-1sSigned.apk` |
| 壳 VM | ✅ `sub_6897C ret=0` |
| 渠道 dex | ❌ `ClassNotFound: ChannelApplicationListener` |
| 进程存活 | ❌ ~2s 退出（非 SIGKILL；疑 Java exit） |
| A 路线保渠道 SigningBlock | ❌ **证伪**（920B 有效载荷已与官方一致；扩 1600B 仍失败） |

### 硬阻塞（定稿）

改签后 **Q1/TapTap 渠道层 dex 未在运行时解出**（见 [jiagu_Q1dex链_20260827.md](./jiagu_Q1dex链_20260827.md)）。  
`SignSpoof` **救不了** native/kadp 对 **APK v2 整包签名** 的校验；保渠道 SigningBlock pair **亦无效**。

| 路线 | 状态 |
|---|---|
| A. Root 官方包注 so | 用户禁止 |
| B. patch native 过签 | K1 ✅；kadp 盲 patch ❌；渠道块 ❌；**待续 v2 读签** |
| C. 官方同签 | 不可行 |

### B 路线笔记

- [B路线_kadp过签_20260827.md](./B路线_kadp过签_20260827.md) — 试验明细  
- [B路线_阶段总结_暂停_20260827.md](./B路线_阶段总结_暂停_20260827.md) — **暂停总结**

Trace：`adb logcat -s StarryTrace StarryMagic:V WgdzzAppComponentFact:V`
