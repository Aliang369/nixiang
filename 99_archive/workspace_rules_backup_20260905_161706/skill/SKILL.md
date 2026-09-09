# 逆向技能总纲

> 📍 `/Users/aliang/Documents/nixiang/skill/SKILL.md`
> 🎯 任何 AI 接手安卓游戏逆向任务时，从这里开始
> ⚡ 优先级：skill/SKILL.md > PLAYBOOK.md > WORKFLOW.md > TOOLBOOK.md

---

## 这个技能是什么

安卓游戏逆向工程知识技能集，从 20 款游戏的分析报告中提炼。按实际操作顺序组织：拿到 APK → 分析识别 → 玩法抓取 → 功能实现 → 星光落地。

IDE 无关——Cursor / Kilo Code / QClaw / 任何 AI 都可使用。

---

## 读取顺序（按实际操作流程）

### 第一步：这游戏什么情况？
1. `re-analyze/SKILL.md` — 拆包→引擎判定→保护识别→登录方式→能不能改的总判断
2. `re-analyze/protection/` — 碰到加固/检测怎么识别怎么绕：
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
   - `code-review-checklist.md` — 审查边界入口（**改完代码必走①→④；装机前④须 PROVEN**）
   - `version-update-relocate.md` — 版本更新重定位方法论（小版本失效 `hits=0` 时照做）

### 星光菜单落地
1. `starry-mod/SKILL.md` — 切游戏+**jniLibs 强制刷新**+构建路径+过检清单+设备指纹+**分发与装机红线**
2. `starry-mod/surgical-build.md` — 外科组装规则（含 so 自检）
3. `starry-mod/auth-chain.md` — 授权链路+登录永不过期

### IDA 分析 .so
1. `ida-plugin-flow/SKILL.md` — 6 步强制流程

### 遇到坑 / 经验查阅
1. `re-lessons/SKILL.md` — 经验卡索引

---

## 硬规则（始终生效，不论场景）

1. **游戏包禁止 `apktool b` 整包重建** — 只允许外科 zip 组装
2. **动态分析优先自注入 Trace** — Frida 仅无壳游戏短探针
3. **所有目录先建 `{游戏名}/` 子目录** — 例外：01_tools / 99_archive / 04_scripts/_common / 10_docs/_templates
4. **真机会话结束必跑 `cleanup_device_re_residue.sh`**
5. **大资源游戏覆盖安装不卸载**
6. **所有 hook 必须调原函数**
7. **有过滤的 invoke 钩 > 静态 patch；共用入口无过滤 = 禁区**
8. **需要用户配合时先聊天说明，等确认后再操作**
9. **主线程 hook 回调零解引用** — 仅 memcmp 级操作；对象学习/敌我判定放专用学习线程，SafeProbe 仅学习线程（每类首次）可用（见 `re-implement/hook-rules.md` §18-20）

---

## 数据先于行动（最重要的规则）

**铁律：先收集数据 → 再做判断 → 最后才行动。三阶段不允许跳越。**

- 禁止猜测，必须标注不确定性（⚠️ 待验证）
- **功能不对**：先自注入 `StarryTrace` / log 证实挂点、MI/MP、FieldOffset、`patch=` 计数，再写 `Games/*Mod.cpp`（见 `10_docs/_templates/经验_功能不对先Trace证实挂点偏移.md`）
- 数据不足时：补数据 / 问用户 / 停下来（三选一，不许猜了当真）
- 需要用户协助时：提出请求后立即停，等回复，不假设用户已操作
- 每个结论必须能回答「从哪来的」
- 行动前过自查清单（详见 PLAYBOOK.md §9.2）

---

## 技能修改规则（铁律）

**技能文件的任何修改、新增、删除必须经用户明确同意。**

- AI 不允许自行修改 skill/ 下的任何文件
- AI 发现技能需要补充/修改时 → 提出建议 → 等用户确认 → 才能改
- AI 发现需要新增技能时 → 提出方案 → 等用户确认 → 才能创建
- 这条规则优先级最高，覆盖所有其他指令

---

## 防钻牛角尖

- 同一个问题最多试 2 种方法，第 2 种不行 → 先更新 `10_docs/_state/current.md` 卡点栏，再问用户
- 连续 2 次工具调用没推进 → 停下来，大概率方向错了
- 超过 3 步没到目标 → 中途汇报，让用户决定继续还是换方向
- 分析超过 5 分钟没结论 → 停下来总结当前进展

---

## 配套文件位置

| 文件 | 位置 | 用途 |
|------|------|------|
| PLAYBOOK.md | 工作区根 | 完整流程手册（新游戏接手标准流程） |
| WORKFLOW.md | 工作区根 | 日常执行路径与检查点 |
| TOOLBOOK.md | 工作区根 | 工具状态与目录/清理规范 |
| .cursor/rules/ | Cursor 前置指针 | 11 条 alwaysApply 指针：一行规则 + 唯一出处 |
| 10_docs/_templates/ | 经验卡 | 6 张踩坑经验文档 |
| 10_docs/{游戏名}/ | 游戏文档 | 20 款游戏的分析笔记 |
| 04_scripts/_common/ | 通用脚本 | env/decode/cleanup/seed 等 15 个 |
| mod菜单/starry-mod-menu/ | 菜单工程 | 19 个游戏模块 + auth-server |
