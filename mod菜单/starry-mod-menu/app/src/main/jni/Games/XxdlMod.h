#ifndef STARRY_XXDL_MOD_H
#define STARRY_XXDL_MOD_H

#include <jni.h>

// 功能位号：与 XxdlEntry.cpp 的 featureList 下标一一对应
// 对应斌哥新版菜单 4 项（抓点总表 §四.1）：资源 / 灵石 / 仙晶 / 体力
enum XxdlFeat : int {
    XXDL_FEAT_RES       = 1,  // Toggle 资源+1000万   → base.gold
    XXDL_FEAT_LINGSHI   = 2,  // Toggle 灵石+1000万   → base.diamond
    XXDL_FEAT_XIANJING  = 3,  // Toggle 仙晶+100万    → base.gem
    XXDL_FEAT_TILI      = 4,  // Button 体力回满      → assets.new = assets.max
    XXDL_FEAT_NOAD      = 5,  // Toggle 免广告        → PlatformManager 播放回调直通
};

bool Xxdl_TryInit(JavaVM* vm);
bool Xxdl_IsReady();
void Xxdl_OnFeatureChanged(int featNum, bool enabled);
void Xxdl_OnValueChanged(int featNum, int value);
void Xxdl_Tick(const bool* features, const int* values);
void Xxdl_ForceClear();

#endif  // STARRY_XXDL_MOD_H
