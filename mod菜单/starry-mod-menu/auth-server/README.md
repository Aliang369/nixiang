# 星光魔法 · 网络验证服务

Ubuntu + Docker 部署的卡密 / 公告 / 功能门控 / Toast / 看板后台。

## 快速启动

```bash
cd auth-server
cp .env.example .env   # 可改密码与 SECRET_KEY
# 若看不到 .env.example（隐藏文件），用：cp env.example .env
docker compose up -d --build
```

- API / OpenAPI：`http://服务器IP:8000/docs`
- 管理后台：`http://服务器IP:8000/admin/`
- 健康检查：`http://服务器IP:8000/health`

默认管理员：`admin` / `admin123`（请立刻改 `ADMIN_PASSWORD`）。

## 本机无 Docker 调试（推荐 uv）

```bash
cd auth-server
uv venv --python 3.11 .venv
source .venv/bin/activate
uv pip install -r requirements.txt
DATABASE_URL=sqlite:///./starlight.db SECRET_KEY=devtest \
  uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
```

Ubuntu 服务器正式环境仍用 `docker compose up -d --build`。

## 已实现能力

| 模块 | 说明 |
|------|------|
| 卡密 | 时/天/月/年/永久/次数；单游戏/全游戏；单设备；用户自助解绑 1 次；后台改期/封禁/强制解绑 |
| 公告 | 全游戏 / 单游戏 CRUD；`popup_mode`（每次打开 / 本机一次）；可见性 always / 已登录 / 未登录 |
| 功能门控 | 按 `game_id` + `feat_num` 开关；菜单模板入库，后台 CRUD 后同步 |
| Toast | 按 key 远程文案 |
| 站点设置 | 如 `custom_mod_qq`（菜单「定制 MOD」） |
| 看板 | 总在线/卡密/游客（按设备）、分游戏在线；今日/7日/30日/历史活跃；卡密总数/激活/未激活/封禁、游戏数 |
| 客户端 | login / heartbeat / unbind / bootstrap（含 `logged_in`，退出后回游客）；会话心跳续期；离线缓存建议 180 秒 |

## 首个游戏

默认种子游戏：`starry_self`（星光自测 App）。

详细接口见 [docs/API.md](docs/API.md)。  
数据库备份见 [docs/BACKUP.md](docs/BACKUP.md)（`./scripts/backup_db.sh`）。  
授权短票 / 防破解（最简操作）见 [docs/GRANT_AUTH.md](docs/GRANT_AUTH.md)。
