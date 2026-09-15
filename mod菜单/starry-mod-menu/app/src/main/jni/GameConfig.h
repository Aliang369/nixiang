/* ================================================================
 *  GameConfig.h — 游戏适配配置（⚠️ LEGACY 专属）
 *
 *  ⚠️ 双轨告示（2026-09-09 起）：新游戏禁止在本文件加任何映射/配置！
 *    modern 路线（MainModern.cpp 注册表）的 game_id 在 Games/{Game}Entry.cpp
 *    的 authGameId 字段，目标库在 waitLib；本文件仅服务 legacy 存量游戏。
 *    新游戏流程 = scripts/new_game_mod.sh 四件套（见 SWITCH_GAME.md 顶部）。
 *
 *  legacy 用法（存量游戏维持）：切游戏只改 Android.mk 里启用的
 *  STARRY_GAME_*（+ ENABLE_*_HOOK / LOCAL_SRC_FILES）。
 *  鉴权 game_id（STARRY_AUTH_GAME_ID）由此文件根据 STARRY_GAME_* 自动映射，
 *  禁止再在 Android.mk 里单独 -DSTARRY_AUTH_GAME_ID。
 * ================================================================ */

#ifndef STARRY_GAME_CONFIG_H
#define STARRY_GAME_CONFIG_H

#include <cstdint>
#include <cstddef>

#include "Includes/SecStrings.hpp" // sec_lib_il2cpp/sec_lib_cocosjs（GameConfig 各分支 TARGET_LIB 依赖）

// ---------- 鉴权 game_id（后台 games.game_id / 卡密 scope 必须同名）----------
#ifdef STARRY_AUTH_GAME_ID
#error "不要在 Android.mk 写 -DSTARRY_AUTH_GAME_ID；只启用一个 STARRY_GAME_*，id 由 GameConfig.h 映射"
#endif

#if defined(STARRY_GAME_SELF)
#define STARRY_AUTH_GAME_ID "starry_self"
#elif defined(STARRY_GAME_CWJH)
#define STARRY_AUTH_GAME_ID "cwjh"
#elif defined(STARRY_GAME_ZXR)
#define STARRY_AUTH_GAME_ID "zxr"
#elif defined(STARRY_GAME_ZMXY)
#define STARRY_AUTH_GAME_ID "zmxy"
#elif defined(STARRY_GAME_ZMWS)
#define STARRY_AUTH_GAME_ID "zmws"
#elif defined(STARRY_GAME_LTZJ)
#define STARRY_AUTH_GAME_ID "ltzj"
#elif defined(STARRY_GAME_NEON)
#define STARRY_AUTH_GAME_ID "neon"
#elif defined(STARRY_GAME_CSTJJS)
#define STARRY_AUTH_GAME_ID "cstjjs"
#elif defined(STARRY_GAME_ZJMC)
#define STARRY_AUTH_GAME_ID "zjmc"
#elif defined(STARRY_GAME_HZSJ)
#define STARRY_AUTH_GAME_ID "hzsj"
#elif defined(STARRY_GAME_MILTHM)
#define STARRY_AUTH_GAME_ID "milthm"
#elif defined(STARRY_GAME_YWGN)
#define STARRY_AUTH_GAME_ID "ywgn"
#elif defined(STARRY_GAME_WZYM)
#define STARRY_AUTH_GAME_ID "wzym"
#elif defined(STARRY_GAME_MATCH2)
#define STARRY_AUTH_GAME_ID "xxh2"
#elif defined(STARRY_GAME_PCR)
#define STARRY_AUTH_GAME_ID "pcr"
#elif defined(STARRY_GAME_GYWS)
#define STARRY_AUTH_GAME_ID "gyws"
#elif defined(STARRY_GAME_ZHMR)
#define STARRY_AUTH_GAME_ID "zhmr"
#elif defined(STARRY_GAME_DWCT)
#define STARRY_AUTH_GAME_ID "dwct"
#elif defined(STARRY_GAME_GJC2)
#define STARRY_AUTH_GAME_ID "gjc2"
#elif defined(STARRY_GAME_GJC2INTL)
#define STARRY_AUTH_GAME_ID "gjc2intl"
#elif defined(STARRY_GAME_TLGD)
#define STARRY_AUTH_GAME_ID "tlgd"
#elif defined(STARRY_GAME_SHLT)
#define STARRY_AUTH_GAME_ID "shlt"
#elif defined(STARRY_GAME_DZQS)
#define STARRY_AUTH_GAME_ID "dzqs"
#elif defined(STARRY_GAME_SHHJ)
#define STARRY_AUTH_GAME_ID "shhj"
#elif defined(STARRY_GAME_SC33)
#define STARRY_AUTH_GAME_ID "sc33"
#elif defined(STARRY_GAME_HALY)
#define STARRY_AUTH_GAME_ID "haly"
#elif defined(STARRY_GAME_ZMXS)
#define STARRY_AUTH_GAME_ID "zmxs"
#elif defined(STARRY_GAME_WGGLS)
#define STARRY_AUTH_GAME_ID "wggls"
#elif defined(STARRY_GAME_WHRHX4399)
#define STARRY_AUTH_GAME_ID "whhx4399"
#elif defined(STARRY_GAME_WHRHX)
#define STARRY_AUTH_GAME_ID "whhx"
#elif defined(STARRY_GAME_WHRHXKB)
#define STARRY_AUTH_GAME_ID "whhxkb"
#elif defined(STARRY_GAME_WGDZZ)
#define STARRY_AUTH_GAME_ID "wgdzz"
#elif defined(STARRY_GAME_BJDW)
#define STARRY_AUTH_GAME_ID "bjdw"
#elif defined(STARRY_GAME_QLFH)
#define STARRY_AUTH_GAME_ID "qlfh"
#elif defined(STARRY_GAME_ENDFIELD)
#define STARRY_AUTH_GAME_ID "mrfzzmd"
#elif defined(STARRY_GAME_WDYZ)
#define STARRY_AUTH_GAME_ID "wdyz"
#elif defined(STARRY_GAME_WXSJ)
#define STARRY_AUTH_GAME_ID "wxsj"
#elif defined(STARRY_GAME_PHIGROS)
#define STARRY_AUTH_GAME_ID "phigros"
#elif defined(STARRY_GAME_YQZD)
#define STARRY_AUTH_GAME_ID "yqzd"
#elif defined(STARRY_GAME_ES2)
#define STARRY_AUTH_GAME_ID "oxmhj2"
#else
#error "Android.mk 须启用且仅启用一个 STARRY_GAME_*（见 SWITCH_GAME.md）"
#endif

// ============================================================
//  ① Unity (il2cpp) — 物质与魔法 / 消消英雄2 / 战魂铭人
// ============================================================
#ifdef ENABLE_UNITY_HOOK
namespace GameUnity {
    static const char* TARGET_LIB = sec_lib_il2cpp();
#if defined(STARRY_GAME_NEON)
    constexpr const char* PACKAGE    = "com.shangyoo.neon";
    constexpr const char* GAME_NAME  = "霓虹深渊";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_CSTJJS)
    constexpr const char* PACKAGE    = "com.netkuu.changsheng.tjjs";
    constexpr const char* GAME_NAME  = "长生天机降世";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_ZJMC)
    constexpr const char* PACKAGE    = "com.TeaMelo.DungeonChasers";
    constexpr const char* GAME_NAME  = "追迹迷城";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_HZSJ)
    constexpr const char* PACKAGE    = "com.chillyroom.containerofsoul.yw";
    constexpr const char* GAME_NAME  = "魂坠深境";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 8;
#elif defined(STARRY_GAME_MILTHM)
    constexpr const char* PACKAGE    = "game.taptap.morizero.milthm";
    constexpr const char* GAME_NAME  = "milthm";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_YWGN)
    constexpr const char* PACKAGE    = "com.uniugame.ywgn";
    constexpr const char* GAME_NAME  = "音舞光年";
    // HybridCLR 热更：优先 offset+0，失败再 +8（见 YwgnMod）
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 8;
#elif defined(STARRY_GAME_WZYM)
    constexpr const char* PACKAGE    = "com.huanshits.materialMagic";
    constexpr const char* GAME_NAME  = "物质与魔法";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_MATCH2)
    constexpr const char* PACKAGE    = "com.blina.match2.tt";
    constexpr const char* GAME_NAME  = "消消英雄2";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_WGGLS)
    constexpr const char* PACKAGE    = "com.sixag.gaga";
    constexpr const char* GAME_NAME  = "我嘎嘎乱杀";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_WHRHX4399)
    constexpr const char* PACKAGE    = "com.yhhy.whrhx.m4399";
    constexpr const char* GAME_NAME  = "无悔华夏4399";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_WHRHX)
    constexpr const char* PACKAGE    = "com.yhhy.whrhx.aligames";
    constexpr const char* GAME_NAME  = "无悔华夏";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_WHRHXKB)
    constexpr const char* PACKAGE    = "com.syyx.whrhx";
    constexpr const char* GAME_NAME  = "无悔华夏快爆";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_WGDZZ)
    constexpr const char* PACKAGE    = "com.xr.wgdzztap";
    constexpr const char* GAME_NAME  = "王国大作战前线";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 8;
#elif defined(STARRY_GAME_BJDW)
    constexpr const char* PACKAGE    = "com.hnyg.bjdwxcf.topon";
    constexpr const char* GAME_NAME  = "伯吉的温馨厨房";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_ENDFIELD)
    constexpr const char* PACKAGE    = "com.hypergryph.endfield";
    constexpr const char* GAME_NAME  = "明日方舟：终末地";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_WDYZ)
    constexpr const char* PACKAGE    = "com.rsg.wdyz";
    constexpr const char* GAME_NAME  = "我的勇者";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_WXSJ)
    constexpr const char* PACKAGE    = "com.xingzuodaoshi.infinityUpgrade";
    constexpr const char* GAME_NAME  = "无限升级";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_PCR)
    constexpr const char* PACKAGE    = "com.bilibili.priconne";
    constexpr const char* GAME_NAME  = "公主连结";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_GYWS)
    constexpr const char* PACKAGE    = "com.fattoy.gyws.android";
    constexpr const char* GAME_NAME  = "孤勇无双";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_GJC2)
    constexpr const char* PACKAGE    = "com.xq.archeroii.ua";
    constexpr const char* GAME_NAME  = "弓箭传说2";
    // HybridCLR 热更：字段偏移优先 runtime FieldOffset
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_GJC2INTL)
    constexpr const char* PACKAGE    = "com.facai.archeroii";
    constexpr const char* GAME_NAME  = "弓箭传说2国际版";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_TLGD)
    constexpr const char* PACKAGE    = "com.ginicat.sweet.m4399";
    constexpr const char* GAME_NAME  = "弹力果冻";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_DZQS)
    constexpr const char* PACKAGE    = "com.Shimmer.SoloKnight.cn";
    constexpr const char* GAME_NAME  = "独奏骑士";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_SHLT)
    constexpr const char* PACKAGE    = "com.itaotuo.shanhelvtan";
    constexpr const char* GAME_NAME  = "山河旅探";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_ZMXS)
    constexpr const char* PACKAGE    = "com.yofijoy.zmxs";
    constexpr const char* GAME_NAME  = "掌门下山";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_ZHMR)
    constexpr const char* PACKAGE    = "com.chillyroom.zhmr.yw";
    constexpr const char* GAME_NAME  = "战魂铭人";
    // 3.5.0：HybridCLR 热更类偏移用 il2cpp_field_get_offset 原值（旧「+8」已否定，
    // 见 skill/re-extract/hybridclr-runtime.md；ZhmrMod 运行时仍做双方案实证兜底）
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;

    constexpr const char* CLASS_CHARACTER       = "Character";
    constexpr const char* CLASS_BODY            = "Body";
    constexpr const char* CLASS_HERO            = "Hero";
    constexpr const char* CLASS_LEVEL_PROCESS   = "LevelProcess";
    constexpr const char* CLASS_HERO_ARCHIVE    = "HeroArchive";
    constexpr const char* CLASS_PLAYER_ARCHIVE  = "PlayerArchive";
    constexpr const char* CLASS_ARCHIVE         = "Archive";
    constexpr const char* CLASS_ENCRYPT_VALUE   = "EncryptValue";

    namespace Fields {
        constexpr const char* HERO_BODY      = "_body";
        constexpr const char* BODY_CHARACTER = "<character>k__BackingField";
        constexpr const char* CHAR_BLOCK     = "_isBlockDamage";
        constexpr const char* CHAR_HP        = "_currentHealth";
        constexpr const char* CHAR_SHIELD    = "_currentShield";
        constexpr const char* LP_ALL_MONSTER = "<AllMonster>k__BackingField";
        constexpr const char* EV_SHOW        = "_showValue";
        constexpr const char* EV_M           = "mValue";
        constexpr const char* EV_KEY         = "encryptKey";
        constexpr const char* HA_NAME      = "myHeroBodyName";
        constexpr const char* HA_UNLOCK    = "_isUnlock";
        constexpr const char* HA_EXP       = "_experience";
        constexpr const char* HA_SKILL_DICT = "skillUnlockLibrary";
        constexpr const char* HA_STYLE_DICT = "styleUnlockLibrary";
        constexpr const char* ARCHIVE_HERO_DIC = "heroArchiveDic";
        constexpr const char* PA_RESOURCE_DIC  = "_resourceDic";
        constexpr const char* ARCHIVE_SKILL_LIB = "HeroBodySkillNameLibrary";
        constexpr const char* ARCHIVE_STYLE_LIB = "HeroSkillStyleNameLibrary";
        constexpr const char* DM_SKIN_CONFIG    = "HeroSkinConfigData";
    }
#elif defined(STARRY_GAME_PHIGROS)
    constexpr const char* PACKAGE    = "com.PigeonGames.Phigros";
    constexpr const char* GAME_NAME  = "Phigros";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#elif defined(STARRY_GAME_ES2)
    constexpr const char* PACKAGE    = "com.happyelements.es2";
    constexpr const char* GAME_NAME  = "偶像梦幻祭2";
    constexpr size_t HYBRIDCLR_FIELD_BIAS = 0;
#endif
}
#endif // ENABLE_UNITY_HOOK

// ============================================================
//  ② Cocos Creator — 堡垒之战（兼动物餐厅旧栈符号注释保留）
// ============================================================
#ifdef ENABLE_COCOS_HOOK
namespace GameCocos {
#if defined(STARRY_GAME_CWJH)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "com.grinning.beta.yszl";
    constexpr const char* GAME_NAME  = "超维进化";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        constexpr const char* EVAL_STRING =
            "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
        constexpr const char* SE_EVAL_STRING =
            "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
    }
#elif defined(STARRY_GAME_ZXR)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "com.zxr.dlxmz.zxrwz37zgx10.gg9b";
    constexpr const char* GAME_NAME  = "镇邪人";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        // 旧栈 org.cocos2dx.lib（仅 Java→JS；战斗 .jsc 不走这里）
        constexpr const char* EVAL_STRING =
            "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
        // 斌哥同款主路径：.jsc decrypt → runScript → se::ScriptEngine::evalString
        constexpr const char* SE_EVAL_STRING =
            "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
    }
#elif defined(STARRY_GAME_ZMXY)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "com.xm4399.zmxy6hj";
    constexpr const char* GAME_NAME  = "造梦西游";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        constexpr const char* EVAL_STRING =
            "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
        constexpr const char* SE_EVAL_STRING =
            "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
    }
#elif defined(STARRY_GAME_ZMWS)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "com.xm4399.zmws.m4399";
    constexpr const char* GAME_NAME  = "造梦无双";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        constexpr const char* EVAL_STRING =
            "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
        constexpr const char* SE_EVAL_STRING =
            "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
    }
#elif defined(STARRY_GAME_LTZJ)
    static const char* TARGET_LIB = sec_lib_cocos();
    constexpr const char* PACKAGE    = "com.a5game.ltzj.jj";
    constexpr const char* GAME_NAME  = "雷霆战机";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        constexpr const char* EVAL_STRING =
            "Java_com_cocos_lib_CocosJavascriptJavaBridge_evalString";
    }
#elif defined(STARRY_GAME_DWCT)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "inland.twgame.restaurant";
    constexpr const char* GAME_NAME  = "动物餐厅";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        constexpr const char* EVAL_STRING =
            "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
        constexpr const char* SE_EVAL_STRING =
            "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
    }
#elif defined(STARRY_GAME_SHHJ)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "com.chx.shhj";
    constexpr const char* GAME_NAME  = "山海幻境";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        constexpr const char* EVAL_STRING =
            "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
        constexpr const char* SE_EVAL_STRING =
            "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
    }
#elif defined(STARRY_GAME_SC33)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive";
    constexpr const char* GAME_NAME  = "生存33天";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        constexpr const char* EVAL_STRING =
            "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
        constexpr const char* SE_EVAL_STRING =
            "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
    }
#elif defined(STARRY_GAME_HALY)
    static const char* TARGET_LIB = sec_lib_cocos();
    constexpr const char* PACKAGE    = "com.haly.tap";
    constexpr const char* GAME_NAME  = "黑暗领域";    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        // Creator 3.8：com.cocos.lib.CocosJavascriptJavaBridge
        constexpr const char* EVAL_STRING =
            "Java_com_cocos_lib_CocosJavascriptJavaBridge_evalString";
    }
#elif defined(STARRY_GAME_QLFH)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "com.hdg4.g4qlfh";
    constexpr const char* GAME_NAME  = "青鸾繁华录";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        // Creator 2.x：明文 bundle 过 se::evalString
        constexpr const char* EVAL_STRING =
            "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
        constexpr const char* SE_EVAL_STRING =
            "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
    }
#elif defined(STARRY_GAME_YQZD)
    static const char* TARGET_LIB = sec_lib_cocosjs();
    constexpr const char* PACKAGE    = "com.hortor.yqzd";
    constexpr const char* GAME_NAME  = "遗弃之地";

    constexpr const char* MOD_FIRING_RANGE = "";
    constexpr const char* MOD_GAME_CONTEXT = "";
    constexpr const char* MOD_MGR_DEBUG    = "";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        // Creator 3.8.5：零 native hook，EvalOnGLThread 运行时 wrap（不 hook evalString）
        constexpr const char* EVAL_STRING = "";
        constexpr const char* SE_EVAL_STRING = "";
    }
#else
    static const char* TARGET_LIB = sec_lib_cocos();
    constexpr const char* PACKAGE    = "com.ultralisk.game304";
    constexpr const char* GAME_NAME  = "堡垒之战";

    // JS 虚拟模块（System.get chunks:///_virtual/*.ts）
    constexpr const char* MOD_FIRING_RANGE = "mgrFiringRange";
    constexpr const char* MOD_GAME_CONTEXT = "_GameContext";
    constexpr const char* MOD_MGR_DEBUG    = "mgrDebug";

    namespace Symbols {
        constexpr const char* PLAYER_SET_SCORE  = "";
        constexpr const char* PLAYER_ADD_GOLD   = "";
        constexpr const char* ENEMY_TAKE_DAMAGE = "";
        // Creator 3.8：Java_com_cocos_lib_CocosJavascriptJavaBridge_evalString
        constexpr const char* EVAL_STRING =
            "Java_com_cocos_lib_CocosJavascriptJavaBridge_evalString";
    }
#endif

    namespace Offsets {
        constexpr uintptr_t FUNC_SET_SCORE     = 0x0;
        constexpr uintptr_t FUNC_ADD_GOLD      = 0x0;
        constexpr uintptr_t FUNC_PLAYER_HURT   = 0x0;
        constexpr uintptr_t FUNC_ENEMY_DIE     = 0x0;
        constexpr uintptr_t GLOBAL_PLAYER_MGR  = 0x0;
        constexpr uintptr_t GLOBAL_GAME_MGR    = 0x0;
    }

    namespace Patterns {
        constexpr const char* SET_SCORE     = "";
        constexpr const char* ADD_GOLD      = "";
        constexpr const char* PLAYER_HURT   = "";
    }
}
#endif // ENABLE_COCOS_HOOK

// ============================================================
//  ③ Unreal Engine（预留）
// ============================================================
#ifdef ENABLE_UNREAL_HOOK
namespace GameUnreal {
    constexpr const char* TARGET_LIB = "libUE4.so";
    namespace Offsets {
        constexpr uintptr_t FUNC_PLAYER_HEALTH = 0x0;
    }
}
#endif

// ============================================================
//  ④ 通用 / 自研引擎
// ============================================================
#ifdef ENABLE_GENERIC_HOOK
namespace GameGeneric {
    constexpr const char* TARGET_LIB = "libgame.so";

    namespace Offsets {
        constexpr uintptr_t ADDR_GOLD      = 0x0;
        constexpr uintptr_t ADDR_HP        = 0x0;
        constexpr uintptr_t ADDR_SCORE     = 0x0;
        constexpr uintptr_t FUNC_PLAYER_HURT = 0x0;
        constexpr uintptr_t FUNC_ENEMY_DIE   = 0x0;
    }

    namespace Patterns {
        constexpr const char* PLAYER_HURT = "";
        constexpr const char* ENEMY_DIE   = "";
    }
}
#endif // ENABLE_GENERIC_HOOK

#endif // STARRY_GAME_CONFIG_H#include "Includes/SecStrings.hpp"
