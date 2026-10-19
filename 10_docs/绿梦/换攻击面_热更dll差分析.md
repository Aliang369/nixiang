# 绿梦：时空之声 — 换攻击面：热更 dll 差分析（Stage 8，纯离线）

- 问题：10-19 热更后的 30 s 干净退出，**是不是 Assembly-ACT.dll 触发的？** 若是，patch 那个调用点即可绕开 FairGuard（成本比逆壳低一个量级）。
- 纪律：纯离线，未装机、未碰真机、未逆壳。

## 1. 样本

| 版本 | 路径 | 大小 |
|---|---|---|
| 10-16（热更前） | `06_dumped/绿梦_20261016/hotfix_dll/Assembly-ACT.dll` | 14,529,536 B |
| 10-19（现役） | `06_dumped/绿梦_20261019/hotfix_dll/Assembly-ACT.dll` | 14,534,656 B |
| 差值 | — | **+5,120 B** |

（10-19 的 `Assembly-ACT.new.dll` 与 hotfix_dll 版同尺寸 14,534,656；反编译基线 `06_dumped/绿梦_20261016/decompiled/` 共 9,355 个 .cs，对应 10-16。）

## 2. 结论：**B —— dll 里没有触发器，换攻击面-via-dll 死**

### 证据 1：类型差异只有玩法逻辑
`ilspycmd -l c` 对比：11,122 → 11,125 个类型，**新增 3 个**、无安全类型：

| 新增类型 | 内容 |
|---|---|
| `ACT.Config.GuideFinshCondition` | 抽象基类（`abstract bool CheckCondition()`） |
| `ACT.Config.Condition.CheckTargetBottleLevelGreaterThanOne` | 血瓶引导条件：`Model<BloodBottleModel>.GetBloodBottleById(BottleId).curLevel > 1` |
| `MissionLocalDatabase+<>c__DisplayClassNN_0` 等 | 新增 lambda 的编译器产物（MissionLocalDatabase / GuideModel / RogueWorld / CardDrawView 下标位移） |

—— 全是任务/引导/血瓶的玩法改动，**没有任何安全/校验类型**。

### 证据 2：字符串集合**零新增**
两 dll 按 UTF-16LE 解出字符串集合（3,870 / 3,869 条 ASCII 串）取差集：

- **新增 = 0 条**；消失 = 1 条（编译器产物类名位移）。
- 关键字过滤 `fair|guard|sign|verif|cert|package|apk|integrity|cheat|hack|abnormal|root|emulat|debug|license|sdk`：
  **新增命中 0 条**。
- ⇒ +5,120 B 是**纯代码/元数据**增量，没有引入新 API 名、新路径、新提示文案。

### 证据 3：游戏程序集根本够不到签名 API
- `grep -rn "DllImport"` → **0 处**（Assembly-ACT 是纯托管，无 P/Invoke 直调 native）。
- `grep -rn "getPackageInfo|signatures|sourceDir|SourceDir"` → **0 处**
  （唯一命中的 `GetPackageInfos()` 是 YooAsset 资源包字段，与 APK 签名无关）。
- 与"校验"沾边的只有 YooAsset 资源完整性（`CheckZipStateFlow` / `CheckResourceIntegrity`，发
  `file_verification_start/end` 只是**埋点事件**，比的是下载 zip 的大小/哈希，不是 APK 证书）。

### 反证（关键）
我们自己的 **IL-patch 过的 dll**（内容被改、注入过 StarryLvmeng）在 10-16 期间**跑了 32 分钟**未被 30 s 杀掉。
⇒ FairGuard **并不按 dll 内容做校验**；"dll 变了 → 触发校验"这条因果链不成立。

## 3. 对下一步的意义

- 选项 A（patch dll 里的校验调用）**不存在**：dll 里没有这样的调用。
- 30 s 闸门由 **FairGuard native（及其云端配置）**驱动；10-19 热更只是让某个条件从满足变成不满足，
  该条件不在托管层（很可能与热更资源/服务端下发配置有关，或 FairGuard 自身计时/配置更新）。
- ⇒ 换攻击面-via-dll **终止**；要突破只能回到 **逆壳 PC9** 或 **root 兜底**。

## 4. 未闭环

1. **真正的触发器仍未定位**：现有证据只能排除"dll 代码触发"，不能指出是什么让 10-16 满足、10-19 不满足。
   可继续的低成本方向：对比 10-16 / 10-19 的热更资源与配置（`assets/lvmeng/__data`、YooAsset bundle、
   服务端 remote config），看是否新增了让游戏走"反作弊自检"分支的开关。
2. 未做 IL 级逐方法 diff（全量反编译 14 MB dll 单文件模式太慢，已中止）；本轮"字符串集合零新增 + 类型差只有玩法"
   已足以支撑 B。若日后需要精确定位改动方法，可只对 4 个受影响类型单独反编译对比。
3. Java 层（bamboosdk）是否在 10-19 新增 native 调用未查（它在 dll 内是托管实现；全程序集仅 3 处 AndroidJavaClass，
   均与版本号/分辨率/重启相关）。
