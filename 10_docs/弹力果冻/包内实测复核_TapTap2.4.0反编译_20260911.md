# 弹力果冻 TapTap 2.4.0 — 包内实测复核（反编译实证）

> 2026-09-11 · **输入 = MuMu 上实际安装的那份 TapTap 包**，不是历史文档转述
> 本文修正前文《TapTap换版_功能全清单与闪退复查_20260911.md》中**未读包时**的推断，并补齐上报/检测面的实锤
> 证据分级：**PROVEN（包内代码可复核）** / **待运行时定论** / **NOT PROVEN** / **否证**

---

## 0. 本次输入、指纹与可复核性

| 输入 | 路径 | 大小 | 指纹 |
|------|------|------|------|
| base.apk | MuMu `/data/app/~~F6LLLOf8AGLAk64wU0hXEg==/com.ginicat.sweet-ayADSZ_X9kajEoqQpkPeWw==/base.apk` | 2,128,558,103 B | md5 `634b37691f40f94d05b77e0702604fec`（**设备端与本地一致**） |
| `hotfix.bytes` | `assets/AOTDLL/hotfix.bytes` | 9,534,976 B | md5 `1cdad24cc429eca965a4b8dc0311975e`；**明文 PE32 .NET 程序集**（MZ 头，非加密） |
| `global-metadata.dat` | `assets/bin/Data/Managed/Metadata/` | 9,983,256 B | md5 `c95d2ef936d70498b4442a2e8f058d27` |
| `libil2cpp.so` | `lib/arm64-v8a/libil2cpp.so` | 58,291,656 B | md5 `c067234d6526cd247691598e44aa5825` |
| `classes.dex` | 根目录（**仅一个 dex**） | 4,079,652 B | 单 dex → 外科注入可加 `classes2.dex` |
| 资源结构 | `assets/Bundle/` **2,887 个 bundle**（UnityFS / 2020.3.22f1c1）+ `assets/aa` + `assets/Catalog`（Addressables） | — | 场景在 bundle 内 |

- 反编译：`ilspycmd 11.0.0`（dotnet 10.0.302）→ **625,334 行 C# / 7,830 个类型**
- 落地：`02_apks/弹力果冻/弹力果冻_2.4.0_taptap_official.apk`、`06_dumped/弹力果冻/taptap_2.4.0/`（`hotfix.dll`、`hotfix_decompiled/hotfix.decompiled.cs`、`libil2cpp.so`、`global-metadata.dat`、`classes.dex`、`mini_dec/`、`mini_smali/`）
- ⚠️ **管线**：弹力果冻**未注册** `04_scripts/弹力果冻/pipeline.json`（现有 7 个游戏已注册）。本次提取属**分析输入准备**，用 adb/unzip/apktool 直接完成；**出包/签名/装机前必须先注册管线**，否则违反机械操作路由。

---

## 1. 对前文结论的修正（3 处）

| 前文（未读包） | 包内实测 | 判定 |
|----------------|----------|------|
| R-9：自造 48 字节 `DamageHandleBlob` 可能**小于**真实 `DamageHandle` → 读越栈帧 | 真实结构 = 8+8+4+4+8 + 6×bool = **40 字节**；blob = 48 字节 ≥ 40；且**前 32 字节偏移逐字段吻合** | **否证**（前文过度担忧）；但 6 个 flag 全 false 是真实存在的语义问题 |
| R-6：无敌写 `beAttackerID=0` 「语义非法」 | `HpHandle` 首行 `FindEntityByID(0)` → null → 直接 `return`；且 `Clamp(finalDamage,1,…)` 下限为 1 | **部分修正**：写 0 在「实体 0 不存在」时确实等于免伤，**不是必崩**；但正解是 `BuffKind.Invincible`（见 §2.2） |
| §4：服务端校验「未证实」 | `C2S_NormalBattleEnd` 上报 `KillMonster[]/TimeCost/Win/ToTleDamager`；开战需服务端批准；奖励由服务端下发 | **升级为「已证实存在」**（见 §3.4） |

---

## 2. 战斗核心实测（决定所有伤害类功能怎么写）

### 2.1 `DamageHandle` 真实布局（PROVEN，130046）

```csharp
public struct DamageHandle {
    public long finalDamage;               // 偏移 0
    public long attackerID;                // 偏移 8
    public float raceValue;                // 偏移 16
    public LinkSkillType attachSkillType;  // 偏移 20（enum=int）
    public long beAttackerID;              // 偏移 24
    public bool isCritical, isSkill, isBuff, isAntiDamage,
                needForceNetProxyDamage, isFromNet;   // 偏移 32 起，6 个 bool
}
```

旧代码（`TlgdMod.cpp:128-135`）的 blob 与之一致：`kOffFinalDmg=0 / kOffAttackerId=8 / kOffBeAttacker=24` **全部正确**。
→ 唯一遗留问题：**6 个 flag 全为 false**。其中 `isBuff=false` 会让 `HpHandle` 里 `!handle.isBuff` 成立 → **每次铺场 invoke 都会触发攻击方的 `oncreateDam` 回调链**（判定：副作用真实存在 / 具体后果 **NOT PROVEN**）。

### 2.2 `HpHandle` 真身（PROVEN，130232）——所有伤害功能的枢纽

```csharp
public void HpHandle(DamageHandle handle) {
    var victim = FindEntityByID(handle.beAttackerID) as BattleUnit;
    var atk    = FindEntityByID(handle.attackerID)    as BattleUnit;
    if (victim == null || !victim.IsAvailable ||
        victim.buffDriver.HasBuffKind(BuffKind.Invincible)) return;         // ① 原生无敌
    handle.finalDamage = (long)Mathf.Clamp((float)handle.finalDamage, 1f, 9.223372E+18f);  // ② 下限 1
    if (atk != null && atk.IsAvailable) {
        if (atk.proxy != null) { var p = atk.proxy as BattleUnitProxy;
            if (!p.isLocalOrProxyedProxy && p.unit.team == BattleTeamFlag.PlayerTeam) return; }  // ③ 非本地玩家代理→丢弃
        atk.oncreateDam_ForMvp?.Invoke(handle);
        if (!atk.unitAttr.isDead && !handle.isBuff) {
            atk.oncreateDam?.Invoke(handle);                                 // ④ 使用 handle.isBuff
            if (atk.buffDriver.HasBuffKind(BuffKind.Leech)) LeechHandle(handle);   // ⑤ 吸血
        }
    }
    if (victim.unitAttr.isDead) return;
    if (finalDamage >= victim.unitAttr.Hp + Shield &&
        victim.buffDriver.HasBuffKind(BuffKind.Deadproetected)) { ...; return; }   // ⑥ 免死 buff
    victim.beforeCalDmgHpHandle?.Invoke(ref handle);                          // ⑦ ref 修改点 ← 更优挂点候选
    if (finalDamage > Shield) { Shield -= shield; Hp -= finalDamage - shield; } else Shield -= finalDamage;
    if (victim.proxy != null) { var p = victim.proxy as BattleUnitProxy;
        if (p.isLocalOrProxyedProxy) { ... DamageNumMsg ... p.player.broadcaster.Broadcast(damageNumMsg); } }  // ⑧ 联机广播
}
```

**可直接用于功能的结论**：

| 现象 | 结论 |
|------|------|
| ① 无敌 | **原生机制 = `BuffKind.Invincible`**（HasBuffKind 成立即整段 return）→ 比写字段/写 `vic=0` 干净 |
| ② 下限 1 | 写 `finalDamage=0` 无效（被 Clamp 到 1）→ 旧文档「仍掉 1 血」记录**与代码一致** |
| ④ `isBuff` | 被真实使用；自造 handle 传 false 会触发额外回调 |
| ⑥ 免死 | `BuffKind.Deadproetected` |
| ⑤ 吸血 | `BuffKind.Leech` + `LeechHandle` |
| ⑦ `beforeCalDmgHpHandle` | 游戏自己的伤害管线内的 **`ref handle` 修改点** → 比共享跳板 `0xBAAB0C` 更精准的挂点候选（**待验证**其调用链） |
| ⑧ 联机 | **伤害数字会 `Broadcast` 给同房其他玩家** → 联机开功能队友直接可见，不只是服务端能查 |

### 2.3 游戏自带「全屏秒杀」的正解（PROVEN，132690）

```csharp
public static void KillAllEnemy() {                       // 由 EditorGoldFinger()（KeyCode.z + Application.isEditor）触发
    BattleHeroUnit leader = StuffObject<BattleTeamManager>.Stuff.aBattleTeam.leader;
    foreach (BattleUnit item in BattleStuff<BattleEntityManager>.Stuff.FindBattleUnitByTeam(BattleTeamFlag.EnemyTeam)) {
        item.unitAttr.Hp = 1f;                            // ObscuredFloat 属性 setter（自动处理加密）
        DamageInfo damageInfo = new DamageInfo(leader, item, isSkill: false);
        damageInfo.atk = 100000000f;
        damageInfo.isReal = true;                         // 真伤
        BattleStuff<BattleServer>.Stuff.CreateDamage(damageInfo);   // ← 走游戏自己的伤害管线
    }
}
```

**这就是全屏秒杀的官方实现**：走 `BattleServer.CreateDamage` 正规管线，**不需要 `runtime_invoke(HpHandle)`、不需要自造结构体、不需要手工铺场**。功能重写应照此实现（这也顺带解决铁律二的 invoke 问题）。

### 2.4 战斗数值全是 `ObscuredFloat`（PROVEN，110561-110577 / 110716-110740）——「数据直写」的正确姿势

```
UnitAttr: FullHp, Hp, Defense, MoveSpeed, Shield, FullShield, ShieldRecover, SkillStrength, CD
另     : Attack, AttackSpeed, CriticalChance, CriticalDamage, Break, BulletCount, ReloadingSpeed …
```

`ObscuredFloat` 结构 = `currentCryptoKey / hiddenValue / fakeValue / fakeValueActive / inited`，读取时解密。因此：

- **直接按 float 偏移裸写 = 破坏解密 → 读到的是随机值**（不是"无效"，是"乱值"，后果比无效更糟）；
- 正确做法：走**属性 setter**（`unitAttr.Hp = x`）或按 ACTk 的 `Encrypt(value, currentCryptoKey)` 规则写 `hiddenValue`；
- `fakeValueActive = ObscuredCheatingDetector.ExistsAndIsRunning`（赋值时判定）→ 与 §3.3 的检测器结论联动。

### 2.5 `BuffKind` 枚举 = 功能实现的原生菜单（PROVEN，113189）

沿用官方枚举即可覆盖大部分需求，且是游戏自己的状态机（联机代理/present 都认）：

```
Invincible=10   无敌         Deadproetected=22 免死        Leech=6        吸血
ResetCD=5       清CD/无限技能  EndlessBullet=12  无限子弹    AntiDamage=7   免伤
Freeze=901      冻结         StopBehaviourTree=28 停止AI行为树（=定怪正解）  Dizzy=902/Silent=903/Confinement=904/Unattackable=905
RealDamage=19   真伤         SufferDam=29      受伤        MotionControll=9 位移控制
AntiMoveDown=26 免疫减速      Size=4 / ColliderSize=18      Chaos=21 / AntiChaos=-21
```

`BuffDriver` 公共 API（112752）：`AddBuff(int buffID, long casterID=0, float externalBaseValue=0, int times=1)`、`RemoveBuff(int)`、`RemoveBuffByCaster(int, long)`、`HasBuffKind(BuffKind)`、`GetBuffListByType(BuffKind)`、`AddBuffListener(BuffKind, Action)`。

→ **第三方修改版宣传的「超级定怪」= `StopBehaviourTree=28`**（不是冻结坐标）。

---

## 3. 上报与检测面（实锤 — 本次新增，旧文档完全未覆盖）

### 3.1 异常上报：客户端会把「UID + 页面 + 异常」POST 给服务端 🔴

```csharp
public static class ExceptionUploader {                                  // 174883
    private static bool needUpload => StaticDataManager.Initiated
        ? StuffObject<SwitchOpenStateManager>.Stuff.IsOpen("ErrorUploadSwitch")   // 服务端开关
        : true;                                                          // 未初始化时默认 true
    private static string uploadUrl => StuffObject<BigzoneManager>.Stuff.Manager + "/error";
    private static string GetPostData(ulong uid, string pageName, string exceptionMsg, string env, string version)
        => new JsonData { ["uuid"]=uid, ["gui"]=pageName, ["error"]=exceptionMsg,
                          ["env"]=env, ["version"]=version }.ToJson();
}
```

- 调用点遍布全局：`物品不足了啦[UseItem_1093]`、`次数不足了啦[QuickStage_1989]`、`点太快了啦[GetAchievementReward_1715]`…（单条上限 50 次，`GetExceptionCount`）。
- **对我们的直接含义**：被服务端拒绝的操作（次数/物品/体力类 `GameException`）都会以**带 UID 的 JSON**落到服务端 `/error`。功能若造成「异常簇」，就是后台最容易筛出的特征。
- **伪装正解**：**不要屏蔽 `UploadException`**（异常率骤降本身异常，且服务端可跨客户端对照）；正确做法是**让功能不产生异常**。
- ⚠️ 与旧代码的直接因果链：`InvokeHpHandle` **丢弃 `exc`** → 托管异常被悬挂 → 在**任意后续 il2cpp 调用点**被 raise → 若正好落在带 `try/catch` 的路径里，就**把我们的崩溃上报给服务端**（`UploadException` 的 catch 里还带 `第N次` 计数）。

### 3.2 程序集名单上传（反注入检测；本包无调用点）🟡

```csharp
public static async void UploadAssemblyInBackground() {                  // 204390
    var names = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.GetName().Name.StartsWith("UnityEngine.")).Select(...);
    await StuffObject<CloudDataManager>.Stuff.SetKeyAsync("Security", "assembly", string.Join(", ", names));
}
```

- 含义：**注入托管程序集（自写 dll / 改名 hotfix）会改变名单并上传云端 `Security/assembly`**。
- 本包实测：`"Security"` 与 `UploadAssemblyInBackground` 在 625k 行里**只有定义、无调用点** → 当前版本**未启用**（可能由 native/反射调用，**待运行时确认**）。
- 设计含义：**保持纯 native 方案（Dobby + 现有 so）不改变程序集名单**；将来若做「热更侧注入」，此处是必处理点。

### 3.3 反作弊工具包 ACTk 存在，但检测器未启动 🟡→倾向「安全」

- 引用 `CodeStage.AntiCheat.{ObscuredTypes, Detectors, Common, Utils}`；自定义子类（109812）：

```csharp
public class AntiCheatDetector : MonoBehaviour {
    public List<string> CheatRecord = new List<string>();
    public void DetectingCheat(string report) { CheatRecord.Add(report); DoCrash(); }
    public static void DoCrash() { while (true) { } }        // 主线程死循环 = 卡死/被系统杀
}
```

- `DoCrash()` 在 625k 行里**唯一调用点是 GM 控制台命令 `GameCrash()`**（234966）；`DetectingCheat` **无调用点**。
  → 前文若把「反作弊主动崩进程」当作闪退主因，**不成立**；这只是一个开发者 GM 命令。
- `ObscuredCheatingDetector.ExistsAndIsRunning` = `Instance != null && Instance.IsRunning`；`Instance` 仅由 `GetOrCreateInstance`（`AddToSceneOrGetExisting`/`StartDetection`）创建 —— **游戏代码从不调用它们**。
- **全包字符串扫描**（4,303 个条目全量解压）：`Obscured Cheating Detector` / `Time Cheating Detector` / `Injection Detector` / `WallHack Detector` / `Anti-Cheat Toolkit Detectors` **只出现在 `hotfix.bytes` 自身**；`level0`(134,176 B) 与 `globalgamemanagers`(602,548 B) 均无。
- **结论（强倾向）**：本构建**未挂载 ACTk 检测器** → `fakeValueActive` 恒 false → 改 `ObscuredFloat` **不会**被 `ObscuredCheatingDetector` 抓。
- ⚠️ **保留项**：2,887 个 bundle 是 `UnityFS`(2020.3.22f1c1)，多含 LZ4 压缩，**若场景里挂了检测器，字符串扫描查不到** → 定为「**待运行时定论**」（Frida 查一次 `ObscuredCheatingDetector.Instance` 即可）。
- 另注：`TimeCheatingDetector` 的存在说明**改时间/倍速是官方预期的作弊面**（即使本包未启用）。

### 3.4 战斗结算上报：服务端能看到什么（PROVEN，138250）

```csharp
var sendMsg = new C2S_NormalBattleEnd_1983_Wrapper {
    BattleUID    = battleUID,
    KillMonster  = allKillMonster.ToArray(),          // 客户端记录的击杀列表
    QuickFlag    = SweepabelChecker(),
    TimeCost     = (int)StuffObject<BattleDriver>.Stuff.totalTimePass,   // 通关耗时
    Win          = StuffObject<BattleDriver>.Stuff.isWin,
    ToTleDamager = StuffObject<BattleDriver>.Stuff.totalDamage           // 总伤害
};
```

- **`TotalDamageCounter(DamageHandle _dam)`（132708）**：
  `if (attacker.team == PlayerTeam && _dam.finalDamage > 0) { totalDamage += finalDamage; maxDamage = max(…); DamageCnt++; }`
  → **我们的伤害放大直接进 `totalDamage`，并作为 `ToTleDamager` 上报服务端**。旧包 `kKillDmg = 99,999,999` 会让服务端收到 ~1e8×命中次数 级总伤害 —— **最容易被后台识别的量级特征**。
- **`OnMonsterDead(int monsterId, Vector3 pos)`（138237）**：仅当 `StaticDataManager.Root.HasmonsterKey(monsterId)` 成立才 `allKillMonster.Add(monsterId)`，否则只写一条 `OnEnemyDead dont have monster id` 日志。
  → 全屏连杀若波及不在静态表内的实体（召唤物等），**客户端击杀列表与真实死亡不一致，且不崩**——静默失真，服务端对照即可发现。
- **奖励由服务端下发**：`NetRewardConvertor.ConvertRewardItem("MonsterItem"/"StageItem"/"FirstReward", s2C_NormalBattleEnd_)`（反射读 FlatBuffer 向量）→ 客户端无法凭空造奖励 → **秒杀只省时间，不直接给资源**（这也实证了 C 档为什么不能做）。
- **开战需服务端批准**：`S2C_NormalBattleStart`，失败抛 `ServerRefuseException("1981", code)` 或 `GameException("BattleNet_ItemNotEnough")`（体力/次数）→ **体力/次数是服务端权威**，旧文档称「次数不足是客户端检查」**不准确**。
- 旧文档记录的 `S2C_NormalBattleEnd.MonsterItem` 越界：`ConvertRewardItem` 用反射按 `rewardName+"Length"` 循环 `rewardName(i)` 取元素 → **越界最可能在这条反射路径**（服务端数组长度与客户端预期不一致）。机制已定位，但「由全屏秒杀触发」**NOT PROVEN**。

### 3.5 渠道与 SDK 侧采集面 🟡

| 面 | 实测证据 | 含义 |
|----|----------|------|
| Unity Services | `com/unity3d/splash/services/core/device/Device.smali` 采集 `android_id`、`installer`、`/proc/meminfo`、`/proc/self/stat`、**`X.509` + `SHA-1` 证书指纹**、屏幕/传感器/电池 | 统计上报含**安装器与证书指纹** |
| QQ/微信 SSO | `com.tencent.mm.opensdk…BaseWXApiImplV10.checkSignature` | 微信 SDK **校验应用真实签名** → 应用内 hook 无效，必须真证书 |
| TapSDK | `com/tds/*` **903 个类**（`TDSNetInterceptor`/`AuthInterceptor`/`TdsTrackerHandler`/`com.tds.common.oauth.TapTapEntryActivity`） | 登录/追踪/网络鉴权 |
| 渠道 | Manifest：`CHANNEL_NAME=UDP`、`UDP_SDK_VERSION=2.0.0`、`UDP_SDK_DIST=packman`；`libtds_core.so` + `libBlueDoveMediaRender.so` | **Unity UDP + TapTap 分发**；无 `libtoolChecker.so` |
| 注入点 | `<application>` **无 `android:name`**（无自定义 Application）**但有** `android:appComponentFactory="androidx.core.app.CoreComponentFactory"` | StarryApp 继承 `android.app.Application` 可行；`appComponentFactory` 必须原样保留 |
| 广告 | `ChannelManager.Channel.ShowAdVideo()`（virtual，200820） | 广告由**渠道 SDK** 实现 → 免广告应在通道层处理 |
| 内购 | `PaymentService`(237473) + Manifest `com.android.vending.BILLING` + 支付宝/微信支付 Activity | 内购存在，但发货在服务端 |

---

## 4. 伪装清单（按实锤修正，取代前文的推测版）

| 面 | 实锤 | 该做什么 | **别做** |
|----|------|----------|----------|
| `ExceptionUploader` | `uuid+gui+error+env+version` → `POST /error` | 让功能**不产生异常**；把 `exc` 处理干净 | 屏蔽上传（异常率骤降同样显眼） |
| `totalDamage`→`ToTleDamager` | 玩家伤害累加后上报 | 伤害倍率控制量级（×10~×100） | 用 `99999999` 恒定值 |
| `KillMonster[]` 一致性 | 仅静态表内 id 入列 | 走 `CreateDamage` 正规管线，保持击杀真实 | 手工铺场 invoke |
| 程序集名单 | `Security/assembly`（本包未启用） | 保持纯 native；不注入新托管程序集 | 自写 dll 注入 |
| ACTk / ObscuredTypes | 检测器未启动（待运行时定论） | 仍按「会被抓」标准写值（setter / Encrypt） | 裸写 float 偏移 |
| 签名 | 微信 `checkSignature`、Unity 证书指纹、TapSDK | 真证书回灌（SignSpoof 换 TapTap 证书） | — |
| 联机 | `DamageNumMsg` **广播给同房** + 服务端校验 | 联机/排行场次关功能（菜单提示） | 联机开全屏秒杀（队友直接看见） |
| 体力/次数/奖励/货币 | 服务端权威（开战批准 + 奖励下发） | 不做（C 档） | 改客户端数字 |
| 时间/倍速 | ACTk 含 `TimeCheatingDetector`（未启用） | 倍速改动画/逻辑速度（官方用法 `_anim.timeScale = attackSpeed`） | 长期乱改全局 `Time.timeScale` |

---

## 5. 功能清单的包内修正（实现层）

| 功能 | 前文建议 | **包内实证后的正解** |
|------|----------|----------------------|
| 秒杀 / 全屏秒杀 | hook 跳板 + 改 `finalDamage` + 铺场 invoke | **`BattleServer.CreateDamage(DamageInfo{atk, isReal=true})`**（照 `KillAllEnemy`），或改 `beforeCalDmgHpHandle` 的 ref handle |
| 无敌 / 免死 | 写 `vic=0` / 数据直写 | **`BuffKind.Invincible` / `Deadproetected`**（`BuffDriver` API） |
| 定怪 | 冻结坐标 | **`BuffKind.StopBehaviourTree=28`**（或 `Freeze=901`） |
| 无限技能 | 改 `cdCounter` | **`BuffKind.ResetCD=5`** / `curSkill.ReSetSkillCD()` |
| 无限子弹 | 改 `BulletCount` | **`BuffKind.EndlessBullet=12`** |
| 移速/攻速/暴击/破防 | 直接写字段 | **必须走 `ObscuredFloat` setter 或按 Encrypt 写 `hiddenValue`** |
| 吸血 / 免伤 | — | `BuffKind.Leech=6` / `AntiDamage=7` |
| 倍速 | 改 `Time.timeScale` | 优先动画/逻辑速度（官方用法），避免触碰 `TimeCheatingDetector` 面 |
| 免广告 | 待定 | `ChannelManager.Channel.ShowAdVideo()` 通道层 |
| 体力/次数/奖励/货币/抽卡/内购 | C 档不做 | **实证支持**：全部服务端权威 |

---

## 6. 仍待验证 / 下一步

| 项 | 状态 | 定论方法 |
|----|------|----------|
| ACTk 检测器是否真的未运行 | 强倾向「未运行」，**待运行时定论** | Frida 查 `ObscuredCheatingDetector.Instance` / `ExistsAndIsRunning`（需授权运行游戏） |
| `UploadAssemblyInBackground` 是否被 native 调用 | 本包无托管调用点 | 扫 `libil2cpp.so` 字符串/交叉引用 |
| `beforeCalDmgHpHandle` 能否作主挂点 | 候选（游戏管线内 `ref` 修改点） | 定位其 `Action<DamageHandle>` 调用链 + 新包偏移 |
| **TapTap 版跳板偏移（替代 0xBAAB0C）** | **✅ 已做（装机实测，见 §7）** | 结论：**根本不需要偏移**——热更类/方法可运行时按名解析；挂点取 `MethodInfo.methodPointer`（同签名桥）+ 按 MethodInfo 分发 |
| hotfix 被服务端热更替换的风险 | **已实锤存在**：运行时热更来自 CDN `dll-android/4880/hotfix.bytes.zip`（`Assembly-CSharp.dll` 9,990,656B），**非 APK 内置那份**（见 §7.1） | 类/方法归属须以运行时下发版本为准；失配即停策略仍待做 |
| 旧包 `MonsterItem` 越界的触发条件 | 机制已定位，触发 **NOT PROVEN** | 需在旧包复现或读旧版反编译（已清理，不可得） |
| 管线注册 | **✅ 已注册（10 阶段，已实跑到 install 并装机）** | `04_scripts/弹力果冻/pipeline.json` |

---

## 7. 运行时实测补充（2026-09-11 装机实测；探针包只读、无 hook）

探针包 = `08_output/弹力果冻/弹力果冻_2.4.0_taptap_probe-releaseSigned.apk`（md5 `ba57fd62…`，装机指纹 ✓）。
完整证据与反汇编见 [探针结论_TapTap2.4.0_20260911.md](探针结论_TapTap2.4.0_20260911.md)。本节只记**对本文档结论的修正/坐实**。

### 7.1 运行时热更 ≠ APK 内置 hotfix（新增事实，影响所有"按行号引用"的结论）

启动日志：`[AOT] load remote res http://gini.oss-cn-hangzhou.aliyuncs.com/dll-android/4880/hotfix.bytes.zip success`。
解包得 **`Assembly-CSharp.dll`（9,990,656 B，2026-09-09）** —— 这才是实际执行的玩法程序集；本文档 §2/§3/§5 引用的行号来自 **APK 内置** `hotfix.bytes`（9,534,976 B）。
两者候选类名基本一致（`BattleServer`/`DamageCalculator`/`KillAllEnemy` 均在），但**行号与个别成员不可跨版本直接引用**；归属关系已用 4880 版 `ilspycmd` 复核（见探针结论 §6.4）。

### 7.2 R-1「mod 线程未 attach / 时序不当」= **实测复现并定因**（升级为 PROVEN）

首次装机的探针在 `IL2CPP: JNI_OnLoad` 后 **约 0.2 秒**就去 attach + 读元数据，结果：

```
F libc: exiting due to SIG_DFL handler for signal 11, ucontext ...
Abort message: 'exiting due to SIG_DFL handler for signal 11'
#00 abort   #01 art::SignalChain::Handler   #06/#07 libunity.so   Process uptime: 0s
```

- 真身是 **SIGSEGV（libunity.so）** → ART `SignalChain` 判定无人接管 → `abort()` 整个进程；
  Unity 自己的 `MemoryManager` 初始化日志要再晚 ~80ms 才出现 ⇒ **调用发生在 il2cpp 运行时初始化完成之前**。
- **`domain != null` 不是就绪判据**：il2cpp 在初始化完成前就发布了 domain 指针。
  修正后改用 **`Assemblies()` 返回非空**（更晚的里程碑，纯元数据只读），并配合正确延时 → **同一探针零崩溃复现成功**。
- **`Il2Cpp::SafeProbe` 兜不住这类崩溃**（生效 handler 是 ART SignalChain）。不要把 `sigsetjmp` 兜底当保险。
- 与 §R-1 的原始推断（"未 attach 的 mod 线程调托管方法"）**机制一致但更精确**：不仅是 attach，**时序早于运行时就绪同样致命**。

### 7.3 R-7「共享入口」= 桥级别实锤（PROVEN）

`HpHandle` 与 `CreateDamage`（同为实例/1 参/void）**methodPointer 完全相同**（`0xB8F8A0`），
而 `KillAllEnemy`（0 参静态）→ `0xB8CA30`、`DamageCalculator` → `0xB95220`。
⇒ **桥按签名形状生成，一个桥服务同签名所有方法**。任何"按地址 hook 一个方法"的方案都会误伤同签名全体；
正解 = **hook 桥 + 按 `method_get_name(mi)` 分发 + 必调 orig**（`skill/re-extract/hybridclr-runtime.md` §四；魂坠深境已跑通）。
旧 4399 的 `0xBAAB0C` 在解释器主体 `[0xBA3CE8,0xBAB884)` 内，比桥更靠里 = 更彻底的共享入口，**双重不可用**。

### 7.4 新增 FAILED：不是所有热更方法都能挂

`GameCalculator.DamageCalculator` 的 methodPointer 指向的代码加载字符串 **`"NotSupportNative2Managed"`**（占位桩），
即 HybridCLR 判定该方法**不可从 native 直接调用**。⇒ 挂点候选必须逐个核对落点，不能由"按名解析成功"推定"能挂"。

### 7.5 框架用法陷阱（两处，建议列经验候选）

1. **注册表时序字段名带 `Ms`，值却是微秒**：`MainModern.cpp:229` 是 `usleep(mod->initDelayMs)`。
   `initDelayMs=15000` 只等 **15 毫秒**（正确写法 `15000000`；参照魂坠深境 `3000000`=3 秒）。
   同理 `initRetryIntervalMs`（我填 5000 → 5 毫秒，应填 `5000000`）、`tickIntervalMs`（`200000`=200 毫秒 ✓）。
2. **遍历 image 不能截断**：热更程序集**最后加载 → 排在枚举尾部**（本包 96 个里索引 95）。
   初版 `i < 64` 导致 20 分钟窗口内每轮误报"热更类未解析"。

### 7.6 ACtk / 上报面（本文档 §3）状态更新

本次实测**未触发**任何 ACTk 检测器行为（探针只读元数据，不改内存、不 hook）；§3 的"强倾向未运行"维持不变，
**仍需 Frida 或行为注入才能定论**（见 §6 表）。`ObscuredFloat` 结论（裸写浮点会破坏解密）在实测中未被推翻，实现仍须走 setter/Encrypt 路径。
