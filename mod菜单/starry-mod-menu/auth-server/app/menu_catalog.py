"""
内置菜单模板种子 —— 仅在数据库尚无该 template_id 时写入。

运行时后台读写数据库表 menu_templates / menu_template_features。
日常加游戏：管理端「菜单模板」页新建/编辑即可，无需改本文件、无需重启后才能选到
（部署含本能力的新版后；进程热更仍需一次上传，但之后加模板不再重启）。

客户端 AuthState.game_id / Preferences 默认必须与后台 games.game_id 一致。
"""

from __future__ import annotations

from typing import Any

# key = 模板 id（创建游戏时选择；也常直接用作推荐 game_id）
SEED_TEMPLATES: dict[str, dict[str, Any]] = {
    "zhmr": {
        "name": "战魂铭人",
        "package": "com.chillyroom.zhmr.yw",
        "source": "app/src/main/jni/Main.cpp g_featureList + Games/ZhmrMod.h",
        "features": [
            {"feat_num": 1, "name": "无敌（主角免伤锁血）"},
            {"feat_num": 2, "name": "秒杀（敌人0.1血+破盾）"},
            {"feat_num": 4, "name": "无限货币"},
            {"feat_num": 5, "name": "无限道具（高封号操作）"},
            {"feat_num": 7, "name": "解锁全角色与技能"},
            {"feat_num": 8, "name": "解锁全皮肤"},
        ],
    },
    "dwct": {
        "name": "动物餐厅",
        "package": "inland.twgame.restaurant",
        "source": "Games/DwctMod.h DWCT_FEAT_*",
        "features": [
            {"feat_num": 1, "name": "免广告"},
            {"feat_num": 2, "name": "免内购"},
        ],
    },
    "blzz": {
        "name": "堡垒之战",
        "package": "com.ultralisk.game304",
        "source": "Games/BlzzMod.h BLZZ_FEAT_* / mgrDebug 客户端项",
        "features": [
            {"feat_num": 1, "name": "无敌"},
            {"feat_num": 2, "name": "十倍攻击"},
            {"feat_num": 3, "name": "蓄力无CD"},
            {"feat_num": 4, "name": "解锁所有功能"},
        ],
    },
    "xxh2": {
        "name": "消消英雄2",
        "package": "com.blina.match2.tt",
        "source": "Games/Match2Mod.h MATCH2_FEAT_* / 改包 AN_1/2/3",
        "features": [
            {"feat_num": 1, "name": "无敌（敌方伤害归零）"},
            {"feat_num": 2, "name": "十倍伤害（我方伤害×10）"},
            {"feat_num": 3, "name": "瞬间结束战斗（5倍速自动）"},
        ],
    },
    "wzym": {
        "name": "物质与魔法",
        "package": "com.huanshits.materialMagic",
        "source": "Games/WzymMod.h / StatusC UpPhysicalDamage+Update",
        "features": [
            {"feat_num": 1, "name": "无敌（wudi）"},
            {"feat_num": 2, "name": "伤害倍率（滑条）"},
            {"feat_num": 3, "name": "全屏秒杀"},
            {"feat_num": 4, "name": "移速攻速（滑条 1关~5快）"},
        ],
    },
    "zxr": {
        "name": "镇邪人",
        "package": "com.zxr.dlxmz.zxrwz37zgx10.gg9b",
        "source": "Games/ZxrMod.h FightChangeEntityHp+getAtk",
        "features": [
            {"feat_num": 1, "name": "秒杀"},
            {"feat_num": 2, "name": "无敌"},
            {"feat_num": 3, "name": "攻速"},
            {"feat_num": 4, "name": "全屏秒杀"},
            {"feat_num": 6, "name": "免广告"},
            {"feat_num": 8, "name": "游戏加速（滑条 1关~5快）"},
        ],
    },
    "sc33": {
        "name": "生存33天",
        "package": "com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive",
        "source": "已归档 APK；feat 见 10_docs/生存33天/",
        "features": [
            {"feat_num": 1, "name": "百倍伤害"},
            {"feat_num": 2, "name": "无敌"},
            {"feat_num": 3, "name": "全屏秒杀"},
            {"feat_num": 4, "name": "攻速", "type": "slider", "min": 1, "max": 10},
            {"feat_num": 6, "name": "免广告"},
        ],
    },
    "zmxy": {
        "name": "造梦西游",
        "package": "com.xm4399.zmxy6hj",
        "source": "Games/ZmxyMod.h AppActivity.get_ret 1..3 evalString",
        "features": [
            {"feat_num": 1, "name": "百倍伤害"},
            {"feat_num": 2, "name": "无敌"},
            {"feat_num": 3, "name": "无限技能（无CD+不耗蓝）"},
        ],
    },
    "ltzj": {
        "name": "雷霆战机",
        "package": "com.a5game.ltzj.jj",
        "source": "Games/LtzjMod.h AppActivity.get_ret 1..3 + TopOn",
        "features": [
            {"feat_num": 1, "name": "百倍伤害"},
            {"feat_num": 2, "name": "无敌"},
            {"feat_num": 3, "name": "免广告"},
        ],
    },
    "mwdl": {
        "name": "迷雾大陆",
        "package": "com.hortor.mwdl",
        "source": "Games/MwdlMod.h AppActivity.get_ret 1..4 evalString splice",
        "features": [
            {"feat_num": 1, "name": "百倍伤害"},
            {"feat_num": 2, "name": "无敌"},
            {"feat_num": 3, "name": "移动速度（滑条1-10）"},
            {"feat_num": 4, "name": "无限技能"},
        ],
    },
    "milthm": {
        "name": "milthm",
        "package": "game.taptap.morizero.milthm",
        "source": "Games/MilthmMod.h AddJudge+Erode→Exact",
        "features": [
            {"feat_num": 1, "name": "自动跳舞"},
        ],
    },
    "neon": {
        "name": "霓虹深渊",
        "package": "com.shangyoo.neon",
        "source": "Games/NeonMod.cpp 仅战斗",
        "features": [
            {"feat_num": 1, "name": "无敌"},
            {"feat_num": 2, "name": "秒杀"},
            {"feat_num": 3, "name": "技能无CD"},
            {"feat_num": 4, "name": "移速"},
        ],
    },
    "starry_self": {
        "name": "星光自测",
        "package": None,
        "source": "Main.cpp STARRY_GAME_SELF（UI 控件全集，无真实作弊）",
        "features": [
            {"feat_num": 1, "name": "测试开关"},
            {"feat_num": 2, "name": "测试勾选"},
            {"feat_num": 4, "name": "测试滑块"},
            {"feat_num": 5, "name": "测试滑轮"},
            {"feat_num": 7, "name": "难度"},
            {"feat_num": 9, "name": "备注"},
            {"feat_num": 10, "name": "数量"},
            {"feat_num": 12, "name": "测试执行"},
        ],
    },
    "shhj": {
        "name": "山海幻境",
        "package": "com.chx.shhj",
        "source": "Games/ShhjMod.h SHHJ_FEAT_* / Main.cpp STARRY_GAME_SHHJ",
        "features": [
            {"feat_num": 1, "name": "无敌"},
            {"feat_num": 2, "name": "百倍伤害"},
        ],
    },
    "haly": {
        "name": "黑暗领域",
        "package": "com.haly.tap",
        "source": "Games/HalyMod.h HALY_FEAT_* / Main.cpp STARRY_GAME_HALY",
        "features": [
            {"feat_num": 1, "name": "免广告"},
            {"feat_num": 2, "name": "不羁怒火(自动无敌)"},
            {"feat_num": 3, "name": "先祖之魂(自动召唤)"},
            {"feat_num": 4, "name": "雷罚打击(普攻触发)"},
        ],
    },
    "wggls": {
        "name": "我嘎嘎乱杀",
        "package": "com.sixag.gaga",
        "source": "Games/WgglsMod.h WGGLS_FEAT_* / Main.cpp STARRY_GAME_WGGLS",
        "features": [
            {"feat_num": 1, "name": "百倍伤害"},
            {"feat_num": 2, "name": "超级无敌"},
            {"feat_num": 3, "name": "战斗加速", "type": "slider", "min": 1, "max": 5},
            {"feat_num": 4, "name": "免广告"},
        ],
    },
}

# 兼容旧引用名
MENU_TEMPLATES = SEED_TEMPLATES
