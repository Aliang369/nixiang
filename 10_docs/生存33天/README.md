# 生存33天 — 星光 Mod 执行手册

> 包名 `com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive` · TapTap **1.0.23**  
> 验证：2026-08-24 真机 · 五项功能均已通过

## 正式成品

| 项 | 路径 |
|----|------|
| **正式 APK** | `08_output/生存33天/生存33天_1.0.23_surgical-releaseSigned.apk` |
| Native | `mod菜单/starry-mod-menu/app/src/main/jni/Games/Sc33Mod.cpp` |
| Java | `mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/cocos/Sc33Hooks.java` |
| 菜单模板 | `10_docs/生存33天/sc33_menu_template.json` |

## 构建（禁止 apktool b 游戏原包）

```bash
# 首包 / 换 classes10+Manifest（少见）
bash mod菜单/starry-mod-menu/scripts/surgical_sc33_1.0.23.sh

# 日常：只换 so + classes11
cd mod菜单/starry-mod-menu
STARRY_INSTALL=1 bash scripts/sc33_so_only.sh

# 仅改 so、dex 已正确时加速
STARRY_UPDATE_DEX=0 STARRY_INSTALL=1 bash scripts/sc33_so_only.sh

# Trace 定锚（不 splice）
STARRY_TRACE=1 STARRY_INSTALL=1 bash scripts/sc33_so_only.sh
```

覆盖安装，**禁止卸载**。

## 菜单 ↔ 实现

| 菜单 feat | 功能 | 实现 |
|-----------|------|------|
| 1 | 百倍伤害 | `get_ret(1)` · baseDmg splice |
| 2 | 无敌 | `get_ret(2)` |
| 3 | 全屏秒杀 | `get_ret(3)` · FightChangeEntityHp AOE |
| 4 | 攻速滑条 1–10 | `Sc33Hooks.getAtkMul()` |
| 6 | 免广告 | `SdkShowAdReward` smali |

锚点：`Trace锚点_1.0.23.md`

## 文档索引

| 文档 | 用途 |
|------|------|
| `抓点定稿_20260824.md` | prefs / 引擎 / 编号 |
| `验证记录_20260824.md` | 真机验证 |
| `sc33_menu_template.json` | 后台 feat 同步 |
