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
