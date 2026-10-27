# 解锁修改模式

> 📍 路径：`re-implement/unlock-pattern.md`
> 🎯 触发场景：需要解锁角色、皮肤、关卡等内容

---

## 模式一：角色解锁 dic 直写法

**适用游戏**：战魂铭人（Unity IL2CPP / HybridCLR）

### 原理

游戏的角色解锁状态存储在 `Archive.heroArchiveDic` 静态字段中，是一个 IL2CPP Dictionary，每个 value 是 `HeroArchive` 对象，其中有 `_isUnlock` 布尔字段。

### 操作步骤

1. **读静态字段**：

```c
void* heroArchiveDic = nullptr;
il2cpp_field_static_get_value(klass, field_offset, &heroArchiveDic);
```

2. **遍历 Dictionary**：

```c
// IL2CPP Dictionary 内存布局
// entries 在 +0x18
// count 在 +0x20
// Entry stride = 0x18

void** entries = *(void**)((char*)heroArchiveDic + 0x18);
int count = *(int*)((char*)heroArchiveDic + 0x20);

for (int i = 0; i < count; i++) {
    void* entry = (char*)entries + i * 0x18;
    void* heroArchive = entry;  // Entry 的第一个字段是 value
    if (heroArchive) {
        // 直写 _isUnlock
        *(bool*)((char*)heroArchive + isUnlock_offset) = true;
    }
}
```

3. **字段偏移用 `il2cpp_field_get_offset` 原值**：

```c
size_t isUnlock_offset = il2cpp_field_get_offset(isUnlock_field);
// 直接用这个值，不要 +8
```

### 关键规则

- **用 `il2cpp_field_get_offset` 返回的原值**，不要手动 +8（HybridCLR bias 已包含）
- **不调 `SetHeroUnlock` / `OnPurchasesSuccess`**：`il2cpp_runtime_invoke` 调热更方法会 SIGSEGV
- **直接写内存字段**，不经过任何 setter 方法

### 坑/弯路

- **堆扫描不可靠**：`ScanInstances` 只能找到 ~7 个对象，Dictionary 里实际有 ~43 个。必须通过 dic 遍历
- **unlock@40 是错误的**：某处文档记录 `_isUnlock` 在 offset 40，但实际在 offset 32。这是因为错误地 +8 了。`il2cpp_field_get_offset` 返回 32，直接用
- **Unity GC 移动对象**：堆扫描找到的地址随时可能失效，所以不能依赖堆扫描

### 验证

选人界面所有角色头像全亮且可选中进入游戏。

## 通用思路扩展

类似 dic 直写法可应用于其他"批量解锁"场景：

| 场景 | 静态字段 | 目标字段 |
|------|----------|----------|
| 角色解锁 | `Archive.heroArchiveDic` | `HeroArchive._isUnlock` |
| 皮肤解锁 | 类似 skinArchiveDic | `_isUnlock` / `_owned` |
| 关卡解锁 | `LevelManager.levelDic` | `_unlocked` |

关键原则不变：
1. 通过静态字段找到 Dictionary
2. 按 IL2CPP Dictionary 内存布局遍历 entries
3. 用 `il2cpp_field_get_offset` 获取字段偏移（不 +8）
4. 直接写内存，不调 setter

---

## 模式二：批量授予 + 触发升级（回灌候选时，别指望原函数二次进入）

**适用游戏**：蛋壳战机 1.1.7（Unity IL2CPP / TapTap 渠道）

### 场景

「选 1 个得其余」这类补授功能：Hook 真正的授予收敛点，再对其余候选逐个回灌原授予函数。

### 三轮才收敛的坑

| 轮次 | 做法 | 结果 |
|---|---|---|
| 1.1.5 | 调 `AddSkill` 系的列表方法（`AddSkillSurvior`） | 方法体只有 `_allSkills.Add(id); OnAddSkill(id);`，**只改 UI 列表**，不实例化技能 ⇒ 用户观感「纯 UI 修改」 |
| 1.1.6 | 回灌完整授予链（`AddSkillToCharacter` → `checkIfReplaced`） | 走对了路，日志证明 `granted=N`，但**同名重复仍不升级**（假设被真机否定） |
| 1.1.7 | 授予前自己算目标 id（读升级链取第一个未拥有的下一级） | 生效 |

### 判据（可复用）

**「把同一个 id 再交给原授予函数」不等于会触发升级。** 多数授予函数把幂等去重写在函数入口，
一旦发现玩家已拥有该 id 就**提前 return**，第二次进入是空操作。要触发升级必须：

1. 先读出「玩家已拥有 id 集合」和「该 id 的升级链」（如 `Skill_Main.UpgradeSkillId` 这类 `int[]` 字段）；
2. 自己算出**下一个未拥有的目标 id**；
3. 把**目标 id** 交给原授予函数，而不是把原 id 再交一次。

### 相关细节

- 已拥有 id 集合的容量要给够（蛋壳战机从 32 提到 256 才不再漏判，`ContainsSkillId` 的截断会默默放错）。
- 「同一轮候选内去重」和「跳过已拥有 id」是两件事：前者要做，后者会把升级场景整条挡掉。
- 打点日志直接把 `requested -> granted` 打出来，这是唯一能区分「没被调用」与「被原函数跳过」的证据。
- 本模式靠运行时按名解析 `MethodInfo`（`AddSkillToCharacter` / `GetSkill_Main` / `Game.get_Table`），只把 Hook 落点写死为 file offset；见 `hook-rules.md` §24。

---

## 模式三：门控判定函数「幂等覆盖」（Lua / 游戏内置功能的解锁）★2026-10-02 消消英雄2

**适用**：游戏**本来就有**这个功能，只是被进度/等级/任务门控按下去了
（消消英雄2 1.2.27：自动战斗 + 3 倍速，提示「通关主线4-10后解锁…」）。
用户要的是「解锁」，**不是**替玩家操作（旧做法「开战强制 `InputMode.Autoplay` + 改
`TimeScaleManager` 5 倍速」已被用户否掉并删除）。

### 定位（门控只有一个函数，通常很好找）

1. **从提示文案反查**：先拿到弹窗文案的 key（如 `auto_play_open_tip`）及其调用点
   —— 静态面无 Lua 时先做运行时 dump（`re-extract/xlua-from-scratch.md` §十二），
   dump 出来的字节码里源码路径串能直接认出模块（`.../Fight/View/GameViewUpperUI.lua.txt`）。
2. **顺着调用点看判定分支**：`UserInfoManager.HasUnlockSpeed3()` → `TEST/JMP`
   为假则弹提示 `return`，为真才执行真正的切换 ⇒ **这个函数就是唯一门控**。
3. **反查它是「纯判定」**：`HasUnlockSpeed3` 体 =
   `return MainLevel.Create(GetAutoPlayOpenChapter(), GetAutoPlayOpenSection()) <= _accountMaxMainLevel`
   ⇒ 无副作用，可以安全覆盖。
4. **确认共用**：`OnAutoPlayButtonClicked`（proto#14）与 `OnSpeedUpButtonClicked`（proto#12）
   首句都调它 ⇒ 覆盖一处，两个功能同时解锁（3 倍速与自动战斗同一门控）。

### 实现（Lua 侧，幂等 + 可回退）

```lua
if type(UserInfoManager) == "table" then
  if not rawget(UserInfoManager, "__starry_unlock") then
    local _orig = UserInfoManager.HasUnlockSpeed3
    if type(_orig) == "function" then
      UserInfoManager.HasUnlockSpeed3 = function(...)
        if AN_3 then return true end      -- 开关为真：直接放行
        return _orig(...)                 -- 关掉：逐帧回退原判定
      end
      UserInfoManager.__starry_unlock = true
      __starry_unlock = "1"               -- C++ 侧据此判定「本段已生效」
    end
  end
end
```

C++ 侧完成判定别只看「RunLua 成功」：**两个目标表可能先后就绪**
（`CFightSystem` 先、`UserInfoManager` 后）⇒ 读 Lua 置下的
`__starry_fight` / `__starry_unlock` 字符串标记，两者都到位才清 `g_wantInstall`，
否则会在目标表没就绪时提前收工（消消英雄2 实际踩到，改成双标记后才稳）。

### 判据与坑

| 项 | 结论 |
|---|---|
| 边界 | **只解锁门控，不注入/不替换/不仿写游戏内置逻辑**（消消英雄2 用户明确要求）；dump 只为定位那个函数 |
| 覆盖对象 | 必须确认原函数是纯判定；带副作用（建表/发消息/初始化）的函数覆盖会出连锁问题 |
| 共用门控 | 先全库统计调用点（`grep` 反汇编文本），一个函数常被多个入口共用 ⇒ 只覆盖一处 |
| 重入 | `rawget` 守卫 + 标志位；别用 `X = X or {}`（xLua 全局表白名单式 `__index`，见 xlua §七） |
| 回退 | 开关关掉即回退原判定（原函数逐帧仍在）；**「不要一次性改存档」仅指回退方式**，不否定持久解锁本身必须落库 —— 见下方「模式四」 |
| 验证 | 打开开关：不再弹提示、按钮直接可用；关掉开关：提示恢复 |

### 与「数值/替玩家操作」的分界

- 需求说「解锁/可以使用」→ 本模式；
- 需求说「自动帮我打/加速战斗」→ 属于**替玩家操作**，先跟用户确认是否真的需要
  （多数会被否掉：与游戏自身状态机打架，且属超出功能边界的扩张）。

## 模式四：解锁必须真写存档（C++ hook 版）★2026-10-02 疾风骑士

> 来源：疾风骑士 60.0.1 会话复盘（`10_docs/_state/history/reviews/规范优化_20261002_疾风骑士60.0.1会话复盘.md`）。
> v4 只让 `DataController.LoadPetUnlock` 恒返 true 而不落库，用户装备了宠物、重启游戏后
> 关掉功能 ⇒ 存档里存着宠物 ID 但解锁表里没有 ⇒ 弹「尚未拥有该宠物。」并**卡住整个 UI**，
> 什么都点不了。这是**用户存档被写坏**级别的缺陷，不是「功能无效」。

### 问题本质

「解锁」类功能里，判定函数和存档表是**两个独立的事实来源**：

| 来源 | 谁读它 | 恒真判定的后果 |
|---|---|---|
| 判定函数（`Load*Unlock` / `CheckHave*` / `Is*Owned`） | 运行时逐帧查询 | 当下「能用」 |
| 存档表（`Save*Unlock` 写入的持久数据） | 重启后重新加载、UI 列表、装备校验 | 仍是「没拥有」 |

只要玩家在开着功能时**产生了持久副作用**（装备了宠物/选中了皮肤/上了阵），
存档就会记录一个「拥有」的状态，而解锁表没记 ⇒ 下次加载时两边不一致 ⇒
轻则功能消失，重则**一致性校验失败卡死 UI**。

### 正确做法

1. **`Load*Unlock` 恒真的同时，调一次对应的 `Save*Unlock`**，让两个来源一致。
2. **必须去重**（`MemoFirst` 式「每槽只写一次」标志），否则逐帧调用会**每帧写盘**，
   既伤性能也伤闪存。
3. 优先找**成对的 `Load*` / `Save*`** API：dump 里 `LoadPlayerUnlock` / `SavePlayerUnlock`、
   `LoadPetUnlock` / `SavePetUnlock` 这种命名成对出现，就是设计上给你这么用的。
4. 真门禁常是 `Load*`（读存档的那个），不是 `Check*`/`Is*`（派生查询）。
   疾风骑士「解锁全角色」v4 无效就是因为挂在了 `IsCharacterUnlockedForPurchase` 上，
   v5 改挂 `DataController.LoadPlayerUnlock`（argc=1）才生效。

### 判据与坑

| 项 | 结论 |
|---|---|
| 验收口径 | **不能只看「当下能用」**：必须做「开功能 → 产生持久副作用（装备/选中）→ 关功能 → 重启 → 仍在」的回归 |
| 写盘频率 | 不去重就是逐帧写盘；用「每槽只写一次」标志，别用「每帧都写」 |
| 存档一致性 | 判定恒真 + 存档不写 = 两边事实来源不一致 ⇒ UI 卡死。任何「解锁」需求都要问一句：**存档写了吗？** |
| 与模式三的关系 | 模式三（Lua 门控覆盖）适用于**纯门控、不产生持久副作用**的场景；一旦玩家能把解锁物「装备/上阵」，就必须升级为模式四 |
| 与上表「回退」行的关系 | 「不要一次性改存档」说的是**回退方式**（靠关开关回退，而不是靠删存档回退）；持久解锁本身**必须**真写存档，两者不矛盾 |
