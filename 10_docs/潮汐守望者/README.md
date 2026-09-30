# 潮汐守望者（`com.gf.cxswz`）

| 项 | 值 |
|----|-----|
| 包名 | `com.gf.cxswz`（Moonton 潮汐守望者 / TapTap 渠道，QuickSDK 聚合） |
| 真机官方版本 | **1.2.56.375.3 / vc 12563753** |
| 星光 `game_id` | `cxswz`（待正式申请菜单页） |
| 官方底包 | `02_apks/潮汐守望者/潮汐守望者_1.2.56.375.3_taptap_original.apk`（真机拉取，222,834,861 B / md5 `ce54dce5…`） |
| 引擎 | Unity 2019.4.40f1 + IL2CPP + xLua（外层 QuickSDK「MobaGame」7z 自解压壳） |
| 正式包 | `08_output/潮汐守望者/潮汐守望者_1.2.56.375.3_starry-releaseSigned.apk`（md5 `ad83faf6…`） |
| 当前状态 | 已收尾：环境、无敌、伤害倍率（1–10）均完成；root 机 `-37` 封禁绕过仍为 NOT PROVEN |

## 文档

| 文档 | 用途 |
|------|------|
| [改包初判_20260930](./潮汐守望者_改包初判_20260930.md) | **当前主文档**：身份指纹、7z 自解压架构、保护/风控清单、`-37` 登录诊断证据链、环境阶段工作项、NOT PROVEN |
| [cxswz_runtime_menu.json](./cxswz_runtime_menu.json) | 装机门禁用的运行时控件清单；允许 `Toggle_` / `Slider_` 前缀，不用于后台上传 |
| `08_output/潮汐守望者/delivery_20260930/` | 交付副本：icon 512/144、`menu_template.json` |

## 已知要点（细节见主文档）

- APK 内 `libil2cpp.so` 是伪装的 7z 解压库；真代码在首启解压目录 `…/files/dragon2019/assets/`：`comlibs/arm64-v8a/liblogic.bytes`（187 MB 真 IL2CPP）、`UnityData/Managed/Metadata/global-metadata.dat`（22 MB，APK 内为 0 字节占位）、`lua/logic.bin`（42 MB，magic `ZGFS`）。
- 保护：`libgp.so`（GameProtect 风控心跳）、`libqkcheck.so`（QuickSDK 签名校验，含 `deal_check_fail`）、`assets/com.gf.cxswz.cert.pem`；**无 ACE/TSS**。
- 登录报障（用户原文「检测到您的游戏数据异常，您的角色被限制登录」）经实测为**服务端 `-37` 拒绝**，提示文本不在客户端任何文件中。
