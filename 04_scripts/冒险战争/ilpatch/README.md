# ilpatch —— HybridCLR / 热更程序集 IL 补丁器模板

把「加载边界替换整个程序集」的做法固化成一个可直接复制的 C# 工程。
首次成型于 2026-10-02 境界死神激斗；工程骨架与 04_scripts/境界死神激斗/ilpatch/ 同源。

## 何时用

- 目标逻辑在**热更程序集**里（HybridCLR / 解释执行），拿不到稳定的静态 RVA ⇒ 不能在 so 里 hook 具体玩法函数。
- 已经证明（Frida / 日志）存在一个**唯一的加载边界**：运行时把整个程序集的字节交给某个加载 API。
  此时把字节换成打过补丁的程序集，等价于「改了源码再热更」，改动落在托管层，稳定性远高于原生 hook。
- 反例（不要用本模板）：逻辑在原生 so 里、或游戏走 Lua/配置表。先按
  [玩法层定位决策树](../../../skill/re-analyze/SKILL.md) 定层。

## 用法

```bash
# 1) 准备：拿到运行时**已解密**的程序集字节（dump 或 hook 打印），放到项目目录
# 2) 改 Patch.cs：CFG_PATH、FindType 的名字、补丁方法体、Targets、Gate 断言
# 3) 跑
~/.dotnet/dotnet run                          # 用 Patch.cs 顶部的默认 in/out 路径
~/.dotnet/dotnet run -- <src.dll> <dst.dll>   # 显式指定
~/.dotnet/dotnet run -- --dump <Full.Type> <method|*>   # 只看 IL（排障）
~/.dotnet/dotnet run -- --probe [<dll>]       # 诊断写盘期 AssemblyResolutionException
```

退出码 0 = 出包自检 PASS，可以进入 `.a` 内嵌 → so 构建 → 外科出包；非 0 = 禁止出包。

## 复制到新游戏时的替换点

| 位置 | 改什么 |
|---|---|
| `Patch.cs` 顶部 `CFG_PATH` | 改成该游戏包名的私有目录绝对路径（解释器下 `Application.persistentDataPath` 常为空串，别依赖） |
| `Patch.cs` `Run()` | 换 `FindType/FindM` 的名字与参数个数；换 `P(...)` 补丁清单 |
| `Patch.cs` `BuildStarryConfig()` | 保留原样即可（读文本配置的注入类型，各游戏通用） |
| `Patch.cs` `Targets` | 与 `P(...)` 清单一一对应，供 `Program.cs` 打印完整 IL |
| `Patch.cs` `Gate()` | 每个补丁配一条**意图断言**（见下） |
| `Program.cs` 顶部 `root/src/dst` | 改成新项目路径 |
| 原生侧 | 补丁程序集比原文件大时，必须在加载边界按**新长度**重新分配容器再替换参数 |

## 三条硬检查（最容易漏，漏了就是整轮出包+装机报废）

1. **参数下标**：`MethodDefinition.Parameters` **不含 `this`**。
   `SlotToParamIndex(slot) = HasThis ? slot - 1 : slot`。
   写回参数（`starg`）落点必须按此反推，并断言参数**名字与类型**。
   （历史事故：把 `Int64` 伤害值写进了相邻的 `Int32` 技能 id，签名与 IL 全都合法，运行时才炸。）
2. **写盘期引用解析**：`AssemblyDefinition.Write` 会对**每个** Param/Field/Property 走
   `GetConstantType → CheckedResolve(成员类型)`，**任何**解析失败都中止整包写盘 ——
   哪怕那个成员和本次补丁毫无关系。
   本模板用 `TolerantRefs.cs`（垫片工厂 + 宽容解析器 + 常量台账）在**没有引用程序集**的机器上也能跑，
   验收判据是产物 sha256 与提供齐全引用时**逐字节一致**。
3. **字节容器容量**：产物比原文件大时不能原地覆盖；缩小 `max_length` = 截断。

细节与理由见 `skill/re-implement/hotfix-dll-replace-pattern.md` 的「Cecil 检查清单」第 5/6/7 项。

## 其它纪律

- 补丁一律**入口前置门控**：配置缺失/关闭时原 IL 原样执行。不要删改原指令，只做插入。
- 门禁打印**完整** IL；只打前 N 条会漏掉 N 之后的人工检查点。
- 汇编器产生的 in/out 都别入库（`.a`、`.dll` 走 `.gitignore` 例外规则）。
- 门禁是「意图断言」不是「没报错」：断言要能区分「注入对了」和「注入到一个错误的落点」。
