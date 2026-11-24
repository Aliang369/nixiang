# 旷野征途 3.2 / vc32 —— 环境可行性探测：Houdini 翻译层下「改游戏代码页」能否走通

> 阶段 0.5 子任务（env-probe），2026-11-24 15:49 起。全部设备动作**只读**（唯一写侧＝启动进程），未 screencap / 未 input tap / 未 force-stop。
> 设备：雷电 LDPlayer14 emulator-5554（SGT-AL10 / Android 14 / x86_64 + libhoudini / root）。真机 96f0d883 未触碰。
> 底包指纹：libil2cpp.so 66,268,112 B sha256 b249a131…332b；global-metadata.dat 11,782,304 B sha256 9a106895…8ddd（v31）。

---

## 1. 问题（唯一待答）

在雷电 Houdini 翻译环境下，对本仓星光框架（arm64 libstarry_magic.so + Dobby inline hook）而言：

**「改游戏代码页」这条路可行吗？若不可行，剩下哪些形态仍可行？**

二分拆解：
- **A**：libil2cpp.so 的代码段在内存里是不是可执行的原始 arm64 指令？
- **B**：若是翻译执行，则「直接写原始字节的 inline hook」能不能生效？即使 mprotect 成功？

---

## 2. 仓内既有经验（先查，避免重复试错）

检索：rg -n -i "houdini" 10_docs skill 04_scripts（约 60 行命中）+ 逐条回溯原文。**仓内结论不一致，必须分开记**：

### 2.1 已被反例否掉的强断言：「r--p ⇒ 无 hook / 不可能」

| 出处 | 结论（原文要点） | 类型 |
|---|---|---|
| 10_docs/_state/games/999秒幸存者.md:33 | 「雷电 houdini 不能做 RVA inline hook：libil2cpp 全段 r--，Dobby 装钩 9/9 全失败（not in RX）。必须用 arm64 真机」 | 实测，但**只覆盖 RVA/file-offset 路径**（该包无 by-name 兜底） |
| 10_docs/_state/games/敢达争锋对决.md:28 | 「**铁律**：guest 代码段在 maps 里全是 r--p（houdini 不标 X）⇒『无 r-xp』≠『无 hook』；补丁只在 guest 别名，host 别名与磁盘逐字节相同」 | 实测，反向否掉「无 X 即无 hook」 |
| 10_docs/_state/games/我本布衣世家录.md:26 | 「Houdini 把 libil2cpp 映射为 r--p，VaInExecLib 恒假 ⇒ 全表走 @name 兜底；实测 **24/24 hooked @name、OnLoginSuccess 与 ShowRewardAd intercepted 真实生效**（真机仍走 @rva）」 | 实测，**同机 emulator-5554** |
| 10_docs/_state/games/蛋壳战机.md:23 | 「首轮 12 hook not in RX — try by-name…40s 后重试 **12/12 全部 hooked by-name**（r-xp=0 是 houdini 特性，**Dobby 在转译下有效**）」；L3 四项 PASS | 实测，同机 |
| 10_docs/阿比斯少年冒险团/09_v1.5设计_血量0x198.md:591 | 「**Dobby inline hook 在 houdini 转译层下可用** — dmgx payload-copy 23 条（hook 被真实伤害结算调用 23 次）」 | 实测，同机 |

### 2.2 已实测的强风险：装钩成功＝假阳性，且可能直接崩

| 出处 | 结论 |
|---|---|
| 10_docs/无限升级/雷电转译层闪退定案_20261026.md:8,28,42 | DobbyHook 报装成功（base=0x400023600000 +0x2b890d8，orig=0x7d147e0ba000）但**整轮零命中**；进程 SIGSEGV，rip 恒 libhoudini.so+0x30c02a、线程 dTi；**A/B/A 闭环**（去掉该 so 存活 22 s+，恢复再崩）。原文推论：hook 改动了 houdini 管理的只读代码页，且 orig=0x7d147e0ba000 落在 **x86 用户空间**、不在 houdini 的 ARM 模块地址空间（0x4000…）⇒ 首次翻译解析跳转目标失败 |
| 10_docs/_state/games/绝区零.md:172–173 | 最小钩子集（只 2 钩）仍崩，fault 0xdead1007，崩栈 4 帧全在 libhoudini.so；另有 TenSafe 线程（libtersafe2）⇒「雷电 houdini + TenSafe 下 inline hook 不可用」 |
| 10_docs/阿比斯少年冒险团/06_装机记录.md:409–420,549 | maps 原文：ARM so 全段 r--p、**只有 libhoudini.so 有 r-xp**；「sym_reject … 不在可执行段 判断在该环境恒为真」；同时 §12.5 反过来说「houdini **可以**从我们 so 调到 libil2cpp 的函数」（放宽落点校验后 8 符号全过） |

### 2.3 框架现状（代码现状，不是环境事实）

- 模板 TryHookRva 有 by-name 兜底：日志 MOD … rva 0x%lX not in RX — try by-name（Games/AyxsMod.cpp:348、BwxrkMod.cpp:366、Byyxx2Mod.cpp:285 等 10 余处同构）。
- Dobby 直调点：Games/*Mod.cpp 的 DobbyHook(...)（如 AbssnmxtMod.cpp:2061）。
- ⇒ 框架在 houdini 上**会自动降级到 by-name**；「装钩 9/9 失败」那类记录属于**没有兜底的旧挂点形态**。

---

## 3. 本次实测原始证据

### 3.1 设备与进程

~~~text
$ adb devices -l
96f0d883  device … M2102K1AC   /   emulator-5554  device … SGT_AL10 (graceltexx)
$ adb -s emulator-5554 shell am start -n com.xf.kyzt/com.xiefeng.game2d.PrivacyActivity
Starting: Intent { cmp=com.xf.kyzt/com.xiefeng.game2d.PrivacyActivity }
$ adb -s emulator-5554 shell pidof com.xf.kyzt
3580        # 10 s 后复核仍为 3580（稳定；停在协议页不影响 libil2cpp 已加载）
~~~

### 3.2 getprop（完整）

~~~text
[ro.dalvik.vm.native.bridge]: [libhoudini.so]
[ro.dalvik.vm.isa.arm64]: [x86_64]         [ro.dalvik.vm.isa.arm]: [x86]
[ro.enable.native.bridge.exec]: [1]        [ro.enable.native.bridge.exec64]: [1]
[ro.vendor.enable.native.bridge.exec]: [1] [ro.vendor.enable.native.bridge.exec64]: [1]
[ro.product.cpu.abi]: [x86_64]             [ro.product.cpu.abilist64]: [x86_64,arm64-v8a]
~~~

### 3.3 /proc/3580/maps（关键段，原样）

~~~text
400023200000-400024994000 r--p 00000000 … /data/app/~~SDM__…/lib/arm64/libil2cpp.so
400024997000-400026c92000 r--p 01793000 … libil2cpp.so     ← ELF ph2（X 段）落在这里
400026c95000-400026f02000 r--p 03a8d000 … libil2cpp.so     ← RelRO
400026f05000-40002713f000 rw-p 03cf9000 … libil2cpp.so     ← data
400020800000-400020d48000 r--p 00000000 … lib/arm64/libunity.so   （4 段，同样无 x）
400060000000-400060126000 r-xp 00000000 … /system/vendor/lib64/arm64/libtcb.so   ← TenSafe
7823f951e000-7823f95c8000 r-xp 00000000 … /system/vendor/lib64/libhoudini.so     ← 翻译器（3 段 r-xp）
7823eae00000-7823eee00000 rw-s 00000000 00:01 306 /memfd:jit-cache (deleted)      ← 64 MB
78223a8c5000-78223e7f8000 r--p 00000000 … lib/arm64/libil2cpp.so                 ← host 别名（同文件第二套映射）
~~~

全进程段统计（awk 按第二列含 x）：316× r-xp / 2× rwxp / 2× r-xs / 1× --xp(vsyscall)；但 **316 个 r-xp 里没有一个是本包的 arm64 so**（ARM 侧唯一的 x 段是 libtcb.so 与 libhoudini.so；两个 rwxp 均为匿名段：400060126000 4 KB 与 40006014b000-4000691de000 ≈ 8.7 MB）。

### 3.4 ELF 程序头（本地 13_scratch/kyzt32_base/lib/arm64-v8a/libil2cpp.so 解析输出）

~~~text
ph1 PT_LOAD flags=R    off=0x0        vaddr=0x0        filesz=0x1793714
ph2 PT_LOAD flags=R+X  off=0x1793714  vaddr=0x1797714  filesz=0x22f9b0c   ← 唯一可执行段
ph3 PT_LOAD flags=R+W  off=0x3a8d220  vaddr=0x3a95220  filesz=0x26cc08
ph4 PT_LOAD flags=R+W  off=0x3cf9e28  vaddr=0x3d05e28  filesz=0x238420
~~~

### 3.5 va − file 核对（0x4000 关系是否仍成立）

- ph2：vaddr − off = 0x1797714 − 0x1793714 = **0x4000** ✓
- maps：0x400024997000 − 0x1793000 = 0x400023204000 = guest base(0x400023200000) + 0x4000 ✓
- ⇒ **guest base = 0x400023200000**（本包；敢达那轮是 0x400023600000），RVA = va − base = file_off + 0x4000。仓内「RVA−Offset=0x4000」规则在翻译环境下**算术上依然成立**——成立的是算术，不是可执行性。

### 3.6 smaps / VmFlags（决定 mprotect 会不会失败）

~~~text
400024997000-400026c92000 r--p 01793000 … libil2cpp.so
Shared_Clean: 1064 kB   Private_Clean: 5296 kB   Private_Dirty: 0 kB   Anonymous: 0 kB
VmFlags: rd mr mw me        ← 无 ex（当前不可执行），但有 mw / me（允许 mprotect 改成 W / X）
~~~

⇒ mprotect 到 RW/RX **不会**被内核拒绝，Dobby 的写页流程在本机不会报错。这就是「装钩成功」日志在 houdini 上不可信的原因。

### 3.7 map_files（同一 so 两套映射）

~~~text
lr-------- 400023200000-400024994000 -> …/lib/arm64/libil2cpp.so
lr-------- 400024997000-400026c92000 -> …/lib/arm64/libil2cpp.so
lr-------- 400026c95000-400026f02000 -> …/lib/arm64/libil2cpp.so
lr-------- 400026f05000-40002713f000 -> …/lib/arm64/libil2cpp.so
lr-------- 78223a8c5000-78223e7f8000 -> …/lib/arm64/libil2cpp.so   ← host 别名（x86 侧同一文件）
lr-------- 7823f951e000-…            -> /system/vendor/lib64/libhoudini.so（5 条）
~~~

guest 4 段 + host 别名 1 段 = 同一文件被映射两次：0x4000…（houdini 的 guest 地址空间）与 0x7822…（宿主 x86 地址空间）。

### 3.8 内存里的字节到底是什么（决定性）

入口与 6 处采样，**guest 内存 vs 磁盘文件逐字节对照**：

~~~text
guest 400024997714: 5f24 03d5 e017 01d0 0080 0891 68dd 8b14   (NOP / ADRP x0 / ADD x0,x0,#0x220 / B …)
disk  01793714:     5f24 03d5 e017 01d0 0080 0891 68dd 8b14
guest 400024997724: 5f24 03d5 c003 5fd6 …                    (NOP / RET)
disk  同位置:        5f24 03d5 c003 5fd6 …
—— 6/6 采样点各 32 B 全部相同（off 0x1793714 / 0x1a00000 / 0x2000000 / 0x2800000 / 0x3000000 / 0x3a00000），不同 0 字节
~~~

结论：**guest 页里就是原始 arm64 指令、与磁盘一致、未被翻译层改写**；只是**没有 PROT_EXEC**，CPU 不会直接执行这里的字节。执行体是 houdini 的翻译产物（libhoudini.so 3 段 r-xp + memfd:jit-cache 64 MB rw-s）。

---

## 4. 二分判定

### A. 代码段是不是「可执行的原始 arm64」？—— **否（实测，确定）**

依据 §3.3–3.8：四段全 r--p / rw-p，VmFlags 无 ex；原始 ARM64 字节完好但无执行权限；进程里 ARM 侧唯一可执行映射是 libhoudini.so（+ TenSafe 的 libtcb.so）。

### B. 「写原始字节的 inline hook 不可能生效」？—— **部分成立，不能写成绝对**

| 子命题 | 判定 | 依据 |
|---|---|---|
| 对**已被 houdini 翻译过**的函数，改原始字节**必然无效** | **成立（推断，强）** | 执行体在翻译缓存；绝区零历史记录「装钩太晚 → Houdini 已把函数块译进 rwxp 缓存」 |
| 对**尚未翻译**的函数，改原始字节**可能生效** | **成立（实测反例）** | 我本布衣 24/24 hooked @name 且 ShowRewardAd intercepted 真实生效；蛋壳 12/12 + L3 四项 PASS；阿比斯 dmgx 命中 23 次——三例都在**同一台 emulator-5554** |
| 「装钩成功」日志能证明生效 | **不成立** | VmFlags mw/me ⇒ mprotect+写页不会失败；无限升级正是「报成功 + 零命中 + 首译崩」 |
| 只要写进去就安全 | **不成立** | 无限升级：trampoline orig=0x7d147e0ba000 落在 x86 空间（不在 0x4000… guest 区间）⇒ 首译解析失败，rip 恒 libhoudini.so+0x30c02a；绝区零 0xdead1007 崩栈全在 libhoudini |

### 综合工程判定

> **「改游戏代码页」在雷电 Houdini 上不能当作可依赖的默认路线（判：不可行作主线）；它能「有时生效」（3 例）也能「装得上但零命中/直接崩」（3 例），成功与否取决于 houdini 翻译缓存时序与 trampoline 落点——两项都不由我们控制。**
>
> 本包（旷野征途 3.2）**未做 hook 运行实测**（装星光是写设备动作，越权）⇒ 本包 hook 生效性 = **未证**。缺的证据：一次单点探针包（1 个 by-name 挂钩 + hit 计数 + 转译环境判据），跑 60 s 看是否出现 hooked by-name、hit>0、零 SIGSEGV。

---

## 5. 替代形态清单（① ~ ④，另补 ⑤）

| # | 形态 | 判定 | 依据 | 要确认还需什么 |
|---|---|---|---|---|
| ① | **il2cpp 按名 API**（dlsym il2cpp_class_get_method_from_name / il2cpp_field_get_offset / il2cpp_runtime_invoke） | **可行（证据充分）** | 与代码页无关（纯动态符号解析 + 数据层读写）。本地实测 .dynsym 2,693 个符号，含 il2cpp* **243 个**，且 il2cpp_class_get_method_from_name / field_get_offset / runtime_invoke / domain_get / thread_attach / class_from_name / domain_assembly_open / object_new / gc_disable **全部导出**（shndx=15，GLOBAL） | 只需运行时验一次：进程起来后 domain_get → thread_attach → 按名拿 MethodInfo 全成功（蛋壳/我本布衣已验证同类链，本包需一次日志） |
| ② | **MethodInfo->methodPointer 替换**（免广告这类 AOT 方法） | **可行但作用面窄（推断）** | MethodInfo 是数据（rw-），改它不碰代码页；但只有「经指针调用」的路径受影响（delegate / 反射 / il2cpp_runtime_invoke / 部分虚调用），**AOT 直接调用点写死了目标，改 methodPointer 无效**。偏移：IL2CPP MethodInfo 首字段即 methodPointer（offset 0，virtualMethodPointer 在 +8），需按版本核 | 核 offset：运行时把 mi+0 读出的指针与该函数 RVA 对应 guest 地址比对（一次日志即可定）；再确认目标方法实际调用方式（AOT 直调 or delegate） |
| ③ | **字段直写**（无敌拉 curHp、倍率写 atk / _baseAttributes） | **可行（最稳，首选）** | 纯数据层，与 hook / 代码页 / 翻译缓存全无关；字段偏移可 il2cpp_field_get_offset（已导出）或 dump.cs 静态取；代价是要找到实例（ScanInstances 类路径，边界见霓虹深渊2 台账 m04997） | 阶段 3 才做：dump.cs 定位类/字段 + 一次实例可达性日志。**注意**：若数值服务端权威（伤害结算），客户端直写上限有限 |
| ④ | **解释器方法**（HotUpdate.dll 的 ApplyHp，无独立 methodPointer） | **走 inline hook 解释器入口 ⇒ 不可行（推断，强）；走 IL 字节/数据面 ⇒ 可行（未证）** | 本包**实测是 HybridCLR**：libil2cpp.so 内 HybridCLR 字符串 53 处、调试路径 D:/Fork/2dgame/HybridCLRData/LocalIl2CppData-WindowsEditor/il2cpp/libil2cpp；EnterFrameFromInterpreter 0 处、il2cpp_interp 0 处 ⇒ 该入口名不适用本包；HybridCLR 解释器入口在 libil2cpp 代码段内 ⇒ 钩它＝又回到代码页问题。可行绕法：改**内存里的 IL 字节**（热更 dll 的 image 在托管内存 rw-，不碰代码页），或经 ① 调解释器方法 | 需确认：①HotUpdate.dll 落在 115 个 yoo bundle 中的哪一个（2.4 的 53a9548b… 指纹不可沿用）②HybridCLR 版本（决定 image/IL 内存布局）③ApplyHp 是否真走解释器帧（也可能是 AOT 桥接调用） |
| ⑤ | **补：静态改 libil2cpp.so 字节**（底包内预打补丁，非运行时） | **理论最干净（未证）** | houdini 翻译时从文件→guest 页读字节；补丁若在磁盘上，翻译产物天然含补丁 ⇒ 不需 mprotect、不需 trampoline、不受翻译缓存时序影响 | 需确认：①游戏是否有 so 完整性校验（vc32 未查）②与规范冲突：AGENTS.md §3.2 明写「直装包不得只换 libil2cpp.so」⇒ 属**需 lead 决策**的形态，不宜自行采用 |

---

## 6. 对阶段 3 的影响（决策建议）

1. **挂点形态排序改掉**：阶段 3 默认优先级 = **③ 字段直写 > ① il2cpp 按名 API > ② methodPointer 替换 >> inline hook**。原「RVA/file-offset 首钩」在模拟器上恒假（r-xp = 0），999 秒那轮的 9/9 not in RX 就是这条路的天花板。
2. **inline hook 只作单点探针**：真要试必须 ① 走 by-name ② 只挂 1 个 ③ 首钩失败/零命中即永久 disable ④ 以 **hit 计数**为唯一生效判据，不认 hooked / installed。并要接受「首译崩」概率（属 houdini，修不了）。
3. **L3 验收口径**：模拟器侧只认「功能日志命中 + 零 SIGSEGV/ANR」；「装钩成功、菜单正常」不算通过（蛋壳/我本布衣能过是因为 by-name 真命中，不是装钩成功）。
4. **真机不可替代的部分**：任何必须拦 AOT 直调内部函数的功能，在模拟器上应当**设计成不需要**；否则只能靠 96f0d883（本轮禁止触碰）或换 arm64 宿主。
5. **阶段 3 第一个验证点**（约 30–60 分钟，纯日志）：进程起来后跑 ①链路（domain_get / thread_attach / class_from_name / method_from_name）+ ② 的 mi+0 偏移核对 + ③ 的一个字段偏移取值，三项出日志即可判定「数据层路线在本包成立」。这一步不改任何代码页，风险为零。

---

## 7. 未证 / 边界

- **本包 hook 生效性未证**：本轮只有只读探测 + 启动进程，未装星光探针包（写设备，未授权）。
- **0x40006014b000-0x4000691de000（8.7 MB 匿名 rwxp）归属未证**：位置紧贴 libtcb.so（TenSafe），我判为壳/风控的代码缓存，**不是** houdini 翻译缓存；houdini 侧证据是 libhoudini.so 3 段 r-xp + memfd:jit-cache 64 MB rw-s（推断）。
- **本包是否有 TenSafe 类风控**：libtcb.so 已加载，但未观测其线程行为；绝区零那轮「hook 后被 TenSafe 线程杀」是否复现于本作 = 未证。
- **HybridCLR 版本/入口符号**：仅由字符串与调试路径确认存在，未定位入口 RVA（属阶段闸门内工作）。
- **未做**：screencap / input tap / keyevent / uiautomator / install / uninstall / pm clear / 任何写设备动作 / 触碰 96f0d883。
- **进程状态**：com.xf.kyzt **未 force-stop**，pid 3580 收尾时仍存活（/proc/3580/stat 状态 S）。

## 8. 复现命令（只读）

~~~bash
ADB="E:/leidian/LDPlayer14/adb.exe"; S="$ADB -s emulator-5554"
$S shell am start -n com.xf.kyzt/com.xiefeng.game2d.PrivacyActivity
PID=$($S shell pidof com.xf.kyzt | tr -d '\r')
$S shell "su -c 'cat /proc/$PID/maps'"  | grep -E "libil2cpp|libunity|houdini|memfd"
$S shell "su -c 'cat /proc/$PID/smaps'" | grep -A12 "^400024997000"
$S shell "su -c 'ls -l /proc/$PID/map_files/'" | grep -E "libil2cpp|houdini"
# 字节对照（guest 0x400024997714 ↔ 磁盘 0x1793714）
$S shell "su -c 'xxd -s 0x400024997714 -l 64 /proc/$PID/mem'"
$S shell "su -c 'xxd -s 0x1793714 -l 64 <libil2cpp.so 落盘路径>'"
~~~

输入指纹：libil2cpp.so sha256 b249a13111e87c82ca595d374a10da5c4c972e3dd94bac2fd0540f07b65e332b（66,268,112 B）；本包 guest base 0x400023200000；.dynsym 2,693 符号 / il2cpp* 243 个（本地 ELF 解析，脚本可重跑）。
