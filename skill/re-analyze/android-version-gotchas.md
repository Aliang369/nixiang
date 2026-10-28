# Android 版本差异速查（改包/探针/装机踩过的实证差异）

> 路径：`re-analyze/android-version-gotchas.md`
> 用途：同一份改包在 A 机跑通、B 机异常时，先对照本表排除"版本/厂商差异"，别急着改功能代码。
> 数据来源：勇士之心 1.0.4 会话（Android 16 / SDK 36 的 OPD2404 KernelSU root 机 vs Android 14 / SDK 34 的小米 Mi 11 非 root 机）。

---

## 1. 平台版本差异

| 现象 | Android 16（SDK 36，OPD2404） | Android 14（SDK 34，小米） | 应对 |
|------|------------------------------|---------------------------|------|
| 隐藏 API 豁免（历史：`SignSpoof` 的日志，该类 2026-10-28 已删，新包不再出现） | 可豁免 | `E StarrySignSpoof: exemptHiddenApis(meta) failed … Caused by: java.lang.NoSuchMethodException: dalvik.system.VMRuntime.setHiddenApiExemptions [class [Ljava.lang.String;]` | 豁免失败**不一定致命**：本会话签名伪造仍生效（`spoof hit … md5=79a6a62f…` = 官方证书 MD5），壳照常解 payload。别把豁免失败当根因 |
| JNI 退出符号 | 部分可见 | Android 14 起 `Java_java_lang_System_exit__I`、`Runtime_exit__I`、`Runtime_exitInternal__I`、`Java_android_os_Process_killProcess__I` **不在 `libandroid_runtime.so` 导出表** | 探针打印「符号未找到」属正常；要拦退出就下沉到 syscall 层（见 `../re-implement/anti-cheat-pattern.md` 模式五） |
| 进程退出日志 | `Zygote: Process N exited cleanly (0)` + `ActivityManager: … has died: fg TOP` | 同左 | 这两行**不等于崩溃**；有 Java 异常栈才是崩溃，无栈的 clean exit 优先怀疑主动 `exit(0)` |
| 权限弹窗干扰 | 未响应权限弹窗会让进程约 12 s 后 clean exit | 同左 | 判定"主动退出"前先排除弹窗未响应 |

## 2. 厂商（MIUI）差异

| 项 | 实测结论 | 应对 |
|----|----------|------|
| adb 注入点击 | `SecurityException: Injecting input events requires the caller … INJECT_EVENTS` | 交互验证必须请用户手点；不要反复尝试 `adb shell input tap` |
| 读 `/data/app` | 非 root 读不到 | 装机校验改组件指纹：`dumpsys package <pkg>` 里本包独有组件 + `versionName=`/`signatures=`（先例 `04_scripts/勇士之心/install_ysxz.sh --VERIFY_MODE=sig`） |
| `pm grant` | 普通 shell 被拒：`Neither user 2000 nor current process has android.permission.GRANT_RUNTIME_PERMISSIONS` | 有 root 时 `su -c 'pm grant …'`；无 root 走首次启动的系统弹窗 |
| 覆盖安装 | 签名一致可直接 `pm install -r -d` 覆盖；不一致报 `INSTALL_FAILED_UPDATE_INCOMPATIBLE: … signatures do not match newer version` | 换签名的那一次必须卸载重装（破坏性操作需用户授权） |
| 环境检测 | 非 root 机可跑通；**root 机（KernelSU）出现"不安全设备环境"直接闪退** | 明确在范围内才处理；本会话用户明确不处理 |

## 3. 工具/API 差异（易踩）

- Frida 17：`Module.getExportByName` 已移除 ⇒ 用 `Process.findModuleByName('x.so').findExportByName('sym')`。
- `NativeFunction(ptr,'I',[])` 非法（`'I'` 不是合法返回类型）⇒ 写 `'int'`。
- macOS 无 `timeout`；`frida -o logfile` 缓冲日志在 `kill` 时整批丢失。
- baksmali 包装脚本：`bash 01_tools/smali/baksmali list classes <dex>` / `dis <dex> -o <out>`
  （jar 无 `Main-Class`，不能 `java -jar`）。

## 4. 使用方式

改包在某一台机异常、另一台正常时：

```text
□ 两台机的 Android 版本/厂商记录了吗（`adb shell getprop ro.build.version.release`、`ro.product.model`）？
□ 异常是"版本差异"还是"我们的改动"？→ 拿同一 APK 在另一台机复现对照
□ 非 root 机：装机指纹/md5 校验方式换了吗？
□ 探针"符号未找到"是否被误判为探针失败？
□ 主动退出类现象是否先排除了权限弹窗/Activity 未 attach？
```

- 装机脚本双态：同一脚本要能同时支持 root 机（md5 校验、`su -c`）与非 root 机（组件指纹）；
  先例 `04_scripts/勇士之心/install_ysxz.sh`（`SERIAL` 环境变量覆盖 + `VERIFY_MODE=sig`）。
