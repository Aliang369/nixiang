# 超维进化 2.1 真机启动证据

2026-09-05，用户明确授权使用已连接真机。唯一在线设备 serial=f9a116a2，型号 OPD2515，Android 16 / API 36，arm64-v8a。仅测试 com.grinning.beta.yszl；没有安装、卸载、清数据、清 logcat、修改设备全局设置或执行领奖/战斗操作。

## 身份与基线

- 当前安装 vc 11 / v2.1；APK SHA-256 `fa9313c98a5be10bfb5af5fbb2825e476334987fbf980d68698c77b0628f99d0`，与本地 surgical-releaseSigned.apk 一致，不是原签名包。
- 安装路径与 hash：[installed_hash.txt](installed_hash.txt)。安装信息：[package_before.txt](package_before.txt)。
- 启动前无目标 PID、stopped=true，前台 Launcher。历史退出信息已有 12:29 时的 signal 9 / exit_self；这不是本轮新发生事件。
- 原包和安装对应修改包的 arm64 libcocos2djs.so 内容一致，SHA-256 `76ff0cf4a272e39edbf4075931f70c576fa367c8f64065943e60db26f62f5282`。

## 本轮操作与结果

用指定 serial 的 adb logcat -b all -v threadtime -T 1 开始采集，再 am start -W 启动 SplashActivity；没有清空历史缓冲。命令输出 [launch.txt](launch.txt) 为 COLD / Status ok（仅表示 Activity 启动命令成功）。

证据：
- [live_logcat.txt](live_logcat.txt)：完整短窗口日志，含系统背景日志，仅用于本地诊断，不外传。
- [target_process_timeline.txt](target_process_timeline.txt)：目标初始化、启动、死亡时间线，保留原日志行号。
- [pid_samples.txt](pid_samples.txt)：主机时间的 PID 采样。注意主机时钟与设备日志时钟有偏差，不能跨时钟直接相减。
- [exit_after.txt](exit_after.txt)：退出原因 reason=2 SIGNALED、status=9、description=exit_self_<pid>_10652。

首进程设备时钟：
- 17:51:47.325 PID 27664 启动 SplashActivity。
- 17:51:52.563 进入 AppActivity。
- 17:51:52.581 libcocos2djs.so 加载成功。
- 17:51:52.582 打印 main / cocos_jni_env_init（原日志8821行）。
- 17:51:52.627 am_proc_died（8849行）。
- 17:51:52.651 系统再次为 top-activity AppActivity 启动 PID 27987；52.778 初始化日志、52.794 死亡。后续重复同一序列。

判定：反复 signal 9 退出与 Activity 重启已实测确认，不再只是历史描述；不是可以仅称为静态白屏/主线程死锁的现象。exit_self 只是系统描述，尚不能当作确切调用栈。日志没有出现目标 FATAL EXCEPTION、Fatal signal 崩溃回溯或 [Cwjh] splice 命中。

星光库加载、MenuService 创建及 cwjh 映射有日志；同时有 grant rejected，不能因此证明六功能工作，也不能把此行直接当进程退出原因。未操作登录/授权。

## 本轮回收

采集后发现持续重启，仅对本轮启动的目标执行 am force-stop com.grinning.beta.yszl，停止循环；主机 logcat 子进程已退出。随后核验无目标 PID、stopped=true，见 [cleanup_verification.txt](cleanup_verification.txt)。没有清除游戏数据或处理其他会话残留；没有使用全局清理脚本。

## 静态定位与边界

离线定位 cocos_jni_env_init 入口 ELF VA 0x717b2c，日志调用0x717b60，后续0x717b88调用0x16d5100；其0x16d52e8同步调用0x16d16ac，后者读取 PackageInfo.signatures 并存在SIGKILL路径。0x16d1b2c调用getpid、0x16d1b30设置signal=9、0x16d1b34调用0x16ebdf8；封装入口将w0清零，0x16ebe08置x8=129，0x16ebe0c svc 0，即AArch64 kill(0,9)。详细条件归因另见后续证据。

### 后续静态条件核验（同轮完成）

完整证据：[native_init_evidence.txt](native_init_evidence.txt)。从 apksigner 输出取得证书 DER，以 zlib.adler32 与独立双累加循环交叉复算：原包 899 字节，Adler32=0xd18450ce；Starry 896 字节，Adler32=0xbbfa5bea。ELF 0x19f33b6 比较表为 0xd18450ce、0x01a520d4、0x685242a4。

比较分派还原后：任一表项命中则 pass=1、返回；三项不命中则 pass=0、进入上述 kill(0,9)。原证书命中，当前 Starry 证书不命中。故签名门禁及当前证书对应的静态退出条件已证实，而非仅见可疑符号。

运行归因：在 JNI 返回实际签名且运行代码未被改写条件下，当前包会在该同步检查退出；与每次初始化后16–45ms死亡及signal 9高度吻合。尚未获得实际syscall PC/信号发送者，最终运行归因仍属于强证据支持的推断，不伪称已取得调用点trace。原包通过该门禁不代表后续检查或全部功能均通过。

本轮诊断完成，未修改任何源码、原包或最终包，未实施门禁绕过或修复，未完成六功能运行验收。
