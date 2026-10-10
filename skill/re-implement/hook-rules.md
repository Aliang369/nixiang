# Hook 通用技术规则

> 路径：`re-implement/hook-rules.md`
> 触发场景：编写或审查 Hook；通用政策见 `../../AGENTS.md`。
> 历史事故是候选解释的来源，不自动证明唯一根因或跨版本通用定律。

---

## 0. 标准框架与生成器（新游戏起点）

新游戏从标准骨架生成，避免复制旧模块的隐含前提。模板中的懒装、地址校验、orig 转发、重入保护和写入助手仍需按本文件审查，名称包含 Safe 不等于已证明安全。

```bash
bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
```

- 框架参考：`mod菜单/starry-mod-menu/docs/GAME_MOD_GUIDE.md`
- 骨架：`Games/_TemplateMod.h/.cpp`（**v3.1，2026-10-02 起**）
- 实例参考：`Games/Archero2Mod.cpp`、`Games/WhrhxMod.cpp`、`Games/JfqsMod.cpp`
- 历史事故参考：`Games/HzsjMod.cpp`；只迁移有相同前提且有证据的经验。
- 存量模块不全量回改；新增部分及直接影响面适用本规则。

### 0.1 骨架 v3.1 的 §④「并发 + 致命信号兜底 + 槽位熔断」

来源：疾风骑士 60.0.1 两次真机闪退。三件设施是**代码**而非文档约定，删任一段都会复现事故：

| 设施 | 防的事故 | 关键实现 |
|------|----------|----------|
| `g_installMutex` 串行化 + double-check | UI 线程与游戏线程同入 `SyncHooks()`；`HookSlotInstall` 的 `if (origPtr && *origPtr)` 是 check-then-act 非原子 ⇒ 同址两次 `DobbyHook` ⇒ `DobbyCodePatch` 越界 memcpy ⇒ `SIGSEGV(SEGV_ACCERR)`（栈 `__memcpy_aarch64_simd ← DobbyCodePatch+96 ← DobbyHook+216`） | `SyncHooks()` 快路径 `g_installedCount >= kSlotsCount` 直接 return；否则加锁后 double-check |
| `sigsetjmp` 保护区（`DobbyHookGuarded` / `ReadU32Guarded` / `RunGuarded`）+ `sigaltstack` | `DobbyHook` 返回码接不住 Dobby 内部进程级 SIGSEGV；hook 体内会跑游戏/第三方代码（如免广告直接 Invoke 发奖回调，回调读未填充字段 ⇒ null-deref） | 保护区必须包住「装钩」「读序言」「回调」三处；longjmp 回来必须 `if (orig) *orig = nullptr;` 清掉可能被半写的 orig |
| `g_slotDead[]` 槽位熔断 | 同一槽位每轮 Tick 重试 ⇒ 每 5 s 一次闪退 | 装钩期出致命信号即标记该槽位本次会话不再重试；日志 `slot[%d] %s 已熔断` |

`sigaltstack` 与全部故障态（`t_faultJmp`/`t_inGuard`/`t_faultDepth`/`t_faultSig`/`t_faultAddr`/`t_altStack`）**必须 `thread_local`** —— hook 体在 UI 线程、游戏线程、SDK 回调线程并发执行。诚实边界：Dobby 炸后内部状态可能已不一致，熔断只保证不再重试，不回滚。

### 0.2 构建脚本差异区默认带 `Games/NoAceStubs.cpp`

`Android_modern_template.mk` 自 2026-10-02 起默认加入。原因：`MainModern.cpp` 的 `JNI_OnLoad` 无条件引用 `SignalQuarantine_Start` / `FgoBoot_InjectEarlyFromOnLoad`，缺它即 `ld.lld: undefined symbol`。B 类（包内无 `libtprt.so`、无 ACE，菜单 dex 由 Manifest 的 `StarryInitProvider` 拉起）用 no-op 桩；A 类改为 `Games/SignalQuarantine.cpp Games/FgoBoot.cpp`（样例 `Android_jjdm.mk`/`Android_mygwzd.mk`）。

生成新模块后必须做一次 **copy-and-build-clean** 验证：`new_game_mod.sh` 生成 scratch 模块 → `ndk-build` 到**链接**通过（只看编译通过不够，符号缺失是链接期错误）→ 删除 scratch 产物（`Games/{Scratch}*`、`Android_<tag>.mk`、`obj_<tag>/`、`libs_<tag>/`），不得入库。唯一可用的 ndk-build 在 `$HOME/Library/Android/sdk/ndk/27.0.12077973/ndk-build`（`26.1.10909125/` 是空目录）；`NDK_OUT`/`NDK_LIBS_OUT` 必须绝对路径，相对路径会被解析到 `app/` 下导致旧 so 顶包假成功。

## 1. Dobby inline hook 标准模式

Dobby 修改入口并提供原函数跳板；具体指令序列取决于架构、版本和入口布局，不固定为一条 B 或 br。

```cpp
// 概念示意：安装返回值、目标签名和 orig 有效性须在放行前确认。
DobbyHook(target, replacement, &orig);
// replacement 默认按真实 ABI 转发到 orig，保留必要副作用。
```

## 2. base_va 计算（三种方式）

| 方式 | 公式 | 适用条件 |
|------|------|----------|
| 符号解析 | `dlsym(handle, symbol)` | 正确模块、符号及签名，核对解析到的实际入口 |
| 文件映射换算 | `map_start + (file_off - map_file_off)` | 目标位于该文件支持的映射范围；映射与文件身份一致 |
| ELF 虚拟地址换算 | `load_bias + elf_va` | 从 PT_LOAD 与运行时映射确认 load bias |

`base + RVA` 并非天然错误：若 base 是明确定义的运行时参考基址，RVA 是相对同一参考点的虚拟地址差，则成立。禁止把首个 RX 映射起点、文件偏移、IDA EA 混称 base/RVA；详见 §6–7。

## 3. 默认保留原函数（orig）副作用

不调 orig **不会必然 SIGSEGV**，也不存在“不调用就把参数当指针”的机制。崩溃可能来自 ABI 不匹配、无效对象、错误跳板、缺失必要副作用等，须分别取证。

- 默认调用 orig，保留初始化、引用计数、锁、回调、状态迁移等副作用；仅调用了 orig 也不证明调用次数、时机和参数正确。
- 任何省略原调用的设计都必须说明替代契约，并有 ABI、必要副作用、返回值、对象所有权与生命周期证据；不能凭“只改返回值”豁免。技术可行性不替代工作区授权政策。
- 核对实际调用约定、隐含参数、异常路径、orig 安装与发布状态。orig 不可用时应阻止该 hook 放行，而非伪造成功返回。

```cpp
// 假设原函数的确是 int(int, int)，且 orig 已成功安装并安全发布。
int replacement(int a, int b) {
    const int result = orig(a, b); // 所有正常分支先保留原调用副作用
    return result;
}
```

## 4. 双 Dobby 叠同一函数：兼容性待证

- 多个框架改写同一入口可能破坏跳板、形成递归或覆盖对方，但不等于必崩。
- 默认避免重复安装；核对入口归属、安装顺序、跳板链和卸载顺序。没有链式兼容证据时不叠装。
- 历史双 hook 事故只说明该配置曾失败，不能据此排除 ABI、生命周期或安装竞态等其他根因。

## 5. 偏移必须有来源并绑定版本

- 不猜偏移。记录 APK/ELF 身份、架构、版本、坐标系、工具输出或分析位置。
- 静态结果是可测试候选，不等同运行时确认；更新后重新核对。
- 无效时先区分模块身份、挂点、调用路径和字段布局，再改逻辑。Trace 命中、类型信息和计数可提供局部证据，不能单独证明行为正确；不强制某个探针工具。

## 6. file_off 与 RVA 的区别

| 概念 | 含义 |
|------|------|
| file offset | ELF 文件内字节位置，不是 IDA 默认显示的地址 |
| ELF VA | ELF 链接时虚拟地址，以 PT_LOAD 的 p_vaddr 为依据 |
| load bias | 加载后运行时地址相对 ELF VA 的整体平移量 |
| IDA EA | IDA 数据库有效地址，可能已 rebase；需核对 image base、段和文件映射 |
| RVA | 相对约定参考基址的虚拟地址差，必须写明参考点 |

对同一 PT_LOAD 的文件支持部分：`elf_va = p_vaddr + (file_off - p_offset)`。只在 `p_filesz` 覆盖范围内有效；BSS 等内存尾部没有对应文件字节。`p_vaddr - p_offset` 可因段而异，`0x4000` 不是通用常数，也不是运行时 load bias。

Il2CppDumper `dump.cs` 的三个字段里，**`Offset` 才是 file offset**（`RVA − Offset` 即首段 `p_vaddr − p_offset`，本例 0x4000，须按程序头实测）；序言期望值按小端读成 `uint32_t`（`eb 2c bb 6d` → `0x6DBB2C6D`）；锚点表须绑定 `libil2cpp.so` 的 SHA-256（§23）。口径填错**不报错**，只会静默退回按名路径——「功能生效」不能作为口径正确的证据，唯一判据是 logcat 的 `@rva`/`@by-name`。详见 `version-update-relocate.md` §0.1。

## 7. 运行时 VA 换算与交叉检查

- 用 ELF 程序头和实际 maps 对齐匹配；maps 可能按页拆分，不能把任意 RX 起点当模块基址。
- 文件映射法使用 §2 公式，并验证目标位于同一映射、权限合适且字节对应。直接从 APK 映射时还需区分 APK 外层偏移和 ELF 内偏移。
- ELF 法使用 `load_bias + elf_va`；IDA EA 先按数据库 rebase 关系还原到 ELF VA。
- 已明确定义 base/RVA 时允许 `base + RVA`，并与映射或符号结果交叉核对。匿名/运行时改写区域不能生搬文件公式。

## 8. hook 选择：具体方法 > 有过滤跳板 > 静态 patch

- 优先选择语义明确、影响面小且可观测的具体方法；调用频率仍须测量。
- 共享跳板须有可验证的方法身份过滤、正确 ABI 与原调用转发、并发安全且可失效的分类缓存。
- 不按一个历史游戏的 MethodPointer 失败断言所有 HybridCLR 方法不可直接 hook。
- 共用入口无过滤不放行；方法名可能重名，须核对类、程序集、签名等身份。

### 8.1 广告 / 内购类挂点的定位法（2026-10-02 疾风骑士）

> 来源：疾风骑士会话复盘黑洞 #3 —— 「免广告」第一版挂在了 `ADXManager`（一套**从未被启用**的
> 广告系统，logcat 里 `ShowRewardedAd` 一次都没被调用），白跑一轮。真实广告走的是
> MarSDK 的 Java 侧 `com.mar.sdk.gg.topon.ToponAd`。**同一款游戏里存在多套广告系统是常态。**

按顺序做，别跳步：

1. **先查服务端下发的开关**。渠道 SDK 的广告位通常由服务端配置控制，客户端只执行。
   疾风骑士实测 `MARSDK-MggControl: adSwitch` 只有 `videoSwitch:true`，
   `intersSwitch` / `splashSwitch` / `bannerSwitch` 全 false ⇒ 直接排除插屏/开屏/横幅，
   **只剩激励视频一条路**，候选面立刻缩小。
2. **再查 logcat 确认哪一套 SDK 真的在跑**。判据是「哪个类被调用了」而不是「哪个类存在」：
   存在但从不被调用的类（`ADXManager`）是陷阱。**没有调用日志 = 不挂它。**
3. **再选挂点**。优先「发奖回调链」而非「展示函数」：拦截展示（`Show*`）只是不弹窗，
   但奖励也不会发；要拦的是**展示 + 发奖**这条链，按顺序补发回调
   （疾风骑士：`InvokeAction(onShow) → success → onClosed`）。
4. **Java 侧 vs native 侧**：真实广告可能在 Java（渠道 SDK）而不在 `libil2cpp.so`。
   IL2CPP 侧能找到的 `MarSDKManager` 类**可能不在 `Assembly-CSharp.dll`**，
   需要跨 image 的 `FindClass` 回退；这一条属于**未证明的前提**，版本升级后要重新验证。
5. **顺手找「内置后门」**。渠道 SDK 里常有调试/白名单分支（疾风骑士在
   `MarSDKManager.ShowReward` 内有一处 `PlayerPrefs.HasKey(<key>)` 为真就**直接调 success
   回调、完全不弹广告**的分支）。这类分支比 hook 更干净，但 key 名可能是运行时 interned 槽
   填充的、静态搜不到 —— **能挖到就用，挖不到不要为它多耗一轮**。

反面教训：`IsNoAd` / `LoadRemoveAds` 这种**名字最像**的函数往往只是本地开关/UI 状态，
恒真不改变真实广告路径（疾风骑士两条都挂了、用户实测仍弹广告）。

## 9. 不将堆扫描命中当作托管对象证明

- GC 是否移动对象取决于具体运行时、收集器与配置；有的 IL2CPP 使用非移动 GC。
- 即使不移动，对象仍可能回收、地址复用，托管壳仍在也可能对应已销毁的原生对象。
- 原始堆匹配会命中元数据、旧对象或随机相似值，不能直接据此调用方法或写字段。
- 优先从有契约的参数或字段链取得对象；跨回调保存仍需运行时支持的持有机制、销毁/场景失效处理和线程约束，不因来源可靠就永久有效。
- 终末地历史记录中的 klass 假阳性和失败哨兵值混用是候选事故链；是否唯一根因须以栈、对象来源和对照实验确认。

## 10. 防钻牛角尖

同一卡点的信息增益预算与升级规则引用 `../../AGENTS.md`。先列候选与区分证据，优先最小可区分实验；失败记录前提和观察，不把换工具本身当进展。

## 11. 内存读写：地址检查不能证明对象生命周期

- 判空、对齐、范围、映射权限及 `IsValidHeapPtr`/`SoftPtr` 只能排除部分坏地址。
- 每次访问须有对象类型/布局、字段边界、存活期、所有权与同步依据；“检查后再访问”之间仍可能回收或解除映射（TOCTOU）。
- `memcpy`、`memcmp` 同样访问内存，可能越界或触发异常；不提供对象类型或生命周期保证。
- 写入助手应集中检查并表达失败，但不能仅因包了一层 SafeWrite 就放行；托管引用写入还须遵循运行时写屏障契约。
- 缺少存活证据时不访问，不用信号兜底代替对象管理。

## 12. SafeProbe 不进入热路径

- 先检查具体实现：安装/恢复进程级信号处理器可能干扰其他线程和宿主处理器。
- 一次性诊断也需隔离范围、线程、恢复路径和运行时兼容性证据；从故障中返回不证明后续进程状态正确。
- 禁止在 hook/Tick/每帧循环中用异常探测替代有效性证明。热路径依赖已建立的生命周期与同步契约，不是“地址检查后就随便读写”。

## 13. 类名/方法名解析缓存

- 根据实测频率缓存昂贵解析，缓存键包含必要的方法/类/运行时身份。
- 缓存需线程安全，并在场景、域、模块或对象生命周期变化时按契约失效。
- 读取对象头取得 klass 本身就是解引用，先满足 §11；缓存不延长对象存活期。

## 14. 同线程重入与跨线程互斥分开

同线程递归保护可用 `thread_local` + RAII，确保正常返回及 C++ 栈展开时复位。它不负责跨线程互斥，也不能覆盖 longjmp 或进程终止。

```cpp
thread_local bool in_hook = false;
struct ReentryGuard {
    bool& flag;
    explicit ReentryGuard(bool& f) : flag(f) { flag = true; }
    ~ReentryGuard() { flag = false; }
    ReentryGuard(const ReentryGuard&) = delete;
    ReentryGuard& operator=(const ReentryGuard&) = delete;
};
// 已确认 orig 有效且签名匹配：
// if (in_hook) return orig(args...);
// ReentryGuard guard(in_hook);
// return orig(args...);
```

- 两次独立原子 `load()`/`store()` 不是互斥获取；两个线程都可能读到 false。
- 跨线程共享状态使用符合语义的互斥锁或原子 RMW 协议；核对竞争失败路径、内存序及是否允许等待。
- 不在未知 orig/回调调用链外长期持锁；评估递归、锁顺序和死锁。
- BSS 普通 `int32_t` 跨线程无同步读写仍是 C++ data race，即使硬件对齐访问不撕裂。需原子或锁；多字段一致快照另建协议，不能只把一个访问端改成 atomic。

## 15. 写新游戏前先看标准实例

从 §0 骨架起步，实例仅供模式参考；对照当前架构、版本、调用契约与证据审查，不照搬“正面范例即安全”的结论。

## 16. 懒加载分组 Hook（默认减少启动改写面）

- 默认关闭时不安装无必要 hook；模块映射完成不等于运行时、目标入口和依赖状态已就绪。
- 按功能组按需安装，公共入口去重；启动批量 hook 增加排障面和时序风险，但不是必崩通用定律。
- `SyncHooks` 对检查/安装/发布状态整体同步，安装成功不重复装，失败有界重试并记录；重入保护不能代替安装互斥。
- 无悔华夏启动多点安装、终末地并发安装历史故障只能作为线索，不能仅凭崩溃推定“重复 hook 被保护杀死”。
- 失败挂点在已知版本/条件下隔离；重新启用须有修正证据，不将一次事故永久推广到所有版本。

## 17. 可选方法缺失不得阻断独立功能

- 多候选解析须验证签名与语义身份，不能名称相似就接受。
- 依赖缺失时关闭对应功能并记录原因；只允许前提独立且已验证的其他功能继续，不伪装整体成功。
- 实体缓存与列表遍历都需生命周期、同步和规模依据。历史列表遍历崩溃不证明所有遍历必崩，近期缓存也不天然安全。

## 18. 桩签名三查（安装前确认 ABI）

签名错误可能表现为恒 0、错误结果、寄存器/栈破坏或崩溃；不能断言“错了也不崩”。元数据/签名探针只是证据之一，还需结合目标架构 ABI、实际入口和必要的调用点反汇编；不强制特定工具。

1. **返回类型**：以 AArch64 为例，float/double 通常经 s0/d0，整数按宽度经 w0/x0，指针经 x0；聚合返回可能走多个寄存器或隐藏返回缓冲区。正确类型的 C++ 调用由编译器保存返回值，手写跳板需自行遵约。
2. **参数与调用约定**：整数/指针与 FP 参数分配规则不同，还需核对结构体、byref、栈参数、对齐和 trampoline 转发。
3. **static/instance 与隐含参数**：`flags & 0x10` 是托管 static 线索，不足以确定 native 桩。核对 this、MethodInfo、泛型共享上下文、invoker/wrapper 的实际布局；不能套“argc+method”统一槽数公式。

## 19. 基类方法指针 ≠ 派生 override

需要虚派发语义时按实际 klass 和运行时 API（如 `il2cpp_object_get_virtual_method`）确认目标。直接调用基类实现可能是有意的非虚调用，必须与目标语义相符；每 klass 缓存仍须满足 §11、§13。

## 20. 主线程访问与专用学习线程的边界

- 任意线程均不能访问存活期、布局或线程契约不明的对象；并非“主线程一律零解引用”。有正确契约的主线程访问可以成立。
- 指针值相等比较通常不读取所指内存；读对象头、类名、`memcpy`/`memcmp` 都会访存，不能称为零解引用。
- 专用学习线程不是万能安全区：运行时 attach 不等于具备引擎主线程权限，转移对象指针还可能失去持有关系。
- 后台工作优先接收在合法上下文中取得、可独立持有的一致快照；必须跨线程访问对象时明确持有、同步、线程许可和失效机制。
- 类名关键词仅为分类线索，不证明身份或行为；历史 SIGSEGV/保护终止记录不自动证明某一种根因。

## 21. 功能实现分层（定稿 2026-09-09：数据优先、行为 hook、字节 patch 禁用）

- **按“改什么”分层，不是按“找什么”分层**：拿到需求先分类，再选实现路线。
- **数据型**（无敌=HP 回写、属性增益、资源锁定）：偏移定位（dump 实测）+ SafeWrite 直写、Tick 轮询维持是首选——零 hook 痕迹（开关全关=零改写，防封最优），挂错偏移有校验兜底不会闪退。范例：超维进化属性增益、`mod菜单/starry-mod-menu/docs/reference/无RVA便携模板.cpp`（纯数据流，零 hook，仅作历史参考）。
- **行为型**（改判定、伤害结算、拦截上报、CD 重置）：Dobby hook 是首选，RVA 优先（maps RX + 序言校验）失效自动转按名（模板 v3 HookSlot 双路径）。Dobby 本身就是“RVA 定位后安全改函数”的工程化实现（trampoline 保 orig、指令迁移、页属性），不是“实在找不到才用的兜底”。
- **手写字节 patch 改函数序言**：默认禁用。依据：E15-E18 断闸全灭（1Hz 校验 + 多通道级联杀）、超维进化完整性扫描实锤、arm64 指令手编易错、RVA 漂移后写错位置=闪退。
- 数据直写与 hook 的选择不因“RVA 已知”而改变：RVA 只解决“去哪找”，不解决“怎么改”；定位用 RVA、改数据用直写、改行为用 Dobby，三者各司其职。
- 分层表全文与接入清单见 `mod菜单/starry-mod-menu/docs/GAME_MOD_GUIDE.md`（2026-09-09 版）。

## 22. 挂点五步门禁（2026-09-17 起；2026-10-03 扩为五步，写功能代码前必走）

> 来源：18 份会话记录审计——弹力果冻 TapTap 迁移三轮返工（methodPointer 当跳板/硬搬旧偏移/重犯当天已否定方案），每轮都走完整构建-装机-实测周期。静态定位不是终点，是第一步。
> 第 5 步来源：冒险与征途 1.5.9 复盘——有对照包却自建路线，跑了 11.6 小时 / 2428 次工具调用后回到对照方案。
> 根因不是「没读资料」，而是**偏离不落盘、不确认**，没有任何一步会因此停下。

按顺序走完**前五步**才允许写功能实现（第 5 步在装机前复核）：

| 步 | 动作 | 拦截的失败模式 |
|----|------|----------------|
| 1 | 静态定位候选挂点（dump/反编译） | — |
| 2 | **只读计数验证**：Frida/探针确认候选点真实被调用（calls>0） | 挂点从未被调用（内部调用不走该入口） |
| 3 | **跨版本/跨渠道偏移 diff**：从旧版迁移硬编码偏移前，先做新旧 so 逐字节/逐指令 diff | 函数前移导致 RVA 静默错位 |
| 4 | 写功能实现，首次装机日志必须含挂载/hit 计数 | 无 installed 日志却宣称"已验证"（假成功） |
| 5 | **对照方 1:1 核对**：填 `10_docs/{游戏}/路线锁定.md`（模板 `10_docs/_templates/路线锁定.md`）——**表内任何一行「1:1？=否」未取得用户确认时，禁止装机**；对照方的机制读不懂就写「未解析」并停下先解析 | 有对照包仍自研路线（偏离被静默吸收，直到用户第 N 次说"照他的改"）；「未解析」被自己的猜测填坑 |

- 同一会话中**改挂点前强制回看本会话已否定的方案清单**，防止重犯（弹力果冻 HpHandle 事故：当天已总结"内部调用不经桥"，数小时后换挂点时重犯）。
- **第 5 步的例外只有一个**：对照方**没有**该功能（写「无参考实现」+ 假设矩阵 + 验证方法）。
  「对照方的做法我觉得不对 / 不方便 / 我有个更好的思路」**不构成例外**——那属于要用户拍板的偏离，
  写进「未确认偏离清单」后等确认，不确认就不装机。
- 用户口头重申「照 XX 做」时，先看 `路线锁定.md` 是否已存在、是否填满；**缺表就先建表再动手**。
- HybridCLR 相关：热更程序集的内部调用不经 methodPointer 桥、`[DllImport]` 解析不到符号调用必抛异常（宝石战争/弹力果冻双实锤）——该结论已升格为硬规则，见 `re-extract/hybridclr-runtime.md`；热更侧拿 native 状态优先文件镜像通道。

## 23. 双 so 库判定与 RVA↔so 绑定（2026-09-17 起）

> 来源：无悔华夏热更修复（"游戏会加载两套 so，之前的文档都有写明"——历史文档存在但任务开始没检索，方向错误多耗一轮）；大话蜀山双副本诱饵（`/data/data` 真身 vs APK 内诱饵，v1/v2 两轮返工后才定位）。

- 热更类游戏 hook 前必须确认**实际执行库**：加载路径 + SHA，不以 APK 内 so 或首个 dlopen 结果为准。
- 每个游戏文档维护 **RVA ↔ so SHA 绑定表**：RVA 只在对应 so 哈希下有效；换渠道/换版本先比对 so 哈希，同源可直接复用 RVA（无悔华夏 4399 vs 九游实测字节一致 → RVA 零重定位）。
- **任务开始第一步强制检索该游戏 `10_docs/{游戏}/` 历史文档**：已有的双库记载、否定结论、机制定案不重新推导。

## 24. 指纹铁律：禁止用会被混淆器改名的标识符（2026-09-21 起）

> 来源：勇士之心热更 res 1.0.98.10 功能失效——自动战斗指纹写死 `bVs(`，热更把 buff
> 管理器方法改名为 `bXP`，`tryAuto` 全部落空，功能静默失效。**同一功能第二次翻车**
> （09-15 是 bundle 归属判断错）。完整诊断路径见 `../re-extract/hotupdate-fingerprint-recheck.md`。

**允许作为指纹（混淆器不动）**

| 类型 | 示例 | 说明 |
|---|---|---|
| 字符串字面量 | `"isPlayer"`、`"finalHurtValue"` | TS 编译期字符串，改名不改字面量 |
| 枚举成员全路径 | `BuffTableEnum.AUTO_BATTLE` | 枚举成员名保留 |
| 明文属性/字段名 | `lockMinHP`、`maxApDefRate`、`hTimeScale` | 引擎/协议字段名不参与混淆 |
| 明文 API 名 | `worldTimeScale`、`hitPauseTimeScale`、`prototype.update` | ECS/引擎 API |

**禁止作为指纹**

- 短混淆标识符：`bVs(`、`bXP(`、`fv`、`hX` —— **每次热更/换版本都可能变**。
- 单一混淆名 + 无兜底：即使本次命中，下次仍会静默失效。
- 整文件/整模块级判定：运行时是**按函数**遍历，粒度不对会得到假阴性。

**写法要求（两级指纹 + 失败留痕）**

```js
// 主指纹：只用明文锚点
var f = findFn(Cls.prototype, function (s) {
    return s.indexOf("AUTO_BATTLE") >= 0 && s.indexOf("BuffTableEnum") >= 0 &&
           s.indexOf("return") >= 0 && s.length < 600;
});
// 兜底指纹：结构特征（容忍任意方法名）
if (!f) {
    f = findFn(Cls.prototype, function (s) {
        return s.indexOf("AUTO_BATTLE") >= 0 && s.length < 400 &&
               /return\s+[\w$.]+\.[\w$]+\s*\(/.test(s);
    });
}
// 命中失败必须留日志：静默跳过是排查最贵的失败模式
if (!f) {
    var cand = findFn(Cls.prototype, function (s) { return s.indexOf("AUTO_BATTLE") >= 0; });
    if (cand) log("!! auto-battle candidate .prototype." + cand + " rejected by fingerprint");
    return;
}
```

- 每加一条指纹，同步加一条**静态断言**（生成器/verify 脚本）禁止回退到写死标识符。
- 菜单能开 ≠ 功能生效：功能失效优先按 §24 查指纹漂移，不要先怀疑 native 偏移。

## 25. 生成器幂等 + 探针自检行（2026-09-30 起）

> 来源：勇士之心 1.0.4 会话复盘。两类纯浪费被固化：①`gen_*.py` 每次运行都重写头文件，
> mtime 变化让 `git status` 常驻噪声、pipeline 陈旧度门禁误判「上游已更新」；②探针装了
> 却看不出「装没装上」，白跑一轮 Frida。

**25.1 生成器必须幂等写**

- 所有产出文件（patch 头、模板 JSON、spec 派生文件）统一走
  `04_scripts/_common/gen_util.py` 的 `write_if_changed(path, text, encoding="utf-8")`：
  内容相同**不落盘**并打印 `unchanged <path> bytes=N`，不同才写 `wrote …`。
- 新写生成器一律用它；`04_scripts/{游戏}/gen_*.py` 里禁止裸 `open(...,"w")`。
- 自检：连跑两次生成器，第二次必须全部 `unchanged` 且 `git status` 该文件为空。

**25.2 探针必须自带装配自检行**

- 每个安装点都要在 logcat 留一行可判定结果——`hook ok <库>.<符号>` /
  `hook 失败 <库>.<符号>` / `符号未找到 <库>!<符号>`，并单独打一行「探针已安装」。
- 目的：区分「没装上」和「装上了但没命中」。实例：`YsxzExitTrace` 打出
  `hook ok libc.exit/_exit/abort/kill/pthread_exit` + `hook 失败 libc._Exit` +
  `符号未找到 libandroid_runtime.so!Java_java_lang_System_exit__I`（Android 14 上这四个
  JNI 符号本就不在导出表）⇒ 直接排除「Java 侧 System.exit/Runtime.exit/killProcess」路径，
  少走一轮。
- 探针行要带**独立计数**：多个调用点共用一个计数器时，首个命中会被计数门限吃掉、栈不打印，
  反而掩盖真正的调用者（`YsxzExitTrace` 因此按符号分别计数）。

**25.3 探针通道选择**

- 非 root 机（无 `frida-server`）与需要长期驻留的场景，用**自家 so 内探针**写 logcat，
  两机通用；不要先搭 Frida。
- 探针输出不要只走缓冲 stdout：`frida -o logfile` 在 `kill` 收尾时把未 flush 的
  `setInterval` 轮询行整批丢掉（实测整轮结论不可用）。native 侧用 `__android_log_print`。
- 拦截 native 退出/自检类行为时，先看 `re-analyze/anti-cheat-pattern.md`
  「原生退出拦截（seccomp）」一节，再决定钩 libc 还是拦 syscall。

## 26. 诊断隔离与证据优先（2026-09-30 起）

> 来源：伊莫 1.0.1 会话复盘（`10_docs/_state/规范优化_20260930_伊莫1.0.1会话复盘.md` P0）。
> 背景：调试期诊断「先进包、最后手术拆除」——伊莫为此做了一轮 115 行/35 处手术
> + 一整轮 389 MB 重建重装，而且手术本身会引入回归。本节点把「诊断」变成可开关的构件。

**26.1 诊断必须编译期可整体关掉（不是运行时短路）**

- native：`jni/ModDiag.h` 的 `DLOG/DLOGN/DWARN`。`#ifdef STARRY_DIAG` 时写 logcat
  （TAG `StarryDiag`），否则宏展开成 `((void)0)` ⇒ 正式包 so 里**一个字节都不进**。
- Lua：`jni/ModDiagLua.h` 的独立诊断 chunk，`MOD_DIAG_LUA_TEXT` 在正式包为空串。
  **诊断 chunk 必须与功能补丁分开两次 `RunLua`**：拼成一段会把「诊断语法错」
  放大成「整个补丁失效」（同 §「补丁与写开关必须独立 chunk」）。
- 构建：`ndk-build ... STARRY_DIAG=1`（`Android_<tag>.mk` 里 `ifdef STARRY_DIAG`）；
  脚本 `surgical_*.sh` 带 `STARRY_DIAG=1` ⇒ 产物名带 `-diag` 后缀、**不覆盖**正式包 alias。
- ★**换模式必须换 `NDK_OUT` obj 目录**：ndk-build 不会因 CFLAGS 变化重编已有 `.o`，
  两模式共用 obj 目录会让「诊断包静默退化成正式包」（伊莫实测：so 内没有诊断 chunk，
  构建却报成功）。诊断包构建后要**硬校验** so 内存在诊断标记。
- ★**`app/src/main/jniLibs/` 也会被模式污染**：脚本把 so 拷进 jniLibs 再给 Gradle 打包，
  跑过一次诊断构建后 jniLibs 里留的是**诊断 so**。任何跳过 ndk-build 的 Gradle-only 构建
  都会把诊断痕迹打进「正式包」——release 构建前必须重跑 ndk-build（脚本已强制），
  收尾时重跑一次正式构建把 jniLibs 恢复成正式 so，别留着诊断 so 过夜。
- 正式包门禁：`bash 04_scripts/_common/check_release_clean.sh --tag <Tag> --so <so>`
  （块感知源码扫描 + so 二进制扫描）；内嵌 Lua chunk 用
  `python3 04_scripts/_common/lua_struct_check.py` 做结构配平体检。

**26.2 证据优先：先出 `-diag` 包采证，再改功能代码**

- 症状不明时不要凭猜改代码：先出诊断包，把「装上没有 / 命中没有 / 值是多少」打出来。
  伊莫「伤害倍率到底对谁生效」是靠 TRACE/RSLOG 定位到「只有宠物通道可放大」的，
  改代码之前先花一轮采证，比盲改三轮便宜。
- **「没装上」与「没命中」是两回事**：安装点必须有自检行（§25.2），
  并在诊断启动时把关键前提打出来（目标 Lua 表是否存在、路径、上限等）。
- **诊断每行带序号**（`#1 #2 #3`）：某个字段恒为常数时，先怀疑**诊断本身**错，
  再怀疑功能错（伊莫实测：`plan=0.0` 恒成立是因为读了共享引用
  `getPosition()` 的传送后值 ⇒ 自适应逻辑是死代码）。
- 证据要能区分「行为正确」与「数值正确」：菜单里的勾选状态 ≠ 游戏里真的生效。

**26.3 诊断上限放运行时，不写死**

- 上限写在设备上的配置文件（伊莫：`/sdcard/.starry_magic/logcfg`，一行 `tag=上限`，
  负数=不限），改完重启游戏即生效。别为把上限 5 改成 24 重建重装一轮。
- 每 tag 独立计数（§25.2），诊断代码一律 `rawget/rawset`（xLua 全局表白名单式，
  见 `../re-extract/xlua-from-scratch.md` §七）。

---

## 27. 按名取 MethodInfo 的「同 argc 重载」消歧（2026-10-01 失落城堡）

- **问题**：`il2cpp_class_get_method_from_name(klass, name, argc)` 只按「名字 + 参数个数」匹配，
  返回**第一个**命中项，同一个类里同名且参数个数相同的重载它区分不了。失落城堡
  `Tera_NF.ContentUnlockCtrl` 的 ACTk `ObscuredInt.op_Implicit` 有 **4 个 argc=1 重载**
  （`int→ObscuredInt`、`float→ObscuredInt`、`bool→ObscuredBool`…），取错一个既不报错也不崩，
  只静默走错分支 —— 当轮表现就是「日志说写入成功，游戏里毫无变化」。
- **判定**：`GetMethod(k, name, argc)` 返回非空 **不代表选对了**，只代表「存在这个参数个数的重载」。
  只要目标方法名在 metadata 里同 name+argc 出现多行（`01_tools/MetadataSearch` 的 `methods` / `sig` 模式可直接看），
  就必须消歧；唯一命中的普通方法不受影响。
- **正确做法**：用 `Il2Cpp::Api::GetMethodBySig(klass, name, argc, retSuffix, p0Suffix)`
  （`mod菜单/starry-mod-menu/app/src/main/jni/Unity/Il2CppApi.h`，2026-10-01 增加）——
  先按 name 遍历类方法，再用「返回类型名后缀 / 第 0 参类型名后缀」筛，**唯一命中才返回**，
  0 命中或多命中一律退回 `GetMethod` 原行为（不猜）。
  例：`api.GetMethodBySig(k, "op_Implicit", 1, "ObscuredInt", "Int32")`。
- **实现前提**：`class_methods`（`il2cpp_class_get_methods` 迭代器）与
  `method_ret_type` / `method_param` / `type_name` 都是**可空导出**，任一缺失该方法自动退回旧行为；
  用之前先确认这几个符号解析到了（`Il2Cpp::Api` 成员，见头文件「成员实名速查」注释）。
- **仍然歧义时**：改为**运行时实参类型判断**（在 hook 里看传入对象的 `object_class`/`class_name`），
  或换一个同名唯一的方法当锚点。宁可少做一个功能，也不要挂错重载。
- 顺带一条相关约束：`type_name()` 返回的串由 il2cpp 分配、按约定要 `free`；
  但**判等用的短名串可故意不 free**（每次解析只跑一遍，泄漏有界），别为省内存去 free 一个不确定归属的指针。

## 28. 谓词 / 门禁恒真陷阱：「无限 X」只改数量上限（2026-10-02 疾风骑士）

> 来源：疾风骑士 60.0.1 会话复盘（`10_docs/_state/规范优化_20261002_疾风骑士60.0.1会话复盘.md`）。
> 无限跳跃从 v2 改到 v6 共三轮返工，每轮都走完整「改代码 → 构建 → 装机 → 用户实测」周期；
> 用户自己在 m02751 说出正解「跳跃本身是限制只能跳两次，让不限制不就可以无限跳了」，
> 反汇编随后证实用户判断正确。**这类函数名高度相似、语义却分三类，只看名字必错。**

### 28.1 三类函数必须分开（先分类，再动手）

| 类别 | 语义 | 恒真后果 | 处理 |
|---|---|---|---|
| **数量上限** | 告诉引擎「这一轮最多允许几次」 | 上限变大 = 行为正确 | ✅ **只改这一类** |
| **判定谓词** | 纯派生判断「现在还剩几次 / 还能不能」 | 与真实计数脱钩，状态机与 UI 不一致 | ❌ 交回 orig |
| **状态门禁** | 「此刻是否处于允许触发的状态」 | **每帧都判定为允许 ⇒ 自动连发** | ❌ 交回 orig |

**核心判据**：恒真一个「此刻能不能做」的门禁，等于把「用户操作触发的时机判断」删掉，
状态机每帧调用它就每帧放行 ⇒ 表现为**玩家没操作，角色自己在动**（自动连跳/自动攻击）。
这不是「功能太强」，是**挂错了函数**。

### 28.2 「无限 X」的定位套路（先反汇编找上限来源，别先挂谓词）

1. 先找**读计数器并写回**的那个函数（通常是「重置/初始化/补充」语义），
   从它 `bl` 出去的第一个「取上限」调用就是候选；
2. 反汇编该上限函数，确认形态是
   `条件 ? (等级/参数 + 1) : 默认值` 或直接读某个计数字段 —— 这类才是**数量上限**；
3. 再反汇编名字最像的那几个（`CanXxx` / `CheckXxx` / `XxxCooltime`），确认它们是
   「读计数比 0」「读状态位」的纯谓词/门禁 ⇒ **明确排除**；
4. 只 hook 第 2 步那个函数，返回一个大常数。谓词与门禁一律交回 orig。

### 28.3 实例对照表（疾风骑士 `PlayerMovement`，RVA 仅注释、实现全 by-name）

| 函数 | RVA | 反汇编语义 | 判定 |
|---|---|---|---|
| `GetMaxMoreJumpCount()` | `0x37C4728` | `CheckEquipNormalArtifact(0x6d) ? level+1 : 1` | ✅ 唯一正解（默认 1 ⇒ 地面 1 + 空中 1 = 共 2 跳，与用户描述完全一致） |
| `CanMoreJump()` | `0x37C47E4` | 读 `[this+0x440]` 计数器 → `> 0` | ❌ 纯派生谓词 |
| `CheckCanJump()` | `0x37BB970` | 读三个状态位 + 内部状态机调用 | ❌ 状态门禁（恒真 ⇒ **自动连跳**） |
| `PlayerJumpCooltime(float)` | `0x37C47A4` | `(Time.time - [this+0x838]) > (offset+const)` | ❌ 起跳节流（恒真 ⇒ **每帧允许起跳**） |
| `PlayerMoreJumpCountReset(int)` | `0x37C46C0` | `w0 = GetMaxMoreJumpCount() + moreCount` 写回 `[this+0x440]` | 不必 hook（改上限即自动生效） |

### 28.4 通用化

- 「无限技能/无限怒气/无限连击/无限弹药」同构：找**上限来源**（`GetMax*Count` / `Get*Capacity` /
  `CheckEquip* ? level+n : 默认值`），不是找 `Can*`。
- 「冷却归零」是另一类问题，见 §28.5，别与上限混为一谈。
- 数据型资源锁定（HP/属性/货币）仍按 §21 走偏移直写，不适用本节。

### 28.5 相关：冷却类功能的真门禁往往不是名字带 `CoolTime` 的函数

同一会话里「技能无限」也翻过一次车：名字最像的 `CheckCanSubSkillByCoolTime()` 实际是
`CheckCanAttackSkill() && !CheckEquipNormalArtifact(0x23) && !CheckEquipNormalArtifact(0x15b)`，
**与冷却无关**；真正的冷却只有一个计时字段，由少数几个写入点维护。
⇒ 判据：**先找「计时字段的写入点」，再找读它的 getter**，而不是找名字。

| 角色 | 疾风骑士实例 | RVA |
|---|---|---|
| 唯一计时字段 | `PlayerMovement + 0x588`（`float`，`get_WeaponSkillCoolTime()` = `ldr s0,[x0,#0x588]; ret`） | `0x37AA27C` |
| 写入点（填满） | `SetWeaponSkillCoolTime(bool)` | `0x37B596C` |
| 写入点（加减） | `SetPlusWeaponSkillCoolTime(float)` | `0x37D3D6C` |
| 假门禁（名字骗人） | `CheckCanSubSkillByCoolTime()` | `0x3817E6C` |

做法：拦写入点 + `SafeWriteF32(self, 0x588, 0.0f)`，并把各冷却 getter 返 0；
**不要**去恒真那些 `CheckCan*` 谓词（会连带改掉攻击判定，见 §28.1）。

## 29. 功能无效先探针，别先改代码（2026-10-02 疾风骑士）

> 来源：疾风骑士会话复盘黑洞 #1 —— 54 个 hook 在 logcat 里**全部**打出 `hooked`，
> 但用户实测有 5 个功能毫无效果；直到 v5 才加探针，一轮定位。
> 承接 §22「挂点四步门禁」的第 2 步，本节把它落成**默认动作**。
> 也承接 §26「证据优先」——但 §26 面向 Lua/诊断包，本节面向 native hook。

### 29.1 铁律

**「装钩成功」与「打中真实调用路径」是两件事。**
`HookSlotInstall` 返回 true 只证明 `DobbyHook` 装上了、方法名解析到了；
它**不证明**游戏真的走这条路径（内部调用可能绕过该入口、可能有另一个未启用的同类系统、
可能挂的是被同名字段遮蔽的旧实现）。

### 29.2 默认动作

1. **写功能 hook 的同时写探针**，不要等功能报无效再补。每功能配一个编译期可关的
   探针宏（`#ifdef`，参见 §26.1 的 `ModDiag.h` 思路；本仓 native 侧用 `JFQS_TRACE` 式宏）。
2. 探针只在**前 N 次（N=3 足够）**打一行，格式固定：
   `trace <函数名> orig=<原返回值>`（有参数就打参数），避免刷爆 logcat。
3. **首次装机后先读这一行，再判断功能是否有效**：
   - 没有 `trace` 行 ⇒ 该挂点**从未被调用**（挂点错，或功能路径不经此入口）⇒ 换挂点，别改逻辑；
   - 有 `trace` 行但游戏内无变化 ⇒ **被调用了但语义不符**（返回值被上层丢弃、改了副本、
     被另一处覆盖）⇒ 才轮到改逻辑/换返回值位置。
4. 多槽共用一条路径时，探针**按槽独立计数**（承 §25.2：共用计数器会被首个命中吃掉）。

### 29.3 反面教训

- 「日志说 hooked 就是好了」⇒ 5 个功能白跑一轮用户实测。
- 「功能无效就再多挂几个同类谓词」⇒ 正是 §28 的坑，会从「无效」变成「行为错乱」。
- 探针未装就宣称「已验证」= §22 第 4 步明确禁止的假成功。

### 29.4 与正式包的关系

探针宏默认**关闭**，出货二进制里不留探针代码；诊断期打开时建议同时换 `NDK_OUT` obj 目录
（§26.1：ndk-build 不因 CFLAGS 变化重编 `.o`，共用 obj 目录会让诊断包静默退化成正式包）。

### 29.5 三段判定：上游没发生 / 装上了没命中 / 命中了下游不接（2026-10-03 冒险与征途补充）

> 来源：冒险与征途 1.5.9「点看广告没任何反应」——用户报的是**功能没效果**，
> 实际是**上游业务事件根本没发生**（点按钮后连 SDK 回调都没有），
> 却先花时间去对齐 `DS_Advertising::OnADVideoResult(bool,int,ulong)` 的参数。
> 判错段的代价：把 A 段问题当 C 段修 ⇒ 反复改结算逻辑，而事件永远不发生。

用户报「没效果」时，**改代码前先写出结论行**：`判定段=A|B|C 依据=<证据行> 下一步=<动作>`。

| 段 | 现象 | 需要的证据行 | 结论与动作 |
|---|---|---|---|
| **A 上游没发生** | 业务事件本身没产生（点按钮后无任何请求/回调/日志） | 事件链**入口处**的 trace / 业务日志连一行都没有 | 问题在触发链上游：菜单没发出指令 / 前置条件不满足 / 请求没发 / 被别的门禁挡下 ⇒ **去补上游触发**，不要碰结算与判定函数 |
| **B 装上了没命中** | 有 install 行、无 trace 行（或根本没有三行自检） | §30.3 的 `base/entry` + `DobbyHook rc` + `armed` 三行；探针 trace | 挂点/定位错（§30）或路径不经此入口（§29.2）⇒ 换挂点或换判据，别改逻辑 |
| **C 命中了下游不接** | 有 trace 行、参数正确，游戏内仍无变化 | trace + 实参 + 上层返回值/后续调用 | 语义不符：返回值被丢弃 / 改的是副本 / 被别处覆盖 / 结算在服务端（§21 分层选错）⇒ 才轮到改逻辑或换返回值位置 |

- 分流顺序固定为 **A → B → C**：A 段没排除就谈 C 段，等于假设「事件一定发生了」。
- A 段的常见形态：菜单开关没生效（菜单侧读值与 native 侧不一致）、事件被广告 SDK 前置条件挡下、
  请求发出但被服务端/风控拒（此时 native 侧**永远等不到回调**）。
- 只有 C 段才允许进入「重复试不同返回值」的循环；B 段重复试返回值一律视为违反 §29.1。

#### 29.5.1 段判定判据表：症状 → 第一条判据命令（2026-10-10 追迹迷城补充）

> 来源：追迹迷城 1.0.20261009 装机后「菜单不出」——先上 frida Java 桥、`dumpsys` 读 `ApplicationInfo.className`、
> `package_cache`、`packages.xml`（**四条都是结构性死路**），兜了一圈才用「一条 logcat」定案。
> **铁律：先选一条能证伪的命令，再谈取证；重工具（frida / 全量 dumpsys dump）排在最后。**

| 症状 | 第一条判据（一条命令） | 判读 |
|---|---|---|
| 装机后菜单不出 / 功能全无 | `adb logcat -c` → 冷启 → `adb logcat -d \| grep -aE "Starry\|starry_magic\|MenuService\|Mod_Menu\|native menu"` | **零命中 ＝ 段=A（入口没跑）**；有 `native ensureLoaded=true` 但无 `native menu shown` ＝ 段=B；有 overlay 附着但开关无效 ＝ 段=C |
| 自家 so 是否真的注入 | `adb exec-out "su -c 'grep <自家 so 名> /proc/$(pidof <包名>)/maps'"` | 空 ＝ **从未 dlopen**（与「装上了没命中」不是同一层）；有映射但不命中 ＝ 段=B |
| 组件是否被平台登记 | `adb shell am start-foreground-service -n <包名>/<类名>` | `Requires permission … not exported from uid …` ＝ **已登记**（Manifest 补丁的属性被平台认到）；`Not found; no service started` ＝ 未登记 |
| 注入的属性是否被平台认到 | 与官方包 `aapt2 dump xmltree` 逐行 diff | 只应有预期行差异；**aapt2 复读通过 ≠ 平台按 res id 取到值**（resource map 错位时 aapt2 照样正常解析，平台读到 0）⇒ 跑 `04_scripts/_common/axml_inspect.py <manifest> --assert-map` |
| TapTap 没走内嵌网页登录 | `adb logcat -d \| grep -a "AppSwitchEvent\|onAppEnter"` | 出现 `targetName=com.taptap` ＝ **仍拉起客户端**（段=A：入口前置条件没满足，见 `skill/starry-mod/SKILL.md`「TapTap 接入检查表」） |

- **禁止当判据的三条**（实测读不到，别浪费轮次）：`dumpsys package -v` / `cmd package dump` 读 `ApplicationInfo.className`（只打印 `applicationInfo=ApplicationInfo{<hash> <包名>}`）；`/data/system/package_cache/`（只缓存系统包）；`/data/system/packages.xml`（不列组件、不记 className）。
- `dumpsys package` 的 **Service / Activity Resolver Table 只收录带 `intent-filter` 的组件** ⇒ 自家新增的 `<service>` 不在其中属正常，**不能**当作「没登记」的证据。
- 要真凭据时用「与不存在类对照」的写法：`am start-foreground-service -n <pkg>/<不存在类>` 报 `Not found`，两条报错文本不同即可定案。
- 段判定判据与「日志可见性」要分开确认：自家 tag 零命中时，先确认该 ROM 是否抑制了本包 Java 日志（同窗口应能看到同进程的其它 Java tag，如 Unity/SDK 日志）；能看到 ⇒ 零命中就是真结论。

---

## 30. 模拟器 / 翻译层环境下的基址定位与装钩判据（2026-10-03 冒险与征途）

> 来源：冒险与征途 1.5.9 复盘 Q1——用户长期以为「菜单不适配模拟器 ⇒ 功能全无效」，
> 实际菜单装载完全正常，失效的是**框架通用 libil2cpp 基址定位**在模拟器翻译层选不出基址，
> 于是 hook 全不装。旧会话把这三件事混成一件查了很久。

### 30.1 先分流：菜单装载 / 基址定位 / 装钩 是三层

| 层 | 正常证据（日志原文例） | 失败时的典型文案 |
|---|---|---|
| ① 菜单装载（Java/dex 侧） | `星光魔法 MOD 菜单已加载 (MainModern tag=…)`、`[桥接] RegisterNatives N/N 全部成功`、`MOD 已就绪` | 菜单不出现 / RegisterNatives 缺项 |
| ② 基址定位（native 侧） | `interp base=0x… entry=0x… rd=1 rva=0x…` | `本进程里【没有 libil2cpp.so】`、`【libil2cpp 已加载(%d条映射)但选不出加载基址】…多半是这个环境(模拟器翻译层/加固/特殊映射)不吃现有判据` |
| ③ 装钩 | `DobbyHook rc=0 errno=0 orig=0x…` + `armed` | `rc!=0` / 无 armed 行 |

- **模拟器上「菜单能出来」只证明第 ① 层**，不证明功能路径可用。
- 结论写法必须指明层号：不是「模拟器不适配」，而是「第 ② 层判据在翻译层失效，改用显式 RVA 交叉校验后恢复」。

### 30.2 基址多判据顺序（逐级降级；每级都要打印判据本身）

1. **显式 RVA 交叉校验**：候选基址 + 固定 RVA（如解释器入口）读 4 字节指令，与预期常量比对，
   命中才认。一个游戏定下这一对（RVA + 期望字节）后可跨环境复用。
2. **maps 段属性筛选**：按（可执行段 + 段尺寸最大 + 名字含 `libil2cpp`/`anon:libil2cpp.so`）排序取候选，
   逐个送第 1 级校验。**禁止只看第一个命中就认**（同进程可能有多段映射）。
3. **特征码搜索**：在候选段内搜序言字节序列（选跨版本稳定的部分），命中地址 − 序列 RVA = base。
4. **符号 / 字符串**：导出符号地址（`il2cpp_*`）− 符号 RVA；或邻近字面量定位。
5. 每级失败都要打印「级号 + 探到的值 + 否掉的原因」；**四级全失败 ⇒ fail-closed**：
   不装钩、不写内存、直接报告（禁止「先装上看效果」）。

### 30.3 装钩三行自检（每次装机日志必须同时出现；缺一行 = 没装上）

```
[hook] base=0x… entry=0x… rd=1 rva=0x…（判据=第N级）
[hook] DobbyHook rc=0 errno=0 orig=0x…
[hook] armed（<覆盖面：哪些入口走这条路>）
```

- `rd=1` = 写钩前**读回校验**通过（读到预期指令）；`rc=0 && errno=0` = Dobby 装成功；
  `armed` = 业务 hook 已就位。只有 `installed` 而没有 `rd`/`armed` ⇒ 视为未装上（承 §29.1/§29.5-B）。
- 三行缺失时的动作是**修判据**，不是换挂点、不是改功能逻辑。

### 30.4 环境差异必须落文档

- 同一游戏在不同运行环境（真机 arm64 / 模拟器翻译层）的基址判据结果不同 ⇒ 在
  `10_docs/{游戏}/` 记录「环境 + 判据级号 + base + 命中判据」，同环境下次直接复用，不重新探索。
- 模拟器上 hook 数量与改写面都要收敛（翻译层对代码改写/信号链更敏感），优先数据型（§21）。
- **反模拟器清单 ≠ 装钩失败原因**（某游戏 `assets/na.czl` 里的 emulator 黑名单只是不拦而已）：
  先按 30.1 看三层证据，再下结论。
- 「菜单没适配模拟器」不能自动推广到别的游戏（每个游戏 base/RVA 都不同）；
  只有**判据顺序**与**三行自检**是可复用的，且要求每个游戏各自落一条环境记录。

## §31 设备无关单路径与 SELFCHECK（2026-10-04 定）

> 来源：冒险与征途 1.5.9「模拟器全过、真机四项全灭 + 免广告卡死」两轮回归。
> 目标：**一次改动覆盖所有设备**，把「两台设备各改一次代码」这件事从流程上消灭。

### 31.1 铁律：禁止编译期设备分叉

- 任何设备差异**不得**写成 `#ifdef`、机型/ABI 白名单、`isEmulator()` 分支或「先判断是不是模拟器」。
  设备名单永远不全（新机型、新翻译层、云手机、分屏/多用户都会带新的 `maps` 形态）。
- 唯一合法形态：**运行时探测 → 候选策略表 → 失败自动降级**，并在启动日志里说明**本机选了哪条**。
- 自查命令：对功能源码 `grep -n "mumu\|x86\|isEmu\|getprop\|#ifdef" <源码>` —— 只允许命中**注释**，命中代码即为违规。
  （历史：mxyzt 1.5.9 曾有 4 处设备特判注释残留，v45 双映射修复后清零。）

### 31.2 三类设备差异与对应的运行时判据

| 差异 | 运行时判据（不依赖设备名单） | 降级 |
|---|---|---|
| `libil2cpp` 单/多映射（模拟器单份、真机 dual：只读影子 + 执行真身） | 按 `dev:inode` 分组 → 组内地址首尾相接 → 取 **rw-p 总量最大**那组的 `off==00000000` 起点为基址（与 `MxytzMod.cpp` 的 `LibBaseResolve()` 同规则） | 无 rw-p 段时退化为「总长最大组起点」 |
| 代码段改写在目标上不可行（翻译层/r-x 严格写保护） | `DobbyHook` 返回 `rc!=0` 或 `rd` 读回校验失败 | **整体退回纯解释器分发**（不装 inline hook，只靠解释器入口挂点），功能必须仍可用，只是覆盖面变小 |
| 免广告/回调类出口有两条（静态挂点 vs 解释器分发） | 两条路都实现、共用同一个待办/门；不能只依赖其中一条 | 任一条未命中时另一条兜底；两条都未命中才判失败 |

**关键红线**：降级必须是「功能可用但覆盖面变小」，不是「静默失效」。任何降级都要在 STATS/SELFCHECK 里可见。

### 31.3 装钩三行自检之外的第四行：SELFCHECK（功能开启后打印一次）

装钩三行（§30.3）只证明**装上**，不证明**本机走的是哪条路**。功能开启后 5 秒内必须再打一行：

```
SELFCHECK base=0x<选中基址> rd=<0|1> hook=<0|1> interp=<0|1> path=<A|B|both> maps=<组数> page=<页大小> verity=<enforcing|off|?>
```

- `rd` = 写钩前读回校验；`hook` = Dobby 是否就位；`interp` = 解释器入口挂点是否就位；
  `path` = 免广告最终实际生效的那条路；`maps` = `libil2cpp` 映射组数（1=单映射、≥2=多映射）。
- **一行即可判断本机形态**，不必再翻 `maps` 对照表；跨机/跨设备复核时这一行就是证据。

### 31.4 装机前先出设备画像（`04_scripts/_common/device_profile.sh`）

```bash
bash 04_scripts/_common/device_profile.sh --serial <S> --pkg <包名> --rva <名>=0x<HEX> ...
```

- 输出 `10_docs/_state/设备画像/<serial>.md`：设备/ABI/页大小/root、包与安装形态、
  **`libil2cpp` 映射分组与选中基址判据**、每个 RVA 的「段权限 / 是否在选中组 / 内存 16B vs 文件 16B」、
  迭代快路可行性（`extractNativeLibs`、`ro.apk_verity.mode`）。
- 退出码 0=判据正常、2=判据异常 ⇒ **异常就不装机**（省掉一整轮装机+实测）。
- 画像只是**静态预判**，运行时真值仍以 §31.3 的 SELFCHECK 与 §30.3 三行自检为准。
- 只读设备；`--launch` 只 `monkey` 冷启，不 `force-stop`、不清数据。
- 实测范例（OnePlus OPD2404 / Android 16）：`组数=2 选中组=#1 段数=8 rw-p=0x2b7000`，
  两条 RVA 均 `perm=r-xp inbest=yes`；该机 `ro.apk_verity.mode=enforcing` ⇒ **原地改已装 apk 的快路不可用**。

### 31.5 验收阶梯：人工只出现在最后一层

- **L1 静态（AI，出包前）** → **L2 机器（AI 全自动跑模拟器/任一可自动化设备，不打扰用户）** → **L3 人工（用户终审）**。
- **同一个包**走完 L2 与 L3（禁止为模拟器单独出包）；L2 失败不消耗用户实测轮，L2 通过也不构成「要用户在真机再测一遍」的理由。
- L1/L2/L3 的完整规则见 `AGENTS.md`「授权与测试边界 · 验收阶梯」。

### 31.6 迭代快路的设备前提（探到才允许用）

- 想用「root 原地替换已装 apk 内的 so」跳过重签+重装，必须先满足**两个前提**且**实测读过**：
  ① `extractNativeLibs` 为 true 或能定位到磁盘上可直接替换的 lib 副本；
  ② **apk 未受 fs-verity 保护**（`ro.apk_verity.mode` 非 `enforcing`）。fs-verity 开启时改写盘上字节会在读时 `EIO`。
- 该快路**只用于自测迭代**；交付永远走「同签重打 + 全量安装」。

### 31.7 出包提速：哪些真省时间（2026-10-04 实测）

在 886MB 成品包（`冒险与征途_1.5.9_starry-final`）上逐段计时，**结论是打包不是瓶颈**：

| 环节 | 实测 | 结论 |
|---|---|---|
| `cp -f` 复刻底包 886MB | 原 ≈5 秒 → **`cp -c`（APFS clonefile）0.005 秒** | ✅ 真省；脚本已改为 Darwin 上优先 `cp -c`，失败回落 `cp -f` |
| python 外科重写单个 zip entry | **6.5 秒** | 已经够快；注意它**会重压 deflate 条目**（不是「原样搬压缩流」），重写后包必然失去 4 字节对齐 |
| `zipalign -f -p 4`（886MB 读+写） | **1.46 秒** | 不值得为它做复杂优化 |
| `zipalign -c -p 4` 预检后跳过 zipalign | 预检 rc=1（未对齐）⇒ 本游戏不触发 | 只给「改动落在包尾 / 尺寸不变」的包留的保险 |
| 每轮真正的大头 | NDK 重编（已有 obj stamp 增量）＋ `pm install`（40–60 秒） | 想再快只能减少**轮次**，不是减少秒数 |

**推论（写进纪律的理由）**：既然单轮机器耗时主要在「编译+装机」，省时间的手段是**减少轮次**——
诊断与最可能修复同包、一轮一个变量、L2 机器层先把能自动判的都判掉（§31.5），而不是继续抠打包含量。
