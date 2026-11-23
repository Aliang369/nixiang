# FAILED — 勇士之心 1.0.6 星光包装机验证（2026-11-23）

> 结论：**装机成功、killer 生效、壳解密成功；但游戏在 `AppActivity.onCreate` 立刻 Java 崩溃（2/2 复现），预检 FAIL_A。**
> 按要求：未重装、未改包、未清数据；诊断只做一轮，然后停。

## 1. 决定性崩溃行（两轮完全相同）

第 1 轮（am start 冷启，pid 7067 / 22:03:13）：

```
11-23 22:03:13.471  7067  7067 E AndroidRuntime: FATAL EXCEPTION: main
11-23 22:03:13.471  7067  7067 E AndroidRuntime: Process: com.fightingstars.games.warrior, PID: 7067
11-23 22:03:13.471  7067  7067 E AndroidRuntime: java.lang.RuntimeException: Unable to start activity ComponentInfo{com.fightingstars.games.warrior/org.cocos2dx.javascript.AppActivity}: java.lang.NullPointerException: Attempt to read from field 'java.util.HashMap com.bingo.sdklibs.utils.AppInfo.parameters' on a null object reference in method 'java.lang.Object com.bingo.sdklibs.utils.PluginHelper.getMetaValue(java.lang.String)'
11-23 22:03:13.471  7067  7067 E AndroidRuntime:   at com.bingo.sdklibs.utils.PluginHelper.getMetaValue(PluginHelper.java:77)
11-23 22:03:13.471  7067  7067 E AndroidRuntime:   at com.bingo.sdklibs.utils.PluginHelper.getNumberParameter(PluginHelper.java:152)
11-23 22:03:13.471  7067  7067 E AndroidRuntime:   at com.bingo.androidlibs.NativeInfo.init(NativeInfo.java:92)
11-23 22:03:13.471  7067  7067 E AndroidRuntime:   at com.bingo.sdklibs.utils.PluginHelper.initWithMainActivity(PluginHelper.java:46)
11-23 22:03:13.471  7067  7067 E AndroidRuntime:   at com.bingo.cocos.bridge.BingoActivity.onCreate(BingoActivity.java:80)
11-23 22:03:13.471  7067  7067 E AndroidRuntime:   at org.cocos2dx.javascript.AppActivity.onCreate(AppActivity.java:54)
```

第 2 轮（预检脚本冷启，pid 7435 / 22:09:08）：同一栈、同一 NPE（见 `logcat_boot2_precheck.txt`）。

## 2. 根因（一轮诊断，机器证据）

**SecShell 看门狗没被编进 `libstarry_magic.so` ⇒ `dirty` 位没被清 ⇒ 壳跳过真实 `Application.onCreate` ⇒ bingo SDK 未初始化 ⇒ 上述 NPE。**

| # | 证据 | 原文 / 事实 |
|---|---|---|
| F1 | 运行期 JNI 符号缺失（两轮各 2 次，共 4 次） | `E s.games.warrior: No implementation found for void com.starry.magic.ysxz.YsxzAppComponentFactory.nativePrepareSecureShell()` → `E YsxzAppFactory: SecShell watchdog start failed` |
| F2 | 全量日志**没有任何** `[YsxzSec]` 行 | 连兜底入口 `Ysxz_OnLoad` 也没跑到 ⇒ 不是"启动了但没生效"，而是**代码根本不在 so 里** |
| F3 | 包内 so 无该符号、无该字符串 | 抽出 `lib/arm64-v8a/libstarry_magic.so`（263,640 B / md5 `4a2d9460fc949d978d9eb788abb58b57`，与 `.ysxz_103_build/libs/arm64-v8a` 同一份）：搜索 `nativePrepareSecureShell` / `[YsxzSec]` / `sec shell watchdog started` 全部 **0 命中**；`JNI_OnLoad` 命中（说明 so 是星光的，只是没这段） |
| F4 | 源码里确实有 | `mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzSecShell.cpp:350`：`extern "C" JNIEXPORT void JNICALL Java_com_starry_magic_ysxz_YsxzAppComponentFactory_nativePrepareSecureShell(...)`；`Android_ysxz.mk:58` 也写了 `LOCAL_SRC_FILES += Games/YsxzSecShell.cpp` |
| F5 | 但出包脚本不用工作区那份 mk 的这份清单 | `04_scripts/勇士之心/ysxz_build_so_103.sh`：覆盖清单（L120-128）**没有** `Games/YsxzSecShell.cpp`；mk 注入（L133-164）只补 `Games/NoAceStubs.cpp`。真正编译的是 `$JNIW`（从 git 基线 `YSXZ_103_BASE` 导出）的 mk，工作区那份不参与（脚本 L129-132 自己写明"改工作区那份没用"） |
| F6 | 历史同因 | `10_docs/勇士之心/星空落地_1.0.6_20261024.md` §2：该 NPE 的机理就是"壳自检不通过 ⇒ dirty=1 ⇒ ~649ms 不给真实 Application 调 onCreate ⇒ bingo SDK 未初始化"；`YsxzSecShell.cpp:1-22` 文件头同款记录（Frida 每 1ms 清 dirty/failedMask 即恢复） |
| F7 | 与 killer / 解密**无关** | killer 已挂（`SignatureKiller mounted` / `signature killer installed (bin.mt.signature.KillerApplication193)`）；明文引擎已落盘（`no_backup/mlib/arm64-v8a/libcocos2djs.so` = 35,844,552 B）；日志无 `key unwrap failed`、无 `refuse empty mlib`；崩溃发生在 cocos JNI 初始化（`JniHelper::setJavaVM` / `cocos_jni_env_init`）**之后** |

## 3. 修法方向（交构建方，本轮未改）

在 `ysxz_build_so_103.sh` 的框架层覆盖清单里补上 `Games/YsxzSecShell.cpp`（以及它需要的 `Games/YsxzSecShell.h`），并在 mk 注入段像 `NoAceStubs.cpp` 那样补 `LOCAL_SRC_FILES += Games/YsxzSecShell.cpp`；出包门禁加一条机器断言：so 里必须能搜到 `nativePrepareSecureShell` / `[YsxzSec]`，否则拒绝出包。

## 4. 我已停手的边界

未重装、未改包、未清数据、未 screencap、未 input tap、未 git 提交。
