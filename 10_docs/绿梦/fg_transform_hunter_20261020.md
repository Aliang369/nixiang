# 绿梦 · Phase 2：抓「滚动变换执行者」——三条路的实测结果（静态 / perf / 页写陷阱）

> 日期：2026-10-20 04:35–04:41　设备 f9a116a2
> 承接：10_docs/绿梦/il2cpp_win_flip_20261020.md（Phase 1：t≈12–17 s 的 3256 步长滚动变换）
> 结论一句话：**执行者仍未拿到**；但三条路的负结果 + 一个已修好的抓手（页写陷阱 mode 5）都落地了。

## 0. 结论一览

| 路 | 做法 | 结果 |
|---|---|---|
| 2B 静态 | 在 \`06_dumped/绿梦_20261016/fg_unpacked/libFairGuard_unpacked.bin\`（0x400FD0 B）里 capstone 全量反汇编找立即数 0xcb8/3256/0x32e，并扫数据侧 u32/u64 | **零命中**（0xcb8 u32/u64 计数均为 0；对照 0x1000 有 161 处）⇒ 步长不是载荷里的字面常量，是算出来的或在本镜像之外 |
| 2A' 免注入归因 | per-thread utime/stime 采样（新增 \`04_scripts/绿梦/lvmeng_thread_cpu.sh\`） | 主线程 13291 独占 ~20.4 s/21 s ⇒ 变换太稀疏，CPU 归因不可分辨 |
| 2A'' perf | 设备 **perf_event_paranoid = -1**、自带 /system/bin/simpleperf；t=8 s 起 \`simpleperf record -p PID -f 999 -g --duration 10\`（7225 样本） | libFairGuard.so 仅 **1.74%**、libunity 24.7%、libc 23%、kernel 36% ⇒ 采样法抓不到稀疏变换 |
| 2A 页写陷阱（本轮实现并装机） | \`LvmengSeccomp.cpp\` 新增 **mode 5**：把 il2cpp 影副本前三页 mprotect(PROT_READ\|PROT_EXEC)，写入者吃 SIGSEGV，处理器记 pc/lr/tid/si_addr 后立刻恢复 rwxp 并重布防 | **装机有、布防成功、本轮零命中**（原因见 §2） |

## 1. mode 5 实现要点（game-private，未碰公共文件）

- 触发：prop \`debug.starry.lvmeng_tgkillguard=5\`（0/1/2/3/4 各自的语义见 A/A2 文档，5 新增）。
- 布防：轮询线程每 50 ms 检查一次，拿到 \`libil2cpp.so\` 的 **rwxp** 段基址后 mprotect
  \`base+0x0000 / base+0x1000 / base+0x2000\`（Phase 1 实测被改字落在 +0xa10/+0xa78、+0x16c8/+0x1730、+0x2380/+0x23e8）。
- 处理器：命中页内的 fault ⇒ 记 pc（**执行写入的那条指令**）/lr/fp/tid/si_addr，立刻把三页恢复
  \`PROT_READ|PROT_WRITE|PROT_EXEC\` 让写入完成，再由轮询线程重布防（可连续抓）；fault 不在本页 ⇒ **转交原 SIGSEGV 处理器**（不吃真崩溃）。
- 产物：so \`450f9637ee1386f85c5d229acb9cd79c\`（2,149,744 B）；包 \`07_patched/绿梦/fg_test/绿梦_pagetrap.apk\`
  \`127ec717486730e0bf23ad69c258fde8\`（同签覆盖装 Success，未卸载未清数据）。

## 2. 本轮为什么零命中（实测证据）

\`\`\`
04:40:06.406 I/StarryLvmeng: 页写陷阱（mode=5）：SIGSEGV 处理器已装（原=0x7574c837cc）
04:40:06.944 W/StarryLvmeng: tgkill 闸：SIGSYS 处理器被覆盖(现=0x7574c837cc) -> 已抢回(第 1 次)
04:40:10.764 I/StarryLvmeng: 页写陷阱：base=0x731622c000 布防 0x731622c000/0x731622d000/0x731622e000
04:40:12.463 I/[CrashSightReport-Native]: get system java classes success      ← CrashSight 在此前后装自己的信号处理器
04:40:18.886 E/[CrashSightReport-Native]: get signal 4                        ← 13.9 s 的伪造崩溃
04:40:19.475 E/CRASH: signal 4 (SIGILL), code -6 (SI_TKILL)
\`\`\`
1. **我们的 SIGSEGV 处理器被 CrashSight 覆盖**：6.9 s 的 SIGSYS"被覆盖→抢回"日志证明同一套处理器会在
   运行中被替换；SIGSEGV 我们只在 6.4 s 装了一次，12.4 s CrashSight 起来后就没再抢回 ⇒ 在 12–17 s 的
   变换窗口里，我们布的"只读页"即使被写，SIGSEGV 也落到 CrashSight 那边（本轮没出现 signal 11，说明
   这段窗口**根本没写成**，或被其吞掉）。
2. 因此 mode 5 还缺一环：**轮询线程必须同时抢回 SIGSEGV 处理器**（SIGSYS 已有该逻辑，SIGSEGV 漏了）。
   修法只有一行（把 \`sigaction(SIGSEGV,...)\` 加入 200 ms 重抢回），另加 \`mprotect\` 返回值日志以便确认布防真生效。

## 3. 判据对照

| 判据 | 结果 |
|---|---|
| (A) 变换执行者「模块+RVA」 | **未取得**（静态零命中、perf 无法分辨、页写陷阱本轮零命中且已定位缺失环节） |
| (B) 反汇编执行者 + 找调用者 | 未开始（无执行者地址） |
| (C) 未闭环 | 见 §4；但把"步长不是字面常量"和"信号处理器会被 CrashSight 抢走"两条写死，下一轮不再重复 |

## 4. 未闭环 / 下一轮最小动作（按性价比）

1. **mode 5 补 SIGSEGV 重抢回 + mprotect rc 日志**（约 10 行改动，一轮构建装机即可）→ 若变换窗口真在写，
   第一发就会被抓到，直接给出 WRITER pc（预期 libFairGuard.so 或某 [anon] JIT 页）。
2. 若仍零命中：说明"只读页"改变了行为（写入者可能先看权限）⇒ 改用 **in-process perf 硬件写断点**
   （\`perf_event_open(PERF_TYPE_BREAKPOINT, bp_type=HW_BREAKPOINT_W, sigtrap=1)\`；本机
   \`perf_event_paranoid=-1\` 已实测允许，且**不留 TracerPid**，比 ptrace 安全），对 base+0xa10/0xa78 各开一个。
3. ptrace 路线仍留作最后手段：\`PTRACE_SEIZE + NT_ARM_HW_WATCH\` 只能按**线程**布点（aarch64 仅 4 槽），
   而本进程线程数 >> 4，槽位会轮转丢样本；且 TracerPid 会被 FairGuard 看到（历史：frida attach 即 SIGKILL）。
4. 步长 3256 的来源：既然载荷里没有这个常量，优先怀疑它由 \`区域长度/槽数\` 算出（例如 0x40000/50 之类），
   下一轮可对 unpacked 镜像做「常量除法/移位 + 循环写」的模式扫描，而不是继续搜 0xcb8。

## 5. 复现

\`\`\`
# 静态
python3 -c "img=open('06_dumped/绿梦_20261016/fg_unpacked/libFairGuard_unpacked.bin','rb').read(); print(img.count((3256).to_bytes(4,'little')))"   # -> 0
# 线程 CPU 归因（免注入）
bash 04_scripts/绿梦/lvmeng_thread_cpu.sh --dur 22 --iv 0.3
# perf（root；paranoid=-1）
su -c 'simpleperf record -p <pid> -f 999 -g --duration 10 -o /data/local/tmp/lv.perf'
# 页写陷阱
adb shell su -c 'setprop debug.starry.lvmeng_tgkillguard 5'
\`\`\`

## 6. 补丁后复跑（04:43–04:45，SIGSEGV 重抢回已加）

改动：轮询线程每 50 ms 重装 SIGSEGV 处理器（对抗 CrashSight 在 ~12 s 的覆盖）；\`LvRearmTrap\` 打印 mprotect 返回值。
产物：so \`da2f80b0a3de8644b3aabdf61974b090\`；包 \`07_patched/绿梦/fg_test/绿梦_pagetrap2.apk\`
\`67bab0c2d57a75d29c4fd46c633f7c1d\`（同签覆盖 Success）。

\`\`\`
04:44:58.710 I/StarryLvmeng(19308): 页写陷阱：布防生效 mprotect rc=0/0/0（PROT_READ|PROT_EXEC）
04:44:58.710 I/StarryLvmeng(19308): 页写陷阱：base=0x7331a0d000 布防 0x7331a0d000/0x7331a0e000/0x7331a0f000
（t≈5.7 s 布防成功；到 t=26 s 仍 **零命中**，进程 19308 存活）
\`\`\`

结论：**布防确已生效（rc=0/0/0），但 12–17 s 窗口内没有写入打到前三页**。三种可能，按可能性排：
1. 该轮没有发生滚动变换（Phase 1 也有过"有变换但不崩"的轮，说明变换不是每轮必发）；
2. 变换落在其它页（轮 A/B 的相位是 +0xa10/+0xa78，也可能换相）；
3. 写入者用的是同一文件的**另一个映射**（写 r-x 视图不可能，但若它 mmap 了第二份 rw 视图，页陷阱抓不到）。

**下一轮决定性控制实验（顺序不能反）**：
① 自检——布防后由我们自己的线程向被布防页写一次同值字节：能抓到 ⇒ 机制有效，问题在"变换没发生/不在这几页"；
抓不到 ⇒ 处理器仍被 CrashSight 抢走（那就把重抢回频率提到 5 ms，并打印每次抢回时看到的 handler）；
② 自检通过后，把布防页扩到 8 页（+0x0000…+0x7000）并在 t=8–30 s 全程保持；
③ 若仍空，再上 in-process perf 硬件写断点（PERF_TYPE_BREAKPOINT/HW_BREAKPOINT_W，paranoid=-1 已实测允许、无 TracerPid）。


## 7. 位置纠正 + 自检通过（04:47）

纠正：Phase 1 的格点 0xa78/0x1730/0x23e8 是**相对窗口基址 base+0x3630000** 的，真实地址为
base+0x3630a78 / 0x3631730 / 0x36323e8 ⇒ mode 5 布防页改为 base+0x3630000/0x3631000/0x3632000。
产物：so \`ef61d2ae1e1c00a345a0978ac7eb9a72\`；包 \`绿梦_pagetrap3.apk\` \`dd0d8784bafe97715f03d7c02b434b27\`。

\`\`\`
04:47:46.179 I/StarryLvmeng: 布防生效 mprotect rc=0/0/0（base=0x7335230000 → 0x7338860000/…61000/…62000）
04:47:46.179 I/StarryLvmeng: 自检写入 0x7338860a78（同值 76a6c173）完成
04:47:46.232 W/StarryLvmeng: ==== 页写陷阱命中 #1：tid=20957 comm=ay.lvmengnew.op fault_addr=0x7338860a78
04:47:46.232 W/StarryLvmeng:   WRITER pc=0x736df0ac84 -> libstarry_magic.so+0x10c84
\`\`\`
⇒ **机制 + 位置双双确认**（自检写入被自己的 store 指令抓到，pc 落在我们 so 里，fault_addr 正是目标址）。

但 **t=6–28 s 内没有第二发命中**（真实变换未落到这三页），进程 20822 存活。判定：该轮窗口内没发生
滚动变换（与 Phase 1 中"有变换不崩/不总发生"一致），不是位置或机制问题。

下一步：把布防页扩到 8 页（base+0x3630000…+0x3637000，覆盖格点 0xa78…0x6fd0 的全部相位）并把观测窗拉到
t=8–40 s；若仍零命中再上 in-process perf 硬件写断点。


## 8. 8 页加宽 + 三轮连撞（04:49–04:52）—— 仍零命中

改动：布防页 3→8（base+0x3630000…+0x3637000，覆盖格点 0xa78…0x6fd0 全部相位），每轮保留自检。
产物：so \`90602319e67f5f361f65de7608bc3a37\`；包 \`绿梦_pagetrap4.apk\` \`07df5c396f208acbc002b68a93b1b3bc\`。

三轮（每轮 冷启→布防→等 40 s，force-stop 重来）：**均无真实 WRITER 命中**（自检命中被过滤，pc 在
libstarry_magic 里），三轮进程都存活。

判定：在「页权限=RO」的这三轮里，12–40 s 窗口内**没有任何写入**打到 base+0x3630000…+0x3637000。
与 Phase 1（不设陷阱时该段确实出现滚动变换）对照，最可能的解释变成：**变换被我们自己的只读页挡掉了**
（写入者先探测可写性 / 变换由"执行+写"耦合触发，RO 使其退化为不做），而不是"变换间歇"。
这与 §7 自检成功一起，说明该路线已把目标"逼停"，拿不到执行者。

**下一步（切换手段）**：in-process perf 硬件写断点（不改变页权限，因此不会逼停变换）：
\`perf_event_open(PERF_TYPE_BREAKPOINT, bp_addr=base+0x3630a78, bp_len=4, bp_type=HW_BREAKPOINT_W, sigtrap=1)\`，
溢出信号里记 pc/lr/tid；paranoid=-1 已实测允许、无 TracerPid。每个地址一个 event，aarch64 仅 4 个硬件槽，先打 1–2 个格点。


## 9. mode 6：in-process perf 硬件写断点（04:53–04:55）

实现（\`LvmengSeccomp.cpp\`，prop=6）：\`perf_event_open(PERF_TYPE_BREAKPOINT, bp_addr=base+0x3630a78,
bp_len=4, bp_type=HW_BREAKPOINT_W, sigtrap=1, exclude_kernel=1)\`，对 \`/proc/self/task/*\` 逐个线程开事件
（上限 32 ）；SIGTRAP 处理器记 pc（=写之后那条指令）/lr/tid，轮询线程归一模块+RVA。**不碰页权限**，
因此理论上不会像 mode 5 那样把变换逼停。
产物：so \`dca7a53d4210f6e8b7840989c8c5c374\`；包 \`绿梦_perfbp.apk\` \`6d1446f7664203976c36cbbbd23e5721\`。

本轮结果：**没有拿到任何 StarryLvmeng 日志**（连构造器的"tgkill 闸/页写陷阱"都没有），说明该次冷启的进程里
我们的 so 根本没被加载（pid 存在但日志为空）⇒ 这一轮的 perf 数据无效，不是"零命中"，是"没跑起来"。
下一步只需一次干净冷启复核（先确认 \`logcat | grep StarryLvmeng\` 有构造器日志，再看 perf 布点/命中）。


## 10. mode 6 干净重跑（04:56）：so 已加载，但 perf 布点全失败

\`\`\`
04:56:15.504 I/StarryLvmeng(25622): fg range ...   ← so 已加载（构造器日志齐全）
04:56:15.554 I/StarryLvmeng(25622): perf 写断点：addr=0x7334c75a78 布点成功=0 失败=48（线程数=0）
\`\`\`
⇒ 上一轮"无日志"只是那一轮没加载；本轮 **so 正常、基址与目标地址都对**（0x7334c75a78 = base+0x3630a78），
但 \`perf_event_open\` 对全部 48 个线程 **全部失败**（0 成功）。\`perf_event_paranoid=-1\` 虽允许，
SELinux（untrusted_app 域）仍会拦 perf_event_open，或 \`sigtrap\` 字段与设备内核不兼容（EINVAL）。
未记录 errno 是本实现的疏漏 —— 下一轮第一步就是把 errno 打出来（\`SE_LOGW("... errno=%d")\`），
再用 \`PERF_TYPE_HARDWARE\`+\`PERF_COUNT_HW_BREAKPOINTS\` 或去掉 sigtrap 改 mmap ring 复测。

结论：perf 直采执行者的路线在**未 debug errno 前不能判定失败**，但也拿不到 A 判据。


## 11. mode 7：v2 签名块页 PROT_NONE 抓 cert 比较读取者（04:57–05:00）

实现（prop=7，\`LvmengSeccomp.cpp\`）：
1. 轮询线程从 \`/proc/self/maps\` 找 \`base.apk\` 的 **r--s ∧ file offset 0** 那条映射基址；
2. \`v2 页 = map_base + 0xab3c000\`（§13.2 的 v2 块），\`mprotect(页, PROT_NONE)\`；
3. SIGSEGV 处理器：fault 落在该页 → 记 pc（读指令）/lr/fp/tid/si_addr → **立刻恢复 PROT_READ(r--s)** → 由轮询线程重布防；
   非本页 fault → 转交 CrashSight 原处理器；每次布防前重装 handler（对抗 CrashSight 覆盖），并有自检读（自己读一次该页确认在线）。
产物：so \`d822cb27e0870f4cce6af69764581b83\`；包 \`07_patched/绿梦/fg_test/绿梦_v2trap.apk\` \`0ab8e0360700a56a9a0212fe0d3e4012\`。

实测：装机后两次冷启（04:58:47 / 05:00:34）**logcat 里 StarryLvmeng/StarryMagic 行数为 0** ⇒ 该进程里我们的 so 没被加载
（连 mode 7 分支的"已起轮询线程"都没有），因此本档**未产生任何 v2 读取命中数据**，不是"零命中"。
同一症状在 §9 出现过一次、下一次干净冷启即恢复（04:56 那次 so 正常）⇒ 优先怀疑"装机后首次冷启不加载 so"的运行期偶发，
下一步：先 \`install_starry_apk\` 的 so md5 闭环核对 + 一次干净冷启并确认 \`logcat | grep StarryLvmeng\` 有构造器日志，
再等 v2 读取命中（预期 libFairGuard.so）。
