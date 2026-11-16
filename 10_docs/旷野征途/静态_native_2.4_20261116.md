# 旷野征途 2.4 / vc24 · native 静态分析与挂点重定位（2026-11-16）

> 范围：**只做静态**。本轮未组包、未签名、未装机、未上机；真机只读（未 push/frida/install）。
> 输入底包：02_apks/旷野征途/旷野征途_2.4_taptap_official.apk（194,003,543 B，sha256 8d677670…e484e），解包面 13_scratch/kyzt24_base/。
> 参照口径：10_docs/旷野征途/路线锁定_2.2.md（§二 挂点表、§七-e / §七-f 战斗合同）。
> 方法：每条结论都按「静态分析 → 代码/IL 反推验证 → 结论」；未定位项写失败关闭，不填伪地址。

底包漂移（沿用 底包取证_2.4_20261116.md，本轮只复核与本任务相关的两项）：

| 项 | 2.2 | 2.4 | 说明 |
|---|---|---|---|
| libil2cpp.so | 65,520,080 B | **65,835,528 B** | .text 由 file 0x1748604/va 0x174c604 → **file 0x1769634/va 0x176d634**，va−file 仍 = **0x4000** |
| global-metadata.dat | 11,670,528 B | **11,692,792 B** | 仍 **v31**；Il2CppDumper 直读通过（CodeRegistration 0x3a32548 / MetadataRegistration 0x3b50c70） |

---

## ① 结论表

| # | 功能 | 目标 | 2.4 解析方式 | 2.4 值 | 验证状态 |
|---|---|---|---|---|---|
| 1 | 免广告 | Dirichlet.Ad.DirichletRewardAdBase::ShowInternal（Slot 5） | **按名可行**（推荐）；偏移可作备用 | 按名：ns Dirichlet.Ad / cls DirichletRewardAdBase / argc **1**；备用 file **0x1CCE6CC**（VA 0x1CD26CC），序言 **0xF81E0FFE** | 静态已验证（未上机） |
| 2 | 免广告（原生事件） | 同类 HandleNativeEvent（Slot 7） | 偏移 | file **0x1CCE444**（VA 0x1CD2444），首指令 **0xD10183FF**（sub sp,sp,#0x60） | 静态已验证（当前实现未挂此点，仅备用） |
| 3 | 游戏加速 | UnityEngine.Time::set_timeScale | **按名可行**（现状即按名，argc 1） | 备用 file **0x37541A0**（VA 0x37581A0），首指令 **0xFC1E0FE8** | 静态已验证 |
| 4 | 玩家无敌 / 百倍伤害 | InterpFrameGroup::EnterFrameFromInterpreter（解释器帧入口） | **只能偏移**（HybridCLR 运行时函数，无托管符号、无按名路径） | file **0x19C9A78**（VA 0x19cda78），序言 **0xF81D0FFE** | 静态已验证（合同待上机复核） |
| 5 | 经济三项（金币/钻石/积分） | AccountInfo / PlayerInfo / ItemSaveData / MemoryValueEncoding | **名字契约**（运行时 il2cpp_field_get_offset 现取，无需固化数字） | 四个类型全部存在，关键字段/属性/方法名齐全；**均不在 AOT dump**（在 HotUpdate.dll） | 静态已验证 |
| 6 | 热更层存在性 | HotUpdate.dll | 指纹重定位 | bundle 53a9548b7d255f7af5030cc4358b6808；PE@**937,776**；**3,181,568 B**；sha256 a350357a…9030 | 已验证（PE 头 + 程序集名 + 哈希实测） |

**一句话**：2.4 上「按名可解」的三项（免广告、加速、经济）不依赖任何硬编码地址；唯一天然需要偏移的是解释器帧入口（战斗两项），本轮已定位并给出同构证据。**全部未上机**。

---

## ② 每挂点证据段

### 2.0 公共工具与口径

- dump：01_tools/Il2CppDumper（命令 /Users/aliang/.dotnet/dotnet Il2CppDumper.dll <so> <metadata> <out>）→ 06_dumped/旷野征途/2.4/out/dump.cs（24,658,291 B，**11,969 类型 / 97,886 方法 / 13,485 字段偏移**，DummyDll 92 个）。
- 反汇编/扫描脚本（本轮新建，可复跑）：13_scratch/kyzt24_native/d.py（切片反汇编）、scan_xref.py（adrp+add 字符串 xref）、scan_callers.py、callers_full.py（bl 目标统计）、syms.py（dynsym 导出）、scan_bundles.py（bundle 扫描）、ildump/（Mono.Cecil 读 HotUpdate.dll：字段/属性/方法/IL）。
- 换算规则：**va − file = 0x4000**（两个版本相同）；框架 HookAtRva 的参数是 **file offset**（内部 FileOffToVa 扫 /proc/self/maps 可执行段），不是 VA。

### 2.1 免广告 DirichletRewardAdBase::ShowInternal

dump.cs（2.4，类定义在 dump.cs:672531）直接给出：

    // RVA: 0x1CD26CC Offset: 0x1CCE6CC VA: 0x1CD26CC Slot: 5
    protected override bool ShowInternal(DirichletAdShowOptions options) { }
    // RVA: 0x1CD2444 Offset: 0x1CCE444 VA: 0x1CD2444 Slot: 7
    internal override void HandleNativeEvent(DirichletNativeEvent nativeEvent) { }

序言与指令序列**与 2.2 逐条同构**（仅 adrp 页与 bl 目标重定位）：

| # | 2.2 @VA 0x1CAD130（file 0x1CA9130） | 2.4 @VA 0x1CD26CC（file 0x1CCE6CC） |
|---|---|---|
| 1 | str x30,[sp,#-0x20]!（0xF81E0FFE） | 同（0xF81E0FFE） |
| 2 | stp x20,x19,[sp,#0x10] | 同 |
| 3 | adrp x19,#0x3e87000 | adrp x19,#0x3ed4000 |
| 4 | ldrb w8,[x19,#0xdd5] | ldrb w8,[x19,#0xe05] |
| 5 | mov x20,x0 | 同 |
| 6 | tbnz w8,#0,#0x1cad15c | tbnz w8,#0,#0x1cd26f8 |
| 7 | adrp x0,#0x3bf4000 | adrp x0,#0x3c3f000 |
| 8 | ldr x0,[x0,#0x320] | ldr x0,[x0,#0x1a0] |
| 9 | bl #0x1abdbc0（惰性初始化） | bl #0x1ae317c |
| 10-11 | mov w8,#1 / strb w8,[x19,#+0xdd5] | mov w8,#1 / strb w8,[x19,#0xe05] |
| 12 | ldr x19,[x20,#0x18] | 同 |

**按名路径可用性（本轮实测）**

- libil2cpp.so 的 .dynsym 在 2.4 仍导出 **241 个 il2cpp_***，含 il2cpp_class_from_name(0x19c76c8)、il2cpp_class_get_method_from_name(0x19c76f0)、il2cpp_method_get_name、il2cpp_field_get_offset、il2cpp_runtime_invoke（2.2 同为 241 个）⇒ 框架 Il2CppApi 的 dlsym 主路径成立，无需落硬编码符号表。
- 框架链路（只读核对 旷野征途Mod.cpp 与 Unity/Il2CppApi.h）：Assemblies → ImageOf → ClassFromName(ns,name) → GetMethod(name, argc) → 取 *(void**)MethodInfo（methodPointer@0）→ IsExecutableRange 校验 → DobbyHook。AOT 方法的 methodPointer 是真实代码地址 ⇒ **免广告完全可以按名挂，不需要 2.4 偏移**；InitSlotAddrs 只在 HookAtRva 失败时兜底。
- 覆盖关系：dump 中 DirichletRewardVideoAd / DirichletRewardAd **不 override ShowInternal**（只有 .ctor / Create），overrider 是 Interstitial/Banner/Splash 三类（同属另一基类 DirichletAd）⇒ 挂基类实现即可覆盖激励视频广告。
- 类所属 image：Image 90 __Generated（Assembly-CSharp 面），命名空间 Dirichlet.Ad。

### 2.2 游戏加速 UnityEngine.Time::set_timeScale

dump.cs:396848 → RVA 0x37581A0 / Offset 0x37541A0。

| 2.2 @VA 0x37145F8（file 0x37105F8） | 2.4 @VA 0x37581A0（file 0x37541A0） |
|---|---|
| str d8,[sp,#-0x20]!（0xFC1E0FE8） | 同 |
| stp x30,x19,[sp,#0x10] | 同 |
| adrp x19,#0x3e94000 / ldr x0,[x19,#0xf10] | adrp x19,#0x3ee1000 / ldr x0,[x19,#0xf70] |
| mov v8.16b,v0.16b / cbnz x0,#+0x18 | 同 |
| adrp x0,#0xa48000 + add #0x96 → bl #0x1abdb7c | adrp x0,#0xa59000 + add #0x6c8 → bl #0x1ae3138 |
| ldp x30,x19 / mov v0.16b,v8.16b | 同 |

现状实现已是「rva=0 只走按名」（kSlots[0] = UnityEngine/Time/set_timeScale，argc 1）⇒ **2.4 无需改动**；偏移仅作 drift 兜底记录。

### 2.3 战斗：解释器帧入口（本任务最硬项）

**定位链条（三条独立证据 + 边界证据）**

1. **字符串锚 → 分配器本体**：AllocStackSlot@va 0xa1c7a5 全 .text **唯一** adrp+add 引用 → va **0x19cdb48**（file 0x19c9b48），落在 AllocStackSlot 的 raise 冷路径；由此得本体 **0x19cdaec**（与 2.2 的 +0x58 同型）。同法：AllocFrame@va 0xa3f7cf 唯一引用 va **0x19cdba0** → PushFrame 本体 **0x19cdb58**。
2. **调用拓扑（全 .text 穷举）**：bl AllocStackSlot **恰好 2 处**（0x19cdab0、0x19cdbe4）、bl PushFrame **恰好 2 处**（0x19cdab8、0x19cdbf4）——2.2 完全同型（2 处 / 2 处）。
3. **入口函数与同构**：取 0x19cdab0 所在函数头 0x19cda78，与 2.2 的 0x19a9004 **33 条指令逐条同构**（见下）；函数边界由前一条 ret（0x19cda74）界定，非拼接邻近。

    2.4  VA 0x19cda78   file 0x19c9a78     ← EnterFrameFromInterpreter（三特征全中）
      str x30,[sp,#-0x30]!        0xF81D0FFE  ← 序言（HookAtRva 校验字）
      stp x22,x21,[sp,#0x10] / stp x20,x19,[sp,#0x20]
      mov x21,x0 ; mov x0,x1 ; mov x19,x2 ; mov x20,x1     ; (group, methodInfo, argBase)
      bl  #0x1ae340c                                       ; RuntimeInitClassCCtor 形态（2.2: 0x1abde50）
      ldr x8,[x20,#0x58]                                   ; 特征① imi = method->interpData
      ldr x0,[x21] ; ldr w9,[x8,#0x2c] ; ldr w8,[x8,#0x20] ; imi->maxStackSize / argStackObjectSize
      ldr w22,[x0,#0xc] ; sub w1,w9,w8                     ; 特征① maxStack − argStackObjectSize
      bl  #0x19cdaec                                       ; 特征② AllocStackSlot
      ldr x0,[x21] ; bl #0x19cdb58                         ; 特征② PushFrame
      ldr x8,[x21] ; movi v0.2d,#0 ; ldr w8,[x8,#0x10]
      stp x20,x19,[x0]                                     ; frame->method / frame->argBase
      str w22,[x0,#0x10]                                   ; frame->oldStackTop
      stur q0,[x0,#0x18] ; stur q0,[x0,#0x28]              ; 3×nullptr 区
      str w8,[x0,#0x38]                                    ; 特征③ localPoolBottomIdx
      ret                                                  ; 返回 newFrame

4. **调用点分布（第 4 条独立证据）**：bl 0x19cda78 共 **16 处**；2.2 也是 **16 处**，且**相对位移逐一相同**（2.4 首 0x19dae7c、末 0x19dbe84，跨度 **0x1008**；2.2 首 0x19b6408、末 0x19b7410，跨度同为 0x1008）⇒ 对应同一个 Interpreter::Execute 内的 16 个 PREPARE_NEW_FRAME_FROM_INTERPRETER。
5. **区分 FromNative**：2.2 的 0x19a913c ↔ 2.4 的 **0x19cdbb0**（stp x30,x25,[sp,#-0x40]! 起、ldr w1,[x24,#0x2c] **无减法**、唯一调用者 0x19cf890，2.2 唯一调用者 0x19aae1c）——同型。
6. **内存布局自证**：PushFrame 内 add x0, x9, x8, lsl #6 ⇒ **InterpFrame = 0x40**（同 2.2）；AllocStackSlot 内 add x0,x10,w8,sxtw #3 ⇒ **StackObject = 8B**（同 2.2）。

**合同复核（2.4 是否仍成立）**

| 合同项 | 2.2 结论 | 2.4 复核 | 结论 |
|---|---|---|---|
| ApplyHp 存在且 argc=4 | 是 | Cecil：Battle.BattleRoleActor::ApplyHp(BattleRoleActor, Int32, BattleDamageResult, Action<BattleRoleActor>) **params=4** | 成立 |
| ApplyHp 方法体 | Code size 1265 | 2.4 **body=1265**，关键 IL 偏移一致：IL_0172 ldarg.0 / IL_0173 call get_isGod / IL_0178 brfalse / IL_017a ldfld dmg / IL_0180 ldc.r4 0 / IL_0185 bge.un.s / IL_0187 ret；IL_0197 get_curHp → IL_019f add → IL_01A0 set_curHp | **逐字节级同构（未改）** |
| 伤害符号 | 负值（curHp += intDmg） | 同上 IL 自证 | 成立 |
| dmgInfo 字段名 | dmg(float) | BattleDamageResult.dmg : Single 存在；另有 reallyDmg / intDmg / WasApplied（后者仍**不可用作去重**） | 成立 |
| argBase 槽位 | +0 this / +8 atker / +16 seat / +24 dmgInfo / +32 OnDie | 帧入口签名与 frame 布局同构；槽位属解释器参数打包约定，**静态无法独立复证**（见 ④） | 沿用 2.2，待上机 |
| 本机/非本机判定 | BattleRoleMgr.Instance.<player>k__BackingField（备选 LocalSeatNum） | 2.4 均存在：BattleRoleMgr.Instance : BattleRoleMgr static、<player>k__BackingField : BattleRoleActor、BattleRoleActor.get_LocalSeatNum、PartyBattleRuntime.get_LocalPlayer | 成立 |
| ApplyHp 是否 AOT | 否（解释方法） | AOT dump.cs 中 BattleRoleActor / ApplyHp **0 命中**（2.2 同为 0） | 仍只能走帧入口 |

**两条已否定的做法（本轮未重犯）**：① 不在 Tick 线程无节流跑 FindClassByShortName（2.4 侧仍要求 500ms 节流 + 可重试，Init 只在帧 hook 内懒执行）；② 不用全局 g_reenter 挡内层调用（只保留 thread_local 保护我们自己的 il2cpp 查询，orig 全路径必调）。

### 2.4 经济字段 / 编码契约

**关键更正（避免下一轮去 dump.cs 白找）**：AccountInfo / PlayerInfo / ItemSaveData / MemoryValueEncoding **都不在 AOT metadata**（2.4 dump.cs 中 class AccountInfo 0 命中，2.2 同样 0）——它们在 **HotUpdate.dll**（解释执行）。所以「从 dump.cs 取 @偏移」这条路本身不存在；**2.2 的 @96/@272/@328/… 也不是常量**。

证据（只读核对 Games/旷野征途Economy22.h）：字段位置一律在运行时用 il2cpp_field_get_offset 按名现取（L268 注释「全部用 FieldOffset(name) 现取，不写死数字」，实现 L280/317/341/404），文档里的数字只是**观测记录** ⇒ **2.4 只需名字存在**，P3 不用改任何契约常量。

2.4 HotUpdate.dll 实测（Cecil）：

| 类型 | 2.4 字段/属性/方法 | 与 2.2 对比 |
|---|---|---|
| AccountInfo | players : List<PlayerInfo>、encodedPoints : Int64(priv)、pointsKey : Int64(priv)、encodedGold : Int64(priv)、goldKey : Int64(priv)、hasSharedGold : Boolean(priv)；属性 points : Int64、sharedGold : Nullable<Int64>（含 setter） | 名字/类型**完全一致**；字段总量明显增多（新增 artifact / bossTower / ironFortress / enchant* 等），按名解析不受影响 |
| PlayerInfo | saveBags : List<ItemSaveData>、bags : List<…>(nonpub)、bagsInitialized : Boolean；ChangeGold(Int64, Boolean) -> Boolean、AddItem(Int32,Int32,EItemQuality,Func3,Func3) | 一致（ChangeGold 首参仍是 **Int64** —— 2.2 踩过按 int32 传的事故，勿重犯） |
| ItemSaveData | encodedCount : Int32(priv)、countKey : Int32(priv)、baseItemid : Int32、属性 count : Int32 | 一致；另新增 itemSaveid / isEquiping / auditTrail / limitEnhanceRefundToPaid 等 |
| MemoryValueEncoding | RotateKey(Int32)、RotateKey(Int64)、NextKey() : UInt32、sequence : Int32 static | 算法**逐条等价**（见下） |

RotateKey 2.4 IL（Cecil 原样，body=26 / 37）：

    RotateKey(Int32)  body=26
      ldarg.0 / brfalse.s → NextKey()        ; k = (key==0) ? NextKey() : key
      dup ; ldc.i4.s 13 ; shl ; xor          ; k ^= k << 13
      dup ; ldc.i4.s 17 ; shr.un ; xor       ; k ^= k >>> 17
      dup ; ldc.i4.5  ; shl ; xor            ; k ^= k << 5
      ret
    RotateKey(Int64) body=37
      k = (key==0) ? ((ulong)NextKey()<<32 | (ulong)NextKey()) : key
      k ^= k << 13 ; k ^= k >>> 7 ; k ^= k << 17 ; ret

与 2.2 反编译源码（13_scratch/kyzt22_login/MemoryValueEncoding.cs）**逐算子相同**（13/>>>17/<<5 与 13/>>>7/<<17）。NextKey 仍是 xorshift*（Interlocked.Add(ref sequence,-1640531527) → ^>>16 *2246822507 → ^>>13 *3266489909 → ^>>16，零值回退 2654435769）⇒ **key 不可自行推测，必须读现值再 RotateKey 递推**（2.2 已如此，保持）。

写法规约不变：key = RotateKey(key); enc = value ^ key（金币/积分为 Int64、钻石为 Int32，各一套）。

### 2.5 热更层 HotUpdate.dll

扫描方式：APK 内 assets/yoo/DefaultPackage/*.bundle 共 **108** 个 → UnityPy 逐个解 → 枚举 TextAsset → 按名 / PE 头筛。

| 项 | 2.2 | 2.4 |
|---|---|---|
| bundle | …/9e98f08cc22dcf9ca6f0054cf3f14c1d.bundle（PE@901,272） | assets/yoo/DefaultPackage/**53a9548b7d255f7af5030cc4358b6808.bundle**（2,110,150 B，sha256 9c954df0…b67b） |
| 容器路径 / path_id | 未记录 | container **Assets/Bundles/CodeHot/HotUpdate.dll.bytes**，path_id -451116252152059905，CAB-322f6f497e00f97ea35fc3d391695a8e |
| PE 偏移（序列化文件内） | 901,272 | **937,776**（0xE4F30；byte_start 937,752 + 24） |
| 大小 / sha256 | 3,042,816 B / 5a4cb3bd…cb8cbca | **3,181,568 B / a350357a0fdf9d9d685ce65281f00994e1f5c4712758d4aa6a0f1f43b5949030** |
| 伴随文件 | 未记录 | **HotUpdate.pdb 935,104 B / 5583f86a…d0a4**（bundle 内偏移 2,624）——后续可做 IL↔行号定位 |
| 程序集身份 | monodis --assembly = HotUpdate | Cecil：ASM HotUpdate v0.0.0.0，MVID 42eb3f74-68c8-42a1-bcb2-b61290b40d48，**2,293 类型** |

**是否仍为 HybridCLR 解释执行：是。** 三条依据：① DLL 是纯 IL 的 .NET PE（无 native 段），位于 CodeHot 代码热更目录；② AOT dump.cs 无 BattleRoleActor / ApplyHp（0 命中）⇒ 这些方法根本没被 AOT 编译；③ libil2cpp 2.4 仍含 HybridCLR 运行时特征：InterpFrameGroup 三件套字符串（AllocStackSlot / AllocFrame / AllocLocalloc）+ hybridclr doesn't support %s@va 0xa2243c（11 处引用）+ 帧入口 16 个 PREPARE_NEW_FRAME 调用点 ⇒ **第 4 项的存在性成立**。

---

## ③ 与 2.2 差异逐条

| 项 | 2.2 | 2.4 | 影响 |
|---|---|---|---|
| 免广告 ShowInternal | file 0x1CA9130 | file **0x1CCE6CC**（Δ +0x2559C） | 走偏移需改；走按名无关 |
| HandleNativeEvent | file 0x1CA8EA8 | file **0x1CCE444**（Δ +0x2559C） | 同上（本轮未挂） |
| set_timeScale | file 0x37105F8 | file **0x37541A0**（Δ +0x43BA8） | 按名，无需改 |
| 解释器帧入口 | VA 0x19a9004 / file 0x19a5004 | VA **0x19cda78** / file **0x19C9A78**（Δ +0x324A74） | **必须改**；InitSlotAddrs 目前只认 18/22 |
| AllocStackSlot / PushFrame | 0x19a9078 / 0x19a90e4 | **0x19cdaec / 0x19cdb58** | 仅作定位证据，不挂 |
| 版本探测结果 | DetectLibVersion 命中 0x1CA9130 序言 → ver=22 | 该处 2.4 字节 = **0xB4000D96**（不匹配）⇒ **ver=0「未知版本」**；1.8 探测点 0x1C8A9AC = 0x97F951DD 也不匹配（**无误判风险**） | ver=0 ⇒ 所有 RVA 归零、只留按名；而 NoAd 槽位 ns/cls/method 为空串 ⇒ **免广告在 2.4 会失败关闭**（见 ⑤） |
| HybridCLR 字符串 xref | 唯一 1 处 | **11 处** | 只能作区域锚，不能作唯一锚（本轮改用分配器拓扑，不依赖它） |
| HotUpdate.dll | 3,042,816 B | 3,181,568 B（+138,752，+4.6%）；bundle 名全变 | 指纹必须换（已换） |
| 经济字段偏移 | @96/@272/@280/@288/@328/@336/@376/@384；ItemSaveData baseItemid@180 / encodedCount@16 / countKey@20 | **不再需要**（按名现取）；字段名/类型/编码算法不变 | 契约常量零改动 |

---

## ④ 待验证项与风险（**全部未上机**）

1. **argBase 槽位（+0/+8/+16/+24/+32）在 2.4 未独立复证**。帧布局与入口指令序列同构，但槽位由「谁打包 argBase」决定（IL 驱动侧），静态无法单独证明。→ 上机第一件事：在帧 hook 里对首个命中的 ApplyHp 打日志（this/atker/dmg 指针 + object_class 比对 + dmg 值），确认三槽位与符号；未确认前不得宣称无敌/百倍有效。
2. **按名路径的运行时前提**：2.4 的 .dynsym 有 241 个 il2cpp_* 导出（静态已证），但 dlsym 是否被 anti-cheat 拦、il2cpp_class_get_method_from_name 是否被投毒，需运行期日志确认（框架已有 GetMethodScan 降级）。
3. **DetectLibVersion 未覆盖 vc24**：ver=0 时 NoAd 无 ns/cls（空串）⇒ 免广告不会自动按名安装（按名技术可行，是实现缺口，不是能力缺口）。
4. **虚方法挂点语义**：ShowInternal 是 slot 5 的 override，Dobby 打在实现入口；若将来出现「直接调用基类实现」的路径会绕过——2.4 静态未发现该形态（子类未 override），保留为观察项。
5. **百倍与 AoE**：同一 dmgInfo 是否被多次 ApplyHp 仍未证伪（2.2 遗留风险；2.4 IL 未变，结论照旧）；缓解仍是「thread_local 作用域内按 dmgInfo 指针去重」，不跨调用假设。
6. HotUpdate.pdb 已落盘但本轮未使用（未做 IL↔行号映射）；后续精确定位其它热更方法时这是最省力的入口。
7. 未做：idat 全量函数边界（本轮 capstone + 调用拓扑已足够）；未改任何 mod菜单 文件；未组包 / 签名 / 装机。

---

## ⑤ 给 P3（组包）的输入清单

**A. 必须改（否则 2.4 战斗 / 免广告不可用）**

1. Games/旷野征途Mod.cpp → InitSlotAddrs()：新增 ver **24** 分支
   - interp = **0x19C9A78u**（file offset；VA 0x19cda78，序言 0xF81D0FFE）
   - noad = **0x1CCE6CCu**（file offset；序言 0xF81E0FFE）——或按 A-3 直接改按名
   - handleNative = **0x1CCE444u**（file offset；首指令 0xD10183FF，当前未挂，仅记录）
2. DetectLibVersion()：增加 2.4 探针，建议「file 0x1CCE6CC 处 == 0xF81E0FFE」或「libil2cpp 文件大小 65,835,528」判定；**必须与 2.2(0x1CA9130) 探针互斥**（两处序言相同，靠偏移区分；已验证 2.4 在 0x1CA9130 处为 0xB4000D96，不误判）。
3. kSlots 的 NoAd 槽位：填 ns/cls/method = **Dirichlet.Ad / DirichletRewardAdBase / ShowInternal**，argc **1**；rva 保留 0（按名优先）。理由：HookByName 取 methodPointer 后过 IsExecutableRange，AOT 方法可用，2.4 即不依赖任何偏移。
4. KYZT_BATTLE_ENABLE 处：版本门槛由 ver == 22 扩为 **ver == 22 || ver == 24**（地址取 kRvaEnterInterp，不写字面量）。

**B. 不必改**

- Games/旷野征途Economy22.h：字段一律 FieldOffset(name) 现取，**契约常量零改动**；2.4 四类型字段/属性/方法名齐备。
- kSlotThis/kSlotAtker/kSlotDmg = 0/8/24、kIdxAtker/kIdxSeat/kIdxDmg/kIdxOnDie、static_assert 全部保持（check_battle_contract.py 不用动）。
- TimeScale 槽位不变（按名）；菜单 9 项位置对齐不变。

**C. 建议同时记录（不进代码）**

- 2.4 指纹表：libil2cpp 65,835,528 B / metadata 11,692,792 B / HotUpdate 3,181,568 B a350357a… / bundle 53a9548b…。
- 战斗两项无按名路径（解释方法的 methodPointer 是标记值，框架会拒绝 hook）——这是设计，不是缺陷。

---

## 附：本轮脚本与产物

| 路径 | 内容 |
|---|---|
| 06_dumped/旷野征途/2.4/out/dump.cs | 2.4 IL2CPP dump（公共输入） |
| 06_dumped/旷野征途/2.4/out/DummyDll/ | 92 个 dummy 程序集 |
| 13_scratch/kyzt24_native/HotUpdate.dll | 从 bundle 解出的 2.4 热更程序集（3,181,568 B） |
| 13_scratch/kyzt24_native/d.py、scan_xref.py、scan_callers.py、callers_full.py、syms.py | 反汇编与扫描工具 |
| 13_scratch/kyzt24_native/scan_bundles.py、bundle_scan.log | 108 bundle 扫描结果 |
| 13_scratch/kyzt24_native/bundle_offset*.py | HotUpdate.dll 在 bundle / 序列化文件内的偏移 |
| 13_scratch/kyzt24_native/ildump/ | Mono.Cecil 小工具（dump / sig / il 三个子命令） |
