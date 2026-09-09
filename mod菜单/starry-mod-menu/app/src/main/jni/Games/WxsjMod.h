/* ================================================================
 *  无限升级 (com.xingzuodaoshi.infinityUpgrade)
 *  方案：裸 Dobby hook 热更 dll 装载边界（免 il2cpp API，壳无导出）
 *    RuntimeAssemblyLoader::ExecuteRuntimeAssembly RVA=0x2b3f3c4
 *    来源：06_dumped/无限升级 bss methodPointers 实测（2026-09-05，
 *    Il2CppDumper 失效，bss 搜模块名指针→methodPointers[rid-1]）
 *    绑定版本 vc1637 / 11.266018；升级后 RVA 必须重解
 *  功能：hook 内就地把 targetfuntional.dll 字节替换为内置 patched dll，
 *        功能开关由 dll 内 StarryConfig 读浮窗配置文件（2s 缓存）
 *  feat_num = Main.cpp g_featureList 下标（含 Category）
 * ================================================================ */
#ifndef STARRY_WXSJ_MOD_H
#define STARRY_WXSJ_MOD_H

// 2026-09-05 精简版：保留 无敌/秒杀/免广告/物品不减/技能免费
// 2026-09-07 v4.1/v4.2：屏蔽检测进菜单（feat7→10→7）；v7.2 无条件武装替换（清数据/首装生效）
// 2026-09-07 v7.3：屏蔽封号执法撤出菜单——block_report 由 native 强制默认开，玩家不可关
// featNum = g_featureList 数组下标（含 Category 占位），开关映射必须一一对应
enum WxsjFeat : int {
    WXSJ_FEAT_GOD       = 1,  // Toggle_无敌          -> god
    WXSJ_FEAT_ONESHOT   = 2,  // Toggle_秒杀          -> oneshot
    WXSJ_FEAT_FREEADS   = 4,  // Toggle_免广告        -> free_ads
    WXSJ_FEAT_ITEMKEEP  = 5,  // Toggle_物品不减      -> item_keep
    WXSJ_FEAT_SKILLFREE = 6,  // Toggle_技能免费升级  -> skill_free
};

bool Wxsj_TryInit();
bool Wxsj_IsReady();
void Wxsj_OnFeatureChanged(int featNum, bool enabled);
void Wxsj_ForceClear();

#endif  // STARRY_WXSJ_MOD_H
