# 伤害修改模式

> 📍 路径：`re-implement/damage-pattern.md`
> 🎯 触发场景：需要实现秒杀、无敌、全屏攻击等伤害相关功能

---

## 模式一：ChangeHP 拦截

**适用游戏**：追迹迷城、物质与魔法、虚无之刃（Unity IL2CPP）

### Hook 签名

```c
// 怪物秒杀
void hook_Monster_ChangeHP(void* self, float hp, bool isShow) {
    if (开关 && 敌方判定(self) && hp < 0) {
        hp = -9999999.f;  // 放大致死量
    }
    orig_ChangeHP(self, hp, isShow);
}

// 玩家无敌
void hook_LocalPlayer_ChangeHP(void* self, float hp, bool isShow) {
    if (开关 && hp < 0) {
        hp = 0;  // 致死伤害归零
    }
    orig_ChangeHP(self, hp, isShow);
}
```

### 操作步骤

1. Il2CppDumper 找 `Monster.ChangeHP` 和 `LocalPlayer.ChangeHP` 的 RVA
2. Dobby hook 两个函数
3. 在替换函数内做敌我判定 + 伤害修改
4. **必须调 orig**

### 全屏秒杀变体

Hook `BattleManager.Update` → 每帧 `GetCurWaveAliveEnemyList()` → 对每个怪调 `ChangeHP(-999999)`

## 模式二：DeductHP 拦截

**适用游戏**：霓虹深渊（Unity IL2CPP）

### Hook 签名

```c
// 怪物秒杀
bool hook_HitPoints_DeductHP(void* self, float amount) {
    if (开关) { 血量归零; return true; }
    return orig_DeductHP(self, amount);
}

// 玩家无敌
bool hook_Hearts_DeductHP(void* self, float amount) {
    if (开关) return false;  // 拒绝扣血
    return orig_DeductHP(self, amount);
}
```

### 必做：同时 hook 通关校验

Hook `DbgPoint.BuildPassCheckInfo` → 旁路校验。详见 `anti-cheat-pattern.md`。

## 模式三：getAtk 放大/归零（Cocos JS）

**适用游戏**：造梦西游、造梦无双、镇邪人（Cocos Creator JS）

### 操作步骤

1. Hook `se::ScriptEngine::evalString`
2. 在明文 script 上 splice 功能 JS
3. 伤害计算走 `getAtk` 函数，splice 注入阵营判定：

```javascript
// 秒杀（敌方）
var _origGetAtk = getAtk;
getAtk = function(e, i) {
    if (开关 && 敌方判定(e)) return 9999999999999;
    return _origGetAtk(e, i);
};

// 无敌（我方）
getAtk = function(e, t) {
    if (开关 && 我方判定(e)) return 0;
    return _origGetAtk(e, t);
};
```

### 坑/弯路

- **误锚 getAtk**：锚到子弹/自定义 ATK 函数 → 双方伤害都变 1。正确锚点：`if (0 == e || this.isDead()) return 0;` 之后插入
- **参数名区分**：`(e,i)` 是输出路径，`(e,t)` 是受击路径，不同游戏可能不同

## 模式四：全屏攻击扩散

**适用游戏**：孤勇无双、虚无之刃

### 操作步骤

1. Hook `SetScreenTargets` → 收集屏幕敌人到全局数组
2. Hook `onHit4Effect` → 玩家命中时遍历敌人数组，对每个调原函数
3. 递归保护：前置标志防递归
4. 最多扩散 40 个（斌哥上限）

```c
static bool spreading = false;
void hook_onHit4Effect(...) {
    orig_onHit4Effect(...);  // 原始命中
    if (spreading) return;
    spreading = true;
    for (int i = 0; i < enemy_count && i < 40; i++) {
        orig_onHit4Effect(enemy[i], ...);
    }
    spreading = false;
}
```

### 坑/弯路

- **spread 参数错位**：误调 `g_origOnHit4Effect`（x0=this 约定），参数顺序错位 → 崩溃
- **死亡判定在 processHit**：不在 `onHit4Effect` 路径，onHit4Effect 只管特效

## 敌我阵营判定通用法

| 方法 | 字段/方式 | 值 |
|------|----------|-----|
| IL2CPP 通用 | `m_Camp` / `myTeam` | Player=1, Npc=2, Pet=3 |
| IL2CPP | `isAlly` / `isHero` | bool |
| Cocos JS | `unitType` | `qA.Hero` / `qA.Monster` |
| 斌哥壳 | `entity[0xCC]` | 2=敌人 |

**坑**：`IsFriendly` 漏检 `isHero` → 友军判定全否。必须同时检查 `isHero` 字段。

## 秒杀防溢出

```c
#define CAP (INT64_MAX / 100)
if (damage > CAP) damage = CAP;  // 防止溢出导致负数变正数
```
