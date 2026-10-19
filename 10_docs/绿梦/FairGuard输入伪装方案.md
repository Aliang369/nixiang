# 绿梦：时空之声 — FairGuard 输入侧伪装方案（Stage 6：实现 / 出包 / L2）

- 思路（决策官定）：FairGuard 做的是「签名者身份比对」，只要让它读到的 base.apk 的签名块是**原厂**的即可。
- 纪律：**不碰 FairGuard 任何代码/页**（Stage 4 已证改码 ≤0.3 s 被反制）；只 hook libc（Stage 2 已证 libc hook 不触发 0.3 s 检测）。

## 1. 实现

改动文件（全部 per-game 私有）：
- `mod菜单/starry-mod-menu/app/src/main/jni/Games/LvmengMod.cpp`：新增 namespace fgspoof
  - constructor 起线程：从 /proc/self/maps 取 base.apk 路径 → 从**包内 STORED 资产** assets/lvmeng/fg_orig.apk
    用最小 zip 解析（EOCD→中央目录→本地头→按数据偏移 pread）抽到
    /data/data/<pkg>/files/fg_orig.apk（幂等：已存在即跳过）；
  - Dobby inline hook open / openat：path 含 base.apk **且** __builtin_return_address(0) 落在
    libFairGuard 的 maps 区间内 → 返回 open(fg_orig.apk) 的 fd；其余一律原样转发（Unity/ART 的 1342 次读不受影响）；
  - libFairGuard 比本壳晚加载，故**先装 hook、再轮询 maps 补范围**（范围未知期间 ra_in_fg() 恒 false ⇒ 等价直通）；
  - 诊断钩子（只记不改）：read/pread64/mmap/fstat/readlink，仅当调用者回址在 libFairGuard 内才落日志。
- `mod菜单/starry-mod-menu/scripts/surgical_lvmeng_1.0.5.sh`：把 /tmp/lvmeng_apk/base.apk 作为 **ZIP_STORED**
  资产 assets/lvmeng/fg_orig.apk 打进包，并把该条目加入包内闭环校验清单。

出包：APK 由 191,725,207 B → **371384030 B**（+179,649,718 B，STORED 不压缩）；签名仍 CN=StarryMagic，覆盖安装、未清数据。

## 2. L2 实测（两轮，均未过 35 s）

| 轮次 | 结果 |
|---|---|
| v1（fg range 只在启动时找一次） | 抽取成功但 libFairGuard range not found; spoof disabled → 伪装未生效，进程照旧 ~30 s 退出 |
| v2（先装 hook、轮询补范围） | **伪装命中**，但进程仍在 **~31 s** EXIT_SELF(0) |

v2 关键日志（starry_loader.log + logcat 时间轴）：

    01:29:18.389 hooks installed (open=0x7572eca000 openat=0x7572eca020)
    01:29:22.637 fg range 0x733c188000..0x733c5cd000 (t=4100 ms)
    01:29:49.138 spoof HIT #1 open(.../base.apk) ra=0x733c2a35c4 -> fd=238
    01:29:49.770 Process ... has died   (EXIT_SELF, status=0)   ← 命中后 0.63 s

- ra=0x733c2a35c4 与 Stage 5 的 libFairGuard.so+0x11b5c4 **完全对上** ⇒ 拦的正是签名那次 open。
- 命中前 0.02 s 内出现 ro.secure / ro.debuggable / adb_enabled 探测，随后退出。

## 3. 诊断轮的硬证据（决定下一步）

加了 read/pread64/mmap/fstat/readlink 诊断钩子（只观测）后重跑：

    spoof HIT #1 open(.../base.apk) ra=0x7342fc55c4 -> fd=174
    FG readlink /proc/self/fd/174 => .../adv685985f2.dat   ← readlink 钩子工作正常（证明钩子都装上了）
    （其后：没有任何 FG read / FG pread / FG mmap / FG fstat）

**结论：FairGuard 拿到我们替换的 fd 之后，没有通过 libc 读它**（readlink 钩子能出日志，说明钩子有效）。
即：它要么用**自带的 svc 桩**按真实路径重开/直读（libc 层看不到），要么识别了 fd 替换。

## 4. 对"输入伪装可行性"的修正

| 通道 | Stage 5 判断 | Stage 6 实测 |
|---|---|---|
| libc open | 是 | **是**（拦到了，ra 精确匹配） |
| libc read/pread/mmap | 疑似 | **未观测到任何 FairGuard 源的读** |
| 自带 svc 读 | 未排除 | **更可疑**：换 fd 后它不再用 libc 读，说明读取很可能走自有 svc |

⇒ **只在 open 层替换 fd 不足以过闸门**。若读取确实走自带 svc 桩，则输入伪装不可行
（该桩就在 libFairGuard 代码里，改它会被 ≤0.3 s 完整性检测反制——Stage 4 已证）。

## 5. 下一步可选（供决策官）

1. **低成本判定**：把 open 的替换改成"返回真实 fd，但在 read/pread 返回缓冲里改写签名块字节"（保持 inode/路径不变）。
   若仍退 ⇒ 基本可断定它不走 libc 读；若过了 ⇒ 说明它只是识别了 fd 替换（好办）。
   前提是能记录到它读的偏移区间（若走 libc 就能看到）。
2. **反证法**：给一个未重签的原厂 APK 打同一壳（若能装）看是否 30 s 退出——预期不退出，可证明是签名问题
   （受"官方包无法重签安装"限制，代价高）。
3. 若确认走自有 svc：输入伪装路线终止；且 SignSpoof 已在 Java 层伪造 PackageManager（Stage 4 日志可见 spoof hit），
   仍被杀 ⇒ 它不认 PackageManager。

## 5.5 Stage 7 判定（H1 vs H2）—— 结论：**H2**

改造：**open 不再替换 fd**（真实 base.apk，路径/inode 不变），只保留诊断钩子并只记 FairGuard 源。单次冷启实测：

    01:36:06      am start
    01:36:26.067  FG readlink /proc/self/fd/192 => .../files/adv685985f2.dat  ra=0x733ea92498
    01:36:38.577  FG open(real) #1 .../base.apk  ra=0x733ea2a5c4     ← fg+0x11b5c4，与 Stage 5 一致
    01:36:38.586  FG readlink /proc/self/fd/192 => ...adv685985f2.dat  ra=0x733ea92498
    01:36:39.174  Process ... has died (EXIT_SELF, status=0)          ← open 后 0.60 s

- open 之后 **0 条 FairGuard 源 libc read / pread64 / mmap / fstat**；只有同一 fd(192) 的 readlink，
  而 readlink 的 ra 落在 libFairGuard 内 ⇒ **钩子确实生效**（不是没装上）。
- 存活时长：**≈33 s**（01:36:06 → 01:36:39.174），与基线 ~30 s 同量级。

**判定 = H2**：FairGuard 那次 open 之后并**没有**用 libc 读数据 ⇒ 读取极可能走**自带的 svc 桩**（libc 层不可见）。
按决策官判据，**输入伪装路线到此终止**（除非把判定再收紧到 readv/preadv/__read_chk 等变体，见未闭环）。

## 6. 代价与未闭环

- **assets 体积代价**：+179,649,718 B（完整原厂 base.apk，ZIP_STORED）⇒ APK **371384030 B**（原 191,725,207 B）；
  首次启动抽取约 1–2 s、占 /data 179 MB（幂等，已存在则跳过）。瘦身版（只嵌签名块+CD+EOCD 重建）未做。
- 未闭环：① 它到底用哪个 syscall 读那个 fd（libc 侧零证据，疑似自有 svc）；② 是否识别 fd 替换；
  ③ 环境探测（ro.secure/ro.debuggable/adb_enabled）与退出是否同一条链；④ 菜单/5 功能未测（进程没活到那步）。
- 设备现状：安装的是本轮实验包（会 ~31 s EXIT_SELF(0)），回基线可覆盖装
  07_patched/绿梦/fg_test/绿梦_fgtest_resign.apk（同签名、不清数据）。
