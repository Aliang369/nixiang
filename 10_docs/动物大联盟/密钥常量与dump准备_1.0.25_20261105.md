# 密钥常量与 dump 准备 — 动物大联盟 1.0.25（2026-11-05）

对象 `/tmp/dwdlm_sec/lib/arm64-v8a/libsecret.so`（459344 B，取自 `02_apks/动物大联盟/动物大联盟_1.0.25_official.apk`）。工具 NDK 27 `llvm-readelf`/`llvm-objdump`。标注：**实测/推断/待验证**。

## 1 段与节（实测）

| PT_LOAD | offset | vaddr | filesz | memsz | flg | vaddr−offset |
|---|---|---|---|---|---|---|
| #0 | 0x0 | 0x0 | 0x62d90 | 0x62d90 | RE | 0 |
| #1 | 0x62d90 | 0x66d90 | 0x3220 | 0x3220 | RW | 0x4000 |
| #2 | 0x65fb0 | 0x6dfb0 | 0x9b78 | 0xa9f0 | RW | 0x8000 |

| 节 | vaddr | size | offset |
|---|---|---|---|
| .rodata | 0xe0b0 | 0xa378 | 0xe0b0 |
| .text | 0x24d40 | 0x3d85c | 0x24d40 |
| .data | 0x6dfb0 | 0x9b78 | 0x65fb0 |
| .bss | 0x77b30 | 0xe70 | 无（NOBITS，节头 0x6fb28 仅名义） |

换算：段#0/#1 `foff=vaddr`；段#2 `foff=vaddr−0x8000`；.bss 无偏移。

## 2 地址→偏移对照表

| 目标 | vaddr | 长 | 节 | 文件偏移 | 属性 |
|---|---|---|---|---|---|
| 1B 开关 | 0x6e0cd | 1 | .data | **0x660cd** | 可改 |
| const32 | 0x6e0de | 32 | .data | **0x660de** | 可改 |
| const16 | 0x6e0fe | 16 | .data | **0x660fe** | 可改 |
| sig hashCode | 0x6e0a8 | 4 | .data | **0x660a8** | 可改 |
| 校验起点 | 0x17900 | 8 | .rodata | **0x17900** | 可改（=0x24d40） |
| 校验长度 | 0x17908 | 8 | .rodata | **0x17908** | 可改（=0x3d85c） |
| 期望摘要 | 0x17910 | 32 | .rodata | **0x17910** | 可改；0x17930 紧接 `" (deleted)"` |
| flag | 0x786b0 | 4 | .bss | **无** | 仅运行期 |
| IV | 0x787a0 | 16 | .bss | **无** | 仅运行期 |
| KEY | 0x788a0 | 32 | .bss | **无** | 仅运行期 |

实测字节：hashCode `a2fd62fd`(LE 4251123106)、开关 `01`、const32 `96d17f23…92fe1f03`、const16 `87aede97…12cfbd81`、摘要 `70b63c1e…7dec2020`。

## 3 常量独占性（实测）

.data 侧 0x6e010 起是一条 `SCFGBLOB01` 记录（全文 magic 仅 1 处）。相对 0x6e010：+0x98 hashCode、+0x9c 长度字节(0x20)、+0x9d 32B 缓冲、+0xbd 开关、**+0xbe 64B 掩码块**（+0xce=const32、+0xee=const16）、+0xfe u16=6、+0x100 `assets/apksecurefile`。步长 0x120 由 `0x44620: madd x9,x25,#0x120,x10` 实测确认。

引用证据（`adrp 0x6e000` 共 27 处）：

- `0x56374: ldrb w8,[x8,#0xcd]` — 开关按 **1 字节**读，`cbz` 决定是否调用 0x55a78（签名解析）。
- `0x56338: add x23,x23,#0xce` → `0x56350: ldp q1,q0,[x23,#0x10]` 读 0x6e0de 的 32B、`0x56354: ldr q2,[x23,#0x30]` 读 0x6e0fe 的 16B：二者属于「0x6e0ce 起 64B 掩码块」，其前 16B（0x6e0ce–0x6e0dd）另有用途。
- `0x5b3f8: add x1,x1,#0xac` + `ldrb w8,[x1],#1`(=0x20) + 32B memcpy → 长度前缀缓冲止于 **0x6e0cc**，不压 0x6e0cd（边界干净）。
- 其余落点（`0x42f90`+0xac、`0x44608`+0xfe→u16@0x6e10e、`0x5190c`→包名@0x6e028）不触及三目标。
- **写者**：27 处窗口内无 `str/strb` 落 0x6e0a8–0x6e10e（实测）；.data 可写，远距离写**待验证**。
- **副本**：const32/const16/掩码头16B/摘要/hash4 在 459344 B 内各仅 1 次（脚本扫描）。
- **指针**：1306 条 RELATIVE 中涉及 0x6e 页只有 `0x6e000←0x78660`，无指针指向常量区。

结论：三者均在 .data、**可改**、无副本、无数据指针复用；但须按精确边界改（0x6e0cd 仅 1B，0x6e0ce 那 16B 属另一字段）。

## 4 .bss 三元组（实测引用点）

`0x56310: ldr w8,[x9,#0x6b0]`(flag) → `0x5631c: add x8,x8,#0x7a0` → `0x5632c: ldr q0,[x8]`(IV)；`0x56320: ldp q1,q0,[x8,#0x100]`(KEY)。另 `0x56380` 读 0x786b4（flag 后第二个 u32，作 0x55a78 第 4 参）。

maps 实测样本 `04_scripts/动物大联盟/official_runtime_maps.txt`（pid 27292）：base=0x6fc1c80000；rw-p(offset 0x65000) 落 base+0x6d000，文件背书段止于 **base+0x78000**；三地址均 >0x78000，属 .bss 匿名补零区（推断：bionic 保留区 mprotect，仍可读）。该样本含 **2 个 offset=0 实体**（双加载，实测）。

## 5 脚本

`04_scripts/动物大联盟/dump_secret_key_dwdlm.sh`（245 行）

- 用法：`bash <脚本> [包名] [pid]` / `-p` / `--pid` / `--self-test`。
- 取基址：awk 只认「带 `/libsecret.so` 绝对路径 + offset=`00000000`」的行，按 maps 顺序取首条；`[anon:…bss]` 排除；实体 >1 逐个读并告警。
- 读法：`su -c 'dd if=/proc/$PID/mem bs=1 skip=$abs count=N 2>/dev/null | xxd -p'`，空则退回 `od -An -tx1`。
- 自检：flag 按 u32 **小端**解析，`==1` 才判 KEY 可信；IV/KEY 全 0 或全 FF 告警并置 rc=3；每实体打印 YAML 块。
- 失败分档：无 adb／serial／非 device／pidof 空／pid 已退／su≠0／maps 空／无映射／mem 空（附 dd stderr）；只读幂等、不启动应用，退出码 0/1/2/3。

## 6 预演结果

- `bash -n` 通过；**`/bin/bash 3.2.57` 自测 11 项全绿**（基址、anon 行排除、地址加法、字节序、全 0 与全 FF），连跑两次 diff 一致；`preflight.sh` 通过（余 2 条 R2 WARN 为宿主机 `$((0x…))` 算术，设备侧不执行）。
- 设备只读实测：serial `f9a116a2` 在线、`pm path` 为本机测试包、`pidof` 空 → **进程未运行**，真实 mem 读取**待运行期实测**。
- 读取链另测：`su -c 'id -u'`=0、`dd/xxd/od` 均在 `/system/bin`、管道链取 `/proc/self/cmdline` 4B 得 `64640069`（su+dd+xxd 与引号层全通）。

## 7 风险

1. **SELinux/ptrace**：Android 12+ 即使 root，跨进程读 mem 仍可能被 `ptrace_may_access` 拒（现象：dd 空 + Permission denied）；脚本已打印 dd stderr。
2. **壳可能清零/复用 .bss**：flag≠1 前 KEY 不可信；建议 app-ready 后 1–2 s、登录与资源校验之后再读。
3. **双实体**（实测）：逐实体读，取 flag==1 且 KEY 非全 0 者。
4. 基址每次现取；进程重启或 so 重载后旧基址失效。
5. 改 const32/16 必然改变派生 KEY（64B 掩码块整体参与），若 Java/服务端有固定期望值需同步对齐——**待验证**。
6. 未发现写常量区的指令，但 .data 可写，运行期自改写**待验证**。
