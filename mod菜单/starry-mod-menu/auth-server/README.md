# 星光魔法 · MOD 菜单网络验证

卡密 / 公告 / 功能门控 / Toast / 看板 后台服务。
技术栈：**FastAPI + SQLAlchemy + MySQL**，宝塔面板用 systemd + nginx 部署。

## 生产环境

| 项目 | 值 |
|---|---|
| 站点 | `https://auth.xgmfyx.cn` |
| 管理后台 | `https://auth.xgmfyx.cn/admin/` |
| 接口文档 | 生产默认关闭；需要时在 `.env` 设 `ENABLE_DOCS=true` 再重启 |
| 健康检查 | `https://auth.xgmfyx.cn/health`（根路径 `/` 返回 404，不暴露后台入口） |
| 主机 | Ubuntu 24.04 · 宝塔面板 · nginx 1.30 |
| 代码目录 | `/www/wwwroot/starlight-mod-auth` |
| 进程 | systemd `starlight-mod-auth` → `uvicorn 127.0.0.1:8010`（单进程） |
| 数据库 | MySQL 5.7 · 库 `starlight_mod`（独立库 + 独立账号） |
| 站点配置 | `/www/server/panel/vhost/nginx/starlight-mod-auth.conf` |
| HTTPS | Let's Encrypt（acme.sh 每日自动续期） |

完整部署、隔离边界与运维命令见 **[docs/DEPLOY.md](docs/DEPLOY.md)**。

## 本地开发

```bash
cd auth-server
python3 -m venv .venv && . .venv/bin/activate
pip install -r requirements.txt pymysql
DATABASE_URL=sqlite:///./starlight.db SECRET_KEY=devtest \
  uvicorn app.main:app --host 127.0.0.1 --port 8010 --reload
```

本地用 SQLite 即可跑通（`ensure_schema()` 会自动补齐列）。
生产环境必须是 **MySQL + 单进程**：多 worker 并发执行 `create_all` 会互相撞表。

## 已实现能力

| 模块 | 说明 |
|------|------|
| 卡密 | 时/天/月/年/永久/次数；单游戏/全游戏；单设备；用户自助解绑 1 次；后台改期/封禁/强制解绑 |
| 公告 | 全游戏 / 单游戏 CRUD；`popup_mode`（每次打开 / 本机一次）；可见性 always / 已登录 / 未登录；按钮：关闭程序（`exit_enabled`/`exit_label`）/ 跳转（`action_url`/`action_label`）/ 关闭进游戏（`close_label`），文案与开关全部后台可配，不配=旧行为 |
| 功能门控 | 按 `game_id` + `feat_num` 开关；菜单模板入库，后台 CRUD 后同步。**掩码只认后台登记位**：未登记/停用一律 0，后台没配功能的游戏功能不可用 |
| 关卡密默认 | 新建游戏默认 `card_auth_enabled=false`（隐藏账号 Tab，免登录）；未在后台注册的 `game_id` 不发功能、不签票，客户端直接不可用 |
| Toast | 按 key 远程文案 |
| 站点设置 | 如 `custom_mod_qq`（菜单「定制 MOD」） |
| 看板 | 总在线/卡密/游客（按设备、互斥）、分游戏在线；今日/7日/30日/历史活跃；卡密总数/激活/未激活/封禁、游戏数 |
| 游客统计 | **需卡游戏的游客也落库**（identity=guest，不写卡关联）；在线心跳 `POST /v1/presence` 5 分钟一次；在线窗口 360 秒；口径正文 [docs/STATS_DEFINITION.md](docs/STATS_DEFINITION.md) |
| 设备日志 | 按设备追溯；新增 `guest_seen`（游客出现）事件与「身份（卡密/游客）」列与筛选 |
| 可疑丢弃 | 非法设备号 / 证书不符 / 版本过低 / IP 突发 → 不落库、不记明细，只在本机计数（`GET /v1/admin/dropped-stats`，侧栏无入口） |
| 客户端 | login / heartbeat / unbind / bootstrap（含 `logged_in`，退出后回游客）；会话心跳续期；离线缓存建议 180 秒 |

## 内置菜单模板

`app/menu_catalog.py` 只保留 **`starry_self`（星光自测）** 一个内置模板，用于验证菜单控件全集。

正式游戏的模板全部在管理端「菜单模板」页新建/编辑，无需改代码、无需重启。
服务启动时 `seed_builtin_templates()` 只会补写数据库里缺失的模板，不会覆盖后台改过的内容。

## 目录结构

```
auth-server/
├── app/                 # FastAPI 应用
│   ├── main.py          # 启动、建表补列、种子数据、静态挂载
│   ├── config.py        # 环境变量（读 .env）
│   ├── models.py        # SQLAlchemy 模型
│   ├── schemas.py       # 请求/响应模型
│   ├── menu_catalog.py  # 内置模板种子（仅 starry_self）
│   ├── routers/         # admin / client
│   └── services/        # 授权票、功能同步、在线/身份（presence）、可信判定（trust）、丢弃计数（dropstats）
├── static/admin/        # 管理后台前端（单页，纯静态）
├── docs/                # 部署、接口、备份、安全、授权票文档
├── scripts/             # 数据库备份、菜单导出等运维脚本
└── requirements.txt
```

> `_legacy_docker/` 是历史 Docker 部署归档（Dockerfile / compose / 自签证书方案），
> **不参与当前部署**，仅供追溯。

## 文档

- [docs/DEPLOY.md](docs/DEPLOY.md) —— 宝塔部署、隔离边界、运维命令
- [docs/API.md](docs/API.md) —— 接口清单
- [docs/MENU_CATALOG.md](docs/MENU_CATALOG.md) —— 菜单模板规范
- [docs/GRANT_AUTH.md](docs/GRANT_AUTH.md) —— 授权短票 / 防破解
- [docs/BACKUP.md](docs/BACKUP.md) —— 数据库备份与恢复
- [docs/SECURITY.md](docs/SECURITY.md) —— 安全加固
