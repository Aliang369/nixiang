/* ================================================================
 *  山河旅探 (com.itaotuo.shanhelvtan) — 1.4.38 / vc7
 *  解锁：钩 ILRuntime.AppDomain.LoadAssembly，改热更 DLL IL。
 *  不 hook ILIntepreter.Execute。
 *  feat_num = Main.cpp g_featureList 下标（含 Category 占位）
 * ================================================================ */
#ifndef STARRY_SHLT_MOD_H
#define STARRY_SHLT_MOD_H

enum ShltFeat : int {
    SHLT_FEAT_CHAPTER = 1,  // Toggle_解锁所有章节
    SHLT_FEAT_SKIN    = 2,  // Toggle_解锁所有服饰
};

bool Shlt_TryInit();
bool Shlt_IsReady();
void Shlt_OnFeatureChanged(int featNum, bool enabled);
void Shlt_OnValueChanged(int featNum, int value);
void Shlt_Tick(const bool* features);
void Shlt_ForceClear();

#endif // STARRY_SHLT_MOD_H
