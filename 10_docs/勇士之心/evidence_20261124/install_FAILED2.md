# FAILED2 — 壳净化修复轮（2026-11-23 22:30，子代理 agent-7）

> 结论：**壳净化（SecShell 看门狗）修复成功，NPE 闪退链已断**；但进程仍在启动 11~14s 后
> `Zygote: Process N exited cleanly (0)` 干净退出（无异常栈、无 FATAL），JS 注入因此没跑到。
> 退出属**另一子系统**（壳的保护线程主动 `_exit(0)`，1.0.4 线已有定位记录），不在本轮改范围内 ⇒ 停手交回。

## 1. 本轮改了什么（唯一改动文件）

`04_scripts/勇士之心/ysxz_build_so_103.sh`，两个 hunk（diff 已核，`preflight.sh` PASS）：
1. FW 覆盖清单 L120-124 增 `Games/YsxzSecShell.cpp Games/YsxzSecShell.h`；
2. mk 注入 python 段（原只补 NoAceStubs）改为循环补 `Games/NoAceStubs.cpp` + `Games/YsxzSecShell.cpp`（幂等，插在 `include $(BUILD_SHARED_LIBRARY)` 前）。
去加固宏（只 `-DSG_AUTH_V2_ONLY`）、体积门禁、菜单门禁、pepper 门禁全部未动。

为什么这就能修：基线 81c70279 的 jni 树里没有 `Games/YsxzSecShell.*`，而 `MENU_DIR/YsxzMod.cpp` 用
`__has_include("Games/YsxzSecShell.h")` 条件包含 ⇒ 头不在 ⇒ `YsxzSecShell_Start()` 整段被编译掉 ⇒ so 里没有
`Java_com_starry_magic_ysxz_YsxzAppComponentFactory_nativePrepareSecureShell` 导出。
注册方式＝**按 JNI 名直接导出**（`extern "C" JNIEXPORT void JNICALL Java_..._nativePrepareSecureShell`），
不走 JNI_OnLoad / RegisterNatives；依赖只有 `Games/YsxzSecShell.h`（同目录）与 `Includes/Logger.h`（基线树自带）。
1.0.6 libsecret RVA 候选 `SecShell_IsDirty 0x54130 / FailedMask 0x541c8` 已在候选表内。

## 2. 新 so（三 ABI，均落 `mod菜单/starry-mod-menu/libs_ysxz`）

| ABI | size | sha256 | 导出符号 | nativePrepareSecureShell / SecShell_IsDirty / FailedMask / [YsxzSec] / __starry_ac / sg-grant-v1 |
|---|---|---|---|---|
| arm64-v8a | 269,728 | 5925bc8f…407c5 | `FUNC GLOBAL DEFAULT…nativePrepareSecureShell` | 1 / 1 / 1 / 11 / 0 / 0 |
| x86 | 301,524 | 1d5a0e79…84cf6 | 同上 | 1 / 1 / 1 / 11 / 0 / 0 |
| armeabi-v7a | 241,456 | f6ca36a0…fc854 | 同上 | 1 / 1 / 1 / 11 / 0 / 0 |

菜单：`kYsxz_Features` 13 串三 ABI 全在（基础功能/伤害倍率1:50/秒杀/无敌/无限技能/移动速度1:10/战斗加速1:5/自动战斗/解锁金卡/自动化/自动重复挑战/自动出售/自动分解）；
`自动穿戴` 三 ABI **0 命中**。

## 3. 新包

`08_output/勇士之心/勇士之心_1.0.6_surgical-releaseSigned.apk` — size 67,858,789 B / md5 `a7536e526a167804e73874db4c5a4fb4` /
sha256 `7b1d9b0ade8e55e369b2fcef4cac412d7e3fd53489eb5342781deaaa8550c3ac`（CN=StarryMagic，so_md5=44e0b7fc…c687d88；
含 classes.dex + classes2.dex 646,132 + classes3.dex 13,988(killer) + 三 ABI so）。旧包已改名 `…-releaseSigned-v1.apk` 留证。

## 4. 装机判据（emulator-5554，覆盖安装 install -r 成功；两轮冷启 `logcat_boot2.txt` / `logcat_boot3.txt` 一致）

| 判据 | 结果 | 原始行 |
|---|---|---|
| ① SecShell watchdog started | **命中**（2 次/轮） | `22:22:22.587 I YsxzAppFactory: SecShell watchdog started before application=dev.mars.secure.ProxyApplication`；`22:22:23.138 … WarriorApplication` |
| ② 无 UnsatisfiedLinkError | **命中**（仅剩与壳无关的历史噪声 `libluster.so not found`） | `22:22:23.108 W StarryInitProvider: UnsatisfiedLinkError: dlopen failed: library "libluster.so" not found` |
| ③ 无 AppInfo.parameters NPE / FATAL | **命中**（全 720KB 日志 0 行） | `grep -c 'AppInfo.parameters\|FATAL EXCEPTION' → 0` |
| ④ libcocos2djs.so 已加载 | **命中** | `22:22:23.941 I Mod_Menu: [Ysxz] readable-map fallback engaged lib=libcocos2djs.so` |
| ⑤ splice #1/#2 或 [MODMOD] | **未命中** | `22:22:33.908 W Mod_Menu: [Ysxz] evalString not found (dynsym + rva both miss, rva=0x3138E4)`；`grep -c MODMOD → 0`（`星空落地_1.0.6_20261024.md:76` 已记录 1.0.6 原生 splice 不命中、走 JS 自举路线） |
| ⑥ MenuService native menu shown | **命中** | `22:22:24.248 I MenuService: native menu shown mode=ACTIVITY`；`22:22:24.308 I MenuService: overlayVisible=true attached=true` |
| ⑦ 菜单 12 项（无自动穿戴） | **命中** | `feature #1..#8, #10..#12=OFF`（#9=Category_自动化 占位，12 号即末位，无 13 号=自动穿戴） |
| 看门狗内部（附加证据） | 成功 | `22:22:22.616 I Mod_Menu: [YsxzSec] state words dirty=0x4000209310cc(tag=1.0.6) failed=0x4000209310d8(tag=1.0.6)`；`normalized failedMask=0x8 / 0x200` |

## 5. 剩下卡在哪（未修，交回 lead 决策）

```
22:22:33.937 I WindowManager: WIN DEATH: Window{… com.fightingstars.games.warrior/org.cocos2dx.javascript.AppActivity}
22:22:33.938 I Zygote: Process 7561 exited cleanly (0)
22:22:33.947 I ActivityManager: Process com.fightingstars.games.warrior (pid 7561) has died: fg TOP
22:25:41.908 I Zygote: Process 7777 exited cleanly (0)     # 第二轮同一现象（启动后 14.3s）
```

- 无 Java 栈、无 native 崩溃、无 ANR：`exit(0)`，与 `星空落地_1.0.4_20260930.md` §4 记录同源
  （「保护线程主动 `_exit(0)`」，该轮的解法是 `Games/YsxzExitTrace.cpp` 的 seccomp 只拦 `exit_group(94)` + 只结束该线程）。
- 本轮**没有**加 `Games/YsxzExitTrace.cpp`：它是退出探针（钩 libc exit/_exit/abort/kill/pthread_exit + seccomp），
  属另一个子系统的行为改动，超出「壳净化最小改动」授权；且 `Android_ysxz.mk:59-60` 把它标为「需定位」的探针。
- 建议下一步（任选其一，均需 lead 点头）：A）只加探针出诊断包，先拿到调用点；B）直接移植 1.0.4 §4 的 seccomp 方案
  （只拦 94 → TRAP → 处理器只 `syscall(__NR_exit,0)` 结束当前线程）。

## 6. 复现命令（本机 Git Bash 缺 md5/rsync，用垫片；已放在 /tmp/ysxzbins，不入仓）

```bash
export PATH="/tmp/ysxzbins:$PATH"     # md5（→md5sum）、rsync（python 最小实现）
ANDROID_NDK_HOME=/c/Users/Administrator/AppData/Local/Android/Sdk/ndk/27.0.12077973 \
YSXZ_ABIS="arm64-v8a x86 armeabi-v7a" YSXZ_PRISTINE=.ysxz_pristine/main.js \
  bash 04_scripts/勇士之心/ysxz_build_so_103.sh
ORIG_APK=02_apks/勇士之心_1.0.6_official.apk ANDROID_HOME=/c/Users/Administrator/AppData/Local/Android/Sdk \
  bash mod菜单/starry-mod-menu/scripts/surgical_ysxz_1.0.6.sh
VERSION=1.0.6 bash 04_scripts/勇士之心/ysxz_sign.sh          # 末尾 mac stat 误报可忽略
adb -s emulator-5554 install -r --no-incremental 08_output/勇士之心/勇士之心_1.0.6_surgical-releaseSigned.apk
```
`.ysxz_pristine/main.js` = 官方包 `assets/main.js`（1,685 B / sha256 `f2311201…401af`）。
