# auth-server ops（本轮新增，部署/回滚/数据修复）

> 所有脚本只操作 `/www/wwwroot/starlight-auth/auth-server`，不碰其它游戏、不碰 .env/volume。
> 生产写操作前必须已有 `backups/pre_<TS>/` 快照（code.tar.gz + starlight.dump + 镜像 rollback tag）。

## 文件
| 文件 | 用途 | 是否写生产 |
|---|---|---|
| `deploy_api.sh` | manifest 精确同步 + 构建 + 仅重建 api + 健康检查 | 是（需 lead 批准后执行） |
| `rollback_api.sh` | 回滚到指定 rollback 镜像 tag | 是（应急） |
| `sql/redact_auth_events_detail.sql` | 历史明文卡密脱敏（分批可重入） | 是（需批准） |
| `sql/purge_payload_presence.sql` | 清理已取证的注入 presence 行 | 是（需批准） |
| `sql/create_indexes_concurrently.sql` | 大表索引 CONCURRENTLY（不锁表） | 是（需批准） |
| `sql/cleanup_session_tokens.sql` | 过期/撤销 token 清理（默认 dry-run） | 否（默认只统计） |

## 铁律
1. 不改 `.env`、不改 docker volume、不整目录上传（用 manifest 白名单）。
2. 任何 DELETE/UPDATE 必须：先导出取证 → 事务 → 核行数 → 不符即 ROLLBACK。
3. 索引一律 `CREATE INDEX CONCURRENTLY`，禁止在 ensure_schema 里建大表索引。
4. 不自动建定时任务；清理类脚本手动执行。
