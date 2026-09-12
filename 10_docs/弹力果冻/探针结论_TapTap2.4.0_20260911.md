# 弹力果冻 TapTap 2.4.0 — 偏移探针结论（2026-09-11 实测）

> 承接 `跳板重定位_TapTap2.4.0_20260911.md`（静态定位）与本文件（**运行时实测**）。
> 样本：MuMu 实装官方 TapTap 包 `com.ginicat.sweet` vc4409/2.4.0 → 装探针包实测。
> 标记：**PROVEN** = 本次实测/反汇编直接得到；**推断** = 证据支持但未直接验证；**FAILED** = 已验证不可用；**待验证** = 未验证。

---

## 0. 一句话结论

1. **热更类与方法可在运行时按名解析**（`BattleServer` 等命中热更 image）→ **不需要硬编码任何偏移**。
2. 热更方法的 `MethodInfo.methodPointer` 指向**同签名桥**（桥 ABI：`x0=参数/this`、`x1=参数`、`x2=MethodInfo`，`bl 0xB95790` 公共 invoke 入口）。
3. **桥可 hook，但必须"按 MethodInfo 名分发 + 必调 orig"**（工作区既有正确挂法，见 §5）。
4. 旧 4399 挂点 `0xBAAB0C` **落在解释器主体内部**（`[0xBA3CE8,0xBAB884)`）= 共享入口 → **换版本不可复用（FAILED）**，且正是旧版三功能互撞/闪退的机制来源（见 §4）。
5. **不是所有热更方法都能挂**：如 `GameCalculator.DamageCalculator` 的 methodPointer 是 `NotSupportNative2Managed` 占位桩（见 §3.3）。

---

## 1. 样本与产物（PROVEN）

| 项 | 值 |
|---|---|
| 设备 | MuMu `127.0.0.1:16384`，HUAWEI 伪装属性 |
| 包 | `com.ginicat.sweet` vc4409 / 2.4.0 |
| 官方底包 | `02_apks/弹力果冻/弹力果冻_2.4.0_taptap_official.apk`，2,128,558,103 B，md5 `634b37691f40f94d05b77e0702604fec` |
| 探针包 | `08_output/弹力果冻/弹力果冻_2.4.0_taptap_probe-releaseSigned.apk`，md5 `ba57fd62ef5e61db53ccf66e23bc80d6` |
| 装机校验 | 设备 base.apk md5 == 本地产物 ✓（`04_scripts/_common/verify_installed_apk.sh`） |
| 探针 so | `libs_tlgd/arm64-v8a/libstarry_magic.so`，md5 `ac915aa05a4fe14a494674b889c9f959`；包内 so md5 == 构建产物 ✓ |
| 静态样本 | `06_dumped/弹力果冻/taptap_2.4.0/libil2cpp.so` 58,291,656 B md5 `c067234d6526cd247691598e44aa5825`；`global-metadata.dat` 9,983,256 B md5 `c95d2ef936d70498b4442a2e8f058d27` |

**运行时热更并非 APK 内置那份**（重要）：游戏启动时从 CDN 拉
`http://gini.oss-cn-hangzhou.aliyuncs.com/dll-android/4880/hotfix.bytes.zip`
→ 解出 **`Assembly-CSharp.dll`（9,990,656 B，2026-09-09）**，这才是实际执行的玩法程序集。
APK 内置 `assets/AOTDLL/hotfix.bytes`（9,534,976 B）仅作静态参考。**类/方法归属以 4880 为准**（宿主机 `ilspycmd` 核对）。

---

## 2. 探针实测结果（第 1 次尝试即命中，无崩溃）

```
[TLGDP] mod 线程已 attach 到 il2cpp domain
[TLGDP] 尝试#1 程序集数=96
[TLGDP]   命中类 BattleServer=0x7521b51420   @img[95] class_count=11363
[TLGDP]   命中类 GameCalculator=0x7511a96a10 @img[95] class_count=11363
[TLGDP]   命中类 BattleUtility=0x7511ad0610  @img[95] class_count=11363
[TLGDP]   M BattleServer.HpHandle          argc=1 mi=0x75f2eabf18 methodPointer=0x74079ac8a0 file_off=0xB8F8A0
[TLGDP]   M BattleServer.CreateDamage      argc=1 mi=0x75f2eabea8 methodPointer=0x74079ac8a0 file_off=0xB8F8A0
[TLGDP]   M GameCalculator.DamageCalculator argc=1 mi=0x75f2eabb50 methodPointer=0x74079b2220 file_off=0xB95220
[TLGDP]   M BattleUtility.KillAllEnemy     argc=0 mi=0x75f2ec0c78 methodPointer=0x74079a9a30 file_off=0xB8CA30
[TLGDP] VERDICT distinct=3 shared=0 lib_base=0x72C61CD000 hit_img=95 assembles=96
```

结论与解读：

- **热更 image = 索引 95（96 个里的最后一个）**，`class_count=11363`（其余 AOT image 最多 1860）。
  热更程序集**最后加载 → 排在枚举尾部**，这点决定了遍历不能截断（见 §6.3）。
- **`HpHandle` 与 `CreateDamage` 共享同一 methodPointer（0xB8F8A0）**；两者签名完全相同（实例 / 1 参 / void）。
  → **按地址 Dobby 会一网打尽同签名所有方法**，不能当"单方法挂点"。
- `distinct=3`（4 个方法 3 个不同地址）：既非"全共享"也非"每方法唯一"，说明桥是按**签名形状**生成的。
- **元数据里 118 个 image / 约 95 个 assembly，运行时枚举到 96** → 差值即热更程序集。

---

## 3. 桥与入口的反汇编（PROVEN）

### 3.1 2 参实例 void 桥 @ file_off `0xB8F8A0`（`HpHandle` / `CreateDamage` 的落点）

```asm
b8f8a0: sub  sp, sp, #0x20
b8f8a4: stp  x29, x30, [sp, #0x10]
b8f8a8: add  x29, sp, #0x10
b8f8ac: stp  x0, x1, [sp]        ; 把 x0/x1 两个入参放到栈上，形成参数数组
b8f8b0: mov  x1, sp              ; x1 = 参数数组指针
b8f8b4: mov  x0, x2              ; x0 = MethodInfo   ← 调用方在 x2 传入（与 hzsj 注释 "mi=x2" 一致）
b8f8b8: mov  x2, xzr
b8f8bc: bl   0xb95790            ; → 公共 invoke 入口
b8f8c0: ldp  x29, x30, [sp, #0x10]
b8f8c4: add  sp, sp, #0x20
b8f8c8: ret
```

### 3.2 0 参静态 void 桥 @ `0xB8CA30`（`KillAllEnemy` 的落点）

同形状，仅参数处理不同（`add x1, sp, #0x8`；无需搬 x0）：

```asm
b8ca30: sub  sp, sp, #0x20 ; stp x29,x30 ; add x29,sp,#0x10
b8ca3c: add  x1, sp, #0x8
b8ca40: mov  x2, xzr
b8ca44: str  xzr, [sp, #0x8]
b8ca48: bl   0xb95790      ; 同一公共 invoke 入口
b8ca4c: ldp  x29,x30 ; add sp,sp,#0x20 ; ret
```

### 3.3 `0xB95220` **不是桥**，是"不支持 native 调用"的占位桩（FAILED 作为挂点）

`GameCalculator.DamageCalculator` 的 methodPointer 指向的代码加载字符串 **`"NotSupportNative2Managed"`**（相邻还有 `"NotSupportAdjustorThunk"`）：

```asm
b95220: stp x29,x30,[sp,#-0x10]! ; mov x29,sp
b95228: adrp x0, 0x2b84000 ; add x0,x0,#0xff   → 0x2b840ff = "NotSupportNative2Managed"
b95230: bl  0xadd578
b95234: mov x1, xzr
b95238: bl  0xadadc4
b9523c: (下一个函数开始)
```

→ 该方法是 HybridCLR 判定"**不可从 native 直接调用**"的方法（无调整 thunk）。**挂点候选必须逐个核对是不是这种桩**，不能假定"能按名解析 = 能挂"。

### 3.4 公共 invoke 入口 `0xB95790`

所有桥都 `bl` 到这里；入口自身把 **x2 当 MethodInfo**（`mov x20, x2`）、x1 当参数数组（`mov x19, x1`），分配 0x1410 栈帧。与 hzsj 记录的 `BRIDGE_FILE_EXECUTE` 同角色。

---

## 4. 与旧 4399 挂点 `0xBAAB0C` 的关系（FAILED，不可复用）

- `0xBAAB0C` 落在 `.eh_frame` FDE **`[0xBA3CE8, 0xBAB884)`（31KB 单函数）** 内部，**不是** §3 那些桥所在的 `0xB8Cxxx–0xB95xxx` 区域。
- 该函数的三条独立证据（见静态定位文档）：引用 `"hybridclr doesn't support %s"` 页（10 处 `adrp`）、`smaddl …#0xc` + `strh [x25,#2/4/6]`（`StackObject` 求值栈 12 字节步长）、入口按解释器 ABI 取参 → **解释器主体**。
- 因此 hook `0xBAAB0C` = hook **所有解释执行方法的公共路径**。这与旧文档自述"四功能**共用一条 HybridCLR 跳板**；开关不增删 patch"完全吻合 —— 也正好解释了旧 4399 版**三功能互相干扰 + 频繁闪退**：多份 patch 叠在同一条公共路径上，按方法名过滤不做、重入不管、orig 契约不保。
- **结论**：换渠道/换版本后该偏移既**不可静态复用**，机制上也**不应复用**。框架骨架注释"绝不 hook 共享 invoke 入口"即此意。

---

## 5. 正确挂法（工作区已有先例，非本次发明）

`skill/re-extract/hybridclr-runtime.md` §四「公共 invoke 跳板法」：

```
DobbyHook(trampoline_addr, replacement, &orig);
// replacement 内：
//   1. method_get_name(method) 取方法名
//   2. strcmp 判断是否目标方法
//   3. 命中才读/改参数
//   4. 必须调 orig（不调会崩）
```

已跑通的 **魂坠深境**（`Games/HzsjMod.cpp`）就是这条路：`methodPointer 槽（探针 fileoff；2 参 void 桥，mi=x2）` +
`BRIDGE_FILE_EXECUTE`，并专门留了"methodPointer 重复聚集 → 公共 invoke 桥 RVA"的探针注释。

**弹力果冻的落地形态（关键：不需要硬编码偏移）**：

1. `Assemblies()` → `ImageOf` → `FindClassInImage(img, "BattleServer")`；`Assemblies()` 为空 = il2cpp 未就绪（见 §6.2）。
2. `GetMethod(cls, "HpHandle", 1)` → `mi`；`mp = *(void**)mi`。
3. **核对 `mp` 落点**（file_off 是否在 `0xB8Cxxx–0xB95xxx` 桥区、且不是 §3.3 的桩）——不要在运行时盲信。
4. `DobbyHook(mp, Hook, &g_orig)`；`Hook` 内以 `method_get_name(mi)`（mi 即 x2）区分目标，非目标一律直通 `g_orig`。
5. 同签名多方法共用桥 ⇒ **分发是必需的，不是优化**；同时意味着 hook 常驻于该签名高频路径，`tick` 类重活不得放在 Hook 内。

---

## 6. 本次踩坑（**框架用法/时序**，建议作为经验候选）

### 6.1 注册表时序字段名带 `Ms`，**值却是微秒**（PROVEN）

`MainModern.cpp:229` 直接 `usleep(mod->initDelayMs)`。字段名 `initDelayMs`，而 `usleep` 收**微秒**：

| 字段 | 我最初填 | 实际含义 | 正确填法（参照魂坠深境） |
|---|---|---|---|
| `initDelayMs` | `15000` → **15 毫秒** | 微秒 | `15000000` = 15 秒（hzsj 用 `3000000` = 3 秒） |
| `initRetryIntervalMs` | `5000` → 5 毫秒 | 微秒 | `5000000` = 5 秒 |
| `tickIntervalMs` | `200000` ✓ | 微秒 | `200000` = 200 毫秒（默认值即 200000） |

### 6.2 il2cpp 会**提前发布 domain**，据此 attach 会在 libunity.so 段错误（PROVEN，实测复现）

- 现象：`IL2CPP: JNI_OnLoad` 后 ~0.2s，探针 `thread_attach(domain_get())` → `SIGSEGV` in libunity.so
  → `Abort message: 'exiting due to SIG_DFL handler for signal 11'`（ART `SignalChain` 判定无人接管后 `abort()`），**进程猝死，Process uptime 0s**，Unity 自己的 `MemoryManager` 日志还晚 80ms 才出现。
- 因此 **`domain != null` 不是就绪判据**。改用更晚的里程碑：**`Assemblies()` 返回非空**（纯元数据只读、无需 attach；模板亦如此调用）。
- 补充：`Il2Cpp::SafeProbe` 的 `sigsetjmp`+`sigaction` 兜底**没能兜住**这次崩溃（生效 handler 是 ART `SignalChain`）。不要把它当作 mid-init 崩溃的保险。

### 6.3 遍历 image 不能截断（PROVEN）

初版写 `for (i = 0; i < assmCount && i < 64; i++)`，而**热更 image 索引 95**（最后加载）→ 永远搜不到，
每 5 秒误报"热更类未解析"，白等 20 分钟窗口。热更程序集一定在枚举**尾部**，必须全量遍历。

### 6.4 目标方法的**归属类**必须核对（PROVEN）

原表把 4 个方法都当成 `BattleServer` 的成员，实际（4880 `ilspycmd` 核对）：

| 方法 | 归属 | 签名 |
|---|---|---|
| `CreateDamage` | `BattleServer`（全局命名空间） | 实例，1 参，void |
| `HpHandle` | `BattleServer` | 实例，1 参，void |
| `DamageCalculator` | `GameCalculator` | 静态，1 参（≠ BattleServer） |
| `KillAllEnemy` | `BattleUtility`（`public static class`） | 静态，0 参 |

`GetMethod` 用错宿主类只会静默返回 null，不会报错 —— 必须逐个核对。

### 6.5 坐标系陷阱：本包**两段 p_offset 都是 0**（PROVEN）

`/proc/<pid>/maps` 里 `libil2cpp.so` 三段：

```
72c61cd000-72c9965000 r--p 00000000   ← 段 A（r--），off=0
7406e1d000-740a1cb000 r-xp 00000000   ← 段 B（r-x，代码），off=0  ← 指针/桥都在这段
740a1da000-740a5c4000 rw-p 033ad000
```

- 探针日志的 `lib_base=0x72C61CD000` 是**段 A** 的基址；而 `HpHandle` 的指针在**段 B**（base `0x7406E1D000`）。
- 因此 **`runtime = lib_base + file_off` 在本包不成立**（会差一个段A大小）。
  验证：`0x74079AC8A0 − 0x7406E1D000 = 0xB8F8A0` = 探针报的 `file_off` ✓（即 **`file_off == VA` 只对 r-x 段成立**）。
- 正确做法：**hook 直接用 `methodPointer` 的运行时地址**；`file_off` 只用于与静态反汇编比对。

---

## 7. 后续功能实现机制（S5 待办，基于以上实测）

> 原则：**能改内存就不 hook；要 hook 只挂桥并按 mi 分发；绝不碰 0xBAAB0C / 0xB95790。**

| 功能 | 首选机制 | 目标/依据 | 风险 |
|---|---|---|---|
| 无敌 | `BuffKind.Invincible` 判定位 or `HpHandle` 桥内改参 | `BattleServer.HpHandle`（桥 `0xB8F8A0`）；hotfix 里 `HasBuffKind(BuffKind.Invincible)` 分支 | 与游戏原生无敌同源，可被服务端状态覆盖 |
| 秒杀 | `HpHandle` 桥内把伤害抬到上限 / `BattleUtility.KillAllEnemy` | `KillAllEnemy`（桥 `0xB8CA30`，0 参静态最好挂） | 上报面（伤害统计）见复核文档 |
| 倍率伤害 | `CreateDamage` 桥内改 `DamageInfo` 字段（需字段偏移） | 与 `HpHandle` **共用桥**，分发必须按名 | 改的是 Obscured 值会解错（见复核文档 R-9/ACTk） |
| 无限技能 | `ReSetSkillCD/ReCalSkillCD/UnResetSkillCD`（另一类） | 需补探针定位归属类 | 未实测 |
| 移速/资源/货币 | 字段直接读写（`FieldOffset` + 对象指针） | 无需 hook，最稳 | 需先定位字段与实例获取路径 |
| 免广告 | Java 层/SDK 回调（非 il2cpp） | 广告桥 `bridgeClass.*AdUtil`（启动日志里可见 TikTokAdUtil 缺失异常） | 与玩法无关 |
| 内购 | TapTap/支付回调伪装 | 高风控，且与 `SignSpoof`（本包官服条目已加）耦合 | 建议先不做 |

**探针扩展（下一轮）**：把 `ReSetSkillCD` 等技能类、玩家/货币持有类的归属与字段偏移一并探出，并把"桥是否落在 §3.3 桩区"做成自动判定。

---

## 8. 复现（机械操作只走管线）

```bash
bash 04_scripts/pipeline.sh 弹力果冻 status
bash 04_scripts/pipeline.sh 弹力果冻 next        # check_apk→dump→anchor→impl_mod→build_so→surgical→sign
bash 04_scripts/pipeline.sh 弹力果冻 run device_ready
bash 04_scripts/pipeline.sh 弹力果冻 run install   # push+（异签名时）卸载+pm install+装机指纹校验
# 采集（探针 tryInit 在库命中后 15 秒首发，此后每 5 秒重试）
adb -s 127.0.0.1:16384 logcat -c
adb -s 127.0.0.1:16384 shell am start -n com.ginicat.sweet/main.GameActivity
adb -s 127.0.0.1:16384 logcat -d -s Mod_Menu:V | grep -E "TLGDP"
```

关键约束：`env.SERIAL=127.0.0.1:16384`；gradle 需写 `~/.gradle`（工作区之外）；外科包会把官包逐字节保留 `classes.dex`/`resources.arsc`。

---

## 9. 证据分级

**PROVEN（本次实测/反汇编）**
- 热更类按名解析可行；热更 image = 索引 95/96，class_count=11363
- `HpHandle`/`CreateDamage` 共享 methodPointer `0xB8F8A0`；`KillAllEnemy`→`0xB8CA30`；`DamageCalculator`→`0xB95220` 为 `NotSupportNative2Managed` 桩
- 桥 ABI（x0=参数、x1=参数、x2=MethodInfo）与公共 invoke `0xB95790`
- `0xBAAB0C ∈ 解释器 [0xBA3CE8,0xBAB884)`；两段 p_offset=0；`file_off==VA` 仅 r-x 段
- 提前 attach → libunity.so SIGSEGV → abort（可复现）；`initDelayMs` 按微秒解析

**推断**
- `0xB95790` 为 HybridCLR 公共 invoke 入口/HybridCLR 解释器调度（与 hzsj 同角色；未在 HybridCLR 源码级确认）
- 桥按"签名形状"生成（由 2 个同签名方法共享 + 另两种签名各不同推断）

**FAILED / 不可用**
- `0xBAAB0C` 作为挂点（共享解释器路径，机制与版本双重不可用）
- `0xB95220` 作为挂点（不支持 native 调用的占位桩）

**待验证**
- 各功能字段偏移（HP/货币/移速/技能 CD）与实例获取路径
- 技能类（`ReSetSkillCD` 等）归属与桥落点
- 桥被 hook 后同签名所有方法的性能/重入影响实测
