/* ================================================================
 *  {Game}Entry — 注册表入口模板（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  生成：new_game_mod.sh 自动复制为 {Game}Entry.cpp 并替换标识。
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 Mod 的 enum 对齐
 *
 *  ★ game_id 与 C++ 标识符分离（2026-11-15 Lead 裁决）：
 *    menu/template_id 与后台 games.game_id **必须是字符串 "999mxcz"**（全名 999秒!幸存者 拼音首字母，
 *    数字保留；见 10_docs/999秒幸存者/game_id_推导.md）；
 *    C++ 标识符仅因语法限制（不能以数字开头）用 Mxcz999 —— 文件名/宏/函数名/tag 全走 Mxcz999，
 *    鉴权串一律 "999mxcz"。同先例：Lmq20fz（文件/tag）→ lmq20fz（authGameId）。
 * ================================================================
 */
#include "ModEntry.h"
#include "Mxcz999Mod.h"

static const char* kMxcz999_Features[] = {
    "Category_战斗",              // 0  （分类占一个下标，无回调）
    "Toggle_无敌",                // 1  MXCZ999_FEAT_INVINCIBLE
    "Slider_伤害倍率:1:100:1",    // 2  MXCZ999_FEAT_DMG_MUL（滑杆，1 = 关；getVal）
    "Slider_移动速度:1:5:1",      // 3  MXCZ999_FEAT_MOVE_SPEED（滑杆，1 = 关；getVal）
    "Toggle_解锁所有皮肤",        // 4  MXCZ999_FEAT_UNLOCK_SKIN
    "Toggle_无限解锁券",          // 5  MXCZ999_FEAT_UNLOCK_COUPON
    "Toggle_免广告",              // 6  MXCZ999_FEAT_NO_AD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Mxcz999",
        .authGameId = "999mxcz",        // 999秒!幸存者 → 999mxcz（数字保留 + 秒/幸/存/者 首字母；后台 games.game_id 须同名）
        .featureList = kMxcz999_Features,
        .featureCount = (int)(sizeof(kMxcz999_Features) / sizeof(kMxcz999_Features[0])),

        .waitLib = "libil2cpp.so",       // Unity IL2CPP classic（本包 libil2cpp.so 63,850,280 B）；与 Mod.cpp 的 TPL_TARGET_LIB 一致
        .initDelayMs = 2000000,            // 等库后延迟（避开 il2cpp/HybridCLR 半初始化；实测再调）
        .initRetryMax = 120,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Mxcz999_TryInit,
        .isReady = Mxcz999_IsReady,
        .tick = Mxcz999_Tick,
        .onFeatureChanged = Mxcz999_OnFeatureChanged,
        .onValueChanged = Mxcz999_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Mxcz999_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
