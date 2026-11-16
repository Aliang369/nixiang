# 冒险与征途 B站 2.0.1 · 热更程序集（Assembly-CSharp.dll）提取与元数据对照

> 分析对象：`13_scratch/bili_201/`（com.my.mxyzt.bilibili / vc201 / arm64-v8a / Unity 2022.3.14f1 + HybridCLR）
> 产物目录：`06_dumped/冒险与征途/bili_2.0.1/`
> 方法：纯静态、Python 标准库 + `lz4.block`（C 实现，只用来解压；无外网、无 UnityPy/lief）
> 证据等级：**[实测]**＝本轮量出的值；**[文档历史]**＝1.5.9 报告/文档留存值；**（推断）**＝由结构直接推得、未过运行期；**[待验证]**＝必须装机或 IL 反编译才能定

---

## 1. 结论摘要

| 项 | 新版 2.0.1 实测 |
|---|---|
| 容器 | `assets/dataroot/95d30e2bdf8fbf25564bd77272c8c242.unity3d`（9,133,368 B，sha256 `ead1d2b9…38af9c1`） |
| 程序集 | **Assembly-CSharp**，21,671,936 B，sha256 `ceab1132b321996a630e256d4750231a8920caa983ea26c0335e05de22b4f9ae` |
| 元数据 | BSJB@0x8A55D8，`v4.0.30319`，runtime 2.5，5 个流（#~ / #Strings / #US / #GUID / #Blob） |
| TypeDef | **14,305**（1.5.9 为 14,971，**−666**） |
| MethodDef | **165,234**（1.5.9 为 173,184，**−7,950**） |
| RVA≠0 | 164,156 / 165,234 = **99.35%**（1.5.9 为 99.365%）⇒ 仍是带真 IL 体的热更程序集，不是 metadata-only |
| 关键类型 | **31/31 命中**（30 个在规格给的命名空间内；`Proto_CSRsp` 实际在 `proto_ff` 命名空间，与 1.5.9 相同） |
| 方法点名 | **11/11 命中**，argc 与 static/instance **全部与 1.5.9 一致** |
| 广告实现类 | 仍是 **MiaoYueSDKProxy**，**不存在 Bili 系适配器** |
| token | **全部变化**（非等比平移，见 `6）⇒ 任何硬编码 token 在 2.0.1 全部失效 |

---

## 2. 提取链路（复用 1.5.9 的 carve 手法，参数已变）

1.5.9 的手法（[文档历史] `06_dumped/冒险与征途/aot/_aot_report.md` `2）：UnityFS 头 → blocksInfo **LZ4 block 解压** → 逐块解压 → 在解压数据里按目录项布局找 `Assembly-CSharp.dll` → 裁切 PE。

新版同名文件 `hot1.unity3d` **不存在**（dataroot 全是 32-hex 哈希名，无任何 hot* 名字）⇒ 改为「全量解码 + 按魔数定位」。

### 2.1 定位过程（[实测]）

1. 按名找：`find assets -iname 'hot*'` → **0 命中**。
2. 裸字节扫 `BSJB` / `Assembly-CSharp`：3,714 个 `.unity3d` 里 23 个命中（多数是 `MonoScript` 引用程序集名的巧合串）。
3. 证明原始文件里没有明文 PE（全 assets 4,923 个文件扫 `MZ`+`PE\0\0`：**0 命中**）⇒ 必须解码。
4. 自写通用 UnityFS 解码器（`_find_dll.py`）：**3,714/3,714 全部解析成功**，逐块解压后找合法 PE ⇒ **6 个候选**，其中只有 1 个含 `Assembly-CSharp.dll` 目录项。

### 2.2 容器参数（[实测]，与 1.5.9 同构）

| 字段 | 2.0.1（95d30e2b…） | 1.5.9（hot1.unity3d） |
|---|---|---|
| 头起点 | **0x0D**（13 字节前缀 `556e6974794653 0037f357129a` = 伪 `UnityFS\0` + 垃圾） | 0x0D（同款） |
| version / unity | 8 / `5.x.x` / `2022.3.14f1` | 8 / 同 |
| flags | `0x243`（含 0x200 位；实测**未加密**） | `0x243` |
| 压缩 | 低 6 位=3 = **LZ4HC**（166/166 块全 3） | 3 = LZ4HC（173 块） |
| blocksInfo | @0x4D，cbs=817 → ubs=1741 | @0x4D，cbs=862 → ubs=1811 |
| 数据区 | 0x38D（=0x4D+817，**+15 字节填充**） | 0x3AD（+2 字节填充） |
| Σ 解压尺寸 | 21,674,764 | 22,647,808 起 |
| node | 1 个 `CAB-`（路径串为空） | 1 个 `CAB-fac7085f…` |

> 块数 173→166，块大小仍 131,072；数据区前填充从 2 字节变 15 字节 ⇒ 「blocksInfo 偏移 + cbs」不能硬抄，要用 `data_start = 文件长 − ΣcompressedSize` 反推。

### 2.3 裁切布局（[实测]）

```
解压体 21,674,764 B
0x0AF0: 13 00 00 00           ; 名字长度 = 0x13 = 19
0x0AF4: "Assembly-CSharp.dll\0"
0x0B08: B0 4A 01 4D           ; u32 = 21,671,936 = dll 长度
0x0B0C: 4D 5A 90 00 …         ; PE 起点（MZ）
```

PE 自校验通过：e_lfanew=0x80、machine=0x14C、3 段（.text/.rsrc/.reloc）、CLR 头 cb=0x48 runtime=2.5、MetaDataRVA=0x8A68CC、BSJB@0x8A55D8、`raw_end == size_field == 21,671,936`。

### 2.4 其余 5 个「有魔数」的 bundle 是什么（[实测]，用于排除）

| bundle | 解压体 | 内容 |
|---|---|---|
| `9e2fe9a8…` | 5,319,716 | 3 个 PE：**DOTween**（186 类）、**UnityEngine.CoreModule**（826 类）、**UnityEngine.AssetBundleModule**（8 类） |
| `697730d0…` | 33,032 | 1 个 PE：**AotUpdate**（28 类 / 149 方法） |
| `748e419b…` / `998eb4e9…` / `14f6e1a8…` | 25.4M / 9.4M / 1.1M | 无 PE；`BSJB` 是**假阳性**（三份同一段压缩数据的巧合串，后面不是合法元数据根） |

⇒ 前两类是 HybridCLR 的 **AOT 补充元数据（AOT dll 参考集）**，**不是热更代码**；热更逻辑只有 Assembly-CSharp 一份。

---

## 3. 元数据统计与 G6 对照

| 指标 | 1.5.9 [文档历史] | 2.0.1 [实测] | 变化 |
|---|---|---|---|
| 程序集名 | Assembly-CSharp | Assembly-CSharp | 不变 |
| 文件大小 | 22,647,808 | 21,671,936 | −975,872 |
| sha256 | `7fa836ee…` | `ceab1132…` | **≠（G6 通过）** |
| TypeDef | 14,971 | 14,305 | −666 |
| MethodDef | 173,184 | 165,234 | −7,950 |
| RVA≠0 | 172,085（99.365%） | 164,156（99.35%） | 比例持平 |
| Field | 118,820 | 113,572 | −5,248 |
| Param | 80,741 | 77,369 | −3,372 |
| Property | 50,742 | 48,461 | −2,281 |
| MemberRef | 24,574 | 23,591 | −983 |
| TypeRef | 647 | 643 | −4 |
| NestedClass | 5,483 | 5,208 | −275 |
| CustomAttribute | 26,249 | 25,163 | −1,086 |
| TypeSpec | 6,870 | 6,524 | −346 |
| MethodSpec | 6,154 | 5,887 | −267 |
| AssemblyRef | 47 | 47 | 不变 |

**命名空间直方图 Top20（2.0.1，[实测]）**

| # | 命名空间 | TypeDef | 1.5.9 对照 [文档历史] |
|---|---|---|---|
| 1 | `<global>`（无命名空间） | 7,767 | 8,203 |
| 2 | `proto_ff` | 2,850 | 2,987 |
| 3 | `GameConfigData` | 1,725 | 1,812 |
| 4 | `ActionGameLibrary` | 853 | 853（**不变**） |
| 5 | `WellFired` | 122 | — |
| 6 | `ActionData` | 118 | 118（不变） |
| 7 | `BehaviorDesigner.Runtime.Tasks` | 50 | — |
| 8 | `ProtoBuf.Serializers` | 40 | — |
| 9 | `WellFired.Shared` | 37 | — |
| 10 | …Tasks.Unity.SharedVariables | 36 | — |
| 11 | …Tasks.Unity.UnityRigidbody | 36 | — |
| 12 | …Tasks.Unity.UnityAudioSource | 35 | — |
| 13 | `ProtoBuf` | 33 | — |
| 14 | …Tasks.Unity.UnityTransform | 31 | — |
| 15 | …Tasks.Unity.UnityAnimator | 31 | — |
| 16 | `ActionGameLibrary.BattleTypeController` | 28 | — |
| 17 | `BehaviorDesigner.Runtime` | 27 | — |
| 18 | …ParticleSystem | 27 | — |
| 19 | …Rigidbody | 25 | — |
| 20 | `Google.ProtocolBuffers` | 24 | — |

命名空间总数 **84**（1.5.9 也是 84）。战斗核心 `ActionGameLibrary` 853=853、`ActionData` 118=118 **一个没少** ⇒ 战斗侧无大规模重构。
> 旧版完整直方图**未留存**（旧 dll 不在本机，`aot/` 只有 sha256 记录）⇒ 只能对比上面 4 个已知值，其余标「未确认」。


---

## 4. 关键类型命中（31 项）

「方法数」= TypeDef.MethodList 区间长度；token = `0x02xxxxxx`。

### 4.1 `ActionGameLibrary` 命名空间（17）

| # | 类型 | 结果 | token（新） | 方法数 | 旧 token [文档历史] |
|---|---|---|---|---|---|
| 1 | CBattleActorUtil | ✅ | 0x02001DD4 | 26 | 0x02001F55 |
| 2 | CtagHurtHPPara | ✅ | 0x02001E48 | 21 | 0x02001FC9 |
| 3 | CHitNotifier | ✅ | 0x02001E26 | 52 | 0x02001FA7 |
| 4 | DamageFactory | ✅ | 0x02001E4C | 33 | 0x02001FCD |
| 5 | Character | ✅ | 0x02001FC2 | 31 | 0x02002143 |
| 6 | SimpleCharacter | ✅ | 0x02001D6C | 8 | 0x02001EED |
| 7 | InteractiveGameObject | ✅ | 0x02001D6B | 24 | 0x02001EEC |
| 8 | StarsCollider | ✅ | 0x02001EB1 | 45 | 0x02002032 |
| 9 | GameObjectManager | ✅ | 0x02001ED4 | 98 | 0x02002055 |
| 10 | ActionGameObjectData | ✅ | 0x02001FBB | 6 | 0x0200213C |
| 11 | ActionState | ✅ | 0x02001FD0 | 114 | 0x02002151 |
| 12 | SecondaryAttribute | ✅ | 0x02001D63 | 175 | 0x02001EE4 |
| 13 | BattleElementCenter | ✅ | 0x02001E8C | 22 | 0x0200200D |
| 14 | DamageInfo | ✅ | 0x02001E88 | 5 | 0x02002009 |
| 15 | TSpellHitPara | ✅ | 0x02001E40 | 4 | 0x02001FC1 |
| 16 | ActionGameLibraryConfig | ✅ | 0x02001E85 | 8 | 0x02002006 |
| 17 | ActionCommand_AddProjectile | ✅ | 0x02001F1F | 8 | — |

### 4.2 无命名空间（13，+1 附带）

| # | 类型 | 结果 | token（新） | 方法数 | 旧 token |
|---|---|---|---|---|---|
| 18 | BigInt | ✅ | 0x020000CA | 55 | 0x020000CB |
| 19 | DamageEnumerate | ✅ | 0x020000DD | 1 | 0x020000DE |
| 20 | CharacterType | ✅ | 0x020000F1 | 0 | 0x020000F2 |
| 21 | DS_Advertising | ✅ | 0x02000166 | 19 | 0x0200016B |
| 22 | DS_Privilege | ✅ | 0x020009BA | 106 | 0x02000A5A |
| 23 | SDKProxy | ✅ | 0x020003D0 | 2 | 0x0200040E |
| 24 | BaseSDKAdapter | ✅ | 0x020003CB | 80 | 0x02000409 |
| 25 | MiaoYueADCallBack | ✅ | 0x020003CC | 2 | 0x0200040A |
| 26 | AdMascotManager | ✅ | 0x020004F5 | 45 | 0x02000535 |
| 27 | ProtocolManager | ✅ | 0x020003AB | 34 | — |
| 28 | DataService | ✅ | 0x02000194 | 13 | — |
| 29 | EventItemInt | ✅ 但**是 `ProtocolManager` 的嵌套类型**（全名 `ProtocolManager.EventItemInt`） | 0x020026A8 | 2 | — |
| 30 | Proto_CSRsp | ✅ 名字命中，**命名空间是 `proto_ff`**（与 1.5.9 相同） | 0x02001587 | — | — |
| +1 | MiaoYueSDKProxy（规格未列，#4 的真实落点） | ✅ | 0x020003CF | 46 | 0x0200040D |

**命中 31/31**：30 项完全符合规格给的命名空间，1 项（`Proto_CSRsp`）按名命中、命名空间为 `proto_ff` 且与旧版一致。

---

## 5. 方法级点名核对（11/11）

| 类::方法 | 结果 | 新 token | argc | static/instance | 返回 | 旧值（规格） |
|---|---|---|---|---|---|---|
| CBattleActorUtil::Hurt | ✅ | 0x0601CAF9 | **5** | **static** | int | 5 静态 ✓ |
| StarsCollider::DoAttack | ✅ | 0x0601D175 | **7** | **instance** | void | 7 实例 ✓ |
| Character::Damage | ✅ 两个重载 | 0x0601D9CB（long）/ 0x0601D9CC（BigInt） | 3 / 3 | instance | void | 3 ✓ |
| BattleElementCenter::get_localDamage | ✅ | 0x0601D097 | **0** | **static** | bool | 0 ✓ |
| ActionCommand_AddProjectile::Execute | ✅ | 0x0601D432 | **1** | instance | object | 1 实例 ✓ |
| ActionCommand_AddProjectile::AddProjectile | ✅ | 0x0601D433 | **17** | instance | ActionGameProjectile | 17 ✓ |
| DS_Advertising::OnGetADInfoRsp | ✅ | 0x06001229 | **1** | instance | void | 1 ✓ |
| DS_Advertising::OnADVideoResult | ✅ | 0x0600122B | **3** | instance | void | 3 ✓ |
| DS_Advertising::DispatchResult | ✅ | 0x0600122E | **2** | instance | void | 2 ✓ |
| ProtocolManager::sendEvent | ✅ | 0x06003194 | **1** | instance | void | 1 ✓ |
| BaseSDKAdapter::PlayADVideo | ✅ | 0x060032B5 | **2** | instance | void | 2 ✓ |

DoAttack 形参序列逐字一致：`(IAttack, IAccept, AttackData, AcceptData, Vector3, int, SynHitData)`；Execute 形参 = `ActionCommandData`；Hurt 形参 = `(ref CtagHurtHPPara, ref bool, bool, ref BigInt, bool)`。

附带（规格依赖的邻近方法，[实测]）：

| 方法 | token | argc | 说明 |
|---|---|---|---|
| CBattleActorUtil::HurtHp | 0x0601CAFA | 5 static | 与 Hurt **相邻**（旧版 0x0601E25A/0x0601E25B 同样相邻）⇒ 整体位移 |
| DamageFactory::ProcDmgHit | 0x0601CE16 | 7 static | — |
| DS_Advertising::WatchAdvertisements | 0x06001227 | 1 | 顶层入口（旧 0x06001251） |
| DS_Advertising::StartWatchAdReq | 0x06001228 | 1 | — |
| MiaoYueADCallBack::onRewardArrived | 0x060032E2 | 2 | `(bool, AndroidJavaObject)`，旧 0x0600362C |

### 5.1 所有名为 `PlayADVideo` 的方法（3 个，[实测]）

| 所属类 | token | argc | 类型 | 角色 |
|---|---|---|---|---|
| **MiaoYueSDKProxy** | **0x06003342** | 2 | instance | **★真实实现，推荐 hook 点**（`BaseSDKAdapter` 子类） |
| BaseSDKAdapter | 0x060032B5 | 2 | instance | 基类**空桩**（必须挂子类） |
| MiaoYueSDKGameObject | 0x060032F3 | 1 | instance | Unity 侧包装（Hashtable 参数，走 Java 反射） |

**`BaseSDKAdapter` 的全部子类（扫 Extends）：**

| 子类 | token | 方法数 | 有 PlayADVideo |
|---|---|---|---|
| MiaoYueSDKProxy | 0x020003CF | 46 | ✅ |
| WeChatSDKProxy | 0x020003D6 | 7 | ❌ |

⇒ **B 站渠道没有换适配器**：实现仍是妙月 `MiaoYueSDKProxy`；不存在 Bili 系子类，全程序集中名字含 `Bili` 的类型 **0 个**。
⇒ 规格 #4 第一条风险（「B 站渠道极可能换成 Bili 系适配器」）**不成立**，`K_PLAYAD` 落点类名可沿用。
[待验证] B 站 SDK 是否有 Java 层旁路：`MiaoYueADCallBack.onRewardArrived` 仍在，说明 Java 回调链在；是否另起一路需读 dex（本轮范围只到 assets/dll）。

### 5.2 DS_Advertising 全部 19 个方法（名字 + argc，[实测]）

| 方法 | argc | 返回 | 方法 | argc | 返回 |
|---|---|---|---|---|---|
| init | 0 | void | GetADCdsParam | 1 | int |
| clear | 0 | void | **WatchAdvertisements** | 1 | void |
| OnRefreshLoginOnceTip | 1 | void | **StartWatchAdReq** | 1 | void |
| MarkLoginOnceTip | 2 | void | **OnGetADInfoRsp** | 1 | void |
| HasLoginOnceTip | 2 | bool | OnLoginFinish | 1 | void |
| ResetLoginOnceTips | 0 | void | **OnADVideoResult** | 3 | void |
| get_FunctionEnable | 0 | bool | ReqADVideoResult | 3 | void |
| GetADTimes | 1 | int | RspADVideoResult | 1 | void |
| GetADSysParam | 1 | string | **DispatchResult** | 2 | void |
| .ctor | 0 | void | — | — | — |

⇒ 规格 #4 的 4 条落点全部在位、argc 全对。

协议访问器（[实测]）：`proto_ff.Proto_CSRsp` 有 `get_start_watch_ad_rsp`(0)->`Proto_SCStartWatchAdRsp`、`set_start_watch_ad_rsp`(1)、`get_finish_watch_ad_rsp`(0)；`Proto_SCStartWatchAdRsp` 有 `get_ad_id`(0,int)、`get_uuid`(0,**ulong**)、`get_result`(0,int)、`set_result`(1)、`set_uuid`(1)；`Proto_CSFinishWatchAdReq` 另有 `get_sign/set_sign`(string)。
⇒ 规格「协议访问器逐字存在」与「uuid 为 ulong ⇒ 对应 strtoull 还原」**均成立**。

---

## 6. token 变化对照

**结论：TypeDef/MethodDef token 全部变化，且非等比平移。**

| 区域 | 旧 → 新 | 位移 |
|---|---|---|
| BigInt | 0x020000CB → 0x020000CA | −1 |
| DamageEnumerate / CharacterType | 0x020000DE/0x020000F2 → 0x020000DD/0x020000F1 | −1 |
| DS_Advertising | 0x0200016B → 0x02000166 | −5 |
| SDKProxy / BaseSDKAdapter / MiaoYueSDKProxy | 0x0200040E/0x02000409/0x0200040D → 0x020003D0/0x020003CB/0x020003CF | −0x3E |
| DS_Privilege | 0x02000A5A → 0x020009BA | −0xA0 |
| CBattleActorUtil / CtagHurtHPPara / StarsCollider / Character | 0x02001F55/0x02001FC9/0x02002032/0x02002143 → 0x02001DD4/0x02001E48/0x02001EB1/0x02001FC2 | −0x17D ~ −0x181 |
| DS_Advertising 一族方法 | 0x06001251 → 0x06001227 | −0x2A |
| CBattleActorUtil::Hurt | 0x0601E25A → 0x0601CAF9 | −0x1761 |
| Character::Damage(BigInt) | 0x0601F132 → 0x0601D9CC | −0x1766 |
| MiaoYueSDKProxy::PlayADVideo | 0x0600368D → 0x06003342 | −0x34B |

**规律**：同一类内部方法相对顺序不变（Hurt/HurtHp 相邻、两个 Damage 重载相邻）⇒「类名 + 方法名 + argc 取 MethodInfo」跨版本安全；硬编码 token 在 2.0.1 全部失效（与 `hook-rules.md` 指纹铁律一致）。

---

## 7. 字段核对（FieldDef 声明序 + 偏移推断）

> **方法学（必读）**：.NET 程序集**实例字段的字节偏移不在 dll 内** —— 实测 `FieldLayout` 表 **0 行**、`ClassLayout` 仅 20 行（都不是这些战斗类）。下表「偏移」全部是**按声明序 + IL2CPP 对象头 0x10 + 8 字节对齐推出的（推断）**，不是读出来的。运行时必须**按名解析**（规格本来就要求偏移 ≤0x400 才采用），**不要把推断值写成常量**。

### 7.1 CtagHurtHPPara（21 字段）

| 序 | 字段 | 类型 | 推断偏移 |
|---|---|---|---|
| #0 | `m_pTarget` | `ActionGameLibrary.IActionGameObject` | 0x10 |
| #1 | `m_pAttacker` | `IActionGameObject` | 0x18 |
| #2 | `m_llValue` | `BigInt`（**值类型**，double+int+pad ≈16B） | 0x20 |
| #3 | `m_llCalculateDamage` | BigInt | 0x30 |
| #20 | `s_pool` | Stack<CtagHurtHPPara> static readonly | —（池化证据，与规格「池化、逐笔一次性」一致） |

⇒ `m_llValue` **仍是 BigInt**（不是 long）⇒「写 8 字节 double + 4 字节 int32」的写法不变。

### 7.2 BigInt（6 字段）

| 序 | 字段 | 类型 | 推断偏移 |
|---|---|---|---|
| #0 | `m_mantissa` | **double** | +0 |
| #1 | `m_exponent` | **int32** | +8 |
| #2 | `Zero` | BigInt static readonly | — |

⇒ **与 1.5.9 完全一致**。**注意字段名带 `m_` 前缀**：规格里的 `mantissa`/`exponent` 是简写，按简写名解析会失败。

### 7.3 LAddProjectile（`ActionData.LAddProjectile`）

| 序 | 字段 | 类型 | 备注 |
|---|---|---|---|
| #8 | `_count` | **int32** | ✅ 仍在、类型未变 |
| #9 | `_activeCountLimit` | int32 | **紧邻 _count**（规格要求不写的那个，小心错位） |

### 7.4 ActionCommandData（10 字段，`ActionGameLibrary.ActionCommandData`）

| 序 | 字段 | 类型 | 推断偏移 |
|---|---|---|---|
| #0 | `eventData` | — | 0x10 |
| #1 | `data` | `ProtoBuf.IExtensible`（接口；运行时是 `LAddProjectile` 实例） | 0x18 |
| #2 | `action` | `ActionGameLibrary.Action` | 0x20 |
| #3 | `actionGameObject` | `IActionGameObject` | 0x28 |

⇒ 规格 #3 依赖的 `data` 与 `actionGameObject` **都在**，且都是**普通字段**（无 `_` 前缀、无 `<>k__BackingField` 变体）⇒ 可按名解析。注意 `data` 静态类型是接口，类名判据必须落在**运行时对象**上。

### 7.5 EventItemInt（嵌套于 ProtocolManager，5 字段）

| 序 | 字段 | 类型 | 推断偏移 |
|---|---|---|---|
| #0 | `type` | uint | 0x10 |
| #1 | `reqSequence` | int | 0x14 |
| #2 | `iCacheIndex` | int | 0x18 |
| #3 | `data` | **`proto_ff.Proto_CSRsp`** | **0x20** |
| #4 | `packageType` | int | 0x28 |

⇒ `Proto_CSRsp` 在 **+0x20**，**与 1.5.9 一致**（推断但结构自洽：0x10+0xC=0x1C → 8 对齐 0x20）。
⇒ 类型全名是 `ProtocolManager.EventItemInt`，正是 `sendEvent` 的形参类型。

### 7.6 DataService.advertising

| 字段 | 类型 | 修饰 | 序号 |
|---|---|---|---|
| `advertising` | `DS_Advertising` | **static** | #104（共 130 字段） |

⇒ 代回调实例来源仍在（规格 #4 第 8 条）。

### 7.7 ActionState 免疫位（81 字段中的 5 个）

| 序 | 字段 | 类型 |
|---|---|---|
| #29 | `immuneDamage` | bool |
| #30 | `immunePhysicalDamage` | bool |
| #31 | `immuneMagicDamage` | bool |
| #32 | `immuneDebuff` | bool（规格未提，多出来的） |
| #36 | `invincible` | bool |

⇒ 规格要求的 4 个名字**全在**、类型仍 bool（1 字节、{0,1}）。偏移**未确认**（无 FieldLayout，需 il2cpp FieldOffset / dump.cs）；#29–#32 连续但 #36 前有非 bool 字段 ⇒ 不能按序数线性推，必须按名取 offset（本就是规格做法）。

### 7.8 CharacterType 枚举字面量序 [实测]

| 名 | 值 |
|---|---|
| PLAYER | **0** |
| APC | 1 |
| MONSTER | **2** |
| BOSS | **3** |
| TOWER | 4 |
| HERO | 12 |
| CHARGEDUNIT / CANNON / HOME | 13 / 14 / 15 |
| POWERUNIT / MECH / NeutralCannon / Resource / RecoverPoint / SceneTrigger | 17 / 18 / 19 / 20 / 21 / 22 |
| PARTNER / DESTRUCTIBLE / PROTAL | 27 / 30 / 36 |
| MONSTER_GREEN / MainCity_PARTNER / NPC / MOUNT | 90 / 99 / 100 / 101 |
| NONE | 1000 |

⇒ **0=PLAYER、2=MONSTER、3=BOSS 与 1.5.9 完全一致**（规格 #1 第 6 条通过）。注意存在 APC=1 ⇒ 「!=2 就退阵营判据」的分支不要改成「>2」。


---

## 8. 规格卡必验项 → 2.0.1 实测结果

图例：✅ 通过（静态可判）/ ⚠️ 部分（名字在、语义未证）/ ❓ 未确认（括号里写缺什么证据）

### 卡 #1 秒杀小怪（10 条）

| # | 必验项 | 结果 | 证据 |
|---|---|---|---|
| 1 | CBattleActorUtil 存在；Hurt argc=5、静态 | ✅ | 0x02001DD4；Hurt 0x0601CAF9 argc=5 static |
| 2 | Hurt 帧从 pushframe 可见 | ❓ | 需 .so 侧重定位 pushframe RVA + 真机 `LEARN ok Hurt5`（本轮只做 dll） |
| 3 | CtagHurtHPPara 类名含 HurtHPPara | ✅ | 全名 `ActionGameLibrary.CtagHurtHPPara` |
| 4 | m_pTarget / m_pAttacker / m_llValue 存在、偏移 ≤0x400 | ⚠️ 名字 ✅；偏移（推断）0x10/0x18/0x20 | 声明序 #0/#1/#2；dll 无 FieldLayout ⇒ 偏移必须运行时按名取 |
| 5 | BigInt = double + int32 | ✅ | #0 `m_mantissa` double、#1 `m_exponent` int |
| 6 | CharacterType 0/2/3 | ✅ | PLAYER=0 / MONSTER=2 / BOSS=3 |
| 7 | 玩家身份链完整 | ⚠️ | ✅ `BattleElementCenter.gameObjectManager`(static)、`GameObjectManager._player`(SimpleCharacter)、`InteractiveGameObject._hitter`(Hitter)、`Hitter._camp`(int)、`SimpleCharacter._characterData`(CharacterData) 全在；❓ `ulRoleId` 具体字段名未逐字段核（只到 CharacterData 这一层） |
| 8 | 战斗 30s 内 STATS 打怪 ≥1 | ❓ | 需装机运行 |
| 9 | 首领不被波及 | ❓ | 需装机运行（静态侧只证 BOSS=3） |
| 10 | 血量量级重估 kKillMul | ❓ | 需 2.0.1 血量数值样本 |

### 卡 #2 无敌（9 条）

| # | 必验项 | 结果 | 证据 |
|---|---|---|---|
| 1 | StarsCollider + DoAttack argc=7 实例 | ✅ | 0x02001EB1；DoAttack 0x0601D175 argc=7 instance，形参序列与旧版逐字一致 |
| 2 | 帧从 pushframe 可见 | ❓ | 需真机 `LEARN ok DoAttack7` |
| 3 | 「accepter 空 ⇒ ret」分支仍在 | ❓ | 需 IL 反编译（本机无 ilspycmd/dnSpy/monodis；元数据定不了方法体） |
| 4 | accepter 仍是 IL 第 2 参、类型仍是 IAccept 系 | ✅（签名层） | DoAttack 第 2 形参 = `ActionGameLibrary.IAccept` |
| 5 | _interactiveGameObject 三候选至少命中一个 | ✅ | `Hitter._interactiveGameObject` 存在 |
| 6 | 玩家 _hitter 链成立 | ✅（结构层） | `InteractiveGameObject._hitter: Hitter` |
| 7 | ActionState 免疫位 4 个字段名在、1 字节、{0,1} | ✅ | #29/#30/#31/#36（另有 `immuneDebuff`），均 bool；偏移未确认 |
| 8 | 免疫位真被伤害判定读到 | ❓ | 需 IL / 真机 |
| 9 | 是否必须补 Character::Damage 层 | ❓ | 需真机（静态侧：两个重载都在 0x0601D9CB / 0x0601D9CC） |

### 卡 #3 全模式技能伤害（10 条）

| # | 必验项 | 结果 | 证据 |
|---|---|---|---|
| 1 | ActionCommand_AddProjectile + Execute argc=1 实例 | ✅ | 0x02001F1F；Execute 0x0601D432 argc=1 instance，形参 `ActionCommandData` |
| 2 | 帧从 pushframe 可见 | ❓ | 需真机 `LEARN ok ProjExec` |
| 3 | Execute 内 get_count() 仍是 17 参 AddProjectile 的循环上界 | ❓ | 需 IL（元数据侧：`LAddProjectile.get_count` 与 17 参 AddProjectile 0x0601D433 都在） |
| 4 | 17 参 AddProjectile 仍不读 _count | ❓ | 需 IL |
| 5 | _count 在、int32；ActionCommandData.data 在 | ✅ | #8 int32；`data` #1 |
| 6 | ActionCommandData.actionGameObject 在 | ✅ | #3 `IActionGameObject`（`action` #2 也在） |
| 7 | _count 语义仍是发射条数 | ❓ | 需真机（弹幕条数变化） |
| 8 | 敌方弹幕不受影响 | ❓ | 需真机回归 |
| 9 | 关闭瞬间不闪退 | ❓ | 需真机连续开关 5 次 |
| 10 | 扇出 3 / cap 120 / 每秒 600 重新标定 | ❓ | 需 2.0.1 帧率与同屏对象数 |

### 卡 #4 免广告（12 条，风险最高的一张）

| # | 必验项 | 结果 | 证据 |
|---|---|---|---|
| 1 | **广告 SDK 适配器类名（是否换 Bili 系）** | ✅ **风险不成立，仍是 MiaoYueSDKProxy** | `BaseSDKAdapter` 只有 2 个子类：MiaoYueSDKProxy（有 PlayADVideo）/ WeChatSDKProxy（无）；无 Bili 类型；`MiaoYueSDKProxy::PlayADVideo` = 0x06003342 argc=2 |
| 2 | BaseSDKAdapter::PlayADVideo 仍空桩、实现在子类 | ⚠️ | 结构成立（基类 0x060032B5 + 子类 0x06003342 两份）；「基类是空桩」是方法体语义 ⇒ 需 IL |
| 3 | DS_Advertising + OnGetADInfoRsp(1)/OnADVideoResult(3)/DispatchResult(2) | ✅ | 0x02000166；0x06001229 / 0x0600122B / 0x0600122E |
| 4 | start_watch_ad_rsp.result 语义未变（==2 跳过） | ❓ | 元数据只能证访问器齐备，语义需 IL/真机 |
| 5 | 协议访问器逐字存在 | ✅ | `get_start_watch_ad_rsp` / `get_result` / `get_ad_id` / `get_uuid`(ulong) / `set_result` 全在 |
| 6 | Proto_CSRsp 名含 Proto_CSRsp；EventItemInt 内偏移可被发现 | ✅（推断 +0x20 不变） | `proto_ff.Proto_CSRsp` 0x02001587；`ProtocolManager.EventItemInt.data` 为第 4 字段（#3）→ 0x20 |
| 7 | ProtocolManager::sendEvent(1) 存在且 pushframe 可见 | ⚠️ | 存在 0x06003194 argc=1；可见性 ❓ 需真机 |
| 8 | DataService.advertising 静态字段在 | ✅ | `DataService.advertising: DS_Advertising` static（#104/130） |
| 9 | 代回调签名 (bool,int,ulong)；runtime_invoke 返回 r+0x10 | ⚠️ | 签名 ✅；返回值约定是运行期行为 ❓ |
| 10 | 入口未换：WatchAdvertisements→StartWatchAdReq→OnGetADInfoRsp；奖励靠 cmd 2762 | ✅ 前半 / ❓ cmd 号 | 三方法都在（0x06001227/0x06001228/0x06001229）；cmd 号需协议表（可从 dll Constant 表读，本轮未做） |
| 11 | B 站 SDK 是否有 Java 层激励视频旁路（需去重门） | ❓ | `MiaoYueADCallBack.onRewardArrived(bool, AndroidJavaObject)` 仍在（0x060032E2）⇒ Java 回调链在；是否另起一路需读 dex |
| 12 | 真机验收两条同时成立 | ❓ | 需装机 |

### G 全局（只列与本任务相关的两条）

| # | 必验项 | 结果 | 证据 |
|---|---|---|---|
| G6 | 重取 2.0.1 dll、sha256 ≠ 7fa836ee、TypeDef/MethodDef 与命名空间直方图对比 | ✅ sha256 / 数量 / 直方图 Top20 已给；❓ 旧版完整直方图未留存（只能对比 4 个已知值：ActionGameLibrary 853=853、ActionData 118=118、proto_ff 2987→2850、GameConfigData 1812→1725） | 见 `3 |
| G8 | 1.5.9 四条「目标类.方法」全部存在 | ✅ 11/11 | 见 `5 |

---

## 9. 未确认清单（缺什么证据）

| 项 | 缺什么 |
|---|---|
| 所有 pushframe「帧可见性」（卡#1-2 / #2-2 / #3-2 / #4-7） | 2.0.1 `libil2cpp.so` 的 pushframe / 共享桩 RVA 重定位（G3/G4/G5）+ 真机 `LEARN ok …` 日志 |
| 卡#2-3、#3-3、#3-4、#4-2、#4-4（都是**方法体语义**） | **IL 反编译**：本机无 ilspycmd / dnSpy / monodis；元数据只有签名与字段，定不了方法体 |
| m_pTarget / m_pAttacker / m_llValue、ActionState 免疫位的**真实字节偏移** | `FieldLayout` 表 0 行 ⇒ dll 不含偏移；需 il2cpp `field_get_offset` 或 dump.cs（即 G1 的 AOT 侧工作） |
| `ulRoleId` 字段名 | CharacterData 未逐字段展开（本轮只到 `SimpleCharacter._characterData: CharacterData`） |
| cmd 2762 = finish_watch_ad_req | 需 `proto_ff.Proto_CS_CMD` 的 Constant 值（可从 dll Constant 表直接读，本轮未做） |
| B 站 Java 层广告旁路 | 需 dex 侧扫描（`TTRewardVideoActivity` 等），本轮范围只到 assets/dll |

---

## 10. 产物清单（`06_dumped/冒险与征途/bili_2.0.1/`）

| 文件 | 大小 | 说明 |
|---|---|---|
| `Assembly-CSharp.dll` | 21,671,936 | **主产物**，sha256 `ceab1132b321996a630e256d4750231a8920caa983ea26c0335e05de22b4f9ae` |
| `_body_95d30e2b.bin` | 21,674,764 | 该 bundle 完整解压体（可复现裁切） |
| `_body_9e2fe9a8.bin` | 5,319,716 | AOT 参考集（DOTween / UnityEngine.CoreModule / AssetBundleModule） |
| `_body_697730d0.bin` | 33,032 | AOT 参考集（AotUpdate） |
| `_body_748e419b.bin` / `_body_998eb4e9.bin` / `_body_14f6e1a8.bin` | 25.4M / 9.4M / 1.1M | 排除项（BSJB 假阳性），留作证据 |
| `_u3dlib.py` | — | UnityFS 解析原型（含 LZ4 块解压） |
| `_survey.py` | — | 全量 bundle 头字段普查（3,714/3,714 解析成功） |
| `_find_dll.py` | — | 全量解码 + PE/BSJB 定位（产出上表 body） |
| `_carve.py` | — | 按目录项裁切 + PE/CLR/元数据自校验 |
| `_mdread2.py` | 17,073 | 元数据读取器（复用 1.5.9 `aot/_mdread.py`，逻辑未改） |
| `_an1.py` … `_an10.py` | — | 本轮取数脚本（统计 / 类型 / 方法 / 字段 / 枚举 / 协议 / 兄弟程序集） |

**（推断）可行性结论**：2.0.1 的战斗与广告落点在**类名、方法名、argc、static/instance** 四个维度上与 1.5.9 完全同构 ⇒ 规格表的挂点方案不必改设计，只需（a）放弃 token 硬编码，（b）重新定位 .so 侧三个 RVA，（c）字段偏移按名取。
