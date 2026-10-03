# 境界守望者（`com.ap.watcherofrealms`）

| 项 | 值 |
|----|-----|
| 包名 | `com.ap.watcherofrealms`（Moonton Watcher of Realms / Google Play 国际服） |
| 真机官方版本 | **1.6.38.829.1 / vc 16388291** |
| 星光 `game_id` | `wor`（待正式申请菜单页） |
| 官方底包 | `02_apks/境界守望者/境界守望者_1.6.38.829.1_global_original.apk`（真机拉取，171,550,562 B / md5 `ac171aa3…`）+ `…split_config.arm64_v8a.apk`（33,397,726 B） |
| 真 IL2CPP 库 | `06_dumped/境界守望者/base_1.6.38.829.1/liblogic.bytes`（137,648,336 B / md5 `9c600b9c…`） |
| 引擎 | Unity 2019.4.40f1 + IL2CPP + xLua（外层 MobaGame 7z 自解压壳，与国服同架构） |
| 正式包 | 待出 |
| 当前状态 | ⚠️ **环境阶段启动中**：底包/指纹/架构已识别，待用户官包登录确认后出星光环境包 |

## 文档

| 文档 | 用途 |
|------|------|
| [改包初判_20261003](./境界守望者_改包初判_20261003.md) | **当前主文档**：身份指纹、与国服差异一字表、保护/风控清单、登录链路分析、NOT PROVEN、下一步 |
| 10_docs/_state/games/境界守望者.md | 游戏状态摘要 |

## 与国服（潮汐守望者）的关键差异

| 维度 | 国服 | 国际服 |
|------|------|--------|
| 包名 | `com.gf.cxswz` | `com.ap.watcherofrealms` |
| 渠道 | QuickSDK + TapTap | **Google Play** 直连 |
| 登录 | TapTap→QuickSDK→loginServer | MufSDK→Google/FB/VK→loginServer |
| APK 形态 | 单体 APK | **Split APK**（base + split arm64，Qigsaw） |
| 签名校验 | libqkcheck.so | 无；可能由 libmoba.so/hades 替代 |
| 风控 | libgp.so | **libhades.so** + libshadowhook.so |