# 宝石战争 6.0.012 星光包「回合结束卡死」证据（2026-09-16）

## 采集

- 设备：`f9a116a2`（星光包 `cdda0126…` 装机期间，21:53 清 logcat 缓冲后至 22:12）。
- 原始文件：`logcat_full.txt`（72,376 行，全量 logcat）。

## 观察

- `FATAL EXCEPTION` / `ANR in` / `SIGSEGV` / tombstone：**0**。
- 进程始终存活（pid 22320），无 native crash，无 ANR 判定。
- 21:58:49 用户开启「上帝模式」；此后无任何 `Unity` tag 日志，也无托管异常打印。
- 用户操作路径：进关卡 → 自己回合结束后画面停住不再推进。

## 结论（结合静态证据）

卡死不是崩溃或死循环，而是**战斗协程被异常打断**：

- 注入 IL 的 `[DllImport("starry_magic", EntryPoint = "AdjustDamage")]` 等四个入口名与 `libstarry_magic.so` 实际导出符号 `StarryGw_*` 不一致。
- `nm -D --defined-only` 对包内 so 复核：短名导出数量为 0。
- 注入是无条件的：开关全关也会走到桥接调用，因此卡死与三个开关状态无关；三项功能也因此从未真正生效。
- 回合结束时第一次伤害结算走到 `PuzzlePlayer.AdjustHealth`，`dlsym` 失败抛 `EntryPointNotFoundException`，异常被游戏协程吞掉 → 界面停在原地。

## 修复与回归入口

- 修复与门禁：见 `../星光落地_6.0.012_20260916.md` §5.3。
- 修复包：`08_output/宝石战争/宝石战争_6.0.012_surgical-releaseSigned.apk`（md5 `355cfba0…`）。
- 复现验证：`bash 04_scripts/pipeline.sh 宝石战争 reset install && bash 04_scripts/pipeline.sh 宝石战争 next`（卸载安装，需授权）。
