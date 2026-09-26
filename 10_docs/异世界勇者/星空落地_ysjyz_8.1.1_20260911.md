# 异世界勇者 8.1.1 星空落地记录（game_id = `ysjyz`）

> 更新：2026-09-11 · 设备 MuMu `127.0.0.1:5555` · 路线：modern 注册表（`MainModern.cpp` + `Games/YsjYzEntry.cpp`）
> 上游分析：[分析_可逆向功能清单_20260911.md](分析_可逆向功能清单_20260911.md)
> 菜单模板（后台用）：[ysjyz_menu_template.json](ysjyz_menu_template.json)

---

> **状态：已收官**（2026-09-11）—— 功能实测通过，正式包交付，管线 10/10。

## 1. 结论速览

| 项 | 值 |
|---|---|
| 官包 | `02_apks/异世界勇者/异世界勇者_8.1.1_official.apk` |
| 官包 md5 | `710d1cfe9a86f837d5c697b376e73767`（545,635,354 B，versionName 8.1.1 / versionCode 811） |
| 引擎 | Cocos Creator **2.4.2**，`libcocos2djs.so`（27,320,344 B），arm64-v8a（MuMu 装 `arm64` ABI，已实测） |
| 代码 | `assets/assets/main/index.jsc` → XXTEA(`fa6d338a-c7b8-4d`)+gzip → 明文 2,993,782 B / 96,334 行 |
| 注入 | Dobby hook `se::ScriptEngine::evalString`（按名解析，未 strip 导出符号）→ 明文**尾部 splice** JS 补丁块 |
| 外科结构 | `classes2.dex` patch + **新增** `classes3.dex` + `lib/arm64-v8a/{libstarry_magic.so,libc++_shared.so}` + `assets/certs/`；其余 zip 条目字节不变 |
| 正式包 | `08_output/异世界勇者/异世界勇者_8.1.1_surgical-releaseSigned.apk`（546,432,039 B，md5 `94bdd89714236311496c09bd352f0bf6`） |
| so | `libstarry_magic.so` 128,048 B，md5 `54cc06236f7d04bbf070131f60979b22`（包内 md5 == 构建产物，闭环校验通过） |
| 签名 | CN=StarryMagic（`apksigner verify` 过） |
| 功能 | **6 项**（**1 个 `Category_`** + 6 个功能，共 7 个菜单位；见 §3） |
| 运行证据 | **✅ 用户实测确认通过**（2026-09-11「功能都没有问题」→ 单分类版复测「游戏没问题」），见 §7.2 |

> 版本沿革：v1（14 项）→ v2（19 项）→ v3（20 项）→ v4（6 项，大幅精简）→ **v5（6 项 + 单分类，当前交付版）**。
> v4→v5 的唯一变化：三个分类（基础功能／战斗增强／速度增强）**合并为一个** `Category_基础功能`，功能代码一字未改。
> 每次均为**同签名覆盖安装**，无需卸载、不丢数据。全部 `Toggle_*` 默认关；
> ⚠️ **`SeekBar_*` 的默认值 = 声明的第三个数字，不是最小值**（实测见 §7.3）：
> 本包 `dmg` 默认 **50**、`gspd` 默认 **2` → 装完未动菜单就已生效 50 倍伤害 + 2 倍速。
> → 菜单不动时行为与官包一致。

### 1.1 v4 相对 v3 的删减（用户 2026-09-11 指示）

| 处理 | 项 |
|---|---|
| **合并** | `无限技能零CD` + `技能不耗蓝` → 一个 **`无限技能`** |
| **删除（三个广告只留一个）** | 广告CD清零、广告特权一键领取 |
| **删除（战斗类）** | 免疫控制、必中暴击、必闪避、减伤百分比、倍血防 |
| **删除（杂项）** | 去公告弹窗、自动战斗自动技能 |
| **删除（速度只留一个）** | 移速倍率、攻速倍率 |
| **保留** | 免广告、无限技能、无敌、吸血拉满、伤害倍率秒杀、游戏倍速 |
| **分类合并（v5）** | 三个 `Category_` 合并为一个 `Category_基础功能`（用户要求「直接做一个」），6 项功能平铺其下 |

**删减带来的结构性简化**：本包从此**没有任何"改面板"的功能**，所有战斗增益都落在 `AddHp` 结算层。
因此 `result` 的 `Hp/Atk/Def/Free/Php/Patk/Pdef` **天然与官包一致**，
原 v2/v3 的 `CheckResult` 折回与 `Php/Pdef` 伪装**整层删除**（见 §4）。

---

## 2. 注入实现（为什么是尾部 splice）

`main.index.js` 是**单个自包含 IIFE**：

```js
window.__require = function e(t, i, c) { /* 模块表 */ }({ ...模块... }, {}, ["IndexGuide", ...]);
```

- 全部模块（`Service` / `BattleHero` / `UIHelper` / …）在这一次 `evalString` 调用内**同步执行完毕**；
- 所以「明文末尾追加补丁块」= 在 `cc.Service`、`cc.UIHelper`、`BattleHero` 全部就绪之后运行 —— 不需要任何延时、不依赖场景加载时序；
- 补丁块是**纯 JS 覆写**，`.jsc` 文件本体一字节未改 → 外科 zip 组装即可（不重打包、不动 `assets`）。

**命中判据**（避免误伤引擎脚本）：`len ≥ 200000` + 含 `window.__require` + 含 `"BattleHero"` + **不含** `starry_ysjyz_v1`（幂等）。
`cocos2d-jsb.jsc` / `settings.jsc` 三个条件均不满足 → 原样透传。

**关键实现细节**：`BattleHero` 类体 `Init()` 里 `this.Extneds([...])` 把 8 个 mixin 模块的成员
**逐个复制到实例上**（`this[o] = c[o]`，跳过 `Init`），因此 mixin 方法**不能**挂 `BattleHero.prototype`。

补丁用**双保险**：

1. 主路径 —— 直接改混入模块的 `t.exports` 对象（`window.__require("BattleHeroBase"/"BattleHeroSkCost"/"BattleHeroBuffs")`），此后新建实例拷贝到的就是已补丁版本；
2. 兜底路径 —— 包 `BattleHero.prototype.Extneds`，在原始拷贝完成后再对**该实例**包一层。

`wrap()` 用 `fn.__ysjz` 标记幂等，两条路径不会重复包装。

### 2.1 本作战斗系统的两个"必须先弄清楚"的语义

这两条都曾让我写出错误代码 / 错误结论，写在这里避免重蹈：

1. **`AddHp(e,t,i,c,o,…)` 的 `this` 是受击方，末参 `o` 才是攻击方。**
   判据是游戏本体 y30529：`d<0 && o && 2===this.grp && o.StatDam(-d,…)`。
   → 「我方免伤」判 `this.grp===1`，「对敌放大」判 `this.grp===2`。搞反了会写成"给怪物加buff"。
2. **伤害不是 `atk` 的线性函数**（官方公式见 §3.2），所以面板级倍攻**无法精确折回**上报伤害。
   → 倍攻放结算层（`AddHp` 放大 + `StatDam` 1:1 折回），不放面板。这也是 v4 保留伤害倍率、
   而不是去改 `atk.value` 的原因。

---

## 3. 功能清单（featNum 对齐 `YsjYzEntry.cpp`）

**单一分类**：`0 Category_基础功能` 之后 6 项功能平铺（用户要求不做多分类）。

| # | 菜单项 | 挂点 | 做法 | 上报影响 |
|---|---|---|---|---|
| 0 | `Category_基础功能` | — | — | — |
| 1 | 免广告 | `cc.UIHelper.ShowAD(cb)` | 直接 `cb()` | 无 |
| 2 | 无限技能 | `GetSkillCDDuration` + `UpdateSkillCD` + `GetSkillCost` | `grp===1` 时 CD 返 0、`skState[*].Cd=0` 且 `Charged=MaxCharged`、蓝耗返 0 | 无 |
| 3 | 无敌 | `BattleHero.AddHp` | `this.grp===1 && d<0` → 直接 return（不结算） | 无 |
| 4 | 吸血拉满 | `AddHp` 对敌分支后 | 按**实伤 100%** 回血（`hp.cur`） | 无（`hp.cur` 不上报） |
| 5 | `SeekBar_伤害倍率秒杀_1_1000_50` | `AddHp` + `StatDam` | 对敌负伤害 ×N；`StatDam` 按同系数 **1:1 折回**上报 | **Dam 还原为原版量级** |
| 6 | `SeekBar_游戏倍速_1_5_2` | `cc.Service.Battle.Update(dt,node)` | `dt ×= N`（战斗整体节奏） | 无 |

### 3.1 取值约定

- **`SeekBar_*` 没有独立开关**：滑块最小值即「关」（`clamp1()` 把 <1 的值夹到 1）。
- **`Toggle_*` 默认全关**；但 **`SeekBar_*` 默认值 = 声明里第三个数字**（`dmg=50 / gspd=2`），
  **不是** 1 → **装完即已生效 50 倍伤害 + 2 倍速**。要实现「菜单不动 = 与官包一致」，
  必须把默认写成 `SeekBar_伤害倍率秒杀_1_1000_1` 与 `SeekBar_游戏倍速_1_5_1`。
  > `Games/YsjYzEntry.cpp` 的头部注释与本文档早期版本都误记为「默认 1（=关）」，
  > 已按实测更正；**源码注释的更正留到下次构建**（避免现在重建改动正式包 md5）。
- **免广告同时覆盖"快速挂机免费"**：y41348 是 `ShowAD(function(){ Send("fortress/quick") })`，
  免广告直接回调即免费快速挂机，故不再单列。
- 「无限技能」是**零CD + 不耗蓝的合并项**（原两项合一），三个挂点同属 `grp===1` 过滤。

### 3.2 为什么不做"倍攻击"（真改 `atk.value`）

游戏内帮助文本（y56438）给出官方公式：

> 造成伤害 = 伤害系数 ×（**攻方伤害 − 防方护甲**）× 增伤修正 × 命中修正

伤害对 `atk` **不是线性**（含 `−def` 与系数项），面板级倍攻的 `Dam` 无法 1:1 折回。
故用结算层放大（#6）：实伤 ×N 已知精确、上报可精确还原，**效果等价而风险更低**。

---

## 4. 上报一致性设计（v4 已大幅简化）

`chapter/result` 的 `state.result` 里，与本菜单相关的字段来源：

| 字段 | 写入点 | v4 处理 |
|---|---|---|
| `Hp / Atk / Def / Free` | `node.CheckResult`（y13581）`Math.max(自身面板, 现值)` | **不碰**（本包无改面板功能 → 天然与官包一致） |
| `Php / Patk / Pdef` | y31779-31781 `cc.Service.GetHero().GetMaxRateHp/Atk/Def(1)` | **不碰**（同上） |
| `Dam`（总伤害） | `HeroBase.StatDam` → `Battle.AddMonsterHurtDamage`（y32645，**整章跨波次累加**） | **#6 按放大系数 1:1 折回** + 宽松兜底限幅 |
| `Mhp`（怪物血上限） | y32721（当前怪） | 不改 |
| `Score/Score1/Score2` | y54964 `GetAllScoreConst/Rate`（装备类 ADH 表） | 不改（不读面板） |

### 4.1 唯一需要照看的量：`Dam`

- **精确折回**：`AddHp` 内把 `S.sc = N` 存到栈变量，同一次**同步**调用链上的 `StatDam` 读到 `S.sc>1`
  就把上报伤害 `÷ N`，`finally` 还原。能精确复原的依据是 `StatDam` 由 `AddHp` 同步调用（y30529），
  **中间没有异步断层**。
- **兜底限幅**：`result.Dam` 是**整章累计**，而 `result.Mhp` 只是「当前怪」的血上限
  （y32645 / y32721），故 `Dam/Mhp ≈ 本章怪数`，长章节可到几十倍。
  因此上界取 **`Mhp × 100`**（宽松）：只拦明显病态的数值，**绝不把正常战报压低**。
  单测里有「80× 长章节战报不被误伤」与「`Mhp=0` 时不猜、不限幅」两条专门断言。
- 改完在 `Send` 返回后**立即还原**，因此 `__reApi` 防重放串（y56211）与服务器看到的载荷天然一致，
  且同一 `result` 对象反复发送**不会累积**除算/限幅。

### 4.2 残留风险

`无敌` / `吸血` / `伤害倍率` 只改结算结果与 `hp.cur`，`hp.cur` 不在任何上报字段里 → 无残留。
唯一的有界偏离来自 `伤害倍率` 本身（`Dam` 已精确折回，但**击杀速度**变快会影响通关耗时等非数值指标），
属预期效果而非异常。

---

## 5. 外科组装证据（原包 vs 成品）

| zip 条目 | 原包 | 成品 | 判定 |
|---|---|---|---|
| `classes.dex` | 7,972,576 B | 7,972,576 B | **字节不变**（sha256 比对通过） |
| `classes2.dex` | 7,052,188 B | 7,052,820 B（+632） | patch：`LocalApplication.onCreate` + `AppActivity` 生命周期 + `get_ret/get_val/set_ret/set_val` |
| `classes3.dex` | — | 388,044 B | 新增：`com/starry/**`（YsjYzHooks / MenuService / SignSpoof / StarryApp） |
| `resources.arsc` | — | — | **字节不变**（sha256 比对通过） |
| `assets/assets/main/index.jsc` | — | — | **字节不变**（sha256 比对通过） |
| `assets/src/settings.jsc` | — | — | **字节不变**（sha256 比对通过） |
| `lib/arm64-v8a/libstarry_magic.so` | — | 128,048 B | 新增，包内 md5 **==** 构建产物（`54cc0623…`）闭环校验通过 |
| `AndroidManifest.xml` | 二进制 | 二进制 | 见下 |

`classes2.dex` 的 4 处 patch（全部 `invoke-static` 注入，未删改原有指令）：

1. `org/cocos2dx/javascript/LocalApplication.onCreate`：`.registers 1→2`，在 `invoke-super XiPuSDKApplication.onCreate` **之前**插入
   `SignSpoof.install()` + `System.loadLibrary("starry_magic")`；
2. `AppActivity.onCreate` 尾部 → `MenuService.start(p0)`；
3. `AppActivity.onResume/onPause` → `notifyHostResumed/notifyHostLeaving`；新增 `onUserLeaveHint` → `notifyHostLeaving`；
4. 类尾追加 `get_ret(I)Z` / `get_val(I)I` / `set_ret(IZ)V` / `set_val(II)V` → `com.starry.magic.cocos.YsjYzHooks`。

**Manifest 对照**（`aapt2 dump xmltree` 归一化逐行 diff）：仅新增 3 个 `uses-permission`
（`SYSTEM_ALERT_WINDOW` / `FOREGROUND_SERVICE` / `FOREGROUND_SERVICE_SPECIAL_USE`）与 `MenuService` 声明；
`android:name="org.cocos2dx.javascript.LocalApplication"`、`extractNativeLibs=true`、
`appComponentFactory`、`networkSecurityConfig` 等原属性**全部保留**，无属性丢失。
二进制 Manifest 已缓存 `07_patched/异世界勇者/surgical/AndroidManifest.xml`（md5 `2938404f…`），后续重跑不再解码 545MB 官包。

---

## 6. 反馈通道（菜单开关 → JS）

```
Java 菜单（Preferences 持久化）
   └─ nativeSetFeature / nativeSetValue（MainModern.cpp）
        └─ YsjYz_OnFeatureChanged / OnValueChanged → 写 g_feat[]/g_val[] 原子表 → SyncJava()
             └─ com.starry.magic.cocos.YsjYzHooks.setRet/setVal（静态数组）
                  └─ JS 侧每 0.5s 轮询 AppActivity.get_ret(i)/get_val(i)（jsb.reflection）
```

- JS **不在热路径**里跨语言调用（攻击/受击/逐帧都只读本地缓存变量 `window.__ysjyz_s`）；
- 轮询用 `cc.director.getScheduler().schedule(..., 0.5, REPEAT_FOREVER)`，失败退回 `setInterval`；
- `Android_ysjyz.mk` 的 `tickIntervalMs = 200000`（Tick 只当兜底），开关变更靠 `onFeatureChanged` 立即下发。

---

## 7. 验证证据

### 7.1 静态 / 构建（已过）

- JS 补丁块 **node 行为回归 66/66 PASS**（`node 04_scripts/异世界勇者/ysjyz_js_regression.js`）：
  6 项功能的开/关行为 + 关闭态透传 + `StatDam` 1:1 折回 + `Dam` 限幅（含"长章节不误伤""`Mhp=0` 不限幅"）
  + 反复发送不累积 + **我方/怪物侧隔离**，以及一组**负向断言**证明 9 个已删功能确实不再生效
  （`GetAdCD`/`IsNoNeedAd`/`IsAutoDpen`/`Call`/`IsVertigo`/`Move`/`syncAtkSpeedValue`/面板四值）。
- so 构建防呆串自检通过（`ysjyz` / `Toggle_免广告` / `Toggle_无限技能` / `Toggle_无敌` / `Toggle_吸血拉满` /
  `SeekBar_伤害倍率秒杀` / `SeekBar_游戏倍速` / `clampResult` / `YsjYzHooks` / `starry_ysjyz_v1`）；`JNI_OnLoad` 在。
- 管线 10 阶段：`check_apk`/`dump`/`anchor`/`impl_mod`/`build_so`/`surgical`/`sign`/`device_ready`/`install` 全 done
  且各有产物指纹；仅剩 `archive` 等实测证据回填。
- 成品包结构/签名/so 闭环校验见 §5。
- **装机记录（两台设备，同签名覆盖安装，均未卸载）**：

  | 设备 | 时间 | 装的 APK | 装机指纹校验 |
  |---|---|---|---|
  | MuMu `127.0.0.1:5555`（实测用） | 2026-09-11 17:10 | `680a3d2c…` | 设备 base.apk md5 == 产物 ✓ |
  | 小米 M2102K1AC `96f0d883`（Android 14 / SDK 34，宣传录制用） | 2026-09-11 18:14 | `94bdd897…` | 设备 base.apk md5 == 产物 ✓ |

  ⚠️ **两次 APK 的字节不同，但 mod 载荷完全相同**：`libstarry_magic.so` 两次都是
  `54cc06236f7d04bbf070131f60979b22`（字节一致），功能表 7 项防呆串齐全。
  差异只在 `classes3.dex`（菜单 UI），来自 **gradle 重建不可字节复现**。
  详见 §10 教训 5。
- **运行实测**：用户 MuMu 实测通过（2026-09-11），跑的是 `680a3d2c…`；因 so 与功能表同上，
  该结论对 `94bdd897…` 同样成立。

### 7.2 运行（待用户实测）

> 状态：**✅ 实测通过** —— 用户 2026-09-11 先反馈「功能都没有问题」（v4），再在单分类版（v5）上确认「游戏没问题」。
> v5 相对 v4 仅改分类结构与 featNum 重排，功能代码一字未改。
> 下表保留为**回归清单**：后续改版（热更/新版本）重跑时按此逐项复核。

| 待验项 | 观察点 |
|---|---|
| 悬浮菜单出现 | 启动游戏后星空悬浮球/菜单可呼出（Android 12 需授权「显示在其他应用上层」） |
| 菜单内容 | **只有一个分类「基础功能」**，其下 6 项：免广告、无限技能、无敌、吸血拉满、伤害倍率秒杀、游戏倍速（不应再出现「战斗增强」「速度增强」分组） |
| 免广告(1) | 广告点位直接给奖励、无视频；快速挂机不播广告直接结算 |
| 无限技能(2) | 技能图标无 CD、连点可放、蓝条不掉 |
| 无敌(3) | 掉血条不动、不死 |
| 吸血拉满(4) | 命中后自身血量按实伤回升 |
| 伤害倍率(5) | 小怪一击死；**结算不弹「检测到数据异常／审判之眼」** |
| 游戏倍速(6) | 战斗整体加速 |
| 回归 | 关掉全部开关后与官包行为一致；杀进程重启后菜单状态与 prefs 一致 |

---

## 7.3 小米 M2102K1AC（Android 14）装机实测日志（2026-09-11 18:15）

用户自行启动游戏（**管线不启动游戏**：`install` 只做 `am force-stop` + `pm install`，
`verify_installed_apk.sh` 无任何启动命令）。logcat 证据：

```
I StarryMagic: [验证] grant ok exp=1789208125          ← 授权通过，到期 2026-09-12 18:15:25
I MenuService: overlayVisible=true attached=true
I MenuService: host layout changed → show ACTIVITY overlay host=AppActivity alive=true
                                                       ← 未授悬浮窗也能挂上（ACTIVITY 模式）
I Mod_Menu: [YsjYz] feature #1=OFF  ... #4=OFF
I StarryMagic: [开关] #1 Toggle_免广告 = OFF  ... #4 Toggle_吸血拉满 = OFF
I Mod_Menu: [YsjYz] value #5=50      ← 伤害倍率默认 50（非 1）
I StarryMagic: [数值] #5 SeekBar_伤害倍率秒杀_1_1000_50 = 50
I Mod_Menu: [YsjYz] value #6=2       ← 游戏倍速默认 2（非 1）
I StarryMagic: [数值] #6 SeekBar_游戏倍速_1_5_2 = 2
I Mod_Menu: [YsjYz] bundle splice #2 len 3152175->3160148 file=assets/main/index.jsc
                                                       ← evalString 尾部 splice 生效（+7,973 B）
D jswrapper: JS: [ysjyz] patch applied n=5             ← 5 处 JS 覆写加载成功
```

- **无 FATAL / 无 tombstone / 无 ANR**（logcat 里的 tombstone 属 `android.vending`，无关）。
- 喜扑 SDK 正常初始化（`api.kuaigames.com/v1/api/muda`，channel `01aysjyztaptap1335`）。
- `MenuService` 走 **ACTIVITY 模式**挂载 → **不需要悬浮窗权限**即可显示菜单。
- ⚠️ 两条**良性但需知情**的告警：
  1. `DeviceFingerprint: shared id write fail @/storage/emulated/0/Download/.starry_magic/device_id: EACCES`
     —— targetSdk 30 + Android 14 分区存储，写不了 `Download/`。授权仍通过（`grant ok`），
     只是 device_id 无法持久化；若日后出现授权失败，这是第一嫌疑点。
  2. 授权 **24 小时有效**（`exp=1789208125` = 2026-09-12 18:15:25）。录制请在此前完成，
     过期后重新启动游戏会重新签发。

## 8. 复现链

```bash
# 0) 官包已在位：02_apks/异世界勇者/异世界勇者_8.1.1_official.apk
bash 04_scripts/pipeline.sh 异世界勇者 status
bash 04_scripts/pipeline.sh 异世界勇者 next      # 依次跑到底（大包装机需用户确认）

# 离线行为回归（不需要设备）
node 04_scripts/异世界勇者/ysjyz_js_regression.js
```

| 层 | 文件 |
|---|---|
| 注册表入口 | `mod菜单/starry-mod-menu/app/src/main/jni/Games/YsjYzEntry.cpp`（tag/authGameId `ysjyz`，`waitLib=libcocos2djs.so`） |
| 功能实现 | `Games/YsjYzMod.h` / `Games/YsjYzMod.cpp`（evalString hook + JS 补丁块） |
| 构建 | `Android_ysjyz.mk`（`obj_ysjyz` / `libs_ysjyz` 独立目录）+ `04_scripts/异世界勇者/ysjyz_build_so.sh` |
| Java 桥 | `app/src/main/java/com/starry/magic/cocos/YsjYzHooks.java` |
| 外科 | `mod菜单/starry-mod-menu/scripts/surgical_ysjyz_8.1.1.sh`（产出 unsigned） |
| 签名 | `04_scripts/异世界勇者/ysjyz_sign.sh` |
| Manifest 缓存 | `07_patched/异世界勇者/surgical/AndroidManifest.xml` |
| 管线 | `04_scripts/异世界勇者/pipeline.json` |
| dump | `03_decompiled/异世界勇者/`（Manifest xmltree / badging / smali2）、`06_dumped/异世界勇者/`（jsc 明文） |
| 行为回归 | `04_scripts/异世界勇者/ysjyz_js_regression.js`（66 断言，自包含抽取补丁块） |

---

## 9. 遗留 / 待办

1. ~~运行实测~~ —— **✅ 已完成**（§7.2，用户 2026-09-11 实测确认通过）。
2. 后台 `game_id = ysjyz` 注册 + 菜单 JSON 上传（[ysjyz_menu_template.json](ysjyz_menu_template.json)，6 项）—— 用户侧运营操作。
3. `libthemis.so`（TapTap themis-lite 环境组件）行为未知；本次未触发异常，持续观察。
4. 首跑 `surgical` 因 gradle 需写 `~/.gradle` 被文件沙箱拦截（`Operation not permitted`），
   改用 `danger-full-access` 单次放行后通过 —— 属环境限制，与脚本本身无关。
5. `08_output/异世界勇者/` 现有 `_surgical-unsigned.apk`（518M）+ `_surgical-releaseSigned.apk`（521M）。
   unsigned 是 **surgical 阶段的注册产物**：删了会让管线判定"上游链路过期"而拒绝 install
   （本次实测踩到，被迫重跑 surgical → 见 §10 教训 5）。
   要腾 518M 就删，代价是下次装机前需 `reset surgical && next` 重跑。
6. 官方包已按用户授权卸载（签名冲突 `INSTALL_FAILED_UPDATE_INCOMPATIBLE`）；此后各版均为
   **同签名覆盖安装**，无需卸载、不丢数据。

---

## 10. 方法论教训（本次实际踩到）

1. **`grep … | head -N` 会截断证据，不能据此下"不存在"的结论。**
   v3 期间我曾据此把「必闪避」判成"全 bundle 无消费点"，实际证据就在被截掉的那一行
   （y29001 `a = t.free - 攻击方.shotHit`；y29043 `l < a` 圆桌首位）。
   规则：**凡是用来证明"某东西不存在"的检索，必须先看命中总数再决定怎么看**（`grep -c` / 落文件后完整读），
   不要用 `head` 收尾。（该项已在 v4 按用户要求删除，但教训保留。）
2. **先找官方口径，再动代码。** 本作把伤害/命中公式写在游戏内帮助文本里（y56438），
   一次检索就同时解释了"命中修正=圆桌判定""招架率也来自回避""倍攻为何不能精确折回"三件事。
3. **`result` 类字段要连"累计口径"一起确认。** `Dam` 是整章累计而 `Mhp` 只是当前怪，
   按 `Mhp×10` 设限幅会**压低正常战报**（比不设更危险）。设上限前先确认两边是不是同一口径。
5. **`classes3.dex` 经 gradle 构建，字节不可复现 → 重跑 `surgical` 必然改变正式包 md5。**
   本次为装小米手机重跑了一次 `surgical`，APK md5 从 `680a3d2c…` 变成 `94bdd897…`；
   但 `build_so` 未重跑，故 `libstarry_magic.so` 保持 `54cc0623…` 字节一致，
   功能表 7 项防呆串齐全 → **mod 行为未变，只是菜单 UI 的 dex 不同**。
   推论：①"APK md5"是交付物标识，不是内容标识；要证明内容一致应比对 **so md5 + 功能表防呆串**。
   ②删掉 `surgical` 的中间产物会连带让管线判定链路过期，进而触发 surgical 重跑 → 反过来改变正式包 md5。
   ③所以**已交付的 APK 如果想保持字节可复现，就别重跑 surgical**。
6. **删功能比加功能更容易出错**：本次用脚本按行删除 JS 代码块时，
   `s.index('      }\n')` 找的是"第一个匹配的缩进右括号"，结果吃掉了外层 `if (cc.UIHelper) {` 的闭合括号，
   造成 `SyntaxError`。教训：**改完 JS 先单独 `node --check` 抽取出来的补丁块**，再跑行为回归 ——
   这一步能把语法级错误与逻辑级错误分开定位。且**每删一个功能都补一条负向断言**，
   否则"删掉了"只是阅读代码的印象，不是证据。

---

## 2026-09-27 v6：修复「人物等级 ≥199 后打怪恒 1 滴血」

> 用户报障：**人物等级到 199 后，打怪一直是 1 滴血；伤害倍率拉几倍就是多少（拉 1000 倍就造成 1000 伤害）。**
> 详析另见 [分析_199级伤害恒1_修复_20260927.md](分析_199级伤害恒1_修复_20260927.md)。

### 病因（一串行号就够）

| 环节 | 位置 | 代码 | 后果 |
| --- | --- | --- | --- |
| 护甲减法 | main.index.js:29158 | `d -= E;` | **唯一**能把战前伤害打到 ≤0 的运算 |
| 最低伤害钳制 | main.index.js:29807 | `d < 1 && (d = 1);` | d ≤ 0 → 强制 **1** |
| v5 的放大 | `YsjYzMod.cpp` `applyProto` 包 `AddHp` | `orig.call(this, d * amp, …)` | 放大的是**已钳成 −1 的值** ⇒ 伤害恰=滑块值（1×N） |

其余所有对 d 的运算都有下限：暴击 `ke ≥ 1`（main.index.js:29399）、减伤 `1 - Ze ∈ [0.15, 1]`（main.index.js:29723 封顶 .85）、招架只减半（main.index.js:29229-29232）。**所以 v5 的 AddHp 侧无论乘多大都修不了「恒 1」。**

### 为什么恰好卡在 199 级

JS 里**没有** 199/200 的等级常量（`grep 199` 只有 3 处无关命中）。真身是数据侧交叉：怪物战斗属性 = 配置表值 × `Ratio`，`Ratio = 1 + .2 × (怪等级 − 1)`（main.index.js:32228，落点 main.index.js:13638-13640；怪等级由服务端下发，main.index.js:31786/31815 → `LoadMonsters` main.index.js:32171）。设备配置表实测：`Chapter35` OpenLevel=195、Levels=196-200（黑暗森林），怪 mlvl≈200 ⇒ 有效护甲 = cfgDef 49 × 40.8 ≈ **1,999**。玩家靠倍率速升到 199 级、装备没跟上（攻强 ≈2,000 量级）时，`护甲 ≥ 攻强` ⇒ d ≤ 0 ⇒ 恒 1。

### 修法（`Games/YsjYzMod.cpp`，`applyProto` 里 `AddHp` 包装之后）

```js
// ① 我方出刀时记住本刀攻强（o 与 main.index.js:29001 同参）
wrap(P, "Damage", function (orig) { return function (t, i, c, o) {
  var av = 0;
  if (S.dmg > 1 && this && this.grp === 1 && typeof this.GetAtk === "function") av = this.GetAtk(o) || 0;
  var pv = S.ax; S.ax = av;
  try { return orig.apply(this, arguments); } finally { S.ax = pv; }
};});
// ② 只对受击的怪做护甲上限 ⇒ main.index.js:29158 后 d ≥ 20%×攻强 > 0，钳制永不触发
wrap(P, "GetDef", function (orig) { return function () {
  var v = orig.call(this);
  if (S.ax > 0 && this && this.grp === 2 && v > S.ax * 0.8) return S.ax * 0.8;
  return v;
};});
```

不介入的情形（刻意保留官包行为）：护甲本就 < 80% 攻强（正常高攻玩家，公式分毫不差）、`S.dmg ≤ 1`（功能关 / 倍率 1）、怪物打我（攻击方 `grp===2` ⇒ `S.ax=0`）。官方旁路 `limitDam` / `igrDam`（铁壁）/ `DefenseBuff` 护盾 / `aiScript.OnSubDamage` 一律不动。

### 验证与出包

- 离线回归 `node 04_scripts/异世界勇者/ysjyz_js_regression.js` → **73/73 PASS**（v5 的 66 条 + 新 7 条）。新增断言用 mock 复刻官方链路 `GetAtk → d -= GetDef() → d<1→1 → ShareDamage`：倍率关仍 1；倍率 50 → 实伤 20000（400×50）且上报 `Dam` 折回 400；倍率 1000 → 秒杀 23134 血；高攻玩家 98000×50 公式原样；怪打我方（5000 vs 护甲 100）实伤 4900 护甲不被削。
- **构建坑**：共享 `MainModern.cpp` 自 2026-09-22 起在 `JNI_OnLoad` 无条件引用 `SignalQuarantine_Start`(MainModern.cpp:653) 与 `FgoBoot_InjectEarlyFromOnLoad`(MainModern.cpp:663)，而 `Android_ysjyz.mk` 是 09-11 写的 ⇒ `ld.lld: error: undefined symbol`。本包无 `libtprt.so`（无 ACE）、菜单 dex 已在 `classes3.dex` ⇒ 属 **B 类**，把 `Games/NoAceStubs.cpp` 加进 `LOCAL_SRC_FILES` 即可（不编 `SignalQuarantine.cpp`/`FgoBoot.cpp`）。
- **出包=so-swap 增量外科**：`02_apks/异世界勇者/` 与 `08_output/异世界勇者/` 已被用户清空，官包 md5 校验跑不了；v6 只动 so，遂以**设备现装的 v5 包为底，只替换 `lib/arm64-v8a/libstarry_magic.so`**（新脚本 `04_scripts/异世界勇者/ysjyz_so_swap.sh`）：
  - 底包 `07_patched/异世界勇者/base_v5_from_device.apk`（设备 `/data/app/…/base.apk`，md5 `680a3d2c…`；注意这是 v5 内容、只是当时外科重跑过，另一个合法 md5 是 `94bdd897…`，见本文第 221-222/336 行）+ 包内 so md5 == `54cc0623…` ✓ + `classes3.dex` 在 ✓
  - 新 so **130,000 B / md5 `1cd91a19…`**（v5 为 128,048 B）；unsigned md5 `eee30862…`
  - 逐条目核验：**24,624 个非 so 条目 size/CRC 与 v5 全等**，只有 so 变化；旧 `META-INF/{MANIFEST.MF,STARRY.RSA,STARRY.SF}` 剥离后由 `starry_release.jks` 重签（CN=StarryMagic）
  - 正式包 `08_output/异世界勇者/异世界勇者_8.1.1_surgical-releaseSigned.apk` = **546,432,063 B / md5 `7ac89939c9b1f31cb27cff4b7454f3a4`**
- **装机**：真机 `f9a116a2`（MuMu 127.0.0.1:5555 已弃用，pipeline.json 的 serial/env 已更新）覆盖安装成功，未卸载、未清数据；`verify_installed_apk.sh` 报「设备 base.apk md5 == 本地产物」✓。
- **待用户实测**：进游戏打怪，确认倍率拉 N 打出「真实伤害 × N」而不是 N。

## v7（2026-09-27）— 删吸血 + 伤害倍率 1-100 + 默认值回 1 + 菜单框架换新

用户要求（v6 装机后）：①删「吸血拉满」；②伤害倍率改 **1-100** 并改名「伤害倍率」（去「秒杀」）；③伤害倍率默认 **1**、游戏倍速默认 **1**；④调查本游戏反检测（防封号/屏蔽上报/存档上传上传）；⑤mod 菜单框架 dex 一并换新构建。

### 菜单现状（6 项 / featNum 0..5）

| featNum | 名称 | JS 侧读法 |
|---|---|---|
| 0 | `Category_基础功能` | — |
| 1 | `Toggle_免广告` | `RET(1)` |
| 2 | `Toggle_无限技能` | `RET(2)` |
| 3 | `Toggle_无敌` | `RET(3)` |
| 4 | `SeekBar_伤害倍率_1_100_1` | `S.dmg = clampv(VAL(4), 100)` |
| 5 | `SeekBar_游戏倍速_1_5_1` | `S.gspd = clampv(VAL(5), 5)` |

enum 缩为 `YSJYZ_FEAT_ADFREE=1, SKILL=2, INV=3, DMG=4, GSPD=5, COUNT=6`（`Games/YsjYzMod.h`）。

### 改名/重编号为什么安全

- 滑块默认值 = **菜单名里第三个数**（缺省 = min）：`FeatureListParser.java:93-114` 解析 `SeekBar_名_min_max[_default]`，`FeatureListParser.java:117-141` 无偏好时取该 default 并夹到 `[min,max]`。
- `Preferences` 以 featNum 字符串为 key（`Preferences.java:20-22, 199-229`），但 **`ModBridge` 构造函数每次菜单初始化都 `Preferences.clearFeaturePrefs();`（`ModBridge.java:189-202`，注释「功能列表随 so 切换；丢掉旧 featNum 偏好，默认全关」）** ⇒ 旧值不继承，v6→v7 重编号不会串档。
- JS 侧仍加 `clampv(v, hi)` 兜底（防 UI 层给越界值）。

### JS 补丁改动（`Games/YsjYzMod.cpp`）

`S` 去掉 `leech`（`{adfree:0,skill:0,inv:0,dmg:1,gspd:1,sc:1,n:0,ax:0,clip:0}`）；`refresh()` 的 `S.dmg = clampv(VAL(4), 100)`、`S.gspd = clampv(VAL(5), 5)`；`AddHp` 对敌分支由 `S.dmg > 1 || S.leech > 0` 改为 `S.dmg > 1`，吸血回血块整体删除。v6 的 `Damage`/`GetDef` 护甲上限修复保持不变。

### 菜单框架 dex 换新（新增阶段 build_menu_dex）

- 链路：`gradle assembleRelease` → `apktool d` → 合并 `com/starry/**` smali（**159 个**，v5 为 129）→ `smali assemble` → `07_patched/异世界勇者/classes3_menu.dex`（**454,328 B / md5 `d9767e09…`**）。脚本 `04_scripts/异世界勇者/ysjyz_build_menu_dex.sh`，末尾用 `baksmali` 回验硬依赖签名。同输入两次 assemble **大小相同、md5 不同**（dex 内部序/校验字段非确定性，内容等价）。
- **兼容性核验（结论：可整包置换）**：classes2.dex patch 只引用 **3 个类的 8 个方法** —— `Lcom/starry/magic/sign/SignSpoof;->install()V`、`Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V` / `notifyHostResumed(Landroid/app/Activity;)V` / `notifyHostLeaving()V`、`Lcom/starry/magic/cocos/YsjYzHooks;->getRet(I)Z` / `getVal(I)I` / `setRet(IZ)V` / `setVal(II)V`；新旧 `MenuService` 方法清单逐行一致，新 dex 全部满足。新框架新增 `com/starry/magic/{conch,dwdlm,h5box}/`、`cocos/{Dwdlm,Gyzw,Rsjy,Ysxz,Zzbx2}Hooks`、`menu/{MenuSafeHandler,StateMirror}`、`ota/OtaSoStamp`、`sign/SignSpoofAppComponentFactory` 等。
- ⚠️ 本游戏**不用 `StarryInitProvider`**（其它游戏用 provider），走 classes2 patch：LocalApplication.onCreate 注入 `SignSpoof.install()` + `System.loadLibrary("starry_magic")`，AppActivity 注入 MenuService 生命周期与 `get_ret/get_val/set_ret/set_val`。

### 出包（增量置换，同时换 so + classes3.dex）

`04_scripts/异世界勇者/ysjyz_so_swap.sh` 重写为 v7 双重置换（底包 md5 白名单 `{680a3d2c…, 94bdd897…}`，底包内 so 须 `54cc0623…`、classes3.dex 须 `916bdd49…`；`YSJYZ_SKIP_CLASSES3=1` 可退化为纯 so-swap）：

- 新 so（build_so）= **129,688 B / md5 `8aa06587c7b63b925fcecef8d58c78c1`**
- unsigned = `46874b72…` / 543,564,957 B（消失条目仅旧签名 `META-INF/{MANIFEST.MF,STARRY.RSA,STARRY.SF}`，新增 **0**，内容变化恰 2 条，其余 **24,623 条** size/CRC 与底包全等）
- 签名后正式包 = `08_output/异世界勇者/异世界勇者_8.1.1_surgical-releaseSigned.apk` **546,464,855 B / md5 `cc44d8b918b999c2cd1be76b4890094d`**（包内 so=`8aa06587…`、classes3=`d9767e09…`、classes2=`974858bf…`，CN=StarryMagic）

### 回归与装机冒烟

- 离线回归 `node 04_scripts/异世界勇者/ysjyz_js_regression.js` → **77/77 PASS**（新增：吸血删除后打怪不回血、倍率 1000 被夹到 100 等）。
- 装机：真机 `f9a116a2` 覆盖安装（未卸载、未清数据），`verify_installed_apk.sh` 报「设备 base.apk md5 == 本地产物」✓。
- 冒烟：启动 45s 进程存活（`pid 2428`），无 FATAL；日志 `[YsjYz] bundle splice #1 len 3152175->3161770 file=assets/main/index.jsc`（注入命中）、`JS: [ysjyz] patch applied n=7`、菜单读到 6 项且 **#4 伤害倍率=1 / #5 游戏倍速=1**（新默认值生效）、吸血项消失。
- 观察：新框架自带 `AuthClient` 访问 `https://47.100.216.101:8443/v1/bootstrap?game_id=ysjyz&device_id=…`（mod 自身授权通道，与游戏上报无关）。

### 反检测调查（独立报告）

`10_docs/异世界勇者/反检测调查_ysjyz_20260927.md`。核心结论：网络层为**明文 WebSocket**（`send(api, JSON.stringify(body))`，无 sign/时间戳/nonce/加密），JS 层可按 api 名完全拦截请求与响应；**客户端零本地校验**，判罚全在服务端 `ErrCode`（`errData`→「审判之眼」+踢回登录；`invalidData`→禁入团本；`ban/tokenBan`）；**不存在云存档上传接口**（纯服务端权威，本地只存 token）；第三方 SDK 全在 Java/native（JS 唯一可屏蔽自有埋点 `cc.UIHelper.Stat`）；风险排序 **倍速＞无限技能＞伤害滑块＞无敌＞免广告**。

### 实测结论与待办

**2026-09-27 用户实测通过（「游戏没问题」）**：菜单 6 项、伤害倍率/游戏倍速默认 1、199 级打怪伤害修复均正常 ⇒ 已按「收尾+正式包+清理+git」口令收尾，`08_output/异世界勇者/` 裁剪为唯一正式包。

反检测五条建议尚未实施：倍速加发送时刻门限并限 ≤2、静默 `cc.UIHelper.Stat`、`clampResult` 换「期望 DPS × 实测耗时」基准、无限技能按 `GetBattleMode()` 分场景自动关闭、调试期 `errData` 只记日志。
