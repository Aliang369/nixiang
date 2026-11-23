# 勇士之心 1.0.6 · 战斗五项锚点修复装机会话（2026-11-23，agent-9）

> 设备 `emulator-5554`（雷电 x86_64 / Android 14 / root）。**未碰 96f0d883；未 screencap、未 input tap、未 git 提交。**

## 1. 结论

| 项 | 结果 | 判据行 |
|---|---|---|
| 新锚点命中 | ✅ | `evalString candidate hit ver=1.0.6 rva=0x31418C` |
| native hook 装上 | ✅ | `hooked se::evalString @0x400020f1418c via=readable` / `SELFCHECK … rd=1 hook=1` |
| splice 命中 | ✅ main + game（两轮冷启一致） | `logic splice #1 … assets/main/index.js` / `#2 … assets/game/index.js` |
| 注入体运行 | ✅ | `[MODMOD] capture installed` / `boot: 118 modules` → `boot: 1236 modules` |
| 自举让位（判据生效） | ✅ | `[MODMOD] native splice active (proof pid ok) -> bootstrap stand-down` |
| 壳看门狗 | ✅ 锚点复核 | `[YsxzSec] state words dirty=0x4000209310cc(tag=1.0.6) failed=0x4000209310d8(tag=1.0.6)` |
| **进程存活** | ❌ **10–13s 被壳 SIGKILL** | `ProxyApplication.f() → Process.killProcess`，见 §4 |
| battleLogic splice | ⏳ 未发生 | 需进战斗（用户侧动作，机器不点屏） |

## 2. 包与 so 指纹

见 `pkg_fingerprint.json`：apk 67,879,269 / md5 `7cca2ceae1b6675bc9680645ab4bb5d4`；arm64 so 295,368 / sha256 `23f94f29…8d83`；
引擎 so（分析基准）35,844,552 / sha256 `bc603df6…b3ba`。同签（CN=StarryMagic，`install -r` 保数据成功，res 945 MB 未被清）。

## 3. 决定性日志（完整见 `decisive_lines.txt` / `decisive_lines_boot2.txt`）

```
23:19:55.111 [Ysxz] readable-map fallback engaged lib=libcocos2djs.so (#1)
23:19:55.112 [Ysxz] evalString candidate hit ver=1.0.6 rva=0x31418C
23:19:55.130 [Ysxz] SELFCHECK evalString va=0x400020f1418c via=readable rd=1 hook=1
23:19:55.131 [Ysxz] hooked se::evalString @0x400020f1418c via=readable
23:20:01.157 [MODMOD] native splice active (proof pid ok) -> bootstrap stand-down
23:20:01.180 [Ysxz] logic splice #1 len 1153939->1249195 file=assets/main/index.js
23:20:01.631 [Ysxz] logic splice #2 len 7148543->7243799 file=assets/game/index.js
23:20:01.220 [MODMOD] boot: 118 modules, toString OK
23:20:01.860 [MODMOD] boot: 1236 modules, toString OK
```

## 4. 阻塞：壳主动 kill（无崩溃栈）

```
23:22:01.781 W System.err: java.lang.Exception: call killProcess callstack! pid=20597
                at android.os.Process.killProcess(Process.java:1318)
                at dev.mars.secure.ProxyApplication.f(EXCEPTIONCLASS:1)
                at dev.mars.secure.ProxyApplication.d(EXCEPTIONCLASS:1)
                at dev.mars.secure.ProxyApplication$d.run(EXCEPTIONCLASS:4)
23:22:01.782 I Process : Sending signal. PID: 20597 SIG: 9
```

- 对照：上一轮同壳、**splice 未命中**的包存活 200s（`../install_20261123/RESULT_boot4.md` 判据①）。
- 缺口：ExitTrace 想钩 `Java_android_os_Process_killProcess__I`，`dlsym` 失败（RegisterNatives）。
- 未做任何绕过（不在本轮授权内），候选排查顺序写在 `10_docs/勇士之心/_work_20261123_锚点全量重定位.md` §6。

## 5. 本目录文件

| 文件 | 内容 |
|---|---|
| `logcat_boot_anchor.txt` | 第 1 轮冷启全量（5,194 行） |
| `logcat_boot2_anchor.txt` | 第 2 轮冷启全量（复现一致性） |
| `decisive_lines.txt` / `decisive_lines_boot2.txt` | 关键词过滤（36 行 / 第 2 轮） |
| `am_start.txt` / `liveness_boot2.txt` | 启动命令、存活探测 |
| `pkg_fingerprint.json` | apk / 三 ABI so / 引擎 so / 签名指纹 |

## 6. 约束遵守

未改 framework 共享头（MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk）；未动 `ysxz_anticheat` 拆层结论；未回退 SecShell / ExitTrace；
未 screencap / input tap；未 git 提交；未操作 `96f0d883`。只改 `04_scripts/勇士之心/ysxz_1.0.3_menu/YsxzMod.cpp`（锚点常量/候选表/日志限速）。
