# Hook 通用铁律

> 📍 路径：`re-implement/hook-rules.md`
> 🎯 触发场景：编写任何 Dobby inline hook 或 Cocos JS splice 之前必读

---

## 0. 标准框架与生成器（新游戏一律从这里开始，全游戏适用）

新游戏 `Games/*Mod.cpp` **禁止从旧游戏复制**（会把旧坑一起抄进来），一律用生成器从标准骨架 v2 产出。v2 已把防闪退设施内置为代码：默认 0 hook 懒装、双路径挂点（RVA+按名，谁成功用谁）、RVA 漂移拒绝、幂等安装、orig 必调、reenter、SafeWrite、方法缺失降级、Tick 轻量。

```bash
bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}   # 生成 {Game}Mod.h/.cpp + 打印三处接线 TODO
```

- **铁律全文**：`mod菜单/starry-mod-menu/docs/GAME_MOD_GUIDE.md`
- **标准骨架**：`Games/_TemplateMod.h/.cpp`（v2；只填 ①②⑤⑥ 的 TODO 区，勿自造内存读写）
- **正面范例**：`Games/Archero2Mod.cpp`（按名 hook）、`Games/WhrhxMod.cpp`（RVA 双路径，模板 v2 蓝本）
- **反面教材**：`Games/HzsjMod.cpp`（共享跳板 + SafeProbe 热路径，闪退/掉帧教训）
- 存量旧 Mod 不强制回改；但往旧 Mod **新增 hook** 时必须沿用模板的安全助手与懒装模式，不许再写裸解引用/启动即 bulk hook

## 1. Dobby inline hook 标准模式

函数入口首指令改成 `B` 跳转 → Dobby veneer → 替换函数 → 替换函数内读开关旗标 → 决定行为 → `br` 原函数。

```c
// 标准模式
DobbyHook(base_va, replacement, &orig);
// replacement 内部：
//   if (开关开) { 修改参数/返回值 }
//   return orig(args...);  // 必须调原函数
```

## 2. base_va 计算（三种方式）

| 方式 | 公式 | 适用场景 |
|------|------|----------|
| dlsym | `dlsym(handle, "符号名")` | 有导出符号的函数 |
| maps 偏移 | `map_start + (file_off - map_file_off)` | 无符号，从 IDA file_off 算 |
| ❌ 错误 | `base + RVA` | RX 分段映射时地址错误 |

**注意**：`file_off ≠ RVA`。ELF bias 通常 `RVA = file_off + 0x4000`，但不同游戏可能不同。运行时 VA 必须用 maps 偏移算，不能直接 `base+RVA`。

## 3. 必须调原函数（orig）

**不调 orig → SIGSEGV。**

- 替换函数内必须通过 `orig` 指针调用原函数
- 不调 orig 时，参数会被当作指针解引用 → 野指针 → SIGSEGV
- 即使只想改返回值，也要调 orig 拿到原始结果再改

```c
// ✅ 正确
int replacement(int a, int b) {
    if (开关) return 改后的值;
    return orig(a, b);
}

// ❌ 错误：不调 orig
int replacement(int a, int b) {
    return 改后的值;  // 参数被当指针 → SIGSEGV
}
```

## 4. 双 Dobby 叠同一函数 → SIGSEGV

- 星光和斌哥壳（或其他 hook 框架）同时 hook 同一个函数 → SIGSEGV
- 如果该函数已被斌哥壳 hook，星光不能再 hook
- 解决：找到斌哥壳的 hook 位置，在其外围做过滤，或换一个 hook 点

## 5. 偏移必须来自实测

- **不猜测偏移**。所有 RVA/offset 必须来自 Il2CppDumper 产出或 memdiff / **运行时 Trace 实测**
- 游戏更新后偏移可能变化，必须重新 dump 验证
- 静态分析的偏移需要运行时验证确认
- **功能无效时**：先 `StarryTrace` 打 `types ok` + 命中/`patch=` 计数，证实挂点与 FieldOffset 再改逻辑（见 `10_docs/_templates/经验_功能不对先Trace证实挂点偏移.md`）

## 6. file_off 与 RVA 的区别

| 概念 | 含义 |
|------|------|
| file_off | ELF 文件中的偏移（IDA 显示的地址） |
| RVA | 相对虚拟地址（相对模块基址） |
| ELF bias | 通常 `RVA = file_off + 0x4000`，但不保证 |

从 IDA 拿到的是 file_off，需要转换为 RVA 或直接用 maps 偏移法算运行时 VA。

## 7. 运行时 VA 用 maps 偏移算

```bash
# 获取进程 maps
adb shell su -c 'cat /proc/<pid>/maps | grep libil2cpp'
# 找到 RX 段的 map_start 和 map_file_off
# 运行时 VA = map_start + (file_off - map_file_off)
```

**勿用 `base+RVA`**：libil2cpp 的 RX 段可能是分段映射，base 地址不一定是 RX 段起始。

## 8. hook 选择：具体方法 > 有过滤跳板 > 静态 patch

- **首选**：hook 具体方法（`SetTarget` / `UpdateHp` / `GenerateDamage` 等）。只有该方法被调才触发，调用频率低、逻辑短、性能开销最小
- **次选**：HybridCLR 公共 invoke 跳板 + 方法名过滤（斌哥同款；Hzsj 用这个，可行但必须满足下面约束）
- **末选**：静态 patch
- **例外且仅此一种**：部分 HybridCLR 方法的 `MethodPointer` 不可靠（魂坠深境 `GenerateDamage` 实锤钩不到真实结算），此时才允许 hook 斌哥同款公共跳板入口
- **如果 hook 共享跳板，必须全部满足**：方法名过滤 + 必须调 orig + 方法分类缓存（够大，且功能切换时清空）+ 方法名/类名解析只在第一次（命中缓存后续零解析）
- **禁用**：共用入口无过滤（所有方法都走同一个跳板、无法区分）

## 9. 禁止堆扫托管对象

- Unity GC 会移动托管对象，堆扫描找到的地址随时失效
- 不要用 `ScanInstances` 扫描 IL2CPP 对象
- 正确做法：通过静态字段（如 Dictionary）遍历，或 hook 函数参数获取对象引用
- 终末地实锤（v11-v13 五连闪退主因）：/proc 扫 klass 指针命中 5000+ 全是元数据假阳性，真实例从未命中；runtime_invoke 失败值 -1 撞上未解析阵营值 -1 → 对垃圾对象写 HP。**游戏实例定位禁止堆扫，一律走 hook 参数/字段链**

## 10. 防钻牛角尖

- 同一个问题最多试 2 种方法
- 第一种不行 → 换思路（如 ChangeHP 不行试 DeductHP）
- 第二种也不行 → 停下来重新分析，不要继续试第三种
- 典型弯路：Keypatch libcocos2djs.so（Cocos JS 玩法不在 native stub 里）

## 11. 内存读写必须先校验指针（防闪退）

- 任何托管对象解引用前先过 `IsValidHeapPtr`（或 `SoftPtr`）
- 写字段封装助手，禁止裸解引用：

```c
static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (obj && Il2Cpp::IsValidHeapPtr(obj)) Il2Cpp::WriteI32(obj, off, v);
}
```

- ❌ 禁止：`*reinterpret_cast<int32_t*>((uintptr_t)obj + off) = v;`（对象指针无效时直接 SIGSEGV）

## 12. SafeProbe 只准一次性初始化用（禁进热路径）

- `SafeProbe` 每次调用都会 `sigaction` 装/卸 SIGSEGV+SIGBUS 信号处理器
- 且崩溃发生在**非扫描线程**时会直接 `raise` 让进程死掉
- ✅ 只允许在 `InstallHooks` 等一次性初始化里用
- ❌ 禁止在 hook 函数 / Tick / 每帧循环里用；热路径一律用指针校验 + 直接读写

## 13. 类名/方法名解析必须缓存（防掉帧）

- `object_class` + `class_name` 是字符串解析，禁止每帧重复调用
- 单槽缓存：obj 的 `klass` 未变就复用已解析的类名（参考 `Games/Archero2Mod.cpp` 的 `CacheClassName`）
- 共享跳板的方法分类缓存：命中缓存后零解析；缓存要做够大；功能切换时清空重分类

## 14. 重入保护（防死循环 SIGSEGV）

- hook 内部再次触发同类 hook（如 AOE 逐个调 `UpdateHp`、偷技能里调 `AddSkill`）必须用 `reenter` 旗标防止递归
- 模式：

```c
static std::atomic<bool> g_reenter{false};
static void Hooked_XXX(void* self, ...) {
    if (g_reenter.load()) { if (orig) orig(self, ...); return; }
    g_reenter.store(true);
    // 在这里调 orig / 扩散
    g_reenter.store(false);
}
```

## 15. 写新游戏前先看标准实例

- 从 `Games/_TemplateMod.h/.cpp` 复制（见第 0 节）
- 对照 `Games/Archero2Mod.cpp`（正面）/ `Games/HzsjMod.cpp`（反面）自查

## 16. 懒加载分组 Hook（禁止「启动即 bulk hook」，全游戏生效）

- **禁止** lib 一加载就 Dobby 几十个点：用户没开任何功能也改写了 il2cpp 热路径，部分机型/时机 SIGSEGV/SIGILL（无悔华夏旧包 40+ hook 闪退即此因）
- 标准模式（任意引擎/游戏通用）：
  - `g_libReady` 只表示 il2cpp 已映射，**不表示已 hook**
  - 默认**全关 = 0 个 Dobby hook**
  - 开关 ON → `SyncHooks()` 按功能组安装（经济/军队/人口/战斗/加速…），共用 hook 去重
  - 开关变化（`OnFeatureChanged` / Tick）时再 `SyncHooks`
  - **SyncHooks 并发防护**（终末地 v10 实锤：浮窗线程与主线程同时装 → 对同一函数重复 DobbyHook → 被保护runtime毒杀）：互斥锁 + 装成即停（装前先查已装）+ 熔断（同一 hook 连续 3 次 Dobby 失败 → 放弃并记录，不再重试）
- 分析阶段发现的坏挂点（SIGILL/SIGSEGV 实锤过的）**永久禁用**，注释写明原因，换等价语义路径实现同一功能

## 17. 可选方法缺失不得阻断整包安装（全游戏生效）

- 方法名解析用**多候选**逐一尝试；Il2CppDumper 失败/版本差异时以 `StarryTrace` 运行时 dump 补名
- 任何 hook 的前置方法缺失时：只跳过依赖它的功能 + log 警告，**其余 hook 照装**——把单个方法当硬前置，一个缺失就全包不装，功能集体失效（弓箭传说2国际版 1.0.1 实锤）
- 扩散类功能（一次攻击全屏受伤等）依赖运行时实体列表：用**近期实体缓存**字段扩散；**禁止**全量实体字典/列表热路径遍历（SIGSEGV 实锤）

## 18. 桩签名三查（装 hook 前必须先 CalcSig）

**桩签名错 = 功能恒 0 / 无效，且不崩——最隐蔽的一类 bug**（终末地 v14.0/v14.1 伤害恒 0 的根因）。

**SOP**：装 hook 前先打签名探针——`il2cpp_method_get_return_type` / `method_get_param` / `type_get_name` / `method_get_flags` 一次拿全，**按签名写桩**，不按猜的写。

1. **返回类型**：`double` 返回走 d0，float 走 s0，int/指针走 x0——桩的位型全错就拿垃圾；wrapper 内再调其他函数会清掉 d0，返回值要保住
2. **FP 参数寄存器**：参数含 float/double 时，整数桩（void*）会打坏 d0-d7 → 公式拿垃圾。终末地参数恰好全 byref 指针才幸免——只要有一个 FP 参数就必须按 ABI 写对
3. **static 判定**：`il2cpp_method_get_flags & 0x10`。static = argc 槽 + method；instance = argc 槽 + this。兜底桩少转发 1 槽 = 参数错位（伤害恒 0 实锤）

## 19. 基类方法指针 ≠ 派生 override（虚方法必须按 klass 解析）

- 直调基类方法指针，派生类对象走不到 override → 读到错误结果（终末地敌我判定：怪是 `AbilitySystemForEnemyPart` 等派生类，直调基类 `get_factionIndex` 读不到正确阵营）
- 正确：按对象**实际 klass** 解析 override（`il2cpp_object_get_virtual_method`），**每 klass 缓存**（同 §13：klass 未变零解析）

## 20. 主线程 hook 回调零解引用（学习逻辑放专用学习线程）

- hook 回调（游戏主线程/渲染线程进入）**禁止解引用任何未验证指针**——参数可能是垃圾指针，解引用即 SIGSEGV（终末地 v6-v8 三连闪退主因）；学习线程 + SEGV handler 兜底也会被 TP 毒杀，不要指望兜底救命
- 主线程回调内只做 **memcmp 级操作**：读对象头拿 klass 指针 + 与缓存 klass 指针比对（指针值比较，不解引用 klass 内容）
- 对象学习（类名解析、敌我判定、字段扫描）放**专用学习线程**：`SafeProbe` 只准学习线程用，且仅在每个类**首次出现**时探一次，之后零探针
- 类名读取走「memcpy 读对象头 + klass 缓存」策略，判玩家/怪用类名含 `Character`/`Avatar`/`Player` 等关键词
