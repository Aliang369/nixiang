# 安全收尾清单（接游戏之前）

## 已在代码里

1. **管理登录限流**：同一 IP 15 分钟内失败 5 次 → 临时禁止登录  
2. **管理 IP 白名单（可选）**：`.env` 设置  
   ```env
   ADMIN_ALLOW_IPS=你的家宽公网IP
   ```
   空 = 不限制。配置后只有列表内 IP 能调 `/v1/admin/*`（含登录）。
3. **玩家卡密登录限流**：按 IP（`CARD_LOGIN_MAX_FAILS` / `CARD_LOGIN_WINDOW_SECONDS`）  
4. **客户端授权短票（防 AI 改 bool）**：见专用说明 **[GRANT_AUTH.md](./GRANT_AUTH.md)**（含你日常三步操作 + 给 AI 的改代码地图）  
5. **Release R8 混淆**、禁止明文 HTTP、`allowBackup=false`  
6. **客户端 HTTPS 证书固定**：`assets/certs/starlight.crt`（`AuthClient`）

查自己的公网 IP：浏览器打开 `https://ip.sb` 或 `https://ifconfig.me`

## 服务器上请你手动做

1. **定期备份数据库**（见 [BACKUP.md](./BACKUP.md)）  
   ```bash
   ./scripts/backup_db.sh
   # 建议 cron 每天跑一次
   ```

2. **防火墙只放行**  
   - `22` SSH  
   - `8443` 验证/管理 HTTPS  
   - 宝塔面板端口  
   - **不要**对外开放 `8000`、`5432`

3. **轻量控制台防火墙** 与宝塔安全规则保持一致  

4. **改强管理密码**（若还在用简单密码）  
   - 改 `.env` 的 `ADMIN_PASSWORD` **不会**自动更新已存在的管理员  
   - 简易做法：管理台能进的话先别管；或重建库（会丢卡密，慎用）  
   - 也可用下面 SQL 重置（进 db 容器）：  
     ```bash
     docker compose exec api python -c "from app.security import hash_password; print(hash_password('新密码'))"
     docker compose exec db psql -U starlight -d starlight -c "UPDATE admin_users SET password_hash='上面输出的哈希' WHERE username='你的用户名';"
     ```

## 部署本轮代码

上传更新后的 `app/` 与 `env.example`，然后：

```bash
cd /www/wwwroot/starlight-auth/auth-server
# 编辑 .env 按需加上 ADMIN_ALLOW_IPS=...
docker compose up -d --build
```

菜单玩家接口 `/v1/auth/*` **不受**管理白名单影响。
