# 超维进化 2.1 功能注入修复（2026-09-05 下午）

用户反馈：TapTap 登录已成功，但六个功能全部无效。设备 f9a116a2。

## 问题一：注入从未发生（已修复，PROVEN）

**现象**：菜单/授权正常（grant ok、开关状态到达 Java 侧），但 `[Cwjh] feature #N=… splice=0` 恒为 0，`等待游戏库…(cocosjs=0)` 每 5s 一次，引擎识别"未检测到已知游戏引擎，进入通用模式"。

**根因**：本游戏 `extractNativeLibs=false`，游戏 so 直接从 base.apk 内映射。实测运行进程 `/proc/<pid>/maps` 中 **0 行**含 `libcocos2djs`（映射行只显示 `…/base.apk`）。`isLibraryLoaded()`（Utils.cpp）用 strstr 扫 maps 找库名 → 永远 false → Main.cpp 等待循环 120s 超时后 ENGINE_UNKNOWN → Cwjh_TryInit 从未执行 → hook 从未安装。 JNI_OnLoad 分发链缺 CWJH 分支，`Cwjh_StartEarlyHookThread`（现成的 20ms 轮询抢挂机制）也从未启动。

**修复**：
1. `Utils.cpp isLibraryLoaded`：maps 未命中时回退 `xdl_open`（dl_iterate_phdr，linker 对 APK 内嵌库记 `base.apk!/lib/<abi>/<name>.so`，可按后缀匹配）。
2. `Main.cpp JNI_OnLoad`：补 `#elif defined(STARRY_GAME_CWJH) Cwjh_StartEarlyHookThread(vm)`。

**修复后实测**：`cocosjs=1`、`检测到 libcocos2djs.so，ENGINE_COCOS`、`hooked se::ScriptEngine::evalString`、`hooked JNI evalString`、`early se hook ok`、`init ok se=1 jni=1`。

## 问题二：早期线程 SIGSEGV（已修复，PROVEN）

修复一生效后暴露：early 线程在 `TryHookEvalString` → `DobbySymbolResolver("libcocos2djs.so",…)` 拿到垃圾指针（fault addr 0x786564a613a05573，高位为字符串 ASCII "xed"）→ DobbyCodePatch 解引用崩溃 → 进程死循环重启。

**根因**：DobbySymbolResolver 内部 dlopen/名字查找不支持 APK 内嵌库。

**修复**：`CwjhMod.cpp` 新增 `Cwjh_ResolveLibSym`：`getSymAddress`（xdl 解析）+ 库基址 ±256MB 范围校验，替代 DobbySymbolResolver。Utils.hpp 补导出声明。

## 问题三：splice 语法错误 + 锚点漂移（已修复，PROVEN）

hook 通了之后 game.js 拼接首次真正执行，暴露两问题：
1. **⑤号锚点括号 bug（实现期就存在，此前从未执行到）**：`return jsb…(4)||((e=…) >= …;` —— `||(` 的括号未闭合 → `SyntaxError: Unexpected token ';'` → game.js 求值失败 → 游戏自退（exit 2）。
2. **锚点漂移**：设备实际运行的是热更 0.2.83 game.js（2,575,785 字节，SHA256 849104333d189266e7befe277dab7394864daae8cb77246024ca46d06fe40f3c；档 `evidence_20260905_resume/game_js_0.2.83.js` 已按 2026-09-30 热更可删规则清理，hash 留在本文）。2.1 锚点中 `doDmg(e, n, r, …)` 与暴击 `Math.random() < Math.min(u, 1)` 已不存在；0.2.83 为 `doDmg(e, r, a, …, p = !1)`（攻击方=第3参 a）与 `t.Random.inst.next() < Math.min(i, 1)`（两处，玩家处用 `o *= c;` 前缀锁定）。

**修复**（CwjhMod.cpp SpliceCombatJs）：⑤补右括号（`;`→`)` +`;`）；①/⑥ 新增 0.2.83 锚点（保留 2.1 旧锚回退）；NeedsCombatSplice 增加两套签名。全锚点唯一性核对=1；Python 严格模拟拼接 + `node --check` 语法通过。

**修复后实测**：`combat splice #1 len 2575785→2577330`（7 锚点全中，+1545B），无 SyntaxError，游戏正常完成热更检查（0.2.83→0.2.83 的 restart 往返为游戏自带流程：首次 hotUpdate 落 undefined→cc.game.restart()→第二次 eval，非异常）、TapTap 自动登录成功（loginDone success=true）、进入服务器连接与资源加载。

## 问题四（当前阻塞）：游戏反篡改延迟自杀

- 1067（完整运行）：登录成功后 ~7.7s 被 SIGKILL。退出记录 `exit_self_1067_10652`（与启动期签名门禁同款 kill 语义）。
- 2997（MenuService 重启拉起后被复用为完整游戏进程，RSS 285MB）：19:27:06 同样 `exit_self` SIGKILL。
- **对照**：用户成功会话（22055，同一 TapTap patch 包）hook 未装、splice=0，稳定运行 8 分钟以上 → 排除 TapTap patch 与签名表 patch 本身。
- 判定：游戏自带反篡改线程（静态分析已知 sub_16d5100 在 0x16d56ac 建 0x16d48b8 线程、0x16d56b0 调 sub_170b068 再建线程）周期性校验，检测到 inline hook（se::evalString/JNI evalString 代码字节被改）或 splice 后 kill(0,9)。校验对象（hook 代码完整性 vs 文件 hash vs 其他）待静态分析。
- 后台分析：antitamper_thread_evidence.txt（生成中）。

## 版本与产物

- 当前装机：`08_output/超维进化/超维进化_2.1_surgical-releaseSigned.apk`（含问题一/二/三修复）。
- 改动文件：`Includes/Utils.cpp/.hpp`、`Main.cpp`（JNI_OnLoad CWJH 分支）、`Games/CwjhMod.cpp`（Cwjh_ResolveLibSym + SpliceCombatJs 0.2.83 锚点 + ⑤括号修复 + NeedsCombatSplice 双签名）、`sign/SignSpoof.java`（cwjh 证书）、`sign/TapLoginForceWeb.java`（白名单注释）、`scripts/surgical_cwjh_2.1.sh`（[3c/6] TapTap dex patch + classes.dex 断言）。
- 证据：launch*.txt、pids*.txt、device maps 取证（正文）。（热更档 game_js_0.2.83.js 与全量 logcat2–7/snapshot 已按 2026-09-30 可再生规则清理，可重拉/重抓；隔离区 `~/.Trash/nixiang_slim_docs_20260930_20260930_140157`。）

## 问题四修复（20:18 装机，PROVEN）：反篡改 kill 原语单点中和

静态分析（`../device_20260905_175117/antitamper_thread_evidence.txt`，so SHA256 复核一致）确认：

- watchdog 线程 0x16d48b8（sleep(10) 周期、OLLVM 状态机）→ checkTask → 每 5s bl 0x170ab48（protectFlag=3 分派）→ bl 0x16f06b8：解析 /proc/<pid>/maps 中 "r-xp" 段 → fopen 模块文件 + ELF 解析 → **Adler32 逐字节哈希**（0x17044bc 循环，A=(A+b)%65521; B=(B+A)%65521）→ 不命中 log "check ret : %s %s,kill and exit"/"has been changed!" → pthread_create 炸弹线程 0x16d5a08（sleep(6) → getpid → kill(pid,9) 无条件）。
- libcocos2djs.so 全文仅 2 条 svc：0x16ebdf8=kill(#129) 封装、0x16ebe24=clone(#56)。**7 条自杀路径全部经 0x16ebdf8**：代码校验 kill 点 0x16f79c4/0x16f7b04、端口占用 0x16eed98/0x16eee2c（"%d has being accupied, kill prc!"）、TracerPid 0x16ef0dc、签名门禁 0x16d1b34、炸弹 0x16d5a18。
- 另确认存在 root 检测（access su + popen which su）、maps 扫描（xposed/frida/substrate 等 6 needle）、JavaVM watchdog（DetachCurrentThread 期望 -2）；game.js zip hash 校验低置信（readZipFileData 调用点未追到条目名）。

**中和实施**（CwjhMod.cpp `Cwjh_NeutralizeKillPrimitive`，early hook 线程内先拔牙再挂钩）：对 base+0x16ebdf8 以版本守卫（原指令 0x2a1f03e0 mov w0,wzr / 0xd10203ff sub sp,sp,#0x80）校验后 mprotect RWX → 写入 `mov w0,#0; ret`（0x52800000/0xd65f03c0）→ clear_cache → 恢复 RX。此后任何检测命中都无法执行自杀；clone 封装不受影响（pthread_create 正常）。

**验证（20:18-20:26）**：装机后进程 16799 持续存活 7.5 分钟以上（原死窗为登录后 8~10s，checker 周期 5s 已过多个周期），跨过 20:11 复现运行（14334）同款死亡时点；退出信息无新增条目；游戏在线（RPC seq=156、auth heartbeat 正常）；splice 计数=2；用户已在游戏内逐项开启 6 开关。战斗内实际效果由用户验收。

**残留风险（未覆盖）**：checker 的服务端上报行为未知；game.js zip hash 校验若存在于其他路径，当前 splice 仅改内存缓冲、文件未动，理论不触发；功能效果与封号风险评估未做。

## 功能错位修复 + 4 功能定稿（20:55 装机，待用户战斗内验收）

**用户实测反馈**：开无敌→实际秒杀；开回血→实际无敌；其余无效果。**根因**：菜单 featNum=0 基列表下标（日志 feature #0..#5），而 CWJH 枚举按 1 基定义（1..6），整体错一格——与无限升级"featNum=数组下标含 Category 占位"同款教训。同时菜单列表无 Category 占位，0 号位直接是首个 Toggle，所以必须按 0 基对齐（不能学 ZXR 靠 Category 占位）。

**新功能定义（0 基，删暴击必爆与回血回满）**：
| # | 菜单 | JS get_ret | 实现 |
|---|---|---|---|
| 0 | Toggle_秒杀 | 0 | doDmg 入口，己方攻击→伤害=受击方CUR_HP+1 |
| 1 | Toggle_无敌 | 1 | doDmg 入口，己方受击 return 0 |
| 2 | Toggle_无限技能 | 2 | cost/hasEnoughCost 恒真 + Skill.isInCd 在 isCasting 守卫后恒 false（绕过技能CD与GCD，0.2.83 锚唯一）+ tick 蓝回满 |
| 3 | Toggle_免广告 | 3 | adsPlay 开头直走 C2S_WATCH_ADS_END（需在线） |

改动：CwjhMod.h 枚举 0 基 4 项；Main.cpp 列表 4 项；SyncJavaRets setRet(0..3)；SpliceCombatJs 删 recHp/暴击拼接、加 isInCd；OnFeatureChanged/Tick/ForceClear 对齐；CwjhHooks.java 注释更新。

**装机验证（20:55-20:56）**：early hook ok → anti-tamper neutralized → hooked se/jni → init ok → combat splice #1/#2 len 2575785→2577187（+1402B，6 锚点：doDmg/recSp/cost/hasEC/isInCd/adsPlay）；模拟拼接 node --check 语法通过；进程存活观察期 80s+ 无退出。战斗内实际效果待用户验收。
