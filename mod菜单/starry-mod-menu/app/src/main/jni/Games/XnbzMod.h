/* ================================================================
 *  仙逆：本尊（com.qqsy.xnbz 0.0.14 / vc14 TapTap 渠道原版）
 *  星光菜单游戏模块 —— 注册表版（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  引擎：Cocos Creator 3.x / libcocos.so（JSB，非 IL2CPP；包内无加固壳）
 *
 *  ★ 当前阶段 = ①过环境（把 Mod 菜单组装进游戏、能正常玩）
 *    本模块此刻是**零 hook 骨架**：只做「目标库映射判定 + 开关状态同步」，
 *    不含任何 hook / 内存改写 / JS 注入；开关置 ON 只写日志。
 *    【无敌】【伤害倍率】的真实实现待用户明确放行后进 .cpp 的 §TODO 区。
 * ================================================================ */
#ifndef STARRY_XNBZ_MOD_H
#define STARRY_XNBZ_MOD_H

#include <jni.h>

// 功能位号：必须与 XnbzEntry.cpp 的 kXnbz_Features 数组下标一一对应
enum XnbzFeat : int {
    XNBZ_FEAT_INV = 1,    // Toggle_无敌
    XNBZ_FEAT_DMG = 2,    // Slider_伤害倍率:1:100:1（1 = 关闭）
    XNBZ_FEAT_RETRY = 3,  // Toggle_重试判胜（服务端判负 → 自动重开，直到服务端判胜或到上限）
};

// 签名与 ModEntry.h 的回调契约一致（Cocos 系 tryInit 需要 vm 以便后续挂 JS 桥）
// ★ onLoad 在 JNI_OnLoad 内被调用（有 Java 栈帧）⇒ 只有此刻 FindClass 能解析到应用类加载器，
//   因此在这里缓存 XnbzHooks 类的全局引用 + pump 方法 ID，供后续 native 线程直接用。
void Xnbz_OnLoad(JavaVM* vm);
bool Xnbz_TryInit(JavaVM* vm);
bool Xnbz_IsReady();
void Xnbz_OnFeatureChanged(int featNum, bool enabled);
void Xnbz_OnValueChanged(int featNum, int value);
void Xnbz_Tick(const bool* features, const int* values);
void Xnbz_ForceClear();

#endif  // STARRY_XNBZ_MOD_H
