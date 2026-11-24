# 旷野征途 3.2 / vc32 · native 静态分析与挂点重定位（2026-11-24）

> 范围：**只做静态**。本轮未组包、未签名、未装机、未上机、未 hook；未改任何 .cpp/.h。
> 输入底包：02_apks/旷野征途/旷野征途_3.2_taptap_official.apk（198,941,707 B，sha256 6318a972…df1d），解包面 13_scratch/kyzt32_base/。
> 操作模板：10_docs/旷野征途/静态_native_2.4_20261116.md（逐项照做，逐项对照）。
> 方法：每条结论按「静态分析 → 代码反推验证 → 结论」；证据不足一律写「未定位/待验证」，不填伪地址。

---

## 0. 一句话结论

四个 AOT/HybridCLR 挂点在 3.2 **全部重新定位成功**（序言与 2.4 逐字节同构，调用拓扑/调用点数/跨度/相对位移全部复现），**但战斗与经济的 IL 合同本轮无法复核**：3.2 把 CodeHot 的 HotUpdate.dll 从「裸 PE 明文」改成了「签名 + AES 封装」（新发现的 ET.HotUpdateCodePackage v2），明文不可静态取得，且密钥未解出。因此 ApplyHp / RotateKey 一类合同只能标「未验证」，沿用 2.2/2.4 结论，不得当作已复核。

---

## 1. 底包核心面指纹（本轮实测）

| 项 | 3.2 实测 | 2.4 对照 | 结论 |
|---|---|---|---|
| APK | 198,941,707 B / sha256 6318a972a599cfa664c1db867b960fc1a5f9b878d9bad3c15b003d28da17df1d | 194,003,543 B | 新底包 |
| libil2cpp.so | **66,268,112 B** / sha256 **b249a13111e87c82ca595d374a10da5c4c972e3dd94bac2fd0540f07b65e332b** | 65,835,528 B | 指纹必须换 |
| global-metadata.dat | **11,782,304 B** / sha256 **9a106895fdf32adb7a77528962adc3576c8124898fe829d3a4d0f31da8428ddd** | 11,692,792 B | 指纹必须换 |
| metadata 版本 | 头部 sanity 0xFAB11BAF，version 字段 **0x1F = 31**（仍是 v31） | v31 | 不变 |
| Il2CppDumper 自动判定 | Il2Cpp Version 29 → 自动改判 **29.1**（日志有 WARNING: find JNI_OnLoad / This file may be protected） | 同 | 解析通过 |
| CodeRegistration | **0x3a95238** | 0x3a32548 | 漂移 +0x62CF0 |
| MetadataRegistration | **0x3bb57d0** | 0x3b50c70 | 漂移 +0x64B60 |
| 可执行段（实测 PT_LOAD） | .text off 0x1793714 / va 0x1797714 / size 0x492a40；**il2cpp** off 0x1c26154 / va 0x1c2a154 / size 0x1e64b44；.plt off 0x3a8aca0 | .text va 0x176d634 起 | 3.2 的**游戏代码主体在名为 il2cpp 的第二可执行段**，不在 .text |
| **va − file** | .text 段 +0x4000，**il2cpp 段也 +0x4000**（两段实测一致）；rodata 段（file < 0x1793714）+0 | +0x4000 | **仍为 0x4000，已实测**（4 个挂点全部落在 +0x4000 区） |
| .dynsym | 命名符号 2,693 个，其中 **il2cpp_* = 241** | 241 | 按名路径仍成立 |
| dump.cs | 25,786,132 B / sha256 3bdecf5da5a3c213dc6ea6a23dfd167b8a9087e346b679981a2e9ed336c8000f；**12,051 类型 / 90,037 条带 RVA 的方法 / 92 个 image（Image 0–91，91 = __Generated）** | 11,969 类型 / 97,886 方法 / 92 image | 类型 +82 |

关键导出（本轮实测地址，供 Il2CppApi 核对）：
il2cpp_class_from_name 0x19f3fb4、il2cpp_class_get_method_from_name 0x19f3fdc、il2cpp_class_get_methods 0x19f3fd8、il2cpp_field_get_offset 0x19f467c、il2cpp_method_get_name 0x19f47f0、il2cpp_runtime_invoke 0x19f48d0、il2cpp_domain_get 0x19f44ac、il2cpp_domain_get_assemblies 0x19f44b8、il2cpp_assembly_get_image 0x19f3f7c、il2cpp_thread_attach 0x19f4908。

**dump 生成命令（可复现）**

    cd /e/nixiang
    dotnet 01_tools/il2cppdumper/Il2CppDumper.dll \
      13_scratch/kyzt32_base/lib/arm64-v8a/libil2cpp.so \
      13_scratch/kyzt32_base/assets/bin/Data/Managed/Metadata/global-metadata.dat \
      06_dumped/旷野征途/3.2/out
    # 输入 sha256：libil2cpp b249a131…e332b / metadata 9a106895…28ddd
    # 产物 sha256：dump.cs 3bdecf5d…600f（25,786,132 B；本仓 01_tools/il2cppdumper/config.json 只出 dump.cs，不出 DummyDll）

---

## 2. 四挂点表（本轮 3.2 结论）

| # | 功能 | 目标 | 3.2 file offset | 3.2 VA | 首 4 条指令（机器码小端字） | 置信度 |
|---|---|---|---|---|---|---|
| a | 免广告 | Dirichlet.Ad.DirichletRewardAdBase::ShowInternal（Slot 5，argc 1） | **0x1CFC6D0** | 0x1D006D0 | 0xF81E0FFE / 0xA9014FF4 / 0xD00111F3 / 0x395F5668 | 高（dump 符号 + 逐指令同构） |
| b | 免广告原生事件 | 同类 HandleNativeEvent（Slot 7） | **0x1CFC448** | 0x1D00448 | 0xD10183FF / 0xA90267FE / 0xA9035FF8 / 0xA90457F6 | 高（dump 符号 + 同构） |
| c | 游戏加速 | UnityEngine.Time::set_timeScale（argc 1） | **0x37B27A8** | 0x37B67A8 | 0xFC1E0FE8 / 0xA9014FFE / 0xB0003CB3 / 0xF9451260 | 高（dump 符号 + 同构） |
| d | 玩家无敌 / 百倍伤害 | InterpFrameGroup::EnterFrameFromInterpreter（HybridCLR 帧入口，无托管符号） | **0x19F6364** | **0x19FA364** | 0xF81D0FFE / 0xA90157F6 / 0xA9024FF4 / 0xAA0003F5 | 高（三条互相独立的拓扑证据 + 逐指令同构） |

与 2.4 的位移：a/b 同为 **+0x2E004**，c 为 **+0x5E608**，d 为 **+0x2C8EC**。

### 2.a ShowInternal（dump 直接给符号，证据链＝符号 + 同构）

dump.cs:707781 附近给出：

    // RVA: 0x1D006D0 Offset: 0x1CFC6D0 VA: 0x1D006D0 Slot: 5
    protected override bool ShowInternal(DirichletAdShowOptions options) { }

反汇编（file 0x1CFC6D0）逐条与 2.4 同构，只有 adrp 页/立即数与 bl 目标重定位：

| # | 2.4 @VA 0x1CD26CC | 3.2 @VA 0x1D006D0 |
|---|---|---|
| 1 | str x30,[sp,#-0x20]!（0xF81E0FFE） | 同 |
| 2 | stp x20,x19,[sp,#0x10] | 同 |
| 3 | adrp x19,#0x3ed4000 | adrp x19,#0x3f3e000 |
| 4 | ldrb w8,[x19,#0xe05] | ldrb w8,[x19,#0x7d5] |
| 5 | mov x20,x0 | 同 |
| 6 | tbnz w8,#0,→0x1cd26f8 | tbnz w8,#0,→0x1d006fc |
| 7 | adrp x0,#0x3c3f000 / ldr x0,[x0,#0x1a0] | adrp x0,#0x3ca4000 / ldr x0,[x0,#0xc40] |
| 8 | bl #0x1ae317c（惰性初始化） | bl #0x1b11190 |
| 9 | mov w8,#1 / strb w8,[x19,#+0xe05] | mov w8,#1 / strb w8,[x19,#+0x7d5] |
| 10 | ldr x19,[x20,#0x18] | 同 |
| 11 | cbz x19,→末尾 | 同（→0x1d00778） |

第二条独立证据：按名可行 —— 241 个 il2cpp_* 导出仍在，class_from_name / class_get_method_from_name 地址已实测（§1）。AOT 方法的 methodPointer 是真实代码地址，所以「按名挂」与「按偏移挂」两条路都通；偏移仅作 ver 探测命中后的兜底。

### 2.b HandleNativeEvent

    // RVA: 0x1D00448 Offset: 0x1CFC448 VA: 0x1D00448 Slot: 7
    internal override void HandleNativeEvent(DirichletNativeEvent nativeEvent) { }

首指令 0xD10183FF（sub sp,sp,#0x60）与 2.4 的 0xD10183FF **完全一致**；其后 stp x30,x25 / x24,x23 / x22,x21 / x20,x19 的栈布局（#0x20/#0x30/#0x40/#0x50）也与 2.4 一致。当前实现不挂此点，仅记录。

### 2.c set_timeScale

    // RVA: 0x37B67A8 Offset: 0x37B27A8 VA: 0x37B67A8
    public static void set_timeScale(float value) { }

| 2.4 @VA 0x37581A0 | 3.2 @VA 0x37B67A8 |
|---|---|
| str d8,[sp,#-0x20]!（0xFC1E0FE8） | 同 |
| stp x30,x19,[sp,#0x10] | 同 |
| adrp x19,#0x3ee1000 / ldr x0,[x19,#0xf70] | adrp x19,#0x3f4b000 / ldr x0,[x19,#0xa20] |
| mov v8.16b,v0.16b / cbnz x0,+0x18 | 同 |
| adrp x0,#0xa59000 + add #0x6c8 → bl #0x1ae3138 | adrp x0,#0xa6a000 + add #0x802 → bl #0x1b1114c |
| str x0,[x19,#0xf70] | str x0,[x19,#0xa20] |
| ldp x30,x19 / mov v0.16b,v8.16b | 同 |

现状实现本来就是「rva=0 只走按名（UnityEngine/Time/set_timeScale, argc 1）」⇒ **阶段3 无需改**，本节仅作漂移记录。

### 2.d EnterFrameFromInterpreter（本轮最硬项，四条独立证据）

**证据 1 · 字符串锚 → 分配器本体（全可执行区唯一 xref）**

| 字符串 | file 偏移 | 全可执行区 xref | 结论 |
|---|---|---|---|
| AllocStackSlot | 0xA2C331 | **1 处**，site VA 0x19FA434 | 落在 AllocStackSlot 的 raise 冷路径 ⇒ 本体 **VA 0x19FA3D8**（file 0x19F63D8） |
| AllocFrame | 0xA502A0 | **1 处**，site VA 0x19FA48C | 落在 PushFrame 的 raise 冷路径 ⇒ 本体 **VA 0x19FA444**（file 0x19F6444） |
| AllocLocalloc | 0xA827F0 | **1 处**，site VA 0x1A1066C | 本轮未用（localloc 路径） |

**证据 2 · 调用拓扑（穷举全可执行区 9,168,579 条指令里的 1,056,976 个 bl）**

- bl AllocStackSlot(0x19FA3D8) **恰好 2 处**：0x19FA39C、0x19FA4D0
- bl PushFrame(0x19FA444) **恰好 2 处**：0x19FA3A4、0x19FA4E0
- 其中 0x19FA39C/0x19FA3A4 这一对就是目标帧入口；0x19FA4D0/0x19FA4E0 是「无减法」变体
- 与 2.4 完全同型（2 处 / 2 处）

**证据 3 · 入口函数与 33 条指令逐条同构**

    3.2  VA 0x19FA364  file 0x19F6364   ← EnterFrameFromInterpreter（三特征全中）
      str x30,[sp,#-0x30]!        0xF81D0FFE  ← 序言（HookAtRva 校验字，与 2.4 同值）
      stp x22,x21,[sp,#0x10] / stp x20,x19,[sp,#0x20]
      mov x21,x0 ; mov x0,x1 ; mov x19,x2 ; mov x20,x1          ; 寄存器分配与 2.4 完全一致
      bl  #0x1b11420                                            ; RuntimeInitClassCCtor 形态（2.4: 0x1ae340c）
      ldr x8,[x20,#0x58]                                        ; 特征① imi = method->interpData
      ldr x0,[x21] ; ldr w9,[x8,#0x2c] ; ldr w8,[x8,#0x20] ; ldr w22,[x0,#0xc]
      sub w1,w9,w8                                              ; 特征① maxStackSize − argStackObjectSize
      bl  #0x19FA3D8                                            ; 特征② AllocStackSlot
      ldr x0,[x21] ; bl #0x19FA444                              ; 特征② PushFrame
      ldr x8,[x21] ; movi v0.2d,#0 ; ldr w8,[x8,#0x10]
      stp x20,x19,[x0]                                          ; frame->method / frame->argBase
      str w22,[x0,#0x10]                                        ; frame->oldStackTop
      stur q0,[x0,#0x18] ; stur q0,[x0,#0x28]                   ; 3×nullptr 区
      str w8,[x0,#0x38]                                         ; 特征③ localPoolBottomIdx
      ret                                                       ; 返回 newFrame

函数边界：前一条指令 0x19FA360 是上一函数的 ret，不存在「拼接邻近」⇒ 0x19FA364 是干净函数头。

**证据 4 · 调用点分布**

- bl 0x19FA364 共 **16 处**：首 0x1A07768、末 0x1A08770，跨度 **0x1008**
- 2.4 同为 16 处、跨度同为 0x1008；且**入口→首调用点的相对位移两版同为 +0xD404**（2.4：0x19CDA78→0x19DAE7C；3.2：0x19FA364→0x1A07768）
- 对应同一 Interpreter::Execute 内的 16 个 PREPARE_NEW_FRAME_FROM_INTERPRETER

**区分变体（不采用，仅记录）**：0x19FA49C（stp x30,x25,[sp,#-0x40]! 起，ldr w1,[x24,#0x2c] **无减法**，全库 bl 命中 **1 处** = 0x19FC17C；2.4 对应项也是「唯一调用者」，且两版 入口↔调用者 距离同为 **0x1CE0**）。3.2 该函数尾部比 2.4 多一段 switch（cmp w8,#8 + 跳转表），形态变化，本轮不使用。

**内存布局自证（与 2.2/2.4 相同）**：PushFrame 内 add x0,x9,x8,lsl #6 ⇒ InterpFrame = 0x40；AllocStackSlot 内 add x0,x10,w8,sxtw #3 ⇒ StackObject = 8 B。

**HybridCLR 运行时仍在**：AllocStackSlot / AllocFrame / AllocLocalloc 三串各 1 份，hybridclr doesn't support（file 0xA32410）1 份，帧入口 16 个调用点，分配器读 interpData(+0x58)/maxStackSize(+0x2c)/argStackObjectSize(+0x20) 三个偏移 —— 结构未变。

---

## 3. 战斗合同复核 —— **阻塞，未验证**

| 合同项 | 2.4 值 | 3.2 本轮 | 判定 |
|---|---|---|---|
| Battle.BattleRoleActor 在 AOT dump 存在 | 0 命中 | **0 命中**（rg BattleRoleActor = 0） | 一致（仍是解释方法） |
| ApplyHp argc=4 / body=1265 / IL 偏移 | 已测 | **无法测** | **未验证** |
| BattleRoleMgr / get_LocalSeatNum / PartyBattleRuntime.get_LocalPlayer | 2.4 侧在 HotUpdate.dll 内证 | **AOT 0 命中**，HotUpdate 侧不可读 | **未验证** |
| argBase 槽位 +0/+8/+16/+24/+32 | 沿用 2.2 | 同上 | 沿用，待上机 |

**阻塞原因（本轮新发现，属硬事实）**：3.2 的 HotUpdate.dll 不再是明文 PE。

- APK 内 115 个 assets/yoo/DefaultPackage/*.bundle 全量扫描：**没有任何 bundle 含明文 PE（MZ\x90\x00\x03）或 CLI 头（BSJB）**；唯二的 HotUpdate 命中的另 3 个 bundle 只是 prefab 文案里出现该词。
- 承载 bundle 的 TextAsset 载荷首字节 0x02（不是 'M'），整体高熵。

---

## 4. 经济契约复核

| 项 | 2.4 | 3.2 本轮 | 判定 |
|---|---|---|---|
| AccountInfo / ItemSaveData / MemoryValueEncoding 在 AOT dump | 0 命中 | **0 命中** | 一致（仍在 HotUpdate.dll） |
| PlayerInfo | 2.4 记「0 命中」 | **1 命中，但是 TapSDK.OnlineBattle.PlayerInfo**（dump.cs:710477，字段 id/status/customStatus/customProperties） | 与经济学 PlayerInfo 无关；**按名路径必须区分命名空间**（名字契约用 ET.PlayerInfo / 全名匹配，别用短名裸匹配） |
| RotateKey 语义（Int32 13/>>>17/<<5；Int64 13/>>>7/<<17） | 已逐算子核对 | **无法复核**（DLL 不可读） | **未验证** |
| 经济代码是否需改 | 不改（FieldOffset 运行期现取） | 同上（Games/旷野征途Economy22.h 注释明确「不写死数字」） | 不改 |

注：PlayerInfo 同名不同类是 3.2 一个真实陷阱 —— 若运行时用「短名 + 无命名空间」找类，可能命中 TapSDK 的 PlayerInfo。

---

## 5. HotUpdate 指纹（3.2）—— 明文不可得，改为封装指纹

| 项 | 2.4 | 3.2 本轮实测 |
|---|---|---|
| 承载 bundle | 53a9548b…bundle（2,110,150 B） | **assets/yoo/DefaultPackage/35c70c79c548d5fe9bf0428475675b15.bundle**（3,585,765 B，sha256 **77cb0837570243ef10571d26b3b268abad5e0d49c49cdc50b5ac4d2170712f1a**） |
| container / path_id | Assets/Bundles/CodeHot/HotUpdate.dll.bytes / -451116252152059905 | **同**（Assets/Bundles/CodeHot/HotUpdate.dll.bytes，path_id -451116252152059905） |
| SerializedFile | CAB-322f6f497e00f97ea35fc3d391695a8e | **CAB-322f6f497e00f97ea35fc3d391695a8e，Unity 2022.3.62f3c1** |
| 载荷 | 明文 PE @937,776；3,181,568 B；sha256 a350357a…9030 | **封装包 3,583,905 B，sha256 728f1fc340d3b7b8977818d48b5f44d9157b32437ea42c680f107b70ab19cb29，首字节 0x02** |
| PE 偏移 / DLL 字节数 / DLL sha256 | 有 | **无（不存在于包内）** |

**封装格式的证据链（静态可证）**：AOT 里有 ET.HotUpdateCodePackage（dump.cs:672281，TypeDefIndex 10299）：

    public const int FormatVersion = 2;
    private const int IvLength = 16;
    private const int SignatureLength = 384;
    private const int HeaderLength = 17;
    private static readonly byte[] EncryptionKey;
    private const string PublicModulus = "vwZcqjBY…UWs1";   // RSA-2048
    private const string PublicExponent = "AQAB";
    ReadCodeAsset(byte[] package, string assetName, long versionCode)   // file 0x2BA3A64
    LoadAssembly(Dictionary<string,byte[]> files, long versionCode)     // file 0x2BA3FA8
    ComputeSignatureHash(package, signedLength, assetName, versionCode) // file 0x2BA44EC
    VerifyReleaseSignature(hash, signature)                             // file 0x2BA48C8

反汇编 ReadCodeAsset（file 0x2BA3A64）读出的包布局，与实测字节逐项吻合：

| 反汇编事实 | 实测 |
|---|---|
| len = package.Length；要求 len ≥ 0x191(401) 且 (len−401) % 16 == 0 | 3,583,905 − 401 = **3,583,504 = 16 × 223,969** ✔ |
| package[0] 必须 == 2（FormatVersion） | 首字节 **0x02** ✔ |
| 签名 = 末 0x180(384) 字节（Array.Copy(src, len−0x180, sig, 0, 0x180)） | 已按此切分 |
| IV = package[1..17)（Array.Copy(src, 1, iv, 0, 16)） | HeaderLength 17 = 1 + 16 ✔ |
| Key 取静态字段 EncryptionKey（[class+0xb8][0]），随后交给对称算法 set_Key | — |
| 先 ComputeSignatureHash → VerifyReleaseSignature，失败即抛 | RSA-2048 验签（PublicModulus/Exponent 常量在 dump 里） |

**密钥恢复失败（已试，记录以免下轮重跑）**：从 global-metadata.dat 抽出 7,526 个 base64 风格候选串，分别按 utf8 / base64 decode / hex 三种方式取 key（16/24/32 字节），以 IV = package[1:17] 做 AES-CBC 解首块，判据 plaintext 前 2 字节 == 'MZ'（或含 BSJB）⇒ **0 命中**。说明 EncryptionKey 不是「明文写死的 base64 字面量直给」，而是经 .cctor（file 0x2BA4BD0）二次构造或来自更深的派生。

**给阶段2/3 的两条可行路**（都指向同一个 AOT 函数，argc=3，可只读取返回的 byte[]，写入磁盘）：
1. 运行时：hook **ET.HotUpdateCodePackage::ReadCodeAsset**（file 0x2BA3A64 / VA 0x2BA7A64，AOT 方法，可按名挂），在其返回处 dump assetName=HotUpdate.dll 的明文。
2. 静态：完整逆向 ReadCodeAsset 剩余分支 + .cctor 的 EncryptionKey 构造（需解析 MetadataUsage 取字面量，建议用 Il2CppDumper 的 stringliteral / script.json 辅助）。

---

## 6. 与 2.4 的差异总表

| 项 | 2.4 | 3.2 | 影响 |
|---|---|---|---|
| libil2cpp | 65,835,528 B | 66,268,112 B（+432,584） | 指纹必须换 |
| metadata | 11,692,792 B | 11,782,304 B（+89,512），仍 v31 | 指纹必须换 |
| ShowInternal | file 0x1CCE6CC | file **0x1CFC6D0**（+0x2E004） | 走偏移要改；按名无关 |
| HandleNativeEvent | file 0x1CCE444 | file **0x1CFC448**（+0x2E004） | 同上（现不挂） |
| set_timeScale | file 0x37541A0 | file **0x37B27A8**（+0x5E608） | 按名，无需改 |
| 解释器帧入口 | VA 0x19CDA78 / file 0x19C9A78 | VA **0x19FA364** / file **0x19F6364**（+0x2C8EC） | **必须改**；否则战斗两项失败关闭 |
| AllocStackSlot / PushFrame | 0x19CDAEC / 0x19CDB58 | **0x19FA3D8 / 0x19FA444** | 仅定位证据，不挂 |
| 调用点数 / 跨度 | 16 / 0x1008 | 16 / 0x1008 | 同型 |
| HybridCLR 特征串 | 3 串各 1 份 | 同（各 1 份） | 存在性成立 |
| HotUpdate.dll | 明文 PE 3,181,568 B | **签名+AES 封装 3,583,905 B** | **指纹口径要改；IL 合同无法静态复核** |
| 版本探测 | ver=0（未知） | 仍 ver=0（见下） | 免广告靠按名、战斗必挂必须加 3.2 分支 |

**探测点实测（3.2 libil2cpp 上读出的 4 字节）**：

| 探针 file 偏移 | 含义 | 3.2 处实际值 | 是否等于 0xF81E0FFE |
|---|---|---|---|
| 0x1CA9130 | 2.2 ShowInternal | 0xF9400104 | 否 |
| 0x1CCE6CC | 2.4 ShowInternal | 0x97F8FAB6 | 否 |
| 0x1C8A9AC | 1.8 ShowInternal | 0xF945BC00 | 否 |
| **0x1CFC6D0** | **3.2 ShowInternal** | **0xF81E0FFE** | **是** |

⇒ 现版本 DetectLibVersion 在 3.2 上返回 0（未知），所有 RVA 归零：免广告仍可按名装（kSlots 已带三要素），**战斗帧入口不会被装 ⇒ 无敌/百倍静默失效**。这是阶段3 的唯一硬阻塞点。

---

## 7. 给阶段3 的必改项清单（对照 Games/旷野征途Mod.cpp）

**A. 必改**

1. kSlots 的 NoAd 槽位：rva 由 InitSlotAddrs 填 **0x1CFC6D0u**；expectInsn 仍 0xF81E0FFEu；ns/cls/method = Dirichlet.Ad / DirichletRewardAdBase / ShowInternal，argc 1（不变）。
2. InitSlotAddrs()：新增 **ver == 32** 分支
   - interp = **0x19F6364u**（file；VA 0x19FA364，序言 0xF81D0FFE）
   - noad   = **0x1CFC6D0u**（file；VA 0x1D006D0）
   - handleNative = **0x1CFC448u**（file；VA 0x1D00448，首指令 0xD10183FF；仍只记录不挂）
3. DetectLibVersion()：探针表最前面加 **{32, 0x1CFC6D0u}**；建议再加一条 AND 条件 **{32, 0x19F6364u} 处 == 0xF81D0FFE**（双点同时成立才认 3.2），并用 **libil2cpp 文件大小 66,268,112** 作兜底判据（最稳，不受段内偶然序言影响）。
4. g_libVer 取值域与 KYZT_BATTLE_ENABLE 门槛：由「ver == 22 || ver == 24」扩为 **|| ver == 32**（地址一律取 kRvaEnterInterp，不写版本字面量）。

**B. 不必改**

- TimeScale 槽位（按名，rva 0）。
- Games/旷野征途Economy22.h 的契约常量（FieldOffset(name) 现取）。
- kSlotThis/kSlotAtker/kSlotDmg = 0/8/24 等战斗槽序常量（但见 §8 未验证项）。
- ET.GameManagerMono.Update 帧锚点：3.2 仍存在（dump.cs，file **0x2B7E3B8**，VA 0x2B823B8，argc 0）。

**C. 建议记录（不进代码）**

- 3.2 指纹：libil2cpp 66,268,112 B / b249a131…；metadata 11,782,304 B / 9a106895…；dump.cs 3bdecf5d…。
- HotUpdate 侧：bundle 35c70c79…，载荷 3,583,905 B / 728f1fc3…（**封装包，不是 DLL**）。
- 若需要 IL 级合同复核：走 §5 的两条路，不要再去 bundle 里找「PE 偏移」。

---

## 8. 未定位 / 未验证项（明确失败关闭）

1. **HotUpdate.dll 明文未取得** ⇒ ApplyHp 的 argc/body/IL 偏移、RotateKey 语义、BattleRoleActor 成员表、PlayerInfo/ItemSaveData/AccountInfo 字段全部**未验证**。禁止把 2.2/2.4 的数字当 3.2 已复核结果使用。
2. **探针互斥性未实测**：2.2 / 2.4 的 libil2cpp.so 已不在仓（06_dumped/旷野征途 整目录缺失、02_apks 只剩 3.2），无法确认「2.4 库在 0x1CFC6D0 处 ≠ 0xF81E0FFE」。⇒ 所以建议用**文件大小**做主判据，序言做副判据。
3. argBase 槽位（+0/+8/+16/+24/+32）仍只有 2.2 的上机结论，3.2 静态无法独立复证。
4. 百倍与 AoE 的 dmgInfo 重复命中问题（2.2 遗留）未证伪。
5. 本轮的「短名匹配」风险：PlayerInfo 在 3.2 AOT 有同名 TapSDK 类，经济侧按名查找必须带命名空间/全名。
6. 未做：idat 全量函数边界（capstone 拓扑已足够）；未改任何 mod菜单 / Games / 04_scripts / pipeline 文件；未上机。
7. 横向风险（引自 10_docs/旷野征途/底包取证_3.2_20261124.md，非本轮实测）：设备为雷电 x86_64 + Houdini 翻译层执行 arm64 ⇒ **native hook 在模拟器是否可用尚无运行期证据**；本轮的四个地址是「ARM64 文件偏移」，Houdini 翻译后代码地址与文件偏移的映射未经实测，阶段3 首装必须先看 hook 命中日志再谈功能。

---

## 9. 本轮脚本与产物（可复跑）

| 路径 | 内容 |
|---|---|
| 06_dumped/旷野征途/3.2/out/dump.cs | 3.2 IL2CPP dump（25,786,132 B，sha256 3bdecf5d…600f） |
| 13_scratch/kyzt32_base/lib/arm64-v8a/libil2cpp.so | 从 APK 只读提取（unzip -p，未解整包） |
| 13_scratch/kyzt32_base/assets/bin/Data/Managed/Metadata/global-metadata.dat | 同上 |
| 13_scratch/kyzt32_native/d.py | 切片反汇编（capstone，file offset → 指令） |
| 13_scratch/kyzt32_native/syms.py | .dynsym 段/导出统计（il2cpp_* 计数与关键地址） |
| 13_scratch/kyzt32_native/scan.py | 字符串锚 + adrp/add&ldr xref + bl 拓扑（按 VA 查询） |
| 13_scratch/kyzt32_native/scan_q2.py | 单遍全可执行区扫描（numpy 找 bl + capstone 找 xref，23 s） |
| 13_scratch/kyzt32_native/find_hotupdate.py | 115 bundle 按 container 名定位 CodeHot |
| 13_scratch/kyzt32_native/read_bundle3.py | UnityPy 取 TextAsset 原始字节（落 HotUpdate 封装包） |
| 13_scratch/kyzt32_native/probe.py | SerializedFile 缓冲 PE/CLI 特征探测 |
| 13_scratch/kyzt32_native/scan_all_hits.py | 全 bundle 明文 PE/BSJB 扫描（结论：0 个明文程序集） |
| 13_scratch/kyzt32_native/crack_key.py | EncryptionKey 候选穷举（7,526 候选，0 命中，留证） |
| 13_scratch/kyzt32_native/list_containers.py | 指定 bundle 的 container / TextAsset 清单 |
