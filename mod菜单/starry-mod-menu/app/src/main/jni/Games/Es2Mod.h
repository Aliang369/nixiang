/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v2（新游戏从这里生成/复制，勿删此文件）
 *
 *  使用方式（二选一）：
 *    ① 推荐：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
 *       自动生成 Games/{Game}Mod.h/.cpp，改好前缀/日志标签/include
 *    ② 手工：复制本对文件为 {Game}Mod.h/.cpp
 *       → 全局替换 Es2_ → {Game}_、ES2_ → {GAME}_、[Es2] → [{game}]
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
#ifndef STARRY_ES2_MOD_H
#define STARRY_ES2_MOD_H

// 功能位号：必须与 Main.cpp 的 g_featureList 数组下标一一对应
// 对外单一功能「自动跳舞」（用户定稿）：效果 = 全部音符最高判定（Amazing）；
// AllPerfect 作为内部中间档保留位号不对外
enum Es2Feat : int {
    ES2_FEAT_CAT_PLAY    = 0,  // Category_玩法
    ES2_FEAT_AUTODANCE   = 1,  // Toggle_自动跳舞（判定全最高，复刻斌哥 AllAmazing）
    ES2_FEAT_ALLPERFECT  = 2,  // 内部保留（不进菜单）
};

bool Es2_TryInit();
bool Es2_IsReady();
void Es2_OnFeatureChanged(int featNum, bool enabled);
void Es2_OnValueChanged(int featNum, int value);
void Es2_Tick(const bool* features);
void Es2_ForceClear();

#endif  // STARRY_ES2_MOD_H
