/*
 * BattleRole 秒杀菜单 (Bilingual Mod Menu)
 * 基于 Android-Mod-Menu 模板 (LGLTeam/Android-Mod-Menu)
 * 历史参考，不参与星光菜单构建；依赖旧 Android-Mod-Menu 头文件，不能直接编译。
 *
 * 目标: libil2cpp.so  (il2cpp 游戏)
 * 关键类: Game.Battle.BattleManager / Game.Battle.BattleRole
 *
 * 关键 RVA (arm64):
 *   GetPlayer    RVA = 0x243AD9C  (仅 8 字节! 不可 Dobby hook)
 *   GetEnemy     RVA = 0x243ADA4  (138 字节)
 *   CalcDamage   RVA = 0x243AEDC  (静态方法)
 *   DoBattleLoop RVA = 0x24383BC  (协程, 不 hook)
 *
 * ! 安全方案: 不 hook 超小函数, 改用 il2cpp API 动态解析字段偏移,
 *   通过 BattleManager.Instance 在后台线程直接读写 player/enemy.
 *   使用 il2cpp_field_static_get_value 安全读取静态字段, 不依赖
 *   Il2CppClass->static_fields 的硬编码偏移 (避免崩溃).
 */

#include <list>
#include <vector>
#include <cstring>
#include <pthread.h>
#include <thread>
#include <cstring>
#include <string>
#include <jni.h>
#include <unistd.h>
#include <fstream>
#include <iostream>
#include <dlfcn.h>
#include "Includes/Logger.h"
#include "Includes/obfuscate.h"
#include "Includes/Utils.hpp"
#include "Menu/Menu.hpp"
#include "Menu/Jni.hpp"
#include "Includes/Macros.h"
#include "dobby.h"

// ============================================================
//  地址定义
// ============================================================
#define targetLibName OBFUSCATE("libil2cpp.so")

// ============================================================
//  il2cpp API 函数指针 (运行时通过 dlsym 解析)
// ============================================================
typedef void* (*il2cpp_class_from_name_t)(void* image, const char* ns, const char* name);
typedef void* (*il2cpp_class_get_field_from_name_t)(void* klass, const char* name);
typedef size_t (*il2cpp_field_get_offset_t)(void* field);
typedef void* (*il2cpp_domain_get_t)(void);
typedef void** (*il2cpp_domain_get_assemblies_t)(void* domain, size_t* size);
typedef void (*il2cpp_field_static_get_value_t)(void* field, void* value);
typedef const void* (*il2cpp_assembly_get_image_t)(const void* assembly);
typedef void* (*il2cpp_class_get_static_field_data_t)(void* klass);

static il2cpp_class_from_name_t             p_il2cpp_class_from_name = nullptr;
static il2cpp_class_get_field_from_name_t   p_il2cpp_class_get_field_from_name = nullptr;
static il2cpp_field_get_offset_t            p_il2cpp_field_get_offset = nullptr;
static il2cpp_domain_get_t                  p_il2cpp_domain_get = nullptr;
static il2cpp_domain_get_assemblies_t       p_il2cpp_domain_get_assemblies = nullptr;
static il2cpp_field_static_get_value_t      p_il2cpp_field_static_get_value = nullptr;
static il2cpp_assembly_get_image_t         p_il2cpp_assembly_get_image = nullptr;
static il2cpp_class_get_static_field_data_t p_il2cpp_class_get_static_field_data = nullptr;

// ============================================================
//  运行时解析的字段偏移 (全部动态获取)
// ============================================================
struct FieldOffsets {
    size_t bm_player;
    size_t bm_enemy;
    size_t br_maxHp;
    size_t br_hpNow;
    size_t br_atk;
    size_t br_def;
    size_t br_spd;
    size_t br_hit;
    size_t br_dodge;
    size_t br_crit;
    size_t br_anticrit;
    size_t br_fixedAtk;
    size_t br_fixedDef;
    size_t br_addAtk;
    size_t br_reduceAtk;
    size_t br_critDmg;
    size_t br_skillDmg;
    size_t br_critDmgReduce;
    size_t br_spellResist;
    size_t br_xianyuanAtk;
    size_t br_xianyuanGuard;
    size_t br_shenweiChance;
    size_t br_shenweiAmp;
    bool   resolved;
} g_offsets = {0};

// 缓存 BattleManager class 和 Instance FieldInfo
static void* g_bmClass = nullptr;
static void* g_bmInstanceField = nullptr;
static size_t g_bmInstanceStaticOffset = 0;
// 静态字段读取方式: 0=未设置, 1=field_static_get_value, 2=class_get_static_field_data, 3=硬编码fallback
static int g_staticReadMode = 0;

// 便捷读写
static inline double ReadDouble(void* obj, size_t offset) {
    return *reinterpret_cast<double*>(reinterpret_cast<uintptr_t>(obj) + offset);
}

static inline void WriteDouble(void* obj, size_t offset, double value) {
    *reinterpret_cast<double*>(reinterpret_cast<uintptr_t>(obj) + offset) = value;
}

static inline void* ReadPtr(void* obj, size_t offset) {
    return *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(obj) + offset);
}

// 指针有效性检查 (arm64 Android, 考虑 MTE 标签)
static inline bool IsValidPtr(void* ptr) {
    if (!ptr) return false;
    uintptr_t p = (uintptr_t)ptr;
    // 剥离 MTE 标签 (高位字节)
    p = p & 0x00FFFFFFFFFFFFFFULL;
    // arm64 用户空间堆指针范围
    if (p < 0x1000) return false;
    if (p > 0x7fffffffffffULL) return false;
    return true;
}

// ============================================================
//  解析 il2cpp API 并获取所有字段偏移
// ============================================================
bool ResolveIl2cppOffsets() {
    void* handle = dlopen("libil2cpp.so", RTLD_NOW);
    if (!handle) {
        LOGI(OBFUSCATE("dlopen libil2cpp.so failed / dlopen 失败"));
        return false;
    }

    p_il2cpp_class_from_name = (il2cpp_class_from_name_t)dlsym(handle, "il2cpp_class_from_name");
    p_il2cpp_class_get_field_from_name = (il2cpp_class_get_field_from_name_t)dlsym(handle, "il2cpp_class_get_field_from_name");
    p_il2cpp_field_get_offset = (il2cpp_field_get_offset_t)dlsym(handle, "il2cpp_field_get_offset");
    p_il2cpp_domain_get = (il2cpp_domain_get_t)dlsym(handle, "il2cpp_domain_get");
    p_il2cpp_domain_get_assemblies = (il2cpp_domain_get_assemblies_t)dlsym(handle, "il2cpp_domain_get_assemblies");
    p_il2cpp_field_static_get_value = (il2cpp_field_static_get_value_t)dlsym(handle, "il2cpp_field_static_get_value");
    p_il2cpp_assembly_get_image = (il2cpp_assembly_get_image_t)dlsym(handle, "il2cpp_assembly_get_image");
    p_il2cpp_class_get_static_field_data = (il2cpp_class_get_static_field_data_t)dlsym(handle, "il2cpp_class_get_static_field_data");

    if (!p_il2cpp_class_from_name || !p_il2cpp_class_get_field_from_name ||
        !p_il2cpp_field_get_offset || !p_il2cpp_domain_get || !p_il2cpp_domain_get_assemblies) {
        LOGI(OBFUSCATE("il2cpp core API resolve failed / il2cpp 核心 API 解析失败"));
        return false;
    }

    if (p_il2cpp_field_static_get_value) {
        g_staticReadMode = 1;
        LOGI(OBFUSCATE("Using il2cpp_field_static_get_value / 使用安全API读取静态字段"));
    } else if (p_il2cpp_class_get_static_field_data) {
        g_staticReadMode = 2;
        LOGI(OBFUSCATE("Using il2cpp_class_get_static_field_data / 使用class API读取静态字段"));
    } else {
        g_staticReadMode = 3;
        LOGI(OBFUSCATE("WARNING: Using hardcoded static_fields offset! / 警告: 使用硬编码偏移!"));
    }

    // 遍历所有 assembly 找到 Assembly-CSharp image
    void* domain = p_il2cpp_domain_get();
    size_t asmCount = 0;
    void** assemblies = p_il2cpp_domain_get_assemblies(domain, &asmCount);
    if (!assemblies || asmCount == 0) {
        LOGI(OBFUSCATE("No assemblies found / 未找到程序集"));
        return false;
    }

    void* targetImage = nullptr;
    for (size_t i = 0; i < asmCount; i++) {
        void* assembly = assemblies[i];
        if (!assembly) continue;

        void* image = nullptr;
        if (p_il2cpp_assembly_get_image) {
            image = const_cast<void*>(p_il2cpp_assembly_get_image(assembly));
        } else {
            image = ReadPtr(assembly, 0x8);
        }
        if (!image) continue;

        void* testClass = p_il2cpp_class_from_name(image, "Game.Battle", "BattleManager");
        if (testClass) {
            targetImage = image;
            g_bmClass = testClass;
            LOGI(OBFUSCATE("Found Assembly-CSharp image / 找到目标程序集"));
            break;
        }
    }

    if (!targetImage || !g_bmClass) {
        LOGI(OBFUSCATE("Assembly-CSharp image not found / 未找到目标程序集映像"));
        return false;
    }

    void* brClass = p_il2cpp_class_from_name(targetImage, "Game.Battle", "BattleRole");
    if (!brClass) {
        LOGI(OBFUSCATE("BattleRole class not found / 未找到 BattleRole 类"));
        return false;
    }

    auto getOffset = [](void* klass, const char* fieldName) -> size_t {
        void* field = p_il2cpp_class_get_field_from_name(klass, fieldName);
        if (!field) {
            LOGI(OBFUSCATE("Field not found: %s"), fieldName);
            return 0;
        }
        return p_il2cpp_field_get_offset(field);
    };

    // 缓存 Instance FieldInfo
    g_bmInstanceField = p_il2cpp_class_get_field_from_name(g_bmClass, "Instance");
    if (g_bmInstanceField) {
        g_bmInstanceStaticOffset = p_il2cpp_field_get_offset(g_bmInstanceField);
    } else {
        LOGI(OBFUSCATE("WARNING: Instance FieldInfo not found! / 警告: 未找到 Instance 字段!"));
    }

    // BattleManager 实例字段偏移
    g_offsets.bm_player   = getOffset(g_bmClass, "player");
    g_offsets.bm_enemy    = getOffset(g_bmClass, "enemy");

    // BattleRole 字段偏移
    g_offsets.br_maxHp         = getOffset(brClass, "maxHp");
    g_offsets.br_hpNow         = getOffset(brClass, "hpNow");
    g_offsets.br_atk           = getOffset(brClass, "atk");
    g_offsets.br_def           = getOffset(brClass, "def");
    g_offsets.br_spd           = getOffset(brClass, "spd");
    g_offsets.br_hit           = getOffset(brClass, "hit");
    g_offsets.br_dodge         = getOffset(brClass, "dodge");
    g_offsets.br_crit          = getOffset(brClass, "crit");
    g_offsets.br_anticrit      = getOffset(brClass, "anticrit");
    g_offsets.br_fixedAtk      = getOffset(brClass, "fixedAtk");
    g_offsets.br_fixedDef      = getOffset(brClass, "fixedDef");
    g_offsets.br_addAtk       = getOffset(brClass, "addAtk");
    g_offsets.br_reduceAtk    = getOffset(brClass, "reduceAtk");
    g_offsets.br_critDmg       = getOffset(brClass, "critDmg");
    g_offsets.br_skillDmg      = getOffset(brClass, "skillDmg");
    g_offsets.br_critDmgReduce = getOffset(brClass, "critDmgReduce");
    g_offsets.br_spellResist   = getOffset(brClass, "spellResist");
    g_offsets.br_xianyuanAtk   = getOffset(brClass, "xianyuanAtk");
    g_offsets.br_xianyuanGuard = getOffset(brClass, "xianyuanGuard");
    g_offsets.br_shenweiChance = getOffset(brClass, "shenweiChance");
    g_offsets.br_shenweiAmp    = getOffset(brClass, "shenweiAmp");

    g_offsets.resolved = true;

    LOGI(OBFUSCATE("Offsets resolved! mode=%d / 偏移解析完成!"), g_staticReadMode);
    LOGI(OBFUSCATE("BM: player=%zu enemy=%zu"), g_offsets.bm_player, g_offsets.bm_enemy);
    LOGI(OBFUSCATE("BR: maxHp=%zu hpNow=%zu atk=%zu def=%zu"),
         g_offsets.br_maxHp, g_offsets.br_hpNow, g_offsets.br_atk, g_offsets.br_def);

    return true;
}

// ============================================================
//  获取 BattleManager.Instance (静态字段)
//  使用 il2cpp_field_static_get_value 安全读取, 不依赖
//  Il2CppClass->static_fields 的硬编码偏移
// ============================================================
void* GetBattleManagerInstance() {
    if (!g_bmClass) return nullptr;

    void* instance = nullptr;

    if (g_staticReadMode == 1 && p_il2cpp_field_static_get_value && g_bmInstanceField) {
        // 方式1: il2cpp_field_static_get_value (最安全)
        p_il2cpp_field_static_get_value(g_bmInstanceField, &instance);
    } else if (g_staticReadMode == 2 && p_il2cpp_class_get_static_field_data) {
        // 方式2: il2cpp_class_get_static_field_data
        void* static_fields = p_il2cpp_class_get_static_field_data(g_bmClass);
        if (IsValidPtr(static_fields) && g_bmInstanceStaticOffset > 0) {
            instance = ReadPtr(static_fields, g_bmInstanceStaticOffset);
        }
    } else {
        // 方式3: 硬编码偏移 fallback (不推荐)
        static const size_t candidateOffsets[] = {0xB8, 0xC0, 0x88, 0xB0};
        for (size_t off : candidateOffsets) {
            void* sf = ReadPtr(g_bmClass, off);
            if (IsValidPtr(sf) && g_bmInstanceStaticOffset > 0) {
                void* candidate = ReadPtr(sf, g_bmInstanceStaticOffset);
                if (IsValidPtr(candidate)) {
                    instance = candidate;
                    break;
                }
            }
        }
    }

    if (!IsValidPtr(instance)) return nullptr;
    return instance;
}

// ============================================================
//  全局开关
// ============================================================
bool g_oneHitKill    = false;
bool g_godMode       = false;
bool g_highAttack    = false;
bool g_zeroDef       = false;
bool g_critAlways    = false;
bool g_highSpeed     = false;
bool g_highHp        = false;

// ============================================================
//  菜单功能列表 (中英双语)
// ============================================================
jobjectArray GetFeatureList(JNIEnv *env, jobject context) {
    jobjectArray ret;

    const char *features[] = {
            OBFUSCATE("Category_战斗修改 Battle Cheats"),
            OBFUSCATE("Toggle_秒杀 (敌方血量归零) One Hit Kill"),
            OBFUSCATE("Toggle_无敌 (血量锁定满) God Mode"),
            OBFUSCATE("Toggle_高攻 (攻击力 999M) High Attack"),
            OBFUSCATE("Toggle_破防 (敌方防御归零) Zero Enemy Defense"),
            OBFUSCATE("Toggle_必暴击 (暴击率拉满) Always Critical Hit"),
            OBFUSCATE("Toggle_神速 (速度 999M) High Speed"),
            OBFUSCATE("Toggle_巨血 (血量 999M) High Max HP"),

            OBFUSCATE("Category_实用工具 Utils"),
            OBFUSCATE("Button_立即秒杀敌人 Force Kill Enemy Now"),
//            OBFUSCATE("Button_满血恢复玩家 Full Heal Player"),
//            OBFUSCATE("Button_重置所有功能 Reset All Cheats"),

//            OBFUSCATE("RichTextView_<b>BattleRole 秒杀菜单 Mod Menu</b><br/>"
//                      "基于 Android-Mod-Menu 模板<br/>"
//                      "Target: <font color='red'>libil2cpp.so</font><br/>"
//                      "il2cpp API 动态偏移 / Dynamic Offsets<br/>"
//                      "安全静态字段读取 / Safe Static Field Read"),
    };

    int Total_Feature = (sizeof features / sizeof features[0]);
    ret = (jobjectArray)
            env->NewObjectArray(Total_Feature, env->FindClass(OBFUSCATE("java/lang/String")),
                                env->NewStringUTF(""));

    for (int i = 0; i < Total_Feature; i++)
        env->SetObjectArrayElement(ret, i, env->NewStringUTF(features[i]));

    return (ret);
}

// 按钮触发标志
bool btnForceKill  = false;
bool btnFullHeal   = false;
bool btnResetStats = false;

// ============================================================
//  菜单回调
// ============================================================
void Changes(JNIEnv *env, jclass clazz, jobject obj, jint featNum, jstring featName, jint value, jlong Lvalue, jboolean boolean, jstring text) {

    switch (featNum) {
        case 0:
            g_oneHitKill = boolean;
            if (boolean) LOGI(OBFUSCATE("One Hit Kill: ON / 秒杀: 已开启"));
            break;
        case 1:
            g_godMode = boolean;
            break;
        case 2:
            g_highAttack = boolean;
            break;
        case 3:
            g_zeroDef = boolean;
            break;
        case 4:
            g_critAlways = boolean;
            break;
        case 5:
            g_highSpeed = boolean;
            break;
        case 6:
            g_highHp = boolean;
            break;
        case 7:
            btnForceKill = true;
            break;
        case 8:
            btnFullHeal = true;
            break;
        case 9:
            btnResetStats = true;
            break;
        default:
            break;
    }
}

// ============================================================
//  按钮处理
// ============================================================
void ProcessButtons() {
    if (!g_offsets.resolved) return;
    void* bm = GetBattleManagerInstance();
    if (!IsValidPtr(bm)) return;

    void* player = ReadPtr(bm, g_offsets.bm_player);
    void* enemy = ReadPtr(bm, g_offsets.bm_enemy);

    if (btnForceKill && IsValidPtr(enemy)) {
        WriteDouble(enemy, g_offsets.br_hpNow, 0.0);
        LOGI(OBFUSCATE("Force killed enemy / 已秒杀敌人"));
        btnForceKill = false;
    }
    if (btnFullHeal && IsValidPtr(player)) {
        double maxHp = ReadDouble(player, g_offsets.br_maxHp);
        WriteDouble(player, g_offsets.br_hpNow, maxHp);
        LOGI(OBFUSCATE("Player fully healed / 玩家已满血恢复"));
        btnFullHeal = false;
    }
    if (btnResetStats) {
        g_oneHitKill = false;
        g_godMode = false;
        g_highAttack = false;
        g_zeroDef = false;
        g_critAlways = false;
        g_highSpeed = false;
        g_highHp = false;
        btnResetStats = false;
        LOGI(OBFUSCATE("All cheats reset / 已重置所有功能"));
    }
}

// ============================================================
//  后台线程: 持续修改属性 (所有指针经 IsValidPtr 验证)
// ============================================================
void mod_thread() {
    while (!g_offsets.resolved) {
        sleep(1);
    }
    LOGI(OBFUSCATE("mod_thread started / 后台线程启动"));

    int logCounter = 0;
    while (true) {
        void* bm = GetBattleManagerInstance();
        if (IsValidPtr(bm)) {
            void* player = ReadPtr(bm, g_offsets.bm_player);
            void* enemy = ReadPtr(bm, g_offsets.bm_enemy);

            if (IsValidPtr(player)) {
                if (g_godMode) {
                    double maxHp = ReadDouble(player, g_offsets.br_maxHp);
                    WriteDouble(player, g_offsets.br_hpNow, maxHp);
                }
                if (g_highAttack) {
                    WriteDouble(player, g_offsets.br_atk, 999999999.0);
                    WriteDouble(player, g_offsets.br_fixedAtk, 999999999.0);
                    WriteDouble(player, g_offsets.br_addAtk, 999999999.0);
                }
                if (g_highSpeed) {
                    WriteDouble(player, g_offsets.br_spd, 999999999.0);
                }
                if (g_highHp) {
                    WriteDouble(player, g_offsets.br_maxHp, 999999999.0);
                    double maxHp = ReadDouble(player, g_offsets.br_maxHp);
                    WriteDouble(player, g_offsets.br_hpNow, maxHp);
                }
                if (g_critAlways) {
                    WriteDouble(player, g_offsets.br_crit, 100.0);
                    WriteDouble(player, g_offsets.br_critDmg, 999.0);
                    WriteDouble(player, g_offsets.br_skillDmg, 999.0);
                    WriteDouble(player, g_offsets.br_hit, 100.0);
                    WriteDouble(player, g_offsets.br_shenweiChance, 100.0);
                    WriteDouble(player, g_offsets.br_shenweiAmp, 999.0);
                    WriteDouble(player, g_offsets.br_xianyuanAtk, 999.0);
                }
            }

            if (IsValidPtr(enemy)) {
                if (g_oneHitKill) {
                    WriteDouble(enemy, g_offsets.br_hpNow, 0.0);
                }
                if (g_zeroDef) {
                    WriteDouble(enemy, g_offsets.br_def, 0.0);
                    WriteDouble(enemy, g_offsets.br_fixedDef, 0.0);
                    WriteDouble(enemy, g_offsets.br_dodge, 0.0);
                    WriteDouble(enemy, g_offsets.br_anticrit, 0.0);
                    WriteDouble(enemy, g_offsets.br_critDmgReduce, 0.0);
                    WriteDouble(enemy, g_offsets.br_spellResist, 0.0);
                    WriteDouble(enemy, g_offsets.br_xianyuanGuard, 0.0);
                    WriteDouble(enemy, g_offsets.br_reduceAtk, 0.0);
                }
                if (g_critAlways) {
                    WriteDouble(enemy, g_offsets.br_anticrit, 0.0);
                    WriteDouble(enemy, g_offsets.br_critDmgReduce, 0.0);
                    WriteDouble(enemy, g_offsets.br_dodge, 0.0);
                }
            }

            if (++logCounter >= 300) {
                logCounter = 0;
                LOGI(OBFUSCATE("Mod running: bm=%p player=%p enemy=%p / 运行中"),
                     bm, player, enemy);
            }
        }

        ProcessButtons();
        usleep(100000);
    }
}

// ============================================================
//  主 Hack 线程
// ============================================================
void hack_thread() {
    while (!isLibraryLoaded(targetLibName)) {
        sleep(1);
    }

    LOGI(OBFUSCATE("libil2cpp.so loaded / libil2cpp.so 已加载"));

    sleep(3);

    if (!ResolveIl2cppOffsets()) {
        LOGI(OBFUSCATE("Failed to resolve offsets! / 偏移解析失败!"));
        return;
    }

    std::thread(mod_thread).detach();

    LOGI(OBFUSCATE("Mod Menu ready! / 秒杀菜单就绪!"));
}

// ============================================================
//  入口
// ============================================================
__attribute__((constructor))
void lib_main() {
    LOGI(OBFUSCATE("BattleRole Mod Menu loaded / 秒杀菜单已加载"));
    std::thread(hack_thread).detach();
}
