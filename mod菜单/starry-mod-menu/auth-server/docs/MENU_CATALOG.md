# 菜单模板与功能联动

## 你要的效果

以后日常只做两件事：

1. **后台「菜单模板」**新建/编辑功能清单（或创建游戏时选已有模板）
2. **功能页开关**（开/关某功能是否可用）

不用再手填 `feat_num`，也**不必为加模板而改代码/重启服务**。

## 重要澄清：后台不会「实时读客户端」

| 层 | 位置 | 作用 |
|----|------|------|
| 客户端 `game_id`（唯一） | `Android.mk` 启用一个 `STARRY_GAME_*` → `GameConfig.h` 映射 → `nativeGetAuthGameId` → prefs | 登录/bootstrap/grant 上报的 id |
| 客户端菜单功能 | `Main.cpp` `g_featureList`（同 mk 的 `STARRY_GAME_*`） | feat 编号真相 |
| 服务端模板 | 数据库 `menu_templates` / `menu_template_features` | 后台选模板、同步门控 |
| 种子（仅缺省写入） | `auth-server/app/menu_catalog.py` `SEED_TEMPLATES` | 首次部署补种，**不覆盖**库里已有模板 |
| 数据库门控 | `feature_flags` | 实际下发给客户端的开关 |

后台「选模板自动出功能列表」= 读 **数据库模板**，不是连手机读 APK。

**必须一致**：`GameConfig.h` 映射出的 `game_id` ↔ 后台 `games.game_id` ↔ 卡密所属游戏。  
**切游戏只改** `Android.mk` 的 `STARRY_GAME_*`（见 `SWITCH_GAME.md`）。

## 日常加新游戏（推荐）

1. 改客户端 `Main.cpp` / `*Mod`（菜单真有功能）
2. 后台 → **菜单模板** → 新建（或编辑）`template_id` + `编号\|名称` 列表
3. 后台 → **游戏列表** → 添加游戏并选该模板（或功能页点「同步菜单」）
4. 功能页开关门控

同步模式：`fill` 只补缺失；`replace` 补缺失并更新名称；`prune` 同 replace 并**删除模板外多余功能**。三种模式都**保留已有 `enabled`**。换错模板后用功能页「同步并清理多余」，或单条「删除」。

## AI / 人工改菜单

1. 改 `Main.cpp`（以及对应 `*Mod.h`）
2. 后台 **菜单模板 → JSON 导入**，粘贴如下格式（单条或数组）；已存在 `template_id` 则更新：

```json
{
  "template_id": "milthm",
  "name": "milthm",
  "package": "game.taptap.morizero.milthm",
  "source": "Games/MilthmMod.cpp",
  "features": [
    {"feat_num": 1, "name": "自动跳舞"}
  ]
}
```

3. 游戏列表里该游戏点「同步菜单」

**禁止**为日常加游戏去改 `menu_catalog.py` / 要求重启服务。  
该文件仅「全新库 / 点补种内置」缺省写入；线上已有模板后**以数据库为准**。

## 推荐 game_id（2026-08-27 起）

**新游戏**：官方中文**全名**的拼音首字母（小写、无分隔符），虚词与数字均计入。例：伯吉的温馨厨房 → `bjdwxcf`。

**存量**（`bjdw`、`zhmr`、`neon` 等）已绑定卡密/so，**不重命名**。

规则全文：`.cursor/rules/starry-mod-game-id.mdc`

## API

- `GET /v1/admin/menu-templates`
- `POST /v1/admin/menu-templates`
- `PATCH /v1/admin/menu-templates/{template_id}`
- `DELETE /v1/admin/menu-templates/{template_id}?force=false`
- `POST /v1/admin/menu-templates/seed-builtins`（只补缺失）
- `POST /v1/admin/games`（可带 `menu_template`，默认自动 sync）
- `POST /v1/admin/games/{game_id}/sync-features`
