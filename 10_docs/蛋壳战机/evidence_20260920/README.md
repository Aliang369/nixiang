# 蛋壳战机 1.1.5 真机证据索引

## 结论

- 四项 Hook 已在真机冷启动安装成功，菜单授权连续 `grant ok`。
- 偷盗技能补授链已触发；重复同名技能不升级，已记录为已知限制。
- 启动与冷启动样本未见 `FATAL`、`SIGSEGV` 或 `ANR`。

## 决定性证据

| # | 证据 | 文件与定位 | 说明 |
|---|---|---|---|
| 1 | 四项 Hook 安装成功 | `logcat_install_coldstart_20260920.txt:311-317` | 四个 file-offset 挂点与 `ready=1` |
| 2 | 连续冷启动仍正常 | `logcat_coldstart_repeat_20260920.txt:22-35` | 4 条 Hook 与 `grant ok` |
| 3 | 修复版菜单初始化 | `logcat_duplicate_skill_fix_coldstart_20260920.txt:2-14` | grant、攻击倍率与三个 Toggle |
| 4 | 偷盗技能补授命中 | `logcat_steal_order_fix_coldstart_20260920.txt:36` | `steal selected=10000601 granted=2` |
| 5 | 设备包安装核对 | `package_after_install_20260920.txt` | 安装后包状态；正式包 hash 见收尾清单 |

## 复现命令

```bash
rg -n '\\[dkzj\\].*hooked|grant ok|ready=1' \
  10_docs/蛋壳战机/evidence_20260920/logcat_*.txt

rg -n 'steal selected=' \
  10_docs/蛋壳战机/evidence_20260920/logcat_steal_order_fix_coldstart_20260920.txt
```

## 未证明项

- CLAIM：攻击倍率、上帝模式、广告奖励在战斗/广告完整业务链中的最终行为。
- EVIDENCE：当前只有 Hook、授权和有限交互日志，缺少用户全量人工验收。
- VERDICT：NOT PROVEN。

## 原始资料

- 完整日志：本目录 `logcat_*.txt`
- 结论文档：`../星空落地_1.1.5_20260920.md`
- 功能验收：`../功能验收_偷盗技能_20260920.md`
