/* ================================================================
 *  星光菜单游戏模块 — 凡尘西游修真记 (com.qxjc.fcxyxzj / TapTap 1.1.0)
 *
 *  引擎：Unity 2022.3.62f2c1 IL2CPP AOT（无 HybridCLR 热更）
 *  入口：MainModern.cpp 注册表模式（Main.cpp/GameConfig.h/公共 Android.mk 零修改）
 *
 *  ★ 本包特殊点：libil2cpp.so 的 5 个导出符号被 libthemis-v8.so 劫持
 *    （il2cpp_image_get_name / il2cpp_assembly_get_image / il2cpp_class_from_name /
 *      il2cpp_class_get_method_from_name / il2cpp_class_get_type）
 *    → 本文件自带裸读解析（image = *(void**)assembly + image 类枚举），
 *      不使用 Unity/Il2CppApi.* 的 ImageOf / ClassFromName / GetMethod 链路。
 *
 *  功能位号：必须与 FcxyxzjEntry.cpp 的 featureList 下标一一对应
 * ================================================================ */
#ifndef STARRY_FCXYXZJ_MOD_H
#define STARRY_FCXYXZJ_MOD_H

#include <jni.h>

enum FcxyxzjFeat : int {
    FCXYXZJ_FEAT_AD   = 1,  // Toggle_免广告（挂机/祈福/广告位直接发奖，不播广告）
    FCXYXZJ_FEAT_CUR  = 2,  // Toggle_无限资源（修为/灵石 获取放大 + 消耗不减）
    FCXYXZJ_FEAT_ITEM = 3,  // Toggle_物品不减（药瓶/物品/符文书/快捷栏喝药 不消耗）
    FCXYXZJ_FEAT_DROP = 4,  // Toggle_掉落翻倍（战斗结算掉落 修为/灵石 ×2）
    FCXYXZJ_FEAT_RATE = 5,  // Toggle_掉落必掉（副本结算概率判定全通过，配置内可掉项必掉）
};

// 签名与 ModEntry.h 回调一致（Unity 系忽略 vm）
bool Fcxyxzj_TryInit(JavaVM* vm);
bool Fcxyxzj_IsReady();
void Fcxyxzj_OnFeatureChanged(int featNum, bool enabled);
void Fcxyxzj_OnValueChanged(int featNum, int value);
void Fcxyxzj_Tick(const bool* features, const int* values);
void Fcxyxzj_ForceClear();

#endif  // STARRY_FCXYXZJ_MOD_H
