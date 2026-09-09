# 工具模式

> 📍 路径：`re-implement/utility-pattern.md`
> 🎯 触发场景：零 CD、加速、挂机菜单修复、悬浮窗权限等辅助功能

---

## 模式一：零 CD / 无限技能

### IL2CPP 方式

**适用游戏**：追迹迷城、迷雾大陆、堡垒之战

Hook 技能检查函数，强制返回 true：

```c
bool hook_CheckCurrentIsCanUse() {
    if (开关) return true;
    return orig_CheckCurrentIsCanUse();
}

bool hook_CheckCurSp() {
    if (开关) return true;
    return orig_CheckCurSp();
}
```

可能需要 hook 的函数系列：
- `CheckCurrentIsCanUse()`
- `CheckCurSp()`
- `CheckSkillUse_*()` 系列

### Cocos JS 方式

**适用游戏**：造梦西游、迷雾大陆

evalString splice 两种方式：

```javascript
// 方式1：直接清 CD
reduceAllSkillColdDown(1e9);

// 方式2：hook 技能释放检查
var _origIsCanRelease = isCanReleaseSkill;
isCanReleaseSkill = function(...) {
    var ret = _origIsCanRelease(...);
    if (!ret && 开关) return null;  // 放行
    return ret;
};
```

## 模式二：游戏加速

### Unity 方式

**适用游戏**：镇邪人、公主连结

Hook `UnityEngine.Time.get_deltaTime`，返回值乘以倍数：

```c
float hook_Time_get_deltaTime() {
    float dt = orig_Time_get_deltaTime();
    if (加速开关) return dt * 倍数;
    return dt;
}
```

### Cocos JS 方式

```javascript
cc.director.getScheduler().setTimeScale(N);  // N=2 即两倍速
```

### 验证

游戏整体加速（动画、移动、战斗），不仅仅是 UI 加速。

## 模式三：挂机菜单消失修复

**适用游戏**：公主连结（ColorOS）

### 问题

挂机时 Activity Stop → Start 闪断，悬浮球/菜单消失。

### 解决方案

1. **后台 hide 防抖 600ms**：收到 hide 后不立即隐藏，等 600ms 确认仍在后台才隐藏
2. **已 resumed 跳过 hide**：如果 Activity 已经 resumed，忽略 hide 请求
3. **ACTIVITY 模式 resume 后主动恢复**：

```c
void onResume() {
    orig_onResume();
    if (已resumed) {
        setOverlayVisible(true);
        clear_suppressShowUntilElapsed();
    }
}
```

### 验证

挂机 10 分钟后切回游戏，菜单仍在。

## 模式四：悬浮窗权限

### 授权

```bash
adb shell su -c "appops set <pkg> SYSTEM_ALERT_WINDOW allow"
```

### ColorOS 兼容问题

- ColorOS alpha ≤ 0.79 时悬浮窗不显示
- 冷启动需 `notifyHostResumed` / lifecycle 修补
- **ACTIVITY 模式**：嵌入游戏 Activity 内，不依赖悬浮窗权限

### ACTIVITY 模式 vs 悬浮窗模式

| 模式 | 优点 | 缺点 |
|------|------|------|
| 悬浮窗 | 全局覆盖 | 需权限，ColorOS 兼容差 |
| ACTIVITY | 嵌入游戏 Activity，不依赖权限 | 需处理生命周期 |

优先使用 ACTIVITY 模式，避免悬浮窗权限问题。
