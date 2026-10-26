# 绿梦 · 方案 A 第二刀：mode 2（RET_TRAP+SIGSYS）与 mode 3（Dobby 观测）实测报告

> 日期：2026-10-20 04:18–04:29　设备 f9a116a2（OPD2404 / Android 16 / KernelSU）
> 承接：10_docs/绿梦/killer_pc_seccomp_A_20261020.md（第一刀 RET_LOG）
> 本轮唯一变量：tgkill 闸的**动作档位**（RET_LOG → RET_TRAP → Dobby 观测），其余包内容逐字节不变

## 0. 结论（先看这段）

1. **mode 2 实现并跑通**：SIGSYS 处理器取到 pc/lr/fp/sp/tid/16 个栈字，且用「预开 fd + 纯手写 hex + write()」
   在信号上下文里直接落盘（进程随后被打死也不丢记录）。
2. **抓到的唯一 tgkill 发送者是崩溃上报器自己**：**libCrashSight.so + 0xe300**
   （两次 mode 2 冷启 + 一次 mode 3 冷启，3/3 一致；sig 分别为 4/4/11，即「Forwarding signal N」的重新投递）。
   ⇒ **它不是 cert 比较点**，只是 CrashSight 把信号再抬高一次让进程按原信号死掉。
3. **FairGuard 的致命动作不是 tgkill，而是让解密后的 libil2cpp 影子副本里出现真故障**：
   - mode 2 首次冷启：\`signal 4 (SIGILL), code 1 (ILL_ILLOPC)\`，**pc = libil2cpp.so + 0x363b118（pc==lr）**
   - mode 3 首次冷启：\`signal 4 (SIGILL), code -6 (SI_TKILL)\`（16.4 s，转发链）；\`#00 = libil2cpp + 0x364a500\`
   - mode 3 第二次冷启：\`signal 11 (SIGSEGV), code -6 (SI_TKILL)\`；\`#00 = libil2cpp + 0x36356e8\`
   ⇒ 故障点稳定落在 **libil2cpp 0x3635xxx–0x364axxx（约 24 KB 窗口）**，且该区在 rwxp 解密影副本里
     （包内磁盘字节是密文，实测 dump 为高熵乱码）⇒ 这是下一代取证要盯的真地址。
4. **重要修正（推翻前一份文档的读法）**：崩溃报告 CRASH 块是在「重新投递之后」才打印的，
   所以块里的 \`code -6 (SI_TKILL)\` 描述的是 **CrashSight 的重投**，不是 FairGuard 的原始动作。
   第一刀文档 §0.2/§0.3 里「audit ip=libc 那条是杀手」的表述应改读为「那条是 CrashSight 的重投」。
5. **RET_TRAP 档对本案是自我否定**：拦住 SIGILL 投递后，FairGuard 改走「真非法指令」这条路
   （mode 2 见到 ILL_ILLOPC，mode 1/3 见到 SI_TKILL）⇒ 想在 tgkill 上抓 FairGuard 的调用点，TRAP 档不成立；
   要抓只能零行为观测（mode 3 那种钩子）。

## 1. 实现（都在 \`Games/LvmengSeccomp.cpp\`，game-private，未碰公共文件）

| 档 | prop 值 | 行为 | 本轮结论 |
|---|---|---|---|
| 不装 | 未设/0 | 与未加闸一致 | — |
| RET_LOG | 1 | BPF 对 129/130/131/240/424 全族返回 LOG，syscall 照常执行 | 第一刀已用 |
| RET_TRAP | 2 | BPF **只对 sig==SIGILL(4)** 的投递返回 TRAP（tgkill args[2] / tkill·kill·pidfd args[1]），SIGSYS 记寄存器后把 x0 置 -EPERM | 抓到的是 CrashSight 重投 |
| SPY | 3 | 不装 seccomp；Dobby 钩 libc \`syscall()\`（记录 nr∈{129,130,131,240,424} 的调用者）与 \`tgkill()\`（只记 sig==SIGILL） | 抓到的是 CrashSight 重投 |

mode 2 关键设计（两点都是被上一版实测打出来的）：
- **只拦 SIGILL**：第一版全族 TRAP + EPERM 让游戏在 **5.8 s 静默退出**（FairGuard 启动期探针被 EPERM，
  \`am_proc_died\` 04:18:35.829，无墓碑、无记录）；
- **信号上下文 write() 落盘**：第一版靠轮询线程刷 logcat，5.8 s 那次一条都没来得及留 ⇒ 改成预开
  \`files/starry_lvmeng_seccomp.log\`，处理器内手写 hex + 单次 \`write()\`（不 snprintf/malloc/fopen）；
  轮询线程每 500 ms 快照 \`files/starry_lvmeng_maps_last.txt\` 供死后归一。

## 2. 构建与装机

| 用途 | so md5 | 包 | 包 md5 |
|---|---|---|---|
| mode 2 全族 TRAP（弃用） | 1aeb885550e25b53de21e2c2e0ba7d7a | 绿梦_seccomp_m2.apk | ec50a83a6ba5a2d6d0b054cea36d608b |
| mode 2 sig=SIGILL + 落盘 | 6ef171cb680925516d378267ee03649b | 绿梦_seccomp_m2c.apk | 45c02038d40ca9ef5850d828bd58fcce |
| mode 3 Dobby 观测（未过滤，弃用） | e6bd9a328483d2394082251dda7a412b | 绿梦_tgk_spy.apk | 4d2605c8f0b3550a942b9fcd9890a48a |
| **mode 3 观测（只记 SIGILL）＝当前装机** | **dcb95cdfcf03a139eb1a3d26cb860e1e** | **绿梦_tgk_spy2.apk** | **e936ad13d791930027db68391f632bb9** |

装机链仍是外科（未用 surgical_lvmeng_1.0.5.sh 的官方底包，理由见第一刀文档 §3）：
\`fg_so_swap.py\` 只换 \`lib/arm64-v8a/libstarry_magic.so\` → \`surgical_equal_length_resign.sh --no-pad\`；
底包 = \`绿梦_seccomp_audit.apk\`（含 NOP 后的 libFairGuard，md5 5f4b4556…）；
每个包都用 \`install_starry_apk.sh\` 同签覆盖装（Success，未卸载、未清数据，闭环核对 so md5 一致）。

## 3. 真机证据（逐字）

### 3.1 mode 2（sig=SIGILL）：抓到 CrashSight 的重投
\`\`\`
10-20 04:22:02.073 W/StarryLvmeng( 4398): ==== poll tgkill 拦下 #1 tid=4773 comm=UnityMain nr=131 sig=4 累计=1 ====
10-20 04:22:02.073 W/StarryLvmeng( 4398):   CALLER lr(x30)=0x71d6264300 -> libCrashSight.so+0xe300 [r-xp @0x71d6256000]
10-20 04:22:02.073 W/StarryLvmeng( 4398):   stub pc=0x755ed32fe4 -> libc.so+0xaffe4 [r-xp @0x755ed22000] / fp(x29)=0x7552e6bd90 -> [anon]+0x3ed90 / sp=0x7552e6bc60
10-20 04:22:02.073 W/StarryLvmeng( 4398):   stack: 16 words 全在 libc/[anon]
\`\`\`
落盘原文（信号上下文 write，文件 \`files/starry_lvmeng_seccomp.log\`）：
\`\`\`
HIT #0x1 tid=0x12a5 nr=0x83 sig=0x4 lr=0x71d6264300 pc=0x755ed32fe4 fp=0x7552e6bd90 sp=0x7552e6bc60 x0=0x112e x1=0x12a5 x2=0x4 stk=0x0 0x71d631f9f4 0x71d63203e0 0x736840c7c0 0x736840c840 0x6ad67be9 0xf4087 0x6ad67be9 0x71a61 0x74b0dee4c0 0xb400007355aa94a0 0x71d631fa28 0x71000012a5 0x71d631fe68 0x0 0x7552e6bda0
\`\`\`
（x0=tgid 4398、x1=tid 4773、x2=4=SIGILL、nr=131 与 lr 完全自洽；16 个栈字里 0x71d631f9f4/0x71d63203e0/0x71d631fa28/0x71d631fe68
全在 libCrashSight 段内 ⇒ 调用栈整条都在上报器里。）
第二次冷启（同 so）：\`HIT #0x1 tid=0x18c9 nr=0x83 sig=0x4 lr=0x733aa3c300 …\`，同样是 libCrashSight+0xe300。

### 3.2 mode 2 同轮的真正致命事件（不是 tgkill）
\`\`\`
10-20 04:22:01.303 E/[CrashSightReport-Native]( 4398): get signal 4
10-20 04:22:02.002 E/CRASH ( 4398): signal 4 (SIGILL), code 1 (ILL_ILLOPC), fault addr --------
10-20 04:22:02.002 E/CRASH ( 4398):     lr  0000007334c60118  sp  000000736840b690  pc  0000007334c60118
10-20 04:22:02.002 E/CRASH ( 4398):       #00 pc 000000000363b118  libil2cpp.so
\`\`\`
⇒ **pc==lr、code=ILL_ILLOPC**：这是一条**真非法指令**（不是被投递的信号），位置 libil2cpp+0x363b118。

### 3.3 mode 3（Dobby 观测，零行为变化）
\`\`\`
10-20 04:26:13.426 I/StarryLvmeng( 7867): tgkill SPY（mode=3，零行为变化）：syscall hook rc=0 orig=0x7572eca000 / tgkill hook rc=0 orig=0x7572eca020
10-20 04:28:15.103 W/StarryLvmeng( 9176): SPY#1 syscall(nr=131) caller=0x71cfcd3300 -> libCrashSight.so+0xe300 [r-xp @0x71cfcc5000] [caller_tid=9376 comm=UnityMain] tgid=9176 tid=9376 sig=11
10-20 04:28:14.454 E/[CrashSightReport-Native]( 9176): get signal 11
10-20 04:28:15.106 E/CRASH ( 9176): signal 11 (SIGSEGV), code -6 (SI_TKILL), fault addr --------
10-20 04:28:15.106 E/CRASH ( 9176):       #00 pc 00000000036356e8  libil2cpp.so
\`\`\`
未过滤那版（绿梦_tgk_spy）证明另一件事：**ART 自己的 tgkill(sig=24/30) 极密**（8 s 内 60 条，
caller 全在 libc+0x9b4b8 = pthread 内部）⇒ 观测必须按 sig 过滤，否则名额被吃光（第一次就漏掉了崩溃那一发）。

## 4. 判据对照

| 判据 | 结果 |
|---|---|
| 给出「谁调的 tgkill」= 模块+RVA | ✅ **libCrashSight.so + 0xe300**（lr/fp/sp/16 栈字齐备，3 次冷启复现） |
| 其中是否是 FairGuard 的 cert 比较点 | ❌ 不是：它是崩溃上报器把信号重新抬高（Forwarding），与 tamper 判定无关 |
| 拿到 FairGuard 的 tgkill 调用点 | ❌ **未取得**，且本轮证明「它不用 tgkill 收尾」（见 §0.3/§0.5） |
| 拿不到时是否留证据 | ✅ 六个包的 md5、三档 prop 行为、逐字日志、落盘的 HIT 记录与 maps 快照路径全部落档 |
| 是否破坏既有逻辑 | 否：mode 2 两次运行都走到 CrashSight 上报后才结束；mode 3 完全零行为变化 |
| 当前设备状态 | prop 已复位 **0**；装机包 = 绿梦_tgk_spy2.apk（so dcb95cdf…）；游戏进程在最后一轮崩溃后已退出 |

## 5. 未闭环 / 下一步（按性价比排序）

1. **真凶就在 libil2cpp 0x3635xxx–0x364axxx 这段解密影副本里**（三次故障点：0x363b118 / 0x364a500 / 0x36356e8）。
   建议：root 侧在 ~10 s 起对 \`/proc/<pid>/mem\` 的 il2cpp rwxp 段做**周期小窗快照**（现有
   \`04_scripts/绿梦/fg_mem_probe.sh\`、\`memdump_vm.c\` 可复用），与包内密文对照，看这一段是
   「被 FairGuard 覆写成垃圾」还是「解密密钥错⇒解出垃圾」；两者对应的修法完全不同。
2. 想直接拿「谁写的」：ptrace 硬件观察点（watchpoint）打在 libil2cpp rwxp 影副本的该地址上，
   谁写谁就是凶手（本仓 ptrace 注入器已有骨架，但上一轮远程 mprotect 未打通）。
3. 前一份文档 §0.2/§0.3 需按 §0.4 改写（CRASH 块的 code 字段描述的是重投，不是原始动作）。
4. 三档 prop 的手工复位纪律：mode 2 会把 tgkill 变成 EPERM、mode 3 会挂钩子，**用完必须
   \`setprop debug.starry.lvmeng_tgkillguard 0\`**（本轮已复位）。

## 6. 证据文件（本机 /tmp，未入库）

| 文件 | md5 |
|---|---|
| \`/tmp/lv3_main.log\`（mode 2 sig 过滤轮，含 CALLER/CRASH） | \`0bb9f464ad4a351ef3ac02ef75610a5a\` |
| \`/tmp/lv6_main.log\`（mode 3 过滤轮，含 SPY#1/SIGSEGV） | \`414d63c0be32cebd0ea257fb22c8c756\` |
| \`/tmp/lv3_seccomp.log\`（mode 2 落盘 HIT 原文） | 见 §3.1 |
| \`/tmp/lv5_main.log\`（mode 3 未过滤轮，ART tgkill 噪声样本） | — |
