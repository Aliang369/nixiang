# 绿梦 · 方案 A 第一刀：seccomp 拦 tgkill 取 killer PC（RET_LOG 档）实测报告

> 日期：2026-10-20 04:02–04:14　设备：f9a116a2（OnePlus OPD2404 / Android 16 / KernelSU root）
> 包名：com.zhuziplay.lvmengnew.op 1.0.5 / vc12035　底包 md5 e59d4250747bd4e2ed1d80e231e6b272
> 本轮唯一自变量 = **在 mod 壳里加 seccomp tgkill 闸（mode 1 = SECCOMP_RET_LOG）**；其余（NOP 后的 libFairGuard、SIGILL 吞信号、fgspoof、StateMirror）逐字节沿用上一轮。

## 0. 结论（先看这段）

1. **闸装上了，且真的生效**：壳被 dlopen 时（04:05:00.937，游戏冷启前 0.06 s）安装；
   `PR_SET_NO_NEW_PRIVS` + `seccomp(SET_MODE_FILTER, TSYNC)` **rc=0**，`/proc/self/status` 的
   `Seccomp_filters` 由 1（zygote 自带）→ **2**，audit 里随即出现 `type=1326 … syscall=131 … comm="UnityMain"` ⇒ 覆盖面含 UnityMain，取证通道打通。
2. **本轮没有拿到「调用者模块+RVA」——但拿到了决定性否定证据**：killer 的 tgkill **不是裸 svc**，
   而是走 bionic 的 `tgkill()` / `syscall()` 包装，所以 audit 的 `ip=` 落在 **libc.so**：
   - 04:05:38.411（= SIGILL 死亡点）`ip=0x755ed32fe4` → **libc.so + 0xaffe4**，反汇编确认该地址在
     `libc.so:000affc0 <syscall>` 里、紧跟 `affe0: svc #0` 之后一条（`affe4: cmn x0, #0x1, lsl #12`）；
   - 04:05:05.515/.667（4 条）`ip=0x755ed7244c` → **libc.so + 0xef44c**，同法确认为 libc 的
     `tgkill` 内联包装（`ef444: mov x8, #0x83 (131)` / `ef448: svc #0` 之后一条）。
   ⇒ **RET_LOG 只能给出「svc 之后的 libc 桩地址」，永远给不出调用者**。要 killer PC 必须拿 **x30(lr)**，
   即 mode 2 = `SECCOMP_RET_TRAP` + SIGSYS 处理器（本仓 nhsy2 §⑦.6h 已有现成实现，改 syscall 号即可）。
   这一条把「方案 A 第一刀是否值得继续」变成了确定性结论，不是猜测。
3. **附带重大发现（改变对死亡机制的认知）**：本轮的死亡表现**不是 101–157 s 的周期检测**，而是
   **37.8 s 一次「伪造崩溃」**：tgkill(UnityMain,SIGILL) → 信号被 Unity/CrashSight 的 SIGILL 处理器接走
   （不是我们 ctor 里那个，已被覆盖）→ 打完整 native 崩溃报告 + `tombstone_00` → **04:05:40.776 游戏自己
   `finish()` 回桌面**，而**进程一直活到 557 s+ 没被 SIGKILL**。也就是说上一轮看到的「101–157 s 死」很可能
   依赖「玩家已进入对局」这一条件；本轮不碰屏幕（无 tap）⇒ 41 s 就退回 launcher，后续 cron 式检查没有再触发。
   （判定：实测；与 §18「269 s 才死」的差异来源见 §8 未闭环。）

## 1. 实现位置（纯追加，未碰公共文件）

| 项 | 值 |
|---|---|
| 新增源文件 | `mod菜单/starry-mod-menu/app/src/main/jni/Games/LvmengSeccomp.cpp`（238 行，新文件） |
| 挂载点 | `app/src/main/jni/Android_lvmeng.mk`：`LOCAL_SRC_FILES := MainModern.cpp Games/LvmengMod.cpp Games/LvmengEntry.cpp **Games/LvmengSeccomp.cpp** Unity/Il2CppApi.cpp` |
| 安装时机 | `__attribute__((constructor))`（so 一被 dlopen 就装，早于 Unity/FairGuard 起业务线程） |
| 开关 | prop `debug.starry.lvmeng_tgkillguard`：未设/0 = 不装；1 = RET_LOG；>1 按 1 处理（2/3 档未实现） |
| BPF 目标 | 主拦 **131 tgkill / 130 tkill / 240 rt_tgsigqueueinfo**，兜底 **129 kill / 424 pidfd_send_signal**；`AUDIT_ARCH_AARCH64` 校验不变 |
| 动作 | 命中 → `SECCOMP_RET_LOG`；其余 → `SECCOMP_RET_ALLOW`（**不改寄存器/PC，syscall 照常执行**） |
| 装法 | `prctl(PR_SET_NO_NEW_PRIVS,1)` → `syscall(__NR_seccomp, SET_MODE_FILTER, FILTER_FLAG_TSYNC, &prog)`；失败记 errno 并**退回单线程**安装，再起 6×300 ms 退避线程重试 TSYNC |
| 日志 | tag `StarryLvmeng`（`tgkill 闸已装…` / 装前·装后 `Seccomp=… Seccomp_filters=…`） |
| 未做（按任务书） | SIGSYS 处理器、RET_TRAP、任何寄存器改写 |

先例移植说明：BPF/TSYNC/失败回退**整段照 `Games/nhsy2Mod.cpp` §⑦.6h（L4756–4807）**，仅把命中号从 `__NR_exit_group(94)` 改为 tgkill 族 + 双兜底；`ResolvePcToModule/FlushExitGuardRecords` 等 TRAP 档部件本轮未搬。

## 2. 构建产物

| 项 | 值 |
|---|---|
| 构建脚本 | `bash 04_scripts/绿梦/lvmeng_build_so.sh`（mk 变化触发 obj 全量重编，链接活动保险通过） |
| 产物 | `mod菜单/starry-mod-menu/libs_lvmeng/arm64-v8a/libstarry_magic.so` |
| 大小 / md5 | **2,138,576 B / `3a52c3163d610518c15cb2fcf6e55e75`**（sha256 `6943e661…e2b2`） |
| 串校验 | `lvmeng_tgkillguard`×2、`tgkill 闸`×8、`RET_LOG`×2、`StarryLvmeng`、`lmskzs`、6 条菜单串全在 |
| preflight | `bash 04_scripts/_common/preflight.sh` → 2×REMIND（.cpp/.mk 无独立语法器，以构建代替）→ 已用真实构建+装机验证 |

## 3. 装机包

**偏离说明（必须记录）**：未用 `mod菜单/starry-mod-menu/scripts/surgical_lvmeng_1.0.5.sh`。
理由：该脚本的底包写死 `/tmp/lvmeng_apk/base.apk`（官方原包，md5 强校验），**会丢掉上一轮已 NOP 的
libFairGuard.so**（0x2F08C+0x47E1C 两处 `bl exit_group`），装上去必然回到 35 s 闪退，
且等于同时改两个自变量。改用**上一轮同一处方的外科链**（该链已在本仓落档、产物已装机验证）：

```
底包 07_patched/绿梦/fg_test/绿梦_mod_noexit_sigill.apk   (371,355,358 B / md5 4ddf06a2…，含 NOP FairGuard + SIGILL 吞 + 旧 so)
→ python3 04_scripts/绿梦/fg_so_swap.py --entry lib/arm64-v8a/libstarry_magic.so --so <新 so>
→ bash 04_scripts/绿梦/surgical_equal_length_resign.sh --no-pad    (apksigner 重签，v1+v2)
→ 07_patched/绿梦/fg_test/绿梦_seccomp_audit.apk
```

| 项 | 值 |
|---|---|
| 新包 | `07_patched/绿梦/fg_test/绿梦_seccomp_audit.apk` |
| 大小 / md5 | **371,359,454 B / `79803a8c2524b4cb8d0f092b9c4c48a7`**（apksigner verify OK，`zipfile.testzip()=None`） |
| 包内 so | `libstarry_magic.so` md5 **3a52c316…**（= 构建产物，逐字节一致）；`libFairGuard.so` md5 **5f4b4556…**（= NOP 版，未动） |
| 装机 | `bash 04_scripts/_common/install_starry_apk.sh --apk … --pkg com.zhuziplay.lvmengnew.op --serial f9a116a2` → **Success（同签覆盖，未卸载、未清数据）**；装机闭环校验「设备包 md5 == 本地产物」✓、「运行 so md5 == 包内 so」✓ |
| 设备画像 | 装机前 `device_profile.sh --serial f9a116a2 --pkg …`：arm64-v8a / page=4096 / android=16 / 进程未运行（基址层未探测，不影响本轮） |

## 4. 真机取证时间线（单次冷启，全程无 screencap、无 input tap）

| 时刻 | 事件 |
|---|---|
| 04:04:59 | `logcat -b all -c` 清缓冲；起 `-b events` 与 `-b main` 两路落盘 |
| 04:05:00.937 | **seccomp 闸装好**：装前 `Seccomp=2 Seccomp_filters=1` → 装后 `Seccomp_filters=2`，TSYNC rc=0 |
| 04:05:00 | `am start -W -n …/com.bamboo.game.SplashActivity`，LaunchState=COLD（pid 31401） |
| 04:05:05.0 | Splash → `MainFragmentActivity` |
| 04:05:05.5 / .667 | 4 条 audit `syscall=131 ip=libc+0xef44c`（libc `tgkill` 包装） |
| **04:05:37.797** | `E/[CrashSightReport-Native] get signal 4`（= FairGuard 的 tgkill(UnityMain,SIGILL) 落地） |
| **04:05:38.411** | **audit：`comm="UnityMain" syscall=131 ip=0x755ed32fe4`（= libc `syscall()` +0xaffe4）** ← 本轮 killer 事件 |
| 04:05:38.414 | Unity 崩溃报告（tid 31646 UnityMain，SIGILL/SI_TKILL，pc=libil2cpp+0x37b16c0，lr=+0x37b14f0） |
| 04:05:38.638 | `Tombstone written to: /storage/emulated/0/Android/data/…/files/tombstone_00` + 反复 `Forwarding signal 4` |
| **04:05:40.776** | `wm_finish_activity … MainFragmentActivity, app-request` → 游戏自己退回 launcher |
| 04:12:49 / 04:14:17 | 进程仍存活（469 s / **557 s**），**无新 audit、无 SIGKILL、无系统 tombstone** |
| 04:14:1x | 收尾：`setprop debug.starry.lvmeng_tgkillguard 0`，停 logcat |

## 5. audit 原文切片（逐字）

生效证据（main）：
```
10-20 04:05:00.937 I/StarryLvmeng(31401): tgkill 闸 · 装前 seccomp 状态：Seccomp=2 Seccomp_filters=1（2=filter 已生效）
10-20 04:05:00.937 I/StarryLvmeng(31401): tgkill 闸已装：mode=1 RET_LOG · TSYNC 成功（覆盖全部现有线程）· 目标 syscall 131/130/240 + 兜底 129/424 · 装后内核 audit 会记 ip=
10-20 04:05:00.937 I/StarryLvmeng(31401): tgkill 闸 · 装后 seccomp 状态：Seccomp=2 Seccomp_filters=2（2=filter 已生效）
10-20 04:05:00.937 I/StarryLvmeng(31401): tgkill 闸：prop 原文="1" mode=1 结果=0（0=TSYNC 全线程 / 1=仅本线程 / -1=失败）
```
死亡点那条（events，全仓唯一 1 条与崩溃同刻）：
```
10-20 04:05:38.411 I/auditd  (31401): type=1326 audit(0.0:1807): auid=4294967295 uid=11088 gid=11088 ses=4294967295 subj=u:r:untrusted_app:s0:c64,c260,c512,c768 comm="UnityMain" exe="/system/bin/app_process64" sig=0 arch=c00000b7 syscall=131 compat=0 ip=0x755ed32fe4 code=0x7ffc0000 app=com.zhuziplay.lvmengnew.op
```
早期 4 条（同一 ip，libc `tgkill` 包装）：
```
10-20 04:05:05.515 I/auditd  (31401): type=1326 audit(0.0:1317): … comm="UnityMain" … syscall=131 compat=0 ip=0x755ed7244c code=0x7ffc0000 app=com.zhuziplay.lvmengnew.op
10-20 04:05:05.515 I/auditd  (31401): type=1326 audit(0.0:1318): … ip=0x755ed7244c …
10-20 04:05:05.667 I/auditd  (31401): type=1326 audit(0.0:1322): … ip=0x755ed7244c …
10-20 04:05:05.667 I/auditd  (31401): type=1326 audit(0.0:1323): … ip=0x755ed7244c …
```
崩溃报告的「受害者」侧（**注意：pc 是被打断的线程，不是发送者**）：
```
10-20 04:05:38.414 E/CRASH (31401): pid: 31401, tid: 31646, name: UnityMain  >>> com.zhuziplay.lvmengnew.op <<<
10-20 04:05:38.414 E/CRASH (31401): signal 4 (SIGILL), code -6 (SI_TKILL), fault addr --------
10-20 04:05:38.414 E/CRASH (31401):     lr  0000007317fb54f0  sp  000000736f3f0530  pc  0000007317fb56c0
10-20 04:05:38.414 E/CRASH (31401):       #00 pc 00000000037b16c0  libil2cpp.so
10-20 04:05:38.415 E/CRASH (31401):       #01 pc 0000000005781ea8  libil2cpp.so
```

## 6. ip 归一结果（本轮成功判据的正面对照）

| ip | 归一（/proc/31401/maps，pid 存活期间采） | 反汇编定位 | 结论 |
|---|---|---|---|
| `0x755ed32fe4` | **libc.so + 0xaffe4**（r-xp 段 0x755ecde000+0x5b000） | `libc.so:000affc0 <syscall>`：`affe0 svc #0` → **ip = svc 的下一条**（libc 通用 `syscall()` 包装） | 不是凶手 PC |
| `0x755ed7244c` | **libc.so + 0xef44c** | libc `tgkill` 内联包装：`ef444 mov x8,#131` / `ef448 svc #0` → ip = svc+4 | 不是凶手 PC |
| （对照）`0x7317fb56c0` | libil2cpp.so + **0x37b16c0**（rwxp 影副本 0x7314804000 起，与崩溃报告 #00 完全对上） | — | 受害者（UnityMain）被打断处，**不是发送者** |

⇒ **killer PC 未取得**；但「tgkill 一定经 libc 包装」这一点是硬证据（两条不同包装都实测到，且逐条反汇编核对）。

## 7. 判据对照

| 判据 | 结果 |
|---|---|
| 拿到 ≥1 个 tgkill 调用者 ip | ✅ 拿到 2 种共 5 条（0x755ed32fe4 / 0x755ed7244c） |
| 归一成「模块 + RVA」 | ⚠️ 归一成功但落在 **libc.so+0xaffe4 / +0xef44c**，**不是 killer 模块** |
| 理想（libFairGuard.so + 0xXXXXX） | ❌ 未达成；RET_LOG 档在原理上做不到（见 §0.2） |
| 「闸生效」证明 | ✅ `Seccomp_filters 1→2` + TSYNC rc=0 + 5 条 type=1326 记录（其中 comm=UnityMain） |
| 「装闸导致提前死」（失败分支） | ❌ 不成立：没有提前死，反而不死（557 s+），也没有 tombstone/SIGKILL；死点表现见 §0.3 |

## 8. 新发现与未闭环

**新发现（实测）**
1. 死亡动作是「**伪造 native 崩溃**」：tgkill(SIGILL) → CrashSight 接管并上报（`REPORT ID 93eb9a19…`，`uc:1`）→ 写 `tombstone_00` → 游戏自己 finish 回桌面。**不是** FairGuard 直接 SIGKILL 进程。
2. 我们 ctor 装的 SIGILL 处理器**已被 CrashSight 覆盖**（本轮 `get signal 4` 由 CrashSight 打出）⇒ 上一轮「吞 SIGILL 把死点推到 269 s」这一结论的机制需要重审：真正吞信号的不是我们，或者覆盖时序每一轮不同。
3. 本轮 41 s 就退回 launcher（无 tap、无人进对局）⇒ 「101–157 s 周期检测」**很可能以「已进入对局」为前提**；不碰屏幕时它不复发（557 s 无第二次 tgkill）。

**未闭环**
1. **killer PC 本体**：需 mode 2（`RET_TRAP` + SIGSYS 取 `x30/lr`、`x29/fp`、`sp`、`tid`、栈字），先例代码（nhsy2 §⑦.6h L4679–4754）可直接搬，只改 syscall 号与日志前缀；风险：RET_TRAP 会把 tgkill 变成 `-EPERM`（等于顺手中和「伪造崩溃」），可能引出 FairGuard 升级惩罚，需单变量记录死点时间。
2. `comm="UnityMain"` 与「受害者 tid 31646 的 pc 在 libil2cpp」**互相矛盾**（同一线程不可能既在 libc 桩里发信号、又在 il2cpp 里被打断）⇒ 两个候选假设待 mode 2 判：**H1 killer 线程把自己改名成 "UnityMain" 伪装**（更可能）；H2 audit 的 comm 取的是进程组名而非发起线程名。mode 2 的 `tid`+`/proc/self/task/<tid>/comm` 能一次分开。
3. 「37.8 s vs 101–157 s vs 269 s」三个死点的触发条件（是否依赖局内状态）未定：需一次「用户手动进对局」的对照跑。
4. 本轮的 `ip` 归一依赖进程存活时的 maps（做到了）；若下一轮进程先死，需改为预采 maps 或用 `/proc/<pid>/maps` 死前快照。

## 9. 复现与证据落点

```
# 装闸（已装在本轮包内）
adb -s f9a116a2 shell su -c 'setprop debug.starry.lvmeng_tgkillguard 1'
adb -s f9a116a2 logcat -b all -c
adb -s f9a116a2 logcat -b events -v time > /tmp/lv_events.log &
adb -s f9a116a2 logcat -b main   -v time > /tmp/lv_main.log &
adb -s f9a116a2 shell am start -W -n com.zhuziplay.lvmengnew.op/com.bamboo.game.SplashActivity
# 取证
grep -a 'type=1326' /tmp/lv_events.log            # audit（含 ip=）
grep -a 'tgkill 闸' /tmp/lv_main.log | head        # 闸是否生效
grep -a 'E/CRASH' /tmp/lv_main.log | head -30      # 伪造崩溃的受害者栈
```

| 证据文件（本机 /tmp，未入库） | 行数 / md5 |
|---|---|
| `/tmp/lv_events.log`（events 全量，含 audit + wm 事件） | 108,617 行 / `4623c9158df6220c0ceef815c1b99a05` |
| `/tmp/lv_main.log`（main 全量，含 StarryLvmeng + CRASH） | 138,143 行 / `24311e27bfa015e88ae98f4b40b5531c` |
| `/tmp/lv_maps_31401.txt`（归一用 maps，pid 存活期采） | 4,133 行 / `8090145703579c010439b1ef8c25a388` |

复检入口：`LC_ALL=C grep -a 'type=1326' /tmp/lv_events.log`；包与 so 的 md5 见 §2/§3。
