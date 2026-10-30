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
 * ================================================================
 */
#include "ModEntry.h"
#include "WlwwMod.h"

// FgoBoot.cpp 导出：无 Manifest provider 时由本 so 自己起菜单启动线程
extern "C" void FgoBoot_StartMenu(JNIEnv* env);

// JNI_OnLoad 尾部回调。本作不走 StarryInitProvider（TP 拦 Manifest 改动），
// 菜单 dex 由 FgoBoot_InjectEarlyFromOnLoad 从 assets/starry_dex/starry.dex 注入，
// 注入完这里补一步"起线程等 Application 就绪后 MenuService.start"，
// 否则没有任何一方会拉起菜单（FgoBoot_StartMenu 原本只由 AceStub.cpp 调用）。
static void Wlww_OnLoad(JavaVM* vm) {
    if (!vm) return;
    JNIEnv* env = nullptr;
    if (vm->GetEnv((void**)&env, JNI_VERSION_1_6) != JNI_OK) {
        if (vm->AttachCurrentThread(&env, nullptr) != JNI_OK) return;
    }
    FgoBoot_StartMenu(env);
}

static const char* k为了吾王_Features[] = {
    "Category_资源",          // 0（占下标，无回调）
    "Toggle_无限学识点",      // 1  为了吾王_FEAT_LORE   LorePersistence::GetLore / IsPurchased
    "Toggle_无限金币",        // 2  为了吾王_FEAT_GOLD   CharacterStats::m_Gold / ChangeGold
    "Toggle_无限专注点",      // 3  为了吾王_FEAT_FOCUS  CharacterStats::UpdateFocusPoints
    "Category_战斗",          // 4（占下标，无回调）
    "Toggle_技能判定全成功",  // 5  为了吾王_FEAT_SKILL  SkillRoll::Roll
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "为了吾王",
        .authGameId = "wlww",           // 为了吾王 → wei le wu wang（虚词计入）
        .featureList = k为了吾王_Features,
        .featureCount = (int)(sizeof(k为了吾王_Features) / sizeof(k为了吾王_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 2000000,            // 等库后延迟（避开 il2cpp 半初始化；实测再调）
        .initRetryMax = 120,            // 启动期重试上限
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        // 过检隔离本身在 MainModern.cpp 的 JNI_OnLoad 第一条语句（SignalQuarantine_Start），
        // 而本作进 JNI_OnLoad 的方式是 Games/WlwwEarlyBoot.cpp 的构造函数手工调用
        // （native dlopen 不触发 JNI_OnLoad），见该文件头。
        // 这里只做菜单启动这一环（无 Manifest provider）。
        .onLoad = Wlww_OnLoad,
        .tryInit = 为了吾王_TryInit,
        .isReady = 为了吾王_IsReady,
        .tick = 为了吾王_Tick,
        .onFeatureChanged = 为了吾王_OnFeatureChanged,
        .onValueChanged = 为了吾王_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = 为了吾王_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
