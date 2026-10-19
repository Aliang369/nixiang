# 绿梦：时空之声 — libFairGuard.so 签名校验定位（只读静态侦察）

- 阶段：用户拍板方案 2（定位并 patch `libFairGuard.so` 签名校验点）的 **第 0 阶段**。本文件只读侦察结论，**未改任何 so 字节、未重打包、未装机、未碰真机**。
- 日期 / 执行：2026-10-20 子代理会话（只读）。上游事实：`10_docs/_state/games/绿梦.md` 2026-10-19 条目、硬约束 #13。

## 0. 结论摘要（先看这一段）

1. **本 so 磁盘态无法静态定位校验逻辑**：`libFairGuard.so` 是**加壳样本**——文件 0x400..0x1F0000 是 ~2 MB 高熵（7.96~7.98）**加密载荷**，0x1F0000..0x400000 全零，只有 0x400000 之后约 121 KB 是**明文壳代码**（自带 mini-libc + 系统调用桩）。真正的引擎/校验逻辑在加密载荷内，明文区**零**处字符串、**零**处 ZIP/APK 解析常量。
2. 但仍拿到 **3 个可复核的候选点 + 1 条明确的运行期取证路径**：最可能是 **`exit_group` 系统调用桩 `0x42F100`**（"安静退出 0" 的唯一原语），其**唯一明文调用者 = `0x418184`**（函数 `0x418110`，比较魔数 `0x4FAF2AEB` 后 `exit_group(0)`）。
3. 因此「定位校验点」这一步**必须补一次运行期取证**（frida 挂 `0x42F100` 取 backtrace / dump 解密内存），静态到此为止。详情见 §5。

## 1. so 指纹

| 项 | 值 |
|---|---|
| 文件 | `06_dumped/绿梦_20261016/libFairGuard.so`（源 `/tmp/fg_so/lib/arm64-v8a/libFairGuard.so`） |
| 大小 | 4,477,792 B（0x445C60） |
| md5 | `7bee7f8b44d9106165707f26c33ffca6` |
| sha256 | `2fd5bf13c64e2e09414de4db2c590164257147e0e0e7bf2ad320d68631405fc8` |
| 架构 | ELF64 little-endian **AArch64**，类型 DYN，soname `libFairGuard.so`，entry `0x10B00` |
| NEEDED | `liblog.so libdl.so libz.so libstdc++.so libm.so libc.so`（**无 libart**，与已知事实一致） |
| 字节一致性 | 官方 `/tmp/lvmeng_apk/base.apk` 与重签 `07_patched/绿梦/fg_test/绿梦_fgtest_resign.apk` 内 `lib/arm64-v8a/libFairGuard.so` **md5 完全相同**（`7bee7f8b…`）⇒ 差异只在签名，so 未变 |
| 分析副本 | `06_dumped/绿梦_20261016/libFairGuard_an.so`（**仅修 section header 用于反汇编**，见 §2；原文件未动） |

## 2. 侦察方法与工具

1. **IG5**：`ig5_open`（engine=reverse，provider=ghidra，dbRevision 0）→ `n_funcs=9`、`partial=true`。后台作业两次失败（`IG5 analysis failed: IG5 operation cancelled before execution`），改用 `auto_analysis=false` 内联打开成功；`ig5_doctor` 正常（version 12.1.4 / pyghidra 3.1.0）。**本机当前 IG5 只能给出段视图，给不出可用伪代码**（原因见下一条）。
2. **本机工具面**：`llvm-readelf / llvm-objdump / llvm-nm`（`/opt/homebrew/opt/llvm/bin`），无 GNU `readelf`；`IG5` 的 `xrefs/scan/listing` 未在本会话工具面暴露，故 xref 用 objdump 反汇编 + 地址文本检索替代。
3. **关键技巧（可复用）**：本 so 的 **section header 是伪造的**——`.text` 声明 `addr=0x190 size=0x10`（两个 NOP），真实代码在 `0x413AB0` 之后。做法：复制一份、把第 7 个 section header 的 `sh_addr/sh_offset/sh_size` 改成 `0x413AB0/0x413AB0/0x1D8A0`、`sh_flags=ALLOC|EXEC`，即可 `llvm-objdump -d` 反汇编出 **32,069 条指令**。原始样本未改。
4. **熵/零率分段扫描**（python，每 64 KB）、**全文件字节扫描**（ZIP/DER/ASN.1 魔数）、`strings`（≥4/≥6）、**系统调用桩枚举**（`svc #0` 前一条 `mov x8, #imm` 配对）。

## 3. 关键结构事实（实测）

| 区间 | 大小 | 内容 / 判据 |
|---|---|---|
| `0x0..0x400` | 1 KB | ELF header + program headers（明文，parse 正常） |
| `0x400..0x1F0000` | 1,998,848 B | **加密载荷**，熵 7.96~7.98、零率 0；全文件搜不到任何 ZIP/DER/关键字明文 |
| `0x1F0000..0x400000` | 2,031,616 B | 全零（保留区） |
| `0x403190..0x4139E8` | ~66 KB | 明文表：`.dynsym`(0x61C8) `.dynstr`(0x7BFA) `.rela.dyn`(0x930) `.rela.plt`(0xF0) |
| `0x4139F0..0x413AB0` | 0xC0 | `.plt`，**10 个桩全部符号名 `tan`**（导入名被改名） |
| `0x413AB0..0x431350` | 0x1D8A0 ≈ 121 KB | **明文壳代码**（真实 prologue、有 mini-libc） |
| `0x431350..0x43172C` | 0x3DC | `.rodata`（988 B，内容是加密/编码表，无可读串） |
| `0x443000..0x444D20` | 0x1D20 | `.data`（表结构，运行期被 R_AARCH64_RELATIVE 填充） |

反静态分析特征（全部实测）：

- **符号表污染**：`.dynsym` 1,043 条，除 `tan/sigaction/sigprocmask/environ` 外符号名全为乱码（名字指向 `.dynstr` 中间位置，如 `tdc++.so`、`rGuard.so`）；两条符号地址 `0xFDCDB695` 为非法地址。
- **诱饵函数海**：`0x425A20` 起是成片的 `ret`（`C0035FD6`）sled，`.dynsym` 里约 1000 条 4 字节 FUNC 全指向此处 ⇒ 任何"导出函数清单"都不可信。
- **初始化表清空**：`.init_array`/`.fini_array`/`.data.rel.ro` 磁盘态全零；只有 78 条 `R_AARCH64_RELATIVE` 在加载期填值（指向 `0x413B0C/0x413B10/0x413B5C` 三个构造器及 `0x41EF60/0x427CDC…` 等函数指针）。
- **entry 指向载荷内**：`e_entry=0x10B00` 落在加密载荷里（so 不靠 entry，靠构造器）。
- **字符串全灭**：全文件 `strings` 仅 2,601 条且多为乱码；关键字命中数：`META-INF`=0、`.RSA/.DSA`=0、`base.apk`=0、`/data/app`=0、`signature`=0、`cert`=0、`APK Sig Block 42`=0、`getPackageCodePath`=0、`classes.dex`=0、`/proc/`=0、`JNI_OnLoad`/`Java_`=0、`fvv`=0、`com.zhuziplay`=0。
- **字节级**：`PK\x03\x04`/`PK\x05\x06`/`PK\x07\x08` 各 0 处；ASN.1 `2a864886f70d0101`(RSA OID)=0、`2a8648ce3d0301`(EC OID)=0；无 AES S-box / SHA-256 K / MD5 T 表。
  ⇒ **明文壳自己不做 APK/ZIP/DER 解析**；签名读取行为发生在解密载荷内（或通过 JNI 间接）。

### 3.1 明文壳的自带运行时（系统调用桩，x8 = asm-generic 号）

| EA | x8 | 系统调用 | 明文调用者 |
|---|---|---|---|
| `0x42EB00` | 0x38 | `openat` | 2（`0x42A488`、`0x42A61C`） |
| `0x42EC00` | 0x3F | `read` | 2 |
| `0x42ED00` | 0x40 | `write` | 1 |
| `0x42EE00` | 0x39 | `close` | 2 |
| `0x42EF00` | 0xE2 | `mprotect` | 0（明文内无直接调用 ⇒ 载荷内使用） |
| `0x42F000` | 0xA9 | 169（asm-generic `reboot`，**存疑**，见 §5） | 0 |
| **`0x42F100`** | **0x5E** | **`exit_group`** | **1（`0x418184`）** |
| `0x42F200` | 0x50 | `fstat` | 1 |
| `0x42F300` | 0x65 | `nanosleep` | 0 |
| `0x42F400` | 0x4F | `newfstatat` | 0 |
| `0x42F500` | 0x3E | `lseek` | 0 |
| `0x42F600` | 0xDE | `mmap` | 1 |
| `0x42F700` | 0xD7 | `munmap` | 1 |
| `0x42F800` | 0xE9 | `madvise` | 0 |

桩体形态（以 `exit_group` 为例，实测）：

```asm
42f100:  mov  x8, #0x5e        ; exit_group
42f104:  svc  #0
42f108:  cmn  x0, #0x1, lsl #12
42f10c:  cneg x0, x0, hi
42f110:  b.hi 42ea7c          ; errno 分支
42f114:  ret
```

**这与现场症状高度一致**：「加载页面 100% → 35 s → `Zygote: Process exited cleanly (0)`、无 tombstone、无 crash 记录」正是 `exit_group(0)`（不是 `abort`、不是信号崩溃）的指纹。`openat/read/mmap/mprotect` 齐备，也说明壳具备"读文件 → 解密 → 映射执行"的完整原语。

## 4. 候选校验点清单

> 说明：以下三项**都不是"已确认的签名比较点"**（比较点在加密载荷内）。C1/C2 是「退出端」候选，C3 是「读取端」观测点。

### C1（最可能）`exit_group` 桩 `0x42F100` — 全局退出闸门

- **EA**：`0x42F100`（`svc #0` 在 `0x42F104`，文件偏移同名，因 LOAD#0 的 `Offset==VirtAddr`）
- **证据**：明文壳内**唯一**能产生"干净退出 0"的原语；全 so 明文内 `exit_group` 桩只有 1 个调用者（`0x418184`）；`abort/exit/_exit` 均无明文导入（`sigaction/sigprocmask/environ` 是唯一可见 libc 名字）。
- **调用链（明文侧）**：`0x418110`（函数体）→ `0x41817C: mov w0,#0` → `0x418184: bl 0x42F100`。
- **伪代码摘要（`0x418110`）**：
  ```c
  void sub_418110(void *ctx, /*x1*/ u32 code, /*w3*/ u32 want) {
      w4 = *(u32*)(*(void**)(*(u64*)0x442E20)->... + 0x400 + 0x30);   // 取"原因码"
      if (w4 == want) goto EXIT;                 // 0x418140 b.eq
      if (w4 == 0x4FAF2AEB) *(u16*)(...+0x2E) += 1;
      (*(void(**)(void*))0x444D60)(ctx);         // 0x41816C blr x2 → 正常返回路径
      return;
  EXIT:                                          // 0x41817C
      exit_group(0);                             // ← 干净退出
  }
  ```
  （另一处魔数 `0x79BACE2B` 在 `0x418118/0x41811C` 作为 `want` 的默认值参与比较；两个魔数疑为混淆后的"原因码/事件码"枚举。）
- **patch 建议（最小、4 字节）**：`0x42F104` 的 `svc #0`(`01 00 00 D4`) → `ret`(`C0 03 5F D6`)，即 `exit_group` 变空操作（`x0` 保持入参）。或把整桩改成 `mov x0,#0; ret`。
  - 风险：**爆宽面最大**——所有 `exit_group` 调用（含正常退出/异常收尾）都被废掉；调用方若假定不返回（后续代码不可达/栈已毁），会走到未定义路径。**不建议直接出包**，只作为兜底。
- 备选更小面：只废掉「`mov w0,#0` + `bl`」这一处（`0x41817C..0x418188` 改 `ret`），前提是运行期证实 35 s 退出确实走这条。

### C2（同源，面更小）`0x418110` 内的退出分支

- **EA**：判定 `0x418140 (b.eq)` / 退出 `0x41817C~0x418188`
- **patch 建议**：`0x418140` 的 `b.eq` → `b`/NOP，或 `0x41817C` 起改 `ret`（`C0 03 5F D6`）。
- **未闭环**：该函数是「原因码分发器」还是「签名闸门」**尚未证实**（见 §5）。若现场 backtrace 命中 `0x418184`，则本点优先于 C1。

### C3（观测点，不建议 patch）`openat` 桩 `0x42EB00`

- **用途**：运行期挂这里打印 `x1` 指向的路径字符串 + backtrace，**一次就能回答**"壳是否自己在读 `/data/app/.../base.apk`（签名块）"，以及是谁让它读。
- **patch 不建议**：拒绝打开会让游戏自身资源/热更读取失败（明显副作用）。
- 明文内两个调用者 `0x42A488 / 0x42A61C` 是 mini-libc 的 `open/openat` 封装（`mov w0,#-100 ; orr w2,w2,#0x20000`），**不是**业务读取点。

## 5. 未闭环 / 存疑项

1. **静态到此为止**：校验比较逻辑在 `0x400..0x1F0000` 的加密载荷内，磁盘态**无任何可读证据**；壳内也**没有**任何指向载荷基址的 `adrp`（`adrp xN, 0x0` 命中 0 次）⇒ 解密/映射路径无法从明文直接串起来。想在静态侧继续，等价于先脱壳（逆向壳的解密算法），工作量远大于一次运行期取证。
2. **`0x418110` 语义未定**：`0x442E20` 是 GOT 槽（运行期才填），魔数 `0x79BACE2B / 0x4FAF2AEB` 无法从明文判断含义；"原因码"可能来自 Java 侧也可能是 native 自身事件。
3. **`0x42F000` 的 `x8=0xA9`（asm-generic 169 = `reboot`）存疑**：反汇编边界可能错位（该桩周围是数据/代码混排），也可能是壳的自毁/重启原语，本轮不下结论。
4. **触发者身份未证**：本轮只证明"本 so 里存在干净退出原语且形态吻合"，**没有**证明 35 s 退出确实由 `libFairGuard.so` 发出。APK 里还有 `libthemis.so`(849,496 B)、`libfntvcrash.so`、`libCrashSight.so`，也可能是 FairGuard 的 Java/dex 侧调用链。
5. **与硬约束 #13 的关系**：本 so 明文里 **0 处** APK/ZIP/DER 解析常量 ⇒ 机制与《邂逅在迷宫》的"APK 文件级校验"不同，**不套用**迷宫检测点（与用户当轮要求一致）；但这不等于"绿梦没有签名闸门"——闸门在载荷内，仍未定位。
6. **IG5 工具可用性**：后台分析作业两次 `cancelled before execution`（`ig5_doctor` 正常），需主会话侧再验一次；本轮静态结论不依赖 IG5 反编译，全部由 `llvm-objdump/readelf` + 字节扫描给出，可复核。

## 6. 下一步建议（需主会话决策，本阶段未执行）

按成本从低到高，全部**只读取证**，不改字节：

1. **frida 挂 `exit_group` 桩**（最省）：对 `fg_test` 包冷启，20 s 前
   `Interceptor.attach(Module.findBaseAddress("libFairGuard.so").add(0x42F100), {...})`，
   在回调里取 `Thread.backtrace()` 与 `this.context.lr`。35 s 命中即可**直接得到解密后代码里的校验点 VA**。
2. **同时挂 `openat` 桩 `0x42EB00`**：打印 `x1`(path)、`x2`(flags) 与 backtrace ⇒ 判定"是否读 `base.apk`/签名块"。
3. **内存态取码**：进程存活期间 dump `libFairGuard.so` 映射段（含被解密/被 `mprotect` 成 rwx 的区域）到本地，再离线静态分析 ⇒ 之后所有 patch 决策都基于明文代码。
4. 拿到 VA 后，patch 实施走 `ig5-patch-and-sign`（expected 字节核验 + 导出 + 回滚），再按 L1/L2/L3 验证；**本阶段不产出任何字节改动**。

## 7. 本阶段产物

| 产物 | 说明 |
|---|---|
| `06_dumped/绿梦_20261016/libFairGuard.so` | 原样副本，md5 `7bee7f8b…`（未改动） |
| `06_dumped/绿梦_20261016/libFairGuard_an.so` | **分析用副本**：仅改 section header（`.text`→`0x413AB0/0x1D8A0`），供 `llvm-objdump` 反汇编；**不可用于出包** |
| `/tmp/fg_text.asm` | 32,069 行反汇编（可重建：对 `libFairGuard_an.so` 跑 `llvm-objdump -d`） |
| 本文件 | 候选点 + 证据 + patch 建议 + 未闭环项 |
