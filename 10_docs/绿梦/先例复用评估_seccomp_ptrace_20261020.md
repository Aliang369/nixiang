# 绿梦 FairGuard：「seccomp / ptrace 抓 killer PC」先例复用评估（2026-10-20）

- 性质：**纯评估 + 只读真机取证**。未改游戏、未装机、未重启进程（硬约束 #5/#6/#7 未触碰）。
- 设备：`f9a116a2`（root = KernelSU `u:r:ksu:s0`，uid=0 已验证）；内核 `6.1.141-android14-11-o-g9f7acd01cdbc`；`ro.build.version.release=16`。
- 采信的前置事实（不重复推导）：FairGuard「加密」= zlib、35s 闪退已绕过、剩余 101–157 s 异步 `tgkill` 信号阶梯 SIGILL→SIGTRAP→SIGKILL 杀 UnityMain、cert 比较/tampered 标志未定位、Frida 注入即死、**ptrace ATTACH 不被反制**。
- 本轮新增实测：`04_scripts/绿梦/seccomp_probe.c`（4 模式）已在真机跑通，见 §2。

---

## 0. 结论（先看这段）

**先例可复用，但要换两处：换 syscall（94 → 131）、换安装位置（mod 内晚装 → so 的 JNI_OnLoad 早装）。**

1. 霓虹深渊2 r20c 的 seccomp 骨架（`nhsy2Mod.cpp` §⑦.6h，L4612–4807）**结构完全可移植**：BPF 只差 syscall 号与 args 匹配；`ResolvePcToModule()`（L4633–4657）、`EgRec` 记录环（L4659–4670）、`OnSigsys`（L4682–4702）、`InstallExitGroupGuard`（L4756–4807）可整段抄。
2. **本机实测证明这条路在绿梦的机器上成立**：`SECCOMP_RET_LOG` 的 audit 记录真的进 logcat，且 **audit 的 `ip=` 与 SIGSYS 的 `pc` 逐位相同**（两次独立运行同偏移 `0x54fe4`）⇒ 只要打日志这一档，就能拿到 killer PC，**不需要 ptrace、不需要注入器、不需要 Frida**。
3. 推荐顺序：**A1（RET_LOG 画像）→ A2（RET_TRAP+SIGSYS 拿 lr/fp 并中和）→ B（ptrace 精准补刀）**；**C（现有 mprotect 注入器造 SIGSEGV 读墓碑）弃用**——墓碑 pc 是受害者 UnityMain，不是凶手，原理上拿不到 killer PC，且现有实现的 stop 配对会被目标自己的异步信号打乱（这就是「返回值垃圾」的直接解释）。
4. 附带收益：`process_vm_readv` 通道（`memdump_vm.c`，35 行、零注入）可以**直接读绿梦进程内存**，用来抓 FairGuard 运行期解密的字符串、以及定位 tampered 标志（这比追 killer PC 更贴「根源」）。列为 D 路（配套，不是替代）。
5. 主要未知：FairGuard 是否检测 seccomp（`Seccomp_filters` 计数 1→2 已被实测证明是可见的），**无法用 strings 否定**——`libFairGuard.so` 字符串表在盘上加密（20,741 条 strings 里几无可读标识符，`UND` 具名导入只有 `environ/sigaction/sigprocmask/tan` 4 个），必须靠 D 路运行期解密后检索。

---

## 1. 目标 syscall 号（aarch64，本机核准）

来源：NDK 27 `sysroot/usr/include/asm-generic/unistd.h`（本地实读行号）：

| nr | 名称 | 头文件行 | 在绿梦里的角色 |
|---|---|---|---|
| 94 | `exit_group` | :133 | nhsy2 拦的那个（35s 闪退已用 NOP 绕过，本任务不需要） |
| **131** | **`tgkill`** | :182 | **首选**：`tgkill(tgid,tid,sig)`，能精确打单线程 → 与「只死 UnityMain」现象吻合 |
| 130 | `tkill` | :181 | 同上但无 tgid；native 代码少见，顺手带上 |
| 240 | `rt_tgsigqueueinfo` | :303 | `pthread_sigqueue`/带 siginfo 的排队信号；阶梯若用 SA_SIGINFO 可能走这条 |
| 129 | `kill` | :180 | 只能整进程；若真凶用它，`args[0]==pid` 匹配即可 |
| 138 | `rt_sigqueueinfo` | :191 | 整进程版排队信号，兜底 |
| 424 | `pidfd_send_signal` | :373 | 内核 ≥5.1 才有；本机 6.1 支持，真凶理论上可绕开 tgkill 用它 → 兜底必挂 |

**选型**：主挂 **131**，同一份 BPF 里把 **130 / 240** 一并挂（成本≈0，分支而已），**129 / 424 作兜底**。
理由：阶梯是「精确杀 UnityMain 一个线程」⇒ 只可能是 `tgkill` 系；`kill` 会让整个进程立刻没，与观察不符。

**参数位置各不相同**（BPF 必须按 nr 分支取参，不能「随便看 a0」）：

| nr | 目标线程 | 信号号 |
|---|---|---|
| 131 | `args[1]` | `args[2]` |
| 130 | `args[0]` | `args[1]` |
| 129 / 138 | `args[0]`（pid） | `args[1]` |
| 240 | `args[1]` | `args[2]` |
| 424 | —（fd） | `args[1]` |

> 主线程 `tid == pid` ⇒「目标 == UnityMain」在 BPF 里就是 `args[1]==getpid()`（或 `args[0]==getpid()`），装过滤器时把 pid 常量烘焙进指令即可。

---

## 2. 本机实测（本轮新增证据，独立探针进程，不碰游戏）

探针：`04_scripts/绿梦/seccomp_probe.c`（模式 1=RET_LOG / 2=RET_TRAP+SIGSYS / 3=TSYNC / 4=tgkill 参数过滤），
二进制 `/data/local/tmp/seccomp_probe`。

| # | 实测结论 | 原始证据 |
|---|---|---|
| F1 | **RET_LOG 真的落 logcat**（auditd 转发，非 dmesg） | `10-20 03:53:20.191 … I auditd : type=1326 audit(0.0:1315): … comm="seccomp_probe" exe="/data/local/tmp/seccomp_probe" sig=0 arch=c00000b7 syscall=173 compat=0 ip=0x7a7c35efe4 code=0x7ffc0000` |
| F2 | **audit 记录里没有 `a0/a1/a2`**（只有 sig=0/arch/syscall/compat/ip/code）⇒ 不能从日志读「目标 tid / 信号号」；参数判别只能在 BPF 内做，「命中/不命中」本身就是判据 | 同上 |
| F3 | **audit 的 `ip` == SIGSYS 的 `pc`**：模式 1 `ip=0x7a7c35efe4` − libc_base `0x7a7c30a000` = `0x54fe4`；模式 2 `pc=0x7a0dd79fe4` − base `0x7a0dd25000` = `0x54fe4`，两次独立运行同偏移 | 两次独立运行输出 |
| F4 | **RET_TRAP+SIGSYS 能拿 pc/lr/x8**，且 `pc += 4`+`x0=-EPERM` 后线程可继续执行、不崩 | `hits=3 pc=0x7a0dd79fe4 pc_off=0x54fe4 lr=0x6401bca298 x8=0xad` → 三次调用返回 -1 后 `DONE` |
| F5 | **tgkill 参数过滤精确有效**：`args[1]==victim_tid && args[2]==SIGUSR1` 命中；同函数 `tgkill(...,0)` 正常放行；**信号根本没送达目标线程** | `tgkill(victim,0) -> 0`；`tgkill(victim,SIGUSR1) -> -1`；`victim_got_sig=0 hits=1 x0=0x7346 x2=0xa x8=0x83` |
| F6 | **TSYNC 跨线程有效**：4 线程进程 `rc=0`，4 个 tid 全部被拦（100 次命中来自 4 个不同 tid） | `TSYNC rc=0` + `hits=100 tids: 29518 29516 29515 29517 …` |
| F7 | **`Seccomp_filters` 计数可见**：同进程从 `Seccomp:0 / Seccomp_filters:0` 变 `Seccomp:2 / Seccomp_filters:1` ⇒ 过滤器「个数」是可读的检测面 | `BEFORE/AFTER` 两行 `/proc/self/status` |

补充（同轮只读取证）：`/proc/sys/kernel/seccomp/actions_logged` 与 `actions_avail` 在 shell **和** `su -c`（ksu 域）下都是 `Permission denied` ⇒ 该 sysctl 在本机不可读，**也无法通过它确认 RET_LOG 是否被 admin 关掉**——但 F1 已实测证明本机默认就是记的（`code=0x7ffc0000` = RET_LOG 生效）。
当前设备 **无任何 app 进程在跑**（`ps -A | grep -c " u0_"` = 0）⇒ 绿梦进程内 `Seccomp_filters` 基线（预期 1）留给下一轮实测。

---

## 3. 三条路（+ 前置 D）性价比对比

| 路 | 拿到什么 | 要注入吗 | 对 FairGuard 的扰动 | 成本 | 本机证据 | 判定 |
|---|---|---|---|---|---|---|
| **A1** `RET_LOG` | killer 线程 `comm`、进程 pid、**killer PC（audit ip）**、命中的 syscall nr | 要（进程内装过滤器，so 里几十行） | **零行为变化**（syscall 照常执行、信号照常送达），只多一条 audit | 1–2 h | F1/F2/F3/F7 | **推荐①（先跑这个）** |
| **A2** `RET_TRAP`+SIGSYS | 上面再加 **lr / fp / sp / 栈字 / tid**，并可**就地中和**（信号不送达 ⇒ UnityMain 不死） | 要（同一份 BPF，换 action + 装 handler） | 中：kill 阶梯被拦，FairGuard 可能升级/看返回值 | 2–4 h | F4/F5 | **推荐②** |
| **B** `PTRACE_SEIZE`+`PTRACE_SYSCALL` | 每次命中的**全寄存器**、可反复采样、`PTRACE_GETSIGINFO` 还能看 `si_pid`（谁在发信号） | 不要（纯外部小工具） | 中高：全线程 per-syscall 停会把 Unity 拖到不可玩；`TracerPid` 可被读（但实测不被反制） | 3–6 h | 霓虹 r20b 设计（:79）+ 本仓无实现 | **推荐③（A1 指名 tid 后只 trace 那一个）** |
| **C** mprotect 注入 → SIGSEGV → 墓碑 | 只有**受害者**的崩溃现场（UnityMain 的 pc/栈），**拿不到凶手** | 要（现有 `fg_mprotect.c`） | 高（在目标栈写 svc、改 pc） | ≥4 h（还得先修注入器） | `fg_mprotect.c:64-108` 现有缺陷 | **弃用** |
| **D** `process_vm_readv` | 任意可读内存段：运行期解密字符串、tampered 标志、代码字节 | **不要**（root 外部读取，无 attach） | 极低（进程无感） | 0.5 h（工具已存在） | `memdump_vm.c` 35 行现成 | **前置/配套（不是替代）** |

一句话：**A1 最便宜且已实测可行；A2 顺带把「101–157 s 必死」变成「不死」，从而解锁所有后续动态分析；B 是 A2 拿不到 lr 时的补刀；C 原理上就拿不到 killer PC。**

---

## 4. 可复用源码定位与行号（逐块判断「能直接抄吗」）

### 4.1 `mod菜单/starry-mod-menu/app/src/main/jni/Games/nhsy2Mod.cpp` §⑦.6h（L4612–4807）

| 块 | 行号 | 内容 | 移植判断 |
|---|---|---|---|
| 头文件 | :41-46 | `sys/prctl.h` / `sys/syscall.h` / `linux/seccomp.h` / `linux/filter.h` / `linux/audit.h` / `stddef.h` | **照抄**（NDK 27 齐全） |
| 设计注释（四档语义） | :4612-4630 | 为什么裸 svc 只能在内核边界拦、TSYNC 一次覆盖全线程 | **照抄**，把 `exit_group` 改 `tgkill` |
| `ResolvePcToModule()` | :4633-4657 | 地址 → `模块+文件偏移 [段权限]`，自带 `[anon]` 分支 | **一字不改直接抄**（绿梦 killer 极可能落在匿名桩页，:4650 的 anon 分支正好用上） |
| 记录结构 `EgRec` / 环 / 原子 | :4659-4670 | pc/lr/fp/sp/tid + 16 个栈字，8 条上限 | **照抄**，`kEgRecMax` 可保持 8 |
| `ParkTrappedThread()` | :4675-4677 | `wfe` 永久停住该线程（第 3 档） | **备用**：绿梦先试「pc+=4 放行」，只有桩页 svc 后是 0（udf）才需要 park |
| `OnSigsys()` | :4682-4702 | 异步信号安全地只写定长数组 + 原子 + 读本线程栈 | **改 3 处**：① 记录 `x8`（nr）与 `x0..x2`（参数），② 第 2 档改 `pc+=4`（只置 `x0` 会回头执行 svc 之后的字节），③ 档位沿用原子 |
| `FlushExitGuardRecords()` | :4704-4754 | 守护线程下一拍格式化 + `/proc/self/task/<tid>/comm` + 栈上候选返回地址 | **照抄**，日志前缀换成绿梦的 |
| `InstallExitGroupGuard()` | :4756-4807 | prop 门控 → sigaction(SIGSYS) → no_new_privs → 7 条 BPF → TSYNC → 失败退回单线程 | **改 2 处**：① BPF 扩到 nr 集合 + args 匹配（约 25–32 条），② TSYNC 在「JNI_OnLoad 早期单线程」场景可省；晚装则保留并**把失败返回的 tid 记进日志**（见 §6） |

### 4.2 `skill/re-implement/anti-cheat-pattern.md`「模式五」（L119–178）

四轮失败教训（:145-150）是绿梦**直接继承**的硬规则：
- **拦 93 会让线程从 svc 之后继续执行落进 `udf`，整进程陪葬** ⇒ 绿梦对应风险：若真凶走匿名桩页且 svc 后是 0，必须 park（与 4.1 备用档一致）；
- **`RET_ERRNO` 不行（留 destroyed-mutex 二次崩溃），必须 TRAP**；
- **只杀/只停该线程**，别动别人；
- **每类调用独立计数**（共用计数器会被首个命中吃掉、栈不打印，反而掩盖真凶）⇒ 绿梦要给 131/130/240 各留独立计数，才能一眼看出阶梯走的是哪条。

### 4.3 `04_scripts/蜀汉传卧龙篇/memdump_vm.c`（全文 35 行，L1–35）

- 实现：`process_vm_readv(pid, local, 1, remote, 1, 0)`，1 MB 分块，读失败 `GAP` + 零填充（:23-28）。
- **能不能直接读绿梦目标进程 cert 比较区？能**：`process_vm_readv` **不 attach**（不产生 `TracerPid`、不触发 ptrace 反制），只要 `ptrace_may_access` 通过（同 uid 或 root/CAP_SYS_PTRACE）——本机 `su -c id` = `uid=0` 已验证。
- 限制：① `PROT_NONE`/未映射段读不到（GAP 零填，别当「数据是 0」）；② 只能读**当前已映射**内容；③ 读不到其它 uid；④ 大段建议只 dump 目标模块数据段 + anon 可写段，别整进程。
- 用途：抓 FairGuard 运行期解密的字符串（`Seccomp`、`/proc/self/status`、证书比较常量）、tampered 标志前后 diff、核验「本地 so 偏移 ↔ 运行期字节」是否一致。

### 4.4 `04_scripts/绿梦/fg_mprotect.c`（L64–108）——为什么「远程 svc 未正确执行、返回值垃圾」

四处结构性缺陷（不改它，但结论落档，避免下一轮再花时间）：
1. **`PTRACE_ATTACH`（:64）**：attach 的第一个 stop 是 **SIGSTOP 信号投递停**，代码没消费就 `PTRACE_SYSCALL`（:90），后面每一对 stop 都可能错位；标准做法是 `PTRACE_SEIZE` + `PTRACE_INTERRUPT`。
2. **没有 `PTRACE_O_TRACESYSGOOD`**：syscall-stop 与普通信号停都是 `SIGTRAP`，裸 `waitpid` 分不出来。
3. **目标环境本身就在异步发信号**（FairGuard 的 tgkill 阶梯 + ART 线程挂起）：任一信号投递停插进「进入/退出」之间，配对就**永久**错位 ⇒ 这就是「PTRACE_SYSCALL 入口/出口仍垃圾」的直接解释，不是 syscall 号写错。
4. 只 attach 主线程、在 `sp-16` 写 svc（:75-81）后无条件 `pc=slot`：中途被信号打断时 svc 可能从未执行，读到的是别人留下的 `x0`。

⇒ **结论**：C 路要么整体重写成 SEIZE 版（那时已等价于 B 路），要么放弃；**它拿不到 killer PC 是原理问题，不是 bug**。

---

## 5. 绿梦适配步骤（最小可执行）

### 5.1 A1：RET_LOG 画像（零行为变化，先跑这个）

1. **落点**：`libstarry_magic.so` 的 `JNI_OnLoad`（native 壳，进程早期**单线程** ⇒ 免 TSYNC，之后新建线程自动继承；也避开与 FairGuard 初始化竞态）。**不要**从热更 C# 装（硬约束 #12 前提：C# 不能 DllImport；且 SIGSYS handler 绝不能用托管回调）。
2. **门控**：`__system_property_get("debug.starry.lvmeng_sigguard")`，默认 `0`（关）；`1`=RET_LOG，`2`=TRAP 记录，`3`=TRAP 停住。与 nhsy2 的 `debug.starry.nhsy2_exitguard` 同构，便于回滚。
3. **BPF**（约 10 条起）：
   - `arch == AUDIT_ARCH_AARCH64` 否则 ALLOW；
   - `nr == 131` → 比对 `args[1] == getpid()` 且 `args[2] ∈ {4,5,9}`（SIGILL/SIGTRAP/SIGKILL）→ `SECCOMP_RET_LOG`；
   - 同结构再挂 130 / 240（各 3 条）；其余 ALLOW；
   - 参数常量在装的时候烘焙（pid 运行期才知道）；
   - ⚠ 别一上来挂全部 nr（ART 线程挂起也用 tgkill，会刷满 audit）；窄档零命中再退回**宽档**（只按 nr、不看 args）做一次画像。
4. **判读**：`adb shell logcat -b events -d | grep 1326` → 拿 `ip`（=killer PC，F3 已证）、`comm`、`syscall`；再用 maps 把 ip 换算成 `模块+RVA`。
5. **记录**：一次冷启跑满 180 s，锁 101–157 s 窗口；预期窗口内出现 **1 次**（阶梯首击 SIGILL）。
6. **停止条件**：拿到 ip+RVA 即达成；窗口内零命中 → 转宽档一次；仍零命中 → 阶梯不走这几个 nr，回看宽档画像。

### 5.2 A2：TRAP + SIGSYS（拿 lr/fp/栈字，顺带中和）

1. 同一份 BPF，action 换 `SECCOMP_RET_TRAP`；`sigaction(SIGSYS, SA_SIGINFO|SA_NODEFER)` + **`sigaltstack`**（FairGuard 有 `sigaction`/`sigprocmask` 导入，阶梯很可能在信号上下文里，栈可能很小）。
2. handler 内**只做**：读 `pc/lr(x30)/fp(x29)/sp/x8/x0..x2` + 16 个栈字 → 写定长数组 + 原子；**禁止** malloc/fopen/打日志。
3. 行为：先 `uc->regs[0] = -EPERM; uc->pc += 4;`（F4 实测可继续执行）。**只有**日志显示「pc 落在匿名 r-xp 页 且该页 svc 之后为 0」时，才切 nhsy2 的 `ParkTrappedThread` 档（:4675）。
4. 守护线程下一拍用 `ResolvePcToModule` 打印：`lr`=调用者（真凶本帧）、`pc`=svc+4、`fp`+栈字做人工回溯。
5. **自检**：装完回读 `sigaction(SIGSYS, NULL, &old)` 确认 handler 还是我们的（防 FairGuard 覆盖）；每档一行「已就位/尚未命中」日志（照 `hook-rules.md` §25 装配自检惯例）。
6. **中和的收益**：信号不再送达 UnityMain ⇒ 101–157 s 死点消失 ⇒ 后续动态分析（内存 diff、页监视、二次验证）全部解锁。**代价**：FairGuard 可能升级（已观察到的阶梯就是证据），所以 A2 必须**每一档都记首击 pc**，别只记 SIGILL。

### 5.3 B：ptrace 精准补刀（A 拿到 tid/comm 之后）

1. NDK 27 编动态 PIE（`-fPIE -pie`；`-static` 在本机 bionic 下被拒，已有先例结论）。
2. `PTRACE_SEIZE`（不停止目标）→ `PTRACE_INTERRUPT` 拿 stop → `PTRACE_O_TRACESYSGOOD|TRACECLONE|TRACEEXIT` → `PTRACE_SYSCALL`。
3. `waitpid` 判 `WSTOPSIG == (SIGTRAP|0x80)` 才是 syscall-stop；`PTRACE_GETREGSET(NT_PRSTATUS)` 读 `x8`(nr)/`pc`/`lr`/`sp`，命中 {129,130,131,138,240,424} 就打印 + 解析 maps。
4. **只 trace A1 指名的 tid**（其它线程不 attach 或不 SYSCALL），否则 60 线程 per-syscall 停会把 Unity 拖死。
5. 附带：在 UnityMain 的**信号投递停**上 `PTRACE_GETSIGINFO` 读 `si_pid`/`si_code` ⇒ 直接看到「谁在发信号」，与 seccomp 交叉验证。

### 5.4 D：process_vm_readv 取证（同日可做，零注入）

```bash
# 先看映射，挑 "libFairGuard.so 的 rw- 段 + [anon:...] 可写段"
adb shell su -c 'grep -E "libFairGuard|anon" /proc/<pid>/maps' | head
# 编译：$CC -O2 -o memdump_vm 04_scripts/蜀汉传卧龙篇/memdump_vm.c
adb push memdump_vm /data/local/tmp/
adb shell su -c '/data/local/tmp/memdump_vm <pid> /data/local/tmp/fg_rw.bin <start_hex> <len_hex>'
adb pull /data/local/tmp/fg_rw.bin
strings -a fg_rw.bin | grep -iE 'seccomp|/proc/self/status|tracerpid|signature|cert'
```
用途排序：① 判 FairGuard 是否检测 seccomp（最关键的未知）；② tampered 前后段 diff；③ 校验本地 so 偏移与运行期字节一致。

---

## 6. FairGuard 检测面与风险（逐条给「实测/未测」）

| 检测面 | 状态 | 缓解 / 判断 |
|---|---|---|
| `/proc/self/status` 的 `Seccomp_filters` 计数 1→2 | **实测存在**（F7：同进程 0→1） | 无法隐藏；只能靠 D 路确认 FairGuard 是否读它。⚠ 先测绿梦进程基线（预期 `Seccomp:2 / Seccomp_filters:1`） |
| `Seccomp: 2` 模式字段 | **不构成新信号**（Android app 因 zygote 本就是 2） | 无需处理 |
| tgkill 返回 `-EPERM` 而非 0 | 未测 | A1 档（RET_LOG）零行为变化，先跑它；A2 才冒这个险；必要时用「TRAP 后手工重放 syscall」消除差异（BPF 可用 `instruction_pointer` 排除自身重放桩，避免递归） |
| FairGuard 自己装 SIGSYS/SIGILL handler 或 `sigprocmask` 屏蔽 | **有迹象**（`libFairGuard.so` 的 `UND` 具名导入里就有 `sigaction`、`sigprocmask`） | 装完回读 `sigaction` 自检；安装点放在 FairGuard 初始化**之后**或做二次重装；handler 用 `sigaltstack` |
| `TSYNC` 失败会返回「首个无法同步的 tid」 | 未测（本机自测 rc=0） | **失败本身就是情报**：说明某线程自己装了过滤器（FairGuard 反调试）⇒ 记日志并改单线程安装 + 逐线程补装 |
| 阶梯升级（只拦 SIGILL → 见 SIGTRAP → 见 SIGKILL） | **已观察到** | BPF 一次挂 {4,5,9}；F5 已证 SIGKILL 经 tgkill 也能被拦（syscall 不执行即信号不送达） |
| 过滤器**不可卸载**（进程生命周期内永久生效） | 内核语义 | 必须 prop 门控 + 先 RET_LOG；装错只能重启进程（重启前要报告，硬约束 #6） |
| 子进程继承（WebView 等 fork 子进程） | 设计上无害 | 烘焙的是**本进程 pid**，子进程 pid 不同 ⇒ 不误伤 |
| 静态 `strings` 判「不检测 seccomp」 | **不可用** | 本轮实测：20,741 条 strings 几无可读标识符，`UND` 具名导入仅 4 个 ⇒ 必须 D 路运行期检索，**不许**凭 strings 0 命中就下结论 |

---

## 7. 未闭环

1. **FairGuard 是否检测 seccomp / 是否读 `/proc/self/status`** —— 字符串加密，静态判不了；需 D 路运行期 dump 后检索。
2. **绿梦进程内 `Seccomp_filters` 基线未实测**（设备当前无 app 运行；预期 1，需下轮冷启时 `su -c grep Seccomp /proc/<pid>/status`）。
3. **安装时机 vs FairGuard 初始化顺序**：`libstarry_magic.so` 的 `JNI_OnLoad` 与 FairGuard 谁先？若 FairGuard 先装了自己的过滤器，我们的 TSYNC 会失败（同时暴露其反调试），需实测。
4. **真凶的 svc 形态未知**：若经匿名桩页且 svc 之后为 0 ⇒ 必须 park 档（副作用：若它持锁会拖住别人）；若内联在模块里 ⇒ `pc+=4` 即可。
5. **阶梯到底用 131 还是 240**（带 siginfo 排队）——A1 首个命中即可回答；若都不中，回看宽档画像。
6. **拦截成功后 FairGuard 是否改用非 syscall 手段**（直接写内存、网络上报、server 端踢）——未知，A2 跑满 10 min 才能观察。
7. **硬约束 #12 的边界**：把 seccomp 闸/诊断代码放进 `libstarry_magic.so` 是否越过「native 壳不 hook 业务、唯一适配点是执行层」？**建议按「诊断构件、不进交付包、默认 prop=0 关」处置，并请用户确认一次**。
8. **B 路全线程 trace 的性能影响未测**（预期不可接受，需按 5.3 只 trace 单 tid）。

---

## 8. 本轮证据与产物

| 产物 | 说明 |
|---|---|
| `04_scripts/绿梦/seccomp_probe.c` | 4 模式机制验证器（RET_LOG / RET_TRAP+SIGSYS / TSYNC / tgkill 参数过滤），NDK 27 一行编译；已入库 |
| `/data/local/tmp/seccomp_probe`（设备） | 探针二进制，可复用复测；与游戏无关，不影响 FairGuard |
| 本文档 §2 表 | 7 条实测事实 + 原始 audit 行 |

复测命令（只读，安全）：
```bash
CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android26-clang
$CC -O1 -fPIE -pie -o /tmp/seccomp_probe 04_scripts/绿梦/seccomp_probe.c
adb push /tmp/seccomp_probe /data/local/tmp/ && adb shell chmod 755 /data/local/tmp/seccomp_probe
adb shell /data/local/tmp/seccomp_probe 1   # RET_LOG，随后 logcat -b events -d | grep 1326
adb shell /data/local/tmp/seccomp_probe 4   # tgkill 参数过滤 + 信号不送达
```
