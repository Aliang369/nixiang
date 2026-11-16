# 冒险与征途 bili 2.0.1 — 8 条语义假设的反编译反推验证

> 目标程序集：`06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll`（21,671,936 B，HybridCLR 热更真 IL 体）
> 工具：`ilspycmd 11.1.0.9782`（`DOTNET_ROOT=/Users/aliang/.dotnet`）
> 方法：整程序集反编译到 `13_scratch/verify_bili201/full/`（9090 个 .cs，69 MB）后逐点 grep 定位，行号均为该目录下文件的**实际行号**。
> 边界：只做静态反编译；未改任何 mod 源码、未装机、未 git 提交。
> 证据等级：【原文】=反编译代码逐字引用；【推断】=由字段序/调用关系推得、无运行时实证；【未验证】=工具失败。

---

## 0. 工具与失败项（先看这条，避免重复踩坑）

| 项 | 结果 |
|---|---|
| 整包反编译 `-p -o` | 成功，82 s，9090 文件 / 69 MB |
| 单类反编译 `-t` | 成功（除下述 IL 模式） |
| **IL 指令级转储 `--il`** | **不可用**。任何类型都崩：`Abort trap: 6`，末尾栈为 `--- End of stack trace from previous location --- / at ICSharpCode.ILSpyCmd.ILSpyCmdProgram.<Main>(String[] args)`。⇒ 本轮所有 IL 偏移（如「`IL_0039`」）**无法核对**，只能用 C# 层语义 + 语句顺序代替；规格卡里写死的 IL 偏移一律视为「待运行时复核」 |

---

## V1 · `StarsCollider.DoAttack` 的「accepter 为空 ⇒ 直接 return」作废分支

**结论：成立。** 且它位于方法体**第 3 条语句**，早于任何结算动作（伤害计算、`DamageTo` 飘字、命中特效、`CBattleActorUtil.Hurt`），把第 2 实参槽置空即等于「本次命中整段跳过」。

原文（`full/ActionGameLibrary/StarsCollider.cs`，方法 1523–1910，共 7 参、**实例方法**）：

```csharp
// 1523
public void DoAttack(IAttack attacker, IAccept accepter, AttackData attackData, AcceptData acceptData, Vector3 hitPoint, int seed = 0, SynHitData synHitData = null)
{
    ...
    _isNumb = false;                                                    // 1560
    _direction.Set(0f, 1f, 1f);                                         // 1561
    _damageInfo = DamageInfo.DefaultDamage;                             // 1562
    InteractiveGameObject interactiveGameObject = attacker?.interactiveGameObject;  // 1563
    if (accepter == null || accepter.interactiveGameObject == null)     // 1564  ← 判据原文
    {
        return;                                                        // 1566
    }
    bool flag = interactiveGameObject == null;                          // 1568
```

补充事实：
- 全方法体（1523–1910）内 `accepter == null` 只出现 **1 次**（1564）；另有 2 处早退：1639（`CanAccepter(skillID)` 不通过）、以及后续若干分支内 return，均与 accepter 判空无关。
- 参数位置：实例方法 ⇒ `this`=槽 0，`attacker`=槽 1，`accepter`=槽 2。规格卡的 `a1 + 8*(argBase+1)`（argBase=1）成立。
- `||` 短路顺序为「先判 accepter 自身，再判其 interactiveGameObject」——与规格卡「accepter != null && accepter.interactiveGameObject == null ⇒ return」**语义等价、写法不同**（本版是 `== null ||` 的负向写法，两个条件都会 ret）。

**对四项功能的影响（#2 无敌）**：机制成立——置空 accepter 槽 ⇒ 1567 return ⇒ 无伤害、无飘字、无命中特效、不进 `Hurt`/`AddOneHitHurt`，即「命中作废」。

**代码落点是否需要调整**：不需要。仍需保留「先自证 accepter 身份（=`g_playerHitter` 或 `IsPlayerSide(igo)`）再写槽」的 fail-closed 闸门；本版新增的旁证是 `DamageTo`（1965 起，飘字/血条 UI）在 DoAttack 之后才被调用，早退一定连带跳过 UI。

---

## V2 · `ActionCommand_AddProjectile.Execute` 里 `get_count()` 是 17 参 `AddProjectile` 的循环上界

**结论：成立（写 `_count` 确实改发射条数），但公式有变化，必须改口径。**

原文（`full/ActionGameLibrary/ActionCommand_AddProjectile.cs`，`Execute` 11–122，17 参 `AddProjectile` 124–799）：

```csharp
int num = lAddProjectile.count;                         // 65  ← 属性 count ⇒ IL 是 get_count()（_count 的 getter，LAddProjectile.cs:427-437）
if (lAddProjectile.emitByTargetCount)                   // 66
{
    num = ((interactiveGameObject.targets != null) ? interactiveGameObject.targets.Count : 0);  // 68  ← num 被**覆盖**，_count 失效
}
else if (lAddProjectile.count > 1)                      // 70
{
    num += commandData.action.projectileCount;          // 72  ← 只有 count>1 才叠加
}
...
for (int i = 0; i < num; i++)                           // 75  ← num 就是循环上界
{
    ...
    AddProjectile(character, interactiveGameObject, lAddProjectile, direction, position, vector,
        commandData.action, interactiveGameObject.actionManager.actionUsedCount, seed, actionGameObject,
        0, flag2, sequenceFromServer, buffTriggerProjectileId, relyOnSyn: false, projectileTriggerMode, buffObjectId);  // 119
}
```

17 参 `AddProjectile` 体内是否读 `_count`：**不读**。方法体（124–799）内 `data.count` / `data._count` 命中 **0 次**；出现的 `Count` 全是集合（`frameCommandMap.Count` 314、`actionList.Count` 338、`linkRepeatTexture.Count` 562/565、`moveFragment.Count` 611），另有 `data.emitByTargetCount`（541，是另一个字段）。⇒ 旧结论「它体内不读 `_count`」**未变**。

全工程读 `LAddProjectile.count` 的点：仅在 `ActionCommand_AddProjectile.cs:65 / 70`（在引用 `LAddProjectile` 的 14 个文件里全量 grep）⇒ 「唯一读点」成立。

**变化点（必须回报）**：
1. 规格卡写的上界公式 `n = count + Action::projectileCount + 1` **不对**；本版是 `num = count`，仅当 `count > 1` 时 `num += projectileCount`，**没有 +1**。
2. `emitByTargetCount == true` 的技能（SURROUND/LINK/CHAIN/PARABOLA 等按目标数发射）`num` 被 `targets.Count` 覆盖 ⇒ 对这些技能写 `_count` **完全不生效**。
3. 实际发数 = `3*count + projectileCount`（扇出 3）而不是 `3*(count+projectileCount+1)` ⇒ 扇出倍数**低于**原估算。

**对四项功能的影响（#3）**：写 `LAddProjectile._count` 改弹幕条数这条路线成立；但预算/扇出标定要按真实公式重算，且应在 `Execute` 帧里判断 `emitByTargetCount`，否则 STATS 里会计入「写了但没效果」的假成功。

**代码落点是否需要调整**：**建议调整**（观测口径，不是写入点）：① 不要把发数估成 `count+proj+1`；② 若要做「本秒新增发数」预算，应按 `num` 的实际算法累加；③ 观测 `K_PROJ`（17 参）继续只取证。

---

## V3 · `BaseSDKAdapter.PlayADVideo` 空桩、实现只在 `MiaoYueSDKProxy`

**结论：成立。** 并且**新增一条否定规格卡 [推断] 的证据：B 站 2.0.1 渠道的适配器仍是 `MiaoYueSDKProxy`（硬编码 new），不是 Bili 系类。**

原文：

```csharp
// full/BaseSDKAdapter.cs:188  —— 基类：方法体为空
public virtual void PlayADVideo(int adid, string uuid)
{
}
```

```csharp
// full/MiaoYueSDKProxy.cs:9    —— 唯一实现者
public class MiaoYueSDKProxy : BaseSDKAdapter
...
// :139  参数 (int adid, string uuid)，2 参、实例
public override void PlayADVideo(int adid, string uuid)
{
    if (!ConfigData.CFG_WatchAdvertisement.TryGetValue((uint)adid, out var _))
    {
        Log.infoError("OnBuyGoodsRsp error,unkown goods_id:{0}", adid);
        return;
    }
    Hashtable hashtable = new Hashtable();
    hashtable["goodsId"] = uuid;          // ← uuid(string) 与 adid 位置互换后塞进 hashtable
    hashtable["goodsName"] = adid.ToString();
    ...
    MiaoYueSDKGameObject.Instance.PlayADVideo(hashtable);   // :170
}
```

```csharp
// full/SDKProxy.cs:17  —— 实例是写死的
public static BaseSDKAdapter Instance = new MiaoYueSDKProxy();
```

全工程 `PlayADVideo` 定义仅 3 处：`BaseSDKAdapter:188`（空）、`MiaoYueSDKProxy:139`（真实现）、`MiaoYueSDKGameObject:262`（Java 桥，收 Hashtable）。`DS_Advertising.cs:154` 的调用点是 `SDKProxy.Instance.PlayADVideo(...)`。

**对四项功能的影响（#4）**：`K_PLAYAD` 挂 `MiaoYueSDKProxy::PlayADVideo`（argc=2、实例）**无需改类名**；规格卡「B 站极可能换 Bili 系适配器」的版本风险**在本版不成立**（除非 Java/SDK 层另有旁路，那是 native/Java 侧问题，不在本程序集）。

**代码落点是否需要调整**：不需要（照旧挂子类，不要挂基类空桩）。可删掉「类名可能变」这条待验证项，改为「已确认不变（本包）」。

---

## V4 · `start_watch_ad_rsp.result` 语义：==0 才拉 SDK，非 0 走跳过/特权分支

**结论：主判断成立；但「非 0 ⇒ 奖励到手」在代码层**没有**保证，是一条需要保留的风险。**

原文（`full/DS_Advertising.cs`，`OnGetADInfoRsp` 142–162，argc=1、实例；注册于 32 行 `regProto(E_SC_START_WATCH_AD_RSP, OnGetADInfoRsp)`）：

```csharp
private void OnGetADInfoRsp(Proto_CSRsp e)                       // 142
{
    if (e == null || e.start_watch_ad_rsp == null) return;       // 146-149
    Proto_SCStartWatchAdRsp start_watch_ad_rsp = e.start_watch_ad_rsp;                       // 150
    Log.infoPhone($"DS_Advertising OnGetADInfoRsp, adid:{...}, adname:{...} result:{start_watch_ad_rsp.result}");  // 151 ← 无条件打日志
    if (start_watch_ad_rsp.result == 0)                          // 152 ← 唯一「==0」判据
    {
        SDKProxy.Instance.PlayADVideo(start_watch_ad_rsp.ad_id, start_watch_ad_rsp.uuid.ToString());  // 154 ← 只有这里拉 SDK
        return;                                                  // 155
    }
    DispatchResult(start_watch_ad_rsp.ad_id, success: true);      // 157 ← 非 0 无条件「成功」
    if (start_watch_ad_rsp.result == 2)                          // 158
    {
        MsgCenter.SimpleTip("已使用免广告特权跳过观看");           // 160
    }
}
```

配套（同一文件）：
- `ReqADVideoResult` 202：`proto_CSReq.finish_watch_ad_req.result = ((!ret) ? 1 : 0);` ⇒ **ret=true→result=0（成功）、ret=false→result=1**，规格卡「反置」描述正确。发包 `SendPackge(2762u, ...)`（204），开始包 `SendPackge(2760u, ...)`（139）。
- `RspADVideoResult` 212：`DispatchResult(ad_id, finish_watch_ad_rsp.result == 0)`。
- `DispatchResult` 216–222：**只做一件事**——`EventManager.send("Event_WatchADResult", adid)`。
- 全包内 `start_watch_ad_rsp` 只在 `DS_Advertising` 出现（146/150/151/152/154/157/158），没有第二处消费。

**新发现（对 #4 关键）**：`Event_WatchADResult` 的 5 个监听者（`MW_TowerJingMo:423`、`MW_NewDailyGameplayEnter:217`、`UI_PartnerGacha:586`、`UI_ThemeSignInActivity:98`、`MW_WordCarnivalActSignln:114`、`MW_AFKBlessLevelAD:89`）**都不发 2762**：多数是刷新界面，只有 `UI_PartnerGacha` 直接发业务请求 `ReqDraw(..., 2)`。`finish_watch_ad_req(2762)` 的唯一发送点是 `ReqADVideoResult`，而它只被 `OnADVideoResult`（SDK 回调，179）和登录补发（175）调用。⇒ **`result != 0` 分支下客户端根本不发 2762**。

**对四项功能的影响（#4）**：`result=2` 确实能让「不出画」成立（不走 154），也能让 UI 认为广告成功（`DispatchResult(true)`）；但「奖励到手」依赖各业务 UI 自己的后续请求（如 `ReqDraw`）或服务端对 start 包的认可，**不是代码闭环**。规格卡「奖励 100% 由服务端下发、客户端唯一领奖动作 = 2762」这半句需要按上面修改。

**代码落点是否需要调整**：**建议调整**：① `K_GETADRSP`（写 2）仍保留为主落点，但验收时必须同时看 `AD43 PlayADVideo` 消失 **和** 目标玩法实际到账；② 保留 `K_ADRESULT`/`K_ADREWARD`（代回调 + 一次性门）作为「奖励链路兜底」，不要因为「result=2 就万事大吉」把它关掉；③ 日志断言可直接用 151 行那条 `infoPhone`（本版仍有）。

---

## V5 · `CtagHurtHPPara.m_llValue` → 命中通知 → `Character.Damage` 的落地链

**结论：成立（三处落地同源），但链路的**中间跳**与规格卡写的不一样：本版不是 `Hurt→AddOneHitHurt`，而是 `ProcDmgHit→Hurt … ProcDmgHit→AddOneHitHurt`；飘字/包值取 `m_llValue`（**未扣护盾**），真正扣血/死亡在 `EffectToCast→Character.Damage`。``**

逐跳调用链（全部为 **原文**，括号内是文件:行）：

```
DamageFactory.ProcDmgHit                                   (DamageFactory.cs:409)
 ├─ CtagHurtHPPara.Alloc(pTarget, attacker, llDamage, ...) (424)  ⇒ m_llValue = m_llCalculateDamage = llDamage
 ├─ CBattleActorUtil.Hurt(ref para, ref bDead, true, ref pllDmgToHeal, false)  (430)
 │   ├─ pHurtHPPara.GetValue()  <= 0 ⇒ return -1            (CBattleActorUtil.cs:20, 取值=85-88)
 │   ├─ IsActorDead / ProcDmgShare                          (24, 28)
 │   ├─ IsDamageImmune(attacker, target) ⇒ SetValue(0); return 0   (31, 33-37)
 │   ├─ GetCalculateDamageRef → HurtHealthSuper(HEALTH_SUPER / SHIELD) → SetCalculateDamage   (51-58)
 │   └─ HurtHp(para, ...)                                   (64 → 68)
 │       ├─ dmg = GetCalculateDamage(); hp = GetAttribRealValueBigInt(EN_ATTR_HP)  (83-84)
 │       ├─ if (hp <= dmg) { dmg = hp - 1; if (dmg <= 0) return -1; }             (86-91)  ← V6 的钳制
 │       ├─ CBuffTriggerManager.OnAttrChange(HP, hp, hp-dmg)                       (95)   ← 只触发 buff 事件
 │       └─ CEventDisp.OnActorBeDamage(target, attacker, hp-(hp-dmg), para)        (96)   ← 实际伤害量(已钳制)
 ├─ CHitNotifier.AddOneHitHurt(targetId, CS_HURT_HP, pHurtHPPara.GetValue(), ref showdamage, ...)  (458)
 │      ⇒ effectHurt.value = ToolKit.ToMsgInt128(llValue)   (CHitNotifier.cs:313)  ← 包值 = m_llValue
 │      ⇒ bool flag = GetAttribSumLife(actor) <= effectHurt.value; if (die|flag) CheckSetDieState(... bDie:true)  (336-341)
 └─ CHitNotifier.Broadcast()                                (460 → 444)
        ├─ SendToClient()                                   (451, 459-465, 事件 AFK_LOCAL_DAMAGE_EFFECT ⇒ 飘字/血条 UI)
        └─ EffectToCast()                                   (452 → 466)
              └─ (actor as Character).Damage(effectHurt.value.ToBigInt(), DAMAGE, knock:true)   (485)
                    ⇒ healthPointNow -= num; healthPointNowBigInt -= num   (Character.cs:152-153)
                    ⇒ CheckDead(knock)                                     (Character.cs:179) ⇒ 死亡
```

同源性核对（三处）：
| 落地 | 取值来源 | 是否被钳制 |
|---|---|---|
| 飘字 / 命中通知包 | `pHurtHPPara.GetValue()` = `m_llValue`（DamageFactory:458） | 否（原始值，连护盾都没扣） |
| 掉血（本地血条） | `effectHurt.value` = 同 `m_llValue`（EffectToCast:485 → Character:152） | 否 |
| 死亡 | `CheckDead`（Character:179），触发条件 `healthPointNowBigInt <= 0` | 否 |

另有一处**不看 m_llValue** 的旁路：`HurtHp` 内 `EN_ATTR_HEALTH_SUPER / EN_ATTR_SHIELD` 只改 `m_llCalculateDamage`，而包值取的是 `m_llValue` ⇒ 护盾吸收对公司包值**不起作用**（本版行为，供后续复核是否与 1.5.9 一致）。

**对四项功能的影响（#1）**：改 `m_llValue` 是唯一能让「飘字 + 本地掉血 + 死亡」同时变大的点，且三者同源（同一字段、同一条包），不会出现「数字变了血没掉」。`kKillMul=100` 的乘区写法继续有效。

**代码落点是否需要调整**：不需要（`Hurt` 帧 + `m_llValue` 不变）。**但理由要改**：不是「`HurtHp` 永不致死所以不能走它」，而是「`HurtHp` 内部的 `m_llCalculateDamage` 与包/掉血/死亡用的 `m_llValue` 不是同一个字段，改它不改结论」（见 V6）。

---

## V6 · `CBattleActorUtil.Hurt` 对 `HurtHp` 的钳制（最多打到 hp-1、永不致死）

**结论：钳制代码原文存在（成立）；但「因此永不致死」的错误推论必须否定（有变化）。**

原文（`CBattleActorUtil.cs`，`HurtHp` 68–99）：

```csharp
BigInt attribRealValueBigInt = CSpellUtil.GetAttribRealValueBigInt(pTarget, AttrType.EN_ATTR_HP);  // 83
BigInt bigInt = pHurtHPPara.GetCalculateDamage();                                                  // 84
BigInt bigInt2 = attribRealValueBigInt;                                                            // 85
if (bigInt2 <= bigInt)                    // 86  ← 伤害 >= 当前血
{
    bigInt = bigInt2 - 1;                 // 88  ← 钳到 hp-1
    if (bigInt <= 0) return -1;           // 89-92 ← 只剩 1 血及以下 ⇒ 一点血都不掉
}
bigInt2 -= bigInt;                        // 94
SingletonClass<CBuffTriggerManager>.Instance.OnAttrChange(pTarget, EN_ATTR_HP, attribRealValueBigInt, bigInt2);  // 95
CEventDisp.OnActorBeDamage(pTarget, pHurtHPPara.m_pAttacker, attribRealValueBigInt - bigInt2, pHurtHPPara);      // 96
return 0;
```

为什么「永不致死」不成立（两条原文证据）：
1. **`OnAttrChange` 不写 HP**：`CBuffTriggerManager.cs:266-274` 只有 `TriggerAttrChangeEvent(pCaster, iAttrId, llBefore, llAfter)`，后者（276-289 / 291+）遍历 buff、做 `BETT_AttrCompare` 触发与移除判定，**没有任何对 `_attributes[23]` / `healthPointNow` 的写入**。⇒ 钳制后的 `hp-1` 只是一个「告诉 buff 的账面值」。
2. **真正扣血在 `EffectToCast`，且无钳制**：`CHitNotifier.EffectToCast()`（466）→ `(actor as Character).Damage(value, DAMAGE, knock:true)`（485）→ `Character.cs:152-153` 直接 `healthPointNow -= num` → `CheckDead`（179）。这条路上的值就是 `m_llValue`（raw），血可以打到 ≤0 并正常死亡。
3. 另有 `HurtHp` 之外的致死判据：`AddOneHitHurt` 内 `GetAttribSumLife(actor) <= effectHurt.value` ⇒ `CheckSetDieState(bDie:true)`（CHitNotifier:336-341），即「包里的数值 ≥ 目标总血量 ⇒ 直接打死亡标记」。

**对四项功能的影响（#1）**：`m_llValue × 100` 能真秒杀小怪（走 336-341 与 485 两条），并不会被 `hp-1` 钳住；「不能走 `HurtHp`」的**结论仍对**，但**理由要换**：`HurtHp` 读的是 `m_llCalculateDamage`，包值/掉血/死亡读的是 `m_llValue`，改前者对结果无影响。

**代码落点是否需要调整**：不需要改挂点；**规格卡 #1 的语义描述需要改写**（把「HurtHp 钳制 ⇒ 永不致死」替换为「HurtHp 只影响 buff 账面与 `OnActorBeDamage` 报量，不影响 `m_llValue` 的落地」）。

---

## V7 · `ActionState` 的免疫位被伤害判定读取

**结论：成立，但精度要修正——`IsDamageImmune` 只读 1 个字段（`immuneDamage`），另外三个位在别的判定点被读。**

原文（`CBattleActorUtil.cs:298-305`）：

```csharp
public static bool IsDamageImmune(IActionGameObject pCaster, IActionGameObject pTarget)
{
    if (!pTarget.state.immuneDamage)   // 300 ← 唯一读的字段
    {
        return false;
    }
    return true;
}
```

字段声明（`ActionState.cs`，均为 `bool`，类 `ActionState : IDisposable`）：
- `public bool immuneDamage;` — 72
- `public bool immunePhysicalDamage;` — 74
- `public bool immuneMagicDamage;` — 76
- `public bool immuneDebuff;` — 78（额外发现，规格卡未列）
- `public bool invincible;` — 86

四个免疫位的**全部读取点**（整包 grep，逐字）：

| 读取点 | 读的字段 | 条件 |
|---|---|---|
| `CBattleActorUtil.cs:300` `IsDamageImmune` | `immuneDamage` | 由 `Hurt` 31 行调用 ⇒ 伤害归零并 `SetNoDamageType(UMMUNE_DMG)` |
| `DamageFactory.cs:56`（`GetOriginalDamage`） | `immuneDamage` \|\| (`skillType==1 && immunePhysicalDamage`) \|\| (`skillType==2 && immuneMagicDamage`) | 返回 `damage=0, invalidDamage=true` |
| `StarsCollider.cs:2208`（同名 `GetOriginalDamage`，副本） | 同上三字段 | 同上（DoAttack 侧用的是这份） |
| `DamageFactory.cs:183`（`ExecDamageCalc`） | `immuneDamage` \|\| `invincible`（另 `IsActorStateSetted(target,4/14)`） | 直接把伤害清 0 |
| `DamageFactory.cs:201`（`ExecDamageCalc`） | `!invincible && bigInt > 0` 才继续走 `ProcDmgHit` | 免疫时不扣血 |
| `StarsCollider.cs:1052`（DoAttack 命中筛选） | `accept.interactiveGameObject.state.invincible` | `acceptEnabled` 为假或 invincible ⇒ 该次命中被跳过 |
| `GameObjectManager.cs:1461`、`JudgeInvincible.cs:15`、`Force.cs:316` | `invincible` | 命中/推挤判定 |

写入点（供对照，说明这些位确实会被游戏自己改回）：`BuffFragmentEffect_EX_immuneAllDamage`（immuneDamage）、`_EX_immunePhyDamage`、`_EX_immuneMgcDamage`、`_EX_Invincible`；`InteractiveGameObjectData.cs:43`：`actionGameObject.state.immuneDamage = data.Invincible == 1;`；`LocalRoleAIManager` 会自己置 `invincible`。

**对四项功能的影响（#2）**：写 4 个位有效，且「同时把 `immuneDamage` 和 `invincible` 都置 1」覆盖面最大（前者管 `Hurt`/`GetOriginalDamage`，后者管命中筛选与 `ExecDamageCalc`）；只置 `immunePhysicalDamage`/`immuneMagicDamage` 需 `skillType` 恰为 1/2 才生效。

**代码落点是否需要调整**：**建议调整（增强）**：① 4 位按名解析的候选里，建议把 `immuneDebuff`（78）也纳入可选集合（不影响兼容）；② 免疫位需要「每 400ms 维持」的判断被证实必要（4 个 BuffFragment 与 `InteractiveGameObjectData` 都会覆写）；③ 若真机仍被无来源伤害打死，先看 `EffectToCast→Character.Damage`（V5）这条路——它绕过 `IsDamageImmune`，只受 `Character.Damage` 之前的 `CheckDead` 约束。

---

## V8 · `ProtocolManager.sendEvent` 里 `EventItemInt` 内 `Proto_CSRsp` 的偏移/结构

**结论：成立。`sendEvent` 一进来就能拿到 `Proto_CSRsp`（`e.data`），且早于 `get_result()` 的读取点（读取发生在被派发的监听者里）。偏移 +0x20 属【推断】——与 1.5.9 实证一致，本版未做运行时验证。**

原文（`full/ProtocolManager.cs`）：

```csharp
private class EventItemInt                       // 35
{
    public uint type;                            // 37
    public int reqSequence;                      // 39
    public int iCacheIndex;                      // 41
    public Proto_CSRsp data;                     // 43  ← 就是这里
    public int packageType;                      // 45
}

private void sendEvent(EventItemInt e)           // 192  ← argc=1、实例（私有）
{
    List<EventReference> value = null;
    if (!dict.TryGetValue(e.type, out value)) return;      // 194-197
    if (!cacheDict.ContainsKey(e.type)) cacheDict.Add(e.type, new List<EventReference>());
    cacheDict[e.type].AddRange(value);
    while (cacheDict[e.type].Count > 0)
    {
        EventReference eventReference = cacheDict[e.type][0];
        if (e.packageType == 2) { ... eventReference._listener(e.data); }        // 211
        else if (e.packageType == 1) { ... eventReference._listener(e.data); }   // 218
        else if (!NetGame.instance().IsStopPackageProcess && e.data != null)
             { eventReference._listener(e.data); }                               // 223
        cacheDict[e.type].RemoveAt(0);
    }
    ...
}
```

代码顺序（关键）：`sendEvent` 入口（192）→ `e.data` 在 211/218/223 三处被传给 `_listener` → 监听者即 `DS_Advertising.OnGetADInfoRsp`（注册于 DS_Advertising:32）→ **result 才第一次被读**（DS_Advertising:152）。⇒ 在 `sendEvent` 帧入口写 `e.data.start_watch_ad_rsp.result` 必然早于读取点，成立。

结构/偏移【推断】：`type`(u32,0x00) → `reqSequence`(i32,0x04) → `iCacheIndex`(i32,0x08) → `data`(引用，8 字节对齐 ⇒ 0x10) → `packageType`(i32,0x18)；加 il2cpp 对象头 0x10 ⇒ `data` 落在 **对象+0x20**，与 1.5.9 实测 +0x20 一致。结构发现在本版仍可用（字段名 `data` 未改名）。

其他相关原文：`Send`(183-190) 同步路径、`update()`(266+) 异步路径（`qEvent` 出队 → `sendEvent` → `recycleRsp`）；`GetEventItem`(233-247) 会 **`Reset` 复用** `EventItemInt` 实例（缓存池）⇒ 不能按对象地址长期缓存 `e.data`，必须每次从帧实参取。

**对四项功能的影响（#4）**：`K_SENDEVENT` 作为 `K_GETADRSP` 不可见时的等价入口成立；但**注意**：`sendEvent` 里 `e` 只在帧内有效（对象池复用），且 `e.data` 可能是 null（223 行有 `e.data != null` 判断）⇒ 写入前必须判 `data != null` 且类名含 `Proto_CSRsp`、`start_watch_ad_rsp != null`。

**代码落点是否需要调整**：不需要改（入口/顺序都对）；建议把「`+0x20`」标注为「推断，结构发现自适应的兜底判据」，并保留 `data != null` 的 fail-closed 检查。

---

## 汇总：对代码落点的影响

| 假设 | 结论 | 落点是否要改 |
|---|---|---|
| V1 accepter 置空作废 | 成立 | 不改 |
| V2 `_count` 是循环上界 | 成立，**公式有变化**（无 +1；`emitByTargetCount` 时失效） | 观测/预算口径要改 |
| V3 PlayADVideo 空桩+子类实现 | 成立，**且本包类名固定 `MiaoYueSDKProxy`** | 不改（可删「类名可能变」风险项） |
| V4 `result` 语义 | 主判断成立，**「非 0 ⇒ 奖励」非代码闭环** | 保留 A2 代回调兜底，验收加「实际到账」 |
| V5 三处同源 | 成立（包值 = `m_llValue`） | 不改 |
| V6 HurtHp 钳制 | 钳制存在，**「永不致死」不成立** | 挂点不改，**理由/文案要改** |
| V7 免疫位 | 成立（`IsDamageImmune` 只读 `immuneDamage`） | 建议补 `invincible` 必置 + 可选 `immuneDebuff` |
| V8 sendEvent/EventItemInt | 成立（`e.data` 在 172-223 前可见；+0x20 为推断） | 不改（加 null 检查） |

**一句话**：8 条里 **6 条成立、2 条有变化**（V2 发数公式、V6 「永不致死」推论），另 **V4 有一处必须保留的风险**（`result!=0` 分支不发 `finish_watch_ad_req` ）；四个功能的**挂点与类名/argc 全部不用改**，要改的是 #1 的语义说明、#3 的发数估算口径、#4 的验收口径。

---

## 附录 · 复现命令

```bash
export DOTNET_ROOT=/Users/aliang/.dotnet
ILSPY=/Users/aliang/.dotnet/tools/.store/ilspycmd/11.1.0.9782/ilspycmd/11.1.0.9782/tools/net10.0/any/ilspycmd.dll
DLL="06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll"
# 整包（69 MB / 9090 文件）
/Users/aliang/.dotnet/dotnet $ILSPY -p -o 13_scratch/verify_bili201/full "$DLL"
# 单类
/Users/aliang/.dotnet/dotnet $ILSPY -t ActionGameLibrary.CBattleActorUtil "$DLL"
```

产物：`13_scratch/verify_bili201/full/`（全量源码）、`13_scratch/verify_bili201/*.cs`（本轮用到的单类副本）。
