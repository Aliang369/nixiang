# 无限升级 13.101 脱壳工具（2026-11-18）

纯只读 `/proc/<pid>/mem` 扫描/守候，**不注入、不改进程**。用于从易盾壳进程里取出运行时解密后的真 dex。

## 文件
| 文件 | 用途 |
|---|---|
| `wxsj_dex_scan2.c` | 立即扫描：遍历可读段搜 dex 魔数（`dex\n0`），读 `header+0x20 file_size`，打印 `addr/file_size/version` |
| `wxsj_dex_watch.c` | 守候模式：轮询到出现目标 size 的 dex 立即落盘（抢在壳释放前） |
| `wxsj_memstr.c` | 内存字符串扫描（定位 `HybridCLR`/`LoadMetadataForAOTAssembly` 等痕迹，判断执行引擎） |
| `run_watch.sh` | 守候模式驱动脚本（读 maps → 调 watch → 落盘） |

## 用法（真机已 root）
```sh
# 0) 交叉编译（NDK 27；动态链接，静态链接会因 Android 15 TLS 对齐检查被拒）
$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android24-clang -O2 -o wxsj_dex_scan2 wxsj_dex_scan2.c

# 1) 推上去
adb -s <serial> push wxsj_dex_scan2 /data/local/tmp/ && adb -s <serial> shell su -c 'chmod 755 /data/local/tmp/wxsj_dex_scan2'

# 2) 扫（PID = pidof com.xingzuodaoshi.infinityUpgrade）
adb -s <serial> shell su -c '/data/local/tmp/wxsj_dex_scan2 <PID>'

# 3) 按打印出的 addr/file_size 用 dd 取出
adb -s <serial> shell su -c 'dd if=/proc/<PID>/mem bs=4096 iflag=skip_bytes,count_bytes skip=<addr> count=<file_size> status=none' > dex_N.dex
```

## 关键经验
- **必须动态链接**：静态链接产物在本机（Android 15/16）启动即报 `executable's TLS segment is underaligned … needs 64 for ARM64 Bionic`。
- **frida 走不通**：13.101 反调试会让 `attach`/`spawn` 都报 `agent connection closed unexpectedly`；`/proc` 直读绕开它。
- **dex 不落盘**：私有目录只有 `Anonymous-DexFile@*.vdex`（92 B 空 header），`base.vdex` 只覆盖壳 dex；只能内存取。
- 取到的 dex 要校验 `adler32` + `baksmali d` 能反编译，才算完整（本次 4 个全过、6,910 类/52,674 方法）。