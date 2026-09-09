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

### 热更字段偏移不要 +8
`il2cpp_field_get_offset` 返回值已包含 HybridCLR bias，**不要再 +8**。AOT 时代需要 +8 是因为对象头，HybridCLR 已内置。

### il2cpp_runtime_invoke 调热更方法会 SIGSEGV
`il2cpp_runtime_invoke` 调 AOT 方法没问题，但调热更方法会 SIGSEGV（地址 0x0）。必须用跳板法或直接 Hook 方法入口。

### 堆扫描不可靠
通过堆扫描找对象实例不可靠。Dictionary 中大量实例扫不到（实测 ~7/43）。应通过静态字段（如 `Archive.heroArchiveDic`）遍历。

### 跳板对货币 return 不调 orig → SIGSEGV
跳板命中货币方法时如果 `return (void*)1` 不调 orig → SIGSEGV。所有路径都必须调 orig。

### strcmp 野指针 → 崩 libstarry_magic
`method_get_name` 返回的字符串可能是野指针，strcmp 前需检查非 NULL。

### SoftPtr 范围
HybridCLR 的 SoftPtr 有效范围 `0x10_0000_0000…0x80_0000_0000`。旧范围 `0x70-0x80` 会漏 `0x6f` 堆区域。

---

## 六、验证方法

- 运行时 `il2cpp_field_get_offset` 校准偏移 + 功能生效
- logcat 确认跳板命中 + 参数修改正确
- 功能语义匹配（秒杀/无敌/货币修改等）
