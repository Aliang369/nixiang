// ============================================================================
//  WbxxmMod.cpp — 吾辈小熊猫（com.wbxxm.wbxxm 5.0 TapTap）
//
//  引擎：Cocos Creator 3.7.1（libcocos.so），无 il2cpp、无壳、7 dex 明文。
//  玩法：assets/assets/main/index.jsc（XXTEA key=a7wAilvNtHvYDTVG，585 个 SystemJS 模块）。
//  包内 libcocos.so 只负责加载/解密 jsc ⇒ **零 hook**（本文件不 DobbyHook 任何地址）。
//
//  开关通道（不是本文件实现的，勿在这里加 hook 去"同步"）：
//    Java 菜单 → ModBridge.onToggleChanged
//              → com.starry.magic.menu.StateMirror.mirror(featNum, value)
//              → {filesDir}/starry_state.txt（空格分隔 8 个 int）
//    JS 侧 assets/main.js 注入段用 jsb.fileUtils 读取（同目录，见
//    10_docs/吾辈小熊猫/功能挂点分析.md 与 星光落地.md）。
//
//  因此 native 侧只需：处于 ready、接受 featNum、Tick 空实现。
//  唯一功能的实际效果在 JS 里（免广告=VideoAdMgr.showVideoAd 短路）。
//  feat#2 内购(GM)：2026-11-13 删除（jsc 还原官方原版 IsGmLogin=false，main.js 分流补丁已撤）。
//  feat#3 无限体力：2026-11-13 删除（服务端字段，本地抬高无效）。
// ============================================================================

#include "WbxxmMod.h"

#include <atomic>

#include "Includes/Logger.h"

#define WBXXM_LOG "[Wbxxm]"

namespace {

std::atomic<bool> g_ready{false};

}  // namespace

bool Wbxxm_TryInit(JavaVM* vm) {
    (void)vm;
    g_ready.store(true);
    LOGI("%s Cocos 3.7.1 静态补丁模式：无 native hook，开关经 starry_state.txt 交给 JS", WBXXM_LOG);
    return true;
}

bool Wbxxm_IsReady() { return g_ready.load(); }

// 开关值不在这里消费：Java StateMirror 已落盘，JS 侧自己读。
// 保留空实现（注册表要求函数指针非空以免回调缺失时崩溃）。
void Wbxxm_OnFeatureChanged(int featNum, bool enabled) {
    LOGI("%s feat#%d = %d", WBXXM_LOG, featNum, enabled ? 1 : 0);
}

void Wbxxm_OnValueChanged(int featNum, int value) {
    LOGI("%s value#%d = %d", WBXXM_LOG, featNum, value);
}

void Wbxxm_Tick(const bool* features, const int* values) {
    (void)features;
    (void)values;
    // 空：不做任何周期性动作（铁律：Tick 只同步开关 + 兜底装钩，本作两者都不需要）
}

void Wbxxm_ForceClear() {
    // 无 hook、无内存改写 ⇒ 无可清理项；JS 侧读到开关=0 自然不生效。
    LOGI("%s forceClear（无 native 状态）", WBXXM_LOG);
}
