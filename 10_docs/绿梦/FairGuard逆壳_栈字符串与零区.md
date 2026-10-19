# 绿梦：时空之声 — FairGuard 逆壳：栈字符串解码 + 零区 0x1F0000 来源

- 轮次：2026-10-20（子代理会话），承接 `FairGuard逆壳_阶段1_解包器.md` §7/§8。
- 目标：① 逐条摸清 0x42A440..0x42A640 的「mov 立即数 → strb 栈偏移」规律；② 解出完整字符串；③ 判定零区 `0x1F0000..0x400000` 约 2 MB 代码的来源文件。
- 输入：`/tmp/fg_text.asm`（32,069 行，llvm-objdump 反汇编）、`06_dumped/绿梦_20261016/libFairGuard_an.so`（分析副本）、`06_dumped/绿梦_20261016/fg_runtime/libFairGuard_rwxp.bin`（运行期 4 MB 镜像）。
- 工具：capstone 线性反汇编（分块解码，避免 `md.disasm` 遇非法字即停）+ 常量传播；**IG5(reverse) 本轮不可用**（`ig5_open` 返回 `IG5 read cancelled`，全程手工静态）。

## 0. 结论（TL;DR）

| # | 问题 | 结论 | 证据级别 |
|---|---|---|---|
| 1 | 0x42A440.. 是不是字符串构造 | **是**。函数入口 0x42A498 `sub sp,sp,#0x110`，逐字节 `mov wN,#imm` + `strb wN,[sp,#off]` 写两段 NUL 结尾串 | 实测 |
| 2 | 两段串是什么 | **`/libcocos.so`**（12 B，栈 0x10..0x1B）、**`/split_config.arm64_v8a.apk`**（27 B，栈 0x20..0x3A） | 实测（解码规则从代码读出，非爆破） |
| 3 | 为什么以前 XOR 试不出来 | 编码**按站点随机化**：①向量 XOR 只覆盖前 8/16 字节，后半截是明文；②另一条串用逐字节 `codec(K,C)`（含条件加/减，非线性）；③同一段区域里两条串用两套不同编码 | 实测 |
| 4 | 壳里有 "base.apk" 吗 | **没有**。壳内与装包相关的明文只有 `/split_config.arm64_v8a.apk` 和 `/libcocos.so`（另有 `/proc/self/maps`、`libdl.so`、`dl_iterate_phdr` 等）。原假设「栈上构造 base.apk 路径」**不成立** | 实测 |
| 5 | 零区代码来源 | **不是本 .so 磁盘镜像的任何一段**：磁盘此区全零、运行期含原库 `.eh_frame_hdr/.eh_frame/.gcc_except_table/relro` 且地址与 section 表精确吻合、2.03 MB 密文无任何压缩流可生成 4.19 MB 明文 ⇒ **必须由外部文件在运行期喂入**；最可能是 AAB 分包（`/split_config.arm64_v8a.apk`）承载的 blob。**真正的 open/read 调用点未定位 ⇒ 未闭环** | 结构实测 + 推理 |

## 1. 方法（可复算）

1. 反汇编来源：`/tmp/fg_text.asm`（已是分析副本的完整线性反汇编）。分析副本 `libFairGuard_an.so` 的 vaddr == 文件偏移（PT_LOAD#0 off 0 vaddr 0），所以 `capstone.disasm(so[addr:]) ` 可直接用地址。
2. 栈字符串提取：对 `mov/movz/movn` 建 w 寄存器常量表；`add xN,sp,#imm` 建栈基址表；`strb <const>,[sp,#off]` 落到 (窗口, 偏移) → 字节图。
3. 变换归属：只认代码里真实发生的变换——(a) `movi vN.8b,#K` + `ldr d/q` + `eor vD,vS,vN` + `str d/q`（向量 XOR，宽度 8/16）；(b) `ldrb/eor #K/add #C/uxtb/tbz #7/sub #D/strb`（逐字节 codec）。**不做整串爆破**——256 个单字节 key 对 ≥8 B 短串必然能凑出「全可打印」的假结果（本轮实测确认，见 §6 经验）。
4. 段级判定：ELF phdr/shdr 解析、`.eh_frame_hdr` 二分表解析、zlib/raw-deflate/lzma 探测、熵与零率。

## 2. 0x42A440..0x42A640 逐条表（立即数 → 栈偏移）

函数头：`0x42A498 sub sp,sp,#0x110`；入参 `x0`=路径, `w1`=flags（`0x42A56C mov w2,w1`、`0x42A578 mov x1,x0`）。
0x42A408..0x42A494 是**另一个**函数（`sub sp,sp,#0xe0`，变参保存 + `bl 0x42EB00` openat 封装），不含字符串。

### 2.1 串 A：`/libcocos.so`（栈 0x10..0x1B，0x1C 处 NUL）

| 栈偏移 | 存储值 | 立即数指令 | strb 指令 |
|---|---|---|---|
| 0x10 | 0x99 | `0x42A4B4 mov w2,#-0x67` | 0x42A4B8 |
| 0x11 | 0xDA | `0x42A4BC mov w2,#-0x26` | 0x42A4C0 |
| 0x12 | 0xDF | `0x42A4C4 mov w2,#-0x21` | 0x42A4C8 |
| 0x13 | 0xD4 | `0x42A4CC mov w2,#-0x2c` | 0x42A4D0 |
| 0x14 | 0xD5 | `0x42A4A0 mov w12,#-0x2b` | 0x42A524 |
| 0x15 | 0xD9 | `0x42A49C mov w8,#-0x27` | 0x42A52C |
| 0x16 | 0xD5 | w12 | 0x42A534 |
| 0x17 | 0xD9 | w8 | 0x42A53C |
| 0x18 | 0xC5 | `0x42A4A4 mov w11,#-0x3b` | 0x42A544 |
| 0x19 | 0x98 | `0x42A4D4 mov w2,#-0x68` | 0x42A4D8 |
| 0x1A | 0xC5 | w11 | 0x42A54C |
| 0x1B | 0xD9 | w8 | 0x42A554 |
| 0x1C | 0x00 | `wzr` | 0x42A55C |

raw = `99 da df d4 d5 d9 d5 d9 c5 98 c5 d9`（与 §7 记录一致）。**解码点 0x42A668..0x42A6AC**：

    movi v1.8b,#0xb6        ; 键
    add  x9,sp,#0x10
    ldr  d0,[x9]            ; 前 8 字节
    eor  v0.8b,v0.8b,v1.8b
    str  d0,[x9]            ; 0x10..0x17 ^= 0xB6
    之后 4 次 ldrb/eor #-0x4a(=0xB6)/strb 处理 0x18..0x1B

⇒ 12 B 全部 ^0xB6 = `/libcocos.so`（注意：`mov w12,#-0x2b` 是 **0xD5** 不是 0xC5，上一轮把它当 0xC5 才解出 "libsosos" 这种怪串）。

### 2.2 串 B：`/split_config.arm64_v8a.apk`（栈 0x20..0x3A，0x3B 处 NUL）

| 栈偏移 | 值 | 立即数来源 | strb |
|---|---|---|---|
| 0x20 | 0x0C | `0x42A4DC mov w2,#0xc` | 0x42A4E0 |
| 0x21 | 0x57 | `0x42A4E4 mov w2,#0x57` | 0x42A4E8 |
| 0x22 | 0x4A | `0x42A4A8 mov w9,#0x4a` | 0x42A560 |
| 0x23 | 0x4E | `0x42A4EC mov w2,#0x4e` | 0x42A510 |
| 0x24 | 0x41 | `0x42A4AC mov w10,#0x41` | 0x42A564 |
| 0x25 | 0x56 | `0x42A514 mov w2,#0x56` | 0x42A568 |
| 0x26 | 0x3C | `0x42A50C mov w6,#0x3c` | 0x42A5C8 |
| 0x27 | 0x47 | `0x42A570 mov w1,#0x47` | 0x42A574 |
| 0x28 | 0x4B | `0x42A57C mov w0,#0x4b` | 0x42A580 |
| 0x29 | 0x4C | `0x42A584 mov w0,#0x4c` | 0x42A588 |
| 0x2A | 0x44 | `0x42A58C mov w0,#0x44` | 0x42A590 |
| 0x2B | 0x41 | w10 | 0x42A5D4 |
| 0x2C | 0x43 | `0x42A594 mov w0,#0x43` | 0x42A598 |
| 0x2D | 0x0D | `0x42A504 mov w5,#0xd` | 0x42A5D8 |
| 0x2E | 0x3A | `0x42A4FC mov w4,#0x3a` | 0x42A5DC |
| 0x2F | 0x48 | `0x42A59C mov w0,#0x48` | 0x42A5A0 |
| 0x30 | 0x4D | `0x42A5A4 mov w0,#0x4d` | 0x42A5A8 |
| 0x31 | 0x15 | `0x42A5AC mov w0,#0x15` | 0x42A5B0 |
| 0x32 | 0x17 | `0x42A5B4 mov w0,#0x17` | 0x42A5B8 |
| 0x33 | 0x3C | w6 | 0x42A5E0 |
| 0x34 | 0x54 | `0x42A5BC mov w0,#0x54` | 0x42A5C0 |
| 0x35 | 0x13 | `0x42A5C4 mov w0,#0x13` | 0x42A5CC |
| 0x36 | 0x3A | w4 | 0x42A5E4 |
| 0x37 | 0x0D | w5 | 0x42A5E8 |
| 0x38 | 0x3A | w4 | 0x42A5EC |
| 0x39 | 0x4A | w9 | 0x42A5F0 |
| 0x3A | 0x4F | `0x42A5D0 mov w0,#0x4f` | 0x42A5F4 |
| 0x3B | 0x00 | wzr | 0x42A5F8 |

raw = `0c 57 4a 4e 41 56 3c 47 4b 4c 44 41 43 0d 3a 48 4d 15 17 3c 54 13 3a 0d 3a 4a 4f`
（= §7 记的「39 字节」里第 17 字节之后的 "WJNAV<GKLDAC…" 段；前 12 B 属于串 A，两条串不同编码、不同 NUL）。

**这条不是 XOR**，而是逐字节 codec（0x42A70C..0x42A74C，范围 `add x0,x9,#0x1b` ⇒ 27 B）：

    mov w7,#0x47
  L: ldrb w5,[x4]
     eor  w5,w5,w7          ; t = b ^ 0x47
     add  w6,w5,#0x63       ; uxtb
     tbz  w6,#7, STORE      ; bit7==0 → 存 (t+0x63)&0xff
     sub  w5,w5,#0x1c       ; 否则存 (t-0x1C)&0xff
  STORE: strb w5,[x4]

即：`t=b^0x47; p = (t+0x63)&0xff if <0x80 else (t-0x1C)&0xff` ⇒ `/split_config.arm64_v8a.apk`。

> 注意 `0x42A720` 存的是 **w6**（`t+C`），`0x42A748` 存的是 **w5**（`t-D`）；把两处都看成存 w5 就会解不出来（上一轮卡点之一）。

### 2.3 这个函数在干什么（语义，供下一轮用）

- 入口 0x42A498 → flags 走 `tbnz w2,#6`；两条路径都做「解出上面两条串 → 在入参字符串 x1 里做子串查找 → **命中就 `mov w0,#-1` 直接返回**」；未命中才落到 0x42A614 的 `openat(AT_FDCWD=-0x64, path, flags|0x20000)`。
- ⇒ 它是**带拦截名单的 open 封装**：拒绝对含 `/libcocos.so` 或 `/split_config.arm64_v8a.apk` 的路径开 fd（把这两个路径列为"被保护对象"，不让第三方读）。
- 该函数**没有直接 `bl` 调用者**（全壳 122 条 `br/blr`、0 条 `bl 0x42A498`；`.data` 段里也没有指向壳代码的函数指针，只有 3 个 `.init_array/.fini_array/GOT` 指针表项）⇒ 要么走算出来的 `br`，要么是同一 OBF 变体的死块。**未闭环**。

## 3. 三种栈字符串编码法（本轮全部实测到）

| 法 | 形态 | 覆盖范围 | 已解出 |
|---|---|---|---|
| V（向量 XOR） | `movi vN.8b,#K` + `ldr d/q` + `eor vD,vS,vN` + `str d/q` | 只覆盖从该栈偏移起的 8 或 16 B；**串的其余部分是明文** | `/libcocos.so`(K=B6)、`/proc/self/maps`(K=06，只前 8 B) |
| C（逐字节 codec） | `ldrb / eor #K / add #C / uxtb / tbz #7 / sub #D / strb`，恒有 **C+D=0x7F** | 整串（长度由 `add xE,xBase,#len` 给出） | `/split_config.arm64_v8a.apk`(K=47,C=63)、`/proc/self/maps`(K=78,C=43) |
| P（明文直存） | 纯 `mov #imm` + `strb` | 整串 | `dlopen`、`dlsym`、`dlclose`、`dlerror`、`fread` 等 |

codec 的闭式（`D=0x7F-C`，因此可逆、p 值域 0x01..0x7F）：`t=b^K; p=(t+C)&0xff 若 ((t+C)&0xff)<0x80，否则 p=(t-D)&0xff`。
**站点随机化实证**：全壳 22 处向量 XOR，key 各不相同（0x06/0x1E(×2)/0x31/0x34/0x39/0x4A/0x4B/0x4F/0x57/0x68/0x70/0x94/0xAE/0xB6/0xE3/0xF5/0xF7/0xFC）；逐字节 codec 站点扫到 4 组 (K,C)：(0x78,0x43)×15、(0xC1,0x39)×3、(0x03,0x39)、(0x1C,0x06)。同一个字符串 `/proc/self/maps` 在 0x427430 用 V 法、在 0x42AC50 用 C 法 ⇒ **任何"单 key XOR 爆破 / 明文搜字符串"都必然失败**，必须按站点读代码。

## 4. 已解出的字符串清单

实测（解码规则从代码读出）：

| 字符串 | 出现点（构造 → 解码） | 编码 |
|---|---|---|
| `/libcocos.so` | 0x42A4B8.. → 0x42A668 | V, K=0xB6（8 B 向量 + 4 B 单字节） |
| `/split_config.arm64_v8a.apk` | 0x42A4E0.. → 0x42A70C | C, K=0x47,C=0x63 |
| `/proc/self/maps` | 0x427468.. → 0x427538/0x427558 | V, K=0x06（仅前 8 B） |
| `/proc/self/maps` | 0x42AC84.. → 0x42AD30.. | C, K=0x78,C=0x43 |
| `libdl.so`、`dl_iterate_phdr` | 0x414Bxx / 0x414Exx | V, K=0x39 / 0x1E |
| `dlopen/dlsym/dlclose/dlerror`、`fread` | 0x4238xx / 0x41F000 | P（明文） |

未完成（本轮定位到站点但未逐串解）：0x41C8xx、0x4190xx、0x4194xx、0x4198xx 一带的十余段（多为 lib 名候选，如 0x421400 区已见 `/liblog.so`、`/libmediandk.so` 明文），以及 0x4155xx 的 64 B 段。**结论不受影响**：装包/文件相关的路径只有上表前 4 条。

## 5. 零区 0x1F0000..0x400000 的来源

### 5.1 实测事实

1. **磁盘全零，运行期是密集内容**：disk 熵 0.13（2,162,688 B 全 0），runtime 熵 6.59、零字节仅 17%（≈代码常见比例）。
2. **运行期这一区含原库的元数据，且地址与 section 表精确吻合**（磁盘同址全零）：

   | section 表声明 | 运行期实测首字节 |
   |---|---|
   | `.eh_frame_hdr` @0x3BD1F8 size 0x3A24 | `01 1b 03 3b 24 3a 00 00 43 07 00 00` = version 1, enc 1b/03/3b, **fde_count=0x743=1859** |
   | `.eh_frame` @0x3C0C20 size 0x15B00 | `14 00 00 00 00 00 00 00 01 7a 52 00 04 78 1e 01` = 标准 CIE（zR 增强） |
   | `.gcc_except_table` @0x3D6720、GNU_RELRO @0x3FAA08 | 有内容（relro 区已由阶段1 §10.4 记为"每 8 B 一个运行期指针"） |

3. **`.eh_frame_hdr` 的 1859 条 FDE 覆盖 0x1B100..0x3B3ED8**（按 1 MB 分桶：0x0 桶 203、0x1 桶 323、0x2 桶 132、**0x3 桶 1201**）⇒ 原库的**函数代码横跨 A 区（0x400..0x1F0000）与 B 区（0x1F0000..0x400000）**，不是只有 B 有代码。
4. **B 区不是 A 区的拷贝**：从 B 取 6 个 128 B 探针回搜整镜像，只命中自身；A→B 各 64 KB 位移的同字节率最高 13.7%（≈随机）。
5. **A 区没有可用的压缩流**：对运行期 payload（已解密的 2,030,592 B）以 4 KB 网格试 `zlib`/`raw-deflate`（wbits 15/-15）与 `lzma FORMAT_ALONE`，**没有任何候选解出 ≥512 KB**。
6. **信息量/尺寸上不可能内生成**：磁盘承载的载荷只有 **2,030,592 B**，而运行期 A+B 的明文是 **4,193,280 B**（0x400..0x400000）。无压缩流 ⇒ 2.03 MB 无法生成 4.19 MB。**B 区（2,162,688 B）在磁盘上没有任何副本、也没有可解压的来源。**
7. 载荷头 0x400..0x437 是**明文描述块**（磁盘=运行期）：`0x00400b78 / 0x001f40d6 / 0x00403000 / 0x00400fd0 / 0x003ff3f0` 全是 0x3FF000..0x403000 附近的地址，0x434 在运行期被写成 `0xc25ca588` ⇒ 加载器对这些槽做了运行期回填。
8. 壳**能**用 libc 打开文件：`.dynsym` 29 个未定义符号里 25 个被改名成 `tan`（假名），真名只漏 `environ/sigaction/sigprocmask`；壳内自带 musl 风格 `fopen(0x426AF0)/fread(0x4269A0)/fclose(0x426AE0)` 与 FILE vtable（0x427430 里把 0x4269EC/0x426AE8 写进 struct+0x40/+0x38）。

### 5.2 判读

- **零区代码不是从本 .so 的文件镜像读来的**（事实 1/3/5/6）。它必须是运行期从**别的文件**取来后写进自身 rwx 映射（maps 快照：`base..base+0x413000 rwxp`）的。
- 阶段2 的「openat 桩（0x42EB00）hook 零命中 ⇒ 没读 APK」**不能作为否证**：壳自带 musl stdio 且 25 个 libc 导入被改名，读文件完全可能走 libc 的 open/fopen（不经过它自己的 `svc` 桩）。上一轮的 hook 点选错了。
- 候选来源（按证据强弱）：
  1. **AAB 分包 `/split_config.arm64_v8a.apk`** —— 壳内唯一与"装包"相关的明文路径，且它出现在 0x42A498 的**拦截名单**里（壳不许别人打开它，说明它是被保护/自用对象）。AAB 安装后全部 native 库都在这个分包里。
  2. `/libcocos.so` —— 同上被拦截；若游戏确实有 cocos 层，这里可能是"原库"或其载体。
  3. base.apk 本体（壳里**没有** base.apk 路径字面量，故只作为兜底）。
- 与阶段1 §10.4 的关系：那段说"零区/指针区必须模拟加载器的重定位与布局"。本轮补充一条更硬的分界——**重定位只能在已有内容上打补丁**，而 `.eh_frame`/`.gcc_except_table` 这类**编译器产物无法重定位生成**，磁盘上又不存在 ⇒ 内容本身是外部喂入的，重定位只是最后一步。

## 6. 未闭环 / 下一轮建议

1. **真正定位 open/read 点**（唯一硬缺口）：
   - 静态：`fopen/fread` 无直接调用者（经 FILE vtable 间接）；应在壳区找 `blr` 目标落在 0x426AF0/0x4269A0 的站点，或找 `add xN,xM,#imm` 把 0x42xxxx 写进 struct 的站点；
   - 运行期（更省）：frida spawn 后 hook **libc 的 openat/open/fopen/read**（不是壳的 svc 桩），冷启一次即可看到 fd 与路径；同时 hook 0x43054C(匿名 RWX 分配)/0x430508(mprotect) 看写入目标。
2. **零区写入者**：壳区 30,036 条指令里没有 0x1F0000/0x400000 字面量（本轮复核：`0x1f0000` 0 命中），上界来自运行期寄存器 ⇒ 与阶段1 §11.3 的"两段式自解密"假设一致；但 B 区含编译器元数据，故"两段式"应理解为"外部 blob 解密后补齐"。
3. **A/B 区不像普通编译代码**：指令画像 shell(ret 4.16%/udf 0.17%) vs A(ret 0.11%/udf 4.11%) vs B(ret 0.25%/udf 4.79%) ⇒ A/B 更像 VM/字节码化产物或仍是混合数据。**待验证**，不影响本轮字符串结论，但影响"零区能不能直接当代码读"。
4. 0x42A498 无直接调用者（可能经 `br` 分发）——若要确认"拦截名单"真在生效，需运行期对上电断点。
5. 上一轮 §7 的 "39 B 单串" 是**两条串被误并**（0x10..0x1B 与 0x20..0x3A，中间 0x1D..0x1F 未写）；本条已修正。

## 7. 经验（可复用）

- 这类壳的栈字符串**不要爆破**：短串 + 256 key 必然凑出"全可打印"假解（本轮用打分法复现：所有 5–30 B 段都能找到 ≥0.75 分的"解"）。必须从代码里读变换：抓 `strb <const>,[sp,#off]` 建字节图，再找**写回同一偏移**的 `eor/sub/add` 块，把变换按偏移绑定。
- `mov w12,#-0x2b` 这类立即数**按 8 位取低字节**（0xD5），不要按 6 位口算——上一轮把 `/libcocos.so` 算成 "libsosos.so" 就是这一步错的。
- `md.disasm` 遇非法字会**停止迭代**（不是跳过），扫壳区必须按 4 KB 分块、每次前进 4 B 重试，否则只能扫到前 16 KB（本轮第二次踩到：22 处向量 XOR 里有 21 处曾因这个 bug 丢失）。

## 8. 运行期补测（2026-10-20 02:19–02:23，非 frida + 一轮 frida）

### 8.1 三条**修正/否证**（覆盖 §5 与本文件早先表述）

| # | 新事实 | 影响 |
|---|---|---|
| 1 | **本机没有 split APK**：`pm path com.zhuziplay.lvmengnew.op` 只返回 `base.apk`（371,384,030 B）。设备上不存在 `/split_config.arm64_v8a.apk` | §5.2 的"最可能来源 = AAB 分包"在本机**没有文件基础**。该串是 FairGuard 的**通用**拦截名单项（为 AAB 装机形态准备），不是本机实际路径 |
| 2 | **磁盘 `0x1F0000..0x400000` 不是"全零"**：`0x1F0000` 起有 **17,664 B 高熵块（47 段**，首段 161 B，第二段 1,136 B…），其余 **2,145,024 B 为 0**；首字节 `3a 89 ca a7 73 e3 c5 cf…` | 逐字节确认：`dd if=<装机 libFairGuard.so> bs=4096 skip=496` 与本地副本 md5 `7bee7f8b44d9106165707f26c33ffca6` 一致 ⇒ 装机文件与本地副本同源；该 17 KB 块是**唯一**为 B 区准备的磁盘数据 |
| 3 | **B 区是"写"进去的，不是文件映射**：`smaps` 里 `0x734081a000` 起 rwxp 段 Size 4,172 kB / **Rss 4,172 kB / Pss_Dirty 4,132 kB**；全进程 `/proc/PID/io` `read_bytes=0` | 整段是**私有脏页**（运行期写入）。maps 里也没有任何 2 MB 级第三方文件映射可借（base.apk 只有 3 个 `r--s`：resources.arsc 2.25 MB + CD 区 84 KB；libs 都是常规 `.so` 段） |

**B 区运行期确定性（复核通过）**：冷启 pid 31173（base `0x734081a000`）在 `base+0x1F0000` 抓 64 KB，前 32 B = `21 00 40 b9 41 fb 0f 36 01 01 66 9e eb 00 09 4a 68 01 08 0b 08 55 88 13 24 a0 14 d1 23 b0 14 d1`，与已存 4 MB 镜像（`libFairGuard_rwxp.bin` @0x1F0000）逐字节一致。

### 8.2 base.apk 里与本问题相关的条目（解析中央目录，1341 项）

| 条目 | 存储方式 | 大小 | 判读 |
|---|---|---|---|
| `lib/arm64-v8a/libFairGuard.so` | deflate | cs 2,136,076 → us **4,477,792** | 与装机文件同尺寸；deflate 比仅 2.1x ⇒ 与"半份文件是零"吻合，APK 内**没有**第二份未打洞的副本 |
| `assets/lvmeng/fg_orig.apk` | **stored** | **179,649,718** | = 硬约束 #3 的原版官方包尺寸（星光的 mod 载荷），**不是** B 区来源（B 区在未 mod 的 fg_test 包里也存在） |
| `assets/lvmeng/__data` | deflate | 10,549,059 → 13,189,417 | mod 侧载荷，同上排除 |
| `lib/arm64-v8a/libstarry_magic.so` | deflate | 2,134,488 | mod 菜单 so；本次崩溃栈（`pc 0xec20`，500+ 帧递归）在它里面 |

### 8.3 frida 一轮（未取得壳侧数据）

- 第 1 次 spawn：脚本装入（10 个 libc 钩子 + 壳 4 个地址待装），但 **frida CLI 因后台 stdin EOF 立刻退出**，app 存活 ≥38 s（该轮白跑）。
- 第 2 次 spawn（改为 `sleep 50 | frida` 保持 stdin）：app 在 **Java bootstrap 期（约 1–2 s）死亡**，`libFairGuard.so` 还没加载（脚本报 `noshell`），只抓到 ART 自身行为：`/proc/self/cmdline`、`base.vdex`/`base.art`/`base.apk`（`libartbase.so+0x32864`）、`BambooSDK.xml`，以及 `libbase.so+0x1186c` 的 `pread64(fd=102, 65557)`。
- ⇒ **"壳读了哪些文件 / read 是否走 libc" 本轮仍是空白**：上一轮"0x42EB00 零命中"依旧不能被当作"没读文件"，但也不能被本轮证实。
- 设备上 `/sys/kernel/tracing` 存在但 `available_tracers` 为空、无 `events/syscalls` ⇒ **ftrace 不可用**，非 frida 的 syscall 观测这条路本机走不通。

### 8.4 修正后的零区模型（待验证）

1. B 区 = 运行期写入（8.1-3 硬证据），写入者不是文件映射。
2. 磁盘只给了 **17,664 B** 的 B 区数据（8.1-2）；剩余 **2,145,024 B** 必须由别的东西产生：要么"读某文件后解密/解压"，要么"由载荷自己解压生成"。
3. 与 §5.1-6 的尺寸推理一致，但"外部文件"这一步**没有落地证据**：本机没有 split APK，base.apk 里也没有 2 MB 级可疑条目。
4. 因此 **"上一轮 fg_test 包里 B 区也有内容"** 这一条成为关键判据：若当时也是同样内容，则与 mod 无关，只能来自 FairGuard 自身逻辑（读自身/解压）；这一点需要拿当时的包复核。


### 8.5 追加更正（02:26，据决策官口径）

- 4 MB 镜像 `8297e6b9…` 出自 **fg_test 干净重签包**（`FairGuard运行期取证.md` §1 与「被观测包」行：未改 so/APK 字节、未重打包），该镜像已含 B 区内容 ⇒ **B 区 = FairGuard 自身运行期产物，与 mod / 星光载荷无关**；A/B 侧重核不再做。
- **§8.3 的归因要收窄**：本次**非 frida** 冷启（pid 31173，02:22 仍在，约 02:2x 自己消失）同样会死 ⇒ 「frida 下 1–2 s 死」不能单独归给 FairGuard 反调试，与 mod 侧 `libstarry_magic.so`（pc 0xec20，500+ 帧递归）纠缠。FairGuard 侧结论不受影响（取自干净包 + B 区 head 逐字节一致）。
- 取证口径：一律用 fg_test 干净包，避开 mod 崩溃干扰。
