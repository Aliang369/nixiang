/* ================================================================
 *  物质与魔法 (com.huanshits.materialMagic) 6.03
 *  抓点：改包 Dobby → StatusC$$UpPhysicalDamage / StatusC$$Update
 * ================================================================ */
#ifndef STARRY_WZYM_MOD_H
#define STARRY_WZYM_MOD_H

// g_featureList 数组下标（Category_ 占位，逻辑勿用）
enum WzymFeat : int {
    WZYM_FEAT_INVINCIBLE = 1,  // Toggle_无敌 → StatusC.wudi
    WZYM_FEAT_DMG_MUL    = 2,  // Slider_伤害倍率 → UpPhysical/MagicDamage
    WZYM_FEAT_AOE_KILL   = 3,  // Toggle_全屏秒杀
    WZYM_FEAT_SPEED      = 4,  // Slider_移速攻速 1=关 2~5=档位
};

bool Wzym_TryInit();
bool Wzym_IsReady();
void Wzym_OnFeatureChanged(int featNum, bool enabled);
void Wzym_OnValueChanged(int featNum, int value);
void Wzym_Tick(const bool* features);
void Wzym_ForceClear();

#endif // STARRY_WZYM_MOD_H
