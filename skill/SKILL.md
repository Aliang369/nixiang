# 逆向技能总纲

> 📍 `/Users/aliang/Documents/nixiang/skill/SKILL.md`
> 读取、授权、协作与通用政策唯一入口：`../AGENTS.md`。本文件仅作按需知识索引，不另设优先级。

---

## 这个技能是什么

安卓游戏逆向工程知识技能集，从 20 款游戏的分析报告中提炼。按实际操作顺序组织：拿到 APK → 分析识别 → 玩法抓取 → 功能实现 → 星光落地。

IDE 无关——Cursor / Kilo Code / QClaw / 任何 AI 都可使用。

---

## 按需知识索引（由 AGENTS.md 路由，不要求依次通读）

### 第一步：这游戏什么情况？
1. `re-analyze/SKILL.md` — 拆包→引擎判定→保护识别→登录方式→能不能改的总判断
2. `re-analyze/protection/` — 保护组件识别与既有案例：
   - `_总纲_加固与校验全体系.md` — **分层模型 + 死法分类学 + 排查决策树（拿新游戏/卡点无思路时先读）**
   - `baidu-shell.md` — 百度 Sagittarius 加固
   - `ace-tss.md` — 腾讯 ACE/TSS（最危险）
   - `dex-integrity.md` — DEX 完整性 + 黑名单校验
   - `root-check.md` — Root 检测
   - `other-sign-check.md` — libqkcheck / MT Killer / 其他签名校验
   - `kwprotect.md` — Kiwi VM + KwProtect 完整性网 + Q1 PM 证书（加固保护族，8/28 首测）
3. `re-analyze/login/` — 登录与签名问题：
   - `taptap-web-login.md` — TapTap 强制网页登录（三种 SDK 版本）
   - `qq-wechat-sso.md` — QQ/微信 SSO 限制
   - `sign-spoof.md` — 运行时签名伪装（SignSpoof / MT Killer）

### 第二步：从斌哥包提取玩法（或从零抓点）
1. `re-extract/SKILL.md` — 有没有斌哥包？→ 走对照还是从零
2. `re-extract/binge-memdiff.md` — 斌哥壳 memdiff 完整流程（最通用）
3. 没有斌哥包时按引擎走：
   - `il2cpp-from-scratch.md` — Unity IL2CPP 从零抓点
   - `cocos-js-from-scratch.md` — Cocos JS 从零抓点 + jsc 解密
   - `hybridclr-runtime.md` — HybridCLR 热更抓点
   - `xlua-from-scratch.md` — xLua 抓点

### 第三步：把功能写出来
1. `re-implement/SKILL.md` — 按 Hook 模式选择实现方式
2. `re-implement/hook-rules.md` — Hook 通用铁律（必读）
3. 按需读取：
   - `damage-pattern.md` — 伤害修改（秒杀/无敌/全屏/敌我判定）
   - `economy-pattern.md` — 经济修改（货币/免支付/免广告/云存档）
   - `unlock-pattern.md` — 解锁修改（角色 dic 直写）
   - `utility-pattern.md` — 工具（加速/零CD/挂机修复/悬浮窗）
   - `anti-cheat-pattern.md` — 反作弊应对（服务端重放/JS反作弊/设备清理）
   - `anti-ban-analysis.md` — 封号风险与量级安全线（**写功能前必读**）
   - `code-review-checklist.md` — 审查边界入口（分阶段门禁与修复后直接影响面复查见 `../AGENTS.md`）
   - `version-update-relocate.md` — 版本更新重定位方法论（小版本失效 `hits=0` 时照做）

### 星光菜单落地
1. `starry-mod/SKILL.md` — 切游戏+**jniLibs 强制刷新**+构建路径+过检清单+设备指纹+**分发与装机红线**
2. `starry-mod/surgical-build.md` — 外科组装规则（含 so 自检）
3. `starry-mod/auth-chain.md` — 授权链路+登录永不过期

### 收尾与最小复现
1. [TOOLBOOK.md](../TOOLBOOK.md)「清理规则」「真机残留清理」— 保留输入、隔离重建、授权执行与设备分责。
2. [游戏收尾与最小复现清单](../10_docs/_templates/游戏收尾与最小复现清单.md) — 按需填写证据与未处理项。

### IDA 分析 .so
1. `ida-plugin-flow/SKILL.md` — 按问题选步骤；仅有加密定位问题时触发扫描，通用政策见 `../AGENTS.md`

### 遇到坑 / 经验查阅
1. `re-lessons/SKILL.md` — 经验卡索引

---

## 通用政策与证据入口

授权、用户协作、信息增益预算、状态索引、经验生命周期、技能修改审批、清理与审查分阶段门禁统一引用 `../AGENTS.md`，此处不复制技术铁律或旧读取协议。

功能不对时的证据参考：`10_docs/_templates/经验_功能不对先Trace证实挂点偏移.md`（挂点、MI/MP、FieldOffset、`patch=` 计数）；行动前问题清单见 `../PLAYBOOK.md` §9.2。仅在任务需要时查阅。

---

## 配套文件位置

| 文件 | 位置 | 用途 |
|------|------|------|
| PLAYBOOK.md | 工作区根 | 完整流程手册（新游戏接手标准流程） |
| WORKFLOW.md | 工作区根 | 日常执行路径与检查点 |
| TOOLBOOK.md | 工作区根 | 工具状态与目录/清理规范 |
| .cursor/rules/ | Cursor 前置指针 | 短指针与来源链接，匹配条件见各文件 |
| 10_docs/_templates/ | 经验卡 | 踩坑经验文档（数量为历史记录，状态按 AGENTS.md） |
| 10_docs/{游戏名}/ | 游戏文档 | 20 款游戏的分析笔记 |
| 04_scripts/_common/ | 通用脚本 | env/decode/cleanup/seed 等 15 个 |
| mod菜单/starry-mod-menu/ | 菜单工程 | 19 个游戏模块 + auth-server |
