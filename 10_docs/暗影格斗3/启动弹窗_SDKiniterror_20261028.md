# 暗影格斗3 1.45.3 — 启动弹窗「SDK init error」定位与验签链（2026-10-28）

> 现象（用户报告）：打开游戏弹窗提示，点一下游戏就退出。
> 真机文本（uiautomator 取证，非截图）：标题「提示」/ 正文「SDK init error」/ 按钮「继续」。

## 1. 现象与日志证据

```
I/Unity   : [EBBMADFHNDD] Init
I/Unity   : -->[AccessSDKCallBacks] InitFailedCallBack JNI初始化失败     ← 48ms 后（v6 起变 2.5s）
```
- `JNI初始化失败` 字符串在 `classes3.dex`：`com/hoolai/access/open/fastaccess/channel/ChannelInterfaceProxyDefault$10.InitFailed()`，
  由 `com.access.platform.library`（libaccess.so/libcpp.so）核心回调触发 → 最终 C# 弹「SDK init error」。
- 来源不是我们的菜单/hook：失败发生在我们的 so 加载之前，且干净安装的官方包里不存在该失败。

## 2. 决定性对照实验（隔离变量）

| 组合 | 结果 |
|---|---|
| 官方原包（官方签名 CN=sagely） | `InitSuccessCallBack [AccessCoreSDK.AccessSDKInitData] channel: hoolai; channelId: 10247; gameId: 3015`（~3.2s）→ 正常进游戏 |
| 官方原包**内容零改动、只重签名**（CN=StarryMagic） | 同样弹「提示 / SDK init error」 |
| 我们的星空包（dex 补丁 + 菜单框架 + native hook） | 同样弹窗 |

⇒ 弹窗**只由“签名变了”触发**，与 dex 补丁/菜单框架/hook 无关（对照包已删除，未入库）。

## 3. 验签链（frida 实测 + 静态）

```
Java: ConfigNative.configContext(ctx)
  ├─ ConvertUtil::appSignatures(JNIEnv*, jobject)      ← libaccess.so @0x37c18，读 PackageManager：
  │     getPackageManager → getPackageInfo(pkg,0x40) → signatures[0].toByteArray()
  │     返回「证书 SHA-1，大写，冒号分隔」= 82:1C:92:E4:...:91:7D（我方证书）
  └─ HNDynamic::CongfigSignature(const std::string&)   ← libcore.so @0x4d124
        用 Replace(sig, ":", "") 去掉冒号 → 821C92E4...917D，作为签名配置值上报 hoolai 服务端
C# 侧另有本地常量比对：global-metadata.dat 里唯一一处官方证书 SHA-1
        9078E787FB5246BFD147193343CF7944DADC8FD8（全包仅 1 处命中，大写）
```

### 已试补丁与效果
| 补丁 | 位置 | 效果 |
|---|---|---|
| TapSignKit.getSign 恒返回官方 MD5 | classes5.dex | 对 TapTap 侧有效（未单独验证），对 hoolai 门闩无效 |
| `HNAccessCore::checkCopyright` 静态置 `mov w0,#1; ret` | libcpp.so @file+0x17320 | 无效（真机确认该函数运行时已被 Dobby 钩住，却仍失败） |
| global-metadata.dat 官方 SHA-1 → 星空证书 SHA-1（等长 40 字符替换） | metadata @0x2ca38-ish(off 182840) | **有效**：本地 C# 常量门被破——失败时机从 48ms 变为 2.5s（进入网络阶段） |

⇒ 剩余门闩＝**服务端**校验上报的签名值，客户端开关/本地补丁无法关闭；要让 SDK 取到官方签名才通。

## 4. 结论 / 未决

- 静态改包可破本地门（metadata 常量、TapSignKit、checkCopyright），但破不了服务端比对；
  纯静态要过只能让 `ConvertUtil::appSignatures` 返回官方 SHA-1（需 libaccess.so 代码洞穴/字符串注入的二进制手术）。
- 候选方案（待用户裁决，用户 2026-10-28 表示将提供「斌哥改的包」对照学习）：
  1. mod 内定向 native hook（只替换该 SDK 取签名结果，不恢复 SignSpoof、不动系统 PM）；
  2. libaccess.so 代码洞穴静态补丁；
  3. 回到 v2 的 SignSpoof（2026-09-19 已验证可玩）。

## 5. 本轮产物与工具痕迹

- 管线改动（脚本，均已过 preflight）：
  - `04_scripts/暗影格斗3/surgical_sf3_1.45.3.sh`：新增 `[2.5]` classes4 网页登录补丁、`[2.6]` classes5 TapSignKit 补丁、
    `[2.7]` libcpp.so checkCopyright 静态置 1、`[2.8]` global-metadata 签名常量替换；zip 合并后逐项 sha256 回读断言。
  - `04_scripts/暗影格斗3/sf3_sign.sh`：metadata 断言改为「唯一改动＝签名常量（还原后须等于底包）」，其余字节仍强制零改动。
  - `04_scripts/暗影格斗3/sf3_install.sh`：设备无包且 `SF3_ALLOW_UNINSTALL=1` 时允许全新安装（此前会直接报错退出）。
- 取证材料：`09_test/暗影格斗3/repro_20261028/`（logcat_v4/v5/v6、logcat_official_control、frida probe 脚本）。
- frida 用法：`bash 04_scripts/_common/start_frida_server.sh` → `/opt/homebrew/opt/python@3.11/bin/python3.11 run_probe5.py`（spawn 模式）。
- 官方证书指纹（备查）：SHA-1 `9078E787FB5246BFD147193343CF7944DADC8FD8`、MD5 `c9950f9c3790803c7ccf42470142afe7`、
  SHA-256 `b147f6a485dfaa6dcb9154e5e5190d342264b0568566749df0aa38cfe55e76e3`；
  星空签名证书 SHA-1 `821C92E404AB5FDAC8DBF157C2E467E71440917D`、MD5 `161fca32a36d3df274458682a44cb774`。
