# RESULT — 干净退出修复（B 方案）装机会话（2026-11-23 22:41，agent-7）

> 结论：**壳净化 + 干净退出双修成功，进程存活**。`_exit(0)` 的决定性调用者已锁定：`libsecret.so+0x4551c`
> （Thread-39），被拦截后只结束该线程，进程继续跑；热更下载完成（res 37,697 B → 945,535,508 B）。
> 仍停：JS 自举未生效（`evalString` 1.0.3 RVA 在 1.0.6 不命中），游戏停在 `com.joyo.sdk.JoyoSdkActivity`。

## 1. 本轮改动
- `04_scripts/勇士之心/ysxz_build_so_103.sh`：FW 覆盖清单 + mk 注入循环加 `Games/YsxzExitTrace.cpp Games/YsxzExitTrace.h`（幂等，与 SecShell 同一段）。
- `mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzExitTrace.cpp`：+10 行 `#if !defined(__aarch64__) → 同签名 no-op / #else / #endif`（该文件用 `uc_mcontext.pc/regs[30]/AUDIT_ARCH_AARCH64`，x86 与 v7a 编不过；模拟器 primaryCpuAbi=arm64-v8a，实跑 arm64 那份）。
- 探针无 RVA/符号候选需求（Dobby hook libc 导出 + `dladdr(lr)` 判 `libsecret.so` + seccomp 只拦 `exit_group(94)`），故无需加 1.0.6 候选表。

## 2. so 与新包

| 项 | 值 |
|---|---|
| arm64 so | 292,504 B / sha256 `90a436be…a54dd`（APK 内 `lib/arm64-v8a/libstarry_magic.so` 与该文件逐字节一致） |
| x86 so | 301,572 B / `8876b62e…`（ExitTrace no-op，`[YsxzExit]` 计数 0；SecShell 正常） |
| v7a so | 241,456 B / `7b20d981…`（同上） |
| 断言 | arm64：`nativePrepareSecureShell=1`、`[YsxzSec]=11`、`[YsxzExit]=27`、`内核级退出陷阱已安装=1`、`退出探针已安装=1`；三 ABI：`__starry_ac=0`、`sg-grant-v1=0`、菜单 13 串全在、`自动穿戴=0` |
| 新包 | `08_output/勇士之心/勇士之心_1.0.6_surgical-releaseSigned.apk` 67,866,981 B / md5 `3f5485dd6c4327f2730972b2166c1907` / sha256 `97e30ca1…6a02d`（CN=StarryMagic） |

## 3. 判据（logcat_boot4.txt，22:36:21 → 22:39:47；pid 8054）

| 判据 | 结果 | 原文 |
|---|---|---|
| ① 存活 ≥120s（同 pid） | **命中** | `ps -o ETIME -p 8054` → `04:45`；两次 `pidof` 均 `8054`（`liveness4.txt`） |
| ② 无 `exited cleanly (0)` | **命中** | `grep -c 'exited cleanly' → 0`；`grep -c 'has died' → 0` |
| ③ 无 FATAL/NPE | **命中** | `grep -c 'FATAL EXCEPTION\|AppInfo.parameters' → 0` |
| ④ 热更 | **命中** | `22:36:46.893 E Bingo.BingoActivity: 下载完成`；`files/res` 37,697 B → **945,535,508 B** |
| ⑤ JS 自举 | **未命中** | `MODMOD=0`、`[AUTO]=0`、`__starry_speed=0`；`[Ysxz] evalString not found (dynsym + rva both miss, rva=0x3138E4)`；`StarryMagic: [ysxz] 等待初始化…(102/120)` |
| ⑥ 菜单仍显示 | **命中** | `22:37:02.495 I MenuService: overlayVisible=true attached=true`；`resume delayed 1200 → show ACTIVITY overlay host=JoyoSdkActivity alive=true` |

## 4. 决定性退出行（libc 钩子与 syscall 钩子命中同一调用点）

```
22:36:36.091 Mod_Menu: [YsxzExit] _exit 被调用 code=0 (#1)
22:36:36.095   #0 libstarry_magic.so+0x3c65c
               #1 libstarry_magic.so+0x3c3d4
               #2 libstarry_magic.so+0x3ae80
               #3 libsecret.so+0x4551c (pc=0x40002090551c)     <-- 调用者
22:36:36.096 Mod_Menu: [YsxzExit] 壳调用 _exit(0) → 拦截：只结束该线程 tid=8079(Thread-39)，进程存活
22:36:36.096 Mod_Menu: [YsxzExit] syscall(nr=93 code=0) 直调退出 (#2)
               #3 libsecret.so+0x4551c                          <-- 同一调用点
```

拦截只发生 1 次（`grep -c '被调用 code=0' → 1`）；`exit(93)` 未被拦（只有 94 → TRAP，处理器内 `syscall(__NR_exit,0)` 只结束该线程）。

## 5. readable-map fallback 刷屏实测（本轮按要求未改）

- boot2（进程只活 11s）：`readable-map fallback engaged` **407 行 / 10s ≈ 40 行/秒**（每 ~24ms 一次）。
- boot4（进程活 200s）：**2,535 行 / 21,863 行 = 11.6%**；稳态每 ~2s 一次（与 `evalString` 重试同节拍）。
- 副作用实证：boot4 的 logcat 环形缓冲被冲掉，**22:36:21–22:36:35 早期行（含 `[YsxzSec] state words`、`[YsxzExit] 退出探针已安装`）已丢失**，故 ②③ 用残留范围 + 进程存活反证。
- 是否拖慢：每 2s 一次 `fopen(/proc/self/maps)` 解析属低开销；40 行/秒时的主要风险是**日志噪声吞掉证据**，不是 CPU。

## 6. 剩余问题（下一步候选，未做）
1. **JS 自举未生效**：`kEvalRva=0x3138E4` 是 1.0.3 的值，1.0.6 的 cocos so 已重编（见 `星空落地_1.0.6_20261024.md`「原生 splice 路线在 1.0.6 不命中」）⇒ 需在解密后 `no_backup/mlib/arm64-v8a/libcocos2djs.so`（35,844,552 B）上重新定位 `se::evalString` 并更新 `kEvalRva/kEvalInsn`。
2. 游戏当前停在 `com.joyo.sdk.JoyoSdkActivity`（SDK 登录/公告）；需人点按进游戏后 `libcocos2djs.so` 才会 load、`等待初始化` 才结束（禁截图/点按 ⇒ 交用户）。
3. `readable-map fallback engaged` 建议限速（首 3 行 + 每 200 次一行），可显著减少缓冲冲洗。
