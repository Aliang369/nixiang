/* ================================================================
 *  JfqsEntry — 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：疾风骑士 com.emay.wxsy.topon v60.0.1（TapTap 渠道）
 *  目标库：libil2cpp.so（Unity IL2CPP, Metadata v31, arm64-v8a）
 *
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 JfqsMod.h 的 enum 对齐
 *    - 滑块 1 = 关闭，默认必须 1；上限必须与 JfqsMod.h 的 *_MUL_MAX 一致
 * ================================================================
 */
#include "ModEntry.h"
#include "JfqsMod.h"

static const char* kJfqs_Features[] = {
    "Category_战斗",            // 0  分组标题
    "Slider_伤害倍率:1:100:1",  // 1  JFQS_FEAT_DMG     上限 = JFQS_DMG_MUL_MAX
    "Toggle_超级无敌",          // 2  JFQS_FEAT_INV
    "Toggle_无限货币",          // 3  JFQS_FEAT_MONEY
    "Slider_移动速度:1:20:1",   // 4  JFQS_FEAT_MOVE    上限 = JFQS_MOVE_MUL_MAX
    "Toggle_技能无限",          // 5  JFQS_FEAT_SKILL
    "Toggle_无限跳跃",          // 6  JFQS_FEAT_JUMP
    "Toggle_免广告",            // 7  JFQS_FEAT_NOAD
    "Category_解锁",            // 8  分组标题
    "Toggle_解锁全角色",        // 9  JFQS_FEAT_CHAR
    "Toggle_解锁全皮肤",        // 10 JFQS_FEAT_SKIN
    "Toggle_解锁全武器",        // 11 JFQS_FEAT_WEAPON
    "Toggle_解锁全宠物",        // 12 JFQS_FEAT_PET
    "Toggle_解锁全神器",        // 13 JFQS_FEAT_ARTIFACT
    "Toggle_解锁全部技能",      // 14 JFQS_FEAT_UNLOCKSKILL
    "Toggle_解锁协同",          // 15 JFQS_FEAT_SYNERGY
    "Toggle_百分百改造成功",    // 16 JFQS_FEAT_ENHANCE
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "jfqs",
        .authGameId = "jfqs",           // 疾风骑士 拼音首字母
        .featureList = kJfqs_Features,
        .featureCount = (int)(sizeof(kJfqs_Features) / sizeof(kJfqs_Features[0])),

        .waitLib = "libil2cpp.so",      // 等目标库映射后再走 TryInit
        .initDelayMs = 2000,            // 等库后延迟（避开 il2cpp 半初始化窗口）
        .initRetryMax = 120,            // 启动期重试上限
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 5000,         // 5s：按名挂载可重试晚注册的类

        .onLoad = nullptr,              // 本游戏无需 EarlyHook / 过检补丁
        .tryInit = Jfqs_TryInit,
        .isReady = Jfqs_IsReady,
        .tick = Jfqs_Tick,
        .onFeatureChanged = Jfqs_OnFeatureChanged,
        .onValueChanged = Jfqs_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputValue
        .forceClear = Jfqs_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
