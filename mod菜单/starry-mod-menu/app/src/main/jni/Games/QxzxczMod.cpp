// ============================================================================
//  QxzxczMod.cpp — 群侠传：幸存者（com.nenlystudio.qunxiasurvivor.cn 0.2.3.9）
//
//  引擎：Godot 4.6.1（libgodot_android.so），游戏逻辑全在 assets/scripts/**.gdc。
//  包内无 libil2cpp.so ⇒ **零 hook**（本文件不 DobbyHook 任何地址，不读写游戏内存）。
//
//  开关通道（不是本文件实现的，勿在这里加 hook 去"同步"）：
//    Java 菜单 → ModBridge.onToggleChanged/onSliderChanged
//              → com.starry.magic.menu.StateMirror.mirror(featNum, value)
//              → {filesDir}/starry_state.txt（空格分隔 8 个 int）
//    GDScript 侧 FileAccess 读 "user://starry_state.txt"（同目录，实测 user:// == filesDir）。
//
//  因此 native 侧只需：处于 ready、接受 featNum（g_features/g_values 由 MainModern 写），
//  Tick 保持空实现。三个功能的实际效果在 GDScript 里（见
//  10_docs/群侠传：幸存者/星光菜单落地.md 的挂点表）。
// ============================================================================

#include "QxzxczMod.h"

#include <atomic>

#include "Includes/Logger.h"

#define QXZXCZ_LOG "[Qxzxcz]"

namespace {

std::atomic<bool> g_ready{false};

}  // namespace

bool Qxzxcz_TryInit(JavaVM* vm) {
    (void)vm;
    g_ready.store(true);
    LOGI("%s Godot 静态补丁模式：无 native hook，开关经 starry_state.txt 交给 GDScript", QXZXCZ_LOG);
    return true;
}

bool Qxzxcz_IsReady() { return g_ready.load(); }

// 开关值与滑块值不在这里消费：Java StateMirror 已落盘，GDScript 自己读。
// 保留空实现（注册表要求函数指针非空以免回调缺失时崩溃）。
void Qxzxcz_OnFeatureChanged(int featNum, bool enabled) {
    LOGI("%s feat#%d = %d", QXZXCZ_LOG, featNum, enabled ? 1 : 0);
}

void Qxzxcz_OnValueChanged(int featNum, int value) {
    LOGI("%s value#%d = %d", QXZXCZ_LOG, featNum, value);
}

void Qxzxcz_Tick(const bool* features, const int* values) {
    (void)features;
    (void)values;
    // 空：不做任何周期性动作（铁律：Tick 只同步开关 + 兜底装钩，本作两者都不需要）
}

void Qxzxcz_ForceClear() {
    // 无 hook、无内存改写 ⇒ 无可清理项；GDScript 侧读到开关=0 自然不生效。
    LOGI("%s forceClear（无 native 状态）", QXZXCZ_LOG);
}
