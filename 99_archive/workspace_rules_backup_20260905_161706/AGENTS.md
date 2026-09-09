# 安卓游戏逆向工作区（nixiang）— 唯一入口

> 所有 AI 程序（Cursor / Claude Code / Kilo / Codex / Cline / QClaw 等）先读本文件。
> 本文件只存放**读取顺序**和**一行一条的铁律**；知识正文只此一份，在 `skill/` 与三个 BOOK。

---

## 拿到任务先读

1. `skill/SKILL.md` — 总纲（硬规则 + 数据先于行动 + 防钻牛角尖）
2. 按任务类型**只读一门**子技能（不要全读）：
   - 新游戏 / 新版本接手 → `skill/re-analyze/SKILL.md`
   - 已有包提取玩法 / 抓点 → `skill/re-extract/SKILL.md`
   - 写功能 / Hook → `skill/re-implement/SKILL.md`
   - 星光落地 / 切游戏 / 出包 → `skill/starry-mod/SKILL.md`
   - 分析 `.so` → `skill/ida-plugin-flow/SKILL.md`
   - 卡坑了 → `skill/re-lessons/SKILL.md`
3. 需要流程细节时再读 `PLAYBOOK.md`（接手）→ `WORKFLOW.md`（日常）→ `TOOLBOOK.md`（工具/目录）

## 会话锚点（防跑偏，每次会话开始/结束必做）

- 开始前读 `10_docs/_state/current.md`（当前游戏 / 阶段 A-E / 卡点 / 下一步最小动作）
- 结束后更新它（几行即可）

## 铁律（一行一条，细节在 skill/ 与 BOOK，不在此重复）

1. 游戏 APK 禁止 `apktool b` 整包重建，只走外科 zip 组装。
2. 动态分析优先自注入 `libstarry_magic.so` Trace；Frida/Florida 仅无壳无强反作弊游戏的短探针。
3. 分类目录先建 `{游戏名}/` 子目录（例外见 `TOOLBOOK.md`）。
4. 真机会话结束/暂停必跑 `04_scripts/_common/cleanup_device_re_residue.sh`。
5. 大资源游戏只覆盖安装，不卸载。
6. 所有 hook 必须调原函数。
7. 有过滤的 invoke 钩 > 静态 patch；共用入口无过滤 = 禁区。
8. 需要用户配合（进游戏/开菜单）时先在聊天说明并等确认。
9. 数据先于行动：先收集 → 再判断 → 最后行动；未经实测的判断必须标「⚠️ 待验证」。
10. 同一卡点最多试 2 种方法，第 2 种不行 → 先更新 `current.md` 卡点栏，再决定继续或问用户。
11. `skill/` 下任何修改/新增/删除必须先经用户明确同意。
12. 每次改完代码必须走「审查边界」（①改动范围 → ②四条闪退红线 → ③按改动类型查维度 → ④证据门禁，见 `skill/re-implement/code-review-checklist.md`）；用户说「按边界审 / 自查」即触发。停止条件：一轮四步输出结论即停；❌ 只修 ❌ 说的问题；证据不足标 NOT PROVEN 并问用户。

## 经验回流规则

`10_docs/` 里任何复盘/定稿文档的「待办 / 教训」条目，下一次会话开始时必须被吸收：
要么写进对应 skill，要么明确废弃。不要在文档里长期挂着「待合并进 skill」。

## 工具差异（不重要）

各工具的配置目录（`.cursor/`、`.kilo/`、`.workbuddy/`）**只允许放指向本文件与 `skill/` 的指针**，
禁止复制规范正文。判重标准：除本文件外，任何工具目录出现超过 3 行规则正文 = 重复，删。