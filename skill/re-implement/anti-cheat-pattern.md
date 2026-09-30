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

## 模式五：原生退出拦截（seccomp 只拦 exit_group）

**适用游戏**：勇士之心 1.0.4（加固壳 `libsecret.so` 起跑后主动退进程）；同族 tersafe/tprt 亦可参考

### 症状判定（先排除误判）

启动 5~15 s 后**干净退出**，logcat 只有：

```
ActivityManager: Process com.x.y (pid N) has died: fg TOP
Zygote: Process N exited cleanly (0)
```

无 Java 异常、无 native 崩溃栈、无 JS 报错 ⇒ 是应用**主动 exit(0)**（不是被系统杀、不是 ANR）。
先排除「权限弹窗未响应/Activity 未 attach 被回收」这类同样表现为 clean exit 的情况。

### 定位顺序

1. 装 native 探针钩：`exit` / `_exit` / `_Exit` / `abort` / `kill` / `pthread_exit` / `syscall`
   （libc）+ JNI `System.exit` / `Runtime.exit` / `exitInternal` / `Process.killProcess`。
   **Android 14 上后四个 JNI 符号不在 `libandroid_runtime.so` 导出表**，`符号未找到` 属正常，别当成探针失败。
2. 若全部无命中 ⇒ 保护组件用**内联 `svc #0` 直发 `exit(93)/exit_group(94)`** 绕过 libc 导出
   （bionic 的 `_exit` 本身也是内联 `exit_group`）。此时只能拦 syscall。

### seccomp 拦法：只拦 94，且必须 TRAP（2026-09-30 实测四轮）

| 拦法 | 结果 |
|------|------|
| 93 + 94，`SECCOMP_RET_TRAP` | 每进程约 270 ms `exited due to signal 31 (Bad system call)`，SIGSYS 处理器来不及执行 ⇒ 不可用 |
| 93 + 94，`RET_ERRNO\|EPERM` | 变成 `signal 4 (Illegal instruction)`（`pthread_exit(0x0)` 后约 12 ms）；栈 `libstarry_magic.so+0x1b2d0 ← +0x1a524(trampoline) ← libc.so+0xfd530 ← libc.so+0x96b28` ⇒ **拦 93 会让线程从 svc 之后继续执行落进 `udf`，整进程陪葬** |
| 只拦 94 + TRAP + 只杀该线程 | 进程存活 60 s+，正常进游戏（采用） |
| 只拦 94 + `RET_ERRNO` | 留 destroyed-mutex 二次崩溃 `F libc: FORTIFY: pthread_mutex_lock called on a destroyed mutex` ⇒ 必须 TRAP |

### 实现骨架

```c
// 1) 装过滤器：BPF 只匹配 __NR_exit_group，其余 ALLOW，命中 TRAP
prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0);
prctl(PR_SET_SECCOMP, SECCOMP_MODE_FILTER, &prog);

// 2) SIGSYS 处理器：打印 pc/lr/regs[8]/regs[0] + dladdr 库名+偏移，然后只结束当前线程
static void OnSys(int sig, siginfo_t *si, void *uc) {
    /* 打日志（__android_log_print） */
    syscall(__NR_exit, 0);   // 绝不 return：返回会从 svc 之后继续执行 → udf → 整进程死
}
signal(SIGSYS, ...);  // SA_SIGINFO
```

- 参考实现：`mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzExitTrace.cpp`
  （`InstallSyscallTrap()` / SIGSYS 处理器 / `InstallFaultHandler()` 覆盖 SIGILL·SEGV·BUS·ABRT·FPE /
  `prctl(PR_SET_SECCOMP…)`）；装配点 `Games/YsxzMod.cpp::Ysxz_OnLoad` 首句 `YsxzExitTrace_Start(vm)`。
- **每类退出调用独立计数**（共用一个计数器时首个命中被门限吃掉、栈不打印，反而掩盖真凶）。
- 探针必须有装配自检行，见 `hook-rules.md` §25（`hook ok libc.exit` / `符号未找到 …`）。

### 验证与后续

- 命中现场形如：`[YsxzExit] _exit 被调用 code=0 (#68)` → `该线程 tid=… 已被单独结束（进程存活）`
  → `syscall(nr=93 code=0) 直调退出 (#69)`。
- 只杀保护线程后，保护逻辑可能重试；若反复被杀，先改成「命中后不改行为、只留痕」定位真凶，
  再决定中和策略，不要一上来就拦。

---

## 模式六：自己写坏 `Obscured*` 字段 → 游戏反作弊自爆（2026-10-01 失落城堡）

### 症状

- 功能「logcat 说写入成功」但游戏内毫无变化；
- 随后（下次读档 / 结算时）弹出**游戏自己的**警告并退出。失落城堡实测原文：
  「档案数据存在异常，即将退出游戏!如有错误检测，请联系客服!」
  （注意游戏自己把 `exception` 拼成了 `execption`，按 `execption` 搜才搜得到）
- **logcat 里没有任何作弊/校验字样** —— 该事件不落日志，容易误判成「服务器校验」或「未知原因」。

### 判定：目标包带 ACTk

包内有 **ACTk（CodeStage Anti-Cheat Toolkit）** 即高度可疑：

- `CodeStage.AntiCheat.ObscuredTypes.*`（`ObscuredInt` / `ObscuredBool` / `ObscuredFloat`…）
- `CodeStage.AntiCheat.Detectors.{ObscuredCheatingDetector, TimeCheatingDetector, SpeedHackDetector, InjectionDetector, WallHackDetector}`
- `CodeStage.AntiCheat.Utils.xxHash`、`CodeStage.AntiCheat.Common.*`
- 游戏侧中央惩罚器通常是游戏自己写的类（失落城堡是 `Teran.DDSystem`：`OnCheaterDetectedForObscured()` / `ForSpeed()` / `ForInjection(int)`、`RecoverDetectCheating(int)`、`TurnToMainMenuBecauseCheating()`、`QuitGame()`、`ShowQuitGameTips()`）。

### 根因

`Obscured*` 是**值类型 struct**，内部为 `hiddenValue` + `currentCryptoKey` + `fakeValue` + `fakeValueActive` + `inited` 的自校验结构。用 `runtime_invoke` 传「装箱对象」调用它的 setter 时，il2cpp 会把**对象头 16 B（`klass*` + `monitor*`）当成结构体内容**写进后备字段 —— 不抛异常、不崩溃，字段值变垃圾，随后随存档一起被校验抓到。完整 ABI 说明见 `il2cpp-abi-pitfalls.md` 坑 6。

### 三条识别线索

1. 包内含 `ACTk.Runtime.dll` / `CodeStage.AntiCheat.*` 类；
2. 你要写的字段类型是 `ObscuredInt` / `ObscuredBool` 一类（后备字段常命名 `<属性名>k__BackingField`）；
3. 弹窗文案能在 metadata 字符串区搜到（失落城堡在 `Teran.UITextCtrl.gamedata_execption_str`）。

### 正确做法

1. **只读不写。** 读方向有可靠办法：`ObscuredInt.GetDecrypted() -> int`（argc=0 唯一名）。
2. 要改数值就**换挂点**：改上层业务方法（结算入口 / 消费路径 / 普通 getter 返回值），不写它的序列化字段。
3. 已经在写的：**当轮删掉该功能**，不要试图「写对」。失落城堡删掉执照功能后弹窗立即消失，即归因的实测确认。

### 验证

- 删改后重启游戏，连续读档 / 结算 2~3 次，确认不再弹。
- **不要用「日志说写入成功」当证据** —— 本模式的特征恰恰是日志正常而内存已坏；必须用游戏内可见状态（UI 数字 / 图标）验收。

---

## 反作弊应对决策表

| 症状 | 原因 | 应对 |
|------|------|------|
| 结算失败 (78) | 服务端重放校验 | 降低功能量级 / 关功能打结算关 |
| "数据异常"弹窗 | BuildPassCheckInfo | Hook 旁路，与战斗 hook 同批 |
| **"档案数据存在异常"弹窗（游戏自己的文案，logcat 无痕迹）** | **自己写坏了 ACTk `Obscured*` 字段** | **删掉写操作，改挂上游业务方法（模式六）** |
| **功能"日志成功但界面无变化"** | **可能同上 —— 值类型参数被当装箱对象传** | **先查 `il2cpp-abi-pitfalls.md` 坑 6，改用游戏内可见状态验收** |
| 作弊上报 | JS addCheat/onStageCheat | evalString splice 屏蔽 |
| 其他腾讯游戏闪退 | ACE 残留 | 跑 cleanup 脚本 |
| 登录弹"非法登录" | APK 完整性黑名单 | 参考外科组装法 |
| 启动 5~15 s 干净退出（`exited cleanly (0)`） | 保护线程内联 `svc` 直发 `exit_group(94)` | seccomp 只拦 94 + TRAP + 只杀该线程（模式五） |
| 拦 exit_group 后变 `signal 4 (Illegal instruction)` | 把 `exit(93)` 一起拦了，线程从 svc 后继续执行落进 `udf` | **绝不要拦 93**（模式五） |
