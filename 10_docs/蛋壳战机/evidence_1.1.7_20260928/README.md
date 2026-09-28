# 证据索引：蛋壳战机 1.1.7（2026-09-28）

- `decisive_startup_20260928.txt` — 2026-09-28 15:22:35 冷启动（serial `f9a116a2`，pid 26011）决定性行：
  `RegisterNatives 19/19`、四项 Hook 全命中（`PostProcessLearnedSkill file=0x6EDE058`）、
  三个辅助方法按名解析成功、`lib ready` / `初始化成功` / `MOD 已就绪`、菜单仅剩四项。
- 完整 logcat（不入库）：`09_test/蛋壳战机/evidence_1.1.7_20260928/logcat_startup_20260928.txt`（17,823 行）。
- 设备包指纹：`pm path` 指向的 `base.apk` md5 = `5fd08acb6ffa9725c117dcb6a13a3658`，与本地唯一正式包一致。

## NOT PROVEN（本目录不含）

- 局内「重复同名技能升级」实测：本轮没有再进局造同名重复场景，日志里**没有**
  `[dkzj] upgrade requested=N -> granted=M`。只能靠用户口述（见功能验收卡 §4.2）。
- 武器升级：本功能不介入，用户明确「先这样吧」。
