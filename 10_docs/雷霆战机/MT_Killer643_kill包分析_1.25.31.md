# MT Killer643 kill 包分析（雷霆战机 1.25.31）

日期：2026-08-10  
对象：`02_apks/雷霆战机/雷霆战机_1.25.31_mt_kill.apk`（不与斌哥包对比）

## 结论摘要

- **有 IO 重定向资产**：`assets/origin643.apk`（STORED，完整官包 998162749 字节），**无** `assets/SignatureKiller/` 目录。
- **运行时已抽出**：`/data/data/com.a5game.ltzj.jj/files/origin643.apk`（同尺寸，已存在）。
- Killer643 **会** `killPM` + `killOpen`（抽出 origin、设 path、loadLibrary）；**无** `hookApkPath` 符号/方法名。
- 安装后 `base.apk` ≈ **1.99GB**（源 kill ≈ **1.00GB**），与 overlapped zip 展开一致；native 若按 base 尺寸访问却 openat 重定向到 998MB origin → **SIGBUS 高发原因**。
- **2026-08-10 晚基线**：官包也能进 Cocos 后 SIGBUS；kill fault addr=`0x0`（见「实验基线」节）。本机 tombstone 不落盘。
- **2026-08-10 20:50 双轨**：（1）官包无 Frida/无 hide ≈9s SIGBUS（cocos 已上）→ root/ACE 环境杀；（2）kill-apsigned **永不** libcocos（强制 AppActivity 亦然）。详见「设备 ACE vs Root」节。下一步：**先 SukiSU hide 复测官包**，再 **重建 kill**；勿再优先 xhook。

## 1. Zip 中与 kill 相关的条目

| 条目 | 说明 |
|------|------|
| `assets/origin643.apk` | 998162749，STORED，CRC `0x8e11c57e`，**完整官包**（字节级嵌在 kill 内 offset `2981888`） |
| `lib/*/libSignatureKiller643.so` | arm64 19376 等 |
| `classes6.dex` | Killer Java（`bin.mt.signature.KillerApplication643`） |
| `classes2.dex` | 被改：`AceApplication` 继承 Killer643 |

无：`SignatureKiller/` 目录、`origin.apk`（非 643 名）、`libSignatureKiller.so`（无 643 后缀的独立名；so 内 soname 仍写 `libSignatureKiller.so`）。

Zip 结构：前缀仅 5 个 kill 条目（`classes2/6` + 三 ABI killer so），随后 `origin643` 覆盖整段官包；其余 **36295** 条 CD 指向 origin 载荷区间 → **overlapped entries**（Python `ZipFile` 报 zip bomb）。

## 2. origin 是否完整官包

- 与 `雷霆战机_集结_1.25.31_orig_from_device.apk` **同长** 998162749。
- kill 文件内 `[2981888, 1001144637)` 与官包 head/mid/tail 一致 → **full official APK，非 stub**。

## 3. AceApplication

- `Lcom/ace/gshell/AceApplication;` **extends** `Lbin/mt/signature/KillerApplication643;`（再 extends `Application`）
- `KillerApplication643.<clinit>` → `killPM()` + `killOpen()`
- `AceApplication.<clinit>` → `System.loadLibrary("tersafe2")` + `loadLibrary("tprt")`
- Killer `killOpen` 内再 `loadLibrary`（`SignatureKiller643`）
- Native stubs：`gp6ioctl` / `gp7ioctl` / `handleLoad*` / `ioctl` / `initialize`

## 4. libtersafe2.so（arm64）

- 字符串：`TSS_SDK_SIGDATA`、`UnzipSignatureData`、`TssSDKOnRecvSignature`、`__openat_2`、`kill`；**无** `base.apk` / `PackageInfo` 明文字符串。
- `TSS_SDK_SIGDATA`：文件偏移 **0x565400**（`.data` vaddr **0x566400**）；名后 size 字段 **336 (0x150)**；当前 payload **全 0**（运行时由 UnzipSignatureData 填充的占位）。
- 导出（节选）：`JNI_OnLoad`、`TssSDKInit`/`InitEx`、`tss_sdk_init`、`TssSDKOnRecvSignature`、`tss_recv_sec_signature`、`tp2_sdk_init*` 等。

## 5. 签名块

- **未去签**：仍有 v1 `META-INF/CLOVER.{RSA,SF}` + `MANIFEST.MF`，以及 v2 `APK Sig Block 42`（约 4KB @ CD 前）。
- Killer 用内嵌 clover 证书 Base64 做 `killPM` 伪签。

## 6. KillerApplication643 行为

| 能力 | 有无 |
|------|------|
| `hookApkPath` | **无**此方法名 |
| `killPM` | **有** — 伪 `PackageInfo`/`Signature`/`CREATOR` |
| `killOpen` | **有** — 从自身 zip 抽 `assets/origin643.apk` → `files/origin643.apk`，`setProperty(path1/path2)`，`loadLibrary` |
| native | xhook：`open` / `open64` / `openat` / `openat64`；**不** hook `mmap` / `fstat` / `__openat_2` |

依据：https://github.com/L-JINBIN/ApkSignatureKillerEx

## 7. 设备侧

```
/data/data/com.a5game.ltzj.jj/files/origin643.apk  998162749
/data/app/.../com.a5game.ltzj.jj.../base.apk       1996349422
```

另有 `ace_shell_di.dat`、`shell_cache/`、`ano_tmp/`（壳仍在跑）。


## 实验基线：官包 vs kill SIGBUS fault（2026-08-10 晚）

设备：PLK110 / Android 16 / KSU；包名 `com.a5game.ltzj.jj`。  
**本机 `/data/tombstones` 不落盘**（对 `sleep` 发 `SIGBUS`/`SIGSEGV` 亦为空；`trace=null`）。fault 靠 Zygote exit + Frida `Process.setExceptionHandler`。

### A) 官方原包 baseline

- APK：`02_apks/雷霆战机/雷霆战机_集结_1.25.31_orig_from_device.apk`（root `pm install`）
- **开机**：是（短暂）。截图：`08_output/雷霆战机/ltzj_orig_baseline_boot.png`
- **libcocos**：是（`/proc/pid/maps` + Cocos JS log：`Login SetAddress` / Common init）
- **ACE libs**：`libtersafe2.so` + `libtprt.so` 已映射
- **死亡**：约 **8–12s** 后进程退出；**有 SIGBUS**
  - Zygote：`exited due to signal 7 (Bus error)`（常见于 `:core` 与主进程）
  - `exit-info`：`SIGNALED` **status=7**（SIGBUS）或 **11**（SIGSEGV）；主进程偶发因 `:core` 死后 `unstable content provider` 被系统杀掉
- **结论**：在此机上**官包也无法稳定存活**；不能把 SIGBUS 单独归因于 Killer。环境（root/KSU/检测）已足够触发 ACE 杀进程。

### B) kill pmOnly — fault addr

- APK：`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-pmOnly.apk`（较小；expanded-pmOnly 同族 SIGBUS 循环）
- 现象：主进程 **~10–20s 一轮** 重启（pid 轮换）；`exit-info` 交替 **status=7 / 11**
- Tombstone：无（清仓后仍 total 0）
- Frida spawn 捕获（全文：`08_output/雷霆战机/ltzj_frida_sigbus.txt`）：
  - **fault addr = `0x0`**（`memory.operation=read`）
  - Frida 类型：`access-violation`（最终 Zygote 仍报 signal 7）
  - **PC / 符号（可能为二次崩坏，未必是 ACE 根因指令）**：
    1. `boot-okhttp.oat`！`com.android.okhttp.okio.AsyncTimeout.awaitTimeout+0x18`（pc `0xa9eb48a8`）
    2. `boot.oat`！`java.lang.Class.getName+0x14`（pc `0xa9b7a924`）
  - 同期已加载：`libSignatureKiller643.so`、`libtersafe2.so`、`libtprt.so`；该次 spawn 异常时点 **未见 libcocos**
- 脚本：`04_scripts/雷霆战机/ltzj_sigbus_exc.js`

### C) 仅 debug 重签（无 Killer）

- **未跑**：A 已表明官包在本机同样 SIGBUS 退出，重签对照收益低；若需可再补 PreserveSign/uber debug 签。


## SIGBUS 最可能原因（Killer643 之后仍崩）

1. **主因（尺寸不一致）**：`openat*` 把 `base.apk` 打开重定向到 ~998MB 的 `origin643.apk`，但 PM/其它逻辑仍按安装包 **~2GB** `base.apk` 的长度做 `mmap`/seek → 映到 EOF 外 → **SIGBUS**。
2. **钩不全**：`libtersafe2` 引用 `__openat_2`；Killer 只 hook `openat`/`openat64`/`open`/`open64`，未 hook `__openat_2` / `mmap`，校验仍可能打到真实 kill `base.apk`（overlapped/展开布局）。
3. **次要**：`TSS_SDK_SIGDATA` 占位与 `UnzipSignatureData` 从异常 APK 布局解签失败（更多是逻辑失败，不一定直接 SIGBUS）。



## Enhanced SignatureKiller fstat 试包 (2026-08-10)

- 产物：`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-fstat.apk`（**仍 1005916138 字节**，PreserveSign V2，CD/overlap 保留）
- 源码：`04_scripts/雷霆战机/mt_jni_fstat.c`；so：`08_output/雷霆战机/libSignatureKiller643_fstat.so`
- JNI：沿用 Killer643 的 `JNI_OnLoad` + `mt.signature.killer.path1/path2`（非 Ex 上游的 `hookApkPath`）
- 新增 xhook：`__open_2`、`__openat_2`、`stat`、`fstat`、`fstatat`、`dlopen`(load 后 refresh)；保留 open/open64/openat/openat64
- 依据：`libtersafe2`/`libtprt` 实际导入 `__open_2`/`__openat_2`/`fstat`/`fstatat`/`stat`（stock Killer **未** hook 这些）
- 注入：arm64 so 增大后在 origin643 之前重排三 ABI 本地头；x86/x86_64 换成最小 stub
- 开机结果（~30s）：**仍 SIGBUS(signal 7) 循环**；`libSignatureKiller643`+`libtersafe2` 有映射，**无 libcocos**；`origin643.apk=998162749`，`base.apk=1005916138`
- 过度 refresh 循环曾改为 **SIGSEGV**；已改为 dlopen 后单次 refresh
- 结论：尺寸/映射不一致 SIGBUS **未解决**；下一步更可能要 hook `mmap`/`mmap64` 或改 Zip 感知路径，而非只扩 open/stat 族

## libtersafe2 SIGBUS / process-kill 路径排查 (2026-08-10)

### 静态结论（arm64 `libtersafe2.so`，BuildId `732bc4bf…`）

| 项 | 结果 |
|----|------|
| `raise` | **无**导入 |
| `kill@plt` | `0xa8d10`；**仅 2 处** `bl`：`0x125384`（`w1=#0x13` SIGSTOP）、`0x1253b8`（`w1=#0x12` SIGCONT）— **不是 SIGBUS(7)** |
| `syscall`/`svc` + `__NR_kill`/`tgkill` + sig7 | **未找到** |
| `abort` / `_exit` | 有若干站点；与 SIGBUS 无直接对应 |
| `libtprt.so` | 有 `kill@plt`，**无** `bl kill` 调用点 |
| 字符串 | 有 `UnzipSignatureData` / `TssSDK*` / `kill` 名；**无** `SIGBUS` / `APKMOD` 明文 |

符号：`JNI_OnLoad@0x193fb8`，`TssSDKInit@0xaeba8`，`TssSDKInitEx@0xaebd4`，`tss_sdk_init@0xad628`。控制流大量混淆（`csel`+表跳），无清晰「签名失败 → raise(SIGBUS)」直链。

MT MCP：`adb forward` + `/mcp` initialize **可达**；Cursor 侧 `MT_APK_MCP` 描述符 STATUS 曾报错，本次以本机 `llvm-objdump`/`capstone` 为主。

### 等长补丁试改（设备 `/data/app/.../lib/arm64/libtersafe2.so`）

工作副本：`03_decompiled/雷霆战机/_mt_kill_probe/`

| 轮次 | 补丁 | 偏移 |
|------|------|------|
| A | `JNI_OnLoad` → `movz w0,#6; movk w0,#1,lsl#16; ret`（JNI_VERSION_1_6） | `0x193fb8` |
| A | `NOP` 两处 `bl kill` | `0x125384` / `0x1253b8` |
| A | `kill@plt` → `ret` | `0xa8d10` |
| B | 在 A 基础上：`.init_array` **58** 个 ctor 入口首指令 → `ret` | 见 `tersafe2_initret_patch_log.txt` |

- A 包 sha256：`b30f6b757395eb01d079ecad1dd135f0c9b36b551031b49e2ac9f5e8ea2f4776`
- B 包 sha256：`9f8e7bae60dd8fb1f90bad2d8c3ad14b8f4bcd0b85c7f068356257e74f7b14d6`

### 开机结果（各 ~30s）

- **仍** `Zygote: exited due to signal 7 (Bus error)` 循环；无新 tombstone（旧 tombstone_14 为先前 Killer **SIGILL**，无关）。
- 映射：`libSignatureKiller643` + `libtersafe2` + `libtprt`；**仍无 libcocos**。
- 即便 ACE ctor + `JNI_OnLoad` 全短路，SIGBUS **依旧** → **不是** tersafe 内 `kill(..., SIGBUS)` / 主动 raise 路径。

### 与尺寸假说交叉验证

当前安装（fstat 包）：`base.apk=1005916138`，`files/origin643.apk=998162749`。  
进程 maps 可见 `base.apk` 映射偏移 `0x3bac6000`（=1001152512）**大于** origin643 长度（998162749）。若 fd 被重定向到 origin 却按更大偏移/长度访问 → 典型 **真实 SIGBUS**（非 signal 投递）。

### 结论 / 下一步

1. **不要**再在 `libtersafe2` 里找「NOP SIGBUS kill」当主修复——该路径**不存在**（仅有 STOP/CONT 的 `kill`）。
2. SIGBUS 主因仍是 **Killer 打开重定向 vs APK 尺寸/mmap**；应优先 `mmap`/`mmap64`（及可能的 `pread64`）感知，或让对外可见的 APK 长度与 origin 一致。
3. 设备 so 已用 `toybox dd conv=notrunc` 写回 orig（普通 `cp`/`rm` 对 `apk_data_file` 常 Permission denied）；备份仍在 `/data/local/tmp/libtersafe2_orig.so`。


## Expanded (~2GB) + fstat/mmap Killer 试包 (2026-08-10)

### 假设
Overlapped kill + 全量 IO 重定向 → `mmap(base)` 偏移超过 origin 长度 → **SIGBUS**。  
物化展开 zip（~2GB，每 CD 条目独立载荷）+ 增强 Killer（`fstat`/`__openat_2` + **mmap 长度钳制**）或可开机。

### 做法
1. 源：`02_apks/雷霆战机/雷霆战机_1.25.31_mt_kill.apk`（overlapped ~1.00GB / 1005916138）。
2. 展开：`04_scripts/雷霆战机/expand_overlap_apk.py`（按 CD 逐条拷贝 unique 载荷，保留 `assets/origin643.apk`）。
   - 产出原始展开：`1996342049` 字节（≈安装后 `base.apk` 历史尺寸）。
3. 替换 `lib/arm64-v8a/libSignatureKiller643.so`：`08_output/雷霆战机/libSignatureKiller643_fstat.so`（源 `04_scripts/雷霆战机/mt_jni_fstat.c`，含 open*/stat/fstat/`__open*_2`/**mmap/mmap64 clamp**/dlopen refresh；xhook 链自 ApkSignatureKillerEx）。
4. `uber-apk-signer` allowResign + zipalign →  
   **`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-expanded.apk`**（~1.9GB / debugSigned）。

### 装机观测（~35s）
| 项 | 结果 |
|----|------|
| 安装 `base.apk` | **1996353518**（已是展开体，非 1GB overlap） |
| `files/origin643.apk` | **998162749**（Killer 抽出正常） |
| Killer / tersafe2 | 均成功 `loadLibrary` |
| **SIGBUS** | **仍有**：`Zygote: Process exited due to signal 7 (Bus error)`；35s 内多次 `has died` 重启 |
| pid | **不稳定**（例：16736→16951→17023…） |
| libcocos | **未**出现在 maps |
| top activity | 卡在 `com.xxx.sdk.GamePrivacyActivity`（未进游戏） |
| maps 中 `base.apk` | 存在高偏移映射（如 off `0x76599000` / `0x76b54000`，末端 ~`0x76fdf000` ≈ base 全长）；**大于** origin `0x3b7ec13d`。说明部分映射打在真实 2GB base 上，但进程仍因 Bus error 死 |

截图：`08_output/雷霆战机/ltzj_mt_kill_expanded_boot.png`

### 结论（boot verdict）

**失败 / 假设未成立（至少在「展开 + 全量 path 重定向 + fstat/mmap 钳制」这一形态下）。**

- 展开本身成功，且设备 `base.apk`≈2GB、origin 完整抽出。
- **不能开机**：仍稳定复现 **signal 7 SIGBUS**，无 libcocos，隐私页循环重启。
- 解读：Killer 仍对 `mt.signature.killer.path1`（`base.apk` 路径）做 **无差别 open 重定向 → origin**；校验/壳逻辑若按 **展开后 CD/长度** 去 `mmap` 已重定向的 fd，偏移仍可越过 origin EOF → SIGBUS。`mmap` xhook 钳制未能消除该路径（可能未钩到实际调用点，或钳制后仍有其它访问方式）。
- 先前 overlap 包上的 fstat-only 试包同样未解决；本次证明 **「仅物化 zip + 增强 fstat/mmap」不足**。

### 后续方向（未做）
- 缩小重定向范围（仅签名/`META-INF`/特定读，而不是全部 `base.apk` open）。
- 或：展开包上 **关闭 killOpen 重定向**，只留 `killPM` 伪签（若壳只校 PM）。
- 或：重定向后把「逻辑文件」做成与 base 同布局的 2GB 视图（而不是 998MB origin）。

## killPM-only / empty JNI_OnLoad 试包 (2026-08-10)

### 假设
关掉 MT IO 重定向（xhook），只留 `killPM`：若 SIGBUS 消失 → 重定向/尺寸不一致是主因；若仍 SIGBUS → 重定向不是根因（剩 ACE/其它路径）。

### 做法（优先 empty so，未改 smali）
1. 源：`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-apsigned.apk`（1005916138，overlapped）。
2. 编空 `libSignatureKiller643.so`：`JNI_OnLoad` 仅 `return JNI_VERSION_1_6`（`04_scripts/雷霆战机/mt_jni_empty.c`）— **不**注册 open/xhook。
3. 三 ABI 前缀条目原为 DEFLATE；改为 **STORED** 写入 stub（arm64 5048 / x86 3648 / x86_64 4744），更新 LH+CD 的 method/crc/csize/usize；APK 总长不变。
4. `PreserveSign` V2（`debug.p12`）→  
   **`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-pmOnly.apk`**（仍 **1005916138**；V2 verifies，V1 META-INF 不计）。

`killOpen` Java 仍跑：抽 `origin643`、设 path 属性、`loadLibrary`；仅 native 钩子缺席。

### 装机观测（~30s）
| 项 | 结果 |
|----|------|
| **SIGBUS** | **仍有** — 多次 `Zygote: exited due to signal 7 (Bus error)`（例 pid 21467→22032→22193） |
| `libSignatureKiller643` | load **ok**（空 JNI） |
| `libtersafe2` / `libtprt` | 有映射 |
| **libcocos** | **无** |
| top activity | `com.xxx.sdk.GamePrivacyActivity` |
| `files/origin643.apk` | **仍抽出** 998162749 |
| `base.apk` | 1005916138 |

截图：`08_output/雷霆战机/ltzj_mt_kill_pmOnly_boot.png`

### 结论
**IO 重定向不是 SIGBUS 的充分条件**（关掉 hooks 后仍 signal 7）。`killPM` + 抽出 origin 仍发生。下一步应转向 ACE/壳对 **真实 overlapped base.apk** 的访问/校验（或其它非 Killer-open 路径），而不是继续只扩 Killer xhook。

## pmOnly：关闭 IO 重定向（仅 killPM）试包 (2026-08-10)

### 假设
SIGBUS 主因是 Killer native `open*` 把 `base.apk` 重定向到 ~998MB `origin643`，却按更大偏移/`base` 长度映射。  
若 **关掉 xhook/open 重定向**，只保留 Java `killPM`（+ `killOpen` 仍抽 origin / 设 path 属性，但 native 为空），SIGBUS 应消失。

### 做法
1. 源：`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-apsigned.apk`（1005916138，CD/overlap 保留）。
2. 空 so：`JNI_OnLoad` 仅 `return JNI_VERSION_1_6`（**无 xhook、无 open 重定向**）。
   - 源：`04_scripts/雷霆战机/mt_jni_empty_killer.c`
   - so：`08_output/雷霆战机/libSignatureKiller643_empty.so`（arm64 5048；另有 x86/x86_64 stub）
3. 等长就地替换三 ABI `libSignatureKiller643.so`（STORED + pad），CD 尺寸与 CRC 更新；`PreserveSign` V2 重签。
4. 产物：`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-pmOnly.apk`（**仍 1005916138**）。

### 装机观测（~35s+，循环采样）
| 项 | 结果 |
|----|------|
| 安装 `base.apk` | **1005916138**（overlap 体） |
| `files/origin643.apk` | **998162749**（Java `killOpen` 仍抽出） |
| Killer so | 已映射（极小：空 `JNI_OnLoad`） |
| tersafe2 / tprt | 已映射 |
| libcocos | **无** |
| **SIGBUS** | **仍有**：`Zygote: Process exited due to signal 7 (Bus error)` 循环；偶发 **signal 11 (SIGSEGV)** |
| pid | **不稳定**（例：23470→23657→23737→23934→…→25581…） |
| top activity | 卡在 `com.xxx.sdk.GamePrivacyActivity` |
| maps `base.apk` | 有高偏移映射 `off 0x3bac6000`（=1001152512，CD 区）；**无** `origin643.apk` 文件映射（说明 open 重定向确实未生效） |

截图：`08_output/雷霆战机/ltzj_mt_kill_pmOnly_boot.png`

### 结论（boot verdict）

**失败 / 假设未成立。关闭 Killer IO 重定向并不能消除 SIGBUS。**

- killPM + 空 Killer so 下，进程仍稳定复现 **signal 7**，无 libcocos，隐私页重启循环。
- Java 侧仍抽 `origin643` 并设 `mt.signature.killer.path*`，但 maps **未见** origin 文件映射 → native 重定向已关掉。
- 因此：至少在本 overlap kill 包上，**SIGBUS 不只是「open→origin 尺寸不一致」这一条路径**；真实 `base.apk`（1.00GB overlapped）本身在加载/校验路径上仍会 Bus error（或另有 tersafe/壳路径）。
- 与先前 fstat/expanded 试包交叉：增强或关闭重定向均未开机；需换方向（例如非 overlap 安装布局、或壳/ACE 在读 zip 时的别的 fault，而不再只盯 Killer open hook）。

## Expanded (~2GB) + pmOnly（空 Killer / 无 IO 重定向）试包 (2026-08-10)

### 假设
overlap 体本身可能让 ART/`mmap` 对 `base.apk` 出错。若 **展开 zip（~2GB）+ 关掉 Killer native 重定向**（空 `JNI_OnLoad`），只留 Java `killPM`，则：
- 若 SIGBUS 消失 → 先前是 overlap 布局和/或 redirect 尺寸问题；
- 若仍 SIGBUS → 更像壳/ACE 对签名或其它路径（非 Killer open hook）。

### 做法
1. 源：已有 `08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-expanded.apk`（~1.9GB）。
2. 替换三 ABI `lib/*/libSignatureKiller643.so` → 空 `JNI_OnLoad`（`libSignatureKiller643_empty.so` / `mt_jni_empty_killer.c`；arm64 **5048**）。
3. uber-apk-signer debug resign（V1+V2+V3 ok）。
4. 产物：`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-expanded-pmOnly.apk`（**1996341230**）。

### 装机观测（~35s）
| 项 | 结果 |
|----|------|
| 安装 `base.apk` | **1996341230**（展开体） |
| `files/origin643.apk` | **998162749**（Java `killOpen` 仍抽出） |
| Killer so | load **ok**（空 JNI；maps 极小） |
| tersafe2 / tprt | load **ok**（`AceApplication`/`classes2`） |
| libcocos | **无** |
| **SIGBUS** | **仍有**：`Zygote: Process exited due to signal 7 (Bus error)`（pid 28412、28660、28951…） |
| signal 11 | **偶发**（例 pid 28834 SIGSEGV） |
| pid | **不稳定**（28412→28660→28834→28951→29083…） |
| top activity | 卡在 `com.xxx.sdk.GamePrivacyActivity` |
| maps `base.apk` | `r--s` 高偏移 `0x76591000`、`0x76b51000`（≈99.5% / 99.8% of 2GB）；**无** `origin643.apk` 文件映射 |
| Ace/gshell `initialize` | **无完成证据**：logcat 无 AceApplication/gshell initialize 成功痕迹；仅见 tersafe2/tprt/SignatureKiller load；崩在隐私页循环，未到 cocos |

截图：`08_output/雷霆战机/ltzj_mt_kill_expanded_pmOnly_boot.png`

### 结论（boot verdict）

**失败 / 假设未成立。** 展开 + 关 IO 重定向 **不能**消除 SIGBUS。

- 与 overlap+pmOnly 同病：空 Killer + killPM 下仍 signal 7（偶发 11），无 libcocos。
- maps 已直接映真实 ~2GB `base.apk`（非 origin）→ 排除「open→origin EOF」作为本变体充分条件。
- 交叉四变体后：**SIGBUS 对 overlap/expand × redirect/pmOnly 全复现** → 需转向签名校验失败后的壳行为、或其它非 Killer-open 的 apk 访问路径（含 ACE 初始化是否在校验后主动异常）。

### 四变体对照矩阵

| variant | SIGBUS | libcocos |
|---------|--------|----------|
| overlap+redirect（mt_kill / apsigned / 原 Killer） | 有（signal 7） | 无 |
| overlap+pmOnly（空 Killer） | 有（signal 7；偶发 11） | 无 |
| expand+fstat redirect（expanded + fstat/mmap Killer） | 有（signal 7） | 无 |
| expand+pmOnly（本试包） | 有（signal 7；偶发 11） | 无 |


## 设备 ACE vs Root / Kill 永不进 cocos（2026-08-10 20:50）

设备：PLK110 · **KernelSU**（`ksud 4.1.3`，manager `com.sukisu.ultra`）· **无 Magisk**。  
模块：仅 `Cdd`；**无** Shamiko / Zygisk / Magisk DenyList。`ksud susfs` → unsupported；`ksud umount list` 空。

### Track 1 — 官包是否被 root/KSU 打死？

**Root 栈**

| 项 | 结果 |
|----|------|
| Magisk | `magisk` 不存在；`/data/adb` 有 `ksu`/`ksud`/`modules` |
| KSU allowlist | 仅 `com.abc.abc`；**无** `com.a5game.ltzj.jj` |
| 非交互隐藏 | **做不到 DenyList 级**：`ksud profile` 只有 sepolicy/template；无 per-app umount CLI。需 **SukiSU Manager → App Profile**（对该包 Non-root + Umount modules）。全局 `ksud umount add` 非 per-app。 |

**官包重装 + 无 Frida + 无 hide · 60s**

- APK：`02_apks/雷霆战机/雷霆战机_集结_1.25.31_orig_from_device.apk` → `base.apk` **998162749**，sig `e2a46a42`
- 时间线（`08_output/雷霆战机/ltzj_official_60s_summary.txt`）：
  - **t≈1s**：`libtersafe2` + `libtprt` → **`libcocos` 已加载**；top=`AppActivity`
  - **t≈9s**：`:core` **SIGBUS (signal 7)** → 主进程随之 `has died`；之后 **无进程**（不进隐私重启环）
- 截图：`08_output/雷霆战机/ltzj_official_60s_nofrida.png`
- logcat：`08_output/雷霆战机/ltzj_official_60s_detection.txt` — **无** magisk/ksu/`/data/adb`/shamiko 等 tersafe 明文；仅 Zygote signal 7

**结论 (1) 环境 ACE / 壳杀 root 机官包**：官包 **能进 cocos**，约 **8–12s** 后仍 **SIGBUS 身亡**。与「hide 未开」一致；**尚未**在 Manager 里对该包做 umount 复测（CLI 无法代劳）。

### Track 2 — 为何 kill 包永不加载 libcocos？

包：`08_output/雷霆战机/雷霆战机_1.25.31_mt_kill-apsigned.apk`（`base` **1005916138**，sig `3b3ed849`）。

| 试验 | top | Killer/tersafe | libcocos | 死亡 |
|------|-----|----------------|----------|------|
| clean 启动 | 卡 `GamePrivacyActivity` | 有 | **无** | ~19s SIGSEGV → 重启环；后混 SIGBUS |
| `am start …/AppActivity`（+2s） | 可切到 `AppActivity` | 有 | **仍无** | ~8s 起 SIGBUS 循环 |

产物：`ltzj_kill_apsign_clean_timeline.txt` / `forceApp_timeline.txt` + 对应 png。

**与官包对照**

| | 官包（root，无 hide） | kill-apsigned |
|--|---------------------|---------------|
| cocos | **有**（~1s） | **从未** |
| 死亡形态 | ~9s 一次 SIGBUS 后空 | 隐私/AppActivity **重启环**（11/7） |
| 根因层 | **(1) 环境/ACE 晚杀** | **(2) 改包路径到不了 cocos**（早于/平行于晚杀） |

**AceApplication（classes2 前缀，仍成立）**

路径：`CloverApplication` → `AceApplication` →（kill 上）`KillerApplication643` → `Application`。

```
attachBaseContext → attachBaseContextShell → native initialize(...)
AceApplication.<clinit> → loadLibrary(tersafe2/tprt)
KillerApplication643.<clinit> → killPM + killOpen → loadLibrary(SignatureKiller643)
```

kill 上实测：Killer → tersafe2 → tprt **均 ok**；**无** `initialize failed` 明文；**无** cocos load。强制 `AppActivity` 也不能拉起 cocos → 不是「隐私 UI 没点完」这么简单，而是 **壳/校验在进游戏 so 之前就把进程弄残或挡掉加载**（与官包「先 cocos 再晚杀」分离）。

### Track 3 — 手机 MT 签名 kill？

```
/sdcard/MT2/mcp/ltzj_1.25.31_orig_kill.apk   959M  md5=25a5b505… (= 02_apks/…/雷霆战机_1.25.31_mt_kill.apk)
```

**无** `*sign*` 产物。手机只有 MT 打的 kill 源包；PC `*-apsigned` 为另签，不是「MT 管理器一键签好的独立包」。

### 双轨分离（写死）

1. **环境 ACE 杀 root 官包**：cocos 能上，~8–12s SIGBUS；需 **SukiSU App Profile 隐藏** 后再测 60s 存活。  
2. **kill-apk 永不进 cocos**：与 root 晚杀 **不同问题**；overlap/Killer/壳初始化挡在 `libcocos` 之前；force AppActivity 无效。

### 下一步工程判定

| 优先 | 动作 | 理由 |
|------|------|------|
| **1** | **Hide root**（SukiSU UI：`com.a5game.ltzj.jj` Non-root + umount modules）→ 复测官包 60s | 验证轨 1；官包已证明能加载 cocos |
| **2** | **重建 kill（去 overlap / 勿让 Ace 继承链+Killer 挡壳）** | 轨 2：先让 kill 也出现 libcocos，再谈存活 |
| **3** | 再考虑 **patch Ace shell / initialize** | 仅当 kill 已能进 cocos 或官包 hide 后仍壳杀 |

**不要**再优先堆 Killer xhook/fstat（四变体已证不足）；**不要**假设「只 hide root 就能修好 kill 无 cocos」。
