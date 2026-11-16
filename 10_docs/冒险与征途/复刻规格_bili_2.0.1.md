# 冒险与征途 B 站 2.0.1 复刻规格表（4 功能 · 规格卡 + 新版必验清单）

> 目标：在 `com.my.mxyzt.bilibili` 2.0.1 / vc201 / arm64-v8a / Android 16 上复刻 TapTap 渠道 1.5.9 的四项功能。
> 本文**只写规格，不含功能代码**；旧版静态偏移一律只作「核对参考」，新版的偏移/IL 位置必须重新确认。
> 证据分级：**[实测]**=1.5.9 真机日志/验收卡；**[文档]**=离线 IL/元数据/斌哥抓点；**[推断]**=有证据链未验证；**[待验证]**=必须在新版确认。
> 源材料：`mod菜单/starry-mod-menu/app/src/main/jni/Games/MxytzMod.cpp`(2681 行)、`MxytzBattle.cpp`(816 行)、`MxytzBattle.h`/`MxytzMod.h`/`MxytzEntry.cpp`；
> `10_docs/冒险与征途/_钩子目标与语义.md`、`技能先例.md`、`验收卡_v44_20261003.md`、`硬约束.md`、`斌哥包抓点_20261002.md`。

---

## 0. 全局前提（复刻前必须先成立）

| 项 | 事实/要求 | 等级 |
|---|---|---|
| 包名 / 版本 | `com.my.mxyzt.bilibili` 2.0.1 / versionCode 201 / arm64-v8a / Android 16（root 可用） | [实测]（已装真机） |
| 底包 | `02_apks/冒险与征途/冒险与征途_bili_2.0.1_原版.apk`（881 MB） | [实测]（文件存在） |
| 引擎路线 | Unity IL2CPP 2022.3.14f1 + HybridCLR 热更**解释执行**（1.5.9 判定）；2.0.1 **必须重判**（见 §5·G） | [文档] → [待验证] |
| **game_id** | **必须新开，禁止复用 `mxyzt`**；`.tag` / `.authGameId` / `featureList` 菜单模板 / 后台 `games.game_id` 四处一致 | 规范（AGENTS §4） |
| 旧 dump 不可外推 | `06_dumped/冒险与征途/aot/_dll_sha256.txt` = `7fa836ee…`（1.5.9 TapTap 的 Assembly-CSharp.dll），不是 2.0.1 | [实测] |
| 禁改文件 | `Main.cpp` / `MainModern.cpp` / `GameConfig.h` / 公共 `Android.mk` / `SignSpoof` 系 | 规范 |
| 本阶段边界 | 只做静态分析；真机只允许只读 `adb shell`；不装机、不清数据、不 git 提交 | 规范 |

**版本敏感性**（1.5.9 结论不可直接搬运的三件事）：① `libil2cpp.so` 的 RVA（0x207F798 / 0x207DCCC / 0x207F04C 全是 so 指纹绑定的）；② 静态字段偏移；③ **渠道 SDK 适配器类名（B 站渠道极可能不是 `MiaoYueSDKProxy`）**。

---

## 1. 公共挂点与参数约定（四项共用，先看这一节再看规格卡）

### 1.1 为什么只能挂两个点

热更方法的 `methodPointer` 全部指向解释器共享桩（1.5.9 实测 `PREFLIGHT Hurt5 fn=0x40002527f04c code=0 static=1`）⇒ **逐方法 DobbyHook 在本作物理上不成立** [实测]。因此只能改「解释器入口」与「建帧函数」两个点，再按「类::方法」名分发。

| 挂点 | 1.5.9 RVA | 原型 | 能看到什么 | 能力/限制 |
|---|---|---|---|---|
| 解释器入口（AOT 侧） | `0x207F798` | `(MethodInfo* a0, void* a1, void* a2)` | 从 AOT 桩群进来的调用 | 可改实参槽；**看不到纯托管→托管**（战斗核心一次都不从这里进） |
| 建帧 / pushframe | `0x207DCCC` | `(ctx, MethodInfo* mi, args)` | 托管→托管建帧全覆盖 | 战斗三功能**唯一可见通道**；**无法改写返回值**（只能改实参/字段） |

依据 [文档]：`bl 0x207DCCC` 全库 16 个调用点、**16/16 落在解释器体内**（AOT 桩群 0 个）⇒ `Hurt`/`Damage`/`DoAttack` 这类「解释调解释」只走 pushframe。

### 1.2 参数约定（写错即毁帧）

- `a1` = **实参值内联的 8 字节槽区**首地址（值在槽里，不是指针数组！v29 按指针数组读 ⇒ 免广告一次都没写成功）[实测]。
- 第 i 个 IL 实参槽地址 = `a1 + 8*i`；实例方法 `this` 占槽 0。
- **`argBase = (method_flags(a0, nullptr) & 0x10) ? 0 : 1**（0x10 = static）。首字节先读/写原值、只写 1 字节（bool）或按字段宽度写，不要整槽写 8 字节覆盖相邻实参。
- `il2cpp_method_get_flags` 是**双参** API，第二参必须显式传 `nullptr`（单参调用会写只读页 → `SIGSEGV_ACCERR`）[实测]。
- 入口热路径**碰 a1 之前必须自证**：`IsAligned(a0) && InUserVa(a0)`，否则直接透传（v26g 冒烟事故）[实测]。

### 1.3 分发判据（名字 + 类 双键）

- 键 = `MethodInfo+0x18`（方法名字符串指针）+ `MethodInfo+0x20`（类指针）。**只比名字一定错**：il2cpp 方法名字符串是共享池，`EventDelegate::Execute` 与 `ActionCommand_AddProjectile::Execute` 是同一个指针（v39 毒化 bug 根因）[实测]。
- 结构：256 槽开放寻址表 + 首字节位图快筛；**学习期登记一次，之后热路径纯指针比较，无字符串操作**；表满/预算耗尽/窗口到期即关；每 30s「已开功能仍有目标未登记 ⇒ 重开 60s 窗口」补学。
- 目标名册（1.5.9 实际 14 条，`kKinds[]`）：

| 位 | 类 | 方法 | 用途 | 新版必验 |
|---|---|---|---|---|
| `K_HURT` | CBattleActorUtil | Hurt | #1 | 必 |
| `K_DOATTACK` | StarsCollider | DoAttack | #2 | 必 |
| `K_DAMAGE` | Character | Damage | #2 第二层（**1.5.9 只取证不写**） | 必（存在性） |
| `K_LOCALDMG` | BattleElementCenter | get_localDamage | #3（**只取证**，v41 停用写入） | 必（存在性） |
| `K_PROJ` | ActionCommand_AddProjectile | AddProjectile | #3（**只观测**，它体内不读 `_count`） | 必 |
| `K_PROJEXEC` | ActionCommand_AddProjectile | Execute | #3 真落点 | 必 |
| `K_PLAYAD` | MiaoYueSDKProxy | PlayADVideo | #4 代回调入口 | 必（**类名可能变**） |
| `K_ADRESULT` | DS_Advertising | OnADVideoResult | #4 强制 ret=true | 必 |
| `K_ADREWARD` | DS_Advertising | DispatchResult | #4 强制 ok=true | 必 |
| `K_GETADRSP` | DS_Advertising | OnGetADInfoRsp | #4 主落点（result=2） | 必 |
| `K_SENDEVENT` | ProtocolManager | sendEvent | #4 实测可见的那条路 | 必 |
| `K_CANCLAIM` | AdMascotManager | CanClaimReward | #4 悬浮广告（只取证） | 必（存在性） |
| `K_DMGMUL` | （已删） | set_damage* | — | 禁止复活（会填满学习表 ⇒ 静默失效） |

### 1.4 装配门槛与重入闸

- 懒装：首次开功能后 ≥1s **且** 进程启动 ≥12s 才装 Dobby（冷启恢复窗口）；幂等（防双 Dobby 同址 SIGSEGV）。
- 库基址：不能取「off==0 且地址最小」，要用「最大 rw-p」判据（v45：旧判据命中只读影子副本 ⇒ 四项全灭）。
- 重入闸：`t_modInvoke`（mod 自己发起的 `runtime_invoke` 期间不派发）+ 解释器深度上限；`orig` 必调（唯一例外=#2 的「命中作废」是靠**改写实参**让原函数自己 return，不是不调 orig）。

---

## 2. 规格卡

### 卡 #1 · 秒杀小怪（feat **1**，`MXYTZ_FEAT_KILL_MOB`）

**挂点**
- hook **pushframe**（`0x207DCCC`）→ 命中 `ActionGameLibrary.CBattleActorUtil::Hurt` 帧。判据 = 名字字符串 `"Hurt"` + 类指针 == `CBattleActorUtil` 的 klass（**双键**，1.5.9 argc=5、**静态** ⇒ `argBase=0`）[文档+实测]。
- 槽位：IL 第 1 参（`ref CtagHurtHPPara`）在 `a1 + 8*argBase`；该槽内容是 **`CtagHurtHPPara` 的引用槽**，需自证深度：
  - 深度 1 = 槽里直接是对象；深度 2 = 槽里是「指向对象的变量」。判定靠**类名含 `HurtHPPara`**，一次定案后缓存（`g_paraDepth`）。

**语义**
- 布局（**按元数据名运行时解析**，偏移 ≤0x400 才采用）：`m_pTarget`（被打者）、`m_pAttacker`（攻击者）、`m_llValue`（`BigInt`，本轮要写的就是它）。
- `BigInt` = `doubles mantissa`(+0) + `int32 exponent`(+8)，值 = `mantissa × 10^exponent`。**乘区，不是置空**：`mantissa *= 100`（`kKillMul=100`），`mantissa > 1e9` 时 `/=10; exponent += 1` 归一；写 8 字节 double + 4 字节 int32。
- 为什么改这里有效 [文档]：`m_llValue` 经 `CHitNotifier::AddOneHitHurt(para.GetValue())` 进命中通知包 → `EffectToCast → Character::Damage` 落地 ⇒ 飘字/掉血/死亡判定**三者同源**，不会「数字变了血没掉」；而 `HurtHp` 自带钳制（最多打到 hp-1）**永不致死**，所以不能走它。
- 门闸顺序（任一不过 ⇒ 不写）：
  1. `IsPlayerSide(target)` 为真 ⇒ 跳过（**修「人物先死、boss 后死」**）；
  2. 攻击者必须是玩家侧：`IsPlayerSide(attacker)` **或** `characterType == 0`（PLAYER）；两者都不成立 ⇒ 不写；
  3. `characterType(target) == 3`（BOSS）⇒ 绝不写（菜单名是「秒杀**小怪**」）；
  4. `characterType(target) != 2`（MONSTER）时退回阵营判据 `Hitter._camp == 1`（敌方）；`ctype` 读不到且 camp 也读不到 ⇒ 不写；
  5. 值合理性：`isfinite(mant) && 0 ≤ mant ≤ 1e9 && -40 ≤ exp ≤ 60`，否则不写。

**身份判据（#2/#3 共用同一套）**
- 400ms 心跳：`BattleElementCenter.gameObjectManager`（静态字段）→ `GameObjectManager._player` ⇒ 缓存三量：玩家对象指针、`player._hitter`、`player._characterData.ulRoleId`（`_characterData` 候选名 `characterData/_cdata/cdata`；`ulRoleId` 候选 `roleId`）。
- `IsPlayerSide(obj)` = 指针相等 ‖ `roleId` 相等（双方非 0）‖ `_hitter` 相等；**三条全不命中 = 非玩家侧（fail-closed）**。
- 为什么不用缓存类名/指针表：`_player`/`_hitter` 每场战斗换新对象（v41 `INV cancel=0` 真因）⇒ 必须每拍刷新。

**约束与坑**
- 倍率上限：×100 是 anti-ban 安全区（1.5.9 曾用 ×1000 且把指数硬抬 ≥12 = 1e12 量级，被判定高危后回退）。该数字会随命中通知包**到服务端**，加倍数前先看结算回包。
- 池化对象：`CtagHurtHPPara` 是池化的，写入是「逐笔一次性乘区」，**不需要还原**；关功能 = 停止写入，不残留乘区（对应 m03027「全关后伤害还是高」的要求）。
- 热路径禁 I/O：无日志（首 20 次 + 每 200 次一条）、无 maps 查询、无字符串比较、无容器遍历。
- **MI 槽（`*MethodInfo`）永远不许写**（v6 实测 `Scudo ERROR: corrupted chunk header` → SIGABRT）。
- 与 #3 不得共用判据/计数（m02212「各自分开为战」）。

**新版验证清单（#1，10 条）**
- [ ] `CBattleActorUtil` 类存在；`Hurt` 存在、argc=5、**静态**（`method_flags & 0x10 ≠ 0` ⇒ `argBase=0`）
- [ ] `Hurt` 帧确实从 **pushframe** 可见（学习期日志 `LEARN ok Hurt5`）
- [ ] `CtagHurtHPPara` 类名仍含 `HurtHPPara`（`ParaLooksLike` 的自证串）
- [ ] 字段 `m_pTarget` / `m_pAttacker` / `m_llValue` 均存在，偏移 ≤0x400
- [ ] `BigInt` 布局仍是 `double mantissa(+0) + int32 exponent(+8)`（按字段序核对，不看旧偏移）
- [ ] `CharacterType` 枚举字面量序未变：`2=MONSTER`、`3=BOSS`、`0=PLAYER`
- [ ] 玩家身份链完整：`BattleElementCenter.gameObjectManager` → `_player` → `_characterData.ulRoleId` → `_hitter` → `Hitter._camp`（日志能打出「玩家对象 _player=…（刷新）」）
- [ ] 战斗 30s 内 `STATS` 的「打怪」计数 ≥1（证明 `Hurt` 帧真被看到）
- [ ] 首领不被波及：打 BOSS 时 `首领/未知` 计数增长、血量按原速下降
- [ ] 血量量级变化时重估 `kKillMul`（若 2.0.1 血量膨胀，×100 可能不足以秒杀小怪）

---

### 卡 #2 · 无敌（feat **2**，`MXYTZ_FEAT_INVINCIBLE`）

**挂点**
- hook **pushframe** → `ActionGameLibrary.StarsCollider::DoAttack`（1.5.9 argc=7、**实例** ⇒ `argBase=1`；IL 参数 `arg1=IAttack attacker, arg2=IAccept accepter`）。
- 目标槽 = `a1 + 8*(argBase + 1)` = 槽 2（accepter）。**写入 8 字节 0**（引用置空）。
- 命中判定：`accepter == g_playerHitter`；否则取 `accepter` 的 `_interactiveGameObject` / `<interactiveGameObject>k__BackingField` / `interactiveGameObject`（三候选之一），再 `IsPlayerSide(igo)`。**结构认不出 ⇒ 一个字节都不写**（写错槽等于毁解释器帧）。

**语义**
- 置空走**游戏自带的作废分支**（1.5.9 `IL_0039-0044`）：`if (accepter != null && accepter.interactiveGameObject == null) return;` ⇒ 本次命中不结算、不飘字、不触发死亡 = 斌哥包里「打中玩家的命中已作废(accepter置null)」的同一机制。
- 第二层（心跳兜底）：玩家 `ActionState` 的免疫位 `immuneDamage / immunePhysicalDamage / immuneMagicDamage / invincible`（按名解析，偏移 ≤0x400），**只把当前为 0 的字节写 1**，非 0/1 的字节不碰；每个地址登记原值。
- 只写 1 字节；每 400ms 心跳维持（游戏可能自己清位）。

**约束与坑**
- pushframe **改不了返回值** ⇒ 只能靠「实参置空 + 免疫位」两条，不要试图让 DoAttack 提前 return。
- 置空只对**本次调用**有效（原函数读的是实参槽），无残留、无需还原；免疫位需还原。
- **不要重开 `localDamage` 写入**（v41 血证：`localDamage` 是**攻击方**的开关，敌人 `DoAttack` 一样写 ⇒ 敌人打我们变成客户端权威伤害还跳过 miss/dodge，用户 m02899「我开了 3 反而被秒」）。
- 已知限制（1.5.9 交付态）：`Character::Damage`（服务端同步效果包 `EffectToCast` 的落地路径）**不经过 DoAttack**，本版对它**只取证不写** ⇒ 服务端同步伤害仍可能致死。要补这一层属于**新增判据**，须用户当轮授权。
- `DoAttack` 是战斗最高频方法之一，热路径零日志（首 10 次 + 每 100 次一条）。

**新版验证清单（#2，9 条）**
- [ ] `StarsCollider` 类 + `DoAttack` 存在、argc=7、实例方法
- [ ] 帧从 pushframe 可见（`LEARN ok DoAttack7`）
- [ ] 「accepter 空 ⇒ ret」分支仍存在（**按语义找**：`ldarg.2 / brfalse / ret`，IL 偏移会漂，不要按 `IL_0039` 硬抄）
- [ ] accepter 的首个实参位置仍是 IL 第 2 参（槽 = argBase+1），且类型仍是 `IAccept`/`InteractiveGameObject` 系
- [ ] `_interactiveGameObject` 三候选至少命中一个（否则 accepter 认不出 ⇒ 无敌静默失效）
- [ ] 玩家 `_hitter` 链成立（`HitterOf(player) != 0`）
- [ ] `ActionState` 免疫位 4 个字段名仍在，且位宽 1 字节、值域 {0,1}
- [ ] 免疫位真被伤害判定读到（关闭功能后同一场景应当重新掉血）
- [ ] 是否必须补 `Character::Damage` 层（若真机仍被「无来源伤害」打死 ⇒ 先报用户，不自行加判据）

---

### 卡 #3 · 全模式技能伤害增加（feat **3**，`MXYTZ_FEAT_SKILL_DMG`）

**挂点**
- hook **pushframe** → `ActionData/全局命名空间.ActionCommand_AddProjectile::Execute`（1.5.9 argc=1、实例；参数 `ActionCommandData`）。
- 取参：槽 `argBase+0` 与 `argBase+1` 里**按类名含 `ActionCommandData`** 认（挡 `EventDelegate::Execute` 这类同名方法混入）。
- 数据链：`acd.data` 或 `acd._data` → 对象**类名含 `LAddProjectile`** → 字段 `_count`（int32）。
- 并发观测点 `AddProjectile`（17 参）**只观测不写**：它体内没有 `ldfld _count`，写它无效（v36/v37 的 14 次无效写入就是这么来的）。

**语义**
- `_count` 是 **17 参 `AddProjectile` 的循环上界**（`n = count + Action::projectileCount + 1`），全工程**唯一**读 `_count` 的地方是 `Execute` 的 `IL_011B callvirt get_count()`（1.5.9 token `0x0601EB99`）[文档]。
- 写入 = **乘区**：`target = round(base × kProjFanout)`，`kProjFanout = 3.0f`（v60 由 5 降到 3，专治发烫/卡顿）；`base` = 首次见到的原值（`> 120` 时截到 120）；`target ≤ base` 时不写（**只加不减**）；上限 `kProjCountCap = 120`。
- 每秒预算：本秒「新增发数」累计 > `kProjPerSecCap = 600` ⇒ 本次**不放大**（按原值透传），不再用旧的「20s 挂起」。
- **不写 `_activeCountLimit`**（v47 定案：保留引擎自己的回收闸门，写入它会改变回收语义）。
- 发送者闸（**fail-closed**）：`acd.actionGameObject` / `acd.action` → `IsPlayerSide`；再兜底 `action.character/_character/gameObject/_gameObject`；**解不出玩家侧 ⇒ 不写**（m02899：`LAddProjectile` 敌我共用实例，不判施法者会把敌人弹幕一起翻倍）。
- 还原：`RestoreProj(why)` 在「关 #3 / ForceClear」调用，**只用裸内存**：`IsReadableRange` + 对象首字 klass 裸比对（== 写入时记下的 klass）→ 当前值仍等于我们写的值才写回 base。全程**不调 il2cpp API**（否则已回收对象会 SIGSEGV，2026-10-26 tombstone `libil2cpp+0x205aa5c`）。

**约束与坑**
- 路线锁定：**「伤害数值放大」在本作实测无效**（属性、公式参数、投射物伤害三条路都试过），只能走「投射物数量」这条路。
- 只对**玩家侧施放**生效；敌人技能必须不受影响（回归项）。
- 关功能必须**立即还原**且不闪退（用户实测过「关 #3 瞬间闪退」，根因=还原时对野指针调 il2cpp API）。
- 学习表饱和史：`kKinds` 里 `cls==""` 的按名匹配目标会持续占槽并让 `LearnAllDone()` 恒假 ⇒ 战斗目标登记不进去、功能静默失效。**不要加回 `set_damage*` 那族**。
- 性能：对象数按扇出线性涨，`STATS` 里要看「投射秒峰值 / 槽数」；掉帧优先降扇出，不要加更多预算机制。

**新版验证清单（#3，10 条）**
- [ ] `ActionCommand_AddProjectile` 类 + `Execute` 存在、argc=1、实例方法
- [ ] 帧从 pushframe 可见（`LEARN ok ProjExec`）
- [ ] `Execute` 内仍存在 `get_count()` 且其返回值仍作 17 参 `AddProjectile` 的**循环上界**（重新定位 IL 偏移：判据是「Execute 里唯一读 `_count` 的点」，不是旧 `IL_011B`）
- [ ] 17 参 `AddProjectile` 仍**不读** `_count`（若变成读，可考虑回顶层帧）
- [ ] `LAddProjectile._count` 仍在、类型仍是 int32；`ActionCommandData.data`(或 `_data`) 仍在
- [ ] `ActionCommandData.actionGameObject`(或 `action`) 仍在（发送者闸依赖它，缺了功能整体 fail-closed 失效）
- [ ] `_count` 语义仍是「发射条数」（写 3 倍后**弹幕条数**实测变多，而不是单发伤害变高）
- [ ] 敌方弹幕不受影响（开 #3 被敌人打，伤害应与关闭时一致）
- [ ] 关闭瞬间不闪退（连续开关 5 次，无 tombstone）
- [ ] 扇出 3 / cap 120 / 每秒 600 三个常数按 2.0.1 的帧率与同屏对象数重新标定

---

### 卡 #4 · 免广告（feat **4**，`MXYTZ_FEAT_NO_AD`）

**挂点（1.5.9 真机定案的四条落点，缺一不可）**

| 位 | 类.方法（1.5.9） | argc | 作用 | 备注 |
|---|---|---|---|---|
| `K_GETADRSP` | `DS_Advertising::OnGetADInfoRsp` | 1（`Proto_CSRsp`） | **主落点**：把 `start_watch_ad_rsp.result` 写 **2** ⇒ 游戏自己走「免广告特权跳过观看」分支，**根本不调 `PlayADVideo`**（= 不出画） | 若该帧不可见，靠下一条 |
| `K_SENDEVENT` | `ProtocolManager::sendEvent` | 1（`EventItemInt`） | 实测**可见**的等价入口；结构发现 `EventItemInt+0x20` 拿到 `Proto_CSRsp`，在 `get_result()` 被读之前写 2 | 帧内先于 `OnGetADInfoRsp` |
| `K_PLAYAD` | `MiaoYueSDKProxy::PlayADVideo` | 2（`int adId, string uuid`） | 兜底 A2：**代厂商 SDK 发成功回调**（奖励到手），uuid 从十进制字符串 `strtoull` 还原 | 必须挂**子类**：基类 `BaseSDKAdapter::PlayADVideo` 是空桩 |
| `K_ADRESULT` / `K_ADREWARD` | `DS_Advertising::OnADVideoResult` / `DispatchResult` | 3 / 2 | 把 bool 实参槽（arg0=ret / arg1=ok）写成 1（**一次性事件门**，一次激活期只放 1 次） | 防「失败也发奖」刷屏 |

**语义**
- 字段链一律走**游戏自己的 getter/setter**（`runtime_invoke`），**禁止裸写字段**：`Proto_CSRsp.get_start_watch_ad_rsp()` → `SCStartWatchAdRsp` → `get_ad_id()/get_uuid()/get_result()/set_result(2)`。v48 裸写字段被真机证伪（该族实际用 `<X>k__BackingField`）。
- `runtime_invoke` 返回值读法：**值类型读 `r+0x10`**（v52 三断言定案 B），引用类型读 `r` 本身 + 类名子串自证；不确定就双读 + 业务约束自证。
- A2 代回调 = `runtime_invoke(DS_Advertising::OnADVideoResult, inst, {true, adId, uuid})`，`inst` 取自 `DataService.advertising` 静态字段；**必须在帧外执行**（orig 已返回、`t_modInvoke==0`），否则会在解释器帧内套帧（v46 帧外代回调）。
- 代回调用闸：熔断窗口（异常降级 30s）+ 累计配额 `kAdV43MaxDone` + `mi` 就绪 + 一次性事件门；开关切换时重置。
- 协议约束 [文档]：**不能跳过 `StartWatchAdReq`**（uuid 由服务端在 `start_watch_ad_rsp` 里下发，finish 必须带它）⇒ 本方案保留 start 包，只砍「播放」这一段。

**约束与坑**
- `result` 语义**反置**：`ret==true → result=0`（成功），`ret==false → result=1`；而 `OnGetADInfoRsp` 侧 `result != 0` 才是「跳过」。写错方向 = 直接失败。
- `finish_watch_ad_req.sign` 客户端**从不赋值**（1.5.9 最大不确定点）；服务端也可能校验 start→finish 时间间隔（0 秒完成）。
- 奖励 100% 由**服务端**下发；客户端唯一「领奖动作」= 那一发 `cmd 2762`。
- **验收口径（硬约束）**：`广告不出画` **且** `奖励到手`，两条同时成立才算完成；日志断言 `AD50 ★set_result(2)（getter 复核 ok=1）` 出现 **且** `AD43 PlayADVideo` 不再出现。禁止回到「只代回调、任由 SDK 出画」的旧设计。
- 任何拦截都必须**先保证奖励链路已武装**（fail-safe 方向 = 宁可广告照播也不能没奖励）。

**新版验证清单（#4，12 条 —— 本卡是版本风险最高的一张）**
- [ ] **广告 SDK 适配器类名**：1.5.9 是 `MiaoYueSDKProxy`（无命名空间，`BaseSDKAdapter` 子类）。**B 站渠道极可能换成 Bili 系适配器**（[推断]，必须从 2.0.1 的 dump/metadata 实证；改错类名 ⇒ 代回调整条断）
- [ ] `BaseSDKAdapter::PlayADVideo` 仍是空桩、实现仍在子类（否则挂基类即可，但先确认）
- [ ] `DS_Advertising` 类存在，`OnGetADInfoRsp`(1) / `OnADVideoResult`(3) / `DispatchResult`(2) 三个方法存在且 argc 未变
- [ ] `start_watch_ad_rsp.result` 语义未变：`==0` 才拉 SDK；非 0 走 `DispatchResult(ad_id,true)`；`==2` 仍是「已使用免广告特权跳过观看」
- [ ] 协议访问器逐字存在：`get_start_watch_ad_rsp` / `get_result` / `get_ad_id` / `get_uuid` / `set_result`
- [ ] `Proto_CSRsp` 类名仍含 `Proto_CSRsp`；`EventItemInt` 内 `Proto_CSRsp` 的偏移仍可被结构发现（1.5.9 是 +0x20，结构发现会自适应，但要确认能命中）
- [ ] `ProtocolManager::sendEvent`(argc=1) 仍存在且仍经 pushframe 可见
- [ ] `DataService.advertising` 静态字段仍在（代回调实例来源）
- [ ] 代回调签名仍是 `(bool, int, ulong)`；`runtime_invoke` 返回值约定仍是 `r+0x10`（若拿不到值，用双读+业务约束自证，不要猜）
- [ ] 广告入口未换：仍走 `WatchAdvertisements → StartWatchAdReq → OnGetADInfoRsp`，奖励仍靠 `cmd 2762 finish_watch_ad_req`
- [ ] B 站 SDK 是否有「激励视频回调走 Java 层」的旁路（若有真回调，可能与代回调叠加 ⇒ 需去重门）
- [ ] 真机验收两条同时成立（不出画 + 奖励到手），且 `AD50`/`AD43` 日志断言齐全

---

## 3. 关功能回滚与 fail-closed 总表

| 功能 | 写入物 | 还原时机 | 还原方式 | 残留风险 |
|---|---|---|---|---|
| #1 | `CtagHurtHPPara.m_llValue`（逐笔一次性） | 不需要 | 关功能即停止写 | 无（池化对象逐笔覆盖） |
| #2 | ① `DoAttack` 的 accepter 实参槽（本次调用）② 玩家 `ActionState` 免疫位 | ① 不需要 ② 关 #2 / ForceClear | ② 逐位写回登记的原值（地址需 `IsReadableRange`） | ② 状态对象已回收时跳过（有可读性检查） |
| #3 | `LAddProjectile._count` | **开关一变 false 立即** + ForceClear | 裸内存 + klass 裸比对 + 当前值==写入值才回写 | 对象已回收/复用 ⇒ 主动放弃（不写） |
| #4 | ① `start_watch_ad_rsp.result=2` ② bool 实参槽 ③ 代回调待办队列 | 关 #4 / ForceClear 重置待办、熔断、配额、一次性门 | ① 协议包一次性消费，无需还原 ② 槽随帧消失 ③ 队列清空 | 无持久写入 |

**fail-closed 方向（统一原则）**：解不出身份 / 解不出结构 / 值不合理 / 目标类型不认识 ⇒ **一律不写**（少生效一次，绝不写坏内存）。唯一例外方向是 #4：宁可广告照播，也不能让奖励链路断。

---

## 4. 已知坑速查（复刻时逐条对照）

| # | 坑 | 后果 | 出处 |
|---|---|---|---|
| 1 | 只 hook 具体方法 | 在本作物理不成立（methodPointer=共享桩） | `MxytzMod.cpp` 文件头、`PREFLIGHT` 实测 |
| 2 | 二级挂点 `allowWrite=false` | 战斗三功能**全部静默失效**（`IHURT=0 / INV cancel=0`） | v29→v30 |
| 3 | 只按方法名做键 | `EventDelegate::Execute` 毒化 → `ProjExec` 永不登记 | v39/v40 |
| 4 | 入口热路径直接解 a1 | SIGSEGV（`libstarry_magic.so+0x1a338`） | v26g |
| 5 | 写 MI 槽 | Scudo corrupted chunk → SIGABRT | v6 |
| 6 | 还原时对野指针调 `object_class/FieldOffset` | 关 #3 瞬间闪退（tombstone `libil2cpp+0x205aa5c`） | 2026-10-26 |
| 7 | `il2cpp_method_get_flags` 单参调用 | SIGSEGV_ACCERR | 2026-10-03 |
| 8 | 库基址取「off==0 且最小」 | 命中只读影子副本 ⇒ 四项全灭 | v45 |
| 9 | `localDamage=1` | 敌人打我们也走客户端权威 ⇒ 反被秒 | v41 / m02899 |
| 10 | `kKinds` 里留 `cls==""` 的按名目标 | 学习表饱和 ⇒ 战斗目标登记不进去 | v47 |
| 11 | 裸写广告字段 | 字段名不是直名（`<X>k__BackingField`）⇒ 无效 | v48 |
| 12 | 值类型返回值按 `*(void**)r` 读 | 读到 klass 残段 | 技能先例 §1（v50→v52） |
| 13 | 只代回调、不拦 `PlayADVideo` | 「奖励到手但广告照样跳出来」 | 硬约束 #8 |
| 14 | 把四功能串成一条通道 | 一个开关影响另一个（#3 连敌方投射物一起翻倍） | m02212 |

---

## 5. 新版必验清单汇总（勾选用）

- 卡 #1：**10 条**；卡 #2：**9 条**；卡 #3：**10 条**；卡 #4：**12 条** ⇒ 功能级 **41 条**。
- 全局级 **8 条**（见下）⇒ 合计 **49 条**。

**G 全局与环境**
- [ ] G1 2.0.1 仍是 Unity IL2CPP + HybridCLR 热更解释执行（抽样验证：`methodPointer` 仍等于同一个共享桩地址；若是 AOT 化，整套挂点方案要重做）
- [ ] G2 `libil2cpp.so` 未加固/未换壳（能取基址、能读段、能 dump）
- [ ] G3 解释器入口 RVA 重新定位（1.5.9 `0x207F798`）——用 `interp_entry_disasm` 同型手法核对 prologue 与 `bl` 调用点分布
- [ ] G4 pushframe RVA 重新定位（1.5.9 `0x207DCCC`）——判据：`bl` 到该地址的调用点**全部**落在解释器体内
- [ ] G5 共享桩 RVA 重新定位（1.5.9 `0x207F04C`）——用于确认「逐方法 hook 不可行」仍成立
- [ ] G6 重新取 2.0.1 的 `Assembly-CSharp.dll`（`hot1.unity3d`）并核 sha256 ≠ `7fa836ee…`；做 TypeDef/MethodDef 数量与命名空间直方图对比
- [ ] G7 `game_id` 新开并四处一致（`.tag` / `.authGameId` / 菜单模板 / 后台），**禁止复用 `mxyzt`**
- [ ] G8 1.5.9 的四条「目标类.方法」全部在新 DLL 里存在（缺失项要先判定是否改走等价落点，而不是先写代码）

---

## 6. 代码结构地图（供后续拆改；行号基于当前工作副本）

### 6.1 `Games/MxytzMod.cpp`（2681 行）— 分发层 / 免广告 / 装配

分区（按行号）：
| 区间 | 内容 |
|---|---|
| 1–128 | 文件头（路线声明、四项功能、防闪退设施、日志闸） |
| 129–250 | 状态/倍率/时间闸常量 + 安全助手（`NowMs`/`CanInstallHooks`/`SafeRead*`/`NameHas*`） |
| 251–420 | 类/方法查找、观察表、免广告待办（`EnqueueNoAdV43`）与一次性门 |
| 421–700 | 免广告旧桩族（`Hooked_Ad*`/`Hooked_PlayADVideo2`）与 `tryHook` |
| 701–1000 | 类名解析 `ResolveClassPref/ResolveMethod`、库基址 `LibBaseFromMaps/LibBaseResolve/LogLibMaps` |
| 1001–1150 | 学习表哈希 `LearnHash`、`RawWord`、`TagOf`、`InterpDumpTarget` |
| 1150–1250 | **`ForceTrueArg`**（#4 bool 实参强写）、`Il2CppU64OfStringV43` |
| 1250–1515 | **免广告主体**：`ApplyNoAdV43`、`NoAdEnqueueGatedV48`、`NoAdMiGetV50`〜`NoAdReadScalarsV52`、`NoAdCall*` |
| 1515–1750 | **`NoAdRspCoreV50`**（result=2 核心）、`ProtoCsRspInObjV49`、`ApplyNoAdRspV48`、`ApplyNoAdRspFromEventV49` |
| 1750–1830 | `DrainPendingAdV43`（帧外代回调）、`InterpEvidence` |
| 1830–1870 | **`InterpFire`**（位 → 功能帧转发，唯一分派点） |
| 1870–1980 | `LearnRegister`/`PreRegisterKindV43`/`EnsureAdTargetsV43`/`LearnAllDone` |
| 1980–2096 | **`InterpLearn`**（名字+类双键登记、`argBase` 定案）、`LearnProbe*` |
| 2096–2154 | **`InterpDispatch`**（热路径指针表查） |
| 2154–2298 | **`Hooked_Interp`**（入口桩）、`Hooked_PushFrame` 注释块 |
| 2298–2412 | **`Hooked_PushFrame`**、`InstallPushFrameHook`、`InstallInterpHook`、`InstallAdHooks` |
| 2412–2460 | **`SyncHooks`**（装配决策） |
| 2460–2510 | 裸读写助手（`PtrAt/I32At/SetI32/F32At`…） |
| 2511–2541 | `Mxytz_TryInit` / `Mxytz_IsReady` |
| 2541–2681 | **`Mxytz_OnFeatureChanged` / `Mxytz_Tick` / `Mxytz_ForceClear`**（菜单回调、学习期调度、STATS） |

函数定义清单（`名字 起始-结束行`，按行号）：
```
NowMs 133-138 | CanInstallHooks 139-147 | SyncFeatOnGate 148-156 | OkPtr 157-167 | LivePtr 168-169
SafeReadI32 170-176 | SafeReadF32 177-183 | SafeWriteF32 184-194 | CacheClassName 195-214
NameHas 215-224 | NameHasAny 225-230 | TypeEndsWith 231-246 | EnsureApi 247-251 | MethodPointer 252-253
FindClassNs 254-267 | FindClassAnywhere 268-295 | FindMethodNth 296-325 | RegisterWatch 326-367
NoteStatic 368-411 | Il2CppU64OfStringV43(声明) 412-414 | EnqueueNoAdV43 415-445 | AdLogOnce 446-469
AdForceGateOpen 470-481 | AdForceGateReset 482-485 | Hooked_AdVoid 486-492 | Hooked_AdVoid2 493-499
Hooked_AdVoid3 500-506 | Hooked_AdBoolRet 507-535 | Hooked_AdArg0TrueB 536-561
Hooked_PlayADVideo2 562-643 | tryHook 644-661 | Hooked_SelfTest 662-670 | LogTargetAddr 671-705
ResolveClassPref 706-714 | ResolveMethod 715-772 | LibBaseFromMaps 773-826 | LibBaseResolve 827-837
LogLibMaps 838-1001 | LearnHash 1002-1090 | RawWord 1091-1094 | PlausiblePtr 1095-1099
SafeObjType 1100-1127 | TagOf 1128-1134 | InterpDumpTarget 1135-1149 | ForceTrueArg 1150-1219
Il2CppU64OfStringV43 1220-1247 | NoAdEnqueueGatedV48(声明) 1248-1249 | ApplyNoAdV43 1250-1271
NoAdEnqueueGatedV48 1272-1350 | NoAdMiGetV50 1351-1381 | NoAdInvokeGetRawV52 1382-1395
NoAdReadRawV52 1396-1411 | NoAdReadScalarsV52 1412-1461 | NoAdCallGetObjV51 1462-1499
NoAdCallSetI32V51 1500-1513 | NoAdRspCoreV50 1514-1647 | ProtoCsRspInObjV49 1648-1707
ApplyNoAdRspV48 1708-1719 | ApplyNoAdRspFromEventV49 1720-1745 | DrainPendingAdV43 1746-1788
InterpEvidence 1789-1827 | InterpFire 1828-1866 | LearnRegister 1867-1900 | PreRegisterKindV43 1901-1922
EnsureAdTargetsV43 1923-1965 | LearnAllDone 1966-1977 | InterpLearn 1978-2069 | LearnProbe 2070-2088
LearnProbeRollWindow 2089-2095 | InterpDispatch 2096-2153 | Hooked_Interp 2154-2297
Hooked_PushFrame 2298-2315 | InstallPushFrameHook 2316-2338 | InstallInterpHook 2339-2368
InstallAdHooks 2369-2411 | SyncHooks 2412-2459 | PtrAt 2460-2464 | ByteAt 2465-2469 | SetByte 2470
I32At 2471-2475 | SetI32 2476 | U32At 2477-2481 | U64At 2482-2486 | F32At 2487-2491 | SetF32 2492-2495
RuntimeReadyForWalk 2496-2510 | Mxytz_TryInit 2511-2538 | Mxytz_IsReady 2539-2540
Mxytz_OnFeatureChanged 2541-2570 | Mxytz_OnValueChanged 2571-2575 | Mxytz_Tick 2576-2670
Mxytz_ForceClear 2671-2681
```

### 6.2 `Games/MxytzBattle.cpp`（816 行）— 战斗三功能（#1/#2/#3 全部在这里）

| 区间 | 内容 |
|---|---|
| 1–58 | 文件头（三条生效机制、纪律） |
| 60–98 | 基础读写与指针判据（`Plaus/RefAt/I32At/SetI32/SetU8`…） |
| 99–175 | 类/字段解析缓存（`KC` / `Off` / `OffAny`） |
| 175–280 | 身份取数（`KOf`/`RoleOf`/`CtypeOf`/`HitterOf`/`CampOf`/`RefreshPlayer`） |
| 280–320 | **`IsPlayerSide`**（三判据 fail-closed） |
| 319–377 | `ParaLooksLike`/`ParaOf`（深度自证）/`ParaLayoutReady` |
| 378–462 | **#1 `HurtFrame`** |
| 463–518 | #2 免疫位 `EnsureInvulnBits`/`RestoreInvulnBits` |
| 519–582 | **#2 `DoAttackFrame`** |
| 583–652 | #3 预算与还原 `ProjSecTick`/`RestoreProj`/`ProjSlotTake` |
| 653–752 | **#3 `ProjExecFrame`** |
| 753–816 | 对外接口 `SetSwitches`/`OnFeatureChanged`/`OnTick`/`OnFrame`/`ForceClear`/`StatLine` |

函数清单：
```
NowMs 60-66 | Plaus 67-69 | Un 70-72 | RawWord 73-76 | RefAt 77-81 | I32At 82 | U64At 83 | U8At 84
SetI32 85 | SetU8 86 | SetU64 87-98 | KC 99-137 | Off 138-166 | OffAny 167-174 | KOf 175-189
RoleOf 190-206 | CtypeOf 207-222 | HitterOf 223-232 | CampOf 233-242 | RefreshPlayer 243-279
IsPlayerSide 280-318 | ParaLooksLike 319-328 | ParaOf 329-360 | ParaLayoutReady 361-377
HurtFrame 378-462 | EnsureInvulnBits 463-503 | RestoreInvulnBits 504-518 | DoAttackFrame 519-582
ProjSecTick 583-593 | RestoreProj 594-630 | ProjSlotTake 631-652 | ProjExecFrame 653-752
SetSwitches 753-760 | OnFeatureChanged 761-765 | OnTick 766-774 | OnFrame 775-792 | ForceClear 793-803
StatLine 804-816
```

### 6.3 头文件与入口（拆改时的契约）

- `MxytzMod.h`：`MxytzFeat` 枚举（0 占位 / 1 秒杀 / 2 无敌 / 3 技能 / 4 免广告 / 5 COUNT）——**菜单下标硬契约**，改菜单必须同步 `MxytzEntry.cpp` 的 `featureList`。
- `MxytzBattle.h`：`Frame` 枚举（`FRAME_HURT/DOATTACK/PROJADD/PROJEXEC`）+ 6 个对外函数；**本模块不装任何 hook**，只接收已命中的帧（拆改时保持这个边界，别把 hook 塞进战斗文件）。
- `MxytzEntry.cpp`：`StarryGameMod` 注册表（`.tag` / `.authGameId` = 本作的 game_id 落点；`initRetryMax=0` 禁启动期 init，避免加载窗死锁）。

---

## 7. 未决与待验证假设（复刻开始前要补齐的）

| # | 待验证项 | 影响 | 怎么确认 |
|---|---|---|---|
| 1 | **B 站渠道的广告 SDK 适配器类名**（1.5.9 是 `MiaoYueSDKProxy`） | #4 代回调可能整条断 | 2.0.1 的 DLL 元数据里枚举 `BaseSDKAdapter` 的子类 |
| 2 | 2.0.1 是否仍 HybridCLR 解释执行 | 若是 AOT 化，整套「hook 解释器」方案失效 | 取 `methodPointer` 抽样对比 |
| 3 | 2.0.1 的 `libil2cpp.so` 是否加固/分段变化 | RVA 定位与 dump 成本 | ELF 段表与 `bl` 分布核对 |
| 4 | 服务端是否校验广告时长/签名（`sign` 恒空） | #4 是否会被判作弊 | 真机一轮观测 `finish_watch_ad_rsp` 结果 |
| 5 | 2.0.1 是否新增「反作弊校验」读 `m_llValue` 量级 | #1 的 ×100 是否安全 | 结算回包/异常日志观察 |
| 6 | `EventItemInt+0x20` 结构偏移 | #4 入口② 能否发现 rsp | 结构发现日志（会自适应，但要确认命中） |
| 7 | `#2` 是否需要补 `Character::Damage` 层 | 服务端同步伤害仍可致死 | 真机「无来源掉血」观测；补层需用户授权 |

---

## 8. 与规范的关系（本文件不新增规则）

- 本文件只承载**单游戏、单渠道、单版本**的技术事实与规格（AGENTS §1「入口边界」：游戏级内容落 `10_docs/{游戏}/`）。
- 硬约束仍以 `10_docs/冒险与征途/硬约束.md` 为准（当前 A/B 区为空，F 区 9 条历史；本文件引用的是其中的**机制性教训**，不新增约束）。
- 四项功能「各自独立、互不复用判据」与「关功能不得残留乘区」是历史用户明令（m02212 / m03027），复刻时按**机制性要求**执行。
