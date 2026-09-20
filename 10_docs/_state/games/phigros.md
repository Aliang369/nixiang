# Phigros 状态摘要

> 由 2026-09-20 `current.md` 索引拆分生成。后续只更新本文件；
> 原始完整历史见 `../history/current_before_split_20260920.md`。

## 2026-09-20 索引迁移记录

> 来源：`../history/current_before_split_20260920.md` 的原始行。

<!-- source-line:94 -->
| [Phigros](../../phigros/分析_斌哥包抓点_20260908.md) | 3.20.0 / vc 154 | **✅ 20260908 定稿交付（MuMu 实测通过）**：正式包=…3.20.0_starry-releaseSigned.apk（2.5G，包内 so=最新构建字节一致，v2/v3 签名过）；功能=「玩法→满分」（复刻斌哥增强链：hook RVA 0x1AF0A7C 每帧函数，ON 调 0x1AEC318 Perfect 事件处理驱动准度积分；实测分数 10 万级+连击叠高=斌哥一致）；**真·自动跳未完成**（4 轮 Trace 判定点未锁死，方法名全混淆；字段语义+弯路+重启路线见抓点文档 §5）；**>2G 新坑实锤**=zip64 EOCD 归一化（surgical 脚本）；复现链=scripts/surgical_phigros_3.20.0.sh + so_only.sh（阈值已修）；遗留=后台 game_id=phigros 配置（鉴权卡，交付前运营侧完成） |
