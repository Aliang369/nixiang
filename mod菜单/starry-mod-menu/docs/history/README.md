# 历史文档（**勿按此开发**）

> 2026-09-22 起菜单 UI 已从 WebView + 单文件 HTML 改为全原生实现（`menu/NativeMenuController` + `MagicRes`）。
> 本目录三份文档描述的是**已删除**的 HTML 方案，仅作追溯用，链接/行号一律按当时版本。

| 文件 | 当时内容 | 失效原因 |
|---|---|---|
| `ARCHITECTURE.md` | 三层架构总览、JS Bridge 接口规范、`PassThroughWebView` 专节 | HTML/WebView 菜单 2026-08-13 删除，JS 桥方法 2026-11-23 清理 |
| `功能说明文档.md` | `mod-menu-v2.html` 的页面结构 / Header / TabBar / 公告 / Toast 说明 | 同上 |
| `design-spec.md` | 单文件 HTML 设计规范、页面目录树（`mod-menu.html`/`mod-menu-v2.html`/…） | 同上 |

**现行文档**：

- 原生菜单实现：`../原生菜单_星光魔法阵_20260813.md`
- 移植/接新游戏：`../GAME_MOD_GUIDE.md`
- 视觉规范：根目录 `../../设计风格美术规范.md`
- 工程总览与目录：根目录 `../../README.md`
