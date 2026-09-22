# 星光魔法菜单 - 设计规范

> ⚠️ **历史设计稿档案（2026-09-22 标注）**：其中的 `mod-menu-v2.html` 单文件菜单方案已废弃，现行菜单为全原生实现（`NativeMenuController`）。
> 设计 token 与组件尺寸的现行权威见 [设计风格美术规范.md](./设计风格美术规范.md) 与 `auth-server/docs/ADMIN_UI_SPEC.md`。

> **已合并**：完整权威规范请阅读仓库根目录 [设计风格美术规范.md](./设计风格美术规范.md)（v2.0）。  
> 本文保留历史菜单页清单，仅作索引。

> Stardust / Starlight Magic MOD Menu — 版本索引 2.0 | 2026-08-08

---

## 1. 项目概述

「星光魔法菜单」是面向 Android 游戏 MOD 的移动端 UI，星空魔法主题；运营后台共用同一套色板与符号系统。

### 页面清单

| 页面 | 文件 | 说明 |
|------|------|------|
| 星光魔法菜单 - 统一版 | `app/src/main/assets/web/pages/mod-menu-v2.html` | 主菜单 |
| 运营后台 | `auth-server/static/admin/index.html` | 验证运营台 |
| 设计规范（权威） | `设计风格美术规范.md` | 色彩 / 字体 / 控件 / 动效 |
| 后台 UI 摘要 | `auth-server/docs/ADMIN_UI_SPEC.md` | 后台专用 |

其余章节（旧色板、旧组件尺寸）若与 v2.0 冲突，**以 `设计风格美术规范.md` 为准**。


---

## 2. 色彩系统

### 2.1 核心色板

| 变量 | 色值 | 用途 |
|------|------|------|
| `--bg` | `#1A1B3A` | 星空蓝，页面主背景 |
| `--surface` | `#252650` | 面板表面色 |
| `--surface-2` | `#2D2E5F` | 次级表面 / 输入框背景 |
| `--ink` | `#E8E8F5` | 银白色，主文字 |
| `--ink-2` | `#B8B8D4` | 次级文字 |
| `--ink-3` | `#7A7A9E` | 弱化文字 / 图标默认色 |
| `--line` | `rgba(155, 126, 216, 0.15)` | 分割线 / 边框 |
| `--brand` | `#9B7ED8` | 薰衣草紫，品牌主色 |
| `--brand-glow` | `rgba(155, 126, 216, 0.4)` | 品牌色辉光 |

### 2.2 语义状态色

| 变量 | 色值 | 语义 |
|------|------|------|
| `--state-success` | `#7EE0A0` | 成功 / 在线 / 活动通知 |
| `--state-warning` | `#FFE5B4` | 警告 / 维护通知 |
| `--state-error` | `#E07E7E` | 错误 / 重要消息 |
| `--state-info` | `#7EB8E0` | 信息 / 更新公告 |

### 2.3 使用规则

- 品牌色 `--brand` 用于：激活状态、强调文字、图标高亮、交互反馈
- 语义色仅用于状态指示，不作为装饰色
- 面板背景统一使用 `rgba(37, 38, 80, 0.85)` + `backdrop-filter: blur(20px)` 实现毛玻璃效果
- 透明度最低限制 0.2，保证可用性

---

## 3. 字体系统

### 3.1 字体家族

| 变量 | 字体栈 | 用途 |
|------|--------|------|
| `--font-display` | `"Fredoka", "Noto Sans SC", "PingFang SC", system-ui` | 标题、分类名、展示文字 |
| `--font-body` | `"Inter", "Noto Sans SC", "PingFang SC", system-ui` | 正文、选项标签、按钮 |
| `--font-mono` | `"JetBrains Mono", "Courier New", monospace` | 数值显示、计数器、时间戳 |

### 3.2 字号层级

| 场景 | 字号 | 字重 | 字体 |
|------|------|------|------|
| 面板标题 | 17px | 600 | Fredoka |
| 分类标题 | 13px | 600 | Fredoka |
| 选项标签 | 13px | 400 | Inter |
| 数值显示 | 12px | 500 | JetBrains Mono |
| 标签栏文字 | 10px | 400/600 | Inter |
| 计数徽章 | 10px | 600 | JetBrains Mono |

---

## 4. 间距与圆角

### 4.1 间距体系

| 变量 | 值 | 典型用途 |
|------|----|----------|
| `--s-1` | 4px | 微间距 |
| `--s-2` | 8px | 组件内间距 / 图标与文字间距 |
| `--s-3` | 12px | 内容区内边距 |
| `--s-4` | 16px | 页面外边距 |
| `--s-5` | 24px | 区块间距 |
| `--s-6` | 32px | 大区块间距 |

### 4.2 圆角体系

| 变量 | 值 | 用途 |
|------|----|------|
| `--r-xs` | 2px | 极小元素 |
| `--r-sm` | 4px | 按钮、输入框 |
| `--r-md` | 8px | 卡片、选项行 |
| `--r-lg` | 16px | 面板、大卡片 |
| `--r-pill` | 999px | 开关、徽章、指示灯 |

---

## 5. 组件规范

### 5.1 面板 (Panel)

```
背景: rgba(37, 38, 80, 0.85)
毛玻璃: backdrop-filter: blur(20px)
边框: 1px solid var(--line)
圆角: 16px
阴影: 0 24px 60px -20px rgba(10, 10, 30, 0.6)
最大宽度: 340px (菜单面板)
```

面板透明度由设置页滑块控制，alpha 范围 0.2 ~ 1.0。

### 5.2 开关 (Toggle)

```
尺寸: 42px × 23px
关闭态: background var(--surface-2), border var(--line)
开启态: background var(--brand), border var(--brand), box-shadow 0 0 12px var(--brand-glow)
旋钮: 17px × 17px, 圆形, 开启时左移至 21px
动画: 280ms ease-state 过渡 + 弹簧回弹 + 开启脉冲发光
```

### 5.3 滑块 (Slider)

```
轨道高度: 4px, 圆角 pill, 背景 var(--surface-2)
填充: 背景 var(--brand), 宽度随值变化
滑块: 14px × 14px, 圆形, 背景 var(--brand), 阴影发光
支持鼠标拖拽 + 触摸拖拽
```

### 5.4 分类组 (Category Group)

```
头部: 背景 rgba(155,126,216,0.08), 1px solid var(--line), 圆角 8px
头部 hover: 背景 rgba(155,126,216,0.14)
头部 active: 背景 rgba(155,126,216,0.18)
标题: Fredoka 13px 600
计数徽章: JetBrains Mono 10px, 背景半透明品牌色, pill 圆角
箭头: 14px, 开启时 rotate(180deg), 弹簧动画
展开: max-height 500px → 0, opacity 1 → 0, 400ms 过渡
默认状态: 1个分类自动展开, 2个及以上默认收缩
```

### 5.5 下拉选择器 (Dropdown)

```
触发器: 按钮样式, var(--surface-2) 背景
菜单: 绝对定位, var(--surface-2) 背景, 阴影 shadow-2
选中项: 品牌色高亮
动画: 展开/收起 280ms
```

### 5.6 执行按钮 (Exec Button)

```
背景: var(--brand)
文字: var(--brand-ink) / 白色
圆角: var(--r-sm)
点击: ripple 涟漪 + executing 闪烁动画
```

### 5.7 公告卡片 (Announcement Card)

```
背景: rgba(37, 38, 80, 0.85)
毛玻璃: backdrop-filter: blur(20px)
圆角: 16px
左侧类型条: 3px 宽, 对应语义色, 带发光
hover/active: 上浮 + 品牌色发光边框
点击: ripple 涟漪效果
加载: stagger 淡入, 每张延迟 100ms
```

### 5.8 悬浮球 (Floating Ball)

```
容器: #floating-ball
内部球: .fw-ball-inner (图标 + 背景)
外部装饰: .fw-mini-circle (SVG 魔法圆环)
透明度联动: 值为 0 时容器整体隐藏, 其他值仅改变内部球透明度
大小范围: 36px ~ 84px (由设置滑块控制)
```

---

## 6. 动效规范

### 6.1 缓动函数

| 变量 | 值 | 用途 |
|------|----|------|
| `--ease-out` | `cubic-bezier(0.2, 0.8, 0.2, 1)` | 通用过渡 |
| `--ease-spring` | `cubic-bezier(0.34, 1.56, 0.64, 1)` | 弹簧回弹 |
| `--ease-state` | `cubic-bezier(0.3, 0, 0, 1)` | 状态切换 |

### 6.2 时长体系

| 变量 | 值 | 用途 |
|------|----|------|
| `--dur-hover` | 140ms | hover 反馈 |
| `--dur-state` | 280ms | 状态切换 |
| `--dur-block` | 400ms | 区块展开/收起 |

### 6.3 装饰动画

| 动画 | 时长 | 说明 |
|------|------|------|
| 星星闪烁 (ps-twinkle) | 2.2s ~ 3.8s | 面板内星星 opacity + scale 循环 |
| 星空层 (twinkle-1/2/3) | 3s ~ 6s | 页面背景三层星空 opacity 循环 |
| 魔法阵旋转 (rotate-circle) | 60s | 面板内魔法阵缓慢旋转 |
| 角落装饰浮动 (corner-float) | 8s | 四角装饰上下微浮 |
| 开关脉冲 (toggle-pulse) | 0.4s | 开关开启时发光脉冲 |
| 旋钮回弹 (knob-bounce) | 0.28s | 开关旋钮弹簧形变 |

### 6.4 减少动效

```css
@media (prefers-reduced-motion: reduce) {
  * { transition-duration: 0.01ms !important; animation-duration: 0.01ms !important; }
}
```

---

## 7. 装饰元素

### 7.1 魔法阵 (Magic Circle)

- SVG 矢量绘制，viewBox 200x200
- 多层同心圆 + 六芒星 + 对角线 + 节点圆点
- 带发光滤镜 (feGaussianBlur + feMerge)
- 旋转动画 60s 线性循环
- 透明度 0.38，由「显示魔法阵」开关控制显隐

### 7.2 角落装饰 (Corner Decoration)

- 四角各一个 SVG (viewBox 120x120)
- 弧线 + 虚线弧 + 圆点 + 多边形组合
- 上下浮动动画 8s，各角延迟 0/2/4/6s
- 由「显示魔法阵」开关统一控制显隐

### 7.3 闪烁星星 (Panel Stars)

- 面板内 14 颗五角星 (SVG background)
- 绝对定位散布
- twinkle 动画 (opacity 0.25→1, scale 0.8→1.15, drop-shadow 发光)
- 由「背景星云」开关控制显隐，由「星星闪烁」开关控制动画

### 7.4 星空层 (Star Layers)

- 页面背景三层 radial-gradient 星空
- 各层不同密度、颜色、闪烁速度
- 纯装饰，不受菜单设置控制

---

## 8. 设置项规范

### 8.1 设置项清单

| 设置项 | 类型 | 控制范围 | 说明 |
|--------|------|----------|------|
| 面板透明度 | 滑块 | 20% ~ 100% | 控制面板背景 alpha |
| 悬浮球透明度 | 滑块 | 0% ~ 100% | 0% 时悬浮球+装饰全部隐藏 |
| 悬浮球大小 | 滑块 | 36px ~ 84px | 控制悬浮球及装饰尺寸 |
| 显示魔法阵 | 开关 | 显/隐 | 控制魔法阵 + 四角装饰 |
| 星星闪烁 | 开关 | 动画开/关 | 控制面板星星显隐和动画 |
| 恢复默认 | 按钮 | - | 重置所有设置 |

### 8.2 作用域规则

- 所有设置仅影响菜单面板内部，不修改网页背景
- 悬浮球透明度为 0 时：容器整体隐藏（含外部装饰圆环）
- 悬浮球透明度 > 0 时：仅改变内部球透明度，外部装饰不受影响

---

## 9. 图标系统

- 图标库: Lucide Icons (v1.8.0)
- 引入方式: `<script src="https://unpkg.com/lucide@1.8.0/dist/umd/lucide.min.js"></script>`
- 调用方式: `<i data-lucide="icon-name"></i>` + `lucide.createIcons()`
- 默认尺寸: 16px (选项图标) / 15px (分类图标) / 14px (箭头)
- 品牌色图标: 添加 class `is-brand`，color 设为 `var(--brand)`

### 常用图标映射

| 场景 | 图标名 |
|------|--------|
| 功能菜单 | `wand-2` |
| 设置 | `settings` |
| 账号 | `user` |
| 关闭 | `x` |
| 展开/收起 | `chevron-down` |
| 执行 | `play` |
| 星光 | `sparkles` |

---

## 10. 布局规范

### 10.1 菜单面板布局

```
┌─────────────────────────┐
│      Header (42px)       │  标题居中 + 关闭按钮
├─────────────────────────┤
│                         │
│    Tab Content Area     │  可滚动
│    (flex: 1)            │
│                         │
├─────────────────────────┤
│   Tab Bar (底部标签栏)   │  功能 / 设置 / 账号
└─────────────────────────┘
面板高度: 310px (菜单) / 可变 (公告页)
最大宽度: 340px (菜单)
```

### 10.2 功能菜单分类结构

```
分类组 (可展开/收起)
├── 分类头部 (图标 + 标题 + 计数 + 箭头)
└── 分类内容
    ├── 选项行 (图标 + 标签 + 控件)
    ├── 选项行
    └── ...
```

### 10.3 响应式

- 基准宽度: 375px (iPhone)
- 小屏适配: `@media (max-width: 320px)` 缩小滑块/输入框/标题
- 面板居中显示，页面背景填充

---

## 11. 文件结构

```
starry-magic-mod-menu/
├── starry-magic-mod-menu.design   # 画布项目文件
├── design-spec.md                 # 本设计规范文档
├── pages/
│   ├── mod-menu.html              # 初版菜单
│   ├── mod-menu-v2.html           # 统一版菜单（主版本）
│   ├── floating-widget.html       # 悬浮球设计稿
│   └── remote-public.html         # 远程公共公告页
└── assets/
    └── images/
        ├── background.jpg         # 星空背景图
        └── magic-circle.jpg       # 魔法阵素材
```

---

## 12. 技术约束

1. 所有页面为单文件 HTML（CSS + JS 内联），无构建步骤
2. Tailwind CSS 4 browser CDN 用于工具类，关键布局使用自定义 CSS
3. 移动端优先，375px 基准宽度
4. 所有装饰元素使用 CSS/SVD 实现，不依赖外部图片（除 background.jpg）
5. 支持 `prefers-reduced-motion` 无障碍降级
6. UTF-8 编码，`lang="zh-CN"`
