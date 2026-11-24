# auth-server ops（现行：宝塔 + systemd + MySQL）

> 2026-11-24 重整：Docker + Postgres + 自签证书时期的运维脚本（deploy/rollback/smoke/run_purge/run_redact
> + 一套 PG 语法 SQL）已整体归档到 `_legacy_docker/ops/`，**不要再执行**——它们同步的
> `nginx/starlight.conf`、`docker-compose.yml`、`Dockerfile` 在仓库里已不存在，第一条命令就会失败。
> 现行部署口径见 `docs/DEPLOY.md`（唯一权威）。

## 现行文件

| 文件 | 用途 | 是否写生产 |
|---|---|---|
| `deploy_api.sh` | 远端快照 → `rsync app/ static/` → `chown www:www` → `systemctl restart` → 回环 /health 轮询 | 是（需 lead 批准后执行） |
| `smoke_api.sh` | 部署后只读冒烟：回环 /health、/ 与 /docs 应 404、公网 /admin/ 200、无 token 401、/v1/ota 与 /v1/portal 应 404 | 否（几条 GET） |
| `sql/mysql_migrate_guest_stats_20261123.sql` | 游客统计列迁移（MySQL 5.7 手工执行；应用 ensure_schema 也会幂等补列） | 是（需批准） |

历史脚本与其 PG SQL：`../_legacy_docker/ops/`（含 rollback_api.sh / run_purge.sh / run_redact.sh / smoke_pg_activity.py）。

## 用法

```bash
cd auth-server/scripts/ops
./deploy_api.sh --dry-run                 # 打印命令清单，不连远端、零写操作
./deploy_api.sh                           # 发布（默认 REMOTE=starlight，ROOT=/www/wwwroot/starlight-mod-auth）
./deploy_api.sh --with-reqs               # requirements.txt 变更时用（远端 .venv/bin/pip install）
./smoke_api.sh                            # 部署后冒烟
```

可覆盖变量：`REMOTE` `ROOT` `SVC` `PORT` `OWNER` `PUBLIC_HEALTH`（deploy）、`BASE`（smoke）。

## 铁律

1. **不碰** `.env` / `certs/` / `data/` / `keys/` / `backups/` / `*.db` / `.venv`；同步白名单只有 `app/`（+`static/`、`requirements.txt`）。
2. 不整目录上传、不带 `--delete`；远端其余文件保持原样。
3. 任何 DELETE/UPDATE 必须：先导出取证 → 事务 → 核行数 → 不符即 ROLLBACK（PG 时期脚本已归档，MySQL 侧需另行改写后再用）。
4. 不自动建定时任务；清理类脚本手动执行。
5. 回滚：`ssh <host> "cd /www/wwwroot/starlight-mod-auth && tar xzf backups/pre_<TS>/code.tar.gz && systemctl restart starlight-mod-auth"`。

## 状态

`deploy_api.sh` / `smoke_api.sh` 为 2026-11-24 新写，**未在真实生产链路上实测**（本机 ssh 别名 `starlight`
指向旧 Docker 主机 47.100.216.101，生产已迁至 154.94.232.231 且未配 ssh 别名）。
首次使用请先 `--dry-run`，再带上 `REMOTE=root@154.94.232.231` 小步验证。
