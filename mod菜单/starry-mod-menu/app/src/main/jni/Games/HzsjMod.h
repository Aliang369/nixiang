/* ================================================================
 *  魂坠深境 (com.chillyroom.containerofsoul.yw) — 1.0.5 / vc40
 * ================================================================ */
#ifndef STARRY_HZSJ_MOD_H
#define STARRY_HZSJ_MOD_H

enum HzsjFeat : int {
    HZSJ_FEAT_SECKILL     = 1,  // Toggle_秒杀
    HZSJ_FEAT_INVINCIBLE  = 2,  // Toggle_无敌
    HZSJ_FEAT_INF_CARD    = 3,  // Toggle_无限卡牌
    HZSJ_FEAT_INF_COIN    = 4,  // Toggle_无限金币
    HZSJ_FEAT_INF_CRYSTAL = 5,  // Toggle_无限魂晶
};

bool Hzsj_TryInit();
bool Hzsj_IsReady();
void Hzsj_OnFeatureChanged(int featNum, bool enabled);
void Hzsj_OnValueChanged(int featNum, int value);
void Hzsj_Tick(const bool* features);
void Hzsj_ForceClear();
/** OTA patch.json：覆盖兜底偏移 / invoke 文件偏移（须在挂跳板前调用） */
void Hzsj_ApplyPlayPatch(const char* json);
#ifdef STARRY_HZSJ_PROBE
void Hzsj_StartProbe();
#endif

#endif // STARRY_HZSJ_MOD_H
