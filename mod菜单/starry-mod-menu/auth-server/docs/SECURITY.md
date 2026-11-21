# 安全收尾清单（接游戏之前）

## 已在代码里

1. **管理登录限流**：同一 IP 15 分钟内失败 5 次 → 临时禁止登录  
2. **管理 IP 白名单（可选）**：`.env` 设置  
   ```env
   ADMIN_ALLOW_IPS=你的家宽公网IP
   ```
   空 = 不限制。配置后只有列表内 IP 能调 `/v1/admin/*`（含登录）。
3. **玩家卡密登录限流**：按 IP（`CARD_LOGIN_MAX_FAILS` / `CARD_LOGIN_WINDOW_SECONDS`）  
4. **客户端授权短票（防 AI 改 bool）**：见专用说明 **[GRANT_AUTH.md](./GRANT_AUTH.md)**
5. **Release R8 混淆**、禁止明文 HTTP、`allowBackup=false`  
6. **HTTPS**：服务端由 Let's Encrypt 签发 `auth.xgmfyx.cn`，Android 系统根证书天然信任，
   客户端无需再内置自签证书。若仍要做证书固定，固定 Let's Encrypt 中间证书（R10/R11）的 SPKI，
   并注意每次换中间证书要跟版本。

查自己的公网 IP：浏览器打开 `https://ip.sb` 或 `https://ifconfig.me`

## 服务器上请你手动做

1. **定期备份数据库**（见 [BACKUP.md](./BACKUP.md)）  
   ```bash
   cd /www/wwwroot/starlight-mod-auth && ./scripts/backup_db.sh
   # 建议 cron 每天跑一次
   ```

2. **防火墙只放行**  
   - `22` SSH  
   - `80` / `443`（ACME 校验与站点 HTTPS）
   - 宝塔面板端口  
   - **不要**对外开放 `8010`、`3306`（8010 已绑定 127.0.0.1；3306 由宝塔默认仅本机）

3. **轻量控制台防火墙** 与宝塔安全规则保持一致  

4. **改强管理密码**（若还在用简单密码）  
   - 改 `.env` 的 `ADMIN_PASSWORD` **不会**自动更新已存在的管理员  
   - 推荐：直接登录后台 → 账号菜单 → 修改密码
   - 也可用 SQL 重置：
     ```bash
     cd /www/wwwroot/starlight-mod-auth
     .venv/bin/python -c "from app.security import hash_password; print(hash_password('新密码'))"
     MYSQL_PWD='<库密码>' mysql -h127.0.0.1 -u starlight_mod starlight_mod \
       -e "UPDATE admin_users SET password_hash='上面输出的哈希' WHERE username='admin';"
     ```

## 部署本轮代码

上传更新后的 `app/` 与 `static/`，然后：

```bash
chown -R www:www /www/wwwroot/starlight-mod-auth/app /www/wwwroot/starlight-mod-auth/static
systemctl restart starlight-mod-auth
curl -s https://auth.xgmfyx.cn/health
```

`.env` 变更后同样需要 `systemctl restart`。

菜单玩家接口 `/v1/auth/*` **不受**管理白名单影响。
