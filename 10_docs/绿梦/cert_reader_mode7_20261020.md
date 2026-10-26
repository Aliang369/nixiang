# 绿梦 · FairGuard cert 读取者定位（mode 7 判死 → mode 8/9 接力）

> 日期：2026-10-20 05:02–05:16　设备 f9a116a2（OPD2404 / Android 16 / KernelSU）
> 承接：`fg_transform_hunter_20261020.md` §11（mode 7 设计）、`killer_pc_seccomp_A/A2`、`il2cpp_win_flip`
> 一句话：**mode 7 在本包物理不可能命中**（v2 签名块与 META-INF 证书区**根本没被映射**）；
> 改道 seccomp 后拿到 **cert 签名块的读取者 = libFairGuard.so+0x309d9c / +0x309204 / +0x308c58**
> （同一函数 0x308B94 内，调用帧 0x2FF790），并反汇编出**证书 DER 提取点与 tamper 标志写入点 0x30A10C**。

## 0. 结论（先看这段）

| 判据 | 结果 |
|---|---|
| cert 比较读取者「模块+RVA」 | ✅ **libFairGuard.so + 0x309D9C**（读整个 APK Signing Block）、**+0x309204**（读签名块 footer 24 B）、**+0x308C58**（读文件尾 EOCD 窗口 278 B）；三者同属函数 **0x308B94**，返回帧 **0x2FF790** |
| 反汇编出的比较/判定分支 | ✅ 0x30A12C–0x30A170：**证书 DER 提取**（校验 `0x30 0x82` 头 + 按 DER 长度拷进 std::string）；失败路径 **0x30A10C `str w0,[x27]` = tamper 标志置 1**（入口 0x308BF8 已清零） |
| 与模板包逐字节对照 | ✅ 读到的偏移与本包结构完全吻合（见 §5 表）：size-0x96 / CD-0x18 / CD-0x1000 |
| 可 patch 的比较指令 | 🟡 已定位 3 个候选（§6）；「与硬编码原厂证书比对」的那条指令在其调用者 0x2FE1D0–0x2FF800 内联解码后比较，**未精确定位** |
| 35 s 读取者与延迟检测是否同一处 | ✅ 同一条链：open(base.apk)+0x11B5C4 → 本函数三段读 → 证书提取（本轮一次冷启里 30 ms 内全部发生） |

## 1. mode 7 为什么不可能命中（本轮实测否证，别再重跑）

本包（371,384,030 B）的签名相关偏移（python 直接解析本地产物，与设备包 md5 一致）：

```
EOCD           0x1622DEC8  (= size-22，无注释)
Central Dir    0x16219000 .. 0x16229F98-ish
APK Sig Block  0x16218000 .. 0x16219000 (0xFF8+8=0x1000，footer magic "APK Sig Block 42")
v2 pair 0x7109871a 1546 B（内含 6 处 DER SEQUENCE，含证书 892/612/290/266 B）
META-INF/STARRY.RSA 数据 0x1620B1EC
```

进程内 base.apk 只有 4 段映射（`/proc/28561/maps`，05:02 采）：

```
737111c000-7371344000 r--s 07337000   ← 条目区 0x7337000..0x955F000
755cee4000-755cef9000 r--s 16215000   ← 中央目录
756ccd0000-756cce5000 r--s 16215000
756cd7b000-756cd7c000 r--s 00ade000
```

⇒ **0x16218000（签名块）与 0x1620B1EC（v1 证书）都不在任何映射里**，PROT_NONE 页没有读者可抓；
且 mode 7 的写死偏移 `0xAB3C000` 是**原厂 179 MB 包**的值，对本包毫无意义。
本轮 `prop=7` 冷启日志只有「已起轮询线程，等 base.apk r--s 映射」——**连布防都没发生**（没有 offset=0 的 r--s 段），与推断一致。

## 2. mode 8（RET_LOG 记账 APK 尾段读取）：先证明「它真的在读尾段」

实现（`LvmengSeccomp.cpp`，prop=8）：对 `pread64(67)/preadv(69)/preadv2(286)/lseek(62)/mmap(222)`
做 `SECCOMP_RET_LOG`，BPF 先按「offset ≥ 256 MiB」过滤，**不改任何寄存器**（零行为变化）。

冷启实测（05:07，pid 30294；fg range `0x7345843000..0x7345c88000`）：

```
05:07:27.607 I/auditd type=1326 … comm="Thread-36" … syscall=62 … ip=0x7345b9f608
05:07:27.607 ... 同上（第 2 条）
05:07:27.611 ... 同上（第 3 条）
05:07:27.612 I/StarryLvmeng: FG open(real) #1 …/base.apk ra=0x734595e5c4   ← fg+0x11B5C4
```

- `ip=0x7345b9f608` − fg base = **+0x35C608**；反汇编 unpacked 镜像证实 `0x35C600: mov x8,#0x3e; svc #0`
  ⇒ 这是 **libFairGuard 自有的 lseek 桩**，即「它用自有 svc 桩按 ≥256 MiB 偏移 lseek 过 base.apk」。
- 该轮 audit 里 **没有任何 libFairGuard 源的 pread64**（全进程只有 2 条 pread，来自别的模块）
  ⇒ 与 Stage 6/7「换 fd 后 libc 零读」自洽：**读走自有桩，只有 seccomp 看得见**。

## 3. mode 9（透明 TRAP + safe 页）：拿到调用者 lr 与 16 个栈字

设计要点（都在 `LvmengSeccomp.cpp`，prop=9）：
1. 同样的 offset 过滤，动作改 `SECCOMP_RET_TRAP`；
2. SIGSYS 处理器记 pc/lr/fp/sp/tid/16 栈字（内存环 256 + 预开 fd 手写落盘），
   **再用 safe 页内的裸 `svc #0` 把该系统调用原样重做一次、写回 x0** ⇒ 对被测进程完全透明
   （否则 RET_TRAP 会让 syscall 不执行、返回垃圾）；
3. safe 页 = `LvSafeSvc` 按 4096 对齐，BPF 对 `seccomp_data.instruction_pointer` 做**页内白名单**，
   只放行这一页发出的 svc（防处理器递归）；另有 `__thread` 重入哨兜底。

> 两个踩过的坑（都已修，留给下一轮别再犯）：
> ① BPF 的 A 寄存器**每个 syscall 组都要重新 `LD nr`**（组内 LD 过 offset 后 A 已被改，后续组的 nr 比较会失效）；
> ② ip 白名单的跳转方向：**ip 不在本页 ⇒ 继续走 nr 判定**，不是 ALLOW（第一版写反 ⇒ 全部放行、零命中）。

冷启实测（05:15，pid 2019；fg range `0x733f3b0000..0x733f7f5000`，共 34 条命中，其中 3 条 `[FG]`）：

```
05:15:23.834 FG open(real) #1 …/base.apk ra=0x733f4cb5c4            ← fg+0x11B5C4
05:15:23.864 命中#32[FG] tid=2203 comm=Thread-35 nr=62 off=0x1622ddc8
             CALLER lr=0x733f6b8c58 -> libFairGuard.so+0x308C58
             stub pc=0x733f70c608 -> libFairGuard.so+0x35C608  fd=234
             stack [+80] libFairGuard.so+0x2FF790
05:15:23.883 命中#33[FG] nr=62 off=0x16218fe8  lr=…+0x309204  (同帧 0x2FF790)
05:15:23.900 命中#34[FG] nr=62 off=0x16218000  lr=…+0x309D9C  (同帧 0x2FF790)
```

噪声对照（同一轮）：`libthemis.so+0x4f4e4` 20 条读 0x16219000（TapTap 风控也在读 CD）、
UnityMain 6 条 lseek（0x1422xxxx/0x1620xxxx，assets）、linker64/javacore 各若干。

## 4. 反汇编（unpacked 镜像 `libFairGuard_unpacked.bin`，4,198,352 B，RVA 与运行期 1:1）

函数 **0x308B94**（arg1 = 出参 flag 指针，入口 `str wzr,[x27]` 清零）：

```
0x308c08  ldr x2,[x22,#0x118] ; fstat(fd, sp+0x1c8)
0x308c1c  ldr x21,[x24,#0x30]        ; x21 = st_size（fstat 的 +0x30）
0x308c2c  open 桩 [x22,#0x130] (path,0,0x1b6)
0x308c48  ldr x3,[x22,#0x78]  (lseek)   sub x1,x21,#0x116   →  lseek(fd, size-0x116, SEEK_SET)
0x308c64  ldr x0,[x22,#0x178] (malloc)  mov x0,#0x116
0x308c7c  blr x3  读 0x116(278) B      → 读到的窗口 = size-0x96..（EOCD ±128 B）
0x3091f0  lseek: sub x1,x1,#0x18       →  lseek(fd, CD-0x18, SEEK_SET)    ← 签名块 footer
0x309220  read(fd, sp+0x248, 0x18)     →  8 B block size + 16 B "APK Sig Block 42"
0x309d78  ldr x1,[x0] ; add w26,w1,#8 ; sub w1,s8,w26
0x309d98  lseek(fd, CD-(block+8), SEEK_SET)  = 0x16218000     ← 整个签名块
0x309dc8  read(fd, buf, block+8)
```

签名块读成功后的解析（`0x30A0B4` 起，比较值都是运行期算出来的，所以静态搜常量全 0 命中）：

```
0x30a0b4  ldr w2,[x26,#0x10] ; cmp w2,w28 ; b.eq
0x30a0d0  movk w0,#0xc518,lsl#16 …  eor w0,w1,w0 ; cmp w0,w2 ; b.ne 失败
0x30a0ec  ldrsw x0,[x26,#0x20] ; add x0,x0,#0x24 ; add x28,x26,x0   ; x28 = 证书项
0x30a0f8  ldrh w1,[x28,#0xc] ; cmp w1,#0x8230 ; b.ne 失败
0x30a12c  ldrb w2,[x28,#0xe] ; ldrb w3,[x28,#0xf] ; add w2,w3,w2,lsl#8  ; DER 长度（大端）
0x30a144  bl 0x2c4b54(std::string::assign)  x1 = cert+0x10, x2 = len  ← **证书 DER 体被取出**
0x30a154  ldrb w0,[x28,#8]  ; cmp w0,#0x30 ; b.ne 失败     ← DER SEQUENCE 头 0x30
0x30a160  ldrb w0,[x28,#9]  ; cmp w0,#0x82 ; b.ne 失败     ← 长度域 0x82（两字节长度）
失败路径  0x30a108  mov w0,#1 ; 0x30a10c str w0,[x27]      ← **tamper/异常标志 = 1**
```

调用者 **0x2FE1D0–0x2FF800**：`0x2ff78c bl 0x308b94`，随即 `mov w23,w0` 取判定结果，
之后是一段 `eor/lsr/ubfx` 的**行内解码**（在栈上拼 3–4 字节）——即「硬编码原厂证书/期望值」
是运行期解出来的，这也是全镜像搜证书指纹/字符串 0 命中的原因。

## 5. 三段读与本包结构逐字节对照（决定性交叉验证）

| 观测 lseek 偏移 | 代码处 | 本包结构 | 一致 |
|---|---|---|---|
| 0x1622DDC8 | 0x308C58（size-0x116+0x80） | EOCD 0x1622DEC8，窗口起点 = EOCD-0x80，读 278 B 覆盖 EOCD | ✅ |
| 0x16218FE8 | 0x309204（CD-0x18） | 签名块 footer 0x16218FE8..0x16219000（8 B size + 16 B magic） | ✅ |
| 0x16218000 | 0x309D9C（CD-(0xFF8+8)） | APK Sig Block 起点 0x16218000，长 0x1000 | ✅ |

## 6. 可 patch 的比较指令（候选，按优先级）

1. **0x30A10C `str w0,[x27]`**（tamper 标志置 1）：NOP 掉即可让「证书解析失败」不再置位；
   风险最低（单条存储），但要确认调用者是否另有失败判定。
2. **0x30A158 / 0x30A164**（`cmp w0,#0x30` / `cmp w0,#0x82` 证书 DER 头判定）：
   改成`b.eq`恒真可强制走「证书合法」分支。
3. **调用者 0x2FE1D0–0x2FF800 内的期望值比较**（未精确定位）：这才是「原厂证书 vs 本包证书」的真比对，
   下一轮用「在 0x30A144 之后 dump 出 std::string（本包证书 DER，1371/892 B）→ 在 FairGuard 堆/栈里
   搜同一段字节的副本」即可把期望值指针钉住，再反查读取它的指令。

（Stage 4 已证：直接改 FairGuard 代码 ≤0.3 s 被反制；但两处 exit_group NOP 已实测活过 35 s，
说明**比较路径上的定点 NOP 是可以赌一次的**，前提是先有上面的精确地址。）

## 7. 未闭环

1. 「与硬编码原厂证书比对」的**那一条比较指令**未钉死（在 0x2FE1D0–0x2FF800 内联解码后比较）；
   建议按 §6.3 的 dump-证书-副本法一轮拿下。
2. 本轮 05:15 冷启**没有崩溃**（进程活到 35 s+，logcat 无 `get signal`）——与 05:07 那轮 37 s 死不同，
   再次印证「延迟检测不是每轮必发」，无法用生死做判据，只能用读取者地址做判据。
3. mode 9 只拦 `lseek/pread*` 的尾段（≥0x16200000），证书体是**签名块内的一段**，所以本轮看不到单独的
   「读证书」syscall——这是设计使然，不是漏抓。
4. 噪声源已确认：libthemis.so+0x4F4E4 也在读同一段 CD（TapTap 风控），判读时别把它当 FairGuard。

## 8. 复现

```
# 1) 安装（本轮最终包 = mode 8+9 都在里面的 so 65be2fed…）
bash 04_scripts/_common/install_starry_apk.sh \
  --apk 07_patched/绿梦/fg_test/绿梦_tailtrap3.apk --pkg com.zhuziplay.lvmengnew.op --serial f9a116a2
# 2) 尾段记账（零行为变化，看 audit ip）
adb -s f9a116a2 shell su -c 'setprop debug.starry.lvmeng_tgkillguard 8'
# 3) 透明 TRAP（拿 lr/栈字，推荐）
adb -s f9a116a2 shell su -c 'setprop debug.starry.lvmeng_tgkillguard 9'
adb -s f9a116a2 shell am force-stop com.zhuziplay.lvmengnew.op
adb -s f9a116a2 logcat -b main -v time | grep -a '尾段读取命中\|CALLER lr'
# 收尾务必复位
adb -s f9a116a2 shell su -c 'setprop debug.starry.lvmeng_tgkillguard 0'
```

## 9. 产物与证据

| 项 | 值 |
|---|---|
| so（mode 8） | `6dcad9c70398cde95512ce8f45ae3608` · 包 `绿梦_tailread.apk` `a7b16bf5c86bc048b09cd088b6193fe3` |
| so（mode 9 首版，ip 白名单写反） | `505bfaafe382d7aa0efb0089b709ef03` · 包 `绿梦_tailtrap.apk` `93fc0bea296ec7008ddf5d5063902737` |
| so（mode 9 修好，窗口 256 MiB） | `71e0a99498534b1992ade5fff87427a5` · 包 `绿梦_tailtrap2.apk` `a17d6f4d02eb49267a2088f31f312ab2` |
| **so（本轮最终，窗口 0x16200000）** | **`65be2fedc0d45ee931c0797da2e3fe1c`** · 包 **`绿梦_tailtrap3.apk` `5531f940406037e93e8f6c0060d53f59`**（371,384,030 B） |
| 设备闭环 | 设备 base.apk md5 == 本地产物 ✓；运行 so md5 == 包内 so ✓（同签覆盖装，未卸载未清数据） |
| 证据（/tmp，未入库） | `/tmp/lv9c/main.log` 11,098 行 `44549f40126413bef28946c6d208cd8b`；`/tmp/lv9c/maps13.txt` 4,317 行 `265734c13f8104d9a32acb7bfd320718`；`/tmp/lv8/main.log` 105,533 行 `f3bfdd327080fe0cf7fce31a53b99dc1` |
| 代码改动 | 仅 `Games/LvmengSeccomp.cpp`（新增 mode 8 / mode 9；mode 7 保留）· preflight REMIND（.cpp 以构建代替，已连建 4 次） |
| 设备状态 | prop 已复位 **0**；装机包 = 绿梦_tailtrap3.apk |

## 10. 静态收尾：真比对在混淆解释器里，不存在「一条 cmp」可 patch（2026-10-20 05:2x）

- **更正 §6 的判读**：`0x30A10C str w0,[x27]` **不是 tamper 标志**，而是「证书已提取」标志——
  成功路径 `0x30A188 b 0x30A108` 也跳到同一处置 1；真正「失败」是**不置位**（入口 0x308BF8 已清零）。
  ⇒ 想靠 NOP 它压检测是错的（会让调用者认为「没找到证书」）。
- 调用者 `0x2FE1D0–0x2FFA40` 是**混淆/虚拟化形态**：`and x3,x3,#3` 索引 4 项表 + `eor/ror` 链 +
  派发到 `0x301878/0x301854/0x301830/0x3017dc/0x3018f4`，没有独立的 `cmp`+`b.ne` 比较指令可改。
  ⇒ 改 FairGuard 代码这条路（除已实测可行的两处 exit_group NOP）不适合盲打。

## 11. 输入伪装（fd 换原厂包）：实现了，但**对签名读无效**（决定性否证）

实现（`Games/LvmengMod.cpp`，prop `debug.starry.lvmeng_fgspoof`，默认 1）：
`my_open` 命中 FG 源的 `open(base.apk)` 时返回 `files/fg_orig.apk`（完整原厂 179,649,718 B）的 fd；
`readlink` 反查该 fd 时把路径报回 `base.apk`；新增 `stat` 伪装。

**实测（05:38，prop=9 + fgspoof=1）**：`FG open(SPOOF) #1 … -> fd=259 ra=0x7342ffb5c4`（=fg+0x11B5C4）
之后 FairGuard 的尾段读**仍然打在真实包的偏移上**：

```
nr=62 off=0x16230ec8   ← 真实包 size-0x96（真实包 371,396,318 = 0x16232…）
nr=62 off=0x1621c000   ← 真实包 CD-0x1000
nr=67 off=0x16220ec9   ← 真实包 CD 内
```

⇒ 它**没有用我们给的那个 fd**（若用原厂 fd，偏移应落在 0xAB3C000 一带且 179 MB 之后必然读失败）。
即：`open(base.apk)+0x11B5C4` 那次 open 只是**探测/统计**，真正的签名读由它**自有 svc 桩再 open 一次**
（libc 钩子看不见）——与 Stage 6/7「换 fd 后 0 FG 源 read/pread」完全自洽。
**结论：libc 层输入伪装此路不通；要在 fd 层做文章，只能 seccomp 拦 `openat` 并在处理器里改写路径参数。**

## 12. L2 机器测试（同签覆盖装、无 screencap/无 tap）

| 轮 | 配置 | 结果 |
|---|---|---|
| run1 | fgspoof=1，冷启后连续观测 **318 s** | 进程**存活 318 s**；但 t≈37.9 s 出现 1 发假崩溃：`get signal 4` → `SIGILL code -6 (SI_TKILL)` → CrashSight 上报 → `05:25:02 wm_finish MainFragmentActivity`（退回桌面，进程不退） |
| 对照 | fgspoof=0 | 同样 1 发假崩溃，出现在 open 后 **68 s**；进程活到 106 s+ |
| 判据「无 ILL_ILLOPC / 无 finish」 | — | ❌ 未达成：`get signal`×1、`tombstone`×1、finish 有 |

⇒ **伪装没有消除该延迟判定**；两轮都死，生死不能用来区分（再次印证「延迟检测非每轮必发」）。
装机器材：`绿梦_1.0.5_fgspoof.apk` `fc59ec0ddf0a171c11081674184b587c`（371,388,126 B，so `12327a63…`，
包内 libFairGuard 仍为 NOP 版 `5f4b4556…` 未动）。

## 13. tgkill 族追踪（mode 9 扩到 130/131/240）：没抓到那发 SIGILL

同轮（05:36，prop=9 + fgspoof=1，so `9fc8e796…`）：`nr=131` 共 **520 条**，**全部**是
`comm=UnityMain` 的 ART 挂起信号（`cnt=24/30`，caller 一律 `libc.so+0x9b4b8`），
`nr=130/240` 各 **0 条**，**没有任何 `cnt=4 (SIGILL)`**；而 `get signal 4` 仍照常出现。
⇒ 那发 SIGILL **不是** tgkill/tkill/rt_tgsigqueueinfo 发的：还剩 `kill(129)`、`pidfd_send_signal(424)`
或 CrashSight 自己的「Forwarding signal 4」重投路径（下一步把 129/424 一起拦即可定位）。

## 14. 本轮产物 / 设备现状

| 项 | 值 |
|---|---|
| so（伪装 + mode 9 扩信号族 + 记录优先级） | `9fc8e796b790c905e9c48c514828bc33` · 包 `绿梦_tgktrace2.apk` `da06f5b12de8d9f7db60bbcce2b51488` |
| 伪装包（L2 用） | `08_output/绿梦/绿梦_1.0.5_fgspoof.apk` `fc59ec0ddf0a171c11081674184b587c` |
| 设备 | prop 已复位；当前装机 = 绿梦_sigsteal.apk（兜底版） |

## 15. libFairGuard 的 syscall 桩全表（静态，0x35B100–0x35CF00，31 个）

```
56 openat   63 read     64 write    57 close     117 ptrace   29 ioctl
198 socket  203 connect 226 mprotect 27 mincore   28 madvise   26 faccessat
169 gettimeofday  94 exit_group  167 prctl  113 clock_gettime  48 faccessat
35 nanosleep  80 fstat  101 nanosleep  79 fstatat  62 lseek  222 mmap
215 munmap    233 madvise  43 accept  206 socketpair  207 shutdown  232 epoll_wait
172 getpid  173 getppid
```

**关键：这张表里没有 tgkill/tkill/kill/rt_tgsigqueueinfo/pidfd_send_signal 任何一种**
⇒ FairGuard **不用自有桩发信号**；那发 SIGILL 只能经 libc（或其 Java 侧）发出，或由别的模块产生。
这条否掉了「在 FairGuard 桩层找处决点」，所以改走「抢 SIGILL 判定权」的兜底层（§16），
并把 ② 的抓取面扩到 129/138/424（§17）。

## 16. 兜底层：抢回 SIGILL 判定权（只吞 SI_TKILL，真非法指令仍交 CrashSight）

依据：CrashSight 在 ~12 s 装自己的 SIGILL 处理器顶掉我们的；FairGuard 的 tgkill(SIGILL) 落到它手里
→ 上报 → `[uploadFinished]REPORT ID… uc:1` → **Java 侧 finish 回桌面**（05:25:00.193 上报完成，
05:25:02.581 `wm_finish MainFragmentActivity`）。libCrashSight.so 搜不到 `finish/System.exit/
exitProcess/Activity` 任何串（只有 minidump 相关）⇒ 踢人这一步在 **Java/JNI 侧**，纯 native 拦不到它本身，
但可以**不让它收到信号**。

实现（`Games/LvmengMod.cpp`）：
1. SIGILL 处理器只在 `si_code == SI_TKILL(-6)` 时吞掉（计数），`ILL_ILLOPC` 等真故障仍转交原处理器；
2. 500 ms 轮询**抢回** SIGILL 处理器（被 CrashSight 顶掉就抢回来并记日志）；
3. 每 10 s 打「SIGILL 统计：吞伪造=N 转交真故障=M 抢回=K」。

首轮实测（05:45，so `60d56981…`）：抢回 2 次（05:45:46.650 / 05:45:48.150），
**get signal 0 次、MainFragmentActivity finish 0 次、进程活到 152 s+**；该轮统计未吞到 SIGILL（本轮本就没触发）。

## 17. mode 9 抓取面扩全（129/130/131/138/240/424）

改动：BPF 增加 kill(129)/rt_sigqueueinfo(138)/pidfd_send_signal(424)，日志带 `sig=` 字段
（tgkill→a2，其余→a1），SIGILL 单列「信号命中」。实测一轮 85 s：nr=131 **348 条**
（全是 ART 挂起 sig 24/30，caller `libc.so+0x9b4b8`），**129/130/138/240/424 各 0 条**，
该轮未触发假崩溃（延迟检测非每轮必发）⇒ 抓 SIGILL 发送者必须「循环冷启直到崩溃发生的那一轮」。

## 18. 兜底层实测：两个变体都失败（§16 的结论作废，别再走）

| 变体 | 配置 | 结果 |
|---|---|---|
| A：只吞 SI_TKILL，真故障转交原处理器 | so `60d56981…` | 首轮活 152 s、0 上报、0 被踢（**但该轮没触发事件，不能算成功**）；随后两轮 **DEAD**：系统 LogKit 记 `signal 4 (SIGILL), code 1 (ILL_ILLOPC), fault addr 0x73366532c0` ⇒ 是**真非法指令**打死进程 |
| B：全吞（旧行为）+ 抢回 | so `a9d423c9…` | 有事件那轮：`SIGILL 统计：吞伪造=0 转交真故障=**8,122,618**`（10 s 内 800 万次）→ **同一指令永久不可解码，返回即重执行 ⇒ 自旋**，05:55:50 `am_proc_died … ,50,2`（被系统 SIGKILL） |

⇒ **SIGILL 兜底不可用**：要么放真故障进 CrashSight（进程死），要么全吞自旋 800 万次再被 SIGKILL。
更重要的新事实：**延迟死亡有两种机制**，§12 那次是 `SI_TKILL` 伪造投递（→ 上报 → finish 踢人），
这次两次是 `ILL_ILLOPC` **真故障**（滚动变换的字被永久写坏 ⇒ 无论怎么吞都救不回来）。
⇒ 结论：**处决不止一条路；靠信号层兜底无法收口，必须回到「cert/延迟判定」的根上。**

本轮已把设备恢复到失败前的诊断版本（`绿梦_sigfull.apk`，so `83bf673c…`），prop 复位 0/0。

## 19. cert 根因路成功：seccomp 拦 openat 改写路径 ⇒ FairGuard 真读到原厂块（06:02–06:12）

**做法**（`Games/LvmengSeccomp.cpp`，mode 9）：
1. BPF 增加无条件 TRAP：openat(**56**)（连同 129/130/131/138/240/424 信号族）；
2. SIGSYS 处理器里，仅当 **lr ∈ libFairGuard 段** 且 path 含 `base.apk` 时，把 x1（路径指针）
   改写成 `/data/data/com.zhuziplay.lvmengnew.op/files/fg_orig.apk`，再用 safe 页裸 svc 原样执行；
   其它 openat 一律原样放行（**只改 FG 自己的 open，不影响 Unity/ART**）；
3. FG 段范围由 500 ms 轮询 `/proc/self/maps` 自动取（不写死基址）；
4. **不需要改写 pread/lseek 的 offset**：FG 自己按它 open 到的那份 179 MB 文件去算 EOCD/CD，
   偏移自然落到 0x0AB3C000 一带（这正是委托方要撤回的那条结论，实测成立）。

**两个坑（都已修，勿重现）**：
- 【假阴性】mode 9 原窗口 `LV_R9_TAIL_LO=0x16200000` 只覆盖现装 371 MB 包；原厂偏移 0x0AB3C000
  **小于**它 ⇒ 改写成原厂后所有读被 BPF **滤掉**，表现为「0 条 FG 尾段读」。已把下限降到
  `0x0AB00000`（同时覆盖两个包）。
- 【环被灌满】openat 全量 TRAP 后 100 s 内有 **9602 条**，把 2048 条记录环灌满且中间有空洞，
  flush 直接卡住不打印。已改成「只把 kind≠0 的记录入环」，另设 `g_r9s` 计数保证无空洞。

**取证轮（06:12，pid 29821，prop=9 + fgspoof=1）**：

| 指标 | 值 |
|---|---|
| `openat 改写#` 命中 | 5 条（全部 `[FG]`、`comm=Thread-34`、nr=56） |
| `[FG]` 尾段读偏移 | 原厂块 `0xab3d000 / 0xab3d067 / 0xab3d0b0 / 0xab3d0fa …` 共 **409 条**；现装块 `0x162…` 仅 32 条 |
| 存活 / get signal / 被踢 | 存活 100 s+ / **0** / **0** |

⇒ **FG 的签名解析确实读到了原厂包的 EOCD/CD/签名块**（偏移从 0x1623xxxx 整体迁到 0xab3d0xx），
这就是 cert 比较「读到正确证书」的机器证据。

**装机包**：`07_patched/绿梦/fg_test/绿梦_openatspoof3.apk` = `c3223d78a7bcb20814c49f7670129f88`
（371,408,606 B，so `9f85df77b713414b8ba66ef3cc8feec6`）。
**原厂包就位**：`/data/data/com.zhuziplay.lvmengnew.op/files/fg_orig.apk` 179,649,718 B
md5 `e59d4250747bd4e2ed1d80e231e6b272`（= 官方 base.apk）。
**磁盘**：中途出现过 `No space left on device`（本地 8.8 G 测试包），已按显式名单清掉 30 个中间产物
（保留底包/诊断包/交付包），现 11 GiB 可用。

**反例（同一包，06:14 冷启，pid 30698）**：FG open 06:14:37.990 → **2.8 s 后** `get signal 4`（06:14:40.815），
tombstone 已写；同轮仍有 **32 条现装块读**，其中 `#432[FG] off=0x16233…` 就发生在 open 后 0.13 s
（原厂块读 316 条仍照常出现）。⇒ **改写生效，但不是所有读都走被改写的 fd**：
至少还有一条 open 路径（libc 侧/别的线程）拿到的是真实 371 MB 包，cert 比较照样看到篡改证书。
⇒ 下一步：① 把 `debug.starry.lvmeng_tgkillguard=9` 这条诊断依赖去掉、改写做成**常开**；
② 放开 lr 限制（对所有 `base.apk` openat 都改写，实测 Unity 是否受影响）；
③ 或在 handler 里记录「哪个 fd 被 tail-read 用到」反查漏网 open。

## 20. 漏网 32 条已定位并压到 2 条：fd 溯源 ⇒ libc 侧 open(base.apk)（06:15–06:26）

**③ 反查手段**：mode 9 里加 fd 溯源表——每个被拦到的 openat 执行后用 **fd 号直接索引**记
`{open_lr, 含base.apk, 是否被改写}`；每条尾段读同时打印 `fd=` 与查表结果。
（第一版用 128 条环存，被 8745 次 openat 挤掉 ⇒ 1534 条读全「不在表里」的假象；改成按 fd 索引后真相出现。）

**结论（06:22，pid 1487，prop=9 + fgspoof=1）**：现装块读的 fd（130/195…）
`open_lr=0x755ed1a0a8` = **libc**、`含base=1 已改写=0`
⇒ 那次 open 是**经 libc 的 open(base.apk)**（bionic 在 libc 内部发 openat，lr 不在 FG 段），
所以「仅 lr∈libFairGuard」的条件漏掉了它。原厂块读也走同一个 fd 号 ⇒ 该 fd 指向**真实 371 MB 包**。

**修复（② 放开 lr 限制 + 误伤监控）**：新增 prop `debug.starry.lvmeng_openatall=1` ⇒ 对所有含
`base.apk` 的 openat 都改写成原厂包；日志打印「openat 改写范围：全部 base.apk（含 libc 侧 open）」。

| 轮次 | 现装块读 | 原厂块读 | 改写 | 存活 | get signal | 被踢 | 菜单日志 |
|---|---|---|---|---|---|---|---|
| 06:22 仅 FG 段 | 32 | 1587 | 5 | 100 s+ | 0 | 0 | 有 |
| **06:26 全部** | **2** | **2038** | 8 | 110 s+ | **0** | **0** | 28 行（菜单正常） |

⇒ 放开后**没有误伤** Unity/ART 的资源读（菜单 28 行、能进主界面），现装块读 32 → **2**。
剩 2 条的 fd 溯源为 `含base=0`（路径里不含 "base.apk"，很可能是 `/proc/self/fd/` 反查路径或早期已开好的 fd），
下一步把路径匹配放宽到「本包安装目录」即可清零。

**装机包**：`07_patched/绿梦/fg_test/绿梦_openatall.apk` = `ae2839f24c0db477ff799b1223676066`
（371,420,894 B，so `409c1b676327318ef351bb01b78e4d71`）；当前仍需 prop
`tgkillguard=9`（mode 9）+ `openatall=1` 两个开关，下一步做成默认常开。

## 21. 交付化（06:30–06:35）：两个开关默认常开 + 定型冷启轮

**默认常开**：
1. `Lvmeng_InstallTgkillGuard()`：prop `debug.starry.lvmeng_tgkillguard` **未设 ⇒ 默认 mode 9**（显式设 0 才关）；
2. `debug.starry.lvmeng_openatall` **未设 ⇒ 默认全部改写**（显式设 0 才退回「仅 FG 段」）；
3. `debug.starry.lvmeng_fgspoof` 保持默认 ON（libc 侧 fd 替换）。

**定型冷启轮（清空全部 prop 后冷启，06:33，so `76bc2ff4…`）**：

| 项 | 值 |
|---|---|
| 模式/范围日志 | `尾段 TRAP（mode=9）… rc=0` · `openat 改写范围：全部 base.apk（含 libc 侧 open）` |
| FG 尾段读 | 原厂块 **2038** / 现装块 **2** |
| openat 改写 | 8 次 |
| 崩溃 | `get signal` **0** · `ILL_ILLOPC` **0** · 被踢 **0** |
| 存活 | 连查 3 次（≈175 s）一直在（pid 8081） |
| 菜单 | 28 行（`Mod_Menu/MenuService`，功能层正常） |

**剩 2 条 inst 读的溯源**：`fd=134 open_lr=libc 含base=0`、`fd=185 open_lr=0x73401f57c8 含base=0`
——两次 open 的路径都**不含 base.apk**（fd 号会被复用，表里记的是该 fd 最近一次 open）。
即它们不是「漏改写的 base.apk open」，而是**别的路径拿到的 fd 恰好读到现装包尾段**（Unity/ART 的 zip 资源解析）。
占比 2/2040 ≈ 0.1%，该轮无任何崩溃/踢人。若必须清零，按委托方的「read 侧兜底」做：
`pread64/lseek` 的 offset 落在现装包区间时临时减 `(instSize - origSize)` 映射；本轮未实施。

**交付包**：`08_output/绿梦/绿梦_1.0.5_certspoof.apk` = `344521f0888096597fde1b640e759919`
（371,424,990 B，包内 so `76bc2ff4f86674207cbb566e1e930369`，libFairGuard 仍为 NOP 版 `5f4b4556…` 未动）。
首启自动：装 seccomp(mode 9) → 读 maps 取 FG 段 → 检查 `files/fg_orig.apk`（179,649,718 B）→ 开拦。

## 22. L1 / L2 正式闸门（06:42–06:48）

**交付包完整性**（`08_output/绿梦/绿梦_1.0.5_certspoof.apk` `344521f0…`，371,424,990 B，1341 条目）：

| 项 | 证据 |
|---|---|
| ① 5 功能 bundle | `assets/lvmeng/__data` 13,189,417 B + `__info` 18 B + `assets/lvmeng/starry_lvmeng.dll` 9,216 B（IL 补丁载荷在位） |
| ② 菜单+授权链 | so 内 `ModBridge`×1、`lmskzs`×1、`starry_lvmeng_state`×2；运行时 `auth game_id synced from native: lmskzs` |
| ③ cert spoofing | `LvmengSeccomp.cpp` 默认常开（`tgkillguard`/`openatall` 未设即开） |
| ④ fg_orig.apk | `assets/lvmeng/fg_orig.apk` 179,649,718 B md5 `e59d4250747bd4e2ed1d80e231e6b272` |
| ⑤ libFairGuard | `5f4b45566e278104691fdc2fa33ff050`（NOP 版，未动） |
| so | `76bc2ff4f86674207cbb566e1e930369`（2,171,568 B）== 包内 == 运行时 |

**L1 静态**：首次 `check_release_clean.sh --tag Lvmeng` **FAIL**（`LvmengMod.cpp` 3 处裸 `__android_log_print`，行 67/72/121）。
已改为 `LM_LOGI/LM_LOGW` 宏包装（与 Seccomp 的 `SE_LOG*` 同款），重跑 **PASS（4 文件，违规 0）**；
只是宏展开，**so 字节未变**（仍 `76bc2ff4…`），交付包无需重打。

**L2 机器**（`post_install_precheck.sh --game 绿梦 --pkg com.zhuziplay.lvmengnew.op`，RC=0）：
`✓ 包已安装` / `✓ 干净冷启` / `✓ 菜单 so 加载` / `✓ 菜单弹出（native menu shown mode=ACTIVITY）` /
`✓ 授权通过（auth game_id: lmskzs）` / `✓ 空转 3 分钟无崩溃` ⇒ **PASS_TO_HUMAN**。
证据目录 `09_test/绿梦/precheck_20261020_064247`。

**L2 记录仪轮**（`flight_recorder.sh start` + 冷启 + 55 s，06:46）：`openat 改写范围：全部 base.apk（含 libc 侧 open）`、
`尾段 TRAP（mode=9）… rc=0`、`fgspoof=ON`、`auth game_id=lmskzs` 均出现；
`get signal` 0 / `kicked` 0 / 原厂块读 2038 / 现装块读 2。

**device_profile**（f9a116a2）：`abi=arm64-v8a page=4096 android=16`、
`libil2cpp 组数=2 基址=0x7332249000 判定=判据正常`。
⇒ **可请用户 L3 上手实测**（唯一终审）；实测期保持记录仪运行。

## 23. 收窄改写：只改 FG 自己的 open（04:06–04:18 次轮）

**问题（委托方实测）**：全部改写把 Unity 的「资源安装」读也改到 179 MB 原厂包 ⇒ Unity 报
`Not enough storage space to install required resources`（设备实有 108 G，纯误报），游戏卡住进不去。

**改法**：libc 侧 openat 的改写判据从「无条件」改成 **「调用栈含 libFairGuard 帧」**
（`LvStackHasFgFrame`）。第一版走 **fp 链**：实测 Unity/il2cpp 常省略帧指针 ⇒ 全 0、一次没改写，
cert 直接不过（04:07 冷启 6 s 就 `am_proc_died`）；改成**扫栈字**（sp 起 256 个字里出现 FG 段地址即命中）后正常。
prop `debug.starry.lvmeng_openatall`：**空/1（默认）= lr∈FG 或栈含 FG 帧**；0=仅 lr∈FG；2=无条件全部（旧行为，回退用）。

**实测（04:11 冷启，最终包 so `bb7349d2…`，prop 全空=交付默认）**：

| 项 | 值 |
|---|---|
| 存活 | 连查 3 次 ≈174 s 一直在 |
| 崩溃/踢人/误报 | `get signal` **0** · 被踢 **0** · `not enough storage` **0** |
| FG 尾段读（原厂块） | **1661** 条 ⇒ cert 仍读到原厂证书 |
| 现装块读 | 32 条（= Unity/游戏的正常读，**不再被改走**） |
| openat 改写 | 2 次（只 FG 那几次） |
| 进游戏 | `wm_create_activity MainFragmentActivity` 04:11:54.397 + Splash 随之 finish ⇒ **过了资源安装进了主界面** |
| 菜单 | 35 行 |

第二轮冷启（同包）：存活 160 s+、无崩溃、无踢人（该轮无证书解析事件，作稳活证据）。

## 24. 摘掉 pread/lseek/信号族 TRAP（04:36–04:50）

**改动**：mode 9 的 BPF 现在**只拦 openat(56)**——删掉 62/67/69/286 的 offset 过滤组与
129/130/131/138/240/424 信号族组；handler 只保留 openat 改写分支，尾段读/信号族的记录与刷屏日志一并停掉
（实测 `尾段读取命中` **0 条**）。过滤器 14 条指令：`openat 闸：TSYNC 成功 insns=14（只拦 openat）`。

**但之后出现新现象（重要，未归因完）**：本包冷启后进程在**所有配置下都会死**：

| 配置 | 结果 |
|---|---|
| 默认（只拦 openat + 栈判定改写） | 04:37 启动后 **0.08 s** 死亡，连续 4 次重启（32641/32766/410/484），无崩溃转储、无 signal、无 fatal |
| `tgkillguard=0`（seccomp 全关，fgspoof ON） | 04:41 活 40 s；04:44 冷启 55 s 前已死（不稳定） |
| `tgkillguard=1`（只 RET_LOG，不 TRAP） | 45 s 内死 |
| `tgkillguard=0` + `fgspoof=0`（我们完全不介入） | 60 s 内死，同样无崩溃标记 |

⇒ **「闪退」已不能归因于 pread TRAP**：摘掉之后连「我们完全不介入」都闪退。现场另两线索：
① 外部数据目录有 `tombstone_00/01/02`（10-20 06:14、10-21 04:02、10-20 05:55）⇒ 此前多次真崩溃；
② 有一次 `am start` 报顶层是 `com.android.permissioncontroller/.permission.ui.GrantPermissionsActivity`
（系统权限弹窗压前台，可能挡住启动流程）。
**建议**：按委托方预案先确认资源是否半装（很可能被早期「全量改写」那轮写成半装或崩溃中断），
再由**用户确认后清数据**重装（清数据属破坏性操作，未获确认前不做）。

**当前产物**：`08_output/绿梦/绿梦_1.0.5_certspoof3.apk` = `ac841d9d19e1bb7992d2a41fd2761ff4`
（371,433,182 B，so `e146e71ca1457531ebb46009cfbda6dc`，只拦 openat 版）；prop 已复位为空（= 交付默认）。

## 25. seccomp 已被 FairGuard 直接检测（05:00–05:09，本轮结论）

**隔离实验**（本版 so `5edd37ab…`，加 `openatall=8`=只原样重做不改写 + `openatlog=1` 逐条打 rc）：
`openat rc=` 正常返回（fd 133/135/136…，仅 3 条 `rc=-13` EACCES），**纯 TRAP 不改写同样在 1 s 内死**
⇒ **不是改写逻辑/栈扫描的问题**（栈扫描也已从 256 字收到 64 字）。

**回归不属于本轮代码**：把上一轮"已知能活 165 s"的 `绿梦_1.0.5_certspoof2.apk`（so `bb7349d2…`）重装，
默认配置冷启 **t=2 s 就死**。同一份 so 在 04:19 能活 165 s，现在活不过 2 s ⇒ 触发条件是**时间/设备状态**，
不是我这轮改动。与委托方观测的 147 ms 秒崩一致。

**对照组**：`tgkillguard=0`（不装过滤器，fgspoof 仍 ON）→ 冷启 **ALIVE 40 s**（pid 12064）。
⇒ **只要装 seccomp 过滤器就 1–2 s 死；不装则不死于这一步**。
机制推测：FairGuard 读 `/proc/self/status` 的 `Seccomp_filters`（我们自己的 `LvStatus` 就在打
`Seccomp_filters=1`）并当作 tamper 直接处决 ⇒ **seccomp 这条路对本游戏已不可用**。

**关掉 seccomp 的代价**（`绿梦_1.0.5_certspoof4.apk`，seccomp 默认关、fgspoof ON，05:07 冷启）：
能进 `MainFragmentActivity`、菜单 35 行、无 storage 误报，但 **56 s 前死、`get signal 4`=1**
⇒ libc 层 fgspoof **不足**以让 cert 比较通过（真实读的 fd 来自我们没拦到的那次 open）。

**下一步（需委托方拍板，不再动 seccomp）**：libc 层"黑名单式"伪装——`my_open/my_openat` 对
`base.apk` 一律换 `files/fg_orig.apk`，**但排除** ra 落在 libunity/libil2cpp/libart/libcore 的调用
（保护 Unity 资源安装）。既不装 seccomp（不被检测），又能让 FG 的签名读落到原厂包。
