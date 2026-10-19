# 绿梦：时空之声 — libFairGuard.so 运行期取证（Stage 2，只读）

- 目标：把「冷启后约 30 s 干净退出 `exit(0)`」的来源钉死，给 patch 决策提供依据。
- 范围：设备 `f9a116a2` 上**已安装的 fg_test 重签包**（`com.zhuziplay.lvmengnew.op`，1.0.5/vc12035）。**未改 so/APK 字节、未重打包、未装新包**；只做冷启 + frida 观测 + logcat + /proc。
- 上游：`10_docs/绿梦/FairGuard签名校验定位.md`（Stage 0：加壳、静态到头、候选 C1/C2/C3）。

## 1. 结论摘要（先看这段）

1. **基线复现**：无 frida 冷启 → `Start proc 00:36:15.169` → `00:36:45.530 Process 19964 exited cleanly (0)` = **30.36 s**，无 tombstone、无 crash 记录（与既有事实一致）。多次重跑落在 19～36 s 区间（见 §3）。
2. **载荷是"运行期原地解包"**：`libFairGuard.so` 首段在运行期映射为 **rwxp**（`base..base+0x413000`），内存内容与磁盘**逐块不同**（0x400..0x3F0000 全段差异），0x1F0000..0x400000 磁盘为全零、内存有内容 ⇒ Stage 0 的"加密载荷"在加载后被解密/重排到同一映射。已 dump 4 MB 运行期镜像（§5）。
3. **Stage 0 的 C1 被否证**：`libFairGuard.so+0x42F100`（exit_group 桩）hook 共 **5 次运行**，**零命中**；`+0x35BE00`（解包镜像里的另一套 exit_group 桩）同样**零命中**；`libc exit/_exit/abort`、`AndroidRuntime::exit` **全部零命中**——而进程每次都是 `exited cleanly (0)`。⇒ **退出不经过这些用户态出口**。
4. **退出前 0.5 s 有环境探测**（app 自身 pid）：`Access denied finding property "ro.secure"` → `"ro.debuggable"`（各 2 次）→ `Setting adb_enabled has moved…` → 下一行即 `Death received, pid=…` + `exited cleanly (0)`。
5. **frida 在机时反调试活跃**：死亡提前到 attach 后 4～19 s（正常 30 s），且一次直接 `signal 4 (Illegal instruction)`。⇒ 当前条件下**无法把"30 s 签名闸门"与"frida 反调试退出"干净分离**。

## 2. 环境

| 项 | 值 |
|---|---|
| 设备 | `f9a116a2`（OPD2404，arm64-v8a，uid=0 root `u:r:ksu:s0`） |
| frida-server | `/data/local/tmp/fsrv`（53,514,296 B）**版本 17.16.4**，与 host 17.16.4 匹配；启动 `setsid fsrv -l 127.0.0.1:27042`，`frida-ps -U` 正常 |
| host | `/opt/homebrew/opt/python@3.11/bin/python3.11` + frida 17.16.4（python binding 可用）；frida CLI 同版本 |
| **Java bridge** | **不可用**：脚本内 `Java` 未定义（`ReferenceError: 'Java' is not defined`）⇒ 无法 hook `System.exit`（下一步需换带 java bridge 的官方 server 或 gadget） |
| 被观测包 | `com.zhuziplay.lvmengnew.op` 1.0.5 / vc12035，firstInstallTime 2026-10-19 13:24:30（fg_test 重签包） |

## 3. 运行时间线（全部实测）

| # | 运行 / 证据文件 | frida 介入 | 启动 | 退出 | 存活 | 结束形态 |
|---|---|---|---|---|---|---|
| 1 | `logcat_baseline_1020_003613` | 无 | 00:36:15.169 | 00:36:45.530 | **30.36 s** | clean 0 |
| 2 | `frida_spawn_1020_003510` | spawn + attach(0 s) | ~00:35:11 | 00:35:24 | ~13 s | clean 0（hook 已装，零命中） |
| 3 | `frida_attach20_1020_003732` | attach@20 s | ~00:37:32 | 00:37:59.303 | ~27 s | clean 0（驱动误判"进程消失"提前 detach） |
| 4 | `frida_attach18_1020_003840` | attach@18 s | 00:38:41.955 | 00:39:07.316 | 25.36 s | clean 0（0x42F100 hook 在位，零命中） |
| 5 | `frida_scan_1020_004054` | attach@15 s | ~00:40:45 | 00:41:21.204 | ~36 s | clean 0，**4 MB 镜像 dump 完成** |
| 6 | `frida_scan2_1020_004343` | attach@10 s | 00:43:44.895 | 00:44:04.217 | 19.32 s | **signal 4 (Illegal instruction)** |
| 7 | `frida_java_1020_004421` | attach@12 s | 00:44:22.927 | 00:44:54.377 | 31.45 s | clean 0（Java bridge 不可用） |
| 8 | `frida_v5_1020_004558` | attach@12 s | 00:45:59.663 | 00:46:25.053 | 25.39 s | clean 0（AndroidRuntime::exit 零命中） |
| 9 | `frida_v6_1020_004641` | attach@8 s | ~00:46:47 | 00:46:55.429 | ~8 s | clean 0（字节探针证明 hook 在位） |

> 结论性读法：#1 是**无干扰基线**；#2–#9 都在 frida 在机状态，存活期普遍**短于**基线（19～31 s，最短 8 s），#6 直接 SIGILL ⇒ 反调试对注入有反应。

## 4. 内存镜像结构（运行期实测）

`/proc/<pid>/maps`（attach 后 1 s 快照，节选，同一次运行）：

```
734e943000-734ed56000 rwxp 00000000 ... /lib/arm64/libFairGuard.so     ← 0x413000，载荷所在，RWX
734ed56000-734ed7f000 r-xp 00413000 ... /lib/arm64/libFairGuard.so     ← 壳代码段
734ed85000-734ed88000 rw-p 00442000 ... /lib/arm64/libFairGuard.so
7340226000-7347ba5000 rwxp 00000000 ... /lib/arm64/libil2cpp.so        ← Unity 侧同样被重映射成 RWX
734fca6000-7351306000 rwxp 00000000 ... /lib/arm64/libunity.so
756f5eb000-756f5ee000 rwxp 00000000 00:00 0                            ← 匿名 RWX（3 页）
```

- **内容对比**：`MEM@0x100000` 磁盘 `946e3e9b8c392110…` vs 内存 `6476003d657a003d…`；0x1F0000 磁盘全零 vs 内存 `210040b941fb0f36…` ⇒ **原地解包**。
- **解包镜像里的系统调用桩表**（对 dump 离线解码，`image+0x35B000` 段，步长 0x100）：

| EA | x8 | 调用 | EA | x8 | 调用 |
|---|---|---|---|---|---|
| 0x35B100 | 0x38 | openat | 0x35B900 | 0xE2 | mprotect |
| 0x35B200 | 0x3F | read | 0x35BD00 | 0xA9 | 169（reboot，存疑） |
| 0x35B300 | 0x40 | write | **0x35BE00** | **0x5E** | **exit_group** |
| 0x35B400 | 0x39 | close | 0x35BF00 | 0xA7 | **prctl** |
| 0x35B500 | 0x75 | **ptrace** | 0x35B600 | 0x1D | ioctl |
| 0x35B700 | 0xC6 | socket | 0x35B800 | 0xCB | connect |

  ⇒ 载荷自带 **ptrace(117) / prctl(167)** 桩 = 反调试原语；另有 socket/connect。
- 另一次运行（00:43:44）在模块内 `Memory.scan` 命中 **`libFairGuard.so+0x42F100`（mov x8,#0x5e; svc #0）** 与 `+0x42EB00`（openat），即磁盘布局那一套桩表在运行期同样存在 ⇒ **同一镜像里有两套桩表**（0x35B000 段 + 0x42E000 段）。

## 5. Hook 命中矩阵（本阶段核心证据）

| hook 目标 | 尝试次数 | 命中 | 备注 |
|---|---|---|---|
| `libFairGuard.so+0x42F100` exit_group 桩 | 5（#2,#4,#6? ,#8,#9） | **0** | 字节探针证明 trampoline 已在位（见下） |
| `libFairGuard.so+0x35BE00` exit_group 桩 | 1（#8 之前的 0x35B 试验运行） | **0** | `MEM@0x35be00=c80b80d2010000d4…` 确认是 exit_group 桩 |
| `libFairGuard.so+0x42EB00` openat 桩 | 4 | **0** | 连"读 APK"的动作都没走这套桩 |
| `libc exit / _exit / abort` | 6 | **0** | |
| `libandroid_runtime AndroidRuntime::exit` | 1 | **0** | |
| `libc syscall`（全量） | 1（#5） | 5,082 次（全是 98=futex、57=close、198=socket、283） | **没有一次 94=exit_group、56=openat** |
| `Java System.exit/Runtime.exit` | — | 无法装 | `Java` 未定义（server 无 java bridge） |

**hook 确实生效的证据**（#9）：

```
BYTES[t2] fg+0x42f100 = 10e296f0 00021fd6 …   ← 原字节应为 c80b80d2 010000d4（mov x8,#0x5e; svc #0）
            libc_exit  = 50000058 00021fd6 …   ← ldr x16,#8 ; br x16（frida trampoline）
```

⇒ hook 在位、且到 2 s 时未被回滚；但退出发生时这些钩子**一次都没被经过**。

## 6. 退出前现场（logcat，app 自身 pid，实测原文）

```
00:44:53.352 W/libc (25869): Access denied finding property "ro.secure"
00:44:53.735 W/libc (25869): Access denied finding property "ro.debuggable"
00:44:53.736 W/libc (25869): Access denied finding property "ro.secure"
00:44:53.763 W/libc (25869): Access denied finding property "ro.debuggable"
00:44:53.764 W/libc (25869): Access denied finding property "ro.secure"
00:44:53.769 W/Settings(25869): Setting adb_enabled has moved from android.provider.Settings.Secure to …Global.
00:44:54.376 I/ActivityManager: Death received, pid = 25869 … for thread android.os.BinderProxy@…
00:44:54.377 I/Zygote: Process 25869 exited cleanly (0)
```

⇒ 退出前 <0.1 s 仍在做**环境探测（ro.secure / ro.debuggable / adb_enabled）**，随后立即干净退出。这与"签名闸门"不冲突：官方签名包在同一台 root 设备上能活 160 s+，说明环境项本身不是判死条件，**签名仍是变量**；但它说明闸门**同时带环境分支**。

## 7. 为什么 hook 全部落空（三种假设，按证据强度排序）

1. **【较强】退出走的是我们没覆盖的隔离路径**：既非 libc 出口、也非 FairGuard 两套桩、也非 AndroidRuntime::exit ⇒ 可能是载荷内部**自带的 syscall 内联**（在 `mprotect` 出来的另一块 RWX 上执行）、或由 **JNI/Java 侧的第三方 SDK** 触发（同包还有 `libthemis.so` 849,496 B、`libfntvcrash.so`）。已核对：这两个 .so 的**明文里没有任何** `META-INF/base.apk/signature/getPackageInfo` 字样（各 0 命中），所以更像仍在 FairGuard 载荷内。
2. **【中等】FairGuard 在运行中把 frida 的 inline hook 回滚**：字节探针只验到 2 s；30 s 前的完整性扫描可能 `mprotect` 复原代码（其桩表里就有 mprotect）。未验证（缺 t=25 s 的字节快照）。
3. **【可排除】hook 没装上**：`BYTES` 已证 trampoline 在位；`libc syscall` hook 在同一次运行里正常触发 5,082 次。

## 8. 产物

| 路径 | 内容 |
|---|---|
| `06_dumped/绿梦_20261016/fg_runtime/libFairGuard_rwxp.bin` | **运行期 4,194,304 B 镜像**（base..base+0x400000），md5 `8297e6b94021acbb87ac862c3acbab67`；含解包后的真实代码与 syscall 桩表 |
| `06_dumped/绿梦_20261016/fg_runtime/libthemis_apk.so / libfntvcrash.so / libthemis_dev.so` | 同包其它防护库（已扫：无明文签名关键字） |
| `04_scripts/绿梦/fg_probe.js` | 只读探针（门控 `HOOK_FG/HOOK_LIBC/HOOK_BYTES/SNAP_MAPS/DUMP/HOOK_JAVA`），preflight PASS |
| `09_test/绿梦/fg_forensics/logcat_*.txt` `frida_*.log` | 9 次运行的原始 logcat / frida 输出（约 20 MB） |

## 9. 未闭环项

1. **退出机制仍未命名**：所有已覆盖的用户态出口零命中；缺 Java bridge、缺 t≈25 s 的 hook 完整性快照。
2. **30 s 闸门 vs 反调试退出未能分离**：frida 在机时死亡时间提前且一次 SIGILL，说明注入本身触发反调试；要拿"签名分支"，必须先做**反调试隐匿**（改名的 server 已在用，但仍被识别：ptrace/prctl/属性探测）。
3. **4 MB 运行期镜像尚未做符号化/比对分析**：这是目前最值钱的离线资产（真实代码 + 桩表），下一步应在离线侧找字符串解密与签名比较。
4. **`0x35B000` 与 `0x42E000` 两套桩表的关系未定**（同镜像双表？还是不同运行布局？需同一进程内同时验证）。
5. **未验证**：FairGuard 是否在运行中回滚 inline hook（§7-2）。

## 10. 对"方案 2（patch libFairGuard.so）"的影响（给决策官）

- **磁盘字节 patch 大概率无效**：签名闸门不在明文壳（0x413AB0..0x431350）里，而在**运行期才解包出来的载荷**里；对磁盘上的加密区改字节，会在解包时被覆盖/解出错误码。
- **可行方向只有两条**：
  1. **运行期 patch**（在解包完成后改写内存里的比较/分支，或 hook 其内联 syscall）；需要先解决反调试隐匿。
  2. **离线解析 4 MB 解包镜像**（推荐下一步，零设备成本）：定位字符串解密 → 找 `signature/META-INF` 相关比较 → 反推"闸门判定点"，再决定是改壳的解包产物还是改判定分支。
- 现有 C2（`0x418184` 明文调用者）**同样被否证**（该地址 hook 零命中，且它属于磁盘壳的旧布局）。
