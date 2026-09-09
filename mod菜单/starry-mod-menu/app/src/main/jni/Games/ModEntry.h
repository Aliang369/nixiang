/* ================================================================
 *  ModEntry.h — 游戏模块注册表接口（modern 入口合同）
 *
 *  配套 MainModern.cpp（注册表模式入口）。双轨说明：
 *    - Main.cpp     = legacy 入口：#if defined(STARRY_GAME_*) 堆叠，存量游戏用
 *    - MainModern.cpp = modern 入口：只认本接口，新游戏一律走这里
 *
 *  接游戏的标准动作（MainModern.cpp / Main.cpp / GameConfig.h 均零修改）：
 *    1. Games/{Game}Mod.h/.cpp   — 游戏逻辑（骨架 _TemplateMod）
 *    2. Games/{Game}Entry.cpp    — 填一张 StarryGameMod 表（菜单/参数/回调）
 *    3. Android_{tag}.mk         — 私有构建文件（模板 Android_modern_template.mk）
 *    4. 构建传 NDK_OUT=app/src/obj_{tag}/ 独立 obj（防并行会话互踩）
 *
 *  铁律：
 *    - 回调全部可选（nullptr = 框架跳过）；不用的留 nullptr，不要造假实现
 *    - 修改框架需求 → 先改本接口并评审，不在 MainModern.cpp 加游戏分支
 *    - 菜单项格式约定与 legacy 一致：
 *      Category_/Toggle_/Button_/Slider_标签:最小:最大/Spinner_标签:选项,/
 *      InputText_标签/InputValue_默认值:标签/CheckBox_/SeekBar_标签_最小_最大[_默认]
 *      featNum = 数组下标（Category_ 占下标，仅供 UI 分组）
 * ================================================================
 */
#ifndef STARRY_MOD_ENTRY_H
#define STARRY_MOD_ENTRY_H

#include <jni.h>

struct StarryGameMod {
    // ---- 身份 ----
    const char* tag;         // 日志/调试标签，如 "cwjh"
    const char* authGameId;  // 鉴权 game_id（后台 games.game_id / 卡密 scope 同名）

    // ---- 菜单 ----
    const char* const* featureList;  // 菜单项数组（见头部格式约定）
    int featureCount;                // = sizeof(featureList)/sizeof(元素)

    // ---- 目标库 ----
    const char* waitLib;  // 框架启动时轮询等待的目标库名（nullptr = 纯壳不等待）

    // ---- 初始化策略（替代 legacy Main.cpp 里每游戏一份的轮询参数）----
    int initDelayMs;         // 等库命中后的延迟（避开 il2cpp/HybridCLR 半初始化窗口；0 = 立即）
    int initRetryMax;        // 启动期 tryInit 重试上限（0 = 禁止启动期 init，Tick 懒加载，如 gjc2 死锁案）
    int initRetryIntervalMs; // 重试间隔
    int tickIntervalMs;      // Tick 主循环节流（<=0 时框架按 200ms）

    // ---- 生命周期回调（全部可选；nullptr = 框架跳过）----
    void (*onLoad)(JavaVM* vm);  // JNI_OnLoad / CaveBoot 时机：EarlyHook、过检补丁、菜单 bootstrap
    bool (*tryInit)(JavaVM* vm); // 挂点解析/安装（Unity 系可忽略 vm；Cocos 系需要）
    bool (*isReady)();           // 模块就绪（无状态游戏可恒 true）
    void (*tick)(const bool* features, const int* values);  // 主循环心跳（只做同步+兜底，禁重活）
    void (*onFeatureChanged)(int featNum, bool enabled);    // 开关（按钮也走这里，enabled=true）
    void (*onValueChanged)(int featNum, int value);         // 滑块/下拉
    void (*onInputChanged)(int featNum, const char* text);  // 文本输入（InputText/InputValue）
    void (*forceClear)();                                    // 授权失效时清游戏侧状态（关全部 hook 效果）
    void (*onApplyPlayPatch)(const char* json);              // OTA 玩法偏移热更（无此能力留 nullptr）
};

// 由 Games/{Game}Entry.cpp 提供唯一实现；MainModern.cpp 只经此访问游戏
extern "C" const StarryGameMod* const GameMod_Get();

#endif  // STARRY_MOD_ENTRY_H
