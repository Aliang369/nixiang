# HybridCLR 热更抓点

> 📍 路径：`re-extract/hybridclr-runtime.md`
> 🎯 触发场景：Unity 游戏有 `libil2cpp.so` + `global-metadata.dat`，但 Il2CppDumper dump 后找不到玩法方法（如 `ChangeHP`、`GenerateDamage`）

---

## 一、检测

### 确认条件
- `libil2cpp.so` + `global-metadata.dat` 都在
- Il2CppDumper dump 后，AOT 中找不到玩法相关方法
- APK 内有 `GameLogic.dll.bytes` / `Assembly-CSharp.dll`（文件头 `QK` = 加密标记）

### 原因
HybridCLR 将玩法逻辑放在热更 DLL 中，运行时动态加载到内存。AOT 的 `global-metadata.dat` 只包含框架代码，不含热更逻辑。

---

## 二、运行时内存 scan + dump PE

### 步骤
1. 运行时在进程内存中 scan `MZ` 头（`0x4D5A`）+ PE 头特征
2. 在 `0x7640000000` 类大 anon 区搜索
3. dump 完整 PE（.NET 程序集）

```bash
# 获取 PID
adb shell su -c 'pidof <pkg>'
# 获取内存布局
adb shell su -c 'cat /proc/<pid>/maps' > maps.txt
# 在大 anon 区搜 MZ 头，dump 完整 PE
```

### 反编译
```bash
ilspycmd <dumped.dll> > decompiled.cs
```

从 `#Strings`（.NET 元数据字符串堆）中找玩法符号。

---

## 三、运行时按名解析

**不能用静态 RVA**（热更每次加载地址不同）。

### IL2CPP 运行时 API
```c
// 按类名获取 class
Il2CppClass *klass = il2cpp_class_from_name(image, namespace, classname);
// 按字段名获取偏移
int offset = il2cpp_field_get_offset(field);
```

### 流程
1. 用 `il2cpp_class_from_name` 按命名空间+类名获取 class 指针
2. 遍历 field 获取字段偏移
3. 用 `il2cpp_field_get_offset` 返回值直接作为偏移使用

---

## 四、公共 invoke 跳板法

当热更方法无法直接按名解析时，使用跳板法：

### 定位跳板
1. 做 OFF vs ON memdiff，找到**唯一**差异 stub
2. 该 stub = 公共 invoke 跳板（所有热更方法调用都经过它）

### Hook 跳板
```c
DobbyHook(trampoline_addr, replacement, &orig);
// 在 replacement 中：
// 1. 用 method_get_name(method) 获取方法名
// 2. strcmp 判断是否目标方法（如 "GenerateDamage"）
// 3. 命中则读/改参数
// 4. 必须调 orig（不调会崩）
```

### 参数访问
- `params[0]` = 第一个参数对象（如 `DamageCommand`）
- 按偏移改字段（如 `baseDamage@0x10`）

---

## 五、坑/弯路

本节旧事故记录缺少完整样本与调用条件，不作为 HybridCLR 通用定律；原因尚无可复查证据的部分保持 **NOT PROVEN**。

### 字段偏移须明确参考对象
核对运行时版本、字段类别、对象/装箱布局以及 API 返回偏移的参考点，不机械加减 8。不能把 AOT 与热更的对象头差异概括成固定常数规则。

### 运行时调用失败不证明热更普遍不可调用
历史笔记记载过运行时调用发生空地址崩溃，但未在本节提供完整签名、参数、异常输出和线程/初始化证据；不能断言所有热更调用都会崩，也不能据此强制换成某种 Hook。应先核对真实目标、调用契约和目标运行时支持条件。

### 对象发现不等于生命周期有效
旧笔记的扫描命中比例仅属历史观察，不能推广。无论对象来自扫描、参数还是字段链，都须验证类型、生命周期与访问线程；遵循 [Hook 对象访问约束](../re-implement/hook-rules.md)。

### 原调用与副作用
旧笔记记录过替换返回值后崩溃，但“不调用 orig”不是参数变为野指针的必然原因。保留原调用契约与副作用，并核对 ABI、返回表示和生命周期；具体要求见 [hook-rules.md](../re-implement/hook-rules.md)。

### 字符串访问边界
非 NULL 不证明字符串可读或存在终止符。核对来源、有效期及有界访问；`strcmp`/`memcpy` 同样访问内存，不能作为安全保证。

### 指针范围不是对象证明
固定地址范围只可能是某设备/进程的启发式，不是 HybridCLR 通用有效地址区间。以当前映射、权限及对象契约取证；地址在映射内也不证明对象仍存活。

---

## 匿名映射与方法表候选（合并 2026-09-05）

- **适用范围**：有权分析的 IL2CPP 运行时镜像、maps 与匹配 metadata；案例为无限升级 11.266018，不能直接推广到其他 Unity/HybridCLR 版本或解释执行的业务方法。
- **历史证据**：[无限升级记录](../../10_docs/无限升级/运行时dump与YAES破解_20260905.md) §一、§二、§五。本轮仅文档合并，无运行复现；历史报告中的“通用替代路线”应按以下条件收窄。
- **历史事实**：记录在文件映射之外的 `[anon:.bss]` 找到模块名指针、CodeGenModule 与方法表候选，并记载部分方法地址定位。仅按 `il2cpp` 文件名筛选 maps 会漏掉匿名段；匿名命名本身也不证明段归属。
- **事实与推断分开**：CodeGenModule/方法表定位成功，不等于完整恢复 `CodeRegistration` 和 `MetadataRegistration`。两 Registration 都被搬迁或已恢复的结论，须分别提供结构、关联与布局证据，否则 **NOT PROVEN**；自动工具返回 0 也不足以证明搬迁根因。
- **候选定位知识**：在已授权取得的映射与镜像中，可把模块名字符串及其引用作为候选线索；需交叉核对名称、计数、指针数组、所属程序集与目标可执行区，单一字符串命中不算确认。这里不新增保护绕过或采集步骤。
- **token/rid 限定**：`methodPointers[rid-1]` 只可在已核对的 AOT 模块、metadata 版本和对应 CodeGenModule 布局下作为候选映射；先确认 token 类型、rid 所属 image/module、计数与边界，并处理空项、共享/泛型等布局差异。不能拿全局方法索引混充模块 rid，也不能由热更 token 推出独立 AOT 入口。
- **全段映射校验**：保留完整映射区间及权限；验证结构、数组整个字节范围和每个目标指针，不仅验证首地址。跨段数据须逐段核对连续性、来源与覆盖，缺段、越界或未知布局就停止该解析并标待验证。
- **坐标规则**：文件偏移、ELF VA、运行时绝对地址、load bias 分开记录；某个样本 `.text` 的 VA 与文件偏移相等不代表其他段也相等。换算和 BSS 无对应文件字节的边界统一见 [hook-rules.md](../re-implement/hook-rules.md) §2、§6–7，不另立算法。
- **样本脚本待复现**：[resolve_il2cpp_method_va.py](../../04_scripts/无限升级/resolve_il2cpp_method_va.py) 仅作案例线索。上轮审阅已发现其第 100–105 行相对/绝对坐标混用、跨段处理不足；本轮不改代码、不运行脚本。历史地址报告不证明现存脚本端到端可复现，不能标为“通用已验证工具”。
- **证据门禁**：复现须绑定样本身份、版本/布局、完整映射与地址字节对应关系；未覆盖的完整注册恢复、解释器入口和跨版本适用性保持 **NOT PROVEN**。合并状态不等于全部命题 PROVEN。

---

## 六、验证方法

- 运行时 `il2cpp_field_get_offset` 校准偏移 + 功能生效
- logcat 确认跳板命中 + 参数修改正确
- 功能语义匹配（秒杀/无敌/货币修改等）
