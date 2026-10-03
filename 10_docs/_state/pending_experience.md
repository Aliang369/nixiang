# 经验生命周期元数据

本文件维护旧经验清单与审批说明；新经验卡的机器可审计状态以经验卡 frontmatter 为准。旧条目暂不批量迁移，审计器将其报告为 `legacy`，不阻断检查。

状态含义：

- `candidate`：记录了可验证的候选经验，尚未获用户批准。
- `approved`：用户已明确批准沉淀，但尚未合并到规范/skill。
- `merged`：已经落入目标规则，必须记录实际目标位置与合并 commit。
- `rejected`：用户明确决定不采纳，保留否定依据。
- `stale`：适用条件变化或被新证据推翻，保留历史与替代关系。

新建经验卡请使用 `10_docs/_templates/经验候选卡.md` 的 YAML frontmatter，并运行：

```bash
python3 -B 04_scripts/_common/audit_experience_registry.py
```

本工具只审计 `10_docs/_templates/经验*.md`，不自行审批经验、不改写卡片、不迁移历史内容。

## 现有待审批/已合并记录

- [证书嵌入 so 经验](../_templates/经验_证书嵌入so_20260906.md)：**已批准并合并**（2026-09-06）——证书固定改 so 烘焙（RegisterNatives 19/19），自测壳实测 200；已写入 SKILL.md 分发自检、surgical-build.md、auth-chain.md。跨样本 3 项 NOT PROVEN 随首个新游戏外科包回归，存量包/旧脚本不追溯。
- 超维进化卡死、追迹迷城偶发闪退、无限升级运行与授权、旁路小屏验证：保留在各摘要，不自动恢复执行或操作设备。
- [追迹迷城经验候选](games/追迹迷城.md#待审批经验索引)：classes7 自动发现失效与 classes4 合并、MenuService2 等长槽；待用户批准写入 skill，不能视为通用定律。
- [终末地已处理经验](games/终末地.md#经验索引)：历史记载已获批沉淀及补战报；本次未修改 skill。
- [dump.cs 字段口径与序言校验（我的勇者 7.6.1）](../_templates/经验_dump偏移口径与序言校验_20260920.md)：**已合并**（提交 `e83da4f`，2026-09-20 用户明确授权）——`FileOffToVa` 收 file_off（dump.cs 的 `Offset`，本例 RVA−0x4000，常数须按 PT_LOAD 实测）；序言期望值按小端读成 u32；填错会静默退回按名，唯一判据是 logcat 的 `@rva`/`@by-name`。落地于 `skill/re-implement/version-update-relocate.md` §0.1 + `hook-rules.md` §6。
- [收官工具链四处摩擦（2026-09-20）](../_templates/经验_收官工具链摩擦_20260920.md)：**已合并**（提交 `8d04780`，2026-09-20 用户「1234都改」）——落地于 `skill/starry-mod/SKILL.md`、`surgical-build.md`、`stages.md`、`pre_install_gate.sh` 和 `AGENTS.md`。
- [splice 离线闭环 + early hook 超时（超维进化 0.3.5，2026-09-21）](../_templates/经验_splice离线闭环与earlyhook_20260921.md)：**候选，未审批**——splice 型改动的离线镜像需抽真源码、主机编译并逐字节比 delta；early hook timeout 的含义待适用条件验证。拟落 `hook-rules.md` + `starry-mod/SKILL.md`。
- [热更指纹漂移与令牌浪费（勇士之心 1.0.3）](../_templates/经验_热更指纹漂移_20260921.md)：**已合并**（2026-09-21 用户明确授权）——已落入 `hook-rules.md` §24、`re-extract/hotupdate-fingerprint-recheck.md`、`AGENTS.md` 和 `stages.md`。
- 新实验使用 [逆向实验记录](../_templates/逆向实验记录.md)；新经验使用 [经验候选卡](../_templates/经验候选卡.md)，审批前保持候选状态。
