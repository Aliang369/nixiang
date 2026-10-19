# 绿梦：时空之声 — FairGuard「怎么读签名」机制判定（Stage 5，纯观测）

- 目的：判定 FairGuard 读 APK/签名走哪条通道 —— **libc** / **自有 svc 桩** / **JNI** / **mmap 内存**，以及**何时读**。
  这决定"输入侧伪装"是否可行。
- 纪律：**只 hook libc，完全没有改动 libFairGuard 的代码或页**（改码 ≤0.3 s 会被完整性检测干净退出，Stage 4 已证）。
- 探针：`04_scripts/绿梦/fg_input_probe.js`（v2/v4：open/openat/read/pread64/mmap/fstat + 6 帧回溯；
  v3 用 readlink 解析 fd 路径，**因重入死锁把进程卡住，已废弃**）。
- 载体：设备上现成的 fg_test 包冷启，attach ~4 s，观测到进程退出。

## 1. 结论（一句话）

**FairGuard 走 libc 读 APK**：在进程退出前约 **1.2 s**（上机 ≈29.6 s）从 `libFairGuard.so+0x11b5c4` 发起了一次
**`open(base.apk)`**（libc 导出函数，fd=201）。⇒ **输入侧伪装可行**。

## 2. 实测证据

| 观测 | 结果 |
|---|---|
| 唯一一条 FairGuard 源的 libc 调用 | `open /data/app/~~5uy59…/base.apk fd=201`，`bt=libFairGuard.so+0x11b5c4` |
| 时间 | attach 后 **25.59 s** ＝ 上机 **≈29.6 s**；进程在 26.8 s（上机 30.8 s）`EXIT_SELF(0)` |
| 之后的 `read/pread64`（同 fd） | **未捕获**（1.2 s 窗口内没有对 fd=201 的读）⇒ 取字节要么走 `mmap`，要么发生得更快/更晚 |
| `mmap` 观测 | 该轮未 hook mmap；另一轮（v4）加了 mmap/fstat 但进程在 21 s 被反调试提前杀掉，未捕获到 FairGuard 事件 |
| base.apk 是否在进程内被映射 | **是**：4 段 `r--s` 映射（off 0x7337000 / 0xab3e000，size 0x228000 / 0x17000），但来源是 ART/Unity 的 zip 映射，**未观测到 FairGuard 直接读这些映射内存** |
| 其它 base.apk 读取 | 1342 次 `read`（n=2048）+ 若干 fopen，回溯全是 `libunity.so / libbase.so / libc.so` = **Unity 资源加载**，与签名无关（早于 5 s 完成） |

## 3. 判定表

| 通道 | 是否 | 依据 |
|---|---|---|
| **libc open/read/mmap** | **是（open 已实测）** | `open(base.apk)` 的回溯首帧是 libFairGuard 自己，函数是 libc 导出的 `open` |
| 自有 svc 桩（0x35B100/0x42EB00） | **否**（至少读取不是） | 自有桩直接 `svc #0`，**不会**出现在 libc hook 里；而我们确实抓到了 libc 层的 open |
| JNI getPackageInfo | 未观测到（不排除） | 本次未启用 Java bridge（Stage 2 已证本机不可用），JNI 层未探；但读取已经落在 native libc，JNI 不是唯一通道 |
| mmap 内存 | 未确认 | base.apk 有映射，但那来自 ART/Unity；FairGuard 自己是否 mmap 未能捕获 |

## 4. 对"输入侧伪装"的意义

- **可行**：既然 FairGuard 用 libc `open`（且后续必然是 libc `read`/`pread64`/`mmap`，因为它是同一个 fd），
  我们可以在 `libstarry_magic.so` 的 constructor 里 hook 这几个导出函数：
  - 对 `base.apk` 的 `read/pread64`：按"文件偏移 + 目标签名块区间"重写返回缓冲区里的签名字节（长度不变，不破坏 zip 结构）；
  - 若 FairGuard 用 `mmap`：拦 `mmap`，换成一匿名映射，把文件内容拷进去并就地替换签名块，再返回该地址；
  - 用调用者回溯过滤 `libFairGuard.so`，避免影响 Unity/ART 的正常读（它们读同一个 base.apk）。
- **关键前提**：我们持有原厂包 `/tmp/lvmeng_apk/base.apk`（原厂证书），可离线生成"签名块替换表"（偏移→长度→原始字节）。
- **风险**：签名块在 v2 签名方案下位于 zip 中央目录前，长度可变；替换必须**等长**或同时修正 EOCD/中央目录偏移（复杂度上升）。
  若 FairGuard 只读"签名块区间"，最简单方案是 **open 时返回我们自建的 memfd**（内容=安装包+原厂签名块，等长重建），其余包内容不变。

## 5. 未闭环

1. **open 之后具体用哪个 syscall 取字节**（read/pread64/mmap）未钉死：需再跑一轮带 mmap+fstat 的探针，且要避开反调试
   （v4 那轮 21 s 就被杀；v2 那轮跑到了 30 s）。
2. Java/JNI 通道未探（本机 frida Java bridge 不可用）。
3. 反调试干扰：hook 越多、attach 越久，越容易提前被杀（v3 直接卡死进程，v4 21 s 掉线）。**探针必须保持最小**。
4. 未验证"替换签名块后能否过闸门"——这是下一阶段的实现目标。
