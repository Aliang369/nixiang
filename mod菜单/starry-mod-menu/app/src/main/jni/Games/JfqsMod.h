/* ================================================================
 *  星光菜单游戏模块 — 疾风骑士 (com.emay.wxsy.topon) v60.0.1 TapTap 渠道
 *
 *  目标库：libil2cpp.so（Unity IL2CPP, Metadata v31, arm64-v8a）
 *  原版与斌哥改包 libil2cpp.so md5 一致 (e8def0676740a16a06cac155c7cf87d2)
 *  ⇒ 抓点 RVA 零重定位，可直接复用。
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_JFQS_MOD_H
#define STARRY_JFQS_MOD_H

#include <jni.h>

// 功能位号：必须与 JfqsEntry.cpp 的 kJfqs_Features 数组下标一一对应
// 0 = Category_战斗, 8 = Category_解锁（分组标题，不占功能开关）
enum JfqsFeat : int {
    JFQS_FEAT_DMG        = 1,   // Slider_伤害倍率      (1 = 关闭)
    JFQS_FEAT_INV        = 2,   // Toggle_超级无敌
    JFQS_FEAT_MONEY      = 3,   // Toggle_无限货币
    JFQS_FEAT_MOVE       = 4,   // Slider_移动速度      (1 = 关闭)
    JFQS_FEAT_SKILL      = 5,   // Toggle_技能无限
    JFQS_FEAT_JUMP       = 6,   // Toggle_无限跳跃
    JFQS_FEAT_NOAD       = 7,   // Toggle_免广告
    JFQS_FEAT_CHAR       = 9,   // Toggle_解锁全角色
    JFQS_FEAT_SKIN       = 10,  // Toggle_解锁全皮肤
    JFQS_FEAT_WEAPON     = 11,  // Toggle_解锁全武器
    JFQS_FEAT_PET        = 12,  // Toggle_解锁全宠物
    JFQS_FEAT_ARTIFACT   = 13,  // Toggle_解锁全神器
    JFQS_FEAT_UNLOCKSKILL= 14,  // Toggle_解锁全部技能
    JFQS_FEAT_SYNERGY    = 15,  // Toggle_解锁协同
    JFQS_FEAT_ENHANCE    = 16,  // Toggle_百分百改造成功
};

// Slider 上限（必须与 JfqsEntry.cpp 的 Slider_ 声明一致）
#define JFQS_DMG_MUL_MAX   100
#define JFQS_MOVE_MUL_MAX  20

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Jfqs_TryInit(JavaVM* vm);
bool Jfqs_IsReady();
void Jfqs_OnFeatureChanged(int featNum, bool enabled);
void Jfqs_OnValueChanged(int featNum, int value);
void Jfqs_Tick(const bool* features, const int* values);
void Jfqs_ForceClear();

#endif  // STARRY_JFQS_MOD_H
