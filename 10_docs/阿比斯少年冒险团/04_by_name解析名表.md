# 阿比斯少年冒险团 2.36 — by-name 解析名表（T7）

> 目的：让 mod 在**进程内**用**字符串名字**解析类/字段（guest→guest 走 il2cpp 导出 API），数字偏移只作交叉校验。
> 来源：`06_dumped/阿比斯少年冒险团/il2cpp_out/dump.cs`（Assembly-CSharp = Image 0）。所有行号可直接 `sed -n 'Np'` 复查。
> 分级：【实测】= dump.cs 原文；【推断】= 由类型/命名推出；【静态不可得】= 名字被混淆或不存在，给运行时探测配方。

## 0. 三条通用事实（先看，能省一半事）

1. **类型名被混淆，但枚举成员名、字段名大量可读** —— 所以「按名字取字段」这条路是通的，不需要数字偏移。
2. **命名空间绝大多数是空串**（Assembly-CSharp 里 4710/4777 个类型在全局命名空间），所以 `il2cpp_class_from_name(image, "", "Unit")` 是常态写法。
3. il2cpp 的 by-name API 不检查可见性：`private static` 字段照样能 `il2cpp_class_get_field_from_name` 取到。

## 1. Unit 类

| 项 | 值 |
|---|---|
| 命名空间 | **空串**（dump.cs L166574 为 `// Namespace:`，后面无内容） |
| 类名 | **`Unit`** |
| 声明原文（L166575） | `public class Unit : MonoBehaviour, BFLILACEGIG // TypeDefIndex: 2631` |
| assembly | **Assembly-CSharp.dll**（dump 头 `// Image 0: Assembly-CSharp.dll - 0`） |
| TypeDefIndex | 2631 |
| 基类/接口 | MonoBehaviour / BFLILACEGIG（自定义接口，名混淆） |

**解析写法**：`image = il2cpp_assembly_get_image(il2cpp_domain_assembly_open(domain, "Assembly-CSharp")); klass = il2cpp_class_from_name(image, "", "Unit");`

## 2. Unit 关键字段（字段名即解析串）

| 用途 | 类名(命名空间) | 字段名 | 声明类型 | 偏移 | dump.cs 行 | 置信度 |
|---|---|---|---|---|---|---|
| **状态对象**（HP/属性 getter 经此转发） | Unit("") | `LAGPMHKOAKN` | OIGNPAHJLCC | +0x120 | L166643 | 实测（语义【推断】） |
| **CharacterController**（位移执行体） | Unit("") | `m_CharacterController` | CharacterController | +0xA0 | L166614 | 实测 |
| **阵营/单位类型枚举** | Unit("") | `GPJLPFOKIDE` | LGONBKNFNAC | +0x150 | L166650 | 实测（枚举成员名可读，见 §4） |
| **血量候选 1** | Unit("") | `DEPKEGHAHMB` | long | +0x190 | L166662 | 实测字段；语义【推断】= 当前血量 |
| **血量候选 2** | Unit("") | `LAMEKJABEAP` | long | +0x1A0 | L166664 | 实测字段；语义【推断】= 血量上限 |
| 计数/等级候选 | Unit("") | `APPACLOOHNK` | int | +0x198 | L166663 | 实测字段；语义【待验证】 |
| bool 状态位 | Unit("") | `PEABCFJEPBB` | bool | +0x188 | L166661 | 实测字段；语义【待验证】 |
| **目标类别掩码**（可被攻击/治疗…） | Unit("") | `KDOFIAPPOGI` | OEDKMCLNPBD | +0x20 | L166587 | 实测（枚举成员可读，见 §4.2） |
| 模型根 Transform | Unit("") | `NOLONIDAOMB` | Transform | +0x140 | L166647 | 实测字段；语义【推断】 |
| 动画器 | Unit("") | `JJNHGELNBDA` | Animator | +0x48 | L166611 | 实测 |
| 模型表现对象 | Unit("") | `LALGGHDJKAJ` | UnitModelBase | +0x180 | L166659 | 实测 |

> **重要**：Unit 上**没有** `m_CurHp / m_Hp / MaxHp` 之类可读名。血量就是 `DEPKEGHAHMB(+0x190, long)` 与 `LAMEKJABEAP(+0x1A0, long)` 两个混淆名 long 字段。
> 旁证（支持"当前/上限"顺序）：`UnitHpBar` 的 `EGHHHEFBLKJ(float, float)` / `JNMAOBCHNDA(float, float)` 两个"血条比例"方法都吃 `(当前, 上限)` 两个 float；且 `DebugUnitCurrentStatusPanel` 有独立的 `m_UnitCurrentHpText(0x28)` 与 `m_UnitMaxHpText(0x30)` 两个文本（见 §7）。

## 3. Unit 静态表（**这两个都是 Unit 的静态字段，不是类名**）

| 用途 | 类名(命名空间) | 字段名 | 声明类型 | 偏移 | dump.cs 行 | 置信度 |
|---|---|---|---|---|---|---|
| **主单位表**（int → Unit） | Unit("") | `LOJFLKLPODK` | `Dictionary<int, Unit>` | +0x0（静态区） | L166584 | 实测 |
| **按阵营分组的单位表** | Unit("") | `FOIAAHJBGNG` | `LFIOLDEBONF<LGONBKNFNAC, int, Unit>` | +0x8（静态区） | L166585 | 实测 |
| 静态计数器 | Unit("") | `KEIJNMIPCPG` | int | +0x10 | L166586 | 实测 |
| 静态只读 int | Unit("") | `JKEAMNHMIIL` | int (readonly) | +0x14 | L166588 | 实测 |

- `LFIOLDEBONF` 是**游戏自定义多键字典容器**（不是 BCL 类型），按名字拿不到标准布局，**建议只用 LOJFLKLPODK**。
- Dictionary 泛型实例的 il2cpp 类型名：【推断】`System.Collections.Generic.Dictionary`2<System.Int32, Unit>`；**builder 不需要解析泛型名** —— 拿到字段值就是 object 指针，按 .NET Dictionary 布局迭代 `_buckets / _entries / _count` 即可（字段名 `_count`/`_entries` 也可以直接 by-name 取，见 §8）。

## 4. 阵营/类别枚举（**成员名可读，直接给数值**）

### 4.1 LGONBKNFNAC（Unit.GPJLPFOKIDE 的类型）L185236

~~~csharp
public enum LGONBKNFNAC // TypeDefIndex: 2879
{
	public int value__; // 0x0
	public const LGONBKNFNAC NONE = 0;      // L185240
	public const LGONBKNFNAC PLAYER = 1;    // L185241
	public const LGONBKNFNAC ENEMY = 2;     // L185242
}
~~~

**⇒ 玩家 = 1，敌人 = 2**（实测）。判别我方：`(*(int*)(unit + 0x150)) == 1`，或 by-name `GPJLPFOKIDE`。

### 4.2 OEDKMCLNPBD（Unit.KDOFIAPPOGI 的类型）L185935 —— **位掩码，不是小枚举**

~~~csharp
public enum OEDKMCLNPBD // TypeDefIndex: 2913
{
	public const OEDKMCLNPBD None = 0;
	public const OEDKMCLNPBD PlayerAttackable = 1;
	public const OEDKMCLNPBD PlayerHealable = 2;
	public const OEDKMCLNPBD PlayerBuffable = 4;
	public const OEDKMCLNPBD PlayerBreakable = 8;
	public const OEDKMCLNPBD PlayerSingleBuffRemovable = 16;
	public const OEDKMCLNPBD PlayerShieldable = 32;
	public const OEDKMCLNPBD PlayerComboable = 128;
	public const OEDKMCLNPBD EnemyAttackable = 65536;      // L185947
	public const OEDKMCLNPBD EnemyHealable = 131072;
	public const OEDKMCLNPBD EnemyBuffable = 262144;
	public const OEDKMCLNPBD EnemyBreakable = 524288;
	public const OEDKMCLNPBD EnemySingleBuffRemovable = 1048576;
	...
}
~~~

用途：**目标类别掩码**（低 16 位 = Player 侧，高 16 位 = Enemy 侧），可作敌我判别的第二判据。
同一枚举也用于 `UnitBaseInfo.m_OverrideSkillEffectableType(+0x50, L154183)`（配置可覆盖）。

### 4.3 CNNLNDBHMMB（伤害类型）L185484
`DAMAGE_NONE=0 / SLASH=1 / BLUNT=2 / PIERCE=3 / WEAPON=4 …`；出现在 Unit 事件 `Action<Unit, CNNLNDBHMMB> AAFEAHAPOGC(+0x1D8)` 与伤害载荷 `EAGEDGNBJHM(+0x20)`。

### 4.4 KCIMFCENEIN（元素）L185496
`ELEMENT_NONE=0 / SPECTRAL=1 / FIRE=2 / METAL=3 / LIGHTING=4 / WATER=5 / WEAPON=6 / ELEMENT_COUNT=7`。

## 5. 伤害 / 攻击力（**静态不可得：名字全混淆**，给可执行替代）

实测：**Unit 上没有任何可读的 Attack/Defense/Crit 字段**（全 region 扫描 `^\s+(public|private|protected)\s+(float|int|long)\s+(m_)?(Attack|Defense|Crit|...);` 只命中 UI 与计时器，无一在 Unit）。数值属性在下面这层：

| 类 | 事实 | 行号 | 置信度 |
|---|---|---|---|
| `OKNDCLIKECF : OABFHDPMGOD` | 属性容器：6 个自动属性，后备字段全是 **`MJJMGGGHDGE`（接口）** @ +0x18/+0x20/+0x28/+0x30/+0x38/+0x40；另有 `float FACAGOKDJMO(+0x48)`、`float <CKPNNMFMMKK>k__BackingField(+0x4C)`、`KEMEJCCDJKJ CGHEBBBLGLH(+0x50)` | L183444+ | 实测结构 |
| `MJJMGGGHDGE` | **interface**（TypeDefIndex 2862，L183159），实现类 `ACKAKNLFLDI`；有 `JKCCBMHOECH(MJJMGGGHDGE)` 与 `GNMIGMFJBMO()` | L183159 / L183225 | 实测 |
| 6 个属性名（by-name 可用） | `DFILAFMKDNP / NGMOALBLDHC / IEIPHJFANFG / JJIJAMMKJMI / NNONHODFHCE / HMGAJNNFILF / JPPGMNCBCGL` | L183444+ | 实测（语义未知） |

**推荐做法**：不要静态猜槽位。用 §7 的 Debug 面板做运行时自证（面板有独立的 `m_UnitAttackText(+0x38)` / `m_UnitDefenseText(+0x40)`）。
**在钉死之前，放大我方输出用更稳的兜底**：Unit 伤害事件 `MKOFCNHMHBI(+0x1B8, Action<Unit,Unit,EAGEDGNBJHM,long>)` 的第 4 参（long）与载荷 `EAGEDGNBJHM(+0x28 long / +0x40 float)` —— 在 raise 前按倍率改写（纯数据面，不依赖属性槽语义）。

## 6. 技能运行时对象（SkillObj 家族）

| 类 | 声明 | 行号 |
|---|---|---|
| `SkillObjShareBase : EventObjBase`（abstract） | 技能基类 | L181844 |
| `SkillObjBase : SkillObjShareBase` | 主技能实体 | **L150759** |
| `SkillObjAdvFxBehaviour : SkillObjBase` | 特效型技能 | L151490 |
| `SkillObjActionAttach : MonoBehaviour` | 动作挂点 | L151381 |
| `SkillObjTraceBone : MonoBehaviour` | 骨骼追踪 | L151892 |
| `EventObjBase : MonoBehaviour, BFLILACEGIG`（abstract） | 事件对象基类：`Unit KDJBJMOBOJO(+0x30)` / `Unit NEOIFHOPNFA(+0x38)`（施法者/目标候选）、`Dictionary<int,int> HPFPMPHEAPA(+0x78)`、`int FALIBAJLJLC(+0x64) / DBCEPFEOKIK(+0x68) / KDCMPBPLPIH(+0x6C) / FBCMHPJIIJC(+0x70)` | L178620 |

SkillObjBase 可读字段（实测，L150759 起）：

| 字段 | 类型 | 偏移 | 行号 | 语义 |
|---|---|---|---|---|
| `m_ChaseType` | PMFPPIIABBM | +0xCC | L150763 | 追踪类型 |
| `m_MovementSpeed` | float | +0xD4 | L150767 | 位移速度 |
| `m_MovementTime` | float | +0xD8 | L150769 | 位移时长 |
| `m_MovementCurve` | AnimationCurve | +0xE0 | L150771 | 位移曲线 |
| `m_MaxScaling` / `m_ScalingCurve` | float / AnimationCurve | +0xE8 / +0xF0 | L150773/5 | 缩放 |
| `m_TotalLifeTime` | float | +0xF8 | L150777 | **技能总存活时间**（最接近"持续时间"的量） |
| `m_MaxCirculatingTime` | int | +0x108 | L150781 | 循环次数上限 |
| `m_CirculatingSkillMaxHit` | int | +0x110 | L150787 | 循环段最大命中 |
| `m_IsPureVFX` | bool | +0x140 | L150806 | 纯特效 |

**冷却字段结论：静态不可得。** SkillObj 家族里没有任何名字像 CD/Cooldown/NextCast/Remain 的字段；全 region 带 cool 字样的只有 UI：`ActionButton.m_CoolDownLayer(+0x90) / m_CoolDownText(+0x98) / m_StackCoolDownLayer(+0xA8)`、`SkillDetailPanel.m_CoolDownText(+0x60) / m_CoolDownGroup(+0x70)`。

### 6.1 运行时探测配方（**不依赖 frida，由我们的 so 打日志**）

~~~text
[配方 A · 找 CD 字段]（推荐）
1. hook SkillObjBase 的 .ctor / SkillObjShareBase 的生成入口：每次技能实体创建时，把 this 的 0x10..0x150 全打一遍（4/8 字节 + 时间戳）。
2. 连放同一技能 3 次。日志里凡「同一偏移在两次施放之间 递减→跳回满值」的，就是 CD。
3. 若 SkillObjBase 上无，转配方 B。

[配方 B · 从 UI 反查]
1. hook ActionButton.m_CoolDownText(+0x98) 的 set_text，打印调用栈与每次写入的字符串。
2. 从栈上找到"算剩余时间"的函数，再看它读哪个字段。
3. 只读日志，不改值。

[配方 C · 从技能配置（最省事兜底）]
by-name 取 UnitSkillInfo 的 m_NormalAttackSpeed(+0x1C)、m_PreAnimateIdleDuration(+0x54)、m_AfterSkillIdleDuration(+0x58)；
若 CD 由攻速推导，直接放大 m_NormalAttackSpeed 即等价"无限技能"，无需找 CD 字段。
~~~

## 7. DebugUnitCurrentStatusPanel（**反查移速/血量的入口**）

| 项 | 值 |
|---|---|
| 类 | `DebugUnitCurrentStatusPanel : MonoBehaviour`（namespace 空） |
| 声明行 | **L170726** |
| 字段 | `m_UnitName(TextMeshProUGUI, +0x20)`、`m_UnitCurrentHpText(+0x28)`、`m_UnitMaxHpText(+0x30)`、`m_UnitAttackText(+0x38)`、`m_UnitDefenseText(+0x40)`、`m_UnitMoveSpeedText(+0x48)`、`m_DamageOverviewTabGroup(+0x50)`、`m_DebugModifierParent(+0x58) / m_DebugModifierComponent(+0x60)`、`m_DebugBuffParent(+0x68) / m_DebugBuffComponent(+0x70)` |

**价值**：官方把「当前血量 / 上限 / 攻击 / 防御 / 移速」**分别**展示，等于给出语义锚点。
**刷新方法名被混淆** ⇒ getter 名静态不可得；用配方 D：

~~~text
[配方 D · 用调试面板钉死血量与移速]
1. hook TextMeshProUGUI.set_text（或该类中几个 Text 字段的写入点），打印 (this, 文本)。
2. this == m_UnitCurrentHpText 时的数值 = 当前血量；同一次刷新里 m_UnitMaxHpText = 上限。
3. 在同一刷新调用栈回读 Unit 的 +0x190 / +0x1A0，与两文本对齐 ⇒ 直接判定谁是谁（不猜）。
4. 移速同理：m_UnitMoveSpeedText 的数值对应哪个 Unit float 字段，就在同帧比对。
~~~

## 8. builder 落地清单（按顺序照做）

1. `image = assembly_get_image(domain_assembly_open("Assembly-CSharp"))`
2. `unitKlass = class_from_name(image, "", "Unit")`
3. 字段 by-name：`LAGPMHKOAKN / m_CharacterController / GPJLPFOKIDE / DEPKEGHAHMB / LAMEKJABEAP / APPACLOOHNK / PEABCFJEPBB / KDOFIAPPOGI / NOLONIDAOMB`
4. 实例枚举：`class_get_field_from_name(unitKlass, "LOJFLKLPODK")` → 取静态字段值（Dictionary 对象）→ 迭代 `_entries`（Key=int, Value=Unit*）
5. 我方判别：`GPJLPFOKIDE == 1 (PLAYER)`；交叉判据 `(KDOFIAPPOGI & 0xFFFF) != 0`
6. 血量：`DEPKEGHAHMB / LAMEKJABEAP`（先用配方 D 验一次）
7. 输出放大：优先改伤害事件载荷（`MKOFCNHMHBI` 的 long 参 / `EAGEDGNBJHM` 的 +0x28 或 +0x40）；属性槽待配方 D 钉死后再用
8. 技能：先试配方 C（放大 `UnitSkillInfo.m_NormalAttackSpeed`）；CD 字段按配方 A/B 找
9. 移速：`m_CharacterController(+0xA0)` 做位移乘算，或按配方 D 定位真移速字段

## 9. 静态不可得清单（诚实）

| # | 项 | 原因 | 替代 |
|---|---|---|---|
| N1 | 血量字段的「当前/上限」语义 | 名字混淆 | 配方 D（一次进战斗钉死） |
| N2 | 攻击/防御/暴击字段名与槽位 | 名字混淆；属性在 OKNDCLIKECF 的 6 个 MJJMGGGHDGE 接口槽 | 配方 D + 伤害事件兜底 |
| N3 | 技能 CD 字段 | 全 region 无 CD 类命名 | 配方 A / B / C |
| N4 | 玩家移速字段 | 名字混淆 | 配方 D（用 m_UnitMoveSpeedText 对齐） |
| N5 | 死亡/无敌标志位 | 名字混淆 | 用 DEPKEGHAHMB <= 0 自判，或配方 D 观察 |

## 10. 可调用锚点（T8，静态，RVA 口径见 §10.0）

> **RVA 口径**：本节所有 RVA = **dump.cs 里的 `// RVA:` 数值 = ELF vaddr**；它与文件偏移的关系是 `RVA = Offset + 0x4000`（dump.cs 同时给了两个值，可直接核对）。进程内实现时用 `il2cpp_module_get_base + RVA` 或直接 `class_get_method_from_name → methodPointer` 取地址，**不要自己做文件偏移换算**。

### 10.1 技能释放入口

| 查询 | 结果 |
|---|---|
| 返回类型为 SkillObjBase / SkillObj* 的方法 | **静态不可得**：全 dump 无任何方法返回 SkillObj* 类型（技能实体是内部 new 出来并塞进事件对象的） |
| 名字像释放且参数含 int 的方法 | 未见游戏侧命名（Release/Emit 全是 BCL/UI）；技能实体创建走 `SkillObjBase.ctor` |
| **可用替代（推荐）** | `SkillObjShareBase`（L181844）是 `EventObjBase` 子类，而 EventObjBase 持有 `Unit KDJBJMOBOJO(+0x30) / Unit NEOIFHOPNFA(+0x38)`（施法者/目标）。⇒ 要"再放一次技能"**不要调释放函数**，改为 runtime_invoke 目标 Unit 的加扣血方法（10.4）或直接写字段——数据面做法，绕开技能系统 |

### 10.2 UnitSkillInfo（配置对象）

| 项 | 值 |
|---|---|
| 类 | `UnitSkillInfo : IScriptableStaticData`（namespace 空） |
| 声明行 | **L154239** |
| 关键字段 | `m_NormalAttackSpeed : float @+0x1C`（L154242，**float 实测**）、`m_SkillConditionType : OJGNGDOLKLH @+0x20`、`m_AnimationIndex : int @+0x50`、`m_PreAnimateIdleDuration : float @+0x54`、`m_AfterSkillIdleDuration : float @+0x58`、`m_SkillMaxHit : int @+0x5C`、`m_ModifierListInAction : SkillInfoModifierInfo[] @+0x60`、`m_SkillEvent : SkillEvent[] @+0x68`、`m_MaxMoveDistance : float @+0x70`、`m_MaxDetectionDistance : float @+0x74` |
| setter 可写性 | `public float m_NormalAttackSpeed` 是**普通 public 字段**（非 property、非 readonly）⇒ 直接写内存即可；但它是 IScriptableStaticData 配置资产，**同类单位共享** ⇒ 改它=该类型全体一起变，不是单个体 |

**谁持有 UnitSkillInfo（实测）**：

| 持有者 | 字段 | 类型 | 偏移 | 行号 |
|---|---|---|---|---|
| `SkillObjShareBase` | `OPKDKCPGDAD` | UnitSkillInfo | +0x18 | L181912（另有属性 CEJCAALCNKD） |
| `KCPGOACDHOB : KPBKFHOKGHJ` | `BJHHKKKBJFJ` | UnitSkillInfo | +0x50 | L159538 |
| `NDJLODOGKNK` | `MGPKHKPDNGM` | UnitSkillInfo | +0x150 | L178288 |
| `EnemyUnitInfo : UnitBaseInfo` | `m_SkillAttackInfo` | UnitSkillInfo[] | +0x60 | L152292 |
| `FFIGCMIJNEF : OIGNPAHJLCC`（Unit+0x120 的实现类） | `MOFHIMEIFKP(int,int) / PNHJAEEEMOE(int,int) / GLAOOFKPGNF(int,int)` 均返回 UnitSkillInfo | 方法 | — | L162280 / L162322 / L162358 |

> **从 Unit 拿配置的最短路径**：`unit+0x120 → FFIGCMIJNEF → 调 MOFHIMEIFKP(skillId, level)`（签名 `(int,int) → UnitSkillInfo`）。纯读，安全。

### 10.3 移速执行链（实测反汇编）

| 方法 | 签名 | RVA | 行号 | 证据 |
|---|---|---|---|---|
| `Unit::EJGLGAJLLOE` | `(Vector2 dir, float len, bool)` | **0x35508C8**（逻辑段 0x3550938） | L167003 | 0x3550938 `ldrb w8,[x19,#0x158]` → `fcmp s10,#0 / fcmp s9,#0`（方向为 0 跳过）→ **0x3550954 `ldr x0,[x19,#0xa0]`（取 m_CharacterController）→ `bl 0x7B608AC`**（【推断】= CharacterController 位移）→ 之后取 `[x19,#0x60]` 做锚点位移 |
| `Unit::DEADDLCOGBK` | `(Vector2, float)` | 0x35515F4 | L167085 | 同族重载（另一分支） |
| `Unit::FNADKPEFIKA` | `(Vector3)` | 0x354A630 | L166786 | 吃 Vector3 的位移/置位候选 |
| `Unit::NAIHAANJGAM` / `OGKLLMPPFGE` | `(Vector2, Vector2)` | 0x354DB48 / 0x354E574 | L166919 / L166953 | (方向, 朝向) 候选 |
| `Unit::NIEALGDILPA` | `(Vector3)` | 0x3552180 | L167133 | 位移候选 |

**结论**：移速乘算点在 `EJGLGAJLLOE(Vector2, float, bool)` 的第 3 个参数（这一帧要移动的长度）。做"移速倍率"最干净是在调用它的上层把 len 乘倍数，或直接操作 `m_CharacterController(+0xA0)` 按帧补偿。
**注意**：`[x19+0x158]`（`GDCNAICDONM : bool`）是该方法前置开关，实测为 0 时先判方向再决定是否移动。

### 10.4 加血/扣血（**本次最大收获**）

| 方法 | 签名 | RVA | 行号 | 语义（实测反汇编） |
|---|---|---|---|---|
| `Unit::JJDKEBAMLNG` | `(Unit src, long delta)` | **0x354E28C**（逻辑 0x354E304） | L166928 | ① raise 血量事件 `JAODIJCPHFL(+0x1B0)`（传 src, this, delta）；② `ldr w8,[x20,#0x150] ; cmp w8,#1`（**只对阵营=PLAYER 生效**）；③ `ldr x8,[x20,#0x190] ; add x8,x8,x19 ; str x8,[x20,#0x190]` ⇒ **curHp += delta 写回 0x190** |
| `Unit::JMIKMDBLCHB` | `(Unit src, long delta)` | **0x354B944**（逻辑 0x354B9BC） | L166967 | 与上者**逐条等价**（同 raise、同 PLAYER 门、同 add 写 0x190）；IFix id 不同，是同一逻辑的两个副本，**用哪个都行** |
| `Unit::FAGNIOBDNJA` | `(long maxHp)` | **0x354A7AC** | L166792 | `cmp x20,#1 ; b.lt → mov w8,#1 ; str x8,[x19,#0x1a0]` ⇒ **set_MaxHp，钳位 >=1，写 +0x1A0** |

**⇒ 语义钉死（实测，不再是推断）**：`+0x190 = 当前血量（long）`、`+0x1A0 = 血量上限（long，钳位>=1）`；加减血入口 = `(Unit src, long delta)`，delta 负数即扣血。

**⚠️ 对「对敌方补扣」的影响（务必转告 builder）**：两个加减血方法内部有 `cmp w8,#1`（PLAYER）**硬门槛**，对 ENEMY 直接跳过、不写血量。所以：
- 给**怪**补扣 (倍率-1)×Δ **不能靠 runtime_invoke 这两个方法**；
- 可行方案：**直接写敌方 Unit 的 +0x190**（数据面：每次伤害事件后读敌方 curHp，减去 (倍率-1)×Δ）——就是"农场守护者百倍伤害"的补扣思路，只是把"调扣血函数"换成"直接写字段"；
- 敌方血量的其它写入路径**静态未定位**（Unit 内写 +0x190 的只有上述两个 + 两个无参方法 `HNNPDHDBACG(0x35512F0)` / `JBMGPALFFGB(0x3552698)`，无参更像重算/钳位）。

### 10.5 其它可直接调用的 Unit 方法

| 方法 | 签名 | RVA | 行号 | 说明 |
|---|---|---|---|---|
| `LKOKJLMCFIK` | `()` | 0x35499EC | L166738 | 读 +0x190 ⇒ get_CurHp |
| `FKIBMJHJACH` | `()` | 0x3551098 | L167049 | 读 +0x190（getter 副本） |
| `HNNPDHDBACG` | `()` | 0x35512F0 | L167064 | 写 +0x190（无参，钳位/重算候选） |
| `JBMGPALFFGB` | `()` | 0x3552698 | L167152 | 写 +0x190（同上） |
| `KKGNKHFDCBG` | `()` | 0x354A5D4 | L166783 | 经 +0x120 状态对象取属性 |

## 11. 落地建议（T8 结论）

1. **血量**：读用 `LKOKJLMCFIK()` 或 `*(long*)(unit+0x190)`；写我方用 `JJDKEBAMLNG(src, ±delta)`（runtime_invoke，阵营门槛已满足）；写敌方只能直接写 +0x190。
2. **上限**：`FAGNIOBDNJA(long)`（钳位 >=1），可做"血量上限倍率"。
3. **伤害倍率**：敌方 = 每次伤害事件后把 `*(long*)(enemy+0x190)` 减去 `(倍率-1)*Δ`；Δ 可从 `MKOFCNHMHBI(+0x1B8)` 的 long 参或 curHp 前后差值取。
4. **移速**：改 `EJGLGAJLLOE(Vector2,float,bool)` 的 len 参数（RVA 0x35508C8），或操作 `m_CharacterController(+0xA0)`。
5. **技能**：不碰技能系统；配置查询走 `MOFHIMEIFKP(int,int)`，攻速写 `UnitSkillInfo.m_NormalAttackSpeed(+0x1C,float)`（注意全家共享）。

### 静态不可得（T8 补充）

| # | 项 | 原因 |
|---|---|---|
| N6 | 返回 SkillObj* 的释放方法 | 全 dump 无此签名 |
| N7 | **敌方**扣血函数 | Unit 内无 ENEMY 分支的血量写方法（两个 (Unit,long) 都硬门槛 PLAYER） |
| N8 | 技能 CD 字段 | 见 §9 N3 |

## 12. 属性容器 OKNDCLIKECF 获取链 + 6 槽语义（T9）

> RVA 口径同 §10：RVA = dump.cs 的 RVA = ELF vaddr（= 文件偏移 + 0x4000）。
> 探针打出 oknd=0x0 的原因：**OKNDCLIKECF 不是 Unit 的字段**，必须经 IAPIHBKPLCF.IFEHBJOMMMB() 这一跳去取（返回方法，不是字段）。

### 12.1 继承关系（先看清，避免按错 klass 取名）

| 类 | 声明 | 行号 |
|---|---|---|
| IAPIHBKPLCF（interface） | 声明 LKGHDOLKKGL{get;set} / KOKIAFLHDKK{get} / NJMJNBPJDIE() / HEDDFPGMEFH(FPIALCOELGG) / IFEHBJOMMMB() → FPIALCOELGG | **L176648**（成员 L176662/L176663/L176740/L176743/L176746） |
| PHBGLMHPMMC（abstract，实现该接口） | public abstract class PHBGLMHPMMC // TypeDefIndex 2857 | **L182068** |
| OIGNPAHJLCC（abstract） | public abstract class OIGNPAHJLCC : PHBGLMHPMMC, IAPIHBKPLCF ← **Unit+0x120 的声明类型** | L167177 |
| FFIGCMIJNEF | public class FFIGCMIJNEF : OIGNPAHJLCC, AIEIJENONKA, IAPIHBKPLCF（具体实现，另有 MOFHIMEIFKP(int,int)→UnitSkillInfo） | L162119 |
| OKNDCLIKECF（属性容器） | public class OKNDCLIKECF : OABFHDPMGOD | **L183444** |
| FPIALCOELGG（属性容器实际子类，即 IFEHBJOMMMB 返回类型） | public class FPIALCOELGG : OKNDCLIKECF | **L183717** |

### 12.2 逐跳取值链（可 by-name 实现）

| 步 | 做什么 | 名字 / 签名 | 证据 |
|---|---|---|---|
| 1 | 取 Unit 的状态对象 | Unit 字段 LAGPMHKOAKN : OIGNPAHJLCC @ **+0x120** | L166643（实测字段） |
| 2 | 在该对象上取属性容器 | IAPIHBKPLCF::IFEHBJOMMMB() → FPIALCOELGG（= OKNDCLIKECF 子类） | 接口 L176746；实现 PHBGLMHPMMC::IFEHBJOMMMB() **RVA 0x36A0A20**（L182205） |
| 3 | 结果即属性容器 | FPIALCOELGG : OKNDCLIKECF，6 槽在 +0x18…+0x40 | L183717 / L183444 |

**实锤（反汇编）**：Unit::KKGNKHFDCBG()（RVA 0x354A5D4）逻辑段：

~~~asm
0x354A60C  ldr  x0, [x19, #0x120]     ; this.LAGPMHKOAKN（状态对象）
0x354A610  cbz  x0, ...
0x354A618  bl   0x36A0A20             ; PHBGLMHPMMC::IFEHBJOMMMB()  ← 返回属性容器
0x354A61C  cbz  x0, ...
0x354A628  b    0x36A0C28             ; OKNDCLIKECF::HKLDOFIAKDD()  ← 再取具体值
~~~

**by-name 写法（给 builder）**：

~~~c
void* state = *(void**)((char*)unit + 0x120);      // LAGPMHKOAKN
Il2CppClass* k = *(Il2CppClass**)state;            // 运行时类（FFIGCMIJNEF / PHBGLMHPMMC 子类）
MethodInfo* m = il2cpp_class_get_method_from_name(k, "IFEHBJOMMMB", 0);
void* oknd = il2cpp_runtime_invoke(m, state, NULL, NULL);   // 返回 FPIALCOELGG（引用类型，直接可用）
~~~

> 若运行时类上取不到 IFEHBJOMMMB，退一步：il2cpp_class_get_method_from_name(il2cpp_class_from_name(image,"","PHBGLMHPMMC"), "IFEHBJOMMMB", 0) 再对 state 调用。

### 12.3 OKNDCLIKECF 的 6 个属性槽（+0x18…+0x40）

每个槽是一组**三件套**：1 个 MJJMGGGHDGE 后备字段（自动属性）+ 1 个 MJJMGGGHDGE 只读属性 + 1 个 **long 只读属性（最终数值）**。

| 槽位 | 后备字段 | 对外 get 属性 | **最终值属性（long）** | 置信度 |
|---|---|---|---|---|
| **+0x18** | <NGMOALBLDHC>k__BackingField | FDMGBDKBKKJ { get; } | PDLFCFHOCME { get; } | 实测结构 |
| **+0x20** | <IEIPHJFANFG>k__BackingField | BOMHMHJJBFK { get; } | LNIJOMDHEBJ { get; } | 实测结构 |
| **+0x28** | <JJIJAMMKJMI>k__BackingField | EEJFKJOEDGF { get; } | EHIOBPHENPA { get; } | 实测结构 |
| **+0x30** | <NNONHODFHCE>k__BackingField | JOKPNBMKINO { get; } | NIKPNLBGKFA { get; } | 实测结构 |
| **+0x38** | <HMGAJNNFILF>k__BackingField | PECMHNKCJCH { get; } | OLBJHLHMLCE { get; } | 实测结构 |
| **+0x40** | <JPPGMNCBCGL>k__BackingField | OGMLKGJJJLJ { get; } | AONEHGPCGKO { get; } | 实测结构 |

另有：int <DFILAFMKDNP>k__BackingField @+0x10（属性 CIDJOIPCPAG）、float FACAGOKDJMO @+0x48（属性 CBDEGMMHPCM）、float <CKPNNMFMMKK>k__BackingField @+0x4C（属性 BIFADHOKBGF）、KEMEJCCDJKJ CGHEBBBLGLH @+0x50（容器自身的属性类型描述符）。

**槽位 getter 的 RVA**：MPDLABDOKLO() 0x36ABD80（L183497）、PMKAGMHPAHB() 0x36A2928（L183500）、IKKIBLMKGEM() 0x36ABE6C（L183503）等共 6 个 long getter；写值入口 GPHAMODMGHC(float) 0x36ACA2C、ONCOLFNNMBB(MJJMGGGHDGE) 0x36ACA34、HCFPHOCMANE(MJJMGGGHDGE) 0x36ACAEC。

**槽位语义（攻击/防御/移速/攻速/暴击）：静态不可得。** 6 个槽的属性名全是随机串；枚举侧只有接口 KEMEJCCDJKJ（L183878，成员 JCIDABPGGEL / IOKMIEIBLHB / KHPIJPHCFGP(int) / KHPIJPHCFGP(KMLFBAENBMH) / JKFAKOGBDML()），**没有任何 ATK/DEF/SPEED 可读名**。

**so 侧自证配方（不打 frida，只打日志）**：

~~~text
[配方 E · 一次进战斗钉死 6 个槽]
1. 用 12.2 的链拿到 oknd；同时取 Unit+0x190（curHp，已知）做对照。
2. 每 500ms 打印：6 个 long getter 的返回值 + 两个 float（+0x48/+0x4C）+ Unit+0x190/+0x1A0。
3. 让角色移动 → 看哪个槽随移速变（换加速道具再比一次）；受击 → 看哪个槽量级像防御；看「某槽 × 系数 ≈ 实际伤害」→ 那个是攻击。
4. 同时打印 DebugUnitCurrentStatusPanel 的 m_UnitAttackText / m_UnitDefenseText / m_UnitMoveSpeedText 文本（§7 配方 D），与 6 槽数值交叉对照 ⇒ 语义直接对齐，不需要猜。
~~~

### 12.4 移动执行链（移速落地关键）

| 环节 | 类/方法 | 地址 | 证据 |
|---|---|---|---|
| 驱动器实例 | Unit 字段 GAMJINGNDHM : KPBKFHOKGHJ @ **+0x38**（另一份 HGMLJJAMBFG @+0x40 同类型） | L166608 / L166612 | 实测字段 |
| 驱动器基类 | KPBKFHOKGHJ：+0x20 AIMKJECPDPI : Unit、+0x28 JBNOMHPKBMP : float、+0x2C/+0x2D bool、+0x30 BJLAKIJPDFI : float | L169007 | 实测字段 |
| 玩家驱动实现 | BDGDBBFPDHA : PMFGNOABCAD : KPBKFHOKGHJ | L158491 / L159273 | 实测 |
| **每帧移动逻辑** | PMFGNOABCAD::OCAAPNNMGAE() **RVA 0x34A0E48**（L159377）；子类 BDGDBBFPDHA::OCAAPNNMGAE() **RVA 0x34A0DEC**（L158521） | — | 实测（调用 Unit::EJGLGAJLLOE 的唯一两处） |
| 移动执行 | Unit::EJGLGAJLLOE(Vector2, float, bool) **RVA 0x35508C8** | — | §10.3 |

**反汇编证据（PMFGNOABCAD::OCAAPNNMGAE，0x34A0E88 起）**：

~~~asm
0x34A0E90  ldr  x20, [x19, #0x20]     ; this.AIMKJECPDPI = Unit（驱动器持有的单位）
0x34A0EAC  ldr  x8,  [x19, #0x38]     ; 驱动器自己的数据字段（PMFGNOABCAD 层）
0x34A0EC0  ldrb w1,  [x8, #0x50]      ; → 传给 EJGLGAJLLOE 的 bool 参数
0x34A0ED8  ldp  s0, s1, [x9]          ; 方向 Vector2（来自 [0x81E3000+0x270] 的静态对象）
0x34A0EDC  bl   Unit::EJGLGAJLLOE     ; this=x20, Vector2 in s0/s1, bool in w1, float in s2
~~~

**注意（实测寄存器序）**：真实参数序是 (Vector2 dir, bool, float) —— Vector2 走 s0/s1、bool 走 w1、float 走 **s2**（本帧 s2=0）；dump 显示的 (Vector2, float, bool) 参数顺序不可信（元数据被打乱）。移植时按寄存器序或按 3 参自行声明。

**移速落地三条（按可靠性）**：
1. **优**：在 PMFGNOABCAD::OCAAPNNMGAE（0x34A0E48）/ BDGDBBFPDHA::OCAAPNNMGAE（0x34A0DEC）里把传给 EJGLGAJLLOE 的长度放大 → 只影响移动。
2. 中：直接操作 Unit+0xA0 m_CharacterController 的位移（每帧补偿）。
3. 兜底：改 KPBKFHOKGHJ+0x28 JBNOMHPKBMP（float，速度量候选【待验证】）与 +0x30 BJLAKIJPDFI（float）。

### 12.5 静态不可得（T9）

| # | 项 | 原因 | 替代 |
|---|---|---|---|
| N9 | 6 个槽各自对应「攻击/防御/移速/攻速/暴击」 | 属性名与属性类型枚举成员名全为随机串 | 配方 E（so 打日志 + Debug 面板文本对齐） |
| N10 | OKNDCLIKECF 的直接字段入口 | 它不是 Unit 的字段，只能经 IFEHBJOMMMB() 取 | §12.2 链 |
| N11 | EJGLGAJLLOE 的 C# 参数顺序 | 反射元数据顺序与寄存器序不一致 | 反汇编寄存器序（§12.4），或自行声明 3 参 |

## 13. 血量真相反查（T10，静态）

> 背景：用户实测「伤害倍率 100 打不死怪 / 无敌开着照样掉血 / 写 Unit+0x198 无效」。
> 结论先给：**+0x198 不是战斗血量，是属性缓存/哨兵值**（有三条反汇编铁证）；**+0x190 的写入受阵营门控**（这解释了为什么我们"写了没效果"）。RVA 口径同 §10（= dump 的 RVA = ELF vaddr = 文件偏移 + 0x4000）。

### 13.1 谁写 Unit+0x198（APPACLOOHNK, int）—— 全类内仅 4 处

| # | 方法 | 写点 RVA | dump 行 | 写法 | 语义判定 |
|---|---|---|---|---|---|
| 1 | __BT__Unit::.ctor__BT__ | 0x354B034 | L166819 | 构造期初始化 | 初始值 |
| 2 | __BT__Unit::FEDPMGDPLGD(int,int,...)__BT__ | 0x354BBE8 | L166886 | __BT__str w0,[x25,#0x194]__BT__ + __BT__str w0,[x25,#0x198]__BT__（**同一个 w0 写两处**） | **镜像/缓存**（见 13.1.1） |
| 3 | __BT__Unit::LNAOPAIAFCH(int)__BT__ | 0x354D6BC | L166910 | __BT__mov w8,#-1 ; str w8,[x19,#0x198]__BT__ + __BT__str wzr,[x19,#0x154]__BT__ | **写 -1 哨兵（失效标记）**，同时清 0x154 |
| 4 | __BT__Unit::MoveNext__(协程)__BT__ | 0x3553494 | L166520 区内 | 协程内按帧刷新 | 状态刷新缓存 |

**13.1.1 铁证：0x194 与 0x198 被写入同一个值，来源是属性容器**

~~~asm
; Unit::FEDPMGDPLGD  窗口 0x354BB78..0x354BBE8
0x354BB7C  bl   0x36A0A20        ; PHBGLMHPMMC::IFEHBJOMMMB()  ← 拿属性容器（§12.2 同一条链）
0x354BB88  str  x1, [x0, #0x18]!
0x354BBCC  mov  x0, x20
0x354BBD0  bl   0x3543070        ; Unit::OMGCLJGCPJA
0x354BBD8  mov  w1, #0x3A        ; ← 属性 id = 0x3A (58)
0x354BBE0  bl   0x36A2B30        ; PHBGLMHPMMC::CGIBCFFNFGL(…, 0x3A) ← 按 id 取 int 属性值
0x354BBE4  str  w0, [x25, #0x194] ; ← 同一值写 0x194
0x354BBE8  str  w0, [x25, #0x198] ; ← 同一值写 0x198（镜像）
~~~

**⇒ 0x198 是「属性容器里 id=0x3A 的属性值」的双写副本，不是受击扣血的目标。** 这与用户实测「写 0x198 无效」完全吻合。

**13.1.2 第二条铁证：0x198 还会被写成 -1 表示失效**

~~~asm
; Unit::LNAOPAIAFCH(int)  窗口 0x354D654..0x354D6C0
0x354D658  ldr  w8, [x19, #0x154]   ; 先看 0x154（状态机枚举）
0x354D65C  cmp  w8, #2 ; b.ne → 跳过
0x354D664  ldr  x8, [x19, #0x120]   ; 取状态对象（OIGNPAHJLCC）
0x354D6B8  mov  w8, #-1
0x354D6BC  str  w8, [x19, #0x198]   ; ← 写 -1（无效哨兵）
0x354D6C0  str  wzr, [x19, #0x154]  ; ← 同时把 0x154 清零
~~~

合法的"当前血量"不会被写成 -1，所以 0x198 只能是**缓存/哨兵**。

### 13.2 JJDKEBAMLNG / FAGNIOBDNJA 的基址链（是否真是 Unit this）

**结论：是 this（Unit），没有中间子对象**。完整链（含 wrapper 的参数搬运）：

~~~asm
; Unit::JJDKEBAMLNG(Unit src, long delta)  入口 0x354E28C
0x354E2A0  mov  x19, x2      ; x19 = delta（第 2 参 long）
0x354E2A4  mov  x21, x1      ; x21 = src（第 1 参 Unit）
0x354E2A8  mov  x20, x0      ; x20 = this（Unit）  ← 基址来源
0x354E2D0  tbz  w0, #0, 0x354E304     ; IFix 未打补丁 → 走原逻辑
0x354E300  b    0x44B8AC4             ; 已打补丁 → ILFix __Gen_Wrap_77
; ---- 原逻辑 ----
0x354E304  ldr  x8, [x20, #0x1B0]     ; this.JAODIJCPHFL（Action<Unit,Unit,long>）
0x354E30C  ldr  x9, [x8, #0x18]       ; delegate.invoke
0x354E310  ldr  x0, [x8, #0x40]       ; delegate.target
0x354E314  ldr  x4, [x8, #0x28]
0x354E318  mov  x1, x21 ; mov x2, x20 ; mov x3, x19   ; (src, this, delta)
0x354E324  blr  x9                     ; ← 先无条件 raise 血量事件
0x354E328  ldr  w8, [x20, #0x150]     ; this.GPJLPFOKIDE（阵营枚举）
0x354E32C  cmp  w8, #1                 ; == PLAYER ?
0x354E330  b.ne 0x354E36C              ; ★ 不是 PLAYER 就整段跳过，不写血量
0x354E358  bl   0x7B0D8BC
0x354E35C  tbz  w0, #0, 0x354E36C     ; 第二个前置判定
0x354E360  ldr  x8, [x20, #0x190]     ; curHp
0x354E364  add  x8, x8, x19           ; curHp += delta
0x354E368  str  x8, [x20, #0x190]     ; ← 写回 this+0x190
0x354E36C  ret
~~~

~~~asm
; Unit::FAGNIOBDNJA(long maxHp)  入口 0x354A7AC
0x354A7B4  mov  x20, x1      ; x20 = 参数 long
0x354A7B8  mov  x19, x0      ; x19 = this（Unit）
0x354A7F4  cmp  x20, #1
0x354A7F8  b.lt 0x354A804
0x354A7FC  str  x20, [x19, #0x1A0]   ; this+0x1A0 = 参数
0x354A804  mov  w8, #1
0x354A808  str  x8,  [x19, #0x1A0]   ; 参数 < 1 时钳位为 1
~~~

**⚠ 关键发现（解释"写了没效果"）**：__BT__JJDKEBAMLNG__BT__ 的第 2 段被 __BT__cmp w8,#1__BT__（Unit+0x150 阵营枚举 == PLAYER）门控。如果我们的玩家单位 __BT__GPJLPFOKIDE != 1__BT__（例如玩家实际是别的枚举值，或该字段未初始化），**事件照样 raise、但血量一个字都不会写** —— 现象就是"我们写了、日志说 write ok、战斗里没变化"。

**→ 立即可验证的一条**：so 里打印玩家单位 __BT__*(int*)(unit+0x150)__BT__ 的真实值。若不是 1，那么 0x190 这条链对玩家根本不生效，"无敌/血量"必须改走别的路径。

### 13.3 第三个嫌疑：KCPGOACDHOB（驱动器）自带一套 0x190/0x194/0x198/0x1A0

`Unit+0x38 GAMJINGNDHM : KCPGOACDHOB`（§12.4 的移动/动作驱动器）内部**也有一组同名偏移**，且在 4 个方法里被一起写：

| 方法 | 写点（驱动器基址 this=x20） | 写入内容 |
|---|---|---|
| __BT__KCPGOACDHOB::OCKDCBJFDMB(int)__BT__ | 0x34AD9A0 / 0x34AD9B0 / 0x34AD9B8 | __BT__str w8,[x20,#0x190]__BT__（常量 1）、__BT__str w8,[x20,#0x198]__BT__（来自 __BT__[x19,#0xb8]__BT__）、__BT__str x1,[x20,#0x1A0]__BT__ |
| __BT__KCPGOACDHOB::MCAKCMHPAKK__BT__ | 0x34AE288 / 0x34AE298 / 0x34AE2A0 | 同样三连写 |
| __BT__KCPGOACDHOB::NAPOEBKAGLG__BT__ | 0x34AE950 / 0x34AE960 / 0x34AE968 | 同样三连写 |
| __BT__KCPGOACDHOB::EIOGCGMCEEC__BT__ | 0x34B1744 / 0x34B1754 / 0x34B175C | 同样三连写 |

**推断（不是实测）**：Unit 上的 0x190/0x198/0x1A0 很可能是**从驱动器同步过来的展示值**，真正的战斗数值在驱动器里。这也解释了为什么改 Unit 侧无效。**待验证**。

### 13.4 死亡判定：静态未定位（说明原因，不猜）

- 时间盒内没找到"读 0x190/0x1A0 → 与 0 比较 → 触发死亡"的干净证据。
- 原因是**线性反汇编越界污染**：我按"方法起始地址 + 固定条数"扫描，遇到短方法会继续读进下一个方法，把邻居的指令算成本方法的（例如 `DFNCCEBKIFF` 被误判为"读 0x1A0 后 cbz"，实际它整段是 __BT__m_CharacterController + Transform__BT__ 相关，见下）。

~~~asm
; Unit::DFNCCEBKIFF(Unit other) 0x3549738 —— 实测与血量无关
0x3549780  ldr  x0, [x19, #0xA0]    ; m_CharacterController
0x3549790  bl   0x7B60430          ; CharacterController 相关
0x35497F0  bl   0x7B0AFB4 / 0x7B16F58 / 0x7B16FF8 / 0x7B171F8   ; Transform 读写
~~~

**下一步定位死亡判定的正确方法（so 侧日志，不用 frida）**：
~~~text
[配方 F · 找死亡判定与真血量]
1. 同一帧打印四组值：Unit+0x190 / Unit+0x1A0 / Unit+0x198，以及 KCPGOACDHOB(Unit+0x38)+0x190/+0x1A0/+0x198。
2. 让玩家被打到死（或让怪被打死），观察哪一组「单调下降到 0 并停住」，那一组才是真血量；
   特别看是否出现「Unit 侧不变、驱动器侧在降」的情况 —— 那就证实 13.3 的推断。
3. 死亡瞬间再打印 Unit+0x158/+0x159/+0x15A（三个 bool 状态位，实测均为 strb 写点）与 +0x154（状态机枚举），
   看哪个由 0/1 翻转 → 那就是 isDead 标志；再对该标志的写点回溯，即得死亡判定函数。
4. 同时打印属性容器的 6 个 long 槽（§12.3），确认「UI 血条」读的是哪个槽。
~~~

### 13.5 结论汇总（给 builder 的行动项）

| # | 结论 | 置信度 | 行动 |
|---|---|---|---|
| 1 | **Unit+0x198 是属性缓存/哨兵，不是血量**（0x194 与它同值双写；还会写 -1） | 实测（两条反汇编铁证） | **停止写 0x198** |
| 2 | JJDKEBAMLNG / FAGNIOBDNJA 写的确实是 **this(Unit)**，无中间对象 | 实测（完整指令链在 13.2） | 若仍要试，只需确认阵营值 |
| 3 | 0x190 的写入被 __BT__GPJLPFOKIDE(cmp #1, PLAYER)__BT__ 门控 | 实测 | **先打印玩家单位 +0x150 的真实值**，不是 1 就换路径 |
| 4 | KCPGOACDHOB(Unit+0x38) 自带 0x190/0x1A0 三连写，可能是真值来源 | 推断，待验证 | 配方 F 第 1/2 步 |
| 5 | 死亡判定方法 | 静态未定位 | 配方 F 第 3 步（看 0x158/0x159/0x15A/+0x154 谁是 isDead） |

## 14. 属性容器 id 表与 setter（T11）

> 起因：§13 铁证 1 里 `FEDPMGDPLGD` 用 id `0x3A`(58) 从属性容器取值再双写 0x194/0x198。顺着这条线挖到了**属性 id 枚举**，语义全可读。
> RVA 口径同 §10（dump 的 RVA = ELF vaddr = 文件偏移 + 0x4000）。

### 14.1 取值 API：CGIBCFFNFGL

| 项 | 值 |
|---|---|
| 声明 | `public int CGIBCFFNFGL(KMLFBAENBMH OHCCOBDLACB) { }` |
| 所属类 | **PHBGLMHPMMC**（= 属性容器基类，§12.1） |
| RVA | **0x36A2B30**（Offset 0x369EB30，Slot 59） |
| dump 行 | **L182407** |
| 参数 | **1 个**：`KMLFBAENBMH`（属性 id 枚举）。**不是 (int id, int default)** |
| 返回 | **int**（属性当前值，已含所有 modifier 的合成结果） |
| 调用点数 | **87 处**（全 Assembly-CSharp 线性扫描实测） |

另有两个同族读接口（在接口 `KEMEJCCDJKJ` 里，L183878）：`KHPIJPHCFGP(int)` / `KHPIJPHCFGP(KMLFBAENBMH)` —— 按索引/按 id 读，返回 int。

### 14.2 属性 id 枚举 `KMLFBAENBMH`（**成员名全可读，这是本轮最大收获**）

声明行 **L185708**（TypeDefIndex 2907）。节选（完整 0–77 见 dump）：

| id | 名称 | 战绩用途 |
|---|---|---|
| 0 | NONE | — |
| 1 | NO_SKILL | 禁技能 |
| 2 | NO_NORMAL_ATK_SKILL | 禁普攻 |
| **3** | **HP_ADD** | **血量加值（无敌/加血首选）** |
| **4** | **PHY_ATK_ADD** | **物理攻击加值（伤害倍率候选）** |
| 5 | ELE_ATK_ADD | 元素攻击加值 |
| 6 | DEF_ADD | 防御加值 |
| 7 | CRITICAL_CHANCE_ADD | 暴击率 |
| 8 | CRITICAL_DAMAGE_ADD | 暴击伤害 |
| **9** | **MOVE_SPEED_ADD** | **移动速度（移速功能第一落点）** |
| 10 | AURA_BALL_ADD | 灵球 |
| 11 | MAX_ENERGY_ADD | 能量上限 |
| 12 | PHY_ATK_RATE | 物理攻击百分比 |
| 13 | ELE_ATK_RATE | 元素攻击百分比 |
| 14 | DEF_RATE | 防御百分比 |
| **15** | **HP_RATE** | **血量百分比（血量上限倍率）** |
| 16 | MAX_ENERGY_RATE | 能量上限百分比 |
| 18 | PHYSIC_FINAL_DAMAGE_RATE | 物理最终伤害率 |
| 19 | ELEMENT_FINAL_DAMAGE_RATE | 元素最终伤害率 |
| 28 | FINAL_DAMAGE_RATE | **最终伤害率（通用增伤）** |
| 29–33 | *_DAMAGE_RESIST_RATE | 各元素/物理抗性 |
| 34 | HEAL_EFFECT_GAIN | 治疗增益 |
| **36** | **COOLDOWN_REDUCTION** | **冷却缩减（无限技能第一落点）** |
| **37** | **DAMAGE_IMMUNE** | **伤害免疫 = 无敌** |
| **38** | **NOT_TARGETABLE** | 不可被选中（伪无敌/脱战） |
| 39 | NO_MOVEMENT_ACTION | 禁止移动动作 |
| **40** | **UNDYING_SPIRIT** | 不死（血量见底不死） |
| 41 | LIFE_STEAL | 吸血 |
| 50/51/52 | WEAPON/RELIC/DASH_SKILL_COOLDOWN_REDUCTION | 分类冷却缩减 |
| 53 | COMBO_EXTRA_HIT | 连击额外段 |
| **58 (0x3A)** | **RICOCHET_COUNT** | **弹射次数** ← **就是 §13 里写 0x194/0x198 的那个 id** |
| **59** | **NORMAL_ATK_SPEED** | **普攻速度（攻速）** |
| 60 | NORMAL_ATK_LEVEL | 普攻等级 |
| 62 | DAMAGE_GUARD | 伤害护盾 |
| 63–77 | *_DAMAGE_VULNERABLE | 各类易伤 |

**⚠ 重要修正**：lead 假设「0x3A = 血量」**不成立** —— `0x3A = 58 = RICOCHET_COUNT`（弹射次数）。
所以 §13 的结论要更精确：**Unit+0x194/+0x198 缓存的是「弹射次数」属性**（不是血量）。这依然证明「0x198 是属性缓存」，但缓存的是弹射次数，写它当然不会掉血。

**交叉证据（实测）**：`CGIBCFFNFGL` 的 87 个调用点里，`KCPGOACDHOB::BJEHIDKAJDB` 传的就是 **id=58**（与 FEDPMGDPLGD 的 0x3A 一致）；另有 `KCPGOACDHOB::FGOFMCLGIKP` 传 id=43（SPECTRAL_OVERFLOW）、`AIControllerBase` 多处传 id=39（NO_MOVEMENT_ACTION）、`ActionButtonBase::PNOIALBMEJJ` 传 108/119/120（超出已知枚举，属后续追加的 id）。

### 14.3 写入接口（**这是无敌能不能落地的地方**）

`OKNDCLIKECF` 上的写/改方法（实测签名 + RVA）：

| 方法 | 签名 | RVA | dump 行 | 推断语义 | 置信度 |
|---|---|---|---|---|---|
| GPHAMODMGHC | `(float)` | 0x36ACA2C | L183517 | 设某个 float 基数/倍率（无 id 参数，疑似整体倍率） | 低 |
| ONCOLFNNMBB | `(MJJMGGGHDGE)` | 0x36ACA34 | L183520 | 用属性对象整体替换某槽 | 中 |
| HCFPHOCMANE | `(MJJMGGGHDGE)` | 0x36ACAEC | L183527 | 同上（另一槽） | 中 |
| FADNOHFBPCM | `(KPGOJLOMPEN, float)` | 0x36AC304 | L183506 | (来源类型, 值) 写 modifier | 中 |
| BBEDPCDDMDB | `(KMLFBAENBMH)` | 0x36AC940 | L183509 | **按 id 取**（反汇编：把 w1 原样透传给 wrapper，未见 str 到槽） | 中 |
| JKCCBMHOECH | `(OKNDCLIKECF, KEMEJCCDJKJ)` | 0x36AB8F8 | L183494 | 容器之间合并/拷贝 | 中 |
| FPIALCOELGG::HCFPHOCMANE | `(MJJMGGGHDGE)` | 0x36B0E30 | L183770 | 子类覆写，同上 | 中 |

**结论：容器上没有任何 `Set(KMLFBAENBMH id, int/float value)` 形式的通用写入口。** 静态不可得的部分是「按 id 写值」的 API —— 它很可能根本不存在（属性是**由 modifier 叠加算出来的**，不是可直接赋值的字段）。

**但有一条确定的落地路径**：OKNDCLIKECF 的 6 个槽各自有 `MJJMGGGHDGE` 后备字段（+0x18…+0x40，§12.3）。`MJJMGGGHDGE` 是**接口**（L183159），实现类 `ACKAKNLFLDI`（L183225）带 `JKCCBMHOECH(MJJMGGGHDGE)` 与 `GNMIGMFJBMO()`（克隆）。⇒ 想改某个属性，正确做法是**改该槽 MJJMGGGHDGE 对象内部的数值并触发重算**，而不是写容器的 long 结果值（long 是算出来的）。

### 14.4 FEDPMGDPLGD 的调用者（判断 0x198 的重同步频率）

线性扫描实测 **11 处**调用，**没有任何 Update/LateUpdate/每帧协程**：

| 调用者 | 说明 |
|---|---|
| Unit::BLKEJBDMGAB | Unit 自身的一次性初始化/刷新 |
| SpawnManager::MoveNext | 单位生成协程里（**出生时**） |
| JJFBPGNPMKB::NLGHFBNBBDJ / JHHBJMANJHJ / JKAGMENNMMA | 三个不同方法（装备/属性变化时） |
| DemoQuestItemPreviewDialog::OJCGDHFCGBB | 预览面板 |
| BattleTutorial::MoveNext | 教学 |
| UIBattleCutinPanel::MoveNext | 战斗特写 UI |

**⇒ 0x198 不是每帧覆盖，只在「出生 / 属性变化 / UI 刷新」时重写。** 这意味着：写 0x198 短时间内不会被覆盖，但它**依然不影响战斗**（因为战斗读的是容器按 id 取的值，不是这个缓存）。这与实测「写 0x198 无效但值确实变了」完全一致。

### 14.5 一句话结论（给 builder）

**真血量/真属性不在 Unit 的字段里，而在属性容器里按 `KMLFBAENBMH` id 取值（`PHBGLMHPMMC::CGIBCFFNFGL(KMLFBAENBMH)`，RVA 0x36A2B30，返回 int）。血量 id = 3 (HP_ADD) / 15 (HP_RATE)；无敌 id = 37 (DAMAGE_IMMUNE) / 40 (UNDYING_SPIRIT)；移速 id = 9 (MOVE_SPEED_ADD)；冷却 id = 36 (COOLDOWN_REDUCTION)；攻速 id = 59；增伤 id = 28 / 18。但容器没有 Set(id,value) 写接口（属性由 modifier 合成），所以：
- **读**：一律走 `CGIBCFFNFGL(id)`；
- **写**：只能改 6 个槽的 `MJJMGGGHDGE` 对象内部数值（+0x18…+0x40，§12.3），或退回到写 `Unit+0x190/+0x1A0`（须先确认 `+0x150 == 1`，§13.2）；
- **0x198 明确作废**（缓存的是 id 58 = RICOCHET_COUNT 弹射次数）。

## 15. 技能 CD（T19）

> 结论先行：**`values[36]`（COOLDOWN_REDUCTION）在运行时没有任何读取点** —— 这是本轮实测确认的，与「写 36 无效」完全一致。无限技能只能走 **字段清零** 或 **hook 释放判定**，而这两个落点本轮**未定位**（时间盒到点，下面给出已排除项与下一步的确切动作）。

### 15.1 id=36 是否被读：**实测 0 处**

全段线性扫描三族属性读接口（`mov w1,#imm` + `bl` 配对，窗口 0x40 字节）：

| 接口 | 调用点 | 是否出现 id=36(0x24) | 实际出现的 id（示例） |
|---|---|---|---|
| `PHBGLMHPMMC::CGIBCFFNFGL` 0x36A2B30 | **87** | **否** | 39(×8)、60(×4)、43(×4)、1(×3)、101(×3)、58(×3)、124(×3)、59(×3)、120、2、121、111、40、133 … |
| `PHBGLMHPMMC::IFEHBJOMMMB` 0x36A0A20 | **70** | **否** | 1(×8)、3、7、5、2、4 |
| `OKNDCLIKECF::FJMHJMEPFMC` 0x36A2BA4 | **78** | **否** | 26(×3)、4、93、94、99、18、27、62 … |

**⇒ 与 id=28(FINAL_DAMAGE_RATE) 同样命运：枚举里定义了，但战斗/UI 代码从不读它。** 冷却缩减在本作里**没有接到属性系统**。

### 15.2 技能 CD 的实际存储：**本轮未定位**

已尝试并排除：

| 尝试 | 结果 | 原因 |
|---|---|---|
| 在 `SkillObjBase` 家族里扫「float 递减 + `fcmp #0.0`」 | 命中 55 处但**全是噪声** | `SkillObjBase` 的 `TYPES` 条目包含大量嵌套迭代器类，方法 RVA 指向 `0x341xxxxx`（另一片区），命中 `prev=blr x9` 说明落在 IFix wrapper 尾巴上 ⇒ **RVA 归属污染** |
| 用 `ActionButton.m_CoolDownText(+0x98)` 反查取值 | 找到 `ActionButton::EHPPIMHHKGI`(0x300A548)、`ActionButton::NLGCHDPEPOF`(0x300A5EC) 会写 `+0x98`，但它们只是**刷文本**，值来自 `ActionButtonBase::FNHGKCIGAJN`(0x300A920) 等辅助，CD 源字段未显现 | 时间盒 |
| `field_access.json` 按 `disp 0x98/0x90` 过滤 ActionButton 族 | **0 条**（该索引未覆盖该区段偏移） | 索引范围限制 |

### 15.3 释放前置判定：候选链（实测地址，语义待验证）

`ActionButtonBase::Update()`（**RVA 0x300BEA0**，L634）是每帧冷却刷新入口，实测调用链：

~~~asm
0x300BF28  bl HClock::NMHDPNKKNBP      ; 取当前时间（ms/int）
0x300BF34  bl ActionButtonBase::EDMIEKLOHDP   ; ★ 0x300B5F4 —— 冷却计算候选
0x300BF40  bl ActionButtonBase::FNHGKCIGAJN   ; 0x300A920 —— 刷新显示
0x300BFCC  bl ActionButtonBase::BEHJPFIEHPA   ; 0x3009A04 —— 交互/可用态候选
~~~

**这是最值得下轮的三个函数**（都在 0x3009A04 / 0x300A920 / 0x300B5F4），其中 `BEHJPFIEHPA` 名字位置像"按钮可用性判定"，**很可能就是「能否释放」的 UI 侧等价物**。

### 15.4 已排除的"无限技能"方案

| 方案 | 状态 |
|---|---|
| `values[36] = 100` | **实测无效**（本作不读 id=36） |
| 属性 id 37/40（免疫/不死） | 与技能无关，§13 已证 |
| `UnitSkillInfo.m_NormalAttackSpeed(+0x1C)` 放大 | 只影响普攻节奏，**不影响技能 CD**（配置是共享资产，改了所有同类单位一起变） |

### 15.5 下轮静态动作（按性价比，30–40 分钟可出结论）

1. **反汇编 `ActionButtonBase::EDMIEKLOHDP`（0x300B5F4）**：它若形如 `remaining = endTime - now`，里面的字段偏移就是 CD 源；
2. **反汇编 `ActionButtonBase::BEHJPFIEHPA`（0x3009A04）**：若返回 bool 且读同一字段，**这就是 hook 点**（恒 true = 无限技能）；
3. 若 UI 侧只是镜像，则去 `PlayerController`（技能释放发起者）找：从 `ActionButton` 的点击回调往上追 `PlayerController::*`。

### 15.6 本轮结论（一句话）

**`values[36]` 确认无效（静态实测 0 读取点）；无限技能需要 hook —— 首选候选 `ActionButtonBase::BEHJPFIEHPA`（RVA 0x3009A04，待下一轮反汇编确认是否返回"能否释放"），次选 hook `EDMIEKLOHDP`（0x300B5F4）并把剩余 CD 清零。字段方案本轮未定位。**
### 15.7 T20 追加：UI 链已证不是门禁（实测）

| 函数 | 调用点 | 结论 |
|---|---|---|
| `ActionButtonBase::BEHJPFIEHPA` 0x3009A04 | **2 处，全在 UI**：`ActionButton::BOBHFNCDAFM`(L446)、`ActionButtonBase::Update`(L634) | **不是施放门禁** |
| `ActionButtonBase::EDMIEKLOHDP` 0x300B5F4 | **1 处**：`ActionButtonBase::Update`(L634) | 不是 CD 计算 |

反汇编实测：

~~~asm
; BEHJPFIEHPA() 0x3009A04 —— 纯 UI 可用态
0x3009A3C  ldrb w8, [x19, #0x71]     ; bool：强制可用？
0x3009A40  cbz  w8, ...
0x3009A44  mov  w0, #1 ; b 返回      ; ⇒ return true
0x3009A4C  ldr  w8, [x19, #0x44]     ; int 状态枚举
0x3009A50  sub  w9, w8, #2 ; cmp #3 ; b.hi
0x3009A5C–6C 跳转表（0x1529000+0xc28）按状态分支
0x3009A74/80/8C/98  ldrb [x19,#0x31 / #0x32 / #0x30]；非 0 → return true
0x3009AA0  cmp  w8, #0
0x3009AA4  cset w0, eq                ; ★ 最终 return (state == 0)
~~~

~~~asm
; EDMIEKLOHDP() 0x300B5F4 —— 只是个 int getter
0x300B62C  ldr  w0, [x19, #0x4C]      ; return this+0x4C
0x300B630  ret
; 紧随其后的第三个函数：0x300B674 ldr w0,[x19,#0x38] ; ret  （另一个 getter）
~~~

**⇒ 两个函数都不读任何 CD 字段、不读属性、不涉及时间**。`BEHJPFIEHPA` 只根据按钮的 `+0x44` 状态枚举与几个 bool 决定"还能不能点"。**hook 它只能骗 UI，点下去照样会被游戏层拒绝。**

### 15.8 下一步（精确到地址，10 分钟可出）

1. 反汇编 `ActionButton::BOBHFNCDAFM`（L446，`BEHJPFIEHPA` 的唯一非 Update 调用者）—— 若它是点击/指针处理，顺它往下的 `bl` 追到 `PlayerController` / `SkillController`；
2. 从 `ActionButton` 的 onClick 事件字段（`+0xC8` 附近的 `Action`/`UnityEvent`）反查注册者；
3. 找到施放入口后，其内部「CD 未好 → return」的分支才是真门禁。

**CD 字段的定位顺延**：本轮确认它**不在** `ActionButtonBase` 的 `+0x44/+0x4C/+0x38/+0x30..0x32` 里（那些是 UI 状态），也不在属性系统（id 36 无读取）。
### 15.9 T21：点击链已追到第 4 跳（施放入口尚未落地）

**结论：本轮拿到完整的「指针事件 → 按钮内部」链路，但施放入口（含 CD 判定）在第 5 跳之后，时间盒内未落地。**

| 跳 | 函数 | RVA | 行 | 关键指令 |
|---|---|---|---|---|
| 1 | `ActionButton::FGPALOJDGBI(PointerEventData)` | **0x3006E18** | L350 | 转发 `bl ActionButtonBase::FGPALOJDGBI`；返回 true 后对 `+0x80`(Image) 做缩放反馈，**无施放** |
| 2 | `ActionButtonBase::FGPALOJDGBI(PointerEventData)` | **0x3006EF8** | — | `ldp x9,x1,[x8,#0x1C8] ; blr x9`（虚调用槽 0x1C8 返回 bool）→ true 时 `strb 1,[x19,#0x34]`，再 `bl FFIJIKGDPNO` / `bl KPAPJHKNAKO` |
| 3 | `ActionButtonBase::FFIJIKGDPNO` | **0x300B94C** | — | 未展开 |
| 3b | `ActionButtonBase::KPAPJHKNAKO` | **0x300B9AC** | — | 未展开 |

另有第二个指针入口 `ActionButton::IJNLNCMKKEE(PointerEventData)` **0x30076D8**（L377），同样先转发 `ActionButtonBase::IJNLNCMKKEE`（0x30077A4）。

**⇒ 施放候选（下一轮从这里开始，一跳即可判定）**：
1. `ActionButtonBase::KPAPJHKNAKO` **0x300B9AC** —— 名字位置在"按下之后"，最可能是"提交/触发"；
2. `ActionButtonBase::FFIJIKGDPNO` **0x300B94C**；
3. 虚槽 `+0x1C8` 的实现（第 2 跳里的 `ldp x9,x1,[x8,#0x1C8]; blr x9`）—— 这是"能否按下"的虚方法，**若它内部读 CD 就是门禁层**（比 `BEHJPFIEHPA` 更接近逻辑）。

### 15.10 已确定排除的（本轮新增）

| 项 | 证据 |
|---|---|
| `ActionButtonBase::BEHJPFIEHPA` 不是门禁 | 仅 2 个调用者，全 UI（§15.7） |
| `ActionButtonBase::EDMIEKLOHDP` 不是 CD | `ldr w0,[x19,#0x4C] ; ret`，纯 getter |
| `ActionButton::BOBHFNCDAFM` 不是点击处理 | 它只是"可点否"组合判定（调用 BEHJPFIEHPA + KEIMCNLNOIH + `+0xE8` + `bl 0x7B0A4C0`） |
| `ActionButton` 的字段 `+0x90/+0x98/+0xA8` 是冷却 **UI**（Image/TMP_Text/Transform），不是数据 | 字段表实测 |

### 15.11 下一轮（10 分钟，纯静态）建议顺序

1. `ActionButtonBase::KPAPJHKNAKO` 0x300B9AC → 看是否 `bl` 到 `PlayerController::*` / `SkillController::*`；
2. 虚槽 `+0x1C8` 实现：从 `ActionButton::` 或 `ActionButtonBase::` 的 vtable 里定位（可取 `ActionButton` 的 klass→vtable，slot 0x1C8/8 = 57 号方法）；
3. 拿到施放入口后，其内部 `if (now < endTime) return` 分支里的字段即 CD 源（届时可给"数据面清零"方案）。

**当前可交付状态**：hook 方案无 RVA（施放入口未落地）；数据面方案无字段（CD 源未落地）。**本轮不构成可实施结论**，但把搜索范围从"整个 ActionButton 类"收敛到 `KPAPJHKNAKO`/`FFIJIKGDPNO`/虚槽 +0x1C8 三个点。
### 15.12 T22：两个候选都被排除，但发现按钮持有「技能描述对象」

| 函数 | 实测行为 | 判定 |
|---|---|---|
| `ActionButtonBase::FFIJIKGDPNO` **0x300B94C** | `ldr x0,[x19,#0x28]`(m_CancelSkillButton) → Unity null check → `bl 0x7B0AFF0` → `mov w1,#1` → `b 0x7B0DAC4`（`SetActive(true)`） | **纯 UI 显隐**，排除 |
| `ActionButtonBase::KPAPJHKNAKO` **0x300B9AC** | 读 `+0x20`(m_SkillJoyStick : VariableJoystick) → `bl Joystick::FBDPPJFLDEJ`(0x37BAC14) / `Joystick::OGHCEDDDBGO`(0x37BABCC) → `fmul/fadd` → `stur d8,[x19,#0x64]` / `str s9,[x19,#0x6C]`(Vector3) | **摇杆视觉方向计算**，排除 |

**⇒ `FGPALOJDGBI` 之后的两条分支都不是施放。** 这也解释了为什么 hook 它们不会有任何效果。

### 15.13 新发现：按钮持有技能/动作描述对象（下一步的入口）

`ActionButtonBase` 字段表（实测）：

| 字段 | 类型 | 偏移 | 意义 |
|---|---|---|---|
| `LDEBGEPHPCL` | **`PKFLGAEJICG`** | **+0x38** | **按钮代表的技能/动作对象**（这就是施放的参数来源） |
| `IBFGNKLOLKP` | `BMLHHOHLCDB` | +0x44 | 状态枚举（`BEHJPFIEHPA` 在此 switch） |
| `MFAODGIHGJK` | int | +0x4C | `EDMIEKLOHDP` 返回的 int（槽位/索引候选） |
| `HFNHFJAFIOG` | int | +0x40 | 未知 int |
| `APIPFENHILA` | **`Action`** | **+0x78** | **点击回调（无参 Action）——注册者即施放入口** |
| `m_SkillJoyStick` / `m_CancelSkillButton` | VariableJoystick / CancelActionButton | +0x20 / +0x28 | 摇杆与取消键 |

### 15.14 下一轮（5–10 分钟能落地，纯静态）

1. **`ActionButtonBase+0x78 (Action APIPFENHILA)` 的 `+=` 注册点**：扫 `ldr [x?,#0x78]` + `Delegate.Combine` 模式，或直接看谁在 `ActionButton`/`ActionButtonBase` 外部给它赋值 —— **注册的回调就是施放函数**；
2. **`PKFLGAEJICG`（+0x38 的类型）**：反汇编该类的成员，找 `(int skillId)` / `()` 形式且 `bl` 到 `SkillObjBase` 相关的方法；
3. 若回调链仍指向 UI，则去 `PlayerController`（它曾以 id=121 调 `CGIBCFFNFGL`，说明它读技能可用标志）。

### 15.15 当前状态（截至本轮）

| 目标 | 状态 |
|---|---|
| 施放入口 RVA | **未拿到** |
| CD 字段（类+偏移） | **未拿到**（已排除：属性系统 id=36、`ActionButtonBase +0x30~0x54` 全部 UI 状态位） |
| 可 hook 点 | **暂无** |
| 收敛进度 | ActionButton 53 方法 → 指针链 4 跳全排除 → **下一跳锁定 `+0x78 Action` 注册者 / `PKFLGAEJICG` 类型** |
### 15.16 T23：两个重要否定结论（省下后续时间）

**① `PKFLGAEJICG` 是 enum，不是类** —— 实测 dump：`fields=1`（仅 `value__ : int` @0x0）、**`methods=0`**。
⇒ `ActionButtonBase+0x38 LDEBGEPHPCL : PKFLGAEJICG` 只是**动作类型枚举值**，不是"按钮代表的技能对象"。上一轮 §15.13 里把它当"技能对象"是错的，**撤回**。

**② `+0x78` 的写入点无法用"偏移扫描"定位** —— 实测全段 `str [x?,#0x78]` **1912 处**，命中类分布极散（`?`1097、BossAIController04Head 81、SpawnManager 29、ChatManager 27、AGGAGLELDIM 16 …），且多数是 IFix wrapper 的 `str x21,[x19,#0x78]!` 前置自增栈帧写法 ⇒ **0x78 是通用偏移，按偏移扫必然噪声爆炸**。

### 15.17 修正后的下一步（不再按偏移扫）

| 目标 | 正确做法 |
|---|---|
| 找「谁给按钮注册点击回调」 | 只在**引用了 ActionButton/ActionButtonBase 类型**的方法里找 `Delegate.Combine` / `ldr [x?,#0x78]`；限定方法集合后噪声可降两个数量级 |
| 找施放入口 | 换向：从 `PlayerController` 侧进（它读 id=121 的技能可用标志），找 `(int skillId)` / `()` 且内部 `bl` 到 `SkillObjBase` 家族的方法 |
| 找 CD 字段 | 只在 `PlayerController` / 技能组件里找，不再在 UI 侧找（UI 侧已彻底排除） |

### 15.18 已排除清单（累计，UI 侧全部关死）

| 对象 | 排除依据 |
|---|---|
| `ActionButtonBase::BEHJPFIEHPA` 0x3009A04 | 2 个调用者全 UI；`return state==0` |
| `ActionButtonBase::EDMIEKLOHDP` 0x300B5F4 | `ldr w0,[x19,#0x4C]; ret` |
| `ActionButton::BOBHFNCDAFM` 0x3009968 | 可点否组合判定 |
| `ActionButtonBase::FFIJIKGDPNO` 0x300B94C | SetActive(true) |
| `ActionButtonBase::KPAPJHKNAKO` 0x300B9AC | 摇杆方向计算（Joystick::*） |
| `ActionButtonBase +0x30..+0x54` | 全部 UI 状态位 |
| `+0x38 PKFLGAEJICG` | **是 enum** |
| 属性 id=36 | 全包 0 读取点 |

**⇒ 无限技能无法在 UI 侧实现。必须从 `PlayerController` / 技能系统侧进入。**
### 15.19 T24：id=121 是 **0/1 标志位**，读点在 `PlayerController` 移动逻辑（不是技能就绪）

`PlayerController::POGJOIGKOPN()` **RVA 0x3545E50**（L166228）内共 4 处属性读取，其中 **id=0x79(121) 两处**：

| 位置 | 指令 | 用法 |
|---|---|---|
| **0x3546030** | `mov w1,#0x79 ; bl CGIBCFFNFGL ; mov w21,w0 ; … ; cbz w21, 跳过` | **当 bool 用**（非 0 = 真） |
| **0x35463BC** | `mov w1,#0x79 ; bl CGIBCFFNFGL ; cbz w0, 0x35463FC` | **当 bool 用**；非 0 时走 `ldp s0,s1,[x19,#0x78] ; fneg s9,s0 ; fneg s8,s1`（对某方向向量取反） |

另两处：`id=0x3C(60)` @0x35465EC、`id=0x3B(59)` @0x35469DC。

**⇒ 结论**：
1. **取值是 0/1 两态**（不是时间戳/毫秒）—— 从 `cbz`/`cbnz` 判读得出【实测】；
2. **它不是"技能就绪"标志**：两处读点都落在**移动/朝向**分支上（第二处显式对 `+0x78/+0x7C` 的向量做 `fneg`），语义更像"禁用移动/反向控制"这类状态位（与 id=39 `NO_MOVEMENT_ACTION` 同族）；
3. **不要写 `values[121]=1`** —— 按现有证据，置 1 大概率会**禁用移动或反转方向**，而不是解锁技能。

### 15.20 本轮未完成项（时间盒到点，如实记录）

| 项 | 状态 |
|---|---|
| Q2 谁给 121 赋值 | **未做**（需要扫 `mov w1,#0x79` + 写接口调用点，约 1 分钟扫描 + 分析） |
| Q3 `POGJOIGKOPN` 的调用者 | **未做**（同上，需全段扫 `bl 0x3545E50`）；从其函数体形态（大量静态初始化 + 长流程）判断**是 Update/主循环**，不是施放入口 |
| 施放入口 / CD 字段 | **仍未拿到** |

### 15.21 累计结论（技能线）

| 已排除 | 依据 |
|---|---|
| 属性 id=36 (COOLDOWN_REDUCTION) | 全包 0 读取点 |
| 属性 id=121 | 是移动/朝向标志位，不是技能就绪 |
| UI 全链（BEHJPFIEHPA / EDMIEKLOHDP / BOBHFNCDAFM / FFIJIKGDPNO / KPAPJHKNAKO） | 逐个反汇编排除（§15.7–15.12） |
| `ActionButtonBase +0x38` | 是 enum，不是对象 |

**⇒ 技能 CD 既不在属性系统、也不在 UI 层。剩下的唯一方向：`SkillObjBase` / 技能事件对象的生命周期字段，或 `PlayerController` 内真正调用技能生成的那条链（需要从"谁 new SkillObjBase"反向查）。**
### 15.22 T25：方案 A 不成立 —— `SkillObj*` 是 MonoBehaviour，靠 Instantiate 而非 `new`

**实测**：扫 `bl` 到下列 8 个 ctor，**全包 0 个调用点**：

| ctor | RVA |
|---|---|
| `SkillObjBase::ctor()` | 0x3411CA4 |
| `SkillObjBase::ctor(int <>1__state)`（3 个迭代器版本） | 0x3415D78 / 0x3417D20 / 0x341914Cu |
| `SkillObjShareBase::ctor()` | 0x36A1E4C |
| `SkillObjAdvFxBehaviour::ctor()` | 0x34211A4 |
| `SkillObjActionAttach::ctor()` | 0x3420CE4 |
| `SkillObjTraceBone::ctor()` | 0x3425CD4 |

⇒ **`SkillObj*` 全部继承 `MonoBehaviour`（§6 已列），只能由 `Object.Instantiate(prefab)` 创建，不可能被 C# `new`。** 用「反向查 ctor」在 Unity 组件上根本不适用 —— 该方法作废。

### 15.23 换向后的正确入口：`KCPGOACDHOB` 才是技能/动作驱动器

已有实测证据链（不新增假设）：

| 证据 | 内容 |
|---|---|
| `Unit+0x38 GAMJINGNDHM : KCPGOACDHOB`（+0x40 `HGMLJJBFG` 同类型第二份） | 单位上的动作/技能驱动器 |
| `KCPGOACDHOB : KPBKFHOKGHJ`，并持有 `UnitSkillInfo BJHHKKKBJFJ @+0x50`（L159538） | **它直接持有技能配置** |
| `KCPGOACDHOB::OCKDCBJFDMB(int index)` 0x34AD634 | 按 `idx*0x120` 从数组取元素 → `bl JENALNPPABB::ENLFIOJBJDD(unit, elemId, &o1, &o2)` → **`new NDJLODOGKNK`**（ctor 0x363C6BC）→ 写时间戳 `+0x10`、memcpy 0x120 字节 |
| `NDJLODOGKNK` 持有 `UnitSkillInfo MGPKHKPDNGM @+0x150`（L178288） | **它就是"一次技能施放"的运行记录对象** |
| `KCPGOACDHOB` 另有 `MCAKCMHPAKK` / `NAPOEBKAGLG` / `EIOGCGMCEEC` 三个同构方法 | 同一套三连写（0x190/0x198/0x1A0 到 NDJLODOGKNK） |

**⇒ 下一步唯一正确目标：`KCPGOACDHOB` 的 4 个方法里，谁在"检查能否施放"**。它们都遵循同一形态（取配置 → 建 NDJLODOGKNK → 填字段），其中**"建之前"的那段判断**就是 CD 门禁。

### 15.24 本轮结论（一句话）

**hook 点/字段都未拿到；方案 A 作废（MonoBehaviour 不走 new）。下轮目标锁定 `KCPGOACDHOB::OCKDCBJFDMB`(0x34AD634) 与 `MCAKCMHPAKK`/`NAPOEBKAGLG`/`EIOGCGMCEEC` 在"new NDJLODOGKNK 之前"的判断段。**
### 15.25 T26 任务 1：**子类 override 假设基本不成立**（只有 1 个 override）

`KCPGOACDHOB` 全部子类（dump 实测）：`NNOIPMHKAEM`(L158425)、`ALAPNALHBLB`(L158458)、`BDGDBBFPDHA : PMFGNOABCAD`(L158491)、`LHLOIGOLAFB`(L158621)、`HGLMLMHMFHE`(L159104)、`BICADNOHCML`(abstract, L159468)，加上 `PMFGNOABCAD`(L159273)。

对 8 个槽方法逐个查各子类是否有同名实现，**结果只有 1 个 override**：

| 方法（slot） | 基类 RVA | 子类 override |
|---|---|---|
| `KCEMGFMOBCB`(15) | 0x34ADC30 | 无 |
| `BJKGHNGILOC`(16) | 0x34ADC78 | 无 |
| `DLHAOAHMNEO`(17) | 0x34ADED4 | 无 |
| `ICNOCENPEFE`(18) | 0x34AE548 | 无 |
| `AEILPNGDHAA`(19) | 0x34A7524 | **`LHLOIGOLAFB::AEILPNGDHAA` 0x34A610C**（L158713） |
| `ELKNFGAJOOF`(20) | 0x34AE9B4 | 无 |
| `HNLMLKAHNAH`(12, float) | 0x34AE480 | 无 |
| `POOEGCLAAEP`(11, bool) | 0x34AC9EC | 无 |

**⇒ 6 个探针 RVA 中 5 个在基类就是唯一实现，虚分发也会走到它们。所以「0 命中」不是子类 override 造成的。**
更可能原因（按可能性）：① hook 的地址与 dump RVA 基准不一致（Dobby 需要 `base + RVA`，若用了文件偏移或漏加 base 就会挂到别处）；② 这 6 个方法只在特定流程走（例如只有真放技能时才调 `DLHAOAHMNEO`），而测试场景没覆盖；③ 单位驱动器实例是 `LHLOIGOLAFB`（唯一 override 者），若它走的就是 `AEILPNGDHAA`，那恰好只有 1 个槽被调。

### 15.26 T26 任务 2：**能量确实走属性系统，冷却部分走**

11 个 id 全段扫描（三族接口 + `mov w1,#imm` 配对），**命中 6 处**：

| id | 名称 | 读取者 | 接口 |
|---|---|---|---|
| **48** | `ENERGY_RECOVER_ADD` | **`JJFBPGNPMKB::IHHHPKIGIBN`** | CGIBCFFNFGL |
| **49** | `ENERGY_RECOVER_RATE` | **`JJFBPGNPMKB::IHHHPKIGIBN`** | CGIBCFFNFGL |
| **51** | `RELIC_SKILL_COOLDOWN_REDUCTION` | `PlayerController::MBEMEGCPKMC` | CGIBCFFNFGL |
| **52** | `DASH_COOLDOWN_REDUCTION` | `PlayerController::CLDHBEFIDGA`；`CharacterTotalAttribPanel::GNDFHMNDGKH`(UI) | CGIBCFFNFGL / FJMHJMEPFMC |
| **57** | `PASSIVE_A_SKILL_COOLDOWN_REDUCTION` | `JJFBPGNPMKB::LNLFKMMPBKM` | CGIBCFFNFGL |
| 11 / 16 / 17 / 36 / 50 / 54 / 55 | 其余 | **0 处** | — |

**⇒ 结论**：
1. **能量走属性系统**：`id=48/49` 被 `JJFBPGNPMKB::IHHHPKIGIBN` 读（能量回复量/回复率）。**写大值可提高回能速度**，这是"无限蓝"的属性面入口。
2. **冷却部分走**：通用 `id=36` **0 读取点**（确认无效），但 **`id=51/52/57` 有真实读取点** ⇒ 我们之前只写 36 所以全无效；写 52（冲刺）/51（遗物）/57（被动A）才有作用。
3. `id=11/16/17/50/54/55` 无读取点。

### 15.27 单位当前能量字段：静态未定位（线索已收窄）

- `UnitSkillInfo.m_EnergyAdd @0x68` / `m_TargetEnergyAdd @0x6C`（L1542xx）= 每次命中给多少能量（配置侧）；
- `UnitHpBar.m_EnergyBar @0x70`（Image）→ 说明单位上有 `fillAmount` 驱动的**能量比例**；
- 具体 float 字段本轮**未定位**（时间盒到点）。下一步：反汇编 `UnitHpBar` 里写 `m_EnergyBar.fillAmount` 的方法，回溯它读的 float 偏移 —— 与当年用 `m_CoolDownText` 反查 CD 同法。
### 15.28 T27：能量回复算式（实测，量纲已定）

`JJFBPGNPMKB::IHHHPKIGIBN(Unit, float, NDJLODOGKNK)` **RVA 0x3641CD0**（L178526）：

~~~asm
; ① id=0x30(48) ENERGY_RECOVER_ADD @0x3641D6C
0x3641D6C  mov  w1, #0x30
0x3641D74  bl   CGIBCFFNFGL            ; w0 = values[48]
0x3641D84  add  w22, w0, w21           ; ★ w22 = values[48] + w21（w21 = 基础回复量）
; ② id=0x31(49) ENERGY_RECOVER_RATE @0x3641E1C
0x3641E1C  mov  w1, #0x31
0x3641E24  bl   CGIBCFFNFGL            ; w0 = values[49]
0x3641E2C  mul  w8, w0, w22            ; rate * base
0x3641E30  movk w9, #0x68DB, lsl #16   ; = 0x68DB8BAD ← 经典 /10000 魔数
0x3641E34  smull x8, w8, w9
0x3641E3C  asr  x8, x8, #0x2C           ; ⇒ w8 = (rate*base)/10000
0x3641E40  add  w8, w8, w9 ; add w8, w8, w22   ; base + (rate*base)/10000
0x3641E4C  scvtf s0, w8
0x3641E50  fmul  s0, s0, s8            ; × 某个 float 系数（s8）
; 另 @0x3642354 id=0x28(40) UNDYING_SPIRIT 也在此函数读，cmp #1 分支
~~~

**⇒ 量纲定死**：`48 = 绝对值加值`，`49 = 万分比（10000 = +100%）`，最终 = `(base + v48) × (1 + v49/10000) × 系数`。

**无限蓝的数据面写法（二选一）**：
- `values[48] = 100000` → 每次回能 tick 直接加十万；
- `values[49] = 10000` → 回复速度 ×2（10000 = +100%），`50000` = ×6。
两者**不需要 hook**，与已实测可用的 AGNK 写入路径一致。

### 15.29 单位「当前能量」字段：仍未定位（本轮进度）

已定位的 UI 侧入口（`UnitHpBar`）：

| 方法 | RVA | 行 | 签名 |
|---|---|---|---|
| `EGLIJCKJPGJ` | **0x3B23C54** | L2793 | `(float)` —— 单 float，最可能是「设置能量比例」 |
| `LIONOOEGJBM` | **0x3B23D28** | L2803 | `(float, float)` —— 当前/上限 双参 |
| `AMHMCENAJOO` | 0x3B206F4 | L2833 | `(float, float)` |
| `JNMAOBCHNDA` / `EGHHHEFBLKJ` | 0x3B2189C / 0x3B21E3C | L2756 / L2750 | `(float, float)` 血条版本 |

字段：`m_EnergyBarContainer(+0x68)`、`m_EnergyBar : Image(+0x70)`、`ENERGY_BAR_MIN_FILL : float(+0x78)`。

**缺口**：`EGLIJCKJPGJ` 的调用者（= 谁把单位能量算成比例传进来）本轮未扫（时间盒到点）。**下一步**：扫 `bl 0x3B23C54` 的调用点，在其调用前一屏找 `ldr s?, [x?,#off]` 的 float 读取 —— 那就是当前能量字段，与「用 `m_CoolDownText` 反查 CD」同一手法。
### 15.30 T28：能量条取值链（实测，当前能量是个**方法**不是裸字段）

`UnitHpBar::EGLIJCKJPGJ(float)` 全包 **0 个调用者**（它可能是旧接口/被 IFix 包装调用）。真正在用的是双参版：

| 调用者 | RVA | 行 | 调用点 |
|---|---|---|---|
| `UnitHpBar::EGODEJNHEHM` | 0x3B24xxx | **L2809** | 0x3B2461C → `bl LIONOOEGJBM(f, f)` |
| `UnitHpBar::IKCOLGCCBHI` | 0x3B25xxx | **L2875** | 0x3B25FA0 → `bl LIONOOEGJBM(f, f)` |

`EGODEJNHEHM` 的取值段（实测）：

~~~asm
0x3B245F4  bl   PHBGLMHPMMC::BLCEAMGIAHH   ; ★ 当前值（int）——参数 x0 = x22（状态对象）
0x3B2460C  bl   Unit::NLEGJEMKCLM          ; ★ 上限值（int）——参数 x0 = x21 指向的对象
0x3B24610  scvtf s0, x20
0x3B24614  scvtf s1, x0
0x3B2461C  bl   UnitHpBar::LIONOOEGJBM     ; (float cur, float max) → 画条
~~~

`IKCOLGCCBHI`（L2875）同构，但上限读 `Unit::AHCJPFPKNNF()`。

### 15.31 结论（可直接写代码）

**当前能量/当前值不是一个裸 float 字段，而是 `PHBGLMHPMMC::BLCEAMGIAHH()`（RVA 0x36A07B4）的返回值（int）**，上限是 `Unit::NLEGJEMKCLM()`（0x354D30C）或 `Unit::AHCJPFPKNNF()`（0x354A578）。

⇒ 两条可实施路径：
1. **读**：`runtime_invoke(PHBGLMHPMMC::BLCEAMGIAHH, stateObj)` 拿当前值（int），配合上限按比例判断；
2. **写**：反汇编 `BLCEAMGIAHH` 本体，找它读的字段与其 setter —— 那才是"当前能量"的存储位置（**本轮未做，需再 5 分钟**）。

> 注：`EGLIJCKJPGJ(float)` 0 调用者这一结果，也解释了为什么之前按"单参设比例"的思路找不到能量字段——**实际走的是双参重载**。
### 15.32 T29：能量取值链已到第 3 跳（`BCIGCKIIODC`），字段本体还差一跳

~~~asm
; ① PHBGLMHPMMC::BLCEAMGIAHH() 0x36A07B4  —— 只是个转发
0x36A07F4  b   PHBGLMHPMMC::CAODLBENOAM
; ② CAODLBENOAM() 0x36A07FC
0x36A0864  ldr x19, [x19, #0x28]        ; ★ state+0x28 → 一个 BCIGCKIIODC 对象
0x36A0888  b   BCIGCKIIODC::JHDJOILDPMG ; ★ 返回值 = 当前值（int）
; ③ 上限 Unit::NLEGJEMKCLM() 0x354D30C
0x354D344  ldr x0, [x19, #0x120]        ; Unit+0x120 状态对象
0x354D350  bl  PHBGLMHPMMC::IFEHBJOMMMB ; → 属性容器
0x354D360  b   OKNDCLIKECF::PMKAGMHPAHB ; ★ 上限 = 六槽 long getter 之一（第 2 个，L183500）
~~~

**⇒ 两条链（实测）**：

| 量 | 链 | 类型 |
|---|---|---|
| **当前值（能量/血量）** | `unit+0x120` →（`IFEHBJOMMMB` 得 state）→ `state+0x28` → `BCIGCKIIODC::JHDJOILDPMG(obj, 0)` | **int** |
| **上限** | `unit+0x120` → `IFEHBJOMMMB` → `OKNDCLIKECF::PMKAGMHPAHB()` | **long**（属性槽值） |

**⇒ 关键结论**：上限**不是字段**，是属性容器第 2 个 long 槽（`OKNDCLIKECF::PMKAGMHPAHB` 0x36A2928，L183500）。这意味着**上限可以通过属性系统改**（写 6 槽之一），而当前值在 `BCIGCKIIODC` 对象里。

**未完成的最后一跳**：`BCIGCKIIODC::JHDJOILDPMG`（RVA 0x36D1F54）本体未反汇编，**那一层才给出"当前值"的真实字段偏移与其 setter**。

### 15.33 给 builder 的现状（可直接用的部分）

| 功能 | 可用方案 | 状态 |
|---|---|---|
| 能量回复速度 | `values[49] = 10000`（+100%）或 `values[48] = 大数` | **量纲已定，可实施** |
| 能量上限 | 容器第 2 个 long 槽（`PMKAGMHPAHB` 对应槽 +0x20 的 long 值） | 读得出，写需再验证 |
| 当前能量写满 | `BCIGCKIIODC` 对象内字段 | **差一跳（0x36D1F54）** |
## 16. 技能/能量：整函数级反汇编（T30）

> 方法：`_fn.py`（新增）按「方法边界表」整函数反汇编到下一个方法起点，标注总字节/指令数；不再用窗口采样。

### 16.1 能量门禁找到了（整函数，128 B / 32 条）

**`ActionButton::NLGNPKOALJP(OIGNPAHJLCC)` RVA 0x3006D98** —— 每个技能按钮各有一个同构方法（50 个 `BLCEAMGIAHH` 调用者里绝大多数是这一类）：

~~~asm
+0x48  cbz  x20, → ret
+0x54  bl   PHBGLMHPMMC::BLCEAMGIAHH        ; w0 = 当前能量（int）
+0x58  ldrsw x8, [x19, #0xEC]                ; ★ x8 = 该技能需要的能量（ActionButton.CCNCMGCLBMB : int @0xEC）
+0x60  strb w9, [x19, #0x70]                 ; LDIGJOHIOKH : bool @0x70
+0x64  cmp  x0, x8                           ; ★ 当前 vs 需要
+0x6C  strb w8, [x19, #0xE8]                 ; ★ JAMLHLEAOMN : bool @0xE8 = 比较结果
~~~

**⇒ 能量门禁 = `BLCEAMGIAHH() >= ActionButton+0xEC`**，结果写进 `+0xE8`。
而 `+0xE8` 正是 `BOBHFNCDAFM`（可点否）里读的那个 bool（§15），**两条链在此闭合**：能量够 → `+0xE8=1` → 按钮可点。

| 字段 | 类型 | 偏移 | 语义（实测） |
|---|---|---|---|
| `ActionButton.CCNCMGCLBMB` | int | **+0xEC** | **该技能需要多少能量** |
| `ActionButton.JAMLHLEAOMN` | bool | **+0xE8** | 能量是否足够 |
| `ActionButton.LDIGJOHIOKH` | bool | +0x70 | 另一关联状态 |

### 16.2 `BCIGCKIIODC::JHDJOILDPMG` 两个重载（整函数）

| 重载 | RVA | 字节/指令 | 关键指令 |
|---|---|---|---|
| `(int)` | **0x36D1F54** | 164 B / 41 | `+0x64 ldr x20,[x20,#0x5A8]`…`+0x94 mul x1, x8, w9`（w9 = `0x678EE5FB`）；`+0x9C b 0x36D1FF8`（尾调用第二重载） |
| `(this, key)` | **0x36D1FF8** | 92 B / 23 | **`+0x48 eor x0, x19, x20` ; `+0x54 ret`** ⇒ 返回值 = **两个操作数异或** |

**⇒ 结论**：`BLCEAMGIAHH()` = `*(state+0x28) → JHDJOILDPMG(obj, key)`，其中 `key = 静态值 × 0x678EE5FB`，最终返回 **`obj ^ key`** —— **值以异或混淆存储**，不是普通字段直读。
这也解释了为什么"直接按偏移读 float"一直找不到能量：**它的读取要经过一次异或解码**。

### 16.3 调用者分布（50 处，全局扫描实测）

| 类 | 方法数 | 说明 |
|---|---|---|
| `ActionButton` | **约 20 个**（`NLGNPKOALJP`/`DCOPBFFIMMM`/`PBDNOFDKGDC`/`ILFBMKCFKPH`/`IICCNCEJKOL`/`EMPKBCNFLMO`/`KNGBOOAOOCO`/`MHBEBPHBGOE`/`GHNLCLBJNDB`/`AFFIIEEICEA`/`NHHGHJNLOHN`/`BDLLBBFNLHE`/`MFABJFJPJCL`/`BDCGFMCEBBK`/`AIOMOFPBOGN`/`MHFMADMDMDN` …） | 每个技能按钮一个：算"能量够不够" |
| `GeneralAIControllerBase::IHPIEIDIOLJ`（0x3505990，1168 B / 292 条） | 1 | AI 侧读能量（+0x374 存 `+0xE0`、+0x3BC 存 `+0xE4`） |
| `UnitHpBar::OPPOHANMFNO`（0x3B2031C，252 B / 63 条） | 1 | 能量条刷新：`+0x64` 与缓存 `+0xAC` 比较，变了才刷新；`+0xC0` 再取 `NLEGJEMKCLM()` 上限 → `AMHMCENAJOO(cur,max)` 0x3B206F4 |

### 16.4 下一步（本轮继续）

1. `BCIGCKIIODC` 全部方法里找 **写**（`str` 到同一字段 / 同名 setter），拿到"设置当前能量"的入口；
2. `state+0x28` 是什么类型、由谁创建（找 `BCIGCKIIODC` ctor 调用点）；
3. Q2/Q3（冷却设置与 CDR 量纲）继续整函数反汇编。
### 16.5 T31-A：按钮层**没有**冷却判定（整类 79 个方法全扫，0 命中）

方法：`ActionButton`（53 个方法）+ `ActionButtonBase`（26 个方法）**逐个整函数反汇编**（`_fn.func`，从入口到下一个方法起点），检索 `fcmp … #0.0`：

| 类 | 方法数 | `fcmp #0.0` 命中 | 带 `strb` 的后续 |
|---|---|---|---|
| `ActionButton` | 53 | **0** | 0 |
| `ActionButtonBase` | 26 | **0** | 0 |

**⇒ 结论（实测，否定式）**：`ActionButton` / `ActionButtonBase` 里**没有任何"读 float 与 0 比较"的代码**，因此：
1. 按钮层**不判断冷却**（只有能量：`+0xEC` 需要量 vs `BLCEAMGIAHH()`，写 `+0xE8`）；
2. `+0xE8` 只由能量判定写，**不是"CD 好了"的开关**；
3. 冷却判定必须在别处（技能系统 / PlayerController / KCPGOACDHOB），不在 UI。

### 16.6 T31-B：`BCIGCKIIODC` **没有写方法**（43 个方法全扫）

类定义（实测）：字段仅 2 个 —— `GBLDNOGAKLO : byte[] @0x0`、`ENPBAFNNJLA : long @0x8`；方法 **43 个**。

对 43 个方法逐个整函数反汇编，检索「同时含 `eor` 与 `str`」：**0 命中**。

**⇒ 结论（实测，否定式）**：
1. `BCIGCKIIODC` 是**字节缓冲/解码器**（`byte[] + long`），不是"能量存储对象"；
2. 类内**没有任何异或写入方法** ⇒ **"每 tick 写回满值"这条路在 `BCIGCKIIODC` 上不成立**；
3. §16.2 里 `JHDJOILDPMG` 的 `eor x0,x19,x20` 是**解码**动作，不是写入。

### 16.7 必须修正的一条（诚实标注）

§16.1 我把 `BLCEAMGIAHH()` 断言为"当前能量"，**证据不足，现降级为待验证**：

| 已证明 | 未证明 |
|---|---|
| 它被 ~20 个技能按钮方法读，并与 `ActionButton+0xEC` 比较，结果写 `+0xE8` | 它的返回值**是不是"当前能量"** |
| 它被 `UnitHpBar::OPPOHANMFNO` 读并用于刷新某个数值条（与缓存 `+0xAC` 比较） | 那个条是能量条还是别的条 |
| 它内部是 `state+0x28`（`BCIGCKIIODC` = byte[]+long 解码器）→ `JHDJOILDPMG(obj, key)`，返回 `obj ^ key` 形态 | 解码出来的是"当前值"还是"配置值" |

**一个极可能的替代解释**：`BLCEAMGIAHH` 读的是**"该技能需要多少能量"的配置**（每技能一个），而 `+0xEC` 可能是它的缓存副本 —— 若如此，则 `cmp` 的两边都是配置，真正的"当前能量"另有其源。**这个分叉必须用一次读数验证，不能靠推理定。**

**验证方法（so 日志，1 次即可）**：同一单位，打印 `BLCEAMGIAHH()` 与 `ActionButton+0xEC`，然后放一次技能，再打印一次：
- 若 `BLCEAMGIAHH()` 施放后**下降** ⇒ 它是当前能量（可写）；
- 若它**恒定不变**、而 `+0xEC` 也不变 ⇒ 它是配置，**当前能量在别处**。

### 16.8 已排除清单（累计，全部有整函数证据）

| 对象 | 结论 |
|---|---|
| `ActionButton` / `ActionButtonBase`（79 方法） | 无 float-零比较 ⇒ 无 CD 判定 |
| `ActionButtonBase::BEHJPFIEHPA` | `return state==0`（UI 态） |
| `ActionButtonBase::EDMIEKLOHDP` | `ldr w0,[x19,#0x4C]; ret` |
| `FFIJIKGDPNO` / `KPAPJHKNAKO` | SetActive / 摇杆方向 |
| `BCIGCKIIODC`（43 方法） | 无 eor+str 写方法，是解码器 |
| 属性 id=36 / 121 | 无读取 / 移动标志 |
### 16.9 T32：**当前能量的真实存储找到了**（`PHBGLMHPMMC+0x28`，异或混淆 long）

**关键修正**：`PHBGLMHPMMC+0x28` 的静态类型是 **`KKDGKNHECIH : long`**（字段表实测，该字段不是对象）⇒ §16.2 里 `eor` 是**异或解码**，`BCIGCKIIODC` 是编解码器（`byte[]+long`），前面"解码器对象"的说法作废。

#### 读写对（整函数实测）

| 角色 | 方法 | RVA | 字节/指令 | 关键指令 |
|---|---|---|---|---|
| **取** | `PHBGLMHPMMC::CAODLBENOAM()` | **0x36A07FC** | 148 B / 37 | `+0x68 ldr x19,[x19,#0x28]` → `+0x9C b BCIGCKIIODC::JHDJOILDPMG(obj=x19, key)` |
| （转发） | `PHBGLMHPMMC::BLCEAMGIAHH()` | 0x36A07B4 | 72 B / 18 | `+0x60 b CAODLBENOAM` |
| **写** | `PHBGLMHPMMC::PCHGGEFIMIE(long)` | **0x36A08F0** | 160 B / 40 | `+0x88 bl BCIGCKIIODC::ACACMPEKGMJ`（编码）→ **`+0x8C str x0,[x19,#0x28]`** |
| 解码 | `BCIGCKIIODC::JHDJOILDPMG(long,key)` | 0x36D1FF8 | 92 B / 23 | `+0x48 eor x0,x19,x20 ; ret` |

`JHDJOILDPMG` 的 key 来源（`CAODLBENOAM` 内 +0x64~+0x94）：静态 `[0x81ED000+0x5A8]` → `+0xB8` → `+0x8` 取值，乘常数 **`0x678EE5FB`**（`mov w9,#0xE5FB ; movk w9,#0x678E,lsl#16`）。

#### ★ 增减+上限钳制（这一步证明它就是"能量"）

**`PHBGLMHPMMC::FJDABAEBLJG(int DLCHNBOMMHN)` RVA 0x36A27E8（204 B / 51 条）**：

~~~asm
+0x4C  bl  PHBGLMHPMMC::CAODLBENOAM      ; cur = 解码(+0x28)
+0x50  add x1, x0, w20, sxtw              ; ★ cur + 参数(int)
+0x58  bl  PHBGLMHPMMC::PCHGGEFIMIE       ; 编码并写回 +0x28
+0x60  bl  PHBGLMHPMMC::OBKEGLFOOFC       ; 取属性容器
+0x6C  bl  PHBGLMHPMMC::CAODLBENOAM       ; 再读（钳制判断）
+0x7C  bl  OKNDCLIKECF::PMKAGMHPAHB       ; ★ 上限（属性槽 long getter）
+0x80  cmp x21, x0 ; +0x84 b.le           ; 未超上限则跳过
+0x98  bl  PHBGLMHPMMC::PCHGGEFIMIE       ; ★ 超了就写回上限
~~~

⇒ **`+0x28` = "当前值（有上限、可增减）"资源，上限来自属性容器 `PMKAGMHPAHB`（第 2 个 long 槽）。结合 `ActionButton+0xEC` 的比较，这就是技能消耗的**当前能量**。**（此结论有增减+钳制+按钮比较三重证据，不再是单点推断）

#### 无限蓝：两种写法（都不需要 hook）

| 写法 | 调用 | 效果 |
|---|---|---|
| **A（推荐）** | 每 tick 调 `PHBGLMHPMMC::FJDABAEBLJG(大数)`（如 10000） | 内部 `cur += delta` 后**自动钳制到上限**，等价"回满" |
| B | 调 `PHBGLMHPMMC::PCHGGEFIMIE(明文long)` 传大数 | 直接写值（**注意：此函数不钳制**，超上限会溢出显示） |

**A 更安全**：它的钳制逻辑由游戏自己保证，不会破坏 UI/判定。参数是 **int（32 位）**，`delta` 传 10000 足够。

#### setter 的调用者（谁在扣能量）

`PCHGGEFIMIE` 全包 **6 处**调用：`FFIGCMIJNEF::NCKECKGPNKN`(Lint 大签名的初始化，@0x34E8C44) / `FFIGCMIJNEF::ILMJKMDEHEJ`(@0x34E9604) / `FFIGCMIJNEF::MLCFCFMFKMF`(@0x34E9D6C) / `PHBGLMHPMMC::FJDABAEBLJG`(×2, @0x36A2840/0x36A2880) / `PHBGLMHPMMC::AEJHOFLHIEH`(@0x36A412C，反序列化整块状态)。
**⇒ 扣能量的通路 = `FJDABAEBLJG(负数)`**（`FFIGCMIJNEF` 那几个是初始化/读档，不是扣减）。
### 16.10 T32-A：驱动器时间戳字段锁定 `KCPGOACDHOB+0x98`（int 毫秒）

| 方法 | RVA | 字节/指令 | 关键指令（实测） |
|---|---|---|---|
| `IEALBAMGPGJ()` | **0x349F850** | 192 / 48 | `+0xA8 bl HClock::NMHDPNKKNBP`（取当前时间 ms）→ **`+0xAC str w0,[x19,#0x98]`** |
| `OCAAPNNMGAE()` | **0x349FC7C** | 272 / 68 | `+0xEC bl NMHDPNKKNBP` → `+0xF4 mov w1,w0` → `+0x108 b 0x3626F04`（把时间传进动作记录） |
| `BJEHIDKAJDB(Unit)` | **0x34A26B4** | 1108 / 277 | `+0x240 bl NMHDPNKKNBP` → `+0x258 bl IMIKKGMNJKD::CLCOGBBBBFH(x21, time, x20)` |
| `POOEGCLAAEP()` | **0x34AC9EC** | 72 / 18 | **`+0x38 ldrb w0,[x19,#0x73] ; ret`** —— 纯 bool getter（槽 11 不是时间判定） |

另：`IEALBAMGPGJ` 里 `+0x88 bl KPBKFHOKGHJ::IEALBAMGPGJ`（基类同名，先走父逻辑）；`OCAAPNNMGAE`/`BJEHIDKAJDB` 都先 `bl Unit::DFNAOICKOJP` 取技能持有者。

**⇒ 驱动器侧"动作开始时间"= `KCPGOACDHOB+0x98`（int，毫秒，来自 `HClock::NMHDPNKKNBP`）。**

**未定位**：读 `+0x98` 并与当前时间比较的那段判断（真正的"CD 未好"分支）。**下一步明确**：扫「同一函数内既 `ldr w?,[x?,#0x98]` 又 `bl 0x32A47CC(NMHDPNKKNBP)`」的方法 —— 目标很窄，10 分钟内可出。
### 16.11 T33：**CD 门禁找到了** —— `LHLOIGOLAFB::HGCKACANGHC`（0x34A40C4，204 B / 51 条，整函数）

这是 `KCPGOACDHOB` 家族里唯一的 override 版（`LHLOIGOLAFB : KCPGOACDHOB`），也是"能否开始下一个动作"的分支：

~~~asm
+0x60  ldr  w8, [x19, #0xBC]        ; ★ w8 = 当前动作的冷却时长（int，毫秒）
+0x64  cmp  w8, #1
+0x68  b.lt → 0x34A417C             ; cd<=0 ⇒ 直接放行
+0x88  bl   HClock::NMHDPNKKNBP     ; w0 = now（毫秒）
+0x8C  ldr  w8, [x19, #0xBC]        ; cd
+0x90  ldr  w9, [x19, #0x98]        ; ★ 上次动作时间（int 毫秒，由 IEALBAMGPGJ +0xAC 写入）
+0x94  add  w8, w9, w8              ; lastTime + cd
+0x98  cmp  w0, w8
+0x9C  b.lt → 0x34A417C             ; ★★ now < last+cd ⇒ 跳过（这就是"CD 未好"）
+0xA0  ldr  x8,[x19] ; ldr x9,[x8,#0x408] ; blr x9   ; 允许 → 虚调用（开始动作）
+0xB4  str  wzr, [x19, #0xBC]       ; ★ 放行后把 cd 清零
+0xC4  b    0x34A4190               ; 转基类 KCPGOACDHOB::HGCKACANGHC（1869 条）
~~~

**⇒ 冷却语义（实测，常数齐全，无推断）**：

| 字段 | 类型 | 偏移 | 语义 |
|---|---|---|---|
| `KCPGOACDHOB+0x98` | int | **0x98** | **上次动作开始时间**（`IEALBAMGPGJ` +0xAC 由 `HClock::NMHDPNKKNBP()` 写入） |
| `KCPGOACDHOB+0xBC` | int | **0xBC** | **该动作的冷却时长（毫秒）** |

判定式：**`if (now - lastTime < cd) return;`**（严格实现为 `if (now < lastTime + cd) skip`）。

**⇒ 无 CD 两条落地方式（都不需要猜）**：

| 方案 | 做法 | 说明 |
|---|---|---|
| **数据面（推荐）** | 每 tick 写 `driver+0xBC = 0` | `cd<=0` 时 `+0x68 b.lt` 直接放行，**完全绕过时间比较** |
| hook | `LHLOIGOLAFB::HGCKACANGHC` **RVA 0x34A40C4**，在 `+0x9C` 处把 `b.lt` 改成 `nop`（或强制 `b` 到 +0xA0） | 一处 patch 覆盖所有该子类实例 |

`driver` = `Unit+0x38`（`GAMJINGNDHM : KCPGOACDHOB`，§6 已列）或 `Unit+0x40`（第二份）。

> 注：基类 `KCPGOACDHOB::HGCKACANGHC`（**0x34A4190**，1869 条）里也有时间逻辑（+0x280 起 `sub w20, w0, [x19,#0x90]`、`+0x2B0 str w0,[x19,#0x90]`），那是**帧间隔/动画推进**，与 CD 门禁不是同一处；CD 门禁只在 override 版里。
### 16.12 T34：`Unit+0x38` = 主驱动器（setter 明确），`+0x40` = 被清空的备用槽

#### 写入点（整函数实测）

**`Unit::NAIHAANJGAM()` RVA 0x354E008（92 B）**：

~~~asm
+0x0   bl   PMFGNOABCAD::ctor              ; new 一个新驱动器
+0x14  bl   PMFGNOABCAD::GHPDCADPAEF       ; 初始化(new, x23)
+0x1C  str  xzr, [x0, #0x40]!              ; ★★ 把 Unit+0x40 清零（pre-index 写）
+0x40  b    Unit::LOHEEJLHBBA(KPBKFHOKGHJ) ; ★★ 新驱动器交给 setter（写 +0x38）
~~~

**`Unit::AJALFOOHBHA()` RVA 0x3550700（268 B）**：`bl BDGDBBFPDHA::ctor` → `Unit::FONCPJDGOEF()` 判 `== 10` → **`ldr x0,[x19,#0x38]`** → `PMFGNOABCAD::BALDOLGHIEA`。⇒ 动作派发读的是 **+0x38**。

| 字段 | 写入者 | 语义 |
|---|---|---|
| **`Unit+0x38`** | `Unit::LOHEEJLHBBA(KPBKFHOKGHJ)` **RVA 0x354D850**（L167006，参数类型就是驱动器基类） | **当前驱动（主）** |
| **`Unit+0x40`** | `str xzr,[x0,#0x40]!`（`Unit::NAIHAANJGAM` +0x1C） | **换驱动时被清零**，属备用/缓存槽 |

**⇒ 结论：真驱动是 `unit+0x38`。** `+0x40` 在重建驱动时被显式置 0，因此 v1.9.7 里看到"换成一个 last 全 0 的对象"正是**旧的 `+0x40` 槽位**；v1.9.8 两份都写是安全的冗余，但**只需要写 +0x38**（真正被派发的是它）。

#### 驱动器子类族（ctor 里 `+0x10` 存类型 id，实测）

| 类 | 类型 id（`str w8,[x19,#0x10]`） | ctor RVA |
|---|---|---|
| `ALAPNALHBLB` | 3 | 0x34A0480 处调用 |
| `BDGDBBFPDHA` | 4（另 `+0x2D=1`） | 0x34A1FAC |
| `LHLOIGOLAFB` | **5** | 0x34A236C |
| `MHMHILEHEIA` | 5 | 0x34AAD18 处调用 |
| `HGLMLMHMFHE` | 6 | 0x34AA0CC |
| `NNOIPMHKAEM` | 9 | 0x349FD8C |
| `PMFGNOABCAD` | 4 | 0x34A1FD0 |
| `MFBKINANPFC` / `BICADNOHCML` / `MCANGKDJMKB` | 虚调用取得 | 0x34AC0E0 / 0x34AC10C / 0x34AC640 处调用 |

另：`IEALBAMGPGJ`（写 `+0x98` 上次动作时间）**也有 override** —— `NNOIPMHKAEM::IEALBAMGPGJ` 与 `LHLOIGOLAFB::IEALBAMGPGJ`（0x34A6B9C 处，`+0x130 fcmp s8,#0.0` 判某 float >0）。**修正 §16.9 的说法：`IEALBAMGPGJ` 不是"5 个槽都无 override"，它有两处 override。**
### 16.13 T35：冷却圈公式 + **门禁对象与 `unit+0x38` 不是同一批**（关键解释）

#### 冷却圈 fillAmount（整函数 820 B / 205 条）

`ActionButton::NLGCHDPEPOF(int a, int b)` **RVA 0x300A5EC**：

~~~asm
+0xEC  scvtf s0, w20        ; a
+0xF0  scvtf s1, w21        ; b
+0xF8  fdiv  s0, s0, s1     ; ★ fillAmount = a / b
+0x100 fmul  s2, s0, s1
+0x1A4 ldr   x0, [x19, #0x90]   ; m_CoolDownLayer : Image
+0x208 scvtf s0, w20 ; +0x20C scvtf s1, w21 ; +0x210 fdiv s0, s0, s1   ; 第二处同式（+0xA8 层）
~~~

**⇒ fillAmount = `a / b`（纯比例，无加减常数）** —— 冷却圈显示的是"剩余比例"。

**`NLGCHDPEPOF` 全包 0 个 `bl` 调用者**（实测扫描）⇒ 它是**虚方法/委托调用**，与 `HGCKACANGHC` 同类问题：**静态 `bl` 扫不到不代表没人调**。

#### ★ 现场日志的正确解释

~~~
gate  arg0 = 0x70a220f5ab60   cd(+0xBC)=60  last(+0x98)=1156
drv   unit+0x38 = 0x70a136cd84e0  iBC=0
~~~

两个地址不同 ⇒ **门禁跑在另一个 `KCPGOACDHOB` 实例上**。这不是 bug，是设计：驱动器的创建点有很多，实测（`bl <ctor>` 全扫 69 处）：

| 创建者 | new 的驱动类 |
|---|---|
| `PlayerController::{OCMFOMKOPNI, MBEMEGCPKMC, POGJOIGKOPN, BCGACFMAKPE, CLDHBEFIDGA}` | **各 new 一对 `NNOIPMHKAEM` + `HGLMLMHMFHE`**（每套技能/动作一套驱动器） |
| `Unit::{NAIHAANJGAM, OGKLLMPPFGE}` | `PMFGNOABCAD` |
| `Unit::AJALFOOHBHA` | `BDGDBBFPDHA` |
| `UnitNetworkTracker::{NGHGGCPOBPM, BPNIGOFMMDL, PFAKDBHHKHO, FMEDOOMJAJM, NOIDBDOGLBD}` | `HGLMLMHMFHE` / `NNOIPMHKAEM` / `LHLOIGOLAFB` |
| `AIControllerBase::MOPDKPPMCPD` / `BossAIController04*` | `LHLOIGOLAFB` |

⇒ **`unit+0x38` 只是"单位主驱动"，玩家技能施放用的是 `PlayerController` 自己 new 的那套。写 `unit+0x38+0xBC` 当然无效。**

#### 最省事的正确做法（利用已有 hook）

门禁 hook 已经拿到 `arg0`（日志里 `btn=0x70a220f5ab60` 就是它）⇒ **在 hook 里直接把 `arg0+0xBC` 写 0 再返回 true**：

~~~c
bool hgc_hook(void* self) {
    if (self) *(int32_t*)((char*)self + 0xBC) = 0;   // 冷却时长归零
    *(int32_t*)((char*)self + 0x98) = 0;             // 上次时间也归零（双保险）
    return true;                                     // 恒放行
}
~~~

**不需要知道实例从哪来** —— 门禁自己把实例送到我们手上，这是所有方案里最稳的。

#### 仍未定位（下一轮可做）
- 递减 `+0xBC`/推进计时的函数（用于"把 remain 写 0"的替代方案）；
- `NLGCHDPEPOF` 的调用者（虚/委托，需按 vtable 或 `blr` 反查）。
### 16.14 T36：**CDR 属性不参与任何冷却算式**（否定式，整函数算术全列）

#### `PlayerController::MBEMEGCPKMC`（RVA 0x3543D34，3236 B / **809 条**）

全部算术指令（无遗漏，逐条列出）：

| 偏移 | 指令 | 归属 |
|---|---|---|
| +0x1E8 | `bl CGIBCFFNFGL`（**w1=0x33=51** RELIC_SKILL_CDR） | 读属性 |
| +0x280~0x290 / +0x2F4~0x304 / +0x3E8~0x3F8 / +0x9E8~0x9F8 | `fsub s,s` ×2 → `fmul s,s,s` ×2 → `fadd` | **两点距离平方**（无 CDR 参与） |
| +0x578 | `bl CGIBCFFNFGL`（w1=0x73=115） | 读属性 |
| +0x7B8 | `bl CGIBCFFNFGL`（w1=-0x16） | 读属性 |
| +0xBFC | `bl CGIBCFFNFGL`（w1=-0x9） | 读属性 |

**关键**：整函数 **没有一条 `mul`/`smull`/`sdiv`/`udiv`/`madd`**，也没有任何浮点乘除作用在属性值上。id=51 的返回值只被 `+0x1F8 cbz w0, →0x3543F48` 当**布尔分支**用。

#### `PlayerController::CLDHBEFIDGA`（RVA 0x3547BF0，3232 B / **808 条**）

同构：`+0x1E8 bl CGIBCFFNFGL(w1=0x34=52 DASH_CDR)` → `+0x204 cbz w21`；其余 `+0x284/0x288/0x2F8/0x2FC/0x3EC/0x3F0/0x9E4/0x9E8` 全是 `fmul s,s,s`（距离平方）；另读 id `0x40(64)`、`0x57(87)`、`1`。**同样零乘除作用在属性值上**。

**⇒ 结论（实测，非推断）**：`RELIC_SKILL_CDR(51)` / `DASH_CDR(52)` 在这两个函数里**只是布尔开关**，**不参与冷却时长计算**。结合 §15.26（id=36 零读取），**冷却缩减属性在本作没有任何算式实现** —— 这就是用户"写了 CDR 也没效果"的根本原因。

#### 顺带：`PlayerController` 没有存驱动器的字段（实测字段表）

`PlayerController` 全部字段里**没有任何 `KCPGOACDHOB` / `KPBKFHOKGHJ` / `NNOIPMHKAEM` / `HGLMLMHMFHE` 类型**。
`new` 出来的驱动器是**喂给 `+0x80` 那个对象**的：
~~~asm
; OCMFOMKOPNI
+0x0   bl NNOIPMHKAEM::ctor
+0x4   ldr x0,[x19,#0x80]      ; ★ +0x80 的持有者接收驱动器
+0xC   ldr x21,[x19,#0x20]
+0x10  bl 0x354331C
~~~
⇒ 想"遍历所有驱动器"不能从 PlayerController 找，要从 `PlayerController+0x80` 的对象（`GCHKDAIABGF` 族）找。

#### 冷却计时器仍然未定位（本轮结论）

已排除作为"技能 CD 限速点"：`HGCKACANGHC`（v1.9.10 实测清零 `+0xBC` 后技能 CD 照旧）、属性 36/51/52/57 的算式路径。
**下一步唯一确定性做法**（1 次运行即可，无需猜）：在 `CGIBCFFNFGL` 入口打「id 计数表」，放一次技能前后各 dump 一次 —— 哪个 id 在施放瞬间被读、且值变化，那个 id 就是真正的冷却来源。静态这边已证 CDR 无算术，再猜下去只会浪费轮次。
### 16.15 T37：真技能容器是 **`Unit+0x128`（`EKFNKDIIKEB : BKMDLBHDGNC`）**，不是 `PlayerController+0x80`

#### 1) `GCHKDAIABGF`（`PlayerController+0x80`）整类实测：**不是技能容器**

| 项 | 实测 |
|---|---|
| 字段（5 个） | `HEFKHKFKMEK : PKFLGAEJICG @0x10`、`NMBBFFCEFBL : Vector2 @0x14`、`GNIIFOENDFK : Vector2 @0x1C`、`PCCIFCNGIPO : Vector2 @0x24`、`DJIPPCLMDEN : PKFLGAEJICG @0x2C` |
| 方法（33 个） | **30 个是 72 B / 18 条的 getter**；4 个 200 B 的 `(Vector2, PKFLGAEJICG)` 设置器；ctor 仅 2 条指令 |

**⇒ 它是一个"瞄准/朝向小工具"（两个枚举 + 三个 Vector2），没有任何时间戳、数组、层数字段。**
`OCMFOMKOPNI` 里 `ldr x0,[x19,#0x80] ; bl GCHKDAIABGF::AACPHNCNCOC` 只是**取一个 int 作为槽位参数**，`new` 出来的驱动器并不存进它。

#### 2) 驱动器的真实去向（实测）

~~~asm
; PlayerController::MBEMEGCPKMC / OCMFOMKOPNI
+0x6D8  bl   NNOIPMHKAEM::ctor / HGLMLMHMFHE::ctor
+0x6DC  ldr  x0, [x19, #0x80]
+0x6E8  bl   GCHKDAIABGF::AACPHNCNCOC     ; 取槽位 id
+0x6FC  bl   IAGGDOCLNND::OIKKANJOLNC      ; ★★ 以 (unit, id, flag) 注册/施放
; 另一分支
+0xC70  bl   KCPGOACDHOB::JPLKLPIPEMN      ; 驱动器挂到动作系统
+0xC98  b    0x3594C44
~~~

#### 3) ★ 真正的技能容器与判定入口

| 项 | 实测 |
|---|---|
| 容器字段 | **`Unit+0x128`**，静态类型 `EKFNKDIIKEB : BKMDLBHDGNC`（抽象类，**0 字段 / 43 方法**，纯虚分发） |
| 取容器 | `Unit::DFNAOICKOJP()` **RVA 0x354A530** = `ldr x0,[x19,#0x128] ; ret` |

**`IAGGDOCLNND`（静态工具类，0 字段 / 5 方法）** —— 技能槽三个入口，全部「取容器 → 类型检查 → 取 now → 虚调用容器方法」：

| 方法 | RVA | 字节/指令 | 虚槽 | 传参 |
|---|---|---|---|---|
| **`OIKKANJOLNC(Unit, PKFLGAEJICG)`** | **0x35546B0** | 308 / 77 | `[x8,#0x2D8]` / `[x8,#0x2E0]` | `+0x100 bl HClock::NMHDPNKKNBP` → now 传入 |
| **`FEJHANBJNCK(Unit, PKFLGAEJICG)`** | **0x35547E4** | 304 / 76 | `[x8,#0x328]` / `[x8,#0x330]` | 同上，先取 now |
| `FGJJINHANHF(Unit, PKFLGAEJICG)` | 0x3554914 | 264 / 66 | `[x8,#0x928]` | 转 `PFNDCCBPLGI::FGJJINHANHF` |
| `HFKNKIDMDMI(Unit, int)` | 0x35544F8 | — | — | 另一个查询 |
| `GNBFBNNIIAL(Unit, float, …)` | 0x355414C | — | — | 另一个入口 |

~~~asm
; IAGGDOCLNND::OIKKANJOLNC 关键段
+0x90  bl  Unit::DFNAOICKOJP          ; 取 unit+0x128 容器
+0x94  cbz x0 → 失败
+0xA0–0xCC 类型检查（比较 [x8,#0x130] 类层级）
+0x100 bl  HClock::NMHDPNKKNBP        ; ★ now（毫秒）
+0x114 ldr x5,[x8,#0x2D8] ; ldr x4,[x8,#0x2E0] ; blr x5   ; ★ 容器虚方法(now, id, flag)
~~~

#### 4) 交付（可实施）

| 方案 | 做法 | 说明 |
|---|---|---|
| **hook（推荐）** | `IAGGDOCLNND::OIKKANJOLNC` **RVA 0x35546B0 恒返回 true**；同时 `FEJHANBJNCK` **0x35547E4 恒返回 true** | 这两个是"技能槽可用/可施放"的**唯一入口**，`PlayerController` 直接调它们；返回 true 即放行 |
| 数据面 | `container = *(void**)(unit+0x128)`，容器内部时间戳数组 | **未定位**（`BKMDLBHDGNC` 是抽象类 0 字段，实现在子类，需再一轮找 override 的字段） |

**为什么这两个是正确目标（而非之前的 `HGCKACANGHC`）**：
1. 它们由 `PlayerController` 在技能施放流程里直接调用（`MBEMEGCPKMC` / `OCMFOMKOPNI` 都有 `bl IAGGDOCLNND::OIKKANJOLNC`）；
2. 它们显式取 `HClock::NMHDPNKKNBP()` 的 now 并传给容器 ⇒ **这里才有时间比较**；
3. 容器是 `Unit+0x128`（每单位一个技能容器），不是驱动器（每动作一个）—— 与"每技能独立 CD"的预期一致。
