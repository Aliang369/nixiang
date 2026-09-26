/* ================================================================
 *  星光魔法 MOD 菜单 — 通用游戏 Hook 框架
 *  Starry Magic Mod Menu — Universal Game Hook Framework
 *
 *  设计理念：
 *  - 不绑定任何特定游戏引擎
 *  - 自动检测游戏类型（Unity/Unreal/Cocos/自研引擎）
 *  - 插件式 Hook 模块，按需加载
 *  - 统一的功能列表接口，C 层定义菜单结构
 *  - WebView 渲染 UI，100% 还原设计稿
 *
 *  架构：
 *    HTML/JS ←→ ModBridge.java ←→ Main.cpp (JNI)
 *                                       ├─ PluginUnity.cpp    (il2cpp Hook)
 *                                       ├─ PluginGeneric.cpp  (内存读写/Patch)
 *                                       └─ PluginUnreal.cpp   (未来扩展)
 *
 *  工具链（从 LGL 项目继承，已验证可用）：
 *    Dobby      — 函数 Hook（inline hook）
 *    KittyMemory — 内存 Patch（hex/asm）
 *    xDL        — 库地址/符号查找（替代 dlopen+dlsym）
 *    Keystone   — 运行时汇编引擎
 *
 *  作者：阿亮
 *  日期：2026-08-07
 * ================================================================
 */

#include <jni.h>
#include <string.h>
#include <stdlib.h>
#include <pthread.h>
#include <unistd.h>
#include <dlfcn.h>
#include <time.h>
#include <android/log.h>
#include <sys/mman.h>          // mprotect (P-04)

// ===== 工具链 =====
#include "Includes/Logger.h"
#include "CertPinned.h"        // 证书固定公钥（编译期烘焙，AuthClient 经桥接读取）
#include "Includes/Utils.hpp"
#include "Includes/StarryStamp.h"   // 构建戳（OTA 守卫判包内 so 与 OTA so 新旧）
#include "Includes/SecStrings.hpp"

#include "Includes/Macros.h"   // HOOK / PATCH / RESTORE 宏
#include "Includes/obfuscate.h"
#include "Includes/hmac_sha256.h"
#include "Dobby/dobby.h"       // DobbyCodePatch (P-05，明确依赖)

#include "xDL/xdl.h"

// ===== 游戏适配配置（适配新游戏时只改这个文件） =====
#include "GameConfig.h"

#ifdef ENABLE_COCOS_HOOK
#if defined(STARRY_GAME_CWJH)
bool Cwjh_IsReady();
void Cwjh_ForceClear();
#elif defined(STARRY_GAME_ZXR)
bool Zxr_IsReady();
void Zxr_ForceClear();
#elif defined(STARRY_GAME_SC33)
bool Sc33_IsReady();
void Sc33_ForceClear();
#elif defined(STARRY_GAME_ZMXY)
bool Zmxy_IsReady();
void Zmxy_ForceClear();
#elif defined(STARRY_GAME_SHHJ)
bool Shhj_IsReady();
void Shhj_ForceClear();
#elif defined(STARRY_GAME_QLFH)
bool Qlfh_IsReady();
void Qlfh_ForceClear();
#elif defined(STARRY_GAME_ZMWS)
bool Zmws_IsReady();
void Zmws_ForceClear();
#elif defined(STARRY_GAME_LTZJ)
bool Ltzj_IsReady();
void Ltzj_ForceClear();
#elif defined(STARRY_GAME_DWCT)
bool Dwct_IsReady();
#elif defined(STARRY_GAME_HALY)
bool Haly_IsReady();
void Haly_ForceClear();
#else
bool Blzz_IsReady();
void Blzz_ForceClear();
#endif
#endif
#ifdef ENABLE_UNITY_HOOK
#if defined(STARRY_GAME_HZSJ)
bool Hzsj_IsReady();
bool Hzsj_TryInit();
void Hzsj_ForceClear();
void Hzsj_ApplyPlayPatch(const char* json);
#elif defined(STARRY_GAME_ZJMC)
bool Zjmc_IsReady();
bool Zjmc_TryInit();
void Zjmc_ForceClear();
#elif defined(STARRY_GAME_TLGD)
bool Tlgd_IsReady();
bool Tlgd_TryInit();
void Tlgd_ForceClear();
#elif defined(STARRY_GAME_DZQS)
bool Dzqs_IsReady();
bool Dzqs_TryInit();
void Dzqs_ForceClear();
#elif defined(STARRY_GAME_CSTJJS)
bool Cstjjs_IsReady();
bool Cstjjs_TryInit();
void Cstjjs_ForceClear();
#elif defined(STARRY_GAME_NEON)
bool Neon_IsReady();
void Neon_ForceClear();
#elif defined(STARRY_GAME_WHRHX)
bool Whrhx_IsReady();
bool Whrhx_TryInit();
void Whrhx_ForceClear();
void Whrhx_OnValueChanged(int featNum, int value);
#elif defined(STARRY_GAME_WHRHXKB)
bool Whrhx_IsReady();
bool Whrhx_TryInit();
void Whrhx_ForceClear();
void Whrhx_OnValueChanged(int featNum, int value);
#elif defined(STARRY_GAME_WHRHXKB)
bool Whrhx_IsReady();
bool Whrhx_TryInit();
void Whrhx_ForceClear();
void Whrhx_OnValueChanged(int featNum, int value);
#elif defined(STARRY_GAME_WGDZZ)
bool Wgdzz_IsReady();
bool Wgdzz_TryInit();
void Wgdzz_ForceClear();
void Wgdzz_StartEarlyHooks(JavaVM* vm);
void Wgdzz_StartMenuBootstrap(JavaVM* vm);
#elif defined(STARRY_GAME_BJDW)
bool Bjdw_IsReady();
bool Bjdw_TryInit();
void Bjdw_ForceClear();
void Bjdw_OnFeatureChanged(int featNum, bool enabled);
#elif defined(STARRY_GAME_ENDFIELD)
bool Endfield_IsReady();
bool Endfield_TryInit();
void Endfield_ForceClear();
#elif defined(STARRY_GAME_WDYZ)
bool Wdyz_IsReady();
bool Wdyz_TryInit();
void Wdyz_ForceClear();
void Wdyz_OnFeatureChanged(int featNum, bool enabled);
void Wdyz_OnValueChanged(int featNum, int value);
void Wdyz_Tick(const bool* features);
#elif defined(STARRY_GAME_WXSJ)
bool Wxsj_IsReady();
bool Wxsj_TryInit();
void Wxsj_ForceClear();
void Wxsj_OnFeatureChanged(int featNum, bool enabled);
void Wxsj_OnValueChanged(int featNum, int value);
#elif defined(STARRY_GAME_YQZD)
void Yqzd_OnFeatureChanged(int featNum, bool enabled);
void Yqzd_OnValueChanged(int featNum, int value);
#elif defined(STARRY_GAME_WGGLS)
bool Wggls_IsReady();
bool Wggls_TryInit();
void Wggls_ForceClear();
#elif defined(STARRY_GAME_MILTHM)
bool Milthm_IsReady();
void Milthm_ForceClear();
#elif defined(STARRY_GAME_YWGN)
bool Ywgn_IsReady();
void Ywgn_ForceClear();
void Ywgn_SecurityTick();
bool Ywgn_TryInit();
#elif defined(STARRY_GAME_PHIGROS)
bool Phigros_IsReady();
void Phigros_ForceClear();
void Phigros_SecurityTick();
bool Phigros_TryInit();
void Phigros_OnFeatureChanged(int featNum, bool enabled);
void Phigros_Tick(const bool* features);
#elif defined(STARRY_GAME_ES2)
bool Es2_IsReady();
bool Es2_TryInit();
void Es2_ForceClear();
void Es2_OnFeatureChanged(int featNum, bool enabled);
void Es2_OnValueChanged(int featNum, int value);
void Es2_Tick(const bool* features);
#elif defined(STARRY_GAME_MATCH2)
bool Match2_IsReady();
void Match2_ForceClear();
#elif defined(STARRY_GAME_ZMXS)
bool Zmxs_IsReady();
void Zmxs_ForceClear();
#elif defined(STARRY_GAME_WZYM)
bool Wzym_IsReady();
void Wzym_ForceClear();
#elif defined(STARRY_GAME_PCR)
bool Pcr_IsReady();
bool Pcr_TryInit();
void Pcr_ForceClear();
void Pcr_OnInputChanged(int featNum, const char* text);
#elif defined(STARRY_GAME_GYWS)
bool Gyws_IsReady();
bool Gyws_TryInit();
void Gyws_ForceClear();
#elif defined(STARRY_GAME_GJC2)
bool Gjc2_IsReady();
bool Gjc2_TryInit();
void Gjc2_ForceClear();
#elif defined(STARRY_GAME_GJC2INTL)
bool Gjc2Intl_IsReady();
bool Gjc2Intl_TryInit();
void Gjc2Intl_ForceClear();
#elif defined(STARRY_GAME_SHLT)
bool Shlt_IsReady();
bool Shlt_TryInit();
void Shlt_ForceClear();
#elif defined(STARRY_GAME_ZHMR)
bool Zhmr_IsReady();
bool Zhmr_TryInit();
void Zhmr_ForceClear();
#endif
#endif

#define TAG "StarryMagic"
#undef TAG
#define TAG "StarryMagic"

// ============================================================
//  全局状态
// ============================================================

static JavaVM* g_jvm = nullptr;
static jobject g_context = nullptr;
static bool g_initialized = false;
static bool g_gameLibLoaded = false;

// 游戏引擎类型
enum GameEngine {
    ENGINE_UNKNOWN = 0,
    ENGINE_UNITY   = 1,
    ENGINE_UNREAL  = 2,
    ENGINE_COCOS   = 3,
    ENGINE_CUSTOM  = 4,
};
static GameEngine g_engine = ENGINE_UNKNOWN;

// 目标库名（自动检测或手动配置）
static const char* g_targetLib = nullptr;

// ===== 功能开关（通用） =====
static bool g_features[64] = {false};  // 最多64个开关
static int  g_values[64]   = {0};      // 滑块/下拉值
static char g_texts[64][256] = {{0}};   // 输入框文本
static bool g_authorized   = false;    // 诱饵开关（仅 JS/Java 可改，不单独放行功能）
static bool g_rt_ok        = false;    // 服务端短票验签通过
// g_rt_exp 已废弃（session_live 不再检查过期时间）

// ===== 按钮触发标志 =====
static bool g_btnTrigger[64] = {false};

// 须与 auth-server GRANT_HMAC_SECRET / grant_hmac_secret 默认一致
static const char* grant_pepper() {
    return OBFUSCATE("sg-grant-v1-7f3a9c2e");
}

static bool hex_eq_ci(const char* hex, const uint8_t* dig, size_t n) {
    static const char* lut = "0123456789abcdef";
    if (!hex || strlen(hex) < n * 2) return false;
    for (size_t i = 0; i < n; i++) {
        char a = hex[i * 2], b = hex[i * 2 + 1];
        if (a >= 'A' && a <= 'F') a = (char)(a - 'A' + 'a');
        if (b >= 'A' && b <= 'F') b = (char)(b - 'A' + 'a');
        if (a != lut[(dig[i] >> 4) & 0xf] || b != lut[dig[i] & 0xf]) return false;
    }
    return true;
}

/** 运行时会话：登录后永不过期，除非进程重启或 nativeClearGrant。
 *
 * 设计目标：用户登录一次，功能就一直可用，直到 App 重启。
 * 心跳只用于服务端状态同步（卡密信息、菜单门控），不影响功能放行。
 * 后台禁卡/删卡不会断已登录用户的功能。
 *
 * g_rt_ok 在 apply_grant_utf8() 成功后设为 true，
 * 只在 clear_runtime_session()（进程重启 / nativeClearGrant）时清零。
 */
static bool session_live() {
    return g_rt_ok;
}


static void clear_runtime_session() {
    g_rt_ok = false;
    g_authorized = false;
    for (int i = 0; i < 64; i++) {
        g_features[i] = false;
        g_btnTrigger[i] = false;
    }
#ifdef ENABLE_COCOS_HOOK
#if defined(STARRY_GAME_CWJH)
    if (Cwjh_IsReady()) {
        Cwjh_ForceClear();
    }
#elif defined(STARRY_GAME_ZXR)
    if (Zxr_IsReady()) {
        Zxr_ForceClear();
    }
#elif defined(STARRY_GAME_SC33)
    if (Sc33_IsReady()) {
        Sc33_ForceClear();
    }
#elif defined(STARRY_GAME_ZMXY)
    if (Zmxy_IsReady()) {
        Zmxy_ForceClear();
    }
#elif defined(STARRY_GAME_SHHJ)
    if (Shhj_IsReady()) {
        Shhj_ForceClear();
    }
#elif defined(STARRY_GAME_QLFH)
    if (Qlfh_IsReady()) {
        Qlfh_ForceClear();
    }
#elif defined(STARRY_GAME_ZMWS)
    if (Zmws_IsReady()) {
        Zmws_ForceClear();
    }
#elif defined(STARRY_GAME_LTZJ)
    if (Ltzj_IsReady()) {
        Ltzj_ForceClear();
    }
#elif defined(STARRY_GAME_DWCT)
    // Dwct 无 ForceClear：关授权时 g_features 已清零，Tick/SyncFlagsJs 会关掉 JS 门
#elif defined(STARRY_GAME_YQZD)
    // Yqzd 无 ForceClear：旗标推送归零即全部失效，无残留状态
#elif defined(STARRY_GAME_HALY)
    if (Haly_IsReady()) {
        Haly_ForceClear();
    }
#else
    if (Blzz_IsReady()) {
        Blzz_ForceClear();
    }
#endif
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_HZSJ)
    if (Hzsj_IsReady()) {
        Hzsj_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ZJMC)
    if (Zjmc_IsReady()) {
        Zjmc_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_TLGD)
    if (Tlgd_IsReady()) {
        Tlgd_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_DZQS)
    if (Dzqs_IsReady()) {
        Dzqs_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_CSTJJS)
    if (Cstjjs_IsReady()) {
        Cstjjs_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_NEON)
    if (Neon_IsReady()) {
        Neon_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WHRHX)
    if (Whrhx_IsReady()) {
        Whrhx_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WHRHXKB)
    if (Whrhx_IsReady()) {
        Whrhx_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WGDZZ)
    if (Wgdzz_IsReady()) {
        Wgdzz_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_BJDW)
    if (Bjdw_IsReady()) {
        Bjdw_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ENDFIELD)
    if (Endfield_IsReady()) {
        Endfield_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WDYZ)
    if (Wdyz_IsReady()) {
        Wdyz_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WXSJ)
    if (Wxsj_IsReady()) {
        Wxsj_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WGGLS)
    if (Wggls_IsReady()) {
        Wggls_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_MILTHM)
    if (Milthm_IsReady()) {
        Milthm_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_YWGN)
    if (Ywgn_IsReady()) {
        Ywgn_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_PHIGROS)
    if (Phigros_IsReady()) {
        Phigros_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ES2)
    if (Es2_IsReady()) {
        Es2_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_MATCH2)
    if (Match2_IsReady()) {
        Match2_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ZMXS)
    if (Zmxs_IsReady()) {
        Zmxs_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WZYM)
    if (Wzym_IsReady()) {
        Wzym_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_PCR)
    if (Pcr_IsReady()) {
        Pcr_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_GYWS)
    if (Gyws_IsReady()) {
        Gyws_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_GJC2)
    if (Gjc2_IsReady()) {
        Gjc2_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_GJC2INTL)
    if (Gjc2Intl_IsReady()) {
        Gjc2Intl_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_SHLT)
    if (Shlt_IsReady()) {
        Shlt_ForceClear();
    }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ZHMR)
    if (Zhmr_IsReady()) {
        Zhmr_ForceClear();
    }
#endif
}

/** grant 格式: 1|device|game|exp|menu|card|kid.hexsig */
static bool apply_grant_utf8(const char* grant, const char* device, const char* game) {
    if (!grant || !device || !game || !*grant) return false;
    const char* dot = strrchr(grant, '.');
    if (!dot || dot == grant || !*(dot + 1)) return false;
    size_t body_len = (size_t)(dot - grant);
    if (body_len < 8 || body_len > 512) return false;
    char body[520];
    memcpy(body, grant, body_len);
    body[body_len] = 0;
    const char* sig = dot + 1;

    uint8_t mac[32];
    const char* pepper = grant_pepper();
    hmac_sha256((const uint8_t*)pepper, strlen(pepper),
                (const uint8_t*)body, body_len, mac);
    if (!hex_eq_ci(sig, mac, 32)) return false;

    // parse fields
    char tmp[520];
    memcpy(tmp, body, body_len + 1);
    char* save = nullptr;
    char* ver = strtok_r(tmp, "|", &save);
    char* d = strtok_r(nullptr, "|", &save);
    char* g = strtok_r(nullptr, "|", &save);
    char* exp_s = strtok_r(nullptr, "|", &save);
    char* menu_s = strtok_r(nullptr, "|", &save);
    char* card_s = strtok_r(nullptr, "|", &save);
    char* kid = strtok_r(nullptr, "|", &save);
    if (!ver || !d || !g || !exp_s || !menu_s || !card_s || !kid) return false;
    if (strcmp(ver, "1") != 0) return false;
    if (strcmp(d, device) != 0 || strcmp(g, game) != 0) return false;
    int64_t exp = (int64_t)atoll(exp_s);
    int64_t now = (int64_t)time(nullptr);
    // 首次验签检查过期（登录后不过期，但首次不能接受已过期的票）
    if (now > exp + 45) return false;
    if (menu_s[0] == '0') return false; // 游戏停用票不放行

    LOGI("[验证] grant ok exp=%lld", (long long)exp);
    g_rt_ok = true;
    g_authorized = true; // 同步诱饵，避免只改诱饵造成错觉
    (void)card_s;
    (void)kid;
    return true;
}

// ============================================================
//  功能列表定义
//
//  功能列表定义格式约定:
//    "Category_分类名"              -> 分类标题（不占 featNum，仅供动态渲染）
//    "Toggle_功能名"                -> 开关
//    "Button_功能名"                -> 执行按钮
//    "Slider_标签:最小:最大"        -> 滑块
//    "Spinner_标签:选项1,选项2"     -> 下拉
//    "InputText_标签"               -> 文本输入
//    "InputValue_默认值:标签"       -> 数字输入
//
//  featNum = 数组下标, 从0开始
//  ⚠️ featNum 必须与 HTML 中 data-featnum 一一对应（P-01 修复）
//  修改这里时需同步修改 mod-menu-v2.html 的 data-featnum
// ============================================================

static const char* g_featureList[] = {
    // featNum = 数组下标；Category_ 仅用于 UI 分组
#if defined(STARRY_GAME_SELF)
    // 星光自测：覆盖原生菜单已实现的全部控件类型（无真实作弊逻辑）
    "Category_开关",                     // 0
    "Toggle_测试开关",                   // 1
    "CheckBox_测试勾选",                 // 2
    "Category_滑块",                     // 3
    "Slider_测试滑块:1:10",              // 4  冒号格式
    "SeekBar_测试滑轮_0_100",            // 5  下划线格式
    "Category_选择",                     // 6
    "Spinner_难度:简单,普通,困难",       // 7
    "Category_输入",                     // 8
    "InputText_备注",                    // 9
    "InputValue_100:数量",               // 10
    "Category_操作",                     // 11
    "Button_测试执行",                   // 12
#elif defined(STARRY_GAME_HZSJ)
    // 魂坠深境（HzsjMod.h）— 新手剧情勿开
    "Category_战斗",                     // 0
    "Toggle_秒杀",                       // 1
    "Toggle_无敌",                       // 2
    "Toggle_无限卡牌",                   // 3
    "Toggle_无限金币",                   // 4
    "Toggle_无限魂晶",                   // 5
#elif defined(STARRY_GAME_PCR)
    // 公主连结（PcrMod.h）— 对齐斌哥：属性 UI + get_deltaTime 变速
    "Category_属性",                     // 0
    "Toggle_开启属性修改",               // 1  PCR_FEAT_ATTR_ON
    "InputValue_9999:修改属性",          // 2  PCR_FEAT_ATTR_VAL（1–999999，输入框）
    "Category_速度",                     // 3
    "SeekBar_全局变速_1_10",             // 4  PCR_FEAT_SPEED（1=原速）
#elif defined(STARRY_GAME_GYWS)
    // 孤勇无双（GywsMod.h）— 对齐斌哥 ReceiveDamage / OnDamageEffect
    "Category_战斗",                     // 0
    "Toggle_百倍伤害",                   // 1  GYWS_FEAT_SECKILL
    "Toggle_超级无敌",                   // 2  GYWS_FEAT_INVINCIBLE
    "Toggle_全屏攻击",                   // 3  GYWS_FEAT_AOE
#elif defined(STARRY_GAME_GJC2)
    // 弓箭传说2国服 1.1.8（gjc2）
    "Category_战斗",                     // 0
    "Toggle_拿全部技能",                 // 1  GJC2_FEAT_STEAL_SKILL
    "Toggle_玩家隐身",                   // 2  GJC2_FEAT_INVIS
    "Toggle_全屏攻击",                   // 3  GJC2_FEAT_FULLSCREEN
#elif defined(STARRY_GAME_GJC2INTL)
    // 弓箭传说2国际版 1.0.1（gjc2intl — 独立菜单/卡密）
    "Category_国际版战斗",               // 0
    "Toggle_国际_偷全部技能",            // 1  GJC2INTL_FEAT_STEAL_SKILL
    "Toggle_国际_玩家隐身",              // 2  GJC2INTL_FEAT_INVIS
    "Toggle_国际_全屏攻击",              // 3  GJC2INTL_FEAT_FULLSCREEN
#elif defined(STARRY_GAME_SHLT)
    // 山河旅探（ShltMod.h）— ILRuntime 热更 DLL 改购买/关卡判定
    "Category_解锁",                     // 0
    "Toggle_解锁所有章节",               // 1  SHLT_FEAT_CHAPTER
    "Toggle_解锁所有服饰",               // 2  SHLT_FEAT_SKIN

#elif defined(STARRY_GAME_ZJMC)
    // 追迹迷城（ZjmcMod.h）
    "Category_战斗",                     // 0
    "Toggle_秒杀",                       // 1
    "Toggle_无敌",                       // 2
    "Toggle_全屏秒杀",                   // 3
    "Toggle_无限技能",                   // 4
    "Toggle_免广告",                     // 5
#elif defined(STARRY_GAME_TLGD)
    // 弹力果冻（TlgdMod.h）— 修复斌哥无效的全屏秒杀/无限技能
    "Category_战斗",                     // 0
    "Toggle_秒杀",                       // 1
    "Toggle_无敌",                       // 2
    "Toggle_全屏秒杀",                   // 3
    "Toggle_无限技能",                   // 4
#elif defined(STARRY_GAME_DZQS)
    // 独奏骑士（DzqsMod.h）— 斌哥 menu 720：伤害/无敌/无限技能
    "Category_战斗",                     // 0
    "Toggle_百倍伤害",                   // 1  DZQS_FEAT_KILL
    "Toggle_无敌",                       // 2  DZQS_FEAT_INV
    "Toggle_无限技能",                   // 3  DZQS_FEAT_NO_CD
#elif defined(STARRY_GAME_CSTJJS)
    // 长生天机降世（CstjjsMod.h）
    "Category_战斗",                     // 0
    "Toggle_十倍伤害",                   // 1
    "Toggle_无敌",                       // 2
#elif defined(STARRY_GAME_NEON)
    // 霓虹深渊（NeonMod.h — 仅战斗）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1
    "Toggle_秒杀",                       // 2
    "Toggle_技能无CD",                   // 3
    "Slider_移速:1:5",                   // 4
#elif defined(STARRY_GAME_WHRHX)
    // 无悔华夏（WhrhxMod.h — 斌哥六项对齐）
    "Category_资源",                     // 0
    "Toggle_无限金币",                   // 1  WHRHX_FEAT_GOLD
    "Toggle_无限食物",                   // 2  WHRHX_FEAT_FOOD
    "Toggle_无限军队",                   // 3  WHRHX_FEAT_ARMY
    "Toggle_无限人口",                   // 4  WHRHX_FEAT_POP
    "Category_战斗",                     // 5
    "Toggle_瞬间胜利",                   // 6  WHRHX_FEAT_WIN
    "Category_其它",                     // 7
    "Slider_游戏加速:1:5",               // 8  WHRHX_FEAT_SPEED 1=关
    "Toggle_科研立即完成",               // 9  WHRHX_FEAT_RESEARCH_DONE
#elif defined(STARRY_GAME_WHRHXKB)
    // 无悔华夏（WhrhxMod.h — 斌哥六项对齐）
    "Category_资源",                     // 0
    "Toggle_无限金币",                   // 1  WHRHX_FEAT_GOLD
    "Toggle_无限食物",                   // 2  WHRHX_FEAT_FOOD
    "Toggle_无限军队",                   // 3  WHRHX_FEAT_ARMY
    "Toggle_无限人口",                   // 4  WHRHX_FEAT_POP
    "Category_战斗",                     // 5
    "Toggle_瞬间胜利",                   // 6  WHRHX_FEAT_WIN
    "Category_其它",                     // 7
    "Slider_游戏加速:1:5",               // 8  WHRHX_FEAT_SPEED 1=关
#elif defined(STARRY_GAME_WGDZZ)
    // 王国大作战：前线 Trace 包（WgdzzMod.h）
    "Category_Trace",
    "Toggle_Trace桥接已开",              // 1  WGDZZ_FEAT_TRACE（占位，日志见 StarryTrace）
#elif defined(STARRY_GAME_BJDW)
    // 伯吉的温馨厨房（BjdwMod.h）
    "Category_经济",                     // 0
    "Toggle_无限金币",                   // 1  BJDW_FEAT_COINS
    "Toggle_无限广告券",               // 2  BJDW_FEAT_AD_COUPON
#elif defined(STARRY_GAME_ENDFIELD)
    // 明日方舟：终末地（EndfieldMod.h / 斌哥四功能）
    "Category_战斗",                     // 0
    "Toggle_百倍伤害",                   // 1  ENDFIELD_FEAT_DMG100
    "Toggle_无敌",                       // 2  ENDFIELD_FEAT_INV
    "Toggle_无限跳跃",                   // 3  ENDFIELD_FEAT_JUMP
    "Toggle_无限技能",                   // 4  ENDFIELD_FEAT_SKILL
#elif defined(STARRY_GAME_WXSJ)
    // 无限升级（WxsjMod.h）— 两分类精简版：战斗/功能
    // 屏蔽封号执法（block_report）不进菜单：native 启动强制默认开（v7.3）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1  WXSJ_FEAT_GOD
    "Toggle_秒杀",                       // 2  WXSJ_FEAT_ONESHOT
    "Category_功能",                     // 3
    "Toggle_免广告",                     // 4  WXSJ_FEAT_FREEADS
    "Toggle_物品不减",                   // 5  WXSJ_FEAT_ITEMKEEP
    "Toggle_技能免费升级",               // 6  WXSJ_FEAT_SKILLFREE
    "Toggle_广告奖励翻倍",               // 7  WXSJ_FEAT_ADSREWARD
    "Toggle_物品翻倍",                   // 8  WXSJ_FEAT_ITEMDOUBLE
    "Toggle_无限挑战秘境",               // 9  WXSJ_FEAT_MIJINGFREE
    "Toggle_皮肤全解锁",                 // 10 WXSJ_FEAT_SKINUNLOCK
    "Toggle_功能全解锁",                 // 11 WXSJ_FEAT_FUNCUNLOCK
    "Slider_经验倍率:1:10",              // 12 WXSJ_FEAT_EXPMULTI（滑块 1-10，配置键 exp_multi）
    // 静默防封（出站 RPC 白名单）不设按钮：与 block_report / COS 断链 / 执法 no-op 一样
    // 无条件生效（δ 层 ilpatch_delta 里直接插门），玩家无需也无法关闭。
#elif defined(STARRY_GAME_WGGLS)
    // 我嘎嘎乱杀（WgglsMod.h / 斌哥菜单三项）
    "Category_战斗",                     // 0
    "Toggle_百倍伤害",                   // 1  WGGLS_FEAT_DMG100
    "Toggle_超级无敌",                   // 2  WGGLS_FEAT_INV
    "Slider_战斗加速:1:5",               // 3  WGGLS_FEAT_SPEED 1=关
    "Toggle_免广告",                     // 4  WGGLS_FEAT_NOAD
#elif defined(STARRY_GAME_MILTHM)
    // milthm（与 Games/MilthmMod.h / 斌哥「自动跳舞」单 Toggle 对齐）
    "Category_玩法",                     // 0
    "Toggle_自动跳舞",                   // 1  MILTHM_FEAT_AUTO
#elif defined(STARRY_GAME_YWGN)
    // 音舞光年 — 模拟真人 + 判定概率滑条（与 YwgnMod.h 下标对齐）
    "Category_玩法",                     // 0
    "Toggle_模拟真人",                   // 1  YWGN_FEAT_SIM_HUMAN
    "Category_判定概率",                 // 2
    "SeekBar_Perfect+概率_0_100_85",       // 3  → ScoreType 5
    "SeekBar_Perfect概率_0_100_13",        // 4  → ScoreType 4
    "SeekBar_Great概率_0_100_1",           // 5  → ScoreType 3
    "SeekBar_Good概率_0_100_1",            // 6  → ScoreType 2
#elif defined(STARRY_GAME_PHIGROS)
    // Phigros — 满分（复刻斌哥增强链）
    "Category_玩法",                     // 0
    "Toggle_满分",                       // 1  PHIGROS_FEAT_AUTOPLAY
#elif defined(STARRY_GAME_ES2)
    // 偶像梦幻祭2 — 对外单功能「自动跳舞」（判定全最高，复刻斌哥 AllAmazing）
    "Category_玩法",                     // 0  ES2_FEAT_CAT_PLAY
    "Toggle_自动跳舞",                   // 1  ES2_FEAT_AUTODANCE
#elif defined(STARRY_GAME_SC33)
    // 生存33天（Sc33Mod.h get_ret 1/2/3/5 + 免广告 Java）
    "Category_战斗",                     // 0
    "Toggle_百倍伤害",                   // 1  SC33_FEAT_DMG100
    "Toggle_无敌",                       // 2  SC33_FEAT_INVINCIBLE
    "Toggle_全屏秒杀",                   // 3  SC33_FEAT_AOE_KILL
    "Slider_攻速:1:10",                  // 4  SC33_FEAT_ATK_SPEED  1=关 2..10=倍率
    "Category_其它",                     // 5
    "Toggle_免广告",                     // 6  SC33_FEAT_NO_AD
#elif defined(STARRY_GAME_CWJH)
    // 超维进化（Games/CwjhMod.h：0 基对齐——菜单 featNum=列表下标，无 Category 占位）
    // 20260909 重排：移除秒杀/无敌/无限技能（封号风险），改属性增益叠加系
    "Toggle_超级回血",                   // 0  CWJH_FEAT_REG_HP
    "Toggle_超级回蓝",                   // 1  CWJH_FEAT_REG_SP
    "Toggle_高暴击率",                   // 2  CWJH_FEAT_CHC
    "Toggle_高暴击伤害",                 // 3  CWJH_FEAT_CHD
    "Toggle_高防御",                     // 4  CWJH_FEAT_DEF
    "Toggle_高抗性",                     // 5  CWJH_FEAT_RES
    "Toggle_高闪避",                     // 6  CWJH_FEAT_DODGE
    "Toggle_免广告",                     // 7  CWJH_FEAT_NO_AD
    "Toggle_秒杀（封号）",               // 8  CWJH_FEAT_SECKILL（20260909 用户要求加回）
    "Toggle_无敌（封号）",               // 9  CWJH_FEAT_INV
#elif defined(STARRY_GAME_YQZD)
    // 遗弃之地（Games/YqzdMod.h：featNum=列表下标，Category 占下标；旗标文件桥）
    "Category_战斗",                     // 0  YQZD_FEAT_CAT_BATTLE
    "Toggle_无敌",                       // 1  YQZD_FEAT_INV
    "Toggle_倍攻",                       // 2  YQZD_FEAT_ATK_ON
    "Slider_倍攻:10:100",                // 3  YQZD_FEAT_ATK_VAL
    "Category_功能",                     // 4  YQZD_FEAT_CAT_FUNC
    "Toggle_加速",                       // 5  YQZD_FEAT_SPD_ON
    "Slider_加速:1:5",                   // 6  YQZD_FEAT_SPD_VAL
    "Toggle_免广告",                     // 7  YQZD_FEAT_ADFREE
#elif defined(STARRY_GAME_ZXR)
    // 镇邪人（与 Games/ZxrMod.h / 斌哥 FightChangeEntityHp+getAtk 对齐）
    "Category_战斗",                     // 0
    "Toggle_秒杀",                       // 1  ZXR_FEAT_SECKILL
    "Toggle_无敌",                       // 2  ZXR_FEAT_INVINCIBLE
    "Toggle_攻速",                       // 3  ZXR_FEAT_ATK_SPEED
    "Toggle_全屏秒杀",                   // 4  ZXR_FEAT_AOE_KILL
    "Category_经济",                     // 5
    "Toggle_免广告",                     // 6  ZXR_FEAT_NO_AD
    "Category_工具",                     // 7
    "Slider_游戏加速:1:5",               // 8  ZXR_FEAT_SPEEDUP 1=关 2=1.5x 3=2x 4=3x 5=4x
#elif defined(STARRY_GAME_ZMXY)
    // 造梦西游（ZmxyMod.h get_ret 1..3）
    "Category_战斗",                     // 0
    "Toggle_百倍伤害",                   // 1  ZMXY_FEAT_DMG100
    "Toggle_无敌",                       // 2  ZMXY_FEAT_INVINCIBLE
    "Toggle_无限技能",                   // 3  ZMXY_FEAT_ULTRA_SKILL（无CD+不耗蓝）
#elif defined(STARRY_GAME_SHHJ)
    // 山海幻境（菜单 1/2；JS get_ret 2=无敌 3=百倍）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1  SHHJ_FEAT_INVINCIBLE
    "Toggle_百倍伤害",                   // 2  SHHJ_FEAT_DMG100
#elif defined(STARRY_GAME_QLFH)
    // 青鸾繁华录（QlfhMod.h v2：免广告三合一 + 客户端战斗模拟器 + 变速）
    "Category_功能",                     // 0
    "Toggle_免广告",                     // 1  QLFH_FEAT_NO_AD（直领+本地广告+秒过）
    "Toggle_百倍伤害",                   // 2  QLFH_FEAT_DMG100
    "Toggle_无敌",                       // 3  QLFH_FEAT_INVINCIBLE
    "Slider_游戏加速:1:10",              // 4  QLFH_FEAT_SPEED（1=关 2..10=倍率）
#elif defined(STARRY_GAME_ZMWS)
    // 造梦无双（ZmwsMod.h get_ret 1..4）
    "Category_战斗",                     // 0
    "Toggle_无限技能",                   // 1  ZMWS_FEAT_ULTRA_SKILL
    "Toggle_法宝技能",                   // 2  ZMWS_FEAT_FABAO
    "Toggle_超级秒杀",                   // 3  ZMWS_FEAT_SECKILL
    "Toggle_超级无敌",                   // 4  ZMWS_FEAT_INVINCIBLE
#elif defined(STARRY_GAME_LTZJ)
    // 雷霆战机（LtzjMod.h get_ret 1..3）
    "Category_战斗",                     // 0
    "Toggle_百倍伤害",                   // 1  LTZJ_FEAT_DMG100
    "Toggle_无敌",                       // 2  LTZJ_FEAT_INVINCIBLE
    "Toggle_免广告",                     // 3  LTZJ_FEAT_NO_AD
#elif defined(STARRY_GAME_DWCT)
    // 动物餐厅（DwctMod.h）
    "Category_玩法",                     // 0
    "Toggle_免广告",                     // 1  DWCT_FEAT_NO_AD
    "Toggle_免内购",                     // 2  DWCT_FEAT_NO_IAP
#elif defined(STARRY_GAME_HALY)
    // 黑暗领域（HalyMod.h）
    "Category_玩法",                     // 0
    "Toggle_免广告",                     // 1  HALY_FEAT_NO_AD
    "Toggle_不羁怒火(自动无敌)",         // 2  HALY_FEAT_NUHUO
    "Toggle_先祖之魂(自动召唤)",       // 3  HALY_FEAT_XIANZU
    "Toggle_雷罚打击(普攻触发)",       // 4  HALY_FEAT_LEIFA
#elif defined(STARRY_GAME_WZYM)
    // 物质与魔法（与 Games/WzymMod.h / 改包 StatusC Hook 对齐）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1  wudi
    "Slider_伤害倍率:1:50",              // 2  需授权；>1 生效
    "Toggle_全屏秒杀",                   // 3  Update 扫敌
    "Slider_移速攻速:1:5",               // 4  1=关，越高越快
#elif defined(STARRY_GAME_MATCH2)
    // 消消英雄2（与 Games/Match2Mod.h 中 MATCH2_FEAT_* / 改包 AN_* 对齐）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1  AN_2
    "Toggle_十倍伤害",                   // 2  AN_1 (*10)
    "Toggle_瞬间结束战斗",               // 3  AN_3
#elif defined(STARRY_GAME_ZMXS)
    // 掌门下山（Games/ZmxsMod.h）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1  ZMXS_FEAT_INV
    "Toggle_秒杀",                       // 2  ZMXS_FEAT_ONE
    "Toggle_无限技能",                   // 3  ZMXS_FEAT_INF_SKILL
    "Toggle_零CD",                       // 4  ZMXS_FEAT_NO_CD
    "Slider_游戏加速:1:5",               // 5  ZMXS_FEAT_SPEED
    "Category_其它",                     // 6
    "Toggle_免广告",                     // 7  ZMXS_FEAT_NO_AD
#elif defined(STARRY_GAME_ZHMR)
    // 战魂铭人（ZhmrMod.h）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1  ZHMR_FEAT_GOD
    "Toggle_秒杀",                       // 2  ZHMR_FEAT_ONEHIT
    "Category_经济",                     // 3
    "Toggle_无限货币",                   // 4  ZHMR_FEAT_CURRENCY 三石9999
    "Toggle_无限道具",                   // 5  ZHMR_FEAT_ITEMS 其它999
    "Category_角色",                     // 6
    "Toggle_解锁全角色与技能",           // 7  ZHMR_FEAT_UNLOCK
    "Toggle_解锁全皮肤",                 // 8  ZHMR_FEAT_SKIN
#elif defined(STARRY_GAME_WDYZ)
    // 我的勇者（与 Games/WdyzMod.h 中 WDYZ_FEAT_* 对齐）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1  WDYZ_FEAT_INV
    "Toggle_五倍攻击",                   // 2  WDYZ_FEAT_DMG
#else
    // 堡垒之战（与 Games/BlzzMod.h 中 BLZZ_FEAT_* 对齐）
    "Category_战斗",                     // 0
    "Toggle_无敌",                       // 1  BLZZ_FEAT_INVINCIBLE
    "Toggle_十倍攻击",                   // 2  BLZZ_FEAT_ATK10
    "Toggle_蓄力无CD",                   // 3  BLZZ_FEAT_XULI_NOCD
    "Button_解锁所有功能",               // 4  BLZZ_FEAT_UNLOCK
#endif
};

static const int g_featureCount = sizeof(g_featureList) / sizeof(g_featureList[0]);

// ============================================================
//  游戏引擎自动检测
// ============================================================

static GameEngine detectGameEngine() {
    if (isLibraryLoaded(sec_lib_il2cpp())) {
        LOGI("检测到 Unity 引擎");
        g_targetLib = sec_lib_il2cpp();
        return ENGINE_UNITY;
    }
    if (isLibraryLoaded("libUE4.so") || isLibraryLoaded("libUE5.so")) {
        LOGI("检测到 Unreal Engine 游戏");
        g_targetLib = isLibraryLoaded("libUE4.so") ? "libUE4.so" : "libUE5.so";
        return ENGINE_UNREAL;
    }
    if (isLibraryLoaded(sec_lib_cocos()) || isLibraryLoaded(sec_lib_cocos2dcpp()) ||
        isLibraryLoaded(sec_lib_cocosjs())) {
        LOGI("检测到 Cocos 游戏");
        if (isLibraryLoaded(sec_lib_cocos())) g_targetLib = sec_lib_cocos();
        else if (isLibraryLoaded(sec_lib_cocos2dcpp())) g_targetLib = sec_lib_cocos2dcpp();
        else g_targetLib = sec_lib_cocosjs();
        return ENGINE_COCOS;
    }
    // 尝试常见的游戏库
    if (isLibraryLoaded("libgame.so")) {
        LOGI("检测到自研引擎 (libgame.so)");
        g_targetLib = "libgame.so";
        return ENGINE_CUSTOM;
    }
    return ENGINE_UNKNOWN;
}

// ============================================================
//  Unity il2cpp Hook 模块
//
//  适配 Unity 游戏，通过 il2cpp API 动态解析字段偏移
//  不硬编码 RVA，兼容游戏更新
// ============================================================

#ifdef ENABLE_UNITY_HOOK
#if defined(STARRY_GAME_HZSJ)
#include "Games/HzsjMod.h"
#elif defined(STARRY_GAME_ZJMC)
#include "Games/ZjmcMod.h"
#elif defined(STARRY_GAME_TLGD)
#include "Games/TlgdMod.h"
#elif defined(STARRY_GAME_DZQS)
#include "Games/DzqsMod.h"
#elif defined(STARRY_GAME_CSTJJS)
#include "Games/CstjjsMod.h"
#elif defined(STARRY_GAME_NEON)
#include "Games/NeonMod.h"
#elif defined(STARRY_GAME_WHRHX)
#include "Games/WhrhxMod.h"
#elif defined(STARRY_GAME_WHRHXKB)
#include "Games/WhrhxMod.h"
#elif defined(STARRY_GAME_WGDZZ)
#include "Games/WgdzzMod.h"
#elif defined(STARRY_GAME_BJDW)
#include "Games/BjdwMod.h"
#elif defined(STARRY_GAME_ENDFIELD)
#include "Games/EndfieldMod.h"
#elif defined(STARRY_GAME_WGGLS)
#include "Games/WgglsMod.h"
#elif defined(STARRY_GAME_MILTHM)
#include "Games/MilthmMod.h"
#elif defined(STARRY_GAME_YWGN)
#include "Games/YwgnMod.h"
#elif defined(STARRY_GAME_PHIGROS)
#include "Games/PhigrosMod.h"
#elif defined(STARRY_GAME_ES2)
#include "Games/Es2Mod.h"
#elif defined(STARRY_GAME_MATCH2)
#include "Games/Match2Mod.h"
#elif defined(STARRY_GAME_ZMXS)
#include "Games/ZmxsMod.h"
void Zmxs_SetJvm(JavaVM* vm);
#elif defined(STARRY_GAME_WZYM)
#include "Games/WzymMod.h"
#elif defined(STARRY_GAME_PCR)
#include "Games/PcrMod.h"
#elif defined(STARRY_GAME_GYWS)
#include "Games/GywsMod.h"
#elif defined(STARRY_GAME_GJC2)
#include "Games/Archero2Mod.h"
#elif defined(STARRY_GAME_GJC2INTL)
#include "Games/Archero2IntlMod.h"
#elif defined(STARRY_GAME_SHLT)
#include "Games/ShltMod.h"
#else
#include "Games/ZhmrMod.h"
#endif
#endif // ENABLE_UNITY_HOOK

#ifdef ENABLE_COCOS_HOOK
#if defined(STARRY_GAME_CWJH)
#include "Games/CwjhMod.h"
#elif defined(STARRY_GAME_YQZD)
#include "Games/YqzdMod.h"
#elif defined(STARRY_GAME_ZXR)
#include "Games/ZxrMod.h"
#elif defined(STARRY_GAME_SC33)
#include "Games/Sc33Mod.h"
#elif defined(STARRY_GAME_ZMXY)
#include "Games/ZmxyMod.h"
#elif defined(STARRY_GAME_SHHJ)
#include "Games/ShhjMod.h"
#elif defined(STARRY_GAME_QLFH)
#include "Games/QlfhMod.h"
#elif defined(STARRY_GAME_ZMWS)
#include "Games/ZmwsMod.h"
#elif defined(STARRY_GAME_LTZJ)
#include "Games/LtzjMod.h"
#include "Games/LtzjAceSoft.h"
#elif defined(STARRY_GAME_DWCT)
#include "Games/DwctMod.h"
#elif defined(STARRY_GAME_HALY)
#include "Games/HalyMod.h"
#else
#include "Games/BlzzMod.h"
#endif
#endif // ENABLE_COCOS_HOOK

// ============================================================
//  Cocos2d-x Hook 模块
//
//  适用：libcocos2dcpp.so / libcocos2djs.so
//  提供：符号查找 / RVA 定位 / 特征码搜索 三种方式
//  适配时在 GameConfig.h 中填入对应字段
// ============================================================

#ifdef ENABLE_COCOS_HOOK

// Cocos 偏移解析结果
struct CocosOffsets {
    void* libBase;                  // 目标库基址
    void* funcSetScore;             // setScore 函数地址
    void* funcAddGold;              // addGold 函数地址
    void* funcPlayerHurt;           // 玩家受伤函数地址
    void* funcEnemyDie;             // 敌人死亡函数地址
    void* globalPlayerMgr;          // PlayerManager 全局实例
    void* globalGameMgr;            // GameManager 全局实例
};
static CocosOffsets g_cocosOffsets = {0};

// 通过符号名查找函数地址（适用符号未 strip 的游戏）
static void* resolveBySymbol(const char* libName, const char* symbol) {
    if (!symbol || symbol[0] == '\0') return nullptr;
    void* handle = dlopen(libName, RTLD_NOLOAD | RTLD_LAZY);
    if (!handle) {
        LOGW("[Cocos] 无法打开 %s", libName);
        return nullptr;
    }
    void* addr = dlsym(handle, symbol);
    dlclose(handle);
    if (addr) {
        LOGI("[Cocos] 符号 %s -> %p", symbol, addr);
    } else {
        LOGW("[Cocos] 未找到符号 %s（可能被 strip，改用 RVA 方式）", symbol);
    }
    return addr;
}

// 通过 RVA 偏移查找函数地址（最常用，需 IDA/Ghidra 分析）
static void* resolveByRVA(const char* libName, uintptr_t rva) {
    if (rva == 0) return nullptr;
    uintptr_t base = getLibraryAddress(libName);
    if (!base) {
        LOGW("[Cocos] 无法获取 %s 基址", libName);
        return nullptr;
    }
    void* addr = (void*)(base + rva);
    LOGI("[Cocos] RVA 0x%lx -> %p (base=%p)", (unsigned long)rva, addr, (void*)base);
    return addr;
}

// 解析 Cocos 游戏的所有目标地址
static bool resolveCocosOffsets() {
    const char* libName = GameCocos::TARGET_LIB;
    LOGI("[Cocos] 开始解析目标库: %s", libName);

    if (!isLibraryLoaded(libName)) {
        LOGE("[Cocos] 目标库 %s 未加载", libName);
        return false;
    }

    g_cocosOffsets.libBase = (void*)getLibraryAddress(libName);
    if (!g_cocosOffsets.libBase) {
        LOGE("[Cocos] 无法获取库基址");
        return false;
    }
    LOGI("[Cocos] 库基址: %p", g_cocosOffsets.libBase);

    // 方式 A：符号查找
    g_cocosOffsets.funcSetScore    = resolveBySymbol(libName, GameCocos::Symbols::PLAYER_SET_SCORE);
    g_cocosOffsets.funcAddGold     = resolveBySymbol(libName, GameCocos::Symbols::PLAYER_ADD_GOLD);
    g_cocosOffsets.funcPlayerHurt  = resolveBySymbol(libName, GameCocos::Symbols::ENEMY_TAKE_DAMAGE);

    // 方式 B：RVA 查找（符号未找到时回退）
    if (!g_cocosOffsets.funcSetScore) {
        g_cocosOffsets.funcSetScore = resolveByRVA(libName, GameCocos::Offsets::FUNC_SET_SCORE);
    }
    if (!g_cocosOffsets.funcAddGold) {
        g_cocosOffsets.funcAddGold = resolveByRVA(libName, GameCocos::Offsets::FUNC_ADD_GOLD);
    }
    if (!g_cocosOffsets.funcPlayerHurt) {
        g_cocosOffsets.funcPlayerHurt = resolveByRVA(libName, GameCocos::Offsets::FUNC_PLAYER_HURT);
    }
    g_cocosOffsets.funcEnemyDie = resolveByRVA(libName, GameCocos::Offsets::FUNC_ENEMY_DIE);

    // 全局实例指针
    g_cocosOffsets.globalPlayerMgr = resolveByRVA(libName, GameCocos::Offsets::GLOBAL_PLAYER_MGR);
    g_cocosOffsets.globalGameMgr   = resolveByRVA(libName, GameCocos::Offsets::GLOBAL_GAME_MGR);

    // 方式 C：特征码搜索（TODO，需要 KittyScanner 实现）
    // KittyMemory 提供了 KittyScanner_ToolKit，可扫描内存特征码
    // 适合 RVA 在游戏更新后失效的场景，特征码更抗更新
    if (GameCocos::Patterns::SET_SCORE[0] != '\0') {
        LOGI("[Cocos] 特征码搜索尚未实现，请改用 RVA 方式");
        // TODO: 用 KittyScanner 扫描 GameCocos::Patterns::SET_SCORE
    }

    LOGI("[Cocos] 偏移解析完成（已实现的项已就绪，未填写的为 nullptr）");
    return true;
}

#endif // ENABLE_COCOS_HOOK

// ============================================================
//  通用 Hook 模块（自研引擎 / 任意 .so）
//
//  兜底方案：直接用 RVA / 符号定位代码或数据
//  适配任何游戏，但需要手动分析目标 .so
// ============================================================

#ifdef ENABLE_GENERIC_HOOK

struct GenericOffsets {
    void* libBase;
    void* addrGold;           // 金币地址（数据）
    void* addrHp;             // 血量地址（数据）
    void* addrScore;          // 分数地址（数据）
    void* funcPlayerHurt;     // 玩家受伤函数
    void* funcEnemyDie;       // 敌人死亡函数
};
static GenericOffsets g_genericOffsets = {0};

static bool resolveGenericOffsets() {
    const char* libName = GameGeneric::TARGET_LIB;
    LOGI("[Generic] 开始解析目标库: %s", libName);

    if (!isLibraryLoaded(libName)) {
        LOGE("[Generic] 目标库 %s 未加载", libName);
        return false;
    }

    g_genericOffsets.libBase = (void*)getLibraryAddress(libName);
    if (!g_genericOffsets.libBase) {
        LOGE("[Generic] 无法获取库基址");
        return false;
    }
    LOGI("[Generic] 库基址: %p", g_genericOffsets.libBase);

    // 数据地址（直接读写的数值型作弊）
    if (GameGeneric::Offsets::ADDR_GOLD > 0) {
        g_genericOffsets.addrGold = (void*)((uintptr_t)g_genericOffsets.libBase + GameGeneric::Offsets::ADDR_GOLD);
        LOGI("[Generic] 金币地址: %p", g_genericOffsets.addrGold);
    }
    if (GameGeneric::Offsets::ADDR_HP > 0) {
        g_genericOffsets.addrHp = (void*)((uintptr_t)g_genericOffsets.libBase + GameGeneric::Offsets::ADDR_HP);
        LOGI("[Generic] 血量地址: %p", g_genericOffsets.addrHp);
    }
    if (GameGeneric::Offsets::ADDR_SCORE > 0) {
        g_genericOffsets.addrScore = (void*)((uintptr_t)g_genericOffsets.libBase + GameGeneric::Offsets::ADDR_SCORE);
        LOGI("[Generic] 分数地址: %p", g_genericOffsets.addrScore);
    }

    // 函数地址（用于 HOOK 替换逻辑）
    if (GameGeneric::Offsets::FUNC_PLAYER_HURT > 0) {
        g_genericOffsets.funcPlayerHurt = (void*)((uintptr_t)g_genericOffsets.libBase + GameGeneric::Offsets::FUNC_PLAYER_HURT);
        LOGI("[Generic] 玩家受伤函数: %p", g_genericOffsets.funcPlayerHurt);
    }
    if (GameGeneric::Offsets::FUNC_ENEMY_DIE > 0) {
        g_genericOffsets.funcEnemyDie = (void*)((uintptr_t)g_genericOffsets.libBase + GameGeneric::Offsets::FUNC_ENEMY_DIE);
        LOGI("[Generic] 敌人死亡函数: %p", g_genericOffsets.funcEnemyDie);
    }

    LOGI("[Generic] 偏移解析完成");
    return true;
}

#endif // ENABLE_GENERIC_HOOK

// ============================================================
//  通用内存读写模块
//
//  适用于所有游戏：直接读写内存
//  配合 HOOK / PATCH 宏使用
// ============================================================

static void writeMemory(void* addr, void* data, size_t size) {
    if (!addr || !data) return;
    // Dobby 的 DobbyCodePatch 可以写内存
    DobbyCodePatch(addr, (uint8_t*)data, size);
}

template<typename T>
static void writeValue(void* addr, T value) {
    if (!addr) return;
    // 用 mprotect 改权限后直接写
    uintptr_t page = (uintptr_t)addr & ~0xFFF;
    size_t pages = (sizeof(T) + ((uintptr_t)addr - page) + 0xFFF) / 0x1000;
    mprotect((void*)page, pages * 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC);
    *(T*)addr = value;
}

template<typename T>
static T readValue(void* addr) {
    if (!addr) return T();
    return *(T*)addr;
}

// ============================================================
//  后台作弊线程
// ============================================================

// 前向声明
static void ShowToast(const char* text);

static void* mod_thread(void* arg) {
    LOGI("MOD 后台线程已启动");

    // 注入场景：nativeInit 往往早于游戏 so，需轮询等待
    int waitCount = 0;
    while (true) {
        // 当前优先：战魂铭人 / Unity il2cpp
        if (isLibraryLoaded(sec_lib_il2cpp())) {
            g_targetLib = sec_lib_il2cpp();
            g_engine = ENGINE_UNITY;
            g_gameLibLoaded = true;
            LOGI("检测到 Unity 引擎，ENGINE_UNITY");
            break;
        }
        if (isLibraryLoaded(sec_lib_cocos())) {
            g_targetLib = sec_lib_cocos();
            g_engine = ENGINE_COCOS;
            g_gameLibLoaded = true;
            LOGI("检测到 Cocos 引擎，ENGINE_COCOS");
            break;
        }
        if (isLibraryLoaded(sec_lib_cocosjs())) {
            g_targetLib = sec_lib_cocosjs();
            g_engine = ENGINE_COCOS;
            g_gameLibLoaded = true;
            LOGI("检测到 Cocos JS 引擎，ENGINE_COCOS");
            break;
        }
        if (isLibraryLoaded(sec_lib_mygame())) {
            g_targetLib = sec_lib_mygame();
            g_engine = ENGINE_COCOS; /* 蜀汉传卧龙篇：自研 Cocos2d-x 底座 */
            g_gameLibLoaded = true;
            LOGI("检测到 libMyGame.so，ENGINE_COCOS (SHC)");
            break;
        }
        GameEngine e = detectGameEngine();
        if (e != ENGINE_UNKNOWN && g_targetLib && isLibraryLoaded(g_targetLib)) {
            g_engine = e;
            g_gameLibLoaded = true;
            LOGI("目标库 %s 已加载 (engine=%d)", g_targetLib, (int)g_engine);
            break;
        }
        sleep(1);
        waitCount++;
        if ((waitCount % 5) == 0) {
            LOGI("等待游戏库… %ds (il2cpp=%d cocos=%d cocosjs=%d)", waitCount,
                 isLibraryLoaded(sec_lib_il2cpp()) ? 1 : 0,
                 isLibraryLoaded(sec_lib_cocos()) ? 1 : 0,
                 isLibraryLoaded(sec_lib_cocosjs()) ? 1 : 0);
        }
        if (waitCount > 120) {
            LOGW("等待游戏库超时（120s）");
            g_gameLibLoaded = true;
            break;
        }
    }

    // 根据引擎类型初始化 Hook
    switch (g_engine) {
#ifdef ENABLE_UNITY_HOOK
        case ENGINE_UNITY: {
#if defined(STARRY_GAME_HZSJ)
            LOGI("初始化魂坠深境 / HybridCLR GenerateDamage Hook...");
            sleep(3);
            bool hzsjOk = false;
            for (int i = 0; i < 180; i++) {
                if (Hzsj_TryInit()) { hzsjOk = true; break; }
                LOGI("[Hzsj] 等待 HybridCLR，2s 后重试 (%d/180)", i + 1);
                sleep(2);
            }
            if (!hzsjOk) LOGE("[Hzsj] 初始化超时（进关后 Tick 继续试）");
            else LOGI("[Hzsj] 初始化成功");
#elif defined(STARRY_GAME_ZJMC)
            LOGI("初始化追迹迷城 / ChangeHP+技能闸门 Hook...");
            sleep(2);
            bool zjmcOk = false;
            for (int i = 0; i < 120; i++) {
                if (Zjmc_TryInit()) { zjmcOk = true; break; }
                LOGI("[Zjmc] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!zjmcOk) LOGE("[Zjmc] 初始化超时，战斗功能不可用");
            else LOGI("[Zjmc] 初始化成功");
#elif defined(STARRY_GAME_TLGD)
            LOGI("初始化弹力果冻 / HybridCLR HpHandle+Skill Hook...");
            sleep(2);
            bool tlgdOk = false;
            for (int i = 0; i < 180; i++) {
                if (Tlgd_TryInit()) { tlgdOk = true; break; }
                LOGI("[Tlgd] 等待 hotfix 类，2s 后重试 (%d/180)", i + 1);
                sleep(2);
            }
            if (!tlgdOk) LOGE("[Tlgd] 初始化超时（进关后 Tick 继续试）");
            else LOGI("[Tlgd] 初始化成功");
#elif defined(STARRY_GAME_DZQS)
            LOGI("初始化独奏骑士 / HybridCLR trampoline 0x20B840C...");
            sleep(2);
            bool dzqsOk = false;
            for (int i = 0; i < 180; i++) {
                if (Dzqs_TryInit()) { dzqsOk = true; break; }
                LOGI("[Dzqs] 等待目标库，2s 后重试 (%d/180)", i + 1);
                sleep(2);
            }
            if (!dzqsOk) LOGE("[Dzqs] 初始化超时（进关后 Tick 继续试）");
            else LOGI("[Dzqs] 初始化成功");
#elif defined(STARRY_GAME_CSTJJS)
            LOGI("初始化长生天机降世 / maps跳板+Health MI...");
            sleep(2);
            bool cstjjsOk = false;
            for (int i = 0; i < 120; i++) {
                if (Cstjjs_TryInit()) { cstjjsOk = true; break; }
                LOGI("[Cstjjs] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!cstjjsOk) LOGE("[Cstjjs] 初始化超时（Tick 继续试）");
            else LOGI("[Cstjjs] 初始化成功");
#elif defined(STARRY_GAME_NEON)
            LOGI("初始化霓虹深渊 / DeductHP+过检 Hook...");
            sleep(2);
            bool neonOk = false;
            for (int i = 0; i < 120; i++) {
                if (Neon_TryInit()) { neonOk = true; break; }
                LOGI("[Neon] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!neonOk) LOGE("[Neon] 初始化超时，战斗功能不可用");
            else LOGI("[Neon] 初始化成功");
#elif defined(STARRY_GAME_WHRHX)
            LOGI("初始化无悔华夏 / 官方 RVA Mod...");
            sleep(2);
            bool whrhxOk = false;
            for (int i = 0; i < 120; i++) {
                if (Whrhx_TryInit()) { whrhxOk = true; break; }
                LOGI("[Whrhx] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!whrhxOk) LOGE("[Whrhx] 初始化超时（Tick 继续试）");
            else LOGI("[Whrhx] 初始化成功");
#elif defined(STARRY_GAME_WHRHXKB)
            LOGI("初始化无悔华夏 / 官方 RVA Mod...");
            sleep(2);
            bool whrhxOk = false;
            for (int i = 0; i < 120; i++) {
                if (Whrhx_TryInit()) { whrhxOk = true; break; }
                LOGI("[Whrhx] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!whrhxOk) LOGE("[Whrhx] 初始化超时（Tick 继续试）");
            else LOGI("[Whrhx] 初始化成功");
#elif defined(STARRY_GAME_WGDZZ)
            LOGI("初始化王国大作战前线 / AOT桥接 Trace...");
            sleep(3);
            bool wgdzzOk = false;
            for (int i = 0; i < 120; i++) {
                if (Wgdzz_TryInit()) { wgdzzOk = true; break; }
                LOGI("[Wgdzz] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!wgdzzOk) LOGE("[Wgdzz] 初始化超时（Tick 继续试）");
            else LOGI("[Wgdzz] 初始化成功");
#elif defined(STARRY_GAME_BJDW)
            LOGI("初始化伯吉的温馨厨房 / 金币·广告券...");
            sleep(3);
            bool bjdwOk = false;
            for (int i = 0; i < 120; i++) {
                if (Bjdw_TryInit()) { bjdwOk = true; break; }
                LOGI("[Bjdw] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!bjdwOk) LOGE("[Bjdw] 初始化超时（Tick 继续试）");
            else LOGI("[Bjdw] 初始化成功");
#elif defined(STARRY_GAME_ENDFIELD)
            LOGI("初始化终末地 / Beyond.Gameplay.Core 按名 Hook...");
            sleep(2);
            bool efOk = false;
            for (int i = 0; i < 120; i++) {
                if (Endfield_TryInit()) { efOk = true; break; }
                LOGI("[Endfield] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!efOk) LOGE("[Endfield] 初始化超时（Tick 继续试）");
            else LOGI("[Endfield] 初始化成功");
#elif defined(STARRY_GAME_WDYZ)
            LOGI("初始化我的勇者 / Battle 结算按 RVA Hook...");
            sleep(2);
            bool wdyzOk = false;
            for (int i = 0; i < 120; i++) {
                if (Wdyz_TryInit()) { wdyzOk = true; break; }
                LOGI("[Wdyz] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!wdyzOk) LOGE("[Wdyz] 初始化超时（Tick 继续试）");
            else LOGI("[Wdyz] 初始化成功");
#elif defined(STARRY_GAME_WXSJ)
            LOGI("初始化无限升级 / ExecuteRuntimeAssembly dll 替换 Hook...");
            bool wxsjOk = false;
            for (int i = 0; i < 120; i++) {
                if (Wxsj_TryInit()) { wxsjOk = true; break; }
                if (Wxsj_IsReady()) { wxsjOk = true; break; }   // fast-retry 线程装好后这里立刻确认
                usleep(300 * 1000);
            }
            if (!wxsjOk) LOGE("[Wxsj] 初始化超时（Tick 继续试）");
            else LOGI("[Wxsj] 初始化成功");
#elif defined(STARRY_GAME_WGGLS)
            LOGI("初始化我嘎嘎乱杀 / BattleUtility+TimeScale Hook...");
            sleep(2);
            bool wgglsOk = false;
            for (int i = 0; i < 120; i++) {
                if (Wggls_TryInit()) { wgglsOk = true; break; }
                LOGI("[Wggls] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!wgglsOk) LOGE("[Wggls] 初始化超时（Tick 继续试）");
            else LOGI("[Wggls] 初始化成功");
#elif defined(STARRY_GAME_MILTHM)
            LOGI("初始化 milthm / 自动跳舞 Hook...");
            sleep(2);
            bool milthmOk = false;
            for (int i = 0; i < 120; i++) {
                if (Milthm_TryInit()) { milthmOk = true; break; }
                LOGI("[Milthm] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!milthmOk) LOGE("[Milthm] 初始化超时，自动跳舞不可用");
            else LOGI("[Milthm] 初始化成功");
#elif defined(STARRY_GAME_YWGN)
            LOGI("初始化音舞光年 / 模拟真人（SendScore hook）...");
            sleep(2);
            bool ywgnOk = false;
            for (int i = 0; i < 120; i++) {
                if (Ywgn_TryInit()) { ywgnOk = true; break; }
                LOGI("[Ywgn] 等待 HybridCLR BubbleConfig，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!ywgnOk) LOGE("[Ywgn] 初始化超时（进关后 Tick 会继续尝试）");
            else LOGI("[Ywgn] 初始化成功");
#elif defined(STARRY_GAME_PHIGROS)
            LOGI("初始化 Phigros / 自动跳 v1...");
            sleep(2);
            bool phigrosOk = false;
            for (int i = 0; i < 120; i++) {
                if (Phigros_TryInit()) { phigrosOk = true; break; }
                LOGI("[Phigros] 等待 libil2cpp，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!phigrosOk) LOGE("[Phigros] 初始化超时（Tick 会继续尝试）");
            else LOGI("[Phigros] 初始化成功");
#elif defined(STARRY_GAME_ES2)
            LOGI("初始化偶像梦幻祭2 / 探测 v1（dump-only）...");
            sleep(5); // 等 il2cpp domain 起来
            bool es2Ok = false;
            for (int i = 0; i < 120; i++) {
                if (Es2_TryInit()) { es2Ok = true; break; }
                LOGI("[Es2] 等待 libil2cpp，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!es2Ok) LOGE("[Es2] 初始化超时（Tick 会继续尝试）");
            else LOGI("[Es2] 初始化成功（dump 由 Tick 延迟触发）");
#elif defined(STARRY_GAME_MATCH2)
            LOGI("初始化消消英雄2 / xLua Hook...");
            sleep(8); // 等 il2cpp domain + assemblies 就绪；过早扫会 SIGSEGV / 卡死
            bool match2Ok = false;
            for (int i = 0; i < 180; i++) {
                Match2_TryInitTraceEarly();
                if (Match2_TryInit()) { match2Ok = true; break; }
                if ((i % 4) == 0) {
                    LOGI("[Match2] 等待 LuaManager，0.5s 后重试 (%d/180)", i + 1);
                }
                usleep(500000);
            }
            if (!match2Ok) LOGE("[Match2] 解析超时，战斗功能不可用");
            else LOGI("[Match2] 初始化成功");
#elif defined(STARRY_GAME_ZMXS)
            LOGI("初始化掌门下山 / xLua Hook...");
            sleep(5); // il2cpp 过早 domain/Update hook 会 SIGSEGV（见 ZHMR）
            bool zmxsOk = false;
            for (int i = 0; i < 180; i++) {
                if (Zmxs_TryInit()) { zmxsOk = true; break; }
                if ((i % 4) == 0) {
                    LOGI("[Zmxs] 等待 XLuaManager，0.5s 后重试 (%d/180)", i + 1);
                }
                usleep(500000);
            }
            if (!zmxsOk) LOGE("[Zmxs] 解析超时，战斗功能可能不可用");
            else LOGI("[Zmxs] 初始化成功");
#elif defined(STARRY_GAME_WZYM)
            LOGI("初始化物质与魔法 / StatusC Hook...");
            sleep(3);
            bool wzymOk = false;
            for (int i = 0; i < 120; i++) {
                if (Wzym_TryInit()) { wzymOk = true; break; }
                LOGI("[Wzym] 等待 StatusC，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!wzymOk) LOGE("[Wzym] 解析超时，战斗功能不可用");
            else LOGI("[Wzym] 初始化成功");
#elif defined(STARRY_GAME_PCR)
            LOGI("初始化公主连结 / Time.get_deltaTime 变速...");
            sleep(2);
            bool pcrOk = false;
            for (int i = 0; i < 120; i++) {
                if (Pcr_TryInit()) { pcrOk = true; break; }
                LOGI("[Pcr] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!pcrOk) LOGE("[Pcr] 初始化超时（Tick 会继续尝试）");
            else LOGI("[Pcr] 初始化成功");
#elif defined(STARRY_GAME_GYWS)
            LOGI("初始化孤勇无双 / SkillEffectSystem Hook...");
            sleep(2);
            bool gywsOk = false;
            for (int i = 0; i < 120; i++) {
                if (Gyws_TryInit()) { gywsOk = true; break; }
                LOGI("[Gyws] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!gywsOk) LOGE("[Gyws] 初始化超时（Tick 会继续尝试）");
            else LOGI("[Gyws] 初始化成功");
#elif defined(STARRY_GAME_GJC2)
            // 禁止启动期轮询 Gjc2_TryInit：继续关卡恢复时后台扫 HotFix/Il2Cpp 会与主线程死锁卡死。
            // 等用户打开功能后再由 OnFeatureChanged / Tick 懒初始化。
            LOGI("[Gjc2] 跳过启动期 Init（功能开启后再挂 HotFix）");
#elif defined(STARRY_GAME_GJC2INTL)
            LOGI("初始化弓箭传说2国际版 / HotFix.BattleLogic Hook...");
            sleep(2);
            bool gjc2IntlOk = false;
            for (int i = 0; i < 120; i++) {
                if (Gjc2Intl_TryInit()) { gjc2IntlOk = true; break; }
                LOGI("[Gjc2Intl] 等待 HotFix 热更类，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!gjc2IntlOk) LOGE("[Gjc2Intl] 初始化超时（Tick 会继续尝试）");
            else LOGI("[Gjc2Intl] 初始化成功");
#elif defined(STARRY_GAME_SHLT)
            LOGI("初始化山河旅探 / ILRuntime 热更 DLL 解锁...");
            sleep(2);
            bool shltOk = false;
            for (int i = 0; i < 120; i++) {
                if (Shlt_TryInit()) { shltOk = true; break; }
                LOGI("[Shlt] 等待目标库，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!shltOk) LOGE("[Shlt] 初始化超时（Tick 会继续尝试）");
            else LOGI("[Shlt] 初始化成功");
#elif defined(STARRY_GAME_ZHMR)
            LOGI("初始化战魂铭人 / Unity HybridCLR Hook...");
            sleep(2); // 避开 il2cpp_domain_get 过早 SIGSEGV
            bool zhmrOk = false;
            for (int i = 0; i < 120; i++) {
                if (Zhmr_TryInit()) { zhmrOk = true; break; }
                LOGI("[Zhmr] 等待热更类，2s 后重试 (%d/120)", i + 1);
                sleep(2);
            }
            if (!zhmrOk) LOGE("[Zhmr] 解析超时，战斗功能不可用");
            else LOGI("[Zhmr] 初始化成功");
#else
            LOGW("ENGINE_UNITY 已启用但未匹配 STARRY_GAME_* 玩法模块");
#endif
            break;
        }
#endif
#ifdef ENABLE_COCOS_HOOK
        case ENGINE_COCOS: {
#if defined(STARRY_GAME_CWJH)
            LOGI("初始化超维进化 / Cocos2djs Hook（尽早挂 se::evalString）...");
            bool cwjhOk = false;
            for (int i = 0; i < 90; i++) {
                if (Cwjh_TryInit(g_jvm)) { cwjhOk = true; break; }
                LOGI("[Cwjh] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!cwjhOk) LOGE("[Cwjh] 初始化超时");
            else LOGI("[Cwjh] 初始化成功");
#elif defined(STARRY_GAME_ZXR)
            LOGI("初始化镇邪人 / Cocos2djs Hook（尽早挂 se::evalString）...");
            bool zxrOk = false;
            for (int i = 0; i < 90; i++) {
                if (Zxr_TryInit(g_jvm)) { zxrOk = true; break; }
                LOGI("[Zxr] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!zxrOk) LOGE("[Zxr] 初始化超时");
            else LOGI("[Zxr] 初始化成功");
#elif defined(STARRY_GAME_SC33)
            LOGI("初始化生存33天 / Cocos2djs Hook（尽早挂 se::evalString）...");
            bool sc33Ok = false;
            for (int i = 0; i < 90; i++) {
                if (Sc33_TryInit(g_jvm)) { sc33Ok = true; break; }
                LOGI("[Sc33] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!sc33Ok) LOGE("[Sc33] 初始化超时");
            else LOGI("[Sc33] 初始化成功");
#elif defined(STARRY_GAME_ZMXY)
            LOGI("初始化造梦西游 / Cocos2djs Hook（尽早挂 se::evalString）...");
            bool zmxyOk = false;
            for (int i = 0; i < 90; i++) {
                if (Zmxy_TryInit(g_jvm)) { zmxyOk = true; break; }
                LOGI("[Zmxy] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!zmxyOk) LOGE("[Zmxy] 初始化超时");
            else LOGI("[Zmxy] 初始化成功");
#elif defined(STARRY_GAME_SHHJ)
            LOGI("初始化山海幻境 / Cocos2djs Hook（尽早挂 se::evalString）...");
            bool shhjOk = false;
            for (int i = 0; i < 90; i++) {
                if (Shhj_TryInit(g_jvm)) { shhjOk = true; break; }
                LOGI("[Shhj] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!shhjOk) LOGE("[Shhj] 初始化超时");
            else LOGI("[Shhj] 初始化成功");
#elif defined(STARRY_GAME_QLFH)
            LOGI("初始化青鸾繁华录 / Cocos2djs Hook（尽早挂 se::evalString）...");
            bool qlfhOk = false;
            for (int i = 0; i < 90; i++) {
                if (Qlfh_TryInit(g_jvm)) { qlfhOk = true; break; }
                LOGI("[Qlfh] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!qlfhOk) LOGE("[Qlfh] 初始化超时");
            else LOGI("[Qlfh] 初始化成功");
#elif defined(STARRY_GAME_ZMWS)
            LOGI("初始化造梦无双 / Cocos2djs Hook（尽早挂 se::evalString）...");
            bool zmwsOk = false;
            for (int i = 0; i < 90; i++) {
                if (Zmws_TryInit(g_jvm)) { zmwsOk = true; break; }
                LOGI("[Zmws] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!zmwsOk) LOGE("[Zmws] 初始化超时");
            else LOGI("[Zmws] 初始化成功");
#elif defined(STARRY_GAME_LTZJ)
            LOGI("初始化雷霆战机 / Cocos Creator3 Hook...");
            sleep(2);
            bool ltzjOk = false;
            for (int i = 0; i < 90; i++) {
                if (Ltzj_TryInit(g_jvm)) { ltzjOk = true; break; }
                LOGI("[Ltzj] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!ltzjOk) LOGE("[Ltzj] 初始化超时");
            else LOGI("[Ltzj] 初始化成功");
#elif defined(STARRY_GAME_DWCT)
            LOGI("初始化动物餐厅 / Cocos2djs Hook...");
            bool dwctOk = false;
            for (int i = 0; i < 90; i++) {
                if (Dwct_TryInit(g_jvm)) { dwctOk = true; break; }
                LOGI("[Dwct] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!dwctOk) LOGE("[Dwct] 初始化超时");
            else LOGI("[Dwct] 初始化成功");
#elif defined(STARRY_GAME_YQZD)
            LOGI("初始化遗弃之地 / Cocos Creator3 零hook JS wrap...");
            bool yqzdOk = false;
            for (int i = 0; i < 90; i++) {
                if (Yqzd_TryInit(g_jvm)) { yqzdOk = true; break; }
                LOGI("[Yqzd] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!yqzdOk) LOGE("[Yqzd] 初始化超时");
            else LOGI("[Yqzd] 初始化成功");
#elif defined(STARRY_GAME_HALY)
            LOGI("初始化黑暗领域 / Cocos Creator3 Hook...");
            sleep(2);
            bool halyOk = false;
            for (int i = 0; i < 90; i++) {
                if (Haly_TryInit(g_jvm)) { halyOk = true; break; }
                LOGI("[Haly] 等待 %s，1s 后重试 (%d/90)", GameCocos::TARGET_LIB, i + 1);
                sleep(1);
            }
            if (!halyOk) LOGE("[Haly] 初始化超时");
            else LOGI("[Haly] 初始化成功");
#else
            LOGI("初始化堡垒之战 / Cocos Creator Hook...");
            sleep(2); // 等 JS / System 模块
            bool blzzOk = false;
            for (int i = 0; i < 60; i++) {
                if (Blzz_TryInit(g_jvm)) { blzzOk = true; break; }
                LOGI("[Blzz] 等待 %s，2s 后重试 (%d/60)", GameCocos::TARGET_LIB, i + 1);
                sleep(2);
            }
            if (!blzzOk) LOGE("[Blzz] 初始化超时");
            else LOGI("[Blzz] 初始化成功");
            resolveCocosOffsets();
#endif
            break;
        }
#endif
#ifdef ENABLE_GENERIC_HOOK
        case ENGINE_CUSTOM:
        case ENGINE_UNKNOWN:
            LOGI("初始化通用 Hook（自研引擎 / 未知引擎）...");
            resolveGenericOffsets();
            break;
#endif
        case ENGINE_UNREAL:
            LOGW("Unreal Engine 专用 Hook 未实现，尝试通用 Hook 兜底（若已启用）");
#ifdef ENABLE_GENERIC_HOOK
            resolveGenericOffsets();
#endif
            break;
#if !defined(ENABLE_COCOS_HOOK)
        case ENGINE_COCOS:
            LOGW("Cocos2d-x Hook 未启用");
            break;
#endif
#if !defined(ENABLE_GENERIC_HOOK)
        case ENGINE_CUSTOM:
        case ENGINE_UNKNOWN:
            LOGW("通用 Hook 未启用");
            break;
#endif
    }

    LOGI("MOD 已就绪 engine=%d cocosGame=%d unityGame=%d", (int)g_engine,
#ifdef ENABLE_COCOS_HOOK
#if defined(STARRY_GAME_CWJH)
         Cwjh_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_ZXR)
         Zxr_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_SC33)
         Sc33_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_ZMXY)
         Zmxy_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_SHHJ)
         Shhj_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_QLFH)
         Qlfh_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_ZMWS)
         Zmws_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_LTZJ)
         Ltzj_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_DWCT)
         Dwct_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_YQZD)
         Yqzd_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_HALY)
         Haly_IsReady() ? 1 : 0
#else
         Blzz_IsReady() ? 1 : 0
#endif
#else
         0
#endif
         ,
#ifdef ENABLE_UNITY_HOOK
#if defined(STARRY_GAME_HZSJ)
         Hzsj_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_ZJMC)
         Zjmc_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_TLGD)
         Tlgd_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_DZQS)
         Dzqs_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_CSTJJS)
         Cstjjs_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_NEON)
         Neon_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_WHRHX)
         Whrhx_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_WHRHXKB)
         Whrhx_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_WGDZZ)
         Wgdzz_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_BJDW)
         Bjdw_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_ENDFIELD)
         Endfield_IsReady() ? 1 : 0
         Wxsj_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_WDYZ)
         Wdyz_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_WGGLS)
         Wggls_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_MILTHM)
         Milthm_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_YWGN)
         Ywgn_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_PHIGROS)
         Phigros_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_ES2)
         Es2_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_MATCH2)
         Match2_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_ZMXS)
         Zmxs_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_WZYM)
         Wzym_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_PCR)
         Pcr_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_GYWS)
         Gyws_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_GJC2)
         Gjc2_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_GJC2INTL)
         Gjc2Intl_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_SHLT)
         Shlt_IsReady() ? 1 : 0
#elif defined(STARRY_GAME_WXSJ)
         Wxsj_IsReady() ? 1 : 0
#else
         Zhmr_IsReady() ? 1 : 0
#endif
#else
         0
#endif
    );

    static bool s_clearedOnDeadSession = false;
    while (true) {
        if (!session_live()) {
#ifdef ENABLE_COCOS_HOOK
#if defined(STARRY_GAME_CWJH)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Cwjh_IsReady()) {
                Cwjh_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_ZXR)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Zxr_IsReady()) {
                Zxr_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_SC33)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Sc33_IsReady()) {
                Sc33_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_ZMXY)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Zmxy_IsReady()) {
                Zmxy_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_SHHJ)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Shhj_IsReady()) {
                Shhj_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_QLFH)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Qlfh_IsReady()) {
                Qlfh_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_ZMWS)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Zmws_IsReady()) {
                Zmws_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_LTZJ)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Ltzj_IsReady()) {
                Ltzj_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_DWCT)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Dwct_IsReady()) {
                s_clearedOnDeadSession = true;
            }
#elif defined(STARRY_GAME_YQZD)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Yqzd_IsReady()) {
                s_clearedOnDeadSession = true; // 零hook无残留状态，无需清除
            }
#elif defined(STARRY_GAME_HALY)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Haly_IsReady()) {
                Haly_ForceClear();
                s_clearedOnDeadSession = true;
            }
#else
            if (!s_clearedOnDeadSession && g_engine == ENGINE_COCOS && Blzz_IsReady()) {
                Blzz_ForceClear();
                s_clearedOnDeadSession = true;
            }
#endif
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_HZSJ)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Hzsj_IsReady()) {
                Hzsj_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ZJMC)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Zjmc_IsReady()) {
                Zjmc_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_TLGD)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Tlgd_IsReady()) {
                Tlgd_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_DZQS)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Dzqs_IsReady()) {
                Dzqs_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_CSTJJS)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Cstjjs_IsReady()) {
                Cstjjs_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_NEON)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Neon_IsReady()) {
                Neon_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WHRHX)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Whrhx_IsReady()) {
                Whrhx_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WHRHXKB)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Whrhx_IsReady()) {
                Whrhx_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WGDZZ)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Wgdzz_IsReady()) {
                Wgdzz_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_BJDW)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Bjdw_IsReady()) {
                Bjdw_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ENDFIELD)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Endfield_IsReady()) {
                Endfield_ForceClear();
                s_clearedOnDeadSession = true;
            }
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Wxsj_IsReady()) {
                Wxsj_ForceClear();
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WDYZ)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Wdyz_IsReady()) {
                Wdyz_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WGGLS)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Wggls_IsReady()) {
                Wggls_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_MILTHM)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Milthm_IsReady()) {
                Milthm_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_YWGN)
            // 过检旁路不依赖卡密；未登录也要吞「游戏环境异常」
            if (g_engine == ENGINE_UNITY) {
                Ywgn_TryInit();
                Ywgn_SecurityTick();
            }
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Ywgn_IsReady()) {
                Ywgn_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_PHIGROS)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Phigros_IsReady()) {
                Phigros_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ES2)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Es2_IsReady()) {
                Es2_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_MATCH2)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Match2_IsReady()) {
                Match2_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ZMXS)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Zmxs_IsReady()) {
                Zmxs_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WZYM)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Wzym_IsReady()) {
                Wzym_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_GYWS)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Gyws_IsReady()) {
                Gyws_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_GJC2)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Gjc2_IsReady()) {
                Gjc2_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_GJC2INTL)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Gjc2Intl_IsReady()) {
                Gjc2Intl_ForceClear();
                s_clearedOnDeadSession = true;
            }
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_SHLT)
            if (!s_clearedOnDeadSession && g_engine == ENGINE_UNITY && Shlt_IsReady()) {
                Shlt_ForceClear();
                s_clearedOnDeadSession = true;
            }
#endif
#if defined(ENABLE_UNITY_HOOK) && (defined(STARRY_GAME_WHRHX) || defined(STARRY_GAME_WHRHXKB)) && defined(STARRY_TRACE)
            if (g_engine == ENGINE_UNITY) {
                if (!Whrhx_IsReady()) Whrhx_TryInit();
                if (Whrhx_IsReady()) Whrhx_Tick(g_features);
            }
#endif
            usleep(500000);
            continue;
        }
        s_clearedOnDeadSession = false;
#ifdef ENABLE_COCOS_HOOK
#if defined(STARRY_GAME_CWJH)
        if (g_engine == ENGINE_COCOS && Cwjh_IsReady()) {
            Cwjh_Tick(g_features);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_ZXR)
        if (g_engine == ENGINE_COCOS && Zxr_IsReady()) {
            Zxr_Tick(g_features);
            if (g_values[ZXR_FEAT_SPEEDUP] > 0)
                Zxr_OnValueChanged(ZXR_FEAT_SPEEDUP, g_values[ZXR_FEAT_SPEEDUP]);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_SC33)
        if (g_engine == ENGINE_COCOS && Sc33_IsReady()) {
            Sc33_Tick(g_features);
            if (g_values[SC33_FEAT_ATK_SPEED] > 0)
                Sc33_OnValueChanged(SC33_FEAT_ATK_SPEED, g_values[SC33_FEAT_ATK_SPEED]);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_ZMXY)
        if (g_engine == ENGINE_COCOS && Zmxy_IsReady()) {
            Zmxy_Tick(g_features);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_SHHJ)
        if (g_engine == ENGINE_COCOS && Shhj_IsReady()) {
            Shhj_Tick(g_features);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_QLFH)
        if (g_engine == ENGINE_COCOS && Qlfh_IsReady()) {
            Qlfh_Tick(g_features);
            if (g_values[QLFH_FEAT_SPEED] > 0)
                Qlfh_OnValueChanged(QLFH_FEAT_SPEED, g_values[QLFH_FEAT_SPEED]);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_ZMWS)
        if (g_engine == ENGINE_COCOS && Zmws_IsReady()) {
            Zmws_Tick(g_features);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_LTZJ)
        if (g_engine == ENGINE_COCOS && Ltzj_IsReady()) {
            Ltzj_Tick(g_features);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_DWCT)
        if (g_engine == ENGINE_COCOS) {
            if (!Dwct_IsReady()) Dwct_TryInit(g_jvm);
            if (Dwct_IsReady()) Dwct_Tick(g_features);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_YQZD)
        if (g_engine == ENGINE_COCOS) {
            if (!Yqzd_IsReady()) Yqzd_TryInit(g_jvm);
            if (Yqzd_IsReady()) Yqzd_Tick();
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_HALY)
        if (g_engine == ENGINE_COCOS) {
            if (!Haly_IsReady()) Haly_TryInit(g_jvm);
            if (Haly_IsReady()) Haly_Tick(g_features);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_CWJH)
        if (g_engine == ENGINE_COCOS) {
            if (!Cwjh_IsReady()) Cwjh_TryInit(g_jvm);
            if (Cwjh_IsReady()) Cwjh_Tick(g_features);
            usleep(500000);
            continue;
        }
#else
        if (g_engine == ENGINE_COCOS && Blzz_IsReady()) {
            Blzz_Tick(g_features);
            usleep(500000);
            continue;
        }
#endif
#endif
#ifdef ENABLE_UNITY_HOOK
#if defined(STARRY_GAME_HZSJ)
        if (g_engine == ENGINE_UNITY) {
            if (!Hzsj_IsReady()) Hzsj_TryInit();
            Hzsj_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_ZJMC)
        if (g_engine == ENGINE_UNITY) {
            if (!Zjmc_IsReady()) Zjmc_TryInit();
            if (Zjmc_IsReady()) Zjmc_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_TLGD)
        if (g_engine == ENGINE_UNITY) {
            if (!Tlgd_IsReady()) Tlgd_TryInit();
            if (Tlgd_IsReady()) Tlgd_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_DZQS)
        if (g_engine == ENGINE_UNITY) {
            if (!Dzqs_IsReady()) Dzqs_TryInit();
            if (Dzqs_IsReady()) Dzqs_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_CSTJJS)
        if (g_engine == ENGINE_UNITY) {
            if (!Cstjjs_IsReady()) Cstjjs_TryInit();
            if (Cstjjs_IsReady()) Cstjjs_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_NEON)
        if (g_engine == ENGINE_UNITY) {
            if (!Neon_IsReady()) Neon_TryInit();
            if (Neon_IsReady()) {
                Neon_Tick(g_features);
                if (g_values[NEON_FEAT_MOVE_SPEED] > 0)
                    Neon_OnValueChanged(NEON_FEAT_MOVE_SPEED, g_values[NEON_FEAT_MOVE_SPEED]);
            }
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_WHRHX)
        if (g_engine == ENGINE_UNITY) {
            if (!Whrhx_IsReady()) Whrhx_TryInit();
            if (Whrhx_IsReady()) {
                Whrhx_Tick(g_features);
                if (g_values[WHRHX_FEAT_SPEED] > 0)
                    Whrhx_OnValueChanged(WHRHX_FEAT_SPEED, g_values[WHRHX_FEAT_SPEED]);
            }
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_WHRHXKB)
        if (g_engine == ENGINE_UNITY) {
            if (!Whrhx_IsReady()) Whrhx_TryInit();
            if (Whrhx_IsReady()) {
                Whrhx_Tick(g_features);
                if (g_values[WHRHX_FEAT_SPEED] > 0)
                    Whrhx_OnValueChanged(WHRHX_FEAT_SPEED, g_values[WHRHX_FEAT_SPEED]);
            }
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_WGDZZ)
        if (g_engine == ENGINE_UNITY) {
            if (!Wgdzz_IsReady()) Wgdzz_TryInit();
            if (Wgdzz_IsReady()) Wgdzz_Tick(g_features);
            usleep(500000);
            continue;
        }
#elif defined(STARRY_GAME_BJDW)
        if (g_engine == ENGINE_UNITY) {
            if (!Bjdw_IsReady()) Bjdw_TryInit();
            if (Bjdw_IsReady()) Bjdw_Tick(g_features);
            usleep(300000);
            continue;
        }
#elif defined(STARRY_GAME_ENDFIELD)
        if (g_engine == ENGINE_UNITY) {
            if (!Endfield_IsReady()) Endfield_TryInit();
            if (Endfield_IsReady()) Endfield_Tick(g_features);
            usleep(300000);
            continue;
        }
#elif defined(STARRY_GAME_WDYZ)
        if (g_engine == ENGINE_UNITY) {
            if (!Wdyz_IsReady()) Wdyz_TryInit();
            if (Wdyz_IsReady()) Wdyz_Tick(g_features);
            usleep(300000);
            continue;
        }
#elif defined(STARRY_GAME_WGGLS)
        if (g_engine == ENGINE_UNITY) {
            if (!Wggls_IsReady()) Wggls_TryInit();
            if (Wggls_IsReady()) {
                Wggls_Tick(g_features);
                if (g_values[WGGLS_FEAT_SPEED] > 0)
                    Wggls_OnValueChanged(WGGLS_FEAT_SPEED, g_values[WGGLS_FEAT_SPEED]);
            }
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_MILTHM)
        if (g_engine == ENGINE_UNITY) {
            if (!Milthm_IsReady()) Milthm_TryInit();
            if (Milthm_IsReady()) Milthm_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_YWGN)
        if (g_engine == ENGINE_UNITY) {
            if (!Ywgn_IsReady()) Ywgn_TryInit();
            Ywgn_Tick(g_features);
            Ywgn_SyncPctSliders(g_values);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_PHIGROS)
        if (g_engine == ENGINE_UNITY) {
            if (!Phigros_IsReady()) Phigros_TryInit();
            Phigros_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_ES2)
        if (g_engine == ENGINE_UNITY) {
            if (!Es2_IsReady()) Es2_TryInit();
            Es2_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_MATCH2)
        if (g_engine == ENGINE_UNITY && Match2_IsReady()) {
            Match2_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_ZMXS)
        if (g_engine == ENGINE_UNITY && Zmxs_IsReady()) {
            Zmxs_Tick(g_features);
            if (g_values[ZMXS_FEAT_SPEED] > 0)
                Zmxs_OnValueChanged(ZMXS_FEAT_SPEED, g_values[ZMXS_FEAT_SPEED]);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_WZYM)
        if (g_engine == ENGINE_UNITY && Wzym_IsReady()) {
            Wzym_Tick(g_features);
            // 滑条值同步（避免只改 UI 未回调时倍率卡住）
            if (g_values[WZYM_FEAT_DMG_MUL] > 0)
                Wzym_OnValueChanged(WZYM_FEAT_DMG_MUL, g_values[WZYM_FEAT_DMG_MUL]);
            if (g_values[WZYM_FEAT_SPEED] > 0)
                Wzym_OnValueChanged(WZYM_FEAT_SPEED, g_values[WZYM_FEAT_SPEED]);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_PCR)
        if (g_engine == ENGINE_UNITY) {
            if (!Pcr_IsReady()) Pcr_TryInit();
            Pcr_Tick(g_features);
            if (g_values[PCR_FEAT_ATTR_VAL] > 0)
                Pcr_OnValueChanged(PCR_FEAT_ATTR_VAL, g_values[PCR_FEAT_ATTR_VAL]);
            if (g_values[PCR_FEAT_SPEED] > 0)
                Pcr_OnValueChanged(PCR_FEAT_SPEED, g_values[PCR_FEAT_SPEED]);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_GYWS)
        if (g_engine == ENGINE_UNITY) {
            if (!Gyws_IsReady()) Gyws_TryInit();
            Gyws_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_GJC2)
        if (g_engine == ENGINE_UNITY) {
            if (g_features[GJC2_FEAT_INVIS] || g_features[GJC2_FEAT_FULLSCREEN] ||
                g_features[GJC2_FEAT_STEAL_SKILL]) {
                if (!Gjc2_IsReady()) Gjc2_TryInit();
                Gjc2_Tick(g_features);
            }
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_GJC2INTL)
        if (g_engine == ENGINE_UNITY) {
            if (!Gjc2Intl_IsReady()) Gjc2Intl_TryInit();
            Gjc2Intl_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_SHLT)
        if (g_engine == ENGINE_UNITY) {
            if (!Shlt_IsReady()) Shlt_TryInit();
            Shlt_Tick(g_features);
            usleep(200000);
            continue;
        }
#elif defined(STARRY_GAME_ZHMR)
        if (g_engine == ENGINE_UNITY && Zhmr_IsReady()) {
            Zhmr_Tick(g_features);
            usleep(200000);
            continue;
        }
#endif
#endif
        usleep(100000);
    }

    return nullptr;
}

// ============================================================
//  JNI 工具
// ============================================================

static JNIEnv* GetJNIEnv() {
    if (!g_jvm) return nullptr;
    JNIEnv* env = nullptr;
    if (g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6) == JNI_EDETACHED) {
        g_jvm->AttachCurrentThread(&env, nullptr);
    }
    return env;
}

// ============================================================
//  Toast 回调（C → Java → JS）
// ============================================================

static void ShowToast(const char* text) {
    JNIEnv* env = GetJNIEnv();
    if (!env || !g_context) return;

    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/menu/MenuService"));
    if (!cls) return;

    jmethodID mid = env->GetStaticMethodID(cls, "getInstance",
        "()Lcom/starry/magic/menu/MenuService;");
    if (!mid) return;

    jobject service = env->CallStaticObjectMethod(cls, mid);
    if (!service) return;

    jmethodID toastMid = env->GetMethodID(cls, "showToast", "(Ljava/lang/String;)V");
    if (toastMid) {
        jstring jstr = env->NewStringUTF(text);
        env->CallVoidMethod(service, toastMid, jstr);
        env->DeleteLocalRef(jstr);
    }

    env->DeleteLocalRef(service);
    env->DeleteLocalRef(cls);
}

// 导出给 Games/*Mod（如 WxsjMod 失配保护）崩溃前提示复用
extern "C" void StarryShowToast(const char* text) { ShowToast(text); }

// ============================================================
//  JNI 方法实现
// ============================================================

extern "C" {

// ===== nativeInit =====
static void mb_nativeInit(JNIEnv* env, jclass clazz, jobject context) {
    if (g_initialized) return;

    LOGI("===== 星光魔法 MOD 菜单初始化 =====");
    g_context = env->NewGlobalRef(context);
    g_initialized = true;

    // 检测游戏引擎
    g_engine = detectGameEngine();
    if (g_engine == ENGINE_UNKNOWN) {
        LOGW("未检测到已知游戏引擎，进入通用模式");
        LOGI("通用模式下，请手动配置目标库和 Hook 偏移");
    }

    // 启动后台线程
    pthread_t tid;
    pthread_create(&tid, nullptr, mod_thread, nullptr);
    pthread_detach(tid);
}

// ===== nativeSetFeature (开关) =====
static void mb_nativeSetFeature(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName, jboolean enabled) {

    const char* name = env->GetStringUTFChars(featName, nullptr);
    // 未授权时禁止开启；关闭始终允许（用于 lock / OnFeatureChanged 回落）
    if (enabled && !session_live()) {
        LOGW("[开关] 拒绝开启 #%d %s（未授权）", featNum, name ? name : "");
        env->ReleaseStringUTFChars(featName, name);
        return;
    }
    if (featNum >= 0 && featNum < 64) {
        g_features[featNum] = enabled;
    }
#ifdef ENABLE_COCOS_HOOK
#if defined(STARRY_GAME_CWJH)
    Cwjh_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_ZXR)
    Zxr_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_SC33)
    Sc33_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_ZMXY)
    Zmxy_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_SHHJ)
    Shhj_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_QLFH)
    Qlfh_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_ZMWS)
    Zmws_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_LTZJ)
    Ltzj_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_DWCT)
    Dwct_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_YQZD)
    Yqzd_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_HALY)
    Haly_OnFeatureChanged(featNum, enabled);
#else
    Blzz_OnFeatureChanged(featNum, enabled);
#endif
#endif
#ifdef ENABLE_UNITY_HOOK
#if defined(STARRY_GAME_HZSJ)
    Hzsj_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_ZJMC)
    Zjmc_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_TLGD)
    Tlgd_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_DZQS)
    Dzqs_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_CSTJJS)
    Cstjjs_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_NEON)
    Neon_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_WHRHX)
    Whrhx_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_WHRHXKB)
    Whrhx_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_WGDZZ)
    Wgdzz_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_BJDW)
    Bjdw_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_ENDFIELD)
    Endfield_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_WXSJ)
    Wxsj_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_WGGLS)
    Wggls_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_MILTHM)
    Milthm_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_YWGN)
    Ywgn_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_PHIGROS)
    Phigros_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_ES2)
    Es2_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_MATCH2)
    Match2_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_ZMXS)
    Zmxs_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_WZYM)
    Wzym_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_PCR)
    Pcr_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_GYWS)
    Gyws_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_GJC2)
    Gjc2_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_GJC2INTL)
    Gjc2Intl_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_SHLT)
    Shlt_OnFeatureChanged(featNum, enabled);
#elif defined(STARRY_GAME_WDYZ)
    Wdyz_OnFeatureChanged(featNum, enabled);
#else
    Zhmr_OnFeatureChanged(featNum, enabled);
#endif
#endif
    LOGI("[开关] #%d %s = %s", featNum, name, enabled ? "ON" : "OFF");
    env->ReleaseStringUTFChars(featName, name);
}

// ===== nativeSetAuthorized（诱饵：true  alone 不能放行功能） =====
static void mb_nativeSetAuthorized(JNIEnv* env, jclass clazz,
        jboolean authorized) {
    g_authorized = authorized;
    if (!authorized) {
        clear_runtime_session();
    }
    LOGI("[验证] flag=%s live=%s", authorized ? "true" : "false", session_live() ? "1" : "0");
}

// ===== 短票验签并激活运行时会话 =====
static jboolean mb_nativeApplyGrant(JNIEnv* env, jclass clazz,
        jstring jGrant, jstring jDevice, jstring jGame) {
    if (!jGrant || !jDevice || !jGame) return JNI_FALSE;
    const char* grant = env->GetStringUTFChars(jGrant, nullptr);
    const char* device = env->GetStringUTFChars(jDevice, nullptr);
    const char* game = env->GetStringUTFChars(jGame, nullptr);
    // 续票失败不得清活会话：登录后 session 永不过期（见 session_live 注释），
    // 一张坏票/过期票只代表本次续票失败，等下次续票重试，不把已开功能全清
    bool wasLive = session_live();
    bool ok = apply_grant_utf8(grant, device, game);
    env->ReleaseStringUTFChars(jGrant, grant);
    env->ReleaseStringUTFChars(jDevice, device);
    env->ReleaseStringUTFChars(jGame, game);
    if (!ok) {
        if (wasLive) {
            LOGW("[验证] grant rejected (session kept)");
        } else {
            clear_runtime_session();
            LOGW("[验证] grant rejected");
        }
        return JNI_FALSE;
    }
    return JNI_TRUE;
}

static jboolean mb_nativeHasLiveSession(JNIEnv* env, jclass clazz) {
    return session_live() ? JNI_TRUE : JNI_FALSE;
}

static void mb_nativeClearGrant(JNIEnv* env, jclass clazz) {
    clear_runtime_session();
}

// ===== OTA manifest 验签：payload = game_id|ver|json_sha|so_sha|note =====
// 与服务端 ota_store.sign_payload 一致（hmac_sha256(grant_hmac_secret, payload)）。
static jboolean mb_nativeVerifyOtaSig(JNIEnv* env, jclass clazz,
        jstring jPayload, jstring jSig) {
    if (!jPayload || !jSig) return JNI_FALSE;
    const char* payload = env->GetStringUTFChars(jPayload, nullptr);
    const char* sig = env->GetStringUTFChars(jSig, nullptr);
    if (!payload || !sig) {
        if (payload) env->ReleaseStringUTFChars(jPayload, payload);
        if (sig) env->ReleaseStringUTFChars(jSig, sig);
        return JNI_FALSE;
    }
    uint8_t mac[32];
    const char* pepper = grant_pepper();
    hmac_sha256((const uint8_t*)pepper, strlen(pepper),
                (const uint8_t*)payload, strlen(payload), mac);
    bool ok = hex_eq_ci(sig, mac, 32);
    env->ReleaseStringUTFChars(jPayload, payload);
    env->ReleaseStringUTFChars(jSig, sig);
    if (!ok) LOGW("[OTA] 补丁签名校验失败");
    return ok ? JNI_TRUE : JNI_FALSE;
}

// ===== 构建戳：so 内烘焙的 epoch 字面量（OTA 守卫判包内 so / OTA so 新旧）=====
// 格式 "STARRYSTAMP1:EPOCH=<秒>;"，Java 侧也直接扫 so 字节，这里给日志/自检用。
static jstring mb_nativeGetBuildStamp(JNIEnv* env, jclass clazz) {
    return env->NewStringUTF(starry_build_stamp_marker());
}

// ===== OTA patch.json 应用：按当前游戏分发到 Xxx_ApplyPlayPatch =====
static void mb_nativeApplyPlayPatch(JNIEnv* env, jclass clazz,
        jstring jJson) {
    if (!jJson) return;
    const char* json = env->GetStringUTFChars(jJson, nullptr);
    if (!json || !*json) {
        if (json) env->ReleaseStringUTFChars(jJson, json);
        return;
    }
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_HZSJ)
    Hzsj_ApplyPlayPatch(json);
    LOGI("[OTA] 玩法 patch.json 已应用（hzsj）");
#else
    // 其余游戏暂未提供偏移热更入口：patch.json 下载后不改变 native 偏移。
    LOGW("[OTA] 当前游戏未实现偏移热更，patch.json 已忽略");
#endif
    env->ReleaseStringUTFChars(jJson, json);
}

// ===== nativeSetValue (滑块/下拉) =====
static void mb_nativeSetValue(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName, jint value) {

    const char* name = env->GetStringUTFChars(featName, nullptr);
    if (!session_live()) {
        LOGW("[数值] 拒绝 #%d（未授权）", featNum);
        env->ReleaseStringUTFChars(featName, name);
        return;
    }
    if (featNum >= 0 && featNum < 64) {
        g_values[featNum] = value;
    }
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_HZSJ)
    Hzsj_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ZJMC)
    Zjmc_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_CSTJJS)
    Cstjjs_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_NEON)
    Neon_OnValueChanged(featNum, value);
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WGGLS)
    Wggls_OnValueChanged(featNum, value);
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WHRHX)
    Whrhx_OnValueChanged(featNum, value);
#elif defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WHRHXKB)
    Whrhx_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WZYM)
    Wzym_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_PCR)
    Pcr_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_YWGN)
    Ywgn_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_COCOS_HOOK) && defined(STARRY_GAME_ZXR)
    Zxr_OnValueChanged(featNum, value);
#elif defined(ENABLE_COCOS_HOOK) && defined(STARRY_GAME_SC33)
    Sc33_OnValueChanged(featNum, value);
#elif defined(ENABLE_COCOS_HOOK) && defined(STARRY_GAME_YQZD)
    Yqzd_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_ZMXS)
    Zmxs_OnValueChanged(featNum, value);
#endif
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_WXSJ)
    Wxsj_OnValueChanged(featNum, value);
#endif
    LOGI("[数值] #%d %s = %d", featNum, name, value);
    env->ReleaseStringUTFChars(featName, name);
}

// ===== nativeSetLongValue =====
static void mb_nativeSetLongValue(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName, jlong value) {
    LOGI("[长整] #%d = %lld", featNum, (long long)value);
}

// ===== nativeSetText (输入框) =====
static void mb_nativeSetText(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName, jstring text) {

    const char* name = env->GetStringUTFChars(featName, nullptr);
    const char* txt  = env->GetStringUTFChars(text, nullptr);

    if (!session_live()) {
        LOGW("[文本] 拒绝 #%d（未授权）", featNum);
        env->ReleaseStringUTFChars(featName, name);
        env->ReleaseStringUTFChars(text, txt);
        return;
    }
    if (featNum >= 0 && featNum < 64) {
        strncpy(g_texts[featNum], txt, sizeof(g_texts[featNum]) - 1);
    }
#if defined(ENABLE_UNITY_HOOK) && defined(STARRY_GAME_PCR)
    // InputValue 控件不走 nativeSetValue/g_values，PCR 在此消费输入（1–999999）
    Pcr_OnInputChanged(featNum, txt);
#endif
    LOGI("[文本] #%d %s = %s", featNum, name, txt);

    env->ReleaseStringUTFChars(featName, name);
    env->ReleaseStringUTFChars(text, txt);
}

// ===== nativeExecButton (执行按钮) =====
static void mb_nativeExecButton(JNIEnv* env, jclass clazz,
        jint featNum, jstring featName) {

    const char* name = env->GetStringUTFChars(featName, nullptr);
    if (!session_live()) {
        LOGW("[按钮] 拒绝 #%d %s（未授权）", featNum, name ? name : "");
        env->ReleaseStringUTFChars(featName, name);
        return;
    }
    if (featNum >= 0 && featNum < 64) {
        g_btnTrigger[featNum] = true;
    }
    // 按钮不走 nativeSetFeature，需主动通知游戏模块（如解锁全部）
#ifdef ENABLE_COCOS_HOOK
#if defined(STARRY_GAME_ZXR)
    Zxr_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_SC33)
    Sc33_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_ZMXY)
    Zmxy_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_SHHJ)
    Shhj_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_QLFH)
    Qlfh_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_ZMWS)
    Zmws_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_LTZJ)
    Ltzj_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_DWCT)
    Dwct_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_HALY)
    Haly_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_CWJH)
    Cwjh_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_YQZD)
    Yqzd_OnFeatureChanged(featNum, true); // 按钮型功能暂无，直通保持链完整
#else
    {
        int blzzFeat = featNum;
        if (name && strstr(name, "解锁所有")) {
            blzzFeat = BLZZ_FEAT_UNLOCK;
        }
        Blzz_OnFeatureChanged(blzzFeat, true);
    }
#endif
#endif
#ifdef ENABLE_UNITY_HOOK
#if defined(STARRY_GAME_HZSJ)
    Hzsj_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_ZJMC)
    Zjmc_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_TLGD)
    Tlgd_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_DZQS)
    Dzqs_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_CSTJJS)
    Cstjjs_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_NEON)
    Neon_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_WHRHX)
    Whrhx_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_WHRHXKB)
    Whrhx_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_WGDZZ)
    Wgdzz_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_BJDW)
    Bjdw_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_ENDFIELD)
    Endfield_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_WDYZ)
    Wdyz_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_WGGLS)
    Wggls_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_MILTHM)
    Milthm_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_YWGN)
    Ywgn_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_PHIGROS)
    Phigros_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_ES2)
    Es2_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_MATCH2)
    Match2_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_ZMXS)
    Zmxs_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_WZYM)
    Wzym_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_PCR)
    Pcr_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_GYWS)
    Gyws_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_GJC2)
    Gjc2_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_GJC2INTL)
    Gjc2Intl_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_SHLT)
    Shlt_OnFeatureChanged(featNum, true);
#elif defined(STARRY_GAME_WXSJ)
    Wxsj_OnFeatureChanged(featNum, true);
#else
    Zhmr_OnFeatureChanged(featNum, true);
#endif
#endif
    LOGI("[按钮] #%d %s", featNum, name);
    env->ReleaseStringUTFChars(featName, name);
}

// ===== nativeGetAuthGameId（GameConfig.h：STARRY_GAME_* → STARRY_AUTH_GAME_ID） =====
static jstring mb_nativeGetAuthGameId(JNIEnv* env, jclass clazz) {
    (void)clazz;
#ifndef STARRY_AUTH_GAME_ID
#error "STARRY_AUTH_GAME_ID missing — enable STARRY_GAME_* in Android.mk (mapped in GameConfig.h)"
#endif
    return env->NewStringUTF(STARRY_AUTH_GAME_ID);
}

// ===== AceGate：雷霆过检倒计时；其它游戏恒 0（避免 NativeMenu UnsatisfiedLinkError） =====
static jint mb_nativeGetAceGateRemainSec(JNIEnv* env, jclass clazz) {
    (void)env;
    (void)clazz;
    return 0;
}

// ===== nativeGetFeatureList =====
static jobjectArray mb_nativeGetFeatureList(JNIEnv* env, jclass clazz) {

    jclass stringClass = env->FindClass("java/lang/String");
    jobjectArray result = env->NewObjectArray(g_featureCount, stringClass,
                                               env->NewStringUTF(""));

    for (int i = 0; i < g_featureCount; i++) {
        env->SetObjectArrayElement(result, i, env->NewStringUTF(g_featureList[i]));
    }

    return result;
}

// ===== nativeGetSettingsList =====
static jobjectArray mb_nativeGetSettingsList(JNIEnv* env, jclass clazz) {

    const char* settings[] = {
        "Category_界面设置",
        "Button_关闭菜单",
    };

    int count = sizeof(settings) / sizeof(settings[0]);
    jclass stringClass = env->FindClass("java/lang/String");
    jobjectArray result = env->NewObjectArray(count, stringClass,
                                               env->NewStringUTF(""));

    for (int i = 0; i < count; i++) {
        env->SetObjectArrayElement(result, i, env->NewStringUTF(settings[i]));
    }

    return result;
}

// ===== nativeIsGameLibLoaded =====
static jboolean mb_nativeIsGameLibLoaded(JNIEnv* env, jclass clazz) {
    return g_gameLibLoaded ? JNI_TRUE : JNI_FALSE;
}

// ===== nativeGetPinnedCert =====
// 返回编译期烘焙进 so 的证书固定公钥（PEM 字节）；AuthClient 不再读 assets。
static jbyteArray mb_nativeGetPinnedCert(JNIEnv* env, jclass clazz) {
    (void)clazz;
    if (!starry_pinned_crt || starry_pinned_crt_len == 0) {
        LOGE("[桥接] 证书烘焙数据为空");
        return nullptr;
    }
    jbyteArray arr = env->NewByteArray((jsize)starry_pinned_crt_len);
    if (!arr) return nullptr;
    env->SetByteArrayRegion(arr, 0, (jsize)starry_pinned_crt_len,
                            (const jbyte*)starry_pinned_crt);
    return arr;
}

// Preferences.Changes 已废弃：偏好只写 SharedPreferences，C 状态由 ModBridge.nativeXxx 写入。
// 保留空实现以免旧 so/反射残留调用崩溃。
JNIEXPORT void JNICALL
Java_com_starry_magic_menu_Preferences_Changes(JNIEnv* env, jclass clazz,
        jobject context, jint featNum, jstring featName,
        jint value, jlong Lvalue, jboolean isOn, jstring inputText) {
    (void)env; (void)clazz; (void)context; (void)featNum; (void)featName;
    (void)value; (void)Lvalue; (void)isOn; (void)inputText;
}

// ============================================================
//  JNI_OnLoad
// ============================================================

// ============================================================
//  桥接注册（RegisterNatives，2026-09-06）
//  迁移自 Java_ 静态导出：符号表不再暴露鉴权桥。
//  签名与 ModBridge.java 的 native 声明逐一对应；一处注册失败
//  会在启动日志精确定位（[桥接] 注册失败: 方法名），全部成功才置位。
//  自测壳启动时会自动跑 BridgeSelfTest 逐一调用验证。
// ============================================================
static bool g_bridge_registered_ok = false;

static int g_bridge_skipped = 0;

static bool regOne(JNIEnv* env, jclass clazz, const char* name, const char* sig, void* fn) {
    JNINativeMethod m{name, sig, fn};
    if (env->RegisterNatives(clazz, &m, 1) != 0) {
        // 菜单 dex 可能落后于 so（如 templates/*/classes2.dex 未同步新增的 native 声明）。
        // 关键：ART 的 RegisterNatives 失败会抛出 NoSuchMethodError 挂起异常，若不清除并让
        // JNI_OnLoad 返回 JNI_ERR，System.loadLibrary 会直接把该异常抛给调用方，导致
        // StarryInitProvider.onCreate 中断 ⇒ MenuService/nativeInit/mod_thread 全链不启动
        // ⇒ 热更 dll 替换彻底失效（2026-09-26 实测事故）。
        // 因此「dex 未声明该方法」只记警告并跳过：Java 侧没有声明就永远不会调用它。
        if (env->ExceptionCheck()) env->ExceptionClear();
        g_bridge_skipped++;
        LOGW("[桥接] 跳过 dex 未声明的方法: %s %s（累计 %d）", name, sig, g_bridge_skipped);
        return true;
    }
    return true;
}

static jboolean mb_bridgeRegistered() { return g_bridge_registered_ok ? JNI_TRUE : JNI_FALSE; }
static bool registerBridgeNatives(JNIEnv* env) {
    static bool done = false;
    if (done) return true;   // 可重入：dlopen 构造函数与 System.loadLibrary 都会进 JNI_OnLoad
    jclass bridge = env->FindClass(OBFUSCATE("com/starry/magic/bridge/ModBridge"));
    if (!bridge) { LOGE("[桥接] FindClass ModBridge 失败"); return false; }
    bool ok = true;
    ok = regOne(env, bridge, OBFUSCATE("nativeInit"),              OBFUSCATE("(Landroid/content/Context;)V"),                         (void*)mb_nativeInit) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetFeature"),        OBFUSCATE("(ILjava/lang/String;Z)V"),                              (void*)mb_nativeSetFeature) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetValue"),          OBFUSCATE("(ILjava/lang/String;I)V"),                              (void*)mb_nativeSetValue) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetLongValue"),      OBFUSCATE("(ILjava/lang/String;J)V"),                              (void*)mb_nativeSetLongValue) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetText"),           OBFUSCATE("(ILjava/lang/String;Ljava/lang/String;)V"),             (void*)mb_nativeSetText) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeExecButton"),        OBFUSCATE("(ILjava/lang/String;)V"),                               (void*)mb_nativeExecButton) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetFeatureList"),    OBFUSCATE("()[Ljava/lang/String;"),                                (void*)mb_nativeGetFeatureList) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetSettingsList"),   OBFUSCATE("()[Ljava/lang/String;"),                                (void*)mb_nativeGetSettingsList) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeIsGameLibLoaded"),   OBFUSCATE("()Z"),                                                  (void*)mb_nativeIsGameLibLoaded) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeSetAuthorized"),     OBFUSCATE("(Z)V"),                                                 (void*)mb_nativeSetAuthorized) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeApplyGrant"),        OBFUSCATE("(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Z"), (void*)mb_nativeApplyGrant) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeHasLiveSession"),    OBFUSCATE("()Z"),                                                  (void*)mb_nativeHasLiveSession) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeClearGrant"),        OBFUSCATE("()V"),                                                  (void*)mb_nativeClearGrant) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetAuthGameId"),     OBFUSCATE("()Ljava/lang/String;"),                                 (void*)mb_nativeGetAuthGameId) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetAceGateRemainSec"), OBFUSCATE("()I"),                                                (void*)mb_nativeGetAceGateRemainSec) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeVerifyOtaSig"),      OBFUSCATE("(Ljava/lang/String;Ljava/lang/String;)Z"),              (void*)mb_nativeVerifyOtaSig) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeApplyPlayPatch"),    OBFUSCATE("(Ljava/lang/String;)V"),                                (void*)mb_nativeApplyPlayPatch) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetPinnedCert"),    OBFUSCATE("()[B"),                                                 (void*)mb_nativeGetPinnedCert) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeBridgeRegistered"),  OBFUSCATE("()Z"),                                                  (void*)mb_bridgeRegistered) && ok;
    ok = regOne(env, bridge, OBFUSCATE("nativeGetBuildStamp"),     OBFUSCATE("()Ljava/lang/String;"),                                 (void*)mb_nativeGetBuildStamp) && ok;
    if (ok) {
        done = true;
        g_bridge_registered_ok = true;
        LOGI("[桥接] RegisterNatives 20/20 完成（跳过 dex 未声明 %d 个，不影响 JNI_OnLoad）", g_bridge_skipped);
    }
    return ok;
}

JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM* vm, void* reserved) {
    g_jvm = vm;
    {
        JNIEnv* env = nullptr;
        if (vm->GetEnv((void**)&env, JNI_VERSION_1_6) == JNI_OK && env) {
            registerBridgeNatives(env);   // 失败可重试：下次 JNI_OnLoad（System.loadLibrary）再注册
        } else {
            LOGW("[桥接] JNI_OnLoad 无 JNIEnv，注册延后");
        }
    }
#if defined(STARRY_GAME_WZYM)
    // 默认 1=关闭；拖滑条且授权后才放大
    if (g_values[WZYM_FEAT_DMG_MUL] <= 0) g_values[WZYM_FEAT_DMG_MUL] = 1;
    if (g_values[WZYM_FEAT_SPEED] <= 0) g_values[WZYM_FEAT_SPEED] = 1;
#endif
#if defined(STARRY_GAME_YWGN)
    if (g_values[YWGN_FEAT_PCT_PPLUS] <= 0) g_values[YWGN_FEAT_PCT_PPLUS] = 85;
    if (g_values[YWGN_FEAT_PCT_PERFECT] <= 0) g_values[YWGN_FEAT_PCT_PERFECT] = 13;
    if (g_values[YWGN_FEAT_PCT_GREAT] <= 0) g_values[YWGN_FEAT_PCT_GREAT] = 1;
    if (g_values[YWGN_FEAT_PCT_GOOD] <= 0) g_values[YWGN_FEAT_PCT_GOOD] = 1;
    Ywgn_SyncPctSliders(g_values);
#endif
#if defined(STARRY_GAME_ZMXS)
    Zmxs_SetJvm(vm);
#endif
    LOGI("星光魔法 MOD 菜单已加载 (JNI_OnLoad)");
#if defined(STARRY_GAME_ZXR)
    // 抢在 .jsc → se::evalString 之前挂战斗拼接钩
    Zxr_StartEarlyHookThread(vm);
#elif defined(STARRY_GAME_SC33)
    Sc33_StartEarlyHookThread(vm);
#elif defined(STARRY_GAME_ZMXY)
    Zmxy_StartEarlyHookThread(vm);
#elif defined(STARRY_GAME_SHHJ)
    Shhj_StartEarlyHookThread(vm);
#elif defined(STARRY_GAME_QLFH)
    // 抢在明文 bundle → se::evalString 之前挂拼接钩
    Qlfh_StartEarlyHookThread(vm);
#elif defined(STARRY_GAME_CWJH)
    // 超维进化：game.js 启动即过 se::evalString，20ms 轮询抢在求值前挂钩
    Cwjh_StartEarlyHookThread(vm);
#elif defined(STARRY_GAME_WGDZZ)
    Wgdzz_StartEarlyHooks(vm);
    Wgdzz_StartMenuBootstrap(vm);
#elif defined(STARRY_GAME_ZMWS)
    Zmws_StartEarlyHookThread(vm);
#elif defined(STARRY_GAME_LTZJ)
    // 零 Root / 晚载共用
#if defined(STARRY_CAVE_INTEGRITY_SOFT)
    Ltzj_AceSuicideSoft12531();
#elif !defined(STARRY_CAVE_SKIP_ACESOFT)
    Ltzj_AceSoftPatch12531();
#endif
    Ltzj_StartMenuBootstrap(vm);
#endif
    return JNI_VERSION_1_6;
}

#if defined(STARRY_GAME_WGDZZ)
static JavaVM* Wgdzz_ResolveJvm() {
    typedef jint (*GetCreatedJavaVMs_t)(JavaVM**, jsize, jsize*);
    GetCreatedJavaVMs_t get_vms = nullptr;
    const char* candidates[] = {"libart.so", "libnativehelper.so", "libandroid_runtime.so", nullptr};
    for (int i = 0; candidates[i]; i++) {
        void* h = xdl_open(candidates[i], XDL_DEFAULT);
        if (!h) continue;
        get_vms = (GetCreatedJavaVMs_t)xdl_sym(h, "JNI_GetCreatedJavaVMs", nullptr);
        xdl_close(h);
        if (get_vms) break;
    }
    if (!get_vms) return nullptr;
    JavaVM* vm = nullptr;
    jsize n = 0;
    if (get_vms(&vm, 1, &n) != JNI_OK || !vm) return nullptr;
    return vm;
}

/** KwProtect K1 dlopen 触发；constructor 在 dlopen 时跑，补 JNI_OnLoad 未自动调用。 */
__attribute__((constructor)) static void Wgdzz_OnDlopen() {
    LOGI("Wgdzz_OnDlopen (dlopen constructor)");
    JavaVM* vm = Wgdzz_ResolveJvm();
    if (!vm) {
        LOGE("Wgdzz_OnDlopen: no JavaVM");
        return;
    }
    g_jvm = vm;
    Wgdzz_StartEarlyHooks(vm);
    Wgdzz_StartMenuBootstrap(vm);
}

/** KwProtect K1 dlopen 后备用入口：取 JavaVM 并起 sidecar 菜单。 */
__attribute__((visibility("default"))) void Starry_CaveBoot(void) {
    typedef jint (*GetCreatedJavaVMs_t)(JavaVM**, jsize, jsize*);
    LOGI("Starry_CaveBoot enter (Wgdzz)");
    const char* candidates[] = {
        "libart.so",
        "libnativehelper.so",
        "libandroid_runtime.so",
        nullptr,
    };
    GetCreatedJavaVMs_t get_vms = nullptr;
    for (int i = 0; candidates[i]; i++) {
        void* h = xdl_open(candidates[i], XDL_DEFAULT);
        if (!h) continue;
        get_vms = (GetCreatedJavaVMs_t)xdl_sym(h, "JNI_GetCreatedJavaVMs", nullptr);
        xdl_close(h);
        if (get_vms) break;
    }
    if (!get_vms) {
        LOGE("Starry_CaveBoot: JNI_GetCreatedJavaVMs not found");
        return;
    }
    JavaVM* vm = nullptr;
    jsize n = 0;
    if (get_vms(&vm, 1, &n) != JNI_OK || !vm) {
        LOGE("Starry_CaveBoot: no JavaVM");
        return;
    }
    g_jvm = vm;
    LOGI("星光魔法 MOD 菜单已加载 (Wgdzz CaveBoot)");
    Wgdzz_StartEarlyHooks(vm);
    Wgdzz_StartMenuBootstrap(vm);
}
#endif

#if defined(STARRY_GAME_LTZJ)
/** cocos DT_INIT cave 入口：app namespace 里 dlopen(libart) 会失败，用 xDL 从已映射 libart 解析 GetCreatedJavaVMs。 */
__attribute__((visibility("default"))) void Starry_CaveBoot(void) {
    typedef jint (*GetCreatedJavaVMs_t)(JavaVM**, jsize, jsize*);
    LOGI("Starry_CaveBoot enter");
    const char* candidates[] = {
        "libart.so",
        "libnativehelper.so",
        "libandroid_runtime.so",
        nullptr,
    };
    GetCreatedJavaVMs_t get_vms = nullptr;
    for (int i = 0; candidates[i]; i++) {
        void* h = xdl_open(candidates[i], XDL_DEFAULT);
        if (!h) {
            LOGI("Starry_CaveBoot xdl_open miss %s", candidates[i]);
            continue;
        }
        get_vms = (GetCreatedJavaVMs_t)xdl_sym(h, "JNI_GetCreatedJavaVMs", nullptr);
        LOGI("Starry_CaveBoot %s GetCreatedJavaVMs=%p", candidates[i], (void*)get_vms);
        xdl_close(h);
        if (get_vms) break;
    }
    if (!get_vms) {
        LOGE("Starry_CaveBoot: JNI_GetCreatedJavaVMs not found");
#if !defined(STARRY_CAVE_SKIP_ACESOFT)
        Ltzj_AceSoftPatch12531();  // 至少先软补，抢延迟窗
#endif
        return;
    }
    JavaVM* vm = nullptr;
    jsize n = 0;
    jint rc = get_vms(&vm, 1, &n);
    LOGI("Starry_CaveBoot get_vms rc=%d n=%d vm=%p", (int)rc, (int)n, (void*)vm);
    if (rc != JNI_OK || !vm) {
        LOGE("Starry_CaveBoot: no JavaVM");
#if !defined(STARRY_CAVE_SKIP_ACESOFT)
        Ltzj_AceSoftPatch12531();
#endif
        return;
    }
#if defined(STARRY_CAVE_SKIP_ACESOFT)
    // H5g 对照：只起菜单，不打 AceSoft，看 2s SIGBUS 是否由软补加速
    g_jvm = vm;
    LOGI("星光魔法 MOD 菜单已加载 (CaveBoot menu-only, AceSoft skipped)");
    Ltzj_StartMenuBootstrap(vm);
#elif defined(STARRY_CAVE_INTEGRITY_SOFT)
    // H5i2：只拦自杀（kill GOT + SIGBUS），不做 IntegritySoft（H5i 叠完整性后变 ~3s SIGSEGV）
    g_jvm = vm;
    LOGI("星光魔法 MOD 菜单已加载 (CaveBoot + SuicideSoft H5i6 exit_group7-RX)");
    Ltzj_AceSuicideSoft12531();
    Ltzj_StartMenuBootstrap(vm);
#else
    JNI_OnLoad(vm, nullptr);
#endif
}
#endif

} // extern "C"
