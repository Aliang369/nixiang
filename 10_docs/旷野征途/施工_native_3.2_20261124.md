# 旷野征途 3.2 / vc32 · native 阶段3b 施工（ver==32 + 装钩自证）与 so 构建（2026-11-24）

> 生成：2026-11-24　执行：子代理 builder-native（阶段3b）
> 范围：只改 **游戏私有** `Games/旷野征途Mod.cpp` + 构建 so + 门禁。**未装机、未上机、未组包、未 commit/push。**
> 输入：`10_docs/旷野征途/静态_native_3.2_20261124.md`（四挂点表 + 给阶段3 的必改项）、`环境_Houdini_hook可行性_20261124.md`
> 设备口径变更（lead 2026-11-24 追加）：目标设备改为**真机 Xiaomi M2102K1AC / arm64-v8a 原生 / 未 root**（不是雷电模拟器）⇒ 诊断必须进 logcat。

---

## 0. 一句话结论

3.2 探针（三点取二）与 `ver==32` 地址分支已落地，两处版本门（战斗 / 经济）已扩到 32；
新增 **MODHOOK / MODHIT 单行诊断**（logcat + 文件双落）用于判定「钩真的装上了、真的被调用」；
**战斗合同已按 3.2 真值复核**（HotUpdate 已解出）：argBase 五槽 +0/+8/+16/+24/+32 不变，
argc 口径 = 4（不含 this，metadata Param 行实测），过滤器放行 4/5 两口径并首命中落 logcat；
**真机返工三轮**（2026-11-24）：
① `Time.set_timeScale` 按名装钩「装上但零命中」⇒ 改 **inline hook @rva 0x37B27A8**（序言 0xFC1E0FE8）；
② MODHOOK 的「orig 不在 guest 区间」判据是**误报**（orig 是 Dobby trampoline）⇒ 改为「orig 必须在任意可执行段」；
③ 关掉加速不主动恢复（最后一笔 1.8 留在 `Time.timeScale`，最长要等游戏下次调 setter）⇒ 新增
`g_spdRestore` 待恢复标志 + Tick 写回 1.0，force 改 100 ms 节流。
so 已构建：**239,592 B**，sha256 `f9b31139…e5fdc14c`，ELF64 AArch64。
两个门禁 **全 PASS**（战斗 16 项 / 菜单 16 项），`preflight.sh` **PASS=4 FAIL=0 REMIND=1**。
仍未证：加速度的实际效果与「关后多久恢复」的实测时延、argc 实际值、无敌/百倍实效（均需真机日志）。

---

## 1. 改动点清单（文件:行 → before / after）

文件：`mod菜单/starry-mod-menu/app/src/main/jni/Games/旷野征途Mod.cpp`（79,415 → 100,211 B；+372 / −47 行）
另改：`04_scripts/旷野征途/check_battle_contract.py`（+14 行：3.2 合同 6 项检查，2.4 旧值保留为历史分支）

| # | 行 | before | after |
|---|---|---|---|
| 1 | 40 | — | `#include <sys/stat.h>`（3.2 探针 stat 文件大小） |
| 2 | 89 | — | `static void ModHitFirst(const char* tag);` 前向声明 |
| 3 | 195+ | `g_libVer` 注释只到 24 | 注释加 `32=3.2`；新增常量 `kOff32Show=0x1CFC6D0` / `kOff32Interp=0x19F6364` / `kSize32Lib=66268112` |
| 4 | 216 | 探针表 `{{24,0x1CCE6CC},{22,0x1CA9130},{18,0x1C8A9AC}}` | 表首加 `{32, kOff32Show}`；`ver==32` 分支做「三点取二」判据（见 §2） |
| 5 | 237 | — | `LOGI " verprobe 3.2 cond show=%d interp=%d size=%d/%llu"`（logcat 可见） |
| 6 | 532 | — | `LibGuestRange()`：maps 里 libil2cpp 全部段并集跨度 [lo,hi) |
| 7 | 555 | — | `LibFileSize()`：从 maps 取路径再 stat（大小兜底） |
| 8 | 620 | — | `LogHookDiag()`：**MODHOOK** 单行（LOGI + DbgLog 双落） |
| 9 | 435/439/448 | DobbyHook 后只记 rc | 成功/失败/信号熔断三条路径**都**落 MODHOOK（installed=1/0/-1） |
| 10 | 785 | — | `ModHitFirst()`：**MODHIT** 首命中，每 tag 只落一次（mutex 保护） |
| 11 | 1021–1033 | `interp/noad/handleNative` 只到 24 | 加 `ver==32`：interp=`0x19F6364` / noad=`0x1CFC6D0` / handleNative=`0x1CFC448`（只记录不挂），**全部 file offset** |
| 12 | 1035 | 注释「2.2/2.4 由 InstallBattleHooks 单点装」 | 加 3.2（`EnterInterp` 仍只在 1.8 由 kSlots 填 rva） |
| 13 | 1227 | `verHook == 22 || verHook == 24` | `|| verHook == 32`（战斗帧入口装钩门） |
| 14 | 1704 | `g_libVer == 22 || == 24` | `|| g_libVer.load() == 32`（Economy22 版本门） |
| 15 | 105/837/967/985/1181 | — | `ModHitFirst("GameUpdate"/"EnterInterp"/"NoAd"/"TimeScale"/"Battle")` 五个命中点 |
| 16 | 1047–1062 | 段头注释只写 2.2/2.4 合同 | 补 **3.2 合同块**（rva 0x0FF20C / body 1594 B / Param 行 4 / ldarg 首用 / IL 锚点 / 落血语义），**2.4 旧值整段保留**作历史分支 |
| 17 | 1081–1099 | `kSlotThis/Atker/Dmg` 三槽 | 补 `kSlotSeat=16` / `kSlotOnDie=32`（五槽齐全）+ `static_assert` 全槽校验；新增 `kApplyHpArgc=4` / `kApplyHpArgcWithThis=5` |
| 18 | 1122 | — | `static std::atomic<int> g_argcLog`（argc 口径只记一次） |
| 19 | 1187–1196 | `argc == 4` 硬编码 | 改为 `argcOk = (argc == kApplyHpArgc \|\| argc == kApplyHpArgcWithThis)`；**收窄到 `BattleRoleActor.ApplyHp`** 后落一次 `LOGI battle: ApplyHp argc=%u`（logcat 可判 4/5 口径） |
| 20 | `check_battle_contract.py` | 10 项检查 | +6 项 3.2 检查（五槽 / 双口径 / 无字面量 / 3.2 锚点 / 2.4 历史保留 / 落血语义） |
| 21 | 207–208 | — | 新增 `kOff32Timescale = 0x37B27A8` / `kInsnTimeScale = 0xFC1E0FE8`（TimeScale inline hook 用） |
| 22 | 1093–1105 | TimeScale 槽 rva 恒 0（只按名） | 新增 `timescale` 表（3.2=0x37B27A8 / 2.4=0x37541A0 / 其它=0），槽位 `rva=timescale` + `expectInsn=0xFC1E0FE8`；`slot addrs` 日志加 timescale 字段 |
| 23 | 784–796 | 装钩只记成败 | `HookSlotInstall` 落 `path=rva 0x…` / `path=by-name ok=…`（区分 inline 与按名两条路） |
| 24 | 584–604 / 655–675 | orig 不在 guest 区间 ⇒ WARN「x86 空间」 | 新增 `IsExecRange()`；判据改为「orig 必须在任意 `x` 段」，MODHOOK 尾部加 `origExec=%d`，trampoline 落 INFO 而非 WARN，删掉「x86 用户空间」错误说法 |
| 25 | 152 | — | 新增 `std::atomic<bool> g_spdRestore`（OFF 待恢复标志；UI 线程只置位） |
| 26 | 1841–1853 | `case FEAT_SPD: g_spd.store(enabled);` | 改 `g_spd.exchange` + 「开→关」沿置 `g_spdRestore=true`；重新开启时撤销待恢复 |
| 27 | 1915–1943 | Tick 每帧 `g_origSetTimeScale(1.8f,m)` | 新增 OFF 恢复块（`1.0f` + `LOGI restore timeScale=1.0 (toggle off) spd=%d`，`g_tsMethod` 未就绪时**不吃标志**）；force 改 **100 ms 节流**，日志 `force timeScale=1.8 (100ms tick) spd=%d` |
| 28 | 1945–1950 | `ForceClear` 直接 `g_spd.store(false)` | 改 `if (g_spd.exchange(false)) g_spdRestore.store(true)`（同一缺陷的另一条出口） |

**未改**：`Main.cpp` / `MainModern.cpp` / `GameConfig.h` / 公共 `Android.mk`（零字节）；`04_scripts/_common/**`、`pipeline.sh`；`Extry.cpp` / `Economy22.h` / `BattlePure.h` / `BattleFlow.h`（3.2 无需改，见静态文档 §7-B）。
`NoAd` 按名三要素（`Dirichlet.Ad` / `DirichletRewardAdBase` / `ShowInternal` argc=1）**原样保留**，ver=0 时仍走按名。

**逐处核对（防漏硬编码 24）**：`rg -n "== ?24|== ?22|verHook ==|g_libVer.load\(\) ==" Mod.cpp`
= 只剩「每版本地址三元组」与 `static_assert(kSlotDmg == 24)`（槽位偏移，与版本无关）；**两处放行门都已含 32**（1227 / 1704）。

---

## 2. 3.2 判据（三点取二）与代码口径

| 条件 | 判据 | 3.2 静态实测 |
|---|---|---|
| ① show | file `0x1CFC6D0` 处 4 字节 == `0xF81E0FFE` | 是 |
| ② interp | file `0x19F6364` 处 == `0xF81D0FFE` | 是 |
| ③ size | `stat(libil2cpp.so).st_size == 66,268,112` | 是（底包实测 66,268,112 B） |

接受：`①&&(②||③)` 或 `②&&③`；只中①（其余不符）⇒ 判未知并**继续向后探 24/22/18**，不硬套 2.x 地址。
理由：`0xF81E0FFE` 是常见栈帧序言，单点命中不足以定版；文件大小不受段内同构序言影响，是最稳一条。
探针失败时 `ver=0`、所有 RVA 归零 ⇒ 免广告走按名，战斗两项**显式失败关闭**（行为与 2.4 一致，不新增风险）。

---

## 2b. 3.2 ApplyHp 合同反推（argc / argBase 槽位 / 受击者判定）

> 输入：`13_scratch/kyzt32_native/hotupdate/HotUpdate.decrypted.dll`（3,583,488 B，sha256 `812e6563…d551b0`，合法 PE32）
> 反汇编：`13_scratch/kyzt32_native/dis_ApplyHp.txt`；复核脚本与输出：`hd_verify3_applyhp32.py` → `verify_applyhp32.txt`

### 2b.1 参数表（metadata 实测，不是推测）

```
rva=0x0FF20C  body=1594 B  fat
ParamList rows: ['atker', 'atkSeatNum', 'dmgInfo', 'OnDie']   → Param 行 = 4
ldarg 首用：ldarg0@IL_0003(this)  ldarg1@IL_0014(atker)  ldarg2@IL_004C(seat)
            ldarg3@IL_0000(dmgInfo)  ldarg.s 4@IL_052B(OnDie)
使用次数：{0:91, 1:28, 2:7, 3:40, 4:2}      ← 没有第 6 个槽（不存在 ldarg 5）
```

### 2b.2 槽位反推（按 ldarg 顺序，不照抄推测）

| argBase | 槽内容 | IL 证据 |
|---|---|---|
| **+0** | this（**受击方** / HP 主人） | `IL_0240 ldarg.0; IL_0241 ldarg.0; get_curHp; IL_0247 ldloc.0; op58(add); IL_0249 set_curHp` |
| **+8** | atker | `IL_0013 ldarg.3; IL_0014 ldarg.1; IL_0015 stfld dmgInfo.atker`；`IL_017f ldarg.1; get_SummonOwner`；`IL_0524 Died(ldarg.1)` |
| **+16** | atkSeatNum（int） | `IL_004C ldarg.2; IL_004D ldarg.0; get_LocalSeatNum; IL_0053 bne.un`（座位号与自身座位比较） |
| **+24** | dmgInfo | `IL_000C/0015/001C stfld dmgInfo.WasApplied / .atker / .target`；`IL_0022 ldfld dmg` |
| **+32** | OnDie（Action） | `IL_052B ldarg.s 4; brfalse; IL_052F ldarg.s 4; IL_0531 ldarg.0; callvirt Invoke`（回调传 this） |

⇒ **结论：dmgInfo 仍在 +24，OnDie 在 +32，与 2.4 完全同序**（3.2 没有把参数顺序打乱）。

字段 token 对照（BattleDamageResult）：`dmg=0x40024f7` / `WasApplied=0x4002503` / `atker=0x4002505` /
`target=0x4002506` / `professionSource=0x40024ec` / `isAreaHit=0x40024ed` / `reallyDmg=0x40024f8` / `isKill=0x4002502`。

落血语义（决定「写 dmg 够不够」）：

```
IL_0206 ldarg.3; callvirt get_intDmg; stloc.0      ; lloc0 = dmgInfo.intDmg()
IL_0240 ldarg.0; ldarg.0; get_curHp; ldloc.0; op58(add); set_curHp
get_intDmg: ldfld dmg; if (dmg < -2.14748e9) return 0x80000080; else return CeilToInt(dmg)
```

⇒ `intDmg` **只由 `dmg` 派生**（无独立缓存字段）⇒ hook 在 orig 前写 `dmgInfo.dmg` 就能左右最终扣血；置 0 经任何倍率相乘仍是 0。

### 2b.3 argc 的口径（关键分歧点，已按实测落地）

| 口径 | 值 | 依据 |
|---|---|---|
| `il2cpp_method_get_param_count` | **4** | metadata Param 行 = 4（atker/atkSeatNum/dmgInfo/OnDie），**不含 this** |
| this + 参数（槽位数） | **5** | argBase 五个槽 +0..+32 |

代码里 `api.method_param_count(mi)` 取的是前者 ⇒ **口径 = 4**。内部旁证：同一份 kSlots 里
`GameManagerMono.Update()` 用 `argc=0`（实例方法 0 参数）安装成功、`ShowInternal(options)` 用 `argc=1`
——两者都证明本框架的 argc 不含 this。

处理：过滤器 **4/5 都放行**（`kApplyHpArgc` / `kApplyHpArgcWithThis`），首次命中 `ApplyHp` 时落
`LOGI "[KYZT] battle: ApplyHp argc=%u"`（只一次）。若真机打出 4 ⇒ 口径确认为 metadata；
若打出 5 ⇒ 本机 il2cpp 把 this 计入，功能照常匹配。**不放行字面量**（`argc == 4` 已从代码删除，门禁检查）。

### 2b.4 受击者判定复核

- `this`（+0）= 挨打方：`ApplyHp` 全程对 `ldarg.0` 做 `get_curHp/set_curHp/Died`；
- `atker`（+8）= 出手方：`dmgInfo.atker`、`ApplyLifeSteal`、`AddDmgSum` 都用 `ldarg.1`；
- 本机：`BattleRoleMgr.Instance.<player>k__BackingField`（IL 侧 `ldsfld Instance; callvirt get_player` → `IL_0116 stloc.s 4`），
  随后 `IL_0184 ldarg.1 vs player`、`IL_01B4 ldarg.0 vs player`、`IL_0625 player vs ldarg.0`
  ⇒ 现有 `(pThis == lp)` / `(pAtk == lp)` 两个方向判断**仍然成立**。
- 已知限制（2.4 遗留，3.2 未变）：`atker` 可能是召唤物（`get_SummonOwner`），此时 `pAtk != lp`，
  本机召唤物的「百倍」不会触发；不影响「无敌」（按 this 判）。

---

## 2c. TimeScale 改 inline hook（真机实测返工，2026-11-24）

### 2c.1 真机证据（96f0d883，官方 3.2 底包 + v3 包）

```
TimeScale hooked by-name Time.set_timeScale(argc=1)      ← 装成功
MODHOOK tag=TimeScale target=0x7E46E487A8 orig=0x7F6E1DD000 ... installed=1
（此后全量 MODHIT 只有 GameUpdate / Battle，TimeScale 零命中，开加速无任何效果）
```

### 2c.2 原因

`UnityEngine.Time::set_timeScale` 在游戏 C# 里是 **AOT 直调**（`bl` 到函数体）。按名钩改的是
`MethodInfo->methodPointer`，而 AOT 直调点不经过 methodPointer ⇒ **换指针影响不到调用点**。
反证：`GameManagerMono.Update` 同样 by-name 却命中 —— 因为它由 Unity 引擎回调（delegate/虚表路径）。

### 2c.3 改法（rva 优先，按名只作兜底）

| 项 | 值 |
|---|---|
| file offset | **0x37B27A8**（VA 0x37B67A8） |
| 序言校验字 | **0xFC1E0FE8**（`str d8,[sp,#-0x20]!`） |
| 装钩路径 | `HookSlotInstall` → `rva` 有值先走 `HookAtRva`（序言不符即拒绝、不硬装）；只有 rva=0 / 序言不符 / 不在 RX 才退回 `HookByName` |
| 日志区分 | 成功走 rva ⇒ `LOG "[KYZT] TimeScale hooked @rva 0x37B27A8"` + DbgLog `hook TimeScale: path=rva 0x37B27A8`；走兜底 ⇒ `hooked by-name` + `path=by-name ok=?` |
| 业务逻辑 | 不变（开关开 → 原值换 1.8f；Tick 主动钉 1.8 的兜底也保留） |
| 2.4 顺带 | `0x37541A0`（VA 0x37581A0，同一序言，静态文档 §2c）；2.2/1.8 无实测值 ⇒ 0，仅按名兜底 |

注意（inline 与按名的 ABI 差异）：直调只传 `float`，x1 里没有 MethodInfo ⇒ `Hooked_SetTimeScale` 的
`method` 参数是调用点残留，只被原样转发给 trampoline，而 `set_timeScale(float)` 不读第二参数，无副作用；
`g_tsMethod` 仅当「已命中」标记用。代码里已写明（`Mod.cpp:1029-1032`）。

---

## 2d. 加速开关的 OFF 恢复（真机 v4 缺陷，2026-11-24）

### 2d.1 真机现象（开→关→再开）

```
17:30:08 [开关] #5 = OFF
17:30:13 set_timeScale call#3 arg=1.000 spd=0     ← 5 秒后游戏自己才调一次，这时才恢复
```

用户观察「关了还是有战斗加速」。原因：`Hooked_SetTimeScale` 的转发是对的（`v = spd ? 1.8f : value`），
但**关掉那一刻没人主动把 1.0 写回去** —— Tick 里只有「开时每帧钉 1.8」的分支，没有「关时写 1.0」的分支。
最后一笔 1.8 就留在 `Time.timeScale` 上；战斗内若不再调用 setter，就一直加速。

### 2d.2 改法

| 项 | 实现 |
|---|---|
| 标志 | `g_spdRestore`（atomic）：`case FEAT_SPD` 里 `g_spd.exchange(enabled)`，「开→关」沿置 `true`；重新开启则置回 `false`（撤销竞态下的 1.0 写入） |
| 线程纪律 | **UI 线程只置标志**，不直接调 `g_origSetTimeScale`（跨线程碰 Unity API 的风险照旧规避） |
| 落地 | Tick 里 `if (g_spdRestore && !g_spd) { m = g_tsMethod; if (m && g_origSetTimeScale) { 写 1.0f; 清标志; LOGI } }` |
| 不吃标志 | `g_tsMethod` 还没解析出来时**保持 pending**，下一帧再试（否则关掉后再也不会恢复） |
| 另一条出口 | `旷野征途_ForceClear()` 原本直接 `g_spd.store(false)`，同样会让 1.8 残留 ⇒ 改成 `if (g_spd.exchange(false)) g_spdRestore.store(true)` |
| force 节流 | Tick 的 `1.8f` 钉值从**每帧**改成**100 ms 一次**（`steady_clock` 自算，不在 Tick 做重活；每帧调既浪费又与开关竞态） |
| 日志 | `force timeScale=1.8 (100ms tick) spd=%d`（保留首 3 次）；`restore timeScale=1.0 (toggle off) spd=%d`（每次 OFF 恢复一次） |
| 业务值 | 仍是 **1.8**，未加倍数配置、未加新开关 |

### 2d.3 期望时延

关掉开关后**一次 Tick**（≈ 主循环间隔，正常 < 50 ms；最坏几十毫秒）内即写回 1.0，
不再依赖游戏自己下一次 `set_timeScale`（原来最多要等 5 s 或整场战斗）。

---

## 3. 装钩自证（MODHOOK / MODHIT）—— 真机未 root 的唯一证据链

判定口径（**2026-11-24 真机修订；首版判据是误报，勿回退**）：

- Dobby 返回的 `orig` 默认是它自己 mmap 的 **trampoline**（独立可执行页），天然**不在** libil2cpp 区间内；
- 真机 arm64 用户态地址本来就在 `0x7xxx…`，**「0x7xxx = x86 空间」是错的**；
- 唯一有效判据：`orig` 必须落在**任意 `x` 权限映射段**（新字段 `origExec=1`）；
- 只有 `orig=null` 或 `origExec=0`（且 `installed=1`）才是「装钩不可信」，此时 `LOGW` 一行；
  trampoline（`inGuest=0` 但 `origExec=1`）只落 `LOGI` 说明，不再刷 WARN。
- `inGuest` / `tgtInGuest` 保留：`tgtInGuest` 用于确认 target 落在目标库，`inGuest` 仅参考。

### logcat grep 串（固定字段顺序，勿改）

| grep 串 | 含义 | 打在哪一行 |
|---|---|---|
| `adb logcat -d -s Mod_Menu \| grep MODHOOK` | 每次 DobbyHook 的结果（成功/失败/熔断） | `Mod.cpp:655 LogHookDiag()`，调用点 438 / 442 / 451 |
| `... grep MODHIT` | 首个真正命中（每 tag 恰一行） | `Mod.cpp:835 ModHitFirst()`；打点 105 / 887 / 1017 / 1038 / 1283 |
| `... grep "hooked @rva"` | 走 inline hook 的槽（打的就是 rva） | `HookAtRva()`；`TimeScale` 在 3.2 应打出 `hooked @rva 0x37B27A8` |
| `... grep "hook TimeScale"` | 该槽走的是 rva 还是 by-name（DbgLog/文件侧同内容） | `HookSlotInstall()`（`Mod.cpp:784-796`） |
| `... grep "verprobe 3.2 cond"` | 3.2 三点判据实测值 | `Mod.cpp:240` |
| `... grep "lib version probe"` | 版本判定结论（3.2/2.4/2.2/1.8） | `DetectLibVersion()` 各分支 |
| `... grep "install pass"` | 一轮装钩汇总 ok/dead | `InstallBattleHooks()` |
| `... grep "ApplyHp argc"` | ApplyHp 的真实 argc 口径（4=不含 this / 5=含 this），只落一次 | `Mod.cpp:1188-1196`（`g_argcLog` + 类校验守卫） |

单行样例（字段顺序固定）：

```
MODHOOK tag=TimeScale target=0x7E46E487A8 orig=0x7F6E1DD000 inGuest=0 libbase=0x7D53477000 libsize=0xF415A000 tgtInGuest=1 installed=1 origExec=1
MODHOOK tag=Battle    target=0x7E4508C364 orig=0x7F6E1DD800 inGuest=0 libbase=0x7D53477000 libsize=0xF415A000 tgtInGuest=1 installed=1 origExec=1
MODHIT  tag=Battle    n=1 va=0x7E4508C364
```

（上例取自真机 96f0d883：`inGuest=0` + `origExec=1` = **正常**，`orig` 是 Dobby trampoline。）

判读规则（验收时按此读）：

1. `installed=0` ⇒ 该钩没装上（Dobby 失败 / 序言漂移 / rva 不在 RX）；
2. `installed=1` + `origExec=1` ⇒ 装钩可信（`inGuest` 0/1 都正常，orig 通常是 trampoline）；
3. `installed=1` + `origExec=0`（或 `orig=null`）⇒ 才是**假阳性**，同时落一行 `LOGW`；
4. 有 `installed=1` 但**始终无 MODHIT** ⇒ 地址不是热路径，或该方法是 AOT 直调（按名必失效，见 §2c）；
5. `tgtInGuest=0` ⇒ target 不在 libil2cpp，rva 填错了（版本不符）；
6. `verprobe 3.2 cond` 三值全 0 ⇒ 探针完全没命中，先查 libil2cpp 是否被替换/加固。

`DbgLog` 文件路径**保持原样** `/data/data/com.xf.kyzt/files/kyzt_dbg.log`（真机未 root 读不到，不影响；后续根机/模拟器仍可用）。

---

## 4. 构建

### 4.1 命令（本机 Windows，Git Bash 用 /e/nixiang）

```bash
cd /e/nixiang/mod菜单/starry-mod-menu/app/src/main/jni
mkdir -p ../../libs_kyzt/arm64-v8a            # 该目录曾被历史清理删掉，需重建（gitignore: .gitignore:52 libs*/）
NDK=/e/nixiang/01_tools/android-sdk/ndk/android-ndk-r27c
# Windows 坑：系统 ANSI 代码页 = 936，原生 GNU make 用 ACP 解释 makefile 字节，
# UTF-8 的中文源文件名会 stat 失败（make: *** No rule to make target 'Games/旷野征途Mod.cpp'）。
# 处理：生成一份 **GBK 编码的等价 mk**（内容逐字相同，仅编码不同），build 后删除。
powershell -NoProfile -Command "\$t=[IO.File]::ReadAllText('Android_kyzt.mk',[Text.Encoding]::UTF8);[IO.File]::WriteAllText('Android_kyzt_gbk.mk',\$t,[Text.Encoding]::GetEncoding(936))"
"$NDK/ndk-build.cmd" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_kyzt_gbk.mk \
  NDK_APPLICATION_MK=./Application.mk NDK_OUT=../../obj_kyzt NDK_LIBS_OUT=../../libs_kyzt -j8
rm -f Android_kyzt_gbk.mk
```

实测事实（本机）：`HOST_OS=windows / HOST_ARCH=x86_64 / OS=Windows_NT` 必须显式给（否则 make 调 `uname -s` 取不到 HOST_OS 直接 abort）；`chcp 65001` **不能**解决中文文件名（ACP 未变）。构建日志：`13_scratch/kyzt32_build/build_r27c.log`。

### 4.2 产物指纹

| 项 | 值 |
|---|---|
| 路径 | `mod菜单/starry-mod-menu/app/src/libs_kyzt/arm64-v8a/libstarry_magic.so` |
| 字节 | **239,592 B**（加速 OFF 恢复返工后） |
| sha256 | **f9b31139c8c045f476d38f0b6dec8778f5a25269b4f49d46d1c2ed95e5fdc14c** |
| md5 | `b69cd60ebd9232ea0ddea20fdc9260e5` |
| NDK | `01_tools/android-sdk/ndk/android-ndk-r27c`（27.2.12479018） |
| ELF | `ELF64` / `2's complement, little endian` / `DYN (Shared object file)` / **Machine: AArch64** |
| 导出 | `T JNI_OnLoad`（0x12830）存在 |
| 未定义符号 | 117 条（libc++_shared / liblog / libc 常规），**emutls = 0** ⇒ 无 `__emutls_get_address` 隐患 |
| `readelf -A` | **空**（`-Wl,--strip-all` 已去掉 .ARM.attributes；既有 `libs_self_modern/arm64-v8a/libstarry_magic.so` 同样为空 ⇒ 属本项目正常形态，架构以 ELF header Machine=AArch64 为准） |
| 新代码入产物证据 | so 内含字符串 `MODHOOK tag=%s target=`、`MODHIT tag=%s n=1`、`verprobe 3.2 cond`、`showInternal=0x1CFC6D0`、`battle: ApplyHp argc=%u` |

（过程记录：① 系统 SDK NDK 27.0.12077973 首构建 236,736 B / `ab7a3ab1…3643d`；② 换回工作区规范 NDK
`01_tools/...r27c` 后 236,736 B / `78fb197d…dcda9eaab`；③ 战斗合同返工 237,104 B / `afc1acd9…aea269`（`build_r27c_b3.log`）；④ TimeScale 改 inline + 诊断返工 238,872 B / `03548dc2…922f80a`（`build_r27c_b4.log`）；
⑤ 加速 OFF 恢复返工 ⇒ 上表 239,592 B / `f9b31139…e5fdc14c`（`build_r27c_b6.log`）。以上均全量 clean 重编。）

---

## 5. 门禁输出

### 5.1 `python3 04_scripts/旷野征途/check_battle_contract.py` → exit 0

```
PASS param 索引常量
PASS 槽位常量
PASS 调用 get_param 用常量
PASS 读槽用常量
PASS 无裸索引残留
PASS player backing field
PASS Instance 静态字段
PASS hook 用 file offset
PASS 审计/特权未调用
PASS 3.2 argBase 五槽
PASS 3.2 argc 双口径放行
PASS argc 无字面量硬编码
PASS 3.2 ApplyHp 合同锚点
PASS 2.4 历史合同保留
PASS 落血语义注释
PASS FIXTURE x100/clamp boundary (8 cases)
结论：一致
```

### 5.2 `python3 04_scripts/旷野征途/check_menu_map_kyzt.py` → exit 0

```
PASS featureList 长度 = 9（实际 9）
PASS Java 以数组下标作 featNum 传给 native
PASS Java 前缀解析规则（Category_/Toggle_/Button_）齐备
PASS Category 只占 0/4（实际 [0, 4]）
PASS 功能位下标 = 1/2/3/5/6/7/8（实际 [1, 2, 3, 5, 6, 7, 8]）
PASS native enum 与功能位一致（enum=[1, 2, 3, 5, 6, 7, 8]）
PASS index 1/2/3 类型 Button_ ；index 5/6/7/8 类型 Toggle_
PASS 「待验证」集合 = {1,2,3,6,7}
PASS 不再有「未适配」项
PASS 5/8 不带适配标签
结论： 一致
```

### 5.3 `bash 04_scripts/_common/preflight.sh <改过的文件>` → exit 0

传参：`Games/旷野征途Mod.cpp` + `10_docs/旷野征途/施工_native_3.2_20261124.md`
（上一轮还传了 `check_battle_contract.py`，当时汇总 PASS=5、`[python语法]` PASS；本轮脚本未再改，见 §5.1）

```
✓ PASS  [编号单义] 无 07_tmp/ 或 07_tools/ 遗留目录
⚠ REMIND [C++/mk] ...旷野征途Mod.cpp —— 无独立语法检查器：该改动必须以重跑管线构建阶段验证
✓ PASS  [跳过] ...施工_native_3.2_20261124.md（md 只对入口文件查体量）
✓ PASS  [框架隔离] 框架层无单游戏逻辑
汇总: PASS=4  FAIL=0  REMIND=1  SKIP=0
✅ 机器关通过。
```

REMIND 已由本轮的 ndk-build 一次通过（§4）覆盖。
环境备注：本机 bash 的 PATH 里**没有 `python3`**（只有 `D:/Python313/python.exe` 与 Python311 的 python.exe），
首次跑 preflight 会在 `[python语法]` 项报 `python3: command not found`（假 FAIL）。做法：临时目录
`13_scratch/bin/python3`（两行 shim：`exec D:/Python313/python.exe "$@"`）前置到 PATH 再跑；
**未改** `04_scripts/_common/**`。这是本机环境缺口，不是脚本缺陷。

---

## 6. 未证项（明确失败关闭）

0a. **OFF 恢复是否真的落地未证**：需在真机 ×1 次「开→关」看到 `restore timeScale=1.0 (toggle off) spd=0`，
    且此后 `Time.timeScale` 不再被 1.8 覆盖（观感上加速立即消失，不再等 5 s）。
0. **TimeScale inline hook 是否真的生效未证**：需在真机看到
   `TimeScale hooked @rva 0x37B27A8` + `MODHIT tag=TimeScale n=1` + `set_timeScale call#N`；
   若仍只有 by-name 而无 MODHIT，说明 rva 没走通（查 `hook TimeScale: path=` 与序言 drift 日志）。

1. **运行期探针未验**：`verprobe 3.2 cond` 三值与 `verHook` 取值只在真机 logcat 才能确认；本轮只证静态判据与构建。
2. **Dobby inline hook 在真机 arm64 原生可用性未证**：需看 `MODHOOK ... inGuest=1` + 对应 `MODHIT`。若 inGuest=1 但无 MODHIT ⇒ 地址非热路径。
3. **战斗 IL 合同：静态已复核（2026-11-24 返工），运行期仍未证**——
   `ApplyHp` 3.2 实测：rva 0x0FF20C / body 1594 B / Param 行 4（atker, atkSeatNum, dmgInfo, OnDie）/
   槽位 +0/+8/+16/+24/+32 不变 / `intDmg` 由 `dmg` 派生（见 §2b）。
   **仍属 NOT PROVEN** 的部分：`argc` 在真机 il2cpp 的实际返回值（看 `battle: ApplyHp argc=%u`）、
   `BattleRoleActor` 的运行时字段偏移（`_curHp` 等按名现取，未在 3.2 上取得运行值）、
   无敌/百倍的实际效果。`check_battle_contract.py` 只证明「源码常量与 3.2 反推值一致」，不证明功能生效。
4. **经济三项**：`Economy22.h` 全程 `il2cpp_field_get_offset` 按名现取，3.2 侧名字齐备性未验证；且 3.2 AOT 里有同名 `TapSDK.OnlineBattle.PlayerInfo` ⇒ 按名查找必须带命名空间（现有实现用 `FindClassNs`，风险已规避，但仍需运行期证据）。
5. **免广告按名三要素在 3.2 的命中率未证**：`Dirichlet.Ad.Runtime.dll` image 名与类名未变是静态推断，需 `MODHIT tag=NoAd` 确认。
6. **未做**：装机、上机、组包 APK、`adb` 写操作、`git add/commit/push`。

---

## 7. 复现与证据路径

| 路径 | 内容 |
|---|---|
| `13_scratch/kyzt32_build/build_r27c_b6.log` | 本轮最终 clean 重编日志（加速 OFF 恢复 + 100ms force 节流） |
| `13_scratch/kyzt32_build/build_r27c_b4.log` | 上一轮 clean 重编日志（TimeScale inline hook + MODHOOK 判据修订） |
| `13_scratch/kyzt32_build/build_r27c_b3.log` | 上一轮 clean 重编日志（战斗合同返工） |
| `13_scratch/kyzt32_build/build_r27c_b2.log` | 同轮首次返工构建（argc 日志未收窄，已被 b3 取代） |
| `13_scratch/kyzt32_native/hd_verify3_applyhp32.py` / `verify_applyhp32.txt` | ApplyHp 参数表 / ldarg 首用 / 字段 token / get_intDmg 的复核脚本与输出（可复跑） |
| `13_scratch/bin/python3` | 本机 bash 的 python3 shim（仅供 preflight 语法检查用，非仓库文件） |
| `13_scratch/kyzt32_build/build_r27c.log` | 上一轮构建日志（战斗合同返工前） |
| `13_scratch/kyzt32_build/build3.log` / `build5.log` | 中文文件名失败的原始报错（`No rule to make target`，留证说明为何要 GBK 化 mk） |
| `13_scratch/kyzt32_build/build6.log` | 系统 SDK NDK 27.0 的 GBK 成功构建记录 |
| `13_scratch/kyzt32_build/dis.txt` | 产物反汇编（llvm-objdump，供后续核对） |
| `13_scratch/kyzt32_build/to_gbk.py` | 早期 UTF-8→GBK 转换脚本（最终改用 PowerShell 一行，仓库内无需 python） |

复跑一句（Git Bash）：

```bash
cd /e/nixiang/mod菜单/starry-mod-menu/app/src/main/jni && powershell -NoProfile -Command "\$t=[IO.File]::ReadAllText('Android_kyzt.mk',[Text.Encoding]::UTF8);[IO.File]::WriteAllText('Android_kyzt_gbk.mk',\$t,[Text.Encoding]::GetEncoding(936))" && HOST_OS=windows HOST_ARCH=x86_64 /e/nixiang/01_tools/android-sdk/ndk/android-ndk-r27c/ndk-build.cmd NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_kyzt_gbk.mk NDK_APPLICATION_MK=./Application.mk NDK_OUT=../../obj_kyzt NDK_LIBS_OUT=../../libs_kyzt -j8; rm -f Android_kyzt_gbk.mk
```
