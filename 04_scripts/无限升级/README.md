
## 补丁产物校验工具链（2026-11-19 起，**五关必过**）

| 工具 | 目录 | 查什么 | 判据 |
|---|---|---|---|
| `ilverify` | `04_scripts/无限升级/ilverify` | 31 条落点 + delta 层断言 | `PASS=76 / FAIL=0`（`stage=merged`） |
| `ilstack` | `04_scripts/无限升级/ilstack` | 全模块求值栈数据流（含 EH） | `violations=0` |
| `ilgeo` | `04_scripts/无限升级/ilgeo` | 方法体几何（重叠/code size/EH 越界） | `RESULT: CLEAN` |
| `ilrid` | `04_scripts/无限升级/ilrid` | RID 连续性（官方前缀逐行比对） | 前缀 `不一致 0 行` |
| **`ilraw`** | `04_scripts/无限升级/ilraw` | **字节级**：指令边界 / 分支目标 / switch 目标 / EH 偏移 / 短跳位移范围 | **`RESULT: CLEAN`（`brBad=0 ehBad=0 shortOver=0`）** |

### 用法

```bash
DN=/Users/aliang/.dotnet/dotnet
BASE=07_patched/无限升级/13101_new/targetfuntional_13101new_allfix.dll

$DN run -c Release --no-build -- "$BASE" merged   # 在 ilverify 目录
$DN bin/Release/net10.0/ilstack.dll "$BASE"       # 在 ilstack 目录
$DN bin/Release/net10.0/ilgeo.dll   "$BASE"       # 在 ilgeo 目录
$DN bin/Release/net10.0/ilrid.dll   <官方> "$BASE" # 在 ilrid 目录
$DN bin/Release/net10.0/ilraw.dll   "$BASE"       # 在 ilraw 目录
```

### 为什么必须有 `ilraw`

> 2026-11-19 事故：`EnemyGroupData::CanChanllage` 注入后，`IL_0007` 处短跳需要位移 +138，Mono.Cecil 按 sbyte 静默截断成 **−118**，分支目标变成 `0xFFFFFF8A`（= 崩溃现场 `[node+0x1c]` 的值）⇒ 点秘境必崩，且**与开关无关**（全 key=0 也崩）。
> `ilverify/ilstack/ilgeo/ilrid` 四关**都查不出**（它们只看栈/几何/RID，不看分支目标是否落在指令边界）。

**铁律**：任何补丁产物（v1 / merged / 探针）**必须 `ilraw` CLEAN 才允许装机**。

### 相关修法（已落在 patcher 内，勿删）

- `ilpatch/Patch.cs`：`InsertAt` 登记 `Touched`，写盘前 `FinalizeBodies()` → 对每个被改方法 `SimplifyMacros()+OptimizeMacros()`；
- `ilpatch_delta_13101/Program.cs`：读入即快照指令数，写盘前对「指令数变化」的方法做同样处理。

若新增补丁器，**必须**照此处理，否则会重现「短跳越界」事故。
