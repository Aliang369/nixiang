# evidence_1.1.8_20261014 — 1.1.8 两轮修补证据

## 内容

- `decisive_boot_20261014_035342.txt` —— 2026-10-14 03:53:46 真机 f9a116a2 启动预检的决定性行
  （只保留 `com.survivor.acecn` 主进程 pid 25237）：引导/桥接、15 个 hook 安装结果、
  菜单功能表变化（拦截上报摘除）、本包崩溃检查（空＝干净）。
- `decisive_boot_20261014_041011.txt` —— 2026-10-14 04:10 真机 f9a116a2 启动预检的决定性行
  （主进程 pid 5043）：**「立即胜利」删除轮**，hook 12 项、菜单 5 项（立即胜利已消失）。

## 全量日志位置

按 AGENTS 约定，完整 logcat 不入库：
`09_test/蛋壳战机/precheck_20261014_035342/logcat.txt`（47,900 行）、
`09_test/蛋壳战机/precheck_20261014_041011/logcat.txt`（13,920 行）、各自的 `monkey.log`。
日志目录中含同机其它包噪音（例如 `com.veewo.neonabyss2` 的 SIGSEGV 与本包无关）。

## 第一轮结论（03:52 卡密包挂点对齐）

- 15/15 hook 命中，含 4 个新挂点（`HurtBase.FinalizeAttackResult` 0x6D39C44、
  `EntityHero.set_CurrentHp` 0x6D227E0、`GameAdsManager.CheckAndPlayVideo` 0x43C9720、
  `BattleWorldContext.SetWinPlayerId` 0x6ECA3D4）；
- 菜单 6 项（Category + 5），`Toggle_拦截上报` 已摘除、拦截上报 hook 仍常开；
- 零 SIGSEGV / tombstone / FATAL / ANR，进程存活。

详见 [`../星空落地_1.1.8_修补_卡密包挂点对齐_20261014.md`](../星空落地_1.1.8_修补_卡密包挂点对齐_20261014.md)。

## 第二轮结论（04:10 删除立即胜利 + 偷盗技能完善）

- 12/12 hook 命中（移除 `SetWinPlayerId` / `OnUpdateHook` / `CheckEnemyAllDie` 三项）；
- 菜单 5 项（Category + 4 功能），`Toggle_立即胜利` 已消失；
- 偷盗技能调整：去重窗口 3000→800ms、快照有效期 30s→120s、升级链补 `UpgradeSkillId_B`(0x150)、
  授予日志改为全量 id 列表；
- 零 SIGSEGV / tombstone / FATAL / ANR，空转 80s 进程存活。

详见 [`../星空落地_1.1.8_修补_删除立即胜利与偷盗技能完善_20261014.md`](../星空落地_1.1.8_修补_删除立即胜利与偷盗技能完善_20261014.md)。
