# Unity IL2CPP 从零抓点

> 📍 路径：`re-extract/il2cpp-from-scratch.md`
> 🎯 触发场景：游戏是 Unity IL2CPP（`libil2cpp.so` + `global-metadata.dat`），**没有斌哥改包**，需要自己定位玩法 Hook 点

---

## 〇、先花 60 秒判断「能不能静态 dump」（2026-10-01 增加）

**不要默认先跑 Il2CppDumper。** 加固包（网易易盾 NIS 一类）会把 metadata 加密、节表销毁、dynsym 全随机化，Il2CppDumper 与 Cpp2IL 都是死路，跑完只浪费时间。先做三条零成本检查：

```bash
# ① metadata 魔数：正常 af1b b1fa；被壳加密常见 48 54 50 58（"HTPX"）
xxd -l 8 assets/bin/Data/Managed/Metadata/global-metadata.dat

# ② 节表是否还在：被壳处理过的 so 只剩 program header，没有 section
readelf -S lib/arm64-v8a/libil2cpp.so | head -20

# ③ dynsym 是否被改名：正常应有大量 il2cpp_*；被壳处理过命中 0
readelf --dyn-syms lib/arm64-v8a/libil2cpp.so | grep -c il2cpp
```

| 检查项 | 正常 | 被壳保护 → 静态路线否决 |
|---|---|---|
| metadata 魔数 | `af 1b b1 fa` | `HTPX` 或其他非标准值 |
| `readelf -S` | 完整节表 | 节表被销毁（只剩段） |
| `il2cpp_*` dynsym 命中数 | > 0 | 0 |
| 内存态 so 与磁盘 so 差异 | 接近 0 | 大面积（失落城堡实测 69.9%，仅 `[0,0x3993000)` 被加密） |

**任一格落在右列 ⇒ 立刻停止静态路线**，不要换工具重试。改走：

1. 从 `/proc/<pid>/mem` dump 运行时**已解密**的 metadata（壳是原地解密的，文件映射段即可读）；
2. 用 metadata 静态枚举类/方法/字段名（**只要名字，不要 RVA**）；
3. 运行时按名挂载 Hook（星光框架 `HookSlot` 的 `HookByName` 路径）。

详细症状、停止条件与实机证据见 `re-analyze/protection/nesec-inmemory-dex.md` §9。

---

## 一、Il2CppDumper 使用

> ⚠️ 跑之前先过 §0 前置判定；被加固过的包在这里会直接失败或静默出错。

### 输入
- `libil2cpp.so`（从 APK `lib/arm64-v8a/` 提取）
- `global-metadata.dat`（从 APK `assets/bin/Data/Managed/Metadata/` 提取）

### 输出
- `dump.cs`：类/方法/字段完整信息
- `script.json`：RVA → 方法名映射（IDA 用）
- `script.py`：IDA Python 脚本，自动命名函数

### IDA 导入
1. IDA 打开 `libil2cpp.so`
2. File → Script file → 运行 `script.json`
3. 函数自动命名为 `ClassName.MethodName`

---

## 二、ELF bias 说明

- `RVA = file_off + 0x4000`（部分游戏如此，**不是所有 so 都这样**）
- 不同编译器/链接器 bias 不同
- 验证方法：找一个已知函数（如 `il2cpp_init`），对比 IDA 中的 file offset 和 dump.cs 中的 RVA

---

## 三、运行时 VA 计算

```
运行时 VA = map_start + (file_off - map_file_off)
```

- `map_start`：`/proc/pid/maps` 中 `libil2cpp.so` 的 RX 段起始地址
- `file_off`：IDA/ELF 中的文件偏移
- `map_file_off`：`/proc/pid/maps` 中该段的文件偏移

**注意 RX 分段映射**：ARM64 下 `.text`（RX）和 `.data`（RW）是分开映射的，计算 VA 时要用对应段的信息。

---

## 四、KSU /proc/pid/mem dump 法

**不用 Frida**，壳反调试会杀进程。

```bash
# 1. 获取 PID
adb shell su -c 'pidof <pkg>'

# 2. 获取内存布局
adb shell su -c 'cat /proc/<pid>/maps' > maps.txt

# 3. dump 目标段（用 memdump64 C 工具，toybox dd 会溢出大块）
adb shell su -c '/data/local/tmp/memdump64 <pid> <start_addr> <size> /data/local/tmp/dump.bin'
adb pull /data/local/tmp/dump.bin
```

---

## 五、字符串搜索定位法

在 `libil2cpp.so` 或 `global-metadata.dat` 中搜索关键词：

| 关键词 | 功能 |
|--------|------|
| `damage` / `Damage` / `ChangeHP` | 伤害修改 |
| `health` / `Health` / `DeductHP` | 血量扣减 |
| `score` / `Score` | 分数 |
| `gold` / `Gold` / `Coin` | 货币 |
| `skill` / `Skill` / `CD` | 技能冷却 |
| `isCanRelease` / `CheckCurSp` | 技能释放检查 |

在 `dump.cs` 中搜索这些关键词 → 找到类名/方法名 → 用 `script.json` 查 RVA → IDA 分析函数逻辑。

---

## 六、游戏更新后 RVA 重算流程

1. 新包跑 `Il2CppDumper` → 按类/方法名重算 RVA
2. 核对签名相关 MD5 是否变化
3. 检查关键方法（如 `ClientLoginUtils.isClientLogin`）是否仍存在
4. 更新 Hook 代码中的 RVA 常量
5. 快速重建 so 并测试
6. log 验证：`auth game_id synced` + 初始化成功

---

## 七、坑/弯路

### 4KB 拆页 = Dobby 特征
函数入口被 Dobby inline hook 后，所在 4KB 页会被拆分为新映射。看到 4KB 拆页就是 Dobby hook 的标志，不是游戏原始代码。

### libil2cpp mem vs APK 主段有加载对齐差
`libil2cpp.so` 内存中的 dump 和 APK 中的文件逐字节对比时，可能有加载对齐差。这不是 patch，是正常的 ELF 加载行为。

### 不要直接 base+RVA
勿直接 `base + RVA` 计算地址；注意 RX 分段映射，需用 `map_start + (file_off - map_file_off)`。

### HybridCLR 误判
如果 AOT dump 找不到玩法方法，可能不是方法不存在，而是 HybridCLR 热更。检查 APK 内是否有 `GameLogic.dll.bytes` → 走 `hybridclr-runtime.md`。

---

## 八、加固包路线：内存态 metadata + 按名 Hook（2026-10-01 失落城堡实测）

§0 判定落在「被壳保护」时走本节；症状与停止条件见 `re-analyze/protection/nesec-inmemory-dex.md` §9。

### 8.1 取运行时已解密的 metadata

壳是「运行时原地解密、写回同一映射」，所以文件映射段直接可读：

```bash
# 找到文件映射段（rw-p，路径指向游戏私有目录下的 global-metadata.dat）
adb shell su -c "grep global-metadata /proc/$(pidof <pkg>)/maps"

# 失落城堡实测（外置目录，注意不一定是 /data/data/）：
#   13.55 MB rw-p ... /storage/emulated/0/Android/data/<pkg>/files/il2cpp/Metadata/global-metadata.dat
# 用 memdump64（大块 dd 会溢出）dump 出来，魔数应为 af1b b1fa
adb shell su -c "/data/local/tmp/memdump64 <pid> <start> <size> /data/local/tmp/meta.bin"
adb pull /data/local/tmp/meta.bin
```

坑：
- 落盘路径可能在 `/storage/emulated/0/Android/data/<pkg>/...`（外置），`/data/data/<pkg>/...` 也要一起 grep。
- 一定要用 `memdump64`（`04_scripts/_common/memdump64`），toybox `dd` 读大块会溢出。

### 8.2 按名枚举（**不解 RVA**）

metadata 里类型/方法/字段名是明文，直接取。要点：

- v24 里 `Il2CppType` 对象在二进制中不可得，签名还原只能用 `ByvalTypeIndex.Value → TypeFullName` 反查表。
- ⚠️ `Il2CppMethodDefinition.InternalParameterData` 会抛 `System.InvalidOperationException: OwningContext has not been initialized @ LibCpp2IL/ReadableClass.cs:13`；必须用 `GetParameterDefinitionFromIndex(Il2CppVariableWidthIndex<Il2CppParameterDefinition>.MakeTemporaryForFixedWidthUsage(m.parameterStart.Value + i))`。
- 已知可用实现：`01_tools/MetadataSearch`（模式 `types` / `methods` / `dump` / `fields` / `str` / `sig`）。
- 产出落 `06_dumped/{游戏}/memresolved/meta/{types.txt, dump_all.txt}`；两者是 MB 级文本，**读取一律 `grep -m` / 按 TYPE 块切片**，不要整读。

### 8.3 按名挂载

星光框架 `HookSlot` 的 `HookByName` 路径只需 `class_from_name` + `class_get_method_from_name(name, argc)`，与 RVA/符号完全无关。**代价**：同名且参数个数相同的重载无法区分 —— 见 `re-implement/hook-rules.md` §27。
