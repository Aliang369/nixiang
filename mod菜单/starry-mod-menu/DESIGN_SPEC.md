# 设计规范（索引）

> **版本**：2.0 · 2026-08-08  
> **权威全文**：请以 [设计风格美术规范.md](./设计风格美术规范.md) 为准。

## 主题

**星光魔法** — 暗色星空 + 薰衣草紫 + 毛玻璃 + 魔法阵 / 星星。

## 速查色板

| Token | 色值 |
|------|------|
| `--bg` / `--bg-deep` | `#1A1B3A` / `#0E0F23` |
| `--brand` / `--brand-bright` | `#9B7ED8` / `#B8A0F0` |
| `--ink` / `--ink-2` / `--ink-3` | `#E8E8F5` / `#B8B8D4` / `#7A7A9E` |
| 语义 | 成功 `#7EE0A0` · 警告 `#FFE5B4` · 错误 `#E07E7E` · 信息 `#7EB8E0` |

## 两套表面

| 表面 | 文件 | 要点 |
|------|------|------|
| 游戏菜单 | `app/src/main/java/com/starry/magic/menu/NativeMenuController.java` | 全原生 View 自绘（2026-09-22 星光紫升级：#8B5CF6 系 + 夜空渐变星尘面板 + 手风琴分类） |
| 运营后台 | `auth-server/static/admin/index.html` | 全屏星空、毛玻璃侧栏/面板、控件高 **40 / 32px** |

## 后台专用补充

见 [auth-server/docs/ADMIN_UI_SPEC.md](./auth-server/docs/ADMIN_UI_SPEC.md)。

## 菜单组件尺寸（仍有效）

| 项目 | 值 |
|------|-----|
| 设计基准宽 | 375px |
| 面板圆角 | 16px |
| 开关 | 约 42×23，旋钮 ~17 |
| 魔法阵装饰 | ~280px，opacity ~0.12 |
| 面板内星 | 14 颗错落闪烁 |

## 动效

- 缓动：`cubic-bezier(0.2, 0.8, 0.2, 1)`  
- 必须支持 `prefers-reduced-motion`  

## 技术

- 菜单：单文件 HTML，中文 `lang="zh-CN"`  
- 图标：Lucide / 内联 SVG，不用 emoji 当图标  
- 品牌文案：一律「星光」
