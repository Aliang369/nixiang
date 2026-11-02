/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v3.1（注册表版，复制后按 TODO 填空）
 *
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
 *  产出四件套：{Game}Mod.h/.cpp + {Game}Entry.cpp + Android_{tag}.mk
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 均零修改）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *  v3.1 追加（见 .cpp §④，2026-10-02 疾风骑士 60.0.1 实战下沉）：
 *    并发装钩互斥（g_installMutex + double-check，防同址双 Dobby SIGSEGV）
 *    致命信号兜底（sigsetjmp 保护区 DobbyHookGuarded/ReadU32Guarded/RunGuarded
 *                  + sigaltstack 备用栈 + 链回旧处理器）
 *    槽位熔断（g_slotDead[]：炸过的槽位本次会话不再重试）
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_AYXS_MOD_H
#define STARRY_AYXS_MOD_H

#include <jni.h>

// 功能位号：必须与 {Game}Entry.cpp 的 featureList 数组下标一一对应
enum AyxsFeat : int {
    AYXS_FEAT_DINGGUAI = 1,  // 超级定怪（sh_bgqq1 恒 false）
    AYXS_FEAT_HUIFU    = 2,  // 超级恢复（sh_bgqt1 +1000000）
    AYXS_FEAT_MIAOSHA  = 3,  // 超级秒杀（sh_bgqd1 -9999999，仅敌方）
    AYXS_FEAT_NOAD     = 4,  // 免广告（ShowVideoAds 直接回调 onVideoAdComplete）
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Ayxs_TryInit(JavaVM* vm);
bool Ayxs_IsReady();
void Ayxs_OnFeatureChanged(int featNum, bool enabled);
void Ayxs_OnValueChanged(int featNum, int value);
void Ayxs_Tick(const bool* features, const int* values);
void Ayxs_ForceClear();

#endif  // STARRY_AYXS_MOD_H
