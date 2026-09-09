# 超维进化 2.1 签名检查修正与真机测试

2026-09-05，用户明确要求按已定位原因修正并测试，授权使用设备 f9a116a2。

## 改动与复现

- 脚本：`04_scripts/超维进化/build_signature_test_2_1.py`。
- 输入：`08_output/超维进化/超维进化_2.1_surgical-releaseSigned.apk`，SHA-256 fa9313c98a5be10bfb5af5fbb2825e476334987fbf980d68698c77b0628f99d0。
- 独立输出：`07_patched/超维进化/signature_test_2_1/超维进化_2.1_signature_test.apk`。
- 输出 SHA-256：5c52fc1ce659fff7ddbf3c864c0900d4e4504718ff770ee7c640e3b95884e0d3。
- 仅修改 arm64-v8a/libcocos2djs.so 文件偏移0x19f33b6起4字节，把签名比较表第一项0xd18450ce改为当前Starry证书值0xbbfa5bea。保留另外两项、完整初始化逻辑、线程与退出函数。只覆盖arm64测试，不宣称其他架构修复。
- ELF 输入SHA-256 76ff0cf4a272e39edbf4075931f70c576fa367c8f64065943e60db26f62f5282，输出4b413b4275718227d2d98ebf8cd7c1e781ffe078f0d36d1ec5e4d03667bf78aa。
- 脚本断言源APK/ELF哈希、架构、表原字节；独立输出目录必须不存在，避免覆盖。签名、对齐命令check=True传播失败。没有调用原构建脚本，不更改共用Android.mk、不拾取其他会话native产物。
- 未覆盖02_apks/、08_output/中的任何既有文件。

## G1：范围与静态自检

- 调用契约/副作用：只改比较表数据，没有删初始化调用、改ABI或增加Hook。
- 地址身份：固定APK/ELF哈希、AArch64、文件偏移，修改前12字节表匹配。
- 对象/内存：N/A，无运行时读写对象或指针。
- 重入/并发：N/A，无新增线程或Hook；保留原流程。
- ZIP CRC通过、无条目增删（排除签名目录）、唯一内容变化为目标ELF的4个字节；所有其他DEX/资源/星光库内容相同。
- apksigner验证通过，证书与已安装包一致；zipalign -c -P 16 4通过。
- 可测试，不代表全部功能可交付。恢复可同签名覆盖原星光包，但它有已知启动阻塞，故未自动回退；保留数据并在测试后停止目标进程。

## 安装与运行

设备f9a116a2 / OPD2515 / Android16 / API36。

`adb -s f9a116a2 install -r <测试包>` 返回Success（Incremental Install）。未卸载、未清数据。设备base.apk哈希与输出一致，见 installed_hash.txt。

- logcat.txt、pids.txt、exit_after.txt：约30秒采样内主进程PID14618持续存在；最终活动核验仍为同一PID。
- 18:05:18.248 cocos_jni_env_init后继续进入JS引擎，不再在该点反复SIGKILL。
- 18:05:18.774 Cocos Creator v2.4.15。
- 18:05:19 起游戏自行下载资源更新：0.1.86 -> 0.2.83，约11.5MB；未手动改动该缓存。此后功能挂点不能直接沿用旧资源版本作有效性保证。
- 18:05:22 WebSocket连接、资源初始化、获取服务器列表成功；18:05:22.677进入LoginMgr，18:05:32.693出现MS_LOGIN / taptap / noCache=true。未代用户完成账号登录。
- 退出信息没有本轮新SIGKILL事件；其顶部仍是上一轮17:52的历史记录。
- 当前活动为AppActivity；没有用截图确认界面，因此仅称日志进入登录流程，不宣称已登录或进入战斗。

## 暴露的后续缺口（本轮不扩大修复）

- 星光侧日志持续“等待游戏库…(il2cpp=0 cocos=0 cocosjs=0)”，而游戏的Cocos库已加载且JS运行；没有[Cwjh] early hook/splice命中。功能注入NOT PROVEN，需单独核对库识别/加载定位与0.2.83资源。
- 出现grant rejected；未绕过菜单授权、未操作账号或服务端配置。
- 没有开启六功能、领奖或提交战斗结算；六功能均NOT PROVEN。
- JS日志有videoplayer脚本缺失、首次缓存JSON读取失败，但随后继续初始化至登录；本轮不把非致命日志当新修复目标。

## G2/G3结论

| CLAIM | VERDICT | EVIDENCE |
|---|---|---|
| 当前arm64样本的初始化SIGKILL循环已解除 | PROVEN（本轮启动窗口） | 4字节单点改动前后对照、PID稳定、进入JS及登录、无新增异常退出 |
| 该签名检查是先前启动阻塞原因 | 强因果对照支持 | 仅改变其比较项后越过故障点 |
| 完成账号登录及战斗 | NOT PROVEN | 需要用户完成登录并确认可进入战斗 |
| 六功能注入、生效与关闭回归 | NOT PROVEN | 缺hook/splice命中，尚未操作开关 |
| 正式可交付 | NOT PROVEN | 仅为arm64独立启动测试包，非全功能验收包 |

## 收尾

最终采集活动状态后，am force-stop仅停止本次测试的超维进化；cleanup.txt确认无目标PID、stopped=true。主机logcat进程已回收。没有清数据、清日志、卸载或影响其他游戏。测试包保留安装，游戏自行下载的更新缓存保留；旧APK产物未改。
