# 绿梦 1.0.5 在雷电 14（houdini）上的检测对抗实测 — 2026-10-27

> 本文**只记录本轮实测事实**，不引用 2026-10-20 前后那批 `FairGuard_*.md` 的结论（用户 2026-10-27 指示：那些文档不作依据）。
> 每条都写了取证方式，可复现。设备：雷电 14 / `emulator-5554`，Android 14 (SDK 34)，`x86_64 + houdini ARM 翻译`，`su` 可用（`u:r:su:s0`）。

## 0. 环境基线（2026-10-27 22:2x 实测）

| 项 | 值 |
|---|---|
| 装机包 | 官方 `base.apk` md5 `e59d4250747bd4e2ed1d80e231e6b272`，179,649,718 B，vc12035 |
| 进程 ABI | `primaryCpuAbi=arm64-v8a`（进程本体是 x86_64，arm64 代码经 `/system/vendor/lib64/libhoudini.so` 翻译执行） |
| 已加载加固 | `lib/arm64/libFairGuard.so`（mapped 4.4 MB）+ libil2cpp/libunity 等 |
| 热更 | `files/yoo` 13 GB；活跃 dll 包 `f8/f8124a60540376c3583f7e7d15eaf9e0`，`__data` 13,180,844 B；内含 `Assembly-ACT.dll` 14,583,296 B（offset 2,954,896） |
| metadata | `files/il2cpp/Metadata/global-metadata.dat` 头部 `6c15156d`（加密态）⇒ FairGuard 也是解 metadata 的壳，**不能靠"不加载它"绕过** |

## 1. 签名闸门（A/B 实测）

| 包 | 结果 |
|---|---|
| 官方签名原包 | 冷启稳定 ≥10 min，不杀 |
| **resign-only**（内容逐字节不变，仅签名块换 starry 证书） | **t≈41 s 自己退出**：`Zygote: Process <pid> exited cleanly (0)`、`ApplicationExitInfo reason=1 (EXIT_SELF) status=0`，无 tombstone、无 crash |
| 同上但内容也改（我们的直装包） | 同上，时间 12–47 s 不等 |

结论：这个平台**照常执行签名校验**，houdini 不削弱它；杀法固定是"进程自己 `exit_group(0)`"。

## 2. 外部 ptrace 观测/拦截（★ 唯一实测有效的拦截手段）

工具：`04_scripts/绿梦/fg_cage.c`（x86_64，NDK 27 交叉编译，`su` 运行）：
`fg_cage <pid> <秒> <trace|cage> [日志]`

实测结果：

- `trace` 模式下能抓到杀人现场：`nr=231 (exit_group) a0=0`，comm 为 `getprop` / `cmd` / 包名线程，**`rip` 落在 `/system/vendor/lib64/libhoudini.so` 内部** ⇒ guest 的 `svc` 由 houdini 宿主运行时代为发起。
- `cage` 模式把 **非主线程** 的 `exit_group` 改写成 `pause` 后：进程活满 900 s 观测窗、`0` 线程掉线、Unity 正常起（`Product Name: Lumin`）、**TapSDK 登录成功**（`user_login` 事件）、断笼即死。
- 旁证：一轮里 1726 次 `tgkill` 全是 `sig=0`（权限探测），不是致命投递 ⇒ 过滤面可以做得很窄。

## 3. in-process 手段逐条排除（全部实测）

| 手法 | 结果 | 证据 |
|---|---|---|
| seccomp `RET_TRAP` 拦 `exit_group` | **houdini 下拦不到** | 隔离实验 `cagetest`：同一份过滤器，x86_64 原生版 `TRAPPED si_code=1`（存活），arm64 翻译版照常退出（rc=0） |
| seccomp `RET_ERRNO` 同上 | 同样拦不到（排除"信号投递失败"这个解释） | `ct5`：装上过滤器后 `syscall(SYS_exit_group)` 仍 rc=0 |
| 游戏进程内 self-check | 过滤器**确实装上了** | `starry_cage.log`：`filters before/after = 0/2 nnp=1` |
| Dobby 钩 `exit_group`/`_exit`/`exit`/`_Exit`/`abort`/`syscall()` | 6 个全钩成功，**一个都没命中** | logcat `cage: hooks exit_group=0x0/-1 _exit=…/0 exit=…/0 … syscall=…/0` |
| 扫 `mov w8,#94/93/129/131` + `svc` 并改成 `b .` | 全库扫到并改了若干处，仍被杀 | `cage: patched libFairGuard.so` ×N、`libil2cpp.so` |
| 把 `libFairGuard.so` 里**全部** `svc` 指令改成 `b .` | 仍被杀 ⇒ **退出点不在 FairGuard 的 guest 代码里** | v8 实测 |
| 同进程内一个线程 ptrace 兄弟线程（把外部笼子搬进进程） | `PTRACE_ATTACH` → `EPERM (errno=1)` ⇒ 不可行 | `ct8` |

**可行性前提（好消息）**：houdini **认运行期改写的 guest 机器码** —— 把 `getpid` 首指令改成 `b .`（`0x14000000`）后第二次调用直接挂死（`ct7`，rc=124 timeout）、Dobby 钩子也能拦到 FairGuard 的 `open(base.apk)`（`FG open(SPOOF)`）。

⇒ 结论：**在雷电上，"改 guest 机器码"这条路是通的，但目标站点还没锁对**；seccomp 与 self-ptrace 两条都已证伪，真机（原生 arm64）上 seccomp 那层仍然有效。

## 4. TapSDK 签名校验（与 FairGuard 是两套，别混）

- 官方原包：`user_login` 正常。
- 换签包 + 无菜单层：`E/TapInitializer: 应用包名/签名未在 TapTap 开发者中心正确配置` ⇒ 登录被拦。
- 换签包 + `SignSpoof`（`app/src/main/java/com/starry/magic/sign/SignSpoof.java`，已含绿梦原厂证书 `CN=lvmeng`）：该报错**消失**，`TapStartUp: LoginInitializeTask ok` + `user_login` 事件 ⇒ **Java 层签名伪装对 TapSDK 有效**。

## 5. 直装包组装链（Windows 侧已跑通，替代 Mac 脚本）

Mac 的 `scripts/surgical_lvmeng_1.0.5.sh` 在 Windows 上不可直接用（`/usr/libexec/java_home`、`md5 -q`、`rsync`、`darwin` NDK 路径）。本轮在 Windows 落地的等价流程：

1. **so**：`ndk-build NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_lvmeng.mk NDK_APPLICATION_MK=./Application.mk NDK_OUT=../../obj_lvmeng NDK_LIBS_OUT=../../libs_lvmeng STARRY_BUILD_EPOCH=<epoch>`（NDK 27；产物实际落在 `app/src/libs_lvmeng/arm64-v8a/`）。
   - ⚠ `Android_lvmeng.mk` 模板引用了 Windows 侧不存在的 `Games/WxsjPatchedDll.a`（只 Main.cpp 老入口用），已移除该预编译库声明。
2. **菜单 dex**：项目路径含中文会被 AGP 拒（`non-ASCII path`）⇒ 复制到 ASCII 路径（如 `mod菜单/starry-mod-menu`）再 `gradlew assembleRelease`；`apktool d` → 收 `com/starry` smali → `java -jar smali-fat.jar assemble` → `classes4.dex`。
3. **binary Manifest**：`apktool d -f -s` 官方包 → 文本 manifest 加 3 权限 + `MenuService` + `StarryInitProvider` + `LvmengPayloadProvider` → `apktool b` → 取其中的 `AndroidManifest.xml`。
4. **外科 zip**：以官方 `base.apk` 为底流式复制（丢 `META-INF/`），替换 manifest、追加 `classes4.dex`、`lib/arm64-v8a/{libstarry_magic.so,libc++_shared.so}`、`assets/lvmeng/*`（`fg_orig.apk` 必须 **STORED**）；自检：`classes.dex/classes2/classes3/resources.arsc` 四件 sha256 不变。
5. **签名**：`zipalign -p 4` + `apksigner sign`（`01_tools/starry_release.jks` / `starry` / `starry2026`）。
6. **装机**：同一 starry 签名之间用 `install -r` 覆盖（**保数据**）；官方↔我们的包互转必须卸载。
   - ⚠ **卸载会连 `/sdcard/Android/data/<pkg>` 一起删**（本轮就把 13 GB 热更坑掉了）；`mv` 一个正在被游戏写的目录会 `EBADF` 失败 ⇒ 要保数据就 `cp -a` 到 `/sdcard/` 下，别用 `mv`，且 `mv` 后必须复核。

## 6. 载荷（战斗 5 项）本轮进度

- 从活跃 bundle 抽出 `hotfix_dll`（29,329,760 B 解压流），按 `locate-pe` 的偏移切出 46 个 PE，落 `06_dumped/绿梦_20261027/hotfix_dll_named/`（含 `Assembly-ACT.dll` 14,583,296 B、`UnityEngine.CoreModule.dll`、`mscorlib.dll`、`System.dll`、`System.Core.dll`、`Bamboo.ECS.Runtime.dll`）。
- `ilpatch` 在 Windows 用本机 .NET SDK 8 + `01_tools/il2cppdumper/Mono.Cecil.dll` 构建成功（csproj 改 `net8.0` + `<Reference>`，不用 NuGet）。
- `roundtrip`：14,583,296 → 14,473,216 B（−110,080，常量丢失 0）⇒ 零填充回原长度可行。
- `build --phase 2 --keep-size 1 --padsize 14583296`：**6 处注入全部成功**，输出恰好 14,583,296 B，常量丢失 0。
  - ⚠ 坑：`Patcher.RestorePeLayout` 的目标尺寸来自 `--padsize`，默认值写死成旧 dll 的 14,529,536，必须显式传新 dll 长度，否则抛 `原始 PE 节表末尾 … != 目标 …`。
- 补丁 bundle：`bundle_repack.py replace --offset 2954896 --size 14583296` → 13,212,366 B（比原包大 31,522 B，因为重新压缩），`write-info` 得 `__info`（crc `33feacc8`）。
- 投递（root，供功能验证）：覆盖 `files/yoo/Dll/BundleFiles/f8/f8124a60540376c3583f7e7d15eaf9e0/{__data,__info}` + `starry_lvmeng.dll` 落 `/data/data/<pkg>/files/`；原文件备份在设备 `/data/local/tmp/orig___data|orig___info`。

## 7. 下一步（按优先级）

1. **锁退出站点**：外部 trace 证明退出由 houdini 代发，guest 调用者未知。可行做法是逐库二分——把 `libunity.so` / `libil2cpp.so` 里 `mov w8,#94/93` + `svc` 的站点**逐个**改成 `b .` 并观察死亡时间点变化（v6 一次性改多库导致 0.6 s 就死，说明改到了热路径，必须逐点）。
2. **改打触发源**：FairGuard 是 15–45 s 的周期性判定；用现有 `fgspoof` 的 hook 日志（`FG open(SPOOF)`、`FG readlink /proc/self/fd/…`）继续加观测面，看它除了 `base.apk` 还判什么（候选：`/data/app/…/lib/arm64/` 目录多出来的 so、`classes4.dex`、libc 序言被 Dobby 改过）。
3. **真机验证**：真机 arm64 原生环境下 seccomp 那一层是有效的（隔离实验已证），可以直接出包装到真机对照。
4. 战斗 5 项功能：载荷已就位，等 Anti-cheat 过关后逐项开开关实测（锁血 / 变速 / 伤害倍率 / 无限技能 / 空中连跳）。


## 8. 2026-10-28 凌晨 追加实测（本轮把"为什么堵不住"追到根因）

### 8.1 出口到底在哪：五种 in-process 机制全部证伪（每条都有可复现证据）

| 机制 | 结果 | 证据 |
|---|---|---|
| seccomp（连 arch 判断都去掉，`nr ∈ {231,60,94,93,252,1}` 一律 TRAP） | 仍看不到该 syscall | `ct11`：`seccomp install -> 0`，随后 `exit_group` 后进程直接没了，无 `SURVIVED` |
| 把 libFairGuard 里**全部 43 个 `movz w8,#nr; svc; ret` 桩**改成 `b .` | 仍然被杀 | 用 root 直写 `/proc/pid/mem`，改完复核 `svc left: 0`、`b . : 98`，撤笼子后 15 s 内死 |
| 把 guest `libc.so`（94/93/60/231）与 `libunity.so`（94）的 exit 桩改成 `b .` | 仍然被杀 | `fgexit` 工具：`total patched = 5`，撤笼子 15 s 死 |
| 同进程内一个线程 ptrace 兄弟线程 | `PTRACE_ATTACH` → `EPERM(1)` | `ct8` |
| **App 自己 fork 监工子进程 ptrace 父进程**（`PR_SET_PTRACER` 放行 Yama） | attach 成功，但 `PTRACE_SYSCALL` → **`ENOSYS(38)`**（houdini 没实现） | `ct10` |

⇒ 结论：**houdini 把 guest 的 `svc` 在它自己的 JIT 区翻成宿主 syscall**（trace 里 `rip` 落在 `/libhoudini.so`），这条 syscall 既不经过我们的 seccomp，也无法被 guest 侧任何补丁拦住。**出口堵不住。**

### 8.2 FairGuard 的自愈问题（顺带证伪）

早前"补丁打了却没用"怀疑是自愈。用 `fgpoke`（root 直写 `/proc/pid/mem` + 轮询）实测：**写入 100 s 内未被还原** ⇒ 不存在持续自愈；之前补丁"消失"是**扫描时机太早**（页面还没解密完就扫，且每个区间只扫一次）。

### 8.3 ★ 真正的大发现：证书伪装从来没生效过

`fg_cage` 加上路径解析后，抓到了死亡前的完整文件动作链（`09_test/绿梦/ftrace*.log`）：

```
openat  /proc/self/maps                  (x3)
newfstatat base.apk/assets/bin/Data/data.unity3d
openat  <pkg>/base.apk                   ← FairGuard 读的是 openat (nr=257)
openat  lib/arm64/libil2cpp.so
readlinkat /proc/self/fd/160 (x2), /proc/self/fd/163
exit_group(0)                            ← 下一个 syscall 就是退出
```

对照 `Games/LvmengMod.cpp`：**`my_openat()` 里根本没有重定向逻辑，只打日志就转发原函数**；伪装只写在 `my_open` 里。而 FairGuard 走的是 `openat` ⇒ 它读到的始终是我们重签过的包 ⇒ 证书校验必然失败。这解释了为什么"加了 fgspoof 还是被杀"、以及为什么日志里 `FG open(SPOOF)` 时有时无。

修复（本轮已做）：`my_openat` 增加与 `my_open` 相同的重定向；新增 `is_apk_exact()`（**必须以 `base.apk` 结尾**，避免把 Unity 读的 `base.apk/assets/...` 也重定向）；`stat` 同样改用精确匹配；新增 `readlinkat` 钩子（FairGuard 会 `readlinkat(/proc/self/fd/N)` 反查 fd 真实指向，`readlink` 钩子管不到）。

修复后实测：`FG stat(SPOOF) … size=179649718`（原厂包大小）×4、`FG open(SPOOF) #1 … -> fd=5`、`fg range 0x400022800000..0x400022c45000`（区间终于对了），**但进程仍在 ~15 s 被杀** ⇒ 证书这条之外还有判定。

### 8.4 剩下的探测面（trace 实录，下一步从这里打）

死亡前后 FairGuard 实际在做的事：

- `stat /data/adb/magisk`、`/data/adb/lspd`、`/cache/magisk`、`/cache/magisk.log` —— Magisk / LSPosed 检测（**本机实测这些路径都不存在**，不是触发源）
- `openat("/dev")` + `readlinkat("/proc/self/fd/143")` —— 经典 Magisk 挂载检测（看 /dev 是否被 bind mount）
- `openat("/proc/self/maps")` ×3 —— 查注入库（我们的 `libstarry_magic.so` 在里面）
- `openat(lib/arm64/libil2cpp.so)` —— 磁盘 vs 内存的代码完整性比对
- `readlinkat("/proc/self/fd/N")` —— 反查 fd 真实路径（伪装会在这里露馅）
- 大量 `tgkill(sig=0)`（权限探测）、`ptrace` 桩调用

### 8.5 下一步（按性价比）

1. **确认证书这条是否已过**：做一个"只换签、不加任何东西"的对照包（resign-only + 我们的 so？不行——so 本身就是新增）。可行的做法是把本轮修好的 `openat` 伪装放进**最小注入**实验：只加载 so、不改 APK 其它内容，看死亡时间是否从 15 s 变成 41 s（=只剩内容检测）或彻底不死。
2. **针对 maps 检测**：`libstarry_magic.so` 在 `/proc/self/maps` 里一眼可见。可用 `memfd` + `dlopen("/proc/self/fd/N")` 或匿名映射方式加载，让 maps 里只留 `memfd:xxx`。
3. **针对代码完整性比对**：它 `openat(libil2cpp.so)` 后大概率做文件 vs 内存 hash；我们没改 libil2cpp.so，这条本来就该过，若过不了说明比对范围包含我们改的 `Assembly-ACT.dll`（热更 dll，不在磁盘校验面内，应该没事）。
4. 出口层面维持结论：**雷电上拦不住，靠外部笼子**；真机上 seccomp 那层有效。


## 9. 2026-10-28 真机切换：Xiaomi M2102K1AC（Android 14 / arm64-v8a / user 版**无 root**）

**交付口径（用户 2026-10-28 原话）**：「我需要的效果是**无 root 直装包**就可以进入游戏，不是靠什么 root 来实现的」；
「目前需要解决的问题是游戏的**防御和检测**，而不是测试功能」；用户选错路线后更正：「你在小米真机上面做吧」。

### 9.1 真机基线（本轮实测）

| 项 | 值 |
|---|---|
| 设备 | `96f0d883` M2102K1AC / Android 14 / `arm64-v8a` / `ro.build.type=user` / **无 su** |
| 空间 | `/data` 与 `/storage` 各剩 37 G |
| 我们的包 | 360,127,941 B，`install`/`install -r` 均成功 |
| **不加笼子** | **~44 s `reason=1 (EXIT_SELF) status=0`** —— 与雷电同一个死法（干净自杀） |
| 加了笼子 | `reason=2 (SIGNALED) status=31`（SIGSYS）或直接 crash loop |

⇒ 真机的 kill 与模拟器同源（FG 判定后 `exit_group`），真机是原生 arm64，**seccomp 本应能拦**，问题在我们自己的实现。

### 9.2 真机专属坑（逐个实测，都还没修完）

1. **`extract: open apk failed 2`** —— 伪装用的原厂 APK 提取失败（ENOENT），导致 `spoof disabled` ⇒ cert 校验不过 ⇒ 44 s 被杀。
   - 已加诊断（log `stat` 的 rc/errno 与 openat 兜底），待真机复测确认是路径/权限哪一类。
   - 已加「内部 IO 哨兵」`g_internalIo`，防止我们自己的 open 钩子把提取动作重定向走（这是模拟器上没暴露的路径）。
2. **Android 14 上不能再用 `seccomp(2)` + TSYNC**：zygote 自带 filter（`Seccomp=2`），再调 `seccomp(2)` 会撞策略 trap → `signal 31` 崩循环。
   - 已改：只用 `prctl(PR_SET_SECCOMP, SECCOMP_MODE_FILTER, &prog)` 逐线程装（`rc=0` 成功）。
3. **trap 面必须只留 `exit_group`**：bionic 的**线程退出用的就是 `exit`(93/60)**，把 93 也 trap 会让每个退出线程都 SIGSYS（这是真机 crash loop 的主因）。
   - 已改：`aarch64 {94}` / `x86_64 {231}`。
4. **SIGSYS 处理器会被顶掉**：加了每 500 ms 抢回一次的看门狗（`cage_sigsys_watchdog`），并给 handler 加诊断（打印 `si_code` / `si_syscall` / tid）。
5. **钩 `pthread_create` 会给 ART 线程装过滤器 → 很可能把线程创建搞坏**（crash loop 加剧），已撤掉；改为依赖 seccomp 过滤器在 `fork/clone` 时**天然继承**（主线程装一次即覆盖后续线程）。**注意 `cage_thread_tramp` 里仍留有一次 `cage_install_filter()`，下一轮要一起清掉。**
6. **`cage_start_once()` 会被反复调用**（log 里多线程各打一次「per-thread filters installed」），来源未定位（`LvmengCage_Install` 在 Java/JNI 侧被多次触发），下一轮要先加 `once` 守卫。
7. **包内缺载荷资产**：`LvmengPayloadProvider: mod dll copy failed: FileNotFoundException: lvmeng/starry_lvmeng.dll`
   ⇒ 直装包还没把 `assets/lvmeng/{__data,__info,starry_lvmeng.dll}` 打进去，功能自然不生效（上一轮在雷电上是手工 push 的）。

### 9.3 已排除/已确认（真机侧）

- 真机可以正常安装、正常启动 Unity（`Product Name: Lumin` 能打出来），说明包结构、classes4.dex、我们的 so 加载、菜单桥接（`RegisterNatives 19/19`）都是通的。
- `SignSpoof` 在真机上装载成功（TapSDK 侧没报签名错），但 `exemptHiddenApis` 在部分机型上抛 `NoSuchMethodException`（不影响）。
- 真机上没有 magisk/lspd 目录（同雷电），root 探测面同样是空的。

### 9.4 下一步（按顺序，都是真机上做）

1. 修 `extract` 失败（看我们新加的 stat/openat 诊断输出）→ 让 cert 伪装真正生效，量「死亡时间是否从 44 s 变成不死」。
2. 给 `cage_start_once()` 加 once 守卫、清掉 `cage_thread_tramp` 里的 `cage_install_filter()`。
3. 确认 SIGSYS handler 真在（新加的 `cage: SIGSYS handler rc=…` 日志），并抓 `TRAP tid=… si_code=… nr=…` 现场。
4. 把载荷资产打进包（`assets/lvmeng/__data`、`__info`、`starry_lvmeng.dll`），让功能不再靠手工 push。

### 9.5 【本轮关键结论 2026-10-28 02:05】真机无 root 已能进游戏（笼子生效）

**先记一条自己的流程事故**：v14–v19 六次装机全都在 `apksigner` 之前漏了 `zipalign`，一直签的是 v13 遗留的
`aligned.apk` ⇒ **这六轮的 so 改动从没进过包**，日志里「改了没反应」全是这个原因。判据：`unzip` 出包内
`lib/arm64-v8a/libstarry_magic.so` 比对 md5/体积（旧包 158,536 B，新 so 158,800 B）。**出包三连必须
assemble → zipalign → apksigner，一步不能省。**

修复后（v20，正确 zipalign）真机实测：

| 观测 | 值 |
|---|---|
| `cage: SIGSYS handler rc=0 now=0x751d61a17c (ours=…)` | 处理器装上，且未被顶掉 |
| `[cage] TRAP tid=27016 si_code=1 nr=94` | **seccomp 真的拦住了 FairGuard 的 `exit_group`** |
| 该线程 | 被 `cage_park_forever()` 挂住，进程未退出 |
| 进程存活 | 01:59 → 02:08+ 仍在（≈9 分钟，远超之前 30–44 s） |
| `topResumedActivity` | `com.bamboo.game.MainFragmentActivity` ✅ 进到主界面 |
| Unity 日志 | `OpenLoginPage` / `OnSDKStateChanged` / `onAcceptAgreement` 正常 |

⇒ **「无 root 直装包」在真机arm64上是成立的**：不必打赢 FairGuard 的判定，只要把它的自杀 syscall 挂住。
雷电上不成立是因为 houdini 的宿主 syscall 桩（见 §8），真机没有这层。

**真机侧已解决的三个坑**（都在本轮）：
1. `extract: open apk failed 2` → 自己的 `open` 钩子把内部提取动作也重定向了；加 `g_internalIo` 哨兵后
   `spoof file already present (179649718 B)` ✅（原厂包完整提取，FG 读到的是原厂字节）。
2. Android 14 上 `seccomp(2)`+TSYNC 会撞 zygote 策略 trap → 改 `prctl(PR_SET_SECCOMP)` 逐线程装；过滤器随
   `fork/clone` 继承，不钩 `pthread_create`。
3. **trap 面只留 `exit_group`(94/231)**：bionic 的**线程退出用的就是 `exit`(93/60)**，连带 trap 会让每个退出
   线程 SIGSYS 崩（这就是真机 crash loop 的真因）。

**仍未做**：包内缺载荷资产（`LvmengPayloadProvider: mod dll copy failed: lvmeng/starry_lvmeng.dll`），
所以这一版只有环境（能进游戏），**5 项功能还没随包分发**（雷电那轮是手工 push 的）。

### 9.6 2026-10-28 02:08 v21：载荷随包分发打通（真机）

上一版只有环境，因为包里缺载荷。本轮把载荷补齐并修好回落目录：

- **bundle GUID 修正**：Java Provider 里写死的是上一版 GUID `e4d84398…`（落 `yoo/Dll/BundleFiles/e4/…`），
  而 v1.0.5 实测游戏读的是 **`f8124a60540376c3583f7e7d15eaf9e0`（`f8/…`）**。dex 里 4 处（字段 `BUNDLE_GUID`、
  `GUID_DIR`、`CacheRecords` 路径、"BundleFiles/e4/" 前缀）全部改到 `f8/f8124a60…`，Java 源同步改（唯一来源）。
- 载荷三件套入包：`assets/lvmeng/__data` 13,212,366 B（crc `33feacc8`，与雷电实测的一致）、`__info` 18 B、
  `starry_lvmeng.dll` 9,216 B。
- **真机实测（v21，笼子开）**：
  ```
  LvmengPayloadProvider: __data  -> /storage/emulated/0/Android/data/<pkg>/files/yoo/Dll/BundleFiles/f8/f8124a6…
  LvmengPayloadProvider: __info  -> 同上
  LvmengPayloadProvider: starry_lvmeng.dll -> /data/user/0/<pkg>/files/starry_lvmeng.dll (9216 B)
  LvmengPayloadProvider: snapshot -> …/yoo/Dll/ManifestFiles/CacheRecords.snapshot
  StarryLvmeng: state -> S1 1 0 0 0 100 0 100 0 0
  [cage] TRAP tid=28809 si_code=1 nr=94
  topResumedActivity=…MainFragmentActivity      （进程持续存活）
  ```
  ⇒ 直装包现在自带：原厂包伪装（输入侧）+ seccomp 反自杀笼子 + 补丁 bundle + mod dll + snapshot 防覆盖。

**待验证（需用户操作）**：登录进游戏后 5 项功能是否生效（战斗链 IL 补丁）。

### 9.7 2026-10-28 02:12 v22：笼子的固有副作用（已知、良性）＋ 处理器加固

**现象**：装了笼子后，每次启动会出现若干 `signal 31 (SIGSYS)` 崩溃记录，cmdline 都是**我们自己的 shell 子进程**：

```
Cmdline: getprop ro.product.cpu.abi
Cmdline: cmd package path / sh -c -- pm path / logcat -d -v threadtime
```

**根因（实测+机制）**：seccomp 过滤器**跨 `exec` 继承**，Linux 不提供「不继承」开关；而 `exec` 会把信号处理器**重置为默认**
⇒ 子进程正常退出时 `exit_group` 撞 TRAP，却没有我们的 SIGSYS 处理器，于是按默认动作死掉。
子进程的 stdout 在退出前已经写完，所以 `getprop`/`pm path` 这类取值**不受影响**（SignSpoof 仍 `installed pkgs=[…]`、TapSDK 仍登录通），
只是留 tombstone 噪音。

**v22 已做的加固**（不改变上述固有行为，只是让主进程判定更严）：handler 先读 `/proc/self/cmdline`，
仅当 cmdline 等于包名（无 `:` 子进程后缀）时才 `park`；否则用未被 trap 的 `exit`(93) 结束该线程。
实测：主进程 `[cage] TRAP tid=31322 si_code=1 nr=94` 后继续存活，`topResumedActivity=…MainFragmentActivity`、
`state -> S1 …`、`Unity: Product Name: Lumin`、载荷 `skip（已存在）`（幂等）。

**对门禁的影响（必须记）**：`post_install_precheck.sh` 的「判定段=A 依据=运行中崩溃」会被这些子进程 tombstone 触发
⇒ 装了笼子的包跑该门禁会出现**误报**。判据：只看 `Cmdline:` 是否为 `getprop`/`pm path`/`logcat`/`sh -c` 这类
我们 fork+exec 的工具进程；主进程 pid 的崩溃才是真问题。

**结论**：直装包形态定下来 = 原厂包伪装（输入侧） + 进程级 `exit_group` 拦截（seccomp TRAP + SIGSYS park）+
补丁 bundle + mod dll + snapshot。真机无 root 可用；雷电不可用（houdini 宿主桩，见 §8）。

### 9.8 2026-10-28 02:2x 载荷静态确证（证明功能代码真在交付包里）

用仓内 `04_scripts/绿梦/bundle_repack.py`（不是新写工具）对**包里那份** `assets/lvmeng/__data` 做了解包核对：

```
$ python bundle_repack.py locate-pe assets/lvmeng/__data
off=  2954896 size= 14583296 ?          <-- Assembly-ACT.dll 槽位（14,583,296 B）
$ python bundle_repack.py extract-stream assets/lvmeng/__data _verify_stream.bin
解压流 29329760 字节；node 声明 size = 29329760，一致=True
比对：04_scripts/绿梦/ilpatch/Assembly-ACT.p2.dll（6 处注入）在流中逐字节存在 = True
```

⇒ 「补丁 bundle 里装的就是打好 6 处注入的 Assembly-ACT.dll」这件事**已在交付物层面被证明**，不再依赖记忆或推断。

**另一个必须记的风险（真机当前状态）**：这台真机 `files/yoo` 只有 13 M ⇒ **13 G 热更资源尚未下载**。
用户登录后游戏会拉官方 manifest 并可能重下 Dll bundle（官方 bundle 与我们的大小不同）。
- `LvmengPayloadProvider.copyAsset` 的跳过条件是「大小相等」，官方包大小不同 ⇒ **下次启动会自动覆盖回来（自愈）**；
- 但 `CacheRecords.snapshot` 目前是「缺才写」，游戏重下后快照里可能残留旧 CRC ⇒ 若观察到「功能时好时坏」，
  下一步就把 snapshot 改成**每次都更新该 bundle 记录**（Java 改动，需重编 dex）。

**注入特征串核对（L1 静态口径，2026-10-28 02:15）**：在交付包的 DLL 里 `s_starryTick` / `StarryLvmeng` 各 1 处，
而**原版 `rt.dll` 里 0 处** ⇒ 注入确实落地且具备区分力（`starry_lvmeng.dll`/`starry_state.txt` 这类字面量两者都是 0，
说明它们运行时拼装，不能用作判据）。

### 9.9 2026-10-28 02:2x 雷电（已备料环境）验功能链：包本身自洽

真机还停在登录页（13 G 未下载），所以用**雷电这台 13 G 资源还在的环境**验证「v22 包 + 载荷」的运行侧：

| 观测 | 值 |
|---|---|
| 菜单/授权 | `ModBridge: auth game_id synced from native: lmskzs` ✅ / `StarryMagic: MOD 已就绪 tag=Lvmeng ready=1` |
| 载荷 | `LvmengPayloadProvider: skip（已存在）`（幂等） |
| bundle **未被覆盖** | `…/f8/f8124a60…/__data` 仍 **13,212,366 B**，mtime 00:23（早于本次启动） ⇒ 游戏没有重下覆盖我们的补丁 |
| 游戏自动更新了快照 | `CacheRecords.snapshot` mtime 02:18，内容：`DCER` + `f8124a60…` + `…/__info` + `…/__data` + `crc=89c26790` + **size=0x00c91fac=13,212,366** ⇒ 记录的是**我们的文件大小** |
| 热更程序集加载 | `Unity: ACT.LocalLoader.GameStart:onExtendSuccess(String, String)` ⇒ `Assembly-ACT.dll` 从该 bundle 加载**成功**（若我们改坏了 IL，这里会失败） |
| 进程存活 | 外部笼子（诊断用）下 6 min+ 稳定，`[gone] tid=… status=0` 只是线程正常退出 |

**注意 snapshot 的 crc 字段**：游戏写的是 `89c26790`（官方 manifest 里该 bundle 的 CRC），而文件是我们的（大小 13,212,366 已被记录）。
说明 YooAsset 的缓存判据主要看**大小/记录存在**，不按 manifest CRC 重算文件 ⇒ **我们的补丁不会被校验掉**。这解释了为什么雷电上 5 项功能当时是通的。

**结论**：直装包在「已验证环境」里自洽可用；真机只差 13 G 资源下载与用户登录这一步。

### 9.10 2026-10-28 02:27 v23/v24：两处工程加固（真机 13 G 下载风险 + 路径解析 bug）

1. **载荷强制刷新（v23）**：`LvmengPayloadProvider.copyAsset` 原来「大小相同就跳过」，一旦真机下载官方 bundle 覆盖我们的，
   靠"下次启动才自愈"仍有一局是坏的。改成**每次启动都重写**（13 MB，代价可忽略）。
   - smali 侧：`if-eqz v5, :cond_4c` → `goto :cond_4c`（强制走复制分支）；Java 源同步改为 `if (false && …)` 并注释。
   - 实测：`LvmengPayloadProvider: asset lvmeng/__data -> …`（不再是 `skip（已存在）`），真机/雷电均生效。
2. **`findApkPath` 截断 bug（v24）**：`/proc/self/maps` 里同时有 `base.apk` 和 `base.apk!classes4.dex` 两种映射名，
   旧代码取整行 ⇒ 得到 `…/base.apk!classes4.dex`。这**就是真机首次运行 `extract: open apk failed 2` 的根因**
   （打开这个伪路径必然 ENOENT）。改为在 `"/base.apk"` 末尾截断。
   - 实测：`apk=/data/app/~~Yp-PiQU5…/base.apk`（干净），`fgspoof=ON`、`spoof file already present (179649718 B)`。

**当前交付候选 = `08_output/绿梦/_import_20261028/（v24 已不在磁盘；现存候选 lvmeng_v25/v32）`**（assemble → zipalign → apksigner 三连，包内 so 158,800 B 带 SIGSYS 诊断）。

### 9.11 2026-10-28 02:3x v24 正式包落地（含脚本偏离记录）

| 项 | 值 |
|---|---|
| 正式包 | `08_output/绿梦/绿梦_1.0.5_starry-releaseSigned.apk`（370,704,022 B） |
| md5 | `087ab5fa0896c404cac378d57a2d8c8a` |
| sha256 | `478f723db17de11020991155814e368d0fae4306b39e75e2a24fd4edfb3f92b0` |
| 签名 | `CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN`（`apksigner verify --print-certs`） |
| 对齐 | `zipalign -c -p 4` 通过 |
| 08 目录 | `08_output/绿梦/`：正式包 + `icon_绿梦_192.png`（无 delivery 目录） |

**脚本偏离（必须记）**：`finalize_game_delivery.sh` 是 mac 专用（依赖 `md5_of`/`sha256_of` 函数与 POSIX 路径，
且不接受 `E:\…` 形式），在本机直接 `command not found` ⇒ 本轮**手工做等价动作**（复制 + 计算 md5/sha256 + 建 delivery 目录 +
放入使用说明），未改脚本。下次要么给该脚本补 Windows 垫片，要么继续手工等价并记录。

**JNI 一致性**：`pre_install_gate.sh --cpp … --dex …` 因我们的注册表不是 `regOne` 形式而 SKIP；
真正的证据是运行期 `StarryMagic: [桥接] RegisterNatives 19/19 全部成功`（真机与雷电都出现）⇒ dex 声明与 so 注册表一致。

### 9.12 2026-10-28 02:4x 注入点 IL 级核对（`ilpatch dump`，交付 DLL）

| 核对项 | 结果 |
|---|---|
| 引导挂点 | `Hello::Run` **IL_0000: call Hello::StarryBootstrap()`（注入在入口方法**首条指令**）✅ |
| 引导三件套 | `Hello::StarryBootstrap()` (IL=40) / `Hello::StarryTryLoad(String)` (IL=75) / `Hello::StarryFileLog(String)` (IL=26) 全在 ✅ |
| 程序集引用 | `Assembly-ACT` 引用 `StarryLvmeng 1.0.0.0`（原版无）✅ |
| 战斗挂点（抽样） | `CompAbility` → `call Starry.Mod::OnSetHP(ACT.Battle.CompAbility)` ✅；`CompMotion` → `call Starry.Mod::OnRecoverMotion(ACT.Battle.CompMotion)` ✅ |
| `StarryFileLog` 目标 | `/data/data/com.zhuziplay.lvmengnew.op/files/starry_loader.log`（与 Provider 落 dll 的 `/data/user/0/<pkg>/files/` 同一目录）✅ |

**运行时探针（下次装机直接用）**：
- `starry_loader.log` 里出现 **`[boot]`** 行 ⇒ `Hello.Run` 已执行、IL 引导落地（之前 01:00 那次有 `[boot] enter` → `[load] ok`）；
  本轮各次启动该文件只有原生 fgspoof 行、**无 `[boot]`** ⇒ 还没进到 `Hello.Run`（=没进游戏），与"停在登录页"一致。
- 再往下看 `starry_log.txt` 的 `stage2 init dir=… hp=… spd=…` ⇒ `Mod.Init` 读到状态并接管功能。
