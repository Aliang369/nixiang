# 数据库备份

卡密、审计、公告都在 MySQL 库里，**务必定期备份**。

## 一键备份

在服务器 `auth-server` 目录：

```bash
chmod +x scripts/backup_db.sh
./scripts/backup_db.sh
```

脚本从 `.env` 的 `DATABASE_URL` 自动解析连接信息，无需额外配置。

输出到 `auth-server/backups/`：

- MySQL 生产：`starlight_mysql_starlight_mod_YYYYMMDD_HHMMSS.sql.gz`
- 本机 SQLite 开发：`starlight_sqlite_YYYYMMDD_HHMMSS.db.gz`

环境变量：

| 变量 | 默认 | 说明 |
|------|------|------|
| `BACKUP_DIR` | `./backups` | 备份目录 |
| `KEEP_DAYS` | `14` | 自动删掉更早的备份 |
| `ENV_FILE` | `./.env` | 连接串来源 |
| `SQLITE_PATH` | `./starlight.db` | SQLite 路径（本地开发） |

## 建议 cron（每天 3:30）

```bash
crontab -e
```

加入：

```cron
30 3 * * * cd /www/wwwroot/starlight-mod-auth && ./scripts/backup_db.sh >> /www/wwwlogs/starlight-backup.log 2>&1
```

也可在宝塔「计划任务」里加一条 Shell 脚本任务，内容相同。

## 恢复（MySQL）

```bash
# 1) 停服务，避免写入竞争
systemctl stop starlight-mod-auth

# 2) 解压导入（账号密码见 .env）
gunzip -c backups/starlight_mysql_starlight_mod_XXXXXXXX_XXXXXX.sql.gz \
  | MYSQL_PWD='<库密码>' mysql -h127.0.0.1 -u starlight_mod starlight_mod

# 3) 起服务
systemctl start starlight-mod-auth
curl -s https://auth.xgmfyx.cn/health
```

## 恢复（SQLite，本地开发）

```bash
gunzip -c backups/starlight_sqlite_XXXXXXXX_XXXXXX.db.gz > starlight.db
```

## 注意

- `backups/` 含敏感数据，不要提交到 git、不要对公网开放目录
- 备份文件建议再拷一份到本机或对象存储
- 不要用宝塔的「数据库备份」覆盖运行中的库；如需用面板备份，先 `systemctl stop` 再操作
