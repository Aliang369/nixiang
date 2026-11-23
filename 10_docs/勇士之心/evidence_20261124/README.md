# evidence_20261124 — 1.0.6 星光复刻收官证据（决定性行提取）

> 原始目录 `09_test/勇士之心/{combat_20261123,fix5_check}/` 体积大（21 MiB + 2.9 MiB）、含大量被 logd 冲断的原始 logcat，
> 已随 2026-11-24 收尾移入隔离区（见 `滚动物清理清单_20261124.json`）。此处只保留**判据级**文本，足以复查每个结论。

| 文件 | 内容 | 对应结论 |
|---|---|---|
| `fix7clean_cold_decisive.txt` | fix7-clean 冷启的 `[MODMOD]` / `[AUTO]` / `bag api` / `state@` 行 | §9 机器侧回归（零残留 + 11 项功能链） |
| `fix3_coldstart_decisive.txt` | fix3 冷启的 `gold-card hooked` / `bag api` / `evalString` 行 | §6 三项功能修复 |
| `code7_decisive.txt` | `battle auto-req 在线局: reqAutoBattle=true code=7` 等 | §8 副本自动战斗服务端裁决 |
| `driver_fix_cold_decisive.txt` | `files/res/main.js` 的 md5 与自愈重写、`logcat -G 16M` 后日志完整性 | §11 遗留 #1/#2 |
| `combat_REPORT.md` / `combat_decisive_lines.txt` / `combat_pkg_fingerprint.json` | anchorhit 包冷启与包指纹（原文复制） | §3 锚点重定位 |

复现入口：`10_docs/勇士之心/1.0.6星光复刻_20261124.md` §10。
