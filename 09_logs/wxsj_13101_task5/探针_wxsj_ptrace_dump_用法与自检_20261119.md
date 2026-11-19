# wxsj_ptrace_dump（arm64 静态 ptrace 取证探针）— 用法与自检记录 2026-11-19

## 1 交付物
| 项 | 值 |
|---|---|
| 源码 | 04_scripts/无限升级/wxsj_ptrace_dump.c |
| 构建脚本 | 04_scripts/无限升级/build_wxsj_ptrace_dump.sh（编译 + PT_TLS 对齐修补 + 校验） |
| 本地产物 | 08_output/无限升级/_diag/wxsj_ptrace_dump，449,504 B |
| 指纹 | md5 `2ab88dec6b9781f4a1dab3ff899d8360` / sha256 `475a6db6f6e3cbaae395c96aea3d3309c2d97c65d77d6704aa03b90544e19bf8` |
| 设备位置 | /data/local/tmp/wxsj_ptrace_dump（755，已 push） |
| 类型 | ELF64 aarch64 **静态**、stripped |

## 2 用法
```
wxsj_ptrace_dump -p <pid|tid> [--all] [--addr <hex>] [--timeout <sec>] [--out <path>]
  -p        目标 pid（单线程模式可直接给 tid）
  --all     遍历 /proc/<pid>/task/* 逐线程 PTRACE_SEIZE（崩溃线程不一定是主线程时必用）
  --addr    额外 dump 一段地址（0x100 B）
  --timeout 无致命信号多久自动释放（默认 120s，0=不超时）
  --out     输出文件（默认 /data/local/tmp/wxsj_ptrace_<pid>.txt）
```
典型：`su -c '/data/local/tmp/wxsj_ptrace_dump -p <pid> --all --timeout 120'`（需 root：CAP_SYS_PTRACE）

落盘内容：regs x0–x30/sp/pc/pstate、pc 的 libil2cpp 偏移、sp..sp+0x800、x19-0x40..x19+0x200、x19+{0x28,0x30,0x40,0x1c8,0x1d0,0x1d8}、[x19+0x40] 与 x24 各 0x40、可选 --addr、sp..sp+0x1000 内 libil2cpp 候选返回地址。

## 3 设计（v2，2026-11-19 事故后重写）
- **PTRACE_SEIZE + PTRACE_INTERRUPT**，全程**不制造 SIGSTOP**。
- 致命信号（SEGV/BUS/ABRT/ILL/FPE）→ 用该 tid 的寄存器取证 → 崩溃线程按原信号 DETACH（照原样死），其余线程 INTERRUPT→DETACH(0)。
- **只转发真实信号**；自己 INTERRUPT 产生的 stop（`PTRACE_EVENT_STOP`）标记为内部 stop，绝不回投（这是 v1 会把 SIGSEGV 当「凭空产生」的根源之一）。
- 释放后逐线程核 `/proc/<pid>/task/<tid>/stat` 第 3 字段，仍有 T/t 就补 `kill(pid,SIGCONT)`，最多 3 轮。
- 释放前后各打一行 `[states] total=.. S=.. R=.. T=.. D=.. Z=..`，并列出仍处 T 的 tid。

## 4 v1 事故根因（260 线程停 T、游戏被冻住）
- v1 用 ATTACH + CONT，释放前必须让线程进 ptrace-stop 才能 DETACH ⇒ 先 `SIGSTOP`；
- 而 SIGSTOP 触发的是 **group-stop**，内核在 group-stop 下**丢弃** `PTRACE_DETACH` 携带的 SIGCONT；
- 结果：线程停在 T，进程不跑也不死。lead 手工 `kill -CONT 26638` 才恢复 —— 与现象完全一致。
- v2 改用 SEIZE 后不再有这一步，INTERRUPT 给的是干净 ptrace-stop（EVENT_STOP），DETACH 直接放行。

## 5 自检（全部在**无关进程**上做，未 attach 任何 com.xingzuodaoshi 进程）
测试目标：自建多线程程序 t_threads（线程名 UnityMain/UnityWkNN，pause 循环）+ `sleep 300`。

**A 正常释放（3 线程）**
```
[states] attach 前: total=3 S=3 R=0 T=0 D=0 Z=0 other=0
[seize] 成功 SEIZE 3/3 个线程（SEIZE 不改变运行态，无需 CONT）
[states] seize 后: total=3 S=3 R=0 T=0 D=0 Z=0 other=0
[timeout] 3s 内无致命信号 ⇒ 释放全部线程（INTERRUPT→DETACH，不制造 SIGSTOP）
[detach] 3 个线程已 DETACH（crash_tid=0 按信号 0 回投），0 个已自然退出
[states] detach 后: total=3 S=3 R=0 T=0 D=0 Z=0 other=0
外部复核：31637:S 31638:S 31639:S / TracerPid: 0
```

**B 规模 65 线程（接近事故规模的一半）**
```
[states] attach 前: total=65 S=65 R=0 T=0 D=0 Z=0 other=0
[seize] 成功 SEIZE 65/65 个线程
[detach] 65 个线程已 DETACH（crash_tid=0 按信号 0 回投）
[states] detach 后: total=65 S=65 R=0 T=0 D=0 Z=0 other=0
外部复核：65 行状态全是 S，TracerPid: 0
释放后 `kill -TERM` 立刻生效（已正常响应SIGTERM并退出）⇒ 没被冻住
```

**C 非主线程崩溃（关键场景）**
```
[hit] 捕获致命信号 11 @ tid=31638 comm=UnityMain ⇒ 取证
================ CRASH (pid 31637, tid 31638, comm=UnityMain) ================
[detach] 3 个线程已 DETACH（crash_tid=31638 按信号 11 回投）
目标最终状态：DEAD（按原样死，与自然崩溃一致）
```

**D 真实非致命信号必须原样转发、不得误判崩溃**
```
[sig] tid=32078 真实信号 28 ⇒ 原样转发        （SIGWINCH，默认忽略）
结束后目标：State S，存活 ⇒ 转发正确、未当崩溃处理
另一组用 SIGUSR1（默认动作=终止）验证：转发后进程按默认动作正常终止 ⇒ 说明信号没被吞
```

**E 单线程 sleep**：total=1 S=1 → detach 后 S，TracerPid 0

## 6 已知边界
- 需要 root（`su -c`）；无 CAP_SYS_PTRACE 时 SEIZE 会 EPERM，工具会打 errno 原文。
- SELinux 严格模式可能拦 ptrace；若 EACCES/EPERM，需要按设备策略处理（未在本轮验证）。
- 只观察、不阻断：崩溃仍会按原信号发生（这是刻意设计，便于与 tombstone 对照）。
- 若进程在 attach 期间被外部 SIGKILL/退出，工具打 `[exit]` 并结束，不会挂住设备。

## 7 设备临时件
自检用的 t_threads / selftest_*.txt 已删除；仅保留 /data/local/tmp/wxsj_ptrace_dump。

## 8 v3 增补（2026-11-19 15:09，误抓广告 SDK 之后）

**触发事故**：挂在 pid 26638 上 6 秒内抓到 `tid=26974 comm=anythink_schedu` 的 SIGSEGV，pc=0xaad4e34c **不在 libil2cpp**，进程并未死（广告 SDK 自己处理了该信号）⇒ 假阳性；且工具抓完即退出，真正的秘境崩溃没人盯。

**新增开关**
| 开关 | 作用 |
|---|---|
| `--pc-range <hex>-<hex>` | 只 dump pc 落在该区间的崩溃；不满足则打 `[skip] tid=.. pc=.. 不在范围` 并**原样转发信号继续盯** |
| （默认） | pc 范围自动取 `/proc/<pid>/maps` 里 libil2cpp 的 **r-xp** 段 |
| `--any-pc` | 关闭 pc 过滤（慎用） |
| `--skip-comm <名>` | 追加要跳过的线程 comm 子串（大小写不敏感，可重复） |
| （默认名单） | anythink / pangle / unityads / applovin / mintegral / topon / bytedance / toutiao / sigmob / klevin / ksad / gdt / csj |
| `--no-skip-comm` | 清空内置广告名单 |
| `--keep` | dump 一次后**继续值守**（同一 tid 只 dump 一次，重复命中打 `[skip] 已 dump 过`） |

**fail-safe**：若拿不到 libil2cpp 的可执行段（进程还没加载 / pid 给错），**不猜**：打印一次 `[warn]` 后对所有崩溃事件一律 skip；要无过滤 dump 必须显式 `--any-pc`。

**最小验证（活进程 10s，不依赖用户点击）**
```
[states] attach 前: total=5 S=5 R=0 T=0 D=0 Z=0 other=0
[cfg] pc 过滤: [0x7000000000, 0x7000001000)（libil2cpp 可执行段）
[skip] tid=1844 comm=anythink_schedu 命中 --skip-comm 名单（sig=11 pc=0x76d8979308）⇒ 原样转发继续盯
[skip] tid=1845 pc=0x76d8979308 不在范围 [0x7000000000,0x7000001000)（comm=UnityWk01 sig=11）⇒ 原样转发继续盯
[hit] 捕获致命信号 11 @ tid=1845 comm=UnityWk01 pc=0x76d8979308（在 libil2cpp 范围）⇒ 取证
[keep] dump 完成，继续值守（本次 dump 第 1 次）
[states] detach 后: total=5 S=5 R=0 T=0 D=0 Z=0 other=0
外部复核：5 行状态全 S，TracerPid: 0，无残留探针/测试进程
```
结论：广告线程信号不再导致退出、pc 范围两个方向都对、超时后释放干净不留 T。

**v3 产物**：453,120 B / md5 `b6811d31ab38016dd38920f04ed800bf` / sha256 `ff499ed32f3b5b61c57279f17edb3cfe9527ad5a0ada277c997925b5283656cf`

**建议真实取证命令**
```
su -c '/data/local/tmp/wxsj_ptrace_dump -p <pid> --all --keep --timeout 180 --out /data/local/tmp/wxsj_ptrace_game.txt'
```
（先 force-stop 冷启拿当前 pid；`--keep` 保证 dump 后仍在盯，`--skip-comm` 默认已覆盖广告 SDK）


## 9 v4 增补（2026-11-19 15:20，补「调用者指令页」）

**动机**：libil2cpp 代码页按需解密，未执行过的页事后读出来是**全 0**（lead 实测 base+0x58e000 全 0、base+0x29d6000 有合法指令）⇒ 调用者那页只能在崩溃瞬间读到。

**新增输出（都写进同一个 txt 的 `---- e) code ----` 段）**
1. `pc-0x2000 .. pc+0x2000`（16KB，32 B/行带地址）；
2. `sp..sp+0x1000` 内落在关注区间（默认 libil2cpp r-xp，或 `--pc-range` 指定）的候选返回地址，各 `ret-0x40..ret+0x40`（128B），去重后最多 32 个。

**二进制落盘**：目录 `<out 去掉 .txt>_code/`，文件名 `pc_<addr>.bin`（16384 B）与 `retNN_<addr>.bin`（128 B，addr 为该块起始 = ret-0x40），可直接喂 capstone。

**其它细节**
- 读取走 `read_mem` → `process_vm_readv`（失败才退 /proc/pid/mem），不用 PEEKTEXT 逐字读。
- 某块全 0 时标题行附 `[警告: 全 0 ⇒ 该页未解密/未执行过]`，避免把 0 当有效指令。
- `--pc-range` 同时充当「候选返回地址判定区间」（便于在没有 libil2cpp 的进程上自测）；对游戏用默认 maps 自动范围即可。

**最小验证（无关进程，t_threads + 故意 SIGSEGV，用 libc 段模拟关注区间）**
```
[cfg] pc 过滤: [0x72cb31f000, 0x72cb3c8000)
---- e) code ----
[code] 二进制落盘目录: /data/local/tmp/v4b_code
---- e) pc-0x2000 .. pc+0x2000 ---- 0x72cb3b3308 (+0x4000)
  +0x0000 0x72cb3b3308 | 01 00 00 d4 1f 04 40 b1 00 94 80 da ...
---- e) ret#0 0x72cb35e230 -0x40..+0x40 ----
---- e) ret#1 0x72cb350a4c -0x40..+0x40 ----
---- e) ret#2 0x72cb35e174 -0x40..+0x40 ----
```

文件：pc_72cb3b3308.bin 16384 B；ret00/01/02_*.bin 各 128 B；od 抽查 pc 首 16 B = 01 00 00 d4 1f 04 40 b1 …（真实指令，非全 0）。
释放后：4 S、TracerPid: 0，无残留进程。

**v4 产物**：455,104 B / md5 b80656426c242765902484e26a2b1bd1 / sha256 69d714b4c1b1ebd18519c80cd478e140109d8359fd2b77c75ecf8f55c2d3db2d（本地与设备 md5 一致）

## 10 v5 增补（2026-11-19 15:36，入口断点 + 多段 pc 过滤）

### 10.1 新增 `--brk <hex 绝对地址>`（入口断点）
1. SEIZE 全部线程后，选一个已停线程用 `PTRACE_PEEKDATA/POKEDATA` 在目标地址写 **BRK #0（0xD4200000）**，保存原 8 字节字（4 字节槽位）；POKEDATA 失败退 `/proc/<pid>/mem` 写。
2. 任一线程命中（SIGTRAP 且 pc ∈ {addr, addr+4}）：立刻打印 x0–x30/sp/pc/pstate，**突出 x30 = 调用者返回地址**，并 dump `[x0]-0x40..+0xc0`（0x100 B，含 +0x1c/+0x28/+0x30 关键字段）与栈顶。
3. 打印完**先恢复原指令**（回读比对 ✓）再 DETACH 全部线程；`--brk-continue` 则恢复→单步→重新布点，继续值守（可连续抓）。
4. **释放路径（超时/退出）也会先撤断点**——见 10.3 血案。

### 10.2 pc 过滤改多段（修 v4 只取第一段）
- v4：只记录 maps 里第一个含 x 的 libil2cpp 段（实测那段只有 0x3000，真实代码段被漏）。
- v5：收集**全部** r-xp 段（上限 16），`in_range()` 逐段判定；`[cfg] pc 过滤: N 段 …` 会把每段都打出来。
- `--pc-range` 仍然叠加为一个额外区间，并同时作为候选返回地址判定区间。

### 10.3 自检中踩到并修掉的坑（重要）
- 现象：`--brk-continue` 跑满超时后目标 **DEAD**。
- 根因：释放路径没有撤断点，代码页里仍留着 0xD4200000；目标恢复执行即吃 SIGTRAP（默认动作=终止）；叠加 DETACH 时把 SIGTRAP 当「真实信号」回投，双重致死。
- 修法：① `detach_all()` 开头先 `interrupt_and_wait()` + `brk_restore()`；② 释放时 SIGTRAP 一律不回投（`if (s == SIGTRAP) s = 0;`）。

### 10.4 自检（自己写的函数指针循环程序 t_brk，-O0）
```
程序自报：pid=13300 fn=0x615870c858  lr=0x615870c910   ← __builtin_return_address(0) 真值
[cfg] 入口断点: 0x615870c858 (BRK #0)，命中后 重新布点继续值守（--brk-continue）
[brk] 布点 OK @0x615870c858：字@0x615870c858 原=a9017bfdd10083ff 新=a9017bfdd4200000（BRK #0）
============ BRK HIT #1 (pid 13300, tid 13300, comm=t_brk) ============
  **x30 = 0x000000615870c910   <- 调用者返回地址（caller LR）**     ← 与 lr 真值逐位一致
命中=18（--brk-continue 连续抓）
[brk] 释放前撤销断点（在 tid 13300 上恢复原指令）
[brk] 恢复原指令：回读=a9017bfdd10083ff 期望=a9017bfdd10083ff ✓
释放后目标：State S、TracerPid 0（存活，未留 T）
```

### 10.5 v5 产物
460,544 B / md5 `6436db54ebaec024892e781eb2733fb1` / sha256 `157cb4bc69751898ce10251a7000d1aa58c558bf9730cd07b3aa0c088e625571`（本地 = 设备）

## 11 v6 增补（2026-11-19 15:43，BRK 命中补 dump 调用者指令）

### 11.1 新增输出（BRK 命中时）
- `[cfg] x30 = 0x...（页 0x...，页内偏移 0x...）`；
- `---- x30-0x40 .. x30+0x40（调用者指令） ----` 128 B hexdump；
- `---- x30 所在页（4KB） ----` 整页 hexdump；
- 二进制落到 `<out 去 .txt>_brkcode/`：`x30_<addr>.bin`（128 B，addr = x30-0x40）、`page_<addr>.bin`（4096 B，addr = 页基址）。

### 11.2 两处小修
- **释放后只剩崩溃线程在 T 时不再当失败**：`[recover] 仅剩崩溃线程 tid=X 停在 T —— 它正在走信号投递/死亡路径，此时处于 group-stop 属不可恢复且预期，不是失败，也不再补 SIGCONT`（其余线程在 T 才会补 SIGCONT，并打印 tids 列表）。
- **顺序保证**：BRK 命中一律「先 dump 完 → 再恢复原指令」，恢复失败（进程已在死亡路径）只打提示，不影响已拿到的寄存器/代码页数据。

### 11.3 最小验证
```
程序自报 lr=0x5fb7d9d910
  **x30 = 0x0000005fb7d9d910   <- 调用者返回地址（caller LR）**
[cfg] x30 = 0x5fb7d9d910（页 0x5fb7d9d000，页内偏移 0x910）
---- x30-0x40 .. x30+0x40（调用者指令） ---- 0x5fb7d9d8d0 (+0x80)
---- x30 所在页（4KB） ---- 0x5fb7d9d000 (+0x1000)
_brkcode/: page_5fb7d9d000.bin 4096 B、x30_5fb7d9d8d0.bin 128 B
[brk] 恢复原指令：回读=a9017bfdd10083ff 期望=a9017bfdd10083ff ✓
释放后：State S、TracerPid 0
```

### 11.4 v6 产物
461,440 B / md5 `c1ebe0a55e0015d672ce1276a4e93843` / sha256 `abd0b44c30591863fa03f37735c1d2d7b56a1d6f862bafee60e44b0f9f16438e`（本地 = 设备）

### 11.5 未实测项
- 「只剩崩溃线程在 T」的分类日志在自检中未触发（自检进程没崩），逻辑已实现，**待真实现场验证**。
