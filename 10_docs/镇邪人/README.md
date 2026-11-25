# 镇邪人

| 项 | 值 |
|----|-----|
| 包名 | `com.zxr.dlxmz.zxrwz37zgx10.gg9b` |
| 版本 | 1.0.3 / 11（vc11） |
| 引擎 | Cocos2d-js（`libcocos2djs.so`） |
| 保护 | 官方 vc11 无百度壳/斌哥壳；现存 `libzxprotect` / `libzeus_direct_dex` / 37WAN `sq_plugin`（TapSign） |
| 当前成品 | `08_output/镇邪人/镇邪人_1.0.3_48572_star_20261126-releaseSigned.apk` |
| 底包 | `02_apks/镇邪人/镇邪人_1.0.3_vc11_taptap_原版_20261125.apk` |
| 包内 pin | `d5507` + `48572` |
| 宣传图 | `12_images/镇邪人/镇邪人_宣传图.png` |

## 现行脚本

| 用途 | 入口 |
|---|---|
| 组包（原版底包全量注入 + 双 pin） | `04_scripts/镇邪人/build_star_origbase_zxr.sh` |
| 菜单 dex（javac + R8，Windows 全离线） | `04_scripts/镇邪人/menu_dex_win_zxr.sh` |
| so（ndk-build，Windows 经 cmd.exe + GNUMAKE） | `04_scripts/镇邪人/menu_rebuild_zxr.sh` / `so_only_zxr.sh` |
| 异签名全新安装 | `04_scripts/镇邪人/install_fresh_zxr.sh`（`CONFIRM_UNINSTALL=1`） |
| 静态验收（六组） | `04_scripts/镇邪人/verify_apk_48572_zxr.sh <apk> <底包>` |
| 补丁 spec / 回归 | `zxr_patch_spec_48572.json` + `node test_zxr_patch.js`（7/7） |

## 文档

- [1.0.3(vc11) · 48572 星光复刻 2026-11-26](镇邪人_1.0.3_48572_星光复刻_20261126.md) ← **唯一现行技术文档**
- [硬约束](硬约束.md)（A 长期 / B 本轮一次性）

## 现状要点

- **入口**：Manifest `MenuService` + `classes7.dex`（`JMYBTApplication.attachBaseContext` / `JMYBTActivity` 生命周期）+ `classes8.dex` + `libstarry_magic.so`；保留官方 Application，不换 `StarryApp`。
- **免广告**：`SdkManager.SdkShowAdReward` 前置 `ZxrHooks.shouldSkipAd()`；**TapSign**：`sq_plugin` 内 `TapSignKit.getSign()` 固定原版证书 MD5 `8fa00ae1…`（否则 SDK 报 1001）。
- **战斗四项**：JS 静态补丁 + 包内 pin（`d5507`、`48572`），引擎「包内命中优先」。
- **热更会漂移**：每轮先看设备 `files/gamecaches/cacheList.json` 的 `remote/src/index.<ver>.jsc`；锚点变了就改 `zxr_patch_spec_*.json` 并过 `test_zxr_patch.js` 再出包。
- **兜底**：native「结构派生（`Games/ZxrDerive.h`，不认混淆名）→ 变体表 → 显式告警」，功能日志带 `hot=` 探针（`0` 未介入 / `1` 已挂上 / `2` 锚点全 miss＝战斗静默失效）。
- **菜单 dex 必须现编**：R8 混淆后 smali 无类名可定位，走 `menu_dex_win_zxr.sh`（javac + R8）；`ZxrHooks` 必须 `-keep`（否则按全名调用失败＝功能静默失效）。判据：`shared_prefs/starry_magic_prefs.xml` 里有 `feature_list_sig`。
- **签名/装机**：StarryMagic 证书 `83c5cc0b…` 与官方异签名 ⇒ 必须先卸载再全新安装（须当轮授权）。
