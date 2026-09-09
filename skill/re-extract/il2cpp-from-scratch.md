# Unity IL2CPP 从零抓点

> 📍 路径：`re-extract/il2cpp-from-scratch.md`
> 🎯 触发场景：游戏是 Unity IL2CPP（`libil2cpp.so` + `global-metadata.dat`），**没有斌哥改包**，需要自己定位玩法 Hook 点

---

## 一、Il2CppDumper 使用

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
