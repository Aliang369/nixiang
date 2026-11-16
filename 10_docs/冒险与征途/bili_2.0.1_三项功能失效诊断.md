# 冒险与征途 bili 2.0.1 — 三项战斗功能失效诊断（A/B/C 三组反编译核对）

> 现场（用户实测）：**免广告有效；秒杀小怪 / 无敌 / 技能增伤 三项无效**。
> STATS（每 30s）：`命中 打怪=3503 无敌=0 免疫位=68 投射=168 | 跳过 非玩家侧=130 首领/未知=0 发射者非玩家=15 预算=0 | 投射秒峰值=12 槽=24 para深度=1`
> 另：`BigInt off=mant+0x10 exp+0x18 (src=field)`
> 程序集：`06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll`；工具 `ilspycmd 11.1.0.9782`；反编译全量产物 `13_scratch/verify_bili201/full/`（9090 文件）。
> 边界：只做静态核对，**未改任何 mod 源码 / 未装机 / 未提交**。行号为反编译产物的实际行号。
> 证据等级：【原文】=逐字引用；【推断】=由字段序/调用链推得，无运行时实证。

---

## 结论速览（先看这张表）

| 组 | 关键结论 | 是否命中现场症状 |
|---|---|---|
| A1 | `IAccept` 只有 1 个属性 `interactiveGameObject`（+sourceObject/rootObject/model/camp/acceptEnabled/disposed/Accept 等）；`DoAttack` 全库 3 个调用点**全部传 Hitter** | — |
| A2 | **`Hitter : IAttack, IAccept`（Hitter 确实实现 IAccept）** ⇒ 「accepter==g_playerHitter」判据**结构上成立**，「不实现接口」这个猜想**被否定** | 无敌=0 的根因在别处（见 A4 + 命名陷阱） |
| A3 | 玩家 state：`ActionGameObject._state`（protected **字段**，声明在**基类**，不在 Character）；公开访问器是 **属性** `state`（getter） | 按名取 `state` 字段必失败（是属性）；须解析 `_state` 并沿父类链 |
| A4 | `IsDamageImmune` 读**被打方** `pTarget.state.immuneDamage`；`StarsCollider.cs:1052` 读的是 `accept`（**被打方**）的 `state.invincible`，且该判定**在 DoAttack 之前 `continue`** ⇒ 免疫/无敌一旦生效，DoAttack 根本不进 ⇒ **「无敌」计数结构性恒为 0** | **命中「无敌=0」**（计数口径错，不等于功能无效） |
| B1 | `count` 是**属性**（ProtoMember(24)），背后字段 **`_count`**，`int`，默认 1 | 写错字段名 ⇒ 静默无效 |
| B2 | `LAddProjectile lAddProjectile = commandData.data as LAddProjectile;`（:25）；`ActionCommandData` 里只有 `data`，**没有 `_data`** | — |
| B3 | `data:IExtensible` / `action:Action` / **`actionGameObject:IActionGameObject`**（施法者只能从这里拿） | 读错字段 ⇒ 「发射者非玩家=15」的判定不可靠 |
| B4 | 写 `_count` 会写到**共享配置对象**（`CopyTo` 是引用复制，`data` 来自 `LEventData.eventData`），不是逐次对象；另有 `emitByTargetCount` 覆盖与 5 处 `return null` 闸 | **命中「技能增伤无效」的高概率原因之一** |
| C1 | `GetValue()→AddOneHitHurt→Broadcast→EffectToCast→Character.Damage` 链**成立**（逐跳行号见下） | 修好偏移即可生效 |
| C2 | `BigInt` 是 **struct**：`private double m_mantissa;` + `private int m_exponent;` ⇒ 内联 8+4（补齐 16） | 与 log 的 +0x10/+0x18 误差一致 |
| C3 | **`m_llCalculateDamage` = `m_llValue` + 0x10（16 字节）**；BigInt 内 mant +0 / exp +8 ⇒ 正确写入地址 = `para + 0x20` / `para + 0x28`（【推断】与根因 #1 吻合）；**建议两个 BigInt 一起写** | 这是「秒杀静默无效」的修法 |

---

## A · 「无敌」计数 = 0

### A1 · `IAccept` 成员 + `DoAttack` 的 accepter 来源

【原文】`full/ActionGameLibrary/IAccept.cs`（全文 28 行）：

```csharp
public interface IAccept
{
    InteractiveGameObject interactiveGameObject { get; }   // :7  ← 「igo」就在这，是个**属性**
    InteractiveGameObject sourceObject { get; }            // :9
    InteractiveGameObject rootObject { get; }              // :11
    IModelUnity model { get; }                             // :13
    int camp { get; set; }                                 // :15
    bool acceptEnabled { get; set; }                       // :17
    bool disposed { get; }                                 // :19
    AcceptDataGroup GetCurrentAcceptData();                // :21
    void UpdateCurrentAcceptData(int frame);               // :23
    bool Accept(ref Vector3 direction, AttackData attackData, AcceptData accepterData, int attackerHash, bool isSyn = false, bool isNumb = false, bool isCrit = false, int seed = 0, bool attackerIsMonster = false);
    void Dispose();                                        // :27
}
```

`DoAttack(` 全库只有 3 个调用点，**accepter 实参一律是 `.hitter`**：
- `StarsCollider.cs:1235` `DoAttack(attack, accept, attackData, acceptData, _hitPoint, Random.seed);`（`accept` 来自 `_accepterList`，1039/1043 行）
- `StarsCollider.cs:1282` 同形（`sheildBreakAcceptData`）
- `BattleContextCharacterSynModule.cs:871` `...DoAttack(interactiveGameObject2?.hitter, interactiveGameObject3.hitter, ...)`（服务端同步命中）

`_accepterList` 的注册者（`AddAccepter` 全部调用点）也都是 hitter：
`Command_LoadPlayer.cs:53`、`Command_AddLocalCharacter.cs:120/134`、`BattleContextObjectManagerModule.cs:987/1009`、`BattleObjBase.cs:27`、`Command_SwitchMech.cs:221`、`ActionCommand_AddProjectile.cs:748`、`StarsCollider.cs:443` 均为 `character.hitter` / `simpleCharacter.hitter` / `actionGameProjectile2.hitter`；
**唯一例外**：`Act_CharacterHitSwitch.cs:40` `AddAccepter(simpleCharacter)`（直接把 SimpleCharacter 当 IAccept 注册）。

### A2 · 玩家链 + **Hitter 是否实现 IAccept**

【原文】
- `GameObjectManager.cs:38` `private SimpleCharacter _player;`；`:122` `public SimpleCharacter player => _player;`
- `InteractiveGameObject.cs:16` `protected Hitter _hitter;`；`:66` `public Hitter hitter => _hitter;`
- **`Hitter.cs:13` `public class Hitter : IAttack, IAccept`** ← 明确实现了 IAccept
- `Hitter.cs:14` `protected InteractiveGameObject _interactiveGameObject;`；`:48` `public InteractiveGameObject interactiveGameObject => _interactiveGameObject;`

**结论**：**Hitter 实现 IAccept ⇒ `accepter == g_playerHitter` 的判据本身是对的**（引用类型槽里就是对象指针，直接指针比较即可）。因此「无敌=0」**不能**用「hitter 不是 IAccept」解释。

真正的三个嫌疑（按可能性排序，均需真机日志定案）：
1. **命名陷阱**：`hitter` 是**属性**，字段名是 `_hitter`；`player` 是属性，字段名是 `_player`。若 C++ 侧用 `il2cpp_class_get_field_from_name(cls, "hitter")` 取 `g_playerHitter`，**永远返回 null** ⇒ g_playerHitter=0 ⇒ 名册里 `accepter==g_playerHitter` 恒假。（`_player` 字段名是对的，但 `_hitter` 必须带下划线，且声明在 `InteractiveGameObject`，要从 Character 沿父类链找。）
2. **计数口径**（见 A4）：免疫位一生效，HitTest 在 DoAttack 之前就 `continue` ⇒ DoAttack 帧里永远数不到「玩家被打」，所以「无敌」计数结构性为 0，**与功能是否生效无关**。
3. 特殊路径：`Act_CharacterHitSwitch` 把 `SimpleCharacter` 直接注册为 accepter（不是 Hitter），这类切换场景指针比对会漏。

### A3 · ActionState 字段顺序/类型 + 怎么从玩家对象拿到它

【原文】`full/ActionGameLibrary/ActionState.cs:6` `public class ActionState : IDisposable`，字段声明顺序（逐字）：

```csharp
public bool isHitWall;               // 67
public int  hitLayer = -1;           // 69
public int  reboundCount;            // 71
public bool immuneDamage;            // 73   ← IsDamageImmune 唯一读的位
public bool immunePhysicalDamage;    // 75
public bool immuneMagicDamage;       // 77
public bool immuneDebuff;            // 79   （规格卡未列，可一并作为候选）
public int  bannedMoveCount;         // 81
public bool _bannedSkill;            // 83
public bool isSilent;                // 85
public bool invincible;              // 87   ← HitTest 的命中筛选读它
public bool paBody;                  // 89
public bool prohibitPauseFrame;      // 91
```

怎么拿（【原文】）：
- `ActionGameObject.cs:20` `protected ActionState _state;`（**字段在基类 ActionGameObject 上，不在 Character 上**）
- `ActionGameObject.cs:289` `public ActionState state => _state;`（这只是 **getter 属性**）
- 接口侧：`IActionGameObject` 有 `ActionState state { get; }`

继承链（逐字）：`Character : SimpleCharacter` → `SimpleCharacter : ActiveGameObject` → `ActiveGameObject : PassiveGameObject` → `PassiveGameObject : InteractiveGameObject` → `InteractiveGameObject : ActionGameObject, IBuffNode` → `ActionGameObject : IActionGameObject, ...`

**修正建议**：C++ 侧解析 `ActionGameObject` 上的 **`_state`** 字段（沿父类链向上找），读完 `state` 指针后按 **`ActionState`** 的字段名解析免疫位（**不要**在 Character 上找 `state`，那是属性；也**不要**用 `GameConfigData.StateData`——同名 `immunePhysicalDamage` 在 StateData 里也有一份（`StateData.cs:347`），是配置表对象，写它毫无意义）。

### A4 · 「玩家不掉血」真正生效的机制（读谁的 state）

【原文】
- `CBattleActorUtil.cs:298-305`：`IsDamageImmune(IActionGameObject pCaster, IActionGameObject pTarget)` → `if (!pTarget.state.immuneDamage) return false;` ⇒ **读被打方**。由 `Hurt` `:31` 调用，命中即 `SetValue(0)` + `SetNoDamageType(UMMUNE_DMG)` + `return 0`（`:33-37`）。
- `StarsCollider.cs:1052`（在 `HitTest()` 里，方法 826 起）：
  ```csharp
  if (!attackData.attackBehavior.ignoreInvincible && (!accept.acceptEnabled || accept.interactiveGameObject.state.invincible))
  {
      ... continue;   // :1057 —— 这一击被整条丢弃，DoAttack 不会被调用
  }
  ```
  `accept` 是 `_accepterList[m]` / `attack.targetList[m]`（`:1039/1043`），即**被打方**的 Hitter ⇒ 读的是被打方 state。
- 另一处：`DamageFactory.cs:183` `if ((actor2.state.immuneDamage || actor2.state.invincible) | flag) { bigInt = 0; ... }`（`ExecDamageCalc`，actor2=被打方）；`:201` `if (!actor2.state.invincible && bigInt > 0)` 才继续走 `ProcDmgHit`。
- `GetOriginalDamage`（`StarsCollider.cs:2208` 与 `DamageFactory.cs:56` 两份同名实现）：
  `if (acceptState.immuneDamage || (skillType == 1 && acceptState.immunePhysicalDamage) || (skillType == 2 && acceptState.immuneMagicDamage)) → damage=0, invalidDamage=true`

**结论**：免疫确实是「被打方 state + 三处独立闸」共同决定，且**全部发生在 DoAttack / Hurt 之前或之内**。⇒ 一旦免疫位真生效，`DoAttack` 帧数必然为 0。**「无敌=0」是免疫生效的表征，不是功能失效的表征**（但用户主观「还是掉血」说明另有漏网路径，见下）。

**漏网路径（会造成「免疫了还掉血」）**：
1. `EffectToCast → Character.Damage`（`CHitNotifier.cs:485` → `Character.cs:152-153`）**不带任何免疫检查**。凡是走「命中通知包」结算的伤害都不看 state；免疫只在包生成前把值清 0 才有效。若某条伤害没经过 `Hurt`（例如服务端直接推 `Proto_CSEffectNtf`、或 `Command_EffectToCast` 类路径），免疫位完全无效。
2. `attackData.attackBehavior.ignoreInvincible == true` 的技能无视 `:1052`。
3. 只置 `immunePhysicalDamage`/`immuneMagicDamage` 时，要 `skillType==1/2` 才生效；只置 `immuneDamage` 则覆盖全部技能。
4. 游戏自己会覆写：`BuffFragmentEffect_EX_immuneAllDamage/PhyDamage/MgcDamage/Invincible` 在 buff 结束时置 `false`；`InteractiveGameObjectData.cs:43` `state.immuneDamage = data.Invincible == 1;`（**每次属性重算都可能把位冲掉** ⇒ 400ms 心跳维护是必要的，但仍可能被冲）。

**修正建议**：
- 免疫位写 `immuneDamage` + `invincible` 两位必置（前者管 `Hurt`/`GetOriginalDamage`，后者管 `HitTest:1052` 与 `ExecDamageCalc:183/201`）；
- 「无敌生效」的统计口径不要挂在 `DoAttack` 上（结构性为 0）。要么改挂在 `CBattleActorUtil.Hurt` 帧里 **target==player** 时看 `m_llValue` 是否被清 0 / `IsDamageImmune` 是否命中，要么直接数「玩家血量未下降」的心跳采样；
- 若真机仍掉血，优先怀疑第 1 条漏网路径（`EffectToCast`），这属于需要用户授权的新判据，不要自行加。

---

## B · 「技能增伤」无效（`_count` 写入无效）

### B1 · `count` 是属性，背后字段是 `_count`

【原文】`full/ActionData/LAddProjectile.cs`：
```csharp
private int _count = 1;                                   // :196   ← 真正的字段（int32，默认 1）
...
[ProtoMember(24, IsRequired = false, Name = "count", DataFormat = DataFormat.TwosComplement)]
[DefaultValue(1)]
public int count                                          // :427   ← 属性
{
    get { return _count; }                                // :431
    set { _count = value; }                               // :435
}
```
⇒ 写 `_count` 等价于走 setter（`Execute` 读 `count` 属性时只读 `_count`）。**但若 C++ 侧按名字 `"count"` 取字段会拿到 null（那是属性名）**，必须解析 `_count`。

### B2 · `lAddProjectile` 从哪个字段取到

【原文】`ActionCommand_AddProjectile.cs:25`：
```csharp
LAddProjectile lAddProjectile = commandData.data as LAddProjectile;   // :25
```
`commandData` 是 `ActionCommandData`；**只有 `data`，没有 `_data`**（`ActionCommandData.cs:56` `public IExtensible data;`）。规格卡里写的「`acd.data` 或 `acd._data`」在本版**只有前者**。

### B3 · `ActionCommandData` 字段清单 + 施法者该读哪个

【原文】`full/ActionGameLibrary/ActionCommandData.cs`：
```csharp
public LTimeEventProType eventData;          // :54
public IExtensible data;                     // :56   ← LAddProjectile（接口类型，需 as 强转）
public Action action;                        // :58   ← 动作，**不含角色身份**
public IActionGameObject actionGameObject;   // :60   ← 施法者容器（Character / ActionGameProjectile），**施法者判定只能读这里**
public bool executed;                        // :62
public int frameIndex;                       // :64
public ContextData contextData = ...;        // :66   （isBuffTrigger/projectileId/...）
public string cmdKey = "";                   // :70
public object paramObject;                   // :72
```
施法者判定写法（游戏自己的用法，`ActionCommand_AddProjectile.cs:26`）：`commandData.actionGameObject is InteractiveGameObject` 后强转；可用 `(InteractiveGameObject)actionGameObject` 与 `gameObjectManager._player` 比指针，或比 `data.obj_id`。**不要用 `acd.action`**（`Action` 没有角色身份）。

### B4 · 改 `_count` 之后还有哪些闸（4 条，逐条原文）

1. **`emitByTargetCount` 覆盖 `num`**（`ActionCommand_AddProjectile.cs:65-69`）：
   ```csharp
   int num = lAddProjectile.count;                     // :65
   if (lAddProjectile.emitByTargetCount)                // :66
       num = ((interactiveGameObject.targets != null) ? interactiveGameObject.targets.Count : 0);   // :68
   else if (lAddProjectile.count > 1) num += commandData.action.projectileCount;                     // :70-73
   ```
   ⇒ 该类技能**写 `_count` 完全无效**；且 `count<=1` 时连 `projectileCount` 都不叠加（**没有 +1**）。
2. **5 处 `return null` 早退**（同文件）：`isAllReady/isUsable/isAnimationUsable` 不满足（26-30）、SURROUND/LINK/CHAIN/PARABOLA 且无目标（31-34）、`recursionTimes<=0 && prefabName==modelId`（45-49）、`character==null || state==null || data==null || !isAllReady || !isUsable`（50-53）、`flag`（战斗模式/同步判定，54-62）。任一命中 ⇒ 一次都不发射。
3. **共享配置对象**（**这是最大坑**）：`ActionCommandData.CopyTo`（`ActionCommandData.cs:120-129`）里 `data.data = this.data;` 是**引用复制**；`Fill`（131-135）把 `data = eventData.eventData`，而 `LEventData.eventData`（`LEventData.cs:10`）是**动作配置解析出来的那一份实例**。⇒ 写 `_count` 写的是**共享配置**，同一 Action 的所有 clone（同角色多次施法、甚至敌人使用同款 action 配置）都会读到放大后的值，直到被还原。
   - 推论 1：**「发射者非玩家=15」的闸只在「写的这一瞬间」有效**，写完之后的其它使用者（含敌人）不受该闸约束 ⇒ 规格卡的 `RestoreProj` 还原机制不是可选项而是必须项。
   - 推论 2：写入量级会跨施法者累积（A 写 3 倍，B 再 ×3 = 9 倍）⇒ 必须以「首次见到的原值 base」为锚（规格卡已有），并在写入前判断当前值是否已是我们写过的值。
4. **回收/复用**：`CtagHurtHPPara`/命令对象都走池化（`s_pool`、`ActionGameObjectPool.Get_CommandDataList()`），但 `CopyTo` 只复制引用 ⇒ 池化本身不重置 `_count`。

**修正建议**：① 解析字段名 `_count`（不是 `count`）；② 写入前判 `emitByTargetCount == false`（否则不计入 STATS 的「投射」成功）；③ 把「写入→还原」窗口压到最短（帧内写、orig 返回后立刻还原），因为写的是共享配置；④ 「发射者非玩家」判定改读 `actionGameObject`。

---

## C · 秒杀修复后的正确写法

### C1 · 链是否仍成立

**成立**（本版逐跳【原文】）：

```
ProcDmgHit (DamageFactory.cs:409)
  ├ Alloc(pTarget, attacker, llDamage, ...)                    :424   ⇒ m_llValue = m_llCalculateDamage = llDamage
  ├ CBattleActorUtil.Hurt(ref para,...)                        :430   （Hurt: CBattleActorUtil.cs:17）
  │    ├ GetValue() <= 0 ⇒ return -1                            :20   ← **读 m_llValue**
  │    ├ IsDamageImmune(...) ⇒ SetValue(0)                      :31
  │    └ HurtHp(...)（用 GetCalculateDamage）                    :64/68
  ├ AddOneHitHurt(targetId, CS_HURT_HP, pHurtHPPara.GetValue(), ...)   :458   ← **包值 = m_llValue**
  │    └ effectHurt.value = ToMsgInt128(llValue)                CHitNotifier.cs:313
  │    └ if (GetAttribSumLife(actor) <= value) CheckSetDieState(bDie:true)   CHitNotifier.cs:336-341  ← **致死判据**
  └ Broadcast()                                                :460 → CHitNotifier.cs:444
       ├ SendToClient()  （AFK_LOCAL_DAMAGE_EFFECT ⇒ 飘字/血条 UI）       :451/459
       └ EffectToCast()                                        :452 → :466
            └ (actor as Character).Damage(effectHurt.value, DAMAGE, knock:true)   :485
                 ⇒ healthPointNow -= num                       Character.cs:152-153   ← **真掉血**
                 ⇒ CheckDead(knock)                            Character.cs:179       ← **死亡**
```

### C2 · `BigInt` 定义（确认内联布局）

【原文】`full/BigInt.cs`：
```csharp
[Serializable]
public struct BigInt : IComparable<BigInt>, IEquatable<BigInt>, IComparable   // :7  ← struct
{
    private double m_mantissa;   // :9   ← +0x0
    private int    m_exponent;   // :11  ← +0x8
    ...
}
```
⇒ 是 **struct**，字段 `m_mantissa`(double, +0) / `m_exponent`(int32, +8)，结构体大小按 8 字节对齐补到 **0x10**。
⇒ `il2cpp_field_get_offset` 报出 `mant+0x10 / exp+0x18`，正是「值类型成员偏移被额外加了对象头 0x10」的典型结果（与根因 #1 一致）；**真实内联偏移是 mant+0 / exp+8**。

### C3 · `m_llCalculateDamage` 相对 `m_llValue` 的偏移 + 是否要同时改

【原文】`full/ActionGameLibrary/CtagHurtHPPara.cs`（class，字段声明序）：
```csharp
public IActionGameObject m_pTarget;        // :9    （引用，+0x00）
public IActionGameObject m_pAttacker;      // :11   （引用，+0x08）
public BigInt m_llValue;                   // :13   （struct 16B，+0x10）
public BigInt m_llCalculateDamage;         // :15   （struct 16B，+0x20）  ← 与 m_llValue 相隔 **0x10**
public int m_iSpellID;                     // :17
...
```
（以上为类内相对偏移；对对象指针要再加 0x10 头）

⇒ **`m_llCalculateDamage` = `m_llValue` + 0x10（16 字节）**。
⇒ 正确写入地址（【推断】，与根因 #1 完全吻合）：
| 目标 | 相对 para 指针 |
|---|---|
| `m_llValue.m_mantissa` | **+0x20** |
| `m_llValue.m_exponent` | **+0x28** |
| `m_llCalculateDamage.m_mantissa` | **+0x30** |
| `m_llCalculateDamage.m_exponent` | **+0x38** |

现在的写法（para + 0x10*(field)+0x10/0x18 叠加）落在 **+0x30 / +0x38**，正是 `m_llCalculateDamage` ⇒ 结算读 `GetValue()`（`m_llValue`）拿不到 ⇒ 秒杀静默无效。**这就是根因 #1 的代码级印证。**

**是否要同时改两者：建议两个都写（推荐），但只有 `m_llValue` 是决定性的。**
- `m_llValue`：决定 `Hurt` 的 `GetValue()<=0` 早退、命中通知包值（飘字）、`EffectToCast` 的真掉血与 `CheckDead` 死亡 —— **必须写**。
- `m_llCalculateDamage`：`Hurt` 内部护盾/血上限削减（`HurtHealthSuper` 按引用改它，`:51-58`）与 `HurtHp` 的账面伤害、`CEventDisp.OnActorBeDamage` 报给 buff 的实际伤害量 —— **写了才与「飘字=掉血」一致**；不写则吸血/受击触发类 buff 拿到的是未放大的旧值。
- 两个一起写的语义与游戏自身 `SetValue` 一致（`:95-99` `SetValue` 同时赋两值），不会破坏护盾逻辑（护盾会在 `Hurt` 里再削减 `m_llCalculateDamage`，而包值仍取 `m_llValue`）。

**另附两条修复时必须一起复核的点**：
1. `para` 指针层级：`Hurt` 第 1 参是 `ref CtagHurtHPPara`（托管指针），槽里是「指向引用变量」的指针 ⇒ 需 **1 次解引用**才拿到对象。日志 `para深度=1` 若表示「解引用 1 次」则与 ref 语义吻合；若表示「槽内直接就是对象」则与 ref 语义矛盾（但 打怪=3503 说明解出的 para 是可用的）。改完偏移后**必须**用 `m_pTarget`/`m_pAttacker` 是否指向合法 `InteractiveGameObject`（类名含 `Character`/`Projectile`）来自证，否则 +0x20 可能写到 para 自身头部之外。
2. `CharacterType` 判据仍按 `CharacterData.characterType`（PLAYER=0/APC=1/MONSTER=2/BOSS=3）读，别从 `para` 头部猜。

---

## 汇总：C++ 侧要改的点（不含代码）

| 编号 | 要改什么 | 依据 |
|---|---|---|
| 1 | BigInt 内联偏移由 `mant+0x10/exp+0x18` 改为 **`mant+0` / `exp+8`**；para 基址 + `m_llValue` 类内偏移 0x10 ⇒ 实际 **para+0x20 / para+0x28**；同时写 **para+0x30/0x38**（`m_llCalculateDamage`） | C2/C3 |
| 2 | 玩家链字段名：`GameObjectManager._player`（字段）→ `InteractiveGameObject._hitter`（**带下划线，且在基类 InteractiveGameObject 上**）；`player`/`hitter` 只是属性 | A2 |
| 3 | ActionState 取 `ActionGameObject._state`（基类字段，沿父类链找），**不要**在 Character 上找 `state`，**不要**写 `GameConfigData.StateData` 的同名字段 | A3 |
| 4 | 免疫位：`immuneDamage` + `invincible` 必置（可选再置 phy/mgc/immuneDebuff）；命中筛选在 HitTest:1052，读被打方 state | A4 |
| 5 | 「无敌」成功口径改掉（DoAttack 帧结构性为 0）：改看「玩家为 target 的 Hurt 帧值被清 0」或心跳血量 | A4 |
| 6 | `_count` 字段名（非属性名 `count`）；`ActionCommandData.data`（无 `_data`）；施法者读 `actionGameObject` | B1/B2/B3 |
| 7 | `_count` 是**共享配置**：写入必须帧内临时 + orig 返回即还原；`emitByTargetCount==true` 时不写；`count<=1` 时不存在 `projectileCount` 叠加 | B4 |
| 8 | 三条链已确认（`GetValue→AddOneHitHurt→EffectToCast→Character.Damage`），修好偏移后 #1 应立刻可见；飘字与掉血同源 | C1 |

---

## 附 · 复现

```bash
export DOTNET_ROOT=/Users/aliang/.dotnet
ILSPY=/Users/aliang/.dotnet/tools/.store/ilspycmd/11.1.0.9782/ilspycmd/11.1.0.9782/tools/net10.0/any/ilspycmd.dll
/Users/aliang/.dotnet/dotnet $ILSPY -p -o 13_scratch/verify_bili201/full "06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll"
# 单类：-t ActionGameLibrary.CtagHurtHPPara / ActionGameLibrary.Hitter / ActionGameLibrary.ActionState ...
```
（`--il` 模式在本工具版本对任何类型均 `Abort trap: 6`，IL 偏移仍需运行时复核。）
