# 装机验证报告 — 勇士之心 1.0.6 星光包（雷电 emulator-5554，2026-11-23）

> 子代理：装机验证。授权：用户当轮「授权可以直接卸载」。设备：`emulator-5554`（x86_64 + Houdini / Android 14 / 已 adb root）。**未碰 96f0d883 真机。**

## 0. 一句话结论（判据见 §4）

| 项 | 结果 |
|---|---|
| 装机 | ✅ 成功（1 次成功，0 次重试；base.apk md5 与产物一致） |
| killer | ✅ 生效（`SignatureKiller mounted` + `KillerApplication193` installed） |
| 壳解密 | ✅ 成功（明文引擎 35,844,552 B 落盘，无 key unwrap / 无 refuse empty mlib） |
| splice | ❌ 未命中（evalString drift，且**这版走的本来就是 JS 自举路线**；JS 也没跑到） |
| 菜单 | ✅ 弹出（`native menu shown mode=ACTIVITY`，1..12 号开关被读取） |
| 预检 | ❌ **FAIL_A**（启动即崩，2/2 复现），详见 `FAILED.md` |
| 是否在等热更 | ❌ 不是。崩溃在 `AppActivity.onCreate`，**从未进入 JS/热更阶段**（`files/res` 不存在） |

**根因不是 killer、不是壳解密**：SecShell 看门狗（清 `dirty` 位）没被编进 `libstarry_magic.so` ⇒ 壳跳过真实 `Application.onCreate` ⇒ bingo SDK 未初始化 ⇒ `AppInfo.parameters` NPE。证据链见 `FAILED.md` §2（F1–F7）。

## 1. 包指纹

| 项 | 值 |
|---|---|
| 文件 | `08_output/勇士之心/勇士之心_1.0.6_surgical-releaseSigned.apk` |
| size | 67,850,597 B |
| md5 | `6bd8e97e39d1b15549867461e3c31d42` |
| sha256 | `099ffd0cbd739643853924f1956999eb7a43fd48c0357ec97fe61b6e7618aa91` |
| mtime | 2026-11-23 22:01:04 (+08) |
| 包内 so（arm64） | `libstarry_magic.so` 263,640 B / md5 `4a2d9460fc949d978d9eb788abb58b57`（与 `.ysxz_103_build/libs/arm64-v8a` 同一份） |

## 2. 设备状态

| 项 | 值 |
|---|---|
| serial | `emulator-5554`（另一台 `96f0d883` 全程未操作） |
| ABI | `x86_64,arm64-v8a,x86,armeabi-v7a,armeabi`（Houdini 转译 arm64） |
| Android | 14（sdk 34 侧行为，`ro.build.version.release=14`） |
| 应用 primaryCpuAbi | `arm64-v8a` |

## 3. 装机事实（覆盖安装 → 卸载 → 重装）

1. 装前状态：官方 1.0.6 在装（`versionName=1.0.6 / versionCode=6`，`lastUpdateTime=2026-11-23 21:25:03`）。记录 → `pre_install_state.txt`。
2. 卸载（用户授权）：`adb -s emulator-5554 uninstall com.fightingstars.games.warrior` → `Success`；`pm path` 空（确认已清）。记录 → `uninstall.txt`。
3. 安装：`adb -s emulator-5554 install -r <包>` → 第一次尝试成功（增量安装被系统拒 → 自动回落 streamed install → `Success`；非失败重试）。记录 → `install_attempt1.txt`。
4. 装后事实：
   - `pm path` = `/data/app/~~ou84DdYVvuV2zJYh-38qkQ==/com.fightingstars.games.warrior-UO-wLI2TziLpl2yVzDYvIw==/base.apk`
   - `versionName=1.0.6` / `versionCode=6` / `lastUpdateTime=2026-11-23 22:02:43`
   - `signatures=PackageSignatures{34c6a98 version:3, signatures:[6f4dda0b]}`
   - 拉回 base.apk：67,850,597 B / md5 `6bd8e97e39d1b15549867461e3c31d42` / sha256 `099ffd0c…8aa91` ⇒ **与产物逐字节一致**
   - 全量 dumpsys → `post_install_dumpsys.txt`；launcher activity → `launcher_activity.txt`（`dev.mars.secure.StubActivity`）

## 4. 六条判据（逐条给命中/未命中 + 原始行）

| # | 判据 | 结果 | 原始行 |
|---|---|---|---|
| ① | StarryMagic so 加载 | ✅ 命中 | `I StarryNativeLoad: loaded apk lib-dir so: …/lib/arm64/libstarry_magic.so`；`I ModBridge: native via StarryNativeLoad: …`；`I StarryMagic: [桥接] RegisterNatives 19/19 全部成功`；`I StarryMagic: 星光魔法 MOD 菜单已加载 (MainModern tag=ysxz)` |
| ② | SignatureKiller mounted / bin.mt.signature | ✅ 命中 | `I StarryAppFactory: SignatureKiller mounted: PackageInfo.CREATOR replaced, PM/Parcel caches cleared`；`I StarrySigKiller: signature killer installed (bin.mt.signature.KillerApplication193)` |
| ③ | libsecret 解密成功 / mlib 明文 so ~35.8MB | ✅ 命中 | `D nativeloader: Configuring clns-9 … library_path=/data/user/0/com.fightingstars.games.warrior/no_backup/mlib/arm64-v8a:…`；实测 `no_backup/mlib/arm64-v8a/libcocos2djs.so` = **35,844,552 B**；`D JniHelper: JniHelper::setJavaVM(…)`、`D main: cocos_jni_env_init`。**无** `key unwrap failed`、**无** `refuse empty mlib`（关键词全量 0 命中） |
| ④ | logic splice #1/#2 | ❌ 未命中（本版预期不命中） | `W Mod_Menu: [Ysxz] evalString prologue drift got=0x97FFE631 want=0xD10543FF (#1, 本版走 JS 自举路线)`；`W Mod_Menu: [Ysxz] evalString not found (dynsym + rva both miss, rva=0x3138E4)`；`I Mod_Menu: [Ysxz] note: JNI evalString bridge present (不用作 splice 点)`。与 `星空落地_1.0.6_20261024.md` §7-1 记录一致（1.0.6 引擎重编，原生 splice 不命中，改走 JS 自举） |
| ⑤ | MenuService / native menu shown | ✅ 命中 | `I MenuService: auth game_id=yszzx`；`I MenuService: native menu shown mode=ACTIVITY`；`I MenuService: overlayVisible=true attached=true`；`I MenuService: activity-resumed:SplashActivity → show ACTIVITY overlay host=SplashActivity alive=true`；随后 `[开关] #1..#12 = OFF` 逐项读取（未见 #9/#13 行） |
| ⑥ | 无 Fatal / SIGSEGV | ❌ 未命中 | `E AndroidRuntime: FATAL EXCEPTION: main`（NPE，见 §5）。**无 SIGSEGV、无 native crash、无 tombstone** |
| ⑦ | （附加）SecShell 看门狗 | ❌ 未命中（=本次失败根因） | `E s.games.warrior: No implementation found for void com.starry.magic.ysxz.YsxzAppComponentFactory.nativePrepareSecureShell()` → `E YsxzAppFactory: SecShell watchdog start failed`（两轮进程共 4 次）；全量日志无任何 `[YsxzSec]` 行 |

## 5. 崩溃（决定性行，完整栈见 `FAILED.md` / `logcat_boot1.txt`）

```
22:03:13.471 (pid 7067)  FATAL EXCEPTION: main
RuntimeException: Unable to start activity …/org.cocos2dx.javascript.AppActivity:
  NullPointerException: Attempt to read from field 'java.util.HashMap com.bingo.sdklibs.utils.AppInfo.parameters'
  on a null object reference in method 'com.bingo.sdklibs.utils.PluginHelper.getMetaValue'
  at PluginHelper.getMetaValue(PluginHelper.java:77) ← getNumberParameter(152) ← NativeInfo.init(92)
  ← PluginHelper.initWithMainActivity(46) ← BingoActivity.onCreate(80) ← AppActivity.onCreate(54)
```

冷启动第 1 轮：21:03:04 起进程，22:03:13.471 崩（存活 ~9s）。第 2 轮（预检脚本自己的冷启）：22:09:08.736 崩，栈完全相同 ⇒ **确定性复现**。

## 6. 预检结论

`bash 04_scripts/_common/post_install_precheck.sh --game 勇士之心 --serial emulator-5554 --idle-min 1`

- 输出目录：`09_test/勇士之心/precheck_20261123_220906`（我的副本：`logcat_boot2_precheck.txt`、`precheck_output.txt`）
- 包名来源：`yszzx_menu_template.json → com.fightingstars.games.warrior`
- 结果：`✓ 包已安装` → `✓ 菜单 so 加载标记` → `✗ 崩溃标记 FATAL EXCEPTION` → **`判定段=A 依据=运行中崩溃`，退出码 1 = FAIL_A**
- 类型：**不是 FAIL_B（装了没命中）**，是上游没发生——进程在 Activity 创建阶段就死了，so 加载与菜单弹出都在崩溃之前完成。

## 7. 失败根因（一轮诊断，不改包不重装）

**SecShell 看门狗没编进 `libstarry_magic.so`。** 依据 7 条全部落在 `FAILED.md` §2：
源码 `Games/YsxzSecShell.cpp:350` 定义该 JNI 函数、`Android_ysxz.mk:58` 也列了源文件，但**真正参与编译的 mk 是 `ysxz_build_so_103.sh` 从 git 基线导出并注入的那份**，注入清单（L120-128 / L133-164）里只有 `NoAceStubs.cpp`，没有 `YsxzSecShell.cpp`；包内 so 里 `nativePrepareSecureShell` / `[YsxzSec]` 字符串 0 命中（而 `JNI_OnLoad` 在，证明 so 本体正确）。后果链＝`YsxzAppComponentFactory` 最早入口抛 `UnsatisfiedLinkError` → 看门狗不启动 → 壳 `dirty` 位不清 → 真 `Application.onCreate` 被跳过 → bingo SDK 未初始化 → `AppInfo.parameters` NPE（与 10-24 文档 §2 记录的官方包 root 机崩溃同一机理）。

## 8. 下一步建议

1. **构建方**（下一轮，不是本轮）：把 `Games/YsxzSecShell.cpp`（＋`YsxzSecShell.h`）加进 `ysxz_build_so_103.sh` 的覆盖清单与 mk 注入；出包门禁加断言"so 内必须能搜到 `nativePrepareSecureShell`"，否则拒绝出包。
2. 重出包后只需**同签覆盖安装**（不用再卸载）：`install -r` ＋ 再跑一次本流程（判据 ⑦ 必须先命中，再看 ④⑤）。
3. 热更 res（1.1.22.0 / ~0.9GB）在本次完全没有开始（`files/res` 不存在）；下一轮启动成功后 90s 仍可能停在下载界面，属预期，判据 ④⑤ 到时看 JS 是否加载。
4. 不建议在修复前重装/清数据——崩溃是包内 so 缺符号，清数据不影响结论。

## 9. 约束遵守记录

- 未 screencap、未 `input tap`、未手动 monkey（唯一 monkey 来自预检脚本自身实现）。
- 装机 1 次（第 1 次即 `Success`），无重试，总计 2 次安装动作内。
- 未改包、未重签、未改任何构建脚本、未 git 提交。
- 未操作 `96f0d883`；所有 adb 命令均带 `-s emulator-5554`。
- 卸载为当轮授权动作（用户原话「授权可以直接卸载」）。

## 10. 本目录文件

| 文件 | 内容 |
|---|---|
| `pkg_fingerprint.json` | 包 size/md5/sha256 |
| `pre_install_state.txt` / `uninstall.txt` | 装前状态、卸载结果 |
| `install_attempt1.txt` | 安装输出（含增量→流式回落） |
| `post_install_dumpsys.txt` / `launcher_activity.txt` / `abi.txt` | 装后事实 |
| `pulled_base.apk` | 设备侧 base.apk（md5 校验用，67,850,597 B） |
| `logcat_boot1.txt` / `logcat_boot1_filtered.txt` | 第 1 轮冷启全量 / 关键词过滤（398 行命中共） |
| `logcat_boot2_precheck.txt` / `precheck_output.txt` | 第 2 轮（预检冷启）日志与预检输出 |
| `apk_so_probe/` | 从包内抽出的 arm64/x86 `libstarry_magic.so`（符号取证） |
| `FAILED.md` | 决定性日志行 + 根因证据链 |
| `REPORT.md` | 本文件 |
