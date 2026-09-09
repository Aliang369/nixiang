# 青鸾繁华录（com.hdg4.g4qlfh）— 项目索引

> 游戏更正：正式名《青鸾繁华录》（用户曾写作"青鸢繁华录"）。Cocos Creator 2.x jsb，在线服务端权威（pomelo），战斗模拟器在客户端。
>
> game_id=`qlfh`（用户拍板保留，勿再改动；新游戏命名规则见 `.cursor/rules/starry-mod-game-id.mdc`）。

## 文件地图

| 产物 | 路径 |
|---|---|
| 原始 APK（9.8.146 from_device） | `02_apks/青鸾繁华录/青鸾繁华录_9.8.146_from_device.apk` |
| **正式星空包（v2，装机版）** | `08_output/青鸾繁华录/青鸾繁华录_9.8.146_surgical-releaseSigned.apk` |
| 账号凭据备份（jsb.sqlite，2026-08-29 卸载前） | `07_patched/青鸾繁华录/backup_20260829/` |
| 二进制 Manifest 缓存（surgical 脚本复用） | `07_patched/青鸾繁华录/surgical_9.8.146/AndroidManifest.xml` |
| 初判（引擎/保护/存档/账号） | `10_docs/青鸾繁华录/青鸾繁华录_改包初判_20260829.md` |
| **抓点定稿 v2（锚点表+战斗结论+实测）** | `10_docs/青鸾繁华录/抓点定稿_20260829.md` |
| **后台菜单模板 JSON** | `10_docs/青鸾繁华录/qlfh_menu_template.json`（template_id=`qlfh`） |
| 宣传图 | `12_images/青鸾繁华录/青鸾繁华录_宣传图.png` |
| 功能代码 | `mod菜单/starry-mod-menu/app/src/main/jni/Games/QlfhMod.{h,cpp}` + `java/com/starry/magic/cocos/QlfhHooks.java` |
| 构建脚本 | `mod菜单/starry-mod-menu/scripts/surgical_qlfh_9.8.146.sh`（首包）/ `qlfh_so_only.sh`（只换 so） |

## 菜单功能（v2）

| featNum | 功能 | 状态 |
|---|---|---|
| 1 | 免广告（直领+改本地+秒过三合一） | 拼接验证 ✅，奖励到账待用户实测 |
| 2 | 百倍伤害（客户端战斗模拟器） | 拼接验证 ✅ |
| 3 | 无敌（含反伤/自伤兜底） | 拼接验证 ✅ |
| 4 | 游戏加速 1-10 | 用户已确认有效 ✅ |

## 复现入口（游戏更新后）

1. 读 `抓点定稿_20260829.md` → 拉新包 → 对锚点表逐条 `grep -coF`（APK 版 + 热更版 `files/bingo_remote_assets/` 双验）
2. 锚点存活 → `STARRY_INSTALL=1 bash scripts/qlfh_so_only.sh`；锚点漂移 → 重新抓点改 `QlfhMod.cpp`
3. 引擎/保护变化 → 重读初判文档走 PLAYBOOK 阶段 B
4. game_id=`qlfh`（本项目已定，勿改）；签名 key=`01_tools/starry_release.jks`（so_only 重签必须同 key）
