# 运营后台 UI 规范

> 视觉 Token 与美术总则见仓库根目录 [设计风格美术规范.md](../../设计风格美术规范.md) v2.0。
> 更新：2026-09-21（门户上线 + 后台 UI 重构 + CSS/JS 拆分后同步）。

## 文件结构与缓存

- 骨架：`static/admin/index.html`（~1150 行，只含 HTML + 内联骨架脚本）
- 样式：`css/admin.css`（全量主题）+ `css/time-picker.css`（时间选择/行下拉）
- 脚本：`js/` 9 个文件，加载顺序 time-picker → core → audit → features → dashboard → games → cards → announcements → toasts_ota → settings
- **缓存铁律**：改任何 css/js 必须递增 `index.html` 里的 `?v=N`，否则线上不生效
- 门户页：`static/portal/index.html`（单文件，含内联样式与脚本）

## 布局

- 左侧栏：brand2 品牌卡（六芒星 SVG + 渐变字）→ side-nav 导航（淡紫容器）→ 账号卡（下拉菜单：修改密码 / 退出登录）
- 顶栏已删除；各 tab 标题区只留标题与操作按钮
- 导航顺序：数据看板 → 游戏列表 → 菜单模板 → 卡密管理 → 远程公告 → 游戏补丁 → 开屏Toast → 操作日志 → 后台设置（设置页暂空置）
- 内容区 panel：淡紫低透明 `rgba(124,58,237,.07)` + 紫边框，无毛玻璃，星空透出

## 主色与按钮

- 主按钮 `.btn`：`linear-gradient(135deg, #8B5CF6, #7C3AED)`（与新界面紫统一，勿回退旧薰衣草紫）
- ghost 淡紫描边；danger 红只用于删除/退出类
- 卡片（brand2 / 账号卡 / stat）统一 `rgba(124,58,237,.26→.14)` 渐变底

## 表格铁律

- 全列表每页 **20 条**（core.js 各 PageSize；数据看板/菜单模板为本地分页）
- 无斑马纹；th/td 水平居中；td `nowrap + ellipsis`（max-width 340）
- **`.row-actions` 禁止改 td 的 display**（flex/block 会让 td 脱离 table-cell，行分隔线错位——已踩坑两次）；按钮 inline 排列 + `white-space: nowrap`，`td.row-actions { max-width: none }`，宽度不够靠 `.table-wrap` 横向滚动
- 卡密/游戏操作列收缩为「操作 ▾」二级下拉（time-picker.css 的 `.card-row-menu` 紫色主题）

## 账号与交互

- 修改密码：账号卡下拉菜单 → 弹窗（原/新/确认），成功后 `doLogout` 直退
- 退出登录：先确认弹窗，确认后 `doLogout`
- 删除类操作走原生 confirm；账号菜单项 danger 红、普通项淡紫

## 星空装饰

- 全屏 `.sky`：星云 + 3 层星点 + **2 组亮星层（twinkles，交错闪烁）+ 5 条流星**（全部右上→左下，两种角度 -28°/-38°，错峰）
- 门户页同套星空与淡紫卡片；游戏卡片名字下显示渠道标签 `.ch-tag`
- `prefers-reduced-motion` 下关闭动画

## 渠道字段

- 游戏来源渠道 `games.channel`：TapTap/九游/4399/好游快爆/B站/国际服，选「其它」自定义输入；PATCH 空串=清空
- 下载渠道 `games.download_url`：JSON `[{name,url}]` 最多 3 个，旧纯 URL 兼容
