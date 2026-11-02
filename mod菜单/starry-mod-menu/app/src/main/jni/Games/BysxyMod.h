/* ================================================================
 *  不一样西游（com.wingjoy.toWest 1.0.40 / vc622）星光模块
 *
 *  机制（与对照方「斌哥改包」1:1）：
 *    对照方菜单只有一项：愿/体/缘/毅 各 +99999 —— 实现为
 *    `ToWest.Logic.CharacterLogic::SetAttr(type, delta, isAdd=true)`，type = 0..3。
 *    唯一需要的实例 = 玩家 CharacterLogic（PlayerLogic）：
 *    钩 `ToWest.Mgr.GameMgr::get_Player`（`ldr x0,[x0,#0x28]; ret`）拿到；对照方也钩了这一处。
 *
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_BYSXY_MOD_H
#define STARRY_BYSXY_MOD_H

#include <jni.h>

// 功能位号：必须与 BysxyEntry.cpp 的 featureList 数组下标一一对应
enum BysxyFeat : int {
    BYSXY_FEAT_CAT_ATTR = 0,   // Category_属性
    BYSXY_FEAT_AMOUNT   = 1,   // InputValue_99999:单次加成
    BYSXY_FEAT_ONEKEY   = 2,   // Button_一键四维（对照方同款：愿/体/缘/毅 各加一次）
    BYSXY_FEAT_NOAD     = 3,   // Toggle_免广告（激励视频直接发奖）
};

// 签名与 ModEntry.h 回调一致（Unity IL2CPP 系忽略 vm）
bool Bysxy_TryInit(JavaVM* vm);
bool Bysxy_IsReady();
void Bysxy_OnFeatureChanged(int featNum, bool enabled);
void Bysxy_OnValueChanged(int featNum, int value);
void Bysxy_OnInputChanged(int featNum, const char* text);
void Bysxy_Tick(const bool* features, const int* values);
void Bysxy_ForceClear();

#endif  // STARRY_BYSXY_MOD_H
