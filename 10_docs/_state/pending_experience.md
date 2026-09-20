# 挂起与待审批经验

> 来源：2026-09-20 从 `current.md` 原“挂起与待审批经验”区域原样迁移。

- [证书嵌入 so 经验](../_templates/经验_证书嵌入so_20260906.md)：**已批准并合并**（2026-09-06）——证书固定改 so 烘焙（RegisterNatives 19/19），自测壳实测 200；已写入 SKILL.md 分发自检、surgical-build.md、auth-chain.md。跨样本 3 项 NOT PROVEN 随首个新游戏外科包回归，存量包/旧脚本不追溯。
- 超维进化卡死、追迹迷城偶发闪退、无限升级运行与授权、旁路小屏验证：保留在各摘要，不自动恢复执行或操作设备。
- [追迹迷城经验候选](games/追迹迷城.md#待审批经验索引)：classes7 自动发现失效与 classes4 合并、MenuService2 等长槽；待用户批准写入 skill，不能视为通用定律。
- [终末地已处理经验](games/终末地.md#经验索引)：历史记载已获批沉淀及补战报；本次未修改 skill。
- [dump.cs 字段口径与序言校验（我的勇者 7.6.1）](../_templates/经验_dump偏移口径与序言校验_20260920.md)：**已合并**（提交 `e83da4f`，2026-09-20 用户明确授权）——`FileOffToVa` 收 file_off（dump.cs 的 `Offset`，本例 RVA−0x4000，常数须按 PT_LOAD 实测）；序言期望值按小端读成 u32；填错会**静默**退回按名、功能照样通过，唯一判据是 logcat 的 `@rva`/`@by-name`。落地于 `skill/re-implement/version-update-relocate.md` **§0.1** + `hook-rules.md` §6 交叉引用。
- [收官工具链四处摩擦（2026-09-20）](../_templates/经验_收官工具链摩擦_20260920.md)：**已合并**（提交 `8d04780`，2026-09-20 用户「1234都改」）——① gradle 需 `~/.gradle`，workspace-write 沙箱直接拒 → `skill/starry-mod/SKILL.md` + `surgical-build.md`；② `archive_scoped_git.sh --path` 传 `.gitignore` 的 `mod菜单/**/app/src/libs*` 会中止 → `stages.md` 收官第 6 条；③ `pre_install_gate.sh` 查2 方向定案 + 覆盖 `OBFUSCATE("…")`（旧正则抽取 0 / 新 19）→ `_common/pre_install_gate.sh`；④ 无 `pipeline.json` 的游戏入口判定 → `AGENTS.md` 路由 + `stages.md` 开头。
- 新实验使用 [逆向实验记录](../_templates/逆向实验记录.md)；新经验使用 [经验候选卡](../_templates/经验候选卡.md)，审批前保持候选状态。
