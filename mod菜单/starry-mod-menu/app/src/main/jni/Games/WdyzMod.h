/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v2（新游戏从这里生成/复制，勿删此文件）
 *
 *  使用方式（二选一）：
 *    ① 推荐：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
 *       自动生成 Games/{Game}Mod.h/.cpp，改好前缀/日志标签/include
 *    ② 手工：复制本对文件为 {Game}Mod.h/.cpp
 *       → 全局替换 Wdyz_ → {Game}_、WDYZ_ → {GAME}_、[Wdyz] → [{game}]
 *       → 按 .cpp 尾部 TODO 完成三处接线（Main.cpp / GameConfig.h / Android.mk）
 *
 *  v2 防闪退设计（GAME_MOD_GUIDE 铁律已内置为代码，不是注释）：
 *    - 默认 0 hook：开关 ON 才经 SyncHooks 分组安装（hook-rules §16）
 *    - 双路径挂点：RVA（maps RX + 序言校验）→ 按名（GetMethod），谁成功用谁；
 *      RVA 漂移/空页自动拒绝转按名，杜绝挂错地址闪退
 *    - 单方法缺失只降级并 log，不整包失败（hook-rules §17）
 *    - 幂等安装：*orig 非空即视为已挂，杜绝双 Dobby 叠钩 SIGSEGV
 *    - orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 * ================================================================ */
#ifndef STARRY_WDYZ_MOD_H
#define STARRY_WDYZ_MOD_H

// 功能位号：对齐 Main.cpp g_featureList 的 WDYZ 块（#1 无敌 / #2 五倍攻击）
enum WdyzFeat : int {
    WDYZ_FEAT_INV = 1,  // Toggle_无敌 → HeroSettle 结算值归零（承伤不掉血）
    WDYZ_FEAT_DMG = 2,  // Toggle_五倍攻击 → BattleObjectCore 敌 core 结算 ×5
};

bool Wdyz_TryInit();
bool Wdyz_IsReady();
void Wdyz_OnFeatureChanged(int featNum, bool enabled);
void Wdyz_OnValueChanged(int featNum, int value);
void Wdyz_Tick(const bool* features);
void Wdyz_ForceClear();

#endif  // STARRY_WDYZ_MOD_H
