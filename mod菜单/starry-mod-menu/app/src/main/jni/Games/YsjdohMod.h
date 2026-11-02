#ifndef STARRY_YSJDOH_MOD_H
#define STARRY_YSJDOH_MOD_H

#include <jni.h>

/* 功能位号：必须与 YsjdohEntry.cpp 的 featureList 下标一一对应 */
enum YsjdohFeat : int {
    YSJDOH_FEAT_MULT = 1,  // Slider_伤害倍率:1:100:1（1=关闭，2..100=倍率）
    YSJDOH_FEAT_INV  = 2,  // Toggle_超级无敌
    YSJDOH_FEAT_SPD  = 3,  // Toggle_战斗加速
    YSJDOH_FEAT_NOAD = 4,  // Toggle_免广告（激励视频直接发奖 + 不弹插屏）
};

bool Ysjdoh_TryInit(JavaVM* vm);
bool Ysjdoh_IsReady();
void Ysjdoh_OnFeatureChanged(int featNum, bool enabled);
void Ysjdoh_OnValueChanged(int featNum, int value);
void Ysjdoh_Tick(const bool* features, const int* values);
void Ysjdoh_ForceClear();

#endif  // STARRY_YSJDOH_MOD_H
