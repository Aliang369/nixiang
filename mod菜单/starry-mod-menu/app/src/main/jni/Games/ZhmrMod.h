/* ================================================================
 *  战魂铭人 (com.chillyroom.zhmr.yw) — HybridCLR / IL2CPP 作弊模块
 *  3.5.0 重构（2026-08-29）：按 Games/_TemplateMod v2 骨架 + hook-rules 铁律
 *  - 战斗：Dobby 按名 hook 具体方法（懒装/幂等/必调 orig），不堆扫
 *  - 经济：静态 setter runtime_invoke（值类型传 &int32，禁 boxed）
 *  - 解锁：Archive.heroArchiveDic / PlayerArchive._resourceDic 遍历直写
 *  证据：10_docs/战魂铭人/战魂铭人_3.5.0_复刻记录_20260829.md
 * ================================================================ */
#ifndef STARRY_ZHMR_MOD_H
#define STARRY_ZHMR_MOD_H

// g_featureList 数组下标（Category_ 占 0/3/6，逻辑勿用 Category 下标）
enum ZhmrFeat : int {
    ZHMR_FEAT_GOD      = 1,  // Toggle_无敌
    ZHMR_FEAT_ONEHIT   = 2,  // Toggle_秒杀
    ZHMR_FEAT_CURRENCY = 4,  // Toggle_无限货币（三石 9999）
    ZHMR_FEAT_ITEMS    = 5,  // Toggle_无限道具（高封号操作，其它 999）
    ZHMR_FEAT_UNLOCK   = 7,  // Toggle_解锁全角色与技能
    ZHMR_FEAT_SKIN     = 8,  // Toggle_解锁全皮肤
};

/** 等待 libil2cpp + 热更类就绪后按名解析类/方法/字段（一次性，SafeProbe 仅此处） */
bool Zhmr_TryInit();

bool Zhmr_IsReady();

/** 菜单开关变化（含懒装战斗 hook 的 SyncHooks） */
void Zhmr_OnFeatureChanged(int featNum, bool enabled);

/** zhmr 无 Slider，保留契约空实现 */
void Zhmr_OnValueChanged(int featNum, int value);

/**
 * 主循环调用（Main.cpp 引擎线程 200ms）。
 * 只做：同步开关 + 兜底装 hook + 低频重放经济/解锁；禁扫堆/禁 SafeProbe。
 */
void Zhmr_Tick(const bool* features);

/** 授权失效路径（Main.cpp clear_runtime_session）：全部开关与待办复位 */
void Zhmr_ForceClear();

#endif // STARRY_ZHMR_MOD_H
