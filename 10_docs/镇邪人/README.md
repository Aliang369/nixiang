# 镇邪人

| 项 | 值 |
|----|-----|
| 包名 | `com.zxr.dlxmz.zxrwz37zgx10.gg9b` |
| 版本 | 1.0.3 / 11 |
| 引擎 | Cocos2d-js（`libcocos2djs.so`） |
| 保护 | 官方 vc11 已无百度壳/斌哥壳；现存 `libzxprotect` / `libzeus_direct_dex` / 37WAN `sq_plugin`（TapSign） |
| 当前成品 | `08_output/镇邪人/镇邪人_1.0.3_cb9dc_star-releaseSigned.apk` |
| 宣传图 | `12_images/镇邪人/镇邪人_宣传图.png` |
| 构建 | `04_scripts/镇邪人/build_star_origbase_zxr.sh`（官方原版底包全量注入） |
| 验收 | `04_scripts/镇邪人/verify_apk_cb9dc_zxr.sh` |

## 文档

- [官方原版底包 → 星空版复刻 2026-10-24](镇邪人_1.0.3_cb9dc_原版底包复刻_20261024.md) ← **当前**
- [0d1ae 热更重校与菜单换新 2026-10-01](镇邪人_1.0.3_0d1ae_热更重校与菜单换新_20261001.md)
- [e4278 热更修复与 OTA SO 收尾 2026-09-29](镇邪人_1.0.3_e4278_OTA收尾_20260929.md)
- [ca0d4 星空菜单收尾 2026-09-23](镇邪人_1.0.3_ca0d4_星空菜单收尾_20260923.md)
- [菜单功能抓点](镇邪人_菜单功能抓点_20260809.md)（vc10 时代的机制分析，壳/过检部分对 vc11 已不适用）
- [改包初判](镇邪人_改包初判_20260809.md)

## 现状要点（2026-10-24）

- 底包换成**官方原版 vc11**（官方签名、无壳注入），菜单入口靠 Manifest `MenuService` + `classes7.dex`（`JMYBTApplication.attachBaseContext` / `JMYBTActivity` 生命周期）+ `classes8.dex` / `libstarry_magic.so`。
- 免广告靠 `SdkManager.SdkShowAdReward` 前置 `ZxrHooks.shouldSkipAd()`；TapSign 靠 `sq_plugin` 内 `TapSignKit.getSign()` 固定原版证书 MD5 `8fa00ae1…`。
- 战斗四项走 JS 静态补丁：包内钉 `d5507` + 远端当前版本（本轮 `cb9dc`），引擎「包内命中优先」。
- 服务器 `src` 会漂移：**每轮先看设备 `files/gamecaches/cacheList.json` 里的 `remote/src/index.<ver>.jsc`**；新版本若锚点变了，先改 `zxr_patch_spec_*.json`（`node test_zxr_patch.js` 7/7）再出包。
