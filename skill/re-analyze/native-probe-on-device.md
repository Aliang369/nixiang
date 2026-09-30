# 真机探针范式（自家 so 内探针 + logcat）

> 路径：`re-analyze/native-probe-on-device.md`
> 用途：需要在真机上观测「谁调用了某函数 / 某状态何时变化」，但**目标机没有 root**、或观测要跨多轮启动长期驻留时，用自家 mod so 内探针替代 Frida。
> 先例实现：`mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzExitTrace.cpp`（退出探针）、`Games/YsxzSecShell.cpp`（壳自检状态观测）。

---

## 1. 通道取舍（先选通道，别先写代码）

| 通道 | 适用 | 限制 |
|------|------|------|
| **自家 so 探针 + logcat** | 非 root 机、需长期驻留、两机通用、结论要写进文档 | 要重新构建/重签/装机（一轮约几分钟） |
| Frida | 有 root 且能起 `frida-server`、快速试探、断点式改行为 | 非 root 机**根本跑不了**；日志易丢（见 §6） |
| logcat + `dumpsys`/`top` | 已能复现的现象级判定（进程何时死、Activity 栈、生命周期） | 看不到 native 调用者，只能给"是主动退出还是被杀"这类判定 |

判定流程建议：先用 logcat 定性 → 再决定要不要上探针 → 探针优先编进自家 so。

## 2. 探针装配点（越早越好）

- **进程最早点**：`AppComponentFactory.instantiateApplication` 的 `static {}` 里 `loadNativeEarly()`，
  随后 `JNI_OnLoad` 首句启动探针。先例：`java/com/starry/magic/ysxz/YsxzAppComponentFactory.java`
  + `jni/Games/YsxzMod.cpp::Ysxz_OnLoad` 里 `YsxzExitTrace_Start(vm); YsxzSecShell_Start();`。
- **另一起点**：`Application.attachBaseContext`（壳 App 子类，如 `DwdlmApplication` 先例）。
- 探针要能容忍"目标库还没加载"：线程里轮询等待（`YsxzSecShell::WatchThread` 最多等 2 s），
  拿到基址后打印，再进主循环。

## 3. 库基址与符号（加固壳场景）

- **基址**：扫 `/proc/self/maps` 找目标名（`FindSecLibBase()`），**不要**用 `dlopen` 的返回值——
  壳可能自解密后 `android_dlopen_ext` 到私有目录（勇士之心：明文落在
  `/data/data/{pkg}/no_backup/mlib/arm64-v8a/libcocos2djs.so`）。
- **符号**：优先解析 ELF 导出表；被加固/改签名时用**指令模式解码兜底**
  （`YsxzSecShell::DecodeGlobalWord`：先找 `ADRP Xd,#page`，再找同基址寄存器的 `LDR Wt,[Xn,#imm12*4]`，
  从 `ADRP`+`LDR` 对里取出状态字地址）。导出名固定时直接 `+RVA` 用绝对偏移亦可
  （写清"该 RVA 只在对应 so 哈希下有效"）。
- 状态字地址若在 `.data`，**先确认可写**再试着清零（`AddrIsWritable`），不可写就别硬写。

## 4. 输出规范（决定这轮探针值不值）

- **装配自检行**：每个安装点 `hook ok <库>.<符号>` / `hook 失败 <库>.<符号>` / `符号未找到 <库>!<符号>`，
  再加一行「探针已安装」。目的：区分"没装上"和"装上了没命中"（见 `../re-implement/hook-rules.md` §25）。
- **独立计数**：每个调用点单独计数（共用计数器会让首个命中被门限吃掉、栈不打印，掩盖真凶）。
- **栈**：命中时打 Java 栈（`android.util.Log.getStackTraceString(new Exception())`）+ native 栈
  （`_Unwind_Backtrace` 逐帧 `dladdr` 打「库名+0x偏移」），只对前 N 次命中展开，避免刷屏。
- 统一 tag，便于 `adb logcat -s <TAG>` 过滤；输出走 `__android_log_print`。

## 5. 非 root 真机的额外约束（实测）

- 装机校验不能用 md5：非 root 读不到 `/data/app/**`，改用组件指纹
  （`dumpsys package <pkg>` 里只有本包才有的组件）+ 打印 `versionName=`/`signatures=`。
  先例：`04_scripts/勇士之心/install_ysxz.sh` 的 `VERIFY_MODE=sig` 分支。
- **adb 不能注入点击**（MIUI）：`SecurityException: Injecting input events requires the caller … INJECT_EVENTS`
  ⇒ 交互验证必须请用户手点，不要在这上面反复试。
- `pm grant` 从普通 shell 会被拒：`grantRuntimePermission: Neither user 2000 nor current process has
  android.permission.GRANT_RUNTIME_PERMISSIONS`；有 root 时用 `su -c 'pm grant …'`。
- 覆盖安装失败（`INSTALL_FAILED_UPDATE_INCOMPATIBLE`）只在签名不一致时出现；签名一致可直接 `-r` 覆盖。

## 6. 工具环境坑（macOS）

- macOS 无 `timeout`：改用后台 job + `kill`，或用 `gtimeout`。
- `frida … -o logfile & sleep 60; kill` 会把未 flush 的 buffered 输出整批丢掉（`setInterval` 轮询行全丢，
  整轮结论不可用）⇒ 探针结论别建立在 Frida 缓冲日志上。
- Frida 17 起 `Module.getExportByName` 已移除：用
  `Process.findModuleByName('libx.so').findExportByName('sym')`。
- `NativeFunction(p, 'I', [])` 非法：返回类型写 `'int'`，别写 IL2CPP 风格的 `'I'`。

## 7. 收尾

- 探针代码与开关建议留在工程里（下次同类问题直接复用）；长期驻留的看门狗要做成"可判定成功"的日志。
- 真机残留按 TOOLBOOK「真机残留清理」回收；确认没有遗留的探针进程/文件。

## 检查单

```text
□ 已用 logcat/dumpsys 定性（主动退出 vs 被杀）？
□ 通道选择有理由（非 root ⇒ 不搭 Frida）？
□ 探针装配点足够早，且能等目标库加载？
□ 有装配自检行 + 独立计数 + 前 N 次命中展开栈？
□ 结论有可复现日志行（能贴进文档）？
□ 版本差异已对照 android-version-gotchas.md？
```
