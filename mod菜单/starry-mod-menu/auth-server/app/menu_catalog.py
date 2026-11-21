"""
内置菜单模板种子 —— 仅在数据库尚无该 template_id 时写入。

运行时后台读写数据库表 menu_templates / menu_template_features。
本文件只保留内置自测模板 `starry_self`（星光自测App 的 UI 控件全集）。
正式游戏一律在管理端「菜单模板」页新建/编辑，无需改本文件、无需重启。

客户端 AuthState.game_id / Preferences 默认必须与后台 games.game_id 一致。
"""

from __future__ import annotations

from typing import Any

# key = 模板 id（创建游戏时选择；也常直接用作推荐 game_id）
SEED_TEMPLATES: dict[str, dict[str, Any]] = {
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
}

# 兼容旧引用名
MENU_TEMPLATES = SEED_TEMPLATES
