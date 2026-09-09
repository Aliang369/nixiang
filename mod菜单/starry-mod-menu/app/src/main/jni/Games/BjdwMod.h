/* 伯吉的温馨厨房 com.hnyg.bjdwxcf.topon v1.0.1 · Unity IL2CPP AOT */
#ifndef STARRY_BJDW_MOD_H
#define STARRY_BJDW_MOD_H

enum BjdwFeat : int {
    BJDW_FEAT_COINS     = 1,  // Toggle_无限金币
    BJDW_FEAT_AD_COUPON = 2,  // Toggle_无限广告券
};

bool Bjdw_TryInit();
bool Bjdw_IsReady();
void Bjdw_OnFeatureChanged(int featNum, bool enabled);
void Bjdw_Tick(const bool* features);
void Bjdw_ForceClear();

#endif
