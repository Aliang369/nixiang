/* ================================================================
 *  WbxxmMod.h — 吾辈小熊猫（Cocos Creator 3.7.1 / SystemJS + jsc）模块
 *
 *  本作无 il2cpp、无 native hook 面：玩法全在
 *      assets/assets/main/index.jsc（585 个 SystemJS 模块，XXTEA 加密）
 *  功能实现走「JS 运行时补丁」：
 *      · 免广告 → assets/main.js（明文、不在热更清单）在
 *        application.start() 后 System.import 目标模块改写 prototype 方法。
 *      · 内购(GM) 已于 2026-11-13 删除：jsc 还原官方原版（IsGmLogin=false），
 *        main.js 不再有 showGMLogin 分流。
 *
 *  ⇒ 本模块**不装任何 hook**（不 DobbyHook、不读写游戏内存），只承担：
 *      1) 让注册表有合法的 featNum 表（菜单一项：免广告）
 *      2) 让 native 侧接受开关值（nativeSetFeature/nativeSetValue 写 g_features/g_values）
 *      3) 保持 isReady()=true，避免菜单显示「native 未就绪」
 *
 *  开关通道：Java StateMirror 落盘 {filesDir}/starry_state.txt（空格分隔 8 个 int），
 *  JS 侧用 jsb.fileUtils.getWritablePath() + "starry_state.txt" 读取。
 *
 *  功能位号必须与 WbxxmEntry.cpp 的 featureList 下标一一对应。
 * ================================================================ */
#ifndef STARRY_WBXXM_MOD_H
#define STARRY_WBXXM_MOD_H

#include <jni.h>

enum WbxxmFeat : int {
    WBXXM_FEAT_AD     = 1,  // Toggle_免广告 → starry_state[1]
    // feat#2「内购(GM)」2026-11-13 删除（用户定：只保留免广告）
    // feat#3「无限体力(仅本地)」2026-11-13 删除（服务端字段，本地抬高无效）
};

bool Wbxxm_TryInit(JavaVM* vm);
bool Wbxxm_IsReady();
void Wbxxm_OnFeatureChanged(int featNum, bool enabled);
void Wbxxm_OnValueChanged(int featNum, int value);
void Wbxxm_Tick(const bool* features, const int* values);
void Wbxxm_ForceClear();

#endif  // STARRY_WBXXM_MOD_H
