# 为了吾王 MOD 五个修改点 —— 名字定位结果（运行时内存法）

设备/环境：雷电模拟器（root，arm64 翻译运行）；小米 M2102K1AC 为 arm64 但**无 root**，无法读进程内存。
两份包同一份 `libil2cpp.so`（hykb 官方 3.0.4），故名字结果互通。

## 方法（可复核）

1. `/proc/7364/maps` 取 libil2cpp 基址 `0x400023000000`；
2. dump 内存中 `.data.rel.ro`(两段) + `.bss` 共约 17 MB（`07_tmp/relro1.bin`、`relro2.bin`、`bss.bin`）；
3. metadata 仍在 `0x7b85c58ee000`（24.2 MB `/dev/zero (deleted)`，即 `07_tmp/mz.bin`）；
4. 用 61 个可读 module 名做锚点，定位 `codeGenModules` 指针数组 @`0x4000270f7f30`（115 项）；
5. 逐 module 读 `methodPointers`（`.rela.dyn` 在内存中已填好），线性扫出 5 个修改点所在的 module 与索引。

## 结果

**5 个修改点全部落在 `Assembly-CSharp.dll`（image[0]，38115 个方法）里**，分属 3 个类：

| 点 | 文件偏移 | 所属类（确定） | 反汇编语义（确定） |
|---|---|---|---|
| A | `0x1DBCC88` | **LorePersistence** | 某取值接口改为 `return 999999` |
| D | `0x1DBD5B4` | **LorePersistence** | `bool (f() > 0)` 恒真 |
| B | `0x1F62088` | **CharacterStats** | int 字段 `+0x15C` 夹取上限 `99999 → 999999` |
| C | `0x1F6903C` | **CharacterStats** | `bool (this.+0x15C >= 参数)` 恒真 |
| E | `0x2059C14` | **SkillRoll** | 浮点比较 `(s9 <= s8)` 恒真 |

各类里相邻的方法名（用于确认归属）：
- `LorePersistence`：`get_Instance` / `MigrateOldLore` / `LoadLoreData` / `SaveLoreData` / `UnpurchaseAll` / `UnrevealAll` / `Purchase` / `IsPurchased` / `IsCloudAvailable`
- `CharacterStats`：`get_RawTalent|RawVitality|RawQuickness|RawLuck` / `GetXpPercent` / `GetAllActiveImmunities` / `UpdateXP` / `ResetXP` / `SetNewPartyImmunity`
- `SkillRoll`：`get_AvgSuccess` / `.ctor` / `Roll` / `.cctor`

## 功能解读（推断，但语义链完整）

1. **Lore（学识/图鉴商店）**：A 让某个取值恒为 999999（余额/点数），D 让"是否已拥有"判定恒真
   → **学识商店可无限购买 / 视为已解锁**。
2. **CharacterStats 的 int 字段 `+0x15C`**：B 把上限从 99999 抬到 999999，C 让"够不够"判定恒真
   → **该属性/资源不再受限**（CharacterStats 里带 99999 上限的计数）。
3. **SkillRoll::Roll**：E 把一次浮点比较恒真
   → **技能检定/掷骰必定成功**。

## 星光菜单落地（2026-10-30，已编译通过）

tag `wlww`（为了吾王 → wei le wu wang），game_id `wlww`。
工人：`mod菜单/starry-mod-menu/app/src/main/jni/Games/WlwwMod.{h,cpp}` + `WlwwEntry.cpp` + `Android_wlww.mk`（modern 注册表，共享文件零改动）。

| featNum | 菜单项 | 实现（全部按名字定位，不写死 RVA） |
|---|---|---|
| 1 | 无限学识点 | hook `LorePersistence::GetLore` → 999999；`IsPurchased` → true |
| 2 | 无限金币 | hook `CharacterStats::ChangeGold`，调完 orig 后把 `m_Gold` 字段写 999999（字段偏移运行时由 `il2cpp_field_get_offset` 解析） |
| 3 | 无限专注点 | hook `CharacterStats::UpdateFocusPoints`：delta<0 直接返回 ⇒ 永不消耗；上限保持游戏原值（满 5 格） |
| 5 | 技能判定全成功 | hook `SkillRoll::Roll` → 恒返回 1 |

- 0/4 为 Category 占位（不占回调），featNum 与 Entry 的 featureList 下标严格对齐。
- argc 未知的方法用「同址多 arity 尝试」，先装上的生效，其余只记 warning。
- 构建：`ndk-build.cmd` + `NDK_OUT/NDK_LIBS_OUT` 绝对路径；**obj/libs 目录要先手工建好**（中文父目录 `mod菜单` 下 ndk-build 的 install 步骤会失败，编好的 so 从 `obj_wlww/local/arm64-v8a/` 手动拷出）。
- 产物自检通过：187,256 B；含 `无限学识点/无限金币/无限专注点/技能判定全成功`、`wlww`、`UpdateFocusPoints`、`m_FocusPoints`。

## 遗留不确定（诚实标注）


- module 的 `methodPointers` 数组里相邻项地址间隔仅 8 字节（il2cpp 的 thunk 布局），
  所以"索引 → methodDef"不是严格 1:1，**单个方法名可能有 ±几 的偏移**；
  A/D/E 的类归属与语义已验证，B/C 的类归属（CharacterStats）有相邻方法佐证，但具体方法名待定。
- 要拿到 100% 精确的"方法名 → 地址"，需按 prologue 反扫函数起点（加密页会挡住），
  或改用 C 端 so 内直接调 il2cpp 的 `class_get_method_from_name` 路径。
