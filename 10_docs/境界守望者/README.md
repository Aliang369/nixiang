# 境界守望者（`com.ap.watcherofrealms`）

| 项 | 值 |
|----|-----|
| 包名 | `com.ap.watcherofrealms`（Moonton Watcher of Realms / Google Play 国际服） |
| 真机官方版本 | **1.6.38.829.1 / vc 16388291** |
| 星光 `game_id` | `wor`（native 已回灌，运行时 `ModBridge: auth game_id synced from native: wor`；后台菜单页仍待用 [wor_menu_template.json](./wor_menu_template.json) 申请） |
| 官方底包 | `02_apks/境界守望者/境界守望者_1.6.38.829.1_global_original.apk`（真机拉取，171,550,562 B / md5 `ac171aa3…`）+ `…split_config.arm64_v8a.apk`（33,397,726 B） |
| 真 IL2CPP 库 | `06_dumped/境界守望者/base_1.6.38.829.1/liblogic.bytes`（137,648,336 B / md5 `9c600b9c…`） |
| 引擎 | Unity 2019.4.40f1 + IL2CPP + xLua（外层 MobaGame 7z 自解压壳，与国服同架构） |
| 正式包 | **单 APK 无 root 直装包**：`08_output/境界守望者/境界守望者_1.6.38.829.1_starry-single.apk`（206,070,583 B / md5 `fd554c37718424fdc5735859f5534eb6` / sha256 `6774aeb08c98e3a7d2661db993e560583ddf5477c17ddb5802ea032a33d2ef29`；内含 so `fef9cb7ee510d65a1448b9169748ac33`，tag `wor-e2-20261004b`，C2 正式档 RELEASE + C1 热重载） |
| 当前状态 | ✅ **e1 环境阶段 + e2 功能阶段 + C1 设备侧配置化（含热重载） + C2 诊断分档 全部完成并真机验收通过**；正式包已合并为单 APK，真机 `adb install -r -d` 直装 Success |

## 文档

| 文档 | 用途 |
|------|------|
| [改包初判_20261003](./境界守望者_改包初判_20261003.md) | 身份指纹、与国服差异一字表、保护/风控清单、登录链路分析、NOT PROVEN、下一步 |
| [星光改造_环境阶段_20261003](./境界守望者_星光改造_环境阶段_20261003.md) | e1：底包拆解、去签名、菜单进游戏 |
| [星光改造_功能阶段_20261003](./境界守望者_星光改造_功能阶段_20261003.md) | e2：无敌 / 伤害倍率（含 §10 真机验收与三个卡点定论） |
| [正式包_单APK合并_20261003](./正式包_单APK合并_20261003.md) | **交付形态**：Split → 单 APK 无 root 直装（6 步合并流程 + 真机直装验证） |
| [C1设备侧配置化与C2诊断分档_20261004](./境界守望者_C1设备侧配置化与C2诊断分档_20261004.md) | **调参/排障**：设备侧 `wor_cfg.txt` 全键表与生效顺序；`WOR_DIAG_FULL` 两档构建、构建期自证、体积差；2026-10-04 构建链 junction 根因 |
| [C1热重载_20261004](./境界守望者_C1热重载_20261004.md) | **不重启调参**：`wor_cfg.txt` 热重载实现（mtime+size 轮询，约 2 s 生效）、键的生效时机三分类、日志锚点、真机 `RELOAD-PASS` 与 e2 无回归证据 |
| [wor_menu_template.json](./wor_menu_template.json) | 后台菜单模板（`template_id=wor`，features = 无敌 / 伤害倍率） |
| [_facts.md](./_facts.md) | **事实台账**（给下一个会话用）：产物指纹、设备/包信息、三卡点定论、15 个字段偏移、符号事实、真机验收实证、已知限制、路径速查、待验证 |
| 10_docs/_state/games/境界守望者.md | 游戏状态摘要 |
| 10_docs/_state/current.md | 跨游戏状态索引行（`境界守望者` 行） |

## 与国服（潮汐守望者）的关键差异

| 维度 | 国服 | 国际服 |
|------|------|--------|
| 包名 | `com.gf.cxswz` | `com.ap.watcherofrealms` |
| 渠道 | QuickSDK + TapTap | **Google Play** 直连 |
| 登录 | TapTap→QuickSDK→loginServer | MufSDK→Google/FB/VK→loginServer |
| APK 形态 | 单体 APK | 官方原包为 **Split APK**（base + split arm64，Qigsaw）；**交付形态已合并为单 APK 无 root 直装**（见上「正式包」） |
| 签名校验 | libqkcheck.so | 无；可能由 libmoba.so/hades 替代 |
| 风控 | libgp.so | **libhades.so** + libshadowhook.so |