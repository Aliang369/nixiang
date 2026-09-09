# 反作弊应对

> 📍 路径：`re-implement/anti-cheat-pattern.md`
> 🎯 触发场景：功能实现后遇到服务端校验失败、客户端作弊上报、通关校验拦截等问题

---

## 模式一：服务端战斗重放校验

**适用游戏**：堡垒之战、迷雾大陆

### 问题

开外挂后结算失败（`STAGE_VERIFY_FAILED=78`），服务端用 `bOnlyLogic` 重放战斗，强制 `attackHpScale=1`、`bRoleInvincible=false`，客户端上报的 `rInfo` 与重放不一致 → 结算失败。

### 关键认知

**无法在客户端绕过服务端重放校验。**

- 吞 `onVerifyFail` 只挡踢局弹窗，挡不住结算失败
- 假 `game_stage_end` 骗发奖 → 关卡回滚，**禁止再做**

### 正确做法

1. **功能量级保持温和**：伤害倍数控制在 ×100/×5 以内，不要 ×999999
2. **关功能后再打需要结算的关卡**：有结算校验的关卡，关掉外挂功能后正常打
3. **区分哪些关卡有校验**：部分关卡不校验（如扫荡/自动），可以开功能

### 验证

结算成功发奖 = 通过。结算失败 = 需要降低功能量级或关闭功能。

## 模式二：JS 反作弊屏蔽

**适用游戏**：迷雾大陆、造梦无双（Cocos Creator JS）

### 操作步骤

evalString splice 屏蔽客户端作弊上报：

```javascript
// 1. addCheat → 不上报
var _origAddCheat = addCheat;
addCheat = function() { return; };

// 2. onStageCheat → 置空
var _origOnStageCheat = onStageCheat;
onStageCheat = function() { return {}; };

// 3. onDevilClear → 置空
onDevilClear = function() { return; };

// 4. 聊天 md5 → 替换为禁聊提示
// （防止作弊检测信息通过聊天上报）
```

### 验证

`adb logcat` 确认无作弊上报日志。服务端不收到作弊记录。

## 模式三：BuildPassCheckInfo 旁路

**适用游戏**：霓虹深渊（Unity IL2CPP）

### 操作步骤

1. Hook `DbgPoint.BuildPassCheckInfo`
2. 改写/旁路校验信息
3. **必须与战斗 hook 同批移植**：不能先上战斗 hook 再补校验旁路，否则中间打出来的局会报"数据异常"

```c
void* hook_BuildPassCheckInfo(void* self) {
    if (开关) {
        // 旁路：返回伪造或跳过校验
        return 伪造的校验信息;
    }
    return orig_BuildPassCheckInfo(self);
}
```

### 验证

通关不报"数据异常"，结算正常发奖。

## 模式四：设备清理防残留

**适用游戏**：雷霆战机（腾讯 ACE）

### 问题

逆向过程中留下的 frida-server、ptrace 痕迹、KSU umount 等残留会影响其他腾讯游戏的 ACE 判断。

### 操作步骤

每次会话结束跑清理脚本：

```bash
./04_scripts/_common/cleanup_device_re_residue.sh
```

清理内容：
- `/data/local/tmp/frida-server`
- `/tmp/*` 临时文件
- KSU umount 残留
- ptrace 痕迹

### 验证

```bash
adb shell ps | grep frida  # 无结果
```

### 重要提醒

**ACE 残留会污染其他腾讯游戏判断。** 即使当前游戏不测 ACE，残留也会影响其他腾讯游戏（如王者、和平精英等）。每次逆向结束必须清理。

---

## 反作弊应对决策表

| 症状 | 原因 | 应对 |
|------|------|------|
| 结算失败 (78) | 服务端重放校验 | 降低功能量级 / 关功能打结算关 |
| "数据异常"弹窗 | BuildPassCheckInfo | Hook 旁路，与战斗 hook 同批 |
| 作弊上报 | JS addCheat/onStageCheat | evalString splice 屏蔽 |
| 其他腾讯游戏闪退 | ACE 残留 | 跑 cleanup 脚本 |
| 登录弹"非法登录" | APK 完整性黑名单 | 参考外科组装法 |
