# 本机逆向工作区（nixiang）

规则统一在 `AGENTS.md`，技能在 `skill/`。接手任何任务先读 `AGENTS.md`，再按其中的读取顺序执行。

## 每轮开工第一步（双机协作，不可跳过）

本仓由 **mac 端**与 **Windows 端**共用同一个仓库（分支 `main`）。**每次接手任务，第一步先执行下面三条，再读其它内容：**

```bash
cd "$NIXIANG_ROOT"                 # Win 端在 Git Bash 里；mac 端在 zsh 里
git status -sb                     # 必须干净；不干净先报告处理，不要 pull
git pull --rebase --autostash      # 拿到另一台机器已推送的提交
git log --oneline -5
```

- **顺序不可颠倒**：先 `status` 再 `pull`；工作区不干净时 pull/rebase 会把未提交改动卷进去。
- **一机一游戏**：同一轮只由一台机器写同一个游戏，开工前先划定归属（如 `mac → 游戏 A`，`win → 游戏 B`）。
- push 被拒（另一台已先推）→ `git pull --rebase --autostash` 后再 push；**禁止 `push --force`**。
- 完整规则见 `AGENTS.md` 的「双机协作」一节与 `10_docs/双机协作/双机协作同步方案.md`。