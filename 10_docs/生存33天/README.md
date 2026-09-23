# 生存33天 — 星光 Mod 执行手册

> 包名 `com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive` · TapTap
> **当前版本 1.0.24**（2026-09-23 · 真机进局实测**五项功能全部通过**）
> 上一版 1.0.23（2026-08-24）五项功能均已真机通过

## 正式成品

| 项 | 路径 |
|----|------|
| **正式 APK（1.0.24）** | `08_output/生存33天/生存33天_1.0.24_surgical-releaseSigned.apk` |
| 历史正式包（1.0.23） | `08_output/生存33天/生存33天_1.0.23_surgical-releaseSigned.apk` |
| Native | `mod菜单/starry-mod-menu/app/src/main/jni/Games/Sc33Mod.cpp` |
| Java | `mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/cocos/Sc33Hooks.java` |
| 菜单模板 | `10_docs/生存33天/sc33_menu_template.json` |

## 构建（禁止 apktool b 游戏原包）

```bash
cd mod菜单/starry-mod-menu

# 1.0.24 首包 / 换 classes8+Manifest（脚本自动临时切换 Android.mk，trap 还原）
STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1 bash scripts/surgical_sc33_1.0.24.sh

# Trace 定锚（dump + 锚点扫描，不 splice）
STARRY_TRACE=1 STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1 bash scripts/surgical_sc33_1.0.24.sh

# 1.0.23 旧路径（脚本硬编码 1.0.23，仅历史参考）
bash scripts/surgical_sc33_1.0.23.sh
```

覆盖安装优先，**禁止卸载**（`STARRY_ALLOW_UNINSTALL=1` 仅在异签名覆盖失败时才走卸载）。

## 菜单 ↔ 实现

| 菜单 feat | 功能 | `get_ret` | 实现 |
|-----------|------|-----------|------|
| 1 | 百倍伤害 | 1 | `t = u.baseDmg;`（1.0.24）/ FightChangeEntityHp splice |
| 2 | 无敌 | 2 | 同上 |
| 3 | 全屏秒杀 | 3 | `FightChangeEntityHp` AOE（1.0.24 用 `h.EntityManager`） |
| 4 | 攻速滑条 1–10 | 5 | `Sc33Hooks.getAtkMul()` → `CalcFinalAtkSpeedValue` / `GetViewFightTimeRate` |
| 6 | 免广告 | 4 | `SdkManager.SdkShowAdReward` smali 早退 |

> 菜单下标 4 = 攻速、6 = 免广告（中间 5 为分类标题）。与镇邪人 ZxrMod 的 `get_ret` 编号不同，勿混用。

## 文档索引

| 文档 | 用途 |
|------|------|
| `锚点与验证_1.0.24.md` | **当前版本**锚点重定 / 崩溃根因 / 真机证据 |
| `Trace锚点_1.0.23.md` | 1.0.23 锚点（历史） |
| `抓点定稿_20260824.md` | prefs / 引擎 / 编号 |
| `验证记录_20260824.md` | 1.0.23 真机验证 |
| `sc33_menu_template.json` | 后台 feat 同步 |
