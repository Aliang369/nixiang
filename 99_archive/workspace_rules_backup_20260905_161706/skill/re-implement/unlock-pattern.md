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
