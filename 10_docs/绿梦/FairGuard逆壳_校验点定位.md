# 绿梦：时空之声 — FairGuard 逆壳 阶段 6：校验点定位（签名校验侧）

- 本轮任务：在解密后载荷（`libFairGuard_rwxp.bin`，md5 `8297e6b9…`）里定位「签名校验」路径、给出最小 patch；完整性闸门由 B 子代理负责，本文件只在 §6 记录交叉线索。
- 纪律：**只读离线**。未改 so/APK 字节、未装机、未碰真机；设备侧证据全部引用 Stage 5 既有实测。
- 上一轮事实（直接采信）：open(base.apk) 回址 = **0x11b5c4**（Stage 5，上机 29.6 s）；8 个 exit_group 调用点；改载荷 1 B ≤0.5 s 被干净退出（Stage 4）。

## 0. 结论摘要（先看这段）

1. **`0x11b5c0` 是 libc `open` 的调用点（已确证）**：`bl 0x1ab70`，`0x1ab70` = `adrp x16,#0x3ff000 ; ldr x17,[x16,#0x768] ; br x17` 型 thunk ⇒ 槽 **0x3ff768**；与 Stage 5 的 `ra=0x11b5c4` 完全吻合。
2. **但它不读 APK 字节**：该块只做 **open → fstat(尺寸 A) → close → stat(尺寸 B) → open → fstat(尺寸 C) → close**，随后把「A≠C / B≠C / A≠B」写成**检测位 0x100 / 0x200 / 0x400**，OR 进 `ctx+0x38+0x1190`（检测位图）。**没有 read/pread/mmap**，与 Stage 5「同 fd 未捕获 read」互证。
   ⇒ 该块是 **APK 一致性/存在性探测（TOCTOU 型反篡改）**，不是证书摘要比较。**「open(base.apk) = 签名校验」这一前提需要修正**（见 §5 未闭环 1）。
3. **8 个 exit_group 调用点复核为 9 个**：新增 **0x1EE560**（与 0x46534/0x484A0 同型：`(a^b)&0xff-3`）。全部 9 个点的判定表达式已逐条列出（§4）；**只有 0x2F08C / 0x47E1C 两处是 `cmp x1,x0 ; csetm w0,ne`（结构体清理/断言型），其余 7 处是 MBA 后的哈希/比较值直接当退出码**。**没有一处**在读 §3 的检测位图 ⇒ 检测位与「kill」之间的链路仍未定位。
4. **patch 建议分两档**（§5）：A 档（推荐先试，面最小）= 把三处「尺寸一致性位」的 `orr+csel` 对 NOP 掉（6×4 B）；B 档（兜底）= 把 0x2F08C / 0x47E1C 两处 `bl 0x35BE00` NOP 掉。

## 1. 方法与可复现资产

| 资产 | 说明 |
|---|---|
| `04_scripts/绿梦/fg_xref.py` | 本轮新写：镜像按 raw 线性反汇编（4 B 步进、失败跳 4 B 继续），建 500,280 条指令索引 `/tmp/fg_idx.pkl`；支持 `around/back/xrefs/calls/thunks/const/svc`。**build 只需 2.7 s** |
| `/tmp/fg_payload.elf` | 把运行期镜像包成单 PT_LOAD(RWX, vaddr 0) 的合成 ELF，供 IG5/Ghidra 打开 |
| `/tmp/fg_chk2.elf` | **关键技巧**：只取 0x100000..0x140000（256 KB）做切片 ELF，**e_entry 设成 0x11b524**（该块是 Ghidra 能识别的最早块），Ghidra 才会把这块扁平化代码成一个函数并反编译成功 |
| `~/.dsh/ig5/artifacts/decompile/artifact_a78145…/reverse/func_0x11b524-r0.c` | 反编译产物 1,413 行（58 KB），本轮结论 2 的直接来源 |

- IG5 后台作业仍会 `IG5 operation cancelled before execution`（与上一轮记录一致）；**改用 `background:false` + `auto_analysis:true` 内联打开 256 KB 切片 3 s 完成**（4 MB 全量会超出 120 s 工具档期，别试）。
- 反汇编按 4 B 步进而不是整段 `md.disasm`：capstone 遇到第一条非法指令就停，整段解会得到 0 条（本轮踩过）。

## 2. thunk 表与 API 表（本轮新增确证）

`0x1a894..0x1b030` 共 **122 个 16 B thunk**，形态 `adrp x16,#0x3ff000 / ldr x17,[x16,#off] / add x16,x16,#off / br x17`，槽位 **0x3ff600..0x3ff9c8**。

- **0x1ab70 → 槽 0x3ff768 = libc `open`**（Stage 5 实测锚定）。
- 另有「API 表」：`ctx = *(sp+0x100)`，`tbl = *(ctx+0x30)`，反编译直接可读：

| 表偏移 | 语义 | 证据 |
|---|---|---|
| `+0x8` | **fstat** | `(*(tbl+8))(fd, &statbuf)`，返回 0 后取 `statbuf+0x30` = `st_size` |
| `+0x118` | **stat** | `(*(tbl+0x118))(path, &statbuf)`，同样取 +0x30 |
| `+0x130` | **open** | `(*(tbl+0x130))(path, 0, 0x1b6)` |
| `+0x148` | **close** | `(*(tbl+0x148))(fd)` |

`open` 实参形态固定为 `open(path, 0 /*O_RDONLY*/, 0x1b6 /*0666，无 O_CREAT 时为垃圾位*/)`。

## 3. 0x11b5c4 所在路径：逐条证据

### 3.1 反汇编（原字节）

```asm
0011b5b8  ldr  x19, [x3, #0x30]      ; API 表
0011b5bc  ldr  x0, [x0]              ; x0 = 解密后的路径串（base.apk）
0011b5c0  bl   #0x1ab70              ; <<< open(path,0,0x1b6)   文件字节 6c fd fb 97
0011b5c4  cmn  w0, #1                ; <= Stage 5 观测到的 ra
0011b5c8  mov  w21, w0
0011b5cc  b.eq #0x1233d4             ; fd==-1 -> 尺寸 A = 0
0011b5d0  mov  x1, #0x1bc0
0011b5d4  add  x2, sp, #0x1a0
0011b5d8  add  x25, x2, x1           ; statbuf@sp+0x1d60
0011b5dc  ldr  x2, [x19, #8]         ; fstat
0011b5e0  mov  x1, x25
0011b5e4  blr  x2
0011b5e8  mov  x24, #0
0011b5ec  cbz  w0, #0x122028         ; 成功 -> 0x122028: ldr x24,[x25,#0x30] = st_size (A)
0011b5f0  ldr  x1, [x19, #0x148]     ; close(fd)
```

### 3.2 反编译（IG5/Ghidra，`func_0x11b524-r0.c` 行 145-187，直接引用）

```c
lVar64 = *(long *)(ctx + 0x30);                                   // API 表
uVar38 = func_0x0001ab70(*(undefined8 *)(b0 - 0xd00), 0, 0x1b6);  // open(base.apk)
if ((int)uVar38 != -1) { iVar33 = (**(code **)(lVar64 + 8))(uVar38, &stack0x1d60);   // fstat
                         if (iVar33 == 0) lVar40 = in_stack_00001d90;                 // 尺寸 A
                         (**(code **)(lVar64 + 0x148))(uVar38); }                     // close
iVar33 = (**(code **)(lVar64 + 0x118))(path, &stack0x1de0);                        // stat
if (iVar33 != 0) { uVar38 = (**(code **)(lVar64 + 0x130))(path,0,0x1b6);            // open#2
                   iVar33 = (**(code **)(lVar64 + 8))(uVar38, &stack0x1e60);        // fstat
                   if (iVar33 == 0) in_stack_00001e10 = in_stack_00001e90;           // 尺寸 B
                   (**(code **)(lVar64 + 0x148))(uVar38); }
uVar38 = (**(code **)(lVar64 + 0x130))(path,0,0x1b6);                               // open#3
if ((int)uVar38 != -1) { iVar33 = (**(code **)(lVar64 + 8))(uVar38,&stack0x1ee0);   // fstat
                         if (iVar33 == 0) lVar67 = in_stack_00001f10;                // 尺寸 C
                         (**(code **)(lVar64 + 0x148))(uVar38); }
```

### 3.3 判定：三尺寸互比 → 检测位（反编译行 225-238）

```c
uVar49 = *(uint *)(a8 - 0xa28);          // 上一步的检测位（含 0x100/0x200/0x400 之外的位）
uVar71 = (lVar40 == lVar67) ? uVar49 : (uVar49 | 0x100);      // A != C
uVar49 = (in_stack_00001e10 == lVar67) ? uVar71 : (uVar71 | 0x200);  // B != C
uVar71 = (lVar40 == in_stack_00001e10) ? uVar49 : (uVar49 | 0x400);  // A != B
...
*(uint *)(ctx38 + 0x1190) |= uVar71;     // 全局检测位图
```

随后（行 245-262）再调 `(**(code **)(*(long **)(ctx+0x38) + 0x128))(...)` 造一个路径串，`open` 它做**存在性探测**，成功则再置位 **0x8**。

**全块无 read/pread64/mmap**（反编译里所有 `(**(code**)(tbl+…))` 只有 +8/+0x118/+0x130/+0x148 四个）。⇒ **APK 的字节从未在此被读取**。

### 3.4 检测位图的其它写入点（同一扁平化函数）

| EA | 作用 |
|---|---|
| 0x11B538/0x11B548 | `*(ctx38+0x5e8) |= w1`（另一张位图，入口处即 OR） |
| 0x11BA24/0x11BA34 | `*(ctx38+0x1190) |= w19` |
| 0x11BD8C..0x11BDA8 | 一次性 OR 进 **0x20000/0x40000/0x80000/0x100000/0x200000/0x400000/0x800000/0x2000000/0x4000000**（源字段 0x5f8/0x5ec/0x5c4/0x5c0/0xbf4/0x42c/0x42e/0x954/0x360/0x364） |
| 0x27754/0x2775C | 另一函数的 `|= w4` |
| 0x11BE0C | 读回 0x1190 继续拼报告 |

⇒ 这套位图是 **FairGuard 的「检测事件/取证位图」**：本块只负责“采集并置位”，**本函数内没有任何 `bl 0x35BE00`**（9 个 exit 点全在外面的函数里）。

## 4. exit_group 调用点复核（8 → 9）

`0x35BE00` = `mov x8,#0x5e ; svc #0`（桩本身）。全镜像 `bl 0x35BE00` 共 **9** 处：

| # | EA | 原字节 | 前面的判定（摘要） | 退出码 |
|---|---|---|---|---|
| 1 | 0x2F08C | `5d b3 0c 94` | `cmp x1,x0 ; csetm w0,ne`（结构体/链表清理断言） | a==b→0，否则 -1 |
| 2 | 0x47E1C | `f9 4f 0c 94` | 同上（另一处析构/清理） | 同上 |
| 3 | 0x459EC | `05 59 0c 94` | `eor/eor/orn`（MBA 比较结果取反） | `~((w1^w0)\ |(w2^w1))` |
| 4 | 0x462BC | `d1 56 0c 94` | 同上 | 同上 |
| 5 | 0x46534 | `33 56 0c 94` | `(a^b)&0xff ; sub #3` | `((a^b)&0xff)-3` |
| 6 | 0x484A0 | `58 4e 0c 94` | 同上 | 同上 |
| 7 | 0x1AA824 | `77 c5 06 94` | `eor/eor/orn` | 同 #3 |
| 8 | 0x1B0200 | `00 af 06 94` | `eor/eor/orn` | 同 #3 |
| 9 | **0x1EE560** | `28 b6 05 94` | **本轮新增**；`(a^b)&0xff ; sub #3`（与 #5 同型） | `((a^b)&0xff)-3` |

判读：**没有任何一处读 `ctx+0x38+0x1190`**（本轮已对 `#0x1190` 做全镜像指令扫描，只有 0x27754 与 0x11BA24/0x11BD8C/0x11BE0C 这 4 个写/读点，且都在采集侧）。
⇒ 「检测位 → kill」的边**不在静态可见的直线路径上**（要么走间接跳转表，要么由另一层（如 libthemis / Java 侧）消费）。

## 5. patch 建议（两档；**均未实测**）

### A 档（推荐先试，面最小）：抑制 APK 三尺寸一致性位

| EA | 原字节 | 新字节 | 语义 |
|---|---|---|---|
| 0x11B858 | `61 02 18 32` | `1f 20 03 d5` | `orr w1,w19,#0x100` → NOP |
| 0x11B85C | `33 10 93 1a` | `1f 20 03 d5` | `csel w19,w1,w19,ne` → NOP |
| 0x11B864 | `61 02 17 32` | `1f 20 03 d5` | `orr w1,w19,#0x200` → NOP |
| 0x11B868 | `33 10 93 1a` | `1f 20 03 d5` | `csel` → NOP |
| 0x11B870 | `61 02 16 32` | `1f 20 03 d5` | `orr w1,w19,#0x400` → NOP |
| 0x11B874 | `33 10 93 1a` | `1f 20 03 d5` | `csel` → NOP |

- 安全性：每一对 `orr wX,w19,#imm` + `csel w19,wX,w19,ne` 的输入都是 w19，NOP 后 w19 保持原值，后续 `func_0x002c62d4/1d84d0(w=...,uVar71)` 只是少一个位，**无未定义寄存器**。
- 局限：只抹掉「尺寸不一致」这一路证据；若 30 s 闸门来自别的位（0x5f8/0xbf4/0x954 等），本 patch 无效。

### B 档（兜底）：掐 2 个「断言型」干净退出

| EA | 原字节 | 新字节 |
|---|---|---|
| 0x2F08C | `5d b3 0c 94` (`bl 0x35BE00`) | `1f 20 03 d5` |
| 0x47E1C | `f9 4f 0c 94` | `1f 20 03 d5` |

- 两条 `bl` 的下一条都是 `b`（0x2F090→0x2E7C4 / 0x47E20→0x3984C），因此 NOP 后**直接落到无条件跳转**，不会跑进数据。
- 这两处是镜像里**唯一能产生 exit code 0 的形态**（`csetm w0,ne` 相等时为 0）；其余 7 处的退出码恒 ≠ 0，与「exited cleanly (0)」不符 ⇒ 30 s 干净退出**若**来自 9 点之一，最可能就是这两处。**推断，未经实测确认**。

### 不建议：把 0x35BE00 桩整体改 `ret`
一次抹掉全部 9 个退出（含 B 正在处理的完整性闸门），失败时无法归因，且完整性路径若依赖“退出不返回”会有未定义行为。

## 6. 完整性检测（B 子代理负责，本节只记交叉线索）

- B 实测：改载荷 1 B → 0.5 s 崩，`PC=+0x2D8708` ⇒ 完整性例程在 **0x2D8xxx** 一带，且崩溃点≠退出点（是“算完/比较后跳到坏地址”）。
- 本轮交叉观察：0x2D8xxx 距 9 个 exit 点中的 0x2F08C 仅 0x89xx，**可能同一“自检模块”**；若 B 确认，则该模块同时管完整性与退出。**未验证，交 B**。
- 全镜像扫描再次确认：**无 AES S-box / SHA-256 K / SHA-1 K / MD5 T / CRC32 表 / ZIP 魔数 / "APK Sig Block 42" / META-INF / 证书指纹（4 个都试过，含反序）** ⇒ 哈希与解析常量均为自定义或运行期解码，不能用常量定位法。

## 7. 未闭环

1. **「open(base.apk) = 签名校验」这个前提本身要改**：本轮证明它只 stat 不读（§3.3）。证书/签名摘要的比较点**未找到**。三种可能：(a) 在 §3.3 行 246 那个 `vtable+0x128` 方法里（未展开）；(b) 由 libthemis/libfntvcrash 或 Java 侧做，native 只收结果；(c) 本 so 的「签名校验」其实只做尺寸/存在性一致性（弱校验）。
2. **检测位图 → 退出** 的链路未定位（9 个 exit 点均不读 0x1190；无间接跳转表分析）。
3. 扁平化函数的**真实入口未定**：Ghidra 只给出 0x11B524..0x11E2C4（0x2DA0），但 CFG 显示块跳跃到 0x121xxx/0x125xxx，实际函数 ≥ 0xC000，边界需按“块可达性”重算。
4. A/B 两档 patch **均未上机**；需按 L1/L2/L3 流程验证。
5. 第三段 open（0x11B9A4）的路径串由 `func_0x002c5518` 从 `vtable+0x128` 的产物拼出，**具体路径未解**（可能是 /proc/self/maps 或自身 .so）。

## 8. 复现命令

```bash
python3 04_scripts/绿梦/fg_xref.py build
python3 04_scripts/绿梦/fg_xref.py around 0x11b5c0 40
python3 04_scripts/绿梦/fg_xref.py xrefs 0x35BE00
python3 04_scripts/绿梦/fg_xref.py xrefs 0x1ab70
# 合成 ELF + IG5（256 KB 切片，entry=0x11b524）
python3 - <<'PY'
import struct
img=open('06_dumped/绿梦_20261016/fg_runtime/libFairGuard_rwxp.bin','rb').read()
LO,EM=0x100000,0x11b524
body=img[LO:0x140000]; eh=bytearray(0x400)
eh[0:16]=b'\x7fELF\x02\x01\x01\x00'+b'\x00'*8
struct.pack_into('<HHIQQQIHHHHHH',eh,16,3,183,1,EM,0x40,0,0,0x40,0x38,1,0x40,0,0)
struct.pack_into('<IIQQQQQQ',eh,0x40,1,7,0x400,LO,LO,len(body),len(body),0x1000)
open('/tmp/fg_chk2.elf','wb').write(bytes(eh)+bytes(body))
PY
# 然后 ig5_open(background:false, auto_analysis:true, analysis_timeout:240)
#      -> ig5_decompile(ea=0x11b524, style:"")  产物落在 ~/.dsh/ig5/artifacts/decompile/…
```


## 9. 追加轮（决策官追问）：签名校验在 native 还是 Java？30 s 退出由什么触发？

### 9.1 结论：**native**，不是 Java 层

| 证据 | 内容 |
|---|---|
| 三个防护库的 NEEDED | `libFairGuard.so`：liblog/libdl/libz/libstdc++/libm/libc；`libthemis.so`：liblog/libdl/libEGL/libGLESv2/libandroid/libm/libc；`libfntvcrash.so`：liblog/libdl/libm/libc。**三者都无 libart.so ⇒ 无法调用 Java** |
| libFairGuard 导出面 | `.dynsym` 名称被抹平，无 `JNI_OnLoad` / `Java_*`（Stage 0 已测 0 命中）⇒ Java 侧也无入口调它 |
| dex 侧 | 原包 3 个 dex 里 `strings | grep -i fairguard` = **0 命中**；`themis` 命中全部是 `com.taptap.sdk.themis.lite`（TapTap 登录 SDK），`fntvcrash` 是 CrashSight。**APK 里没有 FairGuard 的 Java SDK** |
| 退出原语 | `libc exit/_exit/abort`、`AndroidRuntime::exit`、`libc syscall` 全量 hook 均 0 命中，而进程 `exited cleanly (0)` ⇒ 退出只能来自**原生 raw `exit_group` svc**（载荷自带桩 0x35BE00，9 个调用点） |

⇒ 判定、退出都在原生载荷内完成；不存在「Java 校验 → native 执行」的链路。

### 9.2 重要修正：Stage 5「无 read」不能证明「没读文件」

本轮发现载荷有**两套 IO 通道**：

| 通道 | 形态 | 是否会被 libc hook 看到 |
|---|---|---|
| A. libc 表（`ctx+0x30`） | 122 个 thunk（0x1A894..0x1B030）→ 槽 0x3FF600..0x3FF9C8，运行期由 0x3FF768=open 解析 | **会**（0x11B5C4 就是这条） |
| B. 自有 syscall 桩 | 桩体 0x35B100=openat / 0x35B200=read / 0x35B400=close / 0x35BE00=exit_group；指针经 24 B 记录表（槽 0x3FFA00..0x3FFFF0）填进一张 API 结构（初始化处见 0x1F420/0x1F600/0x1FC90/0x1FC9C/0x10C1D0） | **不会**（直接 `svc #0`） |

⇒ 若 FairGuard 用通道 B 读 APK 字节，Stage 5 的 libc 探针**本来就看不见**。「无 read」只证明通道 A 没读。
（同理，Stage 5 的「open(base.apk) fd=201」是通道 A 的观测，通道 B 的 openat 未被观测。）

### 9.3 常量侧再确认（通道 B 若做 ZIP/证书解析，必须留下痕迹）

| 扫描 | 结果 |
|---|---|
| ZIP 魔数 `PK\\x03\\x04 / PK\\x01\\x02 / PK\\x05\\x06`（明文） | 0 |
| `mov/movk` 构 0x4B50 / 0x0403 / 0x0201 立即数 | 0 |
| "APK Sig Block 42" / META-INF / .RSA / .DSA / AndroidManifest.xml / classes.dex / base.apk（明文） | 0 |
| 同上，**任意单字节 key 的 XOR / ADD / SUB 编码形态**（numpy 全盘扫） | **0** |
| SHA-1 K0..K3 / H0；SHA-256 K0/K63/H0；MD5 A/T0/T64；AES S0；CRC32；ChaCha | 0（两种字节序） |

⇒ 至少**不是**「直接按 ZIP 结构解析 + 标准哈希」的实现；要么自定义哈希 + 自定义容器解析，要么根本不读内容。

### 9.4 「尺寸 vs 签名」如何分离（建议的一次实验）

- 已知：0x11B52C 块把 **同一路径**的三次测量（fstat / stat / fstat）互比，重签包对这三个测量**同样自洽** ⇒ 该块不可能区分原厂/重签；若 30 s 退出确实由这一段驱动，真正起作用的只能是同函数里的 vtable(+0x128) 探测或第三段 `open`。
- 单变量实验：**等长重签** —— 把重签包用 zip 填充（stored 空条目 / extra field）补到与原包**完全相同**的 179,649,718 B 后再装。
  - 仍 30 s 死 ⇒ 尺寸不是变量 ⇒ 触发来自签名内容（或 zip 结构）；
  - 存活 ⇒ 尺寸/结构是变量。
- 该实验只改一个自变量（文件大小），符合「一轮一个变量」。

### 9.5 本轮未闭环（补充）

1. vtable(+0x128) 方法的具体实现未展开（需要知道 `*(ctx+0x38)` 的实例类型/vtable 基址）。
2. 通道 B 的 `read` 桩（0x35B200）在业务代码中的**调用点未定位**（目前只看到它被填进 API 结构的初始化点）。
3. 30 s 退出与哪一处 `exit_group` 对应仍未实测。



## 10. 等长重签实验（2026-10-20 02:38，实测）——尺寸不是变量，签名才是

### 10.1 载体

| 项 | 值 |
|---|---|
| 产出 | `07_patched/绿梦/fg_test/绿梦_equallen_resign.apk` |
| 大小 / md5 | **179,649,718 B（与官方逐字节同长）/ `6f56c63d33467547d0471a9f5fbd0780`** |
| 装机后设备端核对 | `/data/app/~~HST1SIkX…/base.apk` = 179,649,718 B，`md5sum` 同值 ⇒ 装进去的确实是等长包 |
| 造法 | `04_scripts/绿梦/surgical_equal_length_resign.sh`：保留官方前段内容原字节 → 剥签名块 → 重签(StarryMagic) → 差额用 EOCD 注释补齐 |
| 与官方唯一差异 | 签名块（v2 / 证书）+ v1 签名文件（`META-INF/STARRY.SF/RSA`，SHA-1 摘要）+ 28,672 B zip 注释；**条目/本地头/中央目录内容逐字节相同** |

技术要点（复用价值高）：
- apksigner 会先**删掉旧的 v1 签名条目**（−102,595 B），再插 4,096 B 签名块，并把内容**补齐到 4 KiB 边界**——这是「重签包总是 +4,096」的真正原因（对齐填充，不是签名块变大）。
- 只有让**内容长度落回目标 4 KiB 对齐窗**才能等长：用 `--min-sdk-version 17`（v1 走 SHA-1 摘要，MANIFEST/SF 缩小 32,768 B）使内容 ≤ 0xAB3C000，再以 zip 注释补 28,672 B ⇒ 精确 179,649,718。
- `--min-sdk-version 17` 下 apksigner 仍产出 **v2（SHA-256）**；`apksigner verify` OK，Android 14 装机成功。

### 10.2 结果：**仍然 30 s 干净退出**

`09_test/绿梦/fg_forensics/equallen_1020_023806.txt`：

```
02:38:17.527 I/ActivityManager: Start proc 3775:com.zhuziplay.lvmengnew.op/u0a1088
02:38:48.928 I/Zygote: Process 3775 exited cleanly (0)      ← 存活 31.4 s
```

### 10.3 三方对照（同一设备、同一 so、同一内容）

| 包 | 签名 | 大小 | 存活 |
|---|---|---|---|
| 官方 | 原厂 | 179,649,718 | **160 s+**（Stage 2 既有实测） |
| fg_test 重签 | StarryMagic | 179,653,814 (+4,096) | 30.4 s |
| **等长重签** | StarryMagic | **179,649,718（等长）** | **31.4 s** |

⇒ **尺寸不是变量；签名（证书）是变量。**
残余差异（zip 注释 28,672 B、v1 用 SHA-1）**不能**解释死亡：上一版 fg_test 重签包既无注释也是 SHA-256 v1，同样 30.4 s 死；而两者唯一共有的差异就是「证书不是原厂」。
⇒ FairGuard **确实读到了签名**并据此判死；通道 A（libc）只观测到一次 `open(base.apk)`，故读取点大概率在**通道 B（自有 svc 桩）**或**已映射的 APK 内存**上。



## 11. 通道 B（自有 svc 桩）调用面静态测绘（2026-10-20 追查）

### 11.1 桩 → 槽位 → API 结构字段（已确证）

24 B 记录表（`slot, 0x403, stub`）在 0x18A00..0x19900 区段，解出：

| 槽 | 桩 | 用途 | 被填进的结构字段 |
|---|---|---|---|
| 0x3FFA60 | 0x35B100 | openat | `[structA+0x170]`（0x1FA5C）、`[structB+0x300]`（0x1F420） |
| 0x3FFF90 | 0x35B200 | **read** | `[structA+0x70]`（0x1FC9C） |
| 0x3FFB38 | 0x35B400 | close | `[structA+0x148]`（0x1FC90）、0x10C1D0/0x10C1F4 |
| 0x3FFB00 | 0x35BE00 | exit_group | `[structB+0x258]`（0x1F600） |
| 0x3FFF88 | 0x35B? | 未知 | `[structA+0x80]`（0x1FC80） |

调用形态：`ldr xN,[xR,#0x70]; blr xN`（read）、`[#0x148]`（close）。

### 11.2 read（+0x70）调用点：39 处，按语义分三族

| 族 | 代表 EA | 形态 | 判读 |
|---|---|---|---|
| A. open→read 小文本 | **0x1142FC / 0x10F424 / 0x6F1E0 / 0x1E164C / 0x1E183C / 0x1E1A88** | `open(path,0,0x1B6)` → `read(fd, sp+0x…, **0x200**)` → 去掉末尾 \n → 构造 std::string | 读**小文本文件**（≤0x200 一行），**不是 APK** |
| B. 分块整文件读 | **0x8C364 / 0x8D2D8 / 0x8D8C8 / 0x8DD7C / 0x8E270**（同型 5 处）、0x75CE0、0x1A0694/0x1A1924 | `lseek(fd, chunk<<17, 0)` + `read(fd,buf,**0x20000**)` 循环（另有 0x10000、0x8000 档） | 通用**资源/解压读取**（libil2cpp/libunity/global-metadata 一类） |
| C. 其它 | 0xD7044 / 0xF19F0 / 0x10E098 / 0x1A72B0 / 0x1A7560(0x10 B) / 0x1B15D0(0x200 B) 等 | 多为 C++ 虚表同偏移误命中或定长小读 | 非签名路径 |

### 11.3 判定：**没有找到「读 base.apk 内容」的通道 B 调用点**

- 全镜像**没有**任何指向 0xAB3C000 / 0xAB3D000（签名块/中央目录）附近的立即数或 seek 目标；
- 分块读循环的 offset 是 `chunk<<17`（0x20000 步长）从 0 起顺序读，没有「从文件尾反向」或「按 fstat 尺寸定位签名块」的形态；
- 没有 `statbuf+0x30`（st_size）参与 seek 计算的代码被观察到。

⇒ 若签名内容确实被读取，只可能：(a) 走**已映射的 APK 内存**（无需 syscall，静态看不到）；(b) 在**通道 A 的某个我们尚未枚举的 libc 表项**；(c) 由另一个模块（libthemis/libfntvcrash）完成。**签名比较函数 EA 因此仍未定死**。

### 11.4 下一步（动态，但不用 ptrace）

设备上 **无 strace / 无 toybox strace**（已实测）。可用的低反调试风险观测：

1. **/proc/<pid>/syscall 高频采样**（root，不需 ptrace）：在 t=24..33 s 每 2-5 ms 读一次，命中 `read/pread64/lseek` 时记录 `nr/x1(fd)/x2(count)/x3(offset)`。若出现 count≈0x20000 且 fd 指向 base.apk，即命中签名读取循环的**调用时刻**，再用同刻的 `/proc/<pid>/maps`+PC 采样定位代码。
2. 或推一个**静态 strace** 到 /data/local/tmp（ptrace 型，末尾 5 s 才 attach，风险可控）。
3. 若两者都拿不到，退回到「**行为法**」：改一处**只影响签名块、不影响任何条目**的字节（例如把 v2 签名块内的某个 ID 改掉但不改长度），看是否仍 30 s 死——可区分「读块头」与「读证书」。



## 12. 明文库排查：签名校验不在 libthemis / libfntvcrash / libil2cpp / libunity（2026-10-20）

从官方 `base.apk` 抽出 `lib/arm64-v8a/*`（`/tmp/fglibs`）逐个判：

| 库 | 大小 | 整段熵 | 节表 | 签名校验证据 |
|---|---|---|---|---|
| libthemis.so（TapTap 风控） | 849,496 | 2.93 | 25 节 | 4 个证书指纹 0；META-INF/.RSA/APK Sig Block 0；`cert`/`sign` 0；唯一的 `.apk` 是设备指纹串表里的裸串 |
| libfntvcrash.so | 57,592 | 6.11 | 26 节 | 仅 `/proc/self/maps`（崩溃上报），无签名证据 |
| libCrashSight.so | 740,624 | 4.79 | 27 节 | 同上 |
| **libil2cpp.so** | 127,603,744 | 4.61 | 23 节 | **全 0**（连 SHA1/SHA256/MD5/CRC32 常量都没有） |
| **libunity.so** | 23,667,744 | 4.48 | 23 节 | **全 0** |
| libtapsdkcore.so | 4,212,056 | 3.31 | 29 节 | 14 处 `signature` **全是 BoringSSL/OpenSSL 内部串**（`pkcs7-signedAndEnvelopedData`、`CRL signature failure`、`unable to decrypt certificate's signature`）；SHA1/SHA256 常量属其自带 TLS 栈 |

**结论：这些库全部是明文（有完整节表、无高熵加密段），且不含 APK 签名校验**——与 `libFairGuard` 的 `0x400..0x1F0000` 密文形成鲜明对比。

补充全包扫描（51 个 `.so/.dex/.dll/assets/*` 条目逐个解压搜）：
- 4 个原厂证书指纹（SHA256/SHA1/MD5/pubkey-SHA256）：**0 命中**；
- `APK Sig Block 42`：0 命中；
- `getPackageInfo`：仅 `classes.dex`(3)/`classes2.dex`(6) —— 通用第三方库调用，与 FairGuard 无关（dex 里 `fairguard` 0 命中，见 §9.1）。

⇒ 不存在「明文库里就能直接 patch 的签名校验」。签名判定只能在 `libFairGuard` 的**运行期载荷**内完成。

### 12.1 由此收敛到「已映射 APK 内存」这一条

既然 native 侧没有任何 `read` 命中 APK（§11.3），而等长实验又证明签名是变量（§10.3），最自洽的剩余解释是：

```
open(base.apk) 0x11B5C0 → mmap(fd, 全长) → 在内存里走 ZIP/签名块解析 → 比较
```

`mmap` **不产生 read 系统调用**（缺页由内核完成 I/O），因此 Stage 5 的 libc read 探针天然看不见；而 Stage 5 唯一一轮带 `mmap` hook 的探针（v4）在 21 s 被反调试提前杀掉，恰好没覆盖 29.6 s 这一刻。

**低成本验证（不用 hook，不被反调试识别）**：在 t≈29.0 s 与 t≈31.5 s 各抓一次 `/proc/<pid>/maps`，diff 出新增的 `base.apk` 映射：
- 若 29.6 s 之后**新出现**一段 `r--s` 且 offset 指向签名块/尾部 ⇒ mmap 假设成立，patch 靶点变成「mmap 返回后比较签名的内存读取点」；
- 若没有新映射 ⇒ 它只能在**已存在的 ART 映射**上解析，靶点变成「在 maps 里定位 base.apk 的代码」。



## 13. maps 差分实验 + unpacked 镜像复查（2026-10-20 02:45-02:48 实测）

### 13.1 mmap 假设：**不成立（无新增映射）**

`04_scripts/绿梦/maps_watch.sh`（root 直读 `/proc/<pid>/maps`，26 s 起每 ~0.45 s 一帧，共 10 帧）：
`09_test/绿梦/fg_forensics/maps_1020_024554/`

| 帧 | 行数 | base.apk 映射数 |
|---|---|---|
| 01..10（≈26.0 s .. ≈30.5 s） | 4348（逐帧完全相同） | **3**（无变化） |
| 11 | — | 进程已消失（≈30.6 s） |

⇒ **29.6 s 的 open 之后没有出现新的 `base.apk` mmap**，进程在 ~30.5 s 干净退出。「open→mmap 全长→解析」假设**被否证**。

### 13.2 但发现关键事实：**v2 签名块本来就在进程内存里**

存活期三段的其中两段（ART/zip 解析留下的）：

```
7561f0a000-7561f21000 r--s 0ab36000 fe:48 ... base.apk     ← 0xAB36000..0xAB4D000（0x17000）
756ccce000-756cce5000 r--s 0ab36000 fe:48 ... base.apk     ← 同上（另一份）
7372657000-737287f000 r--s 07337000 fe:48 ... base.apk     ← 0x7337000..0x955F000
```

**0xAB36000..0xAB4D000 完整覆盖 v2 签名块 0xAB3C000..0xAB3D000**。
⇒ FairGuard 不需要自己 mmap，也不需要 read 系统调用：**直接读这段已映射内存就能拿到签名块**。这与「无 read、无新映射、却仍按签名判死」完全自洽。

### 13.3 unpacked 镜像复查（md5 556705e0…，4,198,352 B）

- A 区 482/496 页、B 区 522/528 页与运行期 dump 逐页一致（差异页 = 运行期数据）；
- **4 个原厂证书指纹：全部 0 命中**（unpacked 里也不是明文）；
- `META-INF` / `.RSA` / `APK Sig Block 42` / `PK\\x03\\x04` / `PK\\x05\\x06` / `base.apk` / `/data/app`：**全部 0 命中**；
- 也没有任何 8 字节常量落在 0x0A000000..0x0B000000（APK 尾部偏移）。

⇒ 常量与字符串**均为运行期解码**，静态常量扫描这条路已彻底用尽。

### 13.4 B 区（0x1F0000..0x400000）全量统计（本轮新增，之前只扫了 A 区）

| 项 | 数量 | 说明 |
|---|---|---|
| `bl 0x35BE00`（exit_group） | **17**（A 区 9 + B 区 8） | 新增：0x2746CC、0x28F070、0x28F2E4、0x29062C、0x29A7F4、0x29BEB4、0x2DD2F8、0x2DF100 |
| `bl 0x1AB70`（libc open） | **41**（B 区 16） | B 区簇 0x31F2B8..0x3274C4 共 12 处、0x292C08、0x295E1C、0x31EAD0 |
| `bl 0x35B100`（自有 openat） | 2 | 0x2C7120、0x2C77C4 |
| `svc #0` | 62 | 桩体内联 |

B 区那簇 open 的形态是 `open(path, **0x241**, 0x1B6)` + 写 4 字节 + close（0x241 = O_WRONLY|O_CREAT|O_TRUNC）⇒ **报告/状态落盘**，不是签名读取。
0x2DD2F8 / 0x2DF100 两处 exit 特别：`bl exit_group` 之后还有 `ldr x1,[sp,#0xa0]`（即“调用后继续”），属可返回型退出调用。

### 13.5 该走的路（结论）

签名块既然在内存里，读取点就**不是 syscall 层**，只能靠：
1. **在 maps 解析点下手**：`/proc/self/maps` 的读取点（通道 B 那两个 0x200 字节“按行读”的 0x1142FC / 0x10F424 最像）后续会用 `mapping_start + (0xAB3C000 - 0xAB36000)` = `map+0x6000` 直接索引；
2. **或者直接按行为打**：既然只有签名块区域在内存里且被比对，最省事的验证是——把**安装包内的 v2 签名块内容**换成原厂的（我们持有官方 base.apk 的签名块字节），看是否 30 s 死亡消失。若能复活，则确认“比对对象就是这段内存”，patch 靶点也随之锁定为读取该偏移的指令。



## 14. 第一个真补丁装机验证（L2）：noexit 过 35 s 闸门（2026-10-20 02:54-02:56 实测）

### 14.1 测试包

| 项 | 值 |
|---|---|
| 产物 | `07_patched/绿梦/fg_test/绿梦_noexit_resign.apk` |
| 大小 / md5 | 179,621,046 B / `b96fe9609ce72d667bd390a88d7e21eb`（apksigner verify OK） |
| 换 so 方式 | `04_scripts/绿梦/fg_so_swap.py`（外科替换单 entry：只重写该 entry 的 local header + 压缩流，其余字节原样搬移，按累计位移修正 CD 偏移与 EOCD）→ 再走 `surgical_equal_length_resign.sh --no-pad` 重签 |
| 包内 so 校验 | 解出 `lib/arm64-v8a/libFairGuard.so` md5 = **5f4b45566e278104691fdc2fa33ff050** = A2 的 noexit 产物 ✔ |
| 装机 | 覆盖装（同 CN=StarryMagic，不清数据）`Success`，lastUpdateTime 02:53:59 |

（注：noexit.so 的载荷区与磁盘原 so、运行期明文都不同 —— A2 是「解明文→打 8 字节→re-deflate→重加密写回」，属预期形态。）

### 14.2 结果：**35 s 闸门通过，且进了主界面**

`09_test/绿梦/fg_forensics/equallen_1020_025350.txt` + 现场核查：

```
02:54:01.904  Start proc 9674:com.zhuziplay.lvmengnew.op (COLD)
02:55:22      pid 9674 仍存活（state=S），topResumedActivity =
              com.bamboo.game.MainFragmentActivity      ← 主界面，不只是闪屏
```

- **30 s 的 `exited cleanly (0)` 没有发生**；35 s、80 s 两个时点都活着；
- 对照：改前同内容包 30.4 s / 31.4 s 必死 ⇒ 0x2F08C + 0x47E1C 两处 `bl exit_group` 就是那道闸门的执行点，NOP 有效。

### 14.3 但 102 s 出现**第二个死点**（新墓碑）

`/data/tombstones/tombstone_18`（02:55:43，process uptime **102 s**）：

```
pid 9674, tid 9923, name UnityMain
signal 4 (SIGILL), code -6 (SI_TKILL)     ← 进程自己 tgkill 发的，不是硬件异常
pc 0x732b3def00  libil2cpp.so+0x3b96f00
backtrace: libil2cpp.so ×5 → libunity.so ×3 → ...（30 帧）
```

- 官方原签包实测能活 160 s+，所以这个 102 s 死**不是 Unity 自身问题**，是本轮补丁引入/触发的；
- `SI_TKILL` 说明是**代码主动自杀**（不是野指针），符合"第二道检测→主动崩"或"NOP 后调用方拿到异常返回值→走 fatal 路径"两种解释。

### 14.4 结论与下一步

- **闸门假说成立**：0x2F08C / 0x47E1C 的 `bl 0x35BE00` 就是 30 s 干净退出的触发点，NOP 后进程活过 35 s 并进到 `MainFragmentActivity`。
- **未闭环**：102 s 的 SIGILL 需要单独定性。建议下一轮单变量：
  1. 只 NOP 0x2F08C（保留 0x47E1C），或只 NOP 0x47E1C —— 看 102 s 死点是否随单个点变化；
  2. 抓 02:55:40 前后的 logcat（本轮日志在 02:54:42 就停了，没有覆盖死点）；
  3. 若两处都 NOP 仍有 102 s 死点 ⇒ 存在第三道检测，需要按同一方法（exit 点/crash 触发点）继续定位。



## 15. 单 NOP 对照 + 102s SIGILL 定性（2026-10-20 02:58-03:08 实测）

### 15.1 单变量：只 NOP 0x2F08C（保留 0x47E1C）

产物 `绿梦_nop2F08C_resign.apk`（179,621,046 B，md5 `d8873e13bc0d921d7b364a425a57d9a8`），明文核对：
`0x2F08C = 1f 20 03 d5`（NOP）、`0x47E1C = f9 4f 0c 94`（原 bl 保留）。

| 轮 | 结果 |
|---|---|
| 单 NOP 第 1 次冷启 | 02:58:44 启动 → **03:00:26 SIGILL 死（uptime 102 s）** |
| 双 NOP（上一轮） | 02:54:01 启动 → 02:55:43 SIGILL 死（uptime 102 s） |
| 单 NOP 第 2 次冷启 | 02:54 起 60 s 采样全活 → **03:08:27 SIGILL 死（uptime 157 s）** |

结论：
- **两处 exit 任一单独 NOP 都足以让 30 s 干净退出消失**（单 NOP 也没再出现 `exited cleanly(0)`）；
- **102 s 死点不随「NOP 哪个点」移动**，且第三次跑到 157 s 才死 ⇒ 它不是固定 102 s 定时器，而是**带抖动的延迟/周期检测**。

### 15.2 SIGILL 定性：**异步 tgkill，不是 UnityMain 自己执行非法指令**

- 墓碑 `tombstone_19/22`：`signal 4 (SIGILL), code -6 (SI_TKILL)`，target tid = UnityMain；
- 崩溃 PC 落在 libil2cpp 的**正常函数入口**（运行时解密副本里对应 `sub sp, sp, #0x70; stp x29,x30,...` 之后），不是 `udf`/`brk`；
  ⇒ 信号是**别的线程 tgkill 打过来的**，UnityMain 只是被动中枪。
- 墓碑里 libFairGuard.so 的两个线程停在 `0x35D204`、`x8 = 0x65 (101 = nanosleep)` —— 它们**在睡**；全墓碑没有 `x8 = 0x82 (130 = tgkill)` 的线程，**发送者没被当场抓到**。
- 排除项：Unity 自身的 abort 走 `abort()` → SIGABRT，与 `SI_TKILL + SIGILL` 不符；官方原签包 160 s+ 不死，而补丁包 3/3 死在 101–157 s。

### 15.3 附带发现：libil2cpp 有**运行期全量 rwxp 解密副本**

```
734061d000-7347f9c000 rwxp 00000000  libil2cpp.so   ← 0x7980000，整段可写可执行
7347f9c000-7347fc5000 r-xp 0797f000  libil2cpp.so
```

磁盘上 `libil2cpp+0x3b96ec0` 是乱码；在 rwxp 副本的同一偏移上读出的是正常 ARM64 代码。
⇒ il2cpp 代码被运行期解密到可写可执行映射；**这也是静态反汇编 il2cpp 崩溃地址得到垃圾的原因**，任何 il2cpp 侧定位都必须在运行期副本上做（或用 base_rwxp + 偏移）。

### 15.4 未闭环

1. 异步 tgkill 的发送线程未抓到（墓碑里没有 x8=0x82 的线程）——需在死前对 `/proc/<pid>/task/*/syscall` 高频采样，或对 `tgkill` 桩（0x35D204 同段）下断；
2. 157 s 与 102 s 的差异来源（抖动定时器？某界面事件？）未定；
3. 阶段二的触发条件与 0x47E1C 是否属另一条路径，尚未分离验证。



## 16. sender 追查与「读守护」可行性（2026-10-20 03:10-03:14）

### 16.1 死点抖动（新数据）

| 轮 | 包 | 死点 |
|---|---|---|
| 1 | 双 NOP | 102 s |
| 2 | 单 NOP | 102 s |
| 3 | 单 NOP | 157 s |
| 4 | 单 NOP | **15 s**（03:10:52 启 → 03:11:07 SIGILL） |

第 4 轮 15 s 就死，**根本没到 30 s 闸门** ⇒ 坐实：**SIGILL 阶段与 0x2F08C/0x47E1C 的 exit 补丁完全独立**，两条机制互不相干；死点也不是定时器（15/102/157 s）。

### 16.2 sender：仍未确认（三条路都被平台堵死）

| 手段 | 结论 |
|---|---|
| 墓碑寄存器扫描 | tombstone_19/22 全部线程**没有 `x8 = 0x82 (130 = tgkill)`**；libFairGuard 两个线程停在 `0x35D204`、`x8 = 0x65 (101 = nanosleep)`，在睡 ⇒ 发送者执行完 syscall 后已回到用户态 |
| ftrace syscall tracepoint | **不可用**：`/sys/kernel/tracing/available_tracers` = `nop`，`events/syscalls` 不存在 |
| strace / gdb / ptrace 工具 | 设备均无（前面已实测） |
| simpleperf | 存在（`perf_event_paranoid = -1`），但采样抓不到微秒级 syscall |

⇒ 要抓 sender 必须有**自编 ptrace 注入器**（静态 aarch64 二进制）或内核 tracepoint，当前环境都不具备。

### 16.3 读守护（mprotect PROT_NONE）：**未实施，且当前不可行**

`mprotect` 必须在**目标进程内**执行。当前没有任何向目标进程注入代码的手段（无 ptrace 工具、无 frida 授权、无内核 tracepoint），所以「把 0xAB36000..0xAB4D000 改成 PROT_NONE 让 FairGuard 读崩」这条**做不了**。

可行的替代仪器（已搭一半）：

```
04_scripts/绿梦/fg_pagewatch.sh
  echo 1 > /proc/<pid>/clear_refs      # 清 accessed 位
  /proc/<pid>/pagemap                  # 取 PFN（root 实测可读，返回 0x0 合法）
  /proc/kpageflags                     # PFN 的 PG_referenced/active（root 实测返回 0x100000）
```

用途：判断 v2 签名块所在页（映射基址 + 0x6000）在某个时间窗内**是否被访问过**，从而不靠 hook 也能确定「读发生在启动还是延迟检测」。
当前障碍：脚本里嵌套引号把 `dd` 的 stderr 混进 `od`，取值为空；且死点已抖动到 15 s，采样必须前置到 8–12 s。

### 16.4 建议的下一步（按性价比）

1. **修 fg_pagewatch.sh 的引号**（把 `2>/dev/null` 移到 od 之前、改用单层 su -c 脚本文件 push 到 /data/local/tmp 执行），在 t=8/12/20 s 各测一次该页的 referenced 位；
2. 若要 sender/patch 点一步到位，需要能写一个小型 **ptrace 注入器**（attach → 在目标进程 mmap 一段 → mprotect(PROT_NONE) 该映射 → detach），这是目前唯一能同时拿到「读指令 PC」和「sender」的办法；
3. 纯静态方向已到边界（常量/字符串/dump 全 0 命中），不建议继续加码。



## 17. frida 读监视（fg_v2watch）实测：被检测，未拿到 reader PC（2026-10-20 03:16-03:20）

### 17.1 四次运行，全部没有 [HIT]

| # | 形态 | 结果 |
|---|---|---|
| 1 | `frida -U -f <pkg> -l fg_v2watch.js --no-pause` | CLI 直接报 `unrecognized arguments: --no-pause`（**frida 17.16 已移除该参数**，-f 默认恢复主线程） |
| 2 | `frida -U -f <pkg> -l fg_v2watch.js -q` | 脚本在 spawn 时执行，此时 base.apk 映射尚未建立 → `[!] maps 里没找到 offset=ab36000` |
| 3 | 轮询版（等映射再 arm） | 无任何轮询输出；进程 15 s 被 SIGILL 打死（`tombstone_27`，与无 frida 时同签名 SIGILL/SI_TKILL/UnityMain，uptime 15 s）⇒ **frida 并没有阻止周期检测** |
| 4 | attach 模式 `-n <pkg>` | `Failed to spawn: unable to find process`（进程 15 s 内就死，attach 窗口太短） |

### 17.2 发现的脚本 bug（已修）

`/proc/self/maps` 的 offset 字段是 **8 位补零** 的 `0ab36000`，而脚本用
`p[2].toLowerCase().indexOf('ab36000') === 0` 匹配 ⇒ 永远返回 1，**映射永远找不到**。
已改为 `parseInt(p[2],16) === 0xab36000`。

### 17.3 PROT_NONE 守护版（fg_v2guard.js）同样受阻

改用 `Memory.protect(range, '---')` 直接让读取者 SIGSEGV（墓碑 PC 即读取指令），脚本确认注入成功：

```
[load] fg_v2guard 已注入
Spawned ... Resuming main thread!
Failed to load script: session is gone
Process terminated
```

- **进程在 resume 后立刻被终止，且不产生任何墓碑** ⇒ 属于 `SIGKILL` 级别，不是业务崩溃；
- 对照：不用 frida 时同名进程至少活 15–157 s 且有 `tombstone_27`；
  ⇒ **FairGuard 检测到注入（gum）并立即 SIGKILL**，这条动态路线在当前环境走不通。

### 17.4 结论

- reader PC：**未取得**；tampered 标志地址：**未取得**；
- frida 侧唯一可能还有戏的是「attach 到已存活较久的实例」（本轮所有实例都死得太快，窗口不足）；
- 不依赖注入的替代：`clear_refs + pagemap + kpageflags` 页面访问观测（`fg_pagewatch.sh`，仪器已验证 pagemap/kpageflags 可 root 读，脚本引号待修）；
- 或自编 **ptrace 注入器**（不经 gum，不触发 frida 检测）：attach → 目标进程内 mprotect(PROT_NONE) 该映射 → detach → 等 SIGSEGV 墓碑拿 PC。



## 18. mod 侧 SIGILL handler 装机验证：活到 269 s，FairGuard 升级成 SIGTRAP（2026-10-20 03:24-03:29）

### 18.1 改动与产物

`Games/LvmengMod.cpp` 顶部新增（per-game 文件，仓库允许改）：

```cpp
#include <signal.h>
static void Starry_SigillSwallow(int, siginfo_t*, void*) { /* 返回即继续 */ }
__attribute__((constructor)) static void Starry_InstallSigillHandler() {
    struct sigaction sa; memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = Starry_SigillSwallow;
    sa.sa_flags = SA_SIGINFO | SA_RESTART;
    sigemptyset(&sa.sa_mask);
    sigaction(SIGILL, &sa, nullptr);
}
```

| 项 | 值 |
|---|---|
| `libstarry_magic.so` | 2,134,912 B / md5 `b7bcc3d69a41f0d5be2a113c08adb375`（`llvm-nm -D` 确认 import `sigaction@LIBC`/`sigemptyset@LIBC`） |
| 打入的 FairGuard | `libFairGuard_noexit.so`（md5 `5f4b4556…`，0x2F08C + 0x47E1C 双 NOP） |
| 组装 | 以 `08_output/绿梦/绿梦_1.0.5_starry-releaseSigned.apk` 为底 → `fg_so_swap.py` 依次换 `libstarry_magic.so` / `libFairGuard.so` → `surgical_equal_length_resign.sh --no-pad` 重签 |
| 产物 | `07_patched/绿梦/fg_test/绿梦_mod_noexit_sigill.apk` = **371,355,358 B / md5 `4ddf06a292738dd2f8c66255b0fd0b93`**，apksigner verify OK，`zipfile.testzip()=None` |
| 装机 | 覆盖装 **Success**（03:24:26），冷启 03:24:29（pid 23396） |

（附带修了 `fg_so_swap.py` 两个 bug：CD 的 uncompressed-size 未更新导致 CRC 校验失败；未压缩长度变化被硬拒。）

### 18.2 结果：**活到 269 s**（历史最长：102 / 102 / 157 / 15 s）

- 03:24:29 启 → 03:28:31（242 s）仍存活、无新墓碑；
- 03:28:4x 死亡：`/data/tombstones/tombstone_28`
  ```
  Process uptime: 269s
  pid 23396, tid 24881, name UnityMain
  signal 5 (SIGTRAP), code -6 (SI_TKILL)
  ```
- **信号从 SIGILL 变成 SIGTRAP** ⇒ SIGILL 确实被 handler 吞掉了，FairGuard **升级了惩罚信号**；
- 期间无 SIGSEGV / SIGABRT，也无 Java 崩溃。

### 18.3 结论

1. SIGILL handler 有效：把死点从 102–157 s 推到 **269 s**（+110 s 以上）；
2. FairGuard 的惩罚是**可升级的**（SIGILL → SIGTRAP，都是 tgkill 自伤）；
3. 继续按同一套路吞 SIGTRAP 风险上升（Unity/ART 自身也用 SIGTRAP 做断点/trap），且它可能再升级到 **SIGKILL（不可捕获）**；
4. 本轮未确认游戏是否真正进到主界面（观测窗结束时前台是 launcher，进程仍在后台存活）。



## 19. 自编 ptrace 注入器（aarch64）：编译通过，远程 mprotect 未打通（2026-10-20 03:31-03:34）

### 19.1 工具

`04_scripts/绿梦/fg_mprotect.c`（约 105 行）：
PTRACE_ATTACH → 解析 `/proc/<pid>/maps` 找 `base.apk` 且 offset 匹配的 `r--s` 段 → 读 regs →
在目标栈写 `svc #0` → 置 x8=226/x0=addr/x1=len/x2=0 → 停顿 → 读 x0 → 还原 regs 与指令 → DETACH。

编译（NDK 27）：
```
aarch64-linux-android21-clang -O2 -fno-emulated-tls -o fg_mprotect fg_mprotect.c
```
- **静态版（-static）被 bionic 拒绝**：`executable's TLS segment is underaligned: alignment is 8 (skew 0), needs to be at least 64 for ARM64 Bionic`；
- 改**动态 PIE**（9,024 B）后推送 `/data/local/tmp/fg_mprotect` 可正常执行。

### 19.2 结果：注入未生效

在 179 MB 包（`绿梦_noexit_resign.apk`，映射 `0xab36000`）上实测：

```
map: 0x7561f0a000-0x7561f21000 len=0x17000
mprotect(0x7561f0a000, 0x17000, PROT_NONE) -> 504154333184   ← 返回值是垃圾，syscall 没真的执行
```

- 先试 `PTRACE_SINGLESTEP` 跨 `svc`：拿不到返回值；
- 改 `PTRACE_SYSCALL` 入口/出口两次停顿：返回值仍是垃圾 ⇒ **远程 svc 桩（写在目标栈上）没有被正确执行**（D-cache/I-cache 同步或单步语义问题）；
- 目标进程随后仍以**原来的 SIGILL** 死亡（`tombstone_29`，pid 26158，uptime 15 s），**没有出现 SIGSEGV** ⇒ 该页并未被置为 PROT_NONE；
- 附带观测：**ptrace attach 没有被 FairGuard 立即反制**（进程照常跑到 15 s 的普通死点），说明 attach 本身不敏感，问题只在我们这边。

### 19.3 另一个坑：371 MB mod 包里 v2 块不在任何映射中

`绿梦_mod_noexit_sigill.apk` 的 `cd_off = 0x16212000`，进程里对应映射的 file offset 就是 `0x16212000`，
**签名块在映射起点之前**，没有被 ART 映射进来。
⇒ 做「读守卫」必须回到 179 MB 包（`0xab36000` 段，覆盖 `0xab3c000` 的 v2 块）。

### 19.4 修复方向（下一步）

1. 不要往目标栈写 `svc`：改为在**目标进程的 libc.so 里找现成的 `svc #0` gadget**（`syscall()` 内部），把 PC 指过去 + 设 x8/x0/x1/x2，用 PTRACE_SYSCALL 走到出口；
2. 或改用更成熟的做法：`PTRACE_ATTACH` 后直接在**已驻留的可执行页**里找 `svc`，并用 `PTRACE_SETREGSET` 完整保存/还原（含 pstate）；
3. 注入成功后再等 SIGSEGV 墓碑——其 `pc` 即 v2 块的读取者。
