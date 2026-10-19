# 绿梦：时空之声 — FairGuard 运行期自 patch 方案（Stage 4：实现 / 出包 / L2）

- 决策背景：方案 2 定为「运行期自 patch」——用注入的 native 壳，在 FairGuard 解包后、校验前，把解包镜像里的 exit_group 执行点改成空操作。
- 交付形态不变（硬约束 #11：标准 so 直装包）。
- 本轮**未达成 L2 判据**（进程未稳定撑过 35 s）。本文是完整实测记录与结论。

## 1. 实现（已落码、已出包、已实测）

改动文件（只动 per-game 私有文件，未碰 Main.cpp / GameConfig.h / 公共 Android.mk / SignSpoof.java / 菜单层）：
- `mod菜单/starry-mod-menu/app/src/main/jni/Games/LvmengMod.cpp`
  - 新增 `namespace fgpatch`：解析 `/proc/self/maps` 找 `libFairGuard.so` 的 **可执行且可写**段（rwxp）；
  - 在段内按字节模式 `c8 0b 80 d2 01 00 00 d4`（`mov x8,#0x5e ; svc #0`）**动态定位** exit_group 桩（不硬编码 VA，抗 ASLR）；
  - 命中后把首字改成 `c0 03 5f d6`（`ret`），`__builtin___clear_cache` 刷 I-cache；
  - 50 ms 轮询；干跑阶段只定位不写；到 `kApplyAtMs` 才写；之后每 200 ms 复查、被覆盖则重打；
  - 只写该 rwxp 段，不碰其它 so / 段；
  - 日志：logcat `StarryLvmeng`、`/data/data/com.zhuziplay.lvmengnew.op/files/starry_loader.log`、`.../starry_fgpatch.txt`。
  - 启动点：`__attribute__((constructor))` + `Lvmeng_TryInit`（`pthread_once` 幂等）。

> **偏离说明（有意为之）**：任务书要求把状态写进 `starry_state.txt`；该文件是框架 StateMirror 的 8 整数通道，
> 绿梦 C# 读的是 `starry_lvmeng_state.txt`（已登记的互覆坑）。为避免破坏共享通道，patch 状态改写
> `starry_fgpatch.txt` + `starry_loader.log`，**未写 `starry_state.txt`**。

产物：
- so：`mod菜单/starry-mod-menu/libs_lvmeng/arm64-v8a/libstarry_magic.so`，md5 `cdec726eaa80ee2979bedd430fe313e0`（2114864 B）
- APK：`08_output/绿梦/绿梦_1.0.5_starry-releaseSigned.apk`，重签 CN=StarryMagic（与设备原 fg_test 同签名，**覆盖安装、未清数据**）

## 2. L2 实测（两次时间窗对照）

| 实验 | 何时写字节 | patch 命中 | 进程死亡 | 相差 | 退出原因 |
|---|---|---|---|---|---|
| A（第一版） | 启动后 ~4.0 s | `off=0x35be00 before=d2800bc8 after=d65f03c0` | 4.5 s | **+0.3 s** | `reason=1 (EXIT_SELF) status=0` |
| B（时间窗版） | ~22.0 s（此前只干跑） | 同上（先 `found(dry-run)`，再 `hit(re-patch)`） | 25.0 s | **+0.3 s** | `reason=1 (EXIT_SELF) status=0` |

原始证据（`starry_loader.log` / logcat）：

```
[fgpatch] found(dry-run) off=0x35be00 va=0x733fec8e00
[fgpatch] hit(re-patch) off=0x35be00 va=0x733fec8e00 before=d2800bc8 after=d65f03c0
[fgpatch] PHASE1 done at 22000 ms hits=2 base=0x733fb6d000 size=0x413000
01:12:08.311 I/StarryLvmeng: hit(re-patch) ...
01:12:08.625 … ApplicationExitInfo reason=1 (EXIT_SELF) status=0
```

- 两次都**没有** tombstone、没有 native crash、没有信号 —— 是**进程自己 exit(0)**。
- 主界面证据（实验 A 期间存活段）：`MainFragmentActivity` 已获得焦点、SurfaceView 有帧、菜单 `native menu shown mode=ACTIVITY` 已弹出。
- 结论：**只要写了 FairGuard 的代码字节，0.3 s 内就被"干净退出"**，与写入时刻无关（4 s 或 22 s 都一样）。

## 3. 结论与判读

1. **运行期改 FairGuard 代码的路线，被其完整性监控即时反制**：任何一字节改动 → 0.3 s 内 `EXIT_SELF status=0`。
   这同时解释了 Stage 2 的"hook 零命中"：真正执行退出的路径**不经过**我们 hook/patch 的那个桩（否则桩被改成 `ret` 后就不会退出了）。
2. **"30 s 签名闸门"与"改码检测"是两条独立路径**：Stage 2 里无 frida、不改码时是 30 s 退出；本轮改码后提前到 0.3 s 退出。
3. 因此在 FairGuard 代码上做任何 patch（桩 ret / 8 个调用点 NOP / 判定分支恒真）都会先撞上"改码检测"，
   **在解决检测之前，这条路线不可行**；而"先 patch 检测器"本身也会被检测（自举悖论）。
4. 本轮**未达 L2 判据**（撑过 35 s + 主界面），设备当前安装的是本实验包（会以 `EXIT_SELF(0)` 提前退出）。

## 4. 建议的下一步（供决策官）

**不要继续改 FairGuard 的代码**。改走"输入侧伪装"——让 FairGuard **读到**它期望的数据，而不动它的代码：

- 我们在 `libstarry_magic.so` 里 hook `libc openat/read/mmap/pread64`：
  当 FairGuard 打开 `/data/app/.../base.apk`（或读签名块 `APK Sig Block 42` / `META-INF`）时，
  把读取结果里的签名块替换成**原厂签名**（我们有 `/tmp/lvmeng_apk/base.apk` 的原厂签名块字节）。
- 优点：完全不触碰 FairGuard 的代码/页，不触发完整性检测；改的是 **libc 层面的返回数据**。
- 风险：FairGuard 可能用 `syscall` 直调（绕过 libc）；需要先用 PLT/GOT hook 覆盖 libc 导出并观察是否命中——
  可复用 Stage 2 的 frida 观测（把 frida 换成我们自己的 so 内 hook，反调试风险更低）。
- 备选：回到"签名伪装"路线但在 native 层做（Stage 0 已排除 Java 层 SignSpoof）。

## 5. 未闭环

1. 改码检测的触发条件与频率未刻画（只知道 ≤0.3 s 响应）；它的退出路径仍未命名（Stage 2/3 均未命中）。
2. 未验证"输入侧伪装"是否命中 FairGuard 的读取路径（需要新一轮 hook 观测）。
3. 本轮实验包会提前退出，**不宜作为交付物**；如需恢复设备基线，可覆盖安装 `07_patched/绿梦/fg_test/绿梦_fgtest_resign.apk`（同签名）。
4. `starry_fgpatch.txt` 在进程提前退出时来不及写（只在阶段二结束后写），后续应改为命中即写。
