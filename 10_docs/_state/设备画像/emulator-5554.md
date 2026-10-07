# 设备画像 — emulator-5554

- 生成: 2026-10-07 13:32:51（主机 MSYS_NT-10.0-19045）/ 脚本 `04_scripts/_common/device_profile.sh`
- **静态预判: 进程里【没有 libil2cpp.so】⇒ 未进入 IL2CPP 层**（运行时真值仍以三行自检 + 菜单 SELFCHECK 行为准，见文末）
- 复现命令: `bash 04_scripts/_common/device_profile.sh --serial emulator-5554 --pkg com.miHoYo.Nap --rva vicmid=0x147716F0 --rva victail=0x14771758 --rva dmgsettle=0x14785078`

## 1. 设备与系统

| 项 | 值 |
|---|---|
| 型号/厂商 | HBP-AL00 / HUAWEI |
| Android / SDK | 15 / 35 |
| ABI / abilist | x86_64 / x86_64,arm64-v8a,x86 |
| 内核 | 6.1.90-perf+ |
| 页大小 | 4096  |
| ro.kernel.qemu | （空=不是 qemu/模拟器） |
| 物理内存 | MemTotal:        6084732 kB |
| fingerprint | HUAWEI/HapburnP/HapburnP:15/V417IR/1100:user/release-keys |
| 容器/虚拟化线索 | ro.boot.hardware=kona ro.product.board=HBP-AL00 |

## 2. root 与注入条件

| 项 | 值 |
|---|---|
| root 形态 | su -c 不可用 ⇒ /proc/<pid>/mem 与 /data/app 读取会失败 |
| adb 身份 | uid=2000(shell) gid=2000(shell) groups=2000(shell),1004(input),1007(log),1011(adb),1015(sdcard_rw),1028(sdcard_r),1078(ext_data_rw),1079(ext_obb_rw),3001(net_bt_admin),3002(net_bt),3003(inet),3006(net_bw_stats),3009(readproc),3011(uhid),3012(readtracefs) context=u:r:shell:s0 |
| ptrace_scope | （空）（空=内核无 yama/未启用；1/2 会影响 frida 附加） |

## 3. 包与安装形态

| 项 | 值 |
|---|---|
| 包名 / 版本 | com.miHoYo.Nap / 3.2.0 |
| base.apk | /data/app/~~fPsCHPUkLzOuMiQGNAZE0A==/com.miHoYo.Nap-R1DcyEq6QM5-1VFJJ9yt3A==/base.apk |
| 进程 pid | 10542 |
| apk 大小 | 492288387 |
| 已装菜单 so | 该目录 31 个 .so（starry*=1 il2cpp*=1）⇒ **lib 已展开到磁盘**，extractNativeLibs 应为 true |

## 4. libil2cpp 映射分组（★设备差异核心判据）

（进程未运行或没有 libil2cpp 映射，跳过；加 `--launch` 或先手启动游戏）

## 5. 关键 RVA 可达性（内存字节 vs 文件字节）

（未传 --rva，跳过；例：`--rva interp=0x207F798 --rva pushframe=0x207DCCC`）

## 6. 迭代快路（原地改已装 apk）可行性提示

| 项 | 值 |
|---|---|
| ro.apk_verity.mode | （空）（空=该属性不存在；非空即说明参与 APK 校验，原地改包风险↑） |
| 页大小 | 4096 |
| extractNativeLibs | 静态推断=true（lib/arm64 目录已展开到磁盘） |

- **D3（原地改已装 apk 内的 so）判定**：待实测：先备份原包，做一次单条目原地写 + 冷启自检，读通才允许用于自测快路。（另：lib 已展开到磁盘，改 so 其实只需替换 /data/app/.../lib/arm64/ 下的文件，但仍会破坏 apk 签名校验，需实测确认进程是否重校验。）

> 未验证前**禁止**用作交付路径；要启用必须先做「带备份的一次原地写实测」，且**交付永远走同签重打 + 全量安装**。

## 7. 运行时真值怎么拿（本文件只是静态预判）

1. 装机日志三行自检（缺一行 = 没装上，见 `skill/re-implement/hook-rules.md` §30.3）：
   `interp base=0x… entry=0x… rd=1 rva=0x…` / `DobbyHook rc=0 errno=0 orig=0x…` / `armed`
2. 菜单 SELFCHECK 行（§31 规范）：一行给出 base / rd / hook / interp / path / maps 组数；
3. 先跑 `bash 04_scripts/_common/post_install_precheck.sh --game <游戏> --serial emulator-5554`，预检全过才请用户上手。

## 8. 原始证据

```
(pidof com.miHoYo.Nap) = 10542

```
