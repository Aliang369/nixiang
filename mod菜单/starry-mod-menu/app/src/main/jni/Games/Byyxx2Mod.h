/* ================================================================
 *  不一样修仙2（com.wingjoy.immortal2 1.4.135）星光模块
 *
 *  机制：不逐方法 hook，只挂 Immortal.GameMgr::Update 一帧，从 GameMgr 取
 *        Player（CharacterLogic/PlayerLogic）实例，再在游戏线程上按名调用
 *        PlayerLogic 的属性 setter / Add* 方法（IL2CPP 反射层，argc 全部实测）。
 *        与对照方（斌哥包）1:1：对照包也是唯一一处 12 字节 Dobby 跳桩
 *        @ GameMgr::Update（RVA 0xF6F788，DumpRva 反查）。
 *
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_BYYXX2_MOD_H
#define STARRY_BYYXX2_MOD_H

#include <jni.h>

// 功能位号：必须与 Byyxx2Entry.cpp 的 featureList 数组下标一一对应
enum Byyxx2Feat : int {
    BYYXX2_FEAT_CAT_RES   = 0,   // Category_资源
    BYYXX2_FEAT_AMOUNT    = 1,   // InputValue_1000000:单次加成
    BYYXX2_FEAT_MONEY     = 2,   // Button_金币
    BYYXX2_FEAT_RENOWN    = 3,   // Button_声望
    BYYXX2_FEAT_SECT      = 4,   // Button_宗门贡献
    BYYXX2_FEAT_EXP       = 5,   // Button_修为
    BYYXX2_FEAT_LIFETIME  = 6,   // Button_寿元
    BYYXX2_FEAT_ALL       = 7,   // Button_一键全资源
    BYYXX2_FEAT_CAT_INC   = 8,   // Category_自增
    BYYXX2_FEAT_INC_MULT  = 9,   // InputValue_1000:自增倍率
    BYYXX2_FEAT_INC_MONEY = 10,  // Toggle_金币自增
    BYYXX2_FEAT_INC_EXP   = 11,  // Toggle_修为自增
    BYYXX2_FEAT_INC_LIFE  = 12,  // Toggle_寿元自增
    BYYXX2_FEAT_NOAD      = 13,  // Toggle_免广告
};

// 签名与 ModEntry.h 回调一致（Unity IL2CPP 系忽略 vm）
bool Byyxx2_TryInit(JavaVM* vm);
bool Byyxx2_IsReady();
void Byyxx2_OnFeatureChanged(int featNum, bool enabled);
void Byyxx2_OnValueChanged(int featNum, int value);
void Byyxx2_OnInputChanged(int featNum, const char* text);
void Byyxx2_Tick(const bool* features, const int* values);
void Byyxx2_ForceClear();

#endif  // STARRY_BYYXX2_MOD_H
