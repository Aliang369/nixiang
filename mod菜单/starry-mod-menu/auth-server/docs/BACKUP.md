# 数据库备份

卡密、审计、公告都在库里，**务必定期备份**。

## 一键备份

在服务器 `auth-server` 目录：

```bash
chmod +x scripts/backup_db.sh
./scripts/backup_db.sh
```

默认输出到 `auth-server/backups/`：

- Docker Postgres：`starlight_pg_YYYYMMDD_HHMMSS.sql.gz`
- 本机 SQLite：`starlight_sqlite_YYYYMMDD_HHMMSS.db.gz`

环境变量：

| 变量 | 默认 | 说明 |
|------|------|------|
| `BACKUP_DIR` | `./backups` | 备份目录 |
| `KEEP_DAYS` | `14` | 自动删掉更早的备份 |
| `SQLITE_PATH` | `./starlight.db` | SQLite 路径（非 Docker） |

## 建议 cron（每天 3:30）

```bash
crontab -e
```

加入：

```cron
30 3 * * * cd /www/wwwroot/starlight-auth/auth-server && ./scripts/backup_db.sh >> /var/log/starlight-backup.log 2>&1
```

路径按你实际部署目录改。

## 恢复（Postgres / Docker）

```bash
# 停 API 再导更安全（可选）
gunzip -c backups/starlight_pg_XXXXXXXX_XXXXXX.sql.gz \
  | docker compose exec -T db psql -U starlight -d starlight
docker compose restart api
```

## 恢复（SQLite）

```bash
# 先停 uvicorn
gunzip -c backups/starlight_sqlite_XXXXXXXX_XXXXXX.db.gz > starlight.db
```

## 注意

- `backups/` 含敏感数据，不要提交到 git、不要对公网开放目录
- 备份文件建议再拷一份到本机或对象存储
